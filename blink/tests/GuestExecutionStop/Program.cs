using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Managed.Emulation.Execution;
using Managed.Emulation.Host;

if (args.Length != 4) return 2;
string imageFile = args[0], fixture = args[1], action = args[2], reportFile = args[3];
if (!(fixture is "cpu" or "poll" or "sleep" or "pipe") ||
    !(action is "complete" or "request" or "deadline" or "budget")) return 2;
byte[] image = File.ReadAllBytes(imageFile);
string imageHash = Convert.ToHexStringLower(SHA256.HashData(image));
const string guestPath = "/stop-fixture";
var io = new InstanceIo(new Dictionary<string, ReadOnlyMemory<byte>> { [guestPath] = image },
    executablePaths: new HashSet<string> { guestPath });
int? inheritedPipeWriter = null;
if (fixture == "pipe")
{
    var pair = io.Pipe();
    Require(pair.Succeeded, "Create inherited stdin pipe");
    var duplicate = io.DuplicateTo(pair.Value.Read, 0);
    Require(duplicate.Succeeded && duplicate.Value == 0, "Install inherited stdin pipe");
    Require(io.Close(pair.Value.Read).Succeeded, "Close redundant pipe read descriptor");
    inheritedPipeWriter = pair.Value.Write;
    if (action == "complete")
    {
        var written = await io.WriteAsync(pair.Value.Write, new byte[] { 0x5a });
        Require(written.Succeeded && written.Value == 1, "Supply ordinary stdin byte");
    }
    // Keep the real writer open until InstanceIo disposal. A requested stop
    // must cancel a pending read, not observe EOF from a closed writer.
}
using var stop = new HostExecutionStop(action == "deadline" ? TimeSpan.FromSeconds(5) : null);
var owner = new GuestExecution(io, stop);
GuestExecutionResult? result = null;
Exception? failure = null;
long started = Stopwatch.GetTimestamp();
bool waitSeen = false;
double? waitSeconds = null, requestedSeconds = null;
int pendingAtBarrier = 0;
ulong progressAtBarrier = 0;
using var finished = new ManualResetEventSlim();
var worker = new Thread(() =>
{
    try
    {
        GC.Collect(2, GCCollectionMode.Forced, true, true);
        result = owner.Run(guestPath, action == "complete" ? [guestPath] : [guestPath, "wait"],
            [], action == "budget" ? 128UL : ulong.MaxValue);
    }
    catch (Exception error) { failure = error; }
    finally { finished.Set(); }
});
worker.Start();
try
{
    if (action is "request" or "deadline")
    {
        while (!finished.IsSet && Stopwatch.GetElapsedTime(started) < TimeSpan.FromSeconds(10))
        {
            progressAtBarrier = owner.InstructionsCompleted;
            pendingAtBarrier = io.PendingPipeOperations;
            bool ready = fixture == "cpu" ? progressAtBarrier >= 1024
                : fixture == "pipe" ? pendingAtBarrier == 1 : owner.IsSleeping;
            if (ready)
            {
                waitSeen = true; waitSeconds = Stopwatch.GetElapsedTime(started).TotalSeconds;
                break;
            }
            Thread.Sleep(1);
        }
        Require(waitSeen, "Execution did not reach its real progress/wait barrier");
        if (action == "request")
        {
            requestedSeconds = Stopwatch.GetElapsedTime(started).TotalSeconds;
            Require(stop.RequestStop(), "Request must latch the first stop reason");
        }
    }
    Require(finished.Wait(TimeSpan.FromSeconds(20)), "Cooperative execution did not return");
    worker.Join();
    if (failure != null) ExceptionDispatchInfo.Capture(failure).Throw();
    Require(result != null && stop.NotificationFailure == null, "Owner/notification failure");
    var value = result!;
    double elapsed = Stopwatch.GetElapsedTime(started).TotalSeconds;
    HostExecutionStopReason expected = action switch
    {
        "complete" => HostExecutionStopReason.None,
        "request" => HostExecutionStopReason.Requested,
        "deadline" => HostExecutionStopReason.Deadline,
        _ => HostExecutionStopReason.Budget
    };
    Require(value.StopReason == expected && stop.Reason == expected, "Stop reason differs");
    Require(value.Signal == 0 && value.SignalCode == 0, "Stop must not fabricate a guest signal");
    Require(value.MemoryReleased, "Owner did not complete memory release");
    Require(io.PendingPipeOperations == 0, "An operation remains pending after Run returned");
    var output = io.CapturedOutput;
    Require(output.StandardError.Length == 0, "Unexpected guest stderr");
    byte[] expectedOutput = action == "complete" ? Encoding.ASCII.GetBytes("ok:" + fixture + "\n") : [];
    Require(output.StandardOutput.AsSpan().SequenceEqual(expectedOutput), "Guest output differs");
    if (action == "complete")
        Require(value.Exited && value.ExitStatus == 0 && value.Halt == -10, "Normal guest did not exit successfully");
    else
    {
        Require(!value.Exited && value.Halt == 0, "Stop was misreported as a guest exit or halt");
        Require(!stop.RequestBudgetStop() && !stop.RequestStop() && stop.Reason == expected,
            "First stop reason was not preserved");
    }
    if (action == "budget") Require(value.Instructions == 128, "Instruction budget differs");
    if (action == "deadline") Require(elapsed >= 4.9 && waitSeconds < 5,
        "Deadline did not expire after an observed pending wait");
    int descriptorsBeforeDispose = io.OpenDescriptors;
    long pipeBytesBeforeDispose = io.PipeBytes;
    bool pipeWriterOpen = inheritedPipeWriter is { } writerFd && io.GetDescriptorFlags(writerFd).Succeeded;
    if (fixture == "pipe") Require(pipeWriterOpen, "Inherited pipe writer closed before disposal");
    await io.DisposeAsync();
    Require(io.OpenDescriptors == 0 && io.PendingPipeOperations == 0 && io.PipeBytes == 0,
        "Private I/O did not drain on disposal");
    // Explicit writes preserve the reviewed schema without reflection metadata,
    // including when NativeAOT disables JsonSerializer's reflection fallback.
    using var reportStream = File.Create(reportFile);
    using (var report = new Utf8JsonWriter(reportStream, new JsonWriterOptions { Indented = true }))
    {
        report.WriteStartObject();
        report.WriteString("fixture", fixture);
        report.WriteString("action", action);
        report.WriteString("image_sha256", imageHash);
        report.WriteBoolean("wait_seen", waitSeen);
        if (waitSeconds is { } waited) report.WriteNumber("wait_seconds", waited);
        else report.WriteNull("wait_seconds");
        if (requestedSeconds is { } requested) report.WriteNumber("requested_seconds", requested);
        else report.WriteNull("requested_seconds");
        report.WriteNumber("elapsed_seconds", elapsed);
        report.WriteNumber("pending_at_barrier", pendingAtBarrier);
        report.WriteNumber("instructions_at_barrier", progressAtBarrier);
        report.WriteNumber("pending_after_run", 0);
        report.WriteNumber("descriptors_before_dispose", descriptorsBeforeDispose);
        report.WriteNumber("pipe_bytes_before_dispose", pipeBytesBeforeDispose);
        report.WriteBoolean("inherited_stdin_pipe", fixture == "pipe");
        report.WriteNumber("stdin_pipe_preloaded_bytes", fixture == "pipe" && action == "complete" ? 1 : 0);
        report.WriteBoolean("stdin_pipe_writer_open_after_run", pipeWriterOpen);
        report.WriteNumber("pending_after_dispose", io.PendingPipeOperations);
        report.WriteNumber("descriptors_after_dispose", io.OpenDescriptors);
        report.WriteNumber("pipe_bytes_after_dispose", io.PipeBytes);
        report.WriteString("stdout_hex", Convert.ToHexStringLower(output.StandardOutput));
        report.WriteString("stderr_hex", "");
        if (action == "complete") report.WriteNull("first_reason_preserved");
        else report.WriteBoolean("first_reason_preserved", true);
        report.WriteBoolean("notification_failure", false);
        report.WriteStartObject("result");
        report.WriteNumber("Instructions", value.Instructions);
        report.WriteNumber("InstructionPointer", value.InstructionPointer);
        report.WriteNumber("Halt", value.Halt);
        report.WriteNumber("Signal", value.Signal);
        report.WriteNumber("SignalCode", value.SignalCode);
        report.WriteBoolean("Exited", value.Exited);
        report.WriteNumber("ExitStatus", value.ExitStatus);
        report.WriteNumber("StopReason", (int)value.StopReason);
        report.WriteNumber("RetainedBytesBeforeRelease", value.RetainedBytesBeforeRelease);
        report.WriteNumber("RetainedMappingsBeforeRelease", value.RetainedMappingsBeforeRelease);
        report.WriteBoolean("MemoryReleased", value.MemoryReleased);
        report.WriteEndObject();
        report.WriteEndObject();
        report.Flush();
    }
    reportStream.WriteByte((byte)'\n');
    return 0;
}
finally
{
    // A harness failure still requests ordinary stop and joins before teardown.
    // No disposal races a live translated call; runner timeout is a separate
    // failed attempt and never counts as cancellation qualification.
    if (worker.IsAlive) { stop.RequestStop(); worker.Join(); }
    await io.DisposeAsync();
}
static void Require(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}
