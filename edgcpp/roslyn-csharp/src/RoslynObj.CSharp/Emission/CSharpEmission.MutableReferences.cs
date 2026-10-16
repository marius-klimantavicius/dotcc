using RoslynObj;

namespace RoslynObj.CSharp;

public sealed partial class CSharpEmitter
{
    private sealed partial class Emission
    {
        private sealed record MutableReferenceRoutine(FunctionSymbol Function, RecordType Record,
            PointerType Mutable, PointerType ReadOnly, QualifiedType ConstRecord);
        private MutableReferenceRoutine? mutableReferenceRoutine;
        private readonly Dictionary<CallOperation, FunctionSymbol> mutableReferenceCalls = [];

        private void InitializeMutableReferences()
        {
            var candidates = functions.Where(function => function.Parameters.Any(parameter =>
                MemoryRepresentationType(parameter.Type) is PointerType { IsReference: true, PointeeType: RecordType })).ToArray();
            if (candidates.Length == 0) return;
            RequireNormalLifetimeProfile();
            if (candidates.Length != 1 || selectedProgram is not null || HostProtocolEnabled || classExceptionActive || exceptionFunctions.Count != 0)
                throw Limit("The mutable-reference projection supports one selected generated helper without exception/native boundary adaptation.", "mutable-reference");
            var function = candidates[0];
            var signature = function.Signature;
            if (function.Body is null || !signature.HasPrototype || signature.IsVariadic || signature.HasThisParameter || signature.ThisClass is not null ||
                signature.ReturnsValueAsParameter == true || signature.ReturnsValueViaCopyConstructor || signature.ReturnValueParameterFollowsThis == true ||
                signature.ExceptionSpecification?.NoexceptEffective != true || signature.LanguageLinkage?.Name != "nlk_cplusplus_external" ||
                function.SpecialFunctionKind?.Name != "sfk_none" || function.ContainingType is not null || function.PrimaryEntry is not null || function.AlternateEntryPoints.Count != 0 ||
                function.Details.IsWeak == true || function.Details.IsWeakref == true || function.Details.IsGnuAlias == true || function.Details.IsIfunc == true ||
                function.Details.IsAliasEntry == true || function.GnuSupplement?.AliasedRoutine is not null ||
                MemoryRepresentationType(function.ReturnType).Kind.Name != "tk_void" || function.Parameters.Count != 2 || signature.Parameters.Count != 2 ||
                function.Parameters[0].Type is not PointerType { IsReference: true, IsRValueReference: false, PointeeType: RecordType record } mutable ||
                function.Parameters[1].Type is not PointerType { IsReference: true, IsRValueReference: false, PointeeType: QualifiedType { IsConst: true, IsVolatile: false } constant } readOnly ||
                !constant.UnderlyingType.Equals(record))
                throw Limit("The mutable-reference helper requires the captured void noexcept signature with one mutable and one const reference to the same record.", "mutable-reference");
            ValidateCallingConvention(signature);
            ValidateMutableReferenceRecord(record);
            ValidateAggregateQualifiers(mutable); ValidateAggregateQualifiers(readOnly); ValidateAggregateQualifiers(constant);
            if (mutable.Size != 8 || mutable.Alignment != 8 || readOnly.Size != 8 || readOnly.Alignment != 8 || constant.Size != 8 || constant.Alignment != 4)
                throw Limit("The two generated lvalue references require their exact natural pointer and pointee representations.", "mutable-reference");
            for (var index = 0; index < 2; ++index)
            {
                var parameter = function.Parameters[index]; var selected = signature.Parameters[index];
                if (parameter.Ordinal != index || selected.Ordinal != index || parameter.Role.Name != "ordinary_parameter" ||
                    !parameter.Role.Equals(selected.Role) || !parameter.Descriptor.Equals(selected.Descriptor) || !parameter.Type.Equals(selected.Type) ||
                    parameter.Symbol is null || !parameter.Symbol.Type.Equals(parameter.Type) || parameter.BaseSubobject is not null || selected.BaseSubobject is not null ||
                    parameter.Descriptor.Details.PassedViaCopyConstructor)
                    throw Limit("The selected mutable-reference parameters have an unsupported hidden/copy execution role.", "mutable-reference");
            }
            ValidateMutableReferenceBody(function);
            EnsureGeneratedNoThrow(function);
            var plan = new MutableReferenceRoutine(function, record, mutable, readOnly, constant);
            var graph = functions.ToDictionary(owner => owner, owner => ExceptionExecutionNodes(owner.Body!.Root).ToArray());
            var callers = graph.SelectMany(pair => pair.Value.OfType<CallOperation>().Where(call =>
                call.Callee is ReferenceOperation { Target: FunctionSymbol target } && target.Equals(function) || call.TargetMethod?.Equals(function) == true)
                .Select(call => (Function: pair.Key, Call: call))).ToArray();
            if (callers.Length != 2 || !callers[0].Function.Equals(callers[1].Function) || callers[0].Function.Equals(function))
                throw Limit("The bounded mutable-reference slice requires its two retained direct calls from one separate generated caller.", "mutable-reference");
            var caller = callers[0].Function;
            if (caller.Signature.ExceptionSpecification?.NoexceptEffective != true || NeedsGeneratedLifetimeRoles(caller.Signature))
                throw Limit("The exercised mutable-reference caller requires its captured ordinary noexcept signature.", "mutable-reference");
            ValidateMutableReferenceBody(caller);
            EnsureGeneratedNoThrow(caller);
            foreach (var occurrence in callers)
            {
                ValidateMutableReferenceCallShape(plan, caller, occurrence.Call, graph[caller]);
                if (!mutableReferenceCalls.TryAdd(occurrence.Call, caller))
                    throw Limit("One mutable-reference call identity has more than one retained execution occurrence.", "mutable-reference");
            }
            mutableReferenceRoutine = plan;
        }

        private static void ValidateMutableReferenceRecord(RecordType record)
        {
            ValidateAggregateQualifiers(record);
            if (record.Kind.Name is not ("tk_struct" or "tk_class") || record.IsIncomplete || record.IsPacked || record.Bases.Count != 0 || record.BaseSubobjects.Count != 0 ||
                record.HasVirtualBases || record.HasVirtualFunctions || record.VirtualFunctionTableVariable is not null || record.VirtualTableTableVariable is not null ||
                record.Size != 8 || record.Alignment != 4 || record.Layout is not { IsComplete: true, NonvirtualSize: { } nonvirtualSize } layout ||
                nonvirtualSize != 8 || layout.NonvirtualAlignment != 4 || layout.Fields.Count != 2 || record.Fields.Count != 2)
                throw Limit("The mutable-reference record requires its captured complete ordinary 8-byte/4-aligned scalar layout.", "mutable-reference");
            for (var index = 0; index < 2; ++index)
            {
                var field = layout.Fields[index];
                if (field.Offset != index * 4 || field.BitWidth is not null || field.Field.IsBitField || field.Field.IsGeneratedBase || field.Field.IsPacked == true ||
                    field.Field.ContainingType?.Equals(record) != true || !record.Fields.Contains(field.Field))
                    throw Limit("The mutable-reference record has an unsupported field layout or ownership view.", "mutable-reference");
                ValidateGeneratedInt32(field.Field.Type);
            }
        }

        private static void ValidateMutableReferenceBody(FunctionSymbol function)
        {
            var body = function.Body ?? throw Limit("The mutable-reference context requires its retained generated body.", "mutable-reference");
            var eh = body.ExceptionHandling;
            if (eh.RegionTable is not null || eh.ArrayTable is not null || eh.CleanupRegions.Count != 0 || eh.Arrays.Count != 0 ||
                eh.Details?.Prologue is not null || eh.Epilogues.Count != 0 || body.Lifetimes.Any(lifetime => lifetime.Cleanups.Count != 0) ||
                ExceptionExecutionNodes(body.Root).Any(node => node is ThrowOperation or TryOperation or AllocationOperation or LoweredEhOperation or TemporaryOperation ||
                    node is CallOperation { TargetMethod.SpecialFunctionKind.Name: "sfk_constructor" or "sfk_destructor" }))
                throw Limit("The mutable-reference slice does not adapt construction, cleanup, temporary materialization or exception execution.", "mutable-reference");
        }

        private static void ValidateMutableReferenceCallShape(MutableReferenceRoutine plan, FunctionSymbol caller, CallOperation call, IReadOnlyList<SemanticNode> nodes)
        {
            var body = caller.Body ?? throw Limit("The mutable-reference caller requires its retained generated body.", "mutable-reference");
            if (call.Callee is not ReferenceOperation { Target: FunctionSymbol target } || !target.Equals(plan.Function) || call.TargetMethod?.Equals(target) != true ||
                call.CallKind.Name != "direct" || !call.Operator.EvalLeftToRight || call.Operator.EvalRightToLeft || call.Operator.RetainedVirtualCall ||
                call.Receiver is not null || call.Call.ReturnSlot is not null || call.Call.FunctionType?.Equals(target.Signature) != true ||
                !call.Type.Equals(target.ReturnType) || call.ValueCategory.Name != "prvalue" || call.Arguments.Count != 2 || call.Operands.Count != 3 ||
                !call.Operands[0].Equals(call.Callee))
                throw Limit("The mutable-reference call requires its exact selected direct signature and retained left-to-right argument occurrences.", "mutable-reference");
            for (var index = 0; index < 2; ++index)
            {
                var argument = call.Arguments[index]; var parameter = target.Signature.Parameters[index];
                if (argument.Ordinal != index || argument.Role?.Equals(parameter.Role) != true || argument.Parameter?.Equals(parameter.Descriptor) != true ||
                    !call.Operands[index + 1].Equals(argument.Value))
                    throw Limit("A mutable-reference argument differs from its selected public parameter association.", "mutable-reference");
            }
            if (call.Arguments[0].Value is not OperatorOperation { OperatorKind.Name: "eok_address_of" } address || address.Operands.Count != 1 ||
                address.Operands[0] is not ReferenceOperation { Target: VariableSymbol destination } destinationValue || !destination.Type.Equals(plan.Record) ||
                !body.Locals.Contains(destination) || destinationValue.ValueCategory.Name != "lvalue" ||
                address.Type is not PointerType { IsReference: false, IsRValueReference: false } mutableView || !mutableView.PointeeType.Equals(plan.Record) ||
                call.Arguments[1].Value is not ReferenceOperation { Target: VariableSymbol temporary } sourceValue || sourceValue.ValueCategory.Name != "prvalue" ||
                sourceValue.Type is not PointerType { IsReference: false, IsRValueReference: false } constView || !constView.PointeeType.Equals(plan.ConstRecord) ||
                !temporary.Type.Equals(constView) || !body.Locals.Contains(temporary) || mutableView.Size != 8 || mutableView.Alignment != 8 ||
                constView.Size != 8 || constView.Alignment != 8)
                throw Limit("The retained mutable-reference arguments need an actual local address and the explicit const-pointer temporary.", "mutable-reference");
            ValidateAggregateQualifiers(mutableView); ValidateAggregateQualifiers(constView);
            var commas = nodes.OfType<OperatorOperation>().Where(operation => operation.OperatorKind.Name == "eok_comma" &&
                operation.Operands.Count == 2 && operation.Operands[1].Equals(call)).ToArray();
            if (commas.Length != 1 || commas[0].Operands[0] is not OperatorOperation { OperatorKind.Name: "eok_assign" } assignment ||
                !assignment.Operator.EvalRightToLeft || assignment.Operator.EvalLeftToRight || assignment.Operands.Count != 2 ||
                assignment.Operands[0] is not ReferenceOperation { Target: VariableSymbol assigned } temporaryPlace || !assigned.Equals(temporary) ||
                temporaryPlace.ValueCategory.Name != "lvalue" || !temporaryPlace.Type.Equals(constView) ||
                assignment.Operands[1] is not OperatorOperation { OperatorKind.Name: "eok_address_of" } sourceAddress || sourceAddress.Operands.Count != 1 ||
                !sourceAddress.Type.Equals(constView) || sourceAddress.Operands[0] is not ConversionOperation { OperatorKind.Name: "eok_lvalue_adjust" } qualification ||
                qualification.Operands.Count != 1 || qualification.Operator.RequiresRuntimeCastCheck || !qualification.Type.Equals(plan.ConstRecord) ||
                qualification.ValueCategory.Name != "lvalue" || !qualification.SourceType.Equals(plan.Record) || !qualification.TargetType.Equals(plan.ConstRecord) ||
                qualification.Operand is not ReferenceOperation { Target: VariableSymbol source } sourcePlace || !source.Type.Equals(plan.Record) ||
                !body.Locals.Contains(source) || sourcePlace.ValueCategory.Name != "lvalue" ||
                nodes.OfType<OperatorOperation>().Count(operation => operation.OperatorKind.Name == "eok_assign" && operation.Operands.Count == 2 &&
                    operation.Operands[0] is ReferenceOperation { Target: VariableSymbol variable } && variable.Equals(temporary)) != 1)
                throw Limit("The const reference must retain its one explicit qualification/address/temporary assignment before the actual call.", "mutable-reference");
            // No source/destination inequality is assumed. The retained pointers
            // may alias, so all later reads and stores stay in their own plans.
        }

        private bool TryValidateMutableReferenceRoutine(FunctionSymbol function) => mutableReferenceRoutine?.Function.Equals(function) == true;

        private bool TryMutableReferenceType(PointerType pointer, out string result)
        {
            result = "";
            if (mutableReferenceRoutine is not { } plan || currentFunction is not { } function ||
                !function.Equals(plan.Function) && !mutableReferenceCalls.Values.Contains(function) ||
                !pointer.Equals(plan.Mutable) && !pointer.Equals(plan.ReadOnly)) return false;
            result = TypeName(pointer.PointeeType) + "*";
            return true;
        }

        private void ValidateRegisteredMutableReferenceCall(CallOperation call, FunctionSymbol function)
        {
            if (mutableReferenceRoutine?.Function.Equals(function) != true) return;
            if (currentFunction is null || !mutableReferenceCalls.TryGetValue(call, out var owner) || !owner.Equals(currentFunction))
                throw Limit("The mutable-reference routine is callable only at its prevalidated generated execution occurrences.", "mutable-reference");
        }
    }
}
