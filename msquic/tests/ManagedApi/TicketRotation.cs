using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Managed.Transport.Api;

// Held outside the active test project until the coordinator schedules the
// complete source snapshot. All operations below use the public owning API.
internal static class TicketRotation
{
    private static readonly TimeSpan Limit = TimeSpan.FromSeconds(20);

    internal static async Task RunAsync()
    {
        using var certificates = new Credentials();
        foreach (ushort cipher in new ushort[] { 0x1301, 0x1302 })
        foreach (IPAddress address in new[] { IPAddress.Loopback, IPAddress.IPv6Loopback })
        {
            await using var runtime = await QuicRuntime.CreateAsync(new() { ProcessorCount = 2 });
            await using var serverRegistration = await runtime.OpenRegistrationAsync("ticket-rotation-server");
            await using var clientRegistration = await runtime.OpenRegistrationAsync("ticket-rotation-client");
            using var serverCredentials = QuicCredentials.Server(certificates.EcLeaf, cipherSuite: cipher);
            using var clientCredentials = QuicCredentials.Client([certificates.Root], X509RevocationMode.NoCheck, cipher);
            int policyCalls = 0;
            byte[] observedState = [];
            var options = new QuicConfigurationOptions
            {
                ServerResumptionValidation = (state, _) =>
                {
                    Volatile.Write(ref observedState, state.ToArray());
                    Interlocked.Increment(ref policyCalls);
                    return ValueTask.FromResult(true);
                }
            };
            var settings = new QuicSettings
            {
                PeerBidiStreamCount = 4, IdleTimeoutMs = 15000,
                ServerResumptionLevel = QuicServerResumption.Tickets
            };
            await using var configurationA = await serverRegistration.CreateConfigurationAsync(
                ["ticket-rotation"u8.ToArray()], serverCredentials, settings, options: options);
            await using var configurationB = await serverRegistration.CreateConfigurationAsync(
                ["ticket-rotation"u8.ToArray()], serverCredentials, settings, options: options);
            await using var clientConfiguration = await clientRegistration.CreateConfigurationAsync(
                ["ticket-rotation"u8.ToArray()], clientCredentials, new() { PeerBidiStreamCount = 4, IdleTimeoutMs = 15000 });
            await using var listenerA = await serverRegistration.ListenAsync(configurationA, new(address, 0));
            await using var listenerB = await serverRegistration.ListenAsync(configurationB, new(address, 0));

            byte[] idA = RandomNumberGenerator.GetBytes(16), materialA = RandomNumberGenerator.GetBytes(64);
            byte[] idB = RandomNumberGenerator.GetBytes(16), materialB = RandomNumberGenerator.GetBytes(64);
            byte[] preservedIdB = idB.ToArray(), preservedMaterialB = materialB.ToArray();
            using var keyA = new QuicTicketKey(idA, materialA);
            using var keyB = new QuicTicketKey(idB, materialB);
            try
            {
                // Public key construction owns copies, followed by another
                // configuration import copy. Mutating caller input cannot alter
                // either credential/configuration or later key reuse.
                CryptographicOperations.ZeroMemory(idA); CryptographicOperations.ZeroMemory(materialA);
                CryptographicOperations.ZeroMemory(idB); CryptographicOperations.ZeroMemory(materialB);
                configurationA.SetTicketKeys([keyA]);
                configurationB.SetTicketKeys([keyA]);

                async Task<QuicResumptionTicket?> Exchange(QuicListener listener, ReadOnlyMemory<byte> ticket,
                    bool resumed, ReadOnlyMemory<byte> expectedState, byte[]? issueState = null)
                {
                    int before = Volatile.Read(ref policyCalls);
                    using var timeout = new CancellationTokenSource(Limit);
                    await using var client = await clientRegistration.ConnectAsync(clientConfiguration, "localhost", listener.LocalEndPoint,
                        new QuicConnectOptions { ResumptionTicket = ticket }, timeout.Token);
                    await using var server = await listener.AcceptConnectionAsync(timeout.Token);
                    Check(client.GetStatistics().ResumptionSucceeded == resumed && server.GetStatistics().ResumptionSucceeded == resumed,
                        "Unexpected ticket resumption result");
                    Check(Volatile.Read(ref policyCalls) == before + (resumed ? 1 : 0), "Ticket policy callback count differs");
                    if (resumed) Check(Volatile.Read(ref observedState).AsSpan().SequenceEqual(expectedState.Span), "Ticket app state changed");
                    Check(client.GetHandshakeInformation().CipherSuite == (QuicTlsCipherSuite)cipher && client.GetProtocolVersion() == 1,
                        "Resumed/fallback profile changed");
                    await Transfer(client, server, 37, timeout.Token);
                    await Transfer(server, client, 91, timeout.Token);
                    Check(client.CallbackFailure == null && server.CallbackFailure == null, "Callback failed during ticket exchange");
                    if (issueState == null) return null;
                    server.SendResumptionTicket(issueState);
                    return await client.WaitForResumptionTicketAsync(cancellationToken: timeout.Token);
                }

                var ticketA = await Exchange(listenerA, default, false, default, "key-A-state"u8.ToArray())
                    ?? throw new InvalidOperationException("No key A ticket received");
                await Exchange(listenerB, ticketA.Bytes, true, "key-A-state"u8.ToArray());

                // The first imported key encrypts; remaining keys only decrypt.
                configurationB.SetTicketKeys([keyB, keyA]);
                keyA.Dispose(); keyB.Dispose();
                var ticketB = await Exchange(listenerB, ticketA.Bytes, true, "key-A-state"u8.ToArray(), "key-B-state"u8.ToArray())
                    ?? throw new InvalidOperationException("No key B ticket received");
                Check(!ticketA.Bytes.Span.SequenceEqual(ticketB.Bytes.Span), "Distinct tickets unexpectedly identical");
                // A only knows A: a B-issued ticket must fall back to a complete
                // authenticated handshake, even though A and B share a cert.
                await Exchange(listenerA, ticketB.Bytes, false, default);
                await Exchange(listenerA, ticketA.Bytes, true, "key-A-state"u8.ToArray());

                using var onlyB = new QuicTicketKey(preservedIdB, preservedMaterialB);
                configurationB.SetTicketKeys([onlyB]);
                Reject<ArgumentException>(() => configurationB.SetTicketKeys([onlyB, onlyB]));
                Reject<ArgumentException>(() => configurationB.SetTicketKeys(Array.Empty<QuicTicketKey>()));
                Reject<ObjectDisposedException>(() => configurationB.SetTicketKeys([keyA]));
                onlyB.Dispose();
                await Exchange(listenerB, ticketA.Bytes, false, default);
                await Exchange(listenerB, ticketB.Bytes, true, "key-B-state"u8.ToArray());

                await serverRegistration.DisposeAsync().AsTask().WaitAsync(Limit);
                await clientRegistration.DisposeAsync().AsTask().WaitAsync(Limit);
                await runtime.DisposeAsync().AsTask().WaitAsync(Limit);
                Console.WriteLine($"PASS facade ticket-key rotation cipher=0x{cipher:x} family={address.AddressFamily}: copied inputs, cross-config resume, new-first issuance, removal fallback, rejected-update atomicity, fresh data and close");
            }
            finally
            {
                CryptographicOperations.ZeroMemory(idA); CryptographicOperations.ZeroMemory(materialA);
                CryptographicOperations.ZeroMemory(idB); CryptographicOperations.ZeroMemory(materialB);
                CryptographicOperations.ZeroMemory(preservedIdB); CryptographicOperations.ZeroMemory(preservedMaterialB);
            }
        }
    }

    private static async Task Transfer(QuicConnection sender, QuicConnection receiver, int seed, CancellationToken token)
    {
        byte[] payload = Enumerable.Range(0, 4097).Select(i => (byte)((i * 31 + seed) & 255)).ToArray();
        await using var outgoing = await sender.OpenStreamAsync(cancellationToken: token);
        await outgoing.SendAsync(payload, QuicSendOptions.Fin, token);
        await using var incoming = await receiver.AcceptStreamAsync(token);
        byte[] received = new byte[payload.Length]; int offset = 0;
        while (offset < received.Length)
        {
            int count = await incoming.ReadAsync(received.AsMemory(offset), token);
            Check(count != 0, "Premature ticket-exchange FIN");
            offset += count;
        }
        Check(received.AsSpan().SequenceEqual(payload), "Ticket-exchange payload corruption");
        Check(await incoming.ReadAsync(new byte[1], token) == 0, "Ticket-exchange FIN missing");
    }
    private static void Reject<T>(Action action) where T : Exception
    {
        try { action(); }
        catch (T) { return; }
        throw new InvalidOperationException("Expected " + typeof(T).Name);
    }
    private static void Check(bool value, string message)
    { if (!value) throw new InvalidOperationException(message); }
}
