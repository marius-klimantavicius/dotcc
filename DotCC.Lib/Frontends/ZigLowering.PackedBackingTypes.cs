#nullable enable
using System.Globalization;
using DotCC.Ir;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    private TypeSeed? ReflectedPackedBackingType(ZigTypeInfo info)
    {
        if (info.Type.Unqualified is not CType.Named named || !_ir.PackedStructs.Contains(named.Name)) return null;
        if (!PackedBitSizeEvidence.GetOrCreateValue(_ir).Packed.TryGetValue(named.Name, out var source))
            throw new IrUnsupportedException("zig: packed backing integer has no source declaration proof");
        using var scope = source.Owner.EnterPackedSource(named.Name, source);
        // A packed union overlays equal-width fields; it does not sum them.
        // Existing source proof checks every variant and any explicit backing.
        var unionBits = info.Tag == "union" ? source.Owner.PackedBitSizes(named.Name) : null;
        if (info.Tag == "union" && unionBits is null)
            throw new IrUnsupportedException("zig: packed union backing integer has no consistent source width proof");
        CType type;
        int? bits;
        if (source.Backing is { } declared)
        {
            type = source.Owner.LowerType(declared);
            bits = source.Owner.DeclaredBitsOfTypeArg(declared);
        }
        else
        {
            bits = unionBits ?? source.Owner.PackedBitSizes(named.Name);
            if (bits is null) throw new IrUnsupportedException("zig: packed backing integer has no source width proof");
            type = LowerPrim("u" + bits.Value.ToString(CultureInfo.InvariantCulture));
        }
        return new TypeSeed("", type, bits);
    }
}
