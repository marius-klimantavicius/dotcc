using System.Collections.Concurrent;
using System.Net;
using System.Runtime.InteropServices;
using Managed.Database;
using CoreLibc = Managed.Database.ValkeyCore.Libc;

namespace Managed.Valkey;

/// <summary>Owns one translated Valkey server and its dedicated executor thread.
/// Every entry binds that server's runtime synchronously; no binding crosses await.</summary>
public sealed class ValkeyServer : IAsyncDisposable
{
    private sealed record StopRequest(ValkeyShutdownMode Mode, TaskCompletionSource Completion);
    private readonly ValkeyOptions.Snapshot options;
    private readonly TaskCompletionSource<IPEndPoint> ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly ConcurrentQueue<StopRequest> stops = new();
    private readonly AutoResetEvent wake = new(false);
    private readonly object gate = new();
    private readonly Thread executor;
    private ValkeyCore? core;
    private bool terminal, running;
    private StopRequest? activeStop;
    // Failed cleanup must not let an unreachable startup failure lose its owner.
    // Such instances are retained, with their ID exposed by the cleanup error.
    private static readonly ConcurrentDictionary<Guid, ValkeyServer> quarantined = new();

    public Guid InstanceId { get; } = Guid.NewGuid();
    public Task Ready => ready.Task;
    public Task<IPEndPoint> Endpoint => ready.Task;
    /// <summary>Completes after worker joins and owner disposal; faults on terminal failure.</summary>
    public Task Completion => completion.Task;
    public bool IsRunning => Volatile.Read(ref running);
    public bool IsQuarantined { get; private set; }

    public ValkeyServer(ValkeyOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        this.options = options.Validate();
        executor = new Thread(Run) { IsBackground = true, Name = "Valkey " + InstanceId.ToString("N")[..8] };
        try { executor.Start(); }
        catch { wake.Dispose(); throw; }
    }

    public static async Task<ValkeyServer> StartAsync(ValkeyOptions options)
    {
        var server = new ValkeyServer(options);
        await server.Ready.ConfigureAwait(false);
        return server;
    }

    /// <summary>Queue an upstream SAVE/NOSAVE shutdown. Cancellation stops waiting
    /// for this request; it does not interrupt translated code or revoke the request.
    /// A failed SAVE faults this task while the server continues serving clients.</summary>
    public Task StopAsync(ValkeyShutdownMode mode = ValkeyShutdownMode.Save, CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(mode)) throw new ArgumentOutOfRangeException(nameof(mode));
        cancellationToken.ThrowIfCancellationRequested();
        Task pending;
        lock (gate)
        {
            if (terminal) pending = completion.Task;
            else
            {
                var request = new StopRequest(mode, new(TaskCreationOptions.RunContinuationsAsynchronously));
                stops.Enqueue(request);
                pending = request.Completion.Task;
                wake.Set();
            }
        }
        return cancellationToken.CanBeCanceled ? pending.WaitAsync(cancellationToken) : pending;
    }

    /// <summary>Uses DisposeMode. If a final SAVE fails, disposal reports the
    /// failure and retains the running server; callers can retry or explicitly
    /// request NoSave with StopAsync.</summary>
    public ValueTask DisposeAsync() => new(StopAsync(options.DisposeMode));

    private unsafe void Run()
    {
        Exception? failure = null;
        bool cleanStop = false;
        try
        {
            Directory.CreateDirectory(options.Directory);
            core = new ValkeyCore();
            using (core.__DotCcEnter())
            fixed (byte* directory = options.DirectoryUtf8)
            fixed (byte* configuration = options.ConfigurationUtf8)
            {
                if (CoreLibc.chdir(directory) != 0)
                    throw new ValkeyException($"Unable to select the Valkey data directory (errno {CoreLibc.errno}).");
                if (ValkeyHost.Start(core, configuration) != 0)
                    throw new ValkeyException(ReadError("Valkey startup failed."));
                int port = ValkeyHost.Port(core);
                if (ValkeyHost.State(core) != 2 || port is < 1 or > 65535)
                    throw new ValkeyException("Valkey startup did not publish a listening server.");
                if (core.__DotCcRuntime.Termination is { } startupTermination)
                    throw TerminalFailure(startupTermination);
                Volatile.Write(ref running, true);
                ready.TrySetResult(new IPEndPoint(options.Address, port));
            }

            while (true)
            {
                using (core.__DotCcEnter())
                {
                    if (core.__DotCcRuntime.Termination is { } termination)
                    {
                        if (!IsCleanExit(termination)) throw TerminalFailure(termination);
                        cleanStop = true;
                        break;
                    }
                    if (ValkeyHost.State(core) == 3) { cleanStop = true; break; }
                    if (stops.TryDequeue(out activeStop))
                    {
                        int flags = activeStop.Mode == ValkeyShutdownMode.Save
                            ? ValkeyCore.SHUTDOWN_SAVE : ValkeyCore.SHUTDOWN_NOSAVE;
                        if (ValkeyHost.Stop(core, flags) == 0) { cleanStop = true; break; }
                        activeStop.Completion.TrySetException(new ValkeyStopException(ReadError("Valkey refused shutdown.")));
                        activeStop = null;
                    }
                    if (ValkeyHost.ProcessEvents(core) < 0)
                        throw new ValkeyException(ReadError("Valkey event processing failed."));
                    if (ValkeyHost.State(core) == 3) { cleanStop = true; break; }
                }
                // The bridge performs a nonblocking upstream event-loop turn.
                // This wait bounds idle polling and wakes immediately for Stop.
                wake.WaitOne(2);
            }
        }
        catch (CoreLibc.RuntimeTerminationException termination) when (core is not null && ReferenceEquals(termination.Owner, core.__DotCcRuntime))
        {
            try
            {
                using (core.__DotCcEnter()) cleanStop = IsCleanExit(termination.Termination);
                if (!cleanStop) failure = TerminalFailure(termination.Termination);
            }
            catch (Exception error) { failure = error; }
        }
        catch (Exception error) { failure = error; }
        finally
        {
            Volatile.Write(ref running, false);
            if (core is not null)
            {
                try
                {
                    using (core.__DotCcEnter())
                    {
                        if (core.__DotCcRuntime.Termination is { } beforeCleanup && !IsCleanExit(beforeCleanup))
                        {
                            cleanStop = false;
                            failure ??= TerminalFailure(beforeCleanup);
                        }
                        if (ValkeyHost.Cleanup(core, cleanStop ? 0 : 1) != 0)
                            throw new ValkeyException(ReadError("Valkey workers could not all be joined."));
                        // Joining can reveal a worker fault that raced with Stop.
                        if (core.__DotCcRuntime.Termination is { } joinedTermination &&
                            !(cleanStop && joinedTermination.Kind == CoreLibc.RuntimeTerminationKind.Exit && joinedTermination.Status == 0))
                            failure ??= TerminalFailure(joinedTermination);
                    }
                    // All translated bindings and workers have ended before this.
                    core.Dispose();
                    core = null;
                }
                catch (Exception cleanupError)
                {
                    IsQuarantined = true;
                    quarantined[InstanceId] = this;
                    failure = new ValkeyCleanupException(InstanceId,
                        $"Valkey owner {InstanceId} was retained because cleanup did not finish.",
                        failure is null ? cleanupError : new AggregateException(failure, cleanupError));
                }
            }
            Finish(failure);
        }
    }

    private bool IsCleanExit(CoreLibc.RuntimeTermination termination) =>
        termination.Kind == CoreLibc.RuntimeTerminationKind.Exit && termination.Status == 0 && ValkeyHost.State(core!) == 3;

    private static ValkeyException TerminalFailure(CoreLibc.RuntimeTermination termination) =>
        new($"Valkey terminated with {termination.Kind} (status {termination.Status}).", termination.Fault);

    // Caller holds the owning runtime binding; copy text before cleanup frees it.
    private unsafe string ReadError(string fallback) =>
        Marshal.PtrToStringUTF8((nint)ValkeyHost.LastError(core!)) is { Length: > 0 } message ? message : fallback;

    private void Finish(Exception? failure)
    {
        lock (gate)
        {
            terminal = true;
            if (!ready.Task.IsCompleted)
                ready.TrySetException(failure ?? new ValkeyException("Valkey stopped before startup completed."));
            if (failure is null)
            {
                activeStop?.Completion.TrySetResult();
                while (stops.TryDequeue(out var request)) request.Completion.TrySetResult();
                completion.TrySetResult();
            }
            else
            {
                activeStop?.Completion.TrySetException(failure);
                while (stops.TryDequeue(out var request)) request.Completion.TrySetException(failure);
                completion.TrySetException(failure);
            }
            wake.Dispose();
        }
    }
}
