using System.Text;
using RoslynObj;

namespace RoslynObj.CSharp;

public sealed partial class CSharpEmitter
{
    private sealed partial class Emission
    {
        private const string NoexceptTerminationPolicy = "global::RoslynObj.CSharp.Runtime.ManagedCppTerminationPolicy.UnwindBeforeTerminateDefaultFailFastV1";
        private sealed record NoexceptTermination(FunctionSymbol Function, string Token, string Carrier);
        private NoexceptTermination? noexceptTermination;

        private string SelectedExceptionDestinationAssertion => noexceptTermination is null ? "RequireSelectedHandler" : "RequireSelectedDestination";

        // The public noexcept fact identifies a local execution boundary. It
        // does not turn an arbitrary library entry into an uncaught-program
        // boundary. The selected provider policy here is deliberately the
        // stock unwind-before-default-termination policy, not a manifest fact.
        private bool TryInitializeNoexceptTermination(IReadOnlyDictionary<FunctionSymbol, SemanticNode[]> graph,
            IReadOnlyList<FunctionSymbol> candidates, IReadOnlyList<ThrowOperation> throws, IReadOnlyList<TryOperation> tries)
        {
            var boundaries = functions.Where(function => function.Signature.ExceptionSpecification?.NoexceptEffective == true &&
                FunctionReachesException(function, graph, [])).ToArray();
            if (boundaries.Length == 0) return false;
            if (boundaries.Length != 1 || throws.Count != 1 || tries.Count != 1 || candidates.Count != 2 || throws[0].IsRethrow ||
                functions.Any(function => function.Body!.ExceptionHandling.Arrays.Count != 0 || function.Body!.ExceptionHandling.ArrayTable is not null))
                throw Limit("Default noexcept termination requires one actual boundary, integer throw and outer typed try without arrays or rethrow.", "exception-termination");
            var boundary = boundaries[0];
            var throwing = throws[0];
            var selectedTry = tries[0];
            var throwingFunction = graph.Single(pair => pair.Value.Contains(throwing)).Key;
            var outerFunction = graph.Single(pair => pair.Value.Contains(selectedTry)).Key;
            if (throwingFunction.Equals(outerFunction) || throwingFunction.Equals(boundary) || outerFunction.Equals(boundary) ||
                !candidates.Contains(throwingFunction) || !candidates.Contains(outerFunction) ||
                throwingFunction.Body!.ExceptionHandling.CleanupRegions.Count != 1 || boundary.Body!.ExceptionHandling.CleanupRegions.Count != 1 ||
                outerFunction.Body!.ExceptionHandling.CleanupRegions.Count != 2 || graph[boundary].Any(node => node is ThrowOperation or TryOperation) ||
                !IsStandaloneThrow(throwingFunction.Body!.Root, throwing))
                throw Limit("The termination slice requires separate leaf, noexcept-boundary and outer-handler cleanup frames.", "exception-termination");
            ValidateTerminationRoutine(boundary, true);
            ValidateTerminationRoutine(throwingFunction, false);
            if (throwing.ThrownType is not { } thrownType || throwing.Initialization is not null || throwing.Destructor is not null ||
                throwing.AccessibleBases.Count != 0 || throwing.Operands.Count != 0 || throwing.Operand is null ||
                throwing.ValueCategory.Name != "prvalue" || MemoryRepresentationType(throwing.Type).Kind.Name != "tk_void")
                throw Limit("The default termination payload requires its retained lowered integer initializer.", "exception-termination");
            ValidateGeneratedInt32(thrownType);
            if (throwing.Operand is not OperatorOperation { OperatorKind.Name: "eok_assign" } assignment || !assignment.Operator.EvalRightToLeft ||
                assignment.Operands.Count != 2 || !assignment.Operands[1].Type.Equals(thrownType) ||
                assignment.Operands[0] is not OperatorOperation { OperatorKind.Name: "eok_indirect" } destination || !destination.Type.Equals(thrownType) ||
                destination.Operands.Count != 1 || destination.Operands[0] is not ConversionOperation { OperatorKind.Name: "eok_cast" } cast ||
                MemoryRepresentationType(cast.Type) is not PointerType { IsReference: false } pointer || !pointer.PointeeType.Equals(thrownType) ||
                cast.Operand is not LoweredEhOperation { ExceptionKind.Name: "leck_thrown_object_address" } ||
                selectedTry.Definition.IsFunctionTry || selectedTry.Handlers.Count != 1 || selectedTry.Handlers[0].CaughtType?.Equals(thrownType) != true)
                throw Limit("The termination leaf and retained outer handler must have the same exact integer payload profile.", "exception-termination");
            var handler = selectedTry.Handlers[0];
            if (handler.Parameter is not { } parameter || !parameter.Type.Equals(thrownType) ||
                handler.ParameterInitialization is not { } recipe || recipe.Target?.Equals(parameter) != true || recipe.Destructor is not null || recipe.Lifetime is not null ||
                ExceptionExecutionNodes(handler.Body).Any(node => node is ThrowOperation or TryOperation))
                throw Limit("The unselected outer handler must retain its ordinary integer parameter and metadata-only initialization.", "exception-termination");
            ValidateExceptionMetadataRecipe(recipe, false);

            var calls = graph.Values.SelectMany(nodes => nodes.OfType<CallOperation>()).ToArray();
            foreach (var call in calls)
                if (call.Callee is not ReferenceOperation { Target: FunctionSymbol target } || !functions.Contains(target) ||
                    call.TargetMethod?.Equals(target) != true || target.Signature.IsVariadic)
                    throw Limit("The default termination graph admits only selected fixed direct managed calls, without a native unwinding boundary.", "exception-boundary");
            var leafCalls = calls.Where(call => call.TargetMethod?.Equals(throwingFunction) == true).ToArray();
            var boundaryCalls = calls.Where(call => call.TargetMethod?.Equals(boundary) == true).ToArray();
            if (leafCalls.Length != 1 || !graph[boundary].Contains(leafCalls[0]) || boundaryCalls.Length != 1 ||
                !ExceptionExecutionNodes(selectedTry.Body).Contains(boundaryCalls[0]) ||
                boundary.Parameters[0].Type.Equals(thrownType) != true || throwingFunction.Parameters[0].Type.Equals(thrownType) != true)
                throw Limit("The outer try must call the actual noexcept boundary, which alone calls the selected throwing leaf.", "exception-termination");
            foreach (var function in functions.Where(function => !function.Equals(boundary) && function.Signature.ExceptionSpecification?.NoexceptEffective == true))
                EnsureGeneratedNoThrow(function);
            foreach (var call in ExceptionExecutionNodes(handler.Body).OfType<CallOperation>())
                EnsureGeneratedNoThrow((FunctionSymbol)((ReferenceOperation)call.Callee).Target!);
            foreach (var call in graph[boundary].OfType<CallOperation>().Where(call => !call.Equals(leafCalls[0])))
                EnsureGeneratedNoThrow((FunctionSymbol)((ReferenceOperation)call.Callee).Target!);

            noexceptTermination = new(boundary, InternalName("__cpp_noexcept_boundary"), ExceptionLocal("__terminating_cpp"));
            exceptionIntType = thrownType;
            selectedIntegerThrow = throwing;
            selectedIntegerThrowFunction = throwingFunction;
            exceptionTypeToken = InternalName("__cpp_int32_type");
            var tryPlan = new ExceptionTry(outerFunction, selectedTry, handler, InternalName("__cpp_handler"), ExceptionLocal("__try_entry_state"), ExceptionLocal("__caught_cpp"));
            exceptionTries.Add(selectedTry, tryPlan);
            foreach (var function in new[] { throwingFunction, boundary, outerFunction })
            {
                var nodes = graph[function];
                ValidateExceptionFunction(function, nodes, tryPlan);
                var eh = function.Body!.ExceptionHandling;
                if (!function.Equals(outerFunction) && (eh.CleanupRegions[0].NextRegion is not null ||
                    eh.CleanupRegions[0].Actions[0].Storage?.Kind.Name != "direct_variable"))
                    throw Limit("The leaf and boundary each require one direct-local cleanup that reaches their captured null state.", "exception-termination");
                exceptionFunctions.Add(function, new(function, eh.RegionTable!, eh.CleanupRegions,
                    ExceptionLocal("__cleanup_state"), ExceptionLocal("__unwinding_cpp"), ExceptionLocal("__observed_cpp"), ExceptionLocal("__active_caught_cpp")));
                foreach (var marker in nodes.OfType<LoweredEhOperation>()) exceptionMarkers.Add(marker, function);
            }
            if (semanticRuntimeFunctions.Count != 1 || semanticRuntimeFunctions[0].Contract != SemanticRuntimeFunctionImplementation.Int32NoexceptTerminationOwnerContract ||
                exceptionReleaseHandlers.Count != 1 || !exceptionReleaseHandlers.Values.Single().Equals(tryPlan))
                throw Limit("The retained outer cleanup dependency requires its exact default-termination owner contract, without claiming that handler ran.", "exception-runtime");
            foreach (var function in functions)
                if (FunctionReachesException(function, graph, [])) exceptionParticipants.Add(function);
            return true;
        }

        private static void ValidateTerminationRoutine(FunctionSymbol function, bool boundary)
        {
            var signature = function.Signature;
            if (!signature.HasPrototype || signature.IsVariadic || signature.HasThisParameter || signature.ReturnsValueAsParameter == true ||
                signature.ReturnsValueViaCopyConstructor || signature.ReturnValueParameterFollowsThis == true || function.PrimaryEntry is not null ||
                function.AlternateEntryPoints.Count != 0 || function.SpecialFunctionKind?.Name is not (null or "sfk_none") ||
                function.Details.IsWeak == true || function.Details.IsWeakref == true || function.Details.IsGnuAlias == true || function.Details.IsIfunc == true ||
                function.Details.IsAliasEntry == true || MemoryRepresentationType(function.ReturnType).Kind.Name != "tk_void" ||
                function.Parameters.Count != 1 || signature.Parameters.Count != 1 || function.Parameters[0].Role.Name != "ordinary_parameter" ||
                function.Parameters[0].Symbol is null || !function.Parameters[0].Descriptor.Equals(signature.Parameters[0].Descriptor) ||
                (signature.ExceptionSpecification?.NoexceptEffective == true) != boundary)
                throw Limit("The termination adapter requires the exact ordinary generated leaf and resolved noexcept boundary signatures.", "exception-termination");
            ValidateCallingConvention(signature);
            ValidateGeneratedInt32(function.Parameters[0].Type);
            if (boundary && (signature.ExceptionSpecification is not { } specification || specification.NoexceptEffective != true ||
                specification.ThrowAny || specification.IsDependent || specification.IsIndeterminate || specification.Types.Count != 0))
                throw Limit("Default termination requires the actual resolved true noexcept specification without a dynamic type list.", "exception-termination");
        }

        private void EmitNoexceptTerminationField(StringBuilder fields)
        {
            if (noexceptTermination is { } plan)
                fields.Append("    private static readonly object ").Append(plan.Token).AppendLine(" = new();");
        }

        private void BeginNoexceptTerminationBoundary(CodeWriter writer)
        {
            if (noexceptTermination?.Function.Equals(currentFunction) != true) return;
            // This outer destination is encountered in CLR first-pass search
            // after the function observer but before any caller source catch.
            writer.Line("try"); writer.Open();
        }

        private void EndNoexceptTerminationBoundary(CodeWriter writer)
        {
            if (noexceptTermination is not { } plan || !plan.Function.Equals(currentFunction)) return;
            writer.Close();
            writer.Line($"catch ({ExceptionCarrierType} {plan.Carrier}) when ({plan.Carrier}.SelectNoexceptBoundary({Owner}.{plan.Token}, {NoexceptTerminationPolicy}))");
            writer.Open();
            writer.Line($"{plan.Carrier}.TerminateNoexceptBoundary({Owner}.{plan.Token}, {NoexceptTerminationPolicy});");
            writer.Close();
        }
    }
}
