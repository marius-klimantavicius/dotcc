using System.Net;
using System.Net.Sockets;

namespace Managed.Emulation.Host;

public sealed partial class VirtualTcpNetwork
{
    /// <summary>The socket owns connection establishment. Caller interruption or
    /// a blocking send deadline ends only the wait; poll and SO_ERROR can still
    /// observe completion. Last close or machine disposal terminates the work.</summary>
    public Task<HostResult<int>> ConnectAsync(int handle, GuestEndpoint destination, CancellationToken cancellation = default)
    {
        Task<HostResult<int>> connection;
        bool nonBlocking;
        int timeout;
        lock (sync)
        {
            if (cancellation.IsCancellationRequested) return Task.FromResult(Fail<int>(GuestError.Canceled));
            if (!Find(handle, out var source)) return Task.FromResult(Fail<int>(GuestError.BadDescriptor));
            if (source.Connection == ConnectionState.Connecting) return Task.FromResult(Fail<int>(GuestError.AlreadyInProgress));
            if (source.Connection == ConnectionState.Connected) return Task.FromResult(Fail<int>(GuestError.AlreadyConnected));
            if (source.Listening) return Task.FromResult(Fail<int>(GuestError.Invalid));
            // Linux does not promise that a socket can be reused after a failed
            // connect. Retain failure state independently of SO_ERROR reads.
            if (source.Connection == ConnectionState.Failed) return Task.FromResult(Fail<int>(GuestError.NotConnected));
            IPEndPoint actual;
            try
            {
                if (policy is null)
                {
                    if (destination.Address != GuestEndpoint.Loopback) return Task.FromResult(Fail<int>(GuestError.Access));
                    if (!bindings.TryGetValue(destination.Port, out var target) || !target.Listening)
                        return Task.FromResult(Fail<int>(GuestError.ConnectionRefused));
                    if (source.Local == null)
                    {
                        var bound = Bind(handle, new(GuestEndpoint.Loopback, 0));
                        if (!bound.Succeeded) return Task.FromResult(Fail<int>(bound.Error));
                    }
                    actual = (IPEndPoint)target.Socket.LocalEndPoint!;
                }
                else
                {
                    if (!policy.Destinations.Contains(destination)) return Task.FromResult(Fail<int>(GuestError.Access));
                    actual = new IPEndPoint(new IPAddress(new byte[] { (byte)(destination.Address >> 24),
                        (byte)(destination.Address >> 16), (byte)(destination.Address >> 8), (byte)destination.Address }), destination.Port);
                    if (source.Local == null)
                    {
                        ushort port = AllocatePort();
                        if (port == 0) return Task.FromResult(Fail<int>(GuestError.AddressInUse));
                        source.Socket.Bind(new IPEndPoint(IPAddress.Any, 0));
                        source.Local = new GuestEndpoint(GuestEndpoint.Loopback, port);
                    }
                }
            }
            catch (SocketException error) { return Task.FromResult(Fail<int>(ConvertError(error))); }

            nonBlocking = source.NonBlocking;
            timeout = source.SendTimeoutMilliseconds;
            var completion = new TaskCompletionSource<HostResult<int>>(TaskCreationOptions.RunContinuationsAsynchronously);
            connection = source.ConnectionTask = completion.Task;
            pending.Add(connection);
            source.Connection = ConnectionState.Connecting;
            source.PendingError = GuestError.None;
            ++source.ReadEpoch;
            ++source.WriteEpoch;
            ++source.TerminalEpoch;
            // Start and register atomically against close/disposal. The observer
            // catches all operation exceptions and publishes one retained result.
            _ = CompleteConnectionAsync(source, destination, actual, completion);
            if (connection.IsCompleted) return connection;
            if (nonBlocking) return Task.FromResult(Fail<int>(GuestError.InProgress));
        }
        return RunAsync<int>(token => WaitForConnectionAsync(connection, timeout, token), cancellation);
    }

    private async Task CompleteConnectionAsync(Entry source, GuestEndpoint destination, IPEndPoint actual,
        TaskCompletionSource<HostResult<int>> completion)
    {
        GuestError error = GuestError.None;
        try { await source.Socket.ConnectAsync(actual, stop.Token).ConfigureAwait(false); }
        catch (SocketException failure) { error = ConvertError(failure); }
        catch (OperationCanceledException) { error = GuestError.Canceled; }
        catch (ObjectDisposedException) { error = GuestError.Canceled; }
        catch (Exception) { error = GuestError.Io; }
        lock (sync)
        {
            if (source.Connection == ConnectionState.Closed) error = GuestError.Canceled;
            else
            {
                source.Connection = error == GuestError.None ? ConnectionState.Connected : ConnectionState.Failed;
                source.PendingError = error;
                if (error == GuestError.None) source.Remote = destination;
                ++source.ReadEpoch;
                ++source.WriteEpoch;
                ++source.TerminalEpoch;
            }
            // No code after completion accesses this entry or any descriptor.
            pending.Remove(completion.Task);
            completion.SetResult(error == GuestError.None ? HostResult<int>.Success(0) : Fail<int>(error));
        }
    }

    private static async Task<HostResult<int>> WaitForConnectionAsync(Task<HostResult<int>> connection,
        int timeout, CancellationToken cancellation)
    {
        try
        {
            return await connection.WaitAsync(timeout == 0 ? Timeout.InfiniteTimeSpan : TimeSpan.FromMilliseconds(timeout),
                cancellation).ConfigureAwait(false);
        }
        catch (TimeoutException) { return connection.IsCompleted ? await connection.ConfigureAwait(false) : Fail<int>(GuestError.InProgress); }
        catch (OperationCanceledException) { return Fail<int>(GuestError.Canceled); }
    }
}
