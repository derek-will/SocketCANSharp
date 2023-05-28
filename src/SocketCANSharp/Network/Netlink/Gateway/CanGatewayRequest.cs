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
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=1536)]
        public byte[] Data;

        /// <summary>
        /// Initializes a new instance of the CanGatewayRequest class.
        /// </summary>
        public CanGatewayRequest()
        {
            Data = new byte[1536];
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
    }
}