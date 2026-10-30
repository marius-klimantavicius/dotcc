#nullable enable

namespace DotCC.Backends;

using DotCC.Ir;

internal sealed partial class CSharpBackend
{
    private string TupleElementValue(CExpr value, CType type)
    {
        var text = Coerced(value, type);
        return CSharpTarget.TuplePointerSlot(type) ? $"(nint)({text})" : text;
    }

    private (string, int) RenderZigBitCast(BitCast bitCast)
    {
        if (bitCast.ZigLayout is { } logical) return RenderLogicalZigBitCast(bitCast, logical);
        var source = bitCast.Operand.Type.Unqualified;
        var target = bitCast.Target.Unqualified;
        // Zig forbids pointer/slice bitcasts. An array VALUE has pointer-backed
        // storage here, but its source size and bytes still belong to the array.
        if (source is CType.Pointer or CType.Func or CType.Slice
            || target is CType.Pointer or CType.Func or CType.Slice or CType.Array)
            throw new IrUnsupportedException("zig @bitCast requires supported value types, not pointers, slices, or array destinations");

        var fromSize = BitCastValueSize(source);
        var toSize = BitCastValueSize(target);
        if (fromSize is null || toSize is null || fromSize <= 0 || fromSize != toSize)
            throw new IrUnsupportedException("zig @bitCast requires equal, known, nonzero source and target sizes");

        if (source is CType.Array)
        {
            // Read once through the actual data pointer, including unaligned
            // slice offsets. No pointer-to-integer reinterpret or descriptor
            // read, allocation, write, or change to the storage's lifetime.
            return ($"global::System.Runtime.CompilerServices.Unsafe.ReadUnaligned<{Cs(target)}>((void*)({Expr(bitCast.Operand)}))", PPrimary);
        }
        return ($"global::System.Runtime.CompilerServices.Unsafe.BitCast<{Cs(source)}, {Cs(target)}>({Sub(bitCast.Operand, PAssign)})", PPrimary);
    }

    private long? BitCastValueSize(CType type)
        => (_foldConstant(new SizeOfExpr(type) { Type = CType.SizeT }) as LitInt)?.Value;
}
