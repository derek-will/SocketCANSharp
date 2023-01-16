#region License
/* 
BSD 3-Clause License

Copyright (c) 2021, Derek Will
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

namespace SocketCANSharp
{
    /*
    Prior to Linux Kernel 5.4 and the introduction of CAN_J1939, the call to bind or connect expected 
    a structure length of at least 16 bytes for CAN_RAW and CAN_BCM sockets. Linux Kernel 5.4 and later 
    only requires 8 bytes for CAN_RAW and CAN_BCM sockets. In order to be compatible with both implementations, 
    this managed class pads the base class (which just includes the can_family and can_ifindex) by 8 bytes. 
    */

    /// <summary>
    /// Represents a SocketCAN base address structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 8)]
    public class SockAddrCan : AbstractSockAddrCan
    {
        /// <summary>
        /// Initializes a SocketCAN base address structure with default values of zeroes.
        /// </summary>
        public SockAddrCan() : base()
        {
        }

        /// <summary>
        /// Initializes a SocketCAN base address structure to the AF_CAN address family and the provided interface index value.
        /// </summary>
        /// <param name="interfaceIndex">Interface index value</param>
        public SockAddrCan(int interfaceIndex) : base(interfaceIndex)
        {
        }

        /// <summary>
        /// Returns a string that represents the current SockAddrCan object.
        /// </summary>
        /// <returns>A string that represents the current SockAddrCan object.</returns>
        public override string ToString()
        {
            return $"Address Family: {CanFamily}; Interface Index: {CanIfIndex}";
        }
    }
}