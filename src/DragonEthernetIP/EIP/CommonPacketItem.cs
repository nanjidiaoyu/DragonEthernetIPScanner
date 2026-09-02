using DragonEthernetIP.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DragonEthernetIP.EIP
{
    public class CommonPacketItem
    {
        private CommonPacketItemIds _typeId;
        private ushort _length;
        private List<byte> _data;
        public CommonPacketItem()
        {
            this._typeId = CommonPacketItemIds.NULL_ADDR;
        }
        public CommonPacketItem(CommonPacketItemIds typeId, List<byte> data)
        {
            this._typeId = typeId;
            this._length = (ushort)data.Count;
            this._data = data;
        }

        public List<byte> Pack()
        {
            DragonBuffer dragonBuffer = new DragonBuffer();
            dragonBuffer.Write((ushort)this._typeId);
            dragonBuffer.Write(this._length);
            if (this._length > 0)
            {
                dragonBuffer.Write(this._data);
            }
            return dragonBuffer.Data();
        }

        public CommonPacketItemIds GetTypeId()
        {
            return this._typeId;
        }

        public ushort GetLength()
        {
            return this._length;
        }
        public List<byte> GetData()
        {
            return this._data;
        }
    }
}
