[![NuGet](https://img.shields.io/nuget/v/SocketCANSharp.svg)](https://www.nuget.org/packages/SocketCANSharp)
[![NuGet](https://img.shields.io/nuget/dt/SocketCANSharp.svg)](https://www.nuget.org/packages/SocketCANSharp)

## SocketCAN# (SocketCANSharp)

This repository contains a .NET managed wrapper for the Linux CAN subsystem (aka SocketCAN). This includes the wrapper library implementation, unit tests, and some example code.

Using this library you can either use the higher level classes or the lower level libc P/Invoke calls directly. 

The classes such as `CanNetworkInterface`, `RawCanSocket`, and `IsoTpCanSocket` provide a simpler interface via object-oriented abstraction. This approach should be familiar to developers acquainted with C# and other higher level languages. Invoking the libc calls will be easier to those who prefer or are more familiar with C style (procedural) programming. This approach should also ease the process of porting over code written in C.

#### Feature Chart

| Feature                          | Kernel | SocketCAN#                | 
|:---------------------------------|:-------|:--------------------------|
| Raw CAN Socket                   | 2.6.25 | 0.1[^2], 0.5              | 
| BCM Socket                       | 2.6.25 | 0.1[^2][^1], 0.8[^4], 0.9 |
| ISO-TP (ISO 15765-2) Socket      | 5.10   | 0.1[^2], 0.3              | 
| SAE J1939 Socket                 | 5.4    | 0.1[^2], 0.8              |
| CAN Network Interface Query      | 2.6.25 | 0.2                       |
| CAN Network Interface Attributes | 2.6.31 | 0.10[^3]                  |
| CAN-to-CAN Gateway               | 3.2    | 0.11                      |
| Classical CAN                    | 2.6.25 | 0.1                       |
| CAN FD                           | 3.6    | 0.1                       |
| CAN XL                           | 6.2    | Planned Support ([#50](//github.com/derek-will/SocketCANSharp/issues/50)) |
| CAN XL VCID                      | 6.9    | Planned Support ([#81](//github.com/derek-will/SocketCANSharp/issues/81)) |
| epoll API                        | 2.5.44 | 0.4[^1], 0.8[^4]          |
| Capabilities API                 | 2.2    | 0.11                      |

[^1]: 64-bit process support only
[^2]: Low-level API only 
[^3]: Read-only
[^4]: Added 32-bit process support

### Raw CAN Support

#### Object-Oriented Style
```cs
CanNetworkInterface vcan0 = CanNetworkInterface.GetAllInterfaces(true).First(iface => iface.Name.Equals("vcan0"));

using (var rawCanSocket = new RawCanSocket())
{
    rawCanSocket.Bind(vcan0);
    int bytesWritten = rawCanSocket.Write(new CanFrame(0x123, new byte[] { 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef }));
    int bytesRead = receiverSocket.Read(out CanFrame frame);
}
```

#### Procedural Style
```cs
using (SafeFileDescriptorHandle socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW))
{
    var ifr = new Ifreq("vcan0");
    int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);

    var addr = new SockAddrCan(ifr.IfIndex);
    int bindResult = LibcNativeMethods.Bind(socketHandle, addr, Marshal.SizeOf(typeof(SockAddrCan)));

    var writeFrame = new CanFrame(0x123, new byte[] { 0x11, 0x22 });
    int nWriteBytes = LibcNativeMethods.Write(socketHandle, ref writeFrame, Marshal.SizeOf(typeof(CanFrame)));

    var readFrame = new CanFrame();
    int nReadBytes = LibcNativeMethods.Read(socketHandle, ref readFrame, Marshal.SizeOf(typeof(CanFrame)));
}
```

### ISO 15765-2 (ISO-TP) Support

#### Object-Oriented Style
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

#### Procedural Style
```cs
using (SafeFileDescriptorHandle testerSocketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_ISOTP))
{
    var ifr = new Ifreq("vcan0");
    int ioctlResult = LibcNativeMethods.Ioctl(testerSocketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);

    var testerAddr = new SockAddrCanIsoTp(ifr.IfIndex)
    {
        TxId = 0x600,
        RxId = 0x700,
    };
    int bindResult = LibcNativeMethods.Bind(testerSocketHandle, testerAddr, Marshal.SizeOf(typeof(SockAddrCanIsoTp)));

    var requestMessage = new byte[] { 0x3e, 0x00 };
    int nBytes = LibcNativeMethods.Write(testerSocketHandle, requestMessage, requestMessage.Length);
    var receiveResponseMessage = new byte[4095];
    nBytes = LibcNativeMethods.Read(testerSocketHandle, receiveResponseMessage, receiveResponseMessage.Length);
}
```

### SAE J1939 Support

#### Object-Oriented Style
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

#### Procedural Style
```cs
using (var j1939Handle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_J1939))
{
    var ifr = new Ifreq("vcan0");
    int ioctlResult = LibcNativeMethods.Ioctl(j1939Handle, SocketCanConstants.SIOCGIFINDEX, ifr);

    int value = 1;
    int enableBroadcastResult = LibcNativeMethods.SetSockOpt(j1939Handle, SocketLevel.SOL_SOCKET, SocketLevelOptions.SO_BROADCAST, ref value, Marshal.SizeOf(typeof(int)));

    int prio = 3;
    int prioResult = LibcNativeMethods.SetSockOpt(j1939Handle, SocketLevel.SOL_CAN_J1939, J1939SocketOptions.SO_J1939_SEND_PRIO, ref prio, Marshal.SizeOf(typeof(int)));

    var srcAddr = new SockAddrCanJ1939(ifr.IfIndex)
    {
        Name = SocketCanConstants.J1939_NO_NAME,
        PGN = 0x0F004,
        Address = 0x01,
    };
    int bindResult = LibcNativeMethods.Bind(j1939Handle, srcAddr, Marshal.SizeOf(typeof(SockAddrCanJ1939)));;

    var dstAddr = new SockAddrCanJ1939(vcan0.Index)
    {
        Name = SocketCanConstants.J1939_NO_NAME,
        PGN = 0x0F004,
        Address = SocketCanConstants.J1939_NO_ADDR,
    };
    byte[] data = new byte[] { 0xFF, 0xFF, 0xFF, 0x34, 0x12, 0xFF, 0xFF, 0xFF };
    int sendToResult = LibcNativeMethods.SendTo(j1939Handle, data, data.Length, MessageFlags.None, dstAddr, Marshal.SizeOf(typeof(SockAddrCanJ1939)));
}
```

### Broadcast Manager (BCM) Support

#### Object-Oriented Style
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

#### Procedural Style
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

### CAN Gateway (CGW) Support
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

### Supported Environments
Thorough testing has been done for x64, ARM32 and ARM64 on Linux. Support for Raw CAN, Broadcast Manager, and CAN Gateway have been confirmed as far back as Linux Kernel 4.9. Testing on Alpine Linux has not been carried out yet.

SocketCAN# releases prior to 0.7.0 target .NET 5. SocketCAN# v0.7.0 and later target .NET Standard 2.0.

SocketCAN# has been tested to run on Windows via the WSL (Windows Subsystem for Linux). Instructions on how to set up a WSL instance for running SocketCAN# are available [here](doc/WSLSetup.md).

### Example Code

* CanBusSniffer : Simple CAN bus analyzer
* IsoTpCommSimulator : ISO-TP communication simulation
* ObjectOrientedDiagAppSimulator : Diagnostic Application Simulator using IsoTpCanSocket objects
* J1939EngineSpeedTransmit : Simple application that sends the J1939 Engine Speed signal a couple of different ways using SocketCAN#
* CanNetlinkReader : Simple application that reads out CAN interface information using Netlink through various options offered by the SocketCAN# library
* J1939Sniffer : Basic J1939 bus sniffer utility to illustrate how to use SocketCAN# to monitor a J1939 network.
* CanGatewayConfigTool : Sample application which adds a CAN GW rule, tests it, and then clears all CAN GW rules.


### Additional Information:

* [License](LICENSE.md)
* [Comparison with Iot.Device.SocketCan](doc/IotDeviceSocketCanComparison.md)
