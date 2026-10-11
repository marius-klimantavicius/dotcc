namespace DotObj;

internal sealed record DependentSourceType(long TypeId, string Kind, long? OwnerTypeId, long? OriginalNestedTypeId,
    long? NestingDepth, long? ParameterPosition, bool IsPack, string IdentityKey);

internal sealed partial class ObjectFile
{
    public Dictionary<long, DependentSourceType> SourceDependentTypes { get; } = [];

    private void ReadDependentSourceTypes()
    {
        ReadSourceNestedTypeDeclarations();
        using var query = database.Prepare("SELECT type_id,kind,owner_type_id,original_nested_type_id,nesting_depth,parameter_position,is_pack,identity_key,capture_proof FROM source_dependent_types ORDER BY type_id");
        while (query.Read())
        {
            var value = new DependentSourceType(query.Integer(0), query.Text(1), query.OptionalInteger(2),
                query.OptionalInteger(3), query.OptionalInteger(4), query.OptionalInteger(5), query.Integer(6) == 1, query.Text(7));
            if (!SourceTypes.TryGetValue(value.TypeId, out var type) || type.Kind != "dependent" ||
                query.Integer(6) is not (0 or 1) || query.Text(8) != "before-lowering" ||
                !SourceDependentTypes.TryAdd(value.TypeId, value))
                throw new InvalidDataException("Dependent type lacks its original source type and capture");
            if (value.OriginalNestedTypeId is not null && value.Kind != "member")
                throw new InvalidDataException("Only an original dependent member may retain a nested declaration");
            if (value.Kind == "parameter")
            {
                if (value.OwnerTypeId is not null || value.NestingDepth is not >= 0 || value.ParameterPosition is not > 0 ||
                    type.SourceFileId is not long fileId || type.Line <= 0)
                    throw new InvalidDataException("Template type parameter requires its original declaration and coordinates");

            }
            else if (value.Kind == "auto-placeholder")
            {
                if(value.OwnerTypeId is not null || value.NestingDepth!=-1 || value.ParameterPosition is not(1 or 2) ||
                   value.IsPack || value.IdentityKey.Length!=0 || type.SourceName.Length!=0)
                    throw new InvalidDataException("Auto placeholder differs from its original type-specifier coordinates");
            }
            else if (value.Kind == "member")
            {
                if (value.OwnerTypeId is not long owner || !SourceTypes.ContainsKey(owner) || type.SourceName.Length == 0 ||
                    value.NestingDepth is not null || value.ParameterPosition is not null || value.IsPack || value.IdentityKey.Length != 0)
                    throw new InvalidDataException("Dependent member requires its exact source owner and member name");
            }
            else if (value.Kind != "unknown" || value.OwnerTypeId is not null || value.NestingDepth is not null ||
                value.ParameterPosition is not null || value.IsPack || value.IdentityKey.Length != 0 || type.SourceName.Length != 0)
                throw new InvalidDataException("Invalid unknown dependent source type");
        }
        foreach (var value in SourceDependentTypes.Values)
        {
            var seen = new HashSet<long>();
            SourceType type = SourceTypeById(value.TypeId);
            while (true)
            {
                if (!seen.Add(type.Id)) throw new InvalidDataException("Cycle in dependent source member owners");
                if (type.Kind is "typedef" or "qualified") { type = SourceTypeById(type.UnderlyingTypeId!.Value); continue; }
                if (type.Kind != "dependent")
                {
                    if (type.Kind != "record") throw new InvalidDataException("Dependent member owner is not a class or dependent type");
                    break;
                }
                if (!SourceDependentTypes.TryGetValue(type.Id, out var owner))
                    throw new InvalidDataException("Dependent source type has no semantic description");
                if (owner.Kind != "member") break;
                type = SourceTypeById(owner.OwnerTypeId!.Value);
            }
        }
    }

    private string DependentSourceSignature(SourceType type, int depth)
    {
        var value = SourceDependentTypes[type.Id];
        static string Encode(string text) => Convert.ToHexString(System.Text.Encoding.UTF8.GetBytes(text));
        return value.Kind switch
        {
            "parameter" => "dependent-parameter(" + Encode(value.IdentityKey) + ";" + value.IsPack + ")",
            "auto-placeholder" => SourceArgumentTypeIdentity(type.Id,depth+1),
            "member" => "dependent-member(" + SourceTypeSignature(value.OwnerTypeId!.Value, depth + 1, false, false) + ";" + Encode(type.SourceName) + ")",
            _ => "dependent-unknown"
        };
    }
}
