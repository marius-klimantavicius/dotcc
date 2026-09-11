using System.Security.Cryptography;
using Managed.Security;

internal static partial class Program
{
    // Direct ports of the pinned t/picotls.c private-core tests. These calls
    // reference the actual translated definitions; no layout mirrors/adapters.
    private static void AdditionalUpstreamPorts()
    {
        UpstreamCipherPreferences();
        UpstreamFragmentedMessages();
        UpstreamLegacyClientHellos();
        UpstreamGreaseResumption();
    }

    private static unsafe void UpstreamCipherPreferences()
    {
        var provider = BclCryptoProvider.SymmetricCipherSuites;
        st_ptls_cipher_suite_t* aes128 = null, aes256 = null;
        for (int i = 0; provider[i] != null; i++)
        {
            if (provider[i]->id == 0x1301) aes128 = provider[i];
            else if (provider[i]->id == 0x1302) aes256 = provider[i];
            else throw new Exception("initial provider advertised an unexpected cipher");
        }
        Check(aes128 != null && aes256 != null, "cipher preference uses both actual AES suite tables");
        nint* candidateStorage = stackalloc nint[3];
        var candidates = (st_ptls_cipher_suite_t**)candidateStorage;
        candidates[2] = null;
        // Selected-profile counterpart of upstream client/server preference
        // cases, in both orders. The ChaCha priority flag must have no effect
        // when the server has only its two advertised AES suites.
        foreach (bool reverseServer in new[] { false, true })
        foreach (bool reverseClient in new[] { false, true })
        foreach (int preferServer in new[] { 0, 1 })
        foreach (int chachaPriority in new[] { 0, 1 })
        {
            candidates[0] = reverseServer ? aes256 : aes128;
            candidates[1] = reverseServer ? aes128 : aes256;
            byte[] offer = reverseClient ? [0x13, 2, 0x13, 1] : [0x13, 1, 0x13, 2];
            ushort expected = preferServer != 0
                ? (reverseServer ? (ushort)0x1302 : (ushort)0x1301)
                : (reverseClient ? (ushort)0x1302 : (ushort)0x1301);
            CheckCipherChoice(candidates, offer, preferServer, chachaPriority, expected);
        }
        candidates[0] = aes256; candidates[1] = aes128;
        foreach (int preferServer in new[] { 0, 1 })
        foreach (int chachaPriority in new[] { 0, 1 })
            CheckCipherChoice(candidates, [0x13, 3, 0x13, 1, 0x13, 2], preferServer, chachaPriority,
                preferServer != 0 ? (ushort)0x1302 : (ushort)0x1301);
        CheckCipherChoice(candidates, [], 0, 0, null);
        candidates[0] = aes128; candidates[1] = null;
        CheckCipherChoice(candidates, [0x13, 2], 0, 0, null);
        CheckCipherChoice(candidates, [0x13, 2, 0x13, 1], 0, 0, 0x1301);
        CheckCipherChoice(candidates, [0x13, 2, 0x13, 1], 1, 0, 0x1301);
    }

    private static unsafe void CheckCipherChoice(st_ptls_cipher_suite_t** candidates, byte[] offered,
        int preferServer, int chachaPriority, ushort? expected)
    {
        byte[] storage = offered.Length == 0 ? new byte[1] : offered;
        fixed (byte* start = storage)
        {
            st_ptls_cipher_suite_t* selected = null;
            int result = Picotls.select_cipher(&selected, candidates, start, start + offered.Length,
                preferServer, chachaPriority, null);
            Check(result == (expected.HasValue ? 0 : 40), "select_cipher exact status");
            if (expected.HasValue)
                Check(selected != null && selected->id == expected.Value, "select_cipher exact preferred suite");
        }
    }

    [ThreadStatic] private static List<(byte[] Bytes, bool EndOfRecord)>? upstreamFragments;
    private static readonly unsafe delegate*<st_ptls_t*, st_ptls_message_emitter_t*, st_ptls_iovec_t, int,
        st_ptls_handshake_properties_t*, int> UpstreamFragmentPointer = &CaptureUpstreamFragment;
    private static unsafe int CaptureUpstreamFragment(st_ptls_t* tls, st_ptls_message_emitter_t* emitter,
        st_ptls_iovec_t message, int endOfRecord, st_ptls_handshake_properties_t* properties)
    {
        try
        {
            CallbackScope.RequireActive();
            upstreamFragments!.Add((new ReadOnlySpan<byte>(message.@base, checked((int)message.len)).ToArray(), endOfRecord != 0));
            return 0;
        }
        catch (Exception error) { CallbackScope.Capture(error); return 0x203; }
    }

    private static unsafe void UpstreamFragmentedMessages()
    {
        using var scope = CallbackScope.Enter();
        st_ptls_context_t context = new() { max_buffer_size = 14 };
        st_ptls_t tls = new() { ctx = &context };
        upstreamFragments = [];
        try
        {
            Check(FeedUpstreamFragment(&tls, [1, 0, 0, 3, (byte)'a', (byte)'b', (byte)'c']) == 0, "unfragmented message status");
            scope.ThrowIfFailed();
            Check(upstreamFragments.Count == 1, "one complete message callback");
            CheckFragment(0, [1, 0, 0, 3, (byte)'a', (byte)'b', (byte)'c'], true);
            Check(tls.recvbuf.mess.@base == null, "complete message needs no retained buffer");
            upstreamFragments.Clear();
            Check(FeedUpstreamFragment(&tls, [1, 0, 0, 3, (byte)'a']) == 0x202, "first partial message waits");
            scope.ThrowIfFailed();
            Check(tls.recvbuf.mess.@base != null && upstreamFragments.Count == 0, "partial message buffered without callback");
            Check(FeedUpstreamFragment(&tls, [(byte)'b', (byte)'c', 2, 0, 0, 2, (byte)'d', (byte)'e', 3]) == 0x202, "coalesced messages retain final header fragment");
            scope.ThrowIfFailed();
            Check(upstreamFragments.Count == 2, "two coalesced message callbacks");
            CheckFragment(0, [1, 0, 0, 3, (byte)'a', (byte)'b', (byte)'c'], false);
            CheckFragment(1, [2, 0, 0, 2, (byte)'d', (byte)'e'], false);
            Check(FeedUpstreamFragment(&tls, [0, 0, 3, (byte)'e', (byte)'n', (byte)'d']) == 0, "final fragment completes pending message");
            scope.ThrowIfFailed();
            Check(upstreamFragments.Count == 3 && tls.recvbuf.mess.@base == null, "all completed messages release receive buffer");
            CheckFragment(2, [3, 0, 0, 3, (byte)'e', (byte)'n', (byte)'d'], true);
            upstreamFragments.Clear();
            Check(FeedUpstreamFragment(&tls, [1, 0, 0, 255, .. "0123456789ab"u8.ToArray()]) == 40, "post-callback overflow exact handshake_failure");
            scope.ThrowIfFailed(); Check(upstreamFragments.Count == 0, "overflow invokes no message callback");
            Check(FeedUpstreamFragment(&tls, [1, 0, 0, 255, .. "0123456789"u8.ToArray()]) == 0x202, "exact buffer limit can wait for another record");
            Check(FeedUpstreamFragment(&tls, "abcdef"u8.ToArray()) == 40, "pre-callback accumulated overflow exact handshake_failure");
            scope.ThrowIfFailed(); Check(upstreamFragments.Count == 0, "accumulated overflow invokes no callback");
        }
        finally
        {
            Picotls.dotcc_ptls_buffer_dispose(&tls.recvbuf.mess);
            upstreamFragments = null;
        }
        scope.ThrowIfFailed();
    }

    private static unsafe int FeedUpstreamFragment(st_ptls_t* tls, byte[] bytes)
    {
        fixed (byte* data = bytes)
        {
            st_ptls_record_t record = new() { type = 22, version = 0x0301, length = (ulong)bytes.Length, fragment = data };
            return Picotls.handle_handshake_record(tls, UpstreamFragmentPointer, null, &record, null);
        }
    }
    private static void CheckFragment(int index, byte[] expected, bool endOfRecord)
    {
        var actual = upstreamFragments![index];
        Check(actual.Bytes.AsSpan().SequenceEqual(expected), "exact reassembled upstream handshake bytes");
        Check(actual.EndOfRecord == endOfRecord, "exact upstream end-of-record callback flag");
    }

    private sealed record LegacyHello(byte[] Raw, byte[] ServerName, bool ServerNameWasNull, bool Incompatible);
    [ThreadStatic] private static LegacyHello? upstreamLegacyHello;
    [ThreadStatic] private static int upstreamLegacyCallbacks;
    private static readonly unsafe delegate*<st_ptls_on_client_hello_t*, st_ptls_t*, st_ptls_on_client_hello_parameters_t*, int>
        UpstreamLegacyPointer = &CaptureUpstreamLegacy;
    private static unsafe int CaptureUpstreamLegacy(st_ptls_on_client_hello_t* self, st_ptls_t* tls,
        st_ptls_on_client_hello_parameters_t* parameters)
    {
        try
        {
            CallbackScope.RequireActive();
            upstreamLegacyCallbacks++;
            upstreamLegacyHello = new(
                new ReadOnlySpan<byte>(parameters->raw_message.@base, checked((int)parameters->raw_message.len)).ToArray(),
                new ReadOnlySpan<byte>(parameters->server_name.@base, checked((int)parameters->server_name.len)).ToArray(),
                parameters->server_name.@base == null, parameters->incompatible_version != 0);
            return 0;
        }
        catch (Exception error) { CallbackScope.Capture(error); return 0x203; }
    }

    private static unsafe void UpstreamLegacyClientHellos()
    {
        using var scope = CallbackScope.Enter();
        st_ptls_get_time_t time = new() { cb = TestTimePointer };
        st_ptls_on_client_hello_t listener = new() { cb = UpstreamLegacyPointer };
        st_ptls_context_t context = new() { random_bytes = TestRandomPointer, get_time = &time,
            cipher_suites = BclCryptoProvider.SymmetricCipherSuites, key_exchanges = BclCryptoProvider.AsymmetricKeyExchanges,
            on_client_hello = &listener };
        foreach (var (name, encoded) in UpstreamLegacyPackets)
        {
            byte[] packet = Convert.FromHexString(encoded);
            upstreamLegacyHello = null; upstreamLegacyCallbacks = 0;
            st_ptls_t* server = Picotls.ptls_server_new(&context);
            st_ptls_buffer_t outgoing = PicotlsBuffer.Create();
            try
            {
                scope.ThrowIfFailed(); Check(server != null, "legacy raw server allocation");
                ulong consumed = (ulong)packet.Length;
                fixed (byte* source = packet)
                    Check(Picotls.ptls_handshake(server, &outgoing, source, &consumed, null) == (name == "ssl2" ? 50 : 70),
                        "pinned " + name + " exact rejection alert");
                scope.ThrowIfFailed();
                if (name == "ssl2")
                    Check(upstreamLegacyCallbacks == 0 && upstreamLegacyHello == null, "SSL2 is rejected before legacy callback");
                else if (name != "tls12_no_exts") // Upstream only asserts protocol_version for this packet.
                {
                    var observed = upstreamLegacyHello;
                    Check(upstreamLegacyCallbacks == 1 && observed != null && observed.Incompatible, "legacy callback sees incompatible version");
                    Check(observed!.Raw.AsSpan().SequenceEqual(packet.AsSpan(5)), "legacy callback exact raw handshake bytes");
                    if (name == "tls12")
                        Check(observed.ServerName.AsSpan().SequenceEqual("i_need_sni"u8), "legacy callback SNI metadata");
                    else Check(observed.ServerName.Length == 0 && observed.ServerNameWasNull, "legacy callback absent SNI metadata");
                }
            }
            finally
            {
                if (server != null) Picotls.ptls_free(server);
                Picotls.dotcc_ptls_buffer_dispose(&outgoing);
                upstreamLegacyHello = null;
            }
            scope.ThrowIfFailed();
        }
    }

    [ThreadStatic] private static byte[]? upstreamGreaseTicket;
    private static readonly unsafe delegate*<st_ptls_save_ticket_t*, st_ptls_t*, st_ptls_iovec_t, st_ptls_save_ticket_properties_t*, int>
        UpstreamGreaseTicketPointer = &CaptureUpstreamGreaseTicket;
    private static unsafe int CaptureUpstreamGreaseTicket(st_ptls_save_ticket_t* self, st_ptls_t* tls,
        st_ptls_iovec_t ticket, st_ptls_save_ticket_properties_t* properties)
    {
        try
        {
            CallbackScope.RequireActive();
            byte[] saved = new ReadOnlySpan<byte>(ticket.@base, checked((int)ticket.len)).ToArray();
            if (upstreamGreaseTicket != null) CryptographicOperations.ZeroMemory(upstreamGreaseTicket);
            upstreamGreaseTicket = saved;
            return 0;
        }
        catch (Exception error) { CallbackScope.Capture(error); return 0x203; }
    }

    private static unsafe void UpstreamGreaseResumption()
    {
        using var signer = Identity("server-ecdsa");
        using var verifier = Verifier();
        using var protector = new BclCryptoProvider.TicketProtector(TimeSpan.FromMinutes(5));
        using var serverContext = new PicotlsContext(true, null, signer, ticketProtector: protector);
        using var scope = CallbackScope.Enter();
        Check(BclCryptoProvider.AsymmetricKeyExchanges[0]->id == 23 && BclCryptoProvider.AsymmetricKeyExchanges[1] == null,
            "GREASE test leaves the real provider P256-only");
        // The pinned GREASE path reads the X25519 KEM identifier and emits a
        // random 32-byte encapsulation; it never performs key exchange. Every
        // operation on this test-only descriptor is deliberately null. This is
        // GREASE-format/fallback coverage, not an X25519 or real ECH provider.
        st_ptls_key_exchange_algorithm_t greaseGroup = new() { id = 29 };
        st_ptls_hpke_kem_t greaseKem = new() { id = 32, keyex = &greaseGroup, hash = BclCryptoProvider.HashSha256 };
        st_ptls_hpke_cipher_suite_t greaseCipher = new()
        {
            id = new() { kdf = 1, aead = 1 }, hash = BclCryptoProvider.HashSha256, aead = BclCryptoProvider.Aes128Gcm
        };
        nint* kemStorage = stackalloc nint[2]; kemStorage[0] = (nint)(&greaseKem); kemStorage[1] = 0;
        nint* cipherStorage = stackalloc nint[2]; cipherStorage[0] = (nint)(&greaseCipher); cipherStorage[1] = 0;
        st_ptls_get_time_t time = new() { cb = TestTimePointer };
        st_ptls_save_ticket_t save = new() { cb = UpstreamGreaseTicketPointer };
        st_ptls_context_t clientContext = new()
        {
            random_bytes = TestRandomPointer, get_time = &time, save_ticket = &save,
            cipher_suites = BclCryptoProvider.SymmetricCipherSuites,
            key_exchanges = BclCryptoProvider.AsymmetricKeyExchanges, require_dhe_on_psk = 1
        };
        clientContext.ech.client.kems = (st_ptls_hpke_kem_t**)kemStorage;
        clientContext.ech.client.ciphers = (st_ptls_hpke_cipher_suite_t**)cipherStorage;
        verifier.ApplyTo(&clientContext);
        byte[]? ticket = null, replacement = null;
        try
        {
            ticket = RunUpstreamGreaseSession(&clientContext, serverContext, [], false);
            Check(ticket.Length != 0, "GREASE full handshake captures a real ticket");
            replacement = RunUpstreamGreaseSession(&clientContext, serverContext, ticket, true);
        }
        finally
        {
            if (ticket != null) CryptographicOperations.ZeroMemory(ticket);
            if (replacement != null) CryptographicOperations.ZeroMemory(replacement);
            if (upstreamGreaseTicket != null) CryptographicOperations.ZeroMemory(upstreamGreaseTicket);
            upstreamGreaseTicket = null;
        }
        scope.ThrowIfFailed();
    }

    private static unsafe byte[] RunUpstreamGreaseSession(st_ptls_context_t* context, PicotlsContext serverContext,
        byte[] ticket, bool resumed)
    {
        using var scope = CallbackScope.Enter();
        using var server = serverContext.CreateConnection();
        st_ptls_t* client = Picotls.ptls_client_new(context);
        st_ptls_handshake_properties_t properties = default;
        st_ptls_iovec_t alpn = new() { @base = Libc.L("dotcc-picotls\0"u8), len = (ulong)"dotcc-picotls"u8.Length };
        try
        {
            scope.ThrowIfFailed(); Check(client != null, "GREASE raw client allocation");
            Check(Picotls.ptls_set_server_name(client, Libc.L("localhost\0"u8), 9) == 0, "GREASE authenticated hostname");
            fixed (byte* ticketBytes = ticket)
            {
                Picotls.dotcc_ptls_client_properties(&properties, &alpn, 1,
                    new st_ptls_iovec_t { @base = ticketBytes, len = (ulong)ticket.Length }, 0);
                properties.__anon___Anon16.client.ech.configs = new() { @base = Libc.L("\0"u8), len = 0 };
                byte[] hello = StepUpstreamGreaseClient(client, &properties, [], true);
                _ = RewriteClientExtensions(hello, extensions =>
                    Check(extensions.Any(extension => extension.Type == 0xfe0d), "GREASE extension actually emitted in ClientHello"));
                var pending = new Queue<(bool ToServer, byte[] Bytes)>();
                pending.Enqueue((true, hello));
                int budget = 100;
                while (pending.TryDequeue(out var packet))
                {
                    Check(--budget > 0, "bounded GREASE handshake pump");
                    byte[] output;
                    if (packet.ToServer)
                    {
                        var step = server.Process(packet.Bytes);
                        Check(step.Consumed == packet.Bytes.Length && step.Plaintext.Length == 0, "GREASE server consumes handshake without early data");
                        output = step.Outbound;
                    }
                    else output = StepUpstreamGreaseClient(client, &properties, packet.Bytes, false);
                    if (output.Length != 0) pending.Enqueue((!packet.ToServer, output));
                }
            }
            Check(Picotls.ptls_handshake_is_complete(client) != 0 && server.HandshakeComplete, "GREASE handshake completes");
            Check((Picotls.ptls_is_psk_handshake(client) != 0) == resumed && server.IsResumed == resumed, "GREASE full or PSK-DHE resumed status");
            Check(Picotls.ptls_is_ech_handshake(client, null, null, null) == 0, "GREASE falls back without real ECH acceptance");
            st_ptls_buffer_t application = PicotlsBuffer.Create();
            try
            {
                fixed (byte* bytes = "GREASE resumed application"u8)
                    Check(Picotls.ptls_send(client, &application, bytes, (ulong)"GREASE resumed application"u8.Length) == 0,
                        "GREASE authenticated application send");
                scope.ThrowIfFailed();
                var accepted = server.Process(new ReadOnlySpan<byte>(application.@base, checked((int)application.off)));
                Check(accepted.Plaintext.AsSpan().SequenceEqual("GREASE resumed application"u8), "GREASE application payload survives fallback and resumption");
            }
            finally { Picotls.dotcc_ptls_buffer_dispose(&application); }
            byte[] saved = upstreamGreaseTicket ?? [];
            upstreamGreaseTicket = null;
            return saved;
        }
        finally { if (client != null) Picotls.ptls_free(client); }
    }

    private static unsafe byte[] StepUpstreamGreaseClient(st_ptls_t* client, st_ptls_handshake_properties_t* properties,
        byte[] input, bool initial)
    {
        using var scope = CallbackScope.Enter();
        st_ptls_buffer_t outgoing = PicotlsBuffer.Create(), plaintext = PicotlsBuffer.Create();
        try
        {
            fixed (byte* source = input)
            {
                int offset = 0;
                do
                {
                    ulong consumed = initial ? 0 : (ulong)(input.Length - offset);
                    int status = Picotls.ptls_handshake_is_complete(client) == 0
                        ? Picotls.ptls_handshake(client, &outgoing, initial ? null : source + offset, &consumed, properties)
                        : Picotls.ptls_receive(client, &plaintext, source + offset, &consumed);
                    scope.ThrowIfFailed();
                    Check(status is 0 or 0x202, "GREASE raw client handshake status");
                    Check(consumed <= (ulong)(input.Length - offset), "GREASE raw input consumption bounded");
                    if (initial) break;
                    Check(consumed != 0, "GREASE raw client makes progress");
                    offset += checked((int)consumed);
                } while (offset < input.Length);
            }
            Check(plaintext.off == 0, "GREASE handshake and ticket messages emit no plaintext");
            return new ReadOnlySpan<byte>(outgoing.@base, checked((int)outgoing.off)).ToArray();
        }
        finally
        {
            Picotls.dotcc_ptls_buffer_dispose(&outgoing);
            Picotls.dotcc_ptls_buffer_dispose(&plaintext);
        }
    }

    // Exact byte arrays from test_legacy_ch at the pinned upstream revision.
    private static readonly (string Name, string Hex)[] UpstreamLegacyPackets =
    [
        ("tls12", "16030100d2010000ce0303d1010e39ea2228899942ec70fab34701ce618dee0e3ef7e94f0a8e9428e5e3d300005cc030c02cc028c024c014c00a009f006b0039cca9cca8ccaaff8500c400880081009d003d003500c00084c02fc02bc027c023c013c009009e0067003300be0045009c003c002f00ba0041c011c00700050004c012c0080016000a00ff010000490000000f000d00000a695f6e6565645f736e69000b00020100000a00080006001d0017001800230000000d001c001a06010603efef0501050304010403eeeeeded0301030302010203"),
        ("tls11", "16030100710100006d0302a5acfcef36a04e1ba19d01983eae072e23dcce62c8b67ed05c2eeb632674e76100002ec014c00a0039ff850088008100350084c013c00900330045002f0041c011c00700050004c012c0080016000a00ff01000016000b00020100000a00080006001d0017001800230000"),
        ("tls10_no_exts", "160301004b010000470301000000000000000000000000000000000000000000000000000000000000000000002000040005002f00330032000a00160013000900150012000300080014001100ff0100"),
        ("tls12_no_exts", "1603010067010000630303000000000000000000000000000000000000000000000000000000000000000020ee8a29ddcf6d64fdd0cda09bc13246bf53da2923815f541fbde08e97175b035d001c00ff009cc02bc02fc00ac009c013c014002f0035009e00330039000a0100"),
        ("tls10_with_exts", "160301002f0100002b030163fc5a16de7afcc10c5412a6d38ccfdad3cc5019425cb081f6e2f94b06716838000004000900ff0100"),
        ("ssl2", "808001030100570000002000001600001300000a0700c0000066000007000005000004050080030080010080080080000065000064000063000062000061000060000015000012000009060040000014000011000008000006000003040080020080fbb80c85ac46333970408e3905929fa66d34d39123afbe51507232bbda8cf435"),
    ];
}
