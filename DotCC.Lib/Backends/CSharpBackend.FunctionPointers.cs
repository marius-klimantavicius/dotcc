#nullable enable
using System;
using System.Collections.Generic;
using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    private readonly Dictionary<string, string> _functionPointers = new(StringComparer.Ordinal);

    private readonly HashSet<string> _usedFunctionAddresses = new(StringComparer.Ordinal);

    private string FunctionPointer(Symbol function, bool synthetic = false)
    {
        var name = function.TargetName;
        if (!synthetic) _usedFunctionAddresses.Add(name);
        var signature = Cs(function.Type.Unqualified);
        // The final shell/link binds this alias to the translated class when a
        // definition exists, otherwise Libc. Header provenance does not decide
        // linkage: runtime functions may be declared without including a header.
        var container = FunctionPointerNames.OwnerAlias(name);
        var declaration = $"{(_publicTypes ? "public" : "internal")} static unsafe partial class DotCcFunctionPointers\n"
            + "{\n"
            + $"    public static readonly {signature} {name} = &{container}.{FunctionName(function)};\n"
            + "}\n";
        if (_functionPointers.TryGetValue(name, out var previous) && previous != declaration)
            throw new IrUnsupportedException("conflicting canonical function pointer signatures for '" + name + "'");
        _functionPointers[name] = declaration;
        return (_relocatable ? "DotCcPointers." : "global::" + _pointerClass + ".") + name;
    }

    private void RegisterPublicFunctionPointers(IrBuilder unit)
    {
        // Managed consumers need a stable API even when the C source itself never
        // takes an exported method's address. Variadic pointers include the explicit
        // span tail used by the emitted method's managed calling convention.
        // Macro-generated helpers are callable methods, but do not need an API
        // address unless an actual C designator use requests one via FunctionPointer.
        if (_publicTypes)
            foreach (var function in unit.Functions)
                if (!function.Sym.IsMacroGenerated)
                    FunctionPointer(function.Sym, synthetic: true);
    }
}
