# SocketCAN# (SocketCANSharp)

SocketCAN# is a .NET managed wrapper for the Linux CAN subsystem (SocketCAN). SocketCAN# enables developers to create either object-oriented or procedural style applications that leverage SocketCAN. 

## Raw CAN Support

```cs
CanNetworkInterface vcan0 = CanNetworkInterface.GetAllInterfaces(true).First(iface => iface.Name.Equals("vcan0"));

using (var rawCanSocket = new RawCanSocket())
{
    rawCanSocket.Bind(vcan0);
    int bytesWritten = rawCanSocket.Write(new CanFrame(0x123, new byte[] { 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef }));
    int bytesRead = receiverSocket.Read(out CanFrame frame);
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
using (SafeFileDescriptorHandle socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_BCM))
{
    var ifr = new Ifreq("vcan0");
    int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);

    var addr = new SockAddrCan(ifr.IfIndex);
    int connectResult = LibcNativeMethods.Connect(socketHandle, addr, Marshal.SizeOf(typeof(SockAddrCan)));

    if (Environment.Is64BitProcess)
    {
        var canFrame = new CanFrame(0x333, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF });
        var header = new BcmMessageHeader(BcmOpcode.TX_SETUP)
        {
            CanId = 0x333,
            Flags = BcmFlags.SETTIMER | BcmFlags.STARTTIMER,
            Interval1Count = 10, // 10 messages
            Interval1 = new BcmTimeval(0, 5000), // at 5 ms interval
            Interval2 = new BcmTimeval(0, 100000), // then at 100 ms
            NumberOfFrames = 1,
        };

        var bcmMessage = new BcmCanMessage(header, new CanFrame[] { canFrame });
        int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
    }
    else // 32-bit process
    {
        var canFrame = new CanFrame(0x333, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF });
        var header = new BcmMessageHeader32(BcmOpcode.TX_SETUP)
        {
            CanId = 0x333,
            Flags = BcmFlags.SETTIMER | BcmFlags.STARTTIMER,
            Interval1Count = 10, // 10 messages
            Interval1 = new BcmTimeval(0, 5000), // at 5 ms interval
            Interval2 = new BcmTimeval(0, 100000), // then at 100 ms
            NumberOfFrames = 1,
        };

        var bcmMessage = new BcmCanMessage32(header, new CanFrame[] { canFrame });
        int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
    }
}
```

## Links

- [Project Home](https://github.com/derek-will/SocketCANSharp)
- [NuGet Package](https://www.nuget.org/packages/SocketCANSharp)
- [Release Notes](https://github.com/derek-will/SocketCANSharp/releases)
- [License](https://github.com/derek-will/SocketCANSharp/blob/main/LICENSE.md)