using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace RoslynObj.CSharp;

public sealed partial class Translator
{
    public TranslationResult Translate(TranslationRequest request, CancellationToken cancellationToken = default)
        => Translate(request, cancellationToken, null);

    /// <summary>Notifies the caller synchronously at real build-tool boundaries, before cancellation checks.</summary>
    public TranslationResult Translate(TranslationRequest request, CancellationToken cancellationToken,
        Action<TranslationPhase>? phaseObserver)
    {
        var watch = Stopwatch.StartNew();
        var inventory = Inventory(request, cancellationToken);
        if (!inventory.Success) return inventory;
        var diagnostics = inventory.Diagnostics.ToList();
        TranslationResult Failure(string code, string message)
        {
            diagnostics.Add(new(code, DiagnosticSeverity.Error, message));
            var report = inventory.ProgramReport with { Diagnostics = diagnostics.ToArray() };
            return inventory with { Status = TranslationStatus.Failed, Diagnostics = report.Diagnostics, ProgramReport = report };
        }
        if (inventory.ProgramReport.Inputs.Count > 2 || inventory.ProgramReport.Inputs.Count > request.Budgets.MaximumOpenUnits)
            return Failure("ROC1203", "Emission admits at most two units within the explicit simultaneous open-unit budget.");
        if (request.Roots.Exports.Length != 0)
            return Failure("ROC1205", "Explicit native export wrappers require an accepted export ABI implementation.");
        var nativeBindings = new List<CSharpExternalFunctionBinding>();
        var nativeDataBindings = new List<CSharpExternalDataBinding>();
        var semanticRuntimeCandidates = new List<ImportReport>();
        var selectedDsoCandidates = new List<ImportReport>();
        var intrinsicBuiltins = inventory.ProgramReport.Functions.Where(function => function.Builtin is not null)
            .ToDictionary(function => function.Locator, function => function.Builtin!, StringComparer.Ordinal);
        var intrinsicImports = intrinsicBuiltins.Keys.ToHashSet(StringComparer.Ordinal);
        // Multi-unit execution is initially self-contained. Only ProgramAssembly
        // may fulfill its cross-unit declarations; no name-based import filtering.
        foreach (var import in inventory.ProgramReport.Imports.Where(import => import.Required && (inventory.ProgramReport.Inputs.Count == 1 || !request.Roots.RetainAllDefinitions)))
        {
            if (import.BindingStatus == "captured-linkage-abi-selected-definition") continue;
            // Recorded compiler intrinsics are lowered from their actual call
            // occurrences. They never fall back to a similarly named host export.
            // The emitter still validates each selected signature/order/operand.
            if (import.Kind == "function" && intrinsicImports.Contains(import.Locator))
            {
                if (request.Runtime.Bindings.Any(binding => binding.Kind == import.Kind && binding.LinkageName == import.LinkageName))
                    return Failure("ROC1301", $"Recorded intrinsic '{import.LinkageName}' cannot be replaced by a native function binding.");
                continue;
            }
            // This spelling only defers admission. Successful fulfillment must
            // come from the emitter's actual validated execution/action identity.
            if (import.Kind == "function" && SemanticRuntimeFunctionImplementation.CandidateContract(import.LinkageName) is { } candidateContract &&
                (candidateContract != SemanticRuntimeFunctionImplementation.SelectedStartupRegistrationContract || request.Roots.ManagedStartup is not null))
            {
                if (request.Runtime.Bindings.Any(binding => binding.Kind == import.Kind && binding.LinkageName == import.LinkageName))
                    return Failure("ROC1301", "A selected generated semantic helper cannot be replaced by a native binding.");
                semanticRuntimeCandidates.Add(import);
                continue;
            }
            if (request.Roots.ManagedStartup is not null && import.Kind == "variable" && import.LinkageName == "__dso_handle")
            {
                if (request.Runtime.Bindings.Any(binding => binding.Kind == import.Kind && binding.LinkageName == import.LinkageName))
                    return Failure("ROC1301", "An explicitly selected generated module identity cannot be replaced by a native data binding.");
                selectedDsoCandidates.Add(import);
                continue;
            }
            var binding = request.Runtime.Bindings.SingleOrDefault(binding =>
                binding.Kind == import.Kind && binding.LinkageName == import.LinkageName);
            if (import.Kind == "variable" && binding is not null && import.BindingStatus == "declared-unverified" &&
                SemanticNativeDataImplementation.IsAdmitted(binding) && !string.IsNullOrWhiteSpace(binding.LibraryPath) &&
                binding.LibrarySha256 is { Length: 64 } && binding.LibrarySha256.All(Uri.IsHexDigit))
            {
                nativeDataBindings.Add(new(binding.LinkageName, binding.LibraryPath, binding.LibrarySha256, binding.Symbol, binding.DataAddressContract!));
                continue;
            }
            if (import.Kind != "function" || binding?.DataAddressContract is not null
                || binding is null || import.BindingStatus != "declared-unverified"
                || string.IsNullOrWhiteSpace(binding.Symbol) || string.IsNullOrWhiteSpace(binding.LibraryPath)
                || binding.LibrarySha256 is not { Length: 64 } || !binding.LibrarySha256.All(Uri.IsHexDigit))
                return Failure("ROC1301", $"Required {import.Kind} '{import.LinkageName}' needs a complete exact-signature pinned native function binding or the admitted address-only RTTI data contract.");
            nativeBindings.Add(new(binding.LinkageName, binding.LibraryPath, binding.Symbol));
        }
        if (inventory.ProgramReport.Families.Any(f => f.Family == "link-policy" && f.RequiredCount > 0))
            return Failure("ROC1204", "Required weak/common/alias/ifunc/alternate-entry selection has no accepted implementation.");
        if (string.IsNullOrWhiteSpace(request.Output.Directory) || System.IO.Directory.Exists(request.Output.Directory)
            || File.Exists(request.Output.Directory))
            return Failure("ROC1500", "Translation output must be a new directory so existing artifacts cannot be overwritten.");
        var openedInputs = new List<RoslynObject>();
        ManagedSelectionPlan? managedSelectionPlan = null;
        ManagedProgramSelectionPlan? managedProgramSelection = null;
        try
        {
            phaseObserver?.Invoke(TranslationPhase.InventoryCompleted);
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var binding in request.Runtime.Bindings.Where(binding =>
                binding.Kind == "function" && nativeBindings.Any(native => native.LinkageName == binding.LinkageName) ||
                binding.Kind == "variable" && nativeDataBindings.Any(native => native.LinkageName == binding.LinkageName)))
            {
                if (!Path.IsPathFullyQualified(binding.LibraryPath!) || !File.Exists(binding.LibraryPath)
                    || !HashFile(binding.LibraryPath!, cancellationToken).Equals(binding.LibrarySha256, StringComparison.OrdinalIgnoreCase))
                    return Failure("ROC1301", $"Native provider for '{binding.LinkageName}' differs from its pinned absolute file/hash contract.");
            }
            foreach (var input in inventory.ProgramReport.Inputs)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (HashFile(input.Path, cancellationToken) != input.Sha256)
                    return Failure("ROC1002", "Input bytes changed after admission.");
                openedInputs.Add(RoslynObject.Open(input.Path, new ReadOptions
                {
                    CachedRowCount = request.Budgets.ReaderCachedRows, CachedPageCount = request.Budgets.ReaderCachedPages,
                    PageSize = request.Budgets.ReaderPageSize
                }));
            }
            if (!request.Roots.RetainAllDefinitions)
            {
                using var selectionProcess = Process.GetCurrentProcess();
                void CheckSelectionBudget()
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    selectionProcess.Refresh();
                    if (Math.Max(selectionProcess.WorkingSet64, selectionProcess.PeakWorkingSet64) > request.Budgets.MaximumWorkingSetBytes
                        || watch.Elapsed.TotalSeconds > request.Budgets.MaximumElapsedSeconds)
                        throw new ResourceBudgetException("Managed selection exceeded the translation resource budget.");
                }
                if (openedInputs.Count == 1)
                    managedSelectionPlan = ManagedSelectionPlanner.Select(openedInputs[0], request.Roots.ManagedDefinitions,
                        request.Budgets, cancellationToken, CheckSelectionBudget, request.Roots.ManagedStartup);
                else
                    managedProgramSelection = ManagedProgramSelectionPlanner.Select(openedInputs, inventory.ProgramReport.Inputs.Select(input => input.Sha256).ToArray(),
                        request.Roots.ManagedDefinitions, request.Budgets, cancellationToken, CheckSelectionBudget, request.Roots.ManagedStartup);
            }
            var emission = new CSharpEmitter().Emit(openedInputs, new()
            {
                NamespaceName = request.Output.NamespaceName, ClassName = request.Output.ClassName,
                Executable = request.Roots.Mode == "executable", EntryPointName = request.Roots.EntryPoint,
                RuntimeProjectPath = request.Runtime.RuntimeProjectPath ?? "", ExternalFunctions = nativeBindings, ExternalData = nativeDataBindings,
                NativeErrorProtocol = request.Runtime.NativeErrorProtocol,
                ManagedSelection = managedSelectionPlan, ManagedProgramSelection = managedProgramSelection,
                ManagedStartup = request.Roots.ManagedStartup,
                ProgramAssembly = openedInputs.Count == 1 || managedProgramSelection is not null ? null : new ProgramAssemblyOptions
                {
                    MaximumOpenUnits = request.Budgets.MaximumOpenUnits,
                    DeduplicateIdenticalArtifacts = request.Link.DeduplicateIdenticalArtifacts,
                    InputSha256 = inventory.ProgramReport.Inputs.Select(input => input.Sha256).ToArray()
                }
            });
            diagnostics.AddRange(emission.Diagnostics);
            if (diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
            {
                var report = inventory.ProgramReport with { Diagnostics = diagnostics.ToArray() };
                return inventory with { Status = TranslationStatus.Failed, Diagnostics = report.Diagnostics, ProgramReport = report };
            }
            var semanticRuntimeImplementations = new Dictionary<string, SemanticRuntimeFunctionImplementation>(StringComparer.Ordinal);
            if (semanticRuntimeCandidates.Count != 0 || emission.SemanticRuntimeFunctions.Count != 0)
            {
                var publicFunctions = openedInputs.SelectMany((input, unitIndex) => (managedProgramSelection?.GetUnit(input)?.Functions ?? managedSelectionPlan?.Functions ?? input.TranslationUnit.Functions.AsEnumerable())
                    .Select((function, functionIndex) => (Function: function,
                        Locator: $"unit/{unitIndex + 1:D4}/function/{functionIndex + 1:D6}")))
                    .ToDictionary(item => item.Function, item => item.Locator);
                foreach (var implementation in emission.SemanticRuntimeFunctions)
                {
                    if (!publicFunctions.TryGetValue(implementation.Function, out var locator)
                        || !semanticRuntimeCandidates.Any(candidate => candidate.Locator == locator
                            && SemanticRuntimeFunctionImplementation.IsAdmittedContract(candidate.LinkageName, implementation.Contract))
                        || !semanticRuntimeImplementations.TryAdd(locator, implementation))
                        return Failure("ROC1301", "A semantic runtime selection does not match one required public input function and its admitted contract.");
                }
                if (semanticRuntimeCandidates.Any(candidate => !semanticRuntimeImplementations.ContainsKey(candidate.Locator)))
                    return Failure("ROC1301", "A deferred semantic helper import has no validated generated implementation.");
            }
            var nativeDataImplementations = new Dictionary<string, SemanticNativeDataImplementation>(StringComparer.Ordinal);
            if (nativeDataBindings.Count != 0 || emission.NativeDataImplementations.Count != 0)
            {
                var publicVariables = openedInputs.SelectMany((input, unitIndex) => (managedProgramSelection?.GetUnit(input)?.Variables ?? managedSelectionPlan?.Variables ?? input.TranslationUnit.Variables.AsEnumerable())
                    .Select((variable, variableIndex) => (Variable: variable,
                        Locator: $"unit/{unitIndex + 1:D4}/storage/{variableIndex + 1:D6}")))
                    .ToDictionary(item => item.Variable, item => item.Locator);
                foreach (var implementation in emission.NativeDataImplementations)
                {
                    if (!publicVariables.TryGetValue(implementation.Variable, out var locator) ||
                        !inventory.ProgramReport.Imports.Any(import => import.Required && import.Kind == "variable" &&
                            import.Locator == locator && import.LinkageName == implementation.Binding.LinkageName) ||
                        !nativeDataBindings.Contains(implementation.Binding) || !nativeDataImplementations.TryAdd(locator, implementation))
                        return Failure("ROC1301", "Native data selection does not match a required public declaration and its admitted address-only contract.");
                }
                if (nativeDataImplementations.Count != nativeDataBindings.Count)
                    return Failure("ROC1301", "A required native data binding has no validated address-only projection.");
            }
            var selectedDsoImplementations = new HashSet<string>(StringComparer.Ordinal);
            if (selectedDsoCandidates.Count != 0 || emission.SelectedStartupDsoVariables.Count != 0)
            {
                var publicVariables = openedInputs.SelectMany((input, unitIndex) =>
                    (managedProgramSelection?.GetUnit(input)?.Variables ?? managedSelectionPlan?.Variables ?? input.TranslationUnit.Variables.AsEnumerable())
                    .Select((variable, variableIndex) => (Variable: variable, Locator: $"unit/{unitIndex + 1:D4}/storage/{variableIndex + 1:D6}")))
                    .ToDictionary(item => item.Variable, item => item.Locator);
                foreach (var variable in emission.SelectedStartupDsoVariables)
                    if (!publicVariables.TryGetValue(variable, out var locator) ||
                        !selectedDsoCandidates.Any(candidate => candidate.Locator == locator) ||
                        !selectedDsoImplementations.Add(locator))
                        return Failure("ROC1301", "A generated module identity does not match one required current public declaration and validated address-only use closure.");
                if (selectedDsoImplementations.Count != selectedDsoCandidates.Count)
                    return Failure("ROC1301", "A deferred generated module declaration has no validated address-only implementation.");
            }
            phaseObserver?.Invoke(TranslationPhase.EmissionCompleted);
            cancellationToken.ThrowIfCancellationRequested();
            using var process = Process.GetCurrentProcess();
            process.Refresh();
            var peak = Math.Max(inventory.Metrics.PeakWorkingSetBytes, process.PeakWorkingSet64);
            if (peak > request.Budgets.MaximumWorkingSetBytes || watch.Elapsed.TotalSeconds > request.Budgets.MaximumElapsedSeconds)
                return Failure("ROC0003", "Translation resource budget exceeded; no successful project was published.");
            var completed = inventory.ProgramReport with
            {
                TranslationComplete = true, Diagnostics = diagnostics.ToArray(),
                Imports = inventory.ProgramReport.Imports.Select(import => import.Required && intrinsicImports.Contains(import.Locator)
                    ? intrinsicBuiltins[import.Locator] == "bfk_expect"
                        ? import with
                        {
                            Provider = "generated-expression", ProviderSymbol = null,
                            BindingStatus = "recorded-generated-intrinsic-constant-hint-expect-signed64-v1",
                            Ownership = "constant-hint-expect-signed64-v1: Project the first captured signed64 operand value once; an effect-free integer literal hint owns no storage or runtime value.",
                            ErrorContract = "The projection introduces no host error channel; first-operand effects and exceptions remain those of its captured evaluation. Nonconstant/effectful hints and unsupported signatures are rejected.",
                            ThreadContract = "Synchronous expression evaluation preserves the retained operand constraints; no runtime or thread-local helper is supplied.",
                            CallbackContract = "No callable address, native import, callback entry or Runtime method is supplied."
                        }
                        : import with { Provider = "RoslynObj.CSharp.Runtime.TargetIntrinsics", BindingStatus = "recorded-intrinsic-lowering-selected" }
                    : semanticRuntimeImplementations.TryGetValue(import.Locator, out var implementation)
                        ? implementation.Describe(import)
                        : nativeDataImplementations.TryGetValue(import.Locator, out var dataImplementation)
                            ? dataImplementation.Describe(import)
                            : selectedDsoImplementations.Contains(import.Locator)
                                ? SelectedModuleDataContract.Describe(import)
                            : import).ToArray(),
                Families = inventory.ProgramReport.Families.Select(f => f.RequiredCount == 0 ? f : f with
                {
                    Status = CapabilityStatus.Implemented,
                    Note = "The selected program was completely emitted. Build and execution acceptance are recorded separately."
                }).ToArray()
            };
            var sources = emission.Files.Append(new GeneratedSource("program-report.json", JsonSerializer.Serialize(completed, ProgramManifest.JsonOptions) + "\n")).ToArray();
            var bytes = sources.Sum(source => (long)Encoding.UTF8.GetByteCount(source.Content));
            if (bytes > request.Budgets.MaximumOutputBytes) return Failure("ROC0003", "Generated source/report output budget exceeded.");
            var generated = sources.Select(source =>
            {
                var content = Encoding.UTF8.GetBytes(source.Content);
                return new GeneratedFile(source.RelativePath, Convert.ToHexStringLower(SHA256.HashData(content)), content.LongLength);
            }).ToArray();
            var output = System.IO.Path.GetFullPath(request.Output.Directory);
            var parent = System.IO.Path.GetDirectoryName(output)!;
            System.IO.Directory.CreateDirectory(parent);
            var staging = output + ".incomplete-" + Guid.NewGuid().ToString("N");
            System.IO.Directory.CreateDirectory(staging);
            try
            {
                foreach (var source in sources)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var path = System.IO.Path.GetFullPath(source.RelativePath, staging);
                    if (!path.StartsWith(staging + System.IO.Path.DirectorySeparatorChar, StringComparison.Ordinal))
                        return Failure("ROC1501", "An emitted file escaped the selected output directory.");
                    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
                    File.WriteAllText(path, source.Content, new UTF8Encoding(false));
                }
                var completion = JsonSerializer.Serialize(new { manifestVersion = 1, translatorVersion = Version,
                    runtimeVersion = request.Runtime.Version, complete = true, inputs = completed.Inputs.Select(i => new { i.Ordinal, i.Sha256, i.ArtifactIdentity }), files = generated }, ProgramManifest.JsonOptions) + "\n";
                if (bytes + Encoding.UTF8.GetByteCount(completion) > request.Budgets.MaximumOutputBytes)
                    return Failure("ROC0003", "Completion manifest exceeds the output budget.");
                File.WriteAllText(System.IO.Path.Combine(staging, "completion.json"), completion, new UTF8Encoding(false));
                phaseObserver?.Invoke(TranslationPhase.BeforePublication);
                cancellationToken.ThrowIfCancellationRequested();
                System.IO.Directory.Move(staging, output);
            }
            finally
            {
                // Failed attempts remain visibly incomplete for diagnosis/restart; no completion file survives failure.
                if (System.IO.Directory.Exists(staging))
                {
                    var completion = System.IO.Path.Combine(staging, "completion.json");
                    if (File.Exists(completion)) File.Delete(completion);
                }
            }
            return new(TranslationStatus.TranslationComplete, completed.Diagnostics, generated, completed,
                new(watch.Elapsed.TotalSeconds, peak, inventory.Metrics.ReaderCommands + openedInputs.Sum(obj => obj.Statistics.Commands),
                    inventory.Metrics.ReaderRowsRead + openedInputs.Sum(obj => obj.Statistics.RowsRead)));
        }
        catch (OperationCanceledException)
        {
            var result = Failure("ROC0002", "Translation was cancelled; no complete project was published.");
            return result with { Status = TranslationStatus.Cancelled };
        }
        catch (ManagedSelectionException exception) { return Failure(exception.Code, exception.Message); }
        catch (ResourceBudgetException exception) { return Failure("ROC0003", exception.Message); }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException)
        { return Failure("ROC1502", exception.Message); }
        finally
        {
            foreach (var obj in openedInputs) obj.Dispose();
        }
    }
}
