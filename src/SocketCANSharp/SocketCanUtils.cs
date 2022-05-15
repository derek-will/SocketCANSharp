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

namespace SocketCANSharp
{
    /// <summary>
    /// Set of SocketCAN utility functions.
    /// </summary>
    public static class SocketCanUtils
    {
        /// <summary>
        /// Validates the SocketCAN CAN Identifier structure.
        /// </summary>
        /// <param name="canId">CAN Identifier structure</param>
        /// <exception cref="ArgumentException">CAN_EFF_FLAG not set on a CAN ID that exceeds 11 bits.</exception>
        public static void ThrowIfCanIdStructureInvalid(uint canId)
        {
            uint rawId = canId & SocketCanConstants.CAN_EFF_MASK; // remove ERR, RTR, and EFF flags
            if (rawId > 0x7ff && (canId & (uint)CanIdFlags.CAN_EFF_FLAG) == 0)
            {
                throw new ArgumentException("CAN_EFF_FLAG must be set when CAN ID exceeds 11 bits.", nameof(canId));
            }
        }
    }
}