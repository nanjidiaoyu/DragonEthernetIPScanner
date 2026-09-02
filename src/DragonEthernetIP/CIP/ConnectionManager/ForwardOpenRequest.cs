using DragonEthernetIP.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DragonEthernetIP.CIP.ConnectionManager
{
    internal class ForwardOpenRequest
    {
        private ConnectionParameters _connectionParameters = null;
        public ForwardOpenRequest(ConnectionParameters connectionParameters)
        {
            this._connectionParameters = connectionParameters;
        }

        public List<byte> Pack() 
		{
			ushort size = 36;
			DragonBuffer buffer = new DragonBuffer(size);
			byte reserved = 0;
			ushort o2tNetworkConnectionParams = (ushort)_connectionParameters.O2TNetworkConnectionParams;
			ushort t2oNetworkConnectionParams = (ushort)_connectionParameters.T2ONetworkConnectionParams;
			buffer.Write(_connectionParameters.PriorityTimeTick);
			buffer.Write(_connectionParameters.TimeoutTicks);
			buffer.Write(_connectionParameters.O2TNetworkConnectionId);
			buffer.Write(_connectionParameters.T2ONetworkConnectionId);
			buffer.Write(_connectionParameters.ConnectionSerialNumber);
			buffer.Write(_connectionParameters.OriginatorVendorId);
            buffer.Write(_connectionParameters.OriginatorSerialNumber);
            buffer.Write(_connectionParameters.ConnectionTimeoutMultiplier);
			buffer.Write(reserved);
            buffer.Write(reserved);
            buffer.Write(reserved);
            buffer.Write(_connectionParameters.O2TRPI);
            buffer.Write(o2tNetworkConnectionParams);
            buffer.Write(_connectionParameters.T2ORPI);
            buffer.Write(t2oNetworkConnectionParams);
            buffer.Write(_connectionParameters.TransportTypeTrigger);
            buffer.Write(_connectionParameters.ConnectionPathSize);
            buffer.Write(_connectionParameters.ConnectionPath);
		    return buffer.Data();
	    }
    }
}
