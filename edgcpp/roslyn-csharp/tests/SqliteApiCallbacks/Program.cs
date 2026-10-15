using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

internal static unsafe class ApiCallbackChecks
{
    private const string WorkloadSha = "b5d18a84906ae21cf2c75e04f9ade60cba35afcdbdd45e0f84d1970e68e7d8db";
    private struct State { public int Magic, Error, Count, Outstanding, Pass; public fixed int Ids[8]; }
    private struct Owner { public State* Context; public int Id; }
    private static State* active;
    private static readonly List<Database> databases = [];
    private static readonly List<Statement> statements = [];
    private static readonly JsonArray preEvents = new(), applyEvents = new();
    private static CapturedSession* session;
    private static CapturedIterator* iterator;
    private static CapturedBackup* backup;
    private static void* changeset;
    private static void Require(bool condition, string check) { if (!condition) throw new InvalidOperationException(check); }
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
            var encoded = Encoding.UTF8.GetBytes(value); Length = encoded.Length;
            Pointer = (byte*)NativeMemory.Alloc(checked((nuint)Length + 1));
            if (Pointer == null) throw new OutOfMemoryException();
            encoded.CopyTo(new Span<byte>(Pointer, Length)); Pointer[Length] = 0;
        }
        public void Dispose() => NativeMemory.Free(Pointer);
    }
    private sealed class Database : IDisposable
    {
        public CapturedDatabase* Pointer;
        public Database()
        {
            using var name = new Bytes(":memory:"); CapturedDatabase* pointer = null;
            var code = Api.sqlite3_open_v2((sbyte*)name.Pointer, &pointer, 6, null); Pointer = pointer;
            if (code != 0) { var message = pointer == null ? "no handle" : Text(Api.sqlite3_errmsg(pointer)); if (pointer != null) Api.sqlite3_close(pointer); Pointer = null; throw new InvalidOperationException(message); }
            Require(pointer != null, "open returned no database"); databases.Add(this);
        }
        public void Check(int code, string operation)
        {
            if (code != 0) throw new InvalidOperationException(operation + ": " + code + ": " + Text(Api.sqlite3_errmsg(Pointer)));
        }
        public JsonObject Sql(string sql) { using var statement = new Statement(this, sql); return statement.Read(); }
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
            try { Require(statement != null && (byte*)tail == bytes.Pointer + bytes.Length, "expected exactly one SQL statement"); }
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
                if (code != 100) { database.Check(code, "step"); throw new InvalidOperationException("step returned neither ROW nor DONE"); }
                var values = new JsonArray();
                for (var i = 0; i < n; ++i) values.Add((JsonNode)ColumnValue(Pointer, i));
                rows.Add((JsonNode)values); Require(rows.Count <= 16, "finite row bound");
            }
            Require(active->Error == 0, "callback contained an exception/invariant error");
            return new() { ["columns"] = columns, ["rows"] = rows };
        }
        public void Reset() => database.Check(Api.sqlite3_reset(Pointer), "reset");
        public void Clear() => database.Check(Api.sqlite3_clear_bindings(Pointer), "clear_bindings");
        public void Dispose()
        {
            if (Pointer == null) return; var statement = Pointer; Pointer = null;
            var code = Api.sqlite3_finalize(statement); statements.Remove(this); database.Check(code, "finalize");
        }
    }
    private static JsonObject Integer(long value) => new() { ["type"] = "integer", ["decimal"] = value.ToString(CultureInfo.InvariantCulture) };
    private static JsonObject Real(double value) => new() { ["type"] = "real", ["binary64Hex"] = unchecked((ulong)BitConverter.DoubleToInt64Bits(value)).ToString("x16", CultureInfo.InvariantCulture) };
    private static JsonObject Data(int kind, void* address, int length)
    {
        Require(length is >= 0 and <= 1024 && (length == 0 || address != null), "finite borrowed byte extent");
        return new() { ["type"] = kind == 3 ? "text-utf8" : "blob", ["bytes"] = length,
            ["hex"] = length == 0 ? "" : Convert.ToHexStringLower(new ReadOnlySpan<byte>(address, length)) };
    }
    private static JsonObject ColumnValue(void* statement, int index)
    {
        var kind = Api.sqlite3_column_type(statement, index);
        if (kind == 1) return Integer(Api.sqlite3_column_int64(statement, index));
        if (kind == 2) return Real(Api.sqlite3_column_double(statement, index));
        if (kind is 3 or 4)
        {
            void* pointer = kind == 3 ? Api.sqlite3_column_text(statement, index) : Api.sqlite3_column_blob(statement, index);
            return Data(kind, pointer, Api.sqlite3_column_bytes(statement, index));
        }
        Require(kind == 5, "known SQLite storage class"); return new() { ["type"] = "null" };
    }
    private static JsonObject ProtectedValue(CapturedValue* value)
    {
        if (value == null) return new() { ["type"] = "undefined" };
        var kind = Api.sqlite3_value_type(value);
        if (kind == 1) return Integer(Api.sqlite3_value_int64(value));
        if (kind == 2) return Real(Api.sqlite3_value_double(value));
        if (kind is 3 or 4)
        {
            void* pointer = kind == 3 ? Api.sqlite3_value_text(value) : Api.sqlite3_value_blob(value);
            return Data(kind, pointer, Api.sqlite3_value_bytes(value));
        }
        Require(kind == 5, "known protected storage class"); return new() { ["type"] = "null" };
    }
    private static State* Context(void* pointer)
    {
        Require(pointer == active && active != null && active->Magic == 0x417, "fixed native callback userdata"); return active;
    }
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void Destroy(void* data)
    {
        var owner = (Owner*)((byte*)data - sizeof(Owner));
        try
        {
            try
            {
                var state = Context(owner->Context); Require(state->Count < 8 && state->Outstanding > 0, "destructor count/ownership");
                state->Ids[state->Count++] = owner->Id; --state->Outstanding;
            }
            finally { NativeMemory.Free(owner); }
        }
        catch { if (active != null) active->Error = 1; }
    }
    private static void BindOwner(Database database, Statement statement, JsonElement entry)
    {
        var bytes = Convert.FromHexString(entry.GetProperty("hex").GetString()!);
        var owner = (Owner*)NativeMemory.Alloc(checked((nuint)sizeof(Owner) + (nuint)bytes.Length + 1));
        if (owner == null) throw new OutOfMemoryException(); owner->Context = active; owner->Id = entry.GetProperty("id").GetInt32();
        var payload = (byte*)owner + sizeof(Owner); bytes.CopyTo(new Span<byte>(payload, bytes.Length)); payload[bytes.Length] = 0;
        ++active->Outstanding;
        database.Check(Api.sqlite3_bind_blob(statement.Pointer, 1, payload, bytes.Length, (nint)(delegate* unmanaged[Cdecl]<void*, void>)&Destroy), "bind custom owner");
    }
    private static JsonObject Ownership(JsonElement work)
    {
        var database = new Database(); using var statement = new Statement(database, work.GetProperty("sql").GetString()!);
        var buffers = work.GetProperty("buffers"); var counts = new JsonArray();
        BindOwner(database, statement, buffers[0]); var first = statement.Read(); statement.Reset(); counts.Add((JsonNode?)JsonValue.Create(active->Count));
        BindOwner(database, statement, buffers[1]); counts.Add((JsonNode?)JsonValue.Create(active->Count)); var second = statement.Read(); statement.Reset();
        statement.Clear(); counts.Add((JsonNode?)JsonValue.Create(active->Count)); BindOwner(database, statement, buffers[2]); statement.Dispose();
        counts.Add((JsonNode?)JsonValue.Create(active->Count)); var ids = new JsonArray();
        for (var i = 0; i < active->Count; ++i) ids.Add((JsonNode?)JsonValue.Create(active->Ids[i]));
        Require(active->Error == 0, "destructor callback completed safely");
        return new() { ["first"] = first, ["second"] = second, ["releaseCounts"] = counts, ["destructorIds"] = ids, ["ownersReleased"] = active->Outstanding == 0 };
    }
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void Preupdate(void* user, CapturedDatabase* database, int op, sbyte* schema, sbyte* table, long oldKey, long newKey)
    {
        try
        {
            _ = Context(user); var n = Api.sqlite3_preupdate_count(database); Require(n is >= 0 and <= 8, "preupdate finite columns");
            var item = new JsonObject { ["schema"] = Text(schema), ["table"] = Text(table), ["op"] = op, ["columns"] = n, ["depth"] = Api.sqlite3_preupdate_depth(database) };
            if (op is 9 or 23) { item["oldRowid"] = oldKey; item["old"] = PreValues(database, n, true); }
            if (op is 18 or 23) { item["newRowid"] = newKey; item["new"] = PreValues(database, n, false); }
            preEvents.Add((JsonNode)item); Require(preEvents.Count <= 16, "preupdate finite event bound");
        }
        catch { if (active != null) active->Error = 1; }
    }
    private static JsonArray PreValues(CapturedDatabase* database, int count, bool old)
    {
        var result = new JsonArray();
        for (var i = 0; i < count; ++i)
        {
            CapturedValue* value = null; var code = old ? Api.sqlite3_preupdate_old(database, i, &value) : Api.sqlite3_preupdate_new(database, i, &value);
            Require(code == 0 && value != null, "preupdate borrowed value"); result.Add((JsonNode)ProtectedValue(value));
        }
        return result;
    }
    private static JsonObject Preupdates(JsonElement work)
    {
        var database = new Database(); _ = database.Sql(work.GetProperty("setup").GetString()!);
        var previous = Api.sqlite3_preupdate_hook(database.Pointer, (nint)(delegate* unmanaged[Cdecl]<void*, CapturedDatabase*, int, sbyte*, sbyte*, long, long, void>)&Preupdate, active);
        void* returned = null;
        try { foreach (var sql in work.GetProperty("actions").EnumerateArray()) _ = database.Sql(sql.GetString()!); }
        finally { returned = Api.sqlite3_preupdate_hook(database.Pointer, 0, null); }
        var count = preEvents.Count; _ = database.Sql(work.GetProperty("afterUnregister").GetString()!);
        return new() { ["initialPreviousNull"] = previous == null, ["unregisteredContextMatches"] = returned == active,
            ["events"] = preEvents, ["eventsAfterUnregister"] = preEvents.Count - count, ["result"] = database.Sql(work.GetProperty("read").GetString()!) };
    }
    private static JsonArray ChangeValues(CapturedIterator* item, int count, bool old)
    {
        var result = new JsonArray();
        for (var i = 0; i < count; ++i)
        {
            CapturedValue* value = null; var code = old ? Api.sqlite3changeset_old(item, i, &value) : Api.sqlite3changeset_new(item, i, &value);
            Require(code == 0, "changeset borrowed value"); result.Add((JsonNode)ProtectedValue(value));
        }
        return result;
    }
    private static JsonObject Change(CapturedIterator* item)
    {
        sbyte* table = null; int count = 0, op = 0, indirect = 0;
        Require(Api.sqlite3changeset_op(item, &table, &count, &op, &indirect) == 0 && count is >= 0 and <= 8, "changeset metadata");
        var value = new JsonObject { ["table"] = Text(table), ["columns"] = count, ["op"] = op, ["indirect"] = indirect };
        if (op is 9 or 23) value["old"] = ChangeValues(item, count, true);
        if (op is 18 or 23) value["new"] = ChangeValues(item, count, false);
        return value;
    }
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static int Filter(void* user, sbyte* table)
    {
        try { var state = Context(user); applyEvents.Add((JsonNode)new JsonObject { ["pass"] = state->Pass, ["kind"] = "filter", ["table"] = Text(table) }); return 1; }
        catch { if (active != null) active->Error = 1; return 0; }
    }
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static int Conflict(void* user, int reason, CapturedIterator* item)
    {
        try
        {
            var state = Context(user); applyEvents.Add((JsonNode)new JsonObject { ["pass"] = state->Pass, ["kind"] = "conflict", ["reason"] = reason, ["change"] = Change(item) });
            Require(applyEvents.Count <= 16, "finite apply callback events"); return 0; // SQLITE_CHANGESET_OMIT
        }
        catch { if (active != null) active->Error = 1; return 2; } // SQLITE_CHANGESET_ABORT
    }
    private static (JsonObject Observation, Database Source) Sessions(JsonElement work)
    {
        Require(work.GetProperty("applyPasses").GetInt32() == 2 && work.GetProperty("conflictPolicy").GetString() == "OMIT", "finite apply policy");
        var source = new Database(); var target = new Database();
        foreach (var database in new[] { source, target }) foreach (var sql in work.GetProperty("setup").EnumerateArray()) _ = database.Sql(sql.GetString()!);
        using var schema = new Bytes("main"); using var table = new Bytes(work.GetProperty("table").GetString()!);
        CapturedSession* created = null; source.Check(Api.sqlite3session_create(source.Pointer, (sbyte*)schema.Pointer, &created), "session_create"); session = created;
        source.Check(Api.sqlite3session_attach(session, (sbyte*)table.Pointer), "session_attach");
        foreach (var sql in work.GetProperty("actions").EnumerateArray()) _ = source.Sql(sql.GetString()!);
        int size = 0; void* captured = null; source.Check(Api.sqlite3session_changeset(session, &size, &captured), "session_changeset"); changeset = captured;
        Require(changeset != null && size is > 0 and <= 4096, "finite changeset extent"); var bytes = Convert.ToHexStringLower(new ReadOnlySpan<byte>(changeset, size));
        CapturedIterator* started = null; source.Check(Api.sqlite3changeset_start(&started, size, changeset), "changeset_start"); iterator = started;
        var changes = new JsonArray();
        while (true)
        {
            var code = Api.sqlite3changeset_next(iterator); if (code == 101) break;
            Require(code == 100, "changeset ROW/DONE"); changes.Add((JsonNode)Change(iterator)); Require(changes.Count <= 8, "finite changeset row bound");
        }
        var completedIterator = iterator; iterator = null;
        source.Check(Api.sqlite3changeset_finalize(completedIterator), "changeset_finalize");
        var applied = new JsonArray();
        for (var pass = 1; pass <= 2; ++pass)
        {
            active->Pass = pass;
            target.Check(Api.sqlite3changeset_apply(target.Pointer, size, changeset,
                (nint)(delegate* unmanaged[Cdecl]<void*, sbyte*, int>)&Filter,
                (nint)(delegate* unmanaged[Cdecl]<void*, int, CapturedIterator*, int>)&Conflict, active), "changeset_apply");
            applied.Add((JsonNode)target.Sql(work.GetProperty("read").GetString()!));
        }
        var observation = new JsonObject { ["changesetBytes"] = size, ["changesetHex"] = bytes, ["changes"] = changes,
            ["applyEvents"] = applyEvents, ["source"] = source.Sql(work.GetProperty("read").GetString()!), ["applied"] = applied };
        Api.sqlite3session_delete(session); session = null; Api.sqlite3_free(changeset); changeset = null;
        return (observation, source);
    }
    private static JsonObject Backup(Database source, JsonElement work, string readSql)
    {
        var destination = new Database(); using var schema = new Bytes("main");
        backup = Api.sqlite3_backup_init(destination.Pointer, (sbyte*)schema.Pointer, source.Pointer, (sbyte*)schema.Pointer);
        Require(backup != null, "backup_init handle"); var steps = new JsonArray(); var done = false;
        var cap = work.GetProperty("maximumSteps").GetInt32(); Require(cap is > 0 and <= 64, "finite backup cap");
        for (var i = 0; i < cap; ++i)
        {
            var code = Api.sqlite3_backup_step(backup, work.GetProperty("pagesPerStep").GetInt32());
            steps.Add((JsonNode)new JsonObject { ["code"] = code, ["remaining"] = Api.sqlite3_backup_remaining(backup), ["pageCount"] = Api.sqlite3_backup_pagecount(backup) });
            if (code == 101) { done = true; break; } destination.Check(code, "backup_step");
        }
        Require(done, "backup finished within finite steps"); var completedBackup = backup; backup = null;
        destination.Check(Api.sqlite3_backup_finish(completedBackup), "backup_finish");
        return new() { ["steps"] = steps, ["result"] = destination.Sql(readSql), ["integrity"] = destination.Sql(work.GetProperty("integritySql").GetString()!) };
    }
    private static int Main(string[] args)
    {
        Require(args.Length == 4, "Checks WORKLOAD NEW_REPORT NATIVE_REPORT API_SURFACE_REPORT");
        Require(!File.Exists(args[1]), "fresh report path"); var workloadReceipt = Record(args[0]);
        Require(workloadReceipt["sha256"]!.GetValue<string>() == WorkloadSha, "pinned finite workload");
        using var document = JsonDocument.Parse(File.ReadAllText(args[0])); var work = document.RootElement;
        var oracle = JsonNode.Parse(File.ReadAllText(args[2]))!; var apiReport = JsonNode.Parse(File.ReadAllText(args[3]))!;
        Require(oracle["status"]!.GetValue<string>() == "passed-native-api-callbacks" && oracle["role"]!.GetValue<string>() == "separate-native-test-oracle-only"
            && oracle["workload"]!["sha256"]!.GetValue<string>() == WorkloadSha && oracle["resourcesReleased"]!.GetValue<bool>(), "exact separate native evidence");
        Require(apiReport["status"]!.GetValue<string>() == "validated-published-api-callback-surface", "validated actual API surface");
        var report = new JsonObject { ["status"] = "incomplete-generated-api-callbacks", ["role"] = "generated-csharp-application-only",
            ["nativeApplicationFallbackPermitted"] = false, ["workload"] = workloadReceipt, ["nativeReference"] = Record(args[2]), ["apiSurface"] = Record(args[3]),
            ["limits"] = JsonNode.Parse(work.GetProperty("limits").GetRawText()) };
        active = (State*)NativeMemory.AllocZeroed((nuint)sizeof(State)); if (active == null) throw new OutOfMemoryException(); active->Magic = 0x417;
        var result = 1;
        try
        {
            Require(Api.sqlite3_libversion_number() == 3054000 && work.GetProperty("workloadVersion").GetInt32() == 1, "pinned SQLite/workload version");
            var observations = new JsonObject(); report["observations"] = observations;
            observations["ownership"] = Ownership(work.GetProperty("ownership")); observations["preupdate"] = Preupdates(work.GetProperty("preupdate"));
            var sessionResult = Sessions(work.GetProperty("session")); observations["session"] = sessionResult.Observation;
            observations["backup"] = Backup(sessionResult.Source, work.GetProperty("backup"), work.GetProperty("session").GetProperty("read").GetString()!);
            Require(active->Error == 0 && JsonNode.DeepEquals(observations, oracle["observations"]), "exact native callback/API observations");
            report["nativeComparison"] = "exact-observations-equal"; report["status"] = "passed-generated-api-callbacks"; result = 0;
        }
        catch (Exception error) { report["error"] = error.ToString(); }
        finally
        {
            try
            {
                if (backup != null) { var owned = backup; backup = null; Require(Api.sqlite3_backup_finish(owned) == 0, "cleanup backup"); }
                if (iterator != null) { var owned = iterator; iterator = null; Require(Api.sqlite3changeset_finalize(owned) == 0, "cleanup iterator"); }
                if (session != null) { Api.sqlite3session_delete(session); session = null; }
                if (changeset != null) { Api.sqlite3_free(changeset); changeset = null; }
                foreach (var statement in statements.ToArray()) statement.Dispose();
                foreach (var database in databases.AsEnumerable().Reverse()) database.Dispose();
                Require(active->Outstanding == 0 && active->Error == 0 && statements.Count == 0, "all native owners/callbacks/handles released");
                report["resourcesReleased"] = true; report["callbackErrors"] = new JsonArray();
                var libraries = new JsonArray();
                foreach (var path in File.ReadLines("/proc/self/maps").Select(line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                    .Where(parts => parts.Length >= 6 && parts[^1].StartsWith('/') && parts[^1].Contains(".so", StringComparison.Ordinal))
                    .Select(parts => parts[^1]).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))
                {
                    Require(!Path.GetFileName(path).Contains("sqlite", StringComparison.OrdinalIgnoreCase), "no native SQLite application dependency");
                    libraries.Add((JsonNode)Record(path));
                }
                report["loadedLibraries"] = libraries;
            }
            catch (Exception error) { report["cleanupError"] = error.ToString(); report["resourcesReleased"] = false; result = 1; }
            if (result != 0) report["status"] = "incomplete-generated-api-callbacks";
            NativeMemory.Free(active); active = null;
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(args[1]))!); File.WriteAllText(args[1], report.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n");
        }
        Console.WriteLine(new JsonObject { ["status"] = report["status"]!.GetValue<string>(), ["report"] = Record(args[1]) }.ToJsonString()); return result;
    }
}
