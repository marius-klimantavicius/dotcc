#nullable enable
using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    /// <summary>Linux x64 POSIX record locks on the actual file backing a dotcc
    /// descriptor. The kernel owns ranges, conflicts, process IDs, deadlock
    /// detection and blocking/EINTR behavior. Threads and RuntimeContexts in one
    /// process share POSIX locks; closing any handle for the inode releases them.</summary>
    public static int fcntl(int fd, int cmd, void* arg)
    {
        if (cmd is not (5 or 6 or 7))
            return cmd is 1 or 3 ? fcntl(fd, cmd) : fcntl(fd, cmd, unchecked((int)(nuint)arg));
        return ApplyFileLock(fd, cmd, arg, record: true);
    }

    /// <summary>Linux x64 flock(2), with open-file-description ownership and
    /// real shared/exclusive/nonblocking/unlock semantics. Separate from POSIX
    /// record locks; no descriptor duplication or success fallback.</summary>
    public static int flock(int fd, int operation) => ApplyFileLock(fd, operation, null, record: false);

    private static int ApplyFileLock(int fd, int cmd, void* arg, bool record)
    {
        var slot = SlotByFd(fd);
        if (slot is null) return FcntlError(EBADF);
        if (!OperatingSystem.IsLinux() || RuntimeInformation.ProcessArchitecture != Architecture.X64)
            return FcntlError(ENOTSUP);
        // Console, in-memory pipes and emulated devices have no equivalent
        // host descriptor. Never send the dotcc table index to the OS.
        if (slot.Stream is not FileStream file) return FcntlError(ENOTSUP);
        SafeFileHandle? handle = null;
        bool retained = false;
        try
        {
            // Getting SafeFileHandle drains FileStream's private byte buffer.
            // Borrow the original handle: dup/close would release this process's
            // existing record locks. Retention delays a racing close until this
            // call returns, including an actual blocking F_SETLKW. No descriptor
            // table lock is held while waiting on another process.
            handle = file.SafeFileHandle;
            handle.DangerousAddRef(ref retained);
            int nativeFd = checked((int)handle.DangerousGetHandle());
            // FileStream may use positioned I/O, leaving the kernel offset
            // behind its logical Position. Synchronize that same position for
            // SEEK_CUR; fcntl itself never advances it. SEEK_END uses the inode.
            if (record && RecordLockSeek(nativeFd, file.Position, 0) < 0)
                return FcntlError(Marshal.GetLastPInvokeError());
            // Pass the original C pointer through to the kernel, which checks
            // its accessibility and returns EFAULT for invalid buffers. There
            // is no managed mirror/dereference that can crash on integer args.
            int result = record ? PosixRecordLock(nativeFd, cmd, arg) : PosixFlock(nativeFd, cmd);
            return result < 0 ? FcntlError(Marshal.GetLastPInvokeError()) : result;
        }
        catch (ObjectDisposedException) { return FcntlError(EBADF); }
        catch (NotSupportedException) { return FcntlError(ENOTSUP); }
        catch (IOException) { return FcntlError(EIO); }
        finally { if (retained) handle!.DangerousRelease(); }
    }

    [DllImport("libc", EntryPoint = "fcntl", SetLastError = true)]
    private static extern int PosixRecordLock(int fd, int cmd, void* arg);

    [DllImport("libc", EntryPoint = "flock", SetLastError = true)]
    private static extern int PosixFlock(int fd, int operation);

    [DllImport("libc", EntryPoint = "lseek", SetLastError = true)]
    private static extern long RecordLockSeek(int fd, long offset, int whence);
}
