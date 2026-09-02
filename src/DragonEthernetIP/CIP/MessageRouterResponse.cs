using DragonEthernetIP.EIP;
using DragonEthernetIP.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DragonEthernetIP.CIP
{
    public class MessageRouterResponse
    {
        private ServiceCodes _serviceCode;
        private GeneralStatusCodes _generalStatusCode;
        private List<ushort> _additionalStatus;
        private List<byte> _data;
        private List<CommonPacketItem> _additionalPacketItems;
        public MessageRouterResponse()
        {
            this._serviceCode = ServiceCodes.GET_ATTRIBUTE_ALL;
            this._generalStatusCode = GeneralStatusCodes.SUCCESS;
            this._additionalStatus = new List<ushort>();
            this._data = new List<byte>();
            this._additionalPacketItems = new List<CommonPacketItem>();
        }

        public bool Expand(List<byte> data)
        {
            if (data.Count < 4)
            {
                return false;
            }
            DragonBuffer buffer = new DragonBuffer(data);
            byte reserved = 0;
            byte additionalStatusSize = 0;
            this._serviceCode = (ServiceCodes)buffer.ReadByte();
            reserved = buffer.ReadByte();
            this._generalStatusCode = (GeneralStatusCodes)buffer.ReadByte();
            additionalStatusSize = buffer.ReadByte();
            if (additionalStatusSize * 2 > data.Count - 4)
            {
                return false;
            }
            this._additionalStatus = buffer.ReadUshortList(additionalStatusSize);
            this._data = buffer.ReadByteList(buffer.Size() - buffer.Pos());
            return true;
        }

        public ServiceCodes GetServiceCode() 
        {
		    return this._serviceCode;
	    }

        public GeneralStatusCodes GetGeneralStatusCode()    
        {
		    return this._generalStatusCode;
	    }

        public List<ushort> GetAdditionalStatus()   
        {
		    return this._additionalStatus;
	    }

        public List<byte> GetData()     
        {
		    return this._data;
	    }

        public void SetGeneralStatusCode(GeneralStatusCodes generalStatusCode)
        {
            this._generalStatusCode = generalStatusCode;
        }
        public void SetData(List<byte> data) 
        {
		    this._data = data;
	    }

        public List<CommonPacketItem> GetAdditionalPacketItems() 
        {
		    return this._additionalPacketItems;
	    }

        public void SetAdditionalPacketItems(List<CommonPacketItem> additionalPacketItems) 
        {
		    this._additionalPacketItems = additionalPacketItems;
	    }
    }
}
