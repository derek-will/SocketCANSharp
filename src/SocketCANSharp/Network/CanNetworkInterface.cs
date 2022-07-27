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

using System;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace SocketCANSharp.Network
{
    /// <summary>
    /// Provides information about a CAN interface.
    /// </summary>
    public class CanNetworkInterface
    {
        private const string CanInterfaceStartsWith = "can";
        private const string VirtualCanInterfaceStartsWith = "vcan";

        /// <summary>
        /// Index of the CAN interface.
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// Name of the CAN interface.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Indicates if the CAN interface is virtual or not.
        /// </summary>
        public bool IsVirtual { get; set; }

        /// <summary>
        /// Initializes a new instance of the CanNetworkInterface class with the specified Index, Name and whether the interface is virtual or physical.
        /// </summary>
        /// <param name="index">Interface index</param>
        /// <param name="name">Interface name</param>
        /// <param name="isVirtual">If true, the interface is virtual. If false, the interface is physical.</param>
        public CanNetworkInterface(int index, string name, bool isVirtual)
        {
            Index = index;
            Name = name;
            IsVirtual = isVirtual;
        }

        /// <summary>
        /// Retrieves all CAN interfaces connected on the local system and optionally virtual interfaces as well.
        /// </summary>
        /// <param name="includeVirtual">Indicates whether virtual interfaces should be included or not.</param>
        /// <returns>Collection of CAN interfaces on the local system</returns>
        public static IEnumerable<CanNetworkInterface> GetAllInterfaces(bool includeVirtual)
        {
            IntPtr ptr = LibcNativeMethods.IfNameIndex();
            if (ptr == IntPtr.Zero)
            {
                throw new NetworkInformationException(LibcNativeMethods.Errno);
            }

            var ifList = new List<CanNetworkInterface>();
            IntPtr iteratorPtr = ptr;
            try
            {            
                IfNameIndex i = Marshal.PtrToStructure<IfNameIndex>(iteratorPtr);
                while (i.Index != 0 && i.Name != null)
                {
                    if (i.Name != null)
                    {
                        if (i.Name.StartsWith(CanInterfaceStartsWith))
                        {
                            var canInterface = new CanNetworkInterface((int)i.Index, i.Name, false);
                            ifList.Add(canInterface);
                        }

                        if (includeVirtual && i.Name.StartsWith(VirtualCanInterfaceStartsWith))
                        {
                            var canInterface = new CanNetworkInterface((int)i.Index, i.Name, true);
                            ifList.Add(canInterface);
                        }
                    }

                    iteratorPtr = IntPtr.Add(iteratorPtr, Marshal.SizeOf(typeof(IfNameIndex)));
                    i = Marshal.PtrToStructure<IfNameIndex>(iteratorPtr);
                }
            }
            finally
            {
                LibcNativeMethods.IfFreeNameIndex(ptr);
            }

            return ifList;
        }

        /// <summary>
        /// Looks up and creates a CanNetworkInterface instance from the interface name.
        /// </summary>
        /// <param name="socketHandle">Socket Handle</param>
        /// <param name="interfaceName">Interface Name</param>
        /// <returns>CanNetworkInterface instance with the corresponding name.</returns>
        /// <exception cref="ArgumentNullException">Socket Handle is null.</exception>
        /// <exception cref="ArgumentException">Socket Handle is closed or invalid.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Interface Name is null, empty, or only whitespace characters.</exception>
        /// <exception cref="NetworkInformationException">Failed to look up interface by name.</exception>
        public static CanNetworkInterface GetInterfaceByName(SafeFileDescriptorHandle socketHandle, string interfaceName)
        {
            if (socketHandle == null)
                throw new ArgumentNullException(nameof(socketHandle));
            
            if (socketHandle.IsClosed || socketHandle.IsInvalid)
                throw new ArgumentException("Socket handle must be open and valid", nameof(socketHandle));

            if (string.IsNullOrWhiteSpace(interfaceName))
                throw new ArgumentOutOfRangeException("Interface Name can not by null, empty, or only whitespace characters.", nameof(interfaceName));

            var ifr = new Ifreq(interfaceName);
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFINDEX, ifr);
            if (ioctlResult == -1)
                throw new NetworkInformationException(LibcNativeMethods.Errno);

            return new CanNetworkInterface(ifr.IfIndex, ifr.Name, ifr.Name.StartsWith(VirtualCanInterfaceStartsWith));
        }

        /// <summary>
        /// Looks up and creates a CanNetworkInterface instance from the interface index.
        /// </summary>
        /// <param name="socketHandle">Socket Handle</param>
        /// <param name="interfaceIndex">Interface Index</param>
        /// <returns>CanNetworkInterface instance with the corresponding index.</returns>
        /// <exception cref="ArgumentNullException">Socket Handle is null.</exception>
        /// <exception cref="ArgumentException">Socket Handle is closed or invalid.</exception>
        /// <exception cref="NetworkInformationException">Failed to look up interface by index.</exception>
        public static CanNetworkInterface GetInterfaceByIndex(SafeFileDescriptorHandle socketHandle, int interfaceIndex)
        {
            if (socketHandle == null)
                throw new ArgumentNullException(nameof(socketHandle));
            
            if (socketHandle.IsClosed || socketHandle.IsInvalid)
                throw new ArgumentException("Socket handle must be open and valid", nameof(socketHandle));

            var ifr = new Ifreq(interfaceIndex);
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFNAME, ifr);
            if (ioctlResult == -1)
                throw new NetworkInformationException(LibcNativeMethods.Errno);

            return new CanNetworkInterface(ifr.IfIndex, ifr.Name, ifr.Name.StartsWith(VirtualCanInterfaceStartsWith));
        }

        /// <summary>
        /// Retreives the Maximum Transmission Unit (MTU) of the interface.
        /// </summary>
        /// <param name="socketHandle">Socket Handle</param>
        /// <returns>Maximum Transmission Unit of the interface.</returns>
        /// <exception cref="NetworkInformationException">Unable to retreive MTU size information for the interface.</exception>
        public int ReadSupportedMtu(SafeFileDescriptorHandle socketHandle)
        {
            if (socketHandle == null)
                throw new ArgumentNullException(nameof(socketHandle));
            
            if (socketHandle.IsClosed || socketHandle.IsInvalid)
                throw new ArgumentException("Socket handle must be open and valid", nameof(socketHandle));

            var ifr = new IfreqMtu(Name);
            int ioctlResult = LibcNativeMethods.Ioctl(socketHandle, SocketCanConstants.SIOCGIFMTU, ifr);
            if (ioctlResult == -1)
                throw new NetworkInformationException(LibcNativeMethods.Errno);

            return ifr.MTU;
        }

        /// <summary>
        /// Returns a string that represents the current CanNetworkInterface object.
        /// </summary>
        /// <returns>A string that represents the current CanNetworkInterface object.</returns>
        public override string ToString()
        {
            return $"Index: {Index}; Name: {Name}; Is Virtual: {IsVirtual}";
        }
    }
}