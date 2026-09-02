using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DragonEthernetIP.CIP
{
    public class DragonEndPoint
    {
        public static ushort EIP_DEFAULT_EXPLICIT_PORT = 44818;
        public static ushort EIP_DEFAULT_IMPLICIT_PORT = 2222;
        public ushort SIN_family { get; set; }
        public ushort SIN_port { get; set; }
        public uint SIN_Address { get; set; }
        public byte[] SIN_Zero { get; set; } = new byte[8];
    }
}
