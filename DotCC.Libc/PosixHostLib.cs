#nullable enable
using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    // These imports use the Linux x64 LP64 ABI declared by the guest headers.
    private static bool PosixHostAbi => OperatingSystem.IsLinux() &&
        RuntimeInformation.ProcessArchitecture == Architecture.X64 && IntPtr.Size == 8;

    private static bool SharedMemoryName(byte* name)
    {
        if (name == null) { errno = EFAULT; return false; }
        if (name[0] != (byte)'/' || name[1] == 0) { errno = EINVAL; return false; }
        int length = 1;
        for (; length <= 256 && name[length] != 0; length++)
            if (name[length] == (byte)'/') { errno = EINVAL; return false; }
        if (length > 256) { errno = ENAMETOOLONG; return false; }
        if (name[1] == (byte)'.' && (name[2] == 0 || (name[2] == (byte)'.' && name[3] == 0)))
        { errno = EINVAL; return false; }
        return true;
    }

    /// <summary>Real named POSIX shared memory. The OS descriptor transfers to
    /// an owning FileStream and then the current owner's descriptor table;
    /// callers receive only a guest descriptor, usable by the existing mmap.</summary>
    public static int shm_open(byte* name, int flags, uint mode)
    {
        if (!SharedMemoryName(name)) return -1;
        const int allowed = 2 | 0x40 | 0x80 | 0x200 | 0x80000;
        if ((flags & ~allowed) != 0) { errno = EINVAL; return -1; }
        if (!PosixHostAbi) { errno = ENOTSUP; return -1; }
        int descriptor = HostSharedMemoryOpen(name, flags, mode);
        if (descriptor < 0) { errno = Marshal.GetLastPInvokeError(); return -1; }
        var handle = new SafeFileHandle((IntPtr)descriptor, ownsHandle: true);
        FileStream? stream = null;
        try
        {
            stream = new FileStream(handle, (flags & 3) == 2 ? FileAccess.ReadWrite : FileAccess.Read, bufferSize: 1);
            int guest = RegisterFileSlot(stream);
            SlotByFd(guest)!.DescriptorFlags = 1; // shm_open always sets FD_CLOEXEC
            return guest;
        }
        catch (Exception error) when (error is IOException or ArgumentException or UnauthorizedAccessException or OutOfMemoryException)
        {
            if (stream != null) stream.Dispose(); else handle.Dispose();
            errno = error is OutOfMemoryException ? ENOMEM : error is UnauthorizedAccessException ? EACCES : EIO;
            return -1;
        }
    }

    /// <summary>Remove the shared name. Existing descriptors/mappings retain
    /// the unlinked object until their actual owning resources are released.</summary>
    public static int shm_unlink(byte* name)
    {
        if (!SharedMemoryName(name)) return -1;
        if (!PosixHostAbi) { errno = ENOTSUP; return -1; }
        int result = HostSharedMemoryUnlink(name);
        if (result < 0) errno = Marshal.GetLastPInvokeError();
        return result;
    }

    // void* deliberately accepts the struct emitted from the actual C header.
    // Linux libc reads/writes the 60-byte, 4-aligned guest termios unchanged.
    public static int tcgetattr(int descriptor, void* attributes) => TerminalAttributes(descriptor, 0, attributes, false);
    public static int tcsetattr(int descriptor, int action, void* attributes) => TerminalAttributes(descriptor, action, attributes, true);

    private static int TerminalAttributes(int descriptor, int action, void* attributes, bool setting)
    {
        var slot = SlotByFd(descriptor);
        if (slot == null) { errno = EBADF; return -1; }
        if (attributes == null) { errno = EFAULT; return -1; }
        if (!PosixHostAbi) { errno = ENOTSUP; return -1; }
        try
        {
            int result;
            if (slot.Kind == FileSlot.K.File && slot.Stream is FileStream file)
                result = setting ? HostSetTerminal(file.SafeFileHandle, action, attributes) : HostGetTerminal(file.SafeFileHandle, attributes);
            else
            {
                // fd 0/1/2 may have been freopen'd or replaced by an owned
                // pipe/file. Only genuine console kinds use OS std handles.
                int native = slot.Kind switch
                {
                    FileSlot.K.In when descriptor == 0 => 0,
                    FileSlot.K.Out when descriptor == 1 => 1,
                    FileSlot.K.Err when descriptor == 2 => 2,
                    _ => -1,
                };
                if (native < 0) { errno = ENOTTY; return -1; }
                result = setting ? HostSetStandardTerminal(native, action, attributes) : HostGetStandardTerminal(native, attributes);
            }
            if (result < 0) errno = Marshal.GetLastPInvokeError();
            return result;
        }
        catch (ObjectDisposedException) { errno = EBADF; return -1; }
    }

    [DllImport("libc", EntryPoint = "shm_open", SetLastError = true)]
    private static extern int HostSharedMemoryOpen(byte* name, int flags, uint mode);
    [DllImport("libc", EntryPoint = "shm_unlink", SetLastError = true)]
    private static extern int HostSharedMemoryUnlink(byte* name);
    [DllImport("libc", EntryPoint = "tcgetattr", SetLastError = true)]
    private static extern int HostGetTerminal(SafeFileHandle descriptor, void* attributes);
    [DllImport("libc", EntryPoint = "tcsetattr", SetLastError = true)]
    private static extern int HostSetTerminal(SafeFileHandle descriptor, int action, void* attributes);
    [DllImport("libc", EntryPoint = "tcgetattr", SetLastError = true)]
    private static extern int HostGetStandardTerminal(int descriptor, void* attributes);
    [DllImport("libc", EntryPoint = "tcsetattr", SetLastError = true)]
    private static extern int HostSetStandardTerminal(int descriptor, int action, void* attributes);
}
