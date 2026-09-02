using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DragonEthernetIP.CIP.ConnectionManager
{
    internal enum ConnectionManagerServiceCodes:byte
    {
        FORWARD_OPEN = 0x54,
        LARGE_FORWARD_OPEN = 0x5B,
        FORWARD_CLOSE = 0x4E
    }
}
