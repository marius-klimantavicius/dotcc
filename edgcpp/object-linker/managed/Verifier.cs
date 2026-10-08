using System.Globalization;

namespace DotObj;

internal static class Verifier
{
    private static readonly HashSet<string> StatementKinds = ["block", "return", "expr-stmt", "declare", "if", "while"];
    private static readonly HashSet<string> ExpressionKinds = ["constant", "binding", "call", "binary", "unary", "assign", "convert"];

    public static void Body(ObjectFile obj, Entity function, FunctionBody body)
    {
        void Fail(string message) => throw new InvalidDataException($"{obj.Path}: {function.SourceName}: {message}");
        if (!body.Nodes.TryGetValue(body.RootId, out Node? root) || root.Kind != "block") Fail("Function root must be a block");
        var visited = new HashSet<long>();
        var declarations = new HashSet<long>();
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
        void Visit(Node node, List<long> scopes, HashSet<long> visible, int depth)
        {
            if (depth > 1024) Fail("Graph nesting exceeds supported limit");
            if (!visited.Add(node.Id)) Fail($"Shared or cyclic executable node {node.Id}");
            if (node.FunctionId != function.Id || (!StatementKinds.Contains(node.Kind) && !IsExpression(node))) Fail($"Unsupported node {node.Kind}");
            if (StatementKinds.Contains(node.Kind))
            {
                if (node.TypeId != null || node.ValueCategory != null || node.EvaluationOrder != null) Fail("Statement carries expression metadata");
            }
            else
            {
                string type = Type(node);
                if (type == "void" && node.Kind != "call") Fail("Only calls can have void type");
                if (node.ValueCategory != (node.Kind == "binding" ? "place" : "value")) Fail("Incorrect value category");
                string[] orders = node.Kind is "constant" or "binding" ? ["none"] : ["left-to-right", "unspecified"];
                if (node.Kind == "assign") orders = ["left-to-right", "right-to-left", "unspecified"];
                if (!orders.Contains(node.EvaluationOrder)) Fail($"Unsupported evaluation order at {node.Id}");
            }
            if ((node.EntityId != null) != (node.Kind == "call") ||
                (node.BindingId != null) != (node.Kind is "binding" or "declare") ||
                (node.Operator != null) != (node.Kind is "binary" or "unary") ||
                (node.ConstantText != null) != (node.Kind == "constant")) Fail("Unexpected or missing typed payload");

            if (node.Kind == "block")
            {
                EdgeShape(node, ("statement", 0, int.MaxValue));
                var nestedScopes = new List<long>(scopes) { node.Id };
                var nestedVisible = new HashSet<long>(visible);
                foreach (Edge edge in body.Children(node.Id, "statement"))
                {
                    Node child = body.Nodes[edge.ChildId];
                    Statement(child);
                    Visit(child, nestedScopes, nestedVisible, depth + 1);
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
                    EdgeShape(node, ("condition", 1, 1), ("body", 1, 1));
                    Expr(node, "condition"); Statement(Child(node, "body")); break;
                case "constant":
                    EdgeShape(node);
                    if (!int.TryParse(node.ConstantText, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out int value) ||
                        (Type(node) == "bool" && value is not (0 or 1)) || (Type(node) == "u8" && value is < 0 or > 255)) Fail("Invalid scalar constant");
                    break;
                case "binding":
                    EdgeShape(node);
                    if (!body.Bindings.TryGetValue(node.BindingId!.Value, out Binding? binding) || !scopes.Contains(binding.ScopeNodeId) || !visible.Contains(binding.Id)) Fail("Binding used outside scope or before declaration");
                    if (obj.Type(binding!.TypeId) != Type(node)) Fail("Binding reference type mismatch");
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
                    if (operation is "add" or "sub" or "mul" or "div" or "rem")
                    {
                        if (Type(node) != "i32" || Type(Child(node, "left")) != "i32" || Type(Child(node, "right")) != "i32") Fail("Arithmetic requires promoted i32 operands and result");
                    }
                    else if (operation is "eq" or "ne" or "lt" or "le" or "gt" or "ge")
                    {
                        SameType(Child(node, "left"), Child(node, "right"));
                        if (Type(Child(node, "left")) == "bool" && operation is not ("eq" or "ne")) Fail("Relational bool operands require explicit promotion");
                    }
                    else if (operation is not ("and" or "or")) Fail($"Unsupported binary operator {operation}");
                    break;
                case "unary":
                    EdgeShape(node, ("operand", 1, 1)); Expr(node, "operand");
                    if (node.Operator is "neg" or "plus")
                    {
                        if (Type(node) != "i32" || Type(Child(node, "operand")) != "i32") Fail("Unary arithmetic requires promoted i32");
                    }
                    else if (node.Operator != "not") Fail("Unsupported unary operator");
                    break;
                case "assign":
                    EdgeShape(node, ("target", 1, 1), ("value", 1, 1)); Expr(node, "target"); Expr(node, "value");
                    if (Child(node, "target").Kind != "binding") Fail("Initial profile assignment target must be a binding");
                    SameType(node, Child(node, "target")); SameType(node, Child(node, "value")); break;
                case "convert": EdgeShape(node, ("operand", 1, 1)); Expr(node, "operand"); break;
            }
            foreach (Edge edge in body.Edges.GetValueOrDefault(node.Id, []).OrderBy(e => e.Ordinal))
                Visit(body.Nodes[edge.ChildId], scopes, visible, depth + 1);
        }
        foreach (var edges in body.Edges.Values)
            foreach (Edge edge in edges)
                if (!body.Nodes.ContainsKey(edge.ParentId) || !body.Nodes.ContainsKey(edge.ChildId)) Fail("Edge leaves function body");
        Visit(root!, [], parameters.Select(parameter => parameter.Id).ToHashSet(), 0);
        if (visited.Count != body.Nodes.Count) Fail("Orphan executable nodes");
        if (declarations.Count != body.Bindings.Values.Count(binding => binding.Kind == "local")) Fail("Local has no declaration");
    }
}
