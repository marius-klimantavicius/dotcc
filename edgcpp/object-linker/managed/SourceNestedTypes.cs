namespace DotObj;

internal sealed partial class ObjectFile
{
    public Dictionary<long, long> SourceNestedTypeDeclarations { get; } = [];

    private void ReadSourceNestedTypeDeclarations()
    {
        using var query = database.Prepare("SELECT type_id,owner_type_id,capture_proof FROM source_nested_type_declarations ORDER BY type_id");
        while (query.Read())
        {
            long id = query.Integer(0), owner = query.Integer(1);
            if (!SourceTypes.TryGetValue(id, out var type) || type.Kind is not ("record" or "enum" or "typedef") ||
                !SourceTypes.TryGetValue(owner, out var enclosing) || enclosing.Kind != "record" || owner == id ||
                type.SourceName.Length == 0 || type.SourceFileId is null || type.Line <= 0 ||
                query.Text(2) != "original-nested-declaration" || !SourceNestedTypeDeclarations.TryAdd(id, owner))
                throw new InvalidDataException("Nested source declaration lacks its original named type and enclosing class");
        }
    }

    private string SourceNestedTypeIdentity(long id, int depth = 0)
    {
        if (depth > 128) throw new InvalidDataException("Nested source declaration identity cycle/depth");
        var type = SourceTypeById(id);
        return "source-nested-declaration:" + SourceIdentityFrame(SourceArgumentTypeIdentity(SourceNestedTypeDeclarations[id], depth + 1)) +
            SourceIdentityFrame(type.Kind) + SourceIdentityFrame(type.SourceName);
    }

    private void VerifySourceNestedTypeDeclarations()
    {
        var referenced = new HashSet<long>();
        foreach (var member in SourceDependentTypes.Values.Where(value => value.OriginalNestedTypeId is not null))
        {
            long declaration = member.OriginalNestedTypeId!.Value;
            if (member.Kind != "member" || !SourceNestedTypeDeclarations.TryGetValue(declaration, out long owner) ||
                member.OwnerTypeId != owner || SourceTypeById(member.TypeId).SourceName != SourceTypeById(declaration).SourceName)
                throw new InvalidDataException("Nonreal nested type differs from its exact original declaration owner/name");
            referenced.Add(declaration);
        }
        foreach (var pair in SourceNestedTypeDeclarations)
        {
            if (!referenced.Contains(pair.Key)) throw new InvalidDataException("Nested declaration has no original dependent view");
            var seen = new HashSet<long>();
            long owner = pair.Key;
            while (SourceNestedTypeDeclarations.TryGetValue(owner, out long next))
            {
                if (!seen.Add(owner) || seen.Count > 128) throw new InvalidDataException("Nested source declaration ownership cycle/depth");
                owner = next;
            }
            var type = SourceTypeById(pair.Key);
            string identity = SourceNestedTypeIdentity(pair.Key);
            if (type.Kind == "record" && SourceRecordsByType[type.Id].IdentityKey != identity ||
                type.Kind == "enum" && SourceEnums[type.Id].IdentityKey != identity)
                throw new InvalidDataException("Nested type identity differs from its exact original enclosing declaration");
        }
        foreach (var record in SourceRecords.Values)
            if (record.IdentityKey.StartsWith("source-nested-declaration:", StringComparison.Ordinal) &&
                !SourceNestedTypeDeclarations.ContainsKey(record.SourceTypeId))
                throw new InvalidDataException("Nested source record identity lacks declaration provenance");
        foreach (var enumeration in SourceEnums.Values)
            if (enumeration.IdentityKey.StartsWith("source-nested-declaration:", StringComparison.Ordinal) &&
                !SourceNestedTypeDeclarations.ContainsKey(enumeration.TypeId))
                throw new InvalidDataException("Nested source enum identity lacks declaration provenance");
    }
}
