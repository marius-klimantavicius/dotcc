#nullable enable
using System;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    // Linux LP64 iovec. The void* public boundary also accepts generated iovec*.
    private struct VectorIoEntry { public void* Base; public ulong Length; }

    public static long readv(int fd, void* vectors, int count) => TransferVector(fd, vectors, count, false);
    public static long writev(int fd, void* vectors, int count) => TransferVector(fd, vectors, count, true);

    private static long TransferVector(int fd, void* vectors, int count, bool writing)
    {
        if (count < 0 || count > 1024) { errno = EINVAL; return -1; }
        if (vectors == null && count != 0) { errno = EFAULT; return -1; }
        if (SlotByFd(fd) is not { } slot) { errno = EBADF; return -1; }
        var entries = (VectorIoEntry*)vectors;
        ulong total = 0;
        for (int i = 0; i < count; i++)
        {
            if (entries[i].Length > (ulong)long.MaxValue - total) { errno = EINVAL; return -1; }
            if (entries[i].Base == null && entries[i].Length != 0) { errno = EFAULT; return -1; }
            total += entries[i].Length;
        }
        if (total == 0) return 0;
        // Like Linux's MAX_RW_COUNT, one operation can transfer fewer than the
        // requested bytes. Bound a single BCL buffer without summation overflow.
        int capacity = (int)Math.Min(total, 0x7ffff000UL);
        byte[] buffer;
        try { buffer = global::System.Buffers.ArrayPool<byte>.Shared.Rent(capacity); }
        catch (OutOfMemoryException) { errno = ENOMEM; return -1; }
        try
        {
            if (writing)
            {
                int offset = 0;
                for (int i = 0; i < count && offset < capacity; i++)
                {
                    int size = (int)Math.Min(entries[i].Length, (ulong)(capacity - offset));
                    new ReadOnlySpan<byte>(entries[i].Base, size).CopyTo(buffer.AsSpan(offset, size));
                    offset += size;
                }
            }
            long transferred;
            // One socket send/receive preserves a datagram or TCP short-I/O
            // boundary. Never issue one blocking receive per vector.
            lock (slot)
            {
                fixed (byte* bytes = buffer)
                    transferred = writing ? write(fd, bytes, (ulong)capacity) : read(fd, bytes, (ulong)capacity);
            }
            if (!writing && transferred > 0)
            {
                int offset = 0;
                for (int i = 0; i < count && offset < transferred; i++)
                {
                    int size = (int)Math.Min(entries[i].Length, (ulong)(transferred - offset));
                    buffer.AsSpan(offset, size).CopyTo(new Span<byte>(entries[i].Base, size));
                    offset += size;
                }
            }
            return transferred;
        }
        finally { global::System.Buffers.ArrayPool<byte>.Shared.Return(buffer, clearArray: true); }
    }
}
