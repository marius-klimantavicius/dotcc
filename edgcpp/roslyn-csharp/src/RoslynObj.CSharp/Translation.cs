using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace RoslynObj.CSharp;

public sealed partial class Translator
{
    public TranslationResult Translate(TranslationRequest request, CancellationToken cancellationToken = default)
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
        if (inventory.ProgramReport.Inputs.Count != 1)
            return Failure("ROC1203", "Multi-unit representation and binding are inventoried but have no accepted emission implementation.");
        if (request.Roots.Exports.Length != 0)
            return Failure("ROC1205", "Explicit native export wrappers require an accepted export ABI implementation.");
        var nativeBindings = new List<CSharpExternalFunctionBinding>();
        foreach (var import in inventory.ProgramReport.Imports.Where(import => import.Required))
        {
            var binding = request.Runtime.Bindings.SingleOrDefault(binding =>
                binding.Kind == import.Kind && binding.LinkageName == import.LinkageName);
            if (import.Kind != "function" || inventory.ProgramReport.Functions.Any(function => function.Locator == import.Locator && function.Builtin is not null)
                || binding is null || import.BindingStatus != "declared-unverified"
                || string.IsNullOrWhiteSpace(binding.Symbol) || string.IsNullOrWhiteSpace(binding.LibraryPath)
                || binding.LibrarySha256 is not { Length: 64 } || !binding.LibrarySha256.All(Uri.IsHexDigit))
                return Failure("ROC1301", $"Required {import.Kind} '{import.LinkageName}' needs a complete exact-signature native function binding and pinned provider file; variables and intrinsic providers are not implemented here.");
            nativeBindings.Add(new(binding.LinkageName, binding.LibraryPath, binding.Symbol));
        }
        if (inventory.ProgramReport.Families.Any(f => f.Family == "link-policy" && f.RequiredCount > 0))
            return Failure("ROC1204", "Required weak/common/alias/ifunc/alternate-entry selection has no accepted implementation.");
        if (string.IsNullOrWhiteSpace(request.Output.Directory) || System.IO.Directory.Exists(request.Output.Directory)
            || File.Exists(request.Output.Directory))
            return Failure("ROC1500", "Translation output must be a new directory so existing artifacts cannot be overwritten.");
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var binding in request.Runtime.Bindings.Where(binding => binding.Kind == "function" && nativeBindings.Any(native =>
                native.LinkageName == binding.LinkageName)))
            {
                if (!Path.IsPathFullyQualified(binding.LibraryPath!) || !File.Exists(binding.LibraryPath)
                    || !HashFile(binding.LibraryPath!, cancellationToken).Equals(binding.LibrarySha256, StringComparison.OrdinalIgnoreCase))
                    return Failure("ROC1301", $"Native provider for '{binding.LinkageName}' differs from its pinned absolute file/hash contract.");
            }
            var input = inventory.ProgramReport.Inputs[0];
            if (HashFile(input.Path, cancellationToken) != input.Sha256)
                return Failure("ROC1002", "Input bytes changed after admission.");
            using var obj = RoslynObject.Open(input.Path, new ReadOptions
            {
                CachedRowCount = request.Budgets.ReaderCachedRows, CachedPageCount = request.Budgets.ReaderCachedPages,
                PageSize = request.Budgets.ReaderPageSize
            });
            var emission = new CSharpEmitter().Emit([obj], new()
            {
                NamespaceName = request.Output.NamespaceName, ClassName = request.Output.ClassName,
                Executable = request.Roots.Mode == "executable", EntryPointName = request.Roots.EntryPoint,
                RuntimeProjectPath = request.Runtime.RuntimeProjectPath ?? "", ExternalFunctions = nativeBindings
            });
            diagnostics.AddRange(emission.Diagnostics);
            if (diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
            {
                var report = inventory.ProgramReport with { Diagnostics = diagnostics.ToArray() };
                return inventory with { Status = TranslationStatus.Failed, Diagnostics = report.Diagnostics, ProgramReport = report };
            }
            cancellationToken.ThrowIfCancellationRequested();
            using var process = Process.GetCurrentProcess();
            process.Refresh();
            var peak = Math.Max(inventory.Metrics.PeakWorkingSetBytes, process.PeakWorkingSet64);
            if (peak > request.Budgets.MaximumWorkingSetBytes || watch.Elapsed.TotalSeconds > request.Budgets.MaximumElapsedSeconds)
                return Failure("ROC0003", "Translation resource budget exceeded; no successful project was published.");
            var completed = inventory.ProgramReport with
            {
                TranslationComplete = true, Diagnostics = diagnostics.ToArray(),
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
                new(watch.Elapsed.TotalSeconds, peak, inventory.Metrics.ReaderCommands + obj.Statistics.Commands,
                    inventory.Metrics.ReaderRowsRead + obj.Statistics.RowsRead));
        }
        catch (OperationCanceledException)
        {
            var result = Failure("ROC0002", "Translation was cancelled; no complete project was published.");
            return result with { Status = TranslationStatus.Cancelled };
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException)
        { return Failure("ROC1502", exception.Message); }
    }
}
