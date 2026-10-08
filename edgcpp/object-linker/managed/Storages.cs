namespace DotObj;

internal sealed record Storage(long Id, string SourceName, string Linkage, string? ExternalKey, string Definition,
    long TypeId, long? SourceTypeId, bool IsThreadLocal, bool IsConst, long Alignment, long? SourceFileId, long Line, long Column);
internal sealed record StorageData(long StorageId, long ByteOffset, byte[] Bytes);
internal sealed record Relocation(long StorageId, long ByteOffset, long? TargetStorageId, long? TargetEntityId, long Addend);
internal sealed record StorageSymbol(ObjectFile Object, Storage Storage)
{
    public string Key => Storage.Linkage == "external" ? "storage-external:" + Storage.ExternalKey
        : "storage-internal:" + Object.UnitId + ":" + Storage.Id;
    public string Signature => Object.StorageSourceSignature(Storage) + (Storage.IsThreadLocal ? ":tls" : ":global");
}

internal sealed partial class ObjectFile
{
    public Dictionary<long, Storage> Storages { get; } = [];
    public Dictionary<long, List<StorageData>> StorageData { get; } = [];
    public Dictionary<long, List<Relocation>> Relocations { get; } = [];

    private void ReadStorages()
    {
        using (var query = database.Prepare("SELECT id,source_name,linkage,external_key,definition,type_id,source_type_id,is_thread_local,is_const,alignment,source_file_id,line,column FROM storages ORDER BY id"))
            while (query.Read())
            {
                var value = new Storage(query.Integer(0), query.Text(1), query.Text(2), query.OptionalText(3), query.Text(4), query.Integer(5),
                    query.OptionalInteger(6), query.Integer(7) == 1, query.Integer(8) == 1, query.Integer(9), query.OptionalInteger(10), query.Integer(11), query.Integer(12));
                string type = Type(value.TypeId);
                if (value.Id <= 0 || value.Linkage is not ("external" or "internal") || value.Definition is not ("strong" or "linkonce" or "tentative" or "declaration") ||
                    (value.Linkage == "external" ? string.IsNullOrEmpty(value.ExternalKey) : value.ExternalKey != null) || value.Line < 0 || value.Column < 0 ||
                    query.Integer(7) is not (0 or 1) || query.Integer(8) is not (0 or 1) || (value.Definition == "declaration" ? value.Alignment is < 0 or > 64 : value.Alignment is < 1 or > 64) ||
                    (value.Alignment & (value.Alignment - 1)) != 0 || type == "void" || RoutineTypes.IsRoutine(type) ||
                    value.Definition != "declaration" && (value.Alignment < StorageAlignment(type) || StorageSize(type) <= 0))
                    throw new InvalidDataException("Invalid storage identity, type or alignment");
                Storages.Add(value.Id, value);
            }
        using (var query = database.Prepare("SELECT storage_id,byte_offset,bytes FROM storage_data ORDER BY storage_id,byte_offset"))
            while (query.Read())
            {
                long id = query.Integer(0), offset = query.Integer(1); byte[] bytes = query.Bytes(2);
                if (!Storages.TryGetValue(id, out Storage? storage) || storage.Definition == "declaration" || bytes.Length == 0 || offset < 0 ||
                    offset > StorageSize(Type(storage.TypeId)) - bytes.Length)
                    throw new InvalidDataException("Static data extent lies outside defined storage");
                if (!StorageData.TryGetValue(id, out var rows)) StorageData.Add(id, rows = []);
                if (rows.Count != 0 && rows[^1].ByteOffset + rows[^1].Bytes.Length > offset) throw new InvalidDataException("Overlapping static data ranges");
                rows.Add(new(id, offset, bytes));
            }
    }

    private (bool Wide, bool PointerSlot) RelocationStorageShape(string type, long offset, int depth = 0)
    {
        if (depth > 128 || offset < 0 || offset >= StorageSize(type)) return (false, false);
        if (ScalarTypes.IsWideInteger(type)) return (true, false);
        if (ScalarTypes.IsArray(type))
        {
            string element = ScalarTypes.Element(type);
            long size = StorageSize(element);
            return size == 0 ? (false, false) : RelocationStorageShape(element, offset % size, depth + 1);
        }
        if (ScalarTypes.IsRecord(type))
        {
            bool wide = false, pointerSlot = false;
            foreach (RecordField field in RecordFields(RecordsByKey[type].TypeId))
            {
                if (field.BitWidth is not null) continue;
                var shape = RelocationStorageShape(Type(field.TypeId), offset - field.ByteOffset, depth + 1);
                wide |= shape.Wide; pointerSlot |= shape.PointerSlot;
            }
            return (wide, pointerSlot);
        }
        return (false, offset == 0 && (ScalarTypes.IsPointer(type) || type is "i64" or "u64"));
    }
    private void VerifyStorageRelocations()
    {
        using (var query = database.Prepare("SELECT storage_id,byte_offset,target_storage_id,target_entity_id,addend FROM relocations ORDER BY storage_id,byte_offset"))
            while (query.Read())
            {
                var value = new Relocation(query.Integer(0), query.Integer(1), query.OptionalInteger(2), query.OptionalInteger(3), query.Integer(4));
                if (!Storages.TryGetValue(value.StorageId, out Storage? owner) || owner.Definition == "declaration" ||
                    value.ByteOffset < 0 || value.ByteOffset > StorageSize(Type(owner.TypeId)) - 8 ||
                    (value.TargetStorageId is null) == (value.TargetEntityId is null))
                    throw new InvalidDataException("Invalid storage relocation extent or target");
                var shape = RelocationStorageShape(Type(owner.TypeId), value.ByteOffset);
                if (shape.Wide && !shape.PointerSlot)
                    throw new InvalidDataException("128-bit address initializers require an explicit relocation width and extension contract");
                if (value.TargetStorageId is long targetId)
                {
                    if (!Storages.TryGetValue(targetId, out Storage? target) || value.Addend < 0 || value.Addend > StorageSize(Type(target.TypeId)) ||
                        !owner.IsThreadLocal && target.IsThreadLocal)
                        throw new InvalidDataException("Invalid storage relocation target, displacement or TLS domain");
                }
                else if (!Entities.ContainsKey(value.TargetEntityId!.Value) || value.Addend != 0)
                    throw new InvalidDataException("Invalid function relocation target or displacement");
                if (!Relocations.TryGetValue(value.StorageId, out var rows)) Relocations.Add(value.StorageId, rows = []);
                if (rows.Count != 0 && rows[^1].ByteOffset + 8 > value.ByteOffset) throw new InvalidDataException("Overlapping pointer relocations");
                foreach (StorageData data in StorageData.GetValueOrDefault(value.StorageId, []))
                {
                    long begin = Math.Max(data.ByteOffset, value.ByteOffset), end = Math.Min(data.ByteOffset + data.Bytes.Length, value.ByteOffset + 8);
                    for (long index = begin; index < end; index++)
                        if (data.Bytes[index - data.ByteOffset] != 0) throw new InvalidDataException("Relocation overlaps nonzero constant data");
                }
                rows.Add(value);
            }
    }

    public string StorageSourceSignature(Storage storage) => storage.SourceTypeId is long source ? SourceTypeSignature(source) : Type(storage.TypeId);

    private void VerifyStorageSourceTypes()
    {
        foreach (Storage storage in Storages.Values)
            if (storage.SourceTypeId is long id && SourceTypeById(id).ExecutionTypeId != storage.TypeId)
                throw new InvalidDataException("Storage source type disagrees with execution representation");
    }
}
