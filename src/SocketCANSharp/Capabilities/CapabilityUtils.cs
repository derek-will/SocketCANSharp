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
using System.Diagnostics;

namespace SocketCANSharp.Capabilities
{
    /// <summary>
    /// Constants associated with Capabilities.
    /// </summary>
    public static class CapabilityUtils
    {
        /// <summary>
        /// Checks whether a capability is in the effective set or not.
        /// </summary>
        /// <param name="dataArray">Capability data associated with a thread</param>
        /// <param name="cap">Capability</param>
        /// <returns>True, if the capability is in the effective set, otherwise False.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the data array is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the data array is not long enough</exception>
        public static bool IsCapabilityRaised(UserCapabilityData[] dataArray, Capability cap)
        {
            if (dataArray == null)
                throw new ArgumentNullException(nameof(dataArray));

            int capIndex = (int)cap;
            int requireDataIndex = (capIndex / 32);
            if (dataArray.Length < (requireDataIndex + 1))
                throw new ArgumentOutOfRangeException(nameof(cap), "Capability cannot be detected given the provided data array length.");

            int relativeIndex = capIndex % 32;
            return (1 << relativeIndex & dataArray[requireDataIndex].Effective) != 0;
        }

        /// <summary>
        /// Retrieves the preferred capability version of the system.
        /// </summary>
        /// <returns>Preferred capability version of the system</returns>
        /// <exception cref="NotSupportedException">If capget call fails, then this exception is thrown</exception>
        public static LinuxCapabilityVersion GetPreferredCapabilityVersion()
        {
            var hdr = new UserCapabilityHeader();
            hdr.Version = LinuxCapabilityVersion.UNSUPPORTED_LINUX_CAPABILITY_VERSION;
            int result = CapNativeMethods.CapGet(hdr, null);
            if (result != 0)
                throw new PlatformNotSupportedException($"capget returned errno: {LibcNativeMethods.Errno}");

            return hdr.Version;
        }

        /// <summary>
        /// Determines if the current process possesses the designated capability in the effective set or not. 
        /// </summary>
        /// <param name="capability">Capability to check</param>
        /// <returns>True, if capability is in effective set. Otherwise, false.</returns>
        /// <exception cref="PlatformNotSupportedException">If capget call fails, then this exception is thrown</exception>
        public static bool IsCurrentProcessCapable(Capability capability)
        {
            LinuxCapabilityVersion version = GetPreferredCapabilityVersion();
            var header = new UserCapabilityHeader();
            header.Version = version;
            using (var process = Process.GetCurrentProcess())
            {
                header.Pid = process.Id;
            }

            int dataArraySize = 0;
            switch (version)
            {
                case LinuxCapabilityVersion.LINUX_CAPABILITY_VERSION_1:
                    dataArraySize = CapConstants.LINUX_CAPABILITY_U32S_1;
                    break;
                case LinuxCapabilityVersion.LINUX_CAPABILITY_VERSION_2:
                    dataArraySize = CapConstants.LINUX_CAPABILITY_U32S_2;
                    break;
                case LinuxCapabilityVersion.LINUX_CAPABILITY_VERSION_3:
                    dataArraySize = CapConstants.LINUX_CAPABILITY_U32S_3;
                    break;
                default:
                    dataArraySize = CapConstants.LINUX_CAPABILITY_U32S_3;
                    break;
            }
            var dataArray = new UserCapabilityData[dataArraySize];

            int result = CapNativeMethods.CapGet(header, dataArray);
            if (result != 0)
                throw new PlatformNotSupportedException($"capget returned errno: {LibcNativeMethods.Errno}");

            return IsCapabilityRaised(dataArray, capability);
        }
    }
}