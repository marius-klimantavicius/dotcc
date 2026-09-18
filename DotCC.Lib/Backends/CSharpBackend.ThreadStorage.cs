#nullable enable
using System.Text;
using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    private void EmitThreadLocalArray(StringBuilder output, GlobalVar global, PinnedArray array)
    {
        if (array.Elems is not null)
            throw new IrUnsupportedException("initialized thread-local arrays are not supported");
        var symbol = global.Sym;
        var type = Cs(symbol.Type);
        var element = Cs(array.Element);
        var count = array.Count is { } length ? Expr(length) : "0";
        var alignment = ObjectAlignment(symbol);
        var backing = "__dotcc_tls_array_" + symbol.TargetName.TrimStart('@');
        // ThreadStatic owns the pinned managed object, not just its address.
        // It stays rooted while the thread can use the C pointer, and can be
        // collected with the thread's storage. Byte backing also supports C
        // pointer elements and explicit over-alignment without generic T* args.
        output.Append("    [ThreadStatic]\n    private static byte[] ").Append(backing).Append(";\n");
        output.Append("    public static unsafe ").Append(type).Append(' ').Append(symbol.TargetName)
            .Append("\n    {\n        get\n        {\n            var storage = ").Append(backing)
            .Append(";\n            if (storage is null)\n            {\n                storage = global::System.GC.AllocateArray<byte>(checked((")
            .Append(count).Append(") * sizeof(").Append(element).Append(") + ").Append(alignment - 1)
            .Append("), pinned: true);\n                ").Append(backing).Append(" = storage;\n                DotCcFunctions.ThreadGlobals.")
            .Append(symbol.TargetName).Append(" = (").Append(type)
            .Append(")(((nuint)global::System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::System.Runtime.InteropServices.MemoryMarshal.GetArrayDataReference(storage)) + ")
            .Append(alignment - 1).Append(") & ~(nuint)").Append(alignment - 1)
            .Append(");\n            }\n            return DotCcFunctions.ThreadGlobals.").Append(symbol.TargetName)
            .Append(";\n        }\n    }\n");
    }
}
