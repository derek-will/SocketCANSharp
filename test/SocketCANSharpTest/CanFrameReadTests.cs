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
    public class CanFrameReadTests
    {
        SafeFileDescriptorHandle socketHandle;

        [SetUp]
        public void Setup()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            int value = 1;
            int recv_own_msgs_result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_CAN_RAW, CanSocketOptions.CAN_RAW_RECV_OWN_MSGS, ref value, Marshal.SizeOf(1));
            Assert.AreEqual(0, recv_own_msgs_result);
        }

        [TearDown]
        public void Cleanup()
        {
            socketHandle.Close();
        }

        [Test]
        public void CanFrameRead_Success_Test()
        {
            var ifr = new Ifreq("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
            Assert.AreNotEqual(-1, ioctlResult);

            var addr = new SockAddrCan(ifr.IfIndex);

            int bindResult = LibcNativeMethods.Bind(socketHandle, addr, Marshal.SizeOf(typeof(SockAddrCan)));
            Assert.AreNotEqual(-1, bindResult);

            var writeFrame = new CanFrame(0x123, new byte[] { 0x11, 0x22 });

            int nWriteBytes = LibcNativeMethods.Write(socketHandle, ref writeFrame, Marshal.SizeOf(typeof(CanFrame)));
            Assert.AreEqual(16, nWriteBytes); 

            var readFrame = new CanFrame();
            int nReadBytes = LibcNativeMethods.Read(socketHandle, ref readFrame, Marshal.SizeOf(typeof(CanFrame)));
            
            Assert.AreEqual(16, nReadBytes);
            Assert.AreEqual(0x123, readFrame.CanId);
            Assert.AreEqual(2, readFrame.Length);
            Assert.IsTrue(writeFrame.Data.SequenceEqual(readFrame.Data));
        }

        [Test]
        public void CanFrameRead_FromAnyInterface_Success_Test()
        {
            // Set up CAN interface and bind it to 'any' interface (interface index set to 0)
            var ifr = new Ifreq("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
            Assert.AreNotEqual(-1, ioctlResult);

            var anyAddr = new SockAddrCan(0);

            int bindResult = LibcNativeMethods.Bind(socketHandle, anyAddr, Marshal.SizeOf(typeof(SockAddrCan)));
            Assert.AreNotEqual(-1, bindResult);

            // Write to specified outgoing interface
            var writeFrame = new CanFrame(0x123, new byte[] { 0x11, 0x22 });

            var wAddr = new SockAddrCan(ifr.IfIndex);

            int nBytes = LibcNativeMethods.SendTo(socketHandle, ref writeFrame, Marshal.SizeOf(typeof(CanFrame)), 0, wAddr, Marshal.SizeOf(typeof(SockAddrCan)));
            Assert.AreEqual(16, nBytes); 

            // Read and verify the data as well as the originating interface info
            var rAddr = new SockAddrCan();
            var readFrame = new CanFrame();
            var len = Marshal.SizeOf(typeof(SockAddrCan));
            int nReadBytes = LibcNativeMethods.RecvFrom(socketHandle, ref readFrame, Marshal.SizeOf(typeof(CanFrame)), 0, rAddr, ref len);
            
            Assert.AreEqual(SocketCanConstants.AF_CAN, rAddr.CanFamily);
            Assert.AreEqual(ifr.IfIndex, rAddr.CanIfIndex);
            Assert.AreEqual(16, nReadBytes);
            Assert.AreEqual(0x123, readFrame.CanId);
            Assert.AreEqual(2, readFrame.Length);
            Assert.IsTrue(writeFrame.Data.SequenceEqual(readFrame.Data));
        }

        [Test]
        public void CanFrameRead_Timeout_Failure_Test()
        {
            var ifr = new Ifreq("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
            Assert.AreNotEqual(-1, ioctlResult);

            var addr = new SockAddrCan(ifr.IfIndex);

            int bindResult = LibcNativeMethods.Bind(socketHandle, addr, Marshal.SizeOf(typeof(SockAddrCan)));
            Assert.AreNotEqual(-1, bindResult);

            var timeval = new Timeval(0, 1);
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_SOCKET, SocketLevelOptions.SO_RCVTIMEO, timeval, Marshal.SizeOf(typeof(Timeval)));
            Assert.AreEqual(0, result);

            var readFrame = new CanFrame();
            int nReadBytes = LibcNativeMethods.Read(socketHandle, ref readFrame, Marshal.SizeOf(typeof(CanFrame)));
            
            Assert.AreNotEqual(16, nReadBytes);
        }

        [Test]
        public void CanFrameRead_29BitAddressing_Success_Test()
        {
            var ifr = new Ifreq("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
            Assert.AreNotEqual(-1, ioctlResult);

            var addr = new SockAddrCan(ifr.IfIndex);

            int bindResult = LibcNativeMethods.Bind(socketHandle, addr, Marshal.SizeOf(typeof(SockAddrCan)));
            Assert.AreNotEqual(-1, bindResult);

            var writeFrame = new CanFrame(0x12345678 | (uint)CanIdFlags.CAN_EFF_FLAG, new byte[] { 0x55, 0x55 });

            int nWriteBytes = LibcNativeMethods.Write(socketHandle, ref writeFrame, Marshal.SizeOf(typeof(CanFrame)));
            Assert.AreEqual(16, nWriteBytes); 

            var readFrame = new CanFrame();
            int nReadBytes = LibcNativeMethods.Read(socketHandle, ref readFrame, Marshal.SizeOf(typeof(CanFrame)));
            
            Assert.AreEqual(16, nReadBytes);
            Assert.AreEqual(0x12345678, readFrame.CanId & (uint)SocketCanConstants.CAN_EFF_MASK);
            Assert.AreEqual(2, readFrame.Length);
            Assert.IsTrue(writeFrame.Data.SequenceEqual(readFrame.Data));
        }

        [Test]
        public void CanFrameRead_NonBlocking_Success_Test()
        {
            var ifr = new Ifreq("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
            Assert.AreNotEqual(-1, ioctlResult);

            int arg = 1;
            ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.FIONBIO, ref arg);
            Assert.AreNotEqual(-1, ioctlResult);

            var addr = new SockAddrCan(ifr.IfIndex);

            int bindResult = LibcNativeMethods.Bind(socketHandle, addr, Marshal.SizeOf(typeof(SockAddrCan)));
            Assert.AreNotEqual(-1, bindResult);

            var readFrame = new CanFrame();
            int nReadBytes = LibcNativeMethods.Read(socketHandle, ref readFrame, Marshal.SizeOf(typeof(CanFrame)));
            Assert.AreEqual(-1, nReadBytes);
            Assert.AreEqual(11, LibcNativeMethods.Errno); // EAGAIN / EWOULDBLOCK

            var writeFrame = new CanFrame(0x123, new byte[] { 0x11, 0x22 });
            int nWriteBytes = LibcNativeMethods.Write(socketHandle, ref writeFrame, Marshal.SizeOf(typeof(CanFrame)));
            Assert.AreEqual(16, nWriteBytes); 

            readFrame = new CanFrame();
            nReadBytes = LibcNativeMethods.Read(socketHandle, ref readFrame, Marshal.SizeOf(typeof(CanFrame)));
            
            Assert.AreEqual(16, nReadBytes);
            Assert.AreEqual(0x123, readFrame.CanId);
            Assert.AreEqual(2, readFrame.Length);
            Assert.IsTrue(writeFrame.Data.SequenceEqual(readFrame.Data));
        }

        [Test]
        public void CanFrame_RecvMsg_DONTROUTE_CONFIRM_Flags_Success_Test()
        {
            var ifr = new Ifreq("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
            Assert.AreNotEqual(-1, ioctlResult);

            var addr = new SockAddrCan(ifr.IfIndex);

            int bindResult = LibcNativeMethods.Bind(socketHandle, addr, Marshal.SizeOf(typeof(SockAddrCan)));
            Assert.AreNotEqual(-1, bindResult);

            var writeFrame = new CanFrame(0x123, new byte[] { 0x11, 0x22 });

            int nWriteBytes = LibcNativeMethods.Write(socketHandle, ref writeFrame, Marshal.SizeOf(typeof(CanFrame)));
            Assert.AreEqual(16, nWriteBytes);

            int ctrlMsgSize = ControlMessageMacros.CMSG_SPACE(Marshal.SizeOf<Timeval>()) + ControlMessageMacros.CMSG_SPACE(Marshal.SizeOf<UInt32>());
            
            IntPtr addrPtr = Marshal.AllocHGlobal(Marshal.SizeOf<SockAddrCan>());
            IntPtr iovecPtr = Marshal.AllocHGlobal(Marshal.SizeOf<IoVector>());
            IntPtr canFramePtr = Marshal.AllocHGlobal(Marshal.SizeOf<CanFrame>());
            IntPtr ctrlMsgPtr = Marshal.AllocHGlobal(ctrlMsgSize);
            
            var iovec = new IoVector() { Base = canFramePtr, Length = new IntPtr(Marshal.SizeOf<CanFrame>()) };
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

                Assert.AreEqual(16, nReadBytes);
                Assert.AreEqual(8, readCanMessage.NameLength);
                Assert.AreEqual(1, readCanMessage.IoVectorCount.ToInt32());
                Assert.AreEqual(0, readCanMessage.ControlMessageLength.ToInt32());

                SockAddrCan sockAddr = Marshal.PtrToStructure<SockAddrCan>(readCanMessage.Name);
                Assert.AreEqual(SocketCanConstants.AF_CAN, sockAddr.CanFamily);
                Assert.AreEqual(ifr.IfIndex, sockAddr.CanIfIndex);

                IoVector iov = Marshal.PtrToStructure<IoVector>(readCanMessage.IoVectors);
                Assert.AreEqual(16, iov.Length.ToInt32());
                CanFrame canFrame = Marshal.PtrToStructure<CanFrame>(iov.Base);
                Assert.AreEqual(0x123, canFrame.CanId);
                Assert.AreEqual(2, canFrame.Length);
                Assert.IsTrue(writeFrame.Data.SequenceEqual(canFrame.Data));

                Assert.IsTrue(readCanMessage.Flags.HasFlag(MessageFlags.MSG_DONTROUTE));
                Assert.IsTrue(readCanMessage.Flags.HasFlag(MessageFlags.MSG_CONFIRM));
            }
            finally
            {
                Marshal.FreeHGlobal(ctrlMsgPtr);
                Marshal.FreeHGlobal(iovecPtr);
                Marshal.FreeHGlobal(addrPtr);
                Marshal.FreeHGlobal(canFramePtr);
            }
        }
    }
}