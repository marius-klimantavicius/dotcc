namespace DotObj;

internal sealed record SourceRoutineAbi(long SourceRoutineTypeId, string ReturnPassing, string CaptureProof);
internal sealed record SourceRoutineAbiParameter(long SourceRoutineTypeId, long ExecutionOrdinal, string ParameterRole, long? SourceOrdinal);

internal sealed partial class ObjectFile
{
    public Dictionary<long, SourceRoutineAbi> SourceRoutineAbis { get; } = [];
    private readonly Dictionary<long, List<SourceRoutineAbiParameter>> sourceRoutineAbiParameters = [];
    public IReadOnlyList<SourceRoutineAbiParameter> RoutineAbiParameters(long typeId) => sourceRoutineAbiParameters.GetValueOrDefault(typeId, []);

    private void ReadAndVerifySourceRoutineAbis()
    {
        using (var query = database.Prepare("SELECT source_routine_type_id,return_passing,capture_proof FROM source_routine_abis ORDER BY source_routine_type_id"))
            while (query.Read())
            {
                var abi = new SourceRoutineAbi(query.Integer(0), query.Text(1), query.Text(2));
                if (!SourceRoutines.ContainsKey(abi.SourceRoutineTypeId) || abi.ReturnPassing is not ("direct" or "result-buffer") ||
                    abi.CaptureProof is not ("source-c-signature" or "observed-routine-lowering" or "observed-runtime-signature"))
                    throw new InvalidDataException("Invalid original routine ABI header");
                SourceRoutineAbis.Add(abi.SourceRoutineTypeId, abi);
            }
        if (SourceRoutineAbis.Count != SourceRoutines.Count) throw new InvalidDataException("Missing original routine ABI header");
        using (var query = database.Prepare("SELECT source_routine_type_id,execution_ordinal,parameter_role,source_ordinal FROM source_routine_abi_parameters ORDER BY source_routine_type_id,execution_ordinal"))
            while (query.Read())
            {
                var slot = new SourceRoutineAbiParameter(query.Integer(0), query.Integer(1), query.Text(2), query.OptionalInteger(3));
                if (!SourceRoutineAbis.ContainsKey(slot.SourceRoutineTypeId)) throw new InvalidDataException("ABI slot has no original routine header");
                if (!sourceRoutineAbiParameters.TryGetValue(slot.SourceRoutineTypeId, out var list))
                    sourceRoutineAbiParameters.Add(slot.SourceRoutineTypeId, list = []);
                if (slot.ExecutionOrdinal != list.Count || slot.ParameterRole is not ("source" or "indirect-source" or "this" or "result-buffer" or "vtt") ||
                    (slot.ParameterRole is "this" or "result-buffer" or "vtt" ? slot.SourceOrdinal is not null : slot.SourceOrdinal is null or < 0))
                    throw new InvalidDataException("Invalid original routine ABI slot");
                list.Add(slot);
            }
        foreach (RoutineType routine in SourceRoutines.Values)
        {
            SourceRoutineAbi abi = SourceRoutineAbis[routine.TypeId];
            SourceType source = SourceTypeById(routine.TypeId);
            var physical = RoutineTypes.Signature(Type(source.ExecutionTypeId));
            var slots = RoutineAbiParameters(routine.TypeId);
            if (slots.Count != physical.Parameters.Length || physical.Variadic != routine.Variadic || physical.ExceptionPolicy != routine.ExceptionPolicy)
                throw new InvalidDataException("Original routine ABI slot count or calling policy mismatch");
            int sourceOrdinal = 0, receivers = 0, results = 0, vtts = 0;
            bool sawSource = false;
            foreach (SourceRoutineAbiParameter slot in slots)
            {
                if (slot.ParameterRole is "source" or "indirect-source")
                {
                    sawSource = true;
                    if (slot.SourceOrdinal != sourceOrdinal || sourceOrdinal >= routine.Parameters.Length)
                        throw new InvalidDataException("Original routine ABI must map every source parameter once in order");
                    sourceOrdinal++;
                }
                else
                {
                    if (sawSource) throw new InvalidDataException("Implicit ABI slots must precede original explicit parameters");
                    if (slot.ParameterRole == "this") receivers++; else if (slot.ParameterRole == "vtt") vtts++; else results++;
                }
                if (physical.Parameters[slot.ExecutionOrdinal] != RoutineAbiParameterType(routine, slot))
                    throw new InvalidDataException("Original routine ABI slot disagrees with physical parameter type");
            }
            if (sourceOrdinal != routine.Parameters.Length || receivers != (routine.ObjectTypeId is null ? 0 : 1) ||
                vtts > 1 || vtts != 0 && routine.ObjectTypeId is null ||
                results != (abi.ReturnPassing == "result-buffer" ? 1 : 0) || physical.Result != RoutineAbiReturnType(routine))
                throw new InvalidDataException("Original routine ABI receiver/result/source mapping is incomplete");
            if (abi.CaptureProof == "source-c-signature" && SourceLanguage != "c" ||
                abi.CaptureProof is "observed-routine-lowering" or "observed-runtime-signature" && SourceLanguage != "c++" ||
                abi.CaptureProof != "observed-routine-lowering" &&
                (abi.ReturnPassing != "direct" || slots.Any(slot => slot.ParameterRole != "source")))
                throw new InvalidDataException("Original routine ABI capture proof does not establish its transformation");
        }
    }

    private string RoutineAbiReturnType(RoutineType routine) => SourceRoutineAbis[routine.TypeId].ReturnPassing == "result-buffer"
        ? "void" : Type(SourceTypeById(routine.ReturnTypeId).ExecutionTypeId);

    private string RoutineAbiParameterType(RoutineType routine, SourceRoutineAbiParameter slot)
    {
        if (slot.ParameterRole == "vtt")
        {
            string type = RoutineTypes.Signature(Type(SourceTypeById(routine.TypeId).ExecutionTypeId)).Parameters[slot.ExecutionOrdinal];
            if (routine.ObjectTypeId is null || !ScalarTypes.IsPointer(type))
                throw new InvalidDataException("VTT ABI slot requires an observed member-routine pointer");
            return type;
        }
        if (slot.ParameterRole == "this")
        {
            if (routine.ObjectTypeId is not long id || SourceTypeById(id).Kind != "record")
                throw new InvalidDataException("ABI receiver requires its original unqualified record type");
            return "ptr(" + Type(SourceTypeById(id).ExecutionTypeId) + ")";
        }
        SourceType original = SourceTypeById(slot.ParameterRole == "result-buffer" ? routine.ReturnTypeId :
            slot.SourceOrdinal is long ordinal && ordinal < routine.Parameters.Length ? routine.Parameters[ordinal] :
            throw new InvalidDataException("ABI parameter source ordinal is out of bounds"));
        if (slot.ParameterRole == "source") return ParameterType(original);
        SourceType record = Original(original);
        if (record.Kind != "record" || !Records.TryGetValue(record.ExecutionTypeId, out RecordType? layout) ||
            layout.RepresentationKind != "complete" || layout.Size <= 0)
            throw new InvalidDataException("Indirect value ABI requires an original complete record value");
        return "ptr(" + Type(record.ExecutionTypeId) + ")";
    }

    private SourceRoutineAbiParameter SourceParameterSlot(RoutineType routine, long sourceOrdinal) =>
        RoutineAbiParameters(routine.TypeId).Single(slot => slot.SourceOrdinal == sourceOrdinal);

    private string ParameterBindingType(long entity, long executionOrdinal, SourceType declared)
    {
        ParameterSemantics role = ParameterRoles[(entity, executionOrdinal)];
        if (role.Role == "source") return ParameterType(declared);
        if (role.Role == "indirect-source") return "ptr(" + Type(declared.ExecutionTypeId) + ")";
        throw new InvalidDataException("Synthetic ABI parameter cannot have an original source declaration");
    }
}
