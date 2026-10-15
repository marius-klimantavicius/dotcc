using System.Text.Json;
using RoslynObj;
using RoslynObj.CSharp;

var cleanupMode = args.FirstOrDefault() == "--cleanup";
if (cleanupMode) args = args.Skip(1).ToArray();
if (args.Length != 2) throw new ArgumentException("Usage: Inventory [--cleanup] INPUT.roslynobj OUTPUT_DIRECTORY");
using var obj = RoslynObject.Open(args[0]);
var manifest = obj.Manifest;
var request = new TranslationRequest
{
    Inputs = [new() { Path = Path.GetFullPath(args[0]), Sha256 = Translator.HashFile(args[0]),
        Producer = manifest.Producer, EdgRevision = manifest.EdgRevision, EdgVersion = manifest.EdgVersion,
        ArtifactIdentity = manifest.ArtifactIdentity, CaptureProfile = manifest.CaptureProfile }],
    Target = new() { Abi = manifest.Target.Abi, AddressableUnitBits = manifest.Target.AddressableUnitBits,
        Endianness = manifest.Target.Endianness, PointerBits = (int)manifest.Target.PointerBits,
        PointerAlignment = (int)manifest.Target.PointerAlignment, OperatingSystem = "linux", Architecture = "x64",
        Libc = "glibc", OutputRid = "linux-x64" },
    Output = new() { Directory = Path.GetFullPath(args[1]) }
};
var translator = new Translator();
var first = translator.Inventory(request);
Require(first.Status == TranslationStatus.InventoryComplete, string.Join("\n", first.Diagnostics.Select(d => d.Message)));
if (cleanupMode)
{
    ProgramManifest.WriteReport(args[1] + "-cleanup-inventory.json", first.ProgramReport);
    var body = obj.TranslationUnit.Functions.Single(f => f.Name == "cleanup_context").Body!;
    var actualOperations = body.DescendantOperations().ToArray();
    Require(actualOperations.OfType<CallOperation>().Count(c => c.TargetMethod is { SpecialFunctionKind.Name: "sfk_constructor" }) == 2,
        "genuine body must contain precisely two selected constructor call occurrences");
    var function = first.ProgramReport.Functions.Single(f => f.Name == "cleanup_context");
    Require(function.OperationCount == actualOperations.LongLength,
        "metadata recipes and EH associations must not add body operation identities");
    Require(first.ProgramReport.InitializationRecipes.Any(r => r.CleanupDependency && !r.ExecutionRoot && r.Destructor is not null),
        "genuine exceptional cleanup recipes must be required cleanup dependencies without normal initialization execution");
    Require(first.ProgramReport.InitializationRecipes.Where(r => r.OperationRole == "metadata").All(r => !r.ExecutionRoot),
        "metadata recipe associations must never be promoted to normal execution roots");
    Require(first.ProgramReport.InitializationRecipes.Any(r => r.OperationRole == "metadata" && r.CleanupDependency && !r.ExecutionRoot),
        "recorded metadata recipe identities must remain required exceptional dependencies without becoming normal execution");
    Require(first.ProgramReport.Families.Any(f => f.Family == "cleanup-action" && f.RequiredCount > 0),
        "genuine table EH must inventory selected exceptional actions");
    Require(first.ProgramReport.Imports.Single(i => i.Name == "may_throw").Required,
        "actual throwing boundary remains a required import");
    Console.WriteLine("Genuine C++ normal/metadata/exceptional cleanup context checks passed.");
    return;
}
var variables = first.ProgramReport.Storage.ToDictionary(s => s.Name!, StringComparer.Ordinal);
Require(variables["imported"].Classification == "external-declaration-recorded", "extern declaration must remain an import");
foreach (var name in new[] { "tentative", "redeclared", "localStatic", "initialized" })
    Require(variables[name].Classification == "storage-definition", $"{name} must retain actual definition storage");
Require(first.ProgramReport.Imports.Single(i => i.Name == "imported").Required, "referenced external variable is required");
Require(!first.ProgramReport.Imports.Any(i => i.Name is "tentative" or "redeclared" or "localStatic" or "initialized"), "definitions must not become imports");
var second = translator.Inventory(request);
Require(JsonSerializer.Serialize(first.ProgramReport, ProgramManifest.JsonOptions) == JsonSerializer.Serialize(second.ProgramReport, ProgramManifest.JsonOptions), "complete reports must repeat deterministically");
var failed = translator.Translate(request);
Require(failed.Status == TranslationStatus.Failed && failed.GeneratedFiles.Count == 0 && !failed.ProgramReport.TranslationComplete,
    "unimplemented required imports must fail closed");
Require(!Directory.Exists(request.Output.Directory), "failed translation must not publish an output directory");
var wrongTarget = translator.Inventory(request with { Target = request.Target with { PointerBits = 32 } });
Require(wrongTarget.Status == TranslationStatus.Failed && !wrongTarget.ProgramReport.InventoryComplete, "unsupported target must fail admission");
var duplicate = translator.Inventory(request with { Inputs = [request.Inputs[0], request.Inputs[0]] });
Require(duplicate.Status == TranslationStatus.Failed && duplicate.Diagnostics.Any(d => d.Code == "ROC1003"), "duplicate artifact identities must fail by default");
var cancelled = translator.Inventory(request, new CancellationToken(true));
Require(cancelled.Status == TranslationStatus.Cancelled && !cancelled.ProgramReport.InventoryComplete, "cancelled inventory must stay incomplete");
Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(args[1]))!);
ProgramManifest.WriteReport(args[1] + "-inventory.json", first.ProgramReport);
Console.WriteLine("Inventory admission/storage/import/determinism/cancellation checks passed.");
static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
