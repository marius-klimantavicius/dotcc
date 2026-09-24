using System;
using System.Collections.Generic;
using System.Linq;

namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
    private bool _pendingAttrWeak;
    private bool _sawWeakSpec;
    private int _weakFunctionAttributeApplications;
    // A declaration in one translation unit cannot weaken another unit's
    // strong definition merely because references share a canonical Symbol.
    private readonly HashSet<(string Unit, string Name)> _weakFunctionDeclarations = new();

    private void RememberWeakFunctionDeclaration(string name, bool isStatic)
    {
        if (!_pendingAttrWeak && !_sawWeakSpec) return;
        _weakFunctionAttributeApplications++;
        if (isStatic) throw new CompileException("weak function '" + name + "' must have external linkage");
        _weakFunctionDeclarations.Add((FunctionUnitIdentity, name));
    }

    private bool WeakDefinition(FnDefSite site) =>
        _weakFunctionDeclarations.Contains((site.Unit, site.Sym.Name));

    private bool TryBindWeakFunctionDefinition(FnSig signature, List<FnDefSite> sites, out Symbol symbol)
    {
        symbol = null!;
        if (signature.IsStatic) return false;
        var external = sites.Where(site => site.Sym.Storage != Storage.Static).ToArray();
        if (external.Length == 0 ||
            !_weakFunctionDeclarations.Contains((FunctionUnitIdentity, signature.Name)) && !external.Any(WeakDefinition))
            return false;
        if (external.Any(site => site.Unit == FunctionUnitIdentity))
            throw new CompileException("duplicate definition of function '" + signature.Name + "' in one translation unit");
        var type = new CType.Func(signature.Return, signature.Params.Select(p => p.Type).ToArray(), signature.Variadic);
        if (external.Any(site => !WeakFunctionTypesMatch(site.Sym.Type, type)))
            throw new CompileException("conflicting types for weak function '" + signature.Name + "'");
        symbol = external[0].Sym;
        _symbols.DeclareAlias(symbol);
        return true;
    }

    private static bool WeakFunctionTypesMatch(CType first, CType second) => (first, second) switch
    {
        (CType.Func a, CType.Func b) => a.Quals == b.Quals && a.Variadic == b.Variadic
            && WeakFunctionTypesMatch(a.Return, b.Return) && a.Params.Count == b.Params.Count
            && a.Params.Zip(b.Params).All(pair => WeakFunctionTypesMatch(pair.First.Unqualified, pair.Second.Unqualified)),
        (CType.Pointer a, CType.Pointer b) => a.Quals == b.Quals && WeakFunctionTypesMatch(a.Pointee, b.Pointee),
        _ => first == second,
    };

    internal IEnumerable<string> ReferencedWeakFunctionNames => _weakFunctionDeclarations
        .Select(declaration => declaration.Name).Distinct(StringComparer.Ordinal)
        .Where(name => _referencedFuncs.Contains(name)).Select(name => _symbols.Escape(name));

    internal void ValidateWeakFunctionReferences()
    {
        var definitions = Functions.Where(function => function.Sym.Storage != Storage.Static)
            .Select(function => function.Sym.TargetName).ToHashSet(StringComparer.Ordinal);
        foreach (var name in ReferencedWeakFunctionNames)
            if (!definitions.Contains(name))
                throw new CompileException("undefined weak function reference '" + name + "' is not supported");
    }

    private void AddFunctionDefinition(FuncDef definition)
    {
        Functions.Add(definition);
        var site = _fnDefSites[definition.Sym.Name].Last(site =>
            site.Unit == FunctionUnitIdentity && site.Definition == null);
        site.Definition = definition;
    }

    private void FinishWeakFunctionDefinitions()
    {
        foreach (var sites in _fnDefSites.Values)
        {
            var external = sites.Where(site => site.Sym.Storage != Storage.Static && site.Definition != null).ToArray();
            if (!external.Any(WeakDefinition)) continue;
            var strong = external.Where(site => !WeakDefinition(site)).ToArray();
            if (strong.Length > 1)
                throw new CompileException("duplicate definition of external function '" + strong[0].Sym.Name + "'");
            var winner = strong.Length == 1 ? strong[0] : external[0];
            var discarded = external.Where(site => site != winner).Select(site => site.Definition).ToHashSet();
            Functions.RemoveAll(definition => discarded.Contains(definition));
            if (!Functions.Contains(winner.Definition!)) Functions.Add(winner.Definition!);
            winner.Sym.IsWeak = WeakDefinition(winner);
        }
    }
}
