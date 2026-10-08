namespace DotObj;

internal sealed record AbiRoutineOrigin(long EntityId, long PrimaryEntityId, string EntryKind, bool IsAliasEntry, string CaptureProof, long? TargetEntityId);

internal sealed partial class ObjectFile
{
    public Dictionary<long, AbiRoutineOrigin> AbiRoutineOrigins { get; } = [];
    public Dictionary<long, string> SourceFunctionKinds { get; } = [];

    private void ReadAbiRoutineOrigins()
    {
        using var query = database.Prepare("SELECT entity_id,primary_entity_id,entry_kind,is_alias_entry,capture_proof,target_entity_id FROM abi_routine_origins ORDER BY entity_id");
        while (query.Read())
        {
            var origin = new AbiRoutineOrigin(query.Integer(0), query.Integer(1), query.Text(2), query.Integer(3) == 1, query.Text(4), query.OptionalInteger(5));
            if (!Entities.ContainsKey(origin.EntityId) || !Entities.ContainsKey(origin.PrimaryEntityId) || origin.EntityId == origin.PrimaryEntityId ||
                origin.EntryKind is not ("complete" or "subobject" or "deleting" or "constructor-default-adapter") || query.Integer(3) is not (0 or 1) ||
                (origin.EntryKind == "constructor-default-adapter"
                    ? origin.TargetEntityId is not long target || !Entities.ContainsKey(target) || origin.IsAliasEntry || origin.CaptureProof != "observed-default-wrapper-creation"
                    : origin.TargetEntityId is not null || origin.CaptureProof != "primary-ctor-dtor-link"))
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
            if (origin.EntryKind == "constructor-default-adapter")
            {
                if (kind != "constructor" || entity.Linkage != "internal" || !Functions.ContainsKey(entity.Id) ||
                    parameters.Count < 1 || parameters.Count >= originalParameters.Count ||
                    !parameters.SequenceEqual(originalParameters.Take(parameters.Count)))
                    throw new InvalidDataException("Default constructor adapter requires an owned private body and exact forwarded parameter prefix");
                long targetId = origin.TargetEntityId!.Value;
                if (targetId != origin.PrimaryEntityId && (!AbiRoutineOrigins.TryGetValue(targetId, out AbiRoutineOrigin? target) ||
                    target.PrimaryEntityId != origin.PrimaryEntityId || target.EntryKind is not ("complete" or "subobject")))
                    throw new InvalidDataException("Default constructor adapter target must be its primary or direct nondeleting entry");
                for (int ordinal = parameters.Count - 1; ordinal < primary.Parameters.Length; ordinal++)
                    if (!SourceParameterDefaults.GetValueOrDefault((origin.PrimaryEntityId, ordinal)))
                        throw new InvalidDataException("Default constructor adapter omitted parameter has no original default argument");
                // The defaults execute in this body before the selected ctor is
                // entered. Its exception boundary need not match that ctor's.
            }
            else if (!parameters.SequenceEqual(originalParameters) || parameters.Count != primary.Parameters.Length + 1 ||
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
