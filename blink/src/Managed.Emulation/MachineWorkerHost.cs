using Managed.Emulation.Host;

namespace Managed.Emulation;

/// <summary>Dedicated worker entry point for explicit SeparateProcess mode.
/// The supplied streams carry protocol frames, never guest text.</summary>
public static class MachineWorkerHost
{
    public static async Task<int> RunAsync(Stream input, Stream output)
    {
        await using var connection = new MachineConnection(input, output);
        var receivedStart = new TaskCompletionSource<MachineStart>(TaskCreationOptions.RunContinuationsAsynchronously);
        InProcessMachineRun? run = null;
        MountedFileSystem? fileSystem = null;
        int earlyStop = 0;
        using var transfers = new CancellationTokenSource();
        var tasks = new List<Task>();
        Task receive = connection.ReceiveAsync(frame =>
        {
            switch (frame.Kind)
            {
                case "start-v1":
                    if (frame.Start == null || !receivedStart.TrySetResult(frame.Start)) throw new InvalidDataException("Exactly one start is required.");
                    break;
                case "stop":
                    Volatile.Write(ref earlyStop, 1);
                    if (Volatile.Read(ref run) is { } current) _ = StopObservedAsync(current);
                    break;
                default: throw new InvalidDataException("Unexpected controller command.");
            }
            return Task.CompletedTask;
        });
        bool released = true;
        try
        {
            Task first = await Task.WhenAny(receivedStart.Task, receive).ConfigureAwait(false);
            if (first == receive) { await receive.ConfigureAwait(false); throw new EndOfStreamException("Controller omitted start."); }
            var request = await receivedStart.Task.ConfigureAwait(false);
            var options = request.Options.Freeze();
            if (options.ExecutionMode != ExecutionMode.InProcess || options.Worker != null) throw new InvalidDataException("Recursive workers are forbidden.");
            var execution = new ExecutionOptions { Executable = request.Execution.Executable, Arguments = request.Execution.Arguments,
                WorkingDirectory = request.Execution.WorkingDirectory, Readiness = request.Execution.Readiness,
                Console = new() { RedirectInput = true, RedirectOutput = true, BufferBytes = request.Execution.BufferBytes,
                    CaptureBytes = request.Execution.CaptureBytes, OutputLimit = request.Execution.OutputLimit,
                    DrainTimeout = request.Execution.DrainTimeout } }.Freeze();
            GuestText.EnvironmentSize(request.Execution.Environment);
            foreach (var pair in request.Execution.Environment) GuestText.EnvironmentEntry(pair.Key, pair.Value);
            if (!Path.IsPathFullyQualified(request.Storage.Root) || request.Storage.Mounts.Length > options.FileNodeLimit)
                throw new InvalidDataException("Invalid frozen storage grants.");
            var root = HostDirectoryFileSystem.OpenRoot(request.Storage.Root, descriptorLimit: options.DescriptorLimit,
                writableLimit: options.WritableStorageLimit, nodeLimit: options.FileNodeLimit);
            fileSystem = new(root, ownsRoot: true, privateWritableLimit: options.WritableStorageLimit,
                privateNodeLimit: options.FileNodeLimit);
            MachineDevices.Mount(fileSystem, options.DescriptorLimit);
            foreach (var mount in request.Storage.Mounts)
            {
                GuestText.Path(mount.GuestPath);
                if (!Path.IsPathFullyQualified(mount.HostPath) || !Enum.IsDefined(mount.Access)) throw new InvalidDataException("Invalid frozen mount grant.");
                var store = HostDirectoryFileSystem.OpenRoot(mount.HostPath, mount.Access == MountAccess.ReadOnly,
                    descriptorLimit: options.DescriptorLimit,
                    writableLimit: mount.Access == MountAccess.CopyOnWrite ? options.WritableStorageLimit : long.MaxValue,
                    nodeLimit: options.FileNodeLimit);
                try { fileSystem.Mount(mount.GuestPath, store, mount.Access == MountAccess.ReadOnly, ownsSource: true,
                    replaceExistingDirectory: mount.Replace, privateStorage: mount.Access == MountAccess.CopyOnWrite); }
                catch { store.Dispose(); throw; }
            }
            await connection.SendAsync(new("hello-v1"), transfers.Token).ConfigureAwait(false);
            var session = fileSystem.AcquireSession();
            try { run = new(options, execution, request.Execution.Environment, session); }
            catch { session.Dispose(); throw; }
            released = false;
            tasks.Add(connection.PumpReceivedAsync(0, (bytes, token) => run.StandardInput.WriteAsync(bytes, token).AsTask(),
                () => run.StandardInput.Close(), transfers.Token));
            Task stdout = connection.PumpSentAsync(1, run.StandardOutput, transfers.Token);
            Task stderr = connection.PumpSentAsync(2, run.StandardError, transfers.Token);
            tasks.Add(stdout); tasks.Add(stderr);
            Task ready = SendReadinessAsync(); tasks.Add(ready);
            if (Volatile.Read(ref earlyStop) != 0) tasks.Add(StopObservedAsync(run));
            first = await Task.WhenAny(run.Completion, receive).ConfigureAwait(false);
            if (first == receive && !run.Completion.IsCompleted)
            {
                await receive.ConfigureAwait(false);
                throw new EndOfStreamException("Controller disconnected while the guest was running.");
            }
            MachineRunResult result = await run.Completion.ConfigureAwait(false);
            released = result.ResourcesReleased;
            await ready.ConfigureAwait(false);
            try { await Task.WhenAll(stdout, stderr).WaitAsync(execution.Console.DrainTimeout).ConfigureAwait(false); }
            catch (TimeoutException)
            {
                result = result with { CaptureTruncated = true, Diagnostic = "Console transfer exceeded the drain bound; undelivered output was discarded." };
            }
            // Final frames carry metadata only; data channels own binary bytes.
            await connection.SendAsync(new("final", Result: result with { StandardOutput = [], StandardError = [] }), transfers.Token).ConfigureAwait(false);
            if (released) await run.DisposeAsync().ConfigureAwait(false);
            return result.Reason == RunExitReason.ExecutionFailure ? 1 : 0;

            async Task SendReadinessAsync()
            {
                try { await connection.SendAsync(new("ready", Endpoints: (await run.Ready.ConfigureAwait(false)).ToArray()), transfers.Token).ConfigureAwait(false); }
                catch (InvalidOperationException) { }
                catch (TimeoutException) { }
            }
        }
        catch (Exception error)
        {
            if (run != null && !run.Completion.IsCompleted)
            {
                try { var stopped = await run.StopAsync().ConfigureAwait(false); released = stopped.ResourcesReleased; }
                catch (MachineStopTimeoutException) { }
            }
            try
            {
                using var reporting = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                await connection.SendAsync(new("final", Result: new(RunExitReason.ExecutionFailure, 0, 0, 0, 0,
                    [], [], false, 0, released, error.ToString()[..Math.Min(error.ToString().Length, 4096)])), reporting.Token).ConfigureAwait(false);
            }
            catch (Exception) { /* Parent observes transport failure and process exit. */ }
            return 1;
        }
        finally
        {
            transfers.Cancel(); await connection.DisposeAsync().ConfigureAwait(false);
            foreach (Task task in tasks.Append(receive))
                _ = task.ContinueWith(t => { _ = t.Exception; }, CancellationToken.None,
                    TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            if (released) fileSystem?.Dispose();
        }
    }
    private static async Task StopObservedAsync(MachineRun run)
    {
        try { await run.StopAsync().ConfigureAwait(false); }
        catch (MachineStopTimeoutException) { /* Controller retains explicit Kill. */ }
        catch (Exception) { /* The run's completion/transport owns its outcome. */ }
    }
}
