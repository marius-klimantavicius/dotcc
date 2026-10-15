using System.Text.Json;
using RoslynObj;
using RoslynObj.CSharp;

if (args.Length != 4) throw new ArgumentException("LoaderEmit OBJECT NEW_OUTPUT RUNTIME_PROJECT EXACT_LIBC_PATH");
using var input = RoslynObject.Open(args[0]);
string[] symbols = ["__errno_location", "dlopen", "dlsym", "dlclose", "dlerror"];
var result = new CSharpEmitter().Emit([input], new CSharpEmissionOptions
{
    Executable = false, RuntimeProjectPath = Path.GetFullPath(args[2]), NativeErrorProtocol = NativeErrorProtocol.LinuxX64LogicalErrno,
    ExternalFunctions = symbols.Select(name => new CSharpExternalFunctionBinding(name, Path.GetFullPath(args[3]), name)).ToArray()
});
foreach (var diagnostic in result.Diagnostics) Console.WriteLine($"{diagnostic.Code}: {diagnostic.Message} [{diagnostic.Locator}]");
if (result.Diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)) return 1;
if (Directory.Exists(args[1])) throw new ArgumentException("Use a new output directory.");
Directory.CreateDirectory(args[1]);
foreach (var file in result.Files) File.WriteAllText(Path.Combine(args[1], file.RelativePath), file.Content);
Console.WriteLine(JsonSerializer.Serialize(new { functions = result.EmittedFunctionCount, records = result.EmittedRecordCount, files = result.Files.Select(file => file.RelativePath) }));
return 0;
