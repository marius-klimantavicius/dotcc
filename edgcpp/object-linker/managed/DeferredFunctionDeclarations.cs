namespace DotObj;

internal sealed partial class ObjectFile
{
    // These are original external declarations, not callable ABI inventions.
    // Their provisional execution rows only support addresses and relocations.
    public Dictionary<long, long> DeferredFunctionDeclarations { get; } = [];

    private void ReadDeferredFunctionDeclarations()
    {
        using var query = database.Prepare("SELECT entity_id,source_routine_type_id,capture_proof FROM deferred_function_declarations ORDER BY entity_id");
        while (query.Read())
        {
            if (SourceLanguage != "c++" || query.Integer(0) <= 0 || query.Integer(1) <= 0 ||
                query.Text(2) != "observed-incomplete-declaration" ||
                !DeferredFunctionDeclarations.TryAdd(query.Integer(0), query.Integer(1)))
                throw new InvalidDataException("Invalid deferred function declaration observation");
        }
    }

    private bool PermitsDeferredValue(long entity, string type) =>
        DeferredFunctionDeclarations.ContainsKey(entity) && ScalarTypes.IsRecord(type) &&
        RecordsByKey.TryGetValue(type, out RecordType? record) && record.Size == 0 &&
        record.RepresentationKind == "complete";

    private void VerifyDeferredFunctionDeclarations()
    {
        foreach (var (id, sourceId) in DeferredFunctionDeclarations)
        {
            if (!Entities.TryGetValue(id, out Entity? entity) || entity.Linkage != "external" ||
                entity.Definition != "declaration" || Functions.ContainsKey(id) ||
                SourceFunctionKinds.GetValueOrDefault(id) != "ordinary" ||
                !EntitySourceRoutines.TryGetValue(id, out RoutineType? source) || source.TypeId != sourceId ||
                !DeferredRoutineAbis.Any(abi => abi.RoutineTypeId == entity.RoutineTypeId && abi.SourceRoutineTypeId == sourceId) ||
                !(PermitsDeferredValue(id, Type(entity.ReturnTypeId)) || Parameters.GetValueOrDefault(id, []).Any(type => PermitsDeferredValue(id, type))))
                throw new InvalidDataException("Deferred declaration lacks its exact incomplete original routine ABI");
            // Bodies have not been loaded here. Enforce the no-execution
            // contract directly; their usual typed verification follows later.
            using var uses = database.Prepare("SELECT count(*) FROM nodes WHERE entity_id=" +
                id.ToString(System.Globalization.CultureInfo.InvariantCulture) + " AND kind<>'function-address'");
            if (!uses.Read() || uses.Integer(0) != 0)
                throw new InvalidDataException("Deferred function declaration cannot be directly called or registered for cleanup");
        }
    }
}
