#nullable enable
using System.Linq;
using DotCC.Ir;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    // Runs only at an analyzed module's top-comptime boundary, after its member
    // index is complete. Recording an unused import binding never reaches here.
    private void RootExplicitExportFunctions()
    {
        // Declare all export signatures before demanding any body. Calls and
        // addresses, including forward references, keep the owner's one symbol.
        var exports = _cExportTopFunctions
            .Where(name => _moduleGraph?.ManagedRoots is not { } selected || selected.Contains(name))
            .Select(name => (Name: name,
            Symbol: (_lazy ? EnsureDeclLowered(name) : _exportedFns.GetValueOrDefault(name))
                ?? throw new CompileException("zig: missing explicit export function: " + name))).ToList();
        foreach (var (name, canonical) in exports)
        {
            if (IsFnTemplate(canonical) || canonical.Type.Unqualified is not CType.Func signature || signature.Variadic)
                throw new IrUnsupportedException("zig export fn requires a concrete function declaration");
            if (!TopFunctionAbis.TryGetValue(canonical, out var abi) || !abi.CAbi)
                throw new IrUnsupportedException("zig export fn must use the C calling convention");
            if (!TopExportAbiType(signature.Return, true) || signature.Params.Any(parameter => !TopExportAbiType(parameter, false)))
                throw new IrUnsupportedException("zig export fn has an unsupported ABI parameter or return type");
            _moduleGraph?.ManagedRootsFound.Add(name);
            RegisterTopExportName(name, canonical);
            var target = _names.Escape(name);
            if (_ir.Functions.Any(function => !ReferenceEquals(function.Sym, canonical) && function.Sym.TargetName == target)
                || _exportedFns.Values.Any(function => !ReferenceEquals(function, canonical) && function.TargetName == target)
                || (_modulePrefix is not null && _moduleGraph?.RootFunctionTargetNames().Contains(target) == true))
                throw new CompileException("zig: exported name collides with a function: " + name);
            // A source export is the canonical definition itself. Imported
            // functions normally have qualified names; this one has a C name.
            canonical.TargetName = target;
            RootTopExportBody(canonical);
        }
    }

    private void RegisterTopExportName(string name, Symbol canonical)
    {
        if (!TopExports.GetOrCreateValue(_ir).Names.TryAdd(name, canonical))
            throw new CompileException("zig: exported symbol collision: " + name);
    }

    private void RootTopExportBody(Symbol canonical)
    {
        _moduleGraph?.TryLowerBodyOnDemand(canonical);
        TryLowerBodyOnDemand(canonical);
    }
}
