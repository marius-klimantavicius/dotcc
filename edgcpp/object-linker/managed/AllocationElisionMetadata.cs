namespace DotObj;

internal sealed record AllocationElision(long SourceAllocationObligationId, long FunctionEntityId,
    string Kind, string CaptureProof);

internal sealed partial class ObjectFile
{
    public Dictionary<(long SourceAllocationObligationId, long FunctionEntityId), AllocationElision> AllocationElisions { get; } = [];

    private void ReadAllocationElisions()
    {
        using var query = database.Prepare("SELECT source_allocation_obligation_id,function_entity_id,kind,capture_proof FROM allocation_elisions ORDER BY source_allocation_obligation_id,function_entity_id");
        while (query.Read())
        {
            var value = new AllocationElision(query.Integer(0), query.Integer(1), query.Text(2), query.Text(3));
            if (value.Kind != "empty-initialization" || value.CaptureProof != "observed-empty-allocation-initialization" ||
                !SourceAllocationObligations.TryGetValue(value.SourceAllocationObligationId, out var original) ||
                !Functions.ContainsKey(value.FunctionEntityId) ||
                !IsSourceLifetimeExecution(SourceLifetimes[original.LifetimeId], value.FunctionEntityId) ||
                !AllocationElisions.TryAdd((value.SourceAllocationObligationId, value.FunctionEntityId), value))
                throw new InvalidDataException("Empty allocation initialization lacks its original allocation and exact execution owner");
            if (AllocationCleanupActions.Values.Any(action => action.SourceAllocationObligationId == value.SourceAllocationObligationId &&
                    action.FunctionEntityId == value.FunctionEntityId))
                throw new InvalidDataException("Empty allocation initialization also has a cleanup action");
        }
        RejectRows("""
            SELECT e.source_allocation_obligation_id FROM allocation_elisions e
            JOIN internal_try_expressions t ON t.source_allocation_obligation_id=e.source_allocation_obligation_id
            JOIN nodes n ON n.id=t.node_id WHERE n.function_id=e.function_entity_id
            """, "Empty allocation initialization also has an internal rollback boundary");
    }
}
