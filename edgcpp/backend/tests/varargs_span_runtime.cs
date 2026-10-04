using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Edg.Runtime;

unsafe class Program
{
    [StructLayout(LayoutKind.Sequential, Size = 40000)]
    private struct Large { public int Head; public fixed byte Tail[39996]; }
    [StructLayout(LayoutKind.Sequential)]
    private struct Bits { public ulong Low, High; }
    private static int checks, entered;
    private static void Check(bool value, string message)
    { if (!value) throw new Exception(message); ++checks; }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int Scalars(int marker, params ReadOnlySpan<VaArg> arguments)
    {
        int capacity = VaPack.GetStackBytes(arguments);
        byte* workspace = stackalloc byte[capacity];
        using var owner = new VaPack(arguments, workspace, capacity);
        VaList list = owner.Value;
        int first = *(int*)VaList.Take(&list, 4, 4);
        double second = *(double*)VaList.Take(&list, 8, 8);
        byte* third = (byte*)VaList.Take(&list, 16, 16);
        int* pointer = *(int**)VaList.Take(&list, 8, 8);
        return marker + first + (int)second + (int)(*(Int128*)third - ((Int128)1 << 100)) + *pointer +
            (owner.UsesNativeMemory || ((nuint)third & 15) != 0 ? 1000 : 0);
    }

    private static void ScalarAllocationCheck()
    {
        int pointed = 5;
        for (int i = 0; i < 200; ++i)
            if (Scalars(1, 7, 2.5, VaArg.From(((Int128)1 << 100) + 3, 16), (void*)&pointed) != 18)
                throw new Exception("scalar values/alignment");
        long before = GC.GetAllocatedBytesForCurrentThread();
        int sum = 0;
        for (int i = 0; i < 10000; ++i)
            sum += Scalars(1, 7, 2.5, VaArg.From(((Int128)1 << 100) + 3, 16), (void*)&pointed);
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Check(sum == 180000 && allocated == 0, "small params span calls must not allocate/box");
    }

    private static void SnapshotsAndNativeLifetime()
    {
        Large original = default;
        original.Head = 17;
        original.Tail[39995] = 239;
        VaArg snapshot = VaArg.From(original, 40000, 64);
        original.Head = 99;
        original.Tail[39995] = 0;
        GC.Collect(2, GCCollectionMode.Forced, true, true);
        ReadOnlySpan<VaArg> arguments = [snapshot, snapshot, 21];
        Check(VaPack.GetStackBytes(arguments) == 0, "large pack selects native storage");
        var owner = new VaPack(arguments, null, 0);
        try
        {
            Check(owner.UsesNativeMemory, "native ownership");
            VaList first = owner.Value, copy;
            VaList.Copy(&copy, &first);
            Large* a = (Large*)VaList.Take(&first, 40000, 64);
            Large* b = (Large*)VaList.Take(&first, 40000, 64);
            Check(((nuint)a & 63) == 0 && ((nuint)b & 63) == 0 && a != b, "aggregate alignment/distinct value storage");
            Check(a->Head == 17 && a->Tail[39995] == 239 && b->Head == 17, "owned argument snapshot survives source mutation and GC");
            Check(*(int*)VaList.Take(&first, 4, 4) == 21, "post-aggregate argument");
            Check(((Large*)VaList.Take(&copy, 40000, 64))->Head == 17, "va_copy cursor independence");
            VaList.EndList(&copy);
            Check(copy.Begin == null && copy.Next == null && first.Begin != null, "va_end clears only its cursor");
            owner.Dispose();
            owner.Dispose();
            Check(!owner.UsesNativeMemory && owner.Value.Begin == null, "same-variable disposal is idempotent");
        }
        finally { owner.Dispose(); }
        var tooMany = new VaArg[2000];
        Array.Fill(tooMany, snapshot);
        try { VaPack.GetStackBytes(tooMany); throw new Exception("missing pack size bound"); }
        catch (ArgumentOutOfRangeException) { ++checks; }
    }

    private static void ExactBitsAndAlignment()
    {
        Bits bits = new() { Low = 0xfff123456789abcdUL, High = 0x8000000000000021UL };
        double nan = BitConverter.Int64BitsToDouble(unchecked((long)0xfff8000000000123UL));
        ReadOnlySpan<VaArg> arguments = [VaArg.From(bits, 16), nan, VaArg.From(31, 4096)];
        int capacity = VaPack.GetStackBytes(arguments);
        byte* raw = stackalloc byte[capacity];
        using var owner = new VaPack(arguments, raw, capacity);
        VaList list = owner.Value;
        Bits result = *(Bits*)VaList.Take(&list, 16, 16);
        Check(result.Low == bits.Low && result.High == bits.High, "all128 value bits preserved");
        Check(*(ulong*)VaList.Take(&list, 8, 8) == 0xfff8000000000123UL, "NaN sign/payload exact");
        byte* aligned = (byte*)VaList.Take(&list, 4, 4096);
        Check(((nuint)aligned & 4095) == 0 && *(int*)aligned == 31, "4096-byte explicit alignment");
        try { VaArg.From(1, 8, 4); throw new Exception("missing type-size guard"); }
        catch (ArgumentException) { ++checks; }
        try { VaArg.From(1, 3); throw new Exception("missing alignment guard"); }
        catch (ArgumentOutOfRangeException) { ++checks; }
        try { VaPack.GetStackBytes([default]); throw new Exception("missing descriptor guard"); }
        catch (ArgumentOutOfRangeException) { ++checks; }
    }

    private static int Target(int value) => value + 1;
    private static void ImplicitConversions()
    {
        delegate* managed<int, int> callback = &Target;
        ReadOnlySpan<VaArg> arguments = [uint.MaxValue, long.MinValue, ulong.MaxValue, (void*)callback];
        int capacity = VaPack.GetStackBytes(arguments);
        byte* workspace = stackalloc byte[capacity];
        using var owner = new VaPack(arguments, workspace, capacity);
        VaList list = owner.Value;
        Check(*(uint*)VaList.Take(&list, 4, 4) == uint.MaxValue, "implicit uint boundary");
        Check(*(long*)VaList.Take(&list, 8, 8) == long.MinValue, "implicit long boundary");
        Check(*(ulong*)VaList.Take(&list, 8, 8) == ulong.MaxValue, "implicit ulong boundary");
        var received = *(delegate* managed<int, int>*)VaList.Take(&list, 8, 8);
        Check(received(17) == 18, "implicit pointer carrier preserves managed function pointer");
    }

    private static VaArg ThrowArgument() => throw new InvalidOperationException("later argument");
    private static void NeverEntered(params ReadOnlySpan<VaArg> arguments) { ++entered; }
    private static void ThrowInBody(params ReadOnlySpan<VaArg> arguments)
    {
        using var owner = new VaPack(arguments, null, 0);
        Check(owner.UsesNativeMemory, "exception body owns native pack");
        throw new InvalidOperationException("callee body");
    }
    private static void ExceptionsAndEmpty()
    {
        Large original = default;
        try { NeverEntered(VaArg.From(original, 64), ThrowArgument()); }
        catch (InvalidOperationException) { Check(entered == 0, "later argument throws before callee entry"); }
        try { ThrowInBody(VaArg.From(original, 64)); }
        catch (InvalidOperationException) { ++checks; }
        int capacity = VaPack.GetStackBytes([]);
        byte* raw = stackalloc byte[capacity];
        using var empty = new VaPack([], raw, capacity);
        VaList value = empty.Value;
        Check(value.Begin != null && value.Begin == value.End, "empty pack has valid zero-length cursor");
        try { VaList.Take(&value, 4, 4); throw new Exception("missing va_arg bounds"); }
        catch (InvalidOperationException) { ++checks; }
    }
    private static int Main()
    {
        ScalarAllocationCheck();
        SnapshotsAndNativeLifetime();
        ExactBitsAndAlignment();
        ImplicitConversions();
        ExceptionsAndEmpty();
        Console.WriteLine($"{checks} shared span varargs runtime checks passed; scalar allocation bytes=0");
        return 0;
    }
}
