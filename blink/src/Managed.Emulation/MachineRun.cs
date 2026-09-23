using Managed.Emulation.Host;

namespace Managed.Emulation;

/// <summary>One execution. Canceling a wait does not terminate the guest.</summary>
public abstract class MachineRun : IAsyncDisposable
{
    protected readonly ExecutionOptions Execution;
    protected readonly HostConsole Console;
    protected readonly TaskCompletionSource<MachineRunResult> Finished = new(TaskCreationOptions.RunContinuationsAsynchronously);
    protected readonly TaskCompletionSource<IReadOnlyList<PublishedEndpoint>> ReadySource = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly CancellationTokenSource inputStop = new(), outputStop = new();
    private readonly List<Task> outputPumps = [];
    private Task? inputPump;
    private ConsoleCancelEventHandler? interrupt;
    private readonly object disposalGate = new();
    private Task? disposal;

    private protected MachineRun(ExecutionOptions execution)
    {
        Execution = execution;
        var options = execution.Console;
        Console = new(options.BufferBytes, options.OutputLimit, options.CaptureBytes,
            !options.RedirectOutput && options.Output == null && options.Error == null,
            execution.Readiness.Kind == ReadinessKind.OutputMarker ? execution.Readiness.OutputMarker : null);
    }
    public Stream StandardInput => Console.StandardInput;
    public Stream StandardOutput => Console.StandardOutput;
    public Stream StandardError => Console.StandardError;
    public Task<MachineRunResult> Completion => Finished.Task;
    /// <summary>Started by default, not an implicit service health check.</summary>
    public Task<IReadOnlyList<PublishedEndpoint>> Ready => ReadySource.Task;
    public abstract ExecutionMode ExecutionMode { get; }
    public abstract int? WorkerProcessId { get; }
    public Task<MachineRunResult> WaitAsync(CancellationToken cancellation = default) => Completion.WaitAsync(cancellation);
    protected abstract void RequestStop();
    public abstract Task<MachineRunResult> KillAsync(CancellationToken cancellation = default);

    public async Task<MachineRunResult> StopAsync(TimeSpan? grace = null, CancellationToken cancellation = default)
    {
        TimeSpan wait = grace ?? TimeSpan.FromSeconds(5);
        if (wait < TimeSpan.Zero || wait > TimeSpan.FromMinutes(1)) throw new ArgumentOutOfRangeException(nameof(grace));
        if (!Completion.IsCompleted) RequestStop();
        try { return await Completion.WaitAsync(wait, cancellation).ConfigureAwait(false); }
        catch (TimeoutException)
        {
            throw new MachineStopTimeoutException("Execution has not quiesced. Its resources remain owned; separate-process runs may be explicitly killed.");
        }
    }
    protected void StartConsole()
    {
        if (Execution.Console.InterruptPolicy == ConsoleInterruptPolicy.Stop)
        {
            interrupt = (_, args) => { args.Cancel = true; RequestStop(); };
            System.Console.CancelKeyPress += interrupt;
        }
        if (Execution.Console.Input != null) inputPump = Task.Run(() => PumpInputAsync(Execution.Console.Input));
        else if (!Execution.Console.RedirectInput) Console.CloseInput();
        if (!Execution.Console.RedirectOutput && (Execution.Console.Output != null || Execution.Console.Error != null))
        {
            outputPumps.Add(Task.Run(() => PumpOutputAsync(StandardOutput, Execution.Console.Output ?? Stream.Null)));
            outputPumps.Add(Task.Run(() => PumpOutputAsync(StandardError, Execution.Console.Error ?? Stream.Null)));
        }
    }
    private async Task PumpInputAsync(Stream source)
    {
        try { await source.CopyToAsync(StandardInput, 16384, inputStop.Token).ConfigureAwait(false); }
        finally { Console.CloseInput(); }
    }
    private Task PumpOutputAsync(Stream source, Stream destination)
        => source.CopyToAsync(destination, 16384, outputStop.Token);

    protected async Task FinishAsync(MachineRunResult result)
    {
        try { result = await FinishConsoleAsync(result).ConfigureAwait(false); }
        catch (Exception error)
        {
            result = result with { Diagnostic = Bounded((result.Diagnostic ?? "") + "; Console finalization failed: " + error.Message) };
        }
        finally
        {
            try { if (interrupt != null) { System.Console.CancelKeyPress -= interrupt; interrupt = null; } }
            catch (Exception error) { result = result with { Diagnostic = Bounded((result.Diagnostic ?? "") + "; Console handler removal failed: " + error.Message) }; }
            Finished.TrySetResult(result);
            ReadySource.TrySetException(new InvalidOperationException("Execution ended before its readiness condition: " + result.Reason));
        }
    }
    protected void FailStart(Exception error)
    {
        Console.Stop();
        ObserveFailure(FinishAsync(new(RunExitReason.ExecutionFailure, 0, 0, 0, 0, [], [], false, 0, true, Bounded(error.Message))));
    }
    protected static void ObserveFailure(Task task) => _ = task.ContinueWith(t => { _ = t.Exception; }, CancellationToken.None,
        TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
    private async Task<MachineRunResult> FinishConsoleAsync(MachineRunResult result)
    {
        // User Stream implementations own cancellation callbacks. They may
        // throw or ignore cancellation; keep them outside the control path.
        Task cancelInput = Task.Run(inputStop.Cancel);
        Console.CloseInput(); Console.CompleteOutput();
        if (interrupt != null) { System.Console.CancelKeyPress -= interrupt; interrupt = null; }
        var pending = outputPumps.Concat(inputPump == null ? [] : new[] { inputPump }).Append(cancelInput).ToArray();
        string? consoleFailure = null;
        Task drain = Task.WhenAll(pending);
        try { await drain.WaitAsync(Execution.Console.DrainTimeout).ConfigureAwait(false); }
        catch (OperationCanceledException) { }
        catch (TimeoutException)
        {
            consoleFailure = "Console pump did not finish within the drain bound; its outstanding caller-stream operation remains observed.";
            ObserveFailure(Task.Run(outputStop.Cancel));
            _ = drain.ContinueWith(task => { _ = task.Exception; }, CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }
        catch (Exception error) { consoleFailure = "Console pump failed: " + error.Message; }
        if (!Execution.Console.LeaveOpen)
        {
            var streams = new[] { Execution.Console.Input, Execution.Console.Output, Execution.Console.Error }
                .OfType<Stream>().Distinct().ToArray();
            Task disposal = Task.WhenAll(streams.Select(stream => Task.Run(async () => await stream.DisposeAsync().ConfigureAwait(false))));
            try { await disposal.WaitAsync(Execution.Console.DrainTimeout).ConfigureAwait(false); }
            catch (Exception error)
            {
                consoleFailure ??= "Owned console stream disposal did not complete: " + error.Message;
                _ = disposal.ContinueWith(task => { _ = task.Exception; }, CancellationToken.None,
                    TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            }
        }
        var captured = Console.CapturedOutput;
        if (consoleFailure != null)
            result = result with { Diagnostic = Bounded(result.Diagnostic == null ? consoleFailure : result.Diagnostic + "; " + consoleFailure) };
        result = result with { StandardOutput = captured.StandardOutput, StandardError = captured.StandardError,
            CaptureTruncated = result.CaptureTruncated || Console.CaptureTruncated, OutputBytes = Math.Max(result.OutputBytes, Console.OutputBytes) };
        return result;
    }
    protected static string Bounded(string value) => value[..Math.Min(value.Length, 4096)];
    public ValueTask DisposeAsync()
    {
        TaskCompletionSource completion;
        lock (disposalGate)
        {
            if (disposal != null) return new(disposal);
            completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
            disposal = completion.Task;
        }
        _ = DisposeCoreAsync(completion);
        return new(completion.Task);
    }
    private async Task DisposeCoreAsync(TaskCompletionSource completion)
    {
        try
        {
            if (!Completion.IsCompleted) await StopAsync().ConfigureAwait(false);
            var result = await Completion.ConfigureAwait(false);
            if (!result.ResourcesReleased) throw new MachineStopTimeoutException("Execution resources remain live and cannot be disposed safely.");
            Console.Dispose();
            await DisposeExecutionAsync().ConfigureAwait(false);
            completion.TrySetResult();
        }
        catch (Exception error)
        {
            // All concurrent callers observe this attempt; a later call can
            // retry once the guest has quiesced or cleanup becomes possible.
            lock (disposalGate) { disposal = null; completion.TrySetException(error); }
        }
    }
    protected abstract ValueTask DisposeExecutionAsync();
}
