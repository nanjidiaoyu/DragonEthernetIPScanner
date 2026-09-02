using DragonEthernetIP.EIP;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace DragonEthernetIP
{
    public class SessionInfo
    {
        public string Host { get; private set; }
        public int Port { get; private set; }
        public int TimeOut { get; private set; }
        public int SerialNumberCounter { get; set; }
        public int O2TIdCounter { get; set; }
        public int T2OIdCounter { get; set; }
        private uint sessionHandle = 0;
        private bool _isConnected = false;
        private NetworkStream networkStream = null;
        private readonly ILogger _logger;
        public uint GetSessionHandle()
        {
            return sessionHandle;
        }

        private TcpClient tcpClient;
        public SessionInfo(string host,int port, ILogger logger = null,int timeout = 2000)
        {
            this.Host = host;
            this.Port = port;
            this.TimeOut = timeout;
            this._logger = logger??NullLogger.Instance;
        }
        public bool IsConnected()
        {
            return _isConnected;
        }

        public async Task<bool> RegisterSessionAsync()
        {
            try
            {
                this.tcpClient = new TcpClient();
                this.tcpClient.ReceiveTimeout = this.TimeOut;
                this.tcpClient.SendTimeout = this.TimeOut;
                await this.tcpClient.ConnectAsync(this.Host, this.Port);
                EncapsPacket packet = EncapsPacketFactory.CreateRegisterSessionPacket();
                this.networkStream = this.tcpClient.GetStream();
                var writeData = packet.Pack().ToArray();
                await networkStream.WriteAsync(writeData, 0, writeData.Length);
                var readHeaderData = new byte[EncapsPacket.HEADER_SIZE];
                int bytes = await networkStream.ReadAsync(readHeaderData, 0, EncapsPacket.HEADER_SIZE);
                if(bytes > 0)
                {
                    var length = EncapsPacket.GetLengthFromHeader(readHeaderData.ToList());
                    var readData = new byte[length];
                    bytes = await networkStream.ReadAsync(readData, 0, length);
                    if (bytes > 0)
                    {
                        List<byte> responseData = new List<byte>();
                        responseData.AddRange(readHeaderData);
                        responseData.AddRange(readData);
                        EncapsPacket recvPacket = new EncapsPacket();
                        if (!recvPacket.Expand(responseData))
                        {
                            this._logger.LogError($"Malformed encaps packet from Host:{this.Host},Port:{this.Port}");
                            return false;
                        }
                        if (recvPacket.GetStatusCode() != EncapsStatusCodes.SUCCESS)
                        {
                            this._logger.LogError($"Bad encaps packet code ={recvPacket.GetStatusCode().ToString()}");
                            return false;
                        }
                        if (this.sessionHandle != 0 && recvPacket.GetSessionHandle() != this.sessionHandle)
                        {
                            this._logger.LogError("Wrong session handle received");
                            return false;
                        }
                        this.sessionHandle = recvPacket.GetSessionHandle();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError($"RegisterSession Exception:{ex.ToString()}");
            }
            return false;
        }

        public async Task UnRegisterSessionAsync()
        {
            try
            {
                EncapsPacket packet = EncapsPacketFactory.CreateUnRegisterSessionPacket(this.sessionHandle);
                var writeData = packet.Pack().ToArray();
                await networkStream.WriteAsync(writeData, 0, writeData.Length);
                this._logger.LogError($"Unregistered session {this.sessionHandle}");
            }
            catch (Exception ex)
            {
                this._logger.LogError($"UnRegisterSession Exception:{ex.Message}");
            }
            networkStream.Close();
            networkStream = null;
            tcpClient.Close();
            tcpClient = null;
            sessionHandle = 0;
        }


        public async Task<EncapsPacket> SendAndReceive(EncapsPacket packet) 
        {
		    if (tcpClient == null) 
            {
                this._logger.LogError("EIP session is not connected");
                return null ;
		    }
            try
            {
                var writeData = packet.Pack().ToArray();
                await this.networkStream.WriteAsync(writeData, 0, writeData.Length);
                var readHeaderData = new byte[EncapsPacket.HEADER_SIZE];
                var headerLength = await this.networkStream.ReadAsync(readHeaderData, 0, EncapsPacket.HEADER_SIZE);
                if (headerLength > 0)
                {
                    var bodyLength = EncapsPacket.GetLengthFromHeader(readHeaderData.ToList());
                    if(bodyLength == 0)
                    {
                        EncapsPacket recvPacket = new EncapsPacket();
                        if (!recvPacket.Expand(readHeaderData.ToList()))
                        {
                            this._logger.LogError("Malformed encaps packet");
                        }
                        if (recvPacket.GetStatusCode() != EncapsStatusCodes.SUCCESS)
                        {
                            this._logger.LogError($"Bad encaps packet code ={recvPacket.GetStatusCode()}");
                        }
                        return null;
                    }
                    var readBodyData = new byte[bodyLength];
                    var readBodyLength = await this.networkStream.ReadAsync(readBodyData, 0, bodyLength);
                    if (readBodyLength > 0)
                    {
                        List<byte> responseData = new List<byte>();
                        responseData.AddRange(readHeaderData);
                        responseData.AddRange(readBodyData);
                        EncapsPacket recvPacket = new EncapsPacket();
                        if (!recvPacket.Expand(responseData))
                        {
                            this._logger.LogError("Malformed encaps packet");
                            return null;
                        }
                        if (recvPacket.GetStatusCode() != EncapsStatusCodes.SUCCESS)
                        {
                            this._logger.LogError($"Bad encaps packet code ={recvPacket.GetStatusCode()}");
                            return null;
                        }
                        if (this.sessionHandle != 0 && recvPacket.GetSessionHandle() != this.sessionHandle)
                        {
                            this._logger.LogError("Wrong session handle received");
                            return null;
                        }
                        return recvPacket;
                    }
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError($"SendAndReceive Exception:{ex.Message}");
            }
            return null;
        }

    }
}
