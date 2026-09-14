using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Managed.Emulation.Host;

public readonly record struct GuestEndpoint(uint Address, ushort Port)
{
    public const uint Loopback = 0x7f000001;
}
public enum TcpHostOption { ReuseAddress, NoDelay, SendBufferSize, ReceiveBufferSize }
public readonly record struct AcceptedSocket(int Handle, GuestEndpoint Remote);

/// <summary>Per-instance IPv4/TCP bindings with explicit loopback publication.
/// Guest addresses never become implicit external host destinations.</summary>
public sealed class VirtualTcpNetwork(int descriptorLimit = 128) : IAsyncDisposable
{
    private sealed class Entry(Socket socket)
    {
        internal readonly Socket Socket = socket;
        internal GuestEndpoint? Local;
        internal GuestEndpoint? Remote;
        internal bool Listening;
    }
    private readonly object sync = new();
    private readonly Dictionary<int, Entry> entries = new();
    private readonly Dictionary<ushort, Entry> bindings = new();
    private readonly Dictionary<int, GuestEndpoint> origins = new();
    private readonly HashSet<Task> pending = new();
    private readonly CancellationTokenSource stop = new();
    private readonly int descriptorLimit = descriptorLimit > 0 ? descriptorLimit : throw new ArgumentOutOfRangeException(nameof(descriptorLimit));
    private TaskCompletionSource? shutdown;
    private ushort ephemeral = 49152;
    private volatile bool disposed;

    public int PendingOperations { get { lock (sync) return pending.Count; } }
    public int OpenDescriptors { get { lock (sync) return entries.Count; } }
    public HostResult<int> Create()
    {
        lock (sync)
        {
            if (disposed) return Fail<int>(GuestError.BadDescriptor);
            if (entries.Count >= descriptorLimit) return Fail<int>(GuestError.TooManyFiles);
            try { return HostResult<int>.Success(Add(new Entry(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)))); }
            catch (SocketException error) { return Fail<int>(ConvertError(error)); }
        }
    }

    public HostResult<GuestEndpoint> Bind(int handle, GuestEndpoint endpoint)
    {
        lock (sync)
        {
            if (!Find(handle, out var entry)) return Fail<GuestEndpoint>(GuestError.BadDescriptor);
            if (entry.Local != null) return Fail<GuestEndpoint>(GuestError.Invalid);
            if (endpoint.Address is not (0 or GuestEndpoint.Loopback)) return Fail<GuestEndpoint>(GuestError.AddressUnavailable);
            ushort port = endpoint.Port;
            if (port == 0) port = AllocatePort();
            if (port == 0 || bindings.ContainsKey(port)) return Fail<GuestEndpoint>(GuestError.AddressInUse);
            try
            {
                entry.Socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
                entry.Local = endpoint with { Port = port };
                bindings.Add(port, entry);
                origins.Add(((IPEndPoint)entry.Socket.LocalEndPoint!).Port, entry.Local.Value with { Address = GuestEndpoint.Loopback });
                return HostResult<GuestEndpoint>.Success(entry.Local.Value);
            }
            catch (SocketException error) { return Fail<GuestEndpoint>(ConvertError(error)); }
        }
    }

    public HostResult<int> Listen(int handle, int backlog)
    {
        lock (sync)
        {
            if (!Find(handle, out var entry)) return Fail<int>(GuestError.BadDescriptor);
            if (entry.Local == null || backlog < 0) return Fail<int>(GuestError.Invalid);
            try { entry.Socket.Listen(backlog); entry.Listening = true; return HostResult<int>.Success(0); }
            catch (SocketException error) { return Fail<int>(ConvertError(error)); }
        }
    }

    public HostResult<int> SetOption(int handle, TcpHostOption option, int value)
    {
        lock (sync)
        {
            if (!Find(handle, out var entry)) return Fail<int>(GuestError.BadDescriptor);
            try
            {
                switch (option)
                {
                    case TcpHostOption.ReuseAddress: entry.Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, value != 0); break;
                    case TcpHostOption.NoDelay: entry.Socket.NoDelay = value != 0; break;
                    case TcpHostOption.SendBufferSize when value is > 0 and <= 16 * 1024 * 1024: entry.Socket.SendBufferSize = value; break;
                    case TcpHostOption.ReceiveBufferSize when value is > 0 and <= 16 * 1024 * 1024: entry.Socket.ReceiveBufferSize = value; break;
                    default: return Fail<int>(GuestError.Invalid);
                }
                return HostResult<int>.Success(0);
            }
            catch (SocketException error) { return Fail<int>(ConvertError(error)); }
        }
    }

    public HostResult<IPEndPoint> Publish(int handle)
    {
        lock (sync)
        {
            if (!Find(handle, out var entry)) return Fail<IPEndPoint>(GuestError.BadDescriptor);
            if (!entry.Listening) return Fail<IPEndPoint>(GuestError.Invalid);
            var endpoint = (IPEndPoint)entry.Socket.LocalEndPoint!;
            return HostResult<IPEndPoint>.Success(new(IPAddress.Loopback, endpoint.Port));
        }
    }
    public HostResult<GuestEndpoint> LocalEndpoint(int handle)
    {
        lock (sync) return Find(handle, out var entry)
            ? HostResult<GuestEndpoint>.Success(entry.Local ?? new(0, 0)) : Fail<GuestEndpoint>(GuestError.BadDescriptor);
    }

    public Task<HostResult<int>> ConnectAsync(int handle, GuestEndpoint destination, CancellationToken cancellation = default)
        => RunAsync<int>(async token =>
        {
            Entry source;
            IPEndPoint actual;
            lock (sync)
            {
                if (!Find(handle, out source!)) return Fail<int>(GuestError.BadDescriptor);
                if (source.Remote != null) return Fail<int>(GuestError.AlreadyConnected);
                if (source.Listening) return Fail<int>(GuestError.Invalid);
                if (destination.Address != GuestEndpoint.Loopback) return Fail<int>(GuestError.Access);
                if (!bindings.TryGetValue(destination.Port, out var target) || !target.Listening) return Fail<int>(GuestError.ConnectionRefused);
                if (source.Local == null)
                {
                    var bound = Bind(handle, new(GuestEndpoint.Loopback, 0));
                    if (!bound.Succeeded) return Fail<int>(bound.Error);
                }
                actual = (IPEndPoint)target.Socket.LocalEndPoint!;
            }
            await source.Socket.ConnectAsync(actual, token).ConfigureAwait(false);
            lock (sync) source.Remote = destination;
            return HostResult<int>.Success(0);
        }, cancellation);

    public Task<HostResult<AcceptedSocket>> AcceptAsync(int handle, CancellationToken cancellation = default)
        => RunAsync<AcceptedSocket>(async token =>
        {
            Entry listener;
            lock (sync)
            {
                if (!Find(handle, out listener!)) return Fail<AcceptedSocket>(GuestError.BadDescriptor);
                if (!listener.Listening) return Fail<AcceptedSocket>(GuestError.Invalid);
                if (entries.Count >= descriptorLimit) return Fail<AcceptedSocket>(GuestError.TooManyFiles);
            }
            Socket accepted = await listener.Socket.AcceptAsync(token).ConfigureAwait(false);
            lock (sync)
            {
                if (disposed || entries.Count >= descriptorLimit)
                {
                    accepted.Dispose();
                    return Fail<AcceptedSocket>(disposed ? GuestError.Canceled : GuestError.TooManyFiles);
                }
                int peerPort = ((IPEndPoint)accepted.RemoteEndPoint!).Port;
                GuestEndpoint remote = origins.TryGetValue(peerPort, out var known) ? known : new(GuestEndpoint.Loopback, AllocatePort());
                var entry = new Entry(accepted) { Local = listener.Local!.Value with { Address = GuestEndpoint.Loopback }, Remote = remote };
                return HostResult<AcceptedSocket>.Success(new(Add(entry), remote));
            }
        }, cancellation);

    public Task<HostResult<int>> ReceiveAsync(int handle, Memory<byte> destination, CancellationToken cancellation = default)
        => RunAsync<int>(async token =>
        {
            Socket socket;
            lock (sync) { if (!Find(handle, out var entry)) return Fail<int>(GuestError.BadDescriptor); socket = entry.Socket; }
            return HostResult<int>.Success(await socket.ReceiveAsync(destination, SocketFlags.None, token).ConfigureAwait(false));
        }, cancellation);

    public Task<HostResult<int>> SendAsync(int handle, ReadOnlyMemory<byte> source, CancellationToken cancellation = default)
        => RunAsync<int>(async token =>
        {
            Socket socket;
            lock (sync) { if (!Find(handle, out var entry)) return Fail<int>(GuestError.BadDescriptor); socket = entry.Socket; }
            return HostResult<int>.Success(await socket.SendAsync(source, SocketFlags.None, token).ConfigureAwait(false));
        }, cancellation);

    public Task<HostResult<bool>> WaitReadableAsync(int handle, TimeSpan timeout, CancellationToken cancellation = default)
        => RunAsync<bool>(async token =>
        {
            if (timeout < TimeSpan.Zero && timeout != Timeout.InfiniteTimeSpan) return Fail<bool>(GuestError.Invalid);
            Socket socket;
            lock (sync) { if (!Find(handle, out var entry)) return Fail<bool>(GuestError.BadDescriptor); socket = entry.Socket; }
            long start = Stopwatch.GetTimestamp();
            for (;;)
            {
                token.ThrowIfCancellationRequested();
                if (socket.Poll(0, SelectMode.SelectRead)) return HostResult<bool>.Success(true);
                TimeSpan elapsed = Stopwatch.GetElapsedTime(start);
                if (timeout != Timeout.InfiniteTimeSpan && elapsed >= timeout) return HostResult<bool>.Success(false);
                TimeSpan pause = timeout == Timeout.InfiniteTimeSpan ? TimeSpan.FromMilliseconds(5) : TimeSpan.FromMilliseconds(Math.Min(5, (timeout - elapsed).TotalMilliseconds));
                await Task.Delay(pause, token).ConfigureAwait(false);
            }
        }, cancellation);

    public HostResult<int> Shutdown(int handle, SocketShutdown direction)
    {
        lock (sync)
        {
            if (!Find(handle, out var entry)) return Fail<int>(GuestError.BadDescriptor);
            if (direction is not (SocketShutdown.Receive or SocketShutdown.Send or SocketShutdown.Both)) return Fail<int>(GuestError.Invalid);
            try { entry.Socket.Shutdown(direction); return HostResult<int>.Success(0); }
            catch (SocketException error) { return Fail<int>(ConvertError(error)); }
        }
    }

    public HostResult<int> Close(int handle)
    {
        lock (sync)
        {
            if (!entries.Remove(handle, out var entry)) return Fail<int>(GuestError.BadDescriptor);
            if (entry.Local is { } local && bindings.TryGetValue(local.Port, out var owner) && ReferenceEquals(owner, entry))
            {
                bindings.Remove(local.Port);
                if (entry.Socket.LocalEndPoint is IPEndPoint physical) origins.Remove(physical.Port);
            }
            entry.Socket.Dispose();
            return HostResult<int>.Success(0);
        }
    }

    public async ValueTask DisposeAsync()
    {
        Socket[] sockets = [];
        Task[] operations = [];
        TaskCompletionSource completion;
        bool owner;
        lock (sync)
        {
            owner = shutdown == null;
            shutdown ??= new(TaskCreationOptions.RunContinuationsAsynchronously);
            completion = shutdown;
            if (owner)
            {
                disposed = true;
                sockets = entries.Values.Select(entry => entry.Socket).ToArray();
                operations = pending.ToArray();
                entries.Clear(); bindings.Clear(); origins.Clear();
            }
        }
        if (owner)
        {
            try
            {
                stop.Cancel();
                foreach (var socket in sockets) socket.Dispose();
                await Task.WhenAll(operations).ConfigureAwait(false);
                stop.Dispose();
                completion.SetResult();
            }
            catch (Exception error) { completion.SetException(error); }
        }
        await completion.Task.ConfigureAwait(false);
    }

    private async Task<HostResult<T>> RunAsync<T>(Func<CancellationToken, Task<HostResult<T>>> action, CancellationToken cancellation)
    {
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        lock (sync)
        {
            if (disposed) return Fail<T>(GuestError.BadDescriptor);
            pending.Add(completion.Task);
        }
        try
        {
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(stop.Token, cancellation);
            return await action(linked.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) { return Fail<T>(GuestError.Canceled); }
        catch (ObjectDisposedException) { return Fail<T>(disposed ? GuestError.Canceled : GuestError.BadDescriptor); }
        catch (SocketException error) { return Fail<T>(disposed ? GuestError.Canceled : ConvertError(error)); }
        finally
        {
            lock (sync) pending.Remove(completion.Task);
            completion.SetResult();
        }
    }
    private bool Find(int handle, out Entry entry)
    {
        entry = null!;
        return !disposed && entries.TryGetValue(handle, out entry!);
    }
    private int Add(Entry entry)
    {
        int handle = 3;
        while (entries.ContainsKey(handle)) ++handle;
        entries.Add(handle, entry);
        return handle;
    }
    private ushort AllocatePort()
    {
        for (int count = 0; count < 16384; ++count)
        {
            ushort candidate = ephemeral;
            ephemeral = ephemeral == ushort.MaxValue ? (ushort)49152 : (ushort)(ephemeral + 1);
            if (!bindings.ContainsKey(candidate)) return candidate;
        }
        return 0;
    }
    private static GuestError ConvertError(SocketException error) => error.SocketErrorCode switch
    {
        SocketError.AddressAlreadyInUse => GuestError.AddressInUse,
        SocketError.AddressNotAvailable => GuestError.AddressUnavailable,
        SocketError.ConnectionRefused => GuestError.ConnectionRefused,
        SocketError.ConnectionReset or SocketError.ConnectionAborted => GuestError.ConnectionReset,
        SocketError.NotConnected => GuestError.NotConnected,
        SocketError.IsConnected => GuestError.AlreadyConnected,
        SocketError.Shutdown => GuestError.BrokenPipe,
        SocketError.WouldBlock => GuestError.Again,
        SocketError.TimedOut => GuestError.TimedOut,
        SocketError.OperationAborted or SocketError.Interrupted => GuestError.Canceled,
        SocketError.TooManyOpenSockets => GuestError.TooManyFiles,
        SocketError.AccessDenied => GuestError.Access,
        SocketError.InvalidArgument => GuestError.Invalid,
        SocketError.OperationNotSupported => GuestError.Unsupported,
        _ => GuestError.Io
    };
    private static HostResult<T> Fail<T>(GuestError error) => HostResult<T>.Failure(error);
}
