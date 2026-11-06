#nullable enable
using System;
using System.Collections.Generic;

namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
    // Use the bitcast witness and source storage proof, exactly as runtime
    // emission does. CLR carrier sizes cannot establish Zig integer widths.
    private ComptimeValue? EvalZigBitcast(BitCast expression, ZigBitcastWidthLayout layout)
    {
        var width = layout.Source.LogicalBits;
        if (width is < 1 or > 128 || width != layout.Target.LogicalBits
            || layout.Source.Representation is not (ZigBitcastRepresentation.Integer or ZigBitcastRepresentation.Packed)
            || layout.Target.Representation is not (ZigBitcastRepresentation.Integer or ZigBitcastRepresentation.Packed)
            || EvalComptime(expression.Operand) is not { } value
            || PackedComptimeBits(value, expression.Operand.Type, width) is not { } bits) { return null; }
        return UnpackComptimeBits(bits, expression.Target, width, layout.Target.Signed);
    }

    private static UInt128 ComptimeBitMask(int width) => width == 128 ? UInt128.MaxValue : ((UInt128)1 << width) - 1;

    private UInt128? PackedComptimeBits(ComptimeValue value, CType type, int width)
    {
        var mask = ComptimeBitMask(width);
        if (value is CtInt integer) { return unchecked((UInt128)integer.Value) & mask; }
        if (value is CtBool boolean) { return boolean.Value ? (UInt128)1 : 0; }
        if (value is not CtStruct structure || type.Unqualified is not CType.Named name
            || !ZigStorageLayouts.TryGetValue(name.Name, out var proof) || proof.Kind != ZigStorageKind.PackedInteger
            || proof.LogicalBits != width) { return null; }
        proof.RequireSupported();
        UInt128 result = 0;
        foreach (var field in proof.SourceMembers)
        {
            if (proof.BackingKind == "packed-union" && structure.Active != field.Name) { continue; }
            if (!structure.Fields.TryGetValue(field.Name, out var member)
                || PackedComptimeBits(member, field.SourceType, field.LogicalBits) is not { } fieldBits) { return null; }
            result |= (fieldBits & ComptimeBitMask(field.LogicalBits)) << field.BitOffset;
        }
        if (proof.BackingKind == "packed-union" && structure.Active is null) { return null; }
        return result & mask;
    }

    private ComptimeValue? UnpackComptimeBits(UInt128 bits, CType type, int width, bool signed)
    {
        bits &= ComptimeBitMask(width);
        if (type.Unqualified is CType.Prim { Name: "_Bool" }) { return new CtBool(bits != 0); }
        if (type.Unqualified is CType.Prim { Integer: true } or CType.Enum)
        {
            var value = signed && width < 128 && (bits & ((UInt128)1 << (width - 1))) != 0
                ? unchecked((Int128)(bits | ~ComptimeBitMask(width))) : unchecked((Int128)bits);
            return new CtInt(value, type);
        }
        if (type.Unqualified is not CType.Named name || !ZigStorageLayouts.TryGetValue(name.Name, out var proof)
            || proof.Kind != ZigStorageKind.PackedInteger || proof.LogicalBits != width) { return null; }
        proof.RequireSupported();
        var fields = new Dictionary<string, ComptimeValue>(StringComparer.Ordinal);
        foreach (var field in proof.SourceMembers)
        {
            if (UnpackComptimeBits(bits >> field.BitOffset, field.SourceType, field.LogicalBits, field.Signed) is not { } member)
                return null;
            fields[field.Name] = member;
        }
        return new CtStruct(fields, type);
    }
}
