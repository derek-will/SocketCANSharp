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
using System.Collections.ObjectModel;

namespace SocketCANSharp
{
    /// <summary>
    /// Set of SocketCAN utility functions.
    /// </summary>
    public static class SocketCanUtils
    {
        /// <summary>
        /// Returns CAN XL Header Size.
        /// </summary>
        public static int CanXlHeaderSize { get { return Marshal.OffsetOf<CanXlFrame>("Data").ToInt32(); } }

        /// <summary>
        /// Validates the SocketCAN CAN Identifier structure.
        /// </summary>
        /// <param name="canId">CAN Identifier structure</param>
        /// <exception cref="ArgumentException">CAN_EFF_FLAG not set on a CAN ID that exceeds 11 bits.</exception>
        public static void ThrowIfCanIdStructureInvalid(uint canId)
        {
            uint rawId = canId & SocketCanConstants.CAN_EFF_MASK; // remove ERR, RTR, and EFF flags
            if (rawId > 0x7ff && (canId & (uint)CanIdFlags.CAN_EFF_FLAG) == 0)
            {
                throw new ArgumentException("CAN_EFF_FLAG must be set when CAN ID exceeds 11 bits.", nameof(canId));
            }
        }

        /// <summary>
        /// CAN FD data length to Data Length Code (DLC).
        /// </summary>
        /// <param name="length">CAN FD data length</param>
        /// <returns>Data Length Code (DLC) corresponding to the CAN FD data length</returns>
        public static byte CanFdLengthToDlc(byte length)
        {
            switch (length)
            {
                case 0:
                    return 0;
                case 1:
                    return 1;
                case 2:
                    return 2;
                case 3:
                    return 3;
                case 4:
                    return 4;
                case 5:
                    return 5;
                case 6:
                    return 6;
                case 7:
                    return 7;
                case 8:
                    return 8;
                case 12:
                    return 9;
                case 16:
                    return 10;
                case 20:
                    return 11;
                case 24:
                    return 12;
                case 32:
                    return 13;
                case 48:
                    return 14;
                case 64:
                    return 15;
                default:
                    throw new ArgumentOutOfRangeException(nameof(length), $"Unsupported CAN FD Data Length: {length}.");
            }
        }

        /// <summary>
        /// CAN FD Data Length Code (DLC) to CAN data length.
        /// </summary>
        /// <param name="dlc">CAN FD Data Length Code (DLC)</param>
        /// <returns>CAN data length corresponding to the CAN FD Data Length Code (DLC)</returns>
        public static byte CanFdDlcToLength(byte dlc)
        {
            if (dlc > 15)
                throw new ArgumentException("CAN FD DLC cannot exceed 15.", nameof(dlc));

            var dlc2Len = new ReadOnlyCollection<byte>(
                new byte[] {
                    0, 1, 2, 3, 4, 5, 6, 7,
                    8, 12, 16, 20, 24, 32, 48, 64});

            return dlc2Len[dlc];
        }

        /// <summary>
        /// Classical CAN Data Length Code (DLC) to CAN data length.
        /// </summary>
        /// <param name="dlc">Classical CAN Data Length Code (DLC)</param>
        /// <returns>CAN data length corresponding to the Classical Data Length Code (DLC)</returns>
        public static byte CanDlcToLength(byte dlc)
        {
            if (dlc > 15)
                throw new ArgumentException("Classical CAN DLC cannot exceed 15.", nameof(dlc));

            var dlc2Len = new ReadOnlyCollection<byte>(
                new byte[] {
                    0, 1, 2, 3, 4, 5, 6, 7,
                    8, 8, 8, 8, 8, 8, 8, 8});

            return dlc2Len[dlc];
        }

        /// <summary>
        /// Extracts the Raw CAN ID from a CAN ID that contains embedded flags.
        /// </summary>
        /// <param name="canIdWithFlags">CAN ID with embedded flags</param>
        /// <returns>Raw CAN ID value</returns>
        public static uint ExtractRawCanId(uint canIdWithFlags)
        {
            if ((canIdWithFlags & (uint)CanIdFlags.CAN_EFF_FLAG) == 0)
                return (SocketCanConstants.CAN_SFF_MASK & canIdWithFlags);
            else
                return (SocketCanConstants.CAN_EFF_MASK & canIdWithFlags);
        }

        /// <summary>
        /// Creates the CAN ID with embedded flags (EFF, RTR, ERR).
        /// </summary>
        /// <param name="rawCanId">Raw CAN ID</param>
        /// <param name="isEff">Is Extended Frame Format</param>
        /// <param name="isRtr">Is Remote Transmission Request</param>
        /// <param name="isErr">Is Error Frame</param>
        /// <returns>CAN ID with embedded flags (EFF, RTR, ERR)</returns>
        /// <exception cref="ArgumentException">Raw CAN ID exceeds 11 bits, but EFF is not set to true.</exception>
        public static uint CreateCanIdWithFlags(uint rawCanId, bool isEff, bool isRtr, bool isErr)
        {
            if (rawCanId > SocketCanConstants.CAN_SFF_MASK && isEff == false)
            {
                throw new ArgumentException("Must set EFF flag on a CAN ID that exceeds 11 bits.");
            }

            uint canIdWithFlags = rawCanId;
            if (isEff)
                canIdWithFlags = canIdWithFlags | (uint)CanIdFlags.CAN_EFF_FLAG;
            if (isRtr)
                canIdWithFlags = canIdWithFlags | (uint)CanIdFlags.CAN_RTR_FLAG;
            if (isErr)
                canIdWithFlags = canIdWithFlags | (uint)CanIdFlags.CAN_ERR_FLAG;
            return canIdWithFlags;
        }

        /// <summary>
        /// Configures the raw CAN ID with the Format Flag. This shortcut method assumes Standard Frame Format for CAN IDs that are 11-bit and under and Extended Frame Format for CAN IDs over 11-bit.
        /// Note: For fine-grain CAN ID configuration use <c>CreateCanIdWithFlags</c>.
        /// </summary>
        /// <param name="rawCanId">Raw CAN ID</param>
        /// <returns>CAN ID with embedded frame format flag set when appropriate</returns>
        /// <exception cref="ArgumentException">Raw CAN IDs cannot exceed size of 29-bit.</exception>
        public static uint CreateCanIdWithFormatFlag(uint rawCanId)
        {
            if (rawCanId > SocketCanConstants.CAN_EFF_MASK)
            {
                throw new ArgumentException("Raw CAN IDs cannot exceed size of 29-bit.");
            }

            if (rawCanId > SocketCanConstants.CAN_SFF_MASK)
                return ((uint)CanIdFlags.CAN_EFF_FLAG | rawCanId);
            else
                return rawCanId;
        }
    }
}