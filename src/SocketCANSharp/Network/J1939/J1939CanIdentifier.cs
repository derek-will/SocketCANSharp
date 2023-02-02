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
    /// SAE J1939 CAN ID.
    /// </summary>
    public class J1939CanIdentifier
    {
        private byte _priority;

        /// <summary>
        /// The address of the device which transmitted the message.
        /// </summary>
        public byte SourceAddress { get; set; }

        /// <summary>
        /// The Parameter Group Number of the message.
        /// </summary>
        public ParameterGroupNumber ParameterGroupNumber { get; set; }

        /// <summary>
        /// The priority of the message during the arbitration process. Value of 0 is the highest priority and 7 is the lowest priority. 
        /// </summary>
        public byte Priority
        {
            get
            {
                return _priority;
            }
            set
            {
                if (value > 7)
                    throw new ArgumentOutOfRangeException(nameof(value), "Priority field cannot exceed 7.");

                _priority = value;
            }
        }

        /// <summary>
        /// Gets the raw value of the CAN ID based on the configured properties of the current object.
        /// </summary>
        public uint RawValue
        {
            get
            {
                return GenerateRawValue();
            }
        }

        /// <summary>
        /// Initializes a new instance of the J1939CanIdentifier class.
        /// </summary>
        public J1939CanIdentifier()
        {
            ParameterGroupNumber = new ParameterGroupNumber();
        }

        /// <summary>
        /// Initializes a new instance of the J1939CanIdentifier class using the raw CAN ID.
        /// </summary>
        /// <param name="rawCanId">Raw CAN ID</param>
        /// <exception cref="ArgumentOutOfRangeException">Raw CAN ID exceeds 29-bit in length.</exception>
        public J1939CanIdentifier(uint rawCanId)
        {
            if (rawCanId > SocketCanConstants.CAN_EFF_MASK)
                throw new ArgumentOutOfRangeException(nameof(rawCanId), $"Raw CAN ID cannot exceed {SocketCanConstants.CAN_EFF_MASK}");

            SourceAddress = (byte)(rawCanId & 0xFF);
            ParameterGroupNumber = new ParameterGroupNumber((uint)((rawCanId >> 8) & SocketCanConstants.J1939_PGN_MAX));
            Priority = (byte)((rawCanId >> 26) & 0x07);
        }

        private uint GenerateRawValue()
        {
            uint rawValue = 0;
            rawValue |= SourceAddress;
            rawValue |= (uint)(ParameterGroupNumber.RawValue << 8);
            rawValue |= (uint)(Priority << 26);
            return rawValue;
        }
    }
}