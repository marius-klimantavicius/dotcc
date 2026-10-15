using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using RoslynObj.CSharp.Runtime;

internal static unsafe class FailureSqliteChecks
{
    private const string WorkloadSha256 = "f1cfaffcc508d27de1c7aedca10744b15a04300d06a25911e70666533a83d6bf";
    private const string BuildSha256 = "c5d7e700950971126a1f6aa735fc205850d9563bca482b815715ed1353feb435";
    private const string LibrarySha256 = "39d00f19c6778ab90a51bdfdb8e6f13db6d733482112e8587c0e30eccb284c6a";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static CapturedDatabase* database;
    private static readonly List<Statement> Statements = [];
    private static long? savedLimit;
    private static bool hookInstalled, armed, callbackFailed;
    private static int probe0Calls, activated400;
    private static readonly Dictionary<int, int> CallbackAudit = [];

    private sealed class Buffer : IDisposable
    {
        public byte* Pointer { get; private set; }
        public int Length { get; }
        private Buffer(byte[] bytes)
        {
            Length = checked(bytes.Length + 1);
            Pointer = (byte*)NativeMemory.Alloc((nuint)Length);
            if (Pointer == null) throw new OutOfMemoryException();
            bytes.CopyTo(new Span<byte>(Pointer, bytes.Length)); Pointer[bytes.Length] = 0;
        }
        public static Buffer Utf8(string value) => new(Encoding.UTF8.GetBytes(value));
        public void Dispose() { var pointer = Pointer; Pointer = null; NativeMemory.Free(pointer); }
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
    private static JsonArray ExpectedRows(JsonElement values)
    {
        var result = new JsonArray();
        foreach (var source in values.EnumerateArray())
        {
            var row = new JsonArray();
            foreach (var value in source.EnumerateArray())
                row.Add((JsonNode)new JsonObject { ["type"] = "integer", ["decimal"] = value.GetInt64().ToString(CultureInfo.InvariantCulture) });
            result.Add((JsonNode)row);
        }
        return result;
    }
    private static void RowProtocol(JsonObject observed, JsonNode expected) => Require(
        JsonNode.DeepEquals(observed["rows"], expected) && observed["stepCode"]!.GetValue<int>() == 101 && observed["finalizeCode"]!.GetValue<int>() == 0,
        "Typed result/step/finalize protocol changed.");

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static int Fault(int code)
    {
        try
        {
            CallbackAudit[code] = CallbackAudit.GetValueOrDefault(code) + 1;
            if (code == 0) ++probe0Calls;
            if (code == 400 && armed && activated400 == 0) { armed = false; ++activated400; return 1; }
            return 0;
        }
        catch { callbackFailed = true; return 0; }
    }

    private static int TestControl(nint callback)
    {
        SysVVariadic.RequireAbi();
        SysVVaList probe = default;
        Require(sizeof(SysVVaList) == 24 && (byte*)&probe.GpOffset - (byte*)&probe == 0 &&
            (byte*)&probe.FpOffset - (byte*)&probe == 4 && (byte*)&probe.OverflowArgumentArea - (byte*)&probe == 8 &&
            (byte*)&probe.RegisterSaveArea - (byte*)&probe == 16, "Runtime builtin variadic storage coordinates changed.");
        byte* storage = stackalloc byte[SysVVariadic.RegisterSaveBytes + 8 + 15];
        var registers = SysVVariadic.Align16(storage);
        new Span<byte>(registers, SysVVariadic.RegisterSaveBytes + 8).Clear();
        *(int*)registers = 9; // The fixed int consumes one8-byte SysV GP slot.
        *(nint*)(registers + 8) = callback;
        var arguments = new SysVVaList { GpOffset = 8, FpOffset = 48,
            OverflowArgumentArea = registers + SysVVariadic.RegisterSaveBytes, RegisterSaveArea = registers };
        return Api.sqlite3_test_control(9, &arguments);
    }

    private static JsonObject Success(string sql)
    {
        var result = Query(sql);
        Require(result["stepCode"]!.GetValue<int>() == 101 && result["finalizeCode"]!.GetValue<int>() == 0, "Expected successful SQL action: " + sql);
        return result;
    }
    private static int Open(string path, int flags)
    {
        Require(database == null, "An earlier database is still open.");
        using var name = Buffer.Utf8(path);
        CapturedDatabase* opened = null;
        var code = Api.sqlite3_open_v2((sbyte*)name.Pointer, &opened, flags, null); database = opened;
        Require(code == 0 && database != null, "Open failed."); return code;
    }
    private static int Close()
    {
        var code = Api.sqlite3_close(database);
        if (code == 0) database = null;
        Require(code == 0, "Close failed."); return code;
    }
    private static void RecoverTransaction(JsonObject result)
    {
        var value = Api.sqlite3_get_autocommit(database);
        Require(value is 0 or 1, "Invalid autocommit state.");
        result["autocommitAfterFinalize"] = value;
        result["rollback"] = value == 0 ? Success("ROLLBACK") : null;
    }

    private static void Allocation(JsonElement work, JsonArray cases, JsonObject report)
    {
        var setup = new JsonArray();
        var result = new JsonObject { ["name"] = "controlled-hard-heap-nomem", ["setup"] = setup, ["openCode"] = Open(":memory:", 6) };
        cases.Add((JsonNode)result);
        foreach (var name in new[] { "schema", "seed", "begin", "tentative" }) setup.Add((JsonNode)Success(work.GetProperty(name).GetString()!));
        var demand = new Statement(work.GetProperty("demand").GetString()!); // Prepare before installing the cap.
        var previous = Api.sqlite3_hard_heap_limit64(-1); savedLimit = previous;
        var used = Api.sqlite3_memory_used(); var cap = checked(used + work.GetProperty("memoryHeadroomBytes").GetInt64());
        var audit = new JsonObject { ["previousHardLimit"] = previous, ["memoryUsedBeforeCap"] = used, ["requestedHardLimit"] = cap };
        report["allocationAudit"] = audit;
        Require(previous >= 0 && used > 0 && cap > used, "Invalid live memory accounting/limit.");
        try
        {
            audit["setReturnedPrevious"] = Api.sqlite3_hard_heap_limit64(cap);
            audit["observedHardLimit"] = Api.sqlite3_hard_heap_limit64(-1);
            Require(audit["observedHardLimit"]!.GetValue<long>() == cap && audit["setReturnedPrevious"]!.GetValue<long>() == previous, "Hard limit was not installed.");
            var failure = demand.Read(); failure["sql"] = work.GetProperty("demand").GetString(); result["failure"] = failure;
        }
        finally
        {
            audit["restoreReturnedPrevious"] = Api.sqlite3_hard_heap_limit64(previous);
            var restored = Api.sqlite3_hard_heap_limit64(-1); audit["restoredHardLimit"] = restored;
            if (restored == previous) savedLimit = null;
            Require(restored == previous, "Prior hard limit was not restored.");
        }
        result["failure"]!["finalizeCode"] = demand.Finish();
        var observed = result["failure"]!;
        Require(observed["stepCode"]!.GetValue<int>() == work.GetProperty("expectedFailure").GetInt32() &&
            observed["error"]!["errorCode"]!.GetValue<int>() == 7 && observed["error"]!["extendedErrorCode"]!.GetValue<int>() == 7 && observed["finalizeCode"]!.GetValue<int>() == 7,
            "Actual step did not activate controlled NOMEM.");
        RecoverTransaction(result);
        var unchanged = Success(work.GetProperty("verify").GetString()!); result["unchanged"] = unchanged;
        RowProtocol(unchanged, ExpectedRows(work.GetProperty("expectedBeforeRecovery")));
        result["recoveryWrite"] = Success(work.GetProperty("recovery").GetString()!);
        var recovered = Success(work.GetProperty("verify").GetString()!); result["recovered"] = recovered;
        RowProtocol(recovered, ExpectedRows(work.GetProperty("expectedAfterRecovery"))); result["closeCode"] = Close();
    }

    private static string Io(JsonElement work, JsonArray cases, string folder)
    {
        var path = Path.Combine(folder, work.GetProperty("fileName").GetString()!);
        Require(Path.GetFileName(path) == "io-failure.db" && Path.GetDirectoryName(path) == folder, "I/O file escaped fresh artifacts.");
        var setup = new JsonArray();
        var result = new JsonObject { ["name"] = "injected-pager-commit-ioerr", ["setup"] = setup, ["openCode"] = Open(path, 6) };
        cases.Add((JsonNode)result);
        foreach (var name in new[] { "journalMode", "schema", "seed", "begin", "tentative" }) setup.Add((JsonNode)Success(work.GetProperty(name).GetString()!));
        Require(JsonNode.DeepEquals(setup[0]!["rows"], JsonNode.Parse("[[{\"type\":\"text-utf8\",\"bytes\":6,\"hex\":\"64656c657465\"}]]")), "Genuine DELETE journal mode missing.");
        var commit = new Statement(work.GetProperty("commit").GetString()!);
        try
        {
            hookInstalled = true;
            result["installCode"] = TestControl((nint)(delegate* unmanaged[Cdecl]<int, int>)&Fault);
            Require(result["installCode"]!.GetValue<int>() == 0 && probe0Calls == 1, "Unarmed installation probe0 failed.");
            armed = true;
            var failure = commit.Read(); failure["sql"] = work.GetProperty("commit").GetString(); result["failure"] = failure;
        }
        finally
        {
            armed = false; result["uninstallCode"] = TestControl(0); hookInstalled = false;
        }
        result["failure"]!["finalizeCode"] = commit.Finish();
        result["callback"] = new JsonObject { ["probe0Calls"] = probe0Calls, ["activated400"] = activated400, ["callbackFailed"] = callbackFailed };
        var observed = result["failure"]!;
        Require(observed["stepCode"]!.GetValue<int>() == work.GetProperty("expectedFailure").GetInt32() &&
            observed["error"]!["errorCode"]!.GetValue<int>() == 10 && observed["error"]!["extendedErrorCode"]!.GetValue<int>() == 10 && observed["finalizeCode"]!.GetValue<int>() == 10 &&
            result["uninstallCode"]!.GetValue<int>() == 0 && activated400 == 1 && !callbackFailed, "Actual injected pager IOERR/uninstall failed.");
        RecoverTransaction(result);
        var unchanged = Success(work.GetProperty("verify").GetString()!); result["unchanged"] = unchanged;
        RowProtocol(unchanged, ExpectedRows(work.GetProperty("expectedBeforeRecovery")));
        var recovery = new JsonArray(); result["recovery"] = recovery;
        foreach (var name in new[] { "recoveryBegin", "recoveryUpdate", "recoveryCommit" }) recovery.Add((JsonNode)Success(work.GetProperty(name).GetString()!));
        var recovered = Success(work.GetProperty("verify").GetString()!); result["recovered"] = recovered;
        RowProtocol(recovered, ExpectedRows(work.GetProperty("expectedAfterRecovery")));
        result["closeCode"] = Close(); result["reopenCode"] = Open(path, 2);
        var persisted = Success(work.GetProperty("verify").GetString()!); result["persisted"] = persisted;
        var integrity = Success("PRAGMA integrity_check"); result["integrity"] = integrity;
        RowProtocol(persisted, ExpectedRows(work.GetProperty("expectedAfterRecovery")));
        Require(JsonNode.DeepEquals(integrity["rows"], JsonNode.Parse("[[{\"type\":\"text-utf8\",\"bytes\":2,\"hex\":\"6f6b\"}]]")), "Close/reopen integrity changed.");
        result["finalCloseCode"] = Close();
        Require(!new[] { "-journal", "-wal", "-shm" }.Any(suffix => File.Exists(path + suffix)), "Sidecars remain after normal close.");
        return path;
    }

    private static JsonArray Libraries()
    {
        var paths = File.ReadLines("/proc/self/maps").Select(line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Where(parts => parts.Length >= 6 && parts[^1].StartsWith('/') && parts[^1].Contains(".so", StringComparison.Ordinal))
            .Select(parts => parts[^1]).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        Require(!paths.Any(path => Path.GetFileName(path).Contains("sqlite", StringComparison.OrdinalIgnoreCase)), "Generated execution loaded a native SQLite application library.");
        var values = new JsonArray(); foreach (var path in paths) values.Add((JsonNode)Receipt(path)); return values;
    }

    private static int Main(string[] args)
    {
        if (args.Length != 3) throw new ArgumentException("Checks WORKLOAD_JSON NEW_ARTIFACT_DIRECTORY SEPARATE_NATIVE_REPORT");
        var folder = Path.GetFullPath(args[1]);
        Require(!Directory.Exists(folder) && !File.Exists(folder), "Choose a fresh artifact directory."); Directory.CreateDirectory(folder);
        var cases = new JsonArray();
        var report = new JsonObject { ["status"] = "incomplete", ["role"] = "generated-csharp-application-only", ["generatedExecution"] = true,
            ["nativeApplicationFallbackPermitted"] = false, ["connectionsClosed"] = false, ["cases"] = cases,
            ["faultScope"] = "documented sqlite3FaultSim(400) pager commit injection, not an actual OS/device failure" };
        var frozen = new Dictionary<string, string>(StringComparer.Ordinal);
        try
        {
            Require(Digest(args[0]) == WorkloadSha256, "The frozen finite workload changed.");
            using var document = JsonDocument.Parse(File.ReadAllText(args[0])); var work = document.RootElement;
            Require(work.GetProperty("workloadVersion").GetInt32() == 1, "Unsupported workload version.");
            report["workload"] = Receipt(args[0]); report["remainingT4Areas"] = JsonNode.Parse(work.GetProperty("remainingT4Areas").GetRawText()); frozen.Add(args[0], WorkloadSha256);
            var native = JsonNode.Parse(File.ReadAllText(args[2]))!;
            Require(native["status"]!.GetValue<string>() == "passed-native-controlled-failures" && native["role"]!.GetValue<string>() == "separate-native-test-oracle-only" &&
                native["connectionsClosed"]!.GetValue<bool>() && native["faultHookUninstalled"]!.GetValue<bool>() && native["pinnedFilesUnchanged"]!.GetValue<bool>() &&
                native["workload"]!["sha256"]!.GetValue<string>() == WorkloadSha256 && native["buildReceipt"]!["sha256"]!.GetValue<string>() == BuildSha256 &&
                native["library"]!["sha256"]!.GetValue<string>() == LibrarySha256 && native["sqliteVersionNumber"]!.GetValue<int>() == 3054000,
                "Use exact completed pinned native failure observations.");
            report["nativeObservationReceipt"] = Receipt(args[2]); frozen[args[2]] = Digest(args[2]);
            _ = Libraries(); report["sqliteVersion"] = Text(Api.sqlite3_libversion()); report["sqliteVersionNumber"] = Api.sqlite3_libversion_number();
            Require(report["sqliteVersionNumber"]!.GetValue<int>() == work.GetProperty("sqliteVersionNumber").GetInt32(), "Expected pinned generated SQLite3.54.0.");
            Allocation(work.GetProperty("allocation"), cases, report);
            var path = Io(work.GetProperty("io"), cases, folder); report["database"] = Receipt(path);
            var callbackAudit = new JsonObject(); foreach (var pair in CallbackAudit.OrderBy(pair => pair.Key)) callbackAudit[pair.Key.ToString(CultureInfo.InvariantCulture)] = pair.Value;
            report["faultCallbackAudit"] = callbackAudit;
            Require(JsonNode.DeepEquals(cases, native["cases"]), "Exact native/generated failure/rollback/recovery cases differ; memory accounting is separate audit data.");
            report["loadedLibraries"] = Libraries(); report["nativeComparison"] = "exact-typed-cases-equal-audit-accounting-separate"; report["status"] = "passed-generated-controlled-failures";
        }
        catch (Exception problem) { report["status"] = "incomplete"; report["error"] = problem.ToString(); }
        finally
        {
            var cleanup = new JsonArray(); armed = false;
            if (hookInstalled) { cleanup.Add((JsonNode)new JsonObject { ["uninstallCode"] = TestControl(0) }); hookInstalled = false; }
            if (savedLimit is { } prior) cleanup.Add((JsonNode)new JsonObject { ["restoreHardLimitReturned"] = Api.sqlite3_hard_heap_limit64(prior), ["restoredHardLimit"] = Api.sqlite3_hard_heap_limit64(-1) });
            foreach (var statement in Statements.Where(value => value.Handle != null)) cleanup.Add((JsonNode)new JsonObject { ["finalizeCode"] = statement.Finish() });
            if (database != null)
            {
                var code = Api.sqlite3_close(database); cleanup.Add((JsonNode)new JsonObject { ["closeCode"] = code }); if (code == 0) database = null;
            }
            if (cleanup.Count != 0) report["cleanup"] = cleanup;
            report["connectionsClosed"] = database == null; report["faultHookUninstalled"] = !hookInstalled;
            report["pinnedFilesUnchanged"] = frozen.All(pair => File.Exists(pair.Key) && Digest(pair.Key) == pair.Value);
            if (!report["pinnedFilesUnchanged"]!.GetValue<bool>()) report["status"] = "incomplete";
            using var stream = new StreamWriter(new FileStream(Path.Combine(folder, "failures-report.json"), FileMode.CreateNew, FileAccess.Write)); stream.Write(report.ToJsonString(JsonOptions) + "\n");
        }
        Console.WriteLine(new JsonObject { ["status"] = report["status"]!.GetValue<string>(), ["cases"] = cases.Count }.ToJsonString());
        return report["status"]!.GetValue<string>() == "passed-generated-controlled-failures" ? 0 : 1;
    }
}

