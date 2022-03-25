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

using NUnit.Framework;
using SocketCANSharp;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace SocketCANSharpTest
{
    public class SocketOptionTests
    {
        SafeSocketHandle socketHandle;

        [TearDown]
        public void Cleanup()
        {
            socketHandle.Close();
        }

        [Test]
        public void SocketOption_Set_CAN_RAW_FILTER_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            var canFilter1 = new CanFilter(0x700, 0x700);
            var canFilter2 = new CanFilter(0x600, 0x600);
            var canFilterArray = new CanFilter[] { canFilter1, canFilter2 };
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_CAN_RAW, CanSocketOptions.CAN_RAW_FILTER, canFilterArray, Marshal.SizeOf(typeof(CanFilter)) * canFilterArray.Length);
            Assert.AreEqual(0, result);
        }

        [Test]
        public void SocketOption_Get_CAN_RAW_FILTER_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            var canFilter1 = new CanFilter(0x700, 0x700);
            var canFilter2 = new CanFilter(0x600, 0x600);
            var canFilterArray = new CanFilter[] { canFilter1, canFilter2 };
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_CAN_RAW, CanSocketOptions.CAN_RAW_FILTER, canFilterArray, Marshal.SizeOf(typeof(CanFilter)) * canFilterArray.Length);
            Assert.AreEqual(0, result);

            canFilterArray = new CanFilter[] { new CanFilter(), new CanFilter() };
            int len = Marshal.SizeOf(typeof(CanFilter)) * canFilterArray.Length;
            result = LibcNativeMethods.GetSockOpt(socketHandle, SocketLevel.SOL_CAN_RAW, CanSocketOptions.CAN_RAW_FILTER, canFilterArray, ref len);
            Assert.AreEqual(0, result);
            Assert.AreEqual(2, canFilterArray.Length);
            Assert.AreEqual(0x700, canFilterArray[0].CanId);
            Assert.AreEqual(0x700, canFilterArray[0].CanMask);
            Assert.AreEqual(0x600, canFilterArray[1].CanId);
            Assert.AreEqual(0x600, canFilterArray[1].CanMask);
        }

        [Test]
        public void SocketOption_Set_CAN_RAW_ERR_FILTER_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            uint err_mask = ((uint)CanErrorClass.CAN_ERR_TX_TIMEOUT | (uint)CanErrorClass.CAN_ERR_BUSOFF);
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_CAN_RAW, CanSocketOptions.CAN_RAW_ERR_FILTER, ref err_mask, Marshal.SizeOf(err_mask));
            Assert.AreEqual(0, result);
        }

        [Test]
        public void SocketOption_Get_CAN_RAW_ERR_FILTER_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            uint err_mask = ((uint)CanErrorClass.CAN_ERR_TX_TIMEOUT | (uint)CanErrorClass.CAN_ERR_BUSOFF);
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_CAN_RAW, CanSocketOptions.CAN_RAW_ERR_FILTER, ref err_mask, Marshal.SizeOf(err_mask));
            Assert.AreEqual(0, result);

            err_mask = 0;
            int len = Marshal.SizeOf(err_mask);
            result = LibcNativeMethods.GetSockOpt(socketHandle, SocketLevel.SOL_CAN_RAW, CanSocketOptions.CAN_RAW_ERR_FILTER, ref err_mask, ref len);
            Assert.AreEqual(0, result);
            Assert.AreEqual((uint)CanErrorClass.CAN_ERR_TX_TIMEOUT | (uint)CanErrorClass.CAN_ERR_BUSOFF, err_mask);
        }

        [Test]
        public void SocketOption_Set_CAN_RAW_LOOPBACK_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            int loopback = 1;
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_CAN_RAW, CanSocketOptions.CAN_RAW_LOOPBACK, ref loopback, Marshal.SizeOf(loopback));
            Assert.AreEqual(0, result);
        }

        [Test]
        public void SocketOption_Get_CAN_RAW_LOOPBACK_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            int loopback = 1;
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_CAN_RAW, CanSocketOptions.CAN_RAW_LOOPBACK, ref loopback, Marshal.SizeOf(loopback));
            Assert.AreEqual(0, result);

            loopback = 0;
            int len = Marshal.SizeOf(loopback);
            result = LibcNativeMethods.GetSockOpt(socketHandle, SocketLevel.SOL_CAN_RAW, CanSocketOptions.CAN_RAW_LOOPBACK, ref loopback, ref len);
            Assert.AreEqual(0, result);
            Assert.AreEqual(1, loopback);
        }

        [Test]
        public void SocketOption_Set_CAN_RAW_RECV_OWN_MSGS_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            int recv_own_msgs = 1;
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_CAN_RAW, CanSocketOptions.CAN_RAW_RECV_OWN_MSGS, ref recv_own_msgs, Marshal.SizeOf(recv_own_msgs));
            Assert.AreEqual(0, result);
        }

        [Test]
        public void SocketOption_Get_CAN_RAW_RECV_OWN_MSGS_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            int recv_own_msgs = 1;
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_CAN_RAW, CanSocketOptions.CAN_RAW_RECV_OWN_MSGS, ref recv_own_msgs, Marshal.SizeOf(recv_own_msgs));
            Assert.AreEqual(0, result);

            recv_own_msgs = 0;
            int len = Marshal.SizeOf(recv_own_msgs);
            result = LibcNativeMethods.GetSockOpt(socketHandle, SocketLevel.SOL_CAN_RAW, CanSocketOptions.CAN_RAW_RECV_OWN_MSGS, ref recv_own_msgs, ref len);
            Assert.AreEqual(0, result);
            Assert.AreEqual(1, recv_own_msgs);
        }

        [Test]
        public void SocketOption_Set_CAN_RAW_FD_FRAMES_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            int can_fd_enabled = 1;
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_CAN_RAW, CanSocketOptions.CAN_RAW_FD_FRAMES, ref can_fd_enabled, Marshal.SizeOf(can_fd_enabled));
            Assert.AreEqual(0, result);
        }

        [Test]
        public void SocketOption_Get_CAN_RAW_FD_FRAMES_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            int can_fd_enabled = 1;
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_CAN_RAW, CanSocketOptions.CAN_RAW_FD_FRAMES, ref can_fd_enabled, Marshal.SizeOf(can_fd_enabled));
            Assert.AreEqual(0, result);

            can_fd_enabled = 0;
            int len = Marshal.SizeOf(can_fd_enabled);
            result = LibcNativeMethods.GetSockOpt(socketHandle, SocketLevel.SOL_CAN_RAW, CanSocketOptions.CAN_RAW_FD_FRAMES, ref can_fd_enabled, ref len);
            Assert.AreEqual(0, result);
            Assert.AreEqual(1, can_fd_enabled);
        }

        [Test]
        public void SocketOption_Set_CAN_RAW_JOIN_FILTERS_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            var canFilter1 = new CanFilter(0x700, 0x700);
            var canFilter2 = new CanFilter(0x600, 0x600);
            var canFilterArray = new CanFilter[] { canFilter1, canFilter2 };
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_CAN_RAW, CanSocketOptions.CAN_RAW_FILTER, canFilterArray, Marshal.SizeOf(typeof(CanFilter)) * canFilterArray.Length);
            Assert.AreEqual(0, result);
            int join_filter = 1;
            result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_CAN_RAW, CanSocketOptions.CAN_RAW_JOIN_FILTERS, ref join_filter, Marshal.SizeOf(join_filter));
            Assert.AreEqual(0, result);
        }

        [Test]
        public void SocketOption_Get_CAN_RAW_JOIN_FILTERS_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            var canFilter1 = new CanFilter(0x700, 0x700);
            var canFilter2 = new CanFilter(0x600, 0x600);
            var canFilterArray = new CanFilter[] { canFilter1, canFilter2 };
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_CAN_RAW, CanSocketOptions.CAN_RAW_FILTER, canFilterArray, Marshal.SizeOf(typeof(CanFilter)) * canFilterArray.Length);
            Assert.AreEqual(0, result);
            int join_filter = 1;
            result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_CAN_RAW, CanSocketOptions.CAN_RAW_JOIN_FILTERS, ref join_filter, Marshal.SizeOf(join_filter));
            Assert.AreEqual(0, result);

            join_filter = 0;
            int len = Marshal.SizeOf(join_filter);
            result = LibcNativeMethods.GetSockOpt(socketHandle, SocketLevel.SOL_CAN_RAW, CanSocketOptions.CAN_RAW_JOIN_FILTERS, ref join_filter, ref len);
            Assert.AreEqual(0, result);
            Assert.AreEqual(1, join_filter);
        }

        [Test]
        public void SocketOption_Set_SO_SNDTIMEO_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            var timeval = new Timeval(0, 1);
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_SOCKET, SocketLevelOptions.SO_SNDTIMEO, timeval, Marshal.SizeOf(typeof(Timeval)));
            Assert.AreEqual(0, result);
        }

        [Test]
        public void SocketOption_Get_SO_SNDTIMEO_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            var timeval = new Timeval(2, 100000);
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_SOCKET, SocketLevelOptions.SO_SNDTIMEO, timeval, Marshal.SizeOf(typeof(Timeval)));
            Assert.AreEqual(0, result);

            timeval = new Timeval(0, 0);
            int len = Marshal.SizeOf(typeof(Timeval));
            result = LibcNativeMethods.GetSockOpt(socketHandle, SocketLevel.SOL_SOCKET, SocketLevelOptions.SO_SNDTIMEO, timeval, ref len);
            Assert.AreEqual(0, result);
            Assert.AreEqual(16, len);
            Assert.AreEqual(100000, timeval.Microseconds);
            Assert.AreEqual(2, timeval.Seconds);
        }

        [Test]
        public void SocketOption_Set_SO_RCVTIMEO_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            var timeval = new Timeval(0, 1);
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_SOCKET, SocketLevelOptions.SO_RCVTIMEO, timeval, Marshal.SizeOf(typeof(Timeval)));
            Assert.AreEqual(0, result);
        }

        [Test]
        public void SocketOption_Get_SO_RCVTIMEO_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            var timeval = new Timeval(3, 0);
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_SOCKET, SocketLevelOptions.SO_RCVTIMEO, timeval, Marshal.SizeOf(typeof(Timeval)));
            Assert.AreEqual(0, result);

            timeval = new Timeval(0, 0);
            int len = Marshal.SizeOf(typeof(Timeval));
            result = LibcNativeMethods.GetSockOpt(socketHandle, SocketLevel.SOL_SOCKET, SocketLevelOptions.SO_RCVTIMEO, timeval, ref len);
            Assert.AreEqual(0, result);
            Assert.AreEqual(16, len);
            Assert.AreEqual(0, timeval.Microseconds);
            Assert.AreEqual(3, timeval.Seconds);
        }

        [Test]
        public void SocketOption_Set_CAN_ISOTP_OPTS_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_ISOTP);
            Assert.IsFalse(socketHandle.IsInvalid);

            var canIsoTpOpts = new CanIsoTpOptions()
            {
                Flags = IsoTpFlags.CAN_ISOTP_TX_PADDING | IsoTpFlags.CAN_ISOTP_WAIT_TX_DONE,
                TxPadByte = 0xDD,
            };
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_CAN_ISOTP, CanIsoTpSocketOptions.CAN_ISOTP_OPTS, canIsoTpOpts, Marshal.SizeOf(typeof(CanIsoTpOptions)));
            Assert.AreEqual(0, result);
        }

        [Test]
        public void SocketOption_Get_CAN_ISOTP_OPTS_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_ISOTP);
            Assert.IsFalse(socketHandle.IsInvalid);

            var canIsoTpOpts = new CanIsoTpOptions()
            {
                Flags = IsoTpFlags.CAN_ISOTP_TX_PADDING | IsoTpFlags.CAN_ISOTP_WAIT_TX_DONE,
                TxPadByte = 0xDD,
            };
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_CAN_ISOTP, CanIsoTpSocketOptions.CAN_ISOTP_OPTS, canIsoTpOpts, Marshal.SizeOf(typeof(CanIsoTpOptions)));
            Assert.AreEqual(0, result);

            canIsoTpOpts = new CanIsoTpOptions();
            int len = Marshal.SizeOf(typeof(CanIsoTpOptions));
            result = LibcNativeMethods.GetSockOpt(socketHandle, SocketLevel.SOL_CAN_ISOTP, CanIsoTpSocketOptions.CAN_ISOTP_OPTS, canIsoTpOpts, ref len);
            Assert.AreEqual(0, result);
            Assert.AreEqual(12, len);
            Assert.AreEqual(IsoTpFlags.CAN_ISOTP_TX_PADDING | IsoTpFlags.CAN_ISOTP_WAIT_TX_DONE, canIsoTpOpts.Flags);
            Assert.AreEqual(0xDD, canIsoTpOpts.TxPadByte);
        }

        [Test]
        public void SocketOption_Set_CAN_ISOTP_RECV_FC_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_ISOTP);
            Assert.IsFalse(socketHandle.IsInvalid);

            var canIsoTpFcOpts = new CanIsoTpFlowControlOptions()
            {
                BlockSize = 2,
                Stmin = 10,
                WftMax = 60,
            };
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_CAN_ISOTP, CanIsoTpSocketOptions.CAN_ISOTP_RECV_FC, canIsoTpFcOpts, Marshal.SizeOf(typeof(CanIsoTpFlowControlOptions)));
            Assert.AreEqual(0, result);
        }

        [Test]
        public void SocketOption_Get_CAN_ISOTP_RECV_FC_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_ISOTP);
            Assert.IsFalse(socketHandle.IsInvalid);

            var canIsoTpFcOpts = new CanIsoTpFlowControlOptions()
            {
                BlockSize = 2,
                Stmin = 10,
                WftMax = 60,
            };
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_CAN_ISOTP, CanIsoTpSocketOptions.CAN_ISOTP_RECV_FC, canIsoTpFcOpts, Marshal.SizeOf(typeof(CanIsoTpFlowControlOptions)));
            Assert.AreEqual(0, result);

            canIsoTpFcOpts = new CanIsoTpFlowControlOptions();
            int len = Marshal.SizeOf(typeof(CanIsoTpFlowControlOptions));
            result = LibcNativeMethods.GetSockOpt(socketHandle, SocketLevel.SOL_CAN_ISOTP, CanIsoTpSocketOptions.CAN_ISOTP_RECV_FC, canIsoTpFcOpts, ref len);
            Assert.AreEqual(0, result);
            Assert.AreEqual((byte)2, canIsoTpFcOpts.BlockSize);
            Assert.AreEqual((byte)10, canIsoTpFcOpts.Stmin);
            Assert.AreEqual((byte)60, canIsoTpFcOpts.WftMax);
        }

        [Test]
        public void SocketOption_Set_CAN_ISOTP_TX_STMIN_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_ISOTP);
            Assert.IsFalse(socketHandle.IsInvalid);

            uint stmin = 100;
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_CAN_ISOTP, CanIsoTpSocketOptions.CAN_ISOTP_TX_STMIN, ref stmin, Marshal.SizeOf(typeof(int)));
            Assert.AreEqual(0, result);
        }

        [Test]
        public void SocketOption_Get_CAN_ISOTP_TX_STMIN_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_ISOTP);
            Assert.IsFalse(socketHandle.IsInvalid);

            uint stmin = 100;
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_CAN_ISOTP, CanIsoTpSocketOptions.CAN_ISOTP_TX_STMIN, ref stmin, Marshal.SizeOf(typeof(int)));
            Assert.AreEqual(0, result);

            stmin = 0;
            int len = Marshal.SizeOf(typeof(int));
            result = LibcNativeMethods.GetSockOpt(socketHandle, SocketLevel.SOL_CAN_ISOTP, CanIsoTpSocketOptions.CAN_ISOTP_TX_STMIN, ref stmin, ref len);
            Assert.AreEqual(0, result);
            Assert.AreEqual(100, stmin);
            Assert.AreEqual(4, len);
        }

        [Test]
        public void SocketOption_Set_CAN_ISOTP_RX_STMIN_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_ISOTP);
            Assert.IsFalse(socketHandle.IsInvalid);

            uint stmin = 100;
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_CAN_ISOTP, CanIsoTpSocketOptions.CAN_ISOTP_RX_STMIN, ref stmin, Marshal.SizeOf(typeof(int)));
            Assert.AreEqual(0, result);
        }

        [Test]
        public void SocketOption_Get_CAN_ISOTP_RX_STMIN_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_ISOTP);
            Assert.IsFalse(socketHandle.IsInvalid);

            uint stmin = 100;
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_CAN_ISOTP, CanIsoTpSocketOptions.CAN_ISOTP_RX_STMIN, ref stmin, Marshal.SizeOf(typeof(int)));
            Assert.AreEqual(0, result);

            stmin = 0;
            int len = Marshal.SizeOf(typeof(int));
            result = LibcNativeMethods.GetSockOpt(socketHandle, SocketLevel.SOL_CAN_ISOTP, CanIsoTpSocketOptions.CAN_ISOTP_RX_STMIN, ref stmin, ref len);
            Assert.AreEqual(0, result);
            Assert.AreEqual(100, stmin);
            Assert.AreEqual(4, len);
        }

        [Test]
        public void SocketOption_Set_CAN_ISOTP_LL_OPTS_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_ISOTP);
            Assert.IsFalse(socketHandle.IsInvalid);

            var canIsoTpLlOpts = new CanIsoTpLinkLayerOptions(16, 8);
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_CAN_ISOTP, CanIsoTpSocketOptions.CAN_ISOTP_LL_OPTS, canIsoTpLlOpts, Marshal.SizeOf(typeof(CanIsoTpLinkLayerOptions)));
            Assert.AreEqual(0, result);
        }

        [Test]
        public void SocketOption_Get_CAN_ISOTP_LL_OPTS_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_ISOTP);
            Assert.IsFalse(socketHandle.IsInvalid);

            var canIsoTpLlOpts = new CanIsoTpLinkLayerOptions(16, 8);
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_CAN_ISOTP, CanIsoTpSocketOptions.CAN_ISOTP_LL_OPTS, canIsoTpLlOpts, Marshal.SizeOf(typeof(CanIsoTpLinkLayerOptions)));
            Assert.AreEqual(0, result);

            canIsoTpLlOpts = new CanIsoTpLinkLayerOptions();
            int len = Marshal.SizeOf(typeof(CanIsoTpLinkLayerOptions));
            result = LibcNativeMethods.GetSockOpt(socketHandle, SocketLevel.SOL_CAN_ISOTP, CanIsoTpSocketOptions.CAN_ISOTP_LL_OPTS, canIsoTpLlOpts, ref len);
            Assert.AreEqual(0, result);
            Assert.AreEqual(16, canIsoTpLlOpts.Mtu);
            Assert.AreEqual(8, canIsoTpLlOpts.TxDataLength);
        }

        [Test]
        public void SocketOption_Set_SO_J1939_PROMISC_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_J1939);
            Assert.IsFalse(socketHandle.IsInvalid);

            int enable = 1;
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_CAN_J1939, J1939SocketOptions.SO_J1939_PROMISC, ref enable, Marshal.SizeOf(typeof(int)));
            Assert.AreEqual(1, result);

            enable = 0;
            result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_CAN_J1939, J1939SocketOptions.SO_J1939_PROMISC, ref enable, Marshal.SizeOf(typeof(int)));
            Assert.AreEqual(0, result);
        }

        [Test]
        public void SocketOption_Get_SO_J1939_PROMISC_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_J1939);
            Assert.IsFalse(socketHandle.IsInvalid);

            int enable = 1;
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_CAN_J1939, J1939SocketOptions.SO_J1939_PROMISC, ref enable, Marshal.SizeOf(typeof(int)));
            Assert.AreEqual(1, result);

            int len = Marshal.SizeOf(typeof(int));
            enable = 0;
            result = LibcNativeMethods.GetSockOpt(socketHandle, SocketLevel.SOL_CAN_J1939, J1939SocketOptions.SO_J1939_PROMISC, ref enable, ref len);
            Assert.AreEqual(0, result);
            Assert.AreEqual(4, len);
            Assert.AreEqual(1, enable);

            enable = 0;
            result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_CAN_J1939, J1939SocketOptions.SO_J1939_PROMISC, ref enable, Marshal.SizeOf(typeof(int)));
            Assert.AreEqual(0, result);
        }

        [Test]
        public void SocketOption_Set_SO_J1939_FILTER_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_J1939);
            Assert.IsFalse(socketHandle.IsInvalid);

            var filter = new J1939Filter()
            {
                Name = 0x00000000,
                NameMask = 0xFFFFFFFFFFFFFFFF,
                PGN = 0x40000,
                PGNMask = 0xFFFFFFFF,
                Address = 0xFF,
                AddressMask = 0xFF,
            };

            var filters = new J1939Filter[] { filter };
            var optionValueSize = Marshal.SizeOf(typeof(J1939Filter)) * filters.Length;
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_CAN_J1939, J1939SocketOptions.SO_J1939_FILTER, filters, optionValueSize);
            Assert.AreEqual(0, result);
        }

        [Test]
        public void SocketOption_Get_SO_J1939_FILTER_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_J1939);
            Assert.IsFalse(socketHandle.IsInvalid);

            var filter = new J1939Filter()
            {
                Name = 0x00000000,
                NameMask = 0xFFFFFFFFFFFFFFFF,
                PGN = 0x40000,
                PGNMask = 0xFFFFFFFF,
                Address = 0xFF,
                AddressMask = 0xFF,
            };

            var filters = new J1939Filter[] { filter };
            var optionValueSize = Marshal.SizeOf(typeof(J1939Filter)) * filters.Length;
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_CAN_J1939, J1939SocketOptions.SO_J1939_FILTER, filters, optionValueSize);
            Assert.AreEqual(0, result);

            filters = new J1939Filter[] { new J1939Filter() };
            optionValueSize = Marshal.SizeOf(typeof(J1939Filter)) * filters.Length;
            result = LibcNativeMethods.GetSockOpt(socketHandle, SocketLevel.SOL_CAN_J1939, J1939SocketOptions.SO_J1939_FILTER, filters, ref optionValueSize);
            Assert.AreEqual(-1, result); // per net/can/j1939/socket.c --> This option name is not supported by getsockopt
            Assert.AreEqual(92, LibcNativeMethods.Errno); // ENOPROTOOPT
        }

        [Test]
        public void SocketOption_Set_SO_J1939_SEND_PRIO_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_J1939);
            Assert.IsFalse(socketHandle.IsInvalid);

            int prio = 4;
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_CAN_J1939, J1939SocketOptions.SO_J1939_SEND_PRIO, ref prio, Marshal.SizeOf(typeof(int)));
            Assert.AreEqual(result, 0);
        }

        [Test]
        public void SocketOption_Get_SO_J1939_SEND_PRIO_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_J1939);
            Assert.IsFalse(socketHandle.IsInvalid);

            int prio = 4;
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_CAN_J1939, J1939SocketOptions.SO_J1939_SEND_PRIO, ref prio, Marshal.SizeOf(typeof(int)));
            Assert.AreEqual(result, 0);

            int len = Marshal.SizeOf(typeof(int));
            prio = 0;
            result = LibcNativeMethods.GetSockOpt(socketHandle, SocketLevel.SOL_CAN_J1939, J1939SocketOptions.SO_J1939_SEND_PRIO, ref prio, ref len);
            Assert.AreEqual(result, 0);
            Assert.AreEqual(4, prio);
            Assert.AreEqual(4, len);
        }

        [Test]
        public void SocketOption_Set_SO_J1939_ERRQUEUE_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_J1939);
            Assert.IsFalse(socketHandle.IsInvalid);

            int enable = 1;
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_CAN_J1939, J1939SocketOptions.SO_J1939_ERRQUEUE, ref enable, Marshal.SizeOf(typeof(int)));
            Assert.AreEqual(1, result);

            enable = 0;
            result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_CAN_J1939, J1939SocketOptions.SO_J1939_ERRQUEUE, ref enable, Marshal.SizeOf(typeof(int)));
            Assert.AreEqual(0, result);
        }

        [Test]
        public void SocketOption_Get_SO_J1939_ERRQUEUE_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_J1939);
            Assert.IsFalse(socketHandle.IsInvalid);

            int enable = 1;
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_CAN_J1939, J1939SocketOptions.SO_J1939_ERRQUEUE, ref enable, Marshal.SizeOf(typeof(int)));
            Assert.AreEqual(1, result);

            int len = Marshal.SizeOf(typeof(int));
            enable = 0;
            result = LibcNativeMethods.GetSockOpt(socketHandle, SocketLevel.SOL_CAN_J1939, J1939SocketOptions.SO_J1939_ERRQUEUE, ref enable, ref len);
            Assert.AreEqual(0, result);
            Assert.AreEqual(4, len);
            Assert.AreEqual(1, enable);

            enable = 0;
            result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_CAN_J1939, J1939SocketOptions.SO_J1939_ERRQUEUE, ref enable, Marshal.SizeOf(typeof(int)));
            Assert.AreEqual(0, result);

            len = Marshal.SizeOf(typeof(int));
            enable = 1;
            result = LibcNativeMethods.GetSockOpt(socketHandle, SocketLevel.SOL_CAN_J1939, J1939SocketOptions.SO_J1939_ERRQUEUE, ref enable, ref len);
            Assert.AreEqual(0, result);
            Assert.AreEqual(4, len);
            Assert.AreEqual(0, enable);
        }

        [Test]
        public void SocketOption_Set_SO_BROADCAST_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_J1939);
            Assert.IsFalse(socketHandle.IsInvalid);

            int value = 1;
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_SOCKET, SocketLevelOptions.SO_BROADCAST, ref value, Marshal.SizeOf(typeof(int)));
            Assert.AreEqual(0, result);
        }

        [Test]
        public void SocketOption_Get_SO_BROADCAST_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_J1939);
            Assert.IsFalse(socketHandle.IsInvalid);

            int value = 1;
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_SOCKET, SocketLevelOptions.SO_BROADCAST, ref value, Marshal.SizeOf(typeof(int)));
            Assert.AreEqual(0, result);

            value = 0;
            int len = Marshal.SizeOf(typeof(int));
            result = LibcNativeMethods.GetSockOpt(socketHandle, SocketLevel.SOL_SOCKET, SocketLevelOptions.SO_BROADCAST, ref value, ref len);
            Assert.AreEqual(0, result);
            Assert.AreEqual(1, value);
            Assert.AreEqual(4, len);
        }

        [Test]
        public void SocketOption_Set_SO_SNDBUF_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            int value = 8192;
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_SOCKET, SocketLevelOptions.SO_SNDBUF, ref value, Marshal.SizeOf(typeof(int)));
            Assert.AreEqual(0, result);
        }

        [Test]
        public void SocketOption_Get_SO_SNDBUF_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            int value = 8192;
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_SOCKET, SocketLevelOptions.SO_SNDBUF, ref value, Marshal.SizeOf(typeof(int)));
            Assert.AreEqual(0, result);

            value = 0;
            int len = Marshal.SizeOf(typeof(int));
            result = LibcNativeMethods.GetSockOpt(socketHandle, SocketLevel.SOL_SOCKET, SocketLevelOptions.SO_SNDBUF, ref value, ref len);
            Assert.AreEqual(0, result);
            Assert.AreEqual(16384, value); // the kernel doubles whatever we send it in setsockopt and this doubled value is what is returned by getsockopt
            Assert.AreEqual(4, len);
        }

        [Test]
        public void SocketOption_Set_SO_RCVBUF_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            int value = 8192;
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_SOCKET, SocketLevelOptions.SO_RCVBUF, ref value, Marshal.SizeOf(typeof(int)));
            Assert.AreEqual(0, result);
        }

        [Test]
        public void SocketOption_Get_SO_RCVBUF_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            int value = 8192;
            int result = LibcNativeMethods.SetSockOpt(socketHandle, SocketLevel.SOL_SOCKET, SocketLevelOptions.SO_RCVBUF, ref value, Marshal.SizeOf(typeof(int)));
            Assert.AreEqual(0, result);

            value = 0;
            int len = Marshal.SizeOf(typeof(int));
            result = LibcNativeMethods.GetSockOpt(socketHandle, SocketLevel.SOL_SOCKET, SocketLevelOptions.SO_RCVBUF, ref value, ref len);
            Assert.AreEqual(0, result);
            Assert.AreEqual(16384, value); // the kernel doubles whatever we send it in setsockopt and this doubled value is what is returned by getsockopt
            Assert.AreEqual(4, len);
        }
    }
}