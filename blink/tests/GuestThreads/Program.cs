using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Managed.Emulation.Execution;
using Managed.Emulation.Host;

if (args.Length != 2) return 2;
byte[] image = File.ReadAllBytes(args[0]);
const string path = "/guest-threads";
byte[] expected = Encoding.ASCII.GetBytes("guest-threads: tls=isolated shared=42 tid=cleared\n");
var io = new InstanceIo(new Dictionary<string, ReadOnlyMemory<byte>> { [path] = image },
    executablePaths: new HashSet<string> { path });
var stop = new HostExecutionStop(TimeSpan.FromSeconds(20));
var owner = new ThreadedGuestExecution(io, stop);
var observations = new List<ThreadedSyscallObservation>();
ThreadedGuestExecutionResult? result = null;
Exception? failure = null;
bool traceOverflow = false, joined = false, resourcesDisposed = false;
long started = Stopwatch.GetTimestamp();
var worker = new Thread(() =>
{
    try
    {
        result = owner.Run(path, [path], [], 1_000_000, observation =>
        {
            // Owner serializes observer calls. Read only after the actual joins.
            if (observations.Count < 4096) observations.Add(observation);
            else traceOverflow = true;
        });
    }
    catch (Exception error) { failure = error; }
}) { IsBackground = true, Name = "guest-threads-main" };
try
{
    worker.Start();
    joined = worker.Join(TimeSpan.FromSeconds(30));
    Require(joined, "Main owner thread did not join; discard process without disposing borrowed resources");
    if (failure != null) ExceptionDispatchInfo.Capture(failure).Throw();
    Require(result != null && owner.IsQuiescent, "Owner did not become quiescent");
    var value = result!;
    Require(!traceOverflow && stop.NotificationFailure == null, "Observer/notification failure");
    Require(value.Exited && value.ExitStatus == 0 && value.StopReason == HostExecutionStopReason.None &&
        stop.Reason == HostExecutionStopReason.None, "Guest did not complete normally");
    Require(value.AllWorkersJoined && value.MemoryReleased, "Guest workers or memory remain owned");
    Require(value.Threads.Count == 2 && value.Threads.All(t => t.GuestThreadId > 0 && t.MachineReleased &&
        t.Status == 0 && t.Halt == 0 && t.Signal == 0 && t.Instructions > 0), "Thread state differs");
    Require(value.Threads.Select(t => t.GuestThreadId).Distinct().Count() == 2 &&
        value.Threads.Count(t => t.Termination == "ThreadExit") == 1 &&
        value.Threads.Count(t => t.Termination == "GroupExit") == 1, "Thread/group exit relationship differs");
    var clone = observations.Where(o => o.Number == 56).ToArray();
    Require(clone.Length == 1 && clone[0].Argument1 == 0x7d0f00 && clone[0].ReturnValue is > 0 &&
        clone[0].Argument3 != 0 && clone[0].Argument3 == clone[0].Argument4 && clone[0].Argument5 != 0,
        "Observed clone ABI differs");
    Require(value.Threads.Any(t => (ulong)t.GuestThreadId == clone[0].ReturnValue &&
        t.GuestThreadId != clone[0].GuestThreadId), "Clone return does not identify the child");
    foreach (ulong operation in new ulong[] { 0, 128, 129 })
        Require(observations.Any(o => o.Number == 202 && o.Argument2 == operation), "Futex operation missing");
    Require(observations.Count(o => o.Number == 60) == 1 && observations.Count(o => o.Number == 231) == 1,
        "Ordinary exit syscalls missing");
    var output = io.CapturedOutput;
    Require(output.StandardOutput.AsSpan().SequenceEqual(expected) && output.StandardError.Length == 0,
        "Guest output differs");
    Require(io.PendingPipeOperations == 0, "Pending private pipe operation remains");
    int descriptorsBeforeDispose = io.OpenDescriptors;
    await io.DisposeAsync();
    Require(io.OpenDescriptors == 0 && io.PendingPipeOperations == 0, "Private IO did not drain");
    stop.Dispose();
    resourcesDisposed = true;
    using (var stream = File.Create(args[1]))
    using (var report = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
    {
        report.WriteStartObject();
        report.WriteBoolean("passed", true);
        report.WriteString("image_sha256", Convert.ToHexStringLower(SHA256.HashData(image)));
        report.WriteBoolean("owner_joined", joined);
        report.WriteBoolean("is_quiescent", owner.IsQuiescent);
        report.WriteBoolean("all_workers_joined", value.AllWorkersJoined);
        report.WriteBoolean("memory_released", value.MemoryReleased);
        report.WriteBoolean("notification_failure", false);
        report.WriteBoolean("trace_overflow", traceOverflow);
        report.WriteBoolean("exited", value.Exited);
        report.WriteNumber("exit_status", value.ExitStatus);
        report.WriteString("stop_reason", value.StopReason.ToString());
        report.WriteNumber("instructions", value.Instructions);
        report.WriteNumber("elapsed_seconds", Stopwatch.GetElapsedTime(started).TotalSeconds);
        report.WriteNumber("retained_bytes_before_release", value.RetainedBytesBeforeRelease);
        report.WriteNumber("retained_mappings_before_release", value.RetainedMappingsBeforeRelease);
        report.WriteNumber("descriptors_before_dispose", descriptorsBeforeDispose);
        report.WriteNumber("descriptors_after_dispose", io.OpenDescriptors);
        report.WriteNumber("pending_after_dispose", io.PendingPipeOperations);
        report.WriteString("stdout_hex", Convert.ToHexStringLower(output.StandardOutput));
        report.WriteString("stderr_hex", Convert.ToHexStringLower(output.StandardError));
        report.WriteStartArray("threads");
        foreach (var thread in value.Threads)
        {
            report.WriteStartObject();
            report.WriteNumber("tid", thread.GuestThreadId);
            report.WriteNumber("instructions", thread.Instructions);
            report.WriteNumber("ip", thread.InstructionPointer);
            report.WriteString("termination", thread.Termination);
            report.WriteNumber("status", thread.Status);
            report.WriteNumber("halt", thread.Halt);
            report.WriteNumber("signal", thread.Signal);
            report.WriteBoolean("machine_released", thread.MachineReleased);
            report.WriteEndObject();
        }
        report.WriteEndArray();
        report.WriteStartArray("syscalls");
        foreach (var observation in observations)
        {
            report.WriteStartObject();
            report.WriteNumber("tid", observation.GuestThreadId);
            report.WriteNumber("ip", observation.InstructionPointer);
            report.WriteNumber("number", observation.Number);
            report.WriteStartArray("arguments");
            foreach (ulong argument in new[] { observation.Argument1, observation.Argument2, observation.Argument3,
                observation.Argument4, observation.Argument5, observation.Argument6 }) report.WriteNumberValue(argument);
            report.WriteEndArray();
            if (observation.ReturnValue is { } returned) report.WriteNumber("return_value", returned);
            else report.WriteNull("return_value");
            report.WriteEndObject();
        }
        report.WriteEndArray();
        report.WriteEndObject();
    }
    using var stdout = Console.OpenStandardOutput();
    stdout.Write(output.StandardOutput);
    return 0;
}
catch (Exception error)
{
    // A failed qualification still retains real observations. The observer list
    // can be written by a child after the main thread returns unsuccessfully,
    // so never enumerate it unless every owner thread is quiescent.
    try
    {
        bool safeToInspect = joined && owner.IsQuiescent;
        var captured = io.CapturedOutput; // synchronized copies of private bytes
        File.WriteAllBytes(args[1] + ".guest.stdout", captured.StandardOutput);
        File.WriteAllBytes(args[1] + ".guest.stderr", captured.StandardError);
        using var stream = File.Create(args[1]);
        using var report = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
        report.WriteStartObject();
        report.WriteBoolean("passed", false);
        report.WriteString("exception", error.ToString());
        report.WriteString("image_sha256", Convert.ToHexStringLower(SHA256.HashData(image)));
        report.WriteBoolean("owner_joined", joined);
        report.WriteBoolean("is_quiescent", owner.IsQuiescent);
        report.WriteNumber("instructions_observed", owner.InstructionsCompleted);
        report.WriteString("stop_reason", stop.Reason.ToString());
        report.WriteNumber("elapsed_seconds", Stopwatch.GetElapsedTime(started).TotalSeconds);
        report.WriteString("notification_failure", stop.NotificationFailure?.ToString());
        report.WriteString("stdout_hex", Convert.ToHexStringLower(captured.StandardOutput));
        report.WriteString("stderr_hex", Convert.ToHexStringLower(captured.StandardError));
        report.WriteBoolean("observations_safe_to_inspect", safeToInspect);
        if (safeToInspect)
        {
            report.WriteBoolean("trace_overflow", traceOverflow);
            if (result is { } value)
            {
                report.WriteStartObject("result");
                report.WriteBoolean("exited", value.Exited);
                report.WriteNumber("exit_status", value.ExitStatus);
                report.WriteNumber("instructions", value.Instructions);
                report.WriteString("stop_reason", value.StopReason.ToString());
                report.WriteBoolean("all_workers_joined", value.AllWorkersJoined);
                report.WriteBoolean("memory_released", value.MemoryReleased);
                report.WriteNumber("retained_bytes_before_release", value.RetainedBytesBeforeRelease);
                report.WriteNumber("retained_mappings_before_release", value.RetainedMappingsBeforeRelease);
                report.WriteStartArray("threads");
                foreach (var thread in value.Threads)
                {
                    report.WriteStartObject();
                    report.WriteNumber("tid", thread.GuestThreadId);
                    report.WriteNumber("instructions", thread.Instructions);
                    report.WriteNumber("ip", thread.InstructionPointer);
                    report.WriteString("termination", thread.Termination);
                    report.WriteNumber("status", thread.Status);
                    report.WriteNumber("halt", thread.Halt);
                    report.WriteNumber("signal", thread.Signal);
                    report.WriteBoolean("machine_released", thread.MachineReleased);
                    report.WriteEndObject();
                }
                report.WriteEndArray();
                report.WriteEndObject();
            }
            else report.WriteNull("result");
            report.WriteStartArray("syscalls");
            foreach (var observation in observations)
            {
                report.WriteStartObject();
                report.WriteNumber("tid", observation.GuestThreadId);
                report.WriteNumber("ip", observation.InstructionPointer);
                report.WriteNumber("number", observation.Number);
                report.WriteStartArray("arguments");
                foreach (ulong argument in new[] { observation.Argument1, observation.Argument2,
                    observation.Argument3, observation.Argument4, observation.Argument5, observation.Argument6 })
                    report.WriteNumberValue(argument);
                report.WriteEndArray();
                if (observation.ReturnValue is { } returned) report.WriteNumber("return_value", returned);
                else report.WriteNull("return_value");
                report.WriteEndObject();
            }
            report.WriteEndArray();
        }
        else
        {
            report.WriteNull("result");
            report.WriteNull("syscalls");
        }
        report.WriteEndObject();
    }
    catch (Exception diagnosticError)
    {
        Console.Error.WriteLine("Failure report writing failed: " + diagnosticError);
    }
    Console.Error.WriteLine(error);
    return 1;
}
finally
{
    // Never tear down borrowed resources while any guest worker might use them.
    // A failed join/non-quiescent result requires the surrounding process to end.
    if (joined && owner.IsQuiescent && !resourcesDisposed)
    {
        await io.DisposeAsync();
        stop.Dispose();
    }
}
static void Require(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}
