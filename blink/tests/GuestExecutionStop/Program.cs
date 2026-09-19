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
    await io.DisposeAsync();
    Require(io.OpenDescriptors == 0 && io.PendingPipeOperations == 0 && io.PipeBytes == 0,
        "Private I/O did not drain on disposal");
    File.WriteAllText(reportFile, JsonSerializer.Serialize(new
    {
        fixture, action, image_sha256 = imageHash, wait_seen = waitSeen,
        wait_seconds = waitSeconds, requested_seconds = requestedSeconds, elapsed_seconds = elapsed,
        pending_at_barrier = pendingAtBarrier, instructions_at_barrier = progressAtBarrier,
        pending_after_run = 0, descriptors_before_dispose = descriptorsBeforeDispose,
        pipe_bytes_before_dispose = pipeBytesBeforeDispose,
        pending_after_dispose = io.PendingPipeOperations, descriptors_after_dispose = io.OpenDescriptors,
        pipe_bytes_after_dispose = io.PipeBytes,
        stdout_hex = Convert.ToHexStringLower(output.StandardOutput), stderr_hex = "",
        first_reason_preserved = action == "complete" ? (bool?)null : true,
        notification_failure = false, result = value
    }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
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
