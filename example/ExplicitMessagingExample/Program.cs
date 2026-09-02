
using DragonEthernetIP;
using DragonEthernetIP.CIP;
using DragonEthernetIP.Utils;
using Microsoft.Extensions.Logging;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .SetMinimumLevel(LogLevel.Debug)          
        .AddSimpleConsole(o =>
        {
            o.SingleLine = true;
            o.TimestampFormat = "HH:mm:ss.fff ";
        });
});

SessionInfo sessionInfo = new SessionInfo("10.103.28.101", DragonEndPoint.EIP_DEFAULT_EXPLICIT_PORT,loggerFactory.CreateLogger<SessionInfo>());
await sessionInfo.RegisterSessionAsync();
MessageRouter messageRouter = new MessageRouter(loggerFactory.CreateLogger<MessageRouter>());
var response = await messageRouter.SendRequest(sessionInfo, (byte)ServiceCodes.GET_ATTRIBUTE_SINGLE, new EPath(0x01, 1, 1));
if (response != null)
{
    if (response.GetGeneralStatusCode() == GeneralStatusCodes.SUCCESS)
    {
        DragonBuffer buffer = new DragonBuffer(response.GetData());
        ushort vendorId = buffer.ReadUshort();
    }
    else
    {
        Console.WriteLine($"Error: {response.GetGeneralStatusCode()}");
    }
}

