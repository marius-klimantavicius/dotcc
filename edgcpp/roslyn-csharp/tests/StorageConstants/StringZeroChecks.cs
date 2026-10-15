using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Application = Translated.Program;

internal static unsafe class StaticStringZeroChecks
{
    private static void Require(bool condition, string check)
    {
        if (!condition) throw new InvalidOperationException("Generated static string/zero contract failed: " + check);
    }
    private static int Main()
    {
        var state = Application.static_zero();
        var text = Application.static_text();
        Require(Unsafe.SizeOf<Translated.StaticState>() == 24 && Marshal.OffsetOf<Translated.StaticState>("value") == 8
            && Marshal.OffsetOf<Translated.StaticState>("count") == 16 && (nuint)state % 8 == 0, "actual record representation");
        for (var index = 0; index < 24; ++index) Require(((byte*)state)[index] == 0, "physical static zero byte " + index);
        Require(text[0] == 65 && text[1] == 0 && text[2] == 66 && text[3] == 67 && text[4] == 0, "exact constant bytes");
        Require(Application.fixture() == 208 && state->value == 7 && state->count == 1 && text[2] == 67, "first invocation");
        Require(Application.fixture() == 217 && state->value == 14 && state->count == 2 && text[2] == 68, "second invocation without reinitialization");
        GC.KeepAlive(new byte[128 * 1024]);
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        Require(Application.static_zero() == state && Application.static_text() == text, "static address stability after GC");
        Console.WriteLine("results=208,217");
        Console.WriteLine("static-string-zero-physical=passed");
        return 0;
    }
}
