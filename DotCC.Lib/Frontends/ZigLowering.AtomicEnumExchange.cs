using System.Collections.Generic;
using DotCC.Ir;
using LALR.CC.LexicalGrammar;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    private (CType.Prim Carrier, int Bits) AtomicEnumStorage(CType.Enum type, string builtin)
    {
        if (type.Underlying.Unqualified is not CType.Prim
            { Integer: true, IsComptimeInt: false, Bytes: 1 or 2 or 4 or 8 } carrier
            || EnumBackingBits(type) is not { } bits || bits != carrier.Bytes * 8)
            throw new IrUnsupportedException($"zig {builtin}: enum storage requires a proved exact 8, 16, 32 or 64 bit source backing type");
        return (carrier, bits);
    }

    private CExpr LowerAtomicEnumExchange(IReadOnlyList<Item> arguments, CType.Enum type)
    {
        var (carrier, bits) = AtomicEnumStorage(type, "@atomicRmw");
        if (ComptimeExportTag(arguments[2], "AtomicRmwOp", "@atomicRmw") != "Xchg")
            throw new IrUnsupportedException("zig @atomicRmw: enum storage currently supports Xchg");
        var order = ComptimeExportTag(arguments[4], "AtomicOrder", "@atomicRmw") switch
        {
            "monotonic" => 0, "acquire" => 2, "release" => 3, "acq_rel" => 4, "seq_cst" => 5,
            var other => throw new CompileException($"zig @atomicRmw: invalid memory order '{other}'"),
        };
        var operands = new List<CExpr>();
        using var evaluation = new CallArgumentScope(this, operands, _hoistImpureSeen);
        var address = LowerExpr(arguments[1]);
        if (address.Type.Unqualified is not CType.Pointer pointer || !pointer.Pointee.Unqualified.Equals(type))
            throw new CompileException("zig @atomicRmw: pointer does not address the requested nominal enum type");
        if (pointer.Pointee.IsConst || address is Unary { Op: UnOp.AddrOf, Operand: var target } && IsConstStorage(target))
            throw new CompileException("zig @atomicRmw: cannot write through a const pointer");
        if (pointer.ZigSourceAlignment is > 0 && pointer.ZigSourceAlignment < _ir.AlignOfConst(type))
            throw new CompileException("zig @atomicRmw: pointer alignment is insufficient for atomic storage");
        var carrierPointer = pointer with { Pointee = carrier.WithQuals(pointer.Pointee.Quals) };
        evaluation.Add(new Cast(carrierPointer, address) { Type = carrierPointer });
        var value = LowerExprSink(arguments[3], type);
        if (!value.Type.Unqualified.Equals(type))
            throw new CompileException("zig @atomicRmw: operand must have the requested nominal enum type");
        evaluation.Add(new Cast(carrier, value) { Type = carrier });
        operands.Add(new LitInt(order.ToString(System.Globalization.CultureInfo.InvariantCulture), order) { Type = CType.Int });
        var exchange = new Call("__atomic_exchange_n", operands) { Type = carrier };
        _shared.BitcastWidths.CallResults[exchange] = new(carrier, bits);
        var result = new Cast(type, exchange) { Type = type };
        _shared.BitcastWidths.Expressions[result] = new(type, bits);
        return result;
    }
}
