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
using System.Runtime.InteropServices;

namespace SocketCANSharp
{
    /// <summary>
    /// Represents a time interval structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class Timeval
    {
        private IntPtr _tv_sec;
        private IntPtr _tv_usec;

        /// <summary>
        /// Number of seconds.
        /// </summary>
        public long Seconds 
        { 
            get
            {
                return _tv_sec.ToInt64();
            } 
            set
            {
                _tv_sec = new IntPtr(value);
            }
        }
        
        /// <summary>
        /// Number of microseconds. This value is used in combination with the Seconds property to represent the full interval of time.
        /// </summary>
        public long Microseconds
        { 
            get
            {
                return _tv_usec.ToInt64();
            } 
            set
            {
                _tv_usec = new IntPtr(value);
            }
        }

        /// <summary>
        /// Initializes a time interval object with the specified seconds and microseconds.
        /// </summary>
        /// <param name="seconds">Number of seconds</param>
        /// <param name="microseconds">Number of microseconds</param>
        public Timeval(long seconds, long microseconds)
        {
            Seconds = seconds;
            Microseconds = microseconds;
        }

        /// <summary>
        /// Returns a string that represents the current Timeval object.
        /// </summary>
        /// <returns>A string that represents the current Timeval object.</returns>
        public override string ToString()
        {
            return $"Seconds: {Seconds}; Microseconds: {Microseconds}";
        }
    }
}