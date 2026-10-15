using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Application = Translated.Program;

internal static unsafe class StaticConstantChecks
{
    private static void Require(bool condition, string check)
    {
        if (!condition) throw new InvalidOperationException("Generated static/constant contract failed: " + check);
    }
    private static int Main()
    {
        int[][] expected = [
            [9, 12, 3, 5, 1, 0, 1, 2, 1, 23, 155, 36, 68, 31, 37],
            [11, 13, 3, 5, 3, 0, 1, 4, 1, 23, 155, 36, 68, 31, 37]];
        int[] results = [306, 313];
        for (var iteration = 0; iteration < 2; ++iteration)
        {
            Require(Application.fixture() == results[iteration], "stateful result " + iteration);
            Require(Application.storage_observation_count() == expected[iteration].Length, "observation count");
            for (var index = 0; index < expected[iteration].Length; ++index)
                Require(Application.storage_observe(index) == expected[iteration][index], $"observation {iteration}/{index}");
        }
        var values = Application.static_array();
        var node = Application.static_node();
        var text = Application.literal_bytes();
        Require((nuint)values % 4 == 0 && (nuint)node % 8 == 0 && node == node->next && node->value == 23, "actual static address alignment/fixup");
        Require(Unsafe.SizeOf<Translated.Node>() == 16 && Marshal.OffsetOf<Translated.Node>("value") == 8, "actual carrier size/offset");
        Require(values[2] == 5 && values[3] == 0, "static omitted elements and persisted update");
        Require(text[0] == 65 && text[1] == 0 && text[2] == 90 && text[3] == 0, "exact embedded-NUL and terminal bytes");
        var pair = Application.pair_constant();
        pair.first = -1;
        Require(Application.pair_constant().first == 17 && pair.first == -1, "aggregate constant is read as an independent value");
        GC.KeepAlive(new byte[128 * 1024]);
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        Require(Application.static_array() == values && values[2] == 6 && Application.static_node() == node && Application.literal_bytes() == text,
            "static and literal storage address stability across GC");
        Console.WriteLine("results=306,313");
        Console.WriteLine("observations-0=" + string.Join(",", expected[0]));
        Console.WriteLine("observations-1=" + string.Join(",", expected[1]));
        Console.WriteLine("static-constant-storage=passed");
        return 0;
    }
}
