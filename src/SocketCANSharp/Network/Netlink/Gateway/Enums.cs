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

namespace SocketCANSharp.Network.Netlink.Gateway
{
    /// <summary>
    /// CAN Gateway Type
    /// </summary>
    public enum CanGatewayType : byte
    {
        /// <summary>
        /// Unspecified Gateway Type
        /// </summary>
        CGW_TYPE_UNSPEC,
        /// <summary>
        /// CAN-to-CAN Gateway
        /// </summary>
        CGW_TYPE_CAN_CAN,
    }

    /// <summary>
    /// CAN Gateway Attribute Type.
    /// </summary>
    public enum CanGatewayAttributeType : ushort
    {
        /// <summary>
        /// Unspecified.
        /// </summary>
        CGW_UNSPEC,
        /// <summary>
        /// CAN frame modification binary AND.
        /// </summary>
        CGW_MOD_AND,
        /// <summary>
        /// CAN frame modification binary OR.
        /// </summary>
        CGW_MOD_OR,
        /// <summary>
        /// CAN frame modification binary XOR.
        /// </summary>
        CGW_MOD_XOR,
        /// <summary>
        /// CAN frame modification set alternate values.
        /// </summary>
        CGW_MOD_SET,
        /// <summary>
        /// set data[] XOR checksum into data[index].
        /// </summary>
        CGW_CS_XOR,
        /// <summary>
        /// set data[] CRC8 checksum into data[index].
        /// </summary>
        CGW_CS_CRC8,
        /// <summary>
        /// Number of handled CAN frames.
        /// </summary>
        CGW_HANDLED,
        /// <summary>
        /// Number of dropped CAN frames.
        /// </summary>
        CGW_DROPPED,
        /// <summary>
        /// Obtain interface index of source network interface.
        /// </summary>
        CGW_SRC_IF,
        /// <summary>
        /// Obtain interface index of destination network interface.
        /// </summary>
        CGW_DST_IF,
        /// <summary>
        /// Specify CAN Filter struct on source CAN device.
        /// </summary>
        CGW_FILTER,
        /// <summary>
        /// Number of deleted CAN frames.
        /// </summary>
        CGW_DELETED,
        /// <summary>
        /// Limit the number of hops of this specific rule.
        /// </summary>
        CGW_LIM_HOPS,
        /// <summary>
        /// User defined identifier for modification updates.
        /// </summary>
        CGW_MOD_UID,
        /// <summary>
        /// CAN FD frame modification binary AND.
        /// </summary>
        CGW_FDMOD_AND,
        /// <summary>
        /// CAN FD frame modification binary OR.
        /// </summary>
        CGW_FDMOD_OR,
        /// <summary>
        /// CAN FD frame modification binary XOR.
        /// </summary>
        CGW_FDMOD_XOR,
        /// <summary>
        /// CAN FD frame modification set alternate values.
        /// </summary>
        CGW_FDMOD_SET,
    }

    /// <summary>
    /// CAN Gateway Flags.
    /// </summary>
    [Flags]
    public enum CanGatewayFlag : ushort
    {
        /// <summary>
        /// Enables loopback for listeners on the local CAN sockets.
        /// </summary>
        CGW_FLAGS_CAN_ECHO          = 0x0001,
        /// <summary>
        /// Keep the source timestamp associated with the data instead of clearing it on receive.
        /// </summary>
        CGW_FLAGS_CAN_SRC_TSTAMP    = 0x0002,
        /// <summary>
        /// Enables routing the received data back to the originating interface.
        /// </summary>
        CGW_FLAGS_CAN_IIF_TX_OK     = 0x0004,
        /// <summary>
        /// Process CAN FD Frames.
        /// </summary>
        CGW_FLAGS_CAN_FD            = 0x0008,
    }

    /// <summary>
    /// CAN Gateway Modification Types.
    /// </summary>
    [Flags]
    public enum CanGatewayModificationType : byte
    {
        /// <summary>
        /// Modify Identifier.
        /// </summary>
        CGW_MOD_ID      = 0x01,
        /// <summary>
        /// Modify Length.
        /// </summary>
        CGW_MOD_LEN     = 0x02,
        /// <summary>
        /// Modify Data.
        /// </summary>
        CGW_MOD_DATA    = 0x04,
        /// <summary>
        /// Modify CAN FD Flags.
        /// </summary>
        CGW_MOD_FLAGS   = 0x08,
    }

    /// <summary>
    /// CRC8 Profiles.
    /// </summary>
    public enum Crc8Profile : byte
    {
        /// <summary>
        /// Unspecified Profile.
        /// </summary>
        CGW_CRC8PRF_UNSPEC,
        /// <summary>
        /// Compute one addition byte value.
        /// </summary>
        CGW_CRC8PRF_1U8,
        /// <summary>
        /// Byte value table indexed by data[1] AND 0x0F.
        /// </summary>
        CGW_CRC8PRF_16U8,
        /// <summary>
        /// (CAN_ID AND 0xFF) XOR (CAN_ID right shift 8 bits AND 0xFF)
        /// </summary>
        CGW_CRC8PRF_SFFID_XOR,
    };

}