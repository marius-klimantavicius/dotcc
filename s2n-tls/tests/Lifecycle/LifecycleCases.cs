using Managed.Security.S2n;
using ManagedConsumer;

namespace ManagedTests;

public static class LifecycleCases
{
    public static readonly string[] Names = ["ownership-freeze", "pending-dispose", "cancel-drain", "io-exception",
        "distinct-concurrency", "same-connection-serialization", "invalid-credentials", "short-io-rc4", "short-io-3des",
        "short-io-tls13", "gc-callbacks", "async-validation", "async-validation-dispose", "validation-exception", "key-log-exception", "hostname-rejection",
        "untrusted-rejection", "explicit-verification-bypass", "bad-mac-rc4", "bad-padding-3des", "bad-tag-tls13", "truncated-eof",
        "connection-reuse", "session-resumption", "serialization-roundtrip", "serialization-rc4-rejection", "tls13-key-update", "alpn",
        "allocation-failures", "init-failure-quarantine"];

    public static async Task RunAsync(string name, string fixtures, string source)
    {
        if (!Names.Contains(name)) throw new ArgumentException("Unknown managed case: " + name);
        switch (name)
        {
            case "ownership-freeze": await OwnershipAsync(); break;
            case "pending-dispose": await PendingAsync(dispose: true); break;
            case "cancel-drain": await PendingAsync(dispose: false); break;
            case "io-exception": await IoExceptionAsync(); break;
            case "distinct-concurrency": await DistinctAsync(); break;
            case "same-connection-serialization": await SerializedAsync(); break;
            case "invalid-credentials": InvalidCredentials(); break;
            case "short-io-rc4": await ShortAsync(S2nProtocol.Ssl3, 4, "RC4-MD5", fixtures, source); break;
            case "short-io-3des": await ShortAsync(S2nProtocol.Ssl3, 10, "DES-CBC3-SHA", fixtures, source); break;
            case "short-io-tls13": await ShortAsync(S2nProtocol.Tls13, 0x1301, "TLS_AES_128_GCM_SHA256", fixtures, source); break;
            case "gc-callbacks":
            {
                int calls = 0;
                await Exchange.LoopbackAsync(S2nProtocol.Tls13, 0x1301, "TLS_AES_128_GCM_SHA256", fixtures, source,
                    (c, s) => c.SetKeyLogger(line => { if (line.Length == 0) throw new Exception("Empty key log."); Interlocked.Increment(ref calls); GC.Collect(); GC.WaitForPendingFinalizers(); }));
                Require(calls > 0, "Key logger did not execute."); break;
            }
            case "async-validation":
            {
                int calls = 0;
                await Exchange.LoopbackAsync(S2nProtocol.Tls13, 0x1301, "TLS_AES_128_GCM_SHA256", fixtures, source,
                    (c, s) => c.SetCertificateValidator(async token => { await Task.Yield(); token.ThrowIfCancellationRequested(); Interlocked.Increment(ref calls); return true; }));
                Require(calls == 1, "Additional validator did not run exactly once."); break;
            }
            case "async-validation-dispose": await ValidationDrainAsync(fixtures, source); break;
            case "validation-exception":
                await ExpectAsync<InvalidDataException>(() => Exchange.LoopbackAsync(S2nProtocol.Tls13, 0x1301, "TLS_AES_128_GCM_SHA256", fixtures, source,
                    (c, s) => { c.SetMaximumBlindingDelay(0).SetCertificateValidator(token => throw new InvalidDataException("validation-sentinel")); s.SetMaximumBlindingDelay(0); }), e => e.Message == "validation-sentinel"); break;
            case "key-log-exception":
                await ExpectAsync<InvalidDataException>(() => Exchange.LoopbackAsync(S2nProtocol.Tls13, 0x1301, "TLS_AES_128_GCM_SHA256", fixtures, source,
                    (c, s) => { c.SetMaximumBlindingDelay(0).SetKeyLogger(_ => throw new InvalidDataException("key-log-sentinel")); s.SetMaximumBlindingDelay(0); }), e => e.Message == "key-log-sentinel"); break;
            case "hostname-rejection":
                await ExpectAsync<S2nException>(() => Exchange.LoopbackAsync(S2nProtocol.Tls13, 0x1301, "TLS_AES_128_GCM_SHA256", fixtures, source,
                    (c, s) => { c.SetMaximumBlindingDelay(0).SetHostnameVerifier(_ => false); s.SetMaximumBlindingDelay(0); }), e => e.ErrorName == "S2N_ERR_CERT_INVALID_HOSTNAME"); break;
            case "untrusted-rejection":
                await ExpectAsync<S2nException>(() => Exchange.LoopbackAsync(S2nProtocol.Tls13, 0x1301, "TLS_AES_128_GCM_SHA256", fixtures, source,
                    (c, s) => { c.SetMaximumBlindingDelay(0).ClearTrustStore(); s.SetMaximumBlindingDelay(0); }), e => e.ErrorName == "S2N_ERR_CERT_UNTRUSTED"); break;
            case "explicit-verification-bypass":
                await Exchange.LoopbackAsync(S2nProtocol.Ssl3, 4, "RC4-MD5", fixtures, source, (c, s) => c.ClearTrustStore().DisableCertificateVerification()); break;
            case "bad-mac-rc4": await RejectedRecordAsync(S2nProtocol.Ssl3, 4, fixtures, source); break;
            case "bad-padding-3des": await RejectedRecordAsync(S2nProtocol.Ssl3, 10, fixtures, source); break;
            case "bad-tag-tls13": await RejectedRecordAsync(S2nProtocol.Tls13, 0x1301, fixtures, source); break;
            case "truncated-eof": await RejectedRecordAsync(S2nProtocol.Tls13, 0x1301, fixtures, source, eof: true); break;
            case "connection-reuse": case "session-resumption": case "serialization-roundtrip":
            case "serialization-rc4-rejection": case "tls13-key-update": case "alpn":
                await StateAsync(name, fixtures, source); break;
            case "allocation-failures": AllocationCases.Failures(); break;
            case "init-failure-quarantine": AllocationCases.InitializationFailure(); break;
        }
    }
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private static async Task ExpectAsync<T>(Func<Task> operation, Func<T, bool>? predicate = null) where T : Exception
    {
        try { await operation(); }
        catch (T error) when (predicate?.Invoke(error) ?? true) { return; }
        throw new InvalidOperationException("Expected " + typeof(T).Name + " with the required error contract.");
    }
    private static async Task OwnershipAsync()
    {
        var before = S2nRuntime.Inspect();
        var runtime = new S2nRuntime();
        var config = new S2nConfiguration(runtime);
        var transport = new PendingTransport();
        await using var connection = new S2nConnection(config, S2nRole.Server, transport);
        Require(config.IsFrozen, "Config did not freeze.");
        try { config.DisableCertificateVerification(); throw new Exception("Frozen config mutated."); }
        catch (InvalidOperationException) { }
        runtime.Dispose(); config.Dispose();
        GC.Collect(); GC.WaitForPendingFinalizers();
        Require(S2nRuntime.Inspect().References > 0, "Early owner disposal cleaned live C objects.");
        await connection.DisposeAsync(); await connection.DisposeAsync();
        Require(transport.Disposals == 1, "Transport did not dispose exactly once.");
        var after = S2nRuntime.Inspect();
        Require(after.References == 0 && after.FinalCleanups == before.FinalCleanups + 1, "cleanup_final did not run after the last owner.");
    }
    private static async Task PendingAsync(bool dispose)
    {
        using var runtime = new S2nRuntime(); using var config = new S2nConfiguration(runtime);
        var transport = new PendingTransport();
        await using var connection = new S2nConnection(config, S2nRole.Server, transport);
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        Task pending = connection.NegotiateAsync(cancellation.Token).AsTask();
        await transport.Waiting.Task.WaitAsync(TimeSpan.FromSeconds(3));
        if (dispose) await connection.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(3)); else cancellation.Cancel();
        await ExpectAsync<OperationCanceledException>(() => pending);
        await connection.DisposeAsync();
        int calls = transport.Calls;
        await ExpectAsync<ObjectDisposedException>(() => connection.NegotiateAsync().AsTask());
        Require(calls == transport.Calls && transport.Disposals == 1, "Callbacks survived disposal.");
    }
    private static async Task IoExceptionAsync()
    {
        using var runtime = new S2nRuntime(); using var config = new S2nConfiguration(runtime);
        config.SetMaximumBlindingDelay(0);
        await using var connection = new S2nConnection(config, S2nRole.Server,
            new PendingTransport { OnReceive = () => throw new InvalidDataException("io-sentinel") });
        await ExpectAsync<InvalidDataException>(() => connection.NegotiateAsync().AsTask(), e => e.Message == "io-sentinel");
    }
    private static async Task DistinctAsync()
    {
        using var runtime = new S2nRuntime(); using var config = new S2nConfiguration(runtime);
        using var barrier = new Barrier(2); using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        Action meet = () => { if (!barrier.SignalAndWait(TimeSpan.FromSeconds(2))) throw new InvalidOperationException("Distinct C entries did not overlap."); };
        var a = new PendingTransport { OnReceive = meet }; var b = new PendingTransport { OnReceive = meet };
        await using var left = new S2nConnection(config, S2nRole.Server, a);
        await using var right = new S2nConnection(config, S2nRole.Server, b);
        Task x = Task.Run(async () => await left.NegotiateAsync(cancellation.Token));
        Task y = Task.Run(async () => await right.NegotiateAsync(cancellation.Token));
        await Task.WhenAll(a.Waiting.Task, b.Waiting.Task).WaitAsync(TimeSpan.FromSeconds(4));
        cancellation.Cancel();
        await ExpectAsync<OperationCanceledException>(() => x); await ExpectAsync<OperationCanceledException>(() => y);
        Require(a.Calls == 1 && b.Calls == 1, "Unexpected concurrent callback count.");
    }
    private static async Task SerializedAsync()
    {
        using var runtime = new S2nRuntime(); using var config = new S2nConfiguration(runtime);
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var transport = new PendingTransport();
        await using var connection = new S2nConnection(config, S2nRole.Server, transport);
        Task first = connection.NegotiateAsync(cancellation.Token).AsTask();
        await transport.Waiting.Task.WaitAsync(TimeSpan.FromSeconds(3));
        Task second = connection.NegotiateAsync(cancellation.Token).AsTask();
        Require(!second.IsCompleted && transport.Calls == 1, "A second entry bypassed per-connection serialization.");
        cancellation.Cancel();
        await ExpectAsync<OperationCanceledException>(() => first); await ExpectAsync<OperationCanceledException>(() => second);
    }
    private static void InvalidCredentials()
    {
        using var runtime = new S2nRuntime();
        try { using var invalid = new S2nCredentials(runtime, "not PEM", "not a key"); }
        catch (S2nException) { return; }
        throw new InvalidOperationException("Malformed PEM was accepted.");
    }
    private static Task ShortAsync(S2nProtocol protocol, ushort suite, string name, string fixtures, string source)
        => Exchange.LoopbackAsync(protocol, suite, name, fixtures, source, transport: s => new ShortTransport(s));
    private static async Task ValidationDrainAsync(string fixtures, string source)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var sockets = await Exchange.SocketsAsync(timeout.Token);
        using var clientSocket = sockets.Client; using var serverSocket = sockets.Server;
        using var runtime = new S2nRuntime();
        using var cc = Exchange.Configuration(runtime, S2nRole.Client, fixtures, source).SetMaximumBlindingDelay(0);
        using var sc = Exchange.Configuration(runtime, S2nRole.Server, fixtures, source).SetMaximumBlindingDelay(0);
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var drained = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        cc.SetCertificateValidator(async token =>
        {
            started.TrySetResult();
            try { await Task.Delay(Timeout.Infinite, token); return true; }
            finally { drained.TrySetResult(); }
        });
        await using var client = new S2nConnection(cc, S2nRole.Client, clientSocket, new(S2nProtocol.Tls13, S2nProtocol.Tls13, 0x1301));
        await using var server = new S2nConnection(sc, S2nRole.Server, serverSocket, new(S2nProtocol.Tls13, S2nProtocol.Tls13, 0x1301));
        client.SetServerName("localhost");
        Task clientHandshake = client.NegotiateAsync(timeout.Token).AsTask();
        Task serverHandshake = server.NegotiateAsync(timeout.Token).AsTask();
        try
        {
            await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await client.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(3));
            Require(drained.Task.IsCompletedSuccessfully, "Async certificate callback survived disposal.");
            await ExpectAsync<OperationCanceledException>(() => clientHandshake);
        }
        finally
        {
            timeout.Cancel();
            try { await serverHandshake; }
            catch (OperationCanceledException) { }
            catch (S2nException error) when (error.ErrorType is 1 or 2) { }
            catch (System.Net.Sockets.SocketException error) when
                (error.SocketErrorCode is System.Net.Sockets.SocketError.ConnectionReset or System.Net.Sockets.SocketError.ConnectionAborted) { }
        }
    }
    private static async Task RejectedRecordAsync(S2nProtocol protocol, ushort suite, string fixtures, string source, bool eof = false)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var sockets = await Exchange.SocketsAsync(timeout.Token);
        using var clientSocket = sockets.Client; using var serverSocket = sockets.Server;
        using var runtime = new S2nRuntime();
        using var cc = Exchange.Configuration(runtime, S2nRole.Client, fixtures, source).SetMaximumBlindingDelay(0);
        using var sc = Exchange.Configuration(runtime, S2nRole.Server, fixtures, source).SetMaximumBlindingDelay(0);
        // The CBC identity makes record[-9] ^= 0x80 change the final
        // plaintext padding byte 2 to 130, so 24 - 20 - (130 + 1)
        // deterministically underflows, matching the native control.
        var tamper = new CorruptTransport(clientSocket, suite == 0x000a ? 8 : 0);
        await using var client = new S2nConnection(cc, S2nRole.Client, tamper, new(protocol, protocol, suite));
        await using var server = new S2nConnection(sc, S2nRole.Server, serverSocket, new(protocol, protocol, suite));
        client.SetServerName("localhost");
        await Task.WhenAll(client.NegotiateAsync(timeout.Token).AsTask(), server.NegotiateAsync(timeout.Token).AsTask());
        if (eof) clientSocket.Shutdown(System.Net.Sockets.SocketShutdown.Send);
        else { tamper.Corrupt = true; await client.SendAsync(Exchange.Payload(0), timeout.Token); }
        await ExpectAsync<S2nException>(async () => { await server.ReceiveAsync(new byte[2048], timeout.Token); },
            e => e.ErrorName == (eof ? "S2N_ERR_CLOSED" : suite == 0x0004 ? "S2N_ERR_BAD_MESSAGE" :
                suite == 0x000a ? "S2N_ERR_INTEGER_OVERFLOW" : "S2N_ERR_DECRYPT"));
    }

    private static async Task StateAsync(string name, string fixtures, string source)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        var sockets = await Exchange.SocketsAsync(timeout.Token);
        using var clientSocket = sockets.Client; using var serverSocket = sockets.Server;
        using var runtime = new S2nRuntime();
        using var cc = Exchange.Configuration(runtime, S2nRole.Client, fixtures, source).SetMaximumBlindingDelay(0);
        using var sc = Exchange.Configuration(runtime, S2nRole.Server, fixtures, source).SetMaximumBlindingDelay(0);
        if (name.StartsWith("serialization", StringComparison.Ordinal)) { cc.EnableSerialization(true); sc.EnableSerialization(true); }
        if (name == "session-resumption")
        {
            cc.EnableSessionTickets(true); sc.EnableSessionTickets(true);
            byte[] key = System.Security.Cryptography.RandomNumberGenerator.GetBytes(32);
            try { sc.AddSessionTicketKey("s2n-test-key"u8, key, DateTimeOffset.UtcNow.AddSeconds(-1)); }
            finally { System.Security.Cryptography.CryptographicOperations.ZeroMemory(key); }
        }
        if (name == "alpn") { cc.SetApplicationProtocols("test/1"); sc.SetApplicationProtocols("test/1"); }
        S2nProtocol protocol = name is "connection-reuse" or "serialization-rc4-rejection" ? S2nProtocol.Ssl3
            : name == "session-resumption" ? S2nProtocol.Tls12 : S2nProtocol.Tls13;
        ushort suite = protocol == S2nProtocol.Ssl3 ? (ushort)4 : protocol == S2nProtocol.Tls12 ? (ushort)0x009c : (ushort)0x1301;
        using var transport = new S2nSocketTransport(clientSocket);
        await using var client = new S2nConnection(cc, S2nRole.Client, transport, new(protocol, protocol, suite), ownsTransport: false);
        await using var server = new S2nConnection(sc, S2nRole.Server, serverSocket, new(protocol, protocol, suite));
        client.SetServerName("localhost");
        async Task Handshake(S2nConnection c, S2nConnection s)
        {
            await Task.WhenAll(c.NegotiateAsync(timeout.Token).AsTask(), s.NegotiateAsync(timeout.Token).AsTask());
            Require(c.Negotiated.Protocol == protocol && c.Negotiated.CipherSuite == suite && s.Negotiated.Protocol == protocol && s.Negotiated.CipherSuite == suite, "Wrong state-test negotiation.");
        }
        async Task Transfer(S2nConnection c, S2nConnection s)
        {
            async Task Client() { await Exchange.SendAsync(c, 0, timeout.Token); await Exchange.ReceiveAsync(c, 1, timeout.Token); }
            async Task Server() { await Exchange.ReceiveAsync(s, 0, timeout.Token); await Exchange.SendAsync(s, 1, timeout.Token); }
            await Task.WhenAll(Client(), Server());
        }
        async Task Shutdown(S2nConnection c, S2nConnection s)
        {
            await Task.WhenAll(c.ShutdownAsync(timeout.Token).AsTask(), s.ShutdownAsync(timeout.Token).AsTask());
            Require(c.ShutdownComplete && s.ShutdownComplete && c.PeerClosed && s.PeerClosed, "Missing close_notify in state test.");
        }
        await Handshake(client, server);
        if (name == "serialization-rc4-rejection")
        {
            try { client.ExportUnprotectedState(); }
            catch (S2nException error) when (error.ErrorName == "S2N_ERR_INVALID_STATE")
            { await Transfer(client, server); await Shutdown(client, server); return; }
            throw new InvalidOperationException("RC4 state serialization was accepted.");
        }
        if (name == "serialization-roundtrip")
        {
            byte[] state = client.ExportUnprotectedState();
            await client.DisposeAsync();
            await using var restored = new S2nConnection(cc, S2nRole.Client, transport, new(protocol, protocol, suite), ownsTransport: false);
            try { restored.ImportUnprotectedState(state); }
            finally { System.Security.Cryptography.CryptographicOperations.ZeroMemory(state); }
            await Transfer(restored, server); await Shutdown(restored, server); return;
        }
        byte[] session = name == "session-resumption" ? client.ExportSession() : [];
        if (name == "alpn") Require(client.Negotiated.ApplicationProtocol == "test/1" && server.Negotiated.ApplicationProtocol == "test/1", "ALPN did not negotiate.");
        if (name == "tls13-key-update")
        {
            try { client.RequestKeyUpdate(requestPeerUpdate: true); throw new InvalidOperationException("Unsupported peer key-update request was accepted."); }
            catch (S2nException error) when (error.ErrorName == "S2N_ERR_INVALID_ARGUMENT") { }
            client.RequestKeyUpdate(); server.RequestKeyUpdate();
        }
        await Transfer(client, server);
        if (name == "tls13-key-update") Require(client.KeyUpdates.Sent > 0 && client.KeyUpdates.Received > 0 && server.KeyUpdates.Sent > 0 && server.KeyUpdates.Received > 0, "Key update was not performed in both directions.");
        await Shutdown(client, server);
        if (name is "connection-reuse" or "session-resumption")
        {
            client.Reset(); server.Reset(); client.SetServerName("localhost");
            if (name == "session-resumption")
            {
                Require(session.Length > 0, "No session ticket exported.");
                try { client.ImportSession(session); }
                finally { System.Security.Cryptography.CryptographicOperations.ZeroMemory(session); }
            }
            await Handshake(client, server);
            if (name == "session-resumption") Require(client.Negotiated.Resumed && server.Negotiated.Resumed, "Session was not resumed.");
            await Transfer(client, server); await Shutdown(client, server);
        }
    }
}
