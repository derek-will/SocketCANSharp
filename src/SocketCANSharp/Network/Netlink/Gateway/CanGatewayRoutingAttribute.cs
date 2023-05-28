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
using System.Text;

namespace SocketCANSharp.Network.Netlink.Gateway
{
    /// <summary>
    /// CAN Gateway Routing Attribute.
    /// </summary>
    public struct CanGatewayRoutingAttribute
    {
        private const int LengthSize = 2;
        private const int TypeSize = 2;
        private const int TypeLengthSize = LengthSize + TypeSize;

        /// <summary>
        /// CAN Gateway Attribute Length.
        /// </summary>
        public ushort Length { get; set; }
        /// <summary>
        /// CAN Gateway Attribute Type.
        /// </summary>
        public CanGatewayAttributeType Type { get; set; }
        /// <summary>
        /// Attribute Data.
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// Initializes a new instance of the CanRoutingAttribute class.
        /// </summary>
        public CanGatewayRoutingAttribute(CanGatewayAttributeType type, byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "CAN_GW Routing Attribute Data cannot be null");
            
            Length = (ushort)NetlinkMessageMacros.RTA_LENGTH(data.Length);
            Type = type;
            Data = data;
        }

        /// <summary>
        /// Parses a CAN Gateway Routing Attribute from a buffer. The boolean return value indicates whether the parsing operation was a success or not.
        /// </summary>
        /// <param name="attrData">Buffer to parse</param>
        /// <param name="attribute">CAN Gateway Routing Attribute parsed from the buffer</param>
        /// <returns>Returns True, if buffer was successfully parsed; otherwise, False.</returns>
        public static bool TryParse(byte[] attrData, out CanGatewayRoutingAttribute attribute)
        {
            if (attrData == null || attrData.Length < LengthSize + TypeSize)
            {
                attribute = new CanGatewayRoutingAttribute(CanGatewayAttributeType.CGW_UNSPEC, new byte[] {});
                return false;
            }

            int length = NetlinkUtils.FromBytes<ushort>(attrData.Take(LengthSize).ToArray());
            CanGatewayAttributeType type = (CanGatewayAttributeType)NetlinkUtils.FromBytes<ushort>(attrData.Skip(LengthSize).Take(TypeSize).ToArray());
            byte[] data = attrData.Skip(TypeLengthSize).Take(length - TypeLengthSize).ToArray();

            attribute = new CanGatewayRoutingAttribute(type, data);
            return true;
        }

        /// <summary>
        /// Returns a string that represents the current CanGatewayRoutingAttribute object.
        /// </summary>
        /// <returns>A string that represents the current CanGatewayRoutingAttribute object.</returns>
        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Attribute Length: {Length}");
            stringBuilder.AppendLine($"Attribute Type: {Type}");
            stringBuilder.Append($"Attribute Data: {BitConverter.ToString(Data)}");
            return stringBuilder.ToString();
        }
    }
}