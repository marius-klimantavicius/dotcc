using static Managed.Transport.MsQuic;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Managed.Transport;
using Managed.Transport.Api;

internal static class HandshakeSnapshots
{
    internal static async Task RunAsync()
    {
        using var certificates = new Credentials();
        foreach (ushort cipher in new ushort[] { 0x1301, 0x1302 })
        foreach (var address in new[] { IPAddress.Loopback, IPAddress.IPv6Loopback })
        {
            await using var runtime = await QuicRuntime.CreateAsync(new() { ProcessorCount = 2 });
            await using var registration = await runtime.OpenRegistrationAsync("handshake-snapshot");
            using var serverCredentials = QuicCredentials.Server(certificates.EcLeaf, cipherSuite: cipher);
            using var clientCredentials = QuicCredentials.Client([certificates.Root], X509RevocationMode.NoCheck, cipher);
            var settings = new QuicSettings { ServerResumptionLevel = QuicServerResumption.Disabled, IdleTimeoutMs = 10000 };
            await using var serverConfiguration = await registration.CreateConfigurationAsync(["handshake-snapshot"u8.ToArray()], serverCredentials, settings);
            await using var clientConfiguration = await registration.CreateConfigurationAsync(["handshake-snapshot"u8.ToArray()], clientCredentials, settings);
            await using var listener = await registration.ListenAsync(serverConfiguration, new(address, 0));
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var accepting = listener.AcceptConnectionAsync(timeout.Token).AsTask();
            await using var client = await registration.ConnectAsync(clientConfiguration, "localhost", listener.LocalEndPoint, timeout.Token);
            await using var server = await accepting;
            // Establish the actual native getter's terminal TLS lifetime, rather
            // than crediting a snapshot while the underlying TLS still exists.
            while (RawHandshakeStatus(runtime, server) != 1) await Task.Delay(5, timeout.Token);
            var snapshot = server.GetHandshakeInformation();
            if ((ushort)snapshot.CipherSuite != cipher || snapshot.NamedGroup != QuicTlsNamedGroup.Secp256R1 ||
                snapshot != client.GetHandshakeInformation() || snapshot != server.GetHandshakeInformation(QuicParameterPriority.High))
                throw new InvalidOperationException("Negotiated handshake snapshot changed after TLS retirement");
            try { server.GetHandshakeInformation((QuicParameterPriority)99); throw new InvalidOperationException("Invalid priority accepted"); }
            catch (ArgumentException) { }
            await server.DisposeAsync();
            try { server.GetHandshakeInformation(); throw new InvalidOperationException("Disposed handshake query accepted"); }
            catch (ObjectDisposedException) { }
            await registration.DisposeAsync();
            await runtime.DisposeAsync();
            if (runtime.Host.OutstandingResources != 0 || runtime.Host.OutstandingPlatformAllocations != 0)
                throw new InvalidOperationException("Handshake snapshot control left host resources");
        }
        Console.WriteLine("PASS facade handshake snapshots: no-resumption server TLS retired; copied negotiated values; priority and closed lifetime; both AES and IP families");
    }

    private static unsafe uint RawHandshakeStatus(QuicRuntime runtime, QuicConnection connection)
    {
        QUIC_HANDSHAKE_INFO info = default;
        uint length = (uint)sizeof(QUIC_HANDSHAKE_INFO);
        return runtime.Api->GetParam(connection.Handle, MsQuic.QUIC_PARAM_TLS_HANDSHAKE_INFO, &length, &info);
    }
}
