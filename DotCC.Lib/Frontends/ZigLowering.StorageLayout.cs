using System;
using System.Collections.Generic;
using System.Linq;
using DotCC.Ir;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    private readonly HashSet<string> _storagePackedUnionSources = new(StringComparer.Ordinal);
    // Syntax only: canonical pass-0 owners and their parsed layouts, including imported/nested declarations.
    private void CaptureStorageUnionSources(IEnumerable<(string? Name, object? Content, string? Parent)> declarations)
    {
        foreach (var (name, content, _) in declarations)
            if (name is not null && content is Zig.UnionDecl { Arg3.Content: Zig.UnionHead { Arg0.Content: Zig.LayoutPacked } })
                _storagePackedUnionSources.Add(name);
    }

    private void CaptureStorageUnionSource(string canonicalOwner, LALR.CC.LexicalGrammar.Item head,
        LALR.CC.LexicalGrammar.Item? variants = null)
    {
        if (head.Content is Zig.UnionHead { Arg0.Content: Zig.LayoutPacked })
        {
            _storagePackedUnionSources.Add(canonicalOwner);
            if (variants is not null)
                PackedBitSizeEvidence.GetOrCreateValue(_ir).Packed[canonicalOwner] = new(this,
                    head.Content is Zig.UnionHead { Arg2.Content: Zig.UnionArgTag backing } ? backing.Arg1 : null,
                    CaptureInlineEnvironment(variants));
        }
    }

    // Adapter only: the R44 owner/AST evidence and R38 logical transport stay immutable.
    private void RegisterZigStorage(string name, IReadOnlyList<StructField> fields, bool isUnion, AggregateLayout layout)
    {
        ZigStorageLayout? storage = null;
        string origin = "zig-registration:" + (_moduleName ?? _importerDir ?? "memory") + ":" + name;
        ZigStorageLayout Unsupported(string reason) => new(ZigStorageKind.Unsupported, new CType.Named(name), name,
            origin, "unproved", false, 0, 0, 0, ZigStorageLayout.AcceptedProfile, [], reason);
        if (!isUnion && fields.Count == 0)
        {
            if (layout == AggregateLayout.Packed && PackedBitSizeEvidence.TryGetValue(_ir, out var evidence)
                && evidence.Packed.TryGetValue(name, out var source) && source.Backing is not null)
                storage = Unsupported("explicit backing on an empty packed declaration is outside the recorded zero proof");
            else storage = new(ZigStorageKind.Zero, new CType.Named(name), name, origin, "zero", false,
                0, 0, 1, ZigStorageLayout.AcceptedProfile, []);
        }
        else if (!isUnion && layout != AggregateLayout.Packed
            && fields.All(f => f.Type.Unqualified is CType.Named n
                && _ir.ZigStorageLayouts.TryGetValue(n.Name, out var child) && child.Kind == ZigStorageKind.Zero)
            && fields.All(f => f.Alignment == 0 && f.Type.Quals == TypeQual.None && f.Type.AlignmentOverride == 0
                && (!_shared.StructFieldAttrs.TryGetValue((name, f.Name), out var attrs) || attrs.Item1 is null)))
        {
            var members = fields.Select(f => new ZigStorageField(f.Name, f.Type, f.TargetName ?? EmitHelpers.Id(f.Name),
                0, 0, false, _shared.StructFieldAttrs.TryGetValue((name, f.Name), out var attr) && attr.Item2, false)).ToArray();
            storage = new(ZigStorageKind.Zero, new CType.Named(name), name, origin, "zero", false,
                0, 0, 1, ZigStorageLayout.AcceptedProfile, members);
        }
        else if (!isUnion && layout != AggregateLayout.Packed
            && (fields.Any(f => f.Type.Unqualified is CType.Array { Count: 0 })
                || fields.All(f => f.Type.Unqualified is CType.VoidType
                    || f.Type.Unqualified is CType.Named n && _ir.ZigStorageLayouts.TryGetValue(n.Name, out var z) && z.Kind == ZigStorageKind.Zero)))
            storage = Unsupported("nonempty zero aggregate/zero-length field requires an additional native source layout proof");
        else if (layout == AggregateLayout.Packed)
        {
            try { storage = ProvePackedStorage(name, fields, new HashSet<string>(StringComparer.Ordinal), 0); }
            catch (IrUnsupportedException error) { storage = Unsupported(error.Message); }
        }
        if (storage is null)
        {
            if (!isUnion && layout != AggregateLayout.Packed && fields.Count == 3
                && fields[0].Type.Unqualified is CType.Prim { Integer: true, Signed: false, Bytes: 8 }
                && BitcastFieldLocation(name, fields[0].Name, fields[0].Type).IntegerBits == 64
                && fields[1].Type.Unqualified is CType.Named zero
                && _ir.ZigStorageLayouts.TryGetValue(zero.Name, out var zeroProof) && zeroProof.Kind == ZigStorageKind.Zero
                && fields[2].Type.Unqualified is CType.Prim { Integer: true, Signed: false, Bytes: 1 }
                && BitcastFieldLocation(name, fields[2].Name, fields[2].Type).IntegerBits == 8
                && fields.All(f => f.Type.Quals == TypeQual.None && f.Alignment == 0 && f.Type.AlignmentOverride == 0
                    && (!_shared.StructFieldAttrs.TryGetValue((name, f.Name), out var attrs) || attrs.Item1 is null)))
                _ir.SetZigZeroOwnerLayout(name, new(new CType.Named(name), fields.ToArray(), ZigStorageLayout.AcceptedProfile));
            return;
        }
        _ir.SetZigStorageLayout(name, storage);
        if (storage.Kind == ZigStorageKind.Unsupported) _ir.PrunableTypes.Add(name);
        if (storage.Kind == ZigStorageKind.PackedInteger)
            _shared.BitcastWidths.PackedBits[name] = storage.LogicalBits;
    }

    private ZigStorageLayout ProvePackedStorage(string name, IReadOnlyList<StructField> fields,
        HashSet<string> active, int depth)
    {
        IrUnsupportedException Cut(string detail) => new("zig packed storage '" + name + "': " + detail);
        if (depth >= 32 || fields.Count > 4096 || !active.Add(name)) throw Cut("cyclic or excessive source layout");
        try
        {
            var isUnion = _ir.StructIsUnion.GetValueOrDefault(name);
            if (isUnion && !_storagePackedUnionSources.Contains(name)) throw Cut("union has no packed source syntax");
            if (!PackedBitSizeEvidence.TryGetValue(_ir, out var evidence) || !evidence.Packed.TryGetValue(name, out var source))
                throw Cut("missing R44 owner/AST packed proof (local/reified capture is unsupported)");
            using var environment = source.Owner.EnterPackedSource(name, source);
            var members = new List<ZigStorageField>();
            var cursor = 0;
            foreach (var field in fields)
            {
                if (field.IsAnonBitField || field.Type.Quals != TypeQual.None || field.Alignment != 0 || field.Type.AlignmentOverride != 0
                    || _shared.StructFieldAttrs.TryGetValue((name, field.Name), out var attrs) && attrs.Item1 is not null)
                    throw Cut("custom alignment or anonymous field is unsupported: " + field.Name);
                int bits;
                bool signed;
                switch (field.Type.Unqualified)
                {
                    case CType.Prim { Name: "_Bool" }: bits = 1; signed = false; break;
                    case CType.Prim { Integer: true, IsComptimeInt: false } primitive:
                        bits = source.Owner.BitcastFieldLocation(name, field.Name, field.Type).IntegerBits
                            ?? throw Cut("missing source integer width: " + field.Name);
                        signed = primitive.Signed; break;
                    case CType.Enum enumeration:
                        bits = source.Owner.PackedEnumBitSize(enumeration)
                            ?? throw Cut("enum requires an explicit source tag width: " + field.Name);
                        if (bits > 64) throw Cut("enum logical width above 64 is unsupported: " + field.Name);
                        signed = enumeration.Underlying.Unqualified is CType.Prim { Signed: true }; break;
                    case CType.Named nested:
                        source.Owner.EnsureNestedBody(nested.Name);
                        if ((!_ir.PackedStructs.Contains(nested.Name) && !_storagePackedUnionSources.Contains(nested.Name))
                            || _ir.StructFieldsOf(nested.Name) is not { } children)
                            throw Cut("nested field has no packed source proof: " + field.Name);
                        var child = ProvePackedStorage(nested.Name, children, active, depth + 1);
                        bits = child.LogicalBits; signed = false; break;
                    default: throw Cut("unsupported field kind (pointer/float/array/union): " + field.Name);
                }
                if (bits is < 1 or > 128) throw Cut("field logical width is outside 1..128: " + field.Name);
                if (isUnion && cursor != 0 && cursor != bits) throw Cut("packed union variants have different logical widths");
                members.Add(new(field.Name, field.Type, field.TargetName ?? EmitHelpers.Id(field.Name), isUnion ? 0 : cursor, bits, signed,
                    _shared.StructFieldAttrs.TryGetValue((name, field.Name), out var attr) && attr.Item2,
                    _shared.StructFieldSentinels.ContainsKey((name, field.Name))));
                cursor = isUnion ? bits : checked(cursor + bits);
                if (cursor > 128) throw Cut("total logical width is outside 1..128");
            }
            if (cursor < 1) throw Cut("source widths do not establish the complete logical layout");
            if (source.Backing is { Content: Zig.CallArgs }) throw Cut("unrecorded computed backing proof is unsupported");
            if (source.Owner.PackedBitSizes(name) != cursor) throw Cut("source widths do not establish the complete logical layout");
            bool backingSigned = false;
            var backingKind = isUnion ? "packed-union" : "inferred-unsignedness-unspecified";
            if (source.Backing is { } ast)
            {
                if (source.Owner.LowerType(ast).Unqualified is not CType.Prim { Integer: true } backing
                    || source.Owner.DeclaredBitsOfTypeArg(ast) != cursor) throw Cut("explicit backing width mismatch");
                backingSigned = backing.Signed;
                backingKind = (isUnion ? "packed-union-" : "") + "explicit-" + (backingSigned ? "i" : "u") + cursor;
            }
            // Finite pinned Linux x86_64 proof, including u65..u128 and original Terminal.flags.
            var (size, alignment) = cursor switch { <= 8 => (1, 1), <= 16 => (2, 2), <= 32 => (4, 4), <= 64 => (8, 8), _ => (16, 16) };
            return new(ZigStorageKind.PackedInteger, new CType.Named(name), name,
                "r44-owner-ast:" + (source.Owner._moduleName ?? source.Owner._importerDir ?? "memory") + ":" + name,
                backingKind, backingSigned, cursor, size, alignment, ZigStorageLayout.AcceptedProfile, members);
        }
        finally { active.Remove(name); }
    }
}
