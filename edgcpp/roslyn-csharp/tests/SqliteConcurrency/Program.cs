using System.Collections.Concurrent;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

internal static unsafe class ConcurrencyWorker
{
    private const string WorkloadSha = "132254de51d494552f2c1330a8bd8d2012e465fd72baf9f4ac72bdaa00323ee5";
    private struct Context { public int Magic, Id, Error, BusyCount, NotifyCount, GroupCount; public fixed int BusyArguments[16]; public fixed int NotifyIds[4]; }
    private static Context* busyContext;
    private static Context* notifyContext;
    private static readonly ConcurrentDictionary<string, Database> handles = new();
    private static readonly ConcurrentDictionary<nint, Statement> pending = new();
    private static JsonElement work;
    private static string databasePath = "", artifacts = "";
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private static string Text(sbyte* address) => address == null ? "<no message>" : Marshal.PtrToStringUTF8((nint)address)!;
    private static JsonObject Record(string path)
    {
        using var stream = File.OpenRead(path);
        return new() { ["path"] = Path.GetFullPath(path), ["sha256"] = Convert.ToHexStringLower(SHA256.HashData(stream)), ["bytes"] = stream.Length };
    }
    private sealed class Bytes : IDisposable
    {
        public readonly byte* Pointer; public readonly int Length;
        public Bytes(string value)
        {
            var data = Encoding.UTF8.GetBytes(value); Length = data.Length; Pointer = (byte*)NativeMemory.Alloc(checked((nuint)Length + 1));
            if (Pointer == null) throw new OutOfMemoryException(); data.CopyTo(new Span<byte>(Pointer, Length)); Pointer[Length] = 0;
        }
        public void Dispose() => NativeMemory.Free(Pointer);
    }
    private sealed class Database
    {
        public CapturedDatabase* Pointer;
        public Database(string path, bool shared)
        {
            using var filename = new Bytes(path); CapturedDatabase* db = null;
            var code = Api.sqlite3_open_v2((sbyte*)filename.Pointer, &db, 6 | 0x10000 | (shared ? 0x20000 : 0x40000), null); Pointer = db;
            try { Check(code, "open"); Require(db != null, "database handle"); Check(Api.sqlite3_extended_result_codes(db, 1), "extended_result_codes"); }
            catch { if (Pointer != null) Api.sqlite3_close(Pointer); Pointer = null; throw; }
        }
        public void Check(int code, string operation) { if (code != 0) throw new InvalidOperationException(operation + ": " + code + ": " + Text(Api.sqlite3_errmsg(Pointer))); }
        public void Close()
        {
            if (Pointer == null) return;
            Check(Api.sqlite3_busy_handler(Pointer, 0, null), "cancel busy"); Check(Api.sqlite3_unlock_notify(Pointer, 0, null), "cancel notify");
            Check(Api.sqlite3_close(Pointer), "close"); Pointer = null;
        }
    }
    private static Database Open(string name, string path, bool shared = false)
    {
        Require(!handles.ContainsKey(name), "unique connection owner"); var db = new Database(path, shared);
        if (!handles.TryAdd(name, db)) { db.Close(); throw new InvalidOperationException("duplicate connection owner"); } return db;
    }
    private static void Close(string name) { var db = handles[name]; db.Close(); Require(handles.TryRemove(name, out _), "remove closed connection"); }
    private sealed class Statement
    {
        public void* Pointer; private readonly Database db;
        public Statement(Database db, string sql)
        {
            this.db = db; using var bytes = new Bytes(sql); void* stmt = null; sbyte* tail = null;
            var code = Api.sqlite3_prepare_v2(db.Pointer, (sbyte*)bytes.Pointer, bytes.Length + 1, &stmt, &tail);
            if (code != 0) { if (stmt != null) Api.sqlite3_finalize(stmt); db.Check(code, "prepare"); }
            try { Require(stmt != null && (byte*)tail == bytes.Pointer + bytes.Length, "exactly one SQL statement"); }
            catch { if (stmt != null) Api.sqlite3_finalize(stmt); throw; }
            Pointer = stmt; Require(pending.TryAdd((nint)stmt, this), "unique statement owner");
        }
        public JsonObject Read()
        {
            var count = Api.sqlite3_column_count(Pointer); Require(count is >= 0 and <= 8, "finite column bound");
            var columns = new JsonArray(); var rows = new JsonArray();
            for (var i = 0; i < count; ++i) columns.Add((JsonNode?)JsonValue.Create(Text(Api.sqlite3_column_name(Pointer, i))));
            int code;
            while ((code = Api.sqlite3_step(Pointer)) == 100)
            {
                var values = new JsonArray(); for (var i = 0; i < count; ++i) values.Add((JsonNode)Value(Pointer, i));
                rows.Add((JsonNode)values); Require(rows.Count <= work.GetProperty("maximumRows").GetInt32(), "finite row bound");
            }
            return new() { ["columns"] = columns, ["rows"] = rows, ["stepCode"] = code,
                ["primaryCode"] = Api.sqlite3_errcode(db.Pointer) & 255, ["extendedCode"] = Api.sqlite3_extended_errcode(db.Pointer) };
        }
        public int Finish()
        {
            var owned = Pointer; Require(owned != null, "statement finalized once"); Pointer = null;
            Require(pending.TryRemove((nint)owned, out _), "remove finalized statement"); return Api.sqlite3_finalize(owned);
        }
    }
    private static JsonObject Value(void* statement, int index)
    {
        var kind = Api.sqlite3_column_type(statement, index);
        if (kind == 1) return new() { ["type"] = "integer", ["decimal"] = Api.sqlite3_column_int64(statement, index).ToString(CultureInfo.InvariantCulture) };
        if (kind == 2) return new() { ["type"] = "real", ["binary64Hex"] = unchecked((ulong)BitConverter.DoubleToInt64Bits(Api.sqlite3_column_double(statement, index))).ToString("x16", CultureInfo.InvariantCulture) };
        if (kind is 3 or 4)
        {
            void* address = kind == 3 ? Api.sqlite3_column_text(statement, index) : Api.sqlite3_column_blob(statement, index);
            var size = Api.sqlite3_column_bytes(statement, index);
            Require(size >= 0 && size <= work.GetProperty("maximumColumnBytes").GetInt32() && (size == 0 || address != null), "borrowed byte extent");
            return new() { ["type"] = kind == 3 ? "text-utf8" : "blob", ["bytes"] = size,
                ["hex"] = size == 0 ? "" : Convert.ToHexStringLower(new ReadOnlySpan<byte>(address, size)) };
        }
        Require(kind == 5, "known SQLite storage class"); return new() { ["type"] = "null" };
    }
    private static JsonObject Sql(string name, string sql, int expected = 101)
    {
        var stmt = new Statement(handles[name], sql); JsonObject observation; int code;
        try { observation = stmt.Read(); } finally { code = stmt.Finish(); }
        observation["finalizeCode"] = code;
        Require(observation["stepCode"]!.GetValue<int>() == expected && code == (expected == 101 ? 0 : expected), "exact step/finalize result");
        Require(busyContext->Error == 0 && notifyContext->Error == 0, "callback contained failure"); return observation;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static int Busy(void* context, int attempt)
    {
        try
        {
            var state = (Context*)context;
            if (state == null || state != busyContext || state->Magic != 0x431 || state->Id != 1 || state->BusyCount is < 0 or >= 16)
            { if (busyContext != null) busyContext->Error = 1; return 0; }
            state->BusyArguments[state->BusyCount++] = attempt;
        }
        catch { if (busyContext != null) busyContext->Error = 1; }
        return 0;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void Notify(void** contexts, int count)
    {
        try
        {
            if (contexts == null || count is <= 0 or > 4 || notifyContext == null) { if (notifyContext != null) notifyContext->Error = 1; return; }
            for (var i = 0; i < count; ++i)
            {
                var state = (Context*)contexts[i];
                if (state == null || state != notifyContext || state->Magic != 0x431 || state->Id != 2 || state->NotifyCount != 0)
                { notifyContext->Error = 1; return; }
                state->NotifyIds[i] = state->Id;
            }
            notifyContext->GroupCount = count; ++notifyContext->NotifyCount;
        }
        catch { if (notifyContext != null) notifyContext->Error = 1; }
    }
    private static JsonArray BusyArguments()
    {
        Require(busyContext->Error == 0 && busyContext->BusyCount is >= 0 and <= 16, "busy callback bound"); var array = new JsonArray();
        for (var i = 0; i < busyContext->BusyCount; ++i) array.Add((JsonNode?)JsonValue.Create(busyContext->BusyArguments[i])); return array;
    }
    private static JsonObject Shared()
    {
        var path = Path.Combine(artifacts, "shared.db"); Open("one", path, true);
        foreach (var statement in work.GetProperty("sharedSetup").EnumerateArray()) Sql("one", statement.GetString()!);
        var two = Open("two", path, true); Sql("one", "BEGIN IMMEDIATE"); Sql("one", work.GetProperty("sharedWrite").GetString()!);
        var blocked = new Statement(two, work.GetProperty("read").GetString()!); var observation = blocked.Read();
        Require(observation["stepCode"]!.GetValue<int>() == 262 && observation["primaryCode"]!.GetValue<int>() == 6
            && observation["extendedCode"]!.GetValue<int>() == 262, "real shared-cache table lock");
        two.Check(Api.sqlite3_unlock_notify(two.Pointer, (nint)(delegate* unmanaged[Cdecl]<void**, int, void>)&Notify, notifyContext), "unlock_notify");
        var before = notifyContext->NotifyCount; Require(before == 0, "writer still holds lock before release"); Sql("one", "COMMIT");
        Require(notifyContext->NotifyCount == 1 && notifyContext->Error == 0, "commit triggers notification");
        var finalize = blocked.Finish(); observation["finalizeCode"] = finalize; Require(finalize == 262, "blocked statement finalized once");
        var retry = Sql("two", work.GetProperty("read").GetString()!); two.Check(Api.sqlite3_unlock_notify(two.Pointer, 0, null), "cancel notify");
        var ids = new JsonArray(); for (var i = 0; i < notifyContext->GroupCount; ++i) ids.Add((JsonNode?)JsonValue.Create(notifyContext->NotifyIds[i]));
        var events = new JsonArray(); events.Add((JsonNode)new JsonObject { ["count"] = notifyContext->GroupCount, ["contexts"] = ids });
        Close("two"); Close("one"); return new() { ["blocked"] = observation, ["notificationsBeforeRelease"] = before, ["notifications"] = events, ["retry"] = retry };
    }
    private static JsonObject Threads()
    {
        var rounds = work.GetProperty("threadRounds").GetInt32(); Require(rounds == 3, "finite read rounds");
        using var barrier = new Barrier(2); using var cancel = new CancellationTokenSource(); var errors = new ConcurrentQueue<Exception>();
        var observations = new JsonObject?[2]; var workers = new Thread[2];
        void Wait() => Require(barrier.SignalAndWait(TimeSpan.FromSeconds(30), cancel.Token), "thread barrier safety watchdog");
        void Body(int index)
        {
            var name = "thread" + index;
            try
            {
                Open(name, databasePath); Wait(); var results = new JsonArray();
                for (var round = 0; round < rounds; ++round) { Wait(); results.Add((JsonNode)Sql(name, work.GetProperty("read").GetString()!)); Wait(); }
                Close(name); observations[index] = new() { ["thread"] = index, ["rounds"] = results };
            }
            catch (Exception error) { errors.Enqueue(error); cancel.Cancel(); }
        }
        for (var i = 0; i < workers.Length; ++i) { var index = i; workers[i] = new Thread(() => Body(index)) { Name = "sqlite-read-" + i }; workers[i].Start(); }
        foreach (var thread in workers) thread.Join(); // Outer command watchdog bounds a stuck native/provider call.
        Require(errors.IsEmpty, "joined read threads without failures: " + string.Join("; ", errors.Select(error => error.Message)));
        Require(observations[0] != null && observations[1] != null && JsonNode.DeepEquals(observations[0]!["rounds"], observations[1]!["rounds"]), "per-thread read observations equal");
        var array = new JsonArray(); array.Add((JsonNode?)observations[0]); array.Add((JsonNode?)observations[1]); return new() { ["threads"] = array, ["joined"] = true };
    }
    private static void Emit(JsonObject message) { Console.WriteLine(message.ToJsonString()); Console.Out.Flush(); }
    private static int Main(string[] args)
    {
        Require(args.Length == 10, "Checks --api-surface REPORT --workload JSON --database FILE --artifacts NEW --worker A|B");
        var options = new Dictionary<string, string>(StringComparer.Ordinal);
        for (var i = 0; i < args.Length; i += 2) options.Add(args[i], args[i + 1]);
        databasePath = Path.GetFullPath(options["--database"]); artifacts = Path.GetFullPath(options["--artifacts"]);
        Require(!Directory.Exists(artifacts) && !File.Exists(artifacts), "fresh worker artifact directory"); Directory.CreateDirectory(artifacts);
        var source = Record(options["--workload"]); Require(source["sha256"]!.GetValue<string>() == WorkloadSha, "pinned concurrency workload");
        using var document = JsonDocument.Parse(File.ReadAllText(options["--workload"])); work = document.RootElement;
        var api = JsonNode.Parse(File.ReadAllText(options["--api-surface"]))!;
        Require(api["status"]!.GetValue<string>() == "validated-published-concurrency-surface", "validated actual API surface");
        Require(work.GetProperty("workloadVersion").GetInt32() == 1 && Api.sqlite3_libversion_number() == 3054000 && Api.sqlite3_threadsafe() == 1, "pinned THREADSAFE=1 SQLite");
        busyContext = (Context*)NativeMemory.AllocZeroed((nuint)sizeof(Context)); notifyContext = (Context*)NativeMemory.AllocZeroed((nuint)sizeof(Context));
        if (busyContext == null || notifyContext == null) throw new OutOfMemoryException();
        busyContext->Magic = notifyContext->Magic = 0x431; busyContext->Id = 1; notifyContext->Id = 2;
        var commands = new JsonArray(); var report = new JsonObject { ["status"] = "incomplete-generated-concurrency-worker", ["role"] = "generated-csharp-application-only",
            ["nativeApplicationFallbackPermitted"] = false, ["workload"] = source, ["apiSurface"] = Record(options["--api-surface"]), ["worker"] = options["--worker"], ["commands"] = commands };
        Emit(new() { ["ready"] = true, ["worker"] = options["--worker"], ["sqliteVersion"] = 3054000, ["threadsafe"] = 1 }); var result = 1;
        try
        {
            string? line;
            while ((line = Console.ReadLine()) != null)
            {
                Require(commands.Count < work.GetProperty("maximumCommands").GetInt32(), "finite command bound");
                using var commandDocument = JsonDocument.Parse(line); var command = commandDocument.RootElement; var id = command.GetProperty("id").GetString();
                var operation = command.GetProperty("op").GetString();
                try
                {
                    JsonObject observation;
                    switch (operation)
                    {
                        case "open": Open("main", databasePath); observation = new() { ["code"] = 0 }; break;
                        case "close": Close("main"); observation = new() { ["code"] = 0 }; break;
                        case "busy": handles["main"].Check(Api.sqlite3_busy_handler(handles["main"].Pointer,
                            (nint)(delegate* unmanaged[Cdecl]<void*, int, int>)&Busy, busyContext), "busy_handler"); observation = new() { ["code"] = 0 }; break;
                        case "sql": observation = Sql("main", command.GetProperty("sql").GetString()!, command.TryGetProperty("expectedStep", out var expected) ? expected.GetInt32() : 101); break;
                        case "shared": observation = Shared(); break;
                        case "threads": observation = Threads(); break;
                        case "quit": Require(handles.IsEmpty && pending.IsEmpty, "connections/statements closed before quit"); observation = new() { ["resourcesReleased"] = true }; result = 0; break;
                        default: throw new InvalidOperationException("Unknown finite operation");
                    }
                    observation["busyArguments"] = BusyArguments(); var ack = new JsonObject { ["id"] = id, ["ok"] = true, ["observation"] = observation };
                    commands.Add((JsonNode)ack); Emit(ack); if (operation == "quit") break;
                }
                catch (Exception error)
                {
                    var ack = new JsonObject { ["id"] = id, ["ok"] = false, ["error"] = error.ToString() }; commands.Add((JsonNode)ack); Emit(ack); break;
                }
            }
        }
        finally
        {
            var cleanup = new JsonArray();
            foreach (var statement in pending.Values.ToArray())
                try { statement.Finish(); } catch (Exception error) { cleanup.Add((JsonNode?)JsonValue.Create(error.ToString())); }
            foreach (var name in handles.Keys.ToArray())
                try { Close(name); } catch (Exception error) { cleanup.Add((JsonNode?)JsonValue.Create(error.ToString())); }
            var callbackErrors = new JsonArray();
            if (busyContext->Error != 0 || notifyContext->Error != 0) callbackErrors.Add((JsonNode?)JsonValue.Create("contained callback context/invariant failure"));
            report["resourcesReleased"] = handles.IsEmpty && pending.IsEmpty && cleanup.Count == 0; report["cleanupErrors"] = cleanup; report["callbackErrors"] = callbackErrors;
            try
            {
                var libraries = new JsonArray();
                foreach (var path in File.ReadLines("/proc/self/maps").Select(line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                    .Where(parts => parts.Length >= 6 && parts[^1].StartsWith('/') && parts[^1].Contains(".so", StringComparison.Ordinal))
                    .Select(parts => parts[^1]).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))
                { Require(!Path.GetFileName(path).Contains("sqlite", StringComparison.OrdinalIgnoreCase), "no native SQLite application dependency"); libraries.Add((JsonNode)Record(path)); }
                report["loadedLibraries"] = libraries;
            }
            catch (Exception error) { report["dependencyError"] = error.ToString(); result = 1; }
            if (result == 0 && callbackErrors.Count == 0 && report["resourcesReleased"]!.GetValue<bool>()) report["status"] = "passed-generated-concurrency-worker";
            else result = 1;
            if (handles.IsEmpty) { NativeMemory.Free(busyContext); NativeMemory.Free(notifyContext); busyContext = null; notifyContext = null; }
            var files = new JsonArray(); foreach (var path in Directory.GetFiles(artifacts).Order(StringComparer.Ordinal)) files.Add((JsonNode)Record(path)); report["files"] = files;
            File.WriteAllText(Path.Combine(artifacts, "worker-report.json"), report.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n");
        }
        return result;
    }
}
