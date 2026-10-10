namespace DotObj;

internal sealed record SourceRecordTemporary(long Id, long? FunctionEntityId, long? SourceInitializerId,
    long SourceTypeId, long SourceOrdinal, long DynamicInitKind, long? SourceDestructionObligationId);

internal sealed partial class ObjectFile
{
    public Dictionary<long, SourceRecordTemporary> SourceRecordTemporaries { get; } = [];
    private readonly Dictionary<long, long> recordTemporaryInitializationSources = [];

    private bool RecordTemporaryOwner(SourceRecordTemporary source, long function)
    {
        if (source.FunctionEntityId is long original) return OriginalFunction(function) == original ||
            DefaultArgumentTemporaries.Contains((function,source.Id));
        if (source.SourceInitializerId is not long initializer) return false;
        if (IsSourceObligationExecution(initializer, null, function)) return true;
        if (FieldInitializerExecutions.TryGetValue((initializer, function), out var field))
            return field.ExecutionDisposition == "copied";
        return DynamicInitializers.Values.Any(value => value.SourceInitializerId == initializer &&
            InitializationActions.Any(action => action.Id == value.ActionId && action.EntityId == function));
    }

    private bool RecordTemporaryDestructionType(SourceRecordTemporary temporary, SourceDestructionObligation destruction)
    {
        SourceType source = Original(SourceTypeById(temporary.SourceTypeId));
        SourceType destroyed = Original(SourceTypeById(destruction.SourceTypeId));
        if (source.Id == destroyed.Id) return true;
        if (source.Kind != "array") return false;
        long count = 1;
        while (source.Kind == "array")
        {
            if (source.ElementCount is not long bound || bound <= 0 || count > long.MaxValue / bound) return false;
            count *= bound;
            source = Original(SourceTypeById(source.UnderlyingTypeId!.Value));
        }
        if (source.Id != destroyed.Id) return false;
        // Stock initializer-list backing arrays retain the whole temporary type
        // but their destructor obligation names its leaf class. Require the
        // corresponding observed full-array cleanup, including the exact bound.
        CleanupAction[] actions = CleanupActions.Values.Where(action => action.SourceObligationId == destruction.Id).ToArray();
        return actions.Length != 0 && actions.All(action => CleanupArrays.TryGetValue(action.Id, out var array) &&
            array.ElementCount == count && array.ElementTypeId == source.ExecutionTypeId);
    }

    private void ReadRecordTemporaryInitializations()
    {
        using (var query = database.Prepare("SELECT id,function_entity_id,source_initializer_id,source_type_id,source_ordinal,dynamic_init_kind,source_destruction_obligation_id,capture_proof FROM source_record_temporaries ORDER BY id"))
            while (query.Read())
            {
                var source = new SourceRecordTemporary(query.Integer(0), query.OptionalInteger(1), query.OptionalInteger(2),
                    query.Integer(3), query.Integer(4), query.Integer(5), query.OptionalInteger(6));
                if (SourceLanguage != "c++" || (source.FunctionEntityId is null) == (source.SourceInitializerId is null) ||
                    source.FunctionEntityId is long function && !SourceFunctionKinds.ContainsKey(function) ||
                    source.SourceInitializerId is long initializer && !SourceInitializers.ContainsKey(initializer) ||
                    source.SourceOrdinal < 0 || source.DynamicInitKind is < 0 or > 8 ||
                    DestructionElementType(source.SourceTypeId).Kind != "record" || query.Text(7) != "before-lowering" ||
                    !SourceRecordTemporaries.TryAdd(source.Id, source))
                    throw new InvalidDataException("Record temporary lacks original type, owner or initialization provenance");
                if (source.SourceDestructionObligationId is long obligation &&
                    (!SourceDestructionObligations.TryGetValue(obligation, out var destruction) ||
                     destruction.FunctionEntityId != source.FunctionEntityId || destruction.SourceInitializerId != source.SourceInitializerId ||
                     !RecordTemporaryDestructionType(source, destruction)))
                    throw new InvalidDataException("Record temporary destruction differs from its original initialization identity");
            }
        foreach (var owner in SourceRecordTemporaries.Values.GroupBy(value => (value.FunctionEntityId, value.SourceInitializerId)))
            if (owner.Select(value => value.SourceOrdinal).Distinct().Count() != owner.Count())
                throw new InvalidDataException("Original record temporary ordinals collide within their source owner");
        using var rows = database.Prepare("SELECT x.node_id,x.binding_id,x.source_temporary_id,x.lowered_init_kind,x.site_kind,x.zero_storage,x.partially_initialized,x.capture_proof,n.kind,n.function_id,b.function_id,b.kind,b.type_id,b.is_synthetic FROM record_temporary_initializations x LEFT JOIN nodes n ON n.id=x.node_id LEFT JOIN bindings b ON b.id=x.binding_id ORDER BY x.node_id");
        while (rows.Read())
        {
            long binding = rows.Integer(1), lowered = rows.Integer(3), function = rows.Integer(9);
            bool zero = rows.Integer(5) == 1, partial = rows.Integer(6) == 1;
            string type = Type(rows.Integer(12));
            string leaf = ScalarTypes.IsArray(type) ? ScalarTypes.Leaf(type) : type;
            if (!SourceRecordTemporaries.TryGetValue(rows.Integer(2), out var source) || !RecordTemporaryOwner(source, function) ||
                lowered is < 0 or > 5 || rows.Text(4) is not ("scope-entry" or "dynamic-init" or "lowered-assignment") ||
                rows.Text(4) == "scope-entry" && lowered != 3 || rows.Integer(5) is not (0 or 1) || rows.Integer(6) is not (0 or 1) ||
                zero != (lowered == 3 || lowered == 2 && partial) || partial && lowered != 2 ||
                rows.Text(7) != "observed-temporary-storage-initialization" || rows.OptionalText(8) != "record-storage-init" ||
                function != rows.Integer(10) || rows.OptionalText(11) != "local" || rows.Integer(13) != 1 || BindingSourceTypes.ContainsKey(binding) ||
                Original(SourceTypeById(source.SourceTypeId)).ExecutionTypeId != rows.Integer(12) ||
                !ScalarTypes.IsRecord(leaf) || RecordsByKey[leaf].RepresentationKind != "complete" ||
                !RecordStorageInitializations.TryAdd(rows.Integer(0), new(binding, zero)))
                throw new InvalidDataException("Record temporary initialization lacks its exact generated storage and original execution owner");
            recordTemporaryInitializationSources.Add(rows.Integer(0), source.Id);
        }
    }
    private void VerifyRecordTemporaryDestructionBinding(Node initialization, FunctionBody body)
    {
        if (!recordTemporaryInitializationSources.TryGetValue(initialization.Id, out long sourceId) ||
            SourceRecordTemporaries[sourceId].SourceDestructionObligationId is not long obligation) return;
        long binding = RecordStorageInitializations[initialization.Id].BindingId;
        bool Target(Node node, int depth = 0)
        {
            if (depth > 128) return false;
            if (node.Kind == "binding") return node.BindingId == binding;
            if (node.Kind is "address" or "convert" or "member" or "base-subobject" or "dereference" or "array-decay")
                return Target(body.Child(node.Id, "operand"), depth + 1);
            if (node.Kind == "binary" && node.Operator is "pointer-add" or "pointer-subtract" &&
                body.Child(node.Id, "right").Kind == "constant")
                return Target(body.Child(node.Id, "left"), depth + 1);
            return false;
        }
        bool matching = false, executable = false;
        foreach (var action in CleanupActions.Values.Where(action => action.FunctionEntityId == initialization.FunctionId && action.SourceObligationId == obligation))
        {
            executable = true;
            if (CleanupVerifier.TargetBindings(this, Entities[initialization.FunctionId], body, action).SetEquals([binding]))
                matching = true;
        }
        foreach (long call in NormalDestructions.Where(pair => pair.Value == obligation).Select(pair => pair.Key))
        {
            if (!body.Nodes.ContainsKey(call)) continue;
            executable = true;
            if (Target(body.Nodes[body.Children(call, "argument").First().ChildId])) matching = true;
        }
        foreach (var operation in ArrayRuntimeOperations.Values.Where(operation => operation.SourceObligationId == obligation))
        {
            if (!body.Nodes.ContainsKey(operation.NodeId)) continue;
            executable = true;
            if (Target(body.Nodes[body.Children(operation.NodeId, "argument").First().ChildId])) matching = true;
        }
        if (!matching && (executable || !DestructionElisions.ContainsKey((obligation, initialization.FunctionId))))
            throw new InvalidDataException("Record temporary destruction does not own its observed initialized binding");
    }

}
