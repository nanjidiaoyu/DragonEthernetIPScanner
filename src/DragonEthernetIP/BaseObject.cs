using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DragonEthernetIP
{
    public class BaseObject
    {
        private ushort _classId;
        private ushort _instanceId;
        public BaseObject(ushort classId, ushort instanceId)
        {
            this._classId = classId;
            this._instanceId = instanceId;
        }
    }
}
