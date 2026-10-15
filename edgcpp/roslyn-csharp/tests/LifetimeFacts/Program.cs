using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using RoslynObj;

if (args.Length != 2 || File.Exists(args[1])) throw new ArgumentException("LifetimeFacts INPUT.roslynobj NEW_REPORT.json");
var inputPath = Path.GetFullPath(args[0]);
var inputHash = Hash(inputPath);
var watch = Stopwatch.StartNew();
using var input = RoslynObject.Open(inputPath, new ReadOptions { CachedRowCount = 4096, CachedPageCount = 128, PageSize = 128 });
var probe = new LifetimeProbe(input);
var result = probe.Build();
result["Input"] = new { Path = inputPath, Sha256 = inputHash, Bytes = new FileInfo(inputPath).Length };
result["InputUnchanged"] = Hash(inputPath) == inputHash;
result["ElapsedSeconds"] = watch.Elapsed.TotalSeconds;
Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(args[1]))!);
File.WriteAllText(args[1], JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }) + "\n");
Console.WriteLine($"lifetime-facts functions={probe.FunctionCount}; entities={probe.EntityCount}; relationship-occurrences={probe.OccurrenceCount}; input-unchanged={result["InputUnchanged"]}");
static string Hash(string path) { using var stream = File.OpenRead(path); return Convert.ToHexStringLower(SHA256.HashData(stream)); }

internal sealed class LifetimeProbe(RoslynObject input)
{
    private const int MaximumEntities = 20000, MaximumOccurrences = 20000, MaximumDepth = 96;
    private readonly Dictionary<SemanticEntity, string> locators = [];
    private readonly Queue<SemanticEntity> pending = [];
    private readonly List<Dictionary<string, object?>> entities = [];
    public int FunctionCount { get; private set; }
    public int EntityCount => entities.Count;
    public int OccurrenceCount { get; private set; }

    private static Dictionary<string, object?> Row(params (string Name, object? Value)[] fields) => fields.ToDictionary(field => field.Name, field => field.Value, StringComparer.Ordinal);
    private static string? Number(System.Numerics.BigInteger? value) => value?.ToString(CultureInfo.InvariantCulture);

    private string? Ref(SemanticEntity? entity)
    {
        if (entity is null) return null;
        if (locators.TryGetValue(entity, out var locator)) return locator;
        if (locators.Count >= MaximumEntities) throw new InvalidOperationException("Focused lifetime entity bound exceeded.");
        locator = $"entity/{locators.Count + 1:D6}";
        locators.Add(entity, locator); pending.Enqueue(entity); return locator;
    }

    public Dictionary<string, object?> Build()
    {
        var functions = input.TranslationUnit.Functions.ToArray(); FunctionCount = functions.Length;
        foreach (var function in functions) _ = Ref(function);
        var bodies = functions.Where(function => function.Body is not null).Select(function =>
        {
            var body = function.Body!;
            var occurrences = new List<Dictionary<string, object?>>();
            Walk(body.Root, "body", null, "body-root", 0, [], occurrences, 0);
            var eh = body.ExceptionHandling;
            return Row(("Function", Ref(function)), ("Root", Ref(body.Root)), ("Scope", Ref(body.Scope)),
                ("Locals", body.Locals.Select(variable => Ref(variable)).ToArray()),
                ("ReachableLifetimesNotOwnership", body.Lifetimes.Select(lifetime => Ref(lifetime)).ToArray()),
                ("PublicStructuralOccurrences", occurrences),
                ("ExceptionHandling", Row(("RegionTable", Ref(eh.RegionTable)), ("ArrayTable", Ref(eh.ArrayTable)),
                    ("InitialState", Number(eh.Details?.InitialState)), ("NullState", Number(eh.Details?.NullState)),
                    ("Prologue", Ref(eh.Details?.Prologue)), ("Epilogues", eh.Epilogues.Select(operation => Ref(operation)).ToArray()),
                    ("Regions", eh.CleanupRegions.Select(region => Ref(region)).ToArray()), ("Arrays", eh.Arrays.Select(array => Ref(array)).ToArray()))));
        }).ToArray();
        while (pending.TryDequeue(out var entity)) entities.Add(Describe(entity));
        var manifest = input.Manifest;
        return Row(("ProbeVersion", 1), ("InventoryOnly", true), ("GeneratedExecutionAccepted", false),
            ("Semantics", "Local locators use only public semantic equality. Public structural paths preserve parent/role/ordinal and repeated identities; they are not an execution schedule, reachability proof or inferred ownership. Metadata boundaries are recorded without executing or expanding them into body occurrences. Typed facets independently record actual call/initialization/control relationships; matching facets can alias a single operand and must not be replayed."),
            ("Bounds", new { MaximumEntities, MaximumOccurrences, MaximumDepth, CachedRows = 4096, CachedPages = 128, PageSize = 128 }),
            ("Provenance", new { manifest.FormatMajor, manifest.FormatMinor, manifest.ArtifactIdentity, manifest.Producer,
                manifest.EdgRevision, manifest.EdgVersion, manifest.CaptureProfile, manifest.Stage,
                Target = new { manifest.Target.Abi, manifest.Target.AddressableUnitBits, manifest.Target.Endianness,
                    PointerBits = Number(manifest.Target.PointerBits), PointerAlignment = Number(manifest.Target.PointerAlignment) },
                InvocationArguments = input.TranslationUnit.InvocationArguments.ToArray(),
                Capabilities = input.TranslationUnit.Capabilities.Select(capability => new { capability.Name, capability.Value,
                    capability.IsRequired, capability.Enabled, State = capability.State.Name }).ToArray() }),
            ("Functions", functions.Select(function => Ref(function)).ToArray()), ("Bodies", bodies), ("Entities", entities));
    }

    private void Walk(SemanticNode node, string path, string? parent, string role, long ordinal,
        HashSet<SemanticNode> active, List<Dictionary<string, object?>> rows, int depth)
    {
        if (depth > MaximumDepth || ++OccurrenceCount > MaximumOccurrences)
            throw new InvalidOperationException("Focused lifetime structural occurrence bound exceeded.");
        var cycle = !active.Add(node);
        var metadata = node.RepresentationRole.Name == "metadata";
        rows.Add(Row(("Path", path), ("ParentPath", parent), ("RelationRole", role), ("RelationOrdinal", ordinal),
            ("Node", Ref(node)), ("Kind", node is OperatorOperation operation ? operation.OperatorKind.Name : node.Kind.Name),
            ("RepresentationRole", node.RepresentationRole.Name), ("CycleBoundary", cycle), ("MetadataBoundary", metadata)));
        if (cycle) return;
        if (metadata) { active.Remove(node); return; }
        foreach (var child in node.ChildRelations)
            Walk(child.Child, path + "/" + child.Role.Name + "[" + child.Ordinal.ToString(CultureInfo.InvariantCulture) + "]",
                path, child.Role.Name, child.Ordinal, active, rows, depth + 1);
        active.Remove(node);
    }

    private Dictionary<string, object?> Parameter(SignatureParameter parameter) => Row(
        ("Ordinal", parameter.Ordinal), ("Name", parameter.Name), ("Role", parameter.Role.Name),
        ("Type", Ref(parameter.Type)), ("DeclaredType", Ref(parameter.DeclaredType)), ("Descriptor", Ref(parameter.Descriptor)),
        ("PassedViaCopyConstructor", parameter.Descriptor.Details.PassedViaCopyConstructor),
        ("Variable", parameter is Parameter execution ? Ref(execution.Symbol) : null),
        ("IsSourceParameter", parameter is Parameter source ? (bool?)source.IsSourceParameter : null),
        ("BaseSubobject", Ref(parameter.BaseSubobject)));

    private Dictionary<string, object?> Signature(RoutineType signature) => Row(
        ("Type", Ref(signature)), ("ReturnType", Ref(signature.ReturnType)), ("ThisClass", Ref(signature.ThisClass)),
        ("HasPrototype", signature.HasPrototype), ("IsVariadic", signature.IsVariadic),
        ("HasThisParameter", signature.HasThisParameter), ("CallingConvention", signature.CallingConvention?.Name),
        ("LanguageLinkage", signature.LanguageLinkage?.Name), ("ReturnsValueAsParameter", signature.ReturnsValueAsParameter),
        ("ReturnValueParameterFollowsThis", signature.ReturnValueParameterFollowsThis),
        ("ReturnsValueViaCopyConstructor", signature.ReturnsValueViaCopyConstructor),
        ("SelectedParameters", signature.Parameters.Select(Parameter).ToArray()),
        ("NoexceptEffective", signature.Routine.ExceptionSpecification?.NoexceptEffective));

    private Dictionary<string, object?> Describe(SemanticEntity entity)
    {
        var row = Row(("Locator", Ref(entity)), ("Name", entity.Name), ("CurrentName", entity.CurrentName),
            ("SourceNameForDisplayOnly", entity.SourceCorrespondence?.SourceName));
        switch (entity)
        {
            case FunctionSymbol function:
                row["EntityKind"] = "function"; row["BodyState"] = function.BodyState.Name;
                row["SpecialFunctionKind"] = function.SpecialFunctionKind?.Name;
                row["ConstructorOrDestructorKind"] = function.ConstructorOrDestructorKind?.Name;
                row["IsTrivialDefaultConstructor"] = function.IsTrivialDefaultConstructor;
                row["IsTrivialCopyFunction"] = function.IsTrivialCopyFunction; row["IsTrivialDestructor"] = function.IsTrivialDestructor;
                row["PrimaryEntry"] = Ref(function.PrimaryEntry);
                row["AlternateEntryPoints"] = function.AlternateEntryPoints.Select(entry => Ref(entry)).ToArray();
                row["Signature"] = Signature(function.Signature);
                row["ExecutionParameters"] = function.Parameters.Select(Parameter).ToArray();
                row["SourceParametersForDisplayOnly"] = function.SourceParameters.Select(Parameter).ToArray();
                row["ContainingType"] = Ref(function.ContainingType); row["BodyRoot"] = Ref(function.Body?.Root);
                break;
            case CppType type:
                row["EntityKind"] = "type"; row["Kind"] = type.Kind.Name;
                row["Size"] = Number(type.Size); row["Alignment"] = Number(type.Alignment);
                row["IsIncomplete"] = type.IsIncomplete; row["IsDependent"] = type.IsDependent;
                if (type is TypeReference reference)
                {
                    row["UnderlyingType"] = Ref(reference is QualifiedType qualifiedReference ? qualifiedReference.UnderlyingType : reference.UnderlyingType);
                    row["IsConst"] = reference is QualifiedType qualified ? (bool?)qualified.IsConst : null;
                    row["IsVolatile"] = reference is QualifiedType volatileQualified ? (bool?)volatileQualified.IsVolatile : null;
                }
                if (type is PointerType pointer)
                {
                    row["PointeeType"] = Ref(pointer.PointeeType); row["IsReference"] = pointer.IsReference; row["IsRValueReference"] = pointer.IsRValueReference;
                }
                if (type is ArithmeticType arithmetic)
                {
                    row["ArithmeticKind"] = arithmetic.Arithmetic.ArithmeticKind.Name; row["Bits"] = arithmetic.Arithmetic.Bits;
                    row["Signed"] = arithmetic.Arithmetic.Signed;
                }
                if (type is ArrayType arrayType)
                {
                    row["ElementType"] = Ref(arrayType.ElementType); row["Bound"] = Number(arrayType.Bound); row["BoundKind"] = arrayType.Array.BoundKind.Name;
                }
                if (type is RoutineType routine) row["Signature"] = Signature(routine);
                if (type is RecordType record)
                {
                    row["Fields"] = record.Fields.Select(field => Ref(field)).ToArray();
                    row["Layout"] = record.Layout?.Fields.Select(field => new { Field = Ref(field.Field), Offset = Number(field.Offset),
                        BitOffset = Number(field.BitOffset), BitWidth = Number(field.BitWidth) }).ToArray();
                }
                break;
            case VariableSymbol variable:
                row["EntityKind"] = "variable"; row["Type"] = Ref(variable.Type); row["Storage"] = variable.SymbolDetails.Storage?.Name;
                row["StorageDuration"] = variable.Details.StorageDuration?.Name; row["InitializationKind"] = variable.InitializationKind.Name;
                row["InitializerAssociation"] = Ref(variable.Initializer); break;
            case FieldSymbol field:
                row["EntityKind"] = "field"; row["Type"] = Ref(field.Type); break;
            case ParameterType descriptor:
                row["EntityKind"] = "parameter-descriptor"; row["Type"] = Ref(descriptor.Type);
                row["DeclaredType"] = Ref(descriptor.DeclaredType); row["PassedViaCopyConstructor"] = descriptor.Details.PassedViaCopyConstructor; break;
            case Initialization initialization:
                DescribeInitialization(initialization, row);
                if (initialization.Operation is { } initializationOperation)
                {
                    DescribeNode(initializationOperation, row);
                    row["EntityKind"] = "initialization-and-operation-facets";
                }
                break;
            case Lifetime lifetime:
                row["EntityKind"] = "lifetime"; row["Kind"] = lifetime.Kind.Name; row["Parent"] = Ref(lifetime.Parent);
                row["BoundEntity"] = Ref(lifetime.BoundEntity); row["ParentCheckpoint"] = Ref(lifetime.ParentCheckpoint);
                row["HasImplicitChild"] = lifetime.HasImplicitChild; row["HasBlockAfterLabelChild"] = lifetime.HasBlockAfterLabelChild;
                row["ChildrenReverseCreationOrder"] = lifetime.Children.Select(child => Ref(child)).ToArray();
                row["CleanupRecipesNotNormalExecution"] = lifetime.Cleanups.Select(cleanup => new { cleanup.Ordinal,
                    Initialization = Ref(cleanup.Initialization), Destructor = Ref(cleanup.Destructor), cleanup.IsConditional, cleanup.IsUnordered }).ToArray(); break;
            case CleanupRegion region:
                row["EntityKind"] = "cleanup-region"; row["Ordinal"] = region.Ordinal; row["NextRegion"] = Ref(region.NextRegion);
                row["Condition"] = Ref(region.Condition); row["IsSupplement"] = region.Details.IsSupplement;
                row["SupplementOwner"] = Ref(region.Details.SupplementOwner);
                row["Actions"] = region.Actions.Select(action => Ref(action)).ToArray(); break;
            case CleanupAction action:
                row["EntityKind"] = "cleanup-action"; row["Kind"] = action.Kind.Name; row["Ordinal"] = action.Details.Ordinal;
                row["Routine"] = Ref(action.TargetMethod); row["Storage"] = Ref(action.Storage); row["Array"] = Ref(action.Array);
                row["AbiDetail"] = Ref(action.AbiDetail); row["IsUnordered"] = action.IsUnordered;
                row["Flags"] = new { action.Details.IsArray, action.Details.IsIndirect, action.Details.IsConditional,
                    action.Details.IsBaseSubobject, action.Details.IsVla, action.Details.IsDeallocation, action.Details.IsLocalStaticGuard }; break;
            case CleanupStorageLocation storage:
                row["EntityKind"] = "cleanup-storage"; row["Kind"] = storage.Kind.Name;
                row["Variable"] = Ref(storage.Variable); row["BaseEntity"] = Ref(storage.Details.BaseEntity);
                row["Offset"] = Number(storage.Offset); row["IsIndirect"] = storage.IsIndirect; row["UsesThisParameterOffset"] = storage.UsesThisParameterOffset; break;
            case CleanupCondition condition:
                row["EntityKind"] = "cleanup-condition"; row["Kind"] = condition.Kind.Name;
                row["Storage"] = Ref(condition.Storage); row["ComparisonValue"] = Number(condition.Details.ComparisonValue); break;
            case CleanupArray array:
                row["EntityKind"] = "cleanup-array"; row["HandleConstant"] = Ref(array.HandleConstant);
                row["ElementSize"] = Number(array.ElementSize); row["ElementCount"] = Number(array.ElementCount);
                row["IsNonarraySize"] = array.Details.IsNonarraySize;
                row["StorageLocations"] = array.StorageLocations.Select(location => Ref(location)).ToArray();
                row["CountLocations"] = array.CountLocations.Select(location => Ref(location)).ToArray(); break;
            case CleanupAbiDetail abi:
                row["EntityKind"] = "cleanup-abi"; row["VttLocation"] = Ref(abi.VttLocation);
                row["VlaCountLocation"] = Ref(abi.VlaCountLocation); row["IsDelegating"] = abi.IsDelegating;
                row["DelegationArgumentLocation"] = Ref(abi.DelegationArgumentLocation); break;
            case ExceptionHandler handler:
                row["EntityKind"] = "handler"; row["Body"] = Ref(handler.Body); row["CaughtType"] = Ref(handler.CaughtType);
                row["Parameter"] = Ref(handler.Parameter); row["ParameterInitialization"] = Ref(handler.ParameterInitialization);
                row["BodyLifetime"] = Ref(handler.BodyLifetime); row["ParameterInitializationLifetime"] = Ref(handler.ParameterInitializationLifetime);
                row["Typeinfo"] = Ref(handler.Details.Typeinfo); break;
            case TryDescriptor definition:
                row["EntityKind"] = "try-descriptor"; row["Body"] = Ref(definition.Body);
                row["Lifetime"] = Ref(definition.Lifetime); row["IsFunctionTry"] = definition.IsFunctionTry;
                row["Handlers"] = definition.Handlers.Select(handler => Ref(handler)).ToArray(); break;
            case Scope scope:
                row["EntityKind"] = "scope"; row["Kind"] = scope.Kind.Name; row["Lifetime"] = Ref(scope.Lifetime);
                row["DynamicInitializations"] = scope.DynamicInitializations.Select(initialization => Ref(initialization)).ToArray();
                row["ThisParameter"] = Ref(scope.ThisParameter); row["NrvoVariable"] = Ref(scope.NrvoVariable);
                row["ConstructorInitializers"] = scope.ConstructorInitializers.Select(initializer => new { Kind = initializer.Kind.Name,
                    Field = Ref(initializer.Field), BaseSubobject = Ref(initializer.BaseSubobject), Initialization = Ref(initializer.Initialization),
                    EffectiveInitialization = Ref(initializer.EffectiveInitialization), SourceExpression = Ref(initializer.SourceExpression),
                    initializer.UsesFieldInitializer, initializer.IsCompilerGenerated }).ToArray(); break;
            case SemanticNode node: DescribeNode(node, row); break;
            default: row["EntityKind"] = "associated-entity-not-expanded"; break;
        }
        return row;
    }

    private void DescribeInitialization(Initialization initialization, Dictionary<string, object?> row)
    {
        row["EntityKind"] = "initialization"; row["Kind"] = initialization.Kind.Name; row["InitializationKind"] = initialization.Kind.Name; row["Target"] = Ref(initialization.Target);
        row["Type"] = Ref(initialization.Details.Type); row["Operation"] = Ref(initialization.Operation);
        row["OperationRole"] = initialization.Operation?.RepresentationRole.Name;
        row["Constructor"] = Ref(initialization.Constructor); row["Destructor"] = Ref(initialization.Destructor);
        row["ExpressionAssociation"] = Ref(initialization.Expression); row["BitwiseCopySource"] = Ref(initialization.BitwiseCopySource);
        row["ExactSourceAssociation"] = Ref(initialization.Details.Source); row["Constant"] = Ref(initialization.Constant);
        row["Lifetime"] = Ref(initialization.Lifetime); row["InitExpressionLifetime"] = Ref(initialization.InitExpressionLifetime);
        row["OverlappingLifetime"] = Ref(initialization.OverlappingLifetime); row["MasterAssociation"] = Ref(initialization.Master);
        row["NextDestructionAssociation"] = Ref(initialization.NextInDestructionList);
        row["Operands"] = initialization.Operands.Select(operand => new { Role = operand.Role.Name, operand.Ordinal, Expression = Ref(operand.Expression) }).ToArray();
        row["Flags"] = new { initialization.Details.IsUnordered, initialization.Details.IsConditional, initialization.Details.IsReusedValue,
            initialization.Details.IsOverlapping, initialization.Details.IsClassResultMaster, initialization.Details.IsStaticTemporary,
            initialization.Details.IsPartialAggregate, initialization.Details.IsPartialInitialization, initialization.Details.FollowsExecutableStatement,
            initialization.Details.HasTemporaryLifetime, initialization.Details.IsConstructorInitialization, initialization.Details.ConstructorCopyImpliedSource,
            initialization.Details.ConstructorImplicitCopy, initialization.Details.ConstructorValueInitialization, initialization.Details.ConstructorSequencedArguments,
            initialization.Details.ConstructorArrayCopy, initialization.Details.IsQuestionResult, initialization.Details.IsCommaResult,
            initialization.Details.FreesStorage, initialization.Details.FreesArray, initialization.IsConditionalGuard, initialization.IsVlaDeallocation,
            initialization.IsFreeingOfExceptionObject };
    }

    private void DescribeNode(SemanticNode node, Dictionary<string, object?> row)
    {
        row["EntityKind"] = node is Operation ? "operation" : "semantic-node";
        row["Kind"] = node.Kind.Name; row["NodeKind"] = node.Kind.Name; row["RepresentationRole"] = node.RepresentationRole.Name; row["IsCompilerGenerated"] = node.IsCompilerGenerated;
        row["StructuralRelationsNotSchedule"] = node.ChildRelations.Select(child => new { Role = child.Role.Name, child.Ordinal, Child = Ref(child.Child) }).ToArray();
        row["ReferenceAssociationsNotEvaluation"] = node.References.Select(reference => new { Role = reference.Role.Name, reference.Ordinal, Target = Ref(reference.Target) }).ToArray();
        if (node is ExpressionOperation expression)
        {
            row["Type"] = Ref(expression.Type); row["ValueCategory"] = expression.ValueCategory.Name;
            row["Operands"] = expression.Operands.Select(operand => Ref(operand)).ToArray();
        }
        if (node is OperatorOperation operation)
        {
            row["OperatorKind"] = operation.OperatorKind.Name; row["EvalLeftToRight"] = operation.Operator.EvalLeftToRight;
            row["EvalRightToLeft"] = operation.Operator.EvalRightToLeft;
        }
        switch (node)
        {
            case CallOperation call:
                row["Call"] = new { Kind = call.CallKind.Name, Callee = Ref(call.Callee), TargetMethod = Ref(call.TargetMethod),
                    SourceTargetMethodForDisplayOnly = Ref(call.SourceTargetMethod), Receiver = Ref(call.Receiver), ReturnSlot = Ref(call.Call.ReturnSlot),
                    FunctionType = Ref(call.Call.FunctionType), Arguments = call.Arguments.Select(argument => new { argument.Ordinal,
                        Role = argument.Role?.Name, Parameter = Ref(argument.Parameter), Value = Ref(argument.Value) }).ToArray(),
                    ArgumentOperandIndices = call.Arguments.Select(argument => new { argument.Ordinal,
                        Indices = call.Operands.Select((operand, index) => (operand, index)).Where(pair => pair.operand.Equals(argument.Value)).Select(pair => pair.index).ToArray() }).ToArray() }; break;
            case ReferenceOperation reference: row["Target"] = Ref(reference.Target); break;
            case ConstantOperation constant:
                row["ConstantKind"] = constant.Constant.Value.Kind.Name;
                row["IntegerValue"] = constant.Constant.Value is IntegerValue integer ? Number(integer.Value) : null; break;
            case InitializationOperation initialization:
                row["Initialization"] = Ref(initialization.Initialization); DescribeInitialization(initialization.Initialization, row);
                row["EntityKind"] = "initialization-and-operation-facets"; break;
            case InitializationStatementOperation initialization: row["Initialization"] = Ref(initialization.Initialization); break;
            case StatementExpressionResultOperation result: row["Initialization"] = Ref(result.Initialization); break;
            case TemporaryOperation temporary:
                row["Temporary"] = new { Initialization = Ref(temporary.Initialization), Target = Ref(temporary.Target), SourceType = Ref(temporary.SourceType) }; break;
            case LifetimeOperation lifetime: row["Lifetime"] = Ref(lifetime.Lifetime); row["Value"] = Ref(lifetime.Value); break;
            case ReuseOperation reuse: row["Initialization"] = Ref(reuse.Initialization); break;
            case BlockOperation block:
                row["Block"] = new { Scope = Ref(block.Scope), Lifetime = Ref(block.Lifetime),
                    Statements = block.Statements.Select(statement => Ref(statement)).ToArray() }; break;
            case ConditionalOperation conditional: row["Conditional"] = new { Condition = Ref(conditional.Condition), Then = Ref(conditional.Then), Else = Ref(conditional.Else) }; break;
            case LoopOperation loop: row["Loop"] = new { Initialization = Ref(loop.Initialization), Condition = Ref(loop.Condition), Body = Ref(loop.Body), Increment = Ref(loop.Increment) }; break;
            case JumpOperation jump: row["Jump"] = new { Kind = jump.JumpKind.Name, Value = Ref(jump.Value), Target = Ref(jump.Target), CommonLifetime = Ref(jump.CommonLifetime) }; break;
            case ConditionOperation condition: row["Condition"] = new { Value = Ref(condition.Value), Initialization = Ref(condition.Initialization),
                DynamicInitialization = Ref(condition.DynamicInitialization), Variable = Ref(condition.Variable), Scope = Ref(condition.Scope) }; break;
            case ThrowOperation thrown: row["Throw"] = new { Operand = Ref(thrown.Operand), Type = Ref(thrown.ThrownType),
                Initialization = Ref(thrown.Initialization), Destructor = Ref(thrown.Destructor), thrown.IsRethrow,
                AccessibleBases = thrown.AccessibleBases.Select(subobject => Ref(subobject)).ToArray() }; break;
            case TryOperation statement: row["Try"] = new { Body = Ref(statement.Body), Lifetime = Ref(statement.Definition.Lifetime),
                statement.Definition.IsFunctionTry, Handlers = statement.Handlers.Select(handler => Ref(handler)).ToArray() }; break;
        }
        if (node is LoweredEhOperation lowered)
        {
            row["LoweredEh"] = new { Kind = lowered.ExceptionKind.Name, Initialization = Ref(lowered.Initialization),
                CleanupStartAssociation = Ref(lowered.CleanupStart), CompletedInitializerAssociation = Ref(lowered.CompletedInitializer),
                Region = Ref(lowered.ExceptionOperation.Region), StateNumber = Number(lowered.ExceptionOperation.StateNumber),
                Routine = Ref(lowered.ExceptionOperation.Routine), Handler = Ref(lowered.ExceptionOperation.Handler),
                TryBlockAssociation = Ref(lowered.ExceptionOperation.TryBlock), TryExpression = Ref(lowered.ExceptionOperation.TryExpression),
                CatchExpression = Ref(lowered.ExceptionOperation.CatchExpression) };
            if (node is CleanupStateOperation state)
                row["CleanupState"] = new { Region = Ref(state.Region), StateNumber = Number(state.StateNumber), state.IsUnreachable };
        }
    }
}
