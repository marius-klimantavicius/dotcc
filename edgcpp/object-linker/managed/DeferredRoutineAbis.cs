namespace DotObj;

internal sealed record DeferredRoutineAbi(long RoutineTypeId, long SourceRoutineTypeId);

internal sealed partial class ObjectFile
{
    public List<DeferredRoutineAbi> DeferredRoutineAbis { get; } = [];
    public HashSet<string> DeferredRoutineKeys { get; } = new(StringComparer.Ordinal);

    public bool IsIncompleteOriginalValue(long id)
    {
        SourceType original = Original(SourceTypeById(id));
        return original.Kind == "record" && !SourceRecordsByType[original.Id].IsComplete;
    }

    private void ReadAndVerifyDeferredRoutineAbis()
    {
        var seen = new HashSet<(long, long)>();
        using var query = database.Prepare("SELECT routine_type_id,source_routine_type_id,reason,capture_proof FROM deferred_routine_abis ORDER BY routine_type_id,source_routine_type_id");
        while (query.Read())
        {
            long execution = query.Integer(0), source = query.Integer(1);
            if (SourceLanguage != "c++" || !seen.Add((execution, source)) || !Routines.ContainsKey(execution) ||
                !SourceRoutines.TryGetValue(source, out RoutineType? original) ||
                SourceTypeById(source).ExecutionRepresentationId != execution ||
                !SourceRoutineAbis.TryGetValue(source, out SourceRoutineAbi? observed) || observed.CaptureProof != "observed-routine-lowering" ||
                query.Text(2) != "incomplete-value-type" || query.Text(3) != "observed-incomplete-routine-lowering" ||
                !(IsIncompleteOriginalValue(original.ReturnTypeId) || original.Parameters.Any(IsIncompleteOriginalValue)) ||
                Entities.Values.Any(entity => entity.RoutineTypeId == execution &&
                    !DeferredFunctionDeclarations.ContainsKey(entity.Id)))
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
        var observations = new Dictionary<string, List<(ObjectFile Object, SourceRoutineAbi Abi)>>(StringComparer.Ordinal);
        var requiredSourceSignatures = Objects.SelectMany(obj => obj.DeferredRoutineAbis
            .Select(abi => obj.SourceTypeSignature(abi.SourceRoutineTypeId))).ToHashSet(StringComparer.Ordinal);
        foreach (ObjectFile obj in Objects)
        {
            var deferredSources = obj.DeferredRoutineAbis.Select(abi => abi.SourceRoutineTypeId).ToHashSet();
            foreach (SourceRoutineAbi abi in obj.SourceRoutineAbis.Values)
            {
                SourceType source = obj.SourceTypes[abi.SourceRoutineTypeId];
                if (source.ExecutionRepresentationId is not long execution) continue;
                string key = obj.SourceTypeSignature(source.Id);
                if (!requiredSourceSignatures.Contains(key)) continue;
                if (!observations.TryGetValue(key, out var facts)) observations.Add(key, facts = []);
                facts.Add((obj, abi));
                if (deferredSources.Contains(abi.SourceRoutineTypeId)) continue;
                if (!complete.TryGetValue(key, out var candidates)) complete.Add(key, candidates = []);
                candidates.Add(obj.Type(execution));
            }
        }
        var deferred = Objects.SelectMany(obj => obj.DeferredRoutineAbis.Select(abi =>
            (Object: obj, Abi: abi, Raw: obj.Type(abi.RoutineTypeId), Source: obj.SourceTypeSignature(abi.SourceRoutineTypeId)))).ToArray();
        if (deferred.GroupBy(item => item.Raw).Any(group =>
            group.Select(item => complete.ContainsKey(item.Source)).Distinct().Skip(1).Any()))
            throw new InvalidDataException("Ambiguous deferred routine key has complete and incomplete original semantic families");
        var completedKeys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var family in deferred.GroupBy(item => item.Source, StringComparer.Ordinal))
        {
            VerifyKnownRoutineDecisions(observations[family.Key]);
            bool resolved = complete.TryGetValue(family.Key, out var candidates);
            // An unresolved choice is only an internal representative for
            // opaque pointer storage. Every observed ABI row stays intact,
            // and this routine remains forbidden at every call boundary.
            string selected = resolved ? candidates![0] : family.Select(item => item.Raw).Order(StringComparer.Ordinal).First();
            foreach (var item in family)
            {
                // Distinct original reference/pointer signatures can share
                // a raw execution key. All families must propose the exact
                // same physical target; source identity is never aliased.
                if (deferredRoutineAliases.TryGetValue(item.Raw, out string? previous) && previous != selected)
                    throw new InvalidDataException("Deferred routine has conflicting ABI resolutions");
                deferredRoutineAliases[item.Raw] = selected;
                if (resolved) completedKeys.Add(item.Raw); else unresolvedDeferredRoutines.Add(item.Raw);
            }
        }
        // A shared raw key may resolve only consistently in every semantic
        // family. Original source field/function identity is still compared.
        unresolvedDeferredRoutines.ExceptWith(completedKeys);
        foreach (var candidates in complete.Values)
            if (candidates.Select(CanonicalExecutionType).Distinct(StringComparer.Ordinal).Skip(1).Any())
                throw new InvalidDataException("Original routine signature has competing completed ABIs");
        // Resolving a nested callback parameter can change the canonical key
        // of an outer signature whose own result is still incomplete.
        unresolvedDeferredRoutines.UnionWith(unresolvedDeferredRoutines.Select(CanonicalExecutionType).ToArray());
        if (completedKeys.Any(raw => unresolvedDeferredRoutines.Contains(CanonicalExecutionType(raw))))
            throw new InvalidDataException("Completed routine ABI aliases an unresolved original semantic family");
    }

    private static void VerifyKnownRoutineDecisions(IEnumerable<(ObjectFile Object, SourceRoutineAbi Abi)> observations)
    {
        var decisions = new Dictionary<string, string>(StringComparer.Ordinal);
        void Observe(string slot, string decision)
        {
            if (decisions.TryGetValue(slot, out string? previous) && previous != decision)
                throw new InvalidDataException("Original routine signature has conflicting known partial ABI decisions");
            decisions[slot] = decision;
        }
        foreach (var (obj, abi) in observations)
            {
                RoutineType source = obj.SourceRoutines[abi.SourceRoutineTypeId];
                if (!obj.IsIncompleteOriginalValue(source.ReturnTypeId)) Observe("result", abi.ReturnPassing);
                foreach (SourceRoutineAbiParameter slot in obj.RoutineAbiParameters(source.TypeId))
                {
                    if (slot.SourceOrdinal is long ordinal)
                    {
                        if (!obj.IsIncompleteOriginalValue(source.Parameters[ordinal]))
                            Observe("parameter:" + ordinal, slot.ParameterRole);
                    }
                }
                Observe("implicit", string.Join(',', obj.RoutineAbiParameters(source.TypeId)
                    .Where(slot => slot.SourceOrdinal is null && slot.ParameterRole != "result-buffer").Select(slot => slot.ParameterRole)));
            }
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
