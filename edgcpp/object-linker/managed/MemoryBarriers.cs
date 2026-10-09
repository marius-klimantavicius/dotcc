namespace DotObj;

internal sealed record MemoryBarrier(long NodeId, bool IsVolatile, bool HasVolatileKeyword,
    bool HasInlineKeyword, bool ClobbersMemory, string CaptureProof);
internal sealed record MemoryBarrierOperand(long NodeId, long Ordinal, string Direction,
    string ConstraintText, string SourceName, long SourceElementTypeId, long ElementSize,
    string SourceShape, string ExtentKind, long? FixedElementCount, long? ExtentBindingId,
    string CaptureProof, long? SourceFileId, long Line, long Column);
internal sealed record MemoryBarrierDimension(long NodeId, long OperandOrdinal, long Ordinal,
    string Kind, long? FixedCount, long? ExtentBindingId, long? SourceFileId, long Line, long Column);

internal sealed partial class ObjectFile
{
    public Dictionary<long, MemoryBarrier> MemoryBarriers { get; } = [];
    private readonly Dictionary<long, MemoryBarrierOperand[]> memoryBarrierOperands = [];
    private readonly Dictionary<(long Node, long Operand), MemoryBarrierDimension[]> memoryBarrierDimensions = [];
    public MemoryBarrierOperand[] BarrierOperands(long node) => memoryBarrierOperands.GetValueOrDefault(node, []);
    public MemoryBarrierDimension[] BarrierDimensions(long node, long operand) => memoryBarrierDimensions.GetValueOrDefault((node, operand), []);

    private void ReadMemoryBarrierMetadata()
    {
        using (var query = database.Prepare("SELECT b.node_id,b.is_volatile,b.has_volatile_keyword,b.has_inline_keyword,b.clobbers_memory,b.capture_proof,n.kind FROM memory_barriers b JOIN nodes n ON n.id=b.node_id ORDER BY b.node_id"))
            while (query.Read())
            {
                var barrier = new MemoryBarrier(query.Integer(0), query.Integer(1) == 1, query.Integer(2) == 1,
                    query.Integer(3) == 1, query.Integer(4) == 1, query.Text(5));
                if (barrier.NodeId <= 0 || query.Integer(1) is not (0 or 1) || query.Integer(2) is not (0 or 1) ||
                    query.Integer(3) is not (0 or 1) || query.Integer(4) is not (0 or 1) ||
                    barrier.CaptureProof != "observed-empty-gnu-asm" || query.Text(6) != "memory-barrier" ||
                    barrier.HasVolatileKeyword && !barrier.IsVolatile || !MemoryBarriers.TryAdd(barrier.NodeId, barrier))
                    throw new InvalidDataException("Invalid owned empty GNU assembly statement metadata");
            }
        var operands = new List<MemoryBarrierOperand>();
        using (var query = database.Prepare("SELECT node_id,ordinal,direction,constraint_text,source_name,source_element_type_id,element_size,source_shape,extent_kind,fixed_element_count,extent_binding_id,capture_proof,source_file_id,line,column FROM memory_barrier_operands ORDER BY node_id,ordinal"))
            while (query.Read())
            {
                var operand = new MemoryBarrierOperand(query.Integer(0), query.Integer(1), query.Text(2), query.Text(3),
                    query.Text(4), query.Integer(5), query.Integer(6), query.Text(7), query.Text(8), query.OptionalInteger(9),
                    query.OptionalInteger(10), query.Text(11), query.OptionalInteger(12), query.Integer(13), query.Integer(14));
                if (!MemoryBarriers.ContainsKey(operand.NodeId) || operand.Ordinal < 0 || operand.Line < 0 || operand.Column < 0 ||
                    !SourceTypes.TryGetValue(operand.SourceElementTypeId, out SourceType? element) ||
                    operand.ElementSize <= 0 || StorageSize(Type(element.ExecutionTypeId)) != operand.ElementSize ||
                    operand.SourceShape is not ("object" or "fixed-array" or "vla-array") ||
                    (operand.Direction, operand.ConstraintText) is not (("input", "m") or ("output", "=m") or ("inout", "+m")) ||
                    (operand.ExtentKind == "fixed" ? operand.FixedElementCount is null or < 0 || operand.ExtentBindingId is not null ||
                        operand.CaptureProof != "observed-asm-place" || operand.SourceShape == "vla-array"
                    : operand.ExtentKind != "vla-total" || operand.FixedElementCount is not null || operand.ExtentBindingId is null or <= 0 ||
                        operand.CaptureProof != "observed-asm-vla-total" || operand.SourceShape != "vla-array") ||
                    operand.SourceShape == "object" && operand.FixedElementCount != 1)
                    throw new InvalidDataException("Invalid memory assembly operand shape, constraint or extent proof");
                operands.Add(operand);
            }
        foreach (var group in operands.GroupBy(operand => operand.NodeId))
        {
            MemoryBarrierOperand[] ordered = group.ToArray();
            if (ordered.Where((operand, index) => operand.Ordinal != index).Any())
                throw new InvalidDataException("Memory assembly operand ordinals must be contiguous");
            memoryBarrierOperands.Add(group.Key, ordered);
        }
        var dimensions = new List<MemoryBarrierDimension>();
        using (var query = database.Prepare("SELECT node_id,operand_ordinal,ordinal,kind,fixed_count,extent_binding_id,source_file_id,line,column FROM memory_barrier_dimensions ORDER BY node_id,operand_ordinal,ordinal"))
            while (query.Read())
            {
                var dimension = new MemoryBarrierDimension(query.Integer(0), query.Integer(1), query.Integer(2), query.Text(3),
                    query.OptionalInteger(4), query.OptionalInteger(5), query.OptionalInteger(6), query.Integer(7), query.Integer(8));
                if (dimension.OperandOrdinal < 0 || dimension.OperandOrdinal >= BarrierOperands(dimension.NodeId).Length ||
                    dimension.Ordinal < 0 || dimension.Line < 0 || dimension.Column < 0 ||
                    (dimension.Kind == "fixed" ? dimension.FixedCount is null or < 0 || dimension.ExtentBindingId is not null
                        : dimension.Kind != "vla-total" || dimension.FixedCount is not null || dimension.ExtentBindingId is null or <= 0))
                    throw new InvalidDataException("Invalid original memory assembly array dimension");
                dimensions.Add(dimension);
            }
        foreach (var group in dimensions.GroupBy(dimension => (dimension.NodeId, dimension.OperandOrdinal)))
        {
            MemoryBarrierDimension[] ordered = group.ToArray();
            if (ordered.Where((dimension, index) => dimension.Ordinal != index).Any())
                throw new InvalidDataException("Memory assembly dimension ordinals must be contiguous");
            memoryBarrierDimensions.Add(group.Key, ordered);
        }
        foreach (MemoryBarrierOperand operand in operands)
        {
            MemoryBarrierDimension[] shape = BarrierDimensions(operand.NodeId, operand.Ordinal);
            if (operand.SourceShape == "object" ? shape.Length != 0
                : operand.SourceShape == "vla-array" ? shape.Length != 1 || shape[0].Kind != "vla-total" || shape[0].ExtentBindingId != operand.ExtentBindingId
                : shape.Length == 0 || shape.Any(dimension => dimension.Kind != "fixed"))
                throw new InvalidDataException("Memory assembly dimension metadata differs from its original operand shape");
            if (operand.SourceShape == "fixed-array")
            {
                var count = System.Numerics.BigInteger.One;
                foreach (MemoryBarrierDimension dimension in shape) count *= dimension.FixedCount!.Value;
                if (count != operand.FixedElementCount!.Value)
                    throw new InvalidDataException("Memory assembly fixed array count differs from its dimensions");
            }
        }
    }
}
