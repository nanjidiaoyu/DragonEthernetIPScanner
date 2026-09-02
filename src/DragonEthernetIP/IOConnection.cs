using DragonEthernetIP.CIP;
using DragonEthernetIP.CIP.ConnectionManager;
using DragonEthernetIP.EIP;
using DragonEthernetIP.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DragonEthernetIP
{
    public class IOConnection
    {
        private UdpClient UdpClient  = null;
        private readonly ILogger _logger;

        private DateTime LastHandleTime = DateTime.Now;
        private System.Net.IPEndPoint EndPointSend = null;
        public bool IsLarge { get; set; }
        public DragonEndPoint TargetEndPoint { get; set; }
        public DragonEndPoint SourcetEndPoint { get; set; }
        public IOConnection(ConnectionParameters connectionParameters, SessionInfo sessionInfo, ILogger logger)
        {
            this.ConnectionParameters = connectionParameters;
            this.SessionInfo = sessionInfo;
            this.UdpClient = new UdpClient();
            this._logger = logger;
        }
        public SessionInfo SessionInfo { get; private set; }
        public ConnectionParameters ConnectionParameters { get; private set; }
        public uint O2TNetworkConnectionId { get; set; }
        public uint T2ONetworkConnectionId { get; set; }
        public uint O2TAPI { get; set; } = 0;
        public uint T2OAPI { get; set; } = 0;
        public uint O2TDataSize { get; set; } = 0;
        public uint T2ODataSize { get; set; } = 0;
        public bool O2TFixedSize { get; set; } = true;
        public bool T2OFixedSize { get; set; } = true;
        public double O2TTimer { get; set; } = 0;
        public uint T2O_timer { get; set; } = 0;
        public byte ConnectionTimeoutMultiplier { get; set; } = 0;
        public double ConnectionTimeoutCount { get; private set; } = 0;
        public uint O2tSequenceNumber { get; set; } = 0;
        public uint T2oSequenceNumber { get; set; } = 0;
        public uint SerialNumber { get; set; } = 0;
        public byte TransportTypeTrigger { get; set; } = 0;
        public byte O2TRealTimeFormat { get; set; } = 0;
        public byte T2ORealTimeFormat { get; set; } = 0;
        public ushort SequenceValueCount { get; set; } = 0;
        public List<byte> ConnectionPath { get; set; } = new List<byte>();
        public ushort OriginatorVendorId { get; set; } = 0;
        public uint OriginatorSerialNumber { get; set; } = 0;
        public byte[] T2OArrayData { get; private set; }
        public byte[] O2TArrayData { private get;  set; }
        public bool IsClose { get; set; } = false;

        public void OnReceivedData(List<byte> data)
        {
            ushort pos = 0;
            if (T2ORealTimeFormat !=0)
            {
                pos += 4;
            }
            if ((TransportTypeTrigger & (byte)NetworkConnectionParams.CLASS1) > 0
                || (TransportTypeTrigger & (byte)NetworkConnectionParams.CLASS3) > 0)
            {
                pos += 2;
            }
            ushort payloadSize = (ushort)(data.Count - pos);
            if (T2OFixedSize && payloadSize != T2ODataSize)
            {
                this._logger.LogError($"Connection T2O_ID={T2ONetworkConnectionId} has fixed size {T2ODataSize} bytes but {payloadSize} bytes were received;");
                return;
            }
            if(T2OArrayData == null)
            {
                T2OArrayData = new byte[T2ODataSize];
            }
            var bytes = data.ToArray();
            Array.Copy(bytes, pos, T2OArrayData, 0, T2ODataSize);
            this._logger.LogTrace($"ReadData:{BitConverter.ToString(T2OArrayData)}");
            this.ConnectionTimeoutCount = 0;
        }

        public async Task<bool> OnSendData()
        {
            DateTime time_point = DateTime.Now;
            var sinceLastHandle = ((time_point - this.LastHandleTime).TotalMilliseconds)*1000;
            this.ConnectionTimeoutCount += sinceLastHandle;
            if(this.ConnectionTimeoutCount > this.ConnectionTimeoutMultiplier * this.T2OAPI || this.IsClose)
            {
                this._logger.LogError($"Connection SeriaNumber={this.SerialNumber} is closed by timeout");
                if (this.IsClose)
                {
                    this.IsClose = true;
                }
                return false;
            }
            this.LastHandleTime = time_point;
            this.O2TTimer += sinceLastHandle;
            if(this.O2TTimer >= this.O2TAPI)
            {
                this.O2TTimer = 0;
                this.O2tSequenceNumber++;
                CommonPacketItemFactory factory = new CommonPacketItemFactory();
                CommonPacket commonPacket = new CommonPacket();
                commonPacket.Add(factory.CreateSequenceAddressItem(this.O2TNetworkConnectionId, this.O2tSequenceNumber));
                DragonBuffer buffer = new DragonBuffer();
                if ((this.TransportTypeTrigger & (byte)NetworkConnectionParams.CLASS1) > 0
                || (this.TransportTypeTrigger & (byte)NetworkConnectionParams.CLASS3) > 0)
                {
                    buffer.Write(++this.SequenceValueCount) ;
                }
                if (this.O2TRealTimeFormat!=0)
                {
                    uint header = 1;
                    buffer.Write(header);
                }
                if(this.O2TArrayData == null)
                {
                    this.O2TArrayData = new byte[this.O2TDataSize];
                }
                buffer.Write(this.O2TArrayData);
                commonPacket.Add(factory.CreateConnectedDataItem(buffer.Data()));
                if(this.EndPointSend == null)
                {
                    if (this.TargetEndPoint != null)
                    {
                        if (this.TargetEndPoint.SIN_Address != 0)
                        {
                            this.EndPointSend = new System.Net.IPEndPoint(new System.Net.IPAddress(this.TargetEndPoint.SIN_Address), this.TargetEndPoint.SIN_port);
                        }
                        else
                        {
                            this.EndPointSend = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(this.SessionInfo.Host), DragonEndPoint.EIP_DEFAULT_IMPLICIT_PORT);
                        }
                    }
                    else
                    {
                        this.EndPointSend = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(this.SessionInfo.Host), DragonEndPoint.EIP_DEFAULT_IMPLICIT_PORT);
                    }
                }
                var sendData = commonPacket.Pack().ToArray();
                try
                {
                    await this.UdpClient.SendAsync(sendData, sendData.Length, this.EndPointSend);
                    this._logger.LogTrace($"SendData:{BitConverter.ToString(this.O2TArrayData)}");
                }
                catch (Exception ex)
                {
                    this._logger.LogError($"SendData Exception:{ex.Message}");
                    return false;
                }

            }
            return true;
        }
        
    }
}
