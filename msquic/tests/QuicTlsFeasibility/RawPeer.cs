using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Managed.Security;

// Feasibility adapter around actual translated picotls. Owns only its raw
// configuration/registration memory; signer/verifier owners outlive this peer.
internal sealed unsafe class RawPeer : IDisposable
{
    private const ushort TransportParametersExtension = 0x39;
    private readonly List<nint> allocations = [];
    private readonly string protocol;
    private GCHandle handle;
    private st_ptls_context_t* context;
    private st_ptls_handshake_properties_t* properties;
    private st_ptls_t* tls;
    private bool disposed;
    internal readonly byte[] Parameters;
    internal byte[] ReceivedParameters = [];
    internal int SavedTickets;
    internal readonly Dictionary<(ulong Epoch, bool Encryption), byte[]> Secrets = [];
    public bool Complete => tls != null && Picotls.ptls_handshake_is_complete(tls) != 0;
    public ulong ReadEpoch => Picotls.ptls_get_read_epoch(tls);
    public ushort Cipher => Picotls.ptls_get_cipher(tls)->id;
    public string Protocol => Marshal.PtrToStringUTF8((nint)Picotls.ptls_get_negotiated_protocol(tls)) ?? "";
    public string ServerName => Marshal.PtrToStringUTF8((nint)Picotls.ptls_get_server_name(tls)) ?? "";

    public RawPeer(bool server, ushort suite, BclCryptoProvider.CertificateVerifier? verifier,
        BclCryptoProvider.SigningIdentity? identity, string protocol, string serverName, bool retry,
        BclCryptoProvider.TicketProtector? tickets = null)
    {
        this.protocol = protocol;
        // Valid varint-encoded TP tuples, exchanged opaquely by picotls. The
        // MsQuic core must validate connection-specific values in the real PAL.
        Parameters = server ? [0x0f, 4, 5, 6, 7, 8, 0, 4, 9, 10, 11, 12] : [0x0f, 4, 1, 2, 3, 4];
        using var scope = CallbackScope.Enter();
        try
        {
            BclCryptoProvider.InitializeSymmetric();
            BclCryptoProvider.InitializeAsymmetric();
            context = (st_ptls_context_t*)Allocate(sizeof(st_ptls_context_t));
            properties = (st_ptls_handshake_properties_t*)Allocate(sizeof(st_ptls_handshake_properties_t));
            context->random_bytes = &RandomBytes;
            context->get_time = (st_ptls_get_time_t*)Allocate(sizeof(st_ptls_get_time_t));
            context->get_time->cb = &TimeMilliseconds;
            context->key_exchanges = BclCryptoProvider.AsymmetricKeyExchanges;
            var suites = (st_ptls_cipher_suite_t**)Allocate(2 * sizeof(nint));
            var available = BclCryptoProvider.SymmetricCipherSuites;
            suites[0] = available[0]->id == suite ? available[0] : available[1];
            Program.Require(suites[0]->id == suite, "unsupported requested TLS suite");
            context->cipher_suites = suites;
            context->max_buffer_size = 1024 * 1024;
            context->require_dhe_on_psk = 1;
            context->use_exporter = 1;
            context->omit_end_of_early_data = 1;
            context->send_change_cipher_spec = 0;
            context->max_early_data_size = 0;
            context->update_traffic_key = (st_ptls_update_traffic_key_t*)Allocate(sizeof(st_ptls_update_traffic_key_t));
            context->update_traffic_key->cb = &TrafficKey;
            identity?.ApplyTo(context);
            verifier?.ApplyTo(context);
            tickets?.ApplyTo(context);
            if (server)
            {
                context->on_client_hello = (st_ptls_on_client_hello_t*)Allocate(sizeof(st_ptls_on_client_hello_t));
                context->on_client_hello->cb = &ClientHello;
                Picotls.dotcc_ptls_server_properties(properties, retry ? 1 : 0);
            }
            else
            {
                context->save_ticket = (st_ptls_save_ticket_t*)Allocate(sizeof(st_ptls_save_ticket_t));
                context->save_ticket->cb = &SaveTicket;
                var protocols = (st_ptls_iovec_t*)Allocate(sizeof(st_ptls_iovec_t));
                byte[] encoded = Encoding.ASCII.GetBytes(protocol);
                protocols[0] = new() { @base = Copy(encoded), len = (ulong)encoded.Length };
                Picotls.dotcc_ptls_client_properties(properties, protocols, 1, default, 0);
            }
            var extensions = (st_ptls_raw_extension_t*)Allocate(2 * sizeof(st_ptls_raw_extension_t));
            extensions[0] = new() { type = TransportParametersExtension,
                data = new() { @base = Copy(Parameters), len = (ulong)Parameters.Length } };
            extensions[1].type = ushort.MaxValue;
            properties->additional_extensions = extensions;
            properties->collect_extension = &CollectExtension;
            properties->collected_extensions = &CollectedExtensions;
            tls = server ? Picotls.ptls_server_new(context) : Picotls.ptls_client_new(context);
            scope.ThrowIfFailed();
            if (tls == null) throw new OutOfMemoryException("translated picotls context");
            handle = GCHandle.Alloc(this);
            *Picotls.ptls_get_data_ptr(tls) = (void*)GCHandle.ToIntPtr(handle);
            if (!server)
            {
                byte[] name = Encoding.ASCII.GetBytes(serverName);
                fixed (byte* data = name)
                    Program.Require(Picotls.ptls_set_server_name(tls, data, (ulong)name.Length) == 0, "set SNI");
            }
            scope.ThrowIfFailed();
        }
        catch { Dispose(); throw; }
    }

    public RawStep Process(ulong epoch, ReadOnlySpan<byte> input, bool startClient = false)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        using var scope = CallbackScope.Enter();
        st_ptls_buffer_t output = PicotlsBuffer.Create();
        ulong* offsets = stackalloc ulong[5];
        new Span<ulong>(offsets, 5).Clear();
        try
        {
            int code;
            fixed (byte* bytes = input)
                code = Picotls.ptls_handle_message(tls, &output, offsets, epoch,
                    startClient ? null : bytes, (ulong)input.Length, properties);
            scope.ThrowIfFailed();
            Program.Require(output.off <= 1024 * 1024, "bounded raw TLS output");
            var flights = new List<RawFlight>();
            Program.Require(offsets[0] == 0 && offsets[4] == output.off, "epoch output boundaries");
            for (int i = 0; i < 4; i++)
            {
                Program.Require(offsets[i] <= offsets[i + 1] && offsets[i + 1] <= output.off, "monotonic epoch output offsets");
                if (offsets[i] == offsets[i + 1]) continue;
                flights.Add(new((ulong)i, new ReadOnlySpan<byte>(output.@base + offsets[i], checked((int)(offsets[i + 1] - offsets[i]))).ToArray()));
            }
            return new(code, flights);
        }
        finally { Picotls.dotcc_ptls_buffer_dispose(&output); }
    }

    private void* Allocate(int size)
    {
        void* pointer = Libc.malloc(size);
        if (pointer == null) throw new OutOfMemoryException();
        new Span<byte>(pointer, size).Clear();
        allocations.Add((nint)pointer);
        return pointer;
    }
    private byte* Copy(byte[] bytes)
    {
        byte* pointer = (byte*)Allocate(Math.Max(1, bytes.Length));
        bytes.CopyTo(new Span<byte>(pointer, bytes.Length));
        return pointer;
    }
    private static RawPeer Peer(st_ptls_t* tls)
    {
        var token = (nint)(*Picotls.ptls_get_data_ptr(tls));
        return (RawPeer)GCHandle.FromIntPtr(token).Target!;
    }
    private static void RandomBytes(void* output, ulong length)
    {
        try
        {
            CallbackScope.RequireActive();
            Program.Require(length <= 1024 * 1024, "bounded random request");
            RandomNumberGenerator.Fill(new Span<byte>(output, checked((int)length)));
        }
        catch (Exception error) { CallbackScope.Capture(error); }
    }
    private static ulong TimeMilliseconds(st_ptls_get_time_t* self)
    {
        try { CallbackScope.RequireActive(); return checked((ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()); }
        catch (Exception error) { CallbackScope.Capture(error); return 0; }
    }
    private static int ClientHello(st_ptls_on_client_hello_t* self, st_ptls_t* tls, st_ptls_on_client_hello_parameters_t* input)
    {
        try
        {
            CallbackScope.RequireActive();
            if (input->server_name.len != 0)
            {
                int status = Picotls.ptls_set_server_name(tls, input->server_name.@base, input->server_name.len);
                if (status != 0) return status;
            }
            byte[] protocol = Encoding.ASCII.GetBytes(Peer(tls).protocol);
            for (ulong i = 0; i < input->negotiated_protocols.count; ++i)
            {
                var offered = input->negotiated_protocols.list[i];
                if (offered.len == (ulong)protocol.Length && new ReadOnlySpan<byte>(offered.@base, protocol.Length).SequenceEqual(protocol))
                    fixed (byte* selected = protocol)
                        return Picotls.ptls_set_negotiated_protocol(tls, selected, (ulong)protocol.Length);
            }
            return 120;
        }
        catch (Exception error) { CallbackScope.Capture(error); return 0x203; }
    }
    private static int TrafficKey(st_ptls_update_traffic_key_t* self, st_ptls_t* tls, int encryption, ulong epoch, void* secret)
    {
        try
        {
            CallbackScope.RequireActive();
            Program.Require(epoch is 2 or 3, "unexpected TLS traffic key epoch");
            int length = checked((int)Picotls.ptls_get_cipher(tls)->hash->digest_size);
            Program.Require(length is 32 or 48, "unexpected traffic secret size");
            var peer = Peer(tls);
            var key = (epoch, encryption != 0);
            Program.Require(!peer.Secrets.ContainsKey(key), "unexpected repeat traffic secret");
            peer.Secrets.Add(key, new ReadOnlySpan<byte>(secret, length).ToArray());
            return 0;
        }
        catch (Exception error) { CallbackScope.Capture(error); return 0x203; }
    }
    private static int CollectExtension(st_ptls_t* tls, st_ptls_handshake_properties_t* properties, ushort type)
        => type == TransportParametersExtension ? 1 : 0;
    private static int SaveTicket(st_ptls_save_ticket_t* self, st_ptls_t* tls,
        st_ptls_iovec_t ticket, st_ptls_save_ticket_properties_t* properties)
    {
        try
        {
            CallbackScope.RequireActive();
            Program.Require(ticket.len is > 0 and <= 65535, "saved ticket size");
            Program.Require(properties->early_data == 0 && properties->max_early_data_size == 0, "0-RTT advertised by ticket");
            Peer(tls).SavedTickets++;
            return 0;
        }
        catch (Exception error) { CallbackScope.Capture(error); return 0x203; }
    }
    private static int CollectedExtensions(st_ptls_t* tls, st_ptls_handshake_properties_t* properties, st_ptls_raw_extension_t* extensions)
    {
        try
        {
            CallbackScope.RequireActive();
            for (int i = 0; i < 32 && extensions[i].type != ushort.MaxValue; i++)
            {
                if (extensions[i].type != TransportParametersExtension) continue;
                Program.Require(extensions[i].data.len <= 65535, "transport parameter extension size");
                Peer(tls).ReceivedParameters = new ReadOnlySpan<byte>(extensions[i].data.@base, checked((int)extensions[i].data.len)).ToArray();
            }
            return 0;
        }
        catch (Exception error) { CallbackScope.Capture(error); return 0x203; }
    }
    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        using var scope = CallbackScope.Enter();
        try { if (tls != null) { Picotls.ptls_free(tls); tls = null; } }
        finally
        {
            if (handle.IsAllocated) handle.Free();
            for (int i = allocations.Count - 1; i >= 0; --i) Libc.free((void*)allocations[i]);
            allocations.Clear();
            foreach (byte[] secret in Secrets.Values) CryptographicOperations.ZeroMemory(secret);
            Secrets.Clear();
        }
        scope.ThrowIfFailed();
    }
}
