using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DragonEthernetIP.CIP.ConnectionManager
{
    public enum NetworkConnectionParams:ushort
    {
        // Redundant Owner
        REDUNDANT = (1 << 15),
        OWNED = 0,
        TYPE0 = 0,

        // Connection type.
        MULTICAST = (1 << 13),
        P2P = (2 << 13),

        // Priorities
        LOW_PRIORITY = 0,
        HIGH_PRIORITY = (1 << 10),
        SCHEDULED_PRIORITY = (2 << 10),
        URGENT = (3 << 10),

        // Type of size.
        FIXED = 0,
        VARIABLE = (1 << 9),

        // Type of trigger.
        TRIG_CYCLIC = 0,
        TRIG_CHANGE = (1 << 4),
        TRIG_APP = (2 << 4),

        CLASS0 = 0,
        CLASS1 = 1,
        CLASS2 = 2,
        CLASS3 = 3,
        TRANSP_SERVER = 0x80
    }
}
