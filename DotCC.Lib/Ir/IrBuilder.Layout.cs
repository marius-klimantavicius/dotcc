using System;
using System.Globalization;
using DotCC.Layout;

namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
    private readonly System.Collections.Generic.Dictionary<string, int> _structPacks = new(StringComparer.Ordinal);
    private readonly System.Collections.Generic.Dictionary<string, int> _structAlignments = new(StringComparer.Ordinal);
    private readonly System.Collections.Generic.Dictionary<string, OffsetRequest> _constantOffsetRequests = new(StringComparer.Ordinal);
    internal static string LayoutType(CType type) => type.Unqualified switch
    {
        CType.Prim { Name: "u0", Bytes: 0, Integer: true, Signed: false } => "a:0:p:1:1",
        CType.Prim primitive => $"p:{primitive.Bytes}:{(type.AlignmentOverride != 0 ? type.AlignmentOverride : primitive.Bytes)}",
        CType.LongDoubleStorage16 => $"p:16:{(type.AlignmentOverride != 0 ? type.AlignmentOverride : 16)}",
        CType.Slice => "p:16:8",
        // Numeric 128-byte Zig vectors use the existing native CPU feature
        // policy for their ABI alignment (16/32/64 on x86_64). The bounded
        // value carrier keeps all bytes; aligned locals/arrays supply the address.
        CType.Vector { IsMask: false, UsesStorage: true, SizeOf: 128 } =>
            $"p:128:{(type.AlignmentOverride != 0 ? type.AlignmentOverride : Frontends.ZigSyntheticModules.HostStorageVectorAlignment)}",
        CType.Tuple tuple when tuple.Elements.Any(element => element.Unqualified is CType.Array)
            => "unsupported:pointer-backed-tuple-array",
        CType.Tuple { Elements.Count: 2 } tuple => "t:" + string.Join(",", tuple.Elements.Select(element =>
            Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(LayoutType(element))))),
        CType.VoidType => "a:0:p:1:1",
        CType.ErrorSetType => "p:2:2",
        CType.Func { IsFunctionType: true } => "unsupported:function-type",
        CType.Pointer or CType.Func => $"p:8:{(type.AlignmentOverride != 0 ? type.AlignmentOverride : 8)}",
        CType.Enum enumeration => LayoutType(type.AlignmentOverride != 0
            ? enumeration.Underlying with { AlignmentOverride = type.AlignmentOverride } : enumeration.Underlying),
        CType.Array array => "a:" + array.StorageCount.ToString(CultureInfo.InvariantCulture) + ":" + LayoutType(array.Element),
        CType.Optional { Inner.Unqualified: CType.Optional } => "unsupported:nested-managed-optional",
        CType.Optional { Inner.Unqualified: CType.VoidType or CType.Named { Name: "Unit" } } => "p:1:1",
        CType.Optional { Inner.Unqualified: CType.ErrorSetType } => "p:2:2",
        CType.Optional optional => "o:" + LayoutType(optional.Inner),
        CType.ErrorUnion error => "e:" + LayoutType(error.Payload),
        CType.Named { IsExternal: true, ExternalLayout: { } layout } => $"p:{layout.Size}:{layout.Alignment}",
        CType.Named { IsExternal: true } named => throw new IrUnsupportedException("external type '" + named.Name + "' requires layout metadata (size and alignment) for this operation"),
        CType.Opaque opaque => throw new IrUnsupportedException("opaque type '" + opaque.Name + "' has no value or layout"),
        CType.Named named => "n:" + named.Name,
        // CType's record formatter prints computed self-referential properties
        // (Unqualified/FlatElement), so diagnostic serialization must never call
        // its ToString. Unrequested foreign layouts remain opaque descriptors;
        // requesting one still produces the strict unsupported-layout diagnostic.
        _ => "unsupported:" + type.Unqualified.GetType().Name,
    };

    internal readonly System.Collections.Generic.Dictionary<string, ZigStorageLayout> ZigStorageLayouts = new(StringComparer.Ordinal);

    internal void SetZigStorageLayout(string name, ZigStorageLayout storage)
    {
        if (ZigStorageLayouts.TryGetValue(name, out var previous)
            && ZigStorageObjectMetadata.Contract(previous) != ZigStorageObjectMetadata.Contract(storage))
            throw new IrUnsupportedException("conflicting Zig storage proof for '" + name + "'");
        ZigStorageLayouts[name] = storage;
        var index = Types.FindIndex(t => t.Name == name);
        if (index < 0) throw new IrUnsupportedException("Zig storage registration without source aggregate: " + name);
        Types[index] = Types[index] with { ZigStorage = storage };
    }

    internal readonly System.Collections.Generic.Dictionary<string, ZigZeroOwnerLayout> ZigZeroOwners = new(StringComparer.Ordinal);
    internal void SetZigZeroOwnerLayout(string name, ZigZeroOwnerLayout proof)
    {
        ZigZeroOwners[name] = proof;
        var index = Types.FindIndex(t => t.Name == name);
        Types[index] = Types[index] with { ZigZeroOwner = proof };
    }

    private LayoutAggregate DescribeAggregate(string name)
    {
        if (_externalTypes.ContainsKey(name))
            throw new OffsetLayoutException("external type '" + name + "' has no C member layout; provide a C definition for member offsets");
        if (!_structFields.TryGetValue(name, out var fields))
            throw new OffsetLayoutException("Unknown or incomplete aggregate: " + name);
        var aggregate = new LayoutAggregate
        {
            Name = name, Union = _structIsUnion.GetValueOrDefault(name), Packed = _packedStructs.Contains(name), Alignment = _structAlignments.GetValueOrDefault(name), Pack = _structPacks.GetValueOrDefault(name),
        };
        if (ZigStorageLayouts.TryGetValue(name, out var storage))
        {
            aggregate.ZigStorage = ZigStorageObjectMetadata.Contract(storage);
            aggregate.ZigSourceIdentity = storage.SourceIdentity;
        }
        if (ZigZeroOwners.TryGetValue(name, out var owner))
        {
            aggregate.ZigZeroOwner = ZigStorageObjectMetadata.ZeroOwnerContract(owner);
            aggregate.ZigSourceIdentity = owner.SourceCType.Name;
        }
        foreach (var field in fields)
            // Zig void fields have no addressable storage and offset zero,
            // unlike a GNU C zero-length array at the current cursor. They may
            // still impose an explicitly declared alignment on the owner.
            aggregate.Fields.Add(new LayoutField { Name = field.Name,
                Type = field.Type.Unqualified is CType.VoidType ? "z:void" : LayoutType(field.Type),
                BitWidth = field.BitWidth, Alignment = field.Alignment });
        return aggregate;
    }

    internal OffsetDocument CreateOffsetDocument()
    {
        var document = new OffsetDocument();
        foreach (var name in _structFields.Keys) document.Aggregates.Add(name, DescribeAggregate(name));
        document.Requests.AddRange(_constantOffsetRequests.Values);
        return document;
    }

    private OffsetLayoutModel CreateLayoutModel() => new(DescribeAggregate);

    private (int Size, int Align) Layout(CType type)
    {
        try
        {
            var layout = CreateLayoutModel().Type(LayoutType(type));
            return (layout.Size, layout.Alignment);
        }
        catch (OffsetLayoutException error) { throw new IrUnsupportedException("layout of '" + type.Describe() + "': " + error.Message); }
    }

    private int? OffsetOfConstPath(string structName, System.Collections.Generic.IReadOnlyList<string> path)
    {
        try
        {
            var offset = CreateLayoutModel().Offset(structName, path);
            var name = OffsetDocument.RequestName(structName, path);
            _constantOffsetRequests[name] = new OffsetRequest
            {
                Name = name, Aggregate = structName, Path = System.Linq.Enumerable.ToArray(path), Expected = offset,
            };
            return offset;
        }
        catch (OffsetLayoutException error) { throw new IrUnsupportedException(error.Message); }
    }
}
