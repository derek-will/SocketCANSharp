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
using System.Runtime.InteropServices;
using System.Net.Sockets;

namespace SocketCANSharp.Network
{
    /// <summary>
    /// Provides the base implementation for CAN sockets.
    /// </summary>
    public abstract class AbstractCanSocket : IDisposable
    {
        private bool _blocking = true;

        /// <summary>
        /// Object disposed flag.
        /// </summary>
        protected bool _disposed;

        /// <summary>
        /// Represents the operating system handle exposed in a safe manner for the socket that the current AbstractCanSocket object encapsulates.
        /// </summary>
        public SafeSocketHandle SafeHandle { get; protected set; }

        /// <summary>
        /// Represents the operating system handle for the socket that is encapsulated by this object.
        /// </summary>
        public IntPtr Handle { get { return SafeHandle.DangerousGetHandle(); } }

        /// <summary>
        /// The type of socket.
        /// </summary>
        public SocketType SocketType { get; protected set; }

        /// <summary>
        /// The protocol type of the socket.
        /// </summary>
        public SocketCanProtocolType ProtocolType { get; protected set; }

        /// <summary>
        /// Returns true if the socket is bound to a specific SocketCAN ISO-TP address; otherwise, false.
        /// </summary>
        public bool IsBound { get; protected set; }

        /// <summary>
        /// Returns true if the socket is connected to a SocketCAN ISO-TP address as of the most recent operation; otherwise, false.
        /// </summary>
        public bool Connected { get; protected set; }

        /// <summary>
        /// An integer that contains the size, in bytes, of the send buffer.
        /// </summary>
        public int SendBufferSize
        {
            get
            {
                return GetSocketLevelOption(SocketLevelOptions.SO_SNDBUF);
            }
            set
            {
                SetSocketLevelOption(SocketLevelOptions.SO_SNDBUF, value);
            }
        }

        /// <summary>
        /// An integer that contains the size, in bytes, of the receive buffer.
        /// </summary>
        public int ReceiveBufferSize
        {
            get
            {
                return GetSocketLevelOption(SocketLevelOptions.SO_RCVBUF);
            }
            set
            {
                SetSocketLevelOption(SocketLevelOptions.SO_RCVBUF, value);
            }
        }

        /// <summary>
        /// The amount of time in milliseconds after which a synchronous Send call will time out. A value of 0 represents waiting indefinitely.
        /// </summary>
        public int SendTimeout 
        { 
            get
            {
                return GetSocketLevelOption(SocketLevelOptions.SO_SNDTIMEO);
            }
            set
            {
                SetSocketLevelOption(SocketLevelOptions.SO_SNDTIMEO, value);
            }
        }

        /// <summary>
        /// The amount of time in milliseconds after which a synchronous Receive call will time out. A value of 0 represents waiting indefinitely.
        /// </summary>
        public int ReceiveTimeout 
        { 
            get
            {
                return GetSocketLevelOption(SocketLevelOptions.SO_RCVTIMEO);
            }
            set
            {
                SetSocketLevelOption(SocketLevelOptions.SO_RCVTIMEO, value);
            } 
        }

        /// <summary>
        /// True if the socket allows the sending or receiving of broadcast packets; otherwise, false.
        /// </summary>
        public bool EnableBroadcast
        {
            get
            {
                return GetSocketLevelOption(SocketLevelOptions.SO_BROADCAST) > 0;
            }
            set
            {
                SetSocketLevelOption(SocketLevelOptions.SO_BROADCAST, value ? 1 : 0);
            }
        }

        /// <summary>
        /// True if the Socket is in blocking mode; otherwise, false.
        /// </summary>
        public bool Blocking
        {
            get
            {
                return _blocking;
            }
            set
            {
                SetBlocking(value);
            }
        }

        /// <summary>
        /// Closes the socket and releases all of the associated resources.
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        /// <summary>
        /// Releases all of the resources associated with the current instance of the AbstractCanSocket class.
        /// </summary>
        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the current instance of the AbstractCanSocket class, and optionally disposes of the managed resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to releases only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                SafeHandle.Dispose();
            }

            _disposed = true;
        }

        private void SetBlocking(bool val)
        {
            int arg = val ? 0 : 1;
            int ioctlResult = LibcNativeMethods.Ioctl(SafeHandle, SocketCanConstants.FIONBIO, ref arg);

            if (ioctlResult == -1)
            {
                throw new SocketCanException();
            }

            _blocking = arg == 0;
        }
    
        private int GetSocketLevelOption(SocketLevelOptions option)
        {
            if (option == SocketLevelOptions.SO_SNDTIMEO || option == SocketLevelOptions.SO_RCVTIMEO)
            {
                var timeval = new Timeval(0, 0);
                int len = Marshal.SizeOf(typeof(Timeval));
                int result = LibcNativeMethods.GetSockOpt(SafeHandle, SocketLevel.SOL_SOCKET, option, timeval, ref len);
                if (result == -1)
                {
                    throw new SocketCanException();
                }

                return (int)timeval.Seconds * 1000 + (int)timeval.Microseconds / 1000;
            }
            else if (option == SocketLevelOptions.SO_RCVBUF || option == SocketLevelOptions.SO_SNDBUF || option == SocketLevelOptions.SO_BROADCAST)
            {
                int value = 0;
                int len = Marshal.SizeOf(typeof(int));
                int result = LibcNativeMethods.GetSockOpt(SafeHandle, SocketLevel.SOL_SOCKET, option, ref value, ref len);
                if (result == -1)
                {
                    throw new SocketCanException();
                }

                return value;
            }

            throw new ArgumentOutOfRangeException();
        }

        private void SetSocketLevelOption(SocketLevelOptions option, int value)
        {
            if (option == SocketLevelOptions.SO_SNDTIMEO || option == SocketLevelOptions.SO_RCVTIMEO)
            {
                long seconds = value / 1000;
                long microseconds = (value - (seconds * 1000)) * 1000;

                var timeval = new Timeval(seconds, microseconds);
                int len = Marshal.SizeOf(typeof(Timeval));

                int result = LibcNativeMethods.SetSockOpt(SafeHandle, SocketLevel.SOL_SOCKET, option, timeval, len);
                if (result == -1)
                {
                    throw new SocketCanException();
                }
            }
            else if (option == SocketLevelOptions.SO_RCVBUF || option == SocketLevelOptions.SO_SNDBUF || option == SocketLevelOptions.SO_BROADCAST)
            {
                int len = Marshal.SizeOf(typeof(int));
                int result = LibcNativeMethods.SetSockOpt(SafeHandle, SocketLevel.SOL_SOCKET, option, ref value, len);
                if (result == -1)
                {
                    throw new SocketCanException();
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }
    }
}