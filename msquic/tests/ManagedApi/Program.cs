using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Managed.Transport.Api;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        string? requiredRevision = Environment.GetEnvironmentVariable("DOTCC_REQUIRED_SOURCE_REVISION");
        if (requiredRevision != null && Managed.Transport.MsQuic.VER_GIT_HASH_STR != requiredRevision)
            throw new InvalidOperationException("Full facade qualification requires exact pinned source revision metadata.");
        if (args is ["--datagram-late-ack"]) { await DatagramLateAck.RunAsync(); return; }
        if (args is ["--versions"]) { await VersionPolicyControls.RunAsync(); return; }
        if (args is ["--handshake-snapshots"]) { await HandshakeSnapshots.RunAsync(); return; }
        if (args is ["--network"]) { await NetworkParameters.RunAsync(); return; }
        if (args is ["--flags"]) { await FlagControls.RunAsync(); return; }
        if (args is ["--ticket-rotation"]) { await TicketRotation.RunAsync(); return; }
        if (args is ["--certificates"]) { await CertificatePolicies.RunAsync(); return; }
        if (args is ["--callbacks"]) { await CallbackControls.RunAsync(); return; }
        if (args is ["--handshake-faults"]) { await HandshakeFaults.RunAsync(); return; }
        if (args is ["--packets"]) { await PacketScenarios.RunAsync(); return; }
        if (args is ["--transport"]) { await Transport.RunAsync(); return; }
        if (args is ["--resumption"]) { await ResumptionScenarios.RunAsync(); return; }
        ConnectionParameterInputs.Run();
        await RuntimeParameterControls.InvalidCreationAsync();
        using var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var request = new CertificateRequest("CN=facade ownership control", key, HashAlgorithmName.SHA256);
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
        using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddHours(-1), DateTimeOffset.UtcNow.AddHours(1));
        await using var runtime = await QuicRuntime.CreateAsync(new() { ProcessorCount = 1 });
        VersionPolicy.CheckGlobal(runtime);
        RuntimeParameterControls.BeforeRegistration(runtime);
        await using var registration = await runtime.OpenRegistrationAsync("facade-control");
        RuntimeParameterControls.AfterRegistration(runtime);
        await using var foreign = await runtime.OpenRegistrationAsync("facade-foreign");
        foreach (bool asynchronous in new[] { false, true })
        {
            byte[] alpn = "facade-control"u8.ToArray();
            var credentials = QuicCredentials.Client([certificate], X509RevocationMode.NoCheck,
                loadAsynchronously: asynchronous);
            var creation = registration.CreateConfigurationAsync([alpn], credentials,
                new() { IdleTimeoutMs = 12345, PeerBidiStreamCount = 3 });
            credentials.Dispose();
            alpn.AsSpan().Fill(0);
            var configuration = await creation;
            try
            {
                if (!configuration.ProtocolBytes[0].AsSpan().SequenceEqual("facade-control"u8) ||
                    !configuration.BelongsTo(registration) || configuration.BelongsTo(foreign))
                    throw new InvalidOperationException("Configuration snapshots or registration identity changed.");
                VersionPolicy.CheckConfiguration(runtime, configuration);
                var settings = configuration.GetSettings();
                if (settings.IdleTimeoutMs != 12345 || settings.PeerBidiStreamCount != 3)
                    throw new InvalidOperationException("Initial configuration settings did not round trip.");
                configuration.SetSettings(new() { IdleTimeoutMs = 23456 });
                settings = configuration.GetSettings();
                if (settings.IdleTimeoutMs != 23456 || settings.PeerBidiStreamCount != 3)
                    throw new InvalidOperationException("Partial settings update lost previous values.");
                // Upstream applies IdleTimeoutMs before validating this later
                // receive-window field. A raw rejected update mutates the timeout.
                try
                {
                    configuration.SetSettings(new() { IdleTimeoutMs = 34567, StreamRecvWindowDefault = 3 });
                    throw new InvalidOperationException("Invalid later setting was accepted.");
                }
                catch (ArgumentException) { }
                if (configuration.GetSettings() != settings)
                    throw new InvalidOperationException("Rejected settings update partially mutated the configuration.");
                configuration.SetSettings(new() { MinimumMtu = 1300, MaximumMtu = 1400 });
                settings = configuration.GetSettings();
                try
                {
                    configuration.SetSettings(new() { IdleTimeoutMs = 45678, MinimumMtu = 1450 });
                    throw new InvalidOperationException("MTU update ignored the effective maximum.");
                }
                catch (ArgumentException) { }
                if (configuration.GetSettings() != settings)
                    throw new InvalidOperationException("Rejected dependent MTU update partially mutated settings.");
                var lease = configuration.AcquireLease();
                Task disposal;
                try
                {
                    disposal = configuration.DisposeAsync().AsTask();
                    if (disposal.IsCompleted) throw new InvalidOperationException("Configuration closed with an outstanding lease.");
                }
                finally { lease.Dispose(); }
                await disposal.WaitAsync(TimeSpan.FromSeconds(10));
            }
            finally { await configuration.DisposeAsync(); }
        }
        using var validationCredentials = QuicCredentials.Client([certificate], X509RevocationMode.NoCheck);
        await Reject<ArgumentException>(registration.CreateConfigurationAsync([ReadOnlyMemory<byte>.Empty], validationCredentials));
        await Reject<ArgumentException>(registration.CreateConfigurationAsync([new byte[256]], validationCredentials));
        await Reject<NotSupportedException>(registration.CreateConfigurationAsync([new byte[] { 1, 0, 2 }], validationCredentials));
        using var canceled = new CancellationTokenSource(); canceled.Cancel();
        await Reject<OperationCanceledException>(registration.CreateConfigurationAsync(["test"u8.ToArray()],
            validationCredentials, cancellationToken: canceled.Token));
        await VersionPolicy.CheckConnectionAsync(runtime, registration);
        Console.WriteLine("PASS managed API configuration control: global/configuration/connection QUIC v1; sync/async credentials; copied ALPN; settings; lease drain; rejection before dispatch");
    }

    private static async Task Reject<T>(ValueTask<QuicConfiguration> operation) where T : Exception
    {
        try { await using var unexpected = await operation; }
        catch (T) { return; }
        throw new InvalidOperationException("Expected " + typeof(T).Name);
    }
}
