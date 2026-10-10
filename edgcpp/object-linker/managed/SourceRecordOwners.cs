using System.Globalization;
using System.Text;

namespace DotObj;

internal sealed record SourceRecordOwner(long SourceRecordId, long OwnerSourceRecordId, long OwnerFieldId,
    string Kind, string CaptureProof);

internal sealed partial class ObjectFile
{
    public Dictionary<long, SourceRecordOwner> SourceRecordOwners { get; } = [];

    private void ReadAndVerifySourceRecordOwners()
    {
        using (var query = database.Prepare("SELECT source_record_id,owner_source_record_id,owner_field_id,kind,capture_proof FROM source_record_owners ORDER BY source_record_id"))
            while (query.Read())
            {
                var item = new SourceRecordOwner(query.Integer(0), query.Integer(1), query.Integer(2), query.Text(3), query.Text(4));
                if (SourceLanguage != "c++" || !SourceRecords.TryGetValue(item.SourceRecordId, out SourceRecord? child) ||
                    !SourceRecords.TryGetValue(item.OwnerSourceRecordId, out SourceRecord? owner) || child.Id == owner.Id ||
                    child.SourceName.Length != 0 || !child.IsComplete || !owner.IsComplete ||
                    !SourceFields.TryGetValue(item.OwnerFieldId, out SourceRecordField? field) || field.SourceRecordId != owner.Id ||
                    item.Kind != "anonymous-member" || item.CaptureProof != "original-member-declaration")
                    throw new InvalidDataException("Invalid original anonymous member ownership");
                SourceType leaf = AnonymousMemberLeaf(field.SourceTypeId);
                if (leaf.Id != child.SourceTypeId || leaf.Kind != "record" ||
                    sourceFieldsByRecord.GetValueOrDefault(owner.Id, []).Any(prior => prior.Ordinal < field.Ordinal &&
                        AnonymousMemberLeaf(prior.SourceTypeId).Id == leaf.Id))
                    throw new InvalidDataException("Anonymous member owner must be its first exact original declarator");
                string identity = "record-member:" + Encoding.UTF8.GetByteCount(owner.IdentityKey).ToString(CultureInfo.InvariantCulture) +
                    ":" + owner.IdentityKey + ":field:" + field.Ordinal.ToString(CultureInfo.InvariantCulture);
                if (child.IdentityKey != identity)
                    throw new InvalidDataException("Anonymous member identity differs from its original owner and ordinal");
                SourceRecordOwners.Add(item.SourceRecordId, item);
            }
        foreach (SourceRecord record in SourceRecords.Values)
        {
            if (record.IdentityKey.StartsWith("record-member:", StringComparison.Ordinal) && !SourceRecordOwners.ContainsKey(record.Id))
                throw new InvalidDataException("Anonymous member identity lacks owned declaration provenance");
            var seen = new HashSet<long>();
            long current = record.Id;
            while (SourceRecordOwners.TryGetValue(current, out SourceRecordOwner? owner))
            {
                if (!seen.Add(current) || seen.Count > 128)
                    throw new InvalidDataException("Original anonymous record ownership is cyclic or exceeds supported depth");
                current = owner.OwnerSourceRecordId;
            }
        }
    }

    private SourceType AnonymousMemberLeaf(long id)
    {
        var seen = new HashSet<long>();
        SourceType type = SourceTypeById(id);
        while (type.Kind is "typedef" or "qualified" or "pointer" or "lvalue-reference" or "rvalue-reference" or "array")
        {
            if (!seen.Add(type.Id) || seen.Count > 128 || type.UnderlyingTypeId is not long child)
                throw new InvalidDataException("Invalid anonymous member declarator type path");
            type = SourceTypeById(child);
        }
        return type;
    }
}
