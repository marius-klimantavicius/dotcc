namespace DotObj;

internal static class Verifier
{
    private static readonly HashSet<string> StatementKinds = ["block", "return", "expr-stmt", "declare", "if", "while", "for", "do-while", "switch", "case", "break", "continue", "array-init", "array-element"];
    private static readonly HashSet<string> ExpressionKinds = ["constant", "binding", "call", "binary", "unary", "assign", "convert", "mutate", "conditional", "address", "dereference", "array-decay"];

    public static void Body(ObjectFile obj, Entity function, FunctionBody body)
    {
        void Fail(string message) => throw new InvalidDataException($"{obj.Path}: {function.SourceName}: {message}");
        if (!body.Nodes.TryGetValue(body.RootId, out Node? root) || root.Kind != "block") Fail("Function root must be a block");
        var visited = new HashSet<long>();
        var declarations = new HashSet<long>();
        var switchCases = new HashSet<long>();
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
                if (item.Depth > 1024 || !seen.Add(item.Node.Id) || item.Node.Kind is "call" or "assign" or "mutate") return false;
                foreach (Edge edge in body.Edges.GetValueOrDefault(item.Node.Id, []))
                    pending.Push((body.Nodes[edge.ChildId], item.Depth + 1));
            }
            return true;
        }
        bool Place(Node node) => node.Kind is "binding" or "dereference";
        // An address rooted in owned stack storage with constant offsets cannot
        // change while the RHS runs, even if the RHS calls or mutates values.
        bool StablePlace(Node node, int depth = 0) => depth <= 1024 &&
            (node.Kind == "binding" || node.Kind == "dereference" && StablePointer(Child(node, "operand"), depth + 1));
        bool StablePointer(Node node, int depth = 0)
        {
            if (depth > 1024) return false;
            if (node.Kind == "constant" && ScalarTypes.IsPointer(Type(node))) return true;
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
                if (type == "void" && node.Kind != "call") Fail("Only calls can have void type");
                if (node.ValueCategory != (Place(node) ? "place" : "value")) Fail("Incorrect value category");
                string[] orders = node.Kind is "constant" or "binding" ? ["none"] : ["left-to-right", "unspecified"];
                if (node.Kind == "assign") orders = ["left-to-right", "right-to-left", "unspecified"];
                if (node.Kind is "address" or "dereference" or "array-decay") orders = ["none", "left-to-right", "unspecified"];
                if (!orders.Contains(node.EvaluationOrder)) Fail($"Unsupported evaluation order at {node.Id}");
            }
            if ((node.EntityId != null) != (node.Kind == "call") ||
                (node.BindingId != null) != (node.Kind is "binding" or "declare") ||
                (node.Operator != null) != (node.Kind is "binary" or "unary" or "mutate") ||
                (node.ConstantText != null) != (node.Kind is "constant" or "array-element")) Fail("Unexpected or missing typed payload");

            if (IsExpression(node) && ScalarTypes.IsArray(Type(node)) && !Place(node)) Fail("Array value must be a place");
            foreach (Edge edge in body.Edges.GetValueOrDefault(node.Id, []))
            {
                Node child = body.Nodes[edge.ChildId];
                if (child.TypeId is long childType && ScalarTypes.IsArray(obj.Type(childType)) &&
                    !(node.Kind is "address" or "array-decay" && edge.Role == "operand") &&
                    !(node.Kind == "array-init" && edge.Role == "target"))
                    Fail("Array place requires explicit address, decay or initialization");
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
                case "if":
                    EdgeShape(node, ("condition", 1, 1), ("then", 1, 1), ("else", 0, 1));
                    Expr(node, "condition"); Statement(Child(node, "then"));
                    if (body.Children(node.Id, "else").Any()) Statement(Child(node, "else"));
                    break;
                case "while":
                case "do-while":
                    EdgeShape(node, ("condition", 1, 1), ("body", 1, 1));
                    Expr(node, "condition"); Statement(Child(node, "body")); break;
                case "for":
                    EdgeShape(node, ("initializer", 0, 1), ("condition", 0, 1), ("step", 0, 1), ("body", 1, 1));
                    foreach (string role in new[] { "initializer", "condition", "step" })
                        if (body.Children(node.Id, role).Any()) Expr(node, role, role != "condition");
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
                    foreach (Edge edge in body.Children(switchBody.Id, "statement"))
                    {
                        Node item = body.Nodes[edge.ChildId];
                        if (item.Kind == "case")
                        {
                            labeled = true;
                            switchCases.Add(item.Id);
                            EdgeShape(item, ("value", 0, 1));
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
                        else if (item.Kind == "declare")
                        {
                            if (labeled || body.Children(item.Id, "initializer").Any())
                                Fail("Switch declarations must precede labels and have no initializer");
                        }
                        else if (!labeled) Fail("Executable statement before first switch case");
                    }
                    break;
                case "case":
                    if (!switchCases.Contains(node.Id)) Fail("Case label is not an immediate switch body child");
                    EdgeShape(node, ("value", 0, 1));
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
                case "constant":
                    EdgeShape(node);
                    if (!ScalarTypes.IsConstant(Type(node), node.ConstantText)) Fail("Invalid scalar constant");
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
                    if (arrayTarget.Kind != "binding" || !ScalarTypes.IsArray(Type(arrayTarget)) ||
                        !body.Bindings.TryGetValue(arrayTarget.BindingId!.Value, out Binding? arrayBinding) || arrayBinding.Kind != "local")
                        Fail("Array initialization requires a local array binding");
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
                    if (!ScalarTypes.IsPointer(Type(Child(node, "operand"))) || Type(node) == "void" ||
                        Type(Child(node, "operand")) != "ptr(" + Type(node) + ")")
                        Fail("Dereference requires a nonvoid typed pointer and matching place result");
                    break;
                case "call":
                    EdgeShape(node, ("argument", 0, int.MaxValue));
                    if (!obj.Entities.TryGetValue(node.EntityId!.Value, out Entity? target)) Fail("Unknown call target");
                    if (obj.Type(target!.ReturnTypeId) != Type(node)) Fail("Call return type mismatch");
                    var args = body.Children(node.Id, "argument").Select(edge => body.Nodes[edge.ChildId]).ToArray();
                    var expected = obj.Parameters.GetValueOrDefault(target.Id, []);
                    if (args.Length != expected.Count || args.Where((arg, i) => !IsExpression(arg) || Type(arg) != expected[i]).Any()) Fail("Call argument signature mismatch");
                    break;
                case "binary":
                    EdgeShape(node, ("left", 1, 1), ("right", 1, 1)); Expr(node, "left"); Expr(node, "right");
                    string operation = node.Operator!;
                    if (operation is "pointer-add" or "pointer-subtract")
                    {
                        bool pointerFirst = Type(Child(node, "left")) == Type(node) && ScalarTypes.IsPromotedInteger(Type(Child(node, "right")));
                        bool pointerLast = operation == "pointer-add" && Type(Child(node, "right")) == Type(node) && ScalarTypes.IsPromotedInteger(Type(Child(node, "left")));
                        if (!ScalarTypes.IsPointer(Type(node)) || ScalarTypes.Pointee(Type(node)) == "void" || !(pointerFirst || pointerLast))
                            Fail("Pointer arithmetic requires a nonvoid pointer and promoted integer displacement");
                    }
                    else if (operation == "pointer-difference")
                    {
                        if (Type(node) != "i64" || !ScalarTypes.IsPointer(Type(Child(node, "left"))) ||
                            ScalarTypes.Pointee(Type(Child(node, "left"))) == "void")
                            Fail("Pointer difference requires matching nonvoid pointers and i64 result");
                        SameType(Child(node, "left"), Child(node, "right"));
                    }
                    else if (operation is "add" or "sub" or "mul" or "div" or "rem" or "bit-and" or "bit-or" or "bit-xor")
                    {
                        if (!ScalarTypes.IsPromotedInteger(Type(node))) Fail("Arithmetic requires promoted integer operands and result");
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
                        if (Type(Child(node, "left")) == "bool" && operation is not ("eq" or "ne")) Fail("Relational bool operands require explicit promotion");
                        if (!ScalarTypes.IsBooleanResult(Type(node))) Fail("Invalid comparison result type");
                    }
                    else if (operation is "and" or "or")
                    {
                        if (!ScalarTypes.IsBooleanResult(Type(node))) Fail("Invalid logical result type");
                    }
                    else Fail($"Unsupported binary operator {operation}");
                    break;
                case "unary":
                    EdgeShape(node, ("operand", 1, 1)); Expr(node, "operand");
                    if (node.Operator is "neg" or "plus" or "bit-not")
                    {
                        if (!ScalarTypes.IsPromotedInteger(Type(node))) Fail("Unary arithmetic requires a promoted integer");
                        SameType(node, Child(node, "operand"));
                    }
                    else if (node.Operator == "not")
                    {
                        if (!ScalarTypes.IsBooleanResult(Type(node))) Fail("Invalid logical result type");
                    }
                    else Fail("Unsupported unary operator");
                    break;
                case "conditional":
                    EdgeShape(node, ("condition", 1, 1), ("then", 1, 1), ("else", 1, 1));
                    Expr(node, "condition"); Expr(node, "then"); Expr(node, "else");
                    SameType(node, Child(node, "then")); SameType(node, Child(node, "else"));
                    break;
                case "assign":
                    EdgeShape(node, ("target", 1, 1), ("value", 1, 1)); Expr(node, "target"); Expr(node, "value");
                    if (!Place(Child(node, "target"))) Fail("Assignment target must be a typed place");
                    if (node.EvaluationOrder == "right-to-left" && !StablePlace(Child(node, "target")) &&
                        (!Pure(Child(node, "target")) || !Pure(Child(node, "value"))))
                        Fail("Indirect RHS-before-LHS assignment requires a stable address or effect-free operands in this profile");
                    SameType(node, Child(node, "target")); SameType(node, Child(node, "value")); break;
                case "mutate":
                    EdgeShape(node, ("operand", 1, 1)); Expr(node, "operand");
                    if (node.Operator is not ("pre-inc" or "post-inc" or "pre-dec" or "post-dec") ||
                        !Place(Child(node, "operand")) ||
                        !(ScalarTypes.IsInteger(Type(node)) || ScalarTypes.IsPointer(Type(node)) && ScalarTypes.Pointee(Type(node)) != "void"))
                        Fail("Increment/decrement requires an integer or nonvoid pointer place");
                    SameType(node, Child(node, "operand"));
                    break;
                case "convert":
                    EdgeShape(node, ("operand", 1, 1)); Expr(node, "operand");
                    if (ScalarTypes.IsPointer(Type(node)) && Type(Child(node, "operand")) == "bool")
                        Fail("Boolean-to-pointer conversion requires an explicit integer conversion");
                    break;
            }
            var nestedControls = node.Kind is "while" or "for" or "do-while" or "switch" ? new List<long>(controls) { node.Id } : controls;
            foreach (Edge edge in body.Edges.GetValueOrDefault(node.Id, []).OrderBy(e => e.Ordinal))
                Visit(body.Nodes[edge.ChildId], scopes, visible, nestedControls, depth + 1);
        }
        foreach (var edges in body.Edges.Values)
            foreach (Edge edge in edges)
                if (!body.Nodes.ContainsKey(edge.ParentId) || !body.Nodes.ContainsKey(edge.ChildId)) Fail("Edge leaves function body");
        Visit(root!, [], parameters.Select(parameter => parameter.Id).ToHashSet(), [], 0);
        if (visited.Count != body.Nodes.Count) Fail("Orphan executable nodes");
        if (declarations.Count != body.Bindings.Values.Count(binding => binding.Kind == "local")) Fail("Local has no declaration");
    }
}
