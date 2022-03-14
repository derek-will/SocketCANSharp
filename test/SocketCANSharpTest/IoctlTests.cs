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
using NUnit.Framework;
using SocketCANSharp;
using System.Net.Sockets;

namespace SocketCANSharpTest
{
    public class IoctlTests
    {
        SafeSocketHandle socketHandle;
        
        [SetUp]
        public void Setup()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);
        }

        [TearDown]
        public void Cleanup()
        {
            socketHandle.Close();
        }

        [Test]
        public void GetInterfaceIndex_vcan0_Test()
        {
            var ifr = new Ifreq("vcan0");
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
            Assert.AreNotEqual(-1, ioctlResult);
        }

        [Test]
        public void GetInterfaceIndex_vcan0_SocketClosed_Failure_Test()
        {
            socketHandle.Close();

            var ifr = new Ifreq("vcan0");
            Assert.Throws<ObjectDisposedException>(() => LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFINDEX, ifr));
        }

        [Test]
        public void EnableNonBlockingMode_Success_Test()
        {
            int arg = 1;
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.FIONBIO, ref arg);
            Assert.AreNotEqual(-1, ioctlResult);
        }

        [Test]
        public void DisableNonBlockingMode_Success_Test()
        {
            int arg = 0;
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.FIONBIO, ref arg);
            Assert.AreNotEqual(-1, ioctlResult);
        }
    }
}