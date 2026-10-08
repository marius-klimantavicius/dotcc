#nullable enable
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Edg.Runtime;

// The translated ABI is Linux-like LP64. Native structures never cross it.
internal static unsafe partial class PosixPlatform
{
    private sealed class PortableError(int number) : IOException("POSIX error " + number) { internal readonly int Number = number; }
    internal static Exception Failure(int error) => new PortableError(error);
    internal static int Error(Exception error)
    {
        if (error is PortableError portable) return portable.Number;
        if (error is Win32Exception native) return NativeErrorNumber(native.NativeErrorCode);
        return error switch
        {
            FileNotFoundException or DirectoryNotFoundException => 2,
            UnauthorizedAccessException => 13,
            ObjectDisposedException => 9,
            ArgumentException or OverflowException => 22,
            OutOfMemoryException => 12,
            PlatformNotSupportedException or NotSupportedException => 95,
            IOException io when (io.HResult & 0xffff) != 0 => NativeErrorNumber(io.HResult & 0xffff),
            _ => 5,
        };
    }
    internal static int NativeErrorNumber(int error)
    {
        if (OperatingSystem.IsWindows()) return error switch { 2 or 3 => 2, 5 => 13, 6 => 9, 8 or 14 => 12, 17 => 18, 19 => 30, 32 or 33 => 11, 80 or 183 => 17, 87 => 22, 112 => 28, 145 => 39, 206 => 36, 267 => 20, 1314 => 1, _ => 5 };
        if (OperatingSystem.IsMacOS()) return error switch { 11 => 35, 35 => 11, 45 or 102 => 95, 60 => 110, 62 => 40, 63 => 36, 66 => 39, 69 => 122, 70 => 116, 77 => 37, 78 => 38, 84 => 75, 92 => 84, _ => error };
        return error;
    }
    private static Exception NativeError() => new Win32Exception(Marshal.GetLastPInvokeError());
    private static void EnsureSupported()
    {
        if ((!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS() && !OperatingSystem.IsWindows()) || RuntimeInformation.ProcessArchitecture is not (Architecture.X64 or Architecture.Arm64))
            throw new PlatformNotSupportedException("POSIX platform supports Linux, macOS and Windows x64/arm64.");
    }
    internal static string CanonicalPath(string path)
    {
        EnsureSupported();
        if (!OperatingSystem.IsWindows())
        {
            // Path.GetFullPath is lexical: it would collapse link/.. before
            // following the symlink and would accept nonexistent components.
            nint resolved = RealpathUnix(path, 0);
            if (resolved == 0) throw NativeError();
            try { return Marshal.PtrToStringUTF8(resolved)!; }
            finally { FreeCanonicalPath(resolved); }
        }
        SafeFileHandle handle;
        fixed (char* name = path)
            handle = CreateFile(name, 0, 7, 0, 3, 0x02000000, 0); // OPEN_EXISTING, BACKUP_SEMANTICS (directories)
        using (handle)
        {
            if (handle.IsInvalid) throw NativeError();
            char* buffer = stackalloc char[4096];
            uint length = GetFinalPathName(handle, buffer, 4096, 0);
            if (length == 0) throw NativeError();
            if (length >= 4096) throw Failure(36);
            string result = new(buffer, 0, (int)length);
            if (result.StartsWith(@"\\?\UNC\", StringComparison.OrdinalIgnoreCase)) return @"\\" + result[8..];
            return result.StartsWith(@"\\?\", StringComparison.Ordinal) ? result[4..] : result;
        }
    }
    private static void ValidateOpenFlags(int flags)
    {
        EnsureSupported();
        const int known = 3 | 64 | 128 | 256 | 512 | 1024 | 2048 | 4096 | 65536 | 131072 | 524288 | 1048576;
        if ((flags & ~known) != 0 || (flags & 3) == 3 || (flags & (64 | 65536)) == (64 | 65536)) throw Failure(22);
    }
    private static int UnixOpenFlags(int flags)
    {
        if (!OperatingSystem.IsMacOS()) return flags | 524288;
        int native = (flags & 3) | 0x1000000;
        if ((flags & 64) != 0) native |= 0x200; if ((flags & 128) != 0) native |= 0x800;
        if ((flags & 256) != 0) native |= 0x20000; if ((flags & 512) != 0) native |= 0x400;
        if ((flags & 1024) != 0) native |= 8; if ((flags & 2048) != 0) native |= 4;
        if ((flags & 4096) != 0) native |= 0x400000; if ((flags & 65536) != 0) native |= 0x100000;
        if ((flags & 131072) != 0) native |= 0x100; if ((flags & 1048576) != 0) native |= 0x80;
        return native;
    }
    internal static SafeFileHandle Open(string path, int flags, int mode)
    {
        ValidateOpenFlags(flags);
        if (OperatingSystem.IsWindows())
        {
            uint access = (flags & 3) switch { 0 => 0x80000000u, 1 => 0x40000000u, _ => 0xc0000000u };
            uint disposition = (flags & 64) != 0 ? ((flags & 128) != 0 ? 1u : (flags & 512) != 0 ? 2u : 4u) : (flags & 512) != 0 ? 5u : 3u;
            uint attributes = 0x02000000u | ((flags & 131072) != 0 ? 0x00200000u : 0u) | ((flags & (1048576 | 4096)) != 0 ? 0x80000000u : 0u);
            SafeFileHandle handle;
            fixed (char* p = path) handle = CreateFile(p, access, 7, 0, disposition, attributes, 0);
            if (handle.IsInvalid) { var ex = NativeError(); handle.Dispose(); throw ex; }
            try { if ((flags & 131072) != 0 && (WindowsInfo(handle).Attributes & 0x400) != 0) throw Failure(40); if ((flags & 65536) != 0 && (WindowsInfo(handle).Attributes & 16) == 0) throw Failure(20); return handle; }
            catch { handle.Dispose(); throw; }
        }
        int nativeFlags = UnixOpenFlags(flags);
        byte[] name = Encoding.UTF8.GetBytes(path + "\0"); int fd;
        fixed (byte* p = name) do { fd = OperatingSystem.IsMacOS() && RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? OpenDarwin(p, nativeFlags, 0, 0, 0, 0, 0, 0, (uint)mode) : OpenUnix(p, nativeFlags, (uint)mode); } while (fd < 0 && Marshal.GetLastPInvokeError() == 4);
        if (fd < 0) throw NativeError(); return new SafeFileHandle(fd, true);
    }
    internal static (ulong Device, ulong Inode, ulong High) LockIdentity(SafeFileHandle handle, string path)
    {
        if (OperatingSystem.IsWindows())
        {
            if (!GetFileIdInformation(handle, 18, out var identity, 24)) throw NativeError();
            return (identity.Volume, identity.Low, identity.High);
        }
        var stat = Fstat(handle, path);
        return (stat.st_dev, stat.st_ino, 0);
    }
    internal static PosixStat Stat(string path, bool follow)
    {
        EnsureSupported();
        if (OperatingSystem.IsWindows())
        {
            SafeFileHandle handle;
            fixed (char* p = path) handle = CreateFile(p, 0, 7, 0, 3, 0x02000000u | (follow ? 0u : 0x00200000u), 0);
            if (handle.IsInvalid) { var ex = NativeError(); handle.Dispose(); throw ex; }
            using (handle) return Fstat(handle, path);
        }
        byte* buffer = stackalloc byte[256]; byte[] name = Encoding.UTF8.GetBytes(path + "\0"); int result;
        fixed (byte* p = name) result = OperatingSystem.IsMacOS() && RuntimeInformation.ProcessArchitecture == Architecture.X64 ? (follow ? StatDarwin(p, buffer) : LstatDarwin(p, buffer)) : (follow ? StatUnix(p, buffer) : LstatUnix(p, buffer));
        if (result != 0) throw NativeError(); return ConvertStat(buffer);
    }
    internal static PosixStat Fstat(SafeFileHandle handle, string path)
    {
        EnsureSupported();
        if (OperatingSystem.IsWindows())
        {
            // Query disk metadata directly. In Wine GetFileType inspects file
            // contents, which changes access time even for a stat-only call.
            if (!GetFileInformation(handle, out var info))
            {
                var error = NativeError();
                uint kind = GetFileType(handle);
                if (kind == 2 || kind == 3)
                    return new PosixStat { st_mode = (kind == 2 ? 0x2000U : 0x1000U) | 438, st_nlink = 1, st_blksize = 4096 };
                throw error;
            }
            if (!GetBasicInformation(handle, 0, out var basic, 40) || !GetStandardInformation(handle, 1, out var standard, 24)) throw NativeError();
            return new PosixStat { st_dev = info.VolumeSerial, st_ino = ((ulong)info.IndexHigh << 32) | info.IndexLow, st_nlink = info.Links, st_mode = (uint)(((info.Attributes & 0x400) != 0 ? 0xa000 : ((info.Attributes & 16) != 0 ? 0x4000 : 0x8000)) | ((info.Attributes & 1) != 0 ? 0x124 : 0x1b6)), st_size = (long)(((ulong)info.SizeHigh << 32) | info.SizeLow), st_blksize = 4096, st_blocks = (standard.AllocationSize + 511) / 512, st_atim = WindowsTimespec((long)(((ulong)info.AccessHigh << 32) | info.AccessLow)), st_mtim = WindowsTimespec((long)(((ulong)info.WriteHigh << 32) | info.WriteLow)), st_ctim = WindowsTimespec(basic.ChangeTime) };
        }
        byte* buffer = stackalloc byte[256]; int result = OperatingSystem.IsMacOS() && RuntimeInformation.ProcessArchitecture == Architecture.X64 ? FstatDarwin(handle, buffer) : FstatUnix(handle, buffer);
        if (result != 0) throw NativeError(); return ConvertStat(buffer);
    }
    internal static PosixTimespec WindowsTimespec(long ticks)
    {
        long seconds = Math.DivRem(ticks - 116444736000000000L, 10000000, out long fraction);
        // POSIX fractions remain nonnegative, including dates before the epoch.
        if (fraction < 0) { --seconds; fraction += 10000000; }
        return new() { tv_sec = seconds, tv_nsec = fraction * 100 };
    }
    private static PosixStat ConvertStat(byte* b)
    {
        if (OperatingSystem.IsMacOS()) return new() { st_dev = *(uint*)b, st_mode = *(ushort*)(b + 4), st_nlink = *(ushort*)(b + 6), st_ino = *(ulong*)(b + 8), st_uid = *(uint*)(b + 16), st_gid = *(uint*)(b + 20), st_rdev = *(uint*)(b + 24), st_atim = *(PosixTimespec*)(b + 32), st_mtim = *(PosixTimespec*)(b + 48), st_ctim = *(PosixTimespec*)(b + 64), st_size = *(long*)(b + 96), st_blocks = *(long*)(b + 104), st_blksize = *(int*)(b + 112) };
        bool arm = RuntimeInformation.ProcessArchitecture == Architecture.Arm64;
        return new() { st_dev = *(ulong*)b, st_ino = *(ulong*)(b + 8), st_nlink = arm ? *(uint*)(b + 20) : *(ulong*)(b + 16), st_mode = *(uint*)(b + (arm ? 16 : 24)), st_uid = *(uint*)(b + (arm ? 24 : 28)), st_gid = *(uint*)(b + (arm ? 28 : 32)), st_rdev = *(ulong*)(b + (arm ? 32 : 40)), st_size = *(long*)(b + 48), st_blksize = arm ? *(int*)(b + 56) : *(long*)(b + 56), st_blocks = *(long*)(b + 64), st_atim = *(PosixTimespec*)(b + 72), st_mtim = *(PosixTimespec*)(b + 88), st_ctim = *(PosixTimespec*)(b + 104) };
    }
    internal static uint EffectiveUserId => OperatingSystem.IsWindows() ? 0 : Geteuid();
    internal static void Chmod(SafeFileHandle handle, string path, uint mode)
    {
        if (OperatingSystem.IsWindows())
        {
            if (!GetBasicInformation(handle, 0, out var info, 40)) throw NativeError();
            info.Attributes = (mode & 0x92) == 0 ? info.Attributes | 1 : info.Attributes & ~1u;
            if (info.Attributes == 0) info.Attributes = 0x80;
            // Zero time fields leave timestamps unchanged on SetFileInformation.
            info.CreationTime = info.AccessTime = info.WriteTime = info.ChangeTime = 0;
            if (!SetBasicInformation(handle, 0, ref info, 40)) throw NativeError(); return;
        }
        if (Fchmod(handle, mode) != 0) throw NativeError();
    }
    internal static void Chown(SafeFileHandle handle, uint uid, uint gid) { if (OperatingSystem.IsWindows()) { if (uid is not (0 or uint.MaxValue) || gid is not (0 or uint.MaxValue)) throw Failure(95); return; } if (Fchown(handle, uid, gid) != 0) throw NativeError(); }
    internal static void Sync(SafeFileHandle handle, bool dataOnly = false) { if (OperatingSystem.IsWindows()) { if (!FlushFileBuffers(handle)) throw NativeError(); return; } int rc; do { rc = dataOnly && OperatingSystem.IsLinux() ? Fdatasync(handle) : Fsync(handle); } while (rc < 0 && Marshal.GetLastPInvokeError() == 4); if (rc != 0) throw NativeError(); }
    internal static int GetStatusFlags(SafeFileHandle handle)
    {
        int flags = OperatingSystem.IsMacOS() && RuntimeInformation.ProcessArchitecture == Architecture.Arm64
            ? FcntlDarwin(handle, 3, 0, 0, 0, 0, 0, 0, null)
            : FcntlUnix(handle, 3, null);
        if (flags < 0) throw NativeError();
        return flags;
    }
    internal static void SetStatusFlags(SafeFileHandle handle, int flags)
    {
        if (OperatingSystem.IsWindows()) return;
        int nativeFlags = OperatingSystem.IsMacOS() ? ((flags & 1024) != 0 ? 8 : 0) | ((flags & 2048) != 0 ? 4 : 0) : flags & (1024 | 2048);
        int result = OperatingSystem.IsMacOS() && RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? FcntlDarwin(handle, 4, 0, 0, 0, 0, 0, 0, (void*)(nint)nativeFlags) : FcntlUnix(handle, 4, (void*)(nint)nativeFlags);
        if (result < 0) throw NativeError();
    }
    internal static int Append(SafeFileHandle handle, ReadOnlySpan<byte> data)
    {
        fixed (byte* p = data)
        {
            if (OperatingSystem.IsWindows())
            {
                Overlapped offset = new() { Offset = uint.MaxValue, OffsetHigh = uint.MaxValue };
                if (!WriteFile(handle, p, (uint)data.Length, out uint written, &offset)) throw NativeError();
                return checked((int)written);
            }
            nint n; do { n = WriteUnix(handle, p, (nuint)data.Length); } while (n < 0 && Marshal.GetLastPInvokeError() == 4);
            if (n < 0) throw NativeError(); return checked((int)n);
        }
    }
    internal static SafeFileHandle Duplicate(SafeFileHandle handle)
    {
        int command = OperatingSystem.IsMacOS() ? 67 : 1030; // F_DUPFD_CLOEXEC
        int fd = OperatingSystem.IsMacOS() && RuntimeInformation.ProcessArchitecture == Architecture.Arm64
            ? FcntlDarwin(handle, command, 0, 0, 0, 0, 0, 0, null)
            : FcntlUnix(handle, command, null);
        if (fd < 0) throw NativeError();
        return new SafeFileHandle(fd, true);
    }
    internal static int NativeLock(SafeFileHandle handle, int command, ref PosixFlock value)
    {
        if (OperatingSystem.IsWindows()) throw Failure(95); // Windows uses PosixWindowsLocks, not kernel byte locks.
        int rc;
        if (OperatingSystem.IsMacOS())
        {
            DarwinFlock record = new() { Start = value.l_start, Length = value.l_len, Pid = value.l_pid, Type = (short)(value.l_type == 0 ? 1 : value.l_type == 1 ? 3 : 2), Whence = value.l_whence };
            int op = command switch { 5 => 7, 6 => 8, 7 => 9, 36 => 92, 37 => 90, 38 => 91, _ => throw Failure(22) };
            do { rc = RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? FcntlDarwin(handle, op, 0, 0, 0, 0, 0, 0, &record) : FcntlUnix(handle, op, &record); } while (rc < 0 && Marshal.GetLastPInvokeError() == 4);
            if (rc == 0) { value.l_type = (short)(record.Type == 1 ? 0 : record.Type == 3 ? 1 : 2); value.l_start = record.Start; value.l_len = record.Length; value.l_pid = record.Pid; value.l_whence = record.Whence; }
        }
        else { fixed (PosixFlock* p = &value) do { rc = FcntlUnix(handle, command, p); } while (rc < 0 && Marshal.GetLastPInvokeError() == 4); }
        if (rc == 0) return 0;
        int error = NativeErrorNumber(Marshal.GetLastPInvokeError());
        return command is 36 or 37 or 38 && error is 22 or 95 ? 95 : error;
    }
    private static WindowsFileInformation WindowsInfo(SafeFileHandle handle) { if (!GetFileInformation(handle, out var info)) throw NativeError(); return info; }
    [StructLayout(LayoutKind.Sequential)] private struct DarwinFlock { internal long Start, Length; internal int Pid; internal short Type, Whence; }
    [StructLayout(LayoutKind.Sequential)] private struct WindowsFileId { internal ulong Volume, Low, High; }
    [StructLayout(LayoutKind.Sequential)] private struct WindowsBasicInformation { internal long CreationTime, AccessTime, WriteTime, ChangeTime; internal uint Attributes; }
    [StructLayout(LayoutKind.Sequential)] private struct WindowsStandardInformation { internal long AllocationSize, EndOfFile; internal uint NumberOfLinks; internal byte DeletePending, Directory; }
    [StructLayout(LayoutKind.Sequential)] private struct WindowsFileInformation { internal uint Attributes, CreationLow, CreationHigh, AccessLow, AccessHigh, WriteLow, WriteHigh, VolumeSerial, SizeHigh, SizeLow, Links, IndexHigh, IndexLow; }
    [LibraryImport("libc", EntryPoint = "open", SetLastError = true)] private static partial int OpenUnix(byte* path, int flags, uint mode);
    [LibraryImport("libc", EntryPoint = "realpath", StringMarshalling = StringMarshalling.Utf8, SetLastError = true)] private static partial nint RealpathUnix(string path, nint destination);
    [LibraryImport("libc", EntryPoint = "free")] private static partial void FreeCanonicalPath(nint path);
    [LibraryImport("kernel32.dll", EntryPoint = "GetFinalPathNameByHandleW", SetLastError = true)] private static partial uint GetFinalPathName(SafeFileHandle handle, char* buffer, uint length, uint flags);
    [LibraryImport("libc", EntryPoint = "open", SetLastError = true)] private static partial int OpenDarwin(byte* path, int flags, nint a, nint b, nint c, nint d, nint e, nint f, uint mode);
    [LibraryImport("libc", EntryPoint = "stat", SetLastError = true)] private static partial int StatUnix(byte* path, byte* data);
    [LibraryImport("libc", EntryPoint = "lstat", SetLastError = true)] private static partial int LstatUnix(byte* path, byte* data);
    [LibraryImport("libc", EntryPoint = "fstat", SetLastError = true)] private static partial int FstatUnix(SafeFileHandle handle, byte* data);
    [LibraryImport("libc", EntryPoint = "stat$INODE64", SetLastError = true)] private static partial int StatDarwin(byte* path, byte* data);
    [LibraryImport("libc", EntryPoint = "lstat$INODE64", SetLastError = true)] private static partial int LstatDarwin(byte* path, byte* data);
    [LibraryImport("libc", EntryPoint = "fstat$INODE64", SetLastError = true)] private static partial int FstatDarwin(SafeFileHandle handle, byte* data);
    [LibraryImport("libc", EntryPoint = "fchmod", SetLastError = true)] private static partial int Fchmod(SafeFileHandle handle, uint mode);
    [LibraryImport("libc", EntryPoint = "fchown", SetLastError = true)] private static partial int Fchown(SafeFileHandle handle, uint uid, uint gid);
    [LibraryImport("libc", EntryPoint = "geteuid")] private static partial uint Geteuid();
    [LibraryImport("libc", EntryPoint = "fsync", SetLastError = true)] private static partial int Fsync(SafeFileHandle handle);
    [LibraryImport("libc", EntryPoint = "fdatasync", SetLastError = true)] private static partial int Fdatasync(SafeFileHandle handle);
    [LibraryImport("libc", EntryPoint = "write", SetLastError = true)] private static partial nint WriteUnix(SafeFileHandle handle, byte* data, nuint count);
    [LibraryImport("libc", EntryPoint = "fcntl", SetLastError = true)] private static partial int FcntlUnix(SafeFileHandle handle, int command, void* data);
    [LibraryImport("libc", EntryPoint = "fcntl", SetLastError = true)] private static partial int FcntlDarwin(SafeFileHandle handle, int command, nint a, nint b, nint c, nint d, nint e, nint f, void* data);
    [StructLayout(LayoutKind.Sequential)] private struct Overlapped { internal nuint Internal, InternalHigh; internal uint Offset, OffsetHigh; internal nint Event; }
    [LibraryImport("kernel32.dll", EntryPoint = "WriteFile", SetLastError = true)][return: MarshalAs(UnmanagedType.Bool)] private static partial bool WriteFile(SafeFileHandle handle, byte* data, uint count, out uint written, Overlapped* offset);
    [LibraryImport("kernel32.dll", EntryPoint = "CreateFileW", SetLastError = true)] private static partial SafeFileHandle CreateFile(char* path, uint access, uint share, nint security, uint disposition, uint flags, nint template);
    [LibraryImport("kernel32.dll", EntryPoint = "GetFileInformationByHandleEx", SetLastError = true)][return: MarshalAs(UnmanagedType.Bool)] private static partial bool GetFileIdInformation(SafeFileHandle handle, int type, out WindowsFileId info, uint size);
    [LibraryImport("kernel32.dll", EntryPoint = "GetFileInformationByHandleEx", SetLastError = true)][return: MarshalAs(UnmanagedType.Bool)] private static partial bool GetBasicInformation(SafeFileHandle handle, int type, out WindowsBasicInformation info, uint size);
    [LibraryImport("kernel32.dll", EntryPoint = "GetFileInformationByHandleEx", SetLastError = true)][return: MarshalAs(UnmanagedType.Bool)] private static partial bool GetStandardInformation(SafeFileHandle handle, int type, out WindowsStandardInformation info, uint size);
    [LibraryImport("kernel32.dll", EntryPoint = "SetFileInformationByHandle", SetLastError = true)][return: MarshalAs(UnmanagedType.Bool)] private static partial bool SetBasicInformation(SafeFileHandle handle, int type, ref WindowsBasicInformation info, uint size);
    [LibraryImport("kernel32.dll", EntryPoint = "GetFileInformationByHandle", SetLastError = true)][return: MarshalAs(UnmanagedType.Bool)] private static partial bool GetFileInformation(SafeFileHandle handle, out WindowsFileInformation info);
    [LibraryImport("kernel32.dll", EntryPoint = "GetFileType", SetLastError = true)] private static partial uint GetFileType(SafeFileHandle handle);
    [LibraryImport("kernel32.dll", EntryPoint = "FlushFileBuffers", SetLastError = true)][return: MarshalAs(UnmanagedType.Bool)] private static partial bool FlushFileBuffers(SafeFileHandle handle);
}
