using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using Managed.Emulation.Execution;
using Managed.Emulation.Host;

namespace Managed.Emulation.Worker;

internal static class Program
{
    private sealed class TraceRow
    {
        internal ulong Count, Number;
        internal ulong? Returned;
    }

    public static async Task<int> Main(string[] args)
    {
        if (args.Length == 1 && args[0] == "--machine-api-v1")
        {
            // Terminal Ctrl+C can reach the entire process group. The controller
            // owns interrupt policy and sends a protocol stop; do not terminate
            // this worker before it drains the guest and returns its final result.
            ConsoleCancelEventHandler interrupt = (_, args) => args.Cancel = true;
            Console.CancelKeyPress += interrupt;
            try
            {
                using Stream protocolOutput = Console.OpenStandardOutput();
                using Stream protocolInput = Console.OpenStandardInput();
                Console.SetOut(Console.Error);
                return await MachineWorkerHost.RunAsync(protocolInput, protocolOutput);
            }
            finally { Console.CancelKeyPress -= interrupt; }
        }
        // Capture raw streams before redirecting any generic translated Console
        // diagnostic. Guest descriptors use only InstanceIo's private captures.
        using Stream output = Console.OpenStandardOutput();
        using Stream input = Console.OpenStandardInput();
        Console.SetOut(Console.Error);
        using var receiveLifetime = new CancellationTokenSource();
        InstanceIo? io = null;
        HostExecutionStop? stop = null;
        ThreadedGuestExecution? owner = null;
        Thread? execution = null;
        ThreadedGuestExecutionResult? result = null;
        Exception? executionError = null;
        string? failure = null;
        bool joined = false, ready = false, ioDisposed = false, notificationFailure = false;
        var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        object traceGate = new();
        var traces = new Dictionary<int, TraceRow>();
        ulong unrecorded = 0;
        byte[] stdout = [], stderr = [];
        InstanceOptions? options = null;
        Task<WorkerRequest?>? request = null;
        bool receiverDrained = true;
        try
        {
            WorkerRequest? start = await InstanceProtocol.ReadAsync(input, InstanceJson.Default.WorkerRequest);
            if (start is not { Kind: "start", Options: not null })
                throw new InvalidDataException("First request must start one guest.");
            options = WorkerProfile.Snapshot(start.Options);
            var image = options.Image.ToDictionary(file => file.Path, file => (ReadOnlyMemory<byte>)file.Contents, StringComparer.Ordinal);
            io = new InstanceIo(image, descriptorLimit: options.DescriptorLimit, outputLimit: options.OutputLimit,
                imageLimit: WorkerProfile.ImageBytes, writableLimit: 1024 * 1024, inputLimit: 0,
                executablePaths: options.Image.Where(file => file.Executable).Select(file => file.Path).ToHashSet(StringComparer.Ordinal),
                pipeCapacity: 65536, pipeByteLimit: 1024 * 1024, pipeOperationLimit: 128);
            // Best effort cooperative window only. The controller's earlier,
            // independent hard deadline is authoritative process containment.
            stop = new HostExecutionStop(TimeSpan.FromMilliseconds(options.WallClockMilliseconds - WorkerProfile.ShutdownReserveMilliseconds));
            owner = new ThreadedGuestExecution(io, stop, (ulong)options.MemoryLimit);
            execution = new Thread(() =>
            {
                try
                {
                    result = owner.Run(options.Executable, options.Arguments, options.Environment,
                        (ulong)options.InstructionLimit, observation =>
                        {
                            lock (traceGate)
                            {
                                if (!traces.TryGetValue(observation.GuestThreadId, out var row))
                                {
                                    if (traces.Count == WorkerProfile.MaximumGuestWorkers) { ++unrecorded; return; }
                                    traces.Add(observation.GuestThreadId, row = new());
                                }
                                ++row.Count; row.Number = observation.Number; row.Returned = observation.ReturnValue;
                            }
                        });
                }
                catch (Exception error) { executionError = error; }
                finally { completed.SetResult(); }
            }) { IsBackground = true, Name = "blink-owning-guest" };
            execution.Start();
            request = InstanceProtocol.ReadAsync(input, InstanceJson.Default.WorkerRequest, receiveLifetime.Token);
            long? stopping = null;
            while (!completed.Task.IsCompleted)
            {
                if (!ready && stop.Reason == HostExecutionStopReason.None)
                {
                    PublishedEndpoint? endpoint = TryReady(io, options);
                    if (endpoint != null)
                    {
                        await InstanceProtocol.WriteAsync(output, new WorkerEvent { Kind = "ready", Endpoints = [endpoint] }, InstanceJson.Default.WorkerEvent);
                        ready = true;
                    }
                }
                if (request?.IsCompleted == true)
                {
                    WorkerRequest? control = await request;
                    if (control != null && (control.Kind != "stop" || control.Options != null))
                        throw new InvalidDataException("Only stop is accepted after start.");
                    stop.RequestStop(); // EOF is also a normal owner stop request.
                    request = null;
                }
                if (stop.Reason != HostExecutionStopReason.None)
                {
                    stopping ??= Stopwatch.GetTimestamp();
                    if (Stopwatch.GetElapsedTime(stopping.Value) > TimeSpan.FromSeconds(5))
                        throw new TimeoutException("Guest did not complete cooperative stop; process discard required.");
                }
                await Task.WhenAny(completed.Task, (Task?)request ?? Task.Delay(10), Task.Delay(10));
            }
        }
        catch (Exception error) { failure = error.Message; }
        finally
        {
            receiveLifetime.Cancel();
            // Standard-input cancellation can be implemented by a platform's
            // blocking read. Observe completion when available, but never let
            // protocol drainage block the owning guest join or process exit.
            if (request != null)
            {
                receiverDrained = await Task.WhenAny(request, Task.Delay(250)) == request;
                if (receiverDrained)
                {
                    try { await request; }
                    catch (OperationCanceledException) when (receiveLifetime.IsCancellationRequested) { }
                    catch (Exception error) { failure ??= error.Message; }
                }
                else _ = request.ContinueWith(task => { _ = task.Exception; },
                    CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            }
            if (execution != null && !completed.Task.IsCompleted) stop?.RequestStop();
            joined = execution == null || (execution.ThreadState & System.Threading.ThreadState.Unstarted) != 0 || execution.Join(TimeSpan.FromSeconds(5));
            notificationFailure = stop?.NotificationFailure != null;
            if (executionError != null) failure ??= executionError.Message;
            if (notificationFailure) failure ??= "Stop notification failed.";
            if (io != null)
            {
                (stdout, stderr) = io.CapturedOutput;
                // Never dispose borrowed owners underneath unjoined Machines.
                if (execution == null || (execution.ThreadState & System.Threading.ThreadState.Unstarted) != 0 || joined && owner!.IsQuiescent)
                {
                    try { await io.DisposeAsync(); ioDisposed = true; }
                    catch (Exception error) { failure ??= error.Message; }
                    try { stop?.Dispose(); }
                    catch (Exception error) { failure ??= error.Message; }
                }
                else failure ??= "Execution is not quiescent; resources retained for process discard.";
            }
        }
        bool quiescent = joined && owner?.IsQuiescent == true;
        if (result != null && (!result.AllWorkersJoined || !result.MemoryReleased)) failure ??= "Guest cleanup did not complete.";
        if (ioDisposed && io != null && (io.OpenDescriptors != 0 || io.PendingSocketOperations != 0 ||
            io.PendingPipeOperations != 0 || io.PendingEpollOperations != 0)) failure ??= "Private IO did not drain.";
        string reason = failure != null ? "worker-failure" : result?.StopReason switch
        {
            HostExecutionStopReason.Requested => "stopped",
            HostExecutionStopReason.Deadline => "deadline",
            HostExecutionStopReason.Budget => "budget",
            _ => result?.Exited == true ? "exited" : "fault"
        };
        var outcome = result?.Threads.FirstOrDefault(thread => thread.Halt != 0 || thread.Signal != 0) ?? result?.Threads.FirstOrDefault();
        string detail = Detail(joined, quiescent, ioDisposed, result, failure, notificationFailure, traces, unrecorded, receiverDrained);
        try
        {
            using var reporting = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await InstanceProtocol.WriteAsync(output, new WorkerEvent
            {
                Kind = "final", Reason = reason, ExitStatus = result?.ExitStatus ?? 0,
                Halt = outcome?.Halt ?? 0, Signal = outcome?.Signal ?? 0,
                Instructions = checked((long)(result?.Instructions ?? owner?.InstructionsCompleted ?? 0)),
                StandardOutput = stdout, StandardError = stderr, Detail = detail
            }, InstanceJson.Default.WorkerEvent, reporting.Token);
        }
        catch (Exception error) { Console.Error.WriteLine(error.Message); return 1; }
        return failure == null ? 0 : 1;
    }

    private static PublishedEndpoint? TryReady(InstanceIo io, InstanceOptions options)
    {
        byte[] expected = Encoding.ASCII.GetBytes("READY " + options.PublishedPorts[0].ToString(CultureInfo.InvariantCulture) + "\n");
        byte[] captured = io.CapturedOutput.StandardOutput;
        if (captured.Length < expected.Length)
        {
            if (!expected.AsSpan().StartsWith(captured)) throw new InvalidDataException("Guest readiness prefix differs.");
            return null;
        }
        if (!captured.AsSpan(0, expected.Length).SequenceEqual(expected)) throw new InvalidDataException("Guest readiness line differs.");
        PublishedEndpoint? found = null;
        for (int fd = 0; fd < options.DescriptorLimit; ++fd)
        {
            var local = io.LocalEndpoint(fd);
            if (!local.Succeeded || local.Value.Port != options.PublishedPorts[0]) continue;
            var published = io.Publish(fd);
            if (!published.Succeeded) continue;
            if (!IPAddress.IsLoopback(published.Value.Address) || found != null)
                throw new InvalidDataException("Ambiguous or non-loopback listener publication.");
            found = new(options.PublishedPorts[0], published.Value.Port);
        }
        return found;
    }

    private static string Detail(bool joined, bool quiescent, bool ioDisposed, ThreadedGuestExecutionResult? result,
        string? failure, bool notificationFailure, Dictionary<int, TraceRow> traces, ulong unrecorded, bool receiverDrained)
    {
        using var bytes = new MemoryStream();
        using (var writer = new Utf8JsonWriter(bytes))
        {
            writer.WriteStartObject(); writer.WriteNumber("version", 1);
            writer.WriteBoolean("receiverDrained", receiverDrained);
            writer.WriteBoolean("joined", joined); writer.WriteBoolean("quiescent", quiescent);
            writer.WriteBoolean("ioDisposed", ioDisposed); writer.WriteBoolean("memoryReleased", result?.MemoryReleased == true);
            writer.WriteString("stopReason", result?.StopReason.ToString());
            writer.WriteBoolean("notificationFailure", notificationFailure);
            writer.WriteNumber("maximumWorkers", WorkerProfile.MaximumGuestWorkers);
            if (result == null)
            {
                writer.WriteNull("retainedBytesBeforeRelease"); writer.WriteNull("retainedMappingsBeforeRelease");
            }
            else
            {
                writer.WriteNumber("retainedBytesBeforeRelease", result.RetainedBytesBeforeRelease);
                writer.WriteNumber("retainedMappingsBeforeRelease", result.RetainedMappingsBeforeRelease);
            }
            writer.WriteString("error", failure == null ? null : failure[..Math.Min(failure.Length, 64)]);
            writer.WriteStartArray("threads");
            if (joined && quiescent && result != null)
                foreach (var thread in result.Threads)
                {
                    writer.WriteStartArray(); writer.WriteNumberValue(thread.GuestThreadId); writer.WriteNumberValue(thread.Instructions);
                    writer.WriteStringValue(thread.Termination); writer.WriteNumberValue(thread.Status);
                    writer.WriteNumberValue(thread.Halt); writer.WriteNumberValue(thread.Signal); writer.WriteBooleanValue(thread.MachineReleased); writer.WriteEndArray();
                }
            writer.WriteEndArray(); writer.WriteStartArray("syscallSummary");
            // Only inspect observer-owned collections after all guest workers joined.
            if (joined && quiescent)
                foreach (var pair in traces.OrderBy(pair => pair.Key))
                {
                    writer.WriteStartArray(); writer.WriteNumberValue(pair.Key); writer.WriteNumberValue(pair.Value.Count);
                    writer.WriteNumberValue(pair.Value.Number);
                    if (pair.Value.Returned is { } value) writer.WriteNumberValue(value); else writer.WriteNullValue();
                    writer.WriteEndArray();
                }
            writer.WriteEndArray();
            if (joined && quiescent) writer.WriteNumber("unrecordedThreadObservations", unrecorded);
            else writer.WriteNull("unrecordedThreadObservations");
            writer.WriteEndObject();
        }
        string text = Encoding.UTF8.GetString(bytes.ToArray());
        if (text.Length > 4096) throw new InvalidOperationException("Worker summary exceeded protocol bound.");
        return text;
    }
}
