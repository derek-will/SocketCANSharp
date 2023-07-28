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
using SocketCANSharp;
using SocketCANSharp.Network;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;

namespace SocketCANSharpTest
{
    public class RawCanSocketTests
    {
        [Test]
        public void RawCanSocket_Ctor_Success_Test()
        {
            using (var rawCanSocket = new RawCanSocket())
            {
                Assert.AreEqual(true, rawCanSocket.Blocking);
                Assert.AreEqual(false, rawCanSocket.Connected);
                Assert.AreEqual(false, rawCanSocket.EnableBroadcast);
                Assert.AreNotEqual(IntPtr.Zero, rawCanSocket.Handle);
                Assert.AreEqual(false, rawCanSocket.IsBound);
                Assert.AreEqual(SocketCanProtocolType.CAN_RAW, rawCanSocket.ProtocolType);
                Assert.NotNull(rawCanSocket.SafeHandle);
                Assert.AreEqual(false, rawCanSocket.SafeHandle.IsInvalid);
                Assert.AreEqual(SocketType.Raw, rawCanSocket.SocketType);
            }
        }

        [Test]
        public void RawCanSocket_BindUsingAddressStructure_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var rawCanSocket = new RawCanSocket())
            {
                var addr = new SockAddrCan(iface.Index);
                rawCanSocket.Bind(addr);
                Assert.AreEqual(true, rawCanSocket.IsBound);
            }
        }

        [Test]
        public void RawCanSocket_BindUsingAddressStructure_NullArgumentException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var rawCanSocket = new RawCanSocket())
            {
                SockAddrCan addr = null;
                Assert.Throws<ArgumentNullException>(() => rawCanSocket.Bind(addr));
            }
        }

        [Test]
        public void RawCanSocket_BindUsingAddressStructure_ObjectDisposedException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var rawCanSocket = new RawCanSocket())
            {
                rawCanSocket.Close();

                var addr = new SockAddrCan(iface.Index);
                Assert.Throws<ObjectDisposedException>(() => rawCanSocket.Bind(addr));
            }
        }

        [Test]
        public void RawCanSocket_BindUsingAddressStructure_SocketCanException_Failure_Test()
        {
            using (var rawCanSocket = new RawCanSocket())
            {
                var addr = new SockAddrCan(-1);
                SocketCanException ex = Assert.Throws<SocketCanException>(() => rawCanSocket.Bind(addr));
                Assert.AreEqual(SocketError.SocketError, ex.SocketErrorCode);
                Assert.AreEqual(19, ex.NativeErrorCode);
            }
        }

        [Test]
        public void RawCanSocket_BindUsingParameters_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var rawCanSocket = new RawCanSocket())
            {
                rawCanSocket.Bind(iface);
                Assert.AreEqual(true, rawCanSocket.IsBound);
            }
        }

        [Test]
        public void RawCanSocket_BindUsingParameters_NullArgumentException_Failure_Test()
        {
            using (var rawCanSocket = new RawCanSocket())
            {
                CanNetworkInterface iface = null;
                Assert.Throws<ArgumentNullException>(() => rawCanSocket.Bind(iface));
            }
        }

        [Test]
        public void RawCanSocket_BindUsingParameters_ObjectDisposedException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var rawCanSocket = new RawCanSocket())
            {
                rawCanSocket.Close();

                var addr = new SockAddrCan(iface.Index);
                Assert.Throws<ObjectDisposedException>(() => rawCanSocket.Bind(iface));
            }
        }

        [Test]
        public void RawCanSocket_BindUsingParameters_SocketCanException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var rawCanSocket = new RawCanSocket())
            {
                iface.Index = -2;
                var addr = new SockAddrCanIsoTp(iface.Index);

                SocketCanException ex = Assert.Throws<SocketCanException>(() => rawCanSocket.Bind(iface));
                Assert.AreEqual(SocketError.SocketError, ex.SocketErrorCode);
                Assert.AreEqual(19, ex.NativeErrorCode);
            }
        }

        [Test]
        public void RawCanSocket_CanFilters_Success_Test()
        {
            using (var rawCanSocket = new RawCanSocket())
            {
                rawCanSocket.CanFilters = new CanFilter[] { new CanFilter(0x700, 0x7ff), new CanFilter(0x701, 0x7ff) };

                CanFilter[] filters = rawCanSocket.CanFilters;
                Assert.NotNull(filters);
                Assert.AreEqual(2, filters.Length);
                Assert.AreEqual(0x700, filters[0].CanId);
                Assert.AreEqual(0x7ff, filters[0].CanMask);
                Assert.AreEqual(0x701, filters[1].CanId);
                Assert.AreEqual(0x7ff, filters[1].CanMask);
            }
        }

        [Test]
        public void RawCanSocket_CanFilters_Null_Success_Test()
        {
            using (var rawCanSocket = new RawCanSocket())
            {
                rawCanSocket.CanFilters = null;
                CanFilter[] filters = rawCanSocket.CanFilters;
                Assert.IsNotNull(filters);
                Assert.Zero(filters.Length);
            }
        }

        [Test]
        public void RawCanSocket_CanFilters_ObjectDisposedException_Failure_Test()
        {
            using (var rawCanSocket = new RawCanSocket())
            {
                rawCanSocket.Close();

                Assert.Throws<ObjectDisposedException>(() =>
                {
                    rawCanSocket.CanFilters = new CanFilter[] { new CanFilter(0x700, 0x7ff), new CanFilter(0x701, 0x7ff) };
                });
            }
        }

        [Test]
        public void RawCanSocket_CanFilters_Post_Bind_Assignment_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i => i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var rawCanSocket = new RawCanSocket())
            {
                rawCanSocket.Bind(iface);
                rawCanSocket.CanFilters = new CanFilter[] { new CanFilter(0x700, 0x7ff), new CanFilter(0x701, 0x7ff) };

                CanFilter[] filters = rawCanSocket.CanFilters;
                Assert.NotNull(filters);
                Assert.AreEqual(2, filters.Length);
                Assert.AreEqual(0x700, filters[0].CanId);
                Assert.AreEqual(0x7ff, filters[0].CanMask);
                Assert.AreEqual(0x701, filters[1].CanId);
                Assert.AreEqual(0x7ff, filters[1].CanMask);
            }
        }

        [Test]
        public void RawCanSocket_ErrorFilters_Success_Test()
        {
            using (var rawCanSocket = new RawCanSocket())
            {
                rawCanSocket.ErrorFilters = CanErrorClass.CAN_ERR_ACK | CanErrorClass.CAN_ERR_BUSERROR;
                CanErrorClass errFilters = rawCanSocket.ErrorFilters;
                Assert.IsTrue(errFilters.HasFlag(CanErrorClass.CAN_ERR_ACK));  
                Assert.IsTrue(errFilters.HasFlag(CanErrorClass.CAN_ERR_BUSERROR));
                Assert.IsFalse(errFilters.HasFlag(CanErrorClass.CAN_ERR_BUSOFF));
                Assert.IsFalse(errFilters.HasFlag(CanErrorClass.CAN_ERR_CRTL));
                Assert.IsFalse(errFilters.HasFlag(CanErrorClass.CAN_ERR_LOSTARB));
                Assert.IsFalse(errFilters.HasFlag(CanErrorClass.CAN_ERR_PROT));
                Assert.IsFalse(errFilters.HasFlag(CanErrorClass.CAN_ERR_RESTARTED));
                Assert.IsFalse(errFilters.HasFlag(CanErrorClass.CAN_ERR_TRX));
                Assert.IsFalse(errFilters.HasFlag(CanErrorClass.CAN_ERR_TX_TIMEOUT));
            }
        }

        [Test]
        public void RawCanSocket_ErrorFilters_Invalid_ErrorClass_Test()
        {
            using (var rawCanSocket = new RawCanSocket())
            {
                rawCanSocket.ErrorFilters = (CanErrorClass)0x00001000;
            }
        }

        [Test]
        public void RawCanSocket_ErrorFilters_ObjectDisposedException_Failure_Test()
        {
            using (var rawCanSocket = new RawCanSocket())
            {
                rawCanSocket.Close();

                Assert.Throws<ObjectDisposedException>(() =>
                {
                    rawCanSocket.ErrorFilters = CanErrorClass.CAN_ERR_ACK | CanErrorClass.CAN_ERR_BUSERROR;
                });
            }
        }

        [Test]
        public void RawCanSocket_ErrorFilters_Post_Bind_Assignment_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i => i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var rawCanSocket = new RawCanSocket())
            {
                rawCanSocket.Bind(iface);
                rawCanSocket.ErrorFilters = CanErrorClass.CAN_ERR_ACK | CanErrorClass.CAN_ERR_BUSERROR;
                CanErrorClass errFilters = rawCanSocket.ErrorFilters;
                Assert.IsTrue(errFilters.HasFlag(CanErrorClass.CAN_ERR_ACK));  
                Assert.IsTrue(errFilters.HasFlag(CanErrorClass.CAN_ERR_BUSERROR));
                Assert.IsFalse(errFilters.HasFlag(CanErrorClass.CAN_ERR_BUSOFF));
                Assert.IsFalse(errFilters.HasFlag(CanErrorClass.CAN_ERR_CRTL));
                Assert.IsFalse(errFilters.HasFlag(CanErrorClass.CAN_ERR_LOSTARB));
                Assert.IsFalse(errFilters.HasFlag(CanErrorClass.CAN_ERR_PROT));
                Assert.IsFalse(errFilters.HasFlag(CanErrorClass.CAN_ERR_RESTARTED));
                Assert.IsFalse(errFilters.HasFlag(CanErrorClass.CAN_ERR_TRX));
                Assert.IsFalse(errFilters.HasFlag(CanErrorClass.CAN_ERR_TX_TIMEOUT));
            }
        }

        [Test]
        public void RawCanSocket_LocalLoopback_Success_Test()
        {
            using (var rawCanSocket = new RawCanSocket())
            {
                rawCanSocket.LocalLoopback = true;
                Assert.IsTrue(rawCanSocket.LocalLoopback);
                rawCanSocket.LocalLoopback = false;
                Assert.IsFalse(rawCanSocket.LocalLoopback);
            }
        }

        [Test]
        public void RawCanSocket_LocalLoopback_ObjectDisposedException_Failure_Test()
        {
            using (var rawCanSocket = new RawCanSocket())
            {
                rawCanSocket.Close();

                Assert.Throws<ObjectDisposedException>(() =>
                {
                    rawCanSocket.LocalLoopback = true;
                });
            }
        }

        [Test]
        public void RawCanSocket_LocalLoopback_Post_Bind_Assignment_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i => i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var rawCanSocket = new RawCanSocket())
            {
                rawCanSocket.Bind(iface);
                rawCanSocket.LocalLoopback = true;
                Assert.IsTrue(rawCanSocket.LocalLoopback);
                rawCanSocket.LocalLoopback = false;
                Assert.IsFalse(rawCanSocket.LocalLoopback);
            }
        }

        [Test]
        public void RawCanSocket_ReceiveOwnMessages_Success_Test()
        {
            using (var rawCanSocket = new RawCanSocket())
            {
                rawCanSocket.ReceiveOwnMessages = true;
                Assert.IsTrue(rawCanSocket.ReceiveOwnMessages);
                rawCanSocket.ReceiveOwnMessages = false;
                Assert.IsFalse(rawCanSocket.ReceiveOwnMessages);
            }
        }

        [Test]
        public void RawCanSocket_ReceiveOwnMessages_ObjectDisposedException_Failure_Test()
        {
            using (var rawCanSocket = new RawCanSocket())
            {
                rawCanSocket.Close();

                Assert.Throws<ObjectDisposedException>(() =>
                {
                    rawCanSocket.ReceiveOwnMessages = true;
                });
            }
        }

        [Test]
        public void RawCanSocket_ReceiveOwnMessages_Post_Bind_Assignment_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i => i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var rawCanSocket = new RawCanSocket())
            {
                rawCanSocket.Bind(iface);
                rawCanSocket.ReceiveOwnMessages = true;
                Assert.IsTrue(rawCanSocket.ReceiveOwnMessages);
                rawCanSocket.ReceiveOwnMessages = false;
                Assert.IsFalse(rawCanSocket.ReceiveOwnMessages);
            }
        }

        [Test]
        public void RawCanSocket_EnableCanFdFrames_Success_Test()
        {
            using (var rawCanSocket = new RawCanSocket())
            {
                rawCanSocket.EnableCanFdFrames = true;
                Assert.IsTrue(rawCanSocket.EnableCanFdFrames);
                rawCanSocket.EnableCanFdFrames = false;
                Assert.IsFalse(rawCanSocket.EnableCanFdFrames);
            }
        }

        [Test]
        public void RawCanSocket_EnableCanFdFrames_ObjectDisposedException_Failure_Test()
        {
            using (var rawCanSocket = new RawCanSocket())
            {
                rawCanSocket.Close();

                Assert.Throws<ObjectDisposedException>(() =>
                {
                    rawCanSocket.EnableCanFdFrames = true;
                });
            }
        }

        [Test]
        public void RawCanSocket_EnableCanFdFrames_Post_Bind_Assignment_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i => i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var rawCanSocket = new RawCanSocket())
            {
                rawCanSocket.Bind(iface);
                rawCanSocket.EnableCanFdFrames = true;
                Assert.IsTrue(rawCanSocket.EnableCanFdFrames);
                rawCanSocket.EnableCanFdFrames = false;
                Assert.IsFalse(rawCanSocket.EnableCanFdFrames);
            }
        }

        [Test]
        public void RawCanSocket_AllCanFiltersMustMatch_Success_Test()
        {
            using (var rawCanSocket = new RawCanSocket())
            {
                rawCanSocket.AllCanFiltersMustMatch = true;
                Assert.IsTrue(rawCanSocket.AllCanFiltersMustMatch);
                rawCanSocket.AllCanFiltersMustMatch = false;
                Assert.IsFalse(rawCanSocket.AllCanFiltersMustMatch);
            }
        }

        [Test]
        public void RawCanSocket_AllCanFiltersMustMatch_ObjectDisposedException_Failure_Test()
        {
            using (var rawCanSocket = new RawCanSocket())
            {
                rawCanSocket.Close();

                Assert.Throws<ObjectDisposedException>(() =>
                {
                    rawCanSocket.AllCanFiltersMustMatch = true;
                });
            }
        }

        [Test]
        public void RawCanSocket_AllCanFiltersMustMatch_Post_Bind_Assignment_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i => i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var rawCanSocket = new RawCanSocket())
            {
                rawCanSocket.Bind(iface);
                rawCanSocket.AllCanFiltersMustMatch = true;
                Assert.IsTrue(rawCanSocket.AllCanFiltersMustMatch);
                rawCanSocket.AllCanFiltersMustMatch = false;
                Assert.IsFalse(rawCanSocket.AllCanFiltersMustMatch);
            }
        }

        [Test]
        public void RawCanSocket_Write_CanFrame_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var rawCanSocket = new RawCanSocket())
            {
                rawCanSocket.Bind(iface);
                int bytesWritten = rawCanSocket.Write(new CanFrame(0x123, new byte[] { 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef }));
                Assert.AreEqual(16, bytesWritten);
            }
        }

        [Test]
        public void RawCanSocket_Write_ObjectDisposedException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var rawCanSocket = new RawCanSocket())
            {
                rawCanSocket.Bind(iface);
                rawCanSocket.Close();
                Assert.Throws<ObjectDisposedException>(() => rawCanSocket.Write(new CanFrame(0x123, new byte[] { 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef })));
            }
        }

        [Test]
        public void RawCanSocket_Write_SocketCanException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var rawCanSocket = new RawCanSocket())
            {
                SocketCanException ex = Assert.Throws<SocketCanException>(() => rawCanSocket.Write(new CanFrame(0x123, new byte[] { 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef })));
                Assert.AreEqual(SocketError.HostNotFound, ex.SocketErrorCode);
                Assert.AreEqual(6, ex.NativeErrorCode);
            }
        }

        [Test]
        public void RawCanSocket_Read_CanFrame_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var senderSocket = new RawCanSocket())
            using (var receiverSocket = new RawCanSocket())
            {
                senderSocket.Bind(iface);
                receiverSocket.Bind(iface);

                int bytesWritten = senderSocket.Write(new CanFrame(0x123, new byte[] { 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef }));
                Assert.AreEqual(16, bytesWritten);

                int bytesRead = receiverSocket.Read(out CanFrame frame);
                Assert.AreEqual(16, bytesRead);
                Assert.AreEqual(0x123, frame.CanId);
                Assert.IsTrue(frame.Data.Take(frame.Length).SequenceEqual(new byte[] { 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef }));
            }
        }

        [Test]
        public void RawCanSocket_Read_ObjectDisposedException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var senderSocket = new RawCanSocket())
            using (var receiverSocket = new RawCanSocket())
            {
                senderSocket.Bind(iface);
                receiverSocket.Bind(iface);
                receiverSocket.Close();

                int bytesWritten = senderSocket.Write(new CanFrame(0x123, new byte[] { 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef }));
                Assert.AreEqual(16, bytesWritten);

                Assert.Throws<ObjectDisposedException>(() => receiverSocket.Read(out CanFrame frame));
            }
        }

        [Test]
        public void RawCanSocket_Read_SocketCanException_Timeout_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var senderSocket = new RawCanSocket())
            using (var receiverSocket = new RawCanSocket())
            {
                receiverSocket.ReceiveTimeout = 100;
                receiverSocket.CanFilters = new CanFilter[] { new CanFilter(0x555, 0x555) }; // Set to look for CAN Frames other than the 0x123 message
                receiverSocket.Bind(iface);
                senderSocket.Bind(iface);

                int bytesWritten = senderSocket.Write(new CanFrame(0x123, new byte[] { 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef }));
                Assert.AreEqual(16, bytesWritten);

                SocketCanException ex = Assert.Throws<SocketCanException>(() => receiverSocket.Read(out CanFrame frame));
                Assert.AreEqual(SocketError.WouldBlock, ex.SocketErrorCode);
                Assert.AreEqual(11, ex.NativeErrorCode);
            }
        }

        [Test]
        public void RawCanSocket_Write_CanFdFrame_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var rawCanSocket = new RawCanSocket())
            {
                Assume.That(iface.MaximumTransmissionUnit, Is.EqualTo(SocketCanConstants.CANFD_MTU));
                rawCanSocket.EnableCanFdFrames = true;
                rawCanSocket.Bind(iface);
                int bytesWritten = rawCanSocket.Write(new CanFdFrame(0x123, new byte[] { 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef }, CanFdFlags.CANFD_BRS));
                Assert.AreEqual(72, bytesWritten);
            }
        }

        [Test]
        public void RawCanSocket_Read_CanFdFrame_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var senderSocket = new RawCanSocket())
            using (var receiverSocket = new RawCanSocket())
            {
                Assume.That(iface.MaximumTransmissionUnit, Is.EqualTo(SocketCanConstants.CANFD_MTU));
                senderSocket.EnableCanFdFrames = true;
                receiverSocket.EnableCanFdFrames = true;
                senderSocket.Bind(iface);
                receiverSocket.Bind(iface);

                int bytesWritten = senderSocket.Write(new CanFdFrame(0x123, new byte[] { 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef }, CanFdFlags.CANFD_BRS));
                Assert.AreEqual(72, bytesWritten);

                int bytesRead = receiverSocket.Read(out CanFdFrame frame);
                Assert.AreEqual(72, bytesRead);
                Assert.AreEqual(0x123, frame.CanId);
                Assert.IsTrue(frame.Data.Take(frame.Length).SequenceEqual(new byte[] { 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef }));
                Assert.IsTrue(frame.Flags.HasFlag(CanFdFlags.CANFD_BRS)); // In Kernel 6.1 and higher - CANFD_FDF flag will also be set. Changing to just check for BRS to be backwards compatible.
            }
        }

        [Test]
        public void RawCanSocket_Write_CanFdFrame_SocketCanException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var rawCanSocket = new RawCanSocket())
            {
                Assume.That(iface.MaximumTransmissionUnit, Is.EqualTo(SocketCanConstants.CANFD_MTU));
                rawCanSocket.Bind(iface);
                SocketCanException ex = Assert.Throws<SocketCanException>(() => rawCanSocket.Write(new CanFdFrame(0x123, new byte[] { 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef }, CanFdFlags.CANFD_BRS)));
                Assert.AreEqual(SocketError.InvalidArgument, ex.SocketErrorCode);
                Assert.AreEqual(22, ex.NativeErrorCode);
            }
        }

        [Test]
        public void RawCanSocket_Read_CanFdFrame_FdNotEnabled_SocketCanException_Timeout_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var senderSocket = new RawCanSocket())
            using (var receiverSocket = new RawCanSocket())
            {
                Assume.That(iface.MaximumTransmissionUnit, Is.EqualTo(SocketCanConstants.CANFD_MTU));
                senderSocket.EnableCanFdFrames = true;
                receiverSocket.ReceiveTimeout = 100;
                senderSocket.Bind(iface);
                receiverSocket.Bind(iface);

                int bytesWritten = senderSocket.Write(new CanFdFrame(0x123, new byte[] { 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef }, CanFdFlags.CANFD_BRS));
                Assert.AreEqual(72, bytesWritten);

                SocketCanException ex = Assert.Throws<SocketCanException>(() => receiverSocket.Read(out CanFdFrame frame));
                Assert.AreEqual(SocketError.WouldBlock, ex.SocketErrorCode);
                Assert.AreEqual(11, ex.NativeErrorCode);
            }
        }

        [Test]
        public void RawCanSocket_Write_ClassicFrame_on_FdEnabledSocket_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var rawCanSocket = new RawCanSocket())
            {
                rawCanSocket.EnableCanFdFrames = true;
                rawCanSocket.Bind(iface);
                int bytesWritten = rawCanSocket.Write(new CanFrame(0x123, new byte[] { 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef }));
                Assert.AreEqual(16, bytesWritten);
            }
        }

        [Test]
        public void RawCanSocket_Read_ClassicFrame_Using_CanFdFrame_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var senderSocket = new RawCanSocket())
            using (var receiverSocket = new RawCanSocket())
            {
                senderSocket.EnableCanFdFrames = true;
                receiverSocket.EnableCanFdFrames = true;
                senderSocket.Bind(iface);
                receiverSocket.Bind(iface);

                int bytesWritten = senderSocket.Write(new CanFrame(0x123, new byte[] { 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef }));
                Assert.AreEqual(16, bytesWritten);

                int bytesRead = receiverSocket.Read(out CanFdFrame frame);
                Assert.AreEqual(16, bytesRead);
                Assert.AreEqual(0x123, frame.CanId);
                Assert.IsTrue(frame.Data.Take(frame.Length).SequenceEqual(new byte[] { 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef }));
            }
        }

        [Test]
        public void RawCanSocket_GetAddress_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var rawCanSocket = new RawCanSocket())
            {
                rawCanSocket.Bind(iface);
                Assert.AreEqual(true, rawCanSocket.IsBound);
                SockAddrCan addr = rawCanSocket.Address;
                Assert.AreEqual(SocketCanConstants.AF_CAN, addr.CanFamily);
                Assert.AreEqual(iface.Index, addr.CanIfIndex);
            }
        }

        [Test]
        public void RawCanSocket_GetAddress_NotBound_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var rawCanSocket = new RawCanSocket())
            {
                Assert.AreEqual(false, rawCanSocket.IsBound);
                SockAddrCan addr = rawCanSocket.Address;
                Assert.AreEqual(SocketCanConstants.AF_CAN, addr.CanFamily);
                Assert.AreEqual(0, addr.CanIfIndex);
            }
        }
    }
}
