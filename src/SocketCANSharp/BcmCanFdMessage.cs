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
    /// Broadcast Manager Message for reading and writing to a CAN_BCM socket.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class BcmCanFdMessage
    {
        /// <summary>
        /// Broadcast Manager Message Header.
        /// </summary>
        public BcmMessageHeader Header { get; set; }

        /// <summary>
        /// Sequence of CAN FD frames. 
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=257)]
        public CanFdFrame[] Frames;

        /// <summary>
        /// Instantiates a new Broadcast Manager Message with default values including setting the CAN_FD_FRAME flag in the header.
        /// </summary>
        public BcmCanFdMessage() : this(new BcmMessageHeader(BcmOpcode.UNDEFINED))
        {
        }
    
        /// <summary>
        /// Instantiates a new Broadcast Manager Message using the supplied header. The CAN_FD_FRAME flag is also set in the header if it is not already set.
        /// </summary>
        /// <param name="header">Message Header</param>
        public BcmCanFdMessage(BcmMessageHeader header)
        {
            if (header == null)
                throw new ArgumentNullException(nameof(header), "Header cannot be null");

            Header = header;
            header.Flags |= BcmFlags.CAN_FD_FRAME;
            Frames = new CanFdFrame[257];
        }
        /// <summary>
        /// Instantiates a new Broadcast Manager Message using the supplied header and sequence of CAN FD frames. In addition, the CAN_FD_FRAME flag is set in the header.
        /// </summary>
        /// <param name="header">Message Header</param>
        /// <param name="frames">Sequence of CAN FD frames</param>
        public BcmCanFdMessage(BcmMessageHeader header, CanFdFrame[] frames) : this(header)
        {
            if (frames == null)
                throw new ArgumentNullException(nameof(frames), "Frames cannot be null");
                
            if (frames.Length > 257)
                throw new ArgumentOutOfRangeException(nameof(frames), "Frames must be 257 frames in length or less");

            if (header.NumberOfFrames != frames.Length)
                throw new ArgumentOutOfRangeException(nameof(frames), $"Number of frames referenced in the header ({header.NumberOfFrames}) must match the size of the frames array ({frames.Length}).");

            for (int i = 0; i < frames.Length; i++)
            {
                Frames[i] = frames[i];
            }
        }
    }
}