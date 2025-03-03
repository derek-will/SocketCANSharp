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

using System.Runtime.InteropServices;

namespace SocketCANSharp.Network.Netlink
{
    /// <summary>
    /// Represents a Netlink Routing Attribute.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RoutingAttribute
    {
        /// <summary>
        /// Length of Route Option.
        /// </summary>
        public ushort Length { get; set; }
        /// <summary>
        /// Type of Route Option.
        /// </summary>
        public ushort Type { get; set; }

        /// <summary>
        /// Initializes a Routing Attribute structure for the specified data length and type.
        /// </summary>
        /// <param name="dataLength">Length of Routing Attribute Data</param>
        /// <param name="type">Routing Attribute Type</param>
        public RoutingAttribute(ushort dataLength, ushort type)
        {
            Length = (ushort)NetlinkMessageMacros.RTA_LENGTH(dataLength);
            Type = type;
        }

        /// <summary>
        /// Converts a raw byte array into a RoutingAttribute instance.
        /// </summary>
        /// <param name="data">Raw Byte Array.</param>
        /// <returns>RoutingAttribute instance</returns>
        public static RoutingAttribute FromBytes(byte[] data)
        {
            return NetlinkUtils.FromBytes<RoutingAttribute>(data);
        }
    }
}