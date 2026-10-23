using System.Buffers;
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using Raw = Managed.Security.S2n.S2nTls;

namespace Managed.Security.S2n;

public sealed record S2nNegotiated(S2nProtocol Protocol, ushort CipherSuite, string CipherName, string ApplicationProtocol, bool Resumed);
public sealed record S2nIoSnapshot(long CallbackCalls, long EncryptedSent, long EncryptedReceived);

/// <summary>Owns one actual translated connection. All entry is serialized per
/// connection. Independent connections may execute concurrently. Dispose cancels
/// nonblocking waits and drains entry before releasing callback roots or policy.</summary>
public sealed partial class S2nConnection : IDisposable, IAsyncDisposable
{
    private readonly SemaphoreSlim _entry = new(1, 1);
    private readonly object _disposeGate = new();
    private readonly CancellationTokenSource _stop = new();
    private readonly S2nConfiguration _configuration;
    private readonly IoCallbackState _io;
    private readonly bool _ownsTransport;
    private GCHandle _ioRoot;
    private GCHandle _verifyRoot;
    private nint _handle, _policy, _configHandle;
    private bool _configurationRetained, _started, _shutdown, _peerClosed;
    private Exception? _fault;
    private long _blindUntil;
    private int _disposing;
    private Task? _disposeTask;
    private Exception? _cancellationFailure;
    private Exception? _callbackDrainFailure;
    private (nint Info, Task<bool> Decision)? _validation;
    public S2nRole Role { get; }

    public S2nConnection(S2nConfiguration configuration, S2nRole role, Socket socket,
        S2nPolicy? policy = null, bool ownsSocket = true)
        : this(configuration, role, SocketTransport(configuration, role, socket, ownsSocket), policy, true) { }

    private static S2nSocketTransport SocketTransport(S2nConfiguration configuration, S2nRole role, Socket socket, bool ownsSocket)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        if (!Enum.IsDefined(role)) throw new ArgumentOutOfRangeException(nameof(role));
        return new(socket, ownsSocket);
    }

    public unsafe S2nConnection(S2nConfiguration configuration, S2nRole role, IS2nTransport transport,
        S2nPolicy? policy = null, bool ownsTransport = true)
    {
        ArgumentNullException.ThrowIfNull(configuration); ArgumentNullException.ThrowIfNull(transport);
        if (!Enum.IsDefined(role)) throw new ArgumentOutOfRangeException(nameof(role));
        _configuration = configuration; _io = new(transport); _ownsTransport = ownsTransport; Role = role;
        try
        {
            nint config = configuration.Retain(); _configurationRetained = true; _configHandle = config;
            _handle = Native.Require(Raw.s2n_connection_new((Raw.s2n_mode)(int)role), "s2n_connection_new");
            var c = (Raw.s2n_connection*)_handle;
            Native.Check(Raw.s2n_connection_set_config(c, (Raw.s2n_config*)config), "s2n_connection_set_config");
            if (policy != null)
            {
                _policy = Native.Require(Raw.dotcc_s2n_policy_new((byte)policy.Minimum, (byte)policy.Maximum, policy.CipherSuite), "dotcc_s2n_policy_new");
                Native.Check(Raw.dotcc_s2n_connection_use_policy(c, (Raw.dotcc_s2n_policy*)_policy), "dotcc_s2n_connection_use_policy");
            }
            // Honor the returned delay in the owner instead of sleeping inside
            // translated C, which would prevent cancellation of nonblocking I/O.
            Native.Check(Raw.s2n_connection_set_blinding(c, (Raw.s2n_blinding)(int)S2nBlinding.SelfService), "s2n_connection_set_blinding");
            _ioRoot = GCHandle.Alloc(_io);
            void* context = (void*)GCHandle.ToIntPtr(_ioRoot);
            Native.Check(Raw.s2n_connection_set_recv_ctx(c, context), "s2n_connection_set_recv_ctx");
            Native.Check(Raw.s2n_connection_set_send_ctx(c, context), "s2n_connection_set_send_ctx");
            Native.Check(Raw.s2n_connection_set_recv_cb(c, &IoCallbackState.Receive), "s2n_connection_set_recv_cb");
            Native.Check(Raw.s2n_connection_set_send_cb(c, &IoCallbackState.Send), "s2n_connection_set_send_cb");
        }
        catch
        {
            _disposing = 1;
            ReleaseNative();
            if (_ownsTransport) transport.Dispose();
            throw;
        }
    }

    private static void GuardCallback()
    { if (CallbackScope.Current != null || CallbackScope.InApplicationCallback.Value) throw new InvalidOperationException("Connection entry or disposal from a TLS callback is forbidden."); }
    private void RequireUsable()
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposing) != 0 || _handle == 0, this);
        if (_fault != null) throw new InvalidOperationException("Connection is terminal after a failed or cancelled TLS operation.", _fault);
    }
    private unsafe delegate T ConnectionCall<T>(Raw.s2n_connection* connection);
    private unsafe T Enter<T>(ConnectionCall<T> operation, bool setupOnly = false)
    {
        GuardCallback(); _entry.Wait();
        try
        {
            RequireUsable();
            if (setupOnly && _started) throw new InvalidOperationException("This option must be set before negotiation.");
            using var scope = new CallbackScope(this, _stop.Token);
            T value = operation((Raw.s2n_connection*)_handle);
            scope.Exception?.Throw();
            return value;
        }
        finally { _entry.Release(); }
    }

    public unsafe S2nConnection SetServerName(string name)
    {
        byte[] bytes = Native.Utf8(name);
        Enter(c => { fixed (byte* b = bytes) Native.Check(Raw.s2n_set_server_name(c, b), "s2n_set_server_name"); return 0; }, true);
        return this;
    }
    public unsafe S2nConnection SetNamedPolicy(string name)
    {
        byte[] bytes = Native.Utf8(name);
        Enter(c => { fixed (byte* b = bytes) Native.Check(Raw.s2n_connection_set_cipher_preferences(c, b), "s2n_connection_set_cipher_preferences"); return 0; }, true);
        return this;
    }
    public unsafe S2nConnection SetClientAuthentication(S2nClientAuthentication mode)
    {
        if (!Enum.IsDefined(mode)) throw new ArgumentOutOfRangeException(nameof(mode));
        Enter(c => { Native.Check(Raw.s2n_connection_set_client_auth_type(c, (Raw.s2n_cert_auth_type)(int)mode), "s2n_connection_set_client_auth_type"); return 0; }, true);
        return this;
    }
    public unsafe S2nConnection SetBlinding(S2nBlinding mode)
    {
        if (!Enum.IsDefined(mode)) throw new ArgumentOutOfRangeException(nameof(mode));
        Enter(c => { Native.Check(Raw.s2n_connection_set_blinding(c, (Raw.s2n_blinding)(int)mode), "s2n_connection_set_blinding"); return 0; }, true);
        return this;
    }
    public unsafe S2nConnection SetHostnameVerifier(Func<string, bool> verifier)
    {
        ArgumentNullException.ThrowIfNull(verifier);
        Enter(c =>
        {
            GCHandle root = GCHandle.Alloc(new CallbackState { VerifyHost = verifier });
            try { Native.Check(Raw.s2n_connection_set_verify_host_callback(c, &Callbacks.VerifyHost, (void*)GCHandle.ToIntPtr(root)), "s2n_connection_set_verify_host_callback"); }
            catch { root.Free(); throw; }
            if (_verifyRoot.IsAllocated) _verifyRoot.Free(); _verifyRoot = root;
            return 0;
        }, true);
        return this;
    }

    public S2nIoSnapshot InspectIo() => new(Interlocked.Read(ref _io.Calls), Interlocked.Read(ref _io.BytesSent), Interlocked.Read(ref _io.BytesReceived));
    public bool PeerClosed => Volatile.Read(ref _peerClosed);
    public bool ShutdownComplete => Volatile.Read(ref _shutdown);
    public unsafe S2nNegotiated Negotiated => Enter(c =>
    {
        if (!_started) throw new InvalidOperationException("Negotiate first.");
        byte first = 0, second = 0;
        Native.Check(Raw.s2n_connection_get_cipher_iana_value(c, &first, &second), "s2n_connection_get_cipher_iana_value");
        return new S2nNegotiated((S2nProtocol)Raw.s2n_connection_get_actual_protocol_version(c), (ushort)(first * 256 + second),
            Native.Text(Raw.s2n_connection_get_cipher(c)), Native.Text(Raw.s2n_get_application_protocol(c)), Raw.s2n_connection_is_session_resumed(c) == 1);
    });

    public async ValueTask NegotiateAsync(CancellationToken cancellationToken = default)
    { await RunAsync(Operation.Negotiate, default, cancellationToken).ConfigureAwait(false); }
    public ValueTask<int> SendAsync(ReadOnlyMemory<byte> source, CancellationToken cancellationToken = default)
        => RunAsync(Operation.Send, MemoryMarshal.AsMemory(source), cancellationToken);
    public ValueTask<int> ReceiveAsync(Memory<byte> destination, CancellationToken cancellationToken = default)
        => RunAsync(Operation.Receive, destination, cancellationToken);
    public async ValueTask ShutdownAsync(CancellationToken cancellationToken = default)
    { await RunAsync(Operation.Shutdown, default, cancellationToken).ConfigureAwait(false); }

    private enum Operation { Negotiate, Send, Receive, Shutdown }
    private readonly record struct Step(long Result, int Blocked, S2nException? Error, ExceptionDispatchInfo? CallbackError);
    private unsafe Step Invoke(Operation operation, Memory<byte> buffer, CancellationToken cancellationToken)
    {
        using var scope = new CallbackScope(this, cancellationToken);
        var c = (Raw.s2n_connection*)_handle;
        Raw.s2n_blocked_status blocked = default;
        long result;
        using (MemoryHandle pin = buffer.Pin())
            result = operation switch
            {
                Operation.Negotiate => Raw.s2n_negotiate(c, &blocked),
                Operation.Send => Raw.s2n_send(c, pin.Pointer, buffer.Length, &blocked),
                Operation.Receive => Raw.s2n_recv(c, pin.Pointer, buffer.Length, &blocked),
                Operation.Shutdown => Raw.s2n_shutdown(c, &blocked),
                _ => throw new UnreachableException()
            };
        S2nException? error = result < 0 ? Native.Error(operation.ToString()) : null;
        if (error != null && error.ErrorType != 3)
        {
            ulong delay = Raw.s2n_connection_get_delay(c);
            double ticks = delay * (double)Stopwatch.Frequency / 1_000_000_000d;
            _blindUntil = Stopwatch.GetTimestamp() + (long)Math.Min(ticks, long.MaxValue / 2d);
        }
        return new(result, (int)blocked, error, scope.Exception);
    }

    private async ValueTask<int> RunAsync(Operation operation, Memory<byte> buffer, CancellationToken cancellationToken)
    {
        GuardCallback();
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposing) != 0, this);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _stop.Token);
        CancellationToken token = linked.Token;
        await _entry.WaitAsync(token).ConfigureAwait(false);
        bool entered = false;
        try
        {
            RequireUsable(); token.ThrowIfCancellationRequested();
            if (_shutdown && operation != Operation.Shutdown) throw new InvalidOperationException("TLS shutdown is complete.");
            if ((operation is Operation.Send or Operation.Receive) && buffer.Length == 0) return 0;
            if (operation != Operation.Negotiate && !_started) throw new InvalidOperationException("Negotiate first.");
            _started = true;
            while (true)
            {
                token.ThrowIfCancellationRequested(); entered = true;
                Step step = Invoke(operation, buffer, token);
                step.CallbackError?.Throw();
                if (step.Result >= 0)
                {
                    if (operation == Operation.Receive && step.Result == 0) _peerClosed = true;
                    if (operation == Operation.Shutdown) { _shutdown = true; _peerClosed = true; }
                    return checked((int)step.Result);
                }
                if (step.Error!.ErrorType != 3) throw step.Error;
                if (step.Blocked is 1 or 2) await _io.Transport.WaitAsync(step.Blocked == 1, token).ConfigureAwait(false);
                else if (step.Blocked == 3 && _validation is { } validation)
                {
                    bool accepted;
                    try { accepted = await validation.Decision.WaitAsync(token).ConfigureAwait(false); }
                    catch when (validation.Decision.IsCompleted) { _validation = null; throw; }
                    ApplyValidation(validation.Info, accepted); _validation = null;
                }
                else throw new InvalidOperationException($"Unhandled application blocked status {step.Blocked}; no pending callback can resume it.");
            }
        }
        catch (Exception error) when (entered)
        {
            _fault = error;
            await HonorBlindingAsync().ConfigureAwait(false);
            throw;
        }
        finally { _entry.Release(); }
    }

    internal void SetPendingValidation(nint info, Task<bool> decision)
    {
        if (_validation != null) throw new InvalidOperationException("A certificate decision is already pending.");
        _validation = (info, decision);
        // Observe asynchronous failures even when disposal/cancellation abandons
        // the application task. No continuation retains a C pointer or calls C.
        _ = decision.ContinueWith(t => _ = t.Exception, CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
    }
    private static unsafe void ApplyValidation(nint info, bool accepted)
        => Native.Check(accepted ? Raw.s2n_cert_validation_accept((Raw.s2n_cert_validation_info*)info)
            : Raw.s2n_cert_validation_reject((Raw.s2n_cert_validation_info*)info), "certificate validation decision");
    private async ValueTask HonorBlindingAsync()
    {
        while (true)
        {
            long remaining = _blindUntil - Stopwatch.GetTimestamp();
            if (remaining <= 0) return;
            await Task.Delay(TimeSpan.FromSeconds(Math.Min(remaining / (double)Stopwatch.Frequency, 86400))).ConfigureAwait(false);
        }
    }

    private unsafe void ReleaseNative()
    {
        try
        {
            if (_handle != 0) Native.Check(Raw.s2n_connection_free((Raw.s2n_connection*)_handle), "s2n_connection_free");
            _handle = 0;
            // The narrowed policy MUST outlive s2n_connection_free.
            if (_policy != 0) Raw.dotcc_s2n_policy_free((Raw.dotcc_s2n_policy*)_policy);
            _policy = 0;
            if (_ioRoot.IsAllocated) _ioRoot.Free();
            if (_verifyRoot.IsAllocated) _verifyRoot.Free();
            _validation = null;
            if (_configurationRetained) { _configurationRetained = false; _configuration.Release(); }
        }
        catch (Exception error) { S2nRuntime.Quarantine(error); throw; }
    }
    public ValueTask DisposeAsync()
    {
        GuardCallback();
        lock (_disposeGate)
        {
            if (_disposeTask == null)
            {
                Interlocked.Exchange(ref _disposing, 1);
                try { _stop.Cancel(); } catch (Exception error) { _cancellationFailure = error; }
                _disposeTask = DisposeCoreAsync();
            }
            return new(_disposeTask);
        }
    }
    private async Task DisposeCoreAsync()
    {
        await _entry.WaitAsync().ConfigureAwait(false);
        try
        {
            await HonorBlindingAsync().ConfigureAwait(false);
            if (_validation is { } validation)
            {
                try { await validation.Decision.ConfigureAwait(false); }
                catch (OperationCanceledException) when (_stop.IsCancellationRequested) { }
                catch (Exception error) { _callbackDrainFailure = error; }
                _validation = null;
            }
            ReleaseNative();
        }
        finally
        {
            if (_ownsTransport) _io.Transport.Dispose();
            _entry.Release(); GC.SuppressFinalize(this);
        }
        if (_cancellationFailure != null) ExceptionDispatchInfo.Capture(_cancellationFailure).Throw();
        if (_callbackDrainFailure != null) ExceptionDispatchInfo.Capture(_callbackDrainFailure).Throw();
    }
    public void Dispose() => DisposeAsync().AsTask().GetAwaiter().GetResult();
    ~S2nConnection()
    {
        if (_io == null || (_disposing != 0 && _disposeTask == null)) return;
        try { Dispose(); } catch (Exception error) { S2nRuntime.Quarantine(error); }
    }
}
