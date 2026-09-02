using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DragonEthernetIP.EIP
{
    public enum ConnectionType : byte
    {
        NULL = 0,
        MULTICAST = 1,
        P2P = 2
    }
}
