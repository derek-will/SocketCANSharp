[![NuGet](https://img.shields.io/nuget/v/SocketCANSharp.svg)](https://www.nuget.org/packages/SocketCANSharp)

## SocketCAN# (SocketCANSharp)

This repository contains a .NET managed wrapper for the Linux CAN subsystem (aka SocketCAN). This includes the wrapper library implementation, unit tests, and some example code.

Using this library you can either use the higher level classes or the lower level libc P/Invoke calls directly. 

The classes such as `CanNetworkInterface`, `RawCanSocket`, and `IsoTpCanSocket` provide a simpler interface via object-oriented abstraction. This approach should be familiar to developers acquainted with C# and other higher level languages. Invoking the libc calls will be easier to those who prefer or are more familiar with C style (procedural) programming. This approach should also ease the process of porting over code written in C.

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
SafeFileDescriptorHandle socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);

var ifr = new Ifreq("vcan0");
int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);

var addr = new SockAddrCan(ifr.IfIndex);
int bindResult = LibcNativeMethods.Bind(socketHandle, addr, Marshal.SizeOf(typeof(SockAddrCan)));

var writeFrame = new CanFrame(0x123, new byte[] { 0x11, 0x22 });
int nWriteBytes = LibcNativeMethods.Write(socketHandle, ref writeFrame, Marshal.SizeOf(typeof(CanFrame)));

var readFrame = new CanFrame();
int nReadBytes = LibcNativeMethods.Read(socketHandle, ref readFrame, Marshal.SizeOf(typeof(CanFrame)));
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
SafeFileDescriptorHandle testerSocketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_ISOTP);

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
```

### Other SocketCAN features
J1939 and BCM (Broadcast Manager) are supported by this library, but currently only at the basic level of providing a procedural style interface. Object-Oriented support for these socket types are planned to be implemented and released in the future. 

### Example Code

* CanBusSniffer : Simple CAN bus analyzer
* IsoTpCommSimulator : ISO-TP communication simulation
* ObjectOrientedDiagAppSimulator : Diagnostic Application Simulator using IsoTpCanSocket objects


### Additional Information:

* [License](LICENSE.md)