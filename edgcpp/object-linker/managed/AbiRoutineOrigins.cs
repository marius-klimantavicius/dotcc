namespace DotObj;

internal sealed record AbiRoutineOrigin(long EntityId, long PrimaryEntityId, string EntryKind, bool IsAliasEntry, string CaptureProof);

internal sealed partial class ObjectFile
{
    public Dictionary<long, AbiRoutineOrigin> AbiRoutineOrigins { get; } = [];
    public Dictionary<long, string> SourceFunctionKinds { get; } = [];

    private void ReadAbiRoutineOrigins()
    {
        using var query = database.Prepare("SELECT entity_id,primary_entity_id,entry_kind,is_alias_entry,capture_proof FROM abi_routine_origins ORDER BY entity_id");
        while (query.Read())
        {
            var origin = new AbiRoutineOrigin(query.Integer(0), query.Integer(1), query.Text(2), query.Integer(3) == 1, query.Text(4));
            if (!Entities.ContainsKey(origin.EntityId) || !Entities.ContainsKey(origin.PrimaryEntityId) || origin.EntityId == origin.PrimaryEntityId ||
                origin.EntryKind is not ("complete" or "subobject" or "deleting") || query.Integer(3) is not (0 or 1) || origin.CaptureProof != "primary-ctor-dtor-link")
                throw new InvalidDataException("Invalid generated routine origin");
            AbiRoutineOrigins.Add(origin.EntityId, origin);
        }
    }

    private void VerifyAbiRoutineOrigins()
    {
        foreach (AbiRoutineOrigin origin in AbiRoutineOrigins.Values)
        {
            if (EntitySourceRoutines.ContainsKey(origin.EntityId) || !EntitySourceRoutines.TryGetValue(origin.PrimaryEntityId, out RoutineType? primary) ||
                !SourceFunctionKinds.TryGetValue(origin.PrimaryEntityId, out string? kind) || kind == "ordinary" ||
                origin.EntryKind == "deleting" && kind != "destructor" || primary.ObjectTypeId is null || primary.Variadic)
                throw new InvalidDataException("Generated entry requires a direct original constructor/destructor origin");
            Entity entity = Entities[origin.EntityId];
            RoutineType execution = Routines[entity.RoutineTypeId];
            if (Type(entity.ReturnTypeId) != "void" || execution.Variadic || Type(SourceTypeById(primary.ReturnTypeId).ExecutionTypeId) != "void")
                throw new InvalidDataException("Generated constructor/destructor entry requires the supported void-return ABI");
            var parameters = Parameters.GetValueOrDefault(entity.Id, []);
            var originalParameters = Parameters.GetValueOrDefault(origin.PrimaryEntityId, []);
            if (!parameters.SequenceEqual(originalParameters) || parameters.Count != primary.Parameters.Length + 1 ||
                origin.IsAliasEntry && EntityRoutine(entity.Id) != EntityRoutine(origin.PrimaryEntityId))
                throw new InvalidDataException("Generated entry parameter ABI differs from its primary routine");
            for (int ordinal = 0; ordinal < parameters.Count; ordinal++)
            {
                ParameterSemantics role = ParameterRoles[(entity.Id, ordinal)];
                if (ordinal == 0 ? role.Role != "this" || role.SourceOrdinal is not null
                    : role.Role != "forwarded-source" || role.SourceOrdinal != ordinal - 1)
                    throw new InvalidDataException("Generated entry parameter lacks its proven primary source role");
            }
        }
        RejectRows("SELECT d.id FROM source_declarations d JOIN abi_routine_origins a ON a.entity_id=d.entity_id OR a.entity_id=d.function_entity_id", "Generated ABI entry must not fabricate source declarations");
        RejectRows("SELECT b.id FROM bindings b JOIN abi_routine_origins a ON a.entity_id=b.function_id WHERE b.is_synthetic=0", "Generated ABI entry bindings must be synthetic");
    }
}
