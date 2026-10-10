namespace DotObj;

internal sealed record SourceLifetime(long Id, long? FunctionEntityId, long? ParentLifetimeId,
    long SiblingOrdinal, long? ParentObligationId, long? ParentAllocationObligationId, string Kind, long? SourceFileId, long Line, long Column, string CaptureProof, long? SourceInitializerId);
internal sealed record SourceDestructionObligation(long Id, long? FunctionEntityId, long LifetimeId,
    long Ordinal, long DestructorEntityId, long SourceTypeId, long? SourceFileId, long Line, long Column, string CaptureProof, long? SourceInitializerId, long? SourceStaticObligationId, bool IsPartialAggregate);
internal sealed record CleanupAction(long Id, long FunctionEntityId, long? SourceObligationId,
    long CallbackEntityId, long TargetNodeId, string CaptureProof, long? SourceStaticObligationId, long? SourceFieldInitializerId);
internal sealed record ArrayRuntimeOperation(long NodeId, long? SourceObligationId, long CallbackEntityId, string OperationKind, string CaptureProof, long? SourceStaticObligationId, long? SourceFieldInitializerId);
internal sealed record CleanupArray(long ActionId, long ElementTypeId, long ElementCount, long ElementSize, string CaptureProof);
internal sealed record CleanupState(long Id, long FunctionEntityId, long? ActionId, long? NextStateId, long? HandlerNodeId, long? AllocationActionId, long? GuardActionId);
internal sealed record CleanupTransition(long NodeId, long? StateId, string CaptureProof, bool IsUnreachable);
internal sealed record CleanupFrameMarker(long NodeId, string Kind, string CaptureProof);

internal sealed partial class ObjectFile
{
    public HashSet<long> InternalTryExpressions { get; } = [];
    public Dictionary<long, long> ExpressionLifetimes { get; } = [];
    public Dictionary<long, ArrayRuntimeOperation> ArrayRuntimeOperations { get; } = [];
    public Dictionary<long, CleanupArray> CleanupArrays { get; } = [];
    public Dictionary<long, long> CleanupVttArguments { get; } = [];
    public Dictionary<long, long> CleanupConditions { get; } = [];
    public Dictionary<long, long> NormalDestructions { get; } = [];
    public Dictionary<long, long> NormalStaticDestructions { get; } = [];
    public Dictionary<long, long> NormalFieldDestructions { get; } = [];
    public Dictionary<long, long> VirtualBaseExpressions { get; } = [];
    public Dictionary<long, long> BaseSubobjectExpressions { get; } = [];
    public Dictionary<long, SourceLifetime> SourceLifetimes { get; } = [];
    public Dictionary<long, SourceDestructionObligation> SourceDestructionObligations { get; } = [];
    public Dictionary<long, CleanupAction> CleanupActions { get; } = [];
    public Dictionary<long, CleanupState> CleanupStates { get; } = [];
    public Dictionary<long, CleanupTransition> CleanupTransitions { get; } = [];
    public Dictionary<long, CleanupFrameMarker> CleanupFrameMarkers { get; } = [];
    public Dictionary<long, List<CleanupAction>> FunctionCleanupActions { get; } = [];
    public Dictionary<long, List<CleanupState>> FunctionCleanupStates { get; } = [];

    public long OriginalFunction(long entityId) => AbiRoutineOrigins.TryGetValue(entityId, out AbiRoutineOrigin? origin)
        ? origin.PrimaryEntityId : entityId;

    private SourceType DestructionElementType(long sourceTypeId)
    {
        SourceType type = Original(SourceTypeById(sourceTypeId));
        while (type.Kind == "array") type = Original(SourceTypeById(type.UnderlyingTypeId!.Value));
        return type;
    }

    public long CleanupSourceType(CleanupAction action) => action.SourceObligationId is long automatic
        ? SourceDestructionObligations[automatic].SourceTypeId
        : action.SourceStaticObligationId is long staticId ? SourceStaticDestructions[staticId].SourceTypeId
        : FieldInitializerDestructions[action.SourceFieldInitializerId!.Value].SourceTypeId;

    public long StaticCleanupStorage(CleanupAction action)
        => SourceInitializers[SourceStaticDestructions[action.SourceStaticObligationId!.Value].SourceInitializerId!.Value].StorageId!.Value;

    private bool IsStaticInitializerExecution(SourceInitializer initializer, long functionId)
        => initializer.ExecutionDisposition == "emitted" && (initializer.FunctionEntityId is long original
            ? OriginalFunction(functionId) == original
            : DynamicInitializers.Values.Any(value => value.SourceInitializerId == initializer.Id &&
                InitializationActions.Any(action => action.Id == value.ActionId && action.EntityId == functionId)));

    private void ReadCleanupMetadata()
    {
        using (var query = database.Prepare("SELECT x.node_id,x.cleanup_policy,x.capture_proof,n.kind,n.function_id FROM internal_try_expressions x LEFT JOIN nodes n ON n.id=x.node_id ORDER BY x.node_id"))
            while (query.Read())
                if (SourceLanguage != "c++" || query.Text(1) != "unwind-to-entry-before-handler" ||
                    query.Text(2) != "observed-internal-try" || query.OptionalText(3) != "internal-try-expression" ||
                    query.OptionalInteger(4) is not long owner || !Functions.ContainsKey(owner) ||
                    !InternalTryExpressions.Add(query.Integer(0)))
                    throw new InvalidDataException("Internal try lacks its exact observed expression, function and cleanup policy");
        using (var query = database.Prepare("SELECT x.node_id,x.source_base_id,n.kind FROM base_subobject_expressions x LEFT JOIN nodes n ON n.id=x.node_id ORDER BY x.node_id"))
            while (query.Read())
            {
                if (!SourceBases.ContainsKey(query.Integer(1)) || query.OptionalText(2) != "base-subobject" ||
                    !BaseSubobjectExpressions.TryAdd(query.Integer(0), query.Integer(1)))
                    throw new InvalidDataException("Base-subobject expression lacks its exact original base relationship");
            }
        using (var query = database.Prepare("SELECT x.node_id,x.complete_base_type_id,n.kind FROM virtual_base_expressions x LEFT JOIN nodes n ON n.id=x.node_id ORDER BY x.node_id"))
            while (query.Read())
                if (!Records.ContainsKey(query.Integer(1)) || query.OptionalText(2) != "virtual-base-subobject" ||
                    !VirtualBaseExpressions.TryAdd(query.Integer(0), query.Integer(1)))
                    throw new InvalidDataException("Virtual-base expression lacks its observed complete base type");
        using (var query = database.Prepare("SELECT id,function_entity_id,parent_lifetime_id,sibling_ordinal,parent_obligation_id,parent_allocation_obligation_id,kind,source_file_id,line,column,capture_proof,source_initializer_id FROM source_lifetimes ORDER BY id"))
            while (query.Read())
            {
                var lifetime = new SourceLifetime(query.Integer(0), query.OptionalInteger(1), query.OptionalInteger(2), query.Integer(3),
                    query.OptionalInteger(4), query.OptionalInteger(5), query.Text(6), query.OptionalInteger(7), query.Integer(8), query.Integer(9), query.Text(10), query.OptionalInteger(11));
                if (lifetime.Id <= 0 || (lifetime.FunctionEntityId is null) == (lifetime.SourceInitializerId is null) ||
                    lifetime.FunctionEntityId is long sourceFunction && (!EntitySourceRoutines.ContainsKey(sourceFunction) || SourceFunctionKinds[sourceFunction] == "runtime") ||
                    lifetime.SourceInitializerId is long sourceInitializer && !SourceInitializers.ContainsKey(sourceInitializer) || lifetime.SiblingOrdinal < 0 ||
                    lifetime.Kind is not ("block" or "block-after-label" or "expression-temporary" or "try-block") ||
                    lifetime.Line < 0 || lifetime.Column < 0 || lifetime.CaptureProof != "before-lowering" ||
                    !SourceLifetimes.TryAdd(lifetime.Id, lifetime))
                    throw new InvalidDataException("Invalid original source lifetime owner, kind, order or capture");
            }
        using (var query = database.Prepare("SELECT x.node_id,x.source_lifetime_id,x.capture_proof,n.function_id,n.kind FROM expression_lifetimes x LEFT JOIN nodes n ON n.id=x.node_id ORDER BY x.node_id"))
            while (query.Read())
            {
                if (!SourceLifetimes.TryGetValue(query.Integer(1), out SourceLifetime? lifetime) ||
                    lifetime.Kind != "expression-temporary" || query.OptionalInteger(3) is not long function ||
                    !IsSourceLifetimeExecution(lifetime, function) || query.OptionalText(4) != "lifetime-scope" ||
                    query.Text(2) != "observed-lifetime-wrapper" || !ExpressionLifetimes.TryAdd(query.Integer(0), lifetime.Id))
                    throw new InvalidDataException("Expression lifetime boundary lacks its exact original temporary lifetime and function owner");
            }
        using (var query = database.Prepare("SELECT id,function_entity_id,lifetime_id,ordinal,destructor_entity_id,source_type_id,source_file_id,line,column,capture_proof,source_initializer_id,source_static_obligation_id,is_partial_aggregate FROM source_destruction_obligations ORDER BY lifetime_id,ordinal"))
            while (query.Read())
            {
                var obligation = new SourceDestructionObligation(query.Integer(0), query.OptionalInteger(1), query.Integer(2), query.Integer(3),
                    query.Integer(4), query.Integer(5), query.OptionalInteger(6), query.Integer(7), query.Integer(8), query.Text(9), query.OptionalInteger(10), query.OptionalInteger(11), query.Integer(12) != 0);
                if (obligation.Id <= 0 || !SourceLifetimes.TryGetValue(obligation.LifetimeId, out SourceLifetime? lifetime) ||
                    lifetime.FunctionEntityId != obligation.FunctionEntityId || lifetime.SourceInitializerId != obligation.SourceInitializerId || obligation.Ordinal < 0 ||
                    !SourceFunctionKinds.TryGetValue(obligation.DestructorEntityId, out string? kind) || kind != "destructor" ||
                    DestructionElementType(obligation.SourceTypeId).Kind != "record" ||
                    obligation.Line < 0 || obligation.Column < 0 || obligation.CaptureProof != "before-lowering" ||
                    !SourceDestructionObligations.TryAdd(obligation.Id, obligation))
                    throw new InvalidDataException("Invalid original destruction obligation or destructor provenance");
                RoutineType destructor = EntitySourceRoutines[obligation.DestructorEntityId];
                if (destructor.ObjectTypeId is not long receiver ||
                    Original(SourceTypeById(receiver)).ExecutionTypeId != DestructionElementType(obligation.SourceTypeId).ExecutionTypeId)
                    throw new InvalidDataException("Source destruction type differs from its original destructor receiver");
            }
        ReadAllocationObligations();
        ReadGuardMetadata();
        foreach (var group in SourceLifetimes.Values.Where(value => value.FunctionEntityId is not null).GroupBy(value => value.FunctionEntityId))
        {
            SourceLifetime[] roots = group.Where(value => value.ParentLifetimeId is null).ToArray();
            if (roots.Length != 1 || roots[0].Kind != "block" || roots[0].SiblingOrdinal != 0 || roots[0].ParentObligationId is not null || roots[0].ParentAllocationObligationId is not null)
                throw new InvalidDataException("Original function lifetimes require exactly one block root");
        }
        foreach (var group in SourceDestructionObligations.Values.Select(value => (value.LifetimeId, value.Ordinal))
            .Concat(SourceAllocationObligations.Values.Select(value => (value.LifetimeId, value.Ordinal))).GroupBy(value => value.LifetimeId))
            if (group.OrderBy(value => value.Ordinal).Where((value, ordinal) => value.Ordinal != ordinal).Any())
                throw new InvalidDataException("Original destruction obligation order must be dense");
        foreach (var group in SourceLifetimes.Values.GroupBy(value => (value.FunctionEntityId, value.SourceInitializerId, value.ParentLifetimeId)))
            if (group.OrderBy(value => value.SiblingOrdinal).Where((value, ordinal) => value.SiblingOrdinal != ordinal).Any())
                throw new InvalidDataException("Original lifetime sibling order must be dense");
        foreach (SourceLifetime lifetime in SourceLifetimes.Values)
        {
            var seen = new HashSet<long>();
            for (SourceLifetime? current = lifetime; current is not null;)
            {
                if (!seen.Add(current.Id)) throw new InvalidDataException("Cycle in original lifetime parents");
                if (current.ParentLifetimeId is not long parent) break;
                if (!SourceLifetimes.TryGetValue(parent, out current) || (current.FunctionEntityId != lifetime.FunctionEntityId || current.SourceInitializerId != lifetime.SourceInitializerId))
                    throw new InvalidDataException("Original lifetime parent leaves its source function");
            }
            if (lifetime.ParentAllocationObligationId is long allocation &&
                (lifetime.ParentObligationId is not null || !SourceAllocationObligations.TryGetValue(allocation, out SourceAllocationObligation? allocationParent) ||
                 allocationParent.LifetimeId != lifetime.ParentLifetimeId || lifetime.Kind == "block-after-label"))
                throw new InvalidDataException("Original lifetime allocation overlap must reference its parent's allocation sublist");
            if (lifetime.ParentObligationId is long obligation &&
                (!SourceDestructionObligations.TryGetValue(obligation, out SourceDestructionObligation? parentObligation) ||
                 parentObligation.LifetimeId != lifetime.ParentLifetimeId || lifetime.Kind == "block-after-label"))
                throw new InvalidDataException("Original lifetime overlap must reference its parent's destruction sublist");
        }
        using (var query = database.Prepare("SELECT a.id,a.function_entity_id,a.source_obligation_id,a.callback_entity_id,a.target_node_id,a.capture_proof,n.function_id,a.source_static_obligation_id,a.source_field_initializer_id FROM cleanup_actions a LEFT JOIN nodes n ON n.id=a.target_node_id ORDER BY a.id"))
            while (query.Read())
            {
                var action = new CleanupAction(query.Integer(0), query.Integer(1), query.OptionalInteger(2), query.Integer(3), query.Integer(4), query.Text(5), query.OptionalInteger(7), query.OptionalInteger(8));
                if (action.Id <= 0 || !Functions.ContainsKey(action.FunctionEntityId) || query.OptionalInteger(6) != action.FunctionEntityId ||
                    new long?[] { action.SourceObligationId, action.SourceStaticObligationId, action.SourceFieldInitializerId }.Count(id => id is not null) != 1 ||
                    action.CaptureProof != "observed-region-entry" || !Entities.TryGetValue(action.CallbackEntityId, out Entity? callback) ||
                    AbiRoutineOrigins.TryGetValue(callback.Id, out AbiRoutineOrigin? origin) && origin.EntryKind is not ("complete" or "subobject" or "delegation") ||
                    !CleanupActions.TryAdd(action.Id, action))
                    throw new InvalidDataException("Cleanup action lacks its exact original obligation, function or destructor origin");
                if (action.SourceObligationId is long automatic)
                {
                    if (!SourceDestructionObligations.TryGetValue(automatic, out var obligation) ||
                        !IsSourceLifetimeExecution(SourceLifetimes[obligation.LifetimeId], action.FunctionEntityId) ||
                        OriginalFunction(callback.Id) != obligation.DestructorEntityId)
                        throw new InvalidDataException("Cleanup action differs from its original automatic destruction owner");
                }
                else if (action.SourceFieldInitializerId is long fieldInitializer)
                    VerifyFieldInitializerDestructionUse(fieldInitializer, action.FunctionEntityId, callback.Id, "cleanup");
                else
                {
                    if (!SourceStaticDestructions.TryGetValue(action.SourceStaticObligationId!.Value, out var obligation) ||
                        obligation.Kind != "partial-aggregate" || obligation.SourceInitializerId is not long initializer ||
                        !SourceInitializers.TryGetValue(initializer, out var sourceInitializer) ||
                        OriginalFunction(callback.Id) != obligation.SelectedRoutineEntityId ||
                        AbiRoutineOrigins.TryGetValue(callback.Id, out var staticOrigin) && staticOrigin.EntryKind != "complete")
                        throw new InvalidDataException("Source template rollback lacks its exact initializer and selected destructor");
                    if (sourceInitializer.Kind == "field-default")
                        VerifyFieldInitializerPartialDestructionUse(obligation.Id, action.FunctionEntityId, callback.Id, "cleanup");
                    else if (sourceInitializer.StorageId is null || Original(SourceTypeById(sourceInitializer.SourceTypeId)).Kind != "array" ||
                        !IsStaticInitializerExecution(sourceInitializer, action.FunctionEntityId))
                        throw new InvalidDataException("Static array rollback lacks its exact initializer execution and storage");
                }
                if (!FunctionCleanupActions.TryGetValue(action.FunctionEntityId, out var actions))
                    FunctionCleanupActions.Add(action.FunctionEntityId, actions = []);
                actions.Add(action);
            }
        using (var query = database.Prepare("SELECT a.node_id,a.source_obligation_id,a.callback_entity_id,a.operation_kind,a.capture_proof,n.kind,n.function_id,n.entity_id,a.source_static_obligation_id,a.source_field_initializer_id FROM array_runtime_operations a LEFT JOIN nodes n ON n.id=a.node_id ORDER BY a.node_id"))
            while (query.Read())
            {
                var operation = new ArrayRuntimeOperation(query.Integer(0), query.OptionalInteger(1), query.Integer(2), query.Text(3), query.Text(4), query.OptionalInteger(8), query.OptionalInteger(9));
                if (new long?[] { operation.SourceObligationId, operation.SourceStaticObligationId, operation.SourceFieldInitializerId }.Count(id => id is not null) != 1 ||
                    !Entities.ContainsKey(operation.CallbackEntityId) ||
                    operation.OperationKind is not ("construct" or "destroy") || operation.CaptureProof != "observed-array-runtime" ||
                    query.OptionalText(5) != "call" || query.OptionalInteger(6) is not long owner ||
                    query.OptionalInteger(7) is not long helper || !Entities.TryGetValue(helper, out Entity? runtime) ||
                    RuntimeArrayProvider.TryGetMethod(new Symbol(this, runtime)) is not string method ||
                    (operation.OperationKind == "construct" ? method is not ("Construct" or "CopyConstruct") : method is not ("Destroy" or "Cleanup")) ||
                    !ArrayRuntimeOperations.TryAdd(operation.NodeId, operation))
                    throw new InvalidDataException("Array runtime operation lacks its exact source obligation, destructor or runtime helper origin");
                if (operation.SourceObligationId is long sourceObligation)
                {
                    if (!SourceDestructionObligations.TryGetValue(sourceObligation, out SourceDestructionObligation? obligation) ||
                        OriginalFunction(operation.CallbackEntityId) != obligation.DestructorEntityId ||
                        !IsSourceLifetimeExecution(SourceLifetimes[obligation.LifetimeId], owner))
                        throw new InvalidDataException("Array runtime operation differs from its original automatic destruction owner");
                }
                else if (operation.SourceFieldInitializerId is long fieldInitializer)
                    VerifyFieldInitializerDestructionUse(fieldInitializer, owner, operation.CallbackEntityId, operation.OperationKind);
                else VerifyStaticDestructionUse(operation.SourceStaticObligationId!.Value, owner, operation.CallbackEntityId, operation.OperationKind);
                if (AbiRoutineOrigins.TryGetValue(operation.CallbackEntityId, out AbiRoutineOrigin? origin) && origin.EntryKind != "complete")
                    throw new InvalidDataException("Array runtime operation requires a complete-object destructor callback");
            }
        using (var query = database.Prepare("SELECT action_id,element_type_id,element_count,element_size,capture_proof FROM cleanup_arrays ORDER BY action_id"))
            while (query.Read())
            {
                var array = new CleanupArray(query.Integer(0), query.Integer(1), query.Integer(2), query.Integer(3), query.Text(4));
                if (!CleanupActions.TryGetValue(array.ActionId, out CleanupAction? action) ||
                    AbiRoutineOrigins.TryGetValue(action.CallbackEntityId, out AbiRoutineOrigin? arrayOrigin) && arrayOrigin.EntryKind != "complete" ||
                    !Types.ContainsKey(array.ElementTypeId) || !ScalarTypes.IsRecord(Type(array.ElementTypeId)) ||
                    array.ElementCount <= 0 || array.ElementSize != StorageSize(Type(array.ElementTypeId)) ||
                    array.ElementSize <= 0 || array.ElementCount > long.MaxValue / array.ElementSize ||
                    DestructionElementType(CleanupSourceType(action)).ExecutionTypeId != array.ElementTypeId ||
                    array.CaptureProof != "observed-array-region" || !CleanupArrays.TryAdd(array.ActionId, array))
                    throw new InvalidDataException("Array cleanup requires its exact complete leaf type, positive count and stride");
                SourceType source = Original(SourceTypeById(CleanupSourceType(action)));
                if (source.Kind == "array")
                {
                    long count = 1;
                    while (source.Kind == "array")
                    {
                        if (source.ElementCount is not long bound || bound <= 0 || count > long.MaxValue / bound)
                            throw new InvalidDataException("Array cleanup requires fixed original array bounds");
                        count *= bound; source = Original(SourceTypeById(source.UnderlyingTypeId!.Value));
                    }
                    if (array.ElementCount > count) throw new InvalidDataException("Array cleanup exceeds its original array declaration");
                }
            }
        using (var query = database.Prepare("SELECT v.action_id,v.binding_id,v.capture_proof,b.function_id,b.type_id FROM cleanup_vtt_arguments v LEFT JOIN bindings b ON b.id=v.binding_id ORDER BY v.action_id"))
            while (query.Read())
            {
                if (!CleanupActions.TryGetValue(query.Integer(0), out CleanupAction? action) ||
                    CleanupArrays.ContainsKey(action.Id) || query.Text(2) != "observed-subobject-vtt" ||
                    query.OptionalInteger(3) != action.FunctionEntityId || query.OptionalInteger(4) is not long type ||
                    !ScalarTypes.IsPointer(Type(type)) || !CleanupVttArguments.TryAdd(action.Id, query.Integer(1)))
                    throw new InvalidDataException("Subobject VTT cleanup argument requires its exact owned pointer binding");
                var parameters = Parameters.GetValueOrDefault(action.CallbackEntityId, []);
                if (parameters.Count != 2 || parameters[1] != Type(type))
                    throw new InvalidDataException("Subobject VTT argument differs from the selected destructor ABI");
            }
        foreach (CleanupAction action in CleanupActions.Values)
        {
            Entity callback = Entities[action.CallbackEntityId];
            var parameters = Parameters.GetValueOrDefault(callback.Id, []);
            if (Type(callback.ReturnTypeId) != "void" || Routines[callback.RoutineTypeId].Variadic ||
                parameters.Count != (CleanupVttArguments.ContainsKey(action.Id) ? 2 : 1) ||
                !ScalarTypes.IsPointer(parameters[0]))
                throw new InvalidDataException("Cleanup destructor requires its owned object-pointer and optional VTT-pointer ABI");
            if (CleanupArrays.TryGetValue(action.Id, out CleanupArray? array) &&
                parameters[0] != "ptr(" + Type(array.ElementTypeId) + ")")
                throw new InvalidDataException("Array destructor pointer differs from its complete leaf element type");
        }
        using (var query = database.Prepare("SELECT c.action_id,c.flag_binding_id,c.capture_proof,b.function_id,b.kind,b.type_id,b.is_synthetic FROM cleanup_conditions c LEFT JOIN bindings b ON b.id=c.flag_binding_id ORDER BY c.action_id"))
            while (query.Read())
            {
                if (!CleanupActions.TryGetValue(query.Integer(0), out CleanupAction? action) ||
                    query.Text(2) != "observed-conditional-cleanup-flag" || query.OptionalInteger(3) != action.FunctionEntityId ||
                    query.OptionalText(4) != "local" || query.OptionalInteger(5) is not long flagType || Type(flagType) != "i32" ||
                    query.Integer(6) != 1 || BindingSourceTypes.ContainsKey(query.Integer(1)) ||
                    !CleanupConditions.TryAdd(action.Id, query.Integer(1)))
                    throw new InvalidDataException("Conditional cleanup requires its exact synthetic local flag and owning action");
            }
        using (var query = database.Prepare("SELECT d.node_id,d.source_obligation_id,d.capture_proof,n.function_id,n.kind,n.entity_id,d.source_static_obligation_id,d.source_field_initializer_id FROM normal_destructions d LEFT JOIN nodes n ON n.id=d.node_id ORDER BY d.node_id"))
            while (query.Read())
            {
                long? sourceObligation = query.OptionalInteger(1), staticObligation = query.OptionalInteger(6), fieldInitializer = query.OptionalInteger(7);
                if (new long?[] { sourceObligation, staticObligation, fieldInitializer }.Count(id => id is not null) != 1 ||
                    query.Text(2) != "observed-normal-destruction" || query.OptionalInteger(3) is not long function ||
                    !Functions.ContainsKey(function) || query.OptionalText(4) != "call" || query.OptionalInteger(5) is not long callback ||
                    !Entities.ContainsKey(callback) ||
                    AbiRoutineOrigins.TryGetValue(callback, out AbiRoutineOrigin? origin) && origin.EntryKind is not ("complete" or "subobject" or "delegation"))
                    throw new InvalidDataException("Normal destruction lacks its exact owned call, source obligation or destructor origin");
                if (sourceObligation is long original)
                {
                    if (!SourceDestructionObligations.TryGetValue(original, out SourceDestructionObligation? obligation) ||
                        !IsSourceLifetimeExecution(SourceLifetimes[obligation.LifetimeId], function) ||
                        OriginalFunction(callback) != obligation.DestructorEntityId || !NormalDestructions.TryAdd(query.Integer(0), original))
                        throw new InvalidDataException("Normal destruction differs from its original automatic obligation");
                }
                else if (fieldInitializer is long fieldRoot)
                {
                    VerifyFieldInitializerDestructionUse(fieldRoot, function, callback, "destroy");
                    if (!NormalFieldDestructions.TryAdd(query.Integer(0), fieldRoot))
                        throw new InvalidDataException("Duplicate field initializer destruction operation");
                }
                else
                {
                    VerifyStaticDestructionUse(staticObligation!.Value, function, callback, "destroy");
                    if (!NormalStaticDestructions.TryAdd(query.Integer(0), staticObligation.Value))
                        throw new InvalidDataException("Duplicate static destruction operation");
                }
            }
        using (var query = database.Prepare("SELECT s.id,s.function_entity_id,s.action_id,s.next_state_id,s.handler_node_id,n.function_id,n.kind,s.allocation_action_id,s.guard_action_id FROM cleanup_states s LEFT JOIN nodes n ON n.id=s.handler_node_id ORDER BY s.id"))
            while (query.Read())
            {
                var state = new CleanupState(query.Integer(0), query.Integer(1), query.OptionalInteger(2), query.OptionalInteger(3), query.OptionalInteger(4), query.OptionalInteger(7), query.OptionalInteger(8));
                if (state.Id <= 0 || new long?[] { state.ActionId, state.HandlerNodeId, state.AllocationActionId, state.GuardActionId }.Count(id => id is not null) != 1 ||
                    state.ActionId is long actionId && (!CleanupActions.TryGetValue(actionId, out CleanupAction? action) || action.FunctionEntityId != state.FunctionEntityId) ||
                    state.HandlerNodeId is not null && (query.OptionalInteger(5) != state.FunctionEntityId || query.OptionalText(6) != "catch") ||
                    state.AllocationActionId is long allocationId && (!AllocationCleanupActions.TryGetValue(allocationId, out AllocationCleanupAction? allocationAction) || allocationAction.FunctionEntityId != state.FunctionEntityId) ||
                    state.GuardActionId is long guardId && (!GuardCleanupActions.TryGetValue(guardId, out GuardCleanupAction? guardAction) || guardAction.FunctionEntityId != state.FunctionEntityId) ||
                    !CleanupStates.TryAdd(state.Id, state))
                    throw new InvalidDataException("Cleanup state must reference its own function's action");
                if (!FunctionCleanupStates.TryGetValue(state.FunctionEntityId, out var states))
                    FunctionCleanupStates.Add(state.FunctionEntityId, states = []);
                states.Add(state);
            }
        if (CleanupActions.Keys.Except(CleanupStates.Values.Where(state => state.ActionId is not null).Select(state => state.ActionId!.Value)).Any())
            throw new InvalidDataException("Cleanup action is missing its observed region state");
        if (AllocationCleanupActions.Keys.Except(CleanupStates.Values.Where(state => state.AllocationActionId is not null).Select(state => state.AllocationActionId!.Value)).Any() ||
            GuardCleanupActions.Keys.Except(CleanupStates.Values.Where(state => state.GuardActionId is not null).Select(state => state.GuardActionId!.Value)).Any())
            throw new InvalidDataException("Rollback action is missing its observed region state");
        foreach (CleanupState state in CleanupStates.Values)
        {
            var seen = new HashSet<long>();
            for (CleanupState? current = state; current is not null;)
            {
                if (!seen.Add(current.Id)) throw new InvalidDataException("Cycle in cleanup state successor chain");
                if (current.NextStateId is not long next) break;
                if (!CleanupStates.TryGetValue(next, out current) || current.FunctionEntityId != state.FunctionEntityId)
                    throw new InvalidDataException("Cleanup successor leaves its function");
            }
        }
        using (var query = database.Prepare("SELECT t.node_id,t.state_id,t.capture_proof,t.is_unreachable,n.function_id,n.kind FROM cleanup_transitions t LEFT JOIN nodes n ON n.id=t.node_id ORDER BY t.node_id"))
            while (query.Read())
            {
                var transition = new CleanupTransition(query.Integer(0), query.OptionalInteger(1), query.Text(2), query.Integer(3) == 1);
                if (query.Integer(3) is not (0 or 1) || transition.CaptureProof != "observed-cleanup-state" ||
                    query.OptionalText(5) != "cleanup-transition" || query.OptionalInteger(4) is not long function ||
                    transition.StateId is long id && (!CleanupStates.TryGetValue(id, out CleanupState? state) || state.FunctionEntityId != function) ||
                    !CleanupTransitions.TryAdd(transition.NodeId, transition))
                    throw new InvalidDataException("Invalid observed cleanup transition or state owner");
            }
        var prologues = new Dictionary<long, int>();
        using (var query = database.Prepare("SELECT m.node_id,m.kind,m.capture_proof,n.function_id,n.kind FROM cleanup_frame_markers m LEFT JOIN nodes n ON n.id=m.node_id ORDER BY m.node_id"))
            while (query.Read())
            {
                var marker = new CleanupFrameMarker(query.Integer(0), query.Text(1), query.Text(2));
                if (marker.Kind is not ("prologue" or "epilogue") || marker.CaptureProof != "observed-cleanup-frame" ||
                    query.OptionalText(4) != "cleanup-frame" || query.OptionalInteger(3) is not long function ||
                    !CleanupFrameMarkers.TryAdd(marker.NodeId, marker))
                    throw new InvalidDataException("Invalid observed cleanup frame marker");
                if (marker.Kind == "prologue") prologues[function] = prologues.GetValueOrDefault(function) + 1;
            }
        if (FunctionCleanupActions.Keys.Any(function => prologues.GetValueOrDefault(function) != 1) || prologues.Values.Any(count => count != 1))
            throw new InvalidDataException("Owned cleanup function requires exactly one observed frame prologue");
    }
}
