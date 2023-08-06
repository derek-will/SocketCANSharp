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
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace SocketCANSharp.Network
{
    /// <summary>
    /// Provides raw CAN socket services.
    /// </summary>
    public class RawCanSocket : AbstractCanSocket
    {
        /// <summary>
        /// The current address to which this socket is bound.
        /// </summary>
        public SockAddrCan Address
        {
            get
            {
                return GetSockAddr();
            }
        }

        /// <summary>
        /// CAN Filters to control the reception of CAN frames.
        /// </summary>
        public CanFilter[] CanFilters 
        {
            get
            {
                return GetRawCanFilters();
            }
            set
            {
                SetRawCanFilters(value);
            }
        }

        /// <summary>
        /// Error Mask to filter which Error Message Frames are passed to the socket receive queue.
        /// </summary>
        public CanErrorClass ErrorFilters 
        {
            get
            {
                return GetRawCanErrorFrameFilters();
            }
            set
            {
                SetRawCanErrorFrameFilters(value);
            }
        }

        /// <summary>
        /// Local loopback to receive messages sent from other sockets on this CAN node.
        /// </summary>
        public bool LocalLoopback 
        {
            get
            {
                return GetLoopback();
            }
            set
            {
                SetLoopback(value);
            }
        }

        /// <summary>
        /// Enables a socket to receive the messages that it sent itself.
        /// </summary>
        public bool ReceiveOwnMessages 
        {
            get
            {
                return GetReceiveOwnMessages();
            }
            set
            {
                SetReceiveOwnMessages(value);
            }
        }

        /// <summary>
        /// Allows the socket to handle CAN FD frames.
        /// </summary>
        public bool EnableCanFdFrames 
        {
            get
            {
                return GetEnableCanFdFrames();
            }
            set
            {
                SetEnableCanFdFrames(value);
            }
        }

        /// <summary>
        /// If true, then all CAN filters must match (logical AND) for a CAN frame to be placed into the receive queue. If false, then if any CAN filter matches (logical OR) then a CAN frame will be placed into the receive queue. 
        /// </summary>
        public bool AllCanFiltersMustMatch 
        {
            get
            {
                return GetAllCanFiltersMustMatch();
            }
            set
            {
                SetAllCanFiltersMustMatch(value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the RawCanSocket class.
        /// </summary>
        /// <exception cref="SocketCanException">Unable to create the requested socket.</exception>
        public RawCanSocket()
        {
            SocketType = SocketType.Raw;
            ProtocolType = SocketCanProtocolType.CAN_RAW;
            SafeHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType, ProtocolType);

            if (SafeHandle.IsInvalid)
                throw new SocketCanException("Failed to create CAN_RAW socket.");
        }
        
        /// <summary>
        /// Assigns the SocketCAN Base Address Structure to the CAN_RAW socket.
        /// </summary>
        /// <param name="addr">SocketCAN Base Address Structure.</param>
        /// <exception cref="ObjectDisposedException">The CAN_RAW socket has been closed.</exception>
        /// <exception cref="ArgumentNullException">SocketCAN Base Address Structure is null.</exception>
        /// <exception cref="SocketCanException">Unable to assign the provided SocketCAN Base Address Structure to the CAN_RAW socket.</exception>
        public void Bind(SockAddrCan addr)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (addr == null)
                throw new ArgumentNullException(nameof(addr));

            int result = LibcNativeMethods.Bind(SafeHandle, addr, Marshal.SizeOf(typeof(SockAddrCan)));
            if (result != 0)
                throw new SocketCanException("Unable to assign the provided SocketCAN raw address to the underlying CAN_RAW Socket.");

            IsBound = true;
        }

        /// <summary>
        /// Assigns the CAN Network Interface Index to the CAN_RAW socket.
        /// </summary>
        /// <param name="iface">CAN Network Interface instance containing the name and index of the interface.</param>
        /// <exception cref="ArgumentNullException">CanNetworkInterface instance is null.</exception>
        public void Bind(CanNetworkInterface iface)
        {
            if (iface == null) 
                throw new ArgumentNullException(nameof(iface));

            Bind(new SockAddrCan()
            {
                CanFamily = SocketCanConstants.AF_CAN,
                CanIfIndex = iface.Index,
            });
        }

        /// <summary>
        /// Writes the supplied Classical CAN Frame to the socket.
        /// </summary>
        /// <param name="canFrame">Classical CAN Frame to transmit onto the CAN network.</param>
        /// <returns>Number of bytes written to the socket.</returns>
        /// <exception cref="ObjectDisposedException">The socket has been closed.</exception>
        /// <exception cref="SocketCanException">Writing to the underlying CAN_RAW socket failed.</exception>
        public int Write(CanFrame canFrame)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            int bytesWritten = LibcNativeMethods.Write(SafeHandle, ref canFrame, Marshal.SizeOf(typeof(CanFrame)));
            if (bytesWritten == -1)
                throw new SocketCanException("Writing to the underlying CAN_RAW socket failed.");

            return bytesWritten;
        }

        /// <summary>
        /// Writes the supplied CAN FD Frame to the socket.
        /// </summary>
        /// <param name="canFdFrame">CAN FD Frame to transmit onto the CAN network.</param>
        /// <returns>Number of bytes written to the socket.</returns>
        /// <exception cref="ObjectDisposedException">The socket has been closed.</exception>
        /// <exception cref="SocketCanException">Writing to the underlying CAN_RAW socket failed.</exception>
        public int Write(CanFdFrame canFdFrame)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            int bytesWritten = LibcNativeMethods.Write(SafeHandle, ref canFdFrame, Marshal.SizeOf(typeof(CanFdFrame)));
            if (bytesWritten == -1)
                throw new SocketCanException("Writing to the underlying CAN_RAW socket failed.");

            return bytesWritten;
        }

        /// <summary>
        /// Reads a Classical CAN Frame from the socket.
        /// </summary>
        /// <param name="canFrame">Classical CAN Frame to receive from the CAN network.</param>
        /// <returns>Number of bytes read from the socket.</returns>
        /// <exception cref="ObjectDisposedException">The socket has been closed.</exception>
        /// <exception cref="SocketCanException">Reading from the underlying CAN_RAW socket failed.</exception>
        public int Read(out CanFrame canFrame)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            canFrame = new CanFrame();
            int bytesRead = LibcNativeMethods.Read(SafeHandle, ref canFrame, Marshal.SizeOf(typeof(CanFrame)));
            if (bytesRead == -1)
                throw new SocketCanException("Reading from the underlying CAN_RAW socket failed.");

            return bytesRead;
        }

        /// <summary>
        /// Reads a Classical CAN Frame from the socket.
        /// </summary>
        /// <param name="canFrame">Classical CAN Frame to receive from the CAN network.</param>
        /// <param name="txSuccess">Indicates whether the previous transmission attempt was successful or not.</param>
        /// <param name="localhost">Indicates whether the received CAN frame was generated on the localhost or not.</param>
        /// <returns>Number of bytes read from the socket.</returns>
        /// <exception cref="ObjectDisposedException">The socket has been closed.</exception>
        /// <exception cref="SocketCanException">Reading from the underlying CAN_RAW socket failed.</exception>
        public int Read(out CanFrame canFrame, out bool txSuccess, out bool localhost)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return Read<CanFrame>(out canFrame, out txSuccess, out localhost);
        }

        /// <summary>
        /// Reads a CAN FD Frame from the socket.
        /// </summary>
        /// <param name="canFdFrame">CAN FD Frame to receive from the CAN network.</param>
        /// <param name="txSuccess">Indicates whether the previous transmission attempt was successful or not.</param>
        /// <param name="localhost">Indicates whether the received CAN FD frame was generated on the localhost or not.</param>
        /// <returns>Number of bytes read from the socket.</returns>
        /// <exception cref="ObjectDisposedException">The socket has been closed.</exception>
        /// <exception cref="SocketCanException">Reading from the underlying CAN_RAW socket failed.</exception>
        public int Read(out CanFdFrame canFdFrame, out bool txSuccess, out bool localhost)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            return Read<CanFdFrame>(out canFdFrame, out txSuccess, out localhost);
        }

        /// <summary>
        /// Reads a CAN FD Frame from the socket. ** Note: Check the number of bytes read to determine if frame is Classic CAN (16) of CAN FD (72). **
        /// </summary>
        /// <param name="canFdFrame">CAN FD Frame to receive from the CAN network.</param>
        /// <returns>Number of bytes read from the socket.</returns>
        /// <exception cref="ObjectDisposedException">The socket has been closed.</exception>
        /// <exception cref="SocketCanException">Reading from the underlying CAN_RAW socket failed.</exception>
        public int Read(out CanFdFrame canFdFrame)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            canFdFrame = new CanFdFrame();
            int bytesRead = LibcNativeMethods.Read(SafeHandle, ref canFdFrame, Marshal.SizeOf(typeof(CanFdFrame)));
            if (bytesRead == -1)
                throw new SocketCanException("Reading from the underlying CAN_RAW socket failed.");

            return bytesRead;
        }

        private void SetRawCanFilters(CanFilter[] canFilterArray)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            int len = canFilterArray != null ? Marshal.SizeOf(typeof(CanFilter)) * canFilterArray.Length : 0;
            int result = LibcNativeMethods.SetSockOpt(SafeHandle, SocketLevel.SOL_CAN_RAW, CanSocketOptions.CAN_RAW_FILTER, canFilterArray, len);
            if (result != 0)
                throw new SocketCanException("Unable to set CAN_RAW_FILTER on CAN_RAW socket.");    
        }

        private CanFilter[] GetRawCanFilters()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            int size = Marshal.SizeOf(typeof(CanFilter));
            int len = size * (int)SocketCanConstants.CAN_RAW_FILTER_MAX;
            IntPtr ptr = Marshal.AllocHGlobal(len);
            try
            {
                int result = LibcNativeMethods.GetSockOpt(SafeHandle, SocketLevel.SOL_CAN_RAW, (int)CanSocketOptions.CAN_RAW_FILTER, ptr, ref len);
                if (result != 0)
                    throw new SocketCanException("Unable to get CAN_RAW_FILTER on CAN_RAW socket.");

                var canFilterArray = new CanFilter[len / size];
                IntPtr iteratorPtr = ptr;
                for (int i = 0; i < canFilterArray.Length; ++i)
                {
                    canFilterArray[i] = Marshal.PtrToStructure<CanFilter>(iteratorPtr);
                    iteratorPtr = IntPtr.Add(iteratorPtr, size);
                }

                return canFilterArray;
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        private void SetRawCanErrorFrameFilters(CanErrorClass errorMask)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            uint err_mask = (uint)errorMask;
            int result = LibcNativeMethods.SetSockOpt(SafeHandle, SocketLevel.SOL_CAN_RAW, CanSocketOptions.CAN_RAW_ERR_FILTER, ref err_mask, Marshal.SizeOf(err_mask));
            if (result != 0)
                throw new SocketCanException("Unable to set CAN_RAW_ERR_FILTER on CAN_RAW socket."); 
        }

        private CanErrorClass GetRawCanErrorFrameFilters()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);
            
            uint err_mask = 0;
            int len = Marshal.SizeOf(err_mask);
            int result = LibcNativeMethods.GetSockOpt(SafeHandle, SocketLevel.SOL_CAN_RAW, CanSocketOptions.CAN_RAW_ERR_FILTER, ref err_mask, ref len);

            if (result != 0)
                throw new SocketCanException("Unable to get CAN_RAW_ERR_FILTER on CAN_RAW socket.");

            return (CanErrorClass)err_mask;
        }

        private void SetLoopback(bool enable)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            int loopback = enable ? 1 : 0;
            int result = LibcNativeMethods.SetSockOpt(SafeHandle, SocketLevel.SOL_CAN_RAW, CanSocketOptions.CAN_RAW_LOOPBACK, ref loopback, Marshal.SizeOf(loopback));
            
            if (result != 0)
                throw new SocketCanException("Unable to set CAN_RAW_LOOPBACK on CAN_RAW socket.");
        }

        private bool GetLoopback()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            int loopback = 0;
            int len = Marshal.SizeOf(loopback);
            int result = LibcNativeMethods.GetSockOpt(SafeHandle, SocketLevel.SOL_CAN_RAW, CanSocketOptions.CAN_RAW_LOOPBACK, ref loopback, ref len);

            if (result != 0)
                throw new SocketCanException("Unable to get CAN_RAW_LOOPBACK on CAN_RAW socket.");

            return loopback > 0;
        }

        private void SetReceiveOwnMessages(bool enable)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            int recv_own_msgs = enable ? 1 : 0;
            int result = LibcNativeMethods.SetSockOpt(SafeHandle, SocketLevel.SOL_CAN_RAW, CanSocketOptions.CAN_RAW_RECV_OWN_MSGS, ref recv_own_msgs, Marshal.SizeOf(recv_own_msgs));
            
            if (result != 0)
                throw new SocketCanException("Unable to set CAN_RAW_RECV_OWN_MSGS on CAN_RAW socket.");
        }

        private bool GetReceiveOwnMessages()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            int recv_own_msgs = 0;
            int len = Marshal.SizeOf(recv_own_msgs);
            int result = LibcNativeMethods.GetSockOpt(SafeHandle, SocketLevel.SOL_CAN_RAW, CanSocketOptions.CAN_RAW_RECV_OWN_MSGS, ref recv_own_msgs, ref len);

            if (result != 0)
                throw new SocketCanException("Unable to get CAN_RAW_RECV_OWN_MSGS on CAN_RAW socket.");

            return recv_own_msgs > 0;
        }

        private void SetEnableCanFdFrames(bool enable)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            int can_fd_enabled = enable ? 1 : 0;
            int result = LibcNativeMethods.SetSockOpt(SafeHandle, SocketLevel.SOL_CAN_RAW, CanSocketOptions.CAN_RAW_FD_FRAMES, ref can_fd_enabled, Marshal.SizeOf(can_fd_enabled));
            
            if (result != 0)
                throw new SocketCanException("Unable to set CAN_RAW_FD_FRAMES on CAN_RAW socket.");
        }

        private bool GetEnableCanFdFrames()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            int can_fd_enabled = 0;
            int len = Marshal.SizeOf(can_fd_enabled);
            int result = LibcNativeMethods.GetSockOpt(SafeHandle, SocketLevel.SOL_CAN_RAW, CanSocketOptions.CAN_RAW_FD_FRAMES, ref can_fd_enabled, ref len);

            if (result != 0)
                throw new SocketCanException("Unable to get CAN_RAW_FD_FRAMES on CAN_RAW socket.");

            return can_fd_enabled > 0;
        }

        private void SetAllCanFiltersMustMatch(bool enable)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            int join_filter = enable ? 1 : 0;
            int result = LibcNativeMethods.SetSockOpt(SafeHandle, SocketLevel.SOL_CAN_RAW, CanSocketOptions.CAN_RAW_JOIN_FILTERS, ref join_filter, Marshal.SizeOf(join_filter));
            
            if (result != 0)
                throw new SocketCanException("Unable to set CAN_RAW_JOIN_FILTERS on CAN_RAW socket.");
        }

        private bool GetAllCanFiltersMustMatch()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            int join_filter = 0;
            int len = Marshal.SizeOf(join_filter);
            int result = LibcNativeMethods.GetSockOpt(SafeHandle, SocketLevel.SOL_CAN_RAW, CanSocketOptions.CAN_RAW_JOIN_FILTERS, ref join_filter, ref len);

            if (result != 0)
                throw new SocketCanException("Unable to get CAN_RAW_JOIN_FILTERS on CAN_RAW socket.");

            return join_filter > 0;
        }

        private SockAddrCan GetSockAddr()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            var addr = new SockAddrCan();
            int size = Marshal.SizeOf(typeof(SockAddrCan));
            int result = LibcNativeMethods.GetSockName(SafeHandle, addr, ref size);

            if (result != 0)
                throw new SocketCanException("Unable to get name on CAN_RAW socket.");

            return addr;
        }

        private int Read<T>(out T frame, out bool txSuccess, out bool localhost)
        {
            int ctrlMsgSize = ControlMessageMacros.CMSG_SPACE(Marshal.SizeOf<Timeval>()) + ControlMessageMacros.CMSG_SPACE(Marshal.SizeOf<UInt32>());     
            IntPtr addrPtr = Marshal.AllocHGlobal(Marshal.SizeOf<SockAddrCan>());
            IntPtr iovecPtr = Marshal.AllocHGlobal(Marshal.SizeOf<IoVector>());
            IntPtr framePtr = Marshal.AllocHGlobal(Marshal.SizeOf<T>());
            IntPtr ctrlMsgPtr = Marshal.AllocHGlobal(ctrlMsgSize);
            
            try
            {
                var iovec = new IoVector() { Base = framePtr, Length = new IntPtr(Marshal.SizeOf<T>()) };
                Marshal.StructureToPtr<SockAddrCan>(Address, addrPtr, false);
                Marshal.StructureToPtr<IoVector>(iovec, iovecPtr, false);
                var msgHdr = new MessageHeader()
                {
                    Name = addrPtr,
                    NameLength = Marshal.SizeOf<SockAddrCan>(),
                    IoVectors = iovecPtr,
                    IoVectorCount = new IntPtr(1),
                    ControlMessage = ctrlMsgPtr,
                    ControlMessageLength = new IntPtr(ctrlMsgSize),
                };

                int bytesRead = LibcNativeMethods.RecvMsg(SafeHandle, ref msgHdr, MessageFlags.None);
                if (bytesRead == -1)
                {
                    frame = default(T);
                    throw new SocketCanException("Reading from the underlying CAN_RAW socket failed.");
                }

                IoVector iov = Marshal.PtrToStructure<IoVector>(msgHdr.IoVectors);
                frame = Marshal.PtrToStructure<T>(iov.Base);
                txSuccess = msgHdr.Flags.HasFlag(MessageFlags.MSG_CONFIRM);
                localhost = msgHdr.Flags.HasFlag(MessageFlags.MSG_DONTROUTE);
                return bytesRead;
            }
            finally
            {
                Marshal.FreeHGlobal(ctrlMsgPtr);
                Marshal.FreeHGlobal(iovecPtr);
                Marshal.FreeHGlobal(addrPtr);
                Marshal.FreeHGlobal(framePtr);
            }
        }
    }
}