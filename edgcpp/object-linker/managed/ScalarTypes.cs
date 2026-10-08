using System.Globalization;
using System.Numerics;

namespace DotObj;

internal static class ScalarTypes
{
    public static bool IsArray(string kind) => kind.StartsWith("array(", StringComparison.Ordinal) && kind.EndsWith(')');
    public static long ElementCount(string kind) => long.Parse(kind.AsSpan(6, kind.IndexOf(',') - 6), CultureInfo.InvariantCulture);
    public static string Element(string kind) => kind[(kind.IndexOf(',') + 1)..^1];
    public static string Leaf(string kind) => IsArray(kind) ? Leaf(Element(kind)) : kind;
    public static long FlatCount(string kind) => IsArray(kind) ? checked(ElementCount(kind) * FlatCount(Element(kind))) : 1;
    public static long Size(string kind) => IsArray(kind) ? checked(ElementCount(kind) * Size(Element(kind))) : IsPointer(kind) ? 8 : kind switch
    { "bool" or "i8" or "u8" => 1, "i16" or "u16" => 2, "i32" or "u32" => 4, "i64" or "u64" => 8, _ => throw new InvalidDataException("Type has no storage size") };
    public static string ArrayName(string kind) => "__DotObjArray_" + Mangle(kind);
    private static string Mangle(string kind) => IsArray(kind) ? "A" + ElementCount(kind) + "_" + Mangle(Element(kind)) : IsPointer(kind) ? "P_" + Mangle(Pointee(kind)) : kind;
    public static bool IsPointer(string kind) => kind.StartsWith("ptr(", StringComparison.Ordinal) && kind.EndsWith(')');
    public static string Pointee(string kind) => IsPointer(kind) ? kind[4..^1]
        : throw new InvalidDataException("Expected pointer type");
    public static bool IsInteger(string kind) => kind is "i8" or "u8" or "i16" or "u16" or "i32" or "u32" or "i64" or "u64";
    public static bool IsPromotedInteger(string kind) => kind is "i32" or "u32" or "i64" or "u64";
    public static bool IsBooleanResult(string kind) => kind is "bool" or "i32" or "u8";
    public static string CSharp(string kind) => IsArray(kind) ? ArrayName(kind) : IsPointer(kind) ? CSharp(Pointee(kind)) + "*" : kind switch
    {
        "void" => "void", "bool" => "bool", "i8" => "sbyte", "u8" => "byte",
        "i16" => "short", "u16" => "ushort", "i32" => "int", "u32" => "uint",
        "i64" => "long", "u64" => "ulong", _ => throw new InvalidDataException("Unsupported C# type")
    };

    // Decimal text is exact even where the unsigned value does not fit a SQLite
    // signed INTEGER. Reject whitespace, plus signs and non-canonical spellings.
    public static bool IsConstant(string kind, string? text)
    {
        if (IsPointer(kind)) return text == "0";
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
        if (!IsInteger(sourceKind) || !IsConstant(sourceKind, text) || !IsPromotedInteger(selectorKind))
            throw new InvalidDataException("Invalid switch case conversion");
        int width = selectorKind is "i32" or "u32" ? 32 : 64;
        BigInteger modulus = BigInteger.One << width;
        BigInteger value = BigInteger.Parse(text, CultureInfo.InvariantCulture) % modulus;
        if (value.Sign < 0) value += modulus;
        if (selectorKind is "i32" or "i64" && value >= (modulus >> 1)) value -= modulus;
        return value.ToString(CultureInfo.InvariantCulture);
    }

    public static string Literal(string kind, string text)
    {
        if (!IsConstant(kind, text)) throw new InvalidDataException("Invalid scalar constant");
        if (IsPointer(kind)) return "((" + CSharp(kind) + ")null)";
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
