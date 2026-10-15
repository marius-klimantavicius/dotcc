using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using RoslynObj;

if (args.Length != 2) throw new ArgumentException("ErrnoUseAudit OBJECT NEW_REPORT");
var objectPath = Path.GetFullPath(args[0]);
var reportPath = Path.GetFullPath(args[1]);
if (File.Exists(reportPath)) throw new ArgumentException("Use a fresh report path.");
using var input = RoslynObject.Open(objectPath);
var required = input.TranslationUnit.Functions.Where(f => (f.GnuSupplement?.AssemblyName ?? f.CurrentName) == "__errno_location").ToArray();
if (required.Length != 1) throw new InvalidOperationException("Expected one retained __errno_location declaration.");
var target = required[0];
var timer = Stopwatch.StartNew();
var uses = new List<object>();
var references = new List<object>();
var calls = 0;
var allDirectDereferences = true;
var addressTaking = false;
var noncalleeReferences = 0;
var allTerminalReads = true;
var recipeOverlap = 0;
var terminalCounts = new Dictionary<string, int>(StringComparer.Ordinal);
var functionOrdinal = 0;
foreach (var function in input.TranslationUnit.Functions)
{
    ++functionOrdinal;
    if (function.Body is not { } body) continue;
    var operationOrdinal = 0;
    // Read-only occurrence inventory, never an execution schedule.
    foreach (var operation in body.Root.DescendantOperations())
    {
        ++operationOrdinal;
        if (operation is ReferenceOperation reference && reference.Target?.Equals(target) == true)
        {
            var parents = reference.StructuralParents.OfType<Operation>().ToArray();
            var calleeOnly = parents.Length != 0 && parents.All(p => p is CallOperation call && call.Callee.Equals(reference));
            if (!calleeOnly) ++noncalleeReferences;
            references.Add(new { function = function.CurrentName, locator = $"function/{functionOrdinal}/operation/{operationOrdinal}",
                calleeOnly, parents = parents.Select(p => p.Kind.Name) });
        }
        if (operation is not CallOperation callOperation || callOperation.Callee is not ReferenceOperation { Target: FunctionSymbol called } || !called.Equals(target)) continue;
        ++calls;
        var expressionParents = callOperation.StructuralParents.OfType<ExpressionOperation>().ToArray();
        var directDereferences = expressionParents.Length != 0 && expressionParents.All(p => p is OperatorOperation op &&
            op.OperatorKind.Name == "eok_indirect" && op.ValueCategory.Name == "prvalue" && op.Operands.Count == 1 && op.Operands[0].Equals(callOperation) &&
            Unwrap(op.Type) is ArithmeticType { Arithmetic.Bits: 32, Arithmetic.Signed: true });
        allDirectDereferences &= directDereferences;
        var consumers = expressionParents.SelectMany(p => p.StructuralParents.OfType<Operation>()).Distinct().ToArray();
        var takesAddress = consumers.Any(p => p is OperatorOperation op && op.OperatorKind.Name is "eok_address_of" or "eok_ref_cast" or "eok_ref_dynamic_cast");
        addressTaking |= takesAddress;
        var terminals = new List<object>();
        foreach (var dereference in expressionParents)
        {
            var actualConsumers = dereference.StructuralParents.OfType<Operation>().ToArray();
            if (actualConsumers.Length == 0) allTerminalReads = false;
            foreach (var consumer in actualConsumers)
            {
                var classification = Terminal(consumer, dereference, function);
                allTerminalReads &= classification is not null;
                if (classification is not null) terminalCounts[classification] = terminalCounts.GetValueOrDefault(classification) + 1;
                var overlapsRecipe = consumer is InitializationOperation initialization &&
                    initialization.Initialization.Operation is { } root && root.DescendantOperations().Any(node => node.Equals(callOperation));
                if (overlapsRecipe) ++recipeOverlap;
                terminals.Add(new { classification, overlapsRecipe,
                    kind = consumer is OperatorOperation op ? op.OperatorKind.Name : consumer.Kind.Name,
                    dereferenceCategory = dereference.ValueCategory.Name,
                    target = consumer is CallOperation parentCall ? parentCall.TargetMethod?.CurrentName : null,
                    arguments = consumer is CallOperation argumentCall ? argumentCall.Arguments.Where(argument => argument.Value.Equals(dereference)).Select(argument => new
                    { ordinal = argument.Ordinal, valueType = argument.Value.Type.Kind.Name, valueCategory = argument.Value.ValueCategory.Name,
                        parameterType = argument.Parameter?.Type.Kind.Name, parameterRole = argument.Role?.Name }) : [] });
            }
        }
        uses.Add(new { function = function.CurrentName, locator = $"function/{functionOrdinal}/operation/{operationOrdinal}",
            directDereferences, takesAddress, terminals, hasReceiver = callOperation.Receiver is not null, hasReturnSlot = callOperation.Call.ReturnSlot is not null,
            parents = expressionParents.Select(p => new { kind = p is OperatorOperation op ? op.OperatorKind.Name : p.Kind.Name,
                type = p.Type.Kind.Name, category = p.ValueCategory.Name }),
            consumers = consumers.Select(p => new { kind = p is OperatorOperation op ? op.OperatorKind.Name : p.Kind.Name,
                type = p.Type?.Kind.Name, category = (p as ExpressionOperation)?.ValueCategory.Name,
                roles = p is ExpressionOperation expression ? expression.OperandRelations.Where(r => expressionParents.Contains(r.Expression)).Select(r => r.Role.Name + ":" + r.Ordinal) : [] }) });
    }
}
var report = new
{
    purpose = "Read-only public retained errno call/result-use inventory; no source parsing, raw SQL or execution scheduling",
    input = new { path = objectPath, sha256 = Hash(objectPath), bytes = new FileInfo(objectPath).Length },
    artifact = input.Manifest.ArtifactIdentity,
    declaration = new { name = target.CurrentName, linkageName = target.GnuSupplement?.AssemblyName ?? target.CurrentName,
        parameters = target.Parameters.Count, returnType = target.ReturnType.Kind.Name },
    callCount = calls, referenceCount = references.Count, noncalleeReferences,
    everyCallImmediatelyProducesSigned32Prvalue = allDirectDereferences,
    everyDereferenceHasWhitelistedTerminalRead = allTerminalReads, terminalCounts,
    initializerRootOverlapOccurrences = recipeOverlap,
    bodyPlusExplicitInitializerRootOccurrences = calls + recipeOverlap,
    occurrenceExplanation = "Body traversal deduplicates public node identity; explicit initialization roots revisit the same reported initializer call identities. Counts are inventory contexts, never an execution schedule.",
    dereferenceAddressTakingObserved = addressTaking,
    selectedPatternHasNoPointerEscape = calls != 0 && allDirectDereferences && allTerminalReads && !addressTaking && noncalleeReferences == 0,
    scope = "Only exact signed32 prvalue dereferences with whitelisted terminal value consumers establish this selected no-escape pattern. Lvalue/ref/comma/conditional forwarding is unclassified; no general alias-analysis claim.",
    uses, references,
    elapsedSeconds = timer.Elapsed.TotalSeconds,
    reader = input.Statistics
};
Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);
File.WriteAllText(reportPath, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }) + "\n");
Console.WriteLine($"calls={calls}; references={references.Count}; immediateDereferences={allDirectDereferences}; addressTaking={addressTaking}; noncalleeReferences={noncalleeReferences}");
return 0;

static bool Int32(CppType? type) => type is not null && Unwrap(type) is ArithmeticType { Arithmetic.Bits: 32, Arithmetic.Signed: true };
static string? Terminal(Operation consumer, ExpressionOperation value, FunctionSymbol function)
{
    if (value.ValueCategory.Name != "prvalue" || !Int32(value.Type)) return null;
    if (consumer is CallOperation call)
    {
        var arguments = call.Arguments.Where(a => a.Value.Equals(value)).ToArray();
        return arguments.Length == 1 && arguments[0].Parameter is { } parameter && Int32(parameter.Type)
            ? "ordinary-int32-call-argument" : null;
    }
    if (consumer is OperatorOperation operation)
    {
        if (operation.ValueCategory.Name != "prvalue" || !Int32(operation.Type)) return null;
        if (operation.OperatorKind.Name is "eok_eq" or "eok_ne" && operation.Operands.Count == 2 && operation.Operands.All(operand => Int32(operand.Type)))
            return operation.OperatorKind.Name == "eok_eq" ? "int32-equality-value" : "int32-inequality-value";
        if (operation.OperatorKind.Name == "eok_assign" && operation.Operands.Count == 2 && operation.Operands[1].Equals(value) && Int32(operation.Operands[0].Type))
            return "int32-assignment-rhs-value";
        return null;
    }
    if (consumer is InitializationOperation initialization && initialization.Initialization.Kind.Name == "dik_expression" &&
        initialization.Expression?.Equals(value) == true && initialization.Target is VariableSymbol variable && Int32(variable.Type) &&
        !initialization.Details.IsReusedValue && initialization.Constructor is null && initialization.Destructor is null)
        return "int32-local-initializer-value";
    if (consumer is JumpOperation jump && jump.JumpKind.Name == "return" && jump.Value?.Equals(value) == true && Int32(function.ReturnType))
        return "int32-return-value";
    return null;
}

static CppType Unwrap(CppType type) { while (type is TypeReference reference) type = reference.UnderlyingType; return type; }
static string Hash(string path) { using var stream = File.OpenRead(path); return Convert.ToHexStringLower(SHA256.HashData(stream)); }
