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

namespace SocketCANSharp
{
    /// <summary>
    /// Settings associated with a file descriptor that is part of the interest list of an epoll file descriptor. 
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct EpollData
    {
        /// <summary>
        /// Used for storing a pointer in the epoll data.
        /// </summary>
        [FieldOffset(0)]
        public IntPtr Pointer;
        /// <summary>
        /// Used for storing a file descriptor in the epoll data.
        /// </summary>
        [FieldOffset(0)]
        public int FileDescriptor;
        /// <summary>
        /// Used for storing an unsigned 32 bit number in the epoll data.
        /// </summary>
        [FieldOffset(0)]
        public uint Unsigned32BitNumber;
        /// <summary>
        /// Used for storing an unsigned 64 bit number in the epoll data.
        /// </summary>
        [FieldOffset(0)]
        public ulong Unsigned64BitNumber;

        /// <summary>
        /// Returns a string that represents the current EpollData object.
        /// </summary>
        /// <returns>A string that represents the current EpollData object.</returns>
        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Pointer: " + (IntPtr.Size == 4 ? $"0x{Pointer:X8}" : $"0x{Pointer:X16}"));
            stringBuilder.AppendLine($"FileDescriptor: 0x{FileDescriptor:X8}");
            stringBuilder.AppendLine($"Unsigned32BitNumber: 0x{Unsigned32BitNumber:X8}");
            stringBuilder.Append($"Unsigned64BitNumber: 0x{Unsigned64BitNumber:X16}");
            return stringBuilder.ToString();
        }
    }
}