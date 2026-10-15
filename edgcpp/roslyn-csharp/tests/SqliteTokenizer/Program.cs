using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

internal static unsafe class TokenizerChecks
{
    private const string WorkloadSha = "bbdc900bcdd58bf38723bffd1c5fe2bf44153cd5d92135504ee9da676b6b2579";
    private const string OracleSha = "39d00f19c6778ab90a51bdfdb8e6f13db6d733482112e8587c0e30eccb284c6a";
    private struct Event { public int Kind, Owner, Flags, Start, End, Code, Length; public fixed byte Text[64]; }
    private struct State { public int Magic, Error, Created, Deleted, Destroyed, Outstanding, EventCount; public Event* Events; public nint* Owners; }
    private struct Owner { public State* Context; public int Id, Magic; }
    private static State* state;
    private static CapturedTokenizer* methods;
    private static CapturedDatabase* database;
    private static readonly List<Statement> statements = [];
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
            var bytes = Encoding.UTF8.GetBytes(value); Length = bytes.Length; Pointer = (byte*)NativeMemory.Alloc(checked((nuint)Length + 1));
            if (Pointer == null) throw new OutOfMemoryException(); bytes.CopyTo(new Span<byte>(Pointer, Length)); Pointer[Length] = 0;
        }
        public void Dispose() => NativeMemory.Free(Pointer);
    }
    private static void Check(int code, string operation) { if (code != 0) throw new InvalidOperationException(operation + ": " + code + ": " + Text(Api.sqlite3_errmsg(database))); }
    private sealed class Statement : IDisposable
    {
        public void* Pointer;
        public Statement(string sql)
        {
            using var bytes = new Bytes(sql); void* pointer = null; sbyte* tail = null;
            var code = Api.sqlite3_prepare_v2(database, (sbyte*)bytes.Pointer, bytes.Length + 1, &pointer, &tail);
            if (code != 0) { if (pointer != null) Api.sqlite3_finalize(pointer); Check(code, "prepare"); }
            try { Require(pointer != null && (byte*)tail == bytes.Pointer + bytes.Length, "exactly one SQL statement"); }
            catch { if (pointer != null) Api.sqlite3_finalize(pointer); throw; }
            Pointer = pointer; statements.Add(this);
        }
        public JsonObject Read()
        {
            var count = Api.sqlite3_column_count(Pointer); Require(count is >= 0 and <= 8, "finite column bound");
            var columns = new JsonArray(); var rows = new JsonArray();
            for (var i = 0; i < count; ++i) columns.Add((JsonNode?)JsonValue.Create(Text(Api.sqlite3_column_name(Pointer, i))));
            while (true)
            {
                var code = Api.sqlite3_step(Pointer); if (code == 101) break;
                if (code != 100) { Check(code, "step"); throw new InvalidOperationException("expected ROW or DONE"); }
                var values = new JsonArray(); for (var i = 0; i < count; ++i) values.Add((JsonNode)Value(Pointer, i));
                rows.Add((JsonNode)values); Require(rows.Count <= 16, "finite row bound");
            }
            Require(state->Error == 0, "contained tokenizer callback error"); return new() { ["columns"] = columns, ["rows"] = rows };
        }
        public void Dispose()
        {
            if (Pointer == null) return; var owned = Pointer; Pointer = null;
            var code = Api.sqlite3_finalize(owned); statements.Remove(this); Check(code, "finalize");
        }
    }
    private static JsonObject Value(void* statement, int index)
    {
        var kind = Api.sqlite3_column_type(statement, index);
        if (kind == 1) return new() { ["type"] = "integer", ["decimal"] = Api.sqlite3_column_int64(statement, index).ToString(CultureInfo.InvariantCulture) };
        if (kind == 2) return new() { ["type"] = "real", ["binary64Hex"] = unchecked((ulong)BitConverter.DoubleToInt64Bits(Api.sqlite3_column_double(statement, index))).ToString("x16", CultureInfo.InvariantCulture) };
        if (kind is 3 or 4)
        {
            void* pointer = kind == 3 ? Api.sqlite3_column_text(statement, index) : Api.sqlite3_column_blob(statement, index); var count = Api.sqlite3_column_bytes(statement, index);
            Require(count is >= 0 and <= 1024 && (count == 0 || pointer != null), "borrowed value bound");
            return new() { ["type"] = kind == 3 ? "text-utf8" : "blob", ["bytes"] = count, ["hex"] = count == 0 ? "" : Convert.ToHexStringLower(new ReadOnlySpan<byte>(pointer, count)) };
        }
        Require(kind == 5, "known SQLite value class"); return new() { ["type"] = "null" };
    }
    private static void AddEvent(int kind, int owner, int flags, int start, int end, int code, byte* text, int length)
    {
        Require(state != null && state->Magic == 0x513 && state->EventCount is >= 0 and < 256 && length is >= 0 and <= 64
            && (length == 0 || text != null), "fixed event storage bound");
        var value = state->Events + state->EventCount++;
        value->Kind = kind; value->Owner = owner; value->Flags = flags; value->Start = start; value->End = end; value->Code = code; value->Length = length;
        if (length != 0) new ReadOnlySpan<byte>(text, length).CopyTo(new Span<byte>(value->Text, length));
    }
    private static int OwnerSlot(void* pointer)
    {
        Require(state != null && state->Magic == 0x513 && pointer != null, "tokenizer context/owner");
        for (var index = 0; index < 8; ++index) if (state->Owners[index] == (nint)pointer) return index;
        throw new InvalidOperationException("unowned tokenizer pointer");
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static int Create(void* context, sbyte** arguments, int count, void** output)
    {
        Owner* owner = null; var published = false; var slot = -1;
        try
        {
            Require(context == state && state != null && state->Magic == 0x513 && count == 0 && output != null && state->Created < 8, "create context/arguments");
            slot = state->Created; Require(state->Owners[slot] == 0, "empty owner slot"); owner = (Owner*)NativeMemory.Alloc((nuint)sizeof(Owner));
            if (owner == null) throw new OutOfMemoryException(); owner->Context = state; owner->Id = ++state->Created; owner->Magic = 0x514;
            state->Owners[slot] = (nint)owner; ++state->Outstanding;
            AddEvent(1, owner->Id, 0, 0, 0, count, null, 0); *output = owner; published = true; return 0;
        }
        catch { if (state != null) state->Error = 1; return 1; }
        finally
        {
            if (!published && owner != null) { if (slot >= 0 && state->Owners[slot] == (nint)owner) { state->Owners[slot] = 0; --state->Outstanding; } NativeMemory.Free(owner); }
        }
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void Delete(void* pointer)
    {
        Owner* owner = null; var slot = -1;
        try
        {
            slot = OwnerSlot(pointer); owner = (Owner*)pointer; Require(owner->Context == state && owner->Magic == 0x514, "delete owner");
            AddEvent(5, owner->Id, 0, 0, 0, 0, null, 0); ++state->Deleted;
        }
        catch { if (state != null) state->Error = 1; }
        finally { if (owner != null && slot >= 0) { state->Owners[slot] = 0; --state->Outstanding; owner->Magic = 0; NativeMemory.Free(owner); } }
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void Destroy(void* context)
    {
        try { Require(context == state && state != null && state->Magic == 0x513, "registration context"); ++state->Destroyed; AddEvent(6, 0, 0, 0, 0, 0, null, 0); }
        catch { if (state != null) state->Error = 1; }
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static int Tokenize(void* pointer, void* engineContext, int flags, sbyte* input, int length, nint tokenCallback)
    {
        try
        {
            _ = OwnerSlot(pointer); var owner = (Owner*)pointer;
            Require(owner->Magic == 0x514 && owner->Context == state && length is >= 0 and <= 64 && (length == 0 || input != null) && tokenCallback != 0, "tokenize ownership/extent");
            for (var i = 0; i < length; ++i) Require(input[i] == 32 || input[i] is >= 97 and <= 122, "finite lowercase ASCII-space input");
            AddEvent(2, owner->Id, flags, 0, 0, 0, (byte*)input, length);
            var emit = (delegate* unmanaged[Cdecl]<void*, int, sbyte*, int, int, int, int>)tokenCallback; var start = 0;
            while (start < length)
            {
                if (input[start] == 32) { ++start; continue; } var end = start + 1; while (end < length && input[end] != 32) ++end;
                var code = emit(engineContext, 0, input + start, end - start, start, end);
                AddEvent(3, owner->Id, 0, start, end, code, (byte*)(input + start), end - start);
                if (code != 0) { AddEvent(4, owner->Id, 0, 0, 0, code, null, 0); return code; } start = end;
            }
            AddEvent(4, owner->Id, 0, 0, 0, 0, null, 0); return 0;
        }
        catch { if (state != null) state->Error = 1; return 1; }
    }
    private static JsonArray Events()
    {
        Require(state->EventCount is >= 0 and <= 256, "event count bound"); var result = new JsonArray();
        for (var i = 0; i < state->EventCount; ++i)
        {
            var value = state->Events + i; var hex = Convert.ToHexStringLower(new ReadOnlySpan<byte>(value->Text, value->Length));
            JsonObject entry = value->Kind switch
            {
                1 => new() { ["event"] = "create", ["owner"] = value->Owner, ["argumentCount"] = value->Code },
                2 => new() { ["event"] = "tokenize", ["owner"] = value->Owner, ["flags"] = value->Flags, ["bytes"] = value->Length, ["hex"] = hex },
                3 => new() { ["event"] = "token", ["owner"] = value->Owner, ["flags"] = value->Flags, ["start"] = value->Start, ["end"] = value->End, ["hex"] = hex, ["code"] = value->Code },
                4 => new() { ["event"] = "tokenizeResult", ["owner"] = value->Owner, ["code"] = value->Code },
                5 => new() { ["event"] = "delete", ["owner"] = value->Owner },
                6 => new() { ["event"] = "registrationDestroy" },
                _ => throw new InvalidOperationException("unknown callback event")
            };
            result.Add((JsonNode)entry);
        }
        return result;
    }
    private static void CloseDatabase()
    {
        if (database == null) return; Check(Api.sqlite3_close(database), "close"); database = null;
    }
    private static int Main(string[] args)
    {
        Require(args.Length == 4, "Checks WORKLOAD NEW_REPORT NATIVE_REPORT API_SURFACE_REPORT"); Require(!File.Exists(args[1]), "fresh report path");
        var source = Record(args[0]); Require(source["sha256"]!.GetValue<string>() == WorkloadSha, "pinned tokenizer workload");
        using var document = JsonDocument.Parse(File.ReadAllText(args[0])); var work = document.RootElement;
        var oracle = JsonNode.Parse(File.ReadAllText(args[2]))!; var surface = JsonNode.Parse(File.ReadAllText(args[3]))!;
        Require(oracle["status"]!.GetValue<string>() == "passed-native-tokenizer" && oracle["role"]!.GetValue<string>() == "separate-native-test-oracle-only"
            && oracle["resourcesReleased"]!.GetValue<bool>() && oracle["workload"]!["sha256"]!.GetValue<string>() == WorkloadSha && oracle["library"]!["sha256"]!.GetValue<string>() == OracleSha, "exact separate native evidence");
        Require(surface["status"]!.GetValue<string>() == "validated-published-tokenizer-surface", "validated actual API surface");
        var report = new JsonObject { ["status"] = "incomplete-generated-tokenizer", ["role"] = "generated-csharp-application-only", ["nativeApplicationFallbackPermitted"] = false,
            ["workload"] = source, ["nativeReference"] = Record(args[2]), ["apiSurface"] = Record(args[3]), ["limits"] = JsonNode.Parse(work.GetProperty("limits").GetRawText()) }; var result = 1;
        state = (State*)NativeMemory.AllocZeroed((nuint)sizeof(State)); if (state == null) throw new OutOfMemoryException(); state->Magic = 0x513;
        state->Events = (Event*)NativeMemory.AllocZeroed((nuint)(256 * sizeof(Event))); state->Owners = (nint*)NativeMemory.AllocZeroed((nuint)(8 * sizeof(nint)));
        methods = (CapturedTokenizer*)NativeMemory.AllocZeroed((nuint)sizeof(CapturedTokenizer));
        if (state->Events == null || state->Owners == null || methods == null) throw new OutOfMemoryException();
        try
        {
            Require(Api.sqlite3_libversion_number() == 3054000 && work.GetProperty("workloadVersion").GetInt32() == 1
                && work.GetProperty("maximumInputBytes").GetInt32() == 64 && work.GetProperty("maximumEvents").GetInt32() == 256, "pinned SQLite/workload limits");
            CapturedFts5Api layout = default;
            Require(sizeof(CapturedFts5Api) == 48 && sizeof(CapturedTokenizer) == 24 && (byte*)(&layout.xCreateTokenizer) - (byte*)(&layout) == 8
                && (byte*)(&methods->xDelete) - (byte*)methods == 8 && (byte*)(&methods->xTokenize) - (byte*)methods == 16, "actual generated physical field layout");
            using var filename = new Bytes(":memory:"); CapturedDatabase* opened = null; var code = Api.sqlite3_open_v2((sbyte*)filename.Pointer, &opened, 6, null); database = opened; Check(code, "open");
            CapturedFts5Api* engine = null; JsonObject handoff;
            using (var tag = new Bytes("fts5_api_ptr"))
            using (var statement = new Statement("SELECT fts5(?1)"))
            { Check(Api.sqlite3_bind_pointer(statement.Pointer, 1, &engine, (sbyte*)tag.Pointer, 0), "bind_pointer"); handoff = statement.Read(); }
            Require(engine != null && engine->iVersion >= 2 && engine->xCreateTokenizer != 0, "engine-supplied legacy registration API"); var version = engine->iVersion;
            methods->xCreate = (nint)(delegate* unmanaged[Cdecl]<void*, sbyte**, int, void**, int>)&Create;
            methods->xDelete = (nint)(delegate* unmanaged[Cdecl]<void*, void>)&Delete;
            methods->xTokenize = (nint)(delegate* unmanaged[Cdecl]<void*, void*, int, sbyte*, int, nint, int>)&Tokenize;
            using (var name = new Bytes(work.GetProperty("tokenizerName").GetString()!))
                Check(((delegate* unmanaged[Cdecl]<CapturedFts5Api*, sbyte*, void*, CapturedTokenizer*, nint, int>)engine->xCreateTokenizer)(engine,
                    (sbyte*)name.Pointer, state, methods, (nint)(delegate* unmanaged[Cdecl]<void*, void>)&Destroy), "xCreateTokenizer");
            var observations = new JsonArray();
            foreach (var sql in work.GetProperty("statements").EnumerateArray()) { using var statement = new Statement(sql.GetString()!); observations.Add((JsonNode)statement.Read()); }
            CloseDatabase(); Require(state->Outstanding == 0 && state->Created > 0 && state->Created == state->Deleted && state->Destroyed == 1 && state->Error == 0, "all tokenizer owners/context released after close");
            var observed = new JsonObject { ["apiVersion"] = version, ["handoff"] = handoff, ["statements"] = observations,
                ["events"] = Events(), ["creates"] = state->Created, ["deletes"] = state->Deleted, ["registrationDestroys"] = state->Destroyed };
            report["observations"] = observed; Require(JsonNode.DeepEquals(observed, oracle["observations"]), "exact native tokenizer observations/events");
            report["nativeComparison"] = "exact-observations-equal"; report["status"] = "passed-generated-tokenizer"; result = 0;
        }
        catch (Exception error) { report["error"] = error.ToString(); }
        finally
        {
            var cleanup = new JsonArray();
            foreach (var statement in statements.ToArray()) try { statement.Dispose(); } catch (Exception error) { cleanup.Add((JsonNode?)JsonValue.Create(error.ToString())); }
            try { CloseDatabase(); } catch (Exception error) { cleanup.Add((JsonNode?)JsonValue.Create(error.ToString())); }
            var callbackErrors = new JsonArray(); if (state->Error != 0) callbackErrors.Add((JsonNode?)JsonValue.Create("contained tokenizer callback invariant/exception failure"));
            report["cleanupErrors"] = cleanup; report["callbackErrors"] = callbackErrors;
            report["resourcesReleased"] = database == null && statements.Count == 0 && state->Outstanding == 0 && cleanup.Count == 0;
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
            if (!report["resourcesReleased"]!.GetValue<bool>() || callbackErrors.Count != 0) result = 1;
            if (result != 0) report["status"] = "incomplete-generated-tokenizer";
            if (database == null && state->Outstanding == 0) { NativeMemory.Free(methods); NativeMemory.Free(state->Owners); NativeMemory.Free(state->Events); NativeMemory.Free(state); state = null; methods = null; }
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(args[1]))!); File.WriteAllText(args[1], report.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n");
        }
        Console.WriteLine(new JsonObject { ["status"] = report["status"]!.GetValue<string>(), ["report"] = Record(args[1]) }.ToJsonString()); return result;
    }
}
