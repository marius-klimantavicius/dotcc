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
    public string Digest { get; }
    public Dictionary<long, string> Types { get; } = [];
    public Dictionary<long, Entity> Entities { get; } = [];
    public Dictionary<long, List<string>> Parameters { get; } = [];
    public Dictionary<long, long> Functions { get; } = [];

    private static readonly Dictionary<string, string> Tables = new(StringComparer.Ordinal)
    {
        ["object_info"] = "singleton", ["provenance"] = "ordinal", ["source_files"] = "id",
        ["types"] = "id", ["entities"] = "id", ["parameters"] = "entity_id,ordinal",
        ["functions"] = "entity_id", ["bindings"] = "id", ["nodes"] = "id",
        ["edges"] = "parent_id,role,ordinal", ["content_digest"] = "singleton",
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
            using (var info = database.Prepare("SELECT format_version,profile,target_abi,runtime_abi,unit_id,primary_source,prelower_source,macros,lifetimes,source_declarations,macro_definitions FROM object_info"))
            {
                if (!info.Read() || info.Integer(0) != 1 || info.Text(1) != "scalar-control-v10" ||
                    info.Text(2) != "portable-lp64-binary80-v2" || info.Text(3) != "dotobj-scalar-v1" ||
                    info.Integer(6) != 0 || info.Integer(7) != 0 || info.Integer(8) != 0 || info.Integer(9) != 1 || info.Integer(10) != 1)
                    throw new InvalidDataException("Unsupported object profile, format, ABI or capture capabilities");
                UnitId = info.Text(4); PrimarySource = info.Text(5);
                if (UnitId.Length == 0 || info.Read()) throw new InvalidDataException("Invalid object identity");
            }
            using (var digest = database.Prepare("SELECT sha256 FROM content_digest"))
            { digest.Read(); Digest = digest.Text(0); }
            using (var primary = database.Prepare("SELECT s.available,s.sha256,o.source_sha256 FROM source_files s JOIN object_info o ON s.path=o.primary_source WHERE s.parent_id IS NULL"))
                if (!primary.Read() || primary.Integer(0) != 1 || primary.Text(1) != primary.Text(2) || primary.Read())
                    throw new InvalidDataException("Primary source identity does not match exactly one available root snapshot");
            ReadSummaries();
            ReadAndVerifySourceProvenance();
            ReadAndVerifyMacroProvenance();
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
        var rawTypes = new Dictionary<long, (string Kind, long? Pointee, long? Element, long? Count)>();
        using (var query = database.Prepare("SELECT id,kind,pointee_type_id,element_type_id,element_count FROM types ORDER BY id"))
            while (query.Read())
            {
                long id = query.Integer(0);
                string kind = query.Text(1);
                long? pointee = query.OptionalInteger(2), element = query.OptionalInteger(3), count = query.OptionalInteger(4);
                if (id <= 0 || (kind is not ("void" or "bool" or "pointer" or "array") && !ScalarTypes.IsNumeric(kind)) ||
                    (kind == "pointer") != (pointee is not null) || (kind == "array") != (element is not null && count is > 0) ||
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
                "pointer" => "ptr(" + ResolveType(raw.Pointee!.Value, depth + 1) + ")",
                "array" => "array(" + raw.Count!.Value.ToString(CultureInfo.InvariantCulture) + "," + ResolveType(raw.Element!.Value, depth + 1) + ")",
                _ => raw.Kind
            };
            if (raw.Kind == "array")
            {
                if (ScalarTypes.Element(key) == "void") throw new InvalidDataException("Array element cannot be void");
                long size;
                try { size = ScalarTypes.Size(key); }
                catch (OverflowException) { throw new InvalidDataException("Array layout overflows supported size"); }
                if (size > 65_536) throw new InvalidDataException("Array storage exceeds the supported 64 KiB value type limit");
            }
            int nesting = (raw.Pointee ?? raw.Element) is long child ? typeDepths[child] + 1 : 0;
            if (nesting > 128) throw new InvalidDataException("Pointer type nesting exceeds supported limit");
            typeDepths.Add(id, nesting);
            active.Remove(id);
            if (!canonical.Add(key)) throw new InvalidDataException("Duplicate canonical execution type");
            Types.Add(id, key);
            return key;
        }
        foreach (long id in rawTypes.Keys) ResolveType(id, 0);
        using (var query = database.Prepare("SELECT id,source_name,linkage,external_key,definition,return_type_id,source_file_id,line,column FROM entities ORDER BY id"))
            while (query.Read())
            {
                var entity = new Entity(query.Integer(0), query.Text(1), query.Text(2), query.OptionalText(3), query.Text(4), query.Integer(5), query.OptionalInteger(6), query.Integer(7), query.Integer(8));
                if (entity.Id <= 0 || entity.Linkage is not ("external" or "internal") || entity.Definition is not ("strong" or "declaration") ||
                    (entity.Linkage == "external" ? string.IsNullOrEmpty(entity.ExternalKey) : entity.ExternalKey != null) || entity.Line < 0 || entity.Column < 0)
                    throw new InvalidDataException("Invalid entity summary");
                if (ScalarTypes.IsArray(Type(entity.ReturnTypeId))) throw new InvalidDataException("Function cannot return array by value");
                Entities.Add(entity.Id, entity);
            }
        using (var query = database.Prepare("SELECT entity_id,ordinal,type_id FROM parameters ORDER BY entity_id,ordinal"))
            while (query.Read())
            {
                long entity = query.Integer(0);
                if (!Entities.ContainsKey(entity)) throw new InvalidDataException("Parameter references unknown entity");
                if (!Parameters.TryGetValue(entity, out var parameters)) Parameters.Add(entity, parameters = []);
                string type = Type(query.Integer(2));
                if (query.Integer(1) != parameters.Count || (type == "void" || ScalarTypes.IsArray(type))) throw new InvalidDataException("Invalid parameter ordinal/type");
                parameters.Add(type);
            }
        using (var query = database.Prepare("SELECT entity_id,body_node_id FROM functions ORDER BY entity_id"))
            while (query.Read()) Functions.Add(query.Integer(0), query.Integer(1));
        foreach (Entity entity in Entities.Values)
            if (Functions.ContainsKey(entity.Id) != (entity.Definition == "strong")) throw new InvalidDataException("Definition/body summary mismatch");
        if (Functions.Keys.Any(id => !Entities.ContainsKey(id))) throw new InvalidDataException("Function has no entity summary");
    }

    public string Type(long id) => Types.TryGetValue(id, out string? type) ? type : throw new InvalidDataException($"Unknown type ID {id}");

    public FunctionBody ReadBody(long functionId)
    {
        // IDs are validated integers, never source-controlled SQL fragments.
        string id = functionId.ToString(CultureInfo.InvariantCulture);
        var nodes = new Dictionary<long, Node>();
        using (var query = database.Prepare($"SELECT id,function_id,kind,type_id,entity_id,binding_id,operator,constant_text,value_category,evaluation_order FROM nodes WHERE function_id={id} ORDER BY id"))
            while (query.Read())
            {
                var node = new Node(query.Integer(0), query.Integer(1), query.Text(2), query.OptionalInteger(3), query.OptionalInteger(4), query.OptionalInteger(5), query.OptionalText(6), query.OptionalText(7), query.OptionalText(8), query.OptionalText(9));
                nodes.Add(node.Id, node);
            }
        var bindings = new Dictionary<long, Binding>();
        using (var query = database.Prepare($"SELECT id,function_id,kind,ordinal,type_id,source_name,scope_node_id FROM bindings WHERE function_id={id} ORDER BY id"))
            while (query.Read())
            {
                var binding = new Binding(query.Integer(0), query.Integer(1), query.Text(2), query.OptionalInteger(3), query.Integer(4), query.Text(5), query.Integer(6));
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
        return body;
    }

    public void Dispose() => database.Dispose();
}
