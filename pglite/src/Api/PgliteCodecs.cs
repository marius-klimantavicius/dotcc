// Manual .NET adaptation of @electric-sql/pglite src/types.ts (Apache-2.0).
// Upstream types.ts credits postgres.js types.js, published under the Unlicense.
// https://github.com/porsager/postgres/blob/master/src/types.js
using System.Collections;
using System.Globalization;
using System.Numerics;
using System.Text;
using System.Text.Json;

namespace Managed.PGlite;

/// <summary>PostgreSQL type OIDs exported by the pinned PGlite types module.</summary>
public static class PgliteTypeOids
{
    public const uint Bool = 16, Bytea = 17, Char = 18, Int8 = 20, Int2 = 21, Int4 = 23,
        Regproc = 24, Text = 25, Oid = 26, Tid = 27, Xid = 28, Cid = 29,
        Json = 114, Xml = 142, PgNodeTree = 194, Smgr = 210,
        Point = 600, Lseg = 601, Path = 602, Box = 603, Polygon = 604, Line = 628,
        Cidr = 650, Float4 = 700, Float8 = 701, Abstime = 702, Reltime = 703,
        Tinterval = 704, Circle = 718, Macaddr8 = 774, Money = 790,
        Macaddr = 829, Inet = 869, Aclitem = 1033, Bpchar = 1042, Varchar = 1043,
        Date = 1082, Time = 1083, Timestamp = 1114, Timestamptz = 1184,
        Interval = 1186, Timetz = 1266, Bit = 1560, Varbit = 1562, Numeric = 1700,
        Refcursor = 1790, Regprocedure = 2202, Regoper = 2203, Regoperator = 2204,
        Regclass = 2205, Regtype = 2206, Uuid = 2950, TxidSnapshot = 2970,
        PgLsn = 3220, PgNdistinct = 3361, PgDependencies = 3402, Tsvector = 3614,
        Tsquery = 3615, Gtsvector = 3642, Regconfig = 3734, Regdictionary = 3769,
        Jsonb = 3802, Regnamespace = 4089, Regrole = 4096;

    public const uint BoolArray = 1000, ByteaArray = 1001, Int2Array = 1005,
        Int4Array = 1007, TextArray = 1009, BpcharArray = 1014, VarcharArray = 1015,
        Int8Array = 1016, BoxArray = 1020, Float4Array = 1021, Float8Array = 1022,
        OidArray = 1028, JsonArray = 199, TimestampArray = 1115, DateArray = 1182,
        TimeArray = 1183, TimestamptzArray = 1185, IntervalArray = 1187,
        NumericArray = 1231, TimetzArray = 1270, UuidArray = 2951, JsonbArray = 3807;
}

/// <summary>Lossless PostgreSQL numeric text, including values beyond decimal's
/// range and NaN/infinities. No floating point conversion occurs when decoding.</summary>
public readonly record struct PgliteNumeric
{
    public string Text { get; }
    public PgliteNumeric(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (!IsValid(text)) throw new FormatException("Invalid PostgreSQL numeric text.");
        Text = text;
    }
    public override string ToString() => Text ?? "0";
    public decimal ToDecimal() => decimal.Parse(ToString(), NumberStyles.Float, CultureInfo.InvariantCulture);
    public BigInteger ToBigInteger() => BigInteger.Parse(ToString(), CultureInfo.InvariantCulture);

    private static bool IsValid(string text)
    {
        if (text is "NaN" or "Infinity" or "+Infinity" or "-Infinity") return true;
        int i = 0, digits = 0;
        if (i < text.Length && text[i] is '+' or '-') i++;
        while (i < text.Length && char.IsAsciiDigit(text[i])) { i++; digits++; }
        if (i < text.Length && text[i] == '.')
        {
            i++;
            while (i < text.Length && char.IsAsciiDigit(text[i])) { i++; digits++; }
        }
        if (digits == 0) return false;
        if (i < text.Length && text[i] is 'e' or 'E')
        {
            i++;
            if (i < text.Length && text[i] is '+' or '-') i++;
            int start = i;
            while (i < text.Length && char.IsAsciiDigit(text[i])) i++;
            if (i == start) return false;
        }
        return i == text.Length;
    }
}

/// <summary>Explicit JSON text for AOT-safe parameter binding. Use JsonElement,
/// JsonDocument or this wrapper instead of reflection-based object serialization.</summary>
public sealed record PgliteJson
{
    public string Text { get; }
    public PgliteJson(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        using var document = JsonDocument.Parse(text);
        Text = text;
    }
    public override string ToString() => Text;
}

/// <summary>A PostgreSQL time with a numeric UTC offset. Offset seconds are
/// retained; unlike DateTimeOffset, offsets greater than 14 hours are supported.</summary>
public readonly record struct PgliteTimeWithTimeZone(TimeOnly Time, TimeSpan Offset)
{
    public override string ToString()
    {
        if (Offset.Ticks % TimeSpan.TicksPerSecond != 0 || Math.Abs(Offset.TotalHours) >= 16)
            throw new ArgumentOutOfRangeException(nameof(Offset), "PostgreSQL time offsets must be whole seconds and less than 16 hours.");
        long seconds = Math.Abs(Offset.Ticks / TimeSpan.TicksPerSecond);
        return Time.ToString("HH:mm:ss.fffffff", CultureInfo.InvariantCulture) +
            (Offset < TimeSpan.Zero ? "-" : "+") + (seconds / 3600).ToString("00", CultureInfo.InvariantCulture) +
            ":" + ((seconds / 60) % 60).ToString("00", CultureInfo.InvariantCulture) +
            (seconds % 60 == 0 ? "" : ":" + (seconds % 60).ToString("00", CultureInfo.InvariantCulture));
    }
}

/// <summary>The three independent components of a PostgreSQL interval. A month
/// is never approximated as a fixed number of days. Supports all IntervalStyle
/// output forms: postgres, postgres_verbose, sql_standard and iso_8601.</summary>
public readonly record struct PgliteInterval(int Months, int Days, long Microseconds)
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;
    public override string ToString() => Months.ToString(Invariant) + " mons " + Days.ToString(Invariant) +
        " days " + (Microseconds / 1_000_000m).ToString("0.######", Invariant) + " secs";

    public static PgliteInterval Parse(string text)
    {
        ArgumentException.ThrowIfNullOrEmpty(text);
        text = text.Trim();
        if (text.StartsWith('P') || text.StartsWith("-P", StringComparison.Ordinal) || text.StartsWith("+P", StringComparison.Ordinal))
            return ParseIso(text);
        bool ago = text.EndsWith(" ago", StringComparison.Ordinal);
        if (ago) text = text[..^4].TrimEnd();
        if (text.StartsWith('@')) text = text[1..].TrimStart();
        var tokens = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int months = 0, days = 0;
        long micros = 0;
        for (int i = 0; i < tokens.Length; i++)
        {
            string token = tokens[i];
            if (token.Contains(':')) { micros = checked(micros + ParseClock(token)); continue; }
            int yearSep = token.IndexOf('-', token.StartsWith('-') || token.StartsWith('+') ? 1 : 0);
            if (yearSep > 0)
            {
                int years = int.Parse(token.AsSpan(0, yearSep), NumberStyles.AllowLeadingSign, Invariant);
                int remaining = int.Parse(token.AsSpan(yearSep + 1), NumberStyles.None, Invariant);
                months = checked(months + years * 12 + (token.StartsWith('-') ? -remaining : remaining));
                continue;
            }
            decimal value = decimal.Parse(token, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, Invariant);
            if (i + 1 < tokens.Length && char.IsAsciiLetter(tokens[i + 1][0]))
            {
                string unit = tokens[++i];
                checked
                {
                    switch (unit)
                    {
                        case "year": case "years": months += ExactInt(value * 12); break;
                        case "mon": case "mons": case "month": case "months": months += ExactInt(value); break;
                        case "week": case "weeks": days += ExactInt(value * 7); break;
                        case "day": case "days": days += ExactInt(value); break;
                        case "hour": case "hours": micros += ExactLong(value * 3_600_000_000m); break;
                        case "min": case "mins": case "minute": case "minutes": micros += ExactLong(value * 60_000_000m); break;
                        case "sec": case "secs": case "second": case "seconds": micros += ExactLong(value * 1_000_000m); break;
                        default: throw new FormatException("Invalid PostgreSQL interval unit.");
                    }
                }
            }
            else days = checked(days + ExactInt(value));
        }
        return ago ? new(checked(-months), checked(-days), checked(-micros)) : new(months, days, micros);
    }

    private static PgliteInterval ParseIso(string text)
    {
        int i = 0, sign = 1, months = 0, days = 0;
        long micros = 0;
        if (text[i] is '-' or '+') { if (text[i] == '-') sign = -1; i++; }
        i++; // P
        bool time = false, component = false;
        while (i < text.Length)
        {
            if (text[i] == 'T') { if (time) throw new FormatException("Invalid ISO interval."); time = true; i++; continue; }
            int start = i;
            if (text[i] is '-' or '+') i++;
            while (i < text.Length && (char.IsAsciiDigit(text[i]) || text[i] == '.')) i++;
            if (i == start || i >= text.Length) throw new FormatException("Invalid ISO interval.");
            decimal value = sign * decimal.Parse(text.AsSpan(start, i - start), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, Invariant);
            char unit = text[i++];
            component = true;
            checked
            {
                switch (unit)
                {
                    case 'Y' when !time: months += ExactInt(value * 12); break;
                    case 'M' when !time: months += ExactInt(value); break;
                    case 'W' when !time: days += ExactInt(value * 7); break;
                    case 'D' when !time: days += ExactInt(value); break;
                    case 'H' when time: micros += ExactLong(value * 3_600_000_000m); break;
                    case 'M' when time: micros += ExactLong(value * 60_000_000m); break;
                    case 'S' when time: micros += ExactLong(value * 1_000_000m); break;
                    default: throw new FormatException("Invalid ISO interval unit.");
                }
            }
        }
        if (!component) throw new FormatException("Empty ISO interval.");
        return new(months, days, micros);
    }

    private static long ParseClock(string text)
    {
        int sign = 1;
        if (text.StartsWith('-') || text.StartsWith('+')) { if (text[0] == '-') sign = -1; text = text[1..]; }
        var parts = text.Split(':');
        if (parts.Length != 3) throw new FormatException("Invalid interval clock.");
        long hours = long.Parse(parts[0], NumberStyles.None, Invariant);
        int minutes = int.Parse(parts[1], NumberStyles.None, Invariant);
        decimal seconds = decimal.Parse(parts[2], NumberStyles.AllowDecimalPoint, Invariant);
        if (minutes >= 60 || seconds >= 60 || seconds < 0) throw new FormatException("Invalid interval clock.");
        return checked(sign * ExactLong(hours * 3_600_000_000m + minutes * 60_000_000m + seconds * 1_000_000m));
    }

    private static int ExactInt(decimal value) => checked((int)ExactLong(value));
    private static long ExactLong(decimal value)
    {
        if (decimal.Truncate(value) != value) throw new FormatException("Interval precision exceeds PostgreSQL's representation.");
        return checked((long)value);
    }
}

/// <summary>A text-format PostgreSQL codec registry. Default returns a fresh
/// registry; registrations and clones cannot mutate another session's defaults.
/// Custom delegates must be AOT-compatible and are never called for null.</summary>
public sealed class PgliteCodecs
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;
    private const int MaximumNesting = 64;
    private readonly record struct Codec(Func<string, object?>? Decode, Func<object, string>? Encode);
    private readonly record struct ArrayType(uint ElementOid, char Delimiter);
    private readonly Dictionary<uint, Codec> codecs;
    private readonly Dictionary<uint, ArrayType> arrays;

    public static PgliteCodecs Default => new();

    public PgliteCodecs()
    {
        codecs = new();
        arrays = new();
        // Built-in pg_type typarray mappings. User-defined arrays are registered
        // with RegisterArray, normally after querying the session's pg_type.
        (uint element, uint array)[] builtins =
        [
            (16,1000),(17,1001),(18,1002),(19,1003),(20,1016),(21,1005),(22,1006),
            (23,1007),(24,1008),(25,1009),(26,1028),(27,1010),(28,1011),(29,1012),(30,1013),
            (114,199),(142,143),(600,1017),(601,1018),(602,1019),(603,1020),(604,1027),
            (628,629),(650,651),(700,1021),(701,1022),(718,719),(774,775),(790,791),
            (829,1040),(869,1041),(1033,1034),(1042,1014),(1043,1015),(1082,1182),
            (1083,1183),(1114,1115),(1184,1185),(1186,1187),(1266,1270),(1560,1561),
            (1562,1563),(1700,1231),(1790,2201),(2202,2207),(2203,2208),(2204,2209),
            (2205,2210),(2206,2211),(2950,2951),(2970,2949),(3220,3221),(3614,3643),
            (3615,3645),(3642,3644),(3734,3735),(3769,3770),(3802,3807),(4089,4090),
            (4096,4097)
        ];
        foreach (var (element, array) in builtins) arrays.Add(array, new(element, array == 1020 ? ';' : ','));
    }

    private PgliteCodecs(PgliteCodecs source)
    { codecs = new(source.codecs); arrays = new(source.arrays); }

    public PgliteCodecs Clone() => new(this);

    public PgliteCodecs Register(uint oid, Func<string, object?> decode, Func<object, string>? encode = null)
    {
        ArgumentNullException.ThrowIfNull(decode);
        codecs.TryGetValue(oid, out var previous);
        codecs[oid] = new(decode, encode ?? previous.Encode);
        return this;
    }

    public PgliteCodecs RegisterParser(uint oid, Func<string, object?> decode) => Register(oid, decode);

    public PgliteCodecs RegisterSerializer(uint oid, Func<object, string> encode)
    {
        ArgumentNullException.ThrowIfNull(encode);
        codecs.TryGetValue(oid, out var previous);
        codecs[oid] = new(previous.Decode, encode);
        return this;
    }

    public PgliteCodecs RegisterArray(uint arrayOid, uint elementOid, char delimiter = ',')
    {
        if (arrayOid == 0 || elementOid == 0 || arrayOid == elementOid)
            throw new ArgumentException("Array and element OIDs must be distinct nonzero PostgreSQL OIDs.");
        if (char.IsWhiteSpace(delimiter) || char.IsControl(delimiter) || delimiter is '{' or '}' or '"' or '\\')
            throw new ArgumentException("Invalid PostgreSQL array delimiter.", nameof(delimiter));
        arrays[arrayOid] = new(elementOid, delimiter);
        return this;
    }

    public string? Encode(object? value, uint oid = 0)
    {
        int wrappers = 0;
        while (value is PgliteParameter parameter)
        {
            if (++wrappers > MaximumNesting) throw new ArgumentException("Parameter nesting is too deep.");
            if (parameter.TypeOid != 0) oid = parameter.TypeOid;
            value = parameter.Value;
        }
        if (value is null || value is DBNull) return null;
        if (oid == 0) oid = InferTypeOid(value);
        string encoded;
        if (codecs.TryGetValue(oid, out var custom) && custom.Encode is not null) encoded = custom.Encode(value);
        else if (arrays.TryGetValue(oid, out var array))
            encoded = value is string text ? text : EncodeArray(value, array);
        else encoded = oid switch
        {
            PgliteTypeOids.Bool => EncodeBoolean(value),
            PgliteTypeOids.Bytea => EncodeBytea(value),
            PgliteTypeOids.Json or PgliteTypeOids.Jsonb => EncodeJson(value),
            PgliteTypeOids.Date or PgliteTypeOids.Timestamp or PgliteTypeOids.Timestamptz => EncodeDate(value, oid),
            PgliteTypeOids.Time => value is TimeOnly time ? time.ToString("HH:mm:ss.fffffff", Invariant) : RequireString(value, "time"),
            PgliteTypeOids.Timetz => value is PgliteTimeWithTimeZone timetz ? timetz.ToString() : RequireString(value, "timetz"),
            PgliteTypeOids.Interval => value is PgliteInterval interval ? interval.ToString() : value is TimeSpan span
                ? new PgliteInterval(0, 0, (long)decimal.Round(span.Ticks / 10m, 0, MidpointRounding.ToEven)).ToString()
                : RequireString(value, "interval"),
            _ => EncodeScalar(value)
        };
        return ValidateText(encoded);
    }

    public object? Decode(string? value, uint oid)
    {
        if (value is null) return null;
        if (codecs.TryGetValue(oid, out var custom) && custom.Decode is not null) return custom.Decode(value);
        if (arrays.TryGetValue(oid, out var array)) return new ArrayReader(this, value, array).Read();
        return oid switch
        {
            PgliteTypeOids.Bool => DecodeBoolean(value),
            PgliteTypeOids.Int2 => short.Parse(value, NumberStyles.Integer, Invariant),
            PgliteTypeOids.Int4 => int.Parse(value, NumberStyles.Integer, Invariant),
            PgliteTypeOids.Oid => uint.Parse(value, NumberStyles.Integer, Invariant),
            PgliteTypeOids.Int8 => DecodeBigInteger(value),
            PgliteTypeOids.Float4 => float.Parse(value, NumberStyles.Float, Invariant),
            PgliteTypeOids.Float8 => double.Parse(value, NumberStyles.Float, Invariant),
            PgliteTypeOids.Numeric => new PgliteNumeric(value),
            PgliteTypeOids.Bytea => DecodeBytea(value),
            PgliteTypeOids.Json or PgliteTypeOids.Jsonb => DecodeJson(value),
            PgliteTypeOids.Date => DateOnly.TryParseExact(value, "yyyy-MM-dd", Invariant, DateTimeStyles.None, out var date) ? date : value,
            PgliteTypeOids.Timestamp => DecodeTimestamp(value),
            PgliteTypeOids.Timestamptz => DecodeTimestampTz(value),
            PgliteTypeOids.Time => TimeOnly.TryParseExact(value, ["HH:mm:ss", "HH:mm:ss.FFFFFFF"], Invariant, DateTimeStyles.None, out var time) ? time : value,
            PgliteTypeOids.Timetz => DecodeTimeTz(value),
            PgliteTypeOids.Interval => value is "infinity" or "-infinity" ? value : PgliteInterval.Parse(value),
            _ => value
        };
    }

    public uint InferTypeOid(object? value)
    {
        if (value is PgliteParameter parameter) return parameter.TypeOid != 0 ? parameter.TypeOid : InferTypeOid(parameter.Value);
        if (value is null or DBNull) return 0;
        if (IsArray(value))
        {
            var normalized = NormalizeArray(value, new(ReferenceEqualityComparer.Instance), 0);
            uint element = InferElement(normalized);
            foreach (var pair in arrays) if (pair.Value.ElementOid == element) return pair.Key;
            return 0;
        }
        return value switch
        {
            bool => PgliteTypeOids.Bool,
            sbyte or byte or short => PgliteTypeOids.Int2,
            ushort or int => PgliteTypeOids.Int4,
            uint => PgliteTypeOids.Oid,
            long => PgliteTypeOids.Int8,
            ulong number => number <= long.MaxValue ? PgliteTypeOids.Int8 : PgliteTypeOids.Numeric,
            BigInteger number => number >= long.MinValue && number <= long.MaxValue ? PgliteTypeOids.Int8 : PgliteTypeOids.Numeric,
            float => PgliteTypeOids.Float4,
            double => PgliteTypeOids.Float8,
            decimal or PgliteNumeric => PgliteTypeOids.Numeric,
            string or char => PgliteTypeOids.Text,
            Guid => PgliteTypeOids.Uuid,
            DateOnly => PgliteTypeOids.Date,
            DateTime date => date.Kind == DateTimeKind.Unspecified ? PgliteTypeOids.Timestamp : PgliteTypeOids.Timestamptz,
            DateTimeOffset => PgliteTypeOids.Timestamptz,
            TimeOnly => PgliteTypeOids.Time,
            PgliteTimeWithTimeZone => PgliteTypeOids.Timetz,
            PgliteInterval or TimeSpan => PgliteTypeOids.Interval,
            byte[] or Memory<byte> or ReadOnlyMemory<byte> or ArraySegment<byte> => PgliteTypeOids.Bytea,
            JsonElement or JsonDocument or PgliteJson or IDictionary => PgliteTypeOids.Json,
            _ => 0
        };
    }

    private static string ValidateText(string? text)
    {
        if (text is null) throw new InvalidOperationException("A non-null value's serializer returned null.");
        if (text.Contains('\0')) throw new ArgumentException("PostgreSQL text parameters cannot contain NUL.");
        return text;
    }

    private static string RequireString(object value, string type) => value is string text ? text :
        throw new ArgumentException("Invalid parameter value for PostgreSQL " + type + ". Supply the corresponding managed value or explicit PostgreSQL text.");

    private static string EncodeScalar(object value) => value switch
    {
        string text => text,
        char c => c.ToString(),
        bool b => b ? "true" : "false",
        byte n => n.ToString(Invariant), sbyte n => n.ToString(Invariant),
        short n => n.ToString(Invariant), ushort n => n.ToString(Invariant),
        int n => n.ToString(Invariant), uint n => n.ToString(Invariant),
        long n => n.ToString(Invariant), ulong n => n.ToString(Invariant),
        BigInteger n => n.ToString(Invariant), decimal n => n.ToString(Invariant),
        float n => n.ToString("R", Invariant), double n => n.ToString("R", Invariant),
        PgliteNumeric n => n.ToString(), Guid guid => guid.ToString("D"),
        DateTime date => date.ToString("O", Invariant), DateTimeOffset date => date.ToString("O", Invariant),
        DateOnly date => date.ToString("yyyy-MM-dd", Invariant), TimeOnly time => time.ToString("HH:mm:ss.fffffff", Invariant),
        PgliteInterval interval => interval.ToString(), PgliteTimeWithTimeZone time => time.ToString(),
        _ => throw new ArgumentException("No PostgreSQL serializer is registered for this value. Register a codec with an explicit OID; use JsonElement, JsonDocument or PgliteJson for JSON.")
    };

    private static string EncodeBoolean(object value)
    {
        if (value is bool boolean) return boolean ? "t" : "f";
        if (value is string text)
        {
            return text.Trim().ToLowerInvariant() switch
            {
                "true" or "t" or "yes" or "y" or "on" or "1" => "t",
                "false" or "f" or "no" or "n" or "off" or "0" => "f",
                _ => throw new ArgumentException("Invalid input for boolean type.")
            };
        }
        if (value is sbyte or byte or short or ushort or int or uint or long or ulong or float or double or decimal or BigInteger)
        {
            string number = EncodeScalar(value);
            if (decimal.TryParse(number, NumberStyles.Float, Invariant, out decimal numeric))
            {
                if (numeric == 1) return "t";
                if (numeric == 0) return "f";
            }
        }
        throw new ArgumentException("Invalid input for boolean type.");
    }

    private static bool DecodeBoolean(string text) => text switch
    { "t" => true, "f" => false, _ => throw new FormatException("Invalid PostgreSQL boolean output.") };

    private static object DecodeBigInteger(string text)
    {
        if (long.TryParse(text, NumberStyles.Integer, Invariant, out long number)) return number;
        return BigInteger.Parse(text, NumberStyles.Integer, Invariant);
    }

    private static string EncodeDate(object value, uint oid)
    {
        if (value is string text) return text;
        if (value is DateOnly date) return date.ToString("yyyy-MM-dd", Invariant);
        if (value is DateTime timestamp) return timestamp.ToString("O", Invariant);
        if (value is DateTimeOffset timestampTz) return timestampTz.ToString("O", Invariant);
        if (value is sbyte or byte or short or ushort or int or uint or long or ulong or float or double or decimal)
        {
            decimal milliseconds = decimal.Parse(EncodeScalar(value), NumberStyles.Float, Invariant);
            var instant = DateTimeOffset.UnixEpoch.AddTicks(checked((long)(milliseconds * TimeSpan.TicksPerMillisecond)));
            return instant.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", Invariant);
        }
        throw new ArgumentException("Invalid input for date type.");
    }

    private static object DecodeTimestamp(string value)
    {
        return DateTime.TryParseExact(value.Replace(' ', 'T'), ["yyyy-MM-dd'T'HH:mm:ss", "yyyy-MM-dd'T'HH:mm:ss.FFFFFFF"],
            Invariant, DateTimeStyles.None, out var timestamp) ? DateTime.SpecifyKind(timestamp, DateTimeKind.Unspecified) : value;
    }

    private static object DecodeTimestampTz(string value)
    {
        if (value.EndsWith(" BC", StringComparison.Ordinal)) return value;
        return DateTimeOffset.TryParse(value, Invariant, DateTimeStyles.AssumeUniversal, out var timestamp) ? timestamp : value;
    }

    private static object DecodeTimeTz(string value)
    {
        int sign = value.IndexOfAny(['+', '-']);
        if (sign < 0 || !TimeOnly.TryParseExact(value[..sign], ["HH:mm:ss", "HH:mm:ss.FFFFFFF"], Invariant, DateTimeStyles.None, out var time)) return value;
        var offset = value[(sign + 1)..].Split(':');
        if (offset.Length > 3 || !int.TryParse(offset[0], NumberStyles.None, Invariant, out int hours)) return value;
        int minutes = 0, seconds = 0;
        if (offset.Length > 1 && !int.TryParse(offset[1], NumberStyles.None, Invariant, out minutes)) return value;
        if (offset.Length > 2 && !int.TryParse(offset[2], NumberStyles.None, Invariant, out seconds)) return value;
        if (hours >= 16 || minutes >= 60 || seconds >= 60) return value;
        return new PgliteTimeWithTimeZone(time, TimeSpan.FromSeconds((value[sign] == '-' ? -1 : 1) * (hours * 3600 + minutes * 60 + seconds)));
    }

    private static string EncodeBytea(object value)
    {
        ReadOnlySpan<byte> bytes = value switch
        {
            byte[] array => array,
            Memory<byte> memory => memory.Span,
            ReadOnlyMemory<byte> memory => memory.Span,
            ArraySegment<byte> segment => segment.AsSpan(),
            _ => throw new ArgumentException("Invalid input for bytea type; supply bytes.")
        };
        return "\\x" + Convert.ToHexStringLower(bytes);
    }

    private static byte[] DecodeBytea(string value)
    {
        if (value.StartsWith("\\x", StringComparison.Ordinal)) return Convert.FromHexString(value.AsSpan(2));
        var bytes = new List<byte>();
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            if (c == '\\')
            {
                if (++i >= value.Length) throw new FormatException("Incomplete bytea escape.");
                if (value[i] == '\\') { bytes.Add((byte)'\\'); continue; }
                if (i + 2 >= value.Length || value[i] is < '0' or > '3' || value[i + 1] is < '0' or > '7' || value[i + 2] is < '0' or > '7')
                    throw new FormatException("Invalid bytea octal escape.");
                bytes.Add((byte)((value[i] - '0') * 64 + (value[i + 1] - '0') * 8 + value[i + 2] - '0'));
                i += 2;
            }
            else
            {
                if (c > 255) throw new FormatException("Invalid bytea character.");
                bytes.Add((byte)c);
            }
        }
        return bytes.ToArray();
    }

    private static JsonElement DecodeJson(string value)
    {
        using var document = JsonDocument.Parse(value);
        return document.RootElement.Clone();
    }

    private static string EncodeJson(object value)
    {
        if (value is string text) return text; // Upstream explicit JSON text.
        if (value is PgliteJson json) return json.Text;
        if (value is JsonElement element) return element.GetRawText();
        if (value is JsonDocument document) return document.RootElement.GetRawText();
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
            WriteJson(writer, value, new(ReferenceEqualityComparer.Instance), 0);
        return Encoding.UTF8.GetString(stream.GetBuffer(), 0, checked((int)stream.Length));
    }

    private static void WriteJson(Utf8JsonWriter writer, object? value, HashSet<object> active, int depth)
    {
        if (depth > MaximumNesting) throw new ArgumentException("JSON nesting is too deep.");
        switch (value)
        {
            case null: case DBNull: writer.WriteNullValue(); return;
            case JsonElement element: element.WriteTo(writer); return;
            case JsonDocument document: document.RootElement.WriteTo(writer); return;
            case PgliteJson json: using (var document = JsonDocument.Parse(json.Text)) document.RootElement.WriteTo(writer); return;
            case string text: writer.WriteStringValue(text); return;
            case char c: writer.WriteStringValue(c.ToString()); return;
            case bool boolean: writer.WriteBooleanValue(boolean); return;
            case byte n: writer.WriteNumberValue(n); return;
            case sbyte n: writer.WriteNumberValue(n); return;
            case short n: writer.WriteNumberValue(n); return;
            case ushort n: writer.WriteNumberValue(n); return;
            case int n: writer.WriteNumberValue(n); return;
            case uint n: writer.WriteNumberValue(n); return;
            case long n: writer.WriteNumberValue(n); return;
            case ulong n: writer.WriteNumberValue(n); return;
            case decimal n: writer.WriteNumberValue(n); return;
            case float n when float.IsFinite(n): writer.WriteNumberValue(n); return;
            case double n when double.IsFinite(n): writer.WriteNumberValue(n); return;
            case float: case double: writer.WriteNullValue(); return; // JSON.stringify NaN/infinity.
            case BigInteger number: writer.WriteStringValue(number.ToString(Invariant)); return; // Upstream bigint replacer.
            case DateTime date: writer.WriteStringValue(date.ToString("O", Invariant)); return;
            case DateTimeOffset date: writer.WriteStringValue(date.ToString("O", Invariant)); return;
            case IDictionary dictionary:
                if (!active.Add(value)) throw new ArgumentException("JSON contains a cycle.");
                try
                {
                    writer.WriteStartObject();
                    foreach (DictionaryEntry entry in dictionary)
                    {
                        if (entry.Key is not string key) throw new ArgumentException("JSON object keys must be strings.");
                        writer.WritePropertyName(key);
                        WriteJson(writer, entry.Value, active, depth + 1);
                    }
                    writer.WriteEndObject();
                }
                finally { active.Remove(value); }
                return;
            default:
                if (IsArray(value))
                {
                    if (!active.Add(value)) throw new ArgumentException("JSON contains a cycle.");
                    try
                    {
                        if (value is Array array && array.Rank != 1) throw new ArgumentException("Use nested lists for multidimensional JSON arrays.");
                        writer.WriteStartArray();
                        foreach (object? item in (IEnumerable)value) WriteJson(writer, item, active, depth + 1);
                        writer.WriteEndArray();
                    }
                    finally { active.Remove(value); }
                    return;
                }
                throw new ArgumentException("Unsupported JSON object. Supply JsonElement, JsonDocument, PgliteJson or a tree of dictionaries, lists and scalar values.");
        }
    }

    private static bool IsArray(object? value) => value is Array and not byte[] || value is IList and not byte[];

    private string EncodeArray(object value, ArrayType type)
    {
        if (!IsArray(value)) throw new ArgumentException("PostgreSQL array parameters require an array, list or explicit array text.");
        var normalized = NormalizeArray(value, new(ReferenceEqualityComparer.Instance), 0);
        int[] shape = Shape(normalized, 0);
        if (shape.Contains(0)) return "{}";
        var text = new StringBuilder();
        AppendArray(text, normalized, type);
        return text.ToString();
    }

    private void AppendArray(StringBuilder text, object?[] values, ArrayType type)
    {
        text.Append('{');
        for (int i = 0; i < values.Length; i++)
        {
            if (i > 0) text.Append(type.Delimiter);
            if (values[i] is object?[] nested) { AppendArray(text, nested, type); continue; }
            string? encoded = Encode(values[i], type.ElementOid);
            if (encoded is null) text.Append("NULL");
            else text.Append('"').Append(encoded.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)).Append('"');
        }
        text.Append('}');
    }

    private static object?[] NormalizeArray(object value, HashSet<object> active, int depth)
    {
        if (depth >= MaximumNesting) throw new ArgumentException("Array nesting is too deep.");
        if (!active.Add(value)) throw new ArgumentException("Array contains a cycle.");
        try
        {
            if (value is Array array && array.Rank > 1)
            {
                if (depth + array.Rank > MaximumNesting) throw new ArgumentException("Array nesting is too deep.");
                return ExpandRectangular(array, new int[array.Rank], 0, active, depth);
            }
            var items = new List<object?>();
            foreach (object? item in (IEnumerable)value)
                items.Add(IsArray(item) ? NormalizeArray(item!, active, depth + 1) : item);
            return items.ToArray();
        }
        finally { active.Remove(value); }
    }

    private static object?[] ExpandRectangular(Array array, int[] indices, int dimension, HashSet<object> active, int depth)
    {
        var result = new object?[array.GetLength(dimension)];
        for (int i = 0; i < result.Length; i++)
        {
            indices[dimension] = i + array.GetLowerBound(dimension);
            if (dimension + 1 < array.Rank) result[i] = ExpandRectangular(array, indices, dimension + 1, active, depth);
            else
            {
                object? item = array.GetValue(indices);
                result[i] = IsArray(item) ? NormalizeArray(item!, active, depth + array.Rank) : item;
            }
        }
        return result;
    }

    private static int[] Shape(object?[] values, int depth)
    {
        if (depth >= MaximumNesting) throw new FormatException("Array nesting is too deep.");
        int[]? child = null;
        foreach (object? item in values)
        {
            int[] shape = item is object?[] nested ? Shape(nested, depth + 1) : [];
            if (child is not null && !child.AsSpan().SequenceEqual(shape)) throw new FormatException("PostgreSQL arrays must be rectangular and cannot mix scalar and nested elements.");
            child = shape;
        }
        return [values.Length, .. child ?? []];
    }

    private uint InferElement(object?[] values)
    {
        uint result = 0;
        foreach (object? value in values)
        {
            uint next = value is object?[] nested ? InferElement(nested) : InferTypeOid(value);
            if (value is null or DBNull || next == 0 && value is object?[]) continue;
            if (next == 0) return 0;
            if (result == 0) result = next;
            else if (result != next)
            {
                if (IsNumberOid(result) && IsNumberOid(next)) result = result is 700 or 701 || next is 700 or 701 ? 701u : 1700u;
                else return 0;
            }
        }
        return result;
    }

    private static bool IsNumberOid(uint oid) => oid is 20 or 21 or 23 or 26 or 700 or 701 or 1700;

    private sealed class ArrayReader(PgliteCodecs owner, string text, ArrayType type)
    {
        private int position;

        internal object?[] Read()
        {
            SkipSpace();
            var dimensions = new List<int>();
            while (Peek == '[')
            {
                position++;
                int lower = ReadInteger();
                Expect(':');
                int upper = ReadInteger();
                Expect(']');
                long length = (long)upper - lower + 1;
                if (length < 0 || length > int.MaxValue) throw Invalid();
                dimensions.Add((int)length);
                if (dimensions.Count > MaximumNesting) throw Invalid();
                SkipSpace();
            }
            if (dimensions.Count != 0) Expect('=');
            object?[] values = ReadArray(0, out int[] shape);
            SkipSpace();
            if (position != text.Length) throw Invalid();
            if (dimensions.Count != 0 && !dimensions.ToArray().AsSpan().SequenceEqual(shape)) throw Invalid();
            return values;
        }

        private object?[] ReadArray(int depth, out int[] shape)
        {
            if (depth >= MaximumNesting) throw Invalid();
            SkipSpace();
            Expect('{');
            SkipSpace();
            var values = new List<object?>();
            if (Peek == '}') { position++; shape = [0]; return []; }
            int[]? childShape = null;
            while (true)
            {
                SkipSpace();
                int[] nextShape;
                if (Peek == '{') values.Add(ReadArray(depth + 1, out nextShape));
                else { values.Add(ReadElement()); nextShape = []; }
                // Track syntactic dimensions separately: a custom element
                // decoder may itself return an array or any other CLR object.
                if (childShape is not null && !childShape.AsSpan().SequenceEqual(nextShape)) throw Invalid();
                childShape = nextShape;
                SkipSpace();
                if (Peek == '}') { position++; shape = [values.Count, .. childShape]; return values.ToArray(); }
                Expect(type.Delimiter);
            }
        }

        private object? ReadElement()
        {
            var value = new StringBuilder();
            bool quoted = Peek == '"', escaped = false;
            if (quoted)
            {
                position++;
                bool closed = false;
                while (position < text.Length)
                {
                    char c = text[position++];
                    if (c == '"') { closed = true; break; }
                    if (c == '\\') { if (position >= text.Length) throw Invalid(); c = text[position++]; }
                    value.Append(c);
                }
                if (!closed) throw Invalid();
                SkipSpace();
                if (Peek != type.Delimiter && Peek != '}') throw Invalid();
            }
            else
            {
                int meaningfulLength = 0;
                while (position < text.Length && Peek != type.Delimiter && Peek != '}')
                {
                    char c = text[position++];
                    bool thisEscaped = c == '\\';
                    if (thisEscaped)
                    {
                        if (position >= text.Length) throw Invalid();
                        c = text[position++];
                        escaped = true;
                    }
                    else if (c is '{' or '"') throw Invalid();
                    value.Append(c);
                    if (thisEscaped || !char.IsWhiteSpace(c)) meaningfulLength = value.Length;
                }
                value.Length = meaningfulLength;
                if (value.Length == 0) throw Invalid();
            }
            string item = value.ToString();
            return !quoted && !escaped && item.Equals("NULL", StringComparison.OrdinalIgnoreCase) ? null : owner.Decode(item, type.ElementOid);
        }

        private int ReadInteger()
        {
            int start = position;
            if (Peek is '+' or '-') position++;
            while (position < text.Length && char.IsAsciiDigit(Peek)) position++;
            if (!int.TryParse(text.AsSpan(start, position - start), NumberStyles.AllowLeadingSign, Invariant, out int value)) throw Invalid();
            return value;
        }

        private char Peek => position < text.Length ? text[position] : '\0';
        private void SkipSpace() { while (position < text.Length && char.IsWhiteSpace(text[position])) position++; }
        private void Expect(char c) { if (Peek != c) throw Invalid(); position++; }
        private static FormatException Invalid() => new("Malformed PostgreSQL array text.");
    }
}
