using Managed.Emulation.Host;

namespace Managed.Emulation;

/// <summary>A persistent guest namespace and immutable execution policy. One run
/// may use a machine at a time; independent machines may execute concurrently.</summary>
public sealed class BlinkMachine : IAsyncDisposable
{
    private sealed record Mount(string GuestPath, string? HostPath, MountAccess Access,
        IGuestFileSystem Store, bool Replace);
    private readonly object gate = new();
    private readonly MachineOptions options;
    private readonly List<Mount> mounts = [];
    private readonly List<string> privateDirectories = [];
    private readonly IGuestFileSystem root;
    private readonly MountedFileSystem fileSystem;
    private readonly string? rootPath;
    private MachineRun? active;
    private bool disposed, disposing;
    private Task? disposal;
    private MachineState state;

    public BlinkMachine(MachineOptions? options = null)
    {
        this.options = (options ?? new()).Freeze();
        (root, rootPath) = CreatePrivateStore();
        fileSystem = new(root, ownsRoot: true, privateWritableLimit: this.options.WritableStorageLimit,
            privateNodeLimit: this.options.FileNodeLimit);
    }
    public ExecutionMode ExecutionMode => options.ExecutionMode;
    public MachineCapabilities Capabilities => new(options.ExecutionMode, CooperativeStop: true,
        ForceTermination: options.ExecutionMode == ExecutionMode.SeparateProcess, HardenedSandbox: false,
        PersistentHostUnixModes: HostDirectoryFileSystem.SupportsPersistentUnixModes, HostWideResourceEnforcement: false);
    public MachineState State { get { lock (gate) return disposed ? MachineState.Disposed : active?.Completion.IsCompleted == true ? MachineState.Exited : state; } }
    private void Idle()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (disposing) throw new InvalidOperationException("Machine disposal is in progress.");
        if (active != null && (!active.Completion.IsCompleted || !active.Completion.GetAwaiter().GetResult().ResourcesReleased))
            throw new InvalidOperationException("Machine resources are still owned by an execution.");
    }
    private (IGuestFileSystem Store, string? Path) CreatePrivateStore()
    {
        if (options.ExecutionMode == ExecutionMode.InProcess)
            return (new VirtualFileSystem(new Dictionary<string, ReadOnlyMemory<byte>>(),
                options.WritableStorageLimit, options.DescriptorLimit, nodeLimit: options.FileNodeLimit), null);
        string path = Directory.CreateTempSubdirectory("blink-machine-").FullName;
        privateDirectories.Add(path);
        return (HostDirectoryFileSystem.OpenRoot(path, descriptorLimit: options.DescriptorLimit,
            writableLimit: options.WritableStorageLimit, nodeLimit: options.FileNodeLimit), path);
    }
    /// <summary>Imports files into private storage. Imported bytes count against
    /// the private storage limit and remain mutable across executions.</summary>
    public void ImportImage(IEnumerable<ImageFile> files)
    {
        ArgumentNullException.ThrowIfNull(files);
        lock (gate)
        {
            Idle();
            var image = files.ToArray();
            if (image.Any(f => f == null || f.Contents == null)) throw new ArgumentException("Invalid image.");
            foreach (var file in image) GuestText.Path(file.Path);
            if (image.Any(file => mounts.Any(m => file.Path == m.GuestPath || file.Path.StartsWith(m.GuestPath + "/", StringComparison.Ordinal))))
                throw new ArgumentException("Image import targets private root paths outside mounted stores.");
            using var source = new VirtualFileSystem(image.ToDictionary(f => f.Path, f => (ReadOnlyMemory<byte>)f.Contents),
                imageLimit: options.WritableStorageLimit, executablePaths: image.Where(f => f.Executable).Select(f => f.Path).ToHashSet(),
                nodeLimit: options.FileNodeLimit);
            GuestFileSystemCopy.Copy(source, fileSystem, options.WritableStorageLimit, options.FileNodeLimit);
        }
    }
    /// <summary>Grants a live host directory, read/write by default. Portable BCL
    /// path checks do not provide atomic containment against hostile host changes.</summary>
    public void MountDirectory(string guestPath, string hostPath, MountAccess access = MountAccess.ReadWrite,
        bool replaceExistingDirectory = false)
    {
        GuestText.Path(guestPath);
        if (!Enum.IsDefined(access)) throw new ArgumentException("A valid access mode is required.");
        hostPath = Path.GetFullPath(hostPath);
        lock (gate)
        {
            Idle();
            IGuestFileSystem source = HostDirectoryFileSystem.OpenRoot(hostPath, readOnly: access != MountAccess.ReadWrite,
                descriptorLimit: options.DescriptorLimit, nodeLimit: options.FileNodeLimit);
            string? storePath = Path.GetFullPath(hostPath);
            try
            {
                if (access == MountAccess.CopyOnWrite)
                {
                    var copy = CreatePrivateStore();
                    try { GuestFileSystemCopy.Copy(source, copy.Store, options.WritableStorageLimit, options.FileNodeLimit); }
                    catch { copy.Store.Dispose(); throw; }
                    source.Dispose(); source = copy.Store; storePath = copy.Path;
                }
                fileSystem.Mount(guestPath, source, access == MountAccess.ReadOnly, ownsSource: true,
                    replaceExistingDirectory: replaceExistingDirectory, privateStorage: access == MountAccess.CopyOnWrite);
                mounts.Add(new(guestPath, storePath, access, source, replaceExistingDirectory));
            }
            catch { source.Dispose(); throw; }
        }
    }
    public void Unmount(string guestPath)
    {
        lock (gate) { Idle(); fileSystem.Unmount(guestPath); mounts.RemoveAll(m => m.GuestPath == guestPath); }
    }
    /// <summary>Discards private root and COW contents; live grants are untouched.
    /// COW contents are cleared, not silently refreshed from the host source.</summary>
    public void ResetPrivateStorage()
    {
        lock (gate)
        {
            Idle(); GuestFileSystemCopy.Clear(root, options.FileNodeLimit);
            foreach (var mount in mounts.Where(m => m.Access == MountAccess.CopyOnWrite)) GuestFileSystemCopy.Clear(mount.Store, options.FileNodeLimit);
            // Restore only namespace ancestors needed to reach retained grants.
            // Mount roots themselves are provided by their existing stores.
            foreach (var mount in mounts.OrderBy(m => m.GuestPath.Length))
            {
                string[] parts = mount.GuestPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                for (int i = 1; i < parts.Length; ++i)
                {
                    string parent = "/" + string.Join('/', parts.Take(i));
                    var exists = fileSystem.Stat(parent);
                    if (exists.Succeeded && exists.Value.Directory) continue;
                    if (exists.Error != GuestError.NoEntry) throw new IOException("Cannot restore mount ancestor: " + parent);
                    var created = fileSystem.MakeDirectory(parent, 0x1ed);
                    if (!created.Succeeded) throw new IOException("Cannot restore mount ancestor: " + created.Error);
                }
            }
        }
    }
    /// <summary>Exports one private store into an existing empty host directory.
    /// Failure can leave a partial export; existing destination files are never overwritten.</summary>
    public FileSystemCopyResult ExportPrivateStorage(string destination, string guestMount = "/")
    {
        lock (gate)
        {
            Idle();
            var store = guestMount == "/" ? root : mounts.Single(m => m.GuestPath == guestMount && m.Access == MountAccess.CopyOnWrite).Store;
            using var target = HostDirectoryFileSystem.OpenRoot(destination, nodeLimit: options.FileNodeLimit);
            return GuestFileSystemCopy.Copy(store, target, options.WritableStorageLimit, options.FileNodeLimit);
        }
    }
    public Task<MachineRun> StartAsync(ExecutionOptions execution, CancellationToken cancellation = default)
    {
        ArgumentNullException.ThrowIfNull(execution); cancellation.ThrowIfCancellationRequested();
        execution = execution.Freeze();
        if (execution.Readiness.Kind == ReadinessKind.ListeningPorts && options.Network.Publications.Length == 0)
            throw new ArgumentException("Listening-port readiness requires an explicit publication grant.");
        lock (gate)
        {
            Idle();
            var environment = new Dictionary<string, string>(options.Environment, StringComparer.Ordinal);
            foreach (var pair in execution.Environment)
                if (pair.Value == null) environment.Remove(pair.Key); else environment[pair.Key] = pair.Value;
            GuestText.EnvironmentSize(environment);
            var session = fileSystem.AcquireSession();
            try
            {
                active = options.ExecutionMode == ExecutionMode.InProcess
                    ? new InProcessMachineRun(options, execution, environment, session)
                    : new ProcessMachineRun(options, execution, environment, session, new MachineStorage(rootPath!,
                        mounts.Select(m => new MachineMount(m.GuestPath, m.HostPath!, m.Access, m.Replace)).ToArray()));
                state = MachineState.Running;
                return Task.FromResult(active);
            }
            catch { session.Dispose(); throw; }
        }
    }
    public async Task<MachineRunResult> ExecuteAsync(ExecutionOptions execution, CancellationToken cancellation = default)
    {
        ArgumentNullException.ThrowIfNull(execution);
        if (execution.Console.RedirectInput || execution.Console.RedirectOutput)
            throw new ArgumentException("Use StartAsync to consume manually redirected streams; ExecuteAsync accepts attached streams or bounded capture.");
        // Cancellation belongs to the wait. The caller can recover the active
        // run through CurrentRun and explicitly stop it.
        var run = await StartAsync(execution, cancellation).ConfigureAwait(false);
        var result = await run.WaitAsync(cancellation).ConfigureAwait(false);
        await run.DisposeAsync().ConfigureAwait(false);
        return result;
    }
    public MachineRun? CurrentRun { get { lock (gate) return active; } }
    public ValueTask DisposeAsync()
    {
        MachineRun? run;
        TaskCompletionSource completion;
        lock (gate)
        {
            if (disposal != null) return new(disposal);
            completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
            disposal = completion.Task;
            disposing = true; run = active;
        }
        _ = DisposeCoreAsync(run, completion);
        return new(completion.Task);
    }
    private async Task DisposeCoreAsync(MachineRun? run, TaskCompletionSource completion)
    {
        try
        {
            if (run != null) await run.DisposeAsync().ConfigureAwait(false);
            lock (gate)
            {
                // Once storage cleanup begins, configuration stays closed even
                // if a later disposal attempt must finish removing its paths.
                disposed = true; fileSystem.Dispose();
                // Retain unremoved paths for a retry if host cleanup fails.
                while (privateDirectories.Count != 0)
                {
                    string directory = privateDirectories[^1];
                    if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
                    privateDirectories.RemoveAt(privateDirectories.Count - 1);
                }
                disposed = true; disposing = false;
                completion.TrySetResult();
            }
        }
        catch (Exception error)
        {
            lock (gate) { disposal = null; disposing = false; completion.TrySetException(error); }
        }
    }
}
