#nullable enable

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace DotCC.Libc;

/// <summary>
/// Seq-cst atomic primitives backing C11 <c>_Atomic</c> and <c>&lt;stdatomic.h&gt;</c>.
/// C11 atomic operations default to <c>memory_order_seq_cst</c>; these use
/// <see cref="Interlocked"/> (full fences) on a SAME-WIDTH integer reinterpretation
/// of the location, so any 1-, 2-, 4-, or 8-byte unmanaged scalar — <c>int</c>/<c>uint</c>/
/// <c>long</c>/<c>ulong</c>/<c>nint</c>/<c>nuint</c>/<c>float</c>/<c>double</c> —
/// is covered by one generic implementation. The compare-and-swap loops do the
/// arithmetic/bitwise step in the value type's own space (<see cref="INumber{T}"/> /
/// <see cref="IBinaryInteger{T}"/>) and the CAS compares BIT patterns, so
/// <c>float</c>/<c>double</c> (and <c>±0</c>/<c>NaN</c>) behave correctly.
/// </summary>
/// <remarks>
/// Every access uses a same-width Interlocked overload. Pointer callers must
/// reinterpret pointer storage as nint because pointers cannot be generic type
/// arguments. The C11 frontend retains its documented eligibility rules; GNU
/// builtins also route narrow integer objects through these same primitives.
/// Fence note (same as <c>volatile</c>): on .NET these are full barriers, which is
/// at least as strong as C11 seq-cst requires.
/// <para>
/// The functions come in two RMW flavours mirroring C: <c>FetchX</c> returns the
/// OLD value (what C11's <c>atomic_fetch_*</c> yields), and <c>XFetch</c> returns
/// the NEW value (what the compound assignment <c>x += n</c> yields).
/// </para>
/// </remarks>
public static class Atomic
{
    // ---- same-width load / store / exchange / compare-exchange -------------

    public static T Load<T>(ref T loc) where T : unmanaged =>
        ValueCompareExchange(ref loc, default, default);

    // Returns the stored value, so the C assignment expression x = v yields v.
    public static T Store<T>(ref T loc, T value) where T : unmanaged
    {
        Exchange(ref loc, value);
        return value;
    }

    public static T Exchange<T>(ref T loc, T value) where T : unmanaged
    {
        if (Unsafe.SizeOf<T>() == 1)
        {
            byte old = Interlocked.Exchange(ref Unsafe.As<T, byte>(ref loc), Unsafe.As<T, byte>(ref value));
            return Unsafe.As<byte, T>(ref old);
        }
        if (Unsafe.SizeOf<T>() == 2)
        {
            short old = Interlocked.Exchange(ref Unsafe.As<T, short>(ref loc), Unsafe.As<T, short>(ref value));
            return Unsafe.As<short, T>(ref old);
        }
        if (Unsafe.SizeOf<T>() == 4)
        {
            int old = Interlocked.Exchange(ref Unsafe.As<T, int>(ref loc), Unsafe.As<T, int>(ref value));
            return Unsafe.As<int, T>(ref old);
        }
        if (Unsafe.SizeOf<T>() == 8)
        {
            long old = Interlocked.Exchange(ref Unsafe.As<T, long>(ref loc), Unsafe.As<T, long>(ref value));
            return Unsafe.As<long, T>(ref old);
        }
        throw new global::System.NotSupportedException("Atomic objects must be 1, 2, 4, or 8 bytes");
    }

    /// <summary>The value observed by the single CAS operation, whether or not
    /// the comparison succeeded. No second load can race with this observation.</summary>
    public static T ValueCompareExchange<T>(ref T loc, T desired, T comparand) where T : unmanaged
    {
        if (Unsafe.SizeOf<T>() == 1)
        {
            byte old = Interlocked.CompareExchange(ref Unsafe.As<T, byte>(ref loc),
                Unsafe.As<T, byte>(ref desired), Unsafe.As<T, byte>(ref comparand));
            return Unsafe.As<byte, T>(ref old);
        }
        if (Unsafe.SizeOf<T>() == 2)
        {
            short old = Interlocked.CompareExchange(ref Unsafe.As<T, short>(ref loc),
                Unsafe.As<T, short>(ref desired), Unsafe.As<T, short>(ref comparand));
            return Unsafe.As<short, T>(ref old);
        }
        if (Unsafe.SizeOf<T>() == 4)
        {
            int old = Interlocked.CompareExchange(ref Unsafe.As<T, int>(ref loc),
                Unsafe.As<T, int>(ref desired), Unsafe.As<T, int>(ref comparand));
            return Unsafe.As<int, T>(ref old);
        }
        if (Unsafe.SizeOf<T>() == 8)
        {
            long old = Interlocked.CompareExchange(ref Unsafe.As<T, long>(ref loc),
                Unsafe.As<T, long>(ref desired), Unsafe.As<T, long>(ref comparand));
            return Unsafe.As<long, T>(ref old);
        }
        throw new global::System.NotSupportedException("Atomic objects must be 1, 2, 4, or 8 bytes");
    }

    private static bool BitsEqual<T>(T a, T b) where T : unmanaged => Unsafe.SizeOf<T>() switch
    {
        1 => Unsafe.As<T, byte>(ref a) == Unsafe.As<T, byte>(ref b),
        2 => Unsafe.As<T, short>(ref a) == Unsafe.As<T, short>(ref b),
        4 => Unsafe.As<T, int>(ref a) == Unsafe.As<T, int>(ref b),
        8 => Unsafe.As<T, long>(ref a) == Unsafe.As<T, long>(ref b),
        _ => throw new global::System.NotSupportedException("Atomic objects must be 1, 2, 4, or 8 bytes"),
    };

    public static bool CompareExchangeMatches<T>(ref T loc, T desired, T comparand) where T : unmanaged =>
        BitsEqual(ValueCompareExchange(ref loc, desired, comparand), comparand);

    private static bool TryCas<T>(ref T loc, T desired, T comparand) where T : unmanaged =>
        CompareExchangeMatches(ref loc, desired, comparand);

    /// <summary>C11 strong CAS; a failure writes the value observed by the CAS
    /// to expected. Weak CAS may use the same non-spurious implementation.</summary>
    public static bool CompareExchange<T>(ref T loc, ref T expected, T desired) where T : unmanaged
    {
        T observed = ValueCompareExchange(ref loc, desired, expected);
        if (BitsEqual(observed, expected)) return true;
        expected = observed;
        return false;
    }

    // ---- arithmetic RMW (INumber): Fetch* = old, *Fetch = new --------------

    public static T FetchAdd<T>(ref T loc, T arg) where T : unmanaged, INumber<T>
    { T old; do { old = Load(ref loc); } while (!TryCas(ref loc, old + arg, old)); return old; }
    public static T AddFetch<T>(ref T loc, T arg) where T : unmanaged, INumber<T>
    { T old, neu; do { old = Load(ref loc); neu = old + arg; } while (!TryCas(ref loc, neu, old)); return neu; }

    public static T FetchSub<T>(ref T loc, T arg) where T : unmanaged, INumber<T>
    { T old; do { old = Load(ref loc); } while (!TryCas(ref loc, old - arg, old)); return old; }
    public static T SubFetch<T>(ref T loc, T arg) where T : unmanaged, INumber<T>
    { T old, neu; do { old = Load(ref loc); neu = old - arg; } while (!TryCas(ref loc, neu, old)); return neu; }

    // ---- bitwise RMW (IBinaryInteger) --------------------------------------

    public static T FetchAnd<T>(ref T loc, T arg) where T : unmanaged, IBinaryInteger<T>
    { T old; do { old = Load(ref loc); } while (!TryCas(ref loc, old & arg, old)); return old; }
    public static T AndFetch<T>(ref T loc, T arg) where T : unmanaged, IBinaryInteger<T>
    { T old, neu; do { old = Load(ref loc); neu = old & arg; } while (!TryCas(ref loc, neu, old)); return neu; }

    public static T FetchOr<T>(ref T loc, T arg) where T : unmanaged, IBinaryInteger<T>
    { T old; do { old = Load(ref loc); } while (!TryCas(ref loc, old | arg, old)); return old; }
    public static T OrFetch<T>(ref T loc, T arg) where T : unmanaged, IBinaryInteger<T>
    { T old, neu; do { old = Load(ref loc); neu = old | arg; } while (!TryCas(ref loc, neu, old)); return neu; }

    public static T FetchXor<T>(ref T loc, T arg) where T : unmanaged, IBinaryInteger<T>
    { T old; do { old = Load(ref loc); } while (!TryCas(ref loc, old ^ arg, old)); return old; }
    public static T XorFetch<T>(ref T loc, T arg) where T : unmanaged, IBinaryInteger<T>
    { T old, neu; do { old = Load(ref loc); neu = old ^ arg; } while (!TryCas(ref loc, neu, old)); return neu; }

    // ---- fences ------------------------------------------------------------
    // C11 atomic_thread_fence / atomic_signal_fence. A seq-cst thread fence is a
    // full memory barrier; the signal fence is a compiler barrier (single-thread
    // ordering w.r.t. a signal handler) — on .NET the conservative mapping is also
    // a full barrier. The memory_order argument is accepted and ignored (every
    // order we honour maps to "at least a full barrier here").
    public static void ThreadFence() => Interlocked.MemoryBarrier();
    public static void SignalFence() => Interlocked.MemoryBarrier();
}
