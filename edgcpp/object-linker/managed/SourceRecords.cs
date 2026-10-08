namespace DotObj;

internal sealed record SourceRecord(long Id, long SourceTypeId, string IdentityKey, string SourceName,
    string RecordKind, bool IsComplete, long NativeSize, long NativeAlignment, bool? IsTriviallyCopyable,
    string CapturePhase, long? SourceFileId, long Line, long Column);
internal sealed record SourceRecordField(long Id, long SourceRecordId, long Ordinal, long SourceTypeId,
    string SourceName, bool IsMutable, bool HasNoUniqueAddress, long? BitWidth,
    long? SourceFileId, long Line, long Column, bool HasInitializer);

internal sealed partial class ObjectFile
{
    public Dictionary<long, SourceRecord> SourceRecords { get; } = [];
    public Dictionary<long, SourceRecordField> SourceFields { get; } = [];
    private readonly Dictionary<long, SourceRecordField[]> sourceFieldsByRecord = [];

    private void ReadSourceRecordMetadata()
    {
        using (var query = database.Prepare("SELECT id,source_type_id,identity_key,source_name,record_kind,is_complete,native_size,native_alignment,is_trivially_copyable,capture_phase,source_file_id,line,column FROM source_record_types ORDER BY id"))
            while (query.Read())
            {
                var record = new SourceRecord(query.Integer(0), query.Integer(1), query.Text(2), query.Text(3), query.Text(4),
                    query.Integer(5) == 1, query.Integer(6), query.Integer(7), query.OptionalInteger(8) is long copyable ? copyable == 1 : null,
                    query.Text(9), query.OptionalInteger(10), query.Integer(11), query.Integer(12));
                if (record.Id <= 0 || record.SourceTypeId <= 0 || record.IdentityKey.Length == 0 ||
                    record.RecordKind is not ("struct" or "union" or "class") || query.Integer(5) is not (0 or 1) ||
                    query.OptionalInteger(8) is not (null or 0 or 1) || record.NativeSize < 0 || record.NativeAlignment < 0 ||
                    record.Line < 0 || record.Column < 0 || record.CapturePhase is not ("before-lowering" or "before-class-prelowering") ||
                    (record.IsComplete ? record.NativeAlignment == 0 || record.IsTriviallyCopyable is null
                        : record.NativeSize != 0 || record.NativeAlignment != 0 || record.IsTriviallyCopyable is not null))
                    throw new InvalidDataException("Invalid original record identity, completeness or capture metadata");
                if (!SourceRecords.TryAdd(record.Id, record)) throw new InvalidDataException("Duplicate original record ID");
            }
        using (var query = database.Prepare("SELECT id,source_record_id,ordinal,source_type_id,source_name,is_mutable,has_no_unique_address,bit_width,source_file_id,line,column,has_initializer FROM source_record_fields ORDER BY source_record_id,ordinal"))
            while (query.Read())
            {
                var field = new SourceRecordField(query.Integer(0), query.Integer(1), query.Integer(2), query.Integer(3), query.Text(4),
                    query.Integer(5) == 1, query.Integer(6) == 1, query.OptionalInteger(7), query.OptionalInteger(8), query.Integer(9), query.Integer(10), query.Integer(11) == 1);
                if (field.Id <= 0 || !SourceRecords.ContainsKey(field.SourceRecordId) || field.Ordinal < 0 || field.SourceTypeId <= 0 ||
                    query.Integer(5) is not (0 or 1) || query.Integer(6) is not (0 or 1) || query.Integer(11) is not (0 or 1) || field.BitWidth is < 0 || field.Line < 0 || field.Column < 0)
                    throw new InvalidDataException("Invalid original record field metadata");
                if (!SourceFields.TryAdd(field.Id, field)) throw new InvalidDataException("Duplicate original record field ID");
            }
        foreach (var group in SourceFields.Values.GroupBy(field => field.SourceRecordId))
            sourceFieldsByRecord.Add(group.Key, group.OrderBy(field => field.Ordinal).ToArray());
    }

    private void VerifySourceRecords()
    {
        foreach (SourceRecord source in SourceRecords.Values)
        {
            SourceType type = SourceTypeById(source.SourceTypeId);
            if (type.Kind != "record" || !Records.TryGetValue(type.ExecutionTypeId, out RecordType? execution) ||
                execution.SourceRecordId != source.Id || execution.RepresentationKind != "complete" ||
                source.IdentityKey != execution.IdentityKey || source.SourceName != execution.SourceName || source.RecordKind != execution.LayoutKind ||
                source.IsComplete != (execution.Size != 0) || source.NativeSize != execution.Size || source.NativeAlignment != execution.Alignment)
                throw new InvalidDataException("Original record does not match its complete execution representation");
            SourceRecordField[] fields = sourceFieldsByRecord.GetValueOrDefault(source.Id, []);
            if (!source.IsComplete && fields.Length != 0) throw new InvalidDataException("Incomplete source record contains member declarations");
            var names = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < fields.Length; ++index)
            {
                SourceRecordField field = fields[index];
                if (field.Ordinal != index || field.SourceName.Length != 0 && !names.Add(field.SourceName))
                    throw new InvalidDataException("Original record field order or names are invalid");
                SourceType member = SourceTypeById(field.SourceTypeId);
                if (field.BitWidth is long width && (!ScalarTypes.IsInteger(Type(member.ExecutionTypeId)) ||
                    width > StorageSize(Type(member.ExecutionTypeId)) * 8 || width == 0 && field.SourceName.Length != 0))
                    throw new InvalidDataException("Original bitfield width or unnamed zero-width declaration is invalid");
            }
        }
        foreach (RecordType record in Records.Values)
        {
            RecordField[] fields = RecordFields(record.TypeId);
            if (record.RepresentationKind == "abi")
            {
                if (record.SourceRecordId is not null || record.CaptureProof != "observed-abi-creation" || record.ValueCopyKind != "explicit-only" ||
                    record.Size == 0 || fields.Any(field => field.FieldRole != "abi"))
                    throw new InvalidDataException("ABI record requires observed creation, hidden fields and explicit copy operations");
                continue;
            }
            if (record.SourceRecordId is not long sourceId || !SourceRecords.TryGetValue(sourceId, out SourceRecord? source))
                throw new InvalidDataException("Complete record is missing original source capture");
            if (record.ValueCopyKind == "ordinary" && (!source.IsComplete || source.IsTriviallyCopyable != true))
                throw new InvalidDataException("Ordinary record values require a completed trivially-copyable source record");
            if (record.CaptureProof == "observed-class-lowering" && (!source.IsComplete || source.CapturePhase != "before-class-prelowering"))
                throw new InvalidDataException("Observed class lowering requires an original complete prelowering snapshot");
            var mapped = new HashSet<long>();
            int vptrs = 0;
            foreach (RecordField field in fields)
            {
                if (field.FieldRole == "vptr")
                {
                    ++vptrs;
                    if (record.CaptureProof != "observed-class-lowering" || record.ValueCopyKind != "explicit-only" || source.IsTriviallyCopyable != false ||
                        field.ByteOffset != 0 || !ScalarTypes.IsPointer(Type(field.TypeId)) || field.BitWidth is not null)
                        throw new InvalidDataException("Own vptr requires observed nontrivial class lowering at pointer offset zero");
                    continue;
                }
                if (field.FieldRole != "source" || field.SourceFieldId is not long fieldId || !SourceFields.TryGetValue(fieldId, out SourceRecordField? original) ||
                    !mapped.Add(fieldId) || original.SourceRecordId != source.Id || original.SourceTypeId != field.SourceTypeId ||
                    original.SourceName.Length != 0 && original.SourceName != field.SourceName || original.BitWidth != field.BitWidth)
                    throw new InvalidDataException("Physical field does not correspond to its original member declaration");
            }
            if (vptrs > 1) throw new InvalidDataException("Only a class's single own vptr is supported in this profile");
            foreach (SourceRecordField field in sourceFieldsByRecord.GetValueOrDefault(source.Id, []))
                if (!mapped.Contains(field.Id) && !(field.SourceName.Length == 0 && field.BitWidth is not null))
                    throw new InvalidDataException("Original nonzero member has no physical representation");
        }
    }
}
