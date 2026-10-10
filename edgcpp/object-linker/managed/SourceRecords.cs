namespace DotObj;

internal sealed record SourceRecord(long Id, long SourceTypeId, string IdentityKey, string SourceName,
    string RecordKind, bool IsComplete, long NativeSize, long NativeAlignment, bool? IsTriviallyCopyable,
    string CapturePhase, long? SourceFileId, long Line, long Column, bool IsPacked, long MaxMemberAlignment, long ExplicitAlignment,
    bool LayoutKnown, bool IsDependent, bool IsPrototypeInstantiation);
internal sealed record SourceRecordField(long Id, long SourceRecordId, long Ordinal, long SourceTypeId,
    string SourceName, bool IsMutable, bool HasNoUniqueAddress, long? BitWidth,
    long? SourceFileId, long Line, long Column, bool HasInitializer, bool IsPacked, long ExplicitAlignment);

internal sealed partial class ObjectFile
{
    public Dictionary<long, SourceRecord> SourceRecords { get; } = [];
    public Dictionary<long, SourceRecord> SourceRecordsByType { get; } = [];
    public Dictionary<long, SourceRecordField> SourceFields { get; } = [];
    private readonly Dictionary<long, SourceRecordField[]> sourceFieldsByRecord = [];

    private void ReadSourceRecordMetadata()
    {
        using (var query = database.Prepare("SELECT id,source_type_id,identity_key,source_name,record_kind,is_complete,native_size,native_alignment,is_trivially_copyable,capture_phase,source_file_id,line,column,is_packed,max_member_alignment,explicit_alignment,layout_known,is_dependent,is_prototype_instantiation FROM source_record_types ORDER BY id"))
            while (query.Read())
            {
                var record = new SourceRecord(query.Integer(0), query.Integer(1), query.Text(2), query.Text(3), query.Text(4),
                    query.Integer(5) == 1, query.Integer(6), query.Integer(7), query.OptionalInteger(8) is long copyable ? copyable == 1 : null,
                    query.Text(9), query.OptionalInteger(10), query.Integer(11), query.Integer(12), query.Integer(13) == 1, query.Integer(14), query.Integer(15), query.Integer(16) == 1, query.Integer(17) == 1, query.Integer(18) == 1);
                if (record.Id <= 0 || record.SourceTypeId <= 0 || record.IdentityKey.Length == 0 ||
                    record.RecordKind is not ("struct" or "union" or "class") || query.Integer(5) is not (0 or 1) ||
                    query.OptionalInteger(8) is not (null or 0 or 1) || record.NativeSize < 0 || record.NativeAlignment < 0 ||
                    record.Line < 0 || record.Column < 0 || record.CapturePhase is not ("before-lowering" or "before-class-prelowering") ||
                    query.Integer(13) is not (0 or 1) || !AlignmentDirective(record.MaxMemberAlignment) || !AlignmentDirective(record.ExplicitAlignment) ||
                    query.Integer(16) is not (0 or 1) || query.Integer(17) is not (0 or 1) || query.Integer(18) is not (0 or 1) ||
                    !record.IsComplete && (record.LayoutKnown || record.IsTriviallyCopyable is not null) ||
                    (record.LayoutKnown ? !record.IsComplete || record.IsDependent || record.NativeAlignment == 0
                        : record.NativeSize != 0 || record.NativeAlignment != 0 || record.IsTriviallyCopyable is not null) ||
                    record.IsPrototypeInstantiation && !record.IsDependent)
                    throw new InvalidDataException("Invalid original record identity, completeness or capture metadata");
                if (!SourceRecords.TryAdd(record.Id, record) || !SourceRecordsByType.TryAdd(record.SourceTypeId, record)) throw new InvalidDataException("Duplicate original record ID");
            }
        using (var query = database.Prepare("SELECT id,source_record_id,ordinal,source_type_id,source_name,is_mutable,has_no_unique_address,bit_width,source_file_id,line,column,has_initializer,is_packed,explicit_alignment FROM source_record_fields ORDER BY source_record_id,ordinal"))
            while (query.Read())
            {
                var field = new SourceRecordField(query.Integer(0), query.Integer(1), query.Integer(2), query.Integer(3), query.Text(4),
                    query.Integer(5) == 1, query.Integer(6) == 1, query.OptionalInteger(7), query.OptionalInteger(8), query.Integer(9), query.Integer(10), query.Integer(11) == 1, query.Integer(12) == 1, query.Integer(13));
                if (field.Id <= 0 || !SourceRecords.ContainsKey(field.SourceRecordId) || field.Ordinal < 0 || field.SourceTypeId <= 0 ||
                    query.Integer(5) is not (0 or 1) || query.Integer(6) is not (0 or 1) || query.Integer(11) is not (0 or 1) || query.Integer(12) is not (0 or 1) || !AlignmentDirective(field.ExplicitAlignment) || field.BitWidth is < 0 || field.Line < 0 || field.Column < 0)
                    throw new InvalidDataException("Invalid original record field metadata");
                if (!SourceFields.TryAdd(field.Id, field)) throw new InvalidDataException("Duplicate original record field ID");
            }
        foreach (var group in SourceFields.Values.GroupBy(field => field.SourceRecordId))
            sourceFieldsByRecord.Add(group.Key, group.OrderBy(field => field.Ordinal).ToArray());
    }

    private static bool AlignmentDirective(long alignment) => alignment >= 0 && (alignment & (alignment - 1)) == 0;

    private void VerifySourceRecords()
    {
        VerifyBaseMetadata();
        foreach (SourceRecord source in SourceRecords.Values)
        {
            SourceType type = SourceTypeById(source.SourceTypeId);
            if (source.IdentityKey.StartsWith("source-prototype:", StringComparison.Ordinal) !=
                (source.IsDependent && source.IsPrototypeInstantiation && !SourceRecordOwners.ContainsKey(source.Id) &&
                 !SourceNestedTypeDeclarations.ContainsKey(source.SourceTypeId)))
                throw new InvalidDataException("Original prototype identity differs from its dependent declaration facts");
            if (type.Kind != "record" || type.ExecutionRepresentationId is long physical && (!Records.TryGetValue(physical, out RecordType? execution) ||
                source.IsDependent || source.IsComplete && (!source.LayoutKnown || source.IsTriviallyCopyable is null) ||
                execution.SourceRecordId != source.Id || execution.RepresentationKind != "complete" ||
                source.IdentityKey != execution.IdentityKey || source.SourceName != execution.SourceName || source.RecordKind != execution.LayoutKind ||
                source.IsComplete != (execution.Size != 0) || source.NativeSize != execution.Size || source.NativeAlignment != execution.Alignment))
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
                if (field.BitWidth is long width)
                {
                    SourceType integer = Original(member);
                    if (integer.Kind == "enum") integer = Original(SourceTypeById(SourceEnums[integer.Id].UnderlyingTypeId));
                    var shape = SourceIntegerShape(integer);
                    long storageBits = member.ExecutionRepresentationId is long physicalMember
                        ? StorageSize(Type(physicalMember)) * 8 : Math.Max(8, shape.Bits);
                    if (width > storageBits || width == 0 && field.SourceName.Length != 0)
                        throw new InvalidDataException("Original bitfield width or unnamed zero-width declaration is invalid");
                }
            }
        }
        foreach (RecordType record in Records.Values)
        {
            RecordField[] fields = RecordFields(record.TypeId);
            if (record.RepresentationKind == "abi")
            {
                if (record.SourceRecordId is not null || record.CaptureProof != "observed-abi-creation" || record.ValueCopyKind != "explicit-only" ||
                    record.Size == 0 || fields.Any(field => field.FieldRole is not ("abi" or "abi-code-address")))
                    throw new InvalidDataException("ABI record requires observed creation, hidden fields and explicit copy operations");
                var codeAddresses = fields.Where(field => field.FieldRole == "abi-code-address").ToArray();
                if (codeAddresses.Length != (record.IdentityKey == "abi:member-function-pointer" ? 1 : 0) ||
                    codeAddresses.Any(field => Type(field.TypeId) != "ptr(void)" || field.BitWidth is not null))
                    throw new InvalidDataException("Erased ABI code address requires its observed member-function-pointer representation");
                if (record.IdentityKey == "abi:member-function-pointer" &&
                    (record.LayoutKind != "struct" || record.Size != 16 || record.Alignment != 8 || fields.Length != 2 ||
                     fields[0] is not { Ordinal: 0, SourceName: "f", FieldRole: "abi-code-address", ByteOffset: 0, BitWidth: null } || Type(fields[0].TypeId) != "ptr(void)" ||
                     fields[1] is not { Ordinal: 1, SourceName: "d", FieldRole: "abi", ByteOffset: 8, BitWidth: null } || Type(fields[1].TypeId) != "i64"))
                    throw new InvalidDataException("Observed member-function-pointer ABI must retain its exact LP64 code-address and adjustment layout");
                continue;
            }
            if (record.SourceRecordId is not long sourceId || !SourceRecords.TryGetValue(sourceId, out SourceRecord? source))
                throw new InvalidDataException("Complete record is missing original source capture");
            if (SourceTypeById(source.SourceTypeId).ExecutionRepresentationId !=
                (record.RepresentationKind == "subobject" ? record.CompleteTypeId : record.TypeId))
                throw new InvalidDataException("Physical record requires its original type's exact complete execution mapping");
            if (record.ValueCopyKind == "ordinary" && (!source.IsComplete || source.IsTriviallyCopyable != true))
                throw new InvalidDataException("Ordinary record values require a completed trivially-copyable source record");
            if (record.CaptureProof is "observed-class-lowering" or "observed-subobject-creation" && (!source.IsComplete || source.CapturePhase != "before-class-prelowering"))
                throw new InvalidDataException("Observed class lowering requires an original complete prelowering snapshot");
            var mapped = new HashSet<long>();
            int vptrs = 0;
            foreach (RecordField field in fields)
            {
                if (field.FieldRole is "base" or "virtual-base") continue;
                if (field.FieldRole == "vptr")
                {
                    ++vptrs;
                    if (record.CaptureProof is not ("observed-class-lowering" or "observed-subobject-creation") || record.ValueCopyKind != "explicit-only" || source.IsTriviallyCopyable != false ||
                        field.ByteOffset != 0 || !ScalarTypes.IsPointer(Type(field.TypeId)) || field.BitWidth is not null)
                        throw new InvalidDataException("Own vptr requires observed nontrivial class lowering at pointer offset zero");
                    continue;
                }
                if (field.FieldRole != "source" || field.SourceFieldId is not long fieldId || !SourceFields.TryGetValue(fieldId, out SourceRecordField? original) ||
                    !mapped.Add(fieldId) || original.SourceRecordId != source.Id || original.SourceTypeId != field.SourceTypeId ||
                    original.SourceName.Length != 0 && original.SourceName != field.SourceName || original.BitWidth != field.BitWidth)
                    throw new InvalidDataException("Physical field does not correspond to its original member declaration");
                if (field.BitWidth is null)
                {
                    long minimum = StorageAlignment(Type(field.TypeId));
                    if (source.IsPacked || original.IsPacked) minimum = 1;
                    if (source.MaxMemberAlignment != 0) minimum = Math.Min(minimum, source.MaxMemberAlignment);
                    if (field.ObservedAlignment < minimum)
                        throw new InvalidDataException("Reduced field alignment requires original packing evidence");
                }
            }
            if (vptrs > 1) throw new InvalidDataException("Only a class's single own vptr is supported in this profile");
            foreach (SourceRecordField field in sourceFieldsByRecord.GetValueOrDefault(source.Id, []))
                if (!mapped.Contains(field.Id) && !(field.SourceName.Length == 0 && field.BitWidth is not null))
                    throw new InvalidDataException("Original nonzero member has no physical representation");
        }
    }
}
