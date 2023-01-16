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

using System;
using System.Runtime.InteropServices;

namespace SocketCANSharp
{
    /// <summary>
    /// Represents Link Layer options for ISO 15765-2 (ISO-TP).
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class CanIsoTpLinkLayerOptions
    {
        /// <summary>
        /// Maximum Transmission Unit (MTU) which represents the CAN Frame Type.
        /// Classical CAN (2.0) Frame: 16
        /// CAN FD Frame: 72
        /// </summary>
        public byte Mtu { get; set; }
        /// <summary>
        /// Link Layer Transmit Length: 8, 12, 16, 20, 24, 32, 48, 64	
        /// </summary>
        public byte TxDataLength { get; set; }
        /// <summary>
        /// CAN FD specific flags (BRS, ESI, etc.) to use when transmitting CAN FD Frames.
        /// </summary>
        public CanFdFlags TxFlags { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the CanIsoTpLinkLayerOptions class using all default values.
        /// </summary>
        public CanIsoTpLinkLayerOptions()
        {
            Mtu = 0;
            TxDataLength = 0;
            TxFlags = CanFdFlags.None;
        }

        /// <summary>
        /// Initializes a new instance of the CanIsoTpLinkLayerOptions class using the specified MTU and Transmit Data Length. No CAN FD specific flags are set.
        /// </summary>
        /// <param name="mtu">Maximum Transmission Unit</param>
        /// <param name="txDataLength">Transmit Data Length</param>
        public CanIsoTpLinkLayerOptions(byte mtu, byte txDataLength) : this (mtu, txDataLength, CanFdFlags.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of the CanIsoTpLinkLayerOptions class using the specified MTU, Transmit Data Length and Transmit Flags.
        /// </summary>
        /// <param name="mtu">Maximum Transmission Unit</param>
        /// <param name="txDataLength">Transmit Data Length</param>
        /// <param name="txFlags">CAN FD specific flags</param>
        public CanIsoTpLinkLayerOptions(byte mtu, byte txDataLength, CanFdFlags txFlags)
        {
            if (mtu != SocketCanConstants.CAN_MTU && mtu != SocketCanConstants.CANFD_MTU)
                throw new ArgumentOutOfRangeException(nameof(mtu), $"MTU should be either {SocketCanConstants.CAN_MTU} for Classical CAN or {SocketCanConstants.CANFD_MTU} for CAN FD.");

            if (mtu == SocketCanConstants.CAN_MTU && txDataLength != SocketCanConstants.CAN_MAX_DLEN)
                throw new ArgumentOutOfRangeException(nameof(txDataLength), $"When MTU is set to {SocketCanConstants.CAN_MTU} then TxDataLength must be {SocketCanConstants.CAN_MAX_DLEN}.");

            if (mtu == SocketCanConstants.CANFD_MTU && Array.IndexOf(SocketCanConstants.CanFdFrameLengths, txDataLength) == -1)
                throw new ArgumentOutOfRangeException(nameof(txDataLength), $"When MTU is set to {SocketCanConstants.CANFD_MTU} then TxDataLength must be one of these: {string.Join(", ", SocketCanConstants.CanFdFrameLengths)}.");

            Mtu = mtu;
            TxDataLength = txDataLength;
            TxFlags = txFlags;
        }

        /// <summary>
        /// Returns a string that represents the current CanIsoTpLinkLayerOptions object.
        /// </summary>
        /// <returns>A string that represents the current CanIsoTpLinkLayerOptions object.</returns>
        public override string ToString()
        {
            return $"MTU: {Mtu}, TxDataLength: {TxDataLength}, CAN FD Transmit Flags: {TxFlags}";
        }
    }
}