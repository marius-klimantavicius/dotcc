namespace DotObj;

internal sealed partial class ObjectFile
{
    // Source-only observations are typed object-local locators. Their retained
    // graphs are neither expanded into recipes nor compared across units.
    private string SourceObservationIdentity(string table, long id) =>
        "source-observation(" + SourceIdentityFrame(UnitId) + SourceIdentityFrame(table) + id + ")";

    internal static bool IsLocalSourceObservation(string? signature) =>
        signature?.Contains("source-observation(", StringComparison.Ordinal) == true;

    internal static bool CompatibleSourceSignatures(string? left, string? right) =>
        left == right || IsLocalSourceObservation(left) || IsLocalSourceObservation(right);

    internal string? RecordedFunctionAbiSignature(long entity)
    {
        string entry = "ordinary";
        if (AbiRoutineOrigins.TryGetValue(entity,out var origin)) { entity=origin.PrimaryEntityId; entry=origin.EntryKind; }
        if (!EntitySourceRoutines.TryGetValue(entity,out var routine) || !SourceRoutineAbis.TryGetValue(routine.TypeId,out var abi)) return null;
        return entry + ":" + abi.ReturnPassing + ":" + string.Join(',',RoutineAbiParameters(routine.TypeId).Select(slot =>
            slot.ParameterRole + (slot.SourceOrdinal is long ordinal ? "#" + ordinal : "")));
    }

    internal bool IsLocalSourceRecord(long id)
    {
        var record = SourceRecords[id];
        return record.IsDependent || record.IsPrototypeInstantiation ||
            record.IdentityKey.StartsWith("edg-source-node:", StringComparison.Ordinal);
    }
}
