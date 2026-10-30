#nullable enable
namespace DotCC.Ir;
public enum ZigBitcastRepresentation { Integer, Float, Packed, Array }
/// <summary>Native logical layout at a Zig bitcast boundary; CType still describes storage.</summary>
public sealed record ZigBitcastValueLayout(int LogicalBits, bool Signed, ZigBitcastRepresentation Representation,
    int ElementCount = 0, int ElementLogicalBits = 0, int ElementStride = 0);
public sealed record ZigBitcastWidthLayout(ZigBitcastValueLayout Source, ZigBitcastValueLayout Target);
