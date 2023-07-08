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

namespace SocketCANSharp.Network.Netlink.Gateway
{
    /// <summary>
    /// CAN Gateway Get Request.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class CanGatewayRequest
    {
        private const int DATA_SIZE = 1536;

        /// <summary>
        /// Message Header.
        /// </summary>
        public NetlinkMessageHeader Header { get; set; }
        /// <summary>
        /// Routing CAN Message.
        /// </summary>
        public RoutingCanMessage Message { get; set; }
        /// <summary>
        /// Request Data.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=DATA_SIZE)]
        public byte[] Data;

        /// <summary>
        /// Initializes a new instance of the CanGatewayRequest class.
        /// </summary>
        public CanGatewayRequest()
        {
            Data = new byte[DATA_SIZE];
        }

        /// <summary>
        /// Initializes a new instance of the CanGatewayRequest class using the supplied CGW Operation, Type, and Flags.
        /// </summary>
        /// <param name="cgwOperation">Whether to Add/Update existing rule, List all rules, Remove a rule or Remove All rules.</param>
        /// <param name="cgwType">CAN Gateway Type (i.e. CAN-to-CAN)</param>
        /// <param name="cgwFlags">Operation Flags for CAN FD, Echoing sent frames, etc.</param>
        public CanGatewayRequest(CanGatewayOperation cgwOperation, CanGatewayType cgwType, CanGatewayFlag cgwFlags) : this()
        {
            Header = new NetlinkMessageHeader()
            {
                MessageLength = (uint)NetlinkMessageMacros.NLMSG_LENGTH(Marshal.SizeOf(typeof(RoutingCanMessage))),
                MessageType = GetNlMsgTypeForGwOperation(cgwOperation),
                Flags = GetNlMsgFlagsForGwOperation(cgwOperation),
                SenderPortId = 0,
                SequenceNumber = 0,
            };
            Message = new RoutingCanMessage(cgwType, cgwFlags);
        }
        
        /// <summary>
        /// Initializes a new instance of the CanGatewayRequest class using the supplied CGW Operation and CAN-to-CAN Routing Rule.
        /// </summary>
        /// <param name="cgwOperation">CGW Operation</param>
        /// <param name="canToCanRule">CAN-to-CAN Routing Rule</param>
        /// <exception cref="ArgumentNullException">CAN-to-CAN Routing Rule cannot be null.</exception>
        public CanGatewayRequest(CanGatewayOperation cgwOperation, CgwCanToCanRule canToCanRule) : this(cgwOperation, canToCanRule.GatewayType, GetCgwFlags(canToCanRule))
        {
            if (canToCanRule == null)
                throw new ArgumentNullException(nameof(canToCanRule));

            SetRoutingPath(canToCanRule.SourceIndex, canToCanRule.DestinationIndex);

            if (canToCanRule.ReceiveFilter.HasValue)
                SetReceiveFilter(canToCanRule.ReceiveFilter.Value);

            if (canToCanRule.AndModifier != null)
                SetBinaryAndModifier(canToCanRule.AndModifier);
                
            if (canToCanRule.OrModifier != null)
                SetBinaryOrModifier(canToCanRule.OrModifier);

            if (canToCanRule.XorModifier != null)
                SetBinaryXorModifier(canToCanRule.XorModifier);

            if (canToCanRule.SetModifier != null)
                SetBinarySetModifier(canToCanRule.SetModifier);

            if (canToCanRule.ChecksumXorConfiguration.HasValue)
                SetChecksumXorConfiguration(canToCanRule.ChecksumXorConfiguration.Value);

            if (canToCanRule.Crc8Configuration.HasValue)
                SetCrc8Configuration(canToCanRule.Crc8Configuration.Value);

            if (canToCanRule.HopLimit != 0)
                SetRuleHopLimit(canToCanRule.HopLimit);

            if (canToCanRule.UpdateIdentifier != 0)
                SetUpdateIdentifier(canToCanRule.UpdateIdentifier);
        }

        /// <summary>
        /// Adds a CAN Gateway Routing Attribute to the request payload.
        /// </summary>
        /// <param name="attr">Attribute to append to the request</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if adding the attribute would exceed the bounds of the payload array.</exception>
        public void AddCanGatewayAttribute(CanGatewayRoutingAttribute attr)
        {
            int maxLength = Marshal.SizeOf<CanGatewayRequest>();
            if (NetlinkMessageMacros.RTA_ALIGN(attr.Length) + NetlinkMessageMacros.NLMSG_ALIGN((int)Header.MessageLength) > maxLength)
                throw new ArgumentOutOfRangeException("Adding CGW attribute would exceed the allowed bounds.");

            int offset = (int)Header.MessageLength - NetlinkMessageMacros.NLMSG_LENGTH(Marshal.SizeOf(typeof(RoutingCanMessage)));
            byte[] serializedLength = NetlinkUtils.ToBytes<ushort>(attr.Length);
            byte[] serializedType = NetlinkUtils.ToBytes<ushort>((ushort)attr.Type);
            Buffer.BlockCopy(serializedLength, 0, Data, offset, serializedLength.Length);
            Buffer.BlockCopy(serializedType, 0, Data, offset + serializedLength.Length, serializedType.Length);
            Buffer.BlockCopy(attr.Data, 0, Data, offset + serializedLength.Length + serializedType.Length, attr.Data.Length);

            Header = new NetlinkMessageHeader()
            {
                MessageLength = (uint)(NetlinkMessageMacros.NLMSG_ALIGN((int)Header.MessageLength) + NetlinkMessageMacros.RTA_ALIGN(attr.Length)),
                MessageType = Header.MessageType,
                Flags = Header.Flags,
                SequenceNumber = Header.SequenceNumber,
                SenderPortId = Header.SenderPortId,
            };
        }
        
        /// <summary>
        /// Specify Routing Path.
        /// </summary>
        /// <param name="srcIndex">Source Network Interface Index</param>
        /// <param name="dstIndex">Destination Network InterfaceIndex</param>
        public void SetRoutingPath(uint srcIndex, uint dstIndex)
        {
            AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_SRC_IF, NetlinkUtils.ToBytes<uint>(srcIndex)));
            AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_DST_IF, NetlinkUtils.ToBytes<uint>(dstIndex)));
        }

        /// <summary>
        /// Specify Update Identifier.
        /// </summary>
        /// <param name="uid">UID used when making updates to existing rules.</param>
        public void SetUpdateIdentifier(uint uid)
        {
            AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_MOD_UID, NetlinkUtils.ToBytes<uint>(uid)));
        }

        /// <summary>
        /// Specify Hop Limit for routing rule.
        /// </summary>
        /// <param name="hopLimit">Hop Limit for rule.</param>
        public void SetRuleHopLimit(byte hopLimit)
        {
            AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_LIM_HOPS, NetlinkUtils.ToBytes<byte>(hopLimit)));
        }

        /// <summary>
        /// Specify CAN Receive Filter on source interface.
        /// </summary>
        /// <param name="canFilter">CAN Receive Filter on source interface.</param>
        public void SetReceiveFilter(CanFilter canFilter)
        {
            AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_FILTER, NetlinkUtils.ToBytes<CanFilter>(canFilter)));
        }

        /// <summary>
        /// Specify Binary AND frame modification.
        /// </summary>
        /// <param name="modifier">Frame modification</param>
        /// <exception cref="ArgumentOutOfRangeException">Unrecognized modifier type.</exception>
        public void SetBinaryAndModifier(AbstractCanGatewayModifier modifier)
        {
            if (modifier is ClassicalCanGatewayModifier)
            {
                var cgwModifier = new CgwCanFrameModification()
                {
                    ModificationType = modifier.ModificationTarget,
                    CanFrame = ((ClassicalCanGatewayModifier)modifier).CanFrame,
                };

                var modAttr = new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_MOD_AND, NetlinkUtils.ToBytes<CgwCanFrameModification>(cgwModifier));
                AddCanGatewayAttribute(modAttr);
            }
            else if (modifier is CanFdGatewayModifier)
            {
                var cgwModifier = new CgwCanFdFrameModification()
                {
                    ModificationType = modifier.ModificationTarget,
                    CanFdFrame = ((CanFdGatewayModifier)modifier).CanFdFrame,
                };

                var modAttr = new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_FDMOD_AND, NetlinkUtils.ToBytes<CgwCanFdFrameModification>(cgwModifier));
                AddCanGatewayAttribute(modAttr);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(modifier), "Unsupported Modifier");
            }
        }

        /// <summary>
        /// Specify Binary OR frame modification.
        /// </summary>
        /// <param name="modifier">Frame modification</param>
        /// <exception cref="ArgumentOutOfRangeException">Unrecognized modifier type.</exception>
        public void SetBinaryOrModifier(AbstractCanGatewayModifier modifier)
        {
            if (modifier is ClassicalCanGatewayModifier)
            {
                var cgwModifier = new CgwCanFrameModification()
                {
                    ModificationType = modifier.ModificationTarget,
                    CanFrame = ((ClassicalCanGatewayModifier)modifier).CanFrame,
                };

                var modAttr = new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_MOD_OR, NetlinkUtils.ToBytes<CgwCanFrameModification>(cgwModifier));
                AddCanGatewayAttribute(modAttr);
            }
            else if (modifier is CanFdGatewayModifier)
            {
                var cgwModifier = new CgwCanFdFrameModification()
                {
                    ModificationType = modifier.ModificationTarget,
                    CanFdFrame = ((CanFdGatewayModifier)modifier).CanFdFrame,
                };

                var modAttr = new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_FDMOD_OR, NetlinkUtils.ToBytes<CgwCanFdFrameModification>(cgwModifier));
                AddCanGatewayAttribute(modAttr);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(modifier), "Unsupported Modifier");
            }
        }

        /// <summary>
        /// Specify Binary XOR frame modification.
        /// </summary>
        /// <param name="modifier">Frame modification</param>
        /// <exception cref="ArgumentOutOfRangeException">Unrecognized modifier type.</exception>
        public void SetBinaryXorModifier(AbstractCanGatewayModifier modifier)
        {
            if (modifier is ClassicalCanGatewayModifier)
            {
                var cgwModifier = new CgwCanFrameModification()
                {
                    ModificationType = modifier.ModificationTarget,
                    CanFrame = ((ClassicalCanGatewayModifier)modifier).CanFrame,
                };

                var modAttr = new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_MOD_XOR, NetlinkUtils.ToBytes<CgwCanFrameModification>(cgwModifier));
                AddCanGatewayAttribute(modAttr);
            }
            else if (modifier is CanFdGatewayModifier)
            {
                var cgwModifier = new CgwCanFdFrameModification()
                {
                    ModificationType = modifier.ModificationTarget,
                    CanFdFrame = ((CanFdGatewayModifier)modifier).CanFdFrame,
                };

                var modAttr = new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_FDMOD_XOR, NetlinkUtils.ToBytes<CgwCanFdFrameModification>(cgwModifier));
                AddCanGatewayAttribute(modAttr);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(modifier), "Unsupported Modifier");
            }
        }

        /// <summary>
        /// Specify Binary SET frame modification.
        /// </summary>
        /// <param name="modifier">Frame modification</param>
        /// <exception cref="ArgumentOutOfRangeException">Unrecognized modifier type.</exception>
        public void SetBinarySetModifier(AbstractCanGatewayModifier modifier)
        {
            if (modifier is ClassicalCanGatewayModifier)
            {
                var cgwModifier = new CgwCanFrameModification()
                {
                    ModificationType = modifier.ModificationTarget,
                    CanFrame = ((ClassicalCanGatewayModifier)modifier).CanFrame,
                };

                var modAttr = new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_MOD_SET, NetlinkUtils.ToBytes<CgwCanFrameModification>(cgwModifier));
                AddCanGatewayAttribute(modAttr);
            }
            else if (modifier is CanFdGatewayModifier)
            {
                var cgwModifier = new CgwCanFdFrameModification()
                {
                    ModificationType = modifier.ModificationTarget,
                    CanFdFrame = ((CanFdGatewayModifier)modifier).CanFdFrame,
                };

                var modAttr = new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_FDMOD_SET, NetlinkUtils.ToBytes<CgwCanFdFrameModification>(cgwModifier));
                AddCanGatewayAttribute(modAttr);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(modifier), "Unsupported Modifier");
            }
        }

        /// <summary>
        /// Specify Checksum XOR configuration.
        /// </summary>
        /// <param name="checksumXor">Checksum XOR configuration</param>
        public void SetChecksumXorConfiguration(CgwChecksumXor checksumXor)
        {
            AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_CS_XOR, NetlinkUtils.ToBytes<CgwChecksumXor>(checksumXor)));
        }
        
        /// <summary>
        /// Specify CRC8 configuration.
        /// </summary>
        /// <param name="crc8">CRC8 configuration</param>
        public void SetCrc8Configuration(CgwCrc8 crc8)
        {
            AddCanGatewayAttribute(new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_CS_CRC8, NetlinkUtils.ToBytes<CgwCrc8>(crc8)));
        }

        /// <summary>
        /// Parses the attributes from the buffer.
        /// </summary>
        /// <param name="rxBuffer">Attribute buffer</param>
        /// <returns>List of attributes parsed from the buffer.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the attribute buffer is null</exception>
        public static List<CanGatewayRoutingAttribute> ParseAttributes(byte[] rxBuffer)
        {
            if (rxBuffer == null)
                throw new ArgumentNullException(nameof(rxBuffer));

            var cgwRtAttrList = new List<CanGatewayRoutingAttribute>();
            int len = rxBuffer.Length;
            int offset = 0;
            do
            {
                byte[] rtaData = rxBuffer.Skip(offset).ToArray();
                if (CanGatewayRoutingAttribute.TryParse(rtaData, out CanGatewayRoutingAttribute rta) == false)
                    break;

                len -= NetlinkMessageMacros.RTA_ALIGN(rta.Length);
                offset += NetlinkMessageMacros.RTA_ALIGN(rta.Length);
                cgwRtAttrList.Add(rta);
            } while (len > 0);

            return cgwRtAttrList;
        }

        private NetlinkMessageType GetNlMsgTypeForGwOperation(CanGatewayOperation gwOperation)
        {
            switch (gwOperation)
            {
                case CanGatewayOperation.AddOrUpdate:
                    return NetlinkMessageType.RTM_NEWROUTE;
                case CanGatewayOperation.List:
                    return NetlinkMessageType.RTM_GETROUTE;
                case CanGatewayOperation.Remove:
                case CanGatewayOperation.RemoveAll:
                    return NetlinkMessageType.RTM_DELROUTE;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gwOperation));
            }
        }

        private NetlinkMessageFlags GetNlMsgFlagsForGwOperation(CanGatewayOperation gwOperation)
        {
            switch (gwOperation)
            {
                case CanGatewayOperation.AddOrUpdate:
                case CanGatewayOperation.RemoveAll:
                case CanGatewayOperation.Remove:
                    return NetlinkMessageFlags.NLM_F_REQUEST | NetlinkMessageFlags.NLM_F_ACK;
                case CanGatewayOperation.List:
                    return NetlinkMessageFlags.NLM_F_REQUEST | NetlinkMessageFlags.NLM_F_DUMP;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gwOperation));
            }
        }

        private static CanGatewayFlag GetCgwFlags(CgwBaseRule rule)
        {
            CanGatewayFlag flag = 0;
            if (rule.EnableLocalCanSocketLoopback)
                flag |= CanGatewayFlag.CGW_FLAGS_CAN_ECHO;

            if (rule.MaintainSourceTimestamp)
                flag |= CanGatewayFlag.CGW_FLAGS_CAN_SRC_TSTAMP;

            if (rule.AllowRoutingToSameInterface)
                flag |= CanGatewayFlag.CGW_FLAGS_CAN_IIF_TX_OK;

            if (rule.IsCanFdRule)
                flag |= CanGatewayFlag.CGW_FLAGS_CAN_FD;
            return flag;
        }
    }
}