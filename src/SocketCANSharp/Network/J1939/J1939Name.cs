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
    /// SAE J1939 Name.
    /// </summary>
    public class J1939Name
    {
        private uint _identityNumber;
        private ushort _manufacturerCode;
        private byte _ecuInstance;
        private byte _functionInstance;
        private byte _reserved;
        private byte _vehicleSystem;
        private byte _vehicleSystemInstance;
        private byte _industryGroup;

        /// <summary>
        /// Set by the manufacturer. Much like a serial number, but be unique per unit.
        /// </summary>
        public uint IdentityNumber 
        {
            get
            {
                return _identityNumber;
            }
            set
            {
                if (value > 0x1FFFFF)
                    throw new ArgumentOutOfRangeException(nameof(value), "Identity Number cannot exceed 21 bits in length.");

                _identityNumber = value;
            }
        }

        /// <summary>
        /// Code assigned by the SAE to identify the device manufacturer.
        /// </summary>
        public ushort ManufacturerCode 
        { 
            get
            {
                return _manufacturerCode;
            } 
            set
            {
                if (value > 0x7FF)
                    throw new ArgumentOutOfRangeException(nameof(value), "Manufacturer Code cannot exceed 11 bits in length.");

                _manufacturerCode = value;
            }
        }

        /// <summary>
        /// Used to accommodate situations where several ECUs compose a single functionality. This code is what separates them.
        /// </summary>
        public byte EcuInstance 
        { 
            get
            {
                return _ecuInstance;
            }
            set
            {
                if (value > 0x07)
                    throw new ArgumentOutOfRangeException(nameof(value), "ECU Instance cannot exceed 3 bits in length.");

                _ecuInstance = value;
            }
        }
        
        /// <summary>
        /// Identifies the instance of the function (i.e., TCM #1). 
        /// </summary>
        public byte FunctionInstance 
        { 
            get
            {
                return _functionInstance;
            }
            set
            {
                if (value > 0x1F)
                    throw new ArgumentOutOfRangeException(nameof(value), "Function Instance cannot exceed 5 bits in length.");

                _functionInstance = value;
            }
        }
        
        /// <summary>
        /// Code which identifies the function according to the Industry Group (i.e., TCM).
        /// </summary>
        public byte Function { get; set; }

        /// <summary>
        /// Reserved and should always be set to zero.
        /// </summary>
        public byte Reserved 
        { 
            get
            {
                return _reserved;
            }
            set
            {
                if (value > 0x01)
                    throw new ArgumentOutOfRangeException(nameof(value), "Reserved cannot exceed 1 bit in length.");

                _reserved = value;
            } 
        }

        /// <summary>
        /// Identifies the vehicle system associated with the Industry Group (i.e. Trailer).
        /// </summary>
        public byte VehicleSystem 
        { 
            get
            {
                return _vehicleSystem;
            } 
            set
            {
                if (value > 0x7F)
                    throw new ArgumentOutOfRangeException(nameof(value), "Vehicle System cannot exceed 7 bits in length.");

                _vehicleSystem = value;
            }
        }

        /// <summary>
        /// Identifies a particular occurrence of a vehicle system (i.e., Trailer #2).
        /// </summary>
        public byte VehicleSystemInstance 
        { 
            get
            {
                return _vehicleSystemInstance;
            }
            set
            {
                if (value > 0x0F)
                    throw new ArgumentOutOfRangeException(nameof(value), "Vehicle System cannot exceed 4 bits in length.");

                _vehicleSystemInstance = value;
            }
        }

        /// <summary>
        /// Code associated with a particular industry (i.e., On-Highway)
        /// </summary>
        public byte IndustryGroup 
        { 
            get
            {
                return _industryGroup;
            }
            set
            {
                if (value > 0x07)
                    throw new ArgumentOutOfRangeException(nameof(value), "Vehicle System cannot exceed 4 bits in length.");

                _industryGroup = value;
            } 
        }

        /// <summary>
        /// Indicates whether or not the ECU can negotiate an address. 
        /// Some ECUs only support a single address while others support a range of addresses.
        /// </summary>
        public bool ArbitraryAddressCapable { get; set; }
        
        /// <summary>
        /// Gets the raw value of the J1939 Name based on the configured properties of the current object.
        /// </summary>
        public ulong RawValue
        {
            get
            {
                return GenerateRawValue();
            }
        }

        /// <summary>
        /// Initializes a new instance of the J1939Name class.
        /// </summary>
        public J1939Name()
        {
        }

        /// <summary>
        /// Initializes a new instance of the J1939Name class using the raw 64-bit NAME value.
        /// </summary>
        /// <param name="name">Raw NAME value</param>
        public J1939Name(ulong name)
        {
            IdentityNumber = (uint)(name & 0x1FFFFF);
            ManufacturerCode = (ushort)(name >> 21 & 0x7FF);
            EcuInstance = (byte)(name >> 32 & 0x07);
            FunctionInstance = (byte)(name >> 35 & 0x1F);
            Function = (byte)(name >> 40 & 0xFF);
            Reserved = (byte)(name >> 48 & 0x01);
            VehicleSystem = (byte)(name >> 49 & 0x7F);
            VehicleSystemInstance = (byte)(name >> 56 & 0x0F);
            IndustryGroup = (byte)(name >> 60 & 0x07);
            ArbitraryAddressCapable = (name >> 63 & 0x01) == 0x01;
        }

        private ulong GenerateRawValue()
        {
            ulong rawValue = 0;
            rawValue |= IdentityNumber;
            rawValue |= ((ulong)ManufacturerCode << 21);
            rawValue |= ((ulong)EcuInstance << 32);
            rawValue |= ((ulong)FunctionInstance << 35);
            rawValue |= ((ulong)Function << 40);
            rawValue |= ((ulong)Reserved << 48);
            rawValue |= ((ulong)VehicleSystem << 49);
            rawValue |= ((ulong)VehicleSystemInstance << 56);
            rawValue |= ((ulong)IndustryGroup << 60);
            rawValue |= ((ulong)(ArbitraryAddressCapable ? 1 : 0) << 63);
            return rawValue;
        }
    }
}