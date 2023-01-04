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
using System.Runtime.InteropServices;

namespace SocketCANSharp.Network.Netlink
{
    /// <summary>
    /// CAN Controller Mode Information. 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class CanControllerMode
    {
        /// <summary>
        /// Flag Mask.
        /// </summary>
        public uint Mask { get; set; }
        /// <summary>
        /// CAN Controller Mode Flags.
        /// </summary>
        public CanControllerModeFlags Flags { get; set; }

        /// <summary>
        /// Converts a raw byte array into a CanControllerMode instance.
        /// </summary>
        /// <param name="data">Raw Byte Array.</param>
        /// <returns>CanControllerMode instance</returns>
        public static CanControllerMode FromBytes(byte[] data)
        {
            return NetlinkUtils.FromBytes<CanControllerMode>(data);
        }

        /// <summary>
        /// Returns a string representation of the current CanControllerMode instance.
        /// </summary>
        /// <returns>String representation of this CanControllerMode instance.</returns>
        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Mask: {Mask}");
            stringBuilder.AppendLine($"Flags: {Flags}");
            return stringBuilder.ToString();
        }
    }
}