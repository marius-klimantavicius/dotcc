namespace DotObj;

internal sealed record DestructionElision(long SourceObligationId, long FunctionEntityId, string Kind, string CaptureProof);

internal sealed partial class ObjectFile
{
    public Dictionary<(long SourceObligationId, long FunctionEntityId), DestructionElision> DestructionElisions { get; } = [];

    private void ReadDestructionElisions()
    {
        using var query = database.Prepare("SELECT source_obligation_id,function_entity_id,kind,capture_proof FROM destruction_elisions ORDER BY source_obligation_id,function_entity_id");
        while (query.Read())
        {
            var value = new DestructionElision(query.Integer(0), query.Integer(1), query.Text(2), query.Text(3));
            bool terminal = value.Kind == "terminal-no-throw" && value.CaptureProof == "observed-terminal-partial-elision";
            bool noEffect = value.Kind == "stock-no-effect" && value.CaptureProof == "observed-no-effect-destruction-removal";
            if ((!terminal && !noEffect) ||
                !SourceDestructionObligations.TryGetValue(value.SourceObligationId, out var original) || (terminal && !original.IsPartialAggregate) ||
                !Functions.ContainsKey(value.FunctionEntityId) ||
                !IsSourceLifetimeExecution(SourceLifetimes[original.LifetimeId], value.FunctionEntityId) ||
                !DestructionElisions.TryAdd((value.SourceObligationId, value.FunctionEntityId), value))
                throw new InvalidDataException("Destruction elision lacks its exact original obligation, observed disposition and execution owner");
            if (CleanupActions.Values.Any(action => action.SourceObligationId == value.SourceObligationId && action.FunctionEntityId == value.FunctionEntityId))
                throw new InvalidDataException("Terminal partial elision also has an active cleanup action");
        }
        RejectRows("""
            SELECT e.source_obligation_id FROM destruction_elisions e
            JOIN normal_destructions d ON d.source_obligation_id=e.source_obligation_id
            JOIN nodes n ON n.id=d.node_id WHERE n.function_id=e.function_entity_id
            UNION ALL
            SELECT e.source_obligation_id FROM destruction_elisions e
            JOIN array_runtime_operations d ON d.source_obligation_id=e.source_obligation_id
            JOIN nodes n ON n.id=d.node_id WHERE n.function_id=e.function_entity_id
                AND (e.kind<>'terminal-no-throw' OR d.operation_kind='destroy')
            UNION ALL
            SELECT e.source_obligation_id FROM destruction_elisions e
            JOIN throw_payload_subobjects d ON d.source_obligation_id=e.source_obligation_id
            JOIN nodes n ON n.id=d.throw_node_id WHERE n.function_id=e.function_entity_id
            """, "Terminal partial elision also has an owned destruction operation in the same execution");
    }
}
