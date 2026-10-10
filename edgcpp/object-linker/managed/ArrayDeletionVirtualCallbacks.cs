using System.Globalization;

namespace DotObj;

internal sealed record ArrayDeletionVirtualCallback(long CallbackNodeId, long VtableIndex, long CookieSize);

internal sealed partial class ObjectFile
{
    public Dictionary<long, ArrayDeletionVirtualCallback> ArrayDeletionVirtualCallbacks { get; } = [];

    private void ReadArrayDeletionVirtualCallbacks()
    {
        using var query = database.Prepare("SELECT x.call_node_id,x.callback_node_id,x.vtable_index,x.cookie_size,x.capture_proof,n.function_id,c.function_id FROM array_deletion_virtual_callbacks x JOIN nodes n ON n.id=x.call_node_id JOIN nodes c ON c.id=x.callback_node_id ORDER BY x.call_node_id");
        while (query.Read())
        {
            long id = query.Integer(0);
            if (!ArrayDeletionOperations.TryGetValue(id, out var operation) || operation.CallbackKind != "virtual-cookie-guarded" ||
                query.Integer(2) < 0 || query.Integer(3) != 8 || operation.PaddingSize < query.Integer(3) ||
                query.Text(4) != "observed-array-delete-virtual-dispatch" || query.Integer(5) != query.Integer(6) ||
                SourceFunctionKinds.GetValueOrDefault(OriginalFunction(operation.DestructorEntityId)) != "destructor" ||
                AbiRoutineOrigins.TryGetValue(operation.DestructorEntityId, out var origin) && origin.EntryKind != "complete" ||
                !ArrayDeletionVirtualCallbacks.TryAdd(id, new(query.Integer(1), query.Integer(2), query.Integer(3))))
                throw new InvalidDataException("Virtual array destructor callback lacks its exact complete destructor, cookie and execution provenance");
        }
        if (ArrayDeletionOperations.Values.Any(value => (value.CallbackKind == "virtual-cookie-guarded") != ArrayDeletionVirtualCallbacks.ContainsKey(value.NodeId)))
            throw new InvalidDataException("Array delete callback kind and virtual dispatch provenance disagree");
    }

    private void VerifyArrayDeletionVirtualCallback(ArrayDeletionOperation operation, Node[] args, FunctionBody body)
    {
        var proof = ArrayDeletionVirtualCallbacks[operation.NodeId];
        void Fail() => throw new InvalidDataException("Virtual array delete callback differs from its guarded cookie/vtable dispatch");
        Node Child(Node node, string role) => body.Child(node.Id, role);
        Node Strip(Node node)
        {
            int depth = 0;
            while (node.Kind == "convert" && ++depth < 128)
            {
                Node operand = Child(node, "operand");
                string targetType = Type(node.TypeId!.Value), sourceType = Type(operand.TypeId!.Value);
                // Stock adds data/function pointer casts and a void zero arm.
                // Integer truncation or pointer/integer round trips are not
                // equivalent saved addresses and cannot license this proof.
                if (targetType != sourceType && !(ScalarTypes.IsPointer(targetType) && ScalarTypes.IsPointer(sourceType)) &&
                    !(operand.Kind == "constant" && operand.ConstantText == "0")) { Fail(); return node; }
                node = operand;
            }
            return node;
        }
        bool Zero(Node node) => Strip(node) is var value && value.Kind == "constant" && value.ConstantText == "0";
        bool Constant(Node node, long value) => Strip(node) is var item && item.Kind == "constant" && item.ConstantText == value.ToString(CultureInfo.InvariantCulture);
        bool SameSavedPointer(Node node, Node saved) => Strip(node) is var value && value.Kind == "binding" &&
            saved.Kind == "binding" && value.BindingId == saved.BindingId && value.TypeId == saved.TypeId;
        Node source = Strip(args[0]), callback = Strip(args[3]);
        if (args[3].Id != proof.CallbackNodeId || source.Kind != "binding" ||
            source.TypeId is not long sourceType || Type(sourceType) != "ptr(" + Type(operation.ElementTypeId) + ")" ||
            source.BindingId is not long sourceBinding || !body.Bindings.TryGetValue(sourceBinding, out var binding) ||
            !binding.IsSynthetic || binding.Kind != "local" || callback.Kind != "conditional" || !Zero(Child(callback, "else"))) { Fail(); return; }
        Node count = Strip(Child(callback, "condition"));
        if (count.Kind == "binary" && count.Operator == "ne" && Zero(Child(count, "right"))) count = Strip(Child(count, "left"));
        if (count.Kind != "dereference" || count.TypeId is not long countType || Type(countType) != "u64") { Fail(); return; }
        Node cookie = Strip(Child(count, "operand"));
        if (cookie.Kind != "binary" || cookie.Operator != "pointer-subtract" ||
            !Constant(Child(cookie, "right"), proof.CookieSize) || !SameSavedPointer(Child(cookie, "left"), source) ||
            Child(cookie, "left").TypeId is not long cookiePointerType || ScalarTypes.Pointee(Type(cookiePointerType)) is not ("i8" or "u8")) { Fail(); return; }
        Node selected = Strip(Child(callback, "then"));
        if (selected.Kind != "dereference") { Fail(); return; }
        Node slot = Strip(Child(selected, "operand"));
        if (slot.Kind != "binary" || slot.Operator != "pointer-add" || !Constant(Child(slot, "right"), proof.VtableIndex)) { Fail(); return; }
        Node vptr = Strip(Child(slot, "left"));
        if (vptr.Kind != "member" || vptr.FieldId is not long vptrField || Fields[vptrField].FieldRole != "vptr") { Fail(); return; }
        Node record = Child(vptr, "operand");
        while (record.Kind == "member")
        {
            var field = Fields[record.FieldId!.Value];
            if (field.ByteOffset != 0 || field.FieldRole is not ("base" or "virtual-base") ||
                field.FieldRole == "virtual-base" && !RecordVirtualBases(field.RecordTypeId).Any(value => value.FieldId == field.Id && value.ByteOffset == 0)) { Fail(); return; }
            record = Child(record, "operand");
        }
        if (record.Kind != "dereference" || !SameSavedPointer(Child(record, "operand"), source)) { Fail(); return; }
        string selectedType = Type(selected.TypeId!.Value);
        if (!RoutineTypes.IsFunctionPointer(selectedType)) { Fail(); return; }
        var signature = RoutineTypes.Signature(ScalarTypes.Pointee(selectedType));
        string receiver = "ptr(" + Type(operation.ElementTypeId) + ")";
        Entity destructor = Entities[operation.DestructorEntityId];
        var original = EntitySourceRoutines[OriginalFunction(operation.DestructorEntityId)];
        if (Type(destructor.ReturnTypeId) != "void" || Routines[destructor.RoutineTypeId].Variadic ||
            !Parameters.GetValueOrDefault(destructor.Id, []).SequenceEqual([receiver]) ||
            original.ObjectTypeId is not long originalObject || Original(SourceTypeById(originalObject)).ExecutionTypeId != operation.ElementTypeId ||
            original.Variadic || original.Parameters.Length != 0 || Original(SourceTypeById(original.ReturnTypeId)).Kind != "void") { Fail(); return; }
        // Stock may create the virtual slot cast before adding the implicit
        // destructor receiver. Preserve that original member view in the
        // graph; the actual helper callback uses the finalized one-pointer
        // ABI checked above. This exception applies only to this observed
        // address selection, never to a call through the member-view type.
        bool finalizedView = signature.Parameters.SequenceEqual([receiver]);
        bool originalMemberView = signature.Parameters.Length == 0 && signature.ExceptionPolicy == original.ExceptionPolicy;
        if (signature.Variadic || signature.Result != "void" || !(finalizedView || originalMemberView)) { Fail(); return; }
        bool guarded = false;
        foreach (Node parent in body.Nodes.Values.Where(node => node.Kind == "conditional"))
        {
            if (Child(parent, "then").Id != operation.NodeId || !Zero(Child(parent, "else"))) continue;
            Node condition = Strip(Child(parent, "condition"));
            if (condition.Kind == "binary" && condition.Operator == "ne" && Zero(Child(condition, "right"))) condition = Strip(Child(condition, "left"));
            if (condition.Kind == "assign" && SameSavedPointer(Child(condition, "target"), source)) guarded = true;
        }
        if (!guarded) Fail();
    }
}
