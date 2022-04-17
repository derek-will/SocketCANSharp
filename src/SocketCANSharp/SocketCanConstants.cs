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

namespace SocketCANSharp
{
    /// <summary>
    /// Set of SocketCAN constants
    /// </summary>
    public static class SocketCanConstants
    {
        /// <summary>
        /// Address Family CAN
        /// </summary>
        public const int AF_CAN = 29; // socket.h
        /// <summary>
        /// Protocol Family CAN
        /// </summary>
        public const int PF_CAN = 29; // socket.h  
        /// <summary>
        /// Socket Configuration Control: name -> if_index mapping
        /// </summary>
        public const int SIOCGIFINDEX = 0x8933; // ioctls.h
        /// <summary>
        /// Socket Configuration Control: get MTU size
        /// </summary>
        public const int SIOCGIFMTU = 0x8921; // ioctls.h
        /// <summary>
        /// Used to enable or disable non-blocking mode on a socket.
        /// </summary>
        public const int FIONBIO = 0x5421; // ioctls.h
        /// <summary>
        /// Valid bits in CAN ID in standard frame format (SFF).
        /// </summary>
        public const uint CAN_SFF_MASK = 0x000007FF;
        /// <summary>
        /// Valid bits in CAN ID in extended frame format (EFF).
        /// </summary>
        public const uint CAN_EFF_MASK = 0x1FFFFFFF;
        /// <summary>
        /// Used to omit EFF, RTR, ERR flags.
        /// </summary>
        public const uint CAN_ERR_MASK = 0x1FFFFFFF;
        /// <summary>
        /// Special flag to be set in the CAN ID of a CAN Filter to invert the CAN Filter.
        /// </summary>
        public const uint CAN_INV_FILTER = 0x20000000;
        /// <summary>
        /// Maximum number of can_filters that can be set via setsockopt(). 
        /// </summary>
        public const uint CAN_RAW_FILTER_MAX = 512;
        /// <summary>
        /// Maximum Transmission Unit for Classic CAN.
        /// </summary>
        public const byte CAN_MTU = 16;
        /// <summary>
        /// Maximum Transmission Unit for CAN FD.
        /// </summary>
        public const byte CANFD_MTU = 72;
        /// <summary>
        /// Maximum Data Payload for Classic CAN.
        /// </summary>
        public const byte CAN_MAX_DLEN = 8;
        /// <summary>
        /// Maximum Data Payload for CAN FD.
        /// </summary>
        public const byte CANFD_MAX_DLEN = 64;
        /// <summary>
        /// Valid CAN FD Data Payloads.
        /// </summary>
        public static readonly  byte[] CanFdFrameLengths = new byte[] { 8, 12, 16, 20, 24, 32, 48, 64 };
        /// <summary>
        /// J1939 Maximum Unicast Address.
        /// </summary>
        public const byte J1939_MAX_UNICAST_ADDR = 0xfd;
        /// <summary>
        /// J1939 Idle Address.
        /// </summary>
        public const byte J1939_IDLE_ADDR = 0xfe;
        /// <summary>
        /// J1939 Broadcast or Null Address.
        /// </summary>
        public const byte J1939_NO_ADDR = 0xff;
        /// <summary>
        /// J1939 No Name Value.
        /// </summary>
        public const ulong J1939_NO_NAME = 0;
        /// <summary>
        /// J1939 No PGN Value.
        /// </summary>
        public const uint J1939_NO_PGN = 0x40000;
        /// <summary>
        /// J1939 PGN PDU1 (Peer-to-Peer) Maximum Value
        /// </summary>
        public const uint J1939_PGN_PDU1_MAX = 0x3ff00;
        /// <summary>
        /// J1939 PGN Maximum Value (Limit).
        /// </summary>
        public const uint J1939_PGN_MAX = 0x3ffff;
    }
}
