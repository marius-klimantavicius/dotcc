using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using static Libc;

internal static unsafe class Program
{
    private const int Iterations = 20_000;
    private static readonly VaArg[] Arguments = [1, 2, 3, 4, 5, 6, 7, 8];
    private static int sink;

    // Keep the array baseline across a real call boundary. Otherwise escape
    // analysis could erase the allocation being compared to params spans.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int ArraySum(int count, params VaArg[] arguments)
    {
        int sum = 0;
        for (int i = 0; i < count; i++) sum += (int)arguments[i];
        return sum;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int Empty() => DotCcLib.span_sum(0);
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int One() => DotCcLib.span_sum(1, 7);
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int Eight() => DotCcLib.span_sum(8, 1, 2, 3, 4, 5, 6, 7, 8);
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int Many() => DotCcLib.span_sum(64,
        1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16,
        17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32,
        33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48,
        49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64);
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int ArrayEmpty() => ArraySum(0);
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int ArrayOne() => ArraySum(1, 7);
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int ArrayEight() => ArraySum(8, 1, 2, 3, 4, 5, 6, 7, 8);
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int ArrayMany() => ArraySum(64,
        1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16,
        17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32,
        33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48,
        49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64);
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int ExplicitArray() => DotCcLib.span_sum(8, Arguments);
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int ExplicitSpan() => DotCcLib.span_sum(8, Arguments.AsSpan());
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int ExplicitReadOnlySpan() => DotCcLib.span_sum(8, (ReadOnlySpan<VaArg>)Arguments);
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int StackSpan()
    {
        ReadOnlySpan<VaArg> arguments = stackalloc VaArg[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        return DotCcLib.span_sum(8, arguments);
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int Copied() => DotCcLib.span_copy(8, 1, 2, 3, 4, 5, 6, 7, 8);
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int Callback()
    {
        delegate*<int, ReadOnlySpan<VaArg>, int> callback = &DotCcLib.span_sum;
        return callback(8, Arguments.AsSpan());
    }

    private static (long Bytes, long Ticks) Measure(Func<int> operation, int expected)
    {
        // Warm static initializers, callbacks and runtime helpers before sampling.
        for (int i = 0; i < 4096; i++)
            if (operation() != expected) throw new InvalidOperationException("Wrong warmup result");
        long startTicks = Stopwatch.GetTimestamp();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int total = 0;
        for (int i = 0; i < Iterations; i++) total = unchecked(total + operation());
        long bytes = GC.GetAllocatedBytesForCurrentThread() - before;
        long ticks = Stopwatch.GetTimestamp() - startTicks;
        Volatile.Write(ref sink, total);
        if (total != unchecked(expected * Iterations)) throw new InvalidOperationException("Wrong measured result");
        return (bytes, ticks);
    }

    private static void Check(string name, Func<int> operation, int expected,
        Func<int>? arrayBaseline = null, bool baselineAllocates = false)
    {
        var actual = Measure(operation, expected);
        if (actual.Bytes != 0) throw new InvalidOperationException($"{name}: allocated {actual.Bytes} bytes");
        string baselineText = "";
        if (arrayBaseline is not null)
        {
            var baseline = Measure(arrayBaseline, expected);
            if (baselineAllocates && baseline.Bytes == 0)
                throw new InvalidOperationException($"{name}: array baseline allocation was eliminated");
            baselineText = $" array_bytes={baseline.Bytes} array_ms={baseline.Ticks * 1000.0 / Stopwatch.Frequency:F3}";
        }
        Console.WriteLine($"PASS {name} result={expected} calls={Iterations} span_bytes={actual.Bytes} span_ms={actual.Ticks * 1000.0 / Stopwatch.Frequency:F3}{baselineText}");
    }

    private static int Main()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        try
        {
            Check("expanded-0", Empty, 0, ArrayEmpty);
            Check("expanded-1", One, 7, ArrayOne, true);
            Check("expanded-8", Eight, 36, ArrayEight, true);
            Check("expanded-64", Many, 2080, ArrayMany, true);
            Check("translated-c-0", () => DotCcLib.span_c_empty(), 0, ArrayEmpty);
            Check("translated-c-1", () => DotCcLib.span_c_one(), 7, ArrayOne, true);
            Check("translated-c-8", () => DotCcLib.span_c_eight(), 36, ArrayEight, true);
            Check("translated-c-64", () => DotCcLib.span_c_many(), 2080, ArrayMany, true);
            Check("explicit-array", ExplicitArray, 36);
            Check("explicit-span", ExplicitSpan, 36);
            Check("explicit-readonly-span", ExplicitReadOnlySpan, 36);
            Check("stack-span", StackSpan, 36);
            Check("forward-and-copy", Copied, 72);
            Check("variadic-callback", Callback, 36);
            Check("c-promotions-and-pointer-callback", () => DotCcLib.span_c_promotions(), 65883);
            Console.WriteLine("PASS translated span varargs: 15 cases, zero warmed allocations; array baseline and elapsed times reported");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
