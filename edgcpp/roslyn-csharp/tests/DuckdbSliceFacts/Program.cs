using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;
using System.Text.Json;
using RoslynObj;

const string pinnedPath = "/mnt/storage/roslynobj-duckdb-20261014/export/major2/0316/unit.roslynobj";
const string pinnedSha256 = "cf385364234784aa0d748781d06f74161ad51d28a6a34215449c3ae212810b65";
var arguments = args.Chunk(2).ToArray();
if (arguments.Any(pair => pair.Length != 2 || pair[0] is not ("--input" or "--output")) ||
    arguments.Select(pair => pair[0]).Distinct().Count() != arguments.Length || !arguments.Any(pair => pair[0] == "--output"))
    throw new ArgumentException("DuckdbSliceFacts [--input PINNED_ENTRY_316] --output NEW_REPORT.json");
var inputPath = Path.GetFullPath(arguments.FirstOrDefault(pair => pair[0] == "--input")?[1] ?? pinnedPath);
var outputPath = Path.GetFullPath(arguments.Single(pair => pair[0] == "--output")[1]);
if (File.Exists(outputPath)) throw new ArgumentException("The evidence report must have a fresh path.");
var watch = Stopwatch.StartNew();
var before = Hash(inputPath);
if (before != pinnedSha256) throw new InvalidDataException("This bounded trace accepts only the frozen core entry 316 content hash.");
using var input = RoslynObject.Open(inputPath, new ReadOptions { CachedRowCount = 4096, CachedPageCount = 128, PageSize = 128 });
var probe = new SliceProbe(input);
var report = probe.Build();
var after = Hash(inputPath);
report["input"] = new { path = inputPath, sha256 = before, bytes = new FileInfo(inputPath).Length, afterSha256 = after, unchanged = before == after };
if (before != after) { probe.AddIssue("input-changed", "The input hash changed during the read-only trace."); report["traceComplete"] = false; }
report["elapsedSeconds"] = watch.Elapsed.TotalSeconds;
Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
using (var output = new FileStream(outputPath, FileMode.CreateNew, FileAccess.Write))
    JsonSerializer.Serialize(output, report, new JsonSerializerOptions { WriteIndented = true });
Console.WriteLine($"duckdb-slice trace-complete={report["traceComplete"]}; roots={probe.RootCount}; typed-occurrences={probe.OccurrenceCount}; issues={probe.IssueCount}");
return report["traceComplete"] is true ? 0 : 3;

static string Hash(string path) { using var stream = File.OpenRead(path); return Convert.ToHexStringLower(SHA256.HashData(stream)); }

internal sealed class SliceProbe(RoslynObject input)
{
    private const int NameLimit = 64, SymbolsPerKey = 4, RootLimit = 4, OperationsPerRoot = 256, OccurrencesPerRoot = 4096, DepthLimit = 128;
    private const int DependencyLimit = 256, TypeLimit = 256, FieldsPerType = 64, ParameterLimit = 32, ConstantUnits = 256;
    private const int LocalsPerRoot = 128, RegionsPerRoot = 64, CleanupActionsPerRoot = 128, RecipesPerRoot = 128, FacetsPerNode = 64, EntityLimit = 8192;
    private const long CommandLimit = 20000, RowLimit = 100000;
    private readonly Dictionary<SemanticEntity, string> locators = [];
    private readonly Dictionary<SemanticNode, Dictionary<string, object?>> nodes = [];
    private readonly Dictionary<CppType, Dictionary<string, object?>> types = [];
    private readonly Queue<CppType> pendingTypes = [];
    private readonly Dictionary<FunctionSymbol, Dictionary<string, object?>> dependencies = [];
    private readonly Dictionary<VariableSymbol, Dictionary<string, object?>> storageDependencies = [];
    private readonly Dictionary<Initialization, Dictionary<string, object?>> initializers = [];
    private readonly List<Dictionary<string, object?>> issues = [];
    private readonly List<Dictionary<string, object?>> roots = [];
    private readonly List<Dictionary<string, object?>> selectors = [];
    private string? currentRoot;
    private string? currentPath;
    private HashSet<VariableSymbol> rootStorage = [];
    public int RootCount => roots.Count;
    public int OccurrenceCount { get; private set; }
    public int IssueCount => issues.Count;
    private sealed class TraceBound(string message) : Exception(message);
    private sealed record Edge(string Role, long Ordinal, SemanticNode Child, string StructuralRole, long StructuralOrdinal);
    private static Dictionary<string, object?> Row(params (string Key, object? Value)[] pairs) => pairs.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
    private static string? Number(BigInteger? value) => value?.ToString(CultureInfo.InvariantCulture);

    private string? Ref(SemanticEntity? entity)
    {
        if (entity is null) return null;
        if (locators.TryGetValue(entity, out var locator)) return locator;
        if (locators.Count == EntityLimit) throw new TraceBound("Public identity locator bound exceeded.");
        locator = $"entity/{locators.Count + 1:D6}"; locators.Add(entity, locator); return locator;
    }
    private string? TypeRef(CppType? type)
    {
        if (type is null) return null;
        if (!types.ContainsKey(type))
        {
            if (types.Count == TypeLimit) throw new TraceBound("Related-type inventory bound exceeded.");
            types.Add(type, Row(("locator", Ref(type)), ("pending", true))); pendingTypes.Enqueue(type);
        }
        return Ref(type);
    }
    private T[] Bounded<T>(IEnumerable<T> values, int maximum, string description)
    {
        var result = values.Take(maximum + 1).ToArray();
        if (result.Length > maximum) throw new TraceBound(description + " bound exceeded; omitted items are unknown.");
        return result;
    }
    public void AddIssue(string code, string message, SemanticEntity? entity = null) => issues.Add(Row(
        ("code", code), ("message", message), ("root", currentRoot), ("path", currentPath), ("entity", Ref(entity))));
    private void CheckResources()
    {
        var statistics = input.Statistics;
        if (statistics.Commands > CommandLimit || statistics.RowsRead > RowLimit || statistics.CachedRows > 4096 || statistics.CachedPages > 128 || statistics.MaximumBatchSize > 900)
            throw new TraceBound("Observed public-reader command, row, cache or batch bound exceeded.");
    }

    public Dictionary<string, object?> Build()
    {
        try
        {
            var selected = new List<FunctionSymbol>();
            foreach (var name in new[] { "And", "Or" })
            {
                var matches = Bounded(input.TranslationUnit.SemanticModel.FindSymbols(name), NameLimit, "Exact retained-name materialization");
                if (matches.Any(symbol => symbol.Name != name)) throw new TraceBound("The exact retained-name selector returned a different name.");
                var functions = Bounded(matches.OfType<FunctionSymbol>().Where(function => function.Body is not null), SymbolsPerKey, "Selected symbols per name");
                selectors.Add(Row(("retainedName", name), ("matches", matches.Length), ("selected", functions.Select(Ref).ToArray()),
                    ("selectionOnly", "Exact public retained-name and function-kind selection; no source overload lookup or rebinding.")));
                if (functions.Length != 2) AddIssue("selection-count", "The accepted sample had exactly two defined functions for this retained name.");
                selected.AddRange(functions);
            }
            if (selected.Count != RootLimit || selected.Distinct().Count() != RootLimit) throw new TraceBound("The four distinct accepted sample roots were not reproduced.");
            foreach (var function in selected) TraceRoot(function);
            currentRoot = null; currentPath = "related-types";
            while (pendingTypes.TryDequeue(out var type)) { DescribeType(type); CheckResources(); }
            CheckResources();
        }
        catch (TraceBound error) { AddIssue("truncation-or-bound", error.Message); }
        catch (Exception error) { AddIssue("reader-or-trace-failure", error.Message); }
        var manifest = input.Manifest; var statistics = input.Statistics;
        return Row(("probeVersion", 1), ("traceComplete", issues.Count == 0 && roots.Count == RootLimit),
            ("inventoryOnly", true), ("translatorAccepted", false), ("linkClosureComplete", false), ("generatedExecutionAccepted", false),
            ("scope", "Exactly four retained core-entry-316 And/Or root bodies; one-hop call, cleanup and storage declaration dependencies only. No callee bodies or other translation units are traversed. Trace completeness concerns bounded typed containment, not complete metadata or dependency closure."),
            ("semantics", "Local locators use public semantic equality. Typed containment paths preserve repeated operation identities and retained operands; they are not an evaluation schedule. Call receiver/result/argument facets alias raw operands and add no evaluation. Structural relations are audited independently, never used as a fallback traversal. Metadata associations are deduplicated without inventing ownership or execution; their ordinary cycles do not fail the trace."),
            ("bounds", new { NameLimit, SymbolsPerKey, RootLimit, OperationsPerRoot, OccurrencesPerRoot, DepthLimit, DependencyLimit, TypeLimit, FieldsPerType, ParameterLimit,
                ConstantUnits, LocalsPerRoot, RegionsPerRoot, CleanupActionsPerRoot, RecipesPerRoot, FacetsPerNode, EntityLimit, CommandLimit, RowLimit,
                CachedRows = 4096, CachedPages = 128, PageSize = 128, MaximumBatch = 900,
                NameQueryCaveat = "Take bounds materialization; a paged public name query can still inspect underlying rows. Wall/RSS are controlled separately by the coordinator." }),
            ("manifest", new { manifest.ArtifactIdentity, manifest.FormatMajor, manifest.FormatMinor, manifest.Producer, manifest.EdgRevision, manifest.CaptureProfile, manifest.Stage,
                target = new { manifest.Target.Abi, manifest.Target.AddressableUnitBits, manifest.Target.Endianness, pointerBits = Number(manifest.Target.PointerBits), pointerAlignment = Number(manifest.Target.PointerAlignment) } }),
            ("selectors", selectors), ("roots", roots), ("nodes", nodes.Values.ToArray()), ("initializationAssociationsNotExecution", initializers.Values.ToArray()),
            ("oneHopDependencies", dependencies.Values.ToArray()), ("oneHopStorageDependencies", storageDependencies.Values.ToArray()),
            ("relatedTypesNotLookup", types.Values.ToArray()), ("issues", issues),
            ("statistics", new { statistics.Commands, statistics.RowsRead, statistics.CachedRows, statistics.CachedPages, statistics.MaximumBatchSize }));
    }

    private Dictionary<string, object?> Parameter(SignatureParameter parameter) => Row(("ordinal", parameter.Ordinal), ("role", parameter.Role.Name),
        ("type", TypeRef(parameter.Type)), ("declaredType", TypeRef(parameter.DeclaredType)), ("descriptor", Ref(parameter.Descriptor)),
        ("passedViaCopyConstructor", parameter.Descriptor.Details.PassedViaCopyConstructor), ("baseSubobject", Ref(parameter.BaseSubobject)),
        ("variable", parameter is Parameter execution ? Ref(execution.Symbol) : null), ("isSourceParameter", parameter is Parameter source ? (bool?)source.IsSourceParameter : null));
    private Dictionary<string, object?> Signature(RoutineType signature) => Row(("type", TypeRef(signature)), ("returnType", TypeRef(signature.ReturnType)),
        ("thisClass", TypeRef(signature.ThisClass)), ("hasPrototype", signature.HasPrototype), ("isVariadic", signature.IsVariadic),
        ("hasThisParameter", signature.HasThisParameter), ("returnsValueAsParameter", signature.ReturnsValueAsParameter),
        ("returnsValueViaCopyConstructor", signature.ReturnsValueViaCopyConstructor), ("returnValueParameterFollowsThis", signature.ReturnValueParameterFollowsThis),
        ("callingConvention", signature.CallingConvention?.Name), ("languageLinkage", signature.LanguageLinkage?.Name),
        ("noexceptEffective", signature.ExceptionSpecification?.NoexceptEffective),
        ("parameters", Bounded(signature.Parameters, ParameterLimit, "Signature parameters").Select(Parameter).ToArray()));
    private Dictionary<string, object?> Function(FunctionSymbol function) => Row(("locator", Ref(function)), ("name", function.Name), ("currentName", function.CurrentName),
        ("sourceNameForDisplayOnly", function.SourceCorrespondence?.SourceName), ("capturedLinkageName", function.GnuSupplement?.AssemblyName ?? function.CurrentName),
        ("bodyState", function.BodyState.Name), ("isDefined", function.IsDefined), ("specialFunctionKind", function.SpecialFunctionKind?.Name),
        ("entryKind", function.ConstructorOrDestructorKind?.Name), ("primaryEntry", Ref(function.PrimaryEntry)),
        ("alternateEntries", Bounded(function.AlternateEntryPoints, 16, "Alternate entry associations").Select(Ref).ToArray()),
        ("containingType", TypeRef(function.ContainingType)), ("builtinKind", function.BuiltinKind?.Name), ("signature", Signature(function.Signature)),
        ("executionParameters", Bounded(function.Parameters, ParameterLimit, "Execution parameters").Select(Parameter).ToArray()),
        ("bodyRootAssociationNotTraversal", Ref(function.Body?.Root)));

    private void Dependency(FunctionSymbol function, string context, SemanticEntity owner)
    {
        if (!dependencies.TryGetValue(function, out var row))
        {
            if (dependencies.Count + storageDependencies.Count == DependencyLimit) throw new TraceBound("One-hop selected function/storage dependency bound exceeded.");
            row = Function(function); row["contexts"] = new List<object>(); dependencies.Add(function, row);
        }
        var contexts = (List<object>)row["contexts"]!;
        if (contexts.Count == OccurrencesPerRoot * RootLimit) throw new TraceBound("Dependency context bound exceeded.");
        contexts.Add(new { kind = context, owner = Ref(owner), root = currentRoot, path = currentPath });
    }

    private Dictionary<string, object?> Variable(VariableSymbol variable) => Row(("locator", Ref(variable)), ("name", variable.Name),
        ("type", TypeRef(variable.Type)), ("storage", variable.SymbolDetails.Storage?.Name), ("duration", variable.Details.StorageDuration?.Name),
        ("initializationKind", variable.InitializationKind.Name), ("isThreadLocal", variable.IsThreadLocal),
        ("initializerAssociationNotEvaluation", Initialization(variable.Initializer)), ("constantInitializer", variable.ConstantInitializer is { } constant ? Constant(constant) : null));

    private void TraceRoot(FunctionSymbol function)
    {
        currentRoot = Ref(function); currentPath = "body";
        var row = Function(function); roots.Add(row);
        var body = function.Body!; var occurrences = new List<object>(); row["typedOccurrencesNotSchedule"] = occurrences;
        var locals = Bounded(body.Locals, LocalsPerRoot, "Root locals");
        rootStorage = locals.Concat(function.Parameters.Select(parameter => parameter.Symbol).OfType<VariableSymbol>()).ToHashSet();
        row["locals"] = locals.Select(Variable).ToArray();
        row["scopeAssociation"] = Ref(body.Scope);
        var identities = new HashSet<Operation>();
        Walk(body.Root, "body", null, "body-root", 0, [], identities, occurrences, 0);
        row["operationIdentityCount"] = identities.Count; row["typedOccurrenceCount"] = occurrences.Count;
        row["operatorIdentityCounts"] = identities.GroupBy(operation => operation is OperatorOperation expression ? expression.OperatorKind.Name : operation.Kind.Name)
            .OrderBy(group => group.Key, StringComparer.Ordinal).ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);
        currentPath = "metadata/exception-and-lifetime";
        row["exceptionAndLifetimeMetadataNotNormalExecution"] = CleanupMetadata(function);
        CheckResources();
    }

    private void Walk(SemanticNode node, string path, string? parent, string role, long ordinal, HashSet<SemanticNode> active,
        HashSet<Operation> identities, List<object> occurrences, int depth)
    {
        currentPath = path;
        if (depth > DepthLimit || occurrences.Count == OccurrencesPerRoot) throw new TraceBound("Typed execution containment depth/occurrence bound exceeded.");
        var cycle = !active.Add(node); var metadata = node.RepresentationRole.Name == "metadata";
        occurrences.Add(new { path, parent, role, ordinal, node = Ref(node), metadataBoundary = metadata, cycleBoundary = cycle }); OccurrenceCount++;
        if (cycle) { AddIssue("execution-containment-cycle", "An active typed execution containment path cycles; no schedule was inferred.", node); return; }
        try
        {
            if (metadata) { AddIssue("metadata-on-execution-edge", "A typed execution edge names metadata; it is recorded but never replayed.", node); return; }
            if (node is Operation operation && identities.Add(operation) && identities.Count > OperationsPerRoot) throw new TraceBound("Distinct operation-identity bound exceeded for one root.");
            var edges = TypedEdges(node);
            if (node is CallOperation call && call.Callee is ReferenceOperation { Target: FunctionSymbol direct }) Dependency(direct, "direct-call", call);
            if (node is CallOperation { TargetMethod: { } retainedTarget } retained &&
                (retained.Callee is not ReferenceOperation { Target: FunctionSymbol directTarget } || !directTarget.Equals(retainedTarget)))
                Dependency(retainedTarget, "retained-target-callee-effects-and-order-separate-no-invocation-claim", retained);
            if (node is ThrowOperation { Destructor: { } payloadDestructor } throwing)
                Dependency(payloadDestructor, "retained-throw-payload-destructor-no-body-traversal", throwing);
            if (node is ReferenceOperation { Target: FunctionSymbol function }) Dependency(function, "routine-reference-no-invocation-claim", node);
            if (node is ReferenceOperation { Target: VariableSymbol variable } && !rootStorage.Contains(variable)) StorageDependency(variable, "retained-variable-reference", node);
            if (!nodes.ContainsKey(node)) nodes.Add(node, DescribeNode(node));
            var structural = Bounded(node.ChildRelations, FacetsPerNode, "Structural child audit");
            nodes[node]["structuralAssociationsNotSchedule"] = structural.Select(child => new { role = child.Role.Name, child.Ordinal, child = Ref(child.Child), representationRole = child.Child.RepresentationRole.Name }).ToArray();
            foreach (var child in structural.Where(child => child.Child.RepresentationRole.Name != "metadata" &&
                !edges.Any(edge => edge.StructuralRole == child.Role.Name && edge.StructuralOrdinal == child.Ordinal && edge.Child.Equals(child.Child))))
                AddIssue("unclassified-execution-child", $"Public structural role {child.Role.Name}/{child.Ordinal} is not accounted for by the selected typed facets.", child.Child);
            foreach (var edge in edges) Walk(edge.Child, path + "/" + edge.Role + "[" + edge.Ordinal.ToString(CultureInfo.InvariantCulture) + "]", path, edge.Role, edge.Ordinal,
                active, identities, occurrences, depth + 1);
            CheckResources();
        }
        finally { active.Remove(node); }
    }

    private List<Edge> TypedEdges(SemanticNode node)
    {
        List<Edge> edges = [];
        void Add(string role, SemanticNode? child, long ordinal = 0, string? structuralRole = null, long? structuralOrdinal = null)
        { if (child is not null) edges.Add(new(role, ordinal, child, structuralRole ?? role, structuralOrdinal ?? ordinal)); }
        void Operands(ExpressionOperation expression)
        {
            var operands = Bounded(expression.Operands, FacetsPerNode, "Raw expression operands");
            var relations = Bounded(expression.OperandRelations, FacetsPerNode, "Typed expression operand relations");
            if (relations.Length != operands.Length || relations.Where((relation, index) => !relation.Expression.Equals(operands[index])).Any())
                AddIssue("operand-facet-association", "The typed operand-relation list does not match the public raw operand list; no alternate schedule was inferred.", expression);
            foreach (var relation in relations) Add(relation.Role.Name, relation.Expression, relation.Ordinal);
        }
        switch (node)
        {
            case BlockOperation block:
                foreach (var (statement, index) in Bounded(block.Statements, OccurrencesPerRoot, "Block statements").Select((statement, index) => (statement, index))) Add("statement", statement, index); break;
            case ConditionalOperation conditional: Add("condition", conditional.Condition, structuralRole: "operand"); Add("then", conditional.Then); Add("else", conditional.Else); break;
            case LoopOperation loop: Add("initialization", loop.Initialization); Add("condition", loop.Condition); Add("increment", loop.Increment); Add("body", loop.Body); break;
            case SwitchOperation selectedSwitch: Add("selector", selectedSwitch.Selector); Add("body", selectedSwitch.Body); break;
            case TryOperation selectedTry:
                Add("try-body", selectedTry.Body, structuralRole: "body");
                foreach (var (handler, index) in Bounded(selectedTry.Handlers, 16, "Handlers").Select((handler, index) => (handler, index))) Add("handler-body", handler.Body, index, "handler"); break;
            case InitializationStatementOperation statement: Add("initialization", statement.Initialization.Operation); Initialization(statement.Initialization); break;
            case StatementExpressionResultOperation result: if (result.Initialization is { } resultInitialization) { Add("initialization", resultInitialization.Operation); Initialization(resultInitialization); } break;
            case DeclarationOperation or LabelOperation: break;
            case JumpOperation jump: Add("value", jump.Value, structuralRole: "operand"); break;
            case InitializationOperation initialization:
                Initialization(initialization.Initialization);
                if (initialization.Initialization.Kind.Name == "dik_bitwise_copy") Add("bitwise-source", initialization.Initialization.BitwiseCopySource, structuralRole: "operand");
                else Add("initializer-expression", initialization.Expression, structuralRole: "operand");
                if (initialization.Initialization.Operands.Count != 0) AddIssue("initializer-operand-interpretation", "Additional retained initializer operand roles require interpretation; expression aliases were not replayed.", initialization);
                break;
            case ConditionOperation condition:
                Add("initialization", condition.Initialization); Add("value", condition.Value, structuralRole: "operand"); Initialization(condition.DynamicInitialization);
                if (condition.DynamicInitialization is not null) AddIssue("condition-dynamic-initializer", "Dynamic condition initializer association is recorded without inventing its evaluation relation.", condition); break;
            case LifetimeOperation lifetime: Add("value", lifetime.Value, structuralRole: "body"); break;
            case TemporaryOperation temporary: Initialization(temporary.Initialization); Add("temporary-initialization", temporary.Initialization?.Operation, structuralRole: "initialization"); break;
            case ReuseOperation reuse: Initialization(reuse.Initialization); break;
            case ThrowOperation throwing: Add("throw-operand", throwing.Operand, structuralRole: "operand", structuralOrdinal: 1); Initialization(throwing.Initialization); break;
            case StatementExpressionOperation statement: Add("statement", statement.Statement); break;
            case CallOperation call: Operands(call); break;
            case OperatorOperation operation: Operands(operation); break;
            case LoweredEhOperation lowered: Operands(lowered); break;
            case ReferenceOperation or ConstantOperation: break;
            case StatementOperation { Kind.Name: "stmk_expr" } statement: Add("expression", statement.Expression, structuralRole: "operand"); break;
            case StatementOperation { Kind.Name: "stmk_empty" or "stmk_case" }: break;
            case ExpressionOperation { Kind.Name: "enk_result_of_overriding_function", Operands.Count: 0 }: break;
            default: AddIssue("unknown-typed-execution-form", "No selected typed containment rule for " + node.Kind.Name + "; public structural children remain association-only.", node); break;
        }
        return edges;
    }

    private void StorageDependency(VariableSymbol variable, string context, SemanticEntity owner)
    {
        if (!storageDependencies.TryGetValue(variable, out var row))
        {
            if (dependencies.Count + storageDependencies.Count == DependencyLimit) throw new TraceBound("One-hop selected function/storage dependency bound exceeded.");
            row = Variable(variable); row["contexts"] = new List<object>(); storageDependencies.Add(variable, row);
        }
        var contexts = (List<object>)row["contexts"]!;
        if (contexts.Count == OccurrencesPerRoot * RootLimit) throw new TraceBound("Storage dependency context bound exceeded.");
        contexts.Add(new { kind = context, owner = Ref(owner), root = currentRoot, path = currentPath });
    }

    private Dictionary<string, object?> DescribeNode(SemanticNode node)
    {
        var row = Row(("locator", Ref(node)), ("kind", node.Kind.Name), ("representationRole", node.RepresentationRole.Name), ("isCompilerGenerated", node.IsCompilerGenerated));
        if (node is ExpressionOperation expression)
        {
            row["type"] = TypeRef(expression.Type); row["valueCategory"] = expression.ValueCategory.Name;
            row["volatileFetch"] = expression.Details.VolatileFetch;
            row["rawOperandsNotSchedule"] = Bounded(expression.Operands, FacetsPerNode, "Expression operands").Select(Ref).ToArray();
        }
        if (node is OperatorOperation operation)
            row["operatorFacts"] = new { kind = operation.OperatorKind.Name, operation.Operator.EvalLeftToRight, operation.Operator.EvalRightToLeft,
                operation.Operator.PointerOperandIsSecond, operation.Operator.ReturnsLvalue, operation.Operator.RetainedVirtualCall, operation.Operator.RequiresRuntimeCastCheck };
        switch (node)
        {
            case ReferenceOperation reference:
                row["target"] = Ref(reference.Target);
                row["targetType"] = reference.Target switch { VariableSymbol variable => TypeRef(variable.Type), FieldSymbol field => TypeRef(field.Type), FunctionSymbol function => TypeRef(function.Signature), _ => null };
                break;
            case ConstantOperation constant: row["constant"] = Constant(constant.Constant); break;
            case CallOperation call:
                row["callFacetsNotAdditionalEvaluation"] = new { kind = call.CallKind.Name, callee = Ref(call.Callee), selectedTarget = Ref(call.TargetMethod),
                    selectedFunctionType = TypeRef(call.Call.FunctionType), receiver = Ref(call.Receiver), returnSlot = Ref(call.Call.ReturnSlot),
                    sourceDispatchKind = call.SourceDispatchKind?.Name, sourceTargetAssociationOnly = Ref(call.SourceTargetMethod),
                    arguments = Bounded(call.Arguments, ParameterLimit, "Call arguments").Select(argument => new { argument.Ordinal, role = argument.Role?.Name,
                        parameter = Ref(argument.Parameter), value = Ref(argument.Value), rawOperandIndices = call.Operands.Select((operand, index) => (operand, index)).Where(pair => pair.operand.Equals(argument.Value)).Select(pair => pair.index).ToArray() }).ToArray() };
                row["dependencyClassification"] = call.Callee is ReferenceOperation { Target: FunctionSymbol } ? "direct selected reference" : "indirect callee; target set/provider unresolved by this trace";
                break;
            case InitializationOperation initialization: row["initialization"] = Initialization(initialization.Initialization); break;
            case InitializationStatementOperation initialization: row["initialization"] = Initialization(initialization.Initialization); break;
            case TemporaryOperation temporary: row["temporary"] = new { initialization = Initialization(temporary.Initialization), target = Ref(temporary.Target), sourceType = TypeRef(temporary.SourceType) }; break;
            case LifetimeOperation lifetime: row["lifetimeAssociationNotReplay"] = Ref(lifetime.Lifetime); row["value"] = Ref(lifetime.Value); break;
            case ReuseOperation reuse: row["reuseAssociationNotEvaluation"] = Initialization(reuse.Initialization); break;
            case ConditionOperation condition: row["condition"] = new { value = Ref(condition.Value), initialization = Ref(condition.Initialization), dynamicInitializationAssociation = Initialization(condition.DynamicInitialization), variable = Ref(condition.Variable), scope = Ref(condition.Scope) }; break;
            case DeclarationOperation declaration:
                row["declarationAssociationsNotInitialization"] = Bounded(declaration.Entities, LocalsPerRoot, "Declaration entities").Select(Ref).ToArray(); row["hasStaticOrThreadVariable"] = declaration.HasStaticOrThreadVariable; break;
            case JumpOperation jump: row["jump"] = new { kind = jump.JumpKind.Name, target = Ref(jump.Target), value = Ref(jump.Value), commonLifetimeAssociation = Ref(jump.CommonLifetime) }; break;
            case ThrowOperation throwing: row["throw"] = new { throwing.IsRethrow, operand = Ref(throwing.Operand), thrownType = TypeRef(throwing.ThrownType), initialization = Initialization(throwing.Initialization), destructor = Ref(throwing.Destructor) }; break;
            case LoweredEhOperation marker: row["loweredEhAssociationsNotReplay"] = new { kind = marker.ExceptionKind.Name, initialization = Initialization(marker.Initialization),
                region = Ref(marker.ExceptionOperation.Region), stateNumber = Number(marker.ExceptionOperation.StateNumber), handler = Ref(marker.ExceptionOperation.Handler),
                routine = Ref(marker.ExceptionOperation.Routine), function = Ref(marker.ExceptionOperation.FunctionEh?.Routine), tryBlock = Ref(marker.ExceptionOperation.TryBlock) }; break;
            case TryOperation selectedTry:
                row["handlers"] = Bounded(selectedTry.Handlers, 16, "Handlers").Select(handler => new { handler = Ref(handler), caughtType = TypeRef(handler.CaughtType),
                    parameter = Ref(handler.Parameter), parameterInitializationAssociation = Initialization(handler.ParameterInitialization), body = Ref(handler.Body) }).ToArray(); break;
        }
        return row;
    }

    private string? Initialization(Initialization? initialization)
    {
        if (initialization is null) return null;
        if (initializers.ContainsKey(initialization)) return Ref(initialization);
        if (initializers.Count == RecipesPerRoot * RootLimit) throw new TraceBound("Initialization association inventory bound exceeded.");
        var row = Row(("locator", Ref(initialization))); initializers.Add(initialization, row);
        var flags = initialization.Details;
        row["kind"] = initialization.Kind.Name; row["representationRole"] = initialization.Operation?.RepresentationRole.Name;
        row["type"] = TypeRef(flags.Type); row["target"] = Ref(initialization.Target);
        row["constructor"] = Ref(initialization.Constructor); row["destructor"] = Ref(initialization.Destructor);
        row["expressionAssociation"] = Ref(initialization.Expression); row["bitwiseCopySource"] = Ref(initialization.BitwiseCopySource);
        row["constant"] = initialization.Constant is { } constant ? Constant(constant) : null;
        row["lifetime"] = Ref(initialization.Lifetime); row["initExpressionLifetime"] = Ref(initialization.InitExpressionLifetime);
        row["overlappingLifetime"] = Ref(initialization.OverlappingLifetime); row["masterAssociation"] = Ref(initialization.Master);
        row["nextDestructionAssociation"] = Ref(initialization.NextInDestructionList);
        row["operandsNotSchedule"] = Bounded(initialization.Operands, FacetsPerNode, "Initialization operands").Select(operand => new { role = operand.Role.Name, operand.Ordinal, expression = Ref(operand.Expression) }).ToArray();
        row["flags"] = new { flags.IsUnordered, flags.IsConditional, flags.IsReusedValue, flags.IsOverlapping, flags.IsClassResultMaster, flags.IsStaticTemporary,
            flags.IsPartialAggregate, flags.IsPartialInitialization, flags.HasTemporaryLifetime, flags.IsConstructorInitialization, flags.FollowsExecutableStatement,
            flags.IsFreeingOfExceptionObject, flags.IsConditionalGuard, flags.IsVlaDeallocation, flags.FreesStorage, flags.FreesArray };
        if (initialization.Constructor is { } constructor) Dependency(constructor, "initializer-association-not-independent-call", initialization);
        if (initialization.Destructor is { } destructor) Dependency(destructor, "destruction-recipe-association-not-normal-call", initialization);
        return Ref(initialization);
    }

    private Dictionary<string, object?> CleanupMetadata(FunctionSymbol function)
    {
        var body = function.Body!; var eh = body.ExceptionHandling;
        var regions = Bounded(eh.CleanupRegions, RegionsPerRoot, "Cleanup regions"); var actionCount = 0; var recipeCount = 0;
        var regionRows = new List<object>();
        foreach (var region in regions)
        {
            var actions = Bounded(region.Actions, CleanupActionsPerRoot, "Cleanup actions"); actionCount += actions.Length;
            if (actionCount > CleanupActionsPerRoot) throw new TraceBound("Cleanup action bound exceeded for one root.");
            foreach (var action in actions) if (action.TargetMethod is { } target) Dependency(target, "exception-cleanup-selected-action", action);
            regionRows.Add(new { locator = Ref(region), region.Ordinal, nextRegion = Ref(region.NextRegion), region.Details.IsSupplement, supplementOwner = Ref(region.Details.SupplementOwner),
                condition = region.Condition is { } condition ? new { kind = condition.Kind.Name, comparison = Number(condition.Details.ComparisonValue), storage = Storage(condition.Storage) } : null,
                actions = actions.Select(action => new { locator = Ref(action), kind = action.Kind.Name, target = Ref(action.TargetMethod), storage = Storage(action.Storage),
                    flags = new { action.Details.IsArray, action.Details.IsIndirect, action.Details.IsConditional, action.Details.IsBaseSubobject, action.Details.IsVla, action.Details.IsDeallocation, action.Details.IsLocalStaticGuard },
                    array = action.Array is { } array ? new { locator = Ref(array), count = Number(array.ElementCount), stride = Number(array.ElementSize), handle = Constant(array.HandleConstant) } : null,
                    abi = action.AbiDetail is { } abi ? new { abi.IsDelegating, vtt = Storage(abi.VttLocation), vlaCount = Storage(abi.VlaCountLocation), delegationArgument = Storage(abi.DelegationArgumentLocation) } : null }).ToArray() });
        }
        var lifetimes = Bounded(body.Lifetimes, RegionsPerRoot, "Reachable lifetime associations").Select(lifetime =>
        {
            var recipes = Bounded(lifetime.Cleanups, RecipesPerRoot, "Lifetime cleanup recipes"); recipeCount += recipes.Length;
            if (recipeCount > RecipesPerRoot) throw new TraceBound("Cleanup recipe bound exceeded for one root.");
            return new { locator = Ref(lifetime), kind = lifetime.Kind.Name, boundEntity = Ref(lifetime.BoundEntity), parent = Ref(lifetime.Parent),
                parentCheckpoint = Ref(lifetime.ParentCheckpoint), lifetime.HasImplicitChild, lifetime.HasBlockAfterLabelChild,
                recipesNotSchedule = recipes.Select(recipe => new { recipe.Ordinal, initialization = Initialization(recipe.Initialization), destructor = Ref(recipe.Destructor), recipe.IsConditional, recipe.IsUnordered }).ToArray() };
        }).ToArray();
        return Row(("regionTable", Ref(eh.RegionTable)), ("arrayTable", Ref(eh.ArrayTable)), ("initialState", Number(eh.Details?.InitialState)), ("nullState", Number(eh.Details?.NullState)),
            ("prologue", Ref(eh.Details?.Prologue)), ("epilogues", Bounded(eh.Epilogues, 32, "EH epilogues").Select(Ref).ToArray()), ("regions", regionRows), ("reachableLifetimesNotOwnership", lifetimes));
    }

    private object? Storage(CleanupStorageLocation? storage) => storage is null ? null : new { locator = Ref(storage), kind = storage.Kind.Name,
        variable = Ref(storage.Variable), baseEntity = Ref(storage.Details.BaseEntity), offset = Number(storage.Offset), storage.IsIndirect, storage.UsesThisParameterOffset };

    private Dictionary<string, object?> Constant(Constant constant)
    {
        var row = Row(("locator", Ref(constant)), ("type", TypeRef(constant.Type)), ("originalType", TypeRef(constant.OriginalType)), ("kind", constant.Value.Kind.Name));
        switch (constant.Value)
        {
            case IntegerValue integer: row["value"] = Number(integer.Value); break;
            case AddressValue address:
                row["address"] = new { baseKind = address.BaseKind.Name, baseEntity = Ref(address.BaseEntity), byteOffset = Number(address.ByteOffset), address.OnePastEnd,
                    subobjectPathAssociation = Ref(address.SubobjectPath), address.Details.IsObjectReflection, address.Details.ParameterNumber };
                break;
            case StringValue text:
                row["string"] = new { length = Number(text.Length), text.StorageUnitBits, encoding = text.Encoding.Name, units = Bounded(text.StorageUnits, ConstantUnits, "Constant string units") }; break;
            case AggregateValue aggregate:
                row["aggregateElementsNotExecution"] = Bounded(aggregate.Elements, ConstantUnits, "Aggregate constant elements").Select(element => new { element.Ordinal, type = TypeRef(element.Type),
                    value = Ref(element.Value), kind = element.Value.Kind.Name, integer = element.Value is IntegerValue value ? Number(value.Value) : null,
                    address = element.Value is AddressValue address ? new { baseKind = address.BaseKind.Name, baseEntity = Ref(address.BaseEntity), byteOffset = Number(address.ByteOffset), address.OnePastEnd, path = Ref(address.SubobjectPath) } : null }).ToArray(); break;
            case FloatingValue floating: row["floating"] = new { @class = floating.Details.Class.Name, floating.Details.Negative, floating.Details.Radix, significand = Number(floating.Details.Significand), floating.Details.Exponent }; break;
            case ConstantValue<StackOffsetValueDetails> stack: row["stackOffset"] = new { variable = Ref(stack.Payload.Variable), offset = Number(stack.Payload.Offset) }; break;
            default: row["payloadExpanded"] = false; break;
        }
        return row;
    }

    private void DescribeType(CppType type)
    {
        var row = Row(("locator", Ref(type)), ("kind", type.Kind.Name), ("name", type.Name), ("size", Number(type.Size)), ("alignment", Number(type.Alignment)),
            ("isIncomplete", type.IsIncomplete), ("isDependent", type.IsDependent), ("typeinfoVariableAssociation", Ref(type.Details.TypeinfoVariable)));
        types[type] = row;
        switch (type)
        {
            case TypeReference reference:
                row["underlyingType"] = TypeRef(reference is QualifiedType qualifiedReference ? qualifiedReference.UnderlyingType : reference.UnderlyingType);
                if (reference is QualifiedType qualified) row["qualifiers"] = new { qualified.IsConst, qualified.IsVolatile };
                break;
            case PointerType pointer: row["pointeeType"] = TypeRef(pointer.PointeeType); row["reference"] = pointer.IsReference; row["rvalueReference"] = pointer.IsRValueReference; break;
            case ArithmeticType arithmetic: row["arithmetic"] = new { kind = arithmetic.Arithmetic.ArithmeticKind.Name, arithmetic.Arithmetic.Bits, arithmetic.Arithmetic.Signed }; break;
            case ArrayType array: row["elementType"] = TypeRef(array.ElementType); row["bound"] = Number(array.Bound); row["boundKind"] = array.Array.BoundKind.Name; break;
            case RoutineType signature: row["signature"] = Signature(signature); break;
            case RecordType record:
                row["record"] = new { record.IsPacked, record.HasVirtualBases, record.HasVirtualFunctions, subobjectPartner = TypeRef(record.SubobjectPartner),
                    virtualTableAssociation = Ref(record.VirtualFunctionTableVariable), vttAssociation = Ref(record.VirtualTableTableVariable) };
                row["fields"] = Bounded(record.Fields, FieldsPerType, "Record fields").Select(field => new { locator = Ref(field), field.Name, type = TypeRef(field.Type), field.IsGeneratedBase, field.IsGeneratedVirtualPointer, field.IsBitField }).ToArray();
                row["layout"] = record.Layout is { } layout ? new { size = Number(layout.Size), alignment = Number(layout.Alignment), nonvirtualSize = Number(layout.NonvirtualSize), nonvirtualAlignment = Number(layout.NonvirtualAlignment),
                    fields = Bounded(layout.Fields, FieldsPerType, "Record layout fields").Select(field => new { field.Ordinal, field = Ref(field.Field), offset = Number(field.Offset), bitOffset = Number(field.BitOffset), bitWidth = Number(field.BitWidth), containerOffset = Number(field.ContainerOffset) }).ToArray() } : null;
                row["directBasesNotLookup"] = Bounded(record.Bases, 16, "Direct base associations").Select(baseClass => new { baseClass.Ordinal, type = TypeRef(baseClass.BaseType), baseClass.IsVirtual, subobject = Ref(baseClass.Subobject) }).ToArray();
                row["baseSubobjectsNotLookup"] = Bounded(record.BaseSubobjects, 32, "Base subobjects").Select(subobject => new { locator = Ref(subobject), type = TypeRef(subobject.BaseType), offset = Number(subobject.Offset), subobject.IsVirtual, subobject.IsDirect,
                    vbaseOffsetIndex = Number(subobject.VirtualBaseOffsetIndex), constructionTableIndex = Number(subobject.ConstructionTableIndex), constructionSubarrayIndex = Number(subobject.ConstructionSubarrayIndex) }).ToArray();
                break;
        }
    }
}
