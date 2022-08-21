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
    /// Macros used to create and access Control Messages. Control Messages are also referred to as Ancillary Data. 
    /// The Control Messages are not part of the Socket payload and can be sent via `sendmsg` and received via `recvmsg`.
    /// </summary>
    public static class ControlMessageMacros
    {
        /// <summary>
        /// Given a length this function will return it along with the required alignment. 
        /// </summary>
        /// <param name="len">Length of data</param>
        /// <returns>Length along with the required alignment</returns>
        public static int CMSG_ALIGN(int len)
        {
            // (((len) + sizeof (size_t) - 1) & (size_t) ~(sizeof (size_t) - 1))
            return (len + IntPtr.Size - 1) & ~(IntPtr.Size - 1);
        }

        /// <summary>
        /// Returns the number of bytes an ancillary element with the provided data length occupies.
        /// </summary>
        /// <param name="len">Length of data</param>
        /// <returns>Number of bytes occupied by an ancillary element of the provided length</returns>
        public static int CMSG_SPACE(int len)
        {
            // (CMSG_ALIGN (len) + CMSG_ALIGN (sizeof (struct cmsghdr)))
            return CMSG_ALIGN(len) + CMSG_ALIGN(IntPtr.Size + sizeof(int) + sizeof(int));
        }
    }
}