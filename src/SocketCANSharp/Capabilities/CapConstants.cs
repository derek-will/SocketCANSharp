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

namespace SocketCANSharp.Capabilities
{
    /// <summary>
    /// Constants associated with Capabilities.
    /// </summary>
    public static class CapConstants
    {
        /// <summary>
        /// The number of capabilities that the kernel has for version 1 of the Capabilities API which is 1 unsigned 32-bit integer worth.
        /// </summary>
        public const int LINUX_CAPABILITY_U32S_1 = 1;
        /// <summary>
        /// The number of capabilities that the kernel has for version 2 of the Capabilities API which is 2 unsigned 32-bit integers worth.
        /// </summary>
        public const int LINUX_CAPABILITY_U32S_2 = 2;
        /// <summary>
        /// The number of capabilities that the kernel has for version 3 of the Capabilities API which is 2 unsigned 32-bit integers worth.
        /// </summary>
        public const int LINUX_CAPABILITY_U32S_3 = 2;
    }
}