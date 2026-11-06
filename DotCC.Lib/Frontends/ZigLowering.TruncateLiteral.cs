#nullable enable
using System.Globalization;
using System.Numerics;
using DotCC.Ir;
using LALR.CC.LexicalGrammar;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    private bool TryTruncateLiteral(Item source, CType target, ZigResultLocation? location, out CExpr result)
    {
        result = null!;
        if (!UsesSourceStdlib || target.Unqualified is not CType.Prim { Integer: true, IsComptimeInt: false } integer
            || integer.Name == "_Bool" || !TrySourceIntegerLiteral(source, out var value)) return false;
        var width = location?.IntegerBits ?? integer.Bytes * 8;
        if (width is < 1 or > 128) throw new IrUnsupportedException("zig @truncate literal requires a supported source result width");
        var modulus = BigInteger.One << width;
        value &= modulus - 1;
        if (integer.Signed && value >= (modulus >> 1)) value -= modulus;
        var text = value.ToString(CultureInfo.InvariantCulture);
        CExpr literal = value.Sign >= 0 ? DecodeZigInt(text)
            : new LitInt(text, value >= long.MinValue ? (long)value : null)
                { Type = integer.Bytes == 16 ? CType.Int128 : CType.Long };
        result = new Cast(target, literal) { Type = target };
        _shared.BitcastWidths.Expressions[result] = location ?? new(target, width);
        return true;
    }

    private static bool TrySourceIntegerLiteral(Item source, out BigInteger value)
    {
        if (source.Content is Zig.Grouped grouped) return TrySourceIntegerLiteral(grouped.Arg1, out value);
        if (source.Content is Zig.PreNeg negated && TrySourceIntegerLiteral(negated.Arg1, out value))
        { value = -value; return true; }
        value = default;
        if (source.Content is not Zig.IntLit literal) return false;
        var digits = Tok(literal.Arg0).Replace("_", "");
        var radix = 10;
        if (digits.StartsWith("0x", System.StringComparison.OrdinalIgnoreCase)) { radix = 16; digits = digits[2..]; }
        else if (digits.StartsWith("0o", System.StringComparison.OrdinalIgnoreCase)) { radix = 8; digits = digits[2..]; }
        else if (digits.StartsWith("0b", System.StringComparison.OrdinalIgnoreCase)) { radix = 2; digits = digits[2..]; }
        foreach (var character in digits)
        {
            var digit = character is >= '0' and <= '9' ? character - '0'
                : character is >= 'a' and <= 'f' ? character - 'a' + 10
                : character is >= 'A' and <= 'F' ? character - 'A' + 10 : -1;
            if (digit < 0 || digit >= radix) throw new CompileException("zig: invalid integer literal");
            value = value * radix + digit;
        }
        return true;
    }
}
