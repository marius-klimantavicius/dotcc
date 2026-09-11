using System.Security.Cryptography;
using Managed.Security;

static unsafe class TicketVectors
{
    internal static void Run()
    {
        int handles = BclCryptoProvider.LiveManagedContexts, keys = BclCryptoProvider.LiveTicketKeysForTesting;
        RoundTripsAndRotation(); BindingAndLifetime(); AllocationFailures();
        Program.Check(BclCryptoProvider.LiveManagedContexts == handles && BclCryptoProvider.LiveTicketKeysForTesting == keys,
            "ticket callbacks release all keys and managed handles");
    }
    private static byte[] Invoke(BclCryptoProvider.TicketProtector protector, bool encrypt, byte[] input, int expected)
    {
        using var scope = CallbackScope.Enter();
        st_ptls_buffer_t destination = PicotlsBuffer.Create();
        Program.Check(Picotls.ptls_buffer_reserve(&destination, 8) == 0, "ticket test destination reserve");
        new Span<byte>(destination.@base, 4).Fill(0xa5); destination.off = 4;
        byte* priorBase = destination.@base;
        ulong priorCapacity = destination.capacity;
        try
        {
            fixed (byte* source = input)
            {
                var callback = protector.Callback;
                int result = callback->cb(callback, null, encrypt ? 1 : 0, &destination, Program.Vector(source, input.Length));
                scope.ThrowIfFailed();
                Program.Check(result == expected, "ticket callback status");
            }
            Program.Check(new ReadOnlySpan<byte>(destination.@base, 4).IndexOfAnyExcept((byte)0xa5) < 0, "ticket append preserves destination prefix");
            if (expected == 0x205)
            {
                Program.Check(destination.@base == priorBase && destination.capacity == priorCapacity && destination.off == 4,
                    "rejected ticket leaves destination allocation and offset unchanged");
                return [];
            }
            return new ReadOnlySpan<byte>(destination.@base + 4, checked((int)destination.off - 4)).ToArray();
        }
        finally
        {
            CryptographicOperations.ZeroMemory(new Span<byte>(destination.@base, checked((int)destination.off)));
            Picotls.dotcc_ptls_buffer_dispose(&destination);
        }
    }
    private static void RoundTripsAndRotation()
    {
        using var protector = new BclCryptoProvider.TicketProtector(TimeSpan.FromSeconds(10), retainedPreviousKeys: 1);
        DateTimeOffset now = DateTimeOffset.FromUnixTimeSeconds(1800000000);
        using var clock = protector.UseClockForTesting(() => now);
        byte[] plaintext = "opaque core ticket state"u8.ToArray();
        byte[] first = Invoke(protector, true, plaintext, 0), second = Invoke(protector, true, plaintext, 0);
        Program.Check(!first.AsSpan(37, 12).SequenceEqual(second.AsSpan(37, 12)), "ticket per-key nonces are unique");
        Program.Check(first.AsSpan(5, 16).SequenceEqual(second.AsSpan(5, 16)), "ticket key identifier stable before rotation");
        Program.Equal(Invoke(protector, false, first, 0x209), plaintext, "ticket authenticated decrypt rejects early data");
        foreach (int offset in new[] { 0, 4, 5, 21, 29, 37, 49, first.Length - 1 })
        {
            byte[] tampered = first.ToArray(); tampered[offset] ^= 1;
            Invoke(protector, false, tampered, 0x205);
        }
        foreach (int size in new[] { 0, 1, 12, 64 }) Invoke(protector, false, first[..size], 0x205);
        using (var other = new BclCryptoProvider.TicketProtector(TimeSpan.FromSeconds(10)))
            Invoke(other, false, first, 0x205);
        now = now.AddMilliseconds(-1); Invoke(protector, false, first, 0x205);
        now = now.AddMilliseconds(1);
        now = now.AddSeconds(10); Invoke(protector, false, first, 0x205); // expiry is exclusive
        now = now.AddSeconds(-10);
        protector.Rotate();
        Program.Equal(Invoke(protector, false, first, 0x209), plaintext, "one previous key retained after rotation");
        byte[] newer = Invoke(protector, true, plaintext, 0);
        Program.Check(!first.AsSpan(5, 16).SequenceEqual(newer.AsSpan(5, 16)), "rotation generates independent key identifier");
        protector.Rotate();
        Invoke(protector, false, first, 0x205); // bounded retention invalidates the oldest key early
        Program.Equal(Invoke(protector, false, newer, 0x209), plaintext, "newer previous key remains usable");
        Program.Check(BclCryptoProvider.LiveTicketKeysForTesting == 2, "previous-key retention is bounded");
        now = now.AddSeconds(11); protector.Rotate();
        Program.Check(BclCryptoProvider.LiveTicketKeysForTesting == 1, "expired retired keys are disposed during rotation");
        byte[] empty = Invoke(protector, true, [], 0);
        Program.Equal(Invoke(protector, false, empty, 0x209), [], "empty opaque ticket state round trip");
    }
    private static void BindingAndLifetime()
    {
        int handles = BclCryptoProvider.LiveManagedContexts, keys = BclCryptoProvider.LiveTicketKeysForTesting;
        var protector = new BclCryptoProvider.TicketProtector(TimeSpan.FromHours(1));
        st_ptls_context_t first = default, second = default;
        var lease = protector.RetainAndApply(&first);
        Program.Check(first.encrypt_ticket != null && first.ticket_lifetime == 3600 && first.max_early_data_size == 0 && first.require_dhe_on_psk == 1,
            "ticket context configuration disables early data and requires fresh DHE");
        bool rejected = false;
        try { protector.ApplyTo(&second); } catch (InvalidOperationException) { rejected = true; }
        Program.Check(rejected && second.encrypt_ticket == null, "ticket keys cannot be shared across context policies");
        protector.Dispose();
        Program.Check(BclCryptoProvider.LiveManagedContexts == handles + 1 && BclCryptoProvider.LiveTicketKeysForTesting == keys + 1,
            "active context lease retains keys after owner disposal");
        bool disposed = false;
        try { protector.Rotate(); } catch (ObjectDisposedException) { disposed = true; }
        Program.Check(disposed, "disposed ticket owner rejects rotation");
        lease.Dispose();
        Program.Check(BclCryptoProvider.LiveManagedContexts == handles && BclCryptoProvider.LiveTicketKeysForTesting == keys,
            "last context lease releases ticket key and callback state");
        foreach (TimeSpan lifetime in new[] { TimeSpan.Zero, TimeSpan.FromMilliseconds(1500), TimeSpan.FromDays(8) })
        {
            bool invalid = false;
            try { using var unexpected = new BclCryptoProvider.TicketProtector(lifetime); }
            catch (ArgumentOutOfRangeException) { invalid = true; }
            Program.Check(invalid, "invalid ticket lifetime rejected");
        }
    }
    private static void AllocationFailures()
    {
        int handles = BclCryptoProvider.LiveManagedContexts, keys = BclCryptoProvider.LiveTicketKeysForTesting;
        for (int ordinal = 1; ordinal <= 2; ordinal++)
        {
            using var fault = ProviderFaultInjection.FailAllocation(ordinal);
            bool failed = false;
            try { using var unexpected = new BclCryptoProvider.TicketProtector(TimeSpan.FromSeconds(10)); }
            catch (OutOfMemoryException) { failed = true; }
            Program.Check(failed && fault.DisposedStates == 1 && BclCryptoProvider.LiveManagedContexts == handles && BclCryptoProvider.LiveTicketKeysForTesting == keys,
                "ticket context/handle allocation failures dispose unpublished key state");
        }
    }
}
