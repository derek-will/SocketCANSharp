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

using System;
using NUnit.Framework;
using SocketCANSharp;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Linq;

namespace SocketCANSharpTest
{
    public class CanXlFrameWriteTests
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

            var addr = new SockAddrCan(ifr.IfIndex);
            int bindResult = LibcNativeMethods.Bind(socketHandle, addr, Marshal.SizeOf(typeof(SockAddrCan)));
            Assert.AreNotEqual(-1, bindResult);

            int can_xl_enabled = 1;
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_CAN_RAW, CanSocketOptions.CAN_RAW_XL_FRAMES, ref can_xl_enabled, Marshal.SizeOf(can_xl_enabled));
            Assert.AreEqual(0, result);
        }

        [TearDown]
        public void Cleanup()
        {
            socketHandle.Close();
        }

        [Test]
        public void CanXlFrameWrite_Success_Test()
        {
            var canXlFrame = new CanXlFrame(0x654, CanXlSduType.ClassicalAndFdFrameTunneling, 0x321, new byte[] { 0x33, 0x22, 0x11 }, CanXlFlags.CANXL_XLF);
            int nBytes = LibcNativeMethods.Write(socketHandle, ref canXlFrame, SocketCanUtils.CanXlHeaderSize + canXlFrame.Length);
            Assert.AreEqual(SocketCanUtils.CanXlHeaderSize + canXlFrame.Length, nBytes);
        }

        [Test]
        public void CanXlFrameWrite_2048ByteFrame_Success_Test()
        {            
            byte[] data = new byte[2048];
            foreach (byte b in Enumerable.Range(0, 2048))
            {
                data[b] = b;
            }
            var canXlFrame = new CanXlFrame(0x654, CanXlSduType.ContentBasedAddressing, 0x321, data, CanXlFlags.CANXL_XLF);
            int nBytes = LibcNativeMethods.Write(socketHandle, ref canXlFrame, SocketCanUtils.CanXlHeaderSize + canXlFrame.Length);
            Assert.AreEqual(SocketCanUtils.CanXlHeaderSize + canXlFrame.Length, nBytes);
        }

        [Test]
        public void CanXlFrameWrite_SimpleExtendedContent_Success_Test()
        {
            var canXlFrame = new CanXlFrame(0x654, CanXlSduType.ContentBasedAddressing, 0x321, new byte[] { 0x01, 0x02 }, CanXlFlags.CANXL_XLF | CanXlFlags.CANXL_SEC);
            int nBytes = LibcNativeMethods.Write(socketHandle, ref canXlFrame, SocketCanUtils.CanXlHeaderSize + canXlFrame.Length);
            Assert.AreEqual(SocketCanUtils.CanXlHeaderSize + canXlFrame.Length, nBytes);
        }

        [Test]
        public void CanXlFrameWrite_InvalidAddress_Ctor_Pass_Test()
        {
            var frame = new CanXlFrame(0x800, CanXlSduType.ClassicalAndFdFrameTunneling, 0x321, new byte[] { 0x33, 0x22, 0x11 }, CanXlFlags.CANXL_XLF);
            Assert.AreEqual(0x800, frame.Priority);
        }

        [Test]
        public void CanXlFrameWrite_InvalidAddress_Property_Pass_Test()
        {
            var canXlFrame = new CanXlFrame(0x654, CanXlSduType.ClassicalAndFdFrameTunneling, 0x321, new byte[] { 0x33, 0x22, 0x11 }, CanXlFlags.CANXL_XLF);
            canXlFrame.Priority = 0x800;
            Assert.AreEqual(0x800, canXlFrame.Priority);
        }

        [Test]
        public void CanXlFrameWrite_InvalidMessageLength_Failure_Test()
        {
            var canXlFrame = new CanXlFrame(0x654, CanXlSduType.ClassicalAndFdFrameTunneling, 0x321, new byte[] { 0x33, 0x22, 0x11 }, CanXlFlags.CANXL_XLF);
            int nBytes = LibcNativeMethods.Write(socketHandle, ref canXlFrame, Marshal.SizeOf<CanXlFrame>());
            Assert.AreEqual(-1, nBytes);
        }

        [Test]
        public void CanXlFrameWrite_TooSmallLength_Failure_Test()
        {
            var canXlFrame = new CanXlFrame(0x654, CanXlSduType.ClassicalAndFdFrameTunneling, 0x321, new byte[] { 0x33, 0x22, 0x11 }, CanXlFlags.CANXL_XLF);
            int nBytes = LibcNativeMethods.Write(socketHandle, ref canXlFrame, SocketCanUtils.CanXlHeaderSize);
            Assert.AreEqual(-1, nBytes);
        }

        [Test]
        public void CanXlFrameWrite_TooLargeLength_Failure_Test()
        {
            var canXlFrame = new CanXlFrame(0x654, CanXlSduType.ClassicalAndFdFrameTunneling, 0x321, new byte[] { 0x33, 0x22, 0x11 }, CanXlFlags.CANXL_XLF);
            int nBytes = LibcNativeMethods.Write(socketHandle, ref canXlFrame, SocketCanConstants.CANXL_MTU + 1);
            Assert.AreEqual(-1, nBytes);
        }
    }
}