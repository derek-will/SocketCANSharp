#region License
/* 
BSD 3-Clause License

Copyright (c) 2023, Derek Will
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
    /// Represents a CAN XL (Controller Area Network Extra Long) Frame
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CanXlFrame
    {
        /// <summary>
        /// Priority property contains multiple elements: 
        ///     Bits 0-10: 11-bit Priority ID for bus arbitration purposes on the CAN XL network.
        ///     Bits 15-23: 8-bit Virtual CAN network ID which allows running up to 256 logical networks on a single physical CAN XL network.
        /// </summary>
        public uint Priority { get; set; }
        /// <summary>
        /// CAN XL specific flags for SEC, XL Format.
        /// </summary>
        public CanXlFlags Flags { get; set; }
        /// <summary>
        /// CAN XL SDU (Service Data Unit) Type.
        /// </summary>
        public CanXlSduType SduType { get; set; } 
        /// <summary>
        /// Frame payload length in bytes.
        /// </summary>
        public ushort Length { get; set; }
        /// <summary>
        /// Addressing information.
        /// </summary>
        public uint AcceptanceField { get; set; }
        /// <summary>
        /// CAN XL Frame payload.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=SocketCanConstants.CANXL_MAX_DLEN)]
        public byte[] Data;

        /// <summary>
        /// Initializes a new instance of the CanXlFrame structure using the supplied priority, SDT, acceptance field, flags and data.
        /// </summary>
        /// <param name="priority">Priority property containing Priority ID and VCID.</param>
        /// <param name="sdt">SDU (Service Data Unit) Type.</param>
        /// <param name="acceptanceField">Acceptance field containing address information.</param>
        /// <param name="data">Payload data.</param>
        /// <param name="flags">CAN XL specific flags.</param>
        public CanXlFrame(uint priority, CanXlSduType sdt, uint acceptanceField, byte[] data, CanXlFlags flags)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (data.Length > SocketCanConstants.CANXL_MAX_DLEN)
                throw new ArgumentOutOfRangeException(nameof(data), $"Data must be {SocketCanConstants.CANXL_MAX_DLEN} bytes or less");

            Priority = priority;
            SduType = sdt;
            AcceptanceField = acceptanceField;
            Length = (ushort) data?.Length;
            Flags = flags;
            Data = new byte[SocketCanConstants.CANXL_MAX_DLEN];

            for (int i = 0; i < data.Length; i++)
            {
                Data[i] = data[i];
            }
        }

        /// <summary>
        /// Shortcut to set the Priority ID element on the Priority property.
        /// </summary>
        /// <param name="priorityId">11-bit Priority ID</param>
        public void SetPriorityId(ushort priorityId)
        {
            Priority = SocketCanUtils.SetCanXlPriorityId(Priority, priorityId);
        }

        /// <summary>
        /// Shortcut to get the Priority ID element from the Priority property.
        /// </summary>
        /// <returns>11-bit Priority ID</returns>
        public ushort GetPriorityId()
        {
            return SocketCanUtils.GetCanXlPriorityId(Priority);
        }

        /// <summary>
        /// Shortcut to set the VCID element on the Priority property.
        /// </summary>
        /// <param name="vcid">8-bit Virtual CAN network ID</param>
        public void SetVCID(byte vcid)
        {
            Priority = SocketCanUtils.SetCanXlVCID(Priority, vcid);
        }

        /// <summary>
        /// Shortcut to get the VCID element from the Priority property.
        /// </summary>
        /// <returns>8-bit Virtual CAN network ID</returns>
        public byte GetVCID()
        {
            return SocketCanUtils.GetCanXlVCID(Priority);
        }

        /// <summary>
        /// Returns a string that represents the current CanXlFrame object.
        /// </summary>
        /// <returns>A string that represents the current CanXlFrame object.</returns>
        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"CAN XL Flags: {Flags.ToString()}");
            stringBuilder.AppendLine($"CAN XL SDT: {SduType.ToString()}");
            stringBuilder.Append($"CAN Data: 0x{Priority:X}-0x{AcceptanceField:X} [{Length:D2}] {BitConverter.ToString(Data.Take(Length).ToArray()).Replace("-", " ")}");
            return stringBuilder.ToString();
        }
    }
}