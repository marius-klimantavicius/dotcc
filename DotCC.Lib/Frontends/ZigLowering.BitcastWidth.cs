#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using DotCC.Ir;
using LALR.CC.LexicalGrammar;

namespace DotCC.Frontends;
internal sealed partial class ZigLowering
{
    // Physical carriers alone never establish native integer width.
    internal sealed record ZigResultLocation(CType? Physical, int? IntegerBits = null, ZigResultLocation? Element = null, ZigSentinel? Sentinel = null)
    {
        public static ZigResultLocation Unknown(CType? physical) => new(physical);
    }
    internal sealed class BitcastWidthMetadata
    {
        public Dictionary<Symbol, IReadOnlyList<(string Name, ZigResultLocation Location)>> Parameters { get; } = new();
        public Dictionary<Symbol, ZigResultLocation> Returns { get; } = new();
        public Dictionary<CExpr, ZigResultLocation> Expressions { get; } = new(ReferenceEqualityComparer.Instance);
        public Dictionary<CExpr, ZigResultLocation> CallResults { get; } = new(ReferenceEqualityComparer.Instance);
        public Dictionary<CType, ZigResultLocation> TypeOperands { get; } = new(ReferenceEqualityComparer.Instance);
        public Dictionary<Symbol, ZigResultLocation> Values { get; } = new();
        public Dictionary<(string Owner, string Field), ZigResultLocation> Fields { get; } = new();
        public Dictionary<string, int> PackedBits { get; } = new(StringComparer.Ordinal);
    }

    private ZigResultLocation BitcastTypeLocation(Item ast, CType physical, int depth = 0)
    {
        if (depth > 32) return ZigResultLocation.Unknown(physical);
        while (ast.Content is Zig.Grouped g) ast = g.Arg1;
        ast = StripVolatile(ast);
        if (UsesSourceStdlib && _shared.BitcastWidths.TypeOperands.TryGetValue(physical, out var operandLocation))
            return operandLocation;
        if (ast.Content is Zig.BuiltinCall { Arg2: not null } fieldType && Tok(fieldType.Arg0) == "@FieldType"
            && _reifiedTypeLocations.TryGetValue(fieldType.Arg2, out var fieldLocation))
            return fieldLocation with { Physical = physical };
        if (_typeCallLocations.TryGetValue(ast, out var returnedLocation))
            return returnedLocation with { Physical = physical };
        // Optional pointers/functions share their runtime representation with
        // the payload; the source wrapper must not hide the pointee's width.
        if (ast.Content is Zig.TyOptional optionalPointer && physical.Unqualified is CType.Pointer or CType.Func)
            return BitcastTypeLocation(optionalPointer.Arg1, physical, depth + 1);
        if (physical.Unqualified is CType.ErrorUnion eu)
            return BitcastTypeLocation(ast.Content is Zig.ErrUnion e ? e.Arg2 : ast, eu.Payload, depth + 1) with { Physical = physical };
        if (physical.Unqualified is CType.Optional opt)
            return BitcastTypeLocation(ast.Content is Zig.TyOptional o ? o.Arg1 : ast, opt.Inner, depth + 1) with { Physical = physical };
        if (physical.Unqualified is CType.Prim { Integer: true }) return new(physical, DeclaredBitsOfTypeArg(ast));
        // Vector aliases carry the source lane width, as @typeInfo(V).vector.child
        // does. It is evidence from the constructor, not the carrier's width.
        if (physical.Unqualified is CType.Vector vector && DeclaredBitsOfTypeArg(ast) is { } laneBits)
            return new(physical, Element: new(vector.Element, laneBits));
        if (physical.Unqualified is not (CType.Array or CType.Slice or CType.Pointer or CType.Vector)) return new(physical);
        Item? child = ast.Content switch
        {
            Zig.TyArray a => a.Arg3, Zig.TySentArray a => a.Arg5,
            Zig.TySlice a => a.Arg2, Zig.TySliceConst a => a.Arg3,
            Zig.TySliceAlign a => a.Arg3, Zig.TySliceConstAlign a => a.Arg4,
            Zig.TyManyPtr a => a.Arg1, Zig.TyManyPtrConst a => a.Arg2,
            Zig.TyPointer a => a.Arg1, Zig.TyPtrConst a => a.Arg2,
            Zig.TyPointerAlign a => a.Arg2, Zig.TyPtrConstAlign a => a.Arg3,
            Zig.TyManyPtrAlign a => a.Arg2, Zig.TyManyPtrConstAlign a => a.Arg3,
            Zig.BuiltinCall b when Tok(b.Arg0) == "@Vector" && Flatten(b.Arg2) is [_, var lane] => lane,
            _ => null,
        };
        var element = physical.Unqualified switch
        {
            CType.Array a => a.Element, CType.Slice s => s.Element, CType.Pointer p => p.Pointee,
            CType.Vector v => v.Element, _ => null,
        };
        if (child is not null && element is not null)
            return new(physical, Element: BitcastTypeLocation(child, element, depth + 1), Sentinel: SentinelOfTypeArg(ast));
        // Array aliases resolve in their declaration environment after type lowering.
        if (ast.Content is Zig.Ident id && _topLevelConstRhs.TryGetValue(Tok(id.Arg0), out var rhs))
            return BitcastTypeLocation(rhs, physical, depth + 1);
        if (ast.Content is Zig.Field f && ResolveModulePath(f.Arg0)?.Lowering is { } owner
            && owner._topLevelConstRhs.TryGetValue(Tok(f.Arg2), out var exported))
            return owner.BitcastTypeLocation(exported, physical, depth + 1);
        return new(physical, Sentinel: SentinelOfTypeArg(ast));
    }

    private void CaptureBitcastFunction(Symbol fn, IReadOnlyList<ParamInfo> infos, Item retAst,
        IReadOnlyDictionary<string, int>? anyBits = null)
        => CaptureBitcastSignature((CType.Func)fn.Type.Unqualified, infos, retAst, anyBits)(fn);

    // Resolve source metadata in the same lexical/value-seed scope as the
    // physical signature, then publish it once the function symbol exists.
    private Action<Symbol> CaptureBitcastSignature(CType.Func signature, IReadOnlyList<ParamInfo> infos,
        Item retAst, IReadOnlyDictionary<string, int>? anyBits = null,
        IReadOnlyDictionary<string, ZigResultLocation>? anyLocations = null)
    {
        var slots = new List<(string, ZigResultLocation)>();
        var at = 0;
        foreach (var info in infos)
        {
            if (info.IsComptime) continue;
            if (at >= signature.Params.Count) break;
            var physical = signature.Params[at++];
            var shape = info.Kind == ParamKind.AnyType
                ? anyLocations is not null && anyLocations.TryGetValue(info.Name, out var argument)
                    ? argument with { Physical = physical }
                    : new ZigResultLocation(physical, anyBits is not null && anyBits.TryGetValue(info.Name, out var b) ? b : null)
                : BitcastTypeLocation(info.TypeAst, physical);
            slots.Add((info.Name, shape));
        }
        var result = BitcastTypeLocation(retAst, signature.Return);
        var sentinel = signature.Return.Unqualified is CType.Pointer or CType.Slice or CType.Array or CType.ErrorUnion or CType.Optional
            ? SentinelOfTypeArg(retAst) : null;
        return fn =>
        {
            _shared.BitcastWidths.Parameters[fn] = slots;
            _shared.BitcastWidths.Returns[fn] = result;
            if (sentinel is { } known) _shared.FnReturnSentinels[fn] = known;
        };
    }
    private void CaptureBitcastAggregate(string name, IReadOnlyList<StructField> fields, AggregateLayout layout)
    {
        if (layout != AggregateLayout.Packed) return;
        long bits = 0;
        foreach (var f in fields)
        {
            var width = f.Type.Unqualified is CType.Prim { Name: "_Bool" } ? 1
                : BitcastLogicalBits(BitcastFieldLocation(name, f.Name, f.Type));
            if (width is null) return;
            bits += width.Value;
        }
        if (bits is > 0 and <= 128) _shared.BitcastWidths.PackedBits[name] = (int)bits;
    }
    private ZigResultLocation BitcastParamLocation(Symbol fn, int slot, CType? physical)
        => _shared.BitcastWidths.Parameters.TryGetValue(fn, out var slots) && slot < slots.Count
            ? slots[slot].Location : ZigResultLocation.Unknown(physical);
    private ZigResultLocation BitcastReturnLocation(CType? physical)
    {
        if (_currentFnSym is { } fn)
        {
            if (_shared.BitcastWidths.Returns.TryGetValue(fn, out var shape)) return shape with { Physical = physical };
            if (_fnReturnBits.TryGetValue(fn, out var bits)) return new(physical, bits);
        }
        return new(physical);
    }
    private ZigResultLocation BitcastFieldLocation(string owner, string field, CType physical)
        => _shared.BitcastWidths.Fields.TryGetValue((owner, field), out var shape) ? shape
            : new(physical, _structFieldBits.TryGetValue((owner, field), out var b) ? b : null);
    private ZigResultLocation BitcastDefaultLocation(string owner, string? field, CType physical, Item value)
    {
        if (field is not null) return BitcastFieldLocation(owner, field, physical);
        // Compatibility callers identify the declaration's field by its own default.
        foreach (var entry in _structFieldDefaults)
            if (entry.Key.Item1 == owner && ReferenceEquals(entry.Value, value))
                return BitcastFieldLocation(owner, entry.Key.Item2, physical);
        return new(physical);
    }
    private void RememberBitcastValue(Symbol sym, ZigResultLocation shape)
        => _shared.BitcastWidths.Values[sym] = shape with { Physical = sym.Type };
    private ZigResultLocation BitcastSymbolLocation(Symbol sym)
    {
        if (_shared.BitcastWidths.Values.TryGetValue(sym, out var value)) return value;
        if (sym.Kind == SymKind.Param && _currentFnSym is { } fn && _shared.BitcastWidths.Parameters.TryGetValue(fn, out var parameters))
            foreach (var p in parameters)
                if (p.Name == sym.Name && _fnParamInfos.TryGetValue(fn, out var infos) && infos.Any(i => i.Name == sym.Name))
                    return p.Location;
        ZigResultLocation? element = null;
        if (_valueElemBits.TryGetValue(sym, out var eb))
        {
            var et = sym.Type.Unqualified switch
            {
                CType.Array a => a.Element, CType.Slice s => s.Element, CType.Pointer p => p.Pointee, _ => null,
            };
            if (et is not null) element = new(et, eb);
        }
        return new(sym.Type, _valueBits.TryGetValue(sym, out var b) ? b : null, element);
    }
    private ZigResultLocation BitcastLoweredLocation(CExpr value) => _shared.BitcastWidths.Expressions.TryGetValue(value, out var known) ? known : value switch
    {
        StatementExpression { Value: { } result } expression => BitcastLoweredLocation(result) with { Physical = expression.Type },
        Paren p => BitcastLoweredLocation(p.Inner),
        ComptimeFold { Live: true } live => BitcastLoweredLocation(live.Inner),
        VarRef v => BitcastSymbolLocation(v.Sym),
        BitCast { ZigLayout: { } layout } b => new(b.Type, layout.Target.LogicalBits),
        Call call when _shared.BitcastWidths.CallResults.TryGetValue(call, out var callResult) => callResult,
        Call { CalleeSym: { } fn } when _shared.BitcastWidths.Returns.TryGetValue(fn, out var result) => result,
        Member { Field: "Value", Base: var wrapped } m when wrapped.Type.Unqualified is CType.Optional or CType.ErrorUnion
            => BitcastLoweredLocation(wrapped) with { Physical = m.Type },
        Cast { Operand: Member { Field: "Value", Base.Type.Unqualified: CType.ErrorUnion } payload } c
            when c.Type.Unqualified is CType.Pointer
            => BitcastLoweredLocation(payload) with { Physical = c.Type },
        Cast { Operand: var optional } c
            when optional.Type.Unqualified is CType.Pointer { ZigOptional: true } sourcePointer
                && c.Type.Unqualified is CType.Pointer { ZigOptional: false } payloadPointer
                && sourcePointer.Pointee.Equals(payloadPointer.Pointee)
            => BitcastLoweredLocation(optional) with { Physical = c.Type },
        ZigTry tried => BitcastLoweredLocation(tried.Inner) with { Physical = tried.Type },
        Member { Field: "Ptr", Base: var sequence } m when sequence.Type.Unqualified is CType.Slice
            => new(m.Type, Element: BitcastLoweredLocation(sequence).Element),
        Binary { Op: BinOp.Add or BinOp.Sub, Left: var pointer } b when b.Type.Unqualified is CType.Pointer
            => BitcastLoweredLocation(pointer) with { Physical = b.Type },
        Member m when (m.Base.Type.Unqualified is CType.Pointer p ? p.Pointee.Unqualified : m.Base.Type.Unqualified) is CType.Named owner
            => BitcastFieldLocation(owner.Name, m.Field, m.Type),
        DotCC.Ir.Index ix => BitcastLoweredLocation(ix.Base).Element ?? ZigResultLocation.Unknown(ix.Type),
        Unary { Op: UnOp.Deref } u => BitcastLoweredLocation(u.Operand).Element ?? ZigResultLocation.Unknown(u.Type),
        _ => new(value.Type, DeclaredBitsOfLowered(value)),
    };
    // Read source type syntax without evaluating the pointer/value expression.
    private ZigResultLocation? BitcastPathLocation(Item ast) => ast.Content switch
    {
        Zig.Grouped g => BitcastPathLocation(g.Arg1),
        Zig.Ident id when _symbols.Resolve(Tok(id.Arg0)) is { } sym => BitcastSymbolLocation(sym),
        Zig.BuiltinCall b when Tok(b.Arg0) == "@as" && Flatten(b.Arg2) is [var type, _]
            => BitcastTypeLocation(type, LowerType(type)),
        Zig.PreAddrOf a when BitcastPathLocation(a.Arg1) is { Physical: { } t } child => new(new CType.Pointer(t), Element: child),
        Zig.SliceRange s => BitcastSlicePath(s.Arg0), Zig.SliceOpen s => BitcastSlicePath(s.Arg0),
        Zig.Deref d => BitcastPathLocation(d.Arg0)?.Element, Zig.Index i => BitcastPathLocation(i.Arg0)?.Element,
        _ => null,
    };
    private ZigResultLocation? BitcastSlicePath(Item ast)
    {
        var parent = BitcastPathLocation(ast);
        var child = parent?.Physical?.Unqualified is CType.Pointer { Pointee.Unqualified: CType.Array }
            ? parent.Element?.Element : parent?.Element;
        return child?.Physical is { } et ? new(new CType.Slice(et), Element: child) : null;
    }
    private static bool ExplicitAsContainsBitcast(Item ast)
    {
        while (ast.Content is Zig.Grouped g) ast = g.Arg1;
        if (ast.Content is not Zig.BuiltinCall { Arg2: not null } b) return false;
        return Tok(b.Arg0) == "@bitCast"
            || Tok(b.Arg0) == "@as" && Flatten(b.Arg2) is [_, var value] && ExplicitAsContainsBitcast(value);
    }
    private ZigResultLocation BitcastSourceLocation(Item ast, CExpr value)
    {
        while (ast.Content is Zig.Grouped g) ast = g.Arg1;
        if (ast.Content is Zig.BuiltinCall enumCall && Tok(enumCall.Arg0) is "@backingInt" or "@intFromEnum"
            && value is Cast { Operand.Type.Unqualified: CType.Enum enumeration })
            return new(value.Type, EnumBackingBits(enumeration));
        if (ast.Content is Zig.BuiltinCall { Arg2: not null } b && Tok(b.Arg0) == "@as" && Flatten(b.Arg2) is [var type, _])
            return BitcastTypeLocation(type, value.Type);
        if (ast.Content is Zig.TypedStructInit t) return BitcastTypeLocation(t.Arg0, value.Type);
        if (ast.Content is Zig.Deref d && BitcastPathLocation(d.Arg0) is { } parent)
        {
            if (parent.Physical?.Unqualified is CType.Slice) return new(value.Type, Element: parent.Element);
            if (parent.Element is { } pointee) return pointee with { Physical = value.Type };
        }
        var shape = BitcastLoweredLocation(value);
        return shape with { IntegerBits = DeclaredBitsOfValue(ast) ?? shape.IntegerBits };
    }
    private ZigResultLocation? TryBitcastArrayLiteralLocation(Item type, int count)
    {
        if (type.Content is Zig.TyArray a)
        {
            var element = LowerDataType(a.Arg3);
            var n = a.Arg1.Content is Zig.Ident id && Tok(id.Arg0) == "_" ? count : ConstEvalArraySize(a.Arg1);
            return new(new CType.Array(element, n), Element: BitcastTypeLocation(a.Arg3, element));
        }
        if (TryTypeAliasRhs(type, out var physical) && physical.Unqualified is CType.Array) return BitcastTypeLocation(type, physical);
        return null;
    }
    private CExpr BuildLogicalArrayInit(IReadOnlyList<Item> fields, ZigResultLocation location)
    {
        var arr = (CType.Array)location.Physical!.Unqualified;
        // The established multidimensional flattening route keeps its unknown leaf metadata.
        if (arr.Element.Unqualified is CType.Array || location.Element is null) return BuildArrayInit(fields, arr);
        if (arr.Count is { } n && fields.Count != n)
            throw new IrUnsupportedException($"zig array literal has {fields.Count} elements; target expects {n}");
        var elements = LowerArrayElems(fields, location.Element);
        return new StackArray(arr.Element, elements) { Type = arr.Count is null ? new CType.Array(arr.Element, fields.Count) : arr };
    }
    private int? BitcastLogicalBits(ZigResultLocation shape)
    {
        switch (shape.Physical?.Unqualified)
        {
            case CType.Prim { Integer: true, IsComptimeInt: false, Name: not "_Bool" }: return shape.IntegerBits;
            case CType.Prim { Integer: false } p when p == CType.Float || p == CType.Double: return p.Bytes * 8;
            case CType.Float128Type: return 128;
            // Bool vectors already carry lane zero in the low bit of a ulong.
            // Their logical width is the lane count, independent of that carrier.
            case CType.Vector { IsMask: true, Count: > 0 and <= 64 } mask: return mask.Count;
            case CType.Array { Count: { } n } when shape.Element is { } e && BitcastLogicalBits(e) is { } eb:
                return (long)n * eb is > 0 and <= 128 ? n * eb : null;
            case CType.Vector { IsMask: false } v when shape.Element is { } lane
                && BitcastLogicalBits(lane) is { } laneBits && laneBits == v.Element.SizeOf * 8:
                return (long)v.Count * laneBits is > 0 and <= 128 ? v.Count * laneBits : null;
            case CType.Named n when _shared.BitcastWidths.PackedBits.TryGetValue(n.Name, out var bits): return bits;
            default: return null;
        }
    }
    private ZigBitcastValueLayout BitcastLayout(ZigResultLocation shape, string role)
    {
        var bits = BitcastLogicalBits(shape);
        if (bits is not (> 0 and <= 128))
            throw new IrUnsupportedException($"zig @bitCast needs known logical source and target widths (1..128); missing logical metadata or unsupported layout for {role}: {shape.Physical?.Describe()} (integer bits {shape.IntegerBits}) in {_module?.Path ?? _fileStem}, function '{_currentFnName}'");
        if (shape.Physical!.Unqualified is CType.Array { Count: { } count } a)
        {
            if (shape.Element?.Physical?.Unqualified is not CType.Prim { Integer: true, IsComptimeInt: false, Name: not "_Bool" }
                || BitcastLogicalBits(shape.Element) is not { } elementBits || _ir.SizeOfConst(a.Element) is not { } stride || stride <= 0)
                throw new IrUnsupportedException("zig @bitCast: unsupported array element layout");
            return new(bits.Value, false, ZigBitcastRepresentation.Array, count, elementBits, checked((int)stride));
        }
        return new(bits.Value, shape.Physical.Unqualified is CType.Prim { Integer: true, Signed: true },
            shape.Physical.Unqualified is CType.Vector ? ZigBitcastRepresentation.Vector :
            shape.Physical.Unqualified is CType.Named ? ZigBitcastRepresentation.Packed :
            shape.Physical.Unqualified is CType.Float128Type or CType.Prim { Integer: false }
                ? ZigBitcastRepresentation.Float : ZigBitcastRepresentation.Integer);
    }
    private CExpr LowerLogicalBitcast(Item sourceItem, CExpr operand, ZigResultLocation target, bool beforeOperand)
    {
        // Zig result locations project optional/error-union payloads into the
        // builtin; the surrounding expression still keeps its wrapper CType.
        while (target.Physical?.Unqualified is CType.Optional or CType.ErrorUnion)
            target = target with { Physical = target.Physical.Unqualified is CType.Optional optional
                ? optional.Inner : ((CType.ErrorUnion)target.Physical.Unqualified).Payload };
        while (sourceItem.Content is Zig.Grouped group) sourceItem = group.Arg1;
        var value = operand;
        while (value is Paren paren) value = paren.Inner;
        // R34 array witness, retaining the already lowered pointer exactly once.
        if (sourceItem.Content is Zig.Deref && value is SliceNew slice)
        {
            if (_ir.ConstEval(slice.Len) is not { } length || length < 0 || length > int.MaxValue)
                throw new IrUnsupportedException("zig @bitCast slice dereference requires a comptime-known array length");
            var array = new CType.Array(slice.Element, (int)length);
            var pointer = new CType.Pointer(array);
            operand = new Unary(UnOp.Deref, new Cast(pointer, slice.Ptr) { Type = pointer })
                { Type = array, IsLValue = true, Pos = operand.Pos };
        }
        if (target.Physical?.Unqualified is CType.Pointer or CType.Func or CType.Slice
            || operand.Type.Unqualified is CType.Pointer or CType.Func or CType.Slice)
            throw new IrUnsupportedException("zig @bitCast requires supported value types, not pointers, slices, or functions"
                + $" (source {operand.Type.Describe()}, target {target.Physical?.Describe()}; {_module?.Path ?? _fileStem}:{sourceItem.Position.Line}, function '{_currentFnName}')");
        var source = BitcastSourceLocation(sourceItem, operand);
        if (source.Element is null && value is SliceNew sourceSlice && operand.Type.Unqualified is CType.Array)
            source = source with { Element = BitcastLoweredLocation(sourceSlice.Ptr).Element };
        var arithmetic = value is Binary { Op: BinOp.Add or BinOp.Sub or BinOp.Mul or BinOp.Div or BinOp.Mod
            or BinOp.BitAnd or BinOp.BitOr or BinOp.BitXor or BinOp.Shl or BinOp.Shr }
            or Unary { Op: UnOp.Neg or UnOp.BitNot };
        var sourceBits = source.IntegerBits;
        if (sourceBits is null && value is Binary binary && arithmetic)
        {
            var leftBits = BitcastLoweredLocation(binary.Left).IntegerBits;
            var rightBits = BitcastLoweredLocation(binary.Right).IntegerBits;
            if (binary.Op is BinOp.Shl or BinOp.Shr || leftBits == rightBits) sourceBits = leftBits;
        }
        if (sourceBits is null && value is Unary unary && arithmetic) sourceBits = BitcastLoweredLocation(unary.Operand).IntegerBits;
        source = source with { IntegerBits = sourceBits };
        // An array and a vector of the same native-width lanes have identical
        // value bits. Use the established vector load instead of the scalar
        // (at most 128-bit) packer; this also covers wide std.mem equality scans.
        if (target.Physical?.Unqualified is CType.Vector vector && !vector.IsMask
            && operand.Type.Unqualified is CType.Array { Count: { } count } sourceArray && count == vector.Count
            && sourceArray.Element.Unqualified == vector.Element.Unqualified
            && source.Element is { IntegerBits: { } elementBits }
            && target.Element?.IntegerBits == elementBits
            && elementBits == sourceArray.Element.SizeOf * 8
            && TryCoerceToVector(operand, vector) is { } loaded)
            return loaded;
        if (TryBitcastPackedArrayToVector(operand, target) is { } packedVector)
            return packedVector;
        var from = BitcastLayout(source, $"source at line {sourceItem.Position.Line}");
        var to = BitcastLayout(target, $"target at line {sourceItem.Position.Line}");
        if (from.LogicalBits != to.LogicalBits)
            throw new IrUnsupportedException("zig @bitCast requires equal logical source and target bit widths");
        if (to.Representation == ZigBitcastRepresentation.Array)
        {
            if (from.Representation == ZigBitcastRepresentation.Packed)
            {
                // Read the source-proved packed bits through the existing
                // scalar bitcast, then split that value into target lanes.
                // Physical padding never becomes an invented array payload.
                operand = new BitCast(CType.UInt128, operand) { Type = CType.UInt128,
                    ZigLayout = new(from, new(from.LogicalBits, false, ZigBitcastRepresentation.Integer)) };
            }
            else if (from.Representation != ZigBitcastRepresentation.Integer)
                throw new IrUnsupportedException("zig @bitCast to an array currently requires an integer or source-proved packed value");
            return IntegerToBitcastArray(operand, (CType.Array)target.Physical!.Unqualified, to, beforeOperand);
        }
        // Only witnessed C promotion narrows; a direct u64 never adopts u32 from its sink.
        if (arithmetic && operand.Type.Unqualified is CType.Prim { Integer: true } primitive)
        {
            var bytes = (from.LogicalBits + 7) / 8;
            var narrow = bytes switch { 1 => CType.UChar, 2 => CType.UShort, <= 4 => CType.UInt, _ => (CType?)null };
            if (narrow is not null && narrow.SizeOf < primitive.Bytes) operand = new Cast(narrow, operand) { Type = narrow };
        }
        return new BitCast(target.Physical!, operand) { Type = target.Physical!, ZigLayout = new(from, to) };
    }

    private CExpr IntegerToBitcastArray(CExpr operand, CType.Array target, ZigBitcastValueLayout layout, bool beforeOperand)
    {
        if (layout.ElementLogicalBits > layout.ElementStride * 8)
            throw new IrUnsupportedException("zig @bitCast array element logical width exceeds its storage");
        // Zig's logical array bit order starts at the low bits of the integer.
        // Materialize its validated native-width elements using the established
        // array value representation, with the operand evaluated once.
        var temporary = _symbols.Declare(new Symbol
        {
            Name = "__bitcastArray" + _anfTempCounter++, Kind = SymKind.Var, Type = CType.UInt128,
        });
        var reference = new VarRef(temporary) { Type = temporary.Type };
        var elements = new List<CExpr>(layout.ElementCount);
        for (var index = 0; index < layout.ElementCount; index++)
        {
            var part = new Binary(BinOp.Shr, reference, IntLit(index * layout.ElementLogicalBits)) { Type = CType.UInt128 };
            CExpr element = new Cast(target.Element, part) { Type = target.Element };
            if (layout.ElementLogicalBits < layout.ElementStride * 8)
            {
                var shift = IntLit(layout.ElementStride * 8 - layout.ElementLogicalBits);
                var high = new Cast(target.Element, new Binary(BinOp.Shl, element, shift) { Type = target.Element }) { Type = target.Element };
                element = new Cast(target.Element, new Binary(BinOp.Shr, high, shift) { Type = target.Element }) { Type = target.Element };
            }
            elements.Add(element);
        }
        return HoistLowered("@bitCast array", [new DeclStmt([new(temporary,
            new Cast(CType.UInt128, operand) { Type = CType.UInt128 })])],
            new StackArray(target.Element, elements) { Type = target }, beforeOperand);
    }
}
