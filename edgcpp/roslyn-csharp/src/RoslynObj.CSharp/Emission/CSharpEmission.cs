using System.Globalization;
using System.Numerics;
using System.Security;
using System.Text;
using RoslynObj;

namespace RoslynObj.CSharp;

public sealed record CSharpEmissionOptions
{
    public string NamespaceName { get; init; } = "Translated";
    public string ClassName { get; init; } = "Program";
    public bool Executable { get; init; }
    public string? EntryPointName { get; init; }
    /// <summary>Acceptance harness only: print the selected entry value and exit zero.</summary>
    public bool PrintEntryResult { get; init; }
    public string RuntimeProjectPath { get; init; } = "";
    public IReadOnlyList<CSharpExternalFunctionBinding> ExternalFunctions { get; init; } = [];
    public NativeErrorProtocol NativeErrorProtocol { get; init; }
    /// <summary>Opt in to bounded selected-program assembly. Null keeps legacy single-unit admission.</summary>
    public ProgramAssemblyOptions? ProgramAssembly { get; init; }
}

public sealed record CSharpEmissionResult(
    IReadOnlyList<GeneratedSource> Files,
    IReadOnlyList<TranslationDiagnostic> Diagnostics,
    int EmittedFunctionCount,
    int EmittedRecordCount);

/// <summary>Projects public, already-resolved EDG objects to ordinary unsafe C#.</summary>
public sealed partial class CSharpEmitter
{
    public CSharpEmissionResult Emit(IReadOnlyList<RoslynObject> inputs, CSharpEmissionOptions options)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(options);
        return new Emission(inputs, options).Run();
    }

    private sealed partial class Emission(IReadOnlyList<RoslynObject> inputs, CSharpEmissionOptions options)
    {
        private readonly List<TranslationDiagnostic> diagnostics = [];
        private readonly Dictionary<SemanticEntity, string> names = [];
        private readonly Dictionary<SemanticEntity, int> ordinals = [];
        private readonly HashSet<string> globalNames = new(StringComparer.Ordinal);
        private readonly List<RecordType> records = [];
        private readonly HashSet<CppType> planned = [];
        private readonly List<FunctionSymbol> functions = [];
        private readonly StringBuilder methods = new();
        private FunctionSymbol? currentFunction;
        private int functionOrdinal;
        private int operationOrdinal;

        public CSharpEmissionResult Run()
        {
            if (options.ProgramAssembly is { } assemblyOptions)
            {
                if (!InitializeProgramAssembly(assemblyOptions)) return new([], diagnostics, 0, 0);
            }
            else if (inputs.Count != 1)
                return Failure("CSHARP2001", "Multiple translation units require explicit bounded program assembly options.");
            globalNames.Add(options.ClassName);
            if (options.Executable) globalNames.Add("Main");
            if (!ValidateNativeErrorProtocol()) return new([], diagnostics, 0, 0);
            if (string.IsNullOrWhiteSpace(options.RuntimeProjectPath))
                return Failure("CSHARP2005", "An explicit generated-code runtime project path is required.");
            for (var unit = 0; unit < inputs.Count; ++unit)
            {
                var input = inputs[unit];
                var target = input.Manifest.Target;
                if (target.Abi != "itanium" || target.AddressableUnitBits != 8 || target.PointerBits != 64 || target.PointerAlignment != 8 || target.Endianness != "little")
                    diagnostics.Add(new("CSHARP2002", DiagnosticSeverity.Error, "Emission requires the captured 8-bit-unit, little-endian 64-bit pointer profile.", input.Manifest.ArtifactIdentity, $"unit/{unit + 1}", "profile"));
                if (input.TranslationUnit.FileScopeInitializationRoutines.Count != 0 || input.TranslationUnit.GlobalScope.DynamicInitializations.Count != 0 ||
                    input.TranslationUnit.Roots.Any(r => r.Kind.Name is "file_scope_statement" or "file_initializer") ||
                    input.TranslationUnit.GlobalScope.Lifetime?.Cleanups.Count > 0)
                    diagnostics.Add(new("CSHARP2006", DiagnosticSeverity.Error, "The unit has file initialization or teardown obligations; no initialization schedule is implemented.", input.Manifest.ArtifactIdentity, $"unit/{unit + 1}", "initialization"));
            }
            foreach (var variable in SelectedStorage())
            {
                try { AdmitGlobal(variable); }
                catch (Unsupported ex) { Add("CSHARP2007", ex.Message, variable.Name, ex.Family); }
            }
            foreach (var function in SelectedFunctions())
            {
                if (function.BodyState.Name != "definition")
                {
                    if (function.BodyState.Name == "unavailable" && function.IsDefined && !function.Details.IsDeleted && !function.Details.IsConsteval)
                        Add("CSHARP2008", "A defined function has an unavailable execution body.", function.Name, "function");
                    continue;
                }
                if (function.Details.IsInitializationRoutine == true || function.Details.IsFinalizationRoutine == true)
                    Add("CSHARP2009", "Captured initialization/finalization routines require program execution order planning.", function.Name, "initialization");
                if (function.Body is null)
                {
                    Add("CSHARP2003", "An executable definition has no retained body.", function.Name, "function");
                    continue;
                }
                functions.Add(function);
                Name(function, function.Name ?? "function", globalNames);
                ordinals[function] = functions.Count;
            }
            foreach (var function in functions)
            {
                currentFunction = function;
                functionOrdinal = ordinals[function];
                operationOrdinal = 0;
                try { EmitFunction(function); }
                catch (Unsupported ex) { Add("CSHARP2100", ex.Message, function.Name, ex.Family); }
            }
            currentFunction = null;
            var staticFields = new StringBuilder();
            var staticInitialization = new CodeWriter();
            try { EmitGlobalStorage(staticFields, staticInitialization); }
            catch (Unsupported ex) { Add("CSHARP2300", ex.Message, null, ex.Family); }
            try { EmitVariadicSupport(); }
            catch (Unsupported ex) { Add("CSHARP2401", ex.Message, null, ex.Family); }
            try { EmitCallSupport(staticFields, staticInitialization); }
            catch (Unsupported ex) { Add("CSHARP2400", ex.Message, null, ex.Family); }
            var declarations = new StringBuilder();
            // Either kind can discover the other (record -> array -> record).
            // Drain one joint worklist before publishing any declarations.
            var recordIndex = 0;
            var arrayIndex = 0;
            while (recordIndex < records.Count || arrayIndex < arrays.Count)
            {
                if (recordIndex < records.Count)
                {
                    var record = records[recordIndex++];
                    try { EmitRecord(record, declarations); }
                    catch (Unsupported ex) { Add("CSHARP2200", ex.Message, record.Name, ex.Family); }
                }
                else
                {
                    var array = arrays[arrayIndex++];
                    try { EmitArray(array, declarations); }
                    catch (Unsupported ex) { Add("CSHARP2200", ex.Message, array.Name, ex.Family); }
                }
            }
            var entry = functions.Where(f => f.Name == options.EntryPointName).ToArray();
            if (options.Executable && (entry.Length != 1 || entry[0].Parameters.Count != 0 || TypeName(entry[0].ReturnType) != "int"))
                Add("CSHARP2004", "Executable mode requires one selected entry with no parameters and a captured signed 32-bit result.", options.EntryPointName, "entry");
            if (diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error)) return new([], diagnostics, 0, 0);
            var source = new StringBuilder("// Generated from public RoslynObj executable relationships.\nusing System;\nusing System.Runtime.CompilerServices;\nusing System.Runtime.InteropServices;\nusing RoslynObj.CSharp.Runtime;\n\n");
            source.Append("namespace ").Append(options.NamespaceName).AppendLine(";").AppendLine();
            source.Append(declarations);
            source.Append("public static unsafe class ").Append(options.ClassName).AppendLine("\n{");
            source.Append(staticFields);
            source.Append("    static ").Append(options.ClassName).AppendLine("()\n    {\n        RuntimeContract.RequireLayout(8, true);");
            if (variadicParameters.Count != 0 || variadicCalls.Count != 0)
                source.Append("        ").Append(VaRuntime).AppendLine(".RequireAbi();");
            source.Append(staticInitialization.TextWithIndent(2));
            source.AppendLine("    }").AppendLine();
            source.Append(methods);
            if (options.Executable)
            {
                source.AppendLine("    public static int Main()").AppendLine("    {");
                if (options.PrintEntryResult)
                {
                    source.Append("        Console.WriteLine(").Append(names[entry[0]]).AppendLine("());");
                    source.AppendLine("        return 0;");
                }
                else source.Append("        return ").Append(names[entry[0]]).AppendLine("();");
                source.AppendLine("    }");
            }
            source.AppendLine("}");
            var runtime = SecurityElement.Escape(Path.GetFullPath(options.RuntimeProjectPath));
            var project = $"<Project Sdk=\"Microsoft.NET.Sdk\">\n  <PropertyGroup>\n    <TargetFramework>net10.0</TargetFramework>\n    <LangVersion>14</LangVersion>\n    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>\n    <Nullable>enable</Nullable>\n    <OutputType>{(options.Executable ? "Exe" : "Library")}</OutputType>\n    <CheckForOverflowUnderflow>false</CheckForOverflowUnderflow>\n  </PropertyGroup>\n  <ItemGroup><ProjectReference Include=\"{runtime}\" /></ItemGroup>\n{NativeAbiProjectItems()}</Project>\n";
            var files = new List<GeneratedSource> { new("Generated.cs", source.ToString()), new("Generated.csproj", project) };
            if (generatedAbiLibrary is not null) files.Add(new("Generated.Abi.c", NativeAbiSource()));
            if (nativeImports.Count > 0 || generatedAbiLibrary is not null) files.Add(new("Generated.NativeImports.json", NativeImportReport()));
            if (selectedProgram is not null) files.Add(new("Generated.ProgramAssembly.json", ProgramAssemblyReport()));
            return new(files, diagnostics, functions.Count, records.Count);
        }

        private CSharpEmissionResult Failure(string code, string message)
        {
            Add(code, message, null, "profile");
            return new([], diagnostics, 0, 0);
        }

        private void Add(string code, string message, string? name, string family) => diagnostics.Add(new(code, DiagnosticSeverity.Error,
            message, CurrentArtifactIdentity(),
            $"function/{functionOrdinal}:{name ?? currentFunction?.Name ?? "program"}/operation/{operationOrdinal}", family));

        private void EmitFunction(FunctionSymbol function)
        {
            ValidateOrdinaryRoutineRoles(function.Signature);
            if (IsUInt128Value(function.ReturnType) || function.Parameters.Any(parameter => IsUInt128Value(parameter.Type)))
                throw Limit("Unsigned 128-bit execution parameters/results require a separately exercised call representation.", "integer128-call");
            if (function.Parameters.Any(p => p.Role.Name != "ordinary_parameter"))
                throw Limit("An execution parameter has a hidden role requiring an exercised ABI adapter.", "parameter");
            if (function.Signature.IsVariadic) ValidateNativeScalarSignature(function.Signature);
            var body = function.Body!;
            if (body.ExceptionHandling.CleanupRegions.Count != 0 || body.Lifetimes.Any(l => l.Cleanups.Count != 0))
                throw Limit("This body has retained lifetime cleanup obligations; T1 does not implement C++ cleanup.", "lifetime");
            var localNames = new HashSet<string>(StringComparer.Ordinal);
            foreach (var parameter in function.Parameters)
            {
                if (parameter.Symbol is null) throw Limit("A selected execution parameter has no retained variable symbol.", "parameter");
                Name(parameter.Symbol, parameter.Name ?? "argument", localNames);
            }
            foreach (var variable in body.Locals)
            {
                if (TryAdmitStaticLocal(variable)) continue;
                if (variable.IsThreadLocal || variable.LocalStaticInitializations.Count != 0 || variable.Details.StorageDuration?.Name is "static" or "thread")
                    throw Limit("Static/thread local storage requires the T2 initialization contract.", "storage");
                Name(variable, variable.Name ?? "temporary", localNames);
            }
            continuedLabels.Clear();
            retainedLabels.Clear();
            referencedContinuations.Clear();
            var flatStatements = body.Root is StatementOperation root && NeedsFlatStatements(root);
            targetStatements = flatStatements;
            if (flatStatements) PrepareFlatStatements((StatementOperation)body.Root, localNames);
            else if (body.Root is StatementOperation rootStatement) CollectContinueTargets(rootStatement, localNames);
            if (continuedLabels.Any(label => !retainedLabels.Contains(label)))
                throw Limit("A continue has no retained target label in this execution body.", "loop");
            var writer = new CodeWriter();
            var parameters = function.Parameters.Select(p => $"{TypeName(p.Type)} {names[p.Symbol!]}").ToList();
            if (function.Signature.IsVariadic)
            {
                var argumentPack = "__arguments";
                while (!localNames.Add(argumentPack)) argumentPack += "_";
                variadicParameters[function] = argumentPack;
                parameters.Add($"{VaType}* {argumentPack}");
            }
            writer.Line($"public static {TypeName(function.ReturnType)} {names[function]}({string.Join(", ", parameters)})");
            writer.Open();
            foreach (var variable in body.Locals)
            {
                if (IsAllocatedStaticLocal(variable)) continue;
                writer.Line($"{TypeName(variable.Type)} {names[variable]};");
                if (MemoryRepresentationType(variable.Type) is PointerType) writer.Line($"{names[variable]} = {PointerZero(variable.Type)};");
                else writer.Line($"Unsafe.SkipInit(out {names[variable]});");
            }
            if (body.Locals.Count > 0) writer.Line();
            if (flatStatements) FlatStatement((StatementOperation)body.Root, writer);
            else if (body.Root is BlockOperation block) foreach (var statement in block.Statements) Statement(statement, writer);
            else if (body.Root is StatementOperation statement) Statement(statement, writer);
            else throw Limit($"Body root '{body.Root.Kind}' has no T1 statement projection.", body.Root.Kind.Name);
            writer.Close();
            methods.Append(writer.TextWithIndent(1)).AppendLine();
        }

        private void Statement(StatementOperation statement, CodeWriter writer)
        {
            ++operationOrdinal;
            if (TryTargetStatement(statement, writer)) return;
            switch (statement)
            {
                case BlockOperation block:
                    writer.Open();
                    foreach (var child in block.Statements) Statement(child, writer);
                    writer.Close();
                    return;
                case DeclarationOperation declaration:
                    // Storage declarations are emitted at method scope. Initializers execute only
                    // at their explicit retained statement/expression occurrences.
                    ValidateStaticDeclaration(declaration);
                    return;
                case InitializationStatementOperation initialization:
                    Initialization(initialization.Initialization, writer);
                    return;
                case ConditionalOperation conditional:
                    writer.Line($"if ({ConditionOccurrence(Required(conditional.Condition), writer)})");
                    Embedded(conditional.Then, writer);
                    if (conditional.Else is not null) { writer.Line("else"); Embedded(conditional.Else, writer); }
                    return;
                case LoopOperation loop:
                    EmitLoop(loop, writer);
                    return;
                case LabelOperation label:
                    if (continuedLabels.Contains(label.Label)) writer.Line(names[label.Label] + ":;");
                    return;
                case JumpOperation jump:
                    switch (jump.JumpKind.Name)
                    {
                        case "return":
                            if (jump.Value is null) writer.Line("return;");
                            else if (Unwrap(jump.Value.Type).Kind.Name == "tk_void") { ExpressionStatementOccurrence(jump.Value, writer); writer.Line("return;"); }
                            else writer.Line($"return {ValueOccurrence(jump.Value, writer)};");
                            return;
                        case "break": writer.Line("break;"); return;
                        case "continue":
                            if (jump.Target is { } target) writer.Line("goto " + names[target] + ";");
                            else if (loopContinueLabels.TryPeek(out var continuation) && continuation is not null)
                            {
                                referencedContinuations.Add(continuation); writer.Line("goto " + continuation + ";");
                            }
                            else writer.Line("continue;");
                            return;
                        default: throw Limit($"Jump '{jump.JumpKind}' requires label/cleanup handling.", "jump");
                    }
                default:
                    if (statement.Kind.Name == "stmk_empty") { writer.Line(";"); return; }
                    if (statement.Kind.Name == "stmk_expr" && statement.Expression is { } expression)
                    {
                        ExpressionStatementOccurrence(expression, writer);
                        return;
                    }
                    throw Limit($"Statement '{statement.Kind}' has no T1 projection.", statement.Kind.Name);
            }
        }

        private void Embedded(StatementOperation? statement, CodeWriter writer)
        {
            if (targetStatements) { writer.Open(); if (statement is not null) Statement(statement, writer); writer.Close(); }
            else if (statement is BlockOperation block) Statement(block, writer);
            else { writer.Open(); if (statement is not null) Statement(statement, writer); writer.Close(); }
        }

        private void Initialization(Initialization initialization, CodeWriter writer)
        {
            if (TryStaticInitialization(initialization)) return;
            if (initialization.Kind.Name is not ("dik_expression" or "dik_constant" or "dik_bitwise_copy") ||
                initialization.Details.IsConditional || initialization.Details.IsReusedValue ||
                initialization.Details.IsPartialAggregate && (initialization.Kind.Name != "dik_constant" || initialization.Constant is null))
                throw Limit($"Initialization '{initialization.Kind}' requires a richer storage/execution contract.", "initialization");
            if (initialization.Constructor is not null || initialization.Destructor is not null)
                throw Limit("Constructor/destructor initialization is unsupported in T1.", "initialization");
            if (initialization.Target is not VariableSymbol variable || !names.ContainsKey(variable))
                throw Limit("Initialization target is not selected local storage.", "initialization");
            if (TryEmitVolatileLocalInitialization(initialization, variable, writer)) return;
            if (initialization.Kind.Name == "dik_bitwise_copy")
            {
                var source = initialization.BitwiseCopySource ?? throw Limit("A bitwise copy has no retained source.", "initialization");
                writer.Line($"TargetStorage.Copy(ref {names[variable]}, ref {ValueOccurrence(source, writer)}, {StorageSize(variable.Type)});");
                return;
            }
            if (Unwrap(variable.Type) is RecordType or ArrayType && initialization.Constant is { } aggregate)
            {
                writer.Line($"NativeMemory.Clear(&{names[variable]}, {StorageSize(variable.Type)});");
                WriteConstant(variable.Type, $"(byte*)&{names[variable]}", aggregate.Value, writer, false);
                return;
            }
            var value = initialization.Expression is ExpressionOperation expression ? ValueOccurrence(expression, writer)
                : initialization.Constant is { } constant ? Constant(constant)
                : throw Limit($"Initialization '{initialization.Kind}' has no scalar value.", "initialization");
            writer.Line($"{names[variable]} = {value};");
        }

        private string Expression(ExpressionOperation expression)
        {
            ++operationOrdinal;
            if (expression.Details.VolatileFetch) throw Limit("Volatile fetches require a target memory-order projection.", "volatile");
            return expression switch
            {
                ConstantOperation constant => Constant(constant.Constant),
                ReferenceOperation reference => Reference(reference),
                CallOperation call => Call(call),
                ConversionOperation conversion => Conversion(conversion),
                OperatorOperation operation => Operator(operation),
                LifetimeOperation lifetime when lifetime.Lifetime is null || lifetime.Lifetime.Cleanups.Count == 0 => Expression(lifetime.Value),
                ConditionOperation condition when condition.Initialization is null && condition.DynamicInitialization is null => Expression(Required(condition.Value)),
                _ => throw Limit($"Expression '{expression.Kind}' has no T1 projection.", expression.Kind.Name)
            };
        }

        private string Reference(ReferenceOperation reference)
        {
            if (TryProgramReference(reference, out var programReference)) return programReference;
            if (reference.Target is FunctionSymbol function) return FunctionAddress(function);
            if (reference.Target is FieldSymbol) throw Limit("A field reference requires its actual retained object operand.", "field");
            if (reference.Target is { } target && names.TryGetValue(target, out var name)) return storage.ContainsKey(target) ? $"(*{OwnedName(target)})" : name;
            throw Limit($"Reference '{reference.Target?.Name}' has no selected function/local representation; globals and imports require T2.", "reference");
        }

        private string Call(CallOperation call)
        {
            return EmitCall(call);
        }

        private string Conversion(ConversionOperation conversion)
        {
            var kind = conversion.OperatorKind.Name;
            if (conversion.Operator.RequiresRuntimeCastCheck || kind is "eok_dynamic_cast" or "eok_ref_dynamic_cast" or "eok_base_class_cast" or "eok_derived_class_cast")
                throw Limit($"Conversion '{kind}' needs a C++ object-model representation.", "conversion");
            var value = Expression(conversion.Operand);
            if (kind == "eok_bool_cast") return BoolValue(TruthValue(value, conversion.Operand.Type), conversion.Type);
            if (kind is "eok_lvalue_adjust" or "eok_class_rvalue_adjust")
            {
                if (TypeName(conversion.SourceType) != TypeName(conversion.TargetType)) throw Limit("A lvalue adjustment changes the storage representation.", "conversion");
                return value;
            }
            if (kind == "eok_array_to_pointer") return $"(({TypeName(conversion.TargetType)})&({value}))";
            if (kind is not ("eok_cast" or "eok_lvalue_cast" or "eok_ref_cast")) throw Limit($"Conversion '{kind}' is unsupported.", "conversion");
            return $"unchecked(({TypeName(conversion.TargetType)})({value}))";
        }

        private string Operator(OperatorOperation operation)
        {
            var kind = operation.OperatorKind.Name;
            var operands = operation.Operands;
            string A(int index) => Expression(operands[index]);
            if ((AssignmentOperators.ContainsKey(kind) || kind is "eok_pre_incr" or "eok_pre_decr" or "eok_post_incr" or "eok_post_decr") &&
                operands.Count > 0 && IsBitFieldPlace(operands[0]) && !operation.Details.ResultUnused)
                throw Limit("A value-used bitfield store/update needs a normalized stored-value plan.", "bitfield");
            if (operation.Operator.EvalRightToLeft && operands.Count > 1 && operands.Any(HasEffects))
                throw Limit($"Operator '{kind}' has effectful right-to-left operands and requires explicit sequencing temporaries.", "sequencing");
            switch (kind)
            {
                case "eok_va_start": case "eok_va_arg": case "eok_va_copy": case "eok_va_end": return VariadicOperator(operation);
                case "eok_parens": return $"({A(0)})";
                case "eok_address_of": case "eok_reference_to":
                    if (operands[0] is ReferenceOperation { Target: FunctionSymbol addressed }) return FunctionAddress(addressed);
                    ValidateValueAddress(operands[0].Type);
                    return $"&({A(0)})";
                case "eok_indirect": case "eok_ref_indirect": return $"(*({A(0)}))";
                case "eok_dot_field": case "eok_points_to_field":
                    if (operands[1] is not ReferenceOperation { Target: FieldSymbol field })
                        throw Limit("A member access requires the selected field reference operand.", "field");
                    return $"({A(0)}){(kind == "eok_dot_field" ? "." : "->")}{FieldName(field)}";
                case "eok_subscript":
                    if (operation.Operator.PointerOperandIsSecond)
                    {
                        if (operands.Any(HasEffects)) throw Limit("An effectful pointer-second subscript requires explicit sequencing temporaries.", "sequencing");
                        return $"({A(1)})[{A(0)}]";
                    }
                    return $"({A(0)})[{A(1)}]";
                case "eok_padd": case "eok_psubtract": case "eok_pdiff":
                    if (operation.Operator.PointerOperandIsSecond)
                    {
                        if (operands.Any(HasEffects)) throw Limit("Effectful pointer-second arithmetic requires explicit sequencing temporaries.", "sequencing");
                        return $"(({A(1)}) + ({A(0)}))";
                    }
                    return $"(({A(0)}) {(kind == "eok_padd" ? "+" : "-")} ({A(1)}))";
                case "eok_question":
                    if (operands.Any(HasEffects)) throw Limit("Effectful conditional branches require structured expression planning.", "sequencing");
                    return $"({Truth(operands[0])} ? {A(1)} : {A(2)})";
                case "eok_bassign":
                    return $"TargetStorage.Copy(ref {A(0)}, ref {A(1)}, {StorageSize(operands[0].Type)})";
                case "eok_not": return BoolValue($"!({Truth(operands[0])})", operation.Type);
                case "eok_negate": return $"unchecked(({TypeName(operation.Type)})(-({A(0)})))";
                case "eok_unary_plus": return $"unchecked(({TypeName(operation.Type)})(+({A(0)})))";
                case "eok_complement": return $"unchecked(({TypeName(operation.Type)})(~({A(0)})))";
                case "eok_pre_incr": return $"++{A(0)}";
                case "eok_pre_decr": return $"--{A(0)}";
                case "eok_post_incr": return $"{A(0)}++";
                case "eok_post_decr": return $"{A(0)}--";
                case "eok_land": return BoolValue($"({Truth(operands[0])}) && ({Truth(operands[1])})", operation.Type);
                case "eok_lor": return BoolValue($"({Truth(operands[0])}) || ({Truth(operands[1])})", operation.Type);
                case "eok_shiftl": case "eok_shiftr":
                    return $"TargetArithmetic.{(kind == "eok_shiftl" ? "ShiftLeft" : "ShiftRight")}({A(0)}, checked((int)({A(1)})))";
            }
            if (ComparisonOperators.TryGetValue(kind, out var comparison)) return BoolValue($"({A(0)}) {comparison} ({A(1)})", operation.Type);
            if (AssignmentOperators.TryGetValue(kind, out var assignment))
            {
                if (Unwrap(operands[0].Type) is RecordType or ArrayType)
                    return $"TargetStorage.Copy(ref {A(0)}, ref {A(1)}, {StorageSize(operands[0].Type)})";
                if (kind != "eok_assign" && TypeName(operands[0].Type) is "sbyte" or "byte" or "short" or "ushort" && TypeName(operands[1].Type) == "int")
                {
                    if (operands.Any(HasEffects)) throw Limit("Effectful narrow compound operands require explicit address/load/store planning.", "sequencing");
                    if (operands[0] is OperatorOperation member && member.Operands.LastOrDefault() is ReferenceOperation { Target: FieldSymbol { Details.IsBitField: true } })
                        throw Limit("A narrow bitfield compound operation needs a typed object setter helper.", "bitfield");
                    var helper = kind switch
                    {
                        "eok_add_assign" => "Add", "eok_subtract_assign" => "Subtract", "eok_multiply_assign" => "Multiply", "eok_divide_assign" => "Divide",
                        "eok_remainder_assign" => "Remainder", "eok_and_assign" => "And", "eok_or_assign" => "Or", "eok_xor_assign" => "Xor",
                        _ => throw Limit("This narrow compound operation has no target helper.", "operator")
                    };
                    return $"TargetArithmetic.{helper}Assign(ref {A(0)}, {A(1)})";
                }
                return $"{A(0)} {assignment} {A(1)}";
            }
            if (BinaryOperators.TryGetValue(kind, out var binary))
            {
                var value = $"({A(0)}) {binary} ({A(1)})";
                return TypeName(operation.Type) is "byte" or "sbyte" or "short" or "ushort"
                    ? $"unchecked(({TypeName(operation.Type)})({value}))" : $"unchecked({value})";
            }
            throw Limit($"Operator '{kind}' has no T1 projection.", kind);
        }

        private static readonly Dictionary<string, string> ComparisonOperators = new()
        { ["eok_eq"] = "==", ["eok_ne"] = "!=", ["eok_gt"] = ">", ["eok_lt"] = "<", ["eok_ge"] = ">=", ["eok_le"] = "<=" };
        private static readonly Dictionary<string, string> AssignmentOperators = new()
        { ["eok_assign"] = "=", ["eok_add_assign"] = "+=", ["eok_subtract_assign"] = "-=", ["eok_multiply_assign"] = "*=", ["eok_divide_assign"] = "/=", ["eok_remainder_assign"] = "%=", ["eok_and_assign"] = "&=", ["eok_or_assign"] = "|=", ["eok_xor_assign"] = "^=" };
        private static readonly Dictionary<string, string> BinaryOperators = new()
        { ["eok_add"] = "+", ["eok_subtract"] = "-", ["eok_multiply"] = "*", ["eok_divide"] = "/", ["eok_remainder"] = "%", ["eok_and"] = "&", ["eok_or"] = "|", ["eok_xor"] = "^" };
        private static bool IsStatementExpression(string kind) => AssignmentOperators.ContainsKey(kind) || kind is "eok_pre_incr" or "eok_pre_decr" or "eok_post_incr" or "eok_post_decr" or "eok_bassign" or "eok_va_start" or "eok_va_copy" or "eok_va_end";

        private string Constant(Constant constant)
        {
            if (TryStoredConstant(constant, out var stored)) return stored;
            if (constant.Value is IntegerValue integer)
            {
                var type = constant.Type ?? throw Limit("Integer constant has no recorded type.", "constant");
                if (TryWideIntegerLiteral(integer.Value, type, out var wide)) return wide;
                var name = TypeName(type);
                var value = integer.Value.ToString(CultureInfo.InvariantCulture);
                var suffix = name switch { "uint" => "U", "long" => "L", "ulong" => "UL", _ => "" };
                return name is "int" or "uint" or "long" or "ulong" ? value + suffix : $"({name}){value}";
            }
            if (constant.Value is AddressValue address && constant.Type is { } addressType) return AddressExpression(address, addressType);
            if (constant.Value is FloatingValue floating && constant.Type is { } floatingType) return FloatingLiteral(floating, floatingType);
            throw Limit($"Constant value '{constant.Value.Kind}' has no T1 exact-value projection.", "constant");
        }

        private string Truth(ExpressionOperation expression)
        {
            if (expression is ConversionOperation conversion && conversion.OperatorKind.Name == "eok_bool_cast") return Truth(conversion.Operand);
            if (expression is OperatorOperation operation)
            {
                if (operation.Operator.EvalRightToLeft && operation.Operands.Any(HasEffects))
                    throw Limit("Condition with right-to-left effects requires explicit sequencing temporaries.", "sequencing");
                if (ComparisonOperators.TryGetValue(operation.OperatorKind.Name, out var comparison))
                    return $"({Expression(operation.Operands[0])}) {comparison} ({Expression(operation.Operands[1])})";
                if (operation.OperatorKind.Name is "eok_land" or "eok_lor")
                    return $"({Truth(operation.Operands[0])}) {(operation.OperatorKind.Name == "eok_land" ? "&&" : "||")} ({Truth(operation.Operands[1])})";
                if (operation.OperatorKind.Name == "eok_not") return $"!({Truth(operation.Operands[0])})";
            }
            return TruthValue(Expression(expression), expression.Type);
        }
        private string TruthValue(string value, CppType type) => $"({value}) != {(Unwrap(type) is PointerType ? PointerZero(type) : "0")}";
        private static string PointerZero(CppType type) => MemoryRepresentationType(type) is PointerType pointer && MemoryRepresentationType(pointer.PointeeType) is not RoutineType ? "null" : "0";
        private string BoolValue(string comparison, CppType type) => $"(({comparison}) ? ({TypeName(type)})1 : ({TypeName(type)})0)";

        private string TypeName(CppType type)
        {
            if (IsRawUInt128(type)) { ValidateUInt128Value(type); return "UInt128"; }
            if (TryVolatileStorageRepresentation(type, out var representation)) type = representation;
            type = Unwrap(type);
            if (type.Details.IsAtomic) throw Limit("Atomic storage requires a target atomic projection.", "atomic");
            if (type.Kind.Name == "tk_void") return "void";
            if (type is ArithmeticType arithmetic)
            {
                var detail = arithmetic.Arithmetic;
                if (detail.ArithmeticKind.Name == "floating")
                    return (detail.Bits, type.Size) switch
                    {
                        (32, var size) when size == 4 => "float",
                        (64, var size) when size == 8 => "double",
                        _ => throw Limit("Floating storage is not a supported IEEE binary32/binary64 representation.", "floating")
                    };
                if (detail.ArithmeticKind.Name is not ("signed_integer" or "unsigned_integer" or "character" or "boolean"))
                    throw Limit($"Arithmetic kind '{detail.ArithmeticKind}' has no T1 exact representation.", "type");
                var bits = detail.Bits;
                if (detail.IsBool) bits = type.Size * 8 is { } storageBits ? (long)storageBits : null;
                return (bits, detail.Signed == true) switch
                {
                    (8, true) => "sbyte", (8, false) => "byte", (16, true) => "short", (16, false) => "ushort",
                    (32, true) => "int", (32, false) => "uint", (64, true) => "long", (64, false) => "ulong",
                    _ => throw Limit($"Arithmetic storage width '{bits}' is unsupported.", "type")
                };
            }
            if (type is PointerType pointer)
            {
                if (pointer.IsReference) throw Limit("Reference storage adaptation has no exercised T1 contract.", "reference");
                ValidateValueAddress(pointer.PointeeType);
                if (TryVoidPointerType(pointer, out var voidPointer)) return voidPointer;
                if (MemoryRepresentationType(pointer.PointeeType) is RoutineType routine) { ValidateCallingConvention(routine); return "nint"; }
                if (TryProgramPointerType(pointer, out var pointerView)) return pointerView;
                return TypeName(pointer.PointeeType) + "*";
            }
            if (type is RecordType record)
            {
                if (planned.Add(record))
                {
                    records.Add(record);
                    Name(record, record.Name ?? record.DeclarationSymbol?.Name ?? "Record", globalNames);
                }
                return names[record];
            }
            if (type is ArrayType array)
            {
                if (!planned.Contains(array))
                {
                    // Finish dependent representation planning before scheduling
                    // a declaration; a diagnosed element must not leave an unnamed type.
                    var elementName = TypeName(array.ElementType);
                    Name(array, $"Array_{elementName}_{array.Bound}", globalNames);
                    planned.Add(array);
                    arrays.Add(array);
                }
                return names[array];
            }
            throw Limit($"Type '{type.Kind}' has no T1 representation.", "type");
        }

        private static CppType Unwrap(CppType type)
        {
            while (type is TypeReference reference)
            {
                if (type is QualifiedType qualified && qualified.IsVolatile) throw Limit("Volatile qualified storage is unsupported.", "volatile");
                type = reference.UnderlyingType;
            }
            return type;
        }

        private void EmitRecord(RecordType record, StringBuilder output)
        {
            if (record.HasVirtualFunctions || record.HasVirtualBases || record.Bases.Count != 0)
                throw Limit("C++ inheritance/dispatch storage is unsupported in T1.", "record");
            var layout = record.Layout ?? throw Limit("Record has no captured layout.", "record");
            if (record.Size is not { } size || size <= 0 || size > int.MaxValue || record.Alignment is not { } alignment || alignment < 1 || alignment > 8)
                throw Limit("Record size/alignment has no T1 CLR layout representation.", "record");
            var fieldAlignment = layout.Fields.Count == 0 ? BigInteger.One : layout.Fields.Max(f => f.Field.Type.Alignment ?? BigInteger.Zero);
            if (fieldAlignment == 0)
                throw Limit("Captured record alignment exceeds CLR field alignment; explicit offsets/Pack cannot increase alignment.", "record");
            output.Append("[StructLayout(LayoutKind.Explicit, Size = ").Append(size).Append(", Pack = ").Append(alignment).AppendLine(")]");
            output.Append("public unsafe struct ").Append(names[record]).AppendLine("\n{");
            if (BigInteger.Min(fieldAlignment, alignment) != alignment || layout.Fields.All(f => f.Field.Details.IsBitField))
                output.Append("    [FieldOffset(0)] public ").Append(AlignmentCarrier(alignment)).AppendLine(" __alignment;");
            foreach (var field in layout.Fields)
            {
                ValidateValueAddress(field.Field.Type);
                if (field.Offset is not { } offset || offset < 0 || offset > int.MaxValue)
                    throw Limit("Record field has a bitfield or unavailable storage offset.", "record");
                if (field.Field.Details.IsBitField)
                {
                    EmitBitField(record, field, output);
                    continue;
                }
                if (TryEmitFlexibleTail(field.Field, output)) continue;
                output.Append("    [FieldOffset(").Append(offset).Append(")] public ").Append(TypeName(field.Field.Type)).Append(' ').Append(FieldName(field.Field)).AppendLine(";");
            }
            output.AppendLine("}").AppendLine();
        }

        private string FieldName(FieldSymbol field)
        {
            if (!names.ContainsKey(field))
            {
                var used = names.Where(p => p.Key is FieldSymbol f && f.ContainingType.Equals(field.ContainingType)).Select(p => p.Value.TrimStart('@')).ToHashSet(StringComparer.Ordinal);
                Name(field, field.Name ?? "field", used);
            }
            return names[field];
        }

        private void Name(SemanticEntity entity, string candidate, HashSet<string> used)
        {
            if (names.ContainsKey(entity)) return;
            var identifier = new string(candidate.Select(c => char.IsAsciiLetterOrDigit(c) || c == '_' ? c : '_').ToArray());
            if (identifier.Length == 0 || char.IsDigit(identifier[0])) identifier = "_" + identifier;
            var baseName = identifier;
            var suffix = 1;
            while (!used.Add(identifier)) identifier = baseName + "__" + (++suffix).ToString(CultureInfo.InvariantCulture);
            names[entity] = Keywords.Contains(identifier) ? "@" + identifier : identifier;
        }

        private static readonly HashSet<string> Keywords = new(("abstract as base bool break byte case catch char checked class const continue decimal default delegate do double else enum event explicit extern false finally fixed float for foreach goto if implicit in int interface internal is lock long namespace new null object operator out override params private protected public readonly ref return sbyte sealed short sizeof stackalloc static string struct switch this throw true try typeof uint ulong unchecked unsafe ushort using virtual void volatile while __arglist __makeref __reftype __refvalue").Split(' '), StringComparer.Ordinal);

        private static bool HasEffects(ExpressionOperation expression) => expression switch
        {
            CallOperation or TemporaryOperation or AllocationOperation or ThrowOperation => true,
            OperatorOperation op when IsStatementExpression(op.OperatorKind.Name) || op.OperatorKind.Name == "eok_va_arg" => true,
            _ => expression.Operands.Any(HasEffects)
        };
        private static bool IsBitFieldPlace(ExpressionOperation expression) => expression switch
        {
            ConversionOperation conversion => IsBitFieldPlace(conversion.Operand),
            OperatorOperation member when member.OperatorKind.Name is "eok_dot_field" or "eok_points_to_field" =>
                member.Operands.LastOrDefault() is ReferenceOperation { Target: FieldSymbol { Details.IsBitField: true } },
            _ => false
        };
        private static ExpressionOperation Required(ExpressionOperation? expression) => expression ?? throw Limit("Required executable expression is absent.", "expression");
        private static Unsupported Limit(string message, string family) => new(message, family);
    }

    private sealed class Unsupported(string message, string family) : Exception(message) { public string Family { get; } = family; }

    private sealed class CodeWriter
    {
        private readonly StringBuilder text = new();
        private int indentation;
        public void Line(string value = "") => text.Append(' ', indentation * 4).AppendLine(value);
        public void Open() { Line("{"); ++indentation; }
        public void Close() { --indentation; Line("}"); }
        public string TextWithIndent(int levels) => string.Join('\n', text.ToString().TrimEnd('\n').Split('\n').Select(line => new string(' ', levels * 4) + line)) + "\n";
    }
}
