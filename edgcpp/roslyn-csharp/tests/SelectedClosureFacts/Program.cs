using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;
using System.Text.Json;
using RoslynObj;

if (args.Length != 1) throw new ArgumentException("SelectedClosureFacts NEW_REPORT.json");
var destination = Path.GetFullPath(args[0]);
if (File.Exists(destination) || Directory.Exists(destination)) throw new ArgumentException("The report must be fresh.");
using var probe = new ClosureProbe();
probe.Execute();
Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
using (var file = new FileStream(destination, FileMode.CreateNew, FileAccess.Write))
    JsonSerializer.Serialize(file, probe.Report, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
Console.WriteLine($"selected-closure complete={probe.Complete}; bodies={probe.BodyCount}; issues={probe.Issues.Count}");
return probe.Complete ? 0 : 3;

internal sealed record InputPin(int Entry, string Path, string Sha256, long Bytes);
internal sealed record OpenUnit(InputPin Pin, RoslynObject Object, Dictionary<string, object?> Row);
internal sealed class ProbeBound(string message) : Exception(message);

internal sealed class ClosureProbe : IDisposable
{
    private const int MaxFunctions = 64, MaxOperations = 4096, MaxTypes = 256, MaxStorage = 128, MaxDepth = 128;
    private const int MaxCandidates = 32, MaxAssociations = 4096;
    private const string RootLinkage = "_ZNK6duckdb6Vector13GetVectorTypeEv";
    private static readonly InputPin[] Pins =
    [
        new(316, "/mnt/storage/roslynobj-duckdb-20261014/export/major2/0316/unit.roslynobj", "cf385364234784aa0d748781d06f74161ad51d28a6a34215449c3ae212810b65", 156354560),
        new(293, "/mnt/storage/roslynobj-duckdb-20261014/export/major2/0293/unit.roslynobj", "e8f1840398a035990420e67e3261db8b2ed044c8e6b471d07d36a7f2542d4a46", 1837615104)
    ];
    private readonly Stopwatch watch = Stopwatch.StartNew();
    private readonly Process process = Process.GetCurrentProcess();
    private readonly List<OpenUnit> units = [];
    private readonly Dictionary<SemanticEntity, string> labels = [];
    private readonly Dictionary<FunctionSymbol, Dictionary<string, object?>> functions = [];
    private readonly Dictionary<VariableSymbol, Dictionary<string, object?>> variables = [];
    private readonly HashSet<CppType> types = [];
    private readonly HashSet<Constant> constants = [];
    private readonly HashSet<ConstantValue> values = [];
    private readonly HashSet<(Initialization Recipe, bool Cleanup)> recipes = [];
    private readonly HashSet<Lifetime> lifetimes = [];
    private readonly HashSet<Scope> scopes = [];
    private readonly HashSet<FieldSymbol> fields = [];
    private readonly HashSet<SemanticNode> initializerRoots = [];
    private readonly HashSet<SemanticNode> executedNodes = [];
    private readonly Dictionary<(int Unit, string Lookup, string Linkage), FunctionSymbol[]> lookups = [];
    private readonly Queue<(int Unit, FunctionSymbol Function)> functionQueue = new();
    private readonly Queue<(int Unit, VariableSymbol Variable)> variableQueue = new();
    private readonly List<object> functionRows = [], variableRows = [], typeRows = [], constantRows = [], valueRows = [];
    private readonly List<object> bodyRows = [], nodeRows = [], occurrences = [], edges = [], bindings = [], recipeRows = [], lifetimeRows = [], ehRows = [], fieldRows = [];
    private readonly HashSet<string> issueKeys = [];
    private long ticks, peakBytes;
    public List<object> Issues { get; } = [];
    public int BodyCount => bodyRows.Count;
    public bool Complete { get; private set; }
    public Dictionary<string, object?> Report { get; }

    public ClosureProbe()
    {
        Report = Row(("probeVersion", 1), ("rootLinkage", RootLinkage), ("closureComplete", false),
            ("translationAccepted", false), ("programWideBindingClaim", false), ("odrOrTypeCompatibilityValidated", false),
            ("scope", "Required public dependencies of one exact Vector getter in two pinned inputs only. Definition matches are candidates, not ODR/type or emitter acceptance. Both retained branches are preserved."),
            ("semantics", "Identity-deduplicated body nodes and structural occurrences are separate. Structural order is recorded context, not a C++ execution schedule. Cleanup recipes/typeinfo are semantic dependencies, never replayed as executable metadata."),
            ("ambientExecutionCatalogInspected", false), ("relatedClassMetadataClosurePerformed", false),
            ("bounds", new { InputObjects = 2, Functions = MaxFunctions, OperationsPerBody = MaxOperations, ShallowTypes = MaxTypes, Storage = MaxStorage, Depth = MaxDepth,
                CandidatesPerInput = MaxCandidates, CandidateOverflowSentinel = MaxCandidates + 1, AssociationsPerList = MaxAssociations,
                WholePhaseSeconds = 180, WholePhaseBytes = 2L * 1024 * 1024 * 1024, CachedRows = 4096, CachedPages = 128, PageSize = 128,
                IndexCaveat = "Take bounds materialized candidates, not internal name-index work; no function enumeration fallback." }),
            ("inputs", units.Select(unit => unit.Row).ToList()), ("functions", functionRows), ("storage", variableRows),
            ("shallowTypes", typeRows), ("constants", constantRows), ("constantValues", valueRows), ("bodies", bodyRows),
            ("typedNodes", nodeRows), ("structuralOccurrences", occurrences), ("dependencyEdges", edges), ("definitionCandidates", bindings),
            ("initializationRecipes", recipeRows), ("lifetimes", lifetimeRows), ("exceptionHandling", ehRows), ("issues", Issues));
        Report["selectedFields"] = fieldRows;
    }

    public void Execute()
    {
        try
        {
            foreach (var pin in Pins)
            {
                Check();
                if (new FileInfo(pin.Path).Length != pin.Bytes || Hash(pin.Path) != pin.Sha256)
                    throw new InvalidDataException("Pinned input content differs; reader not opened: " + pin.Entry);
                var obj = RoslynObject.Open(pin.Path, new ReadOptions { CachedRowCount = 4096, CachedPageCount = 128, PageSize = 128, CommandTimeoutSeconds = 30 });
                var manifest = obj.Manifest;
                var row = Row(("entry", pin.Entry), ("path", pin.Path), ("sha256", pin.Sha256), ("bytes", pin.Bytes),
                    ("artifactIdentity", manifest.ArtifactIdentity), ("producer", manifest.Producer), ("edgRevision", manifest.EdgRevision),
                    ("edgVersion", manifest.EdgVersion), ("captureProfile", manifest.CaptureProfile), ("stage", manifest.Stage),
                    ("mandatoryOpenStatistics", Stats(obj.Statistics)));
                units.Add(new(pin, obj, row)); Check();
            }
            Report["inputs"] = units.Select(unit => unit.Row).ToList();
            var roots = Lookup(0, "GetVectorType", RootLinkage);
            if (roots.Length != 1) throw new InvalidDataException("The exact defined Vector root is absent or ambiguous in core316.");
            UseFunction(0, roots[0], "root", "managed-root");
            while (functionQueue.Count != 0 || variableQueue.Count != 0)
            {
                Check();
                if (functionQueue.TryDequeue(out var function)) VisitFunction(function.Unit, function.Function);
                else { var variable = variableQueue.Dequeue(); VisitVariable(variable.Unit, variable.Variable); }
            }
            foreach (var initializer in initializerRoots.Where(initializer => !executedNodes.Contains(initializer)))
                Issue("initializer-schedule-gap", "initialization", "A required executable initializer association has no visited typed occurrence.");
            Complete = Issues.Count == 0;
        }
        catch (Exception error)
        {
            Issue(error is ProbeBound ? "bound" : "probe-failed", "phase", error.Message);
        }
        finally
        {
            foreach (var unit in units)
            {
                unit.Row["totalReaderStatistics"] = Stats(unit.Object.Statistics);
                try
                {
                    var after = Hash(unit.Pin.Path);
                    unit.Row["afterSha256"] = after;
                    unit.Row["unchanged"] = after == unit.Pin.Sha256;
                    if (after != unit.Pin.Sha256) Issue("input-changed", unit.Pin.Path, "Pinned input changed during this read-only phase.");
                }
                catch (Exception error) { Issue("after-hash-failed", unit.Pin.Path, error.Message); }
            }
            Report["inputs"] = units.Select(unit => unit.Row).ToList();
            Complete &= Issues.Count == 0;
            Report["closureComplete"] = Complete;
            Report["elapsedSecondsIncludingHashesAndOpen"] = watch.Elapsed.TotalSeconds;
            Report["observedProcessPeakBytes"] = peakBytes;
            Report["counts"] = new { Functions = functions.Count, ExecutableBodies = BodyCount, Variables = variables.Count,
                ShallowTypes = types.Count, Constants = constants.Count, TypedNodeIdentitiesPerBody = nodeRows.Count,
                StructuralOccurrences = occurrences.Count, DependencyEdges = edges.Count, WorkTicks = ticks };
        }
    }

    private static Dictionary<string, object?> Row(params (string Key, object? Value)[] fields) => fields.ToDictionary(field => field.Key, field => field.Value);
    private static string? Number(BigInteger? value) => value?.ToString(CultureInfo.InvariantCulture);
    private static object Stats(ReaderStatistics value) => new { value.Commands, value.RowsRead, value.CachedRows, value.CachedPages, value.MaximumBatchSize };
    private void Check(int depth = 0)
    {
        if (++ticks > 2_000_000 || depth > MaxDepth) throw new ProbeBound("Dependency work/depth bound exceeded.");
        if ((ticks & 31) != 1 && depth != 0) return;
        process.Refresh(); peakBytes = Math.Max(peakBytes, Math.Max(process.WorkingSet64, process.PeakWorkingSet64));
        if (watch.Elapsed.TotalSeconds > 180 || peakBytes > 2L * 1024 * 1024 * 1024)
            throw new ProbeBound("Whole-phase time/memory budget exceeded (including hashing/Open).");
    }
    private string Hash(string path)
    {
        using var file = File.OpenRead(path);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var bytes = new byte[128 * 1024];
        int count;
        while ((count = file.Read(bytes)) != 0) { Check(); hash.AppendData(bytes.AsSpan(0, count)); }
        return Convert.ToHexStringLower(hash.GetHashAndReset());
    }
    private void Issue(string code, string path, string message)
    {
        Complete = false;
        if (issueKeys.Add(code + "|" + path + "|" + message)) Issues.Add(new { code, path, message });
    }
    private T[] Bounded<T>(IEnumerable<T> source, string path)
    {
        var result = source.Take(MaxAssociations + 1).ToArray(); Check();
        if (result.Length > MaxAssociations) throw new ProbeBound("Association list bound exceeded at " + path);
        return result;
    }
    private string? Ref(int unit, SemanticEntity? entity)
    {
        if (entity is null) return null;
        if (!labels.TryGetValue(entity, out var label))
        {
            if (labels.Count >= 524288) throw new ProbeBound("Public identity-label bound exceeded.");
            labels.Add(entity, label = $"input/{Pins[unit].Entry}/entity/{labels.Count + 1:D6}");
        }
        return label;
    }
    private void Edge(int unit, string from, string kind, SemanticEntity? target)
    {
        if (target is null) return;
        Check(); edges.Add(new { from, kind, to = Ref(unit, target) });
    }
    private static string? FinalName(FunctionSymbol function) => function.GnuSupplement?.AssemblyName ?? function.CurrentName;
    private FunctionSymbol[] Lookup(int unit, string lookup, string linkage)
    {
        if (lookups.TryGetValue((unit, lookup, linkage), out var cached)) return cached;
        Check();
        var candidates = units[unit].Object.TranslationUnit.SemanticModel.FindSymbols(lookup).Take(MaxCandidates + 1).ToArray();
        Check();
        var row = Row(("inputEntry", Pins[unit].Entry), ("lookupNameForIndexOnly", lookup), ("exactLinkage", linkage),
            ("candidateCount", candidates.Length), ("overflowSentinelObserved", candidates.Length > MaxCandidates));
        bindings.Add(row);
        if (candidates.Length > MaxCandidates)
        {
            Issue("candidate-bound", linkage, "Indexed candidate overflow in entry " + Pins[unit].Entry + "; uniqueness is unknown.");
            return lookups[(unit, lookup, linkage)] = [];
        }
        var exact = candidates.OfType<FunctionSymbol>().Where(function => FinalName(function) == linkage).ToArray();
        var defined = exact.Where(function => function.IsDefined && function.BodyState.Name == "definition" && function.Body is not null).ToArray();
        row["exactCandidates"] = exact.Select(function => new { identity = Ref(unit, function), function.IsDefined, bodyState = function.BodyState.Name,
            currentName = function.CurrentName, assemblyName = function.GnuSupplement?.AssemblyName, sourceName = function.SourceCorrespondence?.SourceName,
            nameHasBeenMangled = function.SourceCorrespondence?.NameHasBeenMangled, bodyRootAssociation = Ref(unit, function.Body?.Root) }).ToArray();
        row["definedCandidates"] = defined.Select(function => Ref(unit, function)).ToArray();
        return lookups[(unit, lookup, linkage)] = defined;
    }
    private void UseFunction(int unit, FunctionSymbol function, string from, string kind)
    {
        Edge(unit, from, kind, function);
        if (functions.ContainsKey(function)) return;
        if (functions.Count >= MaxFunctions) { Issue("function-bound", from, "Required function bound reached; additional function is an unvisited leaf."); return; }
        var row = Row(("identity", Ref(unit, function)), ("inputEntry", Pins[unit].Entry), ("bodyTraversed", false));
        functions.Add(function, row); functionRows.Add(row); functionQueue.Enqueue((unit, function));
    }
    private void UseVariable(int unit, VariableSymbol variable, string from, string kind)
    {
        Edge(unit, from, kind, variable);
        if (variables.ContainsKey(variable)) return;
        if (variables.Count >= MaxStorage) { Issue("storage-bound", from, "Required storage/variable bound reached; additional variable is an unvisited leaf."); return; }
        var row = Row(("identity", Ref(unit, variable)), ("inputEntry", Pins[unit].Entry));
        variables.Add(variable, row); variableRows.Add(row); variableQueue.Enqueue((unit, variable));
    }
    private void Use(int unit, SemanticEntity? entity, string from, string kind, int depth = 0)
    {
        Check(depth);
        switch (entity)
        {
            case null: return;
            case FunctionSymbol function: UseFunction(unit, function, from, kind); break;
            case VariableSymbol variable: UseVariable(unit, variable, from, kind); break;
            case CppType type: Type(unit, type, depth + 1); Edge(unit, from, kind, type); break;
            case Constant constant: Constant(unit, constant, from, depth + 1); break;
            case ConstantValue value: Value(unit, value, from, depth + 1); break;
            case FieldSymbol field:
                Type(unit, field.Type, depth + 1); Type(unit, field.ContainingType, depth + 1); Edge(unit, from, kind, field);
                if (fields.Add(field))
                {
                    var layouts = Bounded(field.ContainingType.Layout?.Fields ?? [], from).Where(layout => layout.Field.Equals(field)).ToArray();
                    fieldRows.Add(new { identity = Ref(unit, field), nameForDisplay = field.Name, owner = Ref(unit, field.ContainingType),
                        type = Ref(unit, field.Type), field.IsMutable, field.IsGeneratedBase, field.IsGeneratedVirtualPointer,
                        layouts = layouts.Select(layout => new { layout.Ordinal, offset = Number(layout.Offset), bitOffset = Number(layout.BitOffset),
                            bitWidth = Number(layout.BitWidth), containerOffset = Number(layout.ContainerOffset) }).ToArray() });
                }
                break;
            case LabelSymbol: Edge(unit, from, "function-local-label", entity); break;
            default: Issue("unknown-dependency", from, "Unsupported public dependency entity; no reflection or type reconstruction."); break;
        }
    }
    private void Type(int unit, CppType? type, int depth = 0)
    {
        if (type is null || types.Contains(type)) return;
        Check(depth);
        if (types.Count >= MaxTypes) { Issue("type-bound", "shallow-types", "Required shallow type bound reached; remaining associations are unexpanded."); return; }
        types.Add(type);
        var row = Row(("identity", Ref(unit, type)), ("inputEntry", Pins[unit].Entry), ("nameForDisplay", type.Name), ("kind", type.Kind.Name),
            ("size", Number(type.Size)), ("alignment", Number(type.Alignment)), ("descriptorSize", Number(type.DescriptorSize)),
            ("descriptorAlignment", Number(type.DescriptorAlignment)), ("effectiveSize", Number(type.EffectiveSize)),
            ("effectiveAlignment", Number(type.EffectiveAlignment)), ("incomplete", type.IsIncomplete), ("dependent", type.IsDependent));
        typeRows.Add(row);
        switch (type)
        {
            case QualifiedType qualified:
                row["underlying"] = Ref(unit, qualified.UnderlyingType); row["const"] = qualified.IsConst; row["volatile"] = qualified.IsVolatile;
                Type(unit, qualified.UnderlyingType, depth + 1); break;
            case TypeReference reference: row["underlying"] = Ref(unit, reference.UnderlyingType); Type(unit, reference.UnderlyingType, depth + 1); break;
            case PointerType pointer:
                row["pointee"] = Ref(unit, pointer.PointeeType); row["isReference"] = pointer.IsReference; row["isRvalueReference"] = pointer.IsRValueReference;
                Type(unit, pointer.PointeeType, depth + 1); break;
            case ArrayType array: row["element"] = Ref(unit, array.ElementType); row["bound"] = Number(array.Bound); Type(unit, array.ElementType, depth + 1); break;
            case RoutineType routine:
                Type(unit, routine.ReturnType, depth + 1);
                foreach (var parameter in Bounded(routine.Parameters, "signature")) { Type(unit, parameter.Type, depth + 1); Type(unit, parameter.DeclaredType, depth + 1); }
                row["signature"] = Signature(unit, routine); break;
            case ArithmeticType arithmetic: row["arithmetic"] = new { arithmetic.Arithmetic.Bits, arithmetic.Arithmetic.Signed, arithmetic.Arithmetic.IsBool }; break;
            case RecordType record:
                row["recordFieldsBasesMethodsUnexpanded"] = true;
                row["layout"] = record.Layout is { } layout ? new { size = Number(layout.Size), alignment = Number(layout.Alignment),
                    nonvirtualSize = Number(layout.NonvirtualSize), nonvirtualAlignment = Number(layout.NonvirtualAlignment), layout.IsComplete } : null;
                row["subobjectPartnerAssociationOnly"] = Ref(unit, record.SubobjectPartner);
                row["partnerExtent"] = record.SubobjectPartner is { } partner ? new { size = Number(partner.Size), alignment = Number(partner.Alignment) } : null;
                break;
        }
    }
    private object Signature(int unit, RoutineType signature) => new
    {
        identity = Ref(unit, signature), returnType = Ref(unit, signature.ReturnType), signature.HasPrototype, signature.HasThisParameter,
        signature.IsVariadic, signature.ReturnsValueAsParameter, signature.ReturnsValueViaCopyConstructor,
        callingConvention = signature.CallingConvention?.Name, languageLinkage = signature.LanguageLinkage?.Name,
        noexceptEffective = signature.ExceptionSpecification?.NoexceptEffective,
        parameters = Bounded(signature.Parameters, "signature-parameters").Select(parameter => new { parameter.Ordinal, role = parameter.Role.Name,
            type = Ref(unit, parameter.Type), declaredType = Ref(unit, parameter.DeclaredType), descriptor = Ref(unit, parameter.Descriptor) }).ToArray()
    };
    private void VisitFunction(int unit, FunctionSymbol function)
    {
        Check(); var row = functions[function]; var identity = Ref(unit, function)!;
        row["sourceNameForIndexOnly"] = function.SourceCorrespondence?.SourceName; row["currentName"] = function.CurrentName;
        row["assemblyName"] = function.GnuSupplement?.AssemblyName; row["capturedFinalLinkage"] = FinalName(function);
        row["nameHasBeenMangled"] = function.SourceCorrespondence?.NameHasBeenMangled;
        row["isDefined"] = function.IsDefined; row["bodyState"] = function.BodyState.Name; row["builtinKind"] = function.BuiltinKind?.Name;
        row["specialFunctionKind"] = function.SpecialFunctionKind?.Name; row["entryKind"] = function.ConstructorOrDestructorKind?.Name;
        Type(unit, function.Signature); Type(unit, function.ContainingType); row["signature"] = Signature(unit, function.Signature);
        row["executionParameters"] = Bounded(function.Parameters, identity).Select(parameter => new { parameter.Ordinal, role = parameter.Role.Name,
            type = Ref(unit, parameter.Type), declaredType = Ref(unit, parameter.DeclaredType), symbol = Ref(unit, parameter.Symbol) }).ToArray();
        foreach (var parameter in Bounded(function.Parameters, identity))
        { Type(unit, parameter.Type); Type(unit, parameter.DeclaredType); if (parameter.Symbol is { } symbol) UseVariable(unit, symbol, identity, "execution-parameter"); }
        if (function.BodyState.Name != "definition" || function.Body is not { } body)
        {
            row["bodyDisposition"] = function.BuiltinKind is not null ? "recorded-builtin-leaf" : "declaration-or-unavailable-leaf";
            if (function.BuiltinKind is not null) return;
            var display = function.SourceCorrespondence?.SourceName; var linkage = FinalName(function);
            if (string.IsNullOrEmpty(display) || string.IsNullOrEmpty(linkage)) { Issue("unresolved-function", identity, "No indexed source name/final linkage for declaration lookup."); return; }
            var candidates = units.SelectMany((_, index) => Lookup(index, display, linkage).Select(candidate => (Unit: index, Function: candidate))).ToArray();
            row["definedCandidateIdentitiesAcrossTwoInputs"] = candidates.Select(candidate => Ref(candidate.Unit, candidate.Function)).ToArray();
            if (candidates.Length == 1) UseFunction(candidates[0].Unit, candidates[0].Function, identity, "exact-linkage-definition-candidate-not-odr-proof");
            else Issue(candidates.Length == 0 ? "unresolved-function" : "ambiguous-definition", identity, "Exact defined candidates within two inspected inputs: " + candidates.Length);
            return;
        }
        row["bodyTraversed"] = true; row["bodyRoot"] = Ref(unit, body.Root); row["bodyFunctionReciprocal"] = body.Function.Equals(function);
        foreach (var local in Bounded(body.Locals, identity)) UseVariable(unit, local, identity, "body-local-storage");
        var summary = Row(("function", identity), ("root", Ref(unit, body.Root)), ("traceComplete", false)); bodyRows.Add(summary);
        var issuesBefore = Issues.Count;
        var seen = new HashSet<SemanticNode>(); var active = new HashSet<SemanticNode>();
        Walk(unit, body.Root, function, identity + "/body", null, "body", 0, seen, active, 0);
        Scope(unit, body.Scope, identity + "/scope", 0);
        foreach (var lifetime in Bounded(body.Lifetimes, identity).Where(lifetime => lifetime.BoundEntity?.Equals(function) == true)) Lifetime(unit, lifetime, identity, 0);
        var eh = body.ExceptionHandling;
        Use(unit, eh.RegionTable, identity, "eh-region-table"); Use(unit, eh.ArrayTable, identity, "eh-array-table");
        if (eh.Details?.Prologue is { } prologue) Walk(unit, prologue, function, identity + "/prologue", null, "eh-prologue", 0, seen, active, 0);
        foreach (var epilogue in Bounded(eh.Epilogues, identity)) Walk(unit, epilogue, function, identity + "/epilogue", null, "eh-epilogue", 0, seen, active, 0);
        foreach (var region in Bounded(eh.CleanupRegions, identity))
        {
            ehRows.Add(new { function = identity, region = Ref(unit, region), region.Ordinal, next = Ref(unit, region.NextRegion),
                conditionStorage = Ref(unit, region.Condition?.Storage.Variable), actions = Bounded(region.Actions, identity).Select(action => new
                { identity = Ref(unit, action), kind = action.Kind.Name, target = Ref(unit, action.TargetMethod), storage = Ref(unit, action.Storage?.Variable),
                    offset = Number(action.Storage?.Offset), indirect = action.Storage?.IsIndirect, thisOffset = action.Storage?.UsesThisParameterOffset,
                    array = Ref(unit, action.Array), vtt = Ref(unit, action.AbiDetail?.VttLocation?.Variable), count = Ref(unit, action.AbiDetail?.VlaCountLocation?.Variable) }).ToArray() });
            Use(unit, region.Condition?.Storage.Variable, identity, "cleanup-condition");
            foreach (var action in Bounded(region.Actions, identity))
            { Use(unit, action.TargetMethod, identity, "cleanup-action-target"); Use(unit, action.Storage?.Variable, identity, "cleanup-storage");
                Use(unit, action.AbiDetail?.VttLocation?.Variable, identity, "cleanup-vtt"); Use(unit, action.AbiDetail?.VlaCountLocation?.Variable, identity, "cleanup-count"); }
        }
        foreach (var array in Bounded(eh.Arrays, identity))
        {
            ehRows.Add(new { function = identity, array = Ref(unit, array), size = Number(array.ElementSize), count = Number(array.ElementCount), handle = Ref(unit, array.HandleConstant) });
            Constant(unit, array.HandleConstant, identity, 0);
            foreach (var location in Bounded(array.StorageLocations, identity).Concat(Bounded(array.CountLocations, identity))) Use(unit, location.Variable, identity, "array-storage-or-count");
        }
        summary["typedIdentityCount"] = seen.Count;
        summary["traceComplete"] = Issues.Count == issuesBefore;
    }

    private void VisitVariable(int unit, VariableSymbol variable)
    {
        Check(); var row = variables[variable]; var identity = Ref(unit, variable)!;
        row["nameForDisplay"] = variable.Name; row["currentName"] = variable.CurrentName;
        row["assemblyName"] = variable.AssemblyName; row["storage"] = variable.SymbolDetails.Storage?.Name;
        row["type"] = Ref(unit, variable.Type); row["declaredType"] = Ref(unit, variable.DeclaredType);
        row["duration"] = variable.Details.StorageDuration?.Name; row["initializationKind"] = variable.InitializationKind.Name;
        row["isParameter"] = variable.Details.IsParameter; row["isThreadLocal"] = variable.IsThreadLocal;
        row["enclosingRoutineAssociation"] = Ref(unit, variable.SourceCorrespondence?.EnclosingRoutine);
        Type(unit, variable.Type); Type(unit, variable.DeclaredType);
        if (variable.SymbolDetails.Storage?.Name == "sc_extern") row["disposition"] = "external-data-declaration-leaf-no-provider-or-definition-validation";
        Constant(unit, variable.ConstantInitializer, identity, 0);
        Recipe(unit, variable.Initializer, identity, false, 0);
        Use(unit, variable.CleanupRoutine, identity, "variable-cleanup-target");
        Use(unit, variable.Details.DynamicInitializationRoutine, identity, "variable-dynamic-initializer");
        Use(unit, variable.Details.ThreadInitializationRoutine, identity, "thread-initializer");
        Use(unit, variable.Details.ThreadWrapperRoutine, identity, "thread-wrapper");
        foreach (var local in Bounded(variable.LocalStaticInitializations, identity))
        { Constant(unit, local.Constant, identity, 0); Recipe(unit, local.Initialization, identity, false, 0); Lifetime(unit, local.Lifetime, identity, 0); }
    }

    private void Constant(int unit, Constant? constant, string from, int depth)
    {
        if (constant is null) return;
        Edge(unit, from, "constant", constant);
        if (!constants.Add(constant)) return;
        Check(depth);
        constantRows.Add(new { identity = Ref(unit, constant), inputEntry = Pins[unit].Entry, type = Ref(unit, constant.Type),
            originalTypeAssociationOnly = Ref(unit, constant.OriginalType), value = Ref(unit, constant.Value),
            associatedStorage = Ref(unit, constant.AssociatedStorageVariable), backingExpressionNotTraversed = Ref(unit, constant.BackingExpression) });
        Type(unit, constant.Type, depth + 1);
        Use(unit, constant.AssociatedStorageVariable, Ref(unit, constant)!, "constant-associated-storage", depth + 1);
        Value(unit, constant.Value, Ref(unit, constant)!, depth + 1);
        foreach (var component in Bounded(constant.Components, from)) Constant(unit, component.Constant, Ref(unit, constant)!, depth + 1);
    }

    private void Value(int unit, ConstantValue value, string from, int depth)
    {
        Edge(unit, from, "constant-value", value);
        if (!values.Add(value)) return;
        Check(depth); var row = Row(("identity", Ref(unit, value)), ("kind", value.Kind.Name)); valueRows.Add(row);
        switch (value)
        {
            case IntegerValue integer: row["integer"] = Number(integer.Value); break;
            case AddressValue address:
                row["baseKind"] = address.BaseKind.Name; row["base"] = Ref(unit, address.BaseEntity);
                row["byteOffset"] = Number(address.ByteOffset); row["onePastEnd"] = address.OnePastEnd;
                row["subobjectPathAssociation"] = Ref(unit, address.SubobjectPath);
                Use(unit, address.BaseEntity, Ref(unit, value)!, "address-base", depth + 1); break;
            case AggregateValue aggregate:
                var elements = Bounded(aggregate.Elements, from);
                row["elements"] = elements.Select(element => new { element.Ordinal, type = Ref(unit, element.Type), value = Ref(unit, element.Value) }).ToArray();
                foreach (var element in elements) { Type(unit, element.Type, depth + 1); Value(unit, element.Value, Ref(unit, value)!, depth + 1); }
                break;
            case ConstantValue<RepeatValueDetails> repeat:
                row["repeatedValue"] = Ref(unit, repeat.Payload.Value); Type(unit, repeat.Payload.Type, depth + 1); Value(unit, repeat.Payload.Value, from, depth + 1); break;
            case ConstantValue<StackOffsetValueDetails> stack:
                row["variable"] = Ref(unit, stack.Payload.Variable); row["offset"] = Number(stack.Payload.Offset);
                Use(unit, stack.Payload.Variable, from, "stack-offset-storage", depth + 1); break;
            case ConstantValue<MemberPointerValueDetails> member: Use(unit, member.Payload.Member, from, "member-pointer", depth + 1); break;
            case LabelDifferenceValue difference: Value(unit, difference.FromValue, from, depth + 1); Value(unit, difference.ToValue, from, depth + 1); break;
            case FloatingValue or StringValue: row["payloadNotExpandedNoSymbolEdges"] = true; break;
            default: Issue("unknown-constant-kind", from, value.Kind.Name); break;
        }
    }

    private void Recipe(int unit, Initialization? recipe, string from, bool cleanup, int depth)
    {
        if (recipe is null) return;
        Edge(unit, from, cleanup ? "selected-cleanup-recipe" : "required-initialization-recipe", recipe);
        if (!recipes.Add((recipe, cleanup))) return;
        Check(depth); var identity = Ref(unit, recipe)!;
        var operation = recipe.Operation; var expression = recipe.Expression;
        var execution = operation?.RepresentationRole.Name == "execution" || operation is null && expression?.RepresentationRole.Name == "execution";
        recipeRows.Add(new { identity, inputEntry = Pins[unit].Entry, kind = recipe.Kind.Name, cleanupDependency = cleanup,
            operation = Ref(unit, operation), operationRole = operation?.RepresentationRole.Name, expression = Ref(unit, expression),
            target = Ref(unit, recipe.Target), constructor = Ref(unit, recipe.Constructor), destructor = Ref(unit, recipe.Destructor),
            masterMetadataOnly = Ref(unit, recipe.Master), nextDestruction = Ref(unit, recipe.NextInDestructionList),
            lifetime = Ref(unit, recipe.Lifetime), initExpressionLifetime = Ref(unit, recipe.InitExpressionLifetime),
            overlappingLifetime = Ref(unit, recipe.OverlappingLifetime), constant = Ref(unit, recipe.Constant),
            recipe.IsFreeingOfExceptionObject, executionOccurrenceFollowsTypedContainmentOnly = true });
        Type(unit, recipe.Details.Type, depth + 1);
        Use(unit, recipe.Target, identity, "recipe-target-storage", depth + 1);
        if (cleanup) Use(unit, recipe.Destructor, identity, "selected-cleanup-destructor", depth + 1);
        else if (execution) Use(unit, recipe.Constructor, identity, "executable-initializer-constructor-association", depth + 1);
        Constant(unit, recipe.Constant, identity, depth + 1);
        // A metadata master/original recipe is not a second execution root.
        if (execution || cleanup)
        {
            Lifetime(unit, recipe.Lifetime, identity, depth + 1); Lifetime(unit, recipe.InitExpressionLifetime, identity, depth + 1);
            Lifetime(unit, recipe.OverlappingLifetime, identity, depth + 1);
        }
        if (cleanup) Recipe(unit, recipe.NextInDestructionList, identity, true, depth + 1);
        // Executable recipes that are not retained in a selected body remain
        // explicit schedule gaps; metadata operations are never evaluated.
        if (execution && operation is not null) initializerRoots.Add(operation);
        else if (execution && expression is not null) initializerRoots.Add(expression);
    }

    private void Lifetime(int unit, Lifetime? lifetime, string from, int depth)
    {
        if (lifetime is null) return;
        Edge(unit, from, "live-lifetime-association", lifetime);
        if (!lifetimes.Add(lifetime)) return;
        Check(depth); var identity = Ref(unit, lifetime)!;
        var cleanups = Bounded(lifetime.Cleanups, identity);
        lifetimeRows.Add(new { identity, kind = lifetime.Kind.Name, boundEntity = Ref(unit, lifetime.BoundEntity), parent = Ref(unit, lifetime.Parent),
            parentCheckpoint = Ref(unit, lifetime.ParentCheckpoint), lifetime.HasImplicitChild, lifetime.HasBlockAfterLabelChild,
            cleanupRecipes = cleanups.Select(cleanup => new { cleanup.Ordinal, cleanup.IsConditional, cleanup.IsUnordered, recipe = Ref(unit, cleanup.Initialization) }).ToArray(),
            childrenAreRecordedAssociationsNotSchedule = true });
        foreach (var cleanup in cleanups) Recipe(unit, cleanup.Initialization, identity, true, depth + 1);
        foreach (var child in Bounded(lifetime.Children, identity)) Lifetime(unit, child, identity, depth + 1);
    }

    private void Scope(int unit, Scope? scope, string from, int depth)
    {
        if (scope is null || !scopes.Add(scope)) return;
        Check(depth); Lifetime(unit, scope.Lifetime, from, depth + 1); Lifetime(unit, scope.FunctionStaticLifetime, from, depth + 1);
        foreach (var initialization in Bounded(scope.DynamicInitializations, from)) Recipe(unit, initialization, from, false, depth + 1);
        foreach (var initialization in Bounded(scope.LocalStaticInitializations, from))
        { Recipe(unit, initialization.Initialization, from, false, depth + 1); Constant(unit, initialization.Constant, from, depth + 1); Lifetime(unit, initialization.Lifetime, from, depth + 1); }
        foreach (var initialization in Bounded(scope.ConstructorInitializers, from)) Recipe(unit, initialization.EffectiveInitialization, from, false, depth + 1);
    }

    private void ExceptionTypeinfo(int unit, CppType? type, string path, int depth)
    {
        if (type is null) return;
        Check(depth); Type(unit, type, depth + 1);
        Use(unit, type.Details.TypeinfoVariable, path, "exception-type-typeinfo-variable", depth + 1);
        if (type is QualifiedType qualified) ExceptionTypeinfo(unit, qualified.UnderlyingType, path, depth + 1);
        else if (type is TypeReference reference) ExceptionTypeinfo(unit, reference.UnderlyingType, path, depth + 1);
        else if (type is PointerType { IsReference: true } pointer) ExceptionTypeinfo(unit, pointer.PointeeType, path, depth + 1);
    }

    private void Walk(int unit, SemanticNode node, FunctionSymbol owner, string path, SemanticNode? parent, string role, int ordinal,
        HashSet<SemanticNode> seen, HashSet<SemanticNode> active, int depth)
    {
        Check(depth);
        occurrences.Add(new { function = Ref(unit, owner), path, identity = Ref(unit, node), parent = Ref(unit, parent), role, ordinal,
            representationRole = node.RepresentationRole.Name, identitySeenEarlier = seen.Contains(node) });
        if (node.RepresentationRole.Name != "execution") return;
        if (active.Contains(node)) { Issue("execution-cycle", path, "Execution containment cycle."); return; }
        if (seen.Contains(node)) return;
        if (seen.Count >= MaxOperations) { Issue("body-operation-bound", Ref(unit, owner)!, "Remaining selected body nodes are unvisited."); return; }
        seen.Add(node); active.Add(node); executedNodes.Add(node);
        try
        {
            var row = Row(("identity", Ref(unit, node)), ("function", Ref(unit, owner)), ("firstContextPath", path),
                ("kind", node.Kind.Name), ("representationRole", node.RepresentationRole.Name)); nodeRows.Add(row);
            if (node is Operation operation) { Type(unit, operation.Type, depth + 1); row["type"] = Ref(unit, operation.Type); }
            if (node is ExpressionOperation expression)
            { row["valueCategory"] = expression.ValueCategory.Name; row["volatileFetch"] = expression.Details.VolatileFetch; }
            if (node is OperatorOperation op)
            { row["operatorKind"] = op.OperatorKind.Name; row["leftToRight"] = op.Operator.EvalLeftToRight; row["rightToLeft"] = op.Operator.EvalRightToLeft; }
            switch (node)
            {
                case ReferenceOperation reference: row["target"] = Ref(unit, reference.Target); Use(unit, reference.Target, path, "typed-reference", depth + 1); break;
                case ConstantOperation constant: row["constant"] = Ref(unit, constant.Constant); Constant(unit, constant.Constant, path, depth + 1); break;
                case CallOperation call:
                    Type(unit, call.Call.FunctionType, depth + 1); Use(unit, call.TargetMethod, path, "actual-selected-direct-target", depth + 1);
                    row["call"] = new { kind = call.CallKind.Name, target = Ref(unit, call.TargetMethod), sourceTargetAssociationOnly = Ref(unit, call.SourceTargetMethod),
                        callee = Ref(unit, call.Callee), receiver = Ref(unit, call.Receiver), returnSlot = Ref(unit, call.Call.ReturnSlot),
                        functionType = Ref(unit, call.Call.FunctionType), rawOperands = Bounded(call.Operands, path).Select(operand => Ref(unit, operand)).ToArray(),
                        arguments = Bounded(call.Arguments, path).Select(argument => new { argument.Ordinal, role = argument.Role?.Name,
                            value = Ref(unit, argument.Value), parameter = Ref(unit, argument.Parameter) }).ToArray() };
                    if (call.CallKind.Name != "direct" || call.TargetMethod is null) Issue("unresolved-indirect-call", path, "No finite direct body target is selected; no native fallback.");
                    break;
                case InitializationStatementOperation initialization: Recipe(unit, initialization.Initialization, path, false, depth + 1); break;
                case InitializationOperation initialization:
                    Recipe(unit, initialization.Initialization, path, false, depth + 1); break;
                case StatementExpressionResultOperation result: Recipe(unit, result.Initialization, path, false, depth + 1); break;
                case TemporaryOperation temporary: Recipe(unit, temporary.Initialization, path, false, depth + 1); Type(unit, temporary.SourceType, depth + 1); break;
                case ThrowOperation thrown:
                    row["throw"] = new { thrown.IsRethrow, operand = Ref(unit, thrown.Operand), thrownType = Ref(unit, thrown.ThrownType),
                        initialization = Ref(unit, thrown.Initialization), destructor = Ref(unit, thrown.Destructor) };
                    ExceptionTypeinfo(unit, thrown.ThrownType, path, depth + 1); Use(unit, thrown.Destructor, path, "actual-throw-destructor", depth + 1);
                    Recipe(unit, thrown.Initialization, path, false, depth + 1); break;
                case TryOperation selectedTry:
                    Lifetime(unit, selectedTry.Definition.Lifetime, path, depth + 1);
                    row["handlers"] = Bounded(selectedTry.Handlers, path).Select(handler => new { identity = Ref(unit, handler),
                        caughtType = Ref(unit, handler.CaughtType), actualTypeinfo = Ref(unit, handler.Details.Typeinfo),
                        parameter = Ref(unit, handler.Parameter), parameterInitialization = Ref(unit, handler.ParameterInitialization), body = Ref(unit, handler.Body) }).ToArray();
                    foreach (var handler in Bounded(selectedTry.Handlers, path))
                    { ExceptionTypeinfo(unit, handler.CaughtType, path, depth + 1); Use(unit, handler.Details.Typeinfo, path, "actual-handler-typeinfo", depth + 1);
                        Use(unit, handler.Parameter, path, "handler-parameter", depth + 1); Recipe(unit, handler.ParameterInitialization, path, false, depth + 1); Lifetime(unit, handler.BodyLifetime, path, depth + 1); }
                    break;
                case AllocationOperation allocation:
                    Use(unit, allocation.Allocator, path, "allocator", depth + 1); Use(unit, allocation.Deallocator, path, "deallocator", depth + 1);
                    Recipe(unit, allocation.Initialization, path, false, depth + 1); Recipe(unit, allocation.ExceptionFreeing, path, true, depth + 1); break;
                case LoweredEhOperation marker:
                    row["loweredEh"] = new { kind = marker.ExceptionKind.Name, initializationAssociation = Ref(unit, marker.Initialization),
                        cleanupStart = Ref(unit, marker.CleanupStart), completedInitializer = Ref(unit, marker.CompletedInitializer),
                        region = Ref(unit, marker.ExceptionOperation.Region), state = Number(marker.ExceptionOperation.StateNumber),
                        routine = Ref(unit, marker.ExceptionOperation.Routine), handler = Ref(unit, marker.ExceptionOperation.Handler) };
                    Use(unit, marker.ExceptionOperation.Routine, path, "lowered-eh-selected-routine", depth + 1);
                    Recipe(unit, marker.CleanupStart, path, true, depth + 1); break;
                case BlockOperation block: Scope(unit, block.Scope, path, depth + 1); Lifetime(unit, block.Lifetime, path, depth + 1); break;
                case LifetimeOperation lifetime: Lifetime(unit, lifetime.Lifetime, path, depth + 1); break;
                case LabelOperation label: Lifetime(unit, label.Lifetime, path, depth + 1); break;
                case JumpOperation jump: Lifetime(unit, jump.CommonLifetime, path, depth + 1); break;
                case LoopOperation loop: Scope(unit, loop.InitScope, path, depth + 1); break;
                case ConditionOperation condition:
                    Scope(unit, condition.Scope, path, depth + 1); Use(unit, condition.Variable, path, "condition-variable", depth + 1); Recipe(unit, condition.DynamicInitialization, path, false, depth + 1); break;
            }
            var children = Children(node, path).ToArray();
            row["typedChildren"] = children.Select((child, index) => new { identity = Ref(unit, child), ordinal = index }).ToArray();
            foreach (var relation in Bounded(node.ChildRelations, path))
                if (relation.Child.RepresentationRole.Name == "execution" && !children.Contains(relation.Child))
                    Issue("unknown-execution-child", path, relation.Role.Name + " -> " + Ref(unit, relation.Child));
            for (var index = 0; index < children.Length; ++index)
                Walk(unit, children[index], owner, path + "/child[" + index + "]", node, "typed-child", index, seen, active, depth + 1);
        }
        finally { active.Remove(node); }
    }

    private IEnumerable<SemanticNode> Children(SemanticNode node, string path)
    {
        switch (node)
        {
            case BlockOperation block: return Bounded(block.Statements, path);
            case ConditionalOperation conditional: return new SemanticNode?[] { conditional.Condition, conditional.Then, conditional.Else }.OfType<SemanticNode>();
            case LoopOperation loop: return new SemanticNode?[] { loop.Initialization, loop.Condition, loop.Increment, loop.Body }.OfType<SemanticNode>();
            case SwitchOperation selectedSwitch: return new SemanticNode?[] { selectedSwitch.Selector, selectedSwitch.Body }.OfType<SemanticNode>();
            case TryOperation selectedTry: return new[] { selectedTry.Body }.Concat(Bounded(selectedTry.Handlers, path).Select(handler => handler.Body));
            case JumpOperation jump: return jump.Value is { } jumpValue ? [jumpValue] : [];
            case InitializationStatementOperation initialization:
                return initialization.Initialization.Operation is { RepresentationRole.Name: "execution" } initializerOperation ? [initializerOperation] :
                    initialization.Initialization.Operation is null && initialization.Initialization.Expression is { RepresentationRole.Name: "execution" } initializerValue ? [initializerValue] : [];
            case InitializationOperation initialization:
                return new SemanticNode?[] { initialization.Expression }.Concat(Bounded(initialization.Initialization.Operands, path).Select(operand => (SemanticNode?)operand.Expression)).OfType<SemanticNode>();
            case ThrowOperation thrown: return thrown.Operand is { } thrownValue ? [thrownValue] : [];
            case LifetimeOperation lifetime: return [lifetime.Value];
            case ConditionOperation condition: return new SemanticNode?[] { condition.Initialization, condition.Value }.OfType<SemanticNode>();
            case StatementExpressionOperation statementExpression: return [statementExpression.Statement];
            case ReferenceOperation or ConstantOperation or LabelOperation or DeclarationOperation: return [];
            case LoweredEhOperation marker: return Bounded(marker.Operands, path);
            case OperatorOperation selectedOperator: return Bounded(selectedOperator.Operands, path);
            case StatementOperation { Kind.Name: "stmk_expr", Expression: { } statementValue }: return [statementValue];
            case StatementOperation { Kind.Name: "stmk_empty" }: return [];
            case ExpressionOperation genericExpression: return Bounded(genericExpression.Operands, path);
            default: Issue("unknown-node-kind", path, node.Kind.Name); return [];
        }
    }
    public void Dispose()
    {
        foreach (var unit in units) unit.Object.Dispose();
        process.Dispose();
    }
}
