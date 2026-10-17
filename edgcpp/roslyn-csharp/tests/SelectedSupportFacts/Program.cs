using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using RoslynObj;
using RoslynObj.CSharp;

if (args.Length is not (2 or 4 or 5 or 6) || args.Length >= 5 && args[4] != "--captured-linkage" ||
    args.Length == 6 && args[5] != "--forwarding-body")
    throw new ArgumentException("SelectedSupportFacts SUPPORT_PATH NEW_REPORT_PATH [LOOKUP_NAME EXACT_LINKAGE [--captured-linkage [--forwarding-body]]]");
var supportPath = Path.GetFullPath(args[0]);
var reportPath = Path.GetFullPath(args[1]);
if (File.Exists(reportPath) || Directory.Exists(reportPath)) throw new ArgumentException("Report path must be fresh.");
var probe = new SupportProbe(args.Length >= 4 ? args[2] : null, args.Length >= 4 ? args[3] : null, args.Length >= 5, args.Length == 6);
try { probe.Run(supportPath); }
catch (Exception exception) { probe.AddIssue(exception is ProbeBound ? "bound" : "failure", exception.Message); }
finally { probe.Finish(); }
Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);
using (var stream = new FileStream(reportPath, FileMode.CreateNew, FileAccess.Write))
    JsonSerializer.Serialize(stream, probe.Report, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
Console.WriteLine($"selected-support complete={probe.Report["inspectionComplete"]}; issues={probe.IssueCount}");
return probe.Report["inspectionComplete"] is true ? 0 : 3;

internal sealed class ProbeBound(string message) : Exception(message);

internal sealed class SupportProbe
{
    private readonly string displayName;
    private readonly string finalName;
    private readonly bool capturedLinkageDiscovery;
    private readonly bool forwardingBody;
    private const int MaximumSources = 512, MaximumCandidates = 256, MaximumEntries = 16;
    private const long MaximumTotalCommands = 16384, MaximumTotalRows = 65536;
    private const long MaximumLookupCommands = 8192, MaximumLookupRows = 32768;
    private const long MaximumPhysicalSourceBytes = 256L * 1024 * 1024;
    private readonly Stopwatch watch = Stopwatch.StartNew();
    private readonly List<object> issues = [];
    private readonly List<UnitProbe> units = [];
    private readonly Dictionary<SemanticEntity, string> identities = [];
    private readonly List<object> compatibility = [];
    private bool completed;
    private long physicalSourceBytes;
    public int IssueCount => issues.Count;
    public Dictionary<string, object?> Report { get; }

    public SupportProbe(string? lookupName, string? exactLinkage, bool capturedLinkageDiscovery, bool forwardingBody)
    {
        this.capturedLinkageDiscovery = capturedLinkageDiscovery;
        this.forwardingBody = forwardingBody;
        displayName = lookupName ?? "~basic_string";
        finalName = exactLinkage ?? "_ZNSt7__cxx1112basic_stringIcSt11char_traitsIcESaIcEED1Ev";
        if (string.IsNullOrWhiteSpace(displayName) || string.IsNullOrWhiteSpace(finalName) ||
            Encoding.UTF8.GetByteCount(displayName) > 4096 || Encoding.UTF8.GetByteCount(finalName) > 4096)
            throw new ArgumentException("Lookup name and exact linkage must be nonempty and at most 4096 UTF-8 bytes each.");
        Report = Row(("probeVersion", 4), ("inspectionComplete", false), ("translatorAccepted", false),
            ("linkClosureComplete", false), ("historicalHeaderByteIdentityClaim", false),
            ("typedBodyTracePerformed", false), ("typedBodyOccurrenceCount", 0),
            ("functionOrTypeEnumerationPerformed", false), ("sourceRecoveryPerformed", false),
            ("scope", capturedLinkageDiscovery
                ? "One newly supplied support capture and exactly two pinned consumer objects. Public source-file metadata/current-byte hashes and exact captured-linkage candidates only; no source-name discovery. ABI comparison of retained defined-body associations is not provider admission, ODR, link closure or body proof."
                : "One newly supplied support capture and exactly two pinned consumer objects. Public source-file metadata/current-byte hashes and indexed exact-entry associations only. ABI comparison is representation evidence, not ODR, link closure or body proof."),
            ("lookup", new { DisplayName = displayName, ExactCapturedFinalName = finalName,
                DefaultD1Mode = lookupName is null && exactLinkage is null,
                DiscoveryMode = capturedLinkageDiscovery ? "public-captured-final-linkage-candidates" : "public-display-name-index-candidates",
                LookupNameUsedForDiscovery = !capturedLinkageDiscovery, SupportExactC2BodyOnly = forwardingBody }),
            ("usableProviderAdmissionPerformed", false),
            ("templateAssociationScope", "Only each exact/explicitly related routine and its containing record: direct descriptor/instance links, descriptor kind/original/definition/prototype identities, instance result/state/flags. No template argument, instance/member collection, definition text or body expansion."),
            ("bounds", new
            {
                InputObjects = 3, SourceFiles = MaximumSources, SourceOverflowSentinel = MaximumSources + 1,
                DisplayCandidatesPerUnit = capturedLinkageDiscovery ? 0 : MaximumCandidates,
                CapturedLinkageCandidatesPerUnit = capturedLinkageDiscovery ? MaximumCandidates : 0,
                CandidateOverflowSentinel = MaximumCandidates + 1,
                ExplicitPrimaryAlternateComponentEntries = MaximumEntries, ParametersPerList = 16,
                SupportForwardingBodyDistinctOperations = forwardingBody ? 64 : 0,
                SupportForwardingBodyStructuralOccurrences = forwardingBody ? 128 : 0,
                ReceiverWrapperDepth = 16, ReceiverLayoutFields = 32, InvocationArguments = 4096,
                InvocationUtf8Bytes = 1024 * 1024, Capabilities = 512,
                CurrentPhysicalSourceBytes = MaximumPhysicalSourceBytes, CurrentPhysicalBytesPerFile = 64L * 1024 * 1024,
                TotalCommandsIncludingOpen = MaximumTotalCommands, TotalRowsIncludingOpen = MaximumTotalRows,
                LookupCommandsAfterOpen = MaximumLookupCommands, LookupRowsAfterOpen = MaximumLookupRows,
                WholePhaseSeconds = 120, SampledWorkingSetBytes = 1024L * 1024 * 1024,
                CachedRowsPerObject = 4096, CachedPagesPerObject = 128, PageSize = 128, CommandTimeoutSeconds = 30,
                Accounting = "Every reader operation is followed by aggregate checks, including mandatory Open. Open totals and post-Open deltas are both retained. Synchronous operations may reach a check only after returning; coordinator must enforce whole-process time/RSS independently.",
                IndexCaveat = capturedLinkageDiscovery
                    ? "Public captured-linkage lookup returns at most maximum+1. Current-name arms use existing indexes; assembly-name override arms may scan. Returned row counts do not measure engine scan work, and no program-wide uniqueness is claimed."
                    : "Public indexed display-name materialization is bounded by maximum+1, not an assertion about engine work or program-wide uniqueness."
            }), ("units", units.Select(unit => unit.Report).ToList()), ("compatibility", compatibility), ("issues", issues));
    }

    public void Run(string supportPath)
    {
        var support = Open("support", supportPath, null, null);
        DescribeCapture(support);
        Lookup(support);
        if (forwardingBody) TraceSupportForwardingBody(support);
        foreach (var pin in new[]
        {
            (Name: "consumer293", Path: "/mnt/storage/roslynobj-duckdb-20261014/export/major2/0293/unit.roslynobj", Bytes: 1837615104L, Sha: "e8f1840398a035990420e67e3261db8b2ed044c8e6b471d07d36a7f2542d4a46"),
            (Name: "consumer300", Path: "/mnt/storage/roslynobj-duckdb-20261014/export/major2/0300/unit.roslynobj", Bytes: 415345664L, Sha: "6856d59e243fffd9b1b986beee12651948204195ebe94b200ad516e65fb48841")
        })
        {
            var consumer = Open(pin.Name, pin.Path, pin.Bytes, pin.Sha);
            Lookup(consumer);
        }
        var providers = support.Exact.Where(function => Read(() => function.IsDefined) && Read(() => function.Body) is not null).ToArray();
        Report["supportDefinedExactCandidateCount"] = providers.Length;
        Report["supportDefinedExactCandidateCountIsProviderEligibility"] = false;
        if (providers.Length != 1)
            AddIssue("support-definition-absent-or-ambiguous", "Exactly one retained defined exact-entry candidate with a body association is required for a comparison; no provider is chosen otherwise.");
        else
        {
            foreach (var consumer in units.Skip(1))
            {
                if (consumer.Exact.Count == 0) AddIssue("consumer-exact-declaration-absent", consumer.Name + " has no exact retained entry candidate in the bounded name index.");
                foreach (var declaration in consumer.Exact)
                {
                    var result = Read(() => new AbiCompatibility().CompareFunctions(declaration, providers[0]));
                    compatibility.Add(Row(("consumerUnit", consumer.Name), ("declaration", Ref(declaration)),
                        ("consumerCandidateIsDefined", Read(() => declaration.IsDefined)), ("supportDefinition", Ref(providers[0])),
                        ("status", result.Status.ToString()), ("path", result.Path), ("reason", result.Reason),
                        ("compatible", result.Compatible), ("odrOrSemanticIdentityClaim", false), ("bodyTraversalPerformed", false)));
                }
            }
        }
        Check();
        completed = true;
    }

    private UnitProbe Open(string name, string path, long? expectedBytes, string? expectedSha)
    {
        Check();
        var unit = new UnitProbe(name, path);
        units.Add(unit);
        ((List<Dictionary<string, object?>>)Report["units"]!).Add(unit.Report);
        var bytes = new FileInfo(path).Length;
        unit.Report["bytes"] = bytes;
        unit.Report["expectedSha256"] = expectedSha;
        if (expectedBytes is not null && bytes != expectedBytes) throw new InvalidDataException(name + " length differs from the explicit pin; reader not opened.");
        unit.Before = Hash(path);
        unit.Report["sha256"] = unit.Before;
        if (expectedSha is not null && unit.Before != expectedSha) throw new InvalidDataException(name + " hash differs from the explicit pin; reader not opened.");
        unit.Input = RoslynObject.Open(path, new ReadOptions { CachedRowCount = 4096, CachedPageCount = 128, PageSize = 128, CommandTimeoutSeconds = 30 });
        unit.Opened = unit.Input.Statistics;
        unit.Report["mandatoryOpenStatistics"] = Statistics(unit.Opened);
        Check();
        unit.Unit = Read(() => unit.Input.TranslationUnit);
        unit.Report["manifest"] = Manifest(Read(() => unit.Input.Manifest));
        return unit;
    }

    private void DescribeCapture(UnitProbe support)
    {
        var unit = support.Unit!;
        support.Report["languageMode"] = Read(() => unit.LanguageMode);
        var primary = Read(() => unit.PrimaryInput);
        support.Report["primaryInputPath"] = primary is null ? null : Read(() => primary.Path);
        var arguments = Bounded(Read(() => unit.InvocationArguments), 4096, "Invocation arguments");
        if (arguments.Sum(argument => (long)Encoding.UTF8.GetByteCount(argument)) > 1024 * 1024)
            throw new ProbeBound("Invocation argument text exceeds one MiB.");
        support.Report["invocationArguments"] = arguments;
        support.Report["capabilities"] = Bounded(Read(() => unit.Capabilities), 512, "Capabilities")
            .Select(capability => new { capability.Name, capability.Value, capability.IsRequired, capability.Enabled, State = capability.State.Name }).ToArray();
        var sources = new List<Dictionary<string, object?>>();
        support.Report["sourceFiles"] = sources;
        support.Report["currentPhysicalFileHashScope"] = "Only absolute paths explicitly retained in Sources; no path search, source parsing or recovery. Hashes describe current readable bytes, not capture-time or historical header identity. All source records are reported; headers are not inferred from suffixes.";
        var paths = new HashSet<string>(StringComparer.Ordinal);
        foreach (var source in Bounded(Read(() => unit.Sources), MaximumSources, "Source files"))
        {
            var path = Read(() => source.Path);
            var row = Row(("source", Ref(source)), ("path", path), ("originKind", Read(() => source.Details.OriginKind.Name)));
            sources.Add(row);
            if (!paths.Add(path)) { row["currentBytesStatus"] = "duplicate-retained-path"; continue; }
            if (!Path.IsPathFullyQualified(path)) { row["currentBytesStatus"] = "nonphysical-or-relative-path-not-resolved"; continue; }
            if (!File.Exists(path)) { row["currentBytesStatus"] = "missing-or-nonphysical"; continue; }
            try
            {
                var before = new FileInfo(path);
                var size = before.Length; var modified = before.LastWriteTimeUtc;
                if (size > 64L * 1024 * 1024 || physicalSourceBytes + size > MaximumPhysicalSourceBytes)
                    throw new ProbeBound("Current source-file byte bound exceeded; remaining current hashes are unknown.");
                physicalSourceBytes += size;
                row["currentBytes"] = size;
                row["currentSha256"] = Hash(path);
                var after = new FileInfo(path);
                row["lengthAndMtimeStableDuringHash"] = size == after.Length && modified == after.LastWriteTimeUtc;
                row["currentBytesStatus"] = "hashed-current-physical-file";
                if (!(bool)row["lengthAndMtimeStableDuringHash"]!) AddIssue("source-changed-during-hash", path);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            { row["currentBytesStatus"] = "unreadable"; row["currentHashError"] = exception.Message; }
        }
        support.Report["retainedSourceCount"] = sources.Count;
        support.Report["currentHashedFileCount"] = sources.Count(row => row.ContainsKey("currentSha256"));
        support.Report["currentPhysicalBytesHashed"] = physicalSourceBytes;
        support.Report["allRetainedPathsCurrentlyHashed"] = sources.All(row => row.ContainsKey("currentSha256") || Equals(row["currentBytesStatus"], "duplicate-retained-path"));
        var currentManifest = sources.Where(row => row.ContainsKey("currentSha256"))
            .OrderBy(row => (string)row["path"]!, StringComparer.Ordinal).Select(row => new { Path = row["path"], Bytes = row["currentBytes"], Sha256 = row["currentSha256"] }).ToArray();
        support.Report["currentPhysicalFileManifestSha256"] = Convert.ToHexStringLower(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(currentManifest)));
    }

    private void Lookup(UnitProbe unit)
    {
        var rows = new List<Dictionary<string, object?>>();
        unit.Report["displayNameHint"] = displayName;
        unit.Report["requiredExactCapturedFinalName"] = finalName;
        unit.Report["discoveryMode"] = capturedLinkageDiscovery ? "public-captured-final-linkage-candidates" : "public-display-name-index-candidates";
        unit.Report["displayNameHintUsedForDiscovery"] = !capturedLinkageDiscovery;
        unit.Report["candidates"] = rows;
        var model = Read(() => unit.Unit!.SemanticModel);
        var candidates = capturedLinkageDiscovery
            ? Read(() => model.FindCapturedLinkageCandidates(finalName, MaximumCandidates))
            : Read(() => model.FindSymbols(displayName));
        var symbols = Bounded(candidates, MaximumCandidates,
            (capturedLinkageDiscovery ? "Captured-linkage candidates in " : "Display-name candidates in ") + unit.Name);
        unit.Report[capturedLinkageDiscovery ? "capturedLinkageCandidateCount" : "displayCandidateCount"] = symbols.Length;
        unit.Report["candidateOverflowSentinelObserved"] = false;
        foreach (var symbol in symbols)
        {
            var row = Row(("symbol", Ref(symbol)), ("displayName", Read(() => symbol.Name)),
                ("currentName", Read(() => symbol.CurrentName)), ("kind", Read(() => symbol.Kind.Name)));
            rows.Add(row);
            if (symbol is not FunctionSymbol function) { row["functionCandidate"] = false; continue; }
            var gnu = Read(() => function.GnuSupplement);
            var assemblyName = gnu is null ? null : Read(() => gnu.AssemblyName);
            var currentName = (string?)row["currentName"];
            var exact = (assemblyName ?? currentName) == finalName;
            row["assemblyName"] = assemblyName;
            row["capturedFinalName"] = assemblyName ?? currentName;
            row["capturedFinalNameRule"] = "Retained GNU AssemblyName when present, otherwise CurrentName; no remangling, stripping or source interpretation.";
            row["exactRequiredFinalName"] = exact;
            if (!exact) continue;
            unit.Exact.Add(function);
            row["exactFunction"] = Function(function);
            row["explicitEntryRelationsNotExecution"] = EntryRelations(function);
        }
        unit.Report["exactCandidateCount"] = unit.Exact.Count;
        unit.Report[capturedLinkageDiscovery ? "capturedLinkageLookupCompleteWithinThisInput" : "indexLookupCompleteWithinThisInput"] = true;
    }

    private object EntryRelations(FunctionSymbol root)
    {
        var relations = new Dictionary<FunctionSymbol, (FunctionSymbol? Primary, FunctionSymbol[] Alternates)>();
        var queue = new Queue<FunctionSymbol>(); var seen = new HashSet<FunctionSymbol> { root }; queue.Enqueue(root);
        while (queue.TryDequeue(out var current))
        {
            var primary = Read(() => current.PrimaryEntry);
            var alternates = Bounded(Read(() => current.AlternateEntryPoints), MaximumEntries, "Alternate entries");
            relations.Add(current, (primary, alternates));
            foreach (var next in primary is null ? alternates : alternates.Prepend(primary))
            {
                if (seen.Contains(next)) continue;
                if (seen.Count == MaximumEntries) throw new ProbeBound("Explicit primary/alternate component exceeds sixteen entries.");
                seen.Add(next); queue.Enqueue(next);
            }
        }
        return relations.Select(pair => new
        {
            Function = Function(pair.Key), Primary = Ref(pair.Value.Primary),
            PrimaryReciprocallyListsEntry = pair.Value.Primary is null ? (bool?)null : relations[pair.Value.Primary].Alternates.Contains(pair.Key),
            Alternates = pair.Value.Alternates.Select(alternate => new { Entry = Ref(alternate), PrimaryIsThisEntry = relations[alternate].Primary?.Equals(pair.Key) }).ToArray()
        }).ToArray();
    }

    private void TraceSupportForwardingBody(UnitProbe support)
    {
        if (support.Exact.Count != 1) throw new InvalidDataException("Forwarding-body mode requires one exact support entry; no consumer or alternate body is substituted.");
        var function = support.Exact[0];
        if (Read(() => function.SpecialFunctionKind?.Name) != "sfk_constructor" ||
            Read(() => function.ConstructorOrDestructorKind?.Name) != "cdk_subobject")
            throw new InvalidDataException("Forwarding-body mode inspects only the supplied support exact subobject constructor entry.");
        var body = Read(() => function.Body) ?? throw new InvalidDataException("Exact support C2 has no retained body association.");
        var primary = Read(() => function.PrimaryEntry);
        var parameters = Bounded(Read(() => function.Parameters), 16, "C2 selected parameters");
        var parameterSymbols = parameters.Select(parameter => (Parameter: parameter, Symbol: Read(() => parameter.Symbol))).ToArray();
        var nodes = new List<Dictionary<string, object?>>(); var occurrences = new List<object>();
        var children = new Dictionary<SemanticNode, StructuralChildDetails[]>();
        var operations = new HashSet<Operation>(); var active = new HashSet<SemanticNode>();
        var facts = Row(("function", Ref(function)), ("primaryEntryAssociationOnly", Ref(primary)),
            ("bodyRoot", Ref(Read(() => body.Root))), ("bodyFunctionIsExactC2", Read(() => body.Function.Equals(function))),
            ("scope", "Only this exact support C2 structural body. Primary C1 and callee bodies, consumer bodies, type graphs and NoThrow proof are not traversed. Structural ordinals/aliases are facts, not inferred evaluation order or ABI argument remapping."),
            ("parameters", parameters.Select(ParameterAssociation).ToArray()), ("nodes", nodes), ("occurrences", occurrences),
            ("completeWithinThisBody", false), ("structuralOccurrenceCount", 0));
        Report["forwardingBody"] = facts;
        Report["typedBodyTracePerformed"] = true;
        Report["typedBodyDistinctOperationCount"] = 0;
        Walk(Read(() => body.Root), null, "body-root", 0);
        facts["completeWithinThisBody"] = true;

        void Walk(SemanticNode node, SemanticNode? parent, string role, long ordinal)
        {
            Check();
            if (occurrences.Count == 128) throw new ProbeBound("Support C2 structural occurrence bound exceeded; remaining children are unknown.");
            var metadata = Read(() => node.RepresentationRole.Name) == "metadata";
            occurrences.Add(new { Ordinal = occurrences.Count, Node = Ref(node), Parent = Ref(parent), Role = role, RoleOrdinal = ordinal,
                MetadataBoundaryNotTraversed = metadata, RepeatedIdentity = children.ContainsKey(node) });
            facts["structuralOccurrenceCount"] = occurrences.Count;
            if (metadata) return;
            if (node is Operation operation)
            {
                Report["typedBodyOccurrenceCount"] = (int)Report["typedBodyOccurrenceCount"]! + 1;
                if (!operations.Contains(operation) && operations.Count == 64) throw new ProbeBound("Support C2 distinct-operation bound exceeded.");
                operations.Add(operation); Report["typedBodyDistinctOperationCount"] = operations.Count;
            }
            if (!active.Add(node)) throw new ProbeBound("Support C2 structural cycle encountered; remaining cycle is not expanded.");
            try
            {
                if (!children.TryGetValue(node, out var edges))
                {
                    edges = Bounded(Read(() => node.ChildRelations), 128, "C2 structural child relations"); children.Add(node, edges);
                    var row = Row(("node", Ref(node)), ("nodeKind", Read(() => node.Kind.Name)),
                        ("representationRole", Read(() => node.RepresentationRole.Name)), ("isOperation", node is Operation),
                        ("typeAssociationOnly", node is Operation typed ? Ref(Read(() => typed.Type)) : null),
                        ("structuralEdgesNotSchedule", edges.Select(edge => new { Role = Read(() => edge.Role.Name), Ordinal = Read(() => edge.Ordinal), Child = Ref(Read(() => edge.Child)) }).ToArray()));
                    nodes.Add(row);
                    ExpressionOperation[] operands = [];
                    if (node is ExpressionOperation expression)
                    {
                        operands = Bounded(Read(() => expression.Operands), 16, "C2 raw operands");
                        row["valueCategory"] = Read(() => expression.ValueCategory.Name);
                        row["rawOperands"] = operands.Select(Ref).ToArray();
                        row["operandRelations"] = Bounded(Read(() => expression.OperandRelations), 16, "C2 operand relations")
                            .Select(edge => new { Role = Read(() => edge.Role.Name), Ordinal = Read(() => edge.Ordinal), Expression = Ref(Read(() => edge.Expression)) }).ToArray();
                    }
                    if (node is StatementOperation statement) row["statementExpressionAlias"] = Ref(Read(() => statement.Expression));
                    if (node is OperatorOperation op)
                        row["operator"] = new { Kind = Read(() => op.OperatorKind.Name), EvalLeftToRight = Read(() => op.Operator.EvalLeftToRight),
                            EvalRightToLeft = Read(() => op.Operator.EvalRightToLeft), RetainedVirtualCall = Read(() => op.Operator.RetainedVirtualCall),
                            QualifiedFunctionName = Read(() => op.Operator.QualifiedFunctionName), IsConversionCall = Read(() => op.Operator.IsConversionCall),
                            CallUsesOperatorSyntax = Read(() => op.Operator.CallUsesOperatorSyntax) };
                    if (node is ReferenceOperation reference)
                    {
                        var target = Read(() => reference.Target); row["referenceTarget"] = Ref(target);
                        row["exactC2ParameterSymbolMatches"] = parameterSymbols.Where(pair => pair.Symbol?.Equals(target) == true)
                            .Select(pair => ParameterAssociation(pair.Parameter)).ToArray();
                    }
                    if (node is CallOperation call)
                    {
                        var callee = Read(() => call.Callee); var target = Read(() => call.TargetMethod);
                        var receiver = Read(() => call.Receiver); var functionType = Read(() => call.Call.FunctionType);
                        row["call"] = new
                        {
                            Kind = Read(() => call.CallKind.Name), TargetMethod = Ref(target), Callee = Ref(callee),
                            CalleeReferenceTarget = callee is ReferenceOperation calleeReference ? Ref(Read(() => calleeReference.Target)) : null,
                            TargetIsExactC2PrimaryEntry = target is not null && target.Equals(primary),
                            Receiver = Ref(receiver), ReturnSlot = Ref(Read(() => call.Call.ReturnSlot)), FunctionType = Ref(functionType),
                            FunctionTypeIsTargetSignature = target is null ? (bool?)null : functionType?.Equals(Read(() => target.Signature)),
                            TargetSelectedParameters = target is null ? null : Bounded(Read(() => target.Parameters), 16, "Call target selected parameter associations").Select(ParameterAssociation).ToArray(),
                            SourceDispatchKind = Read(() => call.SourceDispatchKind?.Name), SourceTargetMethodAssociationOnly = Ref(Read(() => call.SourceTargetMethod)),
                            CalleeRawOperandIndices = operands.Select((operand, index) => (operand, index)).Where(pair => pair.operand.Equals(callee)).Select(pair => pair.index).ToArray(),
                            Arguments = Bounded(Read(() => call.Arguments), 16, "C2 call arguments").Select(argument => new
                            {
                                Ordinal = Read(() => argument.Ordinal), Role = Read(() => argument.Role?.Name), Parameter = Ref(Read(() => argument.Parameter)),
                                Expression = Ref(Read(() => argument.Value)),
                                RawOperandIndices = operands.Select((operand, index) => (operand, index)).Where(pair => pair.operand.Equals(Read(() => argument.Value))).Select(pair => pair.index).ToArray()
                            }).ToArray()
                        };
                    }
                }
                foreach (var edge in edges) Walk(Read(() => edge.Child), node, Read(() => edge.Role.Name), Read(() => edge.Ordinal));
            }
            finally { active.Remove(node); }
        }
    }

    private object ParameterAssociation(Parameter parameter)
    {
        var symbol = Read(() => parameter.Symbol);
        return new { Ordinal = Read(() => parameter.Ordinal), Role = Read(() => parameter.Role.Name),
            Descriptor = Ref(Read(() => parameter.Descriptor)), Type = Ref(Read(() => parameter.Type)),
            DeclaredType = Ref(Read(() => parameter.DeclaredType)), Symbol = Ref(symbol),
            SymbolType = symbol is null ? null : Ref(Read(() => symbol.Type)), IsSourceParameter = Read(() => parameter.IsSourceParameter) };
    }

    private object Function(FunctionSymbol function)
    {
        var details = Read(() => function.Details); var gnu = Read(() => function.GnuSupplement);
        var body = Read(() => function.Body); var parameters = Bounded(Read(() => function.Parameters), 16, "Execution parameters");
        var signature = Read(() => function.Signature); var specification = Read(() => signature.ExceptionSpecification);
        var weak = Read(() => details.IsWeak); var weakref = Read(() => details.IsWeakref);
        var alias = Read(() => details.IsGnuAlias); var ifunc = Read(() => details.IsIfunc);
        var comdat = Read(() => details.IsUseComdat); var aliasEntry = Read(() => details.IsAliasEntry);
        var aliased = gnu is null ? null : Read(() => gnu.AliasedRoutine);
        FunctionSymbol[] versions = gnu is null ? [] : Bounded(Read(() => gnu.TargetedVersions), 16, "GNU targeted versions");
        var defined = Read(() => function.IsDefined);
        var correspondence = Read(() => function.SourceCorrespondence);
        return Row(("function", Ref(function)), ("currentName", Read(() => function.CurrentName)),
            ("assemblyName", gnu is null ? null : Read(() => gnu.AssemblyName)),
            ("capturedNameFacts", correspondence is null ? null : new
            {
                CurrentName = Read(() => correspondence.CurrentName), NameHasBeenMangled = Read(() => correspondence.NameHasBeenMangled),
                UnmangledNameOrMangledEncoding = Read(() => correspondence.Details.UnmangledNameOrMangledEncoding),
                MangledEncoding = Read(() => correspondence.MangledEncoding),
                UnnamedEntityGivenFabricatedName = Read(() => correspondence.UnnamedEntityGivenFabricatedName)
            }),
            ("isDefined", defined), ("bodyState", Read(() => function.BodyState.Name)),
            ("bodyRootAssociationNotTraversal", body is null ? null : Ref(Read(() => body.Root))),
            ("bodyFunctionAssociation", body is null ? null : Ref(Read(() => body.Function))),
            ("bodyFunctionIsThisFunction", body is null ? null : Read(() => (bool?)body.Function.Equals(function))),
            ("bodyScopeAssociationNotTraversal", body is null ? null : Ref(Read(() => body.Scope))),
            ("specialFunctionKind", Read(() => function.SpecialFunctionKind?.Name)),
            ("entryKind", Read(() => function.ConstructorOrDestructorKind?.Name)),
            ("source", Source(Read(() => function.Source))),
            ("containingType", ReceiverType(Read(() => function.ContainingType))),
            ("templateAssociations", new
            {
                FunctionTemplate = TemplateFacts(Read(() => function.Template)),
                FunctionInstantiation = InstantiationFacts(Read(() => function.Instantiation)),
                ContainingRecord = ContainingRecordTemplateFacts(Read(() => function.ContainingType))
            }),
            ("thisAdjustment", Number(Read(() => function.ThisAdjustment))),
            ("returnAdjustment", Number(Read(() => function.ReturnAdjustment))),
            ("virtualCallIndexNotComputedOffset", Number(Read(() => function.VirtualCallIndex))),
            ("virtualBaseIndexNotComputedOffset", Number(Read(() => function.VirtualBaseIndex))),
            ("policy", new
            {
                IsWeak = weak, IsWeakref = weakref, IsGnuAlias = alias, IsIfunc = ifunc, IsUseComdat = comdat, IsAliasEntry = aliasEntry,
                IsInline = Read(() => details.IsInline), DefinitionForInliningOnly = Read(() => details.DefinitionForInliningOnly),
                ExplicitInstantiation = Read(() => details.ExplicitInstantiation),
                ClassExplicitlyInstantiated = Read(() => details.ClassExplicitlyInstantiated),
                ExplicitDoNotInstantiate = Read(() => details.ExplicitDoNotInstantiate),
                IsTemplateFunction = Read(() => details.IsTemplateFunction),
                IsPrototypeInstantiation = Read(() => details.IsPrototypeInstantiation),
                SuppressInlineBody = Read(() => details.SuppressInlineBody), SupersededExternal = Read(() => details.SupersededExternal),
                IsDeleted = Read(() => details.IsDeleted), AliasedRoutine = Ref(aliased), TargetedVersions = versions.Select(Ref).ToArray(),
                Representative = gnu is null ? null : Ref(Read(() => gnu.Representative)),
                IsRepresentative = gnu is null ? null : Read(() => gnu.IsRepresentative),
                IsTargetSpecificVersion = gnu is null ? null : Read(() => gnu.IsTargetSpecificVersion),
                ResolverRequired = gnu is null ? null : Read(() => gnu.ResolverRequired)
            }),
            ("providerFactsOnly", new
            {
                DefinedBodyAssociation = defined && body is not null,
                CandidateStatus = !defined || body is null ? "not-a-defined-body-candidate" : "defined-body-association-only; explicit-binding-policy-and-body-proof-not-decided",
                WeakAliasIfuncOrTargetedResolutionRequired = weak == true || weakref == true || alias == true || ifunc == true || aliased is not null || versions.Length != 0,
                ExplicitComdatOrAlternateEntryPolicyRequired = comdat == true || aliasEntry == true,
                OptionalPolicyFlagsUnavailable = weak is null || weakref is null || alias is null || ifunc is null || comdat is null || aliasEntry is null,
                ProductionProviderAdmissionDecided = false, BodyProofPerformed = false
            }),
            ("signature", new
            {
                Type = Ref(signature), ReturnType = Type(Read(() => signature.ReturnType)), ThisClass = Ref(Read(() => signature.ThisClass)),
                HasPrototype = Read(() => signature.HasPrototype), IsVariadic = Read(() => signature.IsVariadic),
                HasThisParameter = Read(() => signature.HasThisParameter), ReturnsValueAsParameter = Read(() => signature.ReturnsValueAsParameter),
                ReturnsValueViaCopyConstructor = Read(() => signature.ReturnsValueViaCopyConstructor),
                ReturnValueParameterFollowsThis = Read(() => signature.ReturnValueParameterFollowsThis),
                CallingConvention = Read(() => signature.CallingConvention?.Name), LanguageLinkage = Read(() => signature.LanguageLinkage?.Name),
                NoexceptEffective = specification is null ? null : Read(() => specification.NoexceptEffective),
                SignatureParameters = Bounded(Read(() => signature.Parameters), 16, "Signature parameters").Select(ParameterFacts).ToArray()
            }), ("executionParameters", parameters.Select(ParameterFacts).ToArray()),
            ("sourceParameters", Bounded(Read(() => function.SourceParameters), 16, "Source parameters").Select(ParameterFacts).ToArray()));
    }

    private object? TemplateFacts(TemplateDescriptor? descriptor)
    {
        if (descriptor is null) return null;
        return new
        {
            Descriptor = Ref(descriptor), TemplateKind = Read(() => descriptor.TemplateKind.Name),
            OriginalDefinition = Ref(Read(() => descriptor.OriginalDefinition)),
            Definition = Ref(Read(() => descriptor.Definition)), Prototype = Ref(Read(() => descriptor.Prototype)),
            PatternAssociationNotTraversal = Ref(Read(() => descriptor.Pattern)),
            RecursiveTemplateExpansionPerformed = false
        };
    }

    private object? InstantiationFacts(TemplateInstantiation? instance)
    {
        if (instance is null) return null;
        return new
        {
            Instance = Ref(instance), OriginalDefinition = TemplateFacts(Read(() => instance.OriginalDefinition)),
            ResultAssociationOnly = Ref(Read(() => instance.Result)), State = Read(() => instance.State.Name),
            IsDependent = Read(() => instance.IsDependent), IsPrototype = Read(() => instance.IsPrototype),
            IsSuppressed = Read(() => instance.IsSuppressed), IsExplicit = Read(() => instance.IsExplicit),
            IsSpecialized = Read(() => instance.IsSpecialized),
            SelectedSpecializationAssociationOnly = Ref(Read(() => instance.SelectedSpecialization)),
            ArgumentsOrInstanceCollectionsExpanded = false
        };
    }

    private object? ContainingRecordTemplateFacts(CppType? type)
    {
        if (type is null) return null;
        var seen = new HashSet<CppType>();
        while (type is TypeReference reference)
        {
            if (!seen.Add(type) || seen.Count > 16) throw new ProbeBound("Containing record type-wrapper depth/cycle bound exceeded.");
            type = reference is QualifiedType qualified ? Read(() => qualified.UnderlyingType) : Read(() => reference.UnderlyingType);
        }
        if (type is not RecordType record) return new { Type = Ref(type), IsRecord = false };
        return new
        {
            Record = Ref(record), TemplateViaPublicOriginalDefinition = TemplateFacts(Read(() => record.Template)),
            RetainedRecordInstance = InstantiationFacts(Read(() => record.Record.Instance)),
            PublicInstantiation = Ref(Read(() => record.Instantiation)),
            IsTemplateClass = Read(() => record.Record.IsTemplateClass),
            IsPrototypeInstantiation = Read(() => record.Record.IsPrototypeInstantiation),
            IsSpecialized = Read(() => record.Record.IsSpecialized),
            ExplicitlyInstantiated = Read(() => record.Record.ExplicitlyInstantiated),
            DoNotInstantiate = Read(() => record.Record.DoNotInstantiate),
            RecordDetailsHasDirectTemplateProperty = false,
            TemplateAssociationRule = "RecordType.Template is its public instance OriginalDefinition; RecordTypeDetails directly retains Instance."
        };
    }

    private object ParameterFacts(SignatureParameter parameter)
    {
        var descriptor = Read(() => parameter.Descriptor); var role = Read(() => parameter.Role.Name);
        var execution = parameter as Parameter; var symbol = execution is null ? null : Read(() => execution.Symbol);
        var type = Read(() => parameter.Type);
        return Row(("ordinal", Read(() => parameter.Ordinal)), ("role", role), ("descriptor", Ref(descriptor)),
            ("type", role == "this_parameter" ? ReceiverType(type) : Type(type)),
            ("declaredType", Type(Read(() => parameter.DeclaredType))), ("symbol", Ref(symbol)),
            ("symbolType", symbol is null ? null : ReceiverType(Read(() => symbol.Type))),
            ("passedViaCopyConstructor", Read(() => descriptor.Details.PassedViaCopyConstructor)),
            ("isUnaligned", Read(() => descriptor.Details.IsUnaligned)), ("namedAddressSpace", Read(() => descriptor.Details.NamedAddressSpace)),
            ("baseSubobjectAssociationOnly", Ref(Read(() => parameter.BaseSubobject))),
            ("isSourceParameter", execution is null ? null : Read(() => (bool?)execution.IsSourceParameter)));
    }

    private object? ReceiverType(CppType? start)
    {
        if (start is null) return null;
        var chain = new List<object>(); var seen = new HashSet<CppType>(); var current = start;
        while (true)
        {
            if (!seen.Add(current)) throw new ProbeBound("Receiver wrapper cycle encountered.");
            if (chain.Count == 16) throw new ProbeBound("Receiver type-wrapper depth exceeds sixteen.");
            var row = Type(current)!; chain.Add(row);
            if (current is QualifiedType qualified)
            { row["isConst"] = Read(() => qualified.IsConst); row["isVolatile"] = Read(() => qualified.IsVolatile); current = Read(() => qualified.UnderlyingType); }
            else if (current is TypeReference reference) current = Read(() => reference.UnderlyingType);
            else if (current is PointerType pointer)
            {
                row["isReference"] = Read(() => pointer.IsReference); row["isRValueReference"] = Read(() => pointer.IsRValueReference);
                row["isPtr32"] = Read(() => pointer.Pointer.IsPtr32); row["isPtr64"] = Read(() => pointer.Pointer.IsPtr64);
                row["isHandle"] = Read(() => pointer.Pointer.IsHandle); row["isTrackingReference"] = Read(() => pointer.Pointer.IsTrackingReference);
                row["isInterior"] = Read(() => pointer.Pointer.IsInterior); row["isPinned"] = Read(() => pointer.Pointer.IsPinned);
                row["isSptr"] = Read(() => pointer.Pointer.IsSptr); row["isUptr"] = Read(() => pointer.Pointer.IsUptr);
                row["basedVariableAssociationOnly"] = Ref(Read(() => pointer.Pointer.BasedVariable));
                current = Read(() => pointer.PointeeType);
            }
            else
            {
                if (current is RecordType record)
                {
                    var layout = Read(() => record.Layout);
                    row["definitionState"] = Read(() => record.DefinitionState.Name);
                    row["hasSubobjectType"] = Read(() => record.Record.HasSubobjectType);
                    row["subobjectPartnerAssociationOnly"] = Type(Read(() => record.SubobjectPartner));
                    row["layout"] = layout is null ? null : new
                    {
                        Type = Ref(Read(() => layout.Type)), Size = Number(Read(() => layout.Size)), Alignment = Number(Read(() => layout.Alignment)),
                        NonvirtualSize = Number(Read(() => layout.NonvirtualSize)), NonvirtualAlignment = Number(Read(() => layout.NonvirtualAlignment)),
                        IsComplete = Read(() => layout.IsComplete),
                        Fields = Bounded(Read(() => layout.Fields), 32, "Receiver layout fields").Select(field => new
                        {
                            field.Ordinal, Field = Ref(Read(() => field.Field)), Offset = Number(Read(() => field.Offset)),
                            BitOffset = Number(Read(() => field.BitOffset)), BitWidth = Number(Read(() => field.BitWidth)),
                            Type = Type(Read(() => field.Field.Type))
                        }).ToArray()
                    };
                }
                break;
            }
            row["nextType"] = Ref(current);
        }
        return chain;
    }

    private Dictionary<string, object?>? Type(CppType? type) => type is null ? null : Row(("type", Ref(type)),
        ("kind", Read(() => type.Kind.Name)), ("size", Number(Read(() => type.Size))), ("alignment", Number(Read(() => type.Alignment))),
        ("descriptorSize", Number(Read(() => type.DescriptorSize))), ("descriptorAlignment", Number(Read(() => type.DescriptorAlignment))),
        ("isIncomplete", Read(() => type.IsIncomplete)), ("descriptorIsIncomplete", Read(() => type.DescriptorIsIncomplete)),
        ("isDependent", Read(() => type.IsDependent)));

    private object? Source(SourceInfo? source)
    {
        var position = source is null ? null : Read(() => source.Position);
        if (position is null) return null;
        var file = Read(() => position.File);
        return new { File = file is null ? null : Read(() => file.Path), LogicalLine = Number(Read(() => position.LogicalLine)), PhysicalLine = Number(Read(() => position.PhysicalLine)), Column = Read(() => position.Column) };
    }

    private string? Ref(SemanticEntity? entity)
    {
        if (entity is null) return null;
        if (!identities.TryGetValue(entity, out var value))
        {
            if (identities.Count == 8192) throw new ProbeBound("Public diagnostic identity bound exceeded.");
            identities.Add(entity, value = $"entity/{identities.Count + 1:D6}");
        }
        return value;
    }
    private T Read<T>(Func<T> read) { var value = read(); Check(); return value; }
    private T[] Bounded<T>(IEnumerable<T> values, int maximum, string description)
    {
        var retained = new List<T>(); using var iterator = values.GetEnumerator();
        while (Read(iterator.MoveNext))
        {
            if (retained.Count == maximum) throw new ProbeBound(description + " overflow sentinel encountered; remaining associations are unknown.");
            retained.Add(iterator.Current);
        }
        return retained.ToArray();
    }
    private void Check()
    {
        if (watch.Elapsed > TimeSpan.FromSeconds(120)) throw new ProbeBound("Whole phase exceeds 120 seconds, including hashing/Open/query.");
        if (Environment.WorkingSet > 1024L * 1024 * 1024) throw new ProbeBound("Sampled process working set exceeds one GiB.");
        long commands = 0, rows = 0, lookupCommands = 0, lookupRows = 0;
        foreach (var unit in units.Where(unit => unit.Input is not null))
        {
            var statistics = unit.Input!.Statistics;
            commands += statistics.Commands; rows += statistics.RowsRead;
            if (unit.Opened is { } opened) { lookupCommands += statistics.Commands - opened.Commands; lookupRows += statistics.RowsRead - opened.RowsRead; }
            if (statistics.CachedRows > 4096 || statistics.CachedPages > 128 || statistics.MaximumBatchSize > 900)
                throw new ProbeBound("Reader cache/page/batch bound exceeded.");
        }
        if (commands > MaximumTotalCommands || rows > MaximumTotalRows || lookupCommands > MaximumLookupCommands || lookupRows > MaximumLookupRows)
            throw new ProbeBound("Aggregate public-reader command/row bound exceeded.");
    }
    private string Hash(string path)
    {
        Check(); using var file = File.OpenRead(path); using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var buffer = new byte[1024 * 1024]; int count;
        while ((count = file.Read(buffer, 0, buffer.Length)) != 0) { hash.AppendData(buffer, 0, count); Check(); }
        return Convert.ToHexStringLower(hash.GetHashAndReset());
    }
    public void AddIssue(string code, string message) => issues.Add(new { code, message });
    public void Finish()
    {
        foreach (var unit in units)
        {
            if (unit.Input is not null)
            {
                var statistics = unit.Input.Statistics;
                unit.Report["totalReaderStatistics"] = Statistics(statistics);
                if (unit.Opened is { } opened) unit.Report["lookupStatisticsAfterOpen"] = new { Commands = statistics.Commands - opened.Commands, RowsRead = statistics.RowsRead - opened.RowsRead };
            }
            if (unit.Before is not null)
            {
                try
                {
                    unit.Report["afterSha256"] = Hash(unit.Path);
                    unit.Report["unchanged"] = Equals(unit.Before, unit.Report["afterSha256"]);
                    if (!(bool)unit.Report["unchanged"]!) AddIssue("input-changed", unit.Name);
                }
                catch (Exception exception) { AddIssue("after-hash-failed", unit.Name + ": " + exception.Message); }
            }
        }
        var totals = units.Where(unit => unit.Input is not null).Select(unit => unit.Input!.Statistics).ToArray();
        Report["wholePhaseReaderTotalsIncludingOpen"] = new { Commands = totals.Sum(value => value.Commands), RowsRead = totals.Sum(value => value.RowsRead) };
        Report["elapsedSeconds"] = watch.Elapsed.TotalSeconds;
        Report["sampledFinalWorkingSetBytes"] = Environment.WorkingSet;
        Report["inspectionComplete"] = completed && issues.Count == 0;
        foreach (var unit in units) unit.Input?.Dispose();
    }
    private static string? Number(BigInteger? value) => value?.ToString(CultureInfo.InvariantCulture);
    private static object Manifest(ObjectManifest value) => new
    {
        value.ArtifactIdentity, value.FormatMajor, value.FormatMinor, value.Producer, value.EdgRevision, value.EdgVersion,
        value.CaptureProfile, value.Stage, Target = new { value.Target.Abi, value.Target.AddressableUnitBits, value.Target.Endianness, PointerBits = Number(value.Target.PointerBits), PointerAlignment = Number(value.Target.PointerAlignment) }
    };
    private static object Statistics(ReaderStatistics value) => new { value.Commands, value.RowsRead, value.CachedRows, value.CachedPages, value.MaximumBatchSize };
    private static Dictionary<string, object?> Row(params (string Key, object? Value)[] values) => values.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);

    private sealed class UnitProbe(string name, string path)
    {
        public string Name { get; } = name;
        public string Path { get; } = path;
        public RoslynObject? Input { get; set; }
        public TranslationUnit? Unit { get; set; }
        public ReaderStatistics? Opened { get; set; }
        public string? Before { get; set; }
        public List<FunctionSymbol> Exact { get; } = [];
        public Dictionary<string, object?> Report { get; } = Row(("name", name), ("path", path));
    }
}
