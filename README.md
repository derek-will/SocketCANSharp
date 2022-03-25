[![NuGet](https://img.shields.io/nuget/v/SocketCANSharp.svg)](https://www.nuget.org/packages/SocketCANSharp)

## SocketCAN# (SocketCANSharp)

This repository contains a .NET managed wrapper for the Linux CAN subsystem (aka SocketCAN). This includes the wrapper library implementation, unit tests, and some example code.

Using this library you can either use the higher level classes or the lower level libc P/Invoke calls directly. 

The classes such as `CanNetworkInterface` and `IsoTpCanSocket` provide a simpler interface via object-oriented abstraction. This approach should be familiar to developers acquainted with C# and other higher level languages.
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

Invoking the libc calls will be easier to those who prefer or are more familiar with C style programming. This approach should also ease the process of porting over code written in C.
```cs
SafeSocketHandle testerSocketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_ISOTP);

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


### Example Code

* CanBusSniffer : Simple CAN bus analyzer
* IsoTpCommSimulator : ISO-TP communication simulation
* ObjectOrientedDiagAppSimulator : Diagnostic Application Simulator using IsoTpCanSocket objects


### Additional Information:

* [License](LICENSE.md)