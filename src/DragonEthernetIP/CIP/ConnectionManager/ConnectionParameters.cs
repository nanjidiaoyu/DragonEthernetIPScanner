using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DragonEthernetIP.CIP.ConnectionManager
{
    public class ConnectionParameters
    {
        public byte PriorityTimeTick { get; set; } = 0;
        public byte TimeoutTicks { get; set; } = 0;
        public uint O2TNetworkConnectionId { get; set; } = 0;
        public uint T2ONetworkConnectionId { get; set; } = 0;
        public ushort ConnectionSerialNumber { get; set; } = 0;
        public ushort OriginatorVendorId { get; set; } = 0;
        public uint OriginatorSerialNumber { get; set; } = 0;
        public byte ConnectionTimeoutMultiplier { get; set; } = 0;
        public uint O2TRPI { get; set; } = 0;
        public uint O2TNetworkConnectionParams { get; set; } = 0;
        public uint T2ORPI { get; set; } = 0;
        public uint T2ONetworkConnectionParams { get; set; } = 0;
        public byte TransportTypeTrigger { get; set; } = 0;
        public byte ConnectionPathSize { get; set; } = 0;
        public byte O2TRealTimeFormat { get; set; } = 0;
        public byte T2ORealTimeFormat { get; set; } = 0;
        public List<byte> ConnectionPath { get; set; } = new List<byte>();
    }
}
