using System;
using System.Collections.Generic;
using System.Linq;
using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    private readonly Dictionary<CType, bool> _zeroIntegerTypes = new();
    // u0 uses a byte only as a managed scalar expression carrier, as smaller
    // positive widths do. Its source type is distinct and has size/bits zero.
    // No byte carrier is admitted as source aggregate/array/pointer storage.
    private bool ContainsZeroInteger(CType type, HashSet<string>? active = null)
    {
        if (type.IsZigZeroInteger) return true;
        if (active is null)
        {
            if (_zeroIntegerTypes.TryGetValue(type, out var known)) return known;
            var result = ContainsZeroInteger(type, new HashSet<string>(StringComparer.Ordinal));
            _zeroIntegerTypes[type] = result;
            return result;
        }
        return type.Unqualified switch
        {
            CType.Named n when active.Add(n.Name) && _aggregateDefinitions.TryGetValue(n.Name, out var definition)
                => definition.Fields.Any(field => ContainsZeroInteger(field.Type, active)),
            CType.Pointer p => ContainsZeroInteger(p.Pointee, active),
            CType.Array a => ContainsZeroInteger(a.Element, active),
            CType.Slice s => ContainsZeroInteger(s.Element, active),
            CType.Optional o => ContainsZeroInteger(o.Inner, active),
            CType.ErrorUnion e => ContainsZeroInteger(e.Payload, active),
            CType.Enum e => ContainsZeroInteger(e.Underlying, active),
            CType.Tuple t => t.Elements.Any(element => ContainsZeroInteger(element, active)),
            CType.Vector v => ContainsZeroInteger(v.Element, active),
            CType.Func f => ContainsZeroInteger(f.Return, active) || f.Params.Any(parameter => ContainsZeroInteger(parameter, active)),
            _ => false,
        };
    }

    private void ValidateZeroIntegerType(CType type)
    {
        if (type.IsZigZeroInteger)
        {
            if (type.IsVolatile || type.IsAtomic || type.AlignmentOverride != 0)
                throw new IrUnsupportedException("zig u0 qualified/custom-aligned storage is unsupported");
            return;
        }
        if (type.Unqualified is CType.Func function)
        {
            if (function.IsNativeCallConv && ContainsZeroInteger(type))
                throw new IrUnsupportedException("zig u0 native transport requires an erased-argument ABI proof (unsupported)");
            ValidateZeroIntegerType(function.Return);
            foreach (var parameter in function.Params) ValidateZeroIntegerType(parameter);
            return;
        }
        if (ContainsZeroInteger(type))
            throw new IrUnsupportedException("zig u0 aggregate/zero-stride/optional/pointer storage is unsupported; only scalar zero values are supported");
    }

    private string ZeroIntegerConversion(CExpr value)
    {
        if (value.Type.IsZigZeroInteger) return Sub(value, PAssign);
        if (value.Type.Unqualified is not CType.Prim { Integer: true, Name: not "_Bool" })
            throw new IrUnsupportedException("zig u0 conversion requires an integer operand");
        if (TryConstInt(value, out var constant) && constant != 0)
            throw new IrUnsupportedException("zig u0 cannot represent a nonzero integer value");
        // Compare the operand once, in its original evaluation slot. There is
        // exactly one valid result; the checked conversion cannot admit byte1.
        return $"({Sub(value, PEq)} == 0 ? (byte)0 : throw new global::System.OverflowException(\"u0 can only represent zero\"))";
    }

    private bool TryRenderZeroInteger(CExpr expression, out (string, int) rendered)
    {
        rendered = default;
        switch (expression)
        {
            case Cast cast when cast.Target.IsZigZeroInteger:
                rendered = (ZeroIntegerConversion(cast.Operand), PPrimary);
                return true;
            case LitInt literal when literal.Type.IsZigZeroInteger:
                if (literal.Value != 0) throw new IrUnsupportedException("zig u0 literal must be zero");
                rendered = ("(byte)0", PUnary);
                return true;
            case DefaultLit or ZigUndefinedValue when expression.Type.IsZigZeroInteger:
                rendered = ("(byte)0", PUnary);
                return true;
            case Unary unary when unary.Operand.Type.IsZigZeroInteger:
                throw new IrUnsupportedException("zig u0 unary/address operation is unsupported: " + unary.Op);
            case Binary binary when binary.Type.IsZigZeroInteger:
                throw new IrUnsupportedException("zig u0 runtime arithmetic is unsupported: " + binary.Op);
            case Assign { CompoundOp: not null } assignment when assignment.Target.Type.IsZigZeroInteger:
                throw new IrUnsupportedException("zig u0 compound arithmetic is unsupported");
        }
        return false;
    }
}
