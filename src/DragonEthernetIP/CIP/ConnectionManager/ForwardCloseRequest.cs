using DragonEthernetIP.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DragonEthernetIP.CIP.ConnectionManager
{
    internal class ForwardCloseRequest
    {
        private ushort _connectionSerialNumber;
        private ushort _originatorVendorID;
        private uint _originatorSerialNumber;
        private List<byte> _connectionPath;

        public void SetConnectionSerialNumber(ushort connectionSerialNumber)
        {
            _connectionSerialNumber = connectionSerialNumber;
        }
        public void SetOriginatorVendorId(ushort originatorVendorId)
        {
            _originatorVendorID = originatorVendorId;
        }
        public void SetOriginatorSerialNumber(uint originatorSerialNumber)
        {
            _originatorSerialNumber = originatorSerialNumber;
        }
        public void SetConnectionPath(List<byte> connectionPath)
        {
            this._connectionPath = connectionPath;
        }

        public byte[] Pack()
        {
            DragonBuffer buffer = new DragonBuffer();
            byte timeTick = 0;
            byte timeOutTicks = 0;
            byte reserved = 0;
            buffer.Write(timeTick);
            buffer.Write(timeOutTicks);
            buffer.Write(_connectionSerialNumber);
            buffer.Write(_originatorVendorID);
            buffer.Write(_originatorSerialNumber);
            buffer.Write((byte)(_connectionPath.Count / 2));
            buffer.Write(reserved);
            buffer.Write(_connectionPath);
            return buffer.Data().ToArray();
        }
    }
}
