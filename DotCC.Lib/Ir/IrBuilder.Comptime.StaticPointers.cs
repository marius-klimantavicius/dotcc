#nullable enable

namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
    // A static object's address is known without reading its runtime contents.
    // Keep the source symbol rather than inventing a numeric host address.
    internal sealed record CtStaticPtr(Symbol Object, CType Type) : ComptimeValue;

    private static CExpr SpliceStaticPointer(CtStaticPtr pointer)
    {
        pointer.Object.AddressTaken = true;
        var address = new Unary(UnOp.AddrOf, new VarRef(pointer.Object)
        {
            Type = pointer.Object.Type, IsLValue = true,
        }) { Type = new CType.Pointer(pointer.Object.Type) };
        return address.Type == pointer.Type ? address : new Cast(pointer.Type, address) { Type = pointer.Type };
    }

    private ComptimeValue? ReadStaticPointer(CtStaticPtr pointer) =>
        ConstGlobalInits.TryGetValue(pointer.Object, out var initializer)
            && EvalConstGlobal(pointer.Object, initializer) is { } value ? CloneComptime(value) : null;
}
