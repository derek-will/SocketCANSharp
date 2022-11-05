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
using SocketCANSharp.Network.BroadcastManagement;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;

namespace SocketCANSharpTest
{
    public class BcmCanSocketTests
    {
        [Test]
        public void BcmCanSocket_Ctor_Success_Test()
        {
            using (var bcmCanSocket = new BcmCanSocket())
            {
                Assert.AreEqual(true, bcmCanSocket.Blocking);
                Assert.AreEqual(false, bcmCanSocket.Connected);
                Assert.AreEqual(false, bcmCanSocket.EnableBroadcast);
                Assert.AreNotEqual(IntPtr.Zero, bcmCanSocket.Handle);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(SocketCanProtocolType.CAN_BCM, bcmCanSocket.ProtocolType);
                Assert.NotNull(bcmCanSocket.SafeHandle);
                Assert.AreEqual(false, bcmCanSocket.SafeHandle.IsInvalid);
                Assert.AreEqual(SocketType.Dgram, bcmCanSocket.SocketType);
            }
        }

        [Test]
        public void BcmCanSocket_ConnectUsingAddressStructure_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                var addr = new SockAddrCan(iface.Index);
                bcmCanSocket.Connect(addr);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(true, bcmCanSocket.Connected);
            }
        }

        [Test]
        public void BcmCanSocket_ConnectUsingAddressStructure_NullArgumentException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                SockAddrCan addr = null;
                Assert.Throws<ArgumentNullException>(() => bcmCanSocket.Connect(addr));
            }
        }

        [Test]
        public void BcmCanSocket_ConnectUsingAddressStructure_ObjectDisposedException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.Close();

                var addr = new SockAddrCan(iface.Index);
                Assert.Throws<ObjectDisposedException>(() => bcmCanSocket.Connect(addr));
            }
        }

        [Test]
        public void BcmCanSocket_ConnectUsingAddressStructure_SocketCanException_Failure_Test()
        {
            using (var bcmCanSocket = new BcmCanSocket())
            {
                var addr = new SockAddrCan(-1);
                SocketCanException ex = Assert.Throws<SocketCanException>(() => bcmCanSocket.Connect(addr));
                Assert.AreEqual(SocketError.SocketError, ex.SocketErrorCode);
                Assert.AreEqual(19, ex.NativeErrorCode);
            }
        }

        [Test]
        public void BcmCanSocket_ConnectUsingParameters_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.Connect(iface);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(true, bcmCanSocket.Connected);
            }
        }

        [Test]
        public void BcmCanSocket_ConnectUsingParameters_NullArgumentException_Failure_Test()
        {
            using (var bcmCanSocket = new BcmCanSocket())
            {
                CanNetworkInterface iface = null;
                Assert.Throws<ArgumentNullException>(() => bcmCanSocket.Connect(iface));
            }
        }

        [Test]
        public void BcmCanSocket_ConnectUsingParameters_ObjectDisposedException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.Close();

                var addr = new SockAddrCan(iface.Index);
                Assert.Throws<ObjectDisposedException>(() => bcmCanSocket.Connect(iface));
            }
        }

        [Test]
        public void BcmCanSocket_ConnectUsingParameters_SocketCanException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                iface.Index = -2;
                var addr = new SockAddrCan(iface.Index);

                SocketCanException ex = Assert.Throws<SocketCanException>(() => bcmCanSocket.Connect(iface));
                Assert.AreEqual(SocketError.SocketError, ex.SocketErrorCode);
                Assert.AreEqual(19, ex.NativeErrorCode);
            }
        }

        [Test]
        public void BcmCanSocket_CreateCyclicTransmissionTask_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.Connect(iface);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(true, bcmCanSocket.Connected);

                var canFrame = new CanFrame(0x333, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF });
                var frames = new CanFrame[] { canFrame };
                var config = new BcmCyclicTxTaskConfiguration()
                {
                    Id = 0x00,
                    StartTimer = true,
                    SetInterval = true,
                    InitialIntervalConfiguration = new BcmInitialIntervalConfiguration(10, new BcmTimeval(0, 5000)), // 10 messages at 5 ms
                    PostInitialInterval = new BcmTimeval(0, 100000), // Then at 100 ms
                };
                int nBytes = bcmCanSocket.CreateCyclicTransmissionTask(config, frames);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(72, nBytes);
                }
                else
                {
                    Assert.AreEqual(56, nBytes);
                }
            }
        }

        [Test]
        public void BcmCanSocket_CANFD_CreateCyclicTransmissionTask_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.Connect(iface);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(true, bcmCanSocket.Connected);

                var canFrame = new CanFdFrame(0x312, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF }, CanFdFlags.CANFD_BRS);
                var frames = new CanFdFrame[] { canFrame };
                var config = new BcmCyclicTxTaskConfiguration()
                {
                    Id = 0x00,
                    StartTimer = true,
                    SetInterval = true,
                    InitialIntervalConfiguration = new BcmInitialIntervalConfiguration(10, new BcmTimeval(0, 5000)), // 10 messages at 5 ms
                    PostInitialInterval = new BcmTimeval(0, 100000), // Then at 100 ms
                };
                int nBytes = bcmCanSocket.CreateCyclicTransmissionTask(config, frames);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(128, nBytes);
                }
                else
                {
                    Assert.AreEqual(112, nBytes);
                }
            }
        }

        [Test]
        public void BcmCanSocket_CreateCyclicTransmissionTask_InvalidTime_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.Connect(iface);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(true, bcmCanSocket.Connected);

                var canFrame = new CanFrame(0x333, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF });
                var frames = new CanFrame[] { canFrame };
                var config = new BcmCyclicTxTaskConfiguration()
                {
                    Id = 0x00,
                    StartTimer = true,
                    SetInterval = true,
                    InitialIntervalConfiguration = new BcmInitialIntervalConfiguration(10, new BcmTimeval(0, -1)), // invalid time
                    PostInitialInterval = new BcmTimeval(-1, 0), // invalid time
                };
                SocketCanException ex = Assert.Throws<SocketCanException>(() => bcmCanSocket.CreateCyclicTransmissionTask(config, frames));
                Assert.AreEqual(SocketError.InvalidArgument, ex.SocketErrorCode);
                Assert.AreEqual(22, ex.NativeErrorCode);
            }
        }

        [Test]
        public void BcmCanSocket_CreateCyclicTransmissionTask_NotConnected_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(false, bcmCanSocket.Connected);

                var canFrame = new CanFrame(0x333, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF });
                var frames = new CanFrame[] { canFrame };
                var config = new BcmCyclicTxTaskConfiguration()
                {
                    Id = 0x00,
                    StartTimer = true,
                    SetInterval = true,
                    InitialIntervalConfiguration = new BcmInitialIntervalConfiguration(10, new BcmTimeval(0, 5000)), // 10 messages at 5 ms
                    PostInitialInterval = new BcmTimeval(0, 100000), // Then at 100 ms
                };

                SocketCanException ex = Assert.Throws<SocketCanException>(() => bcmCanSocket.CreateCyclicTransmissionTask(config, frames));
                Assert.AreEqual(SocketError.NotConnected, ex.SocketErrorCode);
                Assert.AreEqual(107, ex.NativeErrorCode);
            }
        }

        [Test]
        public void BcmCanSocket_CreateCyclicTransmissionTask_NoDevice_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            iface.Index = 0; // set to no device
            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.Connect(iface);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(true, bcmCanSocket.Connected);

                var canFrame = new CanFrame(0x333, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF });
                var frames = new CanFrame[] { canFrame };
                var config = new BcmCyclicTxTaskConfiguration()
                {
                    Id = 0x00,
                    StartTimer = true,
                    SetInterval = true,
                    InitialIntervalConfiguration = new BcmInitialIntervalConfiguration(10, new BcmTimeval(0, 5000)), // 10 messages at 5 ms
                    PostInitialInterval = new BcmTimeval(0, 100000), // Then at 100 ms
                };

                SocketCanException ex = Assert.Throws<SocketCanException>(() => bcmCanSocket.CreateCyclicTransmissionTask(config, frames));
                Assert.AreEqual(SocketError.SocketError, ex.SocketErrorCode);
                Assert.AreEqual(19, ex.NativeErrorCode);
            }
        }

        [Test]
        public void BcmCanSocket_CreateCyclicTransmissionTask_NoFrames_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.Connect(iface);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(true, bcmCanSocket.Connected);

                var frames = new CanFrame[0];
                var config = new BcmCyclicTxTaskConfiguration()
                {
                    Id = 0x00,
                    StartTimer = true,
                    SetInterval = true,
                    InitialIntervalConfiguration = new BcmInitialIntervalConfiguration(10, new BcmTimeval(0, 5000)), // 10 messages at 5 ms
                    PostInitialInterval = new BcmTimeval(0, 100000), // Then at 100 ms
                };

                SocketCanException ex = Assert.Throws<SocketCanException>(() => bcmCanSocket.CreateCyclicTransmissionTask(config, frames));
                Assert.AreEqual(SocketError.InvalidArgument, ex.SocketErrorCode);
                Assert.AreEqual(22, ex.NativeErrorCode);
            }
        }

        [Test]
        public void BcmCanSocket_CreateCyclicTransmissionTask_NoInitialInterval_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.Connect(iface);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(true, bcmCanSocket.Connected);

                var canFrame = new CanFrame(0x333, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF });
                var frames = new CanFrame[] { canFrame };
                var config = new BcmCyclicTxTaskConfiguration()
                {
                    Id = 0x00,
                    StartTimer = true,
                    SetInterval = true,
                    InitialIntervalConfiguration = new BcmInitialIntervalConfiguration(),
                    PostInitialInterval = new BcmTimeval(0, 10000), // 10 ms
                };
                int nBytes = bcmCanSocket.CreateCyclicTransmissionTask(config, frames);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(72, nBytes);
                }
                else
                {
                    Assert.AreEqual(56, nBytes);
                }
            }
        }

        [Test]
        public void BcmCanSocket_RemoveCyclicTransmissionTask_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.Connect(iface);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(true, bcmCanSocket.Connected);

                var canFrame = new CanFrame(0x333, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF });
                var frames = new CanFrame[] { canFrame };
                var config = new BcmCyclicTxTaskConfiguration()
                {
                    Id = 0x333,
                    StartTimer = true,
                    SetInterval = true,
                    InitialIntervalConfiguration = new BcmInitialIntervalConfiguration(10, new BcmTimeval(0, 5000)), // 10 messages at 5 ms
                    PostInitialInterval = new BcmTimeval(0, 100000), // Then at 100 ms
                };
                int nBytes = bcmCanSocket.CreateCyclicTransmissionTask(config, frames);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(72, nBytes);
                }
                else
                {
                    Assert.AreEqual(56, nBytes);
                }

                nBytes = bcmCanSocket.RemoveCyclicTransmissionTask(0x333, BcmCanFrameType.ClassicCAN);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(56, nBytes);
                }
                else
                {
                    Assert.AreEqual(40, nBytes);
                }
            }
        }

        [Test]
        public void BcmCanSocket_CANFD_RemoveCyclicTransmissionTask_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.Connect(iface);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(true, bcmCanSocket.Connected);

                var canFrame = new CanFdFrame(0x312, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF }, CanFdFlags.CANFD_BRS);
                var frames = new CanFdFrame[] { canFrame };
                var config = new BcmCyclicTxTaskConfiguration()
                {
                    Id = 0x312,
                    StartTimer = true,
                    SetInterval = true,
                    InitialIntervalConfiguration = new BcmInitialIntervalConfiguration(10, new BcmTimeval(0, 5000)), // 10 messages at 5 ms
                    PostInitialInterval = new BcmTimeval(0, 100000), // Then at 100 ms
                };
                int nBytes = bcmCanSocket.CreateCyclicTransmissionTask(config, frames);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(128, nBytes);
                }
                else
                {
                    Assert.AreEqual(112, nBytes);
                }

                nBytes = bcmCanSocket.RemoveCyclicTransmissionTask(0x312, BcmCanFrameType.CANFD);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(56, nBytes);
                }
                else
                {
                    Assert.AreEqual(40, nBytes);
                }
            }
        }

        [Test]
        public void BcmCanSocket_RemoveCyclicTransmissionTask_NotFound_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.Connect(iface);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(true, bcmCanSocket.Connected);

                var canFrame = new CanFrame(0x333, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF });
                var frames = new CanFrame[] { canFrame };
                var config = new BcmCyclicTxTaskConfiguration()
                {
                    Id = 0x333,
                    StartTimer = true,
                    SetInterval = true,
                    InitialIntervalConfiguration = new BcmInitialIntervalConfiguration(10, new BcmTimeval(0, 5000)), // 10 messages at 5 ms
                    PostInitialInterval = new BcmTimeval(0, 100000), // Then at 100 ms
                };
                int nBytes = bcmCanSocket.CreateCyclicTransmissionTask(config, frames);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(72, nBytes);
                }
                else
                {
                    Assert.AreEqual(56, nBytes);
                }
                
                SocketCanException ex = Assert.Throws<SocketCanException>(() => bcmCanSocket.RemoveCyclicTransmissionTask(0x99, BcmCanFrameType.ClassicCAN));
                Assert.AreEqual(SocketError.InvalidArgument, ex.SocketErrorCode);
                Assert.AreEqual(22, ex.NativeErrorCode);              
            }
        }

        [Test]
        public void BcmCanSocket_QueueCyclicTransmissionTaskProperties_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i => i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.Connect(iface);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(true, bcmCanSocket.Connected);

                var canFrame = new CanFrame(0x333, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF });
                var frames = new CanFrame[] { canFrame };
                var config = new BcmCyclicTxTaskConfiguration()
                {
                    Id = 0x333,
                    StartTimer = true,
                    SetInterval = true,
                    InitialIntervalConfiguration = new BcmInitialIntervalConfiguration(10, new BcmTimeval(0, 5000)), // 10 messages at 5 ms
                    PostInitialInterval = new BcmTimeval(0, 100000), // Then at 100 ms
                };
                int nBytes = bcmCanSocket.CreateCyclicTransmissionTask(config, frames);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(72, nBytes);
                }
                else
                {
                    Assert.AreEqual(56, nBytes);
                }

                nBytes = bcmCanSocket.QueueCyclicTransmissionTaskProperties(0x333, BcmCanFrameType.ClassicCAN);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(56, nBytes);
                }
                else
                {
                    Assert.AreEqual(40, nBytes);
                }
            }
        }

        [Test]
        public void BcmCanSocket_CANFD_QueueCyclicTransmissionTaskProperties_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i => i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.Connect(iface);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(true, bcmCanSocket.Connected);

                var canFrame = new CanFdFrame(0x312, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF }, CanFdFlags.CANFD_BRS);
                var frames = new CanFdFrame[] { canFrame };
                var config = new BcmCyclicTxTaskConfiguration()
                {
                    Id = 0x312,
                    StartTimer = true,
                    SetInterval = true,
                    InitialIntervalConfiguration = new BcmInitialIntervalConfiguration(10, new BcmTimeval(0, 5000)), // 10 messages at 5 ms
                    PostInitialInterval = new BcmTimeval(0, 100000), // Then at 100 ms
                };
                int nBytes = bcmCanSocket.CreateCyclicTransmissionTask(config, frames);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(128, nBytes);
                }
                else
                {
                    Assert.AreEqual(112, nBytes);
                }

                nBytes = bcmCanSocket.QueueCyclicTransmissionTaskProperties(0x312, BcmCanFrameType.CANFD);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(56, nBytes);
                }
                else
                {
                    Assert.AreEqual(40, nBytes);
                }
            }
        }

        [Test]
        public void BcmCanSocket_QueueCyclicTransmissionTaskProperties_NotFound_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i => i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.Connect(iface);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(true, bcmCanSocket.Connected);

                var canFrame = new CanFrame(0x333, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF });
                var frames = new CanFrame[] { canFrame };
                var config = new BcmCyclicTxTaskConfiguration()
                {
                    Id = 0x333,
                    StartTimer = true,
                    SetInterval = true,
                    InitialIntervalConfiguration = new BcmInitialIntervalConfiguration(10, new BcmTimeval(0, 5000)), // 10 messages at 5 ms
                    PostInitialInterval = new BcmTimeval(0, 100000), // Then at 100 ms
                };
                int nBytes = bcmCanSocket.CreateCyclicTransmissionTask(config, frames);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(72, nBytes);
                }
                else
                {
                    Assert.AreEqual(56, nBytes);
                }

                SocketCanException ex = Assert.Throws<SocketCanException>(() => bcmCanSocket.QueueCyclicTransmissionTaskProperties(0xF1, BcmCanFrameType.ClassicCAN));
                Assert.AreEqual(SocketError.InvalidArgument, ex.SocketErrorCode);
                Assert.AreEqual(22, ex.NativeErrorCode);   
            }
        }

        [Test]
        public void BcmCanSocket_Read_CyclicTransmissionTaskProperties_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i => i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.Connect(iface);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(true, bcmCanSocket.Connected);

                var canFrame = new CanFrame(0x333, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF });
                var frames = new CanFrame[] { canFrame };
                var config = new BcmCyclicTxTaskConfiguration()
                {
                    Id = 0x333,
                    StartTimer = true,
                    SetInterval = true,
                    InitialIntervalConfiguration = new BcmInitialIntervalConfiguration(10, new BcmTimeval(0, 5000)), // 10 messages at 5 ms
                    PostInitialInterval = new BcmTimeval(0, 100000), // Then at 100 ms
                };
                int nBytes = bcmCanSocket.CreateCyclicTransmissionTask(config, frames);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(72, nBytes);
                }
                else
                {
                    Assert.AreEqual(56, nBytes);
                }

                nBytes = bcmCanSocket.QueueCyclicTransmissionTaskProperties(0x333, BcmCanFrameType.ClassicCAN);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(56, nBytes);
                }
                else
                {
                    Assert.AreEqual(40, nBytes);
                }

                nBytes = bcmCanSocket.Read(out BcmCanMessageResponse response);
                Assert.AreEqual(BcmCanFrameType.ClassicCAN, response.FrameType);
                Assert.AreEqual(BcmResponseType.TransmissionTaskConfiguration, response.ResponseType);
                Assert.AreEqual(0x333, response.CyclicTransmissionTaskConfiguration.Id);
                Assert.IsNotNull(response.ClassicFrames);
                Assert.AreEqual(1, response.ClassicFrames.Length);
                CanFrame firstFrame = response.ClassicFrames.First();
                Assert.AreEqual(0x333, firstFrame.CanId);
                Assert.IsTrue(firstFrame.Data.Take(firstFrame.Length).SequenceEqual(new byte[] { 0xDE, 0xAD, 0xBE, 0xEF }));
                Assert.AreEqual(true, response.CyclicTransmissionTaskConfiguration.StartTimer);
                Assert.AreEqual(true, response.CyclicTransmissionTaskConfiguration.SetInterval);
                Assert.AreEqual(false, response.CyclicTransmissionTaskConfiguration.CopyCanIdInHeaderToEachCanFrame);
                Assert.AreEqual(true, response.CyclicTransmissionTaskConfiguration.ImmediatelyQueueNewFrame); // Will read as True as StartTimer is set to True
                Assert.AreEqual(false, response.CyclicTransmissionTaskConfiguration.NotifyWhenFirstIntervalComplete);
                Assert.AreEqual(false, response.CyclicTransmissionTaskConfiguration.RestartMultipleFrameTxAtIndexZero);
                Assert.GreaterOrEqual(10, response.CyclicTransmissionTaskConfiguration.InitialIntervalConfiguration.Count);
                Assert.AreEqual(0, response.CyclicTransmissionTaskConfiguration.InitialIntervalConfiguration.Interval.Seconds);
                Assert.AreEqual(5000, response.CyclicTransmissionTaskConfiguration.InitialIntervalConfiguration.Interval.Microseconds);
                Assert.AreEqual(0, response.CyclicTransmissionTaskConfiguration.PostInitialInterval.Seconds);
                Assert.AreEqual(100000, response.CyclicTransmissionTaskConfiguration.PostInitialInterval.Microseconds);
            }
        }

        [Test]
        public void BcmCanSocket_CANFD_Read_CyclicTransmissionTaskProperties_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i => i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.Connect(iface);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(true, bcmCanSocket.Connected);

                var canFrame = new CanFdFrame(0x312, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF }, CanFdFlags.CANFD_BRS);
                var frames = new CanFdFrame[] { canFrame };
                var config = new BcmCyclicTxTaskConfiguration()
                {
                    Id = 0x312,
                    StartTimer = true,
                    SetInterval = true,
                    InitialIntervalConfiguration = new BcmInitialIntervalConfiguration(10, new BcmTimeval(0, 5000)), // 10 messages at 5 ms
                    PostInitialInterval = new BcmTimeval(0, 100000), // Then at 100 ms
                };
                int nBytes = bcmCanSocket.CreateCyclicTransmissionTask(config, frames);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(128, nBytes);
                }
                else
                {
                    Assert.AreEqual(112, nBytes);
                }

                nBytes = bcmCanSocket.QueueCyclicTransmissionTaskProperties(0x312, BcmCanFrameType.CANFD);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(56, nBytes);
                }
                else
                {
                    Assert.AreEqual(40, nBytes);
                }

                nBytes = bcmCanSocket.Read(out BcmCanMessageResponse response);
                Assert.AreEqual(BcmCanFrameType.CANFD, response.FrameType);
                Assert.AreEqual(BcmResponseType.TransmissionTaskConfiguration, response.ResponseType);
                Assert.AreEqual(0x312, response.CyclicTransmissionTaskConfiguration.Id);
                Assert.IsNotNull(response.FdFrames);
                Assert.AreEqual(1, response.FdFrames.Length);
                CanFdFrame firstFrame = response.FdFrames.First();
                Assert.AreEqual(0x312, firstFrame.CanId);
                Assert.IsTrue(firstFrame.Flags.HasFlag(CanFdFlags.CANFD_BRS));
                Assert.IsTrue(firstFrame.Data.Take(firstFrame.Length).SequenceEqual(new byte[] { 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF }));
                Assert.AreEqual(true, response.CyclicTransmissionTaskConfiguration.StartTimer);
                Assert.AreEqual(true, response.CyclicTransmissionTaskConfiguration.SetInterval);
                Assert.AreEqual(false, response.CyclicTransmissionTaskConfiguration.CopyCanIdInHeaderToEachCanFrame);
                Assert.AreEqual(true, response.CyclicTransmissionTaskConfiguration.ImmediatelyQueueNewFrame); // Will read as True as StartTimer is set to True
                Assert.AreEqual(false, response.CyclicTransmissionTaskConfiguration.NotifyWhenFirstIntervalComplete);
                Assert.AreEqual(false, response.CyclicTransmissionTaskConfiguration.RestartMultipleFrameTxAtIndexZero);
                Assert.GreaterOrEqual(10, response.CyclicTransmissionTaskConfiguration.InitialIntervalConfiguration.Count);
                Assert.AreEqual(0, response.CyclicTransmissionTaskConfiguration.InitialIntervalConfiguration.Interval.Seconds);
                Assert.AreEqual(5000, response.CyclicTransmissionTaskConfiguration.InitialIntervalConfiguration.Interval.Microseconds);
                Assert.AreEqual(0, response.CyclicTransmissionTaskConfiguration.PostInitialInterval.Seconds);
                Assert.AreEqual(100000, response.CyclicTransmissionTaskConfiguration.PostInitialInterval.Microseconds);
            }
        }

        [Test]
        public void BcmCanSocket_SendSingleFrame_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.Connect(iface);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(true, bcmCanSocket.Connected);

                var canFrame = new CanFrame(0x444, new byte[] { 0xBE, 0xEF });
                int nBytes = bcmCanSocket.SendSingleFrame(canFrame);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(72, nBytes);
                }
                else
                {
                    Assert.AreEqual(56, nBytes);
                }
            }
        }

        [Test]
        public void BcmCanSocket_SendSingleFrame_InvalidFrame_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.Connect(iface);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(true, bcmCanSocket.Connected);

                var canFrame = new CanFrame(0x444, new byte[] { 0xBE, 0xEF });
                canFrame.Length = 200; // invalid length
                SocketCanException ex = Assert.Throws<SocketCanException>(() => bcmCanSocket.SendSingleFrame(canFrame));
                Assert.AreEqual(SocketError.InvalidArgument, ex.SocketErrorCode);
                Assert.AreEqual(22, ex.NativeErrorCode);   
            }
        }

        [Test]
        public void BcmCanSocket_SendSingleFrame_NoDevice_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            iface.Index = 0; // invalid device id
            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.Connect(iface);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(true, bcmCanSocket.Connected);

                var canFrame = new CanFrame(0x444, new byte[] { 0xBE, 0xEF });
                SocketCanException ex = Assert.Throws<SocketCanException>(() => bcmCanSocket.SendSingleFrame(canFrame));
                Assert.AreEqual(SocketError.SocketError, ex.SocketErrorCode);
                Assert.AreEqual(19, ex.NativeErrorCode);
            }
        }

        [Test]
        public void BcmCanSocket_CANFD_SendSingleFrame_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.Connect(iface);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(true, bcmCanSocket.Connected);

                var canFrame = new CanFdFrame(0x313, new byte[] { 0xBE, 0xEF, 0xBE, 0xEF, 0xBE, 0xEF, 0xBE, 0xEF, 0xBE, 0xEF, 0xBE, 0xEF }, CanFdFlags.CANFD_BRS);
                int nBytes = bcmCanSocket.SendSingleFrame(canFrame);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(128, nBytes);
                }
                else
                {
                    Assert.AreEqual(112, nBytes);
                }
            }
        }

        [Test]
        public void BcmCanSocket_Read_FirstIntervalTransmissionComplete_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i => i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.ReceiveTimeout = 500;
                bcmCanSocket.Connect(iface);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(true, bcmCanSocket.Connected);

                var canFrame = new CanFrame(0x15, new byte[] { 0xBE, 0xEE });
                var frames = new CanFrame[] { canFrame };
                var config = new BcmCyclicTxTaskConfiguration()
                {
                    Id = 0x15,
                    StartTimer = true,
                    SetInterval = true,
                    NotifyWhenFirstIntervalComplete = true,
                    InitialIntervalConfiguration = new BcmInitialIntervalConfiguration(3, new BcmTimeval(0, 5000)), // 3 messages at 5 ms
                    PostInitialInterval = new BcmTimeval(0, 100000), // Then at 100 ms
                };
                int nBytes = bcmCanSocket.CreateCyclicTransmissionTask(config, frames);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(72, nBytes);
                }
                else
                {
                    Assert.AreEqual(56, nBytes);
                }

                nBytes = bcmCanSocket.Read(out BcmCanMessageResponse response);
                Assert.AreEqual(BcmCanFrameType.ClassicCAN, response.FrameType);
                Assert.AreEqual(BcmResponseType.FirstIntervalTransmissionComplete, response.ResponseType);
                Assert.AreEqual(0x15, response.CyclicTransmissionTaskConfiguration.Id);
                Assert.AreEqual(true, response.CyclicTransmissionTaskConfiguration.StartTimer);
                Assert.AreEqual(true, response.CyclicTransmissionTaskConfiguration.SetInterval);
                Assert.AreEqual(false, response.CyclicTransmissionTaskConfiguration.CopyCanIdInHeaderToEachCanFrame);
                Assert.AreEqual(true, response.CyclicTransmissionTaskConfiguration.ImmediatelyQueueNewFrame); // Will read as True as StartTimer is set to True
                Assert.AreEqual(true, response.CyclicTransmissionTaskConfiguration.NotifyWhenFirstIntervalComplete);
                Assert.AreEqual(false, response.CyclicTransmissionTaskConfiguration.RestartMultipleFrameTxAtIndexZero);
                Assert.AreEqual(0, response.CyclicTransmissionTaskConfiguration.InitialIntervalConfiguration.Count);
                Assert.AreEqual(0, response.CyclicTransmissionTaskConfiguration.InitialIntervalConfiguration.Interval.Seconds);
                Assert.AreEqual(5000, response.CyclicTransmissionTaskConfiguration.InitialIntervalConfiguration.Interval.Microseconds);
                Assert.AreEqual(0, response.CyclicTransmissionTaskConfiguration.PostInitialInterval.Seconds);
                Assert.AreEqual(100000, response.CyclicTransmissionTaskConfiguration.PostInitialInterval.Microseconds);
            }
        }

        [Test]
        public void BcmCanSocket_CANFD_Read_FirstIntervalTransmissionComplete_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i => i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.ReceiveTimeout = 500;
                bcmCanSocket.Connect(iface);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(true, bcmCanSocket.Connected);

                var canFrame = new CanFdFrame(0x16, new byte[] { 0x0B, 0xEE }, CanFdFlags.CANFD_BRS);
                var frames = new CanFdFrame[] { canFrame };
                var config = new BcmCyclicTxTaskConfiguration()
                {
                    Id = 0x16,
                    StartTimer = true,
                    SetInterval = true,
                    NotifyWhenFirstIntervalComplete = true,
                    InitialIntervalConfiguration = new BcmInitialIntervalConfiguration(3, new BcmTimeval(0, 5000)), // 3 messages at 5 ms
                    PostInitialInterval = new BcmTimeval(0, 100000), // Then at 100 ms
                };
                int nBytes = bcmCanSocket.CreateCyclicTransmissionTask(config, frames);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(128, nBytes);
                }
                else
                {
                    Assert.AreEqual(112, nBytes);
                }

                nBytes = bcmCanSocket.Read(out BcmCanMessageResponse response);
                Assert.AreEqual(BcmCanFrameType.CANFD, response.FrameType);
                Assert.AreEqual(BcmResponseType.FirstIntervalTransmissionComplete, response.ResponseType);
                Assert.AreEqual(0x16, response.CyclicTransmissionTaskConfiguration.Id);
                Assert.AreEqual(true, response.CyclicTransmissionTaskConfiguration.StartTimer);
                Assert.AreEqual(true, response.CyclicTransmissionTaskConfiguration.SetInterval);
                Assert.AreEqual(false, response.CyclicTransmissionTaskConfiguration.CopyCanIdInHeaderToEachCanFrame);
                Assert.AreEqual(true, response.CyclicTransmissionTaskConfiguration.ImmediatelyQueueNewFrame); // Will read as True as StartTimer is set to True
                Assert.AreEqual(true, response.CyclicTransmissionTaskConfiguration.NotifyWhenFirstIntervalComplete);
                Assert.AreEqual(false, response.CyclicTransmissionTaskConfiguration.RestartMultipleFrameTxAtIndexZero);
                Assert.AreEqual(0, response.CyclicTransmissionTaskConfiguration.InitialIntervalConfiguration.Count);
                Assert.AreEqual(0, response.CyclicTransmissionTaskConfiguration.InitialIntervalConfiguration.Interval.Seconds);
                Assert.AreEqual(5000, response.CyclicTransmissionTaskConfiguration.InitialIntervalConfiguration.Interval.Microseconds);
                Assert.AreEqual(0, response.CyclicTransmissionTaskConfiguration.PostInitialInterval.Seconds);
                Assert.AreEqual(100000, response.CyclicTransmissionTaskConfiguration.PostInitialInterval.Microseconds);
            }
        }

        [Test]
        public void BcmCanSocket_CreateReceiveFilterSubscription_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.Connect(iface);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(true, bcmCanSocket.Connected);

                var canFrame = new CanFrame(0x189, new byte[] { 0x01 });
                var frames = new CanFrame[] { canFrame };
                var subscription = new BcmContentRxFilterSubscription()
                {
                    Id = 0x189,
                    StartTimer = true,
                    SetInterval = true,
                    ReceiveTimeout = new BcmTimeval(0, 100000), // 100 ms
                    ReceiveMessageRateLimit = new BcmTimeval(0, 0), // no throttle
                };
                int nBytes = bcmCanSocket.CreateReceiveFilterSubscription(subscription, frames);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(72, nBytes);
                }
                else
                {
                    Assert.AreEqual(56, nBytes);
                }
            }
        }

        [Test]
        public void BcmCanSocket_CreateReceiveFilterSubscription_InvalidTime_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.Connect(iface);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(true, bcmCanSocket.Connected);

                var canFrame = new CanFrame(0x189, new byte[] { 0x01 });
                var frames = new CanFrame[] { canFrame };
                var subscription = new BcmContentRxFilterSubscription()
                {
                    Id = 0x189,
                    StartTimer = true,
                    SetInterval = true,
                    ReceiveTimeout = new BcmTimeval(0, -1), // invalid time
                    ReceiveMessageRateLimit = new BcmTimeval(-1, 0), // invalid time
                };

                SocketCanException ex = Assert.Throws<SocketCanException>(() => bcmCanSocket.CreateReceiveFilterSubscription(subscription, frames));
                Assert.AreEqual(SocketError.InvalidArgument, ex.SocketErrorCode);
                Assert.AreEqual(22, ex.NativeErrorCode);
            }
        }

        [Test]
        public void BcmCanSocket_CANFD_CreateReceiveFilterSubscription_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.Connect(iface);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(true, bcmCanSocket.Connected);

                var canFrame = new CanFdFrame(0x18, new byte[] { 0xDD, 0xFF }, CanFdFlags.CANFD_BRS);
                var frames = new CanFdFrame[] { canFrame };
                var subscription = new BcmContentRxFilterSubscription()
                {
                    Id = 0x18,
                    StartTimer = true,
                    SetInterval = true,
                    ReceiveTimeout = new BcmTimeval(0, 100000), // 100 ms
                    ReceiveMessageRateLimit = new BcmTimeval(0, 0), // no throttle
                };
                int nBytes = bcmCanSocket.CreateReceiveFilterSubscription(subscription, frames);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(128, nBytes);
                }
                else
                {
                    Assert.AreEqual(112, nBytes);
                }
            }
        }

        [Test]
        public void BcmCanSocket_RemoveReceiveFilterSubscription_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.Connect(iface);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(true, bcmCanSocket.Connected);

                var canFrame = new CanFrame(0x189, new byte[] { 0x01 });
                var frames = new CanFrame[] { canFrame };
                var subscription = new BcmContentRxFilterSubscription()
                {
                    Id = 0x189,
                    StartTimer = true,
                    SetInterval = true,
                    ReceiveTimeout = new BcmTimeval(0, 100000), // 100 ms
                    ReceiveMessageRateLimit = new BcmTimeval(0, 0), // no throttle
                };
                int nBytes = bcmCanSocket.CreateReceiveFilterSubscription(subscription, frames);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(72, nBytes);
                }
                else
                {
                    Assert.AreEqual(56, nBytes);
                }

                nBytes = bcmCanSocket.RemoveReceiveFilterSubscription(0x189, BcmCanFrameType.ClassicCAN);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(56, nBytes);
                }
                else
                {
                    Assert.AreEqual(40, nBytes);
                }
            }
        }

        [Test]
        public void BcmCanSocket_CANFD_RemoveReceiveFilterSubscription_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.Connect(iface);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(true, bcmCanSocket.Connected);

                var canFrame = new CanFdFrame(0x18, new byte[] { 0xDD, 0xFF }, CanFdFlags.CANFD_BRS);
                var frames = new CanFdFrame[] { canFrame };
                var subscription = new BcmContentRxFilterSubscription()
                {
                    Id = 0x18,
                    StartTimer = true,
                    SetInterval = true,
                    ReceiveTimeout = new BcmTimeval(0, 100000), // 100 ms
                    ReceiveMessageRateLimit = new BcmTimeval(0, 0), // no throttle
                };
                int nBytes = bcmCanSocket.CreateReceiveFilterSubscription(subscription, frames);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(128, nBytes);
                }
                else
                {
                    Assert.AreEqual(112, nBytes);
                }

                nBytes = bcmCanSocket.RemoveReceiveFilterSubscription(0x18, BcmCanFrameType.CANFD);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(56, nBytes);
                }
                else
                {
                    Assert.AreEqual(40, nBytes);
                }
            }
        }

        [Test]
        public void BcmCanSocket_RemoveReceiveFilterSubscription_NotFound_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.Connect(iface);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(true, bcmCanSocket.Connected);

                var canFrame = new CanFrame(0x189, new byte[] { 0x01 });
                var frames = new CanFrame[] { canFrame };
                var subscription = new BcmContentRxFilterSubscription()
                {
                    Id = 0x189,
                    StartTimer = true,
                    SetInterval = true,
                    ReceiveTimeout = new BcmTimeval(0, 100000), // 100 ms
                    ReceiveMessageRateLimit = new BcmTimeval(0, 0), // no throttle
                };
                int nBytes = bcmCanSocket.CreateReceiveFilterSubscription(subscription, frames);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(72, nBytes);
                }
                else
                {
                    Assert.AreEqual(56, nBytes);
                }

                SocketCanException ex = Assert.Throws<SocketCanException>(() => bcmCanSocket.RemoveReceiveFilterSubscription(0x579, BcmCanFrameType.ClassicCAN));
                Assert.AreEqual(SocketError.InvalidArgument, ex.SocketErrorCode);
                Assert.AreEqual(22, ex.NativeErrorCode);
            }
        }

        [Test]
        public void BcmCanSocket_QueueReceiveFilterSubscriptionProperties_NotFound_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.Connect(iface);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(true, bcmCanSocket.Connected);

                var canFrame = new CanFrame(0x189, new byte[] { 0x01 });
                var frames = new CanFrame[] { canFrame };
                var subscription = new BcmContentRxFilterSubscription()
                {
                    Id = 0x189,
                    StartTimer = true,
                    SetInterval = true,
                    ReceiveTimeout = new BcmTimeval(0, 100000), // 100 ms
                    ReceiveMessageRateLimit = new BcmTimeval(0, 0), // no throttle
                };
                int nBytes = bcmCanSocket.CreateReceiveFilterSubscription(subscription, frames);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(72, nBytes);
                }
                else
                {
                    Assert.AreEqual(56, nBytes);
                }

                SocketCanException ex = Assert.Throws<SocketCanException>(() => bcmCanSocket.QueueReceiveFilterSubscriptionProperties(0xF8, BcmCanFrameType.ClassicCAN));
                Assert.AreEqual(SocketError.InvalidArgument, ex.SocketErrorCode);
                Assert.AreEqual(22, ex.NativeErrorCode);
            }
        }

        [Test]
        public void BcmCanSocket_QueueReceiveFilterSubscriptionProperties_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.Connect(iface);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(true, bcmCanSocket.Connected);

                var canFrame = new CanFrame(0x189, new byte[] { 0x01 });
                var frames = new CanFrame[] { canFrame };
                var subscription = new BcmContentRxFilterSubscription()
                {
                    Id = 0x189,
                    StartTimer = true,
                    SetInterval = true,
                    ReceiveTimeout = new BcmTimeval(0, 100000), // 100 ms
                    ReceiveMessageRateLimit = new BcmTimeval(0, 0), // no throttle
                };
                int nBytes = bcmCanSocket.CreateReceiveFilterSubscription(subscription, frames);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(72, nBytes);
                }
                else
                {
                    Assert.AreEqual(56, nBytes);
                }

                nBytes = bcmCanSocket.QueueReceiveFilterSubscriptionProperties(0x189, BcmCanFrameType.ClassicCAN);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(56, nBytes);
                }
                else
                {
                    Assert.AreEqual(40, nBytes);
                }
            }
        }

        [Test]
        public void BcmCanSocket_CANFD_QueueReceiveFilterSubscriptionProperties_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.Connect(iface);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(true, bcmCanSocket.Connected);

                var canFrame = new CanFdFrame(0x18, new byte[] { 0xDD, 0xFF }, CanFdFlags.CANFD_BRS);
                var frames = new CanFdFrame[] { canFrame };
                var subscription = new BcmContentRxFilterSubscription()
                {
                    Id = 0x18,
                    StartTimer = true,
                    SetInterval = true,
                    ReceiveTimeout = new BcmTimeval(0, 100000), // 100 ms
                    ReceiveMessageRateLimit = new BcmTimeval(0, 0), // no throttle
                };
                int nBytes = bcmCanSocket.CreateReceiveFilterSubscription(subscription, frames);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(128, nBytes);
                }
                else
                {
                    Assert.AreEqual(112, nBytes);
                }

                nBytes = bcmCanSocket.QueueReceiveFilterSubscriptionProperties(0x18, BcmCanFrameType.CANFD);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(56, nBytes);
                }
                else
                {
                    Assert.AreEqual(40, nBytes);
                }
            }
        }

        [Test]
        public void BcmCanSocket_Read_ReceiveFilterSubscriptionProperties_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.Connect(iface);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(true, bcmCanSocket.Connected);

                var canFrame = new CanFrame(0x189, new byte[] { 0x01 });
                var frames = new CanFrame[] { canFrame };
                var subscription = new BcmContentRxFilterSubscription()
                {
                    Id = 0x189,
                    StartTimer = true,
                    SetInterval = true,
                    ReceiveTimeout = new BcmTimeval(0, 100000), // 100 ms
                    ReceiveMessageRateLimit = new BcmTimeval(0, 0), // no throttle
                };
                int nBytes = bcmCanSocket.CreateReceiveFilterSubscription(subscription, frames);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(72, nBytes);
                }
                else
                {
                    Assert.AreEqual(56, nBytes);
                }

                nBytes = bcmCanSocket.QueueReceiveFilterSubscriptionProperties(0x189, BcmCanFrameType.ClassicCAN);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(56, nBytes);
                }
                else
                {
                    Assert.AreEqual(40, nBytes);
                }

                nBytes = bcmCanSocket.Read(out BcmCanMessageResponse response);
                Assert.AreEqual(BcmCanFrameType.ClassicCAN, response.FrameType);
                Assert.AreEqual(BcmResponseType.ReceiveFilterConfiguration, response.ResponseType);
                Assert.AreEqual(0x189, response.ContentReceiveFilterSubscription.Id);
                Assert.IsNotNull(response.ClassicFrames);
                Assert.AreEqual(1, response.ClassicFrames.Length);
                CanFrame firstFrame = response.ClassicFrames.First();
                Assert.AreEqual(0x189, firstFrame.CanId);
                Assert.IsTrue(firstFrame.Data.Take(firstFrame.Length).SequenceEqual(new byte[] { 0x01 }));
                Assert.AreEqual(true, response.ContentReceiveFilterSubscription.StartTimer);
                Assert.AreEqual(true, response.ContentReceiveFilterSubscription.SetInterval);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.FilterOnlyByCanId);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.MonitorLengthChanges);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.PreventAutoStartingReceiveTimer);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.ReplyToRtrRequest);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.NotifyWhenReceiveRestartsAfterTimeout);
                Assert.AreEqual(0, response.ContentReceiveFilterSubscription.ReceiveTimeout.Seconds);
                Assert.AreEqual(100000, response.ContentReceiveFilterSubscription.ReceiveTimeout.Microseconds);
                Assert.AreEqual(0, response.ContentReceiveFilterSubscription.ReceiveMessageRateLimit.Seconds);
                Assert.AreEqual(0, response.ContentReceiveFilterSubscription.ReceiveMessageRateLimit.Microseconds);
            }
        }

        [Test]
        public void BcmCanSocket_CANFD_Read_ReceiveFilterSubscriptionProperties_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.Connect(iface);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(true, bcmCanSocket.Connected);

                var canFrame = new CanFdFrame(0x18, new byte[] { 0xDD, 0xFF }, CanFdFlags.CANFD_BRS);
                var frames = new CanFdFrame[] { canFrame };
                var subscription = new BcmContentRxFilterSubscription()
                {
                    Id = 0x18,
                    StartTimer = true,
                    SetInterval = true,
                    ReceiveTimeout = new BcmTimeval(0, 100000), // 100 ms
                    ReceiveMessageRateLimit = new BcmTimeval(0, 0), // no throttle
                };
                int nBytes = bcmCanSocket.CreateReceiveFilterSubscription(subscription, frames);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(128, nBytes);
                }
                else
                {
                    Assert.AreEqual(112, nBytes);
                }

                nBytes = bcmCanSocket.QueueReceiveFilterSubscriptionProperties(0x18, BcmCanFrameType.CANFD);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(56, nBytes);
                }
                else
                {
                    Assert.AreEqual(40, nBytes);
                }

                nBytes = bcmCanSocket.Read(out BcmCanMessageResponse response);
                Assert.AreEqual(BcmCanFrameType.CANFD, response.FrameType);
                Assert.AreEqual(BcmResponseType.ReceiveFilterConfiguration, response.ResponseType);
                Assert.IsNotNull(response.FdFrames);
                Assert.AreEqual(1, response.FdFrames.Length);
                CanFdFrame firstFrame = response.FdFrames.First();
                Assert.AreEqual(0x18, firstFrame.CanId);
                Assert.IsTrue(firstFrame.Flags.HasFlag(CanFdFlags.CANFD_BRS));
                Assert.IsTrue(firstFrame.Data.Take(firstFrame.Length).SequenceEqual(new byte[] { 0xDD, 0xFF }));
                Assert.AreEqual(true, response.ContentReceiveFilterSubscription.StartTimer);
                Assert.AreEqual(true, response.ContentReceiveFilterSubscription.SetInterval);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.FilterOnlyByCanId);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.MonitorLengthChanges);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.PreventAutoStartingReceiveTimer);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.ReplyToRtrRequest);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.NotifyWhenReceiveRestartsAfterTimeout);
                Assert.AreEqual(0, response.ContentReceiveFilterSubscription.ReceiveTimeout.Seconds);
                Assert.AreEqual(100000, response.ContentReceiveFilterSubscription.ReceiveTimeout.Microseconds);
                Assert.AreEqual(0, response.ContentReceiveFilterSubscription.ReceiveMessageRateLimit.Seconds);
                Assert.AreEqual(0, response.ContentReceiveFilterSubscription.ReceiveMessageRateLimit.Microseconds);
            }
        }

        [Test]
        public void BcmCanSocket_Read_CyclicMessageTimeout_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.Connect(iface);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(true, bcmCanSocket.Connected);

                var canFrame = new CanFrame(0x189, new byte[] { 0x01 });
                var frames = new CanFrame[] { canFrame };
                var subscription = new BcmContentRxFilterSubscription()
                {
                    Id = 0x189,
                    StartTimer = true,
                    SetInterval = true,
                    ReceiveTimeout = new BcmTimeval(0, 100000), // 100 ms
                    ReceiveMessageRateLimit = new BcmTimeval(0, 0), // no throttle
                };
                int nBytes = bcmCanSocket.CreateReceiveFilterSubscription(subscription, frames);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(72, nBytes);
                }
                else
                {
                    Assert.AreEqual(56, nBytes);
                }
                System.Threading.Thread.Sleep(200);

                nBytes = bcmCanSocket.Read(out BcmCanMessageResponse response);
                Assert.AreEqual(BcmCanFrameType.ClassicCAN, response.FrameType);
                Assert.AreEqual(BcmResponseType.CyclicMessageReceiveTimeout, response.ResponseType);
                Assert.AreEqual(0x189, response.ContentReceiveFilterSubscription.Id);
                Assert.IsNull(response.ClassicFrames);
                Assert.AreEqual(true, response.ContentReceiveFilterSubscription.StartTimer);
                Assert.AreEqual(true, response.ContentReceiveFilterSubscription.SetInterval);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.FilterOnlyByCanId);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.MonitorLengthChanges);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.PreventAutoStartingReceiveTimer);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.ReplyToRtrRequest);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.NotifyWhenReceiveRestartsAfterTimeout);
                Assert.AreEqual(0, response.ContentReceiveFilterSubscription.ReceiveTimeout.Seconds);
                Assert.AreEqual(100000, response.ContentReceiveFilterSubscription.ReceiveTimeout.Microseconds);
                Assert.AreEqual(0, response.ContentReceiveFilterSubscription.ReceiveMessageRateLimit.Seconds);
                Assert.AreEqual(0, response.ContentReceiveFilterSubscription.ReceiveMessageRateLimit.Microseconds);
            }
        }

        [Test]
        public void BcmCanSocket_CANFD_Read_CyclicMessageTimeout_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.Connect(iface);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(true, bcmCanSocket.Connected);

                var canFrame = new CanFdFrame((uint)CanIdFlags.CAN_EFF_FLAG | 0x15FFDDF0, new byte[] { 0x00, 0x01, 0x02 }, CanFdFlags.CANFD_BRS);
                var frames = new CanFdFrame[] { canFrame };
                var subscription = new BcmContentRxFilterSubscription()
                {
                    Id = (uint)CanIdFlags.CAN_EFF_FLAG | 0x15FFDDF0,
                    StartTimer = true,
                    SetInterval = true,
                    ReceiveTimeout = new BcmTimeval(0, 100000), // 100 ms
                    ReceiveMessageRateLimit = new BcmTimeval(0, 0), // no throttle
                };
                int nBytes = bcmCanSocket.CreateReceiveFilterSubscription(subscription, frames);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(128, nBytes);
                }
                else
                {
                    Assert.AreEqual(112, nBytes);
                }
                System.Threading.Thread.Sleep(200);

                nBytes = bcmCanSocket.Read(out BcmCanMessageResponse response);
                Assert.AreEqual(BcmCanFrameType.CANFD, response.FrameType);
                Assert.AreEqual(BcmResponseType.CyclicMessageReceiveTimeout, response.ResponseType);
                Assert.AreEqual((uint)CanIdFlags.CAN_EFF_FLAG | 0x15FFDDF0, response.ContentReceiveFilterSubscription.Id);
                Assert.IsNull(response.ClassicFrames);
                Assert.AreEqual(true, response.ContentReceiveFilterSubscription.StartTimer);
                Assert.AreEqual(true, response.ContentReceiveFilterSubscription.SetInterval);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.FilterOnlyByCanId);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.MonitorLengthChanges);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.PreventAutoStartingReceiveTimer);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.ReplyToRtrRequest);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.NotifyWhenReceiveRestartsAfterTimeout);
                Assert.AreEqual(0, response.ContentReceiveFilterSubscription.ReceiveTimeout.Seconds);
                Assert.AreEqual(100000, response.ContentReceiveFilterSubscription.ReceiveTimeout.Microseconds);
                Assert.AreEqual(0, response.ContentReceiveFilterSubscription.ReceiveMessageRateLimit.Seconds);
                Assert.AreEqual(0, response.ContentReceiveFilterSubscription.ReceiveMessageRateLimit.Microseconds);
            }
        }

        [Test]
        public void BcmCanSocket_Read_CanFrameUpdateNotification_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.Connect(iface);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(true, bcmCanSocket.Connected);

                var canFrame = new CanFrame(0x189, new byte[] { 0x01 });
                var frames = new CanFrame[] { canFrame };
                var subscription = new BcmContentRxFilterSubscription()
                {
                    Id = 0x189,
                    StartTimer = true,
                    SetInterval = true,
                    ReceiveTimeout = new BcmTimeval(0, 100000), // 100 ms
                    ReceiveMessageRateLimit = new BcmTimeval(0, 0), // no throttle
                };
                int nBytes = bcmCanSocket.CreateReceiveFilterSubscription(subscription, frames);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(72, nBytes);
                }
                else
                {
                    Assert.AreEqual(56, nBytes);
                }

                using (var rawCanSocket = new RawCanSocket())
                {
                    rawCanSocket.Bind(iface);
                    int bytesWritten = rawCanSocket.Write(new CanFrame(0x189, new byte[] { 0x02 }));
                    Assert.AreEqual(16, bytesWritten);
                }

                System.Threading.Thread.Sleep(200);

                nBytes = bcmCanSocket.Read(out BcmCanMessageResponse response);
                Assert.AreEqual(BcmCanFrameType.ClassicCAN, response.FrameType);
                Assert.AreEqual(BcmResponseType.CanFrameReceiveUpdateNotification, response.ResponseType);
                Assert.AreEqual(0x189, response.ContentReceiveFilterSubscription.Id);
                Assert.IsNotNull(response.ClassicFrames);
                Assert.AreEqual(1, response.ClassicFrames.Length);
                CanFrame firstFrame = response.ClassicFrames.First();
                Assert.AreEqual(0x189, firstFrame.CanId);
                Assert.IsTrue(firstFrame.Data.Take(firstFrame.Length).SequenceEqual(new byte[] { 0x02 }));
                Assert.AreEqual(true, response.ContentReceiveFilterSubscription.StartTimer);
                Assert.AreEqual(true, response.ContentReceiveFilterSubscription.SetInterval);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.FilterOnlyByCanId);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.MonitorLengthChanges);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.PreventAutoStartingReceiveTimer);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.ReplyToRtrRequest);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.NotifyWhenReceiveRestartsAfterTimeout);
                Assert.AreEqual(0, response.ContentReceiveFilterSubscription.ReceiveTimeout.Seconds);
                Assert.AreEqual(100000, response.ContentReceiveFilterSubscription.ReceiveTimeout.Microseconds);
                Assert.AreEqual(0, response.ContentReceiveFilterSubscription.ReceiveMessageRateLimit.Seconds);
                Assert.AreEqual(0, response.ContentReceiveFilterSubscription.ReceiveMessageRateLimit.Microseconds);
            }
        }

        [Test]
        public void BcmCanSocket_CANFD_Read_CanFrameUpdateNotification_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.Connect(iface);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(true, bcmCanSocket.Connected);

                var canFrame = new CanFdFrame((uint)CanIdFlags.CAN_EFF_FLAG | 0x15FFDDF0, new byte[] { 0x00, 0x01, 0x02 }, CanFdFlags.CANFD_BRS);
                var frames = new CanFdFrame[] { canFrame };
                var subscription = new BcmContentRxFilterSubscription()
                {
                    Id = (uint)CanIdFlags.CAN_EFF_FLAG | 0x15FFDDF0,
                    StartTimer = true,
                    SetInterval = true,
                    ReceiveTimeout = new BcmTimeval(0, 100000), // 100 ms
                    ReceiveMessageRateLimit = new BcmTimeval(0, 0), // no throttle
                };
                int nBytes = bcmCanSocket.CreateReceiveFilterSubscription(subscription, frames);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(128, nBytes);
                }
                else
                {
                    Assert.AreEqual(112, nBytes);
                }

                using (var rawCanSocket = new RawCanSocket())
                {
                    rawCanSocket.EnableCanFdFrames = true;
                    rawCanSocket.Bind(iface);
                    int bytesWritten = rawCanSocket.Write(new CanFdFrame((uint)CanIdFlags.CAN_EFF_FLAG | 0x15FFDDF0, new byte[] { 0x00, 0x01, 0x02 }, CanFdFlags.CANFD_BRS));
                    Assert.AreEqual(72, bytesWritten);
                }

                System.Threading.Thread.Sleep(200);

                nBytes = bcmCanSocket.Read(out BcmCanMessageResponse response);
                Assert.AreEqual(BcmCanFrameType.CANFD, response.FrameType);
                Assert.AreEqual(BcmResponseType.CanFrameReceiveUpdateNotification, response.ResponseType);
                Assert.AreEqual((uint)CanIdFlags.CAN_EFF_FLAG | 0x15FFDDF0, response.ContentReceiveFilterSubscription.Id);
                Assert.IsNotNull(response.FdFrames);
                Assert.AreEqual(1, response.FdFrames.Length);
                CanFdFrame firstFrame = response.FdFrames.First();
                Assert.AreEqual((uint)CanIdFlags.CAN_EFF_FLAG | 0x15FFDDF0, firstFrame.CanId);
                Assert.IsTrue(firstFrame.Data.Take(firstFrame.Length).SequenceEqual(new byte[] { 0x00, 0x01, 0x02 }));
                Assert.IsTrue(firstFrame.Flags.HasFlag(CanFdFlags.CANFD_BRS));
                Assert.AreEqual(true, response.ContentReceiveFilterSubscription.StartTimer);
                Assert.AreEqual(true, response.ContentReceiveFilterSubscription.SetInterval);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.FilterOnlyByCanId);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.MonitorLengthChanges);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.PreventAutoStartingReceiveTimer);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.ReplyToRtrRequest);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.NotifyWhenReceiveRestartsAfterTimeout);
                Assert.AreEqual(0, response.ContentReceiveFilterSubscription.ReceiveTimeout.Seconds);
                Assert.AreEqual(100000, response.ContentReceiveFilterSubscription.ReceiveTimeout.Microseconds);
                Assert.AreEqual(0, response.ContentReceiveFilterSubscription.ReceiveMessageRateLimit.Seconds);
                Assert.AreEqual(0, response.ContentReceiveFilterSubscription.ReceiveMessageRateLimit.Microseconds);
            }
        }

        [Test]
        public void BcmCanSocket_Read_CyclicMessageTimeout_CanIdOnly_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.Connect(iface);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(true, bcmCanSocket.Connected);

                var subscription = new BcmContentRxFilterSubscription()
                {
                    Id = 0x189,
                    StartTimer = true,
                    SetInterval = true,
                    FilterOnlyByCanId = true,
                    ReceiveTimeout = new BcmTimeval(0, 100000), // 100 ms
                    ReceiveMessageRateLimit = new BcmTimeval(0, 0), // no throttle
                };
                int nBytes = bcmCanSocket.CreateReceiveFilterSubscription(subscription, new CanFrame[0]);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(56, nBytes);
                }
                else
                {
                    Assert.AreEqual(40, nBytes);
                }
                System.Threading.Thread.Sleep(200);

                nBytes = bcmCanSocket.Read(out BcmCanMessageResponse response);
                Assert.AreEqual(BcmCanFrameType.ClassicCAN, response.FrameType);
                Assert.AreEqual(BcmResponseType.CyclicMessageReceiveTimeout, response.ResponseType);
                Assert.AreEqual(0x189, response.ContentReceiveFilterSubscription.Id);
                Assert.IsNull(response.ClassicFrames);
                Assert.AreEqual(true, response.ContentReceiveFilterSubscription.StartTimer);
                Assert.AreEqual(true, response.ContentReceiveFilterSubscription.SetInterval);
                Assert.AreEqual(true, response.ContentReceiveFilterSubscription.FilterOnlyByCanId);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.MonitorLengthChanges);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.PreventAutoStartingReceiveTimer);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.ReplyToRtrRequest);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.NotifyWhenReceiveRestartsAfterTimeout);
                Assert.AreEqual(0, response.ContentReceiveFilterSubscription.ReceiveTimeout.Seconds);
                Assert.AreEqual(100000, response.ContentReceiveFilterSubscription.ReceiveTimeout.Microseconds);
                Assert.AreEqual(0, response.ContentReceiveFilterSubscription.ReceiveMessageRateLimit.Seconds);
                Assert.AreEqual(0, response.ContentReceiveFilterSubscription.ReceiveMessageRateLimit.Microseconds);
            }
        }

        [Test]
        public void BcmCanSocket_Read_CanFrameUpdateNotification_CanIdOnly_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.Connect(iface);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(true, bcmCanSocket.Connected);

                var subscription = new BcmContentRxFilterSubscription()
                {
                    Id = 0x189,
                    StartTimer = true,
                    SetInterval = true,
                    ReceiveTimeout = new BcmTimeval(0, 100000), // 100 ms
                    ReceiveMessageRateLimit = new BcmTimeval(0, 0), // no throttle
                };
                int nBytes = bcmCanSocket.CreateReceiveFilterSubscription(subscription, new CanFrame[0]);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(56, nBytes);
                }
                else
                {
                    Assert.AreEqual(40, nBytes);
                }

                using (var rawCanSocket = new RawCanSocket())
                {
                    rawCanSocket.Bind(iface);
                    int bytesWritten = rawCanSocket.Write(new CanFrame(0x189, new byte[] { 0x02 }));
                    Assert.AreEqual(16, bytesWritten);
                }

                System.Threading.Thread.Sleep(200);

                nBytes = bcmCanSocket.Read(out BcmCanMessageResponse response);
                Assert.AreEqual(BcmCanFrameType.ClassicCAN, response.FrameType);
                Assert.AreEqual(BcmResponseType.CanFrameReceiveUpdateNotification, response.ResponseType);
                Assert.AreEqual(0x189, response.ContentReceiveFilterSubscription.Id);
                Assert.IsNotNull(response.ClassicFrames);
                Assert.AreEqual(1, response.ClassicFrames.Length);
                CanFrame firstFrame = response.ClassicFrames.First();
                Assert.AreEqual(0x189, firstFrame.CanId);
                Assert.IsTrue(firstFrame.Data.Take(firstFrame.Length).SequenceEqual(new byte[] { 0x02 }));
                Assert.AreEqual(true, response.ContentReceiveFilterSubscription.StartTimer);
                Assert.AreEqual(true, response.ContentReceiveFilterSubscription.SetInterval);
                Assert.AreEqual(true, response.ContentReceiveFilterSubscription.FilterOnlyByCanId);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.MonitorLengthChanges);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.PreventAutoStartingReceiveTimer);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.ReplyToRtrRequest);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.NotifyWhenReceiveRestartsAfterTimeout);
                Assert.AreEqual(0, response.ContentReceiveFilterSubscription.ReceiveTimeout.Seconds);
                Assert.AreEqual(100000, response.ContentReceiveFilterSubscription.ReceiveTimeout.Microseconds);
                Assert.AreEqual(0, response.ContentReceiveFilterSubscription.ReceiveMessageRateLimit.Seconds);
                Assert.AreEqual(0, response.ContentReceiveFilterSubscription.ReceiveMessageRateLimit.Microseconds);
            }
        }

        [Test]
        public void BcmCanSocket_Read_CanFrameUpdateNotification_LengthChange_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.Connect(iface);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(true, bcmCanSocket.Connected);

                var canFrame = new CanFrame(0x189, new byte[] { 0x01 });
                var frames = new CanFrame[] { canFrame };
                var subscription = new BcmContentRxFilterSubscription()
                {
                    Id = 0x189,
                    StartTimer = true,
                    SetInterval = true,
                    MonitorLengthChanges = true,
                    ReceiveTimeout = new BcmTimeval(0, 100000), // 100 ms
                    ReceiveMessageRateLimit = new BcmTimeval(0, 0), // no throttle
                };
                int nBytes = bcmCanSocket.CreateReceiveFilterSubscription(subscription, frames);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(72, nBytes);
                }
                else
                {
                    Assert.AreEqual(56, nBytes);
                }

                using (var rawCanSocket = new RawCanSocket())
                {
                    rawCanSocket.Bind(iface);
                    int bytesWritten = rawCanSocket.Write(new CanFrame(0x189, new byte[] { 0x02 }));
                    Assert.AreEqual(16, bytesWritten);

                    bytesWritten = rawCanSocket.Write(new CanFrame(0x189, new byte[] { 0x02, 0xFF }));
                    Assert.AreEqual(16, bytesWritten);
                }

                System.Threading.Thread.Sleep(200);

                nBytes = bcmCanSocket.Read(out BcmCanMessageResponse response);
                Assert.AreEqual(BcmCanFrameType.ClassicCAN, response.FrameType);
                Assert.AreEqual(BcmResponseType.CanFrameReceiveUpdateNotification, response.ResponseType);
                Assert.AreEqual(0x189, response.ContentReceiveFilterSubscription.Id);
                Assert.IsNotNull(response.ClassicFrames);
                Assert.AreEqual(1, response.ClassicFrames.Length);
                CanFrame firstFrame = response.ClassicFrames.First();
                Assert.AreEqual(0x189, firstFrame.CanId);
                Assert.IsTrue(firstFrame.Data.Take(firstFrame.Length).SequenceEqual(new byte[] { 0x02 }));
                Assert.AreEqual(true, response.ContentReceiveFilterSubscription.StartTimer);
                Assert.AreEqual(true, response.ContentReceiveFilterSubscription.SetInterval);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.FilterOnlyByCanId);
                Assert.AreEqual(true, response.ContentReceiveFilterSubscription.MonitorLengthChanges);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.PreventAutoStartingReceiveTimer);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.ReplyToRtrRequest);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.NotifyWhenReceiveRestartsAfterTimeout);
                Assert.AreEqual(0, response.ContentReceiveFilterSubscription.ReceiveTimeout.Seconds);
                Assert.AreEqual(100000, response.ContentReceiveFilterSubscription.ReceiveTimeout.Microseconds);
                Assert.AreEqual(0, response.ContentReceiveFilterSubscription.ReceiveMessageRateLimit.Seconds);
                Assert.AreEqual(0, response.ContentReceiveFilterSubscription.ReceiveMessageRateLimit.Microseconds);

                nBytes = bcmCanSocket.Read(out BcmCanMessageResponse response2);
                Assert.AreEqual(BcmCanFrameType.ClassicCAN, response.FrameType);
                Assert.AreEqual(BcmResponseType.CanFrameReceiveUpdateNotification, response2.ResponseType);
                Assert.AreEqual(0x189, response2.ContentReceiveFilterSubscription.Id);
                Assert.IsNotNull(response2.ClassicFrames);
                Assert.AreEqual(1, response2.ClassicFrames.Length);
                firstFrame = response2.ClassicFrames.First();
                Assert.AreEqual(0x189, firstFrame.CanId);
                Assert.IsTrue(firstFrame.Data.Take(firstFrame.Length).SequenceEqual(new byte[] { 0x02, 0xFF }));
                Assert.AreEqual(true, response2.ContentReceiveFilterSubscription.StartTimer);
                Assert.AreEqual(true, response2.ContentReceiveFilterSubscription.SetInterval);
                Assert.AreEqual(false, response2.ContentReceiveFilterSubscription.FilterOnlyByCanId);
                Assert.AreEqual(true, response2.ContentReceiveFilterSubscription.MonitorLengthChanges);
                Assert.AreEqual(false, response2.ContentReceiveFilterSubscription.PreventAutoStartingReceiveTimer);
                Assert.AreEqual(false, response2.ContentReceiveFilterSubscription.ReplyToRtrRequest);
                Assert.AreEqual(false, response2.ContentReceiveFilterSubscription.NotifyWhenReceiveRestartsAfterTimeout);
                Assert.AreEqual(0, response2.ContentReceiveFilterSubscription.ReceiveTimeout.Seconds);
                Assert.AreEqual(100000, response2.ContentReceiveFilterSubscription.ReceiveTimeout.Microseconds);
                Assert.AreEqual(0, response2.ContentReceiveFilterSubscription.ReceiveMessageRateLimit.Seconds);
                Assert.AreEqual(0, response2.ContentReceiveFilterSubscription.ReceiveMessageRateLimit.Microseconds);
            }
        }

        [Test]
        public void BcmCanSocket_Read_AnnounceResume_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.Connect(iface);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(true, bcmCanSocket.Connected);

                var canFrame = new CanFrame(0x189, new byte[] { 0x01 });
                var frames = new CanFrame[] { canFrame };
                var subscription = new BcmContentRxFilterSubscription()
                {
                    Id = 0x189,
                    StartTimer = true,
                    SetInterval = true,
                    NotifyWhenReceiveRestartsAfterTimeout = true,
                    ReceiveTimeout = new BcmTimeval(0, 100000), // 100 ms
                    ReceiveMessageRateLimit = new BcmTimeval(0, 0), // no throttle
                };
                int nBytes = bcmCanSocket.CreateReceiveFilterSubscription(subscription, frames);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(72, nBytes);
                }
                else
                {
                    Assert.AreEqual(56, nBytes);
                }
                System.Threading.Thread.Sleep(200);

                nBytes = bcmCanSocket.Read(out BcmCanMessageResponse response);
                Assert.AreEqual(BcmCanFrameType.ClassicCAN, response.FrameType);
                Assert.AreEqual(BcmResponseType.CyclicMessageReceiveTimeout, response.ResponseType);
                Assert.AreEqual(0x189, response.ContentReceiveFilterSubscription.Id);
                Assert.IsNull(response.ClassicFrames);
                Assert.AreEqual(true, response.ContentReceiveFilterSubscription.StartTimer);
                Assert.AreEqual(true, response.ContentReceiveFilterSubscription.SetInterval);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.FilterOnlyByCanId);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.MonitorLengthChanges);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.PreventAutoStartingReceiveTimer);
                Assert.AreEqual(false, response.ContentReceiveFilterSubscription.ReplyToRtrRequest);
                Assert.AreEqual(true, response.ContentReceiveFilterSubscription.NotifyWhenReceiveRestartsAfterTimeout);
                Assert.AreEqual(0, response.ContentReceiveFilterSubscription.ReceiveTimeout.Seconds);
                Assert.AreEqual(100000, response.ContentReceiveFilterSubscription.ReceiveTimeout.Microseconds);
                Assert.AreEqual(0, response.ContentReceiveFilterSubscription.ReceiveMessageRateLimit.Seconds);
                Assert.AreEqual(0, response.ContentReceiveFilterSubscription.ReceiveMessageRateLimit.Microseconds);

                using (var rawCanSocket = new RawCanSocket())
                {
                    rawCanSocket.Bind(iface);
                    int bytesWritten = rawCanSocket.Write(new CanFrame(0x189, new byte[] { 0x01 }));
                    Assert.AreEqual(16, bytesWritten);
                }

                System.Threading.Thread.Sleep(200);

                nBytes = bcmCanSocket.Read(out BcmCanMessageResponse response2);
                Assert.AreEqual(BcmCanFrameType.ClassicCAN, response.FrameType);
                Assert.AreEqual(BcmResponseType.CanFrameReceiveUpdateNotification, response2.ResponseType);
                Assert.AreEqual(0x189, response2.ContentReceiveFilterSubscription.Id);
                Assert.IsNotNull(response2.ClassicFrames);
                Assert.AreEqual(1, response2.ClassicFrames.Length);
                CanFrame firstFrame = response2.ClassicFrames.First();
                Assert.AreEqual(0x189, firstFrame.CanId);
                Assert.IsTrue(firstFrame.Data.Take(firstFrame.Length).SequenceEqual(new byte[] { 0x01 }));
                Assert.AreEqual(true, response2.ContentReceiveFilterSubscription.StartTimer);
                Assert.AreEqual(true, response2.ContentReceiveFilterSubscription.SetInterval);
                Assert.AreEqual(false, response2.ContentReceiveFilterSubscription.FilterOnlyByCanId);
                Assert.AreEqual(false, response2.ContentReceiveFilterSubscription.MonitorLengthChanges);
                Assert.AreEqual(false, response2.ContentReceiveFilterSubscription.PreventAutoStartingReceiveTimer);
                Assert.AreEqual(false, response2.ContentReceiveFilterSubscription.ReplyToRtrRequest);
                Assert.AreEqual(true, response2.ContentReceiveFilterSubscription.NotifyWhenReceiveRestartsAfterTimeout);
                Assert.AreEqual(0, response2.ContentReceiveFilterSubscription.ReceiveTimeout.Seconds);
                Assert.AreEqual(100000, response2.ContentReceiveFilterSubscription.ReceiveTimeout.Microseconds);
                Assert.AreEqual(0, response2.ContentReceiveFilterSubscription.ReceiveMessageRateLimit.Seconds);
                Assert.AreEqual(0, response2.ContentReceiveFilterSubscription.ReceiveMessageRateLimit.Microseconds);
            }
        }

        [Test]
        public void BcmCanSocket_CreateReceiveFilterSubscription_RTR_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.Connect(iface);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(true, bcmCanSocket.Connected);

                var canFrame = new CanFrame(0x2FF, new byte[] { 0x0E });
                var frames = new CanFrame[] { canFrame };
                var subscription = new BcmContentRxFilterSubscription()
                {
                    Id = 0x2FF | (uint)CanIdFlags.CAN_RTR_FLAG,
                    ReplyToRtrRequest = true,
                };
                int nBytes = bcmCanSocket.CreateReceiveFilterSubscription(subscription, frames);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(72, nBytes);
                }
                else
                {
                    Assert.AreEqual(56, nBytes);
                }

                using (var rawCanSocket = new RawCanSocket())
                {
                    rawCanSocket.Bind(iface);
                    int bytesWritten = rawCanSocket.Write(new CanFrame(0x2FF | (uint)CanIdFlags.CAN_RTR_FLAG, new byte[0]));
                    Assert.AreEqual(16, bytesWritten);

                    int bytesRead = rawCanSocket.Read(out CanFrame canFrame2);
                    Assert.AreEqual(16, bytesRead);
                    Assert.AreEqual(0x2FF, canFrame2.CanId);
                    Assert.IsTrue(canFrame2.Data.Take(canFrame2.Length).SequenceEqual(new byte[] { 0x0E }));
                }
            }
        }

        [Test]
        public void BcmCanSocket_CreateReceiveFilterSubscription_RTR_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.Connect(iface);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(true, bcmCanSocket.Connected);

                var canFrame = new CanFrame(0x2FF, new byte[] { 0x0E });
                var canFrame2 = new CanFrame(0x300, new byte[] { 0x01, 0x02, 0x03 });
                var frames = new CanFrame[] { canFrame, canFrame2 };
                var subscription = new BcmContentRxFilterSubscription()
                {
                    Id = 0x2FF | (uint)CanIdFlags.CAN_RTR_FLAG,
                    ReplyToRtrRequest = true,
                };
                SocketCanException ex = Assert.Throws<SocketCanException>(() => bcmCanSocket.CreateReceiveFilterSubscription(subscription, frames));
                Assert.AreEqual(SocketError.InvalidArgument, ex.SocketErrorCode);
                Assert.AreEqual(22, ex.NativeErrorCode);
            }
        }

        [Test]
        public void BcmCanSocket_Read_CyclicTransmissionTaskProperties_CopyCanId_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i => i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var bcmCanSocket = new BcmCanSocket())
            {
                bcmCanSocket.Connect(iface);
                Assert.AreEqual(false, bcmCanSocket.IsBound);
                Assert.AreEqual(true, bcmCanSocket.Connected);

                var canFrame = new CanFrame(0x777, new byte[] { 0xDD, 0xEE, 0xAA, 0xDD, 0xBB, 0xEE, 0xEE, 0xFF  });
                var frames = new CanFrame[] { canFrame };
                var config = new BcmCyclicTxTaskConfiguration()
                {
                    Id = 0x555,
                    StartTimer = true,
                    SetInterval = true,
                    CopyCanIdInHeaderToEachCanFrame = true,
                    InitialIntervalConfiguration = new BcmInitialIntervalConfiguration(10, new BcmTimeval(0, 5000)), // 10 messages at 5 ms
                    PostInitialInterval = new BcmTimeval(0, 100000), // Then at 100 ms
                };
                int nBytes = bcmCanSocket.CreateCyclicTransmissionTask(config, frames);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(72, nBytes);
                }
                else
                {
                    Assert.AreEqual(56, nBytes);
                }

                nBytes = bcmCanSocket.QueueCyclicTransmissionTaskProperties(0x555, BcmCanFrameType.ClassicCAN);
                if (Environment.Is64BitProcess)
                {
                    Assert.AreEqual(56, nBytes);
                }
                else
                {
                    Assert.AreEqual(40, nBytes);
                }

                nBytes = bcmCanSocket.Read(out BcmCanMessageResponse response);
                Assert.AreEqual(BcmCanFrameType.ClassicCAN, response.FrameType);
                Assert.AreEqual(BcmResponseType.TransmissionTaskConfiguration, response.ResponseType);
                Assert.AreEqual(0x555, response.CyclicTransmissionTaskConfiguration.Id);
                Assert.IsNotNull(response.ClassicFrames);
                Assert.AreEqual(1, response.ClassicFrames.Length);
                CanFrame firstFrame = response.ClassicFrames.First();
                Assert.AreEqual(0x555, firstFrame.CanId);
                Assert.IsTrue(firstFrame.Data.Take(firstFrame.Length).SequenceEqual(new byte[] { 0xDD, 0xEE, 0xAA, 0xDD, 0xBB, 0xEE, 0xEE, 0xFF }));
                Assert.AreEqual(true, response.CyclicTransmissionTaskConfiguration.StartTimer);
                Assert.AreEqual(true, response.CyclicTransmissionTaskConfiguration.SetInterval);
                Assert.AreEqual(true, response.CyclicTransmissionTaskConfiguration.CopyCanIdInHeaderToEachCanFrame);
                Assert.AreEqual(true, response.CyclicTransmissionTaskConfiguration.ImmediatelyQueueNewFrame); // Will read as True as StartTimer is set to True
                Assert.AreEqual(false, response.CyclicTransmissionTaskConfiguration.NotifyWhenFirstIntervalComplete);
                Assert.AreEqual(false, response.CyclicTransmissionTaskConfiguration.RestartMultipleFrameTxAtIndexZero);
                Assert.GreaterOrEqual(10, response.CyclicTransmissionTaskConfiguration.InitialIntervalConfiguration.Count);
                Assert.AreEqual(0, response.CyclicTransmissionTaskConfiguration.InitialIntervalConfiguration.Interval.Seconds);
                Assert.AreEqual(5000, response.CyclicTransmissionTaskConfiguration.InitialIntervalConfiguration.Interval.Microseconds);
                Assert.AreEqual(0, response.CyclicTransmissionTaskConfiguration.PostInitialInterval.Seconds);
                Assert.AreEqual(100000, response.CyclicTransmissionTaskConfiguration.PostInitialInterval.Microseconds);
            }
        }
    }
}