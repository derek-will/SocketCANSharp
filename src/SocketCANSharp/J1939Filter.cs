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
   /// Represents an SAE J1939 Filter.
   /// </summary>
   [StructLayout(LayoutKind.Sequential)]
   public struct J1939Filter
   {
        /// <summary>
        /// NAME value to match against after applying the NAME Mask.
        /// </summary>
        public ulong Name { get; set;}
        /// <summary>
        /// Mask to apply to the NAME.
        /// </summary>
        public ulong NameMask { get; set; }
        /// <summary>
        /// Parameter Group Number (PGN) to match against after applying the PGN Mask.
        /// </summary>
        public uint PGN { get; set; }
        /// <summary>
        /// Mask to apply to the PGN.
        /// </summary>
        public uint PGNMask { get; set; }
        /// <summary>
        /// Address value to match against after applying the Address Mask.
        /// </summary>
        public byte Address { get; set; }
        /// <summary>
        /// Mask to apply to the Address.
        /// </summary>
        public byte AddressMask { get; set; }

        /// <summary>
        /// Returns a string that represents the current J1939Filter object.
        /// </summary>
        /// <returns>A string that represents the current J1939Filter object.</returns>
        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Name: 0x{Name:X16}");
            stringBuilder.AppendLine($"NameMask: 0x{NameMask:X16}");
            stringBuilder.AppendLine($"PGN: 0x{PGN:X8}");
            stringBuilder.AppendLine($"PGNMask: 0x{PGNMask:X8}");
            stringBuilder.AppendLine($"Address: 0x{Address:X2}");
            stringBuilder.Append($"AddressMask: 0x{AddressMask:X2}");
            return stringBuilder.ToString();
        }
   }
}