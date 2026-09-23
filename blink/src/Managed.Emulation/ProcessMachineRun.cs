using System.Diagnostics;
using Managed.Emulation.Host;

namespace Managed.Emulation;

internal sealed class ProcessMachineRun : MachineRun
{
    private readonly Process process;
    private readonly int processId;
    private readonly MachineConnection connection;
    private readonly GuestFileSystemSession session;
    private readonly CancellationTokenSource pumps = new();
    private readonly TaskCompletionSource<MachineRunResult> final = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource hello = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly object processGate = new();
    private bool killed, processDisposed;
    private readonly Task receive;
    private readonly Task diagnostics;
    private string diagnostic = "";

    internal ProcessMachineRun(MachineOptions options, ExecutionOptions execution,
        IReadOnlyDictionary<string, string> environment, GuestFileSystemSession session, MachineStorage storage) : base(execution)
    {
        this.session = session;
        WorkerLaunch launch = options.Worker ?? WorkerDiscovery.Discover();
        if (string.IsNullOrWhiteSpace(launch.FileName) || launch.Arguments == null) throw new ArgumentException("Invalid worker launch.");
        var start = new ProcessStartInfo(launch.FileName) { UseShellExecute = false, RedirectStandardInput = true,
            RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };
        foreach (string argument in launch.Arguments) start.ArgumentList.Add(argument);
        start.ArgumentList.Add("--machine-api-v1");
        process = new() { StartInfo = start };
        try { if (!process.Start()) throw new IOException("Worker did not start."); }
        catch { process.Dispose(); throw; }
        processId = process.Id;
        connection = new(process.StandardOutput.BaseStream, process.StandardInput.BaseStream);
        diagnostics = DrainDiagnosticsAsync();
        receive = connection.ReceiveAsync(frame =>
        {
            if (!hello.Task.IsCompleted)
            {
                if (frame.Kind != "hello-v1") throw new InvalidDataException("Worker does not implement machine protocol version 1.");
                hello.TrySetResult(); return Task.CompletedTask;
            }
            switch (frame.Kind)
            {
                case "ready": ReadySource.TrySetResult(frame.Endpoints ?? []); break;
                case "final": final.TrySetResult(frame.Result ?? throw new InvalidDataException("Missing worker outcome.")); break;
                default: throw new InvalidDataException("Unexpected worker control frame.");
            }
            return Task.CompletedTask;
        });
        try { StartConsole(); }
        catch (Exception error)
        {
            _ = connection.DisposeAsync();
            KillProcessIfRunning(requested: false); process.WaitForExit();
            DisposeExecutionAsync().GetAwaiter().GetResult();
            foreach (Task task in new[] { receive, diagnostics }) ObserveFailure(task);
            FailStart(error);
            throw;
        }
        var request = new MachineStart(options with { ExecutionMode = ExecutionMode.InProcess, Worker = null },
            new(execution.Executable, execution.Arguments, execution.WorkingDirectory, new(environment), execution.Readiness,
                execution.Console.BufferBytes, execution.Console.OutputLimit, execution.Console.CaptureBytes, execution.Console.DrainTimeout), storage);
        _ = ObserveAsync(request);
    }
    public override ExecutionMode ExecutionMode => ExecutionMode.SeparateProcess;
    public override int? WorkerProcessId => processId;
    protected override void RequestStop()
    {
        Console.Stop();
        _ = SendStopAsync();
    }
    private async Task SendStopAsync()
    {
        try { await connection.SendAsync(new("stop"), pumps.Token).ConfigureAwait(false); }
        catch (Exception error) { if (!Completion.IsCompleted) final.TrySetException(error); }
    }
    public override async Task<MachineRunResult> KillAsync(CancellationToken cancellation = default)
    {
        KillProcessIfRunning(requested: true);
        return await Completion.WaitAsync(cancellation).ConfigureAwait(false);
    }
    private bool WasKilled { get { lock (processGate) return killed; } }
    private void KillProcessIfRunning(bool requested)
    {
        lock (processGate)
        {
            if (processDisposed || Completion.IsCompleted || process.HasExited) return;
            try
            {
                process.Kill(entireProcessTree: true);
                if (requested) killed = true;
            }
            // Natural exit may win between HasExited and Kill. Only ignore
            // an error when the same, still-owned Process confirms its exit.
            catch (InvalidOperationException) when (process.HasExited) { }
            catch (System.ComponentModel.Win32Exception) when (process.HasExited) { }
        }
    }
    private async Task SendInputAsync()
    {
        byte[] buffer = new byte[MachineConnection.Chunk];
        while (true)
        {
            var read = await Console.ReadInputAsync(buffer, pumps.Token).ConfigureAwait(false);
            if (!read.Succeeded || read.Value == 0) break;
            await connection.SendDataAsync(0, buffer.AsSpan(0, read.Value).ToArray(), pumps.Token).ConfigureAwait(false);
        }
        await connection.SendDataAsync(0, null, pumps.Token).ConfigureAwait(false);
    }
    private async Task AcceptOutputAsync(bool error, byte[] bytes, CancellationToken token)
    {
        int offset = 0;
        while (offset < bytes.Length)
        {
            var write = await Console.WriteOutputAsync(error, bytes.AsMemory(offset), token).ConfigureAwait(false);
            if (!write.Succeeded || write.Value == 0) return;
            offset += write.Value;
        }
    }
    private async Task DrainDiagnosticsAsync()
    {
        char[] buffer = new char[2048]; int count;
        try
        {
            while ((count = await process.StandardError.ReadAsync(buffer).ConfigureAwait(false)) != 0)
                if (diagnostic.Length < 4096) diagnostic += new string(buffer, 0, Math.Min(count, 4096 - diagnostic.Length));
        }
        catch (IOException) { }
    }
    private async Task ObserveAsync(MachineStart request)
    {
        MachineRunResult? outcome = null;
        string? failure = null;
        Task? sending = null, stdout = null, stderr = null;
        try
        {
            await connection.SendAsync(new("start-v1", Start: request), pumps.Token).ConfigureAwait(false);
            Task handshake = hello.Task.WaitAsync(TimeSpan.FromSeconds(10));
            if (await Task.WhenAny(handshake, receive).ConfigureAwait(false) == receive)
            {
                await receive.ConfigureAwait(false);
                if (!hello.Task.IsCompletedSuccessfully) throw new EndOfStreamException("Worker ended before its handshake.");
            }
            await handshake.ConfigureAwait(false);
            sending = SendInputAsync();
            stdout = connection.PumpReceivedAsync(1, (bytes, token) => AcceptOutputAsync(false, bytes, token), () => Console.CloseDescriptor(1), pumps.Token);
            stderr = connection.PumpReceivedAsync(2, (bytes, token) => AcceptOutputAsync(true, bytes, token), () => Console.CloseDescriptor(2), pumps.Token);
            foreach (Task transfer in new[] { sending, stdout, stderr })
                _ = transfer.ContinueWith(t => final.TrySetException(t.Exception!), CancellationToken.None,
                    TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            Task exited = process.WaitForExitAsync();
            Task first = await Task.WhenAny(final.Task, receive, exited).ConfigureAwait(false);
            if (first == final.Task) outcome = await final.Task.ConfigureAwait(false);
            else if (first == receive) await receive.ConfigureAwait(false);
            await exited.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            // Exit notification can precede consumption of the final buffered
            // frame. Drain the protocol pipe before deciding it was absent.
            await receive.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            if (outcome == null)
            {
                if (final.Task.IsCompleted) outcome = await final.Task.ConfigureAwait(false);
                else if (!WasKilled) throw new IOException("Worker exited without a final outcome.");
            }
            try { await Task.WhenAll(stdout, stderr).WaitAsync(Execution.Console.DrainTimeout).ConfigureAwait(false); }
            catch (TimeoutException) { failure = "Worker output transfer did not drain within its bound."; }
        }
        catch (Exception error) { if (!WasKilled) failure = Bounded(error.ToString()); }
        finally
        {
            pumps.Cancel(); await connection.DisposeAsync().ConfigureAwait(false);
            // Broken protocol/startup cannot retain an ownerless child.
            KillProcessIfRunning(requested: false);
            await process.WaitForExitAsync().ConfigureAwait(false);
            try { await diagnostics.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false); }
            catch (Exception error) { failure = Bounded((failure ?? "") + "; Worker diagnostic drain failed: " + error.Message); }
            foreach (Task task in new[] { receive, sending, stdout, stderr, diagnostics }.OfType<Task>())
                _ = task.ContinueWith(t => { _ = t.Exception; }, CancellationToken.None,
                    TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            session.Dispose();
        }
        bool wasKilled = WasKilled;
        outcome ??= new(wasKilled ? RunExitReason.Killed : RunExitReason.ExecutionFailure, 0, 0, 0, 0, [], [], false, 0, true, null);
        string combinedDiagnostic = string.Join("; ", new[] { outcome.Diagnostic, failure, diagnostic }.Where(s => !string.IsNullOrEmpty(s)));
        outcome = outcome with { ResourcesReleased = true, Reason = wasKilled ? RunExitReason.Killed : failure != null ? RunExitReason.ExecutionFailure : outcome.Reason,
            Diagnostic = combinedDiagnostic.Length == 0 ? null : Bounded(combinedDiagnostic) };
        await FinishAsync(outcome).ConfigureAwait(false);
    }
    protected override ValueTask DisposeExecutionAsync()
    {
        lock (processGate)
        {
            if (!processDisposed) { process.Dispose(); processDisposed = true; }
        }
        return ValueTask.CompletedTask;
    }
}
