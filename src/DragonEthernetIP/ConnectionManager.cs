using DragonEthernetIP.CIP;
using DragonEthernetIP.CIP.ConnectionManager;
using DragonEthernetIP.EIP;
using DragonEthernetIP.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DragonEthernetIP
{
    public class ConnectionManager
    {
        private ConcurrentDictionary<string, ConcurrentDictionary<uint, IOConnection>> iOConnectionDics = new ConcurrentDictionary<string, ConcurrentDictionary<uint, IOConnection>>();
        private List<uint> multicastAddressList = new List<uint>();
        private Dictionary<string, Task> taskDic = new Dictionary<string, Task>();
        private ConcurrentDictionary<string, ConcurrentBag<IOConnection>> reTryConcurrentBag = new();
        private MessageRouter _messageRouter;
        private CancellationTokenSource _cancellationTokenSource = null;
        private string _localIpAddress = string.Empty;
        private CancellationToken cancellationToken = default;
        private Task reconnectTask = null;
        private readonly ILogger _logger;
        private UdpClient scannerUdpClient = null;
        private IPEndPoint scannerEndPointReceive = null;
        public ConnectionManager(string localIpAddress, ILogger logger = null)
        {
            _localIpAddress = localIpAddress;
            _cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = _cancellationTokenSource.Token;
            reconnectTask = Task.Factory.StartNew(LoopReconnect, TaskCreationOptions.LongRunning);
            _logger = logger??NullLogger.Instance;
            _messageRouter = new MessageRouter(_logger);
        }
        public async Task<bool> ForwardOpen(SessionInfo sessionInfo, ConnectionParameters connectionParameters, bool isLarge)
        {
            NetworkConnectionParametersBuilder o2tNCP = new NetworkConnectionParametersBuilder(connectionParameters.O2TNetworkConnectionParams, isLarge);
            NetworkConnectionParametersBuilder t2oNCP = new NetworkConnectionParametersBuilder(connectionParameters.T2ONetworkConnectionParams, isLarge);
            connectionParameters.ConnectionSerialNumber = (ushort)++sessionInfo.SerialNumberCounter;
            if (o2tNCP.GetConnectionType() == ConnectionType.MULTICAST)
            {
                connectionParameters.O2TNetworkConnectionId = (uint)++sessionInfo.O2TIdCounter;
            }
            if (t2oNCP.GetConnectionType() == ConnectionType.P2P)
            {
                connectionParameters.T2ONetworkConnectionId = (uint)++sessionInfo.T2OIdCounter;
            }
            var o2tSize = o2tNCP.GetConnectionSize();
            var t2oSize = t2oNCP.GetConnectionSize();
            connectionParameters.ConnectionPathSize = (byte)((connectionParameters.ConnectionPath.Count / 2)
            + (connectionParameters.ConnectionPath.Count % 2));
            if ((connectionParameters.TransportTypeTrigger & (ushort)NetworkConnectionParams.CLASS1) > 0
            || (connectionParameters.TransportTypeTrigger & (ushort)NetworkConnectionParams.CLASS3) > 0)
            {
                connectionParameters.O2TNetworkConnectionParams += 2;
                connectionParameters.T2ONetworkConnectionParams += 2;
            }
            if (connectionParameters.O2TRealTimeFormat != 0)
            {
                connectionParameters.O2TNetworkConnectionParams += 4;
            }
            if (connectionParameters.T2ORealTimeFormat != 0)
            {
                connectionParameters.T2ONetworkConnectionParams += 4;
            }
            MessageRouterResponse messageRouterResponse = null;
            if (isLarge)
            {
                LargeForwardOpenRequest request = new LargeForwardOpenRequest(connectionParameters);
                messageRouterResponse = await _messageRouter.SendRequest(sessionInfo, (byte)ConnectionManagerServiceCodes.LARGE_FORWARD_OPEN,
                    new EPath(6, 1), request.Pack(), null);
            }
            else
            {
                ForwardOpenRequest request = new ForwardOpenRequest(connectionParameters);
                messageRouterResponse = await _messageRouter.SendRequest(sessionInfo, (byte)ConnectionManagerServiceCodes.FORWARD_OPEN,
                    new EPath(6, 1), request.Pack(), null);
            }
            if (messageRouterResponse == null)
            {
                //ForwardOpen失败
                return false;
            }
            if (messageRouterResponse.GetGeneralStatusCode() != GeneralStatusCodes.SUCCESS)
            {
                this._logger.LogError($"Message Router error=0x{messageRouterResponse.GetGeneralStatusCode()} additional statuses {string.Join(' ', messageRouterResponse.GetAdditionalStatus())}");
                return false;
            }
            ForwardOpenResponse forwardOpenResponse = new ForwardOpenResponse();
            if (!forwardOpenResponse.Expand(messageRouterResponse.GetData()))
            {
                this._logger.LogError("Malformed ForwardOpen response");
            }
            this._logger.LogInformation($"Open IO connection O2T_ID={forwardOpenResponse.GetO2TNetworkConnectionId()} T2O_ID={forwardOpenResponse.GetT2ONetworkConnectionId} SerialNumber {forwardOpenResponse.GetConnectionSerialNumber()}");
            IOConnection ioConnection = new IOConnection(connectionParameters, sessionInfo,this._logger);
            ioConnection.IsLarge = isLarge;
            ioConnection.O2TNetworkConnectionId = forwardOpenResponse.GetO2TNetworkConnectionId();
            ioConnection.T2ONetworkConnectionId = forwardOpenResponse.GetT2ONetworkConnectionId();
            ioConnection.O2TAPI = forwardOpenResponse.GetO2TApi();
            ioConnection.T2OAPI = forwardOpenResponse.GetT2OApi();
            ioConnection.ConnectionTimeoutMultiplier = (byte)(4 << connectionParameters.ConnectionTimeoutMultiplier);
            ioConnection.SerialNumber = forwardOpenResponse.GetConnectionSerialNumber();
            ioConnection.TransportTypeTrigger = connectionParameters.TransportTypeTrigger;
            ioConnection.O2TRealTimeFormat = connectionParameters.O2TRealTimeFormat;
            ioConnection.T2ORealTimeFormat = connectionParameters.T2ORealTimeFormat;
            ioConnection.ConnectionPath = connectionParameters.ConnectionPath;
            ioConnection.OriginatorVendorId = connectionParameters.OriginatorVendorId;
            ioConnection.OriginatorSerialNumber = connectionParameters.OriginatorSerialNumber;
            ioConnection.O2TDataSize = o2tSize;
            ioConnection.T2ODataSize = t2oSize;
            ioConnection.O2TFixedSize = (o2tNCP.GetNetworkType() == NetworkType.FIXED);
            ioConnection.T2OFixedSize = (t2oNCP.GetNetworkType() == NetworkType.FIXED);
            ioConnection.IsClose = false;
            List<CommonPacketItem> additionalItems = messageRouterResponse.GetAdditionalPacketItems();
            var o2tSockAddrInfo = additionalItems.FirstOrDefault(item => item.GetTypeId() == CommonPacketItemIds.O2T_SOCKADDR_INFO);
            var t2oSockAddrInfo = additionalItems.FirstOrDefault(item => item.GetTypeId() == CommonPacketItemIds.T2O_SOCKADDR_INFO);
            if (o2tSockAddrInfo != null)
            {
                DragonBuffer sockAddrBuffer = new DragonBuffer(o2tSockAddrInfo.GetData());
                DragonEndPoint endPoint = sockAddrBuffer.ReadEndPoint();
                ioConnection.TargetEndPoint = endPoint;
            }
            uint multicastAddress = 0;
            if (t2oSockAddrInfo != null)
            {
                DragonBuffer sockAddrBuffer = new DragonBuffer(t2oSockAddrInfo.GetData());
                DragonEndPoint endPoint = sockAddrBuffer.ReadEndPoint();
                ioConnection.SourcetEndPoint = endPoint;
                if (t2oNCP.GetConnectionType() == ConnectionType.MULTICAST)
                {
                    multicastAddress = endPoint.SIN_Address;
                }
            }
            if (scannerUdpClient == null)
            {
                var localIPaddr = System.Net.IPAddress.Parse(this._localIpAddress);
                scannerEndPointReceive = new System.Net.IPEndPoint(localIPaddr, DragonEndPoint.EIP_DEFAULT_IMPLICIT_PORT);//建议绑定到具体IP地址，不要使用Any
                scannerUdpClient = new UdpClient(scannerEndPointReceive);
            }
            if (!multicastAddressList.Contains(multicastAddress))
            {
                if (multicastAddress != 0)
                {
                    System.Net.IPAddress multicast = new System.Net.IPAddress(multicastAddress);
                    scannerUdpClient.JoinMulticastGroup(multicast);
                }
                multicastAddressList.Add(multicastAddress);
                UdpState s = new UdpState();
                s.e = scannerEndPointReceive;
                s.u = scannerUdpClient;
                s.sessionIP = sessionInfo.Host;
                var asyncResult = scannerUdpClient.BeginReceive(new AsyncCallback(ReceiveCallbackClass1), s);
            }
            if (this.iOConnectionDics.TryGetValue(sessionInfo.Host, out var localDic))
            {
                localDic[ioConnection.T2ONetworkConnectionId] = ioConnection;
            }
            else
            {
                var newLocalDic = new ConcurrentDictionary<uint, IOConnection>();
                this.iOConnectionDics.TryAdd(sessionInfo.Host, newLocalDic);
                newLocalDic[ioConnection.T2ONetworkConnectionId] = ioConnection;
                Task task = Task.Factory.StartNew(LoopSendData, sessionInfo.Host, TaskCreationOptions.LongRunning);
                taskDic.Add(sessionInfo.Host, task);
            }
            return true;
        }

        public async Task<bool> ForwardClose(IOConnection iOConnection)
        {
            if (!iOConnection.IsClose)
            {
                iOConnection.IsClose = true;
            }
            ForwardCloseRequest request = new ForwardCloseRequest();
            request.SetConnectionPath(iOConnection.ConnectionPath);
            request.SetConnectionSerialNumber((ushort)iOConnection.SerialNumber);
            request.SetOriginatorVendorId(iOConnection.OriginatorVendorId);
            request.SetOriginatorSerialNumber(iOConnection.OriginatorSerialNumber);
            MessageRouterResponse messageRouterResponse = null;
            messageRouterResponse = await this._messageRouter.SendRequest(iOConnection.SessionInfo, (byte)ConnectionManagerServiceCodes.FORWARD_CLOSE,
                    new EPath(6, 1), request.Pack().ToList());
            if (messageRouterResponse != null)
            {
                this._logger.LogError($"ForwardClose failed for T2O_ID={iOConnection.T2ONetworkConnectionId}.Connection removed locally anyway");
            }
            else
            {
                return false;
            }
            if (iOConnectionDics.TryGetValue(iOConnection.SessionInfo.Host, out var hostDic))
            {
                hostDic.Remove(iOConnection.T2ONetworkConnectionId, out var _);
                this._logger.LogError($"Remove IOConnection,T2ONetworkConnectionId:{iOConnection.T2ONetworkConnectionId}");
            }
            return true;
        }

        private void ReceiveCallbackClass1(IAsyncResult ar)
        {
            UdpState udpState = ar.AsyncState as UdpState;
            UdpClient u = udpState.u;
            if (cancellationToken.CanBeCanceled)
            {
                u.BeginReceive(new AsyncCallback(ReceiveCallbackClass1), ar.AsyncState);
            }
            System.Net.IPEndPoint e = udpState.e;
            byte[] receiveBytes = u.EndReceive(ar, ref e);
            CommonPacket commonPacket = new CommonPacket();
            if (commonPacket.Expand(receiveBytes.ToList()))
            {
                var items = commonPacket.GetItems();
                if (items.Count >= 2)
                {
                    DragonBuffer buffer = new DragonBuffer(items[0].GetData());
                    uint connectionId = buffer.ReadUint();
                    var bodyData = items[1].GetData();
                    if (this.iOConnectionDics.TryGetValue(udpState.sessionIP, out var localDic))
                    {
                        if (localDic.TryGetValue(connectionId, out var iOConnection))
                        {
                            iOConnection.OnReceivedData(bodyData);
                        }
                    }
                }
                else
                {
                    this._logger.LogError($"T2O packet has {items.Count} items, expected >= 2; dropped");
                }
            }
            else
            {
                this._logger.LogError("Malformed T2O packet; dropped");
            }
        }

        private async Task LoopSendData(object targetIP)
        {
            while (cancellationToken.CanBeCanceled)
            {
                if (iOConnectionDics.TryGetValue(targetIP.ToString(), out var ioConnections))
                {
                    List<uint> removeConnectionKeys = null;
                    foreach (var (key, ioConnection) in ioConnections)
                    {
                        if (!await ioConnection.OnSendData())
                        {
                            if (removeConnectionKeys == null)
                            {
                                removeConnectionKeys = new();
                            }
                            removeConnectionKeys.Add(key);
                        }
                    }
                    if (removeConnectionKeys != null)
                    {
                        foreach (var key in removeConnectionKeys)
                        {
                            if (ioConnections.TryRemove(key, out var connection))
                            {
                                this.ReconnectIOConnection(connection);
                            }
                        }
                    }
                }
                await Task.Delay(20);
            }

        }

        private void ReconnectIOConnection(IOConnection iOConnection)
        {
            if (reTryConcurrentBag.TryGetValue(iOConnection.SessionInfo.Host, out var concurrentBag))
            {
                concurrentBag.Add(iOConnection);
            }
            else
            {
                ConcurrentBag<IOConnection> iOConnections = new ConcurrentBag<IOConnection>() { iOConnection };
                reTryConcurrentBag.TryAdd(iOConnection.SessionInfo.Host, iOConnections);
            }
        }

        private async Task LoopReconnect()
        {
            while (cancellationToken.CanBeCanceled)
            {
                if (!reTryConcurrentBag.IsEmpty)
                {
                    foreach (var key in reTryConcurrentBag.Keys)
                    {
                        if (reTryConcurrentBag.TryGetValue(key, out var concurrentBag))
                        {
                            if (concurrentBag.Count > 0)
                            {
                                int allCount = concurrentBag.Count;
                                List<IOConnection> disconnections = null;
                                for (int i = 0; i < allCount; i++)
                                {
                                    if (concurrentBag.TryTake(out var iOConnection))
                                    {
                                        await this.ForwardClose(iOConnection);
                                        if (await this.ForwardOpen(iOConnection.SessionInfo, iOConnection.ConnectionParameters, iOConnection.IsLarge))
                                        {
                                            continue;
                                        }
                                        if (disconnections == null)
                                        {
                                            disconnections = new List<IOConnection>();
                                        }
                                        disconnections.Add(iOConnection);
                                    }
                                }
                                if(disconnections != null)
                                {
                                    if (iOConnectionDics.TryGetValue(key, out var ioConnections))
                                    {
                                        if (ioConnections.Count == 0 && disconnections.Count == allCount)
                                        {
                                            await disconnections.First().SessionInfo.UnRegisterSessionAsync();
                                            await Task.Delay(200);
                                            await disconnections.First().SessionInfo.RegisterSessionAsync();
                                        }
                                    }
                                    foreach (var item in disconnections)
                                    {
                                        concurrentBag.Add(item);
                                    }
                                }
                                
                            }
                        }
                    }
                }
                await Task.Delay(5000);
            }
        }

        private void StopLoop()
        {
            this._cancellationTokenSource.Cancel();
        }

        class UdpState
        {
            public System.Net.IPEndPoint e { get; set; }
            public UdpClient u { get; set; }
            public string sessionIP { get; set; }
        }
    }
}
