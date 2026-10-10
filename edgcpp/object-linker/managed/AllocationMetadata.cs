namespace DotObj;

internal sealed record SourceAllocationObligation(long Id, long FunctionEntityId, long LifetimeId,
    long Ordinal, long DeallocatorEntityId, long AllocatedSourceTypeId, bool IsArray,
    long? SourceFileId, long Line, long Column, string CaptureProof);
internal sealed record AllocationCleanupAction(long Id, long FunctionEntityId, long SourceAllocationObligationId,
    long CallbackEntityId, long TargetNodeId, long? AllocationSize, long? ConditionBindingId,
    string NullPolicy, string CaptureProof);

internal sealed partial class ObjectFile
{
    public Dictionary<long, SourceAllocationObligation> SourceAllocationObligations { get; } = [];
    public Dictionary<long, long> InternalTryAllocations { get; } = [];
    public Dictionary<long, AllocationCleanupAction> AllocationCleanupActions { get; } = [];

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
        using var actions = database.Prepare("SELECT a.id,a.function_entity_id,a.source_allocation_obligation_id,a.callback_entity_id,a.target_node_id,a.allocation_size,a.condition_binding_id,a.null_policy,a.capture_proof,n.function_id,n.type_id FROM allocation_cleanup_actions a LEFT JOIN nodes n ON n.id=a.target_node_id ORDER BY a.id");
        while (actions.Read())
        {
            var action = new AllocationCleanupAction(actions.Integer(0), actions.Integer(1), actions.Integer(2), actions.Integer(3),
                actions.Integer(4), actions.OptionalInteger(5), actions.OptionalInteger(6), actions.Text(7), actions.Text(8));
            if (action.Id <= 0 || !SourceAllocationObligations.TryGetValue(action.SourceAllocationObligationId, out SourceAllocationObligation? obligation) ||
                obligation.IsArray || OriginalFunction(action.FunctionEntityId) != obligation.FunctionEntityId ||
                !Functions.ContainsKey(action.FunctionEntityId) || actions.OptionalInteger(9) != action.FunctionEntityId ||
                actions.OptionalInteger(10) is not long targetType || Type(targetType) != "ptr(void)" ||
                !Entities.TryGetValue(action.CallbackEntityId, out Entity? callback) || OriginalFunction(callback.Id) != obligation.DeallocatorEntityId ||
                Type(callback.ReturnTypeId) != "void" || Routines[callback.RoutineTypeId].Variadic ||
                action.NullPolicy != "skip-null" || action.CaptureProof != "observed-allocation-region" ||
                !AllocationCleanupActions.TryAdd(action.Id, action))
                throw new InvalidDataException("Allocation cleanup region lacks its exact source obligation, function, pointer or delete callback");
            var parameters = Parameters.GetValueOrDefault(callback.Id, []);
            if (parameters.Count != (action.AllocationSize is null ? 1 : 2) || parameters[0] != "ptr(void)" ||
                action.AllocationSize is long size && (parameters[1] != "u64" || size <= 0 ||
                    size != StorageSize(Type(SourceTypeById(obligation.AllocatedSourceTypeId).ExecutionTypeId))))
                throw new InvalidDataException("Allocation cleanup size differs from its exact sized-delete ABI and allocated type");
            if (action.ConditionBindingId is long condition)
            {
                using var flag = database.Prepare($"SELECT function_id,kind,type_id,is_synthetic FROM bindings WHERE id={condition}");
                if (!flag.Read() || flag.Integer(0) != action.FunctionEntityId || flag.Text(1) != "local" ||
                    Type(flag.Integer(2)) != "i32" || flag.Integer(3) != 1)
                    throw new InvalidDataException("Allocation cleanup condition lacks its exact owned synthetic flag");
            }
        }
        using (var duplicates = database.Prepare("SELECT id FROM (SELECT id FROM cleanup_actions UNION ALL SELECT id FROM allocation_cleanup_actions UNION ALL SELECT id FROM guard_cleanup_actions) GROUP BY id HAVING count(*)<>1"))
            if (duplicates.Read()) throw new InvalidDataException("Cleanup action identifiers overlap between action kinds");
        if (SourceAllocationObligations.Keys.Except(InternalTryAllocations.Values
                .Concat(AllocationCleanupActions.Values.Select(action => action.SourceAllocationObligationId))).Any())
            throw new InvalidDataException("Original allocation rollback lacks an observed execution boundary");
    }
}
