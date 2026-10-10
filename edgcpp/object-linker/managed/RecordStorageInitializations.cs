namespace DotObj;

internal sealed record RecordStorageInitialization(long BindingId, bool ZeroStorage);

internal sealed partial class ObjectFile
{
    public Dictionary<long, RecordStorageInitialization> RecordStorageInitializations { get; } = [];

    private bool HasOriginalMemberPointerStorage(long sourceTypeId, RecordType record)
    {
        if (!HasScalarAbiValueSemantics(record)) return false;
        SourceType source = SourceTypeById(sourceTypeId);
        while (source.Kind is "typedef" or "qualified" or "array")
            source = SourceTypeById(source.UnderlyingTypeId!.Value);
        return source.Kind == "member-pointer" && source.ExecutionRepresentationId == record.TypeId &&
            SourceMemberPointers.TryGetValue(source.Id, out SourceMemberPointerType? member) &&
            Original(SourceTypeById(member.MemberTypeId)).Kind == "routine";
    }

    private void ReadRecordStorageInitializations()
    {
        using var query = database.Prepare("SELECT x.node_id,x.binding_id,x.source_type_id,x.source_init_kind,x.lowered_init_kind,x.site_kind,x.zero_storage,x.partially_initialized,x.capture_proof,n.kind,n.function_id,b.function_id,b.kind,b.type_id,d.kind,d.binding_id,d.source_type_id,d.function_entity_id FROM record_storage_initializations x LEFT JOIN nodes n ON n.id=x.node_id LEFT JOIN bindings b ON b.id=x.binding_id LEFT JOIN source_declarations d ON d.id=x.source_declaration_id ORDER BY x.node_id");
        while (query.Read())
        {
            long binding = query.Integer(1), original = query.Integer(3), lowered = query.Integer(4);
            bool zero = query.Integer(6) == 1, partial = query.Integer(7) == 1;
            string type = Type(query.Integer(13));
            string leaf = ScalarTypes.IsArray(type) ? ScalarTypes.Leaf(type) : type;
            if (SourceLanguage != "c++" || original is < 0 or > 5 || lowered is < 0 or > 5 ||
                query.Text(5) is not ("scope-entry" or "dynamic-init" or "lowered-assignment") ||
                query.Text(5) == "scope-entry" && lowered != 3 ||
                query.Integer(6) is not (0 or 1) || query.Integer(7) is not (0 or 1) ||
                zero != (lowered == 3 || lowered == 2 && partial) || partial && lowered != 2 ||
                query.Text(8) != "observed-storage-initialization" || query.OptionalText(9) != "record-storage-init" ||
                query.Integer(10) != query.Integer(11) || query.OptionalText(12) != "local" ||
                query.OptionalText(14) != "local" || query.Integer(15) != binding ||
                query.Integer(16) != query.Integer(2) || query.Integer(17) != query.Integer(10) ||
                !BindingSourceTypes.TryGetValue(binding, out long source) || source != query.Integer(2) ||
                !ScalarTypes.IsRecord(leaf) ||
                (RecordsByKey[leaf].RepresentationKind != "complete" && !HasOriginalMemberPointerStorage(source, RecordsByKey[leaf])) ||
                !RecordStorageInitializations.TryAdd(query.Integer(0), new(binding, zero)))
                throw new InvalidDataException("Record storage initialization lacks exact original local and observed zeroing provenance");
        }
        ReadRecordTemporaryInitializations();
        using var nodes = database.Prepare("SELECT id FROM nodes WHERE kind='record-storage-init'");
        while (nodes.Read())
            if (!RecordStorageInitializations.ContainsKey(nodes.Integer(0)))
                throw new InvalidDataException("Record storage initialization lacks owned metadata");
    }

    public void VerifyRecordStorageInitializationBody(Node initialization, FunctionBody body)
    {
        long binding = RecordStorageInitializations[initialization.Id].BindingId;
        bool Target(Node node, int depth = 0)
        {
            if (depth > 128) return false;
            if (node.Kind == "binding") return node.BindingId == binding;
            if (node.Kind == "member") return Target(body.Child(node.Id, "operand"), depth + 1);
            if (node.Kind != "dereference") return false;
            Node add = body.Child(node.Id, "operand");
            if (add.Kind != "binary" || add.Operator != "pointer-add") return false;
            Node array = body.Child(add.Id, "left");
            return array.Kind == "array-decay" && body.Child(add.Id, "right").Kind == "constant" &&
                Target(body.Child(array.Id, "operand"), depth + 1);
        }
        void Check(Node node, int depth)
        {
            if (depth > 128) throw new InvalidDataException("Record initialization body exceeds nesting limit");
            if (node.Kind == "block")
            {
                foreach (Edge edge in body.Children(node.Id, "statement")) Check(body.Nodes[edge.ChildId], depth + 1);
                return;
            }
            Node operation = node.Kind == "expr-stmt" ? body.Child(node.Id, "value") : node;
            if (operation.Kind is not ("assign" or "record-transfer" or "array-init") ||
                !Target(body.Child(operation.Id, "target")))
                throw new InvalidDataException("Typed record initializer writes outside its exact original local storage");
        }
        Check(body.Child(initialization.Id, "body"), 0);
        VerifyRecordTemporaryDestructionBinding(initialization, body);
    }
}
