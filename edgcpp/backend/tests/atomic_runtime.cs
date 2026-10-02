#nullable enable
using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Edg.Runtime;

internal static unsafe class AtomicRuntimeTest
{
    private static void Require(bool condition, string message)
    { if (!condition) throw new Exception(message); }

    private static void Counter<T>(int offset) where T : unmanaged, IBinaryInteger<T>
    {
        byte* storage = (byte*)NativeMemory.Alloc(96);
        try
        {
            new Span<byte>(storage, 96).Fill(0x5a);
            byte* address = storage + 32 + offset;
            ref T location = ref Unsafe.AsRef<T>(address);
            AtomicOps.Store(ref location, T.Zero, 5);
            const int workers = 4, iterations = 6000;
            nuint captured = (nuint)address;
            var elapsed = Stopwatch.StartNew();
            Parallel.For(0, workers, worker => {
                for (int i = 0; i != iterations; ++i)
                    AtomicOps.FetchAdd(ref Unsafe.AsRef<T>((void*)captured), T.One, i % 6);
            });
            Require(AtomicOps.Load(ref location, 5) == T.CreateTruncating(workers * iterations),
                    $"lost update {typeof(T)} offset={offset}");
            for (int i = 0; i != 96; ++i)
                if (i < 32 + offset || i >= 32 + offset + sizeof(T))
                    Require(storage[i] == 0x5a, "atomic operation touched a neighbor byte");
            bool expectedFree = sizeof(T) <= 8 && (offset == 0 || sizeof(T) == 1) &&
                RuntimeInformation.ProcessArchitecture is Architecture.X64 or Architecture.Arm64;
            Require(AtomicOps.IsLockFree((nuint)sizeof(T), address) == expectedFree, "lock-free claim");
            Console.WriteLine($"counter:{typeof(T).Name}:offset={offset}:operations={workers * iterations}:ms={elapsed.ElapsedMilliseconds}:lockFree={expectedFree}");
        }
        finally { NativeMemory.Free(storage); }
    }

    private static void CasAndBits()
    {
        long value = long.MaxValue;
        Require(AtomicOps.FetchAdd(ref value, 1L, 5) == long.MaxValue && value == long.MinValue, "signed wrap");
        Require(AtomicOps.SubFetch(ref value, 1L, 5) == long.MaxValue, "signed subtraction wrap");
        byte narrow = 255;
        Require(AtomicOps.AddFetch(ref narrow, (byte)1, 0) == 0, "narrow wrap");
        int n = 12, expected = 7;
        Require(!AtomicOps.CompareExchange(ref n, ref expected, 18, true, 4, 2) && expected == 12 && n == 12,
                "failed CAS must return its observed value");
        Require(AtomicOps.CompareExchange(ref n, ref expected, 18, false, 5, 5) && expected == 12 && n == 18,
                "successful CAS preserves expected");
        float zero = -0.0F, plus = 0.0F;
        Require(!AtomicOps.CompareExchange(ref zero, ref plus, 1.0F, false, 5, 5) &&
                BitConverter.SingleToUInt32Bits(plus) == 0x80000000, "CAS distinguishes signed zero");
        float nan = BitConverter.UInt32BitsToSingle(0x7fc00123), same = nan;
        Require(AtomicOps.CompareExchange(ref nan, ref same, 2.0F, false, 5, 5) && nan == 2.0F,
                "CAS compares NaN representation");
        UInt128 wide = (UInt128.One << 127) + 7, wrong = 0;
        Require(!AtomicOps.CompareExchange(ref wide, ref wrong, UInt128.Zero, true, 5, 5) && wrong == wide, "wide failed CAS");
        Require(AtomicOps.CompareExchange(ref wide, ref wrong, UInt128.MaxValue, false, 5, 5), "wide successful CAS");
        Require(AtomicOps.AddFetch(ref wide, UInt128.One, 5) == 0, "128-bit wrap");
        uint bits = 0xf0;
        Require(AtomicOps.FetchAnd(ref bits, 0x3fu, 5) == 0xf0 && bits == 0x30, "and");
        Require(AtomicOps.OrFetch(ref bits, 0x0fu, 5) == 0x3f, "or");
        Require(AtomicOps.XorFetch(ref bits, 0xffu, 5) == 0xc0, "xor");
        Require(AtomicOps.NandFetch(ref bits, 0xf0u, 5) == ~0xc0u, "nand");
        byte flag = 0;
        Require(!AtomicOps.TestAndSet(ref flag, 2) && AtomicOps.TestAndSet(ref flag, 5), "atomic flag");
        AtomicOps.Clear(ref flag, 3);
        Require(flag == 0, "atomic flag clear");
        for (int order = 0; order != 6; ++order) { AtomicOps.ThreadFence(order); AtomicOps.SignalFence(order); }
        bool rejected = false;
        try { AtomicOps.Load(ref n, 3); } catch (ArgumentOutOfRangeException) { rejected = true; }
        Require(rejected && n == 18, "invalid load order before access");
        rejected = false;
        try { AtomicOps.CompareExchange(ref n, ref expected, 0, false, 0, 5); }
        catch (ArgumentOutOfRangeException) { rejected = true; }
        Require(rejected && n == 18, "invalid CAS failure order before access");
    }

    private static void MovableManagedFallback()
    {
        // Each helper pins its byref throughout lock acquisition and access.
        // The array itself intentionally lives on the ordinary movable heap.
        byte[] storage = new byte[64];
        Parallel.For(0, 4, worker => {
            for (int i = 0; i != 2000; ++i)
            {
                AtomicOps.FetchAdd(ref Unsafe.As<byte, UInt128>(ref storage[1]), UInt128.One, 5);
                if (worker == 0 && i % 100 == 0) GC.Collect(2, GCCollectionMode.Forced, true, true);
            }
        });
        Require(AtomicOps.Load(ref Unsafe.As<byte, UInt128>(ref storage[1]), 5) == 8000, "moving GC fallback");
    }

    private static void RawObjects(int size)
    {
        byte* storage = (byte*)NativeMemory.Alloc((nuint)(size * 4 + 2));
        try
        {
            byte* value = storage + 1, expected = value + size, desired = expected + size, result = desired + size;
            storage[0] = storage[size * 4 + 1] = 0x5a;
            new Span<byte>(desired, size).Fill(0x37);
            AtomicOps.StoreBytes((ulong)size, value, desired, 3);
            AtomicOps.LoadBytes((ulong)size, value, expected, 2);
            Require(new Span<byte>(expected, size).SequenceEqual(new Span<byte>(desired, size)), "raw load/store");
            new Span<byte>(desired, size).Fill(0x91);
            Require(AtomicOps.CompareExchangeBytes((ulong)size, value, expected, desired, true, 4, 2), "raw successful CAS");
            Require(!AtomicOps.CompareExchangeBytes((ulong)size, value, expected, desired, false, 5, 5), "raw failed CAS");
            Require(new Span<byte>(expected, size).SequenceEqual(new Span<byte>(desired, size)), "raw CAS expected update");
            new Span<byte>(desired, size).Fill(0x12);
            AtomicOps.ExchangeBytes((ulong)size, value, desired, result, 5);
            Require(new Span<byte>(result, size).SequenceEqual(new Span<byte>(expected, size)), "raw exchange old value");
            Require(new Span<byte>(value, size).SequenceEqual(new Span<byte>(desired, size)), "raw exchange new value");
            Require(storage[0] == 0x5a && storage[size * 4 + 1] == 0x5a, "raw neighbor guard");
            // Readers must see a whole representation while multiple writers
            // publish alternating patterns through generic-size operations.
            nuint captured = (nuint)value;
            Parallel.For(0, 4, worker => {
                byte* local = (byte*)NativeMemory.Alloc((nuint)size);
                try
                {
                    for (int i = 0; i != 1000; ++i)
                    {
                        if (worker < 2)
                        {
                            new Span<byte>(local, size).Fill((byte)(worker + 1));
                            AtomicOps.StoreBytes((ulong)size, (void*)captured, local, 3);
                        }
                        else
                        {
                            AtomicOps.LoadBytes((ulong)size, (void*)captured, local, 2);
                            byte first = local[0];
                            for (int j = 1; j != size; ++j) Require(local[j] == first, "torn raw representation");
                        }
                    }
                }
                finally { NativeMemory.Free(local); }
            });
        }
        finally { NativeMemory.Free(storage); }
    }

    private static void MixedRawTyped()
    {
        ulong* storage = (ulong*)NativeMemory.Alloc(8);
        try
        {
            *storage = 0;
            nuint captured = (nuint)storage;
            Parallel.For(0, 4, worker => {
                for (int i = 0; i != 3000; ++i)
                {
                    ulong* address = (ulong*)captured;
                    if (worker < 2) AtomicOps.FetchAdd(ref *address, 1UL, 5);
                    else
                    {
                        ulong expected;
                        AtomicOps.LoadBytes(8, address, &expected, 5);
                        for (;;)
                        {
                            ulong desired = expected + 1;
                            if (AtomicOps.CompareExchangeBytes(8, address, &expected, &desired, false, 5, 5)) break;
                        }
                    }
                }
            });
            Require(*storage == 12000, "mixed raw and typed synchronization");
        }
        finally { NativeMemory.Free(storage); }
    }

    public static int Main()
    {
        CasAndBits();
        Counter<byte>(0); Counter<ushort>(0); Counter<uint>(0); Counter<ulong>(0); Counter<UInt128>(0);
        Counter<ushort>(1); Counter<uint>(1); Counter<ulong>(1); Counter<UInt128>(1);
        MovableManagedFallback();
        foreach (int size in new[] { 1, 2, 3, 4, 8, 16, 600 }) RawObjects(size);
        MixedRawTyped();
        Console.WriteLine("atomic runtime: all synthetic checks passed");
        return 0;
    }
}
