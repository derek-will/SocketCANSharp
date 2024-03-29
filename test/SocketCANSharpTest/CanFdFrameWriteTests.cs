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

using System;
using NUnit.Framework;
using SocketCANSharp;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Linq;

namespace SocketCANSharpTest
{
    public class CanFdFrameWriteTests
    {
        SafeFileDescriptorHandle socketHandle;

        [SetUp]
        public void Setup()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            var ifr = new Ifreq("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
            Assert.AreNotEqual(-1, ioctlResult);

            var ifrMtu = new IfreqMtu("vcan0");
            ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFMTU, ifrMtu);
            Assert.AreNotEqual(-1, ioctlResult, $"Errno: {LibcNativeMethods.Errno}");
            Assume.That(ifrMtu.MTU, Is.GreaterThanOrEqualTo(SocketCanConstants.CANFD_MTU));

            var addr = new SockAddrCan(ifr.IfIndex);
            int bindResult = LibcNativeMethods.Bind(socketHandle, addr, Marshal.SizeOf(typeof(SockAddrCan)));
            Assert.AreNotEqual(-1, bindResult);

            int can_fd_enabled = 1;
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_CAN_RAW, CanSocketOptions.CAN_RAW_FD_FRAMES, ref can_fd_enabled, Marshal.SizeOf(can_fd_enabled));
            Assert.AreEqual(0, result);
        }

        [TearDown]
        public void Cleanup()
        {
            socketHandle.Close();
        }

        [Test]
        public void CanFdFrameWrite_Success_Test()
        {
            var canFdFrame = new CanFdFrame(0x123, new byte[] { 0x11, 0x12}, CanFdFlags.None);
            int nBytes = LibcNativeMethods.Write(socketHandle, ref canFdFrame, Marshal.SizeOf(typeof(CanFdFrame)));
            Assert.AreEqual(72, nBytes); 
        }

        [Test]
        public void CanFdFrameWrite_64ByteFrame_Success_Test()
        {            
            byte[] data = new byte[64];
            foreach (byte b in Enumerable.Range(0, 64))
            {
                data[b] = b;
            }
            var writeFrame = new CanFdFrame(0x123, data, CanFdFlags.None);
            int nWriteBytes = LibcNativeMethods.Write(socketHandle, ref writeFrame, Marshal.SizeOf(typeof(CanFdFrame)));
            Assert.AreEqual(72, nWriteBytes); 
        }

        [Test]
        public void CanFdFrameWrite_BaudRateSwitch_Success_Test()
        {
            var canFdFrame = new CanFdFrame(0x123, new byte[] { 0x11, 0x22 }, CanFdFlags.CANFD_BRS);
            int nBytes = LibcNativeMethods.Write(socketHandle, ref canFdFrame, Marshal.SizeOf(typeof(CanFdFrame)));
            Assert.AreEqual(72, nBytes); 
        }

        [Test]
        public void CanFdFrameWrite_ErrorStateIndicator_Success_Test()
        {
            var canFdFrame = new CanFdFrame(0x123, new byte[] { 0x11, 0x22 }, CanFdFlags.CANFD_ESI);
            int nBytes = LibcNativeMethods.Write(socketHandle, ref canFdFrame, Marshal.SizeOf(typeof(CanFdFrame)));
            Assert.AreEqual(72, nBytes); 
        }

        [Test]
        public void CanFdFrameWrite_ToAnyInterface_Success_Test()
        {
            using (var sockHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW))
            {
                Assert.IsFalse(sockHandle.IsInvalid);

                var ifr = new Ifreq("vcan0");
                int ioctlResult = LibcNativeMethods.Ioctl(sockHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
                Assert.AreNotEqual(-1, ioctlResult);

                var ifrMtu = new IfreqMtu("vcan0");
                ioctlResult = LibcNativeMethods.Ioctl(sockHandle, SocketCanConstants.SIOCGIFMTU, ifrMtu);
                Assert.AreNotEqual(-1, ioctlResult, $"Errno: {LibcNativeMethods.Errno}");
                Assume.That(ifrMtu.MTU, Is.GreaterThanOrEqualTo(SocketCanConstants.CANFD_MTU));

                var anyAddr = new SockAddrCan(0);
                int bindResult = LibcNativeMethods.Bind(sockHandle, anyAddr, Marshal.SizeOf(typeof(SockAddrCan)));
                Assert.AreNotEqual(-1, bindResult);

                int can_fd_enabled = 1;
                int result = LibcNativeMethods.SetSockOpt(sockHandle, SocketLevel.SOL_CAN_RAW, CanSocketOptions.CAN_RAW_FD_FRAMES, ref can_fd_enabled, Marshal.SizeOf(can_fd_enabled));
                Assert.AreEqual(0, result);

                var canFdFrame = new CanFdFrame(0x123, new byte[] { 0x11, 0x22 }, CanFdFlags.None);
                var addr = new SockAddrCan(ifr.IfIndex);
                int nBytes = LibcNativeMethods.SendTo(sockHandle, ref canFdFrame, Marshal.SizeOf(typeof(CanFdFrame)), 0, addr, Marshal.SizeOf(typeof(SockAddrCan)));
                Assert.AreEqual(72, nBytes);
            }
        }

        [Test]
        public void CanFdFrameWrite_29BitAddressing_Success_Test()
        {
            var canFdFrame = new CanFdFrame(0x12345678 | (uint)CanIdFlags.CAN_EFF_FLAG, new byte[] { 0x33, 0x44}, CanFdFlags.None);
            int nBytes = LibcNativeMethods.Write(socketHandle, ref canFdFrame, Marshal.SizeOf(typeof(CanFdFrame)));
            Assert.AreEqual(72, nBytes); 
        }

        [Test]
        public void CanFdFrameWrite_InvalidAddress_Ctor_Failure_Test()
        {
            Assert.Throws<ArgumentException>(() => new CanFdFrame(0x18db33f1, new byte[] { 0x11, 0x22 }, CanFdFlags.None));
        }

        [Test]
        public void CanFdFrameWrite_InvalidAddress_Property_Failure_Test()
        {
            var canFdFrame = new CanFdFrame(0x123, new byte[] { 0x11, 0x22 }, CanFdFlags.None);
            Assert.Throws<ArgumentException>(() => canFdFrame.CanId = 0x18db33f1);
        }
    }
}