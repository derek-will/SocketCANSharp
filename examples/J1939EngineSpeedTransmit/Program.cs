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

using System.Net.Sockets;
using System.Runtime.InteropServices;
using SocketCANSharp;
using SocketCANSharp.Network;

namespace J1939EngineSpeedTransmit
{
    /// <summary>
    /// Really basic J1939 program to show how to use SocketCAN# for J1939.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var vcan0 = CanNetworkInterface.GetAllInterfaces(true).FirstOrDefault(iface => iface.Name.Equals("vcan0"));
            if (vcan0 == null)
            {
                Console.WriteLine("vcan0 interface not found!");
                return;
            }

            using (var j1939Socket = new J1939CanSocket())
            {
                j1939Socket.EnableBroadcast = true;
                j1939Socket.SendPriority = 3;
                j1939Socket.Bind(vcan0, SocketCanConstants.J1939_NO_NAME, 0x0F004, 0x01);
                var destAddr = new SockAddrCanJ1939(vcan0.Index)
                {
                    Name = SocketCanConstants.J1939_NO_NAME,
                    PGN = 0x0F004,
                    Address = SocketCanConstants.J1939_NO_ADDR,
                };
                // Engine Speed: 2573.5 RPM (0x506C -> 20588 * 0.125)
                j1939Socket.WriteTo(new byte[] { 0xFF, 0xFF, 0xFF, 0x6C, 0x50, 0xFF, 0xFF, 0xFF }, MessageFlags.None, destAddr);
                // candump: vcan0  0CF00401   [8]  FF FF FF 6C 50 FF FF FF
            }

            using (var j1939Handle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_J1939))
            {
                var ifr = new Ifreq("vcan0");
                int ioctlResult = LibcNativeMethods.Ioctl(j1939Handle, SocketCanConstants.SIOCGIFINDEX, ifr);

                int value = 1;
                int enableBroadcastResult = LibcNativeMethods.SetSockOpt(j1939Handle, SocketLevel.SOL_SOCKET, SocketLevelOptions.SO_BROADCAST, ref value, Marshal.SizeOf(typeof(int)));

                int prio = 3;
                int prioResult = LibcNativeMethods.SetSockOpt(j1939Handle, SocketLevel.SOL_CAN_J1939, J1939SocketOptions.SO_J1939_SEND_PRIO, ref prio, Marshal.SizeOf(typeof(int)));

                var srcAddr = new SockAddrCanJ1939(ifr.IfIndex)
                {
                    Name = SocketCanConstants.J1939_NO_NAME,
                    PGN = 0x0F004,
                    Address = 0x01,
                };
                int bindResult = LibcNativeMethods.Bind(j1939Handle, srcAddr, Marshal.SizeOf(typeof(SockAddrCanJ1939)));;

                var dstAddr = new SockAddrCanJ1939(vcan0.Index)
                {
                    Name = SocketCanConstants.J1939_NO_NAME,
                    PGN = 0x0F004,
                    Address = SocketCanConstants.J1939_NO_ADDR,
                };
                // Engine Speed: 582.5 RPM (0x1234 -> 4660 * 0.125)
                byte[] data = new byte[] { 0xFF, 0xFF, 0xFF, 0x34, 0x12, 0xFF, 0xFF, 0xFF };
                int sendToResult = LibcNativeMethods.SendTo(j1939Handle, data, data.Length, MessageFlags.None, dstAddr, Marshal.SizeOf(typeof(SockAddrCanJ1939)));
                // candump: vcan0  0CF00401   [8]  FF FF FF 34 12 FF FF FF
            }
        }
    }
}
