using System.Diagnostics;
using Managed.Emulation.Execution;
using Managed.Emulation.Host;

namespace Managed.Emulation;

internal sealed class InProcessMachineRun : MachineRun
{
    private readonly MachineOptions options;
    private readonly GuestFileSystemSession session;
    private readonly HostExecutionStop stop;
    private readonly InstanceIo io;
    private readonly ThreadedGuestExecution owner;
    private readonly Thread thread;
    private readonly TaskCompletionSource returned = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private ThreadedGuestExecutionResult? result;
    private Exception? failure;
    private int loaded;

    internal InProcessMachineRun(MachineOptions options, ExecutionOptions execution,
        IReadOnlyDictionary<string, string> environment, GuestFileSystemSession session) : base(execution)
    {
        this.options = options; this.session = session;
        stop = new(options.ExecutionDeadline);
        InstanceIo? createdIo = null;
        try
        {
        io = createdIo = new(session.FileSystem, descriptorLimit: options.DescriptorLimit, inputLimit: 0,
            pipeCapacity: options.PipeBufferBytes, pipeByteLimit: options.PipeStorageLimit,
            pipeOperationLimit: options.PendingPipeOperations, console: Console,
            networkPolicy: new GuestNetworkPolicy(options.Network.Publications.Select(p => new GuestPortGrant(p.GuestPort, p.HostAddress, p.HostPort)),
                options.Network.OutboundDestinations.Select(p => new GuestOutboundGrant(p.Address, p.Port)), options.Metadata));
        owner = new(io, stop, (ulong)options.MemoryLimit, options.ThreadLimit);
        string[] argv = [execution.Executable, .. execution.Arguments];
        string[] env = environment.Select(p => p.Key + "=" + p.Value).ToArray();
        thread = new Thread(() =>
        {
            try { result = owner.Run(execution.Executable, argv, env, (ulong)(options.InstructionLimit ?? long.MaxValue),
                workingDirectory: execution.WorkingDirectory, started: () => Volatile.Write(ref loaded, 1)); }
            catch (Exception error) { failure = error; }
            finally { returned.TrySetResult(); }
        }) { IsBackground = true, Name = "Blink guest" };
        StartConsole(); thread.Start();
        }
        catch (Exception error)
        {
            FailStart(error);
            try { createdIo?.DisposeAsync().AsTask().GetAwaiter().GetResult(); }
            finally { stop.Dispose(); }
            throw;
        }
        _ = ObserveAsync();
    }
    public override ExecutionMode ExecutionMode => ExecutionMode.InProcess;
    public override int? WorkerProcessId => null;
    protected override void RequestStop() { stop.RequestStop(); Console.Stop(); }
    public override Task<MachineRunResult> KillAsync(CancellationToken cancellation = default)
        => throw new NotSupportedException("An in-process CLR execution cannot be forcibly killed. Use StopAsync or choose SeparateProcess when creating the machine.");

    private PublishedEndpoint[] Endpoints()
    {
        var found = new Dictionary<ushort, PublishedEndpoint>();
        for (int fd = 0; fd < options.DescriptorLimit; ++fd)
        {
            var local = io.LocalEndpoint(fd);
            if (!local.Succeeded || !options.Network.Publications.Any(p => p.GuestPort == local.Value.Port)) continue;
            var published = io.Publish(fd);
            if (published.Succeeded) found[(ushort)local.Value.Port] = new((ushort)local.Value.Port, published.Value.Port, published.Value.Address.ToString());
        }
        return found.Values.OrderBy(e => e.GuestPort).ToArray();
    }
    private async Task ObserveAsync()
    {
        long began = Stopwatch.GetTimestamp();
        try
        {
            while (!returned.Task.IsCompleted)
            {
                if (Console.OutputLimitReached) RequestStop();
                if (!ReadySource.Task.IsCompleted && Volatile.Read(ref loaded) != 0)
                {
                    var endpoints = Endpoints();
                    bool ready = Execution.Readiness.Kind switch
                    {
                        ReadinessKind.Started => true,
                        ReadinessKind.OutputMarker => Console.OutputMarkerSeen,
                        ReadinessKind.ListeningPorts => endpoints.Length == options.Network.Publications.Length,
                        _ => false
                    };
                    if (ready) ReadySource.TrySetResult(endpoints);
                }
                if (!ReadySource.Task.IsCompleted && Execution.Readiness.Timeout is { } timeout && Stopwatch.GetElapsedTime(began) >= timeout)
                {
                    ReadySource.TrySetException(new TimeoutException("Guest readiness condition did not complete within its bound."));
                    RequestStop();
                }
                await Task.WhenAny(returned.Task, Task.Delay(5)).ConfigureAwait(false);
            }
            thread.Join();
            // A very short successful run may finish between monitor samples.
            if (Volatile.Read(ref loaded) != 0 && Execution.Readiness.Kind == ReadinessKind.Started)
                ReadySource.TrySetResult([]);
            else if (Execution.Readiness.Kind == ReadinessKind.OutputMarker && Console.OutputMarkerSeen)
                ReadySource.TrySetResult([]);
        }
        catch (Exception error) { failure ??= error; RequestStop(); await returned.Task.ConfigureAwait(false); thread.Join(); }
        bool released = owner.IsQuiescent;
        if (released)
        {
            try { await io.DisposeAsync().ConfigureAwait(false); stop.Dispose(); session.Dispose(); }
            catch (Exception error) { released = false; failure ??= error; }
        }
        var outcome = result?.Threads.FirstOrDefault(t => t.Signal != 0 || t.Halt != 0);
        var reason = Console.OutputLimitReached ? RunExitReason.OutputLimit : failure != null ? RunExitReason.ExecutionFailure : result?.StopReason switch
        {
            HostExecutionStopReason.Requested => RunExitReason.Stopped,
            HostExecutionStopReason.Deadline => RunExitReason.Deadline,
            HostExecutionStopReason.Budget => RunExitReason.InstructionLimit,
            _ => result?.Exited == true ? RunExitReason.Exited : (outcome?.Signal ?? 0) != 0 ? RunExitReason.GuestSignal : RunExitReason.ExecutionFailure
        };
        string? diagnostic = failure == null ? null : Bounded(failure.ToString());
        if (diagnostic == null && result != null && (reason is RunExitReason.ExecutionFailure or RunExitReason.InstructionLimit or RunExitReason.Deadline ||
            reason == RunExitReason.GuestSignal && result.Threads.Any(t => t.FirstTrap != null)))
            diagnostic = Bounded("Guest execution stopped without exit: " + string.Join("; ", result.Threads.Select(t =>
                $"tid={t.GuestThreadId} {t.Termination} instructions={t.Instructions} ip=0x{t.InstructionPointer:x} halt={t.Halt} signal={t.Signal} first-trap=({t.FirstTrap})")));
        await FinishAsync(new(reason, result?.ExitStatus ?? 0, outcome?.Signal ?? 0, outcome?.Halt ?? 0,
            (long)(result?.Instructions ?? owner.InstructionsCompleted), [], [], false, 0, released,
            diagnostic)).ConfigureAwait(false);
    }
    protected override ValueTask DisposeExecutionAsync() => ValueTask.CompletedTask;
}
