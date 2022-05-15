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
    /// Provides ISO-TP (ISO 15765-2) over CAN socket services.
    /// </summary>
    public class IsoTpCanSocket : AbstractCanSocket
    {
        /// <summary>
        /// The current address to which this socket is bound.
        /// </summary>
        public SockAddrCanIsoTp Address
        {
            get
            {
                return GetSockAddr();
            }
        }

        /// <summary>
        /// Base options for ISO-TP including Extended Addresses, Pad Bytes, Transmit Timing, and various configuration flags.
        /// </summary>
        public CanIsoTpOptions BaseOptions 
        {
            get
            {
                return GetBaseOptions();
            }
            set
            {
                SetBaseOptions(value);
            }
        }

        /// <summary>
        /// Flow Control settings for the ISO-TP conversation (BlockSize, STmin, and Maximum WAIT FC Frame tolerance).
        /// </summary>
        public CanIsoTpFlowControlOptions FlowControlOptions
        {
            get
            {
                return GetFlowControlOptions();
            }
            set
            {
                SetFlowControlOptions(value);
            }
        }

        /// <summary>
        /// Time in nano secs to use for STmin instead of the value provided in the FC frame from the receiver.
        /// </summary>
        public uint TransmitStmin
        {
            get
            {
                return GetStmin(CanIsoTpSocketOptions.CAN_ISOTP_TX_STMIN);
            }
            set
            {
                SetStmin(CanIsoTpSocketOptions.CAN_ISOTP_TX_STMIN, value);
            }
        }

        /// <summary>
        /// Time in nano secs to use for STmin and ignore received CF frames which timestamps differ less than this value.
        /// </summary>
        public uint ReceiveStmin
        {
            get
            {
                return GetStmin(CanIsoTpSocketOptions.CAN_ISOTP_RX_STMIN);
            }
            set
            {
                SetStmin(CanIsoTpSocketOptions.CAN_ISOTP_RX_STMIN, value);
            }
        }

        /// <summary>
        /// Link Layer settings for the ISO-TP conversation (MTU, Frame Size, and flags specific to CAN FD).
        /// </summary>
        public CanIsoTpLinkLayerOptions LinkLayerOptions
        {
            get
            {
                return GetLinkLayerOptions();
            }
            set
            {
                SetLinkLayerOptions(value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the IsoTpCanSocket class.
        /// </summary>
        /// <exception cref="SocketCanException">Unable to create the requested socket</exception>
        public IsoTpCanSocket()
        {
            SocketType = SocketType.Dgram;
            ProtocolType = SocketCanProtocolType.CAN_ISOTP;
            SafeHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType, ProtocolType);

            if (SafeHandle.IsInvalid)
                throw new SocketCanException("Failed to create CAN_ISOTP socket.");
        }

        /// <summary>
        /// Assigns the ISO-TP Address Structure to the CAN_ISOTP socket.
        /// </summary>
        /// <param name="addr"></param>
        /// <exception cref="ObjectDisposedException">The CAN_ISOTP socket has been closed.</exception>
        /// <exception cref="ArgumentNullException">ISO-TP Address Structure is null</exception>
        /// <exception cref="SocketCanException">Unable to assign the provided ISO-TP Address Structure to the CAN_ISOTP socket.</exception>
        public void Bind(SockAddrCanIsoTp addr)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (addr == null)
                throw new ArgumentNullException(nameof(addr));

            int result = LibcNativeMethods.Bind(SafeHandle, addr, Marshal.SizeOf(typeof(SockAddrCanIsoTp)));
            if (result != 0)
                throw new SocketCanException("Unable to assign the provided SocketCAN ISO-TP address to the underlying CAN_ISOTP Socket.");

            IsBound = true;
        }

        /// <summary>
        /// Assigns the CAN Network Interface Index, Transmit CAN ID, and Receive CAN ID to the CAN_ISOTP socket.
        /// </summary>
        /// <param name="iface">CAN Network Interface instance containing the name and index of the interface.</param>
        /// <param name="txId">Transmit CAN ID. Usually a tester will send requests to an ECU using this CAN ID.</param>
        /// <param name="rxId">Receive CAN ID. Usually an ECU will send responses to a tester request using this CAN ID.</param>
        /// <exception cref="ArgumentNullException">CanNetworkInterface instance is null.</exception>
        public void Bind(CanNetworkInterface iface, uint txId, uint rxId)
        {
            if (iface == null) 
                throw new ArgumentNullException(nameof(iface));

            Bind(new SockAddrCanIsoTp()
            {
                CanFamily = SocketCanConstants.AF_CAN,
                CanIfIndex = iface.Index,
                TxId = txId,
                RxId = rxId,
            });
        }

        /// <summary>
        /// Writes the supplied data to the socket.
        /// </summary>
        /// <param name="data">An array of bytes that contains the data to be sent.</param>
        /// <returns>The number of bytes written to the socket.</returns>
        /// <exception cref="ObjectDisposedException">The socket has been closed.</exception>
        /// <exception cref="ArgumentNullException">The data byte array is null.</exception>
        /// <exception cref="SocketCanException">Writing to the underlying CAN_ISOTP socket failed</exception>
        public int Write(byte[] data)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            int bytesWritten = LibcNativeMethods.Write(SafeHandle, data, data.Length);
            if (bytesWritten == -1)
            {
                throw new SocketCanException("Writing to the underlying CAN_ISOTP socket failed.");
            }

            return bytesWritten;
        }

        /// <summary>
        /// Reads data from the socket into the supplied receive buffer.
        /// </summary>
        /// <param name="data">An array of bytes that is the receive buffer</param>
        /// <returns>The number of received from the socket.</returns>
        /// <exception cref="ObjectDisposedException">The socket has been closed.</exception>
        /// <exception cref="ArgumentNullException">The data byte array is null.</exception>
        /// <exception cref="SocketCanException">Reading from the underlying CAN_ISOTP socket failed.</exception>
        public int Read(byte[] data)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            int bytesRead = LibcNativeMethods.Read(SafeHandle, data, data.Length);
            if (bytesRead == -1)
            {
                throw new SocketCanException("Reading from the underlying CAN_ISOTP socket failed.");
            }

            return bytesRead;
        }

        private void SetBaseOptions(CanIsoTpOptions isoTpOptions)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (isoTpOptions == null)
                throw new ArgumentNullException(nameof(isoTpOptions));

            int result = LibcNativeMethods.SetSockOpt(SafeHandle, SocketLevel.SOL_CAN_ISOTP, CanIsoTpSocketOptions.CAN_ISOTP_OPTS, isoTpOptions, Marshal.SizeOf(typeof(CanIsoTpOptions)));
            if (result != 0)
                throw new SocketCanException("Unable to set CAN_ISOTP_OPTS on CAN_ISOTP socket.");
        }

        private CanIsoTpOptions GetBaseOptions()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            var isoTpOptions = new CanIsoTpOptions();
            int len = Marshal.SizeOf(typeof(CanIsoTpOptions));
            int result = LibcNativeMethods.GetSockOpt(SafeHandle, SocketLevel.SOL_CAN_ISOTP, CanIsoTpSocketOptions.CAN_ISOTP_OPTS, isoTpOptions, ref len);
            if (result != 0)
                throw new SocketCanException("Unable to get CAN_ISOTP_OPTS on CAN_ISOTP socket.");

            return isoTpOptions;
        }

        private void SetFlowControlOptions(CanIsoTpFlowControlOptions isoTpFlowControlOptions)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (isoTpFlowControlOptions == null)
                throw new ArgumentNullException(nameof(isoTpFlowControlOptions));

            int result = LibcNativeMethods.SetSockOpt(SafeHandle, SocketLevel.SOL_CAN_ISOTP, CanIsoTpSocketOptions.CAN_ISOTP_RECV_FC, isoTpFlowControlOptions, Marshal.SizeOf(typeof(CanIsoTpFlowControlOptions)));
            if (result != 0)
                throw new SocketCanException("Unable to set CAN_ISOTP_RECV_FC on CAN_ISOTP socket.");
        }

        private CanIsoTpFlowControlOptions GetFlowControlOptions()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            var isoTpFlowControlOptions = new CanIsoTpFlowControlOptions();
            int len = Marshal.SizeOf(typeof(CanIsoTpFlowControlOptions));
            int result = LibcNativeMethods.GetSockOpt(SafeHandle, SocketLevel.SOL_CAN_ISOTP, CanIsoTpSocketOptions.CAN_ISOTP_RECV_FC, isoTpFlowControlOptions, ref len);
            if (result != 0)
                throw new SocketCanException("Unable to get CAN_ISOTP_RECV_FC on CAN_ISOTP socket.");

            return isoTpFlowControlOptions;
        }

        private void SetStmin(CanIsoTpSocketOptions option, uint stmin)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (option != CanIsoTpSocketOptions.CAN_ISOTP_TX_STMIN && option != CanIsoTpSocketOptions.CAN_ISOTP_RX_STMIN)
                throw new ArgumentOutOfRangeException(nameof(option), "Only CAN_ISOTP_TX_STMIN and CAN_ISOTP_RX_STMIN options are supported.");

            int result = LibcNativeMethods.SetSockOpt(SafeHandle, SocketLevel.SOL_CAN_ISOTP, option, ref stmin, Marshal.SizeOf(typeof(uint)));
            if (result != 0)
                throw new SocketCanException($"Unable to set {option} on CAN_ISOTP socket.");
        }

        private uint GetStmin(CanIsoTpSocketOptions option)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (option != CanIsoTpSocketOptions.CAN_ISOTP_TX_STMIN && option != CanIsoTpSocketOptions.CAN_ISOTP_RX_STMIN)
                throw new ArgumentOutOfRangeException(nameof(option), "Only CAN_ISOTP_TX_STMIN and CAN_ISOTP_RX_STMIN options are supported.");

            uint stmin = 0;
            int len = Marshal.SizeOf(typeof(uint));
            int result = LibcNativeMethods.GetSockOpt(SafeHandle, SocketLevel.SOL_CAN_ISOTP, option, ref stmin, ref len);
            if (result != 0)
                throw new SocketCanException($"Unable to get {option} on CAN_ISOTP socket.");

            return stmin;
        }

        private void SetLinkLayerOptions(CanIsoTpLinkLayerOptions isoTpLinkLayerOptions)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (isoTpLinkLayerOptions == null)
                throw new ArgumentNullException(nameof(isoTpLinkLayerOptions));

            int result = LibcNativeMethods.SetSockOpt(SafeHandle, SocketLevel.SOL_CAN_ISOTP, CanIsoTpSocketOptions.CAN_ISOTP_LL_OPTS, isoTpLinkLayerOptions, Marshal.SizeOf(typeof(CanIsoTpLinkLayerOptions)));
            if (result != 0)
                throw new SocketCanException("Unable to set CAN_ISOTP_LL_OPTS on CAN_ISOTP socket.");
        }

        private CanIsoTpLinkLayerOptions GetLinkLayerOptions()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            var isoTpLinkLayerOptions = new CanIsoTpLinkLayerOptions();
            int len = Marshal.SizeOf(typeof(CanIsoTpLinkLayerOptions));
            int result = LibcNativeMethods.GetSockOpt(SafeHandle, SocketLevel.SOL_CAN_ISOTP, CanIsoTpSocketOptions.CAN_ISOTP_LL_OPTS, isoTpLinkLayerOptions, ref len);
            if (result != 0)
                throw new SocketCanException("Unable to get CAN_ISOTP_LL_OPTS on CAN_ISOTP socket.");

            return isoTpLinkLayerOptions;
        }

        private SockAddrCanIsoTp GetSockAddr()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            var addr = new SockAddrCanIsoTp();
            int size = Marshal.SizeOf(typeof(SockAddrCanIsoTp));
            int result = LibcNativeMethods.GetSockName(SafeHandle, addr, ref size);

            if (result != 0)
                throw new SocketCanException("Unable to get name on CAN_ISOTP socket.");

            return addr;
        }
    }
}