using System.Text.Json;

namespace DotObj;

internal sealed record SourceType(long Id, string Kind, string SourceName,
    long? UnderlyingTypeId, long ExecutionTypeId, bool IsConst, bool IsVolatile,
    long? SourceFileId, long Line, long Column, long? ElementCount);

internal sealed partial class ObjectFile
{
    public Dictionary<long, SourceType> SourceTypes { get; } = [];
    public Dictionary<long, long> BindingSourceTypes { get; } = [];

    private void ReadAndVerifySourceProvenance()
    {
        ReadRoutineMetadata(true);
        using (var query = database.Prepare("SELECT id,kind,source_name,underlying_type_id,execution_type_id,is_const,is_volatile,source_file_id,line,column,element_count FROM source_types ORDER BY id"))
            while (query.Read())
            {
                var type = new SourceType(query.Integer(0), query.Text(1), query.Text(2), query.OptionalInteger(3), query.Integer(4),
                    query.Integer(5) == 1, query.Integer(6) == 1, query.OptionalInteger(7), query.Integer(8), query.Integer(9), query.OptionalInteger(10));
                if (type.Id <= 0 || type.Line < 0 || type.Column < 0 || query.Integer(5) is not (0 or 1) || query.Integer(6) is not (0 or 1))
                    throw new InvalidDataException("Invalid source type identity, qualifiers or location");
                string executionType = Type(type.ExecutionTypeId);
                if ((type.Kind == "array" ? type.ElementCount is null or < 0 : type.ElementCount is not null))
                    throw new InvalidDataException("Source array requires a nonnegative element count");
                if (type.Kind is "typedef" or "qualified")
                {
                    if (type.UnderlyingTypeId is null || (type.Kind == "typedef" ? type.SourceName.Length == 0 : type.SourceName.Length != 0 || !type.IsConst && !type.IsVolatile))
                        throw new InvalidDataException("Invalid source typedef or qualified type");
                }
                else if (type.Kind == "array")
                {
                    if (type.UnderlyingTypeId is null || type.SourceName.Length != 0 || type.IsConst || type.IsVolatile ||
                        (!ScalarTypes.IsArray(executionType) || ScalarTypes.ElementCount(executionType) != type.ElementCount))
                        throw new InvalidDataException("Invalid source array type or execution extent");
                }
                else if (type.Kind == "pointer")
                {
                    if (type.UnderlyingTypeId is null || !ScalarTypes.IsPointer(executionType) ||
                        type.SourceName.Length != 0 || type.IsConst || type.IsVolatile)
                        throw new InvalidDataException("Invalid source pointer type");
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
                if (current.Kind == "pointer")
                {
                    if (Original(target).Kind == "array" && Original(target).ElementCount == 0 ||
                        ScalarTypes.Pointee(Type(current.ExecutionTypeId)) != Type(target.ExecutionTypeId))
                        throw new InvalidDataException("Source pointer pointee disagrees with execution representation");
                }
                else if (current.Kind == "array")
                {
                    string representation = Type(current.ExecutionTypeId);
                    string element = ScalarTypes.Element(representation);
                    if (element != Type(target.ExecutionTypeId)) throw new InvalidDataException("Source array element disagrees with execution representation");
                    if (Original(target).Kind == "array" && Original(target).ElementCount == 0)
                        throw new InvalidDataException("Array element cannot be incomplete");
                }
                else if (current.ExecutionTypeId != target.ExecutionTypeId)
                    throw new InvalidDataException("Source typedef/qualification changes the execution representation");
                current = target;
            }
            visited.UnionWith(chain);
        }
        foreach (RoutineType routine in SourceRoutines.Values)
        {
            SourceType type = SourceTypeById(routine.TypeId);
            string signature = RoutineTypes.Key(Type(SourceTypeById(routine.ReturnTypeId).ExecutionTypeId),
                routine.Parameters.Select(parameter => ParameterType(SourceTypeById(parameter))), routine.Variadic, routine.ExceptionPolicy);
            if (type.Kind != "routine" || Type(type.ExecutionTypeId) != signature)
                throw new InvalidDataException("Source routine signature disagrees with execution signature");
        }
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
        using (var query = database.Prepare("SELECT entity_id,return_source_type_id,capture_phase,routine_source_type_id FROM source_functions ORDER BY entity_id"))
            while (query.Read())
            {
                long entity = query.Integer(0);
                if (!Entities.TryGetValue(entity, out Entity? function) || query.Text(2) != "before-lowering" ||
                    SourceTypeById(query.Integer(1)).ExecutionTypeId != function.ReturnTypeId || Original(SourceTypeById(query.Integer(1))).Kind == "array" || !functions.Add(entity))
                    throw new InvalidDataException("Source function signature or capture phase mismatch");
                SourceType signatureType = Original(SourceTypeById(query.Integer(3)));
                if (signatureType.Kind != "routine" || !SourceRoutines.TryGetValue(signatureType.Id, out RoutineType? sourceSignature) ||
                    Type(signatureType.ExecutionTypeId) != EntityRoutine(entity) || sourceSignature.ReturnTypeId != query.Integer(1))
                    throw new InvalidDataException("Original full function signature disagrees with function summary");
                if (sourceSignature.ParameterMode == "unspecified" && (function!.Definition == "declaration" || Parameters.GetValueOrDefault(entity, []).Count != 0))
                    throw new InvalidDataException("Unspecified source parameter mode requires a known zero-parameter definition");
            }
        if (functions.Count != Entities.Count) throw new InvalidDataException("Missing original function signature capture");
        var parameterCounts = new Dictionary<long, int>();
        using (var query = database.Prepare("SELECT entity_id,ordinal,source_type_id FROM source_parameters ORDER BY entity_id,ordinal"))
            while (query.Read())
            {
                long entity = query.Integer(0), ordinal = query.Integer(1);
                int count = parameterCounts.GetValueOrDefault(entity);
                SourceType sourceType = SourceTypeById(query.Integer(2));
                if (!Entities.ContainsKey(entity) || ordinal != count ||
                    !Parameters.TryGetValue(entity, out var parameters) || count >= parameters.Count ||
                    ParameterType(sourceType) != parameters[count])
                    throw new InvalidDataException("Source parameter signature mismatch");
                parameterCounts[entity] = count + 1;
            }
        foreach (Entity entity in Entities.Values)
            if (parameterCounts.GetValueOrDefault(entity.Id) != Parameters.GetValueOrDefault(entity.Id, []).Count)
                throw new InvalidDataException("Missing original parameter signature capture");

        // Join declarations to their owned execution counterpart while streaming
        // rows. Retain the type IDs to verify declared volatile access sites.
        using (var query = database.Prepare("""
            SELECT d.id,d.kind,d.source_name,d.source_type_id,d.entity_id,d.binding_id,d.function_entity_id,d.line,d.column,
                   b.function_id,b.kind,b.ordinal,b.type_id,b.source_name,
                   f.return_source_type_id,p.source_type_id
            FROM source_declarations d
            LEFT JOIN bindings b ON b.id=d.binding_id
            LEFT JOIN source_functions f ON f.entity_id=d.entity_id
            LEFT JOIN source_parameters p ON p.entity_id=b.function_id AND p.ordinal=b.ordinal
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
                        query.OptionalInteger(14) != sourceTypeId || type.ExecutionTypeId != function.ReturnTypeId)
                        throw new InvalidDataException("Source function declaration does not match its execution entity");
                }
                else if (kind is "parameter" or "local")
                {
                    if (!query.IsNull(4) || query.IsNull(5) || query.OptionalInteger(6) is not long function ||
                        !Functions.ContainsKey(function) || query.OptionalInteger(9) != function || query.OptionalText(10) != kind ||
                        (kind == "parameter" ? ParameterType(type) != Type(query.Integer(12)) : type.ExecutionTypeId != query.OptionalInteger(12)) || query.OptionalText(13) != name)
                        throw new InvalidDataException("Source binding declaration does not match its execution binding");
                    if (!BindingSourceTypes.TryAdd(query.Integer(5), sourceTypeId)) throw new InvalidDataException("Duplicate source binding declaration identity");
                    if (kind == "local" && Original(type).Kind == "array" && Original(type).ElementCount == 0)
                        throw new InvalidDataException("Incomplete array is only supported as original parameter provenance");
                    // A canonical routine signature can come from an earlier
                    // prototype using a different typedef or top-level cv spelling.
                    // The definition's declared parameter keeps its own source type.
                    if (kind == "parameter" && (query.OptionalInteger(15) is not long signatureType ||
                        ParameterType(SourceTypeById(signatureType)) != ParameterType(type)))
                        throw new InvalidDataException("Source parameter declaration does not match its signature representation");
                }
                else throw new InvalidDataException("Unsupported source declaration kind");
            }
        RejectRows("SELECT entity_id FROM source_declarations WHERE entity_id IS NOT NULL GROUP BY entity_id HAVING count(*)<>1", "Duplicate source function declaration identity");
        RejectRows("SELECT binding_id FROM source_declarations WHERE binding_id IS NOT NULL GROUP BY binding_id HAVING count(*)<>1", "Duplicate source binding declaration identity");
        RejectRows("SELECT e.id FROM entities e LEFT JOIN source_declarations d ON d.entity_id=e.id WHERE d.id IS NULL", "Missing original function declaration capture");
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

    private string SourceTypeSignature(long id, int depth, bool inheritedConst, bool inheritedVolatile)
    {
        if (depth > 128) throw new InvalidDataException("Source field type nesting exceeds supported limit");
        SourceType type = SourceTypeById(id);
        bool isConst = type.IsConst || inheritedConst, isVolatile = type.IsVolatile || inheritedVolatile;
        while (type.Kind is "typedef" or "qualified")
        {
            if (++depth > 128) throw new InvalidDataException("Source field type nesting exceeds supported limit");
            type = SourceTypeById(type.UnderlyingTypeId!.Value);
            isConst |= type.IsConst;
            isVolatile |= type.IsVolatile;
        }
        // C's qualified array typedef and a directly qualified array element
        // describe the same field type. Move array qualification to its element.
        if (type.Kind == "array") return "array(" + type.ElementCount + "," +
            SourceTypeSignature(type.UnderlyingTypeId!.Value, depth + 1, isConst, isVolatile) + ")";
        string qualifiers = (isConst ? "const:" : "") + (isVolatile ? "volatile:" : "");
        return qualifiers + (type.Kind switch
        {
            "pointer" => "ptr(" + SourceTypeSignature(type.UnderlyingTypeId!.Value, depth + 1, false, false) + ")",
            "record" => Type(type.ExecutionTypeId),
            "routine" => Type(type.ExecutionTypeId),
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
                            if (column is "isConst" or "isVolatile" or "isSynthetic" or "isComplete" or "isTriviallyCopyable" or "isMutable" or "hasNoUniqueAddress") writer.WriteBoolean(column, query.Integer(index) == 1);
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
        Table("initializationActions", "SELECT id,entity_id,phase,priority,ordinal,source_file_id,line,column FROM initialization_actions ORDER BY ordinal",
            "id", "entityId", "phase", "priority", "ordinal", "sourceFileId", "line", "column");
        Table("sourceFiles", "SELECT id,path,parent_id FROM source_files ORDER BY id", "id", "path", "parentId");
        Table("executionTypes", "SELECT id,kind,pointee_type_id,element_type_id,element_count FROM types ORDER BY id", "id", "kind", "pointeeTypeId", "elementTypeId", "elementCount");
        Table("recordTypes", "SELECT type_id,identity_key,source_name,size,alignment,source_file_id,line,column,layout_kind,source_record_id,representation_kind,value_copy_kind,capture_proof FROM record_types ORDER BY type_id",
            "typeId", "identityKey", "sourceName", "size", "alignment", "sourceFileId", "line", "column", "layoutKind", "sourceRecordId", "representationKind", "valueCopyKind", "captureProof");
        Table("recordFields", "SELECT id,record_type_id,ordinal,type_id,source_name,byte_offset,source_type_id,source_file_id,line,column,bit_offset,bit_width,bit_signed,source_field_id,field_role FROM record_fields ORDER BY id",
            "id", "recordTypeId", "ordinal", "typeId", "sourceName", "byteOffset", "sourceTypeId", "sourceFileId", "line", "column", "bitOffset", "bitWidth", "bitSigned", "sourceFieldId", "fieldRole");
        Table("sourceRecords", "SELECT id,source_type_id,identity_key,source_name,record_kind,is_complete,native_size,native_alignment,is_trivially_copyable,capture_phase,source_file_id,line,column FROM source_record_types ORDER BY id",
            "id", "sourceTypeId", "identityKey", "sourceName", "recordKind", "isComplete", "nativeSize", "nativeAlignment", "isTriviallyCopyable", "capturePhase", "sourceFileId", "line", "column");
        Table("sourceFields", "SELECT id,source_record_id,ordinal,source_type_id,source_name,is_mutable,has_no_unique_address,bit_width,source_file_id,line,column FROM source_record_fields ORDER BY id",
            "id", "sourceRecordId", "ordinal", "sourceTypeId", "sourceName", "isMutable", "hasNoUniqueAddress", "bitWidth", "sourceFileId", "line", "column");
        Table("sourceTypes", "SELECT id,kind,source_name,underlying_type_id,execution_type_id,is_const,is_volatile,source_file_id,line,column,element_count FROM source_types ORDER BY id",
            "id", "kind", "sourceName", "underlyingTypeId", "executionTypeId", "isConst", "isVolatile", "sourceFileId", "line", "column", "elementCount");
        Table("storages", "SELECT id,source_name,linkage,external_key,definition,type_id,source_type_id,is_thread_local,is_const,alignment,source_file_id,line,column FROM storages ORDER BY id",
            "id", "sourceName", "linkage", "externalKey", "definition", "typeId", "sourceTypeId", "isThreadLocal", "isConst", "alignment", "sourceFileId", "line", "column");
        Table("routineTypes", "SELECT type_id,return_type_id,variadic,exception_policy FROM routine_types ORDER BY type_id", "typeId", "returnTypeId", "variadic", "exceptionPolicy");
        Table("routineParameters", "SELECT type_id,ordinal,parameter_type_id FROM routine_parameters ORDER BY type_id,ordinal", "typeId", "ordinal", "parameterTypeId");
        Table("sourceRoutineTypes", "SELECT type_id,return_type_id,variadic,exception_policy,parameter_mode FROM source_routine_types ORDER BY type_id", "typeId", "returnTypeId", "variadic", "exceptionPolicy", "parameterMode");
        Table("sourceRoutineParameters", "SELECT type_id,ordinal,parameter_type_id FROM source_routine_parameters ORDER BY type_id,ordinal", "typeId", "ordinal", "parameterTypeId");
        Table("sourceFunctions", "SELECT entity_id,return_source_type_id,capture_phase,routine_source_type_id FROM source_functions ORDER BY entity_id", "entityId", "returnSourceTypeId", "capturePhase", "routineSourceTypeId");
        Table("sourceParameters", "SELECT entity_id,ordinal,source_type_id FROM source_parameters ORDER BY entity_id,ordinal", "entityId", "ordinal", "sourceTypeId");
        Table("executionBindings", "SELECT id,function_id,kind,type_id,source_name,source_file_id,line,column,is_synthetic FROM bindings ORDER BY id",
            "id", "functionId", "kind", "typeId", "sourceName", "sourceFileId", "line", "column", "isSynthetic");
        Table("sourceDeclarations", "SELECT id,kind,source_name,source_type_id,entity_id,binding_id,function_entity_id,source_file_id,line,column FROM source_declarations ORDER BY id",
            "id", "kind", "sourceName", "sourceTypeId", "entityId", "bindingId", "functionEntityId", "sourceFileId", "line", "column");
        writer.WriteEndObject();
    }
}
