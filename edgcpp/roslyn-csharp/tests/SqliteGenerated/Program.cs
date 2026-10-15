using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

internal static unsafe class GeneratedSqliteChecks
{
    private const string WorkloadSha256 = "6b79741d6435b95fd2a663fbd889b8f33aee124e634196f5fadeb14eea6b3229";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private sealed record Value(string Type, string? Decimal = null, string? Binary64Hex = null, int? Bytes = null, string? Hex = null);
    private sealed record StatementObservation(string Sql, string[] Columns, List<Value[]> Rows);
    private sealed record CaseObservation(string Name, List<StatementObservation> Statements);
    private sealed unsafe class NativeBytes : IDisposable
    {
        public byte* Pointer { get; private set; }
        public int Length { get; }
        public NativeBytes(ReadOnlySpan<byte> bytes)
        {
            Length = bytes.Length;
            Pointer = (byte*)NativeMemory.Alloc(checked((nuint)bytes.Length + 1));
            if (Pointer == null) throw new OutOfMemoryException();
            bytes.CopyTo(new Span<byte>(Pointer, bytes.Length));
            Pointer[bytes.Length] = 0;
        }
        public static NativeBytes Utf8(string value) => new(Encoding.UTF8.GetBytes(value));
        public void Dispose() { var pointer = Pointer; Pointer = null; NativeMemory.Free(pointer); }
    }
    private sealed unsafe class Database : IDisposable
    {
        public CapturedDatabase* Pointer { get; private set; }
        private Database(CapturedDatabase* pointer) => Pointer = pointer;
        public static Database Open()
        {
            using var filename = NativeBytes.Utf8(":memory:");
            CapturedDatabase* pointer = null;
            var code = Api.sqlite3_open_v2((sbyte*)filename.Pointer, &pointer, 2 | 4, null);
            if (code != 0)
            {
                var message = pointer == null ? "no database handle" : Text(Api.sqlite3_errmsg(pointer));
                if (pointer != null) Api.sqlite3_close(pointer);
                throw new InvalidOperationException($"open: SQLite {code}: {message}");
            }
            if (pointer == null) throw new InvalidOperationException("Successful open returned no database.");
            return new(pointer);
        }
        public void Check(int code, string operation)
        {
            if (code != 0) throw new InvalidOperationException($"{operation}: SQLite {code}: {Text(Api.sqlite3_errmsg(Pointer))}");
        }
        public Statement Prepare(string sql)
        {
            if (sql.Contains('\0')) throw new ArgumentException("SQL entries must not contain an embedded NUL.");
            using var encoded = NativeBytes.Utf8(sql);
            void* statement = null;
            sbyte* tail = null;
            var code = Api.sqlite3_prepare_v2(Pointer, (sbyte*)encoded.Pointer, checked(encoded.Length + 1), &statement, &tail);
            if (code != 0)
            {
                var message = Text(Api.sqlite3_errmsg(Pointer));
                if (statement != null) Api.sqlite3_finalize(statement);
                throw new InvalidOperationException($"prepare: SQLite {code}: {message}; SQL={sql}");
            }
            try
            {
                if (statement == null || tail == null || (byte*)tail < encoded.Pointer || (byte*)tail > encoded.Pointer + encoded.Length)
                    throw new InvalidOperationException("Preparation did not retain one statement and a bounded input tail.");
                for (var remaining = (byte*)tail; remaining < encoded.Pointer + encoded.Length; ++remaining)
                    if (*remaining is not (9 or 10 or 11 or 12 or 13 or 32))
                        throw new InvalidOperationException("Each workload entry must contain exactly one SQL statement.");
                return new(this, statement, sql);
            }
            catch { if (statement != null) Api.sqlite3_finalize(statement); throw; }
        }
        public void Dispose()
        {
            var pointer = Pointer;
            if (pointer == null) return;
            var code = Api.sqlite3_close(pointer);
            if (code != 0) throw new InvalidOperationException($"close: SQLite {code}: {Text(Api.sqlite3_errmsg(pointer))}");
            Pointer = null;
        }
    }
    private sealed unsafe class Statement(Database database, void* pointer, string sql) : IDisposable
    {
        private void* handle = pointer;
        private readonly List<NativeBytes> bindingOwners = [];
        private bool errorReported;
        private void Check(int code, string operation)
        {
            if (code == 0) return;
            errorReported = true;
            database.Check(code, operation + ": " + sql);
        }
        public void BindFirst()
        {
            Check(Api.sqlite3_bind_int64(handle, 1, long.MinValue + 1), "bind int64");
            Check(Api.sqlite3_bind_double(handle, 2, 3.25), "bind double");
            BindBytes(3, Encoding.UTF8.GetBytes("a\0z"), true);
            BindBytes(4, [0, 1, 254, 255], false);
            Check(Api.sqlite3_bind_null(handle, 5), "bind null");
        }
        public void BindEmpty()
        {
            Check(Api.sqlite3_bind_int64(handle, 1, 42), "bind int64");
            Check(Api.sqlite3_bind_double(handle, 2, -0.5), "bind double");
            BindBytes(3, [], true);
            BindBytes(4, [], false);
            Check(Api.sqlite3_bind_null(handle, 5), "bind null");
        }
        private void BindBytes(int ordinal, ReadOnlySpan<byte> bytes, bool text)
        {
            var owner = new NativeBytes(bytes);
            bindingOwners.Add(owner);
            // Captured destructor routine pointers use nint. SQLITE_STATIC(0):
            // own every passed byte buffer until finalization, including empty BLOB.
            var code = text ? Api.sqlite3_bind_text(handle, ordinal, (sbyte*)owner.Pointer, owner.Length, 0)
                : Api.sqlite3_bind_blob(handle, ordinal, owner.Pointer, owner.Length, 0);
            Check(code, text ? "bind text" : "bind blob");
        }
        public void ResetAndClear()
        {
            Check(Api.sqlite3_reset(handle), "reset");
            Check(Api.sqlite3_clear_bindings(handle), "clear bindings");
        }
        public StatementObservation Read()
        {
            var count = Api.sqlite3_column_count(handle);
            if (count is < 0 or > 10000) throw new InvalidOperationException("Column budget exceeded.");
            var columns = new string[count];
            for (var index = 0; index < count; ++index) columns[index] = Text(Api.sqlite3_column_name(handle, index));
            var rows = new List<Value[]>();
            while (true)
            {
                var code = Api.sqlite3_step(handle);
                if (code == 101) break;
                if (code != 100)
                {
                    Check(code, "step");
                    throw new InvalidOperationException("step returned neither ROW nor DONE: " + code);
                }
                var row = new Value[count];
                for (var index = 0; index < count; ++index) row[index] = ReadValue(index);
                rows.Add(row);
                if (rows.Count > 10000) throw new InvalidOperationException("Frozen per-statement row budget exceeded.");
            }
            return new(sql, columns, rows);
        }
        private Value ReadValue(int index)
        {
            var kind = Api.sqlite3_column_type(handle, index);
            if (kind == 1) return new("integer", Decimal: Api.sqlite3_column_int64(handle, index).ToString(CultureInfo.InvariantCulture));
            if (kind == 2) return new("real", Binary64Hex: unchecked((ulong)BitConverter.DoubleToInt64Bits(Api.sqlite3_column_double(handle, index))).ToString("x16", CultureInfo.InvariantCulture));
            if (kind is 3 or 4)
            {
                void* address = kind == 3 ? Api.sqlite3_column_text(handle, index) : Api.sqlite3_column_blob(handle, index);
                var bytes = Api.sqlite3_column_bytes(handle, index);
                if (bytes < 0 || bytes > 64 * 1024 * 1024 || bytes != 0 && address == null)
                    throw new InvalidOperationException("A column has an invalid/oversized native byte extent.");
                var hex = bytes == 0 ? "" : Convert.ToHexStringLower(new ReadOnlySpan<byte>(address, bytes));
                return new(kind == 3 ? "text-utf8" : "blob", Bytes: bytes, Hex: hex);
            }
            if (kind == 5) return new("null");
            throw new InvalidOperationException("Unknown SQLite storage class: " + kind);
        }
        public void Dispose()
        {
            var statement = handle;
            handle = null;
            if (statement == null) return;
            int code;
            try { code = Api.sqlite3_finalize(statement); }
            finally
            {
                foreach (var owner in bindingOwners) owner.Dispose();
                bindingOwners.Clear();
            }
            if (code != 0 && !errorReported) database.Check(code, "finalize: " + sql);
        }
    }
    private static string Text(sbyte* pointer)
        => pointer == null ? throw new InvalidOperationException("Expected borrowed UTF-8 bytes.") : Marshal.PtrToStringUTF8((nint)pointer)!;
    private static JsonObject Receipt(string path)
    {
        using var input = File.OpenRead(path);
        return new() { ["path"] = Path.GetFullPath(path), ["bytes"] = input.Length,
            ["sha256"] = Convert.ToHexStringLower(SHA256.HashData(input)) };
    }
    private static JsonArray CaseNodes(List<CaseObservation> cases)
    {
        var result = new JsonArray();
        foreach (var item in cases)
        {
            var statements = new JsonArray();
            foreach (var statement in item.Statements)
            {
                var columns = new JsonArray();
                foreach (var column in statement.Columns) columns.Add((JsonNode?)JsonValue.Create(column));
                var rows = new JsonArray();
                foreach (var row in statement.Rows)
                {
                    var values = new JsonArray();
                    foreach (var value in row)
                    {
                        var node = new JsonObject { ["type"] = value.Type };
                        if (value.Decimal != null) node["decimal"] = value.Decimal;
                        if (value.Binary64Hex != null) node["binary64Hex"] = value.Binary64Hex;
                        if (value.Bytes != null) node["bytes"] = value.Bytes.Value;
                        if (value.Hex != null) node["hex"] = value.Hex;
                        values.Add((JsonNode)node);
                    }
                    rows.Add((JsonNode)values);
                }
                statements.Add((JsonNode)new JsonObject { ["sql"] = statement.Sql, ["columns"] = columns, ["rows"] = rows });
            }
            result.Add((JsonNode)new JsonObject { ["name"] = item.Name, ["statements"] = statements });
        }
        return result;
    }
    private static List<CaseObservation> BindSmoke()
    {
        using var database = Database.Open();
        const string sql = "SELECT ?1 AS n,?2 AS r,?3 AS t,?4 AS b,?5 AS missing";
        using var statement = database.Prepare(sql);
        statement.BindFirst();
        var first = statement.Read();
        Value[] expected = [new("integer", Decimal: "-9223372036854775807"), new("real", Binary64Hex: "400a000000000000"),
            new("text-utf8", Bytes: 3, Hex: "61007a"), new("blob", Bytes: 4, Hex: "0001feff"), new("null")];
        if (first.Rows.Count != 1 || !first.Rows[0].SequenceEqual(expected)) throw new InvalidOperationException("Bound typed values differ from the frozen smoke expectation.");
        statement.ResetAndClear();
        var cleared = statement.Read();
        if (cleared.Rows.Count != 1 || cleared.Rows[0].Any(value => value.Type != "null")) throw new InvalidOperationException("clear_bindings did not replace every bound value with NULL.");
        statement.ResetAndClear();
        statement.BindEmpty();
        var empty = statement.Read();
        Value[] expectedEmpty = [new("integer", Decimal: "42"), new("real", Binary64Hex: "bfe0000000000000"),
            new("text-utf8", Bytes: 0, Hex: ""), new("blob", Bytes: 0, Hex: ""), new("null")];
        if (empty.Rows.Count != 1 || !empty.Rows[0].SequenceEqual(expectedEmpty)) throw new InvalidOperationException("Empty bound text/blob values changed storage class.");
        return [new("bound-values-reset-clear", [first, cleared, empty])];
    }
    private static List<CaseObservation> SqlSmoke(JsonElement workload)
    {
        var result = new List<CaseObservation>();
        var statements = 0;
        foreach (var item in workload.GetProperty("cases").EnumerateArray())
        {
            using var database = Database.Open();
            var observations = new List<StatementObservation>();
            result.Add(new(item.GetProperty("name").GetString()!, observations));
            foreach (var sql in item.GetProperty("statements").EnumerateArray())
            {
                using var statement = database.Prepare(sql.GetString()!);
                observations.Add(statement.Read());
                ++statements;
            }
        }
        if (result.Count != 5 || statements != 43) throw new InvalidOperationException("The accepted five-case/43-statement workload was changed or reduced.");
        return result;
    }
    private static int Main(string[] args)
    {
        if (args.Length is not (3 or 4) || args[0] is not ("bind-smoke" or "sql-smoke"))
            throw new ArgumentException("SqliteGenerated bind-smoke|sql-smoke WORKLOAD_JSON NEW_REPORT [SEPARATE_NATIVE_OBSERVATIONS]");
        var reportPath = Path.GetFullPath(args[2]);
        if (File.Exists(reportPath)) throw new ArgumentException("Use a fresh execution report path.");
        var workloadReceipt = Receipt(args[1]);
        if (workloadReceipt["sha256"]!.GetValue<string>() != WorkloadSha256)
            throw new ArgumentException("The accepted 43-statement workload identity changed.");
        using var workload = JsonDocument.Parse(File.ReadAllText(args[1]));
        if (workload.RootElement.GetProperty("workloadVersion").GetInt32() != 1) throw new ArgumentException("Unsupported workload version.");
        var report = new JsonObject
        {
            ["status"] = "incomplete", ["role"] = "generated-csharp-application-only", ["generatedExecution"] = true,
            ["nativeApplicationFallbackPermitted"] = false, ["mode"] = args[0], ["workload"] = workloadReceipt,
            ["remainingT4Areas"] = JsonNode.Parse(workload.RootElement.GetProperty("remainingT4Areas").GetRawText())
        };
        try
        {
            var version = Api.sqlite3_libversion_number();
            report["sqliteVersion"] = Text(Api.sqlite3_libversion());
            report["sqliteVersionNumber"] = version;
            if (version != 3054000) throw new InvalidOperationException("Expected the pinned generated SQLite3.54.0 application.");
            var cases = args[0] == "bind-smoke" ? BindSmoke() : SqlSmoke(workload.RootElement);
            report["cases"] = CaseNodes(cases);
            if (args.Length == 4)
            {
                var oracle = JsonNode.Parse(File.ReadAllText(args[3]))!;
                if (oracle["role"]?.GetValue<string>() != "separate-native-test-oracle-only" ||
                    oracle["status"]?.GetValue<string>() != "passed-native-oracle-only" ||
                    oracle["sqliteVersionNumber"]?.GetValue<int>() != 3054000 ||
                    oracle["workload"]?["sha256"]?.GetValue<string>() != WorkloadSha256 ||
                    !JsonNode.DeepEquals(report["cases"], oracle["cases"]))
                    throw new InvalidOperationException("Typed generated observations differ from the separate native oracle.");
                report["nativeComparison"] = "exact-typed-cases-equal";
                report["nativeObservationReceipt"] = Receipt(args[3]);
            }
            var paths = File.ReadLines("/proc/self/maps").Select(line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                .Where(parts => parts.Length >= 6 && parts[^1].StartsWith('/') && parts[^1].Contains(".so", StringComparison.Ordinal))
                .Select(parts => parts[^1]).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
            if (paths.Any(path => Path.GetFileName(path).Contains("sqlite", StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Generated execution loaded an unauthorized native SQLite application library.");
            var libraries = new JsonArray();
            foreach (var path in paths) libraries.Add((JsonNode)Receipt(path));
            report["loadedLibraries"] = libraries;
            report["status"] = args[0] == "bind-smoke" ? "passed-generated-bind-smoke" : "passed-generated-initial-43-statement-smoke";
            Console.WriteLine(new JsonObject { ["status"] = report["status"]!.GetValue<string>(), ["cases"] = cases.Count,
                ["statements"] = cases.Sum(item => item.Statements.Count) }.ToJsonString());
            return 0;
        }
        catch (Exception error) { report["status"] = "failed-generated-execution"; report["error"] = error.ToString(); throw; }
        finally
        {
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);
            File.WriteAllText(reportPath, report.ToJsonString(JsonOptions) + "\n");
        }
    }
}
