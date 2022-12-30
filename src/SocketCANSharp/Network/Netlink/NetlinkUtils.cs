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

namespace SocketCANSharp.Netlink
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
    }
}