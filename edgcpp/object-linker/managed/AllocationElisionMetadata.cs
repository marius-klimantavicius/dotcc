namespace DotObj;

internal sealed record AllocationElision(long SourceAllocationObligationId, long FunctionEntityId,
    string Kind, string CaptureProof);

internal sealed partial class ObjectFile
{
    public Dictionary<(long SourceAllocationObligationId, long FunctionEntityId), AllocationElision> AllocationElisions { get; } = [];

    private void ReadAllocationElisions()
    {
        ReadSourceUninitializedAllocations();
        using var query = database.Prepare("SELECT source_allocation_obligation_id,function_entity_id,kind,capture_proof FROM allocation_elisions ORDER BY source_allocation_obligation_id,function_entity_id");
        while (query.Read())
        {
            var value = new AllocationElision(query.Integer(0), query.Integer(1), query.Text(2), query.Text(3));
            bool disposition = value.Kind switch
            {
                "empty-initialization" => value.CaptureProof == "observed-empty-allocation-initialization" &&
                    !SourceUninitializedAllocations.ContainsKey(value.SourceAllocationObligationId),
                "no-initialization" => value.CaptureProof == "original-new-without-initialization" &&
                    SourceUninitializedAllocations.ContainsKey(value.SourceAllocationObligationId),
                "dynamic-array-zero-initialization" => value.CaptureProof == "observed-dynamic-array-zero-initialization" &&
                    !SourceUninitializedAllocations.ContainsKey(value.SourceAllocationObligationId) &&
                    SourceAllocationObligations.TryGetValue(value.SourceAllocationObligationId, out var zeroAllocation) &&
                    zeroAllocation.IsArray && Original(SourceTypeById(zeroAllocation.AllocatedSourceTypeId)).Kind == "array",
                _ => false
            };
            if (!disposition ||
                !SourceAllocationObligations.TryGetValue(value.SourceAllocationObligationId, out var original) ||
                !Functions.ContainsKey(value.FunctionEntityId) ||
                !IsSourceLifetimeExecution(SourceLifetimes[original.LifetimeId], value.FunctionEntityId) ||
                !AllocationElisions.TryAdd((value.SourceAllocationObligationId, value.FunctionEntityId), value))
                throw new InvalidDataException("Allocation initialization elision lacks its original allocation and exact execution owner");
            if (AllocationCleanupActions.Values.Any(action => action.SourceAllocationObligationId == value.SourceAllocationObligationId &&
                    action.FunctionEntityId == value.FunctionEntityId))
                throw new InvalidDataException("Allocation initialization elision also has a cleanup action");
        }
        if (SourceUninitializedAllocations.Keys.Except(AllocationElisions.Values
                .Where(value => value.Kind == "no-initialization").Select(value => value.SourceAllocationObligationId)).Any())
            throw new InvalidDataException("Original new without initialization lacks its exact no-rollback disposition");
        RejectRows("""
            SELECT e.source_allocation_obligation_id FROM allocation_elisions e
            JOIN array_allocation_operations a ON a.source_allocation_obligation_id=e.source_allocation_obligation_id
            JOIN nodes n ON n.id=a.node_id
            WHERE e.kind='dynamic-array-zero-initialization' AND n.function_id=e.function_entity_id
            """, "Array zero initialization also has a runtime allocation rollback protocol");
        RejectRows("""
            SELECT e.source_allocation_obligation_id FROM allocation_elisions e
            JOIN internal_try_expressions t ON t.source_allocation_obligation_id=e.source_allocation_obligation_id
            JOIN nodes n ON n.id=t.node_id WHERE n.function_id=e.function_entity_id
            """, "Allocation initialization elision also has an internal rollback boundary");
    }
}
