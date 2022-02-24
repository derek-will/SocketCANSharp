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
    /// Represents options for handling ISO 15765-2 (ISO-TP) FlowControl (FC) Frames.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class CanIsoTpFlowControlOptions
    {
        /// <summary>
        /// BlockSize is part of the FC Frame and is the requested number of frames to send per block between FC Frames. Set to 0 to turn off (no limit).
        /// </summary>
        public byte BlockSize { get; set; }

        /// <summary>
        /// Separation Time is part of the FC Frame and is the requested minimum separation time between each frame in a block. 
        /// 0x00 - 0x7F : 0 - 127 ms
        /// 0x80 - 0xF0 : reserved
        /// 0xF1 - 0xF9 : 100 us - 900 us
        /// 0xFA - 0xFF : reserved
        /// </summary>
        public byte Stmin { get; set; }

        /// <summary>
        /// Maximum number of consecutive FC frames with 'wait' FlowStatus (FS = 1) allowed. Set to 0 to disable.
        /// </summary>
        public byte WftMax { get; set; }

        /// <summary>
        /// Initializes a new instance of the CanIsoTpFlowControlOptions class using sensible default values.
        /// </summary>
        public CanIsoTpFlowControlOptions()
        {
            BlockSize = 0; // no limit
            Stmin = 0; // no delay - send as fast as possible
            WftMax = 0; // disable wait FC frame maximum
        }
        
        /// <summary>
        /// Initializes a new instance of the CanIsoTpFlowControlOptions class using the supplied BlockSize, STmin, and WFTmax values.
        /// </summary>
        /// <param name="blockSize">BlockSize to set in FC Frame.</param>
        /// <param name="stmin">STmin to set in FC Frame.</param>
        /// <param name="wftMax">Maximum number of 'Wait' FC Frame Transmissions allowed.</param>
        public CanIsoTpFlowControlOptions(byte blockSize, byte stmin, byte wftMax)
        {
            BlockSize = blockSize;
            Stmin = stmin;
            WftMax = wftMax;
        }
    }
}