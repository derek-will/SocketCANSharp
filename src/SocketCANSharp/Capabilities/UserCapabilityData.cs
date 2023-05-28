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

using System.Runtime.InteropServices;

namespace SocketCANSharp.Capabilities
{
    /// <summary>
    /// Information about the capability sets of a thread.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct UserCapabilityData
    {
        /// <summary>
        /// The set of capabilities used by the kernel to perform permission checks on the thread.
        /// </summary>
        public uint Effective { get; set; }
        /// <summary>
        /// The limiting superset for the effective capabilities that the thread may assume and the capabilities 
        /// that may be added to the inheritable set by the thread if it does not have the CAP_SETPCAP capability in its 
        /// effective set.
        /// </summary>
        public uint Permitted { get; set; }
        /// <summary>
        /// A set of capabilities preserved across an execve call. This enables the thread to assign capabilities 
        /// to the permitted set of a newly executed program. 
        /// </summary>
        public uint Inheritable { get; set; }
    }
}
