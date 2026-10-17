using System.Security.Cryptography;
using System.Text.Json;
using RoslynObj;
using RoslynObj.CSharp;

if (args.Length < 2) throw new ArgumentException("inspect OBJECT | emit OUTPUT RUNTIME_PROJECT OBJECT [OBJECT]");
if (args[0] == "split")
{
    if (args.Length != 3) throw new ArgumentException("split GENERATED_DIRECTORY NEW_OUTPUT");
    SourceSplit.Write(args[1], args[2]);
    return 0;
}
if (args[0] == "inspect")
{
    using var input = RoslynObject.Open(args[1]);
    Console.WriteLine(JsonSerializer.Serialize(new
    {
        input.Manifest.ArtifactIdentity,
        Variables = input.TranslationUnit.Variables.Where(variable => !variable.Details.IsParameter).Select(variable => new
        {
            variable.Name, Source = variable.SourceCorrespondence?.SourceName,
            Duration = variable.Details.StorageDuration?.Name, Kind = variable.InitializationKind.Name,
            variable.IsThreadLocal, variable.StandardThreadLocalDeclared, variable.StandardThreadLocalEffective,
            variable.ExtensionThreadLocal, Size = variable.Type.Size?.ToString(), Alignment = variable.Type.Alignment?.ToString(),
            ExplicitAlignment = variable.Details.ExplicitAlignment?.ToString(),
            Initializer = variable.Initializer?.Kind.Name, Constant = variable.ConstantInitializer?.Value.Kind.Name,
            DynamicRoutine = variable.Details.DynamicInitializationRoutine?.Name,
            ThreadRoutine = variable.Details.ThreadInitializationRoutine?.Name,
            ThreadWrapper = variable.Details.ThreadWrapperRoutine?.Name,
            Cleanup = variable.CleanupRoutine?.Name, LocalRecipes = variable.LocalStaticInitializations.Count,
            HasDynamicRoutine = variable.Details.DynamicInitializationRoutine is not null,
            HasThreadRoutine = variable.Details.ThreadInitializationRoutine is not null,
            HasThreadWrapper = variable.Details.ThreadWrapperRoutine is not null,
            HasCleanup = variable.CleanupRoutine is not null,
            LocalOwner = variable.SourceCorrespondence?.EnclosingRoutine?.Name,
            variable.Details.PromotedLocalStatic, variable.Details.PromotedLocalStaticInitialization,
            variable.Details.HasExplicitInitializer, variable.Details.InitializationRewrittenAsAssignment
        }),
        Functions = input.TranslationUnit.Functions.Select(function => new
        {
            function.Name, function.Details.IsTlsInitializationRoutine, function.Details.IsTlsInitializationAlias,
            Body = function.BodyState.Name,
            Calls = function.Body?.Root.DescendantOperations().OfType<CallOperation>().Select(call => new
            {
                Target = call.TargetMethod?.Name,
                IsTlsInitializationRoutine = call.TargetMethod?.Details.IsTlsInitializationRoutine,
                IsTlsInitializationAlias = call.TargetMethod?.Details.IsTlsInitializationAlias
            })
        }),
        FileInitializers = input.TranslationUnit.FileScopeInitializationRoutines.Select(row => new { row.Routine.Name, row.IsThreadLocal })
    }, new JsonSerializerOptions { WriteIndented = true }));
    return 0;
}
if (args[0] != "emit" || args.Length is < 4 or > 5) throw new ArgumentException("emit OUTPUT RUNTIME_PROJECT OBJECT [OBJECT]");
var output = Path.GetFullPath(args[1]);
if (Path.Exists(output)) throw new ArgumentException("Output must be new.");
var paths = args.Skip(3).ToArray();
var inputs = paths.Select(path => RoslynObject.Open(path)).ToArray();
try
{
    var options = new CSharpEmissionOptions
    {
        RuntimeProjectPath = Path.GetFullPath(args[2]),
        ProgramAssembly = inputs.Length == 1 ? null : new ProgramAssemblyOptions
        {
            MaximumOpenUnits = 2,
            InputSha256 = paths.Select(path => Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(path)))).ToArray()
        }
    };
    var emitter = new CSharpEmitter();
    var result = emitter.Emit(inputs, options);
    foreach (var diagnostic in result.Diagnostics)
        Console.WriteLine($"{diagnostic.Code}: {diagnostic.Message} [{diagnostic.Family}; {diagnostic.Locator}]");
    if (result.Diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)) return 1;
    var repeated = emitter.Emit(inputs, options);
    if (repeated.Diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error) || !result.Files.SequenceEqual(repeated.Files))
        throw new InvalidOperationException("Storage emission changed on repeat.");
    Directory.CreateDirectory(output);
    foreach (var file in result.Files) File.WriteAllText(Path.Combine(output, file.RelativePath), file.Content);
    Console.WriteLine($"Emitted {result.EmittedFunctionCount} functions deterministically.");
    return 0;
}
finally { foreach (var input in inputs) input.Dispose(); }
