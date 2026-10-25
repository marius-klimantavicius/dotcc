using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Managed.Emulation.Execution;
using Managed.Emulation.Host;

if (args.Length != 3) return 2;
string scenario = args[1];
if (!new[] { "churn", "capacity", "join", "cancel-start", "cancel-clone", "blocked" }.Contains(scenario)) return 2;
byte[] image = File.ReadAllBytes(args[0]);
bool cancelling = scenario.StartsWith("cancel-") || scenario == "blocked";
int limit = scenario == "capacity" ? 3 : 2;
string guestArgument = scenario switch { "capacity" => "c", "join" => "j", "blocked" => "b", _ => "s" };
byte[] expected = Encoding.ASCII.GetBytes(scenario is "churn" or "capacity"
    ? "guest-thread-churn: 96 cleared, capacity recovered\n"
    : scenario == "blocked" ? "guest-thread-churn: cancel ready\n" : "");
var io = new InstanceIo(new Dictionary<string, ReadOnlyMemory<byte>> { ["/guest"] = image },
    executablePaths: new HashSet<string> { "/guest" });
var stop = new HostExecutionStop(TimeSpan.FromSeconds(20));
var owner = new ThreadedGuestExecution(io, stop, maximumWorkers: limit);
ThreadedGuestExecutionResult? result = null;
Exception? failure = null;
bool joined = false, disposed = false, cancelTriggered = false, historyOverflow = false, runOnce = false;
var observations = new List<ThreadedSyscallObservation>();
var thread = new Thread(() =>
{
    try
    {
        result = owner.Run("/guest", ["/guest", guestArgument], [], 20_000_000, observation =>
        {
            // The owner serializes observers; read the history after all joins.
            if (observations.Count < 100_000) observations.Add(observation);
            else historyOverflow = true;
            if ((scenario == "cancel-clone" && observation.Number == 56 && observation.ReturnValue is > 0) ||
                (scenario == "blocked" && observation.Number == 1 && observation.ReturnValue == (ulong)expected.Length))
            { cancelTriggered = true; stop.RequestStop(); }
        }, started: () =>
        {
            if (scenario == "cancel-start") { cancelTriggered = true; stop.RequestStop(); }
        });
    }
    catch (Exception error) { failure = error; }
}) { IsBackground = true, Name = "guest-churn-main" };
try
{
    thread.Start();
    joined = thread.Join(TimeSpan.FromSeconds(30));
    Require(joined, "Owner thread did not join; retain borrowed resources for process teardown");
    if (failure != null) ExceptionDispatchInfo.Capture(failure).Throw();
    Require(owner.IsQuiescent && result is { AllWorkersJoined: true, MemoryReleased: true }, "Owner did not quiesce");
    var value = result!;
    Require(!historyOverflow && stop.NotificationFailure == null, "Trace/stop notification failure");
    Require(value.Threads.All(t => t.MachineReleased && t.Halt == 0 && t.Signal == 0), "Worker cleanup or guest fault");
    Require(value.Threads.Sum(t => (decimal)t.Instructions) == value.Instructions, "Retired instruction history differs");
    var clones = observations.Where(o => o.Number == 56).ToArray();
    int successfulClones = clones.Count(o => o.ReturnValue is > 0 and <= int.MaxValue);
    Require(value.Threads.Count == successfulClones + 1, "Result history lost an admitted child");
    Require(clones.All(o => o.ReturnValue is > 0 and <= int.MaxValue ||
        o.ReturnValue == unchecked((ulong)-11L)), "Unexpected clone result");
    if (cancelling)
    {
        Require(cancelTriggered && !value.Exited && value.StopReason == HostExecutionStopReason.Requested &&
            stop.Reason == HostExecutionStopReason.Requested, "Ordinary cancellation outcome differs");
        Require(value.Threads.Count == (scenario == "cancel-start" ? 1 : 2), "Cancelled startup history differs");
    }
    else
    {
        Require(value.Exited && value.ExitStatus == 0 && value.StopReason == HostExecutionStopReason.None &&
            stop.Reason == HostExecutionStopReason.None, "Guest did not complete normally");
        int children = scenario == "capacity" ? 98 : scenario == "join" ? 1 : 96;
        Require(successfulClones == children && value.Threads.Count == children + 1, "Churn did not exceed lifetime limit");
        Require(value.Threads.Skip(1).All(t => t.Termination == "ThreadExit" && t.Status == 0 && t.Instructions > 0),
            "Child result history differs");
        Require(value.Threads[0].Termination == (scenario == "join" ? "ThreadExit" : "GroupExit") &&
            value.Threads[0].Status == (scenario == "join" ? 7 : 0), "Main exit history differs");
        if (scenario == "capacity")
        {
            Require(clones.Length >= 99 && clones[0].ReturnValue is > 0 && clones[1].ReturnValue is > 0 &&
                clones[2].ReturnValue == unchecked((ulong)-11L) &&
                clones.Skip(3).Any(o => o.ReturnValue is > 0 and <= int.MaxValue), "Live cap rejection/recovery missing");
        }
        if (scenario != "join")
            Require(observations.Any(o => o.Number == 202 && o.Argument2 == 0), "Clear-TID futex wait missing");
    }
    Require(io.CapturedOutput.StandardOutput.AsSpan().SequenceEqual(expected) &&
        io.CapturedOutput.StandardError.Length == 0, "Guest output differs from native witness");
    try { owner.Run("/guest", ["/guest"], [], 1); }
    catch (InvalidOperationException error) when (error.Message.StartsWith("Each execution owner runs once;")) { runOnce = true; }
    Require(runOnce, "Execution owner was reusable after disposal");
    await io.DisposeAsync();
    stop.Dispose(); disposed = true;
    Require(io.OpenDescriptors == 0 && io.PendingPipeOperations == 0 && stop.NotificationFailure == null,
        "Private IO or notification resources remain");
    WriteReport(true);
    return 0;
}
catch (Exception error)
{
    WriteReport(false, error);
    Console.Error.WriteLine(error);
    return 1;
}
finally
{
    // IO/stop belong to the execution until every owner thread has quiesced.
    if (!disposed && joined && owner.IsQuiescent) { await io.DisposeAsync(); stop.Dispose(); }
}

void WriteReport(bool passed, Exception? error = null)
{
    using var stream = File.Create(args[2]);
    using var report = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
    report.WriteStartObject();
    report.WriteBoolean("passed", passed);
    report.WriteString("scenario", scenario);
    report.WriteNumber("limit", limit);
    report.WriteString("image_sha256", Convert.ToHexStringLower(SHA256.HashData(image)));
    report.WriteBoolean("joined", joined);
    report.WriteBoolean("is_quiescent", owner.IsQuiescent);
    report.WriteBoolean("resources_disposed", disposed);
    report.WriteBoolean("run_once", runOnce);
    report.WriteBoolean("cancel_triggered", cancelTriggered);
    if (error != null) report.WriteString("error", error.ToString());
    if (joined && owner.IsQuiescent)
    {
        report.WriteString("stdout_hex", Convert.ToHexStringLower(io.CapturedOutput.StandardOutput));
        report.WriteNumber("successful_clones", observations.Count(o => o.Number == 56 && o.ReturnValue is > 0 and <= int.MaxValue));
        report.WriteNumber("rejected_clones", observations.Count(o => o.Number == 56 && o.ReturnValue == unchecked((ulong)-11L)));
        report.WritePropertyName("result");
        JsonSerializer.Serialize(report, result, ProbeJson.Default.ThreadedGuestExecutionResult);
        if (!passed)
        {
            report.WritePropertyName("last_syscalls");
            JsonSerializer.Serialize(report, observations.TakeLast(30).ToArray(), ProbeJson.Default.ThreadedSyscallObservationArray);
        }
    }
    report.WriteEndObject();
}

static void Require(bool condition, string message)
{ if (!condition) throw new InvalidOperationException(message); }

[JsonSerializable(typeof(ThreadedGuestExecutionResult))]
[JsonSerializable(typeof(ThreadedSyscallObservation[]))]
internal partial class ProbeJson : JsonSerializerContext;
