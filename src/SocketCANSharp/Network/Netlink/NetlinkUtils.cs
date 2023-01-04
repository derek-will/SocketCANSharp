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
                Marshal.Copy(data, 0, ptr, size);
                return Marshal.PtrToStructure<T>(ptr);
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
    }
}