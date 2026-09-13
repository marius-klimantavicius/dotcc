using static Managed.Security.PicoTls;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Managed.Security;

/// <summary>Immutable, shareable TLS 1.3 configuration. Disposal prevents new
/// connections; existing connections retain configuration and credential leases.</summary>
public sealed unsafe class PicotlsContext : IDisposable
{
    public const int MaximumOperationBytes = 8 * 1024 * 1024;
    private readonly object gate = new();
    private st_ptls_context_t* native;
    private st_ptls_get_time_t* clock;
    private st_ptls_save_ticket_t* saveTicket;
    private HelloRegistration* hello;
    private st_ptls_cipher_suite_t** suites;
    private st_ptls_iovec_t* protocols;
    private int protocolCount;
    private IDisposable? identityLease, verifierLease, ticketLease;
    private int connections;
    private bool disposed;
    public bool IsServer { get; }
    internal bool EnforceRetry { get; }
    internal bool NegotiateBeforeKeyExchange { get; }
    internal bool SaveSessionTickets { get; }
    internal st_ptls_iovec_t* Protocols => protocols;
    internal int ProtocolCount => protocolCount;

    [StructLayout(LayoutKind.Sequential)]
    private struct HelloRegistration { public st_ptls_on_client_hello_t Header; public nint Handle; }
    private sealed record HelloPolicy(byte[][] Protocols);
    private static readonly delegate*<void*, ulong, void> RandomPointer = &RandomBytes;
    private static readonly delegate*<st_ptls_get_time_t*, ulong> TimePointer = &TimeMilliseconds;
    private static readonly delegate*<st_ptls_on_client_hello_t*, st_ptls_t*, st_ptls_on_client_hello_parameters_t*, int> HelloPointer = &OnClientHello;

    public PicotlsContext(bool isServer, BclCryptoProvider.CertificateVerifier? verifier,
        BclCryptoProvider.SigningIdentity? identity = null, IEnumerable<string>? applicationProtocols = null,
        ushort? cipherSuite = null, bool requireClientAuthentication = false,
        int maximumHandshakeBuffer = 1024 * 1024, bool enforceRetry = false, bool negotiateBeforeKeyExchange = false,
        BclCryptoProvider.TicketProtector? ticketProtector = null, bool saveSessionTickets = false)
    {
        if (!isServer || requireClientAuthentication) ArgumentNullException.ThrowIfNull(verifier);
        if (isServer) ArgumentNullException.ThrowIfNull(identity);
        if (!isServer && requireClientAuthentication) throw new ArgumentException("Client authentication is a server policy.");
        if (!isServer && ticketProtector != null) throw new ArgumentException("Ticket protection is a server policy.");
        if (isServer && saveSessionTickets) throw new ArgumentException("Saving session tickets is a client policy.");
        if (maximumHandshakeBuffer is < 16384 or > MaximumOperationBytes) throw new ArgumentOutOfRangeException(nameof(maximumHandshakeBuffer));
        if (cipherSuite is not (null or 0x1301 or 0x1302)) throw new ArgumentOutOfRangeException(nameof(cipherSuite));
        IsServer = isServer; EnforceRetry = enforceRetry; NegotiateBeforeKeyExchange = negotiateBeforeKeyExchange;
        SaveSessionTickets = saveSessionTickets;
        try
        {
            BclCryptoProvider.InitializeSymmetric(); BclCryptoProvider.InitializeAsymmetric();
            var encoded = (applicationProtocols ?? ["dotcc-picotls"]).Select(EncodeProtocol).ToArray();
            if (encoded.Length is 0 or > 32) throw new ArgumentException("Specify between one and 32 ALPN protocols.");
            native = (st_ptls_context_t*)Allocate(sizeof(st_ptls_context_t));
            clock = (st_ptls_get_time_t*)Allocate(sizeof(st_ptls_get_time_t));
            clock->cb = TimePointer;
            native->get_time = clock; native->random_bytes = RandomPointer;
            native->key_exchanges = BclCryptoProvider.AsymmetricKeyExchanges;
            if (cipherSuite is { } selected)
            {
                suites = (st_ptls_cipher_suite_t**)Allocate(2 * sizeof(nint));
                var available = BclCryptoProvider.SymmetricCipherSuites;
                suites[0] = available[0]->id == selected ? available[0] : available[1];
                native->cipher_suites = suites;
            }
            else native->cipher_suites = BclCryptoProvider.SymmetricCipherSuites;
            native->max_buffer_size = (ulong)maximumHandshakeBuffer;
            native->use_exporter = 1;
            native->require_dhe_on_psk = 1;
            native->require_client_authentication = requireClientAuthentication ? 1u : 0u;
            // Zero-initialization leaves TLS 1.2, early data, compression and ECH disabled.
            identityLease = identity?.RetainAndApply(native);
            verifierLease = verifier?.RetainAndApply(native);
            ticketLease = ticketProtector?.RetainAndApply(native);
            if (saveSessionTickets)
            {
                saveTicket = (st_ptls_save_ticket_t*)Allocate(sizeof(st_ptls_save_ticket_t));
                saveTicket->cb = PicotlsConnection.SaveTicketPointer;
                native->save_ticket = saveTicket;
            }
            protocols = (st_ptls_iovec_t*)Allocate(encoded.Length * sizeof(st_ptls_iovec_t));
            foreach (byte[] protocol in encoded)
            {
                byte* data = (byte*)Allocate(protocol.Length);
                protocol.CopyTo(new Span<byte>(data, protocol.Length));
                protocols[protocolCount++] = new() { @base = data, len = (ulong)protocol.Length };
            }
            if (isServer)
            {
                hello = (HelloRegistration*)Allocate(sizeof(HelloRegistration));
                hello->Handle = GCHandle.ToIntPtr(GCHandle.Alloc(new HelloPolicy(encoded)));
                hello->Header.cb = HelloPointer;
                native->on_client_hello = &hello->Header;
            }
        }
        catch { Dispose(); throw; }
    }
    private static byte[] EncodeProtocol(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value.Length is 0 or > 255 || value.Any(c => c is '\0' or > '\x7f'))
            throw new ArgumentException("ALPN names must contain 1–255 nonzero ASCII bytes.");
        return Encoding.ASCII.GetBytes(value);
    }
    internal static void* Allocate(int size)
    {
        void* result = Libc.malloc(Math.Max(size, 1));
        if (result == null) throw new OutOfMemoryException();
        new Span<byte>(result, Math.Max(size, 1)).Clear();
        return result;
    }
    internal st_ptls_context_t* Acquire()
    {
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            checked { connections++; }
            return native;
        }
    }
    internal void Release()
    {
        lock (gate) { if (--connections == 0 && disposed) Free(); }
    }
    internal bool OffersProtocol(string value)
    {
        if (value.Any(c => c > '\x7f')) return false;
        byte[] encoded = Encoding.ASCII.GetBytes(value);
        for (int i = 0; i < protocolCount; i++)
            if (protocols[i].len == (ulong)encoded.Length && encoded.AsSpan().SequenceEqual(new ReadOnlySpan<byte>(protocols[i].@base, encoded.Length)))
                return true;
        return false;
    }
    public PicotlsConnection CreateConnection(string? serverName = null, ReadOnlySpan<byte> sessionTicket = default)
        => new(this, serverName, sessionTicket);
    public void Dispose()
    {
        lock (gate)
        {
            if (disposed) return;
            disposed = true;
            if (connections == 0) Free();
        }
        GC.SuppressFinalize(this);
    }
    ~PicotlsContext() { try { Dispose(); } catch { } }
    private void Free()
    {
        Libc.free(native); native = null;
        Libc.free(clock); clock = null;
        Libc.free(saveTicket); saveTicket = null;
        Libc.free(suites); suites = null;
        if (hello != null)
        {
            if (hello->Handle != 0) GCHandle.FromIntPtr(hello->Handle).Free();
            Libc.free(hello); hello = null;
        }
        for (int i = 0; i < protocolCount; i++) Libc.free(protocols[i].@base);
        Libc.free(protocols); protocols = null; protocolCount = 0;
        try { identityLease?.Dispose(); }
        finally
        {
            identityLease = null;
            try { verifierLease?.Dispose(); }
            finally { verifierLease = null; ticketLease?.Dispose(); ticketLease = null; }
        }
    }
    private static void RandomBytes(void* output, ulong length)
    {
        try
        {
            CallbackScope.RequireActive();
            if (CallbackScope.HasFailure) return;
            if (length > MaximumOperationBytes || (output == null && length != 0)) throw new ArgumentOutOfRangeException(nameof(length));
            RandomNumberGenerator.Fill(new Span<byte>(output, (int)length));
        }
        catch (Exception error) { CallbackScope.Capture(error); }
    }
    private static ulong TimeMilliseconds(st_ptls_get_time_t* self)
    {
        try { CallbackScope.RequireActive(); return checked((ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()); }
        catch (Exception error) { CallbackScope.Capture(error); return 0; }
    }
    private static int OnClientHello(st_ptls_on_client_hello_t* self, st_ptls_t* tls, st_ptls_on_client_hello_parameters_t* input)
    {
        try
        {
            CallbackScope.RequireActive();
            if (CallbackScope.HasFailure) return 0x203;
            if (input == null || input->negotiated_protocols.count > 256) return 47;
            if (input->negotiated_protocols.count != 0 && input->negotiated_protocols.list == null) return 47;
            if (input->server_name.len > 255) return 47;
            if (input->server_name.len != 0)
            {
                if (input->server_name.@base == null || new ReadOnlySpan<byte>(input->server_name.@base, (int)input->server_name.len).Contains((byte)0)) return 47;
                int result = PicoTls.ptls_set_server_name(tls, input->server_name.@base, input->server_name.len);
                if (result != 0) return result;
            }
            var policy = (HelloPolicy)GCHandle.FromIntPtr(((HelloRegistration*)self)->Handle).Target!;
            foreach (byte[] protocol in policy.Protocols)
                for (ulong i = 0; i < input->negotiated_protocols.count; i++)
                {
                    var offered = input->negotiated_protocols.list[i];
                    if (offered.len == (ulong)protocol.Length && offered.@base != null &&
                        protocol.AsSpan().SequenceEqual(new ReadOnlySpan<byte>(offered.@base, protocol.Length)))
                        fixed (byte* selected = protocol) return PicoTls.ptls_set_negotiated_protocol(tls, selected, (ulong)protocol.Length);
                }
            return 120; // no_application_protocol; every facade connection requires ALPN.
        }
        catch (Exception error) { CallbackScope.Capture(error); return error is OutOfMemoryException ? 0x201 : 0x203; }
    }
}
