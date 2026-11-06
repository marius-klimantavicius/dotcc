#nullable enable
namespace DotCC.Backends;
using DotCC.Ir;
internal sealed partial class CSharpBackend
{
    private (string, int) RenderLogicalZigBitCast(BitCast bitCast, ZigBitcastWidthLayout layout)
    {
        var from = layout.Source;
        var to = layout.Target;
        if (from.LogicalBits is < 1 or > 128 || from.LogicalBits != to.LogicalBits || to.Representation == ZigBitcastRepresentation.Array)
            throw new IrUnsupportedException("zig @bitCast has an invalid logical layout witness");
        var signed = to.Signed ? "true" : "false";
        var target = Cs(bitCast.Target.Unqualified);
        if (from.Representation == ZigBitcastRepresentation.Array)
            return ($"ZigBitcastWidth.Array<{target}>((void*)({Expr(bitCast.Operand)}), {from.ElementCount}, {from.ElementLogicalBits}, {from.ElementStride}, {to.LogicalBits}, {signed})", PPrimary);
        var source = bitCast.Operand.Type.Unqualified;
        return ($"ZigBitcastWidth.Scalar<{Cs(source)}, {target}>({Coerced(bitCast.Operand, source)}, {to.LogicalBits}, {signed})", PPrimary);
    }
}
