namespace DotObj;

internal sealed record RecordConstantInitialization(long StorageId, long SourceRecordId, bool AllBitsZero);

internal sealed partial class ObjectFile
{
    public Dictionary<long, RecordConstantInitialization> RecordConstantInitializations { get; } = [];

    private void ReadRecordConstantInitializations()
    {
        using var query = database.Prepare("SELECT x.node_id,x.source_record_id,x.storage_id,x.initialization_kind,x.capture_proof,n.kind,n.type_id FROM record_constant_initializations x LEFT JOIN nodes n ON n.id=x.node_id ORDER BY x.node_id");
        while (query.Read())
        {
            long id = query.Integer(0), source = query.Integer(1), storageId = query.Integer(2);
            string type = Type(query.Integer(6));
            if (query.Text(3) is not ("zero" or "constant") ||
                query.Text(4) != "observed-pooled-record-initialization" || query.OptionalText(5) != "record-constant-init" ||
                !ScalarTypes.IsRecord(type) || RecordsByKey[type].RepresentationKind != "complete" ||
                RecordsByKey[type].SourceRecordId != source || !SourceRecords[source].IsComplete ||
                !Storages.TryGetValue(storageId, out Storage? storage) || Type(storage.TypeId) != type ||
                storage.Linkage != "internal" || storage.Definition != "strong" || storage.IsThreadLocal ||
                storage.SourceTypeId != null)
                throw new InvalidDataException("Record constant initialization lacks exact original record and compiler-owned constant storage");
            bool zero = Relocations.GetValueOrDefault(storageId, []).Count == 0 &&
                StorageData.GetValueOrDefault(storageId, []).All(row => row.Bytes.All(value => value == 0));
            if (!RecordConstantInitializations.TryAdd(id, new(storageId, source, zero)))
                throw new InvalidDataException("Duplicate record constant initialization");
        }
        using (var nodes = database.Prepare("SELECT id FROM nodes WHERE kind='record-constant-init'"))
            while (nodes.Read())
                if (!RecordConstantInitializations.ContainsKey(nodes.Integer(0)))
                    throw new InvalidDataException("Record constant initialization lacks owned metadata");
        // The pooled representation is not a live source object. Keep its
        // address private and allow only the observed initialization reads.
        using var uses = database.Prepare("SELECT n.id,n.kind,e.role,x.storage_id FROM nodes n LEFT JOIN edges e ON e.child_id=n.id LEFT JOIN record_constant_initializations x ON x.node_id=e.parent_id WHERE n.storage_id IN (SELECT storage_id FROM record_constant_initializations)");
        while (uses.Read())
            if (uses.Text(1) != "storage" || uses.OptionalText(2) != "value" || uses.OptionalInteger(3) is null)
                throw new InvalidDataException("Compiler-owned record constant storage escapes its initialization reads");
        using var escaped = database.Prepare("SELECT COUNT(*) FROM relocations WHERE target_storage_id IN (SELECT storage_id FROM record_constant_initializations)");
        escaped.Read();
        if (escaped.Integer(0) != 0) throw new InvalidDataException("Compiler-owned record constant storage address escapes through a relocation");
    }
}
