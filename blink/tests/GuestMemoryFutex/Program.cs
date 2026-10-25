using System.Security.Cryptography;
using System.Text.Json;
using Managed.Emulation.Execution;
using Managed.Emulation.Host;

if (args.Length != 2) return 2;
byte[] image = File.ReadAllBytes(args[0]);
var files = new Dictionary<string, ReadOnlyMemory<byte>>
{
    ["/guest"] = image,
    ["/advice-data"] = Enumerable.Repeat((byte)99, 4096).ToArray()
};
var io = new InstanceIo(files, executablePaths: new HashSet<string> { "/guest" });
using var stop = new HostExecutionStop(TimeSpan.FromSeconds(15));
var owner = new ThreadedGuestExecution(io, stop);
ThreadedGuestExecutionResult? result = null;
Exception? failure = null;
var thread = new Thread(() =>
{
    try { result = owner.Run("/guest", ["/guest"], [], 10_000_000); }
    catch (Exception error) { failure = error; }
}) { IsBackground = true };
thread.Start();
if (!thread.Join(TimeSpan.FromSeconds(25))) throw new Exception("Guest owner did not join");
if (failure != null) throw failure;
if (result is not { Exited: true, ExitStatus: 0, AllWorkersJoined: true, MemoryReleased: true } ||
    result.StopReason != HostExecutionStopReason.None || !owner.IsQuiescent ||
    result.Threads.Any(t => !t.MachineReleased || t.Status != 0 || t.Signal != 0 || t.Halt != 0))
    throw new Exception("Guest syscall or cleanup regression: " + result);
if (io.CapturedOutput.StandardOutput.Length != 0 || io.CapturedOutput.StandardError.Length != 0)
    throw new Exception("Unexpected guest output");
await io.DisposeAsync();
if (io.OpenDescriptors != 0 || io.PendingPipeOperations != 0 || stop.NotificationFailure != null)
    throw new Exception("Host operations did not drain");
using (var file = File.Create(args[1]))
using (var report = new Utf8JsonWriter(file, new JsonWriterOptions { Indented = true }))
{
    report.WriteStartObject();
    report.WriteBoolean("passed", true);
    report.WriteString("image_sha256", Convert.ToHexStringLower(SHA256.HashData(image)));
    report.WriteNumber("ExitStatus", result.ExitStatus);
    report.WriteBoolean("AllWorkersJoined", result.AllWorkersJoined);
    report.WriteBoolean("MemoryReleased", result.MemoryReleased);
    report.WriteNumber("thread_count", result.Threads.Count);
    report.WriteBoolean("IsQuiescent", owner.IsQuiescent);
    report.WriteEndObject();
}
return 0;
