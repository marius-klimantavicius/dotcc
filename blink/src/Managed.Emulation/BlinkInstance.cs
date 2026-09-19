using System.Diagnostics;
using System.Text;

namespace Managed.Emulation;

public sealed record WorkerLaunch(string FileName, string[] Arguments);
public sealed record InstanceResult(string Reason, int ExitStatus, int Halt, int Signal,
    long Instructions, byte[] StandardOutput, byte[] StandardError, string? Detail,
    int WorkerExitCode, string WorkerDiagnostics);

/// <summary>Owns one managed worker process. This is lifecycle containment for
/// controlled images, not an OS security sandbox. Every restart uses a new worker.</summary>
public sealed class BlinkInstance : IAsyncDisposable
{
    private readonly Process process;
    private readonly InstanceOptions options;
    private readonly SemaphoreSlim writes = new(1, 1);
    private readonly CancellationTokenSource lifetime = new();
    private readonly TaskCompletionSource<PublishedEndpoint[]> ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource<InstanceResult> completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly CancellationTokenSource channels = new();
    private readonly Task rootExit;
    private readonly Task monitor;
    private string? requestedStop;
    private string? diagnosticFailure;
    private int disposed;
    private BlinkInstance(Process process, InstanceOptions options)
    {
        this.process = process; this.options = options;
        rootExit = process.WaitForExitAsync();
        monitor = MonitorAsync();
        _ = BoundDrainsAsync();
        _ = DeadlineAsync();
    }
    public Task<PublishedEndpoint[]> Ready => ready.Task;
    public Task<InstanceResult> Completion => completion.Task;
    public int WorkerProcessId => process.Id;

    public static async Task<BlinkInstance> StartAsync(WorkerLaunch worker, InstanceOptions options,
        CancellationToken cancellation = default)
    {
        ArgumentNullException.ThrowIfNull(worker);
        ArgumentNullException.ThrowIfNull(options);
        options = options.Snapshot();
        byte[] startFrame = InstanceProtocol.Encode(new WorkerRequest("start", options), InstanceJson.Default.WorkerRequest);
        cancellation.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(worker.FileName) || worker.Arguments is null)
            throw new ArgumentException("A managed worker executable is required.");
        var start = new ProcessStartInfo(worker.FileName)
        {
            UseShellExecute = false, RedirectStandardInput = true,
            RedirectStandardOutput = true, RedirectStandardError = true,
            CreateNoWindow = true,
        };
        foreach (string argument in worker.Arguments) start.ArgumentList.Add(argument);
        var process = new Process { StartInfo = start };
        BlinkInstance? owner = null;
        try
        {
            if (!process.Start()) throw new IOException("Worker did not start.");
            owner = new(process, options);
            await owner.SendEncodedAsync(startFrame, cancellation).ConfigureAwait(false);
            return owner;
        }
        catch
        {
            if (owner is not null) await owner.DisposeAsync().ConfigureAwait(false);
            else process.Dispose();
            throw;
        }
    }
    private Task SendAsync(WorkerRequest request, CancellationToken cancellation)
        => SendEncodedAsync(InstanceProtocol.Encode(request, InstanceJson.Default.WorkerRequest), cancellation);
    private async Task SendEncodedAsync(byte[] frame, CancellationToken cancellation)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellation, lifetime.Token);
        await writes.WaitAsync(linked.Token).ConfigureAwait(false);
        try { await InstanceProtocol.WriteEncodedAsync(process.StandardInput.BaseStream, frame, linked.Token).ConfigureAwait(false); }
        finally { writes.Release(); }
    }
    private async Task BoundDrainsAsync()
    {
        try
        {
            await rootExit.ConfigureAwait(false);
            // An exited worker cannot produce new results. Allow buffered frames
            // to drain, then release inherited handles held by unrelated children.
            await Task.Delay(250, lifetime.Token).ConfigureAwait(false);
            channels.Cancel();
        }
        catch (OperationCanceledException) { }
    }
    private async Task DeadlineAsync()
    {
        try
        {
            await Task.Delay(options.WallClockMilliseconds, lifetime.Token).ConfigureAwait(false);
            Interlocked.CompareExchange(ref requestedStop, "deadline", null);
            Kill();
        }
        catch (OperationCanceledException) { }
    }
    private void Kill()
    {
        try { if (!process.HasExited) process.Kill(entireProcessTree: true); }
        catch (InvalidOperationException) { }
        finally { channels.Cancel(); }
    }
    public async Task<InstanceResult> StopAsync(TimeSpan grace, CancellationToken cancellation = default)
    {
        if (grace < TimeSpan.Zero || grace > TimeSpan.FromMinutes(1)) throw new ArgumentOutOfRangeException(nameof(grace));
        if (!completion.Task.IsCompleted)
        {
            Interlocked.CompareExchange(ref requestedStop, "stopped", null);
            using var stopDeadline = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
            stopDeadline.CancelAfter(grace);
            long began = Stopwatch.GetTimestamp();
            try { await SendAsync(new("stop"), stopDeadline.Token).ConfigureAwait(false); }
            catch (Exception error) when (error is IOException or OperationCanceledException or ObjectDisposedException) { Kill(); }
            TimeSpan remaining = grace - Stopwatch.GetElapsedTime(began);
            if (remaining < TimeSpan.Zero) remaining = TimeSpan.Zero;
            try { return await completion.Task.WaitAsync(remaining, cancellation).ConfigureAwait(false); }
            catch (TimeoutException) { Kill(); }
            catch (OperationCanceledException) { Kill(); throw; }
        }
        return await completion.Task.WaitAsync(cancellation).ConfigureAwait(false);
    }
    private async Task<string> DrainDiagnosticsAsync()
    {
        var text = new StringBuilder();
        char[] buffer = new char[4096];
        int count;
        try
        {
            while ((count = await process.StandardError.ReadAsync(buffer.AsMemory(), channels.Token).ConfigureAwait(false)) != 0)
            {
                int keep = Math.Min(count, 65536 - text.Length);
                if (keep > 0) text.Append(buffer, 0, keep);
                if (keep != count)
                {
                    Interlocked.CompareExchange(ref requestedStop, "worker-output-limit", null);
                    Kill();
                }
            }
        }
        catch (OperationCanceledException) when (channels.IsCancellationRequested)
        {
            diagnosticFailure = "Worker diagnostic drain was cancelled.";
        }
        return text.ToString();
    }
    private void Validate(WorkerEvent item)
    {
        if (item.StandardOutput is null || item.StandardError is null || item.Endpoints is null ||
            (long)item.StandardOutput.Length + item.StandardError.Length > options.OutputLimit ||
            item.Instructions < 0 || item.Instructions > options.InstructionLimit ||
            item.Detail?.Length > 4096)
            throw new InvalidDataException("Worker event exceeded its instance bounds.");
        if (item.Kind == "ready")
        {
            if (item.Endpoints.Length != options.PublishedPorts.Length ||
                item.Endpoints.Select(endpoint => endpoint.GuestPort).Distinct().Count() != item.Endpoints.Length ||
                item.Endpoints.Any(endpoint => !options.PublishedPorts.Contains(endpoint.GuestPort) || endpoint.HostPort is < 1 or > 65535))
                throw new InvalidDataException("Worker published an unexpected endpoint.");
        }
        else if (item.Kind != "final") throw new InvalidDataException("Unknown worker event.");
    }
    private async Task MonitorAsync()
    {
        WorkerEvent? final = null;
        string? failure = null;
        Task<string> diagnostics = DrainDiagnosticsAsync();
        try
        {
            int count = 0;
            while (await InstanceProtocol.ReadAsync(process.StandardOutput.BaseStream,
                       InstanceJson.Default.WorkerEvent, channels.Token).ConfigureAwait(false) is { } item)
            {
                if (++count > 2 || final is not null) throw new InvalidDataException("Unexpected worker event sequence.");
                Validate(item);
                if (item.Kind == "ready")
                {
                    if (!ready.TrySetResult(item.Endpoints)) throw new InvalidDataException("Duplicate readiness event.");
                }
                else final = item;
            }
        }
        catch (Exception error) { failure = error.Message; Kill(); }
        finally
        {
            // Closing the result channel is terminal; a worker must now exit.
            // A broken worker is still bounded by the independent deadline.
            await rootExit.ConfigureAwait(false);
            string captured;
            try { captured = await diagnostics.ConfigureAwait(false); }
            catch (Exception error) { captured = error.Message; failure ??= "Worker diagnostic channel failed."; }
            failure ??= diagnosticFailure;
            lifetime.Cancel();
            int code = process.ExitCode;
            string reason = requestedStop ?? (failure is not null || final is null || code != 0 ? "worker-failure" : final.Reason ?? "worker-failure");
            var result = new InstanceResult(reason, final?.ExitStatus ?? 0, final?.Halt ?? 0,
                final?.Signal ?? 0, final?.Instructions ?? 0, final?.StandardOutput ?? [],
                final?.StandardError ?? [], failure ?? final?.Detail, code, captured);
            completion.TrySetResult(result);
            ready.TrySetException(new InvalidOperationException("Worker ended before readiness: " + reason));
        }
    }
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref disposed, 1) != 0) { await completion.Task.ConfigureAwait(false); return; }
        try
        {
            if (!completion.Task.IsCompleted)
                await StopAsync(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            await monitor.ConfigureAwait(false);
        }
        finally
        {
            lifetime.Cancel(); process.Dispose();
            // Watchers and concurrent StopAsync calls may still be leaving their
            // cancellation callbacks; do not dispose their shared token sources.
            // A concurrent StopAsync can still be leaving the write gate.
        }
    }
}
