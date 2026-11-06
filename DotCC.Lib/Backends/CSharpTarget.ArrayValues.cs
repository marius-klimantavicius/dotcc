using System;
using System.Collections.Generic;
using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpTarget
{
    private readonly Dictionary<string, (string Element, int Count)> _arrayValues = new(StringComparer.Ordinal);
    private string ArrayValueName(CType.Array array)
    {
        var count = FlatCount(array) ?? throw new IrUnsupportedException("array error payload needs a known extent");
        if (count <= 0 || array.FlatElement.Unqualified is CType.Pointer or CType.Func or CType.Array)
            throw new IrUnsupportedException("unsupported array error payload element or extent");
        var element = RenderType(array.FlatElement.Unqualified);
        var name = "__DotCcArrayValue_" + AggregateIdentities.Hash(ObjectAggregateMetadata.Describe(array));
        _arrayValues[name] = (element, count);
        return name;
    }

    internal IEnumerable<KeyValuePair<string, string>> ArrayValueTypesText(bool publicTypes)
    {
        foreach (var (name, (element, count)) in _arrayValues)
            yield return new(name,
                $"[global::System.Runtime.CompilerServices.InlineArray({count})]\n"
                + (publicTypes ? "public " : "internal ") + $"unsafe struct {name}\n{{\n"
                + $"    private {element} _first;\n"
                + $"    public static implicit operator {name}({element}* source)\n    {{\n"
                + $"        {name} value = default;\n"
                + $"        global::System.Buffer.MemoryCopy(source, &value, sizeof({name}), sizeof({name}));\n"
                + "        return value;\n    }\n"
                + $"    public {element}* ToOwnedArray()\n    {{\n"
                + $"        fixed ({name}* source = &this) return ZigAlloc.CopyArrayResult(({element}*)source, {count});\n"
                + "    }\n}\n");
    }
}
