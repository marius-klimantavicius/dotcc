#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using DotCC.Ir;
using LALR.CC.LexicalGrammar;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    private readonly HashSet<string> _reflectionCallsInProgress = new(StringComparer.Ordinal);

    // A source helper returning the standard Struct reflection payload has no
    // runtime value or ABI. Reuse the comptime body walker so its validation
    // statements execute before its returned metadata is accepted.
    private bool TryEvalSourceReflectionCall(Zig.CallArgs call, out ZigTypeInfo result)
    {
        result = null!;
        if (!UsesSourceStdlib) return false;
        // A speculative metadata query must not declare receiver methods or
        // instantiate ordinary generic function types.
        var target = call.Arg0.Content is Zig.Ident ? TryResolveFnAlias(call.Arg0) : null;
        if (target is null && call.Arg0.Content is Zig.Field field
            && ResolveModulePath(field.Arg0)?.Lowering is { } module)
            target = module.ResolveExportedDecl(Tok(field.Arg2));
        if (target is null && call.Arg0.Content is Zig.Ident identifier)
        {
            var name = Tok(identifier.Arg0);
            var symbol = _symbols.Resolve(name);
            if (symbol is null && _lazy) symbol = EnsureDeclLowered(name);
            if (symbol is null && _registeringRootContainers) symbol = DeclareRootFnEarly(name);
            if (symbol is not null) target = (this, symbol);
        }
        if (target is not { } found || !found.Owner._genericFns.TryGetValue(found.Sym, out var generic)
            || generic.ErrUnion || generic.Params.Any(p => p.Kind != ParamKind.ComptimeType)
            || !found.Owner.TryResolveStdPath(generic.RetType, out var returnPath)
            || returnPath is not ("std.lang.Type.Struct" or "std.builtin.Type.Struct")) return false;
        var arguments = Flatten(call.Arg2);
        if (arguments.Count != generic.Params.Count)
            throw new IrUnsupportedException($"source reflection helper '{found.Sym.Name}' expects {generic.Params.Count} arguments, got {arguments.Count}");
        var seeds = arguments.Select((argument, i) => SelectedType(argument, LowerType(argument))
            with { Name = generic.Params[i].Name }).ToArray();
        result = found.Owner.EvalSourceReflectionBody(generic, seeds);
        return true;
    }

    private ZigTypeInfo EvalSourceReflectionBody(GenericFnInfo generic, IReadOnlyList<TypeSeed> seeds)
    {
        var key = generic.Template.Name + "__" + string.Join("_", seeds.Select(MangleTypeSeed));
        if (!_reflectionCallsInProgress.Add(key))
            throw new IrUnsupportedException($"source reflection helper '{generic.Template.Name}' has a comptime dependency loop");
        var previousInfos = new Dictionary<string, ZigTypeInfo>(_typeInfoBindings, StringComparer.Ordinal);
        using var definition = new DefinitionBindingScope(this);
        using var container = EnterContainer(generic.Owner);
        using var ownerSeeds = EnterReifiedSeeds(generic.Owner ?? "");
        using var symbols = EnterSymbolScope();
        try
        {
            // Caller-local reflection names are not in the helper's lexical scope.
            _typeInfoBindings.Clear();
            foreach (var seed in seeds) InstallDefinitionType(seed);
            var value = ProcessTypeReturningBody(key, generic.Body, new(), returnsReflection: true).Reflection
                ?? throw new IrUnsupportedException($"source reflection helper '{generic.Template.Name}' did not return reflection metadata");
            if (value.Tag != "struct")
                throw new IrUnsupportedException($"source reflection helper '{generic.Template.Name}' returned '{value.Tag}', expected the Struct payload");
            return value;
        }
        finally
        {
            _typeInfoBindings.Clear();
            foreach (var (name, info) in previousInfos) _typeInfoBindings[name] = info;
            _reflectionCallsInProgress.Remove(key);
        }
    }
}
