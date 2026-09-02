using DragonEthernetIP;
using DragonEthernetIP.CIP;
using DragonEthernetIP.CIP.ConnectionManager;
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
ConnectionManager connectionManager = new ConnectionManager("10.103.28.102", loggerFactory.CreateLogger<ConnectionManager>());
ConnectionParameters connectionParameters = new ConnectionParameters();
connectionParameters.ConnectionPath = new List<byte>() { 0x34, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x91, 0x09, 0x54, 0x65, 0x73, 0x74, 0x54, 0x61, 0x67, 0x49, 0x6e, 0x00, 0x91, 0x0a, 0x54, 0x65, 0x73, 0x74, 0x54, 0x61, 0x67, 0x4f, 0x75, 0x74 };
connectionParameters.O2TRealTimeFormat = 1;
connectionParameters.T2ORealTimeFormat = 1;
connectionParameters.OriginatorVendorId = 170;
connectionParameters.OriginatorSerialNumber = 0x504d4153;
connectionParameters.O2TNetworkConnectionParams |= (uint)NetworkConnectionParams.P2P;
connectionParameters.O2TNetworkConnectionParams |= (uint)NetworkConnectionParams.SCHEDULED_PRIORITY;
connectionParameters.O2TNetworkConnectionParams |= 4;
connectionParameters.T2ONetworkConnectionParams |= (uint)NetworkConnectionParams.MULTICAST;
connectionParameters.T2ONetworkConnectionParams |= (uint)NetworkConnectionParams.SCHEDULED_PRIORITY;
connectionParameters.T2ONetworkConnectionParams |= 4;
connectionParameters.O2TRPI = 1000000;
connectionParameters.T2ORPI = 1000000;
connectionParameters.TransportTypeTrigger = (byte)NetworkConnectionParams.CLASS1;
connectionParameters.PriorityTimeTick = 10;
connectionParameters.TimeoutTicks = 240;
connectionParameters.ConnectionTimeoutMultiplier = 2;
await connectionManager.ForwardOpen(sessionInfo, connectionParameters, false);
Console.ReadKey();