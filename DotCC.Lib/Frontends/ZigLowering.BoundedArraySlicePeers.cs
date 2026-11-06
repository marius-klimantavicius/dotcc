#nullable enable
using DotCC.Ir;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    private bool IsSliceValuePeer(CExpr value, CType.Slice slice)
    {
        if (value.Type.Unqualified is CType.Slice) return true;
        if (StringArrayExtent(value) is not null) return true;
        if (!UsesSourceStdlib || value.Type.Unqualified is not CType.Pointer
            { ZigSourceSize: "one", ZigOptional: false, Pointee.Unqualified: CType.Array
                { Count: >= 0, RuntimeCount: null } array } pointer
            || array.Element.Unqualified != slice.Element.Unqualified) return false;
        // The bound comes from *[N]T, never from an unbounded pointer's carrier.
        // Keep a stronger target alignment only if this source pointer proves it.
        var available = pointer.ZigSourceAlignment is > 0 ? pointer.ZigSourceAlignment.Value : _ir.AlignOfConst(array);
        var required = slice.ZigSourceAlignment is > 0 ? slice.ZigSourceAlignment.Value : _ir.AlignOfConst(slice.Element.Unqualified);
        return available >= required;
    }

    private bool SliceValuePeerIsReadOnly(CExpr value) => StringArrayExtent(value) is not null
        || value.Type.Unqualified is CType.Slice { Element.IsConst: true }
        || value.Type.Unqualified is CType.Pointer { Pointee: var pointee }
            && (pointee.IsConst || pointee.Unqualified is CType.Array { Element.IsConst: true });
}
