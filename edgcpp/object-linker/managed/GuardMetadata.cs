namespace DotObj;

internal sealed record LocalStaticGuard(long Id, long FunctionEntityId, long StorageId, long GuardStorageId,
    long SourceTypeId, long SourceLifetimeId, string GuardPolicy, long? SourceFileId, long Line, long Column, string CaptureProof);
internal sealed record GuardOperation(long NodeId, long GuardId, string Kind, string CaptureProof);
internal sealed record GuardCleanupAction(long Id, long FunctionEntityId, long GuardId, long TargetNodeId, string CaptureProof);

internal sealed partial class ObjectFile
{
    public Dictionary<long, LocalStaticGuard> LocalStaticGuards { get; } = [];
    public Dictionary<long, GuardOperation> GuardOperations { get; } = [];
    public Dictionary<long, GuardCleanupAction> GuardCleanupActions { get; } = [];

    private void VerifyGuardAddress(long nodeId, LocalStaticGuard guard, long functionId)
    {
        using var query = database.Prepare($"SELECT kind,function_id,type_id,storage_id,constant_text FROM nodes WHERE id={nodeId}");
        if (!query.Read() || query.Text(0) != "storage-address" || query.Integer(1) != functionId ||
            query.OptionalInteger(2) is not long type || Type(type) != "ptr(u64)" ||
            query.OptionalInteger(3) != guard.GuardStorageId || query.OptionalText(4) != "0")
            throw new InvalidDataException("Guard operation address differs from its exact original guard storage");
    }

    private void ReadGuardMetadata()
    {
        using (var query = database.Prepare("SELECT id,function_entity_id,storage_id,guard_storage_id,source_type_id,source_lifetime_id,guard_policy,source_file_id,line,column,capture_proof FROM local_static_guards ORDER BY id"))
            while (query.Read())
            {
                var guard = new LocalStaticGuard(query.Integer(0), query.Integer(1), query.Integer(2), query.Integer(3),
                    query.Integer(4), query.Integer(5), query.Text(6), query.OptionalInteger(7), query.Integer(8), query.Integer(9), query.Text(10));
                if (guard.Id <= 0 || SourceLanguage != "c++" || !EntitySourceRoutines.ContainsKey(guard.FunctionEntityId) ||
                    !SourceLifetimes.TryGetValue(guard.SourceLifetimeId, out SourceLifetime? lifetime) ||
                    lifetime.FunctionEntityId != guard.FunctionEntityId || lifetime.Kind != "block" ||
                    !Storages.TryGetValue(guard.StorageId, out Storage? storage) || storage.SourceTypeId != guard.SourceTypeId ||
                    !Storages.TryGetValue(guard.GuardStorageId, out Storage? guardStorage) || guard.StorageId == guard.GuardStorageId ||
                    Type(guardStorage.TypeId) != "u64" || guardStorage.IsConst || guardStorage.IsThreadLocal || storage.IsThreadLocal ||
                    guardStorage.Definition == "declaration" || storage.Definition == "declaration" ||
                    SourceTypeById(guard.SourceTypeId).ExecutionTypeId != storage.TypeId ||
                    guard.GuardPolicy != "ia64-acquire-release" || guard.Line < 0 || guard.Column < 0 ||
                    guard.CaptureProof != "observed-local-static-guard" || !LocalStaticGuards.TryAdd(guard.Id, guard))
                    throw new InvalidDataException("Local static guard lacks its original storage, type, lifetime or publication contract");
            }
        if (LocalStaticGuards.Values.Select(guard => guard.GuardStorageId).Distinct().Count() != LocalStaticGuards.Count ||
            LocalStaticGuards.Values.Select(guard => guard.StorageId).Distinct().Count() != LocalStaticGuards.Count)
            throw new InvalidDataException("Local static guard storage ownership is not unique");
        using (var query = database.Prepare("SELECT g.node_id,g.guard_id,g.kind,g.capture_proof,n.kind,n.function_id,n.type_id FROM guard_operations g LEFT JOIN nodes n ON n.id=g.node_id ORDER BY g.node_id"))
            while (query.Read())
            {
                var operation = new GuardOperation(query.Integer(0), query.Integer(1), query.Text(2), query.Text(3));
                if (!LocalStaticGuards.TryGetValue(operation.GuardId, out LocalStaticGuard? guard) ||
                    operation.Kind is not ("probe" or "acquire" or "release") || operation.CaptureProof != "observed-local-static-guard-operation" ||
                    query.OptionalText(4) != "guard-operation" || query.OptionalInteger(5) is not long function ||
                    OriginalFunction(function) != guard.FunctionEntityId || query.OptionalInteger(6) is not long type ||
                    Type(type) != (operation.Kind == "release" ? "void" : "i32") || !GuardOperations.TryAdd(operation.NodeId, operation))
                    throw new InvalidDataException("Guard semantic operation lacks its exact observed function, kind and result type");
                using var address = database.Prepare($"SELECT child_id,role,ordinal FROM edges WHERE parent_id={operation.NodeId}");
                if (!address.Read() || address.Text(1) != "guard" || address.Integer(2) != 0)
                    throw new InvalidDataException("Guard semantic operation lacks its single guard address operand");
                VerifyGuardAddress(address.Integer(0), guard, function);
                if (address.Read()) throw new InvalidDataException("Guard semantic operation has extra operands");
            }
        foreach (LocalStaticGuard guard in LocalStaticGuards.Values)
            foreach (string kind in new[] { "probe", "acquire", "release" })
                if (!GuardOperations.Values.Any(operation => operation.GuardId == guard.Id && operation.Kind == kind))
                    throw new InvalidDataException("Local static guard lacks a complete acquire/publication sequence");
        using (var nodes = database.Prepare("SELECT id FROM nodes WHERE kind='guard-operation'"))
            while (nodes.Read())
                if (!GuardOperations.ContainsKey(nodes.Integer(0))) throw new InvalidDataException("Unobserved local static guard operation");
        using (var query = database.Prepare("SELECT id,function_entity_id,guard_id,target_node_id,capture_proof FROM guard_cleanup_actions ORDER BY id"))
            while (query.Read())
            {
                var action = new GuardCleanupAction(query.Integer(0), query.Integer(1), query.Integer(2), query.Integer(3), query.Text(4));
                if (action.Id <= 0 || !Functions.ContainsKey(action.FunctionEntityId) ||
                    !LocalStaticGuards.TryGetValue(action.GuardId, out LocalStaticGuard? guard) ||
                    OriginalFunction(action.FunctionEntityId) != guard.FunctionEntityId || action.CaptureProof != "observed-guard-region" ||
                    !GuardCleanupActions.TryAdd(action.Id, action))
                    throw new InvalidDataException("Guard rollback action lacks its exact observed function and original local initialization");
                VerifyGuardAddress(action.TargetNodeId, guard, action.FunctionEntityId);
            }
    }
}
