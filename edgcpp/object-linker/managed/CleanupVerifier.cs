namespace DotObj;

internal static class CleanupVerifier
{
    // Action roots are recipes over already-owned storage, not executable
    // expressions with arbitrary detached visibility or source side effects.
    public static HashSet<long> TargetBindings(ObjectFile obj, Entity function, FunctionBody body, CleanupAction action)
    {
        var bindings = new HashSet<long>();
        var visited = new HashSet<long>();
        void Fail(string message) => throw new InvalidDataException($"{obj.Path}: {function.SourceName}: {message}");
        string Type(Node node) => node.TypeId is long type ? obj.Type(type) : throw new InvalidDataException("Cleanup target has no type");
        Node Child(Node node, string role) => body.Child(node.Id, role);
        void Visit(Node node, int depth)
        {
            if (depth > 128 || !visited.Add(node.Id)) Fail("Shared or excessively nested cleanup target recipe");
            if (node.FunctionId != function.Id) Fail("Cleanup target leaves its function");
            switch (node.Kind)
            {
                case "binding":
                    if (node.BindingId is not long id || !body.Bindings.TryGetValue(id, out Binding? binding) || binding.FunctionId != function.Id)
                        Fail("Cleanup target references a foreign binding");
                    bindings.Add(node.BindingId!.Value);
                    break;
                case "storage": case "storage-address":
                    if (node.StorageId is not long storage || !obj.Storages.ContainsKey(storage)) Fail("Cleanup target references unknown storage");
                    break;
                case "constant":
                    if (!ScalarTypes.IsInteger(Type(node))) Fail("Cleanup address recipe only permits integer index constants");
                    break;
                case "address": case "dereference": case "array-decay": case "member": case "base-subobject": break;
                case "convert":
                    if (!ScalarTypes.IsPointer(Type(node)) || !ScalarTypes.IsPointer(Type(Child(node, "operand"))))
                        Fail("Cleanup target conversion must preserve pointer representation");
                    break;
                case "binary":
                    if (node.Operator is not ("pointer-add" or "pointer-subtract") ||
                        Child(node, "right").Kind != "constant" || !ScalarTypes.IsPointer(Type(Child(node, "left"))))
                        Fail("Cleanup address adjustment requires an owned pointer and constant index");
                    break;
                default: Fail("Cleanup target recipe contains an unsupported or effectful operation"); break;
            }
            foreach (Edge edge in body.Edges.GetValueOrDefault(node.Id, []))
            {
                if (!body.Nodes.TryGetValue(edge.ChildId, out Node? child)) Fail("Cleanup target edge leaves its function");
                Visit(child!, depth + 1);
            }
        }
        if (!body.Nodes.TryGetValue(action.TargetNodeId, out Node? root)) Fail("Cleanup action has no owned target expression");
        Visit(root!, 0);
        if (Type(root!) != obj.Parameters[action.CallbackEntityId][0]) Fail("Cleanup target differs from the selected destructor's pointer ABI");
        var alignment = new AccessAlignment(obj, body);
        string pointed = ScalarTypes.Pointee(Type(root!));
        if (alignment.Pointer(root!) < obj.AccessAlignment(pointed))
            Fail("Underaligned cleanup target escapes its verified typed address");
        return bindings;
    }
}
