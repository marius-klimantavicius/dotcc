using System.Net;
using System.Security.Cryptography.X509Certificates;
using Managed.Transport.Api;

// Actual public credential policies and translated endpoint callbacks. Internal
// access is used only for the existing harness's final ownership assertions.
internal static class CertificatePolicies
{
    private static readonly TimeSpan Limit = TimeSpan.FromSeconds(20);
    private sealed class ApplicationPolicyFailure : Exception
    { internal ApplicationPolicyFailure() : base("Intentional application certificate policy failure") { } }

    private sealed class Policy(bool asynchronous, bool approve, bool throwPolicyError = false)
    {
        internal readonly TaskCompletionSource<QuicCertificateValidation> Entered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal readonly TaskCompletionSource<bool> Decision = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal readonly TaskCompletionSource Returned = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal readonly TaskCompletionSource Canceled = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal int Calls;

        internal ValueTask<bool> Invoke(QuicCertificateValidation observation, CancellationToken cancellation)
        {
            Interlocked.Increment(ref Calls);
            Entered.TrySetResult(observation);
            if (cancellation.IsCancellationRequested) Canceled.TrySetResult();
            if (asynchronous) return Delayed(cancellation);
            Returned.TrySetResult();
            if (throwPolicyError) throw new ApplicationPolicyFailure();
            return ValueTask.FromResult(approve);
        }
        private async ValueTask<bool> Delayed(CancellationToken cancellation)
        {
            using var registration = cancellation.Register(() => Canceled.TrySetResult());
            // Deliberately ignore cancellation for the application decision.
            // The owner must drain without waiting for arbitrary user work.
            try
            {
                bool value = await Decision.Task.ConfigureAwait(false);
                if (throwPolicyError) throw new ApplicationPolicyFailure();
                return value;
            }
            finally { Returned.TrySetResult(); }
        }
    }

    internal static async Task RunAsync()
    {
        using var certificates = new Credentials();
        using var unrelatedRoot = Credentials.CreateRoot("CN=unrelated certificate policy root");
        foreach (var address in new[] { IPAddress.Loopback, IPAddress.IPv6Loopback })
        {
            foreach (bool asynchronous in new[] { false, true })
            {
                await DecisionAsync(certificates, unrelatedRoot, address, "trusted", asynchronous, approve: true);
                await DecisionAsync(certificates, unrelatedRoot, address, "trusted", asynchronous, approve: false);
                await DecisionAsync(certificates, unrelatedRoot, address, "wrong-name", asynchronous, approve: true);
                await DecisionAsync(certificates, unrelatedRoot, address, "untrusted", asynchronous, approve: true);
                await DecisionAsync(certificates, unrelatedRoot, address, "trusted", asynchronous, approve: true, throwPolicyError: true);
            }
            await CloseWhilePendingAsync(certificates, address, cancelConnect: false);
            await CloseWhilePendingAsync(certificates, address, cancelConnect: true);
        }
        Console.WriteLine("PASS facade certificate policies: sync/async approve/reject/throw; native name/trust failure cannot be approved; pending close/cancel and late approval; owned DER/peer PKCS7; zero rejected app bytes; IPv4/IPv6 ECDSA AES128");
    }

    private static QuicSettings Settings() => new()
    {
        PeerBidiStreamCount = 4, PeerUnidiStreamCount = 4,
        HandshakeIdleTimeoutMs = 15000, IdleTimeoutMs = 30000,
        SendBufferingEnabled = false
    };

    private static async Task DecisionAsync(Credentials certificates, X509Certificate2 unrelatedRoot,
        IPAddress address, string trust, bool asynchronous, bool approve, bool throwPolicyError = false)
    {
        int contextBaseline = QuicObject.LiveContextCount;
        var policy = new Policy(asynchronous, approve, throwPolicyError);
        QuicCertificateValidation? retained = null;
        await using var runtime = await QuicRuntime.CreateAsync(new() { ProcessorCount = 2 });
        await using var serverRegistration = await runtime.OpenRegistrationAsync("certificate-policy-server");
        await using var clientRegistration = await runtime.OpenRegistrationAsync("certificate-policy-client");
        using var serverCredentials = QuicCredentials.Server(certificates.EcLeaf, cipherSuite: 0x1301);
        using var clientCredentials = QuicCredentials.Client([trust == "untrusted" ? unrelatedRoot : certificates.Root],
            X509RevocationMode.NoCheck, cipherSuite: 0x1301, validateCertificate: policy.Invoke);
        await using var serverConfiguration = await serverRegistration.CreateConfigurationAsync(["certificate-policies"u8.ToArray()], serverCredentials, Settings());
        await using var clientConfiguration = await clientRegistration.CreateConfigurationAsync(["certificate-policies"u8.ToArray()], clientCredentials, Settings());
        await using var listener = await serverRegistration.ListenAsync(serverConfiguration, new(address, 0));
        using var timeout = new CancellationTokenSource(Limit);
        using var acceptCancellation = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token);
        Task<QuicConnection> accepted = listener.AcceptConnectionAsync(acceptCancellation.Token).AsTask();
        Task<QuicConnection> connecting = clientRegistration.ConnectAsync(clientConfiguration,
            trust == "wrong-name" ? "wrong.example.invalid" : "localhost", listener.LocalEndPoint, timeout.Token).AsTask();
        QuicConnection? client = null, server = null;
        try
        {
            retained = await policy.Entered.Task.WaitAsync(Limit);
            CheckCopies(retained, certificates);
            uint expectedStatus = trust switch { "wrong-name" => 200000298, "untrusted" => 200000514, _ => 0 };
            Check(retained.ValidationStatus == expectedStatus, $"Actual native certificate status was lost: {trust} {retained.ValidationStatus}");
            Check(Volatile.Read(ref policy.Calls) == 1, "Certificate policy invoked more than once");
            if (asynchronous && trust == "trusted")
            {
                Check(!connecting.IsCompleted, "Trusted handshake completed while application certificate approval was pending");
                NoApplicationBytes(runtime);
                Collect();
                CheckCopies(retained, certificates);
                policy.Decision.SetResult(approve);
            }

            if (trust == "trusted" && approve && !throwPolicyError)
            {
                client = await connecting.WaitAsync(Limit);
                server = await accepted.WaitAsync(Limit);
                Check(client.GetHandshakeInformation().CipherSuite == QuicTlsCipherSuite.Aes128GcmSha256 &&
                    client.GetHandshakeInformation().NamedGroup == QuicTlsNamedGroup.Secp256R1 &&
                    client.GetProtocolVersion() == 1, "Approved connection did not negotiate the requested profile");
                await Transfer(client, server, "approved-client"u8.ToArray());
                await Transfer(server, client, "approved-server"u8.ToArray());
                Check(client.CallbackFailure == null && server.CallbackFailure == null, "Valid certificate policy faulted a callback");
            }
            else
            {
                // For a failed native trust/name check, keep an asynchronous
                // approval pending until the real TLS failure has completed.
                // Approval after that failure must never revive a connection.
                ushort alert = trust == "untrusted" ? (ushort)48 : (ushort)42;
                await RequireTlsFailure(connecting, alert);
                NoApplicationBytes(runtime);
                if (asynchronous && trust != "trusted") policy.Decision.SetResult(true);
                await policy.Returned.Task.WaitAsync(Limit);
                acceptCancellation.Cancel();
                server = await SettleAccept(accepted);
                if (server != null)
                {
                    await server.ShutdownCompletion.WaitAsync(Limit);
                    Check(server.GetStatistics().SentStreamBytes == 0 && server.GetStatistics().ReceivedStreamBytes == 0,
                        "Rejected certificate produced server stream bytes");
                }
                NoApplicationBytes(runtime);
            }
            Check(Volatile.Read(ref policy.Calls) == 1, "Certificate failure retriggered application policy");
        }
        finally
        {
            policy.Decision.TrySetResult(false);
            acceptCancellation.Cancel();
            // Observe any factory outcome before ending its owning runtime.
            if (client == null && connecting.IsCompletedSuccessfully) client = connecting.Result;
            if (client != null) await client.DisposeAsync().AsTask().WaitAsync(Limit);
            if (server != null) await server.DisposeAsync().AsTask().WaitAsync(Limit);
            if (server == null) { server = await SettleAccept(accepted); if (server != null) await server.DisposeAsync().AsTask().WaitAsync(Limit); }
            await runtime.DisposeAsync().AsTask().WaitAsync(Limit);
            try { await connecting.WaitAsync(Limit); }
            catch (Exception error) when (error is IOException or OperationCanceledException or ObjectDisposedException) { }
        }
        Drained(runtime, contextBaseline);
        Collect();
        CheckCopies(retained ?? throw new InvalidOperationException("Certificate policy never received copied data"), certificates);
        Console.WriteLine($"PASS certificate policy {address.AddressFamily} {trust} {(asynchronous ? "async" : "sync")} {(throwPolicyError ? "throw" : approve ? "approve" : "reject")}");
    }

    private static async Task CloseWhilePendingAsync(Credentials certificates, IPAddress address, bool cancelConnect)
    {
        int contextBaseline = QuicObject.LiveContextCount;
        var policy = new Policy(asynchronous: true, approve: true);
        await using var runtime = await QuicRuntime.CreateAsync(new() { ProcessorCount = 2 });
        await using var serverRegistration = await runtime.OpenRegistrationAsync("pending-policy-server");
        await using var clientRegistration = await runtime.OpenRegistrationAsync("pending-policy-client");
        using var serverCredentials = QuicCredentials.Server(certificates.EcLeaf, cipherSuite: 0x1301);
        using var clientCredentials = QuicCredentials.Client([certificates.Root], X509RevocationMode.NoCheck,
            cipherSuite: 0x1301, validateCertificate: policy.Invoke);
        await using var serverConfiguration = await serverRegistration.CreateConfigurationAsync(["pending-certificate"u8.ToArray()], serverCredentials, Settings());
        await using var clientConfiguration = await clientRegistration.CreateConfigurationAsync(["pending-certificate"u8.ToArray()], clientCredentials, Settings());
        await using var listener = await serverRegistration.ListenAsync(serverConfiguration, new(address, 0));
        using var cancellation = new CancellationTokenSource();
        Task<QuicConnection> connecting = clientRegistration.ConnectAsync(clientConfiguration, "localhost", listener.LocalEndPoint, cancellation.Token).AsTask();
        QuicCertificateValidation? retained = null;
        try
        {
            retained = await policy.Entered.Task.WaitAsync(Limit);
            Check(retained.ValidationStatus == 0 && !connecting.IsCompleted, "Pending policy did not hold an otherwise valid handshake");
            CheckCopies(retained, certificates);
            NoApplicationBytes(runtime);
            if (cancelConnect)
            {
                cancellation.Cancel();
                try { await using var unexpected = await connecting.WaitAsync(Limit); throw new InvalidOperationException("Canceled pending connection was published"); }
                catch (OperationCanceledException error) { Check(error.CancellationToken == cancellation.Token, "Connection cancellation lost the caller token"); }
            }
            else
            {
                await clientRegistration.DisposeAsync().AsTask().WaitAsync(Limit);
                try { await using var unexpected = await connecting.WaitAsync(Limit); throw new InvalidOperationException("Closed pending connection was published"); }
                catch (Exception error) when (error is IOException or OperationCanceledException or ObjectDisposedException) { }
            }
            await policy.Canceled.Task.WaitAsync(Limit);
            Check(!policy.Returned.Task.IsCompleted, "Ignoring-cancellation policy unexpectedly finished without a decision");
            NoApplicationBytes(runtime);
            await runtime.DisposeAsync().AsTask().WaitAsync(Limit);
            Drained(runtime, contextBaseline);
            Collect(); CheckCopies(retained, certificates);
            policy.Decision.SetResult(true); // Application approves after native/core/host owners have retired.
            await policy.Returned.Task.WaitAsync(Limit);
            // A subsequent scheduled continuation and collection give the
            // facade's no-op-after-close path a chance to settle. They are not
            // counted as a synthetic native completion or a TLS failure.
            await Task.Yield(); Collect();
            Drained(runtime, contextBaseline);
            CheckCopies(retained, certificates);
            Check(Volatile.Read(ref policy.Calls) == 1, "Late approval re-entered certificate policy");
        }
        finally
        {
            policy.Decision.TrySetResult(false);
            cancellation.Cancel();
            await runtime.DisposeAsync().AsTask().WaitAsync(Limit);
            try { await using var unexpected = await connecting.WaitAsync(Limit); }
            catch (Exception error) when (error is IOException or OperationCanceledException or ObjectDisposedException) { }
        }
        Console.WriteLine($"PASS certificate policy {address.AddressFamily} {(cancelConnect ? "cancel" : "close")} while pending, late approval and copied data after drain");
    }

    private static async Task RequireTlsFailure(Task<QuicConnection> operation, ushort alert)
    {
        try { await using var unexpected = await operation.WaitAsync(Limit); }
        catch (QuicTransportException error)
        {
            Check(error.TlsAlert == alert && error.TransportError == (ulong)(0x100 + alert),
                $"Certificate rejection lost actual TLS alert{alert}: status={error.Status} error={error.TransportError}");
            return;
        }
        throw new InvalidOperationException("Certificate policy rejection published a connection");
    }

    private static async Task<QuicConnection?> SettleAccept(Task<QuicConnection> operation)
    {
        try { return await operation.WaitAsync(Limit); }
        catch (Exception error) when (error is IOException or OperationCanceledException or ObjectDisposedException) { return null; }
    }

    private static async Task Transfer(QuicConnection sender, QuicConnection receiver, byte[] expected)
    {
        await using var writer = await sender.OpenStreamAsync(QuicStreamOpenOptions.Unidirectional).AsTask().WaitAsync(Limit);
        Task sending = writer.SendAsync(expected, QuicSendOptions.Fin).AsTask();
        await using var reader = await receiver.AcceptStreamAsync().AsTask().WaitAsync(Limit);
        byte[] actual = new byte[expected.Length]; int offset = 0;
        while (offset < actual.Length)
        {
            int count = await reader.ReadAsync(actual.AsMemory(offset)).AsTask().WaitAsync(Limit);
            Check(count != 0, "Approved transfer ended before its complete payload");
            offset += count;
        }
        Check(actual.AsSpan().SequenceEqual(expected) && await reader.ReadAsync(new byte[1]).AsTask().WaitAsync(Limit) == 0,
            "Approved transfer data/FIN mismatch");
        await sending.WaitAsync(Limit);
        await writer.CompleteWritesAsync().AsTask().WaitAsync(Limit);
    }

    private static void CheckCopies(QuicCertificateValidation observation, Credentials certificates)
    {
        Check(observation.CertificateDer.Span.SequenceEqual(certificates.EcLeaf.RawData), "Owned DER copy changed after callback/close");
        byte[] encoded = observation.PeerChainPkcs7.ToArray();
        Check(X509Certificate2.GetCertContentType(encoded) == X509ContentType.Pkcs7, "Peer-sent chain is not actual PKCS7");
        var chain = new X509Certificate2Collection();
        try
        {
#pragma warning disable SYSLIB0057 // Collection.Import is the BCL PKCS7 decoder; no PKCS7 loader replaces it.
            chain.Import(encoded);
#pragma warning restore SYSLIB0057
            // Server sends only its leaf. The trust root must not be silently
            // inserted by rebuilding a chain from the client's trust store.
            Check(chain.Count == 1 && chain[0].RawData.AsSpan().SequenceEqual(certificates.EcLeaf.RawData) &&
                !chain.Cast<X509Certificate2>().Any(item => item.RawData.AsSpan().SequenceEqual(certificates.Root.RawData)),
                "PKCS7 did not preserve exactly the peer-sent certificate set");
        }
        finally { foreach (var certificate in chain) certificate.Dispose(); }
    }
    private static void NoApplicationBytes(QuicRuntime runtime)
    {
        var counters = runtime.GetPerformanceCounters();
        Check(counters[QuicPerformanceCounter.ApplicationSentBytes] == 0 && counters[QuicPerformanceCounter.ApplicationReceivedBytes] == 0,
            "Rejected or pending certificate policy admitted actual core application bytes");
    }
    private static void Drained(QuicRuntime runtime, int contextBaseline) => Check(
        QuicObject.LiveContextCount == contextBaseline && runtime.Host.OutstandingResources == 0 &&
        runtime.Host.OutstandingPlatformAllocations == 0 && runtime.Host.OutstandingDatagramReceives == 0,
        "Certificate policy left native context, host resource, allocation or receive ownership live");
    private static void Collect() { GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect(); }
    private static void Check(bool condition, string message)
    { if (!condition) throw new InvalidOperationException(message); }
}
