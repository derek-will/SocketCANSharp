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
using System.Net.Sockets;
using System.Collections.Generic;

namespace SocketCANSharp.Network.Netlink.Gateway
{
    /// <summary>
    /// Provides CAN Gateway (CGW) Netlink socket services.
    /// </summary>
    public class CanGatewaySocket : AbstractNetlinkSocket
    {
        /// <summary>
        /// Initializes a new instance of the CanGatewaySocket class.
        /// </summary>
        /// <exception cref="SocketCanException">Unable to create the requested socket.</exception>
        public CanGatewaySocket()
        {
            SocketType = SocketType.Raw;
            ProtocolType = NetlinkProtocolType.NETLINK_ROUTE;
            SafeHandle = NetlinkNativeMethods.Socket(NetlinkConstants.PF_NETLINK, SocketType, ProtocolType);

            if (SafeHandle.IsInvalid)
                throw new SocketCanException("Failed to create NETLINK_ROUTE socket.");
        }

        /// <summary>
        /// Adds or Updates a CAN-to-CAN Gateway Rule.
        /// </summary>
        /// <param name="rule">CAN-to-CAN Gateway Rule.</param>
        /// <returns>Number of bytes written to the socket.</returns>
        /// <exception cref="ObjectDisposedException">The socket has been closed.</exception>
        /// <exception cref="SocketCanException">Writing to the underlying NETLINK_ROUTE socket failed.</exception>
        public int AddOrUpdateCanToCanRule(CgwCanToCanRule rule)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            var request = new CanGatewayRequest(CanGatewayOperation.AddOrUpdate, rule);
            int bytesWritten = CanGatewayNativeMethods.Write(SafeHandle, request, (int)request.Header.MessageLength);
            if (bytesWritten == -1)
                throw new SocketCanException("Writing to the underlying NETLINK_ROUTE socket failed.");

            return bytesWritten;
        }

        /// <summary>
        /// Remove all CAN-to-CAN Gateway Rules.
        /// </summary>
        /// <returns>Number of bytes written to the socket.</returns>
        /// <exception cref="ObjectDisposedException">The socket has been closed.</exception>
        /// <exception cref="SocketCanException">Writing to the underlying NETLINK_ROUTE socket failed.</exception>
        public int RemoveAllCanToCanRules()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            // rule frame type doesn't matter here as the call is to delete all rules so flags are not inspected.
            // set both src and dst indexes to 0 to trigger delete all logic.
            var rule = new CgwCanToCanRule(CgwCanFrameType.ClassicalCAN)
            {
                SourceIndex = 0,
                DestinationIndex = 0,
            };

            var request = new CanGatewayRequest(CanGatewayOperation.RemoveAll, rule);
            int bytesWritten = CanGatewayNativeMethods.Write(SafeHandle, request, (int)request.Header.MessageLength);
            if (bytesWritten == -1)
                throw new SocketCanException("Writing to the underlying NETLINK_ROUTE socket failed.");

            return bytesWritten;
        }

        /// <summary>
        /// Remove a CAN-to-CAN Gateway Rule.
        /// </summary>
        /// <returns>Number of bytes written to the socket.</returns>
        /// <exception cref="ObjectDisposedException">The socket has been closed.</exception>
        /// <exception cref="SocketCanException">Writing to the underlying NETLINK_ROUTE socket failed.</exception>
        public int RemoveCanToCanRule(CgwCanToCanRule rule)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            var request = new CanGatewayRequest(CanGatewayOperation.Remove, rule);
            int bytesWritten = CanGatewayNativeMethods.Write(SafeHandle, request, (int)request.Header.MessageLength);
            if (bytesWritten == -1)
                throw new SocketCanException("Writing to the underlying NETLINK_ROUTE socket failed.");

            return bytesWritten;
        }
        
        /// <summary>
        /// Requests a list of all CGW Routing Rules.
        /// </summary>
        /// <returns>Number of bytes written to the socket.</returns>
        /// <exception cref="ObjectDisposedException">The socket has been closed.</exception>
        /// <exception cref="SocketCanException">Writing to the underlying NETLINK_ROUTE socket failed.</exception>
        public int RequestListOfAllRules()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            // rule frame type doesn't matter here as the call is to list all rules so flags are not inspected.
            var rule = new CgwCanToCanRule(CgwCanFrameType.ClassicalCAN);

            var request = new CanGatewayRequest(CanGatewayOperation.List, rule);
            int bytesWritten = CanGatewayNativeMethods.Write(SafeHandle, request, (int)request.Header.MessageLength);
            if (bytesWritten == -1)
                throw new SocketCanException("Writing to the underlying NETLINK_ROUTE socket failed.");

            return bytesWritten;
        }

        /// <summary>
        /// Reads data from the socket into the supplied receive buffer.
        /// </summary>
        /// <param name="data">An array of bytes that is the receive buffer</param>
        /// <returns>The number of bytes received from the socket.</returns>
        /// <exception cref="ObjectDisposedException">The socket has been closed.</exception>
        /// <exception cref="ArgumentNullException">The data byte array is null.</exception>
        /// <exception cref="SocketCanException">Reading from the underlying NETLINK_ROUTE socket failed.</exception>
        public int Read(byte[] data)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            int bytesRead = LibcNativeMethods.Read(SafeHandle, data, data.Length);
            if (bytesRead == -1)
            {
                throw new SocketCanException("Reading from the underlying NETLINK_ROUTE socket failed.");
            }

            return bytesRead;
        }

        /// <summary>
        /// Parse CAN-to-CAN Routing Rules from supplied buffer.
        /// </summary>
        /// <param name="buffer">Buffer containing CGW CAN-to-CAN netlink routing rules.</param>
        /// <param name="keepReading">True, to indicate to the caller that Read function needs to be called again. Otherwise, false.</param>
        /// <returns>List of parsed CAN-to-CAN Routing Rules.</returns>
        /// <exception cref="ArgumentNullException">Supplied buffer is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Supplied buffer contains insufficient or invalid data.</exception>
        public List<CgwCanToCanRule> ParseCanToCanRules(byte[] buffer, out bool keepReading)
        {
            keepReading = false;
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            var ruleList = new List<CgwCanToCanRule>();
            int offset = 0;
            int numBytes = buffer.Length;
            while (numBytes > 0)
            {
                if (numBytes < NetlinkMessageMacros.NLMSG_HDRLEN)
                    throw new ArgumentOutOfRangeException(nameof(buffer), "Supplied buffer was expected to contain enough data to include the header, but did not.");

                NetlinkMessageHeader header = NetlinkUtils.PeekAtHeader(buffer);
                offset += NetlinkMessageMacros.NLMSG_HDRLEN;
                if (header.Flags.HasFlag(NetlinkMessageFlags.NLM_F_MULTI) == false)
                    throw new ArgumentOutOfRangeException(nameof(buffer), "Data was expected to contain a header with the NLM_F_MULTI set, but did not.");

                if (header.MessageType != NetlinkMessageType.RTM_NEWROUTE && header.MessageType != NetlinkMessageType.NLMSG_DONE)
                    throw new ArgumentOutOfRangeException(nameof(buffer), "Data was epxected to contain a header of either message type RTM_NEWROUTE or NLMSG_DONE, but did not.");

                if (header.SequenceNumber != 0)
                    throw new ArgumentOutOfRangeException(nameof(buffer), "Data was expected to contain a header with Sequence Number of 0, but did not.");

                if (NetlinkMessageMacros.NLMSG_OK(header, numBytes) == false)
                    throw new ArgumentOutOfRangeException(nameof(buffer), "Supplied netlink buffer was not parseable.");

                numBytes -= (int)header.MessageLength;

                if (header.MessageType == NetlinkMessageType.NLMSG_DONE)
                {
                    keepReading = false;
                }
                else
                {
                    keepReading = true;
                    byte[] rtCanData = new byte[NetlinkMessageMacros.NLMSG_PAYLOAD(header, 0)];
                    if (buffer.Length - offset < rtCanData.Length)
                        throw new ArgumentOutOfRangeException(nameof(buffer), "Buffer does not contain enough data to contain the rtcanmsg data.");

                    Buffer.BlockCopy(buffer, offset, rtCanData, 0, rtCanData.Length);
                    offset += NetlinkMessageMacros.NLMSG_ALIGN(RoutingCanMessage.UnmanagedSize());
                    RoutingCanMessage rtCanMsg = RoutingCanMessage.FromBytes(rtCanData);
                    if (rtCanMsg.CanFamily != SocketCanConstants.AF_CAN)
                        throw new ArgumentOutOfRangeException(nameof(buffer), "Message was expected to contain Address Family of AF_CAN, but did not.");

                    if (rtCanMsg.GatewayType != CanGatewayType.CGW_TYPE_CAN_CAN)
                        throw new ArgumentOutOfRangeException(nameof(buffer), "Message was expected to contain Gateway Type of CGW_TYPE_CAN_CAN, but did not.");

                    byte[] parseData = new byte[(int)header.MessageLength - offset];
                    if (buffer.Length - offset < parseData.Length)
                        throw new ArgumentOutOfRangeException(nameof(buffer), "Buffer does not contain enough data to contain the rtnetlink attribute data.");

                    Buffer.BlockCopy(buffer, offset, parseData, 0, parseData.Length);
                    offset += parseData.Length;
                    List<CanGatewayRoutingAttribute> rtAttrList = CanGatewayRequest.ParseAttributes(parseData);
                    var rule = new CgwCanToCanRule(rtCanMsg.GatewayFlags.HasFlag(CanGatewayFlag.CGW_FLAGS_CAN_FD) ? CgwCanFrameType.CANFD : CgwCanFrameType.ClassicalCAN);
                    rule.AllowRoutingToSameInterface = rtCanMsg.GatewayFlags.HasFlag(CanGatewayFlag.CGW_FLAGS_CAN_IIF_TX_OK);
                    rule.EnableLocalCanSocketLoopback = rtCanMsg.GatewayFlags.HasFlag(CanGatewayFlag.CGW_FLAGS_CAN_ECHO);
                    rule.MaintainSourceTimestamp = rtCanMsg.GatewayFlags.HasFlag(CanGatewayFlag.CGW_FLAGS_CAN_SRC_TSTAMP);
                    foreach (var attr in rtAttrList)
                    {
                        switch (attr.Type)
                        {
                            case CanGatewayAttributeType.CGW_SRC_IF:
                                rule.SourceIndex = NetlinkUtils.FromBytes<uint>(attr.Data);
                                break;
                            case CanGatewayAttributeType.CGW_DST_IF:
                                rule.DestinationIndex = NetlinkUtils.FromBytes<uint>(attr.Data);
                                break;
                            case CanGatewayAttributeType.CGW_MOD_SET:
                                var setModifier = NetlinkUtils.FromBytes<CgwCanFrameModification>(attr.Data);
                                rule.SetModifier = new ClassicalCanGatewayModifier(setModifier.ModificationType, setModifier.CanFrame);
                                break;
                            case CanGatewayAttributeType.CGW_MOD_AND:
                                var andModifier = NetlinkUtils.FromBytes<CgwCanFrameModification>(attr.Data);
                                rule.AndModifier = new ClassicalCanGatewayModifier(andModifier.ModificationType, andModifier.CanFrame);
                                break;
                            case CanGatewayAttributeType.CGW_MOD_OR:
                                var orModifier = NetlinkUtils.FromBytes<CgwCanFrameModification>(attr.Data);
                                rule.OrModifier = new ClassicalCanGatewayModifier(orModifier.ModificationType, orModifier.CanFrame);
                                break;
                            case CanGatewayAttributeType.CGW_MOD_XOR:
                                var xorModifier = NetlinkUtils.FromBytes<CgwCanFrameModification>(attr.Data);
                                rule.XorModifier = new ClassicalCanGatewayModifier(xorModifier.ModificationType, xorModifier.CanFrame);
                                break;
                            case CanGatewayAttributeType.CGW_FDMOD_SET:
                                var setFdModifier = NetlinkUtils.FromBytes<CgwCanFdFrameModification>(attr.Data);
                                rule.SetModifier = new CanFdGatewayModifier(setFdModifier.ModificationType, setFdModifier.CanFdFrame);
                                break;
                            case CanGatewayAttributeType.CGW_FDMOD_AND:
                                var andFdModifier = NetlinkUtils.FromBytes<CgwCanFdFrameModification>(attr.Data);
                                rule.AndModifier = new CanFdGatewayModifier(andFdModifier.ModificationType, andFdModifier.CanFdFrame);
                                break;
                            case CanGatewayAttributeType.CGW_FDMOD_OR:
                                var orFdModifier = NetlinkUtils.FromBytes<CgwCanFdFrameModification>(attr.Data);
                                rule.OrModifier = new CanFdGatewayModifier(orFdModifier.ModificationType, orFdModifier.CanFdFrame);
                                break;
                            case CanGatewayAttributeType.CGW_FDMOD_XOR:
                                var xorFdModifier = NetlinkUtils.FromBytes<CgwCanFdFrameModification>(attr.Data);
                                rule.XorModifier = new CanFdGatewayModifier(xorFdModifier.ModificationType, xorFdModifier.CanFdFrame);
                                break;
                            case CanGatewayAttributeType.CGW_CS_XOR:
                                rule.ChecksumXorConfiguration = NetlinkUtils.FromBytes<CgwChecksumXor>(attr.Data);
                                break;
                            case CanGatewayAttributeType.CGW_CS_CRC8:
                                rule.Crc8Configuration = NetlinkUtils.FromBytes<CgwCrc8>(attr.Data);
                                break;
                            case CanGatewayAttributeType.CGW_FILTER:
                                rule.ReceiveFilter = NetlinkUtils.FromBytes<CanFilter>(attr.Data);
                                break;
                            case CanGatewayAttributeType.CGW_LIM_HOPS:
                                rule.HopLimit = NetlinkUtils.FromBytes<byte>(attr.Data);
                                break;
                            case CanGatewayAttributeType.CGW_MOD_UID:
                                rule.UpdateIdentifier = NetlinkUtils.FromBytes<uint>(attr.Data);
                                break;
                            case CanGatewayAttributeType.CGW_HANDLED:
                                rule.HandledFrames = NetlinkUtils.FromBytes<uint>(attr.Data);
                                break;
                            case CanGatewayAttributeType.CGW_DROPPED:
                                rule.DroppedFrames = NetlinkUtils.FromBytes<uint>(attr.Data);
                                break;
                            case CanGatewayAttributeType.CGW_DELETED:
                                rule.DeletedFrames = NetlinkUtils.FromBytes<uint>(attr.Data);
                                break;
                            default:
                                break;
                        }
                    }
                    ruleList.Add(rule);
                }
            }

            return ruleList;
        }

    }
}