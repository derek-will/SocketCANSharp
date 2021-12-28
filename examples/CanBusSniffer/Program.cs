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
using System.Linq;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using SocketCANSharp;

namespace CanBusSniffer
{
    /// <summary>
    /// Really basic CAN bus sniffer to show how to use SocketCAN# for RAW CAN.
    /// </summary>
    class Program
    {
        private static readonly BlockingCollection<CanFrame> concurrentQueue = new BlockingCollection<CanFrame>();

        static void Main(string[] args)
        {
            using (SafeSocketHandle socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW))
            {
                if (socketHandle.IsInvalid)
                {
                    Console.WriteLine("Failed to create socket.");
                    return;
                }

                var ifr = new Ifreq("vcan0");
                int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
                if (ioctlResult == -1)
                {
                    Console.WriteLine("Failed to look up interface by name.");
                    return;
                }

                var addr = new SockAddrCan(ifr.IfIndex);
                int bindResult = LibcNativeMethods.Bind(socketHandle, addr, Marshal.SizeOf(typeof(SockAddrCan)));
                if (bindResult == -1)
                {
                    Console.WriteLine("Failed to bind to address.");
                    return;
                }

                int frameSize = Marshal.SizeOf(typeof(CanFrame));
                Console.WriteLine("Sniffing vcan0...");
                Task.Run(() => PrintLoop());
                while (true)
                {
                    var readFrame = new CanFrame();
                    int nReadBytes = LibcNativeMethods.Read(socketHandle, ref readFrame, frameSize); 
                    if (nReadBytes > 0)
                    {
                        concurrentQueue.Add(readFrame);
                    }
                }
            }
        }

        private static void PrintLoop()
        {
            while (true)
            {
                CanFrame readFrame = concurrentQueue.Take();
                if ((readFrame.CanId & (uint)CanIdFlags.CAN_RTR_FLAG) != 0)
                {
                    Console.WriteLine($"{SocketCanConstants.CAN_ERR_MASK & readFrame.CanId,8:X}   [{readFrame.Length:D2}]  RTR");
                }
                else
                {
                    Console.WriteLine($"{SocketCanConstants.CAN_ERR_MASK & readFrame.CanId,8:X}   [{readFrame.Length:D2}]  {BitConverter.ToString(readFrame.Data.Take(readFrame.Length).ToArray()).Replace("-", " ")}");
                }
            }
        }
    }
}
