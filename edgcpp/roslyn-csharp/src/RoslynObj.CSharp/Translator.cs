using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;

namespace RoslynObj.CSharp;

public sealed partial class Translator
{
    public const string Version = "0.1.0";

    public TranslationResult Inventory(TranslationRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var watch = Stopwatch.StartNew();
        var process = Process.GetCurrentProcess();
        var state = new InventoryState(request, watch, process, cancellationToken);
        state.ValidateRequest();
        var completed = false;
        var cancelled = false;
        try
        {
            if (!state.HasErrors)
            {
                var identities = new HashSet<string>(StringComparer.Ordinal);
                for (var index = 0; index < request.Inputs.Length; index++)
                {
                    state.CheckBudget();
                    var input = request.Inputs[index];
                    var hash = HashFile(input.Path, cancellationToken);
                    if (!hash.Equals(input.Sha256, StringComparison.OrdinalIgnoreCase))
                    {
                        state.Error("ROC1002", "Input SHA256 does not match the manifest.", input.Path);
                        continue;
                    }
                    using var obj = RoslynObject.Open(input.Path, new ReadOptions
                    {
                        CachedRowCount = request.Budgets.ReaderCachedRows,
                        CachedPageCount = request.Budgets.ReaderCachedPages,
                        PageSize = request.Budgets.ReaderPageSize
                    });
                    if (!state.Admit(obj, input)) continue;
                    if (!identities.Add(obj.Manifest.ArtifactIdentity))
                    {
                        if (request.Link.DeduplicateIdenticalArtifacts)
                            state.Diagnostics.Add(new("ROC1003", DiagnosticSeverity.Info,
                                "Repeated artifact identity was explicitly deduplicated.", input.Path));
                        else state.Error("ROC1003", "Repeated artifact identity is not admitted by the link policy.", input.Path);
                        continue;
                    }
                    state.InventoryUnit(obj, input, index + 1, hash);
                    state.ReaderCommands += obj.Statistics.Commands;
                    state.ReaderRows += obj.Statistics.RowsRead;
                }
                completed = !state.HasErrors;
            }
        }
        catch (OperationCanceledException)
        {
            cancelled = true;
            state.Error("ROC0002", "Inventory was cancelled and is incomplete.");
        }
        catch (ResourceBudgetException exception) { state.Error("ROC0003", exception.Message); }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException)
        { state.Error("ROC1001", exception.Message); }
        state.CheckPeak();
        var report = state.Report(completed);
        if (completed && JsonSerializer.SerializeToUtf8Bytes(report, ProgramManifest.JsonOptions).LongLength > request.Budgets.MaximumOutputBytes)
        {
            state.Error("ROC0003", "Inventory report exceeds the output budget.");
            completed = false;
            report = state.Report(false);
        }
        state.CheckPeak();
        if (completed && (state.PeakWorkingSetBytes > request.Budgets.MaximumWorkingSetBytes
            || watch.Elapsed.TotalSeconds > request.Budgets.MaximumElapsedSeconds))
        {
            state.Error("ROC0003", "Inventory resource budget exceeded while materializing the report.");
            completed = false;
            report = state.Report(false);
        }
        return new(cancelled ? TranslationStatus.Cancelled : completed ? TranslationStatus.InventoryComplete : TranslationStatus.Failed,
            report.Diagnostics, [], report,
            new(watch.Elapsed.TotalSeconds, state.PeakWorkingSetBytes, state.ReaderCommands, state.ReaderRows));
    }

    public static string HashFile(string path, CancellationToken cancellationToken = default)
    {
        using var stream = File.OpenRead(path);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var buffer = new byte[128 * 1024];
        int count;
        while ((count = stream.Read(buffer)) != 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            hash.AppendData(buffer.AsSpan(0, count));
        }
        return Convert.ToHexStringLower(hash.GetHashAndReset());
    }
}

internal sealed class ResourceBudgetException(string message) : Exception(message);

internal sealed partial class InventoryState(TranslationRequest request, Stopwatch watch, Process process, CancellationToken cancellationToken)
{
    internal List<TranslationDiagnostic> Diagnostics { get; } = [];
    internal bool HasErrors => Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
    internal long PeakWorkingSetBytes { get; private set; }
    internal long ReaderCommands { get; set; }
    internal long ReaderRows { get; set; }
    private readonly List<InputReport> inputs = [];
    private readonly List<FunctionReport> functions = [];
    private readonly List<StorageReport> storage = [];
    private readonly List<TypeReport> types = [];
    private readonly List<ImportReport> imports = [];
    private readonly List<InitializationRootReport> roots = [];
    private readonly List<InitializationRecipeReport> recipes = [];
    private readonly SortedDictionary<string, FamilyCounter> families = new(StringComparer.Ordinal);

    internal void Error(string code, string message, string? input = null, string? locator = null, string? family = null)
        => Diagnostics.Add(new(code, DiagnosticSeverity.Error, message, input, locator, family));

    internal void CheckPeak()
    {
        process.Refresh();
        PeakWorkingSetBytes = Math.Max(PeakWorkingSetBytes, Math.Max(process.WorkingSet64, process.PeakWorkingSet64));
    }

    internal void CheckBudget()
    {
        cancellationToken.ThrowIfCancellationRequested();
        CheckPeak();
        if (PeakWorkingSetBytes > request.Budgets.MaximumWorkingSetBytes)
            throw new ResourceBudgetException($"Working-set budget exceeded ({PeakWorkingSetBytes.ToString(CultureInfo.InvariantCulture)} bytes).");
        if (watch.Elapsed.TotalSeconds > request.Budgets.MaximumElapsedSeconds)
            throw new ResourceBudgetException("Elapsed-time budget exceeded.");
    }

    internal void ValidateRequest()
    {
        if (request.ManifestVersion != 1 || request.TranslatorVersion != Translator.Version || request.Runtime.Version != Translator.Version)
            Error("ROC0001", "Manifest, translator or runtime version is unsupported.");
        if (request.Inputs.Length == 0) Error("ROC1000", "At least one immutable object input is required.");
        foreach (var input in request.Inputs)
        {
            if (string.IsNullOrWhiteSpace(input.Path) || input.Sha256.Length != 64 || !input.Sha256.All(Uri.IsHexDigit)
                || string.IsNullOrWhiteSpace(input.Producer) || string.IsNullOrWhiteSpace(input.EdgRevision) || string.IsNullOrWhiteSpace(input.EdgVersion))
                Error("ROC1000", "Each input requires a path, SHA256 and pinned producer, EDG revision and EDG version.", input.Path);
            if (input.CaptureProfile != "native_lowered") Error("ROC1102", "Only explicitly pinned native_lowered capture is admitted.", input.Path);
        }
        var target = request.Target;
        if (target.Abi != "itanium" || target.AddressableUnitBits != 8 || target.Endianness != "little"
            || target.PointerBits != 64 || target.PointerAlignment != 8 || target.OperatingSystem != "linux"
            || target.Architecture != "x64" || target.Libc != "glibc" || target.OutputRid != "linux-x64")
            Error("ROC1100", "Supported source/host profile is explicitly pinned Linux x64 glibc, Itanium ABI, 8-bit units, little endian, 64-bit pointers aligned to 8 target bytes, output linux-x64.");
        if (request.Roots.Mode is not ("library" or "executable") || !request.Roots.RetainAllDefinitions
            || request.Roots.Mode == "executable" && string.IsNullOrWhiteSpace(request.Roots.EntryPoint))
            Error("ROC1200", "Only retain-all library mode or an explicit retain-all executable entry point is supported.");
        if (request.Link.DefinitionSelection != "strong-only" || request.Link.InitializationOrder != "manifest-order" || request.Link.GlobalState != "process")
            Error("ROC1201", "Requested link, initialization or global-state policy is unsupported.");
        if (request.Output.NamePolicy != "source-stable" || request.Output.FilePolicy != "declaration-size")
            Error("ROC1202", "Requested source-name or file policy is unsupported.");
        var budgets = request.Budgets;
        if (budgets.MaximumOpenUnits is < 1 or > 2 || budgets.MaximumWorkingSetBytes <= 0 || budgets.MaximumOperationsPerBody <= 0
            || budgets.MaximumOutputBytes <= 0 || budgets.MaximumElapsedSeconds <= 0 || budgets.ReaderCachedRows < 1
            || budgets.ReaderCachedPages < 1 || budgets.ReaderPageSize is < 1 or > 4096)
            Error("ROC0003", "Resource budgets must be positive; inventory opens one unit at a time and selected program emission admits at most two simultaneous units.");
        foreach (var binding in request.Runtime.Bindings)
        {
            if (string.IsNullOrWhiteSpace(binding.LinkageName) || string.IsNullOrWhiteSpace(binding.Provider)
                || !request.Runtime.Providers.Contains(binding.Provider, StringComparer.Ordinal))
                Error("ROC1300", "Every external binding must name a linkage symbol and a declared runtime provider.");
        }
        foreach (var duplicate in request.Runtime.Bindings.GroupBy(b => (b.Kind, b.LinkageName)).Where(g => g.Count() != 1))
            Error("ROC1300", $"Repeated external binding for {duplicate.Key.Kind} '{duplicate.Key.LinkageName}'.");
    }

    internal bool Admit(RoslynObject obj, InputArtifact input)
    {
        var before = Diagnostics.Count;
        var actual = obj.Manifest;
        var expected = request.Target;
        if (input.ArtifactIdentity is { } identity && identity != actual.ArtifactIdentity)
            Error("ROC1004", "Input artifact identity differs from its pinned manifest identity.", input.Path);
        if (actual.Producer != input.Producer || actual.EdgRevision != input.EdgRevision || actual.EdgVersion != input.EdgVersion)
            Error("ROC1101", "Captured producer provenance differs from the explicit input contract.", input.Path);
        if (actual.CaptureProfile != input.CaptureProfile || actual.Stage != "lowered")
            Error("ROC1102", "Captured profile/stage differs from the supported lowered contract.", input.Path);
        if (actual.Target.Abi != expected.Abi || actual.Target.AddressableUnitBits != expected.AddressableUnitBits
            || actual.Target.Endianness != expected.Endianness || actual.Target.PointerBits != expected.PointerBits
            || actual.Target.PointerAlignment != expected.PointerAlignment)
            Error("ROC1100", "Captured target facts differ from the explicit source ABI contract.", input.Path);
        foreach (var name in input.RequiredCapabilities)
            if (!obj.TranslationUnit.Capabilities.Any(c => c.Name == name && c.Enabled != false && c.State.Name == "available"))
                Error("ROC1103", $"Required captured capability '{name}' is absent or disabled.", input.Path);
        return Diagnostics.Count == before;
    }

    internal void Count(string family, string form, bool required, string locator, bool retained = true)
    {
        var key = family + ":" + form;
        if (!families.TryGetValue(key, out var count)) families.Add(key, count = new(family, form));
        if (retained) count.Retained++;
        if (required) count.Required++;
        if (count.Examples.Count < 3 && !count.Examples.Contains(locator, StringComparer.Ordinal)) count.Examples.Add(locator);
    }

    internal ProgramReport Report(bool complete)
    {
        foreach (var family in new[] { "expression", "statement", "operator", "call", "constant-value", "address-constant",
            "storage", "storage-duration", "initialization-root", "initialization-cleanup-dependency", "lifetime", "exception",
            "cleanup-action", "builtin", "builtin-operation", "extension", "type", "layout", "signature", "import", "link-policy" })
            if (!families.Values.Any(row => row.Family == family)) Count(family, "not-encountered", false, "program", false);
        return new()
    {
        TranslatorVersion = Translator.Version, InventoryComplete = complete, Target = request.Target,
        Runtime = request.Runtime, Roots = request.Roots, Link = request.Link, Budgets = request.Budgets, Output = request.Output,
        Inputs = inputs.ToArray(), Functions = functions.ToArray(), Storage = storage.ToArray(), Types = types.ToArray(),
        Imports = imports.ToArray(), InitializationRoots = roots.ToArray(), InitializationRecipes = recipes.ToArray(),
        Families = families.Values.Select(f => new FamilyReport(f.Family, f.Form, f.Required, f.Retained,
            f.Required == 0 ? CapabilityStatus.NotEncountered : CapabilityStatus.Unsupported, f.Examples.ToArray(),
            f.Required == 0 ? "Retained metadata/declarations only; no required execution occurrence." : "Inventoried requirement; emission/runtime acceptance is not established by inventory.")).ToArray(),
        Diagnostics = Diagnostics.OrderBy(d => d.Input, StringComparer.Ordinal).ThenBy(d => d.Locator, StringComparer.Ordinal)
            .ThenBy(d => d.Code, StringComparer.Ordinal).ThenBy(d => d.Message, StringComparer.Ordinal).ToArray()
        };
    }

    private sealed class FamilyCounter(string family, string form)
    {
        internal string Family { get; } = family;
        internal string Form { get; } = form;
        internal long Required;
        internal long Retained;
        internal List<string> Examples { get; } = [];
    }
}
