namespace DotObj;

internal sealed record ThunkWrapper(long EntityId, long TargetEntityId, long OverriddenEntityId, long PrimaryEntityId,
    long ThisDelta, long VcallIndex, long ReturnDelta, long VbaseIndex, string CaptureProof);
internal sealed record WrapperCall(long NodeId, long WrapperEntityId, string CaptureProof);

internal sealed partial class ObjectFile
{
    public Dictionary<long, ThunkWrapper> ThunkWrappers { get; } = [];
    public Dictionary<long, WrapperCall> WrapperCalls { get; } = [];

    private void ReadThunkWrappers()
    {
        using (var query = database.Prepare("SELECT entity_id,target_entity_id,overridden_entity_id,primary_entity_id,this_delta,vcall_index,return_delta,vbase_index,capture_proof FROM thunk_wrappers ORDER BY entity_id"))
            while (query.Read())
            {
                var wrapper = new ThunkWrapper(query.Integer(0), query.Integer(1), query.Integer(2), query.Integer(3),
                    query.Integer(4), query.Integer(5), query.Integer(6), query.Integer(7), query.Text(8));
                if (!Entities.ContainsKey(wrapper.EntityId) || !Entities.ContainsKey(wrapper.TargetEntityId) ||
                    !Entities.ContainsKey(wrapper.OverriddenEntityId) || !Entities.ContainsKey(wrapper.PrimaryEntityId) ||
                    wrapper.EntityId == wrapper.TargetEntityId || wrapper.EntityId == wrapper.OverriddenEntityId ||
                    wrapper.EntityId == wrapper.PrimaryEntityId || wrapper.VcallIndex > 0 || wrapper.VbaseIndex > 0 ||
                    wrapper.CaptureProof != "observed-wrapper-creation")
                    throw new InvalidDataException("Invalid observed virtual wrapper origin");
                ThunkWrappers.Add(wrapper.EntityId, wrapper);
            }
        using (var query = database.Prepare("SELECT node_id,wrapper_entity_id,capture_proof FROM wrapper_calls ORDER BY node_id"))
            while (query.Read())
            {
                var call = new WrapperCall(query.Integer(0), query.Integer(1), query.Text(2));
                if (!ThunkWrappers.ContainsKey(call.WrapperEntityId) || call.CaptureProof != "observed-wrapper-forwarding")
                    throw new InvalidDataException("Invalid observed virtual wrapper forwarding");
                WrapperCalls.Add(call.NodeId, call);
            }
    }

    private void VerifyThunkWrappers()
    {
        foreach (ThunkWrapper wrapper in ThunkWrappers.Values)
        {
            if (EntitySourceRoutines.ContainsKey(wrapper.EntityId) || AbiRoutineOrigins.ContainsKey(wrapper.EntityId) ||
                GeneratedRoutineOrigins.ContainsKey(wrapper.EntityId) || ThunkWrappers.ContainsKey(wrapper.TargetEntityId) ||
                ThunkWrappers.ContainsKey(wrapper.OverriddenEntityId) ||
                !EntitySourceRoutines.TryGetValue(wrapper.PrimaryEntityId, out RoutineType? original) || original.ObjectTypeId is null ||
                original.Variadic)
                throw new InvalidDataException("Virtual wrapper requires an original nonvariadic member origin");
            long targetPrimary = AbiRoutineOrigins.TryGetValue(wrapper.TargetEntityId, out AbiRoutineOrigin? targetOrigin)
                ? targetOrigin.PrimaryEntityId : wrapper.TargetEntityId;
            long overriddenPrimary = AbiRoutineOrigins.TryGetValue(wrapper.OverriddenEntityId, out AbiRoutineOrigin? overriddenOrigin)
                ? overriddenOrigin.PrimaryEntityId : wrapper.OverriddenEntityId;
            if (targetPrimary != wrapper.PrimaryEntityId ||
                !EntitySourceRoutines.TryGetValue(overriddenPrimary, out RoutineType? overriddenOriginal) ||
                overriddenOriginal.ObjectTypeId is null || overriddenOriginal.Variadic ||
                targetOrigin is not null && targetOrigin.EntryKind is not ("complete" or "deleting"))
                throw new InvalidDataException("Virtual wrapper target/overridden origin mismatch");
            Entity entity = Entities[wrapper.EntityId], target = Entities[wrapper.TargetEntityId], overridden = Entities[wrapper.OverriddenEntityId];
            if (Type(entity.ReturnTypeId) != Type(overridden.ReturnTypeId) || Routines[entity.RoutineTypeId].Variadic ||
                Routines[target.RoutineTypeId].Variadic || entity.Linkage != target.Linkage)
                throw new InvalidDataException("Virtual wrapper return/parameter/linkage contract mismatch");
            bool indirectResult = SourceRoutineAbis[original.TypeId].ReturnPassing == "result-buffer";
            if (indirectResult && (SourceRoutineAbis[overriddenOriginal.TypeId].ReturnPassing != "result-buffer" ||
                SourceTypeSignature(original.ReturnTypeId) != SourceTypeSignature(overriddenOriginal.ReturnTypeId) ||
                Type(entity.ReturnTypeId) != "void" || Type(target.ReturnTypeId) != "void" ||
                wrapper.ReturnDelta != 0 || wrapper.VbaseIndex != 0))
                throw new InvalidDataException("Virtual wrapper result buffer requires the same original indirect return without result adjustment");
            string resultType = Type(target.ReturnTypeId);
            if (resultType != Type(entity.ReturnTypeId) &&
                (!ScalarTypes.IsPointer(resultType) || !ScalarTypes.IsPointer(Type(entity.ReturnTypeId))))
                throw new InvalidDataException("Virtual wrapper has unsupported covariant result representation");
            if ((wrapper.ReturnDelta != 0 || wrapper.VbaseIndex != 0) && !ScalarTypes.IsPointer(resultType))
                throw new InvalidDataException("Virtual wrapper return adjustment requires a pointer result");
            var parameters = Parameters.GetValueOrDefault(wrapper.EntityId, []);
            var targetParameters = Parameters.GetValueOrDefault(wrapper.TargetEntityId, []);
            if (!parameters.SequenceEqual(targetParameters))
                throw new InvalidDataException("Virtual wrapper must forward the selected target's exact execution parameters");
            int receiverCount = 0, resultCount = 0;
            for (int ordinal = 0; ordinal < parameters.Count; ordinal++)
            {
                ParameterSemantics role = ParameterRoles[(wrapper.EntityId, ordinal)];
                ParameterSemantics targetRole = ParameterRoles[(wrapper.TargetEntityId, ordinal)];
                string expectedRole = targetRole.Role is "source" or "indirect-source" or "forwarded-source" ? "forwarded-source" : targetRole.Role;
                if (role.Role != expectedRole || role.SourceOrdinal != targetRole.SourceOrdinal)
                    throw new InvalidDataException("Virtual wrapper parameter lacks exact forwarded origin");
                if (role.Role == "this") receiverCount++;
                if (role.Role == "result-buffer") resultCount++;
            }
            if (receiverCount != 1 || resultCount != (indirectResult ? 1 : 0))
                throw new InvalidDataException("Virtual wrapper requires exactly its original receiver/result slots");
            var calls = WrapperCalls.Values.Where(call => call.WrapperEntityId == wrapper.EntityId).ToArray();
            if (Functions.ContainsKey(wrapper.EntityId) ? calls.Length != 1 : calls.Length != 0)
                throw new InvalidDataException("Virtual wrapper forwarding count differs from declaration/definition status");
        }

        RejectRows("SELECT d.id FROM source_declarations d JOIN thunk_wrappers w ON w.entity_id=d.entity_id OR w.entity_id=d.function_entity_id", "Virtual wrapper must not fabricate source declarations");
    }
    public void VerifyThunkWrapperBody(Entity entity, FunctionBody body)
    {
        if (!ThunkWrappers.TryGetValue(entity.Id, out ThunkWrapper? wrapper)) return;
        if (body.Bindings.Values.Any(binding => !binding.IsSynthetic))
            throw new InvalidDataException("Virtual wrapper body requires synthetic bindings");
        WrapperCall forwarding = WrapperCalls.Values.Single(call => call.WrapperEntityId == wrapper.EntityId);
        Entity target = Entities[wrapper.TargetEntityId];
        if (!body.Nodes.TryGetValue(forwarding.NodeId, out Node? node) || node.Kind != "call" ||
            node.FunctionId != wrapper.EntityId || node.EntityId != wrapper.TargetEntityId || node.TypeId != target.ReturnTypeId)
            throw new InvalidDataException("Virtual wrapper forwarding call has the wrong owner/target/result");
        var parameters = Parameters.GetValueOrDefault(wrapper.EntityId, []);
        var arguments = body.Children(node.Id, "argument").ToArray();
        var formals = body.Bindings.Values.Where(binding => binding.Kind == "parameter").OrderBy(binding => binding.Ordinal).ToArray();
        if (arguments.Length != parameters.Count || formals.Length != parameters.Count)
            throw new InvalidDataException("Virtual wrapper forwarding formal/argument count mismatch");
        for (int ordinal = 0; ordinal < arguments.Length; ordinal++)
        {
            Node argument = body.Nodes[arguments[ordinal].ChildId];
            if (arguments[ordinal].Ordinal != ordinal || argument.Kind != "binding" || argument.BindingId != formals[ordinal].Id ||
                formals[ordinal].Ordinal != ordinal || argument.TypeId != formals[ordinal].TypeId || Type(formals[ordinal].TypeId) != parameters[ordinal])
                throw new InvalidDataException("Virtual wrapper must forward each observed formal once in order");
            // Stock add_body_for_wrapper_routine changes only this. A caller-
            // owned result buffer is forwarded unchanged; it is neither copied
            // into another temporary nor read/assigned by the wrapper body.
            if (ParameterRoles[(entity.Id, ordinal)].Role == "result-buffer" &&
                body.Nodes.Values.Count(candidate => candidate.Kind == "binding" && candidate.BindingId == formals[ordinal].Id) != 1)
                throw new InvalidDataException("Virtual wrapper must use its unchanged result buffer only in the selected forwarding call");
        }
    }

}
