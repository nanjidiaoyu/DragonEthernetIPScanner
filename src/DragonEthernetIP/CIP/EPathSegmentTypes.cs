using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DragonEthernetIP.CIP
{
    public enum EPathSegmentTypes : byte
    {
        CLASS_8_BITS = 0x20,
		CLASS_16_BITS = 0x21,
		INSTANCE_8_BITS = 0x24,
		INSTANCE_16_BITS = 0x25,
		ATTRIBUTE_8_BITS = 0x30,
		ATTRIBUTE_16_BITS = 0x31,
	};
}
