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
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using SocketCANSharp;
using SocketCANSharp.Network;

namespace ObjectOrientedDiagAppSimulator
{
    /// <summary>
    /// Sample Object-Oriented Diagnostic Application Simulator
    /// 
    /// Produces the following output:
    /// 
    /// Tester :: Sent Request for ECU Serial Number (DID 0xF18C)
    /// ECU :: Received Request
    /// ECU :: Received ReadDataByIdentifier Request
    /// ECU :: DID Requested 0xF18C
    /// ECU :: Sent Response for DID 0xF18C
    /// Tester :: Received Response
    /// Tester :: Received ReadDataByIdentifier Positive Response
    /// Tester :: DID Received 0xF18C
    /// Tester :: Received ECU Serial Number (0xF18C): ESN-123456
    /// 
    /// Produces the following CAN bus data:
    /// 
    ///   vcan0       600  [08]  03 22 F1 8C BB BB BB BB
    ///   vcan0       700  [16]  00 0D 62 F1 8C 45 53 4E 2D 31 32 33 34 35 36 BB
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var vcan0 = CanNetworkInterface.GetAllInterfaces(true).FirstOrDefault(iface => iface.Name.Equals("vcan0"));
            if (vcan0 == null)
            {
                Console.WriteLine("vcan0 interface not found!");
            }

            using (var ecuSocket = new IsoTpCanSocket())
            using (var testerSocket = new IsoTpCanSocket())
            {
                SetupEcu(ecuSocket, vcan0);
                Task.Run(() => EcuProcessLoop(ecuSocket));

                testerSocket.BaseOptions = new CanIsoTpOptions()
                {
                    Flags = IsoTpFlags.CAN_ISOTP_TX_PADDING,
                    TxPadByte = 0xBB,
                };
                testerSocket.FlowControlOptions = new CanIsoTpFlowControlOptions()
                {
                    BlockSize = 0x00,
                    Stmin = 0x00,
                    WftMax = 0x00,
                };
                testerSocket.LinkLayerOptions = new CanIsoTpLinkLayerOptions()
                {
                    Mtu = SocketCanConstants.CANFD_MTU,
                    TxDataLength = 64,
                    TxFlags = CanFdFlags.CANFD_BRS,
                };

                testerSocket.Bind(vcan0, 0x600, 0x700);
                
                Console.WriteLine("Tester :: Sent Request for ECU Serial Number (DID 0xF18C)");
                int bytesWritten = testerSocket.Write(new byte[] { 0x22, 0xf1, 0x8c });
                
                var receiveBuffer = new byte[4095];
                int bytesRead = testerSocket.Read(receiveBuffer);
                byte[] rawData = receiveBuffer.Take(bytesRead).ToArray();
                if (rawData.Length > 0)
                {
                    Console.WriteLine($"Tester :: Received Response");
                    if (rawData[0] == 0x62) // ReadDataByIdentifier Positive Response
                    {
                        Console.WriteLine("Tester :: Received ReadDataByIdentifier Positive Response");
                        if (rawData.Length >= 3)
                        {
                            ushort did = BitConverter.ToUInt16(new byte[] { rawData[2], rawData[1] }); // Convert from Big Endian
                            Console.WriteLine($"Tester :: DID Received 0x{did:X4}");
                            if (did == 0xf18c)
                            {
                                Console.WriteLine($"Tester :: Received ECU Serial Number (0xF18C): {Encoding.ASCII.GetString(rawData.Skip(3).ToArray())}");
                            }
                        }
                    }
                }
            }
        }

        private static void SetupEcu(IsoTpCanSocket isoTpCanSocket, CanNetworkInterface canIf)
        {
            isoTpCanSocket.BaseOptions = new CanIsoTpOptions()
            {
                Flags = IsoTpFlags.CAN_ISOTP_TX_PADDING | IsoTpFlags.CAN_ISOTP_RX_PADDING | IsoTpFlags.CAN_ISOTP_CHK_PAD_LEN,
                TxPadByte = 0xBB,
            };
            isoTpCanSocket.FlowControlOptions = new CanIsoTpFlowControlOptions()
            {
                BlockSize = 0x0A,
                Stmin = 0x05,
                WftMax = 0x00,
            };
            isoTpCanSocket.LinkLayerOptions = new CanIsoTpLinkLayerOptions()
            {
                Mtu = SocketCanConstants.CANFD_MTU,
                TxDataLength = 64,
                TxFlags = CanFdFlags.CANFD_BRS,
            };
            isoTpCanSocket.Bind(canIf, 0x700, 0x600);
        }

        private static void EcuProcessLoop(IsoTpCanSocket isoTpCanSocket)
        {
            while (true)
            {
                // ECU reads request
                var receiveRequestMessage = new byte[4095];
                int nBytes = isoTpCanSocket.Read(receiveRequestMessage);
                if (nBytes > 0)
                {
                    Console.WriteLine($"ECU :: Received Request");
                    if (receiveRequestMessage[0] == 0x22) // ReadDataByIdentifier Request
                    {
                        Console.WriteLine("ECU :: Received ReadDataByIdentifier Request");
                        if (nBytes == 3) // Check Request Length - For sake of simplicity only supporting single DID per RDBI request.
                        {
                            ushort did = BitConverter.ToUInt16(new byte[] { receiveRequestMessage[2], receiveRequestMessage[1] }); // Convert from Big Endian
                            Console.WriteLine($"ECU :: DID Requested 0x{did:X4}");

                            // ECU sends back response
                            if (did == 0xf18c) // ECU Serial Number
                            {
                                var esnData = new byte[] { 0x45, 0x53, 0x4e, 0x2d, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36 }; // ASCII: ESN-123456
                                isoTpCanSocket.Write(new byte[] {0x62, 0xf1, 0x8c }.Concat(esnData).ToArray());
                                Console.WriteLine($"ECU :: Sent Response for DID 0x{did:X4}");
                            }
                        }
                    }
                }
            }
        }
    }
}
