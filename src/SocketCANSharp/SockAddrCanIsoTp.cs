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
    /// Represents a SocketCAN ISO-TP address structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class SockAddrCanIsoTp : AbstractSockAddrCan
    {
        private uint _rxId;
        private uint _txId;

        /// <summary>
        /// CAN ID to receive on. Typically, CAN frames coming from the ECU will use this CAN ID (ex. 0x7e8).
        /// </summary>
        public uint RxId 
        { 
            get
            {
                return _rxId;
            }
            set
            {
                SocketCanUtils.ThrowIfCanIdStructureInvalid(value);
                _rxId = value;
            }
        }

        /// <summary>
        /// CAN ID to transmit with. Typically, CAN frames being sent from the Tester will use this CAN ID (ex. 0x7e0).
        /// </summary>
        public uint TxId
        {
            get
            {
                return _txId;
            }
            set
            {
                SocketCanUtils.ThrowIfCanIdStructureInvalid(value);
                _txId = value;
            }
        }

        /// <summary>
        /// Initializes a SocketCAN ISO-TP address structure with default values of zeroes.
        /// </summary>
        public SockAddrCanIsoTp() : base()
        {
            RxId = 0;
            TxId = 0;
        }

        /// <summary>
        /// Initializes a SocketCAN ISO-TP address structure to the AF_CAN address family and the provided interface index value. 
        /// </summary>
        /// <param name="interfaceIndex">Interface index value</param>
        public SockAddrCanIsoTp(int interfaceIndex) : base(interfaceIndex)
        {      
            RxId = 0;
            TxId = 0;   
        }

        /// <summary>
        /// Returns a string that represents the current SockAddrCanIsoTp object.
        /// </summary>
        /// <returns>A string that represents the current SockAddrCanIsoTp object.</returns>
        public override string ToString()
        {
            return $"Address Family: {CanFamily}; Interface Index: {CanIfIndex}; RxId: 0x{RxId:X}; TxId: 0x{TxId:X}";
        }
    }
}