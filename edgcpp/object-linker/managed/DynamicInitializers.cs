namespace DotObj;

internal sealed record GeneratedRoutineOrigin(long EntityId, string Kind, string CaptureProof, string FailurePolicy);
internal sealed record DynamicInitializer(long Id, long ActionId, long StorageId, long SourceOrdinal,
    long Ordinal, string CaptureProof, long? SourceFileId, long Line, long Column, long SourceInitializerId);
internal sealed record GeneratedRoutineScaffold(long EntityId, long Ordinal, long NodeId, long? BindingId, string Kind, string CaptureProof);
internal sealed record DynamicInitializerStatement(long InitializerId, long Ordinal, long NodeId);

internal sealed partial class ObjectFile
{
    public Dictionary<long, GeneratedRoutineOrigin> GeneratedRoutineOrigins { get; } = [];
    public Dictionary<long, List<GeneratedRoutineScaffold>> GeneratedRoutineScaffolds { get; } = [];
    public Dictionary<long, DynamicInitializer> DynamicInitializers { get; } = [];
    public Dictionary<long, List<DynamicInitializerStatement>> DynamicInitializerStatements { get; } = [];

    private void ReadDynamicInitializers()
    {
        var actions = InitializationActions.ToDictionary(action => action.Id);
        var actionTargets = InitializationActions.ToDictionary(action => action.EntityId);
        using (var query = database.Prepare("SELECT entity_id,kind,capture_proof,failure_policy FROM generated_routine_origins ORDER BY entity_id"))
            while (query.Read())
            {
                var origin = new GeneratedRoutineOrigin(query.Integer(0), query.Text(1), query.Text(2), query.Text(3));
                if (origin.Kind != "file-scope-initializer" || origin.CaptureProof != "observed-file-scope-init-creation" ||
                    origin.FailurePolicy != "terminate" || !Entities.TryGetValue(origin.EntityId, out Entity? entity) ||
                    entity.Definition != "strong" || !Functions.ContainsKey(origin.EntityId) ||
                    !actionTargets.ContainsKey(origin.EntityId) || !GeneratedRoutineOrigins.TryAdd(origin.EntityId, origin))
                    throw new InvalidDataException("Generated file-scope initializer requires its observed creation and owned strong startup body");
                // InitializationActions independently verifies the actual void()
                // execution ABI. The action's failure policy does not change it.
            }
        var storages = new HashSet<long>();
        var sourceOrdinals = new HashSet<long>();
        var actionCounts = new Dictionary<long, long>();
        var actionSourceOrdinals = new Dictionary<long, long>();
        using (var query = database.Prepare("SELECT id,action_id,storage_id,source_ordinal,ordinal,capture_proof,source_file_id,line,column,source_initializer_id FROM dynamic_initializers ORDER BY action_id,ordinal"))
            while (query.Read())
            {
                var initializer = new DynamicInitializer(query.Integer(0), query.Integer(1), query.Integer(2), query.Integer(3),
                    query.Integer(4), query.Text(5), query.OptionalInteger(6), query.Integer(7), query.Integer(8), query.Integer(9));
                if (initializer.Id <= 0 || initializer.SourceInitializerId <= 0 || initializer.SourceOrdinal < 0 || initializer.Line < 0 || initializer.Column < 0 ||
                    initializer.CaptureProof != "observed-file-scope-init-lowering" ||
                    !actions.TryGetValue(initializer.ActionId, out InitializationAction? action) ||
                    !GeneratedRoutineOrigins.ContainsKey(action.EntityId) ||
                    !Storages.TryGetValue(initializer.StorageId, out Storage? storage) || storage.Definition != "strong" ||
                    storage.IsThreadLocal || storage.SourceTypeId is null || !storages.Add(storage.Id) ||
                    !sourceOrdinals.Add(initializer.SourceOrdinal) || initializer.Ordinal != actionCounts.GetValueOrDefault(action.Id) ||
                    initializer.SourceOrdinal <= actionSourceOrdinals.GetValueOrDefault(action.Id, -1) ||
                    !DynamicInitializers.TryAdd(initializer.Id, initializer))
                    throw new InvalidDataException("Invalid observed dynamic initializer storage, action, order or location");
                actionCounts[action.Id] = initializer.Ordinal + 1;
                actionSourceOrdinals[action.Id] = initializer.SourceOrdinal;
            }
        foreach (GeneratedRoutineOrigin origin in GeneratedRoutineOrigins.Values)
            if (!actionCounts.ContainsKey(actionTargets[origin.EntityId].Id))
                throw new InvalidDataException("Generated startup action is missing original dynamic initializer provenance");
        RejectRows("""
            SELECT d.id FROM dynamic_initializers d
            LEFT JOIN source_initializers s ON s.id=d.source_initializer_id
            JOIN storages v ON v.id=d.storage_id
            WHERE s.id IS NULL OR s.kind!='file' OR s.function_entity_id IS NOT NULL
               OR s.execution_disposition!='emitted' OR s.storage_id IS NULL
               OR s.storage_id!=d.storage_id OR s.source_type_id!=v.source_type_id
               OR s.source_ordinal!=d.source_ordinal
               OR s.source_file_id IS NOT d.source_file_id OR s.line!=d.line OR s.column!=d.column
            """, "Dynamic initializer differs from its exact original file-scope declaration and order");

        var mappedNodes = new HashSet<long>();
        var rootPositions = new Dictionary<long, long>();
        var framePositions = new Dictionary<long, HashSet<long>>();
        using (var query = database.Prepare("""
            SELECT s.entity_id,s.ordinal,s.node_id,s.binding_id,s.kind,s.capture_proof,e.ordinal,n.function_id,b.function_id,b.scope_node_id,f.body_node_id,b.kind
            FROM generated_routine_scaffolds s
            LEFT JOIN nodes n ON n.id=s.node_id
            LEFT JOIN functions f ON f.entity_id=s.entity_id
            LEFT JOIN bindings b ON b.id=s.binding_id
            LEFT JOIN edges e ON e.parent_id=f.body_node_id AND e.role='statement' AND e.child_id=s.node_id
            ORDER BY s.entity_id,s.ordinal
            """))
            while (query.Read())
            {
                var scaffold = new GeneratedRoutineScaffold(query.Integer(0), query.Integer(1), query.Integer(2),
                    query.OptionalInteger(3), query.Text(4), query.Text(5));
                if (!GeneratedRoutineOrigins.ContainsKey(scaffold.EntityId) ||
                    query.OptionalInteger(6) is not long position || query.OptionalInteger(7) != scaffold.EntityId ||
                    !mappedNodes.Add(scaffold.NodeId))
                    throw new InvalidDataException("Generated startup scaffold must be an observed local storage prefix statement");
                long actionId = actionTargets[scaffold.EntityId].Id;
                if (scaffold.Kind == "cleanup-frame")
                {
                    if (scaffold.BindingId is not null || scaffold.CaptureProof != "observed-function-cleanup-frame")
                        throw new InvalidDataException("Generated cleanup frame scaffold must have exact observed frame provenance");
                    if (!framePositions.TryGetValue(actionId, out var positions)) framePositions.Add(actionId, positions = []);
                    positions.Add(position);
                }
                else
                {
                    if (scaffold.Kind is not ("declaration" or "zero-initialization") || scaffold.CaptureProof != "observed-scope-binding" ||
                        scaffold.BindingId is null || position != scaffold.Ordinal || query.OptionalInteger(8) != scaffold.EntityId ||
                        query.OptionalInteger(9) != query.OptionalInteger(10) || query.OptionalText(11) != "local")
                        throw new InvalidDataException("Generated local scaffold must be an observed storage prefix statement");
                    rootPositions[actionId] = position;
                }
                if (!GeneratedRoutineScaffolds.TryGetValue(scaffold.EntityId, out var scaffolds))
                    GeneratedRoutineScaffolds.Add(scaffold.EntityId, scaffolds = []);
                if (scaffold.Ordinal != scaffolds.Count)
                    throw new InvalidDataException("Generated startup scaffold ordinals must be dense");
                scaffolds.Add(scaffold);
            }
        using (var query = database.Prepare("""
            SELECT d.initializer_id,d.ordinal,d.node_id,e.ordinal,n.function_id
            FROM dynamic_initializer_statements d
            LEFT JOIN nodes n ON n.id=d.node_id
            LEFT JOIN functions f ON f.entity_id=n.function_id
            LEFT JOIN edges e ON e.parent_id=f.body_node_id AND e.role='statement' AND e.child_id=n.id
            JOIN dynamic_initializers i ON i.id=d.initializer_id
            ORDER BY i.action_id,i.ordinal,d.ordinal
            """))
            while (query.Read())
            {
                var statement = new DynamicInitializerStatement(query.Integer(0), query.Integer(1), query.Integer(2));
                if (!DynamicInitializers.TryGetValue(statement.InitializerId, out DynamicInitializer? initializer))
                    throw new InvalidDataException("Dynamic initializer statement has no original initializer");
                InitializationAction action = actions[initializer.ActionId];
                if (!DynamicInitializerStatements.TryGetValue(initializer.Id, out var statements))
                    DynamicInitializerStatements.Add(initializer.Id, statements = []);
                long expectedPosition = rootPositions.GetValueOrDefault(action.Id, -1) + 1;
                while (framePositions.GetValueOrDefault(action.Id, []).Contains(expectedPosition)) expectedPosition++;
                if (statement.Ordinal != statements.Count || query.OptionalInteger(3) is not long position ||
                    query.OptionalInteger(4) != action.EntityId || !mappedNodes.Add(statement.NodeId) ||
                    position != expectedPosition)
                    throw new InvalidDataException("Dynamic initializer statements must be distinct, contiguous, ordered children of their owned startup body");
                statements.Add(statement);
                rootPositions[action.Id] = position;
            }
        using (var query = database.Prepare("""
            SELECT e.child_id FROM generated_routine_origins g
            JOIN functions f ON f.entity_id=g.entity_id
            JOIN edges e ON e.parent_id=f.body_node_id AND e.role='statement'
            """))
            while (query.Read())
                if (!mappedNodes.Remove(query.Integer(0)))
                    throw new InvalidDataException("Generated startup body has an unaccounted or duplicate top-level statement");
        if (mappedNodes.Count != 0)
            throw new InvalidDataException("Generated startup mapping contains a statement outside its body");
    }

    private void VerifyGeneratedRoutineBody(long entityId, FunctionBody body)
    {
        if (!GeneratedRoutineScaffolds.TryGetValue(entityId, out var scaffolds)) return;
        var declarations = new HashSet<long>();
        foreach (GeneratedRoutineScaffold scaffold in scaffolds)
        {
            Node statement = body.Nodes[scaffold.NodeId];
            if (scaffold.Kind == "cleanup-frame")
            {
                var markers = new List<string>();
                void FrameOnly(Node node)
                {
                    if (node.Kind == "cleanup-frame" && CleanupFrameMarkers.TryGetValue(node.Id, out var marker) &&
                        body.Edges.GetValueOrDefault(node.Id, []).Count == 0) { markers.Add(marker.Kind); return; }
                    if (node.Kind == "expr-stmt") { FrameOnly(body.Child(node.Id, "value")); return; }
                    if (node.Kind == "block" && body.Children(node.Id, "statement").Any())
                    { foreach (var edge in body.Children(node.Id, "statement")) FrameOnly(body.Nodes[edge.ChildId]); return; }
                    throw new InvalidDataException("Generated frame scaffold contains non-frame execution");
                }
                FrameOnly(statement);
                if (markers.Count == 0 || markers.Distinct().Count() != 1)
                    throw new InvalidDataException("Generated frame scaffold mixes entry and exit markers");
                var roots = body.Children(body.RootId, "statement").ToArray();
                long position = roots.Single(edge => edge.ChildId == statement.Id).Ordinal;
                var initializerNodes = DynamicInitializers.Values.Where(value => InitializationActions.Any(action =>
                    action.Id == value.ActionId && action.EntityId == entityId)).SelectMany(value =>
                    DynamicInitializerStatements.GetValueOrDefault(value.Id, [])).Select(value => value.NodeId).ToHashSet();
                if (roots.Any(edge => initializerNodes.Contains(edge.ChildId) &&
                    (markers[0] == "prologue" ? edge.Ordinal < position : edge.Ordinal > position)))
                    throw new InvalidDataException("Generated cleanup frame is on the wrong side of original initialization");
                continue;
            }
            if (scaffold.Kind == "declaration")
            {
                if (statement.Kind != "declare" || statement.BindingId != scaffold.BindingId ||
                    body.Edges.GetValueOrDefault(statement.Id, []).Count != 0 || !declarations.Add(scaffold.BindingId!.Value))
                    throw new InvalidDataException("Startup declaration scaffold must declare exactly its local without executable initialization");
            }
            else
            {
                if (scaffold.Ordinal == 0 || scaffolds[(int)scaffold.Ordinal - 1] is not { Kind: "declaration" } previous ||
                    previous.BindingId != scaffold.BindingId || !IsZeroInitialization(statement, scaffold.BindingId!.Value, 0))
                    throw new InvalidDataException("Startup zero scaffold must only zero the immediately preceding local declaration");
            }
        }

        bool IsBinding(Node node, long binding) => node.Kind == "binding" && node.BindingId == binding;
        bool IsZeroInitialization(Node node, long binding, int depth)
        {
            if (depth > 8) return false;
            if (node.Kind == "block")
            {
                Edge[] statements = body.Children(node.Id, "statement").ToArray();
                return statements.Length == 1 && IsZeroInitialization(body.Nodes[statements[0].ChildId], binding, depth + 1);
            }
            if (node.Kind == "array-init")
                return !body.Children(node.Id, "element").Any() && IsBinding(body.Child(node.Id, "target"), binding);
            if (node.Kind != "expr-stmt") return false;
            Node expression = body.Child(node.Id, "value");
            if (expression.Kind != "assign" || !IsBinding(body.Child(expression.Id, "target"), binding)) return false;
            Node value = body.Child(expression.Id, "value");
            return value.Kind == "default-value" || value.Kind == "constant" &&
                value.ConstantText is { Length: > 0 } text && text.All(character => character == '0');
        }
    }

    private void VerifyGeneratedRoutineOrigins()
    {
        // Original declarations inside a GNU statement-expression initializer
        // retain their provenance. Only the created routine itself has no
        // original source function declaration.
        RejectRows("SELECT d.id FROM source_declarations d JOIN generated_routine_origins g ON g.entity_id=d.entity_id",
            "Generated startup routine must not fabricate an original function declaration");
    }
}
