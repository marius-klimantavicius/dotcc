#nullable enable

using System.Collections.Generic;
using System.Linq;
using DotCC.Ir;
using LALR.CC.LexicalGrammar;

namespace DotCC.Frontends;

/// <summary>Zig SIMD vectors, <c>@Vector(N, T)</c> (road-to-zig-std, the target-identity segment T5, by the
/// maintainer's choice: lower to .NET's own vector types). A numeric vector is a
/// <c>System.Runtime.Intrinsics.Vector64/128/256/512&lt;T&gt;</c> chosen by its total width, so element-wise
/// arithmetic is the JIT's SIMD code, with a software fallback on a CPU without it; a BOOL vector, what a
/// comparison yields, is a lane bitmask in a <c>ulong</c>. <c>@splat</c> is <c>VectorN.Create(x)</c>, an array or
/// a slice at a vector sink is a load from its first element, a comparison is a mask (<c>ZigVec.Eq</c>, …), and
/// <c>@reduce</c> / <c>@select</c> / a lane read route through <c>DotCC.Libc.ZigVec</c>. A width .NET has no
/// vector type for uses bounded byte storage at 4 or 128 bytes (splats, raw loads/stores/copies, lane access,
/// integer bitwise operations and integer comparisons into an existing bool mask).
/// Other widths (a <c>@Vector(3, u8)</c>) are a loud cut at runtime; at compile time they are held as arrays
/// (task #108, see <see cref="VectorTypeOf"/>; the runtime fallback is GitHub issue #127).</summary>
internal sealed partial class ZigLowering
{
    /// <summary>Lower <c>@Vector(len, T)</c> to its <see cref="CType.Vector"/>, loud when neither SIMD nor bounded storage fits, except while
    /// evaluating at compile time: there the shape is an array <c>[len]T</c> (task #108: std.meta.FieldEnum's
    /// <c>&amp;std.simd.iota(u8, 3)</c>, a vector read through as <c>*const [3]u8</c>), and a generic instance whose
    /// signature needed one is kept from runtime calls (<see cref="_comptimeVectorArrays"/>).</summary>
    private CType VectorTypeOf(Zig.BuiltinCall call)
    {
        var args = Flatten(call.Arg2);
        if (args.Count != 2)
        {
            throw new IrUnsupportedException($"zig `@Vector` expects (len, T); got {args.Count} argument(s)");
        }
        var len = ConstEvalArraySize(args[0]);
        var element = LowerType(args[1]);
        var vector = new CType.Vector(element, len);
        if (vector.UsesStorage && element.Unqualified is CType.Prim { Integer: true }
            && DeclaredBitsOfTypeArg(args[1]) is { } laneBits && laneBits != element.SizeOf * 8)
        {
            // std.meta.FieldEnum evaluates simd.iota over a narrow tag type at
            // comptime. Preserve the existing interpreter-array path instead
            // of selecting runtime byte storage for that value.
            if (_shared.TypeBodyDepth > 0 || _comptimeDepth > 0 || _loweringForComptimeEval > 0)
            {
                _comptimeVectorArrays++;
                return new CType.Array(element, len);
            }
            throw UnsupportedStorageVector(vector, "non-native integer lane widths");
        }
        if (!vector.IsMask && !vector.UsesStorage && (element.Unqualified is not CType.Prim { Name: not "_Bool" } || vector.NetFamily is null))
        {
            if (element.Unqualified is CType.Prim { Name: not "_Bool" } && (_shared.TypeBodyDepth > 0 || _comptimeDepth > 0 || _loweringForComptimeEval > 0))
            {
                _comptimeVectorArrays++;
                return new CType.Array(element, len);
            }
            throw new IrUnsupportedException(
                $"zig {vector.Describe()}: dotcc lowers a vector to .NET's Vector64/128/256/512, so its lanes must be "
                + $"an integer or float type filling 8, 16, 32 or 64 bytes, or bounded value storage filling 4 or 128 bytes ({vector.Bits / 8} here)"
                + $" ({_module?.Path ?? _fileStem}:{call.Arg0.Position.Line}, function '{_currentFnName}', container '{_currentContainer}')");
        }
        if (vector.IsMask && len > 64)
        {
            throw new IrUnsupportedException($"zig {vector.Describe()}: a bool vector is a 64-bit lane mask, so at most 64 lanes");
        }
        return vector;
    }

    /// <summary>How many times a vector shape .NET cannot hold was lowered as a compile-time array (<see cref="VectorTypeOf"/>).
    /// A generic instance whose signature raised it may only be called at compile time (<see cref="ResolveGenericInstance"/>).</summary>
    private int _comptimeVectorArrays;

    /// <summary>The <c>System.Runtime.Intrinsics</c> class of a numeric vector (<c>System.Runtime.Intrinsics.Vector128</c>).</summary>
    private static string VectorClass(CType.Vector v) => v.UsesStorage
        ? throw UnsupportedStorageVector(v, "SIMD construction")
        : "System.Runtime.Intrinsics." + (v.NetFamily ?? throw new IrUnsupportedException($"zig {v.Describe()}: no SIMD family"));

    private static IrUnsupportedException UnsupportedStorageVector(CType.Vector v, string operation)
        => new($"zig {v.Describe()}: {operation} on a storage vector is not lowered; supported operations are @splat, raw loads/stores/copies, lane access, integer bitwise operations and integer comparisons with at most 64 lanes");

    private static void RequireIntegerStorageVector(CType.Vector vector)
    {
        if (!vector.UsesStorage || vector.Element.Unqualified is not CType.Prim
            { Integer: true, IsComptimeInt: false, Name: not "_Bool", Bytes: 1 or 2 or 4 or 8 })
            throw UnsupportedStorageVector(vector, "bitwise operations require native-width integer lanes");
    }

    // Use the call-argument spill chain so a statement-producing right operand
    // cannot move ahead of a left operand's effects or value read.
    private (CExpr Left, CExpr Right) LowerOrderedBinaryOperands(Item left, Item right)
    {
        var values = new List<CExpr>(2);
        using var evaluation = new CallArgumentScope(this, values, _hoistImpureSeen);
        evaluation.Add(LowerExpr(left));
        evaluation.Add(LowerExpr(right));
        return (values[0], values[1]);
    }

    /// <summary><c>@splat(x)</c> at a vector sink: every lane <c>x</c>. For a bool vector, every bit.</summary>
    private CExpr LowerSplat(IReadOnlyList<Item> args, CType.Vector vector)
    {
        if (args.Count != 1) { throw new IrUnsupportedException($"zig `@splat` expects one argument; got {args.Count}"); }
        var lane = LowerExprSink(args[0], vector.Element);
        if (vector.IsMask)
        {
            // `@splat(true)` is the full mask, `@splat(false)` the empty one; a runtime bool picks between them.
            var full = new Call("ZigVec.Full", new List<CExpr> { IntLit(vector.Count) }) { Type = CType.ULong };
            return new CondExpr(lane, full, new LitInt("0", 0) { Type = CType.ULong }) { Type = vector };
        }
        var factory = vector.UsesStorage ? "ZigVec.Splat" + vector.Bits : VectorClass(vector) + ".Create";
        return new Call(factory, new List<CExpr> { new Cast(vector.Element, lane) { Type = vector.Element } })
        {
            Type = vector,
        };
    }

    /// <summary>A positional list literal at a vector sink: <c>VectorN.Create(e0, e1, …)</c>, one lane per element,
    /// each at the lane type. A bool vector's literal must be comptime-known and becomes its mask constant.</summary>
    private CExpr LowerVectorLiteral(IReadOnlyList<Item> inits, CType.Vector vector)
    {
        if (inits.Count != vector.Count)
        {
            throw new IrUnsupportedException(
                $"zig {vector.Describe()}: a literal needs exactly {vector.Count} element(s); got {inits.Count}");
        }
        var lanes = new List<CExpr>(inits.Count);
        foreach (var init in inits)
        {
            if (init.Content is not Zig.FieldInitPositional pos)
            {
                throw new IrUnsupportedException($"zig {vector.Describe()}: a vector literal lists its lanes positionally");
            }
            lanes.Add(LowerExprSink(pos.Arg0, vector.Element));
        }
        if (vector.IsMask)
        {
            ulong bits = 0;
            for (var k = 0; k < lanes.Count; k++)
            {
                if (_ir.ConstEval(lanes[k]) is not { } on)
                {
                    throw new IrUnsupportedException($"zig {vector.Describe()}: a bool-vector literal must be comptime-known");
                }
                if (on != 0) { bits |= 1UL << k; }
            }
            var mask = new LitInt(bits.ToString(System.Globalization.CultureInfo.InvariantCulture), unchecked((long)bits)) { Type = CType.ULong };
            return new Cast(vector, mask) { Type = vector };
        }
        return new Call(VectorClass(vector) + ".Create",
            lanes.Select(l => (CExpr)new Cast(vector.Element, l) { Type = vector.Element }).ToList())
        {
            Type = vector,
        };
    }

    /// <summary>An array, a pointer to one, or a slice at a vector sink (<c>const block: Block =
    /// slice[i..][0..N].*;</c>): the vector loaded from its first element.</summary>
    private CExpr? TryCoerceToVector(CExpr value, CType.Vector vector)
    {
        // A vector of narrower integer lanes widens lane by lane (std.unicode's `@Vector(16, u8)` chunk passed where a
        // `@Vector(16, u16)` is expected, task #144): zig coerces when every value fits, as for a scalar.
        if (!vector.IsMask && value.Type.Unqualified is CType.Vector { IsMask: false } source && source.Count == vector.Count
            && source.Element.Unqualified is CType.Prim { Integer: true } from && vector.Element.Unqualified is CType.Prim { Integer: true } to
            && to.Bytes > from.Bytes && (to.Signed || !from.Signed))
        {
            if ((vector.UsesStorage || source.UsesStorage) && !(vector.Bits == 1024 && source.NetFamily == "Vector512"))
                throw UnsupportedStorageVector(vector.UsesStorage ? vector : source, "lane widening");
            var witness = new Cast(vector.Element, IntLit(0)) { Type = vector.Element };
            return new Call("ZigVec.Widen" + vector.Bits.ToString(System.Globalization.CultureInfo.InvariantCulture),
                new List<CExpr> { value, witness }) { Type = vector };
        }
        if (vector.IsMask || value.Type.Unqualified is CType.Vector) { return null; }
        CExpr? first = value.Type.Unqualified switch
        {
            CType.Array => PointedArray(value) is ({ } arr, _) ? arr : value,   // an array is its element pointer
            CType.Pointer => PointedArray(value) is ({ } parr, _) ? parr : value,
            CType.Slice => new Member(value, "Ptr", false) { Type = new CType.Pointer(vector.Element) },
            _ => null,
        };
        if (first is null) { return null; }
        return new Call("ZigVec.Load" + vector.Bits.ToString(System.Globalization.CultureInfo.InvariantCulture),
            new List<CExpr> { first })
        {
            Type = vector,
        };
    }

    // page.Mask loads eight packed rows/cells into native integer lanes.
    // Reuse the unaligned vector load only when source layout proves each
    // packed value fills one lane exactly: no padding, repacking or truncation.
    private CExpr? TryBitcastPackedArrayToVector(CExpr value, ZigResultLocation target)
    {
        if (target.Physical?.Unqualified is not CType.Vector { IsMask: false } vector
            || vector.Element.Unqualified is not CType.Prim { Integer: true, Bytes: 1 or 2 or 4 or 8 } lane
            || (!vector.UsesStorage && vector.NetFamily is null)
            || value.Type.Unqualified is not CType.Array { Count: { } count, Element.Unqualified: CType.Named packed } array
            || count != vector.Count
            || !_shared.BitcastWidths.PackedBits.TryGetValue(packed.Name, out var bits)
            || bits != lane.Bytes * 8 || target.Element?.IntegerBits != bits
            || _ir.SizeOfConst(array.Element) != lane.Bytes)
            return null;
        var first = PointedArray(value) is ({ } pointer, _) ? pointer : value;
        var pointerType = new CType.Pointer(vector.Element);
        var loaded = new Call("ZigVec.Load" + vector.Bits,
            new List<CExpr> { new Cast(pointerType, first) { Type = pointerType } }) { Type = vector };
        _shared.BitcastWidths.Expressions[loaded] = target;
        return loaded;
    }

    /// <summary>The operands of a shift whose amount is <c>@splat(n)</c> (std.math.rotr's <c>x &gt;&gt; @splat(ar)</c>), or
    /// null for any other operator (the caller lowers it as usual). zig gives a shift amount the result type
    /// <c>@Vector(N, Log2Int(T))</c>; the splatted count is the same for every lane, so it stays the one scalar count
    /// .NET's vector shift operator takes. Any other operand has no result type in zig (<c>v + @splat(1)</c> is
    /// "@splat must have a known result type"), and neither has a shift of a non-vector by a splat.</summary>
    private (CExpr Left, CExpr Right)? TrySplatBesideVector(BinOp op, Item l, Item r)
    {
        static Item? SplatArg(Item it) =>
            it.Content is Zig.BuiltinCall { Arg2: not null, Arg0: var tok } call && Tok(tok) == "@splat" && Flatten(call.Arg2) is [var one] ? one : null;
        if (op is not (BinOp.Shl or BinOp.Shr))
        {
            if (SplatArg(l) is not null || SplatArg(r) is not null)
            {
                throw new CompileException("zig: @splat must have a known result type (an operand of a binary operator has none; write `@as(V, @splat(x))`)");
            }
            return null;
        }
        if (SplatArg(l) is not null)
        {
            throw new CompileException("zig: @splat must have a known result type (the shifted operand has none; write `@as(V, @splat(x))`)");
        }
        if (SplatArg(r) is not { } count)
        {
            return null;
        }
        var left = LowerExpr(l);
        if (left.Type.Unqualified is not CType.Vector)
        {
            throw new CompileException($"zig: a `@splat` shift amount needs a vector to shift; `{left.Type.Describe()}` is not one");
        }
        return (left, LowerExpr(count));
    }

    /// <summary>A binary operator with a vector operand: arithmetic and bitwise ops are .NET's element-wise
    /// operators; a comparison is a lane mask (<c>ZigVec.Eq</c>, …). A scalar operand is splatted to the vector's
    /// lanes, as zig coerces it. Null when neither operand is a vector.</summary>
    private CExpr? TryVectorBinary(BinOp op, CExpr left, CExpr right)
    {
        var vector = left.Type.Unqualified as CType.Vector ?? right.Type.Unqualified as CType.Vector;
        if (vector is null) { return null; }
        var storage = left.Type.Unqualified as CType.Vector;
        if (storage?.UsesStorage != true) { storage = right.Type.Unqualified as CType.Vector; }
        if (storage?.UsesStorage == true)
        {
            if (op is BinOp.Shl or BinOp.Shr)
            {
                RequireIntegerStorageVector(storage);
                var counts = right.Type.Unqualified as CType.Vector;
                var countType = counts?.Element ?? right.Type;
                if (left.Type.Unqualified != storage || counts is not null && (counts.IsMask || counts.Count != storage.Count)
                    || countType.Unqualified is not CType.Prim { Integer: true, Bytes: 1 or 2 or 4 or 8 }
                    || counts is not null && countType.Unqualified is CType.Prim { Signed: true })
                    throw UnsupportedStorageVector(storage, "shifts need matching unsigned count lanes or a scalar splat count");
                return new Call("ZigVec." + op + "Storage", new List<CExpr>
                {
                    left, right, new Cast(storage.Element, IntLit(0)) { Type = storage.Element },
                    new Cast(countType, IntLit(0)) { Type = countType }, new LitBool(counts is null) { Type = CType.Bool },
                }) { Type = storage };
            }
            if (op is not (BinOp.BitAnd or BinOp.BitOr or BinOp.BitXor or BinOp.Eq or BinOp.Ne or BinOp.Lt or BinOp.Le or BinOp.Gt or BinOp.Ge))
                throw UnsupportedStorageVector(storage, $"binary `{op}`");
            RequireIntegerStorageVector(storage);
            if (left.Type.Unqualified != storage || right.Type.Unqualified != storage)
                throw new CompileException($"zig {storage.Describe()}: operands must have the same integer vector type");
            if (op is BinOp.Eq or BinOp.Ne or BinOp.Lt or BinOp.Le or BinOp.Gt or BinOp.Ge)
            {
                if (storage.Count > 64)
                    throw UnsupportedStorageVector(storage, "comparison results require a bool mask of at most 64 lanes");
                return new Call("ZigVec." + op, new List<CExpr> { left, right })
                    { Type = new CType.Vector(CType.Bool, storage.Count) };
            }
            return new Call("ZigVec." + op, new List<CExpr> { left, right }) { Type = storage };
        }
        CExpr Lanes(CExpr operand) => operand.Type.Unqualified is CType.Vector ? operand
            : new Call(VectorClass(vector) + ".Create", new List<CExpr> { new Cast(vector.Element, operand) { Type = vector.Element } }) { Type = vector };
        // A shift by one scalar count (a splatted amount) is .NET's vector shift operator, a logical right shift for unsigned
        // lanes. A count PER LANE has `Log2Int(T)` lanes, narrower than the value's, which .NET has no operator for.
        if (op is BinOp.Shl or BinOp.Shr && !vector.IsMask && left.Type.Unqualified is CType.Vector)
        {
            if (right.Type.Unqualified is CType.Vector)
            {
                if (right.Type.Unqualified is not CType.Vector { IsMask: false } counts || counts.Count != vector.Count
                    || vector.Element.Unqualified is not CType.Prim { Integer: true, Bytes: 1 or 2 or 4 or 8 }
                    || counts.Element.Unqualified is not CType.Prim { Integer: true, Signed: false, Bytes: 1 or 2 or 4 or 8 }
                    || BitcastLoweredLocation(left).Element?.IntegerBits is { } bits && bits != vector.Element.SizeOf * 8)
                    throw new IrUnsupportedException($"zig {vector.Describe()}: per-lane shifts need native integer value lanes and matching unsigned count lanes");
                var shifted = new Call("ZigVec." + op, new List<CExpr> { left, right,
                    new Cast(counts.Element, IntLit(0)) { Type = counts.Element } }) { Type = vector };
                _shared.BitcastWidths.Expressions[shifted] = BitcastLoweredLocation(left);
                return shifted;
            }
            return new Binary(op, left, new Cast(CType.Int, right) { Type = CType.Int }) { Type = vector };
        }
        var (l, r) = (Lanes(left), Lanes(right));
        if (vector.IsMask)
        {
            // A bool vector's `and` / `or` / `!=` are bit operations on its mask.
            return op switch
            {
                BinOp.BitAnd or BinOp.LogAnd => new Binary(BinOp.BitAnd, l, r) { Type = vector },
                BinOp.BitOr or BinOp.LogOr => new Binary(BinOp.BitOr, l, r) { Type = vector },
                BinOp.BitXor or BinOp.Ne => new Binary(BinOp.BitXor, l, r) { Type = vector },
                _ => throw new IrUnsupportedException($"zig {vector.Describe()}: `{op}` on a bool vector is not lowered yet"),
            };
        }
        var mask = new CType.Vector(CType.Bool, vector.Count);
        string? compare = op switch
        {
            BinOp.Eq => "Eq", BinOp.Ne => "Ne", BinOp.Lt => "Lt", BinOp.Gt => "Gt", BinOp.Le => "Le", BinOp.Ge => "Ge",
            _ => null,
        };
        if (compare is not null) { return new Call("ZigVec." + compare, new List<CExpr> { l, r }) { Type = mask }; }
        if (op is BinOp.Div or BinOp.Mod or BinOp.Shl or BinOp.Shr)
        {
            throw new IrUnsupportedException($"zig {vector.Describe()}: `{op}` on vectors is not lowered yet");
        }
        var result = new Binary(op, l, r) { Type = vector };
        // Bitwise expressions keep their lanes' source type. In particular a
        // following integer reduction must retain u5/i5 rather than the byte
        // carrier when its operand is an expression instead of a local read.
        if (op is BinOp.BitAnd or BinOp.BitOr or BinOp.BitXor
            && BitcastLoweredLocation(l).Element is { IntegerBits: { } laneBits } lane
            && BitcastLoweredLocation(r).Element?.IntegerBits == laneBits)
            _shared.BitcastWidths.Expressions[result] = new(vector, Element: lane);
        return result;
    }

    /// <summary><c>@reduce(op, v)</c>: over a bool vector a mask test (<c>.Or</c> any lane, <c>.And</c> every lane,
    /// <c>.Xor</c> an odd count), over a numeric one a <c>ZigVec.Reduce*</c>.</summary>
    private CExpr LowerReduce(IReadOnlyList<Item> args)
    {
        if (args.Count != 2 || EnumLitName(args[0]) is not { } op)
        {
            throw new IrUnsupportedException("zig `@reduce` expects (.Op, vector) with an enum-literal operator");
        }
        var v = LowerExpr(args[1]);
        if (v.Type.Unqualified is not CType.Vector vector)
        {
            throw new IrUnsupportedException($"zig `@reduce` needs a vector operand; got {v.Type.Describe()}");
        }
        if (vector.UsesStorage) { throw UnsupportedStorageVector(vector, "@reduce"); }
        if (vector.IsMask)
        {
            var zero = new LitInt("0", 0) { Type = CType.ULong };
            var full = new Call("ZigVec.Full", new List<CExpr> { IntLit(vector.Count) }) { Type = CType.ULong };
            return op switch
            {
                "Or" => new Binary(BinOp.Ne, v, zero) { Type = CType.Bool },
                "And" => new Binary(BinOp.Eq, v, full) { Type = CType.Bool },
                "Xor" => new Binary(BinOp.Ne,
                    new Binary(BinOp.BitAnd, new Call("System.Numerics.BitOperations.PopCount", new List<CExpr> { v }) { Type = CType.Int },
                        IntLit(1)) { Type = CType.Int },
                    IntLit(0)) { Type = CType.Bool },
                _ => throw new IrUnsupportedException($"zig `@reduce(.{op}, …)` over a bool vector is not meaningful"),
            };
        }
        if (op == "Or")
        {
            if (vector.Element.Unqualified is not CType.Prim
                { Integer: true, IsComptimeInt: false, Bytes: 1 or 2 or 4 or 8 })
                throw new IrUnsupportedException($"zig `@reduce(.Or, …)` requires supported integer lanes; got {vector.Describe()}"
                    + $" ({_module?.Path ?? _fileStem}:{args[1].Position.Line}, function '{_currentFnName}')");
            // A by-value helper argument evaluates the operand once. OR neither
            // widens a lane nor changes its signedness; retain the source width
            // as well as the physical result type for inferred narrow integers.
            var result = new Call("ZigVec.ReduceOr", new List<CExpr> { v }) { Type = vector.Element };
            _shared.BitcastWidths.CallResults[result] = new(vector.Element,
                BitcastSourceLocation(args[1], v).Element?.IntegerBits);
            return result;
        }
        if (op is not ("Add" or "Min" or "Max"))
        {
            throw new IrUnsupportedException($"zig `@reduce(.{op}, …)` over {vector.Describe()} is not lowered yet (.Or / .Add / .Min / .Max are)"
                + $" ({_module?.Path ?? _fileStem}:{args[1].Position.Line}, function '{_currentFnName}', container '{_currentContainer}')");
        }
        return new Call("ZigVec.Reduce" + op, new List<CExpr> { v }) { Type = vector.Element };
    }

    /// <summary><c>@select(T, mask, a, b)</c>: lane-wise <c>a</c> where the mask is set, else <c>b</c>.</summary>
    private CExpr LowerSelect(IReadOnlyList<Item> args, CType? sink)
    {
        if (args.Count != 4) { throw new IrUnsupportedException($"zig `@select` expects (T, mask, a, b); got {args.Count}"); }
        var mask = LowerExpr(args[1]);
        if (mask.Type.Unqualified is not CType.Vector { IsMask: true } maskType)
        {
            throw new IrUnsupportedException($"zig `@select` needs a bool-vector mask; got {mask.Type.Describe()}");
        }
        var vector = sink?.Unqualified as CType.Vector ?? new CType.Vector(LowerType(args[0]), maskType.Count);
        if (vector.UsesStorage) { throw UnsupportedStorageVector(vector, "@select"); }
        var a = LowerExprSink(args[2], vector);
        var b = LowerExprSink(args[3], vector);
        return new Call("ZigVec.Select", new List<CExpr> { mask, a, b }) { Type = vector };
    }

    /// <summary><c>@shuffle(E, a, b, mask)</c> (std.hash.XxHash3's <c>@shuffle(u64, data, undefined, [_]i32{ 1, 0, 3, 2, … })</c>,
    /// task #179): lane i of the result is <c>a[m]</c> for a non-negative mask element m and <c>b[~m]</c> for a negative one, the
    /// mask comptime-known. An operand no lane reads may be <c>undefined</c>; one read by several lanes is evaluated once.</summary>
    private CExpr LowerShuffle(IReadOnlyList<Item> args)
    {
        if (args.Count != 4) { throw new IrUnsupportedException($"zig `@shuffle` expects (E, a, b, mask); got {args.Count}"); }
        var element = LowerType(args[0]);
        CExpr maskValue;
        using (EnterThrowawayHoist()) { maskValue = LowerExpr(args[3]); }
        if (_ir.EvalComptimeValue(maskValue) is not IrBuilder.CtArray { Elems: var maskElems }
            || maskElems.Any(e => e is not IrBuilder.CtInt))
        {
            throw new IrUnsupportedException("zig `@shuffle` needs a comptime-known integer mask");
        }
        var mask = maskElems.Select(e => (long)((IrBuilder.CtInt)e).Value).ToList();
        CExpr? Operand(Item item, bool read)
        {
            if (!read) { return null; }
            if (item.Content is Zig.UndefinedLit) { throw new IrUnsupportedException("zig `@shuffle`: a lane reads an `undefined` operand"); }
            var lowered = LowerExpr(item);
            if (lowered.Type.Unqualified is not CType.Vector) { throw new IrUnsupportedException($"zig `@shuffle` needs vector operands; got {lowered.Type.Describe()}"); }
            if (lowered is VarRef) { return lowered; }
            var temp = _symbols.Declare(new Symbol { Name = "__shuf" + _anfTempCounter++, Kind = SymKind.Var, Type = lowered.Type });
            RequireHoistable("@shuffle").Add(new DeclStmt(new List<LocalDecl> { new(temp, lowered) }));
            return new VarRef(temp) { Type = temp.Type };
        }
        var a = Operand(args[1], mask.Any(m => m >= 0));
        var b = Operand(args[2], mask.Any(m => m < 0));
        var lanes = new List<CExpr>(mask.Count);
        foreach (var m in mask)
        {
            var (source, index) = m >= 0 ? (a, m) : (b, ~m);
            if (source is not { Type.Unqualified: CType.Vector sourceVector } || index >= sourceVector.Count)
            {
                throw new CompileException($"zig: `@shuffle` mask element {m} is out of range of its operand");
            }
            var lane = VectorLane(source, new LitInt(index.ToString(System.Globalization.CultureInfo.InvariantCulture), index) { Type = CType.Int }, sourceVector);
            lanes.Add(new Cast(element, lane) { Type = element });
        }
        var result = new CType.Vector(element, mask.Count);
        return new Call(VectorClass(result) + ".Create", lanes) { Type = result };
    }

    /// <summary><c>v[i]</c>: one lane of a numeric vector, or one bit of a bool vector's mask.</summary>
    private static CExpr VectorLane(CExpr vector, CExpr index, CType.Vector type) => type.IsMask
        ? new Cast(CType.Bool, new Call("ZigVec.Bit", new List<CExpr> { vector, index }) { Type = CType.Bool }) { Type = CType.Bool }
        : new Call("ZigVec.Get", new List<CExpr> { vector, index }) { Type = type.Element };

    /// <summary>A store to one LANE of a SIMD vector, <c>v[i] = x</c> or <c>v[i] op= x</c> (std.crypto.blake3's
    /// <c>result[i] = counterLow(counter + i);</c> in an <c>inline for</c>, task #155): a .NET vector is immutable, so the
    /// vector with that lane replaced is assigned back, <c>v = ZigVec.With(v, i, x)</c>. zig requires a store's index to
    /// be comptime-known ("vector index not comptime known"; a READ may take a runtime one), and so does dotcc. Null
    /// when the target is not a vector lane.</summary>
    private CExpr? TryVectorLaneStore(Item targetItem, BinOp? op, Item valueItem)
    {
        if (targetItem.Content is not Zig.Index ix) { return null; }
        CExpr probe;
        using (EnterThrowawayHoist()) { probe = LowerExpr(ix.Arg0); }
        // Zig implicitly dereferences a single-item pointer when indexing a
        // vector. Store the scalar lane in the pointed-to storage; a Get call
        // is a value and cannot be an assignment target. Capture the pointer
        // before lowering the RHS so an effectful receiver is evaluated once.
        if (probe.Type.Unqualified is CType.Pointer { Pointee: var pointee }
            && pointee.Unqualified is CType.Vector { IsMask: false } pointedVector
            && PointerSizeOfValue(ix.Arg0) is not ("many" or "c"))
        {
            if (pointee.IsConst) throw new CompileException("zig: cannot assign to constant vector storage");
            if (_hoist is null) throw new IrUnsupportedException("zig vector pointer lane store needs a statement scope");
            var pointer = LowerExpr(ix.Arg0);
            var savedPointer = _symbols.Declare(new Symbol
                { Name = "__vecptr" + _anfTempCounter++, Kind = SymKind.Var, Type = pointer.Type });
            _hoist.Add(new DeclStmt(new List<LocalDecl> { new(savedPointer, pointer) }));
            var index = LowerUsizeOperand(ix.Arg2);
            if (_ir.ConstEval(index) is null)
                throw new CompileException("zig: vector index not comptime known (a store to a vector lane needs a comptime-known index)");
            var lanes = new Cast(new CType.Pointer(pointedVector.Element), new VarRef(savedPointer) { Type = pointer.Type })
                { Type = new CType.Pointer(pointedVector.Element) };
            var lane = new DotCC.Ir.Index(lanes, index) { Type = pointedVector.Element, IsLValue = true };
            return new Assign(op, lane, LowerExprSink(valueItem, pointedVector.Element)) { Type = pointedVector.Element };
        }
        if (probe.Type.Unqualified is not CType.Vector vector) { return null; }
        if (vector.IsMask)
        {
            throw new IrUnsupportedException($"zig {vector.Describe()}: a store to one lane of a bool vector is not lowered yet");
        }
        var vec = LowerExpr(ix.Arg0);
        if (vec is not (VarRef or Member or DotCC.Ir.Index or Unary { Op: UnOp.Deref }))
        {
            throw new IrUnsupportedException("zig vector lane store: the vector must be a variable, a field, an element (`vecs[i][j]`, std.crypto.blake3's transposeNxN) or a dereference");
        }
        var idx = LowerUsizeOperand(ix.Arg2);
        if (_ir.ConstEval(idx) is null)
        {
            throw new CompileException("zig: vector index not comptime known (a store to a vector lane needs a comptime-known index)");
        }
        var value = LowerExprSink(valueItem, vector.Element);
        if (op is { } binOp)
        {
            // `v[i] +%= x`: the lane's new value, narrowed back to the lane type (C# widens a narrow operand to int).
            var combined = new Binary(binOp, VectorLane(vec, idx, vector), value) { Type = vector.Element };
            value = new Cast(vector.Element, combined) { Type = vector.Element };
        }
        // `v[3] = 0x00ff;` (task #188): a literal renders as a bare `255`, which C# types `int`, and `ZigVec.With<T>` then
        // infers no single T (CS1503); the lane value is given the lane type.
        if (value is LitInt || !value.Type.Unqualified.Equals(vector.Element.Unqualified))
        {
            value = new Cast(vector.Element, value) { Type = vector.Element };
        }
        var with = new Call("ZigVec.With", new List<CExpr> { vec, idx, value }) { Type = vector };
        return new Assign(null, vec, with) { Type = vector };
    }

    /// <summary>An <c>int</c> literal.</summary>
    private static LitInt IntLit(int n) => new(n.ToString(System.Globalization.CultureInfo.InvariantCulture), n) { Type = CType.Int };
}
