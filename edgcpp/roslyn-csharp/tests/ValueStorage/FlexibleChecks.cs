using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RoslynObj.CSharp.Runtime;
using Application = Translated.Program;

internal static unsafe class FlexibleChecks
{
    private static void Require(bool condition, string description)
    {
        if (!condition) throw new InvalidOperationException(description);
    }
    private static int Main()
    {
        long[] expected = [3, 17, 1, 8, 11, 23, 34, 1, 12, 90, 8, 16];
        Require(Application.fixture() == expected.Length, "Selected flexible-tail body result changed.");
        for (var index = 0; index < expected.Length; ++index)
            Require(Application.flexible_observe(index) == expected[index], "Flexible-tail observation " + index);
        Require(Unsafe.SizeOf<Translated.Tail>() == 8 && Unsafe.SizeOf<Translated.PaddedTail>() == 16,
            "Actual CLR headers must not gain a fabricated flexible element.");
        Require(Marshal.OffsetOf<Translated.Tail>("tag") == 4 && Marshal.OffsetOf<Translated.PaddedTail>("count") == 8,
            "Actual fixed header field offsets changed.");
        var storage = new PinnedStorage(64, 8);
        var value = (Translated.Tail*)storage.Pointer;
        Application.write_tail(value);
        var tail = Application.tail_data(value);
        Require((byte*)tail - (byte*)value == 8 && Translated.Tail.values(value) == tail
            && tail[0] == 11 && tail[1] == 23 && tail[2] == 34, "Exact physical tail offset/stride/storage bytes changed.");
        var padded = (Translated.PaddedTail*)storage.Pointer;
        var paddedTail = Translated.PaddedTail.values(padded);
        Require((byte*)paddedTail - (byte*)padded == 12 && Application.padded_data(padded) == paddedTail,
            "Tail within captured header padding must use field offset twelve.");
        paddedTail[0] = 101; paddedTail[1] = 103;
        Require(*(int*)((byte*)padded + 12) == 101 && *(int*)((byte*)padded + 16) == 103,
            "Captured element stride must address consecutive actual bytes.");
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        Require(Application.padded_data(padded) == paddedTail && paddedTail[1] == 103, "Pinned tail address changed across GC.");
        GC.KeepAlive(storage);
        Console.WriteLine("result=12");
        for (var index = 0; index < expected.Length; ++index) Console.WriteLine($"{index}:{expected[index]}");
        Console.WriteLine("physical-headers=8,16\nphysical-tail-offsets=8,12\nphysical-stride=4\npinned-tail=passed");
        return 0;
    }
}
