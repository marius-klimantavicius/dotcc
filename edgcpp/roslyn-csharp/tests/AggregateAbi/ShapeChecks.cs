using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Application = Translated.Program;
#if AGGREGATE_SSE
using Record = Translated.SsePair;
#elif AGGREGATE_MIXED
using Record = Translated.MixedPair;
#elif AGGREGATE_MEMORY
using Record = Translated.MemoryTriple;
#elif AGGREGATE_PACKED
using Record = Translated.PackedPair;
#elif AGGREGATE_UNION
using Record = Translated.UnionWord;
#else
#error Select an individually exercised aggregate shape.
#endif

internal static unsafe class AggregateShapeChecks
{
#if AGGREGATE_SSE
    private const string Prefix = "sse";
    private const int ExpectedResult = 94, ExpectedSize = 16, ExpectedAlignment = 8, SecondOffset = 8;
    private static readonly int[] Expected = [-11, 24, -11, 24, -13, 30, -11, 24, -13, 30, -11, 24, 1, 1, 1, 1, 1, 1, 1, 1];
#elif AGGREGATE_MIXED
    private const string Prefix = "mixed";
    private const int ExpectedResult = 76, ExpectedSize = 16, ExpectedAlignment = 8, SecondOffset = 8;
    private static readonly int[] Expected = [-16, 32, -16, 32, -28, 30, -16, 32, -28, 30, -16, 32, 1, 1, 1, 1, 1, 1, 1, 1];
#elif AGGREGATE_MEMORY
    private const string Prefix = "memory";
    private const int ExpectedResult = 161, ExpectedSize = 24, ExpectedAlignment = 8, SecondOffset = 8;
    private static readonly int[] Expected = [-4, 9, 22, -4, 9, 22, -7, 11, 17, -4, 9, 22, -7, 11, 17, -4, 9, 22, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1];
#elif AGGREGATE_PACKED
    private const string Prefix = "packed";
    private const int ExpectedResult = 364, ExpectedSize = 9, ExpectedAlignment = 1, SecondOffset = 1;
    private static readonly int[] Expected = [32, 32, 32, 32, 20, 30, 32, 32, 20, 30, 32, 32, 1, 1, 1, 1, 1, 1, 1, 1];
#elif AGGREGATE_UNION
    private const string Prefix = "unionword";
    private const int ExpectedResult = 11, ExpectedSize = 8, ExpectedAlignment = 8, SecondOffset = 0;
    private static readonly int[] Expected = [1, 1, 0, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1];
#endif
    [StructLayout(LayoutKind.Sequential)]
    private struct AlignmentProbe { public byte Lead; public Record Value; }
#if AGGREGATE_UNION
    [DllImport("aggregate-abi-adapter", EntryPoint = "native_unionword_bits")]
    private static extern ulong Bits(Record* value);
#else
    [DllImport("aggregate-abi-adapter", EntryPoint = "native_" + Prefix + "_field")]
    private static extern double Field(Record* value, int index);
#endif
    [DllImport("aggregate-abi-adapter", EntryPoint = "native_" + Prefix + "_echo_callback")]
    private static extern nint Echo(nint callback);

    private static int Main(string[] args)
    {
        if (args.Length != 1) throw new ArgumentException("Expected exact ABI-only native provider path.");
        var provider = NativeLibrary.Load(Path.GetFullPath(args[0]));
        NativeLibrary.SetDllImportResolver(typeof(AggregateShapeChecks).Assembly, (name, _, _) => name == "aggregate-abi-adapter" ? provider : 0);
        for (var iteration = 0; iteration < 2; ++iteration)
        {
            Require(Application.fixture() == ExpectedResult && Application.aggregate_observation_count() == Expected.Length, "application result/count");
            for (var index = 0; index < Expected.Length; ++index)
                Require(Application.aggregate_observe(index) == Expected[index], $"application observation {index}");
            var callback = *Application.callback;
            Require(callback != 0 && Echo(callback) == callback, "canonical native entry identity");
        }
        Require(Unsafe.SizeOf<Record>() == ExpectedSize, "physical size");
#if AGGREGATE_UNION
        Require((int)Marshal.OffsetOf<Record>(nameof(Record.integer)) == 0 && (int)Marshal.OffsetOf<Record>(nameof(Record.floating)) == SecondOffset, "physical overlapping offsets");
#else
        Require((int)Marshal.OffsetOf<Record>(nameof(Record.first)) == 0 && (int)Marshal.OffsetOf<Record>(nameof(Record.second)) == SecondOffset, "physical offsets");
#endif
#if AGGREGATE_MEMORY
        Require((int)Marshal.OffsetOf<Record>(nameof(Record.third)) == 16, "physical third offset");
#endif
        Require((int)Marshal.OffsetOf<AlignmentProbe>(nameof(AlignmentProbe.Value)) == ExpectedAlignment, "physical alignment");
        AlignmentProbe storage = new() { Lead = 51, Value = default };
#if AGGREGATE_SSE
        storage.Value.first = -123.25; storage.Value.second = 4.125;
        Require(Field(&storage.Value, 0) == -123.25 && Field(&storage.Value, 1) == 4.125, "native physical field reads");
#elif AGGREGATE_MIXED
        storage.Value.first = int.MinValue + 7; storage.Value.second = 4.125;
        Require(Field(&storage.Value, 0) == int.MinValue + 7 && Field(&storage.Value, 1) == 4.125, "native physical field reads");
#elif AGGREGATE_MEMORY
        storage.Value.first = long.MinValue + 7; storage.Value.second = long.MaxValue - 9; storage.Value.third = -703;
        Require(Field(&storage.Value, 0) == (double)(long.MinValue + 7) && Field(&storage.Value, 1) == (double)(long.MaxValue - 9) && Field(&storage.Value, 2) == -703, "native physical field reads");
#elif AGGREGATE_PACKED
        storage.Value.first = 201; storage.Value.second = -123.25;
        Require(Field(&storage.Value, 0) == 201 && Field(&storage.Value, 1) == -123.25, "native unaligned physical field reads");
#elif AGGREGATE_UNION
        storage.Value.integer = 0x3ff0000000000001UL;
        Require(Bits(&storage.Value) == 0x3ff0000000000001UL, "native full-width physical union read");
#endif
        Require((nuint)(&storage.Value) % ExpectedAlignment == 0 && (byte*)(&storage.Value) - (byte*)(&storage) == ExpectedAlignment && storage.Lead == 51, "actual storage address");
        Console.WriteLine($"result={ExpectedResult}");
        Console.WriteLine("observations=" + string.Join(",", Expected));
        Console.WriteLine("canonical-identity=passed");
        Console.WriteLine("physical-layout=passed");
        return 0;
    }
    private static void Require(bool condition, string check)
    {
        if (!condition) throw new InvalidOperationException("Aggregate ABI " + Prefix + " check failed: " + check);
    }
}
