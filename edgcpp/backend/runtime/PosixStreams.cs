#nullable enable
using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Edg.Runtime;

public sealed unsafe partial class PosixContext
{
    string ILibcFileDescriptors.ResolvePath(string path) => ResolveManaged(path);
    private string ResolveManaged(string path)
    {
        if (path.Length == 0) throw new LibcDescriptorException(2);
        return OperatingSystem.IsWindows() ? ResolveWindowsPath(path, workingDirectory)
            : Path.IsPathRooted(path) ? path : Path.Combine(workingDirectory, path);
    }
    private static (int flags, bool read, bool write) StreamMode(string mode)
    {
        if (mode.Length == 0 || "rwa".IndexOf(mode[0]) < 0) throw new LibcDescriptorException(22);
        bool update = false, binary = false, exclusive = false;
        for (int i = 1; i < mode.Length; ++i)
        {
            if (mode[i] == '+' && !update) update = true;
            else if (mode[i] == 'b' && !binary) binary = true;
            else if (mode[i] == 'x' && !exclusive && mode[0] == 'w') exclusive = true;
            else throw new LibcDescriptorException(22);
        }
        bool read = mode[0] == 'r' || update, write = mode[0] != 'r' || update;
        return ((update ? 2 : read ? 0 : 1) | (mode[0] == 'w' ? 0x240 : mode[0] == 'a' ? 0x440 : 0)
            | (exclusive ? 0x80 : 0), read, write);
    }
    Stream ILibcFileDescriptors.Open(string path, string mode)
    {
        var access = StreamMode(mode);
        byte[] name = Utf8.GetBytes(path + '\0');
        int fd;
        fixed (byte* pointer = name) fd = Open((sbyte*)pointer, access.flags);
        if (fd < 0) throw new LibcDescriptorException(lastError);
        try { return new DescriptorStream(this, fd, access.read, access.write); }
        catch { Close(fd); throw; }
    }
    Stream ILibcFileDescriptors.Fdopen(int fd, string mode)
    {
        var access = StreamMode(mode);
        var descriptor = Find(fd);
        if (descriptor is null) throw new LibcDescriptorException(9);
        lock (descriptor.Gate)
        {
            if (descriptor.Closed || (access.read && (descriptor.Flags & 3) == 1) || (access.write && (descriptor.Flags & 3) == 0))
                throw new LibcDescriptorException(9);
            if (mode[0] == 'a' && FcntlFlags(fd, 4, descriptor.Flags | 0x400) != 0)
                throw new LibcDescriptorException(lastError);
            return new DescriptorStream(this, fd, access.read, access.write);
        }
    }
    int ILibcFileDescriptors.Fileno(Stream stream)
    {
        if (stream is not DescriptorStream descriptor || descriptor.Owner != this || Find(descriptor.Fd) is null)
            throw new LibcDescriptorException(9);
        return descriptor.Fd;
    }
    Stream ILibcFileDescriptors.Standard(Stream stream, int fd, bool terminal, bool native)
    {
        lock (descriptorsGate)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            if (descriptors.ContainsKey(fd)) throw new InvalidOperationException("Standard descriptor is already associated.");
            nint handle = native ? OperatingSystem.IsWindows() ? StandardHandle(-10 - fd) : fd : -1;
            var descriptor = new Descriptor(new SafeFileHandle(handle, false), "", fd == 0 ? 0 : 1, 3)
                { StandardStream = stream, Terminal = terminal, NativeStandard = native };
            descriptors.Add(fd, descriptor);
            return new DescriptorStream(this, fd, fd == 0, fd != 0);
        }
    }
    void ILibcFileDescriptors.SetAppend(Stream stream, bool append)
    {
        int fd = ((ILibcFileDescriptors)this).Fileno(stream);
        int flags = FcntlFlags(fd, 3);
        if (flags < 0 || FcntlFlags(fd, 4, append ? flags | 0x400 : flags & ~0x400) != 0)
            throw new LibcDescriptorException(lastError);
    }
    [LibraryImport("kernel32.dll", EntryPoint = "GetStdHandle", SetLastError = true)] private static partial nint StandardHandle(int id);

    private sealed class DescriptorStream : Stream
    {
        internal readonly PosixContext Owner;
        internal readonly int Fd;
        private readonly bool read, write;
        private bool closed;
        internal DescriptorStream(PosixContext owner, int fd, bool read, bool write)
        { Owner = owner; Fd = fd; this.read = read; this.write = write; }
        public override bool CanRead => !closed && read;
        public override bool CanWrite => !closed && write;
        public override bool CanSeek => !closed && Owner.Find(Fd)?.Device == 0;
        public override long Length
        {
            get { PosixStat stat; if (Owner.Fstat(Fd, &stat) != 0) throw new LibcDescriptorException(lastError); return stat.st_size; }
        }
        public override long Position { get => Seek(0, SeekOrigin.Current); set => Seek(value, SeekOrigin.Begin); }
        public override long Seek(long offset, SeekOrigin origin)
        {
            long result = Owner.Lseek(Fd, offset, (int)origin);
            if (result < 0) throw new LibcDescriptorException(lastError);
            return result;
        }
        public override void SetLength(long value)
        { if (Owner.Ftruncate(Fd, value) != 0) throw new LibcDescriptorException(lastError); }
        public override void Flush()
        {
            var descriptor = Owner.Find(Fd); if (descriptor is null) throw new LibcDescriptorException(9);
            // stdio fflush pushes user buffering, not fsync durability.
            lock (descriptor.Gate) { if (descriptor.Closed) throw new LibcDescriptorException(9); descriptor.StandardStream?.Flush(); }
        }
        public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));
        public override int Read(Span<byte> buffer)
        {
            if (!CanRead) throw new LibcDescriptorException(9);
            fixed (byte* pointer = buffer)
            { long result = Owner.Read(Fd, pointer, (ulong)buffer.Length); if (result < 0) throw new LibcDescriptorException(lastError); return (int)result; }
        }
        public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            if (!CanWrite) throw new LibcDescriptorException(9);
            fixed (byte* pointer = buffer)
            {
                int position = 0;
                while (position < buffer.Length)
                { long result = Owner.Write(Fd, pointer + position, (ulong)(buffer.Length - position)); if (result <= 0) throw new LibcDescriptorException(result < 0 ? lastError : 5); position += (int)result; }
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (!closed) { closed = true; if (disposing && Owner.Close(Fd) != 0) throw new LibcDescriptorException(lastError); }
            base.Dispose(disposing);
        }
    }
}
