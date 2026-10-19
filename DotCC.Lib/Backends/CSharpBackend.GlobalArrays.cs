using System.Text;
using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    private void EmitGlobalArrayInitializer(StringBuilder output, GlobalVar global, PinnedArray array)
    {
        var count = array.Elems is { } elements
            ? new LitInt(elements.Count.ToString(System.Globalization.CultureInfo.InvariantCulture), elements.Count) { Type = CType.Int }
            : array.Count;
        var zeroed = new PinnedArray(array.Element, null, count) { Type = array.Type };
        var storage = GlobalStorageReferences.Emit(global.Sym.TargetName);
        output.Append(StaticStorageInitializers.AllocationMarker).Append("        ").Append(storage)
            .Append(" = ").Append(PinnedArrayText(zeroed, global.Sym.Alignment)).Append(";\n");
        if (array.Elems is not { Count: > 0 } values) return;
        string alias = InitializerAlias(global);
        var stores = new StringBuilder();
        EmitArrayStores(stores, alias, array.Element, values);
        AppendInitializerChunks(output, stores.ToString(), Cs(array.Element) + "* " + alias + " = ("
            + Cs(array.Element) + "*)" + storage + ";", pin: false);
    }

    private void EmitArrayStores(StringBuilder output, string alias, CType element,
        System.Collections.Generic.IReadOnlyList<CExpr> values)
    {
        if (EmitPrimitiveCopies(output, alias, element, values, zeroed: true)) return;
        for (int index = 0; index < values.Count; index++)
            EmitStaticAggregateStores(output, alias + "[" + index + "]", element, values[index], readableRow: true);
    }
}
