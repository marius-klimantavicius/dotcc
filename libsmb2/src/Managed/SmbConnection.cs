using System.Runtime.InteropServices;
using System.Text;
using static Managed.Smb.LibSmb2;

namespace Managed.Smb;

public sealed class SmbException(string operation, int status, string message)
    : IOException($"{operation}: {message} (status {status})")
{
    public int Status { get; } = status;
}

/// <summary>Owns one translated SMB context. Operations on a connection are
/// serialized; independent connections may run concurrently. Cancellation waits
/// for an active upstream operation to drain before releasing its buffers.</summary>
public sealed unsafe class SmbConnection : IDisposable, IAsyncDisposable
{
    private static readonly object ContextRegistry = new();
    // O_CREAT/O_EXCL are not exported in this generated closure. These two
    // fallback values match the selected dotcc Linux fcntl.h ABI.
    private const int LinuxOpenCreate = 0x40;
    private const int LinuxOpenExclusive = 0x80;
    private readonly object _gate = new();
    private readonly HashSet<SmbFile> _files = new();
    private smb2_context* _context;
    private bool _connected;
    private int _timeoutSeconds = 10;

    private SmbConnection()
    {
        lock (ContextRegistry) _context = smb2_init_context();
        if (_context == null) throw new OutOfMemoryException("SMB context allocation failed");
    }

    public static SmbConnection Connect(string server, string share, string user,
        string password, string domain = "WORKGROUP", ushort dialect = (ushort)smb2_negotiate_version.SMB2_VERSION_0311,
        bool encrypt = false, int timeoutSeconds = 10)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(timeoutSeconds, 1);
        var connection = new SmbConnection();
        try
        {
            var serverBytes = Utf8(server); var shareBytes = Utf8(share);
            var userBytes = Utf8(user); var domainBytes = Utf8(domain); var passwordBytes = Utf8(password);
            try
            {
                fixed (byte* host = serverBytes, tree = shareBytes, username = userBytes,
                    workgroup = domainBytes, secret = passwordBytes)
                {
                    connection._timeoutSeconds = timeoutSeconds;
                    // One managed deadline owns cancellation of the entire operation.
                    // Do not let independent PDU timers release callback state early.
                    smb2_set_timeout(connection._context, 0);
                    smb2_set_version(connection._context, (smb2_negotiate_version)dialect);
                    smb2_set_authentication(connection._context, (int)smb2_sec.SMB2_SEC_NTLMSSP);
                    smb2_set_domain(connection._context, workgroup);
                    smb2_set_user(connection._context, username);
                    smb2_set_password(connection._context, secret);
                    smb2_set_security_mode(connection._context, SMB2_NEGOTIATE_SIGNING_ENABLED);
                    smb2_set_sign(connection._context, 1);
                    if (encrypt) smb2_set_seal(connection._context, 1);
                    CallbackState state = default;
                    using var pending = new OperationScope(connection, &state);
                    connection.Wait(smb2_connect_share_async(connection._context, host, tree, username, &Complete, &state), &state, "connect");
                    connection._connected = true;
                }
            }
            finally { System.Security.Cryptography.CryptographicOperations.ZeroMemory(passwordBytes); }
            if (connection.Dialect != dialect) throw new IOException("Server negotiated a different SMB dialect");
            return connection;
        }
        catch
        {
            try { connection.Dispose(); } catch { /* Preserve the connection failure. */ }
            throw;
        }
    }

    public static Task<SmbConnection> ConnectAsync(string server, string share, string user,
        string password, string domain = "WORKGROUP", ushort dialect = (ushort)smb2_negotiate_version.SMB2_VERSION_0311,
        bool encrypt = false, int timeoutSeconds = 10, CancellationToken cancellationToken = default)
        => Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            var connection = Connect(server, share, user, password, domain, dialect, encrypt, timeoutSeconds);
            if (cancellationToken.IsCancellationRequested)
            {
                try { connection.Dispose(); } catch { /* Preserve cancellation after cleanup. */ }
                cancellationToken.ThrowIfCancellationRequested();
            }
            return connection;
        }, cancellationToken);

    public ushort Dialect { get { lock (_gate) { RequireContext(); return smb2_get_dialect(_context); } } }

    public SmbFile Open(string path, bool create = false)
    {
        lock (_gate)
        {
            RequireContext();
            fixed (byte* name = Utf8(path))
            {
                CallbackState state = default;
                using var pending = new OperationScope(this, &state, ResultOwnership.File);
                Wait(smb2_open_async(_context, name, create ? O_RDWR | LinuxOpenCreate | LinuxOpenExclusive : O_RDWR, &Complete, &state),
                    &state, "open", ResultOwnership.File);
                var handle = (smb2fh*)state.Data;
                try
                {
                    var file = new SmbFile(this, handle);
                    _files.Add(file);
                    return file;
                }
                catch
                {
                    FreeHandle(handle);
                    DestroyContext();
                    throw;
                }
            }
        }
    }

    public IReadOnlyList<Entry> List(string path = "")
    {
        lock (_gate)
        {
            RequireContext();
            fixed (byte* name = Utf8(path))
            {
                CallbackState state = default;
                using var pending = new OperationScope(this, &state, ResultOwnership.Directory);
                Wait(smb2_opendir_async(_context, name, &Complete, &state), &state, "opendir", ResultOwnership.Directory);
                var directory = (smb2dir*)state.Data;
                try
                {
                    var result = new List<Entry>();
                    smb2dirent* entry;
                    while ((entry = smb2_readdir(_context, directory)) != null)
                        result.Add(new(Marshal.PtrToStringUTF8((nint)entry->name) ?? "", entry->st.smb2_size,
                            entry->st.smb2_type == SMB2_TYPE_DIRECTORY));
                    return result;
                }
                finally { smb2_closedir(_context, directory); }
            }
        }
    }

    public void Delete(string path)
    {
        lock (_gate)
        {
            RequireContext();
            CallbackState state = default;
            fixed (byte* name = Utf8(path))
            {
                using var pending = new OperationScope(this, &state);
                    Wait(smb2_unlink_async(_context, name, &Complete, &state), &state, "unlink");
            }
        }
    }

    public Metadata Stat(string path)
    {
        lock (_gate)
        {
            RequireContext();
            smb2_stat_64 value = default;
            CallbackState state = default;
            fixed (byte* name = Utf8(path))
            {
                using var pending = new OperationScope(this, &state);
                    Wait(smb2_stat_async(_context, name, &value, &Complete, &state), &state, "stat");
            }
            return Metadata.From(value);
        }
    }

    public void Rename(string path, string newPath)
    {
        lock (_gate)
        {
            RequireContext();
            CallbackState state = default;
            fixed (byte* oldName = Utf8(path), newName = Utf8(newPath))
            {
                using var pending = new OperationScope(this, &state);
                    Wait(smb2_rename_async(_context, oldName, newName, &Complete, &state), &state, "rename");
            }
        }
    }

    public void CreateDirectory(string path) => DirectoryOperation(path, true);
    public void RemoveDirectory(string path) => DirectoryOperation(path, false);
    private void DirectoryOperation(string path, bool create)
    {
        lock (_gate)
        {
            RequireContext();
            CallbackState state = default;
            fixed (byte* name = Utf8(path))
            {
                using var pending = new OperationScope(this, &state);
                    Wait(create ? smb2_mkdir_async(_context, name, &Complete, &state)
                        : smb2_rmdir_async(_context, name, &Complete, &state), &state, create ? "mkdir" : "rmdir");
            }
        }
    }

    public SpaceInfo GetSpaceInfo(string path = "")
    {
        lock (_gate)
        {
            RequireContext();
            LibSmb2.__DotCcTags.smb2_statvfs value = default;
            CallbackState state = default;
            fixed (byte* name = Utf8(path))
            {
                using var pending = new OperationScope(this, &state);
                    Wait(smb2_statvfs_async(_context, name, &value, &Complete, &state), &state, "statvfs");
            }
            return new(value.f_bsize, value.f_frsize, value.f_blocks, value.f_bfree, value.f_bavail);
        }
    }

    public Task<IReadOnlyList<Entry>> ListAsync(string path = "", CancellationToken cancellationToken = default)
        => RunAsync(() => List(path), cancellationToken);

    private Task<T> RunAsync<T>(Func<T> operation, CancellationToken token) => Task.Run(() =>
    {
        lock (_gate)
        {
            token.ThrowIfCancellationRequested();
            RequireContext();
            T value = operation();
            token.ThrowIfCancellationRequested();
            return value;
        }
    }, token);

    private static byte[] Utf8(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value.Contains('\0')) throw new ArgumentException("SMB text cannot contain NUL", nameof(value));
        return Encoding.UTF8.GetBytes(value + '\0');
    }
    private void RequireContext() => ObjectDisposedException.ThrowIf(_context == null, this);
    private SmbException Error(string operation, int status) => new(operation, status,
        Marshal.PtrToStringUTF8((nint)smb2_get_error(_context)) ?? "SMB operation failed");
    // This state lives on the caller's stack. Wait returns only after the actual
    // callback, or after destruction has synchronously cancelled every PDU.
    // The caller's pinned buffers and stack outputs remain valid throughout.
    private struct CallbackState
    {
        public bool Finished;
        public bool Settled;
        public int Status;
        public void* Data;
    }
    private enum ResultOwnership { None, File, Directory }
    private readonly ref struct OperationScope(SmbConnection owner, CallbackState* state,
        ResultOwnership ownership = ResultOwnership.None)
    {
        public void Dispose()
        {
            // Covers exceptions from submission itself, before Wait is entered.
            if (!state->Settled) owner.Abort(state, ownership);
        }
    }
    private void Abort(CallbackState* state, ResultOwnership ownership)
    {
        if (state->Finished && state->Status >= 0 && state->Data != null)
        {
            if (ownership == ResultOwnership.File) FreeHandle((smb2fh*)state->Data);
            else if (ownership == ResultOwnership.Directory) smb2_closedir(_context, (smb2dir*)state->Data);
            state->Data = null;
        }
        try { DestroyContext(); }
        finally { state->Settled = true; }
    }
    private static void Complete(smb2_context* context, int status, void* data, void* privateData)
    {
        var state = (CallbackState*)privateData;
        state->Status = status;
        state->Data = data;
        state->Finished = true;
    }

    private int Wait(int started, CallbackState* state, string operation,
        ResultOwnership ownership = ResultOwnership.None, bool closesTransport = false)
    {
        bool abort = started < 0;
        try
        {
            if (started < 0) throw Error(operation, started);
            long deadline = Environment.TickCount64 + (long)_timeoutSeconds * 1000;
            while (!state->Finished)
            {
                if (Environment.TickCount64 >= deadline)
                {
                    abort = true;
                    throw new SmbException(operation, -Libc.ETIMEDOUT, "SMB operation timed out");
                }
                ulong count = 0;
                int connectTimeout = -1;
                int* descriptors = smb2_get_fds(_context, &count, &connectTimeout);
                var polls = new pollfd[checked((int)count)];
                for (int i = 0; i < polls.Length; i++)
                    polls[i] = new pollfd { fd = descriptors[i], events = (short)smb2_which_events(_context) };
                int delay = (int)Math.Min(100, deadline - Environment.TickCount64);
                if (connectTimeout >= 0) delay = Math.Min(delay, connectTimeout);
                fixed (pollfd* fds = polls)
                {
                    if (Libc.poll(fds, count, Math.Max(0, delay)) < 0)
                    {
                        abort = true;
                        throw new SmbException(operation, -Libc.EIO, "SMB socket poll failed");
                    }
                    for (int i = 0; i < polls.Length && !state->Finished; i++)
                    {
                        if (fds[i].revents != 0 && smb2_service_fd(_context, fds[i].fd, fds[i].revents) < 0
                            && !(closesTransport && state->Finished && state->Status >= 0))
                        {
                            abort = true;
                            throw Error(operation, -Libc.EIO);
                        }
                    }
                }
                if (!state->Finished && connectTimeout >= 0 && smb2_service_fd(_context, -1, 0) < 0)
                {
                    abort = true;
                    throw Error(operation, -Libc.EIO);
                }
            }
            state->Settled = true;
            if (state->Status < 0) throw Error(operation, state->Status);
            return state->Status;
        }
        catch
        {
            if (abort || !state->Finished)
            {
                // Also releases successful Open/List results if service detected
                // a later failure before the caller could adopt the resource.
                Abort(state, ownership);
            }
            throw;
        }
    }

    private static void FreeHandle(smb2fh* handle)
    {
        // Mirrors upstream free_smb2fh. Idle handles are not owned by the
        // context; they must be released after its pending callbacks drain.
        Libc.free(handle->path);
        Libc.free(handle);
    }

    private void DestroyContext()
    {
        if (_context == null) return;
        var context = _context;
        _context = null;
        _connected = false;
        try { lock (ContextRegistry) smb2_destroy_context(context); }
        finally
        {
            foreach (var file in _files) file.ReleaseAfterDestroy();
            _files.Clear();
        }
    }

    public void Dispose()
    {
        DisposeCore();
        GC.SuppressFinalize(this);
    }
    private void DisposeCore(bool graceful = true)
    {
        lock (_gate)
        {
            if (_context == null) return;
            try
            {
                if (graceful)
                {
                    foreach (var file in _files.ToArray()) file.Dispose();
                    if (_connected)
                    {
                        CallbackState state = default;
                        using var pending = new OperationScope(this, &state);
                        // A successful disconnect callback closes its socket. The
                        // remaining service read can consequently report EBADF.
                        Wait(smb2_disconnect_share_async(_context, &Complete, &state), &state, "disconnect", closesTransport: true);
                    }
                }
            }
            finally
            {
                DestroyContext();
            }
        }
    }
    ~SmbConnection() { try { DisposeCore(graceful: false); } catch { /* Never throw on the finalizer thread. */ } }
    public ValueTask DisposeAsync() => new(Task.Run(Dispose));

    public sealed record Entry(string Name, ulong Size, bool IsDirectory);
    public sealed record Metadata(ulong Size, ulong Inode, uint Type, uint Attributes)
    {
        internal static Metadata From(smb2_stat_64 value) => new(value.smb2_size, value.smb2_ino,
            value.smb2_type, value.smb2_attributes);
    }
    public sealed record SpaceInfo(uint BlockSize, uint FragmentSize, ulong Blocks, ulong FreeBlocks, ulong AvailableBlocks);

    public sealed class SmbFile : IDisposable
    {
        private readonly SmbConnection _owner;
        private smb2fh* _handle;
        internal SmbFile(SmbConnection owner, smb2fh* handle) { _owner = owner; _handle = handle; }
        public int Read(byte[] buffer, ulong offset = 0) => Transfer(buffer, offset, false);
        public int Write(byte[] buffer, ulong offset = 0) => Transfer(buffer, offset, true);
        private int Transfer(byte[] buffer, ulong offset, bool writing)
        {
            ArgumentNullException.ThrowIfNull(buffer);
            lock (_owner._gate)
            {
                _owner.RequireContext();
                ObjectDisposedException.ThrowIf(_handle == null, this);
                // Upstream credit calculation subtracts one from count. Avoid
                // underflow and passing a null fixed pointer for an empty array.
                if (buffer.Length == 0) return 0;
                fixed (byte* data = buffer)
                {
                    CallbackState state = default;
                    using var pending = new OperationScope(_owner, &state);
                    int started = writing ? smb2_pwrite_async(_owner._context, _handle, data, (uint)buffer.Length, offset, &Complete, &state)
                        : smb2_pread_async(_owner._context, _handle, data, (uint)buffer.Length, offset, &Complete, &state);
                    return _owner.Wait(started, &state, writing ? "write" : "read");
                }
            }
        }
        public void Flush()
        {
            lock (_owner._gate)
            {
                _owner.RequireContext();
                ObjectDisposedException.ThrowIf(_handle == null, this);
                CallbackState state = default;
                using var pending = new OperationScope(_owner, &state);
                _owner.Wait(smb2_fsync_async(_owner._context, _handle, &Complete, &state), &state, "flush");
            }
        }
        public Metadata Stat()
        {
            lock (_owner._gate)
            {
                _owner.RequireContext();
                ObjectDisposedException.ThrowIf(_handle == null, this);
                smb2_stat_64 value = default;
                CallbackState state = default;
                using var pending = new OperationScope(_owner, &state);
                _owner.Wait(smb2_fstat_async(_owner._context, _handle, &value, &Complete, &state), &state, "fstat");
                return Metadata.From(value);
            }
        }
        public void Truncate(ulong length)
        {
            lock (_owner._gate)
            {
                _owner.RequireContext();
                ObjectDisposedException.ThrowIf(_handle == null, this);
                CallbackState state = default;
                using var pending = new OperationScope(_owner, &state);
                _owner.Wait(smb2_ftruncate_async(_owner._context, _handle, length, &Complete, &state), &state, "truncate");
            }
        }
        public Task<int> ReadAsync(byte[] buffer, ulong offset = 0, CancellationToken cancellationToken = default)
            => _owner.RunAsync(() => Read(buffer, offset), cancellationToken);
        public Task<int> WriteAsync(byte[] buffer, ulong offset = 0, CancellationToken cancellationToken = default)
            => _owner.RunAsync(() => Write(buffer, offset), cancellationToken);
        internal void ReleaseAfterDestroy()
        {
            if (_handle != null) FreeHandle(_handle);
            _handle = null;
        }
        public void Dispose()
        {
            lock (_owner._gate)
            {
                if (_handle == null) return;
                if (_owner._context == null) { ReleaseAfterDestroy(); return; }
                var handle = _handle;
                _handle = null;
                _owner._files.Remove(this);
                CallbackState state = default;
                try
                {
                    // Remove the handle from idle ownership before submission.
                    // A close callback owns/frees it, including during shutdown.
                    using var pending = new OperationScope(_owner, &state);
                    _owner.Wait(smb2_close_async(_owner._context, handle, &Complete, &state), &state, "close");
                }
                finally
                {
                    // Scope disposal has drained all pending callbacks. If none
                    // ran, submission never transferred ownership upstream.
                    if (!state.Finished) FreeHandle(handle);
                }
            }
        }
    }
}
