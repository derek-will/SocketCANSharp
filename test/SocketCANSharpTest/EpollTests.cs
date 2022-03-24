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

using System;
using System.Linq;
using NUnit.Framework;
using SocketCANSharp;
using SocketCANSharp.Network;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace SocketCANSharpTest
{
    public class EpollTests
    {
        [Test]
        public void EpollCreate_Success_Test()
        {
            SafeFileDescriptorHandle handle = LibcNativeMethods.EpollCreate(1);
            Assert.IsFalse(handle.IsInvalid);
            Assert.IsFalse(handle.IsClosed);

            handle.Close();
            Assert.IsTrue(handle.IsInvalid);
            Assert.IsTrue(handle.IsClosed);
        }

        [Test]
        public void EpollCreate_Failure_Zero_Test()
        {
            SafeFileDescriptorHandle handle = LibcNativeMethods.EpollCreate(0);
            Assert.IsTrue(handle.IsInvalid);
            Assert.IsFalse(handle.IsClosed);

            handle.Close();
            Assert.IsTrue(handle.IsInvalid);
            Assert.IsTrue(handle.IsClosed);
        }

        [Test]
        public void EpollCreate_Failure_Negative_Test()
        {
            SafeFileDescriptorHandle handle = LibcNativeMethods.EpollCreate(-1);
            Assert.IsTrue(handle.IsInvalid);
            Assert.IsFalse(handle.IsClosed);

            handle.Close();
            Assert.IsTrue(handle.IsInvalid);
            Assert.IsTrue(handle.IsClosed);
        }

        [Test]
        public void Epoll_SafeFileDescriptorHandle_Close_Success_Test()
        {
            SafeFileDescriptorHandle handle = LibcNativeMethods.EpollCreate(1);
            Assert.IsFalse(handle.IsInvalid);
            Assert.IsFalse(handle.IsClosed);

            int value = LibcNativeMethods.Close(handle.DangerousGetHandle());
            Assert.AreEqual(0, value);
        }

        [Test]
        public void EpollControl_Add_Success_Test()
        {
            using (SafeSocketHandle socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW))
            {
                Assert.IsFalse(socketHandle.IsInvalid);

                using (SafeFileDescriptorHandle handle = LibcNativeMethods.EpollCreate(1))
                {
                    Assert.IsFalse(handle.IsInvalid);
                    Assert.IsFalse(handle.IsClosed);

                    var epollEvent = new EpollEvent()
                    {
                        Events = EpollEventType.EPOLLIN,
                        Data = IntPtr.Zero,
                    };
                    int result = LibcNativeMethods.EpollControl(handle, EpollOperation.EPOLL_CTL_ADD, socketHandle, ref epollEvent);
                    Assert.AreEqual(0, result);
                }
            }
        }

        [Test]
        public void EpollControl_Delete_Success_Test()
        {
            using (SafeSocketHandle socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW))
            {
                Assert.IsFalse(socketHandle.IsInvalid);

                using (SafeFileDescriptorHandle handle = LibcNativeMethods.EpollCreate(1))
                {
                    Assert.IsFalse(handle.IsInvalid);
                    Assert.IsFalse(handle.IsClosed);

                    var epollEvent = new EpollEvent()
                    {
                        Events = EpollEventType.EPOLLIN,
                        Data = IntPtr.Zero,
                    };
                    int result = LibcNativeMethods.EpollControl(handle, EpollOperation.EPOLL_CTL_ADD, socketHandle, ref epollEvent);
                    Assert.AreEqual(0, result);

                    result = LibcNativeMethods.EpollControl(handle, EpollOperation.EPOLL_CTL_DEL, socketHandle, ref epollEvent);
                    Assert.AreEqual(0, result);
                }
            }
        }

        [Test]
        public void EpollControl_Modify_Success_Test()
        {
            using (SafeSocketHandle socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW))
            {
                Assert.IsFalse(socketHandle.IsInvalid);

                using (SafeFileDescriptorHandle handle = LibcNativeMethods.EpollCreate(1))
                {
                    Assert.IsFalse(handle.IsInvalid);
                    Assert.IsFalse(handle.IsClosed);

                    var epollEvent = new EpollEvent()
                    {
                        Events = EpollEventType.EPOLLIN,
                        Data = IntPtr.Zero,
                    };
                    int result = LibcNativeMethods.EpollControl(handle, EpollOperation.EPOLL_CTL_ADD, socketHandle, ref epollEvent);
                    Assert.AreEqual(0, result);

                    epollEvent = new EpollEvent()
                    {
                        Events = EpollEventType.EPOLLIN | EpollEventType.EPOLLOUT,
                        Data = IntPtr.Zero,
                    };
                    result = LibcNativeMethods.EpollControl(handle, EpollOperation.EPOLL_CTL_MOD, socketHandle, ref epollEvent);
                    Assert.AreEqual(0, result);
                }
            }
        }

        [Test]
        public void EpollControl_Add_with_Data_Success_Test()
        {
            using (SafeSocketHandle socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW))
            {
                Assert.IsFalse(socketHandle.IsInvalid);

                using (SafeFileDescriptorHandle handle = LibcNativeMethods.EpollCreate(1))
                {
                    Assert.IsFalse(handle.IsInvalid);
                    Assert.IsFalse(handle.IsClosed);

                    var epollEvent = new EpollEvent()
                    {
                        Events = EpollEventType.EPOLLIN,
                        Data = socketHandle.DangerousGetHandle(),
                    };
                    int result = LibcNativeMethods.EpollControl(handle, EpollOperation.EPOLL_CTL_ADD, socketHandle, ref epollEvent);
                    Assert.AreEqual(0, result);
                }
            }
        }

        [Test]
        public void EpollControl_Add_Invalid_Handle_Failure_Test()
        {
            using (SafeSocketHandle socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW))
            {
                Assert.IsFalse(socketHandle.IsInvalid);

                using (SafeFileDescriptorHandle handle = LibcNativeMethods.EpollCreate(-1))
                {
                    Assert.IsTrue(handle.IsInvalid);
                    Assert.IsFalse(handle.IsClosed);

                    var epollEvent = new EpollEvent()
                    {
                        Events = EpollEventType.EPOLLIN,
                        Data = IntPtr.Zero,
                    };
                    int result = LibcNativeMethods.EpollControl(handle, EpollOperation.EPOLL_CTL_ADD, socketHandle, ref epollEvent);
                    Assert.AreEqual(-1, result);
                }
            }
        }

        [Test]
        public void EpollControl_Invalid_Operation_Failure_Test()
        {
            using (SafeSocketHandle socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW))
            {
                Assert.IsFalse(socketHandle.IsInvalid);

                using (SafeFileDescriptorHandle handle = LibcNativeMethods.EpollCreate(1))
                {
                    Assert.IsFalse(handle.IsInvalid);
                    Assert.IsFalse(handle.IsClosed);

                    var epollEvent = new EpollEvent()
                    {
                        Events = EpollEventType.EPOLLIN,
                        Data = IntPtr.Zero,
                    };
                    int result = LibcNativeMethods.EpollControl(handle, (EpollOperation)35, socketHandle, ref epollEvent);
                    Assert.AreEqual(-1, result);
                }
            }
        }

        [Test]
        public void EpollControl_Invalid_SocketHandle_Failure_Test()
        {
            using (SafeSocketHandle socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Seqpacket, SocketCanProtocolType.CAN_RAW))
            {
                Assert.IsTrue(socketHandle.IsInvalid);

                using (SafeFileDescriptorHandle handle = LibcNativeMethods.EpollCreate(1))
                {
                    Assert.IsFalse(handle.IsInvalid);
                    Assert.IsFalse(handle.IsClosed);

                    var epollEvent = new EpollEvent()
                    {
                        Events = EpollEventType.EPOLLIN,
                        Data = IntPtr.Zero,
                    };
                    int result = LibcNativeMethods.EpollControl(handle, EpollOperation.EPOLL_CTL_ADD, socketHandle, ref epollEvent);
                    Assert.AreEqual(-1, result);
                }
            }
        }

        [Test]
        public void EpollControl_Modify_EPOLLEXCLUSIVE_Failure_Test()
        {
            using (SafeSocketHandle socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW))
            {
                Assert.IsFalse(socketHandle.IsInvalid);

                using (SafeFileDescriptorHandle handle = LibcNativeMethods.EpollCreate(1))
                {
                    Assert.IsFalse(handle.IsInvalid);
                    Assert.IsFalse(handle.IsClosed);

                    var epollEvent = new EpollEvent()
                    {
                        Events = EpollEventType.EPOLLIN,
                        Data = IntPtr.Zero,
                    };
                    int result = LibcNativeMethods.EpollControl(handle, EpollOperation.EPOLL_CTL_ADD, socketHandle, ref epollEvent);
                    Assert.AreEqual(0, result);

                    epollEvent = new EpollEvent()
                    {
                        Events = EpollEventType.EPOLLIN | EpollEventType.EPOLLOUT | EpollEventType.EPOLLEXCLUSIVE,
                        Data = IntPtr.Zero,
                    };
                    result = LibcNativeMethods.EpollControl(handle, EpollOperation.EPOLL_CTL_MOD, socketHandle, ref epollEvent);
                    Assert.AreEqual(-1, result);
                }
            }
        }

        [Test]
        public void EpollWait_Success_Test()
        {
            using (SafeSocketHandle readerHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW))
            using (SafeSocketHandle writerHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW))
            {
                Assert.IsFalse(readerHandle.IsInvalid);
                Assert.IsFalse(writerHandle.IsInvalid);

                var ifr = new Ifreq("vcan0");
                int ioctlResult = LibcNativeMethods.Ioctl(readerHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
                Assert.AreNotEqual(-1, ioctlResult);

                var addr = new SockAddrCan(ifr.IfIndex);
                int bindResult = LibcNativeMethods.Bind(readerHandle, addr, Marshal.SizeOf(typeof(SockAddrCan)));
                Assert.AreNotEqual(-1, bindResult);

                bindResult = LibcNativeMethods.Bind(writerHandle, addr, Marshal.SizeOf(typeof(SockAddrCan)));
                Assert.AreNotEqual(-1, bindResult);

                var canFrame = new CanFrame(0x123, new byte[] { 0x11, 0x22 });

                using (SafeFileDescriptorHandle handle = LibcNativeMethods.EpollCreate(1))
                {
                    Assert.IsFalse(handle.IsInvalid);
                    Assert.IsFalse(handle.IsClosed);

                    var epollEvent = new EpollEvent()
                    {
                        Events = EpollEventType.EPOLLIN,
                        Data = readerHandle.DangerousGetHandle(),
                    };
                    int result = LibcNativeMethods.EpollControl(handle, EpollOperation.EPOLL_CTL_ADD, readerHandle, ref epollEvent);
                    Assert.AreEqual(0, result);

                    int nBytes = LibcNativeMethods.Write(writerHandle, ref canFrame, Marshal.SizeOf(typeof(CanFrame)));
                    Assert.AreEqual(16, nBytes);

                    var events = new EpollEvent[5];
                    int numEvents = LibcNativeMethods.EpollWait(handle, events, 1, 1000);
                    Assert.AreEqual(1, numEvents);
                    Assert.IsTrue(events[0].Events.HasFlag(EpollEventType.EPOLLIN));
                    Assert.AreEqual(readerHandle.DangerousGetHandle(), events[0].Data);
                }
            }
        }

        [Test]
        public void EpollWait_MaxEvents_Zero_Failure_Test()
        {
            using (SafeSocketHandle readerHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW))
            using (SafeSocketHandle writerHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW))
            {
                Assert.IsFalse(readerHandle.IsInvalid);
                Assert.IsFalse(writerHandle.IsInvalid);

                var ifr = new Ifreq("vcan0");
                int ioctlResult = LibcNativeMethods.Ioctl(readerHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
                Assert.AreNotEqual(-1, ioctlResult);

                var addr = new SockAddrCan(ifr.IfIndex);
                int bindResult = LibcNativeMethods.Bind(readerHandle, addr, Marshal.SizeOf(typeof(SockAddrCan)));
                Assert.AreNotEqual(-1, bindResult);

                bindResult = LibcNativeMethods.Bind(writerHandle, addr, Marshal.SizeOf(typeof(SockAddrCan)));
                Assert.AreNotEqual(-1, bindResult);

                var canFrame = new CanFrame(0x123, new byte[] { 0x11, 0x22 });

                using (SafeFileDescriptorHandle handle = LibcNativeMethods.EpollCreate(1))
                {
                    Assert.IsFalse(handle.IsInvalid);
                    Assert.IsFalse(handle.IsClosed);

                    var epollEvent = new EpollEvent()
                    {
                        Events = EpollEventType.EPOLLIN,
                        Data = readerHandle.DangerousGetHandle(),
                    };
                    int result = LibcNativeMethods.EpollControl(handle, EpollOperation.EPOLL_CTL_ADD, readerHandle, ref epollEvent);
                    Assert.AreEqual(0, result);

                    int nBytes = LibcNativeMethods.Write(writerHandle, ref canFrame, Marshal.SizeOf(typeof(CanFrame)));
                    Assert.AreEqual(16, nBytes);

                    var events = new EpollEvent[5];
                    int numEvents = LibcNativeMethods.EpollWait(handle, events, 0, 1000);
                    Assert.AreEqual(-1, numEvents);
                }
            }
        }

        [Test]
        public void EpollWait_Invalid_Handle_Failure_Test()
        {
            using (SafeFileDescriptorHandle handle = LibcNativeMethods.EpollCreate(-1))
            {
                Assert.IsTrue(handle.IsInvalid);
                Assert.IsFalse(handle.IsClosed);

                var events = new EpollEvent[5];
                int numEvents = LibcNativeMethods.EpollWait(handle, events, 1, 1000);
                Assert.AreEqual(-1, numEvents);
            }
        }

        [Test]
        public void EpollWait_NullEventsArray_Failure_Test()
        {
            using (SafeSocketHandle readerHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW))
            using (SafeSocketHandle writerHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW))
            {
                Assert.IsFalse(readerHandle.IsInvalid);
                Assert.IsFalse(writerHandle.IsInvalid);

                var ifr = new Ifreq("vcan0");
                int ioctlResult = LibcNativeMethods.Ioctl(readerHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
                Assert.AreNotEqual(-1, ioctlResult);

                var addr = new SockAddrCan(ifr.IfIndex);
                int bindResult = LibcNativeMethods.Bind(readerHandle, addr, Marshal.SizeOf(typeof(SockAddrCan)));
                Assert.AreNotEqual(-1, bindResult);

                bindResult = LibcNativeMethods.Bind(writerHandle, addr, Marshal.SizeOf(typeof(SockAddrCan)));
                Assert.AreNotEqual(-1, bindResult);

                var canFrame = new CanFrame(0x123, new byte[] { 0x11, 0x22 });

                using (SafeFileDescriptorHandle handle = LibcNativeMethods.EpollCreate(1))
                {
                    Assert.IsFalse(handle.IsInvalid);
                    Assert.IsFalse(handle.IsClosed);

                    var epollEvent = new EpollEvent()
                    {
                        Events = EpollEventType.EPOLLIN,
                        Data = readerHandle.DangerousGetHandle(),
                    };
                    int result = LibcNativeMethods.EpollControl(handle, EpollOperation.EPOLL_CTL_ADD, readerHandle, ref epollEvent);
                    Assert.AreEqual(0, result);

                    int nBytes = LibcNativeMethods.Write(writerHandle, ref canFrame, Marshal.SizeOf(typeof(CanFrame)));
                    Assert.AreEqual(16, nBytes);

                    var events = new EpollEvent[0];
                    int numEvents = LibcNativeMethods.EpollWait(handle, null, 1, 1000);
                    Assert.AreEqual(-1, numEvents);
                }
            }
        }

        [Test]
        public void EpollControl_Delete_Not_Found_Failure_Test()
        {
            using (SafeSocketHandle socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW))
            {
                Assert.IsFalse(socketHandle.IsInvalid);

                using (SafeFileDescriptorHandle handle = LibcNativeMethods.EpollCreate(1))
                {
                    Assert.IsFalse(handle.IsInvalid);
                    Assert.IsFalse(handle.IsClosed);

                    var epollEvent = new EpollEvent()
                    {
                        Events = EpollEventType.EPOLLIN,
                        Data = IntPtr.Zero,
                    };

                    int result = LibcNativeMethods.EpollControl(handle, EpollOperation.EPOLL_CTL_DEL, socketHandle, ref epollEvent);
                    Assert.AreEqual(-1, result);
                }
            }
        }

        [Test]
        public void EpollControl_Modify_Not_Found_Failure_Test()
        {
            using (SafeSocketHandle socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW))
            {
                Assert.IsFalse(socketHandle.IsInvalid);

                using (SafeFileDescriptorHandle handle = LibcNativeMethods.EpollCreate(1))
                {
                    Assert.IsFalse(handle.IsInvalid);
                    Assert.IsFalse(handle.IsClosed);

                    var epollEvent = new EpollEvent()
                    {
                        Events = EpollEventType.EPOLLIN,
                        Data = IntPtr.Zero,
                    };
                    int result = LibcNativeMethods.EpollControl(handle, EpollOperation.EPOLL_CTL_MOD, socketHandle, ref epollEvent);
                    Assert.AreEqual(-1, result);
                }
            }
        }

        [Test]
        public void EpollControl_Add_Twice_Failure_Test()
        {
            using (SafeSocketHandle socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW))
            {
                Assert.IsFalse(socketHandle.IsInvalid);

                using (SafeFileDescriptorHandle handle = LibcNativeMethods.EpollCreate(1))
                {
                    Assert.IsFalse(handle.IsInvalid);
                    Assert.IsFalse(handle.IsClosed);

                    var epollEvent = new EpollEvent()
                    {
                        Events = EpollEventType.EPOLLIN,
                        Data = IntPtr.Zero,
                    };
                    int result = LibcNativeMethods.EpollControl(handle, EpollOperation.EPOLL_CTL_ADD, socketHandle, ref epollEvent);
                    Assert.AreEqual(0, result);

                    result = LibcNativeMethods.EpollControl(handle, EpollOperation.EPOLL_CTL_ADD, socketHandle, ref epollEvent);
                    Assert.AreEqual(-1, result);
                }
            }
        }

        [Test]
        public void Epoll_Ctor_Test()
        {
            var epoll = new Epoll();
            Assert.IsFalse(epoll.SafeHandle.IsClosed);
            Assert.IsFalse(epoll.SafeHandle.IsInvalid);
            epoll.Close();
        }

        [Test]
        public void Epoll_Close_Test()
        {
            var epoll = new Epoll();
            Assert.IsFalse(epoll.SafeHandle.IsClosed);
            Assert.IsFalse(epoll.SafeHandle.IsInvalid);
            epoll.Close();
            Assert.IsTrue(epoll.SafeHandle.IsClosed);
            Assert.IsTrue(epoll.SafeHandle.IsInvalid);
        }

        [Test]
        public void Epoll_Dispose_Test()
        {
            var epoll = new Epoll();
            Assert.IsFalse(epoll.SafeHandle.IsClosed);
            Assert.IsFalse(epoll.SafeHandle.IsInvalid);
            epoll.Dispose();
            Assert.IsTrue(epoll.SafeHandle.IsClosed);
            Assert.IsTrue(epoll.SafeHandle.IsInvalid);
        }

        [Test]
        public void Epoll_Add_Test()
        {
            using (var epoll = new Epoll())
            {
                Assert.IsFalse(epoll.SafeHandle.IsClosed);
                Assert.IsFalse(epoll.SafeHandle.IsInvalid);

                using (var isoTpCanSocket = new IsoTpCanSocket())
                {
                    var epollEvent = new EpollEvent()
                    {
                        Events = EpollEventType.EPOLLIN,
                        Data = isoTpCanSocket.Handle,
                    };
                    epoll.Add(isoTpCanSocket.SafeHandle, epollEvent);
                }
            }
        }

        [Test]
        public void Epoll_Add_Twice_Failure_Test()
        {
            using (var epoll = new Epoll())
            {
                Assert.IsFalse(epoll.SafeHandle.IsClosed);
                Assert.IsFalse(epoll.SafeHandle.IsInvalid);

                using (var isoTpCanSocket = new IsoTpCanSocket())
                {
                    var epollEvent = new EpollEvent()
                    {
                        Events = EpollEventType.EPOLLIN,
                        Data = isoTpCanSocket.Handle,
                    };
                    epoll.Add(isoTpCanSocket.SafeHandle, epollEvent);
                    SocketException ex = Assert.Throws<SocketException>(() => epoll.Add(isoTpCanSocket.SafeHandle, epollEvent));
                    Assert.AreEqual(SocketError.SocketError, ex.SocketErrorCode);
                    Assert.AreEqual(17, ex.NativeErrorCode);
                }
            }
        }

        [Test]
        public void Epoll_Add_Null_SafeSocketHandle_Failure_Test()
        {
            using (var epoll = new Epoll())
            {
                Assert.IsFalse(epoll.SafeHandle.IsClosed);
                Assert.IsFalse(epoll.SafeHandle.IsInvalid);

                using (var isoTpCanSocket = new IsoTpCanSocket())
                {
                    var epollEvent = new EpollEvent()
                    {
                        Events = EpollEventType.EPOLLIN,
                        Data = isoTpCanSocket.Handle,
                    };
                    Assert.Throws<ArgumentNullException>(() => epoll.Add(null, epollEvent));
                }
            }
        }

        [Test]
        public void Epoll_Add_Invalid_Event_With_EPOLLEXCLUSIVE_Failure_Test()
        {
            using (var epoll = new Epoll())
            {
                Assert.IsFalse(epoll.SafeHandle.IsClosed);
                Assert.IsFalse(epoll.SafeHandle.IsInvalid);

                using (var isoTpCanSocket = new IsoTpCanSocket())
                {
                    var epollEvent = new EpollEvent()
                    {
                        Events = (EpollEventType)0xffffffff,
                        Data = isoTpCanSocket.Handle,
                    };
                    SocketException ex = Assert.Throws<SocketException>(() => epoll.Add(isoTpCanSocket.SafeHandle, epollEvent));
                    Assert.AreEqual(SocketError.InvalidArgument, ex.SocketErrorCode);
                    Assert.AreEqual(22, ex.NativeErrorCode);
                }
            }
        }

        [Test]
        public void Epoll_Add_ObjectDisposedException_Failure_Test()
        {
            using (var epoll = new Epoll())
            {
                Assert.IsFalse(epoll.SafeHandle.IsClosed);
                Assert.IsFalse(epoll.SafeHandle.IsInvalid);
                epoll.Close();

                using (var isoTpCanSocket = new IsoTpCanSocket())
                {
                    var epollEvent = new EpollEvent()
                    {
                        Events = EpollEventType.EPOLLIN,
                        Data = isoTpCanSocket.Handle,
                    };
                    Assert.Throws<ObjectDisposedException>(() => epoll.Add(isoTpCanSocket.SafeHandle, epollEvent));
                }
            }
        }

        [Test]
        public void Epoll_Remove_Test()
        {
            using (var epoll = new Epoll())
            {
                Assert.IsFalse(epoll.SafeHandle.IsClosed);
                Assert.IsFalse(epoll.SafeHandle.IsInvalid);

                using (var isoTpCanSocket = new IsoTpCanSocket())
                {
                    var epollEvent = new EpollEvent()
                    {
                        Events = EpollEventType.EPOLLIN,
                        Data = isoTpCanSocket.Handle,
                    };
                    epoll.Add(isoTpCanSocket.SafeHandle, epollEvent);
                    epoll.Remove(isoTpCanSocket.SafeHandle);
                }
            }
        }

        [Test]
        public void Epoll_Remove_Not_Added_Failure_Test()
        {
            using (var epoll = new Epoll())
            {
                Assert.IsFalse(epoll.SafeHandle.IsClosed);
                Assert.IsFalse(epoll.SafeHandle.IsInvalid);

                using (var isoTpCanSocket = new IsoTpCanSocket())
                {
                    var epollEvent = new EpollEvent()
                    {
                        Events = EpollEventType.EPOLLIN,
                        Data = isoTpCanSocket.Handle,
                    };
                    SocketException ex = Assert.Throws<SocketException>(() => epoll.Remove(isoTpCanSocket.SafeHandle));
                    Assert.AreEqual(SocketError.AddressNotAvailable, ex.SocketErrorCode);
                    Assert.AreEqual(2, ex.NativeErrorCode);
                }
            }
        }

        [Test]
        public void Epoll_Remove_Null_SafeSocketHandle_Failure_Test()
        {
            using (var epoll = new Epoll())
            {
                Assert.IsFalse(epoll.SafeHandle.IsClosed);
                Assert.IsFalse(epoll.SafeHandle.IsInvalid);

                using (var isoTpCanSocket = new IsoTpCanSocket())
                {
                    var epollEvent = new EpollEvent()
                    {
                        Events = EpollEventType.EPOLLIN,
                        Data = isoTpCanSocket.Handle,
                    };
                    epoll.Add(isoTpCanSocket.SafeHandle, epollEvent);
                    Assert.Throws<ArgumentNullException>(() => epoll.Remove(null));
                }
            }
        }

        [Test]
        public void Epoll_Remove_ObjectDisposedException_Failure_Test()
        {
            using (var epoll = new Epoll())
            {
                Assert.IsFalse(epoll.SafeHandle.IsClosed);
                Assert.IsFalse(epoll.SafeHandle.IsInvalid);

                using (var isoTpCanSocket = new IsoTpCanSocket())
                {
                    var epollEvent = new EpollEvent()
                    {
                        Events = EpollEventType.EPOLLIN,
                        Data = isoTpCanSocket.Handle,
                    };
                    epoll.Add(isoTpCanSocket.SafeHandle, epollEvent);
                    epoll.Close();
                    Assert.Throws<ObjectDisposedException>(() => epoll.Remove(isoTpCanSocket.SafeHandle));
                }
            }
        }

        [Test]
        public void Epoll_Modify_Test()
        {
            using (var epoll = new Epoll())
            {
                Assert.IsFalse(epoll.SafeHandle.IsClosed);
                Assert.IsFalse(epoll.SafeHandle.IsInvalid);

                using (var isoTpCanSocket = new IsoTpCanSocket())
                {
                    var epollEvent = new EpollEvent()
                    {
                        Events = EpollEventType.EPOLLIN,
                        Data = isoTpCanSocket.Handle,
                    };
                    epoll.Add(isoTpCanSocket.SafeHandle, epollEvent);

                    epollEvent.Events |= EpollEventType.EPOLLET;
                    epoll.Modify(isoTpCanSocket.SafeHandle, epollEvent);
                }
            }
        }

        [Test]
        public void Epoll_Modify_Include_EPOLLEXCLUSIVE_Failure_Test()
        {
            using (var epoll = new Epoll())
            {
                Assert.IsFalse(epoll.SafeHandle.IsClosed);
                Assert.IsFalse(epoll.SafeHandle.IsInvalid);

                using (var isoTpCanSocket = new IsoTpCanSocket())
                {
                    var epollEvent = new EpollEvent()
                    {
                        Events = EpollEventType.EPOLLIN,
                        Data = isoTpCanSocket.Handle,
                    };
                    epoll.Add(isoTpCanSocket.SafeHandle, epollEvent);

                    epollEvent.Events |= EpollEventType.EPOLLET | EpollEventType.EPOLLEXCLUSIVE;
                    SocketException ex = Assert.Throws<SocketException>(() => epoll.Modify(isoTpCanSocket.SafeHandle, epollEvent));
                    Assert.AreEqual(SocketError.InvalidArgument, ex.SocketErrorCode);
                    Assert.AreEqual(22, ex.NativeErrorCode);
                }
            }
        }

        [Test]
        public void Epoll_Modify_Not_Added_Failure_Test()
        {
            using (var epoll = new Epoll())
            {
                Assert.IsFalse(epoll.SafeHandle.IsClosed);
                Assert.IsFalse(epoll.SafeHandle.IsInvalid);

                using (var isoTpCanSocket = new IsoTpCanSocket())
                {
                    var epollEvent = new EpollEvent()
                    {
                        Events = EpollEventType.EPOLLIN,
                        Data = isoTpCanSocket.Handle,
                    };

                    epollEvent.Events |= EpollEventType.EPOLLET;
                    SocketException ex = Assert.Throws<SocketException>(() => epoll.Modify(isoTpCanSocket.SafeHandle, epollEvent));
                    Assert.AreEqual(SocketError.AddressNotAvailable, ex.SocketErrorCode);
                    Assert.AreEqual(2, ex.NativeErrorCode);
                }
            }
        }

        [Test]
        public void Epoll_Modify_Null_SafeSocketHandle_Failure_Test()
        {
            using (var epoll = new Epoll())
            {
                Assert.IsFalse(epoll.SafeHandle.IsClosed);
                Assert.IsFalse(epoll.SafeHandle.IsInvalid);

                using (var isoTpCanSocket = new IsoTpCanSocket())
                {
                    var epollEvent = new EpollEvent()
                    {
                        Events = EpollEventType.EPOLLIN,
                        Data = isoTpCanSocket.Handle,
                    };
                    epoll.Add(isoTpCanSocket.SafeHandle, epollEvent);

                    epollEvent.Events |= EpollEventType.EPOLLET;
                    Assert.Throws<ArgumentNullException>(() => epoll.Modify(null, epollEvent));
                }
            }
        }

        [Test]
        public void Epoll_Modify_ObjectDisposedException_Failure_Test()
        {
            using (var epoll = new Epoll())
            {
                Assert.IsFalse(epoll.SafeHandle.IsClosed);
                Assert.IsFalse(epoll.SafeHandle.IsInvalid);

                using (var isoTpCanSocket = new IsoTpCanSocket())
                {
                    var epollEvent = new EpollEvent()
                    {
                        Events = EpollEventType.EPOLLIN,
                        Data = isoTpCanSocket.Handle,
                    };
                    epoll.Add(isoTpCanSocket.SafeHandle, epollEvent);

                    epoll.Close();

                    epollEvent.Events |= EpollEventType.EPOLLET;
                    Assert.Throws<ObjectDisposedException>(() => epoll.Modify(isoTpCanSocket.SafeHandle, epollEvent));
                }
            }
        }

        [Test]
        public void Epoll_Wait_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var testerSocket = new IsoTpCanSocket())
            using (var ecuSocket = new IsoTpCanSocket())
            {
                testerSocket.Bind(iface, 0x600, 0x700);
                ecuSocket.Bind(iface, 0x700, 0x600);
                using (var epoll = new Epoll())
                {
                    Assert.IsFalse(epoll.SafeHandle.IsClosed);
                    Assert.IsFalse(epoll.SafeHandle.IsInvalid);

                    var epollEvent = new EpollEvent()
                    {
                        Events = EpollEventType.EPOLLIN,
                        Data = testerSocket.Handle,
                    };
                    epoll.Add(testerSocket.SafeHandle, epollEvent);

                    byte[] transmitBuffer = new byte[] { 0x50, 0x03, 0xDE, 0xAD, 0xBE, 0xEF };
                    int bytesWritten = ecuSocket.Write(transmitBuffer);
                    Assert.AreEqual(6, bytesWritten);

                    EpollEvent[] eventArray = epoll.Wait(1, 100);
                    Assert.NotNull(eventArray);
                    Assert.AreEqual(1, eventArray.Length);
                    Assert.AreEqual(EpollEventType.EPOLLIN, eventArray[0].Events);
                    Assert.AreEqual(testerSocket.Handle, eventArray[0].Data);

                    var receiveBuffer = new byte[10];
                    int bytesRead = testerSocket.Read(receiveBuffer);
                    Assert.AreEqual(6, bytesRead);
                    Assert.IsTrue(transmitBuffer.SequenceEqual(receiveBuffer.Take(bytesRead)));
                }
            }
        }

        [Test]
        public void Epoll_Wait_ObjectDisposedException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var testerSocket = new IsoTpCanSocket())
            using (var ecuSocket = new IsoTpCanSocket())
            {
                testerSocket.Bind(iface, 0x600, 0x700);
                ecuSocket.Bind(iface, 0x700, 0x600);
                using (var epoll = new Epoll())
                {
                    Assert.IsFalse(epoll.SafeHandle.IsClosed);
                    Assert.IsFalse(epoll.SafeHandle.IsInvalid);

                    var epollEvent = new EpollEvent()
                    {
                        Events = EpollEventType.EPOLLIN,
                        Data = testerSocket.Handle,
                    };
                    epoll.Add(testerSocket.SafeHandle, epollEvent);

                    byte[] transmitBuffer = new byte[] { 0x50, 0x03, 0xDE, 0xAD, 0xBE, 0xEF };
                    int bytesWritten = ecuSocket.Write(transmitBuffer);
                    Assert.AreEqual(6, bytesWritten);

                    epoll.Close();
                    Assert.Throws<ObjectDisposedException>(() => epoll.Wait(1, 100));
                }
            }
        }

        [Test]
        public void Epoll_Wait_ArgumentOutOfRangeException_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var testerSocket = new IsoTpCanSocket())
            using (var ecuSocket = new IsoTpCanSocket())
            {
                testerSocket.Bind(iface, 0x600, 0x700);
                ecuSocket.Bind(iface, 0x700, 0x600);
                using (var epoll = new Epoll())
                {
                    Assert.IsFalse(epoll.SafeHandle.IsClosed);
                    Assert.IsFalse(epoll.SafeHandle.IsInvalid);

                    var epollEvent = new EpollEvent()
                    {
                        Events = EpollEventType.EPOLLIN,
                        Data = testerSocket.Handle,
                    };
                    epoll.Add(testerSocket.SafeHandle, epollEvent);

                    byte[] transmitBuffer = new byte[] { 0x50, 0x03, 0xDE, 0xAD, 0xBE, 0xEF };
                    int bytesWritten = ecuSocket.Write(transmitBuffer);
                    Assert.AreEqual(6, bytesWritten);

                    Assert.Throws<ArgumentOutOfRangeException>(() => epoll.Wait(0, 100));
                    Assert.Throws<ArgumentOutOfRangeException>(() => epoll.Wait(-1, 100));
                }
            }
        }

        [Test]
        public void Epoll_Wait_Timeout_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            using (var testerSocket = new IsoTpCanSocket())
            {
                testerSocket.Bind(iface, 0x600, 0x700);
                using (var epoll = new Epoll())
                {
                    Assert.IsFalse(epoll.SafeHandle.IsClosed);
                    Assert.IsFalse(epoll.SafeHandle.IsInvalid);

                    var epollEvent = new EpollEvent()
                    {
                        Events = EpollEventType.EPOLLIN,
                        Data = testerSocket.Handle,
                    };
                    epoll.Add(testerSocket.SafeHandle, epollEvent);

                    EpollEvent[] eventArray = epoll.Wait(1, 100);
                    Assert.NotNull(eventArray);
                    Assert.AreEqual(0, eventArray.Length);
                }
            }
        }
    }
}