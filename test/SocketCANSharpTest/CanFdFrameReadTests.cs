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
using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Linq;

namespace SocketCANSharpTest
{
    public class CanFdFrameReadTests
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
            Assume.That(ifrMtu.MTU, Is.EqualTo(SocketCanConstants.CANFD_MTU));

            int can_fd_enabled = 1;
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_CAN_RAW, CanSocketOptions.CAN_RAW_FD_FRAMES, ref can_fd_enabled, Marshal.SizeOf(can_fd_enabled));
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
        public void CanFdFrameRead_Success_Test()
        {
            var writeFrame = new CanFdFrame(0x123, new byte[] { 0x11, 0x12}, CanFdFlags.None);
            int nBytes = LibcNativeMethods.Write(socketHandle, ref writeFrame, Marshal.SizeOf(typeof(CanFdFrame)));
            Assert.AreEqual(72, nBytes); 

            var readFrame = new CanFdFrame();
            int nReadBytes = LibcNativeMethods.Read(socketHandle, ref readFrame, Marshal.SizeOf(typeof(CanFdFrame)));        
            Assert.AreEqual(72, nReadBytes);
            Assert.AreEqual(0x123, readFrame.CanId);
            Assert.AreEqual(2, readFrame.Length);
            Assert.IsTrue(writeFrame.Data.SequenceEqual(readFrame.Data));
        }

        [Test]
        public void CanFdFrameRead_64ByteFrame_Success_Test()
        {
            byte[] data = new byte[64];
            foreach (byte b in Enumerable.Range(0, 64))
            {
                data[b] = b;
            }

            var writeFrame = new CanFdFrame(0x123, data, CanFdFlags.None);

            int nWriteBytes = LibcNativeMethods.Write(socketHandle, ref writeFrame, Marshal.SizeOf(typeof(CanFdFrame)));
            Assert.AreEqual(72, nWriteBytes); 

            var readFrame = new CanFdFrame();
            int nReadBytes = LibcNativeMethods.Read(socketHandle, ref readFrame, Marshal.SizeOf(typeof(CanFdFrame)));
            
            Assert.AreEqual(72, nReadBytes);
            Assert.AreEqual(0x123, readFrame.CanId);
            Assert.AreEqual(64, readFrame.Length);
            Assert.IsTrue(writeFrame.Data.SequenceEqual(readFrame.Data));
        }

        [Test]
        public void CanFdFrameRead_Timeout_Failure_Test()
        {
            var timeval = new Timeval(0, 1);
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_SOCKET, SocketLevelOptions.SO_RCVTIMEO, timeval, Marshal.SizeOf(typeof(Timeval)));
            Assert.AreEqual(0, result);

            var readFrame = new CanFdFrame();
            int nReadBytes = LibcNativeMethods.Read(socketHandle,  ref readFrame, Marshal.SizeOf(typeof(CanFdFrame)));
            
            Assert.AreNotEqual(72, nReadBytes);
        }

        [Test]
        public void CanFdFrameRead_FromAnyInterface_Test()
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
                Assume.That(ifrMtu.MTU, Is.EqualTo(SocketCanConstants.CANFD_MTU));

                int can_fd_enabled = 1;
                int result = LibcNativeMethods.SetSockOpt(sockHandle, SocketLevel.SOL_CAN_RAW, CanSocketOptions.CAN_RAW_FD_FRAMES, ref can_fd_enabled, Marshal.SizeOf(can_fd_enabled));
                Assert.AreEqual(0, result);

                int enable = 1;
                int recv_own_msgs_result = LibcNativeMethods.SetSockOpt(sockHandle, SocketLevel.SOL_CAN_RAW, CanSocketOptions.CAN_RAW_RECV_OWN_MSGS, ref enable, Marshal.SizeOf(1));
                Assert.AreEqual(0, recv_own_msgs_result);

                // Set up CAN interface and bind it to 'any' interface (interface index set to 0)
                var anyAddr = new SockAddrCan(0);
                int bindResult = LibcNativeMethods.Bind(sockHandle, anyAddr, Marshal.SizeOf(typeof(SockAddrCan)));
                Assert.AreNotEqual(-1, bindResult);

                // Write to specified outgoing interface
                var writeFrame = new CanFdFrame(0x123, new byte[] { 0x11, 0x12 }, CanFdFlags.None);
                var wAddr = new SockAddrCan(ifr.IfIndex);
                int nBytes = LibcNativeMethods.SendTo(sockHandle, ref writeFrame, Marshal.SizeOf(typeof(CanFdFrame)), 0, wAddr, Marshal.SizeOf(typeof(SockAddrCan)));
                Assert.AreEqual(72, nBytes);

                // Read and verify the data as well as the originating interface info
                var rAddr = new SockAddrCan();
                var readFrame = new CanFdFrame();
                var len = Marshal.SizeOf(typeof(SockAddrCan));
                int nReadBytes = LibcNativeMethods.RecvFrom(sockHandle, ref readFrame, Marshal.SizeOf(typeof(CanFdFrame)), 0, rAddr, ref len);

                Assert.AreEqual(SocketCanConstants.AF_CAN, rAddr.CanFamily);
                Assert.AreEqual(ifr.IfIndex, rAddr.CanIfIndex);
                Assert.AreEqual(72, nReadBytes);
                Assert.AreEqual(0x123, readFrame.CanId);
                Assert.AreEqual(2, readFrame.Length);
                Assert.IsTrue(writeFrame.Data.SequenceEqual(readFrame.Data));
            }
        }

        [Test]
        public void CanFdFrameRead_29BitAddressing_Success_Test()
        {
            var writeFrame = new CanFdFrame(0x17654328 | (uint)CanIdFlags.CAN_EFF_FLAG, new byte[] { 0x66, 0x66, 0x66 }, CanFdFlags.None);
            int nBytes = LibcNativeMethods.Write(socketHandle, ref writeFrame, Marshal.SizeOf(typeof(CanFdFrame)));
            Assert.AreEqual(72, nBytes); 

            var readFrame = new CanFdFrame();
            int nReadBytes = LibcNativeMethods.Read(socketHandle, ref readFrame, Marshal.SizeOf(typeof(CanFdFrame)));
            Assert.AreEqual(72, nReadBytes);
            Assert.AreEqual(0x17654328, readFrame.CanId & (uint)SocketCanConstants.CAN_EFF_MASK);
            Assert.AreEqual(3, readFrame.Length);
            Assert.IsTrue(writeFrame.Data.SequenceEqual(readFrame.Data));
        }

        [Test]
        public void CanFdFrame_RecvMsg_DONTROUTE_CONFIRM_Flags_Success_Test()
        {
            var ifr = new Ifreq("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
            Assert.AreNotEqual(-1, ioctlResult);
            var addr = new SockAddrCan(ifr.IfIndex);

            byte[] data = new byte[64];
            foreach (byte b in Enumerable.Range(0, 64))
            {
                data[b] = b;
            }

            var writeFrame = new CanFdFrame(0x123, data, CanFdFlags.None);
            int nWriteBytes = LibcNativeMethods.Write(socketHandle, ref writeFrame, Marshal.SizeOf(typeof(CanFdFrame)));
            Assert.AreEqual(72, nWriteBytes); 

            int ctrlMsgSize = ControlMessageMacros.CMSG_SPACE(Marshal.SizeOf<Timeval>()) + ControlMessageMacros.CMSG_SPACE(Marshal.SizeOf<UInt32>());
            
            IntPtr addrPtr = Marshal.AllocHGlobal(Marshal.SizeOf<SockAddrCan>());
            IntPtr iovecPtr = Marshal.AllocHGlobal(Marshal.SizeOf<IoVector>());
            IntPtr canFdFramePtr = Marshal.AllocHGlobal(Marshal.SizeOf<CanFdFrame>());
            IntPtr ctrlMsgPtr = Marshal.AllocHGlobal(ctrlMsgSize);
            
            var iovec = new IoVector() { Base = canFdFramePtr, Length = new IntPtr(Marshal.SizeOf<CanFdFrame>()) };
            Marshal.StructureToPtr<SockAddrCan>(addr, addrPtr, false);
            Marshal.StructureToPtr<IoVector>(iovec, iovecPtr, false);
            try
            {
                var readCanMessage = new MessageHeader();
                readCanMessage.Name = addrPtr;
                readCanMessage.NameLength = Marshal.SizeOf<SockAddrCan>();
                readCanMessage.IoVectors = iovecPtr;
                readCanMessage.IoVectorCount = new IntPtr(1);
                readCanMessage.ControlMessage = ctrlMsgPtr;
                readCanMessage.ControlMessageLength = new IntPtr(ctrlMsgSize);
                int nReadBytes = LibcNativeMethods.RecvMsg(socketHandle, ref readCanMessage, MessageFlags.None);

                Assert.AreEqual(72, nReadBytes);
                Assert.AreEqual(8, readCanMessage.NameLength);
                Assert.AreEqual(1, readCanMessage.IoVectorCount.ToInt32());
                Assert.AreEqual(0, readCanMessage.ControlMessageLength.ToInt32());

                SockAddrCan sockAddr = Marshal.PtrToStructure<SockAddrCan>(readCanMessage.Name);
                Assert.AreEqual(SocketCanConstants.AF_CAN, sockAddr.CanFamily);
                Assert.AreEqual(ifr.IfIndex, sockAddr.CanIfIndex);

                IoVector iov = Marshal.PtrToStructure<IoVector>(readCanMessage.IoVectors);
                Assert.AreEqual(72, iov.Length.ToInt32());
                CanFdFrame canFdFrame = Marshal.PtrToStructure<CanFdFrame>(iov.Base);
                Assert.AreEqual(0x123, canFdFrame.CanId);
                Assert.AreEqual(64, canFdFrame.Length);
                Assert.IsTrue(writeFrame.Data.SequenceEqual(canFdFrame.Data));

                Assert.IsTrue(readCanMessage.Flags.HasFlag(MessageFlags.MSG_DONTROUTE));
                Assert.IsTrue(readCanMessage.Flags.HasFlag(MessageFlags.MSG_CONFIRM));
            }
            finally
            {
                Marshal.FreeHGlobal(ctrlMsgPtr);
                Marshal.FreeHGlobal(iovecPtr);
                Marshal.FreeHGlobal(addrPtr);
                Marshal.FreeHGlobal(canFdFramePtr);
            }
        }
    }
}