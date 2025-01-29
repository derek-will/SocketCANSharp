# SocketCAN# (SocketCANSharp)

SocketCAN# is a .NET managed wrapper for the Linux CAN subsystem (SocketCAN). SocketCAN# enables developers to create either object-oriented or procedural style applications that leverage SocketCAN. 

## Raw CAN Support

```cs
CanNetworkInterface vcan0 = CanNetworkInterface.GetAllInterfaces(true).First(iface => iface.Name.Equals("vcan0"));

using (var rawCanSocket = new RawCanSocket())
{
    rawCanSocket.Bind(vcan0);
    int bytesWritten = rawCanSocket.Write(new CanFrame(0x123, new byte[] { 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef }));
    int bytesRead = rawCanSocket.Read(out CanFrame frame);
}
```

## ISO 15765-2 (ISO-TP) Support

```cs
CanNetworkInterface vcan0 = CanNetworkInterface.GetAllInterfaces(true).First(iface => iface.Name.Equals("vcan0"));

using (var testerSocket = new IsoTpCanSocket())
{
    testerSocket.BaseOptions = new CanIsoTpOptions()
    {
        Flags = IsoTpFlags.CAN_ISOTP_TX_PADDING | IsoTpFlags.CAN_ISOTP_WAIT_TX_DONE,
    };

    testerSocket.Bind(vcan0, 0x600, 0x700);
    int bytesWritten = testerSocket.Write(new byte[] { 0x3e, 0x00 });
    var receiveBuffer = new byte[4095];
    int bytesRead = testerSocket.Read(receiveBuffer);
}
```

## SAE J1939 Support

```cs
var vcan0 = CanNetworkInterface.GetAllInterfaces(true).FirstOrDefault(iface => iface.Name.Equals("vcan0"));

using (var j1939Socket = new J1939CanSocket())
{
    j1939Socket.EnableBroadcast = true;
    j1939Socket.SendPriority = 3;
    j1939Socket.Bind(vcan0, SocketCanConstants.J1939_NO_NAME, 0x0F004, 0x01);
    var destAddr = new SockAddrCanJ1939(vcan0.Index)
    {
        Name = SocketCanConstants.J1939_NO_NAME,
        PGN = 0x0F004,
        Address = SocketCanConstants.J1939_NO_ADDR,
    };
    j1939Socket.WriteTo(new byte[] { 0xFF, 0xFF, 0xFF, 0x6C, 0x50, 0xFF, 0xFF, 0xFF }, MessageFlags.None, destAddr);
}
```

## Broadcast Manager (BCM) Support

```cs
CanNetworkInterface vcan0 = CanNetworkInterface.GetAllInterfaces(true).First(iface => iface.Name.Equals("vcan0"));

using (var bcmCanSocket = new BcmCanSocket())
{
    bcmCanSocket.Connect(vcan0);
    var canFrame = new CanFrame(0x333, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF });
    var frames = new CanFrame[] { canFrame };
    var config = new BcmCyclicTxTaskConfiguration()
    {
        Id = 0x333,
        StartTimer = true,
        SetInterval = true,
        InitialIntervalConfiguration = new BcmInitialIntervalConfiguration(10, new BcmTimeval(0, 5000)), // 10 messages at 5 ms
        PostInitialInterval = new BcmTimeval(0, 100000), // Then at 100 ms
    };
    int nBytes = bcmCanSocket.CreateCyclicTransmissionTask(config, frames);
}
```

## CAN Gateway (CGW) Support
```cs
var vcan0 = CanNetworkInterface.GetAllInterfaces(true).FirstOrDefault(iface => iface.Name.Equals("vcan0"));
var vcan1 = CanNetworkInterface.GetAllInterfaces(true).FirstOrDefault(iface => iface.Name.Equals("vcan1"));

using (var cgwSocket = new CanGatewaySocket())
{
    cgwSocket.ReceiveTimeout = 1000;
    cgwSocket.SendTimeout = 1000;
    cgwSocket.Bind(new SockAddrNetlink(0, 0));
    var rule = new CgwCanToCanRule(CgwCanFrameType.ClassicalCAN)
    {
        SourceIndex = (uint)vcan0.Index,
        DestinationIndex = (uint)vcan1.Index,
        EnableLocalCanSocketLoopback = true,
        ReceiveFilter = new CanFilter(0x123, 0x7FF),
        SetModifier = new ClassicalCanGatewayModifier(CanGatewayModificationType.CGW_MOD_LEN, new CanFrame(0x000, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00 })),
        ChecksumXorConfiguration = new CgwChecksumXor(0, 3, 4, 0xCC),
        UpdateIdentifier = 0xFFEEEEDD,
    };
    cgwSocket.AddOrUpdateCanToCanRule(rule);
    
    var data = new byte[8192];
    int bytesRead = cgwSocket.Read(data);
    var realData = data.Take(bytesRead).ToArray();
    NetlinkMessageHeader header = NetlinkUtils.PeekAtHeader(realData);
    if (header.MessageType == NetlinkMessageType.NLMSG_ERROR)
    {
        NetlinkMessageError nlMsgErr = NetlinkMessageError.FromBytes(realData);
        if (nlMsgErr.Error == 0)
        {
            Console.WriteLine("Successfully added CGW Rule!");
        }
    }
}
```

## CAN Network Interface Support
```cs
var can0 = CanNetworkInterface.GetAllInterfaces(false).First(iface => iface.Name.Equals("can0"));

Console.WriteLine("Bringing Link Down...");
iface.SetLinkDown();

Console.WriteLine("Configuring Interface...");
iface.BitTiming = new CanBitTiming() { BitRate = 125000 };
iface.CanControllerModeFlags = CanControllerModeFlags.CAN_CTRLMODE_LOOPBACK | CanControllerModeFlags.CAN_CTRLMODE_ONE_SHOT;
iface.AutoRestartDelay = 5;
iface.MaximumTransmissionUnit = SocketCanConstants.CAN_MTU;

Console.WriteLine("Bringing Link Up...");
iface.SetLinkUp();

Console.WriteLine("Reading Interface Properties...");
Console.WriteLine($"Inteface Device Flags: {iface.DeviceFlags}");
Console.WriteLine($"Inteface Operational Status: {iface.OperationalStatus}");
Console.WriteLine($"Controller State: {iface.CanControllerState}");
Console.WriteLine($"Auto-Restart Delay: {iface.AutoRestartDelay} ms");
Console.WriteLine($"Bit Timing: {iface.BitTiming.BitRate} bit/s");
Console.WriteLine($"Controller Mode Flags: {iface.CanControllerModeFlags}");
Console.WriteLine($"MTU: {iface.MaximumTransmissionUnit}");
```

## Links

- [Project Home](https://github.com/derek-will/SocketCANSharp)
- [NuGet Package](https://www.nuget.org/packages/SocketCANSharp)
- [Release Notes](https://github.com/derek-will/SocketCANSharp/releases)
- [License](https://github.com/derek-will/SocketCANSharp/blob/main/LICENSE.md)