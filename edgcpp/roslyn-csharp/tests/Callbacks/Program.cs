using System.Runtime.InteropServices;
using Application = Translated.Program;

// Inspect the native adapter's event observations after generated application
// execution. This harness never links the native reference application body.
internal static class CallbackChecks
{
    [DllImport("callbacks-adapter", EntryPoint = "native_event_count")] private static extern int EventCount();
    [DllImport("callbacks-adapter", EntryPoint = "native_event_at")] private static extern int EventAt(int index);

    public static int Main()
    {
        var result = Application.fixture();
        int[] expected = [21, 11, 21, 31, 21, 11, 41];
        if (result != 751 || EventCount() != expected.Length)
            throw new InvalidOperationException("Callback result or event count did not match the native oracle.");
        Console.WriteLine($"result={result}");
        Console.WriteLine($"events={EventCount()}");
        for (var index = 0; index < expected.Length; ++index)
        {
            var value = EventAt(index);
            if (value != expected[index]) throw new InvalidOperationException("Callback event order changed.");
            Console.WriteLine($"{index}:{value}");
        }
        return 0;
    }
}
