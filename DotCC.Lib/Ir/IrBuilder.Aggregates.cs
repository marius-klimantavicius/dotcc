#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Item = global::LALR.CC.LexicalGrammar.Item;

namespace DotCC.Ir;

/// <summary>
/// Aggregate-initializer lowering for the typed IR: brace initializers, C99
/// designated initializers (<c>.field =</c> / <c>[i] =</c>), nested-brace and
/// multi-dimensional arrays, struct-element arrays, compound literals, and the
/// C23 empty initializer. Everything resolves against the TARGET <see cref="CType"/>
/// (the binder knows the field/element types), so a value's lowering — including
/// a bare function name decaying to <c>&amp;fn</c> — falls out of the typed nodes
/// rather than any text rewriting.
/// </summary>
internal sealed partial class IrBuilder
{
    // ---- structured brace-initializer tree -------------------------------
    // A brace initializer parses into a small structured tree so designators and
    // nesting survive into the type-directed interpretation below. Leaf values are
    // built to CExpr eagerly (BuildExpr needs no target type); the target type is
    // applied when the tree is interpreted against a struct/array.

    private abstract record Init;
    private sealed record InitVal(CExpr Value) : Init;
    private sealed record InitGroup(IReadOnlyList<Init> Items) : Init;
    private sealed record InitAt(int Index, CExpr Value) : Init;

    /// <summary>Parse an <c>InitList</c> (its element list, with the optional
    /// trailing comma) into the structured init tree.</summary>
    private List<Init> ParseInitList(Item initList)
    {
        var items = new List<Init>();
        // A C23 #embed element expands IN PLACE to its file bytes as integer
        // constants — so `{ #embed "f" }` fills a char array, `{ 1, #embed "f", 2 }`
        // splices into a mixed list, and a non-char element type takes one int per
        // byte. This serves every initializer-list shape; string and embed
        // initializers converge on the same array node.
        void AddElem(Init e)
        {
            if (e is InitVal { Value: EmbedData ed })
            {
                GuardEmbedSize(ed.Bytes.Count);
                var inv = System.Globalization.CultureInfo.InvariantCulture;
                foreach (var b in ed.Bytes)
                {
                    items.Add(new InitVal(new LitInt(b.ToString(inv), b) { Type = CType.Int }));
                }
            }
            else { items.Add(e); }
        }
        void Walk(Item n)
        {
            switch (n.Content)
            {
                case C.InitListCons c: Walk(c.Arg0); AddElem(ParseInitElem(c.Arg2)); break;
                case C.InitListTrail t: Walk(t.Arg0); break;     // trailing comma — no element
                case C.InitListOne o: AddElem(ParseInitElem(o.Arg0)); break;
                default: AddElem(ParseInitElem(n)); break;
            }
        }
        Walk(initList);
        return items;
    }

    /// <summary>Upper bound on a single <c>#embed</c>'s byte count. The bytes
    /// ultimately materialise as a <c>new byte[]{…}</c> source-text literal, so
    /// the ceiling limits source size and any startup copy. Exceeding it
    /// is a loud error, never a silent truncation.</summary>
    private const int MaxEmbedBytes = 4 * 1024 * 1024;

    private static void GuardEmbedSize(int count)
    {
        if (count > MaxEmbedBytes)
        {
            throw new IrUnsupportedException(
                $"#embed payload is {count} bytes, exceeding dotcc's {MaxEmbedBytes}-byte source-emit limit");
        }
    }

    private Init ParseInitElem(Item it) => it.Content switch
    {
        C.InitElemExpr e => new InitVal(BuildExpr(e.Arg0)),
        C.InitElemNest nest => new InitGroup(ParseInitList(nest.Arg1)),
        C.InitElemDesignated d => Gated(1999, "array designators", it, new InitAt(
            ConstEval(BuildExpr(d.Arg1)) is { } ix && ix >= 0 ? (int)ix
                : throw new IrUnsupportedException("array designator index must be a constant non-negative integer"),
            BuildExpr(d.Arg4))),
        _ => new InitVal(BuildExpr(it)),
    };

    // ---- struct / union aggregates ---------------------------------------

    /// <summary>Build a positional struct/union aggregate initializer — zips the
    /// brace elements onto the fields in declaration order, recursing into a nested
    /// brace over a struct/union-typed field. Trailing fields the list doesn't
    /// reach are omitted (C# zero-fills them — C's partial-init rule). This is the
    /// one place the legacy emitter's positional <c>BuildAggregateInit</c> and the
    /// struct-array element builder converge.</summary>
    private StructInit BuildStructPositional(CType type, IReadOnlyList<Init> items)
    {
        // C's universal {0} initializer zeroes the complete aggregate, including
        // nested arrays. Avoid interpreting its one scalar as an array pointer.
        if (items is [InitVal { Value: LitInt { Value: 0 } }])
            return new StructInit(System.Array.Empty<FieldInit>()) { Type = type };
        // Anonymous bit-fields (padding) take no initializer in C — drop them so the
        // positional values land on the accessible members in declaration order.
        var fields = StructFieldsOf(type).Where(f => !f.IsAnonBitField).ToList();
        var members = new List<FieldInit>(Math.Min(items.Count, fields.Count));
        for (var i = 0; i < items.Count && i < fields.Count; i++)
        {
            var field = fields[i];
            var value = TryBuildStaticFlexibleMember(type, field, items[i], out var flexible)
                ? flexible : BuildFieldInitializer(field.Type, items[i]);
            members.Add(new FieldInit(field.Name, field.Type, value));
        }
        return new StructInit(members) { Type = type };
    }

    private CExpr BuildFieldInitializer(CType type, Init initializer)
    {
        if (type.Unqualified is CType.Array array)
        {
            var dimensions = new List<int>();
            for (CType current = array; current.Unqualified is CType.Array dimension; current = dimension.Element)
                dimensions.Add(dimension.Count ?? throw new IrUnsupportedException("inline array initializer requires a constant extent"));
            if (dimensions.Any(count => count <= 0))
                throw new IrUnsupportedException("initializer for flexible or zero-length array member");
            if (TryInlineStringInitializer(array, initializer, out var stringValues))
                return new InlineArrayInit(array.FlatElement, stringValues) { Type = type };
            var items = initializer is InitGroup group ? group.Items
                : throw new IrUnsupportedException("inline array member initializer requires braces");
            var values = BuildArrayElems(array.FlatElement, dimensions, items);
            if (values.Count > dimensions.Aggregate(1, (left, right) => checked(left * right)))
                throw new IrUnsupportedException("too many initializers for inline array member");
            return new InlineArrayInit(array.FlatElement, values) { Type = type };
        }
        return initializer switch
        {
            InitVal value => value.Value,
            InitGroup group when type.Unqualified is CType.Named named && _structFields.ContainsKey(named.Name)
                => BuildStructPositional(type, group.Items),
            InitGroup { Items.Count: 0 } => new DefaultLit { Type = type },
            InitGroup { Items.Count: 1 } group => BuildFieldInitializer(type, group.Items[0]),
            _ => throw new IrUnsupportedException("invalid initializer for aggregate member"),
        };
    }

    private bool TryInlineStringInitializer(CType.Array array, Init initializer, out List<CExpr> values)
    {
        values = new List<CExpr>();
        var literal = initializer switch
        {
            InitVal value => value.Value,
            InitGroup { Items: [InitVal value] } => value.Value,
            _ => null,
        };
        if (literal is LitStr or LitU16Str or LitU32Str && array.Element.Unqualified is not CType.Array)
        {
            var name = (array.Element.Unqualified as CType.Prim)?.Name;
            var characters = literal switch
            {
                LitStr text when name is "char" or "signed char" or "unsigned char" or "char8_t"
                    => DotCC.EmitHelpers.StringByteValues(text.Segments),
                LitU16Str text when name is "char16_t" or "wchar_t"
                    => DotCC.EmitHelpers.StringU16Values(text.Segments),
                LitU32Str text when name is "char32_t"
                    => DotCC.EmitHelpers.StringU32Values(text.Segments),
                _ => throw new IrUnsupportedException("string literal is incompatible with inline array element type"),
            };
            var count = array.Count!.Value;
            // C allows an exact-size character array without the terminator;
            // every actual character must still fit. Shorter literals include
            // their terminator and zero-fill the remaining inline storage.
            if (characters.Count > count)
                throw new IrUnsupportedException($"string literal is too long for inline array member [{count}]");
            for (var index = 0; index < count; ++index)
            {
                var value = index < characters.Count ? characters[index] : 0;
                values.Add(new LitInt(value.ToString(System.Globalization.CultureInfo.InvariantCulture), value) { Type = CType.Int });
            }
            return true;
        }
        if (array.Element.Unqualified is CType.Array inner && initializer is InitGroup group && ContainsString(group))
        {
            if (group.Items.Count > array.Count)
                throw new IrUnsupportedException("too many string initializers for inline array member");
            foreach (var item in group.Items)
            {
                var row = (InlineArrayInit)BuildFieldInitializer(inner, item);
                values.AddRange(row.Elems);
            }
            var total = 1;
            for (CType current = array; current.Unqualified is CType.Array dimension; current = dimension.Element)
                total = checked(total * dimension.Count!.Value);
            while (values.Count < total) values.Add(Zero);
            return true;
        }
        return false;
    }

    private static bool ContainsString(Init initializer) => initializer switch
    {
        InitVal { Value: LitStr or LitU16Str or LitU32Str } => true,
        InitGroup group => group.Items.Any(ContainsString),
        _ => false,
    };

    /// <summary>Build a C99 designated struct/union initializer
    /// (<c>{ .x = 1, .y = 2 }</c>). The user named the fields, so each member's
    /// field type comes from the struct table (driving the store coercion) and the
    /// order may differ from declaration — C# object initializers allow both, and
    /// omitted fields take their zero default.</summary>
    private StructInit BuildStructDesignated(CType type, Item memberList)
    {
        StructFieldsOf(type); // Validate the target before interpreting designators.
        var members = new List<FieldInit>();
        foreach (var (field, valueItem) in ParseMemberInits(memberList))
            SetDesignatedMember(type, members, field, valueItem);
        return new StructInit(members) { Type = type };
    }

    /// <summary>Resolve promoted names through actual anonymous storage. Several
    /// designators into the same anonymous struct share its initializer; choosing
    /// another union member discards the previous active member's initializer.
    /// A repeated designator replaces its earlier value, as in C.</summary>
    private void SetDesignatedMember(CType type, List<FieldInit> members, string field, Item valueItem)
    {
        var canonical = ((CType.Named)type.Unqualified).Name;
        if (_promoted.TryGetValue(canonical, out var promoted) && promoted.TryGetValue(field, out var path))
        {
            SelectUnionMember(path.Hidden);
            var index = members.FindIndex(member => member.Name == path.Hidden);
            var nestedType = new CType.Named(path.Nested);
            var nestedMembers = index >= 0 && members[index].Value is StructInit previous
                ? previous.Members.ToList() : new List<FieldInit>();
            SetDesignatedMember(nestedType, nestedMembers, field, valueItem);
            Store(index, new FieldInit(path.Hidden, nestedType, new StructInit(nestedMembers) { Type = nestedType }));
            return;
        }

        var fields = StructFieldsOf(type);
        var fieldIndex = fields.FindIndex(member => member.Name == field);
        if (fieldIndex < 0)
            throw new IrUnsupportedException($"unknown initializer member '{field}' in struct/union '{canonical}'");
        var definition = fields[fieldIndex];
        SelectUnionMember(field);
        Store(members.FindIndex(member => member.Name == field),
            new FieldInit(field, definition.Type, TryBuildStaticFlexibleMember(type, definition, valueItem, out var flexible)
                ? flexible : BuildDeclaratorInitializer(definition.Type, valueItem)));

        void SelectUnionMember(string name)
        {
            if (_structIsUnion.GetValueOrDefault(canonical) && members.Count != 0 && members[0].Name != name)
                members.Clear();
        }
        void Store(int index, FieldInit member)
        {
            if (index < 0) members.Add(member);
            else members[index] = member;
        }
    }

    /// <summary>The struct/union fields named by <paramref name="type"/>, or throw
    /// if it isn't a known aggregate.</summary>
    private List<StructField> StructFieldsOf(CType type)
    {
        var canonical = (type.Unqualified as CType.Named)?.Name
            ?? throw new IrUnsupportedException("aggregate initializer for a non-struct type");
        return _structFields.TryGetValue(canonical, out var fields) ? fields
            : throw new IrUnsupportedException($"aggregate initializer for unknown struct/union '{canonical}'");
    }

    /// <summary>Collect a <c>MemberInitList</c>'s <c>.field = value</c> items, in
    /// source order.</summary>
    private List<(string field, Item value)> ParseMemberInits(Item memberList)
    {
        var outp = new List<(string, Item)>();
        void Add(Item mi)
        {
            switch (mi.Content)
            {
                case C.MemberInit m: outp.Add((Tok(m.Arg1), m.Arg3)); break;
                case C.MemberInitBrace m: outp.Add((Tok(m.Arg1), m.Arg4)); break;
                case C.MemberInitDesignated m: outp.Add((Tok(m.Arg1), m.Arg4)); break;
                case C.MemberInitEmpty m: outp.Add((Tok(m.Arg1), mi)); break;
                default: throw new IrUnsupportedException(TypeName(mi.Content));
            }
        }
        void Walk(Item n)
        {
            switch (n.Content)
            {
                case C.MemberInitListCons c: Walk(c.Arg0); Add(c.Arg2); break;
                case C.MemberInitListTrail t: Walk(t.Arg0); break;
                case C.MemberInitListOne o: Add(o.Arg0); break;
                default: Add(n); break;
            }
        }
        Walk(memberList);
        return outp;
    }

    // ---- array aggregates ------------------------------------------------

    /// <summary>Interpret a brace initializer against an array TARGET, returning the
    /// dense element list codegen lays into a <c>stackalloc</c>. Dispatches on the
    /// element type and shape: a struct element type maps each top-level group to a
    /// <see cref="StructInit"/>; C99 array designators (<c>[i] =</c>) fill a sparse
    /// 1-D array; constant dimensions flatten a nested/flat scalar initializer with
    /// C's per-dimension zero-fill; an implicit <c>[]</c> takes the values as-is.</summary>
    /// <param name="dims">The constant dimension sizes, or null when implicit
    /// (<c>[]</c>) or non-constant.</param>
    private List<CExpr> BuildArrayElems(CType elem, IReadOnlyList<int>? dims, IReadOnlyList<Init> items)
    {
        var elemName = (elem.Unqualified as CType.Named)?.Name;
        if (elemName is not null && _structFields.ContainsKey(elemName))
        {
            // A struct/union element can initialize its members with braces or
            // copy an already-typed aggregate value (a variable/call/literal).
            var outp = new List<CExpr>(items.Count);
            foreach (var it in items)
            {
                switch (it)
                {
                    case InitGroup group:
                        outp.Add(BuildStructPositional(elem, group.Items));
                        break;
                    case InitVal value when value.Value.Type.Unqualified.Equals(elem.Unqualified):
                        outp.Add(value.Value);
                        break;
                    default:
                        throw new IrUnsupportedException($"each element of a '{elemName}' array initializer must be a brace group or a compatible aggregate expression");
                }
            }
            if (dims is { Count: > 0 })
            {
                var count = dims.Aggregate(1, (left, right) => checked(left * right));
                if (outp.Count > count)
                    throw new IrUnsupportedException("too many initializers for aggregate array");
                while (outp.Count < count) outp.Add(new DefaultLit { Type = elem });
            }
            return outp;
        }
        if (items.Any(i => i is InitAt))
        {
            if (dims is { Count: > 1 }) { throw new IrUnsupportedException("array designators on a multi-dimensional array"); }
            return DesignatedArrayValues(elem, items, dims is { Count: 1 } ? dims[0] : -1);
        }
        if (dims is { Count: > 0 })
        {
            // A string initializes one character-array subobject, not a scalar
            // pointer in the flattened backing store. Reuse the same target-
            // directed rule as inline array fields, including row zero-fill.
            // Arrays of character pointers still contain ordinary pointer values.
            if (elem.Unqualified is CType.Prim { Name: "char" or "signed char" or "unsigned char" or "char8_t" or "char16_t" or "wchar_t" or "char32_t" }
                && TryInlineStringInitializer((CType.Array)MakeArrayType(elem, dims), new InitGroup(items), out var strings))
                return strings;
            return FlattenScalarArray(elem, items, dims);
        }
        // implicit `[]` scalar array — the values as written.
        return items.Select(it => it is InitVal v ? v.Value
            : throw new IrUnsupportedException("nested brace in an implicitly-sized scalar array")).ToList();
    }

    /// <summary>Build the dense, zero-filled value list for a 1-D scalar array with
    /// C99 array designators: a <c>[i] =</c> moves the cursor to <c>i</c>, an
    /// undesignated value fills the cursor, both advance it (a later write to the
    /// same index wins). <paramref name="declaredSize"/> is the constant size, or
    /// -1 to derive it from the highest index touched (the implicit form).</summary>
    private List<CExpr> DesignatedArrayValues(CType elem, IReadOnlyList<Init> items, int declaredSize)
    {
        var slots = new Dictionary<int, CExpr>();
        int cursor = 0, maxIndex = -1;
        foreach (var it in items)
        {
            switch (it)
            {
                case InitAt d: cursor = d.Index; slots[cursor] = d.Value; break;
                case InitVal v: slots[cursor] = v.Value; break;
                default: throw new IrUnsupportedException("nested brace mixed with array designators");
            }
            if (cursor > maxIndex) { maxIndex = cursor; }
            cursor++;
        }
        var size = declaredSize >= 0 ? declaredSize : maxIndex + 1;
        if (maxIndex >= size) { throw new IrUnsupportedException($"array designator index {maxIndex} is out of bounds for [{size}]"); }
        var outp = new List<CExpr>(size);
        for (var i = 0; i < size; i++) { outp.Add(slots.TryGetValue(i, out var v) ? v : Zero); }
        return outp;
    }

    /// <summary>Flatten a (possibly nested) scalar array initializer against the
    /// constant dimensions, applying C's per-dimension zero-fill, to exactly
    /// product(dims) values. Handles the fully-flat (<c>{1,2,3,4,5,6}</c>) and
    /// fully-nested (<c>{{1,2,3},{4,5,6}}</c>) shapes; an irregular mix fails
    /// loudly rather than miscompile.</summary>
    private List<CExpr> FlattenScalarArray(CType elem, IReadOnlyList<Init> items, IReadOnlyList<int> dims)
    {
        var total = 1;
        foreach (var d in dims) { total *= d; }
        var outp = new List<CExpr>(total);
        if (items.All(i => i is InitVal))
        {
            foreach (var it in items) { outp.Add(((InitVal)it).Value); }
            if (outp.Count > total) { throw new IrUnsupportedException("too many initializers for array"); }
        }
        else
        {
            FlattenNested(items, dims, 0, outp);
        }
        while (outp.Count < total) { outp.Add(Zero); }   // zero-fill the tail
        return outp;
    }

    private void FlattenNested(IReadOnlyList<Init> items, IReadOnlyList<int> dims, int dimIdx, List<CExpr> outp)
    {
        if (items.Count > dims[dimIdx]) { throw new IrUnsupportedException("too many initializers for an array dimension"); }
        if (dimIdx == dims.Count - 1)
        {
            foreach (var it in items)
            {
                if (it is InitVal v) { outp.Add(v.Value); }
                else { throw new IrUnsupportedException("irregular nested array initializer"); }
            }
            for (var k = items.Count; k < dims[dimIdx]; k++) { outp.Add(Zero); }
        }
        else
        {
            var subSize = 1;
            for (var i = dimIdx + 1; i < dims.Count; i++) { subSize *= dims[i]; }
            foreach (var it in items)
            {
                if (it is InitGroup g) { FlattenNested(g.Items, dims, dimIdx + 1, outp); }
                else { throw new IrUnsupportedException("irregular nested array initializer (mixed braces and scalars)"); }
            }
            for (var k = items.Count; k < dims[dimIdx]; k++)
            {
                for (var z = 0; z < subSize; z++) { outp.Add(Zero); }
            }
        }
    }

    /// <summary>The integer constant 0 — the zero-fill element. C# converts the
    /// int literal to any scalar element type in an array initializer.</summary>
    private static LitInt Zero => new("0", 0) { Type = CType.Int };

    /// <summary>Build the nested C array type from outer→inner dimensions
    /// (<c>[2][3]</c> → <c>Array(Array(elem, 3), 2)</c>). The nesting is what lets a
    /// partial subscript yield an inner array (and stride correctly); the storage
    /// and the backend's flat-pointer projection still collapse to one pointer.</summary>
    private static CType MakeArrayType(CType elem, IReadOnlyList<int> dims)
    {
        var t = elem;
        for (var i = dims.Count - 1; i >= 0; i--) { t = new CType.Array(t, dims[i]); }
        return t;
    }

    /// <summary>A pointer-to-array declaration <c>T (*p)[N]…</c> — a pointer whose
    /// pointee is an array (a row pointer into a 2-D array). Lowered to a flat
    /// pointer that strides by the array's extent; the type carries the nested array
    /// pointee so subscript striding and <c>sizeof</c> resolve.</summary>
    private DeclStmt BuildPtrToArr(Item typeItem, Item nameItem, Item dimsItem, Item? initItem)
    {
        var elem = ResolveType(typeItem);
        var dims = TryConstDims(dimsItem) ?? throw new IrUnsupportedException("pointer-to-array needs constant dimensions");
        var type = new CType.Pointer(MakeArrayType(elem, dims));
        var sym = _symbols.Declare(new Symbol { Name = Tok(nameItem), Kind = SymKind.Var, Type = type, Storage = Storage.Auto });
        return new DeclStmt(new[] { new LocalDecl(sym, initItem is { } ii ? BuildExpr(ii) : null) });
    }

    /// <summary>The constant dimension sizes of an <c>ArrDims</c> node, or null when
    /// any dimension isn't an integer constant expression (a VLA-ish extent).</summary>
    private List<int>? TryConstDims(Item arrDims)
    {
        var outp = new List<int>();
        foreach (var d in BuildArrDims(arrDims))
        {
            if (ConstEval(d) is { } n) { outp.Add((int)n); }
            else { return null; }
        }
        return outp;
    }

    // ---- compound literals (C99 / C23) -----------------------------------

    /// <summary>A struct/union (or scalar) compound literal <c>(T){ … }</c> — an
    /// unnamed object usable in any expression position. A struct lowers to a
    /// <see cref="StructInit"/> (<c>new T { … }</c>); a scalar/pointer/enum to a
    /// single-value cast (<c>(T)(v)</c>).</summary>
    private CExpr BuildCompoundLit(Item typeItem, Item initListItem)
    {
        var type = ResolveType(typeItem);
        if ((type.Unqualified as CType.Named)?.Name is { } canonical && _structFields.ContainsKey(canonical))
        {
            return BuildStructPositional(type, ParseInitList(initListItem));
        }
        var items = ParseInitList(initListItem);
        if (items is [InitVal one]) { return new Cast(type, one.Value) { Type = type }; }
        throw new IrUnsupportedException($"compound literal of non-aggregate type '{type.Describe()}' needs exactly one value");
    }

    private CExpr BuildCompoundLitDesignated(Item typeItem, Item memberList) =>
        BuildStructDesignated(ResolveType(typeItem), memberList);

    private CExpr BuildCompoundLitEmpty(Item typeItem) =>
        new DefaultLit { Type = ResolveType(typeItem) };

    /// <summary>An array compound literal <c>(T[]){…}</c> / <c>(T[N]){…}</c> — a
    /// <see cref="StackArray"/> value (codegen: a <c>stackalloc</c>, valid in
    /// initializer position).</summary>
    private CExpr BuildArrayCompoundLit(Item elemTypeItem, Item? dimsItem, Item initListItem)
    {
        var elem = ResolveType(elemTypeItem);
        var dims = dimsItem is { } di ? TryConstDims(di) : null;
        var elems = BuildArrayElems(elem, dims, ParseInitList(initListItem));
        return new StackArray(elem, elems) { Type = new CType.Array(elem, elems.Count) };
    }

    // ---- file-scope / static-local arrays --------------------------------

    /// <summary>Complete only the omitted outer bound. Explicit row groups each
    /// initialize one subarray; a flat scalar list fills consecutive rows. A
    /// string initializes a character row as a whole, including any zero fill.</summary>
    private List<int> InferOuterArrayDimensions(CType elem, IReadOnlyList<int>? inner, IReadOnlyList<Init> items)
    {
        if (inner is not { Count: > 0 } || inner.Any(count => count <= 0))
            throw new IrUnsupportedException("inferred outer array extent requires positive constant inner dimensions");
        if (items.Count == 0 || items.Any(item => item is InitAt))
            throw new IrUnsupportedException("inferred multidimensional array requires a nonempty undesignated initializer");
        if (elem.Unqualified is CType.Named aggregate && _structFields.ContainsKey(aggregate.Name))
            throw new IrUnsupportedException("inferred multidimensional arrays of struct/union elements are not supported");
        var rowSize = inner.Aggregate(1, (left, right) => checked(left * right));
        var characterRows = inner.Count == 1
            && elem.Unqualified is CType.Prim { Name: "char" or "signed char" or "unsigned char" or "char8_t" or "char16_t" or "wchar_t" or "char32_t" }
            && ContainsString(new InitGroup(items));
        var outer = items.Any(item => item is InitGroup) || characterRows
            ? items.Count : 1 + (items.Count - 1) / rowSize;
        // Existing target-directed lowering validates row contents and bounds;
        // it also rejects irregular mixed brace/scalar shapes explicitly.
        return new[] { outer }.Concat(inner).ToList();
    }

    // A C file-scope array (and a block-scope `static` array, which shares its
    // static storage duration) persists for the program lifetime, so it can't be a
    // block `stackalloc`. Both lower to a pinned global field (a PinnedArray init);
    // a static local additionally gets a program-unique mangled name + an alias
    // symbol so the function body's references resolve to that field.

    /// <summary>Build a file-scope array <see cref="GlobalVar"/> (a pinned backing
    /// store). When <paramref name="csName"/> is non-null this is a static local —
    /// the field takes that mangled name and an alias symbol is registered so
    /// in-function uses resolve to it; otherwise it's a file-scope name.</summary>
    private void BuildGlobalArr(Item typeItem, Item nameItem, Item? dimsItem, Item? initItem, string? csName, bool inferOuter = false)
    {
        _sawThreadLocalSpec = false;
        var element = csName is not null ? ResolveStaticLocalType(typeItem) : ResolveType(typeItem);
        var threadLocal = _sawThreadLocalSpec;
        BuildGlobalArr(element, nameItem, dimsItem, initItem, csName, DeclarationAlignment(typeItem), inferOuter, threadLocal);
    }

    private void BuildGlobalArr(CType elem, Item nameItem, Item? dimsItem, Item? initItem, string? csName, int alignment = 0, bool inferOuter = false, bool threadLocal = false)
    {
        var name = Tok(nameItem);
        var dims = dimsItem is { } di ? TryConstDims(di) ?? throw new IrUnsupportedException("file-scope array requires a constant bound") : null;

        CType arrType;
        CExpr init;
        if (initItem is { } ii)
        {
            var items = ParseInitList(ii);
            if (inferOuter) dims = InferOuterArrayDimensions(elem, dims, items);
            var elems = BuildArrayElems(elem, dims, items);
            arrType = dims is { Count: >= 1 } ? MakeArrayType(elem, dims) : new CType.Array(elem, elems.Count);
            init = new PinnedArray(elem, elems, null) { Type = new CType.Pointer(elem) };
        }
        else if (dims is { Count: >= 1 })
        {
            var total = 1;
            foreach (var d in dims) { total *= d; }
            arrType = MakeArrayType(elem, dims);
            init = new PinnedArray(elem, null, new LitInt(total.ToString(System.Globalization.CultureInfo.InvariantCulture), total) { Type = CType.Int }) { Type = new CType.Pointer(elem) };
        }
        else
        {
            throw new IrUnsupportedException($"file-scope array '{name}' needs a constant size or an initializer");
        }

        AddGlobalArray(name, arrType, init, csName, alignment, threadLocal);
    }

    /// <summary>Register a global-array symbol and its <see cref="GlobalVar"/>. A
    /// non-null <paramref name="csName"/> marks a static local (mangled field name +
    /// alias symbol); otherwise it's a file-scope name.</summary>
    private void AddGlobalArray(string name, CType arrType, CExpr init, string? csName, int alignment = 0, bool threadLocal = false)
    {
        if (csName is not null)
        {
            var sym = new Symbol { Name = name, Alignment = alignment, Kind = SymKind.Var, Type = arrType, Storage = Storage.Static, IsGlobal = true, IsThreadLocal = threadLocal, TargetName = csName };
            RegisterStaticLocal(sym, init);
        }
        else
        {
            var sym = _symbols.Declare(new Symbol { Name = name, Alignment = alignment, Kind = SymKind.Var, Type = arrType, Storage = Storage.Static, IsGlobal = true, IsThreadLocal = threadLocal });
            Globals.Add(new GlobalVar(sym, init));
        }
    }

    /// <summary>A file-scope / static-local char array initialized from a string
    /// literal (<c>char tag[] = "…"</c>) — a pinned byte array of the decoded bytes
    /// plus the NUL, zero-padded to an explicit size (or truncated, C's rule).</summary>
    private void BuildGlobalCharArr(Item typeItem, Item nameItem, Item strSeqItem, Item? dimsItem, string? csName, bool wide = false)
    {
        _sawThreadLocalSpec = false;
        var elem = csName is not null ? ResolveStaticLocalType(typeItem) : ResolveType(typeItem);
        var threadLocal = _sawThreadLocalSpec;
        var bytes = WideArrValues(elem, strSeqItem, wide);
        bytes.Add(0);   // NUL
        var dims = dimsItem is { } di ? TryConstDims(di) : null;
        var total = dims is { Count: >= 1 } ? dims.Aggregate(1, (a, b) => a * b) : bytes.Count;
        var inv = System.Globalization.CultureInfo.InvariantCulture;
        var elems = new List<CExpr>(total);
        for (var i = 0; i < total; i++)
        {
            var v = i < bytes.Count ? bytes[i] : 0;   // zero-pad beyond the string
            elems.Add(new LitInt(v.ToString(inv), v) { Type = CType.Int });
        }
        AddGlobalArray(Tok(nameItem), new CType.Array(elem, total),
            new PinnedArray(elem, elems, null) { Type = new CType.Pointer(elem) }, csName, DeclarationAlignment(typeItem), threadLocal);
    }

    /// <summary>An <c>extern T a[N];</c> / <c>extern T a[];</c> declaration — storage
    /// lives in another TU (or a later same-TU definition), so emit no field; just
    /// register the name's type so same-TU references resolve (a sized extent keeps
    /// the array type for <c>sizeof</c>; an incomplete one decays to a pointer).</summary>
    private void BuildExternArr(Item typeItem, Item nameItem, Item? dimsItem)
    {
        _sawThreadLocalSpec = false;
        var element = ResolveType(typeItem);
        BuildExternArr(element, nameItem, dimsItem, _sawThreadLocalSpec);
    }

    private void BuildExternArr(CType elem, Item nameItem, Item? dimsItem, bool threadLocal = false)
    {
        var dims = dimsItem is { } di ? TryConstDims(di) : null;
        var type = dims is { Count: >= 1 } ? MakeArrayType(elem, dims) : new CType.Pointer(elem);
        _symbols.Declare(new Symbol { Name = Tok(nameItem), Kind = SymKind.Var, Type = type, Storage = Storage.Extern, IsGlobal = true, IsThreadLocal = threadLocal });
    }

    /// <summary>A block-scope <c>static T a[…]</c> — a pinned global field under a
    /// program-unique mangled name, with the statement itself emitting nothing.</summary>
    private CStmt BuildStaticLocalArr(Item typeItem, Item nameItem, Item? dimsItem, Item? initItem, bool inferOuter = false)
    {
        var csName = $"{_symbols.Escape(Tok(nameItem))}__s{_staticLocalSeq++}";
        BuildGlobalArr(typeItem, nameItem, dimsItem, initItem, csName, inferOuter);
        return new DeclStmt(System.Array.Empty<LocalDecl>());
    }

    /// <summary>A block-scope <c>static char a[] = "…"</c> — a pinned global char
    /// array under a mangled name (the statement emits nothing).</summary>
    private CStmt BuildStaticLocalCharArr(Item typeItem, Item nameItem, Item strSeqItem, Item? dimsItem, bool wide = false)
    {
        var csName = $"{_symbols.Escape(Tok(nameItem))}__s{_staticLocalSeq++}";
        BuildGlobalCharArr(typeItem, nameItem, strSeqItem, dimsItem, csName, wide);
        return new DeclStmt(System.Array.Empty<LocalDecl>());
    }
}
