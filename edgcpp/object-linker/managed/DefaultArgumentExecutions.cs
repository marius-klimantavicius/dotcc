namespace DotObj;

internal sealed record DefaultArgumentEvaluation(long FunctionEntityId, long OwnerFunctionEntityId,
    long SourceFunctionEntityId, long TargetEntityId, long ForwardingCallNodeId, long RootSourceLifetimeId);
internal sealed record DefaultArgumentLifetime(long FunctionEntityId, long SourceLifetimeId,
    long? CopiedParentSourceLifetimeId, long? CopiedSiblingOrdinal);
internal sealed record DefaultArgumentObligation(long FunctionEntityId, long? SourceObligationId,
    long? SourceAllocationObligationId, long SourceLifetimeId, long CopiedOrdinal);

internal sealed partial class ObjectFile
{
    public Dictionary<long, DefaultArgumentEvaluation> DefaultArgumentEvaluations { get; } = [];
    public Dictionary<(long Function, long Lifetime), DefaultArgumentLifetime> DefaultArgumentLifetimes { get; } = [];
    public HashSet<(long Function, long Temporary)> DefaultArgumentTemporaries { get; } = [];
    private readonly List<DefaultArgumentObligation> defaultArgumentObligations = [];

    // Loaded before cleanup so original lifetimes remain owned by the source
    // caller while cleanup in the generated adapter can cite exact adoptions.
    private void ReadDefaultArgumentExecutions()
    {
        using (var query = database.Prepare("SELECT function_entity_id,owner_function_entity_id,source_function_entity_id,target_entity_id,forwarding_call_node_id,root_source_lifetime_id,capture_proof FROM default_argument_evaluations ORDER BY function_entity_id"))
            while (query.Read())
            {
                var value = new DefaultArgumentEvaluation(query.Integer(0),query.Integer(1),query.Integer(2),query.Integer(3),query.Integer(4),query.Integer(5));
                if (SourceLanguage != "c++" || query.Text(6) != "observed-default-argument-copy" ||
                    !AbiRoutineOrigins.TryGetValue(value.FunctionEntityId,out var origin) ||
                    origin.EntryKind != "constructor-default-adapter" || origin.TargetEntityId != value.TargetEntityId ||
                    !Functions.ContainsKey(value.OwnerFunctionEntityId) || !SourceFunctionKinds.ContainsKey(value.SourceFunctionEntityId) ||
                    OriginalFunction(value.OwnerFunctionEntityId) != value.SourceFunctionEntityId ||
                    value.FunctionEntityId == value.OwnerFunctionEntityId ||
                    !DefaultArgumentEvaluations.TryAdd(value.FunctionEntityId,value))
                    throw new InvalidDataException("Default argument evaluation lacks its exact adapter, selected constructor and original caller");
                RequireScalar($"SELECT count(*) FROM nodes WHERE id={value.ForwardingCallNodeId} AND function_id={value.FunctionEntityId} AND kind='call' AND entity_id={value.TargetEntityId}",1);
                RequireScalar($"SELECT count(*) FROM source_lifetimes WHERE id={value.RootSourceLifetimeId} AND function_entity_id={value.SourceFunctionEntityId} AND kind='expression-temporary'",1);
            }
        using (var query = database.Prepare("SELECT function_entity_id,source_lifetime_id,copied_parent_source_lifetime_id,copied_sibling_ordinal,capture_proof FROM default_argument_lifetime_executions ORDER BY function_entity_id,source_lifetime_id"))
            while (query.Read())
            {
                var value = new DefaultArgumentLifetime(query.Integer(0),query.Integer(1),query.OptionalInteger(2),query.OptionalInteger(3));
                if (!DefaultArgumentEvaluations.TryGetValue(value.FunctionEntityId,out var origin) ||
                    query.Text(4) != "observed-default-argument-copy" ||
                    (value.CopiedParentSourceLifetimeId is null) != (value.SourceLifetimeId == origin.RootSourceLifetimeId) ||
                    (value.CopiedParentSourceLifetimeId is null) != (value.CopiedSiblingOrdinal is null) || value.CopiedSiblingOrdinal < 0 ||
                    !DefaultArgumentLifetimes.TryAdd((value.FunctionEntityId,value.SourceLifetimeId),value))
                    throw new InvalidDataException("Default argument lifetime has invalid copied boundary provenance");
                RequireScalar($"SELECT count(*) FROM source_lifetimes WHERE id={value.SourceLifetimeId} AND function_entity_id={origin.SourceFunctionEntityId} AND kind='expression-temporary'",1);
            }
        using (var query = database.Prepare("SELECT function_entity_id,source_obligation_id,source_allocation_obligation_id,source_lifetime_id,copied_ordinal,capture_proof FROM default_argument_obligation_executions ORDER BY function_entity_id,source_lifetime_id,copied_ordinal"))
            while (query.Read())
            {
                var value = new DefaultArgumentObligation(query.Integer(0),query.OptionalInteger(1),query.OptionalInteger(2),query.Integer(3),query.Integer(4));
                if (!DefaultArgumentLifetimes.ContainsKey((value.FunctionEntityId,value.SourceLifetimeId)) ||
                    (value.SourceObligationId is null) == (value.SourceAllocationObligationId is null) || value.CopiedOrdinal < 0 ||
                    query.Text(5) != "observed-default-argument-copy")
                    throw new InvalidDataException("Default argument obligation lacks its copied lifetime and exact original kind");
                var origin = DefaultArgumentEvaluations[value.FunctionEntityId];
                string table = value.SourceObligationId is null ? "source_allocation_obligations" : "source_destruction_obligations";
                long id = value.SourceObligationId ?? value.SourceAllocationObligationId!.Value;
                RequireScalar($"SELECT count(*) FROM {table} WHERE id={id} AND lifetime_id={value.SourceLifetimeId} AND function_entity_id={origin.SourceFunctionEntityId}",1);
                defaultArgumentObligations.Add(value);
            }
        using (var query = database.Prepare("SELECT function_entity_id,source_temporary_id,capture_proof FROM default_argument_temporary_executions ORDER BY function_entity_id,source_temporary_id"))
            while (query.Read())
            {
                long function = query.Integer(0), temporary = query.Integer(1);
                if (!DefaultArgumentEvaluations.TryGetValue(function,out var origin) || query.Text(2) != "observed-default-argument-copy" ||
                    !DefaultArgumentTemporaries.Add((function,temporary)))
                    throw new InvalidDataException("Default argument temporary lacks its exact copied source identity");
                RequireScalar($"SELECT count(*) FROM source_record_temporaries WHERE id={temporary} AND function_entity_id={origin.SourceFunctionEntityId}",1);
            }
    }

    private void VerifyDefaultArgumentExecutions()
    {
        foreach (var origin in DefaultArgumentEvaluations.Values)
        {
            long function = origin.FunctionEntityId;
            var lifetimes = DefaultArgumentLifetimes.Values.Where(value=>value.FunctionEntityId==function).ToArray();
            if (!DefaultArgumentLifetimes.ContainsKey((function,origin.RootSourceLifetimeId)) || lifetimes.Length == 0)
                throw new InvalidDataException("Default argument adapter lost its copied expression lifetime root");
            foreach (var lifetime in lifetimes)
            {
                var source = SourceLifetimes[lifetime.SourceLifetimeId];
                if (source.FunctionEntityId != origin.SourceFunctionEntityId || source.Kind != "expression-temporary" ||
                    lifetime.CopiedParentSourceLifetimeId is long parent &&
                    (!DefaultArgumentLifetimes.ContainsKey((function,parent)) || source.ParentLifetimeId != parent))
                    throw new InvalidDataException("Copied default argument lifetime differs from its original source boundary");
                var visited = new HashSet<long>(); var current = lifetime;
                while (current.CopiedParentSourceLifetimeId is long next)
                {
                    if (!visited.Add(current.SourceLifetimeId)) throw new InvalidDataException("Copied default argument lifetime graph contains a cycle");
                    current = DefaultArgumentLifetimes[(function,next)];
                }
                if (current.SourceLifetimeId != origin.RootSourceLifetimeId)
                    throw new InvalidDataException("Copied default argument lifetime is disconnected from its observed root");
                var ordinals = defaultArgumentObligations.Where(value=>value.FunctionEntityId==function && value.SourceLifetimeId==source.Id)
                    .Select(value=>value.CopiedOrdinal).Order().ToArray();
                if (ordinals.Length == 0 || !ordinals.SequenceEqual(Enumerable.Range(0,ordinals.Length).Select(index=>(long)index)))
                    throw new InvalidDataException("Copied default argument destruction ordering is incomplete");
            }
            var destructionIds = defaultArgumentObligations.Where(value=>value.FunctionEntityId==function && value.SourceObligationId is not null)
                .Select(value=>value.SourceObligationId!.Value).ToHashSet();
            var allocationIds = defaultArgumentObligations.Where(value=>value.FunctionEntityId==function && value.SourceAllocationObligationId is not null)
                .Select(value=>value.SourceAllocationObligationId!.Value).ToHashSet();
            // The lifetime-level lookup never licenses another original object.
            // Check every actual source-obligation use in this execution body.
            using (var uses = database.Prepare($"SELECT source_obligation_id FROM cleanup_actions WHERE function_entity_id={function} AND source_obligation_id IS NOT NULL UNION SELECT d.source_obligation_id FROM normal_destructions d JOIN nodes n ON n.id=d.node_id WHERE n.function_id={function} AND d.source_obligation_id IS NOT NULL UNION SELECT a.source_obligation_id FROM array_runtime_operations a JOIN nodes n ON n.id=a.node_id WHERE n.function_id={function} AND a.source_obligation_id IS NOT NULL UNION SELECT source_obligation_id FROM destruction_elisions WHERE function_entity_id={function} UNION SELECT a.source_destruction_obligation_id FROM array_allocation_operations a JOIN nodes n ON n.id=a.node_id WHERE n.function_id={function} AND a.source_destruction_obligation_id IS NOT NULL UNION SELECT a.source_obligation_id FROM array_deletion_operations a JOIN nodes n ON n.id=a.node_id WHERE n.function_id={function} UNION SELECT a.source_obligation_id FROM throw_payload_subobjects a JOIN nodes n ON n.id=a.throw_node_id WHERE n.function_id={function}"))
                while(uses.Read()) if(!destructionIds.Contains(uses.Integer(0)))
                    throw new InvalidDataException("Default argument cleanup uses an original destruction that was not copied");
            using (var uses = database.Prepare($"SELECT source_allocation_obligation_id FROM allocation_cleanup_actions WHERE function_entity_id={function} UNION SELECT a.source_allocation_obligation_id FROM internal_try_expressions a JOIN nodes n ON n.id=a.node_id WHERE n.function_id={function} AND a.source_allocation_obligation_id IS NOT NULL UNION SELECT source_allocation_obligation_id FROM allocation_elisions WHERE function_entity_id={function} UNION SELECT a.source_allocation_obligation_id FROM array_allocation_operations a JOIN nodes n ON n.id=a.node_id WHERE n.function_id={function}"))
                while(uses.Read()) if(!allocationIds.Contains(uses.Integer(0)))
                    throw new InvalidDataException("Default argument cleanup uses an original allocation that was not copied");
            foreach (var pair in DefaultArgumentTemporaries.Where(pair=>pair.Function==function))
            {
                var source = SourceRecordTemporaries[pair.Temporary];
                if(source.FunctionEntityId!=origin.SourceFunctionEntityId ||
                    source.SourceDestructionObligationId is long obligation && !destructionIds.Contains(obligation))
                    throw new InvalidDataException("Default argument temporary differs from its exact original source cleanup");
            }
            using (var actual = database.Prepare($"SELECT source_temporary_id FROM record_temporary_initializations t JOIN nodes n ON n.id=t.node_id WHERE n.function_id={function}"))
                while(actual.Read())if(!DefaultArgumentTemporaries.Contains((function,actual.Integer(0))))
                    throw new InvalidDataException("Default argument storage initialization uses an uncopied source temporary");
            // Each call of the stock factory creates a fresh adapter. Multiple
            // uses in the same original execution are permitted; cross-caller
            // sharing requires a distinct observed adoption, not a wildcard.
            RequireScalar($"SELECT count(*) FROM nodes WHERE kind='call' AND entity_id={function}",0);
            using var callbacks = database.Prepare($"SELECT function_id FROM nodes WHERE kind='function-address' AND entity_id={function}");
            int count=0;
            while(callbacks.Read()) { ++count; if(callbacks.Integer(0)!=origin.OwnerFunctionEntityId)
                throw new InvalidDataException("Default argument adapter escaped its observed factory caller"); }
            if(count==0)throw new InvalidDataException("Default argument adapter lacks its observed array constructor use");
        }
    }

    public void VerifyDefaultArgumentBody(long functionId, FunctionBody body)
    {
        foreach(var origin in DefaultArgumentEvaluations.Values.Where(value=>value.OwnerFunctionEntityId==functionId))
            foreach(var leaf in body.Nodes.Values.Where(node=>node.Kind=="function-address" && node.EntityId==origin.FunctionEntityId))
            {
                long current=leaf.Id;
                for(int depth=0;;++depth)
                {
                    if(depth>128)throw new InvalidDataException("Default argument adapter callback nesting exceeds limit");
                    var parents=body.Edges.Values.SelectMany(edges=>edges).Where(edge=>edge.ChildId==current).ToArray();
                    if(parents.Length!=1)throw new InvalidDataException("Default argument adapter callback escapes its exact array operation");
                    var edge=parents[0];var parent=body.Nodes[edge.ParentId];
                    if(ArrayRuntimeOperations.TryGetValue(parent.Id,out var construction) &&
                       construction.OperationKind=="construct" && parent.Kind=="call" && edge.Role=="argument" && edge.Ordinal==3 &&
                       RuntimeArrayProvider.TryGetMethod(new Symbol(this,Entities[parent.EntityId!.Value]))=="Construct")
                        break;
                    if(ArrayAllocationOperations.TryGetValue(parent.Id,out var allocation) && allocation.ConstructorEntityId==origin.FunctionEntityId &&
                       parent.Kind=="call" && edge.Role=="argument" && edge.Ordinal==3)
                        break;
                    if(parent.Kind!="convert" || edge.Role!="operand")
                        throw new InvalidDataException("Default argument adapter callback has an unowned use");
                    current=parent.Id;
                }
            }
        if(!DefaultArgumentEvaluations.TryGetValue(functionId,out var evaluation))return;
        var call=body.Nodes[evaluation.ForwardingCallNodeId];
        var args=body.Children(call.Id,"argument").ToArray();
        if(call.Kind!="call" || call.EntityId!=evaluation.TargetEntityId || args.Length<2)
            throw new InvalidDataException("Default argument adapter changed its selected forwarding constructor");
        var receiver=body.Nodes[args[0].ChildId];
        while(receiver.Kind=="convert")receiver=body.Child(receiver.Id,"operand");
        if(receiver.Kind!="binding" || receiver.BindingId is not long binding ||
            !body.Bindings.TryGetValue(binding,out var parameter) || parameter.Kind!="parameter" || parameter.Ordinal!=0)
            throw new InvalidDataException("Default argument constructor does not receive its exact object parameter");
        foreach(var pair in ExpressionLifetimes.Where(pair=>body.Nodes.ContainsKey(pair.Key)))
            if(!DefaultArgumentLifetimes.ContainsKey((functionId,pair.Value)))
                throw new InvalidDataException("Default argument expression lifetime lacks a copied original boundary");
    }
}
