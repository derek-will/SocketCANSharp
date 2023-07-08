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
using System.Text;
using System.Runtime.InteropServices;

namespace SocketCANSharp.Network.Netlink.Gateway
{
    /// <summary>
    /// CAN Gateway CRC8.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CgwCrc8
    {
        private const int CRC_TABLE_SIZE = 256;
        private const int CRC_PROFILE_DATA_SIZE = 20;

        /// <summary>
        /// From Index.
        /// </summary>
        public sbyte FromIndex { get; set; }
        /// <summary>
        /// To Index.
        /// </summary>
        public sbyte ToIndex { get; set; }
        /// <summary>
        /// Result Index.
        /// </summary>
        public sbyte ResultIndex { get; set; }
        /// <summary>
        /// Initial CRC Value.
        /// </summary>
        public byte InitialCrcValue { get; set; }
        /// <summary>
        /// Final XOR Value.
        /// </summary>
        public byte FinalXorValue { get; set; }
        /// <summary>
        /// CRC Table.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=CRC_TABLE_SIZE)]
        public byte[] CrcTable;
        /// <summary>
        /// CRC Profile.
        /// </summary>
        public Crc8Profile Profile { get; set; }
        /// <summary>
        /// CRC Profile Data.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=CRC_PROFILE_DATA_SIZE)]
        public byte[] ProfileData;

        /// <summary>
        /// Returns a string that represents the current CgwCrc8 object.
        /// </summary>
        /// <returns>A string that represents the current CgwCrc8 object.</returns>
        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"From Index: {FromIndex}");
            stringBuilder.AppendLine($"To Index: {ToIndex}");
            stringBuilder.AppendLine($"Result Index: {ResultIndex}");
            stringBuilder.AppendLine($"Initial CRC Value: {InitialCrcValue:X2}");
            stringBuilder.AppendLine($"Final XOR Value: {FinalXorValue:X2}");
            stringBuilder.AppendLine($"CRC Table: {BitConverter.ToString(CrcTable)}");
            stringBuilder.AppendLine($"Profile: {Profile}");
            stringBuilder.Append($"Profile Data: {BitConverter.ToString(ProfileData)}");
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Determines if the structure is valid or not.
        /// </summary>
        /// <returns>True if the structure is valid.</returns>
        public bool IsValid()
        {
            return CrcTable != null && CrcTable.Length == CRC_TABLE_SIZE && ProfileData != null && ProfileData.Length == CRC_PROFILE_DATA_SIZE && Enum.IsDefined(typeof(Crc8Profile), Profile) && Profile != Crc8Profile.CGW_CRC8PRF_UNSPEC;
        }
    }
}