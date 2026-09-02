using DragonEthernetIP.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DragonEthernetIP.EIP
{
    internal class CommonPacketItemFactory
    {

        public CommonPacketItem CreateNullAddressItem()
        {
            CommonPacketItem commonPacketItem = new CommonPacketItem();
            return commonPacketItem;
        }

        public CommonPacketItem CreateUnconnectedDataItem(List<byte> data)
        {
            return new CommonPacketItem(CommonPacketItemIds.UNCONNECTED_MESSAGE, data);
        }
        public CommonPacketItem CreateConnectedDataItem(List<byte> data)  
        {
		    return new CommonPacketItem(CommonPacketItemIds.CONNECTED_TRANSPORT_PACKET, data);
        }
        public CommonPacketItem CreateSequenceAddressItem(uint connectionId, uint seqNumber) 
        {
            DragonBuffer buffer = new DragonBuffer();
            buffer.Write(connectionId);
            buffer.Write(seqNumber);
		    return new CommonPacketItem(CommonPacketItemIds.SEQUENCED_ADDRESS_ITEM, buffer.Data());
        }
    }
}
