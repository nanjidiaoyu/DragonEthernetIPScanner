using DragonEthernetIP.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DragonEthernetIP.EIP
{
    internal class CommonPacket
    {
        private List<CommonPacketItem> _items = new List<CommonPacketItem>();
        public void Add(CommonPacketItem commonPacketItem)
        {
            this._items.Add(commonPacketItem);
        }

        public List<byte> Pack()
        {
            DragonBuffer buffer = new DragonBuffer();
            buffer.Write((ushort)this._items.Count);
            foreach (var item in this._items)
            {
                buffer.Write(item.Pack());
            }
            return buffer.Data();
        }

        public bool Expand(List<byte> data)
        {
            _items.Clear();
            DragonBuffer buffer = new DragonBuffer(data);
            ushort count = buffer.ReadUshort();
            for (int i = 0; i < count && !buffer.empty(); ++i)
            {
                ushort typeId = buffer.ReadUshort();
                ushort length = buffer.ReadUshort();
                List<byte> itemData = buffer.ReadByteList(length);
                if (!buffer.isValid())
                {
                    return false;
                }
                _items.Add(new CommonPacketItem((CommonPacketItemIds)typeId, itemData));
            }
                return true;
        }
        public List<CommonPacketItem> GetItems()
        {
            return _items;
        }
    }
}
