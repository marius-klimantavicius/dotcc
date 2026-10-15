using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.InteropServices;

namespace RoslynObj.CSharp.Runtime;

/// <summary>Selected compiler operations; these never look up native symbols.</summary>
public static unsafe class TargetIntrinsics
{
    public static ushort ByteSwap16(ushort value) => BinaryPrimitives.ReverseEndianness(value);
    public static uint ByteSwap32(uint value) => BinaryPrimitives.ReverseEndianness(value);
    public static ulong ByteSwap64(ulong value) => BinaryPrimitives.ReverseEndianness(value);

    public static int CountLeadingZeros64(ulong value)
    {
        if (value == 0) throw new InvalidOperationException("The selected clzll operation requires a nonzero operand.");
        return BitOperations.LeadingZeroCount(value);
    }

    // Admitted GNU orders use a stronger SC implementation. Loads perform no
    // write or RMW, so a load cannot accidentally require writable storage.
    // The target contract is naturally aligned Linux x64 primitive storage.
    public static byte AtomicLoad1(void* address, int order)
    {
        RequireLoad(address, 1, order); Thread.MemoryBarrier();
        var value = Volatile.Read(ref *(byte*)address); Thread.MemoryBarrier(); return value;
    }
    public static ushort AtomicLoad2(void* address, int order)
    {
        RequireLoad(address, 2, order); Thread.MemoryBarrier();
        var value = Volatile.Read(ref *(ushort*)address); Thread.MemoryBarrier(); return value;
    }
    public static uint AtomicLoad4(void* address, int order)
    {
        RequireLoad(address, 4, order); Thread.MemoryBarrier();
        var value = Volatile.Read(ref *(uint*)address); Thread.MemoryBarrier(); return value;
    }
    public static void AtomicStore1(void* address, byte value, int order)
    { RequireStore(address, 1, order); Interlocked.Exchange(ref *(byte*)address, value); }
    public static void AtomicStore2(void* address, ushort value, int order)
    { RequireStore(address, 2, order); Interlocked.Exchange(ref *(ushort*)address, value); }
    public static void AtomicStore4(void* address, uint value, int order)
    { RequireStore(address, 4, order); Interlocked.Exchange(ref *(uint*)address, value); }
    public static void AtomicStore8(void* address, ulong value, int order)
    { RequireStore(address, 8, order); Interlocked.Exchange(ref *(ulong*)address, value); }

    public static void Synchronize()
    { RequireTarget(); Thread.MemoryBarrier(); }

    // Actual SQLite calls select signed 64-bit operands and result storage.
    // Int128 represents every exact sum, difference, and product of two Int64s.
    public static bool AddOverflow64(long left, long right, long* result)
        => StoreOverflow64((Int128)left + right, result);
    public static bool SubtractOverflow64(long left, long right, long* result)
        => StoreOverflow64((Int128)left - right, result);
    public static bool MultiplyOverflow64(long left, long right, long* result)
        => StoreOverflow64((Int128)left * right, result);
    private static bool StoreOverflow64(Int128 mathematical, long* result)
    {
        RequireAddress(result, 8);
        var stored = unchecked((long)mathematical);
        *result = stored;
        return mathematical != stored;
    }

    private static void RequireLoad(void* address, uint width, int order)
    {
        if (order != 0) throw new InvalidOperationException("Only the exercised captured relaxed atomic load order is admitted.");
        RequireAddress(address, width);
    }
    private static void RequireStore(void* address, uint width, int order)
    {
        if (order != 0) throw new InvalidOperationException("Only the exercised captured relaxed atomic store order is admitted.");
        RequireAddress(address, width);
    }
    private static void RequireAddress(void* address, uint width)
    {
        RequireTarget();
        if (address == null || (nuint)address % width != 0)
            throw new InvalidOperationException("The selected intrinsic storage must have a nonzero naturally aligned address.");
    }
    private static void RequireTarget()
    {
        if (!OperatingSystem.IsLinux() || RuntimeInformation.ProcessArchitecture != Architecture.X64)
            throw new PlatformNotSupportedException("This selected atomic contract requires Linux x64.");
    }
}
