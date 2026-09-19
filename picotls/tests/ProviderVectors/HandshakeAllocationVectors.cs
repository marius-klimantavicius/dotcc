using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Managed.Security;

// These are real facade handshakes over the unchanged translated protocol core.
// The hook covers provider context/GCHandle allocation boundaries only; it does
// not claim exhaustive BCL allocation or translated-core malloc failure coverage.
static class HandshakeAllocationVectors
{
    private const int MaximumAllocations = 512, MaximumFlights = 32;
    private sealed record Outcome(bool Failed, int Attempts, int FailedClones, int Tickets, int FailedCalls);

    internal static void Run()
    {
        BclCryptoProvider.InitializeSymmetric(); BclCryptoProvider.InitializeAsymmetric();
        int handles = BclCryptoProvider.LiveManagedContexts, keys = BclCryptoProvider.LiveTicketKeysForTesting;
        using var rootKey = RSA.Create(2048);
        DateTimeOffset now = DateTimeOffset.UtcNow;
        var rootRequest = new CertificateRequest("CN=handshake allocation vector root", rootKey,
            HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        rootRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
        rootRequest.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign, true));
        using var root = rootRequest.CreateSelfSigned(now.AddDays(-2), now.AddDays(2));
        using var leafKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var leafRequest = new CertificateRequest("CN=unused-cn.invalid", leafKey, HashAlgorithmName.SHA256);
        leafRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
        leafRequest.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, true));
        leafRequest.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new("1.3.6.1.5.5.7.3.1") }, true));
        var names = new SubjectAlternativeNameBuilder(); names.AddDnsName("localhost");
        leafRequest.CertificateExtensions.Add(names.Build());
        using var issued = leafRequest.Create(root.SubjectName, X509SignatureGenerator.CreateForRSA(rootKey, RSASignaturePadding.Pkcs1),
            now.AddDays(-1), now.AddDays(1), RandomNumberGenerator.GetBytes(16));
        using var leaf = issued.CopyWithPrivateKey(leafKey);

        var baseline = Attempt(root, leaf, int.MaxValue);
        Program.Check(!baseline.Failed && baseline.Tickets > 0 && baseline.FailedClones == 0,
            "allocation baseline completes an authenticated handshake and saves a real server ticket");
        Program.Check(baseline.Attempts is > 0 and <= MaximumAllocations, "handshake provider allocation sweep stays within its explicit bound");
        CollectAndCheck(handles, keys);

        int cloneFailureCases = 0;
        for (int ordinal = 1; ordinal <= baseline.Attempts; ordinal++)
        {
            var result = Attempt(root, leaf, ordinal);
            Program.Check(result.Failed && result.Attempts >= ordinal && result.FailedCalls == 1,
                $"provider allocation {ordinal} surfaces exactly one failing facade call");
            if (result.FailedClones != 0) cloneFailureCases++;
            CollectAndCheck(handles, keys);
        }
        // This profile disables ECH; its only transcript clone in a full
        // handshake is send_session_ticket. Both clone ownership boundaries
        // must therefore be reached and safely restored/freed by this sweep.
        Program.Check(cloneFailureCases >= 2, "ticket issuance exercises both failed-clone ownership boundaries and core cleanup");
        Console.WriteLine($"PASS handshake allocation sweep: {baseline.Attempts} provider boundaries, {cloneFailureCases} ticket clone failure cases");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Outcome Attempt(X509Certificate2 root, X509Certificate2 leaf, int ordinal)
    {
        // Credentials, policy contexts and ticket keys are initialized before
        // enabling the per-thread fault. Every attempt has new connections and
        // contexts, all of which are disposed before returning to the caller.
        using var signer = new BclCryptoProvider.SigningIdentity(leaf);
        using var verifier = new BclCryptoProvider.CertificateVerifier([root], X509RevocationMode.NoCheck);
        using var protector = new BclCryptoProvider.TicketProtector(TimeSpan.FromMinutes(5));
        using var serverContext = new PicotlsContext(true, null, signer, cipherSuite: 0x1301, ticketProtector: protector);
        using var clientContext = new PicotlsContext(false, verifier, cipherSuite: 0x1301, saveSessionTickets: true);
        int baseline = BclCryptoProvider.LiveManagedContexts;
        using var fault = ProviderFaultInjection.FailAllocation(ordinal);
        PicotlsConnection? client = null, server = null;
        int failedCalls = 0, tickets = 0;
        bool failed = false;
        var pending = new Queue<(bool ToServer, byte[] Bytes)>();
        try
        {
            client = Create(clientContext, "localhost", fault, ref failedCalls);
            server = Create(serverContext, null, fault, ref failedCalls);
            var initial = Process(client, [], fault, ref failedCalls);
            Program.Check(initial.Consumed == 0 && initial.Plaintext.Length == 0 && initial.Outbound.Length > 0,
                "allocation sweep starts with a real ClientHello and no plaintext");
            pending.Enqueue((true, initial.Outbound));
            int flights = 0;
            while (pending.TryDequeue(out var packet))
            {
                try
                {
                    Program.Check(++flights <= MaximumFlights, "allocation handshake flight count bounded");
                    var result = Process(packet.ToServer ? server : client, packet.Bytes, fault, ref failedCalls);
                    Program.Check(result.Consumed == packet.Bytes.Length && result.Plaintext.Length == 0,
                        "allocation handshake consumes ciphertext without early plaintext");
                    if (result.Outbound.Length != 0) pending.Enqueue((!packet.ToServer, result.Outbound));
                }
                finally { CryptographicOperations.ZeroMemory(packet.Bytes); }
            }
            Program.Check(client.HandshakeComplete && server.HandshakeComplete && !client.IsResumed && !server.IsResumed,
                "allocation baseline authenticates both endpoints with a full TLS 1.3 handshake");
            Program.Check(client.CipherSuite == 0x1301 && server.CipherSuite == 0x1301,
                "allocation handshake uses the selected AES-128-GCM suite");
            var saved = client.TakeSessionTickets();
            try
            {
                tickets = saved.Count;
                Program.Check(tickets > 0 && saved.All(ticket => !ticket.EarlyDataEnabled && !ticket.IsExpired),
                    "allocation baseline receives usable tickets with early data disabled");
            }
            finally { foreach (var ticket in saved) ticket.Dispose(); }
            Program.Check(fault.InjectedFailure == null, "a recorded provider failure can never finish successfully");
        }
        catch (OutOfMemoryException error)
        {
            Program.Check(ReferenceEquals(error, fault.InjectedFailure), "facade preserves the exact first injected allocation exception");
            Program.Check(!error.Data.Contains("PicotlsCleanupFailure"), "failed handshake cleanup has no secondary failure");
            failed = true;
        }
        finally
        {
            while (pending.TryDequeue(out var packet)) CryptographicOperations.ZeroMemory(packet.Bytes);
            try { client?.Dispose(); }
            finally { server?.Dispose(); }
            Program.Check(BclCryptoProvider.LiveManagedContexts == baseline,
                "all connection provider handles are released before context disposal");
        }
        return new(failed, fault.AllocationsAttempted, fault.FailedHashClones, tickets, failedCalls);
    }

    private static PicotlsConnection Create(PicotlsContext context, string? name,
        ProviderFaultInjection.FailureScope fault, ref int failedCalls)
    {
        PicotlsConnection? published = null;
        try
        {
            published = new PicotlsConnection(context, name, default);
            Program.Check(fault.InjectedFailure == null, "connection construction must not publish after callback failure");
            return published;
        }
        catch (OutOfMemoryException)
        {
            failedCalls++;
            Program.Check(published == null, "failed construction publishes no connection");
            throw;
        }
    }

    private static PicotlsStep Process(PicotlsConnection connection, ReadOnlySpan<byte> input,
        ProviderFaultInjection.FailureScope fault, ref int failedCalls)
    {
        PicotlsStep? published = null;
        try
        {
            published = connection.Process(input);
            Program.Check(fault.InjectedFailure == null, "callback failure must not return ciphertext or plaintext from its facade call");
            return published;
        }
        catch (OutOfMemoryException)
        {
            failedCalls++;
            Program.Check(published == null, "the failing facade call publishes neither outbound bytes nor plaintext");
            bool disposed = false;
            try { connection.Process([]); } catch (ObjectDisposedException) { disposed = true; }
            Program.Check(disposed, "failed handshake connection rejects subsequent use");
            throw;
        }
    }

    private static void CollectAndCheck(int handles, int keys)
    {
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        Program.Check(BclCryptoProvider.LiveManagedContexts == handles && BclCryptoProvider.LiveTicketKeysForTesting == keys,
            "attempt disposal and finalizer drain return provider handles and ticket keys to baseline");
    }
}
