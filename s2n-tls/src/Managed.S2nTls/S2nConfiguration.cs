using System.Runtime.InteropServices;
using Raw = Managed.Security.S2n.S2nTls;

namespace Managed.Security.S2n;

/// <summary>Owns a compatibility config. It becomes immutable on first connection
/// creation; connections retain the config, callback roots and credentials.</summary>
public sealed unsafe class S2nConfiguration : IDisposable
{
    private readonly object _gate = new();
    private readonly List<S2nCredentials> _credentials = [];
    private readonly CallbackState _callbacks = new();
    private GCHandle _callbackRoot;
    private S2nRuntime.Lease? _runtime;
    private nint _handle;
    private int _references = 1;
    private bool _disposed, _frozen;
    internal delegate int ConfigCall(Raw.s2n_config* config);
    private void* CallbackContext => (void*)GCHandle.ToIntPtr(_callbackRoot);

    public S2nConfiguration(S2nRuntime runtime)
    {
        ArgumentNullException.ThrowIfNull(runtime);
        _runtime = runtime.Retain();
        try
        {
            _handle = Native.Require(Raw.s2n_config_new(), "s2n_config_new");
            Native.Check(Raw.dotcc_s2n_config_use_compat((Raw.s2n_config*)_handle), "dotcc_s2n_config_use_compat");
            _callbackRoot = GCHandle.Alloc(_callbacks);
        }
        catch { _disposed = true; Release(); throw; }
    }
    public bool IsFrozen { get { lock (_gate) return _frozen; } }
    private S2nConfiguration Set(ConfigCall operation, string name)
    {
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_frozen) throw new InvalidOperationException("Configuration is frozen after its first connection.");
            Native.Check(operation((Raw.s2n_config*)_handle), name);
            return this;
        }
    }

    public S2nConfiguration AddCredentials(S2nCredentials credentials)
    {
        ArgumentNullException.ThrowIfNull(credentials);
        return Set(p =>
        {
            nint chain = credentials.Retain();
            try
            {
                Native.Check(Raw.s2n_config_add_cert_chain_and_key_to_store(p, (Raw.s2n_cert_chain_and_key*)chain), "s2n_config_add_cert_chain_and_key_to_store");
                _credentials.Add(credentials);
                return 0;
            }
            catch { credentials.Release(); throw; }
        }, "add credentials");
    }
    public S2nConfiguration AddTrustPem(string pem)
    { var bytes = Native.Utf8(pem); return Set(p => { fixed (byte* b = bytes) return Raw.s2n_config_add_pem_to_trust_store(p, b); }, "s2n_config_add_pem_to_trust_store"); }
    public S2nConfiguration ClearTrustStore() => Set(Raw.s2n_config_wipe_trust_store, "s2n_config_wipe_trust_store");
    public S2nConfiguration AddDhParameters(string pem)
    { var bytes = Native.Utf8(pem); return Set(p => { fixed (byte* b = bytes) return Raw.s2n_config_add_dhparams(p, b); }, "s2n_config_add_dhparams"); }
    public S2nConfiguration SetNamedPolicy(string name)
    { var bytes = Native.Utf8(name); return Set(p => { fixed (byte* b = bytes) return Raw.s2n_config_set_cipher_preferences(p, b); }, "s2n_config_set_cipher_preferences"); }
    public S2nConfiguration DisableCertificateVerification() => Set(Raw.s2n_config_disable_x509_verification, "s2n_config_disable_x509_verification");
    public S2nConfiguration DisableCertificateTimeVerification() => Set(Raw.s2n_config_disable_x509_time_verification, "s2n_config_disable_x509_time_verification");
    public S2nConfiguration DisableCertificateIntentVerification() => Set(Raw.s2n_config_disable_x509_intent_verification, "s2n_config_disable_x509_intent_verification");
    public S2nConfiguration AllowIpInCommonName() => Set(Raw.s2n_config_allow_ip_in_cn, "s2n_config_allow_ip_in_cn");
    public S2nConfiguration CheckStapledOcsp(bool enabled) => Set(p => Raw.s2n_config_set_check_stapled_ocsp_response(p, enabled ? (byte)1 : (byte)0), "s2n_config_set_check_stapled_ocsp_response");
    public S2nConfiguration SetMaximumCertificateDepth(ushort depth) => Set(p => Raw.s2n_config_set_max_cert_chain_depth(p, depth), "s2n_config_set_max_cert_chain_depth");
    public S2nConfiguration SetMaximumBlindingDelay(uint seconds) => Set(p => Raw.s2n_config_set_max_blinding_delay(p, seconds), "s2n_config_set_max_blinding_delay");
    public S2nConfiguration EnableLegacyDssCertificateRequest() => Set(Raw.s2n_config_enable_cert_req_dss_legacy_compat, "s2n_config_enable_cert_req_dss_legacy_compat");
    public S2nConfiguration SetClientAuthentication(S2nClientAuthentication mode)
    { if (!Enum.IsDefined(mode)) throw new ArgumentOutOfRangeException(nameof(mode)); return Set(p => Raw.s2n_config_set_client_auth_type(p, (Raw.s2n_cert_auth_type)(int)mode), "s2n_config_set_client_auth_type"); }
    public S2nConfiguration IgnoreWarningAlerts(bool ignore) => Set(p => Raw.s2n_config_set_alert_behavior(p, (Raw.s2n_alert_behavior)(ignore ? 1 : 0)), "s2n_config_set_alert_behavior");
    public S2nConfiguration VerifyAfterSigning(bool verify) => Set(p => Raw.s2n_config_set_verify_after_sign(p, (Raw.s2n_verify_after_sign)(verify ? 1 : 0)), "s2n_config_set_verify_after_sign");
    public S2nConfiguration EnableNpn(bool enabled) => Set(p => Raw.s2n_config_set_npn(p, enabled), "s2n_config_set_npn");
    public S2nConfiguration EnableSerialization(bool enabled) => Set(p => Raw.s2n_config_set_serialization_version(p, (Raw.s2n_serialization_version)(enabled ? 1 : 0)), "s2n_config_set_serialization_version");
    public S2nConfiguration EnableSessionTickets(bool enabled) => Set(p => Raw.s2n_config_set_session_tickets_onoff(p, enabled ? (byte)1 : (byte)0), "s2n_config_set_session_tickets_onoff");
    public S2nConfiguration SetSessionLifetime(ulong seconds) => Set(p => Raw.s2n_config_set_session_state_lifetime(p, seconds), "s2n_config_set_session_state_lifetime");
    public S2nConfiguration AddSessionTicketKey(ReadOnlySpan<byte> name, ReadOnlySpan<byte> key, DateTimeOffset introduced)
    {
        long seconds = introduced.ToUnixTimeSeconds();
        if (seconds < 0) throw new ArgumentOutOfRangeException(nameof(introduced));
        byte[] nameCopy = name.ToArray(), keyCopy = key.ToArray();
        try
        {
            return Set(p =>
            {
                fixed (byte* n = nameCopy, k = keyCopy)
                    return Raw.s2n_config_add_ticket_crypto_key(p, n, checked((uint)nameCopy.Length), k,
                        checked((uint)keyCopy.Length), (ulong)seconds);
            }, "s2n_config_add_ticket_crypto_key");
        }
        finally { System.Security.Cryptography.CryptographicOperations.ZeroMemory(keyCopy); }
    }
    public S2nConfiguration SetHostnameVerifier(Func<string, bool> verifier)
    { ArgumentNullException.ThrowIfNull(verifier); return Set(p => { int r = Raw.s2n_config_set_verify_host_callback(p, &Callbacks.VerifyHost, CallbackContext); if (r == 0) _callbacks.VerifyHost = verifier; return r; }, "s2n_config_set_verify_host_callback"); }
    public S2nConfiguration SetKeyLogger(Action<ReadOnlyMemory<byte>> logger)
    { ArgumentNullException.ThrowIfNull(logger); return Set(p => { int r = Raw.s2n_config_set_key_log_cb(p, &Callbacks.KeyLog, CallbackContext); if (r == 0) _callbacks.KeyLog = logger; return r; }, "s2n_config_set_key_log_cb"); }
    public S2nConfiguration SetCertificateValidator(Func<CancellationToken, ValueTask<bool>> validator)
    { ArgumentNullException.ThrowIfNull(validator); return Set(p => { int r = Raw.s2n_config_set_cert_validation_cb(p, &Callbacks.ValidateCertificate, CallbackContext); if (r == 0) _callbacks.ValidateCertificate = validator; return r; }, "s2n_config_set_cert_validation_cb"); }
    public S2nConfiguration SetRenegotiationHandler(Func<S2nRenegotiation> handler)
    { ArgumentNullException.ThrowIfNull(handler); return Set(p => { int r = Raw.s2n_config_set_renegotiate_request_cb(p, &Callbacks.Renegotiate, CallbackContext); if (r == 0) _callbacks.Renegotiate = handler; return r; }, "s2n_config_set_renegotiate_request_cb"); }
    public S2nConfiguration SetApplicationProtocols(params string[] protocols)
    {
        ArgumentNullException.ThrowIfNull(protocols);
        // Upstream copies each protocol during this call. Pins never escape it.
        byte[][] strings = protocols.Select(Native.Utf8).ToArray();
        return Set(p =>
        {
            GCHandle[] pins = new GCHandle[strings.Length];
            try
            {
                byte** pointers = stackalloc byte*[strings.Length];
                for (int i = 0; i < strings.Length; i++) { pins[i] = GCHandle.Alloc(strings[i], GCHandleType.Pinned); pointers[i] = (byte*)pins[i].AddrOfPinnedObject(); }
                return Raw.s2n_config_set_protocol_preferences(p, pointers, strings.Length);
            }
            finally { foreach (var pin in pins) if (pin.IsAllocated) pin.Free(); }
        }, "s2n_config_set_protocol_preferences");
    }

    internal nint Retain()
    {
        lock (_gate)
        { ObjectDisposedException.ThrowIf(_disposed, this); _frozen = true; _references++; return _handle; }
    }
    internal void Release()
    {
        lock (_gate)
        {
            if (--_references != 0) return;
            try
            {
                if (_handle != 0) Native.Check(Raw.s2n_config_free((Raw.s2n_config*)_handle), "s2n_config_free");
                _handle = 0;
                if (_callbackRoot.IsAllocated) _callbackRoot.Free();
                _callbacks.VerifyHost = null; _callbacks.KeyLog = null;
                _callbacks.ValidateCertificate = null; _callbacks.Renegotiate = null;
                foreach (var credentials in _credentials) credentials.Release();
                _credentials.Clear(); _runtime?.Dispose(); _runtime = null;
            }
            catch (Exception error) { S2nRuntime.Quarantine(error); throw; }
        }
    }
    public void Dispose()
    {
        lock (_gate) { if (_disposed) return; _disposed = true; }
        Release(); GC.SuppressFinalize(this);
    }
    ~S2nConfiguration() { try { Dispose(); } catch (Exception error) { S2nRuntime.Quarantine(error); } }
}
