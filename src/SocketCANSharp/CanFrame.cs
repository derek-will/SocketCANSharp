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
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace SocketCANSharp
{
    /// <summary>
    /// Represents a Classical CAN (Controller Area Network) Frame.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CanFrame
    {
        private uint _canId;

        /// <summary>
        /// Controller Area Network Identifier structure:
        /// bit 0-28: CAN identifier (11 or 29 bit)
        /// bit 29: Error frame flag (0 = Data frame, 1 = Error frame)
        /// bit 30: Remote frame flag (1 = Remote Transmission Request (RTR) bit is set)
        /// bit 31: Frame format flag (0 = standard 11 bit, 1 = extended 29 bit)
        /// </summary>
        public uint CanId 
        { 
            get
            {
                return _canId;
            }
            set
            {
                SocketCanUtils.ThrowIfCanIdStructureInvalid(value);
                _canId = value;
            }
        }
        /// <summary>
        /// Frame length in bytes.
        /// </summary>
        public byte Length { get; set; }
        /// <summary>
        /// Padding / Reserved.
        /// </summary>
        public byte Pad { get; set; }
        /// <summary>
        /// Padding / Reserved.
        /// </summary>
        public byte Res0 { get; set; } 
        /// <summary>
        /// Optional DLC value (9-15) at 8 byte payload length. Please note that CAN_CTRLMODE_CC_LEN8_DLC flag has to be enabled in the CAN driver.
        /// </summary>
        public byte Len8Dlc { get; set; }
        /// <summary>
        /// Classical CAN Frame payload.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=SocketCanConstants.CAN_MAX_DLEN)]
        public byte[] Data;

        /// <summary>
        /// Initializes a new instance of the CanFrame structure with the specified CAN ID and Data payload.
        /// </summary>
        /// <param name="canId">CAN ID including remote, error, and SFF/EFF flags.</param>
        /// <param name="data">Payload data.</param>
        public CanFrame(uint canId, byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (data.Length > SocketCanConstants.CAN_MAX_DLEN)
                throw new ArgumentOutOfRangeException(nameof(data), $"Data must be {SocketCanConstants.CAN_MAX_DLEN} bytes or less");

            SocketCanUtils.ThrowIfCanIdStructureInvalid(canId);

            _canId = canId;
            Length = (byte) data?.Length;
            Pad = 0x00;
            Res0 = 0x00;
            Len8Dlc = 0;
            Data = new byte[SocketCanConstants.CAN_MAX_DLEN];

            for (int i = 0; i < data.Length; i++)
            {
                Data[i] = data[i];
            }
        }

        /// <summary>
        /// Returns a string that represents the current CanFrame object.
        /// </summary>
        /// <returns>A string that represents the current CanFrame object.</returns>
        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"CAN ID Flags: EFF: {((uint)CanIdFlags.CAN_EFF_FLAG & CanId) != 0}, RTR: {((uint)CanIdFlags.CAN_RTR_FLAG & CanId) != 0}, ERR: {((uint)CanIdFlags.CAN_ERR_FLAG & CanId) != 0}");
            stringBuilder.Append($"CAN Data: 0x{SocketCanConstants.CAN_EFF_MASK & CanId:X} [{Length:D2}] {BitConverter.ToString(Data.Take(Length).ToArray()).Replace("-", " ")}");
            return stringBuilder.ToString();
        }
    }
}