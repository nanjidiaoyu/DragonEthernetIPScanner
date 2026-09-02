using DragonEthernetIP;
using DragonEthernetIP.CIP;
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

SessionInfo sessionInfo = new SessionInfo("10.103.28.101", DragonEndPoint.EIP_DEFAULT_EXPLICIT_PORT, loggerFactory.CreateLogger<SessionInfo>());
await sessionInfo.RegisterSessionAsync();
MessageRouter messageRouter = new MessageRouter(loggerFactory.CreateLogger<MessageRouter>());
IdentityObject identityObject = new IdentityObject(1, sessionInfo, messageRouter);
await identityObject.ReadIdentityAsync();
Console.WriteLine($"Vendor ID: {identityObject.VendorId}");
Console.WriteLine($"Device Type: {identityObject.DeviceType}");
Console.WriteLine($"Product Code: {identityObject.ProductCode}");
Console.WriteLine($"Revision: {identityObject.Revision}");
Console.WriteLine($"Status: {identityObject.Status}");
Console.WriteLine($"Serial Number: {identityObject.SerialNumber}");
Console.WriteLine($"Product Name: {identityObject.ProductName}");
