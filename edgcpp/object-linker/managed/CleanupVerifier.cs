namespace DotObj;

internal static class CleanupVerifier
{
    // Action roots are recipes over already-owned storage, not executable
    // expressions with arbitrary detached visibility or source side effects.
    public static HashSet<long> TargetBindings(ObjectFile obj, Entity function, FunctionBody body, CleanupAction action)
    {
        var bindings = new HashSet<long>();
        var visited = new HashSet<long>();
        bool payloadTarget = false, hasSubobject = false;
        var payloadPath = new List<PayloadPathPart>();
        void Fail(string message) => throw new InvalidDataException($"{obj.Path}: {function.SourceName}: {message}");
        string Type(Node node) => node.TypeId is long type ? obj.Type(type) : throw new InvalidDataException("Cleanup target has no type");
        Node Child(Node node, string role) => body.Child(node.Id, role);
        void Visit(Node node, int depth)
        {
            if (depth > 128 || !visited.Add(node.Id)) Fail("Shared or excessively nested cleanup target recipe");
            if (node.FunctionId != function.Id) Fail("Cleanup target leaves its function");
            switch (node.Kind)
            {
                case "cleanup-payload-address":
                    if (!obj.CleanupPayloadTargets.TryGetValue(node.Id, out CleanupPayloadTarget? payload) || payload.ActionId != action.Id)
                        Fail("Payload cleanup recipe belongs to a different action");
                    payloadTarget = true;
                    break;
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
                case "member":
                    hasSubobject = true; payloadPath.Add(new(node.FieldId, null)); break;
                case "virtual-base-subobject":
                    if (payloadTarget) Fail("Virtual payload rollback requires an expanded construction path contract");
                    hasSubobject = true; break;
                case "base-subobject":
                    hasSubobject = true; payloadPath.Add(new(null, obj.BaseSubobjectExpressions[node.Id])); break;
                case "address": case "dereference": case "array-decay": break;
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
        if (payloadTarget && !hasSubobject) Fail("Payload rollback requires a proper constructed subobject");
        if (payloadTarget != obj.CleanupPayloadOwners.ContainsKey(action.Id)) Fail("Payload cleanup action has no matching target recipe");
        if (payloadTarget && !payloadPath.AsEnumerable().Reverse().SequenceEqual(obj.PayloadSubobjectPaths[action.SourceObligationId]))
            Fail("Payload rollback target differs from its constructed subobject path");
        if (payloadTarget)
        {
            // Payload paths own exact subobjects, not arbitrary pointer math
            // that merely visits the same member IDs somewhere in a recipe.
            Node place = root!;
            if (place.Kind == "convert") place = Child(place, "operand");
            if (place.Kind != "address") Fail("Payload rollback requires the address of its exact subobject");
            place = Child(place, "operand");
            foreach (PayloadPathPart part in obj.PayloadSubobjectPaths[action.SourceObligationId].AsEnumerable().Reverse())
            {
                if (part.FieldId is long field ? place.Kind != "member" || place.FieldId != field
                    : place.Kind != "base-subobject" || obj.BaseSubobjectExpressions.GetValueOrDefault(place.Id) != part.SourceBaseId)
                    Fail("Payload rollback recipe introduces an unowned address adjustment");
                place = Child(place, "operand");
            }
            if (place.Kind != "dereference" || Child(place, "operand").Kind != "cleanup-payload-address")
                Fail("Payload rollback recipe does not start at its exact payload address");
        }
        if (Type(root!) != obj.Parameters[action.CallbackEntityId][0]) Fail("Cleanup target differs from the selected destructor's pointer ABI");
        if (obj.CleanupArrays.TryGetValue(action.Id, out CleanupArray? array))
        {
            long Available(Node node)
            {
                if (node.Kind == "convert") return Available(Child(node, "operand"));
                if (node.Kind is "address" or "array-decay" && Child(node, "operand").Kind == "dereference")
                    return Available(Child(Child(node, "operand"), "operand"));
                if (node.Kind is "address" or "array-decay") return obj.StorageSize(Type(Child(node, "operand")));
                if (node.Kind == "storage-address") return obj.StorageSize(obj.Type(obj.Storages[node.StorageId!.Value].TypeId));
                if (node.Kind == "binary")
                {
                    Node left = Child(node, "left"), offset = Child(node, "right");
                    if (!long.TryParse(offset.ConstantText, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out long elements) || elements < 0)
                        throw new InvalidDataException("Array cleanup offset must be a bounded forward element index");
                    long stride = obj.StorageSize(ScalarTypes.Pointee(Type(left)));
                    if (stride <= 0 || elements > long.MaxValue / stride || node.Operator != "pointer-add")
                        throw new InvalidDataException("Array cleanup offset is outside its owned span");
                    return Available(left) - elements * stride;
                }
                throw new InvalidDataException("Array cleanup requires an address derived from its bounded array storage");
            }
            if (Available(root!) < checked(array.ElementCount * array.ElementSize))
                Fail("Array cleanup span exceeds its owned target storage");
        }
        var alignment = new AccessAlignment(obj, body);
        string pointed = ScalarTypes.Pointee(Type(root!));
        if (alignment.Pointer(root!) < obj.AccessAlignment(pointed))
            Fail("Underaligned cleanup target escapes its verified typed address");
        return bindings;
    }
}
