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
using NUnit.Framework;
using SocketCANSharp;
using System.Net.Sockets;
using System.Runtime.InteropServices;

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
    }
}