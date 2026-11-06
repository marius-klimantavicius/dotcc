#nullable enable
using DotCC.Ir;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    // Source tuple positions include comptime fields, even when the aggregate's
    // remaining fields are only known at runtime. Reading one needs no runtime base.
    private CExpr TupleElement(CExpr tuple, int index)
    {
        var type = (CType.Tuple)tuple.Type.Unqualified;
        if (type.ComptimeIntAt(index) is { } value)
            return _ir.SpliceComptimeValue(new IrBuilder.CtInt(value, CType.ComptimeInt))!;
        return new TupleIndex(tuple, index, type.Elements[index]) { Type = type.Elements[index], IsLValue = tuple.IsLValue };
    }
}
