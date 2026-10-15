using System.Globalization;

namespace RoslynObj.CSharp;

internal sealed partial class InventoryState
{
    internal void InventoryUnit(RoslynObject obj, InputArtifact input, int ordinal, string hash)
    {
        var unit = obj.TranslationUnit;
        var prefix = $"unit/{ordinal:D4}";
        inputs.Add(new(ordinal, input.Path, hash, obj.Manifest.ArtifactIdentity,
            obj.Manifest.FormatMajor, obj.Manifest.FormatMinor, obj.Manifest.Producer, obj.Manifest.EdgRevision,
            obj.Manifest.EdgVersion, obj.Manifest.CaptureProfile, unit.LanguageMode,
            unit.Capabilities.Select(c => new CapturedCapability(c.Name, c.Value, c.IsRequired, c.Enabled, c.State.Name))
                .OrderBy(c => c.Name, StringComparer.Ordinal).ToArray(), unit.InvocationArguments.ToArray()));
        var typeList = unit.Types.ToArray();
        var typeNames = typeList.Select((type, index) => (type, name: $"{prefix}/type/{index + 1:D6}"))
            .ToDictionary(pair => pair.type, pair => pair.name);
        var functionList = unit.Functions.ToArray();
        var functionNames = functionList.Select((function, index) => (function, name: $"{prefix}/function/{index + 1:D6}"))
            .ToDictionary(pair => pair.function, pair => pair.name);
        var variableList = unit.Variables.ToArray();
        var variableNames = variableList.Select((variable, index) => (variable, name: $"{prefix}/storage/{index + 1:D6}"))
            .ToDictionary(pair => pair.variable, pair => pair.name);
        var requiredTypes = new HashSet<CppType>();
        var requiredConstants = new HashSet<Constant>();
        var retainedConstants = new HashSet<Constant>();
        var requiredValues = new HashSet<ConstantValue>();
        var retainedValues = new HashSet<ConstantValue>();
        var initializations = new HashSet<Initialization>();
        var executionInitializations = new HashSet<Initialization>();
        var cleanupInitializations = new HashSet<Initialization>();
        var metadataRecipes = new HashSet<Initialization>();
        var recipeLocators = new Dictionary<Initialization, string>();
        var cleanupChains = new HashSet<Initialization>();
        var lifetimes = new HashSet<Lifetime>();
        var scopes = new HashSet<Scope>();
        var usage = new Dictionary<Symbol, SymbolUsage>();
        var reports = new Dictionary<FunctionSymbol, FunctionReport>();
        var requiredBodies = new HashSet<FunctionSymbol>(functionList.Where(RequiredBody));
        long checkpoint = 0;

        string TypeName(CppType? type) => type is null ? "<absent>" : typeNames.TryGetValue(type, out var name)
            ? name : throw new InvalidOperationException("A type relationship is outside its translation unit's public type inventory.");
        string EntityName(SemanticEntity entity) => entity switch
        {
            FunctionSymbol function when functionNames.TryGetValue(function, out var name) => name,
            VariableSymbol variable when variableNames.TryGetValue(variable, out var name) => name,
            CppType type => TypeName(type),
            _ => entity.Name ?? "retained-entity"
        };
        void Tick() { if (++checkpoint % 256 == 0) CheckBudget(); }
        void RequireType(CppType? type)
        {
            if (type is null || !requiredTypes.Add(type)) return;
            switch (type)
            {
                case QualifiedType qualified: RequireType(qualified.UnderlyingType); break;
                case TypeReference reference: RequireType(reference.UnderlyingType); break;
                case PointerType pointer: RequireType(pointer.PointeeType); break;
                case ArrayType array: RequireType(array.ElementType); break;
                case RoutineType routine:
                    RequireType(routine.ReturnType);
                    foreach (var parameter in routine.Parameters) RequireType(parameter.Type);
                    break;
                case RecordType record:
                    foreach (var field in record.Fields) RequireType(field.Type);
                    foreach (var directBase in record.Bases) RequireType(directBase.BaseType);
                    break;
                case MemberPointerType member: RequireType(member.DeclaringType); RequireType(member.MemberType); break;
                case VectorType vector: RequireType(vector.ElementType); break;
            }
        }
        void Use(SemanticEntity? entity, string locator, bool address = false, bool directCall = false)
        {
            if (entity is not Symbol symbol) return;
            if (!usage.TryGetValue(symbol, out var used)) usage.Add(symbol, used = new());
            used.Required = true;
            if (address) used.AddressReferences++;
            if (directCall) used.DirectCalls++;
            RequireType(symbol.DeclaredType);
            Count("dependency", directCall ? "direct-call" : address ? "address" : "symbol-reference", true, locator);
        }
        void VisitValue(ConstantValue value, string locator, bool required)
        {
            Tick();
            var firstRetained = retainedValues.Add(value);
            var firstRequired = required && requiredValues.Add(value);
            if (!firstRetained && !firstRequired) return;
            Count("constant-value", value.Kind.Name, firstRequired, locator, firstRetained);
            switch (value)
            {
                case AddressValue address:
                    Count("address-constant", address.BaseKind.Name, firstRequired, locator, firstRetained);
                    if (firstRequired) Use(address.BaseEntity, locator, address: true);
                    if (address.BaseEntity is Constant baseConstant) VisitConstant(baseConstant, locator, required);
                    else if (address.BaseEntity is ConstantValue baseValue) VisitValue(baseValue, locator, required);
                    break;
                case AggregateValue aggregate:
                    foreach (var element in aggregate.Elements)
                    {
                        if (required) RequireType(element.Type);
                        VisitValue(element.Value, locator, required);
                    }
                    break;
                case ConstantValue<RepeatValueDetails> repeat:
                    if (required) RequireType(repeat.Payload.Type);
                    VisitValue(repeat.Payload.Value, locator, required);
                    break;
                case ConstantValue<MemberPointerValueDetails> member:
                    if (firstRequired) Use(member.Payload.Member, locator, address: true);
                    break;
                case LabelDifferenceValue difference:
                    VisitValue(difference.FromValue, locator, required);
                    VisitValue(difference.ToValue, locator, required);
                    break;
            }
        }
        void VisitConstant(Constant? constant, string locator, bool required)
        {
            if (constant is null) return;
            if (required && !requiredConstants.Add(constant)) return;
            if (!required && !retainedConstants.Add(constant)) return;
            retainedConstants.Add(constant);
            if (required)
            {
                RequireType(constant.Type);
                Use(constant.AssociatedStorageVariable, locator, address: true);
            }
            VisitValue(constant.Value, locator, required);
            foreach (var component in constant.Components) VisitConstant(component.Constant, locator, required);
            // BackingExpression and ContextualExpressions are retained metadata, never execution roots.
            if (constant.BackingExpression is not null) Count("constant-metadata", "backing-expression", false, locator);
        }
        void VisitInitialization(Initialization? initialization, string locator, RecipeContext context)
        {
            if (initialization is null) return;
            // Typed associations (for example a lowered throw's retained recipe) do not override
            // the node's recorded metadata role, even when the referring operation executes.
            if (context == RecipeContext.ExecutionRoot && initialization.Operation?.RepresentationRole.Name == "metadata")
                context = RecipeContext.Metadata;
            if (!recipeLocators.ContainsKey(initialization))
                recipeLocators.Add(initialization, prefix + $"/initialization/{recipeLocators.Count + 1:D8}");
            if (context == RecipeContext.Metadata)
            {
                if (!metadataRecipes.Add(initialization)) return;
                Count("initialization-metadata", initialization.Kind.Name, false, locator);
                VisitConstant(initialization.Constant, locator, false);
                VisitInitialization(initialization.Master, locator, RecipeContext.Metadata);
                VisitInitialization(initialization.NextInDestructionList, locator, RecipeContext.Metadata);
                return;
            }
            var contexts = context == RecipeContext.ExecutionRoot ? executionInitializations : cleanupInitializations;
            if (contexts.Add(initialization))
                Count(context == RecipeContext.ExecutionRoot ? "initialization-root" : "initialization-cleanup-dependency", initialization.Kind.Name, true, locator);
            if (!initializations.Add(initialization)) return;
            Tick();
            RequireType(initialization.Details.Type);
            Use(initialization.Target, locator);
            Use(initialization.Constructor, locator, directCall: true);
            Use(initialization.Destructor, locator, directCall: true);
            VisitConstant(initialization.Constant, locator, true);
            if (initialization.Operation is { RepresentationRole.Name: not "metadata" } operation)
                VisitOperations(operation, locator, true, context);
            else if (initialization.Operation is null && initialization.Expression is { RepresentationRole.Name: not "metadata" } expression)
                VisitOperations(expression, locator, true, context);
            // These associations add no evaluated edge. Actual cleanup lists below select live recipes.
            VisitInitialization(initialization.Master, locator + "/master", RecipeContext.Metadata);
            VisitInitialization(initialization.NextInDestructionList, locator + "/destruction", RecipeContext.Metadata);
            VisitLifetime(initialization.Lifetime, locator);
            VisitLifetime(initialization.InitExpressionLifetime, locator);
            VisitLifetime(initialization.OverlappingLifetime, locator);
        }
        void VisitCleanupChain(Initialization? initialization, string locator)
        {
            for (var current = initialization; current is not null && cleanupChains.Add(current); current = current.NextInDestructionList)
                VisitInitialization(current, locator, RecipeContext.CleanupDependency);
        }
        void VisitLifetime(Lifetime? lifetime, string locator)
        {
            if (lifetime is null || !lifetimes.Add(lifetime)) return;
            Tick();
            Count("lifetime", lifetime.Kind.Name, true, locator);
            foreach (var cleanup in lifetime.Cleanups)
            {
                Count("cleanup-recipe", cleanup.IsConditional ? "conditional" : cleanup.IsUnordered ? "unordered" : "ordered", true, locator);
                VisitInitialization(cleanup.Initialization, locator + $"/cleanup/{cleanup.Ordinal:D6}", RecipeContext.CleanupDependency);
            }
            foreach (var child in lifetime.Children) VisitLifetime(child, locator);
        }
        void VisitScope(Scope? scope, string locator)
        {
            if (scope is null || !scopes.Add(scope)) return;
            Tick();
            VisitLifetime(scope.Lifetime, locator);
            VisitLifetime(scope.FunctionStaticLifetime, locator);
            foreach (var initialization in scope.DynamicInitializations)
                VisitInitialization(initialization, locator, initialization.Operation?.RepresentationRole.Name == "metadata" ? RecipeContext.Metadata : RecipeContext.ExecutionRoot);
            foreach (var local in scope.LocalStaticInitializations)
            {
                Count("initialization", "local-static-guard", true, locator);
                VisitInitialization(local.Initialization, locator, local.Initialization?.Operation?.RepresentationRole.Name == "metadata" ? RecipeContext.Metadata : RecipeContext.ExecutionRoot);
                VisitConstant(local.Constant, locator, true);
                VisitLifetime(local.Lifetime, locator);
            }
            foreach (var constructor in scope.ConstructorInitializers)
                VisitInitialization(constructor.EffectiveInitialization, locator, constructor.EffectiveInitialization?.Operation?.RepresentationRole.Name == "metadata" ? RecipeContext.Metadata : RecipeContext.ExecutionRoot);
            // Nested routine/class/template scopes have their own execution classification.
            foreach (var child in scope.ChildScopesByParent)
                if (child.Kind.Name is not ("sck_function" or "sck_func_prototype" or "sck_class_struct_union" or "sck_class_reactivation"
                    or "sck_template_declaration" or "sck_template_instantiation" or "sck_instantiation_context")) VisitScope(child, locator);
        }
        long VisitOperations(SemanticNode root, string locator, bool required, RecipeContext recipeContext)
        {
            long count = 0;
            var directCallees = new HashSet<SemanticEntity>();
            var references = new List<(SemanticEntity? Target, SemanticEntity Node, string Locator)>();
            foreach (var operation in root.DescendantOperations(cancellationToken))
            {
                Tick();
                if (++count > request.Budgets.MaximumOperationsPerBody)
                    throw new ResourceBudgetException($"Per-body operation budget exceeded at {locator}.");
                var operationLocator = locator + $"/operation/{count:D8}";
                Count(operation.Kind.Family, operation.Kind.Name, required, operationLocator);
                if (required) RequireType(operation.Type);
                if (operation is OperatorOperation op) Count("operator", op.OperatorKind.Name, required, operationLocator);
                if (operation is LoweredEhOperation eh) Count("lowered-eh", eh.ExceptionKind.Name, required, operationLocator);
                if (operation is BuiltinOperation builtin) Count("builtin-operation", builtin.BuiltinKind.Name, required, operationLocator);
                if (operation is ExpressionOperation expression && expression.Details.VolatileFetch)
                    Count("extension", "volatile-fetch", required, operationLocator);
                if (operation is CallOperation call)
                {
                    Count("call", call.TargetMethod is null ? "indirect" : "direct", required, operationLocator);
                    if (required)
                    {
                        Use(call.TargetMethod, operationLocator, directCall: true);
                        RequireType(call.Call.FunctionType);
                        directCallees.Add(call.Callee);
                        if (call.TargetMethod?.Signature.IsVariadic == true || call.Call.FunctionType is RoutineType { IsVariadic: true })
                            Count("call", "variadic", true, operationLocator);
                    }
                }
                if (!required) continue;
                switch (operation)
                {
                    case ReferenceOperation reference:
                        references.Add((reference.Target, reference, operationLocator));
                        break;
                    case ConstantOperation constant: VisitConstant(constant.Constant, operationLocator, true); break;
                    case InitializationOperation initialization: VisitInitialization(initialization.Initialization, operationLocator, recipeContext); break;
                    case InitializationStatementOperation initialization: VisitInitialization(initialization.Initialization, operationLocator, recipeContext); break;
                    case StatementExpressionResultOperation result: VisitInitialization(result.Initialization, operationLocator, recipeContext); break;
                    case BlockOperation block: VisitLifetime(block.Lifetime, operationLocator); break;
                    case LabelOperation label: VisitLifetime(label.Lifetime, operationLocator); break;
                    case LifetimeOperation lifetime: VisitLifetime(lifetime.Lifetime, operationLocator); break;
                    case TemporaryOperation temporary: VisitInitialization(temporary.Initialization, operationLocator, recipeContext); break;
                    case ConditionOperation condition: VisitInitialization(condition.DynamicInitialization, operationLocator, recipeContext); break;
                    case TryOperation statement:
                        Count("exception", "try", true, operationLocator);
                        VisitLifetime(statement.Definition.Lifetime, operationLocator);
                        foreach (var handler in statement.Handlers)
                        {
                            Count("exception", "catch", true, operationLocator);
                            RequireType(handler.CaughtType);
                            VisitInitialization(handler.ParameterInitialization, operationLocator, recipeContext);
                        }
                        break;
                    case ThrowOperation thrown:
                        Count("exception", thrown.IsRethrow ? "rethrow" : "throw", true, operationLocator);
                        RequireType(thrown.ThrownType);
                        Use(thrown.Destructor, operationLocator, directCall: true);
                        VisitInitialization(thrown.Initialization, operationLocator, recipeContext);
                        break;
                    case AllocationOperation allocation:
                        Use(allocation.Allocator, operationLocator, directCall: true);
                        Use(allocation.Deallocator, operationLocator, directCall: true);
                        VisitInitialization(allocation.Initialization, operationLocator, recipeContext);
                        VisitInitialization(allocation.ExceptionFreeing, operationLocator, RecipeContext.CleanupDependency);
                        break;
                    case LoweredEhOperation lowered:
                        VisitInitialization(lowered.Initialization, operationLocator, RecipeContext.Metadata);
                        if (lowered.CleanupStart is { } cleanupStart) VisitCleanupChain(cleanupStart, operationLocator);
                        break;
                }
            }
            foreach (var reference in references)
                Use(reference.Target, reference.Locator, address: reference.Target is FunctionSymbol && !directCallees.Contains(reference.Node));
            return count;
        }

        foreach (var function in functionList)
        {
            Tick();
            var locator = functionNames[function];
            var required = requiredBodies.Contains(function);
            Count("body-state", function.BodyState.Name, required, locator);
            var signature = Signature(function.Signature, TypeName);
            if (required) RequireType(function.Signature);
            long operations = 0;
            if (function.Body is { } body)
            {
                operations = VisitOperations(body.Root, locator, required, required ? RecipeContext.ExecutionRoot : RecipeContext.Metadata);
                if (required)
                {
                    VisitScope(body.Scope, locator);
                    foreach (var lifetime in body.Lifetimes)
                    {
                        Count("retained-lifetime", lifetime.Kind.Name, false, locator);
                        if (lifetime.BoundEntity == function) VisitLifetime(lifetime, locator);
                    }
                    var eh = body.ExceptionHandling;
                    if (eh.Details is { } details)
                    {
                        Count("exception", "function-eh", true, locator);
                        if (details.Prologue is { } prologue) VisitOperations(prologue, locator + "/eh-prologue", true, RecipeContext.ExecutionRoot);
                    }
                    Use(eh.RegionTable, locator);
                    Use(eh.ArrayTable, locator);
                    foreach (var epilogue in eh.Epilogues) VisitOperations(epilogue, locator + "/eh-epilogue", true, RecipeContext.CleanupDependency);
                    foreach (var region in eh.CleanupRegions)
                    {
                        Count("exception", "cleanup-region", true, locator);
                        if (region.Condition is { } condition)
                        {
                            Count("cleanup-condition", condition.Kind.Name, true, locator);
                            Use(condition.Storage.Variable, locator);
                        }
                        foreach (var action in region.Actions)
                        {
                            Count("cleanup-action", action.Kind.Name, true, locator);
                            Use(action.TargetMethod, locator, directCall: true);
                            Use(action.Storage?.Variable, locator);
                            Use(action.AbiDetail?.VttLocation?.Variable, locator);
                            Use(action.AbiDetail?.VlaCountLocation?.Variable, locator);
                        }
                    }
                    foreach (var array in eh.Arrays)
                    {
                        Count("exception", "cleanup-array", true, locator);
                        VisitConstant(array.HandleConstant, locator, true);
                        foreach (var location in array.StorageLocations) Use(location.Variable, locator);
                        foreach (var location in array.CountLocations) Use(location.Variable, locator);
                    }
                }
            }
            else if (function.BodyState.Name == "definition")
                Error("ROC1400", "A recorded executable definition has no retained body.", input.Path, locator, "body");
            if (function.Signature.IsVariadic) Count("signature", "variadic", required, locator);
            var independentAlternateEntry = function.PrimaryEntry is not null && request.Inputs.Length == 1
                && DefinedAlternateEntryPolicy.HasIndependentBody(function, functionList);
            if (function.GnuSupplement?.AliasedRoutine is not null || function.PrimaryEntry is not null && !independentAlternateEntry
                || function.Details.IsWeak == true || function.Details.IsIfunc == true)
                Count("link-policy", "alias-weak-ifunc-alternate-entry", required, locator);
            if (independentAlternateEntry)
                Count("signature", "defined-constructor-destructor-alternate-entry", required, locator);
            reports.Add(function, new(locator, function.Name, function.CurrentName, LinkageName(function),
                function.SourceCorrespondence?.Linkage.Name, function.IsDefined, function.BodyState.Name, required,
                required ? "executable-definition" : function.Body is not null ? "retained-nonexecuting-body" : "no-executable-body",
                signature, operations, function.BuiltinKind?.Name, false));
        }
        foreach (var variable in variableList)
        {
            Tick();
            var locator = variableNames[variable];
            var details = variable.Details;
            var enclosing = variable.SourceCorrespondence?.EnclosingRoutine;
            var external = ExternalVariable(variable);
            var metadata = details.IsNonreal || details.IsPrototypeInstantiation || details.IsTemplateVariable;
            var required = !metadata && !external && (enclosing is null || requiredBodies.Contains(enclosing));
            var classification = metadata ? "metadata-only" : details.IsParameter ? "parameter"
                : external ? "external-declaration-recorded" : enclosing is not null ? "local-definition" : "storage-definition";
            Count("storage", classification, required, locator);
            Count("storage-duration", details.StorageDuration?.Name ?? "not-retained", required, locator);
            Count("variable-initialization", variable.InitializationKind.Name, required, locator);
            if (required)
            {
                RequireType(variable.Type);
                VisitConstant(variable.ConstantInitializer, locator, true);
                VisitInitialization(variable.Initializer, locator, RecipeContext.Metadata);
                Use(variable.CleanupRoutine, locator, directCall: true);
                Use(details.DynamicInitializationRoutine, locator, directCall: true);
                Use(details.ThreadInitializationRoutine, locator, directCall: true);
                Use(details.ThreadWrapperRoutine, locator, directCall: true);
                if (variable.IsThreadLocal) Count("initialization", "thread-local", true, locator);
                foreach (var local in variable.LocalStaticInitializations)
                {
                    Count("initialization", "local-static-guard", true, locator);
                    VisitInitialization(local.Initialization, locator, RecipeContext.Metadata);
                    VisitConstant(local.Constant, locator, true);
                    VisitLifetime(local.Lifetime, locator);
                }
            }
            if (details.IsWeak || details.IsCommon || details.IsGnuAlias || details.IsWeakref)
                Count("link-policy", "weak-common-alias-variable", required, locator);
            storage.Add(new(locator, variable.Name, LinkageName(variable), variable.SourceCorrespondence?.Linkage.Name,
                TypeName(variable.Type), classification, details.StorageDuration?.Name, details.DeclaredStorageClass?.Name,
                variable.InitializationKind.Name, variable.Initializer is not null, variable.ConstantInitializer is not null,
                variable.IsThreadLocal, details.AddressTaken, details.IsWeak, details.IsCommon, details.IsGnuAlias)
                { CurrentStorage = variable.SymbolDetails.Storage?.Name });
        }
        VisitScope(unit.GlobalScope, prefix + "/global-scope");
        foreach (var root in unit.Roots)
        {
            Tick();
            var locator = prefix + $"/root/{root.Kind.Name}/{root.Ordinal:D6}";
            roots.Add(new(locator, root.Kind.Name, EntityName(root.Entity), root.Ordinal, false));
            var executableRoot = root.Kind.Name is "file_scope_statement" or "file_initializer";
            Count("translation-unit-root", root.Kind.Name, executableRoot, locator);
            if (!executableRoot) continue;
            switch (root.Entity)
            {
                case Initialization initialization: VisitInitialization(initialization, locator, RecipeContext.ExecutionRoot); break;
                case Lifetime lifetime: VisitLifetime(lifetime, locator); break;
                case VariableSymbol variable when !ExternalVariable(variable): VisitConstant(variable.ConstantInitializer, locator, true); break;
                case SemanticNode node when node.RepresentationRole.Name != "metadata": VisitOperations(node, locator, true, RecipeContext.ExecutionRoot); break;
            }
        }
        foreach (var root in unit.FileScopeInitializationRoutines)
        {
            var locator = prefix + $"/file-initialization/{root.Ordinal:D6}";
            roots.Add(new(locator, "file-scope-routine", EntityName(root.Routine), root.Ordinal, root.IsThreadLocal));
            Use(root.Routine, locator, directCall: true);
            Count("initialization", root.IsThreadLocal ? "file-thread-routine" : "file-routine", true, locator);
        }
        long constantOrdinal = 0;
        foreach (var constant in unit.Constants)
        {
            Tick();
            // Every retained constant is inventoried, including non-executing recipe constants.
            VisitConstant(constant, prefix + $"/constant/{++constantOrdinal:D8}", false);
        }
        foreach (var type in typeList)
        {
            Tick();
            var locator = TypeName(type);
            var required = requiredTypes.Contains(type);
            Count("type", type.Kind.Name, required, locator);
            if (type.Details.IsAtomic) Count("type", "atomic", required, locator);
            if (type.Details.UsedInExceptionOrRtti) Count("type", "eh-rtti", required, locator);
            if (type.Details.IsBuiltinVaList) Count("type", "builtin-va-list", required, locator);
            var fields = type is RecordType ? type.Fields.ToArray() : [];
            var members = type.Layout?.Fields.Select(field => new TypeMemberReport(field.Ordinal, field.Field.Name,
                TypeName(field.Field.Type), Number(field.Offset), Number(field.BitOffset), Number(field.BitWidth), Number(field.ContainerOffset))).ToArray() ?? [];
            var bitFields = members.LongCount(field => field.BitWidth is not null);
            if (bitFields != 0) Count("layout", "bitfield", required, locator);
            types.Add(new(locator, type.Name, type.Kind.Name, Number(type.Size), Number(type.Alignment), type.IsIncomplete,
                type.IsDependent, DescribeType(type, TypeName), fields.LongLength, bitFields, type.Bases.Count) { Members = members });
        }
        foreach (var function in functionList)
        {
            var report = reports[function];
            usage.TryGetValue(function, out var used);
            if (function.BuiltinKind is { } builtin) Count("builtin", builtin.Name, used?.Required == true, report.Locator);
            report = report with { AddressExposed = function.Details.AddressTaken || used?.AddressReferences > 0 };
            functions.Add(report);
            if (!report.RequiredBody && function.BodyState.Name is "declaration_only" or "unavailable")
                AddImport(report.Locator, "function", report.Name, report.LinkageName, report.Signature.Display,
                    report.Signature.CallingConvention, used, function.Details.AddressTaken);
        }
        foreach (var variable in variableList)
            if (ExternalVariable(variable))
            {
                usage.TryGetValue(variable, out var used);
                AddImport(variableNames[variable], "variable", variable.Name, LinkageName(variable), TypeName(variable.Type), null, used, variable.Details.AddressTaken);
            }
        foreach (var recipe in recipeLocators)
            recipes.Add(new(recipe.Value, recipe.Key.Kind.Name, recipe.Key.Target is { } target ? EntityName(target) : null,
                recipe.Key.Constructor is { } constructor ? EntityName(constructor) : null,
                recipe.Key.Destructor is { } destructor ? EntityName(destructor) : null,
                executionInitializations.Contains(recipe.Key), cleanupInitializations.Contains(recipe.Key),
                metadataRecipes.Contains(recipe.Key), recipe.Key.Operation?.RepresentationRole.Name));
        CheckBudget();
    }

    private void AddImport(string locator, string kind, string? name, string? linkageName, string signature,
        string? callingConvention, SymbolUsage? usage, bool capturedAddressTaken)
    {
        var binding = request.Runtime.Bindings.FirstOrDefault(b => b.Kind == kind && b.LinkageName == linkageName);
        var status = binding is null ? "unbound" : binding.Signature != signature || binding.CallingConvention != (callingConvention ?? "")
            ? "contract-mismatch" : string.IsNullOrWhiteSpace(binding.Ownership) || string.IsNullOrWhiteSpace(binding.ErrorContract)
            || string.IsNullOrWhiteSpace(binding.ThreadContract) || string.IsNullOrWhiteSpace(binding.CallbackContract)
                ? "incomplete-contract" : "declared-unverified";
        imports.Add(new(locator, kind, name, linkageName, signature, callingConvention, usage?.Required == true,
            usage?.DirectCalls ?? 0, usage?.AddressReferences ?? 0, capturedAddressTaken || usage?.AddressReferences > 0,
            binding?.Provider, binding?.Symbol, binding?.Signature, binding?.CallingConvention, binding?.Ownership,
            binding?.ErrorContract, binding?.ThreadContract, binding?.CallbackContract, status));
        Count("import", kind + ":" + status, usage?.Required == true, locator);
    }

    private static bool RequiredBody(FunctionSymbol function) => function.BodyState.Name == "definition"
        && function.Body is { Root.RepresentationRole.Name: not "metadata" };
    private static bool ExternalVariable(VariableSymbol variable) => variable.SymbolDetails.Storage?.Name == "sc_extern";
    private static string? LinkageName(Symbol symbol) => symbol switch
    {
        FunctionSymbol function => function.GnuSupplement?.AssemblyName ?? function.CurrentName,
        VariableSymbol variable => variable.AssemblyName ?? variable.CurrentName,
        _ => symbol.CurrentName
    };
    private static string? Number(System.Numerics.BigInteger? value) => value?.ToString(CultureInfo.InvariantCulture);
    private static SignatureReport Signature(RoutineType routine, Func<CppType?, string> name)
    {
        var parameters = routine.Parameters.Select(p => new SignatureParameterReport(p.Ordinal, p.Name, name(p.Type), p.Role.Name)).ToArray();
        var display = name(routine.ReturnType) + "(" + string.Join(",", parameters.Select(p => p.Type + " " + p.Role))
            + (routine.IsVariadic ? ",..." : "") + ") cc=" + (routine.CallingConvention?.Name ?? "not-retained")
            + " linkage=" + (routine.LanguageLinkage?.Name ?? "not-retained")
            + " result-parameter=" + (routine.ReturnsValueAsParameter?.ToString() ?? "not-retained");
        return new(display, name(routine.ReturnType), parameters, routine.CallingConvention?.Name, routine.LanguageLinkage?.Name,
            routine.IsVariadic, routine.HasPrototype, routine.ReturnsValueAsParameter, routine.ReturnsValueViaCopyConstructor);
    }
    private static string DescribeType(CppType type, Func<CppType?, string> name) => type switch
    {
        ArithmeticType arithmetic => $"{arithmetic.Arithmetic.ArithmeticKind.Name};bits={arithmetic.Arithmetic.Bits};signed={arithmetic.Arithmetic.Signed};precision={arithmetic.Arithmetic.Precision};radix={arithmetic.Arithmetic.Radix};scale={arithmetic.Arithmetic.Scale};integer={arithmetic.Arithmetic.EdgIntegerKind?.Name};float={arithmetic.Arithmetic.EdgFloatKind?.Name}",
        QualifiedType qualified => $"underlying={name(qualified.UnderlyingType)};const={qualified.IsConst};volatile={qualified.IsVolatile}",
        TypeReference reference => $"underlying={name(reference.UnderlyingType)}",
        PointerType pointer => $"pointee={name(pointer.PointeeType)};reference={pointer.IsReference};rvalue-reference={pointer.IsRValueReference}",
        ArrayType array => $"element={name(array.ElementType)};bound-kind={array.Array.BoundKind.Name};bound={Number(array.Bound)};vla={array.VlaDimension is not null}",
        RoutineType routine => Signature(routine, name).Display,
        RecordType record => "bases=[" + string.Join(",", record.Bases.Select(b => $"{name(b.BaseType)}:virtual={b.IsVirtual}")) + "]",
        MemberPointerType member => $"declaring={name(member.DeclaringType)};member={name(member.MemberType)}",
        VectorType vector => $"element={name(vector.ElementType)}",
        _ => type.Kind.Name
    };
    private sealed class SymbolUsage
    {
        internal bool Required;
        internal long DirectCalls;
        internal long AddressReferences;
    }
    private enum RecipeContext { ExecutionRoot, CleanupDependency, Metadata }
}
