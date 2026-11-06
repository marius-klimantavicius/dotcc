#nullable enable
using System;
using System.Globalization;

namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
    private ComptimeValue RetypeWrappingAssignment(ComptimeValue value, CType type, int? bits)
    {
        if (bits is { } width && value is CtInt integer && type.Unqualified is CType.Prim { Integer: true } primitive)
        {
            var normalized = primitive.Signed
                ? (integer.Value << (128 - width)) >> (128 - width)
                : unchecked((System.Int128)((System.UInt128)integer.Value & ((System.UInt128.One << width) - 1)));
            value = new CtInt(normalized, integer.Type);
        }
        return RetypeTo(value, type);
    }

    private sealed record ComptimeWrappingSlot(CType Type, Func<ComptimeValue?> Read, Action<ComptimeValue> Store);

    private ComptimeValue? EvalComptimeWrappingAssignment(Assign assignment)
    {
        // Zig reads the destination before evaluating the RHS. Resolve its storage once,
        // keeping the same array/aggregate/pointer even if the RHS changes an alias.
        var slot = ResolveComptimeWrappingSlot(assignment.Target)
            ?? throw new ComptimeAbort("comptime wrapping assignment requires addressable scalar storage");
        if (slot.Read() is not { } old || EvalComptime(assignment.Value) is not { } value
            || assignment.CompoundOp is not { } operation || CombineBin(operation, old, value) is not { } combined)
            return null;
        var stored = RetypeWrappingAssignment(combined, slot.Type, assignment.ZigWrappingBits);
        slot.Store(stored);
        return stored;
    }

    private ComptimeWrappingSlot? ResolveComptimeWrappingSlot(CExpr expression)
    {
        switch (expression)
        {
            case Paren parenthesized: return ResolveComptimeWrappingSlot(parenthesized.Inner);
            case VarRef variable:
            {
                var storage = _comptimeFrame is { } frame && frame.ContainsKey(variable.Sym) ? frame
                    : ComptimeGlobals.ContainsKey(variable.Sym) ? ComptimeGlobals : _comptimeFrame;
                return storage is null ? null : new(variable.Type,
                    () => storage.GetValueOrDefault(variable.Sym), value => storage[variable.Sym] = value);
            }
            case Member member when DereferenceComptimeStruct(EvalComptime(member.Base), member.Base.Type) is CtStruct aggregate:
                return new(member.Type, () => aggregate.Fields.GetValueOrDefault(member.Field), value => aggregate.Fields[member.Field] = value);
            case TupleIndex tuple when EvalComptime(tuple.Tuple) is CtStruct aggregate:
            {
                var field = tuple.Index.ToString(CultureInfo.InvariantCulture);
                return new(tuple.Type, () => aggregate.Fields.GetValueOrDefault(field), value => aggregate.Fields[field] = value);
            }
            case Index index when ElementSlot(index) is ({ } array, var at): return ArraySlot(array, at);
            case Unary { Op: UnOp.Deref, Operand: var pointer }:
                return EvalComptime(pointer) switch
                {
                    CtElemPtr element => ArraySlot(element.Backing, element.Index),
                    CtVarPtr variable => new(expression.Type,
                        () => variable.Storage.GetValueOrDefault(variable.Variable), value => variable.Storage[variable.Variable] = value),
                    CtFieldPtr field => new(expression.Type,
                        () => field.Parent.Fields.GetValueOrDefault(field.Field), value => field.Parent.Fields[field.Field] = value),
                    _ => null,
                };
            default: return null;
        }

        static ComptimeWrappingSlot ArraySlot(CtArray array, long at)
        {
            if (at < 0 || at >= array.Elems.Length) throw new ComptimeAbort("comptime wrapping array index out of bounds");
            return new(array.Element, () => array.Elems[at], value => array.Elems[at] = value);
        }
    }
}
