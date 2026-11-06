using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotCC.Ir;
using Index = DotCC.Ir.Index;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    private bool _hasZigStorage;
    private const string ZigCarrier = "__dotcc_zig_storage";
    private readonly Dictionary<string, (string Name, string Text)> _zigPlaceHelpers = new(StringComparer.Ordinal);
    private bool _zigFunctionActive;
    private string FinishZigStorageFunction(string text)
    {
        _zigFunctionActive = false;
        if (_zigPlaceHelpers.Count == 0) return text;
        var open = text.IndexOf("\n{\n", StringComparison.Ordinal);
        if (open < 0) throw new IrUnsupportedException("Zig place helper requires a function body");
        return text.Insert(open + 3, string.Concat(_zigPlaceHelpers.Values.Select(h => h.Text)));
    }
    private ZigStorageLayout? ZigStorage(CType type) => type.Unqualified is CType.Named n
        && _aggregateDefinitions.TryGetValue(n.Name, out var t) ? t.ZigStorage : null;
    private bool ZigZero(CType type) => ZigStorage(type)?.Kind == ZigStorageKind.Zero;
    private static string ZigMask(int bits, bool wide = false) => wide
        ? bits == 128 ? "global::System.UInt128.MaxValue" : $"((global::System.UInt128.One << {bits}) - global::System.UInt128.One)"
        : bits == 64 ? "ulong.MaxValue" : "0x" + ((1UL << bits) - 1).ToString("X") + "UL";
    private static string ZigWord(ZigStorageLayout proof) => proof.LogicalBits > 64 ? "global::System.UInt128" : "ulong";
    private static string ZigCarrierType(ZigStorageLayout proof) => proof.NativeSize switch
        { 1 => "byte", 2 => "ushort", 4 => "uint", 8 => "ulong", 16 => "global::System.UInt128", _ => throw new IrUnsupportedException("invalid Zig carrier size") };

    private bool HasZigValue(CType type, HashSet<string>? active = null)
    {
        if (type.IsZigZeroInteger) return true;
        if (ZigStorage(type) is not null) return true;
        active ??= new(StringComparer.Ordinal);
        return type.Unqualified switch
        {
            CType.Named n when active.Add(n.Name) && _aggregateDefinitions.TryGetValue(n.Name, out var t) => t.Fields.Any(f => HasZigValue(f.Type, active)),
            CType.Array a => HasZigValue(a.Element, active),
            CType.Optional o => HasZigValue(o.Inner, active),
            CType.ErrorUnion e => HasZigValue(e.Payload, active),
            _ => false,
        };
    }
    private bool ZeroPointer(CType type) => type.Unqualified is CType.Pointer p && ZigZero(p.Pointee);
    private void ZigNativeSignature(CType.Func signature, string operation)
    {
        if (HasZigValue(signature.Return) || signature.Params.Any(p => HasZigValue(p)))
            throw new IrUnsupportedException("zig storage native ABI: packed/zero by-value " + operation + " requires scalar carrier thunks");
        if (ZeroPointer(signature.Return) || signature.Params.Any(ZeroPointer))
            throw new IrUnsupportedException("zig zero place cannot escape through native " + operation);
    }
    private void ValidateZigType(CType type, bool array = false, int depth = 0)
    {
        ValidateZeroIntegerType(type);
        if (!_hasZigStorage) return;
        if (depth > 64) throw new IrUnsupportedException("zig storage type nesting limit");
        if (ZigStorage(type) is { } proof)
        {
            proof.RequireSupported();
            if (array && proof.Kind == ZigStorageKind.Zero)
                throw new IrUnsupportedException("zig zero-element arrays/slices require zero-stride views (unsupported)");
        }
        switch (type.Unqualified)
        {
            case CType.Array a: ValidateZigType(a.Element, true, depth + 1); break;
            case CType.Slice s: ValidateZigType(s.Element, true, depth + 1); break;
            case CType.Pointer p: ValidateZigType(p.Pointee, false, depth + 1); break;
            case CType.Func f:
                if (f.IsNativeCallConv) ZigNativeSignature(f, "callback");
                ValidateZigType(f.Return, false, depth + 1);
                foreach (var parameter in f.Params) ValidateZigType(parameter, false, depth + 1);
                break;
            case CType.Optional o: ValidateZigType(o.Inner, false, depth + 1); break;
            case CType.ErrorUnion e: ValidateZigType(e.Payload, false, depth + 1); break;
        }
    }
    private void ValidateZigStorage(IrBuilder unit)
    {
        foreach (var global in unit.Globals) ValidateZeroIntegerType(global.Sym.Type);
        foreach (var function in unit.Functions) ValidateZeroIntegerType(function.Sym.Type);
        if (!_hasZigStorage) return;
        foreach (var global in unit.Globals)
        {
            ValidateZigType(global.Sym.Type);
            if (ZeroPointer(global.Sym.Type)) throw new IrUnsupportedException("zig zero place cannot escape into global storage");
        }
        foreach (var prototype in unit.ProtoOnlyReferenced.Values)
            if (prototype.Type.Unqualified is CType.Func f) ZigNativeSignature(f, "import");
        foreach (var fn in unit.Functions)
        {
            ValidateZigType(fn.Sym.Type);
            if (fn.Sym.Type.Unqualified is CType.Func signature)
            {
                if (ZeroPointer(signature.Return)) throw new IrUnsupportedException("zig zero place lifetime: pointer return is unsupported");
                // This is exactly the backend's export-candidate set. Managed private copies remain source-typed.
                if (!unit.ZigManagedFunctionPointers && fn.Sym.Name != "main" && fn.Sym.Storage != Storage.Static) ZigNativeSignature(signature, "export");
            }
        }
    }
    // Imported declarations are registered speculatively. Validate a type's fields
    // when it is selected for emission, so an unused platform layout stays inert.
    private void ValidateZigAggregate(StructTypeDef t)
    {
        if (t.Fields.Any(field => ContainsZeroInteger(field.Type)))
            throw new IrUnsupportedException("zig u0 aggregate field storage requires an erasure proof (unsupported): " + t.Name);
        if (!_hasZigStorage) return;
        t.ZigStorage?.RequireSupported();
        if (t.ZigStorage is { Kind: ZigStorageKind.PackedInteger or ZigStorageKind.Zero } proof
            && (proof.SourceMembers.Count != t.Fields.Count || proof.SourceMembers.Where((field, i) =>
                field.Name != t.Fields[i].Name || field.SourceType != t.Fields[i].Type || field.TargetName != FieldTarget(t.Fields[i])).Any()))
            throw new IrUnsupportedException("zig storage source field proof changed after registration: " + t.Name);
        if (t.ZigZeroOwner is { } owner && !owner.SourceMembers.SequenceEqual(t.Fields))
            throw new IrUnsupportedException("zig zero-owner structural proof changed after registration: " + t.Name);
        foreach (var field in t.Fields)
        {
            ValidateZigType(field.Type);
            if (ZeroPointer(field.Type)) throw new IrUnsupportedException("zig zero place cannot be retained in aggregate storage");
        }
    }
    private void ValidateZigExpression(CExpr expression)
    {
        ValidateZigType(expression.Type);
        if (expression is VarRef { Sym: { Kind: SymKind.Func, FromSystemHeader: true }, Type.Unqualified: CType.Func { IsNativeCallConv: true } })
            throw new IrUnsupportedException("zig native extern function address requires a Cdecl pointer resolver/thunk (unsupported)");
        if (expression is Call { CalleeType: { IsNativeCallConv: true } native }) ZigNativeSignature(native, "call/import");
        if (expression is IndirectCall indirect && indirect.Callee.Type.Unqualified is CType.Func { IsNativeCallConv: true } callback)
            ZigNativeSignature(callback, "callback call");
        if (expression is ZigMemCall memory) ValidateZigType(memory.Element, array: true);
        if (expression is BitCast bitcast && (bitcast.Operand.Type.Unqualified is CType.Array a && HasZigValue(a.Element)
            || bitcast.Type.Unqualified is CType.Array b && HasZigValue(b.Element)))
            throw new IrUnsupportedException("zig packed arrays in bitcasts require element witnesses (unsupported)");
        if (expression is Unary { Op: UnOp.AddrOf, Operand: { } operand } && TryZigPackedPlace(operand) is not null)
            throw new IrUnsupportedException("zig packed bit-pointer address/ABI is unsupported; a source field is not an ordinary ref");
        if (expression is Binary { Op: BinOp.Add or BinOp.Sub } binary && (ZeroPointer(binary.Left.Type) || ZeroPointer(binary.Right.Type))
            || expression is Assign { CompoundOp: not null } assignment && ZeroPointer(assignment.Target.Type)
            || expression is Unary { Op: UnOp.PreInc or UnOp.PostInc or UnOp.PreDec or UnOp.PostDec } unary && ZeroPointer(unary.Operand.Type)
            || expression is Index index && ZigZero(index.Type))
            throw new IrUnsupportedException("zig zero pointer arithmetic/indexing requires zero-stride places (unsupported)");
        if (expression is Cast c && ZeroPointer(c.Operand.Type) && c.Target.Unqualified is CType.Pointer && !ZeroPointer(c.Target))
            throw new IrUnsupportedException("zig zero place loses its storage/lifetime proof through pointer conversion");
    }

    private string ZigBits(string value, CType type, bool wide = false)
    {
        var word = wide ? "global::System.UInt128" : "ulong";
        if (ZigStorage(type) is { Kind: ZigStorageKind.PackedInteger }) return $"(({word})({value}).{ZigCarrier})";
        if (wide && type.Unqualified is CType.Enum enumeration) return $"unchecked((global::System.UInt128)({Cs(enumeration.Underlying)})({value}))";
        // CBool has a ulong conversion; C# cannot chain it with UInt128's conversion.
        if (wide && type.Unqualified is CType.Prim { Name: "_Bool" }) return $"((global::System.UInt128)(ulong)({value}))";
        return $"unchecked(({word})({value}))";
    }
    private string ZigExtract(string bits, CType type, int offset, int width, bool signed, bool wide = false)
    {
        var raw = $"(({bits} >> {offset}) & {ZigMask(width, wide)})";
        if (ZigStorage(type) is { Kind: ZigStorageKind.PackedInteger } nested)
            return $"new {Cs(type)} {{ {ZigCarrier} = ({ZigCarrierType(nested)}){raw} }}";
        if (type.Unqualified is CType.Prim { Name: "_Bool" }) return $"((CBool)({raw} != 0))";
        if (signed)
        {
            var wordBits = wide ? 128 : 64;
            var signedWord = wide ? "global::System.Int128" : "long";
            if (width < wordBits) raw = $"(unchecked(({signedWord})({raw} << {wordBits - width})) >> {wordBits - width})";
        }
        // UInt128/Int128 use operators, which C# cannot chain with an enum conversion.
        if (wide && type.Unqualified is CType.Enum enumeration) raw = $"({Cs(enumeration.Underlying)})({raw})";
        return $"unchecked(({Cs(type)})({raw}))";
    }
    private string ZigPatch(ZigStorageLayout root, string current, string value, CType type, int offset, int width)
        => $"unchecked(({ZigCarrierType(root)})((({ZigWord(root)}){current} & ~({ZigMask(width, root.LogicalBits > 64)} << {offset})) | (({ZigBits(value, type, root.LogicalBits > 64)} & {ZigMask(width, root.LogicalBits > 64)}) << {offset})))";

    // Rooted place: child carriers never become writable ref-returning temporaries.
    private static string ZigPlaceSegment(string source) => Convert.ToBase64String(Encoding.UTF8.GetBytes(source));
    private sealed record ZigPackedPlace(CExpr Root, ZigStorageLayout Storage, int Offset, int Width, bool Signed, CType Type, string Path);
    private ZigPackedPlace? TryZigPackedPlace(CExpr expression)
    {
        if (expression is Paren p) return TryZigPackedPlace(p.Inner);
        if (expression is not Member member) return null;
        var owner = member.Arrow && member.Base.Type.Unqualified is CType.Pointer pointer ? pointer.Pointee : member.Base.Type;
        if (ZigStorage(owner) is not { Kind: ZigStorageKind.PackedInteger } proof) return null;
        var field = proof.SourceMembers.SingleOrDefault(f => f.Name == member.Field)
            ?? throw new IrUnsupportedException("zig packed place has no exact source field: " + member.Field);
        if (!member.Arrow && TryZigPackedPlace(member.Base) is { } parent)
            return parent with { Offset = checked(parent.Offset + field.BitOffset), Width = field.LogicalBits,
                Signed = field.Signed, Type = member.Type, Path = parent.Path + "|" + ZigPlaceSegment(field.Name) };
        CExpr root = member.Arrow ? new Unary(UnOp.Deref, member.Base) { Type = owner, IsLValue = true } : member.Base;
        return new(root, proof, field.BitOffset, field.LogicalBits, field.Signed, member.Type, ZigPlaceSegment(field.Name));
    }

    private string ZigPlaceAssignment(Assign assignment, ZigPackedPlace place)
    {
        if (!place.Root.IsLValue) throw new IrUnsupportedException("zig packed store requires an addressable root place");
        var operation = assignment.CompoundOp;
        var wrapping = assignment.ZigWrappingBits is not null;
        if (wrapping && !_canHoist)
            throw new IrUnsupportedException("zig packed wrapping assignment in a loop update/expression requires a scoped storage capture (unsupported)");
        if (operation is not null && (ZigStorage(place.Type) is not null
            || place.Type.Unqualified is CType.Prim { Name: "_Bool" }
                && operation is not (BinOp.BitAnd or BinOp.BitOr or BinOp.BitXor)))
            throw new IrUnsupportedException("zig packed compound operation requires an integer/enum field or boolean bitwise operation");
        if (!_zigFunctionActive) throw new IrUnsupportedException("zig packed stores in computed static initialization require a scoped place host (unsupported)");
        var key = ZigPlaceSegment(place.Storage.SourceIdentity) + "|" + place.Path + ":" + operation + ":" + assignment.ZigWrappingBits;
        var helper = "__dotcc_zig_set_" + AggregateIdentities.Hash(key)[..20];
        if (_zigPlaceHelpers.TryGetValue(key, out var known)) helper = known.Name;
        else { while (_initializerNames.Contains(helper) || !_addressLocalNames.Add(helper)) helper += "_"; }
        var rootType = Cs(place.Root.Type);
        var valueType = Cs(place.Type);
        var read = ZigExtract($"({ZigWord(place.Storage)})root.{ZigCarrier}", place.Type, place.Offset, place.Width, place.Signed, place.Storage.LogicalBits > 64);
        var value = operation is null ? "value" : $"unchecked(({valueType})({(wrapping ? "old" : read)} {BinSym(operation.Value)} "
            + (operation is BinOp.Shl or BinOp.Shr ? "(int)value" : "value") + "))";
        var definition = $"    static {valueType} {helper}(ref {rootType} root, {(wrapping ? valueType + " old, " : "")}{valueType} value)\n    {{\n"
            + $"        {valueType} updated = {value};\n        root.{ZigCarrier} = {ZigPatch(place.Storage, "root." + ZigCarrier, "updated", place.Type, place.Offset, place.Width)};\n"
            + $"        return {read};\n    }}\n";
        _zigPlaceHelpers[key] = (helper, definition);
        // Function-local static helpers remain with their object function; type coalescing cannot drop a path.
        // ref arguments designate storage once; argument evaluation preserves base/index before RHS.
        var root = BareLValue(place.Root);
        if (_canHoist)
        {
            var captured = ZigPlaceLocal("__zig_root");
            _pending.Add($"ref {rootType} {captured} = ref {root}");
            root = captured;
        }
        string oldArgument = "";
        if (wrapping)
        {
            var old = ZigPlaceLocal("__zig_old");
            _pending.Add($"{valueType} {old} = {ZigExtract($"({ZigWord(place.Storage)}){root}.{ZigCarrier}", place.Type, place.Offset, place.Width, place.Signed, place.Storage.LogicalBits > 64)}");
            oldArgument = old + ", ";
        }
        var rhs = Coerced(assignment.Value, place.Type);
        return $"{helper}(ref {root}, {oldArgument}{rhs})";
    }

    private string ZigPlaceLocal(string prefix)
    {
        string name;
        do { name = prefix + _clCounter++; } while (_initializerNames.Contains(name) || !_addressLocalNames.Add(name));
        return name;
    }

    private string ZigZeroOperation(CExpr target, CExpr? value)
    {
        if (target is Paren p) return ZigZeroOperation(p.Inner, value);
        var type = Cs(target.Type);
        string Rhs() => value is null ? $"default({type})" : Coerced(value, target.Type);
        string Capture(string declaration, string text)
        {
            if (!_canHoist) return text;
            var name = ZigPlaceLocal("__zig_zero_base");
            _pending.Add(declaration.Replace("$name", name, StringComparison.Ordinal) + text);
            return name;
        }
        if (target is Member m)
        {
            if (m.Arrow)
            {
                var address = Capture("nuint $name = ", $"(nuint)({Expr(m.Base)})");
                return $"{type}.__dotcc_zero_pointer({address}, {Rhs()})";
            }
            if (m.Base.IsLValue && !ZigZero(m.Base.Type))
            {
                var owner = Capture($"ref {Cs(m.Base.Type)} $name = ref ", BareLValue(m.Base));
                return $"{type}.__dotcc_zero_owner(ref {owner}, {Rhs()})";
            }
            var ownerValue = Capture($"{Cs(m.Base.Type)} $name = ", Expr(m.Base));
            return $"{type}.__dotcc_zero_value_owner({ownerValue}, {Rhs()})";
        }
        if (target is Unary { Op: UnOp.Deref } deref)
        {
            var address = Capture("nuint $name = ", $"(nuint)({Expr(deref.Operand)})");
            return $"{type}.__dotcc_zero_pointer({address}, {Rhs()})";
        }
        if (target is VarRef) return $"{type}.__dotcc_zero_value({Rhs()})";
        throw new IrUnsupportedException("zig zero operation has no proved value/place");
    }

    private string ZigZeroMemberAddress(Member member)
    {
        var owner = member.Arrow && member.Base.Type.Unqualified is CType.Pointer p ? p.Pointee : member.Base.Type;
        if (owner.Unqualified is not CType.Named named || !_aggregateDefinitions.TryGetValue(named.Name, out var definition))
            throw new IrUnsupportedException("zig zero address requires an owned source aggregate");
        // The registration adapter binds the finite native source-shape proof. CLR sizeof never supplies it.
        var fields = definition.Fields;
        if (definition.ZigZeroOwner is not { } ownerProof || ownerProof.Profile != ZigStorageLayout.AcceptedProfile
            || ownerProof.SourceMembers[1].Name != member.Field)
            throw new IrUnsupportedException("zig zero address native owner offset is unproved (auto-layout/reordering remains unsupported)");
        var layout = _offsetModel.Aggregate(named.Name);
        if (layout.Size != 16 || layout.Alignment != 8 || layout.Offsets[fields[1].Name] != 8 || layout.Offsets[fields[2].Name] != 8)
            throw new IrUnsupportedException("zig zero address owner differs from accepted native shape proof");
        if (!member.Arrow && !member.Base.IsLValue)
            throw new IrUnsupportedException("zig zero address lifetime requires stable owner storage");
        return ZeroSizedMemberAddress(member);
    }

    private bool TryRenderZigStorage(CExpr expression, out (string, int) result)
    {
        result = default;
        if (expression is Assign assignment)
        {
            if (ZigZero(assignment.Target.Type))
            {
                if (assignment.CompoundOp is not null) throw new IrUnsupportedException("zig zero compound store is unsupported");
                result = (ZigZeroOperation(assignment.Target, assignment.Value), PPrimary); return true;
            }
            if (TryZigPackedPlace(assignment.Target) is { } place)
            { result = (ZigPlaceAssignment(assignment, place), PPrimary); return true; }
        }
        if (expression is Member zero && ZigZero(zero.Type)
            || expression is Unary { Op: UnOp.Deref } && ZigZero(expression.Type))
        { result = (ZigZeroOperation(expression, null), PPrimary); return true; }
        if (expression is Unary { Op: UnOp.AddrOf, Operand: Member member } && ZigZero(member.Type))
        { result = (ZigZeroMemberAddress(member), PPrimary); return true; }
        if (expression is Binary { Op: BinOp.Eq or BinOp.Ne } comparison && ZigStorage(comparison.Left.Type) is { Kind: ZigStorageKind.PackedInteger } proof)
        {
            var wide = proof.LogicalBits > 64;
            result = ($"((CBool)(({ZigBits(Expr(comparison.Left), comparison.Left.Type, wide)} & {ZigMask(proof.LogicalBits, wide)}) {BinSym(comparison.Op)} ({ZigBits(Expr(comparison.Right), comparison.Right.Type, wide)} & {ZigMask(proof.LogicalBits, wide)})))", PPrimary);
            return true;
        }
        return false;
    }

    private string ZigStorageStructText(StructTypeDef type, ZigStorageLayout proof)
    {
        proof.RequireSupported();
        var sb = new StringBuilder().Append(ZigStorageObjectMetadata.Marker(proof)).Append('\n')
            .Append("// storage profile: ").Append(proof.Profile).Append('\n')
            .Append("[global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Sequential)]\n")
            .Append(_publicTypes ? "public unsafe partial struct " : "unsafe partial struct ").Append(Cs(proof.SourceCType)).Append("\n{\n");
        if (proof.Kind == ZigStorageKind.Zero)
        {
            var ts = Cs(proof.SourceCType);
            foreach (var member in proof.SourceMembers)
                sb.Append("    public ").Append(Cs(member.SourceType)).Append(' ').Append(member.TargetName)
                    .Append(" { get => default; set { } }\n");
            sb.Append("    // CLR token only; no inline payload or zero-stride array representation.\n")
                .Append($"    internal static {ts} __dotcc_zero_pointer(nuint address, {ts} value) => default;\n")
                .Append($"    internal static {ts} __dotcc_zero_owner<T>(ref T owner, {ts} value) where T : unmanaged => default;\n")
                .Append($"    internal static {ts} __dotcc_zero_value_owner<T>(T owner, {ts} value) where T : unmanaged => default;\n")
                .Append($"    internal static {ts} __dotcc_zero_value({ts} value) => default;\n");
        }
        else
        {
            // Registry annotates reserved source names; never Id() an annotated target again.
            if (type.Fields.Any(f => FieldTarget(f).TrimStart('@') == ZigCarrier))
                throw new IrUnsupportedException("Zig synthetic carrier target collides with a source member");
            sb.Append("    internal ").Append(ZigCarrierType(proof)).Append(' ').Append(ZigCarrier).Append(";\n");
            foreach (var f in proof.SourceMembers)
            {
                var field = type.Fields.Single(s => s.Name == f.Name);
                sb.Append("    public ").Append(Cs(f.SourceType)).Append(' ').Append(FieldTarget(field)).Append("\n    {\n")
                    .Append("        get => ").Append(ZigExtract("(" + ZigWord(proof) + ")" + ZigCarrier, f.SourceType, f.BitOffset, f.LogicalBits, f.Signed, proof.LogicalBits > 64)).Append(";\n")
                    .Append("        set => ").Append(ZigCarrier).Append(" = ").Append(ZigPatch(proof, ZigCarrier, "value", f.SourceType, f.BitOffset, f.LogicalBits)).Append(";\n    }\n");
            }
        }
        return sb.Append("}\n\n").ToString();
    }
}
