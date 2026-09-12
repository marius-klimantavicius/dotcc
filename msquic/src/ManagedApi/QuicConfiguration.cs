using System.Security.Cryptography;
using Managed.Transport.Hosting;

namespace Managed.Transport.Api;

public sealed record QuicConfigurationOptions
{
    /// <summary>Retains a peer-opened stream's ID credit until its owning stream
    /// is disposed, instead of returning that credit when transport shutdown ends.</summary>
    public bool DelayAcceptedStreamCreditUntilClose { get; init; }
    /// <summary>Validates copied server resumption application state after the
    /// core authenticates and validates the ticket. Null accepts that state.
    /// The initial invocation runs on the core worker and must not block. An
    /// incomplete ValueTask defers completion; false falls back to a full
    /// handshake. This policy never permits 0-RTT application data.</summary>
    public Func<ReadOnlyMemory<byte>, CancellationToken, ValueTask<bool>>? ServerResumptionValidation { get; init; }
}

/// <summary>An owning QUIC v1 configuration. Listener/connection leases keep its
/// handle and credentials alive until those children finish closing.</summary>
public sealed partial class QuicConfiguration : QuicObject
{
    private readonly QuicRegistration registration;
    private readonly object settingsGate = new();
    private MsQuicHost.CredentialRegistration? credential;
    private TaskCompletionSource<uint>? credentialCompletion;
    private bool registered;
    public bool IsServer { get; }
    internal bool DelayAcceptedStreamCreditUntilClose { get; }
    internal byte[][] ProtocolBytes { get; }
    internal Func<QuicCertificateValidation, CancellationToken, ValueTask<bool>>? CertificateValidation { get; }
    internal Func<ReadOnlyMemory<byte>, CancellationToken, ValueTask<bool>>? ServerResumptionValidation { get; }

    private QuicConfiguration(QuicRegistration owner, byte[][] protocols, QuicCredentials credentials,
        QuicConfigurationOptions? options) : base(owner.Runtime)
    {
        registration = owner; ProtocolBytes = protocols; IsServer = credentials.IsServer;
        CertificateValidation = credentials.CertificateValidation;
        ServerResumptionValidation = options?.ServerResumptionValidation;
        DelayAcceptedStreamCreditUntilClose = options?.DelayAcceptedStreamCreditUntilClose ?? false;
    }

    internal static async ValueTask<QuicConfiguration> CreateAsync(QuicRegistration owner,
        IEnumerable<ReadOnlyMemory<byte>> protocols, QuicCredentials credentials, QuicSettings? settings,
        CancellationToken token, QuicConfigurationOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(owner); ArgumentNullException.ThrowIfNull(credentials);
        if (options?.ServerResumptionValidation != null && !credentials.IsServer)
            throw new ArgumentException("Only a server configuration can validate resumption application state.", nameof(options));
        byte[][] protocolBytes = SnapshotProtocols(protocols);
        QUIC_SETTINGS nativeSettings = (settings ?? new()).ToNative(initialize: true);
        token.ThrowIfCancellationRequested();
        var configuration = new QuicConfiguration(owner, protocolBytes, credentials, options);
        try
        {
            // The initialization operation also prevents parent-driven disposal
            // from closing a handle while an inline credential callback unwinds.
            using var initialization = configuration.EnterOperation();
            configuration.InitializeCore(credentials, nativeSettings, token);
            uint status = await configuration.credentialCompletion!.Task.WaitAsync(token).ConfigureAwait(false);
            QuicError.ThrowIfFailed(status, "ConfigurationLoadCredential completion");
            if (configuration.CallbackFailure is { } error) throw error;
            token.ThrowIfCancellationRequested();
            return configuration;
        }
        catch
        {
            // Cancellation cancels publication, not the native load. Retain the
            // callback context until completion and close safely off the worker.
            await configuration.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    private static byte[][] SnapshotProtocols(IEnumerable<ReadOnlyMemory<byte>> protocols)
    {
        ArgumentNullException.ThrowIfNull(protocols);
        var snapshots = new List<byte[]>();
        int total = 0;
        foreach (var protocol in protocols)
        {
            if (protocol.Length is < 1 or > 255)
                throw new ArgumentException("Each ALPN must contain 1–255 bytes.", nameof(protocols));
            if (protocol.Span.Contains((byte)0))
                throw new NotSupportedException("The selected protocol profile does not support ALPN containing NUL.");
            total = checked(total + 1 + protocol.Length);
            if (total > ushort.MaxValue) throw new ArgumentException("The encoded ALPN list exceeds 65535 bytes.", nameof(protocols));
            byte[] snapshot = GC.AllocateUninitializedArray<byte>(protocol.Length, pinned: true);
            protocol.Span.CopyTo(snapshot); snapshots.Add(snapshot);
        }
        if (snapshots.Count == 0) throw new ArgumentException("At least one ALPN is required.", nameof(protocols));
        return snapshots.ToArray();
    }

    private unsafe void InitializeCore(QuicCredentials credentials, QUIC_SETTINGS settings, CancellationToken token)
    {
        using var admission = registration.AdmitConfiguration(this); registered = true;
        token.ThrowIfCancellationRequested();
        // This independently owned credential copy is acquired synchronously,
        // before CreateAsync's first await or any native callback dispatch.
        credential = credentials.Register(Runtime.Host);
        var descriptors = new QUIC_BUFFER[ProtocolBytes.Length];
        for (int i = 0; i < descriptors.Length; i++)
            fixed (byte* bytes = ProtocolBytes[i]) descriptors[i] = new() { Length = (uint)ProtocolBytes[i].Length, Buffer = bytes };
        QUIC_HANDLE* opened = null;
        uint status;
        fixed (QUIC_BUFFER* buffers = descriptors)
            status = Runtime.Api->ConfigurationOpen(registration.ConfigurationParentHandle, buffers, (uint)descriptors.Length,
                &settings, (uint)sizeof(QUIC_SETTINGS), Context, &opened);
        Handle = opened;
        QuicError.ThrowIfFailed(status, "ConfigurationOpen");
        if (opened == null) throw new InvalidOperationException("ConfigurationOpen returned no handle.");
        QuicRuntime.SetVersionOne(Runtime.Api, Handle, MsQuic.QUIC_PARAM_CONFIGURATION_VERSION_SETTINGS);
        token.ThrowIfCancellationRequested();
        credentialCompletion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        QUIC_CREDENTIAL_FLAGS flags = credentials.LoadAsynchronously ? (QUIC_CREDENTIAL_FLAGS)2 : 0;
        if (CertificateValidation != null) flags |= (QUIC_CREDENTIAL_FLAGS)0x4030;
        try
        {
            status = Runtime.Host.LoadCredential(Handle, credential, flags,
                credentials.LoadAsynchronously ? &CredentialLoaded : null);
        }
        catch { credentialCompletion.TrySetResult(22); throw; }
        if (!credentials.LoadAsynchronously || QuicError.Failed(status)) credentialCompletion.TrySetResult(status);
        QuicError.ThrowIfFailed(status, "ConfigurationLoadCredential");
    }

    private static unsafe void CredentialLoaded(QUIC_HANDLE* handle, void* context, uint status)
    {
        var owner = FromContext<QuicConfiguration>(context);
        try
        {
            if (handle != owner.Handle || owner.credentialCompletion == null)
                throw new InvalidOperationException("Credential completion does not match its configuration.");
            if (!owner.credentialCompletion.TrySetResult(status))
                throw new InvalidOperationException("Credential loading completed more than once.");
        }
        catch (Exception error)
        {
            owner.RecordCallbackFailure(error);
            owner.credentialCompletion?.TrySetResult(22);
        }
    }

    internal IDisposable AcquireLease() => EnterOperation();
    internal bool BelongsTo(QuicRegistration owner) => ReferenceEquals(registration, owner);

    public unsafe QuicSettings GetSettings()
    {
        using var operation = EnterOperation();
        lock (settingsGate) return QuicSettings.FromNative(GetNativeSettings());
    }

    private unsafe QUIC_SETTINGS GetNativeSettings()
    {
        QUIC_SETTINGS settings = default; uint length = (uint)sizeof(QUIC_SETTINGS);
        QuicError.ThrowIfFailed(Runtime.Api->GetParam(Handle, MsQuic.QUIC_PARAM_CONFIGURATION_SETTINGS, &length, &settings), "Get configuration settings");
        return settings;
    }

    public unsafe void SetSettings(QuicSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        QUIC_SETTINGS native = settings.ToNative();
        using var operation = EnterOperation();
        lock (settingsGate)
        {
            QuicSettings.ValidateUpdate(native, GetNativeSettings());
            QuicError.ThrowIfFailed(Runtime.Api->SetParam(Handle, MsQuic.QUIC_PARAM_CONFIGURATION_SETTINGS,
                (uint)sizeof(QUIC_SETTINGS), &native), "Set configuration settings");
        }
    }

    public unsafe void SetTicketKeys(IEnumerable<QuicTicketKey> keys)
    {
        ArgumentNullException.ThrowIfNull(keys);
        if (!IsServer) throw new InvalidOperationException("Only server configurations own ticket keys.");
        var selected = new List<QuicTicketKey>(16);
        foreach (var key in keys)
        {
            ArgumentNullException.ThrowIfNull(key);
            if (selected.Count == 16) throw new ArgumentException("At most 16 ticket keys may be configured.", nameof(keys));
            selected.Add(key);
        }
        if (selected.Count == 0) throw new ArgumentException("At least one ticket key is required.", nameof(keys));
        QUIC_TICKET_KEY_CONFIG* native = stackalloc QUIC_TICKET_KEY_CONFIG[selected.Count];
        var storage = new Span<byte>(native, sizeof(QUIC_TICKET_KEY_CONFIG) * selected.Count);
        storage.Clear();
        try
        {
            for (int i = 0; i < selected.Count; i++)
            {
                selected[i].CopyTo(new Span<byte>(native[i].Id, 16), new Span<byte>(native[i].Material, 64));
                native[i].MaterialLength = 64;
                for (int j = 0; j < i; j++)
                    if (new ReadOnlySpan<byte>(native[j].Id, 16).SequenceEqual(new ReadOnlySpan<byte>(native[i].Id, 16)))
                        throw new ArgumentException("Duplicate ticket key identifier.", nameof(keys));
            }
            using var operation = EnterOperation();
            QuicError.ThrowIfFailed(Runtime.Api->SetParam(Handle, MsQuic.QUIC_PARAM_CONFIGURATION_TICKET_KEYS,
                (uint)storage.Length, native), "Set configuration ticket keys");
        }
        finally { CryptographicOperations.ZeroMemory(storage); }
    }

    public override ValueTask DisposeAsync() => new(CloseOnce(CloseAsync));
    private async Task CloseAsync()
    {
        if (credentialCompletion != null) await credentialCompletion.Task.ConfigureAwait(false);
        if (HasNativeHandle) await Runtime.RunCleanup(CloseCore).ConfigureAwait(false);
        else CloseCore();
    }
    private unsafe void CloseCore()
    {
        if (Handle != null) { Runtime.Api->ConfigurationClose(Handle); Handle = null; }
        credential?.Dispose(); credential = null;
        RetireContext();
        if (registered) { registered = false; registration.ConfigurationClosed(this); }
    }
}
