#region License
/* 
BSD 3-Clause License

Copyright (c) 2021, Derek Will
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

3. Neither the name of the copyright holder nor the names of its
   contributors may be used to endorse or promote products derived from
   this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. 
*/
#endregion

using NUnit.Framework;
using SocketCANSharp;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace SocketCANSharpTest
{
    public class SocketSetupTests
    {
        SafeSocketHandle socketHandle;

        [TearDown]
        public void Cleanup()
        {
            socketHandle.Close();
        }

        [Test]
        public void BindAddress_CAN_RAW_on_vcan0_Interface_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            var ifr = new Ifreq("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
            Assert.AreNotEqual(-1, ioctlResult);

            var addr = new SockAddrCan(ifr.IfIndex);
            int bindResult = LibcNativeMethods.Bind(socketHandle, addr, Marshal.SizeOf(typeof(SockAddrCan)));
            Assert.AreNotEqual(-1, bindResult);
        }

        [Test]
        public void BindAddress_CAN_ISOTP_on_vcan0_Interface_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_ISOTP);
            Assert.IsFalse(socketHandle.IsInvalid);

            var ifr = new Ifreq("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
            Assert.AreNotEqual(-1, ioctlResult);

            var addr = new SockAddrCanIsoTp(ifr.IfIndex)
            {
                TxId = 0x7e0,
                RxId = 0x7e8,
            };
            int bindResult = LibcNativeMethods.Bind(socketHandle, addr, Marshal.SizeOf(typeof(SockAddrCanIsoTp)));
            Assert.AreNotEqual(-1, bindResult);
        }

        [Test]
        public void ConnectAddress_CAN_BCM_on_vcan0_Interface_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_BCM);
            Assert.IsFalse(socketHandle.IsInvalid);

            var ifr = new Ifreq("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
            Assert.AreNotEqual(-1, ioctlResult);

            var addr = new SockAddrCan(ifr.IfIndex);
            int connectResult = LibcNativeMethods.Connect(socketHandle, addr, Marshal.SizeOf(typeof(SockAddrCan)));
            Assert.AreNotEqual(-1, connectResult);
        }

        [Test]
        public void BindAddress_CAN_J1939_on_vcan0_Interface_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_J1939);
            Assert.IsFalse(socketHandle.IsInvalid);

            var ifr = new Ifreq("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
            Assert.AreNotEqual(-1, ioctlResult);

            var addr = new SockAddrCanJ1939(ifr.IfIndex)
            {
                Name = 0x00000000,
                PGN = 0x40000,
                Address = 0xFF,
            };
            int bindResult = LibcNativeMethods.Bind(socketHandle, addr, Marshal.SizeOf(typeof(SockAddrCanJ1939)));
            Assert.AreNotEqual(-1, bindResult);
        }

        [Test]
        public void GetSockName_CAN_RAW_on_vcan0_Interface_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            var ifr = new Ifreq("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
            Assert.AreNotEqual(-1, ioctlResult);

            var addr = new SockAddrCan(ifr.IfIndex);
            int bindResult = LibcNativeMethods.Bind(socketHandle, addr, Marshal.SizeOf(typeof(SockAddrCan)));
            Assert.AreNotEqual(-1, bindResult);

            addr = new SockAddrCan();
            int size = Marshal.SizeOf(typeof(SockAddrCan));
            int getSockNameResult = LibcNativeMethods.GetSockName(socketHandle, addr, ref size);

            Assert.AreEqual(0, getSockNameResult);
            Assert.AreEqual(Marshal.SizeOf(typeof(SockAddrCan)), size);
            Assert.AreEqual(SocketCanConstants.AF_CAN, addr.CanFamily);
            Assert.AreEqual(ifr.IfIndex, addr.CanIfIndex);
        }

        [Test]
        public void GetSockName_CAN_RAW_Unbound_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            var addr = new SockAddrCan();
            int size = Marshal.SizeOf(typeof(SockAddrCan));
            int getSockNameResult = LibcNativeMethods.GetSockName(socketHandle, addr, ref size);

            Assert.AreEqual(0, getSockNameResult);
            Assert.AreEqual(Marshal.SizeOf(typeof(SockAddrCan)), size);
            Assert.AreEqual(SocketCanConstants.AF_CAN, addr.CanFamily);
            Assert.AreEqual(0, addr.CanIfIndex);
        }

        [Test]
        public void GetSockName_CAN_ISOTP_on_vcan0_Interface_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_ISOTP);
            Assert.IsFalse(socketHandle.IsInvalid);

            var ifr = new Ifreq("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
            Assert.AreNotEqual(-1, ioctlResult);

            var addr = new SockAddrCanIsoTp(ifr.IfIndex)
            {
                TxId = 0x7e0,
                RxId = 0x7e8,
            };
            int bindResult = LibcNativeMethods.Bind(socketHandle, addr, Marshal.SizeOf(typeof(SockAddrCanIsoTp)));
            Assert.AreNotEqual(-1, bindResult);

            addr = new SockAddrCanIsoTp();
            int size = Marshal.SizeOf(typeof(SockAddrCanIsoTp));
            int getSockNameResult = LibcNativeMethods.GetSockName(socketHandle, addr, ref size);

            Assert.AreEqual(0, getSockNameResult);
            Assert.AreEqual(Marshal.SizeOf(typeof(SockAddrCanIsoTp)), size);
            Assert.AreEqual(SocketCanConstants.AF_CAN, addr.CanFamily);
            Assert.AreEqual(ifr.IfIndex, addr.CanIfIndex);
            Assert.AreEqual(0x7e0, addr.TxId);
            Assert.AreEqual(0x7e8, addr.RxId);
        }

        [Test]
        public void GetSockName_CAN_ISOTP_Unbound_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_ISOTP);
            Assert.IsFalse(socketHandle.IsInvalid);

            var ifr = new Ifreq("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
            Assert.AreNotEqual(-1, ioctlResult);

            var addr = new SockAddrCanIsoTp();
            int size = Marshal.SizeOf(typeof(SockAddrCanIsoTp));
            int getSockNameResult = LibcNativeMethods.GetSockName(socketHandle, addr, ref size);

            Assert.AreEqual(0, getSockNameResult);
            Assert.AreEqual(Marshal.SizeOf(typeof(SockAddrCanIsoTp)), size);
            Assert.AreEqual(SocketCanConstants.AF_CAN, addr.CanFamily);
            Assert.AreEqual(0, addr.CanIfIndex);
            Assert.AreEqual(0x0, addr.TxId);
            Assert.AreEqual(0x0, addr.RxId);
        }

        [Test]
        public void GetSockName_CAN_J1939_on_vcan0_Interface_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_J1939);
            Assert.IsFalse(socketHandle.IsInvalid);

            var ifr = new Ifreq("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
            Assert.AreNotEqual(-1, ioctlResult);

            var addr = new SockAddrCanJ1939(ifr.IfIndex)
            {
                Name = 0x00000000,
                PGN = 0x40000,
                Address = 0xFF,
            };
            int bindResult = LibcNativeMethods.Bind(socketHandle, addr, Marshal.SizeOf(typeof(SockAddrCanJ1939)));
            Assert.AreNotEqual(-1, bindResult);

            addr = new SockAddrCanJ1939();
            int size = Marshal.SizeOf(typeof(SockAddrCanJ1939));
            int getSockNameResult = LibcNativeMethods.GetSockName(socketHandle, addr, ref size);

            Assert.AreEqual(0, getSockNameResult);
            Assert.AreEqual(Marshal.SizeOf(typeof(SockAddrCanJ1939)), size);
            Assert.AreEqual(SocketCanConstants.AF_CAN, addr.CanFamily);
            Assert.AreEqual(ifr.IfIndex, addr.CanIfIndex);
            Assert.AreEqual(0x00000000, addr.Name);
            Assert.AreEqual(0x40000, addr.PGN);
            Assert.AreEqual(0xFF, addr.Address);
        }
    }
}