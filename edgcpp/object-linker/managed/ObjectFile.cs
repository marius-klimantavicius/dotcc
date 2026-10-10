using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace DotObj;

internal sealed partial class ObjectFile : IDisposable
{
    private readonly Sqlite database;
    public string Path { get; }
    public string UnitId { get; }
    public string PrimarySource { get; }
    public string SourceLanguage { get; }
    public string Digest { get; }
    public Dictionary<long, string> Types { get; } = [];
    public Dictionary<long, Entity> Entities { get; } = [];
    public Dictionary<long, List<string>> Parameters { get; } = [];
    public Dictionary<long, long> Functions { get; } = [];

    private static readonly Dictionary<string, string> Tables = new(StringComparer.Ordinal)
    {
        ["exception_types"] = "id", ["exception_base_conversions"] = "exception_type_id,ordinal",
        ["throw_expressions"] = "node_id", ["throw_object_addresses"] = "node_id",
        ["throw_initialization_boundaries"] = "throw_node_id", ["throw_cleanup_transitions"] = "node_id",
        ["cleanup_payload_targets"] = "node_id",
        ["caught_object_addresses"] = "node_id",
        ["source_tries"] = "node_id", ["source_handlers"] = "node_id", ["handler_markers"] = "node_id",
        ["throw_payload_subobjects"] = "source_obligation_id", ["throw_payload_paths"] = "source_obligation_id,ordinal",
        ["throw_markers"] = "node_id", ["rethrow_expressions"] = "node_id",
        ["virtual_base_expressions"] = "node_id", ["virtual_base_subobjects"] = "record_type_id,complete_base_type_id",
        ["compile_time_selections"] = "node_id", ["thunk_wrappers"] = "entity_id", ["wrapper_calls"] = "node_id",
        ["tls_routine_origins"] = "entity_id",
        ["lambda_entry_origins"] = "entity_id", ["lambda_entry_calls"] = "node_id",
        ["source_allocation_obligations"] = "id", ["internal_try_expressions"] = "node_id", ["normal_destructions"] = "node_id", ["expression_lifetimes"] = "node_id", ["base_subobject_expressions"] = "node_id", ["source_initializers"] = "id",
        ["source_named_templates"] = "template_id", ["source_alias_instances"] = "type_id",
        ["source_alias_arguments"] = "type_id,list_kind,ordinal", ["source_type_operators"] = "type_id",
        ["source_templates"] = "id", ["source_template_parameters"] = "source_template_id,ordinal",
        ["source_record_instantiations"] = "source_record_id", ["source_template_arguments"] = "source_record_id,ordinal",
        ["source_array_bounds"] = "type_id", ["source_semantic_constants"] = "id",
        ["source_semantic_constant_operands"] = "constant_id,ordinal",
        ["source_field_initializer_destructions"] = "source_initializer_id",
        ["field_initializer_executions"] = "source_initializer_id,function_entity_id",
        ["field_initializer_destruction_elisions"] = "source_initializer_id,function_entity_id",
        ["field_initializer_partial_destruction_executions"] = "source_static_obligation_id,function_entity_id",
        ["source_static_lifetimes"] = "id",
        ["source_static_destructions"] = "id",
        ["initializer_lifetime_executions"] = "source_lifetime_id,function_entity_id",
        ["static_destruction_registrations"] = "node_id",
        ["generated_static_destructors"] = "function_entity_id",
        ["default_argument_evaluations"] = "function_entity_id",
        ["default_argument_lifetime_executions"] = "function_entity_id,source_lifetime_id",
        ["default_argument_obligation_executions"] = "function_entity_id,source_lifetime_id,copied_ordinal",
        ["default_argument_temporary_executions"] = "function_entity_id,source_temporary_id",
        ["generated_array_initializers"] = "function_entity_id",
        ["static_destruction_dispositions"] = "source_obligation_id",
        ["source_concepts"] = "id",
        ["source_concept_parameters"] = "concept_id,ordinal",
        ["source_expression_temporaries"] = "expression_id", ["source_expression_temporary_flags"] = "expression_id,flag",
        ["source_expression_field_references"] = "expression_id", ["source_expression_folds"] = "expression_id", ["source_expression_allocations"] = "expression_id", ["source_declaration_operators"] = "declaration_id",
        ["source_expressions"] = "id",
        ["source_expression_edges"] = "expression_id,role,ordinal",
        ["source_type_constraints"] = "source_type_id",
        ["source_concept_uses"] = "expression_id",
        ["source_concept_arguments"] = "expression_id,ordinal",
        ["source_expression_parameters"] = "expression_id,ordinal",
        ["source_expression_parameter_references"] = "expression_id",
        ["source_expression_operations"] = "expression_id",
        ["source_expression_flags"] = "expression_id,flag",
        ["source_expression_type_operands"] = "expression_id",
        ["source_expression_constants"] = "expression_id",
        ["source_expression_bindings"] = "expression_id",
        ["source_routine_prototype_origins"] = "routine_source_type_id",
        ["source_parameter_binders"] = "id",
        ["source_binder_parameters"] = "binder_id,ordinal",
        ["source_expression_requirements"] = "expression_id",
        ["source_expression_queries"] = "expression_id",
        ["source_expression_substitutions"] = "expression_id,substitution_ordinal",
        ["source_expression_substitution_formals"] = "expression_id,substitution_ordinal,ordinal",
        ["source_expression_substitution_arguments"] = "expression_id,substitution_ordinal,ordinal",
        ["source_expression_declarations"] = "id",
        ["source_expression_declaration_references"] = "expression_id",
        ["source_expression_declaration_formals"] = "declaration_id,ordinal",
        ["source_expression_declaration_arguments"] = "declaration_id,ordinal",
        ["source_constraint_argument_details"] = "owner_kind,owner_id,substitution_ordinal,argument_ordinal",
        ["source_pack_expansions"] = "id",
        ["source_pack_references"] = "expansion_id,ordinal",
        ["source_dependent_function_lookups"] = "expression_id",
        ["source_dependent_function_candidates"] = "expression_id,ordinal",
        ["source_dependent_function_arguments"] = "expression_id,ordinal",
        ["source_semantic_constant_expressions"] = "constant_id",
        ["source_aggregate_constants"] = "constant_id",
        ["source_dependent_types"] = "type_id", ["source_nested_type_declarations"] = "type_id",
        ["source_lifetimes"] = "id", ["source_destruction_obligations"] = "id",
        ["destruction_elisions"] = "source_obligation_id,function_entity_id",
        ["source_array_deletions"] = "source_obligation_id", ["array_deletion_operations"] = "node_id", ["array_deletion_virtual_callbacks"] = "call_node_id",
        ["source_array_allocation_elements"] = "source_allocation_obligation_id",
        ["array_allocation_operations"] = "node_id",
        ["source_uninitialized_allocations"] = "source_allocation_obligation_id",
        ["allocation_elisions"] = "source_allocation_obligation_id,function_entity_id",
        ["array_runtime_operations"] = "node_id", ["cleanup_arrays"] = "action_id", ["cleanup_vtt_arguments"] = "action_id",
        ["cleanup_conditions"] = "action_id", ["cleanup_actions"] = "id", ["cleanup_states"] = "id",
        ["allocation_cleanup_actions"] = "id", ["local_static_guards"] = "id", ["guard_operations"] = "node_id", ["guard_cleanup_actions"] = "id",
        ["cleanup_transitions"] = "node_id", ["cleanup_frame_markers"] = "node_id",
        ["memory_barriers"] = "node_id", ["memory_barrier_operands"] = "node_id,ordinal",
        ["memory_barrier_dimensions"] = "node_id,operand_ordinal,ordinal",
        ["source_record_owners"] = "source_record_id", ["source_record_types"] = "id", ["source_record_fields"] = "id",
        ["source_record_operations"] = "source_record_id", ["record_value_transfers"] = "node_id", ["record_storage_copies"] = "node_id", ["record_storage_initializations"] = "node_id", ["source_record_temporaries"] = "id", ["record_temporary_initializations"] = "node_id", ["record_constant_initializations"] = "node_id", ["virtual_base_adjustments"] = "node_id",
        ["initialization_actions"] = "id",
        ["generated_routine_origins"] = "entity_id", ["generated_routine_scaffolds"] = "entity_id,ordinal", ["dynamic_initializers"] = "id",
        ["dynamic_initializer_statements"] = "initializer_id,ordinal",
        ["abi_routine_origins"] = "entity_id", ["source_record_bases"] = "id", ["record_base_subobjects"] = "record_type_id,source_base_id",
        ["object_info"] = "singleton", ["provenance"] = "ordinal", ["source_files"] = "id",
        ["record_types"] = "type_id", ["record_fields"] = "id",
        ["routine_types"] = "type_id", ["routine_parameters"] = "type_id,ordinal",
        ["source_routine_exception_resolutions"] = "source_routine_type_id",
        ["source_dynamic_exception_specifications"] = "source_routine_type_id",
        ["source_dynamic_exception_types"] = "source_routine_type_id,ordinal",
        ["function_dynamic_exception_boundaries"] = "function_entity_id",
        ["source_routine_types"] = "type_id", ["source_routine_parameters"] = "type_id,ordinal",
        ["deferred_function_declarations"] = "entity_id", ["deferred_routine_abis"] = "routine_type_id,source_routine_type_id", ["source_routine_abis"] = "source_routine_type_id", ["source_routine_abi_parameters"] = "source_routine_type_id,execution_ordinal",
        ["storages"] = "id", ["storage_data"] = "storage_id,byte_offset", ["relocations"] = "storage_id,byte_offset",
        ["types"] = "id", ["entities"] = "id", ["parameters"] = "entity_id,ordinal",
        ["functions"] = "entity_id", ["bindings"] = "id", ["nodes"] = "id",
        ["edges"] = "parent_id,role,ordinal", ["content_digest"] = "singleton",
        ["source_type_qualifiers"] = "type_id,name", ["source_enum_types"] = "type_id",
        ["source_enumerators"] = "type_id,ordinal", ["source_member_pointer_types"] = "type_id",
        ["source_types"] = "id", ["source_declarations"] = "id",
        ["source_functions"] = "entity_id", ["source_parameters"] = "entity_id,ordinal",
        ["macro_definitions"] = "id", ["macro_parameters"] = "definition_id,ordinal", ["macro_events"] = "id"
    };
    private static readonly Dictionary<string, (string Kind, string Sql)> ExpectedSchema = LoadSchema();

    private static Dictionary<string, (string Kind, string Sql)> LoadSchema()
    {
        using Stream stream = typeof(ObjectFile).Assembly.GetManifestResourceStream("DotObj.Schema.sql")
            ?? throw new InvalidOperationException("Canonical object schema is missing from linker assembly");
        using var reader = new StreamReader(stream);
        string schema = reader.ReadToEnd();
        var result = new Dictionary<string, (string Kind, string Sql)>(StringComparer.Ordinal);
        foreach (Match match in Regex.Matches(schema, @"CREATE (TABLE|INDEX) ([a-z_]+)\b[^;]*;", RegexOptions.CultureInvariant))
            result.Add(match.Groups[2].Value, (match.Groups[1].Value.ToLowerInvariant(), match.Value[..^1]));
        if (result.Count != Tables.Count + 3) throw new InvalidOperationException("Unexpected canonical object schema");
        return result;
    }

    public ObjectFile(string path)
    {
        Path = System.IO.Path.GetFullPath(path);
        database = new Sqlite(Path);
        try
        {
            RequireScalar("PRAGMA application_id", 1146045002);
            RequireScalar("PRAGMA user_version", 1);
            using (var check = database.Prepare("PRAGMA integrity_check"))
                if (!check.Read() || check.Text(0) != "ok" || check.Read()) throw new InvalidDataException("SQLite integrity check failed");
            using (var check = database.Prepare("PRAGMA foreign_key_check"))
                if (check.Read()) throw new InvalidDataException("Object contains broken foreign keys");
            VerifyDigest();
            using (var info = database.Prepare("SELECT format_version,profile,target_abi,runtime_abi,unit_id,primary_source,prelower_source,macros,lifetimes,source_declarations,macro_definitions,source_language FROM object_info"))
            {
                if (!info.Read() || info.Integer(0) != 1 || info.Text(1) != "scalar-control-v58" ||
                    info.Text(2) != "portable-lp64-binary80-v2" || info.Text(3) != "dotobj-scalar-v1" ||
                    info.Integer(6) != 0 || info.Integer(7) != 0 || info.Integer(8) != 0 || info.Integer(9) != 1 || info.Integer(10) != 1)
                    throw new InvalidDataException("Unsupported object profile, format, ABI or capture capabilities");
                UnitId = info.Text(4); PrimarySource = info.Text(5);
                SourceLanguage = info.Text(11);
                if (SourceLanguage is not ("c" or "c++")) throw new InvalidDataException("Invalid original source language");
                if (UnitId.Length == 0 || info.Read()) throw new InvalidDataException("Invalid object identity");
            }
            using (var digest = database.Prepare("SELECT sha256 FROM content_digest"))
            { digest.Read(); Digest = digest.Text(0); }
            using (var primary = database.Prepare("SELECT s.available,s.sha256,o.source_sha256 FROM source_files s JOIN object_info o ON s.path=o.primary_source WHERE s.parent_id IS NULL"))
                if (!primary.Read() || primary.Integer(0) != 1 || primary.Text(1) != primary.Text(2) || primary.Read())
                    throw new InvalidDataException("Primary source identity does not match exactly one available root snapshot");
            ReadSummaries();
            ReadInitializationActions();
            ReadDynamicInitializers();
            ReadCompileTimeSelections();
            ReadThunkWrappers();
            ReadLambdaEntryOrigins();
            ReadTlsRoutineOrigins();
            ReadAndVerifySourceProvenance();
            ReadAndVerifyMacroProvenance();
            ReadMemoryBarrierMetadata();
            ReadStaticSources();
            ReadDefaultArgumentExecutions();
            ReadCleanupMetadata();
            ReadStaticExecutionMetadata();
            ReadGeneratedArrayInitializers();
            ReadRecordValueTransfers();
            ReadRecordStorageInitializations();
            VerifyDefaultArgumentExecutions();
            ReadRecordConstantInitializations();
            ReadVirtualBaseAdjustments();
            ReadExceptionMetadata();
            ReadDynamicExceptionBoundaries();
        }
        catch { database.Dispose(); throw; }
    }

    private void RequireScalar(string sql, long expected)
    {
        using var query = database.Prepare(sql);
        if (!query.Read() || query.Integer(0) != expected || query.Read()) throw new InvalidDataException("Unsupported SQLite object format");
    }

    private void VerifyDigest()
    {
        var schemas = new SortedDictionary<string, string>(StringComparer.Ordinal);
        var schemaObjects = new HashSet<string>(StringComparer.Ordinal);
        using (var query = database.Prepare("SELECT name,sql,type FROM sqlite_master WHERE name NOT LIKE 'sqlite_%' ORDER BY name"))
            while (query.Read())
            {
                string name = query.Text(0), sql = query.Text(1), kind = query.Text(2);
                if (!ExpectedSchema.TryGetValue(name, out var expected) || expected.Kind != kind || expected.Sql != sql)
                    throw new InvalidDataException($"Unexpected database schema definition: {name}");
                schemaObjects.Add(name);
                if (kind == "table") schemas.Add(name, sql);
            }
        if (schemaObjects.Count != ExpectedSchema.Count) throw new InvalidDataException("Missing required semantic table or index");
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        void Text(string text) => hash.AppendData(Encoding.UTF8.GetBytes(text));
        void Frame(byte[] bytes) { Text(bytes.Length.ToString(CultureInfo.InvariantCulture) + ":"); hash.AppendData(bytes); }
        Text("dotobj-semantic/1\n");
        foreach (var (table, schema) in schemas)
        {
            if (table == "content_digest") continue;
            Text("T"); Frame(Encoding.UTF8.GetBytes(table)); Text("S"); Frame(Encoding.UTF8.GetBytes(schema));
            using var query = database.Prepare($"SELECT * FROM {table} ORDER BY {Tables[table]}");
            while (query.Read())
            {
                Text("R");
                for (int index = 0; index < query.ColumnCount; index++)
                {
                    int type = query.ColumnType(index);
                    Text(type.ToString(CultureInfo.InvariantCulture));
                    Frame(type switch
                    {
                        1 => Encoding.UTF8.GetBytes(query.Integer(index).ToString(CultureInfo.InvariantCulture)),
                        3 => Encoding.UTF8.GetBytes(query.Text(index)), 4 => query.Bytes(index), 5 => [],
                        _ => throw new InvalidDataException("Unsupported SQLite cell type")
                    });
                }
                Text("\n");
            }
            Text("\n");
        }
        string actual = Convert.ToHexStringLower(hash.GetHashAndReset());
        using (var digest = database.Prepare("SELECT algorithm,sha256 FROM content_digest"))
            if (!digest.Read() || digest.Text(0) != "dotobj-semantic-sha256-v1" || digest.Text(1) != actual || digest.Read())
                throw new InvalidDataException("Semantic object digest mismatch");
        var sourceIdentities = new Dictionary<string, string>(StringComparer.Ordinal);
        using (var sources = database.Prepare("SELECT available,sha256,bytes,path FROM source_files"))
            while (sources.Read())
                if (sources.Integer(0) == 1)
                {
                    string digest = sources.Text(1), path = sources.Text(3);
                    if (Convert.ToHexStringLower(SHA256.HashData(sources.Bytes(2))) != digest)
                        throw new InvalidDataException("Source snapshot digest mismatch");
                    if (sourceIdentities.TryGetValue(path, out string? prior) && prior != digest)
                        throw new InvalidDataException("Include occurrences disagree on source snapshot identity");
                    sourceIdentities[path] = digest;
                }
    }

    private void ReadSummaries()
    {
        ReadSourceRecordMetadata();
        ReadSourceRecordOperations();
        ReadRecordMetadata();
        ReadRoutineMetadata(false);
        var rawTypes = new Dictionary<long, (string Kind, long? Pointee, long? Element, long? Count)>();
        using (var query = database.Prepare("SELECT id,kind,pointee_type_id,element_type_id,element_count FROM types ORDER BY id"))
            while (query.Read())
            {
                long id = query.Integer(0);
                string kind = query.Text(1);
                long? pointee = query.OptionalInteger(2), element = query.OptionalInteger(3), count = query.OptionalInteger(4);
                if (id <= 0 || (kind is not ("void" or "bool" or "pointer" or "array" or "record" or "routine" or "va-list") && !ScalarTypes.IsNumeric(kind)) ||
                    (kind == "pointer") != (pointee is not null) || (kind == "array") != (element is not null && count is null or >= 0) ||
                    kind != "array" && (element is not null || count is not null))
                    throw new InvalidDataException("Unsupported execution type or pointer shape");
                rawTypes.Add(id, (kind, pointee, element, count));
            }
        var active = new HashSet<long>();
        var typeDepths = new Dictionary<long, int>();
        var canonical = new HashSet<string>(StringComparer.Ordinal);
        string ResolveType(long id, int depth)
        {
            if (Types.TryGetValue(id, out string? resolved)) return resolved;
            if (depth > 128) throw new InvalidDataException("Pointer type nesting exceeds supported limit");
            if (!rawTypes.TryGetValue(id, out var raw)) throw new InvalidDataException("Unknown pointer pointee type");
            if (!active.Add(id)) throw new InvalidDataException("Cycle in execution pointer types");
            string key = raw.Kind switch
            {
                "routine" => Routines.TryGetValue(id, out RoutineType? routine)
                    ? RoutineTypes.Key(ResolveType(routine.ReturnTypeId, depth + 1), routine.Parameters.Select(parameter => ResolveType(parameter, depth + 1)), routine.Variadic, routine.ExceptionPolicy)
                    : throw new InvalidDataException("Routine execution type has no signature"),
                "record" => Records.TryGetValue(id, out RecordType? record) ? record.Key : throw new InvalidDataException("Record type has no layout metadata"),
                "pointer" => "ptr(" + ResolveType(raw.Pointee!.Value, depth + 1) + ")",
                "array" => "array(" + (raw.Count?.ToString(CultureInfo.InvariantCulture) ?? "?") + "," + ResolveType(raw.Element!.Value, depth + 1) + ")",
                _ => raw.Kind
            };
            if (raw.Kind == "array")
            {
                if (ScalarTypes.IsIncompleteArray(ScalarTypes.Element(key))) throw new InvalidDataException("Array element cannot have an unknown bound");
                if (ScalarTypes.Element(key) == "void") throw new InvalidDataException("Array element cannot be void");
                if (RoutineTypes.IsRoutine(ScalarTypes.Element(key))) throw new InvalidDataException("Array element cannot be routine type");
            }
            if (raw.Kind == "routine")
            {
                var signature = RoutineTypes.Signature(key);
                if (RoutineTypes.IsRoutine(signature.Result) || ScalarTypes.IsArray(signature.Result) ||
                    signature.Parameters.Any(parameter => parameter == "void" || RoutineTypes.IsRoutine(parameter) || ScalarTypes.IsArray(parameter)))
                    throw new InvalidDataException("Invalid routine result or parameter type");
            }
            int nesting = raw.Kind == "routine" ? new[] { Routines[id].ReturnTypeId }.Concat(Routines[id].Parameters).Max(child => typeDepths[child]) + 1 : (raw.Pointee ?? raw.Element) is long child ? typeDepths[child] + 1 : 0;
            if (nesting > 128) throw new InvalidDataException("Pointer type nesting exceeds supported limit");
            typeDepths.Add(id, nesting);
            active.Remove(id);
            if (!canonical.Add(key)) throw new InvalidDataException("Duplicate canonical execution type");
            Types.Add(id, key);
            return key;
        }
        foreach (long id in rawTypes.Keys) ResolveType(id, 0);
        VerifyRecordLayouts();
        foreach (string array in Types.Values.Where(ScalarTypes.IsArray))
            if (ScalarTypes.IsRecord(ScalarTypes.Leaf(array)) && RecordsByKey[ScalarTypes.Leaf(array)].RepresentationKind == "subobject") throw new InvalidDataException("Array elements must use complete nominal records");
        foreach (string array in Types.Values.Where(ScalarTypes.IsArray))
            if (StorageSize(array) < 0 || StorageSize(array) > int.MaxValue) throw new InvalidDataException("Array extent exceeds supported storage");
        if (Routines.Keys.Any(id => !RoutineTypes.IsRoutine(Type(id)))) throw new InvalidDataException("Routine metadata describes a non-routine type");
        ReadStorages();
        ReadDeferredFunctionDeclarations();
        using (var query = database.Prepare("SELECT id,source_name,linkage,external_key,definition,return_type_id,source_file_id,line,column,routine_type_id,is_weak FROM entities ORDER BY id"))
            while (query.Read())
            {
                var entity = new Entity(query.Integer(0), query.Text(1), query.Text(2), query.OptionalText(3), query.Text(4), query.Integer(5), query.OptionalInteger(6), query.Integer(7), query.Integer(8), query.Integer(9), query.Integer(10) == 1);
                if (entity.Id <= 0 || entity.Linkage is not ("external" or "internal") || entity.Definition is not ("strong" or "linkonce" or "weak" or "declaration") ||
                    query.Integer(10) is not (0 or 1) ||
                    (entity.IsWeak ? entity.Linkage != "external" || entity.Definition is not ("weak" or "declaration") : entity.Definition == "weak") ||
                    (entity.Linkage == "external" ? string.IsNullOrEmpty(entity.ExternalKey) : entity.ExternalKey != null) || entity.Line < 0 || entity.Column < 0)
                    throw new InvalidDataException("Invalid entity summary");
                if (ScalarTypes.IsArray(Type(entity.ReturnTypeId)) || !CanTransportRecord(Type(entity.ReturnTypeId)) && !PermitsDeferredValue(entity.Id, Type(entity.ReturnTypeId))) throw new InvalidDataException("Function cannot return an aggregate by value");
                Entities.Add(entity.Id, entity);
            }
        using (var query = database.Prepare("SELECT entity_id,ordinal,type_id,parameter_role,source_ordinal FROM parameters ORDER BY entity_id,ordinal"))
            while (query.Read())
            {
                long entity = query.Integer(0);
                if (!Entities.ContainsKey(entity)) throw new InvalidDataException("Parameter references unknown entity");
                if (!Parameters.TryGetValue(entity, out var parameters)) Parameters.Add(entity, parameters = []);
                string type = Type(query.Integer(2));
                if (query.Integer(1) != parameters.Count || (type == "void" || ScalarTypes.IsArray(type) || !CanPassRecord(type) && !PermitsDeferredValue(entity, type))) throw new InvalidDataException("Invalid parameter ordinal/type");
                string role = query.Text(3); long? sourceOrdinal = query.OptionalInteger(4);
                long ordinal = query.Integer(1);
                if (role is not ("source" or "indirect-source" or "this" or "result-buffer" or "vtt" or "forwarded-source" or "static-object" or "initialization-object") || (role is "this" or "result-buffer" or "vtt" or "static-object" or "initialization-object" ? sourceOrdinal is not null : sourceOrdinal is null or < 0))
                    throw new InvalidDataException("Invalid execution parameter source role");
                ParameterRoles.Add((entity, ordinal), new(entity, ordinal, role, sourceOrdinal));
                parameters.Add(type);
            }
        foreach (Entity entity in Entities.Values)
            if (!Routines.ContainsKey(entity.RoutineTypeId) || Type(entity.RoutineTypeId) != RoutineTypes.Key(Type(entity.ReturnTypeId), Parameters.GetValueOrDefault(entity.Id, []), Routines[entity.RoutineTypeId].Variadic, Routines[entity.RoutineTypeId].ExceptionPolicy))
                throw new InvalidDataException("Entity summary disagrees with its canonical routine type");
        using (var query = database.Prepare("SELECT entity_id,body_node_id FROM functions ORDER BY entity_id"))
            while (query.Read()) Functions.Add(query.Integer(0), query.Integer(1));
        foreach (Entity entity in Entities.Values)
            if (Functions.ContainsKey(entity.Id) != (entity.Definition is "strong" or "linkonce" or "weak")) throw new InvalidDataException("Definition/body summary mismatch");
        VerifyStorageRelocations();
        if (Functions.Keys.Any(id => !Entities.ContainsKey(id))) throw new InvalidDataException("Function has no entity summary");
    }

    public string Type(long id) => Types.TryGetValue(id, out string? type) ? type : throw new InvalidDataException($"Unknown type ID {id}");

    public FunctionBody ReadBody(long functionId)
    {
        // IDs are validated integers, never source-controlled SQL fragments.
        string id = functionId.ToString(CultureInfo.InvariantCulture);
        var nodes = new Dictionary<long, Node>();
        using (var query = database.Prepare($"SELECT id,function_id,kind,type_id,entity_id,binding_id,operator,constant_text,value_category,evaluation_order,field_id,storage_id FROM nodes WHERE function_id={id} ORDER BY id"))
            while (query.Read())
            {
                var node = new Node(query.Integer(0), query.Integer(1), query.Text(2), query.OptionalInteger(3), query.OptionalInteger(4), query.OptionalInteger(5), query.OptionalText(6), query.OptionalText(7), query.OptionalText(8), query.OptionalText(9), query.OptionalInteger(10), query.OptionalInteger(11));
                nodes.Add(node.Id, node);
            }
        var bindings = new Dictionary<long, Binding>();
        using (var query = database.Prepare($"SELECT id,function_id,kind,ordinal,type_id,source_name,scope_node_id,is_synthetic FROM bindings WHERE function_id={id} ORDER BY id"))
            while (query.Read())
            {
                var binding = new Binding(query.Integer(0), query.Integer(1), query.Text(2), query.OptionalInteger(3), query.Integer(4), query.Text(5), query.Integer(6), query.Integer(7) == 1);
                bool syntheticParameter = binding.Kind == "parameter" && binding.Ordinal is long ordinal &&
                    ParameterRoles.TryGetValue((functionId, ordinal), out ParameterSemantics? parameter) && parameter.Role is "this" or "result-buffer" or "vtt" or "forwarded-source" or "static-object" or "initialization-object";
                if (query.Integer(7) is not (0 or 1) || binding.Kind == "parameter" && binding.IsSynthetic != syntheticParameter || binding.IsSynthetic && binding.Kind is not ("local" or "parameter"))
                    throw new InvalidDataException("Synthetic parameter requires a proven generated ABI role");
                bindings.Add(binding.Id, binding);
            }
        var edges = new Dictionary<long, List<Edge>>();
        using (var query = database.Prepare($"SELECT e.parent_id,e.role,e.ordinal,e.child_id FROM edges e JOIN nodes n ON n.id=e.parent_id WHERE n.function_id={id} ORDER BY e.parent_id,e.role,e.ordinal"))
            while (query.Read())
            {
                var edge = new Edge(query.Integer(0), query.Text(1), query.Integer(2), query.Integer(3));
                if (!edges.TryGetValue(edge.ParentId, out var list)) edges.Add(edge.ParentId, list = []);
                list.Add(edge);
            }
        var body = new FunctionBody { RootId = Functions[functionId], Nodes = nodes, Bindings = bindings, Edges = edges };
        Verifier.Body(this, Entities[functionId], body);
        VerifyGeneratedRoutineBody(functionId, body);
        return body;
    }

    public void Dispose() => database.Dispose();
}
