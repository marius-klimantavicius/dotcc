#nullable enable
using DotCC.Ir;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    // Only an existing source operand supplies this proof. Reference keys
    // retain that exact type through aliases; equal CLR carriers cannot supply
    // or share a source element width.
    private void RememberTypeOperandLocation(CType type, ZigResultLocation location)
    {
        if (!UsesSourceStdlib || type.Unqualified is not
            (CType.Array or CType.Slice or CType.Pointer or CType.Optional or CType.ErrorUnion or CType.Vector)
            || !HasTypeOperandProof(location)) return;
        location = location with { Physical = type };
        if (_shared.BitcastWidths.TypeOperands.TryGetValue(type, out var previous))
            location = MergeTypeOperandProof(previous, location);
        _shared.BitcastWidths.TypeOperands[type] = location;
        // Source optional/error payloads retain their actual type object and
        // the same element proof when captured or reflected as .child.
        if (type.Unqualified is CType.Optional optional)
            RememberTypeOperandLocation(optional.Inner, location);
        else if (type.Unqualified is CType.ErrorUnion error)
            RememberTypeOperandLocation(error.Payload, location);
        if (location.Element is { Physical: { } elementType } element)
            RememberTypeOperandLocation(elementType, element);
    }

    private static bool HasTypeOperandProof(ZigResultLocation location)
        => location.IntegerBits is not null || location.Sentinel is not null
            || location.Element is { } element && HasTypeOperandProof(element);

    private static ZigResultLocation MergeTypeOperandProof(ZigResultLocation previous, ZigResultLocation next)
    {
        if (previous.IntegerBits is { } before && next.IntegerBits is { } after && before != after)
            throw new IrUnsupportedException("zig: conflicting witnessed composite source integer widths");
        if (previous.Sentinel is { } beforeSentinel && next.Sentinel is { } afterSentinel
            && beforeSentinel != afterSentinel)
            throw new IrUnsupportedException("zig: conflicting witnessed composite source sentinels");
        var element = previous.Element is { } beforeElement && next.Element is { } afterElement
            ? MergeTypeOperandProof(beforeElement, afterElement) : previous.Element ?? next.Element;
        return previous with { IntegerBits = previous.IntegerBits ?? next.IntegerBits,
            Sentinel = previous.Sentinel ?? next.Sentinel, Element = element };
    }
}
