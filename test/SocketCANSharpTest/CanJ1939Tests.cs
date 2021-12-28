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
    public class CanJ1939Tests
    {
        SafeSocketHandle testerSocketHandle;
        SafeSocketHandle ecuSocketHandle;
        SafeSocketHandle broadcastHandle;

        [SetUp]
        public void Setup()
        {
            testerSocketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_J1939);
            Assert.IsFalse(testerSocketHandle.IsInvalid);

            var ifr = new Ifreq("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(testerSocketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
            Assert.AreNotEqual(-1, ioctlResult);

            var srcAddr = new SockAddrCanJ1939(ifr.IfIndex)
            {
                Name = SocketCanConstants.J1939_NO_NAME,
                PGN = 0x1b100,
                Address = 0x25,
            };

            int bindResult = LibcNativeMethods.Bind(testerSocketHandle, srcAddr, Marshal.SizeOf(typeof(SockAddrCanJ1939)));
            Assert.AreNotEqual(-1, bindResult);

            var dstAddr = new SockAddrCanJ1939(ifr.IfIndex)
            {
                Name = SocketCanConstants.J1939_NO_NAME,
                PGN = 0x1b100,
                Address = 0x50,
            };

            int connectResult = LibcNativeMethods.Connect(testerSocketHandle, dstAddr, Marshal.SizeOf(typeof(SockAddrCanJ1939)));
            Assert.AreNotEqual(-1, connectResult);

            ecuSocketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_J1939);
            Assert.IsFalse(ecuSocketHandle.IsInvalid);

            var timeval = new Timeval(2, 0);
            int result = LibcNativeMethods.SetSockOpt(ecuSocketHandle, SocketLevel.SOL_SOCKET, SocketLevelOptions.SO_RCVTIMEO, timeval, Marshal.SizeOf(typeof(Timeval)));
            Assert.AreEqual(0, result);

            var ecuAddr = new SockAddrCanJ1939(ifr.IfIndex)
            {
                Name = SocketCanConstants.J1939_NO_NAME,
                PGN = SocketCanConstants.J1939_NO_PGN,
                Address = 0x50,
            };

            bindResult = LibcNativeMethods.Bind(ecuSocketHandle, ecuAddr, Marshal.SizeOf(typeof(SockAddrCanJ1939)));
            Assert.AreNotEqual(-1, bindResult);

            var tstAddr = new SockAddrCanJ1939(ifr.IfIndex)
            {
                Name = SocketCanConstants.J1939_NO_NAME,
                PGN = SocketCanConstants.J1939_NO_PGN,
                Address = 0x25,
            };

            connectResult = LibcNativeMethods.Connect(ecuSocketHandle, tstAddr, Marshal.SizeOf(typeof(SockAddrCanJ1939)));
            Assert.AreNotEqual(-1, connectResult);

            broadcastHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_J1939);
            Assert.IsFalse(broadcastHandle.IsInvalid);

            int val = 1;
            result = LibcNativeMethods.SetSockOpt(broadcastHandle, SocketLevel.SOL_SOCKET, SocketLevelOptions.SO_BROADCAST, ref val, Marshal.SizeOf(typeof(int)));
            Assert.AreEqual(0, result);

            var broadcastAddr = new SockAddrCanJ1939(ifr.IfIndex)
            {
                Name = SocketCanConstants.J1939_NO_NAME,
                PGN = 0x1fc02,
                Address = 0x99,
            };

            bindResult = LibcNativeMethods.Bind(broadcastHandle, broadcastAddr, Marshal.SizeOf(typeof(SockAddrCanJ1939)));
            Assert.AreNotEqual(-1, bindResult);
        }

        [TearDown]
        public void Cleanup()
        {
            testerSocketHandle.Close();
            ecuSocketHandle.Close();
            broadcastHandle.Close();
        }

        [Test]
        public void CAN_J1939_PDU1_SendTo_Success_Test()
        {
            var dstAddr = new SockAddrCanJ1939(0)
            {
                Name = SocketCanConstants.J1939_NO_NAME,
                PGN = 0x18300, // R: 0, DP: 1, PF: $83, PS: $00 as Address holds value 
                Address = 0x35,
            };
 
            byte[] data = new byte[] { 0xde, 0xad, 0xbe, 0xef };
            int sendToResult = LibcNativeMethods.SendTo(testerSocketHandle, data, data.Length, MessageFlags.None, dstAddr, Marshal.SizeOf(typeof(SockAddrCanJ1939)));
            Assert.AreNotEqual(-1, sendToResult);
        }

        [Test]
        public void CAN_J1939_PDU1_Send_Success_Test()
        {
            byte[] data = new byte[] { 0xba, 0xdf, 0xad };
            int sendResult = LibcNativeMethods.Send(testerSocketHandle, data, data.Length, MessageFlags.None);
            Assert.AreNotEqual(-1, sendResult);
        }

        [Test]
        public void CAN_J1939_PDU1_SendTo_NonBlocking_Success_Test()
        {
            var dstAddr = new SockAddrCanJ1939(0)
            {
                Name = SocketCanConstants.J1939_NO_NAME,
                PGN = 0x18300, // R: 0, DP: 1, PF: $83, PS: $00 as Address holds value 
                Address = 0x35,
            };
 
            byte[] data = new byte[] { 0xde, 0xad, 0xbe, 0xef };
            int sendToResult = LibcNativeMethods.SendTo(testerSocketHandle, data, data.Length, MessageFlags.MSG_DONTWAIT, dstAddr, Marshal.SizeOf(typeof(SockAddrCanJ1939)));
            Assert.AreNotEqual(-1, sendToResult);
        }

        [Test]
        public void CAN_J1939_PDU1_Send_NonBlocking_Success_Test()
        {
            byte[] data = new byte[] { 0xba, 0xdf, 0xad };
            int sendResult = LibcNativeMethods.Send(testerSocketHandle, data, data.Length, MessageFlags.MSG_DONTWAIT);
            Assert.AreNotEqual(-1, sendResult);
        }

        [Test]
        public void CAN_J1939_PDU1_Write_Success_Test()
        {
            byte[] data = new byte[] { 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f };
            int writeResult = LibcNativeMethods.Write(testerSocketHandle, data, data.Length);
            Assert.AreNotEqual(-1, writeResult);
        }

        [Test]
        public void CAN_J1939_PDU1_Write_HighPriority_Success_Test()
        {
            int prio = 2;
            int result = LibcNativeMethods.SetSockOpt(testerSocketHandle, SocketLevel.SOL_CAN_J1939, J1939SocketOptions.SO_J1939_SEND_PRIO, ref prio, Marshal.SizeOf(typeof(int)));
            Assert.Greater(result, -1);

            byte[] data = new byte[] { 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f };
            int writeResult = LibcNativeMethods.Write(testerSocketHandle, data, data.Length);
            Assert.AreNotEqual(-1, writeResult);
        }

        [Test]
        public void CAN_J1939_PDU2_SendTo_Success_Test()
        {
            // PDU2 are intended to be broadcast messages
            int value = 1;
            int result = LibcNativeMethods.SetSockOpt(testerSocketHandle, SocketLevel.SOL_SOCKET, SocketLevelOptions.SO_BROADCAST, ref value, Marshal.SizeOf(typeof(int)));
            Assert.AreEqual(0, result);

            var dstAddr = new SockAddrCanJ1939(0)
            {
                Name = SocketCanConstants.J1939_NO_NAME,
                PGN = 0x1F077, // R: 0, DP: 1, PF: Set to $F0, PS: $77
                Address = SocketCanConstants.J1939_NO_ADDR,
            };
 
            byte[] data = new byte[] { 0xed, 0xed, 0xde, 0xed };
            int sendToResult = LibcNativeMethods.SendTo(testerSocketHandle, data, data.Length, MessageFlags.None, dstAddr, Marshal.SizeOf(typeof(SockAddrCanJ1939)));
            Assert.AreNotEqual(-1, sendToResult);
        }

        [Test]
        public void CAN_J1939_PDU2_SendTo_NonBlocking_Success_Test()
        {
            // PDU2 are intended to be broadcast messages
            int value = 1;
            int result = LibcNativeMethods.SetSockOpt(testerSocketHandle, SocketLevel.SOL_SOCKET, SocketLevelOptions.SO_BROADCAST, ref value, Marshal.SizeOf(typeof(int)));
            Assert.AreEqual(0, result);

            var dstAddr = new SockAddrCanJ1939(0)
            {
                Name = SocketCanConstants.J1939_NO_NAME,
                PGN = 0x1F077, // R: 0, DP: 1, PF: Set to $F0, PS: $77
                Address = SocketCanConstants.J1939_NO_ADDR,
            };
 
            byte[] data = new byte[] { 0xed, 0xed, 0xde, 0xed };
            int sendToResult = LibcNativeMethods.SendTo(testerSocketHandle, data, data.Length, MessageFlags.MSG_DONTWAIT, dstAddr, Marshal.SizeOf(typeof(SockAddrCanJ1939)));
            Assert.AreNotEqual(-1, sendToResult);
        }

        [Test]
        public void CAN_J1939_PDU1_RecvFrom_Success_Test()
        {
            var dstAddr = new SockAddrCanJ1939(0)
            {
                Name = SocketCanConstants.J1939_NO_NAME,
                PGN = 0x18300, // R: 0, DP: 1, PF: $83, PS: $00 as Address holds value 
                Address = 0x50, // DA
            };

            byte[] data = new byte[] { 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f };
            int sendToResult = LibcNativeMethods.SendTo(testerSocketHandle, data, data.Length, MessageFlags.MSG_DONTWAIT, dstAddr, Marshal.SizeOf(typeof(SockAddrCanJ1939)));
            Assert.AreNotEqual(-1, sendToResult);

            var srcAddr = new SockAddrCanJ1939(0)
            {
                Name = SocketCanConstants.J1939_NO_NAME,
                PGN = SocketCanConstants.J1939_NO_PGN,
                Address = SocketCanConstants.J1939_NO_ADDR,
            };

            var recvData = new byte[128];
            int addrSize = Marshal.SizeOf(typeof(SockAddrCanJ1939));
            int recvFromResult = LibcNativeMethods.RecvFrom(ecuSocketHandle, recvData, recvData.Length, MessageFlags.None, srcAddr, ref addrSize);
            Assert.AreEqual(6, recvFromResult);
            Assert.AreEqual(0x25, srcAddr.Address);
            Assert.IsTrue(recvData.Take(recvFromResult).SequenceEqual(new byte[] { 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f  }));
        }

        [Test]
        public void CAN_J1939_PDU1_Recv_Success_Test()
        {
            byte[] data = new byte[] { 0xba, 0xdf, 0xad };
            int sendResult = LibcNativeMethods.Send(testerSocketHandle, data, data.Length, MessageFlags.None);
            Assert.AreNotEqual(-1, sendResult);

            var recvData = new byte[128];
            int recvFromResult = LibcNativeMethods.Recv(ecuSocketHandle, recvData, recvData.Length, MessageFlags.None);
            Assert.AreEqual(3, recvFromResult);
            Assert.IsTrue(recvData.Take(recvFromResult).SequenceEqual(new byte[] { 0xba, 0xdf, 0xad   }));
        }

        [Test]
        public void CAN_J1939_PDU1_Read_Success_Test()
        {
            byte[] data = new byte[] { 0x01, 0x03, 0x05, 0x07, 0x09, 0x0a };
            int writeResult = LibcNativeMethods.Write(testerSocketHandle, data, data.Length);
            Assert.AreNotEqual(-1, writeResult);

            var recvData = new byte[128];
            int recvFromResult = LibcNativeMethods.Read(ecuSocketHandle, recvData, recvData.Length);
            Assert.AreEqual(6, recvFromResult);
            Assert.IsTrue(recvData.Take(recvFromResult).SequenceEqual(new byte[] { 0x01, 0x03, 0x05, 0x07, 0x09, 0x0a }));
        }

        [Test]
        public void CAN_J1939_PDU2_RecvFrom_Success_Test()
        {
            int enable = 1;
            int result = LibcNativeMethods.SetSockOpt(ecuSocketHandle, SocketLevel.SOL_CAN_J1939, J1939SocketOptions.SO_J1939_PROMISC, ref enable, Marshal.SizeOf(typeof(int)));
            Assert.Greater(result, -1);

            // PDU2 are intended to be broadcast messages
            int value = 1;
            result = LibcNativeMethods.SetSockOpt(testerSocketHandle, SocketLevel.SOL_SOCKET, SocketLevelOptions.SO_BROADCAST, ref value, Marshal.SizeOf(typeof(int)));
            Assert.AreEqual(0, result);

            var dstAddr = new SockAddrCanJ1939(0)
            {
                Name = SocketCanConstants.J1939_NO_NAME,
                PGN = 0x1fe01, // R: 0, DP: 1, PF: Set to $fe, PS: $01
                Address = SocketCanConstants.J1939_NO_ADDR,
            };
 
            byte[] data = new byte[] { 0x88 };
            int sendToResult = LibcNativeMethods.SendTo(testerSocketHandle, data, data.Length, MessageFlags.None, dstAddr, Marshal.SizeOf(typeof(SockAddrCanJ1939)));
            Assert.AreNotEqual(-1, sendToResult);

            var srcAddr = new SockAddrCanJ1939()
            {
                Name = SocketCanConstants.J1939_NO_NAME,
                PGN = SocketCanConstants.J1939_NO_PGN,
                Address = SocketCanConstants.J1939_NO_ADDR,
            };

            var recvData = new byte[128];
            int addrSize = Marshal.SizeOf(typeof(SockAddrCanJ1939));
            int recvFromResult = LibcNativeMethods.RecvFrom(ecuSocketHandle, recvData, recvData.Length, MessageFlags.None, srcAddr, ref addrSize);
            Assert.AreEqual(1, recvFromResult);
            Assert.AreEqual(0x25, srcAddr.Address);
            Assert.AreEqual(0x1fe01, srcAddr.PGN);
            Assert.IsTrue(recvData.Take(recvFromResult).SequenceEqual(new byte[] { 0x88 }));
        }

        [Test]
        public void CAN_J1939_PDU2_Recv_Success_Test()
        {
            int enable = 1;
            int result = LibcNativeMethods.SetSockOpt(ecuSocketHandle, SocketLevel.SOL_CAN_J1939, J1939SocketOptions.SO_J1939_PROMISC, ref enable, Marshal.SizeOf(typeof(int)));
            Assert.Greater(result, -1); 

            // PDU2 are intended to be broadcast messages
            int value = 1;
            result = LibcNativeMethods.SetSockOpt(testerSocketHandle, SocketLevel.SOL_SOCKET, SocketLevelOptions.SO_BROADCAST, ref value, Marshal.SizeOf(typeof(int)));
            Assert.AreEqual(0, result);

            var dstAddr = new SockAddrCanJ1939(0)
            {
                Name = SocketCanConstants.J1939_NO_NAME,
                PGN = 0x1fe01, // R: 0, DP: 1, PF: Set to $fe, PS: $01
                Address = SocketCanConstants.J1939_NO_ADDR,
            };
 
            byte[] data = new byte[] { 0x11 };
            int sendToResult = LibcNativeMethods.SendTo(testerSocketHandle, data, data.Length, MessageFlags.None, dstAddr, Marshal.SizeOf(typeof(SockAddrCanJ1939)));
            Assert.AreNotEqual(-1, sendToResult);

            var recvData = new byte[128];
            int recvFromResult = LibcNativeMethods.Recv(ecuSocketHandle, recvData, recvData.Length, MessageFlags.None);
            Assert.AreEqual(1, recvFromResult);
            Assert.IsTrue(recvData.Take(recvFromResult).SequenceEqual(new byte[] { 0x11 }));
        }

        [Test]
        public void CAN_J1939_PDU2_Read_Success_Test()
        {
            int enable = 1;
            int result = LibcNativeMethods.SetSockOpt(ecuSocketHandle, SocketLevel.SOL_CAN_J1939, J1939SocketOptions.SO_J1939_PROMISC, ref enable, Marshal.SizeOf(typeof(int)));
            Assert.Greater(result, -1);

            // PDU2 are intended to be broadcast messages
            int value = 1;
            result = LibcNativeMethods.SetSockOpt(testerSocketHandle, SocketLevel.SOL_SOCKET, SocketLevelOptions.SO_BROADCAST, ref value, Marshal.SizeOf(typeof(int)));
            Assert.AreEqual(0, result);

            var dstAddr = new SockAddrCanJ1939(0)
            {
                Name = SocketCanConstants.J1939_NO_NAME,
                PGN = 0x1fe01, // R: 0, DP: 1, PF: Set to $fe, PS: $01
                Address = SocketCanConstants.J1939_NO_ADDR,
            };
 
            byte[] data = new byte[] { 0xFF };
            int sendToResult = LibcNativeMethods.SendTo(testerSocketHandle, data, data.Length, MessageFlags.None, dstAddr, Marshal.SizeOf(typeof(SockAddrCanJ1939)));
            Assert.AreNotEqual(-1, sendToResult);

            var recvData = new byte[128];
            int recvFromResult = LibcNativeMethods.Read(ecuSocketHandle, recvData, recvData.Length);
            Assert.AreEqual(1, recvFromResult);
            Assert.IsTrue(recvData.Take(recvFromResult).SequenceEqual(new byte[] { 0xFF }));
        }

        [Test]
        public void CAN_J1939_TP_CM_Write_Success_Test()
        {
            byte[] data = Enumerable.Repeat<byte>(0x05, 1000).ToArray();
            int writeResult = LibcNativeMethods.Write(testerSocketHandle, data, data.Length);
            Assert.AreNotEqual(-1, writeResult);
        }

        [Test]
        public void CAN_J1939_TP_CM_Read_Success_Test()
        {
            byte[] data = Enumerable.Repeat<byte>(0x06, 1000).ToArray();
            int writeResult = LibcNativeMethods.Write(testerSocketHandle, data, data.Length);
            Assert.AreNotEqual(-1, writeResult);

            var recvData = new byte[1000];
            int recvFromResult = LibcNativeMethods.Read(ecuSocketHandle, recvData, recvData.Length);
            Assert.AreEqual(1000, recvFromResult);
            Assert.IsTrue(recvData.Take(recvFromResult).SequenceEqual(data));
        }

        [Test]
        public void CAN_J1939_TP_BAM_SendTo_Success_Test()
        {
            var dstAddr = new SockAddrCanJ1939(0)
            {
                Name = SocketCanConstants.J1939_NO_NAME,
                PGN = 0x1F077, // R: 0, DP: 1, PF: Set to $F0, PS: $77
                Address = SocketCanConstants.J1939_NO_ADDR,
            };
 
            byte[] data = Enumerable.Repeat<byte>(0x02, 100).ToArray();
            int sendToResult = LibcNativeMethods.SendTo(broadcastHandle, data, data.Length, MessageFlags.None, dstAddr, Marshal.SizeOf(typeof(SockAddrCanJ1939)));

            Assert.AreNotEqual(-1, sendToResult);
        }

        [Test]
        public void CAN_J1939_TP_BAM_RecvFrom_Success_Test()
        {
            int enable = 1;
            int result = LibcNativeMethods.SetSockOpt(ecuSocketHandle, SocketLevel.SOL_CAN_J1939, J1939SocketOptions.SO_J1939_PROMISC, ref enable, Marshal.SizeOf(typeof(int)));
            Assert.Greater(result, -1);

            var dstAddr = new SockAddrCanJ1939(0)
            {
                Name = SocketCanConstants.J1939_NO_NAME,
                PGN = 0x1F077, // R: 0, DP: 1, PF: Set to $F0, PS: $77
                Address = SocketCanConstants.J1939_NO_ADDR,
            };
 
            byte[] data = Enumerable.Repeat<byte>(0x0d, 100).ToArray();
            int sendToResult = LibcNativeMethods.SendTo(broadcastHandle, data, data.Length, MessageFlags.None, dstAddr, Marshal.SizeOf(typeof(SockAddrCanJ1939)));
            Assert.AreNotEqual(-1, sendToResult);

            var srcAddr = new SockAddrCanJ1939()
            {
                Name = SocketCanConstants.J1939_NO_NAME,
                PGN = SocketCanConstants.J1939_NO_PGN,
                Address = SocketCanConstants.J1939_NO_ADDR,
            };

            var recvData = new byte[100];
            int addrSize = Marshal.SizeOf(typeof(SockAddrCanJ1939));
            int recvFromResult = LibcNativeMethods.RecvFrom(ecuSocketHandle, recvData, recvData.Length, MessageFlags.None, srcAddr, ref addrSize);
            Assert.AreEqual(100, recvFromResult);
            Assert.AreEqual(0x99, srcAddr.Address);
            Assert.AreEqual(0x1f077, srcAddr.PGN);
            Assert.IsTrue(recvData.Take(recvFromResult).SequenceEqual(data));
        }

        [Test]
        public void CAN_J1939_ETP_Write_Success_Test()
        {
            byte[] data = Enumerable.Repeat<byte>(0x0a, 2000).ToArray();
            int writeResult = LibcNativeMethods.Write(testerSocketHandle, data, data.Length);
            Assert.AreNotEqual(-1, writeResult);
        }

        [Test]
        public void CAN_J1939_ETP_Read_Success_Test()
        {
            byte[] data = Enumerable.Repeat<byte>(0x08, 2000).ToArray();
            int writeResult = LibcNativeMethods.Write(testerSocketHandle, data, data.Length);
            Assert.AreNotEqual(-1, writeResult);

            var recvData = new byte[2000];
            int recvFromResult = LibcNativeMethods.Read(ecuSocketHandle, recvData, recvData.Length);
            Assert.AreEqual(2000, recvFromResult);
            Assert.IsTrue(recvData.Take(recvFromResult).SequenceEqual(data));
        }
    }
}