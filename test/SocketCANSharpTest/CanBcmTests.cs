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
    public class CanBcmTests
    {
        SafeFileDescriptorHandle socketHandle;

        [SetUp]
        public void Setup()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_BCM);
            Assert.IsFalse(socketHandle.IsInvalid);

            var ifr = new Ifreq("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
            Assert.AreNotEqual(-1, ioctlResult);

            var addr = new SockAddrCan(ifr.IfIndex);

            int connectResult = LibcNativeMethods.Connect(socketHandle, addr, Marshal.SizeOf(typeof(SockAddrCan)));
            Assert.AreNotEqual(-1, connectResult);

            var timeval = new Timeval(1, 0);
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_SOCKET, SocketLevelOptions.SO_RCVTIMEO, timeval, Marshal.SizeOf(typeof(Timeval)));
            Assert.AreEqual(0, result);
        }

        [TearDown]
        public void Cleanup()
        {
            socketHandle.Close();
        }

        [Test]
        public void CAN_BCM_TX_SETUP_Success_Test()
        {
            if (Environment.Is64BitProcess)
            {
                var canFrame = new CanFrame(0x333, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF });
                var header = new BcmMessageHeader(BcmOpcode.TX_SETUP)
                {
                    Flags = BcmFlags.SETTIMER | BcmFlags.STARTTIMER,
                    Interval2 = new BcmTimeval(1, 0),
                    NumberOfFrames = 1,
                };

                var bcmMessage = new BcmCanMessage(header, new CanFrame[] { canFrame });

                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(72, nBytes);
            }
            else
            {
                var canFrame = new CanFrame(0x333, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF });
                var header = new BcmMessageHeader32(BcmOpcode.TX_SETUP)
                {
                    Flags = BcmFlags.SETTIMER | BcmFlags.STARTTIMER,
                    Interval2 = new BcmTimeval(1, 0),
                    NumberOfFrames = 1,
                };

                var bcmMessage = new BcmCanMessage32(header, new CanFrame[] { canFrame });

                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(56, nBytes);
            }
        }

        [Test]
        public void CAN_BCM_TX_SETUP_CANFD_Success_Test()
        {
            var ifrMtu = new IfreqMtu("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFMTU, ifrMtu);
            Assert.AreNotEqual(-1, ioctlResult, $"Errno: {LibcNativeMethods.Errno}");
            Assume.That(ifrMtu.MTU, Is.EqualTo(SocketCanConstants.CANFD_MTU));

            if (Environment.Is64BitProcess)
            {
                var canFrame = new CanFdFrame(0x311, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF }, CanFdFlags.CANFD_BRS);
                var header = new BcmMessageHeader(BcmOpcode.TX_SETUP)
                {
                    Flags = BcmFlags.SETTIMER | BcmFlags.STARTTIMER,
                    Interval2 = new BcmTimeval(1, 0),
                    NumberOfFrames = 1,
                };

                var bcmMessage = new BcmCanFdMessage(header, new CanFdFrame[] { canFrame });
                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(128, nBytes);
            }
            else
            {
                var canFrame = new CanFdFrame(0x311, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF }, CanFdFlags.CANFD_BRS);
                var header = new BcmMessageHeader32(BcmOpcode.TX_SETUP)
                {
                    Flags = BcmFlags.SETTIMER | BcmFlags.STARTTIMER,
                    Interval2 = new BcmTimeval(1, 0),
                    NumberOfFrames = 1,
                };

                var bcmMessage = new BcmCanFdMessage32(header, new CanFdFrame[] { canFrame });
                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(112, nBytes);
            }
        }

        [Test]
        public void CAN_BCM_TX_SETUP_29BitAddressing_Success_Test()
        {
            if (Environment.Is64BitProcess)
            {
                var canFrame = new CanFrame(0x13333333 | (uint)CanIdFlags.CAN_EFF_FLAG, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF });
                var header = new BcmMessageHeader(BcmOpcode.TX_SETUP)
                {
                    Flags = BcmFlags.SETTIMER | BcmFlags.STARTTIMER,
                    Interval2 = new BcmTimeval(1, 0),
                    NumberOfFrames = 1,
                };

                var bcmMessage = new BcmCanMessage(header, new CanFrame[] { canFrame });

                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(72, nBytes);
            }
            else
            {
                var canFrame = new CanFrame(0x13333333 | (uint)CanIdFlags.CAN_EFF_FLAG, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF });
                var header = new BcmMessageHeader32(BcmOpcode.TX_SETUP)
                {
                    Flags = BcmFlags.SETTIMER | BcmFlags.STARTTIMER,
                    Interval2 = new BcmTimeval(1, 0),
                    NumberOfFrames = 1,
                };

                var bcmMessage = new BcmCanMessage32(header, new CanFrame[] { canFrame });

                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(56, nBytes);
            }
        }

        [Test]
        public void CAN_BCM_TX_SETUP_With_ExpireEvent_Success_Test()
        {
            if (Environment.Is64BitProcess)
            {
                var timeval = new Timeval(3, 0);
                int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_SOCKET, SocketLevelOptions.SO_RCVTIMEO, timeval, Marshal.SizeOf(typeof(Timeval)));
                Assert.AreEqual(0, result);

                var canFrame = new CanFrame(0x333, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF });
                var header = new BcmMessageHeader(BcmOpcode.TX_SETUP)
                {
                    Flags = BcmFlags.SETTIMER | BcmFlags.STARTTIMER | BcmFlags.TX_COUNTEVT,
                    Interval1 = new BcmTimeval(1, 0),
                    Interval2 = new BcmTimeval(2, 0),
                    Interval1Count = 3,
                    NumberOfFrames = 1,
                };

                var bcmMessage = new BcmCanMessage(header, new CanFrame[] { canFrame });

                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(72, nBytes);

                var receiveMessage = new BcmCanMessage();
                nBytes = LibcNativeMethods.Read(socketHandle, receiveMessage, Marshal.SizeOf(typeof(BcmCanMessage)));
                Assert.AreEqual(BcmOpcode.TX_EXPIRED, receiveMessage.Header.Opcode);
                Assert.AreEqual(56, nBytes);
            }
            else
            {
                var timeval = new Timeval(3, 0);
                int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_SOCKET, SocketLevelOptions.SO_RCVTIMEO, timeval, Marshal.SizeOf(typeof(Timeval)));
                Assert.AreEqual(0, result);

                var canFrame = new CanFrame(0x333, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF });
                var header = new BcmMessageHeader32(BcmOpcode.TX_SETUP)
                {
                    Flags = BcmFlags.SETTIMER | BcmFlags.STARTTIMER | BcmFlags.TX_COUNTEVT,
                    Interval1 = new BcmTimeval(1, 0),
                    Interval2 = new BcmTimeval(2, 0),
                    Interval1Count = 3,
                    NumberOfFrames = 1,
                };

                var bcmMessage = new BcmCanMessage32(header, new CanFrame[] { canFrame });

                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(56, nBytes);

                var receiveMessage = new BcmCanMessage32();
                nBytes = LibcNativeMethods.Read(socketHandle, receiveMessage, Marshal.SizeOf(typeof(BcmCanMessage32)));
                Assert.AreEqual(BcmOpcode.TX_EXPIRED, receiveMessage.Header.Opcode);
                Assert.AreEqual(40, nBytes);
            }
        }

        [Test]
        public void CAN_BCM_TX_SETUP_With_ExpireEvent_CANFD_Success_Test()
        {
            var ifrMtu = new IfreqMtu("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFMTU, ifrMtu);
            Assert.AreNotEqual(-1, ioctlResult, $"Errno: {LibcNativeMethods.Errno}");
            Assume.That(ifrMtu.MTU, Is.EqualTo(SocketCanConstants.CANFD_MTU));

            if (Environment.Is64BitProcess)
            {
                var timeval = new Timeval(3, 0);
                int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_SOCKET, SocketLevelOptions.SO_RCVTIMEO, timeval, Marshal.SizeOf(typeof(Timeval)));
                Assert.AreEqual(0, result);

                var canFrame = new CanFdFrame(0x311, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF }, CanFdFlags.CANFD_BRS);
                var header = new BcmMessageHeader(BcmOpcode.TX_SETUP)
                {
                    Flags = BcmFlags.SETTIMER | BcmFlags.STARTTIMER | BcmFlags.TX_COUNTEVT,
                    Interval1 = new BcmTimeval(1, 0),
                    Interval2 = new BcmTimeval(2, 0),
                    Interval1Count = 3,
                    NumberOfFrames = 1,
                };

                var bcmMessage = new BcmCanFdMessage(header, new CanFdFrame[] { canFrame });

                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(128, nBytes);

                var receiveMessage = new BcmCanFdMessage();
                nBytes = LibcNativeMethods.Read(socketHandle, receiveMessage, Marshal.SizeOf(typeof(BcmCanFdMessage)));
                Assert.AreEqual(BcmOpcode.TX_EXPIRED, receiveMessage.Header.Opcode);
                Assert.AreEqual(56, nBytes);
            }
            else
            {
                var timeval = new Timeval(3, 0);
                int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_SOCKET, SocketLevelOptions.SO_RCVTIMEO, timeval, Marshal.SizeOf(typeof(Timeval)));
                Assert.AreEqual(0, result);

                var canFrame = new CanFdFrame(0x311, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF }, CanFdFlags.CANFD_BRS);
                var header = new BcmMessageHeader32(BcmOpcode.TX_SETUP)
                {
                    Flags = BcmFlags.SETTIMER | BcmFlags.STARTTIMER | BcmFlags.TX_COUNTEVT,
                    Interval1 = new BcmTimeval(1, 0),
                    Interval2 = new BcmTimeval(2, 0),
                    Interval1Count = 3,
                    NumberOfFrames = 1,
                };

                var bcmMessage = new BcmCanFdMessage32(header, new CanFdFrame[] { canFrame });

                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(112, nBytes);

                var receiveMessage = new BcmCanFdMessage32();
                nBytes = LibcNativeMethods.Read(socketHandle, receiveMessage, Marshal.SizeOf(typeof(BcmCanFdMessage32)));
                Assert.AreEqual(BcmOpcode.TX_EXPIRED, receiveMessage.Header.Opcode);
                Assert.AreEqual(40, nBytes);
            }
        }

        [Test]
        public void CAN_BCM_TX_DELETE_Success_Test()
        {
            if (Environment.Is64BitProcess)
            {
                var canFrame = new CanFrame(0x333, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF });
                var header = new BcmMessageHeader(BcmOpcode.TX_SETUP)
                {
                    Flags = BcmFlags.SETTIMER | BcmFlags.STARTTIMER,
                    Interval2 = new BcmTimeval(1, 0),
                    NumberOfFrames = 1,
                };

                var bcmMessage = new BcmCanMessage(header, new CanFrame[] { canFrame });

                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(72, nBytes);

                var deleteHeader = new BcmMessageHeader(BcmOpcode.TX_DELETE)
                {
                    NumberOfFrames = 1,
                };

                nBytes = LibcNativeMethods.Write(socketHandle, deleteHeader, Marshal.SizeOf(typeof(BcmMessageHeader)));
                Assert.AreEqual(56, nBytes);
            }
            else
            {
                var canFrame = new CanFrame(0x333, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF });
                var header = new BcmMessageHeader32(BcmOpcode.TX_SETUP)
                {
                    Flags = BcmFlags.SETTIMER | BcmFlags.STARTTIMER,
                    Interval2 = new BcmTimeval(1, 0),
                    NumberOfFrames = 1,
                };

                var bcmMessage = new BcmCanMessage32(header, new CanFrame[] { canFrame });

                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(56, nBytes);

                var deleteHeader = new BcmMessageHeader32(BcmOpcode.TX_DELETE)
                {
                    NumberOfFrames = 1,
                };

                nBytes = LibcNativeMethods.Write(socketHandle, deleteHeader, Marshal.SizeOf(typeof(BcmMessageHeader32)));
                Assert.AreEqual(40, nBytes);
            }
        }

        [Test]
        public void CAN_BCM_TX_DELETE_CANFD_Success_Test()
        {
            var ifrMtu = new IfreqMtu("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFMTU, ifrMtu);
            Assert.AreNotEqual(-1, ioctlResult, $"Errno: {LibcNativeMethods.Errno}");
            Assume.That(ifrMtu.MTU, Is.EqualTo(SocketCanConstants.CANFD_MTU));

            if (Environment.Is64BitProcess)
            {
                var canFrame = new CanFdFrame(0x111, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF }, CanFdFlags.CANFD_BRS);
                var header = new BcmMessageHeader(BcmOpcode.TX_SETUP)
                {
                    Flags = BcmFlags.SETTIMER | BcmFlags.STARTTIMER,
                    Interval2 = new BcmTimeval(1, 0),
                    NumberOfFrames = 1,
                };

                var bcmMessage = new BcmCanFdMessage(header, new CanFdFrame[] { canFrame });
                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(128, nBytes);

                var deleteHeader = new BcmMessageHeader(BcmOpcode.TX_DELETE)
                {
                    NumberOfFrames = 1,
                    Flags = BcmFlags.CAN_FD_FRAME,
                };

                nBytes = LibcNativeMethods.Write(socketHandle, deleteHeader, Marshal.SizeOf(typeof(BcmMessageHeader)));
                Assert.AreEqual(56, nBytes);
            }
            else
            {
                var canFrame = new CanFdFrame(0x111, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF }, CanFdFlags.CANFD_BRS);
                var header = new BcmMessageHeader32(BcmOpcode.TX_SETUP)
                {
                    Flags = BcmFlags.SETTIMER | BcmFlags.STARTTIMER,
                    Interval2 = new BcmTimeval(1, 0),
                    NumberOfFrames = 1,
                };

                var bcmMessage = new BcmCanFdMessage32(header, new CanFdFrame[] { canFrame });
                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(112, nBytes);

                var deleteHeader = new BcmMessageHeader32(BcmOpcode.TX_DELETE)
                {
                    NumberOfFrames = 1,
                    Flags = BcmFlags.CAN_FD_FRAME,
                };

                nBytes = LibcNativeMethods.Write(socketHandle, deleteHeader, Marshal.SizeOf(typeof(BcmMessageHeader32)));
                Assert.AreEqual(40, nBytes);
            }
        }

        [Test]
        public void CAN_BCM_TX_READ_Success_Test()
        {
            if (Environment.Is64BitProcess)
            {
                var canFrame = new CanFrame(0x333, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF });
                var header = new BcmMessageHeader(BcmOpcode.TX_SETUP)
                {
                    Flags = BcmFlags.SETTIMER | BcmFlags.STARTTIMER,
                    Interval2 = new BcmTimeval(1, 0),
                    NumberOfFrames = 1,
                };

                var bcmMessage = new BcmCanMessage(header, new CanFrame[] { canFrame });

                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(72, nBytes);

                var readHeader = new BcmMessageHeader(BcmOpcode.TX_READ);

                nBytes = LibcNativeMethods.Write(socketHandle, readHeader, Marshal.SizeOf(typeof(BcmMessageHeader)));
                Assert.AreEqual(56, nBytes);
                Assert.AreEqual(BcmOpcode.TX_READ, readHeader.Opcode);

                var receiveMessage = new BcmCanMessage();
                nBytes = LibcNativeMethods.Read(socketHandle, receiveMessage, Marshal.SizeOf(typeof(BcmCanMessage)));
                Assert.AreEqual(BcmOpcode.TX_STATUS, receiveMessage.Header.Opcode);
                Assert.AreEqual(72, nBytes);
            }
            else
            {
                var canFrame = new CanFrame(0x333, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF });
                var header = new BcmMessageHeader32(BcmOpcode.TX_SETUP)
                {
                    Flags = BcmFlags.SETTIMER | BcmFlags.STARTTIMER,
                    Interval2 = new BcmTimeval(1, 0),
                    NumberOfFrames = 1,
                };

                var bcmMessage = new BcmCanMessage32(header, new CanFrame[] { canFrame });

                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(56, nBytes);

                var readHeader = new BcmMessageHeader32(BcmOpcode.TX_READ);

                nBytes = LibcNativeMethods.Write(socketHandle, readHeader, Marshal.SizeOf(typeof(BcmMessageHeader32)));
                Assert.AreEqual(40, nBytes);
                Assert.AreEqual(BcmOpcode.TX_READ, readHeader.Opcode);

                var receiveMessage = new BcmCanMessage32();
                nBytes = LibcNativeMethods.Read(socketHandle, receiveMessage, Marshal.SizeOf(typeof(BcmCanMessage32)));
                Assert.AreEqual(BcmOpcode.TX_STATUS, receiveMessage.Header.Opcode);
                Assert.AreEqual(56, nBytes);
            }
        }

        [Test]
        public void CAN_BCM_TX_READ_Generic_Success_Test()
        {
            if (Environment.Is64BitProcess)
            {
                var canFrame = new CanFrame(0x333, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF });
                var header = new BcmMessageHeader(BcmOpcode.TX_SETUP)
                {
                    Flags = BcmFlags.SETTIMER | BcmFlags.STARTTIMER,
                    Interval2 = new BcmTimeval(1, 0),
                    NumberOfFrames = 1,
                };

                var bcmMessage = new BcmCanMessage(header, new CanFrame[] { canFrame });

                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(72, nBytes);

                var readHeader = new BcmMessageHeader(BcmOpcode.TX_READ);

                nBytes = LibcNativeMethods.Write(socketHandle, readHeader, Marshal.SizeOf(typeof(BcmMessageHeader)));
                Assert.AreEqual(56, nBytes);
                Assert.AreEqual(BcmOpcode.TX_READ, readHeader.Opcode);

                var receiveMessage = new BcmGenericMessage();
                nBytes = LibcNativeMethods.Read(socketHandle, receiveMessage, Marshal.SizeOf(typeof(BcmGenericMessage)));
                Assert.AreEqual(BcmOpcode.TX_STATUS, receiveMessage.Header.Opcode);
                Assert.AreEqual(72, nBytes);
            }
            else
            {
                var canFrame = new CanFrame(0x333, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF });
                var header = new BcmMessageHeader32(BcmOpcode.TX_SETUP)
                {
                    Flags = BcmFlags.SETTIMER | BcmFlags.STARTTIMER,
                    Interval2 = new BcmTimeval(1, 0),
                    NumberOfFrames = 1,
                };

                var bcmMessage = new BcmCanMessage32(header, new CanFrame[] { canFrame });

                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(56, nBytes);

                var readHeader = new BcmMessageHeader32(BcmOpcode.TX_READ);

                nBytes = LibcNativeMethods.Write(socketHandle, readHeader, Marshal.SizeOf(typeof(BcmMessageHeader32)));
                Assert.AreEqual(40, nBytes);
                Assert.AreEqual(BcmOpcode.TX_READ, readHeader.Opcode);

                var receiveMessage = new BcmGenericMessage32();
                nBytes = LibcNativeMethods.Read(socketHandle, receiveMessage, Marshal.SizeOf(typeof(BcmGenericMessage32)));
                Assert.AreEqual(BcmOpcode.TX_STATUS, receiveMessage.Header.Opcode);
                Assert.AreEqual(56, nBytes);
            }
        }

        [Test]
        public void CAN_BCM_TX_READ_CANFD_Success_Test()
        {
            var ifrMtu = new IfreqMtu("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFMTU, ifrMtu);
            Assert.AreNotEqual(-1, ioctlResult, $"Errno: {LibcNativeMethods.Errno}");
            Assume.That(ifrMtu.MTU, Is.EqualTo(SocketCanConstants.CANFD_MTU));

            if (Environment.Is64BitProcess)
            {
                var canFrame = new CanFdFrame(0x111, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF }, CanFdFlags.CANFD_BRS);
                var header = new BcmMessageHeader(BcmOpcode.TX_SETUP)
                {
                    Flags = BcmFlags.SETTIMER | BcmFlags.STARTTIMER,
                    Interval2 = new BcmTimeval(1, 0),
                    NumberOfFrames = 1,
                };

                var bcmMessage = new BcmCanFdMessage(header, new CanFdFrame[] { canFrame });
                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(128, nBytes);

                var readHeader = new BcmMessageHeader(BcmOpcode.TX_READ);
                readHeader.Flags = BcmFlags.CAN_FD_FRAME;
                nBytes = LibcNativeMethods.Write(socketHandle, readHeader, Marshal.SizeOf(typeof(BcmMessageHeader)));
                Assert.AreEqual(56, nBytes);
                Assert.AreEqual(BcmOpcode.TX_READ, readHeader.Opcode);

                var receiveMessage = new BcmCanFdMessage();
                nBytes = LibcNativeMethods.Read(socketHandle, receiveMessage, Marshal.SizeOf(typeof(BcmCanFdMessage)));
                Assert.AreEqual(BcmOpcode.TX_STATUS, receiveMessage.Header.Opcode);
                Assert.AreEqual(128, nBytes);
            }
            else
            {
                var canFrame = new CanFdFrame(0x111, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF }, CanFdFlags.CANFD_BRS);
                var header = new BcmMessageHeader32(BcmOpcode.TX_SETUP)
                {
                    Flags = BcmFlags.SETTIMER | BcmFlags.STARTTIMER,
                    Interval2 = new BcmTimeval(1, 0),
                    NumberOfFrames = 1,
                };

                var bcmMessage = new BcmCanFdMessage32(header, new CanFdFrame[] { canFrame });
                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(112, nBytes);

                var readHeader = new BcmMessageHeader32(BcmOpcode.TX_READ);
                readHeader.Flags = BcmFlags.CAN_FD_FRAME;
                nBytes = LibcNativeMethods.Write(socketHandle, readHeader, Marshal.SizeOf(typeof(BcmMessageHeader32)));
                Assert.AreEqual(40, nBytes);
                Assert.AreEqual(BcmOpcode.TX_READ, readHeader.Opcode);

                var receiveMessage = new BcmCanFdMessage32();
                nBytes = LibcNativeMethods.Read(socketHandle, receiveMessage, Marshal.SizeOf(typeof(BcmCanFdMessage32)));
                Assert.AreEqual(BcmOpcode.TX_STATUS, receiveMessage.Header.Opcode);
                Assert.AreEqual(112, nBytes);
            }
        }

        [Test]
        public void CAN_BCM_TX_READ_CANFD_Generic_Success_Test()
        {
            var ifrMtu = new IfreqMtu("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFMTU, ifrMtu);
            Assert.AreNotEqual(-1, ioctlResult, $"Errno: {LibcNativeMethods.Errno}");
            Assume.That(ifrMtu.MTU, Is.EqualTo(SocketCanConstants.CANFD_MTU));

            if (Environment.Is64BitProcess)
            {
                var canFrame = new CanFdFrame(0x111, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF }, CanFdFlags.CANFD_BRS);
                var header = new BcmMessageHeader(BcmOpcode.TX_SETUP)
                {
                    Flags = BcmFlags.SETTIMER | BcmFlags.STARTTIMER,
                    Interval2 = new BcmTimeval(1, 0),
                    NumberOfFrames = 1,
                };

                var bcmMessage = new BcmCanFdMessage(header, new CanFdFrame[] { canFrame });
                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(128, nBytes);

                var readHeader = new BcmMessageHeader(BcmOpcode.TX_READ);
                readHeader.Flags = BcmFlags.CAN_FD_FRAME;
                nBytes = LibcNativeMethods.Write(socketHandle, readHeader, Marshal.SizeOf(typeof(BcmMessageHeader)));
                Assert.AreEqual(56, nBytes);
                Assert.AreEqual(BcmOpcode.TX_READ, readHeader.Opcode);

                var receiveMessage = new BcmGenericMessage();
                nBytes = LibcNativeMethods.Read(socketHandle, receiveMessage, Marshal.SizeOf(typeof(BcmGenericMessage)));
                Assert.AreEqual(BcmOpcode.TX_STATUS, receiveMessage.Header.Opcode);
                Assert.AreEqual(128, nBytes);
            }
            else
            {
                var canFrame = new CanFdFrame(0x111, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF }, CanFdFlags.CANFD_BRS);
                var header = new BcmMessageHeader32(BcmOpcode.TX_SETUP)
                {
                    Flags = BcmFlags.SETTIMER | BcmFlags.STARTTIMER,
                    Interval2 = new BcmTimeval(1, 0),
                    NumberOfFrames = 1,
                };

                var bcmMessage = new BcmCanFdMessage32(header, new CanFdFrame[] { canFrame });
                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(112, nBytes);

                var readHeader = new BcmMessageHeader32(BcmOpcode.TX_READ);
                readHeader.Flags = BcmFlags.CAN_FD_FRAME;
                nBytes = LibcNativeMethods.Write(socketHandle, readHeader, Marshal.SizeOf(typeof(BcmMessageHeader32)));
                Assert.AreEqual(40, nBytes);
                Assert.AreEqual(BcmOpcode.TX_READ, readHeader.Opcode);

                var receiveMessage = new BcmGenericMessage32();
                nBytes = LibcNativeMethods.Read(socketHandle, receiveMessage, Marshal.SizeOf(typeof(BcmGenericMessage32)));
                Assert.AreEqual(BcmOpcode.TX_STATUS, receiveMessage.Header.Opcode);
                Assert.AreEqual(112, nBytes);
            }
        }

        [Test]
        public void CAN_BCM_TX_READ_29BitAddressing_Success_Test()
        {
            if (Environment.Is64BitProcess)
            {
                var canFrame = new CanFrame(0x13333333 | (uint)CanIdFlags.CAN_EFF_FLAG, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF });
                var header = new BcmMessageHeader(BcmOpcode.TX_SETUP)
                {
                    Flags = BcmFlags.SETTIMER | BcmFlags.STARTTIMER,
                    Interval2 = new BcmTimeval(1, 0),
                    NumberOfFrames = 1,
                };

                var bcmMessage = new BcmCanMessage(header, new CanFrame[] { canFrame });
                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(72, nBytes);

                var readHeader = new BcmMessageHeader(BcmOpcode.TX_READ);
                nBytes = LibcNativeMethods.Write(socketHandle, readHeader, Marshal.SizeOf(typeof(BcmMessageHeader)));
                Assert.AreEqual(56, nBytes);
                Assert.AreEqual(BcmOpcode.TX_READ, readHeader.Opcode);

                var receiveMessage = new BcmCanMessage();
                nBytes = LibcNativeMethods.Read(socketHandle, receiveMessage, Marshal.SizeOf(typeof(BcmCanMessage)));
                Assert.AreEqual(BcmOpcode.TX_STATUS, receiveMessage.Header.Opcode);
                Assert.AreEqual(72, nBytes);
                Assert.AreEqual(0x13333333, receiveMessage.Frames.First().CanId & SocketCanConstants.CAN_EFF_MASK);
            }
            else
            {
                var canFrame = new CanFrame(0x13333333 | (uint)CanIdFlags.CAN_EFF_FLAG, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF });
                var header = new BcmMessageHeader32(BcmOpcode.TX_SETUP)
                {
                    Flags = BcmFlags.SETTIMER | BcmFlags.STARTTIMER,
                    Interval2 = new BcmTimeval(1, 0),
                    NumberOfFrames = 1,
                };

                var bcmMessage = new BcmCanMessage32(header, new CanFrame[] { canFrame });
                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(56, nBytes);

                var readHeader = new BcmMessageHeader32(BcmOpcode.TX_READ);
                nBytes = LibcNativeMethods.Write(socketHandle, readHeader, Marshal.SizeOf(typeof(BcmMessageHeader32)));
                Assert.AreEqual(40, nBytes);
                Assert.AreEqual(BcmOpcode.TX_READ, readHeader.Opcode);

                var receiveMessage = new BcmCanMessage32();
                nBytes = LibcNativeMethods.Read(socketHandle, receiveMessage, Marshal.SizeOf(typeof(BcmCanMessage32)));
                Assert.AreEqual(BcmOpcode.TX_STATUS, receiveMessage.Header.Opcode);
                Assert.AreEqual(56, nBytes);
                Assert.AreEqual(0x13333333, receiveMessage.Frames.First().CanId & SocketCanConstants.CAN_EFF_MASK);
            }
        }

        [Test]
        public void CAN_BCM_TX_SEND_Success_Test()
        {
            if (Environment.Is64BitProcess)
            {
                var canFrame = new CanFrame(0x333, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF });
                var header = new BcmMessageHeader(BcmOpcode.TX_SEND)
                {
                    Flags = BcmFlags.None,
                    NumberOfFrames = 1,
                };

                var bcmMessage = new BcmCanSingleMessage(header, canFrame);

                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(72, nBytes);
            }
            else
            {
                var canFrame = new CanFrame(0x333, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF });
                var header = new BcmMessageHeader32(BcmOpcode.TX_SEND)
                {
                    Flags = BcmFlags.None,
                    NumberOfFrames = 1,
                };

                var bcmMessage = new BcmCanSingleMessage32(header, canFrame);

                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(56, nBytes);
            }
        }

        [Test]
        public void CAN_BCM_TX_SEND_CANFD_Success_Test()
        {
            var ifrMtu = new IfreqMtu("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFMTU, ifrMtu);
            Assert.AreNotEqual(-1, ioctlResult, $"Errno: {LibcNativeMethods.Errno}");
            Assume.That(ifrMtu.MTU, Is.EqualTo(SocketCanConstants.CANFD_MTU));

            if (Environment.Is64BitProcess)
            {
                var canFrame = new CanFdFrame(0x31, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF }, CanFdFlags.CANFD_BRS);
                var header = new BcmMessageHeader(BcmOpcode.TX_SEND)
                {
                    Flags = BcmFlags.CAN_FD_FRAME,
                    NumberOfFrames = 1,
                };

                var bcmMessage = new BcmCanFdSingleMessage(header, canFrame);
                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(128, nBytes);
            }
            else
            {
                var canFrame = new CanFdFrame(0x31, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF }, CanFdFlags.CANFD_BRS);
                var header = new BcmMessageHeader32(BcmOpcode.TX_SEND)
                {
                    Flags = BcmFlags.CAN_FD_FRAME,
                    NumberOfFrames = 1,
                };

                var bcmMessage = new BcmCanFdSingleMessage32(header, canFrame);
                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(112, nBytes);
            }
        }

        [Test]
        public void CAN_BCM_TX_SEND_29BitAddressing_Success_Test()
        {
            if (Environment.Is64BitProcess)
            {
                var canFrame = new CanFrame(0x13333333 | (uint)CanIdFlags.CAN_EFF_FLAG, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF });
                var header = new BcmMessageHeader(BcmOpcode.TX_SEND)
                {
                    Flags = BcmFlags.None,
                    NumberOfFrames = 1,
                };

                var bcmMessage = new BcmCanSingleMessage(header, canFrame);

                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(72, nBytes);
            }
            else
            {
                var canFrame = new CanFrame(0x13333333 | (uint)CanIdFlags.CAN_EFF_FLAG, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF });
                var header = new BcmMessageHeader32(BcmOpcode.TX_SEND)
                {
                    Flags = BcmFlags.None,
                    NumberOfFrames = 1,
                };

                var bcmMessage = new BcmCanSingleMessage32(header, canFrame);

                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(56, nBytes);
            }
        }

        [Test]
        public void CAN_BCM_RX_SETUP_Success_Test()
        {
            if (Environment.Is64BitProcess)
            {
                var header = new BcmMessageHeader(BcmOpcode.RX_SETUP)
                {
                    Flags = BcmFlags.RX_FILTER_ID,
                    CanId = 0x123,
                    NumberOfFrames = 0,
                };

                var bcmMessage = new BcmCanMessage(header);
                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(56, nBytes);
            }
            else
            {
                var header = new BcmMessageHeader32(BcmOpcode.RX_SETUP)
                {
                    Flags = BcmFlags.RX_FILTER_ID,
                    CanId = 0x123,
                    NumberOfFrames = 0,
                };

                var bcmMessage = new BcmCanMessage32(header);
                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(40, nBytes);
            }
        }

        [Test]
        public void CAN_BCM_RX_SETUP_CANFD_Success_Test()
        {
            var ifrMtu = new IfreqMtu("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFMTU, ifrMtu);
            Assert.AreNotEqual(-1, ioctlResult, $"Errno: {LibcNativeMethods.Errno}");
            Assume.That(ifrMtu.MTU, Is.EqualTo(SocketCanConstants.CANFD_MTU));

            if (Environment.Is64BitProcess)
            {
                var header = new BcmMessageHeader(BcmOpcode.RX_SETUP)
                {
                    Flags = BcmFlags.RX_FILTER_ID | BcmFlags.CAN_FD_FRAME,
                    CanId = 0x123,
                    NumberOfFrames = 0,
                };

                var bcmMessage = new BcmCanFdMessage(header);
                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(56, nBytes);
            }
            else
            {
                var header = new BcmMessageHeader32(BcmOpcode.RX_SETUP)
                {
                    Flags = BcmFlags.RX_FILTER_ID | BcmFlags.CAN_FD_FRAME,
                    CanId = 0x123,
                    NumberOfFrames = 0,
                };

                var bcmMessage = new BcmCanFdMessage32(header);
                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(40, nBytes);
            }
        }

        [Test]
        public void CAN_BCM_RX_SETUP_29BitAddressing_Success_Test()
        {
            if (Environment.Is64BitProcess)
            {
                var header = new BcmMessageHeader(BcmOpcode.RX_SETUP)
                {
                    Flags = BcmFlags.RX_FILTER_ID,
                    CanId = 0x123123FF | (uint)CanIdFlags.CAN_EFF_FLAG,
                    NumberOfFrames = 0,
                };

                var bcmMessage = new BcmCanMessage(header);

                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(56, nBytes);
            }
            else
            {
                var header = new BcmMessageHeader32(BcmOpcode.RX_SETUP)
                {
                    Flags = BcmFlags.RX_FILTER_ID,
                    CanId = 0x123123FF | (uint)CanIdFlags.CAN_EFF_FLAG,
                    NumberOfFrames = 0,
                };

                var bcmMessage = new BcmCanMessage32(header);

                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(40, nBytes);
            }
        }

        [Test]
        public void CAN_BCM_RX_SETUP_with_Timeout_Success_Test()
        {
            if (Environment.Is64BitProcess)
            {
                var timeval = new Timeval(3, 0);
                int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_SOCKET, SocketLevelOptions.SO_RCVTIMEO, timeval, Marshal.SizeOf(typeof(Timeval)));
                Assert.AreEqual(0, result);

                var header = new BcmMessageHeader(BcmOpcode.RX_SETUP)
                {
                    Flags = BcmFlags.STARTTIMER | BcmFlags.SETTIMER | BcmFlags.RX_FILTER_ID,
                    CanId = 0x123,
                    NumberOfFrames = 0,
                    Interval1 = new BcmTimeval(2, 0),
                    Interval2 = new BcmTimeval(1, 0),
                };

                var bcmMessage = new BcmCanMessage(header);

                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(56, nBytes);

                var receiveMessage = new BcmCanMessage();
                nBytes = LibcNativeMethods.Read(socketHandle, receiveMessage, Marshal.SizeOf(typeof(BcmCanMessage)));
                Assert.AreEqual(BcmOpcode.RX_TIMEOUT, receiveMessage.Header.Opcode);
                Assert.AreEqual(56, nBytes);
            }
            else
            {
                var timeval = new Timeval(3, 0);
                int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_SOCKET, SocketLevelOptions.SO_RCVTIMEO, timeval, Marshal.SizeOf(typeof(Timeval)));
                Assert.AreEqual(0, result);

                var header = new BcmMessageHeader32(BcmOpcode.RX_SETUP)
                {
                    Flags = BcmFlags.STARTTIMER | BcmFlags.SETTIMER | BcmFlags.RX_FILTER_ID,
                    CanId = 0x123,
                    NumberOfFrames = 0,
                    Interval1 = new BcmTimeval(2, 0),
                    Interval2 = new BcmTimeval(1, 0),
                };

                var bcmMessage = new BcmCanMessage32(header);

                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(40, nBytes);

                var receiveMessage = new BcmCanMessage32();
                nBytes = LibcNativeMethods.Read(socketHandle, receiveMessage, Marshal.SizeOf(typeof(BcmCanMessage32)));
                Assert.AreEqual(BcmOpcode.RX_TIMEOUT, receiveMessage.Header.Opcode);
                Assert.AreEqual(40, nBytes);
            }
        }

        [Test]
        public void CAN_BCM_RX_SETUP_with_Timeout_CANFD_Success_Test()
        {
            var ifrMtu = new IfreqMtu("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFMTU, ifrMtu);
            Assert.AreNotEqual(-1, ioctlResult, $"Errno: {LibcNativeMethods.Errno}");
            Assume.That(ifrMtu.MTU, Is.EqualTo(SocketCanConstants.CANFD_MTU));

            if (Environment.Is64BitProcess)
            {
                var timeval = new Timeval(3, 0);
                int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_SOCKET, SocketLevelOptions.SO_RCVTIMEO, timeval, Marshal.SizeOf(typeof(Timeval)));
                Assert.AreEqual(0, result);

                var header = new BcmMessageHeader(BcmOpcode.RX_SETUP)
                {
                    Flags = BcmFlags.STARTTIMER | BcmFlags.SETTIMER | BcmFlags.RX_FILTER_ID | BcmFlags.CAN_FD_FRAME,
                    CanId = 0x123,
                    NumberOfFrames = 0,
                    Interval1 = new BcmTimeval(2, 0),
                    Interval2 = new BcmTimeval(1, 0),
                };

                var bcmMessage = new BcmCanFdMessage(header);
                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(56, nBytes);

                var receiveMessage = new BcmCanFdMessage();
                nBytes = LibcNativeMethods.Read(socketHandle, receiveMessage, Marshal.SizeOf(typeof(BcmCanFdMessage)));
                Assert.AreEqual(BcmOpcode.RX_TIMEOUT, receiveMessage.Header.Opcode);
                Assert.AreEqual(56, nBytes);
            }
            else
            {
                var timeval = new Timeval(3, 0);
                int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_SOCKET, SocketLevelOptions.SO_RCVTIMEO, timeval, Marshal.SizeOf(typeof(Timeval)));
                Assert.AreEqual(0, result);

                var header = new BcmMessageHeader32(BcmOpcode.RX_SETUP)
                {
                    Flags = BcmFlags.STARTTIMER | BcmFlags.SETTIMER | BcmFlags.RX_FILTER_ID | BcmFlags.CAN_FD_FRAME,
                    CanId = 0x123,
                    NumberOfFrames = 0,
                    Interval1 = new BcmTimeval(2, 0),
                    Interval2 = new BcmTimeval(1, 0),
                };

                var bcmMessage = new BcmCanFdMessage32(header);
                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(40, nBytes);

                var receiveMessage = new BcmCanFdMessage32();
                nBytes = LibcNativeMethods.Read(socketHandle, receiveMessage, Marshal.SizeOf(typeof(BcmCanFdMessage32)));
                Assert.AreEqual(BcmOpcode.RX_TIMEOUT, receiveMessage.Header.Opcode);
                Assert.AreEqual(40, nBytes);
            }
        }

        [Test]
        public void CAN_BCM_RX_DELETE_Success_Test()
        {
            if (Environment.Is64BitProcess)
            {
                var header = new BcmMessageHeader(BcmOpcode.RX_SETUP)
                {
                    Flags = BcmFlags.RX_FILTER_ID,
                    CanId = 0x123,
                    NumberOfFrames = 0,
                };

                var bcmMessage = new BcmCanMessage(header);
                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(56, nBytes);

                var deleteHeader = new BcmMessageHeader(BcmOpcode.RX_DELETE)
                {
                    CanId = 0x123,
                    NumberOfFrames = 0,
                };

                nBytes = LibcNativeMethods.Write(socketHandle, deleteHeader, Marshal.SizeOf(typeof(BcmMessageHeader)));
                Assert.AreEqual(56, nBytes);
            }
            else
            {
                var header = new BcmMessageHeader32(BcmOpcode.RX_SETUP)
                {
                    Flags = BcmFlags.RX_FILTER_ID,
                    CanId = 0x123,
                    NumberOfFrames = 0,
                };

                var bcmMessage = new BcmCanMessage32(header);
                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(40, nBytes);

                var deleteHeader = new BcmMessageHeader32(BcmOpcode.RX_DELETE)
                {
                    CanId = 0x123,
                    NumberOfFrames = 0,
                };

                nBytes = LibcNativeMethods.Write(socketHandle, deleteHeader, Marshal.SizeOf(typeof(BcmMessageHeader32)));
                Assert.AreEqual(40, nBytes);
            }
        }

        [Test]
        public void CAN_BCM_RX_DELETE_CANFD_Success_Test()
        {
            var ifrMtu = new IfreqMtu("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFMTU, ifrMtu);
            Assert.AreNotEqual(-1, ioctlResult, $"Errno: {LibcNativeMethods.Errno}");
            Assume.That(ifrMtu.MTU, Is.EqualTo(SocketCanConstants.CANFD_MTU));

            if (Environment.Is64BitProcess)
            {
                var header = new BcmMessageHeader(BcmOpcode.RX_SETUP)
                {
                    Flags = BcmFlags.RX_FILTER_ID | BcmFlags.CAN_FD_FRAME,
                    CanId = 0x123,
                    NumberOfFrames = 0,
                };

                var bcmMessage = new BcmCanFdMessage(header);
                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(56, nBytes);

                var deleteHeader = new BcmMessageHeader(BcmOpcode.RX_DELETE)
                {
                    CanId = 0x123,
                    Flags = BcmFlags.CAN_FD_FRAME,
                    NumberOfFrames = 0,
                };

                nBytes = LibcNativeMethods.Write(socketHandle, deleteHeader, Marshal.SizeOf(typeof(BcmMessageHeader)));
                Assert.AreEqual(56, nBytes);
            }
            else
            {
                var header = new BcmMessageHeader32(BcmOpcode.RX_SETUP)
                {
                    Flags = BcmFlags.RX_FILTER_ID | BcmFlags.CAN_FD_FRAME,
                    CanId = 0x123,
                    NumberOfFrames = 0,
                };

                var bcmMessage = new BcmCanFdMessage32(header);
                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(40, nBytes);

                var deleteHeader = new BcmMessageHeader32(BcmOpcode.RX_DELETE)
                {
                    CanId = 0x123,
                    Flags = BcmFlags.CAN_FD_FRAME,
                    NumberOfFrames = 0,
                };

                nBytes = LibcNativeMethods.Write(socketHandle, deleteHeader, Marshal.SizeOf(typeof(BcmMessageHeader32)));
                Assert.AreEqual(40, nBytes);
            }
        }

        [Test]
        public void CAN_BCM_RX_READ_Success_Test()
        {
            if (Environment.Is64BitProcess)
            {
                var header = new BcmMessageHeader(BcmOpcode.RX_SETUP)
                {
                    Flags = BcmFlags.STARTTIMER | BcmFlags.RX_FILTER_ID,
                    CanId = 0x123,
                    NumberOfFrames = 0,
                };

                var bcmMessage = new BcmCanMessage(header);
                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(56, nBytes);

                var readHeader = new BcmMessageHeader(BcmOpcode.RX_READ)
                {
                    CanId = 0x123,
                };
                var readMessage = new BcmCanMessage(readHeader);

                nBytes = LibcNativeMethods.Write(socketHandle, readMessage, Marshal.SizeOf(typeof(BcmCanMessage)));
                Assert.AreEqual(56, nBytes);
                Assert.AreEqual(BcmOpcode.RX_READ, readHeader.Opcode);

                var receiveMessage = new BcmCanMessage();
                nBytes = LibcNativeMethods.Read(socketHandle, receiveMessage, Marshal.SizeOf(typeof(BcmCanMessage)));
                Assert.AreEqual(BcmOpcode.RX_STATUS, receiveMessage.Header.Opcode);
                Assert.AreEqual(0x123, receiveMessage.Header.CanId);
                Assert.AreEqual(BcmFlags.STARTTIMER | BcmFlags.RX_FILTER_ID, receiveMessage.Header.Flags);
                Assert.AreEqual(56, nBytes);
            }
            else
            {
                var header = new BcmMessageHeader32(BcmOpcode.RX_SETUP)
                {
                    Flags = BcmFlags.STARTTIMER | BcmFlags.RX_FILTER_ID,
                    CanId = 0x123,
                    NumberOfFrames = 0,
                };

                var bcmMessage = new BcmCanMessage32(header);
                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(40, nBytes);

                var readHeader = new BcmMessageHeader32(BcmOpcode.RX_READ)
                {
                    CanId = 0x123,
                };
                var readMessage = new BcmCanMessage32(readHeader);

                nBytes = LibcNativeMethods.Write(socketHandle, readMessage, Marshal.SizeOf(typeof(BcmCanMessage32)));
                Assert.AreEqual(40, nBytes);
                Assert.AreEqual(BcmOpcode.RX_READ, readHeader.Opcode);

                var receiveMessage = new BcmCanMessage32();
                nBytes = LibcNativeMethods.Read(socketHandle, receiveMessage, Marshal.SizeOf(typeof(BcmCanMessage32)));
                Assert.AreEqual(BcmOpcode.RX_STATUS, receiveMessage.Header.Opcode);
                Assert.AreEqual(0x123, receiveMessage.Header.CanId);
                Assert.AreEqual(BcmFlags.STARTTIMER | BcmFlags.RX_FILTER_ID, receiveMessage.Header.Flags);
                Assert.AreEqual(40, nBytes);
            }
        }

        [Test]
        public void CAN_BCM_RX_READ_CANFD_Success_Test()
        {
            var ifrMtu = new IfreqMtu("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFMTU, ifrMtu);
            Assert.AreNotEqual(-1, ioctlResult, $"Errno: {LibcNativeMethods.Errno}");
            Assume.That(ifrMtu.MTU, Is.EqualTo(SocketCanConstants.CANFD_MTU));

            if (Environment.Is64BitProcess)
            {
                var header = new BcmMessageHeader(BcmOpcode.RX_SETUP)
                {
                    Flags = BcmFlags.STARTTIMER | BcmFlags.RX_FILTER_ID,
                    CanId = 0x123,
                    NumberOfFrames = 0,
                };

                var bcmMessage = new BcmCanFdMessage(header);
                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(56, nBytes);

                var readHeader = new BcmMessageHeader(BcmOpcode.RX_READ)
                {
                    CanId = 0x123,
                    Flags = BcmFlags.CAN_FD_FRAME,
                };
                var readMessage = new BcmCanFdMessage(readHeader);
                nBytes = LibcNativeMethods.Write(socketHandle, readMessage, Marshal.SizeOf(typeof(BcmCanFdMessage)));
                Assert.AreEqual(56, nBytes);
                Assert.AreEqual(BcmOpcode.RX_READ, readHeader.Opcode);

                var receiveMessage = new BcmCanFdMessage();
                nBytes = LibcNativeMethods.Read(socketHandle, receiveMessage, Marshal.SizeOf(typeof(BcmCanFdMessage)));
                Assert.AreEqual(BcmOpcode.RX_STATUS, receiveMessage.Header.Opcode);
                Assert.AreEqual(0x123, receiveMessage.Header.CanId);
                Assert.AreEqual(BcmFlags.STARTTIMER | BcmFlags.RX_FILTER_ID | BcmFlags.CAN_FD_FRAME, receiveMessage.Header.Flags);
                Assert.AreEqual(56, nBytes);
            }
            else
            {
                var header = new BcmMessageHeader32(BcmOpcode.RX_SETUP)
                {
                    Flags = BcmFlags.STARTTIMER | BcmFlags.RX_FILTER_ID,
                    CanId = 0x123,
                    NumberOfFrames = 0,
                };

                var bcmMessage = new BcmCanFdMessage32(header);
                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(40, nBytes);

                var readHeader = new BcmMessageHeader32(BcmOpcode.RX_READ)
                {
                    CanId = 0x123,
                    Flags = BcmFlags.CAN_FD_FRAME,
                };
                var readMessage = new BcmCanFdMessage32(readHeader);
                nBytes = LibcNativeMethods.Write(socketHandle, readMessage, Marshal.SizeOf(typeof(BcmCanFdMessage32)));
                Assert.AreEqual(40, nBytes);
                Assert.AreEqual(BcmOpcode.RX_READ, readHeader.Opcode);

                var receiveMessage = new BcmCanFdMessage32();
                nBytes = LibcNativeMethods.Read(socketHandle, receiveMessage, Marshal.SizeOf(typeof(BcmCanFdMessage32)));
                Assert.AreEqual(BcmOpcode.RX_STATUS, receiveMessage.Header.Opcode);
                Assert.AreEqual(0x123, receiveMessage.Header.CanId);
                Assert.AreEqual(BcmFlags.STARTTIMER | BcmFlags.RX_FILTER_ID | BcmFlags.CAN_FD_FRAME, receiveMessage.Header.Flags);
                Assert.AreEqual(40, nBytes);
            }
        }

        [Test]
        public void CAN_BCM_RX_READ_29BitAddressing_Success_Test()
        {
            if (Environment.Is64BitProcess)
            {
                var header = new BcmMessageHeader(BcmOpcode.RX_SETUP)
                {
                    Flags = BcmFlags.STARTTIMER | BcmFlags.RX_FILTER_ID,
                    CanId = 0x123123FF | (uint)CanIdFlags.CAN_EFF_FLAG,
                    NumberOfFrames = 0,
                };
                var bcmMessage = new BcmCanMessage(header);
                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(56, nBytes);

                var readHeader = new BcmMessageHeader(BcmOpcode.RX_READ)
                {
                    CanId = 0x123123FF | (uint)CanIdFlags.CAN_EFF_FLAG,
                };
                var readMessage = new BcmCanMessage(readHeader);
                nBytes = LibcNativeMethods.Write(socketHandle, readMessage, Marshal.SizeOf(typeof(BcmCanMessage)));
                Assert.AreEqual(56, nBytes);
                Assert.AreEqual(BcmOpcode.RX_READ, readHeader.Opcode);

                var receiveMessage = new BcmCanMessage();
                nBytes = LibcNativeMethods.Read(socketHandle, receiveMessage, Marshal.SizeOf(typeof(BcmCanMessage)));
                Assert.AreEqual(BcmOpcode.RX_STATUS, receiveMessage.Header.Opcode);
                Assert.AreEqual(56, nBytes);
                Assert.AreEqual(BcmFlags.STARTTIMER | BcmFlags.RX_FILTER_ID, receiveMessage.Header.Flags);
                Assert.AreEqual(0x123123FF, receiveMessage.Header.CanId & SocketCanConstants.CAN_EFF_MASK);
            }
            else
            {
                var header = new BcmMessageHeader32(BcmOpcode.RX_SETUP)
                {
                    Flags = BcmFlags.STARTTIMER | BcmFlags.RX_FILTER_ID,
                    CanId = 0x123123FF | (uint)CanIdFlags.CAN_EFF_FLAG,
                    NumberOfFrames = 0,
                };
                var bcmMessage = new BcmCanMessage32(header);
                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(40, nBytes);

                var readHeader = new BcmMessageHeader32(BcmOpcode.RX_READ)
                {
                    CanId = 0x123123FF | (uint)CanIdFlags.CAN_EFF_FLAG,
                };
                var readMessage = new BcmCanMessage32(readHeader);
                nBytes = LibcNativeMethods.Write(socketHandle, readMessage, Marshal.SizeOf(typeof(BcmCanMessage32)));
                Assert.AreEqual(40, nBytes);
                Assert.AreEqual(BcmOpcode.RX_READ, readHeader.Opcode);

                var receiveMessage = new BcmCanMessage32();
                nBytes = LibcNativeMethods.Read(socketHandle, receiveMessage, Marshal.SizeOf(typeof(BcmCanMessage32)));
                Assert.AreEqual(BcmOpcode.RX_STATUS, receiveMessage.Header.Opcode);
                Assert.AreEqual(40, nBytes);
                Assert.AreEqual(BcmFlags.STARTTIMER | BcmFlags.RX_FILTER_ID, receiveMessage.Header.Flags);
                Assert.AreEqual(0x123123FF, receiveMessage.Header.CanId & SocketCanConstants.CAN_EFF_MASK);
            }
        }

        [Test]
        public void CAN_BCM_RX_CHANGED_Detection_Success_Test()
        {
            if (Environment.Is64BitProcess)
            {
                var header = new BcmMessageHeader(BcmOpcode.RX_SETUP)
                {
                    Flags = BcmFlags.RX_FILTER_ID,
                    CanId = 0x123,
                    NumberOfFrames = 0,
                };

                var bcmMessage = new BcmCanMessage(header);

                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(56, nBytes);

                var canFrame = new CanFrame(0x123, new byte[] { 0x0F, 0xED });
                var wrtHeader = new BcmMessageHeader(BcmOpcode.TX_SEND)
                {
                    Flags = BcmFlags.None,
                    NumberOfFrames = 1,
                };

                var wrtMessage = new BcmCanSingleMessage(wrtHeader, canFrame);

                nBytes = LibcNativeMethods.Write(socketHandle, wrtMessage, Marshal.SizeOf(wrtMessage));
                Assert.AreEqual(72, nBytes);

                var receiveMessage = new BcmCanMessage();
                nBytes = LibcNativeMethods.Read(socketHandle, receiveMessage, Marshal.SizeOf(typeof(BcmCanMessage)));
                Assert.AreEqual(BcmOpcode.RX_CHANGED, receiveMessage.Header.Opcode);
                Assert.AreEqual((uint)0x123, receiveMessage.Header.CanId);
                Assert.AreEqual(72, nBytes);

                Assert.AreEqual(1, receiveMessage.Header.NumberOfFrames);

                var readFrame = receiveMessage.Frames.First();
                Assert.AreEqual((uint)0x123, readFrame.CanId);
                Assert.IsTrue(readFrame.Data.Take(readFrame.Length).SequenceEqual(new byte[] { 0x0F, 0xED }));
            }
            else
            {
                var header = new BcmMessageHeader32(BcmOpcode.RX_SETUP)
                {
                    Flags = BcmFlags.RX_FILTER_ID,
                    CanId = 0x123,
                    NumberOfFrames = 0,
                };

                var bcmMessage = new BcmCanMessage32(header);

                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(40, nBytes);

                var canFrame = new CanFrame(0x123, new byte[] { 0x0F, 0xED });
                var wrtHeader = new BcmMessageHeader32(BcmOpcode.TX_SEND)
                {
                    Flags = BcmFlags.None,
                    NumberOfFrames = 1,
                };

                var wrtMessage = new BcmCanSingleMessage32(wrtHeader, canFrame);

                nBytes = LibcNativeMethods.Write(socketHandle, wrtMessage, Marshal.SizeOf(wrtMessage));
                Assert.AreEqual(56, nBytes);

                var receiveMessage = new BcmCanMessage32();
                nBytes = LibcNativeMethods.Read(socketHandle, receiveMessage, Marshal.SizeOf(typeof(BcmCanMessage32)));
                Assert.AreEqual(BcmOpcode.RX_CHANGED, receiveMessage.Header.Opcode);
                Assert.AreEqual((uint)0x123, receiveMessage.Header.CanId);
                Assert.AreEqual(56, nBytes);

                Assert.AreEqual(1, receiveMessage.Header.NumberOfFrames);

                var readFrame = receiveMessage.Frames.First();
                Assert.AreEqual((uint)0x123, readFrame.CanId);
                Assert.IsTrue(readFrame.Data.Take(readFrame.Length).SequenceEqual(new byte[] { 0x0F, 0xED }));
            }
        }

        [Test]
        public void CAN_BCM_RX_CHANGED_Detection_CANFD_Success_Test()
        {
            var ifrMtu = new IfreqMtu("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFMTU, ifrMtu);
            Assert.AreNotEqual(-1, ioctlResult, $"Errno: {LibcNativeMethods.Errno}");
            Assume.That(ifrMtu.MTU, Is.EqualTo(SocketCanConstants.CANFD_MTU));

            if (Environment.Is64BitProcess)
            {
                var header = new BcmMessageHeader(BcmOpcode.RX_SETUP)
                {
                    Flags = BcmFlags.RX_FILTER_ID,
                    CanId = 0x12,
                    NumberOfFrames = 0,
                };

                var bcmMessage = new BcmCanFdMessage(header);
                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(56, nBytes);

                var canFrame = new CanFdFrame(0x12, new byte[] { 0x0F, 0xED, 0x0F, 0xED, 0x0F, 0xED, 0x0F, 0xED, 0x0F, 0xED, 0x0F, 0xED, 0x0F, 0xED, 0x0F, 0xED }, CanFdFlags.CANFD_BRS);
                var wrtHeader = new BcmMessageHeader(BcmOpcode.TX_SEND)
                {
                    Flags = BcmFlags.None,
                    NumberOfFrames = 1,
                };

                var wrtMessage = new BcmCanFdSingleMessage(wrtHeader, canFrame);
                nBytes = LibcNativeMethods.Write(socketHandle, wrtMessage, Marshal.SizeOf(wrtMessage));
                Assert.AreEqual(128, nBytes);

                var receiveMessage = new BcmCanFdMessage();
                nBytes = LibcNativeMethods.Read(socketHandle, receiveMessage, Marshal.SizeOf(typeof(BcmCanFdMessage)));
                Assert.AreEqual(128, nBytes);
                Assert.AreEqual(BcmOpcode.RX_CHANGED, receiveMessage.Header.Opcode);
                Assert.AreEqual((uint)0x12, receiveMessage.Header.CanId);
                Assert.AreEqual(1, receiveMessage.Header.NumberOfFrames);

                var readFrame = receiveMessage.Frames.First();
                Assert.AreEqual(CanFdFlags.CANFD_BRS, readFrame.Flags);
                Assert.AreEqual((uint)0x12, readFrame.CanId);
                Assert.IsTrue(readFrame.Data.Take(readFrame.Length).SequenceEqual(new byte[] { 0x0F, 0xED, 0x0F, 0xED, 0x0F, 0xED, 0x0F, 0xED, 0x0F, 0xED, 0x0F, 0xED, 0x0F, 0xED, 0x0F, 0xED }));
            }
            else
            {
                var header = new BcmMessageHeader32(BcmOpcode.RX_SETUP)
                {
                    Flags = BcmFlags.RX_FILTER_ID,
                    CanId = 0x12,
                    NumberOfFrames = 0,
                };

                var bcmMessage = new BcmCanFdMessage32(header);
                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(40, nBytes);

                var canFrame = new CanFdFrame(0x12, new byte[] { 0x0F, 0xED, 0x0F, 0xED, 0x0F, 0xED, 0x0F, 0xED, 0x0F, 0xED, 0x0F, 0xED, 0x0F, 0xED, 0x0F, 0xED }, CanFdFlags.CANFD_BRS);
                var wrtHeader = new BcmMessageHeader32(BcmOpcode.TX_SEND)
                {
                    Flags = BcmFlags.None,
                    NumberOfFrames = 1,
                };

                var wrtMessage = new BcmCanFdSingleMessage32(wrtHeader, canFrame);
                nBytes = LibcNativeMethods.Write(socketHandle, wrtMessage, Marshal.SizeOf(wrtMessage));
                Assert.AreEqual(112, nBytes);

                var receiveMessage = new BcmCanFdMessage32();
                nBytes = LibcNativeMethods.Read(socketHandle, receiveMessage, Marshal.SizeOf(typeof(BcmCanFdMessage32)));
                Assert.AreEqual(112, nBytes);
                Assert.AreEqual(BcmOpcode.RX_CHANGED, receiveMessage.Header.Opcode);
                Assert.AreEqual((uint)0x12, receiveMessage.Header.CanId);
                Assert.AreEqual(1, receiveMessage.Header.NumberOfFrames);

                var readFrame = receiveMessage.Frames.First();
                Assert.AreEqual(CanFdFlags.CANFD_BRS, readFrame.Flags);
                Assert.AreEqual((uint)0x12, readFrame.CanId);
                Assert.IsTrue(readFrame.Data.Take(readFrame.Length).SequenceEqual(new byte[] { 0x0F, 0xED, 0x0F, 0xED, 0x0F, 0xED, 0x0F, 0xED, 0x0F, 0xED, 0x0F, 0xED, 0x0F, 0xED, 0x0F, 0xED }));
            }
        }

        [Test]
        public void CAN_BCM_RX_CHANGED_Detection_29BitAddressing_Success_Test()
        {
            if (Environment.Is64BitProcess)
            {
                var header = new BcmMessageHeader(BcmOpcode.RX_SETUP)
                {
                    Flags = BcmFlags.RX_FILTER_ID,
                    CanId = 0x123123FF | (uint)CanIdFlags.CAN_EFF_FLAG,
                    NumberOfFrames = 0,
                };

                var bcmMessage = new BcmCanMessage(header);
                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(56, nBytes);

                var canFrame = new CanFrame(0x123123FF | (uint)CanIdFlags.CAN_EFF_FLAG, new byte[] { 0x0F, 0xED });
                var wrtHeader = new BcmMessageHeader(BcmOpcode.TX_SEND)
                {
                    Flags = BcmFlags.None,
                    NumberOfFrames = 1,
                };

                var wrtMessage = new BcmCanSingleMessage(wrtHeader, canFrame);
                nBytes = LibcNativeMethods.Write(socketHandle, wrtMessage, Marshal.SizeOf(wrtMessage));
                Assert.AreEqual(72, nBytes);

                var receiveMessage = new BcmCanMessage();
                nBytes = LibcNativeMethods.Read(socketHandle, receiveMessage, Marshal.SizeOf(typeof(BcmCanMessage)));
                Assert.AreEqual(BcmOpcode.RX_CHANGED, receiveMessage.Header.Opcode);
                Assert.AreEqual(72, nBytes);
                Assert.AreEqual((uint)0x123123FF, receiveMessage.Header.CanId & SocketCanConstants.CAN_EFF_MASK);
                Assert.AreEqual(1, receiveMessage.Header.NumberOfFrames);

                var readFrame = receiveMessage.Frames.First();
                Assert.AreEqual((uint)0x123123FF, readFrame.CanId & SocketCanConstants.CAN_EFF_MASK);
                Assert.IsTrue(readFrame.Data.Take(readFrame.Length).SequenceEqual(new byte[] { 0x0F, 0xED }));
            }
            else
            {
                var header = new BcmMessageHeader32(BcmOpcode.RX_SETUP)
                {
                    Flags = BcmFlags.RX_FILTER_ID,
                    CanId = 0x123123FF | (uint)CanIdFlags.CAN_EFF_FLAG,
                    NumberOfFrames = 0,
                };

                var bcmMessage = new BcmCanMessage32(header);
                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(40, nBytes);

                var canFrame = new CanFrame(0x123123FF | (uint)CanIdFlags.CAN_EFF_FLAG, new byte[] { 0x0F, 0xED });
                var wrtHeader = new BcmMessageHeader32(BcmOpcode.TX_SEND)
                {
                    Flags = BcmFlags.None,
                    NumberOfFrames = 1,
                };

                var wrtMessage = new BcmCanSingleMessage32(wrtHeader, canFrame);
                nBytes = LibcNativeMethods.Write(socketHandle, wrtMessage, Marshal.SizeOf(wrtMessage));
                Assert.AreEqual(56, nBytes);

                var receiveMessage = new BcmCanMessage32();
                nBytes = LibcNativeMethods.Read(socketHandle, receiveMessage, Marshal.SizeOf(typeof(BcmCanMessage32)));
                Assert.AreEqual(BcmOpcode.RX_CHANGED, receiveMessage.Header.Opcode);
                Assert.AreEqual(56, nBytes);
                Assert.AreEqual((uint)0x123123FF, receiveMessage.Header.CanId & SocketCanConstants.CAN_EFF_MASK);
                Assert.AreEqual(1, receiveMessage.Header.NumberOfFrames);

                var readFrame = receiveMessage.Frames.First();
                Assert.AreEqual((uint)0x123123FF, readFrame.CanId & SocketCanConstants.CAN_EFF_MASK);
                Assert.IsTrue(readFrame.Data.Take(readFrame.Length).SequenceEqual(new byte[] { 0x0F, 0xED }));
            }
        }

        [Test]
        public void CAN_BCM_TX_CP_CAN_ID_Success_Test()
        {
            if (Environment.Is64BitProcess)
            {
                var header = new BcmMessageHeader(BcmOpcode.RX_SETUP)
                {
                    Flags = BcmFlags.RX_FILTER_ID,
                    CanId = 0x123,
                    NumberOfFrames = 0,
                };

                var bcmMessage = new BcmCanMessage(header);
                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(56, nBytes);

                var canFrame = new CanFrame(0x333, new byte[] { 0x0F, 0xED });
                var wrtHeader = new BcmMessageHeader(BcmOpcode.TX_SETUP)
                {
                    Flags = BcmFlags.SETTIMER | BcmFlags.STARTTIMER | BcmFlags.TX_CP_CAN_ID,
                    Interval2 = new BcmTimeval(1, 0),
                    NumberOfFrames = 1,
                    CanId = 0x123,
                };

                var wrtMessage = new BcmCanMessage(wrtHeader, new CanFrame[] { canFrame });

                nBytes = LibcNativeMethods.Write(socketHandle, wrtMessage, Marshal.SizeOf(wrtMessage));
                Assert.AreEqual(72, nBytes);

                var receiveMessage = new BcmCanMessage();
                nBytes = LibcNativeMethods.Read(socketHandle, receiveMessage, Marshal.SizeOf(typeof(BcmCanMessage)));
                Assert.AreEqual(BcmOpcode.RX_CHANGED, receiveMessage.Header.Opcode);
                Assert.AreEqual(72, nBytes);
                Assert.AreEqual((uint)0x123, receiveMessage.Header.CanId);
                Assert.AreEqual(1, receiveMessage.Header.NumberOfFrames);

                var readFrame = receiveMessage.Frames.First();
                Assert.AreEqual((uint)0x123, readFrame.CanId);
                Assert.IsTrue(readFrame.Data.Take(readFrame.Length).SequenceEqual(new byte[] { 0x0F, 0xED }));
            }
            else
            {
                var header = new BcmMessageHeader32(BcmOpcode.RX_SETUP)
                {
                    Flags = BcmFlags.RX_FILTER_ID,
                    CanId = 0x123,
                    NumberOfFrames = 0,
                };

                var bcmMessage = new BcmCanMessage32(header);
                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(40, nBytes);

                var canFrame = new CanFrame(0x333, new byte[] { 0x0F, 0xED });
                var wrtHeader = new BcmMessageHeader32(BcmOpcode.TX_SETUP)
                {
                    Flags = BcmFlags.SETTIMER | BcmFlags.STARTTIMER | BcmFlags.TX_CP_CAN_ID,
                    Interval2 = new BcmTimeval(1, 0),
                    NumberOfFrames = 1,
                    CanId = 0x123,
                };

                var wrtMessage = new BcmCanMessage32(wrtHeader, new CanFrame[] { canFrame });

                nBytes = LibcNativeMethods.Write(socketHandle, wrtMessage, Marshal.SizeOf(wrtMessage));
                Assert.AreEqual(56, nBytes);

                var receiveMessage = new BcmCanMessage32();
                nBytes = LibcNativeMethods.Read(socketHandle, receiveMessage, Marshal.SizeOf(typeof(BcmCanMessage32)));
                Assert.AreEqual(BcmOpcode.RX_CHANGED, receiveMessage.Header.Opcode);
                Assert.AreEqual(56, nBytes);
                Assert.AreEqual((uint)0x123, receiveMessage.Header.CanId);
                Assert.AreEqual(1, receiveMessage.Header.NumberOfFrames);

                var readFrame = receiveMessage.Frames.First();
                Assert.AreEqual((uint)0x123, readFrame.CanId);
                Assert.IsTrue(readFrame.Data.Take(readFrame.Length).SequenceEqual(new byte[] { 0x0F, 0xED }));
            }
        }

        [Test]
        public void CAN_BCM_TX_CP_CAN_ID_CANFD_Success_Test()
        {
            var ifrMtu = new IfreqMtu("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFMTU, ifrMtu);
            Assert.AreNotEqual(-1, ioctlResult, $"Errno: {LibcNativeMethods.Errno}");
            Assume.That(ifrMtu.MTU, Is.EqualTo(SocketCanConstants.CANFD_MTU));
            
            if (Environment.Is64BitProcess)
            {
                var header = new BcmMessageHeader(BcmOpcode.RX_SETUP)
                {
                    Flags = BcmFlags.RX_FILTER_ID,
                    CanId = 0x400,
                    NumberOfFrames = 0,
                };

                var bcmMessage = new BcmCanFdMessage(header);
                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(56, nBytes);

                var canFrame = new CanFdFrame(0x88, new byte[] { 0x0F, 0xED }, CanFdFlags.CANFD_BRS);
                var wrtHeader = new BcmMessageHeader(BcmOpcode.TX_SETUP)
                {
                    Flags = BcmFlags.SETTIMER | BcmFlags.STARTTIMER | BcmFlags.TX_CP_CAN_ID,
                    Interval2 = new BcmTimeval(1, 0),
                    NumberOfFrames = 1,
                    CanId = 0x400,
                };

                var wrtMessage = new BcmCanFdMessage(wrtHeader, new CanFdFrame[] { canFrame });
                nBytes = LibcNativeMethods.Write(socketHandle, wrtMessage, Marshal.SizeOf(wrtMessage));
                Assert.AreEqual(128, nBytes);

                var receiveMessage = new BcmCanFdMessage();
                nBytes = LibcNativeMethods.Read(socketHandle, receiveMessage, Marshal.SizeOf(typeof(BcmCanMessage)));
                Assert.AreEqual(BcmOpcode.RX_CHANGED, receiveMessage.Header.Opcode);
                Assert.AreEqual(128, nBytes);
                Assert.AreEqual((uint)0x400, receiveMessage.Header.CanId);
                Assert.AreEqual(1, receiveMessage.Header.NumberOfFrames);

                var readFrame = receiveMessage.Frames.First();
                Assert.AreEqual((uint)0x400, readFrame.CanId);
                Assert.AreEqual(CanFdFlags.CANFD_BRS, readFrame.Flags);
                Assert.IsTrue(readFrame.Data.Take(readFrame.Length).SequenceEqual(new byte[] { 0x0F, 0xED }));
            }
            else
            {
                var header = new BcmMessageHeader32(BcmOpcode.RX_SETUP)
                {
                    Flags = BcmFlags.RX_FILTER_ID,
                    CanId = 0x400,
                    NumberOfFrames = 0,
                };

                var bcmMessage = new BcmCanFdMessage32(header);
                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(40, nBytes);

                var canFrame = new CanFdFrame(0x88, new byte[] { 0x0F, 0xED }, CanFdFlags.CANFD_BRS);
                var wrtHeader = new BcmMessageHeader32(BcmOpcode.TX_SETUP)
                {
                    Flags = BcmFlags.SETTIMER | BcmFlags.STARTTIMER | BcmFlags.TX_CP_CAN_ID,
                    Interval2 = new BcmTimeval(1, 0),
                    NumberOfFrames = 1,
                    CanId = 0x400,
                };

                var wrtMessage = new BcmCanFdMessage32(wrtHeader, new CanFdFrame[] { canFrame });
                nBytes = LibcNativeMethods.Write(socketHandle, wrtMessage, Marshal.SizeOf(wrtMessage));
                Assert.AreEqual(112, nBytes);

                var receiveMessage = new BcmCanFdMessage32();
                nBytes = LibcNativeMethods.Read(socketHandle, receiveMessage, Marshal.SizeOf(typeof(BcmCanMessage32)));
                Assert.AreEqual(BcmOpcode.RX_CHANGED, receiveMessage.Header.Opcode);
                Assert.AreEqual(112, nBytes);
                Assert.AreEqual((uint)0x400, receiveMessage.Header.CanId);
                Assert.AreEqual(1, receiveMessage.Header.NumberOfFrames);

                var readFrame = receiveMessage.Frames.First();
                Assert.AreEqual((uint)0x400, readFrame.CanId);
                Assert.AreEqual(CanFdFlags.CANFD_BRS, readFrame.Flags);
                Assert.IsTrue(readFrame.Data.Take(readFrame.Length).SequenceEqual(new byte[] { 0x0F, 0xED }));
            }
        }

        [Test]
        public void CAN_BCM_TX_CP_CAN_ID_29BitAddressing_Success_Test()
        {
            if (Environment.Is64BitProcess)
            {
                var header = new BcmMessageHeader(BcmOpcode.RX_SETUP)
                {
                    Flags = BcmFlags.RX_FILTER_ID,
                    CanId = 0x123123FF | (uint)CanIdFlags.CAN_EFF_FLAG,
                    NumberOfFrames = 0,
                };

                var bcmMessage = new BcmCanMessage(header);
                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(56, nBytes);

                var canFrame = new CanFrame(0x333, new byte[] { 0x0F, 0xED });
                var wrtHeader = new BcmMessageHeader(BcmOpcode.TX_SETUP)
                {
                    Flags = BcmFlags.SETTIMER | BcmFlags.STARTTIMER | BcmFlags.TX_CP_CAN_ID,
                    Interval2 = new BcmTimeval(1, 0),
                    NumberOfFrames = 1,
                    CanId = 0x123123FF | (uint)CanIdFlags.CAN_EFF_FLAG,
                };

                var wrtMessage = new BcmCanMessage(wrtHeader, new CanFrame[] { canFrame });

                nBytes = LibcNativeMethods.Write(socketHandle, wrtMessage, Marshal.SizeOf(wrtMessage));
                Assert.AreEqual(72, nBytes);

                var receiveMessage = new BcmCanMessage();
                nBytes = LibcNativeMethods.Read(socketHandle, receiveMessage, Marshal.SizeOf(typeof(BcmCanMessage)));
                Assert.AreEqual(BcmOpcode.RX_CHANGED, receiveMessage.Header.Opcode);
                Assert.AreEqual(72, nBytes);
                Assert.AreEqual((uint)0x123123FF, receiveMessage.Header.CanId & SocketCanConstants.CAN_EFF_MASK);
                Assert.AreEqual(1, receiveMessage.Header.NumberOfFrames);

                var readFrame = receiveMessage.Frames.First();
                Assert.AreEqual((uint)0x123123FF, readFrame.CanId & SocketCanConstants.CAN_EFF_MASK);
                Assert.IsTrue(readFrame.Data.Take(readFrame.Length).SequenceEqual(new byte[] { 0x0F, 0xED }));
            }
            else
            {
                var header = new BcmMessageHeader32(BcmOpcode.RX_SETUP)
                {
                    Flags = BcmFlags.RX_FILTER_ID,
                    CanId = 0x123123FF | (uint)CanIdFlags.CAN_EFF_FLAG,
                    NumberOfFrames = 0,
                };

                var bcmMessage = new BcmCanMessage32(header);
                int nBytes = LibcNativeMethods.Write(socketHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
                Assert.AreEqual(40, nBytes);

                var canFrame = new CanFrame(0x333, new byte[] { 0x0F, 0xED });
                var wrtHeader = new BcmMessageHeader32(BcmOpcode.TX_SETUP)
                {
                    Flags = BcmFlags.SETTIMER | BcmFlags.STARTTIMER | BcmFlags.TX_CP_CAN_ID,
                    Interval2 = new BcmTimeval(1, 0),
                    NumberOfFrames = 1,
                    CanId = 0x123123FF | (uint)CanIdFlags.CAN_EFF_FLAG,
                };

                var wrtMessage = new BcmCanMessage32(wrtHeader, new CanFrame[] { canFrame });

                nBytes = LibcNativeMethods.Write(socketHandle, wrtMessage, Marshal.SizeOf(wrtMessage));
                Assert.AreEqual(56, nBytes);

                var receiveMessage = new BcmCanMessage32();
                nBytes = LibcNativeMethods.Read(socketHandle, receiveMessage, Marshal.SizeOf(typeof(BcmCanMessage32)));
                Assert.AreEqual(BcmOpcode.RX_CHANGED, receiveMessage.Header.Opcode);
                Assert.AreEqual(56, nBytes);
                Assert.AreEqual((uint)0x123123FF, receiveMessage.Header.CanId & SocketCanConstants.CAN_EFF_MASK);
                Assert.AreEqual(1, receiveMessage.Header.NumberOfFrames);

                var readFrame = receiveMessage.Frames.First();
                Assert.AreEqual((uint)0x123123FF, readFrame.CanId & SocketCanConstants.CAN_EFF_MASK);
                Assert.IsTrue(readFrame.Data.Take(readFrame.Length).SequenceEqual(new byte[] { 0x0F, 0xED }));
            }
        }
    }
}