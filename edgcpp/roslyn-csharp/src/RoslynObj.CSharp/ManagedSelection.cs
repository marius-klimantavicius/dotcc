using RoslynObj;

namespace RoslynObj.CSharp;

/// <summary>Current-reader identities only. This plan must not outlive or be rebound to another reader.</summary>
internal sealed record ManagedSelectionPlan(
    RoslynObject Source,
    IReadOnlyList<FunctionSymbol> Functions,
    IReadOnlyList<VariableSymbol> Variables,
    IReadOnlyList<CppType> Types,
    IReadOnlyList<Constant> Constants,
    IReadOnlyList<FunctionSymbol> Definitions,
    IReadOnlyList<VariableSymbol> StorageDefinitions,
    ManagedSelectionReport Report)
{
    internal IReadOnlyList<FunctionSymbol> StartupDefinitions { get; init; } = [];
    internal IReadOnlyList<FunctionSymbol> CallableDefinitions { get; init; } = [];
}

internal sealed class ManagedSelectionException(string code, string message, string? locator = null) : Exception(message)
{
    internal string Code { get; } = code;
    internal string? Locator { get; } = locator;
}

/// <summary>Bounded public dependency discovery, not an execution schedule or emission admission.</summary>
internal static class ManagedSelectionPlanner
{
    internal static ManagedSelectionPlan Select(RoslynObject obj, ManagedDefinitionRoot[] roots, ResourceBudgets budgets,
        CancellationToken token = default, Action? checkBudget = null, ManagedStartupConfiguration? startup = null)
        => new Work(obj, roots, budgets, token, checkBudget, startup: startup).Run();

    internal static ManagedSelectionPlan SelectSeeded(RoslynObject obj, ManagedDefinitionRoot[] roots,
        IReadOnlyList<FunctionSymbol> additionalFunctions, IReadOnlyList<VariableSymbol> additionalStorage,
        string inputSha256, int unitOrdinal, ResourceBudgets budgets, CancellationToken token = default, Action? checkBudget = null,
        ManagedStartupConfiguration? startup = null)
        => new Work(obj, roots, budgets, token, checkBudget, additionalFunctions, additionalStorage, inputSha256, unitOrdinal, startup).Run();

    private sealed class Work(RoslynObject obj, ManagedDefinitionRoot[] roots, ResourceBudgets budgets,
        CancellationToken token, Action? checkBudget, IReadOnlyList<FunctionSymbol>? additionalFunctions = null,
        IReadOnlyList<VariableSymbol>? additionalStorage = null, string? inputSha256 = null, int unitOrdinal = 1,
        ManagedStartupConfiguration? startup = null)
    {
        private readonly List<FunctionSymbol> functions = [];
        private readonly List<VariableSymbol> variables = [];
        private readonly List<CppType> types = [];
        private readonly List<Constant> constants = [];
        private readonly HashSet<FunctionSymbol> functionSet = [];
        private readonly HashSet<VariableSymbol> variableSet = [];
        private readonly HashSet<CppType> typeSet = [];
        private readonly HashSet<CppType> valueTypeSet = [];
        private readonly HashSet<Constant> constantSet = [];
        private readonly HashSet<ConstantValue> values = [];
        private readonly HashSet<Initialization> recipes = [];
        private readonly HashSet<Initialization> cleanupRecipes = [];
        private readonly HashSet<Lifetime> lifetimes = [];
        private readonly HashSet<Scope> scopes = [];
        private readonly HashSet<VariableSymbol> locals = [];
        private readonly HashSet<SemanticNode> executionVisited = [];
        private readonly HashSet<SemanticNode> requiredInitializerOccurrences = [];
        private readonly Queue<FunctionSymbol> pendingFunctions = new();
        private readonly Queue<VariableSymbol> pendingVariables = new();
        private readonly Dictionary<SemanticEntity, string> locators = [];
        private readonly List<ManagedRootSelectionReport> selectedRoots = [];
        private readonly List<ManagedRootSelectionReport> selectedStartupRoots = [];
        private readonly List<FunctionSymbol> startupDefinitions = [];
        private readonly List<FunctionSymbol> callableDefinitions = [];
        private readonly List<ManagedSelectionDependencyReport> dependencies = [];
        private long workCount;
        private long bodyOperationCount;
        private long ambientRootRowsInspected;
        private int ambientRoutineCandidatesInspected;
        private int bodiesVisited;

        internal ManagedSelectionPlan Run()
        {
            var admittedHash = inputSha256 ?? roots.FirstOrDefault()?.InputSha256 ?? startup?.UnitInputOrder.SingleOrDefault();
            var startupRoots = startup?.Definitions.Where(root => root.InputSha256.Equals(admittedHash, StringComparison.OrdinalIgnoreCase)).ToArray() ?? [];
            var allRoots = roots.Concat(startupRoots).ToArray();
            if (startup is not null && (startup.Contract != "edg7-itanium-explicit-startup-atexit-v1" || startup.Definitions.Length == 0 ||
                startup.UnitInputOrder.Count(hash => hash.Equals(admittedHash, StringComparison.OrdinalIgnoreCase)) != 1))
                Fail("ROC1210", "Managed startup requires its admitted policy and exactly one current input in the explicit target order.");
            if (allRoots.Length == 0 && admittedHash is null || budgets.MaximumSelectionCandidates < 1 || budgets.MaximumSelectionCandidates == int.MaxValue || budgets.MaximumAmbientRoutineCandidates is < 1 or > 256 || budgets.MaximumSelectedFunctions < 1 || budgets.MaximumSelectedStorage < 1 ||
                budgets.MaximumSelectionOperationsPerBody < 1 || budgets.MaximumSelectedTypes < 1 || budgets.MaximumSelectionDepth < 1)
                Fail("ROC1210", "Managed definition selection requires roots and positive explicit closure bounds.");
            if (allRoots.Any(root => root.InputSha256.Length != 64 || !root.InputSha256.All(Uri.IsHexDigit) ||
                string.IsNullOrWhiteSpace(root.LookupName) || string.IsNullOrWhiteSpace(root.LinkageName)) ||
                allRoots.Select(root => root.InputSha256).Distinct(StringComparer.OrdinalIgnoreCase).Count() > 1
                || admittedHash is not null && allRoots.Any(root => !root.InputSha256.Equals(admittedHash, StringComparison.OrdinalIgnoreCase))
                || unitOrdinal < 1)
                Fail("ROC1210", "Managed roots require the one admitted input hash, indexed lookup name and exact captured linkage.");
            foreach (var root in startupRoots)
            {
                var selected = ResolveRoot(root);
                if (startupDefinitions.Contains(selected)) Fail("ROC1210", "One captured startup definition is listed more than once.");
                startupDefinitions.Add(selected);
            }
            CheckAmbientInitialization();
            foreach (var root in roots)
            {
                var selected = ResolveRoot(root);
                if (startupDefinitions.Contains(selected)) Fail("ROC1210", "A startup definition cannot also be a callable managed root.");
                UseFunction(selected, "managed-root", "root");
                callableDefinitions.Add(selected);
                selectedRoots.Add(new(root.LookupName, root.LinkageName, selected.Name, Signature(selected), Locator(selected)));
            }
            for (var index = 0; index < startupDefinitions.Count; ++index)
            {
                var selected = startupDefinitions[index]; var root = startupRoots[index];
                UseFunction(selected, "managed-startup", "ordered-startup-root");
                selectedStartupRoots.Add(new(root.LookupName, root.LinkageName, selected.Name, Signature(selected), Locator(selected)));
            }
            foreach (var function in additionalFunctions ?? []) UseFunction(function, "selected-program", "bound-definition");
            foreach (var variable in additionalStorage ?? []) UseVariable(variable, "selected-program", "bound-storage");
            while (pendingFunctions.Count != 0 || pendingVariables.Count != 0)
            {
                Tick();
                if (pendingFunctions.TryDequeue(out var function)) VisitFunction(function);
                else VisitVariable(pendingVariables.Dequeue());
            }
            if (requiredInitializerOccurrences.Any(node => !executionVisited.Contains(node)))
                Fail("ROC1214", "A required executable initializer has no selected typed execution occurrence; association-only initializer execution remains unsupported.");
            checkBudget?.Invoke();
            var definitions = functions.Where(function => function.BodyState.Name == "definition" && function.Body is not null).ToArray();
            var storage = variables.Where(IsStorageDefinition).ToArray();
            var report = new ManagedSelectionReport
            {
                InputArtifactIdentity = obj.Manifest.ArtifactIdentity, InputSha256 = admittedHash!.ToLowerInvariant(),
                ClosureComplete = true, WholeInputInventoryComplete = false, WholeInputTranslationComplete = false,
                RelatedTypeMetadataComplete = false, ExecutableBodiesVisited = bodiesVisited,
                RequiredDeclarationCount = functions.Count - definitions.Length, StorageDefinitionCount = storage.Length,
                RequiredVariableCount = variables.Count, ScopedTypeCount = types.Count, RequiredConstantCount = constants.Count,
                BodyOperationCount = bodyOperationCount, SelectionWorkCount = workCount, AmbientRootRowsInspected = ambientRootRowsInspected,
                AmbientRoutineCandidatesInspected = ambientRoutineCandidatesInspected,
                AmbientInitialization = startup is null
                    ? "Required ambient translation-unit initialization/teardown is checked and unsupported execution is rejected, never erased."
                    : "Exact current startup roots cover the admitted backend candidates in explicit target input order. Their executable closure is selected; generated explicit initialization and registered teardown still require emitter admission and separate execution evidence.",
                Roots = selectedRoots.ToArray(), StartupRoots = selectedStartupRoots.ToArray(), StartupExecutionContract = startup?.Contract,
                Dependencies = dependencies.ToArray()
            };
            return new(obj, functions.ToArray(), variables.ToArray(), types.ToArray(), constants.ToArray(), definitions, storage, report)
            { StartupDefinitions = startupDefinitions.ToArray(), CallableDefinitions = callableDefinitions.ToArray() };
        }

        private FunctionSymbol ResolveRoot(ManagedDefinitionRoot root)
        {
            Tick();
            var candidates = obj.TranslationUnit.SemanticModel.FindSymbols(root.LookupName).Take(budgets.MaximumSelectionCandidates + 1).ToArray();
            if (candidates.Length > budgets.MaximumSelectionCandidates) Fail("ROC1211", "Managed root indexed candidate budget exceeded; uniqueness is unknown.");
            var selected = candidates.OfType<FunctionSymbol>().Where(function =>
                (function.GnuSupplement?.AssemblyName ?? function.CurrentName) == root.LinkageName && function.IsDefined &&
                function.BodyState.Name == "definition" && function.Body is { Root.RepresentationRole.Name: "execution" }).Distinct().ToArray();
            if (selected.Length != 1) Fail("ROC1211", "A managed root must select one actual defined public function by its current captured linkage.");
            return selected[0];
        }

        private void CheckAmbientInitialization()
        {
            Tick();
            var unit = obj.TranslationUnit;
            // Selected backend naming contract, not C++ source lookup. In this
            // pinned EDG native profile, default startup routines are not in the
            // separate-routine header list. Their bodies contain any associated
            // __cxa_atexit registration; GNU/TLS entry flags are checked too.
            var ambient = unit.SemanticModel.FindAmbientRoutineCandidates("__sti__", budgets.MaximumAmbientRoutineCandidates, token);
            checkBudget?.Invoke();
            ambientRoutineCandidatesInspected = ambient.Count;
            if (ambient.Count > budgets.MaximumAmbientRoutineCandidates)
                Fail("ROC1213", "Ambient routine discovery candidate budget exceeded; schedule completeness is unknown.");
            if (startup is null && ambient.Count != 0)
                Fail("ROC1212", "Managed selection requires an explicit ambient startup/teardown schedule for captured routine '" +
                    ambient[0].CurrentName + "'; EDG backend startup, source constructor/finalizer and TLS entries cannot be omitted.");
            if (startup is not null)
            {
                foreach (var candidate in ambient)
                {
                    Tick();
                    if (candidate.CurrentName?.StartsWith("__sti__", StringComparison.Ordinal) != true ||
                        candidate.Details.IsInitializationRoutine == true || candidate.Details.IsFinalizationRoutine == true ||
                        candidate.Details.IsTlsInitializationRoutine == true || candidate.Details.IsTlsInitializationAlias == true ||
                        !startupDefinitions.Contains(candidate))
                        Fail("ROC1212", "An actual ambient routine is not covered by the admitted explicit default-backend startup schedule: " + candidate.CurrentName);
                }
                if (startupDefinitions.Any(function => !ambient.Contains(function)))
                    Fail("ROC1212", "An explicit startup root is not an actual default-backend ambient candidate in this current input.");
            }
            if (unit.FileScopeInitializationRoutines.Take(1).Any() || unit.GlobalScope.Lifetime?.Cleanups.Take(1).Any() == true)
                Fail("ROC1212", "Managed selection cannot omit ambient translation-unit initialization or teardown; its execution schedule is unsupported.");
            foreach (var initialization in unit.GlobalScope.DynamicInitializations)
            {
                Tick();
                if (startup is null || !ManagedAmbientPolicy.IsInertDescriptor(initialization))
                    Fail("ROC1212", "An ambient initialization or cleanup recipe has no admitted explicit execution schedule.");
            }
            foreach (var root in unit.Roots)
            {
                Tick();
                ++ambientRootRowsInspected;
                if (root.Kind.Name is "file_scope_statement" or "file_initializer")
                    Fail("ROC1212", "Managed selection cannot omit ambient translation-unit initialization or teardown; its execution schedule is unsupported.");
            }
        }

        private void Tick(int depth = 0)
        {
            token.ThrowIfCancellationRequested();
            if (depth > budgets.MaximumSelectionDepth) Fail("ROC1213", "Managed dependency depth bound exceeded.");
            // Also bound repeated metadata edges/constants independently of
            // type identity sharing. No bound failure returns a partial plan.
            var limit = (long)budgets.MaximumSelectionOperationsPerBody * budgets.MaximumSelectedFunctions +
                budgets.MaximumSelectedStorage + budgets.MaximumSelectedTypes;
            if (++workCount > limit) Fail("ROC1213", "Managed dependency work bound exceeded.");
            if ((workCount & 63) == 1) checkBudget?.Invoke();
        }

        private string Locator(SemanticEntity entity)
        {
            if (locators.TryGetValue(entity, out var value)) return value;
            value = entity switch
            {
                FunctionSymbol => $"unit/{unitOrdinal:D4}/function/{functions.Count:D6}",
                VariableSymbol => $"unit/{unitOrdinal:D4}/storage/{variables.Count:D6}",
                CppType => $"unit/{unitOrdinal:D4}/type/{types.Count:D6}",
                Constant => $"unit/{unitOrdinal:D4}/constant/{constants.Count:D8}",
                _ => $"selection/association/{locators.Count + 1:D8}"
            };
            locators.Add(entity, value);
            return value;
        }

        private void Edge(string from, string kind, SemanticEntity to)
        {
            Tick();
            dependencies.Add(new(from, kind, Locator(to)));
        }

        private void UseFunction(FunctionSymbol function, string from, string kind)
        {
            if (functionSet.Add(function))
            {
                if (functions.Count >= budgets.MaximumSelectedFunctions) Fail("ROC1213", "Managed required-function bound exceeded.", from);
                functions.Add(function); _ = Locator(function); pendingFunctions.Enqueue(function);
            }
            Edge(from, kind, function);
        }

        private void UseVariable(VariableSymbol variable, string from, string kind)
        {
            if (variableSet.Add(variable))
            {
                if (variables.Count >= budgets.MaximumSelectedStorage) Fail("ROC1213", "Managed required-variable/storage bound exceeded.", from);
                variables.Add(variable); _ = Locator(variable); pendingVariables.Enqueue(variable);
            }
            Edge(from, kind, variable);
        }

        private void Use(SemanticEntity? entity, string from, string kind, int depth = 0)
        {
            Tick(depth);
            switch (entity)
            {
                case null: return;
                case FunctionSymbol function: UseFunction(function, from, kind); return;
                case VariableSymbol variable: UseVariable(variable, from, kind); return;
                case FieldSymbol field:
                    RequireType(field.Type, false, depth + 1);
                    RequireType(field.ContainingType, false, depth + 1);
                    Edge(from, kind, field); return;
                case Constant constant: VisitConstant(constant, from, depth + 1); return;
                case ConstantValue value: VisitValue(value, from, depth + 1); return;
                case CppType type: RequireType(type, false, depth + 1); return;
                case LabelSymbol: return; // Function-local control flow, not a new symbol definition.
                default: Fail("ROC1214", "A required dependency entity has no supported public selection rule.", from); return;
            }
        }

        private void RequireType(CppType? type, bool storage, int depth = 0)
        {
            if (type is null) return;
            Tick(depth);
            var first = typeSet.Add(type);
            if (first)
            {
                if (types.Count >= budgets.MaximumSelectedTypes) Fail("ROC1213", "Managed scoped-type bound exceeded.");
                types.Add(type); _ = Locator(type);
            }
            if (!first && (!storage || !valueTypeSet.Add(type))) return;
            if (storage) valueTypeSet.Add(type);
            switch (type)
            {
                case TypeReference reference: RequireType(reference.UnderlyingType, storage, depth + 1); break;
                case PointerType pointer:
                    // Only the pointee's representation association is required.
                    // It never selects its fields, methods, vtables or typeinfo.
                    RequireType(pointer.PointeeType, false, depth + 1); break;
                case ArrayType array: RequireType(array.ElementType, storage, depth + 1); break;
                case RoutineType routine:
                    RequireType(routine.ReturnType, true, depth + 1);
                    foreach (var parameter in routine.Parameters)
                    { RequireType(parameter.Type, true, depth + 1); RequireType(parameter.DeclaredType, false, depth + 1); }
                    break;
                case RecordType record when storage:
                    foreach (var field in record.Layout?.Fields ?? []) RequireType(field.Field.Type, true, depth + 1);
                    break;
                case MemberPointerType member:
                    RequireType(member.DeclaringType, false, depth + 1); RequireType(member.MemberType, false, depth + 1); break;
                case VectorType vector: RequireType(vector.ElementType, storage, depth + 1); break;
            }
        }

        private string Signature(FunctionSymbol function)
        {
            RequireType(function.Signature, false);
            var signature = function.Signature;
            return $"return={Locator(signature.ReturnType)}; parameters=[" +
                string.Join(",", signature.Parameters.Select(parameter => $"{parameter.Ordinal}:{parameter.Role.Name}:{Locator(parameter.Type)}")) +
                $"]; this={signature.HasThisParameter}; prototype={signature.HasPrototype}; variadic={signature.IsVariadic}; cc={signature.CallingConvention?.Name}; linkage={signature.LanguageLinkage?.Name}";
        }

        private void VisitFunction(FunctionSymbol function)
        {
            var locator = Locator(function);
            RequireType(function.Signature, false);
            foreach (var parameter in function.Parameters)
            {
                RequireType(parameter.Type, true);
                RequireType(parameter.DeclaredType, false);
                if (parameter.Symbol is { } variable) UseVariable(variable, locator, "execution-parameter");
            }
            if (function.BodyState.Name != "definition")
            {
                if (function.IsDefined && function.BodyState.Name == "unavailable")
                    Fail("ROC1214", "A selected defined routine has no available executable body.", locator);
                return;
            }
            var body = function.Body ?? throw new ManagedSelectionException("ROC1214", "A selected definition has no retained body.", locator);
            ++bodiesVisited;
            var eh = body.ExceptionHandling;
            if (eh.Details is not null || eh.RegionTable is not null || eh.ArrayTable is not null ||
                eh.CleanupRegions.Count != 0 || eh.Arrays.Count != 0 || eh.Epilogues.Count != 0)
                Fail("ROC1214", "Selected-mode exception-handling dependency closure is unsupported; retained EH frames and tables require default all-definition mode.", locator);
            foreach (var local in body.Locals)
            {
                locals.Add(local); UseVariable(local, locator, "body-local");
            }
            var seen = new HashSet<SemanticNode>();
            Walk(body.Root, function, locator + "/body", seen, [], 0);
            VisitScope(body.Scope, locator + "/scope", 0);
            foreach (var lifetime in body.Lifetimes.Where(lifetime => lifetime.BoundEntity?.Equals(function) == true))
                VisitLifetime(lifetime, locator, 0);
        }

        private void VisitVariable(VariableSymbol variable)
        {
            var locator = Locator(variable);
            RequireType(variable.Type, true);
            RequireType(variable.DeclaredType, false);
            VisitConstant(variable.ConstantInitializer, locator, 0);
            VisitRecipe(variable.Initializer, locator, false, 0);
            Use(variable.CleanupRoutine, locator, "storage-cleanup");
            Use(variable.Details.DynamicInitializationRoutine, locator, "storage-initializer");
            Use(variable.Details.ThreadInitializationRoutine, locator, "thread-initializer");
            Use(variable.Details.ThreadWrapperRoutine, locator, "thread-wrapper");
            foreach (var local in variable.LocalStaticInitializations)
            {
                VisitConstant(local.Constant, locator, 0); VisitRecipe(local.Initialization, locator, false, 0);
                VisitLifetime(local.Lifetime, locator, 0);
            }
        }

        private bool IsStorageDefinition(VariableSymbol variable) => !locals.Contains(variable) && !variable.Details.IsParameter &&
            variable.Details.StorageDuration?.Name is not ("automatic" or "parameter") && variable.SymbolDetails.Storage?.Name != "sc_extern" &&
            !variable.Details.IsNonreal && !variable.Details.IsPrototypeInstantiation && !variable.Details.IsTemplateVariable;

        private void VisitConstant(Constant? constant, string from, int depth)
        {
            if (constant is null || !constantSet.Add(constant)) return;
            Tick(depth); constants.Add(constant); _ = Locator(constant);
            Edge(from, "constant", constant);
            RequireType(constant.Type, false, depth + 1);
            Use(constant.AssociatedStorageVariable, Locator(constant), "constant-storage", depth + 1);
            VisitValue(constant.Value, Locator(constant), depth + 1);
            foreach (var component in constant.Components) VisitConstant(component.Constant, Locator(constant), depth + 1);
            // BackingExpression, OriginalType and contextual source associations
            // do not add execution or independently required definitions.
        }

        private void VisitValue(ConstantValue value, string from, int depth)
        {
            if (!values.Add(value)) return;
            Tick(depth);
            switch (value)
            {
                case AddressValue address: Use(address.BaseEntity, from, "constant-address", depth + 1); break;
                case AggregateValue aggregate:
                    foreach (var element in aggregate.Elements)
                    { RequireType(element.Type, false, depth + 1); VisitValue(element.Value, from, depth + 1); }
                    break;
                case ConstantValue<RepeatValueDetails> repeat:
                    RequireType(repeat.Payload.Type, false, depth + 1); VisitValue(repeat.Payload.Value, from, depth + 1); break;
                case ConstantValue<MemberPointerValueDetails> member: Use(member.Payload.Member, from, "member-address", depth + 1); break;
                case ConstantValue<StackOffsetValueDetails> stack: Use(stack.Payload.Variable, from, "stack-offset", depth + 1); break;
                case LabelDifferenceValue difference:
                    VisitValue(difference.FromValue, from, depth + 1); VisitValue(difference.ToValue, from, depth + 1); break;
                case IntegerValue or FloatingValue or StringValue: break;
                default: Fail("ROC1214", $"Constant value '{value.Kind.Name}' has no supported dependency-selection rule.", from); break;
            }
        }

        private void VisitRecipe(Initialization? recipe, string from, bool cleanup, int depth)
        {
            if (recipe is null || !(cleanup ? cleanupRecipes : recipes).Add(recipe)) return;
            Tick(depth);
            if (cleanup || recipe.Destructor is not null || recipe.IsFreeingOfExceptionObject || recipe.NextInDestructionList is not null)
                Fail("ROC1214", "Selected-mode destruction and exception-owner recipe closure is unsupported.", from);
            VisitLifetime(recipe.Lifetime, from, depth + 1); VisitLifetime(recipe.InitExpressionLifetime, from, depth + 1);
            VisitLifetime(recipe.OverlappingLifetime, from, depth + 1);
            VisitConstant(recipe.Constant, from, depth + 1);
            var executionRoot = recipe.Operation is { RepresentationRole.Name: "execution" } operation ? (SemanticNode)operation :
                recipe.Operation is null && recipe.Expression is { RepresentationRole.Name: "execution" } expression ? expression : null;
            var execution = executionRoot is not null;
            if (!execution)
            {
                if (recipe.Constructor is not null || recipe.Expression is not null || recipe.Operands.Count != 0)
                    Fail("ROC1214", "A required initializer has no selected executable recipe schedule.", from);
                return;
            }
            if (!cleanup && executionRoot is not null) requiredInitializerOccurrences.Add(executionRoot);
            Use(recipe.Target, from, cleanup ? "cleanup-recipe-storage" : "initializer-target", depth + 1);
            RequireType(recipe.Details.Type, false, depth + 1);
            if (!cleanup) Use(recipe.Constructor, from, "initializer-constructor", depth + 1);
            Use(recipe.Destructor, from, cleanup ? "cleanup-recipe-target" : "initializer-destructor", depth + 1);
            // Executable expressions are visited at their retained statement/
            // operation occurrence, never again via this association.
        }

        private void VisitLifetime(Lifetime? lifetime, string from, int depth)
        {
            if (lifetime is null || !lifetimes.Add(lifetime)) return;
            Tick(depth);
            if (lifetime.Cleanups.Count != 0 || lifetime.ParentCheckpoint is not null || lifetime.HasImplicitChild || lifetime.HasBlockAfterLabelChild)
                Fail("ROC1214", "Selected-mode nonempty lifetime and cleanup dependency closure is unsupported.", from);
            foreach (var child in lifetime.Children) VisitLifetime(child, from, depth + 1);
        }

        private void VisitScope(Scope? scope, string from, int depth)
        {
            if (scope is null || !scopes.Add(scope)) return;
            Tick(depth); VisitLifetime(scope.Lifetime, from, depth + 1); VisitLifetime(scope.FunctionStaticLifetime, from, depth + 1);
            foreach (var initializer in scope.DynamicInitializations) VisitRecipe(initializer, from, false, depth + 1);
            foreach (var initializer in scope.LocalStaticInitializations)
            {
                VisitRecipe(initializer.Initialization, from, false, depth + 1); VisitConstant(initializer.Constant, from, depth + 1);
                VisitLifetime(initializer.Lifetime, from, depth + 1);
            }
            foreach (var initializer in scope.ConstructorInitializers) VisitRecipe(initializer.EffectiveInitialization, from, false, depth + 1);
        }

        private void Walk(SemanticNode node, FunctionSymbol owner, string path, HashSet<SemanticNode> seen,
            HashSet<SemanticNode> active, int depth)
        {
            Tick(depth);
            if (active.Contains(node)) Fail("ROC1214", "A selected execution containment path is cyclic.", path);
            if (!seen.Add(node)) return;
            if (node.RepresentationRole.Name != "execution") Fail("ROC1214", "A required typed execution edge names a metadata node.", path);
            if (seen.Count > budgets.MaximumSelectionOperationsPerBody) Fail("ROC1213", "Managed selected-body operation bound exceeded.", path);
            ++bodyOperationCount;
            executionVisited.Add(node);
            active.Add(node);
            try
            {
                if (node is Operation operation) RequireType(operation.Type, false, depth + 1);
                switch (node)
                {
                    case ReferenceOperation reference: Use(reference.Target, path, "reference", depth + 1); break;
                    case ConstantOperation constant: VisitConstant(constant.Constant, path, depth + 1); break;
                    case CallOperation call:
                        RequireType(call.Call.FunctionType, false, depth + 1);
                        if (call.TargetMethod is { } selected) UseFunction(selected, path, "retained-call-target");
                        if (call.CallKind.Name != "direct" || call.Callee is not ReferenceOperation { Target: FunctionSymbol target } ||
                            call.TargetMethod?.Equals(target) != true || call.Call.FunctionType?.Equals(target.Signature) != true)
                            Fail("ROC1214", "Managed selection requires an actual direct selected call; unresolved indirect target sets remain unsupported.", path);
                        break;
                    case InitializationStatementOperation initializationStatement: VisitRecipe(initializationStatement.Initialization, path, false, depth + 1); break;
                    case InitializationOperation initializationOperation: VisitRecipe(initializationOperation.Initialization, path, false, depth + 1); break;
                    case ThrowOperation or TryOperation or LoweredEhOperation:
                        Fail("ROC1214", "Selected-mode throw, handler and lowered-EH dependency closure is unsupported.", path); break;
                    case TemporaryOperation or StatementExpressionResultOperation:
                        Fail("ROC1214", "Selected-mode temporary and statement-result recipe closure is unsupported.", path); break;
                    case BlockOperation block: VisitScope(block.Scope, path, depth + 1); VisitLifetime(block.Lifetime, path, depth + 1); break;
                    case LifetimeOperation lifetime: VisitLifetime(lifetime.Lifetime, path, depth + 1); break;
                    case LabelOperation label: VisitLifetime(label.Lifetime, path, depth + 1); break;
                    case JumpOperation jump: VisitLifetime(jump.CommonLifetime, path, depth + 1); break;
                    case LoopOperation loop: VisitScope(loop.InitScope, path, depth + 1); break;
                    case ConditionOperation condition:
                        VisitScope(condition.Scope, path, depth + 1);
                        Use(condition.Variable, path, "condition-variable", depth + 1);
                        if (condition.DynamicInitialization is not null)
                            Fail("ROC1214", "Selected-mode condition dynamic-initialization recipe closure is unsupported.", path);
                        break;
                    case ExpressionOperation { Kind.Name: "enk_result_of_overriding_function" }:
                        if (owner.OverridingFunctionForWrapper is not { } overriding) Fail("ROC1214", "A selected wrapper result has no retained overriding target.", path);
                        else UseFunction(overriding, path, "wrapper-overriding-result");
                        break;
                }
                var children = ExecutionChildren(node, path).ToArray();
                foreach (var relation in node.ChildRelations)
                    if (relation.Child.RepresentationRole.Name == "execution" && !children.Contains(relation.Child))
                        Fail("ROC1214", $"Selected execution child '{relation.Role.Name}' has no typed dependency-selection rule.", path);
                for (var index = 0; index < children.Length; ++index)
                    Walk(children[index], owner, path + $"/child[{index}]", seen, active, depth + 1);
            }
            finally { active.Remove(node); }
        }

        private IEnumerable<SemanticNode> ExecutionChildren(SemanticNode node, string path)
        {
            switch (node)
            {
                case BlockOperation block: return block.Statements;
                case ConditionalOperation conditional: return new SemanticNode?[] { conditional.Condition, conditional.Then, conditional.Else }.OfType<SemanticNode>();
                case LoopOperation loop: return new SemanticNode?[] { loop.Initialization, loop.Condition, loop.Increment, loop.Body }.OfType<SemanticNode>();
                case SwitchOperation selectedSwitch: return new SemanticNode?[] { selectedSwitch.Selector, selectedSwitch.Body }.OfType<SemanticNode>();
                case TryOperation selectedTry: return new[] { selectedTry.Body }.Concat(selectedTry.Handlers.Select(handler => handler.Body));
                case JumpOperation jump: return jump.Value is { } jumpValue ? [jumpValue] : [];
                case LabelOperation or DeclarationOperation: return [];
                case InitializationStatementOperation initializationStatement:
                    return initializationStatement.Initialization.Operation is { RepresentationRole.Name: "execution" } initializerOperation ? [initializerOperation] :
                        initializationStatement.Initialization.Operation is null && initializationStatement.Initialization.Expression is { RepresentationRole.Name: "execution" } initializerValue ? [initializerValue] : [];
                case InitializationOperation initializationOperation:
                    return new SemanticNode?[] { initializationOperation.Expression }
                        .Concat(initializationOperation.Initialization.Operands.Select(operand => (SemanticNode?)operand.Expression)).OfType<SemanticNode>();
                case ThrowOperation thrown: return thrown.Operand is { } thrownValue ? [thrownValue] : [];
                case LifetimeOperation lifetime: return [lifetime.Value];
                case ConditionOperation condition: return new SemanticNode?[] { condition.Initialization, condition.Value }.OfType<SemanticNode>();
                case StatementExpressionOperation statementExpression: return [statementExpression.Statement];
                case ReferenceOperation or ConstantOperation: return [];
                case LoweredEhOperation marker: return marker.Operands;
                case ExpressionOperation { Kind.Name: "enk_result_of_overriding_function", Operands.Count: 0 }: return [];
                case OperatorOperation operation: return operation.Operands;
                case StatementOperation { Kind.Name: "stmk_expr", Expression: { } statementValue }: return [statementValue];
                case StatementOperation { Kind.Name: "stmk_empty" }: return [];
                default: throw new ManagedSelectionException("ROC1214", $"Execution form '{node.Kind.Name}' has no typed dependency-selection rule.", path);
            }
        }

        private static void Fail(string code, string message, string? locator = null)
            => throw new ManagedSelectionException(code, message, locator);
    }
}

/// <summary>Attributable selected-unit reports and exact public declaration bindings.</summary>
internal static class ManagedProgramSelectionReports
{
    internal static ManagedSelectionReport Create(ManagedProgramSelectionPlan program)
    {
        var units = program.UnitSelections;
        string FunctionLocator(FunctionSymbol symbol, int ordinal)
            => $"unit/{ordinal:D4}/function/{program.Units[ordinal - 1].Functions.ToList().IndexOf(symbol) + 1:D6}";
        string VariableLocator(VariableSymbol symbol, int ordinal)
            => $"unit/{ordinal:D4}/storage/{program.Units[ordinal - 1].Variables.ToList().IndexOf(symbol) + 1:D6}";
        var bindings = program.FunctionBindings.Select(binding => new ManagedSelectionBindingReport("function", binding.LinkageName,
            FunctionLocator(binding.Declaration, binding.DeclarationUnitOrdinal), FunctionLocator(binding.Definition, binding.DefinitionUnitOrdinal), binding.Compatibility.Reason))
            .Concat(program.VariableBindings.Select(binding => new ManagedSelectionBindingReport("variable", binding.LinkageName,
                VariableLocator(binding.Declaration, binding.DeclarationUnitOrdinal), VariableLocator(binding.Definition, binding.DefinitionUnitOrdinal), binding.Compatibility.Reason))).ToArray();
        return new()
        {
            Scope = "selected-multi-input-executable-closure", ClosureComplete = units.All(unit => unit.Selection.Report.ClosureComplete),
            UnitSelections = units.Select(unit => new ManagedUnitSelectionReport(unit.UnitOrdinal, unit.Selection.Source.Manifest.ArtifactIdentity,
                unit.Selection.Report.InputSha256, unit.Selection.Report)).ToArray(), Bindings = bindings,
            ExecutableBodiesVisited = units.Sum(unit => unit.Selection.Report.ExecutableBodiesVisited),
            RequiredDeclarationCount = units.Sum(unit => unit.Selection.Report.RequiredDeclarationCount),
            StorageDefinitionCount = units.Sum(unit => unit.Selection.Report.StorageDefinitionCount),
            RequiredVariableCount = units.Sum(unit => unit.Selection.Report.RequiredVariableCount),
            ScopedTypeCount = units.Sum(unit => unit.Selection.Report.ScopedTypeCount),
            RequiredConstantCount = units.Sum(unit => unit.Selection.Report.RequiredConstantCount),
            BodyOperationCount = units.Sum(unit => unit.Selection.Report.BodyOperationCount),
            SelectionWorkCount = units.Sum(unit => unit.Selection.Report.SelectionWorkCount),
            AmbientRootRowsInspected = units.Sum(unit => unit.Selection.Report.AmbientRootRowsInspected),
            AmbientRoutineCandidatesInspected = units.Sum(unit => unit.Selection.Report.AmbientRoutineCandidatesInspected),
            Roots = units.SelectMany(unit => unit.Selection.Report.Roots).ToArray(),
            StartupRoots = units.SelectMany(unit => unit.Selection.Report.StartupRoots).ToArray(),
            StartupExecutionContract = units.Select(unit => unit.Selection.Report.StartupExecutionContract).FirstOrDefault(contract => contract is not null),
            AmbientInitialization = units.Select(unit => unit.Selection.Report.AmbientInitialization).FirstOrDefault() ?? "No selected ambient schedule.",
            Dependencies = units.SelectMany(unit => unit.Selection.Report.Dependencies).ToArray()
        };
    }
}
