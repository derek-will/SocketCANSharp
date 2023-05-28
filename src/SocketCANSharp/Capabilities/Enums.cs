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

namespace SocketCANSharp.Capabilities
{
    /// <summary>
    /// Capability sets associated with a thread.
    /// </summary>
    public enum CapabilitySet
    {
        /// <summary>
        /// Capabilities a thread may exercise.
        /// </summary>
        Effective,
        /// <summary>
        /// Capabilities a thread may pass through an execve call.
        /// </summary>
        Inheritable,
        /// <summary>
        /// Capabilities a thread can make effective or inheritable. 
        /// </summary>
        Permitted,
    }

    /// <summary>
    /// Linux capability version. 
    /// </summary>
    public enum LinuxCapabilityVersion : uint
    {
        /// <summary>
        /// Unsupported version used for probing the current preferred capability version.
        /// </summary>
        UNSUPPORTED_LINUX_CAPABILITY_VERSION    = 0x00000000,
        /// <summary>
        /// Kernels prior to 2.6.25 prefer 32-bit capabilities via version 1 of the Capabilities API.
        /// </summary>
        LINUX_CAPABILITY_VERSION_1              = 0x19980330,
        /// <summary>
        /// Kernel 2.6.25 added 64-bit capability sets via version 2 of the Capabilities API.
        /// </summary>
        LINUX_CAPABILITY_VERSION_2              = 0x20071026,
        /// <summary>
        /// Kernels starting with 2.6.26 fix an API glitch that was present in version 2 via version 3 of the Capabilities API.
        /// </summary>
        LINUX_CAPABILITY_VERSION_3              = 0x20080522,
    }

    /// <summary>
    /// Capabilities are distinct privileges traditionally associated with a superuser (root).
    /// </summary>
    public enum Capability
    {
        /// <summary>
        /// Make changes to file UIDs and GIDs (chown).
        /// </summary>
        CAP_CHOWN               = 0,
        /// <summary>
        /// Bypass discretionary access control (read, write, execution) permission checks.
        /// </summary>
        CAP_DAC_OVERRIDE        = 1,
        /// <summary>
        /// Bypass discretionary access control for file read, directory read and execution permission checks.
        /// </summary>
        CAP_DAC_READ_SEARCH     = 2,
        /// <summary>
        /// Bypass permission checks on operations which require the file system UID of the process to match the UID of the file not including those covered by CAP_DAC_OVERRIDE and CAP_DAC_READ_SEARCH.
        /// </summary>
        CAP_FOWNER              = 3,
        /// <summary>
        /// Bypass restrictions that the effective ID shall match the file owner ID when setting the permission bits. 
        /// </summary>
        CAP_FSETID              = 4,
        /// <summary>
        /// Bypass restrictions for sending signals to a process.
        /// </summary>
        CAP_KILL                = 5,
        /// <summary>
        /// Bypass restrictions on GID setting and allow for GID forging. 
        /// </summary>
        CAP_SETGID              = 6,
        /// <summary>
        /// Bypass restrictions on UID setting and allow for UID forging. 
        /// </summary>
        CAP_SETUID              = 7,
        /// <summary>
        /// Bypass restrictions on capability modifications.
        /// </summary>
        CAP_SETPCAP             = 8,
        /// <summary>
        /// Enables the setting of the FS_APPEND_FL and FS_IMMUTABLE_FL file attributes.
        /// </summary>
        CAP_LINUX_IMMUTABLE     = 9,
        /// <summary>
        /// Allows binding to privileged ports. 
        /// </summary>
        CAP_NET_BIND_SERVICE    = 10,
        /// <summary>
        /// Enables broadcasting and listening to multicasts.
        /// </summary>
        CAP_NET_BROADCAST       = 11,
        /// <summary>
        /// Enables performing various network administrative tasks.
        /// </summary>
        CAP_NET_ADMIN           = 12,
        /// <summary>
        /// Enables use of RAW and PACKET sockets and binding to any address for transparent proxying. 
        /// </summary>
        CAP_NET_RAW             = 13,
        /// <summary>
        /// Allows for memory locking.
        /// </summary>
        CAP_IPC_LOCK            = 14,
        /// <summary>
        /// Bypass restrictions regarding IPC ownership.
        /// </summary>
        CAP_IPC_OWNER           = 15,
        /// <summary>
        /// Enables loading and unloading of kernel modules.
        /// </summary>
        CAP_SYS_MODULE          = 16,
        /// <summary>
        /// Enables various I/O operations.
        /// </summary>
        CAP_SYS_RAWIO           = 17,
        /// <summary>
        /// Enables use of chroot.
        /// </summary>
        CAP_SYS_CHROOT          = 18,
        /// <summary>
        /// Enables tracing of any process.
        /// </summary>
        CAP_SYS_PTRACE          = 19,
        /// <summary>
        /// Enables turning on or off process accounting.
        /// </summary>
        CAP_SYS_PACCT           = 20,
        /// <summary>
        /// Enables a range of system administrative operations.
        /// </summary>
        CAP_SYS_ADMIN           = 21,
        /// <summary>
        /// Enables use of reboot and the loading of a new kernel for execution following reboot. 
        /// </summary>
        CAP_SYS_BOOT            = 22,
        /// <summary>
        /// Enables raising the priority of any process, manipulation of the scheduling algorithms of any process, setting CPU affinity of any process, etc.
        /// </summary>
        CAP_SYS_NICE            = 23,
        /// <summary>
        /// Enables overrides of various system resources (file systems, disks, console allocation, keymaps, etc.).
        /// </summary>
        CAP_SYS_RESOURCE        = 24,
        /// <summary>
        /// Enables the setting of the system clock and real-time clock. 
        /// </summary>
        CAP_SYS_TIME            = 25,
        /// <summary>
        /// Eables vhangup and privileged configuration operations on virtual terminals.
        /// </summary>
        CAP_SYS_TTY_CONFIG      = 26,
        /// <summary>
        /// Enables creating special files via mknod.
        /// </summary>
        CAP_MKNOD               = 27,
        /// <summary>
        /// Enables creating leases on any file.
        /// </summary>
        CAP_LEASE               = 28,
        /// <summary>
        /// Enables writing to the kernel audit log.
        /// </summary>
        CAP_AUDIT_WRITE         = 29,
        /// <summary>
        /// Enables the configuration of the audit log.
        /// </summary>
        CAP_AUDIT_CONTROL       = 30,
        /// <summary>
        /// Enables the setting of capabilities on files.
        /// </summary>
        CAP_SETFCAP             = 31,
        /// <summary>
        /// Enables the Override of Mandatory Access Control (MAC).
        /// </summary>
        CAP_MAC_OVERRIDE        = 32,
        /// <summary>
        /// Enables MAC configuration and state changes.
        /// </summary>
        CAP_MAC_ADMIN           = 33,
        /// <summary>
        /// Enables configuration of the kernel syslog.
        /// </summary>
        CAP_SYSLOG              = 34,
        /// <summary>
        /// Enables triggering something which will wake up the system.
        /// </summary>
        CAP_WAKE_ALARM          = 35,
        /// <summary>
        /// Enables blocking of system suspension.
        /// </summary>
        CAP_BLOCK_SUSPEND       = 36,
        /// <summary>
        /// Enables reading of the audit log.
        /// </summary>
        CAP_AUDIT_READ          = 37,
        /// <summary>
        /// Enables performance monitoring operations.
        /// </summary>
        CAP_PERFMON             = 38,
        /// <summary>
        /// Enable privileged Berkeley Packet Filter operations.
        /// </summary>
        CAP_BPF                 = 39,
        /// <summary>
        /// Enables checkpoint/restore functionality.
        /// </summary>
        CAP_CHECKPOINT_RESTORE  = 40,
    }
}