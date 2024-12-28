#region License
/* 
BSD 3-Clause License

Copyright (c) 2024, Derek Will
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
using System.Linq;
using NUnit.Framework;
using SocketCANSharp.Network.Netlink;

namespace SocketCANSharpTest
{
    public class NetlinkRoutingAttributeTests
    {
        [Test]
        public void RoutingAttributeWithData_Attribute_Test()
        {
            var rtAttr = new RoutingAttributeWithData(new RoutingAttribute(28, (ushort)LinkInfoAttributeType.IFLA_INFO_DATA));
            Assert.AreEqual(NetlinkMessageMacros.RTA_LENGTH(28), rtAttr.Attribute.Length);
            Assert.AreEqual((ushort)LinkInfoAttributeType.IFLA_INFO_DATA, rtAttr.Attribute.Type);
            Assert.IsTrue(new byte[] { }.SequenceEqual(rtAttr.Data));
            Assert.IsTrue(new byte[] { 0x20, 0x00, 0x02, 0x00 }.SequenceEqual(rtAttr.ToBytes()));
        }

        [Test]
        public void RoutingAttributeWithData_string_Test()
        {
            var rtAttr = new RoutingAttributeWithData((ushort)LinkInfoAttributeType.IFLA_INFO_KIND, "can");
            Assert.AreEqual(7, rtAttr.Attribute.Length);
            Assert.AreEqual((ushort)LinkInfoAttributeType.IFLA_INFO_KIND, rtAttr.Attribute.Type);
            Assert.IsTrue(new byte[] { 0x63, 0x61, 0x6e }.SequenceEqual(rtAttr.Data));
            Assert.IsTrue(new byte[] { 0x07, 0x00, 0x01, 0x00, 0x63, 0x61, 0x6e }.SequenceEqual(rtAttr.ToBytes()));
        }

        [Test]
        public void RoutingAttributeWithData_uint_Test()
        {
            var rtAttr = new RoutingAttributeWithData((ushort)CanRoutingAttributeType.IFLA_CAN_RESTART_MS, (uint)0x000000ff);
            Assert.AreEqual(8, rtAttr.Attribute.Length);
            Assert.AreEqual((ushort)CanRoutingAttributeType.IFLA_CAN_RESTART_MS, rtAttr.Attribute.Type);
            Assert.IsTrue(new byte[] { 0xff, 0x00, 0x00, 0x00 }.SequenceEqual(rtAttr.Data));
            Assert.IsTrue(new byte[] { 0x08, 0x00, 0x06, 0x00, 0xff, 0x00, 0x00, 0x00 }.SequenceEqual(rtAttr.ToBytes()));
        }

        [Test]
        public void RoutingAttributeWithData_CanBitTiming_Test()
        {
            var cbt = new CanBitTiming()
            {
                BitRate = 500000,
                SamplePoint = 875,
                TimeQuanta = 125,
                PropagationSegment = 6,
                PhaseBufferSegment1 = 7,
                PhaseBufferSegment2 = 2,
                SyncJumpWidth = 1,
                BitRatePrescaler = 4,
            };
            var rtAttr = new RoutingAttributeWithData((ushort)CanRoutingAttributeType.IFLA_CAN_BITTIMING, cbt);
            Assert.AreEqual(36, rtAttr.Attribute.Length);
            Assert.AreEqual((ushort)CanRoutingAttributeType.IFLA_CAN_BITTIMING, rtAttr.Attribute.Type);
            Assert.IsTrue(new byte[] { 0x20, 0xA1, 0x07, 0x00, 0x6B, 0x03, 0x00, 0x00, 0x7D, 0x00, 0x00, 0x00, 
                0x06, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 
                0x04, 0x00, 0x00, 0x00 }.SequenceEqual(rtAttr.Data));
            Assert.IsTrue(new byte[] { 0x24, 0x00, 0x01, 0x00, 0x20, 0xA1, 0x07, 0x00, 0x6B, 0x03, 0x00, 0x00, 0x7D, 0x00, 0x00, 0x00, 
                0x06, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 
                0x04, 0x00, 0x00, 0x00 }.SequenceEqual(rtAttr.ToBytes()));
        }

        [Test]
        public void RoutingAttributeWithData_CanControllerMode_Test()
        {
            var ccm = new CanControllerMode()
            {
                Mask = 0,
                Flags = (CanControllerModeFlags)0,
            };
            var rtAttr = new RoutingAttributeWithData((ushort)CanRoutingAttributeType.IFLA_CAN_CTRLMODE, ccm);
            Assert.AreEqual(12, rtAttr.Attribute.Length);
            Assert.AreEqual((ushort)CanRoutingAttributeType.IFLA_CAN_CTRLMODE, rtAttr.Attribute.Type);
            Assert.IsTrue(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }.SequenceEqual(rtAttr.Data));
            Assert.IsTrue(new byte[] { 0x0C, 0x00, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }.SequenceEqual(rtAttr.ToBytes()));
        }

        [Test]
        public void RoutingAttribute_Ctor_Test()
        {
            var rtAttr = new RoutingAttribute(28, (ushort)LinkInfoAttributeType.IFLA_INFO_DATA);
            Assert.AreEqual(NetlinkMessageMacros.RTA_LENGTH(28), rtAttr.Length);
            Assert.AreEqual((ushort)LinkInfoAttributeType.IFLA_INFO_DATA, rtAttr.Type);;
        }
    }
}