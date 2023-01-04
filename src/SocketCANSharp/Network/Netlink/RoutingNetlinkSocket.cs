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
using System.Net.Sockets;

namespace SocketCANSharp.Network.Netlink
{
    /// <summary>
    /// Provides Routing Netlink (rtnetlink) socket services.
    /// </summary>
    public class RoutingNetlinkSocket : AbstractNetlinkSocket
    {
        /// <summary>
        /// Initializes a new instance of the RoutingNetlinkSocket class.
        /// </summary>
        /// <exception cref="SocketCanException">Unable to create the requested socket.</exception>
        public RoutingNetlinkSocket()
        {
            SocketType = SocketType.Raw;
            ProtocolType = NetlinkProtocolType.NETLINK_ROUTE;
            SafeHandle = NetlinkNativeMethods.Socket(NetlinkConstants.PF_NETLINK, SocketType, ProtocolType);

            if (SafeHandle.IsInvalid)
                throw new SocketCanException("Failed to create NETLINK_ROUTE socket.");
        }

        /// <summary>
        /// Writes the supplied Network Interface Information request to the socket.
        /// </summary>
        /// <param name="request">Network Interface Information to transmit.</param>
        /// <returns>Number of bytes written to the socket.</returns>
        /// <exception cref="ObjectDisposedException">The socket has been closed.</exception>
        /// <exception cref="SocketCanException">Writing to the underlying NETLINK_ROUTE socket failed.</exception>
        public int Write(NetworkInterfaceInfoRequest request)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            int bytesWritten = NetlinkNativeMethods.Write(SafeHandle, request, (int)request.Header.MessageLength);
            if (bytesWritten == -1)
                throw new SocketCanException("Writing to the underlying NETLINK_ROUTE socket failed.");

            return bytesWritten;
        }

        /// <summary>
        /// Reads data from the socket into the supplied receive buffer.
        /// </summary>
        /// <param name="data">An array of bytes that is the receive buffer</param>
        /// <returns>The number of bytes received from the socket.</returns>
        /// <exception cref="ObjectDisposedException">The socket has been closed.</exception>
        /// <exception cref="ArgumentNullException">The data byte array is null.</exception>
        /// <exception cref="SocketCanException">Reading from the underlying NETLINK_ROUTE socket failed.</exception>
        public int Read(byte[] data)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            int bytesRead = LibcNativeMethods.Read(SafeHandle, data, data.Length);
            if (bytesRead == -1)
            {
                throw new SocketCanException("Reading from the underlying NETLINK_ROUTE socket failed.");
            }

            return bytesRead;
        }
    }
}