#nullable enable

using System.Numerics;

namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
    private ComptimeValue? EvalZigShlExact(Call call)
    {
        if (call.Args is not [var value, var shift, var width]
            || call.Type.Unqualified is not CType.Prim { Integer: true, IsComptimeInt: false, Bytes: > 0 and <= 16 } type)
            throw new IrUnsupportedException("zig @shlExact requires a fixed-width scalar integer");
        if (EvalComptime(width) is not CtInt bits || bits.Value < 1 || bits.Value > type.Bytes * 8)
            throw new IrUnsupportedException("zig @shlExact has an invalid integer bit width");
        // Reuse the exact numeric reader: it preserves unsigned u128 bit patterns and casts.
        var a = DivCeilInteger(value);
        var count = DivCeilInteger(shift);
        if (count is { } n && (n.Sign < 0 || n >= (int)bits.Value))
            throw new IrUnsupportedException("zig @shlExact shift amount must be non-negative and less than the integer bit width");
        var min = type.Signed ? -(BigInteger.One << ((int)bits.Value - 1)) : BigInteger.Zero;
        var max = (BigInteger.One << ((int)bits.Value - (type.Signed ? 1 : 0))) - 1;
        if (a is { } known && (known < min || known > max))
            throw new IrUnsupportedException("zig @shlExact operand does not fit its logical integer width");
        if (a is not { } input || count is not { } amount) return null;
        var exact = input << (int)amount;
        if (exact < min || exact > max)
            throw new IrUnsupportedException("zig @shlExact overflow: shifted bits disagree with the result sign or leave the unsigned integer width");
        var result = !type.Signed && bits.Value == 128
            ? unchecked((System.Int128)(System.UInt128)exact) : (System.Int128)exact;
        return new CtInt(result, call.Type);
    }
}
