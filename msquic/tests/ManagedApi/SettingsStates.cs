using System.Net;
using System.Security.Cryptography.X509Certificates;
using Managed.Transport;
using Managed.Transport.Api;

internal static class SettingsStates
{
    private static void Check(bool condition, string message)
    { if (!condition) throw new InvalidOperationException("Settings state: " + message); }

    private static void Rejected(Action action)
    {
        try { action(); }
        catch (ArgumentException) { return; }
        throw new InvalidOperationException("Invalid compound settings update was accepted");
    }

    internal static async Task RunAsync()
    {
        using var certificates = new Credentials();
        foreach (var address in new[] { IPAddress.Loopback, IPAddress.IPv6Loopback })
        {
            await using var runtime = await QuicRuntime.CreateAsync(new() { ProcessorCount = 2 });
            await using var registration = await runtime.OpenRegistrationAsync("settings-states");
            using var serverCredential = QuicCredentials.Server(certificates.EcLeaf, cipherSuite: 0x1301);
            using var clientCredential = QuicCredentials.Client([certificates.Root], X509RevocationMode.NoCheck, 0x1301);
            var settings = new QuicSettings
            {
                IdleTimeoutMs = 15000, DatagramReceiveEnabled = true,
                MinimumMtu = 1280, MaximumMtu = 1400
            };
            await using var serverConfiguration = await registration.CreateConfigurationAsync(["settings-states"u8.ToArray()], serverCredential, settings);
            await using var clientConfiguration = await registration.CreateConfigurationAsync(["settings-states"u8.ToArray()], clientCredential, settings);

            // Two independent fields must survive concurrent sparse updates.
            // Each writer has a deterministic last value; ordering between
            // writers is deliberately unspecified.
            await Task.WhenAll(
                Task.Run(() => { for (uint i = 0; i < 16; i++) clientConfiguration.SetSettings(new() { IdleTimeoutMs = 15000 + i }); }),
                Task.Run(() => { for (uint i = 0; i < 16; i++) clientConfiguration.SetSettings(new() { KeepAliveIntervalMs = 1000 + i }); }));
            var configured = clientConfiguration.GetSettings();
            Check(configured.IdleTimeoutMs == 15015 && configured.KeepAliveIntervalMs == 1015,
                "concurrent configuration setters lost an independent field");
            Rejected(() => clientConfiguration.SetSettings(new() { IdleTimeoutMs = 16000, MinimumMtu = 1450 }));
            Check(clientConfiguration.GetSettings() == configured, "invalid later MTU field partially mutated configuration");

            await using var listener = await registration.ListenAsync(serverConfiguration, new(address, 0));
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            var accepting = listener.AcceptConnectionAsync(timeout.Token).AsTask();
            await using var client = await registration.ConnectAsync(clientConfiguration, "localhost", listener.LocalEndPoint, timeout.Token);
            await using var server = await accepting;
            await Task.WhenAll(
                Task.Run(() => { for (uint i = 0; i < 16; i++) client.SetSettings(new() { IdleTimeoutMs = 17000 + i }); }),
                Task.Run(() => { for (uint i = 0; i < 16; i++) client.SetSettings(new() { KeepAliveIntervalMs = 2000 + i }, QuicParameterPriority.High); }));
            var connected = client.GetSettings();
            Check(connected.IdleTimeoutMs == 17015 && connected.KeepAliveIntervalMs == 2015,
                "concurrent live setters lost an independent field");

            // Pinned QuicSettingApply(... AllowMtuAndEcnChanges=false ...)
            // rejects even otherwise valid MTU updates on started connections.
            // Its earlier IdleTimeout assignment must remain only in the
            // facade's temporary preflight value when that rejection occurs.
            Rejected(() => client.SetSettings(new() { IdleTimeoutMs = 18000, MaximumMtu = 1450 }));
            Check(client.GetSettings() == connected, "live MTU rejection partially mutated settings");
            Rejected(() => client.SetSettings(new() { IdleTimeoutMs = 18000, MaxAckDelayMs = uint.MaxValue }));
            Check(client.GetSettings() == connected, "later scalar rejection partially mutated settings");

            // The direct native parameter and aggregate native settings have
            // distinct state rules in this pin. The facade uses aggregate
            // settings and must be assessed against that actual operation.
            Check(DirectDatagramSetting(runtime, server, false) == 1,
                "direct datagram receive parameter did not report InvalidState after start");
            Check(server.GetCapabilities().DatagramReceiveEnabled, "rejected direct setter changed receive state");
            server.SetSettings(new() { DatagramReceiveEnabled = false });
            Check(server.GetSettings().DatagramReceiveEnabled == false && !server.GetCapabilities().DatagramReceiveEnabled,
                "aggregate settings did not update the actual receive state");
            server.SetSettings(new() { DatagramReceiveEnabled = true });
            Check(server.GetSettings().DatagramReceiveEnabled == true && server.GetCapabilities().DatagramReceiveEnabled,
                "aggregate settings did not restore receive state");
            byte[] payload = "live datagram after aggregate setting restore"u8.ToArray();
            var receiving = server.ReceiveDatagramAsync(timeout.Token).AsTask();
            var sending = client.SendDatagramAsync(payload, timeout.Token).AsTask();
            Check((await receiving).Span.SequenceEqual(payload), "datagram payload after setting restore");
            Check(await sending is QuicDatagramSendResult.Acknowledged or QuicDatagramSendResult.AcknowledgedAfterLoss,
                "datagram did not receive an actual acknowledgment");

            await registration.DisposeAsync();
            await runtime.DisposeAsync();
            Check(runtime.Host.OutstandingResources == 0 && runtime.Host.OutstandingPlatformAllocations == 0 &&
                runtime.Host.OutstandingDatagramReceives == 0 && QuicObject.LiveContextCount == 0,
                "settings controls left ownership outstanding");
            Console.WriteLine($"PASS facade settings state {address.AddressFamily}: concurrent sparse updates, compound rejection atomicity, live MTU boundary and direct/aggregate DATAGRAM distinction");
        }
    }

    private static unsafe uint DirectDatagramSetting(QuicRuntime runtime, QuicConnection connection, bool enabled)
    {
        byte value = enabled ? (byte)1 : (byte)0;
        return runtime.Api->SetParam(connection.Handle, MsQuic.QUIC_PARAM_CONN_DATAGRAM_RECEIVE_ENABLED, 1, &value);
    }
}
