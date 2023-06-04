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
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SocketCANSharp;
using SocketCANSharp.Network;
using SocketCANSharp.Network.Netlink;

namespace CanNetlinkReader
{
    class Program
    {
        static void Main(string[] args)
        {
            string canInterfaceName = "can0";
            using (var socketHandle = NetlinkNativeMethods.Socket(NetlinkConstants.PF_NETLINK, SocketType.Raw, NetlinkProtocolType.NETLINK_ROUTE))
            {
                int sendBufferSize = 32768;
                int recvBufferSize = 32768;
                int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_SOCKET, SocketLevelOptions.SO_SNDBUF, ref sendBufferSize, Marshal.SizeOf(typeof(int)));
                if (result == -1)
                {
                    Console.WriteLine($"Failed to set send buffer size. Errno: {LibcNativeMethods.Errno}");
                    return;
                }
                
                result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_SOCKET, SocketLevelOptions.SO_RCVBUF, ref recvBufferSize, Marshal.SizeOf(typeof(int)));
                if (result == -1)
                {
                    Console.WriteLine($"Failed to set receive buffer size. Errno: {LibcNativeMethods.Errno}");
                    return;
                }

                var timeval = new Timeval(1, 0);
                result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_SOCKET, SocketLevelOptions.SO_RCVTIMEO, timeval, Marshal.SizeOf(typeof(Timeval)));
                if (result == -1)
                {
                    Console.WriteLine($"Failed to set receive timeout. Errno: {LibcNativeMethods.Errno}");
                    return;
                }
                
                var addr = new SockAddrNetlink(0, 0);
                int bindResult = NetlinkNativeMethods.Bind(socketHandle, addr, Marshal.SizeOf(typeof(SockAddrNetlink)));
                if (bindResult == -1)
                {
                    Console.WriteLine($"Failed to bind to netlink interface. Errno: {LibcNativeMethods.Errno}");
                    return;
                }

                Console.WriteLine($"Process ID: {System.Environment.ProcessId}");
                var ifr = new Ifreq(canInterfaceName);
                int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
                if (ioctlResult == -1)
                {
                    Console.WriteLine($"Failed to get CAN interface index for {canInterfaceName}. Errno: {LibcNativeMethods.Errno}");
                    return;
                }
                
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
                if (numBytes == -1)
                {
                    Console.WriteLine($"Failed to write to netlink socket. Errno: {LibcNativeMethods.Errno}");
                    return;
                };

                byte[] rxBuffer = new byte[8192];
                numBytes = LibcNativeMethods.Read(socketHandle, rxBuffer, rxBuffer.Length);
                if (numBytes == -1)
                {
                    Console.WriteLine($"Failed to read from netlink socket. Errno: {LibcNativeMethods.Errno}");
                    return;
                };

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

                    byte[] ifiData = rxBuffer.Skip(offset).Take(NetlinkMessageMacros.NLMSG_PAYLOAD(msgHeader, 0)).ToArray();
                    offset += NetlinkMessageMacros.NLMSG_ALIGN(Marshal.SizeOf<InterfaceInfoMessage>());
                    InterfaceInfoMessage interfaceInfo = InterfaceInfoMessage.FromBytes(ifiData);
                    Console.WriteLine($"AF: {interfaceInfo.AddressFamily}");
                    Console.WriteLine($"Device Type: {interfaceInfo.DeviceType}");
                    Console.WriteLine($"Interface Idx: {interfaceInfo.InterfaceIndex}");
                    Console.WriteLine($"Device Flags: {interfaceInfo.DeviceFlags}");
                    Console.WriteLine($"Change Mask: {interfaceInfo.ChangeMask}");

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
                                    Console.WriteLine($"Name: {name}");
                                    break;
                                }
                            case InterfaceLinkAttributeType.IFLA_PARENT_DEV_NAME:
                                {
                                    string name = Encoding.ASCII.GetString(attr.Data).Trim('\0');
                                    Console.WriteLine($"Name: {name}");
                                    break;
                                }
                            case InterfaceLinkAttributeType.IFLA_PARENT_DEV_BUS_NAME:
                                {
                                    string name = Encoding.ASCII.GetString(attr.Data).Trim('\0');
                                    Console.WriteLine($"Name: {name}");
                                    break;
                                }
                            case InterfaceLinkAttributeType.IFLA_TXQLEN:
                                {
                                    uint qlen = BitConverter.ToUInt32(attr.Data);
                                    Console.WriteLine($"TXQLEN: {qlen}");
                                    break;
                                }
                            case InterfaceLinkAttributeType.IFLA_OPERSTATE:
                                {
                                    InterfaceOperationalStatus opState = (InterfaceOperationalStatus)attr.Data[0];
                                    Console.WriteLine($"OPERSTATE: {opState}");
                                    break;
                                }
                            case InterfaceLinkAttributeType.IFLA_LINKMODE:
                                {
                                    InterfaceLinkMode linkMode = (InterfaceLinkMode)attr.Data[0];;
                                    Console.WriteLine($"LINKMODE: {linkMode}");
                                    break;
                                }
                            case InterfaceLinkAttributeType.IFLA_MTU:
                                {
                                    uint mtu = BitConverter.ToUInt32(attr.Data);
                                    Console.WriteLine($"MTU: {mtu}");
                                    break;
                                }
                            case InterfaceLinkAttributeType.IFLA_MIN_MTU:
                                {
                                    uint mtu = BitConverter.ToUInt32(attr.Data);
                                    Console.WriteLine($"Minimun MTU: {mtu}");
                                    break;
                                }
                            case InterfaceLinkAttributeType.IFLA_MAX_MTU:
                                {
                                    uint mtu = BitConverter.ToUInt32(attr.Data);
                                    Console.WriteLine($"Maximum MTU: {mtu}");
                                    break;
                                }
                            case InterfaceLinkAttributeType.IFLA_GROUP:
                                {
                                    uint groupNumber = BitConverter.ToUInt32(attr.Data);
                                    Console.WriteLine($"Group Number: {groupNumber}");
                                    break;
                                }
                            case InterfaceLinkAttributeType.IFLA_PROMISCUITY:
                                {
                                    uint promiscuityCount = BitConverter.ToUInt32(attr.Data);
                                    Console.WriteLine($"Promiscuity Count: {promiscuityCount}");
                                    break;
                                }
                            case InterfaceLinkAttributeType.IFLA_NUM_TX_QUEUES:
                                {
                                    uint numberTxQueues = BitConverter.ToUInt32(attr.Data);
                                    Console.WriteLine($"Number of Transmit Queues: {numberTxQueues}");
                                    break;
                                }
                            case InterfaceLinkAttributeType.IFLA_GSO_MAX_SEGS:
                                {
                                    uint gsoMaxSegs = BitConverter.ToUInt32(attr.Data);
                                    Console.WriteLine($"Maximum number of segments that can be passed to the NIC for GSO: {gsoMaxSegs}");
                                    break;
                                }
                            case InterfaceLinkAttributeType.IFLA_GSO_MAX_SIZE:
                                {
                                    uint gsoMaxSize = BitConverter.ToUInt32(attr.Data);
                                    Console.WriteLine($"Maximum size of generic segmentation offload: {gsoMaxSize}");
                                    break;
                                }
                            case InterfaceLinkAttributeType.IFLA_GRO_MAX_SIZE:
                                {
                                    uint groMaxSize = BitConverter.ToUInt32(attr.Data);
                                    Console.WriteLine($"Maximum size of aggregated packet in generic receive offload: {groMaxSize}");
                                    break;
                                }
                            case InterfaceLinkAttributeType.IFLA_NUM_RX_QUEUES:
                                {
                                    uint numberRxQueues = BitConverter.ToUInt32(attr.Data);
                                    Console.WriteLine($"Number of Receive Queues: {numberRxQueues}");
                                    break;
                                }
                            case InterfaceLinkAttributeType.IFLA_CARRIER:
                                {
                                    bool carrierOK = attr.Data[0] != 0x00;
                                    Console.WriteLine($"Carrier OK: {carrierOK}");
                                    break;
                                }
                            case InterfaceLinkAttributeType.IFLA_QDISC:
                                {
                                    string queueingDiscipline = Encoding.ASCII.GetString(attr.Data).Trim('\0');
                                    Console.WriteLine($"Queueing Discipline: {queueingDiscipline}");
                                    break;
                                }
                            case InterfaceLinkAttributeType.IFLA_CARRIER_CHANGES:
                                {
                                    uint numberOfChanges = BitConverter.ToUInt32(attr.Data);;
                                    Console.WriteLine($"Number of Carrier Changes: {numberOfChanges}");
                                    break;
                                }
                            case InterfaceLinkAttributeType.IFLA_CARRIER_UP_COUNT:
                                {
                                    uint carrierUpCount = BitConverter.ToUInt32(attr.Data);
                                    Console.WriteLine($"Carrier Up Count: {carrierUpCount}");
                                    break;
                                }
                            case InterfaceLinkAttributeType.IFLA_CARRIER_DOWN_COUNT:
                                {
                                    uint carrierDownCount = BitConverter.ToUInt32(attr.Data);
                                    Console.WriteLine($"Carrier Down Count: {carrierDownCount}");
                                    break;
                                }
                            case InterfaceLinkAttributeType.IFLA_PROTO_DOWN:
                                {
                                    bool protodown = attr.Data[0] != 0x00;;
                                    Console.WriteLine($"Protodown On: {protodown}");
                                    break;
                                }
                            case InterfaceLinkAttributeType.IFLA_MAP:
                                {
                                    InterfaceMap map = InterfaceMap.FromBytes(attr.Data);
                                    Console.WriteLine($"Interface Map:{Environment.NewLine}{map}");
                                    break;
                                }
                            case InterfaceLinkAttributeType.IFLA_STATS64:
                                {
                                    InterfaceLinkStatistics64 stats = InterfaceLinkStatistics64.FromBytes(attr.Data);
                                    Console.WriteLine($"STATS64:{Environment.NewLine}{stats}");
                                    break;
                                }
                            case InterfaceLinkAttributeType.IFLA_STATS:
                                {
                                    InterfaceLinkStatistics stats = InterfaceLinkStatistics.FromBytes(attr.Data);
                                    Console.WriteLine($"STATS:{Environment.NewLine}{stats}");
                                    break;
                                }
                            case InterfaceLinkAttributeType.IFLA_XDP:
                                {
                                    int rtaLen = Marshal.SizeOf<RoutingAttribute>();
                                    RoutingAttribute attribute = RoutingAttribute.FromBytes(attr.Data.Take(rtaLen).ToArray());
                                    byte[] xdpData = attr.Data.Skip(rtaLen).Take(attribute.Length - rtaLen).ToArray();
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
                                                    CanDeviceStatistics stats = CanDeviceStatistics.FromBytes(lattr.Data);
                                                    Console.WriteLine($"CAN Device Statistics:{Environment.NewLine}{stats}");
                                                    break;
                                                }
                                            case LinkInfoAttributeType.IFLA_INFO_DATA:
                                                {
                                                    List<CanRoutingAttribute> craList = NetlinkUtils.ParseNestedCanRoutingAttributes(lattr.Data);
                                                    Console.WriteLine("CAN Routing Nested Data");
                                                    foreach (var cra in craList)
                                                    {
                                                        Console.WriteLine($"Type: {cra.Type}");
                                                        Console.WriteLine($"Data: {BitConverter.ToString(cra.Data)}");

                                                        switch (cra.Type)
                                                        {
                                                            case CanRoutingAttributeType.IFLA_CAN_BITTIMING:
                                                                {
                                                                    CanBitTiming timing = CanBitTiming.FromBytes(cra.Data);
                                                                    Console.WriteLine($"CAN Bit Timing:{Environment.NewLine}{timing}");
                                                                    break;
                                                                }
                                                            case CanRoutingAttributeType.IFLA_CAN_BITTIMING_CONST:
                                                                {
                                                                    CanBitTimingConstant timing = CanBitTimingConstant.FromBytes(cra.Data);
                                                                    Console.WriteLine($"CAN Bit Timing Constant:{Environment.NewLine}{timing}");
                                                                    break;
                                                                }
                                                            case CanRoutingAttributeType.IFLA_CAN_CLOCK:
                                                                {
                                                                    CanClock clock = CanClock.FromBytes(cra.Data);
                                                                    Console.WriteLine($"CAN Clock:{Environment.NewLine}{clock}");
                                                                    break;
                                                                }
                                                            case CanRoutingAttributeType.IFLA_CAN_STATE:
                                                                {
                                                                    CanState state = (CanState)BitConverter.ToUInt32(cra.Data);
                                                                    Console.WriteLine($"CAN State: {state}");
                                                                    break;
                                                                }
                                                            case CanRoutingAttributeType.IFLA_CAN_CTRLMODE:
                                                                {
                                                                    CanControllerMode mode = CanControllerMode.FromBytes(cra.Data);
                                                                    Console.WriteLine($"CAN Controller Mode:{Environment.NewLine}{mode}");
                                                                    break;
                                                                }
                                                            case CanRoutingAttributeType.IFLA_CAN_RESTART_MS:
                                                                {
                                                                    uint restartMilliseconds = BitConverter.ToUInt32(cra.Data);
                                                                    Console.WriteLine($"CAN Restart Delay (Milliseconds): {restartMilliseconds}");
                                                                    break;
                                                                }
                                                            case CanRoutingAttributeType.IFLA_CAN_BERR_COUNTER:
                                                                {
                                                                    CanBusErrorCounter counter = CanBusErrorCounter.FromBytes(cra.Data);
                                                                    Console.WriteLine($"CAN Bus Error Counters:{Environment.NewLine}{counter}");
                                                                    break;
                                                                }
                                                            case CanRoutingAttributeType.IFLA_CAN_BITRATE_MAX:
                                                                {
                                                                    uint bitRateMax = BitConverter.ToUInt32(cra.Data);
                                                                    Console.WriteLine($"CAN Bit Rate Maximum: {bitRateMax}");
                                                                    break;
                                                                }
                                                            case CanRoutingAttributeType.IFLA_CAN_BITRATE_CONST:
                                                                {
                                                                    uint bitrateConst = BitConverter.ToUInt32(cra.Data);
                                                                    Console.WriteLine($"CAN (Arbitration Phase) Bitrate Constant: {bitrateConst}");
                                                                    break;
                                                                }
                                                            case CanRoutingAttributeType.IFLA_CAN_DATA_BITRATE_CONST:
                                                                {
                                                                    uint dataBitrateConst = BitConverter.ToUInt32(cra.Data);
                                                                    Console.WriteLine($"CAN Data Phase Bitrate Constant: {dataBitrateConst}");
                                                                    break;
                                                                }
                                                        }
                                                    }

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

            Console.WriteLine("+++++++++++++++++++++++++++++++");
            Console.WriteLine("Testing Object-Oriented Code...");
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            var iface = collection.FirstOrDefault(i =>  i.Name.Equals(canInterfaceName));

            if (iface == null)
            {
                Console.WriteLine($"Failed to find CAN interface {canInterfaceName}. Errno: {LibcNativeMethods.Errno}");
                return;
            }

            Console.WriteLine($"Name:{Environment.NewLine}{iface.Name}");
            Console.WriteLine($"DeviceType:{Environment.NewLine}{iface.DeviceType}");
            Console.WriteLine($"DeviceFlags:{Environment.NewLine}{iface.DeviceFlags}");
            Console.WriteLine($"LinkStats:{Environment.NewLine}{iface.LinkStatistics}");
            Console.WriteLine($"OpStatus:{Environment.NewLine}{iface.OperationalStatus}");
            Console.WriteLine($"LinkKind:{Environment.NewLine}{iface.LinkKind}");
            Console.WriteLine($"DeviceStats:{Environment.NewLine}{iface.DeviceStatistics}");
            Console.WriteLine($"BitTiming:{Environment.NewLine}{iface.BitTiming}");
            Console.WriteLine($"BitTimingConstant:{Environment.NewLine}{iface.BitTimingConstant}");
            Console.WriteLine($"BitrateConstant:{Environment.NewLine}{iface.BitrateConstant}");
            Console.WriteLine($"DataPhaseBitrateConstant:{Environment.NewLine}{iface.DataPhaseBitrateConstant}");
            Console.WriteLine($"MaximumTransmissionUnit:{Environment.NewLine}{iface.MaximumTransmissionUnit}");
        }
    }
}
