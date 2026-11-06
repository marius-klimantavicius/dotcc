#nullable enable

using System.Collections.Generic;
using System.Globalization;
using DotCC.Ir;
using LALR.CC.LexicalGrammar;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    private CExpr LowerShlExact(IReadOnlyList<Item> args)
    {
        if (args.Count != 2)
            throw new IrUnsupportedException($"zig `@shlExact` expects (value, shift); got {args.Count} argument(s)");

        var operands = new List<CExpr>();
        using var evaluation = new CallArgumentScope(this, operands, _hoistImpureSeen);
        var value = LowerExpr(args[0]);
        var type = ZigIntOperandType(args[0]) ?? PeerTypeOfValue(args[0]) ?? value.Type.Unqualified;
        var bits = DeclaredBitsOfValue(args[0]) ?? DeclaredBitsOfLowered(value);
        if (type.Unqualified is not CType.Prim { Integer: true, IsComptimeInt: false, Name: not "_Bool", Bytes: > 0 and <= 16 } primitive
            || IsIntegerLiteral(args[0]))
            throw new IrUnsupportedException("zig @shlExact requires a fixed-width scalar integer; comptime_int and vector shifts are not supported");
        // A carrier alone cannot distinguish u3 from u8. Unknown source widths are an
        // explicit cut, including untyped comptime integers whose IR carrier looks finite.
        if (bits is null)
            throw new IrUnsupportedException("zig @shlExact requires a known logical integer width; comptime_int or untracked operand widths are not supported");
        if (bits is < 1 or > 128 || bits > primitive.Bytes * 8)
            throw new IrUnsupportedException("zig @shlExact supports scalar integer widths 1 through 128 only");
        evaluation.Add(CoerceToPeer(value, type));
        var shift = LowerExpr(args[1]);
        if (shift.Type.Unqualified is not CType.Prim { Integer: true, Name: not "_Bool", Bytes: > 0 and <= 16 })
            throw new IrUnsupportedException("zig @shlExact requires a scalar integer shift amount");
        // Do not narrow a count to int: an invalid large count must never wrap to a valid one.
        // UInt128's top half becomes negative in Int128, which also rejects before shifting.
        evaluation.Add(new Cast(CType.Int128, shift) { Type = CType.Int128 });
        var call = new Call("ZigMath.ShlExact", new List<CExpr> {
            operands[0], operands[1],
            new LitInt(bits.Value.ToString(CultureInfo.InvariantCulture), bits.Value) { Type = CType.Int },
        }) { Type = type };
        // Also validate constants here. The very same call is interpreted later when operands
        // become known only inside a comptime loop/function (Tabstops' mask initializer).
        if (_ir.ConstEval128(call) is not { } result) return call;
        return _ir.SpliceComptimeValue(new IrBuilder.CtInt(result, type)) ?? call;
    }
}
