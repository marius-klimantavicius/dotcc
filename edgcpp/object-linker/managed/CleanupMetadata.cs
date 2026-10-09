namespace DotObj;

internal sealed record SourceLifetime(long Id, long FunctionEntityId, long? ParentLifetimeId,
    long SiblingOrdinal, long? ParentObligationId, string Kind, long? SourceFileId, long Line, long Column, string CaptureProof);
internal sealed record SourceDestructionObligation(long Id, long FunctionEntityId, long LifetimeId,
    long Ordinal, long DestructorEntityId, long SourceTypeId, long? SourceFileId, long Line, long Column, string CaptureProof);
internal sealed record CleanupAction(long Id, long FunctionEntityId, long SourceObligationId,
    long CallbackEntityId, long TargetNodeId, string CaptureProof);
internal sealed record CleanupState(long Id, long FunctionEntityId, long? ActionId, long? NextStateId, long? HandlerNodeId);
internal sealed record CleanupTransition(long NodeId, long? StateId, string CaptureProof, bool IsUnreachable);
internal sealed record CleanupFrameMarker(long NodeId, string Kind, string CaptureProof);

internal sealed partial class ObjectFile
{
    public Dictionary<long, long> ExpressionLifetimes { get; } = [];
    public Dictionary<long, long> CleanupConditions { get; } = [];
    public Dictionary<long, long> NormalDestructions { get; } = [];
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

    private void ReadCleanupMetadata()
    {
        using (var query = database.Prepare("SELECT x.node_id,x.source_base_id,n.kind FROM base_subobject_expressions x LEFT JOIN nodes n ON n.id=x.node_id ORDER BY x.node_id"))
            while (query.Read())
            {
                if (!SourceBases.ContainsKey(query.Integer(1)) || query.OptionalText(2) != "base-subobject" ||
                    !BaseSubobjectExpressions.TryAdd(query.Integer(0), query.Integer(1)))
                    throw new InvalidDataException("Base-subobject expression lacks its exact original base relationship");
            }
        using (var query = database.Prepare("SELECT id,function_entity_id,parent_lifetime_id,sibling_ordinal,parent_obligation_id,kind,source_file_id,line,column,capture_proof FROM source_lifetimes ORDER BY id"))
            while (query.Read())
            {
                var lifetime = new SourceLifetime(query.Integer(0), query.Integer(1), query.OptionalInteger(2), query.Integer(3),
                    query.OptionalInteger(4), query.Text(5), query.OptionalInteger(6), query.Integer(7), query.Integer(8), query.Text(9));
                if (lifetime.Id <= 0 || !EntitySourceRoutines.ContainsKey(lifetime.FunctionEntityId) ||
                    SourceFunctionKinds[lifetime.FunctionEntityId] == "runtime" || lifetime.SiblingOrdinal < 0 ||
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
                    OriginalFunction(function) != lifetime.FunctionEntityId || query.OptionalText(4) != "lifetime-scope" ||
                    query.Text(2) != "observed-lifetime-wrapper" || !ExpressionLifetimes.TryAdd(query.Integer(0), lifetime.Id))
                    throw new InvalidDataException("Expression lifetime boundary lacks its exact original temporary lifetime and function owner");
            }
        using (var query = database.Prepare("SELECT id,function_entity_id,lifetime_id,ordinal,destructor_entity_id,source_type_id,source_file_id,line,column,capture_proof FROM source_destruction_obligations ORDER BY lifetime_id,ordinal"))
            while (query.Read())
            {
                var obligation = new SourceDestructionObligation(query.Integer(0), query.Integer(1), query.Integer(2), query.Integer(3),
                    query.Integer(4), query.Integer(5), query.OptionalInteger(6), query.Integer(7), query.Integer(8), query.Text(9));
                if (obligation.Id <= 0 || !SourceLifetimes.TryGetValue(obligation.LifetimeId, out SourceLifetime? lifetime) ||
                    lifetime.FunctionEntityId != obligation.FunctionEntityId || obligation.Ordinal < 0 ||
                    !SourceFunctionKinds.TryGetValue(obligation.DestructorEntityId, out string? kind) || kind != "destructor" ||
                    Original(SourceTypeById(obligation.SourceTypeId)).Kind != "record" ||
                    obligation.Line < 0 || obligation.Column < 0 || obligation.CaptureProof != "before-lowering" ||
                    !SourceDestructionObligations.TryAdd(obligation.Id, obligation))
                    throw new InvalidDataException("Invalid original destruction obligation or destructor provenance");
                RoutineType destructor = EntitySourceRoutines[obligation.DestructorEntityId];
                if (destructor.ObjectTypeId is not long receiver ||
                    Original(SourceTypeById(receiver)).ExecutionTypeId != Original(SourceTypeById(obligation.SourceTypeId)).ExecutionTypeId)
                    throw new InvalidDataException("Source destruction type differs from its original destructor receiver");
            }
        foreach (var group in SourceLifetimes.Values.GroupBy(value => value.FunctionEntityId))
        {
            SourceLifetime[] roots = group.Where(value => value.ParentLifetimeId is null).ToArray();
            if (roots.Length != 1 || roots[0].Kind != "block" || roots[0].SiblingOrdinal != 0 || roots[0].ParentObligationId is not null)
                throw new InvalidDataException("Original function lifetimes require exactly one block root");
        }
        foreach (var group in SourceDestructionObligations.Values.GroupBy(value => value.LifetimeId))
            if (group.OrderBy(value => value.Ordinal).Where((value, ordinal) => value.Ordinal != ordinal).Any())
                throw new InvalidDataException("Original destruction obligation order must be dense");
        foreach (var group in SourceLifetimes.Values.GroupBy(value => (value.FunctionEntityId, value.ParentLifetimeId)))
            if (group.OrderBy(value => value.SiblingOrdinal).Where((value, ordinal) => value.SiblingOrdinal != ordinal).Any())
                throw new InvalidDataException("Original lifetime sibling order must be dense");
        foreach (SourceLifetime lifetime in SourceLifetimes.Values)
        {
            var seen = new HashSet<long>();
            for (SourceLifetime? current = lifetime; current is not null;)
            {
                if (!seen.Add(current.Id)) throw new InvalidDataException("Cycle in original lifetime parents");
                if (current.ParentLifetimeId is not long parent) break;
                if (!SourceLifetimes.TryGetValue(parent, out current) || current.FunctionEntityId != lifetime.FunctionEntityId)
                    throw new InvalidDataException("Original lifetime parent leaves its source function");
            }
            if (lifetime.ParentObligationId is long obligation &&
                (!SourceDestructionObligations.TryGetValue(obligation, out SourceDestructionObligation? parentObligation) ||
                 parentObligation.LifetimeId != lifetime.ParentLifetimeId || lifetime.Kind == "block-after-label"))
                throw new InvalidDataException("Original lifetime overlap must reference its parent's destruction sublist");
        }
        using (var query = database.Prepare("SELECT a.id,a.function_entity_id,a.source_obligation_id,a.callback_entity_id,a.target_node_id,a.capture_proof,n.function_id FROM cleanup_actions a LEFT JOIN nodes n ON n.id=a.target_node_id ORDER BY a.id"))
            while (query.Read())
            {
                var action = new CleanupAction(query.Integer(0), query.Integer(1), query.Integer(2), query.Integer(3), query.Integer(4), query.Text(5));
                if (action.Id <= 0 || !Functions.ContainsKey(action.FunctionEntityId) || query.OptionalInteger(6) != action.FunctionEntityId ||
                    !SourceDestructionObligations.TryGetValue(action.SourceObligationId, out SourceDestructionObligation? obligation) ||
                    OriginalFunction(action.FunctionEntityId) != obligation.FunctionEntityId ||
                    action.CaptureProof != "observed-region-entry" || !Entities.TryGetValue(action.CallbackEntityId, out Entity? callback) ||
                    OriginalFunction(callback.Id) != obligation.DestructorEntityId ||
                    AbiRoutineOrigins.TryGetValue(callback.Id, out AbiRoutineOrigin? origin) && origin.EntryKind is not ("complete" or "subobject") ||
                    !CleanupActions.TryAdd(action.Id, action))
                    throw new InvalidDataException("Cleanup action lacks its exact original obligation, function or destructor origin");
                var parameters = Parameters.GetValueOrDefault(callback.Id, []);
                if (Type(callback.ReturnTypeId) != "void" || Routines[callback.RoutineTypeId].Variadic ||
                    parameters.Count != 1 || !ScalarTypes.IsPointer(parameters[0]))
                    throw new InvalidDataException("Cleanup destructor requires the supported void(one object pointer) ABI");
                if (!FunctionCleanupActions.TryGetValue(action.FunctionEntityId, out var actions))
                    FunctionCleanupActions.Add(action.FunctionEntityId, actions = []);
                actions.Add(action);
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
        using (var query = database.Prepare("SELECT d.node_id,d.source_obligation_id,d.capture_proof,n.function_id,n.kind,n.entity_id FROM normal_destructions d LEFT JOIN nodes n ON n.id=d.node_id ORDER BY d.node_id"))
            while (query.Read())
            {
                if (!SourceDestructionObligations.TryGetValue(query.Integer(1), out SourceDestructionObligation? obligation) ||
                    query.Text(2) != "observed-normal-destruction" || query.OptionalInteger(3) is not long function ||
                    !Functions.ContainsKey(function) || OriginalFunction(function) != obligation.FunctionEntityId ||
                    query.OptionalText(4) != "call" || query.OptionalInteger(5) is not long callback ||
                    !Entities.ContainsKey(callback) || OriginalFunction(callback) != obligation.DestructorEntityId ||
                    AbiRoutineOrigins.TryGetValue(callback, out AbiRoutineOrigin? origin) && origin.EntryKind is not ("complete" or "subobject") ||
                    !NormalDestructions.TryAdd(query.Integer(0), obligation.Id))
                    throw new InvalidDataException("Normal destruction lacks its exact owned call, source obligation or destructor origin");
            }
        using (var query = database.Prepare("SELECT s.id,s.function_entity_id,s.action_id,s.next_state_id,s.handler_node_id,n.function_id,n.kind FROM cleanup_states s LEFT JOIN nodes n ON n.id=s.handler_node_id ORDER BY s.id"))
            while (query.Read())
            {
                var state = new CleanupState(query.Integer(0), query.Integer(1), query.OptionalInteger(2), query.OptionalInteger(3), query.OptionalInteger(4));
                if (state.Id <= 0 || (state.ActionId is null) == (state.HandlerNodeId is null) ||
                    (state.ActionId is long actionId ? !CleanupActions.TryGetValue(actionId, out CleanupAction? action) || action.FunctionEntityId != state.FunctionEntityId
                        : query.OptionalInteger(5) != state.FunctionEntityId || query.OptionalText(6) != "catch") ||
                    !CleanupStates.TryAdd(state.Id, state))
                    throw new InvalidDataException("Cleanup state must reference its own function's action");
                if (!FunctionCleanupStates.TryGetValue(state.FunctionEntityId, out var states))
                    FunctionCleanupStates.Add(state.FunctionEntityId, states = []);
                states.Add(state);
            }
        if (CleanupActions.Keys.Except(CleanupStates.Values.Where(state => state.ActionId is not null).Select(state => state.ActionId!.Value)).Any())
            throw new InvalidDataException("Cleanup action is missing its observed region state");
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
