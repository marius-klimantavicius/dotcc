using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Managed.Net.Quic;

internal static class Program
{
    private static IPAddress Loopback = IPAddress.Loopback;
    private static readonly SslApplicationProtocol Alpn = new("dotcc-system-quic");
    private static void Check(bool value, string message) { if (!value) throw new Exception(message); }

    private static X509Certificate2 Issue(X509Certificate2 root, string name, string eku)
    {
        using var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var request = new CertificateRequest("CN=" + name, key, HashAlgorithmName.SHA256);
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, true));
        request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new Oid(eku) }, true));
        var san = new SubjectAlternativeNameBuilder(); san.AddDnsName(name); san.AddIpAddress(Loopback);
        request.CertificateExtensions.Add(san.Build());
        using var issued = request.Create(root, DateTimeOffset.UtcNow.AddDays(-1), new DateTimeOffset(root.NotAfter.ToUniversalTime()).AddMinutes(-1), RandomNumberGenerator.GetBytes(16));
        return issued.CopyWithPrivateKey(key);
    }

    private static X509ChainPolicy Policy(X509Certificate2 root) => new()
    {
        TrustMode = X509ChainTrustMode.CustomRootTrust,
        CustomTrustStore = { root }, RevocationMode = X509RevocationMode.NoCheck,
        DisableCertificateDownloads = true
    };

    private static async Task RoundTrip(X509Certificate2 root, X509Certificate2 serverCertificate,
        X509Certificate2 clientCertificate, string scenario, X509Certificate2? intermediate = null)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        CancellationToken ct = timeout.Token;
        bool mutual = scenario.StartsWith("mutual");
        bool missing = scenario.Contains("missing");
        bool reject = scenario.Contains("reject") || scenario.Contains("throw");
        bool untrusted = scenario.Contains("untrusted");
        bool wrongName = scenario.StartsWith("name-");
        bool callbackOverride = scenario.Contains("override");
        int clientCallbacks = 0, serverCallbacks = 0, selections = 0;
        var serverOptions = new QuicServerConnectionOptions
        {
            DefaultCloseErrorCode = 0, DefaultStreamErrorCode = 0,
            MaxInboundBidirectionalStreams = 8,
            ServerAuthenticationOptions = new()
            {
                ApplicationProtocols = [Alpn], ServerCertificate = serverCertificate,
                ClientCertificateRequired = mutual, CertificateChainPolicy = Policy(root),
                RemoteCertificateValidationCallback = (_, certificate, chain, errors) =>
                {
                    Interlocked.Increment(ref serverCallbacks);
                    Check(missing ? certificate == null && errors == SslPolicyErrors.RemoteCertificateNotAvailable
                        : certificate != null && chain != null && errors == SslPolicyErrors.None,
                        "unexpected client certificate validation: " + errors);
                    return !reject && (!missing || callbackOverride);
                }
            }
        };
        if (scenario is "server-context" or "intermediate-context")
        {
            serverOptions.ServerAuthenticationOptions.ServerCertificate = null;
            serverOptions.ServerAuthenticationOptions.ServerCertificateContext = SslStreamCertificateContext.Create(serverCertificate, new() { intermediate ?? root }, offline: true);
        }
        if (scenario == "server-selection")
        {
            serverOptions.ServerAuthenticationOptions.ServerCertificate = null;
            serverOptions.ServerAuthenticationOptions.ServerCertificateSelectionCallback = (_, name) =>
            { Check(name == "localhost", "server SNI selection"); selections++; return serverCertificate; };
        }
        await using var listener = await QuicListener.ListenAsync(new()
        {
            ListenEndPoint = new IPEndPoint(Loopback, 0), ApplicationProtocols = [Alpn],
            ConnectionOptionsCallback = (_, _, _) => ValueTask.FromResult(serverOptions)
        }, ct);
        var clientOptions = new QuicClientConnectionOptions
        {
            RemoteEndPoint = listener.LocalEndPoint, DefaultCloseErrorCode = 0, DefaultStreamErrorCode = 0,
            MaxInboundBidirectionalStreams = 8,
            ClientAuthenticationOptions = new()
            {
                ApplicationProtocols = [Alpn], TargetHost = wrongName ? "wrong.example" : scenario == "ip-name" ? Loopback.ToString() : scenario == "idn-name" ? "bücher.example" : "localhost",
                CertificateChainPolicy = Policy(root),
                ClientCertificates = mutual && !missing ? new X509CertificateCollection { clientCertificate } : null
            }
        };
        if (untrusted)
            clientOptions.ClientAuthenticationOptions.CertificateChainPolicy = new X509ChainPolicy
            { RevocationMode = X509RevocationMode.NoCheck, DisableCertificateDownloads = true };
        if (scenario == "mutual-context")
        {
            clientOptions.ClientAuthenticationOptions.ClientCertificates = null;
            clientOptions.ClientAuthenticationOptions.ClientCertificateContext = SslStreamCertificateContext.Create(clientCertificate, new() { root }, offline: true);
        }
        if (scenario == "mutual-selection")
            clientOptions.ClientAuthenticationOptions.LocalCertificateSelectionCallback = (_, name, _, _, _) =>
            { Check(name == "localhost", "client selection target"); selections++; return clientCertificate; };
        if (scenario == "aes256")
        {
#pragma warning disable CA1416
            clientOptions.ClientAuthenticationOptions.CipherSuitesPolicy = new CipherSuitesPolicy([TlsCipherSuite.TLS_CHACHA20_POLY1305_SHA256, TlsCipherSuite.TLS_AES_256_GCM_SHA384]);
#pragma warning restore CA1416
        }
        if (callbackOverride || scenario.StartsWith("callback"))
            clientOptions.ClientAuthenticationOptions.RemoteCertificateValidationCallback = (_, certificate, chain, errors) =>
            {
                Interlocked.Increment(ref clientCallbacks);
                Check(certificate != null && chain != null, "missing server certificate");
                Check(errors == (wrongName ? SslPolicyErrors.RemoteCertificateNameMismatch : (untrusted || scenario.Contains("eku")) ? SslPolicyErrors.RemoteCertificateChainErrors : SslPolicyErrors.None),
                    "unexpected server certificate validation: " + errors);
                if (scenario == "callback-throw") throw new InvalidOperationException("validation callback failed");
                return scenario != "callback-reject";
            };
        Task<QuicConnection> accept = listener.AcceptConnectionAsync(ct).AsTask();
        QuicConnection? client = null, server = null;
        bool expectedFailure = reject || (wrongName && !callbackOverride) || (missing && !callbackOverride) || (untrusted && !callbackOverride);
        try
        {
            client = await QuicConnection.ConnectAsync(clientOptions, ct);
            server = await accept;
            Check(!expectedFailure, "invalid certificate accepted");
            Check(client.NegotiatedApplicationProtocol == Alpn, "ALPN mismatch");
            Check(client.NegotiatedCipherSuite is TlsCipherSuite.TLS_AES_128_GCM_SHA256 or TlsCipherSuite.TLS_AES_256_GCM_SHA384,
                "cipher mismatch");
            if (scenario == "aes256") Check(client.NegotiatedCipherSuite == TlsCipherSuite.TLS_AES_256_GCM_SHA384, "AES256 policy ignored");
            await using var outgoing = await client.OpenOutboundStreamAsync(QuicStreamType.Bidirectional, ct);
            byte[] payload = "hello translated Managed.Net.Quic"u8.ToArray();
            await outgoing.WriteAsync(payload, completeWrites: true, ct);
            await using var incoming = await server.AcceptInboundStreamAsync(ct);
            byte[] received = new byte[payload.Length];
            await incoming.ReadExactlyAsync(received, ct);
            Check(received.AsSpan().SequenceEqual(payload), "stream data mismatch");
            Check(await incoming.ReadAsync(new byte[1], ct) == 0, "missing FIN");
            await incoming.WriteAsync(received, completeWrites: true, ct);
            await outgoing.ReadExactlyAsync(received, ct);
            Check(await outgoing.ReadAsync(new byte[1], ct) == 0, "missing reply FIN");
        }
        catch (Exception e) when (expectedFailure && (e is AuthenticationException || e is QuicException)) { }
        finally
        {
            if (client != null) await client.DisposeAsync();
            if (server != null) await server.DisposeAsync();
            timeout.Cancel();
            if (server == null)
            {
                try { await using var pending = await accept; }
                catch (Exception e) when (e is OperationCanceledException || e is QuicException || e is AuthenticationException) { }
            }
        }
        if (mutual) Check(serverCallbacks == 1, "client certificate callback not invoked exactly once");
        if (callbackOverride || scenario.StartsWith("callback"))
            Check(clientCallbacks == 1, "server certificate callback not invoked exactly once");
        if (scenario.Contains("selection")) Check(selections == 1, "selection callback not invoked exactly once");
        Console.WriteLine("PASS " + scenario);
    }

    private static async Task CancelValidation(X509Certificate2 root, X509Certificate2 certificate)
    {
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        using var cancelConnect = CancellationTokenSource.CreateLinkedTokenSource(deadline.Token);
        using var release = new ManualResetEventSlim();
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var finished = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var listener = await QuicListener.ListenAsync(new()
        {
            ListenEndPoint = new IPEndPoint(Loopback, 0), ApplicationProtocols = [Alpn],
            ConnectionOptionsCallback = (_, _, _) => ValueTask.FromResult(new QuicServerConnectionOptions
            {
                DefaultCloseErrorCode = 0, DefaultStreamErrorCode = 0,
                ServerAuthenticationOptions = new() { ApplicationProtocols = [Alpn], ServerCertificate = certificate }
            })
        }, deadline.Token);
        Task<QuicConnection> accept = listener.AcceptConnectionAsync(deadline.Token).AsTask();
        Task<QuicConnection> connect = QuicConnection.ConnectAsync(new()
        {
            RemoteEndPoint = listener.LocalEndPoint, DefaultCloseErrorCode = 0, DefaultStreamErrorCode = 0,
            ClientAuthenticationOptions = new()
            {
                ApplicationProtocols = [Alpn], TargetHost = "localhost", CertificateChainPolicy = Policy(root),
                RemoteCertificateValidationCallback = (_, _, _, _) =>
                {
                    entered.TrySetResult();
                    try { Check(release.Wait(TimeSpan.FromSeconds(10)), "validation cancellation timed out"); return true; }
                    finally { finished.TrySetResult(); }
                }
            }
        }, cancelConnect.Token).AsTask();
        try
        {
            await entered.Task.WaitAsync(deadline.Token);
            cancelConnect.Cancel();
            try { await using var unexpected = await connect.WaitAsync(TimeSpan.FromSeconds(3)); throw new Exception("canceled handshake succeeded"); }
            catch (OperationCanceledException) { }
        }
        finally
        {
            release.Set();
            await finished.Task.WaitAsync(deadline.Token);
            deadline.Cancel();
            try { await using var peer = await accept; }
            catch (Exception error) when (error is QuicException or OperationCanceledException or AuthenticationException) { }
        }
        Console.WriteLine("PASS cancel-during-validation");
    }

    public static async Task Main(string[] args)
    {
        if (args.Contains("--ipv6")) Loopback = IPAddress.IPv6Loopback;
        if (args.Contains("--no-cache")) AppContext.SetSwitch("System.Net.Quic.DisableConfigurationCache", true);
        if (Environment.GetEnvironmentVariable("SSLKEYLOGFILE") != null)
            AppContext.SetSwitch("System.Net.EnableSslKeyLogging", true);
        Check(QuicConnection.IsSupported && QuicListener.IsSupported, "managed QUIC unavailable");
        using var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var request = new CertificateRequest("CN=dotcc test root", key, HashAlgorithmName.SHA256);
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.CrlSign, true));
        using var root = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddDays(2));
        using var server = Issue(root, "localhost", "1.3.6.1.5.5.7.3.1");
        using var client = Issue(root, "client", "1.3.6.1.5.5.7.3.2");
        foreach (string scenario in new[] { "custom-trust", "callback", "name-reject", "name-override", "callback-reject",
            "mutual", "mutual-reject", "mutual-missing", "mutual-missing-override", "untrusted-reject", "untrusted-override",
            "callback-throw", "server-context", "server-selection", "mutual-context", "mutual-selection", "ip-name", "aes256" })
            await RoundTrip(root, server, client, scenario);
        using var wrongPurpose = Issue(root, "localhost", "1.3.6.1.5.5.7.3.2");
        await RoundTrip(root, wrongPurpose, client, "eku-reject");
        await RoundTrip(root, wrongPurpose, client, "eku-override");
        using var idn = Issue(root, "xn--bcher-kva.example", "1.3.6.1.5.5.7.3.1");
        await RoundTrip(root, idn, client, "idn-name");
        using var intermediateKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var intermediateRequest = new CertificateRequest("CN=dotcc intermediate", intermediateKey, HashAlgorithmName.SHA256);
        intermediateRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, true, 0, true));
        intermediateRequest.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign, true));
        using var publicIntermediate = intermediateRequest.Create(root, DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1), RandomNumberGenerator.GetBytes(16));
        using var intermediate = publicIntermediate.CopyWithPrivateKey(intermediateKey);
        using var chained = Issue(intermediate, "localhost", "1.3.6.1.5.5.7.3.1");
        await RoundTrip(root, chained, client, "intermediate-context", intermediate);
        await CancelValidation(root, server);
    }
}
