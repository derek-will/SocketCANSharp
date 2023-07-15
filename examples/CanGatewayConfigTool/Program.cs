#region License
/* 
BSD 3-Clause License

Copyright (c) 2023, Derek Will
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
using SocketCANSharp;
using SocketCANSharp.Network;
using SocketCANSharp.Network.Netlink;
using SocketCANSharp.Network.Netlink.Gateway;
using SocketCANSharp.Network.BroadcastManagement;
using SocketCANSharp.Capabilities;

namespace CanGatewayConfigTool
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to the CAN Gateway Configuration Tool Demo!");

            var vcan0 = CanNetworkInterface.GetAllInterfaces(true).FirstOrDefault(iface => iface.Name.Equals("vcan0"));
            if (vcan0 == null)
            {
                Console.WriteLine("vcan0 interface not found and is required for this demo!");
                return;
            }

            var vcan1 = CanNetworkInterface.GetAllInterfaces(true).FirstOrDefault(iface => iface.Name.Equals("vcan1"));
            if (vcan1 == null)
            {
                Console.WriteLine("vcan1 interface not found and is required for this demo!");
                return;
            }

            using (var cgwSocket = new CanGatewaySocket())
            using (var bcmCanSocket = new BcmCanSocket())
            {
                cgwSocket.ReceiveTimeout = 1000;
                cgwSocket.SendTimeout = 1000;
                bcmCanSocket.Connect(vcan0);
                var canFrame = new CanFrame(0x123, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF });
                var frames = new CanFrame[] { canFrame };
                var config = new BcmCyclicTxTaskConfiguration()
                {
                    Id = 0x123,
                    StartTimer = true,
                    SetInterval = true,
                    InitialIntervalConfiguration = new BcmInitialIntervalConfiguration(0, new BcmTimeval(0, 0)),
                    PostInitialInterval = new BcmTimeval(0, 50000), // 50 ms rate
                };
                int nBytes = bcmCanSocket.CreateCyclicTransmissionTask(config, frames);

                cgwSocket.ReceiveTimeout = 1000;
                cgwSocket.SendTimeout = 1000;
                cgwSocket.Bind(new SockAddrNetlink(0, 0));

                if (CapabilityUtils.IsCurrentProcessCapable(Capability.CAP_NET_ADMIN))
                {
                    try
                    {
                        Console.WriteLine("Adding CGW Rule to append checksum of data to CAN Frame with CAN ID 0x123 when routed from vcan0 to vcan1...");
                        var rule = new CgwCanToCanRule(CgwCanFrameType.ClassicalCAN)
                        {
                            SourceIndex = (uint)vcan0.Index,
                            DestinationIndex = (uint)vcan1.Index,
                            EnableLocalCanSocketLoopback = true,
                            ReceiveFilter = new CanFilter(0x123, 0x7FF),
                            SetModifier = new ClassicalCanGatewayModifier(CanGatewayModificationType.CGW_MOD_LEN, new CanFrame(0x000, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00 })),
                            ChecksumXorConfiguration = new CgwChecksumXor(0, 3, 4, 0xCC),
                            UpdateIdentifier = 0xFFEEEEDD,
                        };
                        
                        if (AddRule(cgwSocket, rule))
                        {
                            Console.WriteLine("Successfully added CGW Rule!");

                            Console.WriteLine("Reading vcan0 for CAN ID 0x123...");
                            using (var rawCanSocket = new RawCanSocket())
                            {
                                rawCanSocket.ReceiveTimeout = 1000;
                                rawCanSocket.CanFilters = new CanFilter[] { new CanFilter(0x123, 0x7ff) };
                                rawCanSocket.Bind(new SockAddrCan(vcan0.Index));
                                rawCanSocket.Read(out CanFrame vcan0Frame);
                                Console.WriteLine($"{vcan0.Name}: 0x{vcan0Frame.CanId:X} {BitConverter.ToString(vcan0Frame.Data.Take(vcan0Frame.Length).ToArray())}");
                            }

                            Console.WriteLine("Reading vcan1 for CAN ID 0x123...");
                            using (var rawCanSocket = new RawCanSocket())
                            {
                                rawCanSocket.ReceiveTimeout = 1000;
                                rawCanSocket.CanFilters = new CanFilter[] { new CanFilter(0x123, 0x7ff) };
                                rawCanSocket.Bind(new SockAddrCan(vcan1.Index));
                                rawCanSocket.Read(out CanFrame vcan1Frame);
                                Console.WriteLine($"{vcan1.Name}: 0x{vcan1Frame.CanId:X} {BitConverter.ToString(vcan1Frame.Data.Take(vcan1Frame.Length).ToArray())}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Failed to add CGW rule!");
                        }

                        List<CgwCanToCanRule> ruleList = GetRules(cgwSocket);
                        Console.WriteLine("Printing all current CGW Rules...");
                        foreach (var r in ruleList)
                        {
                            Console.WriteLine("+++++++++++++++++++++++++");
                            Console.WriteLine(r);
                            Console.WriteLine("+++++++++++++++++++++++++");
                        }
                    }
                    finally
                    {
                        Console.WriteLine("Clearing all CGW Rules...");
                        if (RemoveAllRules(cgwSocket))
                        {
                            Console.WriteLine("Successfully cleared all CGW Rules!");
                        }
                        else
                        {
                            Console.WriteLine("Failed to clear all CGW Rules!");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Cannot run the demonstration as the current process lacks suffucient privileges (CAP_NET_ADMIN).");
                }
            }
        }

        private static bool AddRule(CanGatewaySocket cgwSocket, CgwCanToCanRule rule)
        {
            cgwSocket.AddOrUpdateCanToCanRule(rule);
            var data = new byte[8192];
            int bytesRead = cgwSocket.Read(data);
            var realData = data.Take(bytesRead).ToArray();
            NetlinkMessageHeader header = NetlinkUtils.PeekAtHeader(realData);
            if (header.MessageType == NetlinkMessageType.NLMSG_ERROR)
            {
                NetlinkMessageError nlMsgErr = NetlinkMessageError.FromBytes(realData);
                if (nlMsgErr.Error == 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool RemoveAllRules(CanGatewaySocket cgwSocket)
        {
            cgwSocket.RemoveAllCanToCanRules();
            var data = new byte[8192];
            int bytesRead = cgwSocket.Read(data);
            var realData = data.Take(bytesRead).ToArray();
            NetlinkMessageHeader header = NetlinkUtils.PeekAtHeader(realData);
            if (header.MessageType == NetlinkMessageType.NLMSG_ERROR)
            {
                NetlinkMessageError nlMsgErr = NetlinkMessageError.FromBytes(realData);
                if (nlMsgErr.Error == 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static List<CgwCanToCanRule> GetRules(CanGatewaySocket cgwSocket)
        {
            cgwSocket.RequestListOfAllRules();
            var ruleList = new List<CgwCanToCanRule>();
            bool keepReading = true;
            do
            {
                byte[] rxBuffer = new byte[8192];
                int numBytes = cgwSocket.Read(rxBuffer);
                byte[] filledBuffer = rxBuffer.Take(numBytes).ToArray();
                ruleList.AddRange(cgwSocket.ParseCanToCanRules(filledBuffer, out keepReading));
            }
            while (keepReading);

            return ruleList;
        }
    }
}