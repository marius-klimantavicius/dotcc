using System.Text;
using Managed.PGlite.Hosting;
using Managed.PGlite.Initialization;
using Backend = Managed.PGlite.Translated.PGlite;
using Runtime = Managed.PGlite.Translated.PGlite.Libc;

namespace Managed.PGlite;

/// <summary>One serialized, thread-affine embedded PostgreSQL lifetime. The
/// original translated frontend initializes the cluster; the original backend
/// parses/executes SQL and owns transactions, storage, recovery and shutdown.</summary>
public sealed unsafe class PgliteDatabase : IDisposable
{
    [ThreadStatic] private static PgliteDatabase? callbackOwner;
    private readonly int thread = Environment.CurrentManagedThreadId;
    private readonly PgliteOptions options;
    private readonly DirectoryLease lease;
    private readonly string diagnostics;
    private readonly PgliteOwner owner;
    private readonly InitializationArguments arguments;
    private readonly InitializationEnvironment environment;
    private readonly PgliteTransport transport;
    private Exception? lifetimeError;
    private bool entering, disposed, faulted, ready;

    private PgliteDatabase(PgliteOptions options, DirectoryLease lease, string diagnostics)
    {
        this.options = options; this.lease = lease; this.diagnostics = diagnostics;
        transport = new(options.MaximumResponseBytes);
        arguments = new([options.BackendExecutable!, "--single", "-D", options.DataDirectory,
            "-c", "shared_buffers=16MB", "-c", "max_connections=10", "-c", "autovacuum=off",
            "-c", "client_encoding=UTF8",
            "-c", "log_min_messages=warning", "-c", "dynamic_library_path=" + Path.Combine(options.BootstrapPrefix, "lib/postgresql"),
            "template1"]);
        environment = new();
        try { owner = new(Backend.dpg_recover_query_error, Shutdown, Cleanup); }
        catch { environment.Dispose(); arguments.Dispose(); transport.Dispose(); throw; }
        // Callback targets, stable argv allocations and copied transport data
        // survive GC and remain owned until real core cleanup has completed.
        owner.Retain(this); owner.Retain(arguments); owner.Retain(transport);
    }

    public string DataDirectory => options.DataDirectory;
    public string DiagnosticsDirectory => diagnostics;
    public bool IsReady => ready && !faulted && !disposed;

    /// <summary>Initialize a new empty private cluster using original translated
    /// initdb, then enter the actual embedded backend and verify a SQL round trip.</summary>
    public static PgliteDatabase Create(PgliteOptions options) => Start(options, initialize: true);

    /// <summary>Open an existing cluster in a fresh generated backend context.
    /// The previous lifetime must have completed its real shutdown.</summary>
    public static PgliteDatabase Open(PgliteOptions options) => Start(options, initialize: false);

    private static PgliteDatabase Start(PgliteOptions supplied, bool initialize)
    {
        ArgumentNullException.ThrowIfNull(supplied);
        PgliteOptions options = supplied.Normalize();
        var lease = DirectoryLease.Acquire(options.DataDirectory);
        PgliteDatabase? database = null;
        try
        {
            string diagnostics = Path.Combine(options.WorkDirectory, "session-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(diagnostics);
            if (initialize)
            {
                if (Directory.Exists(options.DataDirectory) && Directory.EnumerateFileSystemEntries(options.DataDirectory).Any())
                    throw new IOException("Create requires a new or empty data directory.");
                Initialize(options, diagnostics);
            }
            else if (!File.Exists(Path.Combine(options.DataDirectory, "PG_VERSION")) ||
                !File.Exists(Path.Combine(options.DataDirectory, "global/pg_control")))
                throw new IOException("Open requires a cluster initialized by original PostgreSQL initdb.");
            database = new(options, lease, diagnostics);
            database.StartBackend();
            return database;
        }
        catch (Exception error)
        {
            // Once core startup has begun, a failed execution cannot be called
            // clean shutdown. Retain its owner, resources and exclusion lease.
            if (database == null) lease.Dispose();
            else { database.faulted = true; database.lifetimeError ??= error; }
            throw;
        }
    }

    private static void Initialize(PgliteOptions options, string diagnostics)
    {
        string input = Path.Combine(diagnostics, "initdb.stdin");
        string output = Path.Combine(diagnostics, "initdb.stdout");
        string error = Path.Combine(diagnostics, "initdb.stderr");
        File.WriteAllBytes(input, []);
        var backend = new InitializationBackend(options.BootstrapPrefix);
        var initialization = new PgliteInitializationTrace(diagnostics);
        int sequence = 0;
        using var frontend = new InitializationFrontend(options.BackendExecutable!, diagnostics, request =>
        {
            int index = ++sequence;
            int? invocationStatus = null;
            Exception? failure = null;
            try { return (invocationStatus = backend.Invoke(request)).Value; }
            catch (Exception caught) { failure = caught; throw; }
            finally
            {
                initialization.Complete(index, request, invocationStatus, failure);
            }
        });
        int status = frontend.Run([Path.Combine(options.BootstrapPrefix, "bin/initdb"), "-D", options.DataDirectory,
            "-L", Path.Combine(options.BootstrapPrefix, "share/postgresql"), "--locale=C", "--encoding=UTF8",
            "--username=postgres", "--auth=trust", "--no-instructions"], input, output, error);
        if (frontend.ProcessError != null)
            throw new IOException($"Translated initdb process transport failed; diagnostics: {diagnostics}", frontend.ProcessError);
        if (status != 0) throw new IOException($"Original translated initdb exited {status}; diagnostics: {diagnostics}");
        initialization.RequireCompletedOperations();
        if (!File.Exists(Path.Combine(options.DataDirectory, "PG_VERSION")) ||
            !File.Exists(Path.Combine(options.DataDirectory, "global/pg_control")))
            throw new IOException("Original initdb did not complete backend bootstrap and cluster creation.");
    }

    private static void Redirect(string path, ReadOnlySpan<byte> mode, int stream)
    {
        byte[] name = Encoding.UTF8.GetBytes(path + "\0");
        fixed (byte* p = name) fixed (byte* m = mode)
            if (Backend.pgl_freopen(p, m, stream) == null)
                throw new IOException($"Cannot bind backend stream {stream} to {path} (errno {Runtime.errno}).");
    }

    private void StartBackend()
    {
        string empty = Path.Combine(diagnostics, "backend.stdin");
        File.WriteAllBytes(empty, []);
        int status = Dispatch(() =>
        {
            Redirect(empty, "rb\0"u8, 0);
            // The managed poll service accepts these owned file descriptors;
            // native epoll's socketpair requirement belongs to the oracle.
            Redirect(Path.Combine(diagnostics, "backend.stdout"), "wb\0"u8, 1);
            Redirect(Path.Combine(diagnostics, "backend.stderr"), "wb\0"u8, 2);
            byte[] module = Encoding.UTF8.GetBytes(Path.Combine(options.BootstrapPrefix, "lib/postgresql/plpgsql") + "\0");
            fixed (byte* p = module)
                if (Backend.dpg_static_register(p, Backend.dpg_plpgsql_module()) != 0)
                    throw new IOException($"Cannot register translated PL/pgSQL (errno {Runtime.errno}).");
            Backend.dpg_set_io(&ReadCallback, &WriteCallback);
            Backend.dpg_start(&Backend.main, arguments.Count, arguments.Pointer);
        });
        if (status != 99) throw new IOException($"Original backend startup did not yield 99 (received {status}); diagnostics: {diagnostics}");
        status = Dispatch(() =>
        {
            if (Backend.dpg_open_liveness() != 0) throw new IOException($"Cannot open backend liveness pipe (errno {Runtime.errno}).");
            Backend.dpg_attach_protocol();
        });
        if (status != 0) throw new IOException($"Original protocol attachment yielded {status}.");
        // Protocol attachment alone is insufficient evidence of readiness.
        var probe = QueryCore("SELECT 1").EnsureSuccess();
        if (probe.TransactionStatus != PgliteTransactionStatus.Idle || probe.ResultSets.Count != 1 ||
            probe.ResultSets[0].Rows.Count != 1 || probe.ResultSets[0].Rows[0].Count != 1 || probe.ResultSets[0].Rows[0][0] != "1")
            throw new IOException("Original backend readiness query returned unexpected values.");
        ready = true;
    }

    private void RequireOwner()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (thread != Environment.CurrentManagedThreadId) throw new InvalidOperationException("PGlite must stay on its owning thread.");
        if (entering || callbackOwner != null) throw new InvalidOperationException("PGlite callback reentry is unsupported.");
        if (faulted) throw new InvalidOperationException("PGlite has a failed host/core lifetime and is quarantined.", lifetimeError);
    }

    private int Dispatch(Action entry)
    {
        RequireOwner(); entering = true; callbackOwner = this;
        try
        {
            int status = owner.Dispatch(entry);
            if (transport.Error != null) throw new IOException("A PostgreSQL I/O callback failed.", transport.Error);
            return status;
        }
        catch (Exception error) { lifetimeError ??= error; faulted = true; ready = false; throw; }
        finally { callbackOwner = null; entering = false; }
    }

    /// <summary>Execute a Simple Query. SQL errors are returned after real core
    /// recovery; use EnsureSuccess to throw while preserving the ready session.
    /// Response objects own copies and remain valid after subsequent calls/close.</summary>
    public PgliteQueryResult Query(string sql)
    {
        RequireOwner();
        if (!ready) throw new InvalidOperationException("PGlite is not ready.");
        return QueryCore(sql);
    }

    private PgliteQueryResult QueryCore(string sql)
    {
        // Validate before entry: invalid caller text never quarantines a session.
        byte[] frame = PgliteProtocol.EncodeQuery(sql);
        transport.Begin(frame);
        try
        {
            int status = Dispatch(Backend.dpg_step);
            if (status is not (0 or 100)) throw new IOException($"Unexpected query yield {status}.");
            int flush = Dispatch(() => { Backend.dpg_ready(); Backend.dpg_flush(); });
            if (flush != 0) throw new IOException($"Unexpected ready/flush yield {flush}.");
            long buffered = -1;
            if (Dispatch(() => buffered = Backend.dpg_buffered_input()) != 0 || !transport.InputConsumed || buffered != 0)
                throw new IOException("Original backend did not consume exactly one complete query frame.");
            return PgliteProtocol.DecodeResponse(transport.Response);
        }
        catch (Exception error) { lifetimeError ??= error; faulted = true; ready = false; throw; }
        finally { transport.ReleaseRequest(); }
    }

    private static long ReadCallback(void* buffer, ulong count)
    {
        var instance = callbackOwner;
        if (instance == null) { Runtime.errno = Runtime.EIO; return -1; }
        long countRead = instance.transport.Read(buffer, count);
        if (countRead < 0) Runtime.errno = Runtime.EIO;
        return countRead;
    }

    private static long WriteCallback(void* buffer, ulong count)
    {
        var instance = callbackOwner;
        if (instance == null) { Runtime.errno = Runtime.EIO; return -1; }
        long countWritten = instance.transport.Write(buffer, count);
        if (countWritten < 0) Runtime.errno = Runtime.EIO;
        return countWritten;
    }

    private void Shutdown()
    {
        transport.Begin(PgliteProtocol.EncodeTerminate());
        Backend.dpg_begin_shutdown();
        Backend.dpg_step(); // Requires the real Terminate -> proc_exit(0) path.
    }

    private void Cleanup()
    {
        Backend.dpg_finish_cleanup();
        if (transport.Error != null) throw new IOException("Shutdown callback failed.", transport.Error);
        if (!transport.InputConsumed) throw new IOException("Original backend did not consume Terminate.");
        if (Runtime.fflush(null) != 0) throw new IOException($"Cannot flush backend diagnostics (errno {Runtime.errno}).");
    }

    /// <summary>Disable embedding, feed Terminate, require real proc_exit(0),
    /// drain original atexit callbacks, then retire context/buffers and exclusion.
    /// A failed shutdown retains resources and prevents a misleading reopen.</summary>
    public void Dispose()
    {
        if (disposed) return;
        RequireOwner(); entering = true; callbackOwner = this;
        try
        {
            owner.Dispose();
            environment.Dispose(); arguments.Dispose(); transport.Dispose();
            ready = false; disposed = true; lease.Dispose();
        }
        catch (Exception error) { lifetimeError ??= error; faulted = true; ready = false; throw; }
        finally { entering = false; callbackOwner = null; }
    }

    private sealed class DirectoryLease : IDisposable
    {
        private static readonly object gate = new();
        private static DirectoryLease? active;
        private readonly FileStream file;
        private DirectoryLease(FileStream file) { this.file = file; }
        internal static DirectoryLease Acquire(string directory)
        {
            lock (gate)
            {
                if (active != null) throw new InvalidOperationException("One embedded PGlite owner is allowed in this process.");
                string path = directory + ".dotcc-owner";
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                var file = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                return active = new(file);
            }
        }
        public void Dispose()
        {
            lock (gate) { file.Dispose(); if (ReferenceEquals(active, this)) active = null; }
        }
    }
}
