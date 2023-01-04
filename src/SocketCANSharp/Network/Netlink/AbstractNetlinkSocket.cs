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
using System.Runtime.InteropServices;

namespace SocketCANSharp.Network.Netlink
{
    /// <summary>
    /// Provides the base implementation for Netlink sockets.
    /// </summary>
    public abstract class AbstractNetlinkSocket : AbstractSocket
    {
        /// <summary>
        /// The protocol type of the Netlink socket.
        /// </summary>
        public NetlinkProtocolType ProtocolType { get; protected set; }

        /// <summary>
        /// The current address to which this socket is bound.
        /// </summary>
        public SockAddrNetlink Address
        {
            get
            {
                return GetSockAddr();
            }
        }

        private SockAddrNetlink GetSockAddr()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            var addr = new SockAddrNetlink();
            int size = Marshal.SizeOf(typeof(SockAddrNetlink));
            int result = NetlinkNativeMethods.GetSockName(SafeHandle, addr, ref size);

            if (result != 0)
                throw new SocketCanException("Unable to get name on Netlink socket.");

            return addr;
        }

        /// <summary>
        /// Assigns the Netlink Address Structure to the Netlink socket.
        /// </summary>
        /// <param name="addr">Netlink Address Structure.</param>
        /// <exception cref="ObjectDisposedException">The Netlink socket has been closed.</exception>
        /// <exception cref="ArgumentNullException">Netlink Address Structure is null.</exception>
        /// <exception cref="SocketCanException">Unable to assign the provided Netlink Address Structure to the Netlink socket.</exception>
        public void Bind(SockAddrNetlink addr)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (addr == null)
                throw new ArgumentNullException(nameof(addr));

            int result = NetlinkNativeMethods.Bind(SafeHandle, addr, Marshal.SizeOf(typeof(SockAddrNetlink)));
            if (result != 0)
                throw new SocketCanException("Unable to assign the provided Netlink address to the underlying Netlink Socket.");

            IsBound = true;
        }
    }
}