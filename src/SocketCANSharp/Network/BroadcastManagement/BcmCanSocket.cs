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
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace SocketCANSharp.Network.BroadcastManagement
{
    /// <summary>
    /// Provides Broadcast Manager CAN socket services.
    /// </summary>
    public class BcmCanSocket : AbstractCanSocket
    {
        /// <summary>
        /// Initializes a new instance of the BcmCanSocket class.
        /// </summary>
        /// <exception cref="SocketCanException">Unable to create the requested socket.</exception>
        public BcmCanSocket()
        {
            SocketType = SocketType.Dgram;
            ProtocolType = SocketCanProtocolType.CAN_BCM;
            SafeHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType, ProtocolType);

            if (SafeHandle.IsInvalid)
                throw new SocketCanException("Failed to create CAN_BCM socket.");
        }

        /// <summary>
        /// Sets the SocketCAN Base Address Structure on the CAN_BCM socket.
        /// </summary>
        /// <param name="addr">SocketCAN Base Address Structure.</param>
        /// <exception cref="ObjectDisposedException">The CAN_BCM socket has been closed.</exception>
        /// <exception cref="ArgumentNullException">SocketCAN Base Address Structure is null.</exception>
        /// <exception cref="SocketCanException">Unable to set the provided SocketCAN Base Address Structure on the CAN_BCM socket.</exception>
        public void Connect(SockAddrCan addr)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (addr == null)
                throw new ArgumentNullException(nameof(addr));

            int result = LibcNativeMethods.Connect(SafeHandle, addr, Marshal.SizeOf(typeof(SockAddrCan)));
            if (result != 0)
                throw new SocketCanException("Unable to set the provided SocketCAN raw address for the underlying CAN_BCM Socket.");

            Connected = true;
        }

        /// <summary>
        /// Sets the CAN Network Interface Index on the CAN_BCM socket.
        /// </summary>
        /// <param name="iface">CAN Network Interface instance containing the name and index of the interface.</param>
        /// <exception cref="ArgumentNullException">CanNetworkInterface instance is null.</exception>
        public void Connect(CanNetworkInterface iface)
        {
            if (iface == null) 
                throw new ArgumentNullException(nameof(iface));

            Connect(new SockAddrCan()
            {
                CanFamily = SocketCanConstants.AF_CAN,
                CanIfIndex = iface.Index,
            });
        }

        /// <summary>
        /// Creates a cyclic transmission task on the Broadcast Manager socket.
        /// </summary>
        /// <param name="taskConfiguration">Cylic Transmission Task Configuration</param>
        /// <param name="frames">CAN Frames to transmit</param>
        /// <returns>Number of bytes written to the Broadcast Manager socket.</returns>
        /// <exception cref="ArgumentNullException">CAN Frame Array or BcmCyclicTxTaskConfiguration instance is null.</exception>
        /// <exception cref="SocketCanException">Unable to create the requested task.</exception>
        public int CreateCyclicTransmissionTask(BcmCyclicTxTaskConfiguration taskConfiguration, CanFrame[] frames)
        {
            if (frames == null)
                throw new ArgumentNullException(nameof(frames));

            if (taskConfiguration == null)
                throw new ArgumentNullException(nameof(taskConfiguration));

            int nBytes;
            if (Environment.Is64BitProcess)
            {
                var header = new BcmMessageHeader(BcmOpcode.TX_SETUP)
                {
                    NumberOfFrames = (uint)frames.Length,
                    Flags = taskConfiguration.GetBcmFlags(),
                    Interval1Count = (uint)taskConfiguration.InitialIntervalConfiguration.Count,
                    Interval1 = taskConfiguration.InitialIntervalConfiguration.Interval,
                    Interval2 = taskConfiguration.PostInitialInterval,
                    CanId = taskConfiguration.Id,
                };

                var bcmMessage = new BcmCanMessage(header, frames);
                nBytes = LibcNativeMethods.Write(SafeHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
            }
            else
            {
                var header = new BcmMessageHeader32(BcmOpcode.TX_SETUP)
                {
                    NumberOfFrames = (uint)frames.Length,
                    Flags = taskConfiguration.GetBcmFlags(),
                    Interval1Count = (uint)taskConfiguration.InitialIntervalConfiguration.Count,
                    Interval1 = taskConfiguration.InitialIntervalConfiguration.Interval,
                    Interval2 = taskConfiguration.PostInitialInterval,
                    CanId = taskConfiguration.Id,    
                };

                var bcmMessage = new BcmCanMessage32(header, frames);
                nBytes = LibcNativeMethods.Write(SafeHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
            }

            if (nBytes == -1)
                throw new SocketCanException("Unable to set up cyclic transmission task.");

            return nBytes;
        }

        /// <summary>
        /// Creates a cyclic transmission task on the Broadcast Manager Socket.
        /// </summary>
        /// <param name="taskConfiguration">Cyclic Transmission Task Configuration</param>
        /// <param name="frames">CAN FD Frames to transmit</param>
        /// <returns>Number of bytes written to the Broadcast Manager socket.</returns>
        /// <exception cref="ArgumentNullException">CAN FD Frame Array or BcmCyclicTxTaskConfiguration instance is null.</exception>
        /// <exception cref="SocketCanException">Unable to create the requested task.</exception>
        public int CreateCyclicTransmissionTask(BcmCyclicTxTaskConfiguration taskConfiguration, CanFdFrame[] frames)
        {
            if (frames == null)
                throw new ArgumentNullException(nameof(frames));

            if (taskConfiguration == null)
                throw new ArgumentNullException(nameof(taskConfiguration));

            int nBytes;
            if (Environment.Is64BitProcess)
            {
                var header = new BcmMessageHeader(BcmOpcode.TX_SETUP)
                {
                    NumberOfFrames = (uint)frames.Length,
                    Flags = taskConfiguration.GetBcmFlags(),
                    Interval1Count = (uint)taskConfiguration.InitialIntervalConfiguration.Count,
                    Interval1 = taskConfiguration.InitialIntervalConfiguration.Interval,
                    Interval2 = taskConfiguration.PostInitialInterval,
                    CanId = taskConfiguration.Id,
                };

                var bcmMessage = new BcmCanFdMessage(header, frames);
                nBytes = LibcNativeMethods.Write(SafeHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
            }
            else
            {
                var header = new BcmMessageHeader32(BcmOpcode.TX_SETUP)
                {
                    NumberOfFrames = (uint)frames.Length,
                    Flags = taskConfiguration.GetBcmFlags(),
                    Interval1Count = (uint)taskConfiguration.InitialIntervalConfiguration.Count,
                    Interval1 = taskConfiguration.InitialIntervalConfiguration.Interval,
                    Interval2 = taskConfiguration.PostInitialInterval,
                    CanId = taskConfiguration.Id,    
                };

                var bcmMessage = new BcmCanFdMessage32(header, frames);
                nBytes = LibcNativeMethods.Write(SafeHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
            }

            if (nBytes == -1)
                throw new SocketCanException("Unable to set up cyclic transmission task.");

            return nBytes;
        }

        /// <summary>
        /// Removes a cyclic transmission task from the Broadcast Manager socket.
        /// </summary>
        /// <param name="id">Task ID (CAN ID).</param>
        /// <param name="canFrameType">BCM CAN Frame Type.</param>
        /// <returns>Number of bytes written to the Broadcast Manager socket.</returns>
        /// <exception cref="SocketCanException">Unable to remove the requested task.</exception>
        public int RemoveCyclicTransmissionTask(uint id, BcmCanFrameType canFrameType)
        {
            int nBytes;
            if (Environment.Is64BitProcess)
            {
                var deleteHeader = new BcmMessageHeader(BcmOpcode.TX_DELETE)
                {
                    CanId = id,
                    Flags = canFrameType == BcmCanFrameType.CANFD ? BcmFlags.CAN_FD_FRAME : BcmFlags.None,
                };
                nBytes = LibcNativeMethods.Write(SafeHandle, deleteHeader, Marshal.SizeOf(typeof(BcmMessageHeader)));
            }
            else
            {
                var deleteHeader = new BcmMessageHeader32(BcmOpcode.TX_DELETE)
                {
                    CanId = id,
                    Flags = canFrameType == BcmCanFrameType.CANFD ? BcmFlags.CAN_FD_FRAME : BcmFlags.None,
                };
                nBytes = LibcNativeMethods.Write(SafeHandle, deleteHeader, Marshal.SizeOf(typeof(BcmMessageHeader32)));
            }

            if (nBytes == -1)
                throw new SocketCanException("Unable to remove cyclic transmission task.");

            return nBytes;
        }

        /// <summary>
        /// Requests the queuing of the properties of a cyclic transmission task associated with the provided Task ID (CAN ID) from the Broadcast Manager socket.
        /// </summary>
        /// <param name="id">Task ID to look up the properties of.</param>
        /// <param name="canFrameType">BCM CAN Frame Type.</param>
        /// <returns>Number of bytes written to the Broadcast Manager socket.</returns>
        /// <exception cref="SocketCanException">Failed to write the read cyclic transmission task properties request to the socket.</exception>
        public int QueueCyclicTransmissionTaskProperties(uint id, BcmCanFrameType canFrameType)
        {
            int nBytes;
            if (Environment.Is64BitProcess)
            {
                var readHeader = new BcmMessageHeader(BcmOpcode.TX_READ)
                {
                    CanId = id,
                    Flags = canFrameType == BcmCanFrameType.CANFD ? BcmFlags.CAN_FD_FRAME : BcmFlags.None,
                };
                nBytes = LibcNativeMethods.Write(SafeHandle, readHeader, Marshal.SizeOf(typeof(BcmMessageHeader)));
            }
            else
            {
                var readHeader = new BcmMessageHeader32(BcmOpcode.TX_READ)
                {
                    CanId = id,
                    Flags = canFrameType == BcmCanFrameType.CANFD ? BcmFlags.CAN_FD_FRAME : BcmFlags.None,
                };
                nBytes = LibcNativeMethods.Write(SafeHandle, readHeader, Marshal.SizeOf(typeof(BcmMessageHeader32)));
            }

            if (nBytes == -1)
                throw new SocketCanException("Failed to write TX_READ request to socket.");

            return nBytes;
        }

        /// <summary>
        /// Reads a BCM message from the socket into the response object.
        /// </summary>
        /// <param name="bcmCanMessageResponse">BCM Message Response</param>
        /// <returns>Number of bytes read from the Broadcast Manager socket.</returns>
        /// <exception cref="SocketCanException">Unable to retrieve information from the socket.</exception>
        /// <exception cref="IOException">Unexpected/Unsupported BCM Socket Response Type</exception>
        public int Read(out BcmCanMessageResponse bcmCanMessageResponse)
        {
            int nBytes;
            if (Environment.Is64BitProcess)
            {
                var receiveMessage = new BcmGenericMessage();
                nBytes = LibcNativeMethods.Read(SafeHandle, receiveMessage, Marshal.SizeOf(typeof(BcmGenericMessage)));

                if (nBytes == -1)
                    throw new SocketCanException("Unable to retrieve information from the socket.");
                
                if (receiveMessage.Header.Opcode == BcmOpcode.TX_STATUS)
                {
                    bcmCanMessageResponse = new BcmCanMessageResponse()
                    {
                        FrameType = receiveMessage.Header.Flags.HasFlag(BcmFlags.CAN_FD_FRAME) ? BcmCanFrameType.CANFD : BcmCanFrameType.ClassicCAN,
                        ResponseType = BcmResponseType.TransmissionTaskConfiguration,
                        CyclicTransmissionTaskConfiguration = new BcmCyclicTxTaskConfiguration(receiveMessage.Header.CanId, receiveMessage.Header.Flags, receiveMessage.Header.Interval1Count, receiveMessage.Header.Interval1, receiveMessage.Header.Interval2),
                    };

                    if (bcmCanMessageResponse.FrameType == BcmCanFrameType.ClassicCAN)
                    {
                        bcmCanMessageResponse.ClassicFrames = receiveMessage.GetClassicFrames();
                    }
                    else if (bcmCanMessageResponse.FrameType == BcmCanFrameType.CANFD)
                    {
                        bcmCanMessageResponse.FdFrames = receiveMessage.GetFdFrames();
                    }
                }
                else if (receiveMessage.Header.Opcode == BcmOpcode.TX_EXPIRED)
                {
                    bcmCanMessageResponse = new BcmCanMessageResponse()
                    {
                        FrameType = receiveMessage.Header.Flags.HasFlag(BcmFlags.CAN_FD_FRAME) ? BcmCanFrameType.CANFD : BcmCanFrameType.ClassicCAN,
                        ResponseType = BcmResponseType.FirstIntervalTransmissionComplete,
                        CyclicTransmissionTaskConfiguration = new BcmCyclicTxTaskConfiguration(receiveMessage.Header.CanId, receiveMessage.Header.Flags, receiveMessage.Header.Interval1Count, receiveMessage.Header.Interval1, receiveMessage.Header.Interval2),
                    };
                }
                else if (receiveMessage.Header.Opcode == BcmOpcode.RX_STATUS)
                {
                    bcmCanMessageResponse = new BcmCanMessageResponse()
                    {
                        FrameType = receiveMessage.Header.Flags.HasFlag(BcmFlags.CAN_FD_FRAME) ? BcmCanFrameType.CANFD : BcmCanFrameType.ClassicCAN,
                        ResponseType = BcmResponseType.ReceiveFilterConfiguration,
                        ContentReceiveFilterSubscription = new BcmContentRxFilterSubscription(receiveMessage.Header.CanId, receiveMessage.Header.Flags, receiveMessage.Header.Interval1, receiveMessage.Header.Interval2),
                    };

                    if (bcmCanMessageResponse.FrameType == BcmCanFrameType.ClassicCAN)
                    {
                        bcmCanMessageResponse.ClassicFrames = receiveMessage.GetClassicFrames();
                    }
                    else if (bcmCanMessageResponse.FrameType == BcmCanFrameType.CANFD)
                    {
                        bcmCanMessageResponse.FdFrames = receiveMessage.GetFdFrames();
                    }
                }
                else if (receiveMessage.Header.Opcode == BcmOpcode.RX_TIMEOUT)
                {
                    bcmCanMessageResponse = new BcmCanMessageResponse()
                    {
                        FrameType = receiveMessage.Header.Flags.HasFlag(BcmFlags.CAN_FD_FRAME) ? BcmCanFrameType.CANFD : BcmCanFrameType.ClassicCAN,
                        ResponseType = BcmResponseType.CyclicMessageReceiveTimeout,
                        ContentReceiveFilterSubscription = new BcmContentRxFilterSubscription(receiveMessage.Header.CanId, receiveMessage.Header.Flags, receiveMessage.Header.Interval1, receiveMessage.Header.Interval2),
                        ClassicFrames = null,
                    };
                }
                else if (receiveMessage.Header.Opcode == BcmOpcode.RX_CHANGED)
                {
                    bcmCanMessageResponse = new BcmCanMessageResponse()
                    {
                        FrameType = receiveMessage.Header.Flags.HasFlag(BcmFlags.CAN_FD_FRAME) ? BcmCanFrameType.CANFD : BcmCanFrameType.ClassicCAN,
                        ResponseType = BcmResponseType.CanFrameReceiveUpdateNotification,
                        ContentReceiveFilterSubscription = new BcmContentRxFilterSubscription(receiveMessage.Header.CanId, receiveMessage.Header.Flags, receiveMessage.Header.Interval1, receiveMessage.Header.Interval2),
                    };

                    if (bcmCanMessageResponse.FrameType == BcmCanFrameType.ClassicCAN)
                    {
                        bcmCanMessageResponse.ClassicFrames = receiveMessage.GetClassicFrames();
                    }
                    else if (bcmCanMessageResponse.FrameType == BcmCanFrameType.CANFD)
                    {
                        bcmCanMessageResponse.FdFrames = receiveMessage.GetFdFrames();
                    }
                }
                else
                {
                    throw new IOException("Unexpected/Unsupported BCM Socket Response Type");
                }
            }
            else
            {
                var receiveMessage = new BcmGenericMessage32();
                nBytes = LibcNativeMethods.Read(SafeHandle, receiveMessage, Marshal.SizeOf(typeof(BcmGenericMessage32)));
                while (receiveMessage.Header.Opcode != BcmOpcode.TX_STATUS && nBytes != -1);

                if (nBytes == -1)
                    throw new SocketCanException("Unable to retrieve information from the socket.");

                if (receiveMessage.Header.Opcode == BcmOpcode.TX_STATUS)
                {
                    bcmCanMessageResponse = new BcmCanMessageResponse()
                    {
                        FrameType = receiveMessage.Header.Flags.HasFlag(BcmFlags.CAN_FD_FRAME) ? BcmCanFrameType.CANFD : BcmCanFrameType.ClassicCAN,
                        ResponseType = BcmResponseType.TransmissionTaskConfiguration,
                        CyclicTransmissionTaskConfiguration = new BcmCyclicTxTaskConfiguration(receiveMessage.Header.CanId, receiveMessage.Header.Flags, receiveMessage.Header.Interval1Count, receiveMessage.Header.Interval1, receiveMessage.Header.Interval2),
                    };

                    if (bcmCanMessageResponse.FrameType == BcmCanFrameType.ClassicCAN)
                    {
                        bcmCanMessageResponse.ClassicFrames = receiveMessage.GetClassicFrames();
                    }
                    else if (bcmCanMessageResponse.FrameType == BcmCanFrameType.CANFD)
                    {
                        bcmCanMessageResponse.FdFrames = receiveMessage.GetFdFrames();
                    }
                }
                else if (receiveMessage.Header.Opcode == BcmOpcode.TX_EXPIRED)
                {
                    bcmCanMessageResponse = new BcmCanMessageResponse()
                    {
                        FrameType = receiveMessage.Header.Flags.HasFlag(BcmFlags.CAN_FD_FRAME) ? BcmCanFrameType.CANFD : BcmCanFrameType.ClassicCAN,
                        ResponseType = BcmResponseType.FirstIntervalTransmissionComplete,
                        CyclicTransmissionTaskConfiguration = new BcmCyclicTxTaskConfiguration(receiveMessage.Header.CanId, receiveMessage.Header.Flags, receiveMessage.Header.Interval1Count, receiveMessage.Header.Interval1, receiveMessage.Header.Interval2),
                    };
                }
                else if (receiveMessage.Header.Opcode == BcmOpcode.RX_STATUS)
                {
                    bcmCanMessageResponse = new BcmCanMessageResponse()
                    {
                        FrameType = receiveMessage.Header.Flags.HasFlag(BcmFlags.CAN_FD_FRAME) ? BcmCanFrameType.CANFD : BcmCanFrameType.ClassicCAN,
                        ResponseType = BcmResponseType.ReceiveFilterConfiguration,
                        ContentReceiveFilterSubscription = new BcmContentRxFilterSubscription(receiveMessage.Header.CanId, receiveMessage.Header.Flags, receiveMessage.Header.Interval1, receiveMessage.Header.Interval2),
                    };

                    if (bcmCanMessageResponse.FrameType == BcmCanFrameType.ClassicCAN)
                    {
                        bcmCanMessageResponse.ClassicFrames = receiveMessage.GetClassicFrames();
                    }
                    else if (bcmCanMessageResponse.FrameType == BcmCanFrameType.CANFD)
                    {
                        bcmCanMessageResponse.FdFrames = receiveMessage.GetFdFrames();
                    }
                }
                else if (receiveMessage.Header.Opcode == BcmOpcode.RX_TIMEOUT)
                {
                    bcmCanMessageResponse = new BcmCanMessageResponse()
                    {
                        FrameType = receiveMessage.Header.Flags.HasFlag(BcmFlags.CAN_FD_FRAME) ? BcmCanFrameType.CANFD : BcmCanFrameType.ClassicCAN,
                        ResponseType = BcmResponseType.CyclicMessageReceiveTimeout,
                        ContentReceiveFilterSubscription = new BcmContentRxFilterSubscription(receiveMessage.Header.CanId, receiveMessage.Header.Flags, receiveMessage.Header.Interval1, receiveMessage.Header.Interval2),
                        ClassicFrames = null,
                    };
                }
                else if (receiveMessage.Header.Opcode == BcmOpcode.RX_CHANGED)
                {
                    bcmCanMessageResponse = new BcmCanMessageResponse()
                    {
                        FrameType = receiveMessage.Header.Flags.HasFlag(BcmFlags.CAN_FD_FRAME) ? BcmCanFrameType.CANFD : BcmCanFrameType.ClassicCAN,
                        ResponseType = BcmResponseType.CanFrameReceiveUpdateNotification,
                        ContentReceiveFilterSubscription = new BcmContentRxFilterSubscription(receiveMessage.Header.CanId, receiveMessage.Header.Flags, receiveMessage.Header.Interval1, receiveMessage.Header.Interval2),
                    };

                    if (bcmCanMessageResponse.FrameType == BcmCanFrameType.ClassicCAN)
                    {
                        bcmCanMessageResponse.ClassicFrames = receiveMessage.GetClassicFrames();
                    }
                    else if (bcmCanMessageResponse.FrameType == BcmCanFrameType.CANFD)
                    {
                        bcmCanMessageResponse.FdFrames = receiveMessage.GetFdFrames();
                    }
                }
                else
                {
                    throw new IOException("Unexpected/Unsupported BCM Socket Response Type");
                }
            }

            return nBytes;
        }

        /// <summary>
        /// Writes a single CAN Frame to the Broadcast Manager socket.
        /// </summary>
        /// <param name="canFrame">CAN Frame</param>
        /// <returns>Number of bytes written to the socket</returns>
        /// <exception cref="SocketCanException">Unable to write the CAN Frame to the socket.</exception>
        public int SendSingleFrame(CanFrame canFrame)
        {
            int nBytes;
            if (Environment.Is64BitProcess)
            {
                var header = new BcmMessageHeader(BcmOpcode.TX_SEND)
                {
                    NumberOfFrames = 1,
                };

                var bcmMessage = new BcmCanSingleMessage(header, canFrame);
                nBytes = LibcNativeMethods.Write(SafeHandle, bcmMessage, Marshal.SizeOf(bcmMessage));;
            }
            else
            {
                var header = new BcmMessageHeader32(BcmOpcode.TX_SEND)
                {
                    NumberOfFrames = 1,
                };

                var bcmMessage = new BcmCanSingleMessage32(header, canFrame);
                nBytes = LibcNativeMethods.Write(SafeHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
            }

            if (nBytes == -1)
                throw new SocketCanException("Unable to transmit CAN Frame to the socket.");

            return nBytes;
        }

        /// <summary>
        /// Writes a single CAN FD Frame to the Broadcast Manager socket.
        /// </summary>
        /// <param name="canFrame">CAN FD Frame</param>
        /// <returns>Number of bytes written to the socket.</returns>
        /// <exception cref="SocketCanException">Unable to write the CAN FD Frame to the socket.</exception>
        public int SendSingleFrame(CanFdFrame canFrame)
        {
            int nBytes;
            if (Environment.Is64BitProcess)
            {
                var header = new BcmMessageHeader(BcmOpcode.TX_SEND)
                {
                    NumberOfFrames = 1,
                };

                var bcmMessage = new BcmCanFdSingleMessage(header, canFrame);
                nBytes = LibcNativeMethods.Write(SafeHandle, bcmMessage, Marshal.SizeOf(bcmMessage));;
            }
            else
            {
                var header = new BcmMessageHeader32(BcmOpcode.TX_SEND)
                {
                    NumberOfFrames = 1,
                };

                var bcmMessage = new BcmCanFdSingleMessage32(header, canFrame);
                nBytes = LibcNativeMethods.Write(SafeHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
            }

            if (nBytes == -1)
                throw new SocketCanException("Unable to transmit CAN FD Frame to the socket.");

            return nBytes;
        }

        /// <summary>
        /// Creates a receive filter subscription on the Broadcast Manager Socket.
        /// </summary>
        /// <param name="filterSubscription">Receive Filter Subscription</param>
        /// <param name="frames">CAN Frames Array</param>
        /// <returns>Number of bytes written to the socket.</returns>
        /// <exception cref="ArgumentNullException">CAN Frame Array or BcmContentRxFilterSubscription instance is null.</exception>
        /// <exception cref="SocketCanException">Unable to set up content receive filter subscription.</exception>
        public int CreateReceiveFilterSubscription(BcmContentRxFilterSubscription filterSubscription, CanFrame[] frames)
        {
            if (frames == null)
                throw new ArgumentNullException(nameof(frames));

            if (filterSubscription == null)
                throw new ArgumentNullException(nameof(filterSubscription));

            int nBytes;
            if (Environment.Is64BitProcess)
            {
                var header = new BcmMessageHeader(BcmOpcode.RX_SETUP)
                {
                    NumberOfFrames = (uint)frames.Length,
                    Flags = filterSubscription.GetBcmFlags(),
                    Interval1 = filterSubscription.ReceiveTimeout,
                    Interval2 = filterSubscription.ReceiveMessageRateLimit,
                    CanId = filterSubscription.Id,
                };

                var bcmMessage = new BcmCanMessage(header, frames);
                nBytes = LibcNativeMethods.Write(SafeHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
            }
            else
            {
                var header = new BcmMessageHeader32(BcmOpcode.RX_SETUP)
                {
                    NumberOfFrames = (uint)frames.Length,
                    Flags = filterSubscription.GetBcmFlags(),
                    Interval1 = filterSubscription.ReceiveTimeout,
                    Interval2 = filterSubscription.ReceiveMessageRateLimit,
                    CanId = filterSubscription.Id,
                };

                var bcmMessage = new BcmCanMessage32(header, frames);
                nBytes = LibcNativeMethods.Write(SafeHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
            }

            if (nBytes == -1)
                throw new SocketCanException("Unable to set up content receive filter subscription.");

            return nBytes;
        }

        /// <summary>
        /// Creates a receive filter subscription on the Broadcast Manager Socket.
        /// </summary>
        /// <param name="filterSubscription">Receive Filter Subscription</param>
        /// <param name="frames">CAN FD Frames Array</param>
        /// <returns>Number of bytes written to the socket.</returns>
        /// <exception cref="ArgumentNullException">CAN FD Frame Array or BcmContentRxFilterSubscription instance is null.</exception>
        /// <exception cref="SocketCanException">Unable to set up content receive filter subscription.</exception>
        public int CreateReceiveFilterSubscription(BcmContentRxFilterSubscription filterSubscription, CanFdFrame[] frames)
        {
            if (frames == null)
                throw new ArgumentNullException(nameof(frames));

            if (filterSubscription == null)
                throw new ArgumentNullException(nameof(filterSubscription));

            int nBytes;
            if (Environment.Is64BitProcess)
            {
                var header = new BcmMessageHeader(BcmOpcode.RX_SETUP)
                {
                    NumberOfFrames = (uint)frames.Length,
                    Flags = filterSubscription.GetBcmFlags(),
                    Interval1 = filterSubscription.ReceiveTimeout,
                    Interval2 = filterSubscription.ReceiveMessageRateLimit,
                    CanId = filterSubscription.Id,
                };

                var bcmMessage = new BcmCanFdMessage(header, frames);
                nBytes = LibcNativeMethods.Write(SafeHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
            }
            else
            {
                var header = new BcmMessageHeader32(BcmOpcode.RX_SETUP)
                {
                    NumberOfFrames = (uint)frames.Length,
                    Flags = filterSubscription.GetBcmFlags(),
                    Interval1 = filterSubscription.ReceiveTimeout,
                    Interval2 = filterSubscription.ReceiveMessageRateLimit,
                    CanId = filterSubscription.Id,
                };

                var bcmMessage = new BcmCanFdMessage32(header, frames);
                nBytes = LibcNativeMethods.Write(SafeHandle, bcmMessage, Marshal.SizeOf(bcmMessage));
            }

            if (nBytes == -1)
                throw new SocketCanException("Unable to set up content receive filter subscription.");

            return nBytes;
        }

        /// <summary>
        /// Removes a receive filter subscription from the Broadcast Manager socket.
        /// </summary>
        /// <param name="id">Filter ID.</param>
        /// <param name="canFrameType">BCM CAN Frame Type.</param>
        /// <returns>Number of bytes written to the Broadcast Manager socket.</returns>
        /// <exception cref="SocketCanException">Unable to remove receive filter subscription.</exception>
        public int RemoveReceiveFilterSubscription(uint id, BcmCanFrameType canFrameType)
        {
            int nBytes;
            if (Environment.Is64BitProcess)
            {
                var deleteHeader = new BcmMessageHeader(BcmOpcode.RX_DELETE)
                {
                    CanId = id,
                    Flags = canFrameType == BcmCanFrameType.CANFD ? BcmFlags.CAN_FD_FRAME : BcmFlags.None,
                };
                nBytes = LibcNativeMethods.Write(SafeHandle, deleteHeader, Marshal.SizeOf(typeof(BcmMessageHeader)));
            }
            else
            {
                var deleteHeader = new BcmMessageHeader32(BcmOpcode.RX_DELETE)
                {
                    CanId = id,
                    Flags = canFrameType == BcmCanFrameType.CANFD ? BcmFlags.CAN_FD_FRAME : BcmFlags.None,
                };
                nBytes = LibcNativeMethods.Write(SafeHandle, deleteHeader, Marshal.SizeOf(typeof(BcmMessageHeader32)));
            }

            if (nBytes == -1)
                throw new SocketCanException("Unable to remove receive filter subscription.");

            return nBytes;
        }

        /// <summary>
        /// Requests the queuing of the properties of a content receive filter subscription associated with the provided CAN ID from the Broadcast Manager socket.
        /// </summary>
        /// <param name="id">Filter ID to look up the properties of.</param>
        /// <param name="canFrameType">BCM CAN Frame Type.</param>
        /// <returns>Number of bytes written to the Broadcast Manager socket.</returns>
        /// <exception cref="SocketCanException">Failed to write the read subscription properties request to the socket.</exception>
        public int QueueReceiveFilterSubscriptionProperties(uint id, BcmCanFrameType canFrameType)
        {
            int nBytes;
            if (Environment.Is64BitProcess)
            {
                var readHeader = new BcmMessageHeader(BcmOpcode.RX_READ)
                {
                    CanId = id,
                    Flags = canFrameType == BcmCanFrameType.CANFD ? BcmFlags.CAN_FD_FRAME : BcmFlags.None,
                };
                nBytes = LibcNativeMethods.Write(SafeHandle, readHeader, Marshal.SizeOf(typeof(BcmMessageHeader)));
            }
            else
            {
                var readHeader = new BcmMessageHeader32(BcmOpcode.RX_READ)
                {
                    CanId = id,
                    Flags = canFrameType == BcmCanFrameType.CANFD ? BcmFlags.CAN_FD_FRAME : BcmFlags.None,
                };
                nBytes = LibcNativeMethods.Write(SafeHandle, readHeader, Marshal.SizeOf(typeof(BcmMessageHeader32)));
            }

            if (nBytes == -1)
                throw new SocketCanException("Failed to write the read subscription properties request to the socket..");

            return nBytes;
        }
    }
}