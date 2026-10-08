using System.Text.Json;

namespace DotObj;

internal sealed record SourceType(long Id, string Kind, string SourceName,
    long? UnderlyingTypeId, long ExecutionTypeId, bool IsConst, bool IsVolatile,
    long? SourceFileId, long Line, long Column, long? ElementCount);

internal sealed partial class ObjectFile
{
    public Dictionary<long, SourceType> SourceTypes { get; } = [];

    private void ReadAndVerifySourceProvenance()
    {
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
                        (type.ElementCount == 0 ? !ScalarTypes.IsPointer(executionType) : !ScalarTypes.IsArray(executionType) || ScalarTypes.ElementCount(executionType) != type.ElementCount))
                        throw new InvalidDataException("Invalid source array type or execution extent");
                }
                else if (type.Kind == "pointer")
                {
                    if (type.UnderlyingTypeId is null || !ScalarTypes.IsPointer(executionType) ||
                        type.SourceName.Length != 0 || type.IsConst || type.IsVolatile)
                        throw new InvalidDataException("Invalid source pointer type");
                }
                else if (type.Kind == "record")
                {
                    if (!Records.TryGetValue(type.ExecutionTypeId, out RecordType? record) || type.SourceName != record.SourceName ||
                        type.UnderlyingTypeId is not null || type.IsConst || type.IsVolatile)
                        throw new InvalidDataException("Source record disagrees with its execution representation");
                }
                else
                {
                    string expected = type.Kind switch
                    {
                        "void" => "void", "bool" => "u8", "float" => "f32", "double" => "f64", "signed-char" => "i8", "unsigned-char" => "u8",
                        "short" => "i16", "unsigned-short" => "u16", "int" => "i32", "unsigned-int" => "u32",
                        "long" or "long-long" => "i64", "unsigned-long" or "unsigned-long-long" => "u64",
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
                    string element = current.ElementCount == 0 ? ScalarTypes.Pointee(representation) : ScalarTypes.Element(representation);
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
        foreach (RecordField field in Fields.Values)
        {
            SourceType source = SourceTypeById(field.SourceTypeId);
            if (source.ExecutionTypeId != field.TypeId)
                throw new InvalidDataException("Record field source type disagrees with execution representation");
            // Volatile reads/writes and atomics require explicit semantic nodes.
            while (source.Kind is "typedef" or "qualified" or "array")
            {
                if (source.IsVolatile) throw new InvalidDataException("Volatile record fields are not supported");
                source = SourceTypeById(source.UnderlyingTypeId!.Value);
            }
        }
        var functions = new HashSet<long>();
        using (var query = database.Prepare("SELECT entity_id,return_source_type_id,capture_phase FROM source_functions ORDER BY entity_id"))
            while (query.Read())
            {
                long entity = query.Integer(0);
                if (!Entities.TryGetValue(entity, out Entity? function) || query.Text(2) != "before-lowering" ||
                    SourceTypeById(query.Integer(1)).ExecutionTypeId != function.ReturnTypeId || Original(SourceTypeById(query.Integer(1))).Kind == "array" || !functions.Add(entity))
                    throw new InvalidDataException("Source function signature or capture phase mismatch");
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
        // rows: local declaration metadata need not stay resident after checking.
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
        RejectRows("SELECT b.id FROM bindings b LEFT JOIN source_declarations d ON d.binding_id=b.id WHERE d.id IS NULL", "Missing original binding declaration capture");
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
        return original.Kind == "array" && original.ElementCount > 0
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
                            if (column is "isConst" or "isVolatile") writer.WriteBoolean(column, query.Integer(index) == 1);
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
        Table("sourceFiles", "SELECT id,path,parent_id FROM source_files ORDER BY id", "id", "path", "parentId");
        Table("executionTypes", "SELECT id,kind,pointee_type_id,element_type_id,element_count FROM types ORDER BY id", "id", "kind", "pointeeTypeId", "elementTypeId", "elementCount");
        Table("recordTypes", "SELECT type_id,identity_key,source_name,size,alignment,source_file_id,line,column FROM record_types ORDER BY type_id",
            "typeId", "identityKey", "sourceName", "size", "alignment", "sourceFileId", "line", "column");
        Table("recordFields", "SELECT id,record_type_id,ordinal,type_id,source_name,byte_offset,source_type_id,source_file_id,line,column FROM record_fields ORDER BY id",
            "id", "recordTypeId", "ordinal", "typeId", "sourceName", "byteOffset", "sourceTypeId", "sourceFileId", "line", "column");
        Table("sourceTypes", "SELECT id,kind,source_name,underlying_type_id,execution_type_id,is_const,is_volatile,source_file_id,line,column,element_count FROM source_types ORDER BY id",
            "id", "kind", "sourceName", "underlyingTypeId", "executionTypeId", "isConst", "isVolatile", "sourceFileId", "line", "column", "elementCount");
        Table("sourceFunctions", "SELECT entity_id,return_source_type_id,capture_phase FROM source_functions ORDER BY entity_id", "entityId", "returnSourceTypeId", "capturePhase");
        Table("sourceParameters", "SELECT entity_id,ordinal,source_type_id FROM source_parameters ORDER BY entity_id,ordinal", "entityId", "ordinal", "sourceTypeId");
        Table("sourceDeclarations", "SELECT id,kind,source_name,source_type_id,entity_id,binding_id,function_entity_id,source_file_id,line,column FROM source_declarations ORDER BY id",
            "id", "kind", "sourceName", "sourceTypeId", "entityId", "bindingId", "functionEntityId", "sourceFileId", "line", "column");
        writer.WriteEndObject();
    }
}
