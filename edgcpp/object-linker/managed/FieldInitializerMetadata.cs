namespace DotObj;

internal sealed record FieldInitializerDestruction(long SourceInitializerId, long SourceTypeId,
    long SelectedRoutineEntityId, string CaptureProof);
internal sealed record FieldInitializerExecution(long SourceInitializerId, long FunctionEntityId,
    long ConstructorOrdinal, string ExecutionDisposition, string CaptureProof);

internal sealed partial class ObjectFile
{
    public Dictionary<long, FieldInitializerDestruction> FieldInitializerDestructions { get; } = [];
    public Dictionary<(long SourceInitializerId, long FunctionEntityId), FieldInitializerExecution> FieldInitializerExecutions { get; } = [];

    // The root field DIP has no lifetime before stock DMI expansion. Its
    // template obligation is therefore distinct from nested temporary lists.
    public SourceInitializer VerifyFieldInitializerDestructionUse(long sourceInitializerId, long functionId,
        long callbackId, string operationKind)
    {
        if (!SourceInitializers.TryGetValue(sourceInitializerId, out var source) || source.Kind != "field-default" ||
            source.SourceFieldId is null ||
            !FieldInitializerDestructions.TryGetValue(sourceInitializerId, out var destruction) ||
            !FieldInitializerExecutions.TryGetValue((sourceInitializerId, functionId), out var execution) ||
            execution.ExecutionDisposition != "copied" || OriginalFunction(callbackId) != destruction.SelectedRoutineEntityId ||
            operationKind is not ("cleanup" or "construct" or "destroy" or "destruction" or "normal") ||
            AbiRoutineOrigins.TryGetValue(callbackId, out var origin) && origin.EntryKind != "complete")
            throw new InvalidDataException("Field initializer destruction lacks its original selected callback and observed constructor execution");
        return source;
    }

    public SourceInitializer VerifyFieldInitializerPartialDestructionUse(long sourceStaticObligationId, long functionId,
        long callbackId, string operationKind)
    {
        if (!SourceStaticDestructions.TryGetValue(sourceStaticObligationId, out var partial) || partial.Kind != "partial-aggregate" ||
            partial.SourceInitializerId is not long initializer || !SourceInitializers.TryGetValue(initializer, out var source) ||
            source.Kind != "field-default" || source.SourceFieldId is null ||
            !FieldInitializerExecutions.TryGetValue((initializer, functionId), out var execution) || execution.ExecutionDisposition != "copied" ||
            OriginalFunction(callbackId) != partial.SelectedRoutineEntityId ||
            operationKind is not ("cleanup" or "construct" or "destroy" or "destruction" or "normal") ||
            AbiRoutineOrigins.TryGetValue(callbackId, out var origin) && origin.EntryKind != "complete")
            throw new InvalidDataException("Field partial destruction lacks original template and exact constructor execution");
        return source;
    }

    private void ReadFieldInitializerMetadata()
    {
        using (var q = database.Prepare("SELECT source_initializer_id,source_type_id,selected_routine_entity_id,capture_proof FROM source_field_initializer_destructions"))
            while (q.Read())
            {
                var value = new FieldInitializerDestruction(q.Integer(0), q.Integer(1), q.Integer(2), q.Text(3));
                if (!SourceInitializers.TryGetValue(value.SourceInitializerId, out var initializer) || initializer.Kind != "field-default" ||
                    initializer.SourceTypeId != value.SourceTypeId ||
                    !SourceFunctionKinds.TryGetValue(value.SelectedRoutineEntityId, out var kind) || kind != "destructor" ||
                    !EntitySourceRoutines.TryGetValue(value.SelectedRoutineEntityId, out var routine) || routine.ObjectTypeId is not long receiver ||
                    DestructionElementType(value.SourceTypeId).Kind != "record" ||
                    Original(SourceTypeById(receiver)).ExecutionTypeId != DestructionElementType(value.SourceTypeId).ExecutionTypeId ||
                    value.CaptureProof != "before-lowering" || !FieldInitializerDestructions.TryAdd(value.SourceInitializerId, value))
                    throw new InvalidDataException("Field initializer root destruction differs from its original member type and selected destructor");
            }
        using (var q = database.Prepare("SELECT source_initializer_id,function_entity_id,constructor_ordinal,execution_disposition,capture_proof FROM field_initializer_executions"))
            while (q.Read())
            {
                var value = new FieldInitializerExecution(q.Integer(0), q.Integer(1), q.Integer(2), q.Text(3), q.Text(4));
                long original = OriginalFunction(value.FunctionEntityId);
                if (!SourceInitializers.TryGetValue(value.SourceInitializerId, out var initializer) || initializer.Kind != "field-default" ||
                    initializer.SourceFieldId is not long field || !Functions.ContainsKey(value.FunctionEntityId) ||
                    !SourceFunctionKinds.TryGetValue(original, out var kind) || kind != "constructor" ||
                    !EntitySourceRoutines.TryGetValue(original, out var routine) || routine.ObjectTypeId is not long receiver ||
                    Original(SourceTypeById(receiver)).Id != SourceRecords[SourceFields[field].SourceRecordId].SourceTypeId ||
                    value.ConstructorOrdinal < 0 ||
                    (value.ExecutionDisposition == "copied" ? value.CaptureProof != "observed-field-initializer-copy"
                        : value.ExecutionDisposition != "stock-no-effect" || value.CaptureProof != "observed-no-effect-field-initializer") ||
                    !FieldInitializerExecutions.TryAdd((value.SourceInitializerId, value.FunctionEntityId), value))
                    throw new InvalidDataException("Field initializer execution lacks its exact source member, constructor owner or stock observation");
            }
        foreach (var group in FieldInitializerExecutions.Values.GroupBy(value => value.FunctionEntityId))
            if (group.Select(value => value.ConstructorOrdinal).Distinct().Count() != group.Count())
                throw new InvalidDataException("Field initializer execution constructor ordinals collide");
    }

    private void VerifyFieldInitializerExecutionMetadata()
    {
        var rootElisions = new HashSet<(long Initializer, long Function)>();
        using (var q = database.Prepare("SELECT source_initializer_id,function_entity_id,kind,capture_proof FROM field_initializer_destruction_elisions"))
            while (q.Read())
            {
                long initializer = q.Integer(0), function = q.Integer(1);
                if (!FieldInitializerDestructions.ContainsKey(initializer) ||
                    !FieldInitializerExecutions.TryGetValue((initializer, function), out var execution) || execution.ExecutionDisposition != "copied" ||
                    q.Text(2) != "stock-no-effect" || q.Text(3) != "observed-no-effect-destruction-removal" ||
                    !rootElisions.Add((initializer, function)))
                    throw new InvalidDataException("Field destructor elimination lacks exact source root and constructor execution");
            }
        foreach (var execution in FieldInitializerExecutions.Values.Where(value => FieldInitializerDestructions.ContainsKey(value.SourceInitializerId)))
        {
            long initializer = execution.SourceInitializerId, function = execution.FunctionEntityId;
            using var q = database.Prepare($"""
                SELECT (SELECT count(*) FROM cleanup_actions WHERE source_field_initializer_id={initializer} AND function_entity_id={function}) +
                       (SELECT count(*) FROM normal_destructions d JOIN nodes n ON n.id=d.node_id WHERE d.source_field_initializer_id={initializer} AND n.function_id={function}) +
                       (SELECT count(*) FROM array_runtime_operations a JOIN nodes n ON n.id=a.node_id WHERE a.source_field_initializer_id={initializer} AND n.function_id={function})
                """);
            bool elided = execution.ExecutionDisposition == "stock-no-effect" || rootElisions.Contains((initializer, function));
            if (!q.Read() || elided == (q.Integer(0) > 0))
                throw new InvalidDataException("Original field root destruction lacks exactly owned execution or observed stock elimination");
        }
        var partialExecutions = new HashSet<(long Obligation, long Function)>();
        using (var q = database.Prepare("SELECT source_static_obligation_id,function_entity_id,kind,capture_proof FROM field_initializer_partial_destruction_executions"))
            while (q.Read())
            {
                long obligationId = q.Integer(0), functionId = q.Integer(1); string kind = q.Text(2);
                if (!SourceStaticDestructions.TryGetValue(obligationId, out var obligation))
                    throw new InvalidDataException("Field partial disposition has no original source obligation");
                VerifyFieldInitializerPartialDestructionUse(obligationId, functionId, obligation.SelectedRoutineEntityId, "cleanup");
                using var actions = database.Prepare($"SELECT count(*) FROM cleanup_actions WHERE source_static_obligation_id={obligationId} AND function_entity_id={functionId}");
                bool adopted = SourceDestructionObligations.Values.Any(value => value.SourceStaticObligationId == obligationId &&
                    value.SourceInitializerId == obligation.SourceInitializerId && IsSourceLifetimeExecution(SourceLifetimes[value.LifetimeId], functionId));
                if (kind is not ("cleanup-action" or "stock-terminal-no-throw" or "adopted-automatic") || q.Text(3) != "observed-field-partial-disposition" ||
                    !actions.Read() || (kind == "cleanup-action") != (actions.Integer(0) > 0) || (kind == "adopted-automatic") != adopted ||
                    !partialExecutions.Add((obligationId, functionId)))
                    throw new InvalidDataException("Field partial execution disposition differs from exact owned constructor cleanup");
            }
        foreach (var obligation in SourceStaticDestructions.Values.Where(value => value.Kind == "partial-aggregate" &&
            value.SourceInitializerId is long initializer && SourceInitializers[initializer].Kind == "field-default"))
            foreach (var execution in FieldInitializerExecutions.Values.Where(value => value.SourceInitializerId == obligation.SourceInitializerId && value.ExecutionDisposition == "copied"))
                if (!partialExecutions.Contains((obligation.Id, execution.FunctionEntityId)))
                    throw new InvalidDataException("Original field partial destruction lacks per-constructor execution disposition");
        foreach (var execution in FieldInitializerExecutions.Values.Where(value => value.ExecutionDisposition == "stock-no-effect"))
        {
            using var use = database.Prepare($"SELECT count(*) FROM cleanup_actions WHERE source_field_initializer_id={execution.SourceInitializerId} AND function_entity_id={execution.FunctionEntityId}");
            if (!use.Read() || use.Integer(0) != 0 || InitializerLifetimeExecutions.Values.Any(value =>
                value.SourceInitializerId == execution.SourceInitializerId && value.FunctionEntityId == execution.FunctionEntityId))
                throw new InvalidDataException("Eliminated field initializer still owns executable cleanup or temporary lifetimes");
        }
    }
}
