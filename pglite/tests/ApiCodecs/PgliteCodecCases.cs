using System.Globalization;
using System.Numerics;
using System.Text.Json;
using Managed.PGlite;

/// <summary>Upstream-derived codec and template cases requiring the actual
/// translated PostgreSQL engine. Can also be linked into the ordinary consumer.</summary>
public static class PgliteCodecCases
{
    public static int Run(PgliteDatabase database)
    {
        int assertions = 0;
        void Equal(object? expected, object? actual, string name)
        {
            assertions++;
            if (expected is object?[] left && actual is object?[] right)
            {
                if (left.Length != right.Length) throw new Exception(name + ": array length mismatch.");
                for (int i = 0; i < left.Length; i++) Equal(left[i], right[i], name + "[" + i + "]");
            }
            else if (!Equals(expected, actual)) throw new Exception(name + ": expected " + expected + ", got " + actual);
        }
        object? Scalar(string sql, IReadOnlyList<object?> parameters, PgliteQueryOptions? options = null)
            => database.Query(sql, parameters, options).EnsureSuccess().ResultSets[0].TypedRows[0][0];

        database.Exec("SET DateStyle = 'ISO, YMD'; SET TimeZone = 'UTC'");
        Equal(null, Scalar("SELECT $1::int", [null]), "SQL NULL");
        Equal("42", Scalar("SELECT $1", [42]), "unconstrained parameter remains upstream text");
        Equal("true", Scalar("SELECT $1", [true]), "unconstrained boolean remains upstream text");
        Equal(123, Scalar("SELECT $1::int", [123]), "integer parameter");
        Equal(long.MaxValue, Scalar("SELECT $1::bigint", [long.MaxValue]), "exact int64 parameter");
        Equal(new PgliteNumeric("123456789012345678901234567890.00000000000000000001"),
            Scalar("SELECT $1::numeric", [new PgliteNumeric("123456789012345678901234567890.00000000000000000001")]), "numeric precision and scale");
        Equal(true, Scalar("SELECT $1::bool", [1]), "TypeORM boolean 1");
        Equal(false, Scalar("SELECT $1::bool", [" FALSE "]), "boolean text");
        Equal(double.PositiveInfinity, Scalar("SELECT $1::float8", [double.PositiveInfinity]), "float infinity");

        var bytes = (byte[])Scalar("SELECT $1::bytea", [new byte[] { 0, 1, 92, 255 }])!;
        Equal(true, bytes.AsSpan().SequenceEqual(new byte[] { 0, 1, 92, 255 }), "bytea parameter");
        var json = (JsonElement)Scalar("SELECT $1::jsonb", [new PgliteJson("{\"test\":1}")])!;
        Equal(1, json.GetProperty("test").GetInt32(), "JSONB parameter");
        var jsonArray = (JsonElement)Scalar("SELECT $1::jsonb", [new[] { "hello", "world" }])!;
        Equal("world", jsonArray[1].GetString(), "upstream array sent to JSON parameter");
        var jsonBigint = (JsonElement)Scalar("SELECT $1::jsonb", [new Dictionary<string, object?> { ["id"] = BigInteger.Parse("9007199254740993") }])!;
        Equal("9007199254740993", jsonBigint.GetProperty("id").GetString(), "upstream JSON bigint replacer");
        Equal(new DateOnly(2021, 1, 1), Scalar("SELECT $1::date", [new DateOnly(2021, 1, 1)]), "date parameter");
        Equal(new DateTime(2021, 1, 1, 12, 3, 4).AddTicks(1234560),
            Scalar("SELECT $1::timestamp", [new DateTime(2021, 1, 1, 12, 3, 4).AddTicks(1234560)]), "timestamp microseconds");
        Equal(new DateTimeOffset(2021, 1, 1, 12, 3, 4, TimeSpan.FromHours(2)),
            Scalar("SELECT $1::timestamptz", [new DateTimeOffset(2021, 1, 1, 12, 3, 4, TimeSpan.FromHours(2))]), "timestamp instant");
        Equal(new TimeOnly(12, 3, 4).Add(TimeSpan.FromTicks(1234560)),
            Scalar("SELECT $1::time", [new TimeOnly(12, 3, 4).Add(TimeSpan.FromTicks(1234560))]), "time microseconds");
        Equal(new PgliteTimeWithTimeZone(new TimeOnly(12, 3, 4), TimeSpan.FromSeconds(-9001)),
            Scalar("SELECT $1::timetz", [new PgliteTimeWithTimeZone(new TimeOnly(12, 3, 4), TimeSpan.FromSeconds(-9001))]), "timetz offset seconds");
        Equal("infinity", Scalar("SELECT 'infinity'::date", []), "date infinity preserves text");
        Equal("infinity", Scalar("SELECT 'infinity'::interval", []), "interval infinity preserves text");
        Equal("-infinity", Scalar("SELECT '-infinity'::interval", []), "negative interval infinity preserves text");
        Equal("24:00:00", Scalar("SELECT '24:00:00'::time", []), "time 24:00 preserves text");
        var interval = new PgliteInterval(14, 3, 14_706_123456);
        foreach (string style in new[] { "postgres", "postgres_verbose", "sql_standard", "iso_8601" })
        {
            database.Exec("SET IntervalStyle = '" + style + "'");
            Equal(interval, Scalar("SELECT $1::interval", [interval]), "interval style " + style);
        }
        database.Exec("SET IntervalStyle = 'postgres'");

        object?[] tricky = [null, "hello", "NULL", "", "test,3", "a\"b", "a\\b", "{}", " leading ", "x'y"];
        Equal(tricky, Scalar("SELECT $1::text[]", [tricky]), "NULL, escaping and special array elements");
        Equal(new object?[] { null, 123, 0 }, Scalar("SELECT $1::int[]", [new object?[] { null, 123, 0 }]), "nullable integer array");
        Equal(new object?[] { new object?[] { 1.1d, 2.2d }, new object?[] { 3.3d, 4.4d } },
            Scalar("SELECT $1::float8[]", [new double[,] { { 1.1, 2.2 }, { 3.3, 4.4 } }]), "multidimensional array");
        Equal(new object?[] { "a", null, "NULL" }, Scalar("SELECT '[0:2]={a,NULL,\"NULL\"}'::text[]", []), "nondefault lower bound array");
        Equal(Array.Empty<object?>(), Scalar("SELECT $1::int[]", [Array.Empty<int>()]), "empty array");
        var blobs = (object?[])Scalar("SELECT $1::bytea[]", [new[] { new byte[] { 0, 92 }, new byte[] { 255 } }])!;
        Equal(true, ((byte[])blobs[0]!).AsSpan().SequenceEqual(new byte[] { 0, 92 }) && ((byte[])blobs[1]!)[0] == 255, "bytea array");

        var numericCodecs = PgliteCodecs.Default.Register(1700, text => BigInteger.Parse(text, CultureInfo.InvariantCulture),
            value => ((BigInteger)value).ToString(CultureInfo.InvariantCulture));
        var customOptions = new PgliteQueryOptions { Codecs = numericCodecs };
        Equal(new BigInteger(100), Scalar("SELECT $1::numeric", [new BigInteger(100)], customOptions), "upstream custom numeric parser/serializer");
        Equal(new object?[] { new BigInteger(100), null, new BigInteger(200) },
            Scalar("SELECT $1::numeric[]", [new object?[] { new BigInteger(100), null, new BigInteger(200) }], customOptions), "custom codec array elements");
        Equal(new PgliteNumeric("100"), Scalar("SELECT 100::numeric", []), "query codec override does not mutate defaults");

        database.Exec("CREATE TYPE api_codec_mood AS ENUM ('sad', 'happy')");
        var typeRow = database.Query("SELECT 'api_codec_mood'::regtype::oid, 'api_codec_mood[]'::regtype::oid").EnsureSuccess().ResultSets[0].Rows[0];
        uint elementOid = uint.Parse(typeRow[0]!, CultureInfo.InvariantCulture);
        uint arrayOid = uint.Parse(typeRow[1]!, CultureInfo.InvariantCulture);
        var enumCodecs = PgliteCodecs.Default.Register(elementOid, text => new Mood(text), value => ((Mood)value).Name).RegisterArray(arrayOid, elementOid);
        Equal(new object?[] { new Mood("sad"), null, new Mood("happy") },
            Scalar("SELECT $1::api_codec_mood[]", [new PgliteParameter(new object?[] { new Mood("sad"), null, new Mood("happy") }, arrayOid)],
                new PgliteQueryOptions { Codecs = enumCodecs }), "registered custom enum array");
        database.RefreshArrayTypes();
        Equal(new object?[] { "sad", "happy" }, Scalar("SELECT $1::api_codec_mood[]", [new[] { "sad", "happy" }]), "upstream refreshed custom array types");
        database.Exec("DROP TYPE api_codec_mood");

        var quotedTable = PgliteTemplate.Identifier("api_codec_\"table");
        database.Sql(PgliteTemplate.Sql($"CREATE TEMP TABLE {quotedTable} (value text)")).EnsureSuccess();
        string attack = "x'); DROP TABLE api_codec_ignored; --";
        database.Sql(PgliteTemplate.Sql($"INSERT INTO {quotedTable} VALUES ({attack})")).EnsureSuccess();
        Equal(attack, database.Sql(PgliteTemplate.Sql($"SELECT value FROM {quotedTable}")).EnsureSuccess().ResultSets[0].TypedRows[0][0], "identifier escaping and data binding");
        var one = PgliteTemplate.Sql($"{1}::int");
        var expression = PgliteTemplate.Sql($"SELECT {one} + {2}::int + {one}");
        Equal(4, database.Sql(expression).EnsureSuccess().ResultSets[0].TypedRows[0][0], "nested fragment numbering through engine");
        var joined = PgliteTemplate.Sql($"SELECT 2 IN ({PgliteTemplate.Join(new[] { 1, 2, 3 })})");
        Equal(true, database.Sql(joined).EnsureSuccess().ResultSets[0].TypedRows[0][0], "joined parameters through engine");
        var arrayTemplate = PgliteTemplate.Sql($"SELECT 2 = ANY({new[] { 1, 2, 3 }}::int[])");
        Equal(true, database.Sql(arrayTemplate).EnsureSuccess().ResultSets[0].TypedRows[0][0], "array remains one parameter through engine");

        bool encoderThrew = false;
        try { Scalar("SELECT $1::bool", [2]); } catch (ArgumentException) { encoderThrew = true; }
        Equal(true, encoderThrew, "invalid boolean serializer throws");
        Equal(1, Scalar("SELECT 1", []), "encoder error leaves real engine recovered");
        var throwingCodec = PgliteCodecs.Default.Register(23, _ => throw new FormatException("codec probe"));
        bool decoderThrew = false;
        try { Scalar("SELECT 1", [], new PgliteQueryOptions { Codecs = throwingCodec }); } catch (FormatException) { decoderThrew = true; }
        Equal(true, decoderThrew, "custom parser exception is visible");
        Equal(2, Scalar("SELECT 2", []), "decoder error leaves real engine recovered");
        Equal(1, json.GetProperty("test").GetInt32(), "JSON result survives later engine calls");
        return assertions;
    }

    private sealed record Mood(string Name);
}
