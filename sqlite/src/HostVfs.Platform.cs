#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace DotCC.Sqlite;

// OS services only: SQLite itself and all VFS callbacks remain managed code.
// All handles for this VFS must be opened/closed here. In particular, do not
// dispose FileHandle.Handle: Darwin POSIX locks require deferred descriptor close.
internal static unsafe class HostPlatform
{
    private const long PendingByte = 0x40000000;
    private const long ReservedByte = PendingByte + 1;
    private const long SharedFirst = PendingByte + 2;
    private const long SharedSize = 510;
    private const int Busy = 5, IoLock = 10 | (15 << 8), IoUnlock = 10 | (8 << 8);
    private const int IoCheck = 10 | (14 << 8), IoReadLock = 10 | (9 << 8);
    private static readonly object Gate = new();
#if HOST_VFS_PLATFORM_TESTS
    // Compiled only by the isolated descriptor-lifetime regression fixture.
    internal static Func<SafeFileHandle, long, long, int, int>? RangeOperationForTests;
#endif
    private static readonly Dictionary<(uint Device, ulong Inode), InodeState> Inodes = new();

    internal sealed class InodeState
    {
        internal readonly List<FileHandle> Files = new();
        internal readonly List<SafeFileHandle> Deferred = new();
        internal bool HasOrphanLocks;
        internal bool HasLocks => Files.Exists(f => f.HasLocks);
    }

    internal sealed class FileHandle : IDisposable
    {
        public SafeFileHandle Handle { get; }
        public int LockLevel => exclusive ? 4 : pending ? 3 : reserved ? 2 : shared ? 1 : 0;
        private readonly bool writable;
        private readonly InodeState? inode;
        private readonly (uint Device, ulong Inode) identity;
        private bool shared, reserved, pending, exclusive, disposed;
        private bool gateRead, reservedProbe, sharedOverlay, ioFailed;
        internal bool HasLocks => shared || reserved || pending || exclusive || gateRead || reservedProbe || sharedOverlay;
        private int FailUnlock() { ioFailed = true; return IoUnlock; }
        private int FailReadLock() { ioFailed = true; return IoReadLock; }

        internal FileHandle(SafeFileHandle handle, bool writable, InodeState? inode,
            (uint Device, ulong Inode) identity)
        {
            Handle = handle;
            this.writable = writable;
            this.inode = inode;
            this.identity = identity;
        }

        public int Lock(int level)
        {
            lock (Gate)
            {
                ObjectDisposedException.ThrowIf(disposed, this);
                if (ioFailed || (inode is not null && (inode.HasOrphanLocks || inode.Files.Exists(f => f.ioFailed))) || level is not (1 or 2 or 4)) return IoLock;
                if (LockLevel >= level) return 0;
                if (level > 1 && !writable) return IoLock;
                if (level > 1 && !shared) return IoLock;
                if (level == 1)
                {
                    // POSIX process locks cannot distinguish our own descriptors.
                    // Reuse the inode's read lock, but never admit readers past PENDING.
                    if (inode is not null && inode.Files.Exists(f => f != this && f.pending)) return Busy;
                    if (inode is not null && inode.Files.Exists(f => f != this && f.shared))
                    {
                        shared = true;
                        return 0;
                    }
                    int rc = SetRange(Handle, PendingByte, 1, 0);
                    if (rc != 0) return rc;
                    gateRead = true;
                    rc = SetRange(Handle, SharedFirst, SharedSize, 0);
                    if (rc == 0) shared = true;
                    int release = SetRange(Handle, PendingByte, 1, 2);
                    // A failed temporary read-gate release is not a WRITE PENDING lock.
                    if (release != 0) return FailUnlock();
                    gateRead = false;
                    return rc;
                }
                if (inode is not null && inode.Files.Exists(f => f != this && f.LockLevel > 1)) return Busy;
                if (level == 2)
                {
                    int rc = SetRange(Handle, ReservedByte, 1, 1);
                    if (rc == 0) reserved = true;
                    return rc;
                }
                if (!pending)
                {
                    int rc = SetRange(Handle, PendingByte, 1, 1);
                    if (rc != 0) return rc;
                    pending = true;
                }
                // Retain PENDING on BUSY. This is part of SQLite's lock protocol.
                if (inode is not null && inode.Files.Exists(f => f != this && f.shared)) return Busy;
                if (OperatingSystem.IsWindows())
                {
                    if (SetRange(Handle, SharedFirst, SharedSize, 2) != 0) return FailUnlock();
                    shared = false;
                }
                int result = SetRange(Handle, SharedFirst, SharedSize, 1);
                if (result == 0) { shared = true; exclusive = true; }
                else if (OperatingSystem.IsWindows())
                {
                    // PENDING still excludes newcomers while restoring our reader lock.
                    if (SetRange(Handle, SharedFirst, SharedSize, 0) != 0) return FailReadLock();
                    shared = true;
                }
                return result;
            }
        }

        public int Unlock(int level)
        {
            lock (Gate)
            {
                ObjectDisposedException.ThrowIf(disposed, this);
                if (level is not (0 or 1)) return IoUnlock;
                if (ioFailed && level == 1) return IoUnlock;
                if (LockLevel <= level && !ioFailed) return 0;
                if (exclusive && level == 1)
                {
                    // Windows permits a shared lock overlapping this handle's
                    // exclusive lock; the next unlock removes the exclusive first.
                    // POSIX converts the lock atomically.
                    if (SetRange(Handle, SharedFirst, SharedSize, 0) != 0) return FailReadLock();
                    if (OperatingSystem.IsWindows())
                    {
                        sharedOverlay = true;
                        if (SetRange(Handle, SharedFirst, SharedSize, 2) != 0) return FailUnlock();
                        sharedOverlay = false;
                    }
                    exclusive = false;
                }
                if (level == 0 && shared)
                {
                    if (sharedOverlay)
                    {
                        if (SetRange(Handle, SharedFirst, SharedSize, 2) != 0) return FailUnlock();
                        sharedOverlay = exclusive = false;
                    }
                    bool lastReader = inode is null || !inode.Files.Exists(f => f != this && f.shared);
                    if (lastReader && SetRange(Handle, SharedFirst, SharedSize, 2) != 0) return FailUnlock();
                    shared = exclusive = false;
                }
                if (reserved)
                {
                    if (SetRange(Handle, ReservedByte, 1, 2) != 0) return FailUnlock();
                    reserved = false;
                }
                if (pending)
                {
                    if (SetRange(Handle, PendingByte, 1, 2) != 0) return FailUnlock();
                    pending = false;
                }
                if (gateRead)
                {
                    if (SetRange(Handle, PendingByte, 1, 2) != 0) return FailUnlock();
                    gateRead = false;
                }
                if (reservedProbe)
                {
                    if (SetRange(Handle, ReservedByte, 1, 2) != 0) return FailUnlock();
                    reservedProbe = false;
                }
                ioFailed = false;
                DrainDeferred(inode);
                return 0;
            }
        }

        public int CheckReserved(out bool held)
        {
            lock (Gate)
            {
                ObjectDisposedException.ThrowIf(disposed, this);
                if (ioFailed || inode?.HasOrphanLocks == true) { held = false; return IoCheck; }
                held = LockLevel >= 2 || (inode is not null && inode.Files.Exists(f => f.LockLevel >= 2));
                if (held) return 0;
                if (OperatingSystem.IsWindows())
                {
                    // Shared probe works with a read-only HANDLE and conflicts with
                    // the writer's exclusive RESERVED byte.
                    int rc = SetRange(Handle, ReservedByte, 1, 0);
                    if (rc == Busy) { held = true; return 0; }
                    if (rc != 0) return IoCheck;
                    reservedProbe = true;
                    if (SetRange(Handle, ReservedByte, 1, 2) != 0) { ioFailed = true; return IoCheck; }
                    reservedProbe = false;
                    return 0;
                }
                return QueryReserved(Handle, out held);
            }
        }

        public void Dispose()
        {
            lock (Gate)
            {
                if (disposed) return;
                int rc = Unlock(0);
                if (inode is not null)
                {
                    // xClose releases its managed context even on IOERR_CLOSE, so
                    // this lease can never be retried. Transfer ownership of the
                    // descriptor to the inode until peers no longer need its locks.
                    inode.Files.Remove(this);
                    if (inode.HasLocks)
                    {
                        inode.Deferred.Add(Handle);
                        // Failed unlocks may leave process-owned bytes behind. Do
                        // not admit new locks until closing the deferred descriptor
                        // can safely release those bytes along with the last reader.
                        inode.HasOrphanLocks |= rc != 0;
                    }
                    else
                    {
                        Handle.Dispose();
                        DrainDeferred(inode);
                    }
                    if (inode.Files.Count == 0 && inode.Deferred.Count == 0) Inodes.Remove(identity);
                }
                else Handle.Dispose(); // OFD/Windows close releases this handle's locks only.
                disposed = true;
                if (rc != 0) throw new IOException("Unable to release SQLite file locks before closing.");
            }
        }
    }

    public static FileHandle Open(string path, FileMode mode, FileAccess access, FileOptions options, bool noFollow = false)
    {
        EnsureSupported();
        if (access == FileAccess.Write) throw new ArgumentException("SQLite requires readable file handles.", nameof(access));
        if ((options & (FileOptions.Asynchronous | FileOptions.Encrypted)) != 0)
            throw new ArgumentException("VFS requires synchronous, unencrypted handles.", nameof(options));
        lock (Gate)
        {
            SafeFileHandle handle = OperatingSystem.IsWindows()
                ? OpenWindows(path, mode, access, options, noFollow)
                : OpenUnix(path, mode, access, options, noFollow);
            FileHandle? result = null;
            try
            {
                InodeState? inode = null;
                (uint Device, ulong Inode) identity = default;
                if (OperatingSystem.IsMacOS())
                {
                    identity = DarwinIdentity(handle);
                    if (!Inodes.TryGetValue(identity, out inode)) Inodes.Add(identity, inode = new InodeState());
                }
                result = new FileHandle(handle, access == FileAccess.ReadWrite, inode, identity);
                inode?.Files.Add(result);
                if (!OperatingSystem.IsWindows() && (options & FileOptions.DeleteOnClose) != 0 && unlink(path) != 0)
                    throw NativeError("unlink temporary file", path);
                return result;
            }
            catch
            {
                // Identity failure is exceptional; do not close a Darwin descriptor
                // while it might alias a locked inode whose identity we cannot read.
                if (result is not null) result.Dispose();
                else if (OperatingSystem.IsMacOS() && Inodes.Count != 0)
                {
                    UnknownDarwinHandles.Add(handle);
                    DrainDeferred(null);
                }
                else handle.Dispose();
                throw;
            }
        }
    }

    // Only used if fstat fails after a successful Darwin open. Keep the safe handle
    // rooted until no registered inode is locked; then closing is safe again.
    private static readonly List<SafeFileHandle> UnknownDarwinHandles = new();
    private static void DrainDeferred(InodeState? inode)
    {
        if (inode is not null && !inode.HasLocks)
        {
            foreach (SafeFileHandle handle in inode.Deferred) handle.Dispose();
            inode.Deferred.Clear();
            inode.HasOrphanLocks = false;
        }
        if (OperatingSystem.IsMacOS() && !System.Linq.Enumerable.Any(Inodes.Values, i => i.HasLocks))
        {
            foreach (SafeFileHandle handle in UnknownDarwinHandles) handle.Dispose();
            UnknownDarwinHandles.Clear();
        }
    }

    public static void Sync(SafeFileHandle handle, bool full)
    {
        EnsureSupported();
        if (full && OperatingSystem.IsMacOS())
        {
            // F_FULLFSYNC asks the drive to flush its volatile write cache.
            int rc;
            do { rc = DarwinFcntl(handle, 51, null); } while (rc < 0 && Marshal.GetLastPInvokeError() == 4);
            if (rc == 0) return;
            // Match SQLite full_fsync: unsupported full-cache flush falls back
            // to the filesystem's normal fsync contract below.
        }
        RandomAccess.FlushToDisk(handle);
    }

    public static void SyncDirectory(string directory)
    {
        EnsureSupported();
        // SQLite's native Windows VFS has no directory-fsync primitive either.
        // File contents are flushed with FlushFileBuffers via RandomAccess above.
        if (OperatingSystem.IsWindows()) return;
        int flags = OperatingSystem.IsMacOS() ? 0x1000000 | 0x100000 : 0x80000 | 0x10000;
        int fd = UnixOpen(directory, flags, 0);
        if (fd < 0) throw NativeError("open directory");
        using var handle = new SafeFileHandle((nint)fd, true);
        int rc;
        do { rc = fsync(handle); } while (rc < 0 && Marshal.GetLastPInvokeError() == 4);
        if (rc < 0) throw NativeError("fsync directory");
    }

    public static bool Access(string path, int sqliteAccessMode)
    {
        EnsureSupported();
        if (sqliteAccessMode is < 0 or > 2) throw new ArgumentOutOfRangeException(nameof(sqliteAccessMode));
        if (!OperatingSystem.IsWindows())
            return access(path, sqliteAccessMode == 0 ? 0 : sqliteAccessMode == 1 ? 6 : 4) == 0;
        uint attributes = GetFileAttributesW(path);
        // Like SQLite's winAccess: attributes are a preflight, CreateFile remains
        // authoritative for ACLs. No incidental open/close is needed here.
        if (attributes == uint.MaxValue)
        {
            int error = Marshal.GetLastPInvokeError();
            if (error is 2 or 3) return false;
            throw NativeError("GetFileAttributes", path);
        }
        return sqliteAccessMode != 1 || (attributes & 1) == 0;
    }

    private static void EnsureSupported()
    {
        if ((!OperatingSystem.IsLinux() && !OperatingSystem.IsWindows() && !OperatingSystem.IsMacOS()) ||
            RuntimeInformation.ProcessArchitecture is not (Architecture.X64 or Architecture.Arm64))
            throw new PlatformNotSupportedException("Host VFS supports Linux, Windows and macOS on x64/arm64.");
    }

    private static SafeFileHandle OpenUnix(string path, FileMode mode, FileAccess accessMode, FileOptions options, bool noFollow)
    {
        bool mac = OperatingSystem.IsMacOS();
        int flags = (accessMode == FileAccess.Read ? 0 : 2) | (mac ? 0x1000000 : 0x80000);
        int create = mac ? 0x200 : 0x40, exclusive = mac ? 0x800 : 0x80, truncate = mac ? 0x400 : 0x200;
        flags |= mode switch
        {
            FileMode.Open => 0, FileMode.OpenOrCreate => create,
            FileMode.CreateNew => create | exclusive, FileMode.Create => create | truncate,
            FileMode.Truncate => truncate, _ => throw new ArgumentOutOfRangeException(nameof(mode))
        };
        if (noFollow) flags |= mac ? 0x100 : 0x20000;
        if ((options & FileOptions.WriteThrough) != 0) flags |= mac ? 0x80 : 0x101000;
        int fd = UnixOpen(path, flags, 0x1b6); // 0666, filtered by process umask.
        if (fd < 0) throw NativeError("open", path);
        return new SafeFileHandle((nint)fd, true);
    }

    private static SafeFileHandle OpenWindows(string path, FileMode mode, FileAccess accessMode, FileOptions options, bool noFollow)
    {
        uint disposition = mode switch
        {
            FileMode.CreateNew => 1, FileMode.Create => 2, FileMode.Open => 3,
            FileMode.OpenOrCreate => 4, FileMode.Truncate => 5,
            _ => throw new ArgumentOutOfRangeException(nameof(mode))
        };
        uint flags = 0x80 | (noFollow ? 0x200000u : 0);
        if ((options & FileOptions.DeleteOnClose) != 0) flags |= 0x04000000;
        if ((options & FileOptions.WriteThrough) != 0) flags |= 0x80000000;
        if ((options & FileOptions.RandomAccess) != 0) flags |= 0x10000000;
        uint desiredAccess = accessMode == FileAccess.Read ? 0x80000000u : 0xc0000000u;
        if ((options & FileOptions.DeleteOnClose) != 0) desiredAccess |= 0x00010000; // DELETE
        SafeFileHandle handle = CreateFileW(path, desiredAccess,
            1 | 2 | 4, 0, disposition, flags, 0);
        if (handle.IsInvalid) { var error = NativeError("CreateFile", path); handle.Dispose(); throw error; }
        if (noFollow)
        {
            if (!GetFileInformationByHandle(handle, out WindowsFileInformation info))
            { var error = NativeError("GetFileInformationByHandle", path); handle.Dispose(); throw error; }
            if ((info.Attributes & 0x400) != 0) { handle.Dispose(); throw new IOException("SQLITE_OPEN_NOFOLLOW rejects reparse points."); }
        }
        return handle;
    }

    // kind: 0 read/shared, 1 write/exclusive, 2 unlock. Calls never block.
    private static int SetRange(SafeFileHandle handle, long start, long length, int kind)
    {
#if HOST_VFS_PLATFORM_TESTS
        if (RangeOperationForTests is not null) return RangeOperationForTests(handle, start, length, kind);
#endif
        if (OperatingSystem.IsWindows())
        {
            var overlap = new Overlapped { Offset = (uint)start, OffsetHigh = (uint)(start >> 32) };
            bool ok = kind == 2 ? UnlockFileEx(handle, 0, (uint)length, 0, ref overlap)
                : LockFileEx(handle, (uint)(1 | (kind == 1 ? 2 : 0)), 0, (uint)length, 0, ref overlap);
            if (ok) return 0;
            int error = Marshal.GetLastPInvokeError();
            return kind != 2 && error is 32 or 33 ? Busy : kind == 2 ? IoUnlock : IoLock;
        }
        int rc;
        do
        {
            if (OperatingSystem.IsMacOS())
            {
                var range = new DarwinFlock { Start = start, Length = length, Type = (short)(kind == 0 ? 1 : kind == 1 ? 3 : 2) };
                rc = DarwinFcntl(handle, 8, &range); // F_SETLK
            }
            else
            {
                var range = new LinuxFlock { Start = start, Length = length, Type = (short)kind };
                rc = fcntl(handle, 37, &range); // F_OFD_SETLK, l_pid must be zero.
            }
        } while (rc < 0 && Marshal.GetLastPInvokeError() == 4);
        if (rc == 0) return 0;
        int errno = Marshal.GetLastPInvokeError();
        return kind != 2 && errno is 11 or 13 or 35 ? Busy : kind == 2 ? IoUnlock : IoLock;
    }

    private static int QueryReserved(SafeFileHandle handle, out bool held)
    {
        held = false;
        int rc;
        if (OperatingSystem.IsMacOS())
        {
            var range = new DarwinFlock { Start = ReservedByte, Length = 1, Type = 3 };
            do { rc = DarwinFcntl(handle, 7, &range); } while (rc < 0 && Marshal.GetLastPInvokeError() == 4);
            if (rc == 0) held = range.Type != 2;
        }
        else
        {
            var range = new LinuxFlock { Start = ReservedByte, Length = 1, Type = 1 };
            do { rc = fcntl(handle, 36, &range); } while (rc < 0 && Marshal.GetLastPInvokeError() == 4);
            if (rc == 0) held = range.Type != 2;
        }
        return rc == 0 ? 0 : IoCheck;
    }

    private static (uint Device, ulong Inode) DarwinIdentity(SafeFileHandle handle)
    {
        DarwinStat stat;
        int rc = RuntimeInformation.ProcessArchitecture == Architecture.Arm64
            ? fstatDarwinArm64(handle, &stat) : fstatDarwin(handle, &stat);
        if (rc != 0) throw NativeError("fstat");
        return (stat.Device, stat.Inode);
    }

    private static IOException NativeError(string operation, string? path = null)
    {
        int error = Marshal.GetLastPInvokeError();
        return new IOException($"{operation}{(path is null ? "" : " '" + path + "'")}: {new Win32Exception(error).Message} (OS error {error}).", error);
    }

    // Darwin arm64 variadic arguments are stack-passed. Six register placeholders
    // put open's mode / fcntl's third argument at the first stack argument slot.
    private static int UnixOpen(string path, int flags, uint mode)
    {
        int fd;
        do
        {
            fd = OperatingSystem.IsMacOS() && RuntimeInformation.ProcessArchitecture == Architecture.Arm64
                ? openDarwinArm64(path, flags, 0, 0, 0, 0, 0, 0, mode) : open(path, flags, mode);
        } while (fd < 0 && Marshal.GetLastPInvokeError() == 4);
        return fd;
    }

    private static int DarwinFcntl(SafeFileHandle handle, int command, void* argument) =>
        RuntimeInformation.ProcessArchitecture == Architecture.Arm64
            ? fcntlDarwinArm64(handle, command, 0, 0, 0, 0, 0, 0, argument) : fcntl(handle, command, argument);

    [StructLayout(LayoutKind.Sequential)]
    private struct LinuxFlock { internal short Type, Whence; internal long Start, Length; internal int Pid; }
    [StructLayout(LayoutKind.Sequential)]
    private struct DarwinFlock { internal long Start, Length; internal int Pid; internal short Type, Whence; }
    [StructLayout(LayoutKind.Explicit, Size = 144)]
    private struct DarwinStat { [FieldOffset(0)] internal uint Device; [FieldOffset(8)] internal ulong Inode; }
    [StructLayout(LayoutKind.Sequential)]
    private struct Overlapped { internal nuint Internal, InternalHigh; internal uint Offset, OffsetHigh; internal nint Event; }
    [StructLayout(LayoutKind.Sequential)]
    private struct WindowsFileInformation
    {
        internal uint Attributes, CreationLow, CreationHigh, AccessLow, AccessHigh, WriteLow, WriteHigh;
        internal uint VolumeSerial, SizeHigh, SizeLow, Links, IndexHigh, IndexLow;
    }

    [DllImport("libc", SetLastError = true)] private static extern int open([MarshalAs(UnmanagedType.LPUTF8Str)] string path, int flags, uint mode);
    [DllImport("libc", EntryPoint = "open", SetLastError = true)] private static extern int openDarwinArm64([MarshalAs(UnmanagedType.LPUTF8Str)] string path, int flags, nint p1, nint p2, nint p3, nint p4, nint p5, nint p6, uint mode);
    [DllImport("libc", SetLastError = true)] private static extern int fcntl(SafeFileHandle handle, int command, void* argument);
    [DllImport("libc", EntryPoint = "fcntl", SetLastError = true)] private static extern int fcntlDarwinArm64(SafeFileHandle handle, int command, nint p1, nint p2, nint p3, nint p4, nint p5, nint p6, void* argument);
    [DllImport("libc", SetLastError = true)] private static extern int fsync(SafeFileHandle handle);
    [DllImport("libc", SetLastError = true)] private static extern int unlink([MarshalAs(UnmanagedType.LPUTF8Str)] string path);
    [DllImport("libc", SetLastError = true)] private static extern int access([MarshalAs(UnmanagedType.LPUTF8Str)] string path, int mode);
    [DllImport("libc", EntryPoint = "fstat$INODE64", SetLastError = true)] private static extern int fstatDarwin(SafeFileHandle handle, DarwinStat* stat);
    [DllImport("libc", EntryPoint = "fstat", SetLastError = true)] private static extern int fstatDarwinArm64(SafeFileHandle handle, DarwinStat* stat);
    [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)] private static extern SafeFileHandle CreateFileW(string path, uint access, uint share, nint security, uint disposition, uint flags, nint template);
    [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)] private static extern uint GetFileAttributesW(string path);
    [DllImport("kernel32", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetFileInformationByHandle(SafeFileHandle handle, out WindowsFileInformation information);
    [DllImport("kernel32", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool LockFileEx(SafeFileHandle handle, uint flags, uint reserved, uint lengthLow, uint lengthHigh, ref Overlapped overlap);
    [DllImport("kernel32", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool UnlockFileEx(SafeFileHandle handle, uint reserved, uint lengthLow, uint lengthHigh, ref Overlapped overlap);
}
