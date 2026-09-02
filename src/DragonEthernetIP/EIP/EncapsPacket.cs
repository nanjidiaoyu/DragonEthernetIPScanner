using DragonEthernetIP.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DragonEthernetIP.EIP
{
    public class EncapsPacket
    {
        public const ushort HEADER_SIZE = 24;
        private EncapsCommands _command;
        private ushort _length;
        private uint _sessionHandle;
        EncapsStatusCodes _statusCode;
        private List<byte> _context;
        private uint _options;
        private List<byte> _data;

        public static ushort GetLengthFromHeader(List<byte> data) 
        {
            var lengthVector = data.GetRange(2,4);
            DragonBuffer dragonBuffer = new DragonBuffer(lengthVector);
            ushort len = dragonBuffer.ReadUshort();
		    return len;
	    }

        public EncapsPacket()
        {
            this._command = EncapsCommands.NOP;
            this._length = 0;
            this._sessionHandle = 0;
            this._statusCode = EncapsStatusCodes.SUCCESS;
            this._context = new List<byte>() { 0,0,0,0,0,0,0,0};
            this._options = 0;
            this._data = new List<byte>();
        }
        public bool Expand(List<byte> data) 
        {
		    if (data.Count < HEADER_SIZE) 
            {
			    return false;
		    }
            DragonBuffer buffer = new DragonBuffer(data);
            this._command = (EncapsCommands)buffer.ReadUshort();
            this._length = buffer.ReadUshort();
            this._sessionHandle = buffer.ReadUint();
            this._statusCode = (EncapsStatusCodes)buffer.ReadUint();
            this._context = buffer.ReadByteList(8);
            this._options = buffer.ReadUint();
            var dataSize = data.Count - HEADER_SIZE;
            if (dataSize != _length) 
            {
			    return false;
		    }
            this._data = buffer.ReadByteList(this._length);
		    return true;
	    }

        public List<byte> Pack()
        {
            DragonBuffer buffer = new DragonBuffer();
            buffer.Write((ushort)this._command);
            buffer.Write(_length);
            buffer.Write(this._sessionHandle);
            buffer.Write((uint)this._statusCode);
            buffer.Write(this._context);
            buffer.Write(this._options);
            buffer.Write(this._data);
		    return buffer.Data();
	    }

        public EncapsCommands GetCommand()
        {
		    return _command;
	    }

        public void SetCommand(EncapsCommands command)
        {
            _command = command;
        }

        public ushort GetLength() 
        {
		    return _length;
	    }

        public uint GetSessionHandle() 
        {
		    return _sessionHandle;
	    }

        public void SetSessionHandle(uint sessionHandle)
        {
            _sessionHandle = sessionHandle;
        }

        public EncapsStatusCodes GetStatusCode() 
        {
		    return _statusCode;
	    }

        public void SetStatusCode(EncapsStatusCodes statusCode)
        {
            _statusCode = statusCode;
        }

        public List<byte> GetData() 
        {
		    return _data;
	    }

        public void SetData(List<byte> data) 
        {
		    _data = data;
		    _length = (ushort)data.Count;
	    }
    }
}
