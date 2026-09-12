using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Managed.Security;

internal static unsafe class Program
{
    private static readonly List<TlsCase> Cases = [];
    internal static void Require(bool value, string message)
    { if (!value) throw new InvalidOperationException(message); }

    public static int Main(string[] args)
    {
        using var credentials = new Credentials();
        foreach (ushort suite in new ushort[] { 0x1301, 0x1302 })
        foreach (bool rsa in new[] { false, true })
        foreach (bool retry in new[] { false, true })
        {
            string name = $"{suite:x4}-{(rsa ? "rsa" : "ecdsa")}-{(retry ? "retry-fragment1" : "fragment137")}";
            Run(name, () => Handshake(credentials, suite, rsa, retry, retry ? 1 : 137));
        }
        Run("reject-alpn", () => Reject(credentials, "alpn"));
        Run("reject-name", () => Reject(credentials, "name"));
        Run("reject-trust", () => Reject(credentials, "trust"));
        var receipt = new TlsReceipt(DateTimeOffset.UtcNow,
            RuntimeFeature.IsDynamicCodeSupported ? "jit" : "nativeaot",
            RuntimeInformation.ProcessArchitecture.ToString(), RuntimeInformation.FrameworkDescription,
            Cases.All(c => c.Passed), Cases,
            ["Raw translated picotls handshake API only; no TLS records or stream facade.",
             "Transport parameters are exchanged as opaque test bytes; MsQuic validation/offset/result mapping remains integration work.",
             "Post-handshake ticket output is exercised; no MsQuic ticket envelope, resumption, key phase update, or ticket lifecycle is qualified."]);
        var json = JsonSerializer.Serialize(receipt, TlsJsonContext.Default.TlsReceipt);
        if (args.Length == 1)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(args[0]))!);
            File.WriteAllText(args[0], json + Environment.NewLine);
        }
        Console.WriteLine(json);
        return receipt.Passed ? 0 : 1;
    }

    private static void Run(string name, Func<Dictionary<string, string>> test)
    {
        int before = BclCryptoProvider.LiveManagedContexts;
        var start = Stopwatch.GetTimestamp();
        try
        {
            var observations = test();
            Require(BclCryptoProvider.LiveManagedContexts == before, "provider context leak after case");
            Cases.Add(new(name, true, Stopwatch.GetElapsedTime(start).TotalMilliseconds, observations, null));
            Console.Error.WriteLine($"PASS {name}");
        }
        catch (Exception error)
        {
            Cases.Add(new(name, false, Stopwatch.GetElapsedTime(start).TotalMilliseconds, [], error.ToString()));
            Console.Error.WriteLine($"FAIL {name}: {error.Message}");
        }
    }

    private static Dictionary<string, string> Handshake(Credentials credentials, ushort suite, bool rsa, bool retry, int fragment)
    {
        using var verifier = new BclCryptoProvider.CertificateVerifier([credentials.Root], X509RevocationMode.NoCheck);
        using var signer = new BclCryptoProvider.SigningIdentity(rsa ? credentials.RsaLeaf : credentials.EcLeaf);
        using var tickets = new BclCryptoProvider.TicketProtector(TimeSpan.FromHours(1));
        using var client = new RawPeer(false, suite, verifier, null, "dotcc-quic", "localhost", false);
        using var server = new RawPeer(true, suite, null, signer, "dotcc-quic", "localhost", retry, tickets);
        var counts = Pump(client, server, fragment);
        Require(client.Complete && server.Complete, "both raw handshakes must complete");
        Require(client.Cipher == suite && server.Cipher == suite, "cipher selection");
        Require(client.Protocol == "dotcc-quic" && server.Protocol == "dotcc-quic", "ALPN selection");
        Require(server.ServerName == "localhost", "SNI delivered to server");
        Require(client.ReceivedParameters.AsSpan().SequenceEqual(server.Parameters), "server transport parameters");
        Require(server.ReceivedParameters.AsSpan().SequenceEqual(client.Parameters), "client transport parameters");
        int matches = 0;
        foreach (ulong epoch in new ulong[] { 2, 3 })
        foreach (bool encryption in new[] { false, true })
        {
            Require(client.Secrets.TryGetValue((epoch, encryption), out var clientSecret), "missing client traffic secret");
            Require(server.Secrets.TryGetValue((epoch, !encryption), out var serverSecret), "missing server traffic secret");
            Require(CryptographicOperations.FixedTimeEquals(clientSecret!, serverSecret!), "directional traffic secret mismatch");
            Require(clientSecret!.Length == (suite == 0x1301 ? 32 : 48), "traffic secret digest length");
            matches++;
        }
        Require(client.Secrets.Count == 4 && server.Secrets.Count == 4, "unexpected early-data/key update secrets");
        Require(client.ReadEpoch == 3 && server.ReadEpoch == 3, "final read epoch");
        Require(counts.Epochs.SetEquals(new ulong[] { 0, 2, 3 }) && client.SavedTickets > 0,
            "post-handshake ticket must be emitted/consumed at application epoch");
        Require(client.Process(0, new byte[] { 4, 0, 0, 0 }).Code == 10, "wrong epoch must be rejected");
        Require(counts.ClientHellos == (retry ? 2 : 1), "HelloRetryRequest/ClientHello count");
        return new()
        {
            ["cipher"] = suite.ToString("x4"), ["secret_matches"] = matches.ToString(),
            ["client_hellos"] = counts.ClientHellos.ToString(), ["raw_steps"] = counts.Steps.ToString(),
            ["client_tp_bytes"] = client.Parameters.Length.ToString(), ["server_tp_bytes"] = server.Parameters.Length.ToString(),
            ["output_epochs"] = string.Join(",", counts.Epochs.Order()), ["final_read_epoch"] = "3",
            ["wrong_epoch_alert"] = "10", ["fragment_size"] = fragment.ToString(),
            ["saved_tickets"] = client.SavedTickets.ToString()
        };
    }

    private static Dictionary<string, string> Reject(Credentials credentials, string reason)
    {
        using var foreign = Credentials.CreateRoot("CN=Untrusted probe root");
        using var verifier = new BclCryptoProvider.CertificateVerifier(
            [reason == "trust" ? foreign : credentials.Root], X509RevocationMode.NoCheck);
        using var signer = new BclCryptoProvider.SigningIdentity(credentials.EcLeaf);
        using var client = new RawPeer(false, 0x1301, verifier, null, "dotcc-quic",
            reason == "name" ? "wrong.invalid" : "localhost", false);
        using var server = new RawPeer(true, 0x1301, null, signer,
            reason == "alpn" ? "different-protocol" : "dotcc-quic", "localhost", false);
        try { Pump(client, server, 137); }
        catch (HandshakeAlert error) when (error.Code is > 0 and < 256)
        {
            Require(!client.Complete || !server.Complete, "rejected handshake completed at both peers");
            return new() { ["alert"] = error.Code.ToString(), ["reason"] = reason };
        }
        throw new InvalidOperationException("Expected authenticated handshake rejection: " + reason);
    }

    private static PumpResult Pump(RawPeer client, RawPeer server, int fragment)
    {
        var queue = new Queue<(RawPeer Receiver, ulong Epoch, byte[] Data)>();
        int steps = 0, hellos = 0;
        var epochs = new HashSet<ulong>();
        void Enqueue(RawPeer receiver, RawStep step)
        {
            if (step.Code is not (0 or 0x202)) throw new HandshakeAlert(step.Code);
            foreach (var flight in step.Flights)
            {
                epochs.Add(flight.Epoch);
                Require(flight.Epoch is 0 or 2 or 3, "0-RTT output must stay disabled");
                // Each output range contains complete raw handshake messages.
                int offset = 0;
                while (offset < flight.Data.Length)
                {
                    Require(flight.Data.Length - offset >= 4, "partial output handshake header");
                    byte type = flight.Data[offset];
                    int length = (flight.Data[offset + 1] << 16) | (flight.Data[offset + 2] << 8) | flight.Data[offset + 3];
                    Require(length <= flight.Data.Length - offset - 4, "invalid raw handshake output length");
                    Require(type != 22, "TLS record framing appeared in raw handshake output");
                    if (type == 1) hellos++;
                    offset += 4 + length;
                }
                for (offset = 0; offset < flight.Data.Length; offset += fragment)
                    queue.Enqueue((receiver, flight.Epoch, flight.Data.AsSpan(offset, Math.Min(fragment, flight.Data.Length - offset)).ToArray()));
            }
        }
        Enqueue(server, client.Process(0, [], startClient: true));
        while (queue.TryDequeue(out var next))
        {
            Require(++steps < 100_000, "raw handshake pump exceeded budget");
            Require(next.Receiver.ReadEpoch == next.Epoch, "input supplied before matching read epoch");
            if (steps % 97 == 0) { GC.Collect(); GC.WaitForPendingFinalizers(); }
            Enqueue(ReferenceEquals(next.Receiver, client) ? server : client, next.Receiver.Process(next.Epoch, next.Data));
        }
        return new(steps, hellos, epochs);
    }

    private sealed record PumpResult(int Steps, int ClientHellos, HashSet<ulong> Epochs);
}

internal sealed class HandshakeAlert(int code) : Exception($"raw picotls returned 0x{code:x}")
{ public int Code { get; } = code; }
internal sealed record RawFlight(ulong Epoch, byte[] Data);
internal sealed record RawStep(int Code, List<RawFlight> Flights);
internal sealed record TlsCase(string Name, bool Passed, double Milliseconds, Dictionary<string, string> Observations, string? Error);
internal sealed record TlsReceipt(DateTimeOffset Time, string Runtime, string Architecture, string Framework,
    bool Passed, List<TlsCase> Cases, List<string> Limits);
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(TlsReceipt))]
internal partial class TlsJsonContext : JsonSerializerContext;

internal sealed class Credentials : IDisposable
{
    public X509Certificate2 Root { get; } = CreateRoot("CN=dotcc QUIC feasibility root");
    public X509Certificate2 EcLeaf { get; }
    public X509Certificate2 RsaLeaf { get; }
    public Credentials()
    {
        using var ec = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var rsa = RSA.Create(2048);
        EcLeaf = Leaf(new CertificateRequest("CN=localhost", ec, HashAlgorithmName.SHA256), ec, null);
        RsaLeaf = Leaf(new CertificateRequest("CN=localhost", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1), null, rsa);
    }
    public static X509Certificate2 CreateRoot(string subject)
    {
        using var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var request = new CertificateRequest(subject, key, HashAlgorithmName.SHA256);
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign, true));
        request.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(request.PublicKey, false));
        return request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(2));
    }
    private X509Certificate2 Leaf(CertificateRequest request, ECDsa? ec, RSA? rsa)
    {
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, true));
        request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new("1.3.6.1.5.5.7.3.1") }, false));
        var san = new SubjectAlternativeNameBuilder(); san.AddDnsName("localhost");
        request.CertificateExtensions.Add(san.Build());
        using var issuerKey = Root.GetECDsaPrivateKey()!;
        using var certificate = request.Create(Root.SubjectName, X509SignatureGenerator.CreateForECDsa(issuerKey),
            DateTimeOffset.UtcNow.AddHours(-1), DateTimeOffset.UtcNow.AddDays(1), RandomNumberGenerator.GetBytes(16));
        return ec != null ? certificate.CopyWithPrivateKey(ec) : certificate.CopyWithPrivateKey(rsa!);
    }
    public void Dispose() { EcLeaf.Dispose(); RsaLeaf.Dispose(); Root.Dispose(); }
}
