using DragonEthernetIP.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DragonEthernetIP.CIP
{
    internal class MessageRouterRequest
    {
        private byte _serviceCode;
        private EPath _ePath;
        private List<byte> _data;
        private bool _use_8_bit_path_segments;
        public MessageRouterRequest(byte serviceCode, EPath ePath, List<byte> data, bool use_8_bit_path_segments)
        {
            this._serviceCode = serviceCode;
            this._ePath = ePath;
            this._data = data;
            this._use_8_bit_path_segments = use_8_bit_path_segments;
        }

        public List<byte> Pack()
        {
            DragonBuffer dragonBuffer = new DragonBuffer();
            dragonBuffer.Write(this._serviceCode);
            dragonBuffer.Write(_ePath.GetSizeInWords(_use_8_bit_path_segments));
            dragonBuffer.Write(_ePath.PackPaddedPath(_use_8_bit_path_segments));
            if (_data != null)
            {
                dragonBuffer.Write(this._data);
            }
            return dragonBuffer.Data();
        }
    }
}
