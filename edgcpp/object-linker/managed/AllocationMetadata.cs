namespace DotObj;

internal sealed record SourceAllocationObligation(long Id, long FunctionEntityId, long LifetimeId,
    long Ordinal, long DeallocatorEntityId, long AllocatedSourceTypeId, bool IsArray,
    long? SourceFileId, long Line, long Column, string CaptureProof);

internal sealed partial class ObjectFile
{
    public Dictionary<long, SourceAllocationObligation> SourceAllocationObligations { get; } = [];
    public Dictionary<long, long> InternalTryAllocations { get; } = [];

    private void ReadAllocationObligations()
    {
        using var query = database.Prepare("SELECT id,function_entity_id,lifetime_id,ordinal,deallocator_entity_id,allocated_source_type_id,is_array,source_file_id,line,column,capture_proof FROM source_allocation_obligations ORDER BY lifetime_id,ordinal");
        while (query.Read())
        {
            var obligation = new SourceAllocationObligation(query.Integer(0), query.Integer(1), query.Integer(2),
                query.Integer(3), query.Integer(4), query.Integer(5), query.Integer(6) != 0,
                query.OptionalInteger(7), query.Integer(8), query.Integer(9), query.Text(10));
            if (obligation.Id <= 0 || SourceDestructionObligations.ContainsKey(obligation.Id) ||
                !SourceLifetimes.TryGetValue(obligation.LifetimeId, out SourceLifetime? lifetime) ||
                lifetime.FunctionEntityId != obligation.FunctionEntityId || obligation.Ordinal < 0 ||
                !EntitySourceRoutines.TryGetValue(obligation.DeallocatorEntityId, out RoutineType? routine) ||
                SourceFunctionKinds[obligation.DeallocatorEntityId] is not ("ordinary" or "runtime") ||
                routine.ObjectTypeId is not null || routine.Variadic ||
                Original(SourceTypeById(routine.ReturnTypeId)).Kind != "void" ||
                routine.Parameters.Length == 0 || Original(SourceTypeById(routine.Parameters[0])).Kind != "pointer" ||
                Original(SourceTypeById(obligation.AllocatedSourceTypeId)).Kind is "void" or "routine" or "lvalue-reference" or "rvalue-reference" ||
                query.Integer(6) is not (0 or 1) || obligation.Line < 0 || obligation.Column < 0 ||
                obligation.CaptureProof != "before-lowering" ||
                !SourceAllocationObligations.TryAdd(obligation.Id, obligation))
                throw new InvalidDataException("Allocation rollback lacks its original lifetime, allocated type or selected deallocator");
        }
        using var boundaries = database.Prepare("SELECT x.node_id,x.source_allocation_obligation_id,n.function_id FROM internal_try_expressions x JOIN nodes n ON n.id=x.node_id WHERE x.source_allocation_obligation_id IS NOT NULL");
        while (boundaries.Read())
        {
            if (!SourceAllocationObligations.TryGetValue(boundaries.Integer(1), out SourceAllocationObligation? obligation) ||
                OriginalFunction(boundaries.Integer(2)) != obligation.FunctionEntityId ||
                !InternalTryExpressions.Contains(boundaries.Integer(0)) ||
                !InternalTryAllocations.TryAdd(boundaries.Integer(0), obligation.Id))
                throw new InvalidDataException("Internal allocation boundary differs from its original obligation or function");
        }
        if (SourceAllocationObligations.Keys.Except(InternalTryAllocations.Values).Any())
            throw new InvalidDataException("Original allocation rollback lacks an observed execution boundary");
    }
}
