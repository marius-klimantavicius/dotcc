using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

internal static unsafe class GeneratedSqliteStatefulChecks
{
    private const string WorkloadSha256 = "8f9cdc8262ac0c8b29f73fd6a375fd693abf0311dfc4335493b8e5c02ef75a8a";
    private const string InputSha256 = "e26d8937c94f5ccc9e3fca7915f53c4a31a1bb63432419a847d99394ab06bb1c";
    private const string NativeBuildSha256 = "c5d7e700950971126a1f6aa735fc205850d9563bca482b815715ed1353feb435";
    private const string NativeLibrarySha256 = "39d00f19c6778ab90a51bdfdb8e6f13db6d733482112e8587c0e30eccb284c6a";
    private static readonly JsonSerializerOptions ReportOptions = new() { WriteIndented = true };
    private static readonly JsonSerializerOptions CanonicalOptions = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    private sealed unsafe class NativeBytes : IDisposable
    {
        public byte* Pointer { get; private set; }
        public int Length { get; }
        private NativeBytes(ReadOnlySpan<byte> bytes)
        {
            Length = bytes.Length;
            Pointer = (byte*)NativeMemory.Alloc(checked((nuint)bytes.Length + 1));
            if (Pointer == null) throw new OutOfMemoryException();
            bytes.CopyTo(new Span<byte>(Pointer, bytes.Length));
            Pointer[bytes.Length] = 0;
        }
        public static NativeBytes Utf8(string text) => new(Encoding.UTF8.GetBytes(text));
        public void Dispose() { var pointer = Pointer; Pointer = null; NativeMemory.Free(pointer); }
    }

    private static string Text(sbyte* pointer) => pointer == null
        ? throw new InvalidOperationException("Expected borrowed UTF-8 storage.")
        : Marshal.PtrToStringUTF8((nint)pointer)!;

    private static string Digest(string path)
    {
        using var input = File.OpenRead(path);
        return Convert.ToHexStringLower(SHA256.HashData(input));
    }

    private static JsonObject Receipt(string path) => new()
    {
        ["path"] = Path.GetFullPath(path), ["sha256"] = Digest(path), ["bytes"] = new FileInfo(path).Length
    };

    private static bool HasSidecars(string path) => new[] { "-wal", "-shm", "-journal" }.Any(suffix => File.Exists(path + suffix));
    private static string Required(JsonNode node, string key) => node[key]?.GetValue<string>()
        ?? throw new InvalidOperationException("Missing string field: " + key);

    private static JsonNode CanonicalNode(JsonNode node)
    {
        if (node is JsonObject source)
        {
            var result = new JsonObject();
            foreach (var pair in source.OrderBy(pair => pair.Key, StringComparer.Ordinal))
                result[pair.Key] = pair.Value is null ? null : CanonicalNode(pair.Value);
            return result;
        }
        if (node is JsonArray array)
        {
            var result = new JsonArray();
            foreach (var item in array)
                result.Add((JsonNode)(item is null ? throw new InvalidOperationException("Unexpected null observation node.") : CanonicalNode(item)));
            return result;
        }
        return node.DeepClone();
    }

    private static JsonObject ReadValue(void* statement, int index, int maximumBytes)
    {
        var kind = Api.sqlite3_column_type(statement, index);
        if (kind == 1) return new() { ["type"] = "integer", ["decimal"] = Api.sqlite3_column_int64(statement, index).ToString(CultureInfo.InvariantCulture) };
        if (kind == 2) return new() { ["type"] = "real", ["binary64Hex"] = unchecked((ulong)BitConverter.DoubleToInt64Bits(Api.sqlite3_column_double(statement, index))).ToString("x16", CultureInfo.InvariantCulture) };
        if (kind is 3 or 4)
        {
            void* address = kind == 3 ? Api.sqlite3_column_text(statement, index) : Api.sqlite3_column_blob(statement, index);
            var count = Api.sqlite3_column_bytes(statement, index);
            if (count < 0 || count > maximumBytes || count != 0 && address == null)
                throw new InvalidOperationException("Invalid or oversized borrowed column extent.");
            return new() { ["type"] = kind == 3 ? "text-utf8" : "blob", ["bytes"] = count,
                ["hex"] = count == 0 ? "" : Convert.ToHexStringLower(new ReadOnlySpan<byte>(address, count)) };
        }
        if (kind == 5) return new() { ["type"] = "null" };
        throw new InvalidOperationException("Unexpected SQLite storage class: " + kind);
    }

    private static void ExecuteSql(CapturedDatabase* database, JsonElement action, JsonElement workload, JsonObject observed)
    {
        var sql = action.GetProperty("sql").GetString()!;
        if (sql.Contains('\0')) throw new InvalidOperationException("A SQL action contains an embedded NUL.");
        var limits = workload.GetProperty("limits");
        var columns = new JsonArray();
        var rows = new JsonArray();
        observed["sql"] = sql;
        observed["columns"] = columns;
        observed["rows"] = rows;
        observed["prepareCode"] = null;
        observed["stepCode"] = null;
        observed["finalizeCode"] = null;
        using var encoded = NativeBytes.Utf8(sql);
        void* statement = null;
        sbyte* tail = null;
        try
        {
            var code = Api.sqlite3_prepare_v2(database, (sbyte*)encoded.Pointer, checked(encoded.Length + 1), &statement, &tail);
            observed["prepareCode"] = code;
            if (code != 0) throw new InvalidOperationException($"prepare {action.GetProperty("id").GetString()}: {code}: {Text(Api.sqlite3_errmsg(database))}");
            if (statement == null || tail == null || (byte*)tail < encoded.Pointer || (byte*)tail > encoded.Pointer + encoded.Length)
                throw new InvalidOperationException("The action did not prepare one statement with a bounded tail.");
            for (var remaining = (byte*)tail; remaining < encoded.Pointer + encoded.Length; ++remaining)
                if (*remaining is not (9 or 10 or 11 or 12 or 13 or 32))
                    throw new InvalidOperationException("Every action must contain exactly one SQL statement.");
            var count = Api.sqlite3_column_count(statement);
            if (count < 0 || count > limits.GetProperty("maximumColumns").GetInt32())
                throw new InvalidOperationException("Frozen result-column budget exceeded.");
            for (var index = 0; index < count; ++index)
                columns.Add((JsonNode)JsonValue.Create(Text(Api.sqlite3_column_name(statement, index)))!);
            while (true)
            {
                code = Api.sqlite3_step(statement);
                if (code != 100) { observed["stepCode"] = code; break; }
                var row = new JsonArray();
                for (var index = 0; index < count; ++index)
                    row.Add((JsonNode)ReadValue(statement, index, limits.GetProperty("maximumColumnBytes").GetInt32()));
                rows.Add((JsonNode)row);
                if (rows.Count > limits.GetProperty("maximumRowsPerStatement").GetInt32())
                    throw new InvalidOperationException("Frozen result-row budget exceeded.");
            }
        }
        finally
        {
            // Expected BUSY remains step5/finalize5. The failed statement is
            // destroyed before the next connection/transaction action runs.
            if (statement != null) observed["finalizeCode"] = Api.sqlite3_finalize(statement);
        }
        var expected = action.GetProperty("expected");
        var expectedRows = expected.TryGetProperty("rowsRef", out var reference)
            ? workload.GetProperty("rowSets").GetProperty(reference.GetString()!) : expected.GetProperty("rows");
        if (!JsonNode.DeepEquals(columns, JsonNode.Parse(expected.GetProperty("columns").GetRawText())) ||
            !JsonNode.DeepEquals(rows, JsonNode.Parse(expectedRows.GetRawText())) ||
            observed["stepCode"]!.GetValue<int>() != expected.GetProperty("stepCode").GetInt32() ||
            observed["finalizeCode"]!.GetValue<int>() != expected.GetProperty("finalizeCode").GetInt32())
            throw new InvalidOperationException("Observed SQL protocol differs at " + action.GetProperty("id").GetString());
    }

    private static JsonArray LoadedLibraries()
    {
        var paths = File.ReadLines("/proc/self/maps").Select(line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Where(parts => parts.Length >= 6 && parts[^1].StartsWith('/') && parts[^1].Contains(".so", StringComparison.Ordinal))
            .Select(parts => parts[^1]).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        if (paths.Any(path => Path.GetFileName(path).Contains("sqlite", StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Generated execution loaded an unauthorized native SQLite application library.");
        var libraries = new JsonArray();
        foreach (var path in paths) libraries.Add((JsonNode)Receipt(path));
        return libraries;
    }

    private static Dictionary<string, string> Arguments(string[] args)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        for (var index = 0; index < args.Length; index += 2)
        {
            if (index + 1 == args.Length || args[index] is not ("--mode" or "--workload" or "--artifacts" or "--native-report" or "--input-report") ||
                !result.TryAdd(args[index], args[index + 1]))
                throw new ArgumentException("Checks --mode MODE --workload JSON --artifacts NEW --native-report NATIVE [--input-report CLOSED_PREVIOUS]");
        }
        foreach (var key in new[] { "--mode", "--workload", "--artifacts", "--native-report" })
            if (!result.ContainsKey(key)) throw new ArgumentException("Missing argument: " + key);
        if (result["--mode"] is not ("stateful" or "interchange-append" or "interchange-verify") ||
            (result["--mode"] == "stateful") == result.ContainsKey("--input-report"))
            throw new ArgumentException("Use --input-report only, and always, for an interchange mode.");
        return result;
    }

    private static int Main(string[] args)
    {
        var arguments = Arguments(args);
        var artifactDirectory = Path.GetFullPath(arguments["--artifacts"]);
        if (Directory.Exists(artifactDirectory) || File.Exists(artifactDirectory)) throw new ArgumentException("Choose a fresh artifact directory.");
        Directory.CreateDirectory(artifactDirectory);
        var reportPath = Path.Combine(artifactDirectory, "stateful-report.json");
        var workloadPath = Path.GetFullPath(arguments["--workload"]);
        var nativePath = Path.GetFullPath(arguments["--native-report"]);
        var observations = new JsonArray();
        var databases = new Dictionary<string, nint>(StringComparer.Ordinal);
        var frozenFiles = new Dictionary<string, string>(StringComparer.Ordinal);
        string? incomingFile = null, incomingHash = null;
        var report = new JsonObject
        {
            ["status"] = "incomplete", ["role"] = "generated-csharp-application-only", ["mode"] = arguments["--mode"],
            ["generatedExecution"] = true, ["nativeApplicationFallbackPermitted"] = false,
            ["connectionsClosed"] = false, ["databaseState"] = null, ["observations"] = observations,
            ["lockScope"] = "two connections in one process; explicit ordered SQL barriers, no scheduler timing",
            ["interchangeClosureRequirement"] = "coordinator waits for producing process exit before consumption"
        };
        try
        {
            if (Digest(workloadPath) != WorkloadSha256) throw new InvalidOperationException("The frozen finite workload bytes changed.");
            using var document = JsonDocument.Parse(File.ReadAllText(workloadPath));
            var workload = document.RootElement;
            if (workload.GetProperty("workloadVersion").GetInt32() != 1 || workload.GetProperty("sourceObjectSha256").GetString() != InputSha256)
                throw new InvalidOperationException("Workload version/input identity changed.");
            var mode = workload.GetProperty("modes").GetProperty(arguments["--mode"]);
            var actions = mode.GetProperty("actions");
            if (actions.GetArrayLength() > workload.GetProperty("limits").GetProperty("maximumActionsPerMode").GetInt32())
                throw new InvalidOperationException("Frozen action budget exceeded.");
            report["workload"] = Receipt(workloadPath);
            report["remainingT4Areas"] = JsonNode.Parse(workload.GetProperty("remainingT4Areas").GetRawText());
            frozenFiles.Add(workloadPath, WorkloadSha256);
            var native = JsonNode.Parse(File.ReadAllText(nativePath))!;
            if (Required(native, "status") != "passed-native-stateful" || Required(native, "role") != "separate-native-test-oracle-only" ||
                Required(native, "mode") != arguments["--mode"] || native["sqliteVersionNumber"]!.GetValue<int>() != 3054000 ||
                native["connectionsClosed"]!.GetValue<bool>() is not true || native["pinnedFilesUnchanged"]!.GetValue<bool>() is not true ||
                native["workload"]!["sha256"]!.GetValue<string>() != WorkloadSha256 ||
                native["buildReceipt"]!["sha256"]!.GetValue<string>() != NativeBuildSha256 ||
                native["library"]!["sha256"]!.GetValue<string>() != NativeLibrarySha256)
                throw new InvalidOperationException("Exact same-mode pinned native observations are required.");
            report["nativeObservationReceipt"] = Receipt(nativePath);
            frozenFiles[nativePath] = Digest(nativePath);
            var databaseFile = Path.Combine(artifactDirectory, workload.GetProperty("databaseFileName").GetString()!);
            if (Path.GetFileName(databaseFile) != "stateful.db" || Path.GetDirectoryName(databaseFile) != artifactDirectory)
                throw new InvalidOperationException("The database must remain in the fresh artifact directory.");
            if (arguments.TryGetValue("--input-report", out var inputReport))
            {
                inputReport = Path.GetFullPath(inputReport);
                var incoming = JsonNode.Parse(File.ReadAllText(inputReport))!;
                if (Required(incoming, "status") is not ("passed-native-stateful" or "passed-generated-stateful") ||
                    incoming["connectionsClosed"]!.GetValue<bool>() is not true ||
                    Required(incoming, "databaseState") != mode.GetProperty("inputState").GetString() ||
                    incoming["workload"]!["sha256"]!.GetValue<string>() != WorkloadSha256)
                    throw new InvalidOperationException("The incoming report lacks the expected closed workload state.");
                incomingFile = Path.GetFullPath(Required(incoming["database"]!, "path"));
                incomingHash = Required(incoming["database"]!, "sha256");
                var actual = Receipt(incomingFile);
                if (Required(actual, "sha256") != incomingHash || actual["bytes"]!.GetValue<long>() != incoming["database"]!["bytes"]!.GetValue<long>() || HasSidecars(incomingFile))
                    throw new InvalidOperationException("The incoming closed file changed or retains untransferred sidecars.");
                File.Copy(incomingFile, databaseFile, false);
                if (Digest(incomingFile) != incomingHash || Digest(databaseFile) != incomingHash)
                    throw new InvalidOperationException("The incoming closed file changed while copying.");
                report["incomingTransfer"] = new JsonObject
                {
                    ["report"] = Receipt(inputReport), ["database"] = actual, ["copiedDatabase"] = Receipt(databaseFile),
                    ["closedStateEvidence"] = "prior report and absent sidecars; producing process exit required externally"
                };
                frozenFiles[inputReport] = Digest(inputReport);
            }
            _ = LoadedLibraries();
            report["sqliteVersion"] = Text(Api.sqlite3_libversion());
            report["sqliteVersionNumber"] = Api.sqlite3_libversion_number();
            if (report["sqliteVersionNumber"]!.GetValue<int>() != workload.GetProperty("sqliteVersionNumber").GetInt32())
                throw new InvalidOperationException("Expected the pinned generated SQLite3.54.0 application.");
            foreach (var action in actions.EnumerateArray())
            {
                var connection = action.GetProperty("connection").GetString()!;
                var kind = action.GetProperty("action").GetString()!;
                var observed = new JsonObject { ["id"] = action.GetProperty("id").GetString(), ["action"] = kind, ["connection"] = connection };
                observations.Add((JsonNode)observed);
                if (kind == "open")
                {
                    if (databases.ContainsKey(connection)) throw new InvalidOperationException("Connection ID opened twice without closing.");
                    CapturedDatabase* database = null;
                    using var filename = NativeBytes.Utf8(databaseFile);
                    var code = Api.sqlite3_open_v2((sbyte*)filename.Pointer, &database, action.GetProperty("flags").GetInt32(), null);
                    observed["code"] = code;
                    if (database != null) databases.Add(connection, (nint)database);
                    if (code != action.GetProperty("expectedCode").GetInt32() || database == null)
                        throw new InvalidOperationException("Unexpected open result at " + action.GetProperty("id").GetString());
                    continue;
                }
                if (!databases.TryGetValue(connection, out var address)) throw new InvalidOperationException("Connection is not open: " + connection);
                if (kind == "close")
                {
                    var code = Api.sqlite3_close((CapturedDatabase*)address);
                    observed["code"] = code;
                    if (code == 0) databases.Remove(connection);
                    if (code != action.GetProperty("expectedCode").GetInt32())
                        throw new InvalidOperationException("Unexpected close result at " + action.GetProperty("id").GetString());
                }
                else if (kind == "sql") ExecuteSql((CapturedDatabase*)address, action, workload, observed);
                else throw new InvalidOperationException("Unsupported frozen action: " + kind);
            }
            if (databases.Count != 0) throw new InvalidOperationException("The action list left open connections.");
            if (!File.Exists(databaseFile) || HasSidecars(databaseFile)) throw new InvalidOperationException("The final database is missing or retains untransferred sidecars.");
            if (!JsonNode.DeepEquals(observations, native["observations"]))
                throw new InvalidOperationException("Generated action observations differ from the exact same-mode native oracle.");
            report["database"] = Receipt(databaseFile);
            report["databaseState"] = mode.GetProperty("outputState").GetString();
            report["loadedLibraries"] = LoadedLibraries();
            var canonical = CanonicalNode(observations).ToJsonString(CanonicalOptions);
            report["observationsSha256"] = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
            if (Required(report, "observationsSha256") != Required(native, "observationsSha256"))
                throw new InvalidOperationException("Canonical observation hash differs from the native protocol.");
            report["nativeComparison"] = "exact-typed-action-array-equal";
            report["status"] = "passed-generated-stateful";
        }
        catch (Exception problem) { report["status"] = "incomplete"; report["error"] = problem.ToString(); }
        finally
        {
            var cleanup = new JsonArray();
            foreach (var pair in databases.ToArray())
            {
                try
                {
                    var code = Api.sqlite3_close((CapturedDatabase*)pair.Value);
                    cleanup.Add((JsonNode)new JsonObject { ["connection"] = pair.Key, ["closeCode"] = code });
                    if (code == 0) databases.Remove(pair.Key);
                }
                catch (Exception problem) { cleanup.Add((JsonNode)new JsonObject { ["connection"] = pair.Key, ["error"] = problem.ToString() }); }
            }
            if (cleanup.Count != 0) report["cleanup"] = cleanup;
            report["connectionsClosed"] = databases.Count == 0;
            if (incomingFile != null && (!File.Exists(incomingFile) || Digest(incomingFile) != incomingHash || HasSidecars(incomingFile)))
            {
                report["status"] = "incomplete";
                report["inputFreezeFailure"] = "The incoming database or its absent-sidecar condition changed.";
            }
            var changed = new JsonArray();
            foreach (var pair in frozenFiles)
                if (!File.Exists(pair.Key) || Digest(pair.Key) != pair.Value) changed.Add((JsonNode)JsonValue.Create(pair.Key)!);
            report["pinnedFilesUnchanged"] = changed.Count == 0;
            if (changed.Count != 0) { report["status"] = "incomplete"; report["pinnedFileFreezeFailure"] = changed; }
            using var output = new StreamWriter(new FileStream(reportPath, FileMode.CreateNew, FileAccess.Write));
            output.Write(report.ToJsonString(ReportOptions) + "\n");
        }
        Console.WriteLine(new JsonObject { ["status"] = Required(report, "status"), ["mode"] = arguments["--mode"],
            ["actions"] = observations.Count, ["report"] = reportPath }.ToJsonString());
        return Required(report, "status") == "passed-generated-stateful" ? 0 : 1;
    }
}
