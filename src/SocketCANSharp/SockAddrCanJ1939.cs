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
    /// <summary>
    /// Represents a SocketCAN SAE J1939 address structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class SockAddrCanJ1939 : AbstractSockAddrCan
    {
        /// <summary>
        /// 64-bit NAME belonging to a J1939 Node.
        /// </summary>
        public ulong Name { get; set; }
        /// <summary>
        /// Parameter Group Number (PGN) to use. Note that the PGN is part of the 29-bit CAN ID.
        /// </summary>
        public uint PGN { get; set; }
        /// <summary>
        /// Address byte belonging to a J1939 Node.
        /// </summary>
        public byte Address { get; set; }

        /// <summary>
        /// Initializes a SocketCAN SAE J1939 address structure with default values of zeroes.
        /// </summary>
        public SockAddrCanJ1939() : base()
        {
            Name = 0;
            PGN = 0;
            Address = 0;
        }

        /// <summary>
        /// Initializes a SocketCAN SAE J1939 address structure to the AF_CAN address family and the provided interface index value. 
        /// </summary>
        /// <param name="interfaceIndex">Interface index value</param>
        public SockAddrCanJ1939(int interfaceIndex) : base(interfaceIndex)
        {         
        }

        /// <summary>
        /// Returns a string that represents the current SockAddrCanJ1939 object.
        /// </summary>
        /// <returns>A string that represents the current SockAddrCanJ1939 object.</returns>
        public override string ToString()
        {
            return $"Address Family: {CanFamily}; Interface Index: {CanIfIndex}; Name: 0x{Name:X16}; PGN: 0x{PGN:X8}; Address: 0x{Address:X2}";
        }
    }
}