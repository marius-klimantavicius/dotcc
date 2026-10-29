#nullable enable

using System;
using System.Collections.Generic;

namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
    // Keep one Symbol, not merely one emitted field: earlier expressions retain
    // the symbol and may already have marked its address as taken.
    private sealed class ScalarGlobalDeclaration(Symbol symbol)
    {
        public Symbol Symbol { get; } = symbol;
        public int StorageIndex { get; set; } = -1;
        public bool HasInitializer { get; set; }
    }

    // External linkage is shared; internal linkage belongs to the containing
    // translation unit, including declarations originating in a shared header.
    private readonly Dictionary<(string Unit, string Name), ScalarGlobalDeclaration> _scalarGlobals = new();
    private readonly Dictionary<string, (string Unit, Symbol Symbol)> _scalarGlobalTargetOwners = new(StringComparer.Ordinal);
    private readonly HashSet<Symbol> _internalGlobals = new(ReferenceEqualityComparer.Instance);
    internal IReadOnlyCollection<Symbol> InternalGlobals => _internalGlobals;
    private bool _declaringInternalGlobal;

    private (string Unit, string Name) ScalarGlobalKey(string name, Storage semanticStorage)
    {
        var internalKey = (FunctionUnitIdentity, name);
        if (semanticStorage == Storage.Static) return internalKey;
        // An extern declaration inherits the linkage of a visible file-scope
        // static. A local automatic object can hide that declaration instead.
        if (_scalarGlobals.TryGetValue(internalKey, out var local)
            && ReferenceEquals(_symbols.Resolve(name), local.Symbol)) return internalKey;
        return ("", name);
    }

    private string InternalGlobalTargetName((string Unit, string Name) key)
        => _symbols.Escape(key.Name) + "__tu_" + Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(key.Unit)));

    private void NameRegisteredGlobal(Symbol candidate, (string Unit, string Name) key)
    {
        candidate.TargetName = _symbols.Escape(candidate.Name);
        if (_scalarGlobalTargetOwners.TryGetValue(candidate.Name, out var previous))
        {
            // Keep existing single-TU spellings. Once another object needs the
            // name, rename private symbols themselves so earlier references,
            // including address expressions, retain the same storage identity.
            if (previous.Unit.Length != 0)
                previous.Symbol.TargetName = InternalGlobalTargetName((previous.Unit, candidate.Name));
            if (key.Unit.Length != 0) candidate.TargetName = InternalGlobalTargetName(key);
        }
        else _scalarGlobalTargetOwners.Add(candidate.Name, (key.Unit, candidate));
    }

    private ScalarGlobalDeclaration? RegisterScalarGlobal(Symbol candidate, SrcPos position)
    {
        // Storage.Static is also the legacy representation of external global
        // definitions. The declaration's semantic linkage is tracked separately.
        var semanticStorage = _declaringInternalGlobal ? Storage.Static : Storage.Extern;
        var key = ScalarGlobalKey(candidate.Name, semanticStorage);
        if (!_scalarGlobals.TryGetValue(key, out var declaration))
        {
            _symbols.Declare(candidate);
            NameRegisteredGlobal(candidate, key);
            declaration = new ScalarGlobalDeclaration(candidate);
            _scalarGlobals.Add(key, declaration);
            if (key.Unit.Length != 0) _internalGlobals.Add(declaration.Symbol);
            return declaration;
        }
        if (!CompatibleGlobalTypes(declaration.Symbol.Type, candidate.Type))
        {
            Diagnostics.Add(new Diagnostic(Severity.Error,
                $"conflicting types for global '{candidate.Name}'", position, _file));
            return null;
        }
        if (declaration.Symbol.IsThreadLocal != candidate.IsThreadLocal || declaration.Symbol.IsConstexpr != candidate.IsConstexpr)
        {
            Diagnostics.Add(new Diagnostic(Severity.Error,
                $"conflicting storage specifiers for global '{candidate.Name}'", position, _file));
            return null;
        }
        declaration.Symbol.Alignment = Math.Max(declaration.Symbol.Alignment, candidate.Alignment);
        if (key.Unit.Length != 0) _internalGlobals.Add(declaration.Symbol);
        _symbols.DeclareAlias(declaration.Symbol);
        return declaration;
    }

    private void DefineRegisteredGlobal(ScalarGlobalDeclaration declaration, CExpr? initializer, bool hasInitializer, SrcPos position)
    {
        var symbol = declaration.Symbol;
        if (hasInitializer && declaration.HasInitializer)
        {
            Diagnostics.Add(new Diagnostic(Severity.Error,
                $"redefinition of global '{symbol.Name}'", position, _file));
            return;
        }
        if (!_internalGlobals.Contains(symbol)) _definedGlobalNames.Add(symbol.Name);
        if (declaration.StorageIndex < 0)
        {
            declaration.StorageIndex = Globals.Count;
            Globals.Add(new GlobalVar(symbol, initializer));
        }
        else if (hasInitializer)
        {
            Globals[declaration.StorageIndex] = new GlobalVar(symbol, initializer);
        }
        declaration.HasInitializer |= hasInitializer;
    }

    private static bool CompatibleGlobalTypes(CType left, CType right)
    {
        if (left.Quals != right.Quals) return false;
        return (left.Unqualified, right.Unqualified) switch
        {
            (CType.Pointer a, CType.Pointer b) => CompatibleGlobalTypes(a.Pointee, b.Pointee),
            (CType.Array a, CType.Array b) => (a.Count == b.Count || a.Count is null || b.Count is null)
                && CompatibleGlobalTypes(a.Element, b.Element),
            (CType.Func a, CType.Func b) => CompatibleFunctionTypes(a, b),
            _ => left == right,
        };
    }

    private static bool CompatibleFunctionTypes(CType.Func left, CType.Func right)
    {
        if (left.Variadic != right.Variadic || left.Params.Count != right.Params.Count || !CompatibleGlobalTypes(left.Return, right.Return)) return false;
        for (var index = 0; index < left.Params.Count; ++index)
            if (!CompatibleGlobalTypes(left.Params[index].Unqualified, right.Params[index].Unqualified)) return false;
        return true;
    }
}
