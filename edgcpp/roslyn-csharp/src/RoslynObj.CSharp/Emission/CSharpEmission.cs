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
}

public sealed record CSharpEmissionResult(
    IReadOnlyList<GeneratedSource> Files,
    IReadOnlyList<TranslationDiagnostic> Diagnostics,
    int EmittedFunctionCount,
    int EmittedRecordCount);

/// <summary>Projects public, already-resolved EDG objects to ordinary unsafe C#.</summary>
public sealed class CSharpEmitter
{
    public CSharpEmissionResult Emit(IReadOnlyList<RoslynObject> inputs, CSharpEmissionOptions options)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(options);
        return new Emission(inputs, options).Run();
    }

    private sealed class Emission(IReadOnlyList<RoslynObject> inputs, CSharpEmissionOptions options)
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
            if (inputs.Count != 1)
                return Failure("CSHARP2001", "T1 emission requires exactly one translation unit; multi-unit binding has no implementation yet.");
            var input = inputs[0];
            globalNames.Add(options.ClassName);
            if (options.Executable) globalNames.Add("Main");
            var target = input.Manifest.Target;
            if (target.Abi != "itanium" || target.AddressableUnitBits != 8 || target.PointerBits != 64 || target.PointerAlignment != 8 || target.Endianness != "little")
                return Failure("CSHARP2002", "T1 emission requires the captured 8-bit-unit, little-endian 64-bit pointer profile.");
            if (string.IsNullOrWhiteSpace(options.RuntimeProjectPath))
                return Failure("CSHARP2005", "An explicit generated-code runtime project path is required.");
            if (input.TranslationUnit.FileScopeInitializationRoutines.Count != 0 || input.TranslationUnit.GlobalScope.DynamicInitializations.Count != 0 ||
                input.TranslationUnit.Roots.Any(r => r.Kind.Name is "file_scope_statement" or "file_initializer") ||
                input.TranslationUnit.GlobalScope.Lifetime?.Cleanups.Count > 0)
                Add("CSHARP2006", "The unit has file initialization or teardown obligations; T1 cannot publish it.", null, "initialization");
            var localStorage = input.TranslationUnit.Functions.Where(f => f.Body is not null).SelectMany(f => f.Body!.Locals).ToHashSet();
            foreach (var variable in input.TranslationUnit.Variables)
                if (!variable.Details.IsParameter && !localStorage.Contains(variable))
                    Add("CSHARP2007", $"Nonlocal storage '{variable.Name}' requires global allocation/definition planning; T1 cannot publish it.", variable.Name, "storage");
            foreach (var function in input.TranslationUnit.Functions)
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
            var declarations = new StringBuilder();
            // Type planning may discover field types while producing another record.
            for (var i = 0; i < records.Count; ++i)
            {
                try { EmitRecord(records[i], declarations); }
                catch (Unsupported ex) { Add("CSHARP2200", ex.Message, records[i].Name, ex.Family); }
            }
            var entry = functions.Where(f => f.Name == options.EntryPointName).ToArray();
            if (options.Executable && (entry.Length != 1 || entry[0].Parameters.Count != 0 || TypeName(entry[0].ReturnType) != "int"))
                Add("CSHARP2004", "Executable mode requires one selected entry with no parameters and a captured signed 32-bit result.", options.EntryPointName, "entry");
            if (diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error)) return new([], diagnostics, 0, 0);
            var source = new StringBuilder("// Generated from public RoslynObj executable relationships.\nusing System;\nusing System.Runtime.CompilerServices;\nusing System.Runtime.InteropServices;\nusing RoslynObj.CSharp.Runtime;\n\n");
            source.Append("namespace ").Append(options.NamespaceName).AppendLine(";").AppendLine();
            source.Append(declarations);
            source.Append("public static unsafe class ").Append(options.ClassName).AppendLine("\n{");
            source.Append("    static ").Append(options.ClassName).AppendLine("() => RuntimeContract.RequireLayout(8, true);").AppendLine();
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
            var project = $"<Project Sdk=\"Microsoft.NET.Sdk\">\n  <PropertyGroup>\n    <TargetFramework>net10.0</TargetFramework>\n    <LangVersion>14</LangVersion>\n    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>\n    <Nullable>enable</Nullable>\n    <OutputType>{(options.Executable ? "Exe" : "Library")}</OutputType>\n    <CheckForOverflowUnderflow>false</CheckForOverflowUnderflow>\n  </PropertyGroup>\n  <ItemGroup><ProjectReference Include=\"{runtime}\" /></ItemGroup>\n</Project>\n";
            return new([new("Generated.cs", source.ToString()), new("Generated.csproj", project)], diagnostics, functions.Count, records.Count);
        }

        private CSharpEmissionResult Failure(string code, string message)
        {
            Add(code, message, null, "profile");
            return new([], diagnostics, 0, 0);
        }

        private void Add(string code, string message, string? name, string family) => diagnostics.Add(new(code, DiagnosticSeverity.Error,
            message, inputs.FirstOrDefault()?.Manifest.ArtifactIdentity,
            $"function/{functionOrdinal}:{name ?? currentFunction?.Name ?? "program"}/operation/{operationOrdinal}", family));

        private void EmitFunction(FunctionSymbol function)
        {
            if (function.Signature.Routine.IsVariadic) throw Limit("Variadic routine definitions require the T2 argument-pack contract.", "varargs");
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
                if (variable.IsThreadLocal || variable.LocalStaticInitializations.Count != 0 || variable.Details.StorageDuration?.Name is "static" or "thread")
                    throw Limit("Static/thread local storage requires the T2 initialization contract.", "storage");
                Name(variable, variable.Name ?? "temporary", localNames);
            }
            var writer = new CodeWriter();
            writer.Line($"public static {TypeName(function.ReturnType)} {names[function]}({string.Join(", ", function.Parameters.Select(p => $"{TypeName(p.Type)} {names[p.Symbol!]}"))})");
            writer.Open();
            foreach (var variable in body.Locals)
            {
                writer.Line($"{TypeName(variable.Type)} {names[variable]};");
                if (Unwrap(variable.Type) is PointerType) writer.Line($"{names[variable]} = null;");
                else writer.Line($"Unsafe.SkipInit(out {names[variable]});");
            }
            if (body.Locals.Count > 0) writer.Line();
            if (body.Root is BlockOperation block) foreach (var statement in block.Statements) Statement(statement, writer);
            else if (body.Root is StatementOperation statement) Statement(statement, writer);
            else throw Limit($"Body root '{body.Root.Kind}' has no T1 statement projection.", body.Root.Kind.Name);
            writer.Close();
            methods.Append(writer.TextWithIndent(1)).AppendLine();
        }

        private void Statement(StatementOperation statement, CodeWriter writer)
        {
            ++operationOrdinal;
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
                    if (declaration.HasStaticOrThreadVariable) throw Limit("Static declaration statements are unsupported.", "storage");
                    return;
                case InitializationStatementOperation initialization:
                    Initialization(initialization.Initialization, writer);
                    return;
                case ConditionalOperation conditional:
                    writer.Line($"if ({Truth(Required(conditional.Condition))})");
                    Embedded(conditional.Then, writer);
                    if (conditional.Else is not null) { writer.Line("else"); Embedded(conditional.Else, writer); }
                    return;
                case LoopOperation loop:
                    if (loop.Kind.Name == "stmk_end_test_while")
                    {
                        writer.Line("do"); Embedded(loop.Body, writer); writer.Line($"while ({Truth(Required(loop.Condition))});"); return;
                    }
                    if (loop.Kind.Name is not ("stmk_for" or "stmk_while")) throw Limit($"Loop '{loop.Kind}' is unsupported.", "loop");
                    if (loop.Initialization is not null) Statement(loop.Initialization, writer);
                    if (loop.Kind.Name == "stmk_for")
                        writer.Line($"for (; {(loop.Condition is null ? "true" : Truth(loop.Condition))}; {(loop.Increment is null ? "" : Expression(loop.Increment))})");
                    else writer.Line($"while ({Truth(Required(loop.Condition))})");
                    Embedded(loop.Body, writer);
                    return;
                case JumpOperation jump:
                    switch (jump.JumpKind.Name)
                    {
                        case "return": writer.Line(jump.Value is null ? "return;" : $"return {Expression(jump.Value)};"); return;
                        case "break": writer.Line("break;"); return;
                        case "continue": writer.Line("continue;"); return;
                        default: throw Limit($"Jump '{jump.JumpKind}' requires label/cleanup handling.", "jump");
                    }
                default:
                    if (statement.Kind.Name == "stmk_empty") { writer.Line(";"); return; }
                    if (statement.Kind.Name == "stmk_expr" && statement.Expression is { } expression)
                    {
                        var value = Expression(expression);
                        if (expression is OperatorOperation op && IsStatementExpression(op.OperatorKind.Name)) writer.Line(value + ";");
                        else if (expression is CallOperation) writer.Line(value + ";");
                        else writer.Line("_ = " + value + ";");
                        return;
                    }
                    throw Limit($"Statement '{statement.Kind}' has no T1 projection.", statement.Kind.Name);
            }
        }

        private void Embedded(StatementOperation? statement, CodeWriter writer)
        {
            if (statement is BlockOperation block) Statement(block, writer);
            else { writer.Open(); if (statement is not null) Statement(statement, writer); writer.Close(); }
        }

        private void Initialization(Initialization initialization, CodeWriter writer)
        {
            if (initialization.Kind.Name is not ("dik_expression" or "dik_constant") ||
                initialization.Details.IsConditional || initialization.Details.IsReusedValue || initialization.Details.IsPartialAggregate)
                throw Limit($"Initialization '{initialization.Kind}' requires a richer storage/execution contract.", "initialization");
            if (initialization.Constructor is not null || initialization.Destructor is not null)
                throw Limit("Constructor/destructor initialization is unsupported in T1.", "initialization");
            if (initialization.Target is not VariableSymbol variable || !names.ContainsKey(variable))
                throw Limit("Initialization target is not selected local storage.", "initialization");
            if (Unwrap(variable.Type) is RecordType)
                throw Limit("Aggregate initialization/copy requires the T2 storage contract.", "initialization");
            var value = initialization.Expression is ExpressionOperation expression ? Expression(expression)
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
            if (reference.Target is FunctionSymbol) throw Limit("A function address outside a proven direct call needs the T2 calling convention contract.", "function-pointer");
            if (reference.Target is FieldSymbol) throw Limit("A field reference requires its actual retained object operand.", "field");
            if (reference.Target is { } target && names.TryGetValue(target, out var name)) return name;
            throw Limit($"Reference '{reference.Target?.Name}' has no selected function/local representation; globals and imports require T2.", "reference");
        }

        private string Call(CallOperation call)
        {
            if (call.Callee is not ReferenceOperation { Target: FunctionSymbol calleeTarget })
                throw Limit("Direct call projection requires the actual callee occurrence to be a routine reference; effectful/indirect callees need T2.", "call");
            var target = call.TargetMethod ?? calleeTarget;
            if (!target.Equals(calleeTarget)) throw Limit("Selected call target and actual callee reference require an explicit adapter.", "call");
            if (target is null || !functions.Contains(target)) throw Limit($"Call target '{target?.Name}' is not a selected executable definition.", "call");
            if (call.Receiver is not null || call.Call.ReturnSlot is not null) throw Limit("Hidden receiver/result-slot adaptation requires T2.", "call");
            if (call.Operator.EvalRightToLeft && call.Arguments.Count > 1 && call.Arguments.Any(a => HasEffects(a.Value)))
                throw Limit("A call with right-to-left evaluation and effectful arguments requires explicit sequencing temporaries.", "sequencing");
            return $"{names[target]}({string.Join(", ", call.Arguments.Select(a => Expression(a.Value)))})";
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
            if (kind is not ("eok_cast" or "eok_lvalue_cast" or "eok_ref_cast")) throw Limit($"Conversion '{kind}' is unsupported.", "conversion");
            return $"unchecked(({TypeName(conversion.TargetType)})({value}))";
        }

        private string Operator(OperatorOperation operation)
        {
            var kind = operation.OperatorKind.Name;
            var operands = operation.Operands;
            string A(int index) => Expression(operands[index]);
            if (operation.Operator.EvalRightToLeft && operands.Count > 1 && operands.Any(HasEffects))
                throw Limit($"Operator '{kind}' has effectful right-to-left operands and requires explicit sequencing temporaries.", "sequencing");
            switch (kind)
            {
                case "eok_parens": return $"({A(0)})";
                case "eok_address_of": case "eok_reference_to": return $"&({A(0)})";
                case "eok_indirect": case "eok_ref_indirect": return $"(*({A(0)}))";
                case "eok_dot_field": case "eok_points_to_field":
                    if (operands[1] is not ReferenceOperation { Target: FieldSymbol field })
                        throw Limit("A member access requires the selected field reference operand.", "field");
                    return $"({A(0)}){(kind == "eok_dot_field" ? "." : "->")}{FieldName(field)}";
                case "eok_subscript":
                    if (operation.Operator.PointerOperandIsSecond)
                        throw Limit("A subscript with its pointer second needs explicit operand evaluation planning.", "sequencing");
                    return $"({A(0)})[{A(1)}]";
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
                if (Unwrap(operands[0].Type) is RecordType) throw Limit("Aggregate assignment requires the T2 exact-storage copy contract.", "storage");
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
        private static bool IsStatementExpression(string kind) => AssignmentOperators.ContainsKey(kind) || kind is "eok_pre_incr" or "eok_pre_decr" or "eok_post_incr" or "eok_post_decr";

        private string Constant(Constant constant)
        {
            if (constant.Value is IntegerValue integer)
            {
                var type = constant.Type ?? throw Limit("Integer constant has no recorded type.", "constant");
                var name = TypeName(type);
                var value = integer.Value.ToString(CultureInfo.InvariantCulture);
                var suffix = name switch { "uint" => "U", "long" => "L", "ulong" => "UL", _ => "" };
                return name is "int" or "uint" or "long" or "ulong" ? value + suffix : $"({name}){value}";
            }
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
        private string TruthValue(string value, CppType type) => $"({value}) != {(Unwrap(type) is PointerType ? "null" : "0")}";
        private string BoolValue(string comparison, CppType type) => $"(({comparison}) ? ({TypeName(type)})1 : ({TypeName(type)})0)";

        private string TypeName(CppType type)
        {
            type = Unwrap(type);
            if (type.Kind.Name == "tk_void") return "void";
            if (type is ArithmeticType arithmetic)
            {
                var detail = arithmetic.Arithmetic;
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
                if (Unwrap(pointer.PointeeType) is RoutineType) throw Limit("Function addresses require the T2 calling convention contract.", "function-pointer");
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
            if (fieldAlignment == 0 || BigInteger.Min(fieldAlignment, alignment) != alignment)
                throw Limit("Captured record alignment exceeds CLR field alignment; explicit offsets/Pack cannot increase alignment.", "record");
            output.Append("[StructLayout(LayoutKind.Explicit, Size = ").Append(size).Append(", Pack = ").Append(alignment).AppendLine(")]");
            output.Append("public unsafe struct ").Append(names[record]).AppendLine("\n{");
            foreach (var field in layout.Fields)
            {
                if (field.Field.Details.IsBitField || field.Offset is not { } offset || offset < 0 || offset > int.MaxValue)
                    throw Limit("Record field has a bitfield or unavailable storage offset.", "record");
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
            OperatorOperation op when IsStatementExpression(op.OperatorKind.Name) => true,
            _ => expression.Operands.Any(HasEffects)
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
