#nullable enable
using System;
using System.Text;
using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    private bool EmitFlexibleGlobal(StringBuilder output, GlobalVar global)
    {
        if (global.Init is not FlexibleAggregateInit initializer) return false;
        if (global.Sym.IsThreadLocal)
            throw new IrUnsupportedException("initialized thread-local flexible array storage");
        var symbol = global.Sym;
        var type = Cs(symbol.Type);
        var element = Cs(initializer.Element);
        var storage = "__dotcc_flexible_" + symbol.TargetName.TrimStart('@');
        var offset = Expr(new OffsetOf(symbol.Type, new[] { initializer.Field }, new CType.Array(initializer.Element, 0)) { Type = CType.SizeT });
        var alignment = Math.Max(symbol.Alignment, _offsetModel.Type(IrBuilder.LayoutType(symbol.Type)).Alignment);
        // Allocate the larger of the ABI header extent and the initialized tail
        // end: a tail can begin inside the header's trailing padding.
        output.Append("    private static unsafe ").Append(type).Append("* ").Append(storage)
            .Append(" = ").Append(storage).Append("_init();\n");
        output.Append("    private static unsafe ").Append(type).Append("* ").Append(storage).Append("_init()\n    {\n")
            .Append("        var value = (").Append(type).Append("*)Libc.GlobalAlignedZeroed<byte>(checked((int)global::System.Math.Max((long)sizeof(")
            .Append(type).Append("), (long)(").Append(offset).Append(") + (long)").Append(initializer.Elems.Count)
            .Append(" * sizeof(").Append(element).Append("))), ").Append(alignment).Append(");\n")
            .Append("        *value = ").Append(Coerced(initializer.Header, symbol.Type)).Append(";\n")
            .Append("        var tail = (").Append(element).Append("*)((byte*)value + ").Append(offset).Append(");\n");
        for (var index = 0; index < initializer.Elems.Count; index++)
            output.Append("        tail[").Append(index).Append("] = ").Append(Coerced(initializer.Elems[index], initializer.Element)).Append(";\n");
        output.Append("        return value;\n    }\n")
            .Append("    public static unsafe ref ").Append(type).Append(' ').Append(symbol.TargetName)
            .Append(" => ref *").Append(storage).Append(";\n");
        return true;
    }
}
