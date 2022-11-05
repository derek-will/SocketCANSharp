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
    public class CanFdIsoTpTests
    {
        SafeFileDescriptorHandle testerSocketHandle;
        SafeFileDescriptorHandle ecuSocketHandle;
        SafeFileDescriptorHandle extAddrTesterSocketHandle;
        SafeFileDescriptorHandle extAddrEcuSocketHandle;
        SafeFileDescriptorHandle fxnAddrTesterSocketHandle;
        SafeFileDescriptorHandle obdTester1SocketHandle;
        SafeFileDescriptorHandle obdEcu1SocketHandle;
        SafeFileDescriptorHandle obdTester2SocketHandle;
        SafeFileDescriptorHandle obdEcu2SocketHandle;

        [SetUp]
        public void Setup()
        {
            testerSocketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_ISOTP);
            Assert.IsFalse(testerSocketHandle.IsInvalid);

            var ifr = new Ifreq("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(testerSocketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
            Assert.AreNotEqual(-1, ioctlResult);

            // Set CAN FD
            var canIsoTpLlOpts = new CanIsoTpLinkLayerOptions(72, 64, CanFdFlags.CANFD_BRS);
            int sockOptResult = LibcNativeMethods.SetSockOpt(testerSocketHandle, SocketLevel.SOL_CAN_ISOTP, CanIsoTpSocketOptions.CAN_ISOTP_LL_OPTS, canIsoTpLlOpts, Marshal.SizeOf(typeof(CanIsoTpLinkLayerOptions)));
            Assert.AreEqual(0, sockOptResult);

            var testerAddr = new SockAddrCanIsoTp(ifr.IfIndex)
            {
                TxId = 0x500,
                RxId = 0x600,
            };

            int bindResult = LibcNativeMethods.Bind(testerSocketHandle, testerAddr, Marshal.SizeOf(typeof(SockAddrCanIsoTp)));
            Assert.AreNotEqual(-1, bindResult);

            ecuSocketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_ISOTP);
            Assert.IsFalse(ecuSocketHandle.IsInvalid);

            sockOptResult = LibcNativeMethods.SetSockOpt(ecuSocketHandle, SocketLevel.SOL_CAN_ISOTP, CanIsoTpSocketOptions.CAN_ISOTP_LL_OPTS, canIsoTpLlOpts, Marshal.SizeOf(typeof(CanIsoTpLinkLayerOptions)));
            Assert.AreEqual(0, sockOptResult);

            var ecuAddr = new SockAddrCanIsoTp(ifr.IfIndex)
            {
                TxId = 0x600,
                RxId = 0x500,
            };

            bindResult = LibcNativeMethods.Bind(ecuSocketHandle, ecuAddr, Marshal.SizeOf(typeof(SockAddrCanIsoTp)));
            Assert.AreNotEqual(-1, bindResult);

            extAddrTesterSocketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_ISOTP);
            Assert.IsFalse(extAddrTesterSocketHandle.IsInvalid);

            extAddrEcuSocketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_ISOTP);
            Assert.IsFalse(extAddrEcuSocketHandle.IsInvalid);

            var canIsoTpOpts = new CanIsoTpOptions()
            {
                Flags = IsoTpFlags.CAN_ISOTP_EXTEND_ADDR | IsoTpFlags.CAN_ISOTP_RX_EXT_ADDR,
                ExtendedAddress = 0xCC,
                RxExtendedAddress = 0xCC,
            };

            var extTesterAddr = new SockAddrCanIsoTp(ifr.IfIndex)
            {
                TxId = 0x510,
                RxId = 0x610,
            };

            var extEcuAddr = new SockAddrCanIsoTp(ifr.IfIndex)
            {
                TxId = 0x610,
                RxId = 0x510,
            };

            sockOptResult = LibcNativeMethods.SetSockOpt(extAddrTesterSocketHandle, SocketLevel.SOL_CAN_ISOTP, CanIsoTpSocketOptions.CAN_ISOTP_LL_OPTS, canIsoTpLlOpts, Marshal.SizeOf(typeof(CanIsoTpLinkLayerOptions)));
            Assert.AreEqual(0, sockOptResult);

            sockOptResult = LibcNativeMethods.SetSockOpt(extAddrEcuSocketHandle, SocketLevel.SOL_CAN_ISOTP, CanIsoTpSocketOptions.CAN_ISOTP_LL_OPTS, canIsoTpLlOpts, Marshal.SizeOf(typeof(CanIsoTpLinkLayerOptions)));
            Assert.AreEqual(0, sockOptResult);

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
                TxId = 0x18db33f1 | (uint)CanIdFlags.CAN_EFF_FLAG,
            };

            canIsoTpOpts = new CanIsoTpOptions()
            {
                Flags = IsoTpFlags.CAN_ISOTP_SF_BROADCAST | IsoTpFlags.CAN_ISOTP_WAIT_TX_DONE
            };

            result = LibcNativeMethods.SetSockOpt(fxnAddrTesterSocketHandle, SocketLevel.SOL_CAN_ISOTP, CanIsoTpSocketOptions.CAN_ISOTP_OPTS, canIsoTpOpts, Marshal.SizeOf(typeof(CanIsoTpOptions)));
            Assert.AreEqual(0, result);

            sockOptResult = LibcNativeMethods.SetSockOpt(fxnAddrTesterSocketHandle, SocketLevel.SOL_CAN_ISOTP, CanIsoTpSocketOptions.CAN_ISOTP_LL_OPTS, canIsoTpLlOpts, Marshal.SizeOf(typeof(CanIsoTpLinkLayerOptions)));
            Assert.AreEqual(0, sockOptResult);

            bindResult = LibcNativeMethods.Bind(fxnAddrTesterSocketHandle, fxnAddr, Marshal.SizeOf(typeof(SockAddrCanIsoTp)));
            Assert.AreNotEqual(-1, bindResult);

            obdEcu1SocketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_ISOTP);
            Assert.IsFalse(obdEcu1SocketHandle.IsInvalid);

            sockOptResult = LibcNativeMethods.SetSockOpt(obdEcu1SocketHandle, SocketLevel.SOL_CAN_ISOTP, CanIsoTpSocketOptions.CAN_ISOTP_LL_OPTS, canIsoTpLlOpts, Marshal.SizeOf(typeof(CanIsoTpLinkLayerOptions)));
            Assert.AreEqual(0, sockOptResult);

            obdEcu2SocketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_ISOTP);
            Assert.IsFalse(obdEcu2SocketHandle.IsInvalid);

            sockOptResult = LibcNativeMethods.SetSockOpt(obdEcu2SocketHandle, SocketLevel.SOL_CAN_ISOTP, CanIsoTpSocketOptions.CAN_ISOTP_LL_OPTS, canIsoTpLlOpts, Marshal.SizeOf(typeof(CanIsoTpLinkLayerOptions)));
            Assert.AreEqual(0, sockOptResult);

            obdTester1SocketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_ISOTP);
            Assert.IsFalse(obdTester1SocketHandle.IsInvalid);

            sockOptResult = LibcNativeMethods.SetSockOpt(obdTester1SocketHandle, SocketLevel.SOL_CAN_ISOTP, CanIsoTpSocketOptions.CAN_ISOTP_LL_OPTS, canIsoTpLlOpts, Marshal.SizeOf(typeof(CanIsoTpLinkLayerOptions)));
            Assert.AreEqual(0, sockOptResult);

            obdTester2SocketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_ISOTP);
            Assert.IsFalse(obdTester2SocketHandle.IsInvalid);

            sockOptResult = LibcNativeMethods.SetSockOpt(obdTester2SocketHandle, SocketLevel.SOL_CAN_ISOTP, CanIsoTpSocketOptions.CAN_ISOTP_LL_OPTS, canIsoTpLlOpts, Marshal.SizeOf(typeof(CanIsoTpLinkLayerOptions)));
            Assert.AreEqual(0, sockOptResult);

            var obdEcu1Addr = new SockAddrCanIsoTp(ifr.IfIndex)
            {
                TxId = 0x18daf101 | (uint)CanIdFlags.CAN_EFF_FLAG,
                RxId = 0x18db33f1 | (uint)CanIdFlags.CAN_EFF_FLAG,
            };

            bindResult = LibcNativeMethods.Bind(obdEcu1SocketHandle, obdEcu1Addr, Marshal.SizeOf(typeof(SockAddrCanIsoTp)));
            Assert.AreNotEqual(-1, bindResult);

            var odbTester1Addr = new SockAddrCanIsoTp(ifr.IfIndex)
            {
                TxId = 0x18db33f1 | (uint)CanIdFlags.CAN_EFF_FLAG,
                RxId = 0x18daf101 | (uint)CanIdFlags.CAN_EFF_FLAG,
            };

            bindResult = LibcNativeMethods.Bind(obdTester1SocketHandle, odbTester1Addr, Marshal.SizeOf(typeof(SockAddrCanIsoTp)));
            Assert.AreNotEqual(-1, bindResult);

            var obdEcu2Addr = new SockAddrCanIsoTp(ifr.IfIndex)
            {
                TxId = 0x18daf102 | (uint)CanIdFlags.CAN_EFF_FLAG,
                RxId = 0x18db33f1 | (uint)CanIdFlags.CAN_EFF_FLAG,
            };

            bindResult = LibcNativeMethods.Bind(obdEcu2SocketHandle, obdEcu2Addr, Marshal.SizeOf(typeof(SockAddrCanIsoTp)));
            Assert.AreNotEqual(-1, bindResult);

            var odbTester2Addr = new SockAddrCanIsoTp(ifr.IfIndex)
            {
                TxId = 0x18db33f1 | (uint)CanIdFlags.CAN_EFF_FLAG,
                RxId = 0x18daf102 | (uint)CanIdFlags.CAN_EFF_FLAG,
            };

            bindResult = LibcNativeMethods.Bind(obdTester2SocketHandle, odbTester2Addr, Marshal.SizeOf(typeof(SockAddrCanIsoTp)));
            Assert.AreNotEqual(-1, bindResult);
        }

        [TearDown]
        public void Cleanup()
        {
            // Note - if the system does not support CAN_ISOTP then the rest of the socket handles will not be initialized
            testerSocketHandle.Close();
            ecuSocketHandle?.Close();
            extAddrTesterSocketHandle?.Close();
            extAddrEcuSocketHandle?.Close();
            fxnAddrTesterSocketHandle?.Close();
            obdEcu1SocketHandle?.Close();
            obdTester1SocketHandle?.Close();
            obdEcu2SocketHandle?.Close();
            obdTester2SocketHandle?.Close();
        }
        
        [Test]
        public void CAN_ISOTP_SingleFrame_CANFD_Success_Test()
        {
            // Tester sends request - 8 Bytes which can fit into a single FD Frame
            var requestMessage = new byte[] { 0x31, 0x01, 0xFF, 0x01, 0xEF, 0xCD, 0x12, 0x56  };
            int nBytes = LibcNativeMethods.Write(testerSocketHandle, requestMessage, requestMessage.Length);
            Assert.AreEqual(8, nBytes);

            // ECU reads request
            var receiveRequestMessage = new byte[4095];
            nBytes = LibcNativeMethods.Read(ecuSocketHandle, receiveRequestMessage, receiveRequestMessage.Length);
            Assert.AreEqual(8, nBytes);

            // ECU sends back response
            var responseMessage = new byte[] { 0x71, 0x01, 0xFF, 0x01 };
            nBytes = LibcNativeMethods.Write(ecuSocketHandle, responseMessage, responseMessage.Length);
            Assert.AreEqual(4, nBytes);

            // Tester reads response
            var receiveResponseMessage = new byte[4095];
            nBytes = LibcNativeMethods.Read(testerSocketHandle, receiveResponseMessage, receiveResponseMessage.Length);
            Assert.AreEqual(4, nBytes);
        }

        [Test]
        public void CAN_ISOTP_MultiFrame_CANFD_Success_Test()
        {
            byte[] data = new byte[4093];
            foreach (int b in Enumerable.Range(0, 4093))
            {
                data[b] = (byte)(b & 0xff);
            }

            // Tester sends request
            var requestMessage = new byte[] { 0x36, 0x01 }.Concat(data).ToArray();
            int nBytes = LibcNativeMethods.Write(testerSocketHandle, requestMessage, requestMessage.Length);
            Assert.AreEqual(4095, nBytes);

            // ECU reads request
            var receiveRequestMessage = new byte[4095];
            nBytes = LibcNativeMethods.Read(ecuSocketHandle, receiveRequestMessage, receiveRequestMessage.Length);
            Assert.AreEqual(4095, nBytes);

            // ECU sends back response
            var responseMessage = new byte[] { 0x76, 0x01 };
            nBytes = LibcNativeMethods.Write(ecuSocketHandle, responseMessage, responseMessage.Length);
            Assert.AreEqual(2, nBytes);

            // Tester reads response
            var receiveResponseMessage = new byte[4095];
            nBytes = LibcNativeMethods.Read(testerSocketHandle, receiveResponseMessage, receiveResponseMessage.Length);
            Assert.AreEqual(2, nBytes);
        }

        [Test]
        public void CAN_ISOTP_MultiFrame_CANFD_EscapeSequence_Success_Test()
        {
            // create message that will 8190 bytes in size (double the old limit of ISO-TP)
            byte[] data = new byte[8188];
            foreach (int b in Enumerable.Range(0, 8188))
            {
                data[b] = (byte)(b & 0xff);
            }

            // Tester sends request
            var requestMessage = new byte[] { 0x36, 0x01 }.Concat(data).ToArray();
            int nBytes = LibcNativeMethods.Write(testerSocketHandle, requestMessage, requestMessage.Length);
            Assert.AreEqual(8190, nBytes);

            // ECU reads request
            var receiveRequestMessage = new byte[8190];
            nBytes = LibcNativeMethods.Read(ecuSocketHandle, receiveRequestMessage, receiveRequestMessage.Length);
            Assert.AreEqual(8190, nBytes);

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
        public void CAN_ISOTP_ExtendedAddress_SingleFrame_CANFD_Success_Test()
        {
            // Tester sends request - 8 Bytes which can fit into a single FD Frame
            var requestMessage = new byte[] { 0x31, 0x01, 0xFF, 0x01, 0xEF, 0xCD, 0x12, 0x56  };
            int nBytes = LibcNativeMethods.Write(extAddrTesterSocketHandle, requestMessage, requestMessage.Length);
            Assert.AreEqual(8, nBytes);

            // ECU reads request
            var receiveRequestMessage = new byte[4095];
            nBytes = LibcNativeMethods.Read(extAddrEcuSocketHandle, receiveRequestMessage, receiveRequestMessage.Length);
            Assert.AreEqual(8, nBytes);

            // ECU sends back response
            var responseMessage = new byte[] { 0x71, 0x01, 0xFF, 0x01 };
            nBytes = LibcNativeMethods.Write(extAddrEcuSocketHandle, responseMessage, responseMessage.Length);
            Assert.AreEqual(4, nBytes);

            // Tester reads response
            var receiveResponseMessage = new byte[4095];
            nBytes = LibcNativeMethods.Read(extAddrTesterSocketHandle, receiveResponseMessage, receiveResponseMessage.Length);
            Assert.AreEqual(4, nBytes);
        }

        [Test]
        public void CAN_ISOTP_ExtendedAddress_MultiFrame_CANFD_Success_Test()
        {
            byte[] data = new byte[4092];
            foreach (int b in Enumerable.Range(0, 4092))
            {
                data[b] = (byte)(b & 0xff);
            }

            // Tester sends request
            var requestMessage = new byte[] { 0x36, 0x01 }.Concat(data).ToArray();
            int nBytes = LibcNativeMethods.Write(extAddrTesterSocketHandle, requestMessage, requestMessage.Length);
            Assert.AreEqual(4094, nBytes);

            // ECU reads request
            var receiveRequestMessage = new byte[4095];
            nBytes = LibcNativeMethods.Read(extAddrEcuSocketHandle, receiveRequestMessage, receiveRequestMessage.Length);
            Assert.AreEqual(4094, nBytes);

            // ECU sends back response
            var responseMessage = new byte[] { 0x76, 0x01 };
            nBytes = LibcNativeMethods.Write(extAddrEcuSocketHandle, responseMessage, responseMessage.Length);
            Assert.AreEqual(2, nBytes);

            // Tester reads response
            var receiveResponseMessage = new byte[4095];
            nBytes = LibcNativeMethods.Read(extAddrTesterSocketHandle, receiveResponseMessage, receiveResponseMessage.Length);
            Assert.AreEqual(2, nBytes);
        }

        [Test]
        public void CAN_ISOTP_ExtendedAddress_MultiFrame_CANFD_EscapeSequence_Success_Test()
        {
            // create message that will 8190 bytes in size (double the old limit of ISO-TP)
            byte[] data = new byte[8188];
            foreach (int b in Enumerable.Range(0, 8188))
            {
                data[b] = (byte)(b & 0xff);
            }

            // Tester sends request
            var requestMessage = new byte[] { 0x36, 0x01 }.Concat(data).ToArray();
            int nBytes = LibcNativeMethods.Write(extAddrTesterSocketHandle, requestMessage, requestMessage.Length);
            Assert.AreEqual(8190, nBytes);

            // ECU reads request
            var receiveRequestMessage = new byte[8190];
            nBytes = LibcNativeMethods.Read(extAddrEcuSocketHandle, receiveRequestMessage, receiveRequestMessage.Length);
            Assert.AreEqual(8190, nBytes);

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
        public void CAN_ISOTP_Functional_SingleFrame_CANFD_Success_Test()
        {
            // Tester sends request
            var requestMessage = new byte[] { 0x01, 0x00  };
            int nBytes = LibcNativeMethods.Write(fxnAddrTesterSocketHandle, requestMessage, requestMessage.Length);
            Assert.AreEqual(2, nBytes);
        }
        
        [Test]
        public void CAN_ISOTP_1_to_N_CANFD_Success_Test()
        {
            // Tester sends functional request
            var requestMessage = new byte[] { 0x3e, 0x00 };
            int nBytes = LibcNativeMethods.Write(fxnAddrTesterSocketHandle, requestMessage, requestMessage.Length);
            Assert.AreEqual(2, nBytes);

            // ECU 1 reads request
            var receiveRequestMessage = new byte[4095];
            nBytes = LibcNativeMethods.Read(obdEcu1SocketHandle, receiveRequestMessage, receiveRequestMessage.Length);
            Assert.AreEqual(2, nBytes);

            // ECU 1 sends back response
            var responseMessage = new byte[] { 0x7e, 0x00 };
            nBytes = LibcNativeMethods.Write(obdEcu1SocketHandle, responseMessage, responseMessage.Length);
            Assert.AreEqual(2, nBytes);

            // ECU 2 reads request
            receiveRequestMessage = new byte[4095];
            nBytes = LibcNativeMethods.Read(obdEcu2SocketHandle, receiveRequestMessage, receiveRequestMessage.Length);
            Assert.AreEqual(2, nBytes);

            // ECU 2 sends back response
            responseMessage = new byte[] { 0x7e, 0x00 };
            nBytes = LibcNativeMethods.Write(obdEcu2SocketHandle, responseMessage, responseMessage.Length);
            Assert.AreEqual(2, nBytes);

            // Tester reads response #1
            var receiveResponseMessage = new byte[4095];
            nBytes = LibcNativeMethods.Read(obdTester1SocketHandle, receiveResponseMessage, receiveResponseMessage.Length);
            Assert.AreEqual(2, nBytes);

            // Tester reads response #2
            receiveResponseMessage = new byte[4095];
            nBytes = LibcNativeMethods.Read(obdTester2SocketHandle, receiveResponseMessage, receiveResponseMessage.Length);
            Assert.AreEqual(2, nBytes);
        }
    }
}