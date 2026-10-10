namespace DotObj;

// These preserve already-constructed storage in stock lowering. They do not
// grant a C++ copy constructor, copy assignment, or additional object lifetime.
internal sealed record RecordStorageCopy(long TargetNodeId, long ValueNodeId, long SourceRecordId,
    long ByteCount, string Kind);

internal sealed partial class ObjectFile
{
    public Dictionary<long, RecordStorageCopy> RecordStorageCopies { get; } = [];

    private void ReadRecordStorageCopies()
    {
        using var query = database.Prepare("""
            SELECT x.node_id,x.target_node_id,x.value_node_id,x.source_record_id,x.byte_count,x.kind,x.capture_proof,
                   n.kind,n.type_id,n.function_id,t.function_id,t.type_id,v.function_id,v.type_id,te.child_id,ve.child_id
              FROM record_storage_copies x
              LEFT JOIN nodes n ON n.id=x.node_id
              LEFT JOIN nodes t ON t.id=x.target_node_id
              LEFT JOIN nodes v ON v.id=x.value_node_id
              LEFT JOIN edges te ON te.parent_id=n.id AND te.role='target' AND te.ordinal=0
              LEFT JOIN edges ve ON ve.parent_id=n.id AND ve.role='value' AND ve.ordinal=0
             ORDER BY x.node_id
            """);
        while (query.Read())
        {
            long id=query.Integer(0), target=query.Integer(1), value=query.Integer(2), sourceId=query.Integer(3);
            long bytes=query.Integer(4); string kind=query.Text(5);
            string type=Type(query.Integer(8));
            if (SourceLanguage != "c++" || kind is not ("evaluation-save" or "class-assignment" or "initializer-storage" or "bitwise-initialization") ||
                query.Text(6) != (kind switch { "evaluation-save" => "observed-pre-execution-save", "class-assignment" => "observed-class-assignment-lowering",
                    "initializer-storage" => "observed-initialization-storage", _ => "observed-bitwise-initialization" }) ||
                query.OptionalText(7) != "record-transfer" || !ScalarTypes.IsRecord(type) ||
                !SourceRecords.TryGetValue(sourceId, out SourceRecord? source) || !source.IsComplete || !source.LayoutKnown ||
                source.IsDependent || source.CapturePhase != "before-class-prelowering" ||
                RecordsByKey[type].RepresentationKind != "complete" || RecordsByKey[type].SourceRecordId != sourceId ||
                bytes <= 0 || bytes != RecordsByKey[type].Size || bytes != source.NativeSize || !CanCopyPayload(type) ||
                query.Integer(9) != query.OptionalInteger(10) || query.Integer(9) != query.OptionalInteger(12) ||
                query.Integer(8) != query.OptionalInteger(11) || query.Integer(8) != query.OptionalInteger(13) ||
                query.OptionalInteger(14) != target || query.OptionalInteger(15) != value || target == value ||
                RecordValueTransfers.ContainsKey(id) ||
                !RecordStorageCopies.TryAdd(id,new(target,value,sourceId,bytes,kind)))
                throw new InvalidDataException("Lowered record storage copy lacks its exact source type, extent and execution operands");
        }
    }
}
