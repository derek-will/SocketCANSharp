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

using System.Text;
using System.Runtime.InteropServices;

namespace SocketCANSharp
{
    /// <summary>
    /// Broadcast Manager Message Header which contains the main properties of a message to be written to or read from a CAN_BCM socket.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class BcmMessageHeader
    {
        /// <summary>
        /// Command to send to CAN_BCM socket.
        /// </summary>
        public BcmOpcode Opcode { get; set; }
        /// <summary>
        /// Flags that add additional behavior modifications to a BCM Message.
        /// </summary>
        public BcmFlags Flags { get; set; }
        /// <summary>
        /// TX_SETUP: Number of times to send CAN Frame sequence at Interval1. If this value is zero, the value of Interval1 has no role and does not need to be specified.
        /// RX_SETUP: No function.
        /// </summary>
        public uint Interval1Count { get; set; }
        /// <summary>
        /// TX_SETUP: Interval to send the first 'Interval1Count' CAN frame sequence at.
        /// RX_SETUP: Specifies the timeout for receiving CAN Frames.
        /// </summary>
        public BcmTimeval Interval1 { get; set; }
        /// <summary>
        /// TX_SETUP: Interval to send the CAN frame sequence at. If set to 0, then transmission stops after the first interval count is complete. 
        /// RX_SETUP: Specifies the minimum interval at which successive RX_CHANGED messages for each CAN ID may be transmitted from the BCM.
        /// </summary>
        public BcmTimeval Interval2 { get; set; }
        /// <summary>
        /// CAN ID used for various purposes. 
        /// TX_SETUP / TX_SEND: When TX_CP_CAN_ID flag is set then this value is copied for the CAN ID of every CAN Frame.
        /// RX_SETUP: When RX_FILTER_ID flag is set then this value is solely used to define a filter.
        /// TX_DELETE: Removes frames with this CAN ID from the transmission queue.
        /// TX_READ / RX_READ: Used to extract information from the transmission queue or receive filter for the specified CAN ID.
        /// </summary>
        public uint CanId { get; set; }
        /// <summary>
        /// The CAN Frame sequence count.
        /// </summary>
        public uint NumberOfFrames { get; set; }

        /// <summary>
        /// Instantiates a BCM Message Header with the all the defaults.
        /// </summary>
        public BcmMessageHeader() : this(BcmOpcode.UNDEFINED)
        {
        }

        /// <summary>
        /// Instantiates a BCM Message Header with the designated Opcode.
        /// </summary>
        /// <param name="opcode">BCM Opcode (command)</param>
        public BcmMessageHeader(BcmOpcode opcode)
        {
            Opcode = opcode;
            NumberOfFrames = 0;
            Flags = BcmFlags.None;
            Interval1Count = 0;
            Interval1 = new BcmTimeval(0, 0);
            Interval2 = new BcmTimeval(0, 0);
            CanId = 0x00;     
        }

        /// <summary>
        /// Returns a string that represents the current BcmMessageHeader object.
        /// </summary>
        /// <returns>A string that represents the current BcmMessageHeader object.</returns>
        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"OperationalCode: {Opcode}");
            stringBuilder.AppendLine($"NumberOfFrames: {NumberOfFrames}");
            stringBuilder.AppendLine($"Flags: {Flags}");
            stringBuilder.AppendLine($"Interval1Count: {Interval1Count}");
            stringBuilder.AppendLine($"Interval1: [{Interval1}]");
            stringBuilder.AppendLine($"Interval2: [{Interval2}]");
            stringBuilder.Append($"CAN ID: 0x{CanId:X}");
            return stringBuilder.ToString();
        }
    }
}