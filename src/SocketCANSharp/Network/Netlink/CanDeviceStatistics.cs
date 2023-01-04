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
    /// CAN Device Statistics.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class CanDeviceStatistics
    {
        /// <summary>
        /// Bus errors.
        /// </summary>
        public uint BusErrors { get; set; }
        /// <summary>
        /// Changes to error warning state.
        /// </summary>
        public uint ErrorWarning { get; set; }
        /// <summary>
        /// Changes to error passive state.
        /// </summary>
        public uint ErrorPassive { get; set; }
        /// <summary>
        /// Changes to bus off state.
        /// </summary>
        public uint BusOff { get; set; }
        /// <summary>
        /// Arbitration lost errors.
        /// </summary>
        public uint ArbitrationLost { get; set; }
        /// <summary>
        /// CAN controller restarts.
        /// </summary>
        public uint Restarts { get; set; }

        /// <summary>
        /// Converts a raw byte array into a CanDeviceStatistics instance.
        /// </summary>
        /// <param name="data">Raw Byte Array.</param>
        /// <returns>CanDeviceStatistics instance</returns>
        public static CanDeviceStatistics FromBytes(byte[] data)
        {
            return NetlinkUtils.FromBytes<CanDeviceStatistics>(data);
        }

        /// <summary>
        /// Returns a string representation of the current CanDeviceStatistics instance.
        /// </summary>
        /// <returns>String representation of this CanDeviceStatistics instance.</returns>
        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"BusErrors: {BusErrors}");
            stringBuilder.AppendLine($"ErrorWarning: {ErrorWarning}");
            stringBuilder.AppendLine($"ErrorPassive: {ErrorPassive}");
            stringBuilder.AppendLine($"BusOff: {BusOff}");
            stringBuilder.AppendLine($"ArbitrationLost: {ArbitrationLost}");
            stringBuilder.AppendLine($"Restarts: {Restarts}");
            return stringBuilder.ToString();
        }
    }
}