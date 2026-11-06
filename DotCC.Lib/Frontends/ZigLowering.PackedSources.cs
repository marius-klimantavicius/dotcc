#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using DotCC.Ir;
using LALR.CC.LexicalGrammar;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    private sealed record PackedSource(ZigLowering Owner, Item? Backing, InlineCaptures? Captures);

    private void InstallDefinitionType(TypeSeed seed)
    {
        _typeAliases[seed.Name] = seed.Type;
        SetDeclaredIntBits(seed.Name, seed.DeclaredBits);
        SetDeclaredPtrSize(seed.Name, seed.PointerSize);
        SetDeclaredSentinel(seed.Name, seed.Sentinel);
    }

    // A type-returning body is a new lexical function, even when evaluated while
    // lowering a caller in the same module. Reuse the symbol table's existing
    // function suspension and resolve module aliases from their original RHS.
    private sealed class DefinitionBindingScope : IDisposable
    {
        private readonly ZigLowering _owner;
        private readonly Dictionary<string, CType> _types;
        private readonly Dictionary<string, int> _bits;
        private readonly Dictionary<string, string> _pointers;
        private readonly Dictionary<string, ZigSentinel> _sentinels;
        private readonly Dictionary<string, CExpr> _values;
        private readonly Dictionary<string, (ZigLowering Owner, Symbol Sym)> _functions;
        private readonly Dictionary<string, CType?> _localContainers;
        private readonly SymbolTable.SuspendedFunction _symbols;
        private readonly string? _container;
        private readonly string? _constContainer;
        private readonly string _function;
        private readonly List<string> _addedDeferred = new();
        private bool _disposed;

        internal DefinitionBindingScope(ZigLowering owner)
        {
            _owner = owner;
            _types = new(owner._typeAliases, StringComparer.Ordinal);
            _bits = new(owner._declaredIntBits, StringComparer.Ordinal);
            _pointers = new(owner._declaredPtrSize, StringComparer.Ordinal);
            _sentinels = new(owner._declaredSentinel, StringComparer.Ordinal);
            _values = new(owner._comptimeValues, StringComparer.Ordinal);
            _functions = new(owner._fnAliases, StringComparer.Ordinal);
            _localContainers = new(StringComparer.Ordinal);
            _symbols = owner._symbols.SuspendFunction();
            _container = owner._currentContainer;
            _constContainer = owner._currentConstContainer;
            _function = owner._currentFnName;
            owner._currentFnName = "";
            owner._currentContainer = owner._currentConstContainer = null;
            owner._typeAliases.Clear();
            owner._declaredIntBits.Clear();
            owner._declaredPtrSize.Clear();
            owner._declaredSentinel.Clear();
            owner._fnAliases.Clear();
            owner._comptimeValues.Clear();
            // Register only syntax. Poisoned and unused module declarations stay lazy.
            foreach (var (name, rhs) in owner._topLevelConstRhs)
                if (owner._deferredTypeCalls.TryAdd(name, rhs)) _addedDeferred.Add(name);
            foreach (var (name, previous) in owner._localContainerShadows.AsEnumerable().Reverse())
            {
                _localContainers.TryAdd(name, owner._containerTypes.GetValueOrDefault(name));
                if (previous is null) owner._containerTypes.Remove(name);
                else owner._containerTypes[name] = previous;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            var o = _owner;
            // Keep newly demanded module aliases: their deferred RHS may have been
            // consumed. Existing caller aliases must regain their original bindings.
            foreach (var (name, type) in o._typeAliases)
                if (!_types.ContainsKey(name) && o._topLevelConstRhs.ContainsKey(name))
                {
                    _types[name] = type;
                    if (o._declaredIntBits.TryGetValue(name, out var bits)) _bits[name] = bits;
                    if (o._declaredPtrSize.TryGetValue(name, out var pointer)) _pointers[name] = pointer;
                    if (o._declaredSentinel.TryGetValue(name, out var sentinel)) _sentinels[name] = sentinel;
                }
            Restore(o._typeAliases, _types);
            Restore(o._declaredIntBits, _bits);
            Restore(o._declaredPtrSize, _pointers);
            Restore(o._declaredSentinel, _sentinels);
            Restore(o._comptimeValues, _values);
            Restore(o._fnAliases, _functions);
            foreach (var (name, type) in _localContainers)
                if (type is null) o._containerTypes.Remove(name); else o._containerTypes[name] = type;
            foreach (var name in _addedDeferred) o._deferredTypeCalls.Remove(name);
            o._currentContainer = _container;
            o._currentConstContainer = _constContainer;
            o._currentFnName = _function;
            o._symbols.ResumeFunction(_symbols);
        }

        private static void Restore<T>(Dictionary<string, T> target, Dictionary<string, T> saved)
        {
            target.Clear();
            foreach (var pair in saved) target.Add(pair.Key, pair.Value);
        }
    }

    private DefinitionBindingScope EnterPackedSource(string name, PackedSource source)
    {
        var scope = new DefinitionBindingScope(this);
        try
        {
            _currentContainer = name;
            _symbols.EnterScope();
            if (source.Captures is { } captures)
            {
                foreach (var seed in captures.Types) InstallDefinitionType(seed);
                foreach (var seed in captures.Values) DeclareValueSeed(seed);
                foreach (var (local, hasValue, value, inner) in captures.Optionals)
                {
                    var symbol = _symbols.Declare(new Symbol { Name = local, Kind = SymKind.Var, Type = new CType.Optional(inner) });
                    _comptimeOptionalVars[symbol] = (hasValue, value, inner);
                }
                foreach (var (local, value, type) in captures.Aggregates)
                {
                    var symbol = _symbols.Declare(new Symbol { Name = local, Kind = SymKind.Var, Type = type });
                    _ir.ComptimeGlobals[symbol] = value;
                }
                foreach (var (local, owner, function) in captures.Functions) _fnAliases[local] = (owner, function);
            }
            return scope;
        }
        catch { scope.Dispose(); throw; }
    }
}
