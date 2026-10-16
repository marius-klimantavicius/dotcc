using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;
using System.Text.Json;
using RoslynObj;

const string pinnedPath = "/mnt/storage/roslynobj-duckdb-20261014/export/major2/0316/unit.roslynobj";
const string pinnedSha256 = "cf385364234784aa0d748781d06f74161ad51d28a6a34215449c3ae212810b65";
var arguments = args.Chunk(2).ToArray();
if (arguments.Any(pair => pair.Length != 2 || pair[0] is not ("--input" or "--output" or "--mode")) ||
    arguments.Select(pair => pair[0]).Distinct().Count() != arguments.Length || !arguments.Any(pair => pair[0] == "--output"))
    throw new ArgumentException("DuckdbSliceFacts [--input PINNED_ENTRY_316] [--mode four-roots|accessor|buffer-ref|boolean-conversion|boolean-callee|receiver-layout|accessor-tail] --output NEW_REPORT.json");
var mode = arguments.FirstOrDefault(pair => pair[0] == "--mode")?[1] ?? "four-roots";
if (mode is not ("four-roots" or "accessor" or "buffer-ref" or "boolean-conversion" or "boolean-callee" or "receiver-layout" or "accessor-tail")) throw new ArgumentException("Unknown explicit root-selection mode.");
var inputPath = Path.GetFullPath(arguments.FirstOrDefault(pair => pair[0] == "--input")?[1] ?? pinnedPath);
var outputPath = Path.GetFullPath(arguments.Single(pair => pair[0] == "--output")[1]);
if (File.Exists(outputPath)) throw new ArgumentException("The evidence report must have a fresh path.");
var watch = Stopwatch.StartNew();
var before = Hash(inputPath);
if (before != pinnedSha256) throw new InvalidDataException("This bounded trace accepts only the frozen core entry 316 content hash.");
using var input = RoslynObject.Open(inputPath, new ReadOptions { CachedRowCount = 4096, CachedPageCount = 128, PageSize = 128 });
var probe = new SliceProbe(input, mode != "four-roots", mode == "buffer-ref", mode is "boolean-conversion" or "boolean-callee" or "receiver-layout",
    mode is "boolean-callee" or "receiver-layout", mode == "receiver-layout", mode == "accessor-tail");
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

internal sealed class SliceProbe(RoslynObject input, bool accessorRoot = false, bool bufferReferenceRoot = false, bool booleanConversionRoot = false, bool booleanCalleeRoot = false, bool receiverLayoutRoot = false, bool accessorTailRoot = false)
{
    private const int NameLimit = 64, SymbolsPerKey = 4, ConstantUnits = 256;
    private int RootLimit => accessorTailRoot ? 2 : accessorRoot ? 1 : 4;
    private int OperationsPerRoot => accessorRoot ? 64 : 256;
    private int OccurrencesPerRoot => accessorRoot ? 256 : 4096;
    private int DepthLimit => receiverLayoutRoot ? ReceiverLayoutWrapperDepth : accessorRoot ? 32 : 128;
    private int DependencyLimit => accessorRoot ? 16 : 256;
    private int TypeLimit => accessorRoot ? 64 : 256;
    private int FieldsPerType => receiverLayoutRoot ? ReceiverLayoutFields : accessorRoot ? 32 : 64;
    private int ParameterLimit => accessorRoot ? 16 : 32;
    private const int LocalsPerRoot = 128, RegionsPerRoot = 64, CleanupActionsPerRoot = 128, RecipesPerRoot = 128, FacetsPerNode = 64, EntityLimit = 8192;
    private long CommandLimit => accessorRoot ? 10000 : 20000;
    private long RowLimit => accessorRoot ? 20000 : 100000;
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
    private const int AccessorSelectionRoots = 2, AccessorSelectionOperationsPerRoot = 256, AccessorSelectionCalls = 16;
    private readonly List<Dictionary<string, object?>> accessorSelectionInventory = [];
    private const int BufferParentOperationLimit = 64;
    private Dictionary<string, object?>? bufferReferenceSelection;
    private Dictionary<string, object?>? booleanConversionSelection;
    private Dictionary<string, object?>? booleanCalleeSelection;
    private Dictionary<string, object?>? accessorTailSelection;
    private FunctionSymbol? accessorTailUnsignedTarget;
    private FunctionSymbol? accessorTailPointerTarget;
    private const int ReceiverLayoutRecords = 8, ReceiverLayoutFields = 16, ReceiverLayoutBases = 4, ReceiverLayoutSubobjects = 8, ReceiverLayoutWrapperDepth = 16;
    private CallOperation? receiverLayoutCall;
    private Dictionary<string, object?>? receiverLayoutAudit;
    private readonly Dictionary<RecordType, Dictionary<string, object?>> receiverLayoutRecords = [];
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
            foreach (var name in accessorRoot ? new[] { "GetVectorType" } : new[] { "And", "Or" })
            {
                var matches = Bounded(input.TranslationUnit.SemanticModel.FindSymbols(name), NameLimit, "Exact retained-name materialization");
                if (matches.Any(symbol => symbol.Name != name)) throw new TraceBound("The exact retained-name selector returned a different name.");
                var functions = Bounded(matches.OfType<FunctionSymbol>().Where(function => function.Body is not null), SymbolsPerKey, "Selected symbols per name");
                var selector = Row(("retainedName", name), ("matches", matches.Length), ("selected", functions.Select(Ref).ToArray()),
                    ("selectionOnly", "Exact public retained-name and function-kind selection; no source overload lookup or rebinding."));
                selectors.Add(selector);
                if (accessorRoot)
                {
                    selector["definedCandidates"] = functions.Select(Ref).ToArray();
                    var target = SelectAccessorDependency(functions);
                    selector["selected"] = new[] { Ref(target) };
                    if (accessorTailRoot)
                    {
                        selected.AddRange(SelectAccessorTail(target));
                        continue;
                    }
                    var bodyTarget = bufferReferenceRoot ? SelectBufferReferenceInitializer(target) : booleanConversionRoot ? SelectBooleanConversion(target) : target;
                    selected.Add(booleanCalleeRoot ? SelectBooleanCallee(bodyTarget) : bodyTarget);
                    continue;
                }
                if (!accessorRoot && functions.Length != 2) AddIssue("selection-count", "The accepted sample had exactly two defined functions for this retained name.");
                selected.AddRange(functions);
            }
            if (selected.Count != RootLimit || selected.Distinct().Count() != RootLimit) throw new TraceBound("The selected distinct root set was not reproduced.");
            foreach (var function in selected)
            {
                if (receiverLayoutRoot) { AuditReceiverLayouts(function); continue; }
                TraceRoot(function);
                if (accessorTailRoot) roots[^1]["tailSelectedBodyFactsNotAdmission"] = AccessorTailFacts(function);
                else if (bufferReferenceRoot) roots[^1]["referenceReturnFactsNotAdmission"] = ReferenceReturnFacts(function);
                else if (booleanCalleeRoot) roots[^1]["selectedCalleeFactsNotAdmission"] = BooleanConversionFacts(function);
                else if (booleanConversionRoot) roots[^1]["booleanConversionFactsNotAdmission"] = BooleanConversionFacts(function);
                else if (accessorRoot) roots[^1]["accessorFactsNotAdmission"] = AccessorFacts(function);
            }
            currentRoot = null; currentPath = "related-types";
            while (pendingTypes.TryDequeue(out var type))
            {
                if (receiverLayoutRoot) DescribeReceiverLayoutType(type); else DescribeType(type);
                CheckResources();
            }
            CheckResources();
        }
        catch (TraceBound error) { AddIssue("truncation-or-bound", error.Message); }
        catch (Exception error) { AddIssue("reader-or-trace-failure", error.Message); }
        var manifest = input.Manifest; var statistics = input.Statistics;
        var result = Row(("probeVersion", accessorTailRoot ? 8 : receiverLayoutRoot ? 7 : booleanCalleeRoot ? 6 : booleanConversionRoot ? 5 : bufferReferenceRoot ? 4 : accessorRoot ? 3 : 1), ("traceComplete", issues.Count == 0 && roots.Count == RootLimit),
            ("inventoryOnly", true), ("translatorAccepted", false), ("linkClosureComplete", false), ("generatedExecutionAccepted", false),
            ("scope", receiverLayoutRoot
                ? "Type-only receiver layout audit anchored to the reproduced And/accessor/Boolean/sole-callee identity chain. No typed body trace. Only explicit selected receiver/generated-field-owner, immediate value-field/direct-base records and one partner each receive layout rows; pointer pointees and further layout associations are not expanded. No inferred occupied extent, ownership, full closure, no-throw or translator admission."
                : accessorTailRoot
                ? "Two distinct actual bodies: the unsigned-byte non-Boolean direct callee in the reproduced accessor and the declared ordinary-pointer-returning direct call supplying its receiver. Parent and receiver inventories are selection-only, outside body counts. Shallow types only; no callee recursion, full metadata closure, ownership, closed no-throw or translator admission."
                : booleanCalleeRoot
                ? "One actual sole executable direct-call target body selected from the reproduced Boolean conversion. And, accessor, conversion and nested receiver inventories are selection-only, outside target-body counts. Shallow types only; no callee recursion, full metadata closure, ownership, closed no-throw or translator admission."
                : booleanConversionRoot
                ? "One actual Boolean conversion callee body selected from the typed negated direct-call condition in the reproduced Vector accessor. And and parent identities are selection-only. Types are shallow facts with unexpanded associations; no callee recursion, translation, closed no-throw or ownership admission."
                : bufferReferenceRoot
                ? "One actual direct callee body selected from the reciprocally owned executable local-reference initializer in the reproduced Vector accessor. Parent and And identities are selection-only; no callee-body recursion. Types are explicit shallow facts with unexpanded associations, not metadata/ownership closure or translator admission."
                : accessorRoot
                ? "One core-entry-316 GetVectorType candidate body selected by actual public identity intersection with retained direct calls in a separately bounded And identity inventory. Only that target receives typed body tracing; no callee bodies or other units. Shape comparison is partial evidence, not translator admission or full metadata/dependency closure."
                : "Exactly four retained core-entry-316 And/Or root bodies; one-hop call, cleanup and storage declaration dependencies only. No callee bodies or other translation units are traversed. Trace completeness concerns bounded typed containment, not complete metadata or dependency closure."),
            ("semantics", receiverLayoutRoot
                ? "Local locators use public semantic equality. All operation inventories are selector/receiver identity evidence, never typed body occurrences or an evaluation schedule. Layout and type associations are metadata-only; no body, ownership, occupied-size formula or C++ semantic revalidation is performed."
                : "Local locators use public semantic equality. Typed containment paths preserve repeated operation identities and retained operands; they are not an evaluation schedule. Call receiver/result/argument facets alias raw operands and add no evaluation. Structural relations are audited independently, never used as a fallback traversal. Metadata associations are deduplicated without inventing ownership or execution; their ordinary cycles do not fail the trace."),
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
        if (accessorRoot)
        {
            result["rootSelectionMode"] = accessorTailRoot ? "accessor-tail" : receiverLayoutRoot ? "receiver-layout" : booleanCalleeRoot ? "boolean-callee" : booleanConversionRoot ? "boolean-conversion" : bufferReferenceRoot ? "buffer-ref" : "accessor";
            result["accessorSelectionIdentityInventoryNotSchedule"] = accessorSelectionInventory;
            result["accessorSelectionBounds"] = new { AccessorSelectionRoots, AccessorSelectionOperationsPerRoot, AccessorSelectionCalls,
                NameLimit, SymbolsPerKey, includedInTargetBodyCounters = false,
                semantics = "Public DescendantOperations identity inventory for selection only; metadata excluded by the reader. No order, occurrence schedule or callee-body traversal is inferred." };
            if (bufferReferenceRoot)
            {
                result["bufferReferenceParentSelectionNotBodyTrace"] = bufferReferenceSelection;
                result["bufferReferenceSelectionBounds"] = new { BufferParentOperationLimit, LocalsPerRoot,
                    selectedTargetCount = 1, includesParentInTargetBodyCounters = false };
                result["relatedTypeScope"] = "Shallow receiver/return/execution-symbol/body-expression types and explicit wrapper/pointee chains. Record field/base/partner associations do not enqueue their types; unexpanded edges do not establish ownership or complete dependency closure.";
            }
            if (booleanConversionRoot)
            {
                result["fullRelatedTypeClosure"] = false;
                result["booleanConversionParentSelectionNotBodyTrace"] = booleanConversionSelection;
                result["booleanConversionSelectionBounds"] = new { ParentOperationLimit = BufferParentOperationLimit,
                    selectedTargetCount = 1, includesParentInTargetBodyCounters = false };
                result["relatedTypeScope"] = "Shallow actual/formal receiver, result, execution-symbol and body types; explicit wrapper/pointee/signature chains only. Other record associations stay unexpanded; no full type or no-throw closure is claimed.";
            }
            if (booleanCalleeRoot)
            {
                result["closedNoThrowBodyProof"] = false;
                result["booleanCalleeParentSelectionNotBodyTrace"] = booleanCalleeSelection;
                result["booleanCalleeSelectionBounds"] = new { ParentOperationLimit = BufferParentOperationLimit,
                    ReceiverExpressionIdentityLimit = BufferParentOperationLimit, ReceiverDepthLimit = DepthLimit,
                    selectedTargetCount = 1, includesParentOrReceiverInventoryInTargetBodyCounters = false };
            }
            if (receiverLayoutRoot)
            {
                result["noBodyTrace"] = true;
                result["typedBodyOperationIdentityCount"] = 0;
                result["typedBodyOccurrenceCount"] = 0;
                result["receiverLayoutAuditNotExecution"] = receiverLayoutAudit;
                result["receiverLayoutBounds"] = new { RecordLimit = ReceiverLayoutRecords, TypeLimit,
                    FieldsPerRecord = ReceiverLayoutFields, DirectBasesPerRecord = ReceiverLayoutBases,
                    SubobjectsPerRecord = ReceiverLayoutSubobjects, WrapperDepth = ReceiverLayoutWrapperDepth,
                    includesSelectionInventoryInBodyCounters = false };
                result["relatedTypeScope"] = "Type-only explicit layout seed audit. Related selector descriptors retain identities/representation only; wrapper chains are bounded, pointer pointees never automatically expand, and unseeded record layouts, fields, bases, methods and RTTI remain unexpanded.";
            }
            if (accessorTailRoot)
            {
                result["fullRelatedTypeClosure"] = false;
                result["closedNoThrowBodyProof"] = false;
                result["accessorTailParentSelectionNotBodyTrace"] = accessorTailSelection;
                result["accessorTailSelectionBounds"] = new { ParentIdentityLimit = BufferParentOperationLimit,
                    ReceiverIdentityLimit = BufferParentOperationLimit, ReceiverDepthLimit = DepthLimit,
                    selectedTargetCount = 2, includesParentOrReceiverInventoryInTargetBodyCounters = false };
                result["relatedTypeScope"] = "Combined shallow signature/receiver/body type facts for two selected bodies; explicit wrapper/pointee chains and arithmetic enum associations only. Field/base/partner/pointee layouts are not recursively expanded, and no full type, ownership or no-throw closure is claimed.";
            }
        }
        return result;
    }

    private FunctionSymbol SelectAccessorDependency(FunctionSymbol[] candidates)
    {
        currentPath = "selection/And-direct-target-identity-inventory";
        if (candidates.Length == 0 || candidates.Any(function => !function.IsDefined || function.BodyState.Name != "definition" || function.Body is null))
            throw new TraceBound("Accessor candidates must be actual defined public FunctionSymbols with retained bodies.");
        var matches = Bounded(input.TranslationUnit.SemanticModel.FindSymbols("And"), NameLimit, "And selection name materialization");
        if (matches.Any(symbol => symbol.Name != "And")) throw new TraceBound("The exact And selector returned a different retained name.");
        var owners = Bounded(matches.OfType<FunctionSymbol>().Where(function => function.Body is not null), SymbolsPerKey, "And selection roots");
        if (owners.Length != AccessorSelectionRoots || owners.Distinct().Count() != AccessorSelectionRoots ||
            owners.Any(function => !function.IsDefined || function.BodyState.Name != "definition"))
            throw new TraceBound("The accepted two defined And identity-inventory roots were not reproduced.");
        var candidateSet = candidates.ToHashSet(); var selected = new HashSet<FunctionSymbol>(); var callCount = 0;
        foreach (var owner in owners)
        {
            currentRoot = Ref(owner);
            var operations = Bounded(owner.Body!.DescendantOperations(), AccessorSelectionOperationsPerRoot, "And selection operation identities");
            var row = Row(("caller", Function(owner)), ("operationIdentityCount", operations.Length),
                ("inventoryOnlyNotTypedTraceOrSchedule", true));
            accessorSelectionInventory.Add(row);
            var calls = new List<object>(); row["matchingDirectCallIdentities"] = calls;
            foreach (var call in operations.OfType<CallOperation>())
            {
                if (call.RepresentationRole.Name != "execution" || call.CallKind.Name != "direct" ||
                    call.Callee is not ReferenceOperation { Target: FunctionSymbol target } reference ||
                    reference.RepresentationRole.Name != "execution" || call.TargetMethod?.Equals(target) != true || !candidateSet.Contains(target)) continue;
                if (++callCount > AccessorSelectionCalls) throw new TraceBound("Accessor selection matching-call inventory bound exceeded.");
                selected.Add(target);
                calls.Add(new { call = DescribeNode(call), calleeReference = Ref(reference), calleeTarget = Ref(target),
                    retainedTarget = Ref(call.TargetMethod), sameActualPublicEntity = target.Equals(call.TargetMethod),
                    selectedSignature = Signature(target.Signature) });
            }
            CheckResources();
        }
        currentRoot = null;
        if (selected.Count != 1) throw new TraceBound("The bounded current-input direct-call identity intersection does not select one unique accessor candidate.");
        return selected.Single();
    }

    private FunctionSymbol[] SelectAccessorTail(FunctionSymbol parent)
    {
        currentRoot = Ref(parent); currentPath = "selection/accessor-tail-direct-receiver-context";
        var body = parent.Body ?? throw new TraceBound("The reproduced accessor body is absent.");
        var operations = Bounded(body.DescendantOperations(), BufferParentOperationLimit, "Accessor-tail parent operation identities");
        accessorTailSelection = Row(("parent", Function(parent)), ("operationIdentityCount", operations.Length),
            ("identityInventoryNotSchedule", true), ("parentIncludedInTypedBodyCounters", false));
        var matches = new List<(CallOperation Call, ReferenceOperation Callee, FunctionSymbol Target)>();
        foreach (var candidate in operations.OfType<CallOperation>())
        {
            if (candidate.RepresentationRole.Name != "execution" || candidate.CallKind.Name != "direct" ||
                candidate.Callee is not ReferenceOperation { Target: FunctionSymbol target } callee || callee.RepresentationRole.Name != "execution" ||
                candidate.TargetMethod?.Equals(target) != true || !target.IsDefined || target.BodyState.Name != "definition" || target.Body is null ||
                candidate.Call.FunctionType?.Equals(target.Signature) != true || !candidate.Type.Equals(target.ReturnType) ||
                TypeLeaf(target.ReturnType) is not ArithmeticType { Arithmetic.Bits: 8, Arithmetic.Signed: false, Arithmetic.IsBool: false } ||
                target.ReturnType.Size != 1 || target.ReturnType.Alignment != 1 || candidate.Receiver is null) continue;
            matches.Add((candidate, callee, target));
        }
        accessorTailSelection["unsignedByteDirectCallCandidateCount"] = matches.Count;
        if (matches.Count != 1) throw new TraceBound("The accessor-tail context does not select one actual unsigned-byte non-Boolean direct callee.");
        var selected = matches[0]; var receiver = selected.Call.Receiver!;
        accessorTailSelection["unsignedByteCall"] = DescribeNode(selected.Call);
        accessorTailSelection["unsignedByteCalleeReference"] = DescribeNode(selected.Callee);
        accessorTailSelection["unsignedByteTarget"] = Function(selected.Target);
        accessorTailSelection["receiverRoot"] = Ref(receiver);
        accessorTailSelection["receiverRootType"] = TypeRef(receiver.Type);
        accessorTailSelection["receiverRawOperandIndices"] = selected.Call.Operands.Select((operand, index) => (operand, index))
            .Where(pair => pair.operand.Equals(receiver)).Select(pair => pair.index).ToArray();
        accessorTailSelection["receiverArgumentOrdinals"] = selected.Call.Arguments.Where(argument => argument.Value.Equals(receiver)).Select(argument => argument.Ordinal).ToArray();
        var receiverRows = new List<object>(); accessorTailSelection["receiverExpressionIdentityInventoryNotSchedule"] = receiverRows;
        var seen = new HashSet<ExpressionOperation>(); var active = new HashSet<ExpressionOperation>();
        var pointerCalls = new List<(CallOperation Call, ReferenceOperation Callee, FunctionSymbol Target, string Path)>();
        Inspect(receiver, "receiver", 0);
        accessorTailSelection["receiverExpressionIdentityCount"] = seen.Count;
        accessorTailSelection["declaredOrdinaryPointerCallCandidateCount"] = pointerCalls.Count;
        if (pointerCalls.Count != 1) throw new TraceBound("The actual receiver operand context does not select one declared ordinary-pointer-returning direct callee.");
        var pointerCall = pointerCalls[0];
        if (pointerCall.Target.Equals(selected.Target)) throw new TraceBound("The accessor-tail context does not select two distinct actual function bodies.");
        var declaredPointer = (PointerType)TypeLeaf(pointerCall.Target.ReturnType);
        accessorTailSelection["pointerCallOperandPath"] = pointerCall.Path;
        accessorTailSelection["pointerCall"] = DescribeNode(pointerCall.Call);
        accessorTailSelection["pointerCalleeReference"] = DescribeNode(pointerCall.Callee);
        accessorTailSelection["pointerTarget"] = Function(pointerCall.Target);
        accessorTailSelection["pointerDeclaredResult"] = new { type = TypeRef(pointerCall.Target.ReturnType), declaredPointer.IsReference,
            declaredPointer.IsRValueReference, size = Number(pointerCall.Target.ReturnType.Size), alignment = Number(pointerCall.Target.ReturnType.Alignment),
            pointee = TypeRef(declaredPointer.PointeeType), expressionValueType = TypeRef(pointerCall.Call.Type),
            sameDeclaredAndExpressionType = pointerCall.Target.ReturnType.Equals(pointerCall.Call.Type),
            selectionUsesDeclaredSignatureNotExpressionPointerAppearance = true };
        var formalParameters = Bounded(pointerCall.Target.Signature.Parameters, ParameterLimit, "Accessor-tail pointer callee formals")
            .Where(parameter => parameter.Role.Name == "this_parameter").ToArray();
        var formal = formalParameters.Length == 1 ? formalParameters[0] : null;
        var formalPointer = formal is null ? null : TypeLeaf(formal.Type) as PointerType;
        var actualPointer = pointerCall.Call.Receiver is { } pointerReceiver ? TypeLeaf(pointerReceiver.Type) as PointerType : null;
        accessorTailSelection["pointerCallReceiverFacts"] = new { actualReceiver = Ref(pointerCall.Call.Receiver),
            actualType = TypeRef(pointerCall.Call.Receiver?.Type), actualPointee = TypeRef(actualPointer?.PointeeType),
            formal = formal is null ? null : Parameter(formal), formalType = TypeRef(formal?.Type), formalPointee = TypeRef(formalPointer?.PointeeType),
            sameReceiverTypeIdentity = formal?.Type.Equals(pointerCall.Call.Receiver?.Type) == true,
            samePointeeIdentity = actualPointer is not null && formalPointer is not null && actualPointer.PointeeType.Equals(formalPointer.PointeeType),
            noIdentityNormalization = true };
        accessorTailUnsignedTarget = selected.Target; accessorTailPointerTarget = pointerCall.Target;
        CheckResources(); currentRoot = null;
        return [selected.Target, pointerCall.Target];

        void Inspect(ExpressionOperation expression, string path, int depth)
        {
            if (depth > DepthLimit) throw new TraceBound("Accessor-tail receiver context depth exceeded.");
            if (!active.Add(expression)) throw new TraceBound("The actual accessor-tail receiver operand path cycles; selection is unknown.");
            try
            {
                if (!seen.Add(expression)) return;
                if (seen.Count > BufferParentOperationLimit) throw new TraceBound("Accessor-tail receiver identity inventory bound exceeded.");
                receiverRows.Add(new { operandPath = path, node = DescribeNode(expression) });
                if (expression.RepresentationRole.Name != "execution") throw new TraceBound("An accessor-tail receiver operand context reaches non-execution metadata.");
                if (expression is CallOperation call && call.CallKind.Name == "direct" &&
                    call.Callee is ReferenceOperation { Target: FunctionSymbol target } callee && callee.RepresentationRole.Name == "execution" &&
                    call.TargetMethod?.Equals(target) == true && target.IsDefined && target.BodyState.Name == "definition" && target.Body is not null &&
                    call.Call.FunctionType?.Equals(target.Signature) == true &&
                    TypeLeaf(target.ReturnType) is PointerType { IsReference: false, IsRValueReference: false } &&
                    target.ReturnType.Size == 8 && target.ReturnType.Alignment == 8)
                    pointerCalls.Add((call, callee, target, path));
                var operands = Bounded(expression.Operands, FacetsPerNode, "Accessor-tail receiver raw operands");
                for (var index = 0; index < operands.Length; ++index) Inspect(operands[index], path + $"/operand[{index}]", depth + 1);
            }
            finally { active.Remove(expression); }
        }
    }

    private Dictionary<string, object?> AccessorTailFacts(FunctionSymbol function)
    {
        var leaf = TypeLeaf(function.ReturnType); var arithmetic = leaf as ArithmeticType; var pointer = leaf as PointerType;
        return Row(("selectionRole", function.Equals(accessorTailUnsignedTarget) ? "unsigned-byte-callee" :
                function.Equals(accessorTailPointerTarget) ? "receiver-declared-pointer-callee" : "unknown"),
            ("declaredReturnType", TypeRef(function.ReturnType)), ("declaredReturnLeaf", TypeRef(leaf)),
            ("ordinaryPointerDeclaredReturn", pointer is { IsReference: false, IsRValueReference: false }),
            ("pointeeAssociation", TypeRef(pointer?.PointeeType)),
            ("arithmeticResultDescriptor", arithmetic is null ? null : new { arithmetic.Arithmetic.IsBool, arithmetic.Arithmetic.Bits,
                arithmetic.Arithmetic.Signed, kind = arithmetic.Arithmetic.ArithmeticKind.Name,
                size = Number(function.ReturnType.Size), alignment = Number(function.ReturnType.Alignment) }),
            ("sourceNoexceptFact", function.Signature.ExceptionSpecification?.NoexceptEffective), ("closedNoThrowBodyProof", false),
            ("translationAccepted", false), ("calleeBodiesRecursivelyTraced", false));
    }

    private FunctionSymbol SelectBufferReferenceInitializer(FunctionSymbol parent)
    {
        currentRoot = Ref(parent); currentPath = "selection/executable-local-reference-initializer";
        var body = parent.Body ?? throw new TraceBound("The selected parent accessor body is absent.");
        var operations = Bounded(body.DescendantOperations(), BufferParentOperationLimit, "Parent selection operation identities");
        var locals = Bounded(body.Locals, LocalsPerRoot, "Parent selection locals").ToHashSet();
        var selected = new HashSet<FunctionSymbol>(); var matches = new List<object>();
        bufferReferenceSelection = Row(("parent", Function(parent)), ("operationIdentityCount", operations.Length),
            ("identityInventoryNotSchedule", true), ("matchingInitializerCalls", matches));
        foreach (var statement in operations.OfType<InitializationStatementOperation>())
        {
            var initialization = statement.Initialization;
            var containingBlocks = operations.OfType<BlockOperation>().Where(block => block.Statements.Contains(statement)).ToArray();
            if (containingBlocks.Length != 1 || statement.Kind.Name != "stmk_init" || statement.RepresentationRole.Name != "execution" ||
                initialization.Kind.Name != "dik_expression" || initialization.Target is not VariableSymbol local || !locals.Contains(local) ||
                local.Initializer?.Equals(initialization) != true || initialization.Details.Type?.Equals(local.Type) != true ||
                initialization.Operation is not { RepresentationRole.Name: "execution" } initializerOperation ||
                !initializerOperation.Initialization.Equals(initialization) || TypeLeaf(local.Type) is not PointerType { IsReference: true, IsRValueReference: false } declaredReference ||
                initialization.Expression is not CallOperation call || call.RepresentationRole.Name != "execution" || call.CallKind.Name != "direct" ||
                call.Callee is not ReferenceOperation { Target: FunctionSymbol target } callee || callee.RepresentationRole.Name != "execution" ||
                call.TargetMethod?.Equals(target) != true || !target.IsDefined || target.BodyState.Name != "definition" || target.Body is null ||
                !target.ReturnType.Equals(local.Type) || call.Call.FunctionType?.Equals(target.Signature) != true)
                continue;
            selected.Add(target);
            var expressionPointer = TypeLeaf(call.Type) as PointerType;
            matches.Add(new { parentBlock = Ref(containingBlocks[0]), statement = Ref(statement), initialization = Ref(initialization), kind = initialization.Kind.Name,
                initializerOperation = Ref(initializerOperation), targetLocal = new { locator = Ref(local), local.Name, type = TypeRef(local.Type),
                    initializerAssociationNotEvaluation = Ref(local.Initializer) }, initializerTarget = Ref(initialization.Target),
                initializerType = TypeRef(initialization.Details.Type), expression = Ref(call), expressionType = TypeRef(call.Type),
                selectedReturnType = TypeRef(target.ReturnType), localInitializerNamesSameInitialization = local.Initializer?.Equals(initialization) == true,
                declaredReferenceView = new { type = TypeRef(local.Type), size = Number(local.Type.Size), alignment = Number(local.Type.Alignment),
                    declaredReference.IsReference, declaredReference.IsRValueReference, pointee = TypeRef(declaredReference.PointeeType) },
                expressionValueView = new { type = TypeRef(call.Type), size = Number(call.Type.Size), alignment = Number(call.Type.Alignment),
                    isReference = expressionPointer?.IsReference, isRvalueReference = expressionPointer?.IsRValueReference,
                    pointee = TypeRef(expressionPointer?.PointeeType), sameCapturedPointeeIdentity = expressionPointer?.PointeeType.Equals(declaredReference.PointeeType) == true,
                    sameDeclaredTypeIdentity = call.Type.Equals(local.Type), noIdentityNormalization = true },
                constructorAssociation = Ref(initialization.Constructor), destructorAssociation = Ref(initialization.Destructor),
                lifetimeAssociation = Ref(initialization.Lifetime), initExpressionLifetimeAssociation = Ref(initialization.InitExpressionLifetime),
                call = DescribeNode(call), calleeReference = Ref(callee), calleeTarget = Ref(target), retainedTarget = Ref(call.TargetMethod),
                selectedSignature = Signature(target.Signature) });
        }
        CheckResources(); currentRoot = null;
        if (selected.Count != 1) throw new TraceBound("The actual executable local-reference initializer does not select one unique defined direct callee.");
        return selected.Single();
    }

    private FunctionSymbol SelectBooleanConversion(FunctionSymbol parent)
    {
        currentRoot = Ref(parent); currentPath = "selection/typed-negated-condition-direct-call";
        var body = parent.Body ?? throw new TraceBound("The reproduced parent accessor body is absent.");
        var operations = Bounded(body.DescendantOperations(), BufferParentOperationLimit, "Boolean selection parent operation identities");
        var selected = new HashSet<FunctionSymbol>(); var matches = new List<object>();
        booleanConversionSelection = Row(("parent", Function(parent)), ("operationIdentityCount", operations.Length),
            ("identityInventoryNotSchedule", true), ("matchingTypedConditionCalls", matches));
        foreach (var conditional in operations.OfType<ConditionalOperation>())
        {
            if (conditional.RepresentationRole.Name != "execution" ||
                conditional.Condition is not OperatorOperation { OperatorKind.Name: "eok_not", Operands.Count: 1 } negation ||
                negation.RepresentationRole.Name != "execution" || negation.Operands[0] is not CallOperation call ||
                call.RepresentationRole.Name != "execution" || call.CallKind.Name != "direct" ||
                call.Callee is not ReferenceOperation { Target: FunctionSymbol target } callee || callee.RepresentationRole.Name != "execution" ||
                call.TargetMethod?.Equals(target) != true || target.SpecialFunctionKind?.Name != "sfk_conversion" ||
                !target.IsDefined || target.BodyState.Name != "definition" || target.Body is null ||
                TypeLeaf(target.ReturnType) is not ArithmeticType { Arithmetic.IsBool: true } ||
                !call.Type.Equals(target.ReturnType) || call.Call.FunctionType?.Equals(target.Signature) != true)
                continue;
            selected.Add(target);
            var receiverParameters = Bounded(target.Signature.Parameters, ParameterLimit, "Boolean formal receiver parameters")
                .Where(parameter => parameter.Role.Name == "this_parameter").ToArray();
            var formalReceiver = receiverParameters.Length == 1 ? receiverParameters[0] : null;
            var formalPointer = formalReceiver is null ? null : TypeLeaf(formalReceiver.Type) as PointerType;
            var actualPointer = call.Receiver is { } receiver ? TypeLeaf(receiver.Type) as PointerType : null;
            matches.Add(new { conditional = Ref(conditional), typedCondition = Ref(negation), conditionType = TypeRef(negation.Type),
                callOperand = Ref(call), call = DescribeNode(call), calleeReference = Ref(callee), calleeTarget = Ref(target), retainedTarget = Ref(call.TargetMethod),
                selectedSignature = Signature(target.Signature),
                formalReceiver = formalReceiver is null ? null : Parameter(formalReceiver),
                formalReceiverType = TypeRef(formalReceiver?.Type), formalPointee = TypeRef(formalPointer?.PointeeType),
                actualReceiver = Ref(call.Receiver), actualReceiverType = TypeRef(call.Receiver?.Type), actualPointee = TypeRef(actualPointer?.PointeeType),
                actualReceiverExpression = call.Receiver is { } actualReceiver ? DescribeNode(actualReceiver) : null,
                actualReceiverVariable = call.Receiver is ReferenceOperation { Target: VariableSymbol receiverVariable } ? new {
                    locator = Ref(receiverVariable), type = TypeRef(receiverVariable.Type),
                    initializerAssociationUnexpanded = Ref(receiverVariable.Initializer), valueType = TypeRef(call.Receiver.Type) } : null,
                sameReceiverTypeIdentity = formalReceiver?.Type.Equals(call.Receiver?.Type) == true,
                samePointeeIdentity = formalPointer is not null && actualPointer is not null && formalPointer.PointeeType.Equals(actualPointer.PointeeType),
                samePointeeLeafIdentity = formalPointer is not null && actualPointer is not null && TypeLeaf(formalPointer.PointeeType).Equals(TypeLeaf(actualPointer.PointeeType)),
                noIdentityNormalization = true });
        }
        CheckResources(); currentRoot = null;
        if (selected.Count != 1) throw new TraceBound("The retained typed negated condition does not select one unique defined Boolean conversion target.");
        return selected.Single();
    }

    private FunctionSymbol SelectBooleanCallee(FunctionSymbol parent)
    {
        currentRoot = Ref(parent); currentPath = "selection/conversion-sole-executable-direct-call";
        var body = parent.Body ?? throw new TraceBound("The reproduced Boolean conversion body is absent.");
        var operations = Bounded(body.DescendantOperations(), BufferParentOperationLimit, "Conversion parent operation identities");
        var calls = operations.OfType<CallOperation>().Where(candidate => candidate.RepresentationRole.Name == "execution").ToArray();
        booleanCalleeSelection = Row(("parent", Function(parent)), ("operationIdentityCount", operations.Length),
            ("executableCallIdentityCount", calls.Length), ("identityInventoryNotSchedule", true));
        if (calls.Length != 1) throw new TraceBound("The conversion parent does not retain one sole executable call identity.");
        var call = calls[0];
        if (call.CallKind.Name != "direct" || call.Callee is not ReferenceOperation { Target: FunctionSymbol target } callee ||
            callee.RepresentationRole.Name != "execution" || call.TargetMethod?.Equals(target) != true ||
            !target.IsDefined || target.BodyState.Name != "definition" || target.Body is null || call.Call.FunctionType?.Equals(target.Signature) != true)
            throw new TraceBound("The sole conversion call does not name the same actual defined direct callee and selected signature.");
        if (receiverLayoutRoot) receiverLayoutCall = call;
        var receiverParameters = Bounded(target.Signature.Parameters, ParameterLimit, "Selected callee formal receiver parameters")
            .Where(parameter => parameter.Role.Name == "this_parameter").ToArray();
        var formalReceiver = receiverParameters.Length == 1 ? receiverParameters[0] : null;
        var formalPointer = formalReceiver is null ? null : TypeLeaf(formalReceiver.Type) as PointerType;
        var actualPointer = call.Receiver is { } receiver ? TypeLeaf(receiver.Type) as PointerType : null;
        booleanCalleeSelection["selectedCall"] = DescribeNode(call);
        booleanCalleeSelection["calleeReference"] = DescribeNode(callee);
        booleanCalleeSelection["selectedTarget"] = Ref(target);
        booleanCalleeSelection["selectedSignature"] = Signature(target.Signature);
        booleanCalleeSelection["formalReceiver"] = formalReceiver is null ? null : Parameter(formalReceiver);
        booleanCalleeSelection["formalReceiverType"] = TypeRef(formalReceiver?.Type);
        booleanCalleeSelection["formalPointee"] = TypeRef(formalPointer?.PointeeType);
        booleanCalleeSelection["actualReceiverType"] = TypeRef(call.Receiver?.Type);
        booleanCalleeSelection["actualPointee"] = TypeRef(actualPointer?.PointeeType);
        booleanCalleeSelection["sameReceiverTypeIdentity"] = formalReceiver?.Type.Equals(call.Receiver?.Type) == true;
        booleanCalleeSelection["samePointeeIdentity"] = formalPointer is not null && actualPointer is not null && formalPointer.PointeeType.Equals(actualPointer.PointeeType);
        booleanCalleeSelection["noIdentityNormalization"] = true;
        booleanCalleeSelection["actualReceiverExpressionInventoryNotSchedule"] = call.Receiver is { } actualReceiver ? SelectionReceiverInventory(actualReceiver) : null;
        CheckResources(); currentRoot = null;
        return target;
    }

    private List<Dictionary<string, object?>> SelectionReceiverInventory(ExpressionOperation root)
    {
        var result = new List<Dictionary<string, object?>>(); var seen = new HashSet<ExpressionOperation>(); var active = new HashSet<ExpressionOperation>();
        Walk(root, 0);
        return result;
        void Walk(ExpressionOperation expression, int depth)
        {
            if (depth > DepthLimit || result.Count == BufferParentOperationLimit) throw new TraceBound("Selection receiver expression depth/identity bound exceeded.");
            if (active.Contains(expression)) { AddIssue("receiver-operand-cycle", "The actual receiver operand inventory cycles; no execution schedule was inferred.", expression); return; }
            if (!seen.Add(expression)) return;
            active.Add(expression);
            try
            {
                var row = DescribeNode(expression); result.Add(row);
                if (expression.RepresentationRole.Name != "execution")
                {
                    row["metadataBoundary"] = true;
                    AddIssue("receiver-metadata-boundary", "The actual receiver operand inventory reaches a non-execution occurrence.", expression);
                    return;
                }
                if (expression is ReferenceOperation { Target: FieldSymbol field })
                {
                    var layout = field.ContainingType?.Layout;
                    row["selectedFieldFactsNotAdjustmentInference"] = new { locator = Ref(field), field.Name, type = TypeRef(field.Type),
                        ownerType = TypeRef(field.ContainingType), field.IsGeneratedBase, field.IsGeneratedVirtualPointer, field.IsBitField, field.IsMutable,
                        layouts = layout is null ? null : Bounded(layout.Fields, FieldsPerType, "Receiver field owner layout")
                            .Where(entry => entry.Field.Equals(field)).Select(entry => new { entry.Ordinal, offset = Number(entry.Offset),
                                bitOffset = Number(entry.BitOffset), bitWidth = Number(entry.BitWidth), containerOffset = Number(entry.ContainerOffset) }).ToArray() };
                }
                foreach (var operand in Bounded(expression.Operands, FacetsPerNode, "Selection receiver raw operand identities")) Walk(operand, depth + 1);
            }
            finally { active.Remove(expression); }
        }
    }

    private void AuditReceiverLayouts(FunctionSymbol function)
    {
        currentRoot = Ref(function); currentPath = "type-only/receiver-layout";
        var selected = Function(function);
        selected["bodyTracePerformed"] = false;
        selected["operationIdentityCount"] = 0;
        selected["typedOccurrenceCount"] = 0;
        selected["typedOccurrencesNotSchedule"] = Array.Empty<object>();
        roots.Add(selected);
        var call = receiverLayoutCall ?? throw new TraceBound("The selected receiver-layout call association is absent.");
        var receiverParameters = Bounded(function.Signature.Parameters, ParameterLimit, "Layout receiver parameters")
            .Where(parameter => parameter.Role.Name == "this_parameter").ToArray();
        if (receiverParameters.Length != 1 || TypeLeaf(receiverParameters[0].Type) is not PointerType pointer ||
            TypeLeaf(pointer.PointeeType) is not RecordType receiverRecord ||
            call.Receiver is not OperatorOperation { OperatorKind.Name: "eok_address_of", Operands.Count: 1 } address ||
            address.Operands[0] is not OperatorOperation { OperatorKind.Name: "eok_dot_field", Operands.Count: 2 } basePlace ||
            basePlace.Operands[1] is not ReferenceOperation { Target: FieldSymbol baseField } || !baseField.IsGeneratedBase ||
            basePlace.Operands[0] is not OperatorOperation { OperatorKind.Name: "eok_points_to_field", Operands.Count: 2 } memberPlace ||
            memberPlace.Operands[1] is not ReferenceOperation { Target: FieldSymbol memberField } ||
            baseField.ContainingType is not RecordType baseOwner)
            throw new TraceBound("The selected actual receiver does not expose the approved address/member/generated-base layout associations.");

        var bindings = new[] { ReceiverLayoutField(memberField), ReceiverLayoutField(baseField) };
        var seeds = new List<(RecordType Record, string Reason)> { (receiverRecord, "selected-formal-receiver"), (baseOwner, "actual-generated-base-field-owner") };
        foreach (var baseClass in Bounded(receiverRecord.Bases, ReceiverLayoutBases, "Selected receiver direct-base seeds")
                     .Concat(Bounded(baseOwner.Bases, ReceiverLayoutBases, "Actual base-field owner direct-base seeds")))
        {
            if (TypeLeaf(baseClass.BaseType) is RecordType baseSeedRecord) seeds.Add((baseSeedRecord, "explicit-direct-base"));
            else AddIssue("layout-base-kind-unknown", "The explicit direct-base type is not a record after its captured wrapper chain.", baseClass.BaseType);
        }
        foreach (var field in Bounded(receiverRecord.Fields, ReceiverLayoutFields, "Selected receiver immediate value-field seeds"))
        {
            if (TypeLeaf(field.Type) is RecordType fieldSeedRecord) seeds.Add((fieldSeedRecord, "selected-receiver-immediate-by-value-field"));
        }

        receiverLayoutAudit = Row(("selectedFunction", Ref(function)), ("selectedCallAssociationNotExecution", Ref(call)),
            ("selectedReceiverRecord", TypeRef(receiverRecord)), ("actualGeneratedBaseFieldOwner", TypeRef(baseOwner)),
            ("selectedReceiverRecordMatchesThisClass", function.Signature.ThisClass?.Equals(receiverRecord) == true),
            ("actualGeneratedBaseFieldTypeMatchesReceiverRecord", TypeLeaf(baseField.Type).Equals(receiverRecord)),
            ("actualMemberTypeMatchesGeneratedBaseFieldOwner", TypeLeaf(memberField.Type).Equals(baseOwner)),
            ("selectedFieldBindings", bindings), ("records", receiverLayoutRecords.Values),
            ("bodyTracePerformed", false), ("typedBodyOperationIdentityCount", 0), ("typedBodyOccurrenceCount", 0),
            ("noExecutionOrOwnershipSchedule", true),
            ("occupiedExtentPolicy", "Complete type, nonvirtual layout and explicit subobject-partner quantities remain separate captured facts. BaseSubobject exposes no size quantity; no occupied extent, EBO formula or field-gap size is synthesized. Its absent size is not an audit failure."),
            ("unexpandedPolicy", "Only explicit receiver/field-owner, direct-base and immediate value-field record seeds plus one partner each receive full rows. Further fields/bases/partners and pointer pointees remain associations; no methods, bodies, constants, RTTI or ownership graph is traversed."));
        foreach (var (seedRecord, reason) in seeds)
        {
            AuditReceiverRecord(seedRecord, reason);
            if (seedRecord.SubobjectPartner is { } partner && TypeLeaf(partner) is RecordType partnerSeedRecord)
                AuditReceiverRecord(partnerSeedRecord, "one-explicit-subobject-partner");
        }
        CheckResources(); currentRoot = null;
    }

    private Dictionary<string, object?> ReceiverLayoutField(FieldSymbol field)
    {
        var owner = field.ContainingType;
        var layout = owner?.Layout;
        FieldSymbol[] declared = owner is null ? [] : Bounded(owner.Fields, ReceiverLayoutFields, "Explicit field declaring-owner fields")
            .Where(candidate => candidate.Equals(field)).ToArray();
        FieldLayout[] entries = layout is null ? [] : Bounded(layout.Fields, ReceiverLayoutFields, "Explicit field declaring-owner layout")
            .Where(entry => entry.Field.Equals(field)).ToArray();
        var leaf = TypeLeaf(field.Type);
        return Row(("locator", Ref(field)), ("nameForDisplayOnly", field.Name), ("owner", TypeRef(owner)), ("declaredType", TypeRef(field.Type)),
            ("declaredTypeLeaf", TypeRef(leaf)), ("effectiveSize", Number(field.Type.Size)), ("effectiveAlignment", Number(field.Type.Alignment)),
            ("retainedHelperSize", Number(field.Type.EffectiveSize)), ("retainedHelperAlignment", Number(field.Type.EffectiveAlignment)),
            ("descriptorSize", Number(field.Type.DescriptorSize)), ("descriptorAlignment", Number(field.Type.DescriptorAlignment)),
            ("ownerDeclaresIdenticalField", declared.Length == 1), ("ownerHasOneIdenticalFieldLayout", entries.Length == 1),
            ("layoutEntries", entries.Select(entry => new { entry.Ordinal, field = Ref(entry.Field), owner = Ref(entry.Owner),
                ownerType = TypeRef(entry.Owner.Type), offset = Number(entry.Offset), bitOffset = Number(entry.BitOffset),
                bitWidth = Number(entry.BitWidth), containerOffset = Number(entry.ContainerOffset) }).ToArray()),
            ("flags", new { field.IsGeneratedBase, field.IsGeneratedVirtualPointer, field.IsMutable, field.IsBitField,
                field.IsPacked, field.HasNoUniqueAddressAttribute, explicitAlignment = Number(field.ExplicitAlignment) }),
            ("storageRole", leaf is PointerType ? "pointer-storage-only-pointee-unexpanded" : leaf is RecordType ? "immediate-by-value-record-association" : "captured-value-storage"),
            ("wrapperFactsDescribed", true), ("pointeeGraphExpanded", false));
    }

    private void AuditReceiverRecord(RecordType record, string reason)
    {
        if (receiverLayoutRecords.TryGetValue(record, out var existing))
        {
            var reasons = (List<string>)existing["seedReasons"]!;
            if (!reasons.Contains(reason, StringComparer.Ordinal)) reasons.Add(reason);
            return;
        }
        if (receiverLayoutRecords.Count == ReceiverLayoutRecords) throw new TraceBound("Receiver-layout explicit record bound exceeded.");
        var row = Row(("locator", TypeRef(record)), ("seedReasons", new List<string> { reason }));
        receiverLayoutRecords.Add(record, row);
        var layout = record.Layout;
        row["nameForDisplayOnly"] = record.Name;
        row["typeExtent"] = ReceiverLayoutExtent(record);
        row["recordFlags"] = new { definitionState = record.DefinitionState.Name, record.Record.HasSubobjectType,
            record.IsEmpty, record.IsPacked, record.HasVirtualBases, record.HasVirtualFunctions,
            maximumMemberAlignment = Number(record.MaximumMemberAlignment) };
        var partner = record.SubobjectPartner;
        row["subobjectPartner"] = new { type = TypeRef(partner), sameIdentity = partner?.Equals(record),
            reciprocalIdentity = partner is RecordType partnerRecord ? (bool?)partnerRecord.SubobjectPartner?.Equals(record) : null,
            extent = partner is null ? null : ReceiverLayoutExtent(partner), furtherPartnerTraversal = false };
        row["layout"] = layout is null ? null : new { locator = Ref(layout), type = TypeRef(layout.Type),
            typeIdentityMatches = layout.Type.Equals(record), layout.IsComplete, size = Number(layout.Size), alignment = Number(layout.Alignment),
            nonvirtualSize = Number(layout.NonvirtualSize), nonvirtualAlignment = Number(layout.NonvirtualAlignment),
            fields = Bounded(layout.Fields, ReceiverLayoutFields, "Receiver-layout field rows").Select(entry => new {
                entry.Ordinal, field = Ref(entry.Field), offset = Number(entry.Offset), bitOffset = Number(entry.BitOffset),
                bitWidth = Number(entry.BitWidth), containerOffset = Number(entry.ContainerOffset) }).ToArray() };
        row["fields"] = Bounded(record.Fields, ReceiverLayoutFields, "Receiver-layout explicit fields").Select(ReceiverLayoutField).ToArray();
        row["directBases"] = Bounded(record.Bases, ReceiverLayoutBases, "Receiver-layout direct bases").Select(baseClass => new {
            baseClass.Ordinal, owner = TypeRef(baseClass.Owner), baseType = TypeRef(baseClass.BaseType), baseClass.IsVirtual,
            access = baseClass.Access?.Name, subobject = Ref(baseClass.Subobject),
            subobjectBaseTypeMatches = baseClass.Subobject?.BaseType.Equals(baseClass.BaseType),
            furtherBaseLayoutExpansion = false }).ToArray();
        row["baseSubobjects"] = Bounded(record.BaseSubobjects, ReceiverLayoutSubobjects, "Receiver-layout base subobjects")
            .Select(ReceiverLayoutSubobject).ToArray();
        row["primaryBaseSubobjectAssociationUnexpanded"] = Ref(record.PrimaryBaseSubobject);
        row["virtualTableAssociationUnexpanded"] = Ref(record.VirtualFunctionTableVariable);
        row["vttAssociationUnexpanded"] = Ref(record.VirtualTableTableVariable);
        if (layout is null) AddIssue("selected-layout-unavailable", "An explicit record seed has no retained layout; its facts remain unknown.", record);
        CheckResources();
    }

    private Dictionary<string, object?> ReceiverLayoutSubobject(BaseSubobject subobject)
    {
        return Row(("locator", Ref(subobject)), ("mostDerivedType", TypeRef(subobject.MostDerivedType)),
            ("baseType", TypeRef(subobject.BaseType)), ("originalType", TypeRef(subobject.OriginalType)),
            ("offset", Number(subobject.Offset)), ("directBaseNumber", subobject.DirectBaseNumber),
            ("completeSubobject", subobject.IsCompleteSubobject),
            ("flags", new { subobject.IsVirtual, subobject.IsDirect, subobject.IsAmbiguous, subobject.IsOptimizedEmptyBase,
                subobject.IsPackExpansion, subobject.SharesVirtualFunctionInfo, subobject.HasPublicDerivation }),
            ("retainedBaseTypeExtentNotOccupiedSize", ReceiverLayoutExtent(subobject.BaseType)),
            ("occupiedSize", null), ("occupiedSizeSynthesized", false),
            ("occupiedSizeStatus", "No public BaseSubobject size quantity; separate type/nonvirtual/partner facts are retained without an inferred extent."),
            ("primaryBaseSubobjectAssociationUnexpanded", Ref(subobject.PrimaryBaseSubobject)),
            ("dataSectionBaseSubobjectAssociationUnexpanded", Ref(subobject.DataSectionBaseSubobject)),
            ("virtualBasePointerBaseSubobjectAssociationUnexpanded", Ref(subobject.VirtualBasePointerBaseSubobject)),
            ("virtualBasePointerOffset", Number(subobject.VirtualBasePointerOffset)),
            ("virtualTableAssociationUnexpanded", Ref(subobject.VirtualFunctionTableVariable)));
    }

    private Dictionary<string, object?> ReceiverLayoutExtent(CppType type)
    {
        var layout = type is RecordType record ? record.Layout : null;
        return Row(("type", TypeRef(type)), ("size", Number(type.Size)), ("alignment", Number(type.Alignment)),
            ("effectiveSize", Number(type.EffectiveSize)), ("effectiveAlignment", Number(type.EffectiveAlignment)),
            ("descriptorSize", Number(type.DescriptorSize)), ("descriptorAlignment", Number(type.DescriptorAlignment)),
            ("sizeKind", type.Details.SizeKind.Name), ("isIncomplete", type.IsIncomplete), ("descriptorIncomplete", type.DescriptorIsIncomplete),
            ("layoutIsComplete", layout?.IsComplete), ("nonvirtualSize", Number(layout?.NonvirtualSize)),
            ("nonvirtualAlignment", Number(layout?.NonvirtualAlignment)), ("role", "captured-type-quantity-not-a-synthesized-occupied-extent"));
    }

    private void DescribeReceiverLayoutType(CppType type)
    {
        var row = ReceiverLayoutExtent(type);
        row["locator"] = Ref(type); row["kind"] = type.Kind.Name; row["nameForDisplayOnly"] = type.Name;
        row["descriptionScope"] = "Type-only descriptor facts. Only explicit record seeds receive grouped layout rows; pointer pointees and other record associations are not enqueued.";
        types[type] = row;
        switch (type)
        {
            case TypeReference reference:
                var underlying = reference is QualifiedType qualifiedReference ? qualifiedReference.UnderlyingType : reference.UnderlyingType;
                row["underlyingType"] = Ref(underlying);
                row["underlyingTypeDescribed"] = types.ContainsKey(underlying);
                if (reference is QualifiedType qualified)
                {
                    var facts = qualified.Qualification;
                    row["qualifiers"] = new { qualified.IsConst, qualified.IsVolatile, facts.IsAtomic,
                        addressSpace = facts.AddressSpace?.ToString(CultureInfo.InvariantCulture), facts.IsShared,
                        facts.IsUpcStrict, facts.IsUpcRelaxed, facts.IsNear, facts.IsFar, facts.IsUnaligned };
                }
                break;
            case PointerType pointer:
                row["pointeeAssociationUnexpanded"] = Ref(pointer.PointeeType);
                row["reference"] = pointer.IsReference; row["rvalueReference"] = pointer.IsRValueReference;
                row["pointerPointeeTraversalPerformed"] = false;
                break;
            case RecordType record:
                row["explicitLayoutAuditIncluded"] = receiverLayoutRecords.ContainsKey(record);
                row["subobjectPartnerAssociation"] = Ref(record.SubobjectPartner);
                row["otherRecordAssociationsExpanded"] = false;
                break;
            case ArithmeticType arithmetic:
                row["arithmetic"] = new { kind = arithmetic.Arithmetic.ArithmeticKind.Name, arithmetic.Arithmetic.Bits, arithmetic.Arithmetic.Signed };
                break;
            default: row["otherTypeAssociationsExpanded"] = false; break;
        }
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

    private Dictionary<string, object?> AccessorFacts(FunctionSymbol function)
    {
        currentRoot = Ref(function); currentPath = "metadata/accessor-shape";
        var body = function.Body ?? throw new TraceBound("The uniquely selected accessor body is absent.");
        var parameters = Bounded(function.Parameters, ParameterLimit, "Accessor execution parameters");
        var selected = Bounded(function.Signature.Parameters, ParameterLimit, "Accessor signature parameters");
        var associationsMatch = body.Function.Equals(function) && parameters.Length == selected.Length && parameters.Select((parameter, index) =>
            parameter.Ordinal == index && selected[index].Ordinal == index && parameter.Descriptor.Equals(selected[index].Descriptor) &&
            parameter.Role.Equals(selected[index].Role) && parameter.Type.Equals(selected[index].Type)).All(match => match);
        if (!associationsMatch) AddIssue("accessor-signature-body-association", "The actual selected body and execution/signature parameter associations differ.", function);
        var receivers = parameters.Where(parameter => parameter.Role.Name == "this_parameter").ToArray();
        var receiver = receivers.Length == 1 ? receivers[0] : null;
        var pointer = receiver is not null ? TypeLeaf(receiver.Type) as PointerType : null;
        var pointee = pointer?.PointeeType;
        var record = pointee is not null ? TypeLeaf(pointee) as RecordType : null;
        var returned = body.Root is BlockOperation { Statements.Count: 1 } block && block.Statements[0] is JumpOperation { JumpKind.Name: "return" } jump ? jump : null;
        var read = returned?.Value is OperatorOperation { OperatorKind.Name: "eok_points_to_field", Operands.Count: 2 } fieldRead ? fieldRead : null;
        var field = read?.Operands[1] is ReferenceOperation { Target: FieldSymbol member } ? member : null;
        var receiverRead = read?.Operands[0] as ReferenceOperation;
        var resultLeaf = TypeLeaf(function.ReturnType);
        var layout = record?.Layout;
        FieldLayout[] fieldLayouts = layout is null || field is null ? [] : Bounded(layout.Fields, FieldsPerType, "Accessor record layout")
            .Where(entry => entry.Field.Equals(field)).ToArray();
        return Row(("identityProvenance", "Current-input public FindSymbols candidates intersect the exact same FunctionSymbol in retained direct callee references and TargetMethod facets of the bounded And identity inventory. Prior trace locators are never persistent handles."),
            ("signatureBodyAssociationsMatch", associationsMatch),
            ("methodFacts", new { function.IsVirtual, function.IsPureVirtual, thisAdjustment = Number(function.ThisAdjustment),
                returnAdjustment = Number(function.ReturnAdjustment), virtualCallIndex = Number(function.VirtualCallIndex), virtualBaseIndex = Number(function.VirtualBaseIndex),
                overridingWrapperTarget = Ref(function.OverridingFunctionForWrapper), overriddenWrapperTarget = Ref(function.OverriddenFunctionForWrapper),
                function.Details.IsWeak, function.Details.IsWeakref, function.Details.IsGnuAlias, function.Details.IsIfunc, function.Details.IsAliasEntry }),
            ("receiver", receiver is null ? null : new { parameter = Parameter(receiver),
                executionSymbol = receiver.Symbol is { } symbol ? Variable(symbol) : null,
                executionSymbolType = TypeRef(receiver.Symbol?.Type), valuePointerType = TypeRef(pointer), pointeeType = TypeRef(pointee), containingRecord = TypeRef(record),
                executionSymbolHasConstPointerObjectWrapper = receiver.Symbol?.Type is QualifiedType { IsConst: true, IsVolatile: false } wrapper && wrapper.UnderlyingType.Equals(receiver.Type) }),
            ("return", new { statement = Ref(returned), value = Ref(returned?.Value), actualType = TypeRef(function.ReturnType), leafType = TypeRef(resultLeaf), leafKind = resultLeaf.Kind.Name,
                arithmeticDescriptor = resultLeaf is ArithmeticType, isNaturalUnsignedByteArithmetic = resultLeaf is ArithmeticType { Kind.Name: "tk_integer", Arithmetic.Bits: 8, Arithmetic.Signed: false, Arithmetic.IsBool: false } &&
                    function.ReturnType.Size == 1 && function.ReturnType.Alignment == 1 && resultLeaf.Size == 1 && resultLeaf.Alignment == 1 }),
            ("fieldRead", read is null ? null : new { operation = Ref(read), valueCategory = read.ValueCategory.Name, type = TypeRef(read.Type), receiverOccurrence = Ref(receiverRead),
                actualReceiverVariable = Ref(receiverRead?.Target), receiverValueType = TypeRef(receiverRead?.Type),
                readsSelectedReceiver = receiver?.Symbol is { } receiverSymbol && receiverRead?.Target?.Equals(receiverSymbol) == true,
                field = Ref(field), fieldType = TypeRef(field?.Type), containingType = TypeRef(field?.ContainingType),
                exactReturnType = field?.Type.Equals(function.ReturnType) == true && read.Type.Equals(function.ReturnType),
                layouts = fieldLayouts.Select(entry => new { entry.Ordinal, offset = Number(entry.Offset), bitOffset = Number(entry.BitOffset), bitWidth = Number(entry.BitWidth) }).ToArray() }),
            ("currentOrdinaryMethodsRulePartialComparison", new {
                translatorAccepted = false, admissionChecked = false,
                rule = "Direct generated this-only const ordinary-record unsigned-8 field-return rule; no bases, virtuals or occupied partner. This inspection does not invoke that validator or admit callers.",
                thisOnlySignature = function.Signature.HasThisParameter && selected.Length == 1 && selected[0].Role.Name == "this_parameter",
                constNaturalReceiver = pointer is { IsReference: false, IsRValueReference: false, Size: { } pointerSize, Alignment: { } pointerAlignment } && pointerSize == 8 && pointerAlignment == 8 &&
                    pointee is QualifiedType { IsConst: true, IsVolatile: false } && record is not null && !record.IsIncomplete,
                completeOrdinaryRecordShape = record is not null && record.Kind.Name is ("tk_struct" or "tk_class") && !record.IsIncomplete && !record.IsPacked &&
                    !record.HasVirtualBases && !record.HasVirtualFunctions && record.Bases.Count == 0 && record.BaseSubobjects.Count == 0 &&
                    (record.SubobjectPartner is null || record.SubobjectPartner.Equals(record)) && layout is { IsComplete: true } &&
                    layout.NonvirtualSize == record.Size && layout.NonvirtualAlignment == record.Alignment,
                directFieldReturnShape = returned is not null && read is not null && field is not null,
                noCallsInTraversedBody = !nodes.Keys.OfType<CallOperation>().Any(),
                noLifetimeAssociations = body.Lifetimes.Count == 0,
                remainingChecks = "Full policy, qualification, storage eligibility, native-address exclusions and actual caller contexts remain emitter obligations. A matching partial fact is not translation acceptance." }));
    }

    private Dictionary<string, object?> ReferenceReturnFacts(FunctionSymbol function)
    {
        currentRoot = Ref(function); currentPath = "metadata/reference-return-shallow-facts";
        var body = function.Body ?? throw new TraceBound("The actual selected reference-return body is absent.");
        var parameters = Bounded(function.Parameters, ParameterLimit, "Reference-return execution parameters");
        var selected = Bounded(function.Signature.Parameters, ParameterLimit, "Reference-return signature parameters");
        var associationsMatch = body.Function.Equals(function) && parameters.Length == selected.Length && parameters.Select((parameter, index) =>
            parameter.Ordinal == index && selected[index].Ordinal == index && parameter.Descriptor.Equals(selected[index].Descriptor) &&
            parameter.Role.Equals(selected[index].Role) && parameter.Type.Equals(selected[index].Type)).All(match => match);
        if (!associationsMatch) AddIssue("reference-signature-body-association", "The actual reference-return body and selected parameter associations differ.", function);
        var result = TypeLeaf(function.ReturnType);
        return Row(("translatorAccepted", false), ("ownershipOrLifetimeContractEstablished", false), ("signatureBodyAssociationsMatch", associationsMatch),
            ("result", new { type = TypeRef(function.ReturnType), leafType = TypeRef(result), kind = result.Kind.Name,
                isLvalueReference = result is PointerType { IsReference: true, IsRValueReference: false },
                pointeeType = result is PointerType pointer ? TypeRef(pointer.PointeeType) : null }),
            ("executionParameters", parameters.Select(parameter => new { parameter = Parameter(parameter), symbol = Ref(parameter.Symbol),
                symbolType = TypeRef(parameter.Symbol?.Type),
                symbolHasConstPointerObjectWrapper = parameter.Symbol?.Type is QualifiedType { IsConst: true, IsVolatile: false } wrapper && wrapper.UnderlyingType.Equals(parameter.Type) }).ToArray()),
            ("returnOccurrences", nodes.Keys.OfType<JumpOperation>().Where(jump => jump.JumpKind.Name == "return").Select(jump => new {
                statement = Ref(jump), value = Ref(jump.Value), type = TypeRef(jump.Value?.Type), valueCategory = jump.Value?.ValueCategory.Name,
                target = Ref(jump.Target), commonLifetimeAssociation = Ref(jump.CommonLifetime) }).ToArray()),
            ("fieldReferences", nodes.Keys.OfType<ReferenceOperation>().Where(reference => reference.Target is FieldSymbol).Select(reference =>
            {
                var field = (FieldSymbol)reference.Target!;
                var layout = field.ContainingType?.Layout;
                return new { occurrence = Ref(reference), valueCategory = reference.ValueCategory.Name, valueType = TypeRef(reference.Type), field = Ref(field),
                    field.Name, fieldType = TypeRef(field.Type), containingTypeAssociation = Ref(field.ContainingType),
                    field.IsBitField, field.IsGeneratedBase, field.IsGeneratedVirtualPointer,
                    offsets = layout is null ? null : Bounded(layout.Fields, FieldsPerType, "Returned-field owner layout").Where(entry => entry.Field.Equals(field))
                        .Select(entry => new { entry.Ordinal, offset = Number(entry.Offset), bitOffset = Number(entry.BitOffset), bitWidth = Number(entry.BitWidth) }).ToArray() };
            }).ToArray()),
            ("limits", "Only current public types/occurrences and returned-field offsets are described. Reference representation alone proves neither a shared_ptr copy nor ownership, release, escape safety or a caller lifetime. No admission rule is evaluated."));
    }

    private Dictionary<string, object?> BooleanConversionFacts(FunctionSymbol function)
    {
        currentRoot = Ref(function); currentPath = "metadata/boolean-conversion-shallow-facts";
        var parameters = Bounded(function.Parameters, ParameterLimit, "Boolean execution parameters");
        var selected = Bounded(function.Signature.Parameters, ParameterLimit, "Boolean signature parameters");
        var associationsMatch = function.Body?.Function.Equals(function) == true && parameters.Length == selected.Length &&
            parameters.Select((parameter, index) => parameter.Ordinal == index && selected[index].Ordinal == index &&
                parameter.Descriptor.Equals(selected[index].Descriptor) && parameter.Role.Equals(selected[index].Role) && parameter.Type.Equals(selected[index].Type)).All(match => match);
        if (!associationsMatch) AddIssue("boolean-signature-body-association", "The actual conversion body and selected parameter associations differ.", function);
        var result = TypeLeaf(function.ReturnType);
        return Row(("translatorAccepted", false), ("closedNoThrowBodyProof", false), ("signatureBodyAssociationsMatch", associationsMatch),
            ("specialFunctionKind", function.SpecialFunctionKind?.Name), ("capturedNoexceptEffective", function.Signature.ExceptionSpecification?.NoexceptEffective),
            ("result", new { type = TypeRef(function.ReturnType), leafType = TypeRef(result), kind = result.Kind.Name,
                arithmetic = result is ArithmeticType arithmetic ? new { kind = arithmetic.Arithmetic.ArithmeticKind.Name,
                    arithmetic.Arithmetic.IsBool, arithmetic.Arithmetic.Bits, arithmetic.Arithmetic.Signed, size = Number(result.Size), alignment = Number(result.Alignment) } : null }),
            ("executionParameters", parameters.Select(parameter => new { parameter = Parameter(parameter), symbol = Ref(parameter.Symbol),
                symbolType = TypeRef(parameter.Symbol?.Type),
                constPointerObjectWrapper = parameter.Symbol?.Type is QualifiedType { IsConst: true, IsVolatile: false } wrapper && wrapper.UnderlyingType.Equals(parameter.Type) }).ToArray()),
            ("limits", "The captured conversion role and noexcept fact do not establish callee closure, source lookup, ownership or translator admission. Only this selected body is traced; declared dependencies are not expanded."));
    }

    private CppType TypeLeaf(CppType type)
    {
        var seen = new HashSet<CppType>();
        for (var depth = 0; ; ++depth)
        {
            if (depth > DepthLimit || !seen.Add(type)) throw new TraceBound("Accessor type-wrapper depth/cycle bound exceeded.");
            TypeRef(type);
            if (type is not TypeReference reference) return type;
            type = reference is QualifiedType qualified ? qualified.UnderlyingType : reference.UnderlyingType;
        }
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
        if (bufferReferenceRoot || booleanConversionRoot || accessorTailRoot)
        {
            DescribeShallowReferenceType(type, row);
            if (accessorTailRoot && type is ArithmeticType value)
            {
                row["arithmeticStorageFacts"] = new { value.Arithmetic.IsBool, value.Arithmetic.Bits, value.Arithmetic.Signed,
                    kind = value.Arithmetic.ArithmeticKind.Name, effectiveSize = Number(type.Size), effectiveAlignment = Number(type.Alignment) };
                row["enumDeclarationAssociationsNotLookup"] = Bounded(type.DeclarationSymbols, ParameterLimit, "Accessor-tail arithmetic enum associations")
                    .OfType<EnumSymbol>().Select(enumeration => new { locator = Ref(enumeration), type = TypeRef(enumeration.Type),
                        underlyingType = TypeRef(enumeration.UnderlyingType), explicitUnderlyingType = TypeRef(enumeration.ExplicitUnderlyingType), enumeration.HasExplicitBase }).ToArray();
                row["affiliatedEnumTypeAssociation"] = Ref(value.Arithmetic.AffiliatedEnumType);
            }
            return;
        }
        if (accessorRoot)
        {
            row["arithmeticDescriptor"] = type is ArithmeticType;
            row["retainedDescriptorRepresentation"] = new { size = Number(type.DescriptorSize), alignment = Number(type.DescriptorAlignment), type.Details.IsAtomic };
            row["enumDeclarationAssociationsNotLookup"] = Bounded(type.DeclarationSymbols, ParameterLimit, "Related type declaration associations")
                .OfType<EnumSymbol>().Select(enumeration => new { locator = Ref(enumeration), type = TypeRef(enumeration.Type),
                    underlyingType = TypeRef(enumeration.UnderlyingType), explicitUnderlyingType = TypeRef(enumeration.ExplicitUnderlyingType), enumeration.HasExplicitBase }).ToArray();
            if (type is ArithmeticType value) row["affiliatedEnumType"] = TypeRef(value.Arithmetic.AffiliatedEnumType);
            if (type is RecordType complete) row["layoutComplete"] = complete.Layout?.IsComplete;
        }
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

    private void DescribeShallowReferenceType(CppType type, Dictionary<string, object?> row)
    {
        row["descriptionScope"] = "Shallow fact row; only explicit wrapper/pointee/signature chains enqueue types. Record member/base/partner associations are not expanded.";
        row["retainedDescriptorRepresentation"] = new { size = Number(type.DescriptorSize), alignment = Number(type.DescriptorAlignment), type.Details.IsAtomic };
        switch (type)
        {
            case TypeReference reference:
                row["underlyingType"] = TypeRef(reference is QualifiedType qualifiedReference ? qualifiedReference.UnderlyingType : reference.UnderlyingType);
                if (reference is QualifiedType qualified)
                {
                    var facts = qualified.Qualification;
                    row["qualifiers"] = new { qualified.IsConst, qualified.IsVolatile, facts.IsAtomic,
                        addressSpace = facts.AddressSpace?.ToString(CultureInfo.InvariantCulture), facts.IsShared, facts.IsUpcStrict, facts.IsUpcRelaxed,
                        facts.IsNear, facts.IsFar, facts.IsUnaligned };
                }
                break;
            case PointerType pointer:
                row["pointeeType"] = TypeRef(pointer.PointeeType); row["reference"] = pointer.IsReference; row["rvalueReference"] = pointer.IsRValueReference;
                break;
            case ArithmeticType arithmetic:
                row["arithmetic"] = new { kind = arithmetic.Arithmetic.ArithmeticKind.Name, arithmetic.Arithmetic.Bits, arithmetic.Arithmetic.Signed };
                break;
            case RoutineType routine: row["signature"] = Signature(routine); break;
            case RecordType record:
                row["record"] = new { record.IsPacked, record.HasVirtualBases, record.HasVirtualFunctions,
                    subobjectPartnerAssociationUnexpanded = Ref(record.SubobjectPartner), virtualTableAssociationUnexpanded = Ref(record.VirtualFunctionTableVariable),
                    vttAssociationUnexpanded = Ref(record.VirtualTableTableVariable), fieldTypesEnqueued = false };
                row["fieldsUnexpanded"] = Bounded(record.Fields, FieldsPerType, "Shallow record fields").Select(field => new { locator = Ref(field), field.Name,
                    typeAssociationUnexpanded = Ref(field.Type), field.IsGeneratedBase, field.IsGeneratedVirtualPointer, field.IsBitField }).ToArray();
                row["layout"] = record.Layout is { } layout ? new { layout.IsComplete, size = Number(layout.Size), alignment = Number(layout.Alignment),
                    nonvirtualSize = Number(layout.NonvirtualSize), nonvirtualAlignment = Number(layout.NonvirtualAlignment),
                    fields = Bounded(layout.Fields, FieldsPerType, "Shallow record layout fields").Select(field => new { field.Ordinal, field = Ref(field.Field),
                        offset = Number(field.Offset), bitOffset = Number(field.BitOffset), bitWidth = Number(field.BitWidth), containerOffset = Number(field.ContainerOffset) }).ToArray() } : null;
                row["directBasesUnexpanded"] = Bounded(record.Bases, 16, "Shallow direct base associations").Select(baseClass => new { baseClass.Ordinal,
                    typeAssociationUnexpanded = Ref(baseClass.BaseType), baseClass.IsVirtual, subobjectAssociationUnexpanded = Ref(baseClass.Subobject) }).ToArray();
                break;
            default: row["otherTypeAssociationsExpanded"] = false; break;
        }
    }
}
