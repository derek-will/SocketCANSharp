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

using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace SocketCANSharp
{
    /// <summary>
    /// C Standard Library Native Methods
    /// </summary>
    public static class LibcNativeMethods
    {
        /// <summary>
        /// Number of the last error which indicates what went wrong. Set by system calls and some library functions when an error occurs.
        /// </summary>
        public static int Errno { get { return Marshal.GetLastWin32Error(); } }

        /// <summary>
        /// Creates a socket.
        /// </summary>
        /// <param name="addressFamily">Address Family</param>
        /// <param name="socketType">Type of socket</param>
        /// <param name="protocolType">Protocol Type</param>
        /// <returns>Socket Handle Wrapper Instance</returns>
        [DllImport("libc", EntryPoint="socket", SetLastError=true)]
        public static extern SafeSocketHandle Socket(int addressFamily, SocketType socketType, SocketCanProtocolType protocolType);

        /// <summary>
        /// Manipulates the underlying device parameters of special files.
        /// </summary>
        /// <param name="socketHandle">Socket Handle Wrapper Instance</param>
        /// <param name="request">Request Code</param>
        /// <param name="arg">Integer Argument</param>
        /// <returns>0 on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="ioctl", SetLastError=true)]
        public static extern int Ioctl(SafeSocketHandle socketHandle, int request, ref int arg);

        /// <summary>
        /// Manipulates the underlying device parameters of special files.
        /// </summary>
        /// <param name="socketHandle">Socket Handle Wrapper Instance</param>
        /// <param name="request">Request Code</param>
        /// <param name="ifreq">Interface Request structure</param>
        /// <returns>0 on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="ioctl", SetLastError=true)]
        public static extern int Ioctl(SafeSocketHandle socketHandle, int request, [In][Out] Ifreq ifreq);

        /// <summary>
        /// Assigns the specified SocketCAN base address to the socket.
        /// </summary>
        /// <param name="socketHandle">Socket Handle Wrapper Instance</param>
        /// <param name="addr">SocketCAN base address structure</param>
        /// <param name="addrSize">Size of address structure</param>
        /// <returns>0 on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="bind", SetLastError=true)]
        public static extern int Bind(SafeSocketHandle socketHandle, SockAddrCan addr, int addrSize);

        /// <summary>
        /// Assigns the specified SocketCAN ISO-TP address to the socket.
        /// </summary>
        /// <param name="socketHandle">Socket Handle Wrapper Instance</param>
        /// <param name="addr">SocketCAN ISO-TP address structure</param>
        /// <param name="addrSize">Size of address structure</param>
        /// <returns>0 on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="bind", SetLastError=true)]
        public static extern int Bind(SafeSocketHandle socketHandle, SockAddrCanIsoTp addr, int addrSize);

        /// <summary>
        /// Assigns the specified SocketCAN J1939 address to the socket.
        /// </summary>
        /// <param name="socketHandle">Socket Handle Wrapper Instance</param>
        /// <param name="addr">SocketCAN J1939 address structure</param>
        /// <param name="addrSize">Size of address structure</param>
        /// <returns>0 on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="bind", SetLastError=true)]
        public static extern int Bind(SafeSocketHandle socketHandle, SockAddrCanJ1939 addr, int addrSize);

        /// <summary>
        /// Establishes a connection on the socket to the specified SocketCAN base address.
        /// </summary>
        /// <param name="socketHandle">Socket Handle Wrapper Instance</param>
        /// <param name="addr">SocketCAN base address structure containing the peer address</param>
        /// <param name="addrSize">Size of address structure</param>
        /// <returns>0 on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="connect", SetLastError=true)]
        public static extern int Connect(SafeSocketHandle socketHandle, SockAddrCan addr, int addrSize);

        /// <summary>
        /// Establishes a connection on the socket to the specified SocketCAN base address.
        /// </summary>
        /// <param name="socketHandle">Socket Handle Wrapper Instance</param>
        /// <param name="addr">SocketCAN J1939 address structure containing the peer address</param>
        /// <param name="addrSize">Size of address structure</param>
        /// <returns>0 on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="connect", SetLastError=true)]
        public static extern int Connect(SafeSocketHandle socketHandle, SockAddrCanJ1939 addr, int addrSize);
 
        /// <summary>
        /// Write the CanFrame to the socket.
        /// </summary>
        /// <param name="socketHandle">Socket Handle Wrappper Instance</param>
        /// <param name="frame">CAN Frame to write</param>
        /// <param name="frameSize">Size of CAN Frame in bytes</param>
        /// <returns>The number of bytes written on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="write", SetLastError=true)]
        public static extern int Write(SafeSocketHandle socketHandle, ref CanFrame frame, int frameSize);

        /// <summary>
        /// Write the CanFdFrame to the socket.
        /// </summary>
        /// <param name="socketHandle">Socket Handle Wrapper Instance</param>
        /// <param name="frame">CAN FD Frame to write</param>
        /// <param name="frameSize">Size of CAN FD Frame in bytes</param>
        /// <returns>The number of bytes written on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="write", SetLastError=true)]
        public static extern int Write(SafeSocketHandle socketHandle, ref CanFdFrame frame, int frameSize);

        /// <summary>
        /// Write the BcmCanMessage to the socket.
        /// </summary>
        /// <param name="socketHandle">Socket Handle Wrapper Instance</param>
        /// <param name="message">BCM Message to write</param>
        /// <param name="msgSize">Size of BCM Message in bytes</param>
        /// <returns>The number of bytes written on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="write", SetLastError=true)]
        public static extern int Write(SafeSocketHandle socketHandle, BcmCanMessage message, int msgSize);

        /// <summary>
        /// Write the BcmCanFdMessage to the socket.
        /// </summary>
        /// <param name="socketHandle">Socket Handle Wrapper Instance</param>
        /// <param name="message">BCM Message to write</param>
        /// <param name="msgSize">Size of BCM Message in bytes</param>
        /// <returns>The number of bytes written on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="write", SetLastError=true)]
        public static extern int Write(SafeSocketHandle socketHandle, BcmCanFdMessage message, int msgSize);

        /// <summary>
        /// Write the BcmCanSingleMessage to the socket.
        /// </summary>
        /// <param name="socketHandle">Socket Handle Wrapper Instance</param>
        /// <param name="message">Special Single Frame BCM Message to write</param>
        /// <param name="msgSize">Size of BCM Message in bytes</param>
        /// <returns>The number of bytes written on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="write", SetLastError=true)]
        public static extern int Write(SafeSocketHandle socketHandle, BcmCanSingleMessage message, int msgSize);

        /// <summary>
        /// Write the BcmCanFdSingleMessage to the socket.
        /// </summary>
        /// <param name="socketHandle">Socket Handle Wrapper Instance</param>
        /// <param name="message">Special Single Frame BCM Message to write</param>
        /// <param name="msgSize">Size of BCM Message in bytes</param>
        /// <returns>The number of bytes written on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="write", SetLastError=true)]
        public static extern int Write(SafeSocketHandle socketHandle, BcmCanFdSingleMessage message, int msgSize);

        /// <summary>
        /// Write the BcmMessageHeader to the socket.
        /// </summary>
        /// <param name="socketHandle">Socket Handle Wrapper Instance</param>
        /// <param name="header">BCM Message Header to write</param>
        /// <param name="headerSize">Size of BCM Message Header in bytes</param>
        /// <returns>The number of bytes written on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="write", SetLastError=true)]
        public static extern int Write(SafeSocketHandle socketHandle, BcmMessageHeader header, int headerSize);

        /// <summary>
        /// Write the byte array to the socket.
        /// </summary>
        /// <param name="socketHandle">Socket Handle Wrapper Instance</param>
        /// <param name="data">Byte Array to write</param>
        /// <param name="dataSize">Size of Byte Array</param>
        /// <returns>The number of bytes written on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="write", SetLastError=true)]
        public static extern int Write(SafeSocketHandle socketHandle, byte[] data, int dataSize);

        /// <summary>
        /// Read a CanFrame from the socket.
        /// </summary>
        /// <param name="socketHandle">Socket Handle Wrapper Instance</param>
        /// <param name="frame">CAN Frame structure to populate</param>
        /// <param name="frameSize">Size of CAN Frame structure</param>
        /// <returns>The number of bytes read on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="read", SetLastError=true)]
        public static extern int Read(SafeSocketHandle socketHandle, ref CanFrame frame, int frameSize);

        /// <summary>
        /// Read a CanFdFrame from the socket.
        /// </summary>
        /// <param name="socketHandle">Socket Handle Wrapper Instance</param>
        /// <param name="frame">CAN FD Frame structure to populate</param>
        /// <param name="frameSize">Size of CAN FD Frame structure</param>
        /// <returns>The number of bytes read on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="read", SetLastError=true)]
        public static extern int Read(SafeSocketHandle socketHandle, ref CanFdFrame frame, int frameSize);

        /// <summary>
        /// Read a BcmCanMessage from the socket.
        /// </summary>
        /// <param name="socketHandle">Socket Handle Wrapper Instance</param>
        /// <param name="message">BCM CAN Message structure to populate</param>
        /// <param name="msgSize">Size of BCM CAN Message structure</param>
        /// <returns>The number of bytes read on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="read", SetLastError=true)]
        public static extern int Read(SafeSocketHandle socketHandle, [Out] BcmCanMessage message, int msgSize);

        /// <summary>
        /// Read a BcmCanFdMessage from the socket.
        /// </summary>
        /// <param name="socketHandle">Socket Handle Wrapper Instance</param>
        /// <param name="message">BCM CAN Message structure to populate</param>
        /// <param name="msgSize">Size of BCM CAN FD Message structure</param>
        /// <returns>The number of bytes read on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="read", SetLastError=true)]
        public static extern int Read(SafeSocketHandle socketHandle, [Out] BcmCanFdMessage message, int msgSize);

        /// <summary>
        /// Read a byte array from the socket.
        /// </summary>
        /// <param name="socketHandle">Socket Handle Wrapper Instance</param>
        /// <param name="data">Byte array to populate</param>
        /// <param name="dataSize">Size of byte array</param>
        /// <returns>The number of bytes read on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="read", SetLastError=true)]
        public static extern int Read(SafeSocketHandle socketHandle, [Out] byte[] data, int dataSize);

        /// <summary>
        /// Receive a CanFrame from a socket.
        /// </summary>
        /// <param name="socketHandle">Socket Handle Wrapper Instance</param>
        /// <param name="frame">CAN Frame structure</param>
        /// <param name="frameSize">Size of CAN Frame structure</param>
        /// <param name="flags">Message Flags</param>
        /// <param name="addr">The SocketCAN base source address</param>
        /// <param name="addrSize">The size of the SocketCAN base address</param>
        /// <returns>The number of bytes read on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="recvfrom", SetLastError=true)]
        public static extern int RecvFrom(SafeSocketHandle socketHandle, ref CanFrame frame, int frameSize, MessageFlags flags, [Out] SockAddrCan addr, ref int addrSize);

        /// <summary>
        /// Receive a CanFdFrame from a socket.
        /// </summary>
        /// <param name="socketHandle">Socket Handle Wrapper Instance</param>
        /// <param name="frame">CAN FD Frame structure</param>
        /// <param name="frameSize">Size of CAN FD Frame structure</param>
        /// <param name="flags">Message Flags</param>
        /// <param name="addr">The SocketCAN base source address</param>
        /// <param name="addrSize">The size of the SocketCAN base address</param>
        /// <returns>The number of bytes read on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="recvfrom", SetLastError=true)]
        public static extern int RecvFrom(SafeSocketHandle socketHandle, ref CanFdFrame frame, int frameSize, MessageFlags flags, [Out] SockAddrCan addr, ref int addrSize);

        /// <summary>
        /// Receive a byte array from a socket.
        /// </summary>
        /// <param name="socketHandle">Socket Handle Wrapper Instance</param>
        /// <param name="data">Byte array</param>
        /// <param name="dataSize">Size of byte array</param>
        /// <param name="flags">Message Flags</param>
        /// <param name="addr">The SocketCAN J1939 source address</param>
        /// <param name="addrSize">The size of the SocketCAN J1939 address</param>
        /// <returns>The number of bytes read on success, -1 on error</returns>
        [DllImport("libc", EntryPoint = "recvfrom", SetLastError = true)]
        public static extern int RecvFrom(SafeSocketHandle socketHandle, [Out] byte[] data, int dataSize, MessageFlags flags, [Out] SockAddrCanJ1939 addr, ref int addrSize);

        /// <summary>
        /// Receive a byte array on a connected socket.
        /// </summary>
        /// <param name="socketHandle">Socket Handle Wrapper Instance</param>
        /// <param name="data">Byte array</param>
        /// <param name="dataSize">Size of byte array</param>
        /// <param name="flags">Message Flags</param>
        /// <returns>The number of bytes read on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="recv", SetLastError=true)]
        public static extern int Recv(SafeSocketHandle socketHandle, [Out] byte[] data, int dataSize, MessageFlags flags);

        /// <summary>
        /// Transmit a CanFrame to another socket.
        /// </summary>
        /// <param name="socketHandle">Socket Handle Wrapper Instance</param>
        /// <param name="frame">CAN Frame structure</param>
        /// <param name="frameSize">Size of CAN Frame structure</param>
        /// <param name="flags">Message Flags</param>
        /// <param name="addr">The SocketCAN base destination address</param>
        /// <param name="addrSize">The size of the SocketCAN base address</param>
        /// <returns>The number of bytes sent on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="sendto", SetLastError=true)]
        public static extern int SendTo(SafeSocketHandle socketHandle, ref CanFrame frame, int frameSize, MessageFlags flags, SockAddrCan addr, int addrSize);

        /// <summary>
        /// Transmit a CanFdFrame to another socket.
        /// </summary>
        /// <param name="socketHandle">Socket Handle Wrapper Instance</param>
        /// <param name="frame">CAN FD Frame structure</param>
        /// <param name="frameSize">Size of CAN FD Frame structure</param>
        /// <param name="flags">Message Flags</param>
        /// <param name="addr">The SocketCAN base destination address</param>
        /// <param name="addrSize">The size of the SocketCAN base address</param>
        /// <returns>The number of bytes sent on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="sendto", SetLastError=true)]
        public static extern int SendTo(SafeSocketHandle socketHandle, ref CanFdFrame frame, int frameSize, MessageFlags flags, SockAddrCan addr, int addrSize);

        /// <summary>
        /// Transmit a byte array to another socket.
        /// </summary>
        /// <param name="socketHandle">Socket Handle Wrapper Instance</param>
        /// <param name="data">Byte array</param>
        /// <param name="dataSize">Size of byte array</param>
        /// <param name="flags">Message Flags</param>
        /// <param name="addr">The SocketCAN J1939 destination address</param>
        /// <param name="addrSize">The size of the SocketCAN J1939 address</param>
        /// <returns>The number of bytes sent on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="sendto", SetLastError=true)]
        public static extern int SendTo(SafeSocketHandle socketHandle, byte[] data, int dataSize, MessageFlags flags, SockAddrCanJ1939 addr, int addrSize);

        /// <summary>
        /// Transmit a byte array to another socket.
        /// </summary>
        /// <param name="socketHandle">Socket Handle Wrapper Instance</param>
        /// <param name="data">Byte array</param>
        /// <param name="dataSize">Size of byte array</param>
        /// <param name="flags">Message Flags</param>
        /// <returns>The number of bytes sent on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="send", SetLastError=true)]
        public static extern int Send(SafeSocketHandle socketHandle, byte[] data, int dataSize, MessageFlags flags);

        /// <summary>
        /// Set the socket option specified by the option name and socket level to the provided option value for the supplied socket.
        /// </summary>
        /// <param name="socketHandle">CAN_RAW socket handle</param>
        /// <param name="socketLevel">SOL_CAN_RAW</param>
        /// <param name="optionName">SOL_CAN_RAW socket option</param>
        /// <param name="optionValue">Unsigned integer value</param>
        /// <param name="optionValueSize">Size of unsigned interger</param>
        /// <returns>0 on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="setsockopt", SetLastError=true)]
        public static extern int SetSockOpt(SafeSocketHandle socketHandle, SocketLevel socketLevel, CanSocketOptions optionName, ref uint optionValue, int optionValueSize);

        /// <summary>
        /// Get the socket option specified by the option name and socket level to the provided option value for the supplied socket.
        /// </summary>
        /// <param name="socketHandle">CAN_RAW socket handle</param>
        /// <param name="socketLevel">SOL_CAN_RAW</param>
        /// <param name="optionName">SOL_CAN_RAW socket option</param>
        /// <param name="optionValue">Unsigned integer value</param>
        /// <param name="optionValueSize">Size of an unsigned integer</param>
        /// <returns>0 on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="getsockopt", SetLastError=true)]
        public static extern int GetSockOpt(SafeSocketHandle socketHandle, SocketLevel socketLevel, CanSocketOptions optionName, ref uint optionValue, ref int optionValueSize);

        /// <summary>
        /// Set the socket option specified by the option name and socket level to the provided option value for the supplied socket.
        /// </summary>
        /// <param name="socketHandle">CAN_RAW socket handle</param>
        /// <param name="socketLevel">SOL_CAN_RAW</param>
        /// <param name="optionName">SOL_CAN_RAW socket option</param>
        /// <param name="filters">Array of CAN Filters</param>
        /// <param name="optionValueSize">Size of CAN Filter Array in bytes</param>
        /// <returns>0 on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="setsockopt", SetLastError=true)]
        public static extern int SetSockOpt(SafeSocketHandle socketHandle, SocketLevel socketLevel, CanSocketOptions optionName, CanFilter[] filters, int optionValueSize);

        /// <summary>
        /// Get the socket option specified by the option name and socket level to the provided option value for the supplied socket.
        /// </summary>
        /// <param name="socketHandle">CAN_RAW socket handle</param>
        /// <param name="socketLevel">SOL_CAN_RAW</param>
        /// <param name="optionName">SOL_CAN_RAW socket option</param>
        /// <param name="filters">Array of CAN Filters</param>
        /// <param name="optionValueSize">Size of CAN Filter Array in bytes</param>
        /// <returns>0 on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="getsockopt", SetLastError=true)]
        public static extern int GetSockOpt(SafeSocketHandle socketHandle, SocketLevel socketLevel, CanSocketOptions optionName, [In, Out] CanFilter[] filters, ref int optionValueSize);

        /// <summary>
        /// Set the socket option specified by the option name and socket level to the provided option value for the supplied socket.
        /// </summary>
        /// <param name="socketHandle">CAN_RAW socket handle</param>
        /// <param name="socketLevel">SOL_CAN_RAW</param>
        /// <param name="optionName">SOL_CAN_RAW socket option</param>
        /// <param name="optionValue">Signed Integer Value</param>
        /// <param name="optionValueSize">Size of signed integer</param>
        /// <returns>0 on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="setsockopt", SetLastError=true)]
        public static extern int SetSockOpt(SafeSocketHandle socketHandle, SocketLevel socketLevel, CanSocketOptions optionName, ref int optionValue, int optionValueSize);

        /// <summary>
        /// Get the socket option specified by the option name and socket level to the provided option value for the supplied socket.
        /// </summary>
        /// <param name="socketHandle">CAN_RAW socket handle</param>
        /// <param name="socketLevel">SOL_CAN_RAW</param>
        /// <param name="optionName">SOL_CAN_RAW socket option</param>
        /// <param name="optionValue">Signed Integer Value</param>
        /// <param name="optionValueSize">Size of signed integer</param>
        /// <returns>0 on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="getsockopt", SetLastError=true)]
        public static extern int GetSockOpt(SafeSocketHandle socketHandle, SocketLevel socketLevel, CanSocketOptions optionName, ref int optionValue, ref int optionValueSize);

        /// <summary>
        /// Set the socket option specified by the option name and socket level to the provided option value for the supplied socket.
        /// </summary>
        /// <param name="socketHandle">Socket handle</param>
        /// <param name="socketLevel">SOL_SOCKET</param>
        /// <param name="optionName">SOL_SOCKET socket option</param>
        /// <param name="optionValue">Signed Integer Value</param>
        /// <param name="optionValueSize">Size of signed integer</param>
        /// <returns>0 on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="setsockopt", SetLastError=true)]
        public static extern int SetSockOpt(SafeSocketHandle socketHandle, SocketLevel socketLevel, SocketLevelOptions optionName, ref int optionValue, int optionValueSize);

        /// <summary>
        /// Get the socket option specified by the option name and socket level to the provided option value for the supplied socket.
        /// </summary>
        /// <param name="socketHandle">Socket handle</param>
        /// <param name="socketLevel">SOL_SOCKET</param>
        /// <param name="optionName">SOL_SOCKET socket option</param>
        /// <param name="optionValue">Signed Integer Value</param>
        /// <param name="optionValueSize">Size of signed integer</param>
        /// <returns>0 on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="getsockopt", SetLastError=true)]
        public static extern int GetSockOpt(SafeSocketHandle socketHandle, SocketLevel socketLevel, SocketLevelOptions optionName, ref int optionValue, ref int optionValueSize);

        /// <summary>
        /// Set the socket option specified by the option name and socket level to the provided option value for the supplied socket.
        /// </summary>
        /// <param name="socketHandle">Socket handle</param>
        /// <param name="socketLevel">SOL_SOCKET</param>
        /// <param name="optionName">SOL_SOCKET socket option</param>
        /// <param name="timeval">Time interval object</param>
        /// <param name="optionValueSize">Size of Time interval object in bytes</param>
        /// <returns>0 on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="setsockopt", SetLastError=true)]
        public static extern int SetSockOpt(SafeSocketHandle socketHandle, SocketLevel socketLevel, SocketLevelOptions optionName, Timeval timeval, int optionValueSize);

        /// <summary>
        /// Get the socket option specified by the option name and socket level to the provided option value for the supplied socket.
        /// </summary>
        /// <param name="socketHandle">Socket handle</param>
        /// <param name="socketLevel">SOL_SOCKET</param>
        /// <param name="optionName">SOL_SOCKET socket option</param>
        /// <param name="timeval">Time interval object</param>
        /// <param name="optionValueSize">Size of Time interval object in bytes</param>
        /// <returns>0 on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="getsockopt", SetLastError=true)]
        public static extern int GetSockOpt(SafeSocketHandle socketHandle, SocketLevel socketLevel, SocketLevelOptions optionName, [In, Out] Timeval timeval, ref int optionValueSize);

        /// <summary>
        /// Set the socket option specified by the option name and socket level to the provided option value for the supplied socket.
        /// </summary>
        /// <param name="socketHandle">CAN_ISOTP Socket handle</param>
        /// <param name="socketLevel">SOL_CAN_ISOTP</param>
        /// <param name="optionName">SOL_CAN_ISOTP socket option</param>
        /// <param name="optionValue">Unsigned Integer Value</param>
        /// <param name="optionValueSize">Size of unsigned integer</param>
        /// <returns>0 on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="setsockopt", SetLastError=true)]
        public static extern int SetSockOpt(SafeSocketHandle socketHandle, SocketLevel socketLevel, CanIsoTpSocketOptions optionName, ref uint optionValue, int optionValueSize);

        /// <summary>
        /// Get the socket option specified by the option name and socket level to the provided option value for the supplied socket.
        /// </summary>
        /// <param name="socketHandle">CAN_ISOTP Socket handle</param>
        /// <param name="socketLevel">SOL_CAN_ISOTP</param>
        /// <param name="optionName">SOL_CAN_ISOTP socket option</param>
        /// <param name="optionValue">Unsigned Integer Value</param>
        /// <param name="optionValueSize">Size of unsigned integer</param>
        /// <returns>0 on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="getsockopt", SetLastError=true)]
        public static extern int GetSockOpt(SafeSocketHandle socketHandle, SocketLevel socketLevel, CanIsoTpSocketOptions optionName, ref uint optionValue, ref int optionValueSize);

        /// <summary>
        /// Set the socket option specified by the option name and socket level to the provided option value for the supplied socket.
        /// </summary>
        /// <param name="socketHandle">CAN_ISOTP Socket handle</param>
        /// <param name="socketLevel">SOL_CAN_ISOTP</param>
        /// <param name="optionName">SOL_CAN_ISOTP socket option</param>
        /// <param name="fcOptions">ISO-TP Flow Control Options object</param>
        /// <param name="optionValueSize">Size of ISO-TP Flow Control Options object in bytes</param>
        /// <returns>0 on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="setsockopt", SetLastError=true)]
        public static extern int SetSockOpt(SafeSocketHandle socketHandle, SocketLevel socketLevel, CanIsoTpSocketOptions optionName, CanIsoTpFlowControlOptions fcOptions, int optionValueSize);

        /// <summary>
        /// Get the socket option specified by the option name and socket level to the provided option value for the supplied socket.
        /// </summary>
        /// <param name="socketHandle">CAN_ISOTP Socket handle</param>
        /// <param name="socketLevel">SOL_CAN_ISOTP</param>
        /// <param name="optionName">SOL_CAN_ISOTP socket option</param>
        /// <param name="fcOptions">ISO-TP Flow Control Options object</param>
        /// <param name="optionValueSize">Size of ISO-TP Flow Control Options object in bytes</param>
        /// <returns>0 on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="getsockopt", SetLastError=true)]
        public static extern int GetSockOpt(SafeSocketHandle socketHandle, SocketLevel socketLevel, CanIsoTpSocketOptions optionName, CanIsoTpFlowControlOptions fcOptions, ref int optionValueSize);

        /// <summary>
        /// Set the socket option specified by the option name and socket level to the provided option value for the supplied socket.
        /// </summary>
        /// <param name="socketHandle">CAN_ISOTP Socket handle</param>
        /// <param name="socketLevel">SOL_CAN_ISOTP</param>
        /// <param name="optionName">SOL_CAN_ISOTP socket option</param>
        /// <param name="llOptions">ISO-TP Link Layer Options object</param>
        /// <param name="optionValueSize">Size of ISO-TP Link Layer Options object in bytes</param>
        /// <returns>0 on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="setsockopt", SetLastError=true)]
        public static extern int SetSockOpt(SafeSocketHandle socketHandle, SocketLevel socketLevel, CanIsoTpSocketOptions optionName, CanIsoTpLinkLayerOptions llOptions, int optionValueSize);

        /// <summary>
        /// Get the socket option specified by the option name and socket level to the provided option value for the supplied socket.
        /// </summary>
        /// <param name="socketHandle">CAN_ISOTP Socket handle</param>
        /// <param name="socketLevel">SOL_CAN_ISOTP</param>
        /// <param name="optionName">SOL_CAN_ISOTP socket option</param>
        /// <param name="llOptions">ISO-TP Link Layer Options object</param>
        /// <param name="optionValueSize">Size of ISO-TP Link Layer Options object in bytes</param>
        /// <returns>0 on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="getsockopt", SetLastError=true)]
        public static extern int GetSockOpt(SafeSocketHandle socketHandle, SocketLevel socketLevel, CanIsoTpSocketOptions optionName, CanIsoTpLinkLayerOptions llOptions, ref int optionValueSize);

        /// <summary>
        /// Set the socket option specified by the option name and socket level to the provided option value for the supplied socket.
        /// </summary>
        /// <param name="socketHandle">CAN_ISOTP Socket handle</param>
        /// <param name="socketLevel">SOL_CAN_ISOTP</param>
        /// <param name="optionName">SOL_CAN_ISOTP socket option</param>
        /// <param name="tpOptions">ISO-TP Options object</param>
        /// <param name="optionValueSize">Size of ISO-TP Options object in bytes</param>
        /// <returns>0 on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="setsockopt", SetLastError=true)]
        public static extern int SetSockOpt(SafeSocketHandle socketHandle, SocketLevel socketLevel, CanIsoTpSocketOptions optionName, CanIsoTpOptions tpOptions, int optionValueSize);

        /// <summary>
        /// Get the socket option specified by the option name and socket level to the provided option value for the supplied socket.
        /// </summary>
        /// <param name="socketHandle">CAN_ISOTP Socket handle</param>
        /// <param name="socketLevel">SOL_CAN_ISOTP</param>
        /// <param name="optionName">SOL_CAN_ISOTP socket option</param>
        /// <param name="tpOptions">ISO-TP Options object</param>
        /// <param name="optionValueSize">Size of ISO-TP Options object in bytes</param>
        /// <returns>0 on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="getsockopt", SetLastError=true)]
        public static extern int GetSockOpt(SafeSocketHandle socketHandle, SocketLevel socketLevel, CanIsoTpSocketOptions optionName, CanIsoTpOptions tpOptions, ref int optionValueSize);

        /// <summary>
        /// Set the socket option specified by the option name and socket level to the provided option value for the supplied socket.
        /// </summary>
        /// <param name="socketHandle">CAN_J1939 Socket handle</param>
        /// <param name="socketLevel">SOL_CAN_J1939</param>
        /// <param name="optionName">SOL_CAN_J1939 socket option</param>
        /// <param name="optionValue">Signed Integer Value</param>
        /// <param name="optionValueSize">Size of signed integer</param>
        /// <returns>0 or 1 on success depending on option name and value, -1 on error</returns>
        [DllImport("libc", EntryPoint="setsockopt", SetLastError=true)]
        public static extern int SetSockOpt(SafeSocketHandle socketHandle, SocketLevel socketLevel, J1939SocketOptions optionName, ref int optionValue, int optionValueSize);
        
        /// <summary>
        /// Get the socket option specified by the option name and socket level to the provided option value for the supplied socket.
        /// </summary>
        /// <param name="socketHandle">CAN_J1939 Socket handle</param>
        /// <param name="socketLevel">SOL_CAN_J1939</param>
        /// <param name="optionName">SOL_CAN_J1939 socket option</param>
        /// <param name="optionValue">Signed Integer Value</param>
        /// <param name="optionValueSize">Size of signed integer</param>
        /// <returns>0 or 1 on success depending on option name and value, -1 on error</returns>
        [DllImport("libc", EntryPoint="getsockopt", SetLastError=true)]
        public static extern int GetSockOpt(SafeSocketHandle socketHandle, SocketLevel socketLevel, J1939SocketOptions optionName, ref int optionValue, ref int optionValueSize);

        /// <summary>
        /// Set the socket option specified by the option name and socket level to the provided option value for the supplied socket.
        /// </summary>
        /// <param name="socketHandle">CAN_J1939 Socket handle</param>
        /// <param name="socketLevel">SOL_CAN_J1939</param>
        /// <param name="optionName">SOL_CAN_J1939 socket option</param>
        /// <param name="filters">Array of J1939 Filters</param>
        /// <param name="optionValueSize">Size of array of J1939 Filters in bytes</param>
        /// <returns>0 on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="setsockopt", SetLastError=true)]
        public static extern int SetSockOpt(SafeSocketHandle socketHandle, SocketLevel socketLevel, J1939SocketOptions optionName, J1939Filter[] filters, int optionValueSize);

        /// <summary>
        /// Get the socket option specified by the option name and socket level to the provided option value for the supplied socket.
        /// </summary>
        /// <param name="socketHandle">CAN_J1939 Socket handle</param>
        /// <param name="socketLevel">SOL_CAN_J1939</param>
        /// <param name="optionName">SOL_CAN_J1939 socket option</param>
        /// <param name="filters">Array of J1939 Filters</param>
        /// <param name="optionValueSize">Size of array of J1939 Filters in bytes</param>
        /// <returns>0 on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="getsockopt", SetLastError=true)]
        public static extern int GetSockOpt(SafeSocketHandle socketHandle, SocketLevel socketLevel, J1939SocketOptions optionName, [In, Out] J1939Filter[] filters, ref int optionValueSize);

        /// <summary>
        /// Returns a pointer to an array of IfNameIndex objects. Each IfNameIndex object includes information about one of the network interfaces on the local system.
        /// </summary>
        /// <returns>Pointer to an array of IfNameIndex objects</returns>
        [DllImport("libc", EntryPoint="if_nameindex", SetLastError=true)]
        public static extern IntPtr IfNameIndex();

        /// <summary>
        /// Frees the dynamically allocated data structure returned by IfNameIndex().
        /// </summary>
        /// <param name="ptr">Pointer to an array of IfNameIndex objects</param>
        [DllImport("libc", EntryPoint="if_freenameindex", SetLastError=true)]
        public static extern void IfFreeNameIndex(IntPtr ptr);

        /// <summary>
        /// Opens an epoll file descriptor.
        /// </summary>
        /// <param name="size">The size argument is ignored since Linux 2.6.8, but must be greater than zero for backwards compatibility. 
        /// Originally, this argument was intended as a hint to the kernel as to the number of file descriptors that the caller expected to add to the epoll instance.</param>
        /// <returns>On success, returns a valid file descriptor handle. On failure, returns an invalid file descriptor handle.</returns>
        [DllImport("libc", EntryPoint="epoll_create", SetLastError=true)]
        public static extern SafeFileDescriptorHandle EpollCreate(int size);
        
        /// <summary>
        /// Control interface used to add, modify, and delete entries from the interest list of an epoll file descriptor.
        /// </summary>
        /// <param name="epfd">Epoll File Descriptor.</param>
        /// <param name="op">Operation to be performed.</param>
        /// <param name="fd">Target file descriptor.</param>
        /// <param name="evnt">Event object linked to the targeted file descriptor.</param>
        /// <returns>0 on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="epoll_ctl", SetLastError=true)]
        public static extern int EpollControl(SafeFileDescriptorHandle epfd, EpollOperation op, SafeSocketHandle fd, ref EpollEvent evnt);
        
        /// <summary>
        /// Waits for an I/O event on an epoll file descriptor.
        /// </summary>
        /// <param name="epfd">Epoll File Descriptor.</param>
        /// <param name="events">A buffer that contains information from the ready lists about file descriptors in the interest list that have some event(s) available.</param>
        /// <param name="maxEvents">Maximum number of events to wait for.</param>
        /// <param name="timeout">The maximum number of milliseconds that the function call will block for. Set to 0 to return immediately, set to -1 to wait indefinitely.</param>
        /// <returns>Returns the number of file descriptors ready for the requested I/O. Returns -1 on failure.</returns>
        [DllImport("libc", EntryPoint="epoll_wait", SetLastError=true)]
        public static extern int EpollWait(SafeFileDescriptorHandle epfd, [Out] EpollEvent[] events, int maxEvents, int timeout);

        /// <summary>
        /// Closes a file descriptor.
        /// </summary>
        /// <param name="fd">File descriptor to close.</param>
        /// <returns>0 on success, -1 on error</returns>
        [DllImport("libc", EntryPoint="close", SetLastError=true)]
        public static extern int Close(IntPtr fd);
    }
}