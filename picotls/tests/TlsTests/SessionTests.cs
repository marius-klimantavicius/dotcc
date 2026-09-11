using System.Security.Cryptography;
using Managed.Security;

internal static partial class Program
{
    private static void PumpHandshake(PicotlsConnection client, PicotlsConnection server)
    {
        var pending = new Queue<(bool ToServer, byte[] Data)>();
        pending.Enqueue((true, client.Process([]).Outbound));
        int budget = 100;
        while (pending.TryDequeue(out var packet))
        {
            Check(--budget > 0, "bounded session handshake");
            var step = (packet.ToServer ? server : client).Process(packet.Data);
            Check(step.Consumed == packet.Data.Length && step.Plaintext.Length == 0, "session handshake bytes consumed without early plaintext");
            if (step.Outbound.Length != 0) pending.Enqueue((!packet.ToServer, step.Outbound));
        }
        Check(client.HandshakeComplete && server.HandshakeComplete, "session handshake complete");
    }
    private static byte[] CaptureTicket(PicotlsContext clientContext, PicotlsContext serverContext)
    {
        using var client = clientContext.CreateConnection("localhost");
        using var server = serverContext.CreateConnection();
        PumpHandshake(client, server);
        Check(!client.IsResumed && !server.IsResumed, "ticket issuance follows a full handshake");
        var tickets = client.TakeSessionTickets();
        try
        {
            Check(tickets.Count > 0, "server issued a saved resumption ticket");
            Check(tickets.All(ticket => !ticket.EarlyDataEnabled && !ticket.IsExpired), "fresh tickets prohibit early data");
            return tickets[0].Export();
        }
        finally { foreach (var ticket in tickets) ticket.Dispose(); }
    }
    private static void Resume(PicotlsContext clientContext, PicotlsContext serverContext, byte[] ticket, bool expected)
    {
        using var client = clientContext.CreateConnection("localhost", ticket);
        using var server = serverContext.CreateConnection();
        Reject("no early writes even with a ticket", () => client.Send("early"u8), e => e is InvalidOperationException);
        PumpHandshake(client, server);
        Check(client.IsResumed == expected && server.IsResumed == expected, "PSK-DHE resumption or authenticated fallback as expected");
        byte[] encoded = client.Send("resumed application data"u8);
        Check(server.Process(encoded).Plaintext.AsSpan().SequenceEqual("resumed application data"u8), "authenticated session application bytes");
        Check(client.ExportSecret("resumption exporter", [], 32).AsSpan().SequenceEqual(server.ExportSecret("resumption exporter", [], 32)), "resumed exporter agreement");
    }
    private static void SessionTickets()
    {
        using var verifier = Verifier();
        using var signer = Identity("server-ecdsa");
        using var protector = new BclCryptoProvider.TicketProtector(TimeSpan.FromSeconds(60), retainedPreviousKeys: 1);
        using var serverContext = new PicotlsContext(true, verifier, signer, ticketProtector: protector);
        using var clientContext = new PicotlsContext(false, verifier, saveSessionTickets: true);
        byte[] ticket = CaptureTicket(clientContext, serverContext);
        try
        {
            Resume(clientContext, serverContext, ticket, true);
            byte[] corrupt = (byte[])ticket.Clone();
            try
            {
                // Pinned core saved-ticket format: time(8), group(2), suite(2),
                // NST length(3), lifetime(4), age-add(4), nonce length(1), nonce,
                // ticket length(2), protected ticket. Corrupt only the envelope.
                int envelope = 26 + corrupt[23];
                Check(envelope + 32 < corrupt.Length, "saved ticket contains an encrypted envelope");
                corrupt[envelope + 30] ^= 1;
                Resume(clientContext, serverContext, corrupt, false);
            }
            finally { CryptographicOperations.ZeroMemory(corrupt); }
            protector.Rotate(); Resume(clientContext, serverContext, ticket, true);
            protector.Rotate(); Resume(clientContext, serverContext, ticket, false);
        }
        finally { CryptographicOperations.ZeroMemory(ticket); }

        using var shortProtector = new BclCryptoProvider.TicketProtector(TimeSpan.FromSeconds(1));
        using var shortServer = new PicotlsContext(true, verifier, signer, ticketProtector: shortProtector);
        byte[] expired = CaptureTicket(clientContext, shortServer);
        try
        {
            Thread.Sleep(1200); // Exercise both real core and protector wall-clock expiry.
            Resume(clientContext, shortServer, expired, false);
        }
        finally { CryptographicOperations.ZeroMemory(expired); }

        foreach (string clientIdentity in new[] { "client-rsa", "client-ecdsa" })
        {
            using var clientSigner = Identity(clientIdentity);
            using var mutualProtector = new BclCryptoProvider.TicketProtector(TimeSpan.FromMinutes(5));
            using var mutualServer = new PicotlsContext(true, verifier, signer,
                requireClientAuthentication: true, ticketProtector: mutualProtector);
            using var mutualClient = new PicotlsContext(false, verifier, clientSigner, saveSessionTickets: true);
            byte[] mutualTicket = CaptureTicket(mutualClient, mutualServer);
            try
            {
                // Pinned upstream deliberately turns off PSK resumption when
                // client authentication is required: every retry reauthenticates.
                Resume(mutualClient, mutualServer, mutualTicket, false);
                using var anonymous = clientContext.CreateConnection("localhost", mutualTicket);
                using var requiringClient = mutualServer.CreateConnection();
                Reject("a saved ticket cannot bypass required client authentication",
                    () => PumpHandshake(anonymous, requiringClient));
            }
            finally { CryptographicOperations.ZeroMemory(mutualTicket); }
        }
    }
}
