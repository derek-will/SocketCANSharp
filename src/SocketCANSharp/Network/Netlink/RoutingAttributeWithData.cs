#region License
/* 
BSD 3-Clause License

Copyright (c) 2024, Derek Will
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
using System.Runtime.InteropServices;
using System.Text;

namespace SocketCANSharp.Network.Netlink
{
    /// <summary>
    /// Routing Attribute that includes data in addition to the length and type fields.
    /// </summary>
    public struct RoutingAttributeWithData
    {
        /// <summary>
        /// Routing Attribute information: Length and Type.
        /// </summary>
        public RoutingAttribute Attribute { get; private set; }

        /// <summary>
        /// Routing Attribute data.
        /// </summary>
        public byte[] Data { get; private set; }

        /// <summary>
        /// Initializes a Routing Attribute with Data structure using the specified Routing Attribute only and populating no data.
        /// </summary>
        /// <param name="rtAttr">Routing Attribute</param>
        public RoutingAttributeWithData(RoutingAttribute rtAttr)
        {
            Attribute = rtAttr;
            Data = new byte[0];
        }

        /// <summary>
        /// Initializes a Routing Attribute with Data structure using the specified type and string value.
        /// </summary>
        /// <param name="type">Routing Attribute Type</param>
        /// <param name="value">String value</param>
        public RoutingAttributeWithData(ushort type, string value)
        {
            Attribute = new RoutingAttribute((ushort)value.Length, type);
            Data = Encoding.ASCII.GetBytes(value);
        }

        /// <summary>
        /// Initializes a Routing Attribute with Data structure using the specified type and unsigned 32-bit integer value.
        /// </summary>
        /// <param name="type">Routing Attribute Type</param>
        /// <param name="value">Unsigned 32-bit integer value</param>
        public RoutingAttributeWithData(ushort type, uint value)
        {
            Attribute = new RoutingAttribute((ushort)Marshal.SizeOf<uint>(), type);
            Data = BitConverter.GetBytes(value);
        }

        /// <summary>
        /// Initializes a Routing Attribute with Data structure using the specified type and unsigned 16-bit integer value.
        /// </summary>
        /// <param name="type">Routing Attribute Type</param>
        /// <param name="value">Unsigned 16-bit integer value</param>
        public RoutingAttributeWithData(ushort type, ushort value)
        {
            Attribute = new RoutingAttribute((ushort)Marshal.SizeOf<ushort>(), type);
            Data = BitConverter.GetBytes(value);
        }

        /// <summary>
        /// Initializes a Routing Attribute with Data structure using the specified type and CanBitTiming struct value.
        /// </summary>
        /// <param name="type">Routing Attribute Type</param>
        /// <param name="value">CanBitTiming struct value</param>
        public RoutingAttributeWithData(ushort type, CanBitTiming value)
        {
            Attribute = new RoutingAttribute((ushort)Marshal.SizeOf<CanBitTiming>(), type);
            Data = NetlinkUtils.ToBytes(value);
        }

        /// <summary>
        /// Initializes a Routing Attribute with Data structure using the specified type and CanControllerMode struct value.
        /// </summary>
        /// <param name="type">Routing Attribute Type</param>
        /// <param name="value">CanControllerMode struct value</param>
        public RoutingAttributeWithData(ushort type, CanControllerMode value)
        {
            Attribute = new RoutingAttribute((ushort)Marshal.SizeOf<CanControllerMode>(), type);
            Data = NetlinkUtils.ToBytes(value);
        }

        /// <summary>
        /// Converts the Routing Attribute with Data structure into a raw byte array.
        /// </summary>
        /// <returns>A raw byte array that corresponds to the Routing Attribute with Data structure.</returns>
        public byte[] ToBytes()
        {
            byte[] data = new byte[Marshal.SizeOf<RoutingAttribute>() + Data.Length];;
            byte[] tlData = NetlinkUtils.ToBytes(Attribute);
            Buffer.BlockCopy(tlData, 0, data, 0, tlData.Length);
            Buffer.BlockCopy(Data, 0, data, tlData.Length, Data.Length);
            return data;
        }
    }
}