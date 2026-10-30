#nullable enable

using System.Linq;

namespace DotCC.Backends;

using DotCC.Ir;

/// <summary>
/// The .NET / C# backend's projection. Lowers to low-level <c>unsafe</c> C#: C's
/// <c>char</c> walks bytes (so <c>byte</c>), <c>_Bool</c> is the store-normalizing
/// <c>CBool</c> value type, every array decays to a flat pointer, and a function
/// type is a <c>delegate*</c>. This reproduces exactly the spellings the IR type
/// model used to bake in via the old <c>CType.CsType</c> property.
/// </summary>
internal sealed class CSharpTarget(bool instanceMethods = false, IReadOnlyDictionary<string, string>? aggregateNames = null) : ITarget
{
    // Aggregate bytes may only use the explicitly qualified layout profile.
    internal string StorageAbi => DotCC.Layout.OffsetLayoutModel.Abi;

    public string RenderType(CType t) => t switch
    {
        CType.Prim p => RenderPrim(p),
        CType.VoidType or CType.NoReturnType => "void",
        CType.Vector { IsMask: true } => "ulong",
        CType.Vector v => "System.Runtime.Intrinsics." + (v.NetFamily
            ?? throw new IrUnsupportedException("vector width has no runtime representation"))
            + "<" + RenderType(v.Element.Unqualified) + ">",
        // A pointer-TO-array collapses to the array's own flat row pointer (no extra
        // `*`); every other pointer is the pointee's spelling plus `*`.
        CType.Pointer ptr when VaListLifetimeValidator.IsVaList(ptr.Pointee) => "ref VaList",
        CType.Pointer ptr => ptr.Pointee is CType.Array ? RenderType(ptr.Pointee) : RenderType(ptr.Pointee) + "*",
        // An array lowers to a single pointer to its innermost scalar.
        CType.Array a => RenderType(a.FlatElement) + "*",
        // A native-call-conv fn-ptr (a dlsym'd address) lowers to an unmanaged cdecl
        // delegate* so the JIT/AOT uses the C calling convention (matching the
        // `-shared` exports' [UnmanagedCallersOnly(CallConvs=CallConvCdecl)]); the
        // `CallConvCdecl` modifier resolves without a using. Default (managed) is
        // unchanged — `&fn` of dotcc's own methods stays a managed delegate*.
        CType.Func { IsNativeCallConv: true } when instanceMethods => throw new CompileException("native function-pointer ABI is unsupported with --instance-methods"),
        CType.Func f => (f.IsNativeCallConv ? "delegate* unmanaged[Cdecl]<" : "delegate*<")
            + (instanceMethods && !f.IsNativeCallConv ? "DotCcFunctions, " : "")
            + string.Join(", ", f.Params.Where(p => p.Unqualified is not CType.VoidType).Select(RenderType)
                .Concat(f.Variadic && !f.IsNativeCallConv ? new[] { "global::System.ReadOnlySpan<VaArg>" } : global::System.Array.Empty<string>())
                .Append(RenderType(f.Return))) + ">",
        CType.Named { IsExternal: true } n => n.Name,
        CType.Named n => NoteNamed(n.Name, aggregateNames?.GetValueOrDefault(n.Name, EmitHelpers.Id(n.Name)) ?? EmitHelpers.Id(n.Name)),
        CType.Enum e => aggregateNames?.GetValueOrDefault(e.Name, EmitHelpers.Id(e.Name)) ?? EmitHelpers.Id(e.Name),
        CType.ComplexType => "global::System.Numerics.Complex",
        CType.Float128Type => "Float128",
        CType.LongDoubleStorage16 => "LongDouble80Storage",
        // A Zig value optional `?T` → C# Nullable<T> (`T?`): null = none, `.?` = .Value,
        // `orelse` = `??`. (An optional POINTER `?*T` is a bare nullable `T*`, never this.)
        CType.Optional { Inner.Unqualified: CType.Array optionalArray } => OptionalArrayName(optionalArray),
        CType.Optional o => RenderType(o.Inner) + "?",
        // A Zig error union `E!T` → the runtime `ErrUnion<Payload>` value type. A `void`
        // payload (`!void`) has no generic-over-void in C#, so it uses the `Unit` payload.
        CType.ErrorUnion eu => "ErrUnion<" + (
            eu.Payload is CType.VoidType ? "Unit"
            // `Error!*T` (Milestone U `create`): a pointer can't be an `ErrUnion<T>` generic arg, so
            // the address rides as a `nuint`; the `try` unwrap casts it back to `T*`.
            : eu.Payload.Unqualified is CType.Pointer ? "nuint"
            : RenderType(eu.Payload)) + ">",
        // A Zig error-set value (a bare `error.Foo` / a captured error, Milestone N) → the raw
        // `ushort` error code. V1 erases the named set into one flat code space, so an error value
        // IS its code; error-value equality and a future error `switch` compare the codes directly.
        CType.ErrorSetType => "ushort",
        // A Zig slice `[]T` → the runtime `Slice<T>` fat-pointer value type; `[]const T`
        // (a const-qualified element) → `ConstSlice<T>`. The element is rendered unqualified
        // (the const lives in the slice type's identity, not a C# `const`).
        CType.Slice s => SliceType(s.Element.Unqualified, s.Element.IsConst),
        // A Zig `std.mem.Allocator` → the runtime `Allocator` fat-pointer value type
        // (Milestone F). The concrete `FixedBufferAllocator` is a `CType.Named` (renders its name).
        CType.Allocator => "Allocator",
        // Zig's curated `std.ArrayList(T)` → the runtime `ZigList<T>` value type (wall-plan W0).
        // The element renders unqualified, like the slice above.
        CType.ZigList zl => "ZigList<" + RenderType(zl.Element.Unqualified) + ">",
        // A Zig tuple `struct { T1, T2, … }` → `System.ValueTuple<T1, …>` (Milestone G).
        // Arity-uniform — including arity 1 (`System.ValueTuple<T>`), where C#'s `(T)` shorthand
        // would be a parenthesised expression, not a tuple. Empty → the non-generic `System.ValueTuple`;
        // arity > 7 nests via the 8th `TRest` field (`ValueTuple<T1..T7, ValueTuple<T8..>>`).
        CType.Tuple tup => RenderValueTuple(tup.Elements),
        _ => throw new IrUnsupportedException("C# target cannot render type " + t.GetType().Name),
    };

    /// <summary>Render a <c>System.ValueTuple</c> type of arbitrary arity: empty → the non-generic
    /// <c>System.ValueTuple</c>; 1..7 → <c>ValueTuple&lt;T1, …&gt;</c>; &gt; 7 → the first 7 plus an
    /// 8th <c>TRest</c> that is itself a <c>ValueTuple</c> of the remaining elements (C#'s open-arity
    /// ValueTuple nesting).</summary>
    private string RenderValueTuple(IReadOnlyList<CType> elems)
    {
        if (elems.Count == 0) { return "global::System.ValueTuple"; }
        if (elems.Count <= 7)
        {
            return "global::System.ValueTuple<" + string.Join(", ", elems.Select(TupleElementType)) + ">";
        }
        var head = string.Join(", ", elems.Take(7).Select(TupleElementType));
        var rest = RenderValueTuple(elems.Skip(7).ToList());
        return "global::System.ValueTuple<" + head + ", " + rest + ">";
    }

    // Pointer-shaped tuple slots need a legal generic argument. Keep the
    // logical type in the IR; only the ValueTuple storage carries address bits.
    internal static bool TuplePointerSlot(CType type) => type.Unqualified is CType.Pointer or CType.Array or CType.Func;
    internal string TupleElementType(CType type) => TuplePointerSlot(type) ? "nint" : RenderType(type.Unqualified);

    internal HashSet<string> RenderedNamedTypes { get; } = new(StringComparer.Ordinal);

    private string NoteNamed(string name, string rendered)
    {
        RenderedNamedTypes.Add(name);
        return rendered;
    }

    public string RenderIntLit(LitInt lit) =>
        lit.Type.Unqualified is CType.Prim { Integer: true, Bytes: >= 16 } p128
            ? Render128Lit(lit.Digits, p128.Signed)
            : lit.Digits + IntSuffix(lit.Type);

    /// <summary>Emit a 128-bit integer literal. C# has no <c>Int128</c>/<c>UInt128</c> literal
    /// suffix, so a magnitude that fits <c>ulong</c> is written as a plain literal cast to the
    /// 128-bit type (the cast pins overload resolution), and a larger magnitude — which has no C#
    /// literal form at all — is materialized via <c>Parse</c>. The digit string is the normalized
    /// decimal magnitude (any sign rides on an outer <c>Unary(Neg)</c>).</summary>
    private static string Render128Lit(string digits, bool signed)
    {
        var ty = signed ? "global::System.Int128" : "global::System.UInt128";
        return ulong.TryParse(digits, out _) ? $"({ty}){digits}UL" : $"{ty}.Parse(\"{digits}\")";
    }

    /// <summary>The C# integer-literal suffix for a type: <c>u</c> (uint), <c>L</c>
    /// (long), <c>UL</c> (ulong), none (int / narrower) — reproducing exactly what
    /// the builder used to append before the suffix moved behind this seam. 128-bit types are
    /// handled separately by <see cref="Render128Lit"/> (no C# suffix exists for them).</summary>
    private static string IntSuffix(CType t) => t.Unqualified is CType.Prim { Integer: true } p
        ? (p.Signed, p.Bytes >= 8) switch
        {
            (true, false) => "",
            (false, false) => "u",
            (true, true) => "L",
            (false, true) => "UL",
        }
        : "";

    public string RenderFloatLit(LitFloat lit)
    {
        var text = lit.Text;
        var dot = text.IndexOf('.');
        // C accepts an empty fractional part (`1.`, `1.f`, `1.e2`). C#
        // requires a digit there. Insert it without reparsing or rounding the
        // significand; leave nonliteral spellings such as double.NaN intact.
        if (dot > 0 && char.IsAsciiDigit(text[dot - 1])
            && (dot + 1 == text.Length || text[dot + 1] is 'f' or 'F' or 'e' or 'E'))
            text = text.Insert(dot + 1, "0");
        if (lit.Type.Unqualified == CType.Float && text.Length > 0 && text[^1] is not ('f' or 'F')) return text + "F";
        if (lit.Type.Unqualified is CType.Float128Type)
        {
            if (Binary128Literal.IsExactDouble(lit.Text)) return "Float128.FromDouble(" + text + ")";
            var (hi, lo) = Binary128Literal.ToBits(lit.Text);
            return System.FormattableString.Invariant($"Float128.FromBits(new System.UInt128(0x{hi:X16}UL, 0x{lo:X16}UL))");
        }
        return text;
    }

    /// <summary>Map a C primitive (keyed on its canonical C name) to the C# type it
    /// lowers to. <c>char</c>→<c>byte</c> so <c>char*</c> arithmetic walks bytes;
    /// <c>_Bool</c>→<c>CBool</c> for C store-normalization; <c>long</c>/<c>long
    /// long</c> are both 64-bit <c>long</c> (LP64).</summary>
    private static string RenderPrim(CType.Prim p) => p.Name switch
    {
        "_Bool" => "CBool",
        "char" => "byte",
        "char8_t" => "byte",    // C23 char8_t → C# byte (an 8-bit UTF-8 code unit)
        "signed char" => "sbyte",
        "unsigned char" => "byte",
        "short" => "short",
        "unsigned short" => "ushort",
        "char16_t" => "char",   // C11 char16_t → C# char (both 16-bit UTF-16 code units)
        "wchar_t" => "char",    // wchar_t → C# char — dotcc's MSVC-shaped 16-bit UTF-16 wchar_t
        "char32_t" => "uint",   // C11 char32_t → C# uint (a 32-bit UTF-32 code unit)
        "int" => "int",
        "unsigned int" => "uint",
        "long" => "long",
        "unsigned long" => "ulong",
        "long long" => "long",
        "unsigned long long" => "ulong",
        "__int128" => "global::System.Int128",            // C __int128 / Zig i128 → BCL Int128
        "unsigned __int128" => "global::System.UInt128",  // C unsigned __int128 / Zig u128 → BCL UInt128
        "float" => "float",
        "double" => "double",
        "long double" => "double",
        _ => throw new IrUnsupportedException("C# target has no spelling for primitive " + p.Name),
    };
    /// <summary>The C# slice type over <paramref name="element"/> (a zig <c>[]T</c>, or <c>[]const T</c> when
    /// <paramref name="isConst"/>). A pointer is no C# type argument, so two element kinds take another shape: a slice of
    /// ARRAYS (<c>[][8]u32</c>, task #152) views rows of one flat run, so it is a slice of the innermost element whose
    /// <c>.Len</c> counts rows (<c>s[i]</c> is <c>s.Ptr + i * N</c>); a slice of POINTERS (<c>[][*]const u8</c>, task #153)
    /// is the runtime's <c>PtrSlice</c> over the pointees, whose <c>.Ptr</c> is a <c>T**</c>.</summary>
    internal string SliceType(CType element, bool isConst)
    {
        if (element is CType.Array rows)
        {
            return (isConst ? "ConstSlice<" : "Slice<") + RenderType(rows.FlatElement.Unqualified) + ">";
        }
        if (PtrSliceTarget(element) is { } target)
        {
            return (isConst ? "ConstPtrSlice<" : "PtrSlice<") + RenderType(target) + ">";
        }
        return (isConst ? "ConstSlice<" : "Slice<") + RenderType(element) + ">";
    }

    /// <summary>The pointee a slice of <paramref name="element"/> is a <c>PtrSlice</c> over (see <see cref="SliceType"/>),
    /// or null when the element is not a pointer that shape covers. Also the type argument of a <c>ZigMem</c> helper over
    /// such a slice, whose <c>PtrSlice</c> overload takes the pointee (task #193).</summary>
    internal static CType? PtrSliceTarget(CType element)
        // A pointer to an array renders as the array's flat element pointer, so its pointee is that element.
        => element is CType.Pointer { Pointee: var pointee }
           && (pointee.Unqualified is CType.Array pointedRows ? pointedRows.FlatElement : pointee).Unqualified
               is var target && target is not (CType.Pointer or CType.VoidType or CType.Func or CType.Array)
            ? target
            : null;

    /// <summary>The generated value types standing for zig optional arrays <c>?[N]T</c> (task #151), by name: the
    /// element's C# spelling and the flat element count. Filled as types render; the backend emits one declaration per
    /// entry once everything has rendered (<see cref="OptionalArrayTypesText"/>).</summary>
    private readonly SortedDictionary<string, (string Element, int Count)> _optionalArrays = new(System.StringComparer.Ordinal);

    /// <summary>The name of the value type standing for <c>?[N]T</c>, registering it for <see cref="OptionalArrayTypesText"/>.
    /// Its elements are a <c>fixed</c> buffer, so the element must be a primitive C# allows there.</summary>
    private string OptionalArrayName(CType.Array array)
    {
        var element = RenderType(array.FlatElement);
        var spelled = "?[" + (array.Count?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "_") + "]" + array.Element.Describe();
        if (!CSharpBackend.IsFixedBufferType(element))
        {
            throw new IrUnsupportedException(
                $"zig optional array `{spelled}`: only an array of integers or floats is supported yet");
        }
        var count = FlatCount(array)
            ?? throw new IrUnsupportedException($"zig optional array `{spelled}`: the array needs a comptime-known length");
        var name = "ZigOptArray_" + element + "_" + count.ToString(System.Globalization.CultureInfo.InvariantCulture);
        _optionalArrays[name] = (element, count);
        return name;
    }

    /// <summary>The flat element count of a (possibly nested) array, or null when a dimension is not comptime-known.</summary>
    private static int? FlatCount(CType t) => t.Unqualified is CType.Array { Count: var n } a
        ? n is int outer && FlatCount(a.Element) is int inner ? outer * inner : null
        : 1;

    /// <summary>One declaration per zig optional array type rendered so far (task #151): the elements inline in a
    /// <c>fixed</c> buffer beside a has-value flag, behind the <c>HasValue</c> / <c>Value</c> surface the lowering reads
    /// off a <c>Nullable</c>. <c>Value</c> is the element pointer (an array IS its element pointer here); a <c>T*</c>
    /// converts in by copying the elements, a null one to none (so <c>x = null</c> and a <c>null</c> field default need no
    /// rewrite); <c>x == null</c> tests for none, the only comparison zig allows an optional array.</summary>
    internal string OptionalArrayTypesText()
    {
        var sb = new System.Text.StringBuilder();
        foreach (var (name, (element, count)) in _optionalArrays)
        {
            var bytes = $"sizeof({element}) * {count}";
            sb.Append("/// <summary>zig `?[").Append(count).Append(']').Append(element).Append("`: the elements inline beside a has-value flag.</summary>\n")
              .Append("struct ").Append(name).Append("\n{\n")
              .Append("    public unsafe fixed ").Append(element).Append(" Buf[").Append(count).Append("];\n")
              .Append("    public bool HasValue;\n")
              .Append("    public unsafe ").Append(element).Append("* Value => HasValue ? (").Append(element)
              .Append("*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref this) : throw new System.InvalidOperationException(\"attempt to use null value\");\n")
              .Append("    public static unsafe implicit operator ").Append(name).Append('(').Append(element).Append("* p)\n    {\n")
              .Append("        var r = default(").Append(name).Append(");\n")
              .Append("        if (p == null) { return r; }\n")
              .Append("        System.Buffer.MemoryCopy(p, r.Buf, ").Append(bytes).Append(", ").Append(bytes).Append(");\n")
              .Append("        r.HasValue = true;\n        return r;\n    }\n")
              .Append("    public static unsafe bool operator ==(").Append(name).Append(" a, ").Append(element)
              .Append("* b) => b == null ? !a.HasValue : throw new System.InvalidOperationException(\"zig compares an optional array only with null\");\n")
              .Append("    public static unsafe bool operator !=(").Append(name).Append(" a, ").Append(element).Append("* b) => !(a == b);\n")
              .Append("    public override bool Equals(object o) => false;\n")
              .Append("    public override int GetHashCode() => HasValue ? 1 : 0;\n")
              .Append("}\n\n");
        }
        return sb.ToString();
    }
}

/// <summary>The .NET / C# backend's identifier policy: escape C# keywords with
/// <c>@</c>, forbid shadowing (CS0136), uniquify with a <c>__k</c> suffix.</summary>
internal sealed class CSharpNameLegalizer : INameLegalizer
{
    public string Escape(string rawName) => DotCC.EmitHelpers.Id(rawName);
    public bool ForbidsShadowing => true;
    public string Uniquify(string escaped, int collision) => $"{escaped}__{collision}";
}
