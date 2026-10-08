namespace DotObj;

internal static class Verifier
{
    private static readonly HashSet<string> StatementKinds = ["block", "return", "expr-stmt", "declare", "register-cleanup", "if", "while", "for", "do-while", "switch", "case", "break", "continue", "array-init", "array-element", "label", "goto"];
    private static readonly HashSet<string> ExpressionKinds = ["constant", "binding", "call", "binary", "unary", "assign", "convert", "mutate", "conditional", "address", "dereference", "array-decay", "member", "default-value", "storage", "storage-address", "function-address", "indirect-call", "compound-assign", "sequence", "va-start", "va-copy", "va-end", "va-arg", "volatile-place", "intrinsic"];

    public static void Body(ObjectFile obj, Entity function, FunctionBody body)
    {
        void Fail(string message) => throw new InvalidDataException($"{obj.Path}: {function.SourceName}: {message}");
        if (!body.Nodes.TryGetValue(body.RootId, out Node? root) || root.Kind != "block") Fail("Function root must be a block");
        var visited = new HashSet<long>();
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
            if (binding.Kind == "local" && binding.Ordinal != null) Fail("Local has parameter ordinal");
            if (!body.Nodes.TryGetValue(binding.ScopeNodeId, out Node? scope) || scope.Kind != "block") Fail("Binding scope is not a block");
        }

        string Type(Node node) => node.TypeId is long id ? obj.Type(id) : throw new InvalidDataException("Expression has no type");
        bool IsExpression(Node node) => ExpressionKinds.Contains(node.Kind);
        Node Child(Node parent, string role) => body.Child(parent.Id, role);
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
        bool Pure(Node node)
        {
            var pending = new Stack<(Node Node, int Depth)>();
            var seen = new HashSet<long>();
            pending.Push((node, 0));
            while (pending.TryPop(out var item))
            {
                if (item.Depth > 1024 || !seen.Add(item.Node.Id) || item.Node.Kind is "call" or "indirect-call" or "assign" or "compound-assign" or "mutate" or "volatile-place" or "va-start" or "va-copy" or "va-end" or "va-arg" || item.Node.Kind == "intrinsic" && (item.Node.Operator == "unreachable" || item.Node.Operator!.Contains("overflow", StringComparison.Ordinal))) return false;
                foreach (Edge edge in body.Edges.GetValueOrDefault(item.Node.Id, []))
                    pending.Push((body.Nodes[edge.ChildId], item.Depth + 1));
            }
            return true;
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
            "binding" => node.BindingId is long id && obj.BindingSourceTypes.TryGetValue(id, out long source) && VolatileSource(source),
            "storage" => node.StorageId is long id && obj.Storages.TryGetValue(id, out Storage? storage) && VolatileSource(storage.SourceTypeId),
            "member" => node.FieldId is long id && obj.Fields.TryGetValue(id, out RecordField? field) &&
                (VolatileSource(field.SourceTypeId) || DeclaredVolatile(Child(node, "operand"), depth + 1)),
            _ => false
        };
        bool Place(Node node) => node.Kind is "binding" or "storage" or "dereference" or "member" or "volatile-place";
        bool LocalPlace(Node node, int depth = 0) => depth <= 1024 && (node.Kind == "binding" &&
            body.Bindings.TryGetValue(node.BindingId!.Value, out Binding? local) && local.Kind == "local" ||
            node.Kind == "member" && LocalPlace(Child(node, "operand"), depth + 1));
        // An address rooted in owned stack storage with constant offsets cannot
        // change while the RHS runs, even if the RHS calls or mutates values.
        bool StablePlace(Node node, int depth = 0) => depth <= 1024 &&
            (node.Kind is "binding" or "storage" || node.Kind is "member" or "volatile-place" && StablePlace(Child(node, "operand"), depth + 1) ||
                node.Kind == "dereference" && StablePointer(Child(node, "operand"), depth + 1));
        bool StablePointer(Node node, int depth = 0)
        {
            if (depth > 1024) return false;
            if (node.Kind is "storage-address" or "function-address" || node.Kind == "constant" && ScalarTypes.IsPointer(Type(node))) return true;
            if (node.Kind is "address" or "array-decay") return StablePlace(Child(node, "operand"), depth + 1);
            if (node.Kind == "convert" && ScalarTypes.IsPointer(Type(node)) && ScalarTypes.IsPointer(Type(Child(node, "operand"))))
                return StablePointer(Child(node, "operand"), depth + 1);
            if (node.Kind == "binary" && node.Operator is "pointer-add" or "pointer-subtract")
            {
                Node left = Child(node, "left"), right = Child(node, "right");
                return right.Kind == "constant" && ScalarTypes.IsInteger(Type(right)) && StablePointer(left, depth + 1) ||
                    node.Operator == "pointer-add" && left.Kind == "constant" && ScalarTypes.IsInteger(Type(left)) && StablePointer(right, depth + 1);
            }
            return false;
        }
        void Visit(Node node, List<long> scopes, HashSet<long> visible, List<long> controls, int depth)
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
                if (type == "void" && node.Kind is not ("call" or "indirect-call" or "convert" or "sequence" or "va-start" or "va-copy" or "va-end" or "intrinsic")) Fail("Only calls and explicit void operations can have void type");
                if (node.ValueCategory != (Place(node) ? "place" : "value")) Fail("Incorrect value category");
                string[] orders = node.Kind is "constant" or "binding" or "default-value" or "storage" or "storage-address" or "function-address" or "volatile-place" ? ["none"] : ["left-to-right", "unspecified"];
                if (node.Kind is "assign" or "compound-assign") orders = ["left-to-right", "right-to-left", "unspecified"];
                if (node.Kind is "address" or "dereference" or "array-decay" or "member" or "va-start" or "va-copy" or "va-end" or "va-arg") orders = ["none", "left-to-right", "unspecified"];
                if (!orders.Contains(node.EvaluationOrder)) Fail($"Unsupported evaluation order at {node.Id}");
            }
            if ((node.EntityId != null) != (node.Kind is "call" or "register-cleanup" or "function-address") ||
                (node.BindingId != null) != (node.Kind is "binding" or "declare" or "register-cleanup") ||
                (node.FieldId != null) != (node.Kind == "member") ||
                (node.StorageId != null) != (node.Kind is "storage" or "storage-address") ||
                (node.Operator != null) != (node.Kind is "binary" or "unary" or "mutate" or "compound-assign" or "intrinsic") ||
                (node.ConstantText != null) != (node.Kind is "constant" or "array-element" or "storage-address" or "label")) Fail("Unexpected or missing typed payload");

            if (IsExpression(node) && ScalarTypes.IsArray(Type(node)) && !Place(node)) Fail("Array value must be a place");
            foreach (Edge edge in body.Edges.GetValueOrDefault(node.Id, []))
            {
                Node child = body.Nodes[edge.ChildId];
                if (child.TypeId is long volatileType && ScalarTypes.IsScalar(obj.Type(volatileType)) && DeclaredVolatile(child) &&
                    !(node.Kind is "volatile-place" or "address" && edge.Role == "operand"))
                    Fail("Declared volatile place requires an explicit volatile access wrapper");
                if (child.TypeId is long childType && ScalarTypes.IsArray(obj.Type(childType)) &&
                    !(node.Kind is "address" or "array-decay" && edge.Role == "operand") &&
                    !(node.Kind == "array-init" && edge.Role == "target"))
                    Fail("Array place requires explicit address, decay or initialization");
                if (child.TypeId is long recordType && ScalarTypes.IsRecord(obj.Type(recordType)))
                {
                    bool allowed = node.Kind is "member" or "address" && edge.Role == "operand" ||
                        node.Kind == "assign" && (edge.Role == "target" || edge.Role == "value" && child.Kind == "default-value") ||
                        node.Kind == "declare" && edge.Role == "initializer" && child.Kind == "default-value" ||
                        node.Kind == "expr-stmt" && edge.Role == "value" && child.Kind == "assign";
                    if (!allowed && !obj.CanCopyRecord(obj.Type(recordType))) Fail("Record value copies require complete records without incomplete arrays or volatile fields");
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
                case "intrinsic":
                    EdgeShape(node, ("argument", 0, int.MaxValue));
                    var intrinsicArgs = body.Children(node.Id, "argument").Select(edge => body.Nodes[edge.ChildId]).ToArray();
                    if (intrinsicArgs.Any(argument => !IsExpression(argument) || Type(argument) == "void")) Fail("Intrinsic argument must be a value");
                    string intrinsic = node.Operator!;
                    if (intrinsic == "unreachable") { if (intrinsicArgs.Length != 0 || Type(node) != "void") Fail("Invalid unreachable intrinsic"); }
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
                    }
                    else Fail("Unsupported intrinsic operation");
                    break;
                case "volatile-place":
                    EdgeShape(node, ("operand", 1, 1)); Expr(node, "operand");
                    if (!Place(Child(node, "operand")) || Child(node, "operand").Kind == "volatile-place" || !ScalarTypes.IsScalar(Type(node)))
                        Fail("Volatile place requires a scalar addressable location");
                    if (Child(node, "operand").Kind == "member" && obj.Fields[Child(node, "operand").FieldId!.Value].BitWidth is not null)
                        Fail("Volatile bitfield access requires a storage-unit contract");
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
                    if (node.EvaluationOrder != "left-to-right" || sequence.Any(item => !IsExpression(item)) || Type(node) != Type(sequence[^1]))
                        Fail("Sequence must evaluate expressions in order and retain the final type");
                    break;
                case "label":
                    EdgeShape(node); labelScopes.Add(node.Id, scopes.ToArray()); break;
                case "goto":
                    EdgeShape(node, ("target", 1, 1));
                    if (Child(node, "target").Kind != "label") Fail("Goto target must be an owned label");
                    jumpScopes.Add(node.Id, scopes.ToArray()); return;
                case "storage":
                    EdgeShape(node);
                    if (!obj.Storages.TryGetValue(node.StorageId!.Value, out Storage? storage) || Type(node) != obj.Type(storage.TypeId))
                        Fail("Storage place does not match its declaration type");
                    break;
                case "storage-address":
                    EdgeShape(node);
                    if (!obj.Storages.TryGetValue(node.StorageId!.Value, out Storage? addressStorage) || !ScalarTypes.IsPointer(Type(node)) ||
                        RoutineTypes.IsFunctionPointer(Type(node)) || !ScalarTypes.IsConstant("i64", node.ConstantText) ||
                        !long.TryParse(node.ConstantText, out long displacement) || displacement < 0 ||
                        displacement > obj.StorageSize(obj.Type(addressStorage.TypeId)))
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
                    var indirectSignature = RoutineTypes.Signature(ScalarTypes.Pointee(Type(Child(node, "operand"))));
                    var indirectArgs = body.Children(node.Id, "argument").Select(edge => body.Nodes[edge.ChildId]).ToArray();
                    if (Type(node) != indirectSignature.Result || (indirectSignature.Variadic ? indirectArgs.Length < indirectSignature.Parameters.Length : indirectArgs.Length != indirectSignature.Parameters.Length) ||
                        indirectArgs.Take(indirectSignature.Parameters.Length).Where((arg, index) => !IsExpression(arg) || Type(arg) != indirectSignature.Parameters[index]).Any() ||
                        indirectArgs.Skip(indirectSignature.Parameters.Length).Any(arg => !IsExpression(arg) || !(ScalarTypes.IsPromotedInteger(Type(arg)) || Type(arg) == "f64" || ScalarTypes.IsPointer(Type(arg)))))
                        Fail("Indirect call result or arguments disagree with the callable signature");
                    break;
                case "constant":
                    EdgeShape(node);
                    if (!ScalarTypes.IsConstant(Type(node), node.ConstantText)) Fail("Invalid scalar constant");
                    break;
                case "default-value":
                    EdgeShape(node);
                    if (!ScalarTypes.IsRecord(Type(node))) Fail("Default value requires a record type");
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
                case "array-init":
                    EdgeShape(node, ("target", 1, 1), ("element", 0, int.MaxValue));
                    Expr(node, "target");
                    Node arrayTarget = Child(node, "target");
                    if (!LocalPlace(arrayTarget) || !ScalarTypes.IsArray(Type(arrayTarget)))
                        Fail("Array initialization requires a local array binding or its named member");
                    string leaf = ScalarTypes.Leaf(Type(arrayTarget));
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
                    if (obj.Type(target!.ReturnTypeId) != Type(node)) Fail("Call return type mismatch");
                    var args = body.Children(node.Id, "argument").Select(edge => body.Nodes[edge.ChildId]).ToArray();
                    var expected = obj.Parameters.GetValueOrDefault(target.Id, []);
                    bool variadic = obj.Routines[target.RoutineTypeId].Variadic;
                    if ((variadic ? args.Length < expected.Count : args.Length != expected.Count) ||
                        args.Take(expected.Count).Where((arg, i) => !IsExpression(arg) || Type(arg) != expected[i]).Any() ||
                        args.Skip(expected.Count).Any(arg => !IsExpression(arg) || !(ScalarTypes.IsPromotedInteger(Type(arg)) || Type(arg) == "f64" || ScalarTypes.IsPointer(Type(arg)))))
                        Fail("Call argument signature or default variadic promotion mismatch");
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
                    }
                    else if (operation == "pointer-difference")
                    {
                        if (Type(node) != "i64" || !ScalarTypes.IsPointer(Type(Child(node, "left"))) ||
                            (ScalarTypes.Pointee(Type(Child(node, "left"))) == "void" || RoutineTypes.IsFunctionPointer(Type(Child(node, "left")))))
                            Fail("Pointer difference requires matching nonvoid pointers and i64 result");
                        SameType(Child(node, "left"), Child(node, "right"));
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
                    Condition(node, "condition"); Expr(node, "then"); Expr(node, "else");
                    if (!ScalarTypes.IsScalar(Type(node)) && !(ScalarTypes.IsRecord(Type(node)) && obj.CanCopyRecord(Type(node)))) Fail("Conditional requires a scalar or copyable record result");
                    SameType(node, Child(node, "then")); SameType(node, Child(node, "else"));
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
                    }
                    else if (!ScalarTypes.IsNumeric(Type(node)) || !ScalarTypes.IsNumeric(Type(Child(node, "value")))) Fail("Compound arithmetic requires numeric operands");
                    if (node.Operator is "rem" or "bit-and" or "bit-or" or "bit-xor" or "shl" or "shr" &&
                        (!ScalarTypes.IsInteger(Type(node)) || !ScalarTypes.IsInteger(Type(Child(node, "value"))))) Fail("Compound integer operator requires integer operands");
                    if (node.EvaluationOrder == "right-to-left" && !StablePlace(Child(node, "target")) && (!Pure(Child(node, "target")) || !Pure(Child(node, "value"))))
                        Fail("Compound assignment requires stable address for RHS-before-LHS evaluation");
                    break;
                case "assign":
                    EdgeShape(node, ("target", 1, 1), ("value", 1, 1)); Expr(node, "target"); Expr(node, "value");
                    if (!Place(Child(node, "target"))) Fail("Assignment target must be a typed place");
                    if (ScalarTypes.IsRecord(Type(node)) && Child(node, "value").Kind != "default-value" && !obj.CanCopyRecord(Type(node)))
                        Fail("Record assignment requires a complete record without incomplete arrays or volatile fields");
                    if (node.EvaluationOrder == "right-to-left" && !StablePlace(Child(node, "target")) &&
                        (!Pure(Child(node, "target")) || !Pure(Child(node, "value"))))
                        Fail("Indirect RHS-before-LHS assignment requires a stable address or effect-free operands in this profile");
                    SameType(node, Child(node, "target")); SameType(node, Child(node, "value")); break;
                case "mutate":
                    EdgeShape(node, ("operand", 1, 1)); Expr(node, "operand");
                    if (node.Operator is not ("pre-inc" or "post-inc" or "pre-dec" or "post-dec") ||
                        !Place(Child(node, "operand")) ||
                        !(ScalarTypes.IsNumeric(Type(node)) || ScalarTypes.IsPointer(Type(node)) && ScalarTypes.Pointee(Type(node)) != "void" && !RoutineTypes.IsFunctionPointer(Type(node))))
                        Fail("Increment/decrement requires a numeric or nonvoid pointer place");
                    SameType(node, Child(node, "operand"));
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
                Visit(body.Nodes[edge.ChildId], scopes, visible, nestedControls, depth + 1);
        }
        foreach (var edges in body.Edges.Values)
            foreach (Edge edge in edges)
                if (!body.Nodes.ContainsKey(edge.ParentId) || !body.Nodes.ContainsKey(edge.ChildId)) Fail("Edge leaves function body");
        Visit(root!, [], parameters.Select(parameter => parameter.Id).ToHashSet(), [], 0);
        if (jumpScopes.Count != 0 && cleanupBindings.Count != 0) Fail("Source gotos with lexical cleanup require explicit lifetime exits");
        foreach (var (jump, scopes) in jumpScopes)
        {
            long target = Child(body.Nodes[jump], "target").Id;
            if (!labelScopes.TryGetValue(target, out long[]? targetScopes)) Fail("Goto target has no owned label");
            if (targetScopes!.Length > scopes.Length || !targetScopes.SequenceEqual(scopes.Take(targetScopes.Length)))
                body.RequiresFlatControl = true;
        }
        if (body.RequiresFlatControl && cleanupBindings.Count != 0) Fail("Entries into source regions with lexical cleanup require explicit lifetime exits");
        if (visited.Count != body.Nodes.Count) Fail("Orphan executable nodes");
        if (declarations.Count != body.Bindings.Values.Count(binding => binding.Kind == "local")) Fail("Local has no declaration");
    }
}
