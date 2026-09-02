using DragonEthernetIP.CIP;
using DragonEthernetIP.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DragonEthernetIP
{
    public class IdentityObject : BaseObject
    {
        public static ushort CLASS_ID = 0x01;
        public ushort VendorId { get; set; }
        public ushort DeviceType { get; set; }
        public ushort ProductCode { get; set; }
        public CipRevision Revision { get; set; }
        public ushort Status { get; set; }
        public uint SerialNumber { get; set; }
        public string ProductName { get; set; }
        private SessionInfo _sessionInfo;
        private MessageRouter _messageRouter;
        public IdentityObject(ushort instanceId) : base(CLASS_ID, instanceId)
        {
            this.VendorId = 0;
            this.DeviceType = 0;
            this.ProductCode = 0;
            this.Revision = new CipRevision(0, 0);
            this.Status = 0;
            this.SerialNumber = 0;
            this.ProductName = string.Empty;
        }

        public IdentityObject(ushort instanceId, SessionInfo sessionInfo, MessageRouter messageRouter):base(CLASS_ID, instanceId)
        {
            this.VendorId = 0;
            this.DeviceType = 0;
            this.ProductCode = 0;
            this.Revision = new CipRevision(0,0);
            this.Status = 0;
            this.SerialNumber = 0;
            this.ProductName = string.Empty;
            this._sessionInfo = sessionInfo;
            this._messageRouter = messageRouter;
        }

        public async Task ReadIdentityAsync()
        {
            var response = await this._messageRouter.SendRequest(this._sessionInfo, (byte)ServiceCodes.GET_ATTRIBUTE_ALL, new EPath(CLASS_ID, 1), null);
            if (response == null)
            {
                return;
            }
            if (response.GetGeneralStatusCode() == GeneralStatusCodes.SUCCESS)
            {
                DragonBuffer buffer = new DragonBuffer(response.GetData());
                this.VendorId = buffer.ReadUshort();
                this.DeviceType = buffer.ReadUshort();
                this.ProductCode = buffer.ReadUshort();
                this.Revision = buffer.ReadCipRevision();
                this.Status = buffer.ReadUshort();
                this.SerialNumber = buffer.ReadUint();
                this.ProductName = buffer.ReadString();
            }
        }

    }
}
