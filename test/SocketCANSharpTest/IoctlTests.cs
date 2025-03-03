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
using System.Linq;
using System.Runtime.InteropServices;

namespace SocketCANSharpTest
{
    public class IoctlTests
    {
        [Test]
        public void GetInterfaceIndex_vcan0_Test()
        {
            using (SafeFileDescriptorHandle socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW))
            {
                Assert.IsFalse(socketHandle.IsInvalid);

                var ifr = new Ifreq("vcan0");
                int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
                Assert.AreNotEqual(-1, ioctlResult, $"Errno: {LibcNativeMethods.Errno}");
            }
        }

        [Test]
        public void GetInterfaceIndex_vcan0_SocketClosed_Failure_Test()
        {
            using (SafeFileDescriptorHandle socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW))
            {
                socketHandle.Close();

                var ifr = new Ifreq("vcan0");
                Assert.Throws<ObjectDisposedException>(() => LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFINDEX, ifr));
            }
        }

        [Test]
        public void EnableNonBlockingMode_Success_Test()
        {
            using (SafeFileDescriptorHandle socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW))
            {
                int arg = 1;
                int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.FIONBIO, ref arg);
                Assert.AreNotEqual(-1, ioctlResult);
            }
        }

        [Test]
        public void DisableNonBlockingMode_Success_Test()
        {
            using (SafeFileDescriptorHandle socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW))
            {
                int arg = 0;
                int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.FIONBIO, ref arg);
                Assert.AreNotEqual(-1, ioctlResult);
            }
        }

        [Test]
        public void GetInterfaceIndex_vcan0_MaximumTransmisisonUnit_RAW_Test()
        {
            using (SafeFileDescriptorHandle socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW))
            {
                Assert.IsFalse(socketHandle.IsInvalid);

                var ifr = new IfreqMtu("vcan0");
                int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFMTU, ifr);
                Assert.AreNotEqual(-1, ioctlResult, $"Errno: {LibcNativeMethods.Errno}");
                Assert.That(ifr.MTU, Is.EqualTo(16) | Is.EqualTo(72) | Is.EqualTo(2060)); // some devices may support CAN XL, CAN FD while other may support Classic CAN only
            }
        }

        [Test]
        public void GetInterfaceIndex_vcan0_MaximumTransmisisonUnit_ISOTP_Test()
        {
            using (SafeFileDescriptorHandle socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_ISOTP))
            {
                if (socketHandle.IsInvalid)
                {
                    Assume.That(LibcNativeMethods.Errno, Is.Not.EqualTo(93) & Is.Not.EqualTo(22)); // If EPROTONOSUPPORT, then this protocol is not supported on this platform and no futher testing applies. If EINVAL, then Protocol Type is not being recognized as valid.
                }
                Assert.IsFalse(socketHandle.IsInvalid);

                var ifr = new IfreqMtu("vcan0");
                int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFMTU, ifr);
                Assert.AreNotEqual(-1, ioctlResult, $"Errno: {LibcNativeMethods.Errno}");
                Assert.That(ifr.MTU, Is.EqualTo(16) | Is.EqualTo(72) | Is.EqualTo(2060)); // some devices may support CAN XL, CAN FD while other may support Classic CAN only
            }
        }

        [Test]
        public void GetInterfaceName_vcan0_Test()
        {
            using (SafeFileDescriptorHandle socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW))
            {
                Assert.IsFalse(socketHandle.IsInvalid);

                var ifr = new Ifreq("vcan0");
                int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
                Assert.AreNotEqual(-1, ioctlResult, $"Errno: {LibcNativeMethods.Errno}");

                var ifreq = new Ifreq(ifr.IfIndex);
                ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFNAME, ifreq);
                Assert.AreNotEqual(-1, ioctlResult, $"Errno: {LibcNativeMethods.Errno}");
                Assert.AreEqual("vcan0", ifreq.Name);
            }
        }

        [Test]
        public void GetTimestamp_Success()
        {
            using (var socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW))
            {
                Assert.IsFalse(socketHandle.IsInvalid);

                int value = 1;
                int recv_own_msgs_result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_CAN_RAW, CanSocketOptions.CAN_RAW_RECV_OWN_MSGS, ref value, Marshal.SizeOf(1));
                Assert.AreEqual(0, recv_own_msgs_result);

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

                var timeval = new Timeval();
                int timestampResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGSTAMP, timeval);
                Assert.AreNotEqual(-1, timestampResult, $"Errno: {LibcNativeMethods.Errno}");
                Assert.AreNotEqual(0, timeval.Seconds + timeval.Microseconds);
            }
        }

        [Test]
        public void GetTimestamp_No_Read_Failure()
        {
            using (var socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW))
            {
                Assert.IsFalse(socketHandle.IsInvalid);

                int value = 1;
                int recv_own_msgs_result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_CAN_RAW, CanSocketOptions.CAN_RAW_RECV_OWN_MSGS, ref value, Marshal.SizeOf(1));
                Assert.AreEqual(0, recv_own_msgs_result);

                var ifr = new Ifreq("vcan0");
                int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
                Assert.AreNotEqual(-1, ioctlResult);

                var addr = new SockAddrCan(ifr.IfIndex);

                int bindResult = LibcNativeMethods.Bind(socketHandle, addr, Marshal.SizeOf(typeof(SockAddrCan)));
                Assert.AreNotEqual(-1, bindResult);

                var writeFrame = new CanFrame(0x123, new byte[] { 0x11, 0x22 });

                int nWriteBytes = LibcNativeMethods.Write(socketHandle, ref writeFrame, Marshal.SizeOf(typeof(CanFrame)));
                Assert.AreEqual(16, nWriteBytes);

                var timeval = new Timeval();
                int timestampResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGSTAMP, timeval);
                Assert.AreEqual(-1, timestampResult);
                Assert.AreEqual(2, LibcNativeMethods.Errno); // ENOENT
            }
        }
    }
}