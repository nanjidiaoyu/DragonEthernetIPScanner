using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DragonEthernetIP.CIP
{
    public class CipRevision
    {
        private byte _majorRevision;
        private byte _minorRevision;
        public CipRevision(byte majorRevision,byte minorRevision)
        {
            this._majorRevision = majorRevision;
            this._minorRevision = minorRevision;
        }

        public byte GetMajorRevision()
        {
            return _majorRevision;
        }
        public byte GetMinorRevision()
        {
            return _minorRevision;
        }
        public override string ToString()
        {
            return _majorRevision.ToString() + "." + _minorRevision.ToString();
        }
    }
}
