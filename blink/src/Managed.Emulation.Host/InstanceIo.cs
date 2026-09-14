using System.Net;
using System.Net.Sockets;

namespace Managed.Emulation.Host;

/// <summary>One descriptor namespace for private files, TCP and bounded captured
/// standard streams. No guest instruction execution or C pointer marshalling.</summary>
public sealed partial class InstanceIo : IAsyncDisposable
{
    private enum Kind { Input, Output, Error, File, Socket }
    private sealed class Description(Kind kind, int handle = -1)
    {
        internal readonly Kind Kind = kind;
        internal readonly int Handle = handle;
        internal int References = 1;
    }
    private readonly object sync = new();
    private readonly Dictionary<int, Description> descriptors = new();
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
        bool exclusive = false, bool truncate = false, bool append = false, string cwd = "/")
    {
        lock (sync)
        {
            if (disposed) return Fail<int>(GuestError.BadDescriptor);
            int fd = Allocate();
            if (fd < 0) return Fail<int>(GuestError.TooManyFiles);
            var result = files.Open(path, access, create, exclusive, truncate, append, cwd);
            if (!result.Succeeded) return result;
            descriptors.Add(fd, new(Kind.File, result.Value));
            return HostResult<int>.Success(fd);
        }
    }
    public HostResult<VirtualFileStat> Stat(string path, string cwd = "/")
    {
        lock (sync) return disposed ? Fail<VirtualFileStat>(GuestError.BadDescriptor) : files.Stat(path, cwd);
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
    public HostResult<int> Duplicate(int fd, int minimum = 0)
    {
        lock (sync)
        {
            if (!Find(fd, out var description)) return Fail<int>(GuestError.BadDescriptor);
            if (minimum < 0 || minimum >= descriptorLimit) return Fail<int>(GuestError.Invalid);
            int target = Allocate(minimum);
            if (target < 0) return Fail<int>(GuestError.TooManyFiles);
            ++description.References;
            descriptors.Add(target, description);
            return HostResult<int>.Success(target);
        }
    }
    public HostResult<int> Close(int fd)
    {
        lock (sync)
        {
            if (!descriptors.Remove(fd, out var description)) return Fail<int>(GuestError.BadDescriptor);
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
            if (owner) { disposed = true; descriptors.Clear(); input = []; inputPosition = 0; accepting = pending.ToArray(); }
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
