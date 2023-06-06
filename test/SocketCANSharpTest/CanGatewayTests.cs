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
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using NUnit.Framework;
using SocketCANSharp;
using SocketCANSharp.Network;
using SocketCANSharp.Network.Netlink;
using SocketCANSharp.Network.Netlink.Gateway;
using System.Net.Sockets;
using SocketCANSharp.Capabilities;

namespace SocketCANSharpTest
{
    public class CanGatewayTests
    {
        SafeFileDescriptorHandle socketHandle;

        [SetUp]
        public void Setup()
        {
            socketHandle = NetlinkNativeMethods.Socket(NetlinkConstants.PF_NETLINK, SocketType.Raw, NetlinkProtocolType.NETLINK_ROUTE);
            Assert.IsFalse(socketHandle.IsInvalid);

            int sendBufferSize = 32768;
            int recvBufferSize = 32768;

            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_SOCKET, SocketLevelOptions.SO_SNDBUF, ref sendBufferSize, Marshal.SizeOf(typeof(int)));
            Assert.AreEqual(0, result);

            result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_SOCKET, SocketLevelOptions.SO_RCVBUF, ref recvBufferSize, Marshal.SizeOf(typeof(int)));
            Assert.AreEqual(0, result);

            var timeval = new Timeval(1, 0);
            result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_SOCKET, SocketLevelOptions.SO_RCVTIMEO, timeval, Marshal.SizeOf(typeof(Timeval)));
            Assert.AreEqual(0, result);

            var addr = new SockAddrNetlink(0, 0);
            int bindResult = NetlinkNativeMethods.Bind(socketHandle, addr, Marshal.SizeOf(typeof(SockAddrNetlink)));
            Assert.AreNotEqual(-1, bindResult);
        }

        [TearDown]
        public void Cleanup()
        {
            socketHandle.Close();
        }

        [Test]
        public void CAN_GW_RoutingCanMessage_Verification_Test()
        {
            var canRtMsg = new RoutingCanMessage();
            canRtMsg.CanFamily = SocketCanConstants.AF_CAN;
            canRtMsg.GatewayType = CanGatewayType.CGW_TYPE_CAN_CAN;
            canRtMsg.GatewayFlags = CanGatewayFlag.CGW_FLAGS_CAN_ECHO | CanGatewayFlag.CGW_FLAGS_CAN_SRC_TSTAMP;
            Assert.AreEqual((byte)SocketCanConstants.AF_CAN, canRtMsg.CanFamily);
            Assert.AreEqual(CanGatewayType.CGW_TYPE_CAN_CAN, canRtMsg.GatewayType);
            Assert.IsTrue(canRtMsg.GatewayFlags.HasFlag(CanGatewayFlag.CGW_FLAGS_CAN_ECHO));
            Assert.IsTrue(canRtMsg.GatewayFlags.HasFlag(CanGatewayFlag.CGW_FLAGS_CAN_SRC_TSTAMP));
            Assert.IsFalse(canRtMsg.GatewayFlags.HasFlag(CanGatewayFlag.CGW_FLAGS_CAN_FD));
            Assert.IsFalse(canRtMsg.GatewayFlags.HasFlag(CanGatewayFlag.CGW_FLAGS_CAN_IIF_TX_OK));
            Assert.AreEqual(4, Marshal.SizeOf<RoutingCanMessage>());
        }

        [Test]
        public void CAN_GW_RoutingCanMessage_ToString_Test()
        {
            var canRtMsg = new RoutingCanMessage();
            canRtMsg.CanFamily = SocketCanConstants.AF_CAN;
            canRtMsg.GatewayType = CanGatewayType.CGW_TYPE_CAN_CAN;
            canRtMsg.GatewayFlags = CanGatewayFlag.CGW_FLAGS_CAN_ECHO | CanGatewayFlag.CGW_FLAGS_CAN_SRC_TSTAMP;
            Assert.AreEqual((byte)SocketCanConstants.AF_CAN, canRtMsg.CanFamily);
            Assert.AreEqual(CanGatewayType.CGW_TYPE_CAN_CAN, canRtMsg.GatewayType);
            Assert.IsTrue(canRtMsg.GatewayFlags.HasFlag(CanGatewayFlag.CGW_FLAGS_CAN_ECHO));
            Assert.IsTrue(canRtMsg.GatewayFlags.HasFlag(CanGatewayFlag.CGW_FLAGS_CAN_SRC_TSTAMP));
            Assert.IsFalse(canRtMsg.GatewayFlags.HasFlag(CanGatewayFlag.CGW_FLAGS_CAN_FD));
            Assert.IsFalse(canRtMsg.GatewayFlags.HasFlag(CanGatewayFlag.CGW_FLAGS_CAN_IIF_TX_OK));;
            string str = canRtMsg.ToString();
            Assert.IsNotNull(str);
            Console.WriteLine(str);
        }

        [Test]
        public void CAN_GW_Add_Routing_Entry_Simple_RTM_NEWROUTE_Test()
        {
            var req = new CanGatewayRequest()
            {
                Header = new NetlinkMessageHeader()
                {
                    MessageLength = (uint)NetlinkMessageMacros.NLMSG_LENGTH(Marshal.SizeOf(typeof(RoutingCanMessage))),
                    MessageType = NetlinkMessageType.RTM_NEWROUTE,
                    Flags = NetlinkMessageFlags.NLM_F_REQUEST | NetlinkMessageFlags.NLM_F_ACK,
                    SenderPortId = 0,
                    SequenceNumber = 0,
                },
                Message = new RoutingCanMessage()
                {
                    CanFamily = SocketCanConstants.AF_CAN,
                    GatewayType = CanGatewayType.CGW_TYPE_CAN_CAN,
                    GatewayFlags = CanGatewayFlag.CGW_FLAGS_CAN_ECHO | CanGatewayFlag.CGW_FLAGS_CAN_SRC_TSTAMP,
                }
            };

            CanNetworkInterface srcInterface = CanNetworkInterface.GetInterfaceByName(socketHandle, "vcan0");
            CanNetworkInterface dstInterface = CanNetworkInterface.GetInterfaceByName(socketHandle, "vcan1");
            req.AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_SRC_IF, NetlinkUtils.ToBytes<uint>((uint)srcInterface.Index)));
            req.AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_DST_IF, NetlinkUtils.ToBytes<uint>((uint)dstInterface.Index)));
            int numBytes = CanGatewayNativeMethods.Write(socketHandle, req, (int)req.Header.MessageLength);
            Assert.AreEqual((int)req.Header.MessageLength, numBytes);

            byte[] rxBuffer = new byte[8192];
            numBytes = LibcNativeMethods.Read(socketHandle, rxBuffer, rxBuffer.Length);
            Assert.AreNotEqual(-1, numBytes, $"Errno: {LibcNativeMethods.Errno}");
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
                Assert.AreEqual(req.Header.MessageLength + 20, nlMsgErr.MessageHeader.MessageLength); // complete original message + 4 for error, 16 for header
            }
            Assert.AreEqual(req.Header.MessageLength, nlMsgErr.ErrorMessageHeader.MessageLength);
            Assert.AreEqual(req.Header.MessageType, nlMsgErr.ErrorMessageHeader.MessageType);
            Assert.AreEqual(req.Header.Flags, nlMsgErr.ErrorMessageHeader.Flags);
            Assert.AreEqual(req.Header.SenderPortId, nlMsgErr.ErrorMessageHeader.SenderPortId);
            Assert.AreEqual(req.Header.SequenceNumber, nlMsgErr.ErrorMessageHeader.SequenceNumber);
        }

        [Test]
        public void CAN_GW_Add_Routing_Entry_Simple_CANFD_RTM_NEWROUTE_Test()
        {
            var req = new CanGatewayRequest()
            {
                Header = new NetlinkMessageHeader()
                {
                    MessageLength = (uint)NetlinkMessageMacros.NLMSG_LENGTH(Marshal.SizeOf(typeof(RoutingCanMessage))),
                    MessageType = NetlinkMessageType.RTM_NEWROUTE,
                    Flags = NetlinkMessageFlags.NLM_F_REQUEST | NetlinkMessageFlags.NLM_F_ACK,
                    SenderPortId = 0,
                    SequenceNumber = 0,
                },
                Message = new RoutingCanMessage()
                {
                    CanFamily = SocketCanConstants.AF_CAN,
                    GatewayType = CanGatewayType.CGW_TYPE_CAN_CAN,
                    GatewayFlags = CanGatewayFlag.CGW_FLAGS_CAN_ECHO | CanGatewayFlag.CGW_FLAGS_CAN_FD,
                }
            };

            CanNetworkInterface srcInterface = CanNetworkInterface.GetInterfaceByName(socketHandle, "vcan0");
            CanNetworkInterface dstInterface = CanNetworkInterface.GetInterfaceByName(socketHandle, "vcan1");
            req.AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_SRC_IF, NetlinkUtils.ToBytes<uint>((uint)srcInterface.Index)));
            req.AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_DST_IF, NetlinkUtils.ToBytes<uint>((uint)dstInterface.Index)));
            int numBytes = CanGatewayNativeMethods.Write(socketHandle, req, (int)req.Header.MessageLength);
            Assert.AreEqual((int)req.Header.MessageLength, numBytes);

            byte[] rxBuffer = new byte[8192];
            numBytes = LibcNativeMethods.Read(socketHandle, rxBuffer, rxBuffer.Length);
            Assert.AreNotEqual(-1, numBytes, $"Errno: {LibcNativeMethods.Errno}");
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
                Assert.AreEqual(req.Header.MessageLength + 20, nlMsgErr.MessageHeader.MessageLength); // complete original message + 4 for error, 16 for header
            }
            Assert.AreEqual(req.Header.MessageLength, nlMsgErr.ErrorMessageHeader.MessageLength);
            Assert.AreEqual(req.Header.MessageType, nlMsgErr.ErrorMessageHeader.MessageType);
            Assert.AreEqual(req.Header.Flags, nlMsgErr.ErrorMessageHeader.Flags);
            Assert.AreEqual(req.Header.SenderPortId, nlMsgErr.ErrorMessageHeader.SenderPortId);
            Assert.AreEqual(req.Header.SequenceNumber, nlMsgErr.ErrorMessageHeader.SequenceNumber);
        }

        [Test]
        public void CAN_GW_Delete_All_Entries_RTM_DELROUTE_Test()
        {
            var req = new CanGatewayRequest()
            {
                Header = new NetlinkMessageHeader()
                {
                    MessageLength = (uint)NetlinkMessageMacros.NLMSG_LENGTH(Marshal.SizeOf(typeof(RoutingCanMessage))),
                    MessageType = NetlinkMessageType.RTM_DELROUTE,
                    Flags = NetlinkMessageFlags.NLM_F_REQUEST | NetlinkMessageFlags.NLM_F_ACK,
                    SenderPortId = 0,
                    SequenceNumber = 0,
                },
                Message = new RoutingCanMessage()
                {
                    CanFamily = SocketCanConstants.AF_CAN,
                    GatewayType = CanGatewayType.CGW_TYPE_CAN_CAN,
                    GatewayFlags = 0,
                }
            };

            // Delete all: set interface indexes to 0s
            req.AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_SRC_IF, NetlinkUtils.ToBytes<uint>(0)));
            req.AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_DST_IF, NetlinkUtils.ToBytes<uint>(0)));
            int numBytes = CanGatewayNativeMethods.Write(socketHandle, req, (int)req.Header.MessageLength);
            Assert.AreEqual((int)req.Header.MessageLength, numBytes);

            byte[] rxBuffer = new byte[8192];
            numBytes = LibcNativeMethods.Read(socketHandle, rxBuffer, rxBuffer.Length);
            Assert.AreNotEqual(-1, numBytes, $"Errno: {LibcNativeMethods.Errno}");
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
                Assert.AreEqual(req.Header.MessageLength + 20, nlMsgErr.MessageHeader.MessageLength); // complete original message + 4 for error, 16 for header
            }
            Assert.AreEqual(req.Header.MessageLength, nlMsgErr.ErrorMessageHeader.MessageLength);
            Assert.AreEqual(req.Header.MessageType, nlMsgErr.ErrorMessageHeader.MessageType);
            Assert.AreEqual(req.Header.Flags, nlMsgErr.ErrorMessageHeader.Flags);
            Assert.AreEqual(req.Header.SenderPortId, nlMsgErr.ErrorMessageHeader.SenderPortId);
            Assert.AreEqual(req.Header.SequenceNumber, nlMsgErr.ErrorMessageHeader.SequenceNumber);
        }

        [Test]
        public void CAN_GW_Delete_Single_Entry_RTM_DELROUTE_Test()
        {
            var req = new CanGatewayRequest()
            {
                Header = new NetlinkMessageHeader()
                {
                    MessageLength = (uint)NetlinkMessageMacros.NLMSG_LENGTH(Marshal.SizeOf(typeof(RoutingCanMessage))),
                    MessageType = NetlinkMessageType.RTM_NEWROUTE,
                    Flags = NetlinkMessageFlags.NLM_F_REQUEST | NetlinkMessageFlags.NLM_F_ACK,
                    SenderPortId = 0,
                    SequenceNumber = 0,
                },
                Message = new RoutingCanMessage()
                {
                    CanFamily = SocketCanConstants.AF_CAN,
                    GatewayType = CanGatewayType.CGW_TYPE_CAN_CAN,
                    GatewayFlags = CanGatewayFlag.CGW_FLAGS_CAN_ECHO | CanGatewayFlag.CGW_FLAGS_CAN_SRC_TSTAMP,
                }
            };

            CanNetworkInterface srcInterface = CanNetworkInterface.GetInterfaceByName(socketHandle, "vcan0");
            CanNetworkInterface dstInterface = CanNetworkInterface.GetInterfaceByName(socketHandle, "vcan1");
            req.AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_SRC_IF, NetlinkUtils.ToBytes<uint>((uint)srcInterface.Index)));
            req.AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_DST_IF, NetlinkUtils.ToBytes<uint>((uint)dstInterface.Index)));
            int numBytes = CanGatewayNativeMethods.Write(socketHandle, req, (int)req.Header.MessageLength);
            Assert.AreEqual((int)req.Header.MessageLength, numBytes);

            byte[] rxBuffer = new byte[8192];
            numBytes = LibcNativeMethods.Read(socketHandle, rxBuffer, rxBuffer.Length);
            Assert.AreNotEqual(-1, numBytes, $"Errno: {LibcNativeMethods.Errno}");
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
                Assert.AreEqual(req.Header.MessageLength + 20, nlMsgErr.MessageHeader.MessageLength); // complete original message + 4 for error, 16 for header
            }
            Assert.AreEqual(req.Header.MessageLength, nlMsgErr.ErrorMessageHeader.MessageLength);
            Assert.AreEqual(req.Header.MessageType, nlMsgErr.ErrorMessageHeader.MessageType);
            Assert.AreEqual(req.Header.Flags, nlMsgErr.ErrorMessageHeader.Flags);
            Assert.AreEqual(req.Header.SenderPortId, nlMsgErr.ErrorMessageHeader.SenderPortId);
            Assert.AreEqual(req.Header.SequenceNumber, nlMsgErr.ErrorMessageHeader.SequenceNumber);

            req = new CanGatewayRequest()
            {
                Header = new NetlinkMessageHeader()
                {
                    MessageLength = (uint)NetlinkMessageMacros.NLMSG_LENGTH(Marshal.SizeOf(typeof(RoutingCanMessage))),
                    MessageType = NetlinkMessageType.RTM_DELROUTE,
                    Flags = NetlinkMessageFlags.NLM_F_REQUEST | NetlinkMessageFlags.NLM_F_ACK,
                    SenderPortId = 0,
                    SequenceNumber = 0,
                },
                Message = new RoutingCanMessage()
                {
                    CanFamily = SocketCanConstants.AF_CAN,
                    GatewayType = CanGatewayType.CGW_TYPE_CAN_CAN,
                    GatewayFlags = CanGatewayFlag.CGW_FLAGS_CAN_ECHO | CanGatewayFlag.CGW_FLAGS_CAN_SRC_TSTAMP,
                }
            };

            req.AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_SRC_IF, NetlinkUtils.ToBytes<uint>((uint)srcInterface.Index)));
            req.AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_DST_IF, NetlinkUtils.ToBytes<uint>((uint)dstInterface.Index)));
            numBytes = CanGatewayNativeMethods.Write(socketHandle, req, (int)req.Header.MessageLength);
            Assert.AreEqual((int)req.Header.MessageLength, numBytes);

            rxBuffer = new byte[8192];
            numBytes = LibcNativeMethods.Read(socketHandle, rxBuffer, rxBuffer.Length);
            Assert.AreNotEqual(-1, numBytes, $"Errno: {LibcNativeMethods.Errno}");
            nlMsgErr = NetlinkMessageError.FromBytes(rxBuffer);
            Assert.AreEqual(NetlinkMessageType.NLMSG_ERROR, nlMsgErr.MessageHeader.MessageType);
            Assert.AreEqual((uint)System.Environment.ProcessId, nlMsgErr.MessageHeader.SenderPortId); // process ID for the sender
            Assert.AreEqual((uint)0, nlMsgErr.MessageHeader.SequenceNumber);

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
                Assert.AreEqual(req.Header.MessageLength + 20, nlMsgErr.MessageHeader.MessageLength); // complete original message + 4 for error, 16 for header
            }
            Assert.AreEqual(req.Header.MessageLength, nlMsgErr.ErrorMessageHeader.MessageLength);
            Assert.AreEqual(req.Header.MessageType, nlMsgErr.ErrorMessageHeader.MessageType);
            Assert.AreEqual(req.Header.Flags, nlMsgErr.ErrorMessageHeader.Flags);
            Assert.AreEqual(req.Header.SenderPortId, nlMsgErr.ErrorMessageHeader.SenderPortId);
            Assert.AreEqual(req.Header.SequenceNumber, nlMsgErr.ErrorMessageHeader.SequenceNumber);
        }

        [Test]
        public void CAN_GW_List_All_Entries_RTM_GETROUTE_Test()
        {
            var req = new CanGatewayRequest()
            {
                Header = new NetlinkMessageHeader()
                {
                    MessageLength = (uint)NetlinkMessageMacros.NLMSG_LENGTH(Marshal.SizeOf(typeof(RoutingCanMessage))),
                    MessageType = NetlinkMessageType.RTM_NEWROUTE,
                    Flags = NetlinkMessageFlags.NLM_F_REQUEST | NetlinkMessageFlags.NLM_F_ACK,
                    SenderPortId = 0,
                    SequenceNumber = 0,
                },
                Message = new RoutingCanMessage()
                {
                    CanFamily = SocketCanConstants.AF_CAN,
                    GatewayType = CanGatewayType.CGW_TYPE_CAN_CAN,
                    GatewayFlags = CanGatewayFlag.CGW_FLAGS_CAN_ECHO | CanGatewayFlag.CGW_FLAGS_CAN_SRC_TSTAMP,
                }
            };

            CanNetworkInterface srcInterface = CanNetworkInterface.GetInterfaceByName(socketHandle, "vcan0");
            CanNetworkInterface dstInterface = CanNetworkInterface.GetInterfaceByName(socketHandle, "vcan1");
            req.AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_SRC_IF, NetlinkUtils.ToBytes<uint>((uint)srcInterface.Index)));
            req.AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_DST_IF, NetlinkUtils.ToBytes<uint>((uint)dstInterface.Index)));
            int numBytes = CanGatewayNativeMethods.Write(socketHandle, req, (int)req.Header.MessageLength);
            Assert.AreEqual((int)req.Header.MessageLength, numBytes);

            byte[] rxBuffer = new byte[8192];
            numBytes = LibcNativeMethods.Read(socketHandle, rxBuffer, rxBuffer.Length);
            Assert.AreNotEqual(-1, numBytes, $"Errno: {LibcNativeMethods.Errno}");
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
                Assert.AreEqual(req.Header.MessageLength + 20, nlMsgErr.MessageHeader.MessageLength); // complete original message + 4 for error, 16 for header
            }
            Assert.AreEqual(req.Header.MessageLength, nlMsgErr.ErrorMessageHeader.MessageLength);
            Assert.AreEqual(req.Header.MessageType, nlMsgErr.ErrorMessageHeader.MessageType);
            Assert.AreEqual(req.Header.Flags, nlMsgErr.ErrorMessageHeader.Flags);
            Assert.AreEqual(req.Header.SenderPortId, nlMsgErr.ErrorMessageHeader.SenderPortId);
            Assert.AreEqual(req.Header.SequenceNumber, nlMsgErr.ErrorMessageHeader.SequenceNumber);

            req = new CanGatewayRequest()
            {
                Header = new NetlinkMessageHeader()
                {
                    MessageLength = (uint)NetlinkMessageMacros.NLMSG_LENGTH(Marshal.SizeOf(typeof(RoutingCanMessage))),
                    MessageType = NetlinkMessageType.RTM_GETROUTE,
                    Flags = NetlinkMessageFlags.NLM_F_REQUEST | NetlinkMessageFlags.NLM_F_DUMP,
                    SenderPortId = 0,
                    SequenceNumber = 0,
                },
                Message = new RoutingCanMessage()
                {
                    CanFamily = SocketCanConstants.AF_CAN,
                    GatewayType = CanGatewayType.CGW_TYPE_CAN_CAN,
                    GatewayFlags = CanGatewayFlag.CGW_FLAGS_CAN_ECHO | CanGatewayFlag.CGW_FLAGS_CAN_SRC_TSTAMP,
                }
            };

            req.AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_SRC_IF, NetlinkUtils.ToBytes<uint>((uint)srcInterface.Index)));
            req.AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_DST_IF, NetlinkUtils.ToBytes<uint>((uint)dstInterface.Index)));
            numBytes = CanGatewayNativeMethods.Write(socketHandle, req, (int)req.Header.MessageLength);
            Assert.AreEqual((int)req.Header.MessageLength, numBytes);

            bool keepReading = true;
            while (keepReading)
            {
                rxBuffer = new byte[8192];
                numBytes = LibcNativeMethods.Read(socketHandle, rxBuffer, rxBuffer.Length);
                Assert.AreNotEqual(-1, numBytes, $"Errno: {LibcNativeMethods.Errno}");
                byte[] filledBuffer = rxBuffer.Take(numBytes).ToArray();
                
                int offset = 0;
                while (numBytes > 0)
                {
                    byte[] headerData = filledBuffer.Skip(offset).Take(NetlinkMessageMacros.NLMSG_HDRLEN).ToArray();
                    offset += NetlinkMessageMacros.NLMSG_HDRLEN;
                    NetlinkMessageHeader msgHeader = NetlinkMessageHeader.FromBytes(headerData);
                    Assert.NotZero(msgHeader.MessageLength);
                    Assert.That(msgHeader.Flags, Is.EqualTo(NetlinkMessageFlags.NLM_F_MULTI));
                    Assert.That(msgHeader.MessageType, Is.EqualTo(NetlinkMessageType.RTM_NEWROUTE).Or.EqualTo(NetlinkMessageType.NLMSG_DONE));
                    Assert.AreEqual((uint)System.Environment.ProcessId, msgHeader.SenderPortId); // process ID for the sender
                    Assert.AreEqual((uint)0, msgHeader.SequenceNumber);

                    if (NetlinkMessageMacros.NLMSG_OK(msgHeader, numBytes) == false)
                        break;

                    numBytes -= (int)msgHeader.MessageLength;

                    if (msgHeader.MessageType == NetlinkMessageType.NLMSG_ERROR || msgHeader.MessageType == NetlinkMessageType.NLMSG_DONE)
                    {
                        keepReading = false;
                        break;
                    }

                    byte[] rtCanData = filledBuffer.Skip(offset).Take(NetlinkMessageMacros.NLMSG_PAYLOAD(msgHeader, 0)).ToArray();
                    offset += NetlinkMessageMacros.NLMSG_ALIGN(Marshal.SizeOf<RoutingCanMessage>());
                    RoutingCanMessage rtCanMsg = RoutingCanMessage.FromBytes(rtCanData);
                    Assert.AreEqual(SocketCanConstants.AF_CAN, rtCanMsg.CanFamily);
                    Assert.AreEqual(CanGatewayType.CGW_TYPE_CAN_CAN, rtCanMsg.GatewayType);
                    Assert.IsTrue(rtCanMsg.GatewayFlags.HasFlag(CanGatewayFlag.CGW_FLAGS_CAN_ECHO));
                    
                    byte[] parseData = filledBuffer.Skip(offset).Take((int)msgHeader.MessageLength - offset).ToArray();
                    List<CanGatewayRoutingAttribute> rtAttrList = CanGatewayRequest.ParseAttributes(parseData);
                    foreach (var attr in rtAttrList)
                    {
                        Console.WriteLine(attr.ToString());

                        switch (attr.Type)
                        {
                            case CanGatewayAttributeType.CGW_SRC_IF:
                                Console.WriteLine($"* Source Interface: {NetlinkUtils.FromBytes<uint>(attr.Data)}");
                                break;
                            case CanGatewayAttributeType.CGW_DST_IF:
                                Console.WriteLine($"* Destination Interface: {NetlinkUtils.FromBytes<uint>(attr.Data)}");
                                break;
                            case CanGatewayAttributeType.CGW_MOD_SET:
                            case CanGatewayAttributeType.CGW_MOD_AND:
                            case CanGatewayAttributeType.CGW_MOD_OR:
                            case CanGatewayAttributeType.CGW_MOD_XOR:
                                Console.WriteLine($"* Classic CAN Modification Set: {NetlinkUtils.FromBytes<CgwCanFrameModification>(attr.Data).ToString()}");
                                break;
                            case CanGatewayAttributeType.CGW_FDMOD_SET:
                            case CanGatewayAttributeType.CGW_FDMOD_AND:
                            case CanGatewayAttributeType.CGW_FDMOD_OR:
                            case CanGatewayAttributeType.CGW_FDMOD_XOR:
                                Console.WriteLine($"* CAN FD Modification Set: {NetlinkUtils.FromBytes<CgwCanFdFrameModification>(attr.Data).ToString()}");
                                break;
                            case CanGatewayAttributeType.CGW_CS_XOR:
                                Console.WriteLine($"* Checksum XOR: {NetlinkUtils.FromBytes<CgwChecksumXor>(attr.Data).ToString()}");
                                break;
                            case CanGatewayAttributeType.CGW_CS_CRC8:
                                Console.WriteLine($"* CRC8: {NetlinkUtils.FromBytes<CgwCrc8>(attr.Data).ToString()}");
                                break;
                            case CanGatewayAttributeType.CGW_FILTER:
                                Console.WriteLine($"* Filter: {NetlinkUtils.FromBytes<CanFilter>(attr.Data).ToString()}");
                                break;
                            case CanGatewayAttributeType.CGW_LIM_HOPS:
                                Console.WriteLine($"* Hop Limit: {NetlinkUtils.FromBytes<byte>(attr.Data).ToString()}");
                                break;
                            case CanGatewayAttributeType.CGW_MOD_UID:
                                Console.WriteLine($"* UID: {NetlinkUtils.FromBytes<uint>(attr.Data)}");
                                break;
                            case CanGatewayAttributeType.CGW_HANDLED:
                                Console.WriteLine($"* Handled: {NetlinkUtils.FromBytes<uint>(attr.Data)}");
                                break;
                            case CanGatewayAttributeType.CGW_DROPPED:
                                Console.WriteLine($"* Dropped: {NetlinkUtils.FromBytes<uint>(attr.Data)}");
                                break;
                            case CanGatewayAttributeType.CGW_DELETED:
                                Console.WriteLine($"* Deleted: {NetlinkUtils.FromBytes<uint>(attr.Data)}");
                                break;
                            default:
                                break;
                        }
                    }
                    offset += parseData.Length;
                }
            }
        }

        [Test]
        public void CAN_GW_Add_Routing_Entry_CAN_FD_RTM_NEWROUTE_Test()
        {
            var req = new CanGatewayRequest()
            {
                Header = new NetlinkMessageHeader()
                {
                    MessageLength = (uint)NetlinkMessageMacros.NLMSG_LENGTH(Marshal.SizeOf(typeof(RoutingCanMessage))),
                    MessageType = NetlinkMessageType.RTM_NEWROUTE,
                    Flags = NetlinkMessageFlags.NLM_F_REQUEST | NetlinkMessageFlags.NLM_F_ACK,
                    SenderPortId = 0,
                    SequenceNumber = 0,
                },
                Message = new RoutingCanMessage()
                {
                    CanFamily = SocketCanConstants.AF_CAN,
                    GatewayType = CanGatewayType.CGW_TYPE_CAN_CAN,
                    GatewayFlags = CanGatewayFlag.CGW_FLAGS_CAN_ECHO | CanGatewayFlag.CGW_FLAGS_CAN_SRC_TSTAMP | CanGatewayFlag.CGW_FLAGS_CAN_FD,
                }
            };

            CanNetworkInterface srcInterface = CanNetworkInterface.GetInterfaceByName(socketHandle, "vcan0");
            CanNetworkInterface dstInterface = CanNetworkInterface.GetInterfaceByName(socketHandle, "vcan1");
            req.AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_SRC_IF, NetlinkUtils.ToBytes<uint>((uint)srcInterface.Index)));
            req.AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_DST_IF, NetlinkUtils.ToBytes<uint>((uint)dstInterface.Index)));
            int numBytes = CanGatewayNativeMethods.Write(socketHandle, req, (int)req.Header.MessageLength);
            Assert.AreEqual((int)req.Header.MessageLength, numBytes);

            byte[] rxBuffer = new byte[8192];
            numBytes = LibcNativeMethods.Read(socketHandle, rxBuffer, rxBuffer.Length);
            Assert.AreNotEqual(-1, numBytes, $"Errno: {LibcNativeMethods.Errno}");
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
                Assert.AreEqual(req.Header.MessageLength + 20, nlMsgErr.MessageHeader.MessageLength); // complete original message + 4 for error, 16 for header
            }
            Assert.AreEqual(req.Header.MessageLength, nlMsgErr.ErrorMessageHeader.MessageLength);
            Assert.AreEqual(req.Header.MessageType, nlMsgErr.ErrorMessageHeader.MessageType);
            Assert.AreEqual(req.Header.Flags, nlMsgErr.ErrorMessageHeader.Flags);
            Assert.AreEqual(req.Header.SenderPortId, nlMsgErr.ErrorMessageHeader.SenderPortId);
            Assert.AreEqual(req.Header.SequenceNumber, nlMsgErr.ErrorMessageHeader.SequenceNumber);
        }

        [Test]
        public void CAN_GW_Add_Routing_Entry_Same_Interface_RTM_NEWROUTE_Test()
        {
            var req = new CanGatewayRequest()
            {
                Header = new NetlinkMessageHeader()
                {
                    MessageLength = (uint)NetlinkMessageMacros.NLMSG_LENGTH(Marshal.SizeOf(typeof(RoutingCanMessage))),
                    MessageType = NetlinkMessageType.RTM_NEWROUTE,
                    Flags = NetlinkMessageFlags.NLM_F_REQUEST | NetlinkMessageFlags.NLM_F_ACK,
                    SenderPortId = 0,
                    SequenceNumber = 0,
                },
                Message = new RoutingCanMessage()
                {
                    CanFamily = SocketCanConstants.AF_CAN,
                    GatewayType = CanGatewayType.CGW_TYPE_CAN_CAN,
                    GatewayFlags = CanGatewayFlag.CGW_FLAGS_CAN_IIF_TX_OK,
                }
            };

            CanNetworkInterface srcInterface = CanNetworkInterface.GetInterfaceByName(socketHandle, "vcan0");
            req.AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_SRC_IF, NetlinkUtils.ToBytes<uint>((uint)srcInterface.Index)));
            req.AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_DST_IF, NetlinkUtils.ToBytes<uint>((uint)srcInterface.Index)));
            int numBytes = CanGatewayNativeMethods.Write(socketHandle, req, (int)req.Header.MessageLength);
            Assert.AreEqual((int)req.Header.MessageLength, numBytes);

            byte[] rxBuffer = new byte[8192];
            numBytes = LibcNativeMethods.Read(socketHandle, rxBuffer, rxBuffer.Length);
            Assert.AreNotEqual(-1, numBytes, $"Errno: {LibcNativeMethods.Errno}");;
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
                Assert.AreEqual(req.Header.MessageLength + 20, nlMsgErr.MessageHeader.MessageLength); // complete original message + 4 for error, 16 for header
            }
            Assert.AreEqual(req.Header.MessageLength, nlMsgErr.ErrorMessageHeader.MessageLength);
            Assert.AreEqual(req.Header.MessageType, nlMsgErr.ErrorMessageHeader.MessageType);
            Assert.AreEqual(req.Header.Flags, nlMsgErr.ErrorMessageHeader.Flags);
            Assert.AreEqual(req.Header.SenderPortId, nlMsgErr.ErrorMessageHeader.SenderPortId);
            Assert.AreEqual(req.Header.SequenceNumber, nlMsgErr.ErrorMessageHeader.SequenceNumber);
        }

        [Test]
        public void CAN_GW_Add_Routing_Entry_UID_RTM_NEWROUTE_Test()
        {
            var req = new CanGatewayRequest()
            {
                Header = new NetlinkMessageHeader()
                {
                    MessageLength = (uint)NetlinkMessageMacros.NLMSG_LENGTH(Marshal.SizeOf(typeof(RoutingCanMessage))),
                    MessageType = NetlinkMessageType.RTM_NEWROUTE,
                    Flags = NetlinkMessageFlags.NLM_F_REQUEST | NetlinkMessageFlags.NLM_F_ACK,
                    SenderPortId = 0,
                    SequenceNumber = 0,
                },
                Message = new RoutingCanMessage()
                {
                    CanFamily = SocketCanConstants.AF_CAN,
                    GatewayType = CanGatewayType.CGW_TYPE_CAN_CAN,
                    GatewayFlags = CanGatewayFlag.CGW_FLAGS_CAN_IIF_TX_OK,
                }
            };

            CanNetworkInterface srcInterface = CanNetworkInterface.GetInterfaceByName(socketHandle, "vcan0");
            req.AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_SRC_IF, NetlinkUtils.ToBytes<uint>((uint)srcInterface.Index)));
            req.AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_DST_IF, NetlinkUtils.ToBytes<uint>((uint)srcInterface.Index)));
            req.AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_MOD_UID, NetlinkUtils.ToBytes<uint>(1234)));
            int numBytes = CanGatewayNativeMethods.Write(socketHandle, req, (int)req.Header.MessageLength);
            Assert.AreEqual((int)req.Header.MessageLength, numBytes);

            byte[] rxBuffer = new byte[8192];
            numBytes = LibcNativeMethods.Read(socketHandle, rxBuffer, rxBuffer.Length);
            Assert.AreNotEqual(-1, numBytes, $"Errno: {LibcNativeMethods.Errno}");
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
                Assert.AreEqual(req.Header.MessageLength + 20, nlMsgErr.MessageHeader.MessageLength); // complete original message + 4 for error, 16 for header
            }
            Assert.AreEqual(req.Header.MessageLength, nlMsgErr.ErrorMessageHeader.MessageLength);
            Assert.AreEqual(req.Header.MessageType, nlMsgErr.ErrorMessageHeader.MessageType);
            Assert.AreEqual(req.Header.Flags, nlMsgErr.ErrorMessageHeader.Flags);
            Assert.AreEqual(req.Header.SenderPortId, nlMsgErr.ErrorMessageHeader.SenderPortId);
            Assert.AreEqual(req.Header.SequenceNumber, nlMsgErr.ErrorMessageHeader.SequenceNumber);
        }

        [Test]
        public void CAN_GW_Add_Routing_Entry_Limit_Hops_RTM_NEWROUTE_Test()
        {
            var req = new CanGatewayRequest()
            {
                Header = new NetlinkMessageHeader()
                {
                    MessageLength = (uint)NetlinkMessageMacros.NLMSG_LENGTH(Marshal.SizeOf(typeof(RoutingCanMessage))),
                    MessageType = NetlinkMessageType.RTM_NEWROUTE,
                    Flags = NetlinkMessageFlags.NLM_F_REQUEST | NetlinkMessageFlags.NLM_F_ACK,
                    SenderPortId = 0,
                    SequenceNumber = 0,
                },
                Message = new RoutingCanMessage()
                {
                    CanFamily = SocketCanConstants.AF_CAN,
                    GatewayType = CanGatewayType.CGW_TYPE_CAN_CAN,
                    GatewayFlags = CanGatewayFlag.CGW_FLAGS_CAN_ECHO,
                }
            };

            CanNetworkInterface srcInterface = CanNetworkInterface.GetInterfaceByName(socketHandle, "vcan0");
            CanNetworkInterface dstInterface = CanNetworkInterface.GetInterfaceByName(socketHandle, "vcan1");
            req.AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_SRC_IF, NetlinkUtils.ToBytes<uint>((uint)srcInterface.Index)));
            req.AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_DST_IF, NetlinkUtils.ToBytes<uint>((uint)dstInterface.Index)));
            req.AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_LIM_HOPS, NetlinkUtils.ToBytes<byte>(1)));
            int numBytes = CanGatewayNativeMethods.Write(socketHandle, req, (int)req.Header.MessageLength);
            Assert.AreEqual((int)req.Header.MessageLength, numBytes);

            byte[] rxBuffer = new byte[8192];
            numBytes = LibcNativeMethods.Read(socketHandle, rxBuffer, rxBuffer.Length);
            Assert.AreNotEqual(-1, numBytes, $"Errno: {LibcNativeMethods.Errno}");
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
                Assert.AreEqual(req.Header.MessageLength + 20, nlMsgErr.MessageHeader.MessageLength); // complete original message + 4 for error, 16 for header
            }
            Assert.AreEqual(req.Header.MessageLength, nlMsgErr.ErrorMessageHeader.MessageLength);
            Assert.AreEqual(req.Header.MessageType, nlMsgErr.ErrorMessageHeader.MessageType);
            Assert.AreEqual(req.Header.Flags, nlMsgErr.ErrorMessageHeader.Flags);
            Assert.AreEqual(req.Header.SenderPortId, nlMsgErr.ErrorMessageHeader.SenderPortId);
            Assert.AreEqual(req.Header.SequenceNumber, nlMsgErr.ErrorMessageHeader.SequenceNumber);
        }

        [Test]
        public void CAN_GW_Add_Routing_Entry_CAN_Filter_RTM_NEWROUTE_Test()
        {
            var canFilter = new CanFilter(0x700, 0x700);
            var req = new CanGatewayRequest()
            {
                Header = new NetlinkMessageHeader()
                {
                    MessageLength = (uint)NetlinkMessageMacros.NLMSG_LENGTH(Marshal.SizeOf(typeof(RoutingCanMessage))),
                    MessageType = NetlinkMessageType.RTM_NEWROUTE,
                    Flags = NetlinkMessageFlags.NLM_F_REQUEST | NetlinkMessageFlags.NLM_F_ACK,
                    SenderPortId = 0,
                    SequenceNumber = 0,
                },
                Message = new RoutingCanMessage()
                {
                    CanFamily = SocketCanConstants.AF_CAN,
                    GatewayType = CanGatewayType.CGW_TYPE_CAN_CAN,
                    GatewayFlags = CanGatewayFlag.CGW_FLAGS_CAN_ECHO,
                }
            };

            CanNetworkInterface srcInterface = CanNetworkInterface.GetInterfaceByName(socketHandle, "vcan0");
            CanNetworkInterface dstInterface = CanNetworkInterface.GetInterfaceByName(socketHandle, "vcan1");
            req.AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_SRC_IF, NetlinkUtils.ToBytes<uint>((uint)srcInterface.Index)));
            req.AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_DST_IF, NetlinkUtils.ToBytes<uint>((uint)dstInterface.Index)));
            req.AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_FILTER, NetlinkUtils.ToBytes<CanFilter>(canFilter)));
            int numBytes = CanGatewayNativeMethods.Write(socketHandle, req, (int)req.Header.MessageLength);
            Assert.AreEqual((int)req.Header.MessageLength, numBytes);

            byte[] rxBuffer = new byte[8192];
            numBytes = LibcNativeMethods.Read(socketHandle, rxBuffer, rxBuffer.Length);
            Assert.AreNotEqual(-1, numBytes, $"Errno: {LibcNativeMethods.Errno}");
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
                Assert.AreEqual(req.Header.MessageLength + 20, nlMsgErr.MessageHeader.MessageLength); // complete original message + 4 for error, 16 for header
            }
            Assert.AreEqual(req.Header.MessageLength, nlMsgErr.ErrorMessageHeader.MessageLength);
            Assert.AreEqual(req.Header.MessageType, nlMsgErr.ErrorMessageHeader.MessageType);
            Assert.AreEqual(req.Header.Flags, nlMsgErr.ErrorMessageHeader.Flags);
            Assert.AreEqual(req.Header.SenderPortId, nlMsgErr.ErrorMessageHeader.SenderPortId);
            Assert.AreEqual(req.Header.SequenceNumber, nlMsgErr.ErrorMessageHeader.SequenceNumber);
        }

        [Test]
        public void CAN_GW_Add_Routing_Entry_Checksum_XOR_RTM_NEWROUTE_Test()
        {
            var req = new CanGatewayRequest()
            {
                Header = new NetlinkMessageHeader()
                {
                    MessageLength = (uint)NetlinkMessageMacros.NLMSG_LENGTH(Marshal.SizeOf(typeof(RoutingCanMessage))),
                    MessageType = NetlinkMessageType.RTM_NEWROUTE,
                    Flags = NetlinkMessageFlags.NLM_F_REQUEST | NetlinkMessageFlags.NLM_F_ACK,
                    SenderPortId = 0,
                    SequenceNumber = 0,
                },
                Message = new RoutingCanMessage()
                {
                    CanFamily = SocketCanConstants.AF_CAN,
                    GatewayType = CanGatewayType.CGW_TYPE_CAN_CAN,
                    GatewayFlags = CanGatewayFlag.CGW_FLAGS_CAN_ECHO,
                }
            };

            CanNetworkInterface srcInterface = CanNetworkInterface.GetInterfaceByName(socketHandle, "vcan0");
            CanNetworkInterface dstInterface = CanNetworkInterface.GetInterfaceByName(socketHandle, "vcan1");
            req.AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_SRC_IF, NetlinkUtils.ToBytes<uint>((uint)srcInterface.Index)));
            req.AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_DST_IF, NetlinkUtils.ToBytes<uint>((uint)dstInterface.Index)));

            var csXor = new CgwChecksumXor();
            csXor.FromIndex = 0;
            csXor.ToIndex = 3;
            csXor.ResultIndex = 4;
            csXor.InitialXorValue = 0xFF;
            req.AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_CS_XOR, NetlinkUtils.ToBytes<CgwChecksumXor>(csXor)));

            var canMsgMod = new CgwCanFrameModification();
            canMsgMod.ModificationType = CanGatewayModificationType.CGW_MOD_LEN;
            canMsgMod.CanFrame = new CanFrame(0x123, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00 });
            req.AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_MOD_SET, NetlinkUtils.ToBytes<CgwCanFrameModification>(canMsgMod)));

            int numBytes = CanGatewayNativeMethods.Write(socketHandle, req, (int)req.Header.MessageLength);
            Assert.AreEqual((int)req.Header.MessageLength, numBytes);

            byte[] rxBuffer = new byte[8192];
            numBytes = LibcNativeMethods.Read(socketHandle, rxBuffer, rxBuffer.Length);
            Assert.AreNotEqual(-1, numBytes, $"Errno: {LibcNativeMethods.Errno}");
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
                Assert.AreEqual(req.Header.MessageLength + 20, nlMsgErr.MessageHeader.MessageLength); // complete original message + 4 for error, 16 for header
            }
            Assert.AreEqual(req.Header.MessageLength, nlMsgErr.ErrorMessageHeader.MessageLength);
            Assert.AreEqual(req.Header.MessageType, nlMsgErr.ErrorMessageHeader.MessageType);
            Assert.AreEqual(req.Header.Flags, nlMsgErr.ErrorMessageHeader.Flags);
            Assert.AreEqual(req.Header.SenderPortId, nlMsgErr.ErrorMessageHeader.SenderPortId);
            Assert.AreEqual(req.Header.SequenceNumber, nlMsgErr.ErrorMessageHeader.SequenceNumber);
        }

        [Test]
        public void CAN_GW_Add_Routing_Entry_CRC8_RTM_NEWROUTE_Test()
        {
            var req = new CanGatewayRequest()
            {
                Header = new NetlinkMessageHeader()
                {
                    MessageLength = (uint)NetlinkMessageMacros.NLMSG_LENGTH(Marshal.SizeOf(typeof(RoutingCanMessage))),
                    MessageType = NetlinkMessageType.RTM_NEWROUTE,
                    Flags = NetlinkMessageFlags.NLM_F_REQUEST | NetlinkMessageFlags.NLM_F_ACK,
                    SenderPortId = 0,
                    SequenceNumber = 0,
                },
                Message = new RoutingCanMessage()
                {
                    CanFamily = SocketCanConstants.AF_CAN,
                    GatewayType = CanGatewayType.CGW_TYPE_CAN_CAN,
                    GatewayFlags = CanGatewayFlag.CGW_FLAGS_CAN_ECHO,
                }
            };

            CanNetworkInterface srcInterface = CanNetworkInterface.GetInterfaceByName(socketHandle, "vcan0");
            CanNetworkInterface dstInterface = CanNetworkInterface.GetInterfaceByName(socketHandle, "vcan1");
            req.AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_SRC_IF, NetlinkUtils.ToBytes<uint>((uint)srcInterface.Index)));
            req.AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_DST_IF, NetlinkUtils.ToBytes<uint>((uint)dstInterface.Index)));

            var crc8 = new CgwCrc8();
            crc8.FromIndex = 0;
            crc8.ToIndex = 3;
            crc8.ResultIndex = 4;
            crc8.InitialCrcValue = 0xFF;
            crc8.FinalXorValue = 0x00;
            crc8.CrcTable = new byte[] {
                0x00, 0x07, 0x0e, 0x09, 0x1c, 0x1b, 0x12, 0x15, 0x38, 0x3f, 0x36, 0x31,
                0x24, 0x23, 0x2a, 0x2d, 0x70, 0x77, 0x7e, 0x79, 0x6c, 0x6b, 0x62, 0x65,
                0x48, 0x4f, 0x46, 0x41, 0x54, 0x53, 0x5a, 0x5d, 0xe0, 0xe7, 0xee, 0xe9,
                0xfc, 0xfb, 0xf2, 0xf5, 0xd8, 0xdf, 0xd6, 0xd1, 0xc4, 0xc3, 0xca, 0xcd,
                0x90, 0x97, 0x9e, 0x99, 0x8c, 0x8b, 0x82, 0x85, 0xa8, 0xaf, 0xa6, 0xa1,
                0xb4, 0xb3, 0xba, 0xbd, 0xc7, 0xc0, 0xc9, 0xce, 0xdb, 0xdc, 0xd5, 0xd2,
                0xff, 0xf8, 0xf1, 0xf6, 0xe3, 0xe4, 0xed, 0xea, 0xb7, 0xb0, 0xb9, 0xbe,
                0xab, 0xac, 0xa5, 0xa2, 0x8f, 0x88, 0x81, 0x86, 0x93, 0x94, 0x9d, 0x9a,
                0x27, 0x20, 0x29, 0x2e, 0x3b, 0x3c, 0x35, 0x32, 0x1f, 0x18, 0x11, 0x16,
                0x03, 0x04, 0x0d, 0x0a, 0x57, 0x50, 0x59, 0x5e, 0x4b, 0x4c, 0x45, 0x42,
                0x6f, 0x68, 0x61, 0x66, 0x73, 0x74, 0x7d, 0x7a, 0x89, 0x8e, 0x87, 0x80,
                0x95, 0x92, 0x9b, 0x9c, 0xb1, 0xb6, 0xbf, 0xb8, 0xad, 0xaa, 0xa3, 0xa4,
                0xf9, 0xfe, 0xf7, 0xf0, 0xe5, 0xe2, 0xeb, 0xec, 0xc1, 0xc6, 0xcf, 0xc8,
                0xdd, 0xda, 0xd3, 0xd4, 0x69, 0x6e, 0x67, 0x60, 0x75, 0x72, 0x7b, 0x7c,
                0x51, 0x56, 0x5f, 0x58, 0x4d, 0x4a, 0x43, 0x44, 0x19, 0x1e, 0x17, 0x10,
                0x05, 0x02, 0x0b, 0x0c, 0x21, 0x26, 0x2f, 0x28, 0x3d, 0x3a, 0x33, 0x34,
                0x4e, 0x49, 0x40, 0x47, 0x52, 0x55, 0x5c, 0x5b, 0x76, 0x71, 0x78, 0x7f,
                0x6a, 0x6d, 0x64, 0x63, 0x3e, 0x39, 0x30, 0x37, 0x22, 0x25, 0x2c, 0x2b,
                0x06, 0x01, 0x08, 0x0f, 0x1a, 0x1d, 0x14, 0x13, 0xae, 0xa9, 0xa0, 0xa7,
                0xb2, 0xb5, 0xbc, 0xbb, 0x96, 0x91, 0x98, 0x9f, 0x8a, 0x8d, 0x84, 0x83,
                0xde, 0xd9, 0xd0, 0xd7, 0xc2, 0xc5, 0xcc, 0xcb, 0xe6, 0xe1, 0xe8, 0xef,
                0xfa, 0xfd, 0xf4, 0xf3
            };
            crc8.Profile = Crc8Profile.CGW_CRC8PRF_1U8;
            crc8.ProfileData = new byte[] { 0xC8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            req.AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_CS_CRC8, NetlinkUtils.ToBytes<CgwCrc8>(crc8)));

            var canMsgMod = new CgwCanFrameModification();
            canMsgMod.ModificationType = CanGatewayModificationType.CGW_MOD_LEN;
            canMsgMod.CanFrame = new CanFrame(0x123, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00 });
            req.AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_MOD_SET, NetlinkUtils.ToBytes<CgwCanFrameModification>(canMsgMod)));

            int numBytes = CanGatewayNativeMethods.Write(socketHandle, req, (int)req.Header.MessageLength);
            Assert.AreEqual((int)req.Header.MessageLength, numBytes);

            byte[] rxBuffer = new byte[8192];
            numBytes = LibcNativeMethods.Read(socketHandle, rxBuffer, rxBuffer.Length);
            Assert.AreNotEqual(-1, numBytes, $"Errno: {LibcNativeMethods.Errno}");
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
                Assert.AreEqual(req.Header.MessageLength + 20, nlMsgErr.MessageHeader.MessageLength); // complete original message + 4 for error, 16 for header
            }
            Assert.AreEqual(req.Header.MessageLength, nlMsgErr.ErrorMessageHeader.MessageLength);
            Assert.AreEqual(req.Header.MessageType, nlMsgErr.ErrorMessageHeader.MessageType);
            Assert.AreEqual(req.Header.Flags, nlMsgErr.ErrorMessageHeader.Flags);
            Assert.AreEqual(req.Header.SenderPortId, nlMsgErr.ErrorMessageHeader.SenderPortId);
            Assert.AreEqual(req.Header.SequenceNumber, nlMsgErr.ErrorMessageHeader.SequenceNumber);
        }

        [Test]
        public void CAN_GW_Add_Routing_Entry_CANFD_Checksum_XOR_RTM_NEWROUTE_Test()
        {
            var req = new CanGatewayRequest()
            {
                Header = new NetlinkMessageHeader()
                {
                    MessageLength = (uint)NetlinkMessageMacros.NLMSG_LENGTH(Marshal.SizeOf(typeof(RoutingCanMessage))),
                    MessageType = NetlinkMessageType.RTM_NEWROUTE,
                    Flags = NetlinkMessageFlags.NLM_F_REQUEST | NetlinkMessageFlags.NLM_F_ACK,
                    SenderPortId = 0,
                    SequenceNumber = 0,
                },
                Message = new RoutingCanMessage()
                {
                    CanFamily = SocketCanConstants.AF_CAN,
                    GatewayType = CanGatewayType.CGW_TYPE_CAN_CAN,
                    GatewayFlags = CanGatewayFlag.CGW_FLAGS_CAN_ECHO | CanGatewayFlag.CGW_FLAGS_CAN_FD,
                }
            };

            CanNetworkInterface srcInterface = CanNetworkInterface.GetInterfaceByName(socketHandle, "vcan0");
            CanNetworkInterface dstInterface = CanNetworkInterface.GetInterfaceByName(socketHandle, "vcan1");
            req.AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_SRC_IF, NetlinkUtils.ToBytes<uint>((uint)srcInterface.Index)));
            req.AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_DST_IF, NetlinkUtils.ToBytes<uint>((uint)dstInterface.Index)));

            var csXor = new CgwChecksumXor();
            csXor.FromIndex = 0;
            csXor.ToIndex = 3;
            csXor.ResultIndex = 4;
            csXor.InitialXorValue = 0xFF;
            req.AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_CS_XOR, NetlinkUtils.ToBytes<CgwChecksumXor>(csXor)));

            var canMsgMod = new CgwCanFdFrameModification();
            canMsgMod.ModificationType = CanGatewayModificationType.CGW_MOD_LEN;
            canMsgMod.CanFdFrame = new CanFdFrame(0x123, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00 }, CanFdFlags.CANFD_BRS);
            req.AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_FDMOD_SET, NetlinkUtils.ToBytes<CgwCanFdFrameModification>(canMsgMod)));

            int numBytes = CanGatewayNativeMethods.Write(socketHandle, req, (int)req.Header.MessageLength);
            Assert.AreEqual((int)req.Header.MessageLength, numBytes);

            byte[] rxBuffer = new byte[8192];
            numBytes = LibcNativeMethods.Read(socketHandle, rxBuffer, rxBuffer.Length);
            Assert.AreNotEqual(-1, numBytes, $"Errno: {LibcNativeMethods.Errno}");
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
                Assert.AreEqual(req.Header.MessageLength + 20, nlMsgErr.MessageHeader.MessageLength); // complete original message + 4 for error, 16 for header
            }
            Assert.AreEqual(req.Header.MessageLength, nlMsgErr.ErrorMessageHeader.MessageLength);
            Assert.AreEqual(req.Header.MessageType, nlMsgErr.ErrorMessageHeader.MessageType);
            Assert.AreEqual(req.Header.Flags, nlMsgErr.ErrorMessageHeader.Flags);
            Assert.AreEqual(req.Header.SenderPortId, nlMsgErr.ErrorMessageHeader.SenderPortId);
            Assert.AreEqual(req.Header.SequenceNumber, nlMsgErr.ErrorMessageHeader.SequenceNumber);
        }
    }
}