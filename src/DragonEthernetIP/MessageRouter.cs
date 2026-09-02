using DragonEthernetIP.CIP;
using DragonEthernetIP.EIP;
using DragonEthernetIP.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DragonEthernetIP
{
    public class MessageRouter
    {
        private bool _use_8_bit_path_segments;
        private readonly ILogger _logger;
        public MessageRouter(ILogger logger, bool use_8_bit_path_segments = false)
        {
            this._use_8_bit_path_segments = use_8_bit_path_segments;
            this._logger = logger??NullLogger.Instance;
        }

        public async Task<MessageRouterResponse> SendRequest(SessionInfo si, byte service, EPath path,
                               List<byte> data, List<CommonPacketItem> additionalPacketItems)
        {
            this._logger.LogInformation($"Send request: service={service} epath={path.ToString()}");
            MessageRouterRequest request = new MessageRouterRequest(service, path, data, _use_8_bit_path_segments);
            CommonPacketItemFactory commonPacketItemFactory = new CommonPacketItemFactory();
            CommonPacket commonPacket = new CommonPacket();
            commonPacket.Add(commonPacketItemFactory.CreateNullAddressItem());
            commonPacket.Add(commonPacketItemFactory.CreateUnconnectedDataItem(request.Pack()));
            if (additionalPacketItems != null)
            {
                foreach (var item in additionalPacketItems)
                {
                    commonPacket.Add(item);
                }
            }
            
            var packetToSend = EncapsPacketFactory.CreateSendRRDataPacket(si.GetSessionHandle(), 0, commonPacket.Pack());
            var receivedPacket = await si.SendAndReceive(packetToSend);
            if (receivedPacket == null)
            {
                return null;
            }
            if (receivedPacket.GetData().Count < 6)
            {
                this._logger.LogError($"SendRRData response too short: {receivedPacket.GetData().Count} bytes");
                return null;
            }
            DragonBuffer buffer = new DragonBuffer(receivedPacket.GetData());
            uint interfaceHandle = buffer.ReadUint();
            ushort timeout = buffer.ReadUshort();
            List<byte> receivedData = buffer.ReadByteList(receivedPacket.GetData().Count - 6);
            if (!commonPacket.Expand(receivedData))
            {
                this._logger.LogError("Malformed Common Packet in the response");
                return null;
            }
            var items = commonPacket.GetItems();
            if (items.Count < 2)
            {
                Console.WriteLine($"Response has {items.Count} CPF items, expected >= 2");
                return null;
            }
            MessageRouterResponse response = new MessageRouterResponse();
            if (!response.Expand(items[1].GetData()))
            {
                this._logger.LogError("Malformed Message Router response");
                return null;
            }
            if (items.Count > 2)
            {
                response.SetAdditionalPacketItems(items.GetRange(2, items.Count - 2));
            }
            return response;
        }

        public async Task<MessageRouterResponse> SendRequest(SessionInfo si, byte service, EPath path, List<byte> data)
        {
            return await this.SendRequest(si, service, path, data, null);
        }

        public async Task<MessageRouterResponse> SendRequest(SessionInfo si, byte service, EPath path) {
		return await this.SendRequest(si, service, path, null, null);
	}
}
}

