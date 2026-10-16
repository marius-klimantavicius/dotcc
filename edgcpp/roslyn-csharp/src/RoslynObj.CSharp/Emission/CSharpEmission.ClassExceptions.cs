using System.Text;
using RoslynObj;

namespace RoslynObj.CSharp;

public sealed partial class CSharpEmitter
{
    private sealed partial class Emission
    {
        private sealed record ClassExceptionContext(RecordType Record, CppType ThrownType, PointerType ReferenceType,
            VariableSymbol Typeinfo, FunctionSymbol ThrowingFunction, FunctionSymbol CatchingFunction,
            ThrowOperation Throw, TryOperation Try, ExceptionHandler Handler, CallOperation CopyCall,
            FunctionSymbol Copy, FunctionSymbol Destructor, LoweredEhOperation Started, LoweredEhOperation Destination);

        private ClassExceptionContext? classExceptionContext;
        private bool classExceptionActive;
        private string? classExceptionDestructorThunk;

        // This preflight supplies only the exact retained typeinfo root to the
        // address-only data validator. Full executable EH admission is separate.
        private bool TryClassPayloadTypeinfoContext(out RecordType record, out VariableSymbol typeinfo)
        {
            if (classExceptionContext is { } cached)
            {
                record = cached.Record; typeinfo = cached.Typeinfo;
                return true;
            }
            record = null!; typeinfo = null!;
            var graph = functions.ToDictionary(function => function, function => ExceptionExecutionNodes(function.Body!.Root).ToArray());
            var throws = graph.Values.SelectMany(nodes => nodes.OfType<ThrowOperation>()).ToArray();
            if (!throws.Any(throwing => throwing.ThrownType is { } type && MemoryRepresentationType(type) is RecordType)) return false;
            RequireNormalLifetimeProfile();
            var tries = graph.Values.SelectMany(nodes => nodes.OfType<TryOperation>()).ToArray();
            if (throws.Length != 1 || tries.Length != 1 || throws[0] is not { IsRethrow: false, ThrownType: QualifiedType { IsConst: true, IsVolatile: false } thrownType } throwing ||
                thrownType.UnderlyingType is not RecordType payload || throwing.Initialization is not null || throwing.Destructor is not { } destructor ||
                throwing.AccessibleBases.Count != 0 || throwing.Operands.Count != 0 || throwing.RepresentationRole.Name != "execution" ||
                throwing.ValueCategory.Name != "prvalue" || MemoryRepresentationType(throwing.Type).Kind.Name != "tk_void")
                throw Limit("The bounded class throw requires one lowered const-record throw and one reference handler with its selected payload destructor.", "exception-record");
            ValidateClassPayloadRecord(payload);
            ValidateAggregateQualifiers(thrownType);
            if (thrownType.Size != payload.Size || thrownType.Alignment != payload.Alignment ||
                throwing.Operand is not OperatorOperation { OperatorKind.Name: "eok_comma" } comma || comma.Operands.Count != 2 ||
                comma.RepresentationRole.Name != "execution" || comma.ValueCategory.Name != "prvalue" || !comma.Type.Equals(throwing.Type) ||
                comma.Operator.EvalLeftToRight || comma.Operator.EvalRightToLeft ||
                comma.Operands[0] is not LoweredEhOperation { ExceptionKind.Name: "leck_exception_started" } started ||
                comma.Operands[1] is not CallOperation copyCall || copyCall.Callee is not ReferenceOperation { Target: FunctionSymbol copy } ||
                copyCall.TargetMethod?.Equals(copy) != true || copyCall.CallKind.Name != "direct" ||
                !copyCall.Operator.EvalLeftToRight || copyCall.Operator.EvalRightToLeft || copyCall.Operator.RetainedVirtualCall ||
                copyCall.SourceDispatchKind is not null || copyCall.SourceTargetMethod is not null || copyCall.Call.ReturnSlot is not null ||
                copyCall.Arguments.Count != 2 || copyCall.Operands.Count != 3 || !copyCall.Operands[0].Equals(copyCall.Callee) ||
                !copyCall.Operands[1].Equals(copyCall.Arguments[0].Value) || !copyCall.Operands[2].Equals(copyCall.Arguments[1].Value) ||
                copyCall.Receiver?.Equals(copyCall.Arguments[0].Value) != true ||
                copyCall.Arguments[0].Value is not ConversionOperation { OperatorKind.Name: "eok_cast" } destinationCast ||
                destinationCast.Operand is not LoweredEhOperation { ExceptionKind.Name: "leck_thrown_object_address" } destination ||
                destinationCast.Operands.Count != 1 || destinationCast.Operator.RequiresRuntimeCastCheck ||
                copyCall.Arguments[1].Value is not ReferenceOperation { Target: VariableSymbol source } sourceValue ||
                sourceValue.ValueCategory.Name != "prvalue" || sourceValue.Operands.Count != 0)
                throw Limit("The class throw operand must retain exactly exception-start followed by its selected copy into the thrown-object address.", "exception-record");
            ValidateClassLifetimeSignature(copy, payload, true);
            ValidateClassLifetimeSignature(destructor, payload, false);
            if (!destinationCast.Type.Equals(copy.Parameters[0].Type) || destinationCast.ValueCategory.Name != "prvalue" ||
                copyCall.Call.FunctionType?.Equals(copy.Signature) != true || !copyCall.Type.Equals(copy.ReturnType) || copyCall.ValueCategory.Name != "prvalue" ||
                MemoryRepresentationType(sourceValue.Type) is not PointerType { IsReference: false, IsRValueReference: false } sourcePointer ||
                !sourcePointer.PointeeType.Equals(thrownType) || sourceValue.Type.Size != 8 || sourceValue.Type.Alignment != 8 ||
                sourcePointer.Size != 8 || sourcePointer.Alignment != 8)
                throw Limit("The selected class copy requires its exact ordinary destination and const source pointer views.", "exception-record");
            for (var index = 0; index < 2; ++index)
            {
                var actual = copyCall.Arguments[index]; var formal = copy.Signature.Parameters[index];
                if (actual.Ordinal != index || actual.Parameter?.Equals(formal.Descriptor) != true || actual.Role?.Equals(formal.Role) != true)
                    throw Limit("The class copy receiver/source already belong to the retained argument list in selected order.", "exception-record");
            }
            var selectedTry = tries[0];
            if (selectedTry.Definition.IsFunctionTry || selectedTry.Handlers.Count != 1 ||
                selectedTry.Handlers[0] is not { CaughtType: PointerType { IsReference: true, IsRValueReference: false } reference, Parameter: { } parameter } handler ||
                reference.Size != 8 || reference.Alignment != 8 || !reference.PointeeType.Equals(thrownType) ||
                !parameter.Type.Equals(reference) || !copy.Parameters[1].Type.Equals(reference) ||
                payload.Details.TypeinfoVariable is not { } retainedTypeinfo || handler.Details.Typeinfo?.Equals(retainedTypeinfo) != true)
                throw Limit("The class handler must bind its retained const reference to the identical thrown type and common typeinfo root.", "exception-record");
            ValidateAggregateQualifiers(reference);
            var throwingOwners = graph.Where(pair => pair.Value.Contains(throwing)).Select(pair => pair.Key).ToArray();
            var catchingOwners = graph.Where(pair => pair.Value.Contains(selectedTry)).Select(pair => pair.Key).ToArray();
            if (throwingOwners.Length != 1 || catchingOwners.Length != 1 || throwingOwners[0].Equals(catchingOwners[0]))
                throw Limit("The class payload requires distinct retained throwing and catching functions.", "exception-record");
            var throwingFunction = throwingOwners[0];
            if (throwingFunction.Parameters.Count != 1 || throwingFunction.Parameters[0].Role.Name != "ordinary_parameter" ||
                !throwingFunction.Parameters[0].Type.Equals(reference) || throwingFunction.Parameters[0].Symbol?.Equals(source) != true ||
                !source.Type.Equals(reference) || MemoryRepresentationType(throwingFunction.ReturnType).Kind.Name != "tk_void")
                throw Limit("The selected class throw source is its one captured const-reference parameter.", "exception-record");
            ValidateClassPendingMarker(started, throwingFunction, false);
            ValidateClassPendingMarker(destination, throwingFunction, true);
            classExceptionContext = new(payload, thrownType, reference, retainedTypeinfo, throwingFunction, catchingOwners[0],
                throwing, selectedTry, handler, copyCall, copy, destructor, started, destination);
            record = payload; typeinfo = retainedTypeinfo;
            return true;
        }

        private static void ValidateClassPayloadRecord(RecordType record)
        {
            ValidateAggregateQualifiers(record);
            if (record.Kind.Name is not ("tk_struct" or "tk_class") || record.IsIncomplete || record.IsPacked || record.Bases.Count != 0 ||
                record.BaseSubobjects.Count != 0 || record.HasVirtualBases || record.HasVirtualFunctions || record.HasPureVirtualFunctions ||
                record.VirtualFunctionTableVariable is not null || record.VirtualTableTableVariable is not null ||
                record.Size != 16 || record.Alignment != 8 || record.Layout is not { IsComplete: true } layout ||
                layout.Size != 16 || layout.Alignment != 8 || layout.NonvirtualSize != 12 || layout.NonvirtualAlignment != 8 ||
                record.Fields.Count != 2 || layout.Fields.Count != 2)
                throw Limit("The class-payload owner supports the captured complete 16-byte/8-aligned nonpolymorphic record only.", "exception-record-layout");
            for (var index = 0; index < 2; ++index)
            {
                var field = layout.Fields[index];
                ValidateAggregateQualifiers(field.Field.Type);
                if (field.Offset != (index == 0 ? 0 : 8) || field.BitWidth is not null || field.Field.IsBitField || field.Field.IsGeneratedBase ||
                    field.Field.IsPacked == true || field.Field.ContainingType?.Equals(record) != true || !record.Fields.Contains(field.Field) ||
                    MemoryRepresentationType(field.Field.Type) is not ArithmeticType arithmetic || arithmetic.Arithmetic.Signed != true ||
                    arithmetic.Arithmetic.Bits != (index == 0 ? 64 : 32) || field.Field.Type.Size != (index == 0 ? 8 : 4) ||
                    field.Field.Type.Alignment != (index == 0 ? 8 : 4) || arithmetic.Size != field.Field.Type.Size || arithmetic.Alignment != field.Field.Type.Alignment)
                    throw Limit("The class payload requires its captured natural signed64/signed32 fields at offsets zero/eight.", "exception-record-layout");
            }
        }

        private static void ValidateClassPendingMarker(LoweredEhOperation marker, FunctionSymbol function, bool address)
        {
            var detail = marker.ExceptionOperation;
            if (marker.RepresentationRole.Name != "execution" || marker.ValueCategory.Name != "prvalue" || marker.Operands.Count != 0 ||
                marker.Details.VolatileFetch || detail.FunctionEh is { } owner && !owner.Routine.Equals(function) ||
                detail.Initialization is not null || detail.Region is not null || detail.StateNumber is not null || detail.Routine is not null ||
                detail.Handler is not null || detail.TryBlock is not null || detail.TryExpression is not null || detail.CatchExpression is not null ||
                (address ? MemoryRepresentationType(marker.Type) is not PointerType { IsReference: false, IsRValueReference: false } pointer ||
                    pointer.Size != 8 || pointer.Alignment != 8 || MemoryRepresentationType(pointer.PointeeType).Kind.Name != "tk_void"
                    : MemoryRepresentationType(marker.Type).Kind.Name != "tk_void"))
                throw Limit("A class pending-owner marker has an unsupported retained type or execution association.", "exception-record");
        }

        private void ValidateClassLifetimeSignature(FunctionSymbol function, RecordType record, bool copy)
        {
            var signature = function.Signature;
            var count = copy ? 2 : 1;
            if (!functions.Contains(function) || function.Body is null || function.PrimaryEntry is not null || function.IsVirtual || function.IsPureVirtual ||
                function.Details.IsWeak == true || function.Details.IsWeakref == true || function.Details.IsGnuAlias == true || function.Details.IsIfunc == true ||
                function.Details.IsAliasEntry == true || function.GnuSupplement?.AliasedRoutine is not null ||
                function.ThisAdjustment != 0 || function.ReturnAdjustment != 0 || function.VirtualCallIndex != 0 || function.VirtualBaseIndex != 0 ||
                function.SpecialFunctionKind?.Name != (copy ? "sfk_constructor" : "sfk_destructor") || function.ConstructorOrDestructorKind?.Name != "cdk_complete" ||
                !signature.HasPrototype || signature.IsVariadic || !signature.HasThisParameter || signature.ReturnsValueAsParameter == true ||
                signature.ReturnsValueViaCopyConstructor || signature.ReturnValueParameterFollowsThis == true ||
                signature.ExceptionSpecification?.NoexceptEffective != true || signature.LanguageLinkage?.Name != "nlk_cplusplus_external" ||
                signature.ThisClass?.Equals(record) != true || function.ContainingType?.Equals(record) != true ||
                signature.Parameters.Count != count || function.Parameters.Count != count ||
                MemoryRepresentationType(function.ReturnType).Kind.Name != "tk_void" || !GeneratedRecordPointer(function.Parameters[0].Type, false).Equals(record))
                throw Limit("The payload copy/destructor requires its exact selected complete noexcept generated receiver signature.", "exception-record");
            ValidateCallingConvention(signature);
            for (var index = 0; index < count; ++index)
            {
                var parameter = function.Parameters[index]; var selected = signature.Parameters[index];
                if (parameter.Ordinal != index || selected.Ordinal != index || parameter.Symbol is null || parameter.BaseSubobject is not null ||
                    parameter.Descriptor.Details.PassedViaCopyConstructor || !parameter.Descriptor.Equals(selected.Descriptor) ||
                    !parameter.Type.Equals(selected.Type) || parameter.Role.Name != (index == 0 ? "this_parameter" : "ordinary_parameter") ||
                    !parameter.Role.Equals(selected.Role) || selected.BaseSubobject is not null)
                    throw Limit("The payload lifetime signature has an unexercised hidden execution role.", "exception-record");
            }
            if (copy && !GeneratedRecordPointer(function.Parameters[1].Type, true).Equals(record))
                throw Limit("The selected payload copy requires its exact retained const-reference source record.", "exception-record");
        }

        private bool TryInitializeClassExceptions(IReadOnlyDictionary<FunctionSymbol, SemanticNode[]> graph,
            IReadOnlyList<FunctionSymbol> candidates, IReadOnlyList<ThrowOperation> throws, IReadOnlyList<TryOperation> tries)
        {
            if (!TryClassPayloadTypeinfoContext(out _, out _)) return false;
            var plan = classExceptionContext!;
            if (HostProtocolEnabled || boundedRethrow || noexceptTermination is not null || candidates.Count != 2 || throws.Count != 1 || tries.Count != 1 ||
                !throws[0].Equals(plan.Throw) || !tries[0].Equals(plan.Try) || !candidates.Contains(plan.ThrowingFunction) || !candidates.Contains(plan.CatchingFunction) ||
                functions.Any(function => function.Body!.ExceptionHandling.Arrays.Count != 0 || function.Body!.ExceptionHandling.ArrayTable is not null) ||
                !IsStandaloneThrow(plan.ThrowingFunction.Body!.Root, plan.Throw) ||
                graph.Values.SelectMany(nodes => nodes).Any(node => node is AllocationOperation) ||
                graph.Values.SelectMany(nodes => nodes.OfType<LoweredEhOperation>()).Count(marker => marker.ExceptionKind.Name == "leck_exception_started") != 1 ||
                ExceptionExecutionNodes(plan.Handler.Body).Any(node => node is ThrowOperation or TryOperation))
                throw Limit("The class payload slice requires one generated throw/reference handler and their two simple cleanup functions, without rethrow, termination, allocation or arrays.", "exception-record");
            foreach (var nodes in graph.Values)
            foreach (var call in nodes.OfType<CallOperation>())
                if (call.Callee is not ReferenceOperation { Target: FunctionSymbol target } || !functions.Contains(target) ||
                    call.TargetMethod?.Equals(target) != true || target.Signature.IsVariadic)
                    throw Limit("The class payload slice admits only selected fixed direct generated calls; native exception boundaries are unsupported.", "exception-record");
            foreach (var function in functions.Where(function => function.Signature.ExceptionSpecification?.NoexceptEffective == true))
                EnsureGeneratedNoThrow(function);
            EnsureGeneratedNoThrow(plan.Copy); EnsureGeneratedNoThrow(plan.Destructor);
            foreach (var call in ExceptionExecutionNodes(plan.Handler.Body).OfType<CallOperation>())
                EnsureGeneratedNoThrow((FunctionSymbol)((ReferenceOperation)call.Callee).Target!);
            if (plan.Handler.ParameterInitialization is not { } recipe || recipe.Target?.Equals(plan.Handler.Parameter) != true ||
                recipe.Destructor is not null || recipe.Lifetime is not null || recipe.NextInDestructionList is not null ||
                recipe.Details.Type?.Equals(plan.ReferenceType) != true)
                throw Limit("The reference handler parameter must have its exact metadata-only address-binding recipe, without value ownership.", "exception-record");
            ValidateExceptionMetadataRecipe(recipe, false);
            ValidateClassHandlerEntry(plan);
            classExceptionActive = true;
            exceptionIntType = plan.ThrownType; // Shared token plumbing; class allocation remains a separate exact contract.
            selectedIntegerThrow = plan.Throw;
            selectedIntegerThrowFunction = plan.ThrowingFunction;
            exceptionTypeToken = InternalName("__cpp_record_type");
            classExceptionDestructorThunk = InternalName("__DestroyCppRecord");
            generatedReferenceTypes.Add(plan.ReferenceType);
            var tryPlan = new ExceptionTry(plan.CatchingFunction, plan.Try, plan.Handler, InternalName("__cpp_handler"),
                ExceptionLocal("__try_entry_state"), ExceptionLocal("__caught_cpp"));
            exceptionTries.Add(plan.Try, tryPlan);
            foreach (var function in candidates)
            {
                ValidateExceptionFunction(function, graph[function], tryPlan);
                var eh = function.Body!.ExceptionHandling;
                exceptionFunctions.Add(function, new(function, eh.RegionTable!, eh.CleanupRegions,
                    ExceptionLocal("__cleanup_state"), ExceptionLocal("__unwinding_cpp"), ExceptionLocal("__observed_cpp"), ExceptionLocal("__active_caught_cpp")));
                foreach (var marker in graph[function].OfType<LoweredEhOperation>()) exceptionMarkers.Add(marker, function);
            }
            if (semanticRuntimeFunctions.Count != 1 || semanticRuntimeFunctions[0].Contract != SemanticRuntimeFunctionImplementation.RecordExceptionOwnerContract)
                throw Limit("The class payload requires its one identity-validated generated record-owner release implementation.", "exception-record");
            foreach (var function in functions)
                if (FunctionReachesException(function, graph, [])) exceptionParticipants.Add(function);
            return true;
        }

        private static void ValidateClassHandlerEntry(ClassExceptionContext plan)
        {
            if (plan.Handler.Body is not BlockOperation body || body.Statements.Count < 3 ||
                body.Statements[0].Expression is not CleanupStateOperation { IsUnreachable: false } ||
                body.Statements[1].Expression is not OperatorOperation { OperatorKind.Name: "eok_assign" } assignment ||
                !assignment.Operator.EvalRightToLeft || assignment.Operands.Count != 2 ||
                assignment.Operands[0] is not ReferenceOperation { Target: VariableSymbol parameter } left || !parameter.Equals(plan.Handler.Parameter) ||
                !left.Type.Equals(plan.ReferenceType) || left.ValueCategory.Name != "lvalue" ||
                assignment.Operands[1] is not ConversionOperation { OperatorKind.Name: "eok_cast" } conversion || conversion.Operands.Count != 1 ||
                conversion.Operator.RequiresRuntimeCastCheck || conversion.Operand is not LoweredEhOperation { ExceptionKind.Name: "leck_caught_object_address" } caught ||
                caught.ExceptionOperation.Handler?.Equals(plan.Handler) != true || conversion.Type is not PointerType { IsReference: false, IsRValueReference: false } pointer ||
                !pointer.PointeeType.Equals(plan.ThrownType) || pointer.Size != 8 || pointer.Alignment != 8 ||
                body.Statements[2].Expression is not LoweredEhOperation { ExceptionKind.Name: "leck_exception_caught" } ||
                ExceptionExecutionNodes(body).OfType<LoweredEhOperation>().Count(marker => marker.ExceptionKind.Name == "leck_caught_object_address") != 1 ||
                ExceptionExecutionNodes(body).OfType<LoweredEhOperation>().Count(marker => marker.ExceptionKind.Name == "leck_exception_caught") != 1)
                throw Limit("The class handler must retain its one reference-address assignment before its actual exception-caught marker.", "exception-record");
        }

        private bool IsClassExceptionReferenceContext(FunctionSymbol function) => classExceptionActive && classExceptionContext is { } plan &&
            (function.Equals(plan.ThrowingFunction) || function.Equals(plan.CatchingFunction));

        private bool TryValidateClassExceptionReferenceRoutine(FunctionSymbol function)
        {
            if (!classExceptionActive || classExceptionContext is not { } plan || !function.Equals(plan.ThrowingFunction)) return false;
            var signature = function.Signature;
            if (signature.HasThisParameter || signature.ReturnsValueAsParameter == true || signature.ReturnsValueViaCopyConstructor ||
                signature.ExceptionSpecification?.NoexceptEffective == true || function.Parameters.Count != 1 ||
                function.Parameters[0].Role.Name != "ordinary_parameter" || !function.Parameters[0].Type.Equals(plan.ReferenceType) ||
                MemoryRepresentationType(function.ReturnType).Kind.Name != "tk_void")
                throw Limit("The selected class throwing helper has an unsupported execution reference signature.", "exception-record");
            generatedReferenceTypes.Add(plan.ReferenceType);
            return true;
        }

        private bool IsClassExceptionStarted(LoweredEhOperation marker, FunctionSymbol function) => classExceptionActive &&
            classExceptionContext is { } plan && plan.Started.Equals(marker) && plan.ThrowingFunction.Equals(function);

        private void EmitClassExceptionFields(StringBuilder fields)
        {
            if (!classExceptionActive || classExceptionContext is not { } plan) return;
            fields.Append("    private static void ").Append(classExceptionDestructorThunk).AppendLine("(nint address)");
            fields.AppendLine("    {");
            fields.Append("        ").Append(OwnedName(plan.Destructor)).Append("((").Append(TypeName(plan.Destructor.Parameters[0].Type)).AppendLine(")address);");
            fields.AppendLine("    }");
        }
    }
}
