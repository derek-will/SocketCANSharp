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
using System.Linq;
using System.Runtime.InteropServices;

namespace SocketCANSharp
{
    /// <summary>
    /// Generic Broadcast Manager Message for reading from and writing to a CAN_BCM socket. Can support both CAN and CAN FD messages. Variant for 32-bit.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class BcmGenericMessage32
    {
        /// <summary>
        /// Broadcast Manager Message Header.
        /// </summary>
        public BcmMessageHeader32 Header { get; set; }

        /// <summary>
        /// Sequence of frames in raw byte array format. 
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=257*72)]
        public byte[] Frames;

        /// <summary>
        /// Instantiates a new Generic Broadcast Manager Message with default values.
        /// </summary>
        public BcmGenericMessage32() : this(new BcmMessageHeader32(BcmOpcode.UNDEFINED))
        {
        }
    
        /// <summary>
        /// Instantiates a new Generic Broadcast Manager Message using the supplied header.
        /// </summary>
        /// <param name="header">Message Header</param>
        public BcmGenericMessage32(BcmMessageHeader32 header)
        {
            if (header == null)
                throw new ArgumentNullException(nameof(header), "Header cannot be null");

            Header = header;
            Frames = new byte[257*72];
        }

        /// <summary>
        /// Converts the Frame data byte array to a CanFrame array.
        /// </summary>
        /// <returns>CanFrame array derived from the raw byte array.</returns>
        /// <exception cref="InvalidOperationException">Thrown when invoked on a Message with the CAN_FD_FRAME flag set.</exception>
        public CanFrame[] GetClassicFrames()
        {
            if (Header.Flags.HasFlag(BcmFlags.CAN_FD_FRAME))
                throw new InvalidOperationException();

            var frames = new CanFrame[Header.NumberOfFrames];
            int arrayOffset = 0;
            for (int i = 0; i < Header.NumberOfFrames; i++)
            {
                byte[] destArray = new byte[SocketCanConstants.CAN_MTU];
                Array.Copy(Frames, arrayOffset, destArray, 0, SocketCanConstants.CAN_MTU);
                frames[i] = new CanFrame()
                {
                    CanId = BitConverter.ToUInt32(destArray, 0),
                    Length = destArray[4],
                    Pad = destArray[5],
                    Res0 = destArray[6],
                    Len8Dlc = destArray[7],
                    Data = destArray.Skip(8).ToArray(),
                };
            }

            return frames;
        }

        /// <summary>
        /// Converts the Frame data byte array to a CanFdFrame array.
        /// </summary>
        /// <returns>CanFdFrame array derived from the raw byte array.</returns>
        /// <exception cref="InvalidOperationException">Thrown when invoked on a Message that does not have the CAN_FD_FRAME flag set.</exception>
        public CanFdFrame[] GetFdFrames()
        {
            if (Header.Flags.HasFlag(BcmFlags.CAN_FD_FRAME) == false)
                throw new InvalidOperationException();

            var frames = new CanFdFrame[Header.NumberOfFrames];
            int arrayOffset = 0;
            for (int i = 0; i < Header.NumberOfFrames; i++)
            {
                byte[] destArray = new byte[SocketCanConstants.CANFD_MTU];
                Array.Copy(Frames, arrayOffset, destArray, 0, SocketCanConstants.CANFD_MTU);
                frames[i] = new CanFdFrame()
                {
                    CanId = BitConverter.ToUInt32(destArray, 0),
                    Length = destArray[4],
                    Flags = (CanFdFlags)destArray[5],
                    Res0 = destArray[6],
                    Res1 = destArray[7],
                    Data = destArray.Skip(8).ToArray(),
                };
            }

            return frames;
        }
    }
}