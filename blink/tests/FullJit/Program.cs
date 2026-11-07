using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Managed.Emulation;
using Managed.Emulation.Execution;
using Managed.Emulation.Host;

if (args.Length < 3) throw new ArgumentException("FullJit public|state|cpu fixtures output.json [InProcess|SeparateProcess]");
string command = args[0], fixtures = Path.GetFullPath(args[1]), output = Path.GetFullPath(args[2]);
Directory.CreateDirectory(Path.GetDirectoryName(output)!);
List<Row> rows = [];
using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(30));
void Check(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
void Save() => File.WriteAllText(output, JsonSerializer.Serialize(rows, ReportJson.Default.ListRow));

async Task<Row> Run(string label, byte[] image, ExecutionMode execution, GuestJitMode mode, string[] arguments,
    long budget = 10_000_000, bool capture = false, GuestJitHostFeatures hostFeatures = GuestJitHostFeatures.Detected,
    long cache = 8L << 20, TimeSpan? deadline = null, bool stopAfterReady = false, long memoryLimit = 128L << 20, int threadLimit = 64, bool expectRetentionDenial = false)
{
    await using var machine = new BlinkMachine(new() { ExecutionMode = execution,
        MemoryLimit = memoryLimit, VirtualMemoryLimit = 2L << 30, ThreadLimit = threadLimit,
        InstructionLimit = budget, ExecutionDeadline = deadline ?? TimeSpan.FromSeconds(45),
        GuestJit = new() { Mode = mode, HostFeatures = hostFeatures, CacheLimitBytes = cache,
            CaptureFinalState = capture, HotBlockThreshold = 1, AllowUnverifiedWindows = true } });
    machine.ImportImage([new("/bin/test", image, Executable: true)]);
    var watch = Stopwatch.StartNew();
    var run = await machine.StartAsync(new() { Executable = "/bin/test", Arguments = arguments,
        Console = new() { CaptureBytes = 65536, OutputLimit = 1L << 20 } }, timeout.Token);
    if (stopAfterReady) { await run.Ready.WaitAsync(timeout.Token); await Task.Delay(30, timeout.Token); await run.StopAsync(TimeSpan.FromSeconds(5), timeout.Token); }
    var result = await run.WaitAsync(timeout.Token);
    int? pid = run.WorkerProcessId;
    await run.DisposeAsync(); await machine.DisposeAsync();
    Row row = new() { Label = label, Mode = mode.ToString(), Execution = execution.ToString(),
        Host = RuntimeInformation.FrameworkDescription, Result = result, WallMilliseconds = watch.Elapsed.TotalMilliseconds,
        WorkerGone = pid == null || Gone(pid.Value) };
    lock (rows) { rows.Add(row); Save(); }
    Check(row.WorkerGone && result.ResourcesReleased, label + " cleanup");
    if (mode == GuestJitMode.Full)
    {
        var stats = result.JitStatistics;
        Check(stats != null && stats.Mode == "Full" && stats.InterpreterInstructions == 0 && stats.InterpreterAttempts == 0,
            label + " strict execution: " + result.Diagnostic);
        Check(stats!.InlineNativeInstructions + stats.SemanticHelperInstructions == result.Instructions,
            label + " completed accounting");
        Check(stats.ActiveDataBorrows == 0 && stats.DeferredAnonymousPages == 0, label + " data borrow cleanup");
        // A deadline/stop can win before guest entry, especially while a
        // separate worker starts. Zero completed instructions then require no
        // native entry; ordinary fixtures still require compiled execution.
        bool stoppedBeforeEntry = result.Instructions == 0 &&
            result.Reason is RunExitReason.Deadline or RunExitReason.Stopped;
        bool cleanDenial = expectRetentionDenial && result.Reason == RunExitReason.ExecutionFailure &&
            result.Instructions == 0 && result.ResourcesReleased &&
            result.Diagnostic?.Contains("Full-JIT configured retention quota", StringComparison.Ordinal) == true;
        Check(!expectRetentionDenial || cleanDenial, label + " explicit underfunded retention rejection");
        Check((stats.NativeEntries > 0 || stoppedBeforeEntry || cleanDenial) && stats.CacheBytes == 0 && stats.PeakCacheBytes <= cache,
            label + " native/cache accounting");
    }
    return row;
}

if (command == "public")
{
    string[] names = ["arithmetic", "address-plan", "limits", "resume", "scalar", "loads", "stack-memory", "memory-flags",
        "call-return", "private-code", "static-workers", "private-data", "shared", "regular", "identity", "signals", "futex", "churn",
        "negative-change", "negative-pressure", "native-chain", "memory-recovery", "elf-baseline"];
    foreach (var execution in Enum.GetValues<ExecutionMode>())
    foreach (string name in names)
    {
        byte[] image = File.ReadAllBytes(Path.Combine(fixtures, name + ".elf"));
        string[] guestArgs = name is "churn" or "private-data" ? ["s"] : [];
        long budget = name == "limits" ? 33 : 10_000_000;
        var baseline = (await Run(name, image, execution, GuestJitMode.Interpreter, guestArgs, budget)).Result;
        var full = (await Run(name, image, execution, GuestJitMode.Full, guestArgs, budget)).Result;
        Check((baseline.Reason, baseline.ExitCode, baseline.Signal, baseline.Halt) ==
            (full.Reason, full.ExitCode, full.Signal, full.Halt), name + " result parity: " + full.Diagnostic);
        if (name is not ("static-workers" or "identity" or "signals" or "futex" or "churn"))
            Check(baseline.Instructions == full.Instructions, name + " deterministic completed count");
        Check(baseline.StandardOutput.SequenceEqual(full.StandardOutput) && baseline.StandardError.SequenceEqual(full.StandardError), name + " output parity");
        Check(name == "limits" ? full.Reason == RunExitReason.InstructionLimit && full.Instructions == 33 :
            full.Reason == RunExitReason.Exited && full.ExitCode == 0, name + " completion: " + full.Diagnostic);
        if (name == "native-chain") Check(full.JitStatistics!.NativeTailJumps > 0, "positive owner native tail-jump witness");
        if (name is "elf-baseline" or "arithmetic")
        {
            var masked = (await Run(name+"/SIMULATED-baseline-host-mask", image, execution, GuestJitMode.Full, guestArgs, budget,
                hostFeatures: GuestJitHostFeatures.BaselineOnly)).Result;
            Check(masked.StandardOutput.SequenceEqual(baseline.StandardOutput) && masked.Reason == baseline.Reason &&
                masked.ExitCode == baseline.ExitCode && masked.Instructions == baseline.Instructions,
                name+" simulated host-mask software parity");
            // Full scalar/vector emission now uses baseline x64 instructions
            // and software status capture. BaselineOnly excludes OPTIONAL host
            // instructions; it does not require all guest work to use helpers.
            Check(masked.JitStatistics!.InlineNativeInstructions > 0 && masked.JitStatistics.InterpreterAttempts == 0 &&
                masked.JitStatistics.HostFeatures == "BaselineOnly", name+" simulated optional-host exclusion with baseline native work");
        }
    }
    foreach (var execution in Enum.GetValues<ExecutionMode>())
    {
        byte[] stopImage = File.ReadAllBytes(Path.Combine(fixtures, "stop.elf"));
        var stopped = (await Run("cooperative-stop", stopImage, execution, GuestJitMode.Full, [], 100_000_000,
            stopAfterReady: true)).Result;
        Check(stopped.Reason == RunExitReason.Stopped, "compiled cooperative stop");
        var expired = (await Run("deadline", stopImage, execution, GuestJitMode.Full, [], 100_000_000,
            deadline: TimeSpan.FromMilliseconds(100))).Result;
        Check(expired.Reason == RunExitReason.Deadline, "compiled deadline");
        byte[] recovery = File.ReadAllBytes(Path.Combine(fixtures, "memory-recovery.elf"));
        await Run("undersized-cache-denial", recovery, execution, GuestJitMode.Full, [], cache: 131072,
            memoryLimit: 8L << 20, threadLimit: 2, expectRetentionDenial: true);
        // The charged compiler/worker/authority resources plus the minimum
        // arena exceed 2 MiB in this two-worker profile. Keep that rejection
        // explicit, then qualify a separately declared fitting 3 MiB quota.
        await Run("undersized-cache-owner-floor", recovery, execution, GuestJitMode.Full, [], cache: 2L << 20,
            memoryLimit: 8L << 20, threadLimit: 2, expectRetentionDenial: true);
        var retained = (await Run("small-retained-cache-recovery", recovery, execution, GuestJitMode.Full, [], cache: 3L << 20,
            memoryLimit: 8L << 20, threadLimit: 2)).Result;
        Check(retained.Reason == RunExitReason.Exited && retained.ExitCode == 0 &&
            retained.JitStatistics!.PeakResidentTranslationBlocks > 0 && retained.JitStatistics.PeakCacheBytes <= 3L << 20,
            "fitting retained cache recovery; dedicated controller tests exercise capacity exhaustion");
    }
    // Separate owners share no guest pointers or compiled addresses, including
    // while a moving collection runs during their managed/native transitions.
    byte[] simultaneousImage = File.ReadAllBytes(Path.Combine(fixtures, "resume.elf"));
    var simultaneous = Enumerable.Range(0, 4).Select(i => Run("moving-gc-owner-"+i, simultaneousImage,
        ExecutionMode.InProcess, GuestJitMode.Full, [])).ToArray();
    for (int gc = 0; gc < 3; ++gc) { await Task.Delay(5); GC.Collect(2, GCCollectionMode.Forced, true, true); }
    foreach (var row in await Task.WhenAll(simultaneous))
        Check(row.Result!.Reason == RunExitReason.Exited && row.Result.ExitCode == 0, "simultaneous compiled owner");
}
else if (command == "state")
{
    foreach (string name in new[] { "arithmetic", "address-plan", "loads", "stack-memory", "memory-flags", "call-return", "scalar" })
    foreach (ulong budget in Enumerable.Range(1, 65).Select(n => (ulong)n))
    {
        byte[] image = File.ReadAllBytes(Path.Combine(fixtures, name + ".elf"));
        async Task<ThreadedGuestExecutionResult> Snapshot(GuestJitMode mode)
        {
            await using var io = new InstanceIo(new Dictionary<string, ReadOnlyMemory<byte>> { ["/bin/test"] = image },
                executablePaths: new HashSet<string> { "/bin/test" });
            using var stop = new HostExecutionStop(TimeSpan.FromSeconds(10));
            var owner = new ThreadedGuestExecution(io, stop, 32UL << 20, maximumWorkers: 2,
                jitOptions: new() { Mode = mode, CaptureFinalState = true, HotBlockThreshold = 1, AllowUnverifiedWindows = true });
            ThreadedGuestExecutionResult? result = null; Exception? error = null;
            var thread = new Thread(() => { try { result = owner.Run("/bin/test", ["/bin/test"], [], budget); } catch (Exception e) { error = e; } });
            thread.Start(); Check(thread.Join(TimeSpan.FromSeconds(15)), "snapshot join");
            if (error != null) throw error;
            Check(owner.IsQuiescent && result!.MemoryReleased && result.AllWorkersJoined, "snapshot cleanup");
            return result!;
        }
        var baseline = await Snapshot(GuestJitMode.Interpreter); var full = await Snapshot(GuestJitMode.Full);
        var a = baseline.Threads.Single().FinalState!; var b = full.Threads.Single().FinalState!;
        Check(baseline.Instructions == full.Instructions && baseline.IssuedInstructions == full.IssuedInstructions,
            name + " instruction boundary " + budget);
        Check(a.InstructionPointer == b.InstructionPointer && a.OperationLength == b.OperationLength && a.Flags == b.Flags &&
            a.ExportedFlags == b.ExportedFlags && a.Mxcsr == b.Mxcsr && a.GeneralRegisters.SequenceEqual(b.GeneralRegisters) &&
            a.VectorRegisters.SequenceEqual(b.VectorRegisters) && a.FloatingPoint.SequenceEqual(b.FloatingPoint) && a.Segments.SequenceEqual(b.Segments),
            name + " architectural boundary " + budget + " baseline=" + JsonSerializer.Serialize(a, ReportJson.Default.GuestCpuState) +
            " full=" + JsonSerializer.Serialize(b, ReportJson.Default.GuestCpuState));
        Check(full.JitStatistics.InterpreterInstructions == 0 && full.JitStatistics.InterpreterAttempts == 0 &&
            full.JitStatistics.InlineNativeInstructions + full.JitStatistics.SemanticHelperInstructions == (long)full.Instructions,
            name + " strict boundary " + budget);
        rows.Add(new() { Label = name + "/" + budget, Mode = "Full", State = b, WorkerGone = true }); Save();
    }
}
else if (command == "cpu")
{
    ExecutionMode execution = args.Length > 3 ? Enum.Parse<ExecutionMode>(args[3]) : ExecutionMode.InProcess;
    using var descriptors = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(fixtures, "corpus.json")));
    foreach (var descriptor in descriptors.RootElement.GetProperty("cases").EnumerateArray())
    {
        if (descriptor.GetProperty("fault").GetInt32() != 0) continue;
        string name = descriptor.GetProperty("name").GetString()!, index = descriptor.GetProperty("index").GetInt32().ToString();
        byte[] image = File.ReadAllBytes(Path.Combine(fixtures, "cpu-"+index+".elf"));
        var baseline = (await Run(name, image, execution, GuestJitMode.Interpreter, [])).Result;
        var full = (await Run(name, image, execution, GuestJitMode.Full, [])).Result;
        Check(baseline.Reason == RunExitReason.Exited && baseline.ExitCode == 0 && full.Reason == baseline.Reason && full.ExitCode == 0,
            name + " cpu completion " + full.Diagnostic);
        Check(baseline.StandardError.Length == 0 && full.StandardError.Length == 0, name + " cpu stderr");
        byte[] a = baseline.StandardOutput, b = full.StandardOutput;
        Check(a.Length == 88+descriptor.GetProperty("dataPages").GetInt32()*4096 && a.Length == b.Length, name+" capture size");
        for (int offset = 0; offset < a.Length; ++offset)
        {
            if (name == "rdtsc-defined-state" && (offset < 4 || offset is >= 24 and < 28)) continue;
            Check(a[offset] == b[offset], name + " cpu byte " + offset);
        }
        // Independent native oracle uses the original defined-field contract:
        // CPUID values are virtual; timestamps and undefined flags are excluded.
        string nativePath = Path.Combine(fixtures, "cpu-"+index+".native");
        if (File.Exists(nativePath))
        {
            byte[] native = File.ReadAllBytes(nativePath);
            bool cpuid = descriptor.GetProperty("profileReference").GetInt32() != 0;
            ulong mask = Convert.ToUInt64(descriptor.GetProperty("flagMask").GetString(), 16);
            ulong flags = System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(b.AsSpan(32, 8));
            ulong nativeFlags = System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(native.AsSpan(32, 8));
            Check(((flags ^ nativeFlags) & mask) == 0, name+" native defined flags");
            for (int offset = 0; offset < b.Length; ++offset)
            {
                if (offset is >= 32 and < 40 || cpuid && offset < 32 || name == "rdtsc-defined-state" && (offset < 4 || offset is >= 24 and < 28)) continue;
                Check(b[offset] == native[offset], name+" native capture byte "+offset);
            }
        }
    }
}
else if (command == "tripwire")
{
    await using var io = new InstanceIo(new Dictionary<string, ReadOnlyMemory<byte>>());
    using var stop = new HostExecutionStop();
    var owner = new ThreadedGuestExecution(io, stop, jitOptions: new() { Mode = GuestJitMode.Full, AllowUnverifiedWindows = true });
    unsafe
    {
        using var program = new Managed.Emulation.BlinkCore();
        using var binding = program.__DotCcEnter();
        Managed.Emulation.BlinkCore.Machine machine = default;
        machine.ip = 0x1234;
        Managed.Emulation.BlinkCore.BindHostGuestThreads(owner, program);
        program.SetHostGuestCurrentMachine(&machine);
        try
        {
            for (int attempt = 0; attempt < 3; ++attempt)
            {
                bool rejected = false;
                try
                {
                    if (attempt == 0) program.ExecuteInstruction(&machine);
                    else if (attempt == 1) program.JitlessDispatch(&machine, 0, 0, 0);
                    else _ = program.GetOp(0);
                }
                catch (InvalidOperationException e) when (e.Message.StartsWith("Full-JIT interpreter tripwire")) { rejected = true; }
                Check(rejected && owner.JitStatistics.InterpreterAttempts == attempt+1, "generated old-dispatch tripwire/counter " + attempt);
            }
        }
        finally { program.SetHostGuestCurrentMachine(null); Managed.Emulation.BlinkCore.UnbindHostGuestThreads(); }
    }
    Save();
}
else throw new ArgumentException("Unknown command");
Console.WriteLine("PASS FullJit " + command + " rows=" + rows.Count);

static bool Gone(int pid) { try { using var process = Process.GetProcessById(pid); return process.HasExited; } catch (ArgumentException) { return true; } }
internal sealed class Row
{
    public string Label { get; set; } = "";
    public string Mode { get; set; } = "";
    public string Execution { get; set; } = "";
    public string Host { get; set; } = "";
    public MachineRunResult? Result { get; set; }
    public GuestCpuState? State { get; set; }
    public double WallMilliseconds { get; set; }
    public bool WorkerGone { get; set; }
}
[JsonSerializable(typeof(List<Row>))]
[JsonSerializable(typeof(GuestCpuState))]
internal partial class ReportJson : JsonSerializerContext;
