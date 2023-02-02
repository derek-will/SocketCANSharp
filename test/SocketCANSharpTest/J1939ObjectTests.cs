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
using NUnit.Framework;
using SocketCANSharp.J1939;

namespace SocketCANSharpTest
{
    public class J1939ObjectTests
    {
        [Test]
        public void ParameterGroupNumber_Success_Test()
        {
            var pgn = new ParameterGroupNumber(0x0F004);
            Assert.AreEqual(0x04, pgn.PduSpecific);
            Assert.AreEqual(0xf0, pgn.PduFormat);
            Assert.AreEqual(0x00, pgn.DataPage);
            Assert.AreEqual(0x00, pgn.Reserved);
            Assert.AreEqual(J1939MessageType.PDU2, pgn.MessageType);
            Assert.AreEqual(0x0F004, pgn.RawValue);
        }

        [Test]
        public void ParameterGroupNumber_Success_2_Test()
        {
            var pgn = new ParameterGroupNumber(0x1100A);
            Assert.AreEqual(0x0A, pgn.PduSpecific);
            Assert.AreEqual(0x10, pgn.PduFormat);
            Assert.AreEqual(0x01, pgn.DataPage);
            Assert.AreEqual(0x00, pgn.Reserved);
            Assert.AreEqual(J1939MessageType.PDU1, pgn.MessageType);
            Assert.AreEqual(0x1100A, pgn.RawValue);
        }

        [Test]
        public void ParameterGroupNumber_Success_3_Test()
        {
            var pgn = new ParameterGroupNumber(0x3F34A);
            Assert.AreEqual(0x4A, pgn.PduSpecific);
            Assert.AreEqual(0xf3, pgn.PduFormat);
            Assert.AreEqual(0x01, pgn.DataPage);
            Assert.AreEqual(0x01, pgn.Reserved);
            Assert.AreEqual(J1939MessageType.PDU2, pgn.MessageType);
            Assert.AreEqual(0x3F34A, pgn.RawValue);
        }

        [Test]
        public void ParameterGroupNumber_Exception_PGN_OutOfRange_Ctor_Test()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ParameterGroupNumber(0x40000));
        }

        [Test]
        public void ParameterGroupNumber_Exception_OutOfRange_DataPage_Property_Test()
        {
            var pgn = new ParameterGroupNumber();
            Assert.Throws<ArgumentOutOfRangeException>(() => pgn.DataPage = 2);
        }

        [Test]
        public void ParameterGroupNumber_Exception_OutOfRange_Reserved_Property_Test()
        {
            var pgn = new ParameterGroupNumber();
            Assert.Throws<ArgumentOutOfRangeException>(() => pgn.Reserved = 2);
        }

        [Test]
        public void J1939CanIdentifier_Success_Test()
        {
            var j1939CanId = new J1939CanIdentifier(0xCF004EE);
            Assert.AreEqual(0xEE, j1939CanId.SourceAddress);
            Assert.AreEqual(0x04, j1939CanId.ParameterGroupNumber.PduSpecific);
            Assert.AreEqual(0xf0, j1939CanId.ParameterGroupNumber.PduFormat);
            Assert.AreEqual(0x00, j1939CanId.ParameterGroupNumber.DataPage);
            Assert.AreEqual(0x00, j1939CanId.ParameterGroupNumber.Reserved);
            Assert.AreEqual(J1939MessageType.PDU2, j1939CanId.ParameterGroupNumber.MessageType);
            Assert.AreEqual(0x03, j1939CanId.Priority);
            Assert.AreEqual(0xCF004EE, j1939CanId.RawValue);
        }

        [Test]
        public void J1939CanIdentifier_Exception_OutOfRange_RawCanId_Test()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new J1939CanIdentifier(0x20FF00FF));
        }

        [Test]
        public void J1939CanIdentifier_Exception_OutOfRange_Priority_Property_Test()
        {
            var j1939CanIdentifier = new J1939CanIdentifier();
            Assert.Throws<ArgumentOutOfRangeException>(() => j1939CanIdentifier.Priority = 8);
        }

        [Test]
        public void J1939Name_Success_Test()
        {
            var j1939Name = new J1939Name(0xD2DA9C2B08012345);
            Assert.AreEqual(0x12345, j1939Name.IdentityNumber);
            Assert.AreEqual(0x40, j1939Name.ManufacturerCode);
            Assert.AreEqual(0x03, j1939Name.EcuInstance);
            Assert.AreEqual(0x05, j1939Name.FunctionInstance);
            Assert.AreEqual(0x9C, j1939Name.Function);
            Assert.AreEqual(0x00, j1939Name.Reserved);
            Assert.AreEqual(0x6D, j1939Name.VehicleSystem);
            Assert.AreEqual(0x02, j1939Name.VehicleSystemInstance);
            Assert.AreEqual(0x05, j1939Name.IndustryGroup);
            Assert.AreEqual(true, j1939Name.ArbitraryAddressCapable);
            Assert.AreEqual(0xD2DA9C2B08012345, j1939Name.RawValue);
        }

        [Test]
        public void J1939Name_Exception_OutOfRange_IdentityNumber_Property_Test()
        {
            var j1939Name = new J1939Name();
            Assert.Throws<ArgumentOutOfRangeException>(() => j1939Name.IdentityNumber = 0x3FFFFF);
        }

        [Test]
        public void J1939Name_Exception_OutOfRange_ManufacturerCode_Property_Test()
        {
            var j1939Name = new J1939Name();
            Assert.Throws<ArgumentOutOfRangeException>(() => j1939Name.ManufacturerCode = 0xFFF);
        }

        [Test]
        public void J1939Name_Exception_OutOfRange_EcuInstance_Property_Test()
        {
            var j1939Name = new J1939Name();
            Assert.Throws<ArgumentOutOfRangeException>(() => j1939Name.EcuInstance = 0x0F);
        }

        [Test]
        public void J1939Name_Exception_OutOfRange_FunctionInstance_Property_Test()
        {
            var j1939Name = new J1939Name();
            Assert.Throws<ArgumentOutOfRangeException>(() => j1939Name.FunctionInstance = 0x3F);
        }

        [Test]
        public void J1939Name_Exception_OutOfRange_Reserved_Property_Test()
        {
            var j1939Name = new J1939Name();
            Assert.Throws<ArgumentOutOfRangeException>(() => j1939Name.Reserved = 0x03);
        }

        [Test]
        public void J1939Name_Exception_OutOfRange_VehicleSystem_Property_Test()
        {
            var j1939Name = new J1939Name();
            Assert.Throws<ArgumentOutOfRangeException>(() => j1939Name.VehicleSystem = 0xFF);
        }

        [Test]
        public void J1939Name_Exception_OutOfRange_VehicleSystemInstance_Property_Test()
        {
            var j1939Name = new J1939Name();
            Assert.Throws<ArgumentOutOfRangeException>(() => j1939Name.VehicleSystemInstance = 0x1F);
        }

        [Test]
        public void J1939Name_Exception_OutOfRange_IndustryGroup_Property_Test()
        {
            var j1939Name = new J1939Name();
            Assert.Throws<ArgumentOutOfRangeException>(() => j1939Name.IndustryGroup = 0x0F);
        }
    }
}