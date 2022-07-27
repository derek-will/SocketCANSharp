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

namespace SocketCANSharpTest
{
    public class CanFrameWriteTests
    {
        SafeFileDescriptorHandle socketHandle;

        [SetUp]
        public void Setup()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);
        }

        [TearDown]
        public void Cleanup()
        {
            socketHandle.Close();
        }

        [Test]
        public void CanFrameWrite_Success_Test()
        {
            var ifr = new Ifreq("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
            Assert.AreNotEqual(-1, ioctlResult);

            var addr = new SockAddrCan(ifr.IfIndex);
            int bindResult = LibcNativeMethods.Bind(socketHandle, addr, Marshal.SizeOf(typeof(SockAddrCan)));
            Assert.AreNotEqual(-1, bindResult);

            var canFrame = new CanFrame(0x123, new byte[] { 0x11, 0x22 });

            int nBytes = LibcNativeMethods.Write(socketHandle, ref canFrame, Marshal.SizeOf(typeof(CanFrame)));
            Assert.AreEqual(16, nBytes); 
        }

        [Test]
        public void CanFrameWrite_ToAnyInterface_Success_Test()
        {
            var ifr = new Ifreq("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
            Assert.AreNotEqual(-1, ioctlResult);

            var anyAddr = new SockAddrCan(0);
            int bindResult = LibcNativeMethods.Bind(socketHandle, anyAddr, Marshal.SizeOf(typeof(SockAddrCan)));
            Assert.AreNotEqual(-1, bindResult);

            var canFrame = new CanFrame(0x123, new byte[] { 0x11, 0x22 });
            
            var addr = new SockAddrCan(ifr.IfIndex);
            int nBytes = LibcNativeMethods.SendTo(socketHandle, ref canFrame, Marshal.SizeOf(typeof(CanFrame)), 0, addr, Marshal.SizeOf(typeof(SockAddrCan)));
            Assert.AreEqual(16, nBytes); 
        }

        [Test]
        public void CanFrameWrite_29BitAddressing_Success_Test()
        {
            var ifr = new Ifreq("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
            Assert.AreNotEqual(-1, ioctlResult);

            var addr = new SockAddrCan(ifr.IfIndex);
            int bindResult = LibcNativeMethods.Bind(socketHandle, addr, Marshal.SizeOf(typeof(SockAddrCan)));
            Assert.AreNotEqual(-1, bindResult);

            var canFrame = new CanFrame(0x18da10f1 | (uint)CanIdFlags.CAN_EFF_FLAG, new byte[] { 0x11, 0x44 });

            int nBytes = LibcNativeMethods.Write(socketHandle, ref canFrame, Marshal.SizeOf(typeof(CanFrame)));
            Assert.AreEqual(16, nBytes); 
        }

        [Test]
        public void CanFrameWrite_RemoteFrame_Success_Test()
        {
            var ifr = new Ifreq("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
            Assert.AreNotEqual(-1, ioctlResult);

            var addr = new SockAddrCan(ifr.IfIndex);
            int bindResult = LibcNativeMethods.Bind(socketHandle, addr, Marshal.SizeOf(typeof(SockAddrCan)));
            Assert.AreNotEqual(-1, bindResult);

            var remoteCanFrame = new CanFrame(0x123 | (uint)CanIdFlags.CAN_RTR_FLAG, new byte[] { });
            remoteCanFrame.Length = 4;

            int nBytes = LibcNativeMethods.Write(socketHandle, ref remoteCanFrame, Marshal.SizeOf(typeof(CanFrame)));
            Assert.AreEqual(16, nBytes); 
        }

        [Test]
        public void CanFrameWrite_ErrorFrame_Success_Test()
        {
            var ifr = new Ifreq("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
            Assert.AreNotEqual(-1, ioctlResult);

            var addr = new SockAddrCan(ifr.IfIndex);
            int bindResult = LibcNativeMethods.Bind(socketHandle, addr, Marshal.SizeOf(typeof(SockAddrCan)));
            Assert.AreNotEqual(-1, bindResult);

            var canFrame = new CanFrame((uint)CanErrorClass.CAN_ERR_CRTL | (uint)CanErrorClass.CAN_ERR_LOSTARB | (uint) CanErrorClass.CAN_ERR_PROT | (uint) CanErrorClass.CAN_ERR_TRX | (uint)CanIdFlags.CAN_ERR_FLAG, new byte[] 
            { 
                0x01, // lost arb at bit 1
                (byte) (CanControllerErrorStatus.CAN_ERR_CRTL_RX_OVERFLOW | CanControllerErrorStatus.CAN_ERR_CRTL_TX_OVERFLOW),
                (byte) (CanProtocolErrorType.CAN_ERR_PROT_OVERLOAD | CanProtocolErrorType.CAN_ERR_PROT_TX),
                (byte) (CanProtocolErrorLocation.CAN_ERR_PROT_LOC_DATA),
                (byte) (CanTransceiverErrorStatus.CAN_ERR_TRX_CANH_SHORT_TO_BAT),
                0x00,
                0x50, // tx error count
                0x0a, // rx error count
            });

            int nBytes = LibcNativeMethods.Write(socketHandle, ref canFrame, Marshal.SizeOf(typeof(CanFrame)));
            Assert.AreEqual(16, nBytes); 
        }

        [Test]
        public void CanFrameWrite_InvalidAddress_Ctor_Failure_Test()
        {
            var ifr = new Ifreq("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
            Assert.AreNotEqual(-1, ioctlResult);

            var addr = new SockAddrCan(ifr.IfIndex);
            int bindResult = LibcNativeMethods.Bind(socketHandle, addr, Marshal.SizeOf(typeof(SockAddrCan)));
            Assert.AreNotEqual(-1, bindResult);

            Assert.Throws<ArgumentException>(() => new CanFrame(0x18db33f1, new byte[] { 0x11, 0x22 }));
        }

        [Test]
        public void CanFrameWrite_InvalidAddress_Property_Failure_Test()
        {
            var ifr = new Ifreq("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
            Assert.AreNotEqual(-1, ioctlResult);

            var addr = new SockAddrCan(ifr.IfIndex);
            int bindResult = LibcNativeMethods.Bind(socketHandle, addr, Marshal.SizeOf(typeof(SockAddrCan)));
            Assert.AreNotEqual(-1, bindResult);

            var canFrame = new CanFrame(0x123, new byte[] { 0x11, 0x22 });

            Assert.Throws<ArgumentException>(() => canFrame.CanId = 0x18db33f1);
        }
    }
}