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

namespace SocketCANSharp.J1939
{
    using System;

    /// <summary>
    /// SAE J1939 Parameter Group Number.
    /// </summary>
    public class ParameterGroupNumber
    {
        private byte _reserved;
        private byte _dataPage;

        /// <summary>
        /// Single bit which is reserved for future use. Should be set to 0 for transmitted messages.
        /// </summary>
        public byte Reserved 
        {
            get
            {
                return _reserved;
            }
            set
            {
                if (value > 1)
                    throw new ArgumentOutOfRangeException(nameof(value), "Reserved field of PGN must be set to 0 or 1.");

                _reserved = value;
            }
        }

        /// <summary>
        /// Single bit which is the data page selector and expands the number of possible Parameter Groups that can be represented.
        /// </summary>
        public byte DataPage 
        { 
            get
            {
                return _dataPage;
            }
            set
            {
                if (value > 1)
                    throw new ArgumentOutOfRangeException(nameof(value), "Data Page field of PGN must be set to 0 or 1.");

                _dataPage = value;
            }
        }

        /// <summary>
        /// If the PDU Format is between 0x00 and 0xEF then the message type is PDU1 (Addressable Message) and PDU Specific field contains the destination address.
        /// If the PDU Format is between 0xF0 and 0xFF then the message type is PDU2 (Broadcast Message) and PDU Specific field contains a Group Extension.
        /// </summary>
        public byte PduFormat { get; set; }

        /// <summary>
        /// PDU Specific field is either the destination address or a Group Extension depending of the PDU Format (PDU1 versus PDU2).
        /// </summary>
        public byte PduSpecific { get; set; }

        /// <summary>
        /// J1939 Message Type derived from the PDU Format value.
        /// </summary>
        public J1939MessageType MessageType
        {
            get
            {
                return PduFormat < 0xF0 ? J1939MessageType.PDU1 : J1939MessageType.PDU2;
            }
        }

        /// <summary>
        /// Gets the raw value of the PGN based on the configured properties of the current object.
        /// </summary>
        public uint RawValue
        {
            get
            {
                return GenerateRawValue();
            }
        }

        /// <summary>
        /// Initializes a new instance of the ParameterGroupNumber class.
        /// </summary>
        public ParameterGroupNumber()
        {
        }

        /// <summary>
        /// Initializes a new instance of the ParameterGroupNumber class using the raw PGN value.
        /// </summary>
        /// <param name="pgn">Raw Parameter Group Number</param>
        /// <exception cref="ArgumentOutOfRangeException">Raw PGN value exceeds allowable limits</exception>
        public ParameterGroupNumber(uint pgn)
        {
            if (pgn > SocketCanConstants.J1939_PGN_MAX)
                throw new ArgumentOutOfRangeException(nameof(pgn), $"Raw PGN value cannot exceed {SocketCanConstants.J1939_PGN_MAX}");

            PduSpecific = (byte)(pgn & 0xFF);
            PduFormat = (byte)((pgn >> 8) & 0xFF);
            DataPage = (byte)((pgn >> 16) & 0x01);
            Reserved = (byte)((pgn >> 17) & 0x01);
        }

        private uint GenerateRawValue()
        {
            uint rawValue = 0;
            rawValue |= PduSpecific;
            rawValue |= (uint)(PduFormat << 8);
            rawValue |= (uint)(DataPage << 16);
            rawValue |= (uint)(Reserved << 17);
            return rawValue;
        }
    }
}