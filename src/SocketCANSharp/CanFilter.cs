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
    /// Represents a CAN ID based filter. Filter matches when 'Received CanId' &amp; CanMask == CanId &amp; CanMask.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CanFilter
    {
        /// <summary>
        /// Contains the relevant bits of the CAN ID which are not masked out. Set CAN_INV_FILTER bit to invert the filter.
        /// </summary>
        public uint CanId { get; set; }
        /// <summary>
        /// Contains the CAN ID filter to apply (bit-wise AND) to the CAN ID of incoming CAN Frames. Set CAN_ERR_FLAG to receive Error Frames.
        /// </summary>
        public uint CanMask { get; set; }

        /// <summary>
        /// Initializes a new instance of the CanFilter structure using the specified CAN ID and CAN Mask.
        /// </summary>
        /// <param name="canId">The relevants bits of the CAN ID</param>
        /// <param name="canMask">The CAN ID filter apply</param>
        public CanFilter(uint canId, uint canMask)
        {
            CanId = canId;
            CanMask = canMask;
        }
    }
}