using System.Text.Json;

namespace DotObj;

internal sealed record SourceType(long Id, string Kind, string SourceName,
    long? UnderlyingTypeId, long ExecutionTypeId, bool IsConst, bool IsVolatile,
    long? SourceFileId, long Line, long Column, long? ElementCount);

internal sealed partial class ObjectFile
{
    public Dictionary<long, SourceType> SourceTypes { get; } = [];
    public Dictionary<long, long> BindingSourceTypes { get; } = [];
    public Dictionary<(long EntityId, long Ordinal), bool> SourceParameterDefaults { get; } = [];

    private void ReadAndVerifySourceProvenance()
    {
        ReadRoutineMetadata(true);
        ReadAbiRoutineOrigins();
        using (var query = database.Prepare("SELECT id,kind,source_name,underlying_type_id,execution_type_id,is_const,is_volatile,source_file_id,line,column,element_count FROM source_types ORDER BY id"))
            while (query.Read())
            {
                var type = new SourceType(query.Integer(0), query.Text(1), query.Text(2), query.OptionalInteger(3), query.Integer(4),
                    query.Integer(5) == 1, query.Integer(6) == 1, query.OptionalInteger(7), query.Integer(8), query.Integer(9), query.OptionalInteger(10));
                if (type.Id <= 0 || type.Line < 0 || type.Column < 0 || query.Integer(5) is not (0 or 1) || query.Integer(6) is not (0 or 1))
                    throw new InvalidDataException("Invalid source type identity, qualifiers or location");
                string executionType = Type(type.ExecutionTypeId);
                if ((type.Kind == "array" ? type.ElementCount is < 0 : type.ElementCount is not null))
                    throw new InvalidDataException("Source array requires a nonnegative element count");
                if (type.Kind is "typedef" or "qualified")
                {
                    if (type.UnderlyingTypeId is null || (type.Kind == "typedef" ? type.SourceName.Length == 0 : type.SourceName.Length != 0 || !type.IsConst && !type.IsVolatile))
                        throw new InvalidDataException("Invalid source typedef or qualified type");
                }
                else if (type.Kind == "array")
                {
                    if (type.UnderlyingTypeId is null || type.SourceName.Length != 0 || type.IsConst || type.IsVolatile ||
                        (!ScalarTypes.IsArray(executionType) || ScalarTypes.ArrayBound(executionType) != type.ElementCount))
                        throw new InvalidDataException("Invalid source array type or execution extent");
                }
                else if (type.Kind is "pointer" or "lvalue-reference" or "rvalue-reference")
                {
                    if (type.UnderlyingTypeId is null || !ScalarTypes.IsPointer(executionType) ||
                        type.SourceName.Length != 0 || type.IsConst || type.IsVolatile)
                        throw new InvalidDataException("Invalid source pointer or reference type");
                }
                else if (type.Kind == "routine")
                {
                    if (!SourceRoutines.ContainsKey(type.Id) || !RoutineTypes.IsRoutine(executionType) || type.SourceName.Length != 0 ||
                        type.UnderlyingTypeId is not null || type.IsConst || type.IsVolatile)
                        throw new InvalidDataException("Invalid source routine type");
                }
                else if (type.Kind == "enum")
                {
                    if (!ScalarTypes.IsInteger(executionType) || type.UnderlyingTypeId is not null || type.IsConst || type.IsVolatile)
                        throw new InvalidDataException("Invalid source enum representation");
                }
                else if (type.Kind == "record")
                {
                    if (!Records.TryGetValue(type.ExecutionTypeId, out RecordType? record) || record.SourceRecordId is not long sourceRecordId ||
                        !SourceRecords.TryGetValue(sourceRecordId, out SourceRecord? original) || original.SourceTypeId != type.Id || type.SourceName != record.SourceName ||
                        type.UnderlyingTypeId is not null || type.IsConst || type.IsVolatile)
                        throw new InvalidDataException("Source record disagrees with its execution representation");
                }
                else
                {
                    string expected = type.Kind switch
                    {
                        "long-double" or "float80" or "float64x" => "f80", "va-list" => "va-list", "void" => "void", "bool" => "u8", "float" => "f32", "double" => "f64", "signed-char" => "i8", "unsigned-char" => "u8",
                        "short" => "i16", "unsigned-short" => "u16", "int" => "i32", "unsigned-int" => "u32",
                        "int128" => "i128", "unsigned-int128" => "u128", "long" or "long-long" => "i64", "unsigned-long" or "unsigned-long-long" => "u64",
                        "char" when executionType is "i8" or "u8" => executionType,
                        _ => throw new InvalidDataException("Unsupported original source scalar type")
                    };
                    if (executionType != expected || type.UnderlyingTypeId is not null || type.SourceName.Length != 0 || type.IsConst || type.IsVolatile)
                        throw new InvalidDataException("Source builtin type disagrees with its execution representation");
                }
                SourceTypes.Add(type.Id, type);
            }
        // Walk each alias/qualification chain once, without recursion or reliance
        // on physical row order. Original long and long long remain distinct even
        // though both eventually name the same execution representation.
        var visited = new HashSet<long>();
        foreach (SourceType start in SourceTypes.Values)
        {
            var chain = new HashSet<long>();
            SourceType current = start;
            while (!visited.Contains(current.Id))
            {
                if (!chain.Add(current.Id)) throw new InvalidDataException("Cycle in source type references");
                if (current.UnderlyingTypeId is not long underlying) break;
                if (!SourceTypes.TryGetValue(underlying, out SourceType? target)) throw new InvalidDataException("Unknown underlying source type");
                if (current.Kind is "pointer" or "lvalue-reference" or "rvalue-reference")
                {
                    if (Original(target).Kind is "lvalue-reference" or "rvalue-reference" ||
                        current.Kind != "pointer" && Original(target).Kind == "void" ||
                                                ScalarTypes.Pointee(Type(current.ExecutionTypeId)) != Type(target.ExecutionTypeId))
                        throw new InvalidDataException("Source pointer pointee disagrees with execution representation");
                }
                else if (current.Kind == "array")
                {
                    string representation = Type(current.ExecutionTypeId);
                    string element = ScalarTypes.Element(representation);
                    if (element != Type(target.ExecutionTypeId)) throw new InvalidDataException("Source array element disagrees with execution representation");
                    if (Original(target).Kind == "array" && Original(target).ElementCount is null)
                        throw new InvalidDataException("Array element cannot be incomplete");
                }
                else if (current.ExecutionTypeId != target.ExecutionTypeId)
                    throw new InvalidDataException("Source typedef/qualification changes the execution representation");
                current = target;
            }
            visited.UnionWith(chain);
        }
        ReadAndVerifySourceRoutineAbis();
        foreach (RoutineType routine in Routines.Values)
            if (!CanCopyRecord(Type(routine.ReturnTypeId)) || routine.Parameters.Any(parameter => !CanCopyRecord(Type(parameter))))
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
                    throw new InvalidDataException("Original full function signature disagrees with function summary");
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
        if (functions.Count + AbiRoutineOrigins.Count != Entities.Count) throw new InvalidDataException("Missing original or ABI function origin");
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
        RejectRows("SELECT e.id FROM entities e LEFT JOIN source_declarations d ON d.entity_id=e.id WHERE d.id IS NULL AND e.id NOT IN (SELECT entity_id FROM abi_routine_origins)", "Missing original function declaration capture");
        RejectRows("SELECT b.id FROM bindings b LEFT JOIN parameters p ON p.entity_id=b.function_id AND p.ordinal=b.ordinal WHERE b.kind='parameter' AND (p.parameter_role IS NULL OR b.is_synthetic <> (p.parameter_role IN ('this','result-buffer','forwarded-source')))", "Implicit ABI parameters must be synthetic and source parameters must be original");
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
        ? "abi-entry(" + origin.EntryKind + ";" + SourceFunctionKinds[origin.PrimaryEntityId] + ";" + (origin.EntryKind == "constructor-default-adapter" ? "prefix=" + (Parameters.GetValueOrDefault(entityId, []).Count - 1) + ";" : "") + SourceRoutineSignature(EntitySourceRoutines[origin.PrimaryEntityId], 0) + ")"
        : SourceRoutineSignature(EntitySourceRoutines[entityId], 0);

    private string SourceRoutineSignature(RoutineType routine, int depth)
    {
        if (depth > 128) throw new InvalidDataException("Source routine type nesting exceeds supported limit");
        // This profile admits unspecified C definitions only with known zero
        // parameters. They are compatible with an explicit (void) prototype.
        string receiver = routine.ObjectTypeId is long objectType
            ? SourceTypeSignature(objectType, depth + 1, routine.ObjectIsConst, routine.ObjectIsVolatile) + ":" + routine.ObjectRefQualifier : "none";
        return "source-routine(" + SourceTypeSignature(routine.ReturnTypeId, depth + 1, false, false) + ";" +
            string.Join(',', routine.Parameters.Select(parameter => SourceParameterSignature(parameter, depth + 1))) + ";" +
            (routine.Variadic ? "1" : "0") + ";" + routine.ExceptionPolicy + ";" + receiver + ";abi=" +
            SourceRoutineAbis[routine.TypeId].ReturnPassing + ":" + string.Join(',', RoutineAbiParameters(routine.TypeId).Select(slot =>
                slot.ParameterRole + (slot.SourceOrdinal is long ordinal ? "#" + ordinal : ""))) + ")";
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
        while (type.Kind is "typedef" or "qualified")
        {
            if (++depth > 128) throw new InvalidDataException("Source field type nesting exceeds supported limit");
            type = SourceTypeById(type.UnderlyingTypeId!.Value);
            isConst |= type.IsConst; isVolatile |= type.IsVolatile;
        }
        if (type.Kind == "array") return "array(" + (type.ElementCount?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "?") + "," +
            SourceTypeSignature(type.UnderlyingTypeId!.Value, depth + 1, isConst, isVolatile) + ")";
        // References themselves cannot be cv-qualified; qualifiers belong to
        // their referent and are retained inside the distinct lref/rref node.
        if (type.Kind is "lvalue-reference" or "rvalue-reference")
            return (type.Kind == "lvalue-reference" ? "lref(" : "rref(") + SourceTypeSignature(type.UnderlyingTypeId!.Value, depth + 1, false, false) + ")";
        string qualifiers = (isConst ? "const:" : "") + (isVolatile ? "volatile:" : "");
        return qualifiers + (type.Kind switch
        {
            "pointer" => "ptr(" + SourceTypeSignature(type.UnderlyingTypeId!.Value, depth + 1, false, false) + ")",
            "record" => Type(type.ExecutionTypeId),
            "routine" => SourceRoutineSignature(SourceRoutines[type.Id], depth + 1),
            "enum" => "enum(" + type.SourceName + "," + Type(type.ExecutionTypeId) + ")",
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
                            if (column is "hasVolatileKeyword" or "hasInlineKeyword" or "clobbersMemory" or "hasDefaultArgument" or "isAliasEntry" or "isVirtual" or "isConst" or "isVolatile" or "isSynthetic" or "isComplete" or "isTriviallyCopyable" or "isMutable" or "hasNoUniqueAddress" or "hasInitializer" or "objectIsConst" or "objectIsVolatile") writer.WriteBoolean(column, query.Integer(index) == 1);
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
        Table("memoryBarriers", "SELECT node_id,is_volatile,has_volatile_keyword,has_inline_keyword,clobbers_memory,capture_proof FROM memory_barriers ORDER BY node_id",
            "nodeId", "isVolatile", "hasVolatileKeyword", "hasInlineKeyword", "clobbersMemory", "captureProof");
        Table("memoryBarrierOperands", "SELECT node_id,ordinal,direction,constraint_text,source_name,source_element_type_id,element_size,source_shape,extent_kind,fixed_element_count,extent_binding_id,capture_proof,source_file_id,line,column FROM memory_barrier_operands ORDER BY node_id,ordinal",
            "nodeId", "ordinal", "direction", "constraintText", "sourceName", "sourceElementTypeId", "elementSize", "sourceShape", "extentKind", "fixedElementCount", "extentBindingId", "captureProof", "sourceFileId", "line", "column");
        Table("memoryBarrierDimensions", "SELECT node_id,operand_ordinal,ordinal,kind,fixed_count,extent_binding_id,source_file_id,line,column FROM memory_barrier_dimensions ORDER BY node_id,operand_ordinal,ordinal",
            "nodeId", "operandOrdinal", "ordinal", "kind", "fixedCount", "extentBindingId", "sourceFileId", "line", "column");
        Table("initializationActions", "SELECT id,entity_id,phase,priority,ordinal,source_file_id,line,column FROM initialization_actions ORDER BY ordinal",
            "id", "entityId", "phase", "priority", "ordinal", "sourceFileId", "line", "column");
        Table("sourceFiles", "SELECT id,path,parent_id FROM source_files ORDER BY id", "id", "path", "parentId");
        Table("executionTypes", "SELECT id,kind,pointee_type_id,element_type_id,element_count FROM types ORDER BY id", "id", "kind", "pointeeTypeId", "elementTypeId", "elementCount");
        Table("recordTypes", "SELECT type_id,identity_key,source_name,size,alignment,source_file_id,line,column,layout_kind,source_record_id,representation_kind,value_copy_kind,capture_proof,complete_type_id FROM record_types ORDER BY type_id",
            "typeId", "identityKey", "sourceName", "size", "alignment", "sourceFileId", "line", "column", "layoutKind", "sourceRecordId", "representationKind", "valueCopyKind", "captureProof", "completeTypeId");
        Table("recordFields", "SELECT id,record_type_id,ordinal,type_id,source_name,byte_offset,source_type_id,source_file_id,line,column,bit_offset,bit_width,bit_signed,source_field_id,field_role,source_base_id,observed_alignment FROM record_fields ORDER BY id",
            "id", "recordTypeId", "ordinal", "typeId", "sourceName", "byteOffset", "sourceTypeId", "sourceFileId", "line", "column", "bitOffset", "bitWidth", "bitSigned", "sourceFieldId", "fieldRole", "sourceBaseId", "observedAlignment");
        Table("sourceRecords", "SELECT id,source_type_id,identity_key,source_name,record_kind,is_complete,native_size,native_alignment,is_trivially_copyable,capture_phase,source_file_id,line,column,is_packed,max_member_alignment,explicit_alignment FROM source_record_types ORDER BY id",
            "id", "sourceTypeId", "identityKey", "sourceName", "recordKind", "isComplete", "nativeSize", "nativeAlignment", "isTriviallyCopyable", "capturePhase", "sourceFileId", "line", "column", "isPacked", "maxMemberAlignment", "explicitAlignment");
        Table("sourceFields", "SELECT id,source_record_id,ordinal,source_type_id,source_name,is_mutable,has_no_unique_address,bit_width,source_file_id,line,column,has_initializer,is_packed,explicit_alignment FROM source_record_fields ORDER BY id",
            "id", "sourceRecordId", "ordinal", "sourceTypeId", "sourceName", "isMutable", "hasNoUniqueAddress", "bitWidth", "sourceFileId", "line", "column", "hasInitializer", "isPacked", "explicitAlignment");
        Table("sourceBases", "SELECT id,source_record_id,ordinal,base_source_type_id,access_kind,is_virtual,source_file_id,line,column FROM source_record_bases ORDER BY id",
            "id", "sourceRecordId", "ordinal", "baseSourceTypeId", "accessKind", "isVirtual", "sourceFileId", "line", "column");
        Table("recordBaseSubobjects", "SELECT record_type_id,source_base_id,complete_base_type_id,physical_view_type_id,byte_offset,extent,field_id FROM record_base_subobjects ORDER BY record_type_id,source_base_id",
            "recordTypeId", "sourceBaseId", "completeBaseTypeId", "physicalViewTypeId", "byteOffset", "extent", "fieldId");
        Table("abiRoutineOrigins", "SELECT entity_id,primary_entity_id,entry_kind,is_alias_entry,capture_proof,target_entity_id FROM abi_routine_origins ORDER BY entity_id",
            "entityId", "primaryEntityId", "entryKind", "isAliasEntry", "captureProof", "targetEntityId");
        Table("sourceTypes", "SELECT id,kind,source_name,underlying_type_id,execution_type_id,is_const,is_volatile,source_file_id,line,column,element_count FROM source_types ORDER BY id",
            "id", "kind", "sourceName", "underlyingTypeId", "executionTypeId", "isConst", "isVolatile", "sourceFileId", "line", "column", "elementCount");
        Table("storages", "SELECT id,source_name,linkage,external_key,definition,type_id,source_type_id,is_thread_local,is_const,alignment,source_file_id,line,column FROM storages ORDER BY id",
            "id", "sourceName", "linkage", "externalKey", "definition", "typeId", "sourceTypeId", "isThreadLocal", "isConst", "alignment", "sourceFileId", "line", "column");
        Table("routineTypes", "SELECT type_id,return_type_id,variadic,exception_policy FROM routine_types ORDER BY type_id", "typeId", "returnTypeId", "variadic", "exceptionPolicy");
        Table("routineParameters", "SELECT type_id,ordinal,parameter_type_id FROM routine_parameters ORDER BY type_id,ordinal", "typeId", "ordinal", "parameterTypeId");
        Table("sourceRoutineTypes", "SELECT type_id,return_type_id,variadic,exception_policy,parameter_mode,object_type_id,object_is_const,object_is_volatile,object_ref_qualifier FROM source_routine_types ORDER BY type_id", "typeId", "returnTypeId", "variadic", "exceptionPolicy", "parameterMode", "objectTypeId", "objectIsConst", "objectIsVolatile", "objectRefQualifier");
        Table("sourceRoutineParameters", "SELECT type_id,ordinal,parameter_type_id FROM source_routine_parameters ORDER BY type_id,ordinal", "typeId", "ordinal", "parameterTypeId");
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
