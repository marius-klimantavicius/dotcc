using System.Collections;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Managed.PGlite;

int assertions = 0;
void Check(bool condition, string name)
{
    assertions++;
    if (!condition) throw new Exception("FAILED: " + name);
}
void Equal(object? expected, object? actual, string name)
{
    if (expected is object?[] expectedArray && actual is object?[] actualArray)
    {
        Check(expectedArray.Length == actualArray.Length, name + " length");
        for (int i = 0; i < expectedArray.Length; i++) Equal(expectedArray[i], actualArray[i], name + " element " + i);
    }
    else Check(Equals(expected, actual), name + ": expected " + expected + ", got " + actual);
}
void Throws<T>(Action action, string name) where T : Exception
{
    try { action(); } catch (T) { Check(true, name); return; }
    throw new Exception("FAILED (no exception): " + name);
}

var codecs = PgliteCodecs.Default;
Equal(null, codecs.Decode(null, 23), "null before parser");
Equal(null, codecs.Encode(null), "null parameter");
Equal(null, codecs.Encode(DBNull.Value), "DBNull parameter");
Equal("test", codecs.Decode("test", 0), "unknown type preserves text");
Equal((short)1, codecs.Decode("1", 21), "upstream int2");
Equal(1, codecs.Decode("1", 23), "upstream int4");
Equal(4294967295u, codecs.Decode("4294967295", 26), "unsigned OID");
Equal(9223372036854775807L, codecs.Decode("9223372036854775807", 20), "bigint exactness");
Equal(BigInteger.Parse("9223372036854775808"), codecs.Decode("9223372036854775808", 20), "out-of-range integer exactness");
Equal(1.1f, codecs.Decode("1.1", 700), "upstream float4");
Equal(1.1d, codecs.Decode("1.1", 701), "upstream float8");
Check(double.IsNaN((double)codecs.Decode("NaN", 701)!), "float NaN");
Equal(double.PositiveInfinity, codecs.Decode("Infinity", 701), "float infinity");
Equal("-Infinity", codecs.Encode(double.NegativeInfinity), "encode float infinity");
Equal(new PgliteNumeric("123456789012345678901234567890.00000000000000000001"),
    codecs.Decode("123456789012345678901234567890.00000000000000000001", 1700), "numeric preserves precision and scale");
Equal(new PgliteNumeric("NaN"), codecs.Decode("NaN", 1700), "numeric NaN");
Equal(new PgliteNumeric("-Infinity"), codecs.Decode("-Infinity", 1700), "numeric infinity");
Throws<FormatException>(() => new PgliteNumeric("1; DROP TABLE t"), "numeric validation");
Equal(true, codecs.Decode("t", 16), "upstream true");
Equal(false, codecs.Decode("f", 16), "upstream false");
Equal("true", codecs.Encode(true, 25), "upstream unconstrained boolean text");
Equal("false", codecs.Encode(false, 25), "upstream false boolean text");
foreach (string text in new[] { "true", "t", "yes", "y", "on", "1", "TRUE", " t " })
    Equal("t", codecs.Encode(text, 16), "upstream boolean truth literal " + text);
foreach (string text in new[] { "false", "f", "no", "n", "off", "0", "FALSE", " f " })
    Equal("f", codecs.Encode(text, 16), "upstream boolean false literal " + text);
Equal("t", codecs.Encode(1, 16), "TypeORM boolean 1");
Equal("f", codecs.Encode(0, 16), "TypeORM boolean 0");
Equal("t", codecs.Encode(1.00m, 16), "decimal boolean 1");
foreach (object value in new object[] { 2, -1, double.NaN, "ture", "maybe", "", new Hashtable() })
    Throws<ArgumentException>(() => codecs.Encode(value, 16), "reject invalid boolean");

var previousCulture = CultureInfo.CurrentCulture;
try
{
    CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
    Equal("1234.5", codecs.Encode(1234.5m), "invariant numeric output");
    Equal(1234.5d, codecs.Decode("1234.5", 701), "invariant numeric input");
    Equal("1.5", codecs.Encode(1.5d, 25), "upstream text from number");
}
finally { CultureInfo.CurrentCulture = previousCulture; }

Equal("\\x000102ff", codecs.Encode(new byte[] { 0, 1, 2, 255 }), "bytea hex encoding");
Check(((byte[])codecs.Decode("\\x000102ff", 17)!).AsSpan().SequenceEqual(new byte[] { 0, 1, 2, 255 }), "bytea hex decoding");
Check(((byte[])codecs.Decode("a\\000\\377\\\\", 17)!).AsSpan().SequenceEqual(new byte[] { 97, 0, 255, 92 }), "bytea escape decoding");
Throws<FormatException>(() => codecs.Decode("\\x0", 17), "bytea rejects odd hex");
Throws<FormatException>(() => codecs.Decode("\\400", 17), "bytea rejects invalid octal");
Throws<ArgumentException>(() => codecs.Encode("\\x01", 17), "upstream bytea requires bytes");

var decodedJson = (JsonElement)codecs.Decode("{\"test\":1}", 114)!;
Equal(1, decodedJson.GetProperty("test").GetInt32(), "upstream JSON parser");
Equal(1, ((JsonElement)codecs.Decode("{\"test\":1}", 3802)!).GetProperty("test").GetInt32(), "upstream JSONB parser");
Equal("{\"test\":1}", codecs.Encode(decodedJson, 3802), "owned JsonElement lifetime");
using (var jsonDocument = JsonDocument.Parse("[null,true,1]"))
    Equal("[null,true,1]", codecs.Encode(jsonDocument), "JsonDocument serializer");
Equal("\"hello\"", codecs.Encode(new PgliteJson("\"hello\"")), "explicit JSON scalar");
Equal("{\"test\":1}", codecs.Encode("{\"test\":1}", 114), "upstream JSON text serializer");
var jsonTree = new Dictionary<string, object?> { ["id"] = BigInteger.Parse("9007199254740993"), ["values"] = new object?[] { null, 1, true, "x" } };
var treeText = codecs.Encode(jsonTree, 3802)!;
using (var document = JsonDocument.Parse(treeText))
{
    Equal("9007199254740993", document.RootElement.GetProperty("id").GetString(), "upstream JSON bigint replacer");
    Equal(true, document.RootElement.GetProperty("values")[2].GetBoolean(), "reflection-free JSON tree");
}
var jsonCycle = new Dictionary<string, object?>(); jsonCycle["self"] = jsonCycle;
Throws<ArgumentException>(() => codecs.Encode(jsonCycle, 114), "JSON cycle guard");
var dangerous = new Dangerous();
Throws<ArgumentException>(() => codecs.Encode(dangerous), "unknown object never calls ToString");
Throws<ArgumentException>(() => codecs.Encode(dangerous, 114), "POCO never reflection serializes");
Throws<ArgumentException>(() => codecs.Encode("hello\0world"), "text NUL rejected");

Equal(new DateOnly(2021, 1, 1), codecs.Decode("2021-01-01", 1082), "upstream date");
Equal(new DateTime(2021, 1, 1, 12, 0, 0), codecs.Decode("2021-01-01 12:00:00", 1114), "upstream timestamp");
Equal(DateTimeKind.Unspecified, ((DateTime)codecs.Decode("2021-01-01 12:00:00", 1114)!).Kind, "timestamp has no machine timezone");
Equal(new DateTimeOffset(2021, 1, 1, 12, 0, 0, TimeSpan.FromHours(2)), codecs.Decode("2021-01-01 12:00:00+02", 1184), "timestamp numeric offset");
Equal(new TimeOnly(12, 3, 4).Add(TimeSpan.FromTicks(1234560)), codecs.Decode("12:03:04.123456", 1083), "microsecond time");
Equal(new PgliteTimeWithTimeZone(new TimeOnly(12, 3, 4), TimeSpan.FromSeconds(-9001)), codecs.Decode("12:03:04-02:30:01", 1266), "timetz offset seconds");
Equal(new PgliteTimeWithTimeZone(new TimeOnly(12, 0), TimeSpan.FromHours(15)), codecs.Decode("12:00:00+15", 1266), "timetz wide offset");
foreach (var (value, oid) in new (string, uint)[] { ("infinity", 1082), ("-infinity", 1114), ("infinity", 1184), ("infinity", 1186), ("-infinity", 1186), ("0001-01-01 BC", 1082), ("10000-01-01", 1082), ("24:00:00", 1083) })
    Equal(value, codecs.Decode(value, oid), "special temporal text remains lossless");
Equal("2023-01-01T00:00:00.000Z", codecs.Encode(1672531200000L, 1184), "upstream date from Unix milliseconds");
Throws<ArgumentException>(() => codecs.Encode(true, 1184), "upstream invalid date type");
var interval = new PgliteInterval(14, 3, 14_706_123456);
foreach (string value in new[] { "1 year 2 mons 3 days 04:05:06.123456", "@ 1 year 2 mons 3 days 4 hours 5 mins 6.123456 secs", "1-2 3 4:05:06.123456", "P1Y2M3DT4H5M6.123456S" })
    Equal(interval, codecs.Decode(value, 1186), "interval style " + value);
Equal(interval, codecs.Decode(codecs.Encode(interval, 1186), 1186), "interval exact round trip");
Equal(new PgliteInterval(-14, -3, -14_706_123456), codecs.Decode("@ 1 year 2 mons 3 days 4 hours 5 mins 6.123456 secs ago", 1186), "verbose interval ago");
Equal(new PgliteInterval(0, -3, 14_706_123456), codecs.Decode("P-3DT4H5M6.123456S", 1186), "mixed sign ISO interval");
Equal(new PgliteInterval(0, 0, 180_000_000_001), codecs.Decode("50:00:00.000001", 1186), "interval hours exceed 24");
Equal(new PgliteInterval(0, 0, 0), codecs.Decode("00:00:00", 1186), "zero interval");

object?[] tricky = [null, "hello", "NULL", "", "test,3", "a\"b", "a\\b", "{}", " leading ", "x'y"];
Equal(tricky, codecs.Decode(codecs.Encode(tricky, 1009), 1009), "upstream NULL and complex text array round trip");
Equal(new object?[] { "a,b", "a}b", "NULL", null, "", "a\\b", "a\"b" },
    codecs.Decode("{a\\,b,a\\}b,N\\ULL,NULL,\"\",\"a\\\\b\",\"a\\\"b\"}", 1009), "escaped unquoted and quoted array elements");
Equal(new object?[] { null, 123, 0 }, codecs.Decode("{NULL,123,0}", 1007), "upstream nullable integer array");
Equal(new object?[] { new object?[] { 1.1d, 2.2d }, new object?[] { 3.3d, null } },
    codecs.Decode("{{1.1,2.2},{3.3,NULL}}", 1022), "upstream multidimensional float arrays");
Equal(new object?[] { new object?[] { 1, 2 }, new object?[] { 3, 4 } },
    codecs.Decode(codecs.Encode(new int[,] { { 1, 2 }, { 3, 4 } }, 1007), 1007), "CLR rectangular arrays");
Equal(new object?[] { new object?[] { "a", "b" }, new object?[] { "c", null } },
    codecs.Decode("[0:1][-2:-1]={{a,b},{c,NULL}}", 1009), "explicit array dimensions normalized like JS arrays");
Equal(new object?[] { " a ", "NULL", null }, codecs.Decode(" { \" a \" , \"NULL\" , NULL } ", 1009), "array whitespace outside quotes");
Equal(Array.Empty<object?>(), codecs.Decode("{}", 1009), "empty array decode");
Equal("{}", codecs.Encode(Array.Empty<int>(), 1007), "empty array encode");
Equal("{}", codecs.Encode(new int[2, 0], 1007), "zero-length dimensions");
Equal(new object?[] { "(1,2),(3,4)", "(5,6),(7,8)" }, codecs.Decode("{(1,2),(3,4);(5,6),(7,8)}", 1020), "upstream box delimiter");
Equal("{\"(1,2),(3,4)\";\"(5,6),(7,8)\"}", codecs.Encode(new[] { "(1,2),(3,4)", "(5,6),(7,8)" }, 1020), "box delimiter encode");
var blobs = (object?[])codecs.Decode(codecs.Encode(new[] { new byte[] { 0, 1 }, new byte[] { 255 } }, 1001), 1001)!;
Check(((byte[])blobs[0]!).AsSpan().SequenceEqual(new byte[] { 0, 1 }) && ((byte[])blobs[1]!)[0] == 255, "bytea arrays escape backslashes once per layer");
foreach (string malformed in new[] { "{", "{a,}", "{,a}", "{\"a}", "{a}junk", "{{1},{2,3}}", "{1,{2}}", "[1:2]={1}", "{a\\}" })
    Throws<FormatException>(() => codecs.Decode(malformed, 1009), "reject malformed/ragged array " + malformed);
Throws<FormatException>(() => codecs.Encode(new object?[] { new[] { 1 }, new[] { 2, 3 } }, 1007), "reject ragged input array");
var cycle = new ArrayList(); cycle.Add(cycle);
Throws<ArgumentException>(() => codecs.Encode(cycle, 1009), "array cycle guard");
var custom = PgliteCodecs.Default.Register(1700, text => BigInteger.Parse(text, CultureInfo.InvariantCulture),
    value => ((BigInteger)value).ToString(CultureInfo.InvariantCulture));
Equal(new BigInteger(100), custom.Decode("100", 1700), "upstream custom numeric parser");
Equal("100", custom.Encode(new BigInteger(100), 1700), "upstream custom numeric serializer");
Equal(new object?[] { new BigInteger(100), null, new BigInteger(200) }, custom.Decode("{100,NULL,200}", 1231), "custom codec applies to built-in array elements");
var clone = custom.Clone().Register(1700, text => "changed:" + text);
Equal("changed:100", clone.Decode("100", 1700), "registry clone override");
Equal(new BigInteger(100), custom.Decode("100", 1700), "registry clone isolation");
Equal(new PgliteNumeric("100"), PgliteCodecs.Default.Decode("100", 1700), "default registry isolation");
custom.Register(90001, text => new Mood(text), value => ((Mood)value).Name).RegisterArray(90002, 90001);
Equal(new object?[] { new Mood("sad"), null, new Mood("happy") },
    custom.Decode(custom.Encode(new object?[] { new Mood("sad"), null, new Mood("happy") }, 90002), 90002), "registered enum/custom element array");
Equal("{\"sad\",\"happy\"}", custom.Encode(new PgliteParameter(new[] { new Mood("sad"), new Mood("happy") }, 90002)), "explicit OID wrapper");
var reentrant = PgliteCodecs.Default;
reentrant.Register(90003, text => { reentrant.Decode("{1,2}", 1007); return text; }).RegisterArray(90004, 90003);
Equal(new object?[] { "a", "b" }, reentrant.Decode("{a,b}", 90004), "array parser has no shared reentrant state");
reentrant.Register(90005, text => text.Split('|')).RegisterArray(90006, 90005);
Equal(new object?[] { new object?[] { "a", "b" }, new object?[] { "c" } },
    reentrant.Decode("[1:2]={\"a|b\",c}", 90006), "custom decoder returning arrays does not alter syntactic dimensions");
Equal(1007u, codecs.InferTypeOid(new[] { 1, 2 }), "integer array type inference");
Equal(1009u, codecs.InferTypeOid(new[] { "a", "b" }), "text array type inference");
Equal(17u, codecs.InferTypeOid(new byte[] { 1 }), "bytes infer bytea");
Equal(1001u, codecs.InferTypeOid(new[] { new byte[] { 1 } }), "bytea array inference");
Equal(1700u, codecs.InferTypeOid(BigInteger.One << 100), "large integer infers numeric");
Equal(0u, codecs.InferTypeOid(null), "null leaves server inference");
uint[] types = [23, 25];
var options = new PgliteQueryOptions { ParamTypes = types, Codecs = custom };
types[0] = 999;
Equal(23u, options.ParameterTypes![0], "query options own type list");

Equal("SELECT * FROM test WHERE value = $1;", PgliteTemplate.Sql($"SELECT * FROM test WHERE value = $1;").Text, "upstream plain query");
var simple = PgliteTemplate.Query($"SELECT * FROM test WHERE value = {"foo"} AND num = {3};");
Equal("SELECT * FROM test WHERE value = $1 AND num = $2;", simple.Text, "upstream template values");
Equal(new object?[] { "foo", 3 }, simple.Parameters.ToArray(), "upstream template parameters");
var nullTemplate = PgliteTemplate.Sql($"SELECT {null}, {3}");
Equal(new object?[] { null, 3 }, nullTemplate.Parameters.ToArray(), "upstream template null");
Equal("\"test_5_dance\"", PgliteTemplate.Identifier($"test_{2 + 3}_{"dance"}").Text, "upstream dynamic identifier");
Equal("\"public\".\"a\"\"b\"", PgliteTemplate.Identifier("public", "a\"b").Text, "identifier quote escaping");
Equal("\"a.b\"", PgliteTemplate.Identifier("a.b").Text, "identifier component boundaries");
Throws<ArgumentException>(() => PgliteTemplate.Identifier("a\0b"), "identifier NUL guard");
var filter = PgliteTemplate.Sql($" WHERE {PgliteTemplate.Identifier("foo")} = {"bar"}");
var nested = PgliteTemplate.Sql($"SELECT {42} FROM {PgliteTemplate.Identifier("test")}{filter} AND n={5};");
Equal("SELECT $1 FROM \"test\" WHERE \"foo\" = $2 AND n=$3;", nested.Text, "upstream nested template renumbering");
Equal(new object?[] { 42, "bar", 5 }, nested.Parameters.ToArray(), "nested parameter ordering");
var repeated = PgliteTemplate.Sql($"{filter} OR {filter}");
Equal(" WHERE \"foo\" = $1 OR  WHERE \"foo\" = $2", repeated.Text, "reusing fragment renumbers each occurrence");
var rawTemplate = PgliteTemplate.Sql($"SELECT * FROM {PgliteTemplate.Identifier("test")} {PgliteTemplate.Raw("WHERE value = 'foo'")} AND num = {3};");
Equal("SELECT * FROM \"test\" WHERE value = 'foo' AND num = $1;", rawTemplate.Text, "upstream raw and identifier do not consume parameter numbers");
var joined = PgliteTemplate.Sql($"x IN ({PgliteTemplate.Join(new[] { 1, 2, 3 })}) AND y={4}");
Equal("x IN ($1, $2, $3) AND y=$4", joined.Text, "explicit joined parameters");
Equal(new object?[] { 1, 2, 3, 4 }, joined.Parameters.ToArray(), "join value order");
var separator = PgliteTemplate.Sql($" + {9} + ");
var joinedFragments = PgliteTemplate.Join(new[] { filter, filter }, separator);
Equal(new object?[] { "bar", 9, "bar" }, joinedFragments.Parameters.ToArray(), "parameterized join separator");
Equal("", PgliteTemplate.Join(Array.Empty<object?>()).Text, "empty join");
var arrayArgument = new[] { 1, 2, 3 };
var arrayTemplate = PgliteTemplate.Sql($"SELECT {arrayArgument}");
Check(arrayTemplate.Parameters.Count == 1 && ReferenceEquals(arrayTemplate.Parameters[0], arrayArgument), "array is one parameter");
string attack = "x'; DROP TABLE test; --";
var safe = PgliteTemplate.Sql($"SELECT {attack}");
Equal("SELECT $1", safe.Text, "user data cannot inject text");
Equal(attack, safe.Parameters[0], "user data is preserved for binding");
Equal("SELECT '{x}', $1", PgliteTemplate.Sql($"SELECT '{{x}}', {3}").Text, "escaped interpolation braces");
var literalDollars = PgliteTemplate.Sql($"SELECT '$1', $$ $2 $$, {filter}, {1}");
Equal("SELECT '$1', $$ $2 $$,  WHERE \"foo\" = $1, $2", literalDollars.Text, "literal dollar numbers never text-rewritten");
var outOfOrder = PgliteTemplate.Sql(FormattableStringFactory.Create("{1},{0},{1}", "a", "b"));
Equal(new object?[] { "b", "a", "b" }, outOfOrder.Parameters.ToArray(), "composite argument order and repeated arguments");
Throws<FormatException>(() => PgliteTemplate.Sql($"SELECT {1:000}"), "format specifier rejected");
Throws<FormatException>(() => PgliteTemplate.Sql($"SELECT {1,10}"), "alignment rejected");
Throws<FormatException>(() => PgliteTemplate.Sql(FormattableStringFactory.Create("{9}", 1)), "invalid format argument rejected");
Throws<ArgumentException>(() => PgliteTemplate.Raw("select\0"), "raw NUL guard");
Throws<ArgumentException>(() => PgliteTemplate.Sql($"select\0"), "template literal NUL guard");
Throws<NotSupportedException>(() => ((IList<object?>)simple.Parameters).Add("injected"), "parameter structure read-only");
Console.WriteLine($"API codec/template contracts: {assertions} assertions passed.");

sealed class Dangerous
{
    public override string ToString() => throw new Exception("Arbitrary ToString was called.");
}
sealed record Mood(string Name);
