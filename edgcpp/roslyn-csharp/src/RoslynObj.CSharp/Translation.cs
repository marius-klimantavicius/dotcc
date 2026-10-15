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
        var semanticRuntimeCandidates = new List<ImportReport>();
        var intrinsicImports = inventory.ProgramReport.Functions.Where(function => function.Builtin is not null)
            .Select(function => function.Locator).ToHashSet(StringComparer.Ordinal);
        // Multi-unit execution is initially self-contained. Only ProgramAssembly
        // may fulfill its cross-unit declarations; no name-based import filtering.
        foreach (var import in inventory.ProgramReport.Imports.Where(import => import.Required && inventory.ProgramReport.Inputs.Count == 1))
        {
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
            // come from the emitter's actual validated cleanup-action identity.
            if (import.Kind == "function" && import.LinkageName == "__destroy_exception_object")
            {
                if (request.Runtime.Bindings.Any(binding => binding.Kind == import.Kind && binding.LinkageName == import.LinkageName))
                    return Failure("ROC1301", "The generated exception-owner helper cannot be replaced by a native binding.");
                semanticRuntimeCandidates.Add(import);
                continue;
            }
            var binding = request.Runtime.Bindings.SingleOrDefault(binding =>
                binding.Kind == import.Kind && binding.LinkageName == import.LinkageName);
            if (import.Kind != "function"
                || binding is null || import.BindingStatus != "declared-unverified"
                || string.IsNullOrWhiteSpace(binding.Symbol) || string.IsNullOrWhiteSpace(binding.LibraryPath)
                || binding.LibrarySha256 is not { Length: 64 } || !binding.LibrarySha256.All(Uri.IsHexDigit))
                return Failure("ROC1301", $"Required {import.Kind} '{import.LinkageName}' needs a complete exact-signature native function binding and pinned provider file; native variable providers are not implemented here.");
            nativeBindings.Add(new(binding.LinkageName, binding.LibraryPath, binding.Symbol));
        }
        if (inventory.ProgramReport.Families.Any(f => f.Family == "link-policy" && f.RequiredCount > 0))
            return Failure("ROC1204", "Required weak/common/alias/ifunc/alternate-entry selection has no accepted implementation.");
        if (string.IsNullOrWhiteSpace(request.Output.Directory) || System.IO.Directory.Exists(request.Output.Directory)
            || File.Exists(request.Output.Directory))
            return Failure("ROC1500", "Translation output must be a new directory so existing artifacts cannot be overwritten.");
        var openedInputs = new List<RoslynObject>();
        try
        {
            phaseObserver?.Invoke(TranslationPhase.InventoryCompleted);
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var binding in request.Runtime.Bindings.Where(binding => binding.Kind == "function" && nativeBindings.Any(native =>
                native.LinkageName == binding.LinkageName)))
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
            var emission = new CSharpEmitter().Emit(openedInputs, new()
            {
                NamespaceName = request.Output.NamespaceName, ClassName = request.Output.ClassName,
                Executable = request.Roots.Mode == "executable", EntryPointName = request.Roots.EntryPoint,
                RuntimeProjectPath = request.Runtime.RuntimeProjectPath ?? "", ExternalFunctions = nativeBindings,
                NativeErrorProtocol = request.Runtime.NativeErrorProtocol,
                ProgramAssembly = openedInputs.Count == 1 ? null : new ProgramAssemblyOptions
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
                var publicFunctions = openedInputs.SelectMany((input, unitIndex) => input.TranslationUnit.Functions
                    .Select((function, functionIndex) => (Function: function,
                        Locator: $"unit/{unitIndex + 1:D4}/function/{functionIndex + 1:D6}")))
                    .ToDictionary(item => item.Function, item => item.Locator);
                foreach (var implementation in emission.SemanticRuntimeFunctions)
                {
                    if (implementation.Contract != SemanticRuntimeFunctionImplementation.Int32ExceptionOwnerContract
                        || !publicFunctions.TryGetValue(implementation.Function, out var locator)
                        || !semanticRuntimeCandidates.Any(candidate => candidate.Locator == locator)
                        || !semanticRuntimeImplementations.TryAdd(locator, implementation))
                        return Failure("ROC1301", "A semantic runtime selection does not match one required public input function and its admitted contract.");
                }
                if (semanticRuntimeCandidates.Any(candidate => !semanticRuntimeImplementations.ContainsKey(candidate.Locator)))
                    return Failure("ROC1301", "A deferred exception-owner import has no validated generated cleanup implementation.");
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
                    ? import with { Provider = "RoslynObj.CSharp.Runtime.TargetIntrinsics", BindingStatus = "recorded-intrinsic-lowering-selected" }
                    : semanticRuntimeImplementations.ContainsKey(import.Locator)
                        ? import with
                        {
                            Provider = "RoslynObj.CSharp.Runtime.ManagedCppException",
                            BindingStatus = "generated-exception-owner-contract-selected",
                            Ownership = "The validated managed-cpp-int32-exception-owner-v1 contract releases the selected handler's explicit payload owner once; no ambient current pointer or native application implementation is used.",
                            ErrorContract = "Only the admitted generated exception carrier participates; native C++ unwinding and CLR-error conversion are unsupported.",
                            ThreadContract = "The payload belongs to its carrier and selected handler; no ambient thread-local exception pointer is substituted.",
                            CallbackContract = "No native entry or callback is supplied for this generated cleanup action."
                        }
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
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException)
        { return Failure("ROC1502", exception.Message); }
        finally
        {
            foreach (var obj in openedInputs) obj.Dispose();
        }
    }
}
