#region License
/* 
BSD 3-Clause License

Copyright (c) 2024, Derek Will
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
using System.Linq;
using System.Collections.Generic;
using SocketCANSharp.Capabilities;
using SocketCANSharp.Network;
using SocketCANSharp.Network.Netlink;
using SocketCANSharp;

namespace CanNetlinkReader
{
    class Program
    {
        static void Main(string[] args)
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("can0"));
            if (iface == null)
            {
                Console.WriteLine("can0 interface not found and is required for this demo!");
                return;
            }

            if (CapabilityUtils.IsCurrentProcessCapable(Capability.CAP_NET_ADMIN) == false)
            {
                Console.WriteLine("Cannot run the demonstration as the current process lacks suffucient privileges (CAP_NET_ADMIN).");
                return;
            }

            using (var rtNetlinkSocket = new RoutingNetlinkSocket())
            {
                rtNetlinkSocket.ReceiveBufferSize = 32768;
                rtNetlinkSocket.SendBufferSize = 32768;
                rtNetlinkSocket.ReceiveTimeout = 1000;
                rtNetlinkSocket.Bind(new SockAddrNetlink(0, 0));
                Console.WriteLine("Stopping Interface...");
                SetInterfaceDown(rtNetlinkSocket, iface);
                Console.WriteLine("Configuring Interface...");
                SetDeviceProperties(rtNetlinkSocket, iface);
                Console.WriteLine("Starting Interface...");
                SetInterfaceUp(rtNetlinkSocket, iface);
            }

            Console.WriteLine("Using Interface class...");
            Console.WriteLine("Bringing Link Down...");
            iface.SetLinkDown();
            Console.WriteLine("Configuring...");
            iface.BitTiming = new CanBitTiming() { BitRate = 125000 };
            iface.CanControllerModeFlags = CanControllerModeFlags.CAN_CTRLMODE_LOOPBACK | CanControllerModeFlags.CAN_CTRLMODE_LISTENONLY;
            Console.WriteLine("Bringing Link Up...");
            iface.SetLinkUp();

            Console.WriteLine($"Bit Timing: {iface.BitTiming.BitRate}");
            Console.WriteLine($"Controller Mode Flags: {iface.CanControllerModeFlags}");
        }

        private static void SetDeviceProperties(RoutingNetlinkSocket rtNetlinkSocket, CanNetworkInterface iface)
        {
            var canDevProps = new CanDeviceProperties()
            {
                LinkKind = "can",
                RestartDelay = 100, // restart-ms 100
                BitTiming = new CanBitTiming()
                {
                    BitRate = 250000, // bitrate 250000 
                },
                ControllerMode = new CanControllerMode()
                {
                    Mask = 0xffffffff,
                    Flags = CanControllerModeFlags.CAN_CTRLMODE_LISTENONLY,
                },
            };

            NetworkInterfaceModifierRequest modReq = NetlinkUtils.GenerateRequestForLinkModifierByIndex(iface.Index, canDevProps, null);
            int bytesWritten = rtNetlinkSocket.Write(modReq);
            if (bytesWritten > 0)
            {
                byte[] rxBuffer = new byte[8192];
                int numBytes = rtNetlinkSocket.Read(rxBuffer);
                if (numBytes >= 36)
                {
                    NetlinkMessageError nlMsgErr = NetlinkMessageError.FromBytes(rxBuffer);
                    if (nlMsgErr.MessageHeader.MessageType == NetlinkMessageType.NLMSG_ERROR)
                    {
                        if (nlMsgErr.Error == 0) // Success
                        {
                            Console.WriteLine($"Bitrate: {iface.BitTiming.BitRate}");
                        }
                        else
                        {
                            Console.WriteLine($"Errno Set on Netlink Response: {-1 * nlMsgErr.Error}.");
                            return;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Unexpected Netlink message received: {nlMsgErr.MessageHeader.MessageType}.");
                        return;
                    }
                }
                else
                {
                    Console.WriteLine("Read insufficient bytes from the rtnetlink socket.");
                    return;
                }
            }
            else
            {
                Console.WriteLine($"Failed to write Netlink message: {LibcNativeMethods.Errno}.");
                return;
            }
        }

        private static void SetInterfaceDown(RoutingNetlinkSocket rtNetlinkSocket, CanNetworkInterface iface)
        {
            InterfaceStartStop(rtNetlinkSocket, iface, false);
        }

        private static void SetInterfaceUp(RoutingNetlinkSocket rtNetlinkSocket, CanNetworkInterface iface)
        {
            InterfaceStartStop(rtNetlinkSocket, iface, true);
        }

        private static void InterfaceStartStop(RoutingNetlinkSocket rtNetlinkSocket, CanNetworkInterface iface, bool start)
        {
            NetworkInterfaceModifierRequest modReq = NetlinkUtils.GenerateRequestForLinkModifierByIndex(iface.Index, null, start);
            int bytesWritten = rtNetlinkSocket.Write(modReq);
            if (bytesWritten > 0)
            {
                byte[] rxBuffer = new byte[8192];
                int numBytes = rtNetlinkSocket.Read(rxBuffer);
                if (numBytes >= 36)
                {
                    NetlinkMessageError nlMsgErr = NetlinkMessageError.FromBytes(rxBuffer);
                    if (nlMsgErr.MessageHeader.MessageType == NetlinkMessageType.NLMSG_ERROR)
                    {
                        if (nlMsgErr.Error == 0) // Success
                        {
                            Console.WriteLine($"Inteface Device Flags: {iface.DeviceFlags}");
                            Console.WriteLine($"Inteface Operational Status: {iface.OperationalStatus}");
                        }
                        else
                        {
                            Console.WriteLine($"Errno Set on Netlink Response: {-1 * nlMsgErr.Error}.");
                            return;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Unexpected Netlink message received: {nlMsgErr.MessageHeader.MessageType}.");
                        return;
                    }
                }
                else
                {
                    Console.WriteLine("Read insufficient bytes from the rtnetlink socket.");
                    return;
                }
            }
            else
            {
                Console.WriteLine($"Failed to write Netlink message: {LibcNativeMethods.Errno}.");
                return;
            }
        }
    }
}