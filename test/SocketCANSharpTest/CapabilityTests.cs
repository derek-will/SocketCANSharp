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
using NUnit.Framework;
using SocketCANSharp;
using SocketCANSharp.Capabilities;

namespace SocketCANSharpTest
{
    public class CapabilityTests
    {
        [Test]
        public void Capability_CapGet_Current_Process_V3_Test()
        {
            var hdr = new UserCapabilityHeader();
            hdr.Version = LinuxCapabilityVersion.LINUX_CAPABILITY_VERSION_3;
            hdr.Pid = System.Environment.ProcessId;
            var data = new UserCapabilityData[2];
            int result = CapNativeMethods.CapGet(hdr, data);
            Assert.That(0 == result, $"Errno: {LibcNativeMethods.Errno}");
            Console.WriteLine($"Version: {hdr.Version}");
            Console.WriteLine($"Pid: 0x{hdr.Pid:X8}");
            foreach (var d in data)
            {
                Console.WriteLine($"Effective: 0x{d.Effective:X8}");
                Console.WriteLine($"Permitted: 0x{d.Permitted:X8}");
                Console.WriteLine($"Inheritable: 0x{d.Inheritable:X8}");
            }
        }

        [Test]
        public void Capability_CapGet_Probe_Version_Test()
        {
            var hdr = new UserCapabilityHeader();
            hdr.Version = LinuxCapabilityVersion.UNSUPPORTED_LINUX_CAPABILITY_VERSION;
            int result = CapNativeMethods.CapGet(hdr, null);
            Assert.That(0 == result, $"Errno: {LibcNativeMethods.Errno}");
            Assert.AreNotEqual(LinuxCapabilityVersion.UNSUPPORTED_LINUX_CAPABILITY_VERSION, hdr.Version);
            Console.WriteLine($"Version: {hdr.Version}");
        }

        [Test]
        public void Capability_IsCapabilityRaised_Test()
        {
            var data0 = new UserCapabilityData();
            data0.Effective = 0x00001000;
            var data1 = new UserCapabilityData();
            var dataArray = new UserCapabilityData[] { data0, data1 };

            bool result = CapabilityUtils.IsCapabilityRaised(dataArray, Capability.CAP_NET_ADMIN);
            Assert.IsTrue(result);

            dataArray[0].Effective = 0x00002000;
            result = CapabilityUtils.IsCapabilityRaised(dataArray, Capability.CAP_NET_ADMIN);
            Assert.IsFalse(result);

            dataArray[1].Effective = 0x00000010;
            result = CapabilityUtils.IsCapabilityRaised(dataArray, Capability.CAP_BLOCK_SUSPEND);
            Assert.IsTrue(result);

            dataArray[1].Effective = 0x00000004;
            result = CapabilityUtils.IsCapabilityRaised(dataArray, Capability.CAP_BLOCK_SUSPEND);
            Assert.IsFalse(result);
        }

        [Test]
        public void Capability_GetPreferredCapabilityVersion_Test()
        {
            LinuxCapabilityVersion version = CapabilityUtils.GetPreferredCapabilityVersion();
            Assert.AreNotEqual(LinuxCapabilityVersion.UNSUPPORTED_LINUX_CAPABILITY_VERSION, version);
        }

        [Test]
        public void Capability_IsCurrentProcessCapable_Test()
        {
            // just want to make sure we don't throw. print result.
            bool isCapable = CapabilityUtils.IsCurrentProcessCapable(Capability.CAP_NET_ADMIN);
            Assert.IsNotNull(isCapable); 
            Console.WriteLine($"Has CAP_NET_ADMIN: {isCapable}");
        }
    }
}