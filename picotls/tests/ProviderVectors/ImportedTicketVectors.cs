using System.Buffers.Binary;
using System.Security.Cryptography;
using Managed.Security;
using Protector = Managed.Security.BclCryptoProvider.TicketProtector;
using Import = Managed.Security.BclCryptoProvider.TicketKeyImport;

static unsafe class ImportedTicketVectors
{
    private static readonly byte[] Plaintext = "shared QUIC server ticket"u8.ToArray();
    private static readonly DateTimeOffset Epoch = DateTimeOffset.FromUnixTimeSeconds(1800000000);
    private static Import Key(byte marker) => new(Enumerable.Repeat(marker, 16).ToArray(), Enumerable.Repeat(marker, 64).ToArray());
    private static byte[] Encrypt(Protector owner) => TicketVectors.Invoke(owner, true, Plaintext, 0);
    private static void Decrypt(Protector owner, byte[] ticket) =>
        Program.Equal(TicketVectors.Invoke(owner, false, ticket, 0x209), Plaintext, "imported authenticated ticket rejects early data");
    private static void Reject(Protector owner, byte[] ticket) => TicketVectors.Invoke(owner, false, ticket, 0x205);
    private static void Throws<T>(Action action, string description) where T : Exception
    {
        bool rejected = false;
        try { action(); } catch (T) { rejected = true; }
        Program.Check(rejected, description);
    }
    internal static void Run()
    {
        CrossInstanceRotation(); ValidationAndOwnership(); ImportFailures(); LimitsAndLifetime(); ConcurrentImportAndEncryption();
    }
    private static void CrossInstanceRotation()
    {
        using var first = new Protector(TimeSpan.FromSeconds(10), 0);
        using var second = new Protector(TimeSpan.FromSeconds(10), 0);
        DateTimeOffset now = Epoch;
        using var clock1 = first.UseClockForTesting(() => now);
        using var clock2 = second.UseClockForTesting(() => now);
        Import a = Key(1), b = Key(2);
        first.ImportKeys([a]); second.ImportKeys([a]);
        byte[] ticket = Encrypt(first), peerTicket = Encrypt(second), next = Encrypt(first);
        Program.Check(ticket[4] == 2 && ticket.Length == Plaintext.Length + 81 + 16, "imported ticket format v2 has authenticated salt");
        Program.Check(!ticket.AsSpan(49, 32).SequenceEqual(peerTicket.AsSpan(49, 32)), "same master has independent instance derivation domains");
        Program.Check(ticket.AsSpan(49, 32).SequenceEqual(next.AsSpan(49, 32)) &&
            !ticket.AsSpan(37, 12).SequenceEqual(next.AsSpan(37, 12)), "same domain uses distinct counter nonces");
        Decrypt(second, ticket); Decrypt(first, peerTicket);
        // Independently derive the documented envelope key and authenticate using BCL.
        byte[] info = "Managed.Security.TicketProtector/v2/AES-256-GCM"u8.ToArray().Concat(a.Id.ToArray()).ToArray();
        byte[] derived = HKDF.DeriveKey(HashAlgorithmName.SHA256, a.Material.ToArray(), 32, ticket.AsSpan(49, 32).ToArray(), info);
        try
        {
            using var aes = new AesGcm(derived, 16);
            byte[] clear = new byte[Plaintext.Length];
            aes.Decrypt(ticket.AsSpan(37, 12), ticket.AsSpan(81, clear.Length), ticket.AsSpan(ticket.Length - 16), clear, ticket.AsSpan(0, 81));
            Program.Equal(clear, Plaintext, "independent v2 envelope decode");
            CryptographicOperations.ZeroMemory(clear);
        }
        finally { CryptographicOperations.ZeroMemory(derived); }
        first.ImportKeys([a]);
        byte[] reimported = Encrypt(first);
        Program.Check(!ticket.AsSpan(49, 32).SequenceEqual(reimported.AsSpan(49, 32)), "identical reimport refreshes salt before resetting counter");
        Decrypt(first, ticket); Decrypt(second, reimported);
        second.ImportKeys([b, a]); // a has no local encryption history in this replacement.
        Decrypt(second, ticket);
        byte[] rotated = Encrypt(second);
        Program.Equal(rotated.AsSpan(5, 16), b.Id.Span, "first configured key is encryption key");
        first.ImportKeys([a, b]); Decrypt(first, rotated);
        second.ImportKeys([b]); Reject(second, ticket); Decrypt(second, rotated);
        Throws<InvalidOperationException>(() => first.Rotate(), "random rotation is forbidden after explicit import");
        foreach (int offset in new[] { 0, 4, 5, 21, 29, 37, 41, 49, 80, 81, ticket.Length - 1 })
        {
            byte[] tampered = ticket.ToArray(); tampered[offset] ^= 1; Reject(first, tampered);
        }
        foreach (int size in new[] { 0, 1, 49, 80, 81, 96 }) Reject(first, ticket[..size]);
        now = Epoch.AddMilliseconds(-1); Reject(first, ticket);
        now = Epoch.AddSeconds(10); Reject(first, ticket);
        now = Epoch;
        using var differentLifetime = new Protector(TimeSpan.FromSeconds(11));
        using var clock3 = differentLifetime.UseClockForTesting(() => now);
        differentLifetime.ImportKeys([a]); Reject(differentLifetime, ticket);
        using var random = new Protector(TimeSpan.FromSeconds(10));
        Reject(random, ticket); Reject(first, Encrypt(random));
    }
    private static void ValidationAndOwnership()
    {
        using var owner = new Protector(TimeSpan.FromSeconds(10));
        using var clock = owner.UseClockForTesting(() => Epoch);
        byte[] id = Enumerable.Repeat((byte)3, 16).ToArray(), master = Enumerable.Repeat((byte)3, 64).ToArray();
        owner.ImportKeys([new Import(id, master)]);
        byte[] ticket = Encrypt(owner);
        id.AsSpan().Fill(4); master.AsSpan().Fill(4);
        Decrypt(owner, ticket); Program.Equal(Encrypt(owner).AsSpan(5, 16), Key(3).Id.Span, "import owns independent caller buffer copies");
        foreach (Import[] invalid in new Import[][]
        {
            [], Enumerable.Range(0, 17).Select(x => Key((byte)x)).ToArray(),
            [new Import(new byte[15], new byte[64])], [new Import(new byte[16], new byte[63])],
            [Key(4), Key(4)], [Key(4), new Import(Key(3).Id, Key(4).Material)]
        })
        {
            int keys = BclCryptoProvider.LiveTicketKeysForTesting;
            Throws<ArgumentException>(() => owner.ImportKeys(invalid), "invalid imported ring rejects atomically");
            Program.Check(BclCryptoProvider.LiveTicketKeysForTesting == keys, "invalid ring disposes all unpublished keys");
            Decrypt(owner, ticket);
        }
        Import[] maximum = Enumerable.Range(0, 16).Select(x => Key((byte)(x + 10))).ToArray();
        owner.ImportKeys(maximum);
        using var peer = new Protector(TimeSpan.FromSeconds(10));
        using var peerClock = peer.UseClockForTesting(() => Epoch);
        peer.ImportKeys([maximum[^1]]); Decrypt(owner, Encrypt(peer));
        Program.Check(BclCryptoProvider.LiveTicketKeysForTesting == 17, "all sixteen configured keys survive pruning without local history");
    }
    private static void ImportFailures()
    {
        using var owner = new Protector(TimeSpan.FromSeconds(10));
        using var clock = owner.UseClockForTesting(() => Epoch);
        owner.ImportKeys([Key(1)]);
        byte[] ticket = Encrypt(owner);
        // List allocation, then master copy / salt allocation / cipher ownership for each key.
        for (int ordinal = 1; ordinal <= 7; ordinal++)
        {
            int keys = BclCryptoProvider.LiveTicketKeysForTesting;
            using (var failure = ProviderFaultInjection.FailAllocation(ordinal))
            {
                Throws<OutOfMemoryException>(() => owner.ImportKeys([Key(2), Key(1)]), "failed ring preparation leaves active ring intact");
                Program.Check(failure.AllocationsAttempted == ordinal, "import failure reached requested allocation boundary");
            }
            Program.Check(BclCryptoProvider.LiveTicketKeysForTesting == keys, "import allocation failure releases unpublished ownership");
            Decrypt(owner, ticket);
        }
    }
    private static void LimitsAndLifetime()
    {
        int baseline = BclCryptoProvider.LiveTicketKeysForTesting;
        var owner = new Protector(TimeSpan.FromSeconds(10));
        using var clock = owner.UseClockForTesting(() => Epoch);
        owner.ImportKeys([Key(1)]);
        owner.SetCounterForTesting((1UL << 32) - 1);
        byte[] last = Encrypt(owner);
        Program.Check(BinaryPrimitives.ReadUInt64BigEndian(last.AsSpan(41, 8)) == (1UL << 32) - 1, "last permitted imported counter is serialized exactly");
        Throws<CryptographicException>(() => Encrypt(owner), "counter exhaustion fails closed");
        owner.ImportKeys([Key(1)]); Decrypt(owner, last);
        Program.Check(BinaryPrimitives.ReadUInt64BigEndian(Encrypt(owner).AsSpan(41, 8)) == 0, "new derivation domain restarts its counter");
        st_ptls_context_t context = default;
        var lease = owner.RetainAndApply(&context);
        owner.Dispose();
        Throws<ObjectDisposedException>(() => owner.ImportKeys([Key(1)]), "disposed owner rejects key import");
        Program.Check(BclCryptoProvider.LiveTicketKeysForTesting == baseline + 1 && context.max_early_data_size == 0 && context.require_dhe_on_psk == 1,
            "context lease retains imported material with early data disabled");
        lease.Dispose();
        Program.Check(BclCryptoProvider.LiveTicketKeysForTesting == baseline, "last lease clears imported material");
    }
    private static void ConcurrentImportAndEncryption()
    {
        using var owner = new Protector(TimeSpan.FromSeconds(10));
        using var peer = new Protector(TimeSpan.FromSeconds(10));
        Import a = Key(1), b = Key(2);
        owner.ImportKeys([a, b]); peer.ImportKeys([a, b]);
        byte[][] tickets = new byte[128][];
        // Workers do not use Program.Check's single-threaded assertion counter.
        Parallel.For(0, tickets.Length, i =>
        {
            if ((i & 7) == 0) owner.ImportKeys((i & 8) == 0 ? [a, b] : [b, a]);
            using var scope = CallbackScope.Enter();
            using var clock = owner.UseClockForTesting(() => Epoch);
            st_ptls_buffer_t destination = PicotlsBuffer.Create();
            try
            {
                fixed (byte* input = Plaintext)
                {
                    var callback = owner.Callback;
                    int status = callback->cb(callback, null, 1, &destination, Program.Vector(input, Plaintext.Length));
                    scope.ThrowIfFailed();
                    if (status != 0) throw new InvalidOperationException("Concurrent ticket encryption failed.");
                }
                tickets[i] = new ReadOnlySpan<byte>(destination.@base, checked((int)destination.off)).ToArray();
            }
            finally { Picotls.dotcc_ptls_buffer_dispose(&destination); }
        });
        using var peerClock = peer.UseClockForTesting(() => Epoch);
        var pairs = new HashSet<string>();
        foreach (byte[] ticket in tickets)
        {
            string pair = Convert.ToHexString(ticket.AsSpan(5, 16)) + Convert.ToHexString(ticket.AsSpan(37, 44));
            Program.Check(pairs.Add(pair), "concurrent imports never repeat domain/key/nonce triples");
            Decrypt(peer, ticket);
        }
    }
}
