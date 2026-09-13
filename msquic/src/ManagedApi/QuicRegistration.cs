using static Managed.Transport.MsQuic;
using System.Net;
using System.Text;

namespace Managed.Transport.Api;

public sealed class QuicRegistration : QuicObject
{
    private readonly HashSet<QuicConfiguration> configurations = [];
    private readonly HashSet<QuicListener> listeners = [];
    private readonly HashSet<QuicConnection> connections = [];
    private QuicRegistration(QuicRuntime runtime) : base(runtime) { }
    internal unsafe QUIC_HANDLE* ConfigurationParentHandle => Handle;
    internal unsafe QUIC_HANDLE* ConnectionParentHandle => Handle;
    internal unsafe QUIC_HANDLE* ListenerParentHandle => Handle;

    internal static unsafe QuicRegistration Create(QuicRuntime runtime, string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        if (name.Contains('\0')) throw new ArgumentException("Application name contains NUL", nameof(name));
        byte[] bytes = Encoding.UTF8.GetBytes(name + "\0");
        if (bytes.Length > 256) throw new ArgumentOutOfRangeException(nameof(name));
        var owner = new QuicRegistration(runtime);
        try
        {
            fixed (byte* pointer = bytes)
            {
                QUIC_REGISTRATION_CONFIG config = new() { AppName = pointer, ExecutionProfile = QUIC_EXECUTION_PROFILE.QUIC_EXECUTION_PROFILE_LOW_LATENCY };
                QUIC_HANDLE* handle = null;
                QuicError.ThrowIfFailed(runtime.Api->RegistrationOpen(&config, &handle), "RegistrationOpen");
                owner.Handle = handle;
            }
            return owner;
        }
        catch { owner.RetireContext(); throw; }
    }

    internal IDisposable AdmitConfiguration(QuicConfiguration child)
    { var lease = EnterOperation(); try { lock (Gate) configurations.Add(child); return lease; } catch { lease.Dispose(); throw; } }
    internal IDisposable AdmitListener(QuicListener child)
    { var lease = EnterOperation(); try { lock (Gate) listeners.Add(child); return lease; } catch { lease.Dispose(); throw; } }
    internal IDisposable AdmitConnection(QuicConnection child)
    { var lease = EnterOperation(); try { lock (Gate) { if (connections.Count >= Runtime.Options.MaximumConnectionsPerRegistration) throw new InvalidOperationException("Connection admission limit reached."); connections.Add(child); } return lease; } catch { lease.Dispose(); throw; } }
    internal void ConfigurationClosed(QuicConfiguration child) { lock (Gate) configurations.Remove(child); }
    internal void ListenerClosed(QuicListener child) { lock (Gate) listeners.Remove(child); }
    internal void ConnectionClosed(QuicConnection child) { lock (Gate) connections.Remove(child); }

    public ValueTask<QuicConfiguration> CreateConfigurationAsync(IEnumerable<ReadOnlyMemory<byte>> applicationProtocols,
        QuicCredentials credentials, QuicSettings? settings = null, CancellationToken cancellationToken = default,
        QuicConfigurationOptions? options = null)
        => QuicConfiguration.CreateAsync(this, applicationProtocols, credentials, settings, cancellationToken, options);
    public ValueTask<QuicListener> ListenAsync(QuicConfiguration configuration, IPEndPoint endpoint,
        CancellationToken cancellationToken = default)
        => QuicListener.CreateAsync(this, configuration, endpoint, cancellationToken);
    public ValueTask<QuicConnection> ConnectAsync(QuicConfiguration configuration, string serverName, IPEndPoint endpoint,
        CancellationToken cancellationToken = default)
        => QuicConnection.ConnectAsync(this, configuration, serverName, endpoint, cancellationToken);

    public ValueTask<QuicConnection> ConnectAsync(QuicConfiguration configuration, string serverName, IPEndPoint endpoint,
        QuicConnectOptions options, CancellationToken cancellationToken = default)
        => QuicConnection.ConnectAsync(this, configuration, serverName, endpoint, cancellationToken, options);

    public async Task ShutdownAsync(ulong applicationError = 0)
    {
        QuicError.ErrorCode(applicationError);
        QuicConnection[] children;
        using (var operation = EnterOperation())
        {
            await Runtime.RunCleanup(() => ShutdownNative(applicationError)).ConfigureAwait(false);
            lock (Gate) children = connections.ToArray();
        }
        await Task.WhenAll(children.Select(child => child.ShutdownCompletion)).ConfigureAwait(false);
    }
    private unsafe void ShutdownNative(ulong error)
        => Runtime.Api->RegistrationShutdown(Handle, QUIC_CONNECTION_SHUTDOWN_FLAGS.QUIC_CONNECTION_SHUTDOWN_FLAG_NONE, error);
    private unsafe void CloseNative()
    { if (Handle != null) { Runtime.Api->RegistrationClose(Handle); Handle = null; } }

    public override ValueTask DisposeAsync() => new(CloseOnce(async () =>
    {
        QuicListener[] listenerChildren; QuicConnection[] connectionChildren; QuicConfiguration[] configurationChildren;
        lock (Gate) { listenerChildren = listeners.ToArray(); connectionChildren = connections.ToArray(); configurationChildren = configurations.ToArray(); }
        await Task.WhenAll(listenerChildren.Select(child => child.DisposeAsync().AsTask())).ConfigureAwait(false);
        await Task.WhenAll(connectionChildren.Select(child => child.DisposeAsync().AsTask())).ConfigureAwait(false);
        await Task.WhenAll(configurationChildren.Select(child => child.DisposeAsync().AsTask())).ConfigureAwait(false);
        await Runtime.RunCleanup(CloseNative).ConfigureAwait(false);
        RetireContext();
        Runtime.RegistrationClosed(this);
    }));
}
