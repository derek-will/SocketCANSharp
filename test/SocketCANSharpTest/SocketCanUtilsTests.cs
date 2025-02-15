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
using SocketCANSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SocketCANSharpTest
{
    public class SocketCanUtilsTests
    {
        [Test]
        public void CanDlcToLength_Test()
        {
            Assert.AreEqual(0, SocketCanUtils.CanDlcToLength(0));
            Assert.AreEqual(1, SocketCanUtils.CanDlcToLength(1));
            Assert.AreEqual(2, SocketCanUtils.CanDlcToLength(2));
            Assert.AreEqual(3, SocketCanUtils.CanDlcToLength(3));
            Assert.AreEqual(4, SocketCanUtils.CanDlcToLength(4));
            Assert.AreEqual(5, SocketCanUtils.CanDlcToLength(5));
            Assert.AreEqual(6, SocketCanUtils.CanDlcToLength(6));
            Assert.AreEqual(7, SocketCanUtils.CanDlcToLength(7));
            Assert.AreEqual(8, SocketCanUtils.CanDlcToLength(8));
            Assert.AreEqual(8, SocketCanUtils.CanDlcToLength(9));
            Assert.AreEqual(8, SocketCanUtils.CanDlcToLength(10));
            Assert.AreEqual(8, SocketCanUtils.CanDlcToLength(11));
            Assert.AreEqual(8, SocketCanUtils.CanDlcToLength(12));
            Assert.AreEqual(8, SocketCanUtils.CanDlcToLength(13));
            Assert.AreEqual(8, SocketCanUtils.CanDlcToLength(14));
            Assert.AreEqual(8, SocketCanUtils.CanDlcToLength(15));
            Assert.Throws<ArgumentException>(() => SocketCanUtils.CanDlcToLength(16));
        }

        [Test]
        public void CanFdDlcToLength_Test()
        {
            Assert.AreEqual(0, SocketCanUtils.CanFdDlcToLength(0));
            Assert.AreEqual(1, SocketCanUtils.CanFdDlcToLength(1));
            Assert.AreEqual(2, SocketCanUtils.CanFdDlcToLength(2));
            Assert.AreEqual(3, SocketCanUtils.CanFdDlcToLength(3));
            Assert.AreEqual(4, SocketCanUtils.CanFdDlcToLength(4));
            Assert.AreEqual(5, SocketCanUtils.CanFdDlcToLength(5));
            Assert.AreEqual(6, SocketCanUtils.CanFdDlcToLength(6));
            Assert.AreEqual(7, SocketCanUtils.CanFdDlcToLength(7));
            Assert.AreEqual(8, SocketCanUtils.CanFdDlcToLength(8));
            Assert.AreEqual(12, SocketCanUtils.CanFdDlcToLength(9));
            Assert.AreEqual(16, SocketCanUtils.CanFdDlcToLength(10));
            Assert.AreEqual(20, SocketCanUtils.CanFdDlcToLength(11));
            Assert.AreEqual(24, SocketCanUtils.CanFdDlcToLength(12));
            Assert.AreEqual(32, SocketCanUtils.CanFdDlcToLength(13));
            Assert.AreEqual(48, SocketCanUtils.CanFdDlcToLength(14));
            Assert.AreEqual(64, SocketCanUtils.CanFdDlcToLength(15));
            Assert.Throws<ArgumentException>(() => SocketCanUtils.CanFdDlcToLength(16));
        }

        [Test]
        public void CanFdLengthToDlc_Test()
        {
            Assert.AreEqual(0, SocketCanUtils.CanFdLengthToDlc(0));
            Assert.AreEqual(1, SocketCanUtils.CanFdLengthToDlc(1));
            Assert.AreEqual(2, SocketCanUtils.CanFdLengthToDlc(2));
            Assert.AreEqual(3, SocketCanUtils.CanFdLengthToDlc(3));
            Assert.AreEqual(4, SocketCanUtils.CanFdLengthToDlc(4));
            Assert.AreEqual(5, SocketCanUtils.CanFdLengthToDlc(5));
            Assert.AreEqual(6, SocketCanUtils.CanFdLengthToDlc(6));
            Assert.AreEqual(7, SocketCanUtils.CanFdLengthToDlc(7));
            Assert.AreEqual(8, SocketCanUtils.CanFdLengthToDlc(8));
            Assert.AreEqual(9, SocketCanUtils.CanFdLengthToDlc(12));
            Assert.AreEqual(10, SocketCanUtils.CanFdLengthToDlc(16));
            Assert.AreEqual(11, SocketCanUtils.CanFdLengthToDlc(20));
            Assert.AreEqual(12, SocketCanUtils.CanFdLengthToDlc(24));
            Assert.AreEqual(13, SocketCanUtils.CanFdLengthToDlc(32));
            Assert.AreEqual(14, SocketCanUtils.CanFdLengthToDlc(48));
            Assert.AreEqual(15, SocketCanUtils.CanFdLengthToDlc(64));
            Assert.Throws<ArgumentOutOfRangeException>(() => SocketCanUtils.CanFdLengthToDlc(9));
        }

        [Test]
        public void ThrowIfCanIdStructureInvalid_Test()
        {
            Assert.DoesNotThrow(() => SocketCanUtils.ThrowIfCanIdStructureInvalid(0x7FF));
            Assert.DoesNotThrow(() => SocketCanUtils.ThrowIfCanIdStructureInvalid((uint)CanIdFlags.CAN_EFF_FLAG & 0x1FFFFFFF));
            Assert.DoesNotThrow(() => SocketCanUtils.ThrowIfCanIdStructureInvalid((uint)CanIdFlags.CAN_EFF_FLAG & 0x7FF));
            Assert.Throws<ArgumentException>(() => SocketCanUtils.ThrowIfCanIdStructureInvalid(0x1FFFFFFF));
        }

        [Test]
        public void ExtractRawCanId_Test()
        {
            Assert.AreEqual(0x15, SocketCanUtils.ExtractRawCanId(0x15));
            Assert.AreEqual(0x7FF, SocketCanUtils.ExtractRawCanId(0x7FF));
            Assert.AreEqual(0x25, SocketCanUtils.ExtractRawCanId(0x25 | (uint)CanIdFlags.CAN_RTR_FLAG));
            Assert.AreEqual(0x147, SocketCanUtils.ExtractRawCanId(0x147 | (uint)CanIdFlags.CAN_EFF_FLAG));
            Assert.AreEqual(0x18DB33F1, SocketCanUtils.ExtractRawCanId(0x18DB33F1 | (uint)CanIdFlags.CAN_EFF_FLAG));
            Assert.AreEqual(0x3F1, SocketCanUtils.ExtractRawCanId(0x18DB33F1));
        }

        [Test]
        public void CreateCanIdWithFlags_Test()
        {
            Assert.AreEqual(0x15, SocketCanUtils.CreateCanIdWithFlags(0x15, false, false, false));
            Assert.AreEqual(0x7FF | (uint)CanIdFlags.CAN_RTR_FLAG, SocketCanUtils.CreateCanIdWithFlags(0x7FF, false, true, false));
            Assert.AreEqual(0x25 | (uint)CanIdFlags.CAN_RTR_FLAG | (uint)CanIdFlags.CAN_EFF_FLAG , SocketCanUtils.CreateCanIdWithFlags(0x25, true, true, false));
            Assert.AreEqual(0x147 | (uint)CanIdFlags.CAN_ERR_FLAG, SocketCanUtils.CreateCanIdWithFlags(0x147, false, false, true));
            Assert.AreEqual(0x18DB33F1 | (uint)CanIdFlags.CAN_EFF_FLAG, SocketCanUtils.CreateCanIdWithFlags(0x18DB33F1, true, false, false));
            Assert.Throws<ArgumentException>(() => SocketCanUtils.CreateCanIdWithFlags(0x18DB33F1, false, false, false));
        }

        [Test]
        public void CreateCanIdWithFormatFlag_Test()
        {
            Assert.AreEqual(0x15, SocketCanUtils.CreateCanIdWithFormatFlag(0x15));
            Assert.AreEqual(0x7FF, SocketCanUtils.CreateCanIdWithFormatFlag(0x7FF));
            Assert.AreEqual(0x25 , SocketCanUtils.CreateCanIdWithFormatFlag(0x25));
            Assert.AreEqual(0x800 | (uint)CanIdFlags.CAN_EFF_FLAG, SocketCanUtils.CreateCanIdWithFormatFlag(0x800));
            Assert.AreEqual(0x7147 | (uint)CanIdFlags.CAN_EFF_FLAG, SocketCanUtils.CreateCanIdWithFormatFlag(0x7147));
            Assert.AreEqual(0x18DB33F1 | (uint)CanIdFlags.CAN_EFF_FLAG, SocketCanUtils.CreateCanIdWithFormatFlag(0x18DB33F1));
            Assert.Throws<ArgumentException>(() => SocketCanUtils.CreateCanIdWithFormatFlag(0x10 | (uint)CanIdFlags.CAN_EFF_FLAG));
            Assert.Throws<ArgumentException>(() => SocketCanUtils.CreateCanIdWithFormatFlag(0x10 | (uint)CanIdFlags.CAN_ERR_FLAG));
            Assert.Throws<ArgumentException>(() => SocketCanUtils.CreateCanIdWithFormatFlag(0x10 | (uint)CanIdFlags.CAN_RTR_FLAG));
        }

        [Test]
        public void GetCanXlPriorityId_Test()
        {
            Assert.AreEqual(0x7FF, SocketCanUtils.GetCanXlPriorityId(0xFF07FF));
            Assert.AreEqual(0x80, SocketCanUtils.GetCanXlPriorityId(0x660080));
            Assert.AreEqual(0x100, SocketCanUtils.GetCanXlPriorityId(0x7A0100));
            Assert.AreEqual(0x222, SocketCanUtils.GetCanXlPriorityId(0x4B0222));
            Assert.AreEqual(0x0, SocketCanUtils.GetCanXlPriorityId(0x000800));
        }

        [Test]
        public void GetCanXlVCID_Test()
        {
            Assert.AreEqual(0xFF, SocketCanUtils.GetCanXlVCID(0xFF07FF));
            Assert.AreEqual(0x66, SocketCanUtils.GetCanXlVCID(0x660080));
            Assert.AreEqual(0x7A, SocketCanUtils.GetCanXlVCID(0x7A0100));
            Assert.AreEqual(0x4B, SocketCanUtils.GetCanXlVCID(0x4B0222));
            Assert.AreEqual(0x0, SocketCanUtils.GetCanXlVCID(0x1000800));
        }

        [Test]
        public void SetCanXlPriorityId_Test()
        {
            Assert.AreEqual(0xFF07FF, SocketCanUtils.SetCanXlPriorityId(0xFF0000, 0x7FF));
            Assert.AreEqual(0x660080, SocketCanUtils.SetCanXlPriorityId(0x660000, 0x80));
            Assert.AreEqual(0x7A0100, SocketCanUtils.SetCanXlPriorityId(0x7A0000, 0x100));
            Assert.AreEqual(0x4B0222, SocketCanUtils.SetCanXlPriorityId(0x4B0000, 0x222));
            Assert.AreEqual(0x4B0222, SocketCanUtils.SetCanXlPriorityId(0x4B0F0F, 0x222));
            Assert.Throws<ArgumentException>(() => SocketCanUtils.SetCanXlPriorityId(0x0, 0x800));
        }

        [Test]
        public void SetCanXlVCID_Test()
        {
            Assert.AreEqual(0xFF07FF, SocketCanUtils.SetCanXlVCID(0x0007FF, 0xFF));
            Assert.AreEqual(0x660080, SocketCanUtils.SetCanXlVCID(0x000080, 0x66));
            Assert.AreEqual(0x7A0100, SocketCanUtils.SetCanXlVCID(0x000100, 0x7A));
            Assert.AreEqual(0x4B0222, SocketCanUtils.SetCanXlVCID(0x000222, 0x4B));
            Assert.AreEqual(0x4B0222, SocketCanUtils.SetCanXlVCID(0xEEE222, 0x4B));
            Assert.AreEqual(0x0, SocketCanUtils.SetCanXlVCID(0x1000800, 0x00));
        }

        [Test]
        public void GetAllInterfaces_Test()
        {
            IEnumerable<IfNameIndex> netInterfaces = SocketCanUtils.GetAllInterfaces();

            foreach(var netIf in netInterfaces)
            {
                Console.WriteLine(netIf);
            }

            Assert.GreaterOrEqual(netInterfaces.Count(), 4); // expecting at least vcan0, vcan1, vcan2, foobar
            Assert.IsNotNull(netInterfaces.FirstOrDefault(i => i.Name.Equals("vcan0")));
            Assert.IsNotNull(netInterfaces.FirstOrDefault(i => i.Name.Equals("vcan1")));
            Assert.IsNotNull(netInterfaces.FirstOrDefault(i => i.Name.Equals("vcan2")));
            Assert.IsNotNull(netInterfaces.FirstOrDefault(i => i.Name.Equals("foobar")));
        }
    }
}