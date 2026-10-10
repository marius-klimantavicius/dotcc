namespace DotObj;

internal sealed record LambdaEntryOrigin(long EntityId, long TargetEntityId, long ClosureSourceTypeId,
    long EntrySourceRoutineTypeId, string CaptureProof);
internal sealed record LambdaEntryCall(long NodeId, long EntryEntityId, string CaptureProof);

internal sealed partial class ObjectFile
{
    public Dictionary<long, LambdaEntryOrigin> LambdaEntryOrigins { get; } = [];
    public Dictionary<long, LambdaEntryCall> LambdaEntryCalls { get; } = [];

    private void ReadLambdaEntryOrigins()
    {
        using (var query = database.Prepare("SELECT entity_id,target_entity_id,closure_source_type_id,entry_source_routine_type_id,capture_proof FROM lambda_entry_origins ORDER BY entity_id"))
            while (query.Read())
            {
                var entry = new LambdaEntryOrigin(query.Integer(0), query.Integer(1), query.Integer(2), query.Integer(3), query.Text(4));
                if (!Entities.ContainsKey(entry.EntityId) || !Entities.ContainsKey(entry.TargetEntityId) || entry.EntityId == entry.TargetEntityId ||
                    entry.ClosureSourceTypeId <= 0 || entry.EntrySourceRoutineTypeId <= 0 || entry.CaptureProof != "observed-lambda-static-entry")
                    throw new InvalidDataException("Invalid observed lambda static entry origin");
                LambdaEntryOrigins.Add(entry.EntityId, entry);
            }
        using var calls = database.Prepare("SELECT node_id,entry_entity_id,capture_proof FROM lambda_entry_calls ORDER BY node_id");
        while (calls.Read())
        {
            var call = new LambdaEntryCall(calls.Integer(0), calls.Integer(1), calls.Text(2));
            if (!LambdaEntryOrigins.ContainsKey(call.EntryEntityId) || call.CaptureProof != "observed-lambda-entry-forwarding")
                throw new InvalidDataException("Invalid observed lambda static entry forwarding");
            LambdaEntryCalls.Add(call.NodeId, call);
        }
    }

    private void VerifyLambdaEntryOrigins()
    {
        foreach (LambdaEntryOrigin entry in LambdaEntryOrigins.Values)
        {
            if (!EntitySourceRoutines.TryGetValue(entry.TargetEntityId, out RoutineType? target) ||
                !SourceRoutines.TryGetValue(entry.EntrySourceRoutineTypeId, out RoutineType? original) ||
                target.ObjectTypeId != entry.ClosureSourceTypeId || SourceTypeById(entry.ClosureSourceTypeId).Kind != "record" ||
                original.ObjectTypeId is not null || original.Variadic || target.Variadic ||
                original.ExceptionPolicy != target.ExceptionPolicy ||
                SourceTypeSignature(original.ReturnTypeId) != SourceTypeSignature(target.ReturnTypeId) ||
                !original.Parameters.Select(parameter => SourceParameterSignature(parameter, 0)).SequenceEqual(
                    target.Parameters.Select(parameter => SourceParameterSignature(parameter, 0))) ||
                SourceRoutineAbis[original.TypeId].ReturnPassing != SourceRoutineAbis[target.TypeId].ReturnPassing)
                throw new InvalidDataException("Lambda static entry must retain its selected original closure/call signature");
            Entity entity = Entities[entry.EntityId], selected = Entities[entry.TargetEntityId];
            if (Type(SourceTypeById(original.TypeId).ExecutionTypeId) != EntityRoutine(entity.Id) ||
                Type(entity.ReturnTypeId) != Type(selected.ReturnTypeId))
                throw new InvalidDataException("Lambda static entry differs from its original execution ABI");
            var originalSlots = RoutineAbiParameters(original.TypeId);
            var selectedParameters = Parameters.GetValueOrDefault(selected.Id, []);
            var selectedSlots = Enumerable.Range(0, selectedParameters.Count).Where(index => ParameterRoles[(selected.Id, index)].Role != "this").ToArray();
            var parameters = Parameters.GetValueOrDefault(entity.Id, []);
            if (selectedSlots.Length + 1 != selectedParameters.Count || parameters.Count != originalSlots.Count || parameters.Count != selectedSlots.Length)
                throw new InvalidDataException("Lambda static entry requires exactly its target receiver omission");
            for (int index = 0; index < parameters.Count; index++)
            {
                ParameterSemantics role = ParameterRoles[(entity.Id, index)];
                ParameterSemantics selectedRole = ParameterRoles[(selected.Id, selectedSlots[index])];
                SourceRoutineAbiParameter sourceSlot = originalSlots[index];
                string expectedRole = sourceSlot.ParameterRole is "source" or "indirect-source" ? "forwarded-source" : sourceSlot.ParameterRole;
                if (sourceSlot.ParameterRole is "this" or "vtt" || role.Role != expectedRole || role.SourceOrdinal != sourceSlot.SourceOrdinal ||
                    sourceSlot.ParameterRole != selectedRole.Role || sourceSlot.SourceOrdinal != selectedRole.SourceOrdinal ||
                    parameters[index] != selectedParameters[selectedSlots[index]])
                    throw new InvalidDataException("Lambda static entry formal lacks exact original forwarded ABI role");
            }
            int calls = LambdaEntryCalls.Values.Count(call => call.EntryEntityId == entity.Id);
            if (Functions.ContainsKey(entity.Id) ? calls != 1 : calls != 0)
                throw new InvalidDataException("Lambda static entry forwarding count differs from its body status");
        }
        RejectRows("SELECT d.id FROM source_declarations d JOIN lambda_entry_origins e ON e.entity_id=d.entity_id OR e.entity_id=d.function_entity_id", "Lambda static entry must not fabricate ordinary source declarations");
    }

    public void VerifyLambdaEntryBody(Entity entity, FunctionBody body)
    {
        if (!LambdaEntryOrigins.TryGetValue(entity.Id, out LambdaEntryOrigin? entry)) return;
        if (body.Bindings.Values.Any(binding => !binding.IsSynthetic))
            throw new InvalidDataException("Lambda static entry requires owned generated bindings");
        LambdaEntryCall forwarding = LambdaEntryCalls.Values.Single(call => call.EntryEntityId == entity.Id);
        Entity target = Entities[entry.TargetEntityId];
        if (!body.Nodes.TryGetValue(forwarding.NodeId, out Node? call) || call.Kind != "call" ||
            call.FunctionId != entity.Id || call.EntityId != target.Id || call.TypeId != target.ReturnTypeId)
            throw new InvalidDataException("Lambda static entry forwarding lost its exact call target/result");
        var arguments = body.Children(call.Id, "argument").ToArray();
        var formals = body.Bindings.Values.Where(binding => binding.Kind == "parameter").OrderBy(binding => binding.Ordinal).ToArray();
        var targetParameters = Parameters.GetValueOrDefault(target.Id, []);
        if (arguments.Length != targetParameters.Count || formals.Length + 1 != arguments.Length)
            throw new InvalidDataException("Lambda static entry forwarding argument count differs from selected ABI");
        int formalIndex = 0, receivers = 0;
        for (int index = 0; index < arguments.Length; index++)
        {
            Node argument = body.Nodes[arguments[index].ChildId];
            if (arguments[index].Ordinal != index) throw new InvalidDataException("Lambda forwarding argument order is not dense");
            if (ParameterRoles[(target.Id, index)].Role == "this")
            {
                receivers++;
                if (argument.Kind != "constant" || argument.ConstantText != "0" || Type(argument.TypeId!.Value) != targetParameters[index])
                    throw new InvalidDataException("Lambda static entry must pass its exact null closure receiver");
                continue;
            }
            Binding formal = formals[formalIndex++];
            if (argument.Kind != "binding" || argument.BindingId != formal.Id || formal.Ordinal != formalIndex - 1 || argument.TypeId != formal.TypeId ||
                body.Nodes.Values.Count(node => node.Kind == "binding" && node.BindingId == formal.Id) != 1)
                throw new InvalidDataException("Lambda static entry must forward each unchanged formal once in order");
        }
        if (receivers != 1) throw new InvalidDataException("Lambda static entry must insert exactly one closure receiver");
    }
}
