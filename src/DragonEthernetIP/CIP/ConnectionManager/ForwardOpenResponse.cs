using DragonEthernetIP.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DragonEthernetIP.CIP.ConnectionManager
{
    internal class ForwardOpenResponse
    {
        private uint _o2tNetworkConnectionId;
        private uint _t2oNetworkConnectionId;
        private ushort _connectionSerialNumber;
        private ushort _originatorVendorId;
        private uint _originatorSerialNumber;
        private uint _o2tAPI;
        private uint _t2oAPI;
        private byte _applicationReplaySize;
        private List<byte> _applicationReplay;
        public ForwardOpenResponse()
        {
            this._o2tNetworkConnectionId = 0;
            this._t2oNetworkConnectionId = 0;
            this._connectionSerialNumber = 0;
            this._originatorVendorId = 0;
            this._originatorSerialNumber = 0;
            this._o2tAPI = 0;
            this._t2oAPI = 0;
            this._applicationReplaySize = 0;
            this._applicationReplay = new List<byte>();
        }

        public bool Expand(List<byte> data) 
        {
		    DragonBuffer buffer = new DragonBuffer(data);
            byte reserved = 0;
            this._o2tNetworkConnectionId = buffer.ReadUint();
            this._t2oNetworkConnectionId = buffer.ReadUint();
            this._connectionSerialNumber = buffer.ReadUshort();
            this._originatorVendorId = buffer.ReadUshort();
            this._originatorSerialNumber = buffer.ReadUint();
            this._o2tAPI = buffer.ReadUint();
            this._t2oAPI = buffer.ReadUint();
            this._applicationReplaySize = buffer.ReadByte();
            reserved = buffer.ReadByte();
            this._applicationReplay = buffer.ReadByteList(this._applicationReplaySize * 2);
		    return buffer.isValid();
	    }

        public uint GetO2TNetworkConnectionId() 
        {
		    return _o2tNetworkConnectionId;
	    }

        public uint GetT2ONetworkConnectionId() 
        {
		    return _t2oNetworkConnectionId;
	    }

        public ushort GetConnectionSerialNumber() 
        {
		    return _connectionSerialNumber;
	    }

        public ushort GetOriginatorVendorId() 
        {
		    return _originatorVendorId;
	    }

        public uint GetOriginatorSerialNumber() 
        {
		    return _originatorSerialNumber;
	    }

        public uint GetO2TApi()
        {
		    return _o2tAPI;
	    }

        public uint GetT2OApi() 
        {
		    return _t2oAPI;
	    }

        public byte GetApplicationReplaySize() 
        {
		    return _applicationReplaySize;
	    }

        public List<byte> GetApplicationReplay() 
        {
		return _applicationReplay;
	    }
    }
}
