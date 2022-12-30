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
    /// Represents a Netlink address structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class SockAddrNetlink
    {
        /// <summary>
        /// Address Family.
        /// </summary>
        public ushort NetlinkFamily { get; set; }
        /// <summary>
        /// Zero. Pad Byte.
        /// </summary>
        public ushort Pad { get; set; }
        /// <summary>
        /// Port ID. Must be unique per Netlink socket as it is the unicast address of the socket. 
        /// Set before the call to bind to allow the userspace application to set a unique identifier for the socket, otherwise leave as zero and the kernel will take care of assigning a unique identifier.
        /// </summary>
        public uint PortId { get; set; }
        /// <summary>
        /// Multicast groups mask. It is a bitmask with every bit representing a Netlink group number.
        /// </summary>
        public uint GroupsMask { get; set; }

        /// <summary>
        /// Initializes a Netlink address structure with default values of zeroes.
        /// </summary>
        public SockAddrNetlink()
        {
            NetlinkFamily = 0;
            Pad = 0;
            PortId = 0;
            GroupsMask = 0;
        }

        /// <summary>
        /// Initializes a Netlink address structure to the AF_NETLINK address family with the provided Port ID and Multicast Groups Mask.
        /// </summary>
        /// <param name="portId">Port ID</param>
        /// <param name="groupsMask">Multicast groups mask</param>
        public SockAddrNetlink(uint portId, uint groupsMask)
        {
            NetlinkFamily = NetlinkConstants.AF_NETLINK;
            Pad = 0;
            PortId = portId;
            GroupsMask = groupsMask;
        }
    }
}