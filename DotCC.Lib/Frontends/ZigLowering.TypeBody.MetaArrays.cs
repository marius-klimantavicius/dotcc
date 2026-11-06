#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using DotCC.Ir;
using LALR.CC.LexicalGrammar;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    // Types and reflection attributes have no runtime representation. Mutable
    // arrays of them belong to the type-body evaluator, beside reflection lists.
    private sealed class TypeBodyMetaArray(int count, bool types)
    {
        internal readonly object?[] Elements = new object?[count];
        internal readonly bool Types = types;
        internal readonly int?[] TypeBits = new int?[count];
        internal object Read(int index) => Elements[index]
            ?? throw new CompileException("zig: use of an undefined comptime array element");
        internal ZigComptimeList List(string name) => Types
            ? new() { Label = name, Types = Enumerable.Range(0, Elements.Length).Select(i => (CType)Read(i)).ToArray(), TypeBits = TypeBits.ToArray() }
            : new() { Label = name, Attrs = Enumerable.Range(0, Elements.Length).Select(i => (ZigFieldAttr)Read(i)).ToArray() };
    }
    private readonly Dictionary<string, TypeBodyMetaArray> _typeBodyMetaArrays = new(StringComparer.Ordinal);
    private readonly Dictionary<string, (TypeBodyMetaArray Array, int Index)> _typeBodyMetaPointers = new(StringComparer.Ordinal);

    private bool TryBindTypeBodyMetaArray(string name, Item? annotation, Item initializer)
    {
        if (annotation?.Content is not Zig.TyArray array) { return false; }
        var types = IsTypeKeyword(array.Arg3);
        if (!types && !(TryResolveStdPath(array.Arg3, out var path)
            && path is "std.lang.Type.Struct.FieldAttributes" or "std.builtin.Type.Struct.FieldAttributes"
                or "std.lang.Type.Union.FieldAttributes" or "std.builtin.Type.Union.FieldAttributes")) { return false; }
        var storage = new TypeBodyMetaArray(ConstEvalArraySize(array.Arg1), types);
        if (initializer.Content is Zig.BuiltinCall { Arg2: not null } splat && Tok(splat.Arg0) == "@splat"
            && Flatten(splat.Arg2) is [var item])
        {
            object value = types ? LowerType(item) : ReadTypeBodyAttribute(item);
            Array.Fill(storage.Elements, value);
            if (types) Array.Fill(storage.TypeBits, DeclaredBitsOfTypeArg(item));
        }
        else if (initializer.Content is not Zig.UndefinedLit)
        {
            throw new IrUnsupportedException("zig: a comptime metadata array initializer must be undefined or @splat");
        }
        _typeBodyMetaArrays[name] = storage;
        return true;
    }

    private ZigFieldAttr ReadTypeBodyAttribute(Item expression)
    {
        if (expression.Content is Zig.Ident id && _comptimeAttrs.TryGetValue(Tok(id.Arg0), out var attr)) { return attr; }
        if (expression.Content is Zig.Index index && TryFoldTypeInfoList(index.Arg0, out var list) && list.Attrs is { } attributes)
            return attributes[ComptimeListIndex(list, index.Arg2)];
        if (expression.Content is Zig.AnonStructInitEmpty) { return new(null, false); }
        if (expression.Content is not Zig.AnonStructInit literal)
            throw new IrUnsupportedException("zig: a comptime field attribute must be a literal or reflected field");
        long? alignment = null;
        CExpr? defaultValue = null;
        var comptime = false;
        foreach (var item in Flatten(literal.Arg2))
        {
            if (item.Content is not Zig.FieldInit field) { throw new CompileException("zig: field attributes must be named"); }
            switch (Tok(field.Arg1))
            {
                case "default_value_ptr":
                {
                    if (IsComptimeNull(field.Arg3)) { break; }
                    var pointer = UnwrapGrouped(field.Arg3);
                    if (pointer.Content is Zig.BuiltinCall cast && Tok(cast.Arg0) == "@ptrCast"
                        && Flatten(cast.Arg2) is [var argument]) { pointer = UnwrapGrouped(argument); }
                    if (pointer.Content is not Zig.PreAddrOf address)
                        throw new IrUnsupportedException("zig: field default pointer must address a comptime value");
                    using var hoist = EnterThrowawayHoist();
                    var value = _ir.EvalComptimeValue(LowerExpr(address.Arg1))
                        ?? throw new CompileException("zig: field default is not comptime-known");
                    defaultValue = _ir.SpliceComptimeValue(value)
                        ?? throw new IrUnsupportedException("zig: field default cannot be represented as a constant");
                    break;
                }
                case "align":
                    if (!IsComptimeNull(field.Arg3))
                        alignment = ComptimeIntValue(LowerExpr(field.Arg3))
                            ?? throw new CompileException("zig: field alignment is not comptime-known");
                    break;
                case "comptime": comptime = FoldTypeBodyCondition("field attributes", field.Arg3); break;
                default: throw new CompileException("zig: unknown field attribute " + Tok(field.Arg1));
            }
        }
        return new(alignment, defaultValue is not null, Default: defaultValue, Comptime: comptime);
    }

    private bool TryStoreTypeBodyMetaArray(Item target, Item value)
    {
        (TypeBodyMetaArray Array, int Index) slot;
        if (target.Content is Zig.Deref { Arg0.Content: Zig.Ident pointer }
            && _typeBodyMetaPointers.TryGetValue(Tok(pointer.Arg0), out slot)) { }
        else if (target.Content is Zig.Index { Arg0.Content: Zig.Ident name } indexed
            && _typeBodyMetaArrays.TryGetValue(Tok(name.Arg0), out var storage))
        {
            var index = ComptimeIntValue(LowerExpr(indexed.Arg2))
                ?? throw new CompileException("zig: metadata array index must be comptime-known");
            if (index < 0 || index >= storage.Elements.Length) { throw new CompileException("zig: comptime array index out of bounds"); }
            slot = (storage, (int)index);
        }
        else { return false; }
        slot.Array.Elements[slot.Index] = slot.Array.Types ? LowerType(value) : ReadTypeBodyAttribute(value);
        if (slot.Array.Types) slot.Array.TypeBits[slot.Index] = DeclaredBitsOfTypeArg(value);
        return true;
    }

    private bool TryWalkTypeBodyFor(string fnName, Zig.ForStmt loop,
        List<(string name, CType? prev, int? prevBits)> aliases, out TypeBodyResult? result)
    {
        result = null;
        var (objects, captures) = DecomposeForMulti(loop.Arg2, loop.Arg6);
        TypeBodyMetaArray? Metadata(Item expr) => StripAddrOf(expr).Content is Zig.Ident id
            ? _typeBodyMetaArrays.GetValueOrDefault(Tok(id.Arg0)) : null;
        var lists = new ZigComptimeList?[objects.Count];
        var metadata = new TypeBodyMetaArray?[objects.Count];
        var arrays = new (IrBuilder.CtArray Backing, int Start, int Count)?[objects.Count];
        var elementLocations = new ZigResultLocation?[objects.Count];
        int? count = null;
        void Measure(int length)
        {
            if (count is { } n && n != length) { throw new CompileException("zig: parallel for operands have different lengths"); }
            count = length;
        }
        for (var k = 0; k < objects.Count; k++)
        {
            var operand = objects[k];
            if (operand.IsRange)
            {
                if (operand.End is { } end)
                {
                    var start = ComptimeIntValue(LowerExpr(operand.Expr));
                    var finish = ComptimeIntValue(LowerExpr(end));
                    if (start is null || finish is null || start > finish || finish - start > InlineUnrollCap)
                        throw new CompileException("zig: invalid or excessive comptime for range");
                    lists[k] = new() { Label = "range", Ints = Enumerable.Range(0, (int)(finish - start))
                        .Select(i => start.Value + i).ToArray() };
                    Measure(lists[k]!.Count);
                }
                continue;
            }
            if (Metadata(operand.Expr) is { } meta)
            {
                metadata[k] = meta;
                Measure(meta.Elements.Length);
            }
            else if (TryComptimeIterable(operand.Expr, out var list))
            {
                if (captures[k].ByRef) { throw new IrUnsupportedException("zig: by-reference capture of a reflection list"); }
                lists[k] = list;
                Measure(list.Count);
            }
            else
            {
                CExpr expression;
                using (EnterThrowawayHoist()) { expression = LowerExpr(StripAddrOf(operand.Expr)); }
                elementLocations[k] = BitcastLoweredLocation(expression).Element;
                arrays[k] = _ir.EvalComptimeStorage(expression) switch
                {
                    IrBuilder.CtArray a => (a, 0, a.Elems.Length),
                    IrBuilder.CtSlice s => (s.Backing, checked((int)s.Offset), checked((int)s.Length)),
                    _ => throw new IrUnsupportedException("zig: metadata for operand is not a comptime-known array or slice"),
                };
                Measure(arrays[k]!.Value.Count);
            }
        }
        var total = count ?? throw new IrUnsupportedException("zig: metadata for has no bounded operand");
        if (total > InlineUnrollCap) { throw new IrUnsupportedException("zig: comptime metadata for exceeds iteration limit"); }
        for (var k = 0; k < objects.Count; k++)
        {
            if (!objects[k].IsRange || lists[k] is not null) { continue; }
            var start = ComptimeIntValue(LowerExpr(objects[k].Expr))
                ?? throw new CompileException("zig: comptime for range start must be known");
            lists[k] = new() { Label = "range", Ints = Enumerable.Range(0, total).Select(i => start + i).ToArray() };
        }
        for (var i = 0; i < total; i++)
        {
            var shadows = new List<ComptimeCaptureShadow>();
            var pointers = new Dictionary<string, (TypeBodyMetaArray Array, int Index)>(_typeBodyMetaPointers, StringComparer.Ordinal);
            _symbols.EnterScope();
            try
            {
                for (var k = 0; k < objects.Count; k++)
                {
                    var (name, byRef) = captures[k];
                    if (name == "_") { continue; }
                    _typeBodyMetaPointers.Remove(name);
                    if (lists[k] is { } list) { shadows.Add(SeedComptimeCapture(name, list, i)); }
                    else if (metadata[k] is { } meta)
                    {
                        if (byRef) { _typeBodyMetaPointers[name] = (meta, i); }
                        else
                        {
                            var one = meta.Types ? new ZigComptimeList { Label = name, Types = [(CType)meta.Read(i)], TypeBits = [meta.TypeBits[i]] }
                                : new ZigComptimeList { Label = name, Attrs = [(ZigFieldAttr)meta.Read(i)] };
                            shadows.Add(SeedComptimeCapture(name, one, 0));
                        }
                    }
                    else if (arrays[k] is { } a)
                    {
                        var type = byRef ? new CType.Pointer(a.Backing.Element) : a.Backing.Element;
                        var symbol = _symbols.Declare(new Symbol { Name = name, Kind = SymKind.Var, Type = type });
                        if (elementLocations[k] is { } location)
                            RememberBitcastValue(symbol, byRef ? new(type, Element: location) : location);
                        IrBuilder.ComptimeValue value = byRef ? new IrBuilder.CtElemPtr(a.Backing, a.Start + i, type)
                            : a.Backing.Elems[a.Start + i];
                        _typeBodyValueSession!.Frame[symbol] = value;
                        _ir.ComptimeGlobals[symbol] = value;
                    }
                }
                result = WalkTypeBody(fnName, BodyStatements(loop.Arg9), aliases);
                if (result is not null) { return true; }
            }
            finally
            {
                for (var k = shadows.Count - 1; k >= 0; k--) { RestoreComptimeCapture(shadows[k]); }
                _typeBodyMetaPointers.Clear();
                foreach (var (name, pointer) in pointers) { _typeBodyMetaPointers[name] = pointer; }
                _symbols.ExitScope();
            }
        }
        if (ForElseBody(loop.Arg10) is { } otherwise)
            result = WalkTypeBody(fnName, BodyStatements(otherwise), aliases);
        return true;
    }
}
