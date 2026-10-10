namespace DotObj;

internal sealed record SourceInitializer(long Id, long? StorageId, long SourceTypeId, long? FunctionEntityId,
    long? SourceOrdinal, string Kind, long? SourceFileId, long Line, long Column, string CaptureProof,
    string SourceName, string ExecutionDisposition, long? SourceFieldId);
internal sealed record SourceStaticLifetime(long Id, long? FunctionEntityId, long? SourceInitializerId,
    long? ParentLifetimeId, long? ParentObligationId, long? SiblingOrdinal, string Kind,
    long? SourceFileId, long Line, long Column, string CaptureProof);
internal sealed record SourceStaticDestruction(long Id, long LifetimeId, long Ordinal, long? StorageId,
    long SourceTypeId, long SelectedRoutineEntityId, long? SourceInitializerId, string Kind, bool? IsArrayAllocation,
    string Domain, long? SourceFileId, long Line, long Column, string CaptureProof);
internal sealed record InitializerLifetimeExecution(long SourceLifetimeId, long SourceStaticLifetimeId,
    long SourceInitializerId, long FunctionEntityId, string CaptureProof);
internal sealed record StaticDestructionRegistration(long NodeId, long SourceObligationId, long SourceInitializerId,
    long FunctionEntityId, long CallbackEntityId, long ObjectNodeId, string Domain, string CaptureProof);
internal sealed record GeneratedStaticDestructor(long FunctionEntityId, long SourceObligationId, long ParameterBindingId,
    long StorageId, long? ElementTypeId, long? ElementCount, long? ElementSize, string CaptureProof);

internal sealed partial class ObjectFile
{
    public Dictionary<long, SourceInitializer> SourceInitializers { get; } = [];
    public Dictionary<long, SourceStaticLifetime> SourceStaticLifetimes { get; } = [];
    public Dictionary<long, SourceStaticDestruction> SourceStaticDestructions { get; } = [];
    public Dictionary<(long Lifetime, long Function), InitializerLifetimeExecution> InitializerLifetimeExecutions { get; } = [];
    public Dictionary<long, StaticDestructionRegistration> StaticDestructionRegistrations { get; } = [];
    public Dictionary<long, GeneratedStaticDestructor> GeneratedStaticDestructors { get; } = [];
    public Dictionary<long, string> StaticDestructionDispositions { get; } = [];

    private string StaticDestructorSourceSignature(GeneratedStaticDestructor destruction)
    {
        var obligation = SourceStaticDestructions[destruction.SourceObligationId];
        var storage = Storages[destruction.StorageId];
        string location = "";
        if (obligation.SourceFileId is long fileId)
        {
            using var file = database.Prepare($"SELECT path FROM source_files WHERE id={fileId}");
            if (!file.Read()) throw new InvalidDataException("Static destructor source file is missing");
            location = file.Text(0);
        }
        string identity = storage.ExternalKey ?? UnitId + ";" + location + ";" + obligation.Line + ";" + obligation.Column + ";" + storage.SourceName;
        return "generated-static-destructor(" + SourceTypeSignature(obligation.SourceTypeId) + ";" +
            Convert.ToHexString(System.Text.Encoding.UTF8.GetBytes(identity)) + ";" + EntityRoutine(destruction.FunctionEntityId) +
            ";" + destruction.ElementCount + ";" + destruction.ElementSize + ")";
    }

    public bool IsSourceLifetimeExecution(SourceLifetime lifetime, long functionId)
        => lifetime.FunctionEntityId is long sourceFunction ? OriginalFunction(functionId) == sourceFunction ||
            DefaultArgumentLifetimes.ContainsKey((functionId,lifetime.Id))
            : lifetime.SourceInitializerId is long initializer &&
              InitializerLifetimeExecutions.TryGetValue((lifetime.Id, functionId), out var execution) &&
              execution.SourceInitializerId == initializer;

    public bool IsSourceObligationExecution(long? sourceInitializerId, long? functionEntityId, long functionId)
        => functionEntityId is long sourceFunction ? OriginalFunction(functionId) == sourceFunction
            : sourceInitializerId is long initializer && InitializerLifetimeExecutions.Values.Any(
                execution => execution.SourceInitializerId == initializer && execution.FunctionEntityId == functionId);

    public void VerifyStaticDestructionUse(long sourceStaticObligationId, long functionEntityId,
        long callbackEntityId, string operationKind)
    {
        if (SourceStaticDestructions.TryGetValue(sourceStaticObligationId, out var fieldPartial) &&
            fieldPartial.SourceInitializerId is long fieldInitializer && SourceInitializers[fieldInitializer].Kind == "field-default")
        {
            VerifyFieldInitializerPartialDestructionUse(sourceStaticObligationId, functionEntityId, callbackEntityId, operationKind);
            return;
        }
        if (!SourceStaticDestructions.TryGetValue(sourceStaticObligationId, out var obligation) ||
            obligation.Kind != "persistent" || obligation.Domain is not ("process" or "thread") ||
            !GeneratedStaticDestructors.TryGetValue(functionEntityId, out var generated) ||
            generated.SourceObligationId != obligation.Id ||
            OriginalFunction(callbackEntityId) != obligation.SelectedRoutineEntityId ||
            operationKind is not ("destroy" or "destruction" or "normal"))
            throw new InvalidDataException("Static destruction operation lacks its original selected destructor and generated callback owner");
    }

    private long StaticOperandLeaf(long nodeId, long functionId)
    {
        var seen = new HashSet<long>();
        while (seen.Add(nodeId))
        {
            using var node = database.Prepare($"SELECT kind,function_id FROM nodes WHERE id={nodeId}");
            if (!node.Read() || node.Integer(1) != functionId) throw new InvalidDataException("Static operand belongs to a foreign function");
            if (node.Text(0) is not ("convert" or "address" or "array-decay")) return nodeId;
            using var edge = database.Prepare($"SELECT role,ordinal,child_id FROM edges WHERE parent_id={nodeId}");
            if (!edge.Read() || edge.Text(0) != "operand" || edge.Integer(1) != 0)
                throw new InvalidDataException("Static operand conversion lacks its exact child");
            nodeId = edge.Integer(2);
            if (edge.Read()) throw new InvalidDataException("Static operand has unexpected extra evaluation");
        }
        throw new InvalidDataException("Static operand contains a cycle");
    }

    private void VerifyStaticObject(long nodeId, long functionId, long storageId, bool nullObject)
    {
        using var node = database.Prepare($"SELECT kind,storage_id,constant_text FROM nodes WHERE id={StaticOperandLeaf(nodeId, functionId)}");
        if (!node.Read() || (nullObject ? node.Text(0) != "constant" || node.OptionalText(2) != "0"
            : node.Text(0) is not ("storage" or "storage-address") || node.OptionalInteger(1) != storageId ||
              node.Text(0) == "storage-address" && node.OptionalText(2) != "0"))
            throw new InvalidDataException("Static destruction object differs from its original complete storage");
    }

    private void ReadStaticSources()
    {
        using (var q = database.Prepare("SELECT id,storage_id,source_type_id,function_entity_id,source_ordinal,kind,source_file_id,line,column,capture_proof,source_name,execution_disposition,source_field_id FROM source_initializers ORDER BY id"))
            while (q.Read())
            {
                var value = new SourceInitializer(q.Integer(0), q.OptionalInteger(1), q.Integer(2), q.OptionalInteger(3), q.OptionalInteger(4),
                    q.Text(5), q.OptionalInteger(6), q.Integer(7), q.Integer(8), q.Text(9), q.Text(10), q.Text(11), q.OptionalInteger(12));
                bool fieldTemplate = value.Kind == "field-default";
                if ((fieldTemplate ? value.ExecutionDisposition != "source-template" || value.StorageId is not null ||
                        value.FunctionEntityId is not null || value.SourceOrdinal is not null ||
                        value.SourceFieldId is not long fieldId || !SourceFields.TryGetValue(fieldId, out var field) ||
                        !field.HasInitializer || field.SourceTypeId != value.SourceTypeId || field.SourceName != value.SourceName
                    : value.SourceFieldId is not null ||
                      (value.ExecutionDisposition == "emitted" ? value.StorageId is not long storageId ||
                        !Storages.TryGetValue(storageId, out var storage) || storage.SourceTypeId != value.SourceTypeId
                        : value.ExecutionDisposition != "stock-not-emitted" || value.StorageId is not null) ||
                      (value.Kind == "file" ? value.FunctionEntityId is not null || value.SourceOrdinal is null
                        : value.Kind != "local-static" || value.FunctionEntityId is not long function || !EntitySourceRoutines.ContainsKey(function) || value.SourceOrdinal is not null)) ||
                    !SourceTypes.ContainsKey(value.SourceTypeId) ||
                    value.Line < 0 || value.Column < 0 || value.CaptureProof != "before-lowering" || !SourceInitializers.TryAdd(value.Id, value))
                    throw new InvalidDataException("Static initializer lacks its original declaration, type or source owner");
            }
        if (SourceInitializers.Values.Where(value => value.Kind == "file").Select(value => value.SourceOrdinal).Distinct().Count() !=
            SourceInitializers.Values.Count(value => value.Kind == "file"))
            throw new InvalidDataException("File initializer original ordinals are not unique");
        using (var q = database.Prepare("SELECT id,function_entity_id,source_initializer_id,parent_lifetime_id,parent_obligation_id,sibling_ordinal,kind,source_file_id,line,column,capture_proof FROM source_static_lifetimes ORDER BY id"))
            while (q.Read())
            {
                var value = new SourceStaticLifetime(q.Integer(0), q.OptionalInteger(1), q.OptionalInteger(2), q.OptionalInteger(3),
                    q.OptionalInteger(4), q.OptionalInteger(5), q.Text(6), q.OptionalInteger(7), q.Integer(8), q.Integer(9), q.Text(10));
                if (value.FunctionEntityId is long function && !EntitySourceRoutines.ContainsKey(function) ||
                    value.SourceInitializerId is long initializer && (!SourceInitializers.TryGetValue(initializer, out var source) || source.FunctionEntityId != value.FunctionEntityId) ||
                    value.Kind is not ("global-static" or "function-static" or "block" or "block-after-label" or "expression-temporary" or "try-block") ||
                    value.Kind == "global-static" && (value.FunctionEntityId is not null || value.ParentLifetimeId is not null) ||
                    value.Kind == "function-static" && (value.FunctionEntityId is null || value.ParentLifetimeId is not null) ||
                    value.CaptureProof != "before-lowering" || !SourceStaticLifetimes.TryAdd(value.Id, value))
                    throw new InvalidDataException("Static source lifetime has invalid original owner, kind or capture");
            }
        using (var q = database.Prepare("SELECT id,lifetime_id,ordinal,storage_id,source_type_id,selected_routine_entity_id,source_initializer_id,kind,is_array_allocation,domain,source_file_id,line,column,capture_proof FROM source_static_destructions ORDER BY id"))
            while (q.Read())
            {
                var value = new SourceStaticDestruction(q.Integer(0), q.Integer(1), q.Integer(2), q.OptionalInteger(3), q.Integer(4),
                    q.Integer(5), q.OptionalInteger(6), q.Text(7), q.OptionalInteger(8) is long array ? array != 0 : null,
                    q.Text(9), q.OptionalInteger(10), q.Integer(11), q.Integer(12), q.Text(13));
                if (!SourceStaticLifetimes.TryGetValue(value.LifetimeId, out var lifetime) ||
                    !SourceFunctionKinds.TryGetValue(value.SelectedRoutineEntityId, out string? kind) ||
                    (value.Kind == "allocation" ? value.IsArrayAllocation is null || kind is not ("ordinary" or "runtime")
                        : value.IsArrayAllocation is not null || kind != "destructor" || DestructionElementType(value.SourceTypeId).Kind != "record") ||
                    value.Kind is not ("persistent" or "partial-aggregate" or "allocation" or "automatic") ||
                    value.Kind != "partial-aggregate" && (value.Kind == "persistent" != (lifetime.Kind is "global-static" or "function-static")) ||
                    value.SourceInitializerId is long initializer && !SourceInitializers.ContainsKey(initializer) ||
                    value.StorageId is long storage && !Storages.ContainsKey(storage) ||
                    value.Domain is not ("process" or "thread") || value.CaptureProof != "before-lowering" ||
                    !SourceStaticDestructions.TryAdd(value.Id, value))
                    throw new InvalidDataException("Static source obligation lacks exact original type, lifetime or selected routine");
                if (value.Kind == "persistent" && (value.SourceInitializerId is not long initializerId ||
                    SourceInitializers[initializerId].StorageId != value.StorageId || SourceInitializers[initializerId].SourceTypeId != value.SourceTypeId ||
                    SourceInitializers[initializerId].FunctionEntityId != lifetime.FunctionEntityId ||
                    value.StorageId is long storageId && Storages[storageId].IsThreadLocal != (value.Domain == "thread")))
                    throw new InvalidDataException("Persistent destruction differs from its original static declaration");
                if (value.Kind == "partial-aggregate" && (value.SourceInitializerId is not long aggregateInitializer ||
                    value.StorageId is not null || SourceInitializers[aggregateInitializer].FunctionEntityId != lifetime.FunctionEntityId))
                    throw new InvalidDataException("Partial aggregate destruction lacks its exact containing source initializer");
                if (value.Kind != "allocation" && (EntitySourceRoutines[value.SelectedRoutineEntityId].ObjectTypeId is not long receiver ||
                    Original(SourceTypeById(receiver)).ExecutionTypeId != DestructionElementType(value.SourceTypeId).ExecutionTypeId))
                    throw new InvalidDataException("Static original destructor receiver differs from original destroyed type");
            }
        foreach (var lifetime in SourceStaticLifetimes.Values)
        {
            var visited = new HashSet<long>(); var current = lifetime;
            while (current.ParentLifetimeId is long parent)
            {
                if (!visited.Add(current.Id) || !SourceStaticLifetimes.TryGetValue(parent, out current!))
                    throw new InvalidDataException("Static source lifetime graph has a cycle or missing parent");
            }
            if (lifetime.ParentObligationId is long obligation &&
                (!SourceStaticDestructions.TryGetValue(obligation, out var destruction) || destruction.LifetimeId != lifetime.ParentLifetimeId))
                throw new InvalidDataException("Static lifetime parent obligation does not belong to its exact parent");
        }
        foreach (var group in SourceStaticDestructions.Values.GroupBy(value => value.LifetimeId))
            if (group.OrderBy(value => value.Ordinal).Where((value, ordinal) => value.Ordinal != ordinal).Any())
                throw new InvalidDataException("Original static destruction ordinals are not dense");
        foreach (var group in SourceStaticLifetimes.Values.Where(value => value.SiblingOrdinal is not null).GroupBy(value => value.ParentLifetimeId))
            if (group.Key is null || group.OrderBy(value => value.SiblingOrdinal).Where((value, ordinal) => value.SiblingOrdinal != ordinal).Any())
                throw new InvalidDataException("Original static lifetime child ordering is not dense");
        using (var q = database.Prepare("SELECT source_lifetime_id,source_static_lifetime_id,source_initializer_id,function_entity_id,capture_proof FROM initializer_lifetime_executions"))
            while (q.Read())
            {
                var value = new InitializerLifetimeExecution(q.Integer(0), q.Integer(1), q.Integer(2), q.Integer(3), q.Text(4));
                if (!SourceStaticLifetimes.ContainsKey(value.SourceStaticLifetimeId) || !SourceInitializers.ContainsKey(value.SourceInitializerId) ||
                    !Functions.ContainsKey(value.FunctionEntityId) || value.CaptureProof != "observed-initializer-lifetime-adoption" ||
                    !InitializerLifetimeExecutions.TryAdd((value.SourceLifetimeId, value.FunctionEntityId), value))
                    throw new InvalidDataException("Initializer lifetime execution lacks original boundary or observed owner");
            }
        using (var q = database.Prepare("SELECT function_entity_id,source_obligation_id,parameter_binding_id,storage_id,element_type_id,element_count,element_size,capture_proof FROM generated_static_destructors"))
            while (q.Read())
            {
                var value = new GeneratedStaticDestructor(q.Integer(0), q.Integer(1), q.Integer(2), q.Integer(3),
                    q.OptionalInteger(4), q.OptionalInteger(5), q.OptionalInteger(6), q.Text(7));
                if (!Functions.ContainsKey(value.FunctionEntityId) || EntitySourceRoutines.ContainsKey(value.FunctionEntityId) ||
                    !SourceStaticDestructions.TryGetValue(value.SourceObligationId, out var obligation) || obligation.Kind != "persistent" ||
                    obligation.StorageId != value.StorageId || value.CaptureProof != "observed-static-destructor-body" ||
                    !GeneratedStaticDestructors.TryAdd(value.FunctionEntityId, value))
                    throw new InvalidDataException("Generated static destructor lacks its original persistent source obligation");
            }
        ReadFieldInitializerMetadata();
    }

    private void ReadStaticExecutionMetadata()
    {
        VerifyFieldInitializerExecutionMetadata();
        foreach (var execution in InitializerLifetimeExecutions.Values)
        {
            var original = SourceStaticLifetimes[execution.SourceStaticLifetimeId];
            if (!SourceLifetimes.TryGetValue(execution.SourceLifetimeId, out var lifetime) ||
                lifetime.SourceInitializerId != execution.SourceInitializerId || lifetime.FunctionEntityId is not null ||
                original.SourceInitializerId != execution.SourceInitializerId || original.Kind != lifetime.Kind ||
                !(SourceInitializers[execution.SourceInitializerId].Kind == "field-default"
                    ? FieldInitializerExecutions.TryGetValue((execution.SourceInitializerId, execution.FunctionEntityId), out var fieldExecution) && fieldExecution.ExecutionDisposition == "copied"
                    : GeneratedRoutineOrigins.ContainsKey(execution.FunctionEntityId)))
                throw new InvalidDataException("Initializer lifetime adoption differs from its original source boundary");
            if (lifetime.ParentLifetimeId is null && original.ParentObligationId is not null &&
                (SourceInitializers[execution.SourceInitializerId].Kind != "file" ||
                 lifetime.ParentObligationId is not null || lifetime.ParentAllocationObligationId is not null ||
                 lifetime.SiblingOrdinal != 0 || original.ParentLifetimeId is not long outer ||
                 SourceStaticLifetimes[outer].Kind != "global-static"))
                throw new InvalidDataException("Initializer execution root lost its exact outer static lifetime boundary");
        }
        using (var q = database.Prepare("SELECT node_id,source_obligation_id,source_initializer_id,function_entity_id,callback_entity_id,object_node_id,domain,capture_proof FROM static_destruction_registrations"))
            while (q.Read())
            {
                var value = new StaticDestructionRegistration(q.Integer(0), q.Integer(1), q.Integer(2), q.Integer(3), q.Integer(4), q.Integer(5), q.Text(6), q.Text(7));
                if (!SourceStaticDestructions.TryGetValue(value.SourceObligationId, out var obligation) || obligation.Kind != "persistent" ||
                    obligation.SourceInitializerId != value.SourceInitializerId || value.Domain is not ("process" or "thread") || obligation.Domain != value.Domain ||
                    !Functions.ContainsKey(value.FunctionEntityId) || !Entities.ContainsKey(value.CallbackEntityId) ||
                    value.CaptureProof != "observed-static-destruction-registration" || !StaticDestructionRegistrations.TryAdd(value.NodeId, value))
                    throw new InvalidDataException("Static destruction registration lacks exact source ownership and lifetime domain");
                bool generated = GeneratedStaticDestructors.TryGetValue(value.CallbackEntityId, out var callback);
                if (generated ? callback!.SourceObligationId != obligation.Id : OriginalFunction(value.CallbackEntityId) != obligation.SelectedRoutineEntityId)
                    throw new InvalidDataException("Static registration callback differs from original selected destruction");
                if (!generated && AbiRoutineOrigins.TryGetValue(value.CallbackEntityId, out var origin) && origin.EntryKind != "complete")
                    throw new InvalidDataException("Static registration requires the complete-object destructor entry");
                var initializer = SourceInitializers[value.SourceInitializerId];
                if (initializer.ExecutionDisposition != "emitted" || initializer.StorageId is null)
                    throw new InvalidDataException("Static registration cannot execute a source-only initializer");
                if (initializer.FunctionEntityId is long sourceFunction ? OriginalFunction(value.FunctionEntityId) != sourceFunction :
                    !GeneratedRoutineOrigins.ContainsKey(value.FunctionEntityId))
                    throw new InvalidDataException("Static registration executes outside its original initializer owner");
                using var node = database.Prepare($"SELECT kind,function_id,type_id FROM nodes WHERE id={value.NodeId}");
                if (!node.Read() || node.Text(0) != "static-destruction-register" || node.Integer(1) != value.FunctionEntityId || Type(node.Integer(2)) != "i32")
                    throw new InvalidDataException("Static registration is not its exact typed semantic operation");
                using var edges = database.Prepare($"SELECT role,ordinal,child_id FROM edges WHERE parent_id={value.NodeId} ORDER BY role");
                if (!edges.Read() || edges.Text(0) != "callback" || edges.Integer(1) != 0)
                    throw new InvalidDataException("Static registration lacks its callback operand");
                long callbackNode = StaticOperandLeaf(edges.Integer(2), value.FunctionEntityId);
                using var callbackQuery = database.Prepare($"SELECT kind,entity_id FROM nodes WHERE id={callbackNode}");
                if (!callbackQuery.Read() || callbackQuery.Text(0) != "function-address" || callbackQuery.OptionalInteger(1) != value.CallbackEntityId)
                    throw new InvalidDataException("Static registration callback expression differs from selected callable");
                if (!edges.Read() || edges.Text(0) != "object" || edges.Integer(1) != 0 || edges.Integer(2) != value.ObjectNodeId || edges.Read())
                    throw new InvalidDataException("Static registration object operand or argument count changed");
                VerifyStaticObject(value.ObjectNodeId, value.FunctionEntityId, obligation.StorageId!.Value, generated);
            }
        foreach (var generated in GeneratedStaticDestructors.Values)
        {
            using var formal = database.Prepare($"SELECT function_id,kind,ordinal,type_id,is_synthetic FROM bindings WHERE id={generated.ParameterBindingId}");
            if (!formal.Read() || formal.Integer(0) != generated.FunctionEntityId || formal.Text(1) != "parameter" || formal.OptionalInteger(2) != 0 ||
                Type(formal.Integer(3)) != "ptr(void)" || formal.Integer(4) != 1 ||
                ParameterRoles[(generated.FunctionEntityId, 0)].Role != "static-object" ||
                Parameters[generated.FunctionEntityId].Count != 1 || Type(Entities[generated.FunctionEntityId].ReturnTypeId) != "void")
                throw new InvalidDataException("Generated static destructor lacks its exact synthetic void-pointer formal");
            var obligation = SourceStaticDestructions[generated.SourceObligationId];
            var original = Original(SourceTypeById(obligation.SourceTypeId));
            long count = 1;
            while (original.Kind == "array")
            {
                using var extent = database.Prepare($"SELECT element_count FROM types WHERE id={original.ExecutionTypeId}");
                if (!extent.Read() || extent.OptionalInteger(0) is not long length || length <= 0)
                    throw new InvalidDataException("Generated static array callback lacks its original fixed extent");
                count = checked(count * length); original = Original(SourceTypeById(original.UnderlyingTypeId!.Value));
            }
            if (generated.ElementTypeId is long element && (original.ExecutionTypeId != element || generated.ElementCount != count ||
                generated.ElementSize != StorageSize(Type(element))))
                throw new InvalidDataException("Generated static array destruction differs from original element type, count or stride");
            if (!StaticDestructionRegistrations.Values.Any(value => value.CallbackEntityId == generated.FunctionEntityId))
                throw new InvalidDataException("Generated static destructor has no observed owned registration");
        }
        using (var q = database.Prepare("SELECT source_obligation_id,kind,capture_proof FROM static_destruction_dispositions"))
            while (q.Read())
            {
                long id = q.Integer(0); string kind = q.Text(1);
                if (!SourceStaticDestructions.TryGetValue(id, out var obligation) || obligation.Kind is not ("persistent" or "partial-aggregate") ||
                    (obligation.Kind == "persistent" ? kind is not ("registered" or "stock-proven-no-effect" or "stock-not-emitted")
                        : kind is not ("cleanup-action" or "stock-terminal-no-throw" or "stock-not-emitted" or "source-template")) || q.Text(2) != "observed-static-destruction-disposition" ||
                    (kind == "source-template") != (obligation.SourceInitializerId is long templateInitializer && SourceInitializers[templateInitializer].Kind == "field-default") ||
                    (kind == "registered") != StaticDestructionRegistrations.Values.Any(value => value.SourceObligationId == id) ||
                    (kind == "stock-not-emitted") != (obligation.SourceInitializerId is long initializer && SourceInitializers[initializer].ExecutionDisposition == "stock-not-emitted") ||
                    !StaticDestructionDispositions.TryAdd(id, kind))
                    throw new InvalidDataException("Static source obligation has no exact observed disposition");
                using var action = database.Prepare($"SELECT count(*) FROM cleanup_actions WHERE source_static_obligation_id={id}");
                if (!action.Read() || kind != "source-template" && (kind == "cleanup-action") != (action.Integer(0) > 0))
                    throw new InvalidDataException("Static partial destruction action differs from its observed disposition");
            }
        foreach (var obligation in SourceStaticDestructions.Values.Where(value => value.Kind is "persistent" or "partial-aggregate"))
            if (!StaticDestructionDispositions.ContainsKey(obligation.Id))
                throw new InvalidDataException("Original persistent destruction lacks an owned registration or explicit stock elimination");
        foreach (var generated in GeneratedStaticDestructors.Values)
            VerifyGeneratedStaticDestructorBody(generated, ReadBody(generated.FunctionEntityId));
    }

    private void VerifyGeneratedStaticDestructorBody(GeneratedStaticDestructor generated, FunctionBody body)
    {
        if (body.Bindings.Count != 1 || !body.Bindings.ContainsKey(generated.ParameterBindingId))
            throw new InvalidDataException("Generated static destructor contains unobserved local storage");
        var calls = body.Nodes.Values.Where(node => node.Kind == "call").ToArray();
        if (calls.Length != 1 || body.Nodes.Values.Any(node => node.Kind is not
            ("block" or "expr-stmt" or "call" or "convert" or "address" or "array-decay" or "storage" or "storage-address" or "constant" or "function-address" or "return")))
            throw new InvalidDataException("Generated static destructor contains unexplained executable operations");
        Node call = calls[0];
        var arguments = body.Children(call.Id, "argument").Select(edge => body.Nodes[edge.ChildId]).ToArray();
        if (generated.ElementTypeId is not null)
        {
            if (arguments.Length != 4 || !ArrayRuntimeOperations.TryGetValue(call.Id, out var operation) ||
                operation.SourceStaticObligationId != generated.SourceObligationId || operation.OperationKind != "destroy")
                throw new InvalidDataException("Generated static array destructor lacks its exact observed element destruction operation");
            Node count = body.Nodes[StaticOperandLeaf(arguments[1].Id, generated.FunctionEntityId)];
            Node stride = body.Nodes[StaticOperandLeaf(arguments[2].Id, generated.FunctionEntityId)];
            Node callback = body.Nodes[StaticOperandLeaf(arguments[3].Id, generated.FunctionEntityId)];
            if (count.Kind != "constant" || count.ConstantText != generated.ElementCount!.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) ||
                stride.Kind != "constant" || stride.ConstantText != generated.ElementSize!.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) ||
                callback.Kind != "function-address" || callback.EntityId != operation.CallbackEntityId)
                throw new InvalidDataException("Generated static array callback arguments differ from original count, stride or selected destructor");
        }
        else if (arguments.Length != 1 || !NormalStaticDestructions.TryGetValue(call.Id, out long obligation) || obligation != generated.SourceObligationId)
            throw new InvalidDataException("Generated scalar destruction lacks its exact observed callback");
        VerifyStaticObject(arguments[0].Id, generated.FunctionEntityId, generated.StorageId, false);
    }
}
