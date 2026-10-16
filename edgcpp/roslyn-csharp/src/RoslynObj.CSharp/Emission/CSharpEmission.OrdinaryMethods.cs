using RoslynObj;

namespace RoslynObj.CSharp;

public sealed partial class CSharpEmitter
{
    private sealed partial class Emission
    {
        private sealed record OrdinaryConstMethod(FunctionSymbol Function, RecordType Record,
            PointerType ReceiverType, VariableSymbol Receiver, FieldSymbol Field);
        private readonly Dictionary<FunctionSymbol, OrdinaryConstMethod> ordinaryConstMethods = [];
        private readonly Dictionary<CallOperation, FunctionSymbol> ordinaryConstMethodCalls = [];

        private void InitializeOrdinaryMethods()
        {
            var candidates = functions.Where(function => function.Signature.HasThisParameter &&
                function.SpecialFunctionKind?.Name == "sfk_none" && !IsVirtualMethod(function) && !IsBorrowedFieldMethod(function) &&
                !ordinaryMethodBodies.ContainsKey(function)).ToArray();
            if (candidates.Length == 0) return;
            RequireNormalLifetimeProfile();
            foreach (var function in candidates)
                ordinaryConstMethods.Add(function, ValidateOrdinaryConstMethod(function));
            foreach (var owner in functions)
                foreach (var call in ExceptionExecutionNodes(owner.Body!.Root).OfType<CallOperation>())
                {
                    var direct = call.Callee is ReferenceOperation { Target: FunctionSymbol target } ? CanonicalFunction(target) : null;
                    var retained = call.TargetMethod is { } selected ? CanonicalFunction(selected) : null;
                    if (direct is null || !ordinaryConstMethods.TryGetValue(direct, out var plan))
                    {
                        if (retained is not null && ordinaryConstMethods.ContainsKey(retained))
                            throw Limit("The ordinary const accessor requires its actual direct selected callee.", "ordinary-method");
                        continue;
                    }
                    ValidateOrdinaryConstMethodCall(call, plan);
                    if (ordinaryConstMethodCalls.TryGetValue(call, out var prior) && !prior.Equals(owner))
                        throw Limit("One ordinary method call identity belongs to different generated bodies.", "ordinary-method");
                    ordinaryConstMethodCalls[call] = owner;
                }
        }

        private OrdinaryConstMethod ValidateOrdinaryConstMethod(FunctionSymbol function)
        {
            var signature = function.Signature;
            var body = function.Body ?? throw Limit("The ordinary const accessor requires its selected generated body.", "ordinary-method");
            if (!functions.Contains(function) || function.IsVirtual || function.IsPureVirtual ||
                function.SpecialFunctionKind?.Name != "sfk_none" || function.ConstructorOrDestructorKind?.Name != "cdk_none" ||
                function.PrimaryEntry is not null || function.AlternateEntryPoints.Count != 0 || function.BuiltinKind is not null ||
                function.OverridingFunctionForWrapper is not null || function.OverriddenFunctionForWrapper is not null ||
                function.ThisAdjustment != 0 || function.ReturnAdjustment != 0 || function.VirtualCallIndex != 0 || function.VirtualBaseIndex != 0 ||
                function.Details.IsWeak == true || function.Details.IsWeakref == true || function.Details.IsGnuAlias == true ||
                function.Details.IsIfunc == true || function.Details.IsAliasEntry == true ||
                function.GnuSupplement?.AliasedRoutine is not null || function.GnuSupplement?.TargetedVersions.Count > 0 ||
                function.GnuSupplement?.Representative is not null || function.GnuSupplement?.IsRepresentative == true ||
                function.GnuSupplement?.IsTargetSpecificVersion == true || function.GnuSupplement?.ResolverRequired == true ||
                !signature.HasPrototype || signature.IsVariadic || !signature.HasThisParameter ||
                signature.ReturnsValueAsParameter == true || signature.ReturnsValueViaCopyConstructor || signature.ReturnValueParameterFollowsThis == true ||
                signature.LanguageLinkage?.Name != "nlk_cplusplus_external" || signature.Parameters.Count != 1 || function.Parameters.Count != 1 ||
                function.ContainingType is not RecordType record || signature.ThisClass?.Equals(record) != true)
                throw Limit("The ordinary method adapter supports a fixed unadjusted nonvirtual const receiver and no additional execution parameters.", "ordinary-method");
            ValidateCallingConvention(signature);
            ValidateOrdinaryConstRecord(record);
            ValidateOrdinaryUnsignedByte(function.ReturnType);
            var parameter = function.Parameters[0];
            var selectedParameter = signature.Parameters[0];
            if (parameter.Ordinal != 0 || selectedParameter.Ordinal != 0 || parameter.Role.Name != "this_parameter" ||
                !parameter.Role.Equals(selectedParameter.Role) || !parameter.Descriptor.Equals(selectedParameter.Descriptor) ||
                !parameter.Type.Equals(selectedParameter.Type) || parameter.Symbol is not { } receiver ||
                parameter.BaseSubobject is not null || selectedParameter.BaseSubobject is not null || parameter.Descriptor.Details.PassedViaCopyConstructor ||
                parameter.Type is not PointerType { IsReference: false, IsRValueReference: false } pointer ||
                pointer.PointeeType is not QualifiedType { IsConst: true, IsVolatile: false } constant || !constant.UnderlyingType.Equals(record) ||
                pointer.Size != 8 || pointer.Alignment != 8 || constant.Size != record.Size || constant.Alignment != record.Alignment)
                throw Limit("The ordinary accessor requires its exact selected natural pointer to the const containing record.", "ordinary-method");
            ValidateAggregateQualifiers(pointer); ValidateAggregateQualifiers(constant); ValidateAggregateQualifiers(receiver.Type);
            if (!receiver.Type.Equals(pointer) && (receiver.Type is not QualifiedType { IsConst: true, IsVolatile: false } pointerObject ||
                !pointerObject.UnderlyingType.Equals(pointer) || pointerObject.Size != pointer.Size || pointerObject.Alignment != pointer.Alignment))
                throw Limit("The execution receiver permits only the retained const pointer-object wrapper over its signature pointer.", "ordinary-method");
            var eh = body.ExceptionHandling;
            if (body.Locals.Count != 0 || body.Lifetimes.Count != 0 || eh.RegionTable is not null || eh.ArrayTable is not null ||
                eh.CleanupRegions.Count != 0 || eh.Arrays.Count != 0 || eh.Details?.Prologue is not null || eh.Epilogues.Count != 0 ||
                eh.Details?.InitialState is not null || eh.Details?.NullState is not null ||
                body.Root is not BlockOperation block || block.Lifetime is not null || block.Statements.Count != 1 ||
                block.Statements[0] is not JumpOperation { JumpKind.Name: "return", Target: null, CommonLifetime: null } returned ||
                returned.Value is not OperatorOperation { OperatorKind.Name: "eok_points_to_field" } read || read.Operands.Count != 2 ||
                read.ValueCategory.Name != "prvalue" || !read.Type.Equals(function.ReturnType) ||
                read.Operator.RetainedVirtualCall || read.Operator.RequiresRuntimeCastCheck || read.Details.VolatileFetch ||
                read.Operands[0] is not ReferenceOperation { Target: VariableSymbol actualReceiver } receiverValue || !actualReceiver.Equals(receiver) ||
                receiverValue.Operands.Count != 0 || receiverValue.ValueCategory.Name != "prvalue" || !receiverValue.Type.Equals(pointer) || receiverValue.Details.VolatileFetch ||
                read.Operands[1] is not ReferenceOperation { Target: FieldSymbol field } fieldValue || fieldValue.Operands.Count != 0 ||
                !field.Type.Equals(read.Type) || !fieldValue.Type.Equals(field.Type) || fieldValue.Details.VolatileFetch ||
                field.ContainingType?.Equals(record) != true || !record.Fields.Contains(field) || field.IsBitField || field.IsGeneratedBase ||
                field.IsGeneratedVirtualPointer == true || field.IsPacked == true ||
                record.Layout!.Fields.Count(layout => layout.Field.Equals(field) && layout.Offset is { } offset && offset >= 0 &&
                    offset + field.Type.Size <= record.Size && layout.BitWidth is null) != 1)
                throw Limit("The ordinary accessor body supports only its direct unsigned-byte field return, without calls, locals, lifetime or EH execution.", "ordinary-method");
            foreach (var node in new SemanticNode[] { block, returned, read, receiverValue, fieldValue })
                if (node.RepresentationRole.Name != "execution")
                    throw Limit("The ordinary accessor body requires actual retained execution occurrences.", "ordinary-method");
            ValidateOrdinaryUnsignedByte(field.Type);
            // This body proof authorizes a direct managed call only. It does not
            // change the captured noexcept fact or enter validatedNoThrowFunctions.
            return new(function, record, pointer, receiver, field);
        }

        private static void ValidateOrdinaryConstRecord(RecordType record)
        {
            ValidateAggregateQualifiers(record);
            if (record.Kind.Name is not ("tk_struct" or "tk_class") || record.IsIncomplete || record.IsPacked ||
                record.Bases.Count != 0 || record.BaseSubobjects.Count != 0 || record.HasVirtualBases || record.HasVirtualFunctions ||
                record.VirtualFunctionTableVariable is not null || record.VirtualTableTableVariable is not null ||
                record.SubobjectPartner is { } partner && !partner.Equals(record) ||
                record.Size is not { } size || size <= 0 || record.Alignment is not { } alignment || alignment <= 0 ||
                record.Layout is not { IsComplete: true } layout || layout.NonvirtualSize != size || layout.NonvirtualAlignment != alignment ||
                layout.Fields.Any(field => field.Field.IsGeneratedBase || field.Field.IsGeneratedVirtualPointer == true))
                throw Limit("The ordinary const receiver requires a complete ordinary record without base, virtual or occupied-subobject adaptation.", "ordinary-method");
        }

        private static void ValidateOrdinaryUnsignedByte(CppType type)
        {
            ValidateAggregateQualifiers(type);
            if (MemoryRepresentationType(type) is not ArithmeticType { Kind.Name: "tk_integer", Arithmetic.Bits: 8, Arithmetic.Signed: false, Arithmetic.IsBool: false } value ||
                type.Size != 1 || type.Alignment != 1 || value.Size != 1 || value.Alignment != 1)
                throw Limit("The ordinary accessor result requires its selected natural unsigned 8-bit representation.", "ordinary-method");
        }

        private static void ValidateOrdinaryConstMethodCall(CallOperation call, OrdinaryConstMethod plan)
        {
            var function = plan.Function;
            var signature = function.Signature;
            if (call.Callee is not ReferenceOperation { Target: FunctionSymbol target } callee || !target.Equals(function) || callee.Operands.Count != 0 ||
                call.TargetMethod?.Equals(function) != true || call.CallKind.Name != "direct" ||
                call.SourceTargetMethod is not null || call.SourceDispatchKind is not null || call.Operator.RetainedVirtualCall ||
                !call.Operator.EvalLeftToRight || call.Operator.EvalRightToLeft || call.Operator.RequiresRuntimeCastCheck ||
                call.RepresentationRole.Name != "execution" || call.ValueCategory.Name != "prvalue" ||
                call.Call.FunctionType?.Equals(signature) != true || !call.Type.Equals(function.ReturnType) || call.Call.ReturnSlot is not null ||
                call.Receiver is not { } receiver || !receiver.Type.Equals(plan.ReceiverType) ||
                call.Arguments.Count != 1 || call.Operands.Count != 2 || !call.Operands[0].Equals(call.Callee) || !call.Operands[1].Equals(receiver) ||
                call.Arguments[0].Ordinal != 0 || !call.Arguments[0].Value.Equals(receiver) ||
                call.Arguments[0].Role?.Equals(signature.Parameters[0].Role) != true ||
                call.Arguments[0].Parameter?.Equals(signature.Parameters[0].Descriptor) != true)
                throw Limit("The ordinary accessor call requires its selected direct signature and one already-retained const receiver argument in order.", "ordinary-method");
            ValidateAggregateQualifiers(receiver.Type);
        }

        private void ValidateRegisteredOrdinaryMethodCall(CallOperation call, FunctionSymbol function)
        {
            if (!ordinaryConstMethods.ContainsKey(function)) return;
            if (currentFunction is null || !ordinaryConstMethodCalls.TryGetValue(call, out var owner) || !owner.Equals(currentFunction))
                throw Limit("The ordinary const accessor is callable only at its prevalidated direct generated occurrences.", "ordinary-method");
        }
    }
}
