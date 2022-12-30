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
using System.Text;
using System.Runtime.InteropServices;

namespace SocketCANSharp.Netlink
{
    /// <summary>
    /// CAN hardware-dependent bit timing constant. These values are used for calculating and checking bit timing parameters.
    /// </summary>
    public struct CanBitTimingConstant
    {
        /// <summary>
        /// Name of the CAN controller hardware.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string Name;
        /// <summary>
        /// Time Segment 1 Minimum.
        /// </summary>
        public uint TimeSegment1Minimum { get; set; }
        /// <summary>
        /// Time Segment 1 Maximum.
        /// </summary>
        public uint TimeSegment1Maximum { get; set; }
        /// <summary>
        /// Time Segment 2 Minimum.
        /// </summary>
        public uint TimeSegment2Minimum { get; set; }
        /// <summary>
        /// Time Segment 2 Maximum.
        /// </summary>
        public uint TimeSegment2Maximum { get; set; }
        /// <summary>
        /// Sync Jump Width Maximum.
        /// </summary>
        public uint SyncJumpWidthMaximum { get; set; }
        /// <summary>
        /// Bit Rate Prescaler Minimum.
        /// </summary>
        public uint BitRatePrescalerMinimum { get; set; }
        /// <summary>
        /// Bit Rate Prescaler Maximum.
        /// </summary>
        public uint BitRatePrescalerMaximum { get; set; }
        /// <summary>
        /// Bit Rate Prescaler Increment.
        /// </summary>
        public uint BitRatePrescalerIncrement { get; set; }

        /// <summary>
        /// Converts a raw byte array into a CanBitTimingConstant instance.
        /// </summary>
        /// <param name="data">Raw Byte Array.</param>
        /// <returns>CanBitTimingConstant instance</returns>
        public static CanBitTimingConstant FromBytes(byte[] data)
        {
            return NetlinkUtils.FromBytes<CanBitTimingConstant>(data);
        }

        /// <summary>
        /// Returns a string representation of the current CanBitTimingConstant instance.
        /// </summary>
        /// <returns>String representation of this CanBitTimingConstant instance.</returns>
        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Name: {Name}");
            stringBuilder.AppendLine($"Time Segment 1 Minimum: {TimeSegment1Minimum}");
            stringBuilder.AppendLine($"Time Segment 1 Maximum: {TimeSegment1Maximum}");
            stringBuilder.AppendLine($"Time Segment 2 Minimum: {TimeSegment2Minimum}");
            stringBuilder.AppendLine($"Time Segment 2 Maximum: {TimeSegment2Maximum}");
            stringBuilder.AppendLine($"Sync Jump Width Maximum: {SyncJumpWidthMaximum}");
            stringBuilder.AppendLine($"Bit Rate Prescaler Minimum: {BitRatePrescalerMinimum}");
            stringBuilder.AppendLine($"Bit Rate Prescaler Maximum: {BitRatePrescalerMaximum}");
            stringBuilder.AppendLine($"Bit Rate Prescaler Increment: {BitRatePrescalerIncrement}");
            return stringBuilder.ToString();
        }
    }
}