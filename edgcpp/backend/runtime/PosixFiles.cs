#nullable enable
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Edg.Runtime;

public sealed unsafe partial class PosixContext
{
    public int OpenDescriptorCount { get { lock (descriptorsGate) return descriptors.Count - (descriptors.ContainsKey(0) ? 1 : 0) - (descriptors.ContainsKey(1) ? 1 : 0) - (descriptors.ContainsKey(2) ? 1 : 0); } }

    public long Read(int fd, void* buffer, ulong count) => Transfer(fd, buffer, count, 0, false, false);
    public long Write(int fd, void* buffer, ulong count) => Transfer(fd, buffer, count, 0, true, false);
    public long Pread(int fd, void* buffer, ulong count, long offset) => Transfer(fd, buffer, count, offset, false, true);
    public long Pwrite(int fd, void* buffer, ulong count, long offset) => Transfer(fd, buffer, count, offset, true, true);

    private long Transfer(int fd, void* buffer, ulong count, long offset, bool writing, bool positioned)
    {
        Descriptor? descriptor = Find(fd); if (descriptor is null) return -1;
        if (count > long.MaxValue || (positioned && offset < 0)) { SetError(22); return -1; }
        if (buffer == null && count != 0) { SetError(14); return -1; }
        if (descriptor.Socket is not null) return SocketTransfer(descriptor, buffer, count, writing, positioned);
        lock (descriptor.Gate)
        {
            if (descriptor.Closed || (writing ? (descriptor.Flags & 3) == 0 : (descriptor.Flags & 3) == 1))
            { SetError(9); return -1; }
            try
            {
                int length = (int)Math.Min(count, int.MaxValue);
                if (descriptor.Device != 0)
                {
                    if (positioned) { SetError(29); return -1; }
                    if (descriptor.StandardStream is Stream standard)
                    {
                        if (writing) { standard.Write(new ReadOnlySpan<byte>(buffer, length)); return length; }
                        return standard.Read(new Span<byte>(buffer, length));
                    }
                    if (writing) return length;
                    if (descriptor.Device == 1) return 0;
                    RandomNumberGenerator.Fill(new Span<byte>(buffer, length)); return length;
                }
                long position = positioned ? offset : descriptor.Position;
                int transferred;
                if (writing)
                {
                    if (!positioned && (descriptor.Flags & 0x400) != 0)
                    {
                        transferred = PosixPlatform.Append(descriptor.Handle, new ReadOnlySpan<byte>(buffer, length));
                        descriptor.Position = RandomAccess.GetLength(descriptor.Handle);
                        return transferred;
                    }
                    // Linux pwrite otherwise appends when O_APPEND is set. The
                    // descriptor gate excludes every other operation using this
                    // open-file description while its status is changed.
                    bool restoreAppend = positioned && (descriptor.Flags & 0x400) != 0 && OperatingSystem.IsLinux();
                    if (restoreAppend) PosixPlatform.SetStatusFlags(descriptor.Handle, descriptor.Flags & ~0x400);
                    try { RandomAccess.Write(descriptor.Handle, new ReadOnlySpan<byte>(buffer, length), position); }
                    finally { if (restoreAppend) PosixPlatform.SetStatusFlags(descriptor.Handle, descriptor.Flags); }
                    transferred = length;
                }
                else transferred = RandomAccess.Read(descriptor.Handle, new Span<byte>(buffer, length), position);
                if (!positioned) descriptor.Position = checked(position + transferred);
                return transferred;
            }
            catch (Exception error) when (PlatformFailure(error)) { return Fail(error); }
        }
    }

    public long Lseek(int fd, long offset, int whence)
    {
        Descriptor? descriptor = Find(fd); if (descriptor is null) return -1;
        lock (descriptor.Gate)
        {
            if (descriptor.Closed) { SetError(9); return -1; }
            if (descriptor.Device != 0 || descriptor.Socket is not null) { SetError(29); return -1; }
            try
            {
                long origin;
                switch (whence) { case 0: origin = 0; break; case 1: origin = descriptor.Position; break; case 2: origin = RandomAccess.GetLength(descriptor.Handle); break; default: SetError(22); return -1; }
                if ((offset > 0 && origin > long.MaxValue - offset) || (offset < 0 && offset < -origin)) { SetError(22); return -1; }
                long position = origin + offset; if (position < 0) { SetError(22); return -1; }
                return descriptor.Position = position;
            }
            catch (Exception error) when (PlatformFailure(error)) { return Fail(error); }
        }
    }

    public int Ftruncate(int fd, long length)
    {
        Descriptor? descriptor = Find(fd); if (descriptor is null) return -1;
        lock (descriptor.Gate)
        {
            if (descriptor.Closed || (descriptor.Flags & 3) == 0) { SetError(9); return -1; }
            if (length < 0 || (descriptor.Device != 0 || descriptor.Socket is not null)) { SetError(22); return -1; }
            try { RandomAccess.SetLength(descriptor.Handle, length); return 0; }
            catch (Exception error) when (PlatformFailure(error)) { return Fail(error); }
        }
    }

    public int Fsync(int fd) => Sync(fd, false);
    public int Fdatasync(int fd) => Sync(fd, true);
    private int Sync(int fd, bool dataOnly)
    {
        Descriptor? descriptor = Find(fd); if (descriptor is null) return -1;
        lock (descriptor.Gate)
        {
            if (descriptor.Closed) { SetError(9); return -1; }
            if ((descriptor.Device != 0 || descriptor.Socket is not null)) { SetError(22); return -1; }
            try { PosixPlatform.Sync(descriptor.Handle, dataOnly); return 0; }
            catch (Exception error) when (PlatformFailure(error)) { return Fail(error); }
        }
    }

    public int Stat(sbyte* path, PosixStat* destination) => StatPath(path, destination, true);
    public int Lstat(sbyte* path, PosixStat* destination) => StatPath(path, destination, false);
    private int StatPath(sbyte* path, PosixStat* destination, bool follow)
    {
        if (destination == null) { SetError(14); return -1; }
        try { *destination = PosixPlatform.Stat(Resolve(path), follow); return 0; }
        catch (Exception error) when (PlatformFailure(error)) { return Fail(error); }
    }
    public int Fstat(int fd, PosixStat* destination)
    {
        if (destination == null) { SetError(14); return -1; }
        Descriptor? descriptor = Find(fd); if (descriptor is null) return -1;
        lock (descriptor.Gate)
        {
            if (descriptor.Closed) { SetError(9); return -1; }
            if (descriptor.Socket is not null) { SetError(95); return -1; }
            try
            {
                *destination = (descriptor.Device == 0 || descriptor.NativeStandard) ? PosixPlatform.Fstat(descriptor.Handle, descriptor.Path) : new PosixStat { st_mode = 0x2000 | 438, st_nlink = 1, st_blksize = 4096 };
                return 0;
            }
            catch (Exception error) when (PlatformFailure(error)) { return Fail(error); }
        }
    }
    public int Fchmod(int fd, uint mode)
    {
        Descriptor? descriptor = Find(fd); if (descriptor is null) return -1;
        lock (descriptor.Gate)
        {
            if (descriptor.Closed) { SetError(9); return -1; }
            if (descriptor.Socket is not null) { SetError(95); return -1; }
            try { PosixPlatform.Chmod(descriptor.Handle, descriptor.Path, mode); return 0; }
            catch (Exception error) when (PlatformFailure(error)) { return Fail(error); }
        }
    }
    public int Fchown(int fd, uint owner, uint group)
    {
        Descriptor? descriptor = Find(fd); if (descriptor is null) return -1;
        lock (descriptor.Gate)
        {
            if (descriptor.Closed) { SetError(9); return -1; }
            if (descriptor.Socket is not null) { SetError(95); return -1; }
            try { PosixPlatform.Chown(descriptor.Handle, owner, group); return 0; }
            catch (Exception error) when (PlatformFailure(error)) { return Fail(error); }
        }
    }
    public uint Geteuid() => PosixPlatform.EffectiveUserId;

    public sbyte* Getcwd(sbyte* destination, ulong size)
    {
        if (destination == null) { SetError(22); return null; }
        try
        {
            string path = PortablePath(workingDirectory);
            int length = Utf8.GetByteCount(path);
            if (size <= (ulong)length) { SetError(34); return null; }
            Utf8.GetBytes(path, new Span<byte>(destination, length)); destination[length] = 0;
            return destination;
        }
        catch (Exception error) when (PlatformFailure(error)) { Fail(error); return null; }
    }

    private static void NativePathResult(int result) { if (result != 0) throw new Win32Exception(Marshal.GetLastPInvokeError()); }

    public int Access(sbyte* path, int mode)
    {
        if ((mode & ~7) != 0) { SetError(22); return -1; }
        try
        {
            string name = Resolve(path);
            if (!OperatingSystem.IsWindows()) NativePathResult(AccessUnix(name, mode));
            else
            {
                FileAttributes attributes = File.GetAttributes(name);
                if ((mode & 2) != 0 && (attributes & (FileAttributes.ReadOnly | FileAttributes.Directory)) == FileAttributes.ReadOnly)
                { SetError(13); return -1; }
                // POSIX execute permission has no Windows equivalent; metadata
                // existence and the read-only attribute define this emulation.
            }
            return 0;
        }
        catch (Exception error) when (PlatformFailure(error)) { return Fail(error); }
    }
    public int Unlink(sbyte* path)
    {
        try
        {
            string name = Resolve(path);
            if (OperatingSystem.IsWindows()) { if (DeleteFileWindows(name) == 0) throw new Win32Exception(Marshal.GetLastPInvokeError()); }
            else NativePathResult(UnlinkUnix(name));
            return 0;
        }
        catch (Exception error) when (PlatformFailure(error)) { return Fail(error); }
    }
    public int Mkdir(sbyte* path, uint mode)
    {
        try
        {
            string name = Resolve(path);
            if (OperatingSystem.IsWindows()) { if (CreateDirectoryWindows(name, 0) == 0) throw new Win32Exception(Marshal.GetLastPInvokeError()); }
            else NativePathResult(MkdirUnix(name, mode));
            return 0;
        }
        catch (Exception error) when (PlatformFailure(error)) { return Fail(error); }
    }
    public int Rmdir(sbyte* path)
    {
        try
        {
            string name = Resolve(path);
            if (OperatingSystem.IsWindows()) { if (RemoveDirectoryWindows(name) == 0) throw new Win32Exception(Marshal.GetLastPInvokeError()); }
            else NativePathResult(RmdirUnix(name));
            return 0;
        }
        catch (Exception error) when (PlatformFailure(error)) { return Fail(error); }
    }
    public long Readlink(sbyte* path, sbyte* destination, ulong size)
    {
        if (destination == null || size == 0) { SetError(destination == null ? 14 : 22); return -1; }
        try
        {
            string name = Resolve(path);
            if (!OperatingSystem.IsWindows())
            {
                nint count = ReadlinkUnix(name, destination, (nuint)Math.Min(size, int.MaxValue));
                if (count < 0) throw new Win32Exception(Marshal.GetLastPInvokeError());
                return count;
            }
            string? target = new FileInfo(name).LinkTarget;
            if (target is null) { File.GetAttributes(name); SetError(22); return -1; }
            byte[] bytes = Utf8.GetBytes(PortablePath(target));
            int length = (int)Math.Min((ulong)bytes.Length, size);
            bytes.AsSpan(0, length).CopyTo(new Span<byte>(destination, length)); return length;
        }
        catch (Exception error) when (PlatformFailure(error)) { return Fail(error); }
    }

    [LibraryImport("libc", EntryPoint = "access", StringMarshalling = StringMarshalling.Utf8, SetLastError = true)]
    private static partial int AccessUnix(string path, int mode);
    [LibraryImport("libc", EntryPoint = "unlink", StringMarshalling = StringMarshalling.Utf8, SetLastError = true)]
    private static partial int UnlinkUnix(string path);
    [LibraryImport("libc", EntryPoint = "mkdir", StringMarshalling = StringMarshalling.Utf8, SetLastError = true)]
    private static partial int MkdirUnix(string path, uint mode);
    [LibraryImport("libc", EntryPoint = "rmdir", StringMarshalling = StringMarshalling.Utf8, SetLastError = true)]
    private static partial int RmdirUnix(string path);
    [LibraryImport("libc", EntryPoint = "readlink", StringMarshalling = StringMarshalling.Utf8, SetLastError = true)]
    private static partial nint ReadlinkUnix(string path, sbyte* buffer, nuint size);
    [LibraryImport("kernel32.dll", EntryPoint = "DeleteFileW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    private static partial int DeleteFileWindows(string path);
    [LibraryImport("kernel32.dll", EntryPoint = "CreateDirectoryW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    private static partial int CreateDirectoryWindows(string path, nint security);
    [LibraryImport("kernel32.dll", EntryPoint = "RemoveDirectoryW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    private static partial int RemoveDirectoryWindows(string path);
}
