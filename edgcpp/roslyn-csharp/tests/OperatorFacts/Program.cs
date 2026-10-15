using System.Globalization;
using System.Text.Json;
using RoslynObj;

if (args.Length is not (2 or 4) || args.Length == 4 && args[2] != "--diagnostics")
    throw new ArgumentException("Expected OBJECT OUTPUT_JSON [--diagnostics FIRST_BLOCKER_REPORT].");
string? diagnosticReport = args.Length == 4 ? Path.GetFullPath(args[3]) : null;
var selectedDiagnostics = new List<(string Function, string Family, string Message, string Locator)>();
var representationDiagnostics = new List<(string Family, string Message, string Locator)>();
if (diagnosticReport is not null)
{
    using var document = JsonDocument.Parse(File.ReadAllText(diagnosticReport));
    foreach (var diagnostic in document.RootElement.GetProperty("diagnostics").EnumerateArray())
    {
        var family = diagnostic.GetProperty("Family").GetString();
        if (family is not ("compound-assignment" or "volatile" or "shift")) continue;
        var locator = diagnostic.GetProperty("Locator").GetString() ?? throw new InvalidOperationException("A selected diagnostic has no public function locator.");
        var separator = locator.IndexOf(':');
        if (!locator.StartsWith("function/", StringComparison.Ordinal) || separator < 0)
            throw new InvalidOperationException("A selected diagnostic is not scoped to a public function locator.");
        var name = locator[(separator + 1)..].Split('/')[0];
        if (name == "program")
        {
            representationDiagnostics.Add((family, diagnostic.GetProperty("Message").GetString()!, locator));
            continue;
        }
        selectedDiagnostics.Add((name, family, diagnostic.GetProperty("Message").GetString()!, locator));
    }
    if (selectedDiagnostics.Count == 0) throw new InvalidOperationException("The selected report has no compound/volatile/shift first blockers.");
}
using var input = RoslynObject.Open(args[0], new ReadOptions { CachedRowCount = 4096, CachedPageCount = 128, PageSize = 128 });
string[] selectedNames = ["pcache1InitBulk", "accessPayload", "freeTempSpace", "wherePathSolver",
    "sqlite3VarintLen", "computeMxChoice", "sqlite3FpDecode", "sqlite3Fp2Convert10",
    "sqlite3ReportError", "sqlite3GenerateColumnNames", "sqlite3Multiply128", "sqlite3Multiply160",
    "walCleanupHash", "walFramePgno", "sqlite3WalCheckpoint", "walIndexAppend", "walIteratorInit", "sqlite3WalSnapshotCheck",
    "walIndexPage", "walIndexHdr"];
if (diagnosticReport is not null)
    selectedNames = selectedDiagnostics.Select(diagnostic => diagnostic.Function).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
string[] compoundKinds = ["eok_padd_assign", "eok_psubtract_assign", "eok_shiftl_assign", "eok_shiftr_assign"];
var functions = input.TranslationUnit.Functions.Where(function => selectedNames.Contains(function.CurrentName, StringComparer.Ordinal)).ToArray();
if (representationDiagnostics.Count != 0 && input.TranslationUnit.Functions.Any(function => function.CurrentName == "program"))
    throw new InvalidOperationException("The explicit program representation locator is ambiguous with an actual current function name; no body is guessed.");
var missingNames = selectedNames.Except(functions.Select(function => function.CurrentName), StringComparer.Ordinal).ToArray();
var declarationOnlyNames = functions.Where(function => function.Body is null).Select(function => function.CurrentName).Distinct(StringComparer.Ordinal).ToArray();
if (missingNames.Length != 0 || declarationOnlyNames.Length != 0)
    throw new InvalidOperationException($"Selected public bodies unavailable: missing=[{string.Join(", ", missingNames)}]; declaration-only=[{string.Join(", ", declarationOnlyNames)}].");
var rows = new List<object>();
foreach (var function in functions)
{
    var body = function.Body!;
    // Public identity-deduplicated body graph observations only. The order of
    // this inventory is never an execution schedule or a reachability proof.
    var operations = body.Root.DescendantOperations().Distinct().ToArray();
    var operators = operations.OfType<OperatorOperation>().ToArray();
    var volatileExpressions = operations.OfType<ExpressionOperation>().Where(expression =>
        expression.Details.VolatileFetch || TypeContainsVolatile(expression.Type)).ToArray();
    var locals128 = body.Locals.Where(local => IsInteger128(local.Type)).ToArray();
    var references128 = operations.OfType<ReferenceOperation>()
        .Where(reference => reference.Target is VariableSymbol variable && locals128.Contains(variable)).ToArray();
    bool Contains128Reference(ExpressionOperation operand) => references128.Any(reference => operand.Equals(reference) || operand.DescendantOperations().Contains(reference));
    var is128Body = function.CurrentName is "sqlite3Multiply128" or "sqlite3Multiply160";
    rows.Add(new
    {
        function.CurrentName, BodyGraphOperationCount = operations.Length,
        FirstBlockerDiagnostics = selectedDiagnostics.Where(diagnostic => diagnostic.Function == function.CurrentName).Select(diagnostic => new
        { diagnostic.Family, diagnostic.Message, diagnostic.Locator }).ToArray(),
        SignatureParameters = function.Signature.Parameters.Where(parameter => TypeContainsVolatile(parameter.Type))
            .Select(parameter => new { parameter.Ordinal, Role = parameter.Role.Name, Type = TypeFacts(parameter.Type) }).ToArray(),
        SignatureResult = TypeFacts(function.ReturnType),
        VolatileLocalCatalog = body.Locals.Where(local => TypeContainsVolatile(local.Type)).Select(local => new
        { local.Name, Type = TypeFacts(local.Type), Storage = local.SymbolDetails.Storage?.Name,
          InitializationKind = local.Initializer?.Kind.Name, HasConstantInitializer = local.ConstantInitializer is not null }).ToArray(),
        VolatileBodyGraphExpressions = volatileExpressions.Select(expression => new
        {
            Kind = expression is OperatorOperation operation ? operation.OperatorKind.Name : expression.Kind.Name,
            expression.Details.VolatileFetch, ValueCategory = expression.ValueCategory.Name,
            TypeContainsVolatileQualification = TypeContainsVolatile(expression.Type),
            ObjectTypeIsVolatileQualified = IsVolatileObjectType(expression.Type), Type = TypeFacts(expression.Type),
            ReferencedName = (expression as ReferenceOperation)?.Target?.Name,
            Operands = expression.Operands.Select(OperandFacts).ToArray(),
            DirectOperandConsumers = operators.Where(operation => operation.Operands.Contains(expression)).Select(operation => new
            {
                Kind = operation.OperatorKind.Name, RetainedStoreFamily = IsStoreFamily(operation.OperatorKind.Name),
                operation.Operator.EvalLeftToRight, operation.Operator.EvalRightToLeft,
                OperandIndices = operation.Operands.Select((operand, index) => (operand, index)).Where(pair => pair.operand.Equals(expression)).Select(pair => pair.index).ToArray(),
                Result = TypeFacts(operation.Type)
            }).ToArray()
        }).ToArray(),
        StoreFamiliesWithVolatileRelatedOperand = operators.Where(operation => IsStoreFamily(operation.OperatorKind.Name) && operation.Operands.Any(operand =>
            operand.Details.VolatileFetch || TypeContainsVolatile(operand.Type))).Select(OperationFacts).ToArray(),
        CompoundOperations = operators.Where(operation => compoundKinds.Contains(operation.OperatorKind.Name, StringComparer.Ordinal)).Select(OperationFacts).ToArray(),
        VariadicCalls = operations.OfType<CallOperation>().Where(call =>
            ((call.Callee as ReferenceOperation)?.Target as FunctionSymbol ?? call.TargetMethod)?.Signature.IsVariadic == true).Select(call =>
        {
            var selected = ((call.Callee as ReferenceOperation)?.Target as FunctionSymbol ?? call.TargetMethod)!;
            return new
            {
                TargetName = selected.CurrentName, Builtin = selected.BuiltinKind?.Name,
                TargetsAgree = call.TargetMethod is null || call.TargetMethod.Equals(selected),
                call.Operator.EvalLeftToRight, call.Operator.EvalRightToLeft,
                HasReceiver = call.Receiver is not null, HasReturnSlot = call.Call.ReturnSlot is not null,
                Result = TypeFacts(call.Type),
                Signature = new
                {
                    selected.Signature.HasPrototype, selected.Signature.IsVariadic,
                    selected.Signature.HasThisParameter, selected.Signature.ReturnsValueAsParameter,
                    selected.Signature.ReturnsValueViaCopyConstructor,
                    CallingConvention = selected.Signature.CallingConvention?.Name,
                    Result = TypeFacts(selected.ReturnType),
                    Parameters = selected.Signature.Parameters.Select(parameter => new
                    { parameter.Ordinal, Role = parameter.Role.Name, parameter.Descriptor.Details.PassedViaCopyConstructor, Type = TypeFacts(parameter.Type) }).ToArray()
                },
                Arguments = call.Arguments.Select(argument => new
                { argument.Ordinal, Role = argument.Role?.Name, Value = OperandFacts(argument.Value) }).ToArray()
            };
        }).ToArray(),
        Integer128LocalCatalog = locals128.Select(local => new
        {
            local.Name, Type = TypeFacts(local.Type), Storage = local.SymbolDetails.Storage?.Name,
            BodyGraphReferenceOccurrences = references128.Count(reference => reference.Target!.Equals(local)),
            HasInitializerRecipe = local.Initializer is not null, HasConstantInitializer = local.ConstantInitializer is not null
        }).ToArray(),
        Integer128Operations = is128Body ? operators.Where(operation => IsInteger128(operation.Type) ||
            operation.Operands.Any(operand => IsInteger128(operand.Type) || Contains128Reference(operand))).Select(OperationFacts).ToArray() : [],
        Integer128BodyConstants = is128Body ? operations.OfType<ConstantOperation>().Where(constant => constant.Constant.Value is IntegerValue)
            .Select(constant => new { Type = TypeFacts(constant.Type), Integer = ((IntegerValue)constant.Constant.Value).Value.ToString(CultureInfo.InvariantCulture) }).ToArray() : [],
        Integer128AddressConsumers = is128Body ? operators.Where(operation =>
            operation.OperatorKind.Name is "eok_address_of" or "eok_reference_to" && operation.Operands.Any(Contains128Reference)).Select(address => new
        {
            Address = OperationFacts(address),
            DirectOperandConsumers = operators.Where(operation => operation.Operands.Any(operand => operand.Equals(address))).Select(operation => new
            { Kind = operation.OperatorKind.Name, Type = TypeFacts(operation.Type), Target = (operation as CallOperation)?.TargetMethod?.CurrentName }).ToArray()
        }).ToArray() : []
    });
}
var output = Path.GetFullPath(args[1]);
Directory.CreateDirectory(Path.GetDirectoryName(output)!);
using (var stream = new FileStream(output, FileMode.CreateNew, FileAccess.Write)) JsonSerializer.Serialize(stream, new
{
    InventoryOnly = true, ExecutionOrderAccepted = false, ReachabilityAccepted = false,
    Selection = diagnosticReport is null ? "original-twenty-body-selection" : "exact-first-blocker-function-selection",
    DiagnosticReport = diagnosticReport,
    DiagnosticReportSha256 = diagnosticReport is null ? null : Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(diagnosticReport))),
    RepresentationDiagnostics = representationDiagnostics.Select(diagnostic => new
    { diagnostic.Family, diagnostic.Message, diagnostic.Locator, ExecutionBodyAssigned = false }).ToArray(),
    Artifact = input.Manifest.ArtifactIdentity, SelectedBodyNames = selectedNames, Functions = rows, ReaderStatistics = input.Statistics
}, new JsonSerializerOptions { WriteIndented = true });
Console.WriteLine($"selected-bodies={rows.Count}; output={output}");

static bool IsInteger128(CppType type)
{
    while (type is TypeReference reference) type = reference.UnderlyingType;
    return type is ArithmeticType arithmetic && arithmetic.Arithmetic.Bits == 128;
}
static bool IsStoreFamily(string kind) => kind is "eok_assign" or "eok_bassign" or "eok_add_assign" or "eok_subtract_assign" or
    "eok_multiply_assign" or "eok_divide_assign" or "eok_remainder_assign" or "eok_shiftl_assign" or "eok_shiftr_assign" or
    "eok_and_assign" or "eok_or_assign" or "eok_xor_assign" or "eok_padd_assign" or "eok_psubtract_assign" or
    "eok_pre_incr" or "eok_pre_decr" or "eok_post_incr" or "eok_post_decr";
static bool IsVolatileObjectType(CppType type)
{
    var seen = new HashSet<CppType>();
    while (seen.Add(type))
    {
        if (type is QualifiedType { Qualification.IsVolatile: true }) return true;
        if (type is not TypeReference reference) return false;
        type = reference.UnderlyingType;
    }
    return false;
}
static bool TypeContainsVolatile(CppType type, int depth = 0)
{
    if (IsVolatileObjectType(type)) return true;
    var seen = new HashSet<CppType>();
    while (type is TypeReference reference && seen.Add(type)) type = reference.UnderlyingType;
    return depth < 2 && (type is PointerType pointer && TypeContainsVolatile(pointer.PointeeType, depth + 1) ||
        type is ArrayType array && TypeContainsVolatile(array.ElementType, depth + 1));
}
static object OperationFacts(OperatorOperation operation) => new
{
    Kind = operation.OperatorKind.Name, Result = TypeFacts(operation.Type), ValueCategory = operation.ValueCategory.Name,
    operation.Operator.EvalLeftToRight, operation.Operator.EvalRightToLeft, operation.Operator.PointerOperandIsSecond,
    OperationTypeKind = operation.Operator.OperationTypeKind?.Name, operation.Details.VolatileFetch,
    FirstOperandObjectIsVolatileQualified = operation.Operands.Count > 0 && IsVolatileObjectType(operation.Operands[0].Type),
    Operands = operation.Operands.Select(OperandFacts).ToArray()
};
static object OperandFacts(ExpressionOperation operand) => new
{
    Kind = operand is OperatorOperation operation ? operation.OperatorKind.Name : operand.Kind.Name,
    Type = TypeFacts(operand.Type), ValueCategory = operand.ValueCategory.Name, operand.Details.VolatileFetch,
    ReferencedName = (operand as ReferenceOperation)?.Target?.Name,
    ReferencedDeclarationType = operand is ReferenceOperation { Target: VariableSymbol variable } ? TypeFacts(variable.Type) : null,
    ReferencedField = FieldFacts(operand),
    Integer = operand is ConstantOperation { Constant.Value: IntegerValue integer } ? integer.Value.ToString(CultureInfo.InvariantCulture) : null
};
static object? FieldFacts(ExpressionOperation expression)
{
    if (expression is not ReferenceOperation { Target: FieldSymbol field }) return null;
    var layout = field.ContainingType.Layout?.Fields.FirstOrDefault(entry => entry.Field.Equals(field));
    return new
    {
        field.Name, Type = TypeFacts(field.Type), ContainingType = Dimensions(field.ContainingType),
        Offset = layout?.Offset?.ToString(CultureInfo.InvariantCulture),
        BitOffset = layout?.BitOffset?.ToString(CultureInfo.InvariantCulture),
        BitWidth = layout?.BitWidth?.ToString(CultureInfo.InvariantCulture)
    };
}
static object TypeFacts(CppType type, int pointerDepth = 0)
{
    var raw = type;
    var wrappers = new List<object>();
    var seen = new HashSet<CppType>();
    while (raw is TypeReference reference && seen.Add(raw))
    {
        wrappers.Add(new
        {
            Dimensions = Dimensions(raw), raw.Details.IsAtomic,
            Qualifiers = raw is QualifiedType qualified ? new
            { qualified.Qualification.IsConst, qualified.Qualification.IsVolatile, qualified.Qualification.IsAtomic,
              qualified.Qualification.AddressSpace, qualified.Qualification.IsUnaligned, qualified.Qualification.IsShared } : null
        });
        raw = reference.UnderlyingType;
    }
    return new
    {
        Effective = Dimensions(type), Wrappers = wrappers, Raw = Dimensions(raw), raw.IsIncomplete, raw.Details.IsAtomic,
        Arithmetic = raw is ArithmeticType arithmetic ? new
        { Kind = arithmetic.Arithmetic.ArithmeticKind.Name, arithmetic.Arithmetic.Bits, arithmetic.Arithmetic.Signed, arithmetic.Arithmetic.IsBool,
          IntegerKind = arithmetic.Arithmetic.EdgIntegerKind?.Name } : null,
        Pointee = pointerDepth < 2 && raw is PointerType pointer ? TypeFacts(pointer.PointeeType, pointerDepth + 1) : null,
        Element = pointerDepth < 2 && raw is ArrayType array ? TypeFacts(array.ElementType, pointerDepth + 1) : null
    };
}
static object Dimensions(CppType type) => new
{
    Kind = type.Kind.Name, Size = type.Size?.ToString(CultureInfo.InvariantCulture), Alignment = type.Alignment?.ToString(CultureInfo.InvariantCulture),
    DescriptorSize = type.Details.DescriptorSize?.ToString(CultureInfo.InvariantCulture),
    DescriptorAlignment = type.Details.DescriptorAlignment?.ToString(CultureInfo.InvariantCulture)
};
