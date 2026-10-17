using Application = Translated.Program;

internal static unsafe class StaticChecks
{
    public static int Main()
    {
        int[] expected = [0, 29, 53, 29, 29, 7, 1, 1, 1, 37, 41, 1, 116, 0, 108, 0];
        var before = Read();
        Require(before.SequenceEqual(expected), "zero/constants/cross-TU fixups/literal discovery");
        var addresses = Enumerable.Range(0, 7).Select(index => (nint)Application.static_address(index)).ToArray();
        Require(addresses.Distinct().Count() == addresses.Length && (nuint)addresses[3] % 64 == 0, "per-object identity and alignment");
        Require(*(int*)addresses[0] == 0 && *(int*)addresses[1] == 29 && *(int*)addresses[2] == 53 &&
            *(int*)addresses[3] == 67 && *(int*)addresses[4] == 7 && *(int*)addresses[5] == 5 && *(int*)addresses[6] == 5,
            "physical allocation bytes");
        Application.static_change(19);
        expected[0] = 19; expected[5] = 26;
        var after = Read();
        Require(after.SequenceEqual(expected) && *(int*)addresses[5] == 24 && *(int*)addresses[6] == 5, "independent equal-valued objects");
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        for (var index = 0; index < addresses.Length; ++index)
            Require(addresses[index] == (nint)Application.static_address(index), "rooted pinned owner stability");
        Console.WriteLine("static-before=" + string.Join(',', before));
        Console.WriteLine("static-after=" + string.Join(',', after));
        Console.WriteLine("static-addresses=stable-aligned-distinct");
        return 0;
    }
    private static int[] Read() => Enumerable.Range(0, 16).Select(Application.static_observe).ToArray();
    private static void Require(bool condition, string contract)
    {
        if (!condition) throw new InvalidOperationException("Static storage check failed: " + contract);
    }
}
