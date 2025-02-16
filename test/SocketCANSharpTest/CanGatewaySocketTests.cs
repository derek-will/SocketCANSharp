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
using System;
using SocketCANSharp.Network;
using SocketCANSharp.Network.Netlink;
using SocketCANSharp.Capabilities;
using SocketCANSharp.Network.Netlink.Gateway;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;

namespace SocketCANSharpTest
{
    public class CanGatewaySocketTests
    {
        [Test]
        public void CanGatewaySocket_Ctor_Success_Test()
        {
            using (var cgwSocket = new CanGatewaySocket())
            {
                cgwSocket.ReceiveTimeout = 1000;

                Assert.AreEqual(true, cgwSocket.Blocking);
                Assert.AreEqual(false, cgwSocket.Connected);
                Assert.AreEqual(false, cgwSocket.EnableBroadcast);
                Assert.AreNotEqual(IntPtr.Zero, cgwSocket.Handle);
                Assert.AreEqual(false, cgwSocket.IsBound);
                Assert.AreEqual(NetlinkProtocolType.NETLINK_ROUTE, cgwSocket.ProtocolType);
                Assert.NotNull(cgwSocket.SafeHandle);
                Assert.AreEqual(false, cgwSocket.SafeHandle.IsInvalid);
                Assert.AreEqual(SocketType.Raw, cgwSocket.SocketType);
                Assert.AreEqual(1000, cgwSocket.ReceiveTimeout);
            }
        }

        [Test]
        public void CanGatewaySocket_Bind_Success_Test()
        {
            using (var cgwSocket = new CanGatewaySocket())
            {
                cgwSocket.Bind(new SockAddrNetlink(0, 0));
                Assert.AreEqual(true, cgwSocket.IsBound);
            }
        }

        [Test]
        public void CanGatewaySocket_Address_Property_Success_Test()
        {
            using (var cgwSocket = new CanGatewaySocket())
            {
                cgwSocket.Bind(new SockAddrNetlink(0, 0));
                Assert.AreEqual(true, cgwSocket.IsBound);
                SockAddrNetlink addr = cgwSocket.Address;
                Assert.NotNull(addr);
                Assert.AreEqual(NetlinkConstants.AF_NETLINK, addr.NetlinkFamily);
                Assert.AreEqual(Environment.ProcessId, addr.PortId);
                Assert.AreEqual(0, addr.GroupsMask);
            }
        }

        [Test]
        public void CanGatewaySocket_AddOrUpdateCanToCanRule_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 2);

            var srcIf = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            var dstIf = collection.FirstOrDefault(i =>  i.Name.Equals("vcan1"));
            Assert.IsNotNull(srcIf);
            Assert.IsNotNull(dstIf);

            using (var cgwSocket = new CanGatewaySocket())
            {
                cgwSocket.ReceiveTimeout = 1000;
                cgwSocket.Bind(new SockAddrNetlink(0, 0));
                Assert.AreEqual(true, cgwSocket.IsBound);

                var rule = new CgwCanToCanRule(CgwCanFrameType.ClassicalCAN)
                {
                    SourceIndex = (uint)srcIf.Index,
                    DestinationIndex = (uint)dstIf.Index,
                };

                int reqLen = cgwSocket.AddOrUpdateCanToCanRule(rule);

                byte[] rxBuffer = new byte[8192];
                int numBytes = cgwSocket.Read(rxBuffer);
                Assert.GreaterOrEqual(numBytes, 16);
                NetlinkMessageHeader header = NetlinkUtils.PeekAtHeader(rxBuffer);
                Assert.AreEqual(NetlinkMessageType.NLMSG_ERROR, header.MessageType);
                
                NetlinkMessageError nlMsgErr = NetlinkMessageError.FromBytes(rxBuffer);
                Assert.AreEqual(NetlinkMessageType.NLMSG_ERROR, nlMsgErr.MessageHeader.MessageType);
                Assert.AreEqual((uint)System.Environment.ProcessId, nlMsgErr.MessageHeader.SenderPortId); // process ID for the sender
                Assert.AreEqual((uint)0, nlMsgErr.MessageHeader.SequenceNumber);

                bool isCapable = CapabilityUtils.IsCurrentProcessCapable(Capability.CAP_NET_ADMIN);
                if (isCapable)
                {
                    Assert.AreEqual(0, nlMsgErr.Error); // Success
                    Assert.AreEqual(NetlinkMessageFlags.NLM_F_ROOT, nlMsgErr.MessageHeader.Flags);
                    Assert.AreEqual(36, nlMsgErr.MessageHeader.MessageLength); // 16 for original message header + 4 for error, 16 for header
                }
                else
                {
                    Assert.AreEqual(-1, nlMsgErr.Error); // EPERM
                    Assert.AreEqual((NetlinkMessageFlags)0, nlMsgErr.MessageHeader.Flags);
                    Assert.AreEqual(reqLen + 20, nlMsgErr.MessageHeader.MessageLength); // complete original message + 4 for error, 16 for header
                }
            }
        }

        [Test]
        public void CanGatewaySocket_RemoveAllCanToCanRules_Success_Test()
        {
            using (var cgwSocket = new CanGatewaySocket())
            {
                cgwSocket.ReceiveTimeout = 1000;
                cgwSocket.Bind(new SockAddrNetlink(0, 0));
                Assert.AreEqual(true, cgwSocket.IsBound);

                int reqLen = cgwSocket.RemoveAllCanToCanRules();

                byte[] rxBuffer = new byte[8192];
                int numBytes = cgwSocket.Read(rxBuffer);
                Assert.GreaterOrEqual(numBytes, 16);
                NetlinkMessageHeader header = NetlinkUtils.PeekAtHeader(rxBuffer);
                Assert.AreEqual(NetlinkMessageType.NLMSG_ERROR, header.MessageType);

                NetlinkMessageError nlMsgErr = NetlinkMessageError.FromBytes(rxBuffer);
                Assert.AreEqual(NetlinkMessageType.NLMSG_ERROR, nlMsgErr.MessageHeader.MessageType);
                Assert.AreEqual((uint)System.Environment.ProcessId, nlMsgErr.MessageHeader.SenderPortId); // process ID for the sender
                Assert.AreEqual((uint)0, nlMsgErr.MessageHeader.SequenceNumber);

                bool isCapable = CapabilityUtils.IsCurrentProcessCapable(Capability.CAP_NET_ADMIN);
                if (isCapable)
                {
                    Assert.AreEqual(0, nlMsgErr.Error); // Success
                    Assert.AreEqual(NetlinkMessageFlags.NLM_F_ROOT, nlMsgErr.MessageHeader.Flags);
                    Assert.AreEqual(36, nlMsgErr.MessageHeader.MessageLength); // 16 for original message header + 4 for error, 16 for header
                }
                else
                {
                    Assert.AreEqual(-1, nlMsgErr.Error); // EPERM
                    Assert.AreEqual((NetlinkMessageFlags)0, nlMsgErr.MessageHeader.Flags);
                    Assert.AreEqual(reqLen + 20, nlMsgErr.MessageHeader.MessageLength); // complete original message + 4 for error, 16 for header
                }
            }
        }

        [Test]
        public void CanGatewaySocket_RemoveCanToCanRule_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 2);

            var srcIf = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            var dstIf = collection.FirstOrDefault(i =>  i.Name.Equals("vcan1"));
            Assert.IsNotNull(srcIf);
            Assert.IsNotNull(dstIf);

            using (var cgwSocket = new CanGatewaySocket())
            {
                cgwSocket.ReceiveTimeout = 1000;
                cgwSocket.Bind(new SockAddrNetlink(0, 0));
                Assert.AreEqual(true, cgwSocket.IsBound);

                var rule = new CgwCanToCanRule(CgwCanFrameType.ClassicalCAN)
                {
                    SourceIndex = (uint)srcIf.Index,
                    DestinationIndex = (uint)dstIf.Index,
                };

                int reqLen = cgwSocket.AddOrUpdateCanToCanRule(rule);

                byte[] rxBuffer = new byte[8192];
                int numBytes = cgwSocket.Read(rxBuffer);
                Assert.GreaterOrEqual(numBytes, 16);
                NetlinkMessageHeader header = NetlinkUtils.PeekAtHeader(rxBuffer);
                Assert.AreEqual(NetlinkMessageType.NLMSG_ERROR, header.MessageType);
                
                NetlinkMessageError nlMsgErr = NetlinkMessageError.FromBytes(rxBuffer);
                Assert.AreEqual(NetlinkMessageType.NLMSG_ERROR, nlMsgErr.MessageHeader.MessageType);
                Assert.AreEqual((uint)System.Environment.ProcessId, nlMsgErr.MessageHeader.SenderPortId); // process ID for the sender
                Assert.AreEqual((uint)0, nlMsgErr.MessageHeader.SequenceNumber);

                bool isCapable = CapabilityUtils.IsCurrentProcessCapable(Capability.CAP_NET_ADMIN);
                if (isCapable)
                {
                    Assert.AreEqual(0, nlMsgErr.Error); // Success
                    Assert.AreEqual(NetlinkMessageFlags.NLM_F_ROOT, nlMsgErr.MessageHeader.Flags);
                    Assert.AreEqual(36, nlMsgErr.MessageHeader.MessageLength); // 16 for original message header + 4 for error, 16 for header
                }
                else
                {
                    Assert.AreEqual(-1, nlMsgErr.Error); // EPERM
                    Assert.AreEqual((NetlinkMessageFlags)0, nlMsgErr.MessageHeader.Flags);
                    Assert.AreEqual(reqLen + 20, nlMsgErr.MessageHeader.MessageLength); // complete original message + 4 for error, 16 for header
                }

                reqLen = cgwSocket.RemoveCanToCanRule(rule);

                rxBuffer = new byte[8192];
                numBytes = cgwSocket.Read(rxBuffer);
                Assert.GreaterOrEqual(numBytes, 16);
                header = NetlinkUtils.PeekAtHeader(rxBuffer);
                Assert.AreEqual(NetlinkMessageType.NLMSG_ERROR, header.MessageType);
                
                nlMsgErr = NetlinkMessageError.FromBytes(rxBuffer);
                Assert.AreEqual(NetlinkMessageType.NLMSG_ERROR, nlMsgErr.MessageHeader.MessageType);
                Assert.AreEqual((uint)System.Environment.ProcessId, nlMsgErr.MessageHeader.SenderPortId); // process ID for the sender
                Assert.AreEqual((uint)0, nlMsgErr.MessageHeader.SequenceNumber);

                isCapable = CapabilityUtils.IsCurrentProcessCapable(Capability.CAP_NET_ADMIN);
                if (isCapable)
                {
                    Assert.AreEqual(0, nlMsgErr.Error); // Success
                    Assert.AreEqual(NetlinkMessageFlags.NLM_F_ROOT, nlMsgErr.MessageHeader.Flags);
                    Assert.AreEqual(36, nlMsgErr.MessageHeader.MessageLength); // 16 for original message header + 4 for error, 16 for header
                }
                else
                {
                    Assert.AreEqual(-1, nlMsgErr.Error); // EPERM
                    Assert.AreEqual((NetlinkMessageFlags)0, nlMsgErr.MessageHeader.Flags);
                    Assert.AreEqual(reqLen + 20, nlMsgErr.MessageHeader.MessageLength); // complete original message + 4 for error, 16 for header
                }
            }
        }

        [Test]
        public void CanGatewaySocket_RequestListOfAllRules_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 2);

            var srcIf = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            var dstIf = collection.FirstOrDefault(i =>  i.Name.Equals("vcan1"));
            Assert.IsNotNull(srcIf);
            Assert.IsNotNull(dstIf);

            using (var cgwSocket = new CanGatewaySocket())
            {
                cgwSocket.ReceiveTimeout = 1000;
                cgwSocket.Bind(new SockAddrNetlink(0, 0));
                Assert.AreEqual(true, cgwSocket.IsBound);

                var rule = new CgwCanToCanRule(CgwCanFrameType.ClassicalCAN)
                {
                    SourceIndex = (uint)srcIf.Index,
                    DestinationIndex = (uint)dstIf.Index,
                };

                int reqLen = cgwSocket.AddOrUpdateCanToCanRule(rule);

                byte[] rxBuffer = new byte[8192];
                int numBytes = cgwSocket.Read(rxBuffer);
                Assert.GreaterOrEqual(numBytes, 16);
                NetlinkMessageHeader header = NetlinkUtils.PeekAtHeader(rxBuffer);
                Assert.AreEqual(NetlinkMessageType.NLMSG_ERROR, header.MessageType);
                
                NetlinkMessageError nlMsgErr = NetlinkMessageError.FromBytes(rxBuffer);
                Assert.AreEqual(NetlinkMessageType.NLMSG_ERROR, nlMsgErr.MessageHeader.MessageType);
                Assert.AreEqual((uint)System.Environment.ProcessId, nlMsgErr.MessageHeader.SenderPortId); // process ID for the sender
                Assert.AreEqual((uint)0, nlMsgErr.MessageHeader.SequenceNumber);

                bool isCapable = CapabilityUtils.IsCurrentProcessCapable(Capability.CAP_NET_ADMIN);
                if (isCapable)
                {
                    Assert.AreEqual(0, nlMsgErr.Error); // Success
                    Assert.AreEqual(NetlinkMessageFlags.NLM_F_ROOT, nlMsgErr.MessageHeader.Flags);
                    Assert.AreEqual(36, nlMsgErr.MessageHeader.MessageLength); // 16 for original message header + 4 for error, 16 for header
                }
                else
                {
                    Assert.AreEqual(-1, nlMsgErr.Error); // EPERM
                    Assert.AreEqual((NetlinkMessageFlags)0, nlMsgErr.MessageHeader.Flags);
                    Assert.AreEqual(reqLen + 20, nlMsgErr.MessageHeader.MessageLength); // complete original message + 4 for error, 16 for header
                }

                reqLen = cgwSocket.RequestListOfAllRules();
                var ruleList = new List<CgwCanToCanRule>();
                bool keepReading = true;
                do
                {
                    rxBuffer = new byte[8192];
                    numBytes = cgwSocket.Read(rxBuffer);
                    byte[] filledBuffer = rxBuffer.Take(numBytes).ToArray();
                    ruleList.AddRange(cgwSocket.ParseCanToCanRules(filledBuffer, out keepReading));
                }
                while (keepReading);

                Assert.IsFalse(keepReading);
                Assert.NotNull(ruleList);
            }
        }

        [Test]
        public void CanGatewaySocket_ParseCanToCanRules_MultipleMessagesInBuffer_Tests()
        {
            using (var cgwSocket = new CanGatewaySocket())
            {
                cgwSocket.ReceiveTimeout = 1000;
                cgwSocket.Bind(new SockAddrNetlink(0, 0));
                Assert.AreEqual(true, cgwSocket.IsBound);

                byte[] buffer = new byte[] { 0x60, 0x00, 0x00, 0x00, 0x18, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x2D, 0x0F, 0x00, 0x00,
                0x1D, 0x01, 0x01, 0x00, 0x08, 0x00, 0x07, 0x00, 0x3D, 0x00, 0x00, 0x00, 0x15, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x05,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x08, 0x00, 0x0E, 0x00, 0xDD, 0xEE,
                0xEE, 0xFF, 0x08, 0x00, 0x05, 0x00, 0x00, 0x03, 0x04, 0xCC, 0x0C, 0x00, 0x0B, 0x00, 0x23, 0x01, 0x00, 0x00, 0xFF, 0x07, 0x00,
                0x00, 0x08, 0x00, 0x09, 0x00, 0x05, 0x00, 0x00, 0x00, 0x08, 0x00, 0x0A, 0x00, 0x06, 0x00, 0x00, 0x00, 0x2C, 0x00, 0x00, 0x00,
                0x18, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x2D, 0x0F, 0x00, 0x00, 0x1D, 0x01, 0x03, 0x00, 0x08, 0x00, 0x07, 0x00, 0x47,
                0x30, 0x00, 0x00, 0x08, 0x00, 0x09, 0x00, 0x05, 0x00, 0x00, 0x00, 0x08, 0x00, 0x0A, 0x00, 0x06, 0x00, 0x00, 0x00 };
                List<CgwCanToCanRule> rules = cgwSocket.ParseCanToCanRules(buffer, out bool keepReading);
                Assert.IsNotNull(rules);
                Assert.AreEqual(rules.Count, 2);
                foreach (var rule in rules)
                {
                    Console.WriteLine("***");
                    Console.Write(rule);
                    Console.WriteLine("***");
                }

            }
        }
    }
}