using System.Net;
using System.Net.Sockets;
using Managed.Transport.Hosting;

namespace Managed.Transport.Api;

public sealed record QuicListenerStatistics(ulong AcceptedConnections, ulong RejectedConnections, ulong BindingDroppedPackets);
public sealed record QuicListenerDosState(long Sequence, bool RetryModeEnabled);

/// <summary>A restartable listener whose pending accepted connections have bounded ownership.</summary>
public sealed class QuicListener : QuicObject
{
    private enum ListenerState { Stopped, Starting, Started, Stopping }
    private readonly QuicRegistration registration;
    private readonly QuicConfiguration configuration;
    private readonly SemaphoreSlim lifecycle = new(1, 1);
    private sealed class Incoming
    {
        internal readonly TaskCompletionSource<QuicConnection?> Commitment = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal QuicConnection? Connection;
        internal Task Observer = Task.CompletedTask;
    }
    private readonly HashSet<Incoming> pending = [];
    private readonly Queue<Incoming> ready = [];
    private TaskCompletionSource<QuicConnection>? acceptWaiter;
    private TaskCompletionSource? stopped;
    private IDisposable? configurationLease;
    private IPEndPoint requestedEndpoint;
    private ListenerState state;
    private bool registered;
    private QuicListenerDosState? dosState;
    public QuicListenerDosState? LastDosModeChange { get { lock (Gate) return dosState; } }

    public unsafe QuicListenerStatistics GetStatistics(QuicParameterPriority priority = QuicParameterPriority.Normal)
    {
        using var operation = EnterOperation();
        QUIC_LISTENER_STATISTICS value = default; uint length = (uint)sizeof(QUIC_LISTENER_STATISTICS);
        QuicError.ThrowIfFailed(Runtime.Api->GetParam(Handle, QuicParameterDispatch.Apply(MsQuic.QUIC_PARAM_LISTENER_STATS, priority), &length, &value), "listener statistics");
        if (length != sizeof(QUIC_LISTENER_STATISTICS)) throw new InvalidOperationException("Unexpected listener statistics size");
        return new(value.TotalAcceptedConnections, value.TotalRejectedConnections, value.BindingRecvDroppedPackets);
    }
    public unsafe bool GetDosModeEventsEnabled(QuicParameterPriority priority = QuicParameterPriority.Normal)
    {
        using var operation = EnterOperation();
        byte enabled = 0; uint length = sizeof(byte);
        QuicError.ThrowIfFailed(Runtime.Api->GetParam(Handle, QuicParameterDispatch.Apply(MsQuic.QUIC_PARAM_DOS_MODE_EVENTS, priority), &length, &enabled), "listener DoS notifications");
        if (length != sizeof(byte)) throw new InvalidOperationException("Unexpected listener DoS option size");
        return enabled != 0;
    }
    public unsafe void SetDosModeEventsEnabled(bool enabled, QuicParameterPriority priority = QuicParameterPriority.Normal)
    {
        using var operation = EnterOperation();
        byte value = enabled ? (byte)1 : (byte)0;
        QuicError.ThrowIfFailed(Runtime.Api->SetParam(Handle, QuicParameterDispatch.Apply(MsQuic.QUIC_PARAM_DOS_MODE_EVENTS, priority), sizeof(byte), &value), "listener DoS notifications");
    }

    private QuicListener(QuicRegistration registration, QuicConfiguration configuration, IPEndPoint endpoint)
        : base(registration.Runtime)
    { this.registration = registration; this.configuration = configuration; requestedEndpoint = endpoint; }

    internal static async ValueTask<QuicListener> CreateAsync(QuicRegistration registration,
        QuicConfiguration configuration, IPEndPoint endpoint, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        if (!configuration.BelongsTo(registration)) throw new ArgumentException("Configuration belongs to another registration", nameof(configuration));
        if (!configuration.IsServer) throw new ArgumentException("A listener requires server credentials", nameof(configuration));
        endpoint = SnapshotEndpoint(endpoint);
        cancellationToken.ThrowIfCancellationRequested();
        var listener = new QuicListener(registration, configuration, endpoint);
        try
        {
            using var initialization = listener.EnterOperation();
            listener.InitializeCore();
            await listener.StartAsync(endpoint, cancellationToken).ConfigureAwait(false);
            return listener;
        }
        catch { await listener.DisposeAsync().ConfigureAwait(false); throw; }
    }
    private unsafe void InitializeCore()
    {
        using var admission = registration.AdmitListener(this); registered = true;
        configurationLease = configuration.AcquireLease();
        QUIC_HANDLE* opened = null;
        uint status = Runtime.Api->ListenerOpen(registration.ListenerParentHandle, &Callback, Context, &opened);
        Handle = opened;
        QuicError.ThrowIfFailed(status, "ListenerOpen");
        if (opened == null) throw new InvalidOperationException("ListenerOpen returned no handle");
    }
    private static IPEndPoint SnapshotEndpoint(IPEndPoint endpoint)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        if (endpoint.AddressFamily is not (AddressFamily.InterNetwork or AddressFamily.InterNetworkV6))
            throw new ArgumentException("An IPv4 or IPv6 endpoint is required", nameof(endpoint));
        var address = endpoint.AddressFamily == AddressFamily.InterNetworkV6
            ? new IPAddress(endpoint.Address.GetAddressBytes(), endpoint.Address.ScopeId)
            : new IPAddress(endpoint.Address.GetAddressBytes());
        return new IPEndPoint(address, endpoint.Port);
    }

    public IPEndPoint LocalEndPoint => GetLocalEndPoint();
    /// <summary>Queries the actual bound address. Listener parameters execute
    /// inline; accepting High does not imply queued priority scheduling.</summary>
    public unsafe IPEndPoint GetLocalEndPoint(QuicParameterPriority priority = QuicParameterPriority.Normal)
    {
        using var operation = EnterOperation();
        QUIC_ADDR address = default; uint length = (uint)sizeof(QUIC_ADDR);
        QuicError.ThrowIfFailed(Runtime.Api->GetParam(Handle, QuicParameterDispatch.Apply(MsQuic.QUIC_PARAM_LISTENER_LOCAL_ADDRESS, priority), &length, &address), "listener local endpoint");
        return MsQuicHost.DatagramEndpoint(&address);
    }
    public ValueTask StartAsync(CancellationToken cancellationToken = default)
    {
        IPEndPoint endpoint;
        lock (Gate) endpoint = requestedEndpoint;
        return StartAsync(endpoint, cancellationToken);
    }
    public async ValueTask StartAsync(IPEndPoint endpoint, CancellationToken cancellationToken = default)
    {
        endpoint = SnapshotEndpoint(endpoint);
        using var operation = EnterOperation();
        await lifecycle.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            lock (Gate)
            {
                if (state != ListenerState.Stopped) throw new InvalidOperationException("Listener must finish stopping before starting again");
                state = ListenerState.Starting;
            }
            try
            {
                StartCore(endpoint);
                lock (Gate) { requestedEndpoint = endpoint; state = ListenerState.Started; }
            }
            catch { lock (Gate) state = ListenerState.Stopped; throw; }
        }
        finally { lifecycle.Release(); }
    }
    private unsafe void StartCore(IPEndPoint endpoint)
    {
        QUIC_ADDR address = default;
        MsQuicHost.DatagramAddress(endpoint, &address);
        var descriptors = new QUIC_BUFFER[configuration.ProtocolBytes.Length];
        for (int i = 0; i < descriptors.Length; i++)
            fixed (byte* bytes = configuration.ProtocolBytes[i]) descriptors[i] = new() { Buffer = bytes, Length = (uint)configuration.ProtocolBytes[i].Length };
        fixed (QUIC_BUFFER* buffers = descriptors)
            QuicError.ThrowIfFailed(Runtime.Api->ListenerStart(Handle, buffers, (uint)descriptors.Length, &address), "ListenerStart");
    }
    public async ValueTask StopAsync(CancellationToken cancellationToken = default)
    {
        Task completion;
        using (var operation = EnterOperation())
        {
            await lifecycle.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                bool dispatch = false;
                lock (Gate)
                {
                    if (state == ListenerState.Stopped) return;
                    if (state == ListenerState.Started)
                    {
                        stopped = new(TaskCreationOptions.RunContinuationsAsynchronously);
                        state = ListenerState.Stopping; dispatch = true;
                    }
                    completion = stopped!.Task;
                }
                if (dispatch) StopCore();
            }
            finally { lifecycle.Release(); }
        }
        // Stop completion needs the core worker; close may run while we wait.
        await completion.WaitAsync(cancellationToken).ConfigureAwait(false);
    }
    private unsafe void StopCore() => Runtime.Api->ListenerStop(Handle);

    public ValueTask<QuicConnection> AcceptConnectionAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var operation = EnterOperation();
        TaskCompletionSource<QuicConnection> waiter;
        lock (Gate)
        {
            if (acceptWaiter != null) throw new InvalidOperationException("Only one connection accept operation is allowed");
            if (ready.TryDequeue(out var entry)) { pending.Remove(entry); return ValueTask.FromResult(entry.Connection!); }
            acceptWaiter = waiter = new(TaskCreationOptions.RunContinuationsAsynchronously);
        }
        return new ValueTask<QuicConnection>(WaitAcceptAsync(waiter, cancellationToken));
    }
    private async Task<QuicConnection> WaitAcceptAsync(TaskCompletionSource<QuicConnection> waiter, CancellationToken cancellationToken)
    {
        using var cancellation = cancellationToken.Register(() =>
        {
            lock (Gate)
            {
                if (!ReferenceEquals(acceptWaiter, waiter)) return;
                acceptWaiter = null; waiter.TrySetCanceled(cancellationToken);
            }
        });
        return await waiter.Task.ConfigureAwait(false);
    }

    private static unsafe uint Callback(QUIC_HANDLE* listener, void* context, QUIC_LISTENER_EVENT* notification)
    {
        var owner = FromContext<QuicListener>(context);
        if (notification->Type == QUIC_LISTENER_EVENT_TYPE.QUIC_LISTENER_EVENT_NEW_CONNECTION)
            return owner.AcceptCore(notification->NEW_CONNECTION.Connection);
        try
        {
            if (notification->Type == QUIC_LISTENER_EVENT_TYPE.QUIC_LISTENER_EVENT_DOS_MODE_CHANGED)
            {
                bool enabled = notification->DOS_MODE_CHANGED.DosModeEnabled != 0;
                lock (owner.Gate) owner.dosState = new(checked((owner.dosState?.Sequence ?? 0) + 1), enabled);
            }
            if (notification->Type == QUIC_LISTENER_EVENT_TYPE.QUIC_LISTENER_EVENT_STOP_COMPLETE)
            {
                lock (owner.Gate) { owner.state = ListenerState.Stopped; owner.stopped?.TrySetResult(); }
            }
        }
        catch (Exception error) { owner.RecordCallbackFailure(error); }
        return 0;
    }
    private unsafe uint AcceptCore(QUIC_HANDLE* handle)
    {
        Incoming entry;
        try
        {
            entry = new Incoming();
            lock (Gate)
            {
                if (IsClosing || state is not (ListenerState.Starting or ListenerState.Started)) return 111;
                if (pending.Count >= Runtime.Options.MaximumPendingConnections) return 12;
                pending.Add(entry);
                try { entry.Observer = Task.Run(() => ObserveConnectionAsync(entry)); }
                catch { pending.Remove(entry); throw; }
            }
        }
        catch (Exception error) { RecordCallbackFailure(error); return 12; }
        try
        {
            // Allocate the listener's tracking state before the factory commits
            // the native handle. Only nonallocating publication follows it.
            entry.Connection = QuicConnection.Accept(registration, configuration, handle);
            entry.Commitment.TrySetResult(entry.Connection);
            return 0;
        }
        catch (Exception error)
        {
            RecordCallbackFailure(error);
            if (entry.Connection != null)
            {
                // A publication error after the factory returned cannot reject
                // an already-owned native handle. Preserve the tracked owner.
                entry.Commitment.TrySetResult(entry.Connection);
                return 0;
            }
            entry.Commitment.TrySetResult(null);
            return 12; // The factory throws only before native ownership commits.
        }
    }
    private async Task ObserveConnectionAsync(Incoming entry)
    {
        bool delivered = false;
        QuicConnection? connection = null;
        try
        {
            connection = await entry.Commitment.Task.ConfigureAwait(false);
            if (connection == null) return;
            await connection.ConnectedCompletion.ConfigureAwait(false);
            lock (Gate)
            {
                if (!IsClosing)
                {
                    if (acceptWaiter != null)
                    {
                        var waiter = acceptWaiter; acceptWaiter = null;
                        pending.Remove(entry); delivered = true; waiter.TrySetResult(connection);
                    }
                    else { ready.Enqueue(entry); delivered = true; }
                }
            }
        }
        catch (Exception error)
        {
            lock (Gate) { acceptWaiter?.TrySetException(error); acceptWaiter = null; }
        }
        finally
        {
            if (!delivered)
            {
                try { if (connection != null) await connection.DisposeAsync().ConfigureAwait(false); }
                catch (Exception error) { RecordCallbackFailure(error); }
                finally { lock (Gate) pending.Remove(entry); }
            }
        }
    }

    public override ValueTask DisposeAsync() => new(CloseOnce(CloseAsync));
    private async Task CloseAsync()
    {
        lock (Gate) { acceptWaiter?.TrySetException(new ObjectDisposedException(nameof(QuicListener))); acceptWaiter = null; }
        // ListenerClose synchronously drains native callbacks on the cleanup
        // worker, so the following snapshot includes all committed admissions.
        if (HasNativeHandle) await Runtime.RunCleanup(CloseCore).ConfigureAwait(false);
        Incoming[] children;
        lock (Gate) { children = pending.ToArray(); ready.Clear(); }
        await Task.WhenAll(children.Select(CloseIncomingAsync)).ConfigureAwait(false);
        lock (Gate) { pending.Clear(); state = ListenerState.Stopped; stopped?.TrySetResult(); }
        RetireContext();
        configurationLease?.Dispose(); configurationLease = null;
        if (registered) { registered = false; registration.ListenerClosed(this); }
    }
    private static async Task CloseIncomingAsync(Incoming entry)
    {
        if (entry.Connection != null) await entry.Connection.DisposeAsync().ConfigureAwait(false);
        await entry.Observer.ConfigureAwait(false);
    }
    private unsafe void CloseCore()
    { if (Handle != null) { Runtime.Api->ListenerClose(Handle); Handle = null; } }
}
