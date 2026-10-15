using System.Globalization;
using System.Runtime.InteropServices;
using Application = Translated.Program;

// Inspect values recorded by the narrow native ABI observer after the ordinary
// managed application invokes both a native variadic declaration and an unknown
// native variadic pointer. No source application body enters the provider.
internal static class OutboundVariadicChecks
{
    [DllImport("outbound-adapter", EntryPoint = "native_captured_integer")] private static extern int Integer(int index);
    [DllImport("outbound-adapter", EntryPoint = "native_captured_double")] private static extern double Floating(int index);

    public static int Main()
    {
        var result = Application.fixture();
        if (result != 627) throw new InvalidOperationException("Unknown/direct native variadic result changed.");
        Console.WriteLine($"result={result}");
        for (var index = 0; index < 10; ++index)
        {
            var integer = Integer(index);
            var floating = Floating(index);
            if (integer != index + 11 || BitConverter.DoubleToInt64Bits(floating) != BitConverter.DoubleToInt64Bits(index + 10.5))
                throw new InvalidOperationException("Native variadic observer received different argument order or bits.");
            Console.WriteLine($"{index}:{integer}:{floating.ToString("0.0", CultureInfo.InvariantCulture)}");
        }
        return 0;
    }
}
