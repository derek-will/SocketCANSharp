#region License
/* 
BSD 3-Clause License

Copyright (c) 2023, Derek Will
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
using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Linq;

namespace SocketCANSharpTest
{
    public class CanXlFrameReadTests
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
            Assume.That(ifrMtu.MTU, Is.EqualTo(SocketCanConstants.CANXL_MTU));

            int can_xl_enabled = 1;
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_CAN_RAW, CanSocketOptions.CAN_RAW_XL_FRAMES, ref can_xl_enabled, Marshal.SizeOf(can_xl_enabled));
            Assert.AreEqual(0, result);

            int enable = 1;
            int recv_own_msgs_result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_CAN_RAW, CanSocketOptions.CAN_RAW_RECV_OWN_MSGS, ref enable, Marshal.SizeOf(1));
            Assert.AreEqual(0, recv_own_msgs_result);

            var addr = new SockAddrCan(ifr.IfIndex);
            int bindResult = LibcNativeMethods.Bind(socketHandle, addr, Marshal.SizeOf(typeof(SockAddrCan)));
            Assert.AreNotEqual(-1, bindResult);
        }

        [TearDown]
        public void Cleanup()
        {
            socketHandle.Close();
        }

        [Test]
        public void CanXlFrameRead_Success_Test()
        {
            var wrtFrame = new CanXlFrame(0x654, CanXlSduType.ClassicalAndFdFrameTunneling, 0x321, new byte[] { 0x33, 0x22, 0x11 }, CanXlFlags.CANXL_XLF);
            int nBytes = LibcNativeMethods.Write(socketHandle, ref wrtFrame, SocketCanUtils.CanXlHeaderSize + wrtFrame.Length);
            Assert.AreEqual(SocketCanUtils.CanXlHeaderSize + wrtFrame.Length, nBytes);

            var readFrame = new CanXlFrame();
            int nReadBytes = LibcNativeMethods.Read(socketHandle, ref readFrame, Marshal.SizeOf<CanXlFrame>());
            Assert.AreEqual(SocketCanUtils.CanXlHeaderSize + wrtFrame.Length, nReadBytes);
            Assert.AreEqual(0x654, readFrame.Priority);
            Assert.AreEqual(CanXlSduType.ClassicalAndFdFrameTunneling, readFrame.SduType);
            Assert.AreEqual(0x321, readFrame.AcceptanceField);
            Assert.AreEqual(3, readFrame.Length);
            Assert.AreEqual(CanXlFlags.CANXL_XLF, readFrame.Flags);
            Assert.IsTrue(wrtFrame.Data.SequenceEqual(readFrame.Data));
        }

        [Test]
        public void CanXlFrameRead_2048ByteFrame_Success_Test()
        {
            byte[] data = new byte[2048];
            foreach (byte b in Enumerable.Range(0, 2048))
            {
                data[b] = b;
            }

            var wrtFrame = new CanXlFrame(0x654, CanXlSduType.ContentBasedAddressing, 0x321, data, CanXlFlags.CANXL_XLF);
            int nBytes = LibcNativeMethods.Write(socketHandle, ref wrtFrame, SocketCanUtils.CanXlHeaderSize + wrtFrame.Length);
            Assert.AreEqual(SocketCanUtils.CanXlHeaderSize + wrtFrame.Length, nBytes);

            var readFrame = new CanXlFrame();
            int nReadBytes = LibcNativeMethods.Read(socketHandle, ref readFrame, Marshal.SizeOf<CanXlFrame>());
            
            Assert.AreEqual(SocketCanUtils.CanXlHeaderSize + wrtFrame.Length, nReadBytes);
            Assert.AreEqual(0x654, readFrame.Priority);
            Assert.AreEqual(CanXlSduType.ContentBasedAddressing, readFrame.SduType);
            Assert.AreEqual(0x321, readFrame.AcceptanceField);
            Assert.AreEqual(2048, readFrame.Length);
            Assert.AreEqual(CanXlFlags.CANXL_XLF, readFrame.Flags);
            Assert.IsTrue(wrtFrame.Data.SequenceEqual(readFrame.Data));
        }

        [Test]
        public void CanXlFrameRead_Timeout_Failure_Test()
        {
            var timeval = new Timeval(0, 1);
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_SOCKET, SocketLevelOptions.SO_RCVTIMEO, timeval, Marshal.SizeOf(typeof(Timeval)));
            Assert.AreEqual(0, result);

            var readFrame = new CanXlFrame();
            int nReadBytes = LibcNativeMethods.Read(socketHandle, ref readFrame, Marshal.SizeOf<CanXlFrame>());        
            Assert.AreEqual(-1, nReadBytes);
        }

        [Test]
        public void CanXlFrameRead_29BitAddressing_Success_Test()
        {
            var wrtFrame = new CanXlFrame(0x654, CanXlSduType.ClassicalAndFdFrameTunneling, (uint)CanIdFlags.CAN_EFF_FLAG | 0x18db33f1, new byte[] { 0x33, 0x22, 0x11 }, CanXlFlags.CANXL_XLF);
            int nBytes = LibcNativeMethods.Write(socketHandle, ref wrtFrame, SocketCanUtils.CanXlHeaderSize + wrtFrame.Length);
            Assert.AreEqual(SocketCanUtils.CanXlHeaderSize + wrtFrame.Length, nBytes);

            var readFrame = new CanXlFrame();
            int nReadBytes = LibcNativeMethods.Read(socketHandle, ref readFrame, Marshal.SizeOf<CanXlFrame>());
            Assert.AreEqual(SocketCanUtils.CanXlHeaderSize + wrtFrame.Length, nReadBytes);
            Assert.AreEqual(0x654, readFrame.Priority);
            Assert.AreEqual(CanXlSduType.ClassicalAndFdFrameTunneling, readFrame.SduType);
            Assert.AreEqual((uint)CanIdFlags.CAN_EFF_FLAG | 0x18db33f1, readFrame.AcceptanceField);
            Assert.AreEqual(3, readFrame.Length);
            Assert.AreEqual(CanXlFlags.CANXL_XLF, readFrame.Flags);
            Assert.IsTrue(wrtFrame.Data.SequenceEqual(readFrame.Data));
        }
    }
}