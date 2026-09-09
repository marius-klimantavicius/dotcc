#nullable enable

using System;
using System.Collections.Generic;
using Item = global::LALR.CC.LexicalGrammar.Item;

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

    private readonly Dictionary<string, ScalarGlobalDeclaration> _scalarGlobals = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _scalarDefinitionUnits = new(StringComparer.Ordinal);

    // Preserve the existing cross-TU identical-header-definition behavior, but
    // never suppress declarations in the same TU: two initialized definitions
    // there are an error even when their token text is identical.
    private bool AlreadySeenScalarGlobalInAnotherUnit(Item declaration)
    {
        var fingerprint = declaration.ToString();
        if (_scalarDefinitionUnits.TryGetValue(fingerprint, out var firstFile)) return firstFile != _file;
        _scalarDefinitionUnits.Add(fingerprint, _file);
        return false;
    }

    private ScalarGlobalDeclaration? RegisterScalarGlobal(Symbol candidate, SrcPos position)
    {
        if (!_scalarGlobals.TryGetValue(candidate.Name, out var declaration))
        {
            declaration = new ScalarGlobalDeclaration(_symbols.Declare(candidate));
            _scalarGlobals.Add(candidate.Name, declaration);
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
        _definedGlobalNames.Add(symbol.Name);
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
