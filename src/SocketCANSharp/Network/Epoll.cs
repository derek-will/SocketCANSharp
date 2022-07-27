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
using System.Net.Sockets;
using System.Linq;

namespace SocketCANSharp.Network
{
    /// <summary>
    /// Provides I/O event notifications via an epoll instance.
    /// </summary>
    public class Epoll : IDisposable
    {
        /// <summary>
        /// Object disposed flag.
        /// </summary>
        protected bool _disposed;

        /// <summary>
        /// Represents the operating system handle exposed in a safe manner for the epoll file descriptor that the current Epoll object encapsulates.
        /// </summary>
        public SafeFileDescriptorHandle SafeHandle { get; }

        /// <summary>
        /// Represents the operating system handle for the epoll file descriptor that is encapsulated by this object.
        /// </summary>
        public IntPtr Handle { get { return SafeHandle.DangerousGetHandle(); } }

        /// <summary>
        /// Initializes a new instance of the epoll class.
        /// </summary>
        /// <exception cref="SocketException">Failed to create a new epoll instance.</exception>
        public Epoll()
        {
            SafeHandle = LibcNativeMethods.EpollCreate(1);

            if (SafeHandle.IsInvalid)
                throw new SocketException();
        }
        
        /// <summary>
        /// Adds a socket handle to the interest list of the epoll file descriptor along with settings.
        /// </summary>
        /// <param name="socketHandle">Socket Handle to add to the interest list.</param>
        /// <param name="eventSettings">Settings associated with the socket handle.</param>
        /// <exception cref="ObjectDisposedException">Epoll file descriptor is closed.</exception>
        /// <exception cref="ArgumentNullException">Socket handle is null.</exception>
        /// <exception cref="SocketException">The epoll_ctl call failed.</exception>
        public void Add(SafeFileDescriptorHandle socketHandle, EpollEvent eventSettings)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);
            
            if (socketHandle == null)
                throw new ArgumentNullException(nameof(socketHandle));

            int result = LibcNativeMethods.EpollControl(SafeHandle, EpollOperation.EPOLL_CTL_ADD, socketHandle, ref eventSettings);

            if (result == -1)
                throw new SocketException();
        }

        /// <summary>
        /// Modifies the existing settings associated with a socket handle in the interest list with new settings.
        /// </summary>
        /// <param name="socketHandle">Socket handle currently in the interest list to update.</param>
        /// <param name="eventSettings">New seetings to associated eweith the socket handle.</param>
        /// <exception cref="ObjectDisposedException">Epoll file descriptor is closed.</exception>
        /// <exception cref="ArgumentNullException">Socket handle is null.</exception>
        /// <exception cref="SocketException">The epoll_ctl call failed.</exception>
        public void Modify(SafeFileDescriptorHandle socketHandle, EpollEvent eventSettings)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (socketHandle == null)
                throw new ArgumentNullException(nameof(socketHandle));

            int result = LibcNativeMethods.EpollControl(SafeHandle, EpollOperation.EPOLL_CTL_MOD, socketHandle, ref eventSettings);

            if (result == -1)
                throw new SocketException();
        }

        /// <summary>
        /// Removes the socket handle from the interest list.
        /// </summary>
        /// <param name="socketHandle">Socket handle currently in the interest list to remove.</param>
        /// <exception cref="ObjectDisposedException">Epoll file descriptor is closed.</exception>
        /// <exception cref="ArgumentNullException">Socket handle is null.</exception>
        /// <exception cref="SocketException">The epoll_ctl call failed.</exception>
        public void Remove(SafeFileDescriptorHandle socketHandle)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (socketHandle == null)
                throw new ArgumentNullException(nameof(socketHandle));

            var dummySettings = new EpollEvent();
            int result = LibcNativeMethods.EpollControl(SafeHandle, EpollOperation.EPOLL_CTL_DEL, socketHandle, ref dummySettings);

            if (result == -1)
                throw new SocketException();
        }
        
        /// <summary>
        /// Waits for an I/O event on the underlying epoll instance.
        /// </summary>
        /// <param name="maxEvents">The maximum number of events to wait for.</param>
        /// <param name="timeout">The maximum number of milliseconds that the call will block.</param>
        /// <returns>Array of event information for the file descriptors in the interest list that have some events available and are thus in the ready list.</returns>
        /// <exception cref="ObjectDisposedException">The epoll file descriptor is closed.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Maximum number of events must be greater than 0.</exception>
        /// <exception cref="SocketException">The epoll_wait call failed.</exception>
        public EpollEvent[] Wait(int maxEvents, int timeout)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (maxEvents <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxEvents), "Maximum event count cannot be less than or equal to zero.");

            var eventsArray = new EpollEvent[maxEvents];
            int numEvents = LibcNativeMethods.EpollWait(SafeHandle, eventsArray, maxEvents, timeout);

            if (numEvents == -1)
                throw new SocketException();

            return eventsArray.Take(numEvents).ToArray();
        }

        /// <summary>
        /// Closes the epoll instance and releases all of the associated resources.
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        /// <summary>
        /// Releases all of the resources associated with the current instance of the Epoll class.
        /// </summary>
        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the current instance of the Epoll class, and optionally disposes of the managed resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to releases only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                SafeHandle.Dispose();
            }

            _disposed = true;
        }
    }
}