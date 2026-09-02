using DragonEthernetIP.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DragonEthernetIP.EIP
{
    internal class EncapsPacketFactory
    {
        public static EncapsPacket CreateRegisterSessionPacket()
        {
            EncapsPacket packet = new EncapsPacket();
            packet.SetCommand(EncapsCommands.REGISTER_SESSION);
            ushort protocolVersion = 1;
            ushort optionFlag = 0;
            DragonBuffer buffer = new DragonBuffer(4);
            buffer.Write(protocolVersion);
            buffer.Write(optionFlag);
            packet.SetData(buffer.Data());
            return packet;
        }

        public static EncapsPacket CreateUnRegisterSessionPacket(uint sessionHandle)
        {
            EncapsPacket packet = new EncapsPacket();
            packet.SetCommand(EncapsCommands.UN_REGISTER_SESSION);
            packet.SetSessionHandle(sessionHandle);
            return packet;
        }

        public static EncapsPacket CreateSendRRDataPacket(uint sessionHandle, ushort timeout, List<byte> data)
        {
            EncapsPacket packet = new EncapsPacket();
            packet.SetCommand(EncapsCommands.SEND_RR_DATA);
            packet.SetSessionHandle(sessionHandle);
            uint interfaceHandle = 0;
            DragonBuffer buffer = new DragonBuffer((uint)(6 + data.Count()));
            buffer.Write(interfaceHandle);
            buffer.Write(timeout);
            buffer.Write(data);
            packet.SetData(buffer.Data());
            return packet;
        }
        public static EncapsPacket CreateListIdentityPacket()
        {
            EncapsPacket packet = new EncapsPacket();
            packet.SetCommand(EncapsCommands.LIST_IDENTITY);
            packet.SetSessionHandle(0);
            return packet;
        }
    }
}
