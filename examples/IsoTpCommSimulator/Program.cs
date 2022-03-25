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
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using SocketCANSharp;

namespace IsoTpCommSimulator
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var testerSocketHandle = SetupTester())
            using (var ecuSocketHandle = SetupEcu())
            {
                Task.Run(() => EcuLoop(ecuSocketHandle));

                // Tester sends request
                var requestMessage = new byte[] { 0x03 };
                int nBytes = LibcNativeMethods.Write(testerSocketHandle, requestMessage, requestMessage.Length);
                if (nBytes != 1)
                    throw new IOException("Failed to write the requested bytes.");

                Console.WriteLine($"TX: 0x7e0   [{nBytes}]  {BitConverter.ToString(requestMessage.Take(nBytes).ToArray()).Replace("-", " ")}");

                // Tester reads response
                var receiveResponseMessage = new byte[4095];
                nBytes = LibcNativeMethods.Read(testerSocketHandle, receiveResponseMessage, receiveResponseMessage.Length);
                if (nBytes > 0)
                {
                    Console.WriteLine($"RX: 0x7e8   [{nBytes}]  {BitConverter.ToString(receiveResponseMessage.Take(nBytes).ToArray()).Replace("-", " ")}");
                }
            }
        }

        private static SafeSocketHandle SetupTester()
        {
            var testerSocketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_ISOTP);
            if (testerSocketHandle.IsInvalid)
            {
                throw new InvalidOperationException($"Failed to create socket. Errno: {LibcNativeMethods.Errno}");
            }

            var ifr = new Ifreq("vcan0"); 
            int ioctlResult = LibcNativeMethods.Ioctl(testerSocketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
            if (ioctlResult == -1)
            {
                testerSocketHandle.Close();
                throw new InvalidOperationException($"Failed to look up interface index for vcan0. Errno: {LibcNativeMethods.Errno}");
            }

            var testerAddr = new SockAddrCanIsoTp(ifr.IfIndex)
            {
                TxId = 0x7e0,
                RxId = 0x7e8,
            };

            int bindResult = LibcNativeMethods.Bind(testerSocketHandle, testerAddr, Marshal.SizeOf(typeof(SockAddrCanIsoTp)));
            if (bindResult == -1)
            {
                testerSocketHandle.Close();
                throw new InvalidOperationException($"Failed to bind address to the socket. Errno: {LibcNativeMethods.Errno}");
            }

            return testerSocketHandle;
        }

        private static SafeSocketHandle SetupEcu()
        {
            var ecuSocketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_ISOTP);
            if (ecuSocketHandle.IsInvalid)
            {
                throw new InvalidOperationException($"Failed to create socket. Errno: {LibcNativeMethods.Errno}");
            }

            var ifr = new Ifreq("vcan0"); 
            int ioctlResult = LibcNativeMethods.Ioctl(ecuSocketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
            if (ioctlResult == -1)
            {
                ecuSocketHandle.Close();
                throw new InvalidOperationException($"Failed to look up interface index for vcan0. Errno: {LibcNativeMethods.Errno}");
            }

            var testerAddr = new SockAddrCanIsoTp(ifr.IfIndex)
            {
                TxId = 0x7e8,
                RxId = 0x7e0,
            };

            int bindResult = LibcNativeMethods.Bind(ecuSocketHandle, testerAddr, Marshal.SizeOf(typeof(SockAddrCanIsoTp)));
            if (bindResult == -1)
            {
                ecuSocketHandle.Close();
                throw new InvalidOperationException($"Failed to bind address to the socket. Errno: {LibcNativeMethods.Errno}");
            }

            return ecuSocketHandle;
        }

        private static void EcuLoop(SafeSocketHandle socketHandle)
        {
            while (true)
            {
                // ECU reads request
                var receiveRequestMessage = new byte[4095];
                int nBytes = LibcNativeMethods.Read(socketHandle, receiveRequestMessage, receiveRequestMessage.Length);
                if (nBytes > 0)
                {
                    if (receiveRequestMessage[0] == 0x03) // Request Emission-Related DTCs
                    {
                        // ECU sends back response
                        var responseMessage = new byte[] { 0x43, 0x06, 0x01, 0x43, 0x01, 0x96, 0x02, 0x34, 0x02, 0xCD, 0x03, 0x57, 0x0A, 0x24 };
                        LibcNativeMethods.Write(socketHandle, responseMessage, responseMessage.Length);
                    }
                }
            }
        }
    }
}
