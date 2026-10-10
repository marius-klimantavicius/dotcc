namespace DotObj;

// Operation permissions are original C++ semantic facts. They do not describe a
// new physical representation or authorize invoking a user-defined operation.
internal sealed record SourceRecordOperations(bool BitwiseConstructionAllowed,
    bool BitwiseAssignmentAllowed, bool RequiresReturnStorage, bool RequiresParameterStorage);

internal sealed partial class ObjectFile
{
    public Dictionary<long, SourceRecordOperations> RecordOperations { get; } = [];
    public Dictionary<long, string> RecordValueTransfers { get; } = [];

    private void ReadSourceRecordOperations()
    {
        using var query = database.Prepare("SELECT source_record_id,bitwise_construction_allowed,bitwise_assignment_allowed,requires_return_storage,requires_parameter_storage,capture_proof FROM source_record_operations ORDER BY source_record_id");
        while (query.Read())
        {
            long id = query.Integer(0);
            if (SourceLanguage != "c++" || !SourceRecords.TryGetValue(id, out SourceRecord? source) ||
                !source.IsComplete || !source.LayoutKnown || source.IsDependent || source.CapturePhase != "before-class-prelowering" ||
                query.Integer(1) is not (0 or 1) || query.Integer(2) is not (0 or 1) || query.Integer(3) is not (0 or 1) ||
                query.Integer(4) is not (0 or 1) || query.Text(5) != "before-class-prelowering" ||
                !RecordOperations.TryAdd(id, new(query.Integer(1) == 1, query.Integer(2) == 1, query.Integer(3) == 1, query.Integer(4) == 1)))
                throw new InvalidDataException("Invalid original record operation permissions");
        }
        if (SourceLanguage == "c++" && SourceRecords.Values.Any(source => source.IsComplete && source.CapturePhase == "before-class-prelowering" && !RecordOperations.ContainsKey(source.Id)))
            throw new InvalidDataException("Complete C++ source record lacks original operation permissions");
    }

    private void ReadRecordValueTransfers()
    {
        using (var query = database.Prepare("SELECT x.node_id,x.kind,x.capture_proof,n.kind FROM record_value_transfers x LEFT JOIN nodes n ON n.id=x.node_id ORDER BY x.node_id"))
            while (query.Read())
            {
                string kind = query.Text(1);
                if (kind is not ("construction" or "temporary") ||
                    query.Text(2) != (kind == "construction" ? "observed-dynamic-init" : "observed-temporary-assignment") ||
                    query.OptionalText(3) != "record-transfer" || !RecordValueTransfers.TryAdd(query.Integer(0), kind))
                    throw new InvalidDataException("Invalid observed record value transfer");
            }
        ReadRecordStorageCopies();
        using var nodes = database.Prepare("SELECT id FROM nodes WHERE kind='record-transfer'");
        while (nodes.Read())
            if (!RecordValueTransfers.ContainsKey(nodes.Integer(0)) && !RecordStorageCopies.ContainsKey(nodes.Integer(0)))
                throw new InvalidDataException("Record transfer lacks observed operation provenance");
    }

    // default(T) initializes the owned fixed storage prefix; it does not copy
    // a source object or read/copy a flexible tail. Preserve the original
    // ordinary-layout zeroing permission separately from value operations.
    public bool CanZeroInitializeRecord(string type) => ScalarTypes.IsRecord(type) &&
        (RecordsByKey[type].ValueCopyKind == "ordinary" || CanConstructRecord(type) ||
         RecordsByKey[type] is { RepresentationKind: "abi", IdentityKey: "abi:member-function-pointer", CaptureProof: "observed-abi-creation" });

    public bool CanConstructRecord(string type) => CanRecordOperation(type, "construction");
    public bool CanTransportRecord(string type) => CanRecordOperation(type, "transport");
    public bool CanPassRecord(string type) => CanRecordOperation(type, "argument");
    public bool CanTransferRecord(string type, string kind) => CanRecordOperation(type, kind);

    private bool CanRecordOperation(string type, string kind)
    {
        if (ScalarTypes.IsArray(type)) return !ScalarTypes.IsIncompleteArray(type) &&
            (ScalarTypes.IsZeroArray(type) || CanRecordOperation(ScalarTypes.Element(type), kind));
        if (!ScalarTypes.IsRecord(type)) return true;
        RecordType record = RecordsByKey[type];
        // This is a scalar member-function pointer in the source language;
        // stock lowering uses an ordinary by-value record for its two words.
        // SourceRecords verifies the exact owned ABI shape independently.
        if (HasScalarAbiValueSemantics(record))
            return (kind is "construction" or "temporary" or "transport" or "argument") && CanCopyPayload(type);
        if (SourceLanguage != "c++") return CanCopyRecord(type);
        if (record.RepresentationKind != "complete" || record.SourceRecordId is not long id ||
            !RecordOperations.TryGetValue(id, out SourceRecordOperations? operations) || !CanCopyPayload(type)) return false;
        bool allowed = kind switch
        {
            "construction" => operations.BitwiseConstructionAllowed,
            "temporary" => operations.BitwiseConstructionAllowed || operations.BitwiseAssignmentAllowed,
            "transport" => operations.BitwiseConstructionAllowed && !operations.RequiresReturnStorage,
            "argument" => operations.BitwiseConstructionAllowed && !operations.RequiresParameterStorage,
            _ => false,
        };
        return allowed && CanOperationPayload(record, kind);
    }

    private bool CanOperationPayload(RecordType record, string kind)
    {
        foreach (RecordField field in RecordFields(record.TypeId))
        {
            string type = Type(field.TypeId);
            if (field.FieldRole == "base" && ScalarTypes.IsRecord(type))
            {
                if (!CanOperationPayload(RecordsByKey[type], kind)) return false;
            }
            else if (!CanRecordOperation(type, kind)) return false;
        }
        return true;
    }
}
