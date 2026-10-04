using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Edg.SQLite;
using static Edg.SQLite.SqliteCore;

// This consumer intentionally uses source-named exports and types, including
// managed callbacks with the generated instance-owner ABI. No reflection or
// generated implementation names are required; the same tests run under AOT.
internal static unsafe class Program
{
    private const int Workers = 4, Iterations = 24;
    private const int ReadWriteCreate = 6, FullMutex = 0x10000, NoMutex = 0x8000;
    private static int callbackCalls, callbackErrors;

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static byte[] Utf8(string text) => Encoding.UTF8.GetBytes(text + "\0");
    private static string Error(SqliteCore core, sqlite3* db) =>
        db == null ? "no handle" : Marshal.PtrToStringUTF8((nint)core.sqlite3_errmsg(db)) ?? "no message";

    private static sqlite3* Open(SqliteCore core, string path, int flags = ReadWriteCreate | FullMutex)
    {
#if EDG_SQLITE_UNIX
        if (OperatingSystem.IsWindows() && Path.IsPathFullyQualified(path))
        {
            path = path.Replace('\\', '/');
            path = path.StartsWith("//", StringComparison.Ordinal) ? "/unc/" + path[2..] : "/" + path;
        }
#endif
        sqlite3* db = null;
        fixed (byte* name = Utf8(path))
        {
            int rc = core.sqlite3_open_v2((sbyte*)name, &db, flags, null);
            if (rc != 0)
            {
                string error = Error(core, db);
                if (db != null) core.sqlite3_close(db);
                throw new InvalidOperationException($"open {path}: {rc}: {error}");
            }
        }
        return db;
    }

    private static int Execute(SqliteCore core, sqlite3* db, string sql)
    {
        fixed (byte* text = Utf8(sql)) return core.sqlite3_exec(db, (sbyte*)text, null, null, null);
    }

    private static void ExecuteOk(SqliteCore core, sqlite3* db, string sql)
    {
        int rc = Execute(core, db, sql);
        Require(rc == 0, $"exec {rc}: {Error(core, db)}: {sql}");
    }

    private static string Query(SqliteCore core, sqlite3* db, string sql)
    {
        sqlite3_stmt* statement = null;
        fixed (byte* text = Utf8(sql))
        {
            int rc = core.sqlite3_prepare_v2(db, (sbyte*)text, -1, &statement, null);
            Require(rc == 0, $"prepare {rc}: {Error(core, db)}: {sql}");
        }
        try
        {
            Require(core.sqlite3_step(statement) == 100, $"row: {Error(core, db)}: {sql}");
            string result = Marshal.PtrToStringUTF8((nint)core.sqlite3_column_text(statement, 0)) ?? "NULL";
            Require(core.sqlite3_step(statement) == 101, "done: " + sql);
            return result;
        }
        finally { Require(core.sqlite3_finalize(statement) == 0, "finalize"); }
    }

    private static void ParallelWorkers(Action<int> action)
    {
        using var start = new Barrier(Workers);
        var errors = new ConcurrentQueue<Exception>();
        Thread[] threads = Enumerable.Range(0, Workers).Select(index => new Thread(() =>
        {
            try
            {
                Require(start.SignalAndWait(TimeSpan.FromSeconds(30)), "worker barrier");
                action(index);
            }
            catch (Exception exception) { errors.Enqueue(exception); }
        }) { IsBackground = true }).ToArray();
        foreach (Thread thread in threads) thread.Start();
        foreach (Thread thread in threads)
            Require(thread.Join(TimeSpan.FromSeconds(60)), "worker timeout (possible mutex deadlock)");
        if (!errors.IsEmpty) throw new AggregateException(errors);
    }

    private static void Initialization(SqliteCore core)
    {
        Require(core.sqlite3_threadsafe() == 1, "SQLITE_THREADSAFE=1 required");
        nint[]? staticIdentities = null;
        for (int wave = 0; wave < 3; wave++)
        {
            ParallelWorkers(_ => Require(core.sqlite3_initialize() == 0, "concurrent cold initialize"));
            // Static SQLite mutexes belong to the owner, not an initialization
            // wave. Retain integer identities only while SQLite is shut down;
            // acquire fresh valid mutex pointers after initialization resumes.
            nint[] identities = Enumerable.Range(2, 12)
                .Select(id => (nint)core.sqlite3_mutex_alloc(id)).ToArray();
            Require(identities.All(identity => identity != 0), "initialized static mutex identities");
            if (staticIdentities != null)
                Require(identities.SequenceEqual(staticIdentities), "static mutex identities survive shutdown/reinitialize");
            staticIdentities = identities;
            Require(core.sqlite3_config(3) == 21, "initialized threading configuration rejected");
            Require(core.sqlite3_shutdown() == 0, "quiescent shutdown");
            Require(core.Mutexes.DynamicMutexCount == 0, "initialization mutexes released");
        }
        foreach (int mode in new[] { 1, 2, 3 })
        {
            Require(core.sqlite3_config(mode) == 0, "configure threading mode");
            sqlite3* db = Open(core, ":memory:", ReadWriteCreate);
            try
            {
                Require((core.sqlite3_db_mutex(db) != null) == (mode == 3), "configured default connection mutex");
                Require(Query(core, db, "SELECT 42") == "42", "configured connection SQL");
            }
            finally { Require(core.sqlite3_close(db) == 0, "configured close"); }
            Require(core.sqlite3_shutdown() == 0, "configured shutdown");
        }
        Require(core.sqlite3_config(3) == 0, "restore serialized mode");
        Require(core.sqlite3_initialize() == 0, "initialize serialized mode");
        Console.WriteLine("PASS concurrent cold initialization, stable static mutexes across shutdown/restart and threading configuration");
    }

    private static void Mutexes(SqliteCore core)
    {
        for (int id = 2; id <= 13; id++)
        {
            int mutexId = id;
            nint identity = (nint)core.sqlite3_mutex_alloc(id);
            Require(identity != 0, "static mutex");
            ParallelWorkers(_ => Require((nint)core.sqlite3_mutex_alloc(mutexId) == identity, "stable static mutex"));
        }
        sqlite3_mutex* recursive = core.sqlite3_mutex_alloc(1);
        sqlite3_mutex* fast = core.sqlite3_mutex_alloc(0);
        Require(recursive != null && fast != null && recursive != fast, "dynamic mutex allocation");
        try
        {
            core.sqlite3_mutex_enter(recursive);
            core.sqlite3_mutex_enter(recursive);
            Require(core.sqlite3_mutex_try(recursive) == 0, "recursive try by owner");
            core.sqlite3_mutex_leave(recursive);
            core.sqlite3_mutex_leave(recursive);
            nint address = (nint)recursive;
            ParallelWorkers(_ => Require(core.sqlite3_mutex_try((sqlite3_mutex*)address) == 5, "recursive mutex contention"));
            core.sqlite3_mutex_leave(recursive);
            Require(core.sqlite3_mutex_try(recursive) == 0, "recursive mutex released");
            core.sqlite3_mutex_leave(recursive);
            core.sqlite3_mutex_enter(fast);
            address = (nint)fast;
            ParallelWorkers(_ => Require(core.sqlite3_mutex_try((sqlite3_mutex*)address) == 5, "fast mutex contention"));
            core.sqlite3_mutex_leave(fast);
            core.sqlite3_mutex_enter(null);
            Require(core.sqlite3_mutex_try(null) == 0, "null mutex API");
            core.sqlite3_mutex_leave(null);
            core.sqlite3_mutex_free(null);
        }
        finally { core.sqlite3_mutex_free(recursive); core.sqlite3_mutex_free(fast); }
        Console.WriteLine("PASS static/dynamic mutex identity, recursion, contention and null API");
    }

    private static void Reenter(SqliteCore core, sqlite3_context* context, int count, sqlite3_value** values)
    {
        try
        {
            sqlite3* db = (sqlite3*)core.sqlite3_user_data(context);
            Require(Query(core, db, "SELECT 40+2") == "42", "callback nested query");
            Interlocked.Increment(ref callbackCalls);
            core.sqlite3_result_int(context, 42);
        }
        catch
        {
            Interlocked.Increment(ref callbackErrors);
            fixed (byte* error = "callback reentry failed\0"u8)
                core.sqlite3_result_error(context, (sbyte*)error, -1);
        }
    }

    private static void SharedConnection(SqliteCore core)
    {
        sqlite3* db = Open(core, ":memory:");
        try
        {
            Require(core.sqlite3_db_mutex(db) != null, "FULLMUTEX has real mutex");
            ExecuteOk(core, db, "CREATE TABLE totals(value INTEGER); INSERT INTO totals VALUES(0)");
            fixed (byte* name = "reenter\0"u8)
                Require(core.sqlite3_create_function_v2(db, (sbyte*)name, 0, 1, db, &Reenter, null, null, null) == 0,
                    "register managed recursive function");
            nint address = (nint)db;
            ParallelWorkers(_ =>
            {
                for (int iteration = 0; iteration < Iterations; iteration++)
                {
                    ExecuteOk(core, (sqlite3*)address, "UPDATE totals SET value=value+1");
                    Require(Query(core, (sqlite3*)address, "SELECT reenter()") == "42", "recursive callback result");
                }
            });
            Require(Query(core, db, "SELECT value FROM totals") == (Workers * Iterations).ToString(), "serialized updates");
            Require(callbackCalls == Workers * Iterations && callbackErrors == 0, "callback totals");
        }
        finally { Require(core.sqlite3_close(db) == 0, "shared connection close"); }
        Console.WriteLine("PASS shared FULLMUTEX connection, atomic updates and managed callback reentry");
    }

    private static void IndependentOwners(SqliteCore first)
    {
        using var second = new SqliteCore();
        sqlite3* a = Open(first, ":memory:");
        sqlite3* b = Open(second, ":memory:");
        try
        {
            Require(first.sqlite3_mutex_alloc(2) != second.sqlite3_mutex_alloc(2), "owner-local static mutexes");
            ExecuteOk(first, a, "CREATE TABLE values_table(v); INSERT INTO values_table VALUES(7)");
            ExecuteOk(second, b, "CREATE TABLE values_table(v); INSERT INTO values_table VALUES(100)");
            ExecuteOk(first, a, "BEGIN; INSERT INTO values_table VALUES(11)");
            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            Require(Query(first, a, "SELECT sum(v) FROM values_table") == "18", "first transaction");
            Require(Query(second, b, "SELECT sum(v) FROM values_table") == "100", "second owner isolated");
            ExecuteOk(first, a, "ROLLBACK");
            Require(Query(first, a, "SELECT sum(v) FROM values_table") == "7", "first rollback");
        }
        finally { Require(first.sqlite3_close(a) == 0 && second.sqlite3_close(b) == 0, "owner connection close"); }
        Require(second.sqlite3_shutdown() == 0, "second owner shutdown");
        sqlite3* alive = Open(first, ":memory:");
        try { Require(Query(first, alive, "SELECT 42") == "42", "first survives second shutdown"); }
        finally { Require(first.sqlite3_close(alive) == 0, "first survival close"); }
        Console.WriteLine("PASS independent owners, globals, mutexes, transactions and GC");
    }

    private static void Persistence(SqliteCore core, string directory)
    {
        string path = Path.Combine(directory, "persist-λ.db");
        sqlite3* db = Open(core, path);
        try
        {
            ExecuteOk(core, db, "PRAGMA synchronous=FULL; CREATE TABLE data(id INTEGER PRIMARY KEY,payload);" +
                "INSERT INTO data VALUES(1,jsonb('{\"name\":\"λ\",\"n\":42}'));" +
                "CREATE VIRTUAL TABLE docs USING fts5(body); INSERT INTO docs VALUES('café Ωμέγα database')");
            Require(Query(core, db, "SELECT round(log(100)+ln(exp(3))+sqrt(16))") == "9.0", "musl math functions");
            ExecuteOk(core, db, "PRAGMA temp_store=FILE; CREATE TEMP TABLE scratch AS SELECT value FROM json_each('[1,2,3]')");
            Require(Query(core, db, "SELECT sum(value) FROM scratch") == "6", "file-capable temp storage");
        }
        finally { Require(core.sqlite3_close(db) == 0, "persistent close"); }
        Require(File.ReadAllBytes(path).AsSpan(0, 16).SequenceEqual("SQLite format 3\0"u8), "on-disk SQLite header");
        db = Open(core, path, 1 | FullMutex);
        try
        {
            Require(Query(core, db, "SELECT json_extract(payload,'$.name') FROM data") == "λ", "durable JSONB");
            Require(Query(core, db, "SELECT count(*) FROM docs WHERE docs MATCH 'cafe'") == "1", "FTS5 diacritics");
            Require(Query(core, db, "SELECT count(*) FROM docs WHERE docs MATCH 'ωμέγα'") == "1", "FTS5 Unicode");
            Require(Execute(core, db, "DELETE FROM data") == 8, "read-only rejects writes");
            Require(Query(core, db, "PRAGMA integrity_check") == "ok", "persisted integrity");
        }
        finally { Require(core.sqlite3_close(db) == 0, "readonly close"); }
        Console.WriteLine("PASS real-file persistence, Unicode, JSONB, FTS5, musl math and readonly access");
    }

    private static void SeparateConnections(SqliteCore core, string path, bool wal)
    {
        sqlite3* setup = Open(core, path);
        try
        {
            Require(Query(core, setup, "PRAGMA journal_mode=" + (wal ? "WAL" : "DELETE")) == (wal ? "wal" : "delete"), "journal mode");
            ExecuteOk(core, setup, "CREATE TABLE totals(value INTEGER); INSERT INTO totals VALUES(0)");
            ParallelWorkers(_ =>
            {
                sqlite3* db = Open(core, path, ReadWriteCreate | NoMutex);
                try
                {
                    Require(core.sqlite3_db_mutex(db) == null, "separate NOMUTEX connection");
                    Require(core.sqlite3_busy_timeout(db, 2000) == 0, "busy timeout");
                    for (int iteration = 0; iteration < Iterations; iteration++)
                    {
                        int rc = 5;
                        long started = Stopwatch.GetTimestamp();
                        while ((rc & 255) == 5 && Stopwatch.GetElapsedTime(started) < TimeSpan.FromSeconds(15))
                        {
                            rc = Execute(core, db, "UPDATE totals SET value=value+1");
                            if ((rc & 255) == 5) Thread.Sleep(1);
                        }
                        Require(rc == 0, $"concurrent writer {rc}: {Error(core, db)}");
                        if (iteration % 8 == 7)
                        {
                            Require(core.sqlite3_close(db) == 0, "concurrent intermediate close");
                            db = null;
                            db = Open(core, path, ReadWriteCreate | NoMutex);
                            Require(core.sqlite3_busy_timeout(db, 2000) == 0, "reopened busy timeout");
                        }
                    }
                }
                finally { Require(core.sqlite3_close(db) == 0, "worker close"); }
            });
            Require(Query(core, setup, "SELECT value FROM totals") == (Workers * Iterations).ToString(), "concurrent total");
            if (wal) Require(core.sqlite3_wal_checkpoint_v2(setup, null, 3, null, null) == 0, "truncate checkpoint");
            Require(Query(core, setup, "PRAGMA integrity_check") == "ok", "concurrent integrity");
        }
        finally { Require(core.sqlite3_close(setup) == 0, "concurrent setup close"); }
        Console.WriteLine("PASS separate NOMUTEX connections with " + (wal ? "WAL" : "rollback journal"));
    }

    private static void BackupBlobAndSerialization(SqliteCore core, string directory)
    {
        sqlite3* source = Open(core, ":memory:");
        sqlite3* destination = Open(core, Path.Combine(directory, "backup.db"));
        sqlite3* restored = Open(core, ":memory:");
        try
        {
            ExecuteOk(core, source, "CREATE TABLE payloads(id INTEGER PRIMARY KEY,body BLOB);" +
                "INSERT INTO payloads VALUES(1,zeroblob(32)),(2,zeroblob(32))");
            sqlite3_blob* blob = null;
            fixed (byte* schema = "main\0"u8)
            fixed (byte* table = "payloads\0"u8)
            fixed (byte* column = "body\0"u8)
                Require(core.sqlite3_blob_open(source, (sbyte*)schema, (sbyte*)table, (sbyte*)column, 1, 1, &blob) == 0,
                    "incremental blob open");
            try
            {
                Require(core.sqlite3_blob_bytes(blob) == 32, "blob length");
                byte* bytes = stackalloc byte[8];
                for (int i = 0; i < 8; i++) bytes[i] = (byte)(i + 1);
                Require(core.sqlite3_blob_write(blob, bytes, 8, 4) == 0, "incremental blob write");
                new Span<byte>(bytes, 8).Clear();
                Require(core.sqlite3_blob_read(blob, bytes, 8, 4) == 0, "incremental blob read");
                for (int i = 0; i < 8; i++) Require(bytes[i] == i + 1, "blob bytes retained");
                Require(core.sqlite3_blob_write(blob, bytes, 8, 30) == 1, "blob bounds error");
                Require(core.sqlite3_blob_reopen(blob, 2) == 0, "blob reopen row");
                Require(core.sqlite3_blob_read(blob, bytes, 8, 4) == 0 && new ReadOnlySpan<byte>(bytes, 8).IndexOfAnyExcept((byte)0) == -1,
                    "reopened row independent");
            }
            finally { Require(core.sqlite3_blob_close(blob) == 0, "blob close"); }
            sqlite3_backup* backup;
            fixed (byte* schema = "main\0"u8)
                backup = core.sqlite3_backup_init(destination, (sbyte*)schema, source, (sbyte*)schema);
            Require(backup != null, "backup init");
            try
            {
                int rc;
                do { rc = core.sqlite3_backup_step(backup, 1); } while (rc == 0);
                Require(rc == 101 && core.sqlite3_backup_remaining(backup) == 0 && core.sqlite3_backup_pagecount(backup) > 0,
                    "incremental backup complete");
            }
            finally { Require(core.sqlite3_backup_finish(backup) == 0, "backup finish"); }
            Require(Query(core, destination, "SELECT hex(substr(body,5,8)) FROM payloads WHERE id=1") == "0102030405060708",
                "backup contents");
            long size = 0;
            byte* serialized;
            fixed (byte* schema = "main\0"u8)
                serialized = core.sqlite3_serialize(destination, (sbyte*)schema, &size, 0);
            Require(serialized != null && size > 0, "serialize allocation");
            try
            {
                fixed (byte* schema = "main\0"u8)
                    Require(core.sqlite3_deserialize(restored, (sbyte*)schema, serialized, size, size, 3) == 0,
                        "deserialize owned resizable buffer");
                serialized = null; // FREEONCLOSE transferred allocator ownership.
                Require(Query(core, restored, "SELECT hex(substr(body,5,8)) FROM payloads WHERE id=1") == "0102030405060708",
                    "deserialized contents");
                ExecuteOk(core, restored, "INSERT INTO payloads VALUES(3,randomblob(65536))");
                Require(Query(core, restored, "PRAGMA integrity_check") == "ok", "resized deserialized integrity");
                Require(Query(core, destination, "SELECT count(*) FROM payloads") == "2", "deserialization isolation");
            }
            finally { if (serialized != null) core.sqlite3_free(serialized); }
        }
        finally
        {
            Require(core.sqlite3_close(restored) == 0, "restored close");
            Require(core.sqlite3_close(destination) == 0, "backup destination close");
            Require(core.sqlite3_close(source) == 0, "backup source close");
        }
        Console.WriteLine("PASS incremental blobs, bounds errors, backup, serialization and resizable deserialization");
    }

    private static void FileTemporaryStorage(SqliteCore core)
    {
        int handlesBefore = core.Host.OpenHandleCount;
        sqlite3* db = Open(core, ":memory:");
        try
        {
            ExecuteOk(core, db, "PRAGMA temp_store=FILE; PRAGMA temp.cache_size=4; PRAGMA cache_spill=ON;" +
                "CREATE TEMP TABLE scratch(id INTEGER,body TEXT);" +
                "WITH RECURSIVE t(x) AS (VALUES(1) UNION ALL SELECT x+1 FROM t WHERE x<4096)" +
                " INSERT INTO scratch SELECT x,printf('%01000d',4097-x) FROM t");
            Require(Query(core, db, "PRAGMA temp_store") == "1", "file temporary store selected");
            Require(core.Host.OpenHandleCount > handlesBefore, "temp pages spilled to an actual VFS file");
            Require(Query(core, db, "SELECT count(*) FROM scratch") == "4096", "temp spilled row count");
            Require(Query(core, db, "SELECT id FROM scratch ORDER BY body LIMIT 1") == "4096", "external temp sort");
            Require(Query(core, db, "SELECT sum(length(body)) FROM scratch") == "4096000", "temp spilled contents");
            Require(Query(core, db, "PRAGMA temp.integrity_check") == "ok", "temp integrity");
        }
        finally { Require(core.sqlite3_close(db) == 0, "temp close"); }
        Console.WriteLine("PASS file temporary storage, cache spill, sort and integrity");
    }

    private static void Mapping(SqliteCore core, string directory)
    {
#if EDG_SQLITE_UNIX
        bool mappingSupported = !OperatingSystem.IsWindows();
#else
        bool mappingSupported = true;
#endif
        string requestedSize = mappingSupported ? "1048576" : "0";
        foreach (string mode in new[] { "DELETE", "WAL" })
        {
            sqlite3* writer = Open(core, Path.Combine(directory, "mapped-" + mode + ".db"));
            sqlite3* reader = null;
            try
            {
                Require(Query(core, writer, "PRAGMA journal_mode=" + mode) == mode.ToLowerInvariant(), "mapped journal mode");
                ExecuteOk(core, writer, "CREATE TABLE mapped(id INTEGER PRIMARY KEY,body TEXT);" +
                    "WITH RECURSIVE t(x) AS (VALUES(1) UNION ALL SELECT x+1 FROM t WHERE x<512)" +
                    " INSERT INTO mapped SELECT x,printf('%01000d',x) FROM t");
                if (mode == "WAL") Require(core.sqlite3_wal_checkpoint_v2(writer, null, 3, null, null) == 0, "materialize WAL pages");
                reader = Open(core, Path.Combine(directory, "mapped-" + mode + ".db"));
                Require(Query(core, reader, "PRAGMA mmap_size=1048576") == requestedSize, "platform mmap limit");
                ExecuteOk(core, reader, "PRAGMA cache_size=8; BEGIN");
                long before = core.Host.MappedReadCount;
                Require(Query(core, reader, "SELECT sum(length(body)) FROM mapped") == "512000", "mapped SQL bytes");
                Require(mappingSupported ? core.Host.MappedReadCount > before : core.Host.MappedReadCount == before,
                    "SQL uses mappings when supported, ordinary reads otherwise");
                if (mode == "WAL")
                {
                    ExecuteOk(core, writer, "INSERT INTO mapped VALUES(513,printf('%01000d',513))");
                    Require(Query(core, reader, "SELECT count(*) FROM mapped") == "512", "WAL reader preserves snapshot");
                    int frames = -1, checkpointed = -1;
                    Require(core.sqlite3_wal_checkpoint_v2(writer, null, 3, &frames, &checkpointed) == 5,
                        "truncate checkpoint blocked by active snapshot");
                }
                else Require(Execute(core, writer, "INSERT INTO mapped VALUES(513,printf('%01000d',513))") == 5,
                    "rollback reader blocks writer");
                ExecuteOk(core, reader, "COMMIT");
                if (mode == "DELETE") ExecuteOk(core, writer, "INSERT INTO mapped VALUES(513,printf('%01000d',513))");
                else Require(core.sqlite3_wal_checkpoint_v2(writer, null, 3, null, null) == 0, "checkpoint after reader leaves");
                Require(Query(core, reader, "SELECT sum(length(body)) FROM mapped") == "513000", "mapping observes committed growth");
                Require(Query(core, reader, "PRAGMA mmap_size=0") == "0", "disable mmap");
                before = core.Host.MappedReadCount;
                Require(Query(core, reader, "SELECT sum(length(body)) FROM mapped") == "513000", "unmapped fallback bytes");
                Require(core.Host.MappedReadCount == before, "disabled mmap does not fetch");
                ExecuteOk(core, writer, "DELETE FROM mapped WHERE id>4; VACUUM");
                Require(Query(core, reader, "PRAGMA mmap_size=1048576") == requestedSize, "platform limit retained after VACUUM");
                Require(Query(core, reader, "SELECT sum(length(body)) FROM mapped") == "4000", "mapping after file shrink");
                Require(Query(core, reader, "PRAGMA integrity_check") == "ok", "mapped integrity");
            }
            finally
            {
                if (reader != null) Require(core.sqlite3_close(reader) == 0, "mapped reader close");
                Require(core.sqlite3_close(writer) == 0, "mapped writer close");
            }
            Require(core.Host.MappedViewCount == 0 && core.Host.MappedReferenceCount == 0, "mappings reclaimed");
        }
        Console.WriteLine(mappingSupported
            ? "PASS actual mmap reads, rollback/WAL snapshots, checkpoint contention, growth, fallback and VACUUM"
            : "PASS Windows Unix-VFS read fallback, rollback/WAL snapshots, checkpoint contention, growth and VACUUM");
    }

    private static void Serve(string path, bool readOnly)
    {
        using var core = new SqliteCore();
        sqlite3* db = Open(core, path, readOnly ? 1 : ReadWriteCreate);
        try
        {
            Console.WriteLine("READY"); Console.Out.Flush();
            string? line;
            while ((line = Console.ReadLine()) != null)
            {
                if (line == "X") break;
                if (line.StartsWith("E ", StringComparison.Ordinal)) Console.WriteLine("RC " + Execute(core, db, line[2..]));
                else if (line.StartsWith("Q ", StringComparison.Ordinal)) Console.WriteLine("ROW " + Query(core, db, line[2..]));
                else if (line.StartsWith("C ", StringComparison.Ordinal))
                {
                    int frames = -1, checkpointed = -1;
                    int rc = core.sqlite3_wal_checkpoint_v2(db, null, int.Parse(line[2..]), &frames, &checkpointed);
                    Console.WriteLine($"CK {rc} {frames} {checkpointed}");
                }
                else throw new InvalidOperationException("Unknown protocol command");
                Console.Out.Flush();
            }
        }
        finally { Require(core.sqlite3_close(db) == 0, "server close"); }
        Require(core.sqlite3_shutdown() == 0, "server shutdown");
    }

    public static int Main(string[] args)
    {
        string directory = Path.Combine(Path.GetTempPath(), "edg-sqlite-host-" + Guid.NewGuid().ToString("N"));
        try
        {
            if (args.Length >= 2 && args[0] == "serve") { Serve(args[1], args.Length > 2 && args[2] == "readonly"); return 0; }
            Directory.CreateDirectory(directory);
            using var core = new SqliteCore();
            Initialization(core);
            Mutexes(core);
            SharedConnection(core);
            IndependentOwners(core);
            Persistence(core, directory);
            SeparateConnections(core, Path.Combine(directory, "rollback.db"), false);
            SeparateConnections(core, Path.Combine(directory, "wal.db"), true);
            BackupBlobAndSerialization(core, directory);
            FileTemporaryStorage(core);
            Mapping(core, directory);
            Require(core.Host.OpenHandleCount == 0, "all VFS handles closed");
            Require(core.sqlite3_shutdown() == 0, "final shutdown");
            Require(core.Mutexes.DynamicMutexCount == 0, "all dynamic mutexes released");
            ParallelWorkers(_ => Require(core.sqlite3_initialize() == 0, "final concurrent restart"));
            Require(core.sqlite3_shutdown() == 0, "final restarted shutdown");
            Console.WriteLine("PASS joined workers and final shutdown/restart");
            return 0;
        }
        catch (Exception exception) { Console.Error.WriteLine(exception); return 1; }
        finally { if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true); }
    }
}
