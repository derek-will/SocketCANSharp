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
using System.Linq;
using System.Runtime.InteropServices;
using NUnit.Framework;
using SocketCANSharp;
using SocketCANSharp.Network.Netlink.Gateway;

namespace SocketCANSharpTest
{
    public class CgwStructTests
    {
        [Test]
        public void CAN_GW_RoutingCanMessage_Verification_Test()
        {
            var canRtMsg = new RoutingCanMessage();
            canRtMsg.CanFamily = SocketCanConstants.AF_CAN;
            canRtMsg.GatewayType = CanGatewayType.CGW_TYPE_CAN_CAN;
            canRtMsg.GatewayFlags = CanGatewayFlag.CGW_FLAGS_CAN_ECHO | CanGatewayFlag.CGW_FLAGS_CAN_SRC_TSTAMP;
            Assert.AreEqual((byte)SocketCanConstants.AF_CAN, canRtMsg.CanFamily);
            Assert.AreEqual(CanGatewayType.CGW_TYPE_CAN_CAN, canRtMsg.GatewayType);
            Assert.IsTrue(canRtMsg.GatewayFlags.HasFlag(CanGatewayFlag.CGW_FLAGS_CAN_ECHO));
            Assert.IsTrue(canRtMsg.GatewayFlags.HasFlag(CanGatewayFlag.CGW_FLAGS_CAN_SRC_TSTAMP));
            Assert.IsFalse(canRtMsg.GatewayFlags.HasFlag(CanGatewayFlag.CGW_FLAGS_CAN_FD));
            Assert.IsFalse(canRtMsg.GatewayFlags.HasFlag(CanGatewayFlag.CGW_FLAGS_CAN_IIF_TX_OK));
            Assert.AreEqual(4, Marshal.SizeOf<RoutingCanMessage>());
        }

        [Test]
        public void CAN_GW_RoutingCanMessage_ToString_Test()
        {
            var canRtMsg = new RoutingCanMessage();
            canRtMsg.CanFamily = SocketCanConstants.AF_CAN;
            canRtMsg.GatewayType = CanGatewayType.CGW_TYPE_CAN_CAN;
            canRtMsg.GatewayFlags = CanGatewayFlag.CGW_FLAGS_CAN_ECHO | CanGatewayFlag.CGW_FLAGS_CAN_SRC_TSTAMP;
            Assert.AreEqual((byte)SocketCanConstants.AF_CAN, canRtMsg.CanFamily);
            Assert.AreEqual(CanGatewayType.CGW_TYPE_CAN_CAN, canRtMsg.GatewayType);
            Assert.IsTrue(canRtMsg.GatewayFlags.HasFlag(CanGatewayFlag.CGW_FLAGS_CAN_ECHO));
            Assert.IsTrue(canRtMsg.GatewayFlags.HasFlag(CanGatewayFlag.CGW_FLAGS_CAN_SRC_TSTAMP));
            Assert.IsFalse(canRtMsg.GatewayFlags.HasFlag(CanGatewayFlag.CGW_FLAGS_CAN_FD));
            Assert.IsFalse(canRtMsg.GatewayFlags.HasFlag(CanGatewayFlag.CGW_FLAGS_CAN_IIF_TX_OK));;
            string str = canRtMsg.ToString();
            Assert.IsNotNull(str);
            Console.WriteLine(str);
        }

        [Test]
        public void CAN_GW_CanFdGatewayModifier_Verification_Test()
        {
            var data = new byte[] { 0xde, 0xad, 0xbe, 0xef, 0x00 };
            var frame = new CanFdFrame(0x123, data, CanFdFlags.CANFD_BRS);
            var fdMod = new CanFdGatewayModifier(CanGatewayModificationType.CGW_MOD_DATA, frame);
            Assert.AreEqual(CanGatewayModificationType.CGW_MOD_DATA, fdMod.ModificationTargets);
            Assert.AreEqual(0x123, fdMod.CanFdFrame.CanId);
            Assert.AreEqual(5, fdMod.CanFdFrame.Length);
            Assert.IsTrue(fdMod.CanFdFrame.Data.Take(fdMod.CanFdFrame.Length).SequenceEqual(data));
            Assert.AreEqual(CanFdFlags.CANFD_BRS, fdMod.CanFdFrame.Flags);
        }

        [Test]
        public void CAN_GW_ClassicalCanGatewayModifier_Verification_Test()
        {
            var data = new byte[] { 0xde, 0xad, 0xbe, 0xef, 0x00 };
            var frame = new CanFrame(0x123, data);
            var fdMod = new ClassicalCanGatewayModifier(CanGatewayModificationType.CGW_MOD_DATA, frame);
            Assert.AreEqual(CanGatewayModificationType.CGW_MOD_DATA, fdMod.ModificationTargets);
            Assert.AreEqual(0x123, fdMod.CanFrame.CanId);
            Assert.AreEqual(5, fdMod.CanFrame.Length);
            Assert.IsTrue(fdMod.CanFrame.Data.Take(fdMod.CanFrame.Length).SequenceEqual(data));
        }

        [Test]
        public void CAN_GW_CgwCanToCanRule_ClassicalCAN_Test()
        {
            var rule = new CgwCanToCanRule(CgwCanFrameType.ClassicalCAN);
            rule.HopLimit = 2;
            rule.MaintainSourceTimestamp = false;
            rule.ReceiveFilter = new CanFilter(0x123, 0x7FF);
            rule.OrModifier = new ClassicalCanGatewayModifier(CanGatewayModificationType.CGW_MOD_LEN, new CanFrame(0x123, new byte[] { 0x00, 0x01, 0x02 }));
            rule.AllowRoutingToSameInterface = false;
            rule.ChecksumXorConfiguration = new CgwChecksumXor(0, 1, 2, 0xFF);
            rule.DestinationIndex = 5;
            rule.SourceIndex = 6;
            rule.EnableLocalCanSocketLoopback = true;
            rule.UpdateIdentifier = 0xFFEEEEDD;
            Assert.AreEqual(false, rule.AllowRoutingToSameInterface);
            Assert.AreEqual(null, rule.AndModifier);
            Assert.AreEqual(true, rule.ChecksumXorConfiguration.HasValue);
            Assert.AreEqual(0, rule.ChecksumXorConfiguration.Value.FromIndex);
            Assert.AreEqual(1, rule.ChecksumXorConfiguration.Value.ToIndex);
            Assert.AreEqual(2, rule.ChecksumXorConfiguration.Value.ResultIndex);
            Assert.AreEqual(0xFF, rule.ChecksumXorConfiguration.Value.InitialXorValue);
            Assert.AreEqual(null, rule.Crc8Configuration);
            Assert.AreEqual(0, rule.DeletedFrames);
            Assert.AreEqual(5, rule.DestinationIndex);
            Assert.AreEqual(0, rule.DroppedFrames);
            Assert.AreEqual(true, rule.EnableLocalCanSocketLoopback);
            Assert.AreEqual(CanGatewayType.CGW_TYPE_CAN_CAN, rule.GatewayType);
            Assert.AreEqual(0, rule.HandledFrames);
            Assert.AreEqual(2, rule.HopLimit);
            Assert.AreEqual(false, rule.IsCanFdRule);
            Assert.AreEqual(false, rule.MaintainSourceTimestamp);
            Assert.NotNull(rule.OrModifier);
            Assert.AreEqual(CanGatewayModificationType.CGW_MOD_LEN, rule.OrModifier.ModificationTargets);
            Assert.AreEqual(0x123, ((ClassicalCanGatewayModifier)rule.OrModifier).CanFrame.CanId);
            Assert.AreEqual(3, ((ClassicalCanGatewayModifier)rule.OrModifier).CanFrame.Length);
            Assert.IsTrue(((ClassicalCanGatewayModifier)rule.OrModifier).CanFrame.Data.Take(3).SequenceEqual(new byte[] { 0x00, 0x01, 0x02 }));
            Assert.AreEqual(true, rule.ReceiveFilter.HasValue);
            Assert.AreEqual(0x123, rule.ReceiveFilter.Value.CanId);
            Assert.AreEqual(0x7FF, rule.ReceiveFilter.Value.CanMask);
            Assert.AreEqual(null, rule.SetModifier);
            Assert.AreEqual(6, rule.SourceIndex);
            Assert.AreEqual(0xFFEEEEDD, rule.UpdateIdentifier);
            Assert.AreEqual(null, rule.XorModifier);
        }

        [Test]
        public void CAN_GW_CgwCanToCanRule_CANFD_Test()
        {
            var rule = new CgwCanToCanRule(CgwCanFrameType.CANFD);
            rule.HopLimit = 2;
            rule.MaintainSourceTimestamp = false;
            rule.ReceiveFilter = new CanFilter(0x123, 0x7FF);
            rule.OrModifier = new CanFdGatewayModifier(CanGatewayModificationType.CGW_MOD_LEN, new CanFdFrame(0x123, new byte[] { 0x00, 0x01, 0x02 }, CanFdFlags.CANFD_BRS));
            rule.AllowRoutingToSameInterface = false;
            rule.ChecksumXorConfiguration = new CgwChecksumXor(0, 1, 2, 0xFF);
            rule.DestinationIndex = 5;
            rule.SourceIndex = 6;
            rule.EnableLocalCanSocketLoopback = true;
            rule.UpdateIdentifier = 0xFFEEEEDD;
            Assert.AreEqual(false, rule.AllowRoutingToSameInterface);
            Assert.AreEqual(null, rule.AndModifier);
            Assert.AreEqual(true, rule.ChecksumXorConfiguration.HasValue);
            Assert.AreEqual(0, rule.ChecksumXorConfiguration.Value.FromIndex);
            Assert.AreEqual(1, rule.ChecksumXorConfiguration.Value.ToIndex);
            Assert.AreEqual(2, rule.ChecksumXorConfiguration.Value.ResultIndex);
            Assert.AreEqual(0xFF, rule.ChecksumXorConfiguration.Value.InitialXorValue);
            Assert.AreEqual(null, rule.Crc8Configuration);
            Assert.AreEqual(0, rule.DeletedFrames);
            Assert.AreEqual(5, rule.DestinationIndex);
            Assert.AreEqual(0, rule.DroppedFrames);
            Assert.AreEqual(true, rule.EnableLocalCanSocketLoopback);
            Assert.AreEqual(CanGatewayType.CGW_TYPE_CAN_CAN, rule.GatewayType);
            Assert.AreEqual(0, rule.HandledFrames);
            Assert.AreEqual(2, rule.HopLimit);
            Assert.AreEqual(true, rule.IsCanFdRule);
            Assert.AreEqual(false, rule.MaintainSourceTimestamp);
            Assert.NotNull(rule.OrModifier);
            Assert.AreEqual(CanGatewayModificationType.CGW_MOD_LEN, rule.OrModifier.ModificationTargets);
            Assert.AreEqual(0x123, ((CanFdGatewayModifier)rule.OrModifier).CanFdFrame.CanId);
            Assert.AreEqual(CanFdFlags.CANFD_BRS, ((CanFdGatewayModifier)rule.OrModifier).CanFdFrame.Flags);
            Assert.AreEqual(3, ((CanFdGatewayModifier)rule.OrModifier).CanFdFrame.Length);
            Assert.IsTrue(((CanFdGatewayModifier)rule.OrModifier).CanFdFrame.Data.Take(3).SequenceEqual(new byte[] { 0x00, 0x01, 0x02 }));
            Assert.AreEqual(true, rule.ReceiveFilter.HasValue);
            Assert.AreEqual(0x123, rule.ReceiveFilter.Value.CanId);
            Assert.AreEqual(0x7FF, rule.ReceiveFilter.Value.CanMask);
            Assert.AreEqual(null, rule.SetModifier);
            Assert.AreEqual(6, rule.SourceIndex);
            Assert.AreEqual(0xFFEEEEDD, rule.UpdateIdentifier);
            Assert.AreEqual(null, rule.XorModifier);
        }

        [Test]
        public void CAN_GW_CgwCanToCanRule_CANFD_AssignClassicalCanModifier_Failure_Test()
        {
            var rule = new CgwCanToCanRule(CgwCanFrameType.CANFD);
            Assert.Throws<InvalidOperationException>(() => rule.OrModifier = new ClassicalCanGatewayModifier(CanGatewayModificationType.CGW_MOD_LEN, new CanFrame(0x123, new byte[] { 0x00, 0x01, 0x02 })));
        }

        [Test]
        public void CAN_GW_CgwCanToCanRule_ClassicalCAN_AssignCanFdModifier_Failure_Test()
        {
            var rule = new CgwCanToCanRule(CgwCanFrameType.ClassicalCAN);
            Assert.Throws<InvalidOperationException>(() => rule.OrModifier = new CanFdGatewayModifier(CanGatewayModificationType.CGW_MOD_LEN, new CanFdFrame(0x123, new byte[] { 0x00, 0x01, 0x02 }, CanFdFlags.CANFD_BRS)));
        }
    }
}