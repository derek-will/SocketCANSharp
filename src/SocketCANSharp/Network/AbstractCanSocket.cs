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

namespace SocketCANSharp.Network
{
    /// <summary>
    /// Provides the base implementation for CAN sockets.
    /// </summary>
    public abstract class AbstractCanSocket : AbstractSocket
    {
        /// <summary>
        /// The protocol type of the CAN socket.
        /// </summary>
        public SocketCanProtocolType ProtocolType { get; protected set; }

        /// <summary>
        /// Obtains the timestamp of the last received packet which has been passed to the user.
        /// </summary>
        /// <returns>Timeval containing timestamp of the latest packet.</returns>
        /// <exception cref="SocketCanException">Failed to obtain timestamp of the last received packet.</exception>
        public Timeval GetLatestPacketReceiveTimestamp()
        {
            var timeval = new Timeval();
            int result = LibcNativeMethods.Ioctl(SafeHandle, SocketCanConstants.SIOCGSTAMP, timeval);
            if (result == -1)
                throw new SocketCanException("Failed to obtain timestamp of the last received packet.");

            return timeval;
        }
    }
}