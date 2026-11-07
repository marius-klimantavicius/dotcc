using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Managed.Emulation;
using Managed.Emulation.Execution;

bool nativeFp = args.Length == 3 && args[2] == "--native-fp";
bool nativeGp = args.Length == 3 && args[2] == "--native-gp";
if (args.Length != 2 && !nativeFp && !nativeGp) throw new ArgumentException("FullJitSignals fixtures output.json [--native-fp|--native-gp]");
string fixtures = Path.GetFullPath(args[0]), output = Path.GetFullPath(args[1]);
Directory.CreateDirectory(Path.GetDirectoryName(output)!);
List<SignalRow> rows = [];
using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(fixtures, "manifest.json")));
using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(4));
void Check(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
void Save() => File.WriteAllText(output, JsonSerializer.Serialize(rows, SignalsJson.Default.ListSignalRow));

foreach (ExecutionMode execution in new[] { ExecutionMode.InProcess, ExecutionMode.SeparateProcess })
foreach (string fixture in nativeGp ? new[] { "caught-gp", "caught-gp-recursive" } : nativeFp ? new[] { "caught-simd", "caught-simd-recursive" } :
    new[] { "caught-ud2", "caught-ud2-recursive", "caught-xgetbv", "uncaught-ud2", "cpu-policy" })
{
    SignalRow? baseline = null;
    // Legacy SSE helpers omit unmasked guest MXCSR delivery. Their failing
    // observation is retained separately; Full FP compares with native Linux.
    foreach (GuestJitMode mode in nativeFp || nativeGp ? new[] { GuestJitMode.Full } :
        new[] { GuestJitMode.Interpreter, GuestJitMode.Hybrid, GuestJitMode.Full })
    {
        byte[] image = File.ReadAllBytes(Path.Combine(fixtures, fixture + ".elf"));
        await using var machine = new BlinkMachine(new() {
            ExecutionMode = execution, MemoryLimit = 64L << 20, VirtualMemoryLimit = 1L << 30,
            ThreadLimit = 2, InstructionLimit = 250_000, ExecutionDeadline = TimeSpan.FromSeconds(10),
            GuestJit = new() { Mode = mode, HotBlockThreshold = 1, AllowUnverifiedWindows = true } });
        machine.ImportImage([new("/bin/test", image, Executable: true)]);
        var run = await machine.StartAsync(new() { Executable = "/bin/test" }, timeout.Token);
        var result = await run.WaitAsync(timeout.Token);
        int? pid = run.WorkerProcessId;
        await run.DisposeAsync();
        await machine.DisposeAsync();
        SignalRow row = new() {
            Fixture = fixture, Execution = execution.ToString(), Mode = mode.ToString(),
            Host = RuntimeInformation.FrameworkDescription, OS = RuntimeInformation.OSDescription,
            Architecture = RuntimeInformation.ProcessArchitecture.ToString(), SimulatedCapabilities = false,
            ImageSha256 = Convert.ToHexStringLower(SHA256.HashData(image)),
            Reason = result.Reason.ToString(), ExitCode = result.ExitCode, Signal = result.Signal, Halt = result.Halt,
            Instructions = result.Instructions, StdoutBase64 = Convert.ToBase64String(result.StandardOutput),
            Stderr = Encoding.UTF8.GetString(result.StandardError), Diagnostic = result.Diagnostic,
            Released = result.ResourcesReleased, WorkerGone = pid == null || Gone(pid.Value),
            Stats = result.JitStatistics };
        rows.Add(row); Save(); // retain failing observations too
        Check(row.ImageSha256 == manifest.RootElement.GetProperty(fixture).GetProperty("sha256").GetString(), fixture + " fixture identity");
        Check(row.Released && row.WorkerGone && row.Stats?.CacheBytes is null or 0, fixture + " cleanup");
        Check(row.Stderr == "", fixture + " stderr: " + row.Stderr);
        if (fixture == "uncaught-ud2")
        {
            Check(row.Reason == "GuestSignal" && row.Signal == 4, fixture + " termination: " + row.Diagnostic);
            ulong ip = manifest.RootElement.GetProperty(fixture).GetProperty("symbols").GetProperty("uncaught_fault_site").GetUInt64();
            Check(row.Diagnostic?.Contains($"ip=0x{ip:x} address=0x{ip:x} signal=4 code=2", StringComparison.Ordinal) == true,
                  fixture + " exact fault address/code: " + row.Diagnostic);
            Check(Encoding.UTF8.GetString(result.StandardOutput) == "full-jit-signals: uncaught-before-ud2\n", fixture + " output");
        }
        else
        {
            Check(row.Reason == "Exited" && row.ExitCode == 0, fixture + " exit=" + row.ExitCode + ": " + row.Diagnostic);
            if (fixture == "cpu-policy") CheckPolicy(result.StandardOutput);
            else Check(Encoding.UTF8.GetString(result.StandardOutput) == Expected(fixture), fixture + " output");
        }
        if (baseline is not null)
        {
            Check((row.Reason, row.ExitCode, row.Signal, row.Halt, row.StdoutBase64) ==
                  (baseline.Reason, baseline.ExitCode, baseline.Signal, baseline.Halt, baseline.StdoutBase64), fixture + " mode parity");
            Check(row.Instructions == baseline.Instructions, fixture + " completed-instruction parity");
        }
        else baseline = row;
        if (mode == GuestJitMode.Full)
            Check(row.Stats is not null && row.Stats.Mode == "Full" && row.Stats.InterpreterInstructions == 0 &&
                  row.Stats.InterpreterAttempts == 0 && row.Stats.InlineNativeInstructions + row.Stats.SemanticHelperInstructions > 0,
                  fixture + " full mode must execute positive compiled work with zero interpreter work");
    }
}
Console.WriteLine("PASS FullJitSignals observations=" + rows.Count);

static string Expected(string fixture) => fixture switch
{
    "caught-gp-recursive" => "full-jit-signals: recursive=1 caught=2 code=128 trap=13 context=restored\n",
    "caught-gp" => "full-jit-signals: caught=2 code=128 trap=13 ip=edited registers=restored flags=restored mxcsr=restored mask=restored\n",
    "caught-ud2-recursive" => "full-jit-signals: recursive=1 caught=2 code=2 trap=6 context=restored\n",
    "caught-simd-recursive" => "full-jit-signals: recursive=1 caught=2 code=3 trap=19 context=restored\n",
    "caught-simd" => "full-jit-signals: caught=2 code=3 trap=19 ip=edited registers=restored flags=restored mxcsr=restored mask=restored\n",
    _ => "full-jit-signals: caught=2 code=2 trap=6 ip=edited registers=restored flags=restored mxcsr=restored mask=restored\n"
};

static void CheckPolicy(byte[] bytes)
{
    if (bytes.Length != 128) throw new InvalidOperationException("CPUID output length");
    uint Word(int row, int column) => System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(row*16+column*4, 4));
    uint edx1 = new[] { 0, 4, 6, 8, 15, 19, 23, 24, 25, 26 }.Aggregate(0u, (value, bit) => value | 1u << bit);
    uint edxExtended = new[] { 0, 8, 11, 15, 20, 23, 24, 29 }.Aggregate(0u, (value, bit) => value | 1u << bit);
    if (Word(0, 0) != 7 || Word(1, 2) != 0x80000000 || Word(1, 3) != edx1 || Word(6, 2) != 0 || Word(6, 3) != edxExtended)
        throw new InvalidOperationException("Unexpected virtual CPU baseline/optional policy");
    foreach (int row in new[] { 2, 3, 4, 5, 7 })
    for (int column = 0; column < 4; ++column)
        if (Word(row, column) != 0) throw new InvalidOperationException("Unexpected optional CPUID leaf");
}

static bool Gone(int pid)
{
    try { using var process = Process.GetProcessById(pid); return process.HasExited; }
    catch (ArgumentException) { return true; }
}

internal sealed class SignalRow
{
    public string Fixture { get; set; } = "";
    public string Execution { get; set; } = "";
    public string Mode { get; set; } = "";
    public string Host { get; set; } = "";
    public string OS { get; set; } = "";
    public string Architecture { get; set; } = "";
    public bool SimulatedCapabilities { get; set; }
    public string ImageSha256 { get; set; } = "";
    public string Reason { get; set; } = "";
    public int ExitCode { get; set; }
    public int Signal { get; set; }
    public int Halt { get; set; }
    public long Instructions { get; set; }
    public string StdoutBase64 { get; set; } = "";
    public string Stderr { get; set; } = "";
    public string? Diagnostic { get; set; }
    public bool Released { get; set; }
    public bool WorkerGone { get; set; }
    public GuestJitStatistics? Stats { get; set; }
}
[JsonSerializable(typeof(List<SignalRow>))]
internal partial class SignalsJson : JsonSerializerContext;
