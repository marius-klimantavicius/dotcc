using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using static DotCC.Libc.Libc;
#if !SECURE_ZERO_DIRECT
using Xunit;
#endif

namespace DotCC.Tests;

#if !SECURE_ZERO_DIRECT
[Collection("Runtime")]
public sealed class CPythonR13SecureZeroTests
{
    [Fact] public void Exact_ranges_and_guards_on_stack_and_native_heap() => CPythonR13SecureZeroChecks.Ranges();
    [Fact] public void Pinned_owner_survives_compacting_collection() => CPythonR13SecureZeroChecks.Pinned();
    [Fact] public void Invalid_extent_is_rejected_before_any_store() => CPythonR13SecureZeroChecks.InvalidRanges();
    [Fact] public void Erasure_is_allocation_free_and_callable_with_C_void_signature() => CPythonR13SecureZeroChecks.Allocations();
    [Fact] public void Separate_worker_buffers_and_errno_are_preserved() => CPythonR13SecureZeroChecks.Workers();
}
#endif

public static unsafe class CPythonR13SecureZeroChecks
{
    static int assertions;
    static void Check(bool ok)
    {
        if (!ok) throw new Exception("secure-zero assertion failed");
        Interlocked.Increment(ref assertions);
    }
    static bool All(ReadOnlySpan<byte> bytes, byte value)
    {
        foreach (byte b in bytes) if (b != value) return false;
        return true;
    }
    static void Region(byte* p, int capacity, int start, int length)
    {
        new Span<byte>(p, capacity).Fill(0xa7);
        errno = EDOM;
        explicit_bzero(p + start, (ulong)length);
        Check(errno == EDOM);
        Check(All(new ReadOnlySpan<byte>(p, start), 0xa7));
        Check(All(new ReadOnlySpan<byte>(p + start, length), 0));
        Check(All(new ReadOnlySpan<byte>(p + start + length, capacity - start - length), 0xa7));
    }
    public static int RunAll()
    {
        assertions = 0;
        Ranges(); Pinned(); InvalidRanges(); Allocations(); Workers();
        return assertions;
    }
    public static void Ranges()
    {
        byte* stack = stackalloc byte[273];
        explicit_bzero(null, 0);
        explicit_bzero((void*)nuint.MaxValue, 0); // no dereference for an empty range
        Region(stack, 273, 0, 0);
        Region(stack, 273, 1, 1);
        Region(stack, 273, 3, 255);
        Region(stack, 273, 0, 273);
        byte* heap = (byte*)NativeMemory.Alloc(4099);
        try { Region(heap, 4099, 1, 4097); Region(heap, 4099, 4099, 0); }
        finally { NativeMemory.Free(heap); }
        Check(DeadKey(false) == 1 && DeadKey(true) == 2);
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int DeadKey(bool execute)
    {
        byte* key = stackalloc byte[129];
        if (execute)
        {
            new Span<byte>(key, 129).Fill(0xc5);
            explicit_bzero(key, 129);
        }
        return execute ? 2 : 1; // the key is deliberately never read again
    }
    public static void Pinned()
    {
        var owner = new byte[4113];
        fixed (byte* p = owner)
        {
            owner.AsSpan().Fill(0x9d);
            GC.Collect(2, GCCollectionMode.Forced, true, true);
            explicit_bzero(p + 9, 4097);
            GC.Collect(2, GCCollectionMode.Forced, true, true);
            Check(All(owner.AsSpan(0, 9), 0x9d));
            Check(All(owner.AsSpan(9, 4097), 0));
            Check(All(owner.AsSpan(4106), 0x9d));
        }
        GC.KeepAlive(owner);
    }
    public static void InvalidRanges()
    {
        byte* sentinel = stackalloc byte[1]; sentinel[0] = 0xa7;
        bool rejected = false;
        try { explicit_bzero(null, 1); } catch (ArgumentNullException) { rejected = true; }
        Check(rejected);
        rejected = false;
        try { explicit_bzero(sentinel, ulong.MaxValue); } catch (ArgumentOutOfRangeException) { rejected = true; }
        Check(rejected && sentinel[0] == 0xa7);
        rejected = false;
        try { explicit_bzero((void*)(nuint.MaxValue - 3), 8); } catch (ArgumentOutOfRangeException) { rejected = true; }
        Check(rejected && sentinel[0] == 0xa7);
    }
    public static void Allocations()
    {
        byte* data = stackalloc byte[257];
        delegate*<void*, ulong, void> erase = &explicit_bzero;
        for (int i = 0; i < 100; i++) erase(data, 257);
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 10000; i++) { data[3] = 0xff; erase(data, 257); }
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Check(allocated == 0 && All(new ReadOnlySpan<byte>(data, 257), 0));
    }
    public static void Workers()
    {
        Task first = Task.Run(Worker), second = Task.Run(Worker);
        Task.WaitAll(first, second);
    }
    static void Worker()
    {
        byte* buffer = stackalloc byte[1027];
        Region(buffer, 1027, 1, 1025);
    }
}
