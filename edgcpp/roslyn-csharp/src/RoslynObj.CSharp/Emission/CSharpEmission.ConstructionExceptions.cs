using System.Text;
using RoslynObj;

namespace RoslynObj.CSharp;

public sealed partial class CSharpEmitter
{
    private sealed partial class Emission
    {
        private sealed record ConstructionThrow(FunctionSymbol Function, ThrowOperation Throw, CallOperation Call, LoweredEhOperation Destination);
        private sealed record ConstructionExceptionContext(RecordType Record, VariableSymbol Typeinfo, IReadOnlyList<FunctionSymbol> Constructors,
            FunctionSymbol Destructor, IReadOnlyList<PointerType> ArgumentReferences, TryOperation Try,
            ExceptionHandler Handler, PointerType HandlerReference, FunctionSymbol CatchingFunction,
            IReadOnlyList<ConstructionThrow> Throws);
        private ConstructionExceptionContext? constructionExceptionContext;
        private bool constructionExceptionActive;
        private string? constructionDestructorThunk;
        private readonly HashSet<Initialization> constructionTemporaryRecipes = [];
        private readonly Dictionary<ExpressionOperation, FunctionSymbol> constructionStatementRoots = [];

        private bool TryConstructionTypeinfoContext(out RecordType record, out VariableSymbol typeinfo)
        {
            record = null!; typeinfo = null!;
            if (constructionExceptionContext is { } cached) { record = cached.Record; typeinfo = cached.Typeinfo; return true; }
            var graph = functions.ToDictionary(function => function, function => ExceptionExecutionNodes(function.Body!.Root).ToArray());
            var throws = graph.Values.SelectMany(nodes => nodes.OfType<ThrowOperation>()).Distinct().ToArray();
            if (throws.Length == 0 || !throws.All(throwing => throwing.ThrownType is RecordType && throwing.Operand is CallOperation)) return false;
            var tries = graph.Values.SelectMany(nodes => nodes.OfType<TryOperation>()).Distinct().ToArray();
            if (tries.Length != 1 || throws[0].ThrownType is not RecordType payload || throws[0].Destructor is not { } destructor ||
                payload.Details.TypeinfoVariable is not { } retainedTypeinfo)
                throw Limit("Direct record construction requires one exact payload typeinfo and one retained typed handler.", "exception-construction");
            RequireNormalLifetimeProfile();
            ValidateClassPayloadRecord(payload);
            ValidateClassLifetimeSignature(destructor, payload, false);
            var selected = new List<ConstructionThrow>();
            var constructors = new HashSet<FunctionSymbol>();
            foreach (var throwing in throws)
            {
                if (throwing.IsRethrow || !payload.Equals(throwing.ThrownType) || throwing.Destructor?.Equals(destructor) != true ||
                    throwing.Initialization is not null || throwing.AccessibleBases.Count != 0 || throwing.Operands.Count != 0 ||
                    throwing.RepresentationRole.Name != "execution" || throwing.ValueCategory.Name != "prvalue" ||
                    MemoryRepresentationType(throwing.Type).Kind.Name != "tk_void" || throwing.Operand is not CallOperation call ||
                    call.Callee is not ReferenceOperation { Operands.Count: 0, Target: FunctionSymbol target } || !functions.Contains(target) ||
                    call.TargetMethod?.Equals(target) != true || call.CallKind.Name != "direct" || !call.Operator.EvalLeftToRight || call.Operator.EvalRightToLeft ||
                    call.Operator.RetainedVirtualCall || call.Operator.RequiresRuntimeCastCheck || call.SourceDispatchKind is not null || call.SourceTargetMethod is not null ||
                    call.RepresentationRole.Name != "execution" || call.ValueCategory.Name != "prvalue" || call.Call.ReturnSlot is not null ||
                    call.Call.FunctionType?.Equals(target.Signature) != true || !call.Type.Equals(target.ReturnType) ||
                    call.Arguments.Count == 0 || call.Arguments.Count != target.Parameters.Count || call.Arguments.Count != target.Signature.Parameters.Count ||
                    call.Operands.Count != call.Arguments.Count + 1 || !call.Operands[0].Equals(call.Callee) ||
                    call.Arguments[0].Value is not ConversionOperation { OperatorKind.Name: "eok_cast", Operands.Count: 1 } cast ||
                    cast.Operand is not LoweredEhOperation { ExceptionKind.Name: "leck_thrown_object_address" } destination ||
                    !cast.Type.Equals(target.Parameters[0].Type) || cast.Operator.RequiresRuntimeCastCheck ||
                    call.Receiver?.Equals(cast) != true)
                    throw Limit("A direct record throw requires its actual selected C1 operand and pending receiver in retained left-to-right argument order.", "exception-construction");
                constructors.Add(target);
                for (var index = 0; index < call.Arguments.Count; ++index)
                {
                    var actual = call.Arguments[index]; var formal = target.Signature.Parameters[index];
                    if (actual.Ordinal != index || !call.Operands[index + 1].Equals(actual.Value) ||
                        actual.Parameter?.Equals(formal.Descriptor) != true || actual.Role?.Equals(formal.Role) != true)
                        throw Limit("The construction arguments must already contain the selected receiver and ordinary roles exactly once.", "exception-construction");
                }
                var owners = graph.Where(pair => pair.Value.Contains(throwing)).Select(pair => pair.Key).ToArray();
                if (owners.Length != 1) throw Limit("A direct construction throw has no unique selected body owner.", "exception-construction");
                ValidateClassPendingMarker(destination, owners[0], true);
                selected.Add(new(owners[0], throwing, call, destination));
            }
            var argumentReferences = constructors.SelectMany(constructor => ValidateConstructionSignature(constructor, payload)).Distinct().ToArray();
            var selectedTry = tries[0];
            if (selectedTry.Definition.IsFunctionTry || selectedTry.Handlers.Count != 1 ||
                selectedTry.Handlers[0] is not { CaughtType: PointerType { IsReference: true, IsRValueReference: false } reference, Parameter: { } parameter } handler ||
                reference.PointeeType is not QualifiedType { IsConst: true, IsVolatile: false } qualified || !qualified.UnderlyingType.Equals(payload) ||
                reference.Size != 8 || reference.Alignment != 8 || qualified.Size != payload.Size || qualified.Alignment != payload.Alignment || !parameter.Type.Equals(reference) ||
                handler.Details.Typeinfo?.Equals(retainedTypeinfo) != true)
                throw Limit("The direct construction handler requires its retained const-reference qualification of the identical payload and typeinfo.", "exception-construction");
            var catchers = graph.Where(pair => pair.Value.Contains(selectedTry)).Select(pair => pair.Key).ToArray();
            if (catchers.Length != 1 || selected.Any(item => item.Function.Equals(catchers[0])))
                throw Limit("The direct construction handler must belong to its distinct selected catching function.", "exception-construction");
            constructionExceptionContext = new(payload, retainedTypeinfo, constructors.ToArray(), destructor, argumentReferences,
                selectedTry, handler, reference, catchers[0], selected);
            record = payload; typeinfo = retainedTypeinfo;
            return true;
        }

        private IReadOnlyList<PointerType> ValidateConstructionSignature(FunctionSymbol function, RecordType payload)
        {
            var references = new List<PointerType>();
            var signature = function.Signature;
            if (!functions.Contains(function) || function.Body is null || function.PrimaryEntry is not null || function.IsVirtual || function.IsPureVirtual ||
                function.SpecialFunctionKind?.Name != "sfk_constructor" || function.ConstructorOrDestructorKind?.Name != "cdk_complete" ||
                function.ContainingType?.Equals(payload) != true || signature.ThisClass?.Equals(payload) != true ||
                !signature.HasPrototype || signature.IsVariadic || !signature.HasThisParameter || signature.ReturnsValueAsParameter == true ||
                signature.ReturnsValueViaCopyConstructor || signature.ReturnValueParameterFollowsThis == true ||
                signature.LanguageLinkage?.Name != "nlk_cplusplus_external" ||
                function.Parameters.Count == 0 || function.Parameters.Count != signature.Parameters.Count ||
                MemoryRepresentationType(function.ReturnType).Kind.Name != "tk_void" || !GeneratedRecordPointer(function.Parameters[0].Type, false).Equals(payload) ||
                function.Details.IsWeak == true || function.Details.IsWeakref == true || function.Details.IsGnuAlias == true ||
                function.Details.IsIfunc == true || function.Details.IsAliasEntry == true || function.GnuSupplement?.AliasedRoutine is not null ||
                function.GnuSupplement?.TargetedVersions.Count > 0 || function.GnuSupplement?.Representative is not null ||
                function.ThisAdjustment != 0 || function.ReturnAdjustment != 0 || function.VirtualCallIndex != 0 || function.VirtualBaseIndex != 0 ||
                function.OverridingFunctionForWrapper is not null || function.OverriddenFunctionForWrapper is not null)
                throw Limit("Direct construction requires its selected fixed complete C1 without implicit return, native or adjustment roles.", "exception-construction");
            ValidateCallingConvention(signature);
            for (var index = 0; index < function.Parameters.Count; ++index)
            {
                var actual = function.Parameters[index]; var formal = signature.Parameters[index];
                if (actual.Ordinal != index || formal.Ordinal != index || actual.Symbol is null || actual.BaseSubobject is not null || formal.BaseSubobject is not null ||
                    actual.Descriptor.Details.PassedViaCopyConstructor || !actual.Descriptor.Equals(formal.Descriptor) || !actual.Type.Equals(formal.Type) ||
                    actual.Role.Name != (index == 0 ? "this_parameter" : "ordinary_parameter") || !actual.Role.Equals(formal.Role))
                    throw Limit("The direct construction C1 has an unsupported hidden or adapted execution role.", "exception-construction");
                if (index == 0) continue;
                if (MemoryRepresentationType(actual.Type) is ArithmeticType)
                    ValidateNativeScalarType(actual.Type, false);
                else if (actual.Type is PointerType { IsReference: true, IsRValueReference: false } reference)
                {
                    _ = GeneratedRecordPointer(reference, true);
                    references.Add(reference);
                }
                else throw Limit("A direct constructor argument requires its supported natural scalar or exact const-record lvalue-reference role.", "exception-construction");
            }
            if (function.Body!.Locals.Any(variable => MemoryRepresentationType(variable.Type) is PointerType { IsReference: true }))
                throw Limit("Constructor parameter reference admission does not authorize additional unbound reference locals.", "exception-construction");
            return references;
        }

        private bool TryInitializeConstructionExceptions(IReadOnlyDictionary<FunctionSymbol, SemanticNode[]> graph,
            IReadOnlyList<FunctionSymbol> candidates, IReadOnlyList<ThrowOperation> throws, IReadOnlyList<TryOperation> tries)
        {
            if (constructionExceptionContext is not { } plan) return false;
            if (HostProtocolEnabled || boundedRethrow || noexceptTermination is not null || automaticArray is not null ||
                tries.Count != 1 || !tries[0].Equals(plan.Try) || throws.Count != plan.Throws.Count ||
                graph.Values.SelectMany(nodes => nodes).Any(node => node is AllocationOperation) ||
                graph.Values.SelectMany(nodes => nodes.OfType<LoweredEhOperation>()).Any(marker => marker.ExceptionKind.Name == "leck_exception_started") ||
                ExceptionExecutionNodes(plan.Handler.Body).Any(node => node is ThrowOperation or TryOperation) ||
                functions.Any(function => function.Body!.ExceptionHandling.Arrays.Count != 0 || function.Body!.ExceptionHandling.ArrayTable is not null))
                throw Limit("The direct construction profile has unsupported allocation, marker, handler, array or restart obligations.", "exception-construction");
            var reachable = new HashSet<FunctionSymbol>();
            VisitSelectedCalls(plan.CatchingFunction);
            if (candidates.Any(function => !reachable.Contains(function)) ||
                plan.Throws.Any(item => !candidates.Contains(item.Function)) || !candidates.Contains(plan.CatchingFunction))
                throw Limit("Construction EH frames must belong to the actual selected call closure, with retained tables for the throw and handler owners.", "exception-construction");
            void VisitSelectedCalls(FunctionSymbol function)
            {
                if (!reachable.Add(function)) return;
                if (!graph.TryGetValue(function, out var nodes))
                    throw Limit("A construction callee has no selected executable body.", "exception-construction");
                foreach (var call in nodes.OfType<CallOperation>())
                {
                    if (call.Callee is not ReferenceOperation { Target: FunctionSymbol target } || !graph.ContainsKey(target) ||
                        call.TargetMethod?.Equals(target) != true || call.CallKind.Name != "direct" || call.Call.FunctionType?.Equals(target.Signature) != true)
                        throw Limit("Construction propagation requires closed selected direct call identities and retained signatures.", "exception-construction");
                    VisitSelectedCalls(target);
                }
            }
            foreach (var pair in graph)
            {
                foreach (var call in pair.Value.OfType<CallOperation>())
                    if (call.Callee is not ReferenceOperation { Target: FunctionSymbol target } || !functions.Contains(target) ||
                        call.TargetMethod?.Equals(target) != true || target.Signature.IsVariadic)
                        throw Limit("Direct exception construction admits selected fixed generated calls only.", "exception-construction");
                foreach (var reference in pair.Value.OfType<ReferenceOperation>().Where(reference => reference.Target is FunctionSymbol target &&
                    (plan.Constructors.Contains(target) || target.Equals(plan.Destructor))))
                    if (!pair.Value.OfType<CallOperation>().Any(call => call.Callee.Equals(reference)))
                        throw Limit("The selected construction and payload destructor have no address-exposed or native callback adapter.", "exception-construction");
                foreach (var statement in pair.Value.OfType<StatementOperation>().Where(statement => statement.Expression is { } expression &&
                    ExceptionExecutionNodes(expression).OfType<ThrowOperation>().Any(throwing => plan.Throws.Any(item => item.Throw.Equals(throwing)))))
                {
                    var expression = statement.Expression!;
                    ValidateConstructionStatement(expression, pair.Key);
                    constructionStatementRoots[expression] = pair.Key;
                }
                foreach (var wrapper in pair.Value.OfType<LifetimeOperation>())
                {
                    if (wrapper.Lifetime is not { Kind.Name: "olk_expr_temporary" } lifetime || !plan.Throws.Any(item => item.Function.Equals(pair.Key)) ||
                        !ExceptionExecutionNodes(wrapper.Value).OfType<ThrowOperation>().Any() || lifetime.Cleanups.Count == 0 || lifetime.BoundEntity?.Equals(wrapper) != true)
                        throw Limit("A construction full-expression wrapper requires its actual temporary lifetime and retained cleanup associations.", "exception-construction");
                    foreach (var cleanup in lifetime.Cleanups)
                    {
                        var recipe = cleanup.Initialization;
                        if (!recipe.Details.HasTemporaryLifetime || recipe.Target is not VariableSymbol variable || !pair.Key.Body!.Locals.Contains(variable) ||
                            recipe.Destructor is not { Parameters.Count: 1 } destructor ||
                            !GeneratedRecordPointer(destructor.Parameters[0].Type, false).Equals(MemoryRepresentationType(variable.Type)) ||
                            pair.Key.Body!.ExceptionHandling.CleanupRegions.SelectMany(region => region.Actions).Count(action =>
                                action.Storage?.Variable?.Equals(variable) == true && action.TargetMethod?.Equals(destructor) == true) != 1)
                            throw Limit("The temporary recipe must match its actual local type, selected destructor and exceptional cleanup action.", "exception-construction");
                        constructionTemporaryRecipes.Add(recipe);
                    }
                }
            }
            foreach (var function in functions.Where(function => function.Signature.ExceptionSpecification?.NoexceptEffective == true)) EnsureGeneratedNoThrow(function);
            foreach (var call in ExceptionExecutionNodes(plan.Handler.Body).OfType<CallOperation>())
                EnsureGeneratedNoThrow((FunctionSymbol)((ReferenceOperation)call.Callee).Target!);
            if (plan.Handler.ParameterInitialization is not { } parameterRecipe || parameterRecipe.Target?.Equals(plan.Handler.Parameter) != true ||
                parameterRecipe.Details.Type?.Equals(plan.HandlerReference) != true || parameterRecipe.Destructor is not null || parameterRecipe.Lifetime is not null)
                throw Limit("The direct construction catch parameter requires its exact metadata-only borrowed address binding.", "exception-construction");
            ValidateExceptionMetadataRecipe(parameterRecipe, false);
            ValidateClassHandlerEntry(plan.Handler, plan.HandlerReference, plan.HandlerReference.PointeeType);
            ValidateConstructionHandlerReferenceContext(plan, graph[plan.CatchingFunction]);
            constructionExceptionActive = true;
            exceptionIntType = plan.Record;
            exceptionTypeToken = InternalName("__cpp_constructed_record_type");
            constructionDestructorThunk = InternalName("__DestroyConstructedCppRecord");
            generatedReferenceTypes.Add(plan.HandlerReference);
            foreach (var reference in plan.ArgumentReferences) generatedReferenceTypes.Add(reference);
            foreach (var constructor in plan.Constructors) validatedThrowingConstructors.Add(constructor);
            var tryPlan = new ExceptionTry(plan.CatchingFunction, plan.Try, plan.Handler, InternalName("__cpp_handler"),
                ExceptionLocal("__try_entry_state"), ExceptionLocal("__caught_cpp"));
            exceptionTries.Add(plan.Try, tryPlan);
            foreach (var function in candidates)
            {
                ValidateExceptionFunction(function, graph[function], tryPlan);
                var eh = function.Body!.ExceptionHandling;
                exceptionFunctions.Add(function, new(function, eh.RegionTable!, eh.CleanupRegions, ExceptionLocal("__cleanup_state"),
                    ExceptionLocal("__unwinding_cpp"), ExceptionLocal("__observed_cpp"), ExceptionLocal("__active_caught_cpp")));
                foreach (var marker in graph[function].OfType<LoweredEhOperation>()) exceptionMarkers.Add(marker, function);
            }
            if (semanticRuntimeFunctions.Count != 1 || semanticRuntimeFunctions[0].Contract != SemanticRuntimeFunctionImplementation.RecordConstructionOwnerContract)
                throw Limit("Direct construction requires its exact identity-validated exception-owner release contract.", "exception-construction");
            foreach (var function in functions) if (FunctionReachesException(function, graph, [])) exceptionParticipants.Add(function);
            return true;
        }

        private bool TryValidateConstructionLifetimeRoutine(FunctionSymbol function)
        {
            if (!constructionExceptionActive || constructionExceptionContext is not { } plan ||
                !plan.Constructors.Contains(function) && (function.PrimaryEntry is not { } primary || !plan.Constructors.Contains(primary))) return false;
            if (plan.Constructors.Contains(function)) _ = ValidateConstructionSignature(function, plan.Record);
            else
            {
                if (function.ConstructorOrDestructorKind?.Name != "cdk_subobject" || function.Parameters.Count != function.PrimaryEntry!.Parameters.Count ||
                    function.Parameters.Skip(1).Any(parameter => parameter.Role.Name != "abi_parameter"))
                    throw Limit("The construction alternate requires its actual C2 ABI forwarding roles.", "exception-construction");
                ValidateGeneratedAlternateBody(function);
            }
            foreach (var reference in plan.ArgumentReferences) generatedReferenceTypes.Add(reference);
            return true;
        }

        private static void ValidateConstructionHandlerReferenceContext(ConstructionExceptionContext plan, IReadOnlyList<SemanticNode> nodes)
        {
            bool SameReference(CppType type) => MemoryRepresentationType(type).Equals(plan.HandlerReference);
            if (SameReference(plan.CatchingFunction.ReturnType) || plan.CatchingFunction.Parameters.Any(parameter => SameReference(parameter.Type)) ||
                plan.CatchingFunction.Body!.Locals.Any(variable => SameReference(variable.Type) && !variable.Equals(plan.Handler.Parameter)))
                throw Limit("A construction catch reference context authorizes only its actual retained handler local.", "exception-construction");
            var assignment = ((BlockOperation)plan.Handler.Body).Statements[1].Expression!;
            foreach (var expression in nodes.OfType<ExpressionOperation>().Where(expression => SameReference(expression.Type)))
            {
                if (expression.Equals(assignment)) continue;
                if (expression is not ReferenceOperation { Target: VariableSymbol variable } || !variable.Equals(plan.Handler.Parameter))
                    throw Limit("The construction handler has an additional unvalidated reference-typed use.", "exception-construction");
            }
        }

        private bool TryConstructionHandlerReferenceType(PointerType pointer, out string result)
        {
            result = "";
            if (!constructionExceptionActive || constructionExceptionContext is not { } plan ||
                currentFunction?.Equals(plan.CatchingFunction) != true || !pointer.Equals(plan.HandlerReference)) return false;
            _ = GeneratedRecordPointer(pointer, true);
            result = TypeName(pointer.PointeeType) + "*";
            return true;
        }

        private bool IsConstructionDestination(LoweredEhOperation marker, FunctionSymbol function) => constructionExceptionActive &&
            constructionExceptionContext!.Throws.Any(item => item.Function.Equals(function) && item.Destination.Equals(marker));

        private bool IsConstructionTemporaryRecipe(Initialization recipe) => constructionExceptionActive && constructionTemporaryRecipes.Contains(recipe);

        private void ValidateConstructionStatement(ExpressionOperation expression, FunctionSymbol owner)
        {
            if (constructionStatementRoots.TryGetValue(expression, out var prior) && !prior.Equals(owner))
                throw Limit("A construction expression statement has different retained function owners.", "exception-construction");
            constructionStatementRoots[expression] = owner;
            if (expression is ThrowOperation throwing && constructionExceptionContext!.Throws.Any(item => item.Function.Equals(owner) && item.Throw.Equals(throwing))) return;
            if (expression is LifetimeOperation { Lifetime: not null } lifetime) { ValidateConstructionStatement(lifetime.Value, owner); return; }
            if (expression is ConversionOperation { OperatorKind.Name: "eok_cast", Operands.Count: 1 } conversion && MemoryRepresentationType(conversion.Type).Kind.Name == "tk_void")
            { ValidateConstructionStatement(conversion.Operand, owner); return; }
            if (expression is OperatorOperation { OperatorKind.Name: "eok_comma", Operands.Count: 2 } comma &&
                MemoryRepresentationType(comma.Type).Kind.Name == "tk_void" && !comma.Operator.EvalLeftToRight && !comma.Operator.EvalRightToLeft &&
                !comma.Operator.RequiresRuntimeCastCheck && !comma.Operator.RetainedVirtualCall)
            {
                ValidateConstructionStatement(comma.Operands[0], owner);
                foreach (var node in ExceptionExecutionNodes(comma.Operands[1]))
                    if (node is CallOperation call && (call.Callee is not ReferenceOperation { Target: FunctionSymbol target } || target.SpecialFunctionKind?.Name != "sfk_destructor") ||
                        node is ThrowOperation or TryOperation or LifetimeOperation || node is OperatorOperation operation &&
                        operation.OperatorKind.Name is not ("eok_comma" or "eok_call" or "eok_cast" or "eok_address_of"))
                        throw Limit("The construction comma tail has an unsupported retained post-throw cleanup obligation.", "exception-construction");
                return;
            }
            throw Limit("The selected construction throw requires its exact void lifetime/comma statement context.", "exception-construction");
        }

        private bool TryEmitConstructionStatement(ExpressionOperation expression, CodeWriter writer)
        {
            if (!constructionExceptionActive || currentFunction is null || !constructionStatementRoots.TryGetValue(expression, out var owner)) return false;
            if (!owner.Equals(currentFunction)) throw Limit("The construction statement belongs to a different retained function.", "exception-construction");
            EmitSelected(expression);
            return true;
            void EmitSelected(ExpressionOperation value)
            {
                switch (value)
                {
                    case LifetimeOperation wrapper: EmitSelected(wrapper.Value); return;
                    case ConversionOperation conversion: EmitSelected(conversion.Operand); return;
                    case OperatorOperation { OperatorKind.Name: "eok_comma" } comma:
                        // Validation proves the left occurrence unconditionally
                        // throws. Its retained normal-cleanup tail is unreachable;
                        // exceptional actions come from the actual armed table.
                        EmitSelected(comma.Operands[0]); return;
                    case ThrowOperation throwing:
                        var plan = constructionExceptionContext!.Throws.Single(item => item.Throw.Equals(throwing));
                        var pending = ExceptionLocal("__construction_owner");
                        var committed = ExceptionLocal("__construction_committed");
                        writer.Line($"{ExceptionCarrierType} {pending} = {ExceptionCarrierType}.CreateRecordForConstruction({Owner}.{exceptionTypeToken}, 16, 8, {Owner}.{constructionDestructorThunk});");
                        writer.Line($"bool {committed} = false;");
                        writer.Line("try"); writer.Open();
                        var previous = pendingThrowOwner; pendingThrowOwner = pending;
                        try { EmitPlannedExpressionStatement(plan.Throw.Operand!, writer); }
                        finally { pendingThrowOwner = previous; }
                        writer.Line($"{pending}.CommitThrow();"); writer.Line($"{committed} = true;"); writer.Line($"throw {pending};"); writer.Close();
                        writer.Line("finally"); writer.Open(); writer.Line($"if (!{committed})"); writer.Open();
                        var frame = exceptionFunctions[plan.Function];
                        writer.Line($"if ({frame.Observed} is not null)"); writer.Open();
                        writer.Line($"{frame.Observed}.RequireSelectedHandler();"); EmitExceptionCleanup(frame, "65535", writer); writer.Close();
                        writer.Line($"{pending}.ReleaseUncommitted();"); writer.Close(); writer.Close();
                        return;
                    default: throw Limit("The validated construction statement lost its typed throw context.", "exception-construction");
                }
            }
        }

        private void EmitConstructionExceptionFields(StringBuilder fields)
        {
            if (!constructionExceptionActive || constructionExceptionContext is not { } plan) return;
            fields.Append("    private static void ").Append(constructionDestructorThunk).AppendLine("(nint address)");
            fields.AppendLine("    {");
            fields.Append("        ").Append(OwnedName(plan.Destructor)).Append("((").Append(TypeName(plan.Destructor.Parameters[0].Type)).AppendLine(")address);");
            fields.AppendLine("    }");
        }
    }
}
