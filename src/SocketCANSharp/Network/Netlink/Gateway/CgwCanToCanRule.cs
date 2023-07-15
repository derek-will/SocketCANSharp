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
    /// CGW CAN-to-CAN Routing Rule.
    /// </summary>
    public class CgwCanToCanRule : CgwBaseRule
    {
        /// <summary>
        /// Source CAN Network Interface Index.
        /// </summary>
        public uint SourceIndex { get; set; }
        /// <summary>
        /// Destination CAN Network Interface Index.
        /// </summary>
        public uint DestinationIndex { get; set; }
        /// <summary>
        /// CAN Receive Filter on Source Interface.
        /// </summary>
        public CanFilter? ReceiveFilter { get; set; }

        /// <summary>
        /// Initializes a new instance of the CgwCanToCanRule class.
        /// </summary>
        /// <param name="canFrameType">CAN Frame Type</param>
        public CgwCanToCanRule(CgwCanFrameType canFrameType) : base(CanGatewayType.CGW_TYPE_CAN_CAN, canFrameType)
        {
        }

        /// <summary>
        /// Returns a string that represents the current CgwCanToCanRule object.
        /// </summary>
        /// <returns>A string that represents the current CgwCanToCanRule object.</returns>
        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(base.ToString());

            if (SourceIndex != 0)
            {
                var ptr = Marshal.AllocHGlobal(SocketCanConstants.IF_NAMESIZE);
                try
                {
                    IntPtr p = LibcNativeMethods.IfIndexToName(SourceIndex, ptr);
                    string srcName = p == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(ptr);
                    stringBuilder.AppendLine($"Source: {(srcName == null ? SourceIndex.ToString() : srcName)}");
                }
                finally
                {
                    Marshal.FreeHGlobal(ptr);
                }
            }
            else
            {
                stringBuilder.AppendLine($"Source: <none>");
            }

            if (DestinationIndex != 0)
            {
                var ptr = Marshal.AllocHGlobal(SocketCanConstants.IF_NAMESIZE);
                try
                {
                    IntPtr p = LibcNativeMethods.IfIndexToName(DestinationIndex, ptr);
                    string dstName = p == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(ptr);
                    stringBuilder.AppendLine($"Destination: {(dstName == null ? DestinationIndex.ToString() : dstName)}");
                }
                finally
                {
                    Marshal.FreeHGlobal(ptr);
                }
            }
            else
            {
                stringBuilder.AppendLine($"Destination: <none>");
            }

            stringBuilder.AppendLine($"Receive Filter: {(ReceiveFilter.HasValue ? ReceiveFilter.ToString() : "<none>")}");
            return stringBuilder.ToString();
        }
    }
}