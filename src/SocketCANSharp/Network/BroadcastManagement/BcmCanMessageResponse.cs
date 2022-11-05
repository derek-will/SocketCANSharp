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

namespace SocketCANSharp.Network.BroadcastManagement
{
    /// <summary>
    /// Broadcast Manager (BCM) CAN Message Response.
    /// </summary>
    public class BcmCanMessageResponse
    {
        /// <summary>
        /// BCM CAN Frame Type.
        /// </summary>
        public BcmCanFrameType FrameType { get; set; }
        /// <summary>
        /// BCM Response Message Type.
        /// </summary>
        public BcmResponseType ResponseType { get; set; }
        /// <summary>
        /// Cyclic Transmission Task Configuration which appears on TransmissionTaskConfiguration and FirstIntervalTransmissionComplete response types.
        /// </summary>
        public BcmCyclicTxTaskConfiguration CyclicTransmissionTaskConfiguration { get; set; }
        /// <summary>
        /// Content Receive Filter Configuration which appears on ReceiveFilterConfiguration, CyclicMessageReceiveTimeout, and CanFrameReceiveUpdateNotification response types.
        /// </summary>
        public BcmContentRxFilterSubscription ContentReceiveFilterSubscription { get; set; }
        /// <summary>
        /// Classic CAN Frame Array.
        /// </summary>
        public CanFrame[] ClassicFrames { get; set; }
        /// <summary>
        /// CAN FD Frame Array.
        /// </summary>
        public CanFdFrame[] FdFrames { get; set; }
    }
}