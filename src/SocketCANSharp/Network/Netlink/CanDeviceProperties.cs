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

namespace SocketCANSharp.Network.Netlink
{
    /// <summary>
    /// CAN device properties that can be set via Netlink.
    /// </summary>
    public class CanDeviceProperties
    {
        /// <summary>
        /// Link Kind (i.e., can or vcan).
        /// </summary>
        public string LinkKind { get; set; }

        /// <summary>
        /// Maximum Transmission Unit.
        /// </summary>
        public uint? MaximumTransmissionUnit { get; set; }

        /// <summary>
        /// How soon in milliseconds the CAN controller shall automatically restart whenever BUS OFF state is detected.
        /// </summary>
        public uint? RestartDelay { get; set; }

        /// <summary>
        /// Restart the CAN controller.
        /// Note: This is only possible when the interface is running, Auto-Restart is disabled (delay is set to 0) and the device is in BUS OFF state. 
        /// </summary>
        public bool TriggerRestart { get; set; }

        /// <summary>
        /// CAN Bit Timing Parameters in Arbitration Phase.
        /// </summary>
        public CanBitTiming BitTiming { get; set; }

        /// <summary>
        /// CAN Bit Timing Parameters in Data Phase.
        /// </summary>
        public CanBitTiming DataPhaseBitTiming { get; set; }

        /// <summary>
        /// CAN Controller Mode option.
        /// </summary>
        public CanControllerMode ControllerMode { get; set; }

        /// <summary>
        /// Switchable Termination Resistance option.
        /// </summary>
        public ushort? TerminationResistance { get; set; }
    }
}