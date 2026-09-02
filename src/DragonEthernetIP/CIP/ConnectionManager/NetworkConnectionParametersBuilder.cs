using DragonEthernetIP.EIP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DragonEthernetIP.CIP.ConnectionManager
{
    internal class NetworkConnectionParametersBuilder
    {
        private uint _value;
        private bool _lfo;
        public NetworkConnectionParametersBuilder(uint val = 0, bool lfo = false)
        {
            this._value = val;
            this._lfo = lfo;
        }

        public void SetRedundantOwner(RedundantOwner val)
        {
            if (_lfo)
            {
                _value |= ((uint)val << 31);
            }
            else
            {
                _value |= ((uint)val << 15);
            }
        }

        public void SetConnectionType(ConnectionType val)
        {
            if (_lfo)
            {
                _value |= ((uint)val << 29);
            }
            else
            {
                _value |= ((uint)val << 13);
            }
        }

        public void SetPriority(Priority val)
        {
            if (_lfo)
            {
                _value |= ((uint)val << 26);
            }
            else
            {
                _value |= ((uint)val << 10);
            }
        }

        public void SetType(NetworkType val)
        {
            if (_lfo)
            {
                _value |= ((uint)val << 25);
            }
            else
            {
                _value |= ((uint)val << 9);
            }
        }

        public void SetConnectionSize(ushort val)
        {
            uint mask = 0x000001FF;
            if (_lfo)
            {
                mask = 0x0000FFFF;
            }
            _value |= val & mask;
        }

        public RedundantOwner getRedundantOwner()
        {
            if (_lfo)
            {
                return (RedundantOwner)((_value & (1 << 31)) >> 31);
            }
            else
            {
                return (RedundantOwner)((_value & (1 << 15)) >> 15);
            }
        }

        public ConnectionType GetConnectionType()
        {
            if (_lfo)
            {
                return (ConnectionType)((_value & (3 << 29)) >> 29);
            }
            else
            {
                return (ConnectionType)((_value & (3 << 13)) >> 13);
            }
        }

        public Priority GetPriority()
        {
            if (_lfo)
            {
                return (Priority)((_value & (3 << 26)) >> 26);
            }
            else
            {
                return (Priority)((_value & (3 << 10)) >> 10);
            }
        }

        public NetworkType GetNetworkType()
        {
            if (_lfo)
            {
                return (NetworkType)((_value & (1 << 25)) >> 25);
            }
            else
            {
                return (NetworkType)((_value & (1 << 9)) >> 9);
            }
        }

        public ushort GetConnectionSize()
        {
            uint mask = 0x000001FF;
            if (_lfo)
            {
                mask = 0x0000FFFF;
            }
            return (ushort)(_value & mask);
        }
    }
}
