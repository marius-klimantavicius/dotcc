using global::System;
using global::System.IO;
using global::System.Text;
using Managed.Emulation.Host;

namespace Managed.Emulation;

/// <summary>Calls block on the dedicated C worker. Host pointers are borrowed
/// for this call only; async BCL operations use bounded owned byte arrays.</summary>
public static partial class Blink
{
    [ThreadStatic] private static InstanceIo? io;
    private const int IoChunk = 65536, IoVectorLimit = 1024, PathLimit = 4096;
    private static readonly UTF8Encoding PathEncoding = new(false, true);
    public static void BindHostIo(InstanceIo value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (io != null) throw new InvalidOperationException("I/O already bound on this worker.");
        io = value;
    }
    public static void UnbindHostIo() => io = null;
    private static int IoError(int error) { Libc.errno = error; return -1; }
    private static long IoResult<T>(HostResult<T> result) where T : global::System.Numerics.INumber<T>
        => result.Succeeded ? long.CreateChecked(result.Value) : IoError((int)result.Error);
    private static int IoException(Exception exception) => IoError(exception is OutOfMemoryException ? 12 : 5);

    public static unsafe int blink_io_open(byte* path, int flags)
        => blink_io_open_at(-100, path, flags);
    public static unsafe int blink_io_open_at(int directoryFd, byte* path, int flags)
    {
        try
        {
            if (io == null) return IoError(19);
            if (path == null) return IoError(14);
            const int supported = 3 | 64 | 128 | 256 | 512 | 1024 | 65536 | 131072 | 524288;
            if ((flags & ~supported) != 0) return IoError(95);
            if ((flags & 3) == 3) return IoError(22);
            if ((flags & (64 | 65536)) == (64 | 65536)) return IoError(22);
            int length = 0;
            while (length < PathLimit && path[length] != 0) ++length;
            if (length == PathLimit) return IoError(36);
            string name;
            try { name = PathEncoding.GetString(new ReadOnlySpan<byte>(path, length)); }
            catch (DecoderFallbackException) { return IoError(22); }
            return (int)IoResult(io.OpenFileAt(directoryFd, name, (flags & 3) == 0 ? FileAccessMode.Read : (flags & 3) == 1 ? FileAccessMode.Write : FileAccessMode.Read | FileAccessMode.Write,
                (flags & 64) != 0, (flags & 128) != 0, (flags & 512) != 0, (flags & 1024) != 0,
                (flags & 524288) != 0, (flags & 65536) != 0, (flags & 131072) != 0));
        }
        catch (Exception error) { return IoException(error); }
    }
    public static int blink_io_control(int fd, int command, int argument)
    {
        try
        {
            if (io == null) return IoError(19);
            return (int)IoResult(command switch
            {
                0 => io.Duplicate(fd, argument),
                1 => io.GetDescriptorFlags(fd),
                2 => io.SetDescriptorFlags(fd, argument),
                3 => io.GetStatusFlags(fd),
                4 => io.SetStatusFlags(fd, argument),
                1030 => io.Duplicate(fd, argument, true),
                _ => HostResult<int>.Failure(io.GetDescriptorFlags(fd).Error == GuestError.BadDescriptor ? GuestError.BadDescriptor : GuestError.Unsupported)
            });
        }
        catch (Exception error) { return IoException(error); }
    }
    public static int blink_io_close(int fd)
    {
        try { return io == null ? IoError(19) : (int)IoResult(io.Close(fd)); }
        catch (Exception error) { return IoException(error); }
    }
    public static int blink_io_dup(int fd)
    {
        try { return io == null ? IoError(19) : (int)IoResult(io.Duplicate(fd)); }
        catch (Exception error) { return IoException(error); }
    }
    public static long blink_io_seek(int fd, long offset, int origin)
    {
        try
        {
            if (io == null) return IoError(19);
            if ((uint)origin > 2) return IoError(22);
            return IoResult(io.Seek(fd, offset, (SeekOrigin)origin));
        }
        catch (Exception error) { return IoException(error); }
    }
    public static unsafe long blink_io_read(int fd, void* destination, ulong length)
    {
        try
        {
            if (io == null) return IoError(19);
            if (destination == null && length != 0) return IoError(14);
            byte[] buffer = new byte[(int)global::System.Math.Min(length, (ulong)IoChunk)];
            var result = io.ReadAsync(fd, buffer).GetAwaiter().GetResult();
            if (result.Succeeded) buffer.AsSpan(0, result.Value).CopyTo(new Span<byte>(destination, buffer.Length));
            return IoResult(result);
        }
        catch (Exception error) { return IoException(error); }
    }
    public static unsafe long blink_io_pread(int fd, void* destination, ulong length, long offset)
    {
        try
        {
            if (io == null) return IoError(19);
            if (destination == null && length != 0) return IoError(14);
            int count = (int)global::System.Math.Min(length, (ulong)IoChunk);
            return IoResult(io.ReadAt(fd, new Span<byte>(destination, count), offset));
        }
        catch (Exception error) { return IoException(error); }
    }
    public static unsafe int blink_io_read_at_length(int fd, long* length)
    {
        try
        {
            if (io == null) return IoError(19);
            if (length == null) return IoError(14);
            var result = io.ReadAtLength(fd);
            if (!result.Succeeded) return IoError((int)result.Error);
            *length = result.Value;
            return 0;
        }
        catch (Exception error) { return IoException(error); }
    }
    public static unsafe long blink_io_write(int fd, void* source, ulong length)
    {
        try
        {
            if (io == null) return IoError(19);
            if (source == null && length != 0) return IoError(14);
            int count = (int)global::System.Math.Min(length, (ulong)IoChunk);
            byte[] buffer = new ReadOnlySpan<byte>(source, count).ToArray();
            return IoResult(io.WriteAsync(fd, buffer).GetAwaiter().GetResult());
        }
        catch (Exception error) { return IoException(error); }
    }
    public static unsafe long blink_io_readv(int fd, blink_host_iovec* vectors, int count) => IoVectors(fd, vectors, count, false);
    public static unsafe long blink_io_writev(int fd, blink_host_iovec* vectors, int count) => IoVectors(fd, vectors, count, true);
    private static unsafe long IoVectors(int fd, blink_host_iovec* vectors, int count, bool writing)
    {
        try
        {
            if (io == null) return IoError(19);
            if ((uint)count > IoVectorLimit) return IoError(22);
            if (vectors == null && count != 0) return IoError(14);
            ulong total = 0;
            for (int i = 0; i < count; ++i)
            {
                if (vectors[i].iov_base == null && vectors[i].iov_len != 0) return IoError(14);
                if (vectors[i].iov_len > (ulong)long.MaxValue - total) return IoError(22);
                total += vectors[i].iov_len;
            }
            byte[] buffer = new byte[(int)global::System.Math.Min(total, (ulong)IoChunk)];
            if (writing) CopyVectors(vectors, count, buffer, true);
            var result = (writing ? io.WriteAsync(fd, buffer) : io.ReadAsync(fd, buffer)).GetAwaiter().GetResult();
            if (!writing && result.Succeeded) CopyVectors(vectors, count, buffer.AsSpan(0, result.Value), false);
            return IoResult(result);
        }
        catch (Exception error) { return IoException(error); }
    }
    private static unsafe void CopyVectors(blink_host_iovec* vectors, int count, Span<byte> bytes, bool writing)
    {
        int offset = 0;
        for (int i = 0; i < count && offset < bytes.Length; ++i)
        {
            int length = (int)global::System.Math.Min(vectors[i].iov_len, (ulong)(bytes.Length - offset));
            var memory = new Span<byte>(vectors[i].iov_base, length);
            if (writing) memory.CopyTo(bytes.Slice(offset, length));
            else bytes.Slice(offset, length).CopyTo(memory);
            offset += length;
        }
    }
}
