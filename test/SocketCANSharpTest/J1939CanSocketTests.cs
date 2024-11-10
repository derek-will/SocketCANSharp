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
    public class J1939CanSocketTests
    {
        [SetUp]
        public void Setup()
        {
            // Precondition Check
            using (var socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_J1939))
            {
                if (socketHandle.IsInvalid)
                {
                    Assume.That(LibcNativeMethods.Errno, Is.Not.EqualTo(93) & Is.Not.EqualTo(22)); // If EPROTONOSUPPORT, then this protocol is not supported on this platform and no futher testing applies. If EINVAL, then Protocol Type is not being recognized as valid.
                }
            }
        }

        [Test]
        public void J1939CanSocket_Ctor_Success_Test()
        {
            using (var j1939CanSocket = new J1939CanSocket())
            {
                Assert.AreEqual(true, j1939CanSocket.Blocking);
                Assert.AreEqual(false, j1939CanSocket.Connected);
                Assert.AreEqual(false, j1939CanSocket.EnableBroadcast);
                Assert.AreNotEqual(IntPtr.Zero, j1939CanSocket.Handle);
                Assert.AreEqual(false, j1939CanSocket.IsBound);
                Assert.AreEqual(SocketCanProtocolType.CAN_J1939, j1939CanSocket.ProtocolType);
                Assert.NotNull(j1939CanSocket.SafeHandle);
                Assert.AreEqual(false, j1939CanSocket.SafeHandle.IsInvalid);
                Assert.AreEqual(SocketType.Dgram, j1939CanSocket.SocketType);
            }
        }

        [Test]
        public void J1939CanSocket_BindUsingAddressStructure_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var j1939CanSocket = new J1939CanSocket())
            {
                var addr = new SockAddrCanJ1939(iface.Index)
                {
                    Name = SocketCanConstants.J1939_NO_NAME,
                    PGN = 0x1b100,
                    Address = 0x25,
                };

                j1939CanSocket.Bind(addr);
                Assert.AreEqual(true, j1939CanSocket.IsBound);
            }
        }

        [Test]
        public void J1939CanSocket_ConnectUsingAddressStructure_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var j1939CanSocket = new J1939CanSocket())
            {
                var addr25 = new SockAddrCanJ1939(iface.Index)
                {
                    Name = SocketCanConstants.J1939_NO_NAME,
                    PGN = 0x1b100,
                    Address = 0x25,
                };

                j1939CanSocket.Bind(addr25);
                Assert.AreEqual(true, j1939CanSocket.IsBound);

                var addr50 = new SockAddrCanJ1939(iface.Index)
                {
                    Name = SocketCanConstants.J1939_NO_NAME,
                    PGN = 0x1b100,
                    Address = 0x50,
                };

                j1939CanSocket.Connect(addr50);
                Assert.AreEqual(true, j1939CanSocket.Connected);
            }
        }

        [Test]
        public void J1939CanSocket_ConnectUsingAddressStructure_NotBound_SocketCanException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var j1939CanSocket = new J1939CanSocket())
            {
                var addr = new SockAddrCanJ1939(iface.Index)
                {
                    Name = SocketCanConstants.J1939_NO_NAME,
                    PGN = 0x1b100,
                    Address = 0x50,
                };

                SocketCanException ex = Assert.Throws<SocketCanException>(() => j1939CanSocket.Connect(addr));
                Assert.AreEqual(SocketError.InvalidArgument, ex.SocketErrorCode);
                Assert.AreEqual(22, ex.NativeErrorCode);
                Assert.AreEqual(false, j1939CanSocket.Connected);
            }
        }

        [Test]
        public void J1939CanSocket_BindUsingAddressStructure_NullArgumentException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var j1939CanSocket = new J1939CanSocket())
            {
                Assert.Throws<ArgumentNullException>(() => j1939CanSocket.Bind(null));
            }
        }

        [Test]
        public void J1939CanSocket_ConnectUsingAddressStructure_NullArgumentException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var j1939CanSocket = new J1939CanSocket())
            {
                var addr25 = new SockAddrCanJ1939(iface.Index)
                {
                    Name = SocketCanConstants.J1939_NO_NAME,
                    PGN = 0x1b100,
                    Address = 0x25,
                };

                j1939CanSocket.Bind(addr25);
                Assert.AreEqual(true, j1939CanSocket.IsBound);

                Assert.Throws<ArgumentNullException>(() => j1939CanSocket.Connect(null));
                Assert.AreEqual(false, j1939CanSocket.Connected);
            }
        }

        [Test]
        public void J1939CanSocket_BindUsingAddressStructure_ObjectDisposedException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var j1939CanSocket = new J1939CanSocket())
            {
                j1939CanSocket.Close();

                var addr = new SockAddrCanJ1939(iface.Index)
                {
                    Name = SocketCanConstants.J1939_NO_NAME,
                    PGN = 0x1b100,
                    Address = 0x25,
                };
                Assert.Throws<ObjectDisposedException>(() => j1939CanSocket.Bind(addr));
            }
        }

        [Test]
        public void J1939CanSocket_ConnectUsingAddressStructure_ObjectDisposedException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var j1939CanSocket = new J1939CanSocket())
            {
                var addr25 = new SockAddrCanJ1939(iface.Index)
                {
                    Name = SocketCanConstants.J1939_NO_NAME,
                    PGN = 0x1b100,
                    Address = 0x25,
                };

                j1939CanSocket.Bind(addr25);
                Assert.AreEqual(true, j1939CanSocket.IsBound);

                var addr50 = new SockAddrCanJ1939(iface.Index)
                {
                    Name = SocketCanConstants.J1939_NO_NAME,
                    PGN = 0x1b100,
                    Address = 0x50,
                };

                j1939CanSocket.Close();
                Assert.Throws<ObjectDisposedException>(() => j1939CanSocket.Connect(addr50));
                Assert.AreEqual(false, j1939CanSocket.Connected);
            }
        }

        [Test]
        public void J1939CanSocket_BindUsingAddressStructure_SocketCanException_Failure_Test()
        {
            using (var j1939CanSocket = new J1939CanSocket())
            {
                var addr = new SockAddrCanJ1939(-1)
                {
                    Name = SocketCanConstants.J1939_NO_NAME,
                    PGN = 0x1b100,
                    Address = 0x25,
                };
                SocketCanException ex = Assert.Throws<SocketCanException>(() => j1939CanSocket.Bind(addr));
                Assert.AreEqual(SocketError.SocketError, ex.SocketErrorCode);
                Assert.AreEqual(19, ex.NativeErrorCode);
            }
        }

        [Test]
        public void J1939CanSocket_ConnectUsingAddressStructure_InvalidIndex_SocketCanException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var j1939CanSocket = new J1939CanSocket())
            {
                var addr25 = new SockAddrCanJ1939(iface.Index)
                {
                    Name = SocketCanConstants.J1939_NO_NAME,
                    PGN = 0x1b100,
                    Address = 0x25,
                };

                j1939CanSocket.Bind(addr25);
                Assert.AreEqual(true, j1939CanSocket.IsBound);

                var addr50 = new SockAddrCanJ1939(-1)
                {
                    Name = SocketCanConstants.J1939_NO_NAME,
                    PGN = 0x1b100,
                    Address = 0x50,
                };

                SocketCanException ex = Assert.Throws<SocketCanException>(() => j1939CanSocket.Connect(addr50));
                Assert.AreEqual(SocketError.InvalidArgument, ex.SocketErrorCode);
                Assert.AreEqual(22, ex.NativeErrorCode);
                Assert.AreEqual(false, j1939CanSocket.Connected);
            }
        }

        [Test]
        public void J1939CanSocket_BindUsingParameters_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var j1939CanSocket = new J1939CanSocket())
            {
                j1939CanSocket.Bind(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x25);
                Assert.AreEqual(true, j1939CanSocket.IsBound);
            }
        }

        [Test]
        public void J1939CanSocket_ConnectUsingParameters_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var j1939CanSocket = new J1939CanSocket())
            {
                j1939CanSocket.Bind(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x25);
                Assert.AreEqual(true, j1939CanSocket.IsBound);

                j1939CanSocket.Connect(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x50);
                Assert.AreEqual(true, j1939CanSocket.Connected);
            }
        }

        [Test]
        public void J1939CanSocket_BindUsingParameters_NullArgumentException_Failure_Test()
        {
            using (var j1939CanSocket = new J1939CanSocket())
            {
                Assert.Throws<ArgumentNullException>(() => j1939CanSocket.Bind(null, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x25));
            }
        }

        [Test]
        public void J1939CanSocket_ConnectUsingParameters_NullArgumentException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var j1939CanSocket = new J1939CanSocket())
            {
                j1939CanSocket.Bind(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x25);
                Assert.AreEqual(true, j1939CanSocket.IsBound);

                Assert.Throws<ArgumentNullException>(() => j1939CanSocket.Connect(null, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x50));
                Assert.AreEqual(false, j1939CanSocket.Connected);
            }
        }

        [Test]
        public void J1939CanSocket_BindUsingParameters_ObjectDisposedException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var j1939CanSocket = new J1939CanSocket())
            {
                j1939CanSocket.Close();
                Assert.Throws<ObjectDisposedException>(() => j1939CanSocket.Bind(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x25));
            }
        }

        [Test]
        public void J1939CanSocket_ConnectUsingParameters_ObjectDisposedException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var j1939CanSocket = new J1939CanSocket())
            {
                j1939CanSocket.Bind(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x25);
                Assert.AreEqual(true, j1939CanSocket.IsBound);

                j1939CanSocket.Close();
                Assert.Throws<ObjectDisposedException>(() => j1939CanSocket.Connect(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x50));
                Assert.AreEqual(false, j1939CanSocket.Connected);
            }
        }

        [Test]
        public void J1939CanSocket_BindUsingParameters_SocketCanException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var j1939CanSocket = new J1939CanSocket())
            {
                iface.Index = -2;
                SocketCanException ex = Assert.Throws<SocketCanException>(() => j1939CanSocket.Bind(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x25));
                Assert.AreEqual(SocketError.SocketError, ex.SocketErrorCode);
                Assert.AreEqual(19, ex.NativeErrorCode);
            }
        }

        [Test]
        public void J1939CanSocket_ConnectUsingParameters_SocketCanException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var j1939CanSocket = new J1939CanSocket())
            {
                j1939CanSocket.Bind(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x25);
                Assert.AreEqual(true, j1939CanSocket.IsBound);
                iface.Index = -2;
                SocketCanException ex = Assert.Throws<SocketCanException>(() => j1939CanSocket.Connect(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x50));
                Assert.AreEqual(SocketError.InvalidArgument, ex.SocketErrorCode);
                Assert.AreEqual(22, ex.NativeErrorCode);
                Assert.AreEqual(false, j1939CanSocket.Connected);
            }
        }

        [Test]
        public void J1939CanSocket_Blocking_Success_Test()
        {
            using (var j1939CanSocket = new J1939CanSocket())
            {
                j1939CanSocket.Blocking = true;
                Assert.IsTrue(j1939CanSocket.Blocking);
                j1939CanSocket.Blocking = false;
                Assert.IsFalse(j1939CanSocket.Blocking);
            }
        }

        [Test]
        public void J1939CanSocket_Blocking_ObjectDisposedException_Failure_Test()
        {
            using (var j1939CanSocket = new J1939CanSocket())
            {
                j1939CanSocket.Close();
                Assert.Throws<ObjectDisposedException>(() => j1939CanSocket.Blocking = true);
            }
        }

        [Test]
        public void J1939CanSocket_EnableBroadcast_Success_Test()
        {
            using (var j1939CanSocket = new J1939CanSocket())
            {
                j1939CanSocket.EnableBroadcast = true;
                Assert.IsTrue(j1939CanSocket.EnableBroadcast);
                j1939CanSocket.EnableBroadcast = false;
                Assert.IsFalse(j1939CanSocket.EnableBroadcast);
            }
        }

        [Test]
        public void J1939CanSocket_EnableBroadcast_ObjectDisposedException_Failure_Test()
        {
            using (var j1939CanSocket = new J1939CanSocket())
            {
                j1939CanSocket.Close();
                Assert.Throws<ObjectDisposedException>(() => j1939CanSocket.EnableBroadcast = true);
            }
        }

        [Test]
        public void J1939CanSocket_ReceiveBufferSize_Success_Test()
        {
            using (var j1939CanSocket = new J1939CanSocket())
            {
                j1939CanSocket.ReceiveBufferSize = 16384;
                Assert.AreEqual(32768, j1939CanSocket.ReceiveBufferSize);
            }
        }

        [Test]
        public void J1939CanSocket_ReceiveBufferSize_ObjectDisposedException_Failure_Test()
        {
            using (var j1939CanSocket = new J1939CanSocket())
            {
                j1939CanSocket.Close();
                Assert.Throws<ObjectDisposedException>(() => j1939CanSocket.ReceiveBufferSize = 16384);
            }
        }

        [Test]
        public void J1939CanSocket_ReceiveTimeout_Success_Test()
        {
            using (var j1939CanSocket = new J1939CanSocket())
            {
                j1939CanSocket.ReceiveTimeout = 1000;
                Assert.AreEqual(1000, j1939CanSocket.ReceiveTimeout);
            }
        }

        [Test]
        public void J1939CanSocket_ReceiveTimeout_ObjectDisposedException_Failure_Test()
        {
            using (var j1939CanSocket = new IsoTpCanSocket())
            {
                j1939CanSocket.Close();
                Assert.Throws<ObjectDisposedException>(() => j1939CanSocket.ReceiveTimeout = 1000);
            }
        }

        [Test]
        public void J1939CanSocket_SendBufferSize_Success_Test()
        {
            using (var j1939CanSocket = new J1939CanSocket())
            {
                j1939CanSocket.SendBufferSize = 8192;
                Assert.AreEqual(16384, j1939CanSocket.SendBufferSize);
            }
        }

        [Test]
        public void J1939CanSocket_SendBufferSize_ObjectDisposedException_Failure_Test()
        {
            using (var j1939CanSocket = new J1939CanSocket())
            {
                j1939CanSocket.Close();
                Assert.Throws<ObjectDisposedException>(() => j1939CanSocket.SendBufferSize = 8192);
            }
        }

        [Test]
        public void J1939CanSocket_SendTimeout_Success_Test()
        {
            using (var j1939CanSocket = new J1939CanSocket())
            {
                j1939CanSocket.SendTimeout = 500;
                Assert.AreEqual(500, j1939CanSocket.SendTimeout);
            }
        }

        [Test]
        public void J1939CanSocket_SendTimeout_ObjectDisposedException_Failure_Test()
        {
            using (var j1939CanSocket = new J1939CanSocket())
            {
                j1939CanSocket.Close();
                Assert.Throws<ObjectDisposedException>(() => j1939CanSocket.SendTimeout = 500);
            }
        }

        [Test]
        public void J1939CanSocket_J1939Filters_Success_Test()
        {
            using (var j1939CanSocket = new J1939CanSocket())
            {
                var filter1 = new J1939Filter()
                {
                    Name = 0x00000000,
                    NameMask = 0xFFFFFFFFFFFFFFFF,
                    PGN = 0x40000,
                    PGNMask = 0xFFFFFFFF,
                    Address = 0xFF,
                    AddressMask = 0xFF,
                };

                var filter2 = new J1939Filter()
                {
                    Name = 0x00000000,
                    NameMask = 0xFFFFFFFFFFFFFFFF,
                    PGN = 0x41000,
                    PGNMask = 0xFFFFFFFF,
                    Address = 0xFF,
                    AddressMask = 0xFF,
                };

                j1939CanSocket.J1939Filters = new J1939Filter[] { filter1, filter2 };
            }
        }

        [Test]
        public void J1939CanSocket_J1939Filters_Null_Success_Test()
        {
            using (var j1939CanSocket = new J1939CanSocket())
            {
                j1939CanSocket.J1939Filters = null;
            }
        }

        [Test]
        public void J1939CanSocket_J1939Filters_ObjectDisposedException_Failure_Test()
        {
            using (var j1939CanSocket = new J1939CanSocket())
            {
                j1939CanSocket.Close();

                Assert.Throws<ObjectDisposedException>(() =>
                {
                    var filter1 = new J1939Filter()
                    {
                        Name = 0x00000000,
                        NameMask = 0xFFFFFFFFFFFFFFFF,
                        PGN = 0x40000,
                        PGNMask = 0xFFFFFFFF,
                        Address = 0xFF,
                        AddressMask = 0xFF,
                    };

                    var filter2 = new J1939Filter()
                    {
                        Name = 0x00000000,
                        NameMask = 0xFFFFFFFFFFFFFFFF,
                        PGN = 0x41000,
                        PGNMask = 0xFFFFFFFF,
                        Address = 0xFF,
                        AddressMask = 0xFF,
                    };

                    j1939CanSocket.J1939Filters = new J1939Filter[] { filter1, filter2 };
                });
            }
        }

        [Test]
        public void J1939CanSocket_J1939Filters_Post_Bind_Assignment_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i => i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var j1939CanSocket = new J1939CanSocket())
            {
                j1939CanSocket.Bind(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x25);

                var filter1 = new J1939Filter()
                {
                    Name = 0x00000000,
                    NameMask = 0xFFFFFFFFFFFFFFFF,
                    PGN = 0x40000,
                    PGNMask = 0xFFFFFFFF,
                    Address = 0xFF,
                    AddressMask = 0xFF,
                };

                var filter2 = new J1939Filter()
                {
                    Name = 0x00000000,
                    NameMask = 0xFFFFFFFFFFFFFFFF,
                    PGN = 0x41000,
                    PGNMask = 0xFFFFFFFF,
                    Address = 0xFF,
                    AddressMask = 0xFF,
                };

                j1939CanSocket.J1939Filters = new J1939Filter[] { filter1, filter2 };
            }
        }

        [Test]
        public void J1939CanSocket_EnablePromiscuousMode_Success_Test()
        {
            using (var j1939CanSocket = new J1939CanSocket())
            {
                j1939CanSocket.EnablePromiscuousMode = true;
                Assert.IsTrue(j1939CanSocket.EnablePromiscuousMode);
                j1939CanSocket.EnablePromiscuousMode = false;
                Assert.IsFalse(j1939CanSocket.EnablePromiscuousMode);
            }
        }

        [Test]
        public void J1939CanSocket_SendPriority_Success_Test()
        {
            using (var j1939CanSocket = new J1939CanSocket())
            {
                j1939CanSocket.SendPriority = 4;
                Assert.AreEqual(4, j1939CanSocket.SendPriority);
                j1939CanSocket.SendPriority = 5;
                Assert.AreEqual(5, j1939CanSocket.SendPriority);
            }
        }

        [Test]
        public void J1939CanSocket_EnablePromiscuousMode_ObjectDisposedException_Failure_Test()
        {
            using (var j1939CanSocket = new J1939CanSocket())
            {
                j1939CanSocket.Close();
                Assert.Throws<ObjectDisposedException>(() => j1939CanSocket.EnablePromiscuousMode = true);
            }
        }

        [Test]
        public void J1939CanSocket_SendPriority_ObjectDisposedException_Failure_Test()
        {
            using (var j1939CanSocket = new J1939CanSocket())
            {
                j1939CanSocket.Close();
                Assert.Throws<ObjectDisposedException>(() => j1939CanSocket.SendPriority = 4);
            }
        }

        [Test]
        public void J1939CanSocket_EnableErrorQueue_Success_Test()
        {
            using (var j1939CanSocket = new J1939CanSocket())
            {
                j1939CanSocket.EnableErrorQueue = true;
                Assert.IsTrue(j1939CanSocket.EnableErrorQueue);
                j1939CanSocket.EnableErrorQueue = false;
                Assert.IsFalse(j1939CanSocket.EnableErrorQueue);
            }
        }

        [Test]
        public void J1939CanSocket_EnableErrorQueue_ObjectDisposedException_Failure_Test()
        {
            using (var j1939CanSocket = new J1939CanSocket())
            {
                j1939CanSocket.Close();
                Assert.Throws<ObjectDisposedException>(() => j1939CanSocket.EnableErrorQueue = true);
            }
        }

        [Test]
        public void J1939CanSocket_Write_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var j1939CanSocket = new J1939CanSocket())
            {
                j1939CanSocket.Bind(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x25);
                j1939CanSocket.Connect(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x50);
                int bytesWritten = j1939CanSocket.Write(new byte[] { 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f });
                Assert.AreEqual(6, bytesWritten);
            }
        }

        [Test]
        public void J1939CanSocket_Write_ObjectDisposedException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var j1939CanSocket = new J1939CanSocket())
            {
                j1939CanSocket.Bind(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x25);
                j1939CanSocket.Connect(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x50);
                j1939CanSocket.Close();
                Assert.Throws<ObjectDisposedException>(() => j1939CanSocket.Write(new byte[] { 0x01, 0x00 }));
            }
        }

        [Test]
        public void J1939CanSocket_Write_SocketCanException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var j1939CanSocket = new J1939CanSocket())
            {
                SocketCanException ex = Assert.Throws<SocketCanException>(() => j1939CanSocket.Write(new byte[] { 0x01, 0x00 }));
                Assert.AreEqual(SocketError.SocketError, ex.SocketErrorCode);
                Assert.AreEqual(77, ex.NativeErrorCode); // EBADFD
            }
        }

        [Test]
        public void J1939CanSocket_Write_ArgumentNullException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var j1939CanSocket = new J1939CanSocket())
            {
                j1939CanSocket.Bind(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x25);
                j1939CanSocket.Connect(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x50);
                Assert.Throws<ArgumentNullException>(() => j1939CanSocket.Write(null));
            }
        }

        [Test]
        public void J1939CanSocket_Read_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var j1939CanSocketTester = new J1939CanSocket())
            using (var j1939CanSocketEcu = new J1939CanSocket())
            {
                j1939CanSocketTester.Bind(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x25);
                j1939CanSocketTester.Connect(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x50);
                j1939CanSocketEcu.Bind(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x50);
                j1939CanSocketEcu.Connect(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x25);

                int bytesWritten = j1939CanSocketTester.Write(new byte[] { 0xFF, 0xFE, 0xFD });
                Assert.AreEqual(3, bytesWritten);

                var data = new byte[3];
                int bytesRead = j1939CanSocketEcu.Read(data);
                Assert.AreEqual(3, bytesRead);
                Assert.IsTrue(data.SequenceEqual(new byte[] { 0xFF, 0xFE, 0xFD }));
            }
        }

        [Test]
        public void J1939CanSocket_GetLatestPacketReceiveTimestamp_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var j1939CanSocketTester = new J1939CanSocket())
            using (var j1939CanSocketEcu = new J1939CanSocket())
            {
                j1939CanSocketTester.Bind(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x25);
                j1939CanSocketTester.Connect(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x50);
                j1939CanSocketEcu.Bind(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x50);
                j1939CanSocketEcu.Connect(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x25);

                int bytesWritten = j1939CanSocketTester.Write(new byte[] { 0xFF, 0xFE, 0xFD });
                Assert.AreEqual(3, bytesWritten);

                var data = new byte[3];
                int bytesRead = j1939CanSocketEcu.Read(data);
                Assert.AreEqual(3, bytesRead);
                Assert.IsTrue(data.SequenceEqual(new byte[] { 0xFF, 0xFE, 0xFD }));

                SocketCanException ex = Assert.Throws<SocketCanException>(() => j1939CanSocketEcu.GetLatestPacketReceiveTimestamp());
                Assert.AreEqual(SocketError.SocketError, ex.SocketErrorCode);
                Assert.AreEqual(25, ex.NativeErrorCode); // ENOTTY
            }
        }

        [Test]
        public void J1939CanSocket_Read_ObjectDisposedException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var j1939CanSocketTester = new J1939CanSocket())
            using (var j1939CanSocketEcu = new J1939CanSocket())
            {
                j1939CanSocketTester.Bind(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x25);
                j1939CanSocketTester.Connect(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x50);
                j1939CanSocketEcu.Bind(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x50);
                j1939CanSocketEcu.Connect(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x25);
                j1939CanSocketEcu.Close();

                int bytesWritten = j1939CanSocketTester.Write(new byte[] { 0xFF, 0xFE, 0xFD });
                Assert.AreEqual(3, bytesWritten);

                var data = new byte[3];
                Assert.Throws<ObjectDisposedException>(() => j1939CanSocketEcu.Read(data));
            }
        }

        [Test]
        public void J1939CanSocket_Read_SocketCanException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var j1939CanSocketTester = new J1939CanSocket())
            using (var j1939CanSocketEcu = new J1939CanSocket())
            {
                j1939CanSocketEcu.ReceiveTimeout = 1000;
                j1939CanSocketTester.Bind(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x25);
                j1939CanSocketTester.Connect(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x50);

                int bytesWritten = j1939CanSocketTester.Write(new byte[] { 0xFF, 0xFE, 0xFD });
                Assert.AreEqual(3, bytesWritten);

                var data = new byte[3];
                SocketCanException ex = Assert.Throws<SocketCanException>(() => j1939CanSocketEcu.Read(data));
                Assert.AreEqual(SocketError.WouldBlock, ex.SocketErrorCode);
                Assert.AreEqual(11, ex.NativeErrorCode);
            }
        }

        [Test]
        public void J1939CanSocket_Read_ArgumentNullException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var j1939CanSocketTester = new J1939CanSocket())
            using (var j1939CanSocketEcu = new J1939CanSocket())
            {
                j1939CanSocketTester.Bind(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x25);
                j1939CanSocketTester.Connect(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x50);
                j1939CanSocketEcu.Bind(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x50);
                j1939CanSocketEcu.Connect(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x25);

                int bytesWritten = j1939CanSocketTester.Write(new byte[] { 0xFF, 0xFE, 0xFD });
                Assert.AreEqual(3, bytesWritten);

                var data = new byte[3];
                Assert.Throws<ArgumentNullException>(() => j1939CanSocketEcu.Read(null));
            }
        }

        [Test]
        public void J1939CanSocket_Write_NonBlocking_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var j1939CanSocketTester = new J1939CanSocket())
            {
                j1939CanSocketTester.Blocking = false;
                j1939CanSocketTester.Bind(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x25);
                j1939CanSocketTester.Connect(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x50);

                int bytesWritten = j1939CanSocketTester.Write(new byte[] { 0xFF, 0xFE, 0xFD });
                Assert.AreEqual(3, bytesWritten);
            }
        }

        [Test]
        public void J1939CanSocket_Read_NonBlocking_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var j1939CanSocketTester = new J1939CanSocket())
            using (var j1939CanSocketEcu = new J1939CanSocket())
            {
                j1939CanSocketTester.Blocking = false;
                j1939CanSocketEcu.Blocking = false;
                j1939CanSocketTester.Bind(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x25);
                j1939CanSocketTester.Connect(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x50);
                j1939CanSocketEcu.Bind(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x50);
                j1939CanSocketEcu.Connect(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x25);

                int bytesWritten = j1939CanSocketTester.Write(new byte[] { 0xFF, 0xFE, 0xFD });
                Assert.AreEqual(3, bytesWritten);

                var data = new byte[3];
                int bytesRead = j1939CanSocketEcu.Read(data);
                Assert.AreEqual(3, bytesRead);
                Assert.IsTrue(data.SequenceEqual(new byte[] { 0xFF, 0xFE, 0xFD }));
            }
        }

        [Test]
        public void J1939CanSocket_Write_NoConnect_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var j1939CanSocketTester = new J1939CanSocket())
            {
                j1939CanSocketTester.Bind(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x25);
                SocketCanException ex = Assert.Throws<SocketCanException>(() => j1939CanSocketTester.Write(new byte[] { 0xFF, 0xFE, 0xFD }));
                Assert.AreEqual(SocketError.AccessDenied, ex.SocketErrorCode);
                Assert.AreEqual(13, ex.NativeErrorCode); //EACCES
            }
        }

        [Test]
        public void J1939CanSocket_Close_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var j1939CanSocket = new J1939CanSocket())
            {
                j1939CanSocket.Close();
            }
        }

        [Test]
        public void J1939CanSocket_GetLocalAddress_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var j1939CanSocket = new J1939CanSocket())
            {
                j1939CanSocket.Bind(iface, 0x00000010, 0x1b100, 0x25);
                Assert.AreEqual(true, j1939CanSocket.IsBound);
                SockAddrCanJ1939 addr = j1939CanSocket.LocalAddress;
                Assert.AreEqual(SocketCanConstants.AF_CAN, addr.CanFamily);
                Assert.AreEqual(iface.Index, addr.CanIfIndex);
                Assert.AreEqual(0x00000010, addr.Name);
                Assert.AreEqual(SocketCanConstants.J1939_NO_PGN, addr.PGN); // socket is disconnected so PGN will be set to N/A value.
                Assert.AreEqual(0x25, addr.Address);
            }
        }

        [Test]
        public void J1939CanSocket_GetLocalAddress_NotBound_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var j1939CanSocket = new J1939CanSocket())
            {
                Assert.AreEqual(false, j1939CanSocket.IsBound);
                SockAddrCanJ1939 addr = j1939CanSocket.LocalAddress;
                Assert.AreEqual(SocketCanConstants.AF_CAN, addr.CanFamily);
                Assert.AreEqual(0, addr.CanIfIndex);
                Assert.AreEqual(SocketCanConstants.J1939_NO_NAME, addr.Name);
                Assert.AreEqual(SocketCanConstants.J1939_NO_PGN, addr.PGN);
                Assert.AreEqual(SocketCanConstants.J1939_NO_ADDR, addr.Address);
            }
        }

        [Test]
        public void J1939CanSocket_GetLocalAddress_ObjectDisposedException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var j1939CanSocket = new J1939CanSocket())
            {
                j1939CanSocket.Bind(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x25);
                Assert.AreEqual(true, j1939CanSocket.IsBound);
                j1939CanSocket.Close();
                Assert.Throws<ObjectDisposedException>(() => { SockAddrCanJ1939 addr = j1939CanSocket.LocalAddress; });
            }
        }

        [Test]
        public void J1939CanSocket_GetRemoteAddress_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var j1939CanSocket = new J1939CanSocket())
            {
                j1939CanSocket.Bind(iface, 0x00000010, 0x1b100, 0x25);
                Assert.AreEqual(true, j1939CanSocket.IsBound);
                j1939CanSocket.Connect(iface, 0x00000020, 0x1b200, 0x50);
                Assert.AreEqual(true, j1939CanSocket.Connected);
                SockAddrCanJ1939 addr = j1939CanSocket.RemoteAddress;
                Assert.AreEqual(SocketCanConstants.AF_CAN, addr.CanFamily);
                Assert.AreEqual(iface.Index, addr.CanIfIndex);
                Assert.AreEqual(0x00000020, addr.Name);
                Assert.AreEqual(0x1b200, addr.PGN);
                Assert.AreEqual(0x50, addr.Address);
            }
        }

        [Test]
        public void J1939CanSocket_GetRemoteAddress_NotConnected_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var j1939CanSocket = new J1939CanSocket())
            {
                j1939CanSocket.Bind(iface, 0x00000010, 0x1b100, 0x25);
                Assert.AreEqual(true, j1939CanSocket.IsBound);
                Assert.AreEqual(false, j1939CanSocket.Connected);
                SocketCanException ex = Assert.Throws<SocketCanException>(() => { SockAddrCanJ1939 addr = j1939CanSocket.RemoteAddress; });
                Assert.AreEqual(SocketError.AddressNotAvailable, ex.SocketErrorCode);
                Assert.AreEqual(99, ex.NativeErrorCode); //EADDRNOTAVAIL
            }
        }

        [Test]
        public void J1939CanSocket_GetRemoteAddress_ObjectDisposedException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var j1939CanSocket = new J1939CanSocket())
            {
                j1939CanSocket.Bind(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x25);
                Assert.AreEqual(true, j1939CanSocket.IsBound);
                j1939CanSocket.Connect(iface, 0x00000020, 0x1b200, 0x50);
                Assert.AreEqual(true, j1939CanSocket.Connected);

                j1939CanSocket.Close();
                Assert.Throws<ObjectDisposedException>(() => { SockAddrCanJ1939 addr = j1939CanSocket.RemoteAddress; });
            }
        }

        [Test]
        public void J1939CanSocket_Write_NonBlocking_Flag_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var j1939CanSocket = new J1939CanSocket())
            {
                j1939CanSocket.Bind(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x26);
                j1939CanSocket.Connect(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x56);
                int bytesWritten = j1939CanSocket.Write(new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89 }, MessageFlags.MSG_DONTWAIT);
                Assert.AreEqual(5, bytesWritten);
            }
        }

        [Test]
        public void J1939CanSocket_Read_NonBlocking_Flag_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var j1939CanSocketTester = new J1939CanSocket())
            using (var j1939CanSocketEcu = new J1939CanSocket())
            {
                j1939CanSocketTester.Bind(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x25);
                j1939CanSocketTester.Connect(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x50);
                j1939CanSocketEcu.Bind(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x50);
                j1939CanSocketEcu.Connect(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x25);

                int bytesWritten = j1939CanSocketTester.Write(new byte[] { 0xFF, 0xFE, 0xFD });
                Assert.AreEqual(3, bytesWritten);

                var data = new byte[3];
                int bytesRead = j1939CanSocketEcu.Read(data, MessageFlags.MSG_DONTWAIT);
                Assert.AreEqual(3, bytesRead);
                Assert.IsTrue(data.SequenceEqual(new byte[] { 0xFF, 0xFE, 0xFD }));
            }
        }

        [Test]
        public void J1939CanSocket_WriteTo_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var j1939CanSocket = new J1939CanSocket())
            {
                j1939CanSocket.Bind(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x26);
                var dstAddr = new SockAddrCanJ1939(0)
                {
                    Name = SocketCanConstants.J1939_NO_NAME,
                    PGN = 0x18300, // R: 0, DP: 1, PF: $83, PS: $00 as Address holds value 
                    Address = 0x35,
                };
                int bytesWritten = j1939CanSocket.WriteTo(new byte[] { 0xbe, 0xef, 0xfe, 0xeb }, MessageFlags.None, dstAddr);
                Assert.AreEqual(4, bytesWritten);
            }
        }

        [Test]
        public void J1939CanSocket_ReadFrom_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var j1939CanSocketTester = new J1939CanSocket())
            using (var j1939CanSocketEcu = new J1939CanSocket())
            {
                j1939CanSocketTester.Bind(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x25);
                j1939CanSocketTester.Connect(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x50);
                j1939CanSocketEcu.Bind(iface, SocketCanConstants.J1939_NO_NAME, 0x1b100, 0x50);

                int bytesWritten = j1939CanSocketTester.Write(new byte[] { 0xFF, 0xFE, 0xFD, 0xFC, 0xFB, 0xFA });
                Assert.AreEqual(6, bytesWritten);

                var srcAddr = new SockAddrCanJ1939(0)
                {
                    Name = SocketCanConstants.J1939_NO_NAME,
                    PGN = SocketCanConstants.J1939_NO_PGN,
                    Address = SocketCanConstants.J1939_NO_ADDR,
                };
                var data = new byte[6];
                int bytesRead = j1939CanSocketEcu.ReadFrom(data, MessageFlags.None, srcAddr);
                Assert.AreEqual(6, bytesRead);
                Assert.IsTrue(data.SequenceEqual(new byte[] { 0xFF, 0xFE, 0xFD, 0xFC, 0xFB, 0xFA }));
                Assert.AreEqual(0x25, srcAddr.Address);
            }
        }
    }
}