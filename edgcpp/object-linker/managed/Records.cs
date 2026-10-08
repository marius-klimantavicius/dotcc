using System.Text;

namespace DotObj;

internal sealed record RecordType(long TypeId, string IdentityKey, string SourceName,
    long Size, long Alignment, long? SourceFileId, long Line, long Column)
{
    // A nominal terminal permits recursive pointer graphs without recursively
    // incorporating the record's layout into every pointer signature.
    public string Key => "record(" + Convert.ToHexStringLower(Encoding.UTF8.GetBytes(IdentityKey)) + ")";
}

internal sealed record RecordField(long Id, long RecordTypeId, long Ordinal,
    long TypeId, string SourceName, long ByteOffset, long SourceTypeId,
    long? SourceFileId, long Line, long Column);

internal sealed partial class ObjectFile
{
    public Dictionary<long, RecordType> Records { get; } = [];
    public Dictionary<long, RecordField> Fields { get; } = [];
    public Dictionary<string, RecordType> RecordsByKey { get; } = new(StringComparer.Ordinal);
    private readonly Dictionary<long, RecordField[]> fieldsByRecord = [];

    private void ReadRecordMetadata()
    {
        var groupedFields = new Dictionary<long, List<RecordField>>();
        using (var query = database.Prepare("SELECT type_id,identity_key,source_name,size,alignment,source_file_id,line,column FROM record_types ORDER BY type_id"))
            while (query.Read())
            {
                var record = new RecordType(query.Integer(0), query.Text(1), query.Text(2), query.Integer(3), query.Integer(4),
                    query.OptionalInteger(5), query.Integer(6), query.Integer(7));
                if (record.TypeId <= 0 || !record.IdentityKey.StartsWith("record:", StringComparison.Ordinal) ||
                    record.IdentityKey.Length <= 7 || record.SourceName.Length == 0 || record.Size is < 1 or > 65_536 ||
                    record.Alignment is not (1 or 2 or 4 or 8) || record.Line < 0 || record.Column < 0)
                    throw new InvalidDataException("Invalid record identity or natural layout metadata");
                string[] segments = record.IdentityKey[7..].Split("::", StringSplitOptions.None);
                if (segments[^1] != record.SourceName || segments.Any(segment => segment.Length == 0 ||
                    segment.Any(character => character == ':' || char.IsWhiteSpace(character) || char.IsControl(character))))
                    throw new InvalidDataException("Record identity must be its canonical qualified source name");
                if (!Records.TryAdd(record.TypeId, record) || !RecordsByKey.TryAdd(record.Key, record))
                    throw new InvalidDataException("Duplicate nominal record identity");
                groupedFields.Add(record.TypeId, []);
            }
        using (var query = database.Prepare("SELECT id,record_type_id,ordinal,type_id,source_name,byte_offset,source_type_id,source_file_id,line,column FROM record_fields ORDER BY id"))
            while (query.Read())
            {
                var field = new RecordField(query.Integer(0), query.Integer(1), query.Integer(2), query.Integer(3), query.Text(4),
                    query.Integer(5), query.Integer(6), query.OptionalInteger(7), query.Integer(8), query.Integer(9));
                if (field.Id <= 0 || !Records.ContainsKey(field.RecordTypeId) || field.Ordinal < 0 ||
                    field.SourceName.Length == 0 || field.ByteOffset < 0 || field.SourceTypeId <= 0 || field.Line < 0 || field.Column < 0)
                    throw new InvalidDataException("Invalid record field metadata");
                if (!Fields.TryAdd(field.Id, field)) throw new InvalidDataException("Duplicate record field identity");
                groupedFields[field.RecordTypeId].Add(field);
            }
        foreach (var (typeId, fields) in groupedFields) fieldsByRecord.Add(typeId, fields.OrderBy(field => field.Ordinal).ToArray());
    }

    public RecordField[] RecordFields(long typeId) => fieldsByRecord[typeId];

    public long StorageSize(string type) => ScalarTypes.IsRecord(type) ? RecordsByKey[type].Size
        : ScalarTypes.IsArray(type) ? checked(ScalarTypes.ElementCount(type) * StorageSize(ScalarTypes.Element(type))) : ScalarTypes.Size(type);

    public long StorageAlignment(string type) => ScalarTypes.IsRecord(type) ? RecordsByKey[type].Alignment
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
            if (fields.Length == 0) throw new InvalidDataException("Empty records are not supported in this profile");
            long end = 0, alignment = 1;
            int layoutDepth = 0;
            var names = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < fields.Length; index++)
            {
                RecordField field = fields[index];
                if (field.Ordinal != index || !names.Add(field.SourceName)) throw new InvalidDataException("Invalid field order or duplicate field name");
                string type = Type(field.TypeId);
                if (type == "void") throw new InvalidDataException("Record field cannot have void type");
                if (ScalarTypes.IsRecord(type))
                {
                    RecordType nested = RecordsByKey[type];
                    Verify(nested, depth + 1);
                    layoutDepth = Math.Max(layoutDepth, layoutDepths[nested.TypeId] + 1);
                    if (layoutDepth > 128) throw new InvalidDataException("By-value record layout nesting exceeds supported limit");
                }
                long fieldSize = StorageSize(type), fieldAlignment = StorageAlignment(type);
                alignment = Math.Max(alignment, fieldAlignment);
                long offset = checked((end + fieldAlignment - 1) / fieldAlignment * fieldAlignment);
                if (field.ByteOffset != offset) throw new InvalidDataException("Record field offset differs from supported natural layout");
                end = checked(offset + fieldSize);
            }
            long size = Math.Max(1, checked((end + alignment - 1) / alignment * alignment));
            if (record.Size != size || record.Alignment != alignment)
                throw new InvalidDataException("Record size or alignment differs from supported natural layout");
            active.Remove(record.TypeId);
            layoutDepths.Add(record.TypeId, layoutDepth);
            complete.Add(record.TypeId);
        }
        foreach (RecordType record in Records.Values) Verify(record, 0);
    }
}
