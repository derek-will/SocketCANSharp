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
using System.Linq;
using System.Runtime.InteropServices;

namespace SocketCANSharpTest
{
    public class StructTests
    {
        [Test]
        public void Timeval_Verification_Test()
        {
            var timeval = new Timeval(2, 100000);
            Assert.AreEqual(2, timeval.Seconds);
            Assert.AreEqual(100000, timeval.Microseconds);
            Assert.AreEqual(Environment.Is64BitProcess ? 16 : 8, Marshal.SizeOf<Timeval>(timeval));
        }

        [Test]
        public void Timeval_ToString_Test()
        {
            var timeval = new Timeval(0, 500000);
            Assert.AreEqual(0, timeval.Seconds);
            Assert.AreEqual(500000, timeval.Microseconds);
            string str = timeval.ToString();
            Assert.IsNotNull(str);
            Console.WriteLine(str);
        }

        [Test]
        public void BcmTimeval_Verification_Test()
        {
            var timeval = new BcmTimeval(2, 100000);
            Assert.AreEqual(2, timeval.Seconds);
            Assert.AreEqual(100000, timeval.Microseconds);
            Assert.AreEqual(Environment.Is64BitProcess ? 16 : 8, Marshal.SizeOf<BcmTimeval>(timeval));
        }

        [Test]
        public void BcmTimeval_ToString_Test()
        {
            var timeval = new BcmTimeval(3, 200000);
            Assert.AreEqual(3, timeval.Seconds);
            Assert.AreEqual(200000, timeval.Microseconds);
            string str = timeval.ToString();
            Assert.IsNotNull(str);
            Console.WriteLine(str);
        }

        [Test]
        public void CanFdFrame_Verification_Test()
        {
            var candFdFrame = new CanFdFrame((uint)CanIdFlags.CAN_EFF_FLAG | 0x1FEDCBA9, new byte[] { 0x01, 0x02, 0x03, 0x04 }, CanFdFlags.CANFD_BRS);
            Assert.AreEqual((uint)CanIdFlags.CAN_EFF_FLAG | 0x1FEDCBA9, candFdFrame.CanId);
            Assert.AreEqual(4, candFdFrame.Length);
            Assert.AreEqual(CanFdFlags.CANFD_BRS, candFdFrame.Flags);
            Assert.IsTrue(candFdFrame.Data.Take(candFdFrame.Length).SequenceEqual(new byte[] { 0x01, 0x02, 0x03, 0x04 }));
            Assert.AreEqual(72, Marshal.SizeOf<CanFdFrame>());
        }

        [Test]
        public void CanFdFrame_ToString_Test()
        {
            var candFdFrame = new CanFdFrame((uint)CanIdFlags.CAN_EFF_FLAG | 0x1FEDCBA9, new byte[] { 0x01, 0x02, 0x03, 0x04 }, CanFdFlags.CANFD_BRS);
            Assert.AreEqual((uint)CanIdFlags.CAN_EFF_FLAG | 0x1FEDCBA9, candFdFrame.CanId);
            Assert.AreEqual(4, candFdFrame.Length);
            Assert.AreEqual(CanFdFlags.CANFD_BRS, candFdFrame.Flags);
            Assert.IsTrue(candFdFrame.Data.Take(candFdFrame.Length).SequenceEqual(new byte[] { 0x01, 0x02, 0x03, 0x04 }));
            string str = candFdFrame.ToString();
            Assert.IsNotNull(str);
            Console.WriteLine(str);
        }

        [Test]
        public void CanFilter_Verification_Test()
        {
            var canFilter = new CanFilter((uint)CanIdFlags.CAN_EFF_FLAG | 0x18DAF100, (uint)CanIdFlags.CAN_EFF_FLAG | 0x1FFFFF00);
            Assert.AreEqual((uint)CanIdFlags.CAN_EFF_FLAG | 0x18DAF100, canFilter.CanId);
            Assert.AreEqual((uint)CanIdFlags.CAN_EFF_FLAG | 0x1FFFFF00, canFilter.CanMask);
            Assert.AreEqual(8, Marshal.SizeOf<CanFilter>());
        }

        [Test]
        public void CanFilter_ToString_Test()
        {
            var canFilter = new CanFilter((uint)CanIdFlags.CAN_EFF_FLAG | 0x18DAF100, (uint)CanIdFlags.CAN_EFF_FLAG | 0x1FFFFF00);
            Assert.AreEqual((uint)CanIdFlags.CAN_EFF_FLAG | 0x18DAF100, canFilter.CanId);
            Assert.AreEqual((uint)CanIdFlags.CAN_EFF_FLAG | 0x1FFFFF00, canFilter.CanMask);
            string str = canFilter.ToString();
            Assert.IsNotNull(str);
            Console.WriteLine(str);
        }

        [Test]
        public void CanFrame_Verification_Test()
        {
            var candFrame = new CanFrame((uint)CanIdFlags.CAN_EFF_FLAG | 0x1FEDCB44, new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 });
            Assert.AreEqual((uint)CanIdFlags.CAN_EFF_FLAG | 0x1FEDCB44, candFrame.CanId);
            Assert.AreEqual(7, candFrame.Length);
            Assert.IsTrue(candFrame.Data.Take(candFrame.Length).SequenceEqual(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 }));
            Assert.AreEqual(16, Marshal.SizeOf<CanFrame>());
        }

        [Test]
        public void CanFrame_ToString_Test()
        {
            var canFrame = new CanFrame((uint)CanIdFlags.CAN_EFF_FLAG | 0x1FEDCB44, new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 });
            Assert.AreEqual((uint)CanIdFlags.CAN_EFF_FLAG | 0x1FEDCB44, canFrame.CanId);
            Assert.AreEqual(7, canFrame.Length);
            Assert.IsTrue(canFrame.Data.Take(canFrame.Length).SequenceEqual(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 }));
            string str = canFrame.ToString();
            Assert.IsNotNull(str);
            Console.WriteLine(str);
        }

        [Test]
        public void CanIsoTpFlowControlOptions_Verification_Test()
        {
            var fcOptions = new CanIsoTpFlowControlOptions(10, 8, 3);
            Assert.AreEqual(10, fcOptions.BlockSize);
            Assert.AreEqual(8, fcOptions.Stmin);
            Assert.AreEqual(3, fcOptions.WftMax);
            Assert.AreEqual(3, Marshal.SizeOf<CanIsoTpFlowControlOptions>()); // class size
        }

        [Test]
        public void CanIsoTpFlowControlOptions_ToString_Test()
        {
            var fcOptions = new CanIsoTpFlowControlOptions(10, 8, 3);
            Assert.AreEqual(10, fcOptions.BlockSize);
            Assert.AreEqual(8, fcOptions.Stmin);
            Assert.AreEqual(3, fcOptions.WftMax);
            string str = fcOptions.ToString();
            Assert.IsNotNull(str);
            Console.WriteLine(str);
        }

        [Test]
        public void CanIsoTpLinkLayerOptions_Verification_Test()
        {
            var llOptions = new CanIsoTpLinkLayerOptions(16, 8, CanFdFlags.None);
            Assert.AreEqual(16, llOptions.Mtu);
            Assert.AreEqual(8, llOptions.TxDataLength);
            Assert.AreEqual(CanFdFlags.None, llOptions.TxFlags);
            Assert.AreEqual(3, Marshal.SizeOf<CanIsoTpLinkLayerOptions>()); // class size
        }

        [Test]
        public void CanIsoTpLinkLayerOptions_ToString_Test()
        {
            var llOptions = new CanIsoTpLinkLayerOptions(72, 64, CanFdFlags.CANFD_BRS);
            Assert.AreEqual(72, llOptions.Mtu);
            Assert.AreEqual(64, llOptions.TxDataLength);
            Assert.AreEqual(CanFdFlags.CANFD_BRS, llOptions.TxFlags);
            string str = llOptions.ToString();
            Assert.IsNotNull(str);
            Console.WriteLine(str);
        }

        [Test]
        public void CanIsoTpOptions_Verification_Test()
        {
            var options = new CanIsoTpOptions()
            {
                Flags = IsoTpFlags.CAN_ISOTP_TX_PADDING | IsoTpFlags.CAN_ISOTP_RX_PADDING,
                FrameTxTime = 150,
                TxPadByte = 0xDD,
                RxPadByte = 0xBB,
                RxExtendedAddress = 0x0E,
                ExtendedAddress = 0x0F,
            };
            Assert.AreEqual(IsoTpFlags.CAN_ISOTP_TX_PADDING | IsoTpFlags.CAN_ISOTP_RX_PADDING, options.Flags);
            Assert.AreEqual(150, options.FrameTxTime);
            Assert.AreEqual(0xDD, options.TxPadByte);
            Assert.AreEqual(0xBB, options.RxPadByte);
            Assert.AreEqual(0x0E, options.RxExtendedAddress);
            Assert.AreEqual(0x0F, options.ExtendedAddress);
            Assert.AreEqual(12, Marshal.SizeOf<CanIsoTpOptions>()); // class size
        }

        [Test]
        public void CanIsoTpOptions_ToString_Test()
        {
            var options = new CanIsoTpOptions()
            {
                Flags = IsoTpFlags.CAN_ISOTP_TX_PADDING | IsoTpFlags.CAN_ISOTP_RX_PADDING,
                FrameTxTime = 150,
                TxPadByte = 0xDD,
                RxPadByte = 0xBB,
                RxExtendedAddress = 0x0E,
                ExtendedAddress = 0x0F,
            };
            Assert.AreEqual(IsoTpFlags.CAN_ISOTP_TX_PADDING | IsoTpFlags.CAN_ISOTP_RX_PADDING, options.Flags);
            Assert.AreEqual(150, options.FrameTxTime);
            Assert.AreEqual(0xDD, options.TxPadByte);
            Assert.AreEqual(0xBB, options.RxPadByte);
            Assert.AreEqual(0x0E, options.RxExtendedAddress);
            Assert.AreEqual(0x0F, options.ExtendedAddress);
            string str = options.ToString();
            Assert.IsNotNull(str);
            Console.WriteLine(str);
        }

        [Test]
        public void EpollData_Verification_Test()
        {
            var epollData = new EpollData()
            {
                Unsigned32BitNumber = ushort.MaxValue,
            };
            Assert.AreEqual(ushort.MaxValue, epollData.Unsigned32BitNumber);
            Assert.AreEqual(ushort.MaxValue, epollData.Unsigned64BitNumber);
            Assert.AreEqual(ushort.MaxValue, epollData.FileDescriptor);
            Assert.AreEqual(new IntPtr(ushort.MaxValue), epollData.Pointer);
            Assert.AreEqual(8, Marshal.SizeOf<EpollData>());
        }

        [Test]
        public void EpollData_ToString_Test()
        {
            var epollData = new EpollData()
            {
                Unsigned32BitNumber = ushort.MaxValue,
            };
            Assert.AreEqual(ushort.MaxValue, epollData.Unsigned32BitNumber);
            Assert.AreEqual(ushort.MaxValue, epollData.Unsigned64BitNumber);
            Assert.AreEqual(ushort.MaxValue, epollData.FileDescriptor);
            Assert.AreEqual(new IntPtr(ushort.MaxValue), epollData.Pointer);
            string str = epollData.ToString();
            Assert.IsNotNull(str);
            Console.WriteLine(str);
        }

        [Test]
        public void EpollEvent_Verification_Test()
        {
            var epollEvent = new EpollEvent()
            {
                Events = EpollEventType.EPOLLIN | EpollEventType.EPOLLPRI,
                Data = new EpollData()
                {
                    Unsigned32BitNumber = ushort.MaxValue,
                },
            };
            Assert.AreEqual(EpollEventType.EPOLLIN | EpollEventType.EPOLLPRI, epollEvent.Events);
            Assert.AreEqual(ushort.MaxValue, epollEvent.Data.Unsigned32BitNumber);
            Assert.AreEqual(ushort.MaxValue, epollEvent.Data.Unsigned64BitNumber);
            Assert.AreEqual(ushort.MaxValue, epollEvent.Data.FileDescriptor);
            Assert.AreEqual(new IntPtr(ushort.MaxValue), epollEvent.Data.Pointer);
            Assert.AreEqual(16, Marshal.SizeOf<EpollEvent>());
        }

        [Test]
        public void EpollEvent_ToString_Test()
        {
            var epollEvent = new EpollEvent()
            {
                Events = EpollEventType.EPOLLIN | EpollEventType.EPOLLPRI,
                Data = new EpollData()
                {
                    Unsigned32BitNumber = ushort.MaxValue,
                },
            };
            Assert.AreEqual(EpollEventType.EPOLLIN | EpollEventType.EPOLLPRI, epollEvent.Events);
            Assert.AreEqual(ushort.MaxValue, epollEvent.Data.Unsigned32BitNumber);
            Assert.AreEqual(ushort.MaxValue, epollEvent.Data.Unsigned64BitNumber);
            Assert.AreEqual(ushort.MaxValue, epollEvent.Data.FileDescriptor);
            Assert.AreEqual(new IntPtr(ushort.MaxValue), epollEvent.Data.Pointer);
            string str = epollEvent.ToString();
            Assert.IsNotNull(str);
            Console.WriteLine(str);
        }

        [Test]
        public void Ifreq_Verification_Test()
        {
            var ifreq = new Ifreq("vcan0");
            Assert.AreEqual("vcan0", ifreq.Name);
            Assert.AreEqual(0, ifreq.IfIndex);
            Assert.AreEqual(20, Marshal.SizeOf<Ifreq>()); // class size
        }

        [Test]
        public void Ifreq_ToString_Test()
        {
            var ifreq = new Ifreq("vcan0");
            Assert.AreEqual("vcan0", ifreq.Name);
            Assert.AreEqual(0, ifreq.IfIndex);
            string str = ifreq.ToString();
            Assert.IsNotNull(str);
            Console.WriteLine(str);
        }

        [Test]
        public void IfreqMtu_Verification_Test()
        {
            var ifreq = new IfreqMtu("vcan0");
            Assert.AreEqual("vcan0", ifreq.Name);
            Assert.AreEqual(0, ifreq.MTU);
            Assert.AreEqual(20, Marshal.SizeOf<Ifreq>()); // class size
        }

        [Test]
        public void IfreqMtu_ToString_Test()
        {
            var ifreq = new IfreqMtu("vcan0");
            Assert.AreEqual("vcan0", ifreq.Name);
            Assert.AreEqual(0, ifreq.MTU);
            string str = ifreq.ToString();
            Assert.IsNotNull(str);
            Console.WriteLine(str);
        }

        [Test]
        public void IoVector_Verification_Test()
        {
            var iov = new IoVector()
            {
                Base = new IntPtr(int.MaxValue - short.MaxValue),
                Length = new IntPtr(123456),
            };
            Assert.AreEqual(new IntPtr(int.MaxValue - short.MaxValue), iov.Base);
            Assert.AreEqual(new IntPtr(123456), iov.Length);
            Assert.AreEqual(Environment.Is64BitProcess ? 16 : 8, Marshal.SizeOf<IoVector>());
        }

        [Test]
        public void IoVector_ToString_Test()
        {
            var iov = new IoVector()
            {
                Base = new IntPtr(int.MaxValue - short.MaxValue),
                Length = new IntPtr(123456),
            };
            Assert.AreEqual(new IntPtr(int.MaxValue - short.MaxValue), iov.Base);
            Assert.AreEqual(new IntPtr(123456), iov.Length);
            string str = iov.ToString();
            Assert.IsNotNull(str);
            Console.WriteLine(str);
        }

        [Test]
        public void J1939Filter_Verification_Test()
        {
            var j1939Filter = new J1939Filter()
            {
                Name = 0xFFEEDDCCBBAA9988,
                NameMask = 0xFF00FF00FF00FF00,
                PGN = 0x0F004,
                PGNMask = 0xFFFF,
                Address = 0xF1,
                AddressMask = 0xF1,
            };
            Assert.AreEqual(0xFFEEDDCCBBAA9988, j1939Filter.Name);
            Assert.AreEqual(0xFF00FF00FF00FF00, j1939Filter.NameMask);
            Assert.AreEqual(0x0F004, j1939Filter.PGN);
            Assert.AreEqual(0xFFFF, j1939Filter.PGNMask);
            Assert.AreEqual(0xF1, j1939Filter.Address);
            Assert.AreEqual(0xF1, j1939Filter.AddressMask);
            Assert.AreEqual(32, Marshal.SizeOf<J1939Filter>());
        }

        [Test]
        public void J1939Filter_ToString_Test()
        {
            var j1939Filter = new J1939Filter()
            {
                Name = 0xFFEEDDCCBBAA9988,
                NameMask = 0xFF00FF00FF00FF00,
                PGN = 0x0F004,
                PGNMask = 0xFFFF,
                Address = 0xF1,
                AddressMask = 0xF1,
            };
            Assert.AreEqual(0xFFEEDDCCBBAA9988, j1939Filter.Name);
            Assert.AreEqual(0xFF00FF00FF00FF00, j1939Filter.NameMask);
            Assert.AreEqual(0x0F004, j1939Filter.PGN);
            Assert.AreEqual(0xFFFF, j1939Filter.PGNMask);
            Assert.AreEqual(0xF1, j1939Filter.Address);
            Assert.AreEqual(0xF1, j1939Filter.AddressMask);
            string str = j1939Filter.ToString();
            Assert.IsNotNull(str);
            Console.WriteLine(str);
        }

        [Test]
        public void MessageHeader_Verification_Test()
        {
            var msgHeader = new MessageHeader()
            {
                Name = new IntPtr(7777),
                NameLength = IntPtr.Size,
                IoVectors = new IntPtr(9999),
                IoVectorCount = new IntPtr(2),
                ControlMessage = new IntPtr(8888),
                ControlMessageLength = new IntPtr(3),
                Flags = MessageFlags.MSG_PEEK | MessageFlags.MSG_TRUNC,
            };
            Assert.AreEqual(new IntPtr(7777), msgHeader.Name);
            Assert.AreEqual(IntPtr.Size, msgHeader.NameLength);
            Assert.AreEqual(new IntPtr(9999), msgHeader.IoVectors);
            Assert.AreEqual(new IntPtr(2), msgHeader.IoVectorCount);
            Assert.AreEqual(new IntPtr(8888), msgHeader.ControlMessage);
            Assert.AreEqual(new IntPtr(3), msgHeader.ControlMessageLength);
            Assert.AreEqual(MessageFlags.MSG_PEEK | MessageFlags.MSG_TRUNC, msgHeader.Flags);
            Assert.AreEqual(Environment.Is64BitProcess ? 56 : 28, Marshal.SizeOf<MessageHeader>());
        }

        [Test]
        public void MessageHeader_ToString_Test()
        {
            var msgHeader = new MessageHeader()
            {
                Name = new IntPtr(7777),
                NameLength = IntPtr.Size,
                IoVectors = new IntPtr(9999),
                IoVectorCount = new IntPtr(2),
                ControlMessage = new IntPtr(8888),
                ControlMessageLength = new IntPtr(3),
                Flags = MessageFlags.MSG_PEEK | MessageFlags.MSG_TRUNC,
            };
            Assert.AreEqual(new IntPtr(7777), msgHeader.Name);
            Assert.AreEqual(IntPtr.Size, msgHeader.NameLength);
            Assert.AreEqual(new IntPtr(9999), msgHeader.IoVectors);
            Assert.AreEqual(new IntPtr(2), msgHeader.IoVectorCount);
            Assert.AreEqual(new IntPtr(8888), msgHeader.ControlMessage);
            Assert.AreEqual(new IntPtr(3), msgHeader.ControlMessageLength);
            Assert.AreEqual(MessageFlags.MSG_PEEK | MessageFlags.MSG_TRUNC, msgHeader.Flags);
            string str = msgHeader.ToString();
            Assert.IsNotNull(str);
            Console.WriteLine(str);
        }

        [Test]
        public void SockAddrCan_Verification_Test()
        {
            var addr = new SockAddrCan(15);
            Assert.AreEqual(SocketCanConstants.AF_CAN, addr.CanFamily);
            Assert.AreEqual(15, addr.CanIfIndex);
            Assert.AreEqual(16, Marshal.SizeOf<SockAddrCan>());
        }

        [Test]
        public void SockAddrCan_ToString_Test()
        {
            var addr = new SockAddrCan(15);
            Assert.AreEqual(SocketCanConstants.AF_CAN, addr.CanFamily);
            Assert.AreEqual(15, addr.CanIfIndex);
            string str = addr.ToString();
            Assert.IsNotNull(str);
            Console.WriteLine(str);
        }

        [Test]
        public void SockAddrCanIsoTp_Verification_Test()
        {
            var addr = new SockAddrCanIsoTp(60)
            {
                RxId = 0x7e8,
                TxId = 0x7e0,
            };
            Assert.AreEqual(SocketCanConstants.AF_CAN, addr.CanFamily);
            Assert.AreEqual(60, addr.CanIfIndex);
            Assert.AreEqual(0x7e8, addr.RxId);
            Assert.AreEqual(0x7e0, addr.TxId);
            Assert.AreEqual(16, Marshal.SizeOf<SockAddrCanIsoTp>());
        }

        [Test]
        public void SockAddrCanIsoTp_ToString_Test()
        {
            var addr = new SockAddrCanIsoTp(60)
            {
                RxId = 0x7e8,
                TxId = 0x7e0,
            };
            Assert.AreEqual(SocketCanConstants.AF_CAN, addr.CanFamily);
            Assert.AreEqual(60, addr.CanIfIndex);
            Assert.AreEqual(0x7e8, addr.RxId);
            Assert.AreEqual(0x7e0, addr.TxId);
            string str = addr.ToString();
            Assert.IsNotNull(str);
            Console.WriteLine(str);
        }

        [Test]
        public void SockAddrCanJ1939_Verification_Test()
        {
            var addr = new SockAddrCanJ1939(60)
            {
                Name = 0x1122334455667788,
                PGN = 0x9999,
                Address = 0xAA
            };
            Assert.AreEqual(SocketCanConstants.AF_CAN, addr.CanFamily);
            Assert.AreEqual(60, addr.CanIfIndex);
            Assert.AreEqual(0x1122334455667788, addr.Name);
            Assert.AreEqual(0x9999, addr.PGN);
            Assert.AreEqual(0xAA, addr.Address);
            Assert.AreEqual(24, Marshal.SizeOf<SockAddrCanJ1939>());
        }

        [Test]
        public void SockAddrCanJ1939_ToString_Test()
        {
            var addr = new SockAddrCanJ1939(60)
            {
                Name = 0x1122334455667788,
                PGN = 0x9999,
                Address = 0xAA
            };
            Assert.AreEqual(SocketCanConstants.AF_CAN, addr.CanFamily);
            Assert.AreEqual(60, addr.CanIfIndex);
            Assert.AreEqual(0x1122334455667788, addr.Name);
            Assert.AreEqual(0x9999, addr.PGN);
            Assert.AreEqual(0xAA, addr.Address);
            string str = addr.ToString();
            Assert.IsNotNull(str);
            Console.WriteLine(str);
        }

        [Test]
        public void BcmMessageHeader_Verification_Test()
        {
            var header = new BcmMessageHeader()
            {
                Opcode = BcmOpcode.TX_SETUP,
                NumberOfFrames = 1,
                Flags = BcmFlags.TX_ANNOUNCE | BcmFlags.TX_COUNTEVT,
                Interval1Count = 25,
                Interval1 = new BcmTimeval(2, 600000),
                Interval2 = new BcmTimeval(3, 250000),
                CanId = 0x1F,    
            };
            Assert.AreEqual(BcmOpcode.TX_SETUP, header.Opcode);
            Assert.AreEqual(1, header.NumberOfFrames);
            Assert.AreEqual(BcmFlags.TX_ANNOUNCE | BcmFlags.TX_COUNTEVT, header.Flags);
            Assert.AreEqual(25, header.Interval1Count);
            Assert.AreEqual(2, header.Interval1.Seconds);
            Assert.AreEqual(600000, header.Interval1.Microseconds);
            Assert.AreEqual(3, header.Interval2.Seconds);
            Assert.AreEqual(250000, header.Interval2.Microseconds);
            Assert.AreEqual(0x1F, header.CanId);
            Assert.AreEqual(Environment.Is64BitProcess ? 56 : 36, Marshal.SizeOf<BcmMessageHeader>());
        }

        [Test]
        public void BcmMessageHeader_ToString_Test()
        {
            var header = new BcmMessageHeader()
            {
                Opcode = BcmOpcode.TX_SETUP,
                NumberOfFrames = 1,
                Flags = BcmFlags.TX_ANNOUNCE | BcmFlags.TX_COUNTEVT,
                Interval1Count = 25,
                Interval1 = new BcmTimeval(2, 600000),
                Interval2 = new BcmTimeval(3, 250000),
                CanId = 0x1F,    
            };
            Assert.AreEqual(BcmOpcode.TX_SETUP, header.Opcode);
            Assert.AreEqual(1, header.NumberOfFrames);
            Assert.AreEqual(BcmFlags.TX_ANNOUNCE | BcmFlags.TX_COUNTEVT, header.Flags);
            Assert.AreEqual(25, header.Interval1Count);
            Assert.AreEqual(2, header.Interval1.Seconds);
            Assert.AreEqual(600000, header.Interval1.Microseconds);
            Assert.AreEqual(3, header.Interval2.Seconds);
            Assert.AreEqual(250000, header.Interval2.Microseconds);
            Assert.AreEqual(0x1F, header.CanId);
            string str = header.ToString();
            Assert.IsNotNull(str);
            Console.WriteLine(str);
        }

        [Test]
        public void BcmMessageHeader32_Verification_Test()
        {
            var header = new BcmMessageHeader32()
            {
                Opcode = BcmOpcode.TX_SETUP,
                NumberOfFrames = 1,
                Flags = BcmFlags.TX_ANNOUNCE | BcmFlags.TX_COUNTEVT,
                Interval1Count = 25,
                Interval1 = new BcmTimeval(2, 600000),
                Interval2 = new BcmTimeval(3, 250000),
                CanId = 0x1F,    
            };
            Assert.AreEqual(BcmOpcode.TX_SETUP, header.Opcode);
            Assert.AreEqual(1, header.NumberOfFrames);
            Assert.AreEqual(BcmFlags.TX_ANNOUNCE | BcmFlags.TX_COUNTEVT, header.Flags);
            Assert.AreEqual(25, header.Interval1Count);
            Assert.AreEqual(2, header.Interval1.Seconds);
            Assert.AreEqual(600000, header.Interval1.Microseconds);
            Assert.AreEqual(3, header.Interval2.Seconds);
            Assert.AreEqual(250000, header.Interval2.Microseconds);
            Assert.AreEqual(0x1F, header.CanId);
            Assert.AreEqual(Environment.Is64BitProcess ? 56 : 40, Marshal.SizeOf<BcmMessageHeader32>());
        }

        [Test]
        public void BcmMessageHeader32_ToString_Test()
        {
            var header = new BcmMessageHeader32()
            {
                Opcode = BcmOpcode.TX_SETUP,
                NumberOfFrames = 1,
                Flags = BcmFlags.TX_ANNOUNCE | BcmFlags.TX_COUNTEVT,
                Interval1Count = 25,
                Interval1 = new BcmTimeval(2, 600000),
                Interval2 = new BcmTimeval(3, 250000),
                CanId = 0x1F,    
            };
            Assert.AreEqual(BcmOpcode.TX_SETUP, header.Opcode);
            Assert.AreEqual(1, header.NumberOfFrames);
            Assert.AreEqual(BcmFlags.TX_ANNOUNCE | BcmFlags.TX_COUNTEVT, header.Flags);
            Assert.AreEqual(25, header.Interval1Count);
            Assert.AreEqual(2, header.Interval1.Seconds);
            Assert.AreEqual(600000, header.Interval1.Microseconds);
            Assert.AreEqual(3, header.Interval2.Seconds);
            Assert.AreEqual(250000, header.Interval2.Microseconds);
            Assert.AreEqual(0x1F, header.CanId);
            string str = header.ToString();
            Assert.IsNotNull(str);
            Console.WriteLine(str);
        }
    }
}