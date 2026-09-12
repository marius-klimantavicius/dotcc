using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;
using Managed.Security;
using Managed.Transport;
using Managed.Transport.Hosting;

internal static unsafe class Program
{
    private static readonly List<CaseResult> Cases = [];
    internal static void Require(bool value, string description) { if (!value) throw new InvalidOperationException(description); }
    public static int Main(string[] args)
    {
        using var host = new MsQuicHost();
        MSQUIC_HOST_TABLE table = host.CreateTlsTable();
        Require(MsQuic.MsQuicHostInstall(&table) == 0, "install actual TLS table");
        try
        {
            Require((int)MsQuic.CxPlatTlsGetProvider() == 0x10000, "explicit picotls provider identity");
            foreach (ushort suite in new ushort[] { 0x1301, 0x1302 })
            foreach (bool rsa in new[] { false, true })
            foreach (bool retry in new[] { false, true })
                Run(host, $"handshake-{suite:x4}-{(rsa ? "rsa" : "ec")}-{(retry ? "hrr-bytewise" : "fragment137")}", () => Handshake(host, suite, rsa, retry));
            foreach (string reason in new[] { "name", "trust", "alpn", "tp" })
                Run(host, "reject-" + reason, () => Reject(host, reason));
            Run(host, "reject-nul-alpn", () => NullAlpn(host));
            Run(host, "raw-peer-hrr", () => RawBridge(host));
            Run(host, "delayed-imported-ticket-resumption", () => Tickets(host));
            foreach (string mode in new[] { "accept", "reject", "deferred-trust", "async" })
                Run(host, "credential-" + mode, () => CredentialCallbacks(host, mode));
            Run(host, "credential-flag-rejections", () => CredentialFlagRejections(host));
        }
        finally { Require(MsQuic.MsQuicHostUninstall() == 0, "uninstall TLS table"); }
        var result = new AdapterReceipt(Cases.All(c => c.Passed), Cases,
            ["Actual TLS adapter/packet crypto callbacks; no QUIC transport scheduling or UDP interoperability claim.",
             "TP57 bytes are opaque fixtures; connection-specific core TP validation is outside this harness.",
             "Normal delayed ticket issuance uses elapsed wall time; provider envelope expiry has separate provider-vector coverage."]);
        string json = JsonSerializer.Serialize(result, AdapterJsonContext.Default.AdapterReceipt);
        if (args.Length == 1) File.WriteAllText(args[0], json + "\n");
        Console.WriteLine($"{(result.Passed ? "PASS" : "FAIL")} TLS adapter: {Cases.Count} cases; resources drained");
        return result.Passed ? 0 : 1;
    }
    private static void Run(MsQuicHost host, string name, Action action)
    {
        int handles = BclCryptoProvider.LiveManagedContexts;
        try
        {
            action();
            Require(host.OutstandingResources == 0 && host.OutstandingPlatformAllocations == 0 && BclCryptoProvider.LiveManagedContexts == handles,
                "all typed host resources, buffers and provider contexts drained");
            Cases.Add(new(name, true, null)); Console.Error.WriteLine("PASS " + name);
        }
        catch (Exception error) { Cases.Add(new(name, false, error.ToString())); Console.Error.WriteLine("FAIL " + name + ": " + error); }
    }
    private static void Handshake(MsQuicHost host, ushort suite, bool rsa, bool retry)
    {
        using var certificates = new Credentials();
        using var clientCredential = host.CreateClientCredential([certificates.Root], suite);
        using var serverCredential = host.CreateServerCredential(rsa ? certificates.RsaLeaf : certificates.EcLeaf, cipherSuite: suite);
        using var client = new AdapterPeer(host, clientCredential, false);
        using var server = new AdapterPeer(host, serverCredential, true, retry: retry);
        certificates.Dispose(); clientCredential.Dispose(); serverCredential.Dispose();
        client.DeleteSecurityOwner(); server.DeleteSecurityOwner();
        int hellos = Pump(client, server, retry ? 1 : 137);
        Require(hellos == (retry ? 2 : 1), "actual HelloRetryRequest ClientHello count");
        Require(client.Complete && server.Complete && !client.Resumed && !server.Resumed, "both initial handshakes complete without PSK");
        Require(client.ReadEpoch == 3 && server.ReadEpoch == 3, "application read key activates after Finished");
        Require(client.TpCallbacks == 1 && server.TpCallbacks == 0 && client.ReceivedParameters.AsSpan().SequenceEqual(server.Parameters), "core TP callback role contract");
        Require(client.Tickets.Count == 0 && server.Tickets.Count == 0, "no ticket output before application release");
        Require(client.Compactions > 1 && server.Compactions > 1, "actual output survives repeated core compaction");
        foreach (var peer in new[] { client, server })
        {
            uint required = 0;
            Require(MsQuic.CxPlatTlsParamGet(peer.Token, MsQuic.QUIC_PARAM_TLS_HANDSHAKE_INFO, &required, null) == Status.BufferTooSmall && required == sizeof(QUIC_HANDSHAKE_INFO), "handshake-info size query");
            QUIC_HANDSHAKE_INFO info = default;
            Require(MsQuic.CxPlatTlsParamGet(peer.Token, MsQuic.QUIC_PARAM_TLS_HANDSHAKE_INFO, &required, &info) == 0 && (ushort)info.CipherSuite == suite, "actual handshake cipher result");
            Require(peer.State->NegotiatedAlpn != null && peer.State->NegotiatedAlpn[0] == 10 && new ReadOnlySpan<byte>(peer.State->NegotiatedAlpn + 1, 10).SequenceEqual("dotcc-quic"u8), "negotiated ALPN pointer/length");
            Require((peer.CombinedFlags & (uint)CXPLAT_TLS_RESULT_FLAGS.CXPLAT_TLS_RESULT_HANDSHAKE_COMPLETE) != 0, "handshake result flag observed");
            Require(peer.State->ReadKeys[1].Value == null && peer.State->WriteKeys[1].Value == null, "no zero-RTT packet keys");
        }
        MatchKeys(client, server); MatchKeys(server, client);
        // Reject KeyUpdate as soon as its type arrives, before a fragmented header/body can be accepted.
        client.Process([24], expectedAlert: 10);
        server.Process([24, 0], expectedAlert: 10);
    }
    private static void MatchKeys(AdapterPeer sender, AdapterPeer receiver)
    {
        foreach (int epoch in new[] { 2, 3 })
        {
            QUIC_PACKET_KEY* write = sender.State->WriteKeys[epoch].Value, read = receiver.State->ReadKeys[epoch].Value;
            Require(write != null && read != null && new ReadOnlySpan<byte>(write->Iv, 12).SequenceEqual(new ReadOnlySpan<byte>(read->Iv, 12)), "directional QUIC traffic IVs match");
            byte[] payload = new byte[32]; "adapter key test"u8.CopyTo(payload);
            fixed (byte* bytes = payload)
            {
                Require(MsQuic.CxPlatEncrypt(write->PacketKey, write->Iv, 0, null, 32, bytes) == 0, "derived write key encrypts");
                Require(MsQuic.CxPlatDecrypt(read->PacketKey, read->Iv, 0, null, 32, bytes) == 0, "opposite derived read key authenticates");
            }
            Require(payload.AsSpan(0, 16).SequenceEqual("adapter key test"u8), "derived key plaintext matches");
        }
    }
    private static int Pump(AdapterPeer client, AdapterPeer server, int fragment, List<RawFlight>? initial = null)
    {
        var queue = new Queue<(AdapterPeer Peer, ulong Epoch, byte[] Bytes)>();
        int hellos = 0, steps = 0;
        void Enqueue(AdapterPeer receiver, IEnumerable<RawFlight> flights)
        {
            foreach (var flight in flights)
            {
                Require(flight.Epoch is 0 or 2 or 3, "no early-data output epoch");
                for (int message = 0; message < flight.Data.Length;)
                {
                    Require(flight.Data.Length - message >= 4, "whole emitted handshake header");
                    int length = (flight.Data[message + 1] << 16) | (flight.Data[message + 2] << 8) | flight.Data[message + 3];
                    Require(length <= flight.Data.Length - message - 4, "whole emitted handshake body");
                    if (flight.Data[message] == 1) hellos++;
                    message += 4 + length;
                }
                for (int at = 0; at < flight.Data.Length; at += fragment)
                    queue.Enqueue((receiver, flight.Epoch, flight.Data.AsSpan(at, Math.Min(fragment, flight.Data.Length - at)).ToArray()));
            }
        }
        Enqueue(initial == null ? server : client, initial ?? client.Process([]));
        while (queue.TryDequeue(out var next))
        {
            Require(++steps < 100000, "bounded TLS adapter pump");
            Require(next.Peer.ReadEpoch == next.Epoch, "core feeds matching read encryption level");
            if (steps % 97 == 0) { GC.Collect(); GC.WaitForPendingFinalizers(); }
            Enqueue(ReferenceEquals(next.Peer, client) ? server : client, next.Peer.Process(next.Bytes));
        }
        return hellos;
    }
    private static void Reject(MsQuicHost host, string reason)
    {
        using var certificates = new Credentials(); using var foreign = Credentials.CreateRoot("CN=foreign");
        using var cc = host.CreateClientCredential([reason == "trust" ? foreign : certificates.Root], 0x1301);
        using var sc = host.CreateServerCredential(certificates.EcLeaf, cipherSuite: 0x1301);
        using var client = new AdapterPeer(host, cc, false, name: reason == "name" ? "wrong.invalid" : "localhost");
        using var server = new AdapterPeer(host, sc, true, protocol: reason == "alpn" ? "different" : "dotcc-quic");
        if (reason == "tp") client.AcceptParameters = false;
        try { Pump(client, server, 3); }
        catch (AdapterAlert alert)
        {
            Require(!client.Complete || !server.Complete, "negative handshake cannot complete both roles");
            Require(reason != "alpn" || alert.Alert == 120, "ALPN mismatch exact alert");
            Require(reason != "tp" || alert.Alert == 47, "TP rejection exact alert"); return;
        }
        throw new InvalidOperationException("Expected " + reason + " rejection");
    }
    private static void NullAlpn(MsQuicHost host)
    {
        using var certificates = new Credentials(); using var credential = host.CreateClientCredential([certificates.Root]);
        bool rejected = false;
        try { using var peer = new AdapterPeer(host, credential, false, protocol: "bad\0alpn"); }
        catch (ArgumentException) { rejected = true; }
        Require(rejected, "NUL ALPN rejected before TLS registration");
    }
    private static void RawBridge(MsQuicHost host)
    {
        using var certificates = new Credentials(); using var credential = host.CreateClientCredential([certificates.Root], 0x1302);
        using var signer = new BclCryptoProvider.SigningIdentity(certificates.RsaLeaf);
        using var client = new AdapterPeer(host, credential, false);
        using var server = new RawPeer(true, 0x1302, null, signer, "dotcc-quic", "localhost", true);
        var queue = new Queue<(bool ToServer, RawFlight Flight)>(); int steps = 0;
        void Add(bool toServer, IEnumerable<RawFlight> flights)
        {
            foreach (var flight in flights)
                for (int i = 0; i < flight.Data.Length; i++) queue.Enqueue((toServer, new(flight.Epoch, [flight.Data[i]])));
        }
        Add(true, client.Process([]));
        while (queue.TryDequeue(out var item))
        {
            Require(++steps < 100000, "bounded raw bridge");
            if (item.ToServer)
            {
                Require(server.ReadEpoch == item.Flight.Epoch, "raw peer matching epoch");
                var result = server.Process(item.Flight.Epoch, item.Flight.Data);
                Require(result.Code is 0 or 0x202, "raw server accepts authored client"); Add(false, result.Flights);
            }
            else { Require(client.ReadEpoch == item.Flight.Epoch, "adapter matching epoch"); Add(true, client.Process(item.Flight.Data)); }
        }
        Require(client.Complete && server.Complete && client.TpCallbacks == 1, "independent raw HRR peer handshake");
    }
    private static void Tickets(MsQuicHost host)
    {
        using var certificates = new Credentials();
        using var cc = host.CreateClientCredential([certificates.Root], 0x1301);
        using var sc = host.CreateServerCredential(certificates.EcLeaf, cipherSuite: 0x1301);
        byte[] saved;
        using (var client = new AdapterPeer(host, cc, false))
        using (var server = new AdapterPeer(host, sc, true, key: 1))
        {
            Pump(client, server, 137);
            Require(client.Tickets.Count == 0, "no ticket before explicit application release");
            Thread.Sleep(1100);
            uint previous = server.Total;
            var output = server.Process("application-state"u8, ticket: true);
            Require(output.Count == 1 && output[0].Epoch == 3 && server.Total > previous, "delayed ticket release appends application-epoch bytes");
            Pump(client, server, 1, output);
            Require(client.Tickets.Count == 1, "client receives one saved TLS ticket");
            saved = client.Tickets[0].ToArray();
            Pump(client, server, 1, server.Process("second-application-state"u8, ticket: true));
            Require(client.Tickets.Count == 2 && !client.Tickets[0].AsSpan().SequenceEqual(client.Tickets[1]), "two explicit releases issue distinct tickets");
            Require(!client.Tickets[0].AsSpan(client.Tickets[0].Length - 32).SequenceEqual(client.Tickets[1].AsSpan(client.Tickets[1].Length - 32)), "fresh ticket nonce derives distinct resumption PSKs");
        }
        try
        {
            foreach (string mode in new[] { "retained", "rejected-app", "removed", "corrupt" })
            {
                byte[] input = saved.ToArray();
                if (mode == "corrupt")
                {
                    // Pinned stored-ticket format: obtained8/kx2/cipher2/bodyLength3,
                    // then lifetime4/ageAdd4/nonceLength1/nonce/ticketLength2/identity.
                    int identity = 26 + input[23];
                    Require(input.AsSpan(identity, 4).SequenceEqual("DPTK"u8) && input[identity + 4] == 2, "locate actual imported ticket identity");
                    input[identity + 81] ^= 1;
                }
                using var client = new AdapterPeer(host, cc, false, ticket: input);
                using var server = new AdapterPeer(host, sc, true, key: mode == "removed" ? (byte)2 : (byte)1);
                if (mode == "retained") server.ImportKeys(2, 1);
                if (mode == "rejected-app") server.AcceptTicket = false;
                Pump(client, server, 13);
                bool expected = mode == "retained";
                Require(client.Complete && server.Complete && client.Resumed == expected && server.Resumed == expected,
                    $"imported ticket resumption/fallback {mode}: clientComplete={client.Complete}, serverComplete={server.Complete}, clientResumed={client.Resumed}, serverResumed={server.Resumed}, appCallbacks={server.TicketCallbacks}");
                if (mode is "retained" or "rejected-app")
                    Require(server.TicketCallbacks == 1 && server.Tickets[0].AsSpan().SequenceEqual("application-state"u8), "authenticated application ticket callback");
                else Require(server.TicketCallbacks == 0, "invalid ticket never reaches application");
                Require(client.State->EarlyDataState == CXPLAT_TLS_EARLY_DATA_STATE.CXPLAT_TLS_EARLY_DATA_UNSUPPORTED && client.State->WriteKeys[1].Value == null,
                    "resumption never enables early data");
                CryptographicOperations.ZeroMemory(input);
            }
        }
        finally { CryptographicOperations.ZeroMemory(saved); }
    }
    private static void CredentialCallbacks(MsQuicHost host, string mode)
    {
        using var certificates = new Credentials(); using var foreign = Credentials.CreateRoot("CN=untrusted deferred root");
        using var cc = host.CreateClientCredential([mode == "deferred-trust" ? foreign : certificates.Root], 0x1301);
        using var sc = host.CreateServerCredential(certificates.EcLeaf, cipherSuite: 0x1301);
        uint flags = 0x4010u | (mode == "deferred-trust" ? 0x20u : 0) | (mode == "async" ? 2u : 0);
        using var client = new AdapterPeer(host, cc, false, credentialFlags: flags);
        using var server = new AdapterPeer(host, sc, true, credentialFlags: mode == "async" ? 2u : 0);
        if (mode == "reject") client.AcceptCertificate = false;
        certificates.Dispose(); cc.Dispose(); sc.Dispose(); client.DeleteSecurityOwner(); server.DeleteSecurityOwner();
        bool failed = false;
        try { Pump(client, server, 17); }
        catch (AdapterAlert error)
        {
            failed = true;
            Require(mode == "reject" ? error.Alert == 42 : mode == "deferred-trust", "credential indication rejection alert");
        }
        Require(failed == (mode is "reject" or "deferred-trust"), "application approval cannot bypass failed trust");
        Require(client.CertificateCallbacks == 1, "portable leaf/chain callback exactly once");
        Require((client.DeferredCertificateStatus != Status.Success) == (mode == "deferred-trust"), "deferred certificate validation status");
        if (!failed) Require(client.Complete && server.Complete, "accepted portable indication handshake");
    }
    private static void CredentialFlagRejections(MsQuicHost host)
    {
        using var certificates = new Credentials(); using var credential = host.CreateClientCredential([certificates.Root]);
        foreach (var test in new (uint Flags, bool? Handler, bool Callback, uint Expected)[]
        {
            (2, false, true, Status.InvalidParameter),
            (0, true, true, Status.InvalidParameter),
            (0x20, null, true, Status.InvalidParameter),
            (0x10, null, true, Status.NotSupported),
            (0x4010, null, false, Status.NotSupported)
        })
        {
            bool failed = false;
            try { using var unexpected = new AdapterPeer(host, credential, false, credentialFlags: test.Flags, asyncHandler: test.Handler, certificateCallback: test.Callback); }
            catch (CredentialConfigError error) { failed = true; Require(error.Status == test.Expected, "credential flag rejection status"); }
            Require(failed, "invalid credential combination rejected before completion");
        }
    }
}
internal sealed record CaseResult(string Name, bool Passed, string? Error);
internal sealed record AdapterReceipt(bool Passed, List<CaseResult> Cases, List<string> Limits);
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(AdapterReceipt))]
internal partial class AdapterJsonContext : JsonSerializerContext;
