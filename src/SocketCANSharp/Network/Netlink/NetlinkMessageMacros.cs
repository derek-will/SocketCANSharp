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

namespace SocketCANSharp.Netlink
{
    /// <summary>
    /// Macros used to create and access Netlink Messages. 
    /// The buffer passed to and from a Netlink socket should only be accessed via these macros.
    /// </summary>
    public static class NetlinkMessageMacros
    {
        /// <summary>
        /// Alignment value for all Netlink message headers, payloads, attributes, etc.
        /// </summary>
        public const int NLMSG_ALIGNTO = 4;

        /// <summary>
        /// Alignment value for all Routing Attributes.
        /// </summary>
        public const int RTA_ALIGNTO = 4;

        /// <summary>
        /// Gets the Netlink message header length.
        /// </summary>
        public static int NLMSG_HDRLEN { get { return NLMSG_ALIGN(Marshal.SizeOf<NetlinkMessageHeader>()); } }

        /// <summary>
        /// Returns true if the Netlink message is not truncated and is in a form suitable for parsing. 
        /// </summary>
        /// <param name="nlh">Netlink Message Header.</param>
        /// <param name="len">Number of bytes read from socket.</param>
        /// <returns>True, if message is OK for parsing. Otherwise, false.</returns>
        public static bool NLMSG_OK(NetlinkMessageHeader nlh, int len)
        {
            int size = Marshal.SizeOf<NetlinkMessageHeader>();
            return len >= size && nlh.MessageLength >= size && nlh.MessageLength <= len;
        }

        /// <summary>
        /// Round the length of a Netlink message up to align it properly. 
        /// </summary>
        /// <param name="len">Length of a Netlink message.</param>
        /// <returns>Rounded up / aligned message length value.</returns>
        public static int NLMSG_ALIGN(int len)
        {
            return (len + NLMSG_ALIGNTO - 1) & ~(NLMSG_ALIGNTO - 1);
        }

        /// <summary>
        /// Given the payload length returns the aligned length to store in the Netlink message length field of the header.
        /// </summary>
        /// <param name="len">Payload length</param>
        /// <returns>The aligned length to store in the Netlink message length field of the header.</returns>
        public static int NLMSG_LENGTH(int len)
        {
            return len + NLMSG_HDRLEN;
        }

        /// <summary>
        /// Return the number of bytes that a Netlink message with a payload of the specified length would occupy. 
        /// </summary>
        /// <param name="len">Payload length value</param>
        /// <returns>The number of bytes that a Netlink message with a data payload of the provided length occupies.</returns>
        public static int NLMSG_SPACE(int len)
        {
            return NLMSG_ALIGN(NLMSG_LENGTH(len));
        }

        /// <summary>
        /// Return the length of the payload associated with the Netlink message header. 
        /// </summary>
        /// <param name="nlh">Netlink Message Header</param>
        /// <param name="len">Payload Length</param>
        /// <returns>The length of the Netlink message payload associated with the header.</returns>
        public static int NLMSG_PAYLOAD(NetlinkMessageHeader nlh, int len)
        {
            return (int)nlh.MessageLength - NLMSG_SPACE(len);        
        }

        /// <summary>
        /// Returns true if the Routing Attribute is valid.
        /// </summary>
        /// <param name="rta">Routing Attribute.</param>
        /// <param name="len">Running length of the attribute buffer.</param>
        /// <returns>True, if the Routing Attribute is valid. Otherwise, false.</returns>
        public static bool RTA_OK(RoutingAttribute rta, int len)
        {
            int size = Marshal.SizeOf<RoutingAttribute>();
            return len >= size && rta.Length >= size && rta.Length <= len;
        }

        /// <summary>
        /// Round the length of a Routing Attribute to align it properly.
        /// </summary>
        /// <param name="len">Length of a Routing Attribute.</param>
        /// <returns>Round up / aligned Routine Attribute length.</returns>
        public static int RTA_ALIGN(int len)
        {
            return (len + RTA_ALIGNTO - 1) & ~(RTA_ALIGNTO - 1);
        }

        /// <summary>
        /// Gets the length which is required for the specified length in bytes of data plus the header.
        /// </summary>
        /// <param name="len">Length in bytes of data</param>
        /// <returns>Required length</returns>
        public static int RTA_LENGTH(int len)
        {
            return RTA_ALIGN(Marshal.SizeOf<RoutingAttribute>()) + len;
        }

        /// <summary>
        /// Gets the length of this routing attribute's data.
        /// </summary>
        /// <param name="rta">Routing Attribute</param>
        /// <returns>Length of this routing attribute's data</returns>
        public static int RTA_PAYLOAD(RoutingAttribute rta) 
        {
            return rta.Length - RTA_LENGTH(0);
        }
    }
}