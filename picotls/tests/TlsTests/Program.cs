using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Managed.Security;

internal static partial class Program
{
    private static int assertions;
    private static string credentials = "";
    private static string foreignCredentials = "";
    private static void Check(bool value, string message)
    {
        if (!value) throw new Exception(message);
        Interlocked.Increment(ref assertions);
    }
    private static X509Certificate2 Load(string name, bool foreign = false) => X509CertificateLoader.LoadPkcs12FromFile(
        Path.Combine(foreign ? foreignCredentials : credentials, name + ".pfx"), "", X509KeyStorageFlags.EphemeralKeySet);
    private static BclCryptoProvider.CertificateVerifier Verifier(bool foreign = false)
    {
        using var root = X509CertificateLoader.LoadCertificateFromFile(Path.Combine(foreign ? foreignCredentials : credentials, "ca.cer"));
        return new([root], X509RevocationMode.NoCheck);
    }
    private static BclCryptoProvider.SigningIdentity Identity(string name, bool foreign = false)
    {
        using var leaf = Load(name, foreign);
        return new(leaf);
    }
    private sealed class Pair : IDisposable
    {
        public readonly PicotlsContext ClientContext, ServerContext;
        public readonly PicotlsConnection Client, Server;
        public readonly List<byte> AtClient = [], AtServer = [];
        private readonly Queue<(bool ToServer, byte[] Bytes)> pending = new();
        public Pair(ushort suite = 0x1301, string serverIdentity = "server-ecdsa", string? clientIdentity = null,
            string name = "localhost", bool foreignTrust = false, bool foreignClient = false,
            bool requireClient = false, bool retry = false, bool negotiateFirst = false,
            string serverAlpn = "dotcc-picotls", string clientAlpn = "dotcc-picotls", ushort? clientSuite = null)
        {
            using var signer = Identity(serverIdentity);
            using var clientSigner = clientIdentity == null ? null : Identity(clientIdentity, foreignClient);
            using var clientVerifier = Verifier(foreignTrust);
            using var serverVerifier = Verifier();
            ServerContext = new(true, serverVerifier, signer, [serverAlpn], suite, requireClient, enforceRetry: retry);
            try
            {
                ClientContext = new(false, clientVerifier, clientSigner, [clientAlpn], clientSuite ?? suite, negotiateBeforeKeyExchange: negotiateFirst);
                try
                {
                    Server = ServerContext.CreateConnection();
                    try { Client = ClientContext.CreateConnection(name); }
                    catch { Server.Dispose(); throw; }
                }
                catch { ClientContext.Dispose(); throw; }
            }
            catch { ServerContext.Dispose(); throw; }
            // Credential owners are deliberately disposed at construction end.
            // Context leases must keep native callbacks alive.
        }
        public void Handshake(int fragment = 137)
        {
            var initial = Client.Process([]);
            Check(initial.Consumed == 0 && initial.Plaintext.Length == 0 && initial.Outbound.Length > 0, "client emits real ClientHello");
            Enqueue(true, initial.Outbound, fragment);
            Drain(fragment);
            Check(Client.HandshakeComplete && Server.HandshakeComplete, "both handshakes complete");
            Check(Client.CipherSuite == Server.CipherSuite, "cipher agreement");
            Check(Client.NegotiatedProtocol == "dotcc-picotls" && Server.NegotiatedProtocol == "dotcc-picotls", "ALPN agreement");
        }
        private void Enqueue(bool toServer, byte[] bytes, int fragment)
        {
            for (int offset = 0; offset < bytes.Length; offset += fragment)
                pending.Enqueue((toServer, bytes.AsSpan(offset, Math.Min(fragment, bytes.Length - offset)).ToArray()));
        }
        private void Drain(int fragment)
        {
            int budget = 200000;
            while (pending.TryDequeue(out var packet))
            {
                Check(--budget > 0, "bounded TLS pump");
                var step = (packet.ToServer ? Server : Client).Process(packet.Bytes);
                Check(step.Consumed == packet.Bytes.Length, "all queued bytes consumed exactly once");
                (packet.ToServer ? AtServer : AtClient).AddRange(step.Plaintext);
                Enqueue(!packet.ToServer, step.Outbound, fragment);
            }
        }
        public void Deliver(bool toServer, byte[] bytes, int fragment = 4093) { Enqueue(toServer, bytes, fragment); Drain(fragment); }
        public void Dispose()
        {
            try { Client.Dispose(); }
            finally { try { Server.Dispose(); } finally { ClientContext.Dispose(); ServerContext.Dispose(); } }
        }
    }
    private static void RoundTrip(ushort suite, string identity, int fragment, bool retry = false, bool negotiateFirst = false, string? clientIdentity = null)
    {
        using var pair = new Pair(suite, identity, clientIdentity, requireClient: clientIdentity != null, retry: retry, negotiateFirst: negotiateFirst);
        pair.Handshake(fragment);
        Check(pair.Client.CipherSuite == suite, "requested AES suite selected");
        Check(pair.Server.ServerName == "localhost", "SNI reached server");
        Check(!pair.Client.IsResumed && !pair.Server.IsResumed, "full authenticated handshake");
        byte[] exported = pair.Client.ExportSecret("dotcc test exporter", "context"u8, 64);
        Check(exported.AsSpan().SequenceEqual(pair.Server.ExportSecret("dotcc test exporter", "context"u8, 64)), "exporter agrees");
        Check(!exported.AsSpan().SequenceEqual(pair.Server.ExportSecret("dotcc test exporter", "different"u8, 64)), "exporter context binding");
        pair.ClientContext.Dispose(); pair.ServerContext.Dispose();
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        foreach (int size in new[] { 0, 1, 16384, 65537 })
        {
            byte[] data = Enumerable.Range(0, size).Select(i => (byte)(i * 31 + 7)).ToArray();
            pair.AtServer.Clear(); pair.AtClient.Clear();
            pair.Deliver(true, pair.Client.Send(data));
            Check(data.AsSpan().SequenceEqual(pair.AtServer.ToArray()), "client to server authenticated payload");
            pair.Deliver(false, pair.Server.Send(data));
            Check(data.AsSpan().SequenceEqual(pair.AtClient.ToArray()), "server to client authenticated payload");
        }
        pair.Deliver(true, pair.Client.UpdateKey());
        pair.AtClient.Clear(); pair.AtServer.Clear();
        pair.Deliver(true, pair.Client.Send("after client update"u8));
        pair.Deliver(false, pair.Server.Send("after requested server update"u8));
        Check(pair.AtServer.ToArray().AsSpan().SequenceEqual("after client update"u8), "updated client key works");
        Check(pair.AtClient.ToArray().AsSpan().SequenceEqual("after requested server update"u8), "requested peer update works");
        pair.Deliver(true, pair.Client.CloseNotify(), 1);
        pair.Deliver(false, pair.Server.CloseNotify(), 1);
        Check(pair.Client.PeerClosed && pair.Server.PeerClosed, "bidirectional close_notify");
        pair.Client.CompleteInput(); pair.Server.CompleteInput();
    }
    private static void Reject(string name, Action action, Func<Exception, bool>? expected = null)
    {
        try { action(); }
        catch (Exception error) when (expected?.Invoke(error) ?? error is AuthenticationException)
        { Check(true, name); return; }
        throw new Exception("Expected rejection: " + name);
    }
    private static void Negatives()
    {
        Reject("wrong DNS name", () => { using var p = new Pair(name: "wrong.invalid"); p.Handshake(); });
        Reject("wrong IP", () => { using var p = new Pair(name: "127.0.0.2"); p.Handshake(); });
        Reject("untrusted chain", () => { using var p = new Pair(foreignTrust: true); p.Handshake(); });
        Reject("expired certificate", () => { using var p = new Pair(serverIdentity: "server-expired"); p.Handshake(); });
        Reject("wrong certificate SAN", () => { using var p = new Pair(serverIdentity: "server-wrong-name"); p.Handshake(); });
        Reject("wrong server EKU", () => { using var p = new Pair(serverIdentity: "client-ecdsa"); p.Handshake(); });
        Reject("required client certificate", () => { using var p = new Pair(requireClient: true); p.Handshake(); });
        Reject("untrusted client certificate", () => { using var p = new Pair(requireClient: true, clientIdentity: "client-rsa", foreignClient: true); p.Handshake(); });
        Reject("ALPN mismatch", () => { using var p = new Pair(clientAlpn: "other"); p.Handshake(); });
        Reject("no common cipher", () => { using var p = new Pair(suite: 0x1301, clientSuite: 0x1302); p.Handshake(); });
        using (var p = new Pair(name: "127.0.0.1")) p.Handshake();
        using (var p = new Pair())
        {
            p.Handshake(); byte[] record = p.Client.Send("must never be released"u8); record[^1] ^= 1;
            Reject("corrupt AEAD tag", () => p.Server.Process(record), e => e is PicotlsException { ErrorCode: 20 });
            Reject("failed connection is unusable", () => p.Server.Send("x"u8), e => e is ObjectDisposedException);
        }
        using (var p = new Pair())
        {
            p.Handshake(); byte[] record = p.Client.Send("truncated"u8);
            Check(p.Server.Process(record.AsSpan(0, record.Length - 1)).Plaintext.Length == 0, "partial record emits no plaintext");
            Reject("truncated transport", p.Server.CompleteInput, e => e is EndOfStreamException);
        }
        using (var p = new Pair())
            Reject("oversized record", () => p.Server.Process(new byte[] { 22, 3, 3, 255, 255 }));
    }
    private static void ExporterLabelBounds()
    {
        using var pair = new Pair();
        pair.Handshake();
        foreach (string label in new[] { new string('a', 250), new string('\u00e9', 125), "embedded\0label" })
            Reject("invalid exporter label", () => pair.Client.ExportSecret(label, [], 32), e => e is ArgumentOutOfRangeException);
        foreach (string label in new[] { new string('a', 249), new string('\u00e9', 124) + "a" })
            Check(pair.Client.ExportSecret(label, [], 32).AsSpan().SequenceEqual(pair.Server.ExportSecret(label, [], 32)),
                "maximum encoded exporter label agrees after rejected arguments");
    }
    public static int Main(string[] args)
    {
        string work = args.Length == 1 ? Path.GetFullPath(args[0]) : Path.Combine(Path.GetTempPath(), "dotcc-tls-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(work);
        credentials = Path.Combine(work, "credentials"); foreignCredentials = Path.Combine(work, "foreign-credentials");
        try
        {
            Credentials.Create(credentials); Credentials.Create(foreignCredentials);
            BclCryptoProvider.InitializeSymmetric(); BclCryptoProvider.InitializeAsymmetric();
            foreach (ushort suite in new ushort[] { 0x1301, 0x1302 })
                foreach (string identity in new[] { "server-ecdsa", "server-rsa" })
                    RoundTrip(suite, identity, 137);
            RoundTrip(0x1301, "server-ecdsa", 1, retry: true);
            RoundTrip(0x1302, "server-rsa", 65536, negotiateFirst: true);
            RoundTrip(0x1301, "server-ecdsa", 137, clientIdentity: "client-rsa");
            RoundTrip(0x1302, "server-rsa", 137, clientIdentity: "client-ecdsa");
            Negatives();
            ExporterLabelBounds();
            AuthenticatedHandshakeCorruption();
            UpstreamHandshakePorts();
            AdditionalUpstreamPorts();
            SessionTickets();
            MalformedAndLifetime();
            Parallel.For(0, 8, i => RoundTrip(i % 2 == 0 ? (ushort)0x1301 : (ushort)0x1302, "server-ecdsa", 4093));
            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            Check(BclCryptoProvider.LiveManagedContexts == 0, "provider states released after all success/failure paths");
            Console.WriteLine($"PASS: {assertions} TLS assertions; full handshakes, authentication, records, HRR, exporter, key update, close, concurrency.");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
