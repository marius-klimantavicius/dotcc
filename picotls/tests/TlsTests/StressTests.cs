using System.Runtime.CompilerServices;
using System.Security.Authentication;
using Managed.Security;

internal static partial class Program
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference AbandonConnection(PicotlsContext context)
    {
        var connection = context.CreateConnection("localhost");
        connection.Process([]); // Allocate a real transcript, ECDH context and ClientHello.
        return new WeakReference(connection);
    }
    private static void MalformedAndLifetime()
    {
        using var verifier = Verifier();
        using var signer = Identity("server-ecdsa");
        using var serverContext = new PicotlsContext(true, verifier, signer, maximumHandshakeBuffer: 16384);
        using var clientContext = new PicotlsContext(false, verifier, saveSessionTickets: true);
        int baseline = BclCryptoProvider.LiveManagedContexts;
        byte[][] explicitCases =
        [
            [22, 3, 3, 0, 4, 1, 0, 0, 0], // Empty ClientHello.
            [22, 3, 3, 0, 4, 20, 0, 0, 0], // Unexpected Finished.
            [23, 3, 3, 0, 1, 0], // Application data before authentication.
            [22, 3, 3, 255, 255], // Oversized record.
            [22, 3, 3, 0, 4, 1, 255, 255, 255], // Oversized, truncated handshake.
            [22, 3, 0, 0, 1, 0], // Invalid record version.
            [255, 3, 3, 0, 1, 0], // Invalid content type.
            [22, 3, 3, 0], // Partial record header.
        ];
        var random = new Random(0x5049434f); // Reproducible malformed input only.
        var corpus = new List<byte[]>(explicitCases);
        for (int i = 0; i < 256; i++)
        {
            byte[] bytes = new byte[random.Next(1, 257)]; random.NextBytes(bytes);
            if (i % 2 == 0 && bytes.Length >= 5)
            {
                bytes[0] = 22; bytes[1] = 3; bytes[2] = 3;
                bytes[3] = (byte)((bytes.Length - 5) >> 8); bytes[4] = (byte)(bytes.Length - 5);
            }
            corpus.Add(bytes);
        }
        foreach (byte[] bytes in corpus)
        {
            using var server = serverContext.CreateConnection();
            bool rejected = false;
            try
            {
                for (int offset = 0; offset < bytes.Length; offset += 7)
                {
                    var step = server.Process(bytes.AsSpan(offset, Math.Min(7, bytes.Length - offset)));
                    Check(!step.HandshakeComplete && step.Plaintext.Length == 0, "malformed peer never authenticates or releases plaintext");
                }
                server.CompleteInput();
            }
            catch (Exception error) when (error is AuthenticationException or EndOfStreamException) { rejected = true; }
            Check(rejected, "malformed input or truncation fails closed");
        }
        for (int i = 0; i < 64; i++)
        {
            using var client = clientContext.CreateConnection("localhost");
            client.Process([]);
            if (i % 4 == 0) { GC.Collect(); GC.WaitForPendingFinalizers(); }
            // Cancellation/abort at a partially initialized handshake is Dispose.
        }
        Check(BclCryptoProvider.LiveManagedContexts == baseline, "explicit aborts release provider state");
        WeakReference abandoned = AbandonConnection(clientContext);
        for (int i = 0; i < 5 && abandoned.IsAlive; i++) { GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect(); }
        Check(!abandoned.IsAlive, "connection-local state does not root its owner");
        Check(BclCryptoProvider.LiveManagedContexts == baseline, "finalizer releases abandoned handshake state");
    }
}
