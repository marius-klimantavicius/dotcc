using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

internal static unsafe class ExtendedSqliteChecks
{
    private const string WorkloadSha256 = "1ad62ff2001a6630e52f0fb7a9fa730209574e9c03ec6b3f219e56db5bdb72e7";
    private const string BuildSha256 = "c5d7e700950971126a1f6aa735fc205850d9563bca482b815715ed1353feb435";
    private const string LibrarySha256 = "39d00f19c6778ab90a51bdfdb8e6f13db6d733482112e8587c0e30eccb284c6a";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static CapturedDatabase* database;
    private static readonly List<Statement> Statements = [];
    private static readonly List<Buffer> Owners = [];
    private static nint firstContext, secondContext, thirdContext;
    private static readonly int[] CallbackEvents = new int[3];
    private static int callbackCount;
    private static bool callbackFailed;

    private sealed class Buffer : IDisposable
    {
        public byte* Pointer { get; private set; }
        public int Length { get; }
        private Buffer(int length)
        {
            Length = length;
            Pointer = (byte*)NativeMemory.Alloc(checked((nuint)length));
            if (Pointer == null) throw new OutOfMemoryException();
        }
        public static Buffer Utf8(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            var result = new Buffer(checked(bytes.Length + 1));
            bytes.CopyTo(new Span<byte>(result.Pointer, bytes.Length));
            result.Pointer[bytes.Length] = 0;
            return result;
        }
        public static Buffer Integers(JsonElement array, bool wide)
        {
            var result = new Buffer(checked(array.GetArrayLength() * (wide ? 8 : 4)));
            var index = 0;
            foreach (var value in array.EnumerateArray())
            {
                if (wide) ((long*)result.Pointer)[index] = value.GetInt64();
                else ((int*)result.Pointer)[index] = value.GetInt32();
                ++index;
            }
            Owners.Add(result);
            return result;
        }
        public void Dispose() { var pointer = Pointer; Pointer = null; NativeMemory.Free(pointer); }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void Destroyed(void* context)
    {
        try
        {
            var id = (nint)context == firstContext ? 1 : (nint)context == secondContext ? 2 : (nint)context == thirdContext ? 3 : 0;
            if (id == 0 || *(int*)context != id || callbackCount >= CallbackEvents.Length) { callbackFailed = true; return; }
            CallbackEvents[callbackCount++] = id;
        }
        catch { callbackFailed = true; }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
    private static string Text(sbyte* pointer) => pointer == null
        ? throw new InvalidOperationException("Expected borrowed UTF-8 storage.") : Marshal.PtrToStringUTF8((nint)pointer)!;
    private static string Digest(string path)
    {
        using var input = File.OpenRead(path);
        return Convert.ToHexStringLower(SHA256.HashData(input));
    }
    private static JsonObject Receipt(string path) => new()
    {
        ["path"] = Path.GetFullPath(path), ["bytes"] = new FileInfo(path).Length, ["sha256"] = Digest(path)
    };
    private static JsonObject TextValue(string text)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        return new() { ["type"] = "text-utf8", ["bytes"] = bytes.Length, ["hex"] = Convert.ToHexStringLower(bytes) };
    }
    private static JsonObject Errors() => new()
    {
        ["errorCode"] = Api.sqlite3_errcode(database), ["extendedErrorCode"] = Api.sqlite3_extended_errcode(database),
        ["message"] = Text(Api.sqlite3_errmsg(database))
    };
    private static void Merge(JsonObject target, JsonObject source)
    {
        foreach (var pair in source) target[pair.Key] = pair.Value?.DeepClone();
    }

    private sealed class Statement
    {
        public void* Handle { get; private set; }
        public Statement(string sql)
        {
            Require(!sql.Contains('\0'), "Embedded NUL SQL.");
            using var encoded = Buffer.Utf8(sql);
            void* handle = null;
            sbyte* tail = null;
            var code = Api.sqlite3_prepare_v2(database, (sbyte*)encoded.Pointer, encoded.Length, &handle, &tail);
            Handle = handle;
            Statements.Add(this);
            Require(code == 0 && handle != null && tail != null && (byte*)tail >= encoded.Pointer && (byte*)tail <= encoded.Pointer + encoded.Length - 1, "Preparation/tail failed: " + sql);
            for (var remaining = (byte*)tail; remaining < encoded.Pointer + encoded.Length - 1; ++remaining)
                Require(*remaining is 9 or 10 or 11 or 12 or 13 or 32, "Each action must prepare exactly one statement.");
        }
        public int Finish()
        {
            var pointer = Handle;
            Handle = null;
            return pointer == null ? 0 : Api.sqlite3_finalize(pointer);
        }
        public JsonObject Read()
        {
            var count = Api.sqlite3_column_count(Handle);
            Require(count is >= 0 and <= 8, "Column budget exceeded.");
            var columns = new JsonArray();
            for (var index = 0; index < count; ++index) columns.Add((JsonNode)JsonValue.Create(Text(Api.sqlite3_column_name(Handle, index)))!);
            var rows = new JsonArray();
            var result = new JsonObject { ["columns"] = columns, ["rows"] = rows, ["stepCode"] = null };
            while (true)
            {
                var code = Api.sqlite3_step(Handle);
                if (code != 100)
                {
                    result["stepCode"] = code;
                    if (code != 101) result["error"] = Errors(); // Before finalize or any non-preserving API.
                    return result;
                }
                var row = new JsonArray();
                for (var index = 0; index < count; ++index) row.Add((JsonNode)ReadValue(index));
                rows.Add((JsonNode)row);
                Require(rows.Count <= 16, "Row budget exceeded.");
            }
        }
        private JsonObject ReadValue(int index)
        {
            var kind = Api.sqlite3_column_type(Handle, index);
            if (kind == 1) return new() { ["type"] = "integer", ["decimal"] = Api.sqlite3_column_int64(Handle, index).ToString(CultureInfo.InvariantCulture) };
            if (kind == 2) return new() { ["type"] = "real", ["binary64Hex"] = unchecked((ulong)BitConverter.DoubleToInt64Bits(Api.sqlite3_column_double(Handle, index))).ToString("x16", CultureInfo.InvariantCulture) };
            if (kind is 3 or 4)
            {
                void* address = kind == 3 ? Api.sqlite3_column_text(Handle, index) : Api.sqlite3_column_blob(Handle, index);
                var count = Api.sqlite3_column_bytes(Handle, index);
                Require(count is >= 0 and <= 4096 && (count == 0 || address != null), "Invalid borrowed column extent.");
                return new() { ["type"] = kind == 3 ? "text-utf8" : "blob", ["bytes"] = count,
                    ["hex"] = count == 0 ? "" : Convert.ToHexStringLower(new ReadOnlySpan<byte>(address, count)) };
            }
            Require(kind == 5, "Unknown storage class.");
            return new() { ["type"] = "null" };
        }
    }

    private static JsonObject Query(string sql)
    {
        var statement = new Statement(sql);
        var result = statement.Read();
        result["sql"] = sql;
        result["finalizeCode"] = statement.Finish();
        return result;
    }
    private static JsonArray IntegerRows(JsonElement values)
    {
        var result = new JsonArray();
        foreach (var value in values.EnumerateArray())
        {
            var row = new JsonArray();
            row.Add((JsonNode)new JsonObject { ["type"] = "integer", ["decimal"] = value.GetInt64().ToString(CultureInfo.InvariantCulture) });
            result.Add((JsonNode)row);
        }
        return result;
    }
    private static void RowProtocol(JsonObject observed, JsonNode expected) => Require(
        JsonNode.DeepEquals(observed["rows"], expected) && observed["stepCode"]!.GetValue<int>() == 101 && observed["finalizeCode"]!.GetValue<int>() == 0,
        "Typed result/step/finalize protocol changed.");

    private static void Carray(JsonElement work, JsonArray cases)
    {
        var observations = new JsonArray();
        var result = new JsonObject { ["name"] = "carray-values-ownership", ["observations"] = observations };
        cases.Add((JsonNode)result);
        foreach (var wide in new[] { false, true })
        {
            var field = wide ? "transientInt64" : "staticInt32";
            var values = work.GetProperty(field);
            var data = Buffer.Integers(values, wide);
            var statement = new Statement(work.GetProperty("carraySql").GetString()!);
            var observed = new JsonObject { ["label"] = wide ? "transient" : "static",
                ["bindCode"] = Api.sqlite3_carray_bind(statement.Handle, 1, data.Pointer, values.GetArrayLength(), wide ? 1 : 0, wide ? -1 : 0) };
            observations.Add((JsonNode)observed);
            Require(observed["bindCode"]!.GetValue<int>() == 0, "Carray bind failed.");
            if (wide)
            {
                var index = 0;
                foreach (var value in work.GetProperty("transientMutation").EnumerateArray()) ((long*)data.Pointer)[index++] = value.GetInt64();
            }
            Merge(observed, statement.Read()); observed["finalizeCode"] = statement.Finish();
            RowProtocol(observed, IntegerRows(values));
        }
        using var contextDocument = JsonDocument.Parse("[1,2,3]");
        var contexts = Buffer.Integers(contextDocument.RootElement, false);
        firstContext = (nint)contexts.Pointer; secondContext = (nint)(contexts.Pointer + 4); thirdContext = (nint)(contexts.Pointer + 8);
        var callback = (nint)(delegate* unmanaged[Cdecl]<void*, void>)&Destroyed;
        var owned = new Statement(work.GetProperty("carraySql").GetString()!);
        for (var sequence = 1; sequence <= 2; ++sequence)
        {
            var values = work.GetProperty(sequence == 1 ? "ownedFirst" : "ownedSecond");
            var data = Buffer.Integers(values, false);
            var context = sequence == 1 ? firstContext : secondContext;
            var observed = new JsonObject { ["label"] = "owned-" + sequence.ToString(CultureInfo.InvariantCulture),
                ["bindCode"] = Api.sqlite3_carray_bind_v2(owned.Handle, 1, data.Pointer, values.GetArrayLength(), 0, callback, (void*)context) };
            observations.Add((JsonNode)observed);
            Require(observed["bindCode"]!.GetValue<int>() == 0 && (nint)data.Pointer != firstContext && (nint)data.Pointer != secondContext && (nint)data.Pointer != thirdContext, "Owned carray bind/context failed.");
            Merge(observed, owned.Read()); observed["callbacksAfterRead"] = callbackCount;
            Require(JsonNode.DeepEquals(observed["rows"], IntegerRows(values)) && observed["stepCode"]!.GetValue<int>() == 101 && callbackCount == sequence - 1, "Owned values/lifetime changed.");
            if (sequence == 1)
            {
                observed["resetCode"] = Api.sqlite3_reset(owned.Handle); observed["callbacksAfterReset"] = callbackCount;
                observed["clearCode"] = Api.sqlite3_clear_bindings(owned.Handle); observed["callbacksAfterClear"] = callbackCount;
                Require(observed["resetCode"]!.GetValue<int>() == 0 && observed["callbacksAfterReset"]!.GetValue<int>() == 0 && observed["clearCode"]!.GetValue<int>() == 0 && callbackCount == 1, "reset/clear ownership changed.");
            }
            else
            {
                observed["finalizeCode"] = owned.Finish(); observed["callbacksAfterFinalize"] = callbackCount;
                Require(observed["finalizeCode"]!.GetValue<int>() == 0 && callbackCount == 2, "Finalization ownership changed.");
            }
        }
        var failure = new Statement(work.GetProperty("carraySql").GetString()!);
        using var badDocument = JsonDocument.Parse("[99]");
        var badData = Buffer.Integers(badDocument.RootElement, false);
        var bad = new JsonObject { ["label"] = "owned-bind-failure", ["bindCode"] = Api.sqlite3_carray_bind_v2(failure.Handle, 2, badData.Pointer, 1, 0, callback, (void*)thirdContext) };
        observations.Add((JsonNode)bad);
        bad["callbacksAfterBind"] = callbackCount; bad["finalizeCode"] = failure.Finish();
        Require(bad["bindCode"]!.GetValue<int>() == 25 && callbackCount == 3 && bad["finalizeCode"]!.GetValue<int>() == 0, "Failed-bind ownership changed.");
        var events = new JsonArray();
        for (var index = 0; index < callbackCount; ++index) events.Add((JsonNode)JsonValue.Create(CallbackEvents[index])!);
        result["callbackEvents"] = events; result["callbackFailed"] = callbackFailed; result["callbackContextDistinctFromData"] = true;
        Require(!callbackFailed && JsonNode.DeepEquals(events, JsonNode.Parse(work.GetProperty("callbackExpectedEvents").GetRawText())), "Caught callback protocol failed.");
    }

    private static void Normalize(JsonElement work, JsonArray cases)
    {
        var observations = new JsonArray();
        cases.Add((JsonNode)new JsonObject { ["name"] = "normalized-statement-execution", ["observations"] = observations });
        foreach (var sqlValue in work.GetProperty("normalizedSql").EnumerateArray())
        {
            var sql = sqlValue.GetString()!;
            var statement = new Statement(sql);
            // Copy borrowed normalization bytes before later SQLite calls/finalize.
            var observed = new JsonObject { ["sql"] = sql, ["normalized"] = TextValue(Text(Api.sqlite3_normalized_sql(statement.Handle))),
                ["bindCode"] = Api.sqlite3_bind_int64(statement.Handle, 1, 9) };
            observations.Add((JsonNode)observed);
            Merge(observed, statement.Read()); observed["finalizeCode"] = statement.Finish();
            Require(observed["bindCode"]!.GetValue<int>() == 0, "Normalized statement bind failed.");
            RowProtocol(observed, JsonNode.Parse(work.GetProperty("normalizedExpectedRows")[observations.Count - 1].GetRawText())!);
        }
        Require(JsonNode.DeepEquals(observations[0]!["normalized"], observations[1]!["normalized"]), "Literal-only normalization did not agree.");
    }

    private static void Metadata(JsonElement work, JsonArray cases)
    {
        var columns = new JsonArray();
        var schema = Query(work.GetProperty("schemaSql").GetString()!);
        var result = new JsonObject { ["name"] = "column-metadata", ["schema"] = schema, ["columns"] = columns };
        cases.Add((JsonNode)result);
        Require(schema["stepCode"]!.GetValue<int>() == 101 && schema["finalizeCode"]!.GetValue<int>() == 0, "Schema creation failed.");
        using var main = Buffer.Utf8("main"); using var table = Buffer.Utf8("meta");
        foreach (var expected in work.GetProperty("metadata").EnumerateArray())
        {
            using var name = Buffer.Utf8(expected.GetProperty("column").GetString()!);
            sbyte* datatype = null; sbyte* collation = null;
            int notNull = 0, primary = 0, increment = 0;
            var code = Api.sqlite3_table_column_metadata(database, (sbyte*)main.Pointer, (sbyte*)table.Pointer, (sbyte*)name.Pointer, &datatype, &collation, &notNull, &primary, &increment);
            var observed = new JsonObject { ["column"] = expected.GetProperty("column").GetString(), ["code"] = code };
            columns.Add((JsonNode)observed);
            Require(code == 0, "Expected metadata column missing.");
            // No SQLite API intervenes before both returned strings are copied.
            observed["declaredType"] = Text(datatype); observed["collation"] = Text(collation);
            observed["notNull"] = notNull; observed["primaryKey"] = primary; observed["autoIncrement"] = increment;
            Require(JsonNode.DeepEquals(observed, JsonNode.Parse(expected.GetRawText())), "Metadata differs from finite schema expectation.");
        }
        using var missing = Buffer.Utf8(work.GetProperty("missingColumn").GetString()!);
        result["missingCode"] = Api.sqlite3_table_column_metadata(database, (sbyte*)main.Pointer, (sbyte*)table.Pointer, (sbyte*)missing.Pointer, null, null, null, null, null);
        Require(result["missingCode"]!.GetValue<int>() == work.GetProperty("expectedMissingMetadataCode").GetInt32(), "Missing metadata error changed.");
    }

    private static void Constraints(JsonElement work, JsonArray cases)
    {
        var modes = new JsonArray(); var observations = new JsonArray();
        var result = new JsonObject { ["name"] = "constraint-errors-and-recovery", ["modes"] = modes, ["observations"] = observations };
        cases.Add((JsonNode)result);
        void Mode(int enabled)
        {
            var code = Api.sqlite3_extended_result_codes(database, enabled);
            modes.Add((JsonNode)new JsonObject { ["enabled"] = enabled, ["code"] = code });
            Require(code == 0, "Error mode failed.");
        }
        Mode(0);
        foreach (var action in work.GetProperty("errorSql").EnumerateArray())
        {
            var label = action.GetProperty("label").GetString()!;
            if (label is "notnull-extended" or "transaction-begin") Mode(label == "notnull-extended" ? 1 : 0);
            var observed = Query(action.GetProperty("sql").GetString()!); observed["label"] = label;
            observations.Add((JsonNode)observed);
            Require(observed["stepCode"]!.GetValue<int>() == action.GetProperty("stepCode").GetInt32() && observed["finalizeCode"]!.GetValue<int>() == action.GetProperty("finalizeCode").GetInt32(), "Constraint step/finalize code changed.");
            if (action.TryGetProperty("errorCode", out var code))
                Require(observed["error"]!["errorCode"]!.GetValue<int>() == code.GetInt32() && observed["error"]!["extendedErrorCode"]!.GetValue<int>() == action.GetProperty("extendedErrorCode").GetInt32(), "Immediate extended error changed.");
        }
        var recovery = Query(work.GetProperty("recoverySql").GetString()!);
        result["recovery"] = recovery;
        RowProtocol(recovery, JsonNode.Parse(work.GetProperty("recoveryExpectedRows").GetRawText())!);
    }

    private static JsonArray Libraries()
    {
        var paths = File.ReadLines("/proc/self/maps").Select(line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Where(parts => parts.Length >= 6 && parts[^1].StartsWith('/') && parts[^1].Contains(".so", StringComparison.Ordinal))
            .Select(parts => parts[^1]).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        Require(!paths.Any(path => Path.GetFileName(path).Contains("sqlite", StringComparison.OrdinalIgnoreCase)), "Generated execution loaded a native SQLite application library.");
        var result = new JsonArray(); foreach (var path in paths) result.Add((JsonNode)Receipt(path));
        return result;
    }

    private static int Main(string[] args)
    {
        if (args.Length != 3) throw new ArgumentException("Checks WORKLOAD_JSON NEW_ARTIFACT_DIRECTORY SEPARATE_NATIVE_REPORT");
        var folder = Path.GetFullPath(args[1]);
        Require(!Directory.Exists(folder) && !File.Exists(folder), "Choose a fresh artifact directory."); Directory.CreateDirectory(folder);
        var cases = new JsonArray();
        var report = new JsonObject { ["status"] = "incomplete", ["role"] = "generated-csharp-application-only", ["generatedExecution"] = true,
            ["nativeApplicationFallbackPermitted"] = false, ["connectionsClosed"] = false, ["cases"] = cases };
        var frozen = new Dictionary<string, string>(StringComparer.Ordinal);
        try
        {
            Require(Digest(args[0]) == WorkloadSha256, "The frozen finite workload changed.");
            using var document = JsonDocument.Parse(File.ReadAllText(args[0])); var work = document.RootElement;
            Require(work.GetProperty("workloadVersion").GetInt32() == 1, "Unsupported workload version.");
            report["workload"] = Receipt(args[0]); report["remainingT4Areas"] = JsonNode.Parse(work.GetProperty("remainingT4Areas").GetRawText());
            frozen.Add(args[0], WorkloadSha256);
            var native = JsonNode.Parse(File.ReadAllText(args[2]))!;
            Require(native["status"]!.GetValue<string>() == "passed-native-extended-apis" && native["role"]!.GetValue<string>() == "separate-native-test-oracle-only" &&
                native["connectionsClosed"]!.GetValue<bool>() && native["pinnedFilesUnchanged"]!.GetValue<bool>() &&
                native["workload"]!["sha256"]!.GetValue<string>() == WorkloadSha256 && native["buildReceipt"]!["sha256"]!.GetValue<string>() == BuildSha256 &&
                native["library"]!["sha256"]!.GetValue<string>() == LibrarySha256 && native["sqliteVersionNumber"]!.GetValue<int>() == 3054000,
                "Use exact completed pinned native extended observations.");
            report["nativeObservationReceipt"] = Receipt(args[2]); frozen[args[2]] = Digest(args[2]);
            _ = Libraries();
            report["sqliteVersion"] = Text(Api.sqlite3_libversion()); report["sqliteVersionNumber"] = Api.sqlite3_libversion_number();
            Require(report["sqliteVersionNumber"]!.GetValue<int>() == work.GetProperty("sqliteVersionNumber").GetInt32(), "Expected pinned generated SQLite3.54.0.");
            using var name = Buffer.Utf8(":memory:");
            CapturedDatabase* opened = null;
            report["openCode"] = Api.sqlite3_open_v2((sbyte*)name.Pointer, &opened, 6, null); database = opened;
            Require(report["openCode"]!.GetValue<int>() == 0 && database != null, "Open failed.");
            Carray(work, cases); Normalize(work, cases); Metadata(work, cases); Constraints(work, cases);
            report["closeCode"] = Api.sqlite3_close(database);
            Require(report["closeCode"]!.GetValue<int>() == 0, "Close failed."); database = null;
            Require(JsonNode.DeepEquals(cases, native["cases"]) && report["openCode"]!.GetValue<int>() == native["openCode"]!.GetValue<int>() && report["closeCode"]!.GetValue<int>() == native["closeCode"]!.GetValue<int>(), "Exact native/generated typed cases or connection protocol differ.");
            report["loadedLibraries"] = Libraries(); report["nativeComparison"] = "exact-typed-cases-equal"; report["status"] = "passed-generated-extended-apis";
        }
        catch (Exception problem) { report["status"] = "incomplete"; report["error"] = problem.ToString(); }
        finally
        {
            var cleanup = new JsonArray();
            foreach (var statement in Statements.Where(value => value.Handle != null))
                cleanup.Add((JsonNode)new JsonObject { ["finalizeCode"] = statement.Finish() });
            if (database != null)
            {
                var code = Api.sqlite3_close(database); cleanup.Add((JsonNode)new JsonObject { ["closeCode"] = code });
                if (code == 0) database = null;
            }
            foreach (var owner in Owners) owner.Dispose();
            if (cleanup.Count != 0) report["cleanup"] = cleanup;
            report["connectionsClosed"] = database == null;
            report["pinnedFilesUnchanged"] = frozen.All(pair => File.Exists(pair.Key) && Digest(pair.Key) == pair.Value);
            if (!report["pinnedFilesUnchanged"]!.GetValue<bool>()) report["status"] = "incomplete";
            using var stream = new StreamWriter(new FileStream(Path.Combine(folder, "extended-api-report.json"), FileMode.CreateNew, FileAccess.Write));
            stream.Write(report.ToJsonString(JsonOptions) + "\n");
        }
        Console.WriteLine(new JsonObject { ["status"] = report["status"]!.GetValue<string>(), ["cases"] = cases.Count }.ToJsonString());
        return report["status"]!.GetValue<string>() == "passed-generated-extended-apis" ? 0 : 1;
    }
}
