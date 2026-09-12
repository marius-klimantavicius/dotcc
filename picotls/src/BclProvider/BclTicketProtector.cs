using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Managed.Security;

public static unsafe partial class BclCryptoProvider
{
    private const int TicketNotFound = 0x205, TicketRejectEarlyData = 0x209;
    private const int TicketHeaderSize = 49, ImportedTicketHeaderSize = 81, TicketTagSize = 16, MaximumTicketSize = 65535;
    private const ulong TicketEncryptionsPerKey = 1UL << 32;
    private static int liveTicketKeys;
    internal static int LiveTicketKeysForTesting => Volatile.Read(ref liveTicketKeys);
    [StructLayout(LayoutKind.Sequential)]
    private struct TicketContext { public st_ptls_encrypt_ticket_t Header; public nint Handle; }
    private static readonly delegate*<st_ptls_encrypt_ticket_t*, st_ptls_t*, int, st_ptls_buffer_t*, st_ptls_iovec_t, int> TicketPointer = &EncryptTicket;

    private sealed class TicketKey : IDisposable
    {
        internal readonly byte[] Id = new byte[16], NoncePrefix = new byte[4];
        internal readonly byte[]? Master, Salt;
        private bool disposed;
        internal readonly AesGcm Cipher;
        internal ulong Counter;
        internal long LastExpiry;
        internal TicketKey()
        {
            byte[] material = new byte[32];
            try
            {
                RandomNumberGenerator.Fill(material); RandomNumberGenerator.Fill(Id); RandomNumberGenerator.Fill(NoncePrefix);
                Cipher = new AesGcm(material, TicketTagSize);
                Interlocked.Increment(ref liveTicketKeys);
            }
            catch { Clear(); throw; }
            finally { CryptographicOperations.ZeroMemory(material); }
        }
        internal TicketKey(TicketKeyImport source)
        {
            try
            {
                ProviderFaultInjection.BeforeAllocation();
                Master = source.Material.ToArray();
                source.Id.Span.CopyTo(Id);
                ProviderFaultInjection.BeforeAllocation();
                Salt = new byte[32]; RandomNumberGenerator.Fill(Salt);
                ProviderFaultInjection.BeforeAllocation();
                Cipher = DeriveCipher(Salt);
                Interlocked.Increment(ref liveTicketKeys);
            }
            catch { Clear(); throw; }
        }
        internal AesGcm DeriveCipher(ReadOnlySpan<byte> salt)
        {
            Span<byte> material = stackalloc byte[32];
            ReadOnlySpan<byte> label = "Managed.Security.TicketProtector/v2/AES-256-GCM"u8;
            Span<byte> info = stackalloc byte[label.Length + 16];
            label.CopyTo(info); Id.CopyTo(info[label.Length..]);
            try
            {
                HKDF.DeriveKey(HashAlgorithmName.SHA256, Master!, material, salt, info);
                return new AesGcm(material, TicketTagSize);
            }
            finally { CryptographicOperations.ZeroMemory(material); }
        }
        private void Clear()
        {
            CryptographicOperations.ZeroMemory(Id); CryptographicOperations.ZeroMemory(NoncePrefix);
            if (Master != null) CryptographicOperations.ZeroMemory(Master);
            if (Salt != null) CryptographicOperations.ZeroMemory(Salt);
        }
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            try { Cipher.Dispose(); }
            finally { Clear(); Interlocked.Decrement(ref liveTicketKeys); }
        }
    }
    private sealed class TicketState : IDisposable
    {
        internal readonly object Gate = new();
        internal readonly long LifetimeMilliseconds;
        internal readonly int PreviousKeys;
        internal List<TicketKey> Keys;
        internal bool Imported;
        internal nint BoundContext;
        internal bool Disposed;
        internal TicketState(uint lifetimeSeconds, int previousKeys)
        {
            LifetimeMilliseconds = lifetimeSeconds * 1000L; PreviousKeys = previousKeys;
            Keys = new List<TicketKey>(previousKeys + 2) { new TicketKey() };
        }
        internal void Prune(long now)
        {
            // Imported peers may have minted tickets this instance has never seen.
            // Only explicit replacement may remove configured decrypt keys.
            if (Imported) return;
            for (int i = Keys.Count - 1; i >= 1; i--)
                if (i > PreviousKeys || Keys[i].LastExpiry <= now)
                { var key = Keys[i]; Keys.RemoveAt(i); key.Dispose(); }
        }
        internal void Rotate()
        {
            lock (Gate)
            {
                ObjectDisposedException.ThrowIf(Disposed, this);
                if (Imported) throw new InvalidOperationException("Use ImportKeys to rotate an explicitly configured ticket key ring.");
                long now = TicketClock.Now(this);
                var replacement = new TicketKey();
                try
                {
                    // An id collision must not select a different key on decrypt.
                    if (Keys.Any(key => key.Id.AsSpan().SequenceEqual(replacement.Id)))
                        throw new CryptographicException("Ticket key identifier collision; retry rotation.");
                    Keys.Insert(0, replacement);
                }
                catch { replacement.Dispose(); throw; }
                Prune(now);
            }
        }
        internal void Import(ReadOnlySpan<TicketKeyImport> sources)
        {
            lock (Gate)
            {
                ObjectDisposedException.ThrowIf(Disposed, this);
                if (sources.Length is < 1 or > 16) throw new ArgumentOutOfRangeException(nameof(sources), "Import between 1 and 16 ticket keys.");
                foreach (var source in sources)
                    if (source.Id.Length != 16 || source.Material.Length != 64)
                        throw new ArgumentException("Ticket key identifiers require 16 bytes and master material requires 64 bytes.", nameof(sources));
                ProviderFaultInjection.BeforeAllocation();
                var replacement = new List<TicketKey>(sources.Length);
                try
                {
                    foreach (var source in sources)
                    {
                        var key = new TicketKey(source);
                        replacement.Add(key); // Capacity allocated before any secret ownership.
                        foreach (var other in replacement)
                            if (!ReferenceEquals(key, other) && CryptographicOperations.FixedTimeEquals(key.Id, other.Id))
                                throw new ArgumentException("Duplicate imported ticket key identifier.", nameof(sources));
                        foreach (var old in Keys)
                            if (CryptographicOperations.FixedTimeEquals(key.Id, old.Id) &&
                                (old.Master == null || !CryptographicOperations.FixedTimeEquals(key.Master!, old.Master)))
                                throw new ArgumentException("An existing ticket key identifier cannot select different material.", nameof(sources));
                    }
                }
                catch { foreach (var key in replacement) key.Dispose(); throw; }
                var previous = Keys;
                Keys = replacement; Imported = true;
                foreach (var key in previous) key.Dispose();
            }
        }
        public void Dispose()
        {
            lock (Gate)
            {
                if (Disposed) return;
                Disposed = true;
                foreach (var key in Keys) key.Dispose();
                Keys.Clear(); BoundContext = 0;
            }
        }
    }

    /// <summary>Borrowed import input. ImportKeys copies both buffers; callers retain
    /// responsibility for clearing their originals and must not mutate during import.</summary>
    public readonly record struct TicketKeyImport(ReadOnlyMemory<byte> Id, ReadOnlyMemory<byte> Material);

    /// <summary>Owns server ticket protection keys. Bind to exactly one immutable
    /// server context, and retain until all its connections end. Explicit rotation
    /// retains a bounded number of previous keys; extra rotations can invalidate
    /// tickets early. Disposing the owner defers key cleanup while a facade lease
    /// exists. Early data is always rejected; resumed sessions require fresh DHE.</summary>
    public sealed class TicketProtector : IDisposable
    {
        private readonly object lifetimeGate = new();
        private TicketContext* context;
        private int leases;
        private bool disposeRequested, bound;
        public uint LifetimeSeconds { get; }
        public TicketProtector(TimeSpan lifetime, int retainedPreviousKeys = 2)
        {
            if (lifetime.Ticks <= 0 || lifetime.Ticks % TimeSpan.TicksPerSecond != 0 || lifetime.TotalSeconds > 604800)
                throw new ArgumentOutOfRangeException(nameof(lifetime), "Ticket lifetime must be a whole number of seconds between 1 and 604800.");
            if (retainedPreviousKeys is < 0 or > 8) throw new ArgumentOutOfRangeException(nameof(retainedPreviousKeys));
            LifetimeSeconds = checked((uint)(lifetime.Ticks / TimeSpan.TicksPerSecond));
            TicketState? state = null;
            try
            {
                state = new TicketState(LifetimeSeconds, retainedPreviousKeys);
                context = (TicketContext*)AllocateZeroed(sizeof(TicketContext));
                context->Handle = OwnState(state); state = null;
                context->Header.cb = TicketPointer;
            }
            catch
            {
                if (state != null) DisposeState(state);
                Free(); throw;
            }
        }
        public st_ptls_encrypt_ticket_t* Callback
        {
            get
            {
                lock (lifetimeGate)
                {
                    ObjectDisposedException.ThrowIf(disposeRequested, this);
                    return &context->Header;
                }
            }
        }
        public void Rotate()
        {
            lock (lifetimeGate)
            {
                ObjectDisposedException.ThrowIf(disposeRequested, this);
                State<TicketState>(context->Handle).Rotate();
            }
        }
        /// <summary>Atomically replaces the configured ring (1..16 entries). The first
        /// key encrypts; all keys decrypt. Omitted keys are removed immediately. Each
        /// successful import starts a new random derivation domain, including a repeat
        /// import of identical keys. Rotate is unavailable after the first import.</summary>
        public void ImportKeys(ReadOnlySpan<TicketKeyImport> keys)
        {
            lock (lifetimeGate)
            {
                ObjectDisposedException.ThrowIf(disposeRequested, this);
                State<TicketState>(context->Handle).Import(keys);
            }
        }
        internal void SetCounterForTesting(ulong counter)
        {
            lock (lifetimeGate)
            {
                ObjectDisposedException.ThrowIf(disposeRequested, this);
                var state = State<TicketState>(context->Handle);
                lock (state.Gate) state.Keys[0].Counter = counter;
            }
        }
        private void ApplyCore(st_ptls_context_t* target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (bound) throw new InvalidOperationException("A ticket protector can be bound to only one server context.");
            var state = State<TicketState>(context->Handle);
            lock (state.Gate) state.BoundContext = (nint)target;
            target->encrypt_ticket = &context->Header;
            target->ticket_lifetime = LifetimeSeconds;
            target->max_early_data_size = 0;
            target->require_dhe_on_psk = 1;
            bound = true;
        }
        public void ApplyTo(st_ptls_context_t* target)
        {
            lock (lifetimeGate) { ObjectDisposedException.ThrowIf(disposeRequested, this); ApplyCore(target); }
        }
        internal IDisposable RetainAndApply(st_ptls_context_t* target)
        {
            lock (lifetimeGate)
            {
                ObjectDisposedException.ThrowIf(disposeRequested, this);
                var lease = new ProviderLease(ReleaseLease);
                ApplyCore(target); leases++; return lease;
            }
        }
        private void ReleaseLease()
        { lock (lifetimeGate) { if (--leases == 0 && disposeRequested) Free(); } }
        public void Dispose()
        {
            lock (lifetimeGate)
            {
                if (disposeRequested) return;
                disposeRequested = true;
                if (leases == 0) Free();
            }
            GC.SuppressFinalize(this);
        }
        ~TicketProtector() { try { Dispose(); } catch { } }
        private void Free()
        {
            var previous = context; context = null;
            try { if (previous != null) ReleaseState(ref previous->Handle); }
            finally { Libc.free(previous); }
        }
        internal IDisposable UseClockForTesting(Func<DateTimeOffset> clock)
        {
            ArgumentNullException.ThrowIfNull(clock);
            lock (lifetimeGate)
            {
                ObjectDisposedException.ThrowIf(disposeRequested, this);
                return new TicketClock(State<TicketState>(context->Handle), clock);
            }
        }
    }

    // This override exists only for friend tests, is scoped to one instance and
    // thread, and never replaces production key/nonce randomness.
    private sealed class TicketClock : IDisposable
    {
        [ThreadStatic] private static TicketClock? current;
        private readonly TicketClock? parent;
        private readonly TicketState state;
        private readonly Func<DateTimeOffset> clock;
        private readonly int thread = Environment.CurrentManagedThreadId;
        private bool disposed;
        internal TicketClock(TicketState state, Func<DateTimeOffset> clock)
        { this.state = state; this.clock = clock; parent = current; current = this; }
        internal static long Now(TicketState state)
        {
            for (var item = current; item != null; item = item.parent)
                if (ReferenceEquals(item.state, state)) return item.clock().ToUnixTimeMilliseconds();
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
        public void Dispose()
        {
            if (disposed) return;
            if (thread != Environment.CurrentManagedThreadId || current != this)
                throw new InvalidOperationException("Ticket test clocks require stack-order disposal on their creating thread.");
            current = parent; disposed = true;
        }
    }
    private static int EncryptTicket(st_ptls_encrypt_ticket_t* self, st_ptls_t* tls, int encrypt, st_ptls_buffer_t* destination, st_ptls_iovec_t source)
    {
        byte[]? privateResult = null;
        try
        {
            if (!BeginCallback()) return ErrorLibrary;
            if (self == null || destination == null || encrypt is not (0 or 1)) throw new ArgumentException("Invalid ticket callback arguments.");
            int bound = MaximumTicketSize;
            if (source.len > (ulong)bound || (source.@base == null && source.len != 0))
                return encrypt == 0 ? TicketNotFound : ErrorLibrary;
            var input = ReadBytes(source.@base, source.len, bound);
            var state = State<TicketState>(((TicketContext*)self)->Handle);
            lock (state.Gate)
            {
                ObjectDisposedException.ThrowIf(state.Disposed, state);
                if (state.BoundContext != 0 && (tls == null || (nint)Picotls.ptls_get_context(tls) != state.BoundContext))
                    return encrypt == 0 ? TicketNotFound : ErrorLibrary;
                long now = TicketClock.Now(state);
                state.Prune(now);
                int headerSize = state.Imported ? ImportedTicketHeaderSize : TicketHeaderSize;
                if (encrypt != 0)
                {
                    if (input.Length > MaximumTicketSize - headerSize - TicketTagSize) return ErrorLibrary;
                    if (now < 0) throw new InvalidOperationException("Ticket clock precedes the Unix epoch.");
                    var key = state.Keys[0];
                    if (key.Counter >= TicketEncryptionsPerKey) throw new CryptographicException("Rotate ticket keys before the per-key encryption limit.");
                    long expires = checked(now + state.LifetimeMilliseconds);
                    privateResult = new byte[checked(input.Length + headerSize + TicketTagSize)];
                    var header = privateResult.AsSpan(0, headerSize);
                    "DPTK"u8.CopyTo(header); header[4] = state.Imported ? (byte)2 : (byte)1;
                    key.Id.CopyTo(header[5..21]);
                    BinaryPrimitives.WriteInt64BigEndian(header[21..29], now);
                    BinaryPrimitives.WriteInt64BigEndian(header[29..37], expires);
                    key.NoncePrefix.CopyTo(header[37..41]);
                    BinaryPrimitives.WriteUInt64BigEndian(header[41..49], key.Counter++);
                    if (state.Imported) key.Salt!.CopyTo(header[49..81]);
                    key.Cipher.Encrypt(header[37..49], input,
                        privateResult.AsSpan(headerSize, input.Length), privateResult.AsSpan(headerSize + input.Length, TicketTagSize), header);
                    key.LastExpiry = Math.Max(key.LastExpiry, expires);
                }
                else
                {
                    if (input.Length < headerSize + TicketTagSize || !input[..4].SequenceEqual("DPTK"u8) || input[4] != (state.Imported ? 2 : 1))
                        return TicketNotFound;
                    var header = input[..headerSize];
                    TicketKey? key = null;
                    foreach (var candidate in state.Keys)
                        if (CryptographicOperations.FixedTimeEquals(header[5..21], candidate.Id)) { key = candidate; break; }
                    if (key == null) return TicketNotFound;
                    int length = input.Length - headerSize - TicketTagSize;
                    privateResult = new byte[length];
                    try
                    {
                        using var derived = state.Imported ? key.DeriveCipher(header[49..81]) : null;
                        (derived ?? key.Cipher).Decrypt(header[37..49], input.Slice(headerSize, length), input[^TicketTagSize..], privateResult, header);
                    }
                    catch (AuthenticationTagMismatchException) { return TicketNotFound; }
                    long issued = BinaryPrimitives.ReadInt64BigEndian(header[21..29]);
                    long expires = BinaryPrimitives.ReadInt64BigEndian(header[29..37]);
                    if (issued < 0 || now < issued || now >= expires || expires - issued != state.LifetimeMilliseconds)
                        return TicketNotFound;
                }
            }
            // Authenticate and validate everything in private storage first.
            // In particular, an untrusted ticket never reserves or mutates dst.
            ulong count = (ulong)privateResult.Length;
            if (destination->off > destination->capacity || count > ulong.MaxValue - destination->off)
                throw new ArgumentException("Invalid destination ticket buffer.");
            int result = Picotls.ptls_buffer_reserve(destination, count);
            if (result != 0) return result;
            privateResult.CopyTo(WriteBytes(destination->@base + destination->off, count, MaximumTicketSize));
            destination->off += count;
            return encrypt != 0 ? 0 : TicketRejectEarlyData;
        }
        catch (Exception error) { return SetupError(error); }
        finally { if (privateResult != null) CryptographicOperations.ZeroMemory(privateResult); }
    }
}
