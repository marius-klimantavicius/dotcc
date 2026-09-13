using static Managed.Security.PicoTls;
using System.Buffers.Binary;
using Managed.Security;

internal static partial class Program
{
    private static void UpstreamHandshakePorts()
    {
        HrrRepeatedGroup(); HrrCipherMismatch(); SendBeforeApplicationKeys();
        foreach (string identity in new[] { "server-ecdsa", "server-rsa" })
            SignatureListOverflow(identity);
        foreach (string mutation in new[] { "duplicate", "disallowed", "legacy" })
        {
            using var pair = new Pair();
            byte[] hello = pair.Client.Process([]).Outbound;
            byte[] changed = RewriteClientExtensions(hello, extensions =>
            {
                if (mutation == "duplicate") extensions.Add(extensions.Single(e => e.Type == 43));
                else if (mutation == "disallowed") extensions.Add((0xfd00, []));
                else
                {
                    int index = extensions.FindIndex(e => e.Type == 43);
                    Check(index >= 0, "supported_versions present in real CH");
                    extensions[index] = (43, [2, 3, 3]); // TLS 1.2 only, structurally valid.
                }
            });
            ExpectCoreAlert(() => pair.Server.Process(changed), mutation == "legacy" ? 70 : 47, "CH " + mutation);
        }
    }
    private static void ExpectCoreAlert(Action action, int alert, string name)
    {
        try { action(); }
        catch (PicotlsException error)
        { Check(error.ErrorCode == alert, name + " exact core alert"); return; }
        throw new Exception(name + " accepted invalid handshake");
    }
    private static void HrrRepeatedGroup()
    {
        // Exact pinned test_hrr_selected_group_matches_ch1_key_share packet.
        byte[] hrr = Convert.FromHexString("1603030038020000340303cf21ad74e59a6111be1d8c021e65b891c2a211167abb8c5e079e09e2c8a8339c00130100000c002b00020304003300020017");
        using var verifier = Verifier();
        using var context = new PicotlsContext(false, verifier, cipherSuite: 0x1301);
        using var client = context.CreateConnection("localhost");
        Check(client.Process([]).Outbound.Length > 0, "HRR group port initial CH");
        ExpectCoreAlert(() => client.Process(hrr), 47, "HRR repeats original P256 key share");
    }
    private static void HrrCipherMismatch()
    {
        using var verifier = Verifier();
        using var identity = Identity("server-ecdsa");
        // Offer both hashes, select SHA384 in HRR, then rewrite final SH to the
        // still-offered SHA256 suite: failure must precede key/hash switching.
        using var clientContext = new PicotlsContext(false, verifier, negotiateBeforeKeyExchange: true);
        using var serverContext = new PicotlsContext(true, null, identity, cipherSuite: 0x1302);
        using var client = clientContext.CreateConnection("localhost");
        using var server = serverContext.CreateConnection();
        var first = client.Process([]);
        var retry = server.Process(first.Outbound);
        Check(!retry.HandshakeComplete, "server emits HRR");
        int offset = ServerHelloCipherOffset(retry.Outbound);
        Check(Read16(retry.Outbound, offset) == 0x1302, "HRR selected SHA384");
        var second = client.Process(retry.Outbound);
        var final = server.Process(second.Outbound);
        offset = ServerHelloCipherOffset(final.Outbound);
        Check(Read16(final.Outbound, offset) == 0x1302, "final SH selected SHA384");
        byte[] changed = final.Outbound.ToArray(); changed[offset] = 0x13; changed[offset + 1] = 1;
        ExpectCoreAlert(() => client.Process(changed), 47, "HRR final SH cipher mismatch");
    }
    private static int ServerHelloCipherOffset(byte[] record)
    {
        Check(record.Length > 44 && record[0] == 22 && record[5] == 2, "complete first SH record");
        int offset = 44 + record[43];
        Check(offset + 2 <= record.Length, "SH cipher location bounded");
        return offset;
    }
    private static void SignatureListOverflow(string identity)
    {
        // All 35 pinned Erlang/OTP regression schemes. P256 and RSA-PSS-RSAE
        // appear after the old 16-entry parser limit.
        ushort[] schemes = [0x0907, 0x0906, 0x0905, 0x090e, 0x090d, 0x090c, 0x090b, 0x090a, 0x0909,
            0x0908, 0x0904, 0x0903, 0x0902, 0x0901, 0x0900, 0x0807, 0x0808, 0x0603, 0x0503,
            0x0403, 0x081b, 0x081a, 0x0819, 0x080b, 0x080a, 0x0809, 0x0806, 0x0805, 0x0804,
            0x0601, 0x0501, 0x0401, 0x0603, 0x0503, 0x0403];
        byte[] extension = new byte[2 + schemes.Length * 2];
        Write16(extension, 0, extension.Length - 2);
        for (int i = 0; i < schemes.Length; i++) Write16(extension, 2 + 2 * i, schemes[i]);
        using var pair = new Pair(serverIdentity: identity);
        byte[] hello = pair.Client.Process([]).Outbound;
        byte[] changed = RewriteClientExtensions(hello, extensions =>
        {
            int index = extensions.FindIndex(e => e.Type == 13);
            Check(index >= 0, "real CH signature_algorithms exists"); extensions[index] = (13, extension);
        });
        var response = pair.Server.Process(changed);
        // The client transcript intentionally differs after rewriting. Only the
        // server's parse/sign flight is tested here; don't fake a full exchange.
        Check(response.Consumed == changed.Length && response.Outbound.Length > 0, "35 signature schemes accepted and signed: " + identity);
    }
    private static unsafe void SendBeforeApplicationKeys()
    {
        // Direct port of send-fails-with-handshake-traffic-key. Stop immediately
        // after ServerHello, when handshake keys exist but application keys don't.
        using var signer = Identity("server-ecdsa");
        using var verifier = Verifier();
        using var scope = CallbackScope.Enter();
        st_ptls_get_time_t time = new() { cb = TestTimePointer };
        st_ptls_context_t clientContext = new() { random_bytes = TestRandomPointer, get_time = &time,
            cipher_suites = BclCryptoProvider.SymmetricCipherSuites, key_exchanges = BclCryptoProvider.AsymmetricKeyExchanges };
        st_ptls_context_t serverContext = clientContext;
        verifier.ApplyTo(&clientContext); signer.ApplyTo(&serverContext);
        st_ptls_t* client = null; st_ptls_t* server = null;
        st_ptls_buffer_t clientOutput = PicotlsBuffer.Create(), serverOutput = PicotlsBuffer.Create();
        try
        {
            client = PicoTls.ptls_client_new(&clientContext); server = PicoTls.ptls_server_new(&serverContext);
            scope.ThrowIfFailed(); Check(client != null && server != null, "raw send-gate peers allocated");
            ulong consumed = 0;
            Check(PicoTls.ptls_handshake(client, &clientOutput, null, &consumed, null) == 0x202, "raw initial CH status");
            scope.ThrowIfFailed(); Check(clientOutput.off != 0, "raw initial CH bytes");
            consumed = clientOutput.off;
            Check(PicoTls.ptls_handshake(server, &serverOutput, clientOutput.@base, &consumed, null) == 0, "raw server flight status");
            scope.ThrowIfFailed();
            Check(consumed == clientOutput.off && serverOutput.off > 5, "raw server consumed CH");
            ulong firstRecord = 5UL + ((ulong)serverOutput.@base[3] << 8) + serverOutput.@base[4];
            Check(firstRecord <= serverOutput.off, "raw ServerHello record bounds");
            clientOutput.off = 0; consumed = firstRecord;
            Check(PicoTls.ptls_handshake(client, &clientOutput, serverOutput.@base, &consumed, null) == 0x202, "handshake keys only status");
            scope.ThrowIfFailed(); Check(consumed == firstRecord && clientOutput.off == 0, "only ServerHello consumed");
            fixed (byte* hello = "hello"u8)
                Check(PicoTls.ptls_send(client, &clientOutput, hello, 5) == 0x202, "raw send rejects handshake traffic key");
            scope.ThrowIfFailed(); Check(clientOutput.off == 0, "raw early send emits no application ciphertext");
        }
        finally
        {
            if (client != null) PicoTls.ptls_free(client);
            if (server != null) PicoTls.ptls_free(server);
            PicoTls.dotcc_ptls_buffer_dispose(&clientOutput); PicoTls.dotcc_ptls_buffer_dispose(&serverOutput);
        }
        scope.ThrowIfFailed();
    }
    private static int Read16(byte[] bytes, int offset)
    {
        Check(offset >= 0 && offset + 2 <= bytes.Length, "encoded uint16 bound");
        return BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(offset, 2));
    }
    private static void Write16(byte[] bytes, int offset, int value)
    { BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(offset, 2), checked((ushort)value)); }
    private static byte[] RewriteClientExtensions(byte[] hello, Action<List<(ushort Type, byte[] Bytes)>> mutate)
    {
        Check(hello.Length > 44 && hello[0] == 22 && hello[5] == 1, "real complete ClientHello record");
        Check(Read16(hello, 3) + 5 == hello.Length, "single complete ClientHello record");
        int cursor = 44 + hello[43];
        cursor += 2 + Read16(hello, cursor);
        Check(cursor < hello.Length, "CH compression vector present"); cursor += 1 + hello[cursor];
        int extensionsOffset = cursor, length = Read16(hello, cursor); cursor += 2;
        Check(cursor + length == hello.Length, "CH extensions occupy remainder");
        List<(ushort Type, byte[] Bytes)> extensions = [];
        while (cursor < hello.Length)
        {
            ushort type = (ushort)Read16(hello, cursor);
            int size = Read16(hello, cursor + 2); cursor += 4;
            Check(size <= hello.Length - cursor, "CH extension bounded");
            extensions.Add((type, hello.AsSpan(cursor, size).ToArray())); cursor += size;
        }
        mutate(extensions);
        int total = extensions.Sum(e => 4 + e.Bytes.Length);
        byte[] result = new byte[extensionsOffset + 2 + total];
        hello.AsSpan(0, extensionsOffset).CopyTo(result);
        Write16(result, extensionsOffset, total); cursor = extensionsOffset + 2;
        foreach (var (type, bytes) in extensions)
        {
            Write16(result, cursor, type); Write16(result, cursor + 2, bytes.Length);
            bytes.CopyTo(result.AsSpan(cursor + 4)); cursor += 4 + bytes.Length;
        }
        Write16(result, 3, result.Length - 5);
        int messageLength = result.Length - 9;
        result[6] = (byte)(messageLength >> 16); result[7] = (byte)(messageLength >> 8); result[8] = (byte)messageLength;
        return result;
    }
}
