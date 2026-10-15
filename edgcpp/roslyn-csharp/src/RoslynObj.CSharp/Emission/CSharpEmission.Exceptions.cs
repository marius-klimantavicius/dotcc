using System.Globalization;
using System.Text;
using RoslynObj;

namespace RoslynObj.CSharp;

public sealed partial class CSharpEmitter
{
    private sealed partial class Emission
    {
        private const string ExceptionCarrierType = "global::RoslynObj.CSharp.Runtime.ManagedCppException";
        private sealed record ExceptionFunction(FunctionSymbol Function, VariableSymbol RegionTable,
            IReadOnlyList<CleanupRegion> Regions, string State, string Observed, string ObservedParameter, string Caught);
        private sealed record ExceptionTry(FunctionSymbol Function, TryOperation Operation, ExceptionHandler Handler, string Token, string SavedState, string Carrier);
        private readonly Dictionary<FunctionSymbol, ExceptionFunction> exceptionFunctions = [];
        private readonly Dictionary<TryOperation, ExceptionTry> exceptionTries = [];
        private readonly Dictionary<LoweredEhOperation, FunctionSymbol> exceptionMarkers = [];
        private readonly Dictionary<ThrowOperation, ExceptionTry> selectedRethrows = [];
        private readonly Dictionary<CleanupAction, ExceptionTry> exceptionReleaseHandlers = [];
        private readonly List<SemanticRuntimeFunctionImplementation> semanticRuntimeFunctions = [];
        private readonly HashSet<FunctionSymbol> validatedNoThrowFunctions = [];
        private readonly HashSet<FunctionSymbol> exceptionParticipants = [];
        private readonly HashSet<FunctionSymbol> validatedThrowingConstructors = [];
        private CppType? exceptionIntType;
        private ThrowOperation? selectedIntegerThrow;
        private FunctionSymbol? selectedIntegerThrowFunction;
        private string? exceptionTypeToken;
        private string? pendingThrowOwner;
        private ExceptionTry? emittingExceptionHandler;
        private bool boundedRethrow;

        private string ExceptionLocal(string stem)
        {
            string name;
            do { name = InternalName(stem + "_" + ++expressionTemporaryOrdinal); }
            while (names.Values.Contains(name, StringComparer.Ordinal) || names.Values.Contains("@" + name, StringComparer.Ordinal));
            return name;
        }

        private void InitializeBoundedExceptions()
        {
            var candidates = functions.Where(function => function.Body!.ExceptionHandling.CleanupRegions.Count != 0 &&
                function.Signature.ExceptionSpecification?.NoexceptEffective != true).ToArray();
            if (candidates.Length == 0) return;
            RequireNormalLifetimeProfile();
            var capabilities = inputs[0].TranslationUnit.Capabilities;
            foreach (var capability in new[] { ("DO_IL_LOWERING", "1"), ("DO_FULL_PORTABLE_EH_LOWERING", "0"), ("GENERATE_EH_TABLES", "1"), ("IA64_ABI", "1") })
                if (!capabilities.Any(actual => actual.Name == capability.Item1 && actual.Value == capability.Item2))
                    throw Limit("The bounded integer exception adapter requires the actual nonportable table-based Itanium lowering capabilities.", "exception-profile");
            var graph = functions.ToDictionary(function => function, function => ExceptionExecutionNodes(function.Body!.Root).ToArray());
            var throws = graph.Values.SelectMany(nodes => nodes.OfType<ThrowOperation>()).ToArray();
            var tries = graph.Values.SelectMany(nodes => nodes.OfType<TryOperation>()).ToArray();
            if (throws.Any(throwing => throwing.IsRethrow))
            {
                InitializeBoundedRethrow(graph, candidates, throws, tries);
                return;
            }
            if (throws.Length != 1 || tries.Length != 1 || candidates.Length is not (1 or 2))
                throw Limit("The bounded generated exception slice requires one integer throw, one typed try and their retained cleanup functions.", "exception");
            var throwing = throws[0];
            var selectedTry = tries[0];
            if (throwing.IsRethrow || throwing.ThrownType is not { } thrownType || throwing.Initialization is not null ||
                throwing.Destructor is not null || throwing.AccessibleBases.Count != 0 || throwing.Operands.Count != 0 || throwing.Operand is null ||
                throwing.ValueCategory.Name != "prvalue" || MemoryRepresentationType(throwing.Type).Kind.Name != "tk_void")
                throw Limit("The bounded throw requires its already-lowered integer operand, without rethrow, payload constructor or base matching.", "exception");
            ValidateGeneratedInt32(thrownType);
            if (throwing.Operand is not OperatorOperation { OperatorKind.Name: "eok_assign" } assignment || !assignment.Operator.EvalRightToLeft ||
                assignment.Operands.Count != 2 || !assignment.Operands[1].Type.Equals(thrownType) ||
                assignment.Operands[0] is not OperatorOperation { OperatorKind.Name: "eok_indirect" } destination || !destination.Type.Equals(thrownType) ||
                destination.Operands.Count != 1 || destination.Operands[0] is not ConversionOperation { OperatorKind.Name: "eok_cast" } cast ||
                MemoryRepresentationType(cast.Type) is not PointerType { IsReference: false } payloadPointer || !payloadPointer.PointeeType.Equals(thrownType) ||
                cast.Operand is not LoweredEhOperation { ExceptionKind.Name: "leck_thrown_object_address" })
                throw Limit("The captured integer throw operand must already assign its value once to the lowered thrown-object destination.", "exception");
            if (selectedTry.Definition.IsFunctionTry || selectedTry.Handlers.Count != 1 ||
                selectedTry.Handlers[0].CaughtType is not { } caughtType || !caughtType.Equals(thrownType))
                throw Limit("The selected handler must catch the identical retained integer semantic type.", "exception-type");
            var handler = selectedTry.Handlers[0];
            if (handler.Parameter is not { } parameter || !parameter.Type.Equals(thrownType) ||
                handler.ParameterInitialization is not { } parameterRecipe || parameterRecipe.Target?.Equals(parameter) != true)
                throw Limit("The bounded integer handler requires its retained parameter and metadata initialization association.", "exception-handler");
            ValidateExceptionMetadataRecipe(parameterRecipe, false);
            if (parameterRecipe.Destructor is not null || parameterRecipe.Lifetime is not null)
                throw Limit("The handler parameter recipe adds an unimplemented cleanup obligation.", "exception-handler");
            var throwingFunction = graph.Single(pair => pair.Value.Contains(throwing)).Key;
            var catchingFunction = graph.Single(pair => pair.Value.Contains(selectedTry)).Key;
            if (throwingFunction.Equals(catchingFunction) || !candidates.Contains(catchingFunction) ||
                !IsStandaloneThrow(throwingFunction.Body!.Root, throwing) ||
                ExceptionExecutionNodes(handler.Body).Any(node => node is ThrowOperation or TryOperation))
                throw Limit("The first integer exception transfer requires separate throwing/catching functions and no nested handler throw/try.", "exception");
            if (candidates.Length == 1)
                InitializeAutomaticArray(graph, throwingFunction, catchingFunction);
            else if (!candidates.Contains(throwingFunction))
                throw Limit("The throwing function has no validated cleanup frame or automatic-array constructor role.", "exception");
            // All active execution remains generated. Catalog-only declarations
            // do not add calls, and the no-object cleanup target is handled below.
            foreach (var pair in graph)
                foreach (var call in pair.Value.OfType<CallOperation>())
                    if (call.Callee is not ReferenceOperation { Target: FunctionSymbol target } || !functions.Contains(target) && !automaticArrayCalls.ContainsKey(call) ||
                        call.TargetMethod is null || !call.TargetMethod.Equals(target) || target.Signature.IsVariadic)
                        throw Limit("The first generated integer EH program has only selected fixed direct calls; native/indirect unwinding is unsupported.", "exception-boundary");
            foreach (var function in functions.Where(function => function.Signature.ExceptionSpecification?.NoexceptEffective == true))
                EnsureGeneratedNoThrow(function);
            foreach (var call in ExceptionExecutionNodes(handler.Body).OfType<CallOperation>())
            {
                if (call.Callee is not ReferenceOperation { Target: FunctionSymbol target })
                    throw Limit("A selected integer handler cannot propagate through an indirect call.", "exception-handler");
                EnsureGeneratedNoThrow(target);
            }
            exceptionIntType = thrownType;
            selectedIntegerThrow = throwing;
            selectedIntegerThrowFunction = throwingFunction;
            exceptionTypeToken = InternalName("__cpp_int32_type");
            var tryPlan = new ExceptionTry(catchingFunction, selectedTry, handler, InternalName("__cpp_handler"), ExceptionLocal("__try_entry_state"), ExceptionLocal("__caught_cpp"));
            exceptionTries.Add(selectedTry, tryPlan);
            foreach (var function in candidates)
            {
                var nodes = graph[function];
                ValidateExceptionFunction(function, nodes, tryPlan);
                if (function.Equals(throwingFunction) && function.SpecialFunctionKind?.Name == "sfk_constructor" &&
                    function.ConstructorOrDestructorKind?.Name == "cdk_complete" &&
                    receiverMemberCleanups.Values.Count(member => member.Function.Equals(function)) == 1)
                    validatedThrowingConstructors.Add(function);
                var eh = function.Body!.ExceptionHandling;
                exceptionFunctions.Add(function, new(function, eh.RegionTable!, eh.CleanupRegions,
                    ExceptionLocal("__cleanup_state"), ExceptionLocal("__unwinding_cpp"), ExceptionLocal("__observed_cpp"), ExceptionLocal("__active_caught_cpp")));
                foreach (var marker in nodes.OfType<LoweredEhOperation>()) exceptionMarkers.Add(marker, function);
            }
            if (automaticArray is not null)
                foreach (var marker in graph[throwingFunction].OfType<LoweredEhOperation>())
                {
                    ValidateExceptionMarker(marker, throwingFunction, tryPlan);
                    exceptionMarkers.Add(marker, throwingFunction);
                }
            if (semanticRuntimeFunctions.Count != (automaticArray is null ? 1 : 3) ||
                semanticRuntimeFunctions.Count(implementation => implementation.Contract == SemanticRuntimeFunctionImplementation.Int32ExceptionOwnerContract) != 1)
                throw Limit("The bounded integer EH program requires its exact validated exception-owner and optional automatic-array contracts.", "exception-runtime");
            foreach (var function in functions)
                if (FunctionReachesException(function, graph, [])) exceptionParticipants.Add(function);
        }

        private void InitializeBoundedRethrow(IReadOnlyDictionary<FunctionSymbol, SemanticNode[]> graph,
            IReadOnlyList<FunctionSymbol> candidates, IReadOnlyList<ThrowOperation> throws, IReadOnlyList<TryOperation> tries)
        {
            if (throws.Count != 2 || throws.Count(throwing => throwing.IsRethrow) != 1 || tries.Count != 2 || candidates.Count != 2 ||
                functions.Any(function => function.Body!.ExceptionHandling.Arrays.Count != 0 || function.Body!.ExceptionHandling.ArrayTable is not null))
                throw Limit("The bounded rethrow requires one primary throw, one operandless restart and two separate typed handlers without arrays.", "exception-rethrow");
            var throwing = throws.Single(throwing => !throwing.IsRethrow);
            var rethrowing = throws.Single(throwing => throwing.IsRethrow);
            if (throwing.ThrownType is not { } thrownType || throwing.Initialization is not null || throwing.Destructor is not null ||
                throwing.AccessibleBases.Count != 0 || throwing.Operands.Count != 0 || throwing.Operand is null ||
                throwing.ValueCategory.Name != "prvalue" || MemoryRepresentationType(throwing.Type).Kind.Name != "tk_void")
                throw Limit("The primary rethrow owner requires its retained lowered integer initializer.", "exception-rethrow");
            ValidateGeneratedInt32(thrownType);
            if (throwing.Operand is not OperatorOperation { OperatorKind.Name: "eok_assign" } assignment || !assignment.Operator.EvalRightToLeft ||
                assignment.Operands.Count != 2 || !assignment.Operands[1].Type.Equals(thrownType) ||
                assignment.Operands[0] is not OperatorOperation { OperatorKind.Name: "eok_indirect" } destination || !destination.Type.Equals(thrownType) ||
                destination.Operands.Count != 1 || destination.Operands[0] is not ConversionOperation { OperatorKind.Name: "eok_cast" } cast ||
                MemoryRepresentationType(cast.Type) is not PointerType { IsReference: false } payloadPointer || !payloadPointer.PointeeType.Equals(thrownType) ||
                cast.Operand is not LoweredEhOperation { ExceptionKind.Name: "leck_thrown_object_address" } ||
                rethrowing.ThrownType is not null || rethrowing.Operand is not null || rethrowing.Initialization is not null || rethrowing.Destructor is not null ||
                rethrowing.AccessibleBases.Count != 0 || rethrowing.Operands.Count != 0 || rethrowing.ValueCategory.Name != "prvalue" ||
                MemoryRepresentationType(rethrowing.Type).Kind.Name != "tk_void")
                throw Limit("The selected restart must remain operandless and typeless; it cannot initialize a second exception object.", "exception-rethrow");
            var innerCandidates = tries.Where(selectedTry => ExceptionExecutionNodes(selectedTry.Body).Contains(throwing)).ToArray();
            if (innerCandidates.Length != 1)
                throw Limit("The primary throw must occur inside one actual selected inner try body.", "exception-rethrow");
            var innerTry = innerCandidates[0];
            var outerTry = tries.Single(selectedTry => !selectedTry.Equals(innerTry));
            var innerFunction = graph.Single(pair => pair.Value.Contains(innerTry)).Key;
            var outerFunction = graph.Single(pair => pair.Value.Contains(outerTry)).Key;
            if (innerFunction.Equals(outerFunction) || !candidates.Contains(innerFunction) || !candidates.Contains(outerFunction) ||
                !graph[innerFunction].Contains(throwing) || !graph[innerFunction].Contains(rethrowing) ||
                !IsStandaloneThrow(innerFunction.Body!.Root, throwing) || !IsStandaloneThrow(innerFunction.Body!.Root, rethrowing))
                throw Limit("The rethrow slice requires distinct selected inner and outer cleanup functions.", "exception-rethrow");
            foreach (var selectedTry in new[] { innerTry, outerTry })
            {
                if (selectedTry.Definition.IsFunctionTry || selectedTry.Handlers.Count != 1 ||
                    selectedTry.Handlers[0].CaughtType?.Equals(thrownType) != true ||
                    selectedTry.Handlers[0].Parameter is not { } parameter || !parameter.Type.Equals(thrownType) ||
                    selectedTry.Handlers[0].ParameterInitialization is not { } recipe || recipe.Target?.Equals(parameter) != true ||
                    recipe.Destructor is not null || recipe.Lifetime is not null)
                    throw Limit("Both rethrow handlers require their distinct retained parameters of the exact primary integer type.", "exception-rethrow");
                ValidateExceptionMetadataRecipe(recipe, false);
            }
            var innerHandler = innerTry.Handlers[0];
            var outerHandler = outerTry.Handlers[0];
            var innerHandlerNodes = ExceptionExecutionNodes(innerHandler.Body).ToArray();
            var outerHandlerNodes = ExceptionExecutionNodes(outerHandler.Body).ToArray();
            if (!innerHandlerNodes.Contains(rethrowing) || innerHandlerNodes.OfType<ThrowOperation>().Any(throwing => !throwing.Equals(rethrowing)) ||
                innerHandlerNodes.Any(node => node is TryOperation) ||
                outerHandlerNodes.Any(node => node is ThrowOperation or TryOperation) ||
                innerHandler.Parameter!.Equals(outerHandler.Parameter) ||
                ExceptionExecutionNodes(innerHandler.Body).OfType<LoweredEhOperation>().Any(marker => marker.ExceptionKind.Name == "leck_catch_epilogue") ||
                innerFunction.Body!.ExceptionHandling.Epilogues.Count != 0 ||
                outerHandlerNodes.OfType<LoweredEhOperation>().Count(marker => marker.ExceptionKind.Name == "leck_catch_epilogue") != 1 ||
                !IsTerminatingRethrowHandler(innerHandler.Body, rethrowing, innerFunction.Body!.ExceptionHandling.Details?.NullState))
                throw Limit("Only the actual operandless inner-handler restart is admitted; no normal inner release or function epilogue is synthesized.", "exception-rethrow");
            var outerCalls = ExceptionExecutionNodes(outerTry.Body).OfType<CallOperation>().Where(call =>
                call.Callee is ReferenceOperation { Target: FunctionSymbol target } && target.Equals(innerFunction) && call.TargetMethod?.Equals(target) == true).ToArray();
            if (outerCalls.Length != 1)
                throw Limit("The outer try must contain the actual selected call into the inner rethrow function.", "exception-rethrow");
            foreach (var nodes in graph.Values)
                foreach (var call in nodes.OfType<CallOperation>())
                    if (call.Callee is not ReferenceOperation { Target: FunctionSymbol target } || !functions.Contains(target) ||
                        call.TargetMethod?.Equals(target) != true || target.Signature.IsVariadic)
                        throw Limit("The rethrow slice has only selected fixed direct managed calls; native unwinding is unsupported.", "exception-boundary");
            foreach (var function in functions.Where(function => function.Signature.ExceptionSpecification?.NoexceptEffective == true))
                EnsureGeneratedNoThrow(function);
            foreach (var handler in new[] { innerHandler, outerHandler })
                foreach (var call in ExceptionExecutionNodes(handler.Body).OfType<CallOperation>())
                {
                    if (call.Callee is not ReferenceOperation { Target: FunctionSymbol target })
                        throw Limit("A rethrow handler cannot invoke an indirect throwing helper.", "exception-handler");
                    EnsureGeneratedNoThrow(target);
                }

            boundedRethrow = true;
            exceptionIntType = thrownType;
            selectedIntegerThrow = throwing;
            selectedIntegerThrowFunction = innerFunction;
            exceptionTypeToken = InternalName("__cpp_int32_type");
            var inner = new ExceptionTry(innerFunction, innerTry, innerHandler, InternalName("__cpp_handler"), ExceptionLocal("__try_entry_state"), ExceptionLocal("__caught_cpp"));
            var outer = new ExceptionTry(outerFunction, outerTry, outerHandler, InternalName("__cpp_handler"), ExceptionLocal("__try_entry_state"), ExceptionLocal("__caught_cpp"));
            exceptionTries.Add(innerTry, inner);
            exceptionTries.Add(outerTry, outer);
            selectedRethrows.Add(rethrowing, inner);
            foreach (var plan in new[] { inner, outer })
            {
                var function = plan.Function;
                var nodes = graph[function];
                ValidateExceptionFunction(function, nodes, plan);
                var eh = function.Body!.ExceptionHandling;
                exceptionFunctions.Add(function, new(function, eh.RegionTable!, eh.CleanupRegions,
                    ExceptionLocal("__cleanup_state"), ExceptionLocal("__unwinding_cpp"), ExceptionLocal("__observed_cpp"), ExceptionLocal("__active_caught_cpp")));
                foreach (var marker in nodes.OfType<LoweredEhOperation>()) exceptionMarkers.Add(marker, function);
            }
            if (exceptionReleaseHandlers.Count != 2 || exceptionReleaseHandlers.Values.Count(plan => plan.Equals(inner)) != 1 ||
                exceptionReleaseHandlers.Values.Count(plan => plan.Equals(outer)) != 1 || semanticRuntimeFunctions.Count != 1 ||
                semanticRuntimeFunctions[0].Contract != SemanticRuntimeFunctionImplementation.Int32RethrowOwnerContract)
                throw Limit("Both retained handler contexts must select the same validated rethrow-owner helper identity once.", "exception-runtime");
            var innerActions = innerFunction.Body!.ExceptionHandling.CleanupRegions.SelectMany(region => region.Actions).ToArray();
            var innerRelease = exceptionReleaseHandlers.Single(pair => pair.Value.Equals(inner)).Key;
            var innerRecipes = innerFunction.Body!.Lifetimes.SelectMany(lifetime => lifetime.Cleanups.Select(cleanup => cleanup.Initialization)).ToArray();
            var links = innerRecipes.Where(recipe => recipe.NextInDestructionList is not null).ToArray();
            if (innerActions.Length != 3 || outerFunction.Body!.ExceptionHandling.CleanupRegions.Count != 2 ||
                innerRelease.Details.Region.NextRegion is not null || links.Length != 1 || !IsRethrowHandlerDestructionLink(links[0], inner, innerActions) ||
                innerHandler.Body is not BlockOperation handlerBlock || handlerBlock.Statements.Count < 3 ||
                handlerBlock.Statements[^3].Expression is not CleanupStateOperation { IsUnreachable: false } active ||
                innerActions.Single(action => action.Storage?.Variable?.Equals(links[0].Target) == true).Details.Region is not { } activeRegion ||
                active.StateNumber != activeRegion.Ordinal || active.Region is { } associated && !associated.Equals(activeRegion))
                throw Limit("The restart must retain the exact handler-local to inner-owner cleanup chain and its active state before rethrow.", "exception-rethrow");
            foreach (var function in functions)
                if (FunctionReachesException(function, graph, [])) exceptionParticipants.Add(function);
        }

        private static bool IsTerminatingRethrowHandler(StatementOperation body, ThrowOperation rethrowing, System.Numerics.BigInteger? nullState)
        {
            if (body is not BlockOperation block || block.Statements.Count < 2 || block.Statements[^2].Expression?.Equals(rethrowing) != true ||
                block.Statements[^1].Expression is not CleanupStateOperation { IsUnreachable: true } unreachable ||
                unreachable.Region is not null || unreachable.StateNumber != nullState) return false;
            return block.Statements.Take(block.Statements.Count - 2).All(statement => statement is DeclarationOperation ||
                statement.Kind.Name == "stmk_expr" && statement.Expression is not null);
        }

        private void ValidateExceptionFunction(FunctionSymbol function, IReadOnlyList<SemanticNode> nodes, ExceptionTry tryPlan)
        {
            var body = function.Body!;
            var eh = body.ExceptionHandling;
            if (function.Signature.ExceptionSpecification?.NoexceptEffective == true || eh.Details is not { } details ||
                eh.RegionTable is not { } table || !body.Locals.Contains(table) ||
                (automaticArray?.Function.Equals(function) != true && (eh.ArrayTable is not null || eh.Arrays.Count != 0)) ||
                details.InitialState != 65535 || details.NullState != 65535 || details.Prologue is null || !nodes.Contains(details.Prologue))
                throw Limit("The integer EH function requires its actual cleanup table, null state and prologue without a noexcept boundary.", "exception");
            if (table.ExceptionTable is not { } projection || projection.Kind.Name != "cleanup" || !projection.Owner.Equals(table) ||
                !projection.FunctionEh.Routine.Equals(function) || projection.FunctionEh.RegionTable?.Equals(table) != true ||
                nodes.OfType<ReferenceOperation>().Any(reference => reference.Target?.Equals(table) == true) ||
                nodes.OfType<ConstantOperation>().Any(constant => constant.Constant.Value is not IntegerValue) ||
                table.Initializer is not null || table.ConstantInitializer is not null ||
                table.LocalStaticInitializations.Any(initializer => !initializer.Variable.Equals(table) || initializer.Kind.Name != "initk_static" ||
                    initializer.Initialization is not null || initializer.Constant is not null || initializer.Lifetime is not null))
                throw Limit("The EH table must be its exact metadata projection with no application value reference or initializer.", "exception");
            if (automaticArray?.Function.Equals(function) == true) ValidateAutomaticArrayTable(function, nodes);
            var recipes = body.Lifetimes.SelectMany(lifetime => lifetime.Cleanups.Select(cleanup => cleanup.Initialization)).ToArray();
            var actions = new List<CleanupAction>();
            var matchedRecipes = new HashSet<Initialization>();
            foreach (var region in eh.CleanupRegions)
            {
                if (region.Ordinal is < 0 or > int.MaxValue or 65535 || region.Condition is not null || region.Details.IsSupplement || region.Details.SupplementOwner is not null ||
                    !region.Details.FunctionEh.Routine.Equals(function) || region.Actions.Count != 1 ||
                    region.NextRegion is { } next && (!eh.CleanupRegions.Contains(next) || next.Ordinal >= region.Ordinal))
                    throw Limit("The bounded exception region chain requires one unconditional retained action and an acyclic next link.", "exception-cleanup");
                var action = region.Actions[0];
                var flags = action.Details;
                var arrayAction = automaticArray?.Cleanup.Equals(action) == true;
                if (!flags.Region.Equals(region) || flags.Ordinal != 0 || action.Kind.Name != "destructor" ||
                    (!arrayAction && (action.Array is not null || flags.IsArray)) || action.AbiDetail is not null ||
                    flags.IsIndirect || flags.IsConditional || flags.IsBaseSubobject || flags.IsVla || flags.IsDeallocation || flags.IsLocalStaticGuard ||
                    action.Storage is not { } storage || storage.IsIndirect || storage.Details.BaseEntity is not null)
                    throw Limit("The bounded EH action has an unsupported condition, array, storage offset or ABI side effect.", "exception-cleanup");
                if (arrayAction)
                {
                    if (!matchedRecipes.Add(automaticArray!.FullRecipe) || !matchedRecipes.Add(automaticArray.PartialRecipe))
                        throw Limit("The automatic-array cleanup shares a metadata recipe with another action.", "array-lifetime");
                }
                else if (storage.Kind.Name == "direct_variable")
                {
                    if (storage.Offset != 0 || storage.UsesThisParameterOffset || storage.Variable is not { } variable || !body.Locals.Contains(variable) || action.TargetMethod is not { } destructor ||
                        destructor.SpecialFunctionKind?.Name != "sfk_destructor" || destructor.Parameters.Count != 1 ||
                        !GeneratedRecordPointer(destructor.Parameters[0].Type, false).Equals(MemoryRepresentationType(variable.Type)) ||
                        recipes.Count(recipe => recipe.Target?.Equals(variable) == true && recipe.Destructor?.Equals(destructor) == true && !recipe.IsFreeingOfExceptionObject) != 1)
                        throw Limit("A direct-variable EH action must match one retained local destructor recipe.", "exception-cleanup");
                    var selectedRecipe = recipes.Single(recipe => recipe.Target?.Equals(variable) == true && recipe.Destructor?.Equals(destructor) == true && !recipe.IsFreeingOfExceptionObject);
                    ValidateExceptionMetadataRecipe(selectedRecipe, false);
                    if (!matchedRecipes.Add(selectedRecipe)) throw Limit("Two EH actions unexpectedly share one destruction recipe.", "exception-cleanup");
                    ValidateGeneratedLifetimeRoutine(destructor);
                    EnsureGeneratedNoThrow(destructor);
                }
                else if (storage.Kind.Name == "receiver_relative")
                {
                    var member = ValidateReceiverMemberCleanup(function, action, recipes, nodes, false);
                    if (!matchedRecipes.Add(member.Recipe)) throw Limit("Two member actions unexpectedly share one destruction recipe.", "exception-cleanup");
                }
                else if (storage.Kind.Name == "no_object")
                {
                    if (storage.Offset != 0 || storage.UsesThisParameterOffset || storage.Variable is not null || !nodes.Contains(tryPlan.Operation) ||
                        recipes.Count(recipe => recipe.IsFreeingOfExceptionObject && recipe.Target is null && recipe.Destructor is null &&
                            tryPlan.Handler.Body is BlockOperation handlerBlock && handlerBlock.Scope?.Equals(recipe.Lifetime?.BoundEntity) == true) != 1 ||
                        action.TargetMethod is not { } release)
                        throw Limit("The no-object EH action must have the retained exception-owner release recipe in its handler function.", "exception-runtime");
                    var selectedRecipe = recipes.Single(recipe => recipe.IsFreeingOfExceptionObject && recipe.Target is null && recipe.Destructor is null &&
                        tryPlan.Handler.Body is BlockOperation handlerBlock && handlerBlock.Scope?.Equals(recipe.Lifetime?.BoundEntity) == true);
                    ValidateExceptionMetadataRecipe(selectedRecipe, true);
                    if (!matchedRecipes.Add(selectedRecipe)) throw Limit("Two no-object actions unexpectedly share one release recipe.", "exception-cleanup");
                    exceptionReleaseHandlers.Add(action, tryPlan);
                    ValidateExceptionReleaseRoutine(release);
                }
                else throw Limit("The integer EH adapter supports only validated direct-variable, receiver-relative member and no-object cleanup storage.", "exception-cleanup");
                actions.Add(action);
            }
            if (actions.Count + (automaticArray?.Function.Equals(function) == true ? 1 : 0) != recipes.Length || matchedRecipes.Count != recipes.Length ||
                recipes.Any(recipe => recipe.NextInDestructionList is not null &&
                    (automaticArray is null || !recipe.Equals(automaticArray.FullRecipe) || !recipe.NextInDestructionList.Equals(automaticArray.PartialRecipe)) &&
                    !IsRethrowHandlerDestructionLink(recipe, tryPlan, actions)))
                throw Limit("The first integer EH slice requires one matching metadata recipe for each action, without a second destruction schedule.", "exception-cleanup");
            foreach (var marker in nodes.OfType<LoweredEhOperation>()) ValidateExceptionMarker(marker, function, tryPlan);
            if (eh.Epilogues.Any(epilogue => !nodes.Contains(epilogue)))
                throw Limit("An actual EH epilogue is absent from the typed function body.", "exception");
        }

        private bool IsRethrowHandlerDestructionLink(Initialization recipe, ExceptionTry plan, IReadOnlyList<CleanupAction> actions)
        {
            if (!boundedRethrow || !selectedRethrows.Values.Contains(plan) || recipe.NextInDestructionList is not { } freeing ||
                !freeing.IsFreeingOfExceptionObject || freeing.Target is not null || freeing.Details.Type is not null || freeing.Destructor is not null ||
                freeing.NextInDestructionList is not null || recipe.Target is not { } variable || recipe.Destructor is not { } destructor ||
                plan.Handler.Body is not BlockOperation handlerBlock || handlerBlock.Scope?.Equals(recipe.Lifetime?.BoundEntity) != true ||
                !Equals(recipe.Lifetime, freeing.Lifetime)) return false;
            var locals = actions.Where(action => action.Storage?.Variable?.Equals(variable) == true && action.TargetMethod?.Equals(destructor) == true).ToArray();
            var releases = actions.Where(action => exceptionReleaseHandlers.TryGetValue(action, out var handler) && handler.Equals(plan)).ToArray();
            return locals.Length == 1 && releases.Length == 1 && locals[0].Details.Region.NextRegion?.Equals(releases[0].Details.Region) == true;
        }

        private static void ValidateExceptionMetadataRecipe(Initialization recipe, bool freeing, bool constructorMember = false, bool arrayPartial = false)
        {
            var flags = recipe.Details;
            if (recipe.Kind.Name != "dik_none" || recipe.Operation?.RepresentationRole.Name != "metadata" || recipe.Expression is not null ||
                recipe.Constructor is not null || recipe.Constant is not null || recipe.Operands.Count != 0 || recipe.Master is not null ||
                recipe.OverlappingLifetime is not null || recipe.InitExpressionLifetime is not null || flags.IsFreeingOfExceptionObject != freeing ||
                flags.IsUnordered || flags.IsConditional || flags.IsOverlapping || flags.IsClassResultMaster || flags.IsStaticTemporary || flags.IsPartialAggregate != arrayPartial ||
                flags.IsPartialInitialization || flags.HasTemporaryLifetime || flags.IsConstructorInitialization != constructorMember || flags.IsConditionalGuard || flags.IsVlaDeallocation ||
                flags.CreatesInitializerList || flags.IsInitializerListArray || flags.IsReusedValue || flags.IsQuestionResult || flags.IsCommaResult ||
                flags.ConstructorCopyImpliedSource || flags.ConstructorImplicitCopy || flags.ConstructorValueInitialization || flags.ConstructorSequencedArguments ||
                flags.ConstructorArrayCopy || flags.FreesStorage || flags.FreesArray)
                throw Limit("The integer EH cleanup/parameter recipe must be metadata-only with its exact retained ownership role.", "exception-cleanup");
        }

        private void ValidateExceptionReleaseRoutine(FunctionSymbol release)
        {
            var signature = release.Signature;
            // Recognition needs the actual no-object action, freeing recipe and
            // lowering profile above, in addition to this exact helper contract.
            if (release.CurrentName != "__destroy_exception_object" || release.BodyState.Name != "declaration_only" || release.Body is not null ||
                release.PrimaryEntry is not null || release.AlternateEntryPoints.Count != 0 || release.Details.IsWeak == true || release.Details.IsAliasEntry == true ||
                release.Details.IsWeakref == true || release.Details.IsGnuAlias == true || release.Details.IsIfunc == true ||
                !signature.HasPrototype || signature.IsVariadic || signature.HasThisParameter || signature.ReturnsValueAsParameter == true || signature.ReturnsValueViaCopyConstructor ||
                signature.Parameters.Count != 0 || MemoryRepresentationType(signature.ReturnType).Kind.Name != "tk_void" ||
                signature.LanguageLinkage?.Name != "nlk_cplusplus_external" || options.ExternalFunctions.Any(binding => binding.LinkageName == release.CurrentName))
                throw Limit("The selected no-object cleanup target does not match the bounded generated exception-owner runtime contract.", "exception-runtime");
            ValidateCallingConvention(signature);
            var contract = boundedRethrow ? SemanticRuntimeFunctionImplementation.Int32RethrowOwnerContract : SemanticRuntimeFunctionImplementation.Int32ExceptionOwnerContract;
            if (!semanticRuntimeFunctions.Any(implementation => implementation.Function.Equals(release)))
                semanticRuntimeFunctions.Add(new(release, contract));
            else if (semanticRuntimeFunctions.Any(implementation => implementation.Function.Equals(release) && implementation.Contract != contract))
                throw Limit("One exception cleanup identity cannot select conflicting generated ownership contracts.", "exception-runtime");
        }

        private void ValidateExceptionMarker(LoweredEhOperation marker, FunctionSymbol function, ExceptionTry plan)
        {
            var eh = function.Body!.ExceptionHandling;
            var payload = marker.ExceptionOperation;
            if (marker.RepresentationRole.Name != "execution" || marker.ValueCategory.Name != "prvalue" || marker.Operands.Count != 0 || marker.Details.VolatileFetch ||
                (payload.FunctionEh is { } owner ? !owner.Routine.Equals(function) :
                    automaticArray?.Constructor.Equals(function) != true || marker.ExceptionKind.Name != "leck_thrown_object_address") ||
                payload.Initialization is not null || payload.TryExpression is not null || payload.CatchExpression is not null)
                throw Limit("A lowered integer EH marker has additional execution associations.", "exception");
            if (marker is CleanupStateOperation state)
            {
                if (marker.ExceptionKind.Name != (state.IsUnreachable ? "leck_unreachable_cleanup_state" : "leck_cleanup_state") ||
                    !Equals(payload.Region, state.Region) || payload.StateNumber != state.StateNumber || payload.Routine is not null || payload.Handler is not null || payload.TryBlock is not null ||
                    (state.Region is { } region ? !eh.CleanupRegions.Contains(region) || state.StateNumber != region.Ordinal :
                        state.StateNumber != eh.Details!.NullState && !eh.CleanupRegions.Any(region => state.StateNumber == region.Ordinal)) ||
                    MemoryRepresentationType(marker.Type).Kind.Name != "tk_void")
                    throw Limit("The cleanup state must name its actual region or captured null state.", "exception-cleanup");
                return;
            }
            if (payload.Region is not null || payload.StateNumber is not null) throw Limit("An EH marker unexpectedly selects a cleanup state.", "exception");
            var kind = marker.ExceptionKind.Name;
            if (kind is "leck_thrown_object_address" or "leck_caught_object_address")
            {
                ValidateNativeScalarType(marker.Type, false);
                if (MemoryRepresentationType(marker.Type) is not PointerType { IsReference: false } pointer || MemoryRepresentationType(pointer.PointeeType).Kind.Name != "tk_void" ||
                    payload.Routine is not null || payload.TryBlock is not null ||
                    (kind == "leck_thrown_object_address" ? payload.Handler is not null || !ExceptionExecutionNodes(selectedIntegerThrow!.Operand!).Contains(marker)
                        : payload.Handler?.Equals(plan.Handler) != true || !ExceptionExecutionNodes(plan.Handler.Body).Contains(marker)))
                    throw Limit("An exception payload-address marker is not associated with its retained throw or handler.", "exception");
                return;
            }
            if (MemoryRepresentationType(marker.Type).Kind.Name != "tk_void") throw Limit("This lowered EH marker must be a void operation.", "exception");
            var valid = kind switch
            {
                "leck_function_prologue" => marker.Equals(eh.Details!.Prologue) && payload.Routine is null && payload.Handler is null && payload.TryBlock is null,
                "leck_function_epilogue" => eh.Epilogues.Contains(marker) && payload.Routine?.Equals(function) == true && payload.Handler is null && payload.TryBlock is null,
                "leck_try_epilogue" => payload.TryBlock?.Equals(plan.Operation.Definition) == true && payload.Handler is null && payload.Routine is null,
                "leck_exception_caught" => payload.Handler is null && payload.TryBlock is null && payload.Routine is null && ExceptionExecutionNodes(plan.Handler.Body).Contains(marker),
                "leck_catch_epilogue" => payload.Handler?.Equals(plan.Handler) == true && payload.TryBlock is null && payload.Routine is null && ExceptionExecutionNodes(plan.Handler.Body).Contains(marker),
                _ => false
            };
            if (!valid) throw Limit($"Lowered EH operation '{kind}' is outside the selected integer exception contract.", "exception");
        }

        private void EnsureGeneratedNoThrow(FunctionSymbol function)
        {
            Check(function, []);
            void Check(FunctionSymbol current, HashSet<FunctionSymbol> active)
            {
                if (validatedNoThrowFunctions.Contains(current) || !active.Add(current)) return;
                if (!functions.Contains(current) || current.Body is null || current.Signature.ExceptionSpecification?.NoexceptEffective != true)
                    throw Limit("Normal marker omission requires a closed selected noexcept call graph.", "lifetime");
                var nodes = ExceptionExecutionNodes(current.Body.Root).ToArray();
                if (nodes.Any(node => node is ThrowOperation or TryOperation))
                    throw Limit("Normal-only lifetime marker omission cannot admit a live throw/try, including through noexcept calls.", "lifetime");
                foreach (var call in nodes.OfType<CallOperation>())
                {
                    if (call.Callee is not ReferenceOperation { Target: FunctionSymbol target } || call.TargetMethod?.Equals(target) != true || target.Signature.IsVariadic)
                        throw Limit("Normal marker omission cannot hide indirect/native/variadic exception propagation.", "lifetime");
                    Check(target, active);
                }
                active.Remove(current);
                validatedNoThrowFunctions.Add(current);
            }
        }

        private static bool FunctionReachesException(FunctionSymbol function, IReadOnlyDictionary<FunctionSymbol, SemanticNode[]> graph, HashSet<FunctionSymbol> active)
        {
            if (!active.Add(function)) return false;
            try
            {
                var nodes = graph[function];
                return nodes.Any(node => node is ThrowOperation or TryOperation) || nodes.OfType<CallOperation>().Any(call =>
                    call.Callee is ReferenceOperation { Target: FunctionSymbol target } && graph.ContainsKey(target) && FunctionReachesException(target, graph, active));
            }
            finally { active.Remove(function); }
        }

        private bool IsOmittedExceptionRegionTable(VariableSymbol variable) => currentFunction is { } function &&
            exceptionFunctions.TryGetValue(function, out var plan) && (plan.RegionTable.Equals(variable) ||
                automaticArray?.Function.Equals(function) == true && function.Body!.ExceptionHandling.ArrayTable?.Equals(variable) == true);

        private bool IsGeneratedExceptionParticipant(FunctionSymbol function) => exceptionParticipants.Contains(function) ||
            semanticRuntimeFunctions.Any(implementation => implementation.Function.Equals(function));

        private void ReserveExceptionLocals(FunctionSymbol function, HashSet<string> localNames)
        {
            if (!exceptionFunctions.TryGetValue(function, out var plan)) return;
            localNames.UnionWith([plan.State, plan.Observed, plan.ObservedParameter, plan.Caught]);
            foreach (var selectedTry in exceptionTries.Values.Where(selectedTry => selectedTry.Function.Equals(function)))
                localNames.UnionWith([selectedTry.SavedState, selectedTry.Carrier]);
        }

        private void EmitExceptionFields(StringBuilder fields)
        {
            if (exceptionTypeToken is null) return;
            fields.Append("    private static readonly object ").Append(exceptionTypeToken).AppendLine(" = new();");
            foreach (var plan in exceptionTries.Values)
                fields.Append("    private static readonly object ").Append(plan.Token).AppendLine(" = new();");
            fields.AppendLine();
        }

        private void BeginExceptionFunction(CodeWriter writer)
        {
            if (currentFunction is null || !exceptionFunctions.TryGetValue(currentFunction, out var plan)) return;
            writer.Line($"int {plan.State} = 65535;");
            writer.Line($"{ExceptionCarrierType}? {plan.Observed} = null;");
            if (exceptionTries.Values.Any(selectedTry => selectedTry.Function.Equals(currentFunction)))
                writer.Line($"{ExceptionCarrierType}? {plan.Caught} = null;");
            writer.Line("try"); writer.Open();
        }

        private void EndExceptionFunction(CodeWriter writer)
        {
            if (currentFunction is null || !exceptionFunctions.TryGetValue(currentFunction, out var plan)) return;
            writer.Close();
            writer.Line($"catch ({ExceptionCarrierType} {plan.ObservedParameter}) when ({ExceptionCarrierType}.ObserveUnwind({plan.ObservedParameter}, ref {plan.Observed}))");
            writer.Open(); writer.Line("throw;"); writer.Close();
            writer.Line("finally"); writer.Open();
            writer.Line($"if ({plan.Observed} is not null)"); writer.Open();
            writer.Line($"{plan.Observed}.RequireSelectedHandler();");
            EmitExceptionCleanup(plan, "65535", writer);
            writer.Close(); writer.Close();
        }

        private void EmitExceptionCleanup(ExceptionFunction plan, string stop, CodeWriter writer)
        {
            writer.Line($"while ({plan.State} != {stop})"); writer.Open();
            writer.Line($"switch ({plan.State})"); writer.Open();
            foreach (var region in plan.Regions)
            {
                writer.Line($"case {region.Ordinal.ToString(CultureInfo.InvariantCulture)}:");
                var action = region.Actions[0];
                if (automaticArray?.Cleanup.Equals(action) == true)
                    writer.Line($"    {Owner}.{automaticArray.DestroyMethod}((void*)(&{names[automaticArray.Variable]}), {automaticArray.Type.Bound!.Value.ToString(CultureInfo.InvariantCulture)}UL, {automaticArray.Element.Size!.Value.ToString(CultureInfo.InvariantCulture)}UL);");
                else if (action.Storage!.Kind.Name == "direct_variable")
                    writer.Line($"    {OwnedName(action.TargetMethod!)}(&{names[action.Storage.Variable!]});");
                else if (receiverMemberCleanups.TryGetValue(action, out var member))
                    writer.Line($"    {OwnedName(action.TargetMethod!)}(({TypeName(member.MemberType)}*)((byte*){names[member.Receiver]} + {action.Storage.Offset.ToString(CultureInfo.InvariantCulture)}));");
                else
                {
                    if (!exceptionReleaseHandlers.TryGetValue(action, out var handler) || !handler.Function.Equals(plan.Function))
                        throw Limit("The no-object cleanup lost its exact handler-scope ownership association.", "exception-runtime");
                    writer.Line($"    if ({plan.Caught} is null) throw new InvalidOperationException(\"No eligible caught exception owner for the retained cleanup action.\");");
                    writer.Line($"    {plan.Caught}.ReleaseCaught({Owner}.{handler.Token});");
                    writer.Line($"    {plan.Caught} = null;");
                }
                writer.Line($"    {plan.State} = {(region.NextRegion is { } next ? next.Ordinal.ToString(CultureInfo.InvariantCulture) : "65535")};");
                writer.Line("    break;");
            }
            writer.Line("default: throw new InvalidOperationException(\"The captured cleanup chain did not reach its saved stop state.\");");
            writer.Close(); writer.Close();
        }

        private bool TryEmitExceptionStatement(StatementOperation statement, CodeWriter writer)
        {
            if (statement is not TryOperation selectedTry || !exceptionTries.TryGetValue(selectedTry, out var plan)) return false;
            if (currentFunction is null || !exceptionFunctions.TryGetValue(currentFunction, out var function))
                throw Limit("The typed exception try has no selected cleanup-function adapter.", "exception");
            writer.Line($"int {plan.SavedState} = {function.State};");
            writer.Line("try"); Embedded(selectedTry.Body, writer);
            writer.Line($"catch ({ExceptionCarrierType} {plan.Carrier}) when ({plan.Carrier}.SelectHandler({Owner}.{exceptionTypeToken}, {Owner}.{plan.Token}))");
            writer.Open();
            writer.Line($"{plan.Carrier}.RequireSelectedHandler();");
            EmitExceptionCleanup(function, plan.SavedState, writer);
            writer.Line($"{function.Caught} = {plan.Carrier};");
            var previous = emittingExceptionHandler;
            emittingExceptionHandler = plan;
            try { Statement(plan.Handler.Body, writer); }
            finally { emittingExceptionHandler = previous; }
            writer.Close();
            return true;
        }

        private bool TryEmitExceptionExpressionStatement(ExpressionOperation expression, CodeWriter writer)
        {
            if (expression is not ThrowOperation throwing) return false;
            if (throwing.IsRethrow)
            {
                if (!selectedRethrows.TryGetValue(throwing, out var handler) || !handler.Equals(emittingExceptionHandler) ||
                    currentFunction is null || !handler.Function.Equals(currentFunction))
                    throw Limit("An operandless rethrow must occur in its exact selected active handler context.", "exception-rethrow");
                writer.Line($"throw {handler.Carrier}.CreateRethrow({Owner}.{handler.Token});");
                return true;
            }
            if (!throwing.Equals(selectedIntegerThrow) || throwing.ThrownType?.Equals(exceptionIntType) != true || currentFunction is null || !currentFunction.Equals(selectedIntegerThrowFunction))
                throw Limit("This throw has no selected bounded integer transfer adapter.", "exception");
            var owner = ExceptionLocal("__throw_owner");
            writer.Line($"{ExceptionCarrierType} {owner} = {ExceptionCarrierType}.CreateInt32({Owner}.{exceptionTypeToken});");
            writer.Line("try"); writer.Open();
            var previous = pendingThrowOwner;
            pendingThrowOwner = owner;
            try { EmitPlannedExpressionStatement(throwing.Operand!, writer); }
            finally { pendingThrowOwner = previous; }
            writer.Line($"{owner}.CommitThrow();"); writer.Line($"throw {owner};"); writer.Close();
            writer.Line("finally"); writer.Open(); writer.Line($"{owner}.ReleaseUncommitted();"); writer.Close();
            return true;
        }

        private bool NeedsExceptionExpressionPlan(ExpressionOperation expression) =>
            expression is LoweredEhOperation marker && exceptionMarkers.ContainsKey(marker) || expression is CallOperation call && automaticArrayCalls.ContainsKey(call);

        private bool TryPlanExceptionExpression(ExpressionOperation expression, out ExpressionPlan plan)
        {
            plan = null!;
            if (expression is not LoweredEhOperation marker || !exceptionMarkers.TryGetValue(marker, out var function) || !function.Equals(currentFunction)) return false;
            exceptionFunctions.TryGetValue(function, out var adapter);
            var kind = marker.ExceptionKind.Name;
            var prefix = new List<CsPlanStatement>();
            CsExpression value = new CsLeaf(CsType.Void, "", true);
            var effects = ExpressionEffects.None;
            if (marker is CleanupStateOperation state)
            {
                if (!state.IsUnreachable)
                {
                    if (adapter is null) throw Limit("A table-free constructor cannot acquire a fabricated cleanup state.", "array-lifetime");
                    prefix.Add(new CsSetVariable(new(CsType.Int32, adapter.State), new CsLeaf(CsType.Int32, state.StateNumber!.Value.ToString(CultureInfo.InvariantCulture), true)));
                    effects = ExpressionEffects.WriteStorage;
                }
            }
            else if (kind == "leck_thrown_object_address")
            {
                if (pendingThrowOwner is null) throw Limit("The thrown-object address escaped its retained pending initialization.", "exception-owner");
                value = new CsLeaf(CsType.Value(marker.Type), pendingThrowOwner + ".ObjectAddress", true);
            }
            else if (kind is "leck_caught_object_address" or "leck_exception_caught" or "leck_catch_epilogue")
            {
                var handler = emittingExceptionHandler ?? throw Limit("The caught-object operation is outside its actual selected handler occurrence.", "exception-owner");
                if (kind == "leck_caught_object_address")
                    value = new CsLeaf(CsType.Value(marker.Type), $"{handler.Carrier}.CaughtObjectAddress({Owner}.{handler.Token})", true);
                else if (kind == "leck_exception_caught")
                {
                    value = new CsLeaf(CsType.Void, $"{handler.Carrier}.MarkCaught({Owner}.{handler.Token})", false);
                    effects = ExpressionEffects.WriteStorage | ExpressionEffects.MayThrow;
                }
                else
                {
                    prefix.Add(new CsEvaluate(new CsLeaf(CsType.Void, $"{handler.Carrier}.ReleaseCaught({Owner}.{handler.Token})", false)));
                    prefix.Add(new CsSetVariable(new(new(null, Intrinsic: ExceptionCarrierType + "?"), adapter!.Caught), new CsLeaf(new(null, Intrinsic: ExceptionCarrierType + "?"), "null", true)));
                    effects = ExpressionEffects.WriteStorage | ExpressionEffects.MayThrow;
                }
            }
            plan = new(marker.Type, marker.ValueCategory, prefix, value, effects);
            return true;
        }

        private static bool IsStandaloneThrow(SemanticNode root, ThrowOperation throwing) => ExceptionExecutionNodes(root).OfType<StatementOperation>().Any(statement =>
            statement.Expression?.Equals(throwing) == true || statement.Expression is ConversionOperation conversion &&
            MemoryRepresentationType(conversion.Type).Kind.Name == "tk_void" && conversion.Operand.Equals(throwing));

        // Validation only: typed execution facets expose repeated occurrences.
        // Structural child order, metadata recipes and generic descendants do
        // not choose application execution or destruction order.
        private static IEnumerable<SemanticNode> ExceptionExecutionNodes(SemanticNode root)
        {
            return Walk(root, [], 0);
            static IEnumerable<SemanticNode> Walk(SemanticNode node, HashSet<SemanticNode> active, int depth)
            {
                if (depth > 256 || !active.Add(node)) throw Limit("The bounded integer EH validation graph is cyclic or too deep.", "exception");
                try
                {
                    if (node.RepresentationRole.Name == "metadata") throw Limit("A typed integer EH execution occurrence names metadata.", "exception");
                    yield return node;
                    IEnumerable<SemanticNode> children = node switch
                    {
                        BlockOperation block => block.Statements,
                        ConditionalOperation conditional => new SemanticNode?[] { conditional.Condition, conditional.Then, conditional.Else }.OfType<SemanticNode>(),
                        LoopOperation loop => new SemanticNode?[] { loop.Initialization, loop.Condition, loop.Increment, loop.Body }.OfType<SemanticNode>(),
                        TryOperation selectedTry => new[] { selectedTry.Body }.Concat(selectedTry.Handlers.Select(handler => handler.Body)),
                        JumpOperation { JumpKind.Name: "return" } jump => jump.Value is { } value ? [value] : [],
                        DeclarationOperation => [],
                        InitializationStatementOperation initialization => initialization.Initialization.Expression is { } value ? [value] : [],
                        LifetimeOperation lifetime => [lifetime.Value],
                        ConditionOperation { Initialization: null, DynamicInitialization: null, Value: { } value } => [value],
                        ThrowOperation throwing => throwing.Operand is { } value ? [value] : [],
                        CallOperation call => call.Operands,
                        LoweredEhOperation marker => marker.Operands,
                        ReferenceOperation or ConstantOperation => [],
                        OperatorOperation operation => operation.Operands,
                        StatementOperation { Kind.Name: "stmk_expr", Expression: { } value } => [value],
                        StatementOperation { Kind.Name: "stmk_empty" } => [],
                        _ => throw Limit($"Execution form '{node.Kind}' is outside the bounded integer exception adapter.", "exception")
                    };
                    foreach (var child in children)
                        foreach (var descendant in Walk(child, active, depth + 1)) yield return descendant;
                }
                finally { active.Remove(node); }
            }
        }
    }
}
