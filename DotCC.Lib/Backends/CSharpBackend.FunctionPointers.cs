#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    private readonly Dictionary<string, string> _functionPointers = new(StringComparer.Ordinal);
    private readonly HashSet<string> _definedFunctions = new(StringComparer.Ordinal);

    private string FunctionPointer(Symbol function)
    {
        var name = function.TargetName;
        var signature = Cs(function.Type.Unqualified);
        // A runtime overload is selected by the full structural pointer signature.
        // A translated definition takes precedence over a header prototype.
        var container = function.FromSystemHeader && !_definedFunctions.Contains(name)
            ? "global::Libc" : "DotCcFunctions";
        var declaration = $"{(_publicTypes ? "public" : "internal")} static unsafe partial class DotCcFunctionPointers\n"
            + "{\n"
            + $"    public static readonly {signature} {name} = &{container}.{name};\n"
            + "}\n";
        if (_functionPointers.TryGetValue(name, out var previous) && previous != declaration)
            throw new IrUnsupportedException("conflicting canonical function pointer signatures for '" + name + "'");
        _functionPointers[name] = declaration;
        return "global::DotCcFunctionPointers." + name;
    }

    private void RegisterPublicFunctionPointers(IrBuilder unit)
    {
        _definedFunctions.UnionWith(unit.Functions.Select(function => function.Sym.TargetName));
        // Managed consumers need a stable API even when the C source itself never
        // takes an exported method's address. Variadic methods retain their existing
        // params-array API; the C callback type does not model that managed tail.
        if (_publicTypes)
            foreach (var function in unit.Functions.Where(function => !function.Variadic))
                FunctionPointer(function.Sym);
    }
}
