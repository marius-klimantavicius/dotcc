using System;
using System.Linq;
using DotCC.Ir;

namespace DotCC.Layout;

internal static class ZigStorageLayoutModel
{
    internal static LayoutInfo DescribeZeroOwner(LayoutAggregate aggregate)
    {
        var proof = ZigStorageObjectMetadata.ReadContract(aggregate.ZigZeroOwner!);
        if (!proof.IsZeroOwner || proof.Identity != (aggregate.ZigSourceIdentity ?? aggregate.Name) || proof.Profile != ZigStorageLayout.AcceptedProfile
            || !ZigStorageLayout.CompilerModelHostMatches || aggregate.Union || aggregate.Alignment != 0 || aggregate.Pack != 0
            || proof.Size != 16 || proof.Alignment != 8 || proof.Fields.Length != 3
            || !proof.Fields.Select(f => f.Name).SequenceEqual(aggregate.Fields.Select(f => f.Name), StringComparer.Ordinal)
            || !proof.Fields.Select(f => f.Offset).SequenceEqual(new[] { 0, 64, 64 })
            || !proof.Fields.Select(f => f.Width).SequenceEqual(new[] { 64, 0, 8 }))
            throw new OffsetLayoutException("invalid Zig zero-owner structural/native proof");
        var result = new LayoutInfo { Size = 16, Alignment = 8 };
        foreach (var field in proof.Fields) result.Offsets.Add(field.Name, field.Offset / 8);
        result.StorageOffsets.Add(0, 0); result.StorageOffsets.Add(2, 8);
        return result;
    }

    internal static LayoutInfo Describe(LayoutAggregate aggregate)
    {
        var proof = ZigStorageObjectMetadata.ReadContract(aggregate.ZigStorage!);
        if (proof.Identity != (aggregate.ZigSourceIdentity ?? aggregate.Name)) throw new OffsetLayoutException("Zig storage source identity mismatch");
        if (proof.Profile != ZigStorageLayout.AcceptedProfile || !ZigStorageLayout.CompilerModelHostMatches)
            throw new OffsetLayoutException("unsupported Zig storage target profile: " + proof.Profile);
        if (proof.Kind == ZigStorageKind.Unsupported) throw new OffsetLayoutException("zig storage '" + aggregate.Name + "': " + proof.Reason);
        var expected = proof.Bits switch { 0 => (0, 1), <= 8 => (1, 1), <= 16 => (2, 2), <= 32 => (4, 4), <= 64 => (8, 8), <= 128 => (16, 16), _ => (-1, -1) };
        if (proof.Bits < 0 || proof.Size != expected.Item1 || proof.Alignment != expected.Item2
            || (proof.Kind == ZigStorageKind.Zero) != (proof.Bits == 0)
            || aggregate.Union != proof.IsPackedUnion || aggregate.Alignment != 0 || aggregate.Pack != 0
            || proof.Fields.Length != aggregate.Fields.Count)
            throw new OffsetLayoutException("invalid Zig integer/zero storage proof: " + aggregate.Name);
        var result = new LayoutInfo { Size = proof.Size, Alignment = proof.Alignment };
        int cursor = 0;
        for (int i = 0; i < proof.Fields.Length; i++)
        {
            var field = proof.Fields[i];
            if (field.Name != aggregate.Fields[i].Name || field.Offset != (proof.IsPackedUnion ? 0 : cursor)
                || (proof.Kind == ZigStorageKind.Zero
                    ? field.Width != 0 || aggregate.Fields[i].Type != "n:" + ZigStorageObjectMetadata.NamedSource(field.SourceType)
                    : field.Width < 1))
                throw new OffsetLayoutException("invalid Zig source member map: " + aggregate.Name);
            result.Offsets.Add(field.Name, field.Offset / 8);
            result.ZigBitOffsets.Add(field.Name, field.Offset);
            cursor = proof.IsPackedUnion ? field.Width : checked(cursor + field.Width);
        }
        if (cursor != proof.Bits) throw new OffsetLayoutException("incomplete Zig logical bit map: " + aggregate.Name);
        return result;
    }
}
