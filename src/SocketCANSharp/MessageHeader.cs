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
using System.Text;
using System.Runtime.InteropServices;

namespace SocketCANSharp
{
    /// <summary>
    /// Message header structure used by 'recvmsg' and 'sendmsg' functions.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MessageHeader
    {
        /// <summary>
        /// Address to send to or receive from depending on the function.
        /// </summary>
        public IntPtr Name { get; set; }

        /// <summary>
        /// Length of the address data.
        /// </summary>
        public int NameLength { get; set; }

        /// <summary>
        /// IoVectors to send or receive into.
        /// </summary>
        public IntPtr IoVectors { get; set; }

        /// <summary>
        /// Number of IoVector structures.
        /// </summary>
        public IntPtr IoVectorCount { get; set; }

        /// <summary>
        /// Ancillary Data element (Control Message).
        /// </summary>
        public IntPtr ControlMessage;

        /// <summary>
        /// Size of the Ancillary Data element.
        /// </summary>
        public IntPtr ControlMessageLength { get; set; }

        /// <summary>
        /// Message Flags.
        /// </summary>
        public MessageFlags Flags { get; set; }

        /// <summary>
        /// Returns a string that represents the current MessageHeader object.
        /// </summary>
        /// <returns>A string that represents the current MessageHeader object.</returns>
        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Name: " + (IntPtr.Size == 4 ? $"0x{Name:X8}" : $"0x{Name:X16}"));
            stringBuilder.AppendLine($"NameLength: {NameLength}");
            stringBuilder.AppendLine($"IoVectors: " + (IntPtr.Size == 4 ? $"0x{IoVectors:X8}" : $"0x{IoVectors:X16}"));
            stringBuilder.AppendLine($"IoVectorCount: {IoVectorCount}");
            stringBuilder.AppendLine($"ControlMessage: " + (IntPtr.Size == 4 ? $"0x{ControlMessage:X8}" : $"0x{ControlMessage:X16}"));
            stringBuilder.AppendLine($"ControlMessageLength: {ControlMessageLength}");
            stringBuilder.Append($"Flags: {Flags}");
            return stringBuilder.ToString();
        }
    }
}