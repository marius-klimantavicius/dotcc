// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Handwritten target character conversion following literals.c conv_single_char
// and conv_char_literal: explicit escape state, code-unit range, multichar
// ordering, signed extension and literal type selection. No CLR char folding.
using System.Collections.ObjectModel;
using System.Numerics;

namespace EdgSharp;

public enum LiteralEncoding { Ordinary, Wide, Utf8, Utf16, Utf32 }

public sealed class CharacterLiteralExpression : LiteralExpression
{
    public LiteralEncoding Encoding { get; }
    public ReadOnlyCollection<uint> CodeUnits { get; }
    public bool IsMulticharacter => CodeUnits.Count > 1;
    internal CharacterLiteralExpression(IntegerType type, SourceRange range, DecodedCharacterLiteral value)
        : base(type, range, value.Value) => (Encoding, CodeUnits) = (value.Encoding, Array.AsReadOnly((uint[])value.CodeUnits.Clone()));
}

internal sealed record DecodedCharacterLiteral(IntegerKind Kind, BigInteger Value, LiteralEncoding Encoding, uint[] CodeUnits, bool IsSigned);

internal static class CharacterLiteralDecoder
{
    internal static DecodedCharacterLiteral? Decode(string spelling, SourceRange range, TargetInfo target, DiagnosticBag diagnostics)
    {
        int quote = spelling.IndexOf('\''), close = spelling.LastIndexOf('\'');
        void Invalid(string message) => diagnostics.Add("LEX020", message, range);
        void Unsupported(string message) => diagnostics.Add("UNS020", message, range, DiagnosticClass.UnsupportedFeature);
        if (quote < 0 || close <= quote) { Invalid("Malformed character literal."); return null; }
        LiteralEncoding encoding;
        IntegerKind kind;
        int bits;
        bool signed;
        switch (spelling[..quote])
        {
            case "": encoding = LiteralEncoding.Ordinary; kind = IntegerKind.Char; bits = target.CharBits; signed = target.PlainCharIsSigned; break;
            case "L": encoding = LiteralEncoding.Wide; kind = IntegerKind.WChar; bits = target.WCharBits; signed = target.WCharIsSigned; break;
            case "u8": encoding = LiteralEncoding.Utf8; kind = IntegerKind.Char8; bits = target.Char8Bits; signed = false; break;
            case "u": encoding = LiteralEncoding.Utf16; kind = IntegerKind.Char16; bits = target.Char16Bits; signed = false; break;
            case "U": encoding = LiteralEncoding.Utf32; kind = IntegerKind.Char32; bits = target.Char32Bits; signed = false; break;
            default: Invalid("Invalid character literal prefix."); return null;
        }
        if (close + 1 != spelling.Length) { Unsupported("User-defined character literal suffixes are not implemented."); return null; }
        var units = new List<uint>();
        BigInteger mask = (BigInteger.One << bits) - 1;
        for (int index = quote + 1; index < close; ++index)
        {
            char c = spelling[index];
            BigInteger unit;
            if (c == '\\')
            {
                if (++index == close) { Invalid("Incomplete character escape."); return null; }
                c = spelling[index];
                const string escapes = "abfnrtv\\\"'?";
                const string values = "\a\b\f\n\r\t\v\\\"'?";
                int simple = escapes.IndexOf(c);
                if (simple >= 0) unit = values[simple];
                else if (c is 'u' or 'U')
                { Unsupported("Universal character names in character literals are not implemented yet."); return null; }
                else if (c == 'x' || c is >= '0' and <= '7')
                {
                    bool hexadecimal = c == 'x';
                    unit = 0;
                    int count = 0;
                    if (hexadecimal) ++index;
                    for (; index < close && (hexadecimal || count < 3); ++index)
                    {
                        char digitChar = spelling[index];
                        int digit = digitChar is >= '0' and <= '9' ? digitChar - '0' : digitChar is >= 'a' and <= 'f' ? digitChar - 'a' + 10 : digitChar is >= 'A' and <= 'F' ? digitChar - 'A' + 10 : -1;
                        if (digit < 0 || digit >= (hexadecimal ? 16 : 8)) break;
                        unit = unit * (hexadecimal ? 16 : 8) + digit; ++count;
                    }
                    --index;
                    if (count == 0) { Invalid("A hexadecimal character escape requires at least one digit."); return null; }
                }
                else
                {
                    if (c > 127) { Unsupported("Source character encoding in non-ASCII character escapes is not implemented yet."); return null; }
                    diagnostics.Add("LEX021", "Unrecognized character escape; the escaped character is used.", range, severity: DiagnosticSeverity.Warning);
                    unit = c;
                }
            }
            else
            {
                if (c > 127) { Unsupported("Source character encoding in non-ASCII character literals is not implemented yet."); return null; }
                unit = c;
            }
            if (unit < 0 || unit > mask) { Invalid("Character escape is outside the target code-unit range."); return null; }
            units.Add((uint)unit);
        }
        if (units.Count == 0) { Invalid("A character literal cannot be empty."); return null; }
        if (units.Count > 1 && encoding != LiteralEncoding.Ordinary)
        { Invalid("A prefixed character literal must contain exactly one code unit."); return null; }
        if (units.Count > target.IntBits / target.CharBits)
        { Invalid("A multicharacter literal exceeds the target int width."); return null; }
        BigInteger value = 0;
        for (int index = 0; index < units.Count; ++index)
        {
            BigInteger unit = units[index];
            if (signed && (target.MulticharFirstIsMostSignificant ? index == 0 : index == units.Count - 1) && unit > (mask >> 1)) unit -= mask + 1;
            if (target.MulticharFirstIsMostSignificant) value = (value << bits) | unit;
            else value |= unit << (bits * index);
        }
        if (units.Count > 1)
        {
            kind = IntegerKind.Int; signed = true;
            BigInteger modulus = BigInteger.One << target.IntBits;
            value = ((value % modulus) + modulus) % modulus;
            if (value >= (modulus >> 1)) value -= modulus;
            diagnostics.Add("LEX022", "Multicharacter literal uses the configured target byte order.", range, severity: DiagnosticSeverity.Warning);
        }
        return new(kind, value, encoding, units.ToArray(), signed);
    }
}
