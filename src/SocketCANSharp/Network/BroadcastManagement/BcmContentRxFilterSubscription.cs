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
    /// Content receive filter subscription for the Broadcast Manager (BCM).
    /// </summary>
    public class BcmContentRxFilterSubscription
    {
        /// <summary>
        /// Filter ID (CAN ID).
        /// </summary>
        public uint Id { get; set; }
        /// <summary>
        /// Set interval related information.
        /// </summary>
        public bool SetInterval { get; set; }
        /// <summary>
        /// Start timer and immediately begin monitoring for reception timeouts.
        /// </summary>
        public bool StartTimer { get; set; }
        /// <summary>
        /// Filter only by CAN ID.
        /// </summary>
        public bool FilterOnlyByCanId { get; set; }
        /// <summary>
        /// Monitor for Length Changes in the Frame.
        /// </summary>
        public bool MonitorLengthChanges { get; set; }
        /// <summary>
        /// Prevents the automatic starting of the receive timer.
        /// </summary>
        public bool PreventAutoStartingReceiveTimer { get; set; }
        /// <summary>
        /// Send a notification when monitored CAN frames return after a receive timeout.
        /// </summary>
        public bool NotifyWhenReceiveRestartsAfterTimeout { get; set; }
        /// <summary>
        /// Send a reply to a RTR-request.
        /// </summary>
        public bool ReplyToRtrRequest { get; set; }
        /// <summary>
        /// Timeout for when a frame is not received within a certain interval of time.
        /// </summary>
        public BcmTimeval ReceiveTimeout { get; set; }
        /// <summary>
        /// Used to throttle the number of received messages to the specified rate limit.
        /// </summary>
        public BcmTimeval ReceiveMessageRateLimit { get; set; }

        /// <summary>
        /// Initializes a new instance of the BcmContentRxFilterSubscription class with default values.
        /// </summary>
        public BcmContentRxFilterSubscription()
        {
        }

        /// <summary>
        /// Initializes a new instance of the BcmContentRxFilterSubscription class using the supplied parameters.
        /// </summary>
        /// <param name="id">Filter ID (CAN ID)</param>
        /// <param name="flags">Raw BcmFlags</param>
        /// <param name="receiveTimeout">Receive Timeout</param>
        /// <param name="messageRateLimit">Receive Message Rate Limit</param>
        public BcmContentRxFilterSubscription(uint id, BcmFlags flags, BcmTimeval receiveTimeout, BcmTimeval messageRateLimit)
        {
            Id = id;
            SetInterval = flags.HasFlag(BcmFlags.SETTIMER);
            StartTimer = flags.HasFlag(BcmFlags.STARTTIMER);
            FilterOnlyByCanId = flags.HasFlag(BcmFlags.RX_FILTER_ID);
            MonitorLengthChanges = flags.HasFlag(BcmFlags.RX_CHECK_DLC);
            PreventAutoStartingReceiveTimer = flags.HasFlag(BcmFlags.RX_NO_AUTOTIMER);
            NotifyWhenReceiveRestartsAfterTimeout = flags.HasFlag(BcmFlags.RX_ANNOUNCE_RESUME);
            ReplyToRtrRequest = flags.HasFlag(BcmFlags.RX_RTR_FRAME);
            ReceiveTimeout = receiveTimeout;
            ReceiveMessageRateLimit = messageRateLimit;
        }

        /// <summary>
        /// Calculates the raw BCM Flags using the current object state.
        /// </summary>
        /// <returns>Raw BCM Flags based on the current object state.</returns>
        public BcmFlags GetBcmFlags()
        {
            var flags = BcmFlags.None;
            if (SetInterval)
                flags |= BcmFlags.SETTIMER;
            if (StartTimer)
                flags |= BcmFlags.STARTTIMER;
            if (FilterOnlyByCanId)
                flags |= BcmFlags.RX_FILTER_ID;
            if (MonitorLengthChanges)
                flags |= BcmFlags.RX_CHECK_DLC;
            if (PreventAutoStartingReceiveTimer)
                flags |= BcmFlags.RX_NO_AUTOTIMER;
            if (NotifyWhenReceiveRestartsAfterTimeout)
                flags |= BcmFlags.RX_ANNOUNCE_RESUME;
            if (ReplyToRtrRequest)
                flags |= BcmFlags.RX_RTR_FRAME;
            return flags;
        }
    }   
}