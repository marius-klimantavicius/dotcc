#nullable enable
using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace Edg.Runtime;

/// <summary>
/// GNU/C atomic storage primitives. Orders currently use stronger sequentially
/// consistent operations. Same-width Interlocked paths follow dotcc's
/// AtomicLib/GnuAtomicLib; unaligned and larger objects use shared address locks.
/// This is managed ABI support, not an interoperation guarantee with native
/// atomics or independently generated runtime assemblies.
/// </summary>
public static unsafe class AtomicOps
{
    private static readonly object[] gates = CreateGates();
    private static readonly bool primitiveInterlocked =
        RuntimeInformation.ProcessArchitecture is Architecture.X64 or Architecture.Arm64;

    private static object[] CreateGates()
    {
        var result = new object[256];
        for (int i = 0; i != result.Length; ++i) result[i] = new object();
        return result;
    }

    private static object Gate(void* address)
    {
        nuint bits = (nuint)address;
        return gates[(int)(((bits >> 3) ^ (bits >> 11)) & 255)];
    }

    public static bool IsLockFree(nuint size, void* address) =>
        primitiveInterlocked && (size == 1 || size == 2 || size == 4 || size == 8) &&
        ((nuint)address & (size - 1)) == 0;

    private static void Order(int order, bool load = false, bool store = false)
    {
        if (order is < 0 or > 5 || load && order is 3 or 4 || store && order is 1 or 2 or 4)
            throw new ArgumentOutOfRangeException(nameof(order), "Invalid atomic memory order");
    }

    private static bool BitsEqual<T>(T left, T right) where T : unmanaged =>
        new ReadOnlySpan<byte>(&left, sizeof(T)).SequenceEqual(new ReadOnlySpan<byte>(&right, sizeof(T)));

    private static T ValueCompareExchange<T>(ref T location, T desired, T comparand) where T : unmanaged
    {
        // Pinning the byref also makes the fallback safe for managed host
        // consumers: GC cannot change its lock stripe while an operation waits.
        // Translated globals/TLS are already pinned; frames/heaps are native.
        fixed (T* address = &location)
        {
            if (!IsLockFree((nuint)sizeof(T), address))
            {
                lock (Gate(address))
                {
                    Interlocked.MemoryBarrier();
                    T observed = Unsafe.ReadUnaligned<T>(address);
                    if (BitsEqual(observed, comparand)) Unsafe.WriteUnaligned(address, desired);
                    Interlocked.MemoryBarrier();
                    return observed;
                }
            }
            if (sizeof(T) == 1)
            {
                byte observed = Interlocked.CompareExchange(ref Unsafe.As<T, byte>(ref location),
                    Unsafe.As<T, byte>(ref desired), Unsafe.As<T, byte>(ref comparand));
                return Unsafe.As<byte, T>(ref observed);
            }
            if (sizeof(T) == 2)
            {
                short observed = Interlocked.CompareExchange(ref Unsafe.As<T, short>(ref location),
                    Unsafe.As<T, short>(ref desired), Unsafe.As<T, short>(ref comparand));
                return Unsafe.As<short, T>(ref observed);
            }
            if (sizeof(T) == 4)
            {
                int observed = Interlocked.CompareExchange(ref Unsafe.As<T, int>(ref location),
                    Unsafe.As<T, int>(ref desired), Unsafe.As<T, int>(ref comparand));
                return Unsafe.As<int, T>(ref observed);
            }
            long wide = Interlocked.CompareExchange(ref Unsafe.As<T, long>(ref location),
                Unsafe.As<T, long>(ref desired), Unsafe.As<T, long>(ref comparand));
            return Unsafe.As<long, T>(ref wide);
        }
    }

    public static T Load<T>(ref T location, int order) where T : unmanaged
    {
        Order(order, load: true);
        return ValueCompareExchange(ref location, default, default);
    }

    public static T Store<T>(ref T location, T value, int order) where T : unmanaged
    {
        Order(order, store: true);
        ExchangeCore(ref location, value);
        return value;
    }

    private static T ExchangeCore<T>(ref T location, T value) where T : unmanaged
    {
        fixed (T* address = &location)
        {
            if (!IsLockFree((nuint)sizeof(T), address))
            {
                lock (Gate(address))
                {
                    Interlocked.MemoryBarrier();
                    T observed = Unsafe.ReadUnaligned<T>(address);
                    Unsafe.WriteUnaligned(address, value);
                    Interlocked.MemoryBarrier();
                    return observed;
                }
            }
            if (sizeof(T) == 1)
            {
                byte observed = Interlocked.Exchange(ref Unsafe.As<T, byte>(ref location), Unsafe.As<T, byte>(ref value));
                return Unsafe.As<byte, T>(ref observed);
            }
            if (sizeof(T) == 2)
            {
                short observed = Interlocked.Exchange(ref Unsafe.As<T, short>(ref location), Unsafe.As<T, short>(ref value));
                return Unsafe.As<short, T>(ref observed);
            }
            if (sizeof(T) == 4)
            {
                int observed = Interlocked.Exchange(ref Unsafe.As<T, int>(ref location), Unsafe.As<T, int>(ref value));
                return Unsafe.As<int, T>(ref observed);
            }
            long wide = Interlocked.Exchange(ref Unsafe.As<T, long>(ref location), Unsafe.As<T, long>(ref value));
            return Unsafe.As<long, T>(ref wide);
        }
    }

    public static T Exchange<T>(ref T location, T value, int order) where T : unmanaged
    { Order(order); return ExchangeCore(ref location, value); }

    public static bool CompareExchange<T>(ref T location, ref T expected, T desired,
        bool weak, int success, int failure) where T : unmanaged
    {
        CompareOrders(success, failure);
        // A non-spurious strong implementation also satisfies weak CAS.
        T observed = ValueCompareExchange(ref location, desired, expected);
        if (BitsEqual(observed, expected)) return true;
        expected = observed;
        return false;
    }

    private static void CompareOrders(int success, int failure)
    {
        Order(success);
        Order(failure, load: true);
        bool allowed = failure switch
        {
            0 => true,
            1 => success is 1 or 2 or 4 or 5,
            2 => success is 2 or 4 or 5,
            5 => success == 5,
            _ => false,
        };
        if (!allowed) throw new ArgumentOutOfRangeException(nameof(failure), "Failure order exceeds success order");
    }

    private static void CopyBytes(void* source, void* destination, nuint size)
    {
        // Span.CopyTo handles overlap. Process oversized buffers in the
        // direction that preserves unread source bytes, without a managed array.
        bool backwards = (nuint)destination > (nuint)source &&
                         (nuint)destination - (nuint)source < size;
        nuint remaining = size, offset = 0;
        while (remaining != 0)
        {
            int chunk = remaining > int.MaxValue ? int.MaxValue : (int)remaining;
            nuint position = backwards ? remaining - (nuint)chunk : offset;
            new ReadOnlySpan<byte>((void*)((nuint)source + position), chunk).CopyTo(
                new Span<byte>((void*)((nuint)destination + position), chunk));
            remaining -= (nuint)chunk;
            offset += (nuint)chunk;
        }
    }

    private static bool EqualBytes(void* left, void* right, nuint size)
    {
        for (nuint offset = 0; offset != size;)
        {
            int chunk = size - offset > int.MaxValue ? int.MaxValue : (int)(size - offset);
            if (!new ReadOnlySpan<byte>((void*)((nuint)left + offset), chunk).SequenceEqual(
                    new ReadOnlySpan<byte>((void*)((nuint)right + offset), chunk))) return false;
            offset += (nuint)chunk;
        }
        return true;
    }

    // Generic EDG atomic builtins carry a leading byte count in lowered IL.
    // Raw pointers must reference stable native or pinned translated storage.
    public static void LoadBytes(ulong count, void* address, void* result, int order)
    {
        Order(order, load: true);
        switch (count)
        {
            case 1: Unsafe.WriteUnaligned(result, Load(ref *(byte*)address, order)); return;
            case 2: Unsafe.WriteUnaligned(result, Load(ref *(ushort*)address, order)); return;
            case 4: Unsafe.WriteUnaligned(result, Load(ref *(uint*)address, order)); return;
            case 8: Unsafe.WriteUnaligned(result, Load(ref *(ulong*)address, order)); return;
        }
        lock (Gate(address))
        {
            Interlocked.MemoryBarrier();
            CopyBytes(address, result, checked((nuint)count));
            Interlocked.MemoryBarrier();
        }
    }

    public static void StoreBytes(ulong count, void* address, void* value, int order)
    {
        Order(order, store: true);
        switch (count)
        {
            case 1: Store(ref *(byte*)address, Unsafe.ReadUnaligned<byte>(value), order); return;
            case 2: Store(ref *(ushort*)address, Unsafe.ReadUnaligned<ushort>(value), order); return;
            case 4: Store(ref *(uint*)address, Unsafe.ReadUnaligned<uint>(value), order); return;
            case 8: Store(ref *(ulong*)address, Unsafe.ReadUnaligned<ulong>(value), order); return;
        }
        lock (Gate(address))
        {
            Interlocked.MemoryBarrier();
            CopyBytes(value, address, checked((nuint)count));
            Interlocked.MemoryBarrier();
        }
    }

    public static void ExchangeBytes(ulong count, void* address, void* value, void* result, int order)
    {
        Order(order);
        switch (count)
        {
            case 1: Unsafe.WriteUnaligned(result, Exchange(ref *(byte*)address, Unsafe.ReadUnaligned<byte>(value), order)); return;
            case 2: Unsafe.WriteUnaligned(result, Exchange(ref *(ushort*)address, Unsafe.ReadUnaligned<ushort>(value), order)); return;
            case 4: Unsafe.WriteUnaligned(result, Exchange(ref *(uint*)address, Unsafe.ReadUnaligned<uint>(value), order)); return;
            case 8: Unsafe.WriteUnaligned(result, Exchange(ref *(ulong*)address, Unsafe.ReadUnaligned<ulong>(value), order)); return;
            case 16: Unsafe.WriteUnaligned(result, Exchange(ref *(UInt128*)address, Unsafe.ReadUnaligned<UInt128>(value), order)); return;
        }
        nuint size = checked((nuint)count);
        byte* stack = stackalloc byte[size <= 512 ? (int)size : 0];
        void* saved = stack;
        if (size > 512)
        {
            try { saved = NativeMemory.Alloc(size); }
            catch (OutOfMemoryException error)
            { Environment.FailFast("Unable to allocate atomic exchange temporary", error); }
        }
        try
        {
            lock (Gate(address))
            {
                Interlocked.MemoryBarrier();
                CopyBytes(address, saved, size);
                CopyBytes(value, address, size);
                CopyBytes(saved, result, size);
                Interlocked.MemoryBarrier();
            }
        }
        finally { if (size > 512) NativeMemory.Free(saved); }
    }

    public static bool CompareExchangeBytes(ulong count, void* address, void* expected,
        void* desired, bool weak, int success, int failure)
    {
        CompareOrders(success, failure);
        // Use exactly the same synchronization as typed operations on a given
        // address; raw and typed accesses must be allowed to mix.
        switch (count)
        {
            case 1: return CompareExchange(ref *(byte*)address, ref *(byte*)expected, Unsafe.ReadUnaligned<byte>(desired), weak, success, failure);
            case 2: return CompareExchange(ref *(ushort*)address, ref *(ushort*)expected, Unsafe.ReadUnaligned<ushort>(desired), weak, success, failure);
            case 4: return CompareExchange(ref *(uint*)address, ref *(uint*)expected, Unsafe.ReadUnaligned<uint>(desired), weak, success, failure);
            case 8: return CompareExchange(ref *(ulong*)address, ref *(ulong*)expected, Unsafe.ReadUnaligned<ulong>(desired), weak, success, failure);
        }
        lock (Gate(address))
        {
            Interlocked.MemoryBarrier();
            nuint size = checked((nuint)count);
            bool equal = EqualBytes(address, expected, size);
            if (equal) CopyBytes(desired, address, size);
            else CopyBytes(address, expected, size);
            Interlocked.MemoryBarrier();
            return equal;
        }
    }

    private static bool TryCas<T>(ref T location, T desired, T comparand) where T : unmanaged =>
        BitsEqual(ValueCompareExchange(ref location, desired, comparand), comparand);

    public static T FetchAdd<T>(ref T location, T value, int order) where T : unmanaged, INumber<T>
    { Order(order); T old; do { old = Load(ref location, 0); } while (!TryCas(ref location, unchecked(old + value), old)); return old; }
    public static T AddFetch<T>(ref T location, T value, int order) where T : unmanaged, INumber<T>
    { Order(order); T old, next; do { old = Load(ref location, 0); next = unchecked(old + value); } while (!TryCas(ref location, next, old)); return next; }
    public static T FetchSub<T>(ref T location, T value, int order) where T : unmanaged, INumber<T>
    { Order(order); T old; do { old = Load(ref location, 0); } while (!TryCas(ref location, unchecked(old - value), old)); return old; }
    public static T SubFetch<T>(ref T location, T value, int order) where T : unmanaged, INumber<T>
    { Order(order); T old, next; do { old = Load(ref location, 0); next = unchecked(old - value); } while (!TryCas(ref location, next, old)); return next; }

    // operation: 0=and, 1=or, 2=xor, 3=nand. GNU nand is ~(old & value).
    private static T Bitwise<T>(ref T location, T value, int order, int operation, bool after)
        where T : unmanaged, IBinaryInteger<T>
    {
        Order(order);
        T old, next;
        do
        {
            old = Load(ref location, 0);
            next = operation switch { 0 => old & value, 1 => old | value, 2 => old ^ value, _ => ~(old & value) };
        } while (!TryCas(ref location, next, old));
        return after ? next : old;
    }
    public static T FetchAnd<T>(ref T location, T value, int order) where T : unmanaged, IBinaryInteger<T> => Bitwise(ref location, value, order, 0, false);
    public static T AndFetch<T>(ref T location, T value, int order) where T : unmanaged, IBinaryInteger<T> => Bitwise(ref location, value, order, 0, true);
    public static T FetchOr<T>(ref T location, T value, int order) where T : unmanaged, IBinaryInteger<T> => Bitwise(ref location, value, order, 1, false);
    public static T OrFetch<T>(ref T location, T value, int order) where T : unmanaged, IBinaryInteger<T> => Bitwise(ref location, value, order, 1, true);
    public static T FetchXor<T>(ref T location, T value, int order) where T : unmanaged, IBinaryInteger<T> => Bitwise(ref location, value, order, 2, false);
    public static T XorFetch<T>(ref T location, T value, int order) where T : unmanaged, IBinaryInteger<T> => Bitwise(ref location, value, order, 2, true);
    public static T FetchNand<T>(ref T location, T value, int order) where T : unmanaged, IBinaryInteger<T> => Bitwise(ref location, value, order, 3, false);
    public static T NandFetch<T>(ref T location, T value, int order) where T : unmanaged, IBinaryInteger<T> => Bitwise(ref location, value, order, 3, true);

    public static bool TestAndSet(ref byte location, int order) => Exchange(ref location, (byte)1, order) != 0;
    public static void Clear(ref byte location, int order) => Store(ref location, (byte)0, order);
    public static void ThreadFence(int order) { Order(order); Interlocked.MemoryBarrier(); }
    public static void SignalFence(int order) { Order(order); Interlocked.MemoryBarrier(); }
}
