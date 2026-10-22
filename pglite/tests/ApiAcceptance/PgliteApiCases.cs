using System.Buffers.Binary;
using System.Globalization;
using System.Text;
using System.Text.Json;
using Managed.PGlite;

namespace Managed.PGlite.ProductTests;

/// <summary>Real-engine API checks derived from the pinned upstream tests.
/// These are separate from the original 36 unchanged engine assertions.</summary>
internal static partial class PgliteApiCases
{
    private static int assertions;
    private static void Check(bool condition, string name)
    {
        if (!condition) throw new InvalidOperationException("API acceptance: " + name);
        assertions++;
        Console.WriteLine("PASS api/" + name);
    }

    internal static int Run(PgliteOptions supplied)
    {
        assertions = 0;
        var options = supplied with { DataDirectory = supplied.DataDirectory + "-api" };
        using (var database = PgliteDatabase.Create(options))
        {
            Differential(database);
            TypedValues(database);
            int codecs = PgliteCodecCases.Run(database);
            assertions += codecs;
            Console.WriteLine($"PASS api/{codecs} upstream-derived real-engine codec/template assertions.");
            RawProtocol(database);
            Services(database);
        }
        WorkerAndStorage(options);
        Console.WriteLine($"PASS {assertions} real translated-engine API assertions.");
        return assertions;
    }

    private static object? Parameter(JsonElement item) => item.ValueKind switch
    {
        JsonValueKind.Null => null,
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.String => item.GetString(),
        JsonValueKind.Number => item.TryGetInt32(out int n) ? n : item.GetDouble(),
        JsonValueKind.Array => item.EnumerateArray().Select(Parameter).ToArray(),
        JsonValueKind.Object when item.TryGetProperty("$bytes", out var bytes) =>
            bytes.EnumerateArray().Select(b => b.GetByte()).ToArray(),
        JsonValueKind.Object when item.TryGetProperty("$json", out var json) => json.Clone(),
        _ => throw new InvalidDataException("Unknown differential parameter fixture.")
    };

    private static void Differential(PgliteDatabase database)
    {
        using var fixture = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "api-oracle-cases.json")));
        foreach (var test in fixture.RootElement.GetProperty("cases").EnumerateArray())
        {
            string name = test.GetProperty("name").GetString()!;
            string sql = test.GetProperty("sql").GetString()!;
            var parameters = test.GetProperty("params").EnumerateArray().Select(Parameter).ToArray();
            PgliteQueryResult result;
            try { result = database.Query(sql, parameters); }
            catch (PgliteSqlException error) { result = error.Result; }
            if (test.TryGetProperty("sqlstate", out var state))
                Check(result.Errors.Count == 1 && result.Errors[0].SqlState == state.GetString() &&
                    database.IsReady, "differential/" + name);
            else
            {
                result.EnsureSuccess();
                var expected = test.GetProperty("rows").EnumerateArray()
                    .Select(r => r.EnumerateArray().Select(c => c.ValueKind == JsonValueKind.Null ? null : c.GetString()).ToArray()).ToArray();
                var actual = result.ResultSets.Single().Rows;
                Check(actual.Count == expected.Length && actual.Zip(expected).All(pair => pair.First.SequenceEqual(pair.Second)),
                    "differential/" + name);
            }
        }
    }

    private static void TypedValues(PgliteDatabase database)
    {
        var result = database.Query("SELECT $1::int AS n, $2::bool AS b, $3::int8 AS big, $4::bytea AS bytes, $5::jsonb AS doc, $6::date AS day",
            new object?[] { 42, true, long.MaxValue, new byte[] { 0, 128, 255 }, JsonDocument.Parse("{\"a\":1}").RootElement.Clone(), new DateOnly(2024, 2, 29) }).EnsureSuccess();
        var row = result.ResultSets.Single().TypedRows.Single();
        Check(row[0] is int n && n == 42 && row[1] is true && row[2] is long l && l == long.MaxValue,
            "typed/integer-boolean-bigint");
        Check(row[3] is byte[] bytes && bytes.SequenceEqual(new byte[] { 0, 128, 255 }) &&
              row[4] is JsonElement json && json.GetProperty("a").GetInt32() == 1 && row[5] is DateOnly day && day.Day == 29,
            "typed/binary-json-date");
        Check(result.ResultSets.Single().Columns.Select(c => c.TypeOid).SequenceEqual(new uint[] { 23, 16, 20, 17, 3802, 1082 }),
            "typed/field-oids");
        var array = database.Query("SELECT $1::int[]", new object?[] { new int?[] { 1, null, 3 } }).EnsureSuccess()
            .ResultSets.Single().TypedRows.Single()[0];
        Check(array is object?[] values && values.SequenceEqual(new object?[] { 1, null, 3 }), "typed/nullable-array");
        var template = PgliteTemplate.Sql($"SELECT {"a'); DROP TABLE nowhere; --"}::text AS {PgliteTemplate.Identifier("odd\"column")}, {42}::int");
        var templated = database.Sql(template).EnsureSuccess().ResultSets.Single();
        Check(templated.Columns[0].Name == "odd\"column" && templated.Rows[0][0] == "a'); DROP TABLE nowhere; --" &&
              templated.TypedRows[0][1] is int value && value == 42, "template/safe-values-and-identifiers");
        var custom = PgliteCodecs.Default.Register(23, text => "number:" + text, value =>
            value is string text && text.StartsWith("number:", StringComparison.Ordinal) ? text[7..] : Convert.ToString(value, CultureInfo.InvariantCulture)!);
        var customized = database.Query("SELECT $1::int", new object?[] { new PgliteParameter("number:73", 23) },
            new PgliteQueryOptions { Codecs = custom }).EnsureSuccess();
        Check(Equals(customized.ResultSets[0].TypedRows[0][0], "number:73"), "typed/custom-codec");
        database.Query("CREATE TYPE api_mood AS ENUM ('sad','happy')").EnsureSuccess();
        database.RefreshArrayTypes();
        var moods = database.Query("SELECT $1::api_mood[]", new object?[] { new[] { "sad", "happy" } }).EnsureSuccess();
        Check(moods.ResultSets[0].TypedRows[0][0] is object?[] mood && mood.SequenceEqual(new[] { "sad", "happy" }),
            "typed/dynamic-enum-array");
        var description = database.DescribeQuery("SELECT $2::text AS name, $1::int AS age WHERE $3::bool");
        Check(description.ParameterTypes.SequenceEqual(new uint[] { 23, 25, 16 }) &&
            description.Columns.Select(c => c.Name).SequenceEqual(new[] { "name", "age" }) &&
            description.Columns.Select(c => c.TypeOid).SequenceEqual(new uint[] { 25, 23 }), "describe/parameters-and-fields");
        var describedInsert = database.DescribeQuery("CREATE TABLE api_describe_table(id int)");
        Check(describedInsert.ParameterTypes.Count == 0 && describedInsert.Columns.Count == 0,
            "describe/does-not-execute-ddl");
        Check(database.Query("SELECT to_regclass('api_describe_table') IS NULL").EnsureSuccess().ResultSets[0].Rows[0][0] == "t",
            "describe/no-side-effects");
        Check(Equals(templated.TypedObjectRows[0]["odd\"column"], "a'); DROP TABLE nowhere; --"), "typed/object-row-mode");
        Check(database.FormatQuery("SELECT $2::text, $1::int, $2::text", new object?[] { 42, "O'Reilly" }) ==
            "SELECT 'O''Reilly'::text, '42'::int, 'O''Reilly'::text", "format/reordered-repeated-and-escaped");
        Check(database.FormatQuery("SELECT $1::int, '$1', $$ $1 $$ AS foo$1 /* $1 */ -- $1", new object?[] { 7 }) ==
            "SELECT '7'::int, '$1', $$ $1 $$ AS foo$1 /* $1 */ -- $1", "format/literals-identifiers-comments");
        Check(database.FormatQuery("SELECT $1::int AS a --comment\r, $2::int AS b", new object?[] { 1, 2 }) ==
            "SELECT '1'::int AS a --comment\r, '2'::int AS b", "format/carriage-return-ends-comment");
        database.Exec("SET standard_conforming_strings=off");
        try
        {
            Check(database.FormatQuery("SELECT $1::int, 'escaped\\' $1' AS preserved", new object?[] { 7 }) ==
                "SELECT '7'::int, 'escaped\\' $1' AS preserved", "format/session-string-escape-rules");
        }
        finally { database.Exec("SET standard_conforming_strings=on"); }
        int queryNotices = 0;
        database.Exec("DO $$ BEGIN RAISE NOTICE 'query-specific'; END $$", new PgliteQueryOptions
            { OnNotice = notice => { if (notice.Message == "query-specific") queryNotices++; } });
        Check(queryNotices == 1, "notice/per-query-callback");
        bool callbackFailed = false;
        try
        {
            database.Exec("DO $$ BEGIN RAISE NOTICE 'query-specific-error'; END $$", new PgliteQueryOptions
                { OnNotice = _ => throw new ApplicationException("notice callback") });
        }
        catch (AggregateException) { callbackFailed = true; }
        Check(callbackFailed && database.IsReady && database.Query("SELECT 42").EnsureSuccess().ResultSets[0].Rows[0][0] == "42",
            "notice/per-query-error-preserves-core");
    }

    // Independent fixture framing prevents a serializer bug from making its own
    // round-trip test pass. The server consumes these original v3 wire messages.
    private static byte[] Frame(char type, params byte[][] parts)
    {
        int length = parts.Sum(p => p.Length);
        byte[] frame = new byte[length + 5]; frame[0] = (byte)type;
        BinaryPrimitives.WriteInt32BigEndian(frame.AsSpan(1), length + 4);
        int offset = 5;
        foreach (var part in parts) { part.CopyTo(frame, offset); offset += part.Length; }
        return frame;
    }
    private static byte[] CString(string text) => Encoding.UTF8.GetBytes(text + "\0");
    private static byte[] Parse(string sql) => Frame('P', new byte[] { 0 }, CString(sql), new byte[] { 0, 0 });
    private static byte[] Bind(string? text = null)
    {
        if (text == null) return Frame('B', new byte[8]);
        byte[] data = Encoding.UTF8.GetBytes(text), length = new byte[4];
        BinaryPrimitives.WriteInt32BigEndian(length, data.Length);
        return Frame('B', new byte[] { 0, 0, 0, 0, 0, 1 }, length, data, new byte[] { 0, 0 });
    }
    private static byte[] Batch(params byte[][] messages) => messages.SelectMany(m => m).ToArray();
    private static string MessageTypes(byte[] data)
    {
        var types = new StringBuilder();
        for (int offset = 0; offset < data.Length;)
        {
            Check(data.Length - offset >= 5, "protocol/complete-header");
            int length = BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(offset + 1));
            if (length < 4 || length + 1 > data.Length - offset) throw new InvalidDataException("Malformed server response.");
            types.Append((char)data[offset]); offset += length + 1;
        }
        return types.ToString();
    }
    private static void RawProtocol(PgliteDatabase database)
    {
        var raw = database.ExecProtocolRaw(Frame('Q', CString("SELECT 1")));
        Check(MessageTypes(raw) == "TDCZ", "protocol/simple-message-order");
        Check(MessageTypes(database.ExecProtocolRaw(Parse("SELECT 42"))) == "1", "protocol/split-parse");
        Check(MessageTypes(database.ExecProtocolRaw(Bind())) == "2", "protocol/split-bind");
        Check(MessageTypes(database.ExecProtocolRaw(Frame('D', new byte[] { (byte)'P', 0 }))) == "T", "protocol/split-describe");
        Check(MessageTypes(database.ExecProtocolRaw(Frame('E', new byte[5]))) == "DC", "protocol/split-execute");
        Check(MessageTypes(database.ExecProtocolRaw(Frame('S'))) == "Z", "protocol/split-sync");
        database.Query("CREATE TABLE api_unique(id int PRIMARY KEY); INSERT INTO api_unique VALUES(1)").EnsureSuccess();
        foreach (bool stream in new[] { false, true })
        {
            byte[] Send(byte[] request)
            {
                if (!stream) return database.ExecProtocolRaw(request);
                using var output = new MemoryStream();
                database.ExecProtocolRawStream(request, chunk => output.Write(chunk.Span));
                return output.ToArray();
            }
            foreach (var failure in new[]
            {
                (Name: "parse", Input: Parse("SELECT FROM"), Expected: "E"),
                (Name: "bind", Input: Batch(Parse("SELECT $1::int"), Bind("not-an-integer")), Expected: "1E"),
                (Name: "execute", Input: Batch(Parse("INSERT INTO api_unique VALUES($1)"), Bind("1"), Frame('D', new byte[] { (byte)'P', 0 }), Frame('E', new byte[5])), Expected: "12nE")
            })
            {
                Check(MessageTypes(Send(failure.Input)) == failure.Expected, $"protocol/{stream}/{failure.Name}-error");
                Check(Send(Batch(Parse("SELECT 99"), Bind(), Frame('E', new byte[5]), Frame('H'))).Length == 0,
                    $"protocol/{stream}/{failure.Name}-discard-until-sync");
                var sync = Send(Frame('S'));
                Check(MessageTypes(sync) == "Z" && sync[^1] == (byte)'I', $"protocol/{stream}/{failure.Name}-single-ready");
                Check(database.Query("SELECT $1::int", new object?[] { 42 }).EnsureSuccess().ResultSets[0].Rows[0][0] == "42",
                    $"protocol/{stream}/{failure.Name}-recover");
            }
        }
        var messageOrder = new List<char>();
        var streamed = database.ExecProtocolStream(Frame('Q', CString("SELECT 73")), message => messageOrder.Add(message.Type));
        Check(messageOrder.SequenceEqual(new[] { 'T', 'D', 'C', 'Z' }) && streamed.ResultSets[0].Rows[0][0] == "73",
            "protocol/decoded-stream-order");
        byte[] integer = new byte[4]; BinaryPrimitives.WriteInt32BigEndian(integer, 42);
        var binary = database.ExecProtocol(PgliteProtocol.Concat(PgliteProtocol.EncodeParse("SELECT $1::int", new uint[] { 23 }),
            PgliteProtocol.EncodeBindRaw(new byte[]?[] { integer }, new short[] { 1 }, new short[] { 1 }),
            PgliteProtocol.EncodeDescribe('P'), PgliteProtocol.EncodeExecute(), PgliteProtocol.EncodeSync()));
        Check(binary.ResultSets[0].RawRows[0][0]!.SequenceEqual(integer) && binary.ResultSets[0].Columns[0].FormatCode == 1,
            "protocol/binary-bind-and-results");
        database.Exec("CREATE TABLE api_wire_copy(id int, value text)");
        var copied = database.ExecProtocol(Batch(Frame('Q', CString("COPY api_wire_copy FROM STDIN WITH (FORMAT csv)")),
            Frame('d', Encoding.UTF8.GetBytes("1,Žąsis\n2,wire\n")), Frame('c')));
        Check(copied.CopyResponses.Single().Direction == 'G' && copied.ResultSets.Single().Command == "COPY 2",
            "protocol/copy-from-stdin");
        var exported = database.ExecProtocol(Frame('Q', CString("COPY api_wire_copy TO STDOUT WITH (FORMAT csv)")));
        Check(exported.CopyResponses.Single().Direction == 'H' &&
            Encoding.UTF8.GetString(exported.CopyData.SelectMany(chunk => chunk).ToArray()) == "1,Žąsis\n2,wire\n",
            "protocol/copy-to-stdout");
        var copyError = database.ExecProtocol(Batch(Frame('Q', CString("COPY api_wire_copy FROM STDIN")),
            Frame('f', CString("caller COPY cancelled"))), throwOnError: false);
        Check(copyError.Errors.Count == 1 && copyError.TransactionStatus == PgliteTransactionStatus.Idle &&
            database.Query("SELECT count(*) FROM api_wire_copy").EnsureSuccess().ResultSets[0].Rows[0][0] == "2",
            "protocol/copy-fail-and-recovery");
    }

    static partial void Services(PgliteDatabase database);
    static partial void WorkerAndStorage(PgliteOptions options);
}
