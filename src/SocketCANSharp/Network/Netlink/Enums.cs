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

namespace SocketCANSharp.Network.Netlink
{
    /// <summary>
    /// Protocols of the PF_NETLINK protocol family.
    /// </summary>
    public enum NetlinkProtocolType
    {
        /// <summary>
        /// Netlink protocol entry which provides routing and link information.
        /// </summary>
        NETLINK_ROUTE = 0,
    }

    /// <summary>
    /// Netlink Message Types.
    /// </summary>
    public enum NetlinkMessageType : ushort
    {
        /// <summary>
        /// Nothing - Ignore this message.
        /// </summary>
        NLMSG_NOOP      = 1,
        /// <summary>
        /// Error messsage. 
        /// </summary>
        NLMSG_ERROR     = 2,
        /// <summary>
        /// End of multipart message.
        /// </summary>
        NLMSG_DONE      = 3,
        /// <summary>
        /// Overrun notification (error).
        /// </summary>
        NLMSG_OVERRUN   = 4,
        /// <summary>
        /// Create network interface.
        /// </summary>
        RTM_NEWLINK	    = 16,
        /// <summary>
        /// Remove network interface.
        /// </summary>
	    RTM_DELLINK     = 17,
        /// <summary>
        /// Get information about a network interface.
        /// </summary>
	    RTM_GETLINK     = 18,
        /// <summary>
        /// Set information on a network interface.
        /// </summary>
	    RTM_SETLINK     = 19,
        /// <summary>
        /// Create a network route.
        /// </summary>
        RTM_NEWROUTE    = 24,
        /// <summary>
        /// Remove a network route.
        /// </summary>
	    RTM_DELROUTE    = 25,
        /// <summary>
        /// Receive information about a network route.
        /// </summary>
	    RTM_GETROUTE    = 26,
    }

    /// <summary>
    /// Netlink Message Flags.
    /// </summary>
    [Flags]
    public enum NetlinkMessageFlags : ushort
    {
        /// <summary>
        /// Message is a Request Message.
        /// </summary>
        NLM_F_REQUEST   = 0x0001,
        /// <summary>
        /// Multi-part message which shall be terminated by a NLMSG_DONE message.
        /// </summary>
        NLM_F_MULTI     = 0X0002,
        /// <summary>
        /// Request an acknowledgement from the receiver.
        /// </summary>
        NLM_F_ACK       = 0x0004,
        /// <summary>
        /// Get Tree Root of Network Interfaces.
        /// </summary>
        NLM_F_ROOT      = 0x0100,
        /// <summary>
        /// Get All Matching Network Interfaces.
        /// </summary>
        NLM_F_MATCH     = 0x0200,
        /// <summary>
        /// Dump all network interface information.
        /// </summary>
        NLM_F_DUMP      = NLM_F_ROOT | NLM_F_MATCH,
    }

    /// <summary>
    /// ARP Protocol Hardware Identifier.
    /// </summary>
    public enum ArpHardwareIdentifier : ushort
    {
        /// <summary>
        /// Controller Area Network
        /// </summary>
        ARPHRD_CAN  = 280
    }

    /// <summary>
    /// Device Flags
    /// </summary>
    [Flags]
    public enum NetDeviceFlags : uint
    {
        /// <summary>
        /// Interface is up. System admin has marked link as active. 
        /// </summary>
        IFF_UP              = 0x00000001,
        /// <summary>
        /// Interface supports broadcasting with a valid broadcast address set.
        /// </summary>
        IFF_BROADCAST       = 0x00000002,
        /// <summary>
        /// Invokes driver to perform debugging.
        /// </summary>
        IFF_DEBUG           = 0x00000004,
        /// <summary>
        /// Interface is a Loopback Link.
        /// </summary>
        IFF_LOOPBACK        = 0x00000008,
        /// <summary>
        /// Interface is a Point-to-Point Link.
        /// </summary>
        IFF_POINTOPOINT     = 0x00000010,
        /// <summary>
        /// Don't use trailers.
        /// </summary>
        IFF_NOTRAILERS      = 0x00000020,
        /// <summary>
        /// Interface is up and carrier is OK (RFC2863 Operational).
        /// </summary>
        IFF_RUNNING         = 0x00000040,
        /// <summary>
        /// Interface does not support ARP.
        /// </summary>
        IFF_NOARP           = 0x00000080,
        /// <summary>
        /// Interface is in promiscuous mode.
        /// </summary>
        IFF_PROMISC         = 0x00000100,
        /// <summary>
        /// Interface supports multicast routing.
        /// </summary>
        IFF_ALLMULTI        = 0x00000200,
        /// <summary>
        /// Master of a load balancer.
        /// </summary>
        IFF_MASTER          = 0x00000400,
        /// <summary>
        /// Slave of a load balancer.
        /// </summary>
        IFF_SLAVE           = 0x00000800,
        /// <summary>
        /// Interface supports multicasting.
        /// </summary>
        IFF_MULTICAST       = 0x00001000,
        /// <summary>
        /// Driver supports setting media type.
        /// </summary>
        IFF_PORTSEL         = 0x00002000,
        /// <summary>
        /// Link selects media automatically. 
        /// </summary>
        IFF_AUTOMEDIA       = 0x00004000,
        /// <summary>
        /// Addresses are lost whenever the interface goes down.
        /// </summary>
        IFF_DYNAMIC         = 0x00008000,
        /// <summary>
        /// Link layer is operational.
        /// </summary>
        IFF_LOWER_UP        = 0x00010000,
        /// <summary>
        /// Driver signals dormant.
        /// </summary>
        IFF_DORMANT         = 0x00020000,
        /// <summary>
        /// Echo sent packets.
        /// </summary>
        IFF_ECHO            = 0x00040000,
    }

    /// <summary>
    /// Interface Link Attribute Type.
    /// </summary>
    public enum InterfaceLinkAttributeType
    {
        /// <summary>
        /// Unspecified Interface Link Attribute.
        /// </summary>
        IFLA_UNSPEC,
        /// <summary>
        /// Hardware Address.
        /// </summary>
        IFLA_ADDRESS,
        /// <summary>
        /// Hardware Broadcast Address.
        /// </summary>
        IFLA_BROADCAST,
        /// <summary>
        /// Interface Name.
        /// </summary>
        IFLA_IFNAME,
        /// <summary>
        /// Interface Maximum Transmission Unit (MTU).
        /// </summary>
        IFLA_MTU,
        /// <summary>
        /// Interface Index.
        /// </summary>
        IFLA_LINK,
        /// <summary>
        /// Queueing discipline.
        /// </summary>
        IFLA_QDISC,
        /// <summary>
        /// Interface statistics.
        /// </summary>
        IFLA_STATS,
        /// <summary>
        /// Cost.
        /// </summary>
        IFLA_COST,
        /// <summary>
        /// Priority.
        /// </summary>
        IFLA_PRIORITY,
        /// <summary>
        /// Master Interface Index.
        /// </summary>
        IFLA_MASTER,
        /// <summary>
        /// Wireless Extension event.
        /// </summary>
        IFLA_WIRELESS,
        /// <summary>
        /// Protocol specific information for a link.
        /// </summary>
        IFLA_PROTINFO,
        /// <summary>
        /// Transmit Queue Length.
        /// </summary>
        IFLA_TXQLEN,
        /// <summary>
        /// Interface map which represents hardware parameters of the corresponding device. Dependent on device driver and architecture.
        /// </summary>
        IFLA_MAP,
        /// <summary>
        /// Interface Link Weight.
        /// </summary>
        IFLA_WEIGHT,
        /// <summary>
        /// The RFC2863 state of the interface.
        /// </summary>
        IFLA_OPERSTATE,
        /// <summary>
        /// Link Policy.
        /// </summary>
        IFLA_LINKMODE,
        /// <summary>
        /// Network Interface Information.
        /// </summary>
        IFLA_LINKINFO,
        /// <summary>
        /// Network Namespace Process ID (PID).
        /// </summary>
        IFLA_NET_NS_PID,
        /// <summary>
        /// Interface Alias of a device.
        /// </summary>
        IFLA_IFALIAS,
        /// <summary>
        /// Number of Virtual Functions (VFs) if device is SR-IOV PF
        /// </summary>
        IFLA_NUM_VF,
        /// <summary>
        /// List of Virtual Function (VF) information.
        /// </summary>
        IFLA_VFINFO_LIST,
        /// <summary>
        /// Main device statistics structure (64 bit fields).
        /// </summary>
        IFLA_STATS64,
        /// <summary>
        /// Virtual Function (VF) Ports.
        /// </summary>
        IFLA_VF_PORTS,
        /// <summary>
        /// Ports.
        /// </summary>
        IFLA_PORT_SELF,
        /// <summary>
        /// Address Family Specific Configuration.
        /// </summary>
        IFLA_AF_SPEC,
        /// <summary>
        /// Group the network interface device belongs to.
        /// </summary>
        IFLA_GROUP,
        /// <summary>
        /// Network Namespace File Descriptor (FD).
        /// </summary>
        IFLA_NET_NS_FD,
        /// <summary>
        /// Extended information mask for Virtual Functions (VFs), etc.
        /// </summary>
        IFLA_EXT_MASK,
        /// <summary>
        /// Promiscuity count. When greater than 0 means network interface acts in Promiscuous Mode.
        /// </summary>
        IFLA_PROMISCUITY, 
        /// <summary>
        /// Number of Transmit Queues.
        /// </summary>
        IFLA_NUM_TX_QUEUES,
        /// <summary>
        /// Number of Receive Queues.
        /// </summary>
        IFLA_NUM_RX_QUEUES,
        /// <summary>
        /// Carrier (Lower Layer) Status Information.
        /// </summary>
        IFLA_CARRIER,
        /// <summary>
        /// Physical Port ID.
        /// </summary>
        IFLA_PHYS_PORT_ID,
        /// <summary>
        /// Carrier Up and Down Counters Combined.
        /// </summary>
        IFLA_CARRIER_CHANGES,
        /// <summary>
        /// Physical Switch ID.
        /// </summary>
        IFLA_PHYS_SWITCH_ID,
        /// <summary>
        /// Network Namespace ID.
        /// </summary>
        IFLA_LINK_NETNSID,
        /// <summary>
        /// Physical Port Name.
        /// </summary>
        IFLA_PHYS_PORT_NAME,
        /// <summary>
        /// Protocol port state.
        /// </summary>
        IFLA_PROTO_DOWN,
        /// <summary>
        /// Maximum number of segments that can be passed to the NIC for Generic Segmentation Offload (GSO).
        /// </summary>
        IFLA_GSO_MAX_SEGS,
        /// <summary>
        /// Maximum size of Generic Segmentation Offload (GSO) packet.
        /// </summary>
        IFLA_GSO_MAX_SIZE,
        /// <summary>
        /// Attribute Type for Padding.
        /// </summary>
        IFLA_PAD,
        /// <summary>
        /// Express Data Path (XDP) information.
        /// </summary>
        IFLA_XDP,
        /// <summary>
        /// Interface Link Event.
        /// </summary>
        IFLA_EVENT,
        /// <summary>
        /// New Network Namespace ID.
        /// </summary>
        IFLA_NEW_NETNSID,
        /// <summary>
        /// Target Network Namespace ID.
        /// </summary>
        IFLA_IF_NETNSID,
        /// <summary>
        /// Alias for Target Network Namespace ID.
        /// </summary>
        IFLA_TARGET_NETNSID = IFLA_IF_NETNSID,
        /// <summary>
        /// Carrier Up Counter.
        /// </summary>
        IFLA_CARRIER_UP_COUNT,
        /// <summary>
        /// Carrier Down Counter.
        /// </summary>
        IFLA_CARRIER_DOWN_COUNT,
        /// <summary>
        /// New Interface Index.
        /// </summary>
        IFLA_NEW_IFINDEX,
        /// <summary>
        /// Minimum value for the Interface Maximum Transmission Unit (MTU).
        /// </summary>
        IFLA_MIN_MTU,
        /// <summary>
        /// Maximum value for the Interface Maximum Transmission Unit (MTU).
        /// </summary>
        IFLA_MAX_MTU,
        /// <summary>
        /// Interface Property List.
        /// </summary>
        IFLA_PROP_LIST,
        /// <summary>
        /// Alternate Interface Name.
        /// </summary>
        IFLA_ALT_IFNAME,
        /// <summary>
        /// Permanent Hardware Address of the Interface.
        /// </summary>
        IFLA_PERM_ADDRESS,
        /// <summary>
        /// Reason for protocol port state.
        /// </summary>
        IFLA_PROTO_DOWN_REASON,
        /// <summary>
        /// Parent Device Name.
        /// </summary>
        IFLA_PARENT_DEV_NAME,
        /// <summary>
        /// Parent Device Bus Name.
        /// </summary>
        IFLA_PARENT_DEV_BUS_NAME,
        /// <summary>
        /// Maximum size of Generic Receive Offload (GRO) packet.
        /// </summary>
        IFLA_GRO_MAX_SIZE,
        /// <summary>
        /// Maximum size of TCP Segmentation Offload (TSO) packet.
        /// </summary>
        IFLA_TSO_MAX_SIZE,
        /// <summary>
        /// Maximum number of segments that can be passed to the NIC for TCP Segmentation Offload (TSO).
        /// </summary>
        IFLA_TSO_MAX_SEGS,
        /// <summary>
        /// Counter which enables or disables allmulticast mode.
        /// </summary>
        IFLA_ALLMULTI,		
    }

    /// <summary>
    /// The RFC2863 state of the interface.
    /// </summary>
    public enum InterfaceOperationalStatus
    {
        /// <summary>
        /// Interface is in an unknown state, neither driver nor userspace has set the operational state.
        /// Note: Not all drivers have implemented setting operational state.
        /// </summary>
        IF_OPER_UNKNOWN,
        /// <summary>
        /// Currently unused state in the kernel. Interfaces that are not present are usually not visible.
        /// </summary>
        IF_OPER_NOTPRESENT,
        /// <summary>
        /// Interface is unable to transfer data to Layer 1 (Physical Layer). Typically, this means that either the interface is unplugged or system admin has placed it into down state.
        /// </summary>
        IF_OPER_DOWN,
        /// <summary>
        /// Interface is stacked on an interface that is IF_OPER_DOWN are in this state. 
        /// </summary>
        IF_OPER_LOWERLAYERDOWN,
        /// <summary>
        /// Interface is in testing mode. For example, executing driver self-test or media (cable) test.
        /// </summary>
        IF_OPER_TESTING,
        /// <summary>
        /// Interface has Layer 1 (Physical Layer) up, but is waiting for an external event to occur.
        /// </summary>
        IF_OPER_DORMANT,
        /// <summary>
        /// Interface is operationally up and can be utilized. 
        /// </summary>
        IF_OPER_UP,
    }

    /// <summary>
    /// Interface Link Mode.
    /// </summary>
    public enum InterfaceLinkMode
    {
        /// <summary>
        /// Default Mode.
        /// </summary>
        IF_LINK_MODE_DEFAULT,
        /// <summary>
        /// Dormant Mode.
        /// </summary>
        IF_LINK_MODE_DORMANT,
        /// <summary>
        /// Testing Mode.
        /// </summary>
        IF_LINK_MODE_TESTING,
    }

    /// <summary>
    /// Interface Link Information Attribute Type. Nested in IFLA_LINKINFO Interface Link Attribute Type.
    /// </summary>
    public enum LinkInfoAttributeType 
    {
        /// <summary>
        /// Unspecified.
        /// </summary>
        IFLA_INFO_UNSPEC,
        /// <summary>
        /// Link Kind.
        /// </summary>
        IFLA_INFO_KIND,
        /// <summary>
        /// Link Data.
        /// </summary>
        IFLA_INFO_DATA,
        /// <summary>
        /// Link Statistics.
        /// </summary>
        IFLA_INFO_XSTATS,
        /// <summary>
        /// Link Slave Kind.
        /// </summary>
        IFLA_INFO_SLAVE_KIND,
        /// <summary>
        /// Link Slave Data.
        /// </summary>
        IFLA_INFO_SLAVE_DATA,
    }

    /// <summary>
    /// CAN Routing Attribute Types. Nested in IFLA_INFO_DATA Interface Link Information Attribute Type.
    /// </summary>
    public enum CanRoutingAttributeType
    {
        /// <summary>
        /// Unspecified.
        /// </summary>
        IFLA_CAN_UNSPEC,
        /// <summary>
        /// CAN Bit Timing Parameters.
        /// </summary>
        IFLA_CAN_BITTIMING,
        /// <summary>
        /// CAN hardware-dependent bit timing constant.
        /// </summary>
        IFLA_CAN_BITTIMING_CONST,
        /// <summary>
        /// CAN clock parameters.
        /// </summary>
        IFLA_CAN_CLOCK,
        /// <summary>
        /// CAN operational or error state.
        /// </summary>
        IFLA_CAN_STATE,
        /// <summary>
        /// CAN Controller Mode.
        /// </summary>
        IFLA_CAN_CTRLMODE,
        /// <summary>
        /// CAN Restart Delay.
        /// </summary>
        IFLA_CAN_RESTART_MS,
        /// <summary>
        /// CAN Restart.
        /// </summary>
        IFLA_CAN_RESTART,
        /// <summary>
        /// CAN Bus Error Counter Information.
        /// </summary>
        IFLA_CAN_BERR_COUNTER,
        /// <summary>
        /// CAN Bit Timing Parameters in Data Phase.
        /// </summary>
        IFLA_CAN_DATA_BITTIMING,
        /// <summary>
        /// CAN hardware-dependent bit timing constant in Data Phase.
        /// </summary>
        IFLA_CAN_DATA_BITTIMING_CONST,
        /// <summary>
        /// CAN Termination Value.
        /// </summary>
        IFLA_CAN_TERMINATION,
        /// <summary>
        /// CAN Termination Constant.
        /// </summary>
        IFLA_CAN_TERMINATION_CONST,
        /// <summary>
        /// CAN Bit Rate Constant.
        /// </summary>
        IFLA_CAN_BITRATE_CONST,
        /// <summary>
        /// CAN Bit Rate Constant in Data Phase.
        /// </summary>
        IFLA_CAN_DATA_BITRATE_CONST,
        /// <summary>
        /// CAN Bit Rate Maximum.
        /// </summary>
        IFLA_CAN_BITRATE_MAX,
        /// <summary>
        /// CAN FD Transmitter Delay Compensation (TDC).
        /// </summary>
        IFLA_CAN_TDC,
        /// <summary>
        /// CAN Controller Mode Extended. Options that can be modified by Netlink.
        /// </summary>
        IFLA_CAN_CTRLMODE_EXT,
    }

    /// <summary>
    /// CAN operational or error state.
    /// </summary>
    public enum CanState
    {
        /// <summary>
        /// Error Active: RX/TX error count less than 96.
        /// </summary>
        CAN_STATE_ERROR_ACTIVE,
        /// <summary>
        /// Error Warning: RX/TX error count less than 128.
        /// </summary>
        CAN_STATE_ERROR_WARNING,
        /// <summary>
        /// Error Passive: RX/TX error count less than 256.
        /// </summary>
        CAN_STATE_ERROR_PASSIVE,
        /// <summary>
        /// Bus Off: RX/TX error count greater than or equal to 256.
        /// </summary>
        CAN_STATE_BUS_OFF,
        /// <summary>
        /// CAN Device is stopped.
        /// </summary>
        CAN_STATE_STOPPED,
        /// <summary>
        /// CAN Device is sleeping.
        /// </summary>
        CAN_STATE_SLEEPING,
    }

    /// <summary>
    /// CAN Controller Mode Flags.
    /// </summary>
    [Flags]
    public enum CanControllerModeFlags : uint
    {
        /// <summary>
        /// Loopback mode.
        /// </summary>
        CAN_CTRLMODE_LOOPBACK               = 0x00000001,
        /// <summary>
        /// Listen-only mode.
        /// </summary>
        CAN_CTRLMODE_LISTENONLY             = 0x00000002,
        /// <summary>
        /// Triple sampling mode.
        /// </summary>
        CAN_CTRLMODE_3_SAMPLES              = 0x00000004,
        /// <summary>
        /// One-Shot mode.
        /// </summary>
        CAN_CTRLMODE_ONE_SHOT               = 0x00000008,
        /// <summary>
        /// Bus-error reporting.
        /// </summary>
        CAN_CTRLMODE_BERR_REPORTING         = 0x00000010,
        /// <summary>
        /// CAN FD mode.
        /// </summary>
        CAN_CTRLMODE_FD                     = 0x00000020,
        /// <summary>
        /// Ignore missing CAN ACKs.
        /// </summary>
        CAN_CTRLMODE_PRESUME_ACK            = 0x00000040,
        /// <summary>
        /// CAN FD in non-ISO mode.
        /// </summary>
        CAN_CTRLMODE_FD_NON_ISO             = 0x00000080,
        /// <summary>
        /// Classic CAN DLC option.
        /// </summary>
        CAN_CTRLMODE_CC_LEN8_DLC            = 0x00000100,
        /// <summary>
        /// CAN transiver automatically calculates TDCV.
        /// </summary>
        CAN_CTRLMODE_TDC_AUTO               = 0x00000200,
        /// <summary>
        /// TDCV is manually set up by user.
        /// </summary>
        CAN_CTRLMODE_TDC_MANUAL             = 0x00000400,
    }
}