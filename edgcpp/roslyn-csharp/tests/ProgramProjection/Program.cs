using System.Security.Cryptography;
using System.Text.Json;
using RoslynObj;
using RoslynObj.CSharp;

if (args.Length != 6 || args[0] != "emit" || args[1] is not ("c" or "cpp"))
    throw new ArgumentException("emit c|cpp INPUT_A INPUT_B OUTPUT RUNTIME_PROJECT");
var output = Path.GetFullPath(args[4]);
if (Directory.Exists(output) || File.Exists(output)) throw new ArgumentException("Output must be a new directory.");
using var first = RoslynObject.Open(args[2]);
using var second = RoslynObject.Open(args[3]);
RoslynObject[] inputs = [first, second];
var assemblyOptions = new ProgramAssemblyOptions
{
    MaximumOpenUnits = 2,
    InputSha256 = [Hash(args[2]), Hash(args[3])]
};
var assembly = new ProgramAssembler().Assemble(inputs, assemblyOptions);
foreach (var diagnostic in assembly.Diagnostics) Console.Error.WriteLine($"{diagnostic.Code}: {diagnostic.Message} [{diagnostic.Locator}]");
if (!assembly.Success) return 1;
var program = assembly.Program!;

// Source spellings here identify fixture assertions only. The emitter binds using
// accepted public ProgramAssembly identities, never these assertion filters.
var localFunctions = program.Functions.Where(function => ObservationName(function.Definition) == "local_helper").ToArray();
Require(localFunctions.Length == 2 && localFunctions.Select(function => function.UnitOrdinal).Distinct().Count() == 2
    && localFunctions.All(function => function.ExternalName is null)
    && !localFunctions[0].Definition.Equals(localFunctions[1].Definition), "distinct same-spelling TU-local functions");
var localStorage = program.Storage.Where(storage => ObservationName(storage.Definition) == "local_value").ToArray();
Require(localStorage.Length == 2 && localStorage.Select(storage => storage.UnitOrdinal).Distinct().Count() == 2
    && localStorage.All(storage => storage.ExternalName is null)
    && !localStorage[0].Definition.Equals(localStorage[1].Definition), "distinct same-spelling TU-local data");
var shared = program.Storage.Single(storage => ObservationName(storage.Definition) == "shared_value");
Require(shared.ExternalName is not null && shared.Declarations.Count >= 2, "one selected external data definition");
foreach (var declaration in shared.Declarations)
    Require(program.Resolve(declaration)?.Definition.Equals(shared.Definition) == true, "canonical storage declarations");

var callback = args[1] == "c"
    ? program.Functions.Single(function => ObservationName(function.Definition) == "shared_add")
    : program.Functions.Single(function => ObservationName(function.Definition) == "choose"
        && function.Definition.Parameters.Count == 1 && Unwrap(function.Definition.Parameters[0].Type) is ArithmeticType { Arithmetic.Signed: true, Arithmetic.Bits: 32 });
Require(callback.ExternalName is not null && callback.Declarations.Count >= 2, "cross-TU selected callback declaration/definition");
foreach (var declaration in callback.Declarations)
    Require(program.Resolve(declaration)?.Definition.Equals(callback.Definition) == true, "canonical function declarations");

var options = new CSharpEmissionOptions
{
    Executable = true,
    EntryPointName = "fixture",
    PrintEntryResult = true,
    RuntimeProjectPath = Path.GetFullPath(args[5]),
    ProgramAssembly = assemblyOptions
};
var emitter = new CSharpEmitter();
var emitted = emitter.Emit(inputs, options);
foreach (var diagnostic in emitted.Diagnostics) Console.Error.WriteLine($"{diagnostic.Code}: {diagnostic.Message} [{diagnostic.Locator}]");
if (emitted.Diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)) return 1;
var repeated = emitter.Emit(inputs, options);
Require(!repeated.Diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
    && emitted.Files.SequenceEqual(repeated.Files), "deterministic repeated emission");
Require(emitted.EmittedFunctionCount == program.Functions.Count, "exactly one emitted body per selected definition");
Require(emitted.Files.All(file => file.RelativePath is not ("Generated.NativeImports.json" or "Generated.Abi.c")),
    "no native application/provider dependency in the fixed scalar/pointer candidate");

var reportSource = emitted.Files.Single(file => file.RelativePath == "Generated.ProgramAssembly.json");
using var report = JsonDocument.Parse(reportSource.Content);
var root = report.RootElement;
Require(root.GetProperty("maximumOpenUnits").GetInt32() == 2 && !root.GetProperty("boundedScaleAccepted").GetBoolean(), "explicit bounded candidate report");
var storageRows = root.GetProperty("storage").EnumerateArray().ToArray();
Require(storageRows.Length == program.Storage.Count, "selected storage report count");
var allocationFields = storageRows.Select(row => row.GetProperty("allocationField").GetString()).ToArray();
Require(allocationFields.All(field => !string.IsNullOrWhiteSpace(field))
    && allocationFields.Distinct(StringComparer.Ordinal).Count() == program.Storage.Count, "one admitted allocation per selected storage identity");
var sharedRows = storageRows.Where(row => row.GetProperty("locator").GetString() == shared.Locator).ToArray();
Require(sharedRows.Length == 1 && sharedRows[0].GetProperty("externalName").GetProperty("name").GetString() == shared.ExternalName!.Name,
    "exact captured shared external identity and one allocation");
var functionRows = root.GetProperty("functions").EnumerateArray().ToArray();
Require(functionRows.Length == program.Functions.Count, "selected function report count");
var callbackRows = functionRows.Where(row => row.GetProperty("locator").GetString() == callback.Locator).ToArray();
Require(callbackRows.Length == 1 && !string.IsNullOrWhiteSpace(callbackRows[0].GetProperty("addressField").GetString())
    && callbackRows[0].GetProperty("externalName").GetProperty("name").GetString() == callback.ExternalName!.Name,
    "one canonical address field for the captured callback identity");
var allAddressFields = functionRows.Select(row => row.GetProperty("addressField").GetString()).Where(field => field is not null).ToArray();
Require(allAddressFields.Distinct(StringComparer.Ordinal).Count() == allAddressFields.Length, "distinct selected functions never share an address field");

Directory.CreateDirectory(output);
foreach (var file in emitted.Files) File.WriteAllText(Path.Combine(output, file.RelativePath), file.Content);
File.WriteAllText(Path.Combine(output, "ProgramSelection.json"), JsonSerializer.Serialize(assembly.Report,
    new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
File.WriteAllText(Path.Combine(output, "ProgramProjectionChecks.json"), JsonSerializer.Serialize(new
{
    fixture = args[1], inputSha256 = assemblyOptions.InputSha256,
    maximumOpenUnits = 2, selectedFunctions = program.Functions.Count, selectedStorage = program.Storage.Count,
    internalFunctionLocators = localFunctions.Select(function => function.Locator),
    internalStorageLocators = localStorage.Select(storage => storage.Locator),
    fixtureObservationNames = localFunctions.Select(function => new
    {
        function.Locator,
        sourceName = ObservationName(function.Definition),
        currentName = function.Definition.CurrentName,
        nameHasBeenMangled = function.Definition.SourceCorrespondence?.NameHasBeenMangled
    }),
    sharedStorageLocator = shared.Locator, sharedExternalName = shared.ExternalName,
    canonicalCallbackLocator = callback.Locator, canonicalCallbackName = callback.ExternalName,
    deterministic = true, generatedExecutionAccepted = false,
    limits = new[] { "two open units", "fixed scalar/pointer calls", "integer callback identity exercised only after execution receipt",
        "no native aggregate ABI", "no C++ ODR comparison", "no bounded-scale or SQLite execution claim" }
}, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine($"Emitted {emitted.EmittedFunctionCount} selected functions and {program.Storage.Count} selected allocations deterministically.");
return 0;

static string Hash(string path)
{
    using var stream = File.OpenRead(path);
    return Convert.ToHexStringLower(SHA256.HashData(stream));
}
static CppType Unwrap(CppType type)
{
    while (type is TypeReference reference) type = reference.UnderlyingType;
    return type;
}
static string? ObservationName(Symbol symbol) => symbol.SourceCorrespondence?.SourceName;
static void Require(bool condition, string contract)
{
    if (!condition) throw new InvalidOperationException("Program projection check failed: " + contract);
}
