using RoslynObj;

namespace RoslynObj.CSharp;

public sealed partial class CSharpEmitter
{
    private sealed partial class Emission
    {
        private sealed record BorrowedFieldMethod(FunctionSymbol Function, PointerType Reference,
            PointerType ReceiverType, QualifiedType ConstField, FieldSymbol Field);
        private sealed record BorrowedFieldBinding(FunctionSymbol Owner, VariableSymbol Variable,
            Initialization Initialization, CallOperation Call, BorrowedFieldMethod Method);
        private readonly Dictionary<FunctionSymbol, BorrowedFieldMethod> borrowedFieldMethods = [];
        private readonly Dictionary<CallOperation, BorrowedFieldBinding> borrowedFieldCalls = [];
        private readonly Dictionary<VariableSymbol, BorrowedFieldBinding> borrowedFieldBindings = [];

        private void InitializeBorrowedFieldReferences()
        {
            var candidates = functions.Where(function => function.Signature.HasThisParameter &&
                function.SpecialFunctionKind?.Name == "sfk_none" && !IsVirtualMethod(function) &&
                MemoryRepresentationType(function.ReturnType) is PointerType { IsReference: true }).ToArray();
            if (candidates.Length == 0) return;
            RequireNormalLifetimeProfile();
            foreach (var function in candidates) borrowedFieldMethods.Add(function, ValidateBorrowedFieldMethod(function));
            foreach (var owner in functions)
            {
                var nodes = ExceptionExecutionNodes(owner.Body!.Root).ToArray();
                foreach (var call in nodes.OfType<CallOperation>().Distinct())
                {
                    var direct = call.Callee is ReferenceOperation { Target: FunctionSymbol target } ? CanonicalFunction(target) : null;
                    var retained = call.TargetMethod is { } selected ? CanonicalFunction(selected) : null;
                    if (direct is null || !borrowedFieldMethods.TryGetValue(direct, out var method))
                    {
                        if (retained is not null && borrowedFieldMethods.ContainsKey(retained))
                            throw Limit("A borrowed field reference requires its actual selected direct callee.", "borrowed-reference");
                        continue;
                    }
                    ValidateBorrowedFieldCall(call, method);
                    var statements = nodes.OfType<InitializationStatementOperation>().Distinct().Where(statement =>
                        statement.Initialization.Expression?.Equals(call) == true).ToArray();
                    if (statements.Length != 1)
                        throw Limit("The borrowed reference call requires one actual executable local binding association.", "borrowed-reference");
                    var binding = ValidateBorrowedFieldBinding(owner, statements[0], call, method);
                    if (borrowedFieldCalls.TryGetValue(call, out var prior) && !prior.Equals(binding) ||
                        borrowedFieldBindings.TryGetValue(binding.Variable, out var priorVariable) && !priorVariable.Equals(binding))
                        throw Limit("A borrowed call or local has different retained binding owners.", "borrowed-reference");
                    borrowedFieldCalls[call] = binding;
                    borrowedFieldBindings[binding.Variable] = binding;
                }
                foreach (var reference in nodes.OfType<ReferenceOperation>().Where(reference => reference.Target is FunctionSymbol function &&
                    borrowedFieldMethods.ContainsKey(CanonicalFunction(function))))
                    if (!borrowedFieldCalls.Values.Any(binding => binding.Owner.Equals(owner) && binding.Call.Callee.Equals(reference)))
                        throw Limit("Borrowed reference methods have no address-exposed or other unregistered reference adapter.", "borrowed-reference");
            }
            foreach (var owner in borrowedFieldBindings.Values.Select(binding => binding.Owner).Distinct())
                ValidateBorrowedReferenceContext(owner);
        }

        private BorrowedFieldMethod ValidateBorrowedFieldMethod(FunctionSymbol function)
        {
            var body = function.Body ?? throw Limit("The borrowed method requires its selected generated body.", "borrowed-reference");
            var signature = function.Signature;
            if (!functions.Contains(function) || function.IsVirtual || function.IsPureVirtual ||
                function.SpecialFunctionKind?.Name != "sfk_none" || function.ConstructorOrDestructorKind?.Name != "cdk_none" ||
                function.PrimaryEntry is not null || function.AlternateEntryPoints.Count != 0 || function.BuiltinKind is not null ||
                function.OverridingFunctionForWrapper is not null || function.OverriddenFunctionForWrapper is not null ||
                function.ThisAdjustment != 0 || function.ReturnAdjustment != 0 || function.VirtualCallIndex != 0 || function.VirtualBaseIndex != 0 ||
                function.Details.IsWeak == true || function.Details.IsWeakref == true || function.Details.IsGnuAlias == true ||
                function.Details.IsIfunc == true || function.Details.IsAliasEntry == true || function.GnuSupplement?.AliasedRoutine is not null ||
                function.GnuSupplement?.TargetedVersions.Count > 0 || function.GnuSupplement?.Representative is not null ||
                function.GnuSupplement?.IsRepresentative == true || function.GnuSupplement?.IsTargetSpecificVersion == true || function.GnuSupplement?.ResolverRequired == true ||
                !signature.HasPrototype || signature.IsVariadic || !signature.HasThisParameter || signature.LanguageLinkage?.Name != "nlk_cplusplus_external" ||
                signature.ReturnsValueAsParameter == true || signature.ReturnsValueViaCopyConstructor || signature.ReturnValueParameterFollowsThis == true ||
                signature.Parameters.Count != 1 || function.Parameters.Count != 1 || function.ContainingType is not RecordType owner ||
                signature.ThisClass?.Equals(owner) != true || function.ReturnType is not PointerType { IsReference: true, IsRValueReference: false } result ||
                result.PointeeType is not QualifiedType { IsConst: true, IsVolatile: false } constant)
                throw Limit("The borrowed method supports a fixed ordinary const receiver returning a const record-field lvalue reference.", "borrowed-reference");
            ValidateCallingConvention(signature); ValidateNaturalBorrowedPointer(result);
            ValidateBorrowedRecord(owner); var member = ValidateBorrowedRecord(constant);
            var parameter = function.Parameters[0]; var selected = signature.Parameters[0];
            if (parameter.Ordinal != 0 || selected.Ordinal != 0 || parameter.Role.Name != "this_parameter" ||
                !parameter.Role.Equals(selected.Role) || !parameter.Descriptor.Equals(selected.Descriptor) || !parameter.Type.Equals(selected.Type) ||
                parameter.BaseSubobject is not null || selected.BaseSubobject is not null || parameter.Descriptor.Details.PassedViaCopyConstructor ||
                parameter.Symbol is not { } receiver || parameter.Type is not PointerType { IsReference: false, IsRValueReference: false } receiverType ||
                receiverType.PointeeType is not QualifiedType { IsConst: true, IsVolatile: false } constOwner || !constOwner.UnderlyingType.Equals(owner))
                throw Limit("The borrowed method receiver must retain its exact selected const containing-record pointer.", "borrowed-reference");
            ValidateNaturalBorrowedPointer(receiverType); ValidateBorrowedRecord(constOwner); ValidateAggregateQualifiers(receiver.Type);
            if (!receiver.Type.Equals(receiverType) && (receiver.Type is not QualifiedType { IsConst: true, IsVolatile: false } pointerObject ||
                !pointerObject.UnderlyingType.Equals(receiverType) || pointerObject.Size != receiverType.Size || pointerObject.Alignment != receiverType.Alignment))
                throw Limit("The borrowed execution receiver permits only its retained const pointer-object wrapper.", "borrowed-reference");
            var eh = body.ExceptionHandling;
            if (body.Locals.Count != 0 || body.Lifetimes.Count != 0 || eh.RegionTable is not null || eh.ArrayTable is not null ||
                eh.CleanupRegions.Count != 0 || eh.Arrays.Count != 0 || eh.Details?.Prologue is not null || eh.Epilogues.Count != 0 ||
                eh.Details?.InitialState is not null || eh.Details?.NullState is not null ||
                body.Root is not BlockOperation { Lifetime: null, Statements.Count: 1 } block ||
                block.Statements[0] is not JumpOperation { JumpKind.Name: "return", Target: null, CommonLifetime: null } returned ||
                returned.Value is not OperatorOperation { OperatorKind.Name: "eok_address_of", Operands.Count: 1 } address ||
                address.ValueCategory.Name != "prvalue" || address.Type is not PointerType { IsReference: false, IsRValueReference: false } valuePointer ||
                !valuePointer.PointeeType.Equals(constant))
                throw Limit("The borrowed method body supports one explicit field-address return without calls, locals, lifetime or EH execution.", "borrowed-reference");
            ValidateNaturalBorrowedPointer(valuePointer);
            var place = address.Operands[0];
            if (place is ConversionOperation { OperatorKind.Name: "eok_lvalue_adjust" } qualification)
            {
                if (qualification.Operands.Count != 1 || qualification.ValueCategory.Name != "lvalue" || !qualification.Type.Equals(constant) ||
                    qualification.Operator.RequiresRuntimeCastCheck || qualification.Details.VolatileFetch)
                    throw Limit("The borrowed field adjustment must be the actual const lvalue qualification occurrence.", "borrowed-reference");
                place = qualification.Operand;
            }
            if (place is not OperatorOperation { OperatorKind.Name: "eok_points_to_field", Operands.Count: 2 } fieldPlace ||
                fieldPlace.ValueCategory.Name != "lvalue" ||
                fieldPlace.Operands[0] is not ReferenceOperation { Target: VariableSymbol actualReceiver, Operands.Count: 0 } receiverValue ||
                !actualReceiver.Equals(receiver) || !receiverValue.Type.Equals(receiverType) || receiverValue.ValueCategory.Name != "prvalue" ||
                fieldPlace.Operands[1] is not ReferenceOperation { Target: FieldSymbol field, Operands.Count: 0 } fieldValue ||
                field.ContainingType?.Equals(owner) != true || !owner.Fields.Contains(field) || !fieldValue.Type.Equals(field.Type) ||
                !constant.UnderlyingType.Equals(field.Type) || field.IsBitField || field.IsGeneratedBase || field.IsGeneratedVirtualPointer == true || field.IsPacked == true ||
                !(address.Operands[0].Equals(fieldPlace) ? fieldPlace.Type.Equals(constant) : fieldPlace.Type.Equals(field.Type)) ||
                owner.Layout!.Fields.Count(entry => entry.Field.Equals(field) && entry.BitWidth is null && entry.Offset is { } offset &&
                    offset >= 0 && offset % member.Alignment == 0 && offset + member.Size <= owner.Size) != 1)
                throw Limit("The borrowed address must retain its exact receiver, field alias, const view and in-bounds ordinary field layout.", "borrowed-reference");
            if (!ValidateBorrowedRecord(field.Type).Equals(member))
                throw Limit("The returned field and const reference have different retained record representations.", "borrowed-reference");
            foreach (var node in new SemanticNode[] { block, returned, address, address.Operands[0], fieldPlace, receiverValue, fieldValue })
                if (node.RepresentationRole.Name != "execution" || node is ExpressionOperation { Details.VolatileFetch: true } ||
                    node is OperatorOperation { Operator.RequiresRuntimeCastCheck: true } or OperatorOperation { Operator.RetainedVirtualCall: true })
                    throw Limit("The borrowed method requires ordinary retained execution occurrences without runtime object adjustment.", "borrowed-reference");
            // This body proof grants no noexcept fact, cleanup-marker omission or
            // ownership/destruction behavior for the borrowed member.
            return new(function, result, receiverType, constant, field);
        }

        private RecordType ValidateBorrowedRecord(CppType type)
        {
            ValidateAggregateQualifiers(type);
            if (MemoryRepresentationType(type) is not RecordType record)
                throw Limit("The borrowed field requires a captured ordinary record representation.", "borrowed-reference");
            if (!pointerMethodsActive || !ordinaryFieldCarriers.Contains(record)) ValidateOrdinaryConstRecord(record);
            for (var current = type; ;)
            {
                if (current.Size != record.Size || current.Alignment != record.Alignment || current.IsIncomplete || current.IsDependent)
                    throw Limit("A borrowed record wrapper changes its effective captured size/alignment or completeness.", "borrowed-reference");
                if (current is not TypeReference reference) break;
                current = reference is QualifiedType qualified ? qualified.UnderlyingType : reference.UnderlyingType;
            }
            return record;
        }

        private static void ValidateNaturalBorrowedPointer(PointerType pointer)
        {
            ValidateAggregateQualifiers(pointer); ValidateAggregateQualifiers(pointer.PointeeType);
            if (pointer.Size != 8 || pointer.Alignment != 8 || pointer.IsRValueReference)
                throw Limit("The borrowed reference/value view requires its selected natural eight-byte pointer representation.", "borrowed-reference");
        }

        private static bool IsBorrowedResultView(CppType actual, BorrowedFieldMethod method)
        {
            ValidateAggregateQualifiers(actual);
            if (actual is not PointerType { IsReference: false, IsRValueReference: false } pointer || !pointer.PointeeType.Equals(method.ConstField)) return false;
            ValidateNaturalBorrowedPointer(pointer);
            return true;
        }

        private static void ValidateBorrowedFieldCall(CallOperation call, BorrowedFieldMethod method)
        {
            var function = method.Function; var signature = function.Signature;
            if (call.Callee is not ReferenceOperation { Target: FunctionSymbol target, Operands.Count: 0 } || !target.Equals(function) ||
                call.Callee.RepresentationRole.Name != "execution" ||
                call.TargetMethod?.Equals(function) != true || call.CallKind.Name != "direct" || call.SourceTargetMethod is not null || call.SourceDispatchKind is not null ||
                call.Operator.RetainedVirtualCall || call.Operator.RequiresRuntimeCastCheck || !call.Operator.EvalLeftToRight || call.Operator.EvalRightToLeft ||
                call.RepresentationRole.Name != "execution" || call.ValueCategory.Name != "prvalue" || call.Call.ReturnSlot is not null ||
                call.Call.FunctionType?.Equals(signature) != true || !IsBorrowedResultView(call.Type, method) ||
                call.Receiver is not { } receiver || receiver.RepresentationRole.Name != "execution" || !receiver.Type.Equals(method.ReceiverType) ||
                call.Arguments.Count != 1 || call.Operands.Count != 2 || !call.Operands[0].Equals(call.Callee) || !call.Operands[1].Equals(receiver) ||
                call.Arguments[0].Ordinal != 0 || !call.Arguments[0].Value.Equals(receiver) ||
                call.Arguments[0].Role?.Equals(signature.Parameters[0].Role) != true || call.Arguments[0].Parameter?.Equals(signature.Parameters[0].Descriptor) != true)
                throw Limit("The borrowed call requires its exact direct signature, result value view and already-retained receiver argument.", "borrowed-reference");
        }

        private static BorrowedFieldBinding ValidateBorrowedFieldBinding(FunctionSymbol owner, InitializationStatementOperation statement,
            CallOperation call, BorrowedFieldMethod method)
        {
            var initialization = statement.Initialization; var flags = initialization.Details;
            if (statement.Kind.Name != "stmk_init" || statement.RepresentationRole.Name != "execution" || initialization.Kind.Name != "dik_expression" ||
                initialization.Operation is not { RepresentationRole.Name: "execution" } operation || !operation.Initialization.Equals(initialization) ||
                initialization.Target is not VariableSymbol variable || owner.Body?.Locals.Contains(variable) != true ||
                variable.Details.StorageDuration?.Name != "automatic" || variable.IsThreadLocal || !variable.Type.Equals(method.Reference) ||
                variable.Initializer?.Equals(initialization) != true || flags.Type?.Equals(variable.Type) != true || !Equals(initialization.Expression, call) ||
                initialization.Constructor is not null || initialization.Destructor is not null || initialization.Constant is not null || initialization.Lambda is not null ||
                initialization.BitwiseCopySource is not null || flags.Source is not null || initialization.Operands.Count != 0 ||
                initialization.Lifetime is not null || initialization.InitExpressionLifetime is not null || initialization.OverlappingLifetime is not null ||
                initialization.Master is not null || initialization.NextInDestructionList is not null ||
                flags.IsUnordered || flags.IsConditional || flags.IsReusedValue || flags.IsOverlapping || flags.IsClassResultMaster || flags.IsStaticTemporary ||
                flags.IsPartialAggregate || flags.IsPartialInitialization || flags.HasTemporaryLifetime || flags.IsConstructorInitialization ||
                flags.ConstructorCopyImpliedSource || flags.ConstructorImplicitCopy || flags.ConstructorValueInitialization || flags.ConstructorSequencedArguments ||
                flags.ConstructorArrayCopy || flags.IsQuestionResult || flags.IsCommaResult || flags.FreesStorage || flags.FreesArray || flags.IsConditionalGuard ||
                flags.IsVlaDeallocation || flags.IsFreeingOfExceptionObject || flags.CreatesInitializerList || flags.IsInitializerListArray)
                throw Limit("The borrowed local requires its reciprocal executable reference binding without copy, storage or lifetime obligations.", "borrowed-reference");
            return new(owner, variable, initialization, call, method);
        }

        private void ValidateBorrowedReferenceContext(FunctionSymbol owner)
        {
            var body = owner.Body!;
            bool RegisteredType(CppType type) => borrowedFieldMethods.Values.Any(method => MemoryRepresentationType(type).Equals(method.Reference));
            if (RegisteredType(owner.ReturnType) || owner.Parameters.Any(parameter => RegisteredType(parameter.Type)))
                throw Limit("A borrowed local context does not authorize additional reference parameter or return roles.", "borrowed-reference");
            foreach (var variable in body.Locals.Where(variable => RegisteredType(variable.Type)))
                if (!borrowedFieldBindings.TryGetValue(variable, out var binding) || !binding.Owner.Equals(owner))
                    throw Limit("Every applicable reference local requires its own prevalidated reciprocal call binding.", "borrowed-reference");
            foreach (var node in ExceptionExecutionNodes(body.Root))
            {
                if (node is ExpressionOperation expression && RegisteredType(expression.Type) ||
                    node is ConstantOperation constant && (constant.Constant.Type is { } type && RegisteredType(type) ||
                        constant.Constant.OriginalType is { } original && RegisteredType(original)))
                    throw Limit("The borrowed context has an additional unvalidated reference-typed expression or constant.", "borrowed-reference");
                if (node is ReferenceOperation { Target: VariableSymbol variable } reference && RegisteredType(variable.Type) &&
                    (!borrowedFieldBindings.TryGetValue(variable, out var binding) || !binding.Owner.Equals(owner) ||
                        reference.ValueCategory.Name != "prvalue" || reference.Operands.Count != 0 || !IsBorrowedResultView(reference.Type, binding.Method)))
                    throw Limit("A borrowed local read must retain its registered ordinary pointer value view.", "borrowed-reference");
            }
        }

        private bool IsBorrowedFieldMethod(FunctionSymbol function) => borrowedFieldMethods.ContainsKey(function);

        private bool TryBorrowedReferenceType(PointerType pointer, out string result)
        {
            result = "";
            if (!pointer.IsReference || currentFunction is not { } function) return false;
            var methodReturn = borrowedFieldMethods.TryGetValue(function, out var method) && method.Reference.Equals(pointer);
            var localBinding = borrowedFieldBindings.Values.Any(binding => binding.Owner.Equals(function) && binding.Method.Reference.Equals(pointer));
            if (!methodReturn && !localBinding) return false;
            result = TypeName(pointer.PointeeType) + "*";
            return true;
        }

        private bool IsRegisteredBorrowedCall(CallOperation call, FunctionSymbol function)
        {
            if (!borrowedFieldMethods.ContainsKey(function)) return false;
            if (currentFunction is null || !borrowedFieldCalls.TryGetValue(call, out var binding) ||
                !binding.Owner.Equals(currentFunction) || !binding.Method.Function.Equals(function) || !IsBorrowedResultView(call.Type, binding.Method))
                throw Limit("The borrowed reference method is callable only at its prevalidated direct local-binding occurrences.", "borrowed-reference");
            return true;
        }
    }
}
