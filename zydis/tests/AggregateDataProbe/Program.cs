using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Core = Managed.Zydis.Zydis;

// A separate diagnostic executable, never included in the delivered library or
// the qualification transcripts. Every timing sample runs in a fresh process.
internal static unsafe class Program
{
    private const int Count = 7809;
    private const int Width = 9;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Initialize() => RuntimeHelpers.RunClassConstructor(typeof(Core).TypeHandle);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ReadOnlySpan<byte> Table()
    {
        if (sizeof(Core.ZydisEncodableInstruction_) != Width || Core.Globals.encoder_instructions == null)
            throw new InvalidOperationException("Unexpected emitted encoder table storage");
        return new ReadOnlySpan<byte>(Core.Globals.encoder_instructions, Count * Width);
    }

    private static int Main(string[] args)
    {
        if (args.Length != 1)
            throw new ArgumentException("Expected output path for the complete encoder table");
        // Warm the clock before starting. The interval includes the wrapper call,
        // any initialization JIT, allocations, and the complete product cctor;
        // it excludes hashing, I/O, and process startup.
        _ = Stopwatch.GetTimestamp();
        long started = Stopwatch.GetTimestamp();
        Initialize();
        long elapsed = Stopwatch.GetTimestamp() - started;
        ReadOnlySpan<byte> table = Table();
        File.WriteAllBytes(args[0], table.ToArray());
        Console.WriteLine("init_seconds=" + ((double)elapsed / Stopwatch.Frequency).ToString("R", CultureInfo.InvariantCulture));
        Console.WriteLine("encoder_rows=" + Count);
        Console.WriteLine("encoder_row_bytes=" + Width);
        Console.WriteLine("encoder_bytes=" + table.Length);
        Console.WriteLine("encoder_sha256=" + Convert.ToHexString(SHA256.HashData(table)).ToLowerInvariant());
        return 0;
    }
}
