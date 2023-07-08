#region License
/* 
BSD 3-Clause License

Copyright (c) 2023, Derek Will
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

namespace SocketCANSharp.Network.Netlink.Gateway
{
    /// <summary>
    /// CGW Base Rule.
    /// </summary>
    public abstract class CgwBaseRule
    {
        private AbstractCanGatewayModifier _andModifier;
        private AbstractCanGatewayModifier _orModifier;
        private AbstractCanGatewayModifier _xorModifier;
        private AbstractCanGatewayModifier _setModifier;

        /// <summary>
        /// CGW Type.
        /// </summary>
        public CanGatewayType GatewayType { get; }
        /// <summary>
        /// Enables loopback for listeners on the local CAN sockets.
        /// </summary>
        public bool EnableLocalCanSocketLoopback { get; set; }
        /// <summary>
        /// Keep the source timestamp associated with the data instead of clearing it on receive.
        /// </summary>
        public bool MaintainSourceTimestamp { get; set; }
        /// <summary>
        /// Enables routing the received data back to the originating interface.
        /// </summary>
        public bool AllowRoutingToSameInterface { get; set; }
        /// <summary>
        /// Process CAN FD Frames.
        /// </summary>
        public bool IsCanFdRule { get; }
        /// <summary>
        /// CGW Binary AND Modifier.
        /// </summary>
        public AbstractCanGatewayModifier AndModifier 
        { 
            get
            {
                return _andModifier;
            } 
            set
            {
                if (value is CanFdGatewayModifier && !IsCanFdRule)
                    throw new InvalidOperationException("Argument contains a CAN FD Modifier, but this rule is not CAN FD enabled.");

                if (value is ClassicalCanGatewayModifier && IsCanFdRule)
                    throw new InvalidOperationException("Argument contains a Classical CAN Modifier, but this rule is CAN FD enabled.");

                _andModifier = value;
            }
        }
        /// <summary>
        /// CGW Binary OR Modifier.
        /// </summary>
        public AbstractCanGatewayModifier OrModifier
        { 
            get
            {
                return _orModifier;
            } 
            set
            {
                if (value is CanFdGatewayModifier && !IsCanFdRule)
                    throw new InvalidOperationException("Argument contains a CAN FD Modifier, but this rule is not CAN FD enabled.");

                if (value is ClassicalCanGatewayModifier && IsCanFdRule)
                    throw new InvalidOperationException("Argument contains a Classical CAN Modifier, but this rule is CAN FD enabled.");

                _orModifier = value;
            }
        }
        /// <summary>
        /// CGW Binary XOR Modifier.
        /// </summary>
        public AbstractCanGatewayModifier XorModifier
        { 
            get
            {
                return _xorModifier;
            } 
            set
            {
                if (value is CanFdGatewayModifier && !IsCanFdRule)
                    throw new InvalidOperationException("Argument contains a CAN FD Modifier, but this rule is not CAN FD enabled.");

                if (value is ClassicalCanGatewayModifier && IsCanFdRule)
                    throw new InvalidOperationException("Argument contains a Classical CAN Modifier, but this rule is CAN FD enabled.");

                _xorModifier = value;
            }
        }
        /// <summary>
        /// CGW Binary SET Modifier.
        /// </summary>
        public AbstractCanGatewayModifier SetModifier
        { 
            get
            {
                return _setModifier;
            } 
            set
            {
                if (value is CanFdGatewayModifier && !IsCanFdRule)
                    throw new InvalidOperationException("Argument contains a CAN FD Modifier, but this rule is not CAN FD enabled.");

                if (value is ClassicalCanGatewayModifier && IsCanFdRule)
                    throw new InvalidOperationException("Argument contains a Classical CAN Modifier, but this rule is CAN FD enabled.");

                _setModifier = value;
            }
        }
        /// <summary>
        /// CGW Checksum XOR Configuration.
        /// </summary>
        public CgwChecksumXor? ChecksumXorConfiguration { get; set; }
        /// <summary>
        /// CGW CRC8 Configuration.
        /// </summary>
        public CgwCrc8? Crc8Configuration { get; set; }
        /// <summary>
        /// Specify Hop Limit for this CAN-to-CAN Routing Rule.
        /// </summary>
        public byte HopLimit { get; set; }
        /// <summary>
        /// Update Identifier which is useful when making runtime updates to a rule.
        /// </summary>
        public uint UpdateIdentifier { get; set; }
        /// <summary>
        /// Number of Handled CAN Frames. Only valid in list response.
        /// </summary>
        public uint HandledFrames { get; set; }
        /// <summary>
        /// Number of Dropped CAN Frames. Only valid in list response.
        /// </summary>
        public uint DroppedFrames { get; set; }
        /// <summary>
        /// Number of Deleted CAN Frames. Only valid in list response.
        /// </summary>
        public uint DeletedFrames { get; set; }

        /// <summary>
        /// Initializes a new instance of the CgwBaseRule class with the specified CGW Type.
        /// </summary>
        /// <param name="gwType">CGW Type</param>
        /// <param name="canFrameType">CAN Frame Type</param>
        public CgwBaseRule(CanGatewayType gwType, CgwCanFrameType canFrameType)
        {
            GatewayType = gwType;
            IsCanFdRule = canFrameType == CgwCanFrameType.CANFD;
        }
    }
}