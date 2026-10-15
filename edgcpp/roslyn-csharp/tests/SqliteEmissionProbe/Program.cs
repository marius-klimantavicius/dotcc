using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using RoslynObj;
using RoslynObj.CSharp;

if (args.Length != 4)
    throw new ArgumentException("SqliteEmissionProbe PINNED_OBJECT PROVIDER_PLAN RUNTIME_PROJECT NEW_REPORT");
var inputPath = Path.GetFullPath(args[0]);
var providerPlanPath = Path.GetFullPath(args[1]);
var reportPath = Path.GetFullPath(args[3]);
if (File.Exists(reportPath)) throw new ArgumentException("Report path must be new.");
const string expectedObjectSha = "e26d8937c94f5ccc9e3fca7915f53c4a31a1bb63432419a847d99394ab06bb1c";
var inputSha = Hash(inputPath);
if (inputSha != expectedObjectSha) throw new ArgumentException("Pinned full-feature SQLite object identity mismatch.");
using var plan = JsonDocument.Parse(File.ReadAllText(providerPlanPath));
foreach (var provider in plan.RootElement.GetProperty("providerFiles").EnumerateArray())
    if (Hash(provider.GetProperty("path").GetString()!) != provider.GetProperty("sha256").GetString())
        throw new ArgumentException("Candidate provider file changed.");
var bindings = plan.RootElement.GetProperty("imports").EnumerateArray()
    .Where(item => item.TryGetProperty("exportResolved", out var resolved) && resolved.GetBoolean())
    .Select(item => new CSharpExternalFunctionBinding(
        item.GetProperty("capturedImport").GetProperty("linkageName").GetString()!,
        item.GetProperty("requestedProviderPath").GetString()!, item.GetProperty("hostEntryPoint").GetString()!))
    .ToArray();
if (bindings.Length != 88) throw new ArgumentException("Expected exactly the accepted 88 candidate system bindings.");
var stopwatch = Stopwatch.StartNew();
var emissionOptions = new CSharpEmissionOptions
{
    Executable = false,
    RuntimeProjectPath = Path.GetFullPath(args[2]),
    ExternalFunctions = bindings,
    NativeErrorProtocol = NativeErrorProtocol.LinuxX64LogicalErrno
};
object report;
var exit = 2;
try
{
    Console.Error.WriteLine("stage=public-object-open");
    using var input = RoslynObject.Open(inputPath);
    Console.Error.WriteLine("stage=full-object-library-emission");
    var result = new CSharpEmitter().Emit([input], emissionOptions);
    Console.Error.WriteLine("stage=diagnostic-report");
    var errors = result.Diagnostics.Count(d => d.Severity == DiagnosticSeverity.Error);
    report = new
    {
        status = errors == 0 ? "emission-returned-without-diagnostics-unpublished" : "diagnostics-incomplete-translation",
        scope = "Unchanged full-object library emission diagnostic only; candidate export bindings are not production ABI/error/thread validation.",
        input = new { path = inputPath, sha256 = inputSha, bytes = new FileInfo(inputPath).Length },
        providerPlan = new { path = providerPlanPath, sha256 = Hash(providerPlanPath), candidateBindings = bindings.Length },
        build = BuildFacts(),
        nativeErrorProtocol = emissionOptions.NativeErrorProtocol.ToString(),
        intrinsicAdmission = "Recorded builtin kinds and actual operands are checked by the bounded emitter/runtime implementation; this probe does not execute them.",
        providerAdmission = "All 88 bindings are candidates from the export audit; focused provider fixture evidence does not establish every SQLite provider contract.",
        artifact = input.Manifest.ArtifactIdentity,
        translationComplete = false,
        outputPublished = false,
        emittedFunctions = result.EmittedFunctionCount,
        emittedRecords = result.EmittedRecordCount,
        returnedFiles = result.Files.Count,
        elapsedSeconds = stopwatch.Elapsed.TotalSeconds,
        diagnosticCount = result.Diagnostics.Count,
        errorCount = errors,
        families = result.Diagnostics.GroupBy(d => d.Family ?? "unspecified").OrderByDescending(g => g.Count())
            .Select(g => new { family = g.Key, count = g.Count(), messages = g.GroupBy(d => d.Message).OrderByDescending(m => m.Count()).Select(m => new { message = m.Key, count = m.Count() }) }),
        diagnostics = result.Diagnostics
    };
    exit = errors == 0 ? 0 : 1;
    Console.WriteLine(JsonSerializer.Serialize(new { errors, families = result.Diagnostics.GroupBy(d => d.Family ?? "unspecified").Select(g => new { family = g.Key, count = g.Count() }) }));
}
catch (Exception error)
{
    report = new { status = "unhandled-diagnostic-probe-failure", inputPath, inputSha, build = BuildFacts(),
        translationComplete = false, outputPublished = false, elapsedSeconds = stopwatch.Elapsed.TotalSeconds,
        error = error.ToString() };
    Console.Error.WriteLine(error);
}
Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);
File.WriteAllText(reportPath, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }) + "\n");
return exit;

static string Hash(string path)
{
    using var stream = File.OpenRead(path);
    return Convert.ToHexStringLower(SHA256.HashData(stream));
}

static object[] BuildFacts() => new[] { typeof(CSharpEmitter).Assembly.Location, typeof(RoslynObject).Assembly.Location,
        typeof(Program).Assembly.Location }.Select(path => (object)new { path, sha256 = Hash(path), bytes = new FileInfo(path).Length }).ToArray();
