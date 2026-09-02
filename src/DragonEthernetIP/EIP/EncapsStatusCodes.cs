using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DragonEthernetIP.EIP
{
    public enum EncapsStatusCodes:uint
    {
        SUCCESS = 0x0000,
        UNSUPPORTED_COMMAND = 0x0001,
        INSUFFICIENT_MEMORY = 0x0002,
        INVALID_FORMAT_OR_DATA = 0x0003,
        INVALID_SESSION_HANDLE = 0x0064,
        UNSUPPORTED_PROTOCOL_VERSION = 0x0069
    }
}
