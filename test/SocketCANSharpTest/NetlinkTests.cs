#region License
/* 
BSD 3-Clause License

Copyright (c) 2022, Derek Will
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
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using NUnit.Framework;
using SocketCANSharp;
using SocketCANSharp.Network;
using SocketCANSharp.Network.Netlink;
using System.Net.Sockets;

namespace SocketCANSharpTest
{
    public class NetlinkTests
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
        public void NETLINK_ROUTE_Write_GETLINK_REQUEST_Test()
        {
            var ifr = new Ifreq("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
            Assert.AreNotEqual(-1, ioctlResult);

            var req = new NetworkInterfaceInfoRequest()
            {
                Header = new NetlinkMessageHeader()
                {
                    MessageLength = (uint)Marshal.SizeOf(typeof(NetworkInterfaceInfoRequest)),
                    MessageType = NetlinkMessageType.RTM_GETLINK,
                    Flags = NetlinkMessageFlags.NLM_F_REQUEST,
                    SenderPortId = 0,
                    SequenceNumber = 0,
                },
                Information = new InterfaceInfoMessage()
                {
                    AddressFamily = NetlinkConstants.AF_NETLINK,
                    InterfaceIndex = ifr.IfIndex,
                }
            };
            int size = Marshal.SizeOf(typeof(NetworkInterfaceInfoRequest));
            int numBytes = NetlinkNativeMethods.Write(socketHandle, req, size);
            Assert.AreEqual(size, numBytes);
        }

        [Test]
        public void NETLINK_ROUTE_Read_Link_Information_Test()
        {
            Console.WriteLine($"Process ID: {System.Environment.ProcessId}");
            var ifr = new Ifreq("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
            Assert.AreNotEqual(-1, ioctlResult);
            Console.WriteLine($"Interface Index: {ifr.IfIndex}");

            var req = new NetworkInterfaceInfoRequest()
            {
                Header = new NetlinkMessageHeader()
                {
                    MessageLength = (uint)Marshal.SizeOf<NetworkInterfaceInfoRequest>(),
                    MessageType = NetlinkMessageType.RTM_GETLINK,
                    Flags = NetlinkMessageFlags.NLM_F_REQUEST,
                    SenderPortId = 0,
                    SequenceNumber = 0,
                },
                Information = new InterfaceInfoMessage()
                {
                    AddressFamily = NetlinkConstants.AF_NETLINK,
                    InterfaceIndex = ifr.IfIndex,
                }
            };
            int numBytes = NetlinkNativeMethods.Write(socketHandle, req, (int)req.Header.MessageLength);
            Assert.AreEqual((int)req.Header.MessageLength, numBytes);

            byte[] rxBuffer = new byte[8192];
            numBytes = LibcNativeMethods.Read(socketHandle, rxBuffer, rxBuffer.Length);
            Assert.AreNotEqual(-1, numBytes, $"Errno: {LibcNativeMethods.Errno}");
            Console.WriteLine($"NumBytes: {numBytes}");
            Console.WriteLine(BitConverter.ToString(rxBuffer.Take(numBytes).ToArray()));

            int offset = 0;
            while (numBytes > 0)
            {
                byte[] headerData = rxBuffer.Skip(offset).Take(NetlinkMessageMacros.NLMSG_HDRLEN).ToArray();
                offset += NetlinkMessageMacros.NLMSG_HDRLEN;
                NetlinkMessageHeader msgHeader = NetlinkMessageHeader.FromBytes(headerData);
                Console.WriteLine($"Message Length: {msgHeader.MessageLength}");
                Console.WriteLine($"Message Type: {msgHeader.MessageType}");
                Console.WriteLine($"Message Flags: {msgHeader.Flags}");
                Console.WriteLine($"Message Sequence Number: {msgHeader.SequenceNumber}");
                Console.WriteLine($"Message Port ID: {msgHeader.SenderPortId}");

                if (NetlinkMessageMacros.NLMSG_OK(msgHeader, numBytes) == false)
                    break;

                numBytes -= (int)msgHeader.MessageLength;

                if (msgHeader.MessageType == NetlinkMessageType.NLMSG_DONE)
                    continue;

                if (msgHeader.MessageType != NetlinkMessageType.RTM_NEWLINK)
                    continue;

                Assert.AreEqual(NetlinkMessageType.RTM_NEWLINK, msgHeader.MessageType);
                Assert.AreNotEqual(NetlinkMessageType.NLMSG_DONE, msgHeader.MessageType);

                byte[] ifiData = rxBuffer.Skip(offset).Take(NetlinkMessageMacros.NLMSG_PAYLOAD(msgHeader, 0)).ToArray();
                offset += NetlinkMessageMacros.NLMSG_ALIGN(Marshal.SizeOf<InterfaceInfoMessage>());
                InterfaceInfoMessage interfaceInfo = InterfaceInfoMessage.FromBytes(ifiData);
                Console.WriteLine($"AF: {interfaceInfo.AddressFamily}");
                Console.WriteLine($"Device Type: {interfaceInfo.DeviceType}");
                Console.WriteLine($"Interface Idx: {interfaceInfo.InterfaceIndex}");
                Console.WriteLine($"Device Flags: {interfaceInfo.DeviceFlags}");
                Console.WriteLine($"Change Mask: {interfaceInfo.ChangeMask}");

                Assert.AreEqual(0, interfaceInfo.AddressFamily);
                Assert.AreEqual(ArpHardwareIdentifier.ARPHRD_CAN, interfaceInfo.DeviceType);
                Assert.AreEqual(ifr.IfIndex, interfaceInfo.InterfaceIndex);

                List<InterfaceLinkAttribute> iflaList = NetlinkUtils.ParseInterfaceLinkAttributes(rxBuffer, ref offset);
                foreach (var attr in iflaList)
                {
                    Console.WriteLine($"Type: {attr.Type}");
                    Console.WriteLine($"Data: {BitConverter.ToString(attr.Data)}");

                    switch (attr.Type)
                    {
                        case InterfaceLinkAttributeType.IFLA_IFNAME:
                            {
                                string name = Encoding.ASCII.GetString(attr.Data).Trim('\0');
                                Assert.AreEqual("vcan0", name);
                                Console.WriteLine($"Name: {name}");
                                break;
                            }
                        case InterfaceLinkAttributeType.IFLA_TXQLEN:
                            {
                                uint qlen = BitConverter.ToUInt32(attr.Data);
                                Assert.NotZero(qlen);
                                Console.WriteLine($"TXQLEN: {qlen}");
                                break;
                            }
                        case InterfaceLinkAttributeType.IFLA_OPERSTATE:
                            {
                                InterfaceOperationalStatus opState = (InterfaceOperationalStatus)attr.Data[0];
                                Assert.IsTrue(Enum.IsDefined<InterfaceOperationalStatus>(opState));
                                Console.WriteLine($"OPERSTATE: {opState}");
                                break;
                            }
                        case InterfaceLinkAttributeType.IFLA_LINKMODE:
                            {
                                InterfaceLinkMode linkMode = (InterfaceLinkMode)attr.Data[0];
                                Assert.IsTrue(Enum.IsDefined<InterfaceLinkMode>(linkMode));
                                Console.WriteLine($"LINKMODE: {linkMode}");
                                break;
                            }
                        case InterfaceLinkAttributeType.IFLA_MTU:
                            {
                                uint mtu = BitConverter.ToUInt32(attr.Data);
                                Assert.NotZero(mtu);
                                Console.WriteLine($"MTU: {mtu}");
                                break;
                            }
                        case InterfaceLinkAttributeType.IFLA_MIN_MTU:
                            {
                                uint mtu = BitConverter.ToUInt32(attr.Data);
                                Assert.GreaterOrEqual(mtu, 0);
                                Console.WriteLine($"Minimun MTU: {mtu}");
                                break;
                            }
                        case InterfaceLinkAttributeType.IFLA_MAX_MTU:
                            {
                                uint mtu = BitConverter.ToUInt32(attr.Data);
                                Assert.GreaterOrEqual(mtu, 0);
                                Console.WriteLine($"Maximum MTU: {mtu}");
                                break;
                            }
                        case InterfaceLinkAttributeType.IFLA_GROUP:
                            {
                                uint groupNumber = BitConverter.ToUInt32(attr.Data);
                                Assert.GreaterOrEqual(groupNumber, 0);
                                Console.WriteLine($"Group Number: {groupNumber}");
                                break;
                            }
                        case InterfaceLinkAttributeType.IFLA_PROMISCUITY:
                            {
                                uint promiscuityCount = BitConverter.ToUInt32(attr.Data);
                                Assert.GreaterOrEqual(promiscuityCount, 0);
                                Console.WriteLine($"Promiscuity Count: {promiscuityCount}");
                                break;
                            }
                        case InterfaceLinkAttributeType.IFLA_NUM_TX_QUEUES:
                            {
                                uint numberTxQueues = BitConverter.ToUInt32(attr.Data);
                                Assert.GreaterOrEqual(numberTxQueues, 0);
                                Assert.LessOrEqual(numberTxQueues, 4096);
                                Console.WriteLine($"Number of Transmit Queues: {numberTxQueues}");
                                break;
                            }
                        case InterfaceLinkAttributeType.IFLA_GSO_MAX_SEGS:
                            {
                                uint gsoMaxSegs = BitConverter.ToUInt32(attr.Data);
                                Assert.GreaterOrEqual(gsoMaxSegs, 0);
                                Console.WriteLine($"Maximum number of segments that can be passed to the NIC for GSO: {gsoMaxSegs}");
                                break;
                            }
                        case InterfaceLinkAttributeType.IFLA_GSO_MAX_SIZE:
                            {
                                uint gsoMaxSize = BitConverter.ToUInt32(attr.Data);
                                Assert.GreaterOrEqual(gsoMaxSize, 0);
                                Console.WriteLine($"Maximum size of generic segmentation offload: {gsoMaxSize}");
                                break;
                            }
                        case InterfaceLinkAttributeType.IFLA_GRO_MAX_SIZE:
                            {
                                uint groMaxSize = BitConverter.ToUInt32(attr.Data);
                                Assert.GreaterOrEqual(groMaxSize, 0);
                                Console.WriteLine($"Maximum size of aggregated packet in generic receive offload: {groMaxSize}");
                                break;
                            }
                        case InterfaceLinkAttributeType.IFLA_NUM_RX_QUEUES:
                            {
                                uint numberRxQueues = BitConverter.ToUInt32(attr.Data);
                                Assert.GreaterOrEqual(numberRxQueues, 0);
                                Assert.LessOrEqual(numberRxQueues, 4096);
                                Console.WriteLine($"Number of Receive Queues: {numberRxQueues}");
                                break;
                            }
                        case InterfaceLinkAttributeType.IFLA_CARRIER:
                            {
                                bool carrierOK = attr.Data[0] != 0x00;
                                Assert.GreaterOrEqual(attr.Data[0], 0);
                                Console.WriteLine($"Carrier OK: {carrierOK}");
                                break;
                            }
                        case InterfaceLinkAttributeType.IFLA_QDISC:
                            {
                                string queueingDiscipline = Encoding.ASCII.GetString(attr.Data).Trim('\0');
                                Assert.AreEqual("noqueue", queueingDiscipline);
                                Console.WriteLine($"Queueing Discipline: {queueingDiscipline}");
                                break;
                            }
                        case InterfaceLinkAttributeType.IFLA_CARRIER_CHANGES:
                            {
                                uint numberOfChanges = BitConverter.ToUInt32(attr.Data);
                                Assert.GreaterOrEqual(numberOfChanges, 0);
                                Console.WriteLine($"Number of Carrier Changes: {numberOfChanges}");
                                break;
                            }
                        case InterfaceLinkAttributeType.IFLA_CARRIER_UP_COUNT:
                            {
                                uint carrierUpCount = BitConverter.ToUInt32(attr.Data);
                                Assert.GreaterOrEqual(carrierUpCount, 0);
                                Console.WriteLine($"Carrier Up Count: {carrierUpCount}");
                                break;
                            }
                        case InterfaceLinkAttributeType.IFLA_CARRIER_DOWN_COUNT:
                            {
                                uint carrierDownCount = BitConverter.ToUInt32(attr.Data);
                                Assert.GreaterOrEqual(carrierDownCount, 0);
                                Console.WriteLine($"Carrier Down Count: {carrierDownCount}");
                                break;
                            }
                        case InterfaceLinkAttributeType.IFLA_PROTO_DOWN:
                            {
                                bool protodown = attr.Data[0] != 0x00;
                                Assert.GreaterOrEqual(attr.Data[0], 0);
                                Console.WriteLine($"Protodown On: {protodown}");
                                break;
                            }
                        case InterfaceLinkAttributeType.IFLA_MAP:
                            {
                                InterfaceMap map = InterfaceMap.FromBytes(attr.Data);
                                Assert.IsNotNull(map);
                                Console.WriteLine($"Interface Map:{Environment.NewLine}{map}");
                                break;
                            }
                        case InterfaceLinkAttributeType.IFLA_STATS64:
                            {
                                InterfaceLinkStatistics64 stats = InterfaceLinkStatistics64.FromBytes(attr.Data);
                                Assert.IsNotNull(stats);
                                Console.WriteLine($"STATS64:{Environment.NewLine}{stats}");
                                break;
                            }
                        case InterfaceLinkAttributeType.IFLA_STATS:
                            {
                                InterfaceLinkStatistics stats = InterfaceLinkStatistics.FromBytes(attr.Data);
                                Assert.IsNotNull(stats);
                                Console.WriteLine($"STATS:{Environment.NewLine}{stats}");
                                break;
                            }
                        case InterfaceLinkAttributeType.IFLA_XDP:
                            {
                                int rtaLen = Marshal.SizeOf<RoutingAttribute>();
                                RoutingAttribute attribute = RoutingAttribute.FromBytes(attr.Data.Take(rtaLen).ToArray());
                                Assert.IsNotNull(attribute);
                                Assert.AreEqual(5, attribute.Length);
                                Assert.AreEqual(2, attribute.Type);
                                byte[] xdpData = attr.Data.Skip(rtaLen).Take(attribute.Length - rtaLen).ToArray();
                                Assert.IsTrue(xdpData.SequenceEqual(new byte[] { 0x00 }));
                                Console.WriteLine($"Nested XDP Data: {BitConverter.ToString(xdpData)}");
                                break;
                            }
                        case InterfaceLinkAttributeType.IFLA_LINKINFO:
                            {
                                List<LinkInfoAttribute> liaList = NetlinkUtils.ParseNestedLinkInfoAttributes(attr.Data);
                                Console.WriteLine("Link Info Nested Data");
                                foreach (var lattr in liaList)
                                {
                                    Console.WriteLine($"Type: {lattr.Type}");
                                    Console.WriteLine($"Data: {BitConverter.ToString(lattr.Data)}");

                                    switch (lattr.Type)
                                    {
                                        case LinkInfoAttributeType.IFLA_INFO_KIND:
                                            {
                                                Console.WriteLine($"Kind: {Encoding.ASCII.GetString(lattr.Data).Trim('\0')}");
                                                break;
                                            }
                                        case LinkInfoAttributeType.IFLA_INFO_XSTATS:
                                            {
                                                break;
                                            }
                                        case LinkInfoAttributeType.IFLA_INFO_DATA:
                                            {
                                                break;
                                            }
                                    }
                                }
                                break;
                            }
                    }
                }
            }
        }

        [Test]
        public void NETLINK_ROUTE_Write_GETLINK_REQUEST_WrongInterfaceIndex_Test()
        {
            var ifr = new Ifreq("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
            Assert.AreNotEqual(-1, ioctlResult);

            var req = new NetworkInterfaceInfoRequest()
            {
                Header = new NetlinkMessageHeader()
                {
                    MessageLength = (uint)Marshal.SizeOf(typeof(NetworkInterfaceInfoRequest)),
                    MessageType = NetlinkMessageType.RTM_GETLINK,
                    Flags = NetlinkMessageFlags.NLM_F_REQUEST,
                    SenderPortId = 0,
                    SequenceNumber = 0,
                },
                Information = new InterfaceInfoMessage()
                {
                    AddressFamily = NetlinkConstants.AF_NETLINK,
                    InterfaceIndex = ifr.IfIndex + 100,
                }
            };
            int size = Marshal.SizeOf(typeof(NetworkInterfaceInfoRequest));
            int numBytes = NetlinkNativeMethods.Write(socketHandle, req, size);
            Assert.AreEqual(size, numBytes);
        }

        [Test]
        public void NETLINK_ROUTE_Write_GETLINK_REQUEST_WrongAddressFamily_Test()
        {
            var ifr = new Ifreq("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
            Assert.AreNotEqual(-1, ioctlResult);

            var req = new NetworkInterfaceInfoRequest()
            {
                Header = new NetlinkMessageHeader()
                {
                    MessageLength = (uint)Marshal.SizeOf(typeof(NetworkInterfaceInfoRequest)),
                    MessageType = NetlinkMessageType.RTM_GETLINK,
                    Flags = NetlinkMessageFlags.NLM_F_REQUEST,
                    SenderPortId = 0,
                    SequenceNumber = 0,
                },
                Information = new InterfaceInfoMessage()
                {
                    AddressFamily = SocketCanConstants.AF_CAN, // Wrong Address Family
                    InterfaceIndex = ifr.IfIndex,
                }
            };
            int size = Marshal.SizeOf(typeof(NetworkInterfaceInfoRequest));
            int numBytes = NetlinkNativeMethods.Write(socketHandle, req, size);
            Assert.AreEqual(size, numBytes);
        }

        [Test]
        public void NETLINK_ROUTE_Read_LinkInfo_NoResponse_NoRequestFlag_Test()
        {
            var ifr = new Ifreq("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
            Assert.AreNotEqual(-1, ioctlResult);

            var req = new NetworkInterfaceInfoRequest()
            {
                Header = new NetlinkMessageHeader()
                {
                    MessageLength = (uint)Marshal.SizeOf<NetworkInterfaceInfoRequest>(),
                    MessageType = NetlinkMessageType.RTM_GETLINK,
                    Flags = NetlinkMessageFlags.NLM_F_MATCH, // No Request Flag
                    SenderPortId = 0,
                    SequenceNumber = 0,
                },
                Information = new InterfaceInfoMessage()
                {
                    AddressFamily = NetlinkConstants.AF_NETLINK,
                    InterfaceIndex = ifr.IfIndex,
                }
            };
            int numBytes = NetlinkNativeMethods.Write(socketHandle, req, (int)req.Header.MessageLength);
            Assert.AreEqual((int)req.Header.MessageLength, numBytes);

            byte[] rxBuffer = new byte[8192];
            numBytes = LibcNativeMethods.Read(socketHandle, rxBuffer, rxBuffer.Length);
            Assert.AreEqual(-1, numBytes);
            Assert.AreEqual(11, LibcNativeMethods.Errno); // EWOULDBLOCK
        }

        [Test]
        public void RoutingNetlinkSocket_Write_Info_Request_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var rtNetlinkSocket = new RoutingNetlinkSocket())
            {
                rtNetlinkSocket.ReceiveBufferSize = 32768;
                rtNetlinkSocket.SendBufferSize = 32768;
                rtNetlinkSocket.ReceiveTimeout = 1000;
                rtNetlinkSocket.Bind(new SockAddrNetlink(0, 0));
                var req = new NetworkInterfaceInfoRequest()
                {
                    Header = new NetlinkMessageHeader()
                    {
                        MessageLength = (uint)Marshal.SizeOf<NetworkInterfaceInfoRequest>(),
                        MessageType = NetlinkMessageType.RTM_GETLINK,
                        Flags = NetlinkMessageFlags.NLM_F_REQUEST,
                        SenderPortId = 0,
                        SequenceNumber = 0,
                    },
                    Information = new InterfaceInfoMessage()
                    {
                        AddressFamily = NetlinkConstants.AF_NETLINK,
                        InterfaceIndex = iface.Index,
                    }
                };
                int numBytes = rtNetlinkSocket.Write(req);
                Assert.AreEqual((int)req.Header.MessageLength, numBytes);
            }
        }

        [Test]
        public void RoutingNetlinkSocket_Read_Link_Info_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var rtNetlinkSocket = new RoutingNetlinkSocket())
            {
                rtNetlinkSocket.ReceiveBufferSize = 32768;
                rtNetlinkSocket.SendBufferSize = 32768;
                rtNetlinkSocket.ReceiveTimeout = 1000;
                rtNetlinkSocket.Bind(new SockAddrNetlink(0, 0));
                var req = new NetworkInterfaceInfoRequest()
                {
                    Header = new NetlinkMessageHeader()
                    {
                        MessageLength = (uint)Marshal.SizeOf<NetworkInterfaceInfoRequest>(),
                        MessageType = NetlinkMessageType.RTM_GETLINK,
                        Flags = NetlinkMessageFlags.NLM_F_REQUEST,
                        SenderPortId = 0,
                        SequenceNumber = 0,
                    },
                    Information = new InterfaceInfoMessage()
                    {
                        AddressFamily = NetlinkConstants.AF_NETLINK,
                        InterfaceIndex = iface.Index,
                    }
                };
                int numBytes = rtNetlinkSocket.Write(req);
                Assert.AreEqual((int)req.Header.MessageLength, numBytes);

                byte[] rxBuffer = new byte[8192];
                numBytes = rtNetlinkSocket.Read(rxBuffer);
                Assert.AreNotEqual(-1, numBytes, $"Errno: {LibcNativeMethods.Errno}");
                Assert.Greater(numBytes, 0);
            }
        }

        [Test]
        public void RoutingNetlinkSocket_Read_Link_Info_BadInterfaceId_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var rtNetlinkSocket = new RoutingNetlinkSocket())
            {
                rtNetlinkSocket.ReceiveBufferSize = 32768;
                rtNetlinkSocket.SendBufferSize = 32768;
                rtNetlinkSocket.ReceiveTimeout = 1000;
                rtNetlinkSocket.Bind(new SockAddrNetlink(0, 0));
                var req = new NetworkInterfaceInfoRequest()
                {
                    Header = new NetlinkMessageHeader()
                    {
                        MessageLength = (uint)Marshal.SizeOf<NetworkInterfaceInfoRequest>(),
                        MessageType = NetlinkMessageType.RTM_GETLINK,
                        Flags = NetlinkMessageFlags.NLM_F_REQUEST,
                        SenderPortId = 0,
                        SequenceNumber = 0,
                    },
                    Information = new InterfaceInfoMessage()
                    {
                        AddressFamily = NetlinkConstants.AF_NETLINK,
                        InterfaceIndex = int.MaxValue, // should be non-existent...
                    }
                };
                int numBytes = rtNetlinkSocket.Write(req);
                Assert.AreEqual((int)req.Header.MessageLength, numBytes);

                byte[] rxBuffer = new byte[8192];
                numBytes = rtNetlinkSocket.Read(rxBuffer);
                Assert.AreNotEqual(-1, numBytes, $"Errno: {LibcNativeMethods.Errno}");
                Assert.Greater(numBytes, 0);

                byte[] headerData = rxBuffer.Take(NetlinkMessageMacros.NLMSG_HDRLEN).ToArray();
                NetlinkMessageHeader msgHeader = NetlinkMessageHeader.FromBytes(headerData);
                Assert.AreEqual(NetlinkMessageType.NLMSG_ERROR, msgHeader.MessageType);
            }
        }
    }
}