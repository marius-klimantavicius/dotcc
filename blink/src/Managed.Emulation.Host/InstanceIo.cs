using System.Net;
using System.Net.Sockets;

namespace Managed.Emulation.Host;

/// <summary>One descriptor namespace for private files, TCP and bounded captured
/// standard streams. No guest instruction execution or C pointer marshalling.</summary>
public sealed partial class InstanceIo : IAsyncDisposable
{
    private enum Kind { Input, Output, Error, File, Socket }
    private sealed class Description(Kind kind, int handle = -1, int statusFlags = 0)
    {
        internal readonly Kind Kind = kind;
        internal readonly int Handle = handle;
        internal int References = 1;
        internal int StatusFlags = kind is Kind.Output or Kind.Error ? 1 : kind == Kind.Socket ? 2 : statusFlags;
    }
    private readonly object sync = new();
    private readonly Dictionary<int, Description> descriptors = new();
    private readonly HashSet<int> closeOnExec = new();
    private readonly VirtualFileSystem files;
    private readonly VirtualTcpNetwork network;
    private byte[] input;
    private readonly MemoryStream output = new(), error = new();
    private readonly int descriptorLimit, outputLimit;
    private readonly HashSet<Task> pending = new();
    private int inputPosition, outputBytes;
    private bool disposed;
    private TaskCompletionSource? shutdown;

    public InstanceIo(IReadOnlyDictionary<string, ReadOnlyMemory<byte>> image,
        ReadOnlyMemory<byte> standardInput = default, int descriptorLimit = 128,
        int outputLimit = 65536, long writableLimit = 1 << 20, long imageLimit = 16 << 20, int inputLimit = 1 << 20,
        IReadOnlySet<string>? executablePaths = null)
    {
        if (descriptorLimit < 3) throw new ArgumentOutOfRangeException(nameof(descriptorLimit));
        if (outputLimit < 0) throw new ArgumentOutOfRangeException(nameof(outputLimit));
        if (inputLimit < 0 || standardInput.Length > inputLimit) throw new ArgumentOutOfRangeException(nameof(inputLimit));
        this.descriptorLimit = descriptorLimit; this.outputLimit = outputLimit;
        files = new(image, writableLimit, descriptorLimit, imageLimit, executablePaths);
        network = new(descriptorLimit);
        input = standardInput.ToArray();
        descriptors.Add(0, new(Kind.Input));
        descriptors.Add(1, new(Kind.Output));
        descriptors.Add(2, new(Kind.Error));
    }
    public int OpenDescriptors { get { lock (sync) return descriptors.Count; } }
    public (byte[] StandardOutput, byte[] StandardError) CapturedOutput
    {
        get { lock (sync) return (output.ToArray(), error.ToArray()); }
    }
    public HostResult<int> OpenFile(string path, FileAccessMode access, bool create = false,
        bool exclusive = false, bool truncate = false, bool append = false, string? cwd = null,
        bool closeOnExecFlag = false, bool allowDirectory = false, bool requireDirectory = false, bool noFollow = false,
        uint creationMode = 0x180)
    {
        lock (sync)
        {
            if (disposed) return Fail<int>(GuestError.BadDescriptor);
            if (create && (creationMode & ~0x1ffu) != 0) return Fail<int>(GuestError.Unsupported);
            int fd = Allocate();
            if (fd < 0) return Fail<int>(GuestError.TooManyFiles);
            var result = files.Open(path, access, create, exclusive, truncate, append, cwd ?? currentDirectory, allowDirectory, requireDirectory, ApplyCreationMask(creationMode));
            if (!result.Succeeded) return result;
            int mode = access == FileAccessMode.Read ? 0 : access == FileAccessMode.Write ? 1 : 2;
            descriptors.Add(fd, new(Kind.File, result.Value, mode | (append ? 1024 : 0) | (requireDirectory ? 65536 : 0) | (noFollow ? 131072 : 0)));
            if (closeOnExecFlag) closeOnExec.Add(fd);
            return HostResult<int>.Success(fd);
        }
    }
    public HostResult<int> OpenFileAt(int directoryFd, string path, FileAccessMode access, bool create = false,
        bool exclusive = false, bool truncate = false, bool append = false, bool closeOnExecFlag = false,
        bool requireDirectory = false, bool noFollow = false, uint creationMode = 0x180)
    {
        lock (sync)
        {
            var directory = ResolveDirectory(directoryFd, path);
            return directory.Succeeded ? OpenFile(path, access, create, exclusive, truncate, append, directory.Value,
                closeOnExecFlag, true, requireDirectory, noFollow, creationMode) : Fail<int>(directory.Error);
        }
    }
    public HostResult<VirtualFileStat> Stat(string path, string? cwd = null)
    {
        lock (sync) return disposed ? Fail<VirtualFileStat>(GuestError.BadDescriptor) : files.Stat(path, cwd ?? currentDirectory);
    }
    public HostResult<VirtualFileStat> StatAt(int directoryFd, string path)
    {
        lock (sync)
        {
            var directory = ResolveDirectory(directoryFd, path);
            return directory.Succeeded ? files.Stat(path, directory.Value) : Fail<VirtualFileStat>(directory.Error);
        }
    }
    public HostResult<long> Seek(int fd, long offset, SeekOrigin origin)
    {
        lock (sync)
        {
            if (!Find(fd, out var description)) return Fail<long>(GuestError.BadDescriptor);
            return description.Kind == Kind.File ? files.Seek(description.Handle, offset, origin) : Fail<long>(GuestError.IllegalSeek);
        }
    }
    public HostResult<VirtualFileStat> FStat(int fd)
    {
        lock (sync)
        {
            if (!Find(fd, out var description)) return Fail<VirtualFileStat>(GuestError.BadDescriptor);
            return description.Kind == Kind.File ? files.FStat(description.Handle) : Fail<VirtualFileStat>(GuestError.Unsupported);
        }
    }
    public HostResult<int> Socket()
    {
        lock (sync)
        {
            if (disposed) return Fail<int>(GuestError.BadDescriptor);
            int fd = Allocate();
            if (fd < 0) return Fail<int>(GuestError.TooManyFiles);
            var result = network.Create();
            if (!result.Succeeded) return result;
            descriptors.Add(fd, new(Kind.Socket, result.Value));
            return HostResult<int>.Success(fd);
        }
    }
    public HostResult<int> Duplicate(int fd, int minimum = 0, bool closeOnExecFlag = false)
    {
        lock (sync)
        {
            if (!Find(fd, out var description)) return Fail<int>(GuestError.BadDescriptor);
            if (minimum < 0 || minimum >= descriptorLimit) return Fail<int>(GuestError.Invalid);
            int target = Allocate(minimum);
            if (target < 0) return Fail<int>(GuestError.TooManyFiles);
            ++description.References;
            descriptors.Add(target, description);
            if (closeOnExecFlag) closeOnExec.Add(target);
            return HostResult<int>.Success(target);
        }
    }
    public HostResult<int> GetDescriptorFlags(int fd)
    {
        lock (sync) return Find(fd, out _) ? HostResult<int>.Success(closeOnExec.Contains(fd) ? 1 : 0) : Fail<int>(GuestError.BadDescriptor);
    }
    public HostResult<int> SetDescriptorFlags(int fd, int flags)
    {
        lock (sync)
        {
            if (!Find(fd, out _)) return Fail<int>(GuestError.BadDescriptor);
            if ((flags & ~1) != 0) return Fail<int>(GuestError.Invalid);
            if (flags == 1) closeOnExec.Add(fd); else closeOnExec.Remove(fd);
            return HostResult<int>.Success(0);
        }
    }
    public HostResult<int> GetStatusFlags(int fd)
    {
        lock (sync) return Find(fd, out var description) ? HostResult<int>.Success(description.StatusFlags) : Fail<int>(GuestError.BadDescriptor);
    }
    public HostResult<int> SetStatusFlags(int fd, int flags)
    {
        lock (sync)
        {
            if (!Find(fd, out var description)) return Fail<int>(GuestError.BadDescriptor);
            // Access and retained lookup flags are not mutable with F_SETFL.
            // Nonblocking, async, direct and synchronous modes need real host contracts.
            if ((flags & ~(3 | 1024 | 65536 | 131072)) != 0) return Fail<int>(GuestError.Unsupported);
            bool append = (flags & 1024) != 0;
            if (description.Kind == Kind.File)
            {
                var result = files.SetAppend(description.Handle, append);
                if (!result.Succeeded) return result;
            }
            else if (append) return Fail<int>(GuestError.Unsupported);
            description.StatusFlags = (description.StatusFlags & ~1024) | (append ? 1024 : 0);
            return HostResult<int>.Success(0);
        }
    }
    /// <summary>Owner transition only; call after a successful exec decision.
    /// This API does not implement guest exec or change the process image.</summary>
    public HostResult<int> CloseOnExecDescriptors()
    {
        lock (sync)
        {
            if (disposed) return Fail<int>(GuestError.BadDescriptor);
            int count = 0;
            foreach (int fd in closeOnExec.ToArray())
            {
                var result = Close(fd);
                if (!result.Succeeded) return result;
                ++count;
            }
            return HostResult<int>.Success(count);
        }
    }
    public HostResult<int> Close(int fd)
    {
        lock (sync)
        {
            if (!descriptors.Remove(fd, out var description)) return Fail<int>(GuestError.BadDescriptor);
            closeOnExec.Remove(fd);
            if (--description.References != 0) return HostResult<int>.Success(0);
            return description.Kind switch
            {
                Kind.File => files.Close(description.Handle),
                Kind.Socket => network.Close(description.Handle),
                _ => HostResult<int>.Success(0)
            };
        }
    }
    public Task<HostResult<int>> ReadAsync(int fd, Memory<byte> destination, CancellationToken cancellation = default)
    {
        lock (sync)
        {
            if (cancellation.IsCancellationRequested) return Task.FromResult(Fail<int>(GuestError.Canceled));
            if (!Find(fd, out var description)) return Task.FromResult(Fail<int>(GuestError.BadDescriptor));
            if (description.Kind == Kind.Socket) return network.ReceiveAsync(description.Handle, destination, cancellation);
            HostResult<int> result;
            if (description.Kind == Kind.File) result = files.Read(description.Handle, destination.Span);
            else if (description.Kind == Kind.Input)
            {
                int count = Math.Min(destination.Length, input.Length - inputPosition);
                input.AsSpan(inputPosition, count).CopyTo(destination.Span); inputPosition += count;
                result = HostResult<int>.Success(count);
            }
            else result = Fail<int>(GuestError.BadDescriptor);
            return Task.FromResult(result);
        }
    }
    public HostResult<int> ReadAt(int fd, Span<byte> destination, long offset)
    {
        lock (sync)
        {
            if (!Find(fd, out var description)) return Fail<int>(GuestError.BadDescriptor);
            return description.Kind == Kind.File ? files.ReadAt(description.Handle, destination, offset) : Fail<int>(GuestError.IllegalSeek);
        }
    }
    public HostResult<long> ReadAtLength(int fd)
    {
        lock (sync)
        {
            if (!Find(fd, out var description)) return Fail<long>(GuestError.BadDescriptor);
            return description.Kind == Kind.File ? files.ReadAtLength(description.Handle) : Fail<long>(GuestError.Unsupported);
        }
    }
    public Task<HostResult<int>> WriteAsync(int fd, ReadOnlyMemory<byte> source, CancellationToken cancellation = default)
    {
        lock (sync)
        {
            if (cancellation.IsCancellationRequested) return Task.FromResult(Fail<int>(GuestError.Canceled));
            if (!Find(fd, out var description)) return Task.FromResult(Fail<int>(GuestError.BadDescriptor));
            if (description.Kind == Kind.Socket) return network.SendAsync(description.Handle, source, cancellation);
            HostResult<int> result;
            if (description.Kind == Kind.File) result = files.Write(description.Handle, source.Span);
            else if (description.Kind is Kind.Output or Kind.Error)
            {
                int count = Math.Min(source.Length, outputLimit - outputBytes);
                if (count == 0 && source.Length != 0) result = Fail<int>(GuestError.NoSpace);
                else
                {
                    (description.Kind == Kind.Output ? output : error).Write(source.Span[..count]);
                    outputBytes += count; result = HostResult<int>.Success(count);
                }
            }
            else result = Fail<int>(GuestError.BadDescriptor);
            return Task.FromResult(result);
        }
    }
    public Task<HostResult<int>> ReceiveAsync(int fd, Memory<byte> destination, CancellationToken cancellation = default)
    {
        lock (sync)
        {
            var found = SocketHandle(fd);
            return found.Succeeded ? network.ReceiveAsync(found.Value, destination, cancellation) : Task.FromResult(Fail<int>(found.Error));
        }
    }
    public Task<HostResult<int>> SendAsync(int fd, ReadOnlyMemory<byte> source, CancellationToken cancellation = default)
    {
        lock (sync)
        {
            var found = SocketHandle(fd);
            return found.Succeeded ? network.SendAsync(found.Value, source, cancellation) : Task.FromResult(Fail<int>(found.Error));
        }
    }
    public HostResult<GuestEndpoint> Bind(int fd, GuestEndpoint endpoint) => SocketCall(fd, handle => network.Bind(handle, endpoint));
    public HostResult<int> Listen(int fd, int backlog) => SocketCall(fd, handle => network.Listen(handle, backlog));
    public HostResult<IPEndPoint> Publish(int fd) => SocketCall(fd, network.Publish);
    public HostResult<GuestEndpoint> LocalEndpoint(int fd) => SocketCall(fd, network.LocalEndpoint);
    public HostResult<int> SetOption(int fd, TcpHostOption option, int value) => SocketCall(fd, handle => network.SetOption(handle, option, value));
    public HostResult<int> Shutdown(int fd, SocketShutdown direction) => SocketCall(fd, handle => network.Shutdown(handle, direction));
    public Task<HostResult<int>> ConnectAsync(int fd, GuestEndpoint endpoint, CancellationToken cancellation = default)
    {
        lock (sync)
        {
            var found = SocketHandle(fd);
            return found.Succeeded ? network.ConnectAsync(found.Value, endpoint, cancellation) : Task.FromResult(Fail<int>(found.Error));
        }
    }
    public Task<HostResult<bool>> WaitReadableAsync(int fd, TimeSpan timeout, CancellationToken cancellation = default)
    {
        lock (sync)
        {
            if (!Find(fd, out var description)) return Task.FromResult(Fail<bool>(GuestError.BadDescriptor));
            if (cancellation.IsCancellationRequested) return Task.FromResult(Fail<bool>(GuestError.Canceled));
            if (timeout < TimeSpan.Zero && timeout != Timeout.InfiniteTimeSpan) return Task.FromResult(Fail<bool>(GuestError.Invalid));
            return description.Kind == Kind.Socket ? network.WaitReadableAsync(description.Handle, timeout, cancellation)
                : Task.FromResult(HostResult<bool>.Success(true));
        }
    }
    public async Task<HostResult<AcceptedSocket>> AcceptAsync(int fd, CancellationToken cancellation = default)
    {
        Task<HostResult<AcceptedSocket>> accepted;
        TaskCompletionSource completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        lock (sync)
        {
            var found = SocketHandle(fd);
            if (!found.Succeeded) return Fail<AcceptedSocket>(found.Error);
            if (Allocate() < 0) return Fail<AcceptedSocket>(GuestError.TooManyFiles);
            pending.Add(completion.Task);
            accepted = network.AcceptAsync(found.Value, cancellation);
        }
        try
        {
            var result = await accepted.ConfigureAwait(false);
            if (!result.Succeeded) return result;
            lock (sync)
            {
                int target = disposed ? -1 : Allocate();
                if (target < 0)
                {
                    network.Close(result.Value.Handle);
                    return Fail<AcceptedSocket>(disposed ? GuestError.Canceled : GuestError.TooManyFiles);
                }
                descriptors.Add(target, new(Kind.Socket, result.Value.Handle));
                return HostResult<AcceptedSocket>.Success(new(target, result.Value.Remote));
            }
        }
        finally
        {
            lock (sync) pending.Remove(completion.Task);
            completion.SetResult();
        }
    }
    public async ValueTask DisposeAsync()
    {
        TaskCompletionSource completion;
        Task[] accepting = [];
        bool owner;
        lock (sync)
        {
            owner = shutdown == null;
            shutdown ??= new(TaskCreationOptions.RunContinuationsAsynchronously); completion = shutdown;
            if (owner) { disposed = true; descriptors.Clear(); closeOnExec.Clear(); input = []; inputPosition = 0; accepting = pending.ToArray(); }
        }
        if (owner)
        {
            try
            {
                await network.DisposeAsync().ConfigureAwait(false);
                await Task.WhenAll(accepting).ConfigureAwait(false);
                files.Dispose();
                completion.SetResult();
            }
            catch (Exception failure) { completion.SetException(failure); }
            finally { files.Dispose(); }
        }
        await completion.Task.ConfigureAwait(false);
    }
    private HostResult<T> SocketCall<T>(int fd, Func<int, HostResult<T>> operation)
    {
        lock (sync)
        {
            var found = SocketHandle(fd);
            return found.Succeeded ? operation(found.Value) : Fail<T>(found.Error);
        }
    }
    private HostResult<int> SocketHandle(int fd) => !Find(fd, out var description) ? Fail<int>(GuestError.BadDescriptor)
        : description.Kind != Kind.Socket ? Fail<int>(GuestError.NotSocket) : HostResult<int>.Success(description.Handle);
    private HostResult<string> ResolveDirectory(int fd, string path)
    {
        if (disposed) return Fail<string>(GuestError.BadDescriptor);
        if (string.IsNullOrEmpty(path)) return Fail<string>(GuestError.NoEntry);
        if (path.StartsWith('/')) return HostResult<string>.Success("/");
        if (fd == -100) return HostResult<string>.Success(currentDirectory);
        if (!Find(fd, out var description)) return Fail<string>(GuestError.BadDescriptor);
        return description.Kind == Kind.File ? files.DirectoryPath(description.Handle) : Fail<string>(GuestError.NotDirectory);
    }
    private bool Find(int fd, out Description description)
    {
        description = null!;
        return !disposed && descriptors.TryGetValue(fd, out description!);
    }
    private int Allocate(int minimum = 0)
    {
        for (int fd = minimum; fd < descriptorLimit; ++fd) if (!descriptors.ContainsKey(fd)) return fd;
        return -1;
    }
    private static HostResult<T> Fail<T>(GuestError error) => HostResult<T>.Failure(error);
}
