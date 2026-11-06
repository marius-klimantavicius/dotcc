#nullable enable

using System.Collections.Generic;
using DotCC.Ir;
using LALR.CC.LexicalGrammar;

namespace DotCC.Frontends;

/// <summary>A zig pointer's SIZE class (<c>@typeInfo(P).pointer.size</c>: <c>.one</c>, <c>.many</c>, <c>.c</c>) where
/// the source spelled it (task #119). dotcc lowers <c>*T</c>, <c>[*]T</c> and <c>[*c]T</c> to one C pointer, so the
/// class is not recoverable from the lowered <see cref="CType"/>; like a declared integer width
/// (<see cref="_declaredIntBits"/>) it rides ALONGSIDE the type, read from the spelling: a type alias, a typed local,
/// a parameter, <c>&amp;x</c> (always a single-item pointer), and an <c>anytype</c> argument (std.Random.init's
/// <c>assert(@typeInfo(Ptr).pointer.size == .one)</c> over the <c>*Xoshiro256</c> it is handed). A pointer whose class
/// no spelling gives stays the loud cut in <see cref="TryFoldTypeInfoValue"/>. (A slice's class, <c>.slice</c>, is its
/// own <see cref="CType.Slice"/>.)</summary>
internal sealed partial class ZigLowering
{
    /// <summary>Each type-binding name → the size class of the pointer type bound to it (<c>const Ptr = @TypeOf(pointer);</c>),
    /// set or cleared with every alias binding, like <see cref="_declaredIntBits"/>.</summary>
    private readonly Dictionary<string, string> _declaredPtrSize = new(System.StringComparer.Ordinal);

    /// <summary>Each pointer-valued symbol's size class, where its declaration spelled one (a typed local or a parameter)
    /// or its initializer carries one (<c>&amp;x</c>).</summary>
    private readonly Dictionary<Symbol, string> _valuePtrSize = new();

    /// <summary>Each generic `anytype` parameter's size class while its instance's SIGNATURE lowers (the
    /// <see cref="_anytypeSeedBits"/> counterpart; shadow-restored with it).</summary>
    private readonly Dictionary<string, string> _anytypeSeedPtrSize = new(System.StringComparer.Ordinal);

    /// <summary>Each generic instance's `anytype` parameter size classes, read from the call site's arguments; the
    /// instance BODY gives them to the parameter symbols (<see cref="RecordParamBits"/>).</summary>
    private readonly Dictionary<Symbol, Dictionary<string, string>> _instanceAnytypePtrSize = new();

    /// <summary>The names of the type aliases the body being lowered declares (<c>const Ptr = @TypeOf(pointer);</c>), in
    /// order, cleared per body: an in-function struct's deferred methods take them as seeds (task #124).</summary>
    private readonly List<string> _bodyTypeAliases = new();

    /// <summary>Set or clear a type-binding name's pointer size class (see <see cref="_declaredPtrSize"/>).</summary>
    private void SetDeclaredPtrSize(string name, string? size)
    {
        if (size is { } s) { _declaredPtrSize[name] = s; } else { _declaredPtrSize.Remove(name); }
    }

    /// <summary>The size class a pointer TYPE's AST spells, or null: the pointer prefix itself, a name bound to one
    /// (<see cref="_declaredPtrSize"/>), or <c>@TypeOf(v)</c> of a value whose class is known.</summary>
    private string? PointerSizeOfTypeArg(Item typeAst) => typeAst.Content switch
    {
        Zig.Grouped g => PointerSizeOfTypeArg(g.Arg1),
        Zig.OrElse when _optionalTypeSelections.TryGetValue(typeAst, out var selected) => selected.PointerSize,
        Zig.TyOptional optional => PointerSizeOfTypeArg(optional.Arg1),
        Zig.ErrUnion error => PointerSizeOfTypeArg(error.Arg2),
        Zig.TyPointer or Zig.TyPtrConst or Zig.TyPointerAlign or Zig.TyPtrConstAlign => "one",
        Zig.TyManyPtr or Zig.TyManyPtrConst or Zig.TyManyPtrAlign or Zig.TyManyPtrConstAlign
            or Zig.TySentPtr or Zig.TySentPtrConst or Zig.TySentPtrExpr or Zig.TySentPtrConstExpr
            or Zig.TySentPtrAlign or Zig.TySentPtrConstAlign => "many",
        Zig.TyCPtr or Zig.TyCPtrConst => "c",
        Zig.Ident id => _declaredPtrSize.GetValueOrDefault(Tok(id.Arg0))
            ?? (_typeAliases.GetValueOrDefault(Tok(id.Arg0))?.Unqualified as CType.Pointer)?.ZigSourceSize,
        Zig.Field field when ResolveModulePath(field.Arg0)?.Lowering is { } owner => owner.ExportedPointerSize(Tok(field.Arg2)),
        Zig.BuiltinCall { Arg2: not null } tof when Tok(tof.Arg0) == "@TypeOf" && Flatten(tof.Arg2) is [var v] =>
            v.Content is Zig.Ident vid && _anytypeSeedPtrSize.TryGetValue(Tok(vid.Arg0), out var seeded) ? seeded : PointerSizeOfValue(v),
        _ => null,
    };

    /// <summary>The size class of a pointer VALUE, or null when nothing spelled it: <c>&amp;x</c> is a single-item
    /// pointer (to an array too: <c>*[N]T</c>), a string literal a pointer to its array (<c>*const [N:0]u8</c>), a slice's
    /// <c>.ptr</c> a many-item one; a symbol answers what its declaration recorded, and <c>@as(T, v)</c> what <c>T</c> spells.</summary>
    private string? PointerSizeOfValue(Item e) => e.Content switch
    {
        Zig.Grouped g => PointerSizeOfValue(g.Arg1),
        Zig.OrElse optional => PointerSizeOfValue(optional.Arg0),
        Zig.Unwrap optional => PointerSizeOfValue(optional.Arg0),
        Zig.PreTry error => PointerSizeOfValue(error.Arg1),
        Zig.PreAddrOf or Zig.StrLit => "one",
        Zig.BuiltinCall b when Tok(b.Arg0) == "@embedFile" => "one",
        Zig.Ident id => _symbols.Resolve(Tok(id.Arg0)) is { } s
            ? _valuePtrSize.GetValueOrDefault(s) ?? (s.Type.Unqualified as CType.Pointer)?.ZigSourceSize : null,
        Zig.Field f when Tok(f.Arg2) == "ptr" && ValueTypeForPointerSize(f.Arg0)?.Unqualified is CType.Slice or CType.Pointer { Pointee.Unqualified: CType.Slice } => "many",
        Zig.BuiltinCall { Arg2: not null } bc when Tok(bc.Arg0) == "@as" && Flatten(bc.Arg2) is [var asType, _] => PointerSizeOfTypeArg(asType),
        _ => null,
    };

    /// <summary>The type of a VALUE expression, for <see cref="PointerSizeOfValue"/>'s <c>.ptr</c> case: a symbol's declared
    /// type, else the expression lowered into a throwaway hoist (a question about its type; the use site lowers it itself).</summary>
    private CType? ValueTypeForPointerSize(Item e)
    {
        if (e.Content is Zig.Ident id && _symbols.Resolve(Tok(id.Arg0)) is { } sym) { return sym.Type; }
        // A container namespace is not a value receiver. Its static declarations
        // have their own source metadata; do not lower the namespace as a value.
        if (e.Content is Zig.Ident container && TryLookupContainerType(Tok(container.Arg0), out _)) return null;
        using var hoist = EnterThrowawayHoist();
        return LowerExpr(e).Type;
    }

    /// <summary>Record a pointer-valued symbol's size class, when one is known (see <see cref="_valuePtrSize"/>).</summary>
    private void RecordValuePtrSize(Symbol sym, string? size)
    {
        if (size is { } s) { _valuePtrSize[sym] = s; }
    }

    /// <summary>A pointer, slice or array type's SENTINEL as its spelling gives it (task #213, <c>@typeInfo(T).pointer.sentinel()</c>).
    /// dotcc erases the sentinel in the lowered type (<c>[:0]u8</c> is the slice <c>[]u8</c> is), so like the size class it
    /// rides alongside: <see cref="HasSentinel"/> false for a type spelled without one (<c>[]const u8</c>), true with the
    /// integer <see cref="Value"/> for <c>[:0]T</c> / <c>[:s]T</c> / <c>[N:s]T</c>, and a null value for a sentinel that is
    /// not an integer (<c>[*:null]?*T</c>). A type no spelling describes has no <see cref="ZigSentinel"/> at all.</summary>
    internal sealed record ZigSentinel(bool HasSentinel, long? Value);

    /// <summary>Each type-binding name → the sentinel of the type bound to it, set and cleared alongside
    /// <see cref="_declaredPtrSize"/> (a type alias, a generic's type seed, a comptime <c>field_types</c> capture).</summary>
    private readonly Dictionary<string, ZigSentinel> _declaredSentinel = new(System.StringComparer.Ordinal);

    /// <summary>Set or clear a type-binding name's sentinel (see <see cref="_declaredSentinel"/>).</summary>
    private void SetDeclaredSentinel(string name, ZigSentinel? sentinel)
    {
        if (sentinel is { } s) { _declaredSentinel[name] = s; } else { _declaredSentinel.Remove(name); }
    }

    /// <summary>The sentinel a pointer, slice or array TYPE's AST spells (see <see cref="ZigSentinel"/>), or null when the
    /// spelling does not say: a name bound to a type answers what its binding recorded.</summary>
    private ZigSentinel? SentinelOfTypeArg(Item typeAst) => typeAst.Content switch
    {
        Zig.Grouped g => SentinelOfTypeArg(g.Arg1),
        Zig.IfElseExpr { Arg4: null, Arg7: null } conditional when TryFoldTypeIfCondition(conditional.Arg2) is { } taken
            => SentinelOfTypeArg(taken ? conditional.Arg5 : conditional.Arg8),
        Zig.IfElseExpr conditional when SentinelOfTypeArg(conditional.Arg5) is { } same
            && SentinelOfTypeArg(conditional.Arg8) == same => same,
        Zig.OrElse when _optionalTypeSelections.TryGetValue(typeAst, out var selected) => selected.Sentinel,
        Zig.ErrUnion error => SentinelOfTypeArg(error.Arg2),
        Zig.TyOptional optional => SentinelOfTypeArg(optional.Arg1),
        Zig.TySlice or Zig.TySliceConst or Zig.TySliceAlign or Zig.TySliceConstAlign
            or Zig.TyPointer or Zig.TyPtrConst or Zig.TyPointerAlign or Zig.TyPtrConstAlign
            or Zig.TyManyPtr or Zig.TyManyPtrConst or Zig.TyManyPtrAlign or Zig.TyManyPtrConstAlign
            or Zig.TyCPtr or Zig.TyCPtrConst or Zig.TyArray => new ZigSentinel(false, null),
        Zig.TySentSlice or Zig.TySentSliceConst or Zig.TySentPtr or Zig.TySentPtrConst
            or Zig.TySentPtrAlign or Zig.TySentPtrConstAlign => new ZigSentinel(true, 0),
        Zig.TySentSliceExpr s           => SpelledSentinel(s.Arg2),
        Zig.TySentSliceConstExpr s      => SpelledSentinel(s.Arg2),
        Zig.TySentSliceAlignExpr s      => SpelledSentinel(s.Arg2),
        Zig.TySentSliceConstAlignExpr s => SpelledSentinel(s.Arg2),
        Zig.TySentPtrExpr p             => SpelledSentinel(p.Arg3),
        Zig.TySentPtrConstExpr p        => SpelledSentinel(p.Arg3),
        Zig.TySentArray a               => SpelledSentinel(a.Arg3),
        Zig.Ident id => _declaredSentinel.GetValueOrDefault(Tok(id.Arg0)) ?? ContainerTypeConstSentinel(Tok(id.Arg0)),
        Zig.Field field when ResolveModulePath(field.Arg0)?.Lowering is { } owner => owner.ExportedSentinel(Tok(field.Arg2)),
        // Array child types retain their source sentinel in CType.Array. This
        // includes a proved plain array; pointer carriers cannot supply this proof.
        Zig.Field field when Tok(field.Arg2) == "child"
            && LowerType(typeAst).Unqualified is CType.Array childArray => SourceBackedArraySentinel(childArray),
        Zig.BuiltinCall b when Tok(b.Arg0) == "@TypeOf" && Flatten(b.Arg2) is [var value]
            => SentinelOfValue(value) ?? SourceBackedArraySentinel(LowerType(typeAst)),
        _ => null,
    };

    private static ZigSentinel? SourceBackedArraySentinel(CType type) => type.Unqualified switch
    {
        CType.Array { ZigSentinel: { } sentinel } => new ZigSentinel(true, sentinel),
        CType.Array => new ZigSentinel(false, null),
        CType.Pointer { Pointee.Unqualified: CType.Array { ZigSentinel: not null } } => new ZigSentinel(false, null),
        _ => null,
    };

    private readonly Dictionary<Symbol, ZigSentinel> _valueSentinels = new();
    private readonly Dictionary<string, ZigSentinel> _anytypeSeedSentinels = new(System.StringComparer.Ordinal);
    private readonly Dictionary<Symbol, Dictionary<string, ZigSentinel>> _instanceAnytypeSentinels = new();

    private ZigSentinel? SentinelOfValue(Item value) => value.Content switch
    {
        Zig.Grouped group => SentinelOfValue(group.Arg1),
        Zig.Ident identifier => _anytypeSeedSentinels.GetValueOrDefault(Tok(identifier.Arg0))
            ?? (_symbols.Resolve(Tok(identifier.Arg0)) is { } symbol
                ? _valueSentinels.GetValueOrDefault(symbol) ?? BitcastSymbolLocation(symbol).Sentinel
                    ?? SourceBackedArraySentinel(symbol.Type) : null),
        Zig.Field field when Tok(field.Arg2) == "sentinel_ptr" && TryEvalTypeInfo(field.Arg0, out _)
            => new ZigSentinel(false, null),
        Zig.Field field when ResolveModulePath(field.Arg0)?.Lowering is { } owner
            && owner._lazyValueConsts.TryGetValue(Tok(field.Arg2), out var declaration) && declaration.typeItem is { } annotation
            => owner.SentinelOfTypeArg(annotation),
        Zig.Field field when MemberBaseType(field.Arg0) is { } namespaceType
            && ContainerTypeName(namespaceType) is { } container
            && EnsureContainerVar(container, SourceMemberName(field.Arg2)) is { } variable
            => _valueSentinels.GetValueOrDefault(variable) ?? BitcastSymbolLocation(variable).Sentinel
                ?? SourceBackedArraySentinel(variable.Type),
        Zig.Field field when ValueTypeForPointerSize(field.Arg0)?.Unqualified is { } parent
            && (parent is CType.Pointer pointer ? pointer.Pointee.Unqualified : parent) is CType.Named aggregate
            && _shared.StructFieldSentinels.TryGetValue((aggregate.Name, SourceMemberName(field.Arg2)), out var entry)
            => entry,
        Zig.BuiltinCall cast when Tok(cast.Arg0) == "@as" && Flatten(cast.Arg2) is [var type, _] => SentinelOfTypeArg(type),
        Zig.BuiltinCall cast when Tok(cast.Arg0) is "@alignCast" or "@constCast" or "@volatileCast"
            && Flatten(cast.Arg2) is [var operand] => SentinelOfValue(operand),
        Zig.PreAddrOf => new ZigSentinel(false, null),
        Zig.PreTry tried => SentinelOfValue(tried.Arg1),
        Zig.CatchOp caught when CatchPreservesPayloadSentinel(caught.Arg2) => SentinelOfValue(caught.Arg0),
        Zig.CatchCapture caught when CatchPreservesPayloadSentinel(caught.Arg5) => SentinelOfValue(caught.Arg0),
        Zig.CatchArm caught when CatchPreservesPayloadSentinel(caught.Arg2) => SentinelOfValue(caught.Arg0),
        Zig.CatchCaptureArm caught when CatchPreservesPayloadSentinel(caught.Arg5) => SentinelOfValue(caught.Arg0),
        Zig.OrElse optional => SentinelOfValue(optional.Arg0),
        Zig.Unwrap optional => SentinelOfValue(optional.Arg0),
        Zig.Index indexed => SentinelOfIndexedValue(indexed.Arg0),
        Zig.SliceOpen slice => SentinelOfValue(slice.Arg0),
        Zig.SliceRange slice when SentinelOfValue(slice.Arg0) is { HasSentinel: false } plain => plain,
        Zig.CallArgs => SentinelOfCall(value),
        _ => null,
    };

    private static bool CatchPreservesPayloadSentinel(Item fallback)
    {
        fallback = UnwrapGrouped(fallback);
        return IsUnreachableItem(fallback) || IsNoreturnArm(fallback);
    }

    private ZigSentinel? SentinelOfIndexedValue(Item sequence)
    {
        // Indexing selects the declared element type, not the outer slice's
        // sentinel. Reuse the nested source shape captured where a field or
        // value was declared; the erased pointer carrier proves nothing.
        using var hoist = EnterThrowawayHoist();
        return BitcastLoweredLocation(LowerExpr(sequence)).Element?.Sentinel;
    }

    private ZigSentinel? SentinelOfCall(Item expression)
    {
        using var hoist = EnterFreshHoist();
        var call = LowerExpr(expression);
        return call is Call { CalleeSym: { } function } && _shared.FnReturnSentinels.TryGetValue(function, out var sentinel)
            ? sentinel : null;
    }

    private void RememberValueSentinel(Symbol symbol, Item value, Item? annotation = null)
    {
        if (symbol.Type.Unqualified is CType.Pointer or CType.Slice or CType.Array
            && (annotation is { } type ? SentinelOfTypeArg(type)
                : BitcastSymbolLocation(symbol).Sentinel ?? SentinelOfValue(value) ?? SourceBackedArraySentinel(symbol.Type)) is { } sentinel)
            _valueSentinels[symbol] = sentinel;
    }

    private string? ExportedPointerSize(string name)
    {
        _ = ResolveExportedType(name);
        return _declaredPtrSize.GetValueOrDefault(name);
    }

    private ZigSentinel? ExportedSentinel(string name)
    {
        _ = ResolveExportedType(name);
        return _declaredSentinel.GetValueOrDefault(name);
    }

    /// <summary>A spelled sentinel expression's value: an integer the const evaluator folds, else a sentinel that is not
    /// an integer (<c>null</c> for an optional-pointer element), which a fold of it rejects.</summary>
    private ZigSentinel SpelledSentinel(Item sentinelExpr)
    {
        using (EnterThrowawayHoist())
        {
            try { return new ZigSentinel(true, _ir.ConstEval(LowerExpr(sentinelExpr))); }
            catch (IrUnsupportedException) { return new ZigSentinel(true, null); }
        }
    }
}
