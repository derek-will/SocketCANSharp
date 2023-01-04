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
using System.Runtime.InteropServices;

namespace SocketCANSharp.Network.Netlink
{
    /// <summary>
    /// Interface Link Statistics with 32 bit length parameters.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class InterfaceLinkStatistics
    {
        /// <summary>
        /// Number of good packets received by the interface. 
        /// For hardware interfaces, this count includes all good packets received from the device by the host and includes packets which the host had to drop at various stages of processing.
        /// </summary>
        public uint RxPackets { get; set; }
        /// <summary>
        /// Number of packets successfully transmitted. 
        /// For hardware interfaces, this count includes packets which the host was able to successfully hand over to the device, but this does not necessarily mean that packets were successfully transmitted out of the device and only that the device acknowledged it copied them out of host memory.
        /// </summary>
        public uint TxPackets { get; set; }
        /// <summary>
        /// Number of good received bytes, corresponding to RxPackets.
        /// </summary>
        public uint RxBytes { get; set; }
        /// <summary>
        /// Number of good transmitted bytes, corresponding to TxPackets.
        /// </summary>
        public uint TxBytes { get; set; }
        /// <summary>
        /// Total number of bad packets received on this network device. 
        /// This counter includes events counted by RxLengthErrors, RxCrcErrors, RxFrameErrors as well as other errors.
        /// </summary>
        public uint RxErrors { get; set; }
        /// <summary>
        /// Total number of transmit errors. 
        /// This counter includes events counted by TxAbortedErrors, TxCarrierErrors, TxFifoErrors, TxHeartbeatErrors, TxWindowErrors as well as other errors.
        /// </summary>
        public uint TxErrors { get; set; }
        /// <summary>
        /// Number of packets received but not processed. 
        /// For hardware interfaces this counter may include packets discarded due to Data Link Layer (L2) address filtering, but not packets dropped by the device due to buffer exhaustion which are counted separately in RxMissedErrors.
        /// </summary>
        public uint RxDropped { get; set; }
        /// <summary>
        /// Number of packets dropped before they could be transmitted.
        /// </summary>
        public uint TxDropped { get; set; }
        /// <summary>
        /// Number of multicast packets received. 
        /// For hardware interfaces this counter is typically computed at the device level and therefore may include packets which never reached the host.
        /// </summary>
        public uint Multicast { get; set; }
        /// <summary>
        /// Number of collisions during packet transmission.
        /// </summary>
        public uint Collisions { get; set; }

        /// <summary>
        /// Number of packets dropped due to invalid length. 
        /// </summary>
        public uint RxLengthErrors { get; set; }
        /// <summary>
        /// Receiver buffer overflow event counter.
        /// </summary>
        public uint RxOverErrors { get; set; }
        /// <summary>
        /// Number of packets received with a CRC error.
        /// </summary>
        public uint RxCrcErrors { get; set; }
        /// <summary>
        /// Receiver frame alignment errors. 
        /// </summary>
        public uint RxFrameErrors { get; set; }
        /// <summary>
        /// Receiver FIFO error counter.
        /// </summary>
        public uint RxFifoErrors { get; set; }
        /// <summary>
        /// Number of packets missed by the host. 
        /// </summary>
        public uint RxMissedErrors { get; set; }

        /// <summary>
        /// A general device discard counter.
        /// </summary>
        public uint TxAbortedErrors { get; set; }
        /// <summary>
        /// Number of errors during frame transmission due to loss of the carrier during the operation.
        /// </summary>
        public uint TxCarrierErrors { get; set; }
        /// <summary>
        /// Number of frame transmission errors due to device FIFO underflow. 
        /// This means that the transmission event was started, but the device was unable to deliver the entire frame to the transmitter before timing out.
        /// </summary>
        public uint TxFifoErrors { get; set; }
        /// <summary>
        /// Number of Heartbeat (SQE Test) errors on older half-duplex Ethernet.
        /// </summary>
        public uint TxHeartbeatErrors { get; set; }
        /// <summary>
        /// Number of frame transmission errors due to "late" collisions.
        /// </summary>
        public uint TxWindowErrors { get; set; }

        /// <summary>
        /// Number of correctly received compressed packets.
        /// </summary>
        public uint RxCompressed { get; set; }
        /// <summary>
        /// Number of transmitted compressed packets. 
        /// </summary>
        public uint TxCompressed { get; set; }
        /// <summary>
        /// Number of packets received on the interface, but later dropped by the networking stack because the device is not designated to receive packets.
        /// </summary>
        public uint RxNoHandler { get; set; }

        /// <summary>
        /// Converts a raw byte array into a InterfaceLinkStatistics instance.
        /// </summary>
        /// <param name="data">Raw Byte Array.</param>
        /// <returns>InterfaceLinkStatistics instance</returns>
        public static InterfaceLinkStatistics FromBytes(byte[] data)
        {
            int size = Marshal.SizeOf<InterfaceLinkStatistics>();
            // for backwards compatibility purposes: make new array to map data into
            // linux kernel adds new fields to the statistics structure occasionally and this will help 
            // isolate the changes from causing friction with the wrapper interface
            byte[] array = new byte[size]; 
            Array.Copy(data, 0, array, 0, data.Length > size ? size : data.Length);
            return NetlinkUtils.FromBytes<InterfaceLinkStatistics>(array);
        }

        /// <summary>
        /// Returns a string representation of the current InterfaceLinkStatistics instance.
        /// </summary>
        /// <returns>String representation of this InterfaceLinkStatistics instance.</returns>
        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"RxPackets: {RxPackets}");
            stringBuilder.AppendLine($"TxPackets: {TxPackets}");
            stringBuilder.AppendLine($"RxBytes: {RxBytes}");
            stringBuilder.AppendLine($"TxBytes: {TxBytes}");
            stringBuilder.AppendLine($"RxErrors: {RxErrors}");
            stringBuilder.AppendLine($"TxErrors: {TxErrors}");
            stringBuilder.AppendLine($"RxDropped: {RxDropped}");
            stringBuilder.AppendLine($"TxDropped: {TxDropped}");
            stringBuilder.AppendLine($"Multicast: {Multicast}");
            stringBuilder.AppendLine($"Collisions: {Collisions}");

            stringBuilder.AppendLine($"RxLengthErrors: {RxLengthErrors}");
            stringBuilder.AppendLine($"RxOverErrors: {RxOverErrors}");
            stringBuilder.AppendLine($"RxCrcErrors: {RxCrcErrors}");
            stringBuilder.AppendLine($"RxFrameErrors: {RxFrameErrors}");
            stringBuilder.AppendLine($"RxFifoErrors: {RxFifoErrors}");
            stringBuilder.AppendLine($"RxMissedErrors: {RxMissedErrors}");

            stringBuilder.AppendLine($"TxAbortedErrors: {TxAbortedErrors}");
            stringBuilder.AppendLine($"TxCarrierErrors: {TxCarrierErrors}");
            stringBuilder.AppendLine($"TxFifoErrors: {TxFifoErrors}");
            stringBuilder.AppendLine($"TxHeartbeatErrors: {TxHeartbeatErrors}");
            stringBuilder.AppendLine($"TxWindowErrors: {TxWindowErrors}");

            stringBuilder.AppendLine($"RxCompressed: {RxCompressed}");
            stringBuilder.AppendLine($"TxCompressed: {TxCompressed}");
            stringBuilder.AppendLine($"RxNoHandler: {RxNoHandler}");

            return stringBuilder.ToString();
        }
    }
}