namespace DotObj;

internal static class Verifier
{
    private static readonly HashSet<string> StatementKinds = ["record-storage-init", "compile-time-if", "try", "catch", "memory-barrier", "block", "return", "expr-stmt", "declare", "register-cleanup", "if", "while", "for", "do-while", "switch", "case", "break", "continue", "array-init", "array-element", "label", "goto"];
    private static readonly HashSet<string> ExpressionKinds = ["record-constant-init", "virtual-base-adjustment", "static-destruction-register", "guard-operation", "internal-try-expression", "caught-object-address", "handler-marker", "cleanup-payload-address", "throw", "thrown-object-address", "throw-marker", "rethrow", "record-transfer", "materialize", "lifetime-scope", "base-subobject", "virtual-base-subobject", "cleanup-transition", "cleanup-frame", "constant", "binding", "call", "binary", "unary", "assign", "convert", "mutate", "conditional", "address", "dereference", "array-decay", "member", "default-value", "storage", "storage-address", "function-address", "indirect-call", "compound-assign", "sequence", "va-start", "va-copy", "va-end", "va-arg", "volatile-place", "intrinsic", "block-copy"];

    public static void Body(ObjectFile obj, Entity function, FunctionBody body)
    {
        void Fail(string message) => throw new InvalidDataException($"{obj.Path}: {function.SourceName}: {message}");
        if (!body.Nodes.TryGetValue(body.RootId, out Node? root) || root.Kind != "block") Fail("Function root must be a block");
        var visited = new HashSet<long>();
        var cleanupVisibility = new Dictionary<long, HashSet<long>>();
        var cleanupThrowVisibility = new Dictionary<long, HashSet<long>>();
        var cleanupHandlerVisibility = new Dictionary<long, HashSet<long>>();
        var tryScopes = new Dictionary<long, long[]>();
        var activeTries = new Stack<long>();
        var activeHandlers = new Stack<long>();
        bool visitingCleanupTarget = false;
        var declarations = new HashSet<long>();
        var cleanupBindings = new HashSet<long>();
        var cleanupRegistrations = new HashSet<long>();
        var switchCases = new HashSet<long>();
        var labelScopes = new Dictionary<long, long[]>();
        var jumpScopes = new Dictionary<long, long[]>();
        var arrayElements = new HashSet<long>();
        var parameters = body.Bindings.Values.Where(b => b.Kind == "parameter").OrderBy(b => b.Ordinal).ToArray();
        var signature = obj.Parameters.GetValueOrDefault(function.Id, []);
        if (parameters.Length != signature.Count) Fail("Parameter binding count differs from signature");
        for (int index = 0; index < parameters.Length; index++)
        {
            Binding parameter = parameters[index];
            if (parameter.Ordinal != index || obj.Type(parameter.TypeId) != signature[index] || parameter.ScopeNodeId != body.RootId)
                Fail("Parameter binding differs from signature or root scope");
        }
        foreach (Binding binding in body.Bindings.Values)
        {
            if (binding.FunctionId != function.Id || binding.Kind is not ("local" or "parameter") || obj.Type(binding.TypeId) == "void")
                Fail($"Invalid binding {binding.Id}");
            if (ScalarTypes.IsAddressOnlyArray(obj.Type(binding.TypeId))) Fail("Automatic array storage requires a positive complete extent");
            if (binding.Kind == "local" && binding.Ordinal != null) Fail("Local has parameter ordinal");
            if (!body.Nodes.TryGetValue(binding.ScopeNodeId, out Node? scope) || scope.Kind != "block") Fail("Binding scope is not a block");
        }

        string Type(Node node) => node.TypeId is long id ? obj.Type(id) : throw new InvalidDataException("Expression has no type");
        bool IsExpression(Node node) => ExpressionKinds.Contains(node.Kind);
        Node Child(Node parent, string role) => body.Child(parent.Id, role);
        Node SequenceTail(Node node) => body.Nodes[body.Children(node.Id, "item").Last().ChildId];
        Node PlaceLeaf(Node node)
        {
            while (node.Kind is "lifetime-scope" or "sequence")
                node = node.Kind == "sequence" ? SequenceTail(node) : Child(node, "operand");
            return node;
        }
        void EdgeShape(Node node, params (string Role, int Min, int Max)[] shapes)
        {
            var edges = body.Edges.GetValueOrDefault(node.Id, []);
            if (edges.Any(edge => !shapes.Any(shape => shape.Role == edge.Role))) Fail($"Unexpected edge role on {node.Kind} {node.Id}");
            foreach (var shape in shapes)
            {
                var group = edges.Where(edge => edge.Role == shape.Role).OrderBy(edge => edge.Ordinal).ToArray();
                if (group.Length < shape.Min || group.Length > shape.Max || group.Where((edge, index) => edge.Ordinal != index).Any())
                    Fail($"Wrong edge cardinality/ordinal for {node.Kind}.{shape.Role}");
            }
        }
        void Expr(Node parent, string role, bool allowVoid = false)
        {
            Node child = Child(parent, role);
            if (!IsExpression(child) || (!allowVoid && Type(child) == "void")) Fail($"Expected scalar expression at {parent.Kind}.{role}");
        }
        void Condition(Node parent, string role)
        {
            Expr(parent, role);
            if (!ScalarTypes.IsScalar(Type(Child(parent, role)))) Fail("Condition requires a scalar value");
        }
        void SameType(Node node, Node other)
        {
            if (Type(node) != Type(other)) Fail($"Type mismatch at node {node.Id}");
        }
        void Statement(Node node)
        {
            if (!StatementKinds.Contains(node.Kind)) Fail($"Expected statement, found {node.Kind}");
        }
        bool VolatileSource(long? id)
        {
            if (id is null) return false;
            SourceType type = obj.SourceTypes[id.Value];
            while (true)
            {
                if (type.IsVolatile) return true;
                if (type.Kind is not ("typedef" or "qualified")) return false;
                type = obj.SourceTypes[type.UnderlyingTypeId!.Value];
            }
        }
        bool DeclaredVolatile(Node node, int depth = 0) => depth < 128 && node.Kind switch
        {
            "sequence" => DeclaredVolatile(SequenceTail(node), depth + 1),
            "lifetime-scope" => DeclaredVolatile(Child(node, "operand"), depth + 1),
            "binding" => node.BindingId is long id && obj.BindingSourceTypes.TryGetValue(id, out long source) && VolatileSource(source),
            "storage" => node.StorageId is long id && obj.Storages.TryGetValue(id, out Storage? storage) && VolatileSource(storage.SourceTypeId),
            "member" => node.FieldId is long id && obj.Fields.TryGetValue(id, out RecordField? field) &&
                (VolatileSource(field.SourceTypeId) || DeclaredVolatile(Child(node, "operand"), depth + 1)),
            _ => false
        };
        bool Place(Node node, int depth = 0) => depth <= 1024 && (node.Kind is "binding" or "storage" or "dereference" or "member" or "materialize" or "base-subobject" or "virtual-base-subobject" or "volatile-place" ||
            node.Kind == "lifetime-scope" && Place(Child(node, "operand"), depth + 1) ||
            node.Kind == "sequence" && Place(SequenceTail(node), depth + 1));
        bool LocalPlace(Node node, int depth = 0) => depth <= 1024 && (node.Kind == "binding" &&
            body.Bindings.TryGetValue(node.BindingId!.Value, out Binding? local) && local.Kind == "local" ||
            node.Kind is "member" or "lifetime-scope" && LocalPlace(Child(node, "operand"), depth + 1));
        var activeThrows = new Stack<long>();
        // Discarding a comma expression also discards its final operand.
        // Track this through expression wrappers without granting permission
        // to copy a class value when its result is actually consumed.
        bool DiscardsChild(Node parent, Edge edge, bool discardedResult) =>
            parent.Kind == "expr-stmt" && edge.Role == "value" ||
            parent.Kind == "for" && edge.Role is "initializer" or "step" ||
            parent.Kind == "internal-try-expression" && edge.Role is "attempt" or "cleanup" ||
            parent.Kind == "throw" && edge.Role == "initializer" ||
            parent.Kind == "sequence" && (discardedResult || edge.ChildId != SequenceTail(parent).Id) ||
            parent.Kind == "convert" && Type(parent) == "void" && edge.Role == "operand" ||
            parent.Kind == "lifetime-scope" && discardedResult && edge.Role == "operand";
        bool DiscardedRecordEffect(Node node, int depth = 0) => depth <= 1024 &&
            (Place(node) || node.Kind is "assign" or "record-transfer" or "record-constant-init" ||
             node.Kind == "sequence" && DiscardedRecordEffect(SequenceTail(node), depth + 1) ||
             node.Kind == "lifetime-scope" && DiscardedRecordEffect(Child(node, "operand"), depth + 1));
        void Visit(Node node, List<long> scopes, HashSet<long> visible, List<long> controls, int depth, bool discardedResult = false)
        {
            if (depth > 1024) Fail("Graph nesting exceeds supported limit");
            if (!visited.Add(node.Id)) Fail($"Shared or cyclic executable node {node.Id}");
            if (node.FunctionId != function.Id || (!StatementKinds.Contains(node.Kind) && !IsExpression(node))) Fail($"Unsupported node {node.Kind}");
            if (StatementKinds.Contains(node.Kind))
            {
                if ((node.TypeId != null) != (node.Kind == "array-element") || node.ValueCategory != null || node.EvaluationOrder != null) Fail("Statement carries expression metadata");
            }
            else
            {
                string type = Type(node);
                if (type == "void" && node.Kind is not ("guard-operation" or "call" or "indirect-call" or "convert" or "sequence" or "va-start" or "va-copy" or "va-end" or "intrinsic" or "block-copy" or "conditional" or "cleanup-transition" or "cleanup-frame" or "lifetime-scope" or "throw" or "throw-marker" or "rethrow" or "handler-marker" or "internal-try-expression")) Fail("Only calls and explicit void operations can have void type");
                if (node.ValueCategory != (Place(node) ? "place" : "value")) Fail("Incorrect value category");
                string[] orders = node.Kind is "cleanup-transition" or "cleanup-frame" or "constant" or "binding" or "default-value" or "storage" or "storage-address" or "function-address" or "volatile-place" ? ["none"] : ["left-to-right", "unspecified"];
                if (node.Kind is "virtual-base-adjustment" or "cleanup-transition" or "cleanup-frame" or "base-subobject" or "virtual-base-subobject" or "lifetime-scope") orders = ["none", "left-to-right"];
                if (node.Kind is "thrown-object-address" or "throw-marker" or "rethrow" or "cleanup-payload-address" or "handler-marker" or "caught-object-address") orders = ["none"];
                if (node.Kind is "materialize" or "throw" or "internal-try-expression") orders = ["left-to-right"];
                if (node.Kind is "call" or "assign" or "record-transfer" or "record-constant-init" or "compound-assign" or "block-copy") orders = ["left-to-right", "right-to-left", "unspecified"];
                if (node.Kind is "address" or "dereference" or "array-decay" or "member" or "va-start" or "va-copy" or "va-end" or "va-arg") orders = ["none", "left-to-right", "unspecified"];
                if (!orders.Contains(node.EvaluationOrder)) Fail($"Unsupported evaluation order at {node.Id}");
            }
            if ((node.EntityId != null) != (node.Kind is "call" or "register-cleanup" or "function-address") ||
                (node.BindingId != null) != (node.Kind is "binding" or "declare" or "register-cleanup") ||
                (node.FieldId != null) != (node.Kind == "member") ||
                (node.StorageId != null) != (node.Kind is "storage" or "storage-address") ||
                (node.Operator != null) != (node.Kind is "binary" or "unary" or "mutate" or "compound-assign" or "intrinsic") ||
                (node.ConstantText != null) != (node.Kind is "constant" or "array-element" or "storage-address" or "label" or "block-copy")) Fail("Unexpected or missing typed payload");

            if (IsExpression(node) && ScalarTypes.IsArray(Type(node)) && !Place(node)) Fail("Array value must be a place");
            foreach (Edge edge in body.Edges.GetValueOrDefault(node.Id, []))
            {
                Node child = body.Nodes[edge.ChildId];
                if (child.TypeId is long volatileType && ScalarTypes.IsScalar(obj.Type(volatileType)) && DeclaredVolatile(child) &&
                    !(node.Kind is "volatile-place" or "address" or "lifetime-scope" && edge.Role == "operand") &&
                    !(node.Kind == "sequence" && child.Id == SequenceTail(node).Id))
                    Fail("Declared volatile place requires an explicit volatile access wrapper");
                if (child.TypeId is long childType && ScalarTypes.IsArray(obj.Type(childType)) &&
                    !(node.Kind is "address" or "array-decay" or "lifetime-scope" && edge.Role == "operand") &&
                    !(node.Kind == "sequence" && child.Id == SequenceTail(node).Id) &&
                    !(node.Kind is "array-init" or "record-storage-init" && edge.Role == "target") && node.Kind != "block-copy")
                    Fail("Array place requires explicit address, decay or initialization");
                if (child.TypeId is long recordType && ScalarTypes.IsRecord(obj.Type(recordType)))
                {
                    bool discardedEffect = DiscardsChild(node, edge, discardedResult) && DiscardedRecordEffect(child);
                    bool allowed = node.Kind == "record-constant-init" && edge.Role is "target" or "value" || node.Kind == "record-storage-init" && edge.Role == "target" || discardedEffect || node.Kind == "block-copy" || node.Kind == "sequence" && node.ValueCategory == "place" && child.Id == SequenceTail(node).Id || node.Kind is "virtual-base-adjustment" or "member" or "base-subobject" or "virtual-base-subobject" or "lifetime-scope" or "address" && edge.Role == "operand" ||
                        node.Kind is "assign" or "record-transfer" && (edge.Role == "target" || edge.Role == "value" && child.Kind == "default-value") ||
                        node.Kind == "declare" && edge.Role == "initializer" && child.Kind == "default-value";
                    bool copyAllowed = node.Kind == "record-transfer" && edge.Role == "value"
                        ? obj.RecordStorageCopies.ContainsKey(node.Id) || obj.RecordValueTransfers.TryGetValue(node.Id, out string? transferKind) && obj.CanTransferRecord(obj.Type(recordType), transferKind)
                        : node.Kind == "assign" ? obj.CanCopyRecord(obj.Type(recordType))
                        : node.Kind == "declare" ? obj.CanConstructRecord(obj.Type(recordType))
                        : node.Kind is "call" or "indirect-call" && edge.Role == "argument" ? obj.CanPassRecord(obj.Type(recordType))
                        : obj.CanTransportRecord(obj.Type(recordType));
                    if (!allowed && !copyAllowed) Fail($"Record value operation lacks its required construction, transport or assignment permission (parent {node.Id}:{node.Kind}, child {child.Id}:{child.Kind}, edge {edge.Role}, type {recordType})");
                }
            }

            if (node.Kind == "block")
            {
                EdgeShape(node, ("statement", 0, int.MaxValue));
                var nestedScopes = new List<long>(scopes) { node.Id };
                var nestedVisible = new HashSet<long>(visible);
                foreach (Edge edge in body.Children(node.Id, "statement"))
                {
                    Node child = body.Nodes[edge.ChildId];
                    Statement(child);
                    if (child.Kind == "register-cleanup") cleanupRegistrations.Add(child.Id);
                    Visit(child, nestedScopes, nestedVisible, controls, depth + 1);
                    if (child.Kind == "declare") nestedVisible.Add(child.BindingId!.Value);
                }
                return;
            }
            switch (node.Kind)
            {
                case "try":
                    EdgeShape(node, ("body", 1, 1), ("handler", 1, int.MaxValue));
                    var handlers = body.Children(node.Id, "handler").ToArray();
                    if (!obj.SourceTries.ContainsKey(node.Id) || Child(node, "body").Kind != "block" ||
                        handlers.Any(edge => body.Nodes[edge.ChildId].Kind != "catch" ||
                            obj.SourceHandlers[edge.ChildId].TryNodeId != node.Id || obj.SourceHandlers[edge.ChildId].Ordinal != edge.Ordinal) ||
                        handlers.Length != obj.SourceHandlers.Values.Count(handler => handler.TryNodeId == node.Id))
                        Fail("Try requires its owned body and ordered handlers");
                    tryScopes.Add(node.Id, scopes.ToArray());
                    activeTries.Push(node.Id);
                    Visit(Child(node, "body"), scopes, visible, controls, depth + 1);
                    foreach (var edge in handlers) Visit(body.Nodes[edge.ChildId], scopes, visible, controls, depth + 1);
                    activeTries.Pop(); return;
                case "catch":
                    EdgeShape(node, ("body", 1, 1));
                    if (!obj.SourceHandlers.TryGetValue(node.Id, out SourceHandler? handler) || activeTries.Count == 0 ||
                        activeTries.Peek() != handler.TryNodeId || Child(node, "body").Kind != "block")
                        Fail("Catch is outside its owning source try");
                    if (handler!.ParameterBindingId is long parameter &&
                        (!body.Bindings.TryGetValue(parameter, out Binding? catchBinding) || catchBinding.Kind != "local" || catchBinding.ScopeNodeId != Child(node, "body").Id))
                        Fail("Catch parameter is outside its handler body");
                    activeHandlers.Push(node.Id);
                    Visit(Child(node, "body"), scopes, visible, controls, depth + 1);
                    activeHandlers.Pop(); return;
                case "caught-object-address":
                    EdgeShape(node);
                    if (!obj.CaughtObjectAddresses.TryGetValue(node.Id, out long caughtHandler) || !activeHandlers.Contains(caughtHandler))
                        Fail("Caught object address is outside its owning handler");
                    break;
                case "handler-marker":
                    EdgeShape(node);
                    if (Type(node) != "void" || !obj.HandlerMarkers.TryGetValue(node.Id, out HandlerMarker? marker))
                        Fail("Handler marker lacks its owned metadata");
                    if (marker!.Kind == "try-exit")
                    {
                        if (!activeTries.Contains(marker.OwnerNodeId) &&
                            (!tryScopes.TryGetValue(marker.OwnerNodeId, out long[]? ownerScopes) || !ownerScopes.SequenceEqual(scopes)))
                            Fail("Try exit is outside its owned source region");
                    }
                    else if (!activeHandlers.Contains(marker.OwnerNodeId) || marker.Kind == "caught" && activeHandlers.Peek() != marker.OwnerNodeId)
                        Fail("Handler marker is outside its active source handler");
                    break;
                case "throw":
                    if (Type(node) != "void" || !obj.ThrowExpressions.TryGetValue(node.Id, out ThrowExpression? thrown))
                        Fail("Throw lacks its observed payload initialization");
                    bool hasInitializer = thrown!.InitializationKind == "expression";
                    EdgeShape(node, ("initializer", hasInitializer ? 1 : 0, hasInitializer ? 1 : 0));
                    if (hasInitializer) Expr(node, "initializer", true);
                    activeThrows.Push(node.Id);
                    break;
                case "cleanup-payload-address":
                    EdgeShape(node);
                    if (!visitingCleanupTarget || !obj.CleanupPayloadTargets.ContainsKey(node.Id))
                        Fail("Payload cleanup address is only valid in its owned action recipe");
                    break;
                case "thrown-object-address":
                    EdgeShape(node);
                    if (Type(node) != "ptr(void)" || activeThrows.Count == 0 ||
                        !obj.ThrowObjectAddresses.TryGetValue(node.Id, out long throwOwner) || throwOwner != activeThrows.Peek())
                        Fail("Thrown object address requires its exact lexical initializer and original void-pointer type");
                    break;
                case "throw-marker":
                    EdgeShape(node);
                    if (Type(node) != "void" || activeThrows.Count == 0 ||
                        !obj.ThrowMarkers.TryGetValue(node.Id, out long markerOwner) || markerOwner != activeThrows.Peek())
                        Fail("Throw-start marker requires its exact lexical initializer");
                    break;
                case "rethrow":
                    EdgeShape(node);
                    if (Type(node) != "void" || !obj.RethrowExpressions.Contains(node.Id))
                        Fail("Rethrow requires original throw provenance");
                    break;
                case "materialize":
                    EdgeShape(node, ("operand", 1, 1)); Expr(node, "operand");
                    if (!ScalarTypes.IsRecord(Type(node)) || !obj.CanTransportRecord(Type(node)) ||
                        node.TypeId != Child(node, "operand").TypeId || Child(node, "operand").ValueCategory != "value")
                        Fail("Record materialization requires a same-type construction-safe record value");
                    break;
                case "lifetime-scope":
                    EdgeShape(node, ("operand", 1, 1)); Expr(node, "operand", Type(node) == "void");
                    if (!obj.ExpressionLifetimes.ContainsKey(node.Id) || node.TypeId != Child(node, "operand").TypeId ||
                        node.ValueCategory != Child(node, "operand").ValueCategory)
                        Fail("Lifetime scope must preserve its child's exact type/category and original lifetime identity");
                    break;
                case "guard-operation":
                    EdgeShape(node, ("guard", 1, 1)); Expr(node, "guard");
                    if (!obj.GuardOperations.TryGetValue(node.Id, out GuardOperation? guardOperation) ||
                        Type(node) != (guardOperation.Kind is "release" or "thread-publish" ? "void" : "i32") || Type(Child(node, "guard")) != "ptr(u64)")
                        Fail("Guard operation lacks its verified publication contract");
                    break;
                case "internal-try-expression":
                    EdgeShape(node, ("attempt", 1, 1), ("cleanup", 1, 1));
                    Expr(node, "attempt", true); Expr(node, "cleanup", true);
                    if (Type(node) != "void" || !obj.InternalTryExpressions.Contains(node.Id))
                        Fail("Internal try requires observed void expression and unwind-before-handler semantics");
                    if (obj.InternalTryAllocations.TryGetValue(node.Id, out long allocation))
                    {
                        Node cleanup = Child(node, "cleanup");
                        if (cleanup.Kind != "call" || cleanup.EntityId is not long deallocator ||
                            obj.OriginalFunction(deallocator) != obj.SourceAllocationObligations[allocation].DeallocatorEntityId)
                            Fail("Allocation failure boundary must invoke its exact original selected deallocator");
                    }
                    break;
                case "cleanup-transition":
                    EdgeShape(node);
                    if (Type(node) != "void" || !obj.CleanupTransitions.TryGetValue(node.Id, out CleanupTransition? transition))
                        Fail("Cleanup transition lacks its observed marker metadata");
                    if (activeThrows.Count != 0
                        ? !obj.ThrowCleanupTransitions.TryGetValue(node.Id, out long cleanupOwner) || cleanupOwner != activeThrows.Peek()
                        : obj.ThrowCleanupTransitions.ContainsKey(node.Id))
                        Fail("Cleanup transition differs from its exact throw initialization boundary");
                    cleanupVisibility.Add(node.Id, new HashSet<long>(visible));
                    cleanupThrowVisibility.Add(node.Id, activeThrows.ToHashSet());
                    cleanupHandlerVisibility.Add(node.Id, activeHandlers.ToHashSet());
                    break;
                case "cleanup-frame":
                    EdgeShape(node);
                    if (Type(node) != "void" || !obj.CleanupFrameMarkers.ContainsKey(node.Id))
                        Fail("Cleanup frame annotation lacks its observed marker metadata");
                    break;
                case "memory-barrier":
                    if (!obj.MemoryBarriers.ContainsKey(node.Id)) Fail("Memory assembly statement lacks owned source proof");
                    MemoryBarrierOperand[] operands = obj.BarrierOperands(node.Id);
                    EdgeShape(node, ("address", operands.Length, operands.Length), ("extent", operands.Length, operands.Length));
                    Node[] addresses = body.Children(node.Id, "address").Select(edge => body.Nodes[edge.ChildId]).ToArray();
                    Node[] extents = body.Children(node.Id, "extent").Select(edge => body.Nodes[edge.ChildId]).ToArray();
                    for (int index = 0; index < operands.Length; index++)
                    {
                        MemoryBarrierOperand operand = operands[index];
                        Node address = addresses[index], asmExtent = extents[index];
                        string element = obj.Type(obj.SourceTypes[operand.SourceElementTypeId].ExecutionTypeId);
                        if (!IsExpression(address) || Type(address) != "ptr(" + element + ")" || !IsExpression(asmExtent) || Type(asmExtent) != "u64")
                            Fail("Memory assembly requires its typed element address and unsigned byte extent");
                        if (operand.ExtentKind == "fixed")
                        {
                            var bytes = new System.Numerics.BigInteger(operand.FixedElementCount!.Value) * operand.ElementSize;
                            if (bytes > ulong.MaxValue || asmExtent.Kind != "constant" ||
                                asmExtent.ConstantText != bytes.ToString(System.Globalization.CultureInfo.InvariantCulture))
                                Fail("Memory assembly fixed extent differs from its original bounded object");
                        }
                        else
                        {
                            if (!body.Bindings.TryGetValue(operand.ExtentBindingId!.Value, out Binding? total) || total.Kind != "local" || !total.IsSynthetic || obj.Type(total.TypeId) != "i64" ||
                                asmExtent.Kind != "binary" || asmExtent.Operator != "mul")
                                Fail("Memory assembly variable extent requires its observed saved total binding");
                            Node savedCount = Child(asmExtent, "left"), elementSizeNode = Child(asmExtent, "right");
                            if (savedCount.Kind != "convert" || Type(savedCount) != "u64" || Child(savedCount, "operand").Kind != "binding" ||
                                Child(savedCount, "operand").BindingId != operand.ExtentBindingId || elementSizeNode.Kind != "constant" || Type(elementSizeNode) != "u64" ||
                                elementSizeNode.ConstantText != operand.ElementSize.ToString(System.Globalization.CultureInfo.InvariantCulture))
                                Fail("Memory assembly variable byte extent differs from its saved total and element size");
                        }
                    }
                    break;
                case "return":
                    bool isVoid = obj.Type(function.ReturnTypeId) == "void";
                    EdgeShape(node, ("value", isVoid ? 0 : 1, 1));
                    if (body.Children(node.Id, "value").Any())
                    {
                        Expr(node, "value", isVoid);
                        if (Type(Child(node, "value")) != obj.Type(function.ReturnTypeId)) Fail("Return type mismatch");
                    }
                    break;
                case "expr-stmt": EdgeShape(node, ("value", 1, 1)); Expr(node, "value", true); break;
                case "declare":
                    EdgeShape(node, ("initializer", 0, 1));
                    if (!body.Bindings.TryGetValue(node.BindingId!.Value, out Binding? declared) || declared.Kind != "local" || declared.ScopeNodeId != scopes.LastOrDefault())
                        Fail("Declaration binding is not local to enclosing block");
                    if (!declarations.Add(node.BindingId!.Value)) Fail("Duplicate local declaration");
                    if (body.Children(node.Id, "initializer").Any())
                    {
                        Expr(node, "initializer");
                        if (Type(Child(node, "initializer")) != obj.Type(declared!.TypeId)) Fail("Initializer type mismatch");
                    }
                    break;
                case "register-cleanup":
                    EdgeShape(node);
                    if (!cleanupRegistrations.Contains(node.Id)) Fail("Cleanup registration must be an immediate block child");
                    if (!body.Bindings.TryGetValue(node.BindingId!.Value, out Binding? cleanupBinding) ||
                        cleanupBinding.Kind != "local" || cleanupBinding.ScopeNodeId != scopes.LastOrDefault() ||
                        !visible.Contains(cleanupBinding.Id))
                        Fail("Cleanup registration requires a declared local in its owning block");
                    if (!cleanupBindings.Add(node.BindingId!.Value)) Fail("Duplicate cleanup registration for local binding");
                    if (!obj.Entities.ContainsKey(node.EntityId!.Value)) Fail("Unknown cleanup callback");
                    var cleanupParameters = obj.Parameters.GetValueOrDefault(node.EntityId!.Value, []);
                    if (cleanupParameters.Count != 1 ||
                        cleanupParameters[0] != "ptr(" + obj.Type(cleanupBinding!.TypeId) + ")" && cleanupParameters[0] != "ptr(void)")
                        Fail("Cleanup callback must accept a pointer to its local binding type or void");
                    break;
                case "compile-time-if":
                    EdgeShape(node, ("body", 1, 1)); Statement(Child(node, "body"));
                    if (!obj.CompileTimeSelections.TryGetValue(node.Id, out CompileTimeSelection? selection))
                        Fail("Compile-time selection lacks its observed frontend decision");
                    if (!selection!.ConditionValue && !selection.HasElse &&
                        (Child(node, "body").Kind != "block" || body.Children(Child(node, "body").Id, "statement").Any()))
                        Fail("A false compile-time selection without else must have an empty body");
                    break;
                case "if":
                    EdgeShape(node, ("condition", 1, 1), ("then", 1, 1), ("else", 0, 1));
                    Condition(node, "condition"); Statement(Child(node, "then"));
                    if (body.Children(node.Id, "else").Any()) Statement(Child(node, "else"));
                    break;
                case "while":
                case "do-while":
                    EdgeShape(node, ("condition", 1, 1), ("body", 1, 1));
                    Condition(node, "condition"); Statement(Child(node, "body")); break;
                case "for":
                    EdgeShape(node, ("initializer", 0, 1), ("condition", 0, 1), ("step", 0, 1), ("body", 1, 1));
                    foreach (string role in new[] { "initializer", "condition", "step" })
                        if (body.Children(node.Id, role).Any()) Expr(node, role, role != "condition");
                    if (body.Children(node.Id, "condition").Any()) Condition(node, "condition");
                    Statement(Child(node, "body"));
                    break;
                case "switch":
                    EdgeShape(node, ("condition", 1, 1), ("body", 1, 1));
                    Expr(node, "condition");
                    string selectorType = Type(Child(node, "condition"));
                    if (ScalarTypes.IsWideInteger(selectorType)) Fail("128-bit switch dispatch is not yet supported");
                    if (!ScalarTypes.IsPromotedInteger(selectorType)) Fail("Switch selector must be a promoted integer");
                    Node switchBody = Child(node, "body");
                    if (switchBody.Kind != "block") Fail("Switch body must be a block");
                    bool labeled = false, hasDefault = false;
                    var caseValues = new HashSet<string>();
                    var discovered = new List<Node>();
                    var scanned = new HashSet<long>();
                    void FindCases(Node current, int scanDepth)
                    {
                        if (scanDepth > 1024 || !scanned.Add(current.Id)) Fail("Cyclic or excessively nested switch body");
                        if (current.Kind == "switch") return;
                        if (current.Kind == "case") { discovered.Add(current); return; }
                        foreach (Edge childEdge in body.Edges.GetValueOrDefault(current.Id, []))
                            if (childEdge.Role != "target" && StatementKinds.Contains(body.Nodes[childEdge.ChildId].Kind))
                                FindCases(body.Nodes[childEdge.ChildId], scanDepth + 1);
                    }
                    FindCases(switchBody, 0);
                    HashSet<long> immediateCases = body.Children(switchBody.Id, "statement").Select(edge => body.Nodes[edge.ChildId])
                        .Where(item => item.Kind == "case").Select(item => item.Id).ToHashSet();
                    foreach (Node item in discovered)
                    {
                        switchCases.Add(item.Id);
                        EdgeShape(item, ("value", 0, 1), ("target", 0, 1));
                        if (body.Children(item.Id, "target").Any() && body.Child(item.Id, "target").Id != node.Id)
                            Fail("Case target is not its enclosing switch");
                        if (!immediateCases.Contains(item.Id)) body.RequiresFlatControl = true;
                        if (body.Children(item.Id, "value").Any())
                        {
                            Node value = body.Child(item.Id, "value");
                            if (value.Kind != "constant" || !ScalarTypes.IsInteger(Type(value)) ||
                                !ScalarTypes.IsConstant(Type(value), value.ConstantText)) Fail("Case value must be an integer constant");
                            string converted = ScalarTypes.SwitchConstant(Type(value), value.ConstantText!, selectorType);
                            if (!caseValues.Add(converted)) Fail("Duplicate converted switch case value");
                        }
                        else if (hasDefault) Fail("Duplicate default label");
                        else hasDefault = true;
                    }
                    foreach (Edge edge in body.Children(switchBody.Id, "statement"))
                    {
                        Node item = body.Nodes[edge.ChildId];
                        if (item.Kind == "register-cleanup") Fail("Cleanup registration cannot be bypassed by switch dispatch");
                        if (item.Kind == "case") labeled = true;
                        else if (item.Kind == "declare")
                        {
                            if (labeled || body.Children(item.Id, "initializer").Any()) body.RequiresFlatControl = true;
                        }
                        else if (!labeled) body.RequiresFlatControl = true;
                    }
                    break;
                case "case":
                    if (!switchCases.Contains(node.Id)) Fail("Case label is not owned by a switch");
                    EdgeShape(node, ("value", 0, 1), ("target", 0, 1));
                    long nearestSwitch = controls.LastOrDefault(id => body.Nodes[id].Kind == "switch");
                    if (nearestSwitch == 0 || body.Children(node.Id, "target").Any() && Child(node, "target").Id != nearestSwitch)
                        Fail("Case ownership does not match its enclosing switch");
                    break;
                case "break":
                case "continue":
                    EdgeShape(node, ("target", 1, 1));
                    long nearest = node.Kind == "break" ? controls.LastOrDefault()
                        : controls.LastOrDefault(id => body.Nodes[id].Kind != "switch");
                    if (nearest == 0 || Child(node, "target").Id != nearest)
                        Fail(node.Kind == "break" ? "Break target must be the nearest enclosing loop or switch"
                            : "Continue target must be the nearest enclosing loop");
                    return; // target is an identity reference, not another executable occurrence.
                case "static-destruction-register":
                    EdgeShape(node, ("callback", 1, 1), ("object", 1, 1));
                    Expr(node, "callback"); Expr(node, "object");
                    if (Type(node) != "i32" || node.EvaluationOrder != "left-to-right" ||
                        !RoutineTypes.IsFunctionPointer(Type(Child(node, "callback"))) ||
                        !ScalarTypes.IsPointer(Type(Child(node, "object"))) || RoutineTypes.IsFunctionPointer(Type(Child(node, "object"))) ||
                        !obj.StaticDestructionRegistrations.ContainsKey(node.Id))
                        Fail("Static destructor registration lacks its typed owned provenance");
                    break;
                case "intrinsic":
                    EdgeShape(node, ("argument", 0, int.MaxValue));
                    var intrinsicArgs = body.Children(node.Id, "argument").Select(edge => body.Nodes[edge.ChildId]).ToArray();
                    if (intrinsicArgs.Any(argument => !IsExpression(argument) || Type(argument) == "void")) Fail("Intrinsic argument must be a value");
                    string intrinsic = node.Operator!;
                    bool mathematicalOverflow = intrinsic is "add-overflow" or "sub-overflow" or "mul-overflow" or "add-overflow-volatile" or "sub-overflow-volatile" or "mul-overflow-volatile";
                    if (ScalarTypes.IsWideInteger(Type(node)) || !mathematicalOverflow && intrinsicArgs.Any(argument => ScalarTypes.IsWideInteger(Type(argument)) || ScalarTypes.IsPointer(Type(argument)) && ScalarTypes.IsWideInteger(ScalarTypes.Pointee(Type(argument)))))
                        Fail("128-bit intrinsics require an explicit wide operation contract");
                    if (FloatingIntrinsics.IsClassification(intrinsic))
                    {
                        if (intrinsicArgs.Length != 1 || Type(node) != "i32" ||
                            !FloatingIntrinsics.AcceptsOperand(intrinsic, Type(intrinsicArgs[0])))
                            Fail("Invalid typed floating classification intrinsic");
                    }
                    else if (intrinsic == "unreachable") { if (intrinsicArgs.Length != 0 || Type(node) != "void") Fail("Invalid unreachable intrinsic"); }
                    else if (intrinsic == "expect")
                    {
                        if (intrinsicArgs.Length != 2 || !ScalarTypes.IsInteger(Type(node)) || intrinsicArgs.Any(argument => Type(argument) != Type(node))) Fail("Invalid expectation intrinsic");
                    }
                    else if (intrinsic is "clz" or "ctz" or "popcount" or "parity" or "ffs" or "clrsb" or "bswap")
                    {
                        if (intrinsicArgs.Length != 1 || !ScalarTypes.IsInteger(Type(intrinsicArgs[0])) || !ScalarTypes.IsInteger(Type(node))) Fail("Invalid integer bit intrinsic");
                        string operand = Type(intrinsicArgs[0]);
                        if (intrinsic == "bswap" ? operand != Type(node) || obj.StorageSize(operand) is not (2 or 4 or 8)
                            : intrinsic is "ffs" or "clrsb" ? operand is not ("i32" or "i64") : operand is not ("u32" or "u64")) Fail("Unsupported bit intrinsic width/signature");
                    }
                    else if (intrinsic is "add-overflow" or "sub-overflow" or "mul-overflow" or "add-overflow-volatile" or "sub-overflow-volatile" or "mul-overflow-volatile")
                    {
                        if (intrinsicArgs.Length != 3 || !ScalarTypes.IsInteger(Type(intrinsicArgs[0])) || !ScalarTypes.IsInteger(Type(intrinsicArgs[1])) ||
                            !ScalarTypes.IsPointer(Type(intrinsicArgs[2])) || !ScalarTypes.IsInteger(ScalarTypes.Pointee(Type(intrinsicArgs[2]))) || !ScalarTypes.IsBooleanResult(Type(node)))
                            Fail("Invalid mathematical overflow intrinsic");
                        if (intrinsic.EndsWith("-volatile", StringComparison.Ordinal) && ScalarTypes.IsWideInteger(ScalarTypes.Pointee(Type(intrinsicArgs[2]))))
                            Fail("Volatile 128-bit overflow destination requires an explicit access contract");
                    }
                    else Fail("Unsupported intrinsic operation");
                    break;
                case "volatile-place":
                    EdgeShape(node, ("operand", 1, 1)); Expr(node, "operand");
                    if (!Place(Child(node, "operand")) || Child(node, "operand").Kind == "volatile-place" || !ScalarTypes.IsScalar(Type(node)))
                        Fail("Volatile place requires a scalar addressable location");
                    if (PlaceLeaf(Child(node, "operand")).Kind == "member" && obj.Fields[PlaceLeaf(Child(node, "operand")).FieldId!.Value].BitWidth is not null)
                        Fail("Volatile bitfield access requires a storage-unit contract");
                    if (ScalarTypes.IsWideInteger(Type(node))) Fail("Volatile 128-bit access requires an explicit access contract");
                    SameType(node, Child(node, "operand"));
                    break;
                case "va-start":
                case "va-copy":
                case "va-end":
                case "va-arg":
                    EdgeShape(node, ("operand", 1, 1), ("source", node.Kind == "va-copy" ? 1 : 0, node.Kind == "va-copy" ? 1 : 0));
                    Expr(node, "operand");
                    if (Type(Child(node, "operand")) != "ptr(va-list)") Fail("va_list operation requires a pointer to the shared cursor");
                    if (node.Kind == "va-copy") { Expr(node, "source"); if (Type(Child(node, "source")) != "ptr(va-list)") Fail("va_copy source must be a cursor pointer"); }
                    if (node.Kind == "va-start" && !obj.Routines[function.RoutineTypeId].Variadic) Fail("va_start outside variadic function");
                    if (node.Kind == "va-arg" ? !ScalarTypes.IsScalar(Type(node)) : Type(node) != "void") Fail("Invalid va_list result type");
                    break;
                case "sequence":
                    EdgeShape(node, ("item", 2, int.MaxValue));
                    var sequence = body.Children(node.Id, "item").Select(edge => body.Nodes[edge.ChildId]).ToArray();
                    if (node.EvaluationOrder != "left-to-right" || sequence.Any(item => !IsExpression(item)) || node.TypeId != sequence[^1].TypeId || node.ValueCategory != sequence[^1].ValueCategory)
                        Fail("Sequence must evaluate expressions in order and retain the final exact type and place category");
                    break;
                case "label":
                    EdgeShape(node); labelScopes.Add(node.Id, scopes.ToArray()); break;
                case "goto":
                    EdgeShape(node, ("target", 1, 1));
                    if (Child(node, "target").Kind != "label") Fail("Goto target must be an owned label");
                    jumpScopes.Add(node.Id, scopes.ToArray()); return;
                case "storage":
                    EdgeShape(node);
                    if (!obj.Storages.TryGetValue(node.StorageId!.Value, out Storage? storage))
                        Fail("Storage place has no selected declaration");
                    string declaredStorageType = obj.Type(storage!.TypeId);
                    // c_gen_be::check_type_of_variable_node preserves array
                    // views formed before a late major-bound completion.
                    // Both exact type rows remain owned; inner bounds/elements
                    // and two known major bounds must still match.
                    if (Type(node) != declaredStorageType && !(ScalarTypes.IsArray(Type(node)) && ScalarTypes.IsArray(declaredStorageType) &&
                        (ScalarTypes.IsIncompleteArray(Type(node)) || ScalarTypes.IsIncompleteArray(declaredStorageType)) &&
                        ScalarTypes.Element(Type(node)) == ScalarTypes.Element(declaredStorageType)))
                        Fail("Storage place does not match its declaration type");
                    break;
                case "storage-address":
                    EdgeShape(node);
                    if (!obj.Storages.TryGetValue(node.StorageId!.Value, out Storage? addressStorage) || !ScalarTypes.IsPointer(Type(node)) ||
                        RoutineTypes.IsFunctionPointer(Type(node)) || !ScalarTypes.IsConstant("i64", node.ConstantText) ||
                        !long.TryParse(node.ConstantText, out long displacement) || displacement < 0 ||
                        (displacement > obj.StorageSize(obj.Type(addressStorage.TypeId)) && !(addressStorage.Definition == "declaration" && obj.StorageSize(obj.Type(addressStorage.TypeId)) == 0)))
                        Fail("Storage address requires a data pointer and bounded byte displacement");
                    break;
                case "function-address":
                    EdgeShape(node);
                    if (!obj.Entities.ContainsKey(node.EntityId!.Value) || Type(node) != "ptr(" + obj.EntityRoutine(node.EntityId.Value) + ")")
                        Fail("Function address disagrees with its callable signature");
                    break;
                case "indirect-call":
                    EdgeShape(node, ("operand", 1, 1), ("argument", 0, int.MaxValue)); Expr(node, "operand");
                    if (!RoutineTypes.IsFunctionPointer(Type(Child(node, "operand")))) Fail("Indirect call requires a typed function pointer");
                    if (obj.DeferredRoutineKeys.Contains(ScalarTypes.Pointee(Type(Child(node, "operand")))))
                        Fail("Indirect call requires a completed original routine ABI");
                    var indirectSignature = RoutineTypes.Signature(ScalarTypes.Pointee(Type(Child(node, "operand"))));
                    var indirectArgs = body.Children(node.Id, "argument").Select(edge => body.Nodes[edge.ChildId]).ToArray();
                    if (Type(node) != indirectSignature.Result || (indirectSignature.Variadic ? indirectArgs.Length < indirectSignature.Parameters.Length : indirectArgs.Length != indirectSignature.Parameters.Length) ||
                        indirectArgs.Take(indirectSignature.Parameters.Length).Where((arg, index) => !IsExpression(arg) || Type(arg) != indirectSignature.Parameters[index]).Any() ||
                        indirectArgs.Skip(indirectSignature.Parameters.Length).Any(arg => !IsExpression(arg) || !(ScalarTypes.IsPromotedInteger(Type(arg)) || Type(arg) is "f64" or "f80" || ScalarTypes.IsPointer(Type(arg)))))
                        Fail("Indirect call result or arguments disagree with the callable signature");
                    break;
                case "constant":
                    EdgeShape(node);
                    if (!ScalarTypes.IsConstant(Type(node), node.ConstantText)) Fail("Invalid scalar constant");
                    break;
                case "default-value":
                    EdgeShape(node);
                    if (!obj.CanZeroInitializeRecord(Type(node))) Fail("Default value requires an owned zero-initializable record layout");
                    break;
                case "virtual-base-adjustment":
                    EdgeShape(node, ("operand", 1, 1)); Expr(node, "operand");
                    if (!ScalarTypes.IsPointer(Type(node)) || Type(node) != Type(Child(node, "operand")) ||
                        !obj.VirtualBaseAdjustments.ContainsKey(node.Id))
                        Fail("Virtual base adjustment requires its exact lowered pointer and owned provenance");
                    obj.VerifyVirtualBaseAdjustmentBody(node, body);
                    break;
                case "virtual-base-subobject":
                    EdgeShape(node, ("operand", 1, 1)); Expr(node, "operand");
                    Node virtualOwner = Child(node, "operand");
                    if (!visitingCleanupTarget || !Place(virtualOwner) || virtualOwner.TypeId is not long virtualOwnerType ||
                        !obj.VirtualBaseExpressions.TryGetValue(node.Id, out long completeBase) ||
                        !obj.VirtualBaseSubobjects.TryGetValue((virtualOwnerType, completeBase), out VirtualBaseSubobject? virtualView) ||
                        virtualView.CompleteBaseTypeId != node.TypeId || virtualView.FieldId is not null)
                        Fail("Virtual base view requires its exact observed placement inside a cleanup address recipe");
                    break;
                case "base-subobject":
                    EdgeShape(node, ("operand", 1, 1)); Expr(node, "operand");
                    Node containing = Child(node, "operand");
                    if (!visitingCleanupTarget || !Place(containing) || containing.TypeId is not long ownerType ||
                        !obj.BaseSubobjectExpressions.TryGetValue(node.Id, out long sourceBase) ||
                        !obj.BaseSubobjects.TryGetValue((ownerType, sourceBase), out RecordBaseSubobject? relationship) ||
                        relationship.CompleteBaseTypeId != node.TypeId || relationship.Extent != 0 || relationship.FieldId is not null ||
                        obj.SourceBases[sourceBase].IsVirtual)
                        Fail("Empty base view requires its exact nonvirtual source relationship inside a cleanup address recipe");
                    break;
                case "member":
                    EdgeShape(node, ("operand", 1, 1)); Expr(node, "operand");
                    if (!obj.Fields.TryGetValue(node.FieldId!.Value, out RecordField? field) ||
                        !Place(Child(node, "operand")) || Type(Child(node, "operand")) != obj.Type(field.RecordTypeId) ||
                        Type(node) != obj.Type(field.TypeId))
                        Fail("Member requires a field owned by its record place and matching field result type");
                    break;
                case "binding":
                    EdgeShape(node);
                    if (!body.Bindings.TryGetValue(node.BindingId!.Value, out Binding? binding) || !scopes.Contains(binding.ScopeNodeId) || !visible.Contains(binding.Id)) Fail("Binding used outside scope or before declaration");
                    if (obj.Type(binding!.TypeId) != Type(node)) Fail("Binding reference type mismatch");
                    break;
                case "record-storage-init":
                    EdgeShape(node, ("target", 1, 1), ("body", 1, 1)); Expr(node, "target");
                    Node initialized = Child(node, "target");
                    if (!obj.RecordStorageInitializations.TryGetValue(node.Id, out RecordStorageInitialization? initialization) ||
                        initialized.Kind != "binding" || !Place(initialized) || initialized.BindingId != initialization.BindingId ||
                        Child(node, "body").Kind != "block")
                        Fail("Record storage initialization requires its original local binding and typed initialization body");
                    obj.VerifyRecordStorageInitializationBody(node, body);
                    break;
                case "array-init":
                    EdgeShape(node, ("target", 1, 1), ("element", 0, int.MaxValue));
                    Expr(node, "target");
                    Node arrayTarget = Child(node, "target");
                    if (!LocalPlace(arrayTarget) || !ScalarTypes.IsArray(Type(arrayTarget)))
                        Fail("Array initialization requires a local array binding or its named member");
                    string leaf = ScalarTypes.Leaf(Type(arrayTarget));
                    if (ScalarTypes.IsIncompleteArray(Type(arrayTarget))) Fail("Array initialization requires a known bound");
                    long count = ScalarTypes.FlatCount(Type(arrayTarget));
                    foreach (Edge edge in body.Children(node.Id, "element"))
                    {
                        Node item = body.Nodes[edge.ChildId];
                        if (item.Kind != "array-element" || Type(item) != leaf ||
                            !ScalarTypes.IsConstant("u64", item.ConstantText) ||
                            !long.TryParse(item.ConstantText, out long index) || index >= count)
                            Fail("Array initializer element type or index is invalid");
                        arrayElements.Add(item.Id);
                    }
                    break;
                case "array-element":
                    if (!arrayElements.Contains(node.Id)) Fail("Array element must be an array initialization child");
                    EdgeShape(node, ("value", 1, 1)); Expr(node, "value");
                    SameType(node, Child(node, "value"));
                    break;
                case "array-decay":
                    EdgeShape(node, ("operand", 1, 1)); Expr(node, "operand");
                    if (!Place(Child(node, "operand")) || !ScalarTypes.IsArray(Type(Child(node, "operand"))) ||
                        Type(node) != "ptr(" + ScalarTypes.Element(Type(Child(node, "operand"))) + ")")
                        Fail("Array decay requires an array place and pointer to its element");
                    break;
                case "address":
                    EdgeShape(node, ("operand", 1, 1)); Expr(node, "operand");
                    if (!Place(Child(node, "operand")) || Type(node) != "ptr(" + Type(Child(node, "operand")) + ")")
                        Fail("Address requires a typed place and matching pointer result");
                    if (PlaceLeaf(Child(node, "operand")) is { Kind: "member", FieldId: long addressField } && obj.Fields[addressField].BitWidth is not null)
                        Fail("Bitfield has no independent address");
                    break;
                case "dereference":
                    EdgeShape(node, ("operand", 1, 1)); Expr(node, "operand");
                    if (!ScalarTypes.IsPointer(Type(Child(node, "operand"))) || (Type(node) == "void" || RoutineTypes.IsRoutine(Type(node))) ||
                        Type(Child(node, "operand")) != "ptr(" + Type(node) + ")")
                        Fail("Dereference requires a nonvoid typed pointer and matching place result");
                    break;
                case "call":
                    EdgeShape(node, ("argument", 0, int.MaxValue));
                    if (!obj.Entities.TryGetValue(node.EntityId!.Value, out Entity? target)) Fail("Unknown call target");
                    if (obj.DeferredFunctionDeclarations.ContainsKey(target!.Id)) Fail("Direct call requires a completed original routine ABI");
                    if (obj.Type(target.ReturnTypeId) != Type(node)) Fail("Call return type mismatch");
                    var args = body.Children(node.Id, "argument").Select(edge => body.Nodes[edge.ChildId]).ToArray();
                    var expected = obj.Parameters.GetValueOrDefault(target.Id, []);
                    bool variadic = obj.Routines[target.RoutineTypeId].Variadic;
                    if ((variadic ? args.Length < expected.Count : args.Length != expected.Count) ||
                        args.Take(expected.Count).Where((arg, i) => !IsExpression(arg) || Type(arg) != expected[i]).Any() ||
                        args.Skip(expected.Count).Any(arg => !IsExpression(arg) || !(ScalarTypes.IsPromotedInteger(Type(arg)) || Type(arg) is "f64" or "f80" || ScalarTypes.IsPointer(Type(arg)))))
                        Fail("Call argument signature or default variadic promotion mismatch at node " + node.Id + " calling " + target.SourceName + " (" + (target.ExternalKey ?? "internal:" + target.Id) + "): expected [" + string.Join(", ", expected) + (variadic ? "; ..." : "") + "], actual [" + string.Join(", ", args.Select(arg => arg.TypeId is null ? "<statement:" + arg.Kind + ">" : Type(arg))) + "]");
                    break;
                case "binary":
                    EdgeShape(node, ("left", 1, 1), ("right", 1, 1)); Expr(node, "left"); Expr(node, "right");
                    string operation = node.Operator!;
                    if (operation is "pointer-add" or "pointer-subtract")
                    {
                        bool pointerFirst = Type(Child(node, "left")) == Type(node) && ScalarTypes.IsInteger(Type(Child(node, "right")));
                        bool pointerLast = operation == "pointer-add" && Type(Child(node, "right")) == Type(node) && ScalarTypes.IsInteger(Type(Child(node, "left")));
                        if (!ScalarTypes.IsPointer(Type(node)) || (ScalarTypes.Pointee(Type(node)) == "void" || RoutineTypes.IsFunctionPointer(Type(node))) || !(pointerFirst || pointerLast))
                            Fail("Pointer arithmetic requires a nonvoid pointer and integer displacement");
                        if (ScalarTypes.IsIncompleteArray(ScalarTypes.Pointee(Type(node)))) Fail("Pointer arithmetic requires a known array bound");
                    }
                    else if (operation == "pointer-difference")
                    {
                        if (Type(node) != "i64" || !ScalarTypes.IsPointer(Type(Child(node, "left"))) ||
                            (ScalarTypes.Pointee(Type(Child(node, "left"))) == "void" || RoutineTypes.IsFunctionPointer(Type(Child(node, "left")))))
                            Fail("Pointer difference requires matching nonvoid pointers and i64 result");
                        SameType(Child(node, "left"), Child(node, "right"));
                        if (ScalarTypes.IsAddressOnlyArray(ScalarTypes.Pointee(Type(Child(node, "left"))))) Fail("Pointer difference requires a positive element extent");
                    }
                    else if (operation is "add" or "sub" or "mul" or "div" or "rem" or "bit-and" or "bit-or" or "bit-xor")
                    {
                        bool integerOnly = operation is "rem" or "bit-and" or "bit-or" or "bit-xor";
                        if (integerOnly ? !ScalarTypes.IsPromotedInteger(Type(node)) : !ScalarTypes.IsPromotedNumeric(Type(node)))
                            Fail(integerOnly ? "Integer operation requires promoted integer operands and result" : "Arithmetic requires promoted numeric operands and result");
                        SameType(node, Child(node, "left")); SameType(node, Child(node, "right"));
                    }
                    else if (operation is "shl" or "shr")
                    {
                        if (!ScalarTypes.IsPromotedInteger(Type(node)) || !ScalarTypes.IsPromotedInteger(Type(Child(node, "right"))))
                            Fail("Shift requires promoted integer operands");
                        SameType(node, Child(node, "left"));
                    }
                    else if (operation is "eq" or "ne" or "lt" or "le" or "gt" or "ge")
                    {
                        SameType(Child(node, "left"), Child(node, "right"));
                        if (!ScalarTypes.IsScalar(Type(Child(node, "left")))) Fail("Comparison requires scalar operands");
                        if (Type(Child(node, "left")) == "bool" && operation is not ("eq" or "ne")) Fail("Relational bool operands require explicit promotion");
                        if (!ScalarTypes.IsBooleanResult(Type(node))) Fail("Invalid comparison result type");
                    }
                    else if (operation is "and" or "or")
                    {
                        Condition(node, "left"); Condition(node, "right");
                        if (!ScalarTypes.IsBooleanResult(Type(node))) Fail("Invalid logical result type");
                    }
                    else Fail($"Unsupported binary operator {operation}");
                    break;
                case "unary":
                    EdgeShape(node, ("operand", 1, 1)); Expr(node, "operand");
                    if (node.Operator is "neg" or "plus" or "bit-not")
                    {
                        if (node.Operator == "bit-not" ? !ScalarTypes.IsPromotedInteger(Type(node)) : !ScalarTypes.IsPromotedNumeric(Type(node)))
                            Fail("Unary arithmetic requires a promoted numeric type; bit-not requires integer");
                        SameType(node, Child(node, "operand"));
                    }
                    else if (node.Operator == "not")
                    {
                        Condition(node, "operand");
                        if (!ScalarTypes.IsBooleanResult(Type(node))) Fail("Invalid logical result type");
                    }
                    else Fail("Unsupported unary operator");
                    break;
                case "conditional":
                    EdgeShape(node, ("condition", 1, 1), ("then", 1, 1), ("else", 1, 1));
                    Condition(node, "condition"); Expr(node, "then", Type(node) == "void"); Expr(node, "else", Type(node) == "void");
                    if (Type(node) != "void" && !ScalarTypes.IsScalar(Type(node)) && !(ScalarTypes.IsRecord(Type(node)) && obj.CanTransportRecord(Type(node)))) Fail("Conditional requires void, scalar or copyable record result");
                    SameType(node, Child(node, "then")); SameType(node, Child(node, "else"));
                    break;
                case "block-copy":
                    EdgeShape(node, ("target", 1, 1), ("source", 1, 1)); Expr(node, "target"); Expr(node, "source");
                    if (Type(node) != "void" || !Place(Child(node, "target")) || !Place(Child(node, "source")) ||
                        Child(node, "target").Kind == "volatile-place" || Child(node, "source").Kind == "volatile-place" ||
                        DeclaredVolatile(Child(node, "target")) || DeclaredVolatile(Child(node, "source")) ||
                        !obj.CanCopyPayload(Type(Child(node, "source"))) || !obj.CanCopyPayload(Type(Child(node, "target"))) ||
                        !long.TryParse(node.ConstantText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out long extent) ||
                        extent <= 0 || node.ConstantText != extent.ToString(System.Globalization.CultureInfo.InvariantCulture) ||
                        extent != obj.StorageSize(Type(Child(node, "source"))) || extent > obj.StorageSize(Type(Child(node, "target"))))
                        Fail("Block copy requires nonvolatile places and the exact bounded source extent");
                    break;
                case "compound-assign":
                    EdgeShape(node, ("target", 1, 1), ("value", 1, 1)); Expr(node, "target"); Expr(node, "value");
                    if (!Place(Child(node, "target")) || Type(node) != Type(Child(node, "target")) ||
                        node.Operator is not ("add" or "sub" or "mul" or "div" or "rem" or "bit-and" or "bit-or" or "bit-xor" or "shl" or "shr" or "pointer-add" or "pointer-subtract"))
                        Fail("Invalid compound assignment target or operator");
                    if (node.Operator is "pointer-add" or "pointer-subtract")
                    {
                        if (!ScalarTypes.IsPointer(Type(node)) || RoutineTypes.IsFunctionPointer(Type(node)) || ScalarTypes.Pointee(Type(node)) == "void" ||
                            !ScalarTypes.IsInteger(Type(Child(node, "value")))) Fail("Invalid compound pointer arithmetic");
                        if (ScalarTypes.IsIncompleteArray(ScalarTypes.Pointee(Type(node)))) Fail("Compound pointer arithmetic requires a known array bound");
                    }
                    else if (!ScalarTypes.IsNumeric(Type(node)) || !ScalarTypes.IsNumeric(Type(Child(node, "value")))) Fail("Compound arithmetic requires numeric operands");
                    if (node.Operator is "rem" or "bit-and" or "bit-or" or "bit-xor" or "shl" or "shr" &&
                        (!ScalarTypes.IsInteger(Type(node)) || !ScalarTypes.IsInteger(Type(Child(node, "value"))))) Fail("Compound integer operator requires integer operands");
                    break;
                case "record-constant-init":
                    EdgeShape(node, ("target", 1, 1), ("value", 1, 1)); Expr(node, "target"); Expr(node, "value");
                    if (!obj.RecordConstantInitializations.TryGetValue(node.Id, out RecordConstantInitialization? constantInitialization) ||
                        !Place(Child(node, "target")) || Child(node, "value").Kind != "storage" ||
                        Child(node, "value").StorageId != constantInitialization.StorageId || DeclaredVolatile(Child(node, "target")))
                        Fail("Record constant initialization requires its exact constant storage and nonvolatile target place");
                    SameType(node, Child(node, "target")); SameType(node, Child(node, "value")); break;
                case "record-transfer":
                    EdgeShape(node, ("target", 1, 1), ("value", 1, 1)); Expr(node, "target"); Expr(node, "value");
                    string? transferKind = obj.RecordValueTransfers.GetValueOrDefault(node.Id);
                    bool storageCopy = obj.RecordStorageCopies.TryGetValue(node.Id, out RecordStorageCopy? storageCopyProof);
                    if (!ScalarTypes.IsRecord(Type(node)) || !Place(Child(node, "target")) ||
                        !(storageCopy || transferKind is not null && obj.CanTransferRecord(Type(node), transferKind)))
                        Fail("Record transfer requires its observed construction, temporary-copy or lowered storage-copy permission");
                    if (storageCopy && (Child(node, "target").Id != storageCopyProof!.TargetNodeId ||
                        Child(node, "value").Id != storageCopyProof.ValueNodeId ||
                        DeclaredVolatile(Child(node, "target")) || DeclaredVolatile(Child(node, "value"))))
                        Fail("Lowered record storage copy changed its observed nonvolatile target/value");
                    if ((transferKind == "temporary" || storageCopy && storageCopyProof!.Kind == "evaluation-save") && (Child(node, "target").Kind != "binding" ||
                        !body.Bindings.TryGetValue(Child(node, "target").BindingId!.Value, out Binding? temporary) ||
                        temporary.Kind != "local" || !temporary.IsSynthetic))
                        Fail("Observed temporary transfer requires a generated local destination");
                    SameType(node, Child(node, "target")); SameType(node, Child(node, "value")); break;
                case "assign":
                    EdgeShape(node, ("target", 1, 1), ("value", 1, 1)); Expr(node, "target"); Expr(node, "value");
                    if (!Place(Child(node, "target"))) Fail("Assignment target must be a typed place");
                    if (ScalarTypes.IsRecord(Type(node)) && Child(node, "value").Kind != "default-value" && !obj.CanCopyRecord(Type(node)))
                        Fail("Record assignment requires a complete record without incomplete arrays or volatile fields");
                    SameType(node, Child(node, "target")); SameType(node, Child(node, "value")); break;
                case "mutate":
                    EdgeShape(node, ("operand", 1, 1)); Expr(node, "operand");
                    if (node.Operator is not ("pre-inc" or "post-inc" or "pre-dec" or "post-dec") ||
                        !Place(Child(node, "operand")) ||
                        !(ScalarTypes.IsNumeric(Type(node)) || ScalarTypes.IsPointer(Type(node)) && ScalarTypes.Pointee(Type(node)) != "void" && !RoutineTypes.IsFunctionPointer(Type(node))))
                        Fail("Increment/decrement requires a numeric or nonvoid pointer place");
                    SameType(node, Child(node, "operand"));
                    if (ScalarTypes.IsPointer(Type(node)) && ScalarTypes.IsIncompleteArray(ScalarTypes.Pointee(Type(node))))
                        Fail("Pointer increment requires a known array bound");
                    break;
                case "convert":
                    EdgeShape(node, ("operand", 1, 1)); Expr(node, "operand", Type(node) == "void");
                    if (Type(node) == "void") break;
                    if (!ScalarTypes.IsScalar(Type(node)) || !ScalarTypes.IsScalar(Type(Child(node, "operand"))))
                        Fail("Conversion requires scalar types");
                    if (ScalarTypes.IsPointer(Type(node)) && ScalarTypes.IsFloating(Type(Child(node, "operand"))) ||
                        ScalarTypes.IsFloating(Type(node)) && ScalarTypes.IsPointer(Type(Child(node, "operand"))))
                        Fail("Pointer and floating-point conversion requires an explicit integer intermediate");
                    if (ScalarTypes.IsPointer(Type(node)) && Type(Child(node, "operand")) == "bool")
                        Fail("Boolean-to-pointer conversion requires an explicit integer conversion");
                    break;
            }
            var nestedControls = node.Kind is "while" or "for" or "do-while" or "switch" ? new List<long>(controls) { node.Id } : controls;
            foreach (Edge edge in body.Edges.GetValueOrDefault(node.Id, []).Where(edge => node.Kind != "case" || edge.Role != "target").OrderBy(e => e.Ordinal))
                Visit(body.Nodes[edge.ChildId], scopes, visible, nestedControls, depth + 1, DiscardsChild(node, edge, discardedResult));
            if (node.Kind == "throw") activeThrows.Pop();
        }
        foreach (var edges in body.Edges.Values)
            foreach (Edge edge in edges)
                if (!body.Nodes.ContainsKey(edge.ParentId) || !body.Nodes.ContainsKey(edge.ChildId)) Fail("Edge leaves function body");
        Visit(root!, [], parameters.Select(parameter => parameter.Id).ToHashSet(), [], 0);
        obj.VerifyArrayAllocationBody(function.Id, body);
        obj.VerifyGeneratedArrayInitializerBody(function.Id, body);
        obj.VerifyArrayDeletionBody(function.Id, body);
        foreach (ArrayRuntimeOperation operation in obj.ArrayRuntimeOperations.Values.Where(operation => body.Nodes.ContainsKey(operation.NodeId)))
        {
            Node call = body.Nodes[operation.NodeId];
            string method = RuntimeArrayProvider.TryGetMethod(new Symbol(obj, obj.Entities[call.EntityId!.Value]))!;
            int callbackOrdinal = method == "CopyConstruct" ? 5 : method == "Construct" ? 4 : 3;
            Node callback = body.Nodes[body.Children(call.Id, "argument").Single(edge => edge.Ordinal == callbackOrdinal).ChildId];
            while (callback.Kind == "convert") callback = body.Child(callback.Id, "operand");
            if (callback.Kind != "function-address" || callback.EntityId != operation.CallbackEntityId)
                Fail("Array runtime destructor argument differs from its observed original destruction obligation");
            long? fieldInitializerId = operation.SourceFieldInitializerId;
            if (operation.SourceStaticObligationId is long templateId &&
                obj.SourceStaticDestructions[templateId].SourceInitializerId is long sourceInitializer &&
                obj.SourceInitializers[sourceInitializer].Kind == "field-default")
                fieldInitializerId = sourceInitializer;
            if (fieldInitializerId is long fieldInitializer)
            {
                Node destination = body.Nodes[body.Children(call.Id, "argument").Single(edge => edge.Ordinal == 0).ChildId];
                CleanupVerifier.FieldInitializerTarget(obj, function, body, fieldInitializer, destination);
            }
        }
        var fieldDestructions = obj.NormalFieldDestructions.Concat(obj.NormalStaticDestructions
            .Where(value => obj.SourceStaticDestructions[value.Value].SourceInitializerId is long initializer &&
                obj.SourceInitializers[initializer].Kind == "field-default")
            .Select(value => new KeyValuePair<long, long>(value.Key, obj.SourceStaticDestructions[value.Value].SourceInitializerId!.Value)));
        foreach (var destruction in fieldDestructions.Where(value => body.Nodes.ContainsKey(value.Key)))
        {
            Node call = body.Nodes[destruction.Key];
            Node receiver = body.Nodes[body.Children(call.Id, "argument").Single(edge => edge.Ordinal == 0).ChildId];
            CleanupVerifier.FieldInitializerTarget(obj, function, body, destruction.Value, receiver);
        }
        var actionBindings = new Dictionary<long, HashSet<long>>();
        foreach (CleanupAction action in obj.FunctionCleanupActions.GetValueOrDefault(function.Id, []))
        {
            HashSet<long> captures = CleanupVerifier.TargetBindings(obj, function, body, action);
            // Only the validated pure recipe's actual bindings are visible in
            // this additional root. Arming sites below must see every capture.
            visitingCleanupTarget = true;
            Visit(body.Nodes[action.TargetNodeId], captures.Select(id => body.Bindings[id].ScopeNodeId).Distinct().ToList(), captures, [], 0);
            visitingCleanupTarget = false;
            if (obj.CleanupConditions.TryGetValue(action.Id, out long flag))
            {
                if (!body.Bindings.TryGetValue(flag, out Binding? binding) || binding.FunctionId != function.Id ||
                    binding.Kind != "local" || !binding.IsSynthetic || obj.Type(binding.TypeId) != "i32")
                    Fail("Conditional cleanup flag is not an owned synthetic integer local");
                captures.Add(flag);
            }
            if (obj.CleanupVttArguments.TryGetValue(action.Id, out long vtt))
            {
                if (!body.Bindings.TryGetValue(vtt, out Binding? binding) || binding.FunctionId != function.Id ||
                    !ScalarTypes.IsPointer(obj.Type(binding.TypeId)))
                    Fail("Subobject cleanup VTT argument is not an owned pointer binding");
                captures.Add(vtt);
            }
            actionBindings.Add(action.Id, captures);
            body.CleanupCapturedBindings.UnionWith(captures);
        }
        foreach (var action in obj.AllocationCleanupActions.Values.Where(action => action.FunctionEntityId == function.Id)
            .Select(action => (action.Id, action.TargetNodeId, action.ConditionBindingId, Guard: false))
            .Concat(obj.GuardCleanupActions.Values.Where(action => action.FunctionEntityId == function.Id)
                .Select(action => (action.Id, action.TargetNodeId, ConditionBindingId: (long?)null, Guard: true))))
        {
            HashSet<long> captures = CleanupVerifier.RollbackTargetBindings(obj, function, body, action.TargetNodeId, action.Guard);
            visitingCleanupTarget = true;
            Visit(body.Nodes[action.TargetNodeId], captures.Select(id => body.Bindings[id].ScopeNodeId).Distinct().ToList(), captures, [], 0);
            visitingCleanupTarget = false;
            if (action.ConditionBindingId is long condition) captures.Add(condition);
            actionBindings.Add(action.Id, captures);
            body.CleanupCapturedBindings.UnionWith(captures);
        }
        foreach (var (nodeId, visible) in cleanupVisibility)
        {
            CleanupTransition transition = obj.CleanupTransitions[nodeId];
            if (transition.IsUnreachable) continue;
            for (long? stateId = transition.StateId; stateId is long id; stateId = obj.CleanupStates[id].NextStateId)
            {
                if (obj.CleanupStates[id].HandlerNodeId is long handlerId)
                {
                    if (!cleanupHandlerVisibility[nodeId].Contains(handlerId)) Fail("Release state is armed outside its owning handler");
                    continue;
                }
                long actionId = (obj.CleanupStates[id].ActionId ?? obj.CleanupStates[id].AllocationActionId ?? obj.CleanupStates[id].GuardActionId)!.Value;
                if (!actionBindings[actionId].IsSubsetOf(visible))
                    Fail("Cleanup state arms a target outside its live declaration scope");
                if (obj.CleanupPayloadOwners.TryGetValue(actionId, out long throwId) && !cleanupThrowVisibility[nodeId].Contains(throwId))
                    Fail("Cleanup state arms payload rollback outside its owning throw initializer");
            }
        }
        if (obj.FunctionCleanupStates.ContainsKey(function.Id))
        {
            Node[] prologues = body.Nodes.Values.Where(node => node.Kind == "cleanup-frame" && obj.CleanupFrameMarkers[node.Id].Kind == "prologue").ToArray();
            if (prologues.Length != 1 || !body.Children(body.RootId, "statement").Any(edge =>
                body.Nodes[edge.ChildId].Kind == "expr-stmt" && body.Child(edge.ChildId, "value").Id == prologues[0].Id))
                Fail("Cleanup prologue must be an immediate statement of its owned function body");
        }
        obj.VerifyThunkWrapperBody(function, body);
        obj.VerifyLambdaEntryBody(function, body);
        var accessAlignment = new AccessAlignment(obj, body);
        var parents = body.Edges.Values.SelectMany(edges => edges)
            .Where(edge => body.Nodes[edge.ParentId].TypeId is not null || edge.Role != "target")
            .ToLookup(edge => edge.ChildId);
        foreach (Node node in body.Nodes.Values)
        {
            if (node.Kind is "base-subobject" or "virtual-base-subobject" && (parents[node.Id].Count() != 1 || parents[node.Id].Any(edge =>
                body.Nodes[edge.ParentId].Kind != "address" || edge.Role != "operand")))
                Fail("Zero-extent base subobject is an address-only view, not a complete object value");
            if (node.Kind == "volatile-place" && accessAlignment.Place(node) < obj.AccessAlignment(Type(node)))
                Fail("Underaligned volatile access requires an explicit volatile alignment contract");
            if (node.Kind == "intrinsic" && node.Operator?.EndsWith("-volatile", StringComparison.Ordinal) == true)
                foreach (Edge argument in body.Children(node.Id, "argument"))
                {
                    Node value = body.Nodes[argument.ChildId];
                    if (ScalarTypes.IsPointer(Type(value)) && accessAlignment.Pointer(value) < obj.AccessAlignment(ScalarTypes.Pointee(Type(value))))
                        Fail("Underaligned volatile intrinsic destination is unsupported");
                }
            if (node.Kind is not ("address" or "array-decay")) continue;
            string pointed = ScalarTypes.Pointee(Type(node));
            if (pointed == "void" || RoutineTypes.IsRoutine(pointed) || accessAlignment.Pointer(node) >= obj.AccessAlignment(pointed)) continue;
            void CheckEscape(Node pointer, int depth)
            {
                if (depth > 128) Fail("Excessively nested underaligned address expression");
                foreach (Edge use in parents[pointer.Id])
                {
                    Node consumer = body.Nodes[use.ParentId];
                    if (consumer.Kind == "lifetime-scope") { CheckEscape(consumer, depth + 1); continue; }
                    if (consumer.Kind == "sequence")
                    {
                        if (SequenceTail(consumer).Id == pointer.Id) CheckEscape(consumer, depth + 1);
                        continue;
                    }
                    if (consumer.Kind == "dereference" || consumer.Kind == "memory-barrier" && use.Role == "address") continue;
                    if (consumer.Kind == "convert" && ScalarTypes.IsPointer(Type(consumer)))
                    {
                        string element = ScalarTypes.Pointee(Type(consumer));
                        if (element is "void" or "i8" or "u8") continue;
                        CheckEscape(consumer, depth + 1); continue;
                    }
                    if (consumer.Kind == "binary" && consumer.Operator is "pointer-add" or "pointer-subtract" || consumer.Kind == "conditional")
                    { CheckEscape(consumer, depth + 1); continue; }
                    if (consumer.Kind == "binary" && consumer.Operator is "eq" or "ne" or "lt" or "le" or "gt" or "ge" or "pointer-difference") continue;
                    Fail($"Underaligned typed address {node.Id} ({Type(node)}, alignment {accessAlignment.Pointer(node)}) escapes through {pointer.Id} to {consumer.Id} ({consumer.Kind}, {use.Role}); use byte/void memory access or an explicit alignment-aware ABI");
                }
            }
            CheckEscape(node, 0);
        }
        if (jumpScopes.Count != 0 && cleanupBindings.Count != 0) Fail("Source gotos with lexical cleanup require explicit lifetime exits");
        foreach (var (jump, scopes) in jumpScopes)
        {
            long target = Child(body.Nodes[jump], "target").Id;
            if (!labelScopes.TryGetValue(target, out long[]? targetScopes)) Fail("Goto target has no owned label");
            if (targetScopes!.Length > scopes.Length || !targetScopes.SequenceEqual(scopes.Take(targetScopes.Length)))
                body.RequiresFlatControl = true;
        }
        if (body.RequiresFlatControl && cleanupBindings.Count != 0) Fail("Entries into source regions with lexical cleanup require explicit lifetime exits");
        if (body.RequiresFlatControl && body.Nodes.Values.Any(node => node.Kind == "try"))
            Fail("Flat control flow across source exception regions requires explicit protected-region transfers");
        if (visited.Count != body.Nodes.Count) Fail("Orphan executable nodes");
        if (declarations.Count != body.Bindings.Values.Count(binding => binding.Kind == "local")) Fail("Local has no declaration");
    }
}
