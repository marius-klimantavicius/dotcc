using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using RoslynObj;

if (args.Length != 2 || File.Exists(args[1])) throw new ArgumentException("LoaderUseAudit OBJECT NEW_REPORT");
var timer = Stopwatch.StartNew();
using var input = RoslynObject.Open(args[0], new ReadOptions { CachedRowCount = 4096, CachedPageCount = 128, PageSize = 128 });
string[] entries = ["dlopen", "dlerror", "dlsym", "dlclose"];
var targets = input.TranslationUnit.Functions.Where(function => entries.Contains(function.GnuSupplement?.AssemblyName ?? function.CurrentName)).ToHashSet();
var uses = new List<object>();
var traces = new List<object>();
foreach (var function in input.TranslationUnit.Functions)
{
    if (function.Body is not { } body) continue;
    // Public read-only inventory, never an execution schedule.
    var operations = body.Root.DescendantOperations().ToArray();
    var references = operations.OfType<ReferenceOperation>().Where(reference => reference.Target is FunctionSymbol symbol && targets.Contains(symbol)).ToArray();
    if (references.Length == 0) continue;
    foreach (var reference in references)
    {
        var parents = reference.StructuralParents.OfType<Operation>().ToArray();
        uses.Add(new { Function = function.CurrentName, Entry = ((FunctionSymbol)reference.Target!).CurrentName,
            CalleeOnly = parents.All(parent => parent is CallOperation call && call.Callee.Equals(reference)),
            Parents = parents.Select(Describe).ToArray() });
    }
    traces.Add(new { Function = function.CurrentName, Return = TypeFacts(function.ReturnType),
        Locals = body.Locals.Select(variable => new { variable.Name, Type = TypeFacts(variable.Type) }).ToArray(),
        Operations = operations.Select((operation, ordinal) => new { Ordinal = ordinal + 1, Facts = Describe(operation),
            Parents = operation.StructuralParents.OfType<Operation>().Select(parent => Array.IndexOf(operations, parent) + 1).ToArray() }).ToArray() });
}
var constants = input.TranslationUnit.Constants.Where(constant => constant.Value is AddressValue { BaseEntity: FunctionSymbol function } && targets.Contains(function))
    .Select(constant => new { Entry = ((FunctionSymbol)((AddressValue)constant.Value).BaseEntity!).CurrentName,
        Displacement = ((AddressValue)constant.Value).ByteOffset.ToString(), Type = constant.Type is { } type ? TypeFacts(type) : null }).ToArray();
var report = new
{
    Scope = "Actual selected public loader signatures, uses and containing operation traces; no source parsing or general escape/ownership conclusion",
    Input = new { Path = Path.GetFullPath(args[0]), Sha256 = Hash(args[0]), Bytes = new FileInfo(args[0]).Length },
    input.Manifest.ArtifactIdentity,
    Signatures = targets.Select(function => new { Entry = function.GnuSupplement?.AssemblyName ?? function.CurrentName,
        function.Signature.HasPrototype, function.Signature.HasThisParameter, function.Signature.ReturnsValueAsParameter,
        function.Signature.ReturnsValueViaCopyConstructor, Return = TypeFacts(function.ReturnType),
        Parameters = function.Signature.Parameters.Select(parameter => new { Type = TypeFacts(parameter.Type), Role = parameter.Role.Name }).ToArray() }).ToArray(),
    Uses = uses, AddressConstants = constants, Traces = traces, ElapsedSeconds = timer.Elapsed.TotalSeconds, Reader = input.Statistics
};
Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(args[1]))!);
File.WriteAllText(args[1], JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }) + "\n");
Console.WriteLine($"selected-declarations={targets.Count}; references={uses.Count}; containing-functions={traces.Count}; address-constants={constants.Length}");
static object Describe(Operation operation) => new
{
    Kind = operation is OperatorOperation op ? op.OperatorKind.Name : operation.Kind.Name,
    Type = operation.Type is { } type ? TypeFacts(type) : null,
    Category = (operation as ExpressionOperation)?.ValueCategory.Name,
    Reference = operation is ReferenceOperation reference ? reference.Target?.Name : null,
    Call = operation is CallOperation call ? call.TargetMethod?.CurrentName : null,
    Arguments = operation is CallOperation invocation ? invocation.Arguments.Select(argument => new
    { argument.Ordinal, Role = argument.Role?.Name, Type = TypeFacts(argument.Value.Type),
        Reference = argument.Value is ReferenceOperation reference ? reference.Target?.Name : null }).ToArray() : null,
    Target = operation is InitializationOperation initialization ? initialization.Target?.Name : null,
    Jump = operation is JumpOperation jump ? jump.JumpKind.Name : null
};
static object TypeFacts(CppType type)
{
    var raw = type;
    while (raw is TypeReference reference) raw = reference.UnderlyingType;
    return new { Kind = type.Kind.Name, Size = type.Size?.ToString(), Alignment = type.Alignment?.ToString(),
        Raw = raw.Kind.Name, Bits = raw is ArithmeticType arithmetic ? arithmetic.Arithmetic.Bits : (long?)null,
        Signed = raw is ArithmeticType integer ? integer.Arithmetic.Signed : null,
        Pointee = raw is PointerType pointer ? TypeFacts(pointer.PointeeType) : null };
}
static string Hash(string path) { using var stream = File.OpenRead(path); return Convert.ToHexStringLower(SHA256.HashData(stream)); }
