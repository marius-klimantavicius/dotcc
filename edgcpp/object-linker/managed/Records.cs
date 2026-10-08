using System.Text;

namespace DotObj;

internal sealed record RecordType(long TypeId, string IdentityKey, string SourceName,
    long Size, long Alignment, long? SourceFileId, long Line, long Column, string LayoutKind,
    long? SourceRecordId, string RepresentationKind, string ValueCopyKind, string CaptureProof, long? CompleteTypeId)
{
    // A nominal terminal permits recursive pointer graphs without recursively
    // incorporating the record's layout into every pointer signature.
    public string Key => "record(" + Convert.ToHexStringLower(Encoding.UTF8.GetBytes(IdentityKey)) + ")";
}

internal sealed record RecordField(long Id, long RecordTypeId, long Ordinal,
    long TypeId, string SourceName, long ByteOffset, long? SourceTypeId,
    long? SourceFileId, long Line, long Column, long? BitOffset, long? BitWidth, bool? BitSigned,
    long? SourceFieldId, string FieldRole, long? SourceBaseId);

internal sealed partial class ObjectFile
{
    public Dictionary<long, RecordType> Records { get; } = [];
    public Dictionary<long, RecordField> Fields { get; } = [];
    public Dictionary<string, RecordType> RecordsByKey { get; } = new(StringComparer.Ordinal);
    private readonly Dictionary<long, RecordField[]> fieldsByRecord = [];

    private void ReadRecordMetadata()
    {
        var groupedFields = new Dictionary<long, List<RecordField>>();
        using (var query = database.Prepare("SELECT type_id,identity_key,source_name,size,alignment,source_file_id,line,column,layout_kind,source_record_id,representation_kind,value_copy_kind,capture_proof,complete_type_id FROM record_types ORDER BY type_id"))
            while (query.Read())
            {
                var record = new RecordType(query.Integer(0), query.Text(1), query.Text(2), query.Integer(3), query.Integer(4),
                    query.OptionalInteger(5), query.Integer(6), query.Integer(7), query.Text(8), query.OptionalInteger(9), query.Text(10), query.Text(11), query.Text(12), query.OptionalInteger(13));
                if (record.TypeId <= 0 || record.IdentityKey.Length == 0 || record.Size < 0 || record.Size > int.MaxValue ||
                    (record.Size == 0 ? record.Alignment != 0 : record.Alignment is not (1 or 2 or 4 or 8 or 16)) ||
                    record.LayoutKind is not ("struct" or "class" or "union") || record.Line < 0 || record.Column < 0 ||
                    record.RepresentationKind is not ("complete" or "abi" or "subobject") || record.ValueCopyKind is not ("ordinary" or "explicit-only") ||
                    (record.RepresentationKind == "complete" ? record.SourceRecordId is null || record.CaptureProof is not ("source-snapshot" or "observed-class-lowering")
                        : record.RepresentationKind == "subobject" ? record.SourceRecordId is null || record.CompleteTypeId is null || record.Size == 0 || record.ValueCopyKind != "explicit-only" || record.CaptureProof != "observed-subobject-creation"
                        : record.SourceRecordId is not null || record.CaptureProof != "observed-abi-creation") ||
                    (record.RepresentationKind != "subobject" && record.CompleteTypeId is not null))
                    throw new InvalidDataException("Invalid record identity or layout metadata");
                if (!Records.TryAdd(record.TypeId, record) || !RecordsByKey.TryAdd(record.Key, record))
                    throw new InvalidDataException("Duplicate nominal record identity");
                groupedFields.Add(record.TypeId, []);
            }
        using (var query = database.Prepare("SELECT id,record_type_id,ordinal,type_id,source_name,byte_offset,source_type_id,source_file_id,line,column,bit_offset,bit_width,bit_signed,source_field_id,field_role,source_base_id FROM record_fields ORDER BY id"))
            while (query.Read())
            {
                var field = new RecordField(query.Integer(0), query.Integer(1), query.Integer(2), query.Integer(3), query.Text(4),
                    query.Integer(5), query.OptionalInteger(6), query.OptionalInteger(7), query.Integer(8), query.Integer(9), query.OptionalInteger(10), query.OptionalInteger(11), query.OptionalInteger(12) is long signed ? signed == 1 : null, query.OptionalInteger(13), query.Text(14), query.OptionalInteger(15));
                if (field.Id <= 0 || !Records.ContainsKey(field.RecordTypeId) || field.Ordinal < 0 ||
                    field.ByteOffset < 0 || field.Line < 0 || field.Column < 0 || field.FieldRole is not ("source" or "vptr" or "abi" or "base") ||
                    (field.FieldRole == "source" ? field.SourceTypeId is null or <= 0 || field.SourceFieldId is null or <= 0
                        : field.SourceTypeId is not null || field.SourceFieldId is not null) ||
                    (field.FieldRole == "base") != (field.SourceBaseId is not null))
                    throw new InvalidDataException("Invalid record field metadata");
                if ((field.BitOffset is null) != (field.BitWidth is null) || (field.BitOffset is null) != (field.BitSigned is null) ||
                    field.BitOffset is < 0 or > 7 || field.BitWidth is < 1 or > 64 || query.OptionalInteger(12) is < 0 or > 1)
                    throw new InvalidDataException("Invalid bitfield metadata");
                if (!Fields.TryAdd(field.Id, field)) throw new InvalidDataException("Duplicate record field identity");
                groupedFields[field.RecordTypeId].Add(field);
            }
        ReadBaseMetadata();
        foreach (var (typeId, fields) in groupedFields) fieldsByRecord.Add(typeId, fields.OrderBy(field => field.Ordinal).ToArray());
    }

    public RecordField[] RecordFields(long typeId) => fieldsByRecord[typeId];

    public bool CanCopyRecord(string type)
    {
        if (ScalarTypes.IsArray(type)) return ScalarTypes.ElementCount(type) > 0 && CanCopyRecord(ScalarTypes.Element(type));
        if (!ScalarTypes.IsRecord(type)) return true;
        RecordType record = RecordsByKey[type];
        return record.Size > 0 && record.ValueCopyKind == "ordinary" && CanCopyPayload(type);
    }

    public bool CanCopyPayload(string type)
    {
        if (ScalarTypes.IsArray(type)) return ScalarTypes.ElementCount(type) > 0 && CanCopyPayload(ScalarTypes.Element(type));
        if (!ScalarTypes.IsRecord(type)) return true;
        RecordType record = RecordsByKey[type];
        if (record.Size == 0) return false;
        foreach (RecordField field in RecordFields(record.TypeId))
        {
            if (!CanCopyPayload(Type(field.TypeId))) return false;
            if (field.SourceTypeId is long sourceTypeId && SourceTypes.TryGetValue(sourceTypeId, out SourceType? source))
                while (true)
                {
                    if (source.IsVolatile) return false;
                    if (source.Kind is not ("typedef" or "qualified" or "array")) break;
                    source = SourceTypes[source.UnderlyingTypeId!.Value];
                }
        }
        return true;
    }

    public static long ClrRecordPack(RecordType record)
    {
        long pack = Math.Max(1, record.Alignment);
        if (record.RepresentationKind == "subobject")
            while (record.Size % pack != 0) pack /= 2;
        return pack;
    }

    public long StorageSize(string type) => ScalarTypes.IsRecord(type) ? RecordsByKey[type].Size
        : ScalarTypes.IsArray(type) ? checked(ScalarTypes.ElementCount(type) * StorageSize(ScalarTypes.Element(type))) : ScalarTypes.Size(type);

    public long StorageAlignment(string type) => type == "va-list" ? 8 : ScalarTypes.IsRecord(type) ? RecordsByKey[type].Alignment
        : ScalarTypes.IsArray(type) ? StorageAlignment(ScalarTypes.Element(type)) : ScalarTypes.Size(type);

    private void VerifyRecordLayouts()
    {
        var complete = new HashSet<long>();
        var layoutDepths = new Dictionary<long, int>();
        var active = new HashSet<long>();
        void Verify(RecordType record, int depth)
        {
            if (complete.Contains(record.TypeId)) return;
            if (depth > 128 || !active.Add(record.TypeId)) throw new InvalidDataException("Cyclic or excessively nested by-value record layout");
            if (Type(record.TypeId) != record.Key) throw new InvalidDataException("Record layout does not describe a record execution type");
            RecordField[] fields = RecordFields(record.TypeId);
            if (record.Size == 0)
            {
                if (fields.Length != 0) throw new InvalidDataException("Incomplete record has fields");
                active.Remove(record.TypeId); layoutDepths.Add(record.TypeId, 0); complete.Add(record.TypeId); return;
            }
            if (record.RepresentationKind != "subobject" && record.Size % record.Alignment != 0) throw new InvalidDataException("Complete record size must be a multiple of its alignment");
            int layoutDepth = 0;
            var names = new HashSet<string>(StringComparer.Ordinal);
            var occupied = new List<(long Begin, long End)>();
            for (int index = 0; index < fields.Length; index++)
            {
                RecordField field = fields[index];
                if (field.BitWidth is not null && ScalarTypes.IsWideInteger(Type(field.TypeId)))
                    throw new InvalidDataException("128-bit bitfields require an explicit bit storage contract");
                if (field.Ordinal != index || field.SourceName.Length != 0 && !names.Add(field.SourceName)) throw new InvalidDataException("Invalid field order or duplicate field name");
                string type = Type(field.TypeId), leaf = ScalarTypes.Leaf(type);
                if (type == "void" || RoutineTypes.IsRoutine(type)) throw new InvalidDataException("Record field must have a storage type");
                if (ScalarTypes.IsRecord(leaf))
                {
                    RecordType nested = RecordsByKey[leaf]; Verify(nested, depth + 1);
                    if (nested.RepresentationKind == "subobject" && (field.FieldRole != "base" || ScalarTypes.IsArray(type))) throw new InvalidDataException("Subobject representation must be an owned base view");
                    if (nested.Size == 0) throw new InvalidDataException("Incomplete record field by value");
                    layoutDepth = Math.Max(layoutDepth, layoutDepths[nested.TypeId] + 1);
                    if (layoutDepth > 128) throw new InvalidDataException("By-value record layout nesting exceeds supported limit");
                }
                if (field.BitWidth is null && (StorageAlignment(type) > record.Alignment || field.ByteOffset % StorageAlignment(type) != 0))
                    throw new InvalidDataException("Ordinary field does not satisfy its natural alignment");
                long begin = checked(field.ByteOffset * 8), end;
                if (field.BitWidth is long width)
                {
                    if (!ScalarTypes.IsInteger(type) || width > ScalarTypes.Size(type) * 8) throw new InvalidDataException("Invalid bitfield integer representation");
                    begin = checked(begin + field.BitOffset!.Value); end = checked(begin + width);
                }
                else end = checked(begin + StorageSize(type) * 8);
                if (ScalarTypes.IsArray(type) && ScalarTypes.ElementCount(type) == 0)
                {
                    if (index != fields.Length - 1 || record.LayoutKind == "union" || field.ByteOffset > record.Size)
                        throw new InvalidDataException("Incomplete array field must be a flexible tail member");
                    continue;
                }
                if (end > record.Size * 8 || begin >= end) throw new InvalidDataException("Field extent lies outside complete record storage");
                if (record.LayoutKind != "union" && occupied.Any(other => begin < other.End && other.Begin < end))
                    throw new InvalidDataException("Overlapping non-union fields");
                occupied.Add((begin, end));
            }
            active.Remove(record.TypeId);
            layoutDepths.Add(record.TypeId, layoutDepth);
            complete.Add(record.TypeId);
        }
        foreach (RecordType record in Records.Values) Verify(record, 0);
    }
}
