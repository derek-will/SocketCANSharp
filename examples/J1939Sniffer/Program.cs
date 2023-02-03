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

using System.Collections.Concurrent;
using SocketCANSharp;
using SocketCANSharp.Network;

namespace J1939Sniffer
{
    /// <summary>
    /// Basic J1939 bus sniffer utility to illustrate how to use SocketCAN# to monitor a J1939 network.
    /// </summary>
    class Program
    {
        private static readonly BlockingCollection<J1939Data> concurrentQueue = new BlockingCollection<J1939Data>();

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
                j1939Socket.EnablePromiscuousMode = true;
                j1939Socket.ReceiveBufferSize = 2048;
                j1939Socket.Bind(vcan0, SocketCanConstants.J1939_NO_NAME, 
                    SocketCanConstants.J1939_NO_PGN, SocketCanConstants.J1939_NO_ADDR);
                
                Console.WriteLine("Sniffing vcan0 for J1939 messages...");
                Task.Run(() => PrintLoop());
                while (true)
                {
                    var buffer = new byte[2048];
                    var srcAddr = new SockAddrCanJ1939();
                    int nReadBytes = j1939Socket.ReadFrom(buffer, MessageFlags.None, srcAddr);
                    if (nReadBytes > 0)
                    {
                        concurrentQueue.Add(new J1939Data() { Length = nReadBytes, Buffer = buffer, SocketAddress = srcAddr });
                    }
                }
            }
        }

        private static void PrintLoop()
        {
            while (true)
            {
                J1939Data readData = concurrentQueue.Take();
                Console.WriteLine($"PGN: 0x{readData.SocketAddress.PGN:X5}; SA: 0x{readData.SocketAddress.Address:X2}   [{readData.Length:D4}]  {BitConverter.ToString(readData.Buffer.Take(readData.Length).ToArray()).Replace("-", " ")}"); 
            }
        }

        private struct J1939Data
        {
            public int Length { get; set; }
            public byte[] Buffer { get; set; }
            public SockAddrCanJ1939 SocketAddress { get; set; }
        }
    }
}
