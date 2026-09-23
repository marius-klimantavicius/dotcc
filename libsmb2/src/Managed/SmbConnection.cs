using System.Collections.Concurrent;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using static Managed.Smb.LibSmb2;

namespace Managed.Smb;

public sealed class SmbException(string operation, int status, string message)
    : IOException($"{operation}: {message} (status {status})")
{
    public int Status { get; } = status;
}

/// <summary>Owns one translated SMB context. Operations are serialized and driven
/// by socket completions. Cancellation of a submitted request waits for its callback
/// or terminal context cleanup before releasing buffers.</summary>
public sealed partial class SmbConnection : IDisposable, IAsyncDisposable
{
    private static readonly object ContextRegistry = new();
    private static readonly ConcurrentDictionary<nint, WeakReference<SmbConnection>> Connections = new();
    private readonly ContextExecutor _executor = new();
    private readonly SemaphoreSlim _operations = new(1);
    private readonly HashSet<SmbFile> _files = new();
    private readonly object _disposeGate = new();
    private readonly ConcurrentDictionary<int, byte> _notifications = new();
    private nint _context;
    private bool _connected;
    private int _timeoutSeconds;
    private int _disposing;
    private Task? _disposeTask;
    private Task _transportDrain = Task.CompletedTask;
    private Operation? _pending;
    private Timer? _connectTimer;
    private long _connectTimerVersion;

    private SmbConnection(int timeoutSeconds) => _timeoutSeconds = timeoutSeconds;
    private unsafe smb2_context* Context => (smb2_context*)_context;

    public static SmbConnection Connect(string server, string share, string user,
        string password, string domain = "WORKGROUP", ushort dialect = (ushort)smb2_negotiate_version.SMB2_VERSION_0311,
        bool encrypt = false, int timeoutSeconds = 10)
        => ConnectAsync(server, share, user, password, domain, dialect, encrypt, timeoutSeconds).GetAwaiter().GetResult();

    public static async Task<SmbConnection> ConnectAsync(string server, string share, string user,
        string password, string domain = "WORKGROUP", ushort dialect = (ushort)smb2_negotiate_version.SMB2_VERSION_0311,
        bool encrypt = false, int timeoutSeconds = 10, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(timeoutSeconds, 1);
        // Keep the original server identity, including a possible port suffix, for SMB.
        var addressName = DnsName(server);
        var addresses = await Dns.GetHostAddressesAsync(addressName, cancellationToken).ConfigureAwait(false);
        var connection = new SmbConnection(timeoutSeconds);
        try
        {
            await connection._executor.Invoke(connection.Initialize).ConfigureAwait(false);
            await connection.RunOperationAsync("connect", operation => connection.StartConnect(operation,
                server, share, user, password, domain, dialect, encrypt, addresses), operation =>
                {
                    connection._connected = true;
                    return true;
                }, cancellationToken).ConfigureAwait(false);
            if (connection.Dialect != dialect) throw new IOException("Server negotiated a different SMB dialect");
            return connection;
        }
        catch
        {
            try { await connection.DisposeAsync().ConfigureAwait(false); } catch { }
            throw;
        }
    }

    private static string DnsName(string server)
    {
        ArgumentException.ThrowIfNullOrEmpty(server);
        if (server[0] == '[')
        {
            int end = server.IndexOf(']');
            if (end > 0) return server[1..end];
        }
        int colon = server.LastIndexOf(':');
        return colon > 0 && server.IndexOf(':') == colon ? server[..colon] : server;
    }

    private unsafe void Initialize()
    {
        lock (ContextRegistry) _context = (nint)smb2_init_context();
        if (_context == 0) throw new OutOfMemoryException("SMB context allocation failed");
        var weak = new WeakReference<SmbConnection>(this);
        Connections[_context] = weak;
        HostSockets.RegisterContext(_context, socket => { if (weak.TryGetTarget(out var owner)) owner.Notify(socket); });
        using var scope = HostSockets.EnterContext(_context);
        smb2_fd_event_callbacks(Context, &ChangeFd, &ChangeEvents);
        smb2_set_timeout(Context, 0);
    }

    private unsafe int StartConnect(Operation op, string server, string share, string user, string password,
        string domain, ushort dialect, bool encrypt, IPAddress[] addresses)
    {
        smb2_set_version(Context, (smb2_negotiate_version)dialect);
        smb2_set_authentication(Context, (int)smb2_sec.SMB2_SEC_NTLMSSP);
        smb2_set_domain(Context, op.Text(domain));
        smb2_set_user(Context, op.Text(user));
        smb2_set_password(Context, op.Text(password, secret: true));
        smb2_set_security_mode(Context, SMB2_NEGOTIATE_SIGNING_ENABLED);
        smb2_set_sign(Context, 1);
        if (encrypt) smb2_set_seal(Context, 1);
        using var prepared = HostSockets.PreparedAddresses(DnsName(server), addresses);
        return smb2_connect_share_async(Context, op.Text(server), op.Text(share), op.Text(user), &Complete, op.Token);
    }

    public ushort Dialect => Block(() => Turn(() => { RequireContext(); return ReadDialect(); }));
    private unsafe ushort ReadDialect() => smb2_get_dialect(Context);

    public SmbFile Open(string path, bool create = false) => Block(() => OpenAsync(path, create));
    public Task<SmbFile> OpenAsync(string path, bool create = false, CancellationToken cancellationToken = default)
        => RunOperationAsync("open", op => StartOpen(op, path, create), AdoptFile, cancellationToken, Ownership.File);
    private unsafe int StartOpen(Operation op, string path, bool create)
        => smb2_open_async(Context, op.Text(path), create ? O_RDWR | O_CREAT | O_EXCL : O_RDWR, &Complete, op.Token);
    private unsafe SmbFile AdoptFile(Operation op)
    {
        var file = new SmbFile(this, op.Data);
        _files.Add(file);
        op.Data = 0;
        return file;
    }

    public IReadOnlyList<Entry> List(string path = "") => Block(() => ListAsync(path));
    public Task<IReadOnlyList<Entry>> ListAsync(string path = "", CancellationToken cancellationToken = default)
        => RunOperationAsync("opendir", op => StartList(op, path), ReadDirectory, cancellationToken, Ownership.Directory);
    private unsafe int StartList(Operation op, string path) => smb2_opendir_async(Context, op.Text(path), &Complete, op.Token);
    private unsafe IReadOnlyList<Entry> ReadDirectory(Operation op)
    {
        var directory = (smb2dir*)op.Data;
        try
        {
            var entries = new List<Entry>();
            smb2dirent* entry;
            while ((entry = smb2_readdir(Context, directory)) != null)
                entries.Add(new(Marshal.PtrToStringUTF8((nint)entry->name) ?? "", entry->st.smb2_size,
                    entry->st.smb2_type == SMB2_TYPE_DIRECTORY));
            return entries;
        }
        finally { smb2_closedir(Context, directory); op.Data = 0; }
    }

    public void Delete(string path) => Block(() => DeleteAsync(path));
    public Task DeleteAsync(string path, CancellationToken cancellationToken = default)
        => SimpleAsync("unlink", op => StartPath(op, path, "unlink"), cancellationToken);
    public void CreateDirectory(string path) => Block(() => CreateDirectoryAsync(path));
    public Task CreateDirectoryAsync(string path, CancellationToken cancellationToken = default)
        => SimpleAsync("mkdir", op => StartPath(op, path, "mkdir"), cancellationToken);
    public void RemoveDirectory(string path) => Block(() => RemoveDirectoryAsync(path));
    public Task RemoveDirectoryAsync(string path, CancellationToken cancellationToken = default)
        => SimpleAsync("rmdir", op => StartPath(op, path, "rmdir"), cancellationToken);
    private unsafe int StartPath(Operation op, string path, string action) => action switch
    {
        "unlink" => smb2_unlink_async(Context, op.Text(path), &Complete, op.Token),
        "mkdir" => smb2_mkdir_async(Context, op.Text(path), &Complete, op.Token),
        _ => smb2_rmdir_async(Context, op.Text(path), &Complete, op.Token)
    };
    public void Rename(string path, string newPath) => Block(() => RenameAsync(path, newPath));
    public Task RenameAsync(string path, string newPath, CancellationToken cancellationToken = default)
        => SimpleAsync("rename", op => StartRename(op, path, newPath), cancellationToken);
    private unsafe int StartRename(Operation op, string path, string newPath)
        => smb2_rename_async(Context, op.Text(path), op.Text(newPath), &Complete, op.Token);

    public Metadata Stat(string path) => Block(() => StatAsync(path));
    public Task<Metadata> StatAsync(string path, CancellationToken cancellationToken = default)
        => RunOperationAsync("stat", op => StartStat(op, path), ReadMetadata, cancellationToken);
    private unsafe int StartStat(Operation op, string path)
    {
        op.Output = op.Allocate(sizeof(smb2_stat_64));
        return smb2_stat_async(Context, op.Text(path), (smb2_stat_64*)op.Output, &Complete, op.Token);
    }
    private static unsafe Metadata ReadMetadata(Operation op) => Metadata.From(*(smb2_stat_64*)op.Output);
    public SpaceInfo GetSpaceInfo(string path = "") => Block(() => GetSpaceInfoAsync(path));
    public Task<SpaceInfo> GetSpaceInfoAsync(string path = "", CancellationToken cancellationToken = default)
        => RunOperationAsync("statvfs", op => StartSpace(op, path), ReadSpace, cancellationToken);
    private unsafe int StartSpace(Operation op, string path)
    {
        op.Output = op.Allocate(sizeof(__DotCcTags.smb2_statvfs));
        return smb2_statvfs_async(Context, op.Text(path), (__DotCcTags.smb2_statvfs*)op.Output, &Complete, op.Token);
    }
    private static unsafe SpaceInfo ReadSpace(Operation op)
    {
        var value = *(__DotCcTags.smb2_statvfs*)op.Output;
        return new(value.f_bsize, value.f_frsize, value.f_blocks, value.f_bfree, value.f_bavail);
    }

    private Task SimpleAsync(string name, Func<Operation, int> submit, CancellationToken token, bool disposing = false,
        Action? validate = null)
        => RunOperationAsync(name, submit, static _ => true, token, disposing: disposing, validate: validate);

    private async Task<T> RunOperationAsync<T>(string name, Func<Operation, int> submit,
        Func<Operation, T> consume, CancellationToken token, Ownership ownership = Ownership.None, bool disposing = false,
        Action? validate = null)
    {
        if (!disposing) ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposing) != 0, this);
        await _operations.WaitAsync(token).ConfigureAwait(false);
        using var op = new Operation(name, ownership);
        T result;
        try
        {
            await Turn(() =>
            {
                token.ThrowIfCancellationRequested();
                if (!disposing) ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposing) != 0, this);
                RequireContext();
                validate?.Invoke();
                _pending = op;
                try
                {
                    int started = submit(op);
                    if (started < 0) Terminate(Error(name, started));
                    else
                    {
                        op.Deadline = new Timer(_ => _executor.Post(() =>
                        {
                            if (ReferenceEquals(_pending, op) && !op.Finished)
                                InContext(() => Terminate(new SmbException(name, -Libc.ETIMEDOUT, "SMB operation timed out")));
                        }), null, TimeSpan.FromSeconds(_timeoutSeconds), Timeout.InfiniteTimeSpan);
                        ScheduleConnectDeadline();
                    }
                }
                catch (Exception error) { Terminate(error); }
            }).ConfigureAwait(false);
            await op.Completion.Task.ConfigureAwait(false);
            result = await Turn(() =>
            {
                if (op.Failure != null) throw op.Failure;
                if (op.Status < 0) throw Error(name, op.Status);
                // Cancellation is observed after successful-result ownership is settled.
                return consume(op);
            }).ConfigureAwait(false);

        }
        finally
        {
            try
            {
                await Turn(() =>
                {
                    op.Deadline?.Dispose();
                    CleanupResult(op);
                    if (ReferenceEquals(_pending, op)) _pending = null;
                }).ConfigureAwait(false);
                await _transportDrain.ConfigureAwait(false);
            }
            finally { _operations.Release(); }
        }
        if (token.IsCancellationRequested)
        {
            // Settle an acquired handle before reporting cancellation to its caller.
            if (result is SmbFile file) await file.DisposeAsyncCore(disposing: true).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
        }
        return result;
    }

    private Task Turn(Action action) => _executor.Invoke(() => InContext(action));
    private Task<T> Turn<T>(Func<T> action) => _executor.Invoke(() =>
    {
        if (_context == 0) return action();
        using var scope = HostSockets.EnterContext(_context);
        return action();
    });
    private void InContext(Action action)
    {
        if (_context == 0) { action(); return; }
        using var scope = HostSockets.EnterContext(_context);
        action();
    }
    private T Block<T>(Func<Task<T>> operation)
    {
        if (_executor.IsCurrent) throw new InvalidOperationException("Synchronous SMB operations cannot run inside the context executor");
        return operation().GetAwaiter().GetResult();
    }
    private void Block(Func<Task> operation)
    {
        if (_executor.IsCurrent) throw new InvalidOperationException("Synchronous SMB operations cannot run inside the context executor");
        operation().GetAwaiter().GetResult();
    }
    private void RequireContext() => ObjectDisposedException.ThrowIf(_context == 0, this);
    private unsafe SmbException Error(string operation, int status) => new(operation, status,
        _context == 0 ? "SMB context closed" : Marshal.PtrToStringUTF8((nint)smb2_get_error(Context)) ?? "SMB operation failed");

    private enum Ownership { None, File, Directory }
    private sealed class Operation : IDisposable
    {
        internal readonly string Name;
        internal readonly Ownership Ownership;
        internal readonly TaskCompletionSource Completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal readonly List<(nint Pointer, int Length, bool Secret)> Allocations = new();
        private GCHandle _root;
        internal Timer? Deadline;
        internal bool Finished;
        internal int Status;
        internal nint Data;
        internal nint Output;
        internal Exception? Failure;
        internal Operation(string name, Ownership ownership) { Name = name; Ownership = ownership; _root = GCHandle.Alloc(this); }
        internal unsafe void* Token => (void*)GCHandle.ToIntPtr(_root);
        internal unsafe nint Allocate(int length, bool secret = false)
        {
            var pointer = (nint)NativeMemory.AllocZeroed((nuint)Math.Max(1, length));
            if (pointer == 0) throw new OutOfMemoryException();
            Allocations.Add((pointer, length, secret));
            return pointer;
        }
        internal unsafe byte* Text(string value, bool secret = false)
        {
            ArgumentNullException.ThrowIfNull(value);
            if (value.Contains('\0')) throw new ArgumentException("SMB text cannot contain NUL", nameof(value));
            int length = Encoding.UTF8.GetByteCount(value);
            byte* pointer = (byte*)Allocate(checked(length + 1), secret);
            Encoding.UTF8.GetBytes(value, new Span<byte>(pointer, length));
            return pointer;
        }
        public unsafe void Dispose()
        {
            Deadline?.Dispose();
            foreach (var allocation in Allocations)
            {
                if (allocation.Secret) System.Security.Cryptography.CryptographicOperations.ZeroMemory(new Span<byte>((void*)allocation.Pointer, allocation.Length));
                NativeMemory.Free((void*)allocation.Pointer);
            }
            _root.Free();
        }
    }
    private static unsafe void Complete(smb2_context* context, int status, void* data, void* privateData)
    {
        // Root lifetime extends through callback completion or destruction and transport drain.
        try
        {
            var op = (Operation)GCHandle.FromIntPtr((nint)privateData).Target!;
            op.Status = status;
            op.Data = (nint)data;
            op.Finished = true;
            op.Completion.TrySetResult();
        }
        catch (Exception error) { CallbackFailed((nint)context, error); }
    }

    private static unsafe void ChangeFd(smb2_context* context, t_socket fd, int command)
    {
        try
        {
            if (command == SMB2_ADD_FD) HostSockets.Add((nint)context, fd);
            else if (command == SMB2_DEL_FD) HostSockets.Remove((nint)context, fd);
        }
        catch (Exception error) { CallbackFailed((nint)context, error); }
    }
    private static unsafe void ChangeEvents(smb2_context* context, t_socket fd, int events)
    {
        try { HostSockets.SetEvents((nint)context, fd, events); }
        catch (Exception error) { CallbackFailed((nint)context, error); }
    }
    private static void CallbackFailed(nint context, Exception error)
    {
        if (Connections.TryGetValue(context, out var weak) && weak.TryGetTarget(out var owner))
            owner._executor.Post(() => owner.InContext(() => owner.Terminate(error)));
    }
    private void Notify(t_socket fd)
    {
        int token = (int)fd;
        if (!_notifications.TryAdd(token, 0)) return;
        _executor.Post(() =>
        {
            _notifications.TryRemove(token, out _);
            if (_context == 0) return;
            InContext(() => Service(fd));
        });
    }
    private unsafe void Service(t_socket fd)
    {
        try
        {
            int events = HostSockets.Events(_context, fd);
            if (events == 0) return;
            int result = smb2_service_fd(Context, fd, events);
            if (result < 0 && !(_pending is { Name: "disconnect", Finished: true, Status: >= 0 }))
                Terminate(Error(_pending?.Name ?? "service", result));
            if (_context != 0)
            {
                HostSockets.Rearm(_context, fd);
                ScheduleConnectDeadline();
            }
        }
        catch (Exception error) { Terminate(error); }
    }
    private unsafe void ScheduleConnectDeadline()
    {
        _connectTimer?.Dispose();
        _connectTimer = null;
        long version = ++_connectTimerVersion;
        if (_context == 0) return;
        ulong count = 0;
        int delay = -1;
        smb2_get_fds(Context, &count, &delay);
        if (delay < 0) return;
        _connectTimer = new Timer(_ => _executor.Post(() =>
        {
            if (_context == 0 || version != _connectTimerVersion) return;
            InContext(ConnectDeadline);
        }), null, delay, Timeout.Infinite);
    }
    private unsafe void ConnectDeadline()
    {
        try
        {
            if (smb2_service_fd(Context, -1, 0) < 0) Terminate(Error("connect", -Libc.EIO));
            else ScheduleConnectDeadline();
        }
        catch (Exception error) { Terminate(error); }
    }
    private void Terminate(Exception error)
    {
        if (_pending is { } op) op.Failure ??= error;
        DestroyContext();
        _pending?.Completion.TrySetResult();
    }
    private unsafe void CleanupResult(Operation op)
    {
        if (!op.Finished || op.Status < 0 || op.Data == 0) return;
        if (op.Ownership == Ownership.File) FreeHandle((smb2fh*)op.Data);
        else if (op.Ownership == Ownership.Directory) smb2_closedir(Context, (smb2dir*)op.Data);
        op.Data = 0;
    }
    private static unsafe void FreeHandle(smb2fh* handle) { Libc.free(handle->path); Libc.free(handle); }
    private unsafe void DestroyContext()
    {
        if (_context == 0) return;
        var context = _context;
        _connectTimer?.Dispose();
        _connectTimer = null;
        ++_connectTimerVersion;
        // A successful result may have arrived in the service turn that discovered failure.
        if (_pending is { } before) CleanupResult(before);
        // Keep address retirement under the same lock as allocation. The native
        // allocator may immediately reuse a destroyed context address on another
        // connection; its host registration must be retired before that happens.
        lock (ContextRegistry)
        {
            try { smb2_destroy_context((smb2_context*)context); }
            finally
            {
                _context = 0;
                _connected = false;
                Connections.TryRemove(context, out _);
                foreach (var file in _files) file.ReleaseAfterDestroy();
                _files.Clear();
                _transportDrain = HostSockets.DrainContextAsync(context);
            }
        }
    }

    public void Dispose() => Block(() => DisposeAsync().AsTask());
    public ValueTask DisposeAsync()
    {
        lock (_disposeGate)
        {
            Interlocked.Exchange(ref _disposing, 1);
            return new(_disposeTask ??= DisposeCoreAsync());
        }
    }
    private async Task DisposeCoreAsync()
    {
        try
        {
            // Establish a barrier behind an active request before inspecting idle handles.
            await _operations.WaitAsync().ConfigureAwait(false);
            _operations.Release();
            var files = await Turn(() => _files.ToArray()).ConfigureAwait(false);
            foreach (var file in files) await file.DisposeAsyncCore(disposing: true).ConfigureAwait(false);
            if (_connected) await SimpleAsync("disconnect", StartDisconnect, default, disposing: true).ConfigureAwait(false);
        }
        finally
        {
            await Turn(DestroyContext).ConfigureAwait(false);
            await _transportDrain.ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }
    }
    private unsafe int StartDisconnect(Operation op) => smb2_disconnect_share_async(Context, &Complete, op.Token);
    ~SmbConnection()
    {
        // Local close/destruction only; finalization never performs a graceful exchange.
        _executor.Post(() => { try { InContext(DestroyContext); } catch { } });
    }

    public sealed record Entry(string Name, ulong Size, bool IsDirectory);
    public sealed record Metadata(ulong Size, ulong Inode, uint Type, uint Attributes)
    {
        internal static Metadata From(smb2_stat_64 value) => new(value.smb2_size, value.smb2_ino, value.smb2_type, value.smb2_attributes);
    }
    public sealed record SpaceInfo(uint BlockSize, uint FragmentSize, ulong Blocks, ulong FreeBlocks, ulong AvailableBlocks);
}
