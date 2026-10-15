using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using RoslynObj;

if (args.Length != 2) throw new ArgumentException("Expected genuine object path and output JSON path.");
var stopwatch = Stopwatch.StartNew();
using var input = RoslynObject.Open(args[0], new ReadOptions { CachedRowCount = 4096, CachedPageCount = 128, PageSize = 128 });
string[] kinds = ["bfk_atomic_load_1", "bfk_atomic_load_2", "bfk_atomic_load_4", "bfk_atomic_load_8",
    "bfk_atomic_store_1", "bfk_atomic_store_2", "bfk_atomic_store_4", "bfk_atomic_store_8",
    "bfk_sync_synchronize", "bfk_bswap16", "bfk_bswap32", "bfk_bswap64", "bfk_clzll",
    "bfk_add_overflow", "bfk_sub_overflow", "bfk_mul_overflow"];
bool Selected(FunctionSymbol function) => kinds.Contains(function.BuiltinKind?.Name, StringComparer.Ordinal);
var signatures = input.TranslationUnit.Functions.Where(Selected).Select(function => new
{
    function.CurrentName, Builtin = function.BuiltinKind?.Name,
    CallingConvention = function.Signature.CallingConvention?.Name,
    function.Signature.HasPrototype, function.Signature.IsVariadic, function.Signature.HasThisParameter,
    function.Signature.ReturnsValueAsParameter, function.Signature.ReturnsValueViaCopyConstructor,
    Result = TypeFacts(function.ReturnType),
    Parameters = function.Signature.Parameters.Select(parameter => new
    { parameter.Ordinal, Role = parameter.Role.Name, parameter.Descriptor.Details.PassedViaCopyConstructor, Type = TypeFacts(parameter.Type) }).ToArray()
}).ToArray();
var calls = new List<object>();
var seen = new HashSet<CallOperation>();
var functionOrdinal = 0;
foreach (var function in input.TranslationUnit.Functions)
{
    ++functionOrdinal;
    if (function.Body is not { } body) continue;
    // Identity-deduplicated public inventory only. This is never execution order.
    foreach (var call in body.Root.DescendantOperations().OfType<CallOperation>())
    {
        var operandFunction = (call.Callee as ReferenceOperation)?.Target as FunctionSymbol;
        var selected = operandFunction ?? call.TargetMethod;
        if (selected is null || !Selected(selected) || !seen.Add(call)) continue;
        calls.Add(new
        {
            InventoryLocator = $"function:{functionOrdinal}/intrinsic-call:{calls.Count + 1}",
            ContainingFunction = function.CurrentName, Builtin = selected.BuiltinKind?.Name,
            OperandFunction = operandFunction?.CurrentName, TargetMethod = call.TargetMethod?.CurrentName,
            TargetsAgree = operandFunction is null || call.TargetMethod is null || operandFunction.Equals(call.TargetMethod),
            call.Operator.EvalLeftToRight, call.Operator.EvalRightToLeft,
            HasReceiver = call.Receiver is not null, HasReturnSlot = call.Call.ReturnSlot is not null,
            Result = TypeFacts(call.Type),
            Arguments = call.Arguments.Select(argument => new
            { argument.Ordinal, Role = argument.Role?.Name, Facts = OperandFacts(argument.Value) }).ToArray()
        });
    }
}
stopwatch.Stop();
var output = Path.GetFullPath(args[1]);
Directory.CreateDirectory(Path.GetDirectoryName(output)!);
using (var stream = File.Create(output))
    JsonSerializer.Serialize(stream, new
    {
        InventoryOnly = true, ExecutionOrderAccepted = false, GeneratedExecutionAccepted = false,
        Artifact = input.Manifest.ArtifactIdentity, ReaderOptions = new { CachedRows = 4096, CachedPages = 128, PageSize = 128 },
        ElapsedSeconds = stopwatch.Elapsed.TotalSeconds, Signatures = signatures, Calls = calls,
        ReaderStatistics = input.Statistics
    }, new JsonSerializerOptions { WriteIndented = true });
Console.WriteLine($"signatures={signatures.Length}; unique-inventory-calls={calls.Count}; output={output}");

static object OperandFacts(ExpressionOperation operation)
{
    var stages = new List<object>();
    var seen = new HashSet<ExpressionOperation>();
    for (var current = operation; seen.Add(current);)
    {
        stages.Add(new
        {
            Kind = current is OperatorOperation op ? op.OperatorKind.Name : current.Kind.Name,
            Type = TypeFacts(current.Type), current.Details.VolatileFetch,
            Integer = current is ConstantOperation { Constant.Value: IntegerValue integer } ? integer.Value.ToString(CultureInfo.InvariantCulture) : null,
            ReferencedName = current is ReferenceOperation reference ? reference.Target?.Name : null
        });
        if (current is ConversionOperation conversion) current = conversion.Operand;
        else if (current is OperatorOperation { OperatorKind.Name: "eok_parens", Operands.Count: 1 } parentheses) current = parentheses.Operands[0];
        else break;
    }
    return new { ActualType = TypeFacts(operation.Type), ConversionOrParenthesesChain = stages };
}

static object TypeFacts(CppType type, int pointerDepth = 0)
{
    var chain = new List<object>();
    var seen = new HashSet<CppType>();
    var raw = type;
    while (raw is TypeReference reference && seen.Add(raw))
    {
        chain.Add(new
        {
            Dimensions = Dimensions(raw), raw.Details.IsAtomic,
            Qualifiers = raw is QualifiedType qualified ? new
            {
                qualified.Qualification.IsConst, qualified.Qualification.IsVolatile, qualified.Qualification.IsAtomic,
                qualified.Qualification.AddressSpace, qualified.Qualification.IsUnaligned,
                qualified.Qualification.IsNear, qualified.Qualification.IsFar, qualified.Qualification.IsShared
            } : null
        });
        raw = reference.UnderlyingType;
    }
    return new
    {
        Effective = Dimensions(type), Wrappers = chain, Raw = Dimensions(raw), raw.Details.IsAtomic,
        Arithmetic = raw is ArithmeticType arithmetic ? new
        { Kind = arithmetic.Arithmetic.ArithmeticKind.Name, arithmetic.Arithmetic.Bits, arithmetic.Arithmetic.Signed, arithmetic.Arithmetic.IsBool, IntegerKind = arithmetic.Arithmetic.EdgIntegerKind?.Name } : null,
        Pointee = pointerDepth < 2 && raw is PointerType pointer ? TypeFacts(pointer.PointeeType, pointerDepth + 1) : null
    };
}
static object Dimensions(CppType type) => new
{
    Kind = type.Kind.Name, Size = type.Size?.ToString(CultureInfo.InvariantCulture), Alignment = type.Alignment?.ToString(CultureInfo.InvariantCulture),
    DescriptorSize = type.Details.DescriptorSize?.ToString(CultureInfo.InvariantCulture),
    DescriptorAlignment = type.Details.DescriptorAlignment?.ToString(CultureInfo.InvariantCulture)
};
