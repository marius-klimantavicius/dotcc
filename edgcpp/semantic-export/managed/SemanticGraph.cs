using System.Security.Cryptography;
using System.Text.Json;

namespace Edg.Semantic;

/// <summary>A validated snapshot. Node IDs are identities within this translation unit only.</summary>
public sealed class SemanticGraph : IDisposable
{
    private readonly JsonDocument? document;
    private readonly SqliteSemanticStore? store;
    private readonly Dictionary<ulong, SemanticNode> nodes = new();
    private readonly Dictionary<string, List<SemanticNode>> nodesByKind = new(StringComparer.Ordinal);
    public JsonElement Raw => store?.Metadata ?? document!.RootElement;
    public IReadOnlyDictionary<ulong, SemanticNode> Nodes => store?.Nodes ?? nodes;
    public int DiagnosticCount => store?.DiagnosticCount ?? Raw.GetProperty("diagnostics").GetArrayLength();
    public int MacroInvocationCount => store?.MacroCount ?? Raw.GetProperty("macroInvocations").GetArrayLength();
    public IEnumerable<KeyValuePair<string, int>> KindCounts => store?.KindCounts() ?? nodesByKind.OrderBy(p => p.Key).Select(p => new KeyValuePair<string, int>(p.Key, p.Value.Count));
    public string GraphPath { get; }
    public bool CompleteForEmission => Raw.GetProperty("completeForEmission").GetBoolean();
    public IEnumerable<JsonElement> Diagnostics => store?.Diagnostics() ?? Raw.GetProperty("diagnostics").EnumerateArray();
    public IEnumerable<JsonElement> MacroInvocations => store?.Macros() ?? Raw.GetProperty("macroInvocations").EnumerateArray();

    // Numeric scalar fields (offset, count, size, enum values, etc.) are not references.
    // Unknown fields are retained, never guessed to be references from their numeric value.
    internal static readonly HashSet<string> ReferenceFields = new(StringComparer.Ordinal)
    {
        "parentScope", "enclosingRoutine", "declarationPositions", "sourceSequence",
        "next", "canonicalType", "elementType", "underlyingType", "returnType", "signature",
        "bound", "fields", "recordInfo", "memberClass", "type", "declaredType", "template",
        "templateArguments", "bodyScope", "initializer", "parent", "children", "body", "lifetime",
        "variables", "types", "routines", "templates", "routine", "parameters", "constructorInitializers",
        "originalLvalueType", "operands", "constant", "symbol", "initialization", "expression", "condition",
        "throwInfo", "operandType", "operand", "then", "else", "loopInfo", "statements", "blockInfo",
        "returnInitialization", "label", "commonLifetime", "tryInfo", "entities", "switchInfo", "switch", "caseInfo",
        "increment", "scope", "statement", "originalType", "backingExpression", "elements", "variable",
        "destructor", "nextDestruction", "initializationExpressionLifetime", "master", "constructor", "arguments",
        "source", "parentLifetime", "parentDestructionCursor", "destructions", "childLifetime", "entity",
        "handlers", "parameter", "canonicalTemplate", "definitionTemplate", "prototypeTemplate", "bases",
        "partialSpecializationArguments", "memberDeclarations", "defaultArgument", "firstChild",
        "thisClass", "exceptionSpecification", "initializationStatement", "declaration", "qualifier",
        "writtenTemplateArguments", "namespace", "sourceExpression", "basedType", "field", "base", "overlappingTemporariesLifetime",
        "integerInfo", "enumScope", "enumerators", "typerefInfo", "pragmas", "baseType", "proxyClass", "operatorTypeArgument", "mainRoutine", "cleanupRoutine",
        "cases", "defaultCase", "sortedCases", "caseValue", "nextSorted", "rangeEnd", "localStaticInitializers",
        "localExpressionReferences", "localScopeReferences", "lastSourceSequence", "namespaces", "sourceSequenceSublists",
        "attributes", "gnuRoutineInfo", "subobjectPath", "attributeGroup", "scopedExpression", "aliasedRoutine", "inlinePartner"
    };

    private SemanticGraph(string path)
    {
        GraphPath = Path.GetFullPath(path);
        if (IsSqlite(GraphPath))
        {
            store = new SqliteSemanticStore(this, GraphPath);
            try { ValidateEnvelope(); store.ValidateLiterals(value => ValidatePositions(value, store.HasMacro)); }
            catch { store.Dispose(); throw; }
        }
        else
        {
            document = JsonDocument.Parse(File.ReadAllBytes(GraphPath));
            try { Validate(); }
            catch { document.Dispose(); throw; }
        }
    }

    public static SemanticGraph Load(string path, bool requireEmissionReady = false)
    {
        path = ResolveInput(path);
        var packagePath = Path.GetFileName(path) == "package.json" ? path : null;
        if (packagePath != null) path = ResolvePackage(path);
        var graph = new SemanticGraph(path);
        try { if (packagePath != null) graph.ValidatePackageSourceIds(packagePath, requireEmissionReady); else if (requireEmissionReady) graph.store?.RequireEmissionSources(); }
        catch { graph.Dispose(); throw; }
        if (requireEmissionReady && (!graph.CompleteForEmission || graph.Diagnostics.Any()))
        {
            graph.Dispose();
            throw new InvalidDataException("Package is not emission-ready: completeForEmission must be true and diagnostics must be empty.");
        }
        return graph;
    }

    /// <summary>
    /// Load the source snapshot for the bounded C emitter. Its capability checks
    /// are independent of the graph-wide completeForEmission promise.
    /// </summary>
    public static SemanticGraph LoadForCEmission(string path)
    {
        path = ResolveInput(path);
        if (IsSqlite(path))
        {
            var sqlite = Load(path);
            try { sqlite.store!.RequireEmissionSources(); return sqlite; }
            catch { sqlite.Dispose(); throw; }
        }
        if (Path.GetFileName(path) != "package.json")
            throw new InvalidDataException("C emission requires a package.json with verified source snapshots");
        using var package = JsonDocument.Parse(File.ReadAllBytes(path));
        var root = package.RootElement;
        if (!root.TryGetProperty("graph", out var member) || member.ValueKind != JsonValueKind.Object ||
            !member.TryGetProperty("sha256", out _) ||
            !root.TryGetProperty("producer", out var producer) || producer.ValueKind != JsonValueKind.Object ||
            !producer.TryGetProperty("sha256", out _))
            throw new InvalidDataException("C emission requires graph and producer integrity hashes");
        if (!root.TryGetProperty("diagnostics", out var diagnostics) || diagnostics.ValueKind != JsonValueKind.Array ||
            diagnostics.GetArrayLength() != 0)
            throw new InvalidDataException("C emission requires an explicit empty package diagnostics array");
        if (!root.TryGetProperty("sources", out var sources) || sources.ValueKind != JsonValueKind.Array ||
            sources.GetArrayLength() == 0)
            throw new InvalidDataException("C emission requires source snapshots");
        foreach (var source in sources.EnumerateArray())
        {
            if (!source.TryGetProperty("available", out var available) || available.ValueKind != JsonValueKind.True ||
                !source.TryGetProperty("sha256", out var hash) || hash.ValueKind != JsonValueKind.String ||
                !source.TryGetProperty("byteLength", out var length) || !length.TryGetInt64(out var bytes) || bytes < 0)
                throw new InvalidDataException("C emission requires available, hashed source snapshots with byte lengths");
        }
        var graph = Load(path);
        try
        {
            graph.ValidatePackageSourceIds(path, strict: true);
            return graph;
        }
        catch { graph.Dispose(); throw; }
    }

    private static string ResolveInput(string path) => Directory.Exists(path)
        ? Path.Combine(path, File.Exists(Path.Combine(path, "semantic.sqlite")) ? "semantic.sqlite" : "package.json") : path;

    private static bool IsSqlite(string path)
    {
        if (Path.GetExtension(path).Equals(".sqlite", StringComparison.OrdinalIgnoreCase)) return true;
        using var input = File.OpenRead(path);
        Span<byte> header = stackalloc byte[16];
        return input.Read(header) == 16 && header.SequenceEqual("SQLite format 3\0"u8);
    }

    private static string ResolvePackage(string path)
    {
        using var package = JsonDocument.Parse(File.ReadAllBytes(path));
        var root = package.RootElement;
        var basePath = Path.GetDirectoryName(Path.GetFullPath(path))!;
        if (root.GetProperty("schema").GetString() != "edg-semantic-package/1")
            throw new InvalidDataException("Unsupported semantic package schema");
        foreach (var source in root.GetProperty("sources").EnumerateArray())
        {
            if (source.TryGetProperty("available", out var available) && !available.GetBoolean()) continue;
            var file = PackageMember(basePath, source.GetProperty("path").GetString()!);
            VerifyHash(file, source.GetProperty("sha256").GetString()!);
            if (new FileInfo(file).Length != source.GetProperty("byteLength").GetInt64())
                throw new InvalidDataException("Source byte length mismatch: " + file);
        }
        if (root.TryGetProperty("producer", out var producer))
            VerifyHash(PackageMember(basePath, producer.GetProperty("path").GetString()!), producer.GetProperty("sha256").GetString()!);
        var graph = root.GetProperty("graph");
        var relative = graph.ValueKind == JsonValueKind.String ? graph.GetString()! : graph.GetProperty("path").GetString()!;
        var resolved = PackageMember(basePath, relative);
        if (graph.ValueKind == JsonValueKind.Object && graph.TryGetProperty("sha256", out var hash)) VerifyHash(resolved, hash.GetString()!);
        return resolved;
    }

    private void ValidatePackageSourceIds(string path, bool strict)
    {
        using var package = JsonDocument.Parse(File.ReadAllBytes(path));
        var seen = new HashSet<ulong>();
        foreach (var source in package.RootElement.GetProperty("sources").EnumerateArray())
        {
            var id = source.GetProperty("node").GetUInt64();
            if (Get(id)?.Kind != "source-file" || !seen.Add(id))
                throw new InvalidDataException("Invalid or duplicate source package node: " + id);
            if (strict && source.TryGetProperty("available", out var available) && !available.GetBoolean())
                throw new InvalidDataException("Source unavailable for emission: " + id);
        }
        if (OfKind("source-file").Any(n => !seen.Contains(n.Id))) throw new InvalidDataException("Package omits a source file node");
        if (strict && package.RootElement.TryGetProperty("diagnostics", out var diagnostics) && diagnostics.GetArrayLength() != 0)
            throw new InvalidDataException("Package contains diagnostics");
    }

    internal static string PackageMember(string directory, string relative)
    {
        if (Path.IsPathRooted(relative)) throw new InvalidDataException("Package member must be relative: " + relative);
        var resolved = Path.GetFullPath(Path.Combine(directory, relative));
        var prefix = Path.TrimEndingDirectorySeparator(Path.GetFullPath(directory)) + Path.DirectorySeparatorChar;
        if (!resolved.StartsWith(prefix, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
            throw new InvalidDataException("Package member escapes package: " + relative);
        return resolved;
    }

    internal static void VerifyHash(string path, string expected)
    {
        using var stream = File.OpenRead(path);
        var actual = Convert.ToHexStringLower(SHA256.HashData(stream));
        if (!actual.Equals(expected, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("SHA256 mismatch: " + path);
    }

    public SemanticNode? Get(ulong id) => id == 0 ? null : store != null ? store.Get(id) : nodes.TryGetValue(id, out var node) ? node : throw new InvalidDataException("Unknown node ID " + id);
    public IEnumerable<SemanticNode> OfKind(string kind) => store != null ? store.OfKind(kind) : nodesByKind.TryGetValue(kind, out var list) ? list : Array.Empty<SemanticNode>();

    public IEnumerable<SemanticNode> Linked(ulong first, string nextField = "next")
    {
        var seen = new HashSet<ulong>();
        for (var node = Get(first); node != null; node = node.Reference(nextField))
        {
            if (!seen.Add(node.Id)) throw new InvalidDataException($"Cycle in {nextField} list at node {node.Id}");
            yield return node;
        }
    }

    /// <summary>Source sequence preserves declaration order, including secondary declarations.</summary>
    public IEnumerable<SemanticNode> SourceOrder(SemanticNode scope) => Linked(scope.ReferenceId("sourceSequence"));

    /// <summary>Resolve only direct calls. Virtual dispatch remains annotated on the call expression.</summary>
    public SemanticNode? DirectCallTarget(SemanticNode expression)
    {
        if (expression.Kind != "expr-node" || expression.Text("operator") is not ("eok_call" or "eok_virtual_call")) return null;
        var target = expression.Reference("operands");
        var seen = new HashSet<ulong>();
        while (target != null && seen.Add(target.Id))
        {
            if (target.Kind == "routine") return target;
            if (target.Reference("symbol") is { Kind: "routine" } routine) return routine;
            if (target.Reference("routine") is { Kind: "routine" } address) return address;
            target = target.Reference("constant") ?? target.Reference("operand") ??
                (target.Text("operator") is "eok_address_of" or "eok_cast" ? target.Reference("operands") : null);
        }
        return null;
    }

    private void RequireReference(JsonElement value, string context, string? expectedKind = null)
    {
        if (value.ValueKind != JsonValueKind.Number || !value.TryGetUInt64(out var id)) throw new InvalidDataException("Invalid node reference: " + context);
        var kind = store != null ? store.KindOf(id) : Get(id)?.Kind;
        if (kind != null && expectedKind != null && kind != expectedKind)
            throw new InvalidDataException($"Reference {context} expects {expectedKind}, found {kind}");
    }

    private void ValidateEnvelope()
    {
        if (Raw.GetProperty("schema").GetString() != "edg-semantic/1") throw new InvalidDataException("Unsupported semantic graph schema");
        if (Raw.GetProperty("lowered").GetBoolean()) throw new InvalidDataException("Expected unlowered semantic graph");
        _ = CompleteForEmission;
        RequireReference(Raw.GetProperty("rootScope"), "rootScope", "scope");
        if (Raw.GetProperty("rootScope").GetUInt64() == 0) throw new InvalidDataException("Missing root scope");
        RequireReference(Raw.GetProperty("primarySource"), "primarySource", "source-file");
        if (Raw.TryGetProperty("mainRoutine", out var mainRoutine)) RequireReference(mainRoutine, "mainRoutine", "routine");
    }

    private void Validate()
    {
        if (Raw.GetProperty("schema").GetString() != "edg-semantic/1") throw new InvalidDataException("Unsupported semantic graph schema");
        if (Raw.GetProperty("lowered").GetBoolean()) throw new InvalidDataException("Expected unlowered semantic graph");
        _ = CompleteForEmission;
        foreach (var item in Raw.GetProperty("nodes").EnumerateArray())
        {
            var id = item.GetProperty("id").GetUInt64();
            var kind = item.GetProperty("kind").GetString();
            if (id == 0 || string.IsNullOrWhiteSpace(kind) || !nodes.TryAdd(id, new SemanticNode(this, item)))
                throw new InvalidDataException("Invalid or duplicate node identity: " + id);
            if (!nodesByKind.TryGetValue(kind, out var list)) nodesByKind[kind] = list = new();
            list.Add(nodes[id]);
        }
        ValidateEnvelope();
        var macroIndices = new HashSet<ulong>();
        var realMacroIndices = new HashSet<ulong>();
        foreach (var macro in MacroInvocations)
        {
            var index = macro.GetProperty("index").GetUInt64();
            if (index == 0 || !macroIndices.Add(index)) throw new InvalidDataException("Invalid or duplicate macro invocation index");
            if (macro.GetProperty("definition").GetUInt64() != 0) realMacroIndices.Add(index);
        }
        foreach (var node in nodes.Values)
        {
            foreach (var field in node.Raw.EnumerateObject())
                if (ReferenceFields.Contains(field.Name) && !(node.Kind == "variable" && field.Name == "parameter"))
                    RequireReference(field.Value, $"node {node.Id}.{field.Name}");
            if (node.Raw.TryGetProperty("nativeEdges", out var edges))
                foreach (var edge in edges.EnumerateArray()) RequireReference(edge.GetProperty("target"), $"node {node.Id}.nativeEdges", edge.GetProperty("kind").GetString());
            ValidatePositions(node.Raw, macroIndices.Contains);
        }
        foreach (var macro in MacroInvocations)
        {
            RequireReference(macro.GetProperty("definition"), "macro definition", "macro");
            var parent = macro.GetProperty("parentIndex").GetInt64();
            var index = macro.GetProperty("index").GetUInt64();
            var definition = macro.GetProperty("definition").GetUInt64();
            // EDG stores negative-parent stack-pop placeholders alongside real invocations.
            if ((parent < 0) != (definition == 0)) throw new InvalidDataException("Invalid macro placeholder definition");
            if (parent > 0 && (!macroIndices.Contains((ulong)parent) || (ulong)parent >= index || !realMacroIndices.Contains((ulong)parent)))
                throw new InvalidDataException("Unknown macro parent or invalid parent order " + parent);
            ValidatePositions(macro, macroIndices.Contains);
        }
        foreach (var diagnostic in Diagnostics) RequireReference(diagnostic.GetProperty("node"), "diagnostic.node");
    }

    private void ValidatePositions(JsonElement value, Func<ulong, bool> hasMacro)
    {
        if (value.ValueKind == JsonValueKind.Array) { foreach (var child in value.EnumerateArray()) ValidatePositions(child, hasMacro); return; }
        if (value.ValueKind != JsonValueKind.Object) return;
        if (value.TryGetProperty("sequence", out var sequence))
        {
            _ = sequence.GetUInt64();
            _ = value.GetProperty("column").GetUInt64();
            _ = value.GetProperty("line").GetUInt64();
            RequireReference(value.GetProperty("source"), "source location", "source-file");
        }
        if (value.TryGetProperty("macroContext", out var context))
        {
            var index = context.GetUInt64();
            if (index != 0 && !hasMacro(index)) throw new InvalidDataException("Unknown position macro context " + index);
        }
        foreach (var field in value.EnumerateObject()) ValidatePositions(field.Value, hasMacro);
    }

    public void Dispose() { document?.Dispose(); store?.Dispose(); }
}

/// <summary>Strong identity, common semantic accessors, and lossless JSON for evolving adapters.</summary>
public sealed class SemanticNode
{
    private readonly SemanticGraph graph;
    internal SemanticNode(SemanticGraph graph, JsonElement raw) { this.graph = graph; Raw = raw; }
    public JsonElement Raw { get; }
    public ulong Id => Raw.GetProperty("id").GetUInt64();
    public string Kind => Raw.GetProperty("kind").GetString()!;
    public string? Name => Text("name");
    public string? Form => Text("form");
    public SemanticNode? Type => Reference("type");
    public string? Text(string property) => Raw.TryGetProperty(property, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;
    public bool Flag(string property) => Raw.TryGetProperty(property, out var v) && v.ValueKind == JsonValueKind.True;
    public ulong ReferenceId(string property) => Raw.TryGetProperty(property, out var v) && v.TryGetUInt64(out var id) ? id : 0;
    public SemanticNode? Reference(string property) => graph.Get(ReferenceId(property));
}
