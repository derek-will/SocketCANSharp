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
    /// Provides SAE J1939 over CAN socket services.
    /// </summary>
    public class J1939CanSocket : AbstractCanSocket
    {
        /// <summary>
        /// The current address to which this socket is bound.
        /// </summary>
        public SockAddrCanJ1939 LocalAddress
        {
            get
            {
                return GetSockAddr();
            }
        }

        /// <summary>
        /// The current address to which this socket is connected.
        /// </summary>
        public SockAddrCanJ1939 RemoteAddress
        {
            get
            {
                return GetPeerAddr();
            }
        }

        /// <summary>
        /// J1939 Filters to control the reception of J1939 messages.
        /// </summary>
        public J1939Filter[] J1939Filters 
        {
            // Note: Set only as getsockopt for SO_J1939_FILTER is currently not supported by the linux kernel
            set
            {
                SetJ1939Filters(value);
            }
        }

        /// <summary>
        /// True if the J1939 socket is in promiscuous mode; otherwise, false.
        /// </summary>
        public bool EnablePromiscuousMode 
        {
            get
            {
                return GetJ1939LevelOption(J1939SocketOptions.SO_J1939_PROMISC) > 0;
            }
            set
            {
                SetJ1939LevelOption(J1939SocketOptions.SO_J1939_PROMISC, value ? 1 : 0);
            }
        }

        /// <summary>
        /// Default send priority.
        /// </summary>
        public int SendPriority 
        {
            get
            {
                return GetJ1939LevelOption(J1939SocketOptions.SO_J1939_SEND_PRIO);
            }
            set
            {
                SetJ1939LevelOption(J1939SocketOptions.SO_J1939_SEND_PRIO, value);
            }
        }

        /// <summary>
        /// Indicates whether queued errors should be received from the socket error queue.
        /// </summary>
        public bool EnableErrorQueue 
        {
            get
            {
                return GetJ1939LevelOption(J1939SocketOptions.SO_J1939_ERRQUEUE) > 0;
            }
            set
            {
                SetJ1939LevelOption(J1939SocketOptions.SO_J1939_ERRQUEUE, value ? 1 : 0);
            }
        }

        /// <summary>
        /// Initializes a new instance of the J1939CanSocket class.
        /// </summary>
        /// <exception cref="SocketCanException">Unable to create the requested socket</exception>
        public J1939CanSocket()
        {
            SocketType = SocketType.Dgram;
            ProtocolType = SocketCanProtocolType.CAN_J1939;
            SafeHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType, ProtocolType);

            if (SafeHandle.IsInvalid)
                throw new SocketCanException("Failed to create CAN_J1939 socket.");
        }

        /// <summary>
        /// Assigns the SAE J1939 Address Structure to the CAN_J1939 socket.
        /// </summary>
        /// <param name="addr">J1939 Socket Address structure</param>
        /// <exception cref="ObjectDisposedException">The CAN_J1939 socket has been closed.</exception>
        /// <exception cref="ArgumentNullException">SAE J1939 Address Structure is null</exception>
        /// <exception cref="SocketCanException">Unable to assign the provided SAE J1939 Address Structure to the CAN_J1939 socket.</exception>
        public void Bind(SockAddrCanJ1939 addr)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (addr == null)
                throw new ArgumentNullException(nameof(addr));

            int result = LibcNativeMethods.Bind(SafeHandle, addr, Marshal.SizeOf(typeof(SockAddrCanJ1939)));
            if (result != 0)
                throw new SocketCanException("Unable to assign the provided SocketCAN SAE J1939 address to the underlying CAN_J1939 Socket.");

            IsBound = true;
        }

        /// <summary>
        /// Assigns the CAN Network Interface Index, 64-bit NAME, Parameter Group Number, and Address byte to the CAN_J1939 socket.
        /// </summary>
        /// <param name="iface">CAN Network Interface instance containing the name and index of the interface.</param>
        /// <param name="name">64-bit NAME value assigned to the J1939 node.</param>
        /// <param name="pgn">Parameter Group Number to use. The PGN is part of the 29-bit CAN ID used by the J1939 node.</param>
        /// <param name="address">Address byte associated to the J1939 node.</param>
        /// <exception cref="ArgumentNullException">CanNetworkInterface instance is null.</exception>
        public void Bind(CanNetworkInterface iface, ulong name, uint pgn, byte address)
        {
            if (iface == null) 
                throw new ArgumentNullException(nameof(iface));

            Bind(new SockAddrCanJ1939()
            {
                CanFamily = SocketCanConstants.AF_CAN,
                CanIfIndex = iface.Index,
                Name = name,
                PGN = pgn,
                Address = address,
            });
        }

        /// <summary>
        /// Connects the underlying CAN_J1939 socket to the specified SAE J1939 Address Structure.
        /// </summary>
        /// <param name="addr">The J1939 Socket Address structure to connect to.</param>
        /// <exception cref="ObjectDisposedException">The CAN_J1939 socket has been closed.</exception>
        /// <exception cref="ArgumentNullException">SAE J1939 Address Structure is null</exception>
        /// <exception cref="SocketCanException">Unable to connect the CAN_J1939 socket to the provided SAE J1939 Address Structure.</exception>
        public void Connect(SockAddrCanJ1939 addr)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (addr == null)
                throw new ArgumentNullException(nameof(addr));

            int result = LibcNativeMethods.Connect(SafeHandle, addr, Marshal.SizeOf(typeof(SockAddrCanJ1939)));
            if (result != 0)
                throw new SocketCanException("Unable to connect the CAN_J1939 socket to the provided SAE J1939 Address Structure.");

            Connected = true;
        }

        /// <summary>
        /// Connects the CAN Network Interface Index, 64-bit NAME, Parameter Group Number, and Address byte to the CAN_J1939 socket.
        /// </summary>
        /// <param name="iface">CAN Network Interface instance containing the name and index of the interface.</param>
        /// <param name="name">64-bit NAME value of the J1939 node that the socket is connected to.</param>
        /// <param name="pgn">Parameter Group Number of the J1939 node that the socket is connected to. The PGN is part of the 29-bit CAN ID used by the J1939 node.</param>
        /// <param name="address">Address byte associated with the J1939 node that the socket is connected to.</param>
        /// <exception cref="ArgumentNullException">CanNetworkInterface instance is null.</exception>
        public void Connect(CanNetworkInterface iface, ulong name, uint pgn, byte address)
        {
            if (iface == null) 
                throw new ArgumentNullException(nameof(iface));

            Connect(new SockAddrCanJ1939()
            {
                CanFamily = SocketCanConstants.AF_CAN,
                CanIfIndex = iface.Index,
                Name = name,
                PGN = pgn,
                Address = address,
            });
        }

        /// <summary>
        /// Writes the supplied data to the socket.
        /// </summary>
        /// <param name="data">An array of bytes that contains the data to be sent.</param>
        /// <returns>The number of bytes written to the socket.</returns>
        /// <exception cref="ObjectDisposedException">The socket has been closed.</exception>
        /// <exception cref="ArgumentNullException">The data byte array is null.</exception>
        /// <exception cref="SocketCanException">Writing to the underlying CAN_J1939 socket failed</exception>
        public int Write(byte[] data)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            int bytesWritten = LibcNativeMethods.Write(SafeHandle, data, data.Length);
            if (bytesWritten == -1)
            {
                throw new SocketCanException("Writing to the underlying CAN_J1939 socket failed.");
            }

            return bytesWritten;
        }

        /// <summary>
        /// Writes the supplied data to the socket utilizing the specified flags.
        /// </summary>
        /// <param name="data">An array of bytes that contains the data to be sent.</param>
        /// <param name="messageFlags">Specifies send behavior</param>
        /// <returns>The number of bytes written to the socket.</returns>
        /// <exception cref="ObjectDisposedException">The socket has been closed.</exception>
        /// <exception cref="ArgumentNullException">The data byte array is null.</exception>
        /// <exception cref="SocketCanException">Writing to the underlying CAN_J1939 socket failed</exception>
        public int Write(byte[] data, MessageFlags messageFlags)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            int bytesWritten = LibcNativeMethods.Send(SafeHandle, data, data.Length, messageFlags);
            if (bytesWritten == -1)
            {
                throw new SocketCanException("Writing to the underlying CAN_J1939 socket failed.");
            }

            return bytesWritten;
        }

        /// <summary>
        /// Writes the supplied data on the socket to the target provided in the destination address structure utlizing the specified flags.
        /// </summary>
        /// <param name="data">An array of bytes that contains the data to be sent.</param>
        /// <param name="messageFlags">Specifies send behavior</param>
        /// <param name="destAddr">Target of the data transmission</param>
        /// <returns>The number of bytes written to the socket.</returns>
        /// <exception cref="ObjectDisposedException">The socket has been closed.</exception>
        /// <exception cref="ArgumentNullException">The data byte array is null.</exception>
        /// <exception cref="SocketCanException">Writing to the underlying CAN_J1939 socket failed</exception>
        public int WriteTo(byte[] data, MessageFlags messageFlags, SockAddrCanJ1939 destAddr)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (data == null)
                throw new ArgumentNullException(nameof(data));
            
            if (destAddr == null)
                throw new ArgumentNullException(nameof(destAddr));

            int bytesWritten = LibcNativeMethods.SendTo(SafeHandle, data, data.Length, messageFlags, destAddr, Marshal.SizeOf(typeof(SockAddrCanJ1939)));
            if (bytesWritten == -1)
            {
                throw new SocketCanException("Writing to the underlying CAN_J1939 socket failed.");
            }

            return bytesWritten;
        }

        /// <summary>
        /// Reads data from the socket into the supplied receive buffer.
        /// </summary>
        /// <param name="data">An array of bytes that is the receive buffer</param>
        /// <returns>The number of bytes received from the socket.</returns>
        /// <exception cref="ObjectDisposedException">The socket has been closed.</exception>
        /// <exception cref="ArgumentNullException">The data byte array is null.</exception>
        /// <exception cref="SocketCanException">Reading from the underlying CAN_J1939 socket failed.</exception>
        public int Read(byte[] data)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            int bytesRead = LibcNativeMethods.Read(SafeHandle, data, data.Length);
            if (bytesRead == -1)
            {
                throw new SocketCanException("Reading from the underlying CAN_J1939 socket failed.");
            }

            return bytesRead;
        }

        /// <summary>
        /// Reads data from the socket into the supplied receive buffer utilizing the specified flags.
        /// </summary>
        /// <param name="data">An array of bytes that is the receive buffer</param>
        /// <param name="messageFlags">Specifies receive behavior</param>
        /// <returns>The number of bytes received from the socket.</returns>
        /// <exception cref="ObjectDisposedException">The socket has been closed.</exception>
        /// <exception cref="ArgumentNullException">The data byte array is null.</exception>
        /// <exception cref="SocketCanException">Reading from the underlying CAN_J1939 socket failed.</exception>
        public int Read(byte[] data, MessageFlags messageFlags)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            int bytesRead = LibcNativeMethods.Recv(SafeHandle, data, data.Length, messageFlags);
            if (bytesRead == -1)
            {
                throw new SocketCanException("Reading from the underlying CAN_J1939 socket failed.");
            }

            return bytesRead;
        }

        /// <summary>
        /// Reads data from the socket into the supplied receive buffer utilizing the specified flags and additionally identifies the source of the received data.
        /// </summary>
        /// <param name="data">An array of bytes that is the receive buffer</param>
        /// <param name="messageFlags">Specifies receive behavior</param>
        /// <param name="srcAddr">Source of the data transmission</param>
        /// <returns>The number of bytes received from the socket.</returns>
        /// <exception cref="ObjectDisposedException">The socket has been closed.</exception>
        /// <exception cref="ArgumentNullException">The data byte array is null.</exception>
        /// <exception cref="SocketCanException">Reading from the underlying CAN_J1939 socket failed.</exception>
        public int ReadFrom(byte[] data, MessageFlags messageFlags, SockAddrCanJ1939 srcAddr)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (srcAddr == null)
                throw new ArgumentNullException(nameof(srcAddr));

            int addrSize = Marshal.SizeOf(typeof(SockAddrCanJ1939));
            int bytesRead = LibcNativeMethods.RecvFrom(SafeHandle, data, data.Length, messageFlags, srcAddr, ref addrSize);
            if (bytesRead == -1)
            {
                throw new SocketCanException("Reading from the underlying CAN_J1939 socket failed.");
            }

            return bytesRead;
        }

        private SockAddrCanJ1939 GetSockAddr()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            var addr = new SockAddrCanJ1939();
            int size = Marshal.SizeOf(typeof(SockAddrCanJ1939));
            int result = LibcNativeMethods.GetSockName(SafeHandle, addr, ref size);

            if (result != 0)
                throw new SocketCanException("Unable to get name on CAN_J1939 socket.");

            return addr;
        }

        private SockAddrCanJ1939 GetPeerAddr()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            var addr = new SockAddrCanJ1939();
            int size = Marshal.SizeOf(typeof(SockAddrCanJ1939));
            int result = LibcNativeMethods.GetPeerName(SafeHandle, addr, ref size);

            if (result != 0)
                throw new SocketCanException("Unable to get name on CAN_J1939 peer.");

            return addr;
        }

        private int GetJ1939LevelOption(J1939SocketOptions option)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (option == J1939SocketOptions.SO_J1939_PROMISC || option == J1939SocketOptions.SO_J1939_SEND_PRIO || option == J1939SocketOptions.SO_J1939_ERRQUEUE)
            {
                int value = 0;
                int len = Marshal.SizeOf(typeof(int));
                int result = LibcNativeMethods.GetSockOpt(SafeHandle, SocketLevel.SOL_CAN_J1939, option, ref value, ref len);

                if (result == -1)
                {
                    throw new SocketCanException();
                }

                return value;
            }

            throw new ArgumentOutOfRangeException();
        }

        private void SetJ1939LevelOption(J1939SocketOptions option, int value)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (option == J1939SocketOptions.SO_J1939_PROMISC || option == J1939SocketOptions.SO_J1939_SEND_PRIO || option == J1939SocketOptions.SO_J1939_ERRQUEUE)
            {
                int len = Marshal.SizeOf(typeof(int));
                int result = LibcNativeMethods.SetSockOpt(SafeHandle, SocketLevel.SOL_CAN_J1939, option, ref value, len);
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

        private void SetJ1939Filters(J1939Filter[] j1939FilterArray)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            int len = j1939FilterArray != null ? Marshal.SizeOf(typeof(J1939Filter)) * j1939FilterArray.Length : 0;
            int result = LibcNativeMethods.SetSockOpt(SafeHandle, SocketLevel.SOL_CAN_J1939, J1939SocketOptions.SO_J1939_FILTER, j1939FilterArray, len);
            if (result != 0)
                throw new SocketCanException("Unable to set SO_J1939_FILTER on CAN_J1939 socket.");    
        }
    }
}