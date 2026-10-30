#nullable enable
using System;
using System.Text;
using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    private bool EmitFlexibleGlobal(StringBuilder output, StringBuilder values, GlobalVar global)
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
        // end: a tail can begin inside the header's trailing padding. Separate
        // allocation from C value initialization so all eager global pointers
        // exist before header/tail initializers call owned-array helpers.
        output.Append("    private static unsafe ").Append(type).Append("* ").Append(storage)
            .Append(" = (").Append(type).Append("*)Libc.GlobalAlignedZeroed<byte>(checked((int)global::System.Math.Max((long)sizeof(")
            .Append(type).Append("), (long)(").Append(offset).Append(") + (long)").Append(initializer.Elems.Count)
            .Append(" * sizeof(").Append(element).Append("))), ").Append(alignment).Append(");\n");
        output.Append("    public static unsafe ref ").Append(type).Append(' ').Append(symbol.TargetName)
            .Append(" => ref *").Append(storage).Append(";\n");
        values.Append("        {\n        var value = (").Append(type)
            .Append("*)global::System.Runtime.CompilerServices.Unsafe.AsPointer(ref ")
            .Append(GlobalStorageReferences.Emit(symbol.TargetName)).Append(");\n")
            .Append("        *value = ").Append(Coerced(initializer.Header, symbol.Type)).Append(";\n")
            .Append("        var tail = (").Append(element).Append("*)((byte*)value + ").Append(offset).Append(");\n");
        for (var index = 0; index < initializer.Elems.Count; index++)
            values.Append("        tail[").Append(index).Append("] = ").Append(Coerced(initializer.Elems[index], initializer.Element)).Append(";\n");
        values.Append("        };\n");
        return true;
    }
}
