using System.Runtime.CompilerServices;
using System.Text;

internal static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length != 0)
        {
            Console.Error.WriteLine("Usage: Zydis.ManagedBoundary (no arguments)");
            return 2;
        }

        (string Name, Action Run)[] cases =
        [
            ("formatter-intel", FormatterBoundaryTests.Intel),
            ("formatter-att", FormatterBoundaryTests.Att),
            ("formatter-status-failure", FormatterBoundaryTests.StatusFailure),
            ("formatter-contained-exception", FormatterBoundaryTests.ContainedException),
            ("owning-snapshots-and-slices", OwningBoundaryTests.Run)
        ];
        try
        {
            foreach (var test in cases)
            {
                test.Run();
                Console.WriteLine($"PASS {test.Name}");
            }
            Console.WriteLine($"PASS managed-boundary cases={cases.Length}");
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error);
            return 1;
        }
    }
}

internal static class Check
{
    public const int GuardLength = 16;
    public const byte Canary = 0xA5;

    public static void Require(bool condition, string assertion)
    {
        if (!condition) throw new InvalidOperationException("Managed boundary assertion failed: " + assertion);
    }

    public static byte[] GuardedBuffer(int capacity)
    {
        var result = new byte[capacity + 2 * GuardLength];
        result.AsSpan().Fill(Canary);
        return result;
    }

    public static void Guards(byte[] storage, int capacity)
    {
        Require(storage.AsSpan(0, GuardLength).IndexOfAnyExcept(Canary) < 0,
            "write before borrowed destination slice");
        Require(storage.AsSpan(GuardLength + capacity).IndexOfAnyExcept(Canary) < 0,
            "write after borrowed destination slice");
    }

    public static string Text(ReadOnlySpan<byte> buffer)
    {
        int length = buffer.IndexOf((byte)0);
        Require(length >= 0, "formatted text is terminated within the borrowed buffer");
        return Encoding.UTF8.GetString(buffer[..length]);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void CompactGc()
    {
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
        GC.WaitForPendingFinalizers();
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
    }
}
