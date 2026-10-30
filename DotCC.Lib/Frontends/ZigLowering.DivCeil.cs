#nullable enable

using System.Collections.Generic;
using System.Globalization;
using DotCC.Ir;
using LALR.CC.LexicalGrammar;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    private CExpr LowerDivCeil(IReadOnlyList<Item> args)
    {
        if (args.Count != 2)
            throw new IrUnsupportedException($"zig `@divCeil` expects (a, b); got {args.Count} argument(s)");

        var a = LowerExpr(args[0]);
        var b = LowerExpr(args[1]);
        var aBits = DeclaredBitsOfValue(args[0]) ?? DeclaredBitsOfLowered(a);
        var bBits = DeclaredBitsOfValue(args[1]) ?? DeclaredBitsOfLowered(b);
        // A negative literal is a unary expression in the IR. It still adopts
        // its typed peer in Zig, including that peer's storage size and sign.
        var type = aBits is not null && bBits is null && _ir.ConstEval128(b) is not null
            ? PeerTypeOfValue(args[0]) ?? a.Type.Unqualified
            : bBits is not null && aBits is null && _ir.ConstEval128(a) is not null
                ? PeerTypeOfValue(args[1]) ?? b.Type.Unqualified : PeerIntType(a, b);
        if (type.Unqualified is not CType.Prim { Integer: true } primitive || type.Equals(CType.Bool)
            || !a.Type.IsInteger || !b.Type.IsInteger)
            throw new IrUnsupportedException("zig `@divCeil` currently supports integer operands only; floating-point division is not lowered");

        var bits = aBits is { } ab && bBits is { } bb ? System.Math.Max(ab, bb)
            : aBits ?? bBits ?? primitive.Bytes * 8;
        // Untyped integer constants have no 32/64-bit carrier bound in Zig.
        if (aBits is null && bBits is null && _ir.ConstEval128(a) is not null && _ir.ConstEval128(b) is not null)
        {
            type = CType.ComptimeInt;
            bits = 128;
        }
        var call = new Call("ZigMath.DivCeil", new List<CExpr> {
            CoerceToPeer(a, type), CoerceToPeer(b, type),
            new LitInt(bits.ToString(CultureInfo.InvariantCulture), bits) { Type = CType.Int },
        }) { Type = type };
        // This uses the shared interpreter with function calls disabled: runtime arguments
        // stay call arguments and execute exactly once, including side effects.
        // Validate each known operand before the peer conversion. A negative
        // literal cannot silently wrap to an unsigned peer at this boundary.
        var evaluation = call with { Args = new List<CExpr> { a, b, call.Args[2] } };
        if (_ir.ConstEval128(evaluation) is not { } value) return call;
        if (type.Unqualified is CType.Prim { Signed: false, Bytes: 16 } && value < 0)
            return new LitInt(unchecked((System.UInt128)value).ToString(CultureInfo.InvariantCulture), null) { Type = type };
        return _ir.SpliceComptimeValue(new IrBuilder.CtInt(value, type)) ?? call;
    }
}
