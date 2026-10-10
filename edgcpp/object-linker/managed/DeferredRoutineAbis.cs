namespace DotObj;

internal sealed record DeferredRoutineAbi(long RoutineTypeId, long SourceRoutineTypeId);

internal sealed partial class ObjectFile
{
    public List<DeferredRoutineAbi> DeferredRoutineAbis { get; } = [];
    public HashSet<string> DeferredRoutineKeys { get; } = new(StringComparer.Ordinal);

    private void ReadAndVerifyDeferredRoutineAbis()
    {
        var seen = new HashSet<(long, long)>();
        using var query = database.Prepare("SELECT routine_type_id,source_routine_type_id,reason,capture_proof FROM deferred_routine_abis ORDER BY routine_type_id,source_routine_type_id");
        while (query.Read())
        {
            long execution = query.Integer(0), source = query.Integer(1);
            bool IncompleteValue(long id)
            {
                SourceType original = Original(SourceTypeById(id));
                return original.Kind == "record" && !SourceRecordsByType[original.Id].IsComplete;
            }
            if (SourceLanguage != "c++" || !seen.Add((execution, source)) || !Routines.ContainsKey(execution) ||
                !SourceRoutines.TryGetValue(source, out RoutineType? original) ||
                SourceTypeById(source).ExecutionRepresentationId != execution ||
                !SourceRoutineAbis.TryGetValue(source, out SourceRoutineAbi? observed) || observed.CaptureProof != "observed-routine-lowering" ||
                query.Text(2) != "incomplete-value-type" || query.Text(3) != "observed-incomplete-routine-lowering" ||
                !(IncompleteValue(original.ReturnTypeId) || original.Parameters.Any(IncompleteValue)) ||
                Entities.Values.Any(entity => entity.RoutineTypeId == execution))
                throw new InvalidDataException("Invalid deferred original routine ABI");
            DeferredRoutineAbis.Add(new(execution, source));
            DeferredRoutineKeys.Add(Type(execution));
        }
    }
}

internal sealed partial class Linker
{
    private readonly Dictionary<string, string> deferredRoutineAliases = new(StringComparer.Ordinal);
    private readonly HashSet<string> unresolvedDeferredRoutines = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> normalizedTypes = new(StringComparer.Ordinal);

    private void ResolveDeferredRoutineAbis()
    {
        var complete = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        var requiredSourceSignatures = Objects.SelectMany(obj => obj.DeferredRoutineAbis
            .Select(abi => obj.SourceTypeSignature(abi.SourceRoutineTypeId))).ToHashSet(StringComparer.Ordinal);
        foreach (ObjectFile obj in Objects)
        {
            var deferredSources = obj.DeferredRoutineAbis.Select(abi => abi.SourceRoutineTypeId).ToHashSet();
            foreach (SourceRoutineAbi abi in obj.SourceRoutineAbis.Values)
            {
                if (deferredSources.Contains(abi.SourceRoutineTypeId)) continue;
                SourceType source = obj.SourceTypes[abi.SourceRoutineTypeId];
                if (source.ExecutionRepresentationId is not long execution) continue;
                string key = obj.SourceTypeSignature(source.Id);
                if (!requiredSourceSignatures.Contains(key)) continue;
                if (!complete.TryGetValue(key, out var candidates)) complete.Add(key, candidates = []);
                candidates.Add(obj.Type(execution));
            }
        }
        foreach (ObjectFile obj in Objects)
            foreach (DeferredRoutineAbi abi in obj.DeferredRoutineAbis)
            {
                string raw = obj.Type(abi.RoutineTypeId), source = obj.SourceTypeSignature(abi.SourceRoutineTypeId);
                if (!complete.TryGetValue(source, out var candidates)) { unresolvedDeferredRoutines.Add(raw); continue; }
                string selected = candidates[0];
                if (deferredRoutineAliases.TryGetValue(raw, out string? previous) && previous != selected)
                    throw new InvalidDataException("Deferred routine has conflicting completed ABI resolutions");
                deferredRoutineAliases[raw] = selected;
            }
        // An execution key can be interned for several equivalent source
        // declarations. A completed ABI resolves every such storage view.
        unresolvedDeferredRoutines.ExceptWith(deferredRoutineAliases.Keys);
        foreach (var candidates in complete.Values)
            if (candidates.Select(CanonicalExecutionType).Distinct(StringComparer.Ordinal).Skip(1).Any())
                throw new InvalidDataException("Original routine signature has competing completed ABIs");
        // Resolving a nested callback parameter can change the canonical key
        // of an outer signature whose own result is still incomplete.
        unresolvedDeferredRoutines.UnionWith(unresolvedDeferredRoutines.Select(CanonicalExecutionType).ToArray());
    }

    public bool IsUnresolvedRoutine(string type) => unresolvedDeferredRoutines.Contains(type);

    public string CanonicalExecutionType(string type) => CanonicalExecutionType(type, new HashSet<string>(StringComparer.Ordinal), 0);

    private string CanonicalExecutionType(string type, HashSet<string> active, int depth)
    {
        if (normalizedTypes.TryGetValue(type, out string? cached)) return cached;
        if (depth > 128 || !active.Add(type)) throw new InvalidDataException("Cyclic deferred routine ABI resolution");
        string Resolve(string nested) => CanonicalExecutionType(nested, active, depth + 1);
        string result;
        if (deferredRoutineAliases.TryGetValue(type, out string? target) && target != type) result = Resolve(target);
        else if (ScalarTypes.IsPointer(type)) result = "ptr(" + Resolve(ScalarTypes.Pointee(type)) + ")";
        else if (ScalarTypes.IsArray(type)) result = type[..(type.IndexOf(',') + 1)] + Resolve(ScalarTypes.Element(type)) + ")";
        else if (RoutineTypes.IsRoutine(type))
        {
            var routine = RoutineTypes.Signature(type);
            result = RoutineTypes.Key(Resolve(routine.Result), routine.Parameters.Select(Resolve), routine.Variadic, routine.ExceptionPolicy);
        }
        else result = type;
        active.Remove(type);
        normalizedTypes[type] = result;
        return result;
    }
}
