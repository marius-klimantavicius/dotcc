using System.Text.Json;

namespace DotObj;

internal sealed record SourceType(long Id, string Kind, string SourceName,
    long? UnderlyingTypeId, long? ExecutionRepresentationId, bool IsConst, bool IsVolatile,
    long? SourceFileId, long Line, long Column, long? ElementCount, bool IsRestrict, bool IsAtomic, string? ArrayBoundKind)
{
    // Semantic-only types are valid metadata. A consumer requesting executable
    // storage/ABI must have an observed mapping, never an invented fallback.
    public long ExecutionTypeId => ExecutionRepresentationId ??
        throw new InvalidDataException($"Source type {Id} ({Kind}) has no execution representation");
}

internal sealed partial class ObjectFile
{
    public Dictionary<long, SourceType> SourceTypes { get; } = [];
    public Dictionary<long, long> BindingSourceTypes { get; } = [];
    public Dictionary<(long EntityId, long Ordinal), bool> SourceParameterDefaults { get; } = [];

    private void ReadAndVerifySourceProvenance()
    {
        ReadRoutineMetadata(true);
        ReadAbiRoutineOrigins();
        ReadAndVerifySourceTypes();
        ReadAndVerifySourceRecordOwners();
        VerifySourceNestedTypeDeclarations();
        VerifySourceTemplateMetadata();
        ReadAndVerifySourceRoutineAbis();
        ReadAndVerifyDeferredRoutineAbis();
        foreach (RoutineType routine in Routines.Values)
            if (!DeferredRoutineKeys.Contains(Type(routine.TypeId)) &&
                (!CanTransportRecord(Type(routine.ReturnTypeId)) || routine.Parameters.Any(parameter => !CanPassRecord(Type(parameter)))))
                throw new InvalidDataException("Record value ABI cannot contain incomplete arrays or volatile fields in this profile");
        VerifyStorageSourceTypes();
        VerifySourceRecords();
        foreach (RecordField field in Fields.Values)
        {
            if (field.SourceTypeId is not long originalTypeId) continue;
            SourceType source = SourceTypeById(originalTypeId);
            if (source.ExecutionTypeId != field.TypeId)
                throw new InvalidDataException("Record field source type disagrees with execution representation");
            // Volatile reads/writes are represented by explicit volatile-place nodes.
            while (source.Kind is "typedef" or "qualified" or "array")
            {
                source = SourceTypeById(source.UnderlyingTypeId!.Value);
            }
        }
        var functions = new HashSet<long>();
        using (var query = database.Prepare("SELECT entity_id,return_source_type_id,capture_phase,routine_source_type_id,function_kind FROM source_functions ORDER BY entity_id"))
            while (query.Read())
            {
                long entity = query.Integer(0);
                string functionKind = query.Text(4);
                if (!Entities.TryGetValue(entity, out Entity? function) || query.Text(2) != (functionKind == "runtime" ? "runtime-declaration" : "before-lowering") ||
                    Original(SourceTypeById(query.Integer(1))).Kind == "array" || !functions.Add(entity))
                    throw new InvalidDataException("Source function signature or capture phase mismatch");
                SourceType signatureType = Original(SourceTypeById(query.Integer(3)));
                if (signatureType.Kind != "routine" || !SourceRoutines.TryGetValue(signatureType.Id, out RoutineType? sourceSignature) ||
                    Type(signatureType.ExecutionTypeId) != EntityRoutine(entity) || sourceSignature.ReturnTypeId != query.Integer(1) ||
                    RoutineAbiReturnType(sourceSignature) != Type(function.ReturnTypeId))
                    throw new InvalidDataException($"Original full function signature disagrees with function summary: {function.SourceName} (entity {entity}, source routine {signatureType.Id}, execution routine {function.RoutineTypeId})");
                if (functionKind is not ("ordinary" or "constructor" or "destructor" or "runtime") ||
                    functionKind is "constructor" or "destructor" && (sourceSignature.ObjectTypeId is null || Type(function.ReturnTypeId) != "void"))
                    throw new InvalidDataException("Invalid original special member function kind");
                if (functionKind == "runtime" && (function.Linkage != "external" || function.Definition != "declaration" || Functions.ContainsKey(entity) ||
                    SourceRoutineAbis[sourceSignature.TypeId].CaptureProof != "observed-runtime-signature"))
                    throw new InvalidDataException("Observed runtime function must be an external generated declaration with its own signature proof");
                if (functionKind != "runtime" && SourceRoutineAbis[sourceSignature.TypeId].CaptureProof == "observed-runtime-signature")
                    throw new InvalidDataException("Generated runtime signature cannot fabricate an original source function");
                SourceFunctionKinds.Add(entity, functionKind);
                EntitySourceRoutines.Add(entity, sourceSignature);
                var slots = RoutineAbiParameters(sourceSignature.TypeId);
                if (slots.Any(slot => slot.ParameterRole == "vtt") && functionKind is not ("constructor" or "destructor"))
                    throw new InvalidDataException("VTT pointer requires an original constructor or destructor");
                var executionParameters = Parameters.GetValueOrDefault(entity, []);
                if (executionParameters.Count != slots.Count)
                    throw new InvalidDataException("Execution parameters do not map to original explicit parameters and receiver");
                for (int ordinal = 0; ordinal < executionParameters.Count; ordinal++)
                {
                    ParameterSemantics role = ParameterRoles[(entity, ordinal)];
                    if (role.Role != slots[ordinal].ParameterRole || role.SourceOrdinal != slots[ordinal].SourceOrdinal)
                        throw new InvalidDataException("Execution parameter role disagrees with original routine signature");
                }
                if (sourceSignature.ParameterMode == "unspecified" && (function!.Definition == "declaration" || Parameters.GetValueOrDefault(entity, []).Count != 0))
                    throw new InvalidDataException("Unspecified source parameter mode requires a known zero-parameter definition");
            }
        var staticDestructors = new HashSet<long>();
        using (var generated = database.Prepare("SELECT function_entity_id FROM generated_static_destructors"))
            while (generated.Read())
                if (!Entities.ContainsKey(generated.Integer(0)) || !staticDestructors.Add(generated.Integer(0)))
                    throw new InvalidDataException("Invalid generated static destructor identity");
        if (functions.Overlaps(AbiRoutineOrigins.Keys) || functions.Overlaps(GeneratedRoutineOrigins.Keys) ||
            AbiRoutineOrigins.Keys.Any(GeneratedRoutineOrigins.ContainsKey) ||
            functions.Overlaps(ThunkWrappers.Keys) || AbiRoutineOrigins.Keys.Any(ThunkWrappers.ContainsKey) ||
            GeneratedRoutineOrigins.Keys.Any(ThunkWrappers.ContainsKey) ||
            staticDestructors.Overlaps(functions) || staticDestructors.Overlaps(AbiRoutineOrigins.Keys) ||
            staticDestructors.Overlaps(GeneratedRoutineOrigins.Keys) || staticDestructors.Overlaps(ThunkWrappers.Keys) ||
            functions.Count + AbiRoutineOrigins.Count + GeneratedRoutineOrigins.Count + ThunkWrappers.Count + staticDestructors.Count != Entities.Count)
            throw new InvalidDataException("Missing or overlapping original, ABI or generated function origin");
        var parameterCounts = new Dictionary<long, int>();
        using (var query = database.Prepare("SELECT entity_id,ordinal,source_type_id,has_default_argument FROM source_parameters ORDER BY entity_id,ordinal"))
            while (query.Read())
            {
                long entity = query.Integer(0), ordinal = query.Integer(1);
                int count = parameterCounts.GetValueOrDefault(entity);
                SourceType sourceType = SourceTypeById(query.Integer(2));
                if (!EntitySourceRoutines.TryGetValue(entity, out RoutineType? routine) || ordinal != count ||
                    count >= routine.Parameters.Length || !Parameters.TryGetValue(entity, out var parameters) ||
                    RoutineAbiParameterType(routine, SourceParameterSlot(routine, count)) != parameters[(int)SourceParameterSlot(routine, count).ExecutionOrdinal] ||
                    SourceParameterSignature(sourceType.Id, 0) != SourceParameterSignature(routine.Parameters[count], 0))
                    throw new InvalidDataException("Source parameter signature mismatch");
                if (query.Integer(3) is not (0 or 1)) throw new InvalidDataException("Invalid original parameter default argument fact");
                SourceParameterDefaults.Add((entity, ordinal), query.Integer(3) == 1);
                parameterCounts[entity] = count + 1;
            }
        foreach (var (entityId, sourceRoutine) in EntitySourceRoutines)
            if (parameterCounts.GetValueOrDefault(entityId) != sourceRoutine.Parameters.Length)
                throw new InvalidDataException("Missing original parameter signature capture");

        VerifyAbiRoutineOrigins();
        VerifyGeneratedRoutineOrigins();
        VerifyThunkWrappers();

        // Join declarations to their owned execution counterpart while streaming
        // rows. Retain the type IDs to verify declared volatile access sites.
        using (var query = database.Prepare("""
            SELECT d.id,d.kind,d.source_name,d.source_type_id,d.entity_id,d.binding_id,d.function_entity_id,d.line,d.column,
                   b.function_id,b.kind,b.ordinal,b.type_id,b.source_name,
                   f.return_source_type_id,p.source_type_id,b.ordinal
            FROM source_declarations d
            LEFT JOIN bindings b ON b.id=d.binding_id
            LEFT JOIN source_functions f ON f.entity_id=d.entity_id
            LEFT JOIN parameters execution_parameter ON execution_parameter.entity_id=b.function_id AND execution_parameter.ordinal=b.ordinal
            LEFT JOIN source_parameters p ON p.entity_id=b.function_id AND p.ordinal=execution_parameter.source_ordinal
            ORDER BY d.id
            """))
            while (query.Read())
            {
                long sourceTypeId = query.Integer(3);
                SourceType type = SourceTypeById(sourceTypeId);
                string kind = query.Text(1), name = query.Text(2);
                if (query.Integer(0) <= 0 || query.Integer(7) < 0 || query.Integer(8) < 0)
                    throw new InvalidDataException("Invalid source declaration identity or location");
                if (kind == "function")
                {
                    if (query.OptionalInteger(4) is not long entity || !query.IsNull(5) || !query.IsNull(6) ||
                        !Entities.TryGetValue(entity, out Entity? function) || name != function.SourceName ||
                        query.OptionalInteger(14) != sourceTypeId || RoutineAbiReturnType(EntitySourceRoutines[entity]) != Type(function.ReturnTypeId))
                        throw new InvalidDataException("Source function declaration does not match its execution entity");
                }
                else if (kind is "parameter" or "local")
                {
                    if (!query.IsNull(4) || query.IsNull(5) || query.OptionalInteger(6) is not long function ||
                        !Functions.ContainsKey(function) || query.OptionalInteger(9) != function || query.OptionalText(10) != kind ||
                        (kind == "parameter" ? ParameterBindingType(function, query.Integer(16), type) != Type(query.Integer(12)) : type.ExecutionTypeId != query.OptionalInteger(12)) || query.OptionalText(13) != name)
                        throw new InvalidDataException("Source binding declaration does not match its execution binding");
                    if (!BindingSourceTypes.TryAdd(query.Integer(5), sourceTypeId)) throw new InvalidDataException("Duplicate source binding declaration identity");
                    if (kind == "local" && Original(type).Kind == "array" && Original(type).ElementCount is null)
                        throw new InvalidDataException("Incomplete array is only supported as original parameter provenance");
                    // A canonical routine signature can come from an earlier
                    // prototype using a different typedef or top-level cv spelling.
                    // The definition's declared parameter keeps its own source type.
                    if (kind == "parameter" && (query.OptionalInteger(15) is not long signatureType ||
                        ParameterType(SourceTypeById(signatureType)) != ParameterType(type) ||
                        SourceParameterSignature(signatureType, 0) != SourceParameterSignature(type.Id, 0)))
                        throw new InvalidDataException("Source parameter declaration does not match its signature representation");
                }
                else throw new InvalidDataException("Unsupported source declaration kind");
            }
        RejectRows("SELECT entity_id FROM source_declarations WHERE entity_id IS NOT NULL GROUP BY entity_id HAVING count(*)<>1", "Duplicate source function declaration identity");
        RejectRows("SELECT binding_id FROM source_declarations WHERE binding_id IS NOT NULL GROUP BY binding_id HAVING count(*)<>1", "Duplicate source binding declaration identity");
        RejectRows("SELECT e.id FROM entities e LEFT JOIN source_declarations d ON d.entity_id=e.id WHERE d.id IS NULL AND e.id NOT IN (SELECT entity_id FROM abi_routine_origins) AND e.id NOT IN (SELECT entity_id FROM generated_routine_origins) AND e.id NOT IN (SELECT entity_id FROM thunk_wrappers) AND e.id NOT IN (SELECT function_entity_id FROM generated_static_destructors)", "Missing original function declaration capture");
        RejectRows("SELECT d.id FROM source_declarations d JOIN generated_static_destructors g ON g.function_entity_id=d.entity_id", "Generated static destructor must not fabricate an original declaration");
        RejectRows("SELECT b.id FROM bindings b LEFT JOIN parameters p ON p.entity_id=b.function_id AND p.ordinal=b.ordinal WHERE b.kind='parameter' AND (p.parameter_role IS NULL OR b.is_synthetic <> (p.parameter_role IN ('this','result-buffer','vtt','forwarded-source','static-object')))", "Implicit ABI parameters must be synthetic and source parameters must be original");
        RejectRows("SELECT b.id FROM bindings b JOIN source_declarations d ON d.binding_id=b.id WHERE b.is_synthetic=1", "Synthetic execution binding must not have fabricated source capture");
        RejectRows("SELECT b.id FROM bindings b LEFT JOIN source_declarations d ON d.binding_id=b.id WHERE d.id IS NULL AND b.is_synthetic=0", "Missing original binding declaration capture");
    }

    private SourceType Original(SourceType type)
    {
        var seen = new HashSet<long>();
        while (type.Kind is "typedef" or "qualified")
        {
            if (!seen.Add(type.Id)) throw new InvalidDataException("Cycle in source type references");
            type = SourceTypeById(type.UnderlyingTypeId!.Value);
        }
        return type;
    }
    private string ParameterType(SourceType type)
    {
        SourceType original = Original(type);
        if (original.Kind == "routine") return "ptr(" + Type(original.ExecutionTypeId) + ")";
        return original.Kind == "array"
            ? "ptr(" + ScalarTypes.Element(Type(original.ExecutionTypeId)) + ")" : Type(type.ExecutionTypeId);
    }

    private SourceType SourceTypeById(long id) => SourceTypes.TryGetValue(id, out SourceType? type)
        ? type : throw new InvalidDataException("Unknown original source type identity");

    public string SourceTypeSignature(long id) => SourceTypeSignature(id, 0, false, false);

    public string FunctionSourceSignature(long entityId) => AbiRoutineOrigins.TryGetValue(entityId, out AbiRoutineOrigin? origin)
        ? "abi-entry(" + origin.EntryKind + ";" + SourceFunctionKinds[origin.PrimaryEntityId] + ";" + (origin.EntryKind == "constructor-default-adapter" ? "prefix=" + (Parameters.GetValueOrDefault(entityId, []).Count - 1) + ";" : "") + SourceRoutineSignature(EntitySourceRoutines[origin.PrimaryEntityId], 0, includeAbi: true) + ")"
        : GeneratedRoutineOrigins.TryGetValue(entityId, out GeneratedRoutineOrigin? generated)
            ? "generated-routine(" + generated.Kind + ";" + EntityRoutine(entityId) + ";" + generated.FailurePolicy + ")"
            : GeneratedStaticDestructors.TryGetValue(entityId, out GeneratedStaticDestructor? destruction)
                ? StaticDestructorSourceSignature(destruction)
            : ThunkWrappers.TryGetValue(entityId, out ThunkWrapper? wrapper)
                ? "virtual-wrapper(" + Convert.ToHexString(System.Text.Encoding.UTF8.GetBytes(new Symbol(this, Entities[wrapper.TargetEntityId]).Key)) + ":" +
                    FunctionSourceSignature(wrapper.TargetEntityId) + ";" + Convert.ToHexString(System.Text.Encoding.UTF8.GetBytes(new Symbol(this, Entities[wrapper.OverriddenEntityId]).Key)) + ":" +
                    FunctionSourceSignature(wrapper.OverriddenEntityId) + ";" + wrapper.ThisDelta + ";" + wrapper.VcallIndex + ";" +
                    wrapper.ReturnDelta + ";" + wrapper.VbaseIndex + ")"
                : SourceRoutineSignature(EntitySourceRoutines[entityId], 0, includeAbi: true);

    private string SourceRoutineSignature(RoutineType routine, int depth, bool includeAbi = false)
    {
        if (depth > 128) throw new InvalidDataException("Source routine type nesting exceeds supported limit");
        // This profile admits unspecified C definitions only with known zero
        // parameters. They are compatible with an explicit (void) prototype.
        string receiver = routine.ObjectTypeId is long objectType
            ? SourceTypeSignature(objectType, depth + 1, routine.ObjectIsConst, routine.ObjectIsVolatile) + ":" + routine.ObjectRefQualifier : "none";
        return "source-routine(" + SourceTypeSignature(routine.ReturnTypeId, depth + 1, false, false) + ";" +
            string.Join(',', routine.Parameters.Select(parameter => SourceParameterSignature(parameter, depth + 1))) + ";" +
            (routine.Variadic ? "1" : "0") + ";" + routine.ExceptionPolicy + ";" + receiver +
            (includeAbi ? ";abi=" + SourceRoutineAbis[routine.TypeId].ReturnPassing + ":" + string.Join(',', RoutineAbiParameters(routine.TypeId).Select(slot =>
                slot.ParameterRole + (slot.SourceOrdinal is long ordinal ? "#" + ordinal : ""))) : "") + ")";
    }

    private string SourceParameterSignature(long id, int depth)
    {
        if (depth > 128) throw new InvalidDataException("Source parameter type nesting exceeds supported limit");
        SourceType type = SourceTypeById(id);
        bool isConst = type.IsConst, isVolatile = type.IsVolatile;
        while (type.Kind is "typedef" or "qualified")
        {
            if (++depth > 128) throw new InvalidDataException("Source parameter alias nesting exceeds supported limit");
            type = SourceTypeById(type.UnderlyingTypeId!.Value);
            isConst |= type.IsConst; isVolatile |= type.IsVolatile;
        }
        // Array element qualifications survive decay; pointer top-level cv does
        // not participate in a function type. References never undergo decay.
        if (type.Kind == "array") return "ptr(" + SourceTypeSignature(type.UnderlyingTypeId!.Value, depth + 1, isConst, isVolatile) + ")";
        if (type.Kind == "routine") return "ptr(" + SourceRoutineSignature(SourceRoutines[type.Id], depth + 1) + ")";
        return SourceTypeSignature(type.Id, depth + 1, false, false);
    }

    private string SourceTypeSignature(long id, int depth, bool inheritedConst, bool inheritedVolatile)
    {
        if (depth > 128) throw new InvalidDataException("Source field type nesting exceeds supported limit");
        SourceType type = SourceTypeById(id);
        bool isConst = type.IsConst || inheritedConst, isVolatile = type.IsVolatile || inheritedVolatile;
        bool isRestrict = type.IsRestrict, isAtomic = type.IsAtomic;
        var extra = new SortedSet<string>(SourceTypeQualifiers.GetValueOrDefault(type.Id, []), StringComparer.Ordinal);
        while (type.Kind is "typedef" or "qualified")
        {
            if (++depth > 128) throw new InvalidDataException("Source field type nesting exceeds supported limit");
            type = SourceTypeById(type.UnderlyingTypeId!.Value);
            isConst |= type.IsConst; isVolatile |= type.IsVolatile;
            isRestrict |= type.IsRestrict; isAtomic |= type.IsAtomic;
            extra.UnionWith(SourceTypeQualifiers.GetValueOrDefault(type.Id, []));
        }
        if (type.Kind == "array") return "array(" + (SourceArrayBounds.TryGetValue(type.Id, out long bound)
            ? "dependent:" + SourceConstantIdentity(bound, depth + 1) : type.ElementCount?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "?") + "," +
            SourceTypeSignature(type.UnderlyingTypeId!.Value, depth + 1, isConst, isVolatile) + ")";
        // References themselves cannot be cv-qualified; qualifiers belong to
        // their referent and are retained inside the distinct lref/rref node.
        if (type.Kind is "lvalue-reference" or "rvalue-reference")
            return (type.Kind == "lvalue-reference" ? "lref(" : "rref(") + SourceTypeSignature(type.UnderlyingTypeId!.Value, depth + 1, false, false) + ")";
        string qualifiers = (isConst ? "const:" : "") + (isVolatile ? "volatile:" : "") + (isRestrict ? "restrict:" : "") + (isAtomic ? "atomic:" : "") + string.Concat(extra.Select(name => name + ":"));
        return qualifiers + (type.Kind switch
        {
            "pointer" => "ptr(" + SourceTypeSignature(type.UnderlyingTypeId!.Value, depth + 1, false, false) + ")",
            "record" => "record(" + Convert.ToHexStringLower(System.Text.Encoding.UTF8.GetBytes(SourceRecordsByType[type.Id].IdentityKey)) + ")",
            "routine" => SourceRoutineSignature(SourceRoutines[type.Id], depth + 1),
            "enum" => SourceEnumSignature(type, depth + 1),
            "member-pointer" => "member-pointer(" + SourceTypeSignature(SourceMemberPointers[type.Id].OwnerTypeId, depth + 1, false, false) + "," + SourceTypeSignature(SourceMemberPointers[type.Id].MemberTypeId, depth + 1, false, false) + ")",
            "dependent" => DependentSourceSignature(type, depth + 1),
            _ => type.Kind
        });
    }

    private void RejectRows(string sql, string message)
    {
        using var query = database.Prepare(sql);
        if (query.Read()) throw new InvalidDataException(message);
    }

    public void WriteSourceMap(Utf8JsonWriter writer)
    {
        writer.WriteStartObject();
        writer.WriteString("unitId", UnitId);
        writer.WriteString("primarySource", PrimarySource);
        writer.WriteString("sourceLanguage", SourceLanguage);
        writer.WriteString("semanticDigest", Digest);
        void Table(string name, string sql, params string[] columns)
        {
            writer.WriteStartArray(name);
            using var query = database.Prepare(sql);
            while (query.Read())
            {
                writer.WriteStartObject();
                for (int index = 0; index < columns.Length; index++)
                {
                    string column = columns[index];
                    switch (query.ColumnType(index))
                    {
                        case 1:
                            if (column is "isRestrict" or "isAtomic" or "isScoped" or "enumeratorListComplete" or "hasExplicitBase" or "isUnreachable" or "hasVolatileKeyword" or "hasInlineKeyword" or "clobbersMemory" or "hasDefaultArgument" or "isAliasEntry" or "isVirtual" or "isConst" or "isVolatile" or "isSynthetic" or "isComplete" or "isTriviallyCopyable" or "isMutable" or "hasNoUniqueAddress" or "hasInitializer" or "objectIsConst" or "objectIsVolatile") writer.WriteBoolean(column, query.Integer(index) == 1);
                            else writer.WriteNumber(column, query.Integer(index));
                            break;
                        case 3: writer.WriteString(column, query.Text(index)); break;
                        case 5: writer.WriteNull(column); break;
                        default: throw new InvalidDataException("Unsupported source map cell type");
                    }
                }
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }
        Table("sourceArrayDeletions", "SELECT source_obligation_id,deallocator_entity_id,capture_proof FROM source_array_deletions ORDER BY source_obligation_id",
            "sourceObligationId", "deallocatorEntityId", "captureProof");
        Table("arrayDeletionOperations", "SELECT node_id,source_obligation_id,destructor_entity_id,deallocator_entity_id,element_type_id,padding_size,helper_kind,capture_proof FROM array_deletion_operations ORDER BY node_id",
            "nodeId", "sourceObligationId", "destructorEntityId", "deallocatorEntityId", "elementTypeId", "paddingSize", "helperKind", "captureProof");
        Table("sourceArrayAllocationElements", "SELECT source_allocation_obligation_id,source_destruction_obligation_id,capture_proof FROM source_array_allocation_elements ORDER BY source_allocation_obligation_id",
            "sourceAllocationObligationId", "sourceDestructionObligationId", "captureProof");
        Table("arrayAllocationOperations", "SELECT node_id,source_allocation_obligation_id,allocator_entity_id,deallocator_entity_id,constructor_entity_id,destructor_entity_id,element_type_id,padding_size,helper_kind,capture_proof,source_destruction_obligation_id FROM array_allocation_operations ORDER BY node_id",
            "nodeId", "sourceAllocationObligationId", "allocatorEntityId", "deallocatorEntityId", "constructorEntityId", "destructorEntityId", "elementTypeId", "paddingSize", "helperKind", "captureProof", "sourceDestructionObligationId");
        Table("arrayRuntimeOperations", "SELECT node_id,source_obligation_id,callback_entity_id,operation_kind,capture_proof,source_static_obligation_id,source_field_initializer_id FROM array_runtime_operations ORDER BY node_id",
            "nodeId", "sourceObligationId", "callbackEntityId", "operationKind", "captureProof", "sourceStaticObligationId", "sourceFieldInitializerId");
        Table("cleanupArrays", "SELECT action_id,element_type_id,element_count,element_size,capture_proof FROM cleanup_arrays ORDER BY action_id", "actionId", "elementTypeId", "elementCount", "elementSize", "captureProof");
        Table("cleanupVttArguments", "SELECT action_id,binding_id,capture_proof FROM cleanup_vtt_arguments ORDER BY action_id", "actionId", "bindingId", "captureProof");
        Table("cleanupConditions", "SELECT action_id,flag_binding_id,capture_proof FROM cleanup_conditions ORDER BY action_id", "actionId", "flagBindingId", "captureProof");
        Table("virtualBaseExpressions", "SELECT node_id,complete_base_type_id FROM virtual_base_expressions ORDER BY node_id", "nodeId", "completeBaseTypeId");
        Table("virtualBaseSubobjects", "SELECT record_type_id,complete_base_type_id,physical_view_type_id,byte_offset,extent,field_id,capture_proof FROM virtual_base_subobjects ORDER BY record_type_id,complete_base_type_id", "recordTypeId", "completeBaseTypeId", "physicalViewTypeId", "byteOffset", "extent", "fieldId", "captureProof");
        Table("normalDestructions", "SELECT node_id,source_obligation_id,capture_proof,source_static_obligation_id,source_field_initializer_id FROM normal_destructions ORDER BY node_id",
            "nodeId", "sourceObligationId", "captureProof", "sourceStaticObligationId", "sourceFieldInitializerId");
        Table("sourceAllocationObligations", "SELECT id,function_entity_id,lifetime_id,ordinal,deallocator_entity_id,allocated_source_type_id,is_array,source_file_id,line,column,capture_proof,source_initializer_id,source_static_obligation_id FROM source_allocation_obligations ORDER BY id",
            "id", "functionEntityId", "lifetimeId", "ordinal", "deallocatorEntityId", "allocatedSourceTypeId", "isArray", "sourceFileId", "line", "column", "captureProof", "sourceInitializerId", "sourceStaticObligationId");
        Table("compileTimeSelections", "SELECT node_id,condition_value,has_else,capture_proof FROM compile_time_selections ORDER BY node_id", "nodeId", "conditionValue", "hasElse", "captureProof");
        Table("thunkWrappers", "SELECT entity_id,target_entity_id,overridden_entity_id,primary_entity_id,this_delta,vcall_index,return_delta,vbase_index,capture_proof FROM thunk_wrappers ORDER BY entity_id", "entityId", "targetEntityId", "overriddenEntityId", "primaryEntityId", "thisDelta", "vcallIndex", "returnDelta", "vbaseIndex", "captureProof");
        Table("wrapperCalls", "SELECT node_id,wrapper_entity_id,capture_proof FROM wrapper_calls ORDER BY node_id", "nodeId", "wrapperEntityId", "captureProof");
        Table("internalTryExpressions", "SELECT node_id,source_allocation_obligation_id,cleanup_policy,capture_proof FROM internal_try_expressions ORDER BY node_id", "nodeId", "sourceAllocationObligationId", "cleanupPolicy", "captureProof");
        Table("expressionLifetimes", "SELECT node_id,source_lifetime_id,capture_proof FROM expression_lifetimes ORDER BY node_id", "nodeId", "sourceLifetimeId", "captureProof");
        Table("baseSubobjectExpressions", "SELECT node_id,source_base_id FROM base_subobject_expressions ORDER BY node_id", "nodeId", "sourceBaseId");
        Table("sourceNestedTypeDeclarations", "SELECT type_id,owner_type_id,capture_proof FROM source_nested_type_declarations ORDER BY type_id", "typeId", "ownerTypeId", "captureProof");
        Table("sourceDependentTypes", "SELECT type_id,kind,owner_type_id,original_nested_type_id,nesting_depth,parameter_position,is_pack,identity_key,capture_proof FROM source_dependent_types ORDER BY type_id",
            "typeId", "kind", "ownerTypeId", "originalNestedTypeId", "nestingDepth", "parameterPosition", "isPack", "identityKey", "captureProof");
        Table("sourceTemplates", "SELECT id,kind,source_name,parameter_depth,parameter_position,is_pack,parameter_count,capture_proof,source_file_id,line,column FROM source_templates ORDER BY id",
            "id", "kind", "sourceName", "parameterDepth", "parameterPosition", "isPack", "parameterCount", "captureProof", "sourceFileId", "line", "column");
        Table("sourceTemplateParameters", "SELECT source_template_id,ordinal,kind,source_name,type_id,constant_id,template_id,default_type_id,default_constant_id,default_template_id,is_pack,is_abbreviated,source_file_id,line,column FROM source_template_parameters ORDER BY source_template_id,ordinal",
            "sourceTemplateId", "ordinal", "kind", "sourceName", "typeId", "constantId", "templateId", "defaultTypeId", "defaultConstantId", "defaultTemplateId", "isPack", "isAbbreviated", "sourceFileId", "line", "column");
        Table("sourceRecordInstantiations", "SELECT source_record_id,prototype_source_record_id,resolution_kind,source_template_id,member_owner_source_type_id,member_of_unknown_base,qualified_unknown_base_member,capture_proof FROM source_record_instantiations ORDER BY source_record_id",
            "sourceRecordId", "prototypeSourceRecordId", "resolutionKind", "sourceTemplateId", "memberOwnerSourceTypeId", "memberOfUnknownBase", "qualifiedUnknownBaseMember", "captureProof");
        Table("sourceTemplateArguments", "SELECT source_record_id,ordinal,kind,type_id,constant_id,template_id FROM source_template_arguments ORDER BY source_record_id,ordinal",
            "sourceRecordId", "ordinal", "kind", "typeId", "constantId", "templateId");
        Table("sourceArrayBounds", "SELECT type_id,constant_id,capture_proof FROM source_array_bounds ORDER BY type_id",
            "typeId", "constantId", "captureProof");
        Table("sourceSemanticConstants", "SELECT id,kind,source_type_id,source_name,value_text,parameter_depth,parameter_position,is_pack,operator_name,owner_source_type_id,operand_source_type_id,builtin_operator_kind,is_standard_alignof,source_file_id,line,column FROM source_semantic_constants ORDER BY id",
            "id", "kind", "sourceTypeId", "sourceName", "valueText", "parameterDepth", "parameterPosition", "isPack", "operatorName", "ownerSourceTypeId", "operandSourceTypeId", "builtinOperatorKind", "isStandardAlignof", "sourceFileId", "line", "column");
        Table("sourceSemanticConstantOperands", "SELECT constant_id,ordinal,operand_id FROM source_semantic_constant_operands ORDER BY constant_id,ordinal",
            "constantId", "ordinal", "operandId");
        Table("sourceInitializers", "SELECT id,storage_id,source_name,execution_disposition,source_type_id,function_entity_id,source_ordinal,kind,source_file_id,line,column,capture_proof,source_field_id FROM source_initializers ORDER BY id",
            "id", "storageId", "sourceName", "executionDisposition", "sourceTypeId", "functionEntityId", "sourceOrdinal", "kind", "sourceFileId", "line", "column", "captureProof", "sourceFieldId");
        Table("sourceFieldInitializerDestructions", "SELECT source_initializer_id,source_type_id,selected_routine_entity_id,capture_proof FROM source_field_initializer_destructions ORDER BY source_initializer_id",
            "sourceInitializerId", "sourceTypeId", "selectedRoutineEntityId", "captureProof");
        Table("fieldInitializerExecutions", "SELECT source_initializer_id,function_entity_id,constructor_ordinal,execution_disposition,capture_proof FROM field_initializer_executions ORDER BY source_initializer_id,function_entity_id",
            "sourceInitializerId", "functionEntityId", "constructorOrdinal", "executionDisposition", "captureProof");
        Table("fieldInitializerPartialDestructionExecutions", "SELECT source_static_obligation_id,function_entity_id,kind,capture_proof FROM field_initializer_partial_destruction_executions ORDER BY source_static_obligation_id,function_entity_id",
            "sourceStaticObligationId", "functionEntityId", "kind", "captureProof");
        Table("fieldInitializerDestructionElisions", "SELECT source_initializer_id,function_entity_id,kind,capture_proof FROM field_initializer_destruction_elisions ORDER BY source_initializer_id,function_entity_id",
            "sourceInitializerId", "functionEntityId", "kind", "captureProof");
        Table("sourceStaticLifetimes", "SELECT id,function_entity_id,source_initializer_id,parent_lifetime_id,parent_obligation_id,sibling_ordinal,kind,source_file_id,line,column,capture_proof FROM source_static_lifetimes ORDER BY id",
            "id", "functionEntityId", "sourceInitializerId", "parentLifetimeId", "parentObligationId", "siblingOrdinal", "kind", "sourceFileId", "line", "column", "captureProof");
        Table("sourceStaticDestructions", "SELECT id,lifetime_id,ordinal,storage_id,source_type_id,selected_routine_entity_id,source_initializer_id,kind,is_array_allocation,domain,source_file_id,line,column,capture_proof FROM source_static_destructions ORDER BY id",
            "id", "lifetimeId", "ordinal", "storageId", "sourceTypeId", "selectedRoutineEntityId", "sourceInitializerId", "kind", "isArrayAllocation", "domain", "sourceFileId", "line", "column", "captureProof");
        Table("initializerLifetimeExecutions", "SELECT source_lifetime_id,source_static_lifetime_id,source_initializer_id,function_entity_id,capture_proof FROM initializer_lifetime_executions ORDER BY source_lifetime_id",
            "sourceLifetimeId", "sourceStaticLifetimeId", "sourceInitializerId", "functionEntityId", "captureProof");
        Table("staticDestructionRegistrations", "SELECT node_id,source_obligation_id,source_initializer_id,function_entity_id,callback_entity_id,object_node_id,domain,capture_proof FROM static_destruction_registrations ORDER BY node_id",
            "nodeId", "sourceObligationId", "sourceInitializerId", "functionEntityId", "callbackEntityId", "objectNodeId", "domain", "captureProof");
        Table("generatedStaticDestructors", "SELECT function_entity_id,source_obligation_id,parameter_binding_id,storage_id,element_type_id,element_count,element_size,capture_proof FROM generated_static_destructors ORDER BY function_entity_id",
            "functionEntityId", "sourceObligationId", "parameterBindingId", "storageId", "elementTypeId", "elementCount", "elementSize", "captureProof");
        Table("staticDestructionDispositions", "SELECT source_obligation_id,kind,capture_proof FROM static_destruction_dispositions ORDER BY source_obligation_id",
            "sourceObligationId", "kind", "captureProof");
        Table("sourceLifetimes", "SELECT id,function_entity_id,parent_lifetime_id,sibling_ordinal,parent_obligation_id,parent_allocation_obligation_id,kind,source_file_id,line,column,capture_proof,source_initializer_id FROM source_lifetimes ORDER BY id",
            "id", "functionEntityId", "parentLifetimeId", "siblingOrdinal", "parentObligationId", "parentAllocationObligationId", "kind", "sourceFileId", "line", "column", "captureProof", "sourceInitializerId");
        Table("sourceDestructionObligations", "SELECT id,function_entity_id,lifetime_id,ordinal,destructor_entity_id,source_type_id,source_file_id,line,column,capture_proof,source_initializer_id,source_static_obligation_id,is_partial_aggregate FROM source_destruction_obligations ORDER BY id",
            "id", "functionEntityId", "lifetimeId", "ordinal", "destructorEntityId", "sourceTypeId", "sourceFileId", "line", "column", "captureProof", "sourceInitializerId", "sourceStaticObligationId", "isPartialAggregate");
        Table("destructionElisions", "SELECT source_obligation_id,function_entity_id,kind,capture_proof FROM destruction_elisions ORDER BY source_obligation_id,function_entity_id",
            "sourceObligationId", "functionEntityId", "kind", "captureProof");
        Table("allocationCleanupActions", "SELECT id,function_entity_id,source_allocation_obligation_id,callback_entity_id,target_node_id,allocation_size,condition_binding_id,null_policy,capture_proof FROM allocation_cleanup_actions ORDER BY id",
            "id", "functionEntityId", "sourceAllocationObligationId", "callbackEntityId", "targetNodeId", "allocationSize", "conditionBindingId", "nullPolicy", "captureProof");
        Table("allocationElisions", "SELECT source_allocation_obligation_id,function_entity_id,kind,capture_proof FROM allocation_elisions ORDER BY source_allocation_obligation_id,function_entity_id",
            "sourceAllocationObligationId", "functionEntityId", "kind", "captureProof");
        Table("localStaticGuards", "SELECT id,function_entity_id,storage_id,guard_storage_id,source_type_id,source_lifetime_id,guard_policy,source_file_id,line,column,capture_proof FROM local_static_guards ORDER BY id",
            "id", "functionEntityId", "storageId", "guardStorageId", "sourceTypeId", "sourceLifetimeId", "guardPolicy", "sourceFileId", "line", "column", "captureProof");
        Table("guardOperations", "SELECT node_id,guard_id,kind,capture_proof FROM guard_operations ORDER BY node_id",
            "nodeId", "guardId", "kind", "captureProof");
        Table("guardCleanupActions", "SELECT id,function_entity_id,guard_id,target_node_id,capture_proof FROM guard_cleanup_actions ORDER BY id",
            "id", "functionEntityId", "guardId", "targetNodeId", "captureProof");
        Table("cleanupActions", "SELECT id,function_entity_id,source_obligation_id,source_static_obligation_id,callback_entity_id,target_node_id,capture_proof,source_field_initializer_id FROM cleanup_actions ORDER BY id",
            "id", "functionEntityId", "sourceObligationId", "sourceStaticObligationId", "callbackEntityId", "targetNodeId", "captureProof", "sourceFieldInitializerId");
        Table("cleanupStates", "SELECT id,function_entity_id,action_id,next_state_id,handler_node_id,allocation_action_id,guard_action_id FROM cleanup_states ORDER BY id",
            "id", "functionEntityId", "actionId", "nextStateId", "handlerNodeId", "allocationActionId", "guardActionId");
        Table("sourceTries", "SELECT node_id,source_lifetime_id,try_kind,capture_proof FROM source_tries ORDER BY node_id",
            "nodeId", "sourceLifetimeId", "tryKind", "captureProof");
        Table("sourceHandlers", "SELECT node_id,try_node_id,ordinal,match_kind,source_type_id,match_type_id,parameter_binding_id,capture_proof FROM source_handlers ORDER BY node_id",
            "nodeId", "tryNodeId", "ordinal", "matchKind", "sourceTypeId", "matchTypeId", "parameterBindingId", "captureProof");
        Table("caughtObjectAddresses", "SELECT node_id,handler_node_id,capture_proof FROM caught_object_addresses ORDER BY node_id",
            "nodeId", "handlerNodeId", "captureProof");
        Table("handlerMarkers", "SELECT node_id,owner_node_id,kind,capture_proof FROM handler_markers ORDER BY node_id",
            "nodeId", "ownerNodeId", "kind", "captureProof");
        Table("cleanupTransitions", "SELECT node_id,state_id,capture_proof,is_unreachable FROM cleanup_transitions ORDER BY node_id",
            "nodeId", "stateId", "captureProof", "isUnreachable");
        Table("cleanupFrameMarkers", "SELECT node_id,kind,capture_proof FROM cleanup_frame_markers ORDER BY node_id",
            "nodeId", "kind", "captureProof");
        Table("memoryBarriers", "SELECT node_id,is_volatile,has_volatile_keyword,has_inline_keyword,clobbers_memory,capture_proof FROM memory_barriers ORDER BY node_id",
            "nodeId", "isVolatile", "hasVolatileKeyword", "hasInlineKeyword", "clobbersMemory", "captureProof");
        Table("memoryBarrierOperands", "SELECT node_id,ordinal,direction,constraint_text,source_name,source_element_type_id,element_size,source_shape,extent_kind,fixed_element_count,extent_binding_id,capture_proof,source_file_id,line,column FROM memory_barrier_operands ORDER BY node_id,ordinal",
            "nodeId", "ordinal", "direction", "constraintText", "sourceName", "sourceElementTypeId", "elementSize", "sourceShape", "extentKind", "fixedElementCount", "extentBindingId", "captureProof", "sourceFileId", "line", "column");
        Table("memoryBarrierDimensions", "SELECT node_id,operand_ordinal,ordinal,kind,fixed_count,extent_binding_id,source_file_id,line,column FROM memory_barrier_dimensions ORDER BY node_id,operand_ordinal,ordinal",
            "nodeId", "operandOrdinal", "ordinal", "kind", "fixedCount", "extentBindingId", "sourceFileId", "line", "column");
        Table("generatedRoutineOrigins", "SELECT entity_id,kind,capture_proof,failure_policy FROM generated_routine_origins ORDER BY entity_id",
            "entityId", "kind", "captureProof", "failurePolicy");
        Table("generatedRoutineScaffolds", "SELECT entity_id,ordinal,node_id,binding_id,kind,capture_proof FROM generated_routine_scaffolds ORDER BY entity_id,ordinal",
            "entityId", "ordinal", "nodeId", "bindingId", "kind", "captureProof");
        Table("dynamicInitializers", "SELECT id,action_id,storage_id,source_initializer_id,source_ordinal,ordinal,capture_proof,source_file_id,line,column FROM dynamic_initializers ORDER BY id",
            "id", "actionId", "storageId", "sourceInitializerId", "sourceOrdinal", "ordinal", "captureProof", "sourceFileId", "line", "column");
        Table("dynamicInitializerStatements", "SELECT initializer_id,ordinal,node_id FROM dynamic_initializer_statements ORDER BY initializer_id,ordinal",
            "initializerId", "ordinal", "nodeId");
        Table("initializationActions", "SELECT id,entity_id,phase,priority,ordinal,source_file_id,line,column FROM initialization_actions ORDER BY ordinal",
            "id", "entityId", "phase", "priority", "ordinal", "sourceFileId", "line", "column");
        Table("sourceFiles", "SELECT id,path,parent_id FROM source_files ORDER BY id", "id", "path", "parentId");
        Table("executionTypes", "SELECT id,kind,pointee_type_id,element_type_id,element_count FROM types ORDER BY id", "id", "kind", "pointeeTypeId", "elementTypeId", "elementCount");
        Table("recordTypes", "SELECT type_id,identity_key,source_name,size,alignment,source_file_id,line,column,layout_kind,source_record_id,representation_kind,value_copy_kind,capture_proof,complete_type_id FROM record_types ORDER BY type_id",
            "typeId", "identityKey", "sourceName", "size", "alignment", "sourceFileId", "line", "column", "layoutKind", "sourceRecordId", "representationKind", "valueCopyKind", "captureProof", "completeTypeId");
        Table("recordFields", "SELECT id,record_type_id,ordinal,type_id,source_name,byte_offset,source_type_id,source_file_id,line,column,bit_offset,bit_width,bit_signed,source_field_id,field_role,source_base_id,observed_alignment FROM record_fields ORDER BY id",
            "id", "recordTypeId", "ordinal", "typeId", "sourceName", "byteOffset", "sourceTypeId", "sourceFileId", "line", "column", "bitOffset", "bitWidth", "bitSigned", "sourceFieldId", "fieldRole", "sourceBaseId", "observedAlignment");
        Table("exceptionTypes", "SELECT id,source_type_id,leaf_source_type_id,payload_type_id,payload_size,payload_alignment,source_destructor_entity_id,capture_proof FROM exception_types ORDER BY id",
            "id", "sourceTypeId", "leafSourceTypeId", "payloadTypeId", "payloadSize", "payloadAlignment", "sourceDestructorEntityId", "captureProof");
        Table("exceptionBaseConversions", "SELECT exception_type_id,ordinal,base_source_type_id,byte_offset,is_public,is_ambiguous,capture_proof FROM exception_base_conversions ORDER BY exception_type_id,ordinal",
            "exceptionTypeId", "ordinal", "baseSourceTypeId", "byteOffset", "isPublic", "isAmbiguous", "captureProof");
        Table("throwExpressions", "SELECT node_id,exception_type_id,destructor_entity_id,initialization_kind,capture_proof FROM throw_expressions ORDER BY node_id",
            "nodeId", "exceptionTypeId", "destructorEntityId", "initializationKind", "captureProof");
        Table("throwObjectAddresses", "SELECT node_id,throw_node_id,capture_proof FROM throw_object_addresses ORDER BY node_id", "nodeId", "throwNodeId", "captureProof");
        Table("throwMarkers", "SELECT node_id,throw_node_id,kind,capture_proof FROM throw_markers ORDER BY node_id", "nodeId", "throwNodeId", "kind", "captureProof");
        Table("rethrowExpressions", "SELECT node_id,capture_proof FROM rethrow_expressions ORDER BY node_id", "nodeId", "captureProof");
        Table("sourceRecordOperations", "SELECT source_record_id,bitwise_construction_allowed,bitwise_assignment_allowed,requires_return_storage,requires_parameter_storage,capture_proof FROM source_record_operations ORDER BY source_record_id",
            "sourceRecordId", "bitwiseConstructionAllowed", "bitwiseAssignmentAllowed", "requiresReturnStorage", "requiresParameterStorage", "captureProof");
        Table("virtualBaseAdjustments", "SELECT node_id,operand_node_id,source_node_id,lookup_source_node_id,derived_type_id,complete_base_type_id,offset_node_id,vtable_index,is_complete_object,capture_proof FROM virtual_base_adjustments ORDER BY node_id",
            "nodeId", "operandNodeId", "sourceNodeId", "lookupSourceNodeId", "derivedTypeId", "completeBaseTypeId", "offsetNodeId", "vtableIndex", "isCompleteObject", "captureProof");
        Table("sourceRecordTemporaries", "SELECT id,function_entity_id,source_initializer_id,source_type_id,source_ordinal,dynamic_init_kind,source_destruction_obligation_id,source_file_id,line,column,capture_proof FROM source_record_temporaries ORDER BY id",
            "id", "functionEntityId", "sourceInitializerId", "sourceTypeId", "sourceOrdinal", "dynamicInitKind", "sourceDestructionObligationId", "sourceFileId", "line", "column", "captureProof");
        Table("recordTemporaryInitializations", "SELECT node_id,binding_id,source_temporary_id,lowered_init_kind,site_kind,zero_storage,partially_initialized,capture_proof FROM record_temporary_initializations ORDER BY node_id",
            "nodeId", "bindingId", "sourceTemporaryId", "loweredInitKind", "siteKind", "zeroStorage", "partiallyInitialized", "captureProof");
        Table("recordStorageInitializations", "SELECT node_id,binding_id,source_declaration_id,source_type_id,source_init_kind,lowered_init_kind,site_kind,zero_storage,partially_initialized,capture_proof FROM record_storage_initializations ORDER BY node_id",
            "nodeId", "bindingId", "sourceDeclarationId", "sourceTypeId", "sourceInitKind", "loweredInitKind", "siteKind", "zeroStorage", "partiallyInitialized", "captureProof");
        Table("recordConstantInitializations", "SELECT node_id,source_record_id,storage_id,initialization_kind,capture_proof FROM record_constant_initializations ORDER BY node_id",
            "nodeId", "sourceRecordId", "storageId", "initializationKind", "captureProof");
        Table("recordValueTransfers", "SELECT node_id,kind,capture_proof FROM record_value_transfers ORDER BY node_id",
            "nodeId", "kind", "captureProof");
        Table("sourceRecordOwners", "SELECT source_record_id,owner_source_record_id,owner_field_id,kind,capture_proof FROM source_record_owners ORDER BY source_record_id", "sourceRecordId", "ownerSourceRecordId", "ownerFieldId", "kind", "captureProof");
        Table("sourceRecords", "SELECT id,source_type_id,identity_key,source_name,record_kind,is_complete,native_size,native_alignment,is_trivially_copyable,capture_phase,source_file_id,line,column,is_packed,max_member_alignment,explicit_alignment,layout_known,is_dependent,is_prototype_instantiation FROM source_record_types ORDER BY id",
            "id", "sourceTypeId", "identityKey", "sourceName", "recordKind", "isComplete", "nativeSize", "nativeAlignment", "isTriviallyCopyable", "capturePhase", "sourceFileId", "line", "column", "isPacked", "maxMemberAlignment", "explicitAlignment", "layoutKnown", "isDependent", "isPrototypeInstantiation");
        Table("sourceFields", "SELECT id,source_record_id,ordinal,source_type_id,source_name,is_mutable,has_no_unique_address,bit_width,source_file_id,line,column,has_initializer,is_packed,explicit_alignment FROM source_record_fields ORDER BY id",
            "id", "sourceRecordId", "ordinal", "sourceTypeId", "sourceName", "isMutable", "hasNoUniqueAddress", "bitWidth", "sourceFileId", "line", "column", "hasInitializer", "isPacked", "explicitAlignment");
        Table("sourceBases", "SELECT id,source_record_id,ordinal,base_source_type_id,access_kind,is_virtual,source_file_id,line,column FROM source_record_bases ORDER BY id",
            "id", "sourceRecordId", "ordinal", "baseSourceTypeId", "accessKind", "isVirtual", "sourceFileId", "line", "column");
        Table("recordBaseSubobjects", "SELECT record_type_id,source_base_id,complete_base_type_id,physical_view_type_id,byte_offset,extent,field_id FROM record_base_subobjects ORDER BY record_type_id,source_base_id",
            "recordTypeId", "sourceBaseId", "completeBaseTypeId", "physicalViewTypeId", "byteOffset", "extent", "fieldId");
        Table("abiRoutineOrigins", "SELECT entity_id,primary_entity_id,entry_kind,is_alias_entry,capture_proof,target_entity_id FROM abi_routine_origins ORDER BY entity_id",
            "entityId", "primaryEntityId", "entryKind", "isAliasEntry", "captureProof", "targetEntityId");
        Table("sourceTypeQualifiers", "SELECT type_id,name FROM source_type_qualifiers ORDER BY type_id,name", "typeId", "name");
        Table("sourceEnumTypes", "SELECT type_id,identity_key,underlying_type_id,is_scoped,is_complete,enumerator_list_complete,has_explicit_base FROM source_enum_types ORDER BY type_id", "typeId", "identityKey", "underlyingTypeId", "isScoped", "isComplete", "enumeratorListComplete", "hasExplicitBase");
        Table("sourceEnumerators", "SELECT type_id,ordinal,source_name,value_text,source_file_id,line,column FROM source_enumerators ORDER BY type_id,ordinal", "typeId", "ordinal", "sourceName", "valueText", "sourceFileId", "line", "column");
        Table("sourceMemberPointerTypes", "SELECT type_id,owner_type_id,member_type_id,capture_proof FROM source_member_pointer_types ORDER BY type_id", "typeId", "ownerTypeId", "memberTypeId", "captureProof");
        Table("sourceTypes", "SELECT id,kind,source_name,underlying_type_id,execution_type_id,is_const,is_volatile,source_file_id,line,column,element_count,is_restrict,is_atomic,array_bound_kind FROM source_types ORDER BY id",
            "id", "kind", "sourceName", "underlyingTypeId", "executionTypeId", "isConst", "isVolatile", "sourceFileId", "line", "column", "elementCount", "isRestrict", "isAtomic", "arrayBoundKind");
        Table("storages", "SELECT id,source_name,linkage,external_key,definition,type_id,source_type_id,is_thread_local,is_const,alignment,source_file_id,line,column FROM storages ORDER BY id",
            "id", "sourceName", "linkage", "externalKey", "definition", "typeId", "sourceTypeId", "isThreadLocal", "isConst", "alignment", "sourceFileId", "line", "column");
        Table("routineTypes", "SELECT type_id,return_type_id,variadic,exception_policy FROM routine_types ORDER BY type_id", "typeId", "returnTypeId", "variadic", "exceptionPolicy");
        Table("routineParameters", "SELECT type_id,ordinal,parameter_type_id FROM routine_parameters ORDER BY type_id,ordinal", "typeId", "ordinal", "parameterTypeId");
        Table("sourceRoutineTypes", "SELECT type_id,return_type_id,variadic,exception_policy,parameter_mode,object_type_id,object_is_const,object_is_volatile,object_ref_qualifier FROM source_routine_types ORDER BY type_id", "typeId", "returnTypeId", "variadic", "exceptionPolicy", "parameterMode", "objectTypeId", "objectIsConst", "objectIsVolatile", "objectRefQualifier");
        Table("sourceRoutineParameters", "SELECT type_id,ordinal,parameter_type_id FROM source_routine_parameters ORDER BY type_id,ordinal", "typeId", "ordinal", "parameterTypeId");
        Table("deferredRoutineAbis", "SELECT routine_type_id,source_routine_type_id,reason,capture_proof FROM deferred_routine_abis ORDER BY routine_type_id,source_routine_type_id", "routineTypeId", "sourceRoutineTypeId", "reason", "captureProof");
        Table("sourceRoutineAbis", "SELECT source_routine_type_id,return_passing,capture_proof FROM source_routine_abis ORDER BY source_routine_type_id", "sourceRoutineTypeId", "returnPassing", "captureProof");
        Table("sourceRoutineAbiParameters", "SELECT source_routine_type_id,execution_ordinal,parameter_role,source_ordinal FROM source_routine_abi_parameters ORDER BY source_routine_type_id,execution_ordinal", "sourceRoutineTypeId", "executionOrdinal", "parameterRole", "sourceOrdinal");
        Table("sourceFunctions", "SELECT entity_id,return_source_type_id,capture_phase,routine_source_type_id,function_kind FROM source_functions ORDER BY entity_id", "entityId", "returnSourceTypeId", "capturePhase", "routineSourceTypeId", "functionKind");
        Table("executionParameters", "SELECT entity_id,ordinal,type_id,parameter_role,source_ordinal FROM parameters ORDER BY entity_id,ordinal", "entityId", "ordinal", "typeId", "parameterRole", "sourceOrdinal");
        Table("sourceParameters", "SELECT entity_id,ordinal,source_type_id,has_default_argument FROM source_parameters ORDER BY entity_id,ordinal", "entityId", "ordinal", "sourceTypeId", "hasDefaultArgument");
        Table("executionBindings", "SELECT id,function_id,kind,type_id,source_name,source_file_id,line,column,is_synthetic FROM bindings ORDER BY id",
            "id", "functionId", "kind", "typeId", "sourceName", "sourceFileId", "line", "column", "isSynthetic");
        Table("sourceDeclarations", "SELECT id,kind,source_name,source_type_id,entity_id,binding_id,function_entity_id,source_file_id,line,column FROM source_declarations ORDER BY id",
            "id", "kind", "sourceName", "sourceTypeId", "entityId", "bindingId", "functionEntityId", "sourceFileId", "line", "column");
        writer.WriteEndObject();
    }
}
