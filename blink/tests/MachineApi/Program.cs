using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Managed.Emulation;

if (args.Length != 3 || !Enum.TryParse<ExecutionMode>(args[0], out var mode))
    throw new ArgumentException("usage: Probe InProcess|SeparateProcess fixture-elf attempt-directory");
string fixture = Path.GetFullPath(args[1]), output = Path.GetFullPath(args[2]);
Directory.CreateDirectory(output);
byte[] elf = await File.ReadAllBytesAsync(fixture);
var evidence = new List<string>();
var processes = new List<int>();
string cwd = Environment.CurrentDirectory;
var environment = Environment.GetEnvironmentVariables().Cast<System.Collections.DictionaryEntry>()
    .ToDictionary(p => (string)p.Key, p => (string?)p.Value, StringComparer.Ordinal);
var consoleIn = Console.In; var consoleOut = Console.Out; var consoleError = Console.Error;
using var overall = new CancellationTokenSource(TimeSpan.FromMinutes(3));
var token = overall.Token;
MachineOptions Options() => new() { ExecutionMode = mode, MemoryLimit = 32L << 20,
    InstructionLimit = 2_000_000, ExecutionDeadline = TimeSpan.FromSeconds(15),
    Environment = new Dictionary<string, string> { ["FOO"] = "base", ["DROP"] = "remove" } };
void Check(bool ok, string detail) { if (!ok) throw new InvalidOperationException(detail); }
BlinkMachine Create(MachineOptions? options = null)
{
    var machine = new BlinkMachine(options ?? Options());
    machine.ImportImage([new("/bin/probe", elf, Executable: true)]);
    return machine;
}
ExecutionOptions Command(params string[] arguments) => new() { Executable = "/bin/probe", Arguments = arguments };
async Task<MachineRunResult> Execute(BlinkMachine machine, ExecutionOptions options, string expected, string stderr = "", int status = 0)
{
    var result = await machine.ExecuteAsync(options, token);
    if (machine.CurrentRun!.WorkerProcessId is int pid) processes.Add(pid);
    Check(result.Reason == RunExitReason.Exited && result.ExitCode == status && result.ResourcesReleased && result.Instructions > 0,
        $"{string.Join(' ', options.Arguments)} result {result.Reason}/{result.ExitCode}: {result.Diagnostic}");
    Check(Encoding.UTF8.GetString(result.StandardOutput) == expected && Encoding.UTF8.GetString(result.StandardError) == stderr,
        "console mismatch for " + string.Join(' ', options.Arguments));
    return result;
}
await using (var machine = Create())
{
    Check(machine.ExecutionMode == mode, "mode changed");
    await Execute(machine, Command("mkdir", "/work"), "mkdir-ok\n");
    await Execute(machine, Command("inspect", "argument with spaces") with {
        WorkingDirectory = "/work", Environment = new Dictionary<string, string?> { ["FOO"] = "run", ["DROP"] = null, ["EXTRA"] = "seen" }
    }, "cwd=/work\narg=/bin/probe\narg=inspect\narg=argument with spaces\nenv=FOO=run\nenv=EXTRA=seen\n", "inspect-ok\n");
    await Execute(machine, Command("store", "/work/value", "first"), "store-ok\n");
    await Execute(machine, Command("append", "/work/value", "-second"), "store-ok\n");
    await Execute(machine, Command("rename", "/work/value", "/work/moved"), "rename-ok\n");
    await Execute(machine, Command("load", "/work/moved"), "first-second");
    using var input = new MemoryStream(new byte[] { 0, 255, 17, 10, 128 });
    using var stdout = new MemoryStream(); using var stderr = new MemoryStream();
    var echoed = await machine.ExecuteAsync(Command("echo") with { Console = new() { Input = input, Output = stdout, Error = stderr, BufferBytes = 3, LeaveOpen = true } }, token);
    Check(echoed.Reason == RunExitReason.Exited && echoed.ExitCode == 0 && echoed.ResourcesReleased,
        $"binary echo outcome: reason={echoed.Reason}, exit={echoed.ExitCode}, released={echoed.ResourcesReleased}, instructions={echoed.Instructions}, stdout={Convert.ToHexString(stdout.ToArray())}, stderr={Convert.ToHexString(stderr.ToArray())}, diagnostic={echoed.Diagnostic}");
    Check(stdout.ToArray().SequenceEqual(new byte[] { 0, 255, 17, 10, 128 }) && Encoding.UTF8.GetString(stderr.ToArray()) == "echo-eof\n" && input.CanRead && stdout.CanWrite, "binary/EOF/borrowed streams");
    evidence.Add("argv-env-cwd-private-persistence-binary-eof");
    var ownedInput = new MemoryStream(new byte[] { 0, 255, 17, 10, 128 });
    var ownedOutput = new MemoryStream(); var ownedError = new MemoryStream();
    try
    {
        var owned = await machine.ExecuteAsync(Command("echo") with {
            Console = new() { Input = ownedInput, Output = ownedOutput, Error = ownedError, BufferBytes = 3, LeaveOpen = false }
        }, token);
        Check(owned.Reason == RunExitReason.Exited && owned.ExitCode == 0 && owned.ResourcesReleased && owned.Diagnostic == null,
            "owned echo outcome");
        Check(ownedOutput.ToArray().SequenceEqual(new byte[] { 0, 255, 17, 10, 128 }) && Encoding.UTF8.GetString(ownedError.ToArray()) == "echo-eof\n",
            "owned stream bytes");
        Check(!ownedInput.CanRead && !ownedOutput.CanWrite && !ownedError.CanWrite, "owned console streams were not all disposed");
        if (machine.CurrentRun!.WorkerProcessId is int pid) processes.Add(pid);
        evidence.Add("owned-console-stream-disposal");
    }
    finally { ownedInput.Dispose(); ownedOutput.Dispose(); ownedError.Dispose(); }
}
string mountedWork = Directory.CreateDirectory(Path.Combine(output, "mounted-work")).FullName;
string mountedElf = Path.Combine(mountedWork, "my_app");
File.Copy(fixture, mountedElf);
if (!OperatingSystem.IsWindows()) File.SetUnixFileMode(mountedElf, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
await using (var machine = new BlinkMachine(Options()))
{
    machine.MountDirectory("/work", mountedWork);
    await Execute(machine, new() { Executable = "/work/my_app", WorkingDirectory = "/work",
        Arguments = ["inspect", "mounted executable"] },
        "cwd=/work\narg=/work/my_app\narg=inspect\narg=mounted executable\nenv=FOO=base\nenv=DROP=remove\n", "inspect-ok\n");
    await Execute(machine, new() { Executable = "/work/my_app", WorkingDirectory = "/work",
        Arguments = ["store", "/work/result", "mounted-write"] }, "store-ok\n");
    Check(await File.ReadAllTextAsync(Path.Combine(mountedWork, "result"), token) == "mounted-write", "mounted execution did not persist write");
    evidence.Add("mounted-executable-without-import");
}
// Execute the ELF from a live read-only mount so the private 16-byte quota
// contains only guest data, independent of ELF import representation or size.
await using (var machine = new BlinkMachine(Options() with { DescriptorLimit = 8, WritableStorageLimit = 16 }))
{
    machine.MountDirectory("/work", mountedWork, MountAccess.ReadOnly);
    ExecutionOptions Resource(params string[] arguments) => new() { Executable = "/work/my_app", Arguments = arguments };
    await Execute(machine, Resource("memory-cap"), "", "errno=12\n", 1);
    await Execute(machine, Resource("descriptor-cap", "/quota-fds"), "descriptors=5 first=3 last=7\n", "errno=24\n", 1);
    await Execute(machine, Resource("storage-cap", "/quota-data"), "stored=16\n", "errno=28\n", 1);
    await Execute(machine, Resource("load", "/quota-data"), "ABCDEFGHIJKLMNOP");
    evidence.Add("normal-memory-descriptor-storage-limits");
}
string live = Directory.CreateDirectory(Path.Combine(output, "live")).FullName;
string readOnly = Directory.CreateDirectory(Path.Combine(output, "readonly")).FullName;
string copy = Directory.CreateDirectory(Path.Combine(output, "copy")).FullName;
await File.WriteAllTextAsync(Path.Combine(live, "value"), "host-initial", token);
await File.WriteAllTextAsync(Path.Combine(readOnly, "value"), "read-only", token);
await File.WriteAllTextAsync(Path.Combine(copy, "value"), "base", token);
await using (var machine = Create())
{
    machine.MountDirectory("/live", live);
    machine.MountDirectory("/ro", readOnly, MountAccess.ReadOnly);
    machine.MountDirectory("/cow", copy, MountAccess.CopyOnWrite);
    await Execute(machine, Command("load", "/live/value"), "host-initial");
    await File.WriteAllTextAsync(Path.Combine(live, "value"), "host-change", token);
    await Execute(machine, Command("load", "/live/value"), "host-change");
    await Execute(machine, Command("store", "/live/value", "guest-change"), "store-ok\n");
    Check(await File.ReadAllTextAsync(Path.Combine(live, "value"), token) == "guest-change", "live write not visible");
    await Execute(machine, Command("store", "/ro/value", "denied"), "", "errno=30\n", 1);
    Check(await File.ReadAllTextAsync(Path.Combine(readOnly, "value"), token) == "read-only", "read-only changed");
    await Execute(machine, Command("store", "/cow/value", "private-change"), "store-ok\n");
    Check(await File.ReadAllTextAsync(Path.Combine(copy, "value"), token) == "base", "COW base changed");
    await Execute(machine, Command("load", "/cow/value"), "private-change");
    string exported = Directory.CreateDirectory(Path.Combine(output, "exported")).FullName;
    machine.ExportPrivateStorage(exported, "/cow");
    Check(await File.ReadAllTextAsync(Path.Combine(exported, "value"), token) == "private-change", "COW export");
    machine.ResetPrivateStorage();
    Check(await File.ReadAllTextAsync(Path.Combine(live, "value"), token) == "guest-change", "reset changed live host");
    machine.ImportImage([new("/bin/probe", elf, Executable: true)]);
    await Execute(machine, Command("load", "/live/value"), "guest-change");
    evidence.Add("live-rw-ro-cow-export-reset");
}
Check(await File.ReadAllTextAsync(Path.Combine(live, "value"), token) == "guest-change", "dispose changed live host");
await using (var left = Create())
await using (var right = Create())
{
    var options = Command("wait") with { Console = new() { RedirectInput = true },
        Readiness = new() { Kind = ReadinessKind.OutputMarker, OutputMarker = "WAIT\n"u8.ToArray(), Timeout = TimeSpan.FromSeconds(10) } };
    await using var a = await left.StartAsync(options, token);
    await using var b = await right.StartAsync(options, token);
    await Task.WhenAll(a.Ready, b.Ready).WaitAsync(token);
    Check(!a.Completion.IsCompleted && !b.Completion.IsCompleted, "instances did not overlap");
    Check(mode == ExecutionMode.InProcess ? a.WorkerProcessId == null && b.WorkerProcessId == null : a.WorkerProcessId != b.WorkerProcessId, "mode/worker identity");
    bool busy = false; try { await left.StartAsync(Command("load", "/missing"), token); } catch (InvalidOperationException) { busy = true; }
    Check(busy, "concurrent run on same machine accepted");
    await a.StandardInput.WriteAsync("left"u8.ToArray(), token); a.StandardInput.Close();
    await b.StandardInput.WriteAsync("right"u8.ToArray(), token); b.StandardInput.Close();
    var pair = await Task.WhenAll(a.WaitAsync(token), b.WaitAsync(token));
    Check(pair.All(r => r.Reason == RunExitReason.Exited && r.ExitCode == 0 && r.ResourcesReleased), "concurrent cleanup");
    Check(Encoding.UTF8.GetString(pair[0].StandardOutput) == "WAIT\nleft" && Encoding.UTF8.GetString(pair[1].StandardOutput) == "WAIT\nright", "cross-instance console state");
    if (a.WorkerProcessId is int ap) processes.Add(ap); if (b.WorkerProcessId is int bp) processes.Add(bp);
    evidence.Add("concurrent-independent-machines-same-machine-exclusion");
}
await using (var machine = Create())
{
    var options = Command("wait") with { Console = new() { RedirectInput = true },
        Readiness = new() { Kind = ReadinessKind.OutputMarker, OutputMarker = "WAIT\n"u8.ToArray(), Timeout = TimeSpan.FromSeconds(10) } };
    var run = await machine.StartAsync(options, token);
    await run.Ready.WaitAsync(token);
    var stopped = run.StopAsync(TimeSpan.FromSeconds(10), token);
    Task one = run.DisposeAsync().AsTask(), two = run.DisposeAsync().AsTask(), owner = machine.DisposeAsync().AsTask();
    await Task.WhenAll(one, two, owner).WaitAsync(token);
    var result = await stopped;
    Check(result.Reason == RunExitReason.Stopped && result.ResourcesReleased && run.Completion.IsCompleted, "concurrent stop/dispose result");
    if (run.WorkerProcessId is int pid) processes.Add(pid);
    evidence.Add("concurrent-stop-run-and-machine-dispose");
}
await using (var machine = Create(Options() with { InstructionLimit = null, ExecutionDeadline = null }))
{
    var options = Command("store-spin", "/persist", "acknowledged") with {
        Readiness = new() { Kind = ReadinessKind.OutputMarker, OutputMarker = "SPIN\n"u8.ToArray(), Timeout = TimeSpan.FromSeconds(10) } };
    await using var run = await machine.StartAsync(options, token);
    await run.Ready.WaitAsync(token);
    using var waitCanceled = new CancellationTokenSource(); waitCanceled.Cancel();
    bool canceled = false; try { await run.WaitAsync(waitCanceled.Token); } catch (OperationCanceledException) { canceled = true; }
    Check(canceled && !run.Completion.IsCompleted, "wait cancellation terminated guest");
    if (mode == ExecutionMode.InProcess)
    {
        bool unsupported = false; try { await run.KillAsync(token); } catch (NotSupportedException) { unsupported = true; }
        Check(unsupported, "in-process kill was advertised");
        var stopped = await run.StopAsync(TimeSpan.FromSeconds(10), token);
        Check(stopped.Reason == RunExitReason.Stopped && stopped.ResourcesReleased, "cooperative stop");
    }
    else
    {
        var killedPair = await Task.WhenAll(run.KillAsync(token), run.KillAsync(token));
        var killed = killedPair[0];
        Check(killedPair.All(r => r.Reason == RunExitReason.Killed && r.ResourcesReleased), "concurrent explicit kill");
        Check(killed.Reason == RunExitReason.Killed && killed.ResourcesReleased, "explicit process kill");
    }
    if (run.WorkerProcessId is int pid) processes.Add(pid);
    await run.DisposeAsync();
    await Execute(machine, Command("load", "/persist"), "acknowledged");
    evidence.Add("wait-cancel-stop-or-kill-acknowledged-write-restart");
}
await using (var machine = Create(Options() with { InstructionLimit = null, ExecutionDeadline = TimeSpan.FromSeconds(2) }))
{
    var deadline = await machine.ExecuteAsync(Command("spin"), token);
    Check(deadline.Reason == RunExitReason.Deadline && deadline.ResourcesReleased && deadline.Instructions > 0 && Encoding.UTF8.GetString(deadline.StandardOutput) == "SPIN\n", "actual execution deadline");
    evidence.Add("natural-deadline");
}
await using (var machine = Create(Options() with { InstructionLimit = 10000 }))
{
    var budget = await machine.ExecuteAsync(Command("spin"), token);
    Check(budget.Reason == RunExitReason.InstructionLimit && budget.ResourcesReleased && budget.Instructions == 10000, "actual instruction bound");
    evidence.Add("instruction-bound");
}
Check(Environment.CurrentDirectory == cwd && ReferenceEquals(Console.In, consoleIn) && ReferenceEquals(Console.Out, consoleOut) && ReferenceEquals(Console.Error, consoleError), "host cwd or console replaced");
var afterEnvironment = Environment.GetEnvironmentVariables().Cast<System.Collections.DictionaryEntry>().ToDictionary(p => (string)p.Key, p => (string?)p.Value, StringComparer.Ordinal);
Check(environment.Count == afterEnvironment.Count && environment.All(p => afterEnvironment.GetValueOrDefault(p.Key) == p.Value), "host environment changed");
var summary = new Evidence(mode.ToString(), Environment.ProcessId, evidence.ToArray(), processes.Distinct().ToArray(), true);
await File.WriteAllTextAsync(Path.Combine(output, "observations.json"), JsonSerializer.Serialize(summary, EvidenceJson.Default.Evidence), token);
Console.WriteLine("machine-api " + mode + " " + evidence.Count + " PASS");
public sealed record Evidence(string Mode, int Controller, string[] Cases, int[] Workers, bool HostStatePreserved);
[JsonSerializable(typeof(Evidence))] internal partial class EvidenceJson : JsonSerializerContext;
