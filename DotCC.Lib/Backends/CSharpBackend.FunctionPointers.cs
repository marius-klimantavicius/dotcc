#nullable enable
using System;
using System.Collections.Generic;
using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    private readonly Dictionary<string, string> _functionPointers = new(StringComparer.Ordinal);

    private string FunctionPointer(Symbol function)
    {
        var name = function.TargetName;
        var signature = Cs(function.Type.Unqualified);
        // The final shell/link binds this alias to the translated class when a
        // definition exists, otherwise Libc. Header provenance does not decide
        // linkage: runtime functions may be declared without including a header.
        var container = FunctionPointerNames.OwnerAlias(name);
        var declaration = $"{(_publicTypes ? "public" : "internal")} static unsafe partial class DotCcFunctionPointers\n"
            + "{\n"
            + $"    public static readonly {signature} {name} = &{container}.{name};\n"
            + "}\n";
        if (_functionPointers.TryGetValue(name, out var previous) && previous != declaration)
            throw new IrUnsupportedException("conflicting canonical function pointer signatures for '" + name + "'");
        _functionPointers[name] = declaration;
        return (_relocatable ? "DotCcPointers." : "global::DotCcFunctionPointers.") + name;
    }

    private void RegisterPublicFunctionPointers(IrBuilder unit)
    {
        // Managed consumers need a stable API even when the C source itself never
        // takes an exported method's address. Variadic pointers include the explicit
        // span tail used by the emitted method's managed calling convention.
        if (_publicTypes)
            foreach (var function in unit.Functions)
                FunctionPointer(function.Sym);
    }
}
