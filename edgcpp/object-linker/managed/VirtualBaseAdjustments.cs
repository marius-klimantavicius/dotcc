namespace DotObj;

internal sealed record VirtualBaseAdjustment(long OperandNodeId, long SourceNodeId, long LookupSourceNodeId,
    long DerivedTypeId, long CompleteBaseTypeId, long OffsetNodeId, long VtableIndex);

internal sealed partial class ObjectFile
{
    public Dictionary<long, VirtualBaseAdjustment> VirtualBaseAdjustments { get; } = [];

    private void ReadVirtualBaseAdjustments()
    {
        using var query = database.Prepare("SELECT x.node_id,x.operand_node_id,x.source_node_id,x.lookup_source_node_id,x.derived_type_id,x.complete_base_type_id,x.offset_node_id,x.vtable_index,x.is_complete_object,x.capture_proof,n.kind,n.function_id,o.function_id,s.function_id,l.function_id,f.function_id FROM virtual_base_adjustments x LEFT JOIN nodes n ON n.id=x.node_id LEFT JOIN nodes o ON o.id=x.operand_node_id LEFT JOIN nodes s ON s.id=x.source_node_id LEFT JOIN nodes l ON l.id=x.lookup_source_node_id LEFT JOIN nodes f ON f.id=x.offset_node_id ORDER BY x.node_id");
        while (query.Read())
        {
            long id = query.Integer(0), derived = query.Integer(4), baseType = query.Integer(5);
            if (SourceLanguage != "c++" || query.Integer(8) != 0 || query.Text(9) != "observed-virtual-base-adjustment" ||
                query.OptionalText(10) != "virtual-base-adjustment" ||
                Enumerable.Range(12, 4).Any(index => query.OptionalInteger(index) != query.OptionalInteger(11)) ||
                !Records.TryGetValue(derived, out RecordType? source) || source.RepresentationKind != "complete" ||
                !Records.TryGetValue(baseType, out RecordType? target) || target.RepresentationKind != "complete" ||
                !VirtualBaseSubobjects.ContainsKey((derived, baseType)) ||
                !VirtualBaseAdjustments.TryAdd(id, new(query.Integer(1), query.Integer(2), query.Integer(3), derived, baseType, query.Integer(6), query.Integer(7))))
                throw new InvalidDataException("Virtual base adjustment lacks its original derived/base relationship and exact lowering provenance");
        }
        using var nodes = database.Prepare("SELECT id FROM nodes WHERE kind='virtual-base-adjustment'");
        while (nodes.Read())
            if (!VirtualBaseAdjustments.ContainsKey(nodes.Integer(0)))
                throw new InvalidDataException("Virtual base adjustment lacks owned provenance");
    }

    public void VerifyVirtualBaseAdjustmentBody(Node node, FunctionBody body)
    {
        VirtualBaseAdjustment adjustment = VirtualBaseAdjustments[node.Id];
        void Fail() => throw new InvalidDataException("Virtual base adjustment differs from its exact observed object/vtable lookup");
        Node Child(Node value, string role) => body.Child(value.Id, role);
        bool Contains(Node root, long wanted, int depth = 0) => root.Id == wanted || depth < 128 &&
            body.Edges.GetValueOrDefault(root.Id, []).Any(edge => Contains(body.Nodes[edge.ChildId], wanted, depth + 1));
        bool SamePlace(Node left, Node right, int depth = 0)
        {
            if (left.Id == right.Id) return true;
            if (depth > 128 || left.Kind != right.Kind || left.TypeId != right.TypeId) return false;
            if (left.Kind == "binding") return left.BindingId == right.BindingId;
            if (left.Kind == "storage") return left.StorageId == right.StorageId;
            if (left.Kind == "constant") return left.ConstantText == right.ConstantText;
            if (left.Kind is not ("dereference" or "convert" or "member" or "address" or "array-decay")) return false;
            if (left.FieldId != right.FieldId) return false;
            Edge[] a = body.Edges.GetValueOrDefault(left.Id, []).OrderBy(edge => edge.Role).ThenBy(edge => edge.Ordinal).ToArray();
            Edge[] b = body.Edges.GetValueOrDefault(right.Id, []).OrderBy(edge => edge.Role).ThenBy(edge => edge.Ordinal).ToArray();
            return a.Length == b.Length && a.Zip(b).All(pair => pair.First.Role == pair.Second.Role && pair.First.Ordinal == pair.Second.Ordinal &&
                SamePlace(body.Nodes[pair.First.ChildId], body.Nodes[pair.Second.ChildId], depth + 1));
        }
        long Complete(Node value, bool pointer = false)
        {
            if (value.TypeId is not long type) return 0;
            string key = Type(type);
            if (pointer) { if (!ScalarTypes.IsPointer(key)) return 0; key = ScalarTypes.Pointee(key); }
            return RecordsByKey.TryGetValue(key, out RecordType? record) ? record.CompleteTypeId ?? record.TypeId : 0;
        }
        Node operand = Child(node, "operand"), source = body.Nodes[adjustment.SourceNodeId], lookup = body.Nodes[adjustment.LookupSourceNodeId], offset = body.Nodes[adjustment.OffsetNodeId];
        bool SameInitializedPointers(Node left, Node right)
        {
            if (SamePlace(left, right)) return true;
            if (left.Kind != "binding" || right.Kind != "binding" || left.TypeId != right.TypeId ||
                operand.Kind != "sequence") return false;
            Edge[] items = body.Children(operand.Id, "item").ToArray();
            if (items.Length < 2) return false;
            Node initialization = body.Nodes[items[^2].ChildId];
            if (initialization.Kind == "dereference") initialization = Child(initialization, "operand");
            var initialized = new HashSet<long>();
            while (initialization.Kind == "assign")
            {
                Node target = Child(initialization, "target"), value = Child(initialization, "value");
                if (initialization.EvaluationOrder != "right-to-left" || initialization.TypeId != left.TypeId ||
                    target.Kind != "binding" || target.BindingId is not long binding ||
                    !body.Bindings.TryGetValue(binding, out Binding? local) || local.Kind != "local" || !local.IsSynthetic ||
                    !initialized.Add(binding)) return false;
                initialization = value;
            }
            // The innermost RHS runs before either store. The immediately
            // following adjustment reads only these saved pointers and vptr
            // fields; it cannot observe different evaluations of the RHS.
            return initialized.Contains(left.BindingId!.Value) && initialized.Contains(right.BindingId!.Value);
        }
        bool LookupPath(Node value, int depth = 0)
        {
            if (value.Id == lookup.Id) return true;
            if (depth > 128) return false;
            if (value.Kind == "dereference") return Child(value, "operand").Id == lookup.Id;
            if (value.Kind != "member" || Fields[value.FieldId!.Value].ByteOffset != 0 ||
                Fields[value.FieldId.Value].FieldRole is not ("vptr" or "base")) return false;
            return LookupPath(Child(value, "operand"), depth + 1);
        }
        bool lookupPointer = lookup.TypeId is long lookupType && ScalarTypes.IsPointer(Type(lookupType));
        bool sameObject = lookupPointer ? SameInitializedPointers(source, lookup)
            : lookup.Kind == "dereference" ? SamePlace(source, Child(lookup, "operand"))
            : source.Kind == "address" && SamePlace(Child(source, "operand"), lookup);
        if (operand.Id != adjustment.OperandNodeId || operand.TypeId != node.TypeId ||
            !lookupPointer && lookup.ValueCategory != "place" ||
            Complete(source, true) != adjustment.DerivedTypeId || Complete(lookup, lookupPointer) != adjustment.DerivedTypeId ||
            Complete(node, true) != adjustment.CompleteBaseTypeId ||
            !Contains(operand, source.Id) || !Contains(operand, lookup.Id) || !Contains(operand, offset.Id) || !sameObject) Fail();
        Node pointer = operand;
        if (pointer.Kind == "sequence") pointer = body.Nodes[body.Children(pointer.Id, "item").Last().ChildId];
        if (pointer.Kind != "convert") { Fail(); return; }
        Node addition = Child(pointer, "operand");
        if (addition.Kind != "binary" || addition.Operator != "pointer-add" || Child(addition, "right").Id != offset.Id) { Fail(); return; }
        Node address = Child(addition, "left");
        if (address.Kind == "convert") address = Child(address, "operand");
        if (address.Id != source.Id || offset.Kind != "dereference") { Fail(); return; }
        Node slot = Child(offset, "operand");
        if (slot.Kind != "binary" || slot.Operator != "pointer-add") { Fail(); return; }
        Node index = Child(slot, "right"), vptr = Child(slot, "left");
        if (index.Kind != "constant" || index.ConstantText != adjustment.VtableIndex.ToString(System.Globalization.CultureInfo.InvariantCulture) ||
            vptr.Kind != "member" || Fields[vptr.FieldId!.Value].FieldRole != "vptr" || !LookupPath(vptr)) Fail();
        var alignment = new AccessAlignment(this, body);
        RecordType derived = Records[adjustment.DerivedTypeId], target = Records[adjustment.CompleteBaseTypeId];
        VirtualBaseSubobject placement = VirtualBaseSubobjects[(derived.TypeId, target.TypeId)];
        if (derived.Alignment < target.Alignment || placement.ByteOffset % target.Alignment != 0 ||
            alignment.Pointer(source) < derived.Alignment || (lookupPointer ? alignment.Pointer(lookup) : alignment.Place(lookup)) < derived.Alignment)
            throw new InvalidDataException("Dynamic virtual base adjustment lacks a sufficient original object alignment contract");
    }
}
