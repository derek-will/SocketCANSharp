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

namespace SocketCANSharp
{
    /// <summary>
    /// Protocols of the PF_CAN protocol family.
    /// </summary>
    public enum SocketCanProtocolType 
    { 
        /// <summary>
        /// Raw CAN
        /// </summary>
        CAN_RAW		= 1,
        /// <summary>
        /// CAN Broadcast Manager
        /// </summary>
        CAN_BCM		= 2,
        /// <summary>
        /// VW Transport Protocol v1.6
        /// </summary>
        CAN_TP16	= 3,
        /// <summary>
        /// VW Transport Protocol v2.0
        /// </summary>
        CAN_TP20	= 4,
        /// <summary>
        /// ISO 15765-2 Transport Protocol
        /// </summary>
        CAN_ISOTP	= 6,
        /// <summary>
        /// SAE J1939
        /// </summary>
        CAN_J1939   = 7,
        /// <summary>
        /// For Future Protocol
        /// </summary>
        CAN_NPROTO	= 8,
    }

    /// <summary>
    /// Socket levels at which options reside.
    /// </summary>
    public enum SocketLevel
    {
        /// <summary>
        /// Socket Option Level for Socket Layer itself. For Protocol Independent Options.
        /// </summary>
        SOL_SOCKET      = 1,
        /// <summary>
        /// Socket Option Level for Raw CAN protocol.
        /// </summary>
        SOL_CAN_RAW     = 101,
        /// <summary>
        /// Socket Option Level for ISO-TP over CAN protocol.
        /// </summary>
        SOL_CAN_ISOTP   = 106,
        /// <summary>
        /// Socket Option Level for J1939 over CAN protocol.
        /// </summary>
        SOL_CAN_J1939   = 107,
    }

    /// <summary>
    /// CAN_RAW socket options.
    /// </summary>
    public enum CanSocketOptions 
    {  
        /// <summary>
        /// Set 0 to N Raw CAN Filters.
        /// </summary>
        CAN_RAW_FILTER          = 1,
        /// <summary>
        /// Set Filter for Raw CAN Error Frames.
        /// </summary>
        CAN_RAW_ERR_FILTER      = 2,
        /// <summary>
        /// Local loopback for Raw CAN (on by default)
        /// </summary>
        CAN_RAW_LOOPBACK        = 3,
        /// <summary>
        /// Receive own Raw CAN messages (off by default)
        /// </summary>
        CAN_RAW_RECV_OWN_MSGS   = 4,
        /// <summary>
        /// Allow for CAN FD frames (off by default)
        /// </summary>
        CAN_RAW_FD_FRAMES       = 5,
        /// <summary>
        /// All filters must match
        /// </summary>
        CAN_RAW_JOIN_FILTERS    = 6,
    }

    /// <summary>
    /// Socket Layer Options.
    /// </summary>
    public enum SocketLevelOptions
    {
        /// <summary>
        /// Permits sending of broadcast messages, if it is supported by the underlying protocol. 
        /// </summary>
        SO_BROADCAST    =  6,
        /// <summary>
        /// Reports the size, in bytes, of the send buffer of the socket. The kernel doubles the value supplied in the call to setsockopt and this doubled value is what is returned when getsockopt is called. 
        /// </summary>
        SO_SNDBUF       =  7,
        /// <summary>
        /// Reports the size, in bytes, of the receive buffer of the socket. The kernel doubles the value supplied in the call to setsockopt and this doubled value is what is returned when getsockopt is called. 
        /// </summary>
        SO_RCVBUF       =  8,
        /// <summary>
        /// Sets the timeout value to wait for an input function (read, recv, etc.) to complete. Set to 0 to wait indefinitely.
        /// </summary>
        SO_RCVTIMEO     = 20,
        /// <summary>
        /// Sets the timeout value to wait for an output function (write, send, etc.) to complete. Set to 0 to wait indefinitely.
        /// </summary>
        SO_SNDTIMEO     = 21,
    }

    /// <summary>
    /// Bits in the FLAGS argument of `send', `recv', etc. 
    /// </summary>
    public enum MessageFlags
    {
        /// <summary>
        /// None.
        /// </summary>
        None            = 0x00,
        /// <summary>
        /// Peek at incoming messages without removing them from the receive queue.
        /// </summary>
        MSG_PEEK		= 0x02,
        /// <summary>
        /// Return the real length of the packet or datagram even when it is larger than the passed buffer.
        /// </summary>
        MSG_TRUNC		= 0x20,
        /// <summary>
        /// Nonblocking IO.
        /// </summary>
        MSG_DONTWAIT	= 0x40,
    }

    /// <summary>
    /// CAN_ISOTP socket options.
    /// </summary>
    public enum CanIsoTpSocketOptions 
    {
        /// <summary>
        /// For passing CanIsoTpOptions.
        /// </summary>
        CAN_ISOTP_OPTS      = 1,
        /// <summary>
        /// For passing CanIsoTpFlowControlOptions.
        /// </summary>
        CAN_ISOTP_RECV_FC   = 2,
        /// <summary>
        /// For passing an unsigned 32-bit value representing the time in nano secs to use for STmin instead of the value provided in the FC frame from the receiver.
        /// </summary>
        CAN_ISOTP_TX_STMIN  = 3,
        /// <summary>
        /// For passing an unsigned 32-bit value representing the time in nano secs to use for STmin and ignore received CF frames which timestamps differ less than this value.  
        /// </summary>
        CAN_ISOTP_RX_STMIN  = 4,
        /// <summary>
        /// For passing CanIsoTpLinkLayerOptions.
        /// </summary>
        CAN_ISOTP_LL_OPTS   = 5
    }

    /// <summary>
    /// Special address description flags for the CAN ID field.
    /// </summary>
    [Flags]
    public enum CanIdFlags : uint
    {       
        /// <summary>
        /// Specifies whether to use Extended Frame Format (EFF) or Standard Frame Format (SFF).
        /// </summary>
        CAN_EFF_FLAG = 0x80000000,
        /// <summary>
        /// Specifies whether the CAN ID has the Remote Transmission Request (RTR) bit set or not.
        /// </summary>
        CAN_RTR_FLAG = 0x40000000,
        /// <summary>
        /// Specifies whether this CAN Frame is an Error Frame or not.
        /// </summary>
        CAN_ERR_FLAG = 0x20000000,
    }        
    
    /// <summary>
    /// CAN FD specific flags.
    /// </summary>
    [Flags]
    public enum CanFdFlags : byte
    {
        /// <summary>
        /// None.
        /// </summary>
        None      = 0x00,
        /// <summary>
        /// Bit Rate Switch - Sets the BRS bit meaning that the payload is transmitted at the Data Rate instead of the Arbitration Rate.
        /// </summary>
        CANFD_BRS = 0x01,
        /// <summary>
        /// Error State Indicator - Sets the ESI bit to indicate that the node is in Error Passive mode.
        /// </summary>
        CANFD_ESI = 0x02,
        /// <summary>
        /// FD Frame - Indicates that CAN FD is using the CanFdFrame structure for both Classical CAN and CAN FD content.
        /// </summary>
        CANFD_FDF = 0x04, 
    }

    /// <summary>
    /// Error Class (Mask) in CAN ID.
    /// </summary>
    [Flags] 
    public enum CanErrorClass : uint
    {
        /// <summary>
        /// TX timeout by netdevice driver
        /// </summary>
        CAN_ERR_TX_TIMEOUT  = 0x00000001,
        /// <summary>
        /// Lost arbitration - See CAN Data[0]
        /// </summary>
        CAN_ERR_LOSTARB     = 0x00000002,
        /// <summary>
        /// CAN controller problems - See CAN Data[1]
        /// </summary>
        CAN_ERR_CRTL        = 0x00000004,
        /// <summary>
        /// CAN protocol violations - See CAN Data[2-3]
        /// </summary>
        CAN_ERR_PROT        = 0x00000008,
        /// <summary>
        /// CAN transceiver status - See CAN Data[4]
        /// </summary>
        CAN_ERR_TRX         = 0x00000010,
        /// <summary>
        /// Received no ACK on transmission
        /// </summary>
        CAN_ERR_ACK         = 0x00000020,
        /// <summary>
        /// Bus off
        /// </summary>
        CAN_ERR_BUSOFF      = 0x00000040,
        /// <summary>
        /// Bus error
        /// </summary>
        CAN_ERR_BUSERROR    = 0x00000080,
        /// <summary>
        /// CAN controller restarted
        /// </summary>
        CAN_ERR_RESTARTED   = 0x00000100,
    }

    /// <summary>
    /// Error status of CAN controller specified in CAN Data[1] of Error Frame
    /// </summary>
    [Flags]
    public enum CanControllerErrorStatus : byte
    {
        /// <summary>
        /// No CAN Controller error specified
        /// </summary>
        CAN_ERR_CRTL_UNSPEC         = 0x00,
        /// <summary>
        /// RX buffer overflow
        /// </summary>
        CAN_ERR_CRTL_RX_OVERFLOW    = 0x01,
        /// <summary>
        /// TX buffer overflow 
        /// </summary>
        CAN_ERR_CRTL_TX_OVERFLOW    = 0x02,
        /// <summary>
        /// Reached warning level for RX errors
        /// </summary>
        CAN_ERR_CRTL_RX_WARNING     = 0x04,
        /// <summary>
        /// Reached warning level for TX errors
        /// </summary>
        CAN_ERR_CRTL_TX_WARNING     = 0x08,
        /// <summary>
        /// Reached error passive status RX
        /// </summary>
        CAN_ERR_CRTL_RX_PASSIVE     = 0x10,
        /// <summary>
        /// reached error passive status TX
        /// </summary>
        CAN_ERR_CRTL_TX_PASSIVE     = 0x20,
        /// <summary>
        /// Recovered to error active state
        /// </summary>
        CAN_ERR_CRTL_ACTIVE         = 0x40,
    }

    /// <summary>
    /// Error type in CAN protocol (type) specified in CAN Data[2] of Error Frame
    /// </summary>
    [Flags]
    public enum CanProtocolErrorType : byte
    {
        /// <summary>
        /// No CAN protocol error type specified
        /// </summary>
        CAN_ERR_PROT_UNSPEC         = 0x00,
        /// <summary>
        /// Single bit error
        /// </summary>
        CAN_ERR_PROT_BIT            = 0x01,
        /// <summary>
        /// Frame format error
        /// </summary>
        CAN_ERR_PROT_FORM           = 0x02,
        /// <summary>
        /// Bit stuffing error
        /// </summary>
        CAN_ERR_PROT_STUFF          = 0x04,
        /// <summary>
        /// Unable to send dominant bit
        /// </summary>
        CAN_ERR_PROT_BIT0           = 0x08,
        /// <summary>
        /// unable to send recessive bit
        /// </summary>
        CAN_ERR_PROT_BIT1           = 0x10,
        /// <summary>
        /// Bus overload
        /// </summary>
        CAN_ERR_PROT_OVERLOAD       = 0x20,
        /// <summary>
        /// Active error announcement
        /// </summary>
        CAN_ERR_PROT_ACTIVE         = 0x40,
        /// <summary>
        /// Error occurred on transmission
        /// </summary>
        CAN_ERR_PROT_TX             = 0x80,
    }

    /// <summary>
    /// Location of CAN protocol error specified in CAN Data[3] of Error Frame
    /// </summary>
    public enum CanProtocolErrorLocation
    {
        /// <summary>
        /// Error Location not specified
        /// </summary>
        CAN_ERR_PROT_LOC_UNSPEC  = 0x00,
        /// <summary>
        /// ID bits 28-21 (SFF: 10-3)
        /// </summary>
        CAN_ERR_PROT_LOC_ID28_21 = 0x02,
        /// <summary>
        /// Start of frame
        /// </summary>
        CAN_ERR_PROT_LOC_SOF     = 0x03,
        /// <summary>
        /// Substitute RTR (SFF: RTR)
        /// </summary>
        CAN_ERR_PROT_LOC_SRTR    = 0x04,
        /// <summary>
        /// Identifier extension
        /// </summary>
        CAN_ERR_PROT_LOC_IDE     = 0x05,
        /// <summary>
        /// ID bits 20-18 (SFF: 2-0)
        /// </summary>
        CAN_ERR_PROT_LOC_ID20_18 = 0x06,
        /// <summary>
        /// ID bits 17-13
        /// </summary>
        CAN_ERR_PROT_LOC_ID17_13 = 0x07,
        /// <summary>
        /// CRC sequence
        /// </summary>
        CAN_ERR_PROT_LOC_CRC_SEQ = 0x08,
        /// <summary>
        /// Reserved bit 0
        /// </summary>
        CAN_ERR_PROT_LOC_RES0    = 0x09,
        /// <summary>
        /// Data section
        /// </summary>
        CAN_ERR_PROT_LOC_DATA    = 0x0A,
        /// <summary>
        /// Data length code
        /// </summary>
        CAN_ERR_PROT_LOC_DLC     = 0x0B,
        /// <summary>
        /// RTR
        /// </summary>
        CAN_ERR_PROT_LOC_RTR     = 0x0C,
        /// <summary>
        /// Reserved bit 1
        /// </summary>
        CAN_ERR_PROT_LOC_RES1    = 0x0D,
        /// <summary>
        /// ID bits 4-0
        /// </summary>
        CAN_ERR_PROT_LOC_ID04_00 = 0x0E,
        /// <summary>
        /// ID bits 12-5
        /// </summary>
        CAN_ERR_PROT_LOC_ID12_05 = 0x0F,
        /// <summary>
        /// Intermission
        /// </summary>
        CAN_ERR_PROT_LOC_INTERM  = 0x12,
        /// <summary>
        /// CRC delimiter
        /// </summary>
        CAN_ERR_PROT_LOC_CRC_DEL = 0x18,
        /// <summary>
        /// ACK slot
        /// </summary>
        CAN_ERR_PROT_LOC_ACK     = 0x19,
        /// <summary>
        /// End of frame
        /// </summary>
        CAN_ERR_PROT_LOC_EOF     = 0x1A,
        /// <summary>
        /// ACK delimiter
        /// </summary>
        CAN_ERR_PROT_LOC_ACK_DEL = 0x1B,
    }
 
    /// <summary>
    /// Error status of CAN transceiver specified in CAN Data[4] of Error Frame
    /// </summary>
    public enum CanTransceiverErrorStatus : byte
    {
        /// <summary>
        /// Transceiver error status not specified
        /// </summary>
        CAN_ERR_TRX_UNSPEC             = 0x00,
        /// <summary>
        /// CANH No Wire
        /// </summary>
        CAN_ERR_TRX_CANH_NO_WIRE       = 0x04,
        /// <summary>
        /// CANH Short to Battery
        /// </summary>
        CAN_ERR_TRX_CANH_SHORT_TO_BAT  = 0x05,
        /// <summary>
        /// CANH Short to Voltage
        /// </summary>
        CAN_ERR_TRX_CANH_SHORT_TO_VCC  = 0x06,
        /// <summary>
        /// CANH Short to Ground
        /// </summary>
        CAN_ERR_TRX_CANH_SHORT_TO_GND  = 0x07,
        /// <summary>
        /// CANL No Wire
        /// </summary>
        CAN_ERR_TRX_CANL_NO_WIRE       = 0x40,
        /// <summary>
        /// CANL Short to Battery
        /// </summary>
        CAN_ERR_TRX_CANL_SHORT_TO_BAT  = 0x50,
        /// <summary>
        /// CANL Short to Voltage
        /// </summary>
        CAN_ERR_TRX_CANL_SHORT_TO_VCC  = 0x60,
        /// <summary>
        /// CANL Short to Ground
        /// </summary>
        CAN_ERR_TRX_CANL_SHORT_TO_GND  = 0x70,
        /// <summary>
        /// CANL Short to CANH
        /// </summary>
        CAN_ERR_TRX_CANL_SHORT_TO_CANH = 0x80,
    }
    
    /// <summary>
    /// ISO 15765-2 (ISO-TP) Option Flags
    /// </summary>
    [Flags]
    public enum IsoTpFlags : uint
    {
        /// <summary>
        /// Listen only (do not send Flow Control Frames in response to First Frames)
        /// </summary>
        CAN_ISOTP_LISTEN_MODE   = 0x00000001,
        /// <summary>
        /// Enable extended addressing.
        /// </summary>
        CAN_ISOTP_EXTEND_ADDR   = 0x00000002,
        /// <summary>
        /// Enable CAN frame padding on transmissions.
        /// </summary>
        CAN_ISOTP_TX_PADDING    = 0x00000004,
        /// <summary>
        /// Enable CAN frame padding on receptions.
        /// </summary>
        CAN_ISOTP_RX_PADDING    = 0x00000008, 
        /// <summary>
        /// Check received CAN frame padding length.
        /// </summary>
        CAN_ISOTP_CHK_PAD_LEN   = 0x00000010,
        /// <summary>
        /// Check received CAN frame padding byte.
        /// </summary>
        CAN_ISOTP_CHK_PAD_DATA  = 0x00000020,
        /// <summary>
        /// Half duplex error state handling.
        /// </summary>
        CAN_ISOTP_HALF_DUPLEX   = 0x00000040,
        /// <summary>
        /// Ignore STmin in received Flow Control Frames.
        /// </summary>
        CAN_ISOTP_FORCE_TXSTMIN = 0x00000080,
        /// <summary>
        /// Ignore Consecutive Frames that do not honor transmitted STmin.
        /// </summary>
        CAN_ISOTP_FORCE_RXSTMIN = 0x00000100,
        /// <summary>
        /// Set different Extended Address for reception.
        /// </summary>
        CAN_ISOTP_RX_EXT_ADDR   = 0x00000200,
        /// <summary>
        /// Wait for transmission to complete.
        /// </summary>
        CAN_ISOTP_WAIT_TX_DONE  = 0x00000400,
        /// <summary>
        /// Enable 1-to-N functional addressing.
        /// </summary>
        CAN_ISOTP_SF_BROADCAST  = 0x00000800,
        /// <summary>
        /// Enable 1-to-N functionally addressed segmented transfers without using Flow Control.
        /// Note: This is not supported via the ISO-TP specification (ISO 15765-2), but is instead a SocketCAN special feature.
        /// </summary>
        CAN_ISOTP_CF_BROADCAST  = 0x00001000,
    }

    /// <summary>
    /// Broadcast Manager Operation Codes.
    /// </summary>
    public enum BcmOpcode : uint 
    {
        /// <summary>
        /// Undefined (Not Set).
        /// </summary>
        UNDEFINED   = 0,
        /// <summary>
        /// Create (cyclic) transmission task.
        /// </summary>
        TX_SETUP    = 1,
        /// <summary>
        /// Remove (cyclic) transmission task.
        /// </summary>
        TX_DELETE   = 2,
        /// <summary>
        /// Read properties of (cyclic) transmission task.
        /// </summary>
        TX_READ     = 3,
        /// <summary>
        /// Send a single CAN frame.
        /// </summary>
        TX_SEND 	= 4,
        /// <summary>
        /// Create RX content filter subscription.
        /// </summary>
        RX_SETUP    = 5,
        /// <summary>
        /// Remove RX content filter subscription.
        /// </summary>
        RX_DELETE   = 6,
        /// <summary>
        /// Read properties of RX content filter subscription.
        /// </summary>
        RX_READ     = 7,
        /// <summary>
        /// Reply sent by Broadcast Manager to a TX_READ request.
        /// </summary>
        TX_STATUS   = 8,
        /// <summary>
        /// Notification sent by Broadcast Manager when count is reached and flag TX_COUNTEVT is set.
        /// </summary>
        TX_EXPIRED  = 9,
        /// <summary>
        /// Reply sent by Broadcast Manager to a RX_READ request.
        /// </summary>
        RX_STATUS   = 10,
        /// <summary>
        /// Notification sent by Broadcast Manager when a cyclic message is absent.
        /// </summary>
        RX_TIMEOUT  = 11,
        /// <summary>
        /// Notification sent by Broadcast Manager when a CAN frame is updated or first sent. 
        /// </summary>
        RX_CHANGED  = 12,
    };

    /// <summary>
    /// Broadcast Manager (BCM) Option Flags.
    /// </summary>
    [Flags]
    public enum BcmFlags : uint
    {
        /// <summary>
        /// Not set.
        /// </summary>
        None                = 0x0000,
        /// <summary>
        /// Set the value of Interval1, Interval2 and Interval1Count.
        /// </summary>
        SETTIMER            = 0x0001,
        /// <summary>
        /// Start the timer with the actual value of Interval1, Interval2 and Interval1Count. Immediately starts transmitting CAN Frames.
        /// </summary>
        STARTTIMER          = 0x0002,
        /// <summary>
        /// Send the TX_EXPIRED message when Interval1Count is reached.
        /// </summary>
        TX_COUNTEVT         = 0x0004,
        /// <summary>
        /// A change of data by the process is emitted with a new frame immediately, regardless of the timer status.
        /// </summary>
        TX_ANNOUNCE         = 0x0008,
        /// <summary>
        /// Copies the CanId from the BCM message header to each subsequent CAN Frame in the frame sequence.
        /// </summary>
        TX_CP_CAN_ID        = 0x0010,
        /// <summary>
        /// Filter by CanId only.
        /// </summary>
        RX_FILTER_ID        = 0x0020,
        /// <summary>
        /// A change of the DLC (Data Length Code) leads to an RX_CHANGED notitication.
        /// </summary>
        RX_CHECK_DLC        = 0x0040,
        /// <summary>
        /// Prevent automatically starting the timeout monitor.
        /// </summary>
        RX_NO_AUTOTIMER     = 0x0080,
        /// <summary>
        /// When when a receive timeout occours, a RX_CHANGED will be generated when the (cyclic) receive restarts. 
        /// </summary>
        RX_ANNOUNCE_RESUME  = 0x0100,
        /// <summary>
        /// Reset the index for a multiple frame transmission.
        /// </summary>
        TX_RESET_MULTI_IDX  = 0x0200,
        /// <summary>
        /// Use the filter passed in the RX_SETUP as a CAN message to reply with when receiving a RTR frame.
        /// </summary>
        RX_RTR_FRAME        = 0x0400,
        /// <summary>
        /// The Broadcast Manager uses CAN FD Frames.
        /// </summary>
        CAN_FD_FRAME        = 0x0800,
    }

    /// <summary>
    /// CAN_J1939 socket options.
    /// </summary>
    public enum J1939SocketOptions 
    {
        /// <summary>
        /// Set filters.
        /// </summary>
        SO_J1939_FILTER     = 1,
        /// <summary>
        /// Set or clear promiscuous mode.
        /// </summary>
        SO_J1939_PROMISC    = 2,
        /// <summary>
        /// Change default send priority.
        /// </summary>
        SO_J1939_SEND_PRIO  = 3,
        /// <summary>
        /// Queue errors.
        /// </summary>
        SO_J1939_ERRQUEUE   = 4,
    }

    /// <summary>
    /// Operations to perform on an epoll file descriptor.
    /// </summary>
    public enum EpollOperation
    {
        /// <summary>
        /// Add a file descriptor to the interest list of the epoll file descriptor.
        /// </summary>
        EPOLL_CTL_ADD = 1,
        /// <summary>
        /// Remove a file descriptor from the interest list of the epoll file descriptor.
        /// </summary>
        EPOLL_CTL_DEL = 2,
        /// <summary>
        /// Change the settings associated with a file descriptor within the interest list.
        /// </summary>
        EPOLL_CTL_MOD = 3,
    }

    /// <summary>
    /// Epoll event types.
    /// </summary>
    [Flags]
    public enum EpollEventType : uint
    {
        /// <summary>
        /// The associated file descriptor is available for read operations.
        /// </summary>
        EPOLLIN         = 0x00000001,
        /// <summary>
        /// There is urgent data available to be read on the associated file descriptor.
        /// </summary>
        EPOLLPRI        = 0x00000002,
        /// <summary>
        /// The associated file descriptor is available for write operations.
        /// </summary>
        EPOLLOUT        = 0x00000004,
        /// <summary>
        /// Error condition happened on the associated file descriptor.
        /// </summary>
        EPOLLERR        = 0x00000008,
        /// <summary>
        /// Hang up happened on the associated file descriptor.
        /// </summary>
        EPOLLHUP        = 0x00000010,
        /// <summary>
        /// Same as EPOLLIN.
        /// </summary>
        EPOLLRDNORM     = 0x00000040,
        /// <summary>
        /// Priority data band can be read.
        /// </summary>
        EPOLLRDBAND     = 0x00000080,
        /// <summary>
        /// Same as EPOLLOUT.
        /// </summary>
        EPOLLWRNORM     = 0x00000100,
        /// <summary>
        /// Priority data band may be written.
        /// </summary>
        EPOLLWRBAND     = 0x00000200,
        /// <summary>
        /// Ignored (Do not use).
        /// </summary>
        EPOLLMSG        = 0x00000400,
        /// <summary>
        /// Stream socket peer closed connection, or shut down writing half of connection.
        /// </summary>
        EPOLLRDHUP      = 0x00002000,
        /// <summary>
        /// Sets an exclusive wakeup mode for the epoll file descriptor that is being attached to the target file descriptor.
        /// </summary>
        EPOLLEXCLUSIVE  = 0x10000000,
        /// <summary>
        /// If EPOLLONESHOT and EPOLLET are clear and the process has the CAP_BLOCK_SUSPEND capability, then this will ensure that the system does not enter "suspend" or "hibernate" while this event is pending or being processed.
        /// </summary>
        EPOLLWAKEUP     = 0x20000000,
        /// <summary>
        /// Requests one-shot notification for the associated file descriptor. 
        /// This means that after an event is notified for the associated file descriptor then the file descriptor is disabled in the interest list and no other events will be reported for it.
        /// </summary>
        EPOLLONESHOT    = 0x40000000,
        /// <summary>
        /// Requests edge-triggered notification for the associated file descriptor. 
        /// The default behavior for epoll is level-triggered.
        /// </summary>
        EPOLLET         = 0x80000000,
    }
}