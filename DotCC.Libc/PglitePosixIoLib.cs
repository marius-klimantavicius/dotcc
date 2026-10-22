#nullable enable
using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    private const ulong PositionedTransferLimit = 0x7ffff000;
    public const uint SYNC_FILE_RANGE_WAIT_BEFORE = 1;
    public const uint SYNC_FILE_RANGE_WRITE = 2;
    public const uint SYNC_FILE_RANGE_WAIT_AFTER = 4;

    public static long pread(int descriptor, void* buffer, ulong count, long offset) =>
        TransferPositioned(descriptor, buffer, count, offset, false);
    public static long pwrite(int descriptor, void* buffer, ulong count, long offset) =>
        TransferPositioned(descriptor, buffer, count, offset, true);
    public static long preadv(int descriptor, void* vectors, int count, long offset) =>
        TransferPositionedVector(descriptor, vectors, count, offset, false);
    public static long pwritev(int descriptor, void* vectors, int count, long offset) =>
        TransferPositionedVector(descriptor, vectors, count, offset, true);

    private static FileStream? PositionedFile(int descriptor, bool writing)
    {
        var slot = SlotByFd(descriptor);
        if (slot is null) { errno = EBADF; return null; }
        if (slot.Stream is not FileStream file) { errno = ESPIPE; return null; }
        if (writing ? !file.CanWrite : !file.CanRead) { errno = EBADF; return null; }
        return file;
    }

    private static long PositionedError(Exception error)
    {
        errno = error is ObjectDisposedException ? EBADF : error is UnauthorizedAccessException ? EACCES :
            error is ArgumentException ? EINVAL : error is OutOfMemoryException ? ENOMEM :
            error is NotSupportedException ? ESPIPE : EIO;
        return -1;
    }

    private static long TransferPositioned(int descriptor, void* buffer, ulong count, long offset, bool writing)
    {
        if (offset < 0 || count > long.MaxValue) { errno = EINVAL; return -1; }
        if (buffer == null && count != 0) { errno = EFAULT; return -1; }
        var file = PositionedFile(descriptor, writing);
        if (file is null) return -1;
        count = Math.Min(count, PositionedTransferLimit);
        if (count > (ulong)(long.MaxValue - offset)) { errno = EINVAL; return -1; }
        try
        {
            // SafeHandle marshalling retains the actual owning OS descriptor
            // across close races. A synthetic Libc fd never reaches the OS.
            // Linux also retains its documented O_APPEND pwrite behavior.
            if (OperatingSystem.IsLinux())
            {
                long result = writing ? PositionedLinuxWrite(file.SafeFileHandle, buffer, count, offset) :
                    PositionedLinuxRead(file.SafeFileHandle, buffer, count, offset);
                if (result < 0) errno = Marshal.GetLastPInvokeError();
                return result;
            }
            if (!file.CanSeek) { errno = ESPIPE; return -1; }
            // These BCL primitives issue positioned I/O; neither path seeks or
            // changes FileStream.Position, even for sibling worker calls.
            if (writing)
            {
                RandomAccess.Write(file.SafeFileHandle, new ReadOnlySpan<byte>(buffer, (int)count), offset);
                return (long)count;
            }
            return RandomAccess.Read(file.SafeFileHandle, new Span<byte>(buffer, (int)count), offset);
        }
        catch (Exception error) when (error is IOException or ObjectDisposedException or UnauthorizedAccessException or
                                             ArgumentException or NotSupportedException or OutOfMemoryException)
        { return PositionedError(error); }
    }

    private static long TransferPositionedVector(int descriptor, void* vectors, int count, long offset, bool writing)
    {
        if (offset < 0 || count < 0 || count > 1024) { errno = EINVAL; return -1; }
        if (vectors == null && count != 0) { errno = EFAULT; return -1; }
        var file = PositionedFile(descriptor, writing);
        if (file is null) return -1;
        var entries = (VectorIoEntry*)vectors;
        ulong total = 0;
        for (int i = 0; i < count; i++)
        {
            if (entries[i].Length > (ulong)long.MaxValue - total) { errno = EINVAL; return -1; }
            if (entries[i].Base == null && entries[i].Length != 0) { errno = EFAULT; return -1; }
            total += entries[i].Length;
        }
        int capacity = (int)Math.Min(total, PositionedTransferLimit);
        if (capacity > long.MaxValue - offset) { errno = EINVAL; return -1; }
        try
        {
            if (OperatingSystem.IsLinux())
            {
                long result = writing ? PositionedLinuxWriteVector(file.SafeFileHandle, vectors, count, offset) :
                    PositionedLinuxReadVector(file.SafeFileHandle, vectors, count, offset);
                if (result < 0) errno = Marshal.GetLastPInvokeError();
                return result;
            }
            if (capacity == 0) return TransferPositioned(descriptor, null, 0, offset, writing);
            byte[] buffer = global::System.Buffers.ArrayPool<byte>.Shared.Rent(capacity);
            try
            {
                if (writing)
                {
                    int cursor = 0;
                    for (int i = 0; i < count && cursor < capacity; i++)
                    {
                        int size = (int)Math.Min(entries[i].Length, (ulong)(capacity - cursor));
                        new ReadOnlySpan<byte>(entries[i].Base, size).CopyTo(buffer.AsSpan(cursor, size));
                        cursor += size;
                    }
                }
                long result;
                fixed (byte* data = buffer) result = TransferPositioned(descriptor, data, (ulong)capacity, offset, writing);
                if (!writing && result > 0)
                {
                    int cursor = 0;
                    for (int i = 0; i < count && cursor < result; i++)
                    {
                        int size = (int)Math.Min(entries[i].Length, (ulong)(result - cursor));
                        buffer.AsSpan(cursor, size).CopyTo(new Span<byte>(entries[i].Base, size));
                        cursor += size;
                    }
                }
                return result;
            }
            finally { global::System.Buffers.ArrayPool<byte>.Shared.Return(buffer, clearArray: true); }
        }
        catch (Exception error) when (error is IOException or ObjectDisposedException or UnauthorizedAccessException or
                                             ArgumentException or NotSupportedException or OutOfMemoryException)
        { return PositionedError(error); }
    }

    /// <summary>Synchronize the filesystem containing an owned Linux handle.
    /// Other platforms reject this Linux-specific service explicitly.</summary>
    public static int syncfs(int descriptor) => SynchronizeLinux(descriptor, 0, 0, 0, true);

    /// <summary>Real Linux writeback/range wait service. This is a writeback
    /// hint, not the fsync durability contract. PostgreSQL still performs its
    /// original final fsyncs. Unsupported platforms return ENOTSUP.</summary>
    public static int sync_file_range(int descriptor, long offset, long count, uint flags) =>
        SynchronizeLinux(descriptor, offset, count, flags, false);

    private static int SynchronizeLinux(int descriptor, long offset, long count, uint flags, bool filesystem)
    {
        if (offset < 0 || count < 0 || count > long.MaxValue - offset || (flags & ~7U) != 0)
        { errno = EINVAL; return -1; }
        var slot = SlotByFd(descriptor);
        if (slot is null) { errno = EBADF; return -1; }
        if (slot.Stream is not FileStream file) { errno = EINVAL; return -1; }
        if (!OperatingSystem.IsLinux()) { errno = ENOTSUP; return -1; }
        try
        {
            int result = filesystem ? PositionedLinuxSyncFilesystem(file.SafeFileHandle) :
                PositionedLinuxSyncRange(file.SafeFileHandle, offset, count, flags);
            if (result < 0) errno = Marshal.GetLastPInvokeError();
            return result;
        }
        catch (EntryPointNotFoundException) { errno = ENOSYS; return -1; }
        catch (Exception error) when (error is IOException or ObjectDisposedException or UnauthorizedAccessException or
                                             ArgumentException or NotSupportedException)
        { return (int)PositionedError(error); }
    }

    [DllImport("libc", EntryPoint = "pread", SetLastError = true)]
    private static extern long PositionedLinuxRead(SafeFileHandle handle, void* buffer, ulong count, long offset);
    [DllImport("libc", EntryPoint = "pwrite", SetLastError = true)]
    private static extern long PositionedLinuxWrite(SafeFileHandle handle, void* buffer, ulong count, long offset);
    [DllImport("libc", EntryPoint = "preadv", SetLastError = true)]
    private static extern long PositionedLinuxReadVector(SafeFileHandle handle, void* vectors, int count, long offset);
    [DllImport("libc", EntryPoint = "pwritev", SetLastError = true)]
    private static extern long PositionedLinuxWriteVector(SafeFileHandle handle, void* vectors, int count, long offset);
    [DllImport("libc", EntryPoint = "syncfs", SetLastError = true)]
    private static extern int PositionedLinuxSyncFilesystem(SafeFileHandle handle);
    [DllImport("libc", EntryPoint = "sync_file_range", SetLastError = true)]
    private static extern int PositionedLinuxSyncRange(SafeFileHandle handle, long offset, long count, uint flags);
}
