#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DotCC.Ir;
using LALR.CC.LexicalGrammar;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    private IrBuilder.CtEnumLiteral ComptimeEnumLiteralArgument(Item argument)
    {
        using var discarded = EnterThrowawayHoist();
        var value = LowerExpr(argument);
        if (_ir.EvalComptimeValue(value) is IrBuilder.CtEnumLiteral literal) return literal;
        if (value.Type.Unqualified is CType.EnumLiteral { Name.Length: > 0 } type) return new(type.Name);
        throw new CompileException("zig: @EnumLiteral() argument requires a compile-time-known enum literal");
    }
    private sealed record SourceFunctionType(ZigLowering Owner, Item Original, Zig.TyFn Definition,
        string? Container, InlineCaptures Captures);
    private static readonly ConditionalWeakTable<IrBuilder, Dictionary<CType.ZigGenericFunctionType, SourceFunctionType>> SourceFunctionTypes = new();
    private static readonly ConditionalWeakTable<IrBuilder, Dictionary<Symbol, ZigLowering>> SourceFunctionOwners = new();
    private readonly Dictionary<Item, CType.ZigGenericFunctionType> _sourceFunctionTypes = new(ReferenceEqualityComparer.Instance);

    private CType SourceGenericFunctionType(Item original, Zig.TyFn function)
    {
        if (_sourceFunctionTypes.TryGetValue(original, out var known)) { return known; }
        if (IsCCallConv(function.Arg4))
            throw new CompileException("zig: a generic function type cannot use the C calling convention");
        var type = new CType.ZigGenericFunctionType(QualifyTypeName("__GenericFunctionType" + _sourceFunctionTypes.Count));
        _sourceFunctionTypes.Add(original, type);
        SourceFunctionTypes.GetOrCreateValue(_ir).Add(type,
            new(this, original, function, _currentContainer, CaptureInlineEnvironment(original)));
        return type;
    }

    private CExpr LowerSourceGenericFunctionValue(Item expression, CType.ZigGenericFunctionType expected)
    {
        if (expression.Content is Zig.UndefinedLit) { return new DefaultLit { Type = expected }; }
        if (expression.Content is Zig.Field field && ResolveModulePath(field.Arg0)?.Lowering is { } module)
            module.RaiseIfSkippedDecl(Tok(field.Arg2));
        var function = TryResolveFnAlias(expression)
            ?? throw new CompileException($"zig: a generic function field must name a source function ({_fileStem}:{expression.Position.Line}, {expression.Content?.GetType().Name})");
        if (!SourceFunctionTypes.GetOrCreateValue(_ir).TryGetValue(expected, out var declared)
            || !function.Owner._genericFns.TryGetValue(function.Sym, out var actual))
            throw new CompileException("zig: a generic function field requires a compatible generic source function");
        var parameters = Flatten(declared.Definition.Arg2);
        if (parameters.Count != actual.Params.Count || (declared.Definition.Arg5 is not null) != actual.ErrUnion)
            throw new CompileException("zig: incompatible generic function signature");
        using var targetScope = declared.Owner.EnterPackedSource(declared.Container ?? "", new(declared.Owner, null, declared.Captures));
        using var sourceScope = function.Owner == declared.Owner ? null : new DefinitionBindingScope(function.Owner);
        for (var i = 0; i < parameters.Count; i++)
        {
            var (typeItem, comptime) = parameters[i].Content switch
            {
                Zig.FnTypeParamNamed p => (p.Arg2, false),
                Zig.FnTypeParamUnnamed p => (p.Arg0, false),
                Zig.FnTypeParamComptime p => (p.Arg3, true),
                Zig.FnTypeParamComptimeUnnamed p => (p.Arg1, true),
                _ => throw new IrUnsupportedException("zig: unsupported parameter in generic function field type"),
            };
            var parameter = actual.Params[i];
            if (comptime != parameter.IsComptime || IsAnyTypeKeyword(typeItem) != (parameter.Kind == ParamKind.AnyType))
                throw new CompileException("zig: incompatible comptime/anytype function parameter");
            if (IsAnyTypeKeyword(typeItem)) { continue; }
            var target = declared.Owner.LowerType(typeItem);
            var source = function.Owner.LowerType(parameter.TypeAst);
            if (!EquivalentFunctionParameterType(target, source)
                || declared.Owner.DeclaredBitsOfTypeArg(typeItem) != function.Owner.DeclaredBitsOfTypeArg(parameter.TypeAst))
                throw new CompileException("zig: incompatible generic function parameter type");
        }
        if (!EquivalentFunctionParameterType(declared.Owner.LowerType(declared.Definition.Arg6), function.Owner.LowerType(actual.RetType)))
            throw new CompileException("zig: incompatible generic function return type");
        _ir.ZigComptimeFunctionValues.Add(function.Sym);
        SourceFunctionOwners.GetOrCreateValue(_ir)[function.Sym] = function.Owner;
        return new VarRef(function.Sym) { Type = expected };
    }

    private CExpr CallComptimeFunctionField(CExpr receiver, string name, CType.ZigGenericFunctionType type, IReadOnlyList<Item> arguments)
    {
        var value = _ir.EvalComptimeValue(new Member(receiver, name, false) { Type = type });
        if (value is IrBuilder.CtUndefined) throw new CompileException("zig: use of undefined generic function field " + name);
        if (value is not IrBuilder.CtFunction function
            || !SourceFunctionOwners.GetOrCreateValue(_ir).TryGetValue(function.Function, out var owner))
            throw new CompileException("zig: generic function field must be comptime-known: " + name);
        return CallExportedDecl(owner, function.Function, arguments);
    }

    private static bool EquivalentFunctionParameterType(CType a, CType b)
        => a.Unqualified is CType.EnumLiteral && b.Unqualified is CType.EnumLiteral || a == b;

    private bool RequiresComptimeStorage(CType type) => RequiresComptimeStorage(type, new(StringComparer.Ordinal));
    private bool RequiresComptimeStorage(CType type, HashSet<string> active) => type.Unqualified switch
    {
        CType.ZigMetaType or CType.ZigGenericFunctionType or CType.EnumLiteral => true,
        CType.Array array => RequiresComptimeStorage(array.Element, active),
        CType.Slice slice => RequiresComptimeStorage(slice.Element, active),
        CType.Pointer pointer => RequiresComptimeStorage(pointer.Pointee, active),
        CType.Optional optional => RequiresComptimeStorage(optional.Inner, active),
        CType.Named named when _ir.ZigComptimeFunctionFields.Any(f => f.Owner == named.Name) => true,
        CType.Named named when active.Add(named.Name) && _ir.StructFieldsOf(named.Name) is { } fields
            => fields.Any(field => RequiresComptimeStorage(field.Type, active)),
        _ => false,
    };

    private CExpr ReadComptimeAggregateField(CExpr receiver, string field)
    {
        var type = _ir.StructFieldType(receiver.Type, field)
            ?? throw new CompileException("zig: comptime aggregate has no field " + field);
        var access = new Member(receiver, field, false) { Type = type };
        var value = _ir.EvalComptimeValue(access)
            ?? throw new CompileException("zig: comptime aggregate field is not known: " + field);
        if (value is IrBuilder.CtUndefined) { throw new CompileException("zig: use of undefined comptime field " + field); }
        if (type.Unqualified is CType.Enum enumeration && value is IrBuilder.CtEnumLiteral literal)
            return ResolveEnumLit(literal.Name, enumeration);
        var result = _ir.SpliceComptimeValue(value)
            ?? throw new IrUnsupportedException("zig: comptime aggregate field has no constant form: " + field);
        // Interpreter payloads can carry a promoted integer, or the payload of
        // a present optional. The source field still supplies its nominal type.
        return result.Type == type ? result : new Cast(type, result) { Type = type };
    }

    private CType? TryReadTypeValueField(Zig.Field field)
    {
        var root = field.Arg0;
        while (root.Content is Zig.Field parent) root = parent.Arg0;
        if (root.Content is not Zig.Ident id || _symbols.Resolve(Tok(id.Arg0)) is null) return null;
        using var hoist = EnterThrowawayHoist();
        var receiver = LowerExpr(field.Arg0);
        var name = Tok(field.Arg2);
        if (_ir.StructFieldType(receiver.Type, name)?.Unqualified is not CType.ZigMetaType) return null;
        return _ir.EvalComptimeValue(new Member(receiver, name, false) { Type = new CType.ZigMetaType() }) is IrBuilder.CtType value
            ? value.Value : throw new CompileException("zig: type-valued field is not comptime-known: " + name);
    }

    private bool IsComptimeGlobalReference(CExpr expression) => expression switch
    {
        VarRef variable => _ir.ComptimeGlobals.ContainsKey(variable.Sym),
        Member field => IsComptimeGlobalReference(field.Base),
        ComptimeFold fold => IsComptimeGlobalReference(fold.Inner),
        Paren parenthesized => IsComptimeGlobalReference(parenthesized.Inner),
        _ => false,
    };

    internal static void EraseSourceComptimeTypeDefinitions(IrBuilder ir)
    {
        bool Contains(CType type, HashSet<string> active) => type.Unqualified switch
        {
            CType.ZigMetaType or CType.ZigGenericFunctionType or CType.EnumLiteral => true,
            CType.Array a => Contains(a.Element, active),
            CType.Slice s => Contains(s.Element, active),
            CType.Pointer p => Contains(p.Pointee, active),
            CType.Optional o => Contains(o.Inner, active),
            CType.Tuple t => t.Elements.Any(e => Contains(e, active)),
            CType.Func f => Contains(f.Return, active) || f.Params.Any(p => Contains(p, active)),
            CType.Named n when ir.ZigComptimeFunctionFields.Any(f => f.Owner == n.Name) => true,
            CType.Named n when active.Add(n.Name) && ir.StructFieldsOf(n.Name) is { } fields
                => fields.Any(f => Contains(f.Type, active)),
            _ => false,
        };
        if (ir.Globals.Any(g => Contains(g.Sym.Type, new(StringComparer.Ordinal)))
            || ir.Functions.Any(f => Contains(f.Sym.Type, new(StringComparer.Ordinal))))
            throw new CompileException("zig: a comptime-only aggregate escaped into runtime storage or a function signature");
        ir.Types.RemoveAll(t => Contains(new CType.Named(t.Name), new(StringComparer.Ordinal)));
    }
}
