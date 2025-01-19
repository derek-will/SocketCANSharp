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
using System.Text;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using SocketCANSharp.Network.Netlink;

namespace SocketCANSharp.Network
{
    /// <summary>
    /// Provides information about a CAN interface.
    /// </summary>
    public class CanNetworkInterface
    {
        private const string CanInterfaceStartsWith = "can";
        private const string VirtualCanInterfaceStartsWith = "vcan";

        /// <summary>
        /// Index of the CAN interface.
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// Name of the CAN interface.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Indicates if the CAN interface is virtual or not.
        /// </summary>
        public bool IsVirtual { get; set; }

        /// <summary>
        /// Device Flags.
        /// </summary>
        public NetDeviceFlags DeviceFlags
        {
            get
            {
                InterfaceInfoMessage? msg = GetInterfaceInfoMessage();
                return msg.HasValue ? msg.Value.DeviceFlags : 0;
            }
        }

        /// <summary>
        /// Device Type.
        /// </summary>
        public ArpHardwareIdentifier DeviceType
        {
            get
            {
                InterfaceInfoMessage? msg = GetInterfaceInfoMessage();
                return msg.HasValue ? msg.Value.DeviceType : 0;
            }
        }

        /// <summary>
        /// Bit Timing of the CAN Network Interface.
        /// Note: The caller must have CAP_NET_ADMIN capability for the setting of this property to be successful.
        /// </summary>
        public CanBitTiming BitTiming
        {
            get
            {
                CanRoutingAttribute cbtAttr = GetCanRoutingAttribute(CanRoutingAttributeType.IFLA_CAN_BITTIMING);
                return cbtAttr == null ? null : CanBitTiming.FromBytes(cbtAttr.Data);
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value), "Cannot set Bit Timing to null.");

                var canDevProps = new CanDeviceProperties()
                {
                    BitTiming = value,
                };

                SetLinkInfo(canDevProps);
            }
        }

        /// <summary>
        /// Hardware-dependent Bit Timing of the CAN Network Interface.
        /// </summary>
        public CanBitTimingConstant BitTimingConstant
        {
            get
            {
                CanRoutingAttribute cbtcAttr = GetCanRoutingAttribute(CanRoutingAttributeType.IFLA_CAN_BITTIMING_CONST);
                return cbtcAttr == null ? null : CanBitTimingConstant.FromBytes(cbtcAttr.Data);
            }
        }

        /// <summary>
        /// Data Phase Bit Timing of the CAN Network Interface.
        /// Note: The caller must have CAP_NET_ADMIN capability for the setting of this property to be successful.
        /// </summary>
        public CanBitTiming DataPhaseBitTiming
        {
            get
            {
                CanRoutingAttribute cbtAttr = GetCanRoutingAttribute(CanRoutingAttributeType.IFLA_CAN_DATA_BITTIMING);
                return cbtAttr == null ? null : CanBitTiming.FromBytes(cbtAttr.Data);
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value), "Cannot set Data Phase Bit Timing to null.");

                var canDevProps = new CanDeviceProperties()
                {
                    DataPhaseBitTiming = value,
                };

                SetLinkInfo(canDevProps);
            }
        }

        /// <summary>
        /// Hardware-dependent Data Phase Bit Timing of the CAN Network Interface.
        /// </summary>
        public CanBitTimingConstant DataPhaseBitTimingConstant
        {
            get
            {
                CanRoutingAttribute cbtcAttr = GetCanRoutingAttribute(CanRoutingAttributeType.IFLA_CAN_DATA_BITTIMING_CONST);
                return cbtcAttr == null ? null : CanBitTimingConstant.FromBytes(cbtcAttr.Data);
            }
        }

        /// <summary>
        /// The current state of the CAN controller.
        /// </summary>
        public CanState? CanControllerState
        {
            get
            {
                CanRoutingAttribute cbtcAttr = GetCanRoutingAttribute(CanRoutingAttributeType.IFLA_CAN_STATE);
                return cbtcAttr == null ? (CanState?)null : (CanState)BitConverter.ToUInt32(cbtcAttr.Data, 0);;
            }
        }

        /// <summary>
        /// CAN Controller Mode Flags of the CAN Network Interface.
        /// Note: The caller must have CAP_NET_ADMIN capability for the setting of this property to be successful.
        /// </summary>
        public CanControllerModeFlags CanControllerModeFlags
        {
            get
            {
                CanRoutingAttribute cbtAttr = GetCanRoutingAttribute(CanRoutingAttributeType.IFLA_CAN_CTRLMODE);
                return cbtAttr == null ? 0 : CanControllerMode.FromBytes(cbtAttr.Data).Flags;
            }
            set
            {
                var canDevProps = new CanDeviceProperties()
                {
                    ControllerMode = new CanControllerMode()
                    {
                        Mask = 0xffffffff,
                        Flags = value,
                    },
                };

                SetLinkInfo(canDevProps);
            }
        }

        /// <summary>
        /// CAN controller automatic restart delay time in milliseconds. An automatic restart of the CAN controller 
        /// will be triggered in the event of a BUS OFF state after the specified delay time. Set to 0 to disable auto-restart. 
        /// Note: The caller must have CAP_NET_ADMIN capability for the setting of this property to be successful.
        /// </summary>
        public uint? AutoRestartDelay
        {
            get
            {
                CanRoutingAttribute cbtAttr = GetCanRoutingAttribute(CanRoutingAttributeType.IFLA_CAN_RESTART_MS);
                return cbtAttr == null ? (uint?)null : BitConverter.ToUInt32(cbtAttr.Data, 0);
            }
            set
            {
                if (value.HasValue == false)
                    throw new ArgumentNullException(nameof(value), "Auto-Restart delay cannot be set to null.");

                var canDevProps = new CanDeviceProperties()
                {
                    RestartDelay = value,
                };

                SetLinkInfo(canDevProps);
            }
        }

        /// <summary>
        /// CAN termination resistance value.
        /// Note: The caller must have CAP_NET_ADMIN capability for the setting of this property to be successful.
        /// </summary>
        public ushort? TerminationResistance
        {
            get
            {
                CanRoutingAttribute cbtAttr = GetCanRoutingAttribute(CanRoutingAttributeType.IFLA_CAN_TERMINATION);
                return cbtAttr == null ? (ushort?)null : BitConverter.ToUInt16(cbtAttr.Data, 0);
            }
            set
            {
                if (value.HasValue == false)
                    throw new ArgumentNullException(nameof(value), "Termination Resistance cannot be set to null.");

                var canDevProps = new CanDeviceProperties()
                {
                    TerminationResistance = value,
                };

                SetLinkInfo(canDevProps);
            }
        }

        /// <summary>
        /// Operational Status of the Interface (RFC2863 State).
        /// </summary>
        public InterfaceOperationalStatus OperationalStatus
        {
            get
            {
                InterfaceLinkAttribute operStateAttr = GetInterfaceLinkAttribute(InterfaceLinkAttributeType.IFLA_OPERSTATE);
                return operStateAttr == null ? InterfaceOperationalStatus.IF_OPER_UNKNOWN : (InterfaceOperationalStatus)operStateAttr.Data[0];
            }
        }

        /// <summary>
        /// Link Statistics.
        /// </summary>
        public InterfaceLinkStatistics64 LinkStatistics
        {
            get
            {
                InterfaceLinkAttribute stats64Attr = GetInterfaceLinkAttribute(InterfaceLinkAttributeType.IFLA_STATS64);
                return stats64Attr == null ? null : InterfaceLinkStatistics64.FromBytes(stats64Attr.Data);
            }
        }

        /// <summary>
        /// Maximum Transmission Unit (MTU) of the interface.
        /// </summary>
        public uint? MaximumTransmissionUnit
        {
            get
            {
                InterfaceLinkAttribute mtuAttr = GetInterfaceLinkAttribute(InterfaceLinkAttributeType.IFLA_MTU);
                return mtuAttr == null ? (uint?)null : BitConverter.ToUInt32(mtuAttr.Data, 0);
            }
        }

        /// <summary>
        /// Link Kind (i.e., can or vcan).
        /// </summary>
        public string LinkKind
        {
            get
            {
                LinkInfoAttribute kindAttr = GetLinkInfoAttribute(LinkInfoAttributeType.IFLA_INFO_KIND);
                return kindAttr == null ? null : Encoding.ASCII.GetString(kindAttr.Data).Trim('\0');
            }
        }

        /// <summary>
        /// CAN Device Statistics.
        /// </summary>
        public CanDeviceStatistics DeviceStatistics
        {
            get
            {
                LinkInfoAttribute deviceStatsAttr = GetLinkInfoAttribute(LinkInfoAttributeType.IFLA_INFO_XSTATS);
                return deviceStatsAttr == null ? null : CanDeviceStatistics.FromBytes(deviceStatsAttr.Data);
            }
        }

        /// <summary>
        /// Initializes a new instance of the CanNetworkInterface class with the specified Index, Name and whether the interface is virtual or physical.
        /// </summary>
        /// <param name="index">Interface index</param>
        /// <param name="name">Interface name</param>
        /// <param name="isVirtual">If true, the interface is virtual. If false, the interface is physical.</param>
        public CanNetworkInterface(int index, string name, bool isVirtual)
        {
            Index = index;
            Name = name;
            IsVirtual = isVirtual;
        }

        /// <summary>
        /// Brings the link up.
        /// Note: The caller must have CAP_NET_ADMIN capability for this to be successful.
        /// </summary>
        public void SetLinkUp()
        {
            InterfaceStateChange(true);
        }

        /// <summary>
        /// Brings the link down.
        /// Note: The caller must have CAP_NET_ADMIN capability for this to be successful.
        /// </summary>
        public void SetLinkDown()
        {
            InterfaceStateChange(false);
        }
         
        /// <summary>
        /// Restart the CAN controller.
        /// Note #1: This is only possible when the interface is running, Auto-Restart is disabled (delay is set to 0) and the device is in BUS OFF state. 
        /// Note #2: The caller must have CAP_NET_ADMIN capability for this to be successful.
        /// </summary>
        public void Restart()
        {
            var canDevProps = new CanDeviceProperties()
            {
                TriggerRestart = true,
            };

            SetLinkInfo(canDevProps);
        }

        /// <summary>
        /// Retrieves all CAN interfaces connected on the local system and optionally virtual interfaces as well.
        /// </summary>
        /// <param name="includeVirtual">Indicates whether virtual interfaces should be included or not.</param>
        /// <returns>Collection of CAN interfaces on the local system</returns>
        public static IEnumerable<CanNetworkInterface> GetAllInterfaces(bool includeVirtual)
        {
            IntPtr ptr = LibcNativeMethods.IfNameIndex();
            if (ptr == IntPtr.Zero)
            {
                throw new NetworkInformationException(LibcNativeMethods.Errno);
            }

            var ifList = new List<CanNetworkInterface>();
            IntPtr iteratorPtr = ptr;
            try
            {            
                IfNameIndex i = Marshal.PtrToStructure<IfNameIndex>(iteratorPtr);
                while (i.Index != 0 && i.Name != null)
                {
                    if (i.Name != null)
                    {
                        if (i.Name.StartsWith(CanInterfaceStartsWith))
                        {
                            var canInterface = new CanNetworkInterface((int)i.Index, i.Name, false);
                            ifList.Add(canInterface);
                        }

                        if (includeVirtual && i.Name.StartsWith(VirtualCanInterfaceStartsWith))
                        {
                            var canInterface = new CanNetworkInterface((int)i.Index, i.Name, true);
                            ifList.Add(canInterface);
                        }
                    }

                    iteratorPtr = IntPtr.Add(iteratorPtr, Marshal.SizeOf(typeof(IfNameIndex)));
                    i = Marshal.PtrToStructure<IfNameIndex>(iteratorPtr);
                }
            }
            finally
            {
                LibcNativeMethods.IfFreeNameIndex(ptr);
            }

            return ifList;
        }

        /// <summary>
        /// Looks up and creates a CanNetworkInterface instance from the interface name.
        /// </summary>
        /// <param name="socketHandle">Socket Handle</param>
        /// <param name="interfaceName">Interface Name</param>
        /// <returns>CanNetworkInterface instance with the corresponding name.</returns>
        /// <exception cref="ArgumentNullException">Socket Handle is null.</exception>
        /// <exception cref="ArgumentException">Socket Handle is closed or invalid.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Interface Name is null, empty, or only whitespace characters.</exception>
        /// <exception cref="NetworkInformationException">Failed to look up interface by name.</exception>
        public static CanNetworkInterface GetInterfaceByName(SafeFileDescriptorHandle socketHandle, string interfaceName)
        {
            if (socketHandle == null)
                throw new ArgumentNullException(nameof(socketHandle));
            
            if (socketHandle.IsClosed || socketHandle.IsInvalid)
                throw new ArgumentException("Socket handle must be open and valid", nameof(socketHandle));

            if (string.IsNullOrWhiteSpace(interfaceName))
                throw new ArgumentOutOfRangeException("Interface Name can not by null, empty, or only whitespace characters.", nameof(interfaceName));

            var ifr = new Ifreq(interfaceName);
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
            if (ioctlResult == -1)
                throw new NetworkInformationException(LibcNativeMethods.Errno);

            return new CanNetworkInterface(ifr.IfIndex, ifr.Name, ifr.Name.StartsWith(VirtualCanInterfaceStartsWith));
        }

        /// <summary>
        /// Looks up and creates a CanNetworkInterface instance from the interface index.
        /// </summary>
        /// <param name="socketHandle">Socket Handle</param>
        /// <param name="interfaceIndex">Interface Index</param>
        /// <returns>CanNetworkInterface instance with the corresponding index.</returns>
        /// <exception cref="ArgumentNullException">Socket Handle is null.</exception>
        /// <exception cref="ArgumentException">Socket Handle is closed or invalid.</exception>
        /// <exception cref="NetworkInformationException">Failed to look up interface by index.</exception>
        public static CanNetworkInterface GetInterfaceByIndex(SafeFileDescriptorHandle socketHandle, int interfaceIndex)
        {
            if (socketHandle == null)
                throw new ArgumentNullException(nameof(socketHandle));
            
            if (socketHandle.IsClosed || socketHandle.IsInvalid)
                throw new ArgumentException("Socket handle must be open and valid", nameof(socketHandle));

            var ifr = new Ifreq(interfaceIndex);
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFNAME, ifr);
            if (ioctlResult == -1)
                throw new NetworkInformationException(LibcNativeMethods.Errno);

            return new CanNetworkInterface(ifr.IfIndex, ifr.Name, ifr.Name.StartsWith(VirtualCanInterfaceStartsWith));
        }

        /// <summary>
        /// Retreives the Maximum Transmission Unit (MTU) of the interface.
        /// </summary>
        /// <param name="socketHandle">Socket Handle</param>
        /// <returns>Maximum Transmission Unit of the interface.</returns>
        /// <exception cref="NetworkInformationException">Unable to retreive MTU size information for the interface.</exception>
        [Obsolete("ReadSupportedMtu method is deprecated, please use MaximumTransmissionUnit property instead.")]
        public int ReadSupportedMtu(SafeFileDescriptorHandle socketHandle)
        {
            if (socketHandle == null)
                throw new ArgumentNullException(nameof(socketHandle));
            
            if (socketHandle.IsClosed || socketHandle.IsInvalid)
                throw new ArgumentException("Socket handle must be open and valid", nameof(socketHandle));

            var ifr = new IfreqMtu(Name);
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFMTU, ifr);
            if (ioctlResult == -1)
                throw new NetworkInformationException(LibcNativeMethods.Errno);

            return ifr.MTU;
        }

        /// <summary>
        /// Returns a string that represents the current CanNetworkInterface object.
        /// </summary>
        /// <returns>A string that represents the current CanNetworkInterface object.</returns>
        public override string ToString()
        {
            return $"Index: {Index}; Name: {Name}; Is Virtual: {IsVirtual}";
        }
        
        private byte[] GetLinkInfo()
        {
            using (var rtNetlinkSocket = new RoutingNetlinkSocket())
            {
                rtNetlinkSocket.ReceiveBufferSize = 32768;
                rtNetlinkSocket.SendBufferSize = 32768;
                rtNetlinkSocket.ReceiveTimeout = 1000;
                rtNetlinkSocket.Bind(new SockAddrNetlink(0, 0));
                
                NetworkInterfaceInfoRequest req = NetlinkUtils.GenerateRequestForLinkInfoByIndex(Index);
                int numBytes = rtNetlinkSocket.Write(req);

                byte[] rxBuffer = new byte[8192];
                int bytesRead = rtNetlinkSocket.Read(rxBuffer);

                return rxBuffer.Take(bytesRead).ToArray();
            }
        }

        private void SetLinkInfo(CanDeviceProperties canDevProps)
        {
            using (var rtNetlinkSocket = new RoutingNetlinkSocket())
            {
                rtNetlinkSocket.ReceiveBufferSize = 32768;
                rtNetlinkSocket.SendBufferSize = 32768;
                rtNetlinkSocket.ReceiveTimeout = 1000;
                rtNetlinkSocket.Bind(new SockAddrNetlink(0, 0));
                canDevProps.LinkKind = LinkKind;

                NetworkInterfaceModifierRequest req = NetlinkUtils.GenerateRequestForLinkModifierByIndex(Index, canDevProps, null);
                int bytesWritten = rtNetlinkSocket.Write(req);

                byte[] rxBuffer = new byte[8192];
                int numBytes = rtNetlinkSocket.Read(rxBuffer);
                if (numBytes < NetlinkMessageMacros.NLMSG_HDRLEN)
                    throw new SocketCanException("Read insufficient bytes from the rtnetlink socket for a Netlink message header.");

                NetlinkMessageHeader msgHeader = NetlinkMessageHeader.FromBytes(rxBuffer);
                if (msgHeader.MessageType != NetlinkMessageType.NLMSG_ERROR)
                    throw new SocketCanException($"Unexpected Netlink message received: {msgHeader.MessageType}.");

                if (numBytes < Marshal.SizeOf<NetlinkMessageError>())
                    throw new SocketCanException("Read insufficient bytes from the rtnetlink socket for a NLMSG_ERROR message.");

                NetlinkMessageError nlMsgErr = NetlinkMessageError.FromBytes(rxBuffer);    
                if (nlMsgErr.Error != 0)
                    throw new SocketCanException(-1 * nlMsgErr.Error, $"Errno Set on Netlink Response.");
            }
        }

        private void InterfaceStateChange(bool interfaceUp)
        {
            using (var rtNetlinkSocket = new RoutingNetlinkSocket())
            {
                rtNetlinkSocket.ReceiveBufferSize = 32768;
                rtNetlinkSocket.SendBufferSize = 32768;
                rtNetlinkSocket.ReceiveTimeout = 1000;
                rtNetlinkSocket.Bind(new SockAddrNetlink(0, 0));

                NetworkInterfaceModifierRequest req = NetlinkUtils.GenerateRequestForLinkModifierByIndex(Index, null, interfaceUp);
                int bytesWritten = rtNetlinkSocket.Write(req);

                byte[] rxBuffer = new byte[8192];
                int numBytes = rtNetlinkSocket.Read(rxBuffer);
                if (numBytes < NetlinkMessageMacros.NLMSG_HDRLEN)
                    throw new SocketCanException("Read insufficient bytes from the rtnetlink socket for a Netlink message header.");

                NetlinkMessageHeader msgHeader = NetlinkMessageHeader.FromBytes(rxBuffer);
                if (msgHeader.MessageType != NetlinkMessageType.NLMSG_ERROR)
                    throw new SocketCanException($"Unexpected Netlink message received: {msgHeader.MessageType}.");

                if (numBytes < Marshal.SizeOf<NetlinkMessageError>())
                    throw new SocketCanException("Read insufficient bytes from the rtnetlink socket for a NLMSG_ERROR message.");

                NetlinkMessageError nlMsgErr = NetlinkMessageError.FromBytes(rxBuffer);    
                if (nlMsgErr.Error != 0)
                    throw new SocketCanException(-1 * nlMsgErr.Error, $"Errno Set on Netlink Response.");
            }
        }

        private CanRoutingAttribute GetCanRoutingAttribute(CanRoutingAttributeType type)
        {
            byte[] rxBuffer = GetLinkInfo();
            return NetlinkUtils.FindNestedCanRoutingAttribute(Index, rxBuffer, type);
        }

        private InterfaceLinkAttribute GetInterfaceLinkAttribute(InterfaceLinkAttributeType type)
        {
            byte[] rxBuffer = GetLinkInfo();
            return NetlinkUtils.FindInterfaceLinkAttribute(Index, rxBuffer, type);
        }

        private LinkInfoAttribute GetLinkInfoAttribute(LinkInfoAttributeType type)
        {
            byte[] rxBuffer = GetLinkInfo();
            return NetlinkUtils.FindNestedLinkInfoAttribute(Index, rxBuffer, type);
        }

        private InterfaceInfoMessage? GetInterfaceInfoMessage()
        {
            byte[] rxBuffer = GetLinkInfo();
            return NetlinkUtils.FindInterfaceInfoMessage(Index, rxBuffer);
        }
    }
}