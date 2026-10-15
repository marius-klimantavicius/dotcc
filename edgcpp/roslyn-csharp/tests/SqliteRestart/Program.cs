using System.Security.Cryptography;
using System.Text.Json;
using RoslynObj.CSharp;

if (args.Length is not (3 or 4) || args[0] is not ("cancel" or "complete"))
    throw new ArgumentException("Checks cancel|complete REQUEST NEW_RESULT [InventoryCompleted|EmissionCompleted|BeforePublication]");
var cancel = args[0] == "cancel";
var phase = args.Length == 4 ? Enum.Parse<TranslationPhase>(args[3], false) : TranslationPhase.BeforePublication;
if (!Enum.IsDefined(phase)) throw new ArgumentException("Unknown real translation phase.");
var request = ProgramManifest.Load(args[1]);
var output = Path.GetFullPath(request.Output.Directory);
var parent = Path.GetDirectoryName(output)!;
Directory.CreateDirectory(parent);
var resultPath = Path.GetFullPath(args[2]);
var programPath = resultPath + ".program-report.json";
if (File.Exists(resultPath) || File.Exists(programPath) || Directory.Exists(output) || File.Exists(output))
    throw new ArgumentException("Result/report/output paths must be unused.");
var before = Directory.GetDirectories(parent).ToHashSet(StringComparer.Ordinal);
var trace = new List<string>();
string? staged = null;
var stagedCompletionSeen = false;
var status = "incomplete";
string? error = null;
TranslationResult? result = null;
using var cancellation = new CancellationTokenSource();
try
{
    result = new Translator().Translate(request, cancellation.Token, observed =>
    {
        trace.Add(observed.ToString());
        if (observed == TranslationPhase.BeforePublication)
        {
            // Observe actual newly created staging, without predicting its
            // GUID/name or polling the filesystem to infer a phase transition.
            var created = Directory.GetDirectories(parent).Where(path => !before.Contains(path)).ToArray();
            if (created.Length != 1 || Directory.Exists(output))
                throw new InvalidOperationException("BeforePublication did not expose one unpublished staging directory.");
            staged = created[0];
            var completion = Path.Combine(staged, "completion.json");
            using var document = JsonDocument.Parse(File.ReadAllText(completion));
            stagedCompletionSeen = document.RootElement.GetProperty("complete").GetBoolean();
            if (!stagedCompletionSeen) throw new InvalidOperationException("The actual staged completion was not complete.");
        }
        if (cancel && observed == phase) cancellation.Cancel();
    });
    if (cancel)
    {
        if (!trace.Contains(phase.ToString(), StringComparer.Ordinal) || !cancellation.IsCancellationRequested ||
            result.Status != TranslationStatus.Cancelled || result.Success || result.ProgramReport.TranslationComplete ||
            result.GeneratedFiles.Count != 0 || !result.Diagnostics.Any(item => item.Code == "ROC0002") || Directory.Exists(output))
            throw new InvalidOperationException("Real phase cancellation did not leave an unpublished Cancelled result.");
        if (phase == TranslationPhase.BeforePublication)
        {
            if (!stagedCompletionSeen || staged is null || !Directory.Exists(staged) || File.Exists(Path.Combine(staged, "completion.json")))
                throw new InvalidOperationException("Cancellation did not preserve incomplete staging while removing completion.");
        }
        else if (Directory.GetDirectories(parent).Any(path => !before.Contains(path)))
            throw new InvalidOperationException("An earlier phase cancellation unexpectedly created staging.");
        status = "passed-real-phase-cancellation";
    }
    else
    {
        if (result.Status != TranslationStatus.TranslationComplete || !result.Success || !result.ProgramReport.TranslationComplete ||
            !File.Exists(Path.Combine(output, "completion.json")) || !stagedCompletionSeen || staged is null || Directory.Exists(staged))
            throw new InvalidOperationException("Fresh-process restart did not publish the completed output atomically.");
        status = "passed-fresh-process-restart-from-input";
    }
}
catch (Exception exception) { error = exception.ToString(); }
if (result is not null) ProgramManifest.WriteReport(programPath, result.ProgramReport);
string[] stagePaths = staged is not null && Directory.Exists(staged)
    ? Directory.GetFiles(staged, "*", SearchOption.AllDirectories) : [];
var stageFiles = stagePaths.Order(StringComparer.Ordinal)
    .Select(path => new { relativePath = Path.GetRelativePath(staged!, path), sha256 = Hash(path), sizeBytes = new FileInfo(path).Length }).ToArray();
var summary = new
{
    status, mode = args[0], requestedCancellationPhase = cancel ? phase.ToString() : null, observedPhases = trace,
    manifest = new { path = Path.GetFullPath(args[1]), sha256 = Hash(args[1]) }, output,
    inputHashes = request.Inputs.Select(input => new { input.Path, input.Sha256, actualSha256 = Hash(input.Path) }),
    resultStatus = result?.Status, resultSuccess = result?.Success, translationComplete = result?.ProgramReport.TranslationComplete,
    inventoryComplete = result?.ProgramReport.InventoryComplete, generatedFiles = result?.GeneratedFiles,
    diagnostics = result?.Diagnostics, metrics = result?.Metrics, programReportPath = result is null ? null : programPath,
    stagedDirectory = staged, stagedCompletionSeen, stagedCompletionSurvives = staged is not null && File.Exists(Path.Combine(staged, "completion.json")),
    stagingFiles = stageFiles, outputPublished = Directory.Exists(output), error,
    scope = "Real synchronous phase cancellation and fresh-process recomputation from immutable input; no partial-stage reuse or incremental-resume claim."
};
Directory.CreateDirectory(Path.GetDirectoryName(resultPath)!);
using (var stream = new StreamWriter(new FileStream(resultPath, FileMode.CreateNew, FileAccess.Write)))
    stream.Write(JsonSerializer.Serialize(summary, ProgramManifest.JsonOptions) + "\n");
Console.WriteLine(status + ": " + resultPath);
return status.StartsWith("passed-", StringComparison.Ordinal) ? 0 : 1;

static string Hash(string path)
{
    using var stream = File.OpenRead(path);
    return Convert.ToHexStringLower(SHA256.HashData(stream));
}
