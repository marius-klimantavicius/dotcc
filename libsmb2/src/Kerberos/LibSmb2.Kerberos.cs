using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Concurrent;
using Kerberos.NET;
using Kerberos.NET.Client;
using Kerberos.NET.Credentials;
using Kerberos.NET.Crypto;
using Kerberos.NET.Entities;

namespace Managed.Smb;

public static partial class LibSmb2
{
    private static readonly ConcurrentDictionary<nint, PreparedKerberosAuthentication> PreparedAuthentication = new();

    public sealed class PreparedKerberosAuthentication : IDisposable
    {
        private KerberosAuthState? _state;
        internal PreparedKerberosAuthentication(KerberosAuthState state) => _state = state;
        internal KerberosAuthState Take() => Interlocked.Exchange(ref _state, null)
            ?? throw new InvalidOperationException("Kerberos authentication was already consumed");
        public void Dispose() => Interlocked.Exchange(ref _state, null)?.Dispose();
    }

    internal sealed unsafe class KerberosAuthState : IDisposable
    {
        public KerberosClient? Client { get; }
        public ApplicationSessionContext? Session { get; }
        public WindowsKerberosContext? Sspi { get; }
        public byte[] SessionKey { get; set; }
        public bool Failed { get; set; }
        public bool Authenticated { get; set; }
        public byte* OutputToken { get; private set; }
        public int OutputTokenLength { get; private set; }

        public KerberosAuthState(KerberosClient client, ApplicationSessionContext session)
        {
            Client = client;
            Session = session;
            SessionKey = session.SessionKey.KeyValue.ToArray();
        }

        public KerberosAuthState(WindowsKerberosContext sspi)
        {
            Sspi = sspi;
            SessionKey = [];
        }

        public void SetOutputToken(ReadOnlyMemory<byte> token)
        {
            ClearOutputToken();
            if (token.IsEmpty) return;
            OutputToken = (byte*)NativeMemory.Alloc((nuint)token.Length);
            token.Span.CopyTo(new Span<byte>(OutputToken, token.Length));
            OutputTokenLength = token.Length;
        }

        public void ClearOutputToken()
        {
            if (OutputToken != null)
            {
                CryptographicOperations.ZeroMemory(new Span<byte>(OutputToken, OutputTokenLength));
                NativeMemory.Free(OutputToken);
            }
            OutputToken = null;
            OutputTokenLength = 0;
        }

        public void Dispose()
        {
            ClearOutputToken();
            CryptographicOperations.ZeroMemory(SessionKey);
            Client?.Dispose();
            Sspi?.Dispose();
        }
    }

    // Acquire KDC tickets before entering the translated synchronous callback seam.
    // Kerberos.NET Authenticate has no cancellation overload: cancellation is observed
    // after it settles so its client and credentials are never disposed mid-request.
    public static async Task<PreparedKerberosAuthentication> PrepareKerberosAsync(
        string host, string user, string? password = null, string? realm = null,
        string? keytabPath = null, string? credentialCachePath = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        KerberosClient? client = null;
        KerberosAuthState? state = null;
        try
        {
            if (password == null && keytabPath == null && credentialCachePath == null && OperatingSystem.IsWindows())
            {
                state = new KerberosAuthState(new WindowsKerberosContext("cifs/" + host));
                state.SetOutputToken(state.Sspi!.RequestToken());
            }
            else
            {
                client = new KerberosClient();
                if (credentialCachePath != null)
                {
                    client.CacheInMemory = false;
                    client.Configuration.Defaults.DefaultCCacheName = FileCredentialCache(credentialCachePath);
                }
                else if (keytabPath != null)
                {
                    byte[] bytes = await File.ReadAllBytesAsync(keytabPath, cancellationToken).ConfigureAwait(false);
                    try { await client.Authenticate(new KeytabCredential(user, new KeyTable(bytes), realm)).ConfigureAwait(false); }
                    finally { CryptographicOperations.ZeroMemory(bytes); }
                }
                else if (password != null)
                    await client.Authenticate(new KerberosPasswordCredential(user, password, realm)).ConfigureAwait(false);
                else ConfigureExistingCredentials(client);
                cancellationToken.ThrowIfCancellationRequested();
                var session = await client.GetServiceTicket(new RequestServiceTicket
                {
                    ServicePrincipalName = "cifs/" + host,
                    ApOptions = ApOptions.MutualRequired,
                    GssContextFlags = GssContextEstablishmentFlag.GSS_C_MUTUAL_FLAG |
                        GssContextEstablishmentFlag.GSS_C_REPLAY_FLAG |
                        GssContextEstablishmentFlag.GSS_C_SEQUENCE_FLAG |
                        GssContextEstablishmentFlag.GSS_C_INTEG_FLAG |
                        GssContextEstablishmentFlag.GSS_C_CONF_FLAG
                }, cancellationToken).ConfigureAwait(false);
                state = new KerberosAuthState(client, session);
                client = null;
                state.SetOutputToken(GssApiToken.Encode(new Oid(MechType.KerberosGssApi), session.ApReq));
            }
            cancellationToken.ThrowIfCancellationRequested();
            return new PreparedKerberosAuthentication(state);
        }
        catch { state?.Dispose(); client?.Dispose(); throw; }
    }

    internal static string FileCredentialCache(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (path.StartsWith("FILE:", StringComparison.OrdinalIgnoreCase)) path = path[5..];
        if (!Path.IsPathFullyQualified(path))
            throw new NotSupportedException("A Kerberos FILE cache must use an absolute path; other cache types are unsupported");
        return "FILE:" + path;
    }

    public static void RegisterPreparedKerberos(nint context, PreparedKerberosAuthentication prepared)
    {
        if (context == 0) throw new ArgumentOutOfRangeException(nameof(context));
        if (!PreparedAuthentication.TryAdd(context, prepared)) throw new InvalidOperationException("Kerberos context already registered");
    }
    public static void UnregisterPreparedKerberos(nint context) => PreparedAuthentication.TryRemove(context, out _);

    public static unsafe private_auth_data* krb5_negotiate_reply(smb2_context* smb2,
        byte* server, byte* domain, byte* userName, byte* password)
    {
        try
        {
            if (!PreparedAuthentication.TryRemove((nint)smb2, out var prepared))
                throw new InvalidOperationException("Prepare Kerberos credentials asynchronously before connecting");
            return AllocateAuthState(prepared.Take());
        }
        catch (Exception error) { SetKerberosError(smb2, error); return null; }
    }

    public static unsafe int krb5_session_request(smb2_context* smb2,
        private_auth_data* authData, byte* buffer, int length)
    {
        try
        {
            KerberosAuthState state = GetKerberosState(authData);
            if (state.Sspi != null && buffer != null && length > 0)
            {
                state.SetOutputToken(state.Sspi.RequestToken(
                    new ReadOnlySpan<byte>(buffer, length).ToArray()));
                byte[] sessionKey = state.Sspi.IsAuthenticated ? state.Sspi.SessionKey : [];
                if (sessionKey.Length != 0)
                {
                    CryptographicOperations.ZeroMemory(state.SessionKey);
                    state.SessionKey = sessionKey;
                }
            }
            else if (state.Session != null && buffer != null && length > 0)
            {
                ReadOnlyMemory<byte> response = UnwrapKerberosResponse(new ReadOnlyMemory<byte>(
                    new ReadOnlySpan<byte>(buffer, length).ToArray()));
                byte[] sessionKey = state.Session.AuthenticateServiceResponse(response).KeyValue.ToArray();
                CryptographicOperations.ZeroMemory(state.SessionKey);
                state.SessionKey = sessionKey;
                state.ClearOutputToken();
            }
            if (buffer != null && length > 0) state.Authenticated = state.Sspi?.IsAuthenticated ?? true;
            authData->output_token.length = (ulong)state.OutputTokenLength;
            authData->output_token.value = state.OutputToken;
            return 0;
        }
        catch (Exception error)
        {
            if (authData != null && authData->context != null) GetKerberosState(authData).Failed = true;
            SetKerberosError(smb2, error);
            return -1;
        }
    }

    public static unsafe int krb5_session_get_session_key(smb2_context* smb2,
        private_auth_data* authData)
    {
        try
        {
            KerberosAuthState state = GetKerberosState(authData);
            if (state.Failed || !state.Authenticated) throw new InvalidOperationException("Kerberos mutual authentication did not complete");
            byte[] key = state.SessionKey;
            if (key.Length == 0 && state.Sspi != null) key = state.Sspi.SessionKey;
            if (key.Length < SMB2_KEY_SIZE || key.Length > byte.MaxValue) throw new InvalidOperationException("Kerberos returned an invalid session key");
            if (smb2->session_key != null) Libc.free(smb2->session_key);
            smb2->session_key = (byte*)Libc.malloc(key.Length);
            if (smb2->session_key == null) throw new OutOfMemoryException();
            key.CopyTo(new Span<byte>(smb2->session_key, key.Length));
            smb2->session_key_size = (byte)key.Length;
            return 0;
        }
        catch (Exception error)
        {
            SetKerberosError(smb2, error);
            // Upstream ignores some provider errors when encryption disables sign.
            // The managed service turn catches this and destroys/drains the context.
            throw new System.Security.Authentication.AuthenticationException("Kerberos session key is unavailable", error);
        }
    }

    public static unsafe byte* krb5_get_output_token_buffer(private_auth_data* authData)
        => authData == null ? null : (byte*)authData->output_token.value;

    public static unsafe int krb5_get_output_token_length(private_auth_data* authData)
        => authData == null ? 0 : checked((int)authData->output_token.length);

    public static unsafe void krb5_free_auth_data(private_auth_data* authData)
    {
        if (authData == null) return;
        try
        {
            if (authData->context != null)
            {
                var handle = GCHandle.FromIntPtr((nint)authData->context);
                try { if (handle.Target is KerberosAuthState state) state.Dispose(); }
                finally { handle.Free(); }
            }
        }
        finally { NativeMemory.Free(authData); }
    }

    public static unsafe uint gss_release_cred(uint* minorStatus, void** credential)
    {
        if (minorStatus != null) *minorStatus = 0;
        if (credential != null) *credential = null;
        return 0;
    }

    public static unsafe int krb5_can_do_ntlmssp() => 0;

    public static unsafe private_auth_data* krb5_init_server_client_cred(smb2_server* server,
        smb2_context* smb2, byte* password)
    {
        SetKerberosError(smb2, new NotSupportedException("The managed Kerberos provider supports SMB clients only"));
        return null;
    }

    public static unsafe int krb5_session_reply(smb2_context* smb2, private_auth_data* authData,
        byte* buffer, int length, int* moreProcessingNeeded)
    {
        if (moreProcessingNeeded != null) *moreProcessingNeeded = 0;
        SetKerberosError(smb2, new NotSupportedException("The managed Kerberos provider supports SMB clients only"));
        return -1;
    }

    public static unsafe int krb5_init_server_credentials(smb2_server* server, byte* keytabPath) => -1;
    public static unsafe int krb5_renew_server_credentials(smb2_server* server) => -1;
    public static unsafe void krb5_free_server_credentials(smb2_server* server) { }

    private static unsafe KerberosAuthState GetKerberosState(private_auth_data* authData)
    {
        if (authData == null || authData->context == null) throw new InvalidOperationException("Kerberos context is unavailable");
        var handle = GCHandle.FromIntPtr((nint)authData->context);
        return handle.Target as KerberosAuthState ?? throw new InvalidOperationException("Kerberos context is invalid");
    }

    private static unsafe private_auth_data* AllocateAuthState(KerberosAuthState state)
    {
        var auth = (private_auth_data*)NativeMemory.AllocZeroed((nuint)sizeof(private_auth_data));
        if (auth == null)
        {
            state.Dispose();
            throw new OutOfMemoryException("Failed to allocate the Kerberos authentication context");
        }
        try
        {
            var handle = GCHandle.Alloc(state);
            auth->context = (void*)GCHandle.ToIntPtr(handle);
            auth->output_token.length = (ulong)state.OutputTokenLength;
            auth->output_token.value = state.OutputToken;
            return auth;
        }
        catch
        {
            state.Dispose();
            NativeMemory.Free(auth);
            throw;
        }
    }

    private static unsafe void ConfigureExistingCredentials(KerberosClient client)
    {
        client.CacheInMemory = false;
        string? cache = Environment.GetEnvironmentVariable("KRB5CCNAME");
        if (!string.IsNullOrWhiteSpace(cache))
        {
            client.Configuration.Defaults.DefaultCCacheName = FileCredentialCache(cache);
            return;
        }
        throw new InvalidOperationException("KRB5CCNAME must identify a FILE credential cache when no password is supplied");
    }

    private static unsafe ReadOnlyMemory<byte> UnwrapKerberosResponse(ReadOnlyMemory<byte> token)
    {
        if (token.IsEmpty) throw new InvalidOperationException("The SMB server returned an empty Kerberos response");
        if (token.Span[0] == 0xa1)
        {
            NegotiationToken negotiation = NegotiationToken.Decode(token);
            token = negotiation.ResponseToken?.ResponseToken ?? ReadOnlyMemory<byte>.Empty;
        }
        if (!token.IsEmpty && token.Span[0] == 0x60)
        {
            GssApiToken outer = GssApiToken.Decode(token);
            token = outer.Token;
            if (outer.ThisMech?.Value == MechType.SPNEGO)
            {
                NegotiationToken negotiation = NegotiationToken.Decode(token);
                token = negotiation.ResponseToken?.ResponseToken ?? ReadOnlyMemory<byte>.Empty;
            }
        }
        if (!token.IsEmpty && token.Span[0] == 0x60) token = GssApiToken.Decode(token).Token;
        if (token.IsEmpty) throw new InvalidOperationException("The SMB server returned no Kerberos AP-REP token");
        return token;
    }

    private static unsafe void SetKerberosError(smb2_context* smb2, Exception error)
    {
        if (smb2 == null) return;
        byte[] message = Encoding.UTF8.GetBytes("Kerberos: " + error.Message);
        int length = Math.Min(message.Length, 255);
        byte* destination = smb2->error_string;
        message.AsSpan(0, length).CopyTo(new Span<byte>(destination, 256));
        destination[length] = 0;
        smb2->nterror = 0;
        if (smb2->error_cb != null) smb2->error_cb(smb2, smb2->error_string);
    }
}
