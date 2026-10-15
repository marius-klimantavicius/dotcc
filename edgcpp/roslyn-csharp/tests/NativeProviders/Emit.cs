using System.Text.Json;
using RoslynObj;
using RoslynObj.CSharp;

if (args.Length != 4) throw new ArgumentException("Emit OBJECT OUTPUT RUNTIME_PROJECT EXACT_LIBC_PATH");
using var input = RoslynObject.Open(args[0]);
string[] symbols = ["__errno_location", "open", "getpid", "qsort", "pthread_create", "pthread_join",
    "pthread_mutexattr_init", "pthread_mutexattr_settype", "pthread_mutex_init", "pthread_mutex_lock", "pthread_mutex_trylock",
    "pthread_mutex_unlock", "pthread_mutex_destroy", "pthread_mutexattr_destroy"];
var bindings = symbols.Select(name => new CSharpExternalFunctionBinding(name, Path.GetFullPath(args[3]), name))
    // Exact reviewed provider alias, never a prefix-stripping rule.
    .Append(new CSharpExternalFunctionBinding("__USER_LABEL_PREFIX__open64", Path.GetFullPath(args[3]), "open64")).ToArray();
var result = new CSharpEmitter().Emit([input], new CSharpEmissionOptions
{
    Executable = false, RuntimeProjectPath = Path.GetFullPath(args[2]), ExternalFunctions = bindings,
    NativeErrorProtocol = NativeErrorProtocol.LinuxX64LogicalErrno
});
foreach (var diagnostic in result.Diagnostics) Console.WriteLine($"{diagnostic.Code}: {diagnostic.Message} [{diagnostic.Locator}]");
if (result.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error)) return 1;
if (Directory.Exists(args[1])) throw new ArgumentException("Use a new output directory.");
Directory.CreateDirectory(args[1]);
foreach (var file in result.Files) File.WriteAllText(Path.Combine(args[1], file.RelativePath), file.Content);
Console.WriteLine(JsonSerializer.Serialize(new { functions = result.EmittedFunctionCount, records = result.EmittedRecordCount, files = result.Files.Select(f => f.RelativePath) }));
return 0;
