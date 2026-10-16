using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using RoslynObj;

if (args.Length != 2) throw new ArgumentException("DuckdbDefinitionFacts REQUEST.json NEW_REPORT.json");
var requestPath = Path.GetFullPath(args[0]);
var outputPath = Path.GetFullPath(args[1]);
if (File.Exists(outputPath) || Directory.Exists(outputPath)) throw new ArgumentException("The report path must be fresh.");
var watch = Stopwatch.StartNew();
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true };
var issues = new List<object>();
var report = new Dictionary<string, object?>
{
    ["probeVersion"] = 1,
    ["lookupComplete"] = false,
    ["inventoryOnly"] = true,
    ["translatorAccepted"] = false,
    ["linkClosureComplete"] = false,
    ["generatedExecutionAccepted"] = false,
    ["typedBodyTracePerformed"] = false,
    ["typedBodyOccurrenceCount"] = 0,
    ["typeClosurePerformed"] = false,
    ["programWideBindingClaim"] = false,
    ["scope"] = "One manifest-pinned entry 293 only. Display-name candidates followed by exact captured final-name comparison; body/signature associations only, no body traversal, recursive types or fallback enumeration.",
    ["bounds"] = new
    {
        InputObjects = 1, RetainedCandidates = 4, CandidateOverflowSentinel = 5, ParametersPerList = 16,
        LookupCommands = 128, LookupRows = 512, CachedRows = 4096, CachedPages = 128, PageSize = 128,
        CommandTimeoutSeconds = 30, MaximumBatchSize = 900,
        MandatoryOpenAccounting = "Raw Open statistics are separate; command/row limits apply to subsequent lookup deltas. Whole-process time/RSS are measured by the coordinator.",
        CandidateIndexCaveat = "Take bounds materialized candidates, not internal name-index work. A fifth candidate stops the lookup as bounded ambiguity."
    },
    ["issues"] = issues
};
LookupRequest? request = null;
RoslynObject? input = null;
DefinitionProbe? probe = null;
string? before = null;
string? after = null;
try
{
    if (new FileInfo(requestPath).Length > 65536) throw new InvalidDataException("Request size bound exceeded.");
    var requestBytes = File.ReadAllBytes(requestPath);
    request = JsonSerializer.Deserialize<LookupRequest>(requestBytes, options) ?? throw new InvalidDataException("Request is absent.");
    request.Validate();
    report["request"] = new { path = requestPath, sha256 = Convert.ToHexStringLower(SHA256.HashData(requestBytes)), values = request };
    if (new FileInfo(request.ManifestPath).Length > 32L * 1024 * 1024) throw new InvalidDataException("Manifest size bound exceeded.");
    var manifestBytes = File.ReadAllBytes(request.ManifestPath);
    var manifestSha = Convert.ToHexStringLower(SHA256.HashData(manifestBytes));
    if (manifestSha != request.ManifestSha256) throw new InvalidDataException("The pinned core manifest hash differs.");
    using var manifest = JsonDocument.Parse(manifestBytes);
    var entries = manifest.RootElement.GetProperty("published_objects").EnumerateArray()
        .Where(entry => entry.GetProperty("entry_index").GetInt32() == request.EntryIndex).Take(2).ToArray();
    if (entries.Length != 1) throw new InvalidDataException("The pinned manifest entry is absent or ambiguous.");
    var entry = entries[0];
    if (entry.GetProperty("path").GetString() != request.InputPath || entry.GetProperty("sha256").GetString() != request.InputSha256 ||
        entry.GetProperty("bytes").GetInt64() != request.InputBytes)
        throw new InvalidDataException("The requested object does not match the pinned manifest entry.");
    report["coreManifest"] = new { path = request.ManifestPath, sha256 = manifestSha, bytes = manifestBytes.Length, entry = entry.Clone() };
    if (new FileInfo(request.InputPath).Length != request.InputBytes) throw new InvalidDataException("Input length differs from the pinned object.");
    before = Hash(request.InputPath);
    if (before != request.InputSha256) throw new InvalidDataException("Input content differs from the pinned object; the public reader was not opened.");
    input = RoslynObject.Open(request.InputPath, new ReadOptions { CachedRowCount = 4096, CachedPageCount = 128, PageSize = 128 });
    var opened = input.Statistics;
    report["mandatoryOpenStatistics"] = Statistics(opened);
    var m = input.Manifest;
    report["objectManifest"] = new
    {
        m.ArtifactIdentity, m.FormatMajor, m.FormatMinor, m.Producer, m.EdgRevision, m.EdgVersion, m.CaptureProfile, m.Stage,
        target = new { m.Target.Abi, m.Target.AddressableUnitBits, m.Target.Endianness, PointerBits = m.Target.PointerBits.ToString(CultureInfo.InvariantCulture), PointerAlignment = m.Target.PointerAlignment.ToString(CultureInfo.InvariantCulture) }
    };
    probe = new DefinitionProbe(input, opened, issues);
    report["candidates"] = probe.Candidates;
    probe.Build(report, request);
}
catch (Exception exception)
{
    issues.Add(new { code = exception is LookupBound ? "lookup-bound" : "lookup-failed", message = exception.Message });
}
finally
{
    if (input is not null)
    {
        var final = input.Statistics;
        report["totalReaderStatistics"] = Statistics(final);
        if (probe is not null) report["lookupStatistics"] = probe.Delta(final);
        input.Dispose();
    }
    if (request is not null && before is not null)
    {
        try
        {
            after = Hash(request.InputPath);
            if (after != before) issues.Add(new { code = "input-changed", message = "The input hash changed during this read-only lookup." });
        }
        catch (IOException exception) { issues.Add(new { code = "after-hash-failed", message = exception.Message }); }
        report["input"] = new { path = request.InputPath, bytes = request.InputBytes, sha256 = before, afterSha256 = after, unchanged = before == after };
    }
}
report["lookupComplete"] = probe?.Completed == true && issues.Count == 0 && before == after && before is not null;
report["elapsedSeconds"] = watch.Elapsed.TotalSeconds;
Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
using (var output = new FileStream(outputPath, FileMode.CreateNew, FileAccess.Write)) JsonSerializer.Serialize(output, report, options);
Console.WriteLine($"duckdb-definition lookup-complete={report["lookupComplete"]}; retained-candidates={probe?.Candidates.Count ?? 0}; issues={issues.Count}");
return report["lookupComplete"] is true ? 0 : 3;

static string Hash(string path) { using var file = File.OpenRead(path); return Convert.ToHexStringLower(SHA256.HashData(file)); }
static object Statistics(ReaderStatistics value) => new { value.Commands, value.RowsRead, value.CachedRows, value.CachedPages, value.MaximumBatchSize };

internal sealed record LookupRequest(
    string ManifestPath, string ManifestSha256, int EntryIndex, string InputPath, string InputSha256,
    long InputBytes, string DisplayName, string ExpectedFinalLinkage)
{
    public void Validate()
    {
        if (string.IsNullOrEmpty(ManifestPath) || !Path.IsPathFullyQualified(ManifestPath) ||
            ManifestSha256 != "b9f1cc31ab0495204fbfd948cbdeb68374ce227fce1a4ee542c23275183c17ff" || EntryIndex != 293 ||
            InputPath != "/mnt/storage/roslynobj-duckdb-20261014/export/major2/0293/unit.roslynobj" ||
            InputSha256 != "e8f1840398a035990420e67e3261db8b2ed044c8e6b471d07d36a7f2542d4a46" || InputBytes != 1837615104 ||
            DisplayName != "ThrowNullSharedPtrDereference" || ExpectedFinalLinkage != "_ZN6duckdb29ThrowNullSharedPtrDereferenceEv")
            throw new InvalidDataException("The request exceeds the single approved manifest/content/name/linkage lookup scope.");
    }
}

internal sealed class LookupBound(string message) : Exception(message);

internal sealed class DefinitionProbe(RoslynObject input, ReaderStatistics opened, List<object> issues)
{
    private readonly Dictionary<SemanticEntity, string> locators = [];
    public List<Dictionary<string, object?>> Candidates { get; } = [];
    public bool Completed { get; private set; }
    private static Dictionary<string, object?> Row(params (string Key, object? Value)[] fields) => fields.ToDictionary(field => field.Key, field => field.Value, StringComparer.Ordinal);
    private string? Ref(SemanticEntity? value)
    {
        if (value is null) return null;
        if (!locators.TryGetValue(value, out var label))
        {
            if (locators.Count >= 512) throw new LookupBound("Public identity label bound exceeded.");
            locators.Add(value, label = $"entity/{locators.Count + 1:D6}");
        }
        return label;
    }
    public object Delta(ReaderStatistics value) => new { Commands = value.Commands - opened.Commands, RowsRead = value.RowsRead - opened.RowsRead };
    private void Check()
    {
        var value = input.Statistics;
        if (value.Commands - opened.Commands > 128 || value.RowsRead - opened.RowsRead > 512 ||
            value.CachedRows > 4096 || value.CachedPages > 128 || value.MaximumBatchSize > 900)
            throw new LookupBound("Public-reader lookup command/row/cache/batch bound exceeded after a reader operation.");
    }
    private T Read<T>(Func<T> operation) { var result = operation(); Check(); return result; }
    private T[] Bounded<T>(IEnumerable<T> values, int maximum, string description)
    {
        var result = new List<T>();
        using var iterator = values.GetEnumerator();
        while (Read(iterator.MoveNext))
        {
            if (result.Count == maximum) throw new LookupBound(description + " overflow sentinel observed; remaining associations are unknown.");
            result.Add(iterator.Current);
        }
        return result.ToArray();
    }
    public void Build(Dictionary<string, object?> report, LookupRequest request)
    {
        var unit = Read(() => input.TranslationUnit);
        var model = Read(() => unit.SemanticModel);
        var index = Read(() => model.FindSymbols(request.DisplayName));
        var symbols = new List<Symbol>();
        using (var iterator = index.GetEnumerator())
        {
            while (Read(iterator.MoveNext))
            {
                if (symbols.Count == 4)
                {
                    report["fifthCandidateObserved"] = true;
                    report["observedDisplayCandidateCountLowerBound"] = 5;
                    foreach (var retained in symbols) Candidates.Add(Row(("locator", Ref(retained)), ("detailsNotReadAfterOverflow", true)));
                    throw new LookupBound("A fifth display-name candidate was observed; this bounded index sample cannot establish uniqueness or completeness.");
                }
                symbols.Add(iterator.Current);
            }
        }
        report["fifthCandidateObserved"] = false;
        report["materializedDisplayCandidateCount"] = symbols.Count;
        var exactCount = 0;
        var definedCount = 0;
        foreach (var symbol in symbols)
        {
            var row = Row(("locator", Ref(symbol))); Candidates.Add(row);
            var correspondence = Read(() => symbol.SourceCorrespondence);
            var current = Read(() => symbol.CurrentName);
            var display = Read(() => symbol.Name);
            row["sourceNameForDisplayOnly"] = display;
            row["currentName"] = current;
            row["symbolKind"] = Read(() => symbol.Kind.Name);
            row["sourceCorrespondence"] = correspondence is null ? null : Row(
                ("locator", Ref(correspondence)), ("currentName", Read(() => correspondence.CurrentName)),
                ("nameHasBeenMangled", Read(() => correspondence.NameHasBeenMangled)),
                ("unmangledNameOrMangledEncoding", Read(() => correspondence.Details.UnmangledNameOrMangledEncoding)),
                ("mangledEncoding", Read(() => correspondence.MangledEncoding)),
                ("unnamedEntityGivenFabricatedName", Read(() => correspondence.UnnamedEntityGivenFabricatedName)));
            if (display != request.DisplayName) issues.Add(new { code = "display-index-mismatch", message = "The public display-name index returned a different retained display name.", locator = Ref(symbol) });
            if (symbol is not FunctionSymbol function)
            {
                row["isFunctionCandidate"] = false;
                continue;
            }
            row["isFunctionCandidate"] = true;
            var supplement = Read(() => function.GnuSupplement);
            var assembly = supplement is null ? null : Read(() => supplement.AssemblyName);
            var finalName = assembly ?? current;
            var exact = string.Equals(finalName, request.ExpectedFinalLinkage, StringComparison.Ordinal);
            row["assemblyName"] = assembly;
            row["capturedFinalName"] = finalName;
            row["capturedFinalNameRule"] = "Retained GNU AssemblyName when non-null, otherwise retained CurrentName; no stripping, normalization, source lookup or remangling.";
            row["exactRequestedFinalName"] = exact;
            row["comparisonScope"] = "Attribution candidate within this one pinned input; not program-wide definition selection.";
            var defined = Read(() => function.IsDefined);
            var body = Read(() => function.Body);
            row["isDefined"] = defined;
            row["bodyState"] = Read(() => function.BodyState.Name);
            row["bodyRootAssociationNotTraversal"] = Ref(body?.Root);
            row["bodyFunctionAssociation"] = Ref(body?.Function);
            row["bodyScopeAssociationNotTraversal"] = Ref(body?.Scope);
            row["source"] = Source(Read(() => function.Source));
            row["specialFunctionKind"] = Read(() => function.SpecialFunctionKind?.Name);
            row["entryKind"] = Read(() => function.ConstructorOrDestructorKind?.Name);
            row["containingTypeAssociationOnly"] = Ref(Read(() => function.ContainingType));
            var signature = Read(() => function.Signature);
            row["signature"] = Signature(signature);
            row["executionParameters"] = Bounded(Read(() => function.Parameters), 16, "Execution parameters").Select(Parameter).ToArray();
            row["sourceParameters"] = Bounded(Read(() => function.SourceParameters), 16, "Source parameters").Select(Parameter).ToArray();
            if (exact) exactCount++;
            if (exact && defined && body is not null) definedCount++;
        }
        report["exactFinalNameCandidateCount"] = exactCount;
        report["definedExactFinalNameCandidateCount"] = definedCount;
        report["definitionCandidateLocatedWithinInspectedInput"] = definedCount > 0;
        report["definitionCandidateAmbiguousWithinInspectedInput"] = definedCount > 1;
        if (exactCount == 0) issues.Add(new { code = "exact-final-name-not-found", message = "No retained display-index candidate has the requested exact captured final name." });
        else if (definedCount == 0) issues.Add(new { code = "definition-not-found-in-input", message = "No exact-final-name candidate has a defined body association in this input." });
        if (definedCount > 1) issues.Add(new { code = "multiple-definition-candidates-in-input", message = "Multiple defined candidates remain; this tool does not choose a definition." });
        Check();
        Completed = true;
    }
    private Dictionary<string, object?> Parameter(SignatureParameter parameter)
    {
        var descriptor = Read(() => parameter.Descriptor);
        var execution = parameter as Parameter;
        var symbol = execution is null ? null : Read(() => execution.Symbol);
        return Row(("ordinal", Read(() => parameter.Ordinal)), ("role", Read(() => parameter.Role.Name)),
            ("descriptor", Ref(descriptor)), ("type", Ref(Read(() => parameter.Type))),
            ("declaredType", Ref(Read(() => parameter.DeclaredType))),
            ("passedViaCopyConstructor", Read(() => descriptor.Details.PassedViaCopyConstructor)),
            ("baseSubobjectAssociationOnly", Ref(Read(() => parameter.BaseSubobject))), ("symbol", Ref(symbol)),
            ("symbolTypeAssociationOnly", symbol is null ? null : Ref(Read(() => symbol.Type))),
            ("isSourceParameter", execution is null ? null : Read(() => (bool?)execution.IsSourceParameter)));
    }
    private Dictionary<string, object?> Signature(RoutineType signature)
    {
        var exceptionSpecification = Read(() => signature.ExceptionSpecification);
        return Row(("type", Ref(signature)), ("returnTypeAssociationOnly", Ref(Read(() => signature.ReturnType))),
            ("thisClassAssociationOnly", Ref(Read(() => signature.ThisClass))),
            ("hasPrototype", Read(() => signature.HasPrototype)), ("isVariadic", Read(() => signature.IsVariadic)),
            ("hasThisParameter", Read(() => signature.HasThisParameter)),
            ("returnsValueAsParameter", Read(() => signature.ReturnsValueAsParameter)),
            ("returnsValueViaCopyConstructor", Read(() => signature.ReturnsValueViaCopyConstructor)),
            ("returnValueParameterFollowsThis", Read(() => signature.ReturnValueParameterFollowsThis)),
            ("callingConvention", Read(() => signature.CallingConvention?.Name)), ("languageLinkage", Read(() => signature.LanguageLinkage?.Name)),
            ("noexceptEffective", exceptionSpecification is null ? null : Read(() => exceptionSpecification.NoexceptEffective)),
            ("parameters", Bounded(Read(() => signature.Parameters), 16, "Signature parameters").Select(Parameter).ToArray()));
    }
    private object? Source(SourceInfo? source)
    {
        if (source is null) return null;
        var position = Read(() => source.Position);
        if (position is null) return Row(("association", Ref(source)), ("position", null));
        var file = Read(() => position.File);
        return Row(("association", Ref(source)), ("position", Ref(position)),
            ("file", file is null ? null : Read(() => file.Path)), ("presumedName", Read(() => position.PresumedName)),
            ("logicalLine", Read(() => position.LogicalLine?.ToString(CultureInfo.InvariantCulture))),
            ("physicalLine", Read(() => position.PhysicalLine?.ToString(CultureInfo.InvariantCulture))),
            ("column", Read(() => position.Column)));
    }
}
