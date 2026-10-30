#nullable enable

using System.Globalization;
using System.Numerics;

namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
    private ComptimeValue? EvalZigDivCeil(Call call)
    {
        if (call.Args.Count is not (2 or 3)
            || call.Type.Unqualified is not CType.Prim { Integer: true } type)
            return null;
        var bits = type.Bytes * 8;
        if (call.Args.Count == 3)
        {
            if (EvalComptime(call.Args[2]) is not CtInt width || width.Value < 1 || width.Value > bits)
                throw new IrUnsupportedException("zig @divCeil has an invalid integer bit width");
            bits = (int)width.Value;
        }
        var left = DivCeilInteger(call.Args[0]);
        var right = DivCeilInteger(call.Args[1]);
        if (right is { IsZero: true })
            throw new IrUnsupportedException("zig @divCeil division by zero causes illegal behavior");
        var min = type.Signed ? -(BigInteger.One << (bits - 1)) : BigInteger.Zero;
        var max = (BigInteger.One << (type.Signed ? bits - 1 : bits)) - 1;
        if (left is { } knownLeft && (knownLeft < min || knownLeft > max)
            || right is { } knownRight && (knownRight < min || knownRight > max))
            throw new IrUnsupportedException("zig @divCeil operand does not fit its integer type");
        if (left is not { } a || right is not { } b) return null;
        var q = BigInteger.DivRem(a, b, out var remainder);
        if (!remainder.IsZero && remainder.Sign == b.Sign) q++;
        if (q < min || q > max)
            throw new IrUnsupportedException("zig @divCeil overflow of integer type");
        // CtInt stores u128's top half as Int128 bit patterns. Restore unsigned
        // operands before arithmetic, then put the exact result back in that form.
        var result = !type.Signed && bits == 128
            ? unchecked((System.Int128)(System.UInt128)q) : (System.Int128)q;
        return new CtInt(result, call.Type);
    }

    private BigInteger? DivCeilInteger(CExpr expression)
    {
        // The shared value domain is Int128. A literal in u128's top half can
        // still be read exactly here without changing C's constant evaluator.
        // The shared evaluator honors explicit casts and @truncate. Use it
        // before the wide-literal fallback so valid wrapping operands stay valid.
        if (EvalComptime(expression) is not CtInt value) return DivCeilLiteral(expression);
        var result = (BigInteger)value.Value;
        if (expression.Type.Unqualified is CType.Prim { Integer: true, Signed: false } unsigned && result.Sign < 0)
            result &= (BigInteger.One << (unsigned.Bytes * 8)) - 1;
        return result;
    }

    private static BigInteger? DivCeilLiteral(CExpr expression) => expression switch
    {
        LitInt literal when BigInteger.TryParse(literal.Digits, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) => value,
        Cast { Target.Unqualified: CType.Prim { Integer: true, IsComptimeInt: false, Bytes: > 0 and <= 16 } target } cast
            when DivCeilLiteral(cast.Operand) is { } operand => DivCeilCast(operand, target),
        Cast { Target.Unqualified: CType.Prim { IsComptimeInt: true } } cast => DivCeilLiteral(cast.Operand),
        Paren paren => DivCeilLiteral(paren.Inner),
        Unary { Op: UnOp.Neg } unary when DivCeilLiteral(unary.Operand) is { } magnitude => -magnitude,
        _ => null,
    };

    private static BigInteger DivCeilCast(BigInteger value, CType.Prim target)
    {
        var modulus = BigInteger.One << (target.Bytes * 8);
        var wrapped = value & (modulus - 1);
        return target.Signed && wrapped >= (modulus >> 1) ? wrapped - modulus : wrapped;
    }
}
