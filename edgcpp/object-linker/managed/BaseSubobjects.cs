namespace DotObj;

internal sealed record SourceBase(long Id, long SourceRecordId, long Ordinal, long BaseSourceTypeId,
    string AccessKind, bool IsVirtual, long? SourceFileId, long Line, long Column);
internal sealed record RecordBaseSubobject(long RecordTypeId, long SourceBaseId, long CompleteBaseTypeId,
    long? PhysicalViewTypeId, long ByteOffset, long Extent, long? FieldId);

internal sealed partial class ObjectFile
{
    public Dictionary<long, SourceBase> SourceBases { get; } = [];
    public Dictionary<(long RecordTypeId, long SourceBaseId), RecordBaseSubobject> BaseSubobjects { get; } = [];
    private readonly Dictionary<long, SourceBase[]> basesBySourceRecord = [];
    private readonly Dictionary<long, RecordBaseSubobject[]> basesByRecord = [];
    public RecordBaseSubobject[] RecordBases(long recordTypeId) => basesByRecord.GetValueOrDefault(recordTypeId, []);
    public SourceBase[] OriginalBases(long sourceRecordId) => basesBySourceRecord.GetValueOrDefault(sourceRecordId, []);

    private void ReadBaseMetadata()
    {
        using (var query = database.Prepare("SELECT id,source_record_id,ordinal,base_source_type_id,access_kind,is_virtual,source_file_id,line,column FROM source_record_bases ORDER BY source_record_id,ordinal"))
            while (query.Read())
            {
                var item = new SourceBase(query.Integer(0), query.Integer(1), query.Integer(2), query.Integer(3), query.Text(4),
                    query.Integer(5) == 1, query.OptionalInteger(6), query.Integer(7), query.Integer(8));
                if (item.Id <= 0 || !SourceRecords.ContainsKey(item.SourceRecordId) || item.Ordinal < 0 || item.BaseSourceTypeId <= 0 ||
                    item.AccessKind is not ("public" or "protected" or "private") || query.Integer(5) is not (0 or 1) || item.Line < 0 || item.Column < 0)
                    throw new InvalidDataException("Invalid original base declaration");
                SourceBases.Add(item.Id, item);
            }
        foreach (var group in SourceBases.Values.GroupBy(item => item.SourceRecordId))
            basesBySourceRecord.Add(group.Key, group.OrderBy(item => item.Ordinal).ToArray());
        using (var query = database.Prepare("SELECT record_type_id,source_base_id,complete_base_type_id,physical_view_type_id,byte_offset,extent,field_id FROM record_base_subobjects ORDER BY record_type_id,source_base_id"))
            while (query.Read())
            {
                var item = new RecordBaseSubobject(query.Integer(0), query.Integer(1), query.Integer(2), query.OptionalInteger(3), query.Integer(4), query.Integer(5), query.OptionalInteger(6));
                if (!Records.ContainsKey(item.RecordTypeId) || !SourceBases.ContainsKey(item.SourceBaseId) || !Records.ContainsKey(item.CompleteBaseTypeId) ||
                    item.ByteOffset < 0 || item.Extent < 0 || (item.Extent == 0 ? item.PhysicalViewTypeId is not null || item.FieldId is not null
                        : item.PhysicalViewTypeId is null || item.FieldId is null))
                    throw new InvalidDataException("Invalid physical base subobject relationship");
                BaseSubobjects.Add((item.RecordTypeId, item.SourceBaseId), item);
            }
        foreach (var group in BaseSubobjects.Values.GroupBy(item => item.RecordTypeId))
            basesByRecord.Add(group.Key, group.OrderBy(item => SourceBases[item.SourceBaseId].Ordinal).ToArray());
    }

    private void VerifyBaseMetadata()
    {
        foreach (var group in basesBySourceRecord)
        {
            SourceRecord owner = SourceRecords[group.Key];
            for (int ordinal = 0; ordinal < group.Value.Length; ordinal++)
            {
                SourceBase item = group.Value[ordinal];
                SourceType source = Original(SourceTypeById(item.BaseSourceTypeId));
                if (!owner.IsComplete || owner.CapturePhase != "before-class-prelowering" || item.Ordinal != ordinal || item.IsVirtual ||
                    source.Kind != "record" || !Records.TryGetValue(source.ExecutionTypeId, out RecordType? target) || target.RepresentationKind != "complete" || target.Size == 0 ||
                    target.SourceRecordId == owner.Id)
                    throw new InvalidDataException("Base declaration requires a complete nonvirtual original class");
            }
        }
        var visited = new HashSet<long>();
        var active = new HashSet<long>();
        void CheckBaseGraph(long sourceId, int depth)
        {
            if (depth > 128 || !active.Add(sourceId)) throw new InvalidDataException("Cyclic or excessively nested original base graph");
            if (!visited.Contains(sourceId))
                foreach (SourceBase item in OriginalBases(sourceId))
                    CheckBaseGraph(Records[Original(SourceTypeById(item.BaseSourceTypeId)).ExecutionTypeId].SourceRecordId!.Value, depth + 1);
            active.Remove(sourceId); visited.Add(sourceId);
        }
        foreach (long sourceId in SourceRecords.Keys) CheckBaseGraph(sourceId, 0);
        foreach (RecordType record in Records.Values)
        {
            if (record.RepresentationKind == "subobject")
            {
                if (record.CompleteTypeId is not long completeId || !Records.TryGetValue(completeId, out RecordType? complete) ||
                    complete.RepresentationKind != "complete" || complete.SourceRecordId != record.SourceRecordId || complete.LayoutKind != record.LayoutKind ||
                    complete.Size < record.Size || complete.Alignment != record.Alignment || record.SourceName != complete.SourceName ||
                    record.IdentityKey == complete.IdentityKey || record.ValueCopyKind != "explicit-only")
                    throw new InvalidDataException("Subobject must identify its complete nominal class and bounded native extent");
            }
            SourceBase[] expected = record.SourceRecordId is long sourceId ? OriginalBases(sourceId) : [];
            foreach (SourceBase source in expected)
            {
                if (!BaseSubobjects.TryGetValue((record.TypeId, source.Id), out RecordBaseSubobject? item))
                    throw new InvalidDataException("Original base is missing its physical subobject relationship");
                RecordType complete = Records[item.CompleteBaseTypeId];
                if (complete.RepresentationKind != "complete" || complete.TypeId != Original(SourceTypeById(source.BaseSourceTypeId)).ExecutionTypeId ||
                    item.ByteOffset > record.Size || item.Extent > record.Size - item.ByteOffset || item.ByteOffset % complete.Alignment != 0 ||
                    complete.Alignment > record.Alignment)
                    throw new InvalidDataException("Base subobject nominal identity, extent or alignment mismatch");
                if (item.Extent == 0)
                {
                    if (RecordFields(complete.TypeId).Length != 0 || RecordBases(complete.TypeId).Any(nested => nested.Extent != 0))
                        throw new InvalidDataException("Optimized empty base must have no physical payload");
                    continue;
                }
                if (!Records.TryGetValue(item.PhysicalViewTypeId!.Value, out RecordType? view) ||
                    (view.TypeId != complete.TypeId && view.CompleteTypeId != complete.TypeId) || view.Size != item.Extent ||
                    !Fields.TryGetValue(item.FieldId!.Value, out RecordField? field) || field.RecordTypeId != record.TypeId || field.FieldRole != "base" ||
                    field.SourceBaseId != source.Id || field.TypeId != view.TypeId || field.ByteOffset != item.ByteOffset || field.BitWidth is not null)
                    throw new InvalidDataException("Base field does not match its exact physical view");
            }
            if (RecordBases(record.TypeId).Length != expected.Length)
                throw new InvalidDataException("Physical base relationship has no original base declaration");
            foreach (RecordField field in RecordFields(record.TypeId).Where(field => field.FieldRole == "base"))
                if (!BaseSubobjects.TryGetValue((record.TypeId, field.SourceBaseId!.Value), out RecordBaseSubobject? relationship) || relationship.FieldId != field.Id)
                    throw new InvalidDataException("Base field lacks its owned relationship");
        }
    }
}
