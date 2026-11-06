#nullable enable

using System.Collections.Generic;

namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
    // A field address retains the actual containing object. It has no numeric
    // address and cannot escape as a runtime literal.
    internal sealed record CtFieldPtr(CtStruct Parent, string Field, CType Type) : ComptimeValue;

    private static ComptimeValue? ReadComptimeFieldPointer(ComptimeValue? value)
        => value is CtFieldPtr pointer ? pointer.Parent.Fields.GetValueOrDefault(pointer.Field) : value;

    private bool TryEvalComptimeParentPointer(Cast expression, out ComptimeValue? value)
    {
        value = null;
        // The frontend's @fieldParentPtr is precisely a pointer cast of the
        // field's integer address minus its source owner's OffsetOf. Recover
        // only an address whose symbolic owner and field prove that operation.
        if (expression.Target.Unqualified is not CType.Pointer { Pointee.Unqualified: CType.Named parent }
            || expression.Operand is not Binary { Op: BinOp.Sub,
                Left: Cast { Operand: var fieldAddress }, Right: OffsetOf offset }
            || offset.Path.Count != 1 || StructCanonical(offset.StructType) != parent.Name)
            return false;

        if (EvalComptime(fieldAddress) is not CtFieldPtr pointer
            || StructCanonical(pointer.Parent.Type) != parent.Name
            || pointer.Field != offset.Path[0])
        {
            ComptimeMiss ??= "@fieldParentPtr requires the matching symbolic containing object and field";
            return true;
        }
        value = pointer.Parent;
        return true;
    }
}
