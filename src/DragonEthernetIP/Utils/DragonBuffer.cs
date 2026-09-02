using DragonEthernetIP.CIP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DragonEthernetIP.Utils
{
    public class DragonBuffer
    {
        private List<byte> _buffer;
        private int _position;
        private uint _capacity;
        public DragonBuffer(uint capacity = 0)
        {
            _capacity = capacity;
            _buffer = new List<byte>();
        }
        public DragonBuffer(List<byte> data)
        {
            _buffer = data;
        }
        public List<byte> Data()
        { 
            return _buffer; 
        }
		public int Size() 
        { 
            return _buffer.Count;
        }
        public int Pos() 
        { 
            return _position;
        }
		public bool isValid() 
        { 
            return _position <= _buffer.Count;
        }
		public bool empty()  
        { 
            return  _position >= _buffer.Count;
        }
        public void Write(byte val)
        {
            _buffer.Add(val);
        }
        public void Write(byte[] val)
        {
            _buffer.AddRange(val);
        }
        public byte ReadByte()
        {
            return _buffer[_position++];
        }

        public void Write(ushort val)
        {
            _buffer.Add((byte)val);
            _buffer.Add((byte)(val >> 8));
        }

        public ushort ReadUshort()
        {
            ushort val = _buffer[_position++];
            val += (ushort)(_buffer[_position++] << 8);
            return val;
        }

        public void Write(uint val)
        {
            _buffer.Add((byte)val);
            _buffer.Add((byte)(val >> 8));
            _buffer.Add((byte)(val >> 16));
            _buffer.Add((byte)(val >> 24));
        }

        public uint ReadUint()
        {
            uint val = _buffer[_position++];
            val += (uint)(_buffer[_position++] << 8);
            val += (uint)(_buffer[_position++] << 16);
            val += (uint)(_buffer[_position++] << 24);
            return val;
        }

        public void Write(ulong val)
        {
            _buffer.Add((byte)val);
            _buffer.Add((byte)(val >> 8));
            _buffer.Add((byte)(val >> 16));
            _buffer.Add((byte)(val >> 24));
            _buffer.Add((byte)(val >> 32));
            _buffer.Add((byte)(val >> 40));
            _buffer.Add((byte)(val >> 48));
            _buffer.Add((byte)(val >> 56));
        }

        public ulong ReadUlong()
        {
            ulong val = _buffer[_position++];
            val += (ulong)(_buffer[_position++] << 8);
            val += (ulong)(_buffer[_position++] << 16);
            val += (ulong)(_buffer[_position++] << 24);
            val += (ulong)(_buffer[_position++] << 32);
            val += (ulong)(_buffer[_position++] << 40);
            val += (ulong)(_buffer[_position++] << 48);
            val += (ulong)(_buffer[_position++] << 56);
            return val;
        }

        public void Write(List<byte> val)
        {
            _buffer.AddRange(val);
        }

        public List<byte> ReadByteList(int readsize)
        {
            var vals = _buffer.GetRange(_position, readsize);
            _position += readsize;
            return vals;
        }

        public void WriteUshortList(List<ushort> val)
        {
            foreach (var item in val)
            {
                this.Write(item);
            }
        }
        public List<ushort> ReadUshortList(int readsize)
        {
            List<ushort> ushorts = new List<ushort>();
            for (int i = 0; i < readsize; i++)
            {
                ushorts.Add(this.ReadUshort());
            }
            return ushorts;
        }

        public void WriteCipRevision(CipRevision cipRevision)
        {
            this.Write(cipRevision.GetMajorRevision());
            this.Write(cipRevision.GetMinorRevision());
        }

        public CipRevision ReadCipRevision()
        {
            CipRevision cipRevision = new CipRevision(this.ReadByte(), this.ReadByte());
            return cipRevision;
        }

        public void WriteEndPoint(DragonEndPoint endPoint)
        {
            this.Write(endPoint.SIN_family);
            this.Write(endPoint.SIN_port);
            this.Write(endPoint.SIN_Address);
            this.Write(endPoint.SIN_Zero.ToList());
        }

        public DragonEndPoint ReadEndPoint()
        {
            DragonEndPoint endPoint = new DragonEndPoint();
            endPoint.SIN_family = this.ReadUshort();
            endPoint.SIN_port = this.ReadUshort();
            endPoint.SIN_Address = this.ReadUint();
            endPoint.SIN_Zero = this.ReadByteList(8).ToArray();
            return endPoint;
        }

        public string ReadString()
        {
            byte length = this.ReadByte();
            var stringBytes = this.ReadByteList(length);
            return Encoding.ASCII.GetString(stringBytes.ToArray());
        }
    }
}
