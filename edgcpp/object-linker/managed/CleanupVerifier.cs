namespace DotObj;

internal static class CleanupVerifier
{
    // The field root is construction of the actual member of this constructor,
    // not an unrelated object of the same type or a nested initializer temporary.
    public static void FieldInitializerTarget(ObjectFile obj, Entity function, FunctionBody body,
        long sourceInitializerId, Node target)
    {
        SourceInitializer initializer = obj.SourceInitializers[sourceInitializerId];
        long sourceFieldId = initializer.SourceFieldId!.Value;
        long fieldSize = 0;
        int depth = 0;
        Node Child(Node node) => body.Child(node.Id, "operand");
        long Offset(Node node)
        {
            if (++depth > 128 || node.FunctionId != function.Id)
                throw new InvalidDataException("Field initializer target leaves its owned constructor");
            if (node.Kind is "convert" or "address" or "array-decay" or "dereference")
                return Offset(Child(node));
            if (node.Kind == "binary" && node.Operator == "pointer-add")
            {
                Node left = body.Child(node.Id, "left"), right = body.Child(node.Id, "right");
                if (right.Kind != "constant" || !long.TryParse(right.ConstantText,
                    System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture,
                    out long index) || index < 0 || left.TypeId is not long pointerType ||
                    !ScalarTypes.IsPointer(obj.Type(pointerType)))
                    throw new InvalidDataException("Field initializer requires a fixed forward member element path");
                return checked(Offset(left) + index * obj.StorageSize(ScalarTypes.Pointee(obj.Type(pointerType))));
            }
            if (node.Kind != "member" || node.FieldId is not long fieldId ||
                !obj.Fields.TryGetValue(fieldId, out RecordField? field) || field.SourceFieldId != sourceFieldId)
                throw new InvalidDataException("Field initializer target differs from its original member");
            Node owner = Child(node);
            if (owner.Kind != "dereference")
                throw new InvalidDataException("Field initializer member lacks its constructor receiver");
            Node receiver = Child(owner);
            // No arbitrary saved pointer aliases or base adjustment may stand in
            // for this; those require their own exact observed correspondence.
            if (receiver.Kind != "binding" || receiver.BindingId is not long bindingId ||
                !body.Bindings.TryGetValue(bindingId, out Binding? binding) || binding.Kind != "parameter" ||
                binding.Ordinal is not long ordinal ||
                obj.ParameterRoles[(function.Id, ordinal)].Role != "this")
                throw new InvalidDataException("Field initializer member is not owned by this constructor");
            fieldSize = obj.StorageSize(obj.Type(field.TypeId));
            return 0;
        }
        long offset = Offset(target);
        if (target.TypeId is not long targetType || !ScalarTypes.IsPointer(obj.Type(targetType)) ||
            offset < 0 || offset >= fieldSize)
            throw new InvalidDataException("Field initializer pointer leaves its original member storage");
    }

    public static HashSet<long> RollbackTargetBindings(ObjectFile obj, Entity function, FunctionBody body, long targetNodeId, bool guard)
    {
        var bindings = new HashSet<long>();
        if (!body.Nodes.TryGetValue(targetNodeId, out Node? root) || root.FunctionId != function.Id)
            throw new InvalidDataException("Rollback target leaves its owned function");
        if (guard)
        {
            // ReadGuardMetadata already verifies exact zero-offset guard storage.
            if (root.Kind != "storage-address" || obj.Type(root.TypeId!.Value) != "ptr(u64)")
                throw new InvalidDataException("Guard rollback target must be its owned storage address");
        }
        else
        {
            if (obj.Type(root.TypeId!.Value) != "ptr(void)")
                throw new InvalidDataException("Allocation rollback requires its delete pointer ABI");
            var visited = new HashSet<long>();
            while (root.Kind == "convert")
            {
                if (!visited.Add(root.Id) || visited.Count > 128 || body.Edges.GetValueOrDefault(root.Id, []).Count != 1)
                    throw new InvalidDataException("Invalid allocation rollback pointer conversion recipe");
                root = body.Child(root.Id, "operand");
                if (!ScalarTypes.IsPointer(obj.Type(root.TypeId!.Value)))
                    throw new InvalidDataException("Allocation rollback conversion must preserve pointer representation");
            }
            if (root.Kind != "binding" || root.BindingId is not long bindingId ||
                !body.Bindings.TryGetValue(bindingId, out Binding? binding) || binding.FunctionId != function.Id ||
                binding.Kind != "local" || !ScalarTypes.IsPointer(obj.Type(binding.TypeId)))
                throw new InvalidDataException("Allocation rollback must read its owned allocated-pointer local");
            bindings.Add(binding.Id);
        }
        return bindings;
    }

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
        if (payloadTarget && (action.SourceObligationId is not long automatic || !payloadPath.AsEnumerable().Reverse().SequenceEqual(obj.PayloadSubobjectPaths[automatic])))
            Fail("Payload rollback target differs from its constructed subobject path");
        if (payloadTarget)
        {
            // Payload paths own exact subobjects, not arbitrary pointer math
            // that merely visits the same member IDs somewhere in a recipe.
            Node place = root!;
            if (place.Kind == "convert") place = Child(place, "operand");
            if (place.Kind != "address") Fail("Payload rollback requires the address of its exact subobject");
            place = Child(place, "operand");
            foreach (PayloadPathPart part in obj.PayloadSubobjectPaths[action.SourceObligationId!.Value].AsEnumerable().Reverse())
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
        if (action.SourceFieldInitializerId is long fieldInitializer)
            FieldInitializerTarget(obj, function, body, fieldInitializer, root!);
        if (action.SourceStaticObligationId is long templateId &&
            obj.SourceStaticDestructions[templateId].SourceInitializerId is long initializerId &&
            obj.SourceInitializers[initializerId].Kind == "field-default")
            FieldInitializerTarget(obj, function, body, initializerId, root!);
        if (action.SourceStaticObligationId is long staticId &&
            obj.SourceInitializers[obj.SourceStaticDestructions[staticId].SourceInitializerId!.Value].Kind != "field-default")
        {
            long storageId = obj.StaticCleanupStorage(action);
            long Offset(Node node)
            {
                if (node.Kind is "convert" or "address" or "array-decay" or "dereference")
                    return Offset(Child(node, "operand"));
                if (node.Kind is "storage" or "storage-address")
                {
                    if (node.StorageId != storageId || node.Kind == "storage-address" && node.ConstantText != "0")
                        throw new InvalidDataException("Static array rollback starts at a foreign storage or offset");
                    return 0;
                }
                if (node.Kind == "binary" && node.Operator == "pointer-add")
                {
                    Node left = Child(node, "left"), right = Child(node, "right");
                    if (right.Kind != "constant" || !long.TryParse(right.ConstantText,
                        System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture,
                        out long index) || index < 0)
                        throw new InvalidDataException("Static array rollback requires a fixed forward element path");
                    return checked(Offset(left) + index * obj.StorageSize(ScalarTypes.Pointee(Type(left))));
                }
                throw new InvalidDataException("Static array rollback requires its exact original array storage path");
            }
            long offset = Offset(root!);
            long extent = obj.CleanupArrays.TryGetValue(action.Id, out var staticArray)
                ? checked(staticArray.ElementCount * staticArray.ElementSize)
                : obj.StorageSize(ScalarTypes.Pointee(Type(root!)));
            if (bindings.Count != 0 || payloadTarget || extent <= 0 || offset < 0 ||
                checked(offset + extent) > obj.StorageSize(obj.Type(obj.Storages[storageId].TypeId)))
                Fail("Static array rollback leaves the original array's bounded storage");
        }
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
