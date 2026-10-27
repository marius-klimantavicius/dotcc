using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Managed.Emulation;
using Managed.Emulation.Execution;
using Managed.Emulation.Host;

if (args.Length < 3) throw new ArgumentException("qualify|bench|case fixtures-or-elf output.json [trials|mode] [jit|off] [guest arguments...]");
string command = args[0], input = Path.GetFullPath(args[1]), output = Path.GetFullPath(args[2]);
Directory.CreateDirectory(Path.GetDirectoryName(output)!);
var rows = new List<Row>();
using var overall = new CancellationTokenSource(TimeSpan.FromMinutes(10));
var token = overall.Token;
bool nativeExpected = RuntimeInformation.ProcessArchitecture == Architecture.X64 &&
    (OperatingSystem.IsLinux() || OperatingSystem.IsWindows());
GuestJitOptions Jit(bool enabled, bool capture = false, long cache = 8L << 20) =>
    new() { Enabled = enabled, CaptureFinalState = capture, CacheLimitBytes = cache, AllowUnverifiedWindows = true,
        HotBlockThreshold = capture ? 1 : 16 };
void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
void Save() => File.WriteAllText(output, JsonSerializer.Serialize(rows, JitJson.Default.ListRow));

async Task<Row> Public(string label, byte[] image, ExecutionMode mode, bool jit, string[]? arguments = null,
    long? budget = 100_000_000, TimeSpan? deadline = null, bool cancel = false, int workers = 64, long cache = 8L << 20, int hot = 16)
{
    await using var machine = new BlinkMachine(new() { ExecutionMode = mode, MemoryLimit = 256L << 20,
        VirtualMemoryLimit = 4L << 30, ThreadLimit = workers, InstructionLimit = budget,
        ExecutionDeadline = deadline ?? TimeSpan.FromSeconds(30), GuestJit = Jit(jit, cache: cache) with { HotBlockThreshold = hot } });
    machine.ImportImage([new("/bin/test", image, Executable: true)]);
    long allocated = GC.GetTotalAllocatedBytes(true);
    using var process = Process.GetCurrentProcess();
    var cpu = process.TotalProcessorTime;
    var watch = Stopwatch.StartNew();
    var run = await machine.StartAsync(new() { Executable = "/bin/test", Arguments = arguments ?? [] }, token);
    MachineRunResult result;
    if (cancel) { await run.Ready.WaitAsync(token); await Task.Delay(50, token); result = await run.StopAsync(TimeSpan.FromSeconds(5), token); }
    else result = await run.WaitAsync(token);
    watch.Stop();
    var row = new Row { Label = label, Mode = mode.ToString(), Jit = jit, Runtime = RuntimeInformation.FrameworkDescription,
        DynamicHost = System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeCompiled,
        Os = RuntimeInformation.OSDescription, WallMs = watch.Elapsed.TotalMilliseconds,
        CpuMs = (process.TotalProcessorTime - cpu).TotalMilliseconds, AllocatedBytes = GC.GetTotalAllocatedBytes(true) - allocated,
        ImageSha256 = Convert.ToHexStringLower(SHA256.HashData(image)), Instructions = result.Instructions,
        Reason = result.Reason.ToString(), ExitCode = result.ExitCode, Released = result.ResourcesReleased,
        Stdout = Encoding.UTF8.GetString(result.StandardOutput), Stderr = Encoding.UTF8.GetString(result.StandardError),
        Diagnostic = result.Diagnostic, Stats = result.JitStatistics, WorkerPid = run.WorkerProcessId };
    await run.DisposeAsync();
    await machine.DisposeAsync();
    row.WorkerGone = row.WorkerPid is not int pid || Gone(pid);
    rows.Add(row); Save();
    Check(row.Released && row.WorkerGone, label + " cleanup");
    Check(row.Stats == null || row.Stats.CacheBytes == 0, label + " retained code cache");
    return row;
}

async Task<ThreadedGuestExecutionResult> Snapshot(byte[] image, ulong budget, bool jit)
{
    const string path = "/bin/state";
    await using var io = new InstanceIo(new Dictionary<string, ReadOnlyMemory<byte>> { [path] = image },
        executablePaths: new HashSet<string> { path });
    using var stop = new HostExecutionStop(TimeSpan.FromSeconds(10));
    var owner = new ThreadedGuestExecution(io, stop, 32UL << 20, jitOptions: Jit(jit, true));
    ThreadedGuestExecutionResult? result = null; Exception? error = null;
    var thread = new Thread(() => { try { result = owner.Run(path, [path], [], budget); } catch (Exception e) { error = e; } });
    thread.Start();
    Check(thread.Join(TimeSpan.FromSeconds(15)), "state worker did not join");
    if (error != null) throw error;
    Check(owner.IsQuiescent && result!.MemoryReleased && result.AllWorkersJoined, "state ownership cleanup");
    return result!;
}

async Task CompareSnapshot(string fixture, byte[] image, ulong limit, bool entirelyNative = false)
{
    var baseline = await Snapshot(image, limit, false);
    var compiled = await Snapshot(image, limit, true);
    var a = baseline.Threads.Single().FinalState!; var b = compiled.Threads.Single().FinalState!;
    Check(baseline.Instructions == limit && compiled.Instructions == limit &&
        baseline.IssuedInstructions == compiled.IssuedInstructions && compiled.IssuedInstructions == limit + 1,
        fixture + " issued/completed mismatch at " + limit);
    Check(a.InstructionPointer == b.InstructionPointer && a.OperationLength == b.OperationLength &&
        a.Flags == b.Flags && a.ExportedFlags == b.ExportedFlags && a.Mxcsr == b.Mxcsr &&
        a.GeneralRegisters.SequenceEqual(b.GeneralRegisters) && a.VectorRegisters.SequenceEqual(b.VectorRegisters) &&
        a.FloatingPoint.SequenceEqual(b.FloatingPoint) && a.Segments.SequenceEqual(b.Segments),
        fixture + " architectural state mismatch at " + limit + " baseline=" + JsonSerializer.Serialize(a, JitJson.Default.GuestCpuState) +
        " native=" + JsonSerializer.Serialize(b, JitJson.Default.GuestCpuState));
    if (entirelyNative && nativeExpected)
        Check(compiled.JitStatistics?.GuestInstructions == (long)limit,
            fixture + " fell back inside its pure scalar prefix at " + limit);
    rows.Add(new() { Label = fixture == "original" ? "full-state-limit-" + limit : "full-state-" + fixture + "-limit-" + limit,
        Jit = true, Instructions = (long)limit,
        Reason = compiled.StopReason.ToString(), Released = true, WorkerGone = true, Stats = compiled.JitStatistics }); Save();
}

if (command == "qualify")
{
    byte[] limits = File.ReadAllBytes(Path.Combine(input, "limits.elf"));
    foreach (ulong limit in new ulong[] { 1, 2, 3, 7, 10, 11, 12, 13, 14, 15, 16, 31, 32, 33, 63, 64, 65, 127 })
        await CompareSnapshot("original", limits, limit);
    byte[] scalar = File.ReadAllBytes(Path.Combine(input, "scalar.elf"));
    // Every prefix boundary checks flags, partial/high-byte and alias writes,
    // issue rejection and continuation across the 31/32/33 dynamic limits.
    foreach (ulong limit in Enumerable.Range(1, 128).Select(x => (ulong)x).Concat(new ulong[] { 255, 256, 257 }))
        await CompareSnapshot("scalar", scalar, limit, entirelyNative: true);
    byte[] loads = File.ReadAllBytes(Path.Combine(input, "loads.elf"));
    foreach (ulong limit in Enumerable.Range(1, 65).Select(x => (ulong)x).Concat(new ulong[] { 127, 128, 129 }))
        await CompareSnapshot("loads", loads, limit);
    byte[] stackMemory = File.ReadAllBytes(Path.Combine(input, "stack-memory.elf"));
    foreach (ulong limit in Enumerable.Range(1, 129).Select(x => (ulong)x).Concat(new ulong[] { 255, 256, 257 }))
        await CompareSnapshot("stack-memory", stackMemory, limit);
    byte[] callReturn = File.ReadAllBytes(Path.Combine(input, "call-return.elf"));
    foreach (ulong limit in Enumerable.Range(1, 128).Select(x => (ulong)x).Concat(new ulong[] { 255, 256, 257 }))
        await CompareSnapshot("call-return", callReturn, limit);
    byte[] memoryFlags = File.ReadAllBytes(Path.Combine(input, "memory-flags.elf"));
    foreach (ulong limit in Enumerable.Range(1, 128).Select(x => (ulong)x).Concat(new ulong[] { 255, 256, 257 }))
        await CompareSnapshot("memory-flags", memoryFlags, limit);
    byte[] addressPlan = File.ReadAllBytes(Path.Combine(input, "address-plan.elf"));
    foreach (ulong limit in Enumerable.Range(1, 192).Select(x => (ulong)x).Concat(new ulong[] { 255, 256, 257 }))
        await CompareSnapshot("address-plan", addressPlan, limit);
    byte[] negativeResume = File.ReadAllBytes(Path.Combine(input, "resume.elf"));
    foreach (ulong limit in new ulong[] { 7, 8, 9, 10, 31, 32, 33, 63, 64, 65, 72, 73, 74, 127, 128, 129, 255, 256, 257, 264, 265, 266 })
        await CompareSnapshot("negative-resume", negativeResume, limit);
    foreach (ExecutionMode mode in Enum.GetValues<ExecutionMode>())
    {
        foreach (string fixture in new[] { "address-plan", "arithmetic", "scalar", "loads", "stack-memory", "call-return", "memory-flags", "semantics", "private-code", "private-data", "resume", "shared", "regular", "identity", "signals", "futex", "churn", "static-workers" })
        {
            byte[] image = File.ReadAllBytes(Path.Combine(input, fixture + ".elf"));
            string[] guestArgs = fixture is "churn" or "private-data" ? ["s"] : [];
            var baseline = await Public(fixture, image, mode, false, guestArgs, workers: fixture == "churn" ? 2 : 64, hot: fixture is "arithmetic" or "scalar" or "loads" or "stack-memory" or "call-return" or "memory-flags" or "semantics" or "private-code" ? 1 : 16);
            var compiled = await Public(fixture, image, mode, true, guestArgs, workers: fixture == "churn" ? 2 : 64, hot: fixture is "arithmetic" or "scalar" or "loads" or "stack-memory" or "call-return" or "memory-flags" or "semantics" or "private-code" ? 1 : 16);
            Check(baseline.Reason == "Exited" && baseline.ExitCode == 0 && compiled.Reason == baseline.Reason && compiled.ExitCode == 0 &&
                baseline.Stdout == compiled.Stdout && baseline.Stderr == compiled.Stderr && compiled.Stderr == "",
                fixture + " public mismatch: " + compiled.Diagnostic);
            if (fixture is "address-plan" or "arithmetic" or "scalar" or "loads" or "stack-memory" or "call-return" or "memory-flags" or "semantics" or "private-code" or "private-data" or "resume" or "shared" or "regular")
                Check(baseline.Instructions == compiled.Instructions, fixture + " deterministic instruction mismatch");
            if (nativeExpected && fixture is "address-plan" or "arithmetic" or "scalar" or "loads" or "stack-memory" or "call-return" or "memory-flags" or "semantics" or "private-code" or "private-data")
                Check(compiled.Stats?.GuestInstructions > 0 && compiled.Stats.NativeEntries > 0 &&
                    (compiled.Stats.InterpreterInstructions > 0 || fixture == "memory-flags" &&
                     compiled.Stats.Fallbacks.ContainsKey("UnsupportedInstruction")),
                    fixture + " did not prove hybrid native execution");
            if (fixture == "address-plan" && nativeExpected)
                Check(compiled.Stats?.NativeMemoryInstructions > 1000, "address recipe witness native memory work");
            if (fixture == "call-return")
            {
                Check(compiled.Stdout == "call-return: ok\n", "call/return witness output");
                if (nativeExpected) Check(compiled.Stats?.NativeMemoryInstructions > 0, "call/return native memory work");
            }
            if (fixture == "private-data")
                Check(compiled.Stdout == "private-data: 32768\n", "private data checksum output");
            if (fixture == "resume" && nativeExpected)
                Check(compiled.Stats?.GuestInstructions > 0 && compiled.Stats.CompileAttempts is > 0 and < 100 &&
                    compiled.Stats.NegativeCacheHits > 0 &&
                    compiled.Stats.Fallbacks.Any(pair => pair.Value > 0 && pair.Key is "MemoryInstruction" or "UnprofitableBlock"),
                    "native execution did not resume with bounded negative admission");
            if (fixture == "private-code" && nativeExpected)
                Check(compiled.Stats?.ValidationFailures >= 2, "private patch/remap did not revalidate positive native blocks");
            if (fixture == "static-workers" && nativeExpected)
                Check(compiled.Stats?.FirstChildInstructions >= 0 &&
                    compiled.Stats.GuestInstructions > compiled.Stats.NativeInstructionsAtFirstChild &&
                    compiled.Stats.StaticCodePages >= 2 && compiled.Stats.RevokedStaticCodePages >= 1 &&
                    compiled.Stats.Fallbacks.ContainsKey("StaticCodeOnly") && compiled.Stats.Fallbacks.ContainsKey("NonStaticCode"),
                    "initial ELF execution did not survive child publication or mapping revocation");
            if (fixture is "identity" or "signals" or "futex" or "churn")
                Check(compiled.Stats?.Fallbacks.ContainsKey("Multiworker") == true ||
                    compiled.Stats?.Fallbacks.ContainsKey("StaticCodeOnly") == true || !nativeExpected,
                    fixture + " missing static-code/multiworker restriction");
        }
        foreach (string policy in new[] { "negative-change", "negative-pressure" })
        {
            var image = File.ReadAllBytes(Path.Combine(input, policy + ".elf"));
            long cacheLimit = policy == "negative-pressure" ? 65536 : 8L << 20;
            var a = await Public(policy, image, mode, false, workers: 1, cache: cacheLimit);
            var b = await Public(policy, image, mode, true, workers: 1, cache: cacheLimit);
            Check(a.Reason == "Exited" && a.ExitCode == 0 && b.Reason == a.Reason && b.ExitCode == 0 &&
                a.Stdout == policy + ": ok\n" && b.Stdout == a.Stdout && a.Stderr == "" && b.Stderr == "" &&
                a.Instructions == b.Instructions, policy + " ordinary recovery witness");
            if (nativeExpected)
            {
                var stats = b.Stats!;
                Check(stats.GuestInstructions > 0 && stats.NativeEntries > 0 && stats.InterpreterInstructions > 0 &&
                    stats.NegativeCacheHits > 0 && stats.PeakCacheBytes <= cacheLimit, policy + " active bounded hybrid cache");
                if (policy == "negative-change")
                    Check(stats.NegativePromotions > 0 && stats.NativeMemoryInstructions > 0 &&
                        stats.Fallbacks.GetValueOrDefault("UnprofitableBlock") > 0, "suffix-only negative became native positive");
                else
                    Check(stats.NegativeBudgetEvictions > 0 && stats.BudgetEvictions >= stats.NegativeBudgetEvictions &&
                        stats.Evictions >= stats.BudgetEvictions && stats.NativeMemoryInstructions > 1000 &&
                        stats.Fallbacks.GetValueOrDefault("UnsupportedInstruction") > 0, "negative pressure admitted useful positive");
            }
        }
        byte[] privateData = File.ReadAllBytes(Path.Combine(input, "private-data.elf"));
        var dataBaseline = await Public("private-data-workers", privateData, mode, false);
        var dataCompiled = await Public("private-data-workers", privateData, mode, true);
        // Futex waits and reader visits depend on scheduling; only the single
        // worker variant above has a deterministic instruction count.
        Check(dataBaseline.Reason == "Exited" && dataBaseline.ExitCode == 0 &&
            dataCompiled.Reason == dataBaseline.Reason && dataCompiled.ExitCode == 0 &&
            dataBaseline.Stdout == "private-data: 32768\n" && dataCompiled.Stdout == dataBaseline.Stdout &&
            dataBaseline.Stderr == "" && dataCompiled.Stderr == "",
            "private data workers mismatch: " + dataCompiled.Diagnostic);
        if (nativeExpected)
            Check(dataCompiled.Stats?.FirstChildInstructions >= 0 &&
                dataCompiled.Stats.GuestInstructions > dataCompiled.Stats.NativeInstructionsAtFirstChild &&
                dataCompiled.Stats.NativeEntries > 0 && dataCompiled.Stats.InterpreterInstructions > 0 &&
                dataCompiled.Stats.NativeMemoryInstructions - dataCompiled.Stats.NativeMemoryInstructionsAtFirstChild >= 1000,
                "later private data/stack work did not execute natively after child publication");
        foreach (bool jit in new[] { false, true })
        {
            var budget = await Public("budget", limits, mode, jit, budget: 33);
            Check(budget.Reason == "InstructionLimit" && budget.Instructions == 33, "public exact budget");
            var stopImage = File.ReadAllBytes(Path.Combine(input, "stop.elf"));
            var deadline = await Public("deadline", stopImage, mode, jit, budget: null, deadline: TimeSpan.FromSeconds(1));
            Check(deadline.Reason == "Deadline", "public deadline: " + deadline.Reason);
            var cancelled = await Public("cancel", stopImage, mode, jit, budget: null, cancel: true);
            Check(cancelled.Reason == "Stopped", "public cancellation: " + cancelled.Reason);
            // The stack arithmetic control is intentionally interpreted; its
            // hot negative loop must keep per-instruction stop checkpoints.
            byte[] negativeStop = File.ReadAllBytes(Path.Combine(input, "stack.elf"));
            var negativeDeadline = await Public("negative-deadline", negativeStop, mode, jit, budget: null,
                deadline: TimeSpan.FromSeconds(1));
            Check(negativeDeadline.Reason == "Deadline", "negative deadline: " + negativeDeadline.Reason);
            var negativeCancel = await Public("negative-cancel", negativeStop, mode, jit, budget: null, cancel: true);
            Check(negativeCancel.Reason == "Stopped", "negative cancellation: " + negativeCancel.Reason);
        }
        var smallCache = await Public("bounded-cache", File.ReadAllBytes(Path.Combine(input, "arithmetic.elf")), mode, true, cache: 65536, hot: 1);
        Check(smallCache.Reason == "Exited" && smallCache.ExitCode == 0 && smallCache.Stats?.PeakCacheBytes <= 65536, "cache bound");
    }
    foreach (int owners in new[] { 2, 4 })
    {
        byte[] arithmetic = File.ReadAllBytes(Path.Combine(input, "arithmetic.elf"));
        // Concurrent owners and moving GCs exercise root/lifetime isolation. Rows
        // are recorded after all owners complete to keep JSON writes serialized.
        var gcStop = new CancellationTokenSource();
        var collector = Task.Run(async () => { while (!gcStop.IsCancellationRequested) { GC.Collect(2, GCCollectionMode.Forced, true, true); await Task.Delay(2); } });
        var tasks = Enumerable.Range(0, owners).Select(async _ =>
        {
            await using var machine = new BlinkMachine(new() { GuestJit = Jit(true) with { HotBlockThreshold = 1 }, InstructionLimit = 1000000 });
            machine.ImportImage([new("/bin/test", arithmetic, Executable: true)]);
            return await machine.ExecuteAsync(new() { Executable = "/bin/test" }, token);
        }).ToArray();
        MachineRunResult[] results;
        try { results = await Task.WhenAll(tasks); }
        finally { gcStop.Cancel(); await collector; gcStop.Dispose(); }
        Check(results.All(r => r.Reason == RunExitReason.Exited && r.ExitCode == 0 && r.ResourcesReleased &&
            (!nativeExpected || r.JitStatistics?.GuestInstructions > 0) && r.JitStatistics?.CacheBytes == 0), "concurrent GC owners");
        rows.Add(new() { Label = "concurrent-GC-owners-" + owners, Released = true, WorkerGone = true }); Save();
    }
}
else if (command == "bench")
{
    int trials = args.Length > 3 ? int.Parse(args[3]) : 5;
    foreach (string fixture in new[] { "register", "stack", "file" })
    {
        var image = File.ReadAllBytes(Path.Combine(input, fixture + ".elf"));
        for (int trial = 0; trial <= trials; ++trial)
            foreach (bool jit in trial % 2 == 0 ? new[] { false, true } : new[] { true, false })
            {
                var row = await Public(fixture + "-" + trial, image, ExecutionMode.InProcess, jit);
                row.Warmup = trial == 0; Save();
                Check(row.Reason == "Exited" && row.ExitCode == 0 && row.Stderr == "", "benchmark functional result");
                if (jit && nativeExpected && fixture == "register") Check(row.Stats?.GuestInstructions > 0, "benchmark no native blocks");
            }
    }
}
else if (command == "case")
{
    var row = await Public(Path.GetFileName(input), File.ReadAllBytes(input), Enum.Parse<ExecutionMode>(args[3]), args[4] == "jit", args.Skip(5).ToArray());
    Check(row.Reason == "Exited" && row.ExitCode == 0 && row.Stderr == "", row.Diagnostic ?? "case failed");
}
else throw new ArgumentException("Unknown command");
Console.WriteLine("PASS " + command + " observations=" + rows.Count + " " + output);
return 0;

static bool Gone(int pid)
{
    try { using var p = Process.GetProcessById(pid); return p.HasExited; }
    catch (ArgumentException) { return true; }
}

internal sealed class Row
{
    public string Label { get; set; } = "";
    public string Mode { get; set; } = "";
    public bool Jit { get; set; }
    public bool Warmup { get; set; }
    public string Runtime { get; set; } = "";
    public bool DynamicHost { get; set; }
    public string Os { get; set; } = "";
    public double WallMs { get; set; }
    public double CpuMs { get; set; }
    public long AllocatedBytes { get; set; }
    public string ImageSha256 { get; set; } = "";
    public long Instructions { get; set; }
    public string Reason { get; set; } = "";
    public int ExitCode { get; set; }
    public bool Released { get; set; }
    public bool WorkerGone { get; set; }
    public int? WorkerPid { get; set; }
    public string Stdout { get; set; } = "";
    public string Stderr { get; set; } = "";
    public string? Diagnostic { get; set; }
    public GuestJitStatistics? Stats { get; set; }
}
[JsonSerializable(typeof(List<Row>))]
[JsonSerializable(typeof(GuestCpuState))]
internal partial class JitJson : JsonSerializerContext;
