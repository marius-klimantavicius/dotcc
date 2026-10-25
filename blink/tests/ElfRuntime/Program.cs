using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Managed.Emulation;

if (args.Length != 6 || !Enum.TryParse<ExecutionMode>(args[0], out var mode))
    throw new ArgumentException("usage: Probe InProcess|SeparateProcess native-images fixture scenario interpreter report.json");
string images = Path.GetFullPath(args[1]), fixture = args[2], scenario = args[3], interpreter = args[4];
bool dynamic = fixture == "glibc-dynamic", entry = !fixture.StartsWith("glibc-", StringComparison.Ordinal);
int? worker = null;
MachineRunResult result;
string? written = null;
await using (var machine = new BlinkMachine(new MachineOptions { ExecutionMode = mode,
    InstructionLimit = 20_000_000, ExecutionDeadline = TimeSpan.FromSeconds(15) }))
{
    var files = new List<ImageFile> { new("/bin/app", File.ReadAllBytes(Path.Combine(images, "bin", fixture)), true),
        new("/work/input.txt", Encoding.ASCII.GetBytes("input")) };
    if (dynamic)
    {
        if (scenario != "missing-interpreter" && scenario != "directory-interpreter")
            files.Add(new(interpreter, File.ReadAllBytes(Path.Combine(images, interpreter.TrimStart('/'))), true));
        if (scenario == "directory-interpreter") files.Add(new(interpreter + "/marker", []));
        foreach (string library in new[] { "libelf-runtime.so", "libc.so.6" })
            if (scenario != "missing-libc" || library != "libc.so.6")
                files.Add(new("/lib/" + library, File.ReadAllBytes(Path.Combine(images, "lib", library))));
    }
    machine.ImportImage(files);
    result = await machine.ExecuteAsync(new ExecutionOptions { Executable = "/bin/app", Arguments = ["fixture-argument"],
        WorkingDirectory = "/work", AllowElfInterpreter = dynamic && scenario != "disabled" });
    worker = machine.CurrentRun?.WorkerProcessId;
    if (!entry && scenario == "run")
    {
        string exported = args[5] + ".files";
        Directory.CreateDirectory(exported);
        machine.ExportPrivateStorage(exported);
        written = File.ReadAllText(Path.Combine(exported, "work/output.txt"));
    }
}
bool workerGone = true;
if (worker is int pid)
{
    try { using var process = Process.GetProcessById(pid); workerGone = process.HasExited; }
    catch (ArgumentException) { }
}
bool denied = scenario is "disabled" or "missing-interpreter" or "directory-interpreter";
bool passed = result.ResourcesReleased && workerGone &&
    (mode == ExecutionMode.InProcess ? worker == null : worker != null) &&
    (denied ? result.Reason == RunExitReason.ExecutionFailure && result.Instructions == 0 &&
        result.StandardOutput.Length == 0 && result.StandardError.Length == 0 &&
        result.Diagnostic?.Contains(scenario == "disabled" ? "AllowElfInterpreter=true" : "guest filesystem", StringComparison.Ordinal) == true :
     scenario == "missing-libc" ? result.Reason == RunExitReason.Exited && result.ExitCode == 127 && result.Instructions > 0 &&
        result.StandardOutput.Length == 0 && Encoding.UTF8.GetString(result.StandardError).Contains("libc.so.6", StringComparison.Ordinal) :
     result.Reason == RunExitReason.Exited && result.ExitCode == 0 && result.Instructions > 0 && (entry || written == "file-write OK\n"));
var report = new Report(passed, mode.ToString(), fixture, scenario, worker, workerGone, written, result);
File.WriteAllText(args[5], JsonSerializer.Serialize(report, EvidenceJson.Default.Report));
Console.WriteLine($"elf-runtime {mode} {fixture} {scenario} {(passed ? "PASS" : "FAIL")}");
return passed ? 0 : 1;

internal sealed record Report(bool Passed, string Mode, string Fixture, string Scenario, int? Worker,
    bool WorkerGone, string? Written, MachineRunResult Result);
[JsonSerializable(typeof(Report))]
internal partial class EvidenceJson : JsonSerializerContext;
