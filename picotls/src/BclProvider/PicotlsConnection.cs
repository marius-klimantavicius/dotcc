using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;

namespace Managed.Security;

public sealed class PicotlsException(int errorCode, byte[] alertBytes) : AuthenticationException($"picotls failed with error 0x{errorCode:x}.")
{
    public int ErrorCode { get; } = errorCode;
    /// <summary>Core-produced fatal alert bytes, suitable for forwarding to the peer.</summary>
    public ReadOnlyMemory<byte> AlertBytes { get; } = alertBytes;
}

public sealed record PicotlsStep(int Consumed, byte[] Outbound, byte[] Plaintext, bool HandshakeComplete, bool PeerClosed);

/// <summary>Owns one translated TLS connection. Operations are serialized and
/// synchronous; move the resulting bytes using any BCL stream or socket. EOF
/// must be reported with CompleteInput to detect missing close_notify.</summary>
public sealed unsafe partial class PicotlsConnection : IDisposable
{
    private readonly object gate = new();
    private PicotlsContext? context;
    private st_ptls_t* native;
    private st_ptls_handshake_properties_t* properties;
    private byte* ticket;
    private int ticketLength;
    private bool started, complete, peerClosed, localClosed, disposed;
    private ushort cipher;
    private string? protocol, serverName;
    private bool resumed;
    public bool HandshakeComplete { get { lock (gate) return complete; } }
    public bool PeerClosed { get { lock (gate) return peerClosed; } }
    public ushort CipherSuite { get { lock (gate) { RequireHandshake(); return cipher; } } }
    public string NegotiatedProtocol { get { lock (gate) { RequireHandshake(); return protocol!; } } }
    public string? ServerName { get { lock (gate) { RequireHandshake(); return serverName; } } }
    public bool IsResumed { get { lock (gate) { RequireHandshake(); return resumed; } } }

    internal PicotlsConnection(PicotlsContext configuration, string? endpointName, ReadOnlySpan<byte> sessionTicket)
    {
        if (!configuration.IsServer && (string.IsNullOrEmpty(endpointName) || endpointName.Length > 255 || endpointName.Contains('\0')))
            throw new ArgumentException("Clients require a DNS or IP endpoint name of at most 255 characters.", nameof(endpointName));
        if (configuration.IsServer && !sessionTicket.IsEmpty) throw new ArgumentException("Session tickets are client input.", nameof(sessionTicket));
        if (sessionTicket.Length > 64 * 1024) throw new ArgumentOutOfRangeException(nameof(sessionTicket));
        using var scope = CallbackScope.Enter();
        try
        {
            var rawContext = configuration.Acquire(); context = configuration;
            properties = (st_ptls_handshake_properties_t*)PicotlsContext.Allocate(sizeof(st_ptls_handshake_properties_t));
            if (configuration.IsServer) Picotls.dotcc_ptls_server_properties(properties, configuration.EnforceRetry ? 1 : 0);
            else
            {
                if (!sessionTicket.IsEmpty)
                {
                    ticket = (byte*)PicotlsContext.Allocate(sessionTicket.Length); ticketLength = sessionTicket.Length;
                    sessionTicket.CopyTo(new Span<byte>(ticket, ticketLength));
                }
                Picotls.dotcc_ptls_client_properties(properties, configuration.Protocols, (ulong)configuration.ProtocolCount,
                    new st_ptls_iovec_t { @base = ticket, len = (ulong)ticketLength }, configuration.NegotiateBeforeKeyExchange ? 1 : 0);
            }
            native = configuration.IsServer ? Picotls.ptls_server_new(rawContext) : Picotls.ptls_client_new(rawContext);
            scope.ThrowIfFailed();
            if (native == null) throw new OutOfMemoryException("picotls could not allocate a connection.");
            if (configuration.SaveSessionTickets) InitializeSavedTickets();
            if (!configuration.IsServer)
            {
                // DNS is converted to the wire form once; IP strings remain unchanged.
                string name = System.Net.IPAddress.TryParse(endpointName, out _) ? endpointName! : new System.Globalization.IdnMapping().GetAscii(endpointName!);
                byte[] bytes = Encoding.UTF8.GetBytes(name);
                if (bytes.Length > 255) throw new ArgumentException("Encoded endpoint name exceeds 255 bytes.", nameof(endpointName));
                fixed (byte* data = bytes) Check(Picotls.ptls_set_server_name(native, data, (ulong)bytes.Length));
            }
            scope.ThrowIfFailed();
        }
        catch (Exception error) { Abort(error); throw; }
    }
    /// <summary>Starts a client handshake when called with empty input, or
    /// processes peer bytes. Consumed bytes are retained by the core; callers
    /// must not replay them. Returned plaintext is authenticated.</summary>
    public PicotlsStep Process(ReadOnlySpan<byte> input)
    {
        if (input.Length > PicotlsContext.MaximumOperationBytes) throw new ArgumentOutOfRangeException(nameof(input));
        lock (gate)
        {
            RequireLive();
            if (peerClosed && !input.IsEmpty) throw new InvalidOperationException("The peer has already closed its TLS write side.");
            using var scope = CallbackScope.Enter();
            st_ptls_buffer_t outgoing = PicotlsBuffer.Create(), plaintext = PicotlsBuffer.Create();
            try
            {
                int consumed = 0;
                bool startClient = !started && !context!.IsServer;
                fixed (byte* source = input)
                {
                    while (consumed < input.Length || startClient)
                    {
                        bool initialCall = startClient;
                        ulong count = initialCall ? 0 : (ulong)(input.Length - consumed);
                        int result;
                        if (!complete)
                        {
                            // Upstream distinguishes initial client entry by a null input.
                            result = Picotls.ptls_handshake(native, &outgoing, startClient ? null : source + consumed,
                                &count, properties);
                            if (startClient) count = 0;
                            started = true; startClient = false;
                        }
                        else result = Picotls.ptls_receive(native, &plaintext, source + consumed, &count);
                        scope.ThrowIfFailed();
                        if (count > (ulong)(input.Length - consumed)) throw new InvalidDataException("Core consumed more input than supplied.");
                        consumed += (int)count;
                        if (result == 0x100)
                        {
                            if (!complete) throw new PicotlsException(result, CopyBuffer(outgoing));
                            peerClosed = true;
                            if (consumed != input.Length) throw new InvalidDataException("Input follows close_notify.");
                            break;
                        }
                        if (result is not (0 or 0x202)) throw new PicotlsException(result, CopyBuffer(outgoing));
                        if (!complete && Picotls.ptls_handshake_is_complete(native) != 0) FinishHandshake();
                        scope.ThrowIfFailed();
                        if (count == 0 && consumed < input.Length && !initialCall)
                            throw new InvalidDataException("TLS processing made no progress.");
                        if (outgoing.off > PicotlsContext.MaximumOperationBytes || plaintext.off > PicotlsContext.MaximumOperationBytes)
                            throw new InvalidDataException("TLS operation exceeds its output limit.");
                    }
                }
                scope.ThrowIfFailed();
                return new(consumed, CopyBuffer(outgoing), CopyBuffer(plaintext), complete, peerClosed);
            }
            catch (Exception error) { Abort(error); throw; }
            finally { ReleaseBuffer(ref outgoing); ReleaseBuffer(ref plaintext); }
        }
    }
    public byte[] Send(ReadOnlySpan<byte> plaintext)
    {
        if (plaintext.Length > PicotlsContext.MaximumOperationBytes - 65536) throw new ArgumentOutOfRangeException(nameof(plaintext));
        lock (gate)
        {
            RequireHandshake();
            if (localClosed) throw new InvalidOperationException("The TLS write side is closed.");
            using var scope = CallbackScope.Enter();
            st_ptls_buffer_t output = PicotlsBuffer.Create();
            try
            {
                fixed (byte* data = plaintext) Check(Picotls.ptls_send(native, &output, data, (ulong)plaintext.Length));
                scope.ThrowIfFailed();
                return CopyBuffer(output);
            }
            catch (Exception error) { Abort(error); throw; }
            finally { ReleaseBuffer(ref output); }
        }
    }
    /// <summary>Updates the sending key and emits the update immediately.</summary>
    public byte[] UpdateKey(bool requestPeerUpdate = true)
    {
        lock (gate)
        {
            RequireHandshake();
            if (localClosed) throw new InvalidOperationException("The TLS write side is closed.");
            using var scope = CallbackScope.Enter();
            try { Check(Picotls.ptls_update_key(native, requestPeerUpdate ? 1 : 0)); scope.ThrowIfFailed(); return Send([]); }
            catch (Exception error) { Abort(error); throw; }
        }
    }
    public byte[] ExportSecret(string label, ReadOnlySpan<byte> exporterContext, int length)
    {
        ArgumentNullException.ThrowIfNull(label);
        // TLS HKDFLabel reserves six bytes for "tls13 " in its uint8 vector.
        if (label.Contains('\0') || Encoding.UTF8.GetByteCount(label) > 249 || length is < 1 or > 4096 || exporterContext.Length > 65536)
            throw new ArgumentOutOfRangeException(nameof(label));
        lock (gate)
        {
            RequireHandshake();
            using var scope = CallbackScope.Enter();
            byte[] output = new byte[length], encodedLabel = Encoding.UTF8.GetBytes(label + "\0");
            try
            {
                fixed (byte* target = output, encoded = encodedLabel, value = exporterContext)
                    Check(Picotls.ptls_export_secret(native, target, (ulong)length, encoded,
                        new st_ptls_iovec_t { @base = value, len = (ulong)exporterContext.Length }, 0));
                scope.ThrowIfFailed(); return output;
            }
            catch (Exception error) { CryptographicOperations.ZeroMemory(output); Abort(error); throw; }
        }
    }
    public byte[] CloseNotify()
    {
        lock (gate)
        {
            RequireHandshake();
            if (localClosed) return [];
            using var scope = CallbackScope.Enter();
            st_ptls_buffer_t output = PicotlsBuffer.Create();
            try
            {
                Check(Picotls.ptls_send_alert(native, &output, 1, 0));
                scope.ThrowIfFailed(); localClosed = true; return CopyBuffer(output);
            }
            catch (Exception error) { Abort(error); throw; }
            finally { ReleaseBuffer(ref output); }
        }
    }
    public void CompleteInput()
    {
        lock (gate)
        {
            RequireLive();
            if (!peerClosed)
            {
                var error = new EndOfStreamException("TLS transport ended without close_notify.");
                Abort(error); throw error;
            }
        }
    }
    private void FinishHandshake()
    {
        if (Picotls.ptls_get_protocol_version(native) != 0x0304)
            throw new AuthenticationException("The peer did not negotiate TLS 1.3.");
        cipher = Picotls.ptls_get_cipher(native)->id;
        protocol = ReadCString(Picotls.ptls_get_negotiated_protocol(native));
        if (string.IsNullOrEmpty(protocol) || !context!.OffersProtocol(protocol))
            throw new AuthenticationException("The peer did not select an offered ALPN protocol.");
        serverName = ReadCString(Picotls.ptls_get_server_name(native));
        resumed = Picotls.ptls_is_psk_handshake(native) != 0;
        complete = true;
    }
    private static string? ReadCString(byte* value)
    {
        if (value == null) return null;
        int count = 0;
        while (count < 256 && value[count] != 0) count++;
        if (count == 256) throw new InvalidDataException("Core string exceeds the protocol limit.");
        return Encoding.UTF8.GetString(new ReadOnlySpan<byte>(value, count));
    }
    private static byte[] CopyBuffer(st_ptls_buffer_t buffer)
    {
        if (buffer.off > PicotlsContext.MaximumOperationBytes || buffer.off > buffer.capacity || (buffer.@base == null && buffer.off != 0))
            throw new InvalidDataException("Invalid core output buffer.");
        return new ReadOnlySpan<byte>(buffer.@base, (int)buffer.off).ToArray();
    }
    private static void ReleaseBuffer(ref st_ptls_buffer_t buffer)
    {
        fixed (st_ptls_buffer_t* value = &buffer) Picotls.ptls_buffer__release_memory(value);
        buffer = default;
    }
    private static void Check(int result)
    {
        using var scope = CallbackScope.Enter();
        scope.ThrowIfFailed();
        if (result != 0) throw new PicotlsException(result, []);
    }
    private void RequireLive() => ObjectDisposedException.ThrowIf(disposed, this);
    private void RequireHandshake() { RequireLive(); if (!complete) throw new InvalidOperationException("The TLS handshake is incomplete."); }
    private void Abort(Exception primary)
    {
        disposed = true;
        try { Free(); }
        catch (Exception cleanup)
        {
            // Retain the primary authentication/transport/provider exception.
            // Cleanup failures are diagnostic and must not alter its classification.
            if (!ReferenceEquals(primary, cleanup))
                try { primary.Data["PicotlsCleanupFailure"] = cleanup; } catch { }
        }
    }
    private void Free()
    {
        using var scope = CallbackScope.Enter();
        try
        {
            var previous = native; native = null;
            if (previous != null) Picotls.ptls_free(previous);
        }
        finally
        {
            ReleaseSavedTickets();
            if (ticket != null) { new Span<byte>(ticket, ticketLength).Clear(); Libc.free(ticket); ticket = null; ticketLength = 0; }
            Libc.free(properties); properties = null;
            var previousContext = context; context = null; previousContext?.Release();
        }
        scope.ThrowIfFailed();
    }
    public void Dispose()
    {
        lock (gate)
        {
            if (disposed) return;
            disposed = true; Free();
        }
        GC.SuppressFinalize(this);
    }
    ~PicotlsConnection() { try { Dispose(); } catch { } }
}
