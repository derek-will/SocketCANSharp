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

using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace SocketCANSharp.Netlink
{
    /// <summary>
    /// Netlink Native Methods
    /// </summary>
    public static class NetlinkNativeMethods
    {
        /// <summary>
        /// Creates a Netlink socket.
        /// </summary>
        /// <param name="addressFamily">Address Family</param>
        /// <param name="socketType">Type of socket</param>
        /// <param name="protocolType">Netlink Protocol Type</param>
        /// <returns>Socket Handle Wrapper Instance</returns>
        [DllImport("libc", EntryPoint = "socket", SetLastError = true)]
        public static extern SafeFileDescriptorHandle Socket(int addressFamily, SocketType socketType, NetlinkProtocolType protocolType);

        /// <summary>
        /// Assigns the specified Netlink address to the socket.
        /// </summary>
        /// <param name="socketHandle">Socket Handle Wrapper Instance</param>
        /// <param name="addr">Netlink address structure</param>
        /// <param name="addrSize">Size of address structure</param>
        /// <returns>0 on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="bind", SetLastError=true)]
        public static extern int Bind(SafeFileDescriptorHandle socketHandle, SockAddrNetlink addr, int addrSize);

        /// <summary>
        /// Returns the current address to which the socket is bound to.
        /// </summary>
        /// <param name="socketHandle">Socket handle</param>
        /// <param name="sockAddr">Address structure</param>
        /// <param name="sockAddrLen">The size of the the socket address structure in bytes</param>
        /// <returns>0 on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="getsockname", SetLastError=true)]
        public static extern int GetSockName(SafeFileDescriptorHandle socketHandle, SockAddrNetlink sockAddr, ref int sockAddrLen);

        /// <summary>
        /// Write the Network Interface Information Request to the socket.
        /// </summary>
        /// <param name="socketHandle">Socket Handle Wrappper Instance</param>
        /// <param name="req">Network Interface Information Request to write</param>
        /// <param name="reqSize">Size of Network Interface Information Request in bytes</param>
        /// <returns>The number of bytes written on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="write", SetLastError=true)]
        public static extern int Write(SafeFileDescriptorHandle socketHandle, NetworkInterfaceInfoRequest req, int reqSize);
    }
}