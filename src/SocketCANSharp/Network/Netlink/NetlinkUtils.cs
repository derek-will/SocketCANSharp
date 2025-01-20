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
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SocketCANSharp.Network.Netlink
{
    /// <summary>
    /// Netlink Utilities.
    /// </summary>
    public static class NetlinkUtils
    {
        /// <summary>
        /// Parse Interface Link Attributes (IFLA).
        /// </summary>
        /// <param name="rxBuffer">Receive Buffer</param>
        /// <param name="offset">Offset into buffer</param>
        /// <returns>Interface Link Attribute Collection</returns>
        /// <exception cref="ArgumentNullException">Receive Buffer is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">Offset is negative</exception>
        public static List<InterfaceLinkAttribute> ParseInterfaceLinkAttributes(byte[] rxBuffer, ref int offset)
        {
            if (rxBuffer == null)
                throw new ArgumentNullException(nameof(rxBuffer));

            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), "Offset must be non-negative.");

            var iflaList = new List<InterfaceLinkAttribute>();
            int len = rxBuffer.Length;
            int rtaLen = Marshal.SizeOf<RoutingAttribute>();

            do
            {
                byte[] rtaData = rxBuffer.Skip(offset).Take(rtaLen).ToArray();
                if (rtaData.Length < rtaLen)
                    break;

                RoutingAttribute rta = RoutingAttribute.FromBytes(rtaData);
                if (NetlinkMessageMacros.RTA_OK(rta, len) == false)
                    break;

                byte[] attrData = rxBuffer.Skip(offset + NetlinkMessageMacros.RTA_LENGTH(0)).Take(NetlinkMessageMacros.RTA_PAYLOAD(rta)).ToArray();
                var ifLinkAttribute = new InterfaceLinkAttribute(rta.Type, attrData);
                iflaList.Add(ifLinkAttribute);

                len -= NetlinkMessageMacros.RTA_ALIGN(rta.Length);
                offset += NetlinkMessageMacros.RTA_ALIGN(rta.Length);

            } while (len > 0);

            return iflaList;
        }

        /// <summary>
        /// Parse Nested Link Info Attributes.
        /// </summary>
        /// <param name="rxBuffer">Receive Buffer</param>
        /// <returns>Nested Link Info Attributes Collection</returns>
        /// <exception cref="ArgumentNullException">Receive Buffer is null</exception>
        public static List<LinkInfoAttribute> ParseNestedLinkInfoAttributes(byte[] rxBuffer)
        {
            if (rxBuffer == null)
                throw new ArgumentNullException(nameof(rxBuffer));

            var liaList = new List<LinkInfoAttribute>();
            int len = rxBuffer.Length;
            int rtaLen = Marshal.SizeOf<RoutingAttribute>();
            int offset = 0;

            do
            {
                byte[] rtaData = rxBuffer.Skip(offset).Take(rtaLen).ToArray();
                if (rtaData.Length < rtaLen)
                    break;

                RoutingAttribute rta = RoutingAttribute.FromBytes(rtaData);
                if (NetlinkMessageMacros.RTA_OK(rta, len) == false)
                    break;

                byte[] attrData = rxBuffer.Skip(offset + NetlinkMessageMacros.RTA_LENGTH(0)).Take(NetlinkMessageMacros.RTA_PAYLOAD(rta)).ToArray();
                var liAttribute = new LinkInfoAttribute(rta.Type, attrData);
                liaList.Add(liAttribute);

                len -= NetlinkMessageMacros.RTA_ALIGN(rta.Length);
                offset += NetlinkMessageMacros.RTA_ALIGN(rta.Length);

            } while (len > 0);

            return liaList;
        }

        /// <summary>
        /// Parse Nested CAN Routing Attributes.
        /// </summary>
        /// <param name="rxBuffer">Receive Buffer</param>
        /// <returns>Nested CAN Routing Attributes Collection</returns>
        /// <exception cref="ArgumentNullException">Receive Buffer is null</exception>
        public static List<CanRoutingAttribute> ParseNestedCanRoutingAttributes(byte[] rxBuffer)
        {
            if (rxBuffer == null)
                throw new ArgumentNullException(nameof(rxBuffer));

            var craList = new List<CanRoutingAttribute>();
            int len = rxBuffer.Length;
            int rtaLen = Marshal.SizeOf<RoutingAttribute>();
            int offset = 0;

            do
            {
                byte[] rtaData = rxBuffer.Skip(offset).Take(rtaLen).ToArray();
                if (rtaData.Length < rtaLen)
                    break;

                RoutingAttribute rta = RoutingAttribute.FromBytes(rtaData);
                if (NetlinkMessageMacros.RTA_OK(rta, len) == false)
                    break;

                byte[] attrData = rxBuffer.Skip(offset + NetlinkMessageMacros.RTA_LENGTH(0)).Take(NetlinkMessageMacros.RTA_PAYLOAD(rta)).ToArray();
                var crAttribute = new CanRoutingAttribute(rta.Type, attrData);
                craList.Add(crAttribute);

                len -= NetlinkMessageMacros.RTA_ALIGN(rta.Length);
                offset += NetlinkMessageMacros.RTA_ALIGN(rta.Length);

            } while (len > 0);

            return craList;
        }

        /// <summary>
        /// Converts a raw byte array into a newly allocated managed object of the type specified by the generic type parameter.
        /// </summary>
        /// <typeparam name="T">Netlink Object Type</typeparam>
        /// <param name="data">Raw byte array</param>
        /// <returns>A managed object that contains the data corresponding to the byte array.</returns>
        public static T FromBytes<T>(byte[] data)
        {
            int size = Marshal.SizeOf<T>();
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.AllocHGlobal(size);
                int copyLength = data.Length < size ? data.Length : size;
                Marshal.Copy(data, 0, ptr, copyLength);
                return Marshal.PtrToStructure<T>(ptr);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        /// <summary>
        /// Converts a managed object of the type specified by the generic type parameter into a raw byte array.
        /// </summary>
        /// <typeparam name="T">Object Type</typeparam>
        /// <param name="obj">Managed Object Instance</param>
        /// <returns>A raw byte array that corresponds to the managed object instance.</returns>
        public static byte[] ToBytes<T>(T obj)
        {
            int size = Marshal.SizeOf<T>();
            byte[] data = new byte[size];
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr<T>(obj, ptr, false);
                Marshal.Copy(ptr, data, 0, size);
                return data;
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        /// <summary>
        /// Retrieves the requested Interface Information Message.
        /// </summary>
        /// <param name="interfaceIndex">Interface Index</param>
        /// <param name="rxBuffer">Receive Buffer</param>
        /// <returns>Requested Interface Information Message, if found; otherwise, null.</returns>
        public static InterfaceInfoMessage? FindInterfaceInfoMessage(int interfaceIndex, byte[] rxBuffer)
        {
            int offset = 0;
            int numBytes = rxBuffer.Length;
            while (numBytes > 0)
            {
                byte[] headerData = rxBuffer.Skip(offset).Take(NetlinkMessageMacros.NLMSG_HDRLEN).ToArray();
                offset += NetlinkMessageMacros.NLMSG_HDRLEN;
                NetlinkMessageHeader msgHeader = NetlinkMessageHeader.FromBytes(headerData);  

                if (NetlinkMessageMacros.NLMSG_OK(msgHeader, numBytes) == false)
                    break;

                numBytes -= (int)msgHeader.MessageLength;

                if (msgHeader.MessageType != NetlinkMessageType.RTM_NEWLINK)
                {
                    offset += NetlinkMessageMacros.NLMSG_PAYLOAD(msgHeader, 0);
                    continue;
                }

                byte[] ifiData = rxBuffer.Skip(offset).Take(NetlinkMessageMacros.NLMSG_PAYLOAD(msgHeader, 0)).ToArray();
                InterfaceInfoMessage msg = InterfaceInfoMessage.FromBytes(ifiData);

                if (msg.InterfaceIndex != interfaceIndex)
                {
                    offset += NetlinkMessageMacros.NLMSG_PAYLOAD(msgHeader, 0);
                    continue;
                }

                return msg;
            }

            return null;
        }

        /// <summary>
        /// Retrieves the requested CAN Routing Attribute.
        /// </summary>
        /// <param name="interfaceIndex">Interface Index</param>
        /// <param name="rxBuffer">Receive Buffer</param>
        /// <param name="type">Requested CAN Routing Attribute</param>
        /// <returns>Requested CAN Routing Attribute, if found; otherwise, null.</returns>
        public static CanRoutingAttribute FindNestedCanRoutingAttribute(int interfaceIndex, byte[] rxBuffer, CanRoutingAttributeType type)
        {
            int offset = 0;
            int numBytes = rxBuffer.Length;
            while (numBytes > 0)
            {
                byte[] headerData = rxBuffer.Skip(offset).Take(NetlinkMessageMacros.NLMSG_HDRLEN).ToArray();
                offset += NetlinkMessageMacros.NLMSG_HDRLEN;
                NetlinkMessageHeader msgHeader = NetlinkMessageHeader.FromBytes(headerData);  

                if (NetlinkMessageMacros.NLMSG_OK(msgHeader, numBytes) == false)
                    break;

                numBytes -= (int)msgHeader.MessageLength;

                if (msgHeader.MessageType != NetlinkMessageType.RTM_NEWLINK)
                {
                    offset += NetlinkMessageMacros.NLMSG_PAYLOAD(msgHeader, 0);
                    continue;
                }

                byte[] ifiData = rxBuffer.Skip(offset).Take(NetlinkMessageMacros.NLMSG_PAYLOAD(msgHeader, 0)).ToArray();
                offset += NetlinkMessageMacros.NLMSG_ALIGN(Marshal.SizeOf<InterfaceInfoMessage>());
                InterfaceInfoMessage interfaceInfo = InterfaceInfoMessage.FromBytes(ifiData);

                if (interfaceInfo.InterfaceIndex != interfaceIndex)
                {
                    offset += NetlinkMessageMacros.NLMSG_PAYLOAD(msgHeader, Marshal.SizeOf<InterfaceInfoMessage>());
                    continue;
                }

                List<InterfaceLinkAttribute> iflaList = NetlinkUtils.ParseInterfaceLinkAttributes(rxBuffer, ref offset);
                InterfaceLinkAttribute linkInfo = iflaList.FirstOrDefault(ifla => ifla.Type == InterfaceLinkAttributeType.IFLA_LINKINFO);
                if (linkInfo != null)
                {
                    List<LinkInfoAttribute> liaList = NetlinkUtils.ParseNestedLinkInfoAttributes(linkInfo.Data);
                    LinkInfoAttribute infoData = liaList.FirstOrDefault(lia => lia.Type == LinkInfoAttributeType.IFLA_INFO_DATA);
                    if (infoData != null)
                    {
                        List<CanRoutingAttribute> craList = NetlinkUtils.ParseNestedCanRoutingAttributes(infoData.Data);
                        return craList.FirstOrDefault(cra => cra.Type == type);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Retrieves the requested Link Information Attribute.
        /// </summary>
        /// <param name="interfaceIndex">Interface Index</param>
        /// <param name="rxBuffer">Receive Buffer</param>
        /// <param name="type">Requested Link Information Attribute</param>
        /// <returns>Requested Link Information Attribute, if found; otherwise, null.</returns>
        public static LinkInfoAttribute FindNestedLinkInfoAttribute(int interfaceIndex, byte[] rxBuffer, LinkInfoAttributeType type)
        {
            int offset = 0;
            int numBytes = rxBuffer.Length;
            while (numBytes > 0)
            {
                byte[] headerData = rxBuffer.Skip(offset).Take(NetlinkMessageMacros.NLMSG_HDRLEN).ToArray();
                offset += NetlinkMessageMacros.NLMSG_HDRLEN;
                NetlinkMessageHeader msgHeader = NetlinkMessageHeader.FromBytes(headerData);  

                if (NetlinkMessageMacros.NLMSG_OK(msgHeader, numBytes) == false)
                    break;

                numBytes -= (int)msgHeader.MessageLength;

                if (msgHeader.MessageType != NetlinkMessageType.RTM_NEWLINK)
                {
                    offset += NetlinkMessageMacros.NLMSG_PAYLOAD(msgHeader, 0);
                    continue;
                }

                byte[] ifiData = rxBuffer.Skip(offset).Take(NetlinkMessageMacros.NLMSG_PAYLOAD(msgHeader, 0)).ToArray();
                offset += NetlinkMessageMacros.NLMSG_ALIGN(Marshal.SizeOf<InterfaceInfoMessage>());
                InterfaceInfoMessage interfaceInfo = InterfaceInfoMessage.FromBytes(ifiData);

                if (interfaceInfo.InterfaceIndex != interfaceIndex)
                {
                    offset += NetlinkMessageMacros.NLMSG_PAYLOAD(msgHeader, Marshal.SizeOf<InterfaceInfoMessage>());
                    continue;
                }

                List<InterfaceLinkAttribute> iflaList = NetlinkUtils.ParseInterfaceLinkAttributes(rxBuffer, ref offset);
                InterfaceLinkAttribute linkInfo = iflaList.FirstOrDefault(ifla => ifla.Type == InterfaceLinkAttributeType.IFLA_LINKINFO);
                if (linkInfo != null)
                {
                    List<LinkInfoAttribute> liaList = NetlinkUtils.ParseNestedLinkInfoAttributes(linkInfo.Data);
                    return liaList.FirstOrDefault(lia => lia.Type == type);
                }
            }

            return null;
        }

        /// <summary>
        /// Retrieves the requested Interface Link Attribute.
        /// </summary>
        /// <param name="interfaceIndex">Interface Index</param>
        /// <param name="rxBuffer">Receive Buffer</param>
        /// <param name="type">Requested Interface Link Attribute</param>
        /// <returns>Requested Interface Link Attribute, if found; otherwise, null.</returns>
        public static InterfaceLinkAttribute FindInterfaceLinkAttribute(int interfaceIndex, byte[] rxBuffer, InterfaceLinkAttributeType type)
        {
            int offset = 0;
            int numBytes = rxBuffer.Length;
            while (numBytes > 0)
            {
                byte[] headerData = rxBuffer.Skip(offset).Take(NetlinkMessageMacros.NLMSG_HDRLEN).ToArray();
                offset += NetlinkMessageMacros.NLMSG_HDRLEN;
                NetlinkMessageHeader msgHeader = NetlinkMessageHeader.FromBytes(headerData);  

                if (NetlinkMessageMacros.NLMSG_OK(msgHeader, numBytes) == false)
                    break;

                numBytes -= (int)msgHeader.MessageLength;

                if (msgHeader.MessageType != NetlinkMessageType.RTM_NEWLINK)
                {
                    offset += NetlinkMessageMacros.NLMSG_PAYLOAD(msgHeader, 0);
                    continue;
                }

                byte[] ifiData = rxBuffer.Skip(offset).Take(NetlinkMessageMacros.NLMSG_PAYLOAD(msgHeader, 0)).ToArray();
                offset += NetlinkMessageMacros.NLMSG_ALIGN(Marshal.SizeOf<InterfaceInfoMessage>());
                InterfaceInfoMessage interfaceInfo = InterfaceInfoMessage.FromBytes(ifiData);

                if (interfaceInfo.InterfaceIndex != interfaceIndex)
                {
                    offset += NetlinkMessageMacros.NLMSG_PAYLOAD(msgHeader, Marshal.SizeOf<InterfaceInfoMessage>());
                    continue;
                }

                List<InterfaceLinkAttribute> iflaList = NetlinkUtils.ParseInterfaceLinkAttributes(rxBuffer, ref offset);
                return iflaList.FirstOrDefault(ifla => ifla.Type == type);
            }

            return null;
        }

        /// <summary>
        /// Generates a Network Interface Information Request Message for the Interface specified by the Index number.
        /// </summary>
        /// <param name="index">Interface Index</param>
        /// <returns>Network Interface Request Message for the Interface specified by the Index number.</returns>
        public static NetworkInterfaceInfoRequest GenerateRequestForLinkInfoByIndex(int index)
        {
            return new NetworkInterfaceInfoRequest()
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
                    InterfaceIndex = index,
                }
            };
        }

        /// <summary>
        /// Generates a Network Interface Modifier Request Message for the Interface specified by the Index number.
        /// </summary>
        /// <param name="index">Interface Index</param>
        /// <param name="canDevProperties">CAN Device Properties to set, can be set to null if none need to be set in the request.</param>
        /// <param name="setInterfaceUp">Set to true to put device interface state to up, false to put device interface state to down, and null to do nothing.</param>
        /// <returns>Network Interface Request Message for the Interface specified by the Index number.</returns>
        public static NetworkInterfaceModifierRequest GenerateRequestForLinkModifierByIndex(int index, CanDeviceProperties canDevProperties, bool? setInterfaceUp)
        {
            var hdr = new NetlinkMessageHeader()
            {
                MessageLength = (uint)NetlinkMessageMacros.NLMSG_LENGTH(Marshal.SizeOf<InterfaceInfoMessage>()),
                MessageType = NetlinkMessageType.RTM_NEWLINK,
                Flags = NetlinkMessageFlags.NLM_F_REQUEST | NetlinkMessageFlags.NLM_F_ACK,
                SenderPortId = 0,
                SequenceNumber = 0,
            };

            var info = new InterfaceInfoMessage()
            {
                AddressFamily = 0,
                InterfaceIndex = index
            };

            if (setInterfaceUp.HasValue)
            {
                if (setInterfaceUp.Value)
                {
                    info.ChangeMask |= (uint)NetDeviceFlags.IFF_UP;
                    info.DeviceFlags |= NetDeviceFlags.IFF_UP;
                }
                else
                {
                    info.ChangeMask |= (uint)NetDeviceFlags.IFF_UP;
                    info.DeviceFlags &= ~NetDeviceFlags.IFF_UP;
                }
            }

            var request = new NetworkInterfaceModifierRequest();
            if (canDevProperties != null)
            {
                List<RoutingAttributeWithData> linkAttributes = GenerateRoutingLinkAttributes(canDevProperties);
                byte[] linkAttrBytes = GeneratePayloadFromNestedAttributes(linkAttributes);
                List<RoutingAttributeWithData> infoAttributes = GenerateRoutingInfoAttributes(canDevProperties);
                byte[] infoAttrBytes = GeneratePayloadFromNestedAttributes(infoAttributes);
                List<RoutingAttributeWithData> dataAttributes = GenerateRoutingDataAttributes(canDevProperties);
                byte[] dataAttrBytes = GeneratePayloadFromNestedAttributes(dataAttributes);

                ushort dLength = (ushort)dataAttrBytes.Length;
                var infoDataAttr = new RoutingAttributeWithData(new RoutingAttribute(dLength, (ushort)LinkInfoAttributeType.IFLA_INFO_DATA));
                ushort liLength = (ushort)(infoAttrBytes.Length + dataAttrBytes.Length + Marshal.SizeOf(infoDataAttr.Attribute));
                var linkInfoAttr = new RoutingAttributeWithData(new RoutingAttribute(liLength, (ushort)InterfaceLinkAttributeType.IFLA_LINKINFO));

                var payload = new List<byte>();
                payload.AddRange(linkAttrBytes);
                if (dataAttributes.Count > 0) // don't include IFLA_LINKINFO and IFLA_INFO_DATA nested sections if there are no data attributes specified
                {
                    payload.AddRange(linkInfoAttr.ToBytes());
                    payload.AddRange(infoAttrBytes);
                    payload.AddRange(infoDataAttr.ToBytes());
                    payload.AddRange(dataAttrBytes);
                }
                byte[] payloadArray = payload.ToArray();
                hdr.MessageLength = (uint)NetlinkMessageMacros.NLMSG_ALIGN((int)hdr.MessageLength + payloadArray.Length);
                Buffer.BlockCopy(payloadArray, 0, request.Payload, 0, payloadArray.Length);
            }

            request.Header = hdr;
            request.Information = info;
            return request;
        }

        private static byte[] GeneratePayloadFromNestedAttributes(IEnumerable<RoutingAttributeWithData> attrs)
        {
            var payload = new List<byte>();
            foreach (var attr in attrs)
            {
                payload.AddRange(attr.ToBytes());
                int padCount = NetlinkMessageMacros.RTA_ALIGN(attr.Attribute.Length) - attr.Attribute.Length;
                if (padCount > 0)
                {
                    payload.AddRange(Enumerable.Repeat<byte>(0x00, padCount));
                }
            }
            return payload.ToArray();
        }
        
        private static List<RoutingAttributeWithData> GenerateRoutingLinkAttributes(CanDeviceProperties canDevProperties)
        {
            var linkAttributes = new List<RoutingAttributeWithData>();

            if (canDevProperties.MaximumTransmissionUnit.HasValue)
            {
                linkAttributes.Add(new RoutingAttributeWithData((ushort)InterfaceLinkAttributeType.IFLA_MTU, canDevProperties.MaximumTransmissionUnit.Value));
            }

            return linkAttributes;
        }

        private static List<RoutingAttributeWithData> GenerateRoutingInfoAttributes(CanDeviceProperties canDevProperties)
        {
            var infoAttributes = new List<RoutingAttributeWithData>()
            {
                new RoutingAttributeWithData((ushort)LinkInfoAttributeType.IFLA_INFO_KIND, canDevProperties.LinkKind)
            };        
            return infoAttributes;
        }

        private static List<RoutingAttributeWithData> GenerateRoutingDataAttributes(CanDeviceProperties canDevProperties)
        {
            var dataAttributes = new List<RoutingAttributeWithData>();

            if (canDevProperties.RestartDelay.HasValue)
            {
                dataAttributes.Add(new RoutingAttributeWithData((ushort)CanRoutingAttributeType.IFLA_CAN_RESTART_MS, canDevProperties.RestartDelay.Value));
            }

            if (canDevProperties.TriggerRestart)
            {
                dataAttributes.Add(new RoutingAttributeWithData((ushort)CanRoutingAttributeType.IFLA_CAN_RESTART, (uint)1));
            }

            if (canDevProperties.BitTiming != null)
            {
                dataAttributes.Add(new RoutingAttributeWithData((ushort)CanRoutingAttributeType.IFLA_CAN_BITTIMING, canDevProperties.BitTiming));
            }

            if (canDevProperties.DataPhaseBitTiming != null)
            {
                dataAttributes.Add(new RoutingAttributeWithData((ushort)CanRoutingAttributeType.IFLA_CAN_DATA_BITTIMING, canDevProperties.DataPhaseBitTiming));
            }

            if (canDevProperties.ControllerMode != null)
            {
                dataAttributes.Add(new RoutingAttributeWithData((ushort)CanRoutingAttributeType.IFLA_CAN_CTRLMODE, canDevProperties.ControllerMode));
            }
            
            if (canDevProperties.TerminationResistance.HasValue)
            {
                dataAttributes.Add(new RoutingAttributeWithData((ushort)CanRoutingAttributeType.IFLA_CAN_TERMINATION, canDevProperties.TerminationResistance.Value));
            }

            return dataAttributes;
        }

        /// <summary>
        /// Peeks at the header using the provided buffer.
        /// </summary>
        /// <param name="buffer">Byte array which contains a Netlink message.</param>
        /// <returns>Netlink Message Header object extracted from the buffer.</returns>
        /// <exception cref="ArgumentNullException">Provided buffer is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Provided buffer is too small.</exception>
        public static NetlinkMessageHeader PeekAtHeader(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            if (buffer.Length < Marshal.SizeOf<NetlinkMessageHeader>())
                throw new ArgumentOutOfRangeException(nameof(buffer), "Buffer size must be at least 16 bytes to peek at header.");

            return NetlinkMessageHeader.FromBytes(buffer);
        }
    }
}