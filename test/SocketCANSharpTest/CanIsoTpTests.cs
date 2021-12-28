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
using System.Linq;

namespace SocketCANSharpTest
{
    public class CanIsoTpTests
    {
        SafeSocketHandle testerSocketHandle;
        SafeSocketHandle ecuSocketHandle;
        SafeSocketHandle extAddrTesterSocketHandle;
        SafeSocketHandle extAddrEcuSocketHandle;
        SafeSocketHandle fxnAddrTesterSocketHandle;
        SafeSocketHandle obdTester1SocketHandle;
        SafeSocketHandle obdEcu1SocketHandle;
        SafeSocketHandle obdTester2SocketHandle;
        SafeSocketHandle obdEcu2SocketHandle;

        [SetUp]
        public void Setup()
        {
            testerSocketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_ISOTP);
            Assert.IsFalse(testerSocketHandle.IsInvalid);

            var ifr = new Ifreq("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(testerSocketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
            Assert.AreNotEqual(-1, ioctlResult);

            var testerAddr = new SockAddrCanIsoTp(ifr.IfIndex)
            {
                TxId = 0x500,
                RxId = 0x600,
            };

            int bindResult = LibcNativeMethods.Bind(testerSocketHandle, testerAddr, Marshal.SizeOf(typeof(SockAddrCanIsoTp)));
            Assert.AreNotEqual(-1, bindResult);

            ecuSocketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_ISOTP);
            Assert.IsFalse(ecuSocketHandle.IsInvalid);

            var ecuAddr = new SockAddrCanIsoTp(ifr.IfIndex)
            {
                TxId = 0x600,
                RxId = 0x500,
            };

            bindResult = LibcNativeMethods.Bind(ecuSocketHandle, ecuAddr, Marshal.SizeOf(typeof(SockAddrCanIsoTp)));
            Assert.AreNotEqual(-1, bindResult);

            extAddrTesterSocketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_ISOTP);
            Assert.IsFalse(extAddrTesterSocketHandle.IsInvalid);;

            extAddrEcuSocketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_ISOTP);
            Assert.IsFalse(extAddrEcuSocketHandle.IsInvalid);

            var canIsoTpOpts = new CanIsoTpOptions()
            {
                Flags = IsoTpFlags.CAN_ISOTP_EXTEND_ADDR | IsoTpFlags.CAN_ISOTP_RX_EXT_ADDR,
                ExtendedAddress = 0xDD,
                RxExtendedAddress = 0xDD,
            };

            var extTesterAddr = new SockAddrCanIsoTp(ifr.IfIndex)
            {
                TxId = 0x500,
                RxId = 0x600,
            };

            var extEcuAddr = new SockAddrCanIsoTp(ifr.IfIndex)
            {
                TxId = 0x600,
                RxId = 0x500,
            };

            int result = LibcNativeMethods.SetSockOpt(extAddrTesterSocketHandle, SocketLevel.SOL_CAN_ISOTP, CanIsoTpSocketOptions.CAN_ISOTP_OPTS, canIsoTpOpts, Marshal.SizeOf(typeof(CanIsoTpOptions)));
            Assert.AreEqual(0, result);

            result = LibcNativeMethods.SetSockOpt(extAddrEcuSocketHandle, SocketLevel.SOL_CAN_ISOTP, CanIsoTpSocketOptions.CAN_ISOTP_OPTS, canIsoTpOpts, Marshal.SizeOf(typeof(CanIsoTpOptions)));
            Assert.AreEqual(0, result);

            bindResult = LibcNativeMethods.Bind(extAddrTesterSocketHandle, extTesterAddr, Marshal.SizeOf(typeof(SockAddrCanIsoTp)));
            Assert.AreNotEqual(-1, bindResult);

            bindResult = LibcNativeMethods.Bind(extAddrEcuSocketHandle, extEcuAddr, Marshal.SizeOf(typeof(SockAddrCanIsoTp)));
            Assert.AreNotEqual(-1, bindResult);

            fxnAddrTesterSocketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_ISOTP);
            Assert.IsFalse(fxnAddrTesterSocketHandle.IsInvalid);

            var fxnAddr = new SockAddrCanIsoTp(ifr.IfIndex)
            {
                TxId = 0x7df,
            };

            canIsoTpOpts = new CanIsoTpOptions()
            {
                Flags = IsoTpFlags.CAN_ISOTP_SF_BROADCAST,
            };

            result = LibcNativeMethods.SetSockOpt(fxnAddrTesterSocketHandle, SocketLevel.SOL_CAN_ISOTP, CanIsoTpSocketOptions.CAN_ISOTP_OPTS, canIsoTpOpts, Marshal.SizeOf(typeof(CanIsoTpOptions)));
            Assert.AreEqual(0, result);

            bindResult = LibcNativeMethods.Bind(fxnAddrTesterSocketHandle, fxnAddr, Marshal.SizeOf(typeof(SockAddrCanIsoTp)));
            Assert.AreNotEqual(-1, bindResult);

            obdEcu1SocketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_ISOTP);
            Assert.IsFalse(obdEcu1SocketHandle.IsInvalid);

            obdEcu2SocketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_ISOTP);
            Assert.IsFalse(obdEcu2SocketHandle.IsInvalid);

            obdTester1SocketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_ISOTP);
            Assert.IsFalse(obdTester1SocketHandle.IsInvalid);

            obdTester2SocketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_ISOTP);
            Assert.IsFalse(obdTester2SocketHandle.IsInvalid);

            var obdEcu1Addr = new SockAddrCanIsoTp(ifr.IfIndex)
            {
                TxId = 0x7e8,
                RxId = 0x7df,
            };

            bindResult = LibcNativeMethods.Bind(obdEcu1SocketHandle, obdEcu1Addr, Marshal.SizeOf(typeof(SockAddrCanIsoTp)));
            Assert.AreNotEqual(-1, bindResult);

            var odbTester1Addr = new SockAddrCanIsoTp(ifr.IfIndex)
            {
                TxId = 0x7df,
                RxId = 0x7e8,
            };

            bindResult = LibcNativeMethods.Bind(obdTester1SocketHandle, odbTester1Addr, Marshal.SizeOf(typeof(SockAddrCanIsoTp)));
            Assert.AreNotEqual(-1, bindResult);

            var obdEcu2Addr = new SockAddrCanIsoTp(ifr.IfIndex)
            {
                TxId = 0x7e9,
                RxId = 0x7df,
            };

            bindResult = LibcNativeMethods.Bind(obdEcu2SocketHandle, obdEcu2Addr, Marshal.SizeOf(typeof(SockAddrCanIsoTp)));
            Assert.AreNotEqual(-1, bindResult);

            var odbTester2Addr = new SockAddrCanIsoTp(ifr.IfIndex)
            {
                TxId = 0x7df,
                RxId = 0x7e9,
            };

            bindResult = LibcNativeMethods.Bind(obdTester2SocketHandle, odbTester2Addr, Marshal.SizeOf(typeof(SockAddrCanIsoTp)));
            Assert.AreNotEqual(-1, bindResult);
        }

        [TearDown]
        public void Cleanup()
        {
            testerSocketHandle.Close();
            ecuSocketHandle.Close();
            extAddrTesterSocketHandle.Close();
            extAddrEcuSocketHandle.Close();
            fxnAddrTesterSocketHandle.Close();
            obdEcu1SocketHandle.Close();
            obdTester1SocketHandle.Close();
            obdEcu2SocketHandle.Close();
            obdTester2SocketHandle.Close();
        }

        [Test]
        public void CAN_ISOTP_SingleFrame_ClassicCAN_Success_Test()
        {
            // Tester sends request
            var requestMessage = new byte[] { 0x22, 0xF4, 0x00 };
            int nBytes = LibcNativeMethods.Write(testerSocketHandle, requestMessage, requestMessage.Length);
            Assert.AreEqual(3, nBytes);

            // ECU reads request
            var receiveRequestMessage = new byte[4095];
            nBytes = LibcNativeMethods.Read(ecuSocketHandle, receiveRequestMessage, receiveRequestMessage.Length);
            Assert.AreEqual(3, nBytes);

            // ECU sends back response
            var responseMessage = new byte[] { 0x62, 0xF4, 0x00, 0x01, 0x02, 0x04, 0x08 };
            nBytes = LibcNativeMethods.Write(ecuSocketHandle, responseMessage, responseMessage.Length);
            Assert.AreEqual(7, nBytes);

            // Tester reads response
            var receiveResponseMessage = new byte[4095];
            nBytes = LibcNativeMethods.Read(testerSocketHandle, receiveResponseMessage, receiveResponseMessage.Length);
            Assert.AreEqual(7, nBytes);
        }

        [Test]
        public void CAN_ISOTP_MultiFrame_ClassicCAN_Success_Test()
        {
            // Tester sends request
            var requestMessage = new byte[] { 0x09, 0x02 };
            int nBytes = LibcNativeMethods.Write(testerSocketHandle, requestMessage, requestMessage.Length);
            Assert.AreEqual(2, nBytes);

            // ECU reads request
            var receiveRequestMessage = new byte[4095];
            nBytes = LibcNativeMethods.Read(ecuSocketHandle, receiveRequestMessage, receiveRequestMessage.Length);
            Assert.AreEqual(2, nBytes);

            // ECU sends back response
            var responseMessage = new byte[] { 0x49, 0x02, 0x01, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30  };
            nBytes = LibcNativeMethods.Write(ecuSocketHandle, responseMessage, responseMessage.Length);
            Assert.AreEqual(20, nBytes);

            // Tester reads response
            var receiveResponseMessage = new byte[4095];
            nBytes = LibcNativeMethods.Read(testerSocketHandle, receiveResponseMessage, receiveResponseMessage.Length);
            Assert.AreEqual(20, nBytes);
        }

        [Test]
        public void CAN_ISOTP_MultiFrame_ClassicCAN_EscapeSequence_Success_Test()
        {
            // Create a PDU that will be over 4095 bytes and therefore use escape sequence
            byte[] data = new byte[4094];
            foreach (int b in Enumerable.Range(0, 4094))
            {
                data[b] = (byte)(b & 0xff);
            }

            // Tester sends request
            var requestMessage = new byte[] { 0x36, 0x01 }.Concat(data).ToArray();
            int nBytes = LibcNativeMethods.Write(testerSocketHandle, requestMessage, requestMessage.Length);
            Assert.AreEqual(4096, nBytes);

            // ECU reads request
            var receiveRequestMessage = new byte[4096];
            nBytes = LibcNativeMethods.Read(ecuSocketHandle, receiveRequestMessage, receiveRequestMessage.Length);
            Assert.AreEqual(4096, nBytes);

            // ECU sends back response
            var responseMessage = new byte[] { 0x76, 0x01 };
            nBytes = LibcNativeMethods.Write(ecuSocketHandle, responseMessage, responseMessage.Length);
            Assert.AreEqual(2, nBytes);

            // Tester reads response
            var receiveResponseMessage = new byte[8190];
            nBytes = LibcNativeMethods.Read(testerSocketHandle, receiveResponseMessage, receiveResponseMessage.Length);
            Assert.AreEqual(2, nBytes);
        }

        [Test]
        public void CAN_ISOTP_ExtendedAddress_SingleFrame_ClassicCAN_Success_Test()
        {
            // Tester sends request
            var requestMessage = new byte[] { 0x22, 0xF4, 0x00 };
            int nBytes = LibcNativeMethods.Write(extAddrTesterSocketHandle, requestMessage, requestMessage.Length);
            Assert.AreEqual(3, nBytes);

            // ECU reads request
            var receiveRequestMessage = new byte[4095];
            nBytes = LibcNativeMethods.Read(extAddrEcuSocketHandle, receiveRequestMessage, receiveRequestMessage.Length);
            Assert.AreEqual(3, nBytes);

            // ECU sends back response
            var responseMessage = new byte[] { 0x62, 0xF4, 0x00, 0x01, 0x02, 0x04 };
            nBytes = LibcNativeMethods.Write(extAddrEcuSocketHandle, responseMessage, responseMessage.Length);
            Assert.AreEqual(6, nBytes);

            // Tester reads response
            var receiveResponseMessage = new byte[4095];
            nBytes = LibcNativeMethods.Read(extAddrTesterSocketHandle, receiveResponseMessage, receiveResponseMessage.Length);
            Assert.AreEqual(6, nBytes);
        }

        [Test]
        public void CAN_ISOTP_ExtendedAddress_MultiFrame_ClassicCAN_Success_Test()
        {
            // Tester sends request
            var requestMessage = new byte[] { 0x09, 0x02 };
            int nBytes = LibcNativeMethods.Write(extAddrTesterSocketHandle, requestMessage, requestMessage.Length);
            Assert.AreEqual(2, nBytes);

            // ECU reads request
            var receiveRequestMessage = new byte[4095];
            nBytes = LibcNativeMethods.Read(extAddrEcuSocketHandle, receiveRequestMessage, receiveRequestMessage.Length);
            Assert.AreEqual(2, nBytes);

            // ECU sends back response
            var responseMessage = new byte[] { 0x49, 0x02, 0x01, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30  };
            nBytes = LibcNativeMethods.Write(extAddrEcuSocketHandle, responseMessage, responseMessage.Length);
            Assert.AreEqual(20, nBytes);

            // Tester reads response
            var receiveResponseMessage = new byte[4095];
            nBytes = LibcNativeMethods.Read(extAddrTesterSocketHandle, receiveResponseMessage, receiveResponseMessage.Length);
            Assert.AreEqual(20, nBytes);
        }

        [Test]
        public void CAN_ISOTP_ExtendedAddress_MultiFrame_ClassicCAN_EscapeSequence_Success_Test()
        {
            // Create a PDU that will be over 4095 bytes and therefore use escape sequence
            byte[] data = new byte[4094];
            foreach (int b in Enumerable.Range(0, 4094))
            {
                data[b] = (byte)(b & 0xff);
            }

            // Tester sends request
            var requestMessage = new byte[] { 0x36, 0x01 }.Concat(data).ToArray();
            int nBytes = LibcNativeMethods.Write(extAddrTesterSocketHandle, requestMessage, requestMessage.Length);
            Assert.AreEqual(4096, nBytes);

            // ECU reads request
            var receiveRequestMessage = new byte[4096];
            nBytes = LibcNativeMethods.Read(extAddrEcuSocketHandle, receiveRequestMessage, receiveRequestMessage.Length);
            Assert.AreEqual(4096, nBytes);

            // ECU sends back response
            var responseMessage = new byte[] { 0x76, 0x01 };
            nBytes = LibcNativeMethods.Write(extAddrEcuSocketHandle, responseMessage, responseMessage.Length);
            Assert.AreEqual(2, nBytes);

            // Tester reads response
            var receiveResponseMessage = new byte[8190];
            nBytes = LibcNativeMethods.Read(extAddrTesterSocketHandle, receiveResponseMessage, receiveResponseMessage.Length);
            Assert.AreEqual(2, nBytes);
        }

        [Test]
        public void CAN_ISOTP_Functional_SingleFrame_ClassicCAN_Success_Test()
        {
            // Tester sends request
            var requestMessage = new byte[] { 0x3e, 0x80 };
            int nBytes = LibcNativeMethods.Write(fxnAddrTesterSocketHandle, requestMessage, requestMessage.Length);
            Assert.AreEqual(2, nBytes);
        }

        [Test]
        public void CAN_ISOTP_1_to_N_ClassicCAN_Success_Test()
        {
            // Tester sends functional request
            var requestMessage = new byte[] { 0x01, 0x00 };
            int nBytes = LibcNativeMethods.Write(fxnAddrTesterSocketHandle, requestMessage, requestMessage.Length);
            Assert.AreEqual(2, nBytes);

            // ECU 1 reads request
            var receiveRequestMessage = new byte[4095];
            nBytes = LibcNativeMethods.Read(obdEcu1SocketHandle, receiveRequestMessage, receiveRequestMessage.Length);
            Assert.AreEqual(2, nBytes);

            // ECU 1 sends back response
            var responseMessage = new byte[] { 0x41, 0x00, 0xff, 0xff, 0xff, 0xff };
            nBytes = LibcNativeMethods.Write(obdEcu1SocketHandle, responseMessage, responseMessage.Length);
            Assert.AreEqual(6, nBytes);

            // ECU 2 reads request
            receiveRequestMessage = new byte[4095];
            nBytes = LibcNativeMethods.Read(obdEcu2SocketHandle, receiveRequestMessage, receiveRequestMessage.Length);
            Assert.AreEqual(2, nBytes);

            // ECU 2 sends back response
            responseMessage = new byte[] { 0x41, 0x00, 0xff, 0xff, 0xff, 0xfe };
            nBytes = LibcNativeMethods.Write(obdEcu2SocketHandle, responseMessage, responseMessage.Length);
            Assert.AreEqual(6, nBytes);

            // Tester reads response #1
            var receiveResponseMessage = new byte[4095];
            nBytes = LibcNativeMethods.Read(obdTester1SocketHandle, receiveResponseMessage, receiveResponseMessage.Length);
            Assert.AreEqual(6, nBytes);

            // Tester reads response #2
            receiveResponseMessage = new byte[4095];
            nBytes = LibcNativeMethods.Read(obdTester2SocketHandle, receiveResponseMessage, receiveResponseMessage.Length);
            Assert.AreEqual(6, nBytes);
        }
    }
}