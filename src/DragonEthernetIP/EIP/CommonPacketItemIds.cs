using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DragonEthernetIP.EIP
{
    public enum CommonPacketItemIds:ushort
    {
        NULL_ADDR = 0x0000,
        LIST_IDENTITY = 0x000C,
        CONNECTION_ADDRESS_ITEM = 0x00A1,
        CONNECTED_TRANSPORT_PACKET = 0x00B1,
        UNCONNECTED_MESSAGE = 0x00B2,
        O2T_SOCKADDR_INFO = 0x8000,
        T2O_SOCKADDR_INFO = 0x8001,
        SEQUENCED_ADDRESS_ITEM = 0x8002,
    }
}
