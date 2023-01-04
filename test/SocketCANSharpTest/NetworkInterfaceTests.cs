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

using NUnit.Framework;
using SocketCANSharp;
using SocketCANSharp.Network;
using SocketCANSharp.Network.Netlink;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace SocketCANSharpTest
{
    public class NetworkInterfaceTests
    {
        SafeFileDescriptorHandle socketHandle;

        [SetUp]
        public void Setup()
        {
        }

        [TearDown]
        public void Cleanup()
        {
            socketHandle.Close();
        }

        [Test]
        public void GetListOfNetworkInterfaces_Success_Test()
        {
            IntPtr ptr = LibcNativeMethods.IfNameIndex();
            Assert.AreNotEqual(IntPtr.Zero, ptr);

            try
            {
                IntPtr iteratorPtr = ptr;
                IfNameIndex i = Marshal.PtrToStructure<IfNameIndex>(ptr);
                while (i.Index != 0 && i.Name != null)
                {
                    Console.WriteLine(i);
                    iteratorPtr = IntPtr.Add(iteratorPtr, Marshal.SizeOf(typeof(IfNameIndex)));
                    i = Marshal.PtrToStructure<IfNameIndex>(iteratorPtr);
                }
            }
            finally
            {
                LibcNativeMethods.IfFreeNameIndex(ptr);
            }
        }

        [Test]
        public void GetListOfCanNetworkInterfaces_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);
            foreach (var canInterface in collection)
            {
                Console.WriteLine(canInterface);
                Assert.Greater(canInterface.Index, 0);
                Assert.IsNotNull(canInterface.Name);
            }
        }

        [Test]
        public void CanNetworkInterface_MtuOfInterface_Success_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            int mtu = iface.ReadSupportedMtu(socketHandle);
            Assert.That(mtu, Is.EqualTo(SocketCanConstants.CAN_MTU) | Is.EqualTo(SocketCanConstants.CANFD_MTU)); // some devices may support CAN FD while other may support Classic CAN only
        }

        [Test]
        public void CanNetworkInterface_MtuOfInterface_ClosedOrInvalid_SafeFileDescriptorHandle_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            socketHandle.Close();
            Assert.IsTrue(socketHandle.IsInvalid);

            Assert.Throws<ArgumentException>(() => iface.ReadSupportedMtu(socketHandle));
        }

        [Test]
        public void CanNetworkInterface_MtuOfInterface_Null_SafeFileDescriptorHandle_Failure_Test()
        {
            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            Assert.Throws<ArgumentNullException>(() => iface.ReadSupportedMtu(null));
        }

        [Test]
        public void CanNetworkInterface_GetInterfaceByName_Success_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            CanNetworkInterface iface = CanNetworkInterface.GetInterfaceByName(socketHandle, "vcan0");
            Assert.AreEqual("vcan0", iface.Name);
            Assert.AreEqual(true, iface.IsVirtual);
            Assert.Greater(iface.Index, -1);
        }

        [Test]
        public void CanNetworkInterface_GetInterfaceByName_Null_SocketHandle_Failure_Test()
        {
            Assert.Throws<ArgumentNullException>(() => CanNetworkInterface.GetInterfaceByName(null, "vcan0"));
        }

        [Test]
        public void CanNetworkInterface_GetInterfaceByName_WhitespaceChars_Failure_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            Assert.Throws<ArgumentOutOfRangeException>(() => CanNetworkInterface.GetInterfaceByName(socketHandle, "  "));
        }

        [Test]
        public void CanNetworkInterface_GetInterfaceByName_Null_Failure_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            Assert.Throws<ArgumentOutOfRangeException>(() => CanNetworkInterface.GetInterfaceByName(socketHandle, null));
        }

        [Test]
        public void CanNetworkInterface_GetInterfaceByName_Closed_Socket_Failure_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            socketHandle.Close();
            Assert.IsTrue(socketHandle.IsInvalid);

            Assert.Throws<ArgumentException>(() => CanNetworkInterface.GetInterfaceByName(socketHandle, "vcan0"));
        }

        [Test]
        public void CanNetworkInterface_GetInterfaceByName_Invalid_Name_Failure_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            Assert.Throws<NetworkInformationException>(() => CanNetworkInterface.GetInterfaceByName(socketHandle, "vcan555"));
        }

        [Test]
        public void CanNetworkInterface_GetInterfaceByIndex_Success_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            CanNetworkInterface iface = CanNetworkInterface.GetInterfaceByName(socketHandle, "vcan0");
            Assert.AreEqual("vcan0", iface.Name);
            Assert.AreEqual(true, iface.IsVirtual);
            Assert.Greater(iface.Index, -1);

            CanNetworkInterface iface2 = CanNetworkInterface.GetInterfaceByIndex(socketHandle, iface.Index);
            Assert.AreEqual("vcan0", iface2.Name);
            Assert.AreEqual(true, iface2.IsVirtual);
            Assert.AreEqual(iface2.Index, iface.Index);
        }

        [Test]
        public void CanNetworkInterface_GetInterfaceByIndex_Null_SocketHandle_Failure_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            CanNetworkInterface iface = CanNetworkInterface.GetInterfaceByName(socketHandle, "vcan0");
            Assert.AreEqual("vcan0", iface.Name);
            Assert.AreEqual(true, iface.IsVirtual);
            Assert.Greater(iface.Index, -1);

            Assert.Throws<ArgumentNullException>(() => CanNetworkInterface.GetInterfaceByIndex(null, iface.Index));
        }

        [Test]
        public void CanNetworkInterface_GetInterfaceByIndex_Closed_Socket_Failure_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            CanNetworkInterface iface = CanNetworkInterface.GetInterfaceByName(socketHandle, "vcan0");
            Assert.AreEqual("vcan0", iface.Name);
            Assert.AreEqual(true, iface.IsVirtual);
            Assert.Greater(iface.Index, -1);

            socketHandle.Close();
            Assert.IsTrue(socketHandle.IsInvalid);

            Assert.Throws<ArgumentException>(() => CanNetworkInterface.GetInterfaceByIndex(socketHandle, iface.Index));
        }

        [Test]
        public void CanNetworkInterface_GetInterfaceByIndex_Invalid_Index_Failure_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            Assert.Throws<NetworkInformationException>(() => CanNetworkInterface.GetInterfaceByIndex(socketHandle, -200));
        }

        [Test]
        public void Bind_CAN_RAW_on_vcan0_CanNetworkInterface_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            var addr = new SockAddrCan(iface.Index);
            int bindResult = LibcNativeMethods.Bind(socketHandle, addr, Marshal.SizeOf(typeof(SockAddrCan)));
            Assert.AreNotEqual(-1, bindResult);
        }

        [Test]
        public void Bind_CAN_ISOTP_on_vcan0_CanNetworkInterface_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_ISOTP);
            if (socketHandle.IsInvalid)
            {
                Assume.That(LibcNativeMethods.Errno, Is.Not.EqualTo(93) & Is.Not.EqualTo(22)); // If EPROTONOSUPPORT, then this protocol is not supported on this platform and no futher testing applies. If EINVAL, then Protocol Type is not being recognized as valid.
            }
            Assert.IsFalse(socketHandle.IsInvalid);

            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            var addr = new SockAddrCanIsoTp(iface.Index)
            {
                TxId = 0x7e0,
                RxId = 0x7e8,
            };
            int bindResult = LibcNativeMethods.Bind(socketHandle, addr, Marshal.SizeOf(typeof(SockAddrCanIsoTp)));
            Assert.AreNotEqual(-1, bindResult);
        }

        [Test]
        public void Connect_CAN_BCM_on_vcan0_CanNetworkInterface_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_BCM);
            Assert.IsFalse(socketHandle.IsInvalid);

            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            var addr = new SockAddrCan(iface.Index);
            int connectResult = LibcNativeMethods.Connect(socketHandle, addr, Marshal.SizeOf(typeof(SockAddrCan)));
            Assert.AreNotEqual(-1, connectResult);
        }

        [Test]
        public void Bind_CAN_J1939_on_vcan0_CanNetworkInterface_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Dgram, SocketCanProtocolType.CAN_J1939);
            if (socketHandle.IsInvalid)
            {
                Assume.That(LibcNativeMethods.Errno, Is.Not.EqualTo(93) & Is.Not.EqualTo(22)); // If EPROTONOSUPPORT, then this protocol is not supported on this platform and no futher testing applies. If EINVAL, then Protocol Type is not being recognized as valid.
            }
            Assert.IsFalse(socketHandle.IsInvalid);

            IEnumerable<CanNetworkInterface> collection = CanNetworkInterface.GetAllInterfaces(true);
            Assert.IsNotNull(collection);
            Assert.GreaterOrEqual(collection.Count(), 1);

            var iface = collection.FirstOrDefault(i =>  i.Name.Equals("vcan0"));
            Assert.IsNotNull(iface);

            var addr = new SockAddrCanJ1939(iface.Index)
            {
                Name = 0x00000000,
                PGN = 0x40000,
                Address = 0xFF,
            };
            int bindResult = LibcNativeMethods.Bind(socketHandle, addr, Marshal.SizeOf(typeof(SockAddrCanJ1939)));
            Assert.AreNotEqual(-1, bindResult);
        }

        [Test]
        public void CanNetworkInterface_Get_BitTiming_Success_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            CanNetworkInterface iface = CanNetworkInterface.GetInterfaceByName(socketHandle, "vcan0");
            Assert.AreEqual("vcan0", iface.Name);
            Assert.AreEqual(true, iface.IsVirtual);
            Assert.Greater(iface.Index, -1);

            CanBitTiming bitTiming = iface.BitTiming;
            Assert.IsNull(bitTiming);
        }

        [Test]
        public void CanNetworkInterface_Get_BitTimingConstant_Success_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            CanNetworkInterface iface = CanNetworkInterface.GetInterfaceByName(socketHandle, "vcan0");
            Assert.AreEqual("vcan0", iface.Name);
            Assert.AreEqual(true, iface.IsVirtual);
            Assert.Greater(iface.Index, -1);

            CanBitTimingConstant bitTimingConstant = iface.BitTimingConstant;
            Assert.IsNull(bitTimingConstant);
        }

        [Test]
        public void CanNetworkInterface_Get_OperationalStatus_Success_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            CanNetworkInterface iface = CanNetworkInterface.GetInterfaceByName(socketHandle, "vcan0");
            Assert.AreEqual("vcan0", iface.Name);
            Assert.AreEqual(true, iface.IsVirtual);
            Assert.Greater(iface.Index, -1);

            InterfaceOperationalStatus opStatus = iface.OperationalStatus;
            Assert.AreEqual(InterfaceOperationalStatus.IF_OPER_UNKNOWN, opStatus);
        }

        [Test]
        public void CanNetworkInterface_Get_LinkStatistics_Success_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            CanNetworkInterface iface = CanNetworkInterface.GetInterfaceByName(socketHandle, "vcan0");
            Assert.AreEqual("vcan0", iface.Name);
            Assert.AreEqual(true, iface.IsVirtual);
            Assert.Greater(iface.Index, -1);

            InterfaceLinkStatistics64 linkStatistics64 = iface.LinkStatistics;
            Assert.IsNotNull(linkStatistics64);
        }

        [Test]
        public void CanNetworkInterface_Get_LinkKind_Success_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            CanNetworkInterface iface = CanNetworkInterface.GetInterfaceByName(socketHandle, "vcan0");
            Assert.AreEqual("vcan0", iface.Name);
            Assert.AreEqual(true, iface.IsVirtual);
            Assert.Greater(iface.Index, -1);

            string kind = iface.LinkKind;
            Assert.AreEqual("vcan", kind);
        }

        [Test]
        public void CanNetworkInterface_Get_DeviceStatistics_Success_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            CanNetworkInterface iface = CanNetworkInterface.GetInterfaceByName(socketHandle, "vcan0");
            Assert.AreEqual("vcan0", iface.Name);
            Assert.AreEqual(true, iface.IsVirtual);
            Assert.Greater(iface.Index, -1);

            CanDeviceStatistics stats = iface.DeviceStatistics;
            Assert.IsNull(stats);
        }

        [Test]
        public void CanNetworkInterface_Get_DeviceType_Success_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            CanNetworkInterface iface = CanNetworkInterface.GetInterfaceByName(socketHandle, "vcan0");
            Assert.AreEqual("vcan0", iface.Name);
            Assert.AreEqual(true, iface.IsVirtual);
            Assert.Greater(iface.Index, -1);

            ArpHardwareIdentifier hwId = iface.DeviceType;
            Assert.AreEqual(ArpHardwareIdentifier.ARPHRD_CAN, hwId);
        }

        [Test]
        public void CanNetworkInterface_Get_DeviceFlags_Success_Test()
        {
            socketHandle = LibcNativeMethods.Socket(SocketCanConstants.PF_CAN, SocketType.Raw, SocketCanProtocolType.CAN_RAW);
            Assert.IsFalse(socketHandle.IsInvalid);

            CanNetworkInterface iface = CanNetworkInterface.GetInterfaceByName(socketHandle, "vcan0");
            Assert.AreEqual("vcan0", iface.Name);
            Assert.AreEqual(true, iface.IsVirtual);
            Assert.Greater(iface.Index, -1);

            NetDeviceFlags deviceFlags = iface.DeviceFlags;
            Assert.IsTrue(deviceFlags.HasFlag(NetDeviceFlags.IFF_UP));
            Assert.IsTrue(deviceFlags.HasFlag(NetDeviceFlags.IFF_RUNNING));
            Assert.IsTrue(deviceFlags.HasFlag(NetDeviceFlags.IFF_NOARP));
            Assert.IsTrue(deviceFlags.HasFlag(NetDeviceFlags.IFF_LOWER_UP));
        }
    }
}