using System.Globalization;
using System.Numerics;

namespace DotObj;

internal static class ScalarTypes
{
    public static bool IsRecord(string kind) => kind.StartsWith("record(", StringComparison.Ordinal) && kind.EndsWith(')');
    public static bool IsScalar(string kind) => IsNumeric(kind) || IsPointer(kind) || kind == "bool";
    public static bool IsArray(string kind) => kind.StartsWith("array(", StringComparison.Ordinal) && kind.EndsWith(')');
    public static long? ArrayBound(string kind) => kind.AsSpan(6, kind.IndexOf(',') - 6).SequenceEqual("?")
        ? null : long.Parse(kind.AsSpan(6, kind.IndexOf(',') - 6), CultureInfo.InvariantCulture);
    public static long ElementCount(string kind) => ArrayBound(kind) ?? 0;
    public static bool IsIncompleteArray(string kind) => IsArray(kind) && ArrayBound(kind) is null;
    public static bool IsAddressOnlyArray(string kind) => IsArray(kind) && FlatCount(kind) == 0;
    public static bool IsZeroArray(string kind) => IsAddressOnlyArray(kind) && !IsIncompleteArray(kind);
    public static bool IsZeroStridePointer(string kind) => IsPointer(kind) && IsZeroArray(Pointee(kind));
    public static string Element(string kind) => kind[(kind.IndexOf(',') + 1)..^1];
    public static string Leaf(string kind) => IsArray(kind) ? Leaf(Element(kind)) : kind;
    public static long FlatCount(string kind) => IsArray(kind) ? checked(ElementCount(kind) * FlatCount(Element(kind))) : 1;
    public static long Size(string kind) => IsArray(kind) ? checked(ElementCount(kind) * Size(Element(kind))) : IsPointer(kind) ? 8 : kind switch
    { "f80" or "i128" or "u128" => 16, "va-list" => 24, "bool" or "i8" or "u8" => 1, "i16" or "u16" => 2, "i32" or "u32" or "f32" => 4, "i64" or "u64" or "f64" => 8, _ => throw new InvalidDataException("Type has no storage size") };
    public static string ArrayName(string kind) => "__DotObjArray_" + Mangle(kind);
    private static string Mangle(string kind) => IsArray(kind) ? (IsIncompleteArray(kind) ? "I" : "A" + ElementCount(kind)) + "_" + Mangle(Element(kind)) : IsPointer(kind) ? "P_" + Mangle(Pointee(kind)) : kind;
    public static bool IsPointer(string kind) => kind.StartsWith("ptr(", StringComparison.Ordinal) && kind.EndsWith(')');
    public static string Pointee(string kind) => IsPointer(kind) ? kind[4..^1]
        : throw new InvalidDataException("Expected pointer type");
    public static bool IsWideInteger(string kind) => kind is "i128" or "u128";
    public static bool IsInteger(string kind) => kind is "i8" or "u8" or "i16" or "u16" or "i32" or "u32" or "i64" or "u64" or "i128" or "u128";
    public static bool IsFloating(string kind) => kind is "f32" or "f64" or "f80";
    public static bool IsNumeric(string kind) => IsInteger(kind) || IsFloating(kind);
    public static bool IsPromotedNumeric(string kind) => IsPromotedInteger(kind) || IsFloating(kind);
    public static string Zero(string kind) => kind switch { "f32" => "00000000", "f64" => "0000000000000000", "f80" => "00000000000000000000", _ => "0" };
    public static bool IsPromotedInteger(string kind) => kind is "i32" or "u32" or "i64" or "u64" or "i128" or "u128";
    public static bool IsBooleanResult(string kind) => kind is "bool" or "i32" or "u8";
    public static string CSharp(string kind) => IsArray(kind) ? ArrayName(kind) : IsPointer(kind) ? CSharp(Pointee(kind)) + "*" : kind switch
    {
        "f80" => "global::Edg.Runtime.Float80", "va-list" => "global::Edg.Runtime.VaList", "void" => "void", "bool" => "bool", "i8" => "sbyte", "u8" => "byte",
        "i16" => "short", "u16" => "ushort", "i32" => "int", "u32" => "uint",
        "i64" => "long", "u64" => "ulong", "i128" => "global::System.Int128", "u128" => "global::System.UInt128", "f32" => "float", "f64" => "double", _ => throw new InvalidDataException("Unsupported C# type")
    };

    // Decimal text is exact even where the unsigned value does not fit a SQLite
    // signed INTEGER. Reject whitespace, plus signs and non-canonical spellings.
    public static bool IsConstant(string kind, string? text)
    {
        if (IsPointer(kind)) return text == "0";
        if (IsFloating(kind)) return text is not null && text.Length == (kind == "f32" ? 8 : kind == "f80" ? 20 : 16) &&
            text.All(ch => ch is >= '0' and <= '9' or >= 'a' and <= 'f');
        if (IsWideInteger(kind))
        {
            if (text is null || text.Length == 0 || text.Length > 40) return false;
            return kind == "i128"
                ? Int128.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out Int128 signed128) && text == signed128.ToString(CultureInfo.InvariantCulture)
                : UInt128.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out UInt128 unsigned128) && text == unsigned128.ToString(CultureInfo.InvariantCulture);
        }
        if (text is null || text.Length == 0 || text.Length > 20) return false;
        if (kind is "bool" or "u8" or "u16" or "u32" or "u64")
        {
            if (!ulong.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out ulong value) ||
                text != value.ToString(CultureInfo.InvariantCulture)) return false;
            return kind switch { "bool" => value <= 1, "u8" => value <= byte.MaxValue,
                "u16" => value <= ushort.MaxValue, "u32" => value <= uint.MaxValue, _ => true };
        }
        if (!long.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out long signed) ||
            text != signed.ToString(CultureInfo.InvariantCulture)) return false;
        return kind switch { "i8" => signed is >= sbyte.MinValue and <= sbyte.MaxValue,
            "i16" => signed is >= short.MinValue and <= short.MaxValue,
            "i32" => signed is >= int.MinValue and <= int.MaxValue, "i64" => true, _ => false };
    }

    // Switch dispatch compares case values after conversion to the promoted
    // selector type. Keep the exact converted value for validation and C# labels.
    public static string SwitchConstant(string sourceKind, string text, string selectorKind)
    {
        if (!IsInteger(sourceKind) || !IsConstant(sourceKind, text) || (!IsPromotedInteger(selectorKind) || IsWideInteger(selectorKind)))
            throw new InvalidDataException("Invalid switch case conversion");
        int width = selectorKind is "i32" or "u32" ? 32 : 64;
        BigInteger modulus = BigInteger.One << width;
        BigInteger value = BigInteger.Parse(text, CultureInfo.InvariantCulture) % modulus;
        if (value.Sign < 0) value += modulus;
        if (selectorKind is "i32" or "i64" && value >= (modulus >> 1)) value -= modulus;
        return value.ToString(CultureInfo.InvariantCulture);
    }

    public static bool IsFiniteConstant(string kind, string text) => kind switch
    {
        "f32" => (uint.Parse(text, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture) & 0x7f800000U) != 0x7f800000U,
        "f64" => (ulong.Parse(text, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture) & 0x7ff0000000000000UL) != 0x7ff0000000000000UL,
        _ => true
    };

    private static string FloatingLiteral(string kind, string text)
    {
        if (kind == "f80") return "new global::Edg.Runtime.Float80(0x" + text[4..] + "UL, 0x" + text[..4] + ")";
        // Exact target bits survive SQLite without JSON number rounding. Finite
        // constants use round-trip decimal C# spelling; nonfinite constants keep
        // their payload/sign through BitConverter, including noncanonical NaNs.
        if (!IsFiniteConstant(kind, text))
            return kind == "f32"
                ? "global::System.BitConverter.Int32BitsToSingle(unchecked((int)0x" + text + "U))"
                : "global::System.BitConverter.Int64BitsToDouble(unchecked((long)0x" + text + "UL))";
        string value = kind == "f32"
            ? BitConverter.UInt32BitsToSingle(uint.Parse(text, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture)).ToString("R", CultureInfo.InvariantCulture)
            : BitConverter.UInt64BitsToDouble(ulong.Parse(text, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture)).ToString("R", CultureInfo.InvariantCulture);
        if (!value.Contains('.') && !value.Contains('E')) value += ".0";
        value += kind == "f32" ? "F" : "D";
        return value.StartsWith('-') ? "(" + value + ")" : value;
    }

    public static string Literal(string kind, string text)
    {
        if (!IsConstant(kind, text)) throw new InvalidDataException("Invalid scalar constant");
        if (IsPointer(kind)) return "((" + CSharp(kind) + ")null)";
        if (IsFloating(kind)) return FloatingLiteral(kind, text);
        if (IsWideInteger(kind))
        {
            if (long.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out long small) && (kind == "i128" || small >= 0))
                return "((" + CSharp(kind) + ")" + Literal("i64", text) + ")";
            UInt128 bits = kind == "u128" ? UInt128.Parse(text, CultureInfo.InvariantCulture) : unchecked((UInt128)Int128.Parse(text, CultureInfo.InvariantCulture));
            string literal = "new global::System.UInt128(0x" + ((ulong)(bits >> 64)).ToString("x", CultureInfo.InvariantCulture) + "UL, 0x" + unchecked((ulong)bits).ToString("x", CultureInfo.InvariantCulture) + "UL)";
            if (kind == "i128") literal = "unchecked((global::System.Int128)" + literal + ")";
            return literal + " /* " + text + " */";
        }
        return kind switch
        {
            "bool" => text == "0" ? "false" : "true",
            "i8" or "u8" or "i16" or "u16" => "((" + CSharp(kind) + ")" + text + ")",
            "u32" => text + "U", "u64" => text + "UL",
            "i64" => text.StartsWith('-') ? "(" + text + "L)" : text + "L",
            _ => text.StartsWith('-') ? "(" + text + ")" : text
        };
    }
}
