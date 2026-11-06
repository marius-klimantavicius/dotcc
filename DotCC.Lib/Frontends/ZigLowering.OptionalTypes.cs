#nullable enable
using System.Collections.Generic;
using DotCC.Ir;
using LALR.CC.LexicalGrammar;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    // Selection evidence belongs to the expression site, like type-returning call
    // evidence. Never recover a logical width from the widened managed carrier.
    private readonly Dictionary<Item, TypeSeed> _optionalTypeSelections = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<Symbol, TypeSeed?> _optionalTypeLocals = new();

    private void DeclareOptionalTypeSeed(string name, TypeSeed? value)
    {
        var symbol = _symbols.Declare(new Symbol { Name = name, Kind = SymKind.Var,
            Type = new CType.Optional(new CType.ZigMetaType()) });
        _optionalTypeLocals[symbol] = value;
        _ir.ComptimeGlobals[symbol] = value is { } present
            ? new IrBuilder.CtType(present.Type) : new IrBuilder.CtNull(symbol.Type);
        _zigConstBindings.Add(symbol);
    }

    private static bool IsOptionalTypeAnnotation(Item? annotation)
        => annotation?.Content is Zig.TyOptional optional && IsTypeKeyword(optional.Arg1);

    private TypeSeed SelectedType(Item expression, CType type)
        => new("", type, DeclaredBitsOfTypeArg(expression), PointerSizeOfTypeArg(expression), SentinelOfTypeArg(expression));

    // This recognizer only claims optional TYPE values. Ordinary optional values
    // continue through the existing value orelse path.
    private bool TryOptionalTypeValue(Item expression, out TypeSeed? value)
    {
        value = null;
        switch (expression.Content)
        {
            case Zig.Grouped grouped: return TryOptionalTypeValue(grouped.Arg1, out value);
            case Zig.IfElseExpr { Arg4: null, Arg7: null } conditional when TryFoldTypeIfCondition(conditional.Arg2) is { } taken:
                if (!TryOptionalTypeValue(taken ? conditional.Arg5 : conditional.Arg8, out value)) return false;
                _ = OptionalTypeCondition(conditional.Arg2);
                return true;
            case Zig.NullLit: return true;
            case Zig.BuiltinCall call when Tok(call.Arg0) == "@as" && Flatten(call.Arg2) is [var annotation, var initializer]
                && IsOptionalTypeAnnotation(annotation):
                value = ReadOptionalTypeInitializer(initializer);
                return true;
            case Zig.Ident id:
            {
                var name = Tok(id.Arg0);
                if (_symbols.Resolve(name) is { } symbol)
                {
                    if (symbol.Type.Unqualified is not CType.Optional { Inner: CType.ZigMetaType }) return false;
                    if (_optionalTypeLocals.TryGetValue(symbol, out value)) return true;
                    value = _ir.EvalComptimeValue(new VarRef(symbol) { Type = symbol.Type }) switch
                    {
                        IrBuilder.CtNull => null,
                        IrBuilder.CtType type => new TypeSeed("", type.Value, null),
                        _ => throw new IrUnsupportedException("zig: optional type value must be comptime-known"),
                    };
                    return true;
                }
                for (var container = _currentContainer; container is not null; container = _containerParents.GetValueOrDefault(container))
                    if (TryOptionalTypeConst(container, name, out value)) return true;
                return TryOptionalTypeConst(null, name, out value);
            }
            case Zig.Field backing when Tok(backing.Arg2) == "backing_integer"
                && TryEvalTypeInfo(backing.Arg0, out var backingInfo) && backingInfo.Tag is "struct" or "union":
                value = ReflectedPackedBackingType(backingInfo);
                return true;
            case Zig.Field field when Tok(field.Arg2) == "tag_type" && TryEvalTypeInfo(field.Arg0, out var info) && info.Tag == "union":
                if (info.Type.Unqualified is not CType.Named union)
                    throw new IrUnsupportedException("zig: optional union tag type has no source identity");
                if (_unions.TryGetValue(union.Name, out var tagged))
                    value = SelectedType(expression, tagged.TagType);
                return true;
            case Zig.Field field:
            {
                var name = Tok(field.Arg2);
                if (ResolveModulePath(field.Arg0)?.Lowering is { } module)
                    return module.TryOptionalTypeConst(null, name, out value);
                var type = TryResolveModuleNestedType(field.Arg0)?.Type ?? MemberBaseType(field.Arg0);
                if (type is not null && ContainerTypeName(type) is { } container)
                {
                    var owner = _moduleGraph?.OwnerOfContainer(container) ?? this;
                    return owner.TryOptionalTypeConst(container, name, out value);
                }
                return false;
            }
            default: return false;
        }
    }

    private bool TryOptionalTypeCaptureValue(Item expression, out TypeSeed? value)
    {
        if (!TryOptionalTypeValue(expression, out value)) return false;
        // Null alone does not prove a ?type condition. Keep ordinary optional
        // values on their existing capture path; this projection is comptime-only.
        using var hoist = EnterThrowawayHoist();
        return LowerExpr(expression).Type.Unqualified is CType.Optional { Inner: CType.ZigMetaType };
    }

    private ReifiedSeedScope EnterOptionalTypeCapture(string name, TypeSeed value)
    {
        _symbols.EnterScope();
        var shadows = new List<(string Name, CType? Prev, int? PrevBits, string? PrevPtrSize, ZigSentinel? PrevSentinel)>();
        if (name != "_")
        {
            shadows.Add((name, _typeAliases.GetValueOrDefault(name), _declaredIntBits.TryGetValue(name, out var bits) ? bits : null,
                _declaredPtrSize.GetValueOrDefault(name), _declaredSentinel.GetValueOrDefault(name)));
            InstallDefinitionType(value with { Name = name });
        }
        return new ReifiedSeedScope(this, shadows);
    }

    private bool TryOptionalTypeConst(string? container, string name, out TypeSeed? value)
    {
        value = null;
        (Item? annotation, Item rhs) entry;
        if (container is not null)
        {
            if (!_containerConsts.TryGetValue(container, out var consts) || !consts.TryGetValue(name, out entry)) return false;
        }
        else if (!_lazyValueConsts.TryGetValue(name, out entry)) return false;
        if (entry.annotation is not null && !IsOptionalTypeAnnotation(entry.annotation)) return false;
        var key = "?type:" + container + "." + name;
        if (!_constResolving.Add(key)) throw new IrUnsupportedException("zig: optional type constant has a dependency loop: " + name);
        using var definition = new DefinitionBindingScope(this);
        using var scope = EnterContainer(container);
        using var constScope = EnterConstContainer(container);
        using var seeds = container is not null ? EnterReifiedSeeds(container) : default;
        try
        {
            if (entry.annotation is not null)
            {
                value = ReadOptionalTypeInitializer(entry.rhs);
                return true;
            }
            return TryOptionalTypeValue(entry.rhs, out value);
        }
        finally { _constResolving.Remove(key); }
    }

    private TypeSeed? ReadOptionalTypeInitializer(Item expression)
    {
        if (expression.Content is Zig.Grouped grouped) return ReadOptionalTypeInitializer(grouped.Arg1);
        if (expression.Content is Zig.IfElseExpr { Arg4: null, Arg7: null } conditional)
        {
            var taken = OptionalTypeCondition(conditional.Arg2);
            return ReadOptionalTypeInitializer(taken ? conditional.Arg5 : conditional.Arg8);
        }
        if (TryOptionalTypeValue(expression, out var optional)) return optional;
        if (expression.Content is Zig.Ident id && ResolveContainerTypeConst(Tok(id.Arg0)) is { } memberType)
            return SelectedType(expression, memberType);
        if (TryTypeAliasRhs(expression, out var type)) return SelectedType(expression, type);
        // The optional payload is a TYPE position: imported container aliases
        // such as api.Parent.C use the same proved nested-type lookup as LowerType.
        if (expression.Content is Zig.Field && TryResolveModuleNestedType(expression) is { } nested)
            return SelectedType(expression, nested.Type);
        if (expression.Content is Zig.BuiltinCall call && Tok(call.Arg0) == "@compileError")
            CompileErrorBuiltin(Flatten(call.Arg2));
        throw new IrUnsupportedException("zig: optional type initializer must be a comptime type or null"
            + $" ({_module?.Path ?? _fileStem}:{expression.Position.Line}, {expression.Content?.GetType().Name}, function '{_currentFnName}')");
    }

    private bool? TryFoldOptionalTypeNullEquality(Item left, Item right)
    {
        while (left.Content is Zig.Grouped groupedLeft) left = groupedLeft.Arg1;
        while (right.Content is Zig.Grouped groupedRight) right = groupedRight.Arg1;
        var optional = left.Content is Zig.NullLit ? right
            : right.Content is Zig.NullLit ? left : null;
        // Only a proved ?type has no runtime carrier. Ordinary optional values
        // retain their normal equality and operand evaluation paths.
        return optional is not null && optional.Content is not Zig.NullLit
            && TryOptionalTypeValue(optional, out var value) ? value is null : null;
    }

    private bool OptionalTypeCondition(Item condition)
    {
        using var hoist = EnterThrowawayHoist();
        if (!IsZigBoolValue(LowerExpr(condition)))
            throw new IrUnsupportedException("zig: optional type condition must be bool");
        return TryFoldRequiredComptimeCondition(condition)
            ?? throw new IrUnsupportedException("zig: optional type condition must be comptime-known");
    }

    private CType SelectOptionalType(Item occurrence, TypeSeed? present, Item fallback)
    {
        // Evaluate only the selected expression, in its defining owner above.
        var selected = present ?? SelectedType(fallback, LowerType(fallback));
        _optionalTypeSelections[occurrence] = selected;
        return selected.Type;
    }
}
