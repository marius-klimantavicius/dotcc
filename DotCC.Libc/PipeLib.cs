#nullable enable
using System;
using System.Threading;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    // Managed anonymous pipes have Linux PIPE_BUF atomicity and a bounded buffer.
    // Descriptors and endpoints belong to the current runtime, as files do.
    private sealed class ManagedPipe
    {
        internal readonly object Sync = new();
        internal readonly byte[] Bytes = new byte[65536];
        internal int Head, Count;
        internal bool ReaderOpen = true, WriterOpen = true;
    }

    public static int pipe(int* pipefd)
    {
        if (pipefd == null) return FcntlError(EFAULT);
        var backing = new ManagedPipe();
        var state = FileDescriptors;
        lock (state.Sync)
        {
            int Register(FileSlot slot)
            {
                for (int i = 3; i < state.Files.Count; i++)
                    if (state.Files[i] is null) { state.Files[i] = slot; return i; }
                state.Files.Add(slot);
                return state.Files.Count - 1;
            }
            pipefd[0] = Register(new() { Kind = FileSlot.K.Pipe, Pipe = backing });
            pipefd[1] = Register(new() { Kind = FileSlot.K.Pipe, Pipe = backing, StatusFlags = 1 });
        }
        return 0;
    }

    private static long PipeRead(FileSlot slot, void* destination, ulong count)
    {
        if ((slot.StatusFlags & 3) != 0) return FcntlError(EBADF);
        if (count == 0) return 0;
        if (destination == null) return FcntlError(EFAULT);
        if (count > long.MaxValue) return FcntlError(EINVAL);
        var pipe = slot.Pipe!;
        lock (pipe.Sync)
        {
            while (pipe.Count == 0)
            {
                if (!pipe.ReaderOpen) return FcntlError(EBADF);
                if (!pipe.WriterOpen) return 0;
                if ((slot.StatusFlags & 0x800) != 0) return FcntlError(EAGAIN);
                Monitor.Wait(pipe.Sync);
            }
            if (!pipe.ReaderOpen) return FcntlError(EBADF);
            int length = (int)Math.Min(count, (ulong)pipe.Count);
            var output = new Span<byte>(destination, length);
            int first = Math.Min(length, pipe.Bytes.Length - pipe.Head);
            pipe.Bytes.AsSpan(pipe.Head, first).CopyTo(output);
            pipe.Bytes.AsSpan(0, length - first).CopyTo(output[first..]);
            pipe.Head = (pipe.Head + length) % pipe.Bytes.Length;
            pipe.Count -= length;
            Monitor.PulseAll(pipe.Sync);
            return length;
        }
    }

    private static long PipeWrite(FileSlot slot, void* source, ulong count)
    {
        if ((slot.StatusFlags & 3) != 1) return FcntlError(EBADF);
        if (count == 0) return 0;
        if (source == null) return FcntlError(EFAULT);
        if (count > long.MaxValue) return FcntlError(EINVAL);
        var pipe = slot.Pipe!;
        ulong written = 0;
        lock (pipe.Sync)
        {
            while (written < count)
            {
                if (!pipe.WriterOpen) return written != 0 ? (long)written : FcntlError(EBADF);
                if (!pipe.ReaderOpen) return written != 0 ? (long)written : FcntlError(EPIPE);
                int available = pipe.Bytes.Length - pipe.Count;
                bool atomic = count <= 4096;
                if (available == 0 || atomic && (ulong)available < count)
                {
                    if ((slot.StatusFlags & 0x800) != 0)
                        return written != 0 ? (long)written : FcntlError(EAGAIN);
                    Monitor.Wait(pipe.Sync);
                    continue;
                }
                int length = (int)Math.Min((ulong)available, count - written);
                int tail = (pipe.Head + pipe.Count) % pipe.Bytes.Length;
                int first = Math.Min(length, pipe.Bytes.Length - tail);
                var input = new ReadOnlySpan<byte>((byte*)source + written, length);
                input[..first].CopyTo(pipe.Bytes.AsSpan(tail, first));
                input[first..].CopyTo(pipe.Bytes.AsSpan(0, length - first));
                pipe.Count += length;
                written += (ulong)length;
                Monitor.PulseAll(pipe.Sync);
                if ((slot.StatusFlags & 0x800) != 0) return (long)written;
            }
        }
        return (long)written;
    }

    private static void ClosePipe(FileSlot slot)
    {
        var pipe = slot.Pipe!;
        lock (pipe.Sync)
        {
            if ((slot.StatusFlags & 3) == 0) pipe.ReaderOpen = false;
            else pipe.WriterOpen = false;
            Monitor.PulseAll(pipe.Sync);
        }
    }

    private static short PollPipe(FileSlot slot, short events)
    {
        var pipe = slot.Pipe!;
        lock (pipe.Sync)
        {
            if ((slot.StatusFlags & 3) == 0)
                return (short)((pipe.Count > 0 ? events & 1 : 0) | (!pipe.WriterOpen ? 16 : 0));
            // POLLOUT promises that a PIPE_BUF-sized write will not block.
            return (short)((pipe.Bytes.Length - pipe.Count >= 4096 ? events & 4 : 0) |
                (!pipe.ReaderOpen ? 8 : 0));
        }
    }
}
