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
    public class IsoTpCanSocketTests
    {
        [SetUp]
        public void Setup()
        {
            // Precondition Check
            using (var socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_ISOTP))
            {
                if (socketHandle.IsInvalid)
                {
                    Assume.That(LibcNativeMethods.Errno, Is.Not.EqualTo(93) & Is.Not.EqualTo(22)); // If EPROTONOSUPPORT, then this protocol is not supported on this platform and no futher testing applies. If EINVAL, then Protocol Type is not being recognized as valid.
                }
            }
        }

        [Test]
        public void IsoTpCanSocket_Ctor_Success_Test()
        {
            using (var isoTpCanSocket = new IsoTpCanSocket())
            {
                Assert.AreEqual(true, isoTpCanSocket.Blocking);
                Assert.AreEqual(false, isoTpCanSocket.Connected);
                Assert.AreEqual(false, isoTpCanSocket.EnableBroadcast);
                Assert.AreNotEqual(IntPtr.Zero, isoTpCanSocket.Handle);
                Assert.AreEqual(false, isoTpCanSocket.IsBound);
                Assert.AreEqual(SocketCanProtocolType.CAN_ISOTP, isoTpCanSocket.ProtocolType);
                Assert.NotNull(isoTpCanSocket.SafeHandle);
                Assert.AreEqual(false, isoTpCanSocket.SafeHandle.IsInvalid);
                Assert.AreEqual(SocketType.Dgram, isoTpCanSocket.SocketType);
            }
        }

        [Test]
        public void IsoTpCanSocket_BindUsingAddressStructure_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var isoTpCanSocket = new IsoTpCanSocket())
            {
                var addr = new SockAddrCanIsoTp(iface.Index)
                {
                    TxId = 0x7e0,
                    RxId = 0x7e8,
                };

                isoTpCanSocket.Bind(addr);
                Assert.AreEqual(true, isoTpCanSocket.IsBound);
            }
        }

        [Test]
        public void IsoTpCanSocket_BindUsingAddressStructure_NullArgumentException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var isoTpCanSocket = new IsoTpCanSocket())
            {
                Assert.Throws<ArgumentNullException>(() => isoTpCanSocket.Bind(null));
            }
        }

        [Test]
        public void IsoTpCanSocket_BindUsingAddressStructure_ObjectDisposedException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var isoTpCanSocket = new IsoTpCanSocket())
            {
                isoTpCanSocket.Close();

                var addr = new SockAddrCanIsoTp(iface.Index)
                {
                    TxId = 0x7e0,
                    RxId = 0x7e8,
                };
                Assert.Throws<ObjectDisposedException>(() => isoTpCanSocket.Bind(addr));
            }
        }

        [Test]
        public void IsoTpCanSocket_BindUsingAddressStructure_SocketCanException_Failure_Test()
        {
            using (var isoTpCanSocket = new IsoTpCanSocket())
            {
                var addr = new SockAddrCanIsoTp(-1)
                {
                    TxId = 0x7e0,
                    RxId = 0x7e8,
                };
                SocketCanException ex = Assert.Throws<SocketCanException>(() => isoTpCanSocket.Bind(addr));
                Assert.AreEqual(SocketError.SocketError, ex.SocketErrorCode);
                Assert.AreEqual(19, ex.NativeErrorCode);
            }
        }

        [Test]
        public void IsoTpCanSocket_BindUsingParameters_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var isoTpCanSocket = new IsoTpCanSocket())
            {
                isoTpCanSocket.Bind(iface, 0x7e0, 0x7e8);
                Assert.AreEqual(true, isoTpCanSocket.IsBound);
            }
        }

        [Test]
        public void IsoTpCanSocket_BindUsingParameters_NullArgumentException_Failure_Test()
        {
            using (var isoTpCanSocket = new IsoTpCanSocket())
            {
                Assert.Throws<ArgumentNullException>(() => isoTpCanSocket.Bind(null, 0x7e0, 0x7e8));
            }
        }

        [Test]
        public void IsoTpCanSocket_BindUsingParameters_ObjectDisposedException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var isoTpCanSocket = new IsoTpCanSocket())
            {
                isoTpCanSocket.Close();

                var addr = new SockAddrCanIsoTp(iface.Index)
                {
                    TxId = 0x7e0,
                    RxId = 0x7e8,
                };
                Assert.Throws<ObjectDisposedException>(() => isoTpCanSocket.Bind(iface, 0x7e0, 0x7e8));
            }
        }

        [Test]
        public void IsoTpCanSocket_BindUsingParameters_SocketCanException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var isoTpCanSocket = new IsoTpCanSocket())
            {
                iface.Index = -2;
                var addr = new SockAddrCanIsoTp(iface.Index)
                {
                    TxId = 0x7e0,
                    RxId = 0x7e8,
                };

                SocketCanException ex = Assert.Throws<SocketCanException>(() => isoTpCanSocket.Bind(iface, 0x7e0, 0x7e8));
                Assert.AreEqual(SocketError.SocketError, ex.SocketErrorCode);
                Assert.AreEqual(19, ex.NativeErrorCode);
            }
        }

        [Test]
        public void IsoTpCanSocket_BaseOptions_Success_Test()
        {
            using (var isoTpCanSocket = new IsoTpCanSocket())
            {
                isoTpCanSocket.BaseOptions = new CanIsoTpOptions()
                {
                    Flags = IsoTpFlags.CAN_ISOTP_WAIT_TX_DONE | IsoTpFlags.CAN_ISOTP_TX_PADDING,
                    TxPadByte = 0xEE,
                    RxPadByte = 0x22,
                };

                CanIsoTpOptions options = isoTpCanSocket.BaseOptions;
                Assert.AreEqual(0x00, options.ExtendedAddress);
                Assert.AreEqual(IsoTpFlags.CAN_ISOTP_WAIT_TX_DONE | IsoTpFlags.CAN_ISOTP_TX_PADDING, options.Flags);
                Assert.AreEqual(0, options.FrameTxTime);
                Assert.AreEqual(0x00, options.RxExtendedAddress);
                Assert.AreEqual(0x22, options.RxPadByte);
                Assert.AreEqual(0xEE, options.TxPadByte);
            }
        }

        [Test]
        public void IsoTpCanSocket_BaseOptions_NullArgumentException_Failure_Test()
        {
            using (var isoTpCanSocket = new IsoTpCanSocket())
            {
                Assert.Throws<ArgumentNullException>(() => isoTpCanSocket.BaseOptions = null);
            }
        }

        [Test]
        public void IsoTpCanSocket_BaseOptions_ObjectDisposedException_Failure_Test()
        {
            using (var isoTpCanSocket = new IsoTpCanSocket())
            {
                isoTpCanSocket.Close();

                Assert.Throws<ObjectDisposedException>(() =>
                {
                    isoTpCanSocket.BaseOptions = new CanIsoTpOptions()
                    {
                        Flags = IsoTpFlags.CAN_ISOTP_WAIT_TX_DONE | IsoTpFlags.CAN_ISOTP_TX_PADDING,
                        TxPadByte = 0xEE,
                        RxPadByte = 0x22,
                    };
                });
            }
        }

        [Test]
        public void IsoTpCanSocket_BaseOptions_SocketCanException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var isoTpCanSocket = new IsoTpCanSocket())
            {
                isoTpCanSocket.Bind(iface, 0x7e0, 0x7e8);
                SocketCanException ex = Assert.Throws<SocketCanException>(() =>
                {
                    isoTpCanSocket.BaseOptions = new CanIsoTpOptions()
                    {
                        Flags = IsoTpFlags.CAN_ISOTP_WAIT_TX_DONE | IsoTpFlags.CAN_ISOTP_TX_PADDING,
                        TxPadByte = 0xEE,
                        RxPadByte = 0x22,
                    };
                });

                Assert.AreEqual(SocketError.IsConnected, ex.SocketErrorCode);
                Assert.AreEqual(106, ex.NativeErrorCode);
            }
        }

        [Test]
        public void IsoTpCanSocket_Blocking_Success_Test()
        {
            using (var isoTpCanSocket = new IsoTpCanSocket())
            {
                isoTpCanSocket.Blocking = true;
                Assert.IsTrue(isoTpCanSocket.Blocking);
                isoTpCanSocket.Blocking = false;
                Assert.IsFalse(isoTpCanSocket.Blocking);
            }
        }

        [Test]
        public void IsoTpCanSocket_Blocking_ObjectDisposedException_Failure_Test()
        {
            using (var isoTpCanSocket = new IsoTpCanSocket())
            {
                isoTpCanSocket.Close();
                Assert.Throws<ObjectDisposedException>(() => isoTpCanSocket.Blocking = true);
            }
        }

        [Test]
        public void IsoTpCanSocket_EnableBroadcast_Success_Test()
        {
            using (var isoTpCanSocket = new IsoTpCanSocket())
            {
                // Note: Setting this on CAN_ISOTP sockets will have no effect.
                //       Use CAN_ISOTP_SF_BROADCAST flag in the base options instead.
                isoTpCanSocket.EnableBroadcast = true;
                Assert.IsTrue(isoTpCanSocket.EnableBroadcast);
                isoTpCanSocket.EnableBroadcast = false;
                Assert.IsFalse(isoTpCanSocket.EnableBroadcast);
            }
        }

        [Test]
        public void IsoTpCanSocket_EnableBroadcast_ObjectDisposedException_Failure_Test()
        {
            using (var isoTpCanSocket = new IsoTpCanSocket())
            {
                isoTpCanSocket.Close();
                // Note: Setting this on CAN_ISOTP sockets will have no effect.
                //       Use CAN_ISOTP_SF_BROADCAST flag in the base options instead.
                Assert.Throws<ObjectDisposedException>(() => isoTpCanSocket.EnableBroadcast = true);
            }
        }

        [Test]
        public void IsoTpCanSocket_FlowControlOptions_Success_Test()
        {
            using (var isoTpCanSocket = new IsoTpCanSocket())
            {
                isoTpCanSocket.FlowControlOptions = new CanIsoTpFlowControlOptions()
                {
                    BlockSize = 10,
                    Stmin = 5,
                    WftMax = 0,
                };
                CanIsoTpFlowControlOptions fcOpts = isoTpCanSocket.FlowControlOptions;
                Assert.AreEqual(10, fcOpts.BlockSize);
                Assert.AreEqual(5, fcOpts.Stmin);
                Assert.AreEqual(0, fcOpts.WftMax);
            }
        }

        [Test]
        public void IsoTpCanSocket_FlowControlOptions_ObjectDisposedException_Failure_Test()
        {
            using (var isoTpCanSocket = new IsoTpCanSocket())
            {
                isoTpCanSocket.Close();
                Assert.Throws<ObjectDisposedException>(() =>
                {
                    isoTpCanSocket.FlowControlOptions = new CanIsoTpFlowControlOptions()
                    {
                        BlockSize = 10,
                        Stmin = 5,
                        WftMax = 0,
                    };
                });
            }
        }

        [Test]
        public void IsoTpCanSocket_FlowControlOptions_SocketCanException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var isoTpCanSocket = new IsoTpCanSocket())
            {
                isoTpCanSocket.Bind(iface, 0x7e0, 0x7e8);
                SocketCanException ex = Assert.Throws<SocketCanException>(() =>
                {
                    isoTpCanSocket.FlowControlOptions = new CanIsoTpFlowControlOptions()
                    {
                        BlockSize = 10,
                        Stmin = 5,
                        WftMax = 0,
                    };
                });

                Assert.AreEqual(SocketError.IsConnected, ex.SocketErrorCode);
                Assert.AreEqual(106, ex.NativeErrorCode);
            }
        }

        [Test]
        public void IsoTpCanSocket_LinkLayerOptions_Success_Test()
        {
            using (var isoTpCanSocket = new IsoTpCanSocket())
            {
                isoTpCanSocket.LinkLayerOptions = new CanIsoTpLinkLayerOptions()
                {
                    Mtu = 16,
                    TxDataLength = 8,
                    TxFlags = CanFdFlags.None,
                };
                CanIsoTpLinkLayerOptions llOpts = isoTpCanSocket.LinkLayerOptions;
                Assert.AreEqual(16, llOpts.Mtu);
                Assert.AreEqual(8, llOpts.TxDataLength);
                Assert.AreEqual(CanFdFlags.None, llOpts.TxFlags);
            }
        }

        [Test]
        public void IsoTpCanSocket_LinkLayerOptions_ObjectDisposedException_Failure_Test()
        {
            using (var isoTpCanSocket = new IsoTpCanSocket())
            {
                isoTpCanSocket.Close();
                Assert.Throws<ObjectDisposedException>(() =>
                {
                    isoTpCanSocket.LinkLayerOptions = new CanIsoTpLinkLayerOptions()
                    {
                        Mtu = 16,
                        TxDataLength = 8,
                        TxFlags = CanFdFlags.None,
                    };
                });
            }
        }

        [Test]
        public void IsoTpCanSocket_LinkLayerOptions_SocketCanException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var isoTpCanSocket = new IsoTpCanSocket())
            {
                isoTpCanSocket.Bind(iface, 0x7e0, 0x7e8);
                SocketCanException ex = Assert.Throws<SocketCanException>(() =>
                {
                    isoTpCanSocket.LinkLayerOptions = new CanIsoTpLinkLayerOptions()
                    {
                        Mtu = 16,
                        TxDataLength = 8,
                        TxFlags = CanFdFlags.None,
                    };
                });

                Assert.AreEqual(SocketError.IsConnected, ex.SocketErrorCode);
                Assert.AreEqual(106, ex.NativeErrorCode);
            }
        }

        [Test]
        public void IsoTpCanSocket_ReceiveBufferSize_Success_Test()
        {
            using (var isoTpCanSocket = new IsoTpCanSocket())
            {
                isoTpCanSocket.ReceiveBufferSize = 16384;
                Assert.AreEqual(32768, isoTpCanSocket.ReceiveBufferSize);
            }
        }

        [Test]
        public void IsoTpCanSocket_ReceiveBufferSize_ObjectDisposedException_Failure_Test()
        {
            using (var isoTpCanSocket = new IsoTpCanSocket())
            {
                isoTpCanSocket.Close();
                Assert.Throws<ObjectDisposedException>(() => isoTpCanSocket.ReceiveBufferSize = 16384);
            }
        }

        [Test]
        public void IsoTpCanSocket_ReceiveStmin_Success_Test()
        {
            using (var isoTpCanSocket = new IsoTpCanSocket())
            {
                isoTpCanSocket.ReceiveStmin = 7;
                Assert.AreEqual(7, isoTpCanSocket.ReceiveStmin);
            }
        }

        [Test]
        public void IsoTpCanSocket_ReceiveStmin_ObjectDisposedException_Failure_Test()
        {
            using (var isoTpCanSocket = new IsoTpCanSocket())
            {
                isoTpCanSocket.Close();
                Assert.Throws<ObjectDisposedException>(() => isoTpCanSocket.ReceiveStmin = 7);
            }
        }

        [Test]
        public void IsoTpCanSocket_ReceiveStmin_SocketCanException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var isoTpCanSocket = new IsoTpCanSocket())
            {          
                isoTpCanSocket.Bind(iface, 0x7e0, 0x7e8);
                SocketCanException ex = Assert.Throws<SocketCanException>(() => isoTpCanSocket.ReceiveStmin = 7);
                Assert.AreEqual(SocketError.IsConnected, ex.SocketErrorCode);
                Assert.AreEqual(106, ex.NativeErrorCode);
            }
        }

        [Test]
        public void IsoTpCanSocket_ReceiveTimeout_Success_Test()
        {
            using (var isoTpCanSocket = new IsoTpCanSocket())
            {
                isoTpCanSocket.ReceiveTimeout = 1000;
                Assert.AreEqual(1000, isoTpCanSocket.ReceiveTimeout);
            }
        }

        [Test]
        public void IsoTpCanSocket_ReceiveTimeout_ObjectDisposedException_Failure_Test()
        {
            using (var isoTpCanSocket = new IsoTpCanSocket())
            {
                isoTpCanSocket.Close();
                Assert.Throws<ObjectDisposedException>(() => isoTpCanSocket.ReceiveTimeout = 1000);
            }
        }


        [Test]
        public void IsoTpCanSocket_SendBufferSize_Success_Test()
        {
            using (var isoTpCanSocket = new IsoTpCanSocket())
            {
                isoTpCanSocket.SendBufferSize = 8192;
                Assert.AreEqual(16384, isoTpCanSocket.SendBufferSize);
            }
        }

        [Test]
        public void IsoTpCanSocket_SendBufferSize_ObjectDisposedException_Failure_Test()
        {
            using (var isoTpCanSocket = new IsoTpCanSocket())
            {
                isoTpCanSocket.Close();
                Assert.Throws<ObjectDisposedException>(() => isoTpCanSocket.SendBufferSize = 8192);
            }
        }

        [Test]
        public void IsoTpCanSocket_SendTimeout_Success_Test()
        {
            using (var isoTpCanSocket = new IsoTpCanSocket())
            {
                isoTpCanSocket.SendTimeout = 500;
                Assert.AreEqual(500, isoTpCanSocket.SendTimeout);
            }
        }

        [Test]
        public void IsoTpCanSocket_SendTimeout_ObjectDisposedException_Failure_Test()
        {
            using (var isoTpCanSocket = new IsoTpCanSocket())
            {
                isoTpCanSocket.Close();
                Assert.Throws<ObjectDisposedException>(() => isoTpCanSocket.SendTimeout = 500);
            }
        }

        [Test]
        public void IsoTpCanSocket_TransmitStmin_Success_Test()
        {
            using (var isoTpCanSocket = new IsoTpCanSocket())
            {
                isoTpCanSocket.TransmitStmin = 100;
                Assert.AreEqual(100, isoTpCanSocket.TransmitStmin);
            }
        }

        [Test]
        public void IsoTpCanSocket_TransmitStmin_ObjectDisposedException_Failure_Test()
        {
            using (var isoTpCanSocket = new IsoTpCanSocket())
            {
                isoTpCanSocket.Close();
                Assert.Throws<ObjectDisposedException>(() => isoTpCanSocket.TransmitStmin = 100);
            }
        }

        [Test]
        public void IsoTpCanSocket_TransmitStmin_SocketCanException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var isoTpCanSocket = new IsoTpCanSocket())
            {          
                isoTpCanSocket.Bind(iface, 0x7e0, 0x7e8);
                SocketCanException ex = Assert.Throws<SocketCanException>(() => isoTpCanSocket.TransmitStmin = 100);
                Assert.AreEqual(SocketError.IsConnected, ex.SocketErrorCode);
                Assert.AreEqual(106, ex.NativeErrorCode);
            }
        }

        [Test]
        public void IsoTpCanSocket_Write_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var isoTpCanSocket = new IsoTpCanSocket())
            {
                isoTpCanSocket.Bind(iface, 0x7e0, 0x7e8);
                int bytesWritten = isoTpCanSocket.Write(new byte[] { 0x01, 0x00 });
                Assert.AreEqual(2, bytesWritten);
            }
        }

        [Test]
        public void IsoTpCanSocket_Write_ObjectDisposedException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var isoTpCanSocket = new IsoTpCanSocket())
            {
                isoTpCanSocket.Bind(iface, 0x7e0, 0x7e8);
                isoTpCanSocket.Close();
                Assert.Throws<ObjectDisposedException>(() => isoTpCanSocket.Write(new byte[] { 0x01, 0x00 }));
            }
        }

        [Test]
        public void IsoTpCanSocket_Write_SocketCanException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var isoTpCanSocket = new IsoTpCanSocket())
            {
                SocketCanException ex = Assert.Throws<SocketCanException>(() => isoTpCanSocket.Write(new byte[] { 0x01, 0x00 }));
                Assert.AreEqual(SocketError.AddressNotAvailable, ex.SocketErrorCode);
                Assert.AreEqual(99, ex.NativeErrorCode);
            }
        }

        [Test]
        public void IsoTpCanSocket_Write_ArgumentNullException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var isoTpCanSocket = new IsoTpCanSocket())
            {
                isoTpCanSocket.Bind(iface, 0x7e0, 0x7e8);
                Assert.Throws<ArgumentNullException>(() => isoTpCanSocket.Write(null));
            }
        }

        [Test]
        public void IsoTpCanSocket_Read_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var isoTpCanSocketTester = new IsoTpCanSocket())
            using (var isoTpCanSocketEcu = new IsoTpCanSocket())
            {
                isoTpCanSocketTester.Bind(iface, 0x7e0, 0x7e8);
                isoTpCanSocketEcu.Bind(iface, 0x7e8, 0x7e0);

                int bytesWritten = isoTpCanSocketTester.Write(new byte[] { 0x22, 0xf1, 0x8c });
                Assert.AreEqual(3, bytesWritten);

                var data = new byte[3];
                int bytesRead = isoTpCanSocketEcu.Read(data);
                Assert.AreEqual(3, bytesRead);
                Assert.IsTrue(data.SequenceEqual(new byte[] { 0x22, 0xf1, 0x8c }));
            }
        }

        [Test]
        public void IsoTpCanSocket_Read_ObjectDisposedException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var isoTpCanSocketTester = new IsoTpCanSocket())
            using (var isoTpCanSocketEcu = new IsoTpCanSocket())
            {
                isoTpCanSocketTester.Bind(iface, 0x7e0, 0x7e8);
                isoTpCanSocketEcu.Bind(iface, 0x7e8, 0x7e0);
                isoTpCanSocketEcu.Close();

                int bytesWritten = isoTpCanSocketTester.Write(new byte[] { 0x22, 0xf1, 0x8c });
                Assert.AreEqual(3, bytesWritten);

                var data = new byte[3];
                Assert.Throws<ObjectDisposedException>(() => isoTpCanSocketEcu.Read(data));
            }
        }

        [Test]
        public void IsoTpCanSocket_Read_SocketCanException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var isoTpCanSocketTester = new IsoTpCanSocket())
            using (var isoTpCanSocketEcu = new IsoTpCanSocket())
            {
                isoTpCanSocketEcu.ReceiveTimeout = 100;
                isoTpCanSocketTester.Bind(iface, 0x7e0, 0x7e8);

                int bytesWritten = isoTpCanSocketTester.Write(new byte[] { 0x22, 0xf1, 0x8c });
                Assert.AreEqual(3, bytesWritten);

                var data = new byte[3];
                SocketCanException ex = Assert.Throws<SocketCanException>(() => isoTpCanSocketEcu.Read(data));
                // depending on kernel version - exact behavior may differ
                Assert.That(ex.SocketErrorCode, Is.EqualTo(SocketError.WouldBlock) | Is.EqualTo(SocketError.AddressNotAvailable));
                Assert.That(ex.NativeErrorCode, Is.EqualTo(11) | Is.EqualTo(99));
            }
        }

        [Test]
        public void IsoTpCanSocket_Read_ArgumentNullException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var isoTpCanSocketTester = new IsoTpCanSocket())
            using (var isoTpCanSocketEcu = new IsoTpCanSocket())
            {
                isoTpCanSocketTester.Bind(iface, 0x7e0, 0x7e8);
                isoTpCanSocketEcu.Bind(iface, 0x7e8, 0x7e0);

                int bytesWritten = isoTpCanSocketTester.Write(new byte[] { 0x22, 0xf1, 0x8c });
                Assert.AreEqual(3, bytesWritten);

                var data = new byte[3];
                Assert.Throws<ArgumentNullException>(() => isoTpCanSocketEcu.Read(null));
            }
        }

        [Test]
        public void IsoTpCanSocket_Write_NonBlocking_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var isoTpCanSocket = new IsoTpCanSocket())
            {
                isoTpCanSocket.Blocking = false;
                isoTpCanSocket.Bind(iface, 0x7e0, 0x7e8);
                int bytesWritten = isoTpCanSocket.Write(new byte[] { 0x3e, 0x80 });
                Assert.AreEqual(2, bytesWritten);
            }
        }

        [Test]
        public void IsoTpCanSocket_Read_NonBlocking_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var isoTpCanSocketTester = new IsoTpCanSocket())
            using (var isoTpCanSocketEcu = new IsoTpCanSocket())
            {
                isoTpCanSocketTester.Blocking = false;
                isoTpCanSocketEcu.Blocking = false;
                isoTpCanSocketTester.Bind(iface, 0x7e0, 0x7e8);
                isoTpCanSocketEcu.Bind(iface, 0x7e8, 0x7e0);

                int bytesWritten = isoTpCanSocketTester.Write(new byte[] { 0x3e, 0x00 });
                Assert.AreEqual(2, bytesWritten);

                var data = new byte[3];
                int bytesRead = isoTpCanSocketEcu.Read(data);
                Assert.AreEqual(2, bytesRead);
                Assert.IsTrue(data.Take(bytesRead).ToArray().SequenceEqual(new byte[] { 0x3e, 0x00 }));
            }
        }

        [Test]
        public void IsoTpCanSocket_Close_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var isoTpCanSocket = new IsoTpCanSocket())
            {
                isoTpCanSocket.Close();
            }
        }

        [Test]
        public void IsoTpCanSocket_GetAddress_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var isoTpCanSocket = new IsoTpCanSocket())
            {
                isoTpCanSocket.Bind(iface, 0x7e0, 0x7e8);
                Assert.AreEqual(true, isoTpCanSocket.IsBound);
                SockAddrCanIsoTp addr = isoTpCanSocket.Address;
                Assert.AreEqual(SocketCanConstants.AF_CAN, addr.CanFamily);
                Assert.AreEqual(iface.Index, addr.CanIfIndex);
                Assert.AreEqual(0x7e0, addr.TxId);
                Assert.AreEqual(0x7e8, addr.RxId);
            }
        }

        [Test]
        public void IsoTpCanSocket_GetAddress_NotBound_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var isoTpCanSocket = new IsoTpCanSocket())
            {
                Assert.AreEqual(false, isoTpCanSocket.IsBound);
                SockAddrCanIsoTp addr = isoTpCanSocket.Address;
                Assert.AreEqual(SocketCanConstants.AF_CAN, addr.CanFamily);
                Assert.AreEqual(0, addr.CanIfIndex);
                Assert.AreEqual(0x0, addr.TxId);
                Assert.AreEqual(0x00, addr.RxId);
            }
        }

        [Test]
        public void IsoTpCanSocket_GetAddress_ObjectDisposedException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var isoTpCanSocket = new IsoTpCanSocket())
            {
                isoTpCanSocket.Bind(iface, 0x7e0, 0x7e8);
                Assert.AreEqual(true, isoTpCanSocket.IsBound);
                isoTpCanSocket.Close();
                Assert.Throws<ObjectDisposedException>(() => { SockAddrCanIsoTp addr = isoTpCanSocket.Address; });
            }
        }

        [Test]
        public void IsoTpCanSocket_Read_with_MessageFlags_Success_Test()
        {
            // Tests this change: 
            // https://git.kernel.org/pub/scm/linux/kernel/git/netdev/net-next.git/commit/?id=42bf50a1795a1854d48717b7361dbdbce496b16b

            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var isoTpCanSocketTester = new IsoTpCanSocket())
            using (var isoTpCanSocketEcu = new IsoTpCanSocket())
            {
                isoTpCanSocketTester.Bind(iface, 0x7e0, 0x7e8);
                isoTpCanSocketEcu.Bind(iface, 0x7e8, 0x7e0);

                int bytesWritten = isoTpCanSocketTester.Write(new byte[] { 0x22, 0xf1, 0x8c });
                Assert.AreEqual(3, bytesWritten);

                var data = new byte[1];
                int bytesRead = isoTpCanSocketEcu.Read(data, MessageFlags.MSG_TRUNC | MessageFlags.MSG_PEEK);
                Assert.That(bytesRead, Is.EqualTo(3).Or.EqualTo(1)); // Pre update this would return 1, post update it will return 3.
                Assert.IsTrue(data.SequenceEqual(new byte[] { 0x22 }));
            }
        }

        [Test]
        public void IsoTpCanSocket_Read_with_MessageFlags_ObjectDisposedException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var isoTpCanSocketTester = new IsoTpCanSocket())
            using (var isoTpCanSocketEcu = new IsoTpCanSocket())
            {
                isoTpCanSocketTester.Bind(iface, 0x7e0, 0x7e8);
                isoTpCanSocketEcu.Bind(iface, 0x7e8, 0x7e0);
                isoTpCanSocketEcu.Close();

                int bytesWritten = isoTpCanSocketTester.Write(new byte[] { 0x22, 0xf1, 0x8c });
                Assert.AreEqual(3, bytesWritten);

                var data = new byte[3];
                Assert.Throws<ObjectDisposedException>(() => isoTpCanSocketEcu.Read(data, MessageFlags.MSG_TRUNC | MessageFlags.MSG_PEEK));
            }
        }

        [Test]
        public void IsoTpCanSocket_Read_with_MessageFlags_SocketCanException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var isoTpCanSocketTester = new IsoTpCanSocket())
            using (var isoTpCanSocketEcu = new IsoTpCanSocket())
            {
                isoTpCanSocketEcu.ReceiveTimeout = 100;
                isoTpCanSocketTester.Bind(iface, 0x7e0, 0x7e8);

                int bytesWritten = isoTpCanSocketTester.Write(new byte[] { 0x22, 0xf1, 0x8c });
                Assert.AreEqual(3, bytesWritten);

                var data = new byte[3];
                SocketCanException ex = Assert.Throws<SocketCanException>(() => isoTpCanSocketEcu.Read(data, MessageFlags.MSG_TRUNC | MessageFlags.MSG_PEEK));
                // depending on kernel version - exact behavior may differ
                Assert.That(ex.SocketErrorCode, Is.EqualTo(SocketError.WouldBlock) | Is.EqualTo(SocketError.AddressNotAvailable));
                Assert.That(ex.NativeErrorCode, Is.EqualTo(11) | Is.EqualTo(99));
            }
        }

        [Test]
        public void IsoTpCanSocket_Read_with_MessageFlags_ArgumentNullException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var isoTpCanSocketTester = new IsoTpCanSocket())
            using (var isoTpCanSocketEcu = new IsoTpCanSocket())
            {
                isoTpCanSocketTester.Bind(iface, 0x7e0, 0x7e8);
                isoTpCanSocketEcu.Bind(iface, 0x7e8, 0x7e0);

                int bytesWritten = isoTpCanSocketTester.Write(new byte[] { 0x22, 0xf1, 0x8c });
                Assert.AreEqual(3, bytesWritten);

                var data = new byte[3];
                Assert.Throws<ArgumentNullException>(() => isoTpCanSocketEcu.Read(null, MessageFlags.MSG_TRUNC | MessageFlags.MSG_PEEK));
            }
        }

        [Test]
        public void IsoTpCanSocket_Read_with_MessageFlags_NonBlocking_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var isoTpCanSocketTester = new IsoTpCanSocket())
            using (var isoTpCanSocketEcu = new IsoTpCanSocket())
            {
                isoTpCanSocketTester.Bind(iface, 0x7e0, 0x7e8);
                isoTpCanSocketEcu.Bind(iface, 0x7e8, 0x7e0);

                int bytesWritten = isoTpCanSocketTester.Write(new byte[] { 0x3e, 0x00 });
                Assert.AreEqual(2, bytesWritten);

                var data = new byte[3];
                int bytesRead = isoTpCanSocketEcu.Read(data, MessageFlags.MSG_DONTWAIT);
                Assert.AreEqual(2, bytesRead);
                Assert.IsTrue(data.Take(bytesRead).ToArray().SequenceEqual(new byte[] { 0x3e, 0x00 }));
            }
        }
    }
}