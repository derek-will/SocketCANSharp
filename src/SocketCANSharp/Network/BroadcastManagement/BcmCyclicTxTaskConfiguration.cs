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
    /// Cyclic Transmission Task configuration for the Broadcast Manager (BCM).
    /// </summary>
    public class BcmCyclicTxTaskConfiguration
    {
        /// <summary>
        /// Task ID (CAN ID)
        /// </summary>
        public uint Id { get; set; }
        /// <summary>
        /// Set interval related information.
        /// </summary>
        public bool SetInterval { get; set; }
        /// <summary>
        /// Start timer and immediately start transmitting.
        /// </summary>
        public bool StartTimer { get; set; }
        /// <summary>
        /// Send notification when first interval is complete.
        /// </summary>
        public bool NotifyWhenFirstIntervalComplete { get; set; }
        /// <summary>
        /// Immediately Queue New CAN Frame
        /// </summary>
        public bool ImmediatelyQueueNewFrame { get; set; }
        /// <summary>
        /// Copies the CAN ID in the Header to each CAN Frame in the sequence.
        /// </summary>
        public bool CopyCanIdInHeaderToEachCanFrame { get; set; }
        /// <summary>
        /// Starts Multiple Frame Transmission at Index 0.
        /// </summary>
        public bool RestartMultipleFrameTxAtIndexZero { get; set; }
        /// <summary>
        /// Initial Interval Configuration.
        /// </summary>
        public BcmInitialIntervalConfiguration InitialIntervalConfiguration { get; set; }
        /// <summary>
        /// Post-Initial Interval Configuration.
        /// </summary>
        public BcmTimeval PostInitialInterval { get; set;}

        /// <summary>
        /// Initializes a new instance of the BcmCyclicTxTaskConfiguration class with default values.
        /// </summary>
        public BcmCyclicTxTaskConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the BcmCyclicTxTaskConfiguration class using the supplied parameters.
        /// </summary>
        /// <param name="id">Task ID (CAN ID)</param>
        /// <param name="flags">Raw BcmFlags</param>
        /// <param name="count">The number of frames to send at the initial rate</param>
        /// <param name="initInterval">The initial interval rate</param>
        /// <param name="postInitInterval">The post-initial interval rate</param>
        public BcmCyclicTxTaskConfiguration(uint id, BcmFlags flags, uint count, BcmTimeval initInterval, BcmTimeval postInitInterval)
        {
            Id = id;
            SetInterval = flags.HasFlag(BcmFlags.SETTIMER);
            StartTimer = flags.HasFlag(BcmFlags.STARTTIMER);
            NotifyWhenFirstIntervalComplete = flags.HasFlag(BcmFlags.TX_COUNTEVT);
            ImmediatelyQueueNewFrame = flags.HasFlag(BcmFlags.TX_ANNOUNCE);
            CopyCanIdInHeaderToEachCanFrame = flags.HasFlag(BcmFlags.TX_CP_CAN_ID);
            RestartMultipleFrameTxAtIndexZero = flags.HasFlag(BcmFlags.TX_RESET_MULTI_IDX);
            InitialIntervalConfiguration = new BcmInitialIntervalConfiguration((int)count, initInterval);
            PostInitialInterval = postInitInterval;
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
            if (NotifyWhenFirstIntervalComplete)
                flags |= BcmFlags.TX_COUNTEVT;
            if (ImmediatelyQueueNewFrame)
                flags |= BcmFlags.TX_ANNOUNCE;
            if (CopyCanIdInHeaderToEachCanFrame)
                flags |= BcmFlags.TX_CP_CAN_ID;
            if (RestartMultipleFrameTxAtIndexZero)
                flags |= BcmFlags.TX_RESET_MULTI_IDX;
            return flags;
        }
    }
}