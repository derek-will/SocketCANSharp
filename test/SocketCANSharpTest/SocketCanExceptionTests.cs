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

using NUnit.Framework;
using System;
using SocketCANSharp.Network;
using System.Net.Sockets;

namespace SocketCANSharpTest
{
    public class SocketCanExceptionTests
    {
        [Test]
        public void SocketCanException_Ctor_Success_Test()
        {
            using (var rawCanSocket = new RawCanSocket()) // needed in order to set errno to Success.
            {
                var ex = new SocketCanException();
                Assert.AreEqual(SocketError.Success, ex.SocketErrorCode);
                Assert.AreEqual(0, ex.NativeErrorCode);
                Assert.AreEqual(string.Empty, ex.Description);
            }
        }

        [Test]
        public void SocketCanException_Ctor_WithDescription_Success_Test()
        {
            using (var rawCanSocket = new RawCanSocket()) // needed in order to set errno to Success.
            {
                var ex = new SocketCanException("Test123");
                Assert.AreEqual(SocketError.Success, ex.SocketErrorCode);
                Assert.AreEqual(0, ex.NativeErrorCode);
                Assert.AreEqual("Test123", ex.Description);
            }
        }

        [Test]
        public void SocketCanException_Ctor_WithDescription_ArgumentNullException_Failure_Test()
        {
            Assert.Throws<ArgumentNullException>(() => new SocketCanException(null));
        }

        [Test]
        public void SocketCanException_Ctor_WithErrorCode_Success_Test()
        {
            var ex = new SocketCanException(10061);
            Assert.AreEqual(SocketError.ConnectionRefused, ex.SocketErrorCode);
            Assert.AreEqual(111, ex.NativeErrorCode);
            Assert.AreEqual(string.Empty, ex.Description);
        }

        [Test]
        public void SocketCanException_Ctor_WithErrorCodeAndDescription_Success_Test()
        {
            var ex = new SocketCanException(10057, "Test456");
            Assert.AreEqual(SocketError.NotConnected, ex.SocketErrorCode);
            Assert.AreEqual(107, ex.NativeErrorCode);
            Assert.AreEqual("Test456", ex.Description);
        }

        [Test]
        public void SocketCanException_Ctor_WithErrorCodeAndDescription_ArgumentNullException_Failure_Test()
        {
            Assert.Throws<ArgumentNullException>(() => new SocketCanException(10057, null));
        }
    }
}