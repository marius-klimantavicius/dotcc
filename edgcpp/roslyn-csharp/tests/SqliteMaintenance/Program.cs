using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

internal static unsafe class MaintenanceChecks
{
    private const string WorkloadSha = "8f056be066db0b291929a750dae021202f32a2c30a96be9bb0102c6fafd221eb";
    private const string OracleSha = "39d00f19c6778ab90a51bdfdb8e6f13db6d733482112e8587c0e30eccb284c6a";
    private static readonly List<Database> databases = [];
    private static readonly List<Statement> statements = [];
    private static CapturedSnapshot* oldSnapshot, newSnapshot;
    private static CapturedRbu* rbu;
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private static string Text(sbyte* address) => address == null ? throw new InvalidOperationException("Missing borrowed string") : Marshal.PtrToStringUTF8((nint)address)!;
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
            var bytes = Encoding.UTF8.GetBytes(value); Length = bytes.Length;
            Pointer = (byte*)NativeMemory.Alloc(checked((nuint)Length + 1));
            if (Pointer == null) throw new OutOfMemoryException();
            bytes.CopyTo(new Span<byte>(Pointer, Length)); Pointer[Length] = 0;
        }
        public void Dispose() => NativeMemory.Free(Pointer);
    }
    private sealed class Database : IDisposable
    {
        public CapturedDatabase* Pointer;
        public Database(string path)
        {
            using var name = new Bytes(path); CapturedDatabase* pointer = null;
            var code = Api.sqlite3_open_v2((sbyte*)name.Pointer, &pointer, 6, null); Pointer = pointer;
            if (code != 0)
            {
                var message = pointer == null ? "no handle" : Text(Api.sqlite3_errmsg(pointer));
                if (pointer != null) Api.sqlite3_close(pointer); Pointer = null; throw new InvalidOperationException(message);
            }
            Require(pointer != null, "open returned no database"); databases.Add(this);
        }
        public void Check(int code, string operation)
        {
            if (code != 0) throw new InvalidOperationException(operation + ": " + code + ": " + Text(Api.sqlite3_errmsg(Pointer)));
        }
        public JsonObject Sql(string sql) { using var statement = new Statement(this, sql); return statement.Read(); }
        public void Setup(JsonElement items) { foreach (var item in items.EnumerateArray()) Sql(item.GetString()!); }
        public void Dispose()
        {
            if (Pointer == null) return;
            Check(Api.sqlite3_close(Pointer), "close"); Pointer = null;
        }
    }
    private sealed class Statement : IDisposable
    {
        public void* Pointer; private readonly Database database;
        public Statement(Database database, string sql)
        {
            this.database = database; using var bytes = new Bytes(sql); void* statement = null; sbyte* tail = null;
            var code = Api.sqlite3_prepare_v2(database.Pointer, (sbyte*)bytes.Pointer, bytes.Length + 1, &statement, &tail);
            if (code != 0) { if (statement != null) Api.sqlite3_finalize(statement); database.Check(code, "prepare"); }
            try { Require(statement != null && (byte*)tail == bytes.Pointer + bytes.Length, "exactly one SQL statement"); }
            catch { if (statement != null) Api.sqlite3_finalize(statement); throw; }
            Pointer = statement; statements.Add(this);
        }
        public JsonObject Read()
        {
            var n = Api.sqlite3_column_count(Pointer); Require(n is >= 0 and <= 8, "finite column bound");
            var columns = new JsonArray(); var rows = new JsonArray();
            for (var i = 0; i < n; ++i) columns.Add((JsonNode?)JsonValue.Create(Text(Api.sqlite3_column_name(Pointer, i))));
            while (true)
            {
                var code = Api.sqlite3_step(Pointer); if (code == 101) break;
                if (code != 100) { database.Check(code, "step"); throw new InvalidOperationException("expected ROW or DONE"); }
                var values = new JsonArray();
                for (var i = 0; i < n; ++i) values.Add((JsonNode)ColumnValue(Pointer, i));
                rows.Add((JsonNode)values); Require(rows.Count <= 32, "finite row bound");
            }
            return new() { ["columns"] = columns, ["rows"] = rows };
        }
        public void Dispose()
        {
            if (Pointer == null) return; var owned = Pointer; Pointer = null;
            var code = Api.sqlite3_finalize(owned); statements.Remove(this); database.Check(code, "finalize");
        }
    }
    private static JsonObject ColumnValue(void* statement, int index)
    {
        var kind = Api.sqlite3_column_type(statement, index);
        if (kind == 1) return new() { ["type"] = "integer", ["decimal"] = Api.sqlite3_column_int64(statement, index).ToString(CultureInfo.InvariantCulture) };
        if (kind == 2) return new() { ["type"] = "real", ["binary64Hex"] = unchecked((ulong)BitConverter.DoubleToInt64Bits(Api.sqlite3_column_double(statement, index))).ToString("x16", CultureInfo.InvariantCulture) };
        if (kind is 3 or 4)
        {
            void* pointer = kind == 3 ? Api.sqlite3_column_text(statement, index) : Api.sqlite3_column_blob(statement, index);
            var count = Api.sqlite3_column_bytes(statement, index);
            Require(count is >= 0 and <= 1024 && (count == 0 || pointer != null), "finite borrowed value");
            return new() { ["type"] = kind == 3 ? "text-utf8" : "blob", ["bytes"] = count,
                ["hex"] = count == 0 ? "" : Convert.ToHexStringLower(new ReadOnlySpan<byte>(pointer, count)) };
        }
        Require(kind == 5, "known SQLite storage class"); return new() { ["type"] = "null" };
    }
    private static CapturedSnapshot* GetSnapshot(Database db, Bytes schema)
    {
        CapturedSnapshot* snapshot = null;
        db.Check(Api.sqlite3_snapshot_get(db.Pointer, (sbyte*)schema.Pointer, &snapshot), "snapshot_get");
        Require(snapshot != null, "owned snapshot handle"); return snapshot;
    }
    private static JsonObject Snapshots(JsonElement work, string directory)
    {
        using var schema = new Bytes("main"); var path = Path.Combine(directory, "snapshot.db");
        using var writer = new Database(path); writer.Setup(work.GetProperty("setup"));
        using var pin = new Database(path); pin.Sql("BEGIN"); oldSnapshot = GetSnapshot(pin, schema);
        var oldRows = pin.Sql(work.GetProperty("read").GetString()!);
        writer.Setup(work.GetProperty("mutations")); writer.Sql("BEGIN"); newSnapshot = GetSnapshot(writer, schema);
        var newRows = writer.Sql(work.GetProperty("read").GetString()!); writer.Sql("COMMIT");
        var oldNew = Math.Sign(Api.sqlite3_snapshot_cmp(oldSnapshot, newSnapshot));
        var newOld = Math.Sign(Api.sqlite3_snapshot_cmp(newSnapshot, oldSnapshot));
        var same = Api.sqlite3_snapshot_cmp(oldSnapshot, oldSnapshot);
        Require(oldNew == -1 && newOld == 1 && same == 0, "snapshot chronology in one WAL generation");
        var pinned = pin.Sql(work.GetProperty("read").GetString()!);
        using var replay = new Database(path); replay.Sql(work.GetProperty("initializeReplay").GetString()!);
        replay.Check(Api.sqlite3_snapshot_recover(replay.Pointer, (sbyte*)schema.Pointer), "snapshot_recover");
        var replayed = new JsonArray();
        replay.Sql("BEGIN"); replay.Check(Api.sqlite3_snapshot_open(replay.Pointer, (sbyte*)schema.Pointer, oldSnapshot), "snapshot_open old");
        var replayOld = replay.Sql(work.GetProperty("read").GetString()!); replay.Sql("COMMIT"); replayed.Add((JsonNode)replayOld);
        replay.Sql("BEGIN"); replay.Check(Api.sqlite3_snapshot_open(replay.Pointer, (sbyte*)schema.Pointer, newSnapshot), "snapshot_open new");
        var replayNew = replay.Sql(work.GetProperty("read").GetString()!); replay.Sql("COMMIT"); replayed.Add((JsonNode)replayNew);
        Require(JsonNode.DeepEquals(pinned, oldRows) && JsonNode.DeepEquals(replayOld, oldRows)
            && JsonNode.DeepEquals(replayNew, newRows) && !JsonNode.DeepEquals(oldRows, newRows), "historical visibility");
        var oldOwned = oldSnapshot; oldSnapshot = null; Api.sqlite3_snapshot_free(oldOwned);
        var newOwned = newSnapshot; newSnapshot = null; Api.sqlite3_snapshot_free(newOwned);
        pin.Sql("COMMIT"); replay.Dispose(); pin.Dispose();
        return new() { ["old"] = oldRows, ["new"] = newRows, ["pinnedAfterWrite"] = pinned,
            ["comparisonSigns"] = new JsonObject { ["oldNew"] = oldNew, ["newOld"] = newOld, ["same"] = same },
            ["recoverCode"] = 0, ["replayed"] = replayed, ["checkpointAfterRelease"] = writer.Sql(work.GetProperty("checkpoint").GetString()!),
            ["integrity"] = writer.Sql(work.GetProperty("integrity").GetString()!), ["snapshotsFreed"] = 2 };
    }
    private static JsonObject RbuState()
    {
        var state = Api.sqlite3rbu_state(rbu); var progress = Api.sqlite3rbu_progress(rbu); int one = 0, two = 0;
        Api.sqlite3rbu_bp_progress(rbu, &one, &two);
        Require(state is >= 1 and <= 4 && progress >= 0 && one is >= -1 and <= 10000 && two is >= 0 and <= 10000, "RBU documented state/progress bounds");
        return new() { ["state"] = state, ["progress"] = progress.ToString(CultureInfo.InvariantCulture), ["stageOne"] = one, ["stageTwo"] = two };
    }
    private static long Progress(JsonObject state) => long.Parse(state["progress"]!.GetValue<string>(), CultureInfo.InvariantCulture);
    private static void OpenRbu(Bytes target, Bytes update, Bytes state)
    {
        rbu = Api.sqlite3rbu_open((sbyte*)target.Pointer, (sbyte*)update.Pointer, (sbyte*)state.Pointer);
        Require(rbu != null, "RBU handle allocation");
        Require(Api.sqlite3rbu_db(rbu, 0) != null && Api.sqlite3rbu_db(rbu, 1) != null, "borrowed RBU-owned connections");
    }
    private static int CloseRbu(int expected)
    {
        var owned = rbu; rbu = null; sbyte* message = null;
        var code = Api.sqlite3rbu_close(owned, &message);
        try { Require(code == expected, "rbu_close " + code + (message == null ? "" : ": " + Text(message))); }
        finally { if (message != null) Api.sqlite3_free(message); }
        return code;
    }
    private static JsonObject Rbu(JsonElement work, string directory)
    {
        var targetPath = Path.Combine(directory, "target.db"); var updatePath = Path.Combine(directory, "update.db"); var statePath = Path.Combine(directory, "rbu-state.db");
        JsonObject before;
        using (var db = new Database(targetPath)) { db.Setup(work.GetProperty("targetSetup")); before = db.Sql(work.GetProperty("read").GetString()!); }
        using (var db = new Database(updatePath)) db.Setup(work.GetProperty("updateSetup"));
        using var target = new Bytes(targetPath); using var update = new Bytes(updatePath); using var state = new Bytes(statePath);
        OpenRbu(target, update, state); var opened = RbuState(); var paused = new JsonArray(); JsonObject? last = null;
        var pauseCap = work.GetProperty("maximumPauseSteps").GetInt32(); Require(pauseCap is > 0 and <= 16, "finite RBU pause cap");
        for (var i = 0; i < pauseCap; ++i)
        {
            var code = Api.sqlite3rbu_step(rbu); Require(code == 0, "partial RBU step remains unfinished");
            last = RbuState(); last["code"] = code; paused.Add((JsonNode)last); if (Progress(last) > 0) break;
        }
        Require(last != null && Progress(last) > 0 && last["state"]!.GetValue<int>() == 1, "pause after staged work before publication");
        Require(Api.sqlite3rbu_savestate(rbu) == 0, "save RBU state"); var saved = RbuState(); var partialClose = CloseRbu(0);
        JsonObject unpublished;
        using (var db = new Database(targetPath)) unpublished = db.Sql(work.GetProperty("read").GetString()!);
        Require(JsonNode.DeepEquals(unpublished, before), "unfinished OAL changes remain unpublished");
        OpenRbu(target, update, state); var resumed = RbuState(); Require(Progress(resumed) == Progress(saved), "saved progress restored");
        var steps = new JsonArray(); var previous = Progress(resumed); var done = false;
        var cap = work.GetProperty("maximumSteps").GetInt32(); Require(cap is > 0 and <= 128, "finite RBU completion cap");
        for (var i = 0; i < cap; ++i)
        {
            var code = Api.sqlite3rbu_step(rbu); Require(code is 0 or 101, "RBU step success/DONE");
            last = RbuState(); last["code"] = code; steps.Add((JsonNode)last);
            Require(Progress(last) >= previous, "monotonic RBU progress"); previous = Progress(last);
            if (code == 101) { done = true; break; }
        }
        Require(done && last!["state"]!.GetValue<int>() == 4, "RBU completed within finite cap");
        var repeatedDone = Api.sqlite3rbu_step(rbu); Require(repeatedDone == 101, "completed RBU remains DONE");
        var completeClose = CloseRbu(101); using var result = new Database(targetPath);
        return new() { ["before"] = before, ["opened"] = opened, ["pauseSteps"] = paused, ["saved"] = saved,
            ["partialCloseCode"] = partialClose, ["unpublished"] = unpublished, ["resumed"] = resumed, ["completionSteps"] = steps,
            ["repeatedDoneCode"] = repeatedDone, ["completeCloseCode"] = completeClose, ["result"] = result.Sql(work.GetProperty("read").GetString()!),
            ["indexResult"] = result.Sql(work.GetProperty("indexRead").GetString()!), ["integrity"] = result.Sql(work.GetProperty("integrity").GetString()!) };
    }
    private static int Main(string[] args)
    {
        Require(args.Length == 4, "Checks WORKLOAD NEW_ARTIFACTS NATIVE_REPORT API_SURFACE_REPORT");
        Require(!Directory.Exists(args[1]) && !File.Exists(args[1]), "fresh artifact directory"); Directory.CreateDirectory(args[1]);
        var directory = Path.GetFullPath(args[1]); var reportPath = Path.Combine(directory, "generated-maintenance.json");
        var workReceipt = Record(args[0]); Require(workReceipt["sha256"]!.GetValue<string>() == WorkloadSha, "pinned maintenance workload");
        using var document = JsonDocument.Parse(File.ReadAllText(args[0])); var work = document.RootElement;
        var oracle = JsonNode.Parse(File.ReadAllText(args[2]))!; var surface = JsonNode.Parse(File.ReadAllText(args[3]))!;
        Require(oracle["status"]!.GetValue<string>() == "passed-native-maintenance" && oracle["role"]!.GetValue<string>() == "separate-native-test-oracle-only"
            && oracle["workload"]!["sha256"]!.GetValue<string>() == WorkloadSha && oracle["library"]!["sha256"]!.GetValue<string>() == OracleSha
            && oracle["resourcesReleased"]!.GetValue<bool>(), "exact separate native evidence");
        Require(surface["status"]!.GetValue<string>() == "validated-published-maintenance-surface", "validated actual maintenance API surface");
        var report = new JsonObject { ["status"] = "incomplete-generated-maintenance", ["role"] = "generated-csharp-application-only",
            ["nativeApplicationFallbackPermitted"] = false, ["workload"] = workReceipt, ["nativeReference"] = Record(args[2]), ["apiSurface"] = Record(args[3]),
            ["limits"] = JsonNode.Parse(work.GetProperty("limits").GetRawText()) }; var result = 1;
        try
        {
            Require(Api.sqlite3_libversion_number() == 3054000 && work.GetProperty("workloadVersion").GetInt32() == 1, "pinned SQLite/workload version");
            Require(sizeof(CapturedSnapshot) == 48 && sizeof(CapturedRbu) == 416, "actual generated handle sizes");
            var observations = new JsonObject(); report["observations"] = observations;
            observations["snapshot"] = Snapshots(work.GetProperty("snapshot"), directory);
            observations["rbu"] = Rbu(work.GetProperty("rbu"), directory);
            Require(JsonNode.DeepEquals(observations, oracle["observations"]), "exact native maintenance observations");
            report["nativeComparison"] = "exact-observations-equal"; report["status"] = "passed-generated-maintenance"; result = 0;
        }
        catch (Exception error) { report["error"] = error.ToString(); }
        finally
        {
            var cleanup = new JsonArray();
            try { if (rbu != null) CloseRbu(0); } catch (Exception error) { cleanup.Add((JsonNode?)JsonValue.Create(error.ToString())); }
            if (oldSnapshot != null) { var owned = oldSnapshot; oldSnapshot = null; Api.sqlite3_snapshot_free(owned); }
            if (newSnapshot != null) { var owned = newSnapshot; newSnapshot = null; Api.sqlite3_snapshot_free(owned); }
            foreach (var statement in statements.ToArray())
                try { statement.Dispose(); } catch (Exception error) { cleanup.Add((JsonNode?)JsonValue.Create(error.ToString())); }
            foreach (var db in databases.AsEnumerable().Reverse())
                try { db.Dispose(); } catch (Exception error) { cleanup.Add((JsonNode?)JsonValue.Create(error.ToString())); }
            report["cleanupErrors"] = cleanup; report["resourcesReleased"] = cleanup.Count == 0 && statements.Count == 0
                && databases.All(db => db.Pointer == null) && rbu == null && oldSnapshot == null && newSnapshot == null;
            if (cleanup.Count != 0 || !report["resourcesReleased"]!.GetValue<bool>()) { result = 1; report["status"] = "incomplete-generated-maintenance"; }
            var files = new JsonArray(); foreach (var path in Directory.GetFiles(directory).Order(StringComparer.Ordinal)) files.Add((JsonNode)Record(path));
            report["files"] = files; var libraries = new JsonArray();
            try
            {
                foreach (var path in File.ReadLines("/proc/self/maps").Select(line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                    .Where(parts => parts.Length >= 6 && parts[^1].StartsWith('/') && parts[^1].Contains(".so", StringComparison.Ordinal))
                    .Select(parts => parts[^1]).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))
                {
                    Require(!Path.GetFileName(path).Contains("sqlite", StringComparison.OrdinalIgnoreCase), "no native SQLite application dependency");
                    libraries.Add((JsonNode)Record(path));
                }
                report["loadedLibraries"] = libraries;
            }
            catch (Exception error) { report["dependencyError"] = error.ToString(); result = 1; report["status"] = "incomplete-generated-maintenance"; }
            File.WriteAllText(reportPath, report.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n");
        }
        Console.WriteLine(new JsonObject { ["status"] = report["status"]!.GetValue<string>(), ["report"] = Record(reportPath) }.ToJsonString()); return result;
    }
}
