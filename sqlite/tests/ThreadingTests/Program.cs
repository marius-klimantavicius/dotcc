using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using DotCC.Sqlite;
using Managed.Database;
using static global::Managed.Database.Sqlite;

internal static unsafe class Program
{
    private const int Workers = 4, Iterations = 40;
    private const int ReadWriteCreate = 6, NoMutex = 0x8000, FullMutex = 0x10000;
    private static readonly delegate*<sqlite3_context*, int, sqlite3_value**, void> ReenterPointer = &Reenter;
    private static int callbackCalls, callbackErrors;

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static byte[] Utf8(string text) => Encoding.UTF8.GetBytes(text + "\0");

    private static void ParallelWorkers(Action<int> action)
    {
        using var start = new Barrier(Workers);
        var errors = new ConcurrentQueue<Exception>();
        var threads = Enumerable.Range(0, Workers).Select(index => new Thread(() =>
        {
            try
            {
                Require(start.SignalAndWait(TimeSpan.FromSeconds(30)), "worker start barrier");
                action(index);
            }
            catch (Exception exception) { errors.Enqueue(exception); }
        }) { IsBackground = true }).ToArray();
        foreach (var thread in threads) thread.Start();
        foreach (var thread in threads) Require(thread.Join(TimeSpan.FromSeconds(45)), "worker did not finish");
        if (!errors.IsEmpty) throw new AggregateException(errors);
    }

    private static sqlite3* Open(string path, int mutex = FullMutex, string? vfs = null)
    {
        sqlite3* db = null;
        fixed (byte* name = Utf8(path))
        fixed (byte* vfsName = vfs is null ? null : Utf8(vfs))
        {
            var rc = sqlite3_open_v2(name, &db, ReadWriteCreate | mutex, vfsName);
            if (rc != 0)
            {
                var error = db == null ? "no handle" : Marshal.PtrToStringUTF8((nint)sqlite3_errmsg(db));
                if (db != null) sqlite3_close(db);
                throw new InvalidOperationException($"open {path}: {rc}: {error}");
            }
        }
        Require(sqlite3_busy_timeout(db, 2000) == 0, "busy timeout");
        return db;
    }

    private static int Execute(sqlite3* db, string sql)
    {
        fixed (byte* text = Utf8(sql)) return sqlite3_exec(db, text, null, null, null);
    }

    private static void ExecuteOk(sqlite3* db, string sql)
    {
        var rc = Execute(db, sql);
        Require(rc == 0, $"exec {rc}: {Marshal.PtrToStringUTF8((nint)sqlite3_errmsg(db))}: {sql}");
    }

    private static long Scalar(sqlite3* db, string sql)
    {
        sqlite3_stmt* statement = null;
        fixed (byte* text = Utf8(sql)) Require(sqlite3_prepare_v2(db, text, -1, &statement, null) == 0, "prepare " + sql);
        try
        {
            Require(sqlite3_step(statement) == 100, "scalar row " + sql);
            var value = sqlite3_column_int64(statement, 0);
            Require(sqlite3_step(statement) == 101, "scalar done " + sql);
            return value;
        }
        finally { Require(sqlite3_finalize(statement) == 0, "finalize"); }
    }

    private static void Initialization()
    {
        Require(sqlite3_threadsafe() == 1, "product must compile SQLITE_THREADSAFE=1");
        // The very first API requiring initialization is invoked concurrently.
        nint[]? staticIdentities = null;
        for (var wave = 0; wave < 3; wave++)
        {
            ParallelWorkers(_ => Require(sqlite3_initialize() == 0, "concurrent initialize"));
            var identities = Enumerable.Range(2, 12).Select(id => (nint)sqlite3_mutex_alloc(id)).ToArray();
            if (staticIdentities != null) Require(identities.SequenceEqual(staticIdentities), "static identities survive shutdown/restart");
            staticIdentities = identities;
            Require(sqlite3_config(1) == 21 && sqlite3_config(2) == 21 && sqlite3_config(3) == 21,
                "threading config must reject changes while initialized");
            Require(sqlite3_shutdown() == 0, "quiescent shutdown");
            Require(HostMutex.DynamicMutexCount == 0, "initialization mutexes released at shutdown");
        }
        foreach (var mode in new[] { 1, 2, 3 })
        {
            Require(sqlite3_config(mode) == 0 && sqlite3_initialize() == 0, "quiescent threading configuration");
            var db = Open(":memory:", mutex: 0);
            try
            {
                Require((sqlite3_db_mutex(db) != null) == (mode == 3), "default connection mutex follows configuration");
                Require(Scalar(db, "SELECT 42") == 42, "configured connection works");
            }
            finally { Require(sqlite3_close(db) == 0, "configured connection close"); }
            Require(sqlite3_shutdown() == 0, "configuration lifecycle shutdown");
        }
        Require(sqlite3_config(3) == 0, "restore serialized configuration");
        Console.WriteLine("PASS concurrent cold initialization, quiescent restart and threading configuration");
    }

    private static void Mutexes()
    {
        // GETMUTEX is legal while shut down; earlier initialization waves
        // installed SQLite's APPDEF method table.
        sqlite3_mutex_methods methods = default;
        Require(sqlite3_config(11, (void*)&methods) == 0, "get mutex methods");
        Require(methods.xMutexHeld != null && methods.xMutexNotheld != null, "mutex ownership probes supplied");
        Require(sqlite3_initialize() == 0, "mutex initialization");
        for (var id = 2; id <= 13; id++)
        {
            var address = (nint)sqlite3_mutex_alloc(id);
            Require(address != 0 && address == (nint)sqlite3_mutex_alloc(id), "stable static mutex " + id);
            ParallelWorkers(_ => Require((nint)sqlite3_mutex_alloc(id) == address, "concurrent static identity"));
        }
        var fast = sqlite3_mutex_alloc(0);
        var recursive = sqlite3_mutex_alloc(1);
        var other = sqlite3_mutex_alloc(1);
        Require(fast != null && recursive != null && other != null && fast != recursive && recursive != other, "distinct dynamic mutexes");
        try
        {
            Require(methods.xMutexNotheld(recursive) != 0 && methods.xMutexHeld(recursive) == 0, "initial ownership");
            sqlite3_mutex_enter(recursive);
            sqlite3_mutex_enter(recursive);
            Require(sqlite3_mutex_try(recursive) == 0, "recursive Try succeeds for owner");
            Require(methods.xMutexHeld(recursive) != 0 && methods.xMutexNotheld(recursive) == 0, "recursive ownership");
            sqlite3_mutex_leave(recursive);
            sqlite3_mutex_leave(recursive);
            var address = (nint)recursive;
            var held = methods.xMutexHeld;
            var notHeld = methods.xMutexNotheld;
            ParallelWorkers(_ =>
            {
                var mutex = (sqlite3_mutex*)address;
                Require(held(mutex) == 0 && notHeld(mutex) != 0, "ownership is thread-specific");
                Require(sqlite3_mutex_try(mutex) == 5, "Try reports contention until final recursive leave");
            });
            sqlite3_mutex_leave(recursive);
            Require(sqlite3_mutex_try(recursive) == 0, "Try succeeds after release");
            sqlite3_mutex_leave(recursive);
            sqlite3_mutex_enter(fast);
            var fastAddress = (nint)fast;
            ParallelWorkers(_ => Require(sqlite3_mutex_try((sqlite3_mutex*)fastAddress) == 5, "fast Try contention"));
            sqlite3_mutex_leave(fast);
            sqlite3_mutex_enter(null);
            Require(sqlite3_mutex_try(null) == 0, "null Try");
            sqlite3_mutex_leave(null);
            sqlite3_mutex_free(null);
        }
        finally { sqlite3_mutex_free(fast); sqlite3_mutex_free(recursive); sqlite3_mutex_free(other); }
        Console.WriteLine("PASS static/dynamic mutex identity, recursion, ownership, contention and null API");
    }

    private static void Reenter(sqlite3_context* context, int count, sqlite3_value** values)
    {
        try
        {
            var db = (sqlite3*)sqlite3_user_data(context);
            var result = Scalar(db, "SELECT 40+2");
            Interlocked.Increment(ref callbackCalls);
            sqlite3_result_int(context, (int)result);
        }
        catch
        {
            Interlocked.Increment(ref callbackErrors);
            fixed (byte* error = "callback reentry failed\0"u8) sqlite3_result_error(context, error, -1);
        }
    }

    private static void SharedConnection()
    {
        var db = Open(":memory:");
        try
        {
            Require(sqlite3_db_mutex(db) != null, "FULLMUTEX connection owns a mutex");
            ExecuteOk(db, "CREATE TABLE totals(value INTEGER); INSERT INTO totals VALUES(0)");
            fixed (byte* name = "reenter\0"u8)
                Require(sqlite3_create_function_v2(db, name, 0, 1, db, ReenterPointer, null, null, null) == 0, "register reentry callback");
            var address = (nint)db;
            ParallelWorkers(_ =>
            {
                for (var iteration = 0; iteration < Iterations; iteration++)
                {
                    ExecuteOk((sqlite3*)address, "UPDATE totals SET value=value+1");
                    Require(Scalar((sqlite3*)address, "SELECT reenter()") == 42, "concurrent recursive callback");
                }
            });
            Require(Scalar(db, "SELECT value FROM totals") == Workers * Iterations, "serialized atomic updates");
            Require(callbackCalls == Workers * Iterations && callbackErrors == 0, "all callbacks completed");
        }
        finally { Require(sqlite3_close(db) == 0, "shared close"); }
        Console.WriteLine("PASS shared FULLMUTEX connection atomic updates and managed callback reentry");
    }

    private static void SeparateConnections(string path, string? vfs, bool wal)
    {
        var setup = Open(path, vfs: vfs);
        try
        {
            ExecuteOk(setup, wal ? "PRAGMA journal_mode=WAL" : "PRAGMA journal_mode=DELETE");
            ExecuteOk(setup, "CREATE TABLE totals(value INTEGER); INSERT INTO totals VALUES(0)");
            ParallelWorkers(_ =>
            {
                var db = Open(path, NoMutex, vfs);
                try
                {
                    Require(sqlite3_db_mutex(db) == null, "separate NOMUTEX connection");
                    for (var iteration = 0; iteration < Iterations; iteration++)
                    {
                        int rc = 5;
                        // Memory VFS xSleep intentionally does not wait. Give
                        // actual owner threads a scheduling opportunity instead
                        // of counting simulated busy-handler sleep as wall time.
                        var started = Stopwatch.GetTimestamp();
                        var retry = new SpinWait();
                        while ((rc & 255) == 5 && Stopwatch.GetElapsedTime(started) < TimeSpan.FromSeconds(10))
                        {
                            rc = Execute(db, "UPDATE totals SET value=value+1");
                            if ((rc & 255) == 5)
                            {
                                Require(sqlite3_get_autocommit(db) != 0, "failed statement leaves no explicit transaction");
                                retry.SpinOnce();
                            }
                        }
                        Require(rc == 0, "separate writer rc " + rc);
                    }
                }
                finally { Require(sqlite3_close(db) == 0, "worker close"); }
            });
            Require(Scalar(setup, "SELECT value FROM totals") == Workers * Iterations, "separate updates retained");
            if (wal) Require(sqlite3_wal_checkpoint_v2(setup, null, 3, null, null) == 0, "quiescent truncate checkpoint");
        }
        finally { Require(sqlite3_close(setup) == 0, "setup close"); }
        Console.WriteLine("PASS separate NOMUTEX connections " + (vfs ?? "host") + (wal ? " WAL" : " rollback"));
    }

    private static void MemoryFaultRecovery()
    {
        foreach (var injected in new[] { 7, 10 })
        {
            using var failedOpenFinished = new ManualResetEventSlim();
            ParallelWorkers(worker =>
            {
                var vfs = dotcc_memory_vfs();
                var file = (sqlite3_file*)NativeMemory.AllocZeroed((nuint)vfs->szOsFile);
                try
                {
                    if (worker == 0)
                    {
                        try
                        {
                            dotcc_memory_vfs_fail_after(1, 0, injected);
                            fixed (byte* name = Utf8("fault-memory"))
                                Require(vfs->xOpen(vfs, name, file, 0x106, null) == injected, "injected OPEN result");
                        }
                        finally { failedOpenFinished.Set(); }
                    }
                    else
                    {
                        Require(failedOpenFinished.Wait(TimeSpan.FromSeconds(30)), "failed OPEN completion");
                        // A leaked recursive gate would pass on the failing thread;
                        // these other threads must independently acquire it.
                        fixed (byte* name = Utf8("recovery-" + worker))
                        {
                            Require(dotcc_memory_vfs_import(name, null, 0) == 0, "cross-thread import after failure");
                            Require(vfs->xOpen(vfs, name, file, 0x106, null) == 0, "cross-thread open after failure");
                        }
                        int level = -1;
                        Require(file->pMethods->xFileControl(file, 1, &level) == 0 && level == 0, "cross-thread file control");
                    }
                }
                finally
                {
                    if (file->pMethods != null) Require(file->pMethods->xClose(file) == 0, "raw recovery close");
                    NativeMemory.Free(file);
                }
            });
        }
        Console.WriteLine("PASS injected OPEN NOMEM/IOERR release the memory VFS gate for other threads");
    }

    private static void AllocationAndRandomness()
    {
        ParallelWorkers(worker =>
        {
            var random = stackalloc byte[32];
            for (var iteration = 0; iteration < 200; iteration++)
            {
                var memory = (byte*)sqlite3_malloc64(64);
                Require(memory != null, "parallel allocation");
                try
                {
                    new Span<byte>(memory, 64).Fill((byte)(worker + 1));
                    sqlite3_randomness(32, random);
                    long current = 0, high = 0;
                    Require(sqlite3_status64(0, &current, &high, 0) == 0 && current >= 64 && high >= current, "consistent allocation status");
                    for (var index = 0; index < 64; index++) Require(memory[index] == worker + 1, "allocation isolation");
                }
                finally { sqlite3_free(memory); }
            }
        });
        Console.WriteLine("PASS concurrent allocation/free, memory status and randomness");
    }

    private static int Main()
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-threading-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            Initialization();
            Mutexes();
            SharedConnection();
            SeparateConnections(Path.Combine(directory, "rollback.db"), null, wal: false);
            SeparateConnections(Path.Combine(directory, "wal.db"), null, wal: true);
            var memoryVfs = Marshal.PtrToStringUTF8((nint)dotcc_memory_vfs()->zName)!;
            SeparateConnections("threading-memory", memoryVfs, wal: false);
            MemoryFaultRecovery();
            AllocationAndRandomness();
            Require(HostVfs.OpenHandleCount == 0 && dotcc_memory_vfs_handle_count() == 0, "no leaked VFS handles");
            Require(dotcc_memory_vfs_reset() == 0, "quiescent memory reset");
            Require(sqlite3_shutdown() == 0, "final shutdown after joined workers");
            ParallelWorkers(_ => Require(sqlite3_initialize() == 0, "final concurrent restart"));
            Require(sqlite3_shutdown() == 0, "final restarted shutdown");
            Require(HostMutex.DynamicMutexCount == 0, "no dynamic mutex leaks");
            Console.WriteLine("PASS joined workers, clean handles, final shutdown and concurrent restart");
            return 0;
        }
        catch (Exception exception) { Console.Error.WriteLine(exception); return 1; }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
