using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DragonEthernetIP.EIP
{
    public enum EncapsCommands:ushort
    {
        NOP = 0,
        LIST_SERVICES = 0x0004,
        LIST_IDENTITY = 0x0063,
        LIST_INTERFACES = 0x0064,
        REGISTER_SESSION = 0x0065,
        UN_REGISTER_SESSION = 0x0066,
        SEND_RR_DATA = 0x006F,
        SEND_UNIT_DATA = 0x0070,
        INDICATE_STATUS = 0x0072,
        CANCEL = 0x0073
    }
}
