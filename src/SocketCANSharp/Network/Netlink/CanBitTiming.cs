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

using System.Text;

namespace SocketCANSharp.Netlink
{
    /// <summary>
    /// CAN Bit Timing Parameters.
    /// </summary>
    public struct CanBitTiming
    {
        /// <summary>
        /// Bit rate in bits per second.
        /// </summary>
        public uint BitRate { get; set; }
        /// <summary>
        /// Sample point in one-tenth of a percent.
        /// </summary>
        public uint SamplePoint { get; set; }
        /// <summary>
        /// Time quanta (TQ) in nanoseconds.
        /// </summary>
        public uint TimeQuanta { get; set; }
        /// <summary>
        /// Propagation segment in TQs.
        /// </summary>
        public uint PropagationSegment { get; set; }
        /// <summary>
        /// Phase buffer segment 1 in TQs.
        /// </summary>
        public uint PhaseBufferSegment1 { get; set; }
        /// <summary>
        /// Phase buffer segment 2 in TQs.
        /// </summary>
        public uint PhaseBufferSegment2 { get; set; }
        /// <summary>
        /// Synchronisation jump width in TQs.
        /// </summary>
        public uint SyncJumpWidth { get; set; }
        /// <summary>
        /// Bit Rate Prescaler.
        /// </summary>
        public uint BitRatePrescaler { get; set; }

        /// <summary>
        /// Converts a raw byte array into a CanBitTiming instance.
        /// </summary>
        /// <param name="data">Raw Byte Array.</param>
        /// <returns>CanBitTiming instance</returns>
        public static CanBitTiming FromBytes(byte[] data)
        {
            return NetlinkUtils.FromBytes<CanBitTiming>(data);
        }

        /// <summary>
        /// Returns a string representation of the current CanBitTiming instance.
        /// </summary>
        /// <returns>String representation of this CanBitTiming instance.</returns>
        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"BitRate: {BitRate}");
            stringBuilder.AppendLine($"SamplePoint: {SamplePoint}");
            stringBuilder.AppendLine($"TimeQuanta: {TimeQuanta}");
            stringBuilder.AppendLine($"PropagationSegment: {PropagationSegment}");
            stringBuilder.AppendLine($"PhaseBufferSegment1: {PhaseBufferSegment1}");
            stringBuilder.AppendLine($"PhaseBufferSegment2: {PhaseBufferSegment2}");
            stringBuilder.AppendLine($"SyncJumpWidth: {SyncJumpWidth}");
            stringBuilder.AppendLine($"BitRatePrescaler: {BitRatePrescaler}");
            return stringBuilder.ToString();
        }
    }
}