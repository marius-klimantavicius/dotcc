using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Translated;
using Application = Translated.Program;

internal static unsafe class IntegerAggregateChecks
{
    [StructLayout(LayoutKind.Sequential)]
    private struct AlignmentProbe { public byte Lead; public IntegerPair Value; }
    [DllImport("aggregate-abi-adapter", EntryPoint = "native_integer_field")]
    private static extern double Field(IntegerPair* value, int index);
    [DllImport("aggregate-abi-adapter", EntryPoint = "native_integer_echo_callback")]
    private static extern nint Echo(nint callback);

    private static int Main(string[] args)
    {
        if (args.Length != 1) throw new ArgumentException("Expected exact ABI-only native provider path.");
        var provider = NativeLibrary.Load(Path.GetFullPath(args[0]));
        NativeLibrary.SetDllImportResolver(typeof(IntegerAggregateChecks).Assembly, (name, _, _) => name == "aggregate-abi-adapter" ? provider : 0);
        int[] expected = [-4, 9, -4, 9, -7, 11, -4, 9, -7, 11, -4, 9, 1, 1, 1, 1, 1, 1, 1, 1];
        for (var iteration = 0; iteration < 2; ++iteration)
        {
            Require(Application.fixture() == 36 && Application.aggregate_observation_count() == expected.Length, "application result/count");
            for (var index = 0; index < expected.Length; ++index)
                Require(Application.aggregate_observe(index) == expected[index], $"application observation {index}");
            var callback = *Application.callback;
            Require(callback != 0 && Echo(callback) == callback, "canonical native entry identity");
        }
        Require(Unsafe.SizeOf<IntegerPair>() == 8 && (int)Marshal.OffsetOf<IntegerPair>(nameof(IntegerPair.first)) == 0 &&
            (int)Marshal.OffsetOf<IntegerPair>(nameof(IntegerPair.second)) == 4, "physical size/offsets");
        Require((int)Marshal.OffsetOf<AlignmentProbe>(nameof(AlignmentProbe.Value)) == 4, "physical alignment");
        AlignmentProbe storage = new() { Lead = 51, Value = new() { first = int.MinValue + 7, second = int.MaxValue - 9 } };
        Require((nuint)(&storage.Value) % 4 == 0 && (byte*)(&storage.Value) - (byte*)(&storage) == 4 && storage.Lead == 51,
            "actual storage address");
        Require(Field(&storage.Value, 0) == int.MinValue + 7 && Field(&storage.Value, 1) == int.MaxValue - 9, "native physical field reads");
        Console.WriteLine("result=36");
        Console.WriteLine("observations=" + string.Join(",", expected));
        Console.WriteLine("canonical-identity=passed");
        Console.WriteLine("physical-layout=passed");
        return 0;
    }
    private static void Require(bool condition, string check)
    {
        if (!condition) throw new InvalidOperationException("Integer aggregate ABI check failed: " + check);
    }
}
