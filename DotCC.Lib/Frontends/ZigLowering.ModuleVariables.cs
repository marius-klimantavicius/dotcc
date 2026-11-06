#nullable enable
using System.Collections.Generic;
using DotCC.Ir;
using LALR.CC.LexicalGrammar;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    // Source names are local to this owner. Only the storage's emitted identity
    // is qualified; all references share the same symbol across body scopes.
    private readonly Dictionary<string, (Item Name, Item? Type, Item Rhs, bool ThreadLocal)> _moduleVariables = new(System.StringComparer.Ordinal);
    private readonly Dictionary<string, Symbol> _moduleVariableStorage = new(System.StringComparer.Ordinal);
    private readonly HashSet<string> _moduleVariablesInProgress = new(System.StringComparer.Ordinal);

    private void IndexModuleVariables(IReadOnlyList<Item> declarations)
    {
        foreach (var declaration in declarations)
        {
            // Extern data has no managed definition. Index it without demanding
            // unrelated storage, and retain the explicit rejection on use.
            switch (Unwrap(declaration).Content)
            {
                case Zig.ExternVar d: _externVars.Add(Tok(d.Arg5)); break;
                case Zig.ExternConst d: _externVars.Add(Tok(d.Arg4)); break;
            }
            (Item Name, Item? Type, Item Rhs, bool ThreadLocal)? source = Unwrap(declaration).Content switch
            {
                Zig.VarDecl d => (d.Arg1, null, d.Arg3, false),
                Zig.VarDeclTyped d => (d.Arg1, d.Arg3, d.Arg5, false),
                Zig.VarDeclMods d => (d.Arg1, null, d.Arg4, false),
                Zig.VarDeclTypedMods d => (d.Arg1, d.Arg3, d.Arg6, false),
                Zig.VarDeclThreadLocal d => (d.Arg2, d.Arg4, d.Arg6, true),
                _ => null,
            };
            if (source is { } variable) _moduleVariables[Tok(variable.Name)] = variable;
        }
    }

    private Symbol? EnsureModuleVariable(string name)
    {
        if (_moduleVariableStorage.TryGetValue(name, out var storage)) return storage;
        if (_externVars.Contains(name))
            throw new IrUnsupportedException($"zig: `{name}` is extern data; linked extern data is not supported");
        if (!_moduleVariables.TryGetValue(name, out var source)) return null;
        if (!_moduleVariablesInProgress.Add(name))
            throw new IrUnsupportedException($"zig: top-level `var {name}` has an unsupported initializer dependency loop");
        try
        {
            // A demand may arrive from this owner's method or generic instance.
            // Its locals, type/value seeds and container must not bind the initializer.
            using var definition = new DefinitionBindingScope(this);
            using var function = new FnStateScope(this);
            LowerGlobal(source.Name, source.Type, source.Rhs, threadLocal: source.ThreadLocal);
            storage = _symbols.Resolve(name)
                ?? throw new IrUnsupportedException($"zig: top-level `var {name}` did not produce runtime storage");
            if (_modulePrefix is { } prefix) storage.TargetName = _names.Escape($"{prefix}__{name}");
            _moduleVariableStorage.Add(name, storage);
            return storage;
        }
        finally { _moduleVariablesInProgress.Remove(name); }
    }

    private CExpr? LowerModuleVariable(string name) => EnsureModuleVariable(name) is { } storage
        ? new VarRef(storage) { Type = storage.Type, IsLValue = true }
        : null;
}
