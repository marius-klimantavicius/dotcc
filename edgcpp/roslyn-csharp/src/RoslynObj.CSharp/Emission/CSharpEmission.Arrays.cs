using System.Globalization;
using System.Numerics;
using RoslynObj;

namespace RoslynObj.CSharp;

public sealed partial class CSharpEmitter
{
    private sealed partial class Emission
    {
        private sealed record AutomaticArrayPlan(FunctionSymbol Function, VariableSymbol Variable, ArrayType Type,
            RecordType Element, FunctionSymbol Constructor, FunctionSymbol Destructor, CallOperation Construct,
            CallOperation Destroy, CleanupAction Cleanup, Initialization FullRecipe, Initialization PartialRecipe,
            string ConstructMethod, string DestroyMethod);

        private AutomaticArrayPlan? automaticArray;
        private readonly Dictionary<CallOperation, bool> automaticArrayCalls = [];
        private readonly HashSet<FunctionSymbol> automaticArrayConstructors = [];

        // This recognizes a retained compiler-runtime operation only in its
        // matching automatic-array cleanup context. Ordinary same-named calls
        // have no semantic implementation and remain external requirements.
        private void InitializeAutomaticArray(IReadOnlyDictionary<FunctionSymbol, SemanticNode[]> graph,
            FunctionSymbol throwingFunction, FunctionSymbol catchingFunction)
        {
            var body = catchingFunction.Body!;
            var eh = body.ExceptionHandling;
            if (eh.Arrays.Count != 1 || eh.ArrayTable is null || !body.Locals.Contains(eh.ArrayTable) ||
                eh.CleanupRegions.Count != 2 || throwingFunction.Body is not { } constructorBody ||
                constructorBody.ExceptionHandling.Details is { } constructorEh &&
                    (constructorEh.InitialState is not null || constructorEh.NullState is not null || constructorEh.Prologue is not null) ||
                constructorBody.ExceptionHandling.RegionTable is not null || constructorBody.ExceptionHandling.Epilogues.Count != 0 ||
                constructorBody.ExceptionHandling.ArrayTable is not null || constructorBody.ExceptionHandling.CleanupRegions.Count != 0 ||
                constructorBody.ExceptionHandling.Arrays.Count != 0 || constructorBody.Locals.Count != 0 || constructorBody.Lifetimes.Count != 0)
                throw Limit("The automatic-array slice requires one retained full-array cleanup and a table-free throwing element constructor.", "array-lifetime");
            var array = eh.Arrays[0];
            var actions = eh.CleanupRegions.SelectMany(region => region.Actions).Where(action => action.Array?.Equals(array) == true).ToArray();
            if (actions.Length != 1 || actions[0] is not { Storage: { } storage, TargetMethod: { } destructor } action ||
                action.Kind.Name != "destructor" || !action.Details.IsArray || action.Details.IsIndirect || action.Details.IsConditional ||
                action.Details.IsBaseSubobject || action.Details.IsVla || action.Details.IsDeallocation || action.Details.IsLocalStaticGuard ||
                action.AbiDetail is not null || storage.Kind.Name != "direct_variable" || storage.Variable is not { } variable ||
                !body.Locals.Contains(variable) || storage.Offset != 0 || storage.IsIndirect || storage.UsesThisParameterOffset || storage.Details.BaseEntity is not null ||
                variable.Details.StorageDuration?.Name != "automatic" || variable.IsThreadLocal || variable.InitializationKind.Name != "initk_none" ||
                variable.Initializer is not null || variable.ConstantInitializer is not null || variable.LocalStaticInitializations.Count != 0 ||
                MemoryRepresentationType(variable.Type) is not ArrayType arrayType || arrayType.Array.BoundKind.Name != "constant" ||
                arrayType.Bound is not { } count || count <= 0 || count > long.MaxValue || arrayType.BoundExpression is not null ||
                arrayType.VlaDimension is not null || arrayType.DependentBoundExpression is not null ||
                array.ElementCount != count || array.ElementSize <= 0 || array.Details.IsNonarraySize || array.CountLocations.Count != 0 ||
                !array.StorageLocations.Contains(storage) || arrayType.Size != count * array.ElementSize ||
                array.HandleConstant.Type is not { } handleType || array.HandleConstant.OriginalType is not null ||
                array.HandleConstant.Value is not ConstantValue<StackOffsetValueDetails> handle ||
                !handle.Payload.Variable.Equals(variable) || handle.Payload.Offset != storage.Offset)
                throw Limit("The automatic-array cleanup must retain its exact constant count, stride and direct automatic-variable handle.", "array-lifetime");
            ValidateGeneratedUInt64(handleType);
            ValidateAggregateQualifiers(variable.Type);
            ValidateAggregateQualifiers(arrayType.ElementType);
            if (MemoryRepresentationType(arrayType.ElementType) is not RecordType element || element.Kind.Name != "tk_struct" ||
                element.IsIncomplete || element.IsPacked || element.HasVirtualBases || element.HasVirtualFunctions ||
                element.Bases.Count != 0 || element.Size != array.ElementSize || element.Size != 4 || element.Alignment != 4 ||
                arrayType.Alignment != element.Alignment || element.Layout is not { } layout || layout.Fields.Count != 1 ||
                layout.Fields[0] is not { Offset: var offset, Field: { } field } || offset != 0 || field.Details.IsBitField)
                throw Limit("The automatic-array element requires the exercised ordinary single-integer record layout.", "array-lifetime");
            ValidateAggregateQualifiers(field.Type);
            ValidateGeneratedInt32(field.Type);
            ValidateArrayElementRoutine(throwingFunction, element, false);
            ValidateArrayElementRoutine(destructor, element, true);

            var calls = graph[catchingFunction].OfType<CallOperation>().ToArray();
            var constructors = calls.Where(call => IsRuntimeHelperOperand(call, "__cxa_vec_ctor")).ToArray();
            var destructors = calls.Where(call => IsRuntimeHelperOperand(call, "__cxa_vec_dtor")).ToArray();
            if (constructors.Length != 1 || destructors.Length != 1)
                throw Limit("The automatic-array context requires its actual constructor and normal destructor helper occurrences.", "array-lifetime");
            var construct = constructors[0];
            var destroy = destructors[0];
            var constructHelper = ValidateAutomaticArrayHelperCall(construct, variable, arrayType, throwingFunction, destructor, true);
            var destroyHelper = ValidateAutomaticArrayHelperCall(destroy, variable, arrayType, throwingFunction, destructor, false);
            if (!HasArrayCompletionState(graph[catchingFunction], construct, action.Details.Region, variable, arrayType) ||
                !HasArrayDestructionState(graph[catchingFunction], destroy, eh.Details!.NullState))
                throw Limit("The retained full-array state must arm after successful construction and clear before normal destruction.", "array-lifetime");

            var recipes = body.Lifetimes.SelectMany(lifetime => lifetime.Cleanups.Select(cleanup => cleanup.Initialization)).ToArray();
            var fullRecipes = recipes.Where(recipe => recipe.Target?.Equals(variable) == true && recipe.Destructor?.Equals(destructor) == true).ToArray();
            if (fullRecipes.Length != 1 || fullRecipes[0].NextInDestructionList is not { } partial || !recipes.Contains(partial) ||
                partial.Target is not null || partial.Details.Type is not null || !partial.Details.IsPartialAggregate ||
                partial.Destructor?.Equals(destructor) != true || !Equals(partial.Lifetime, fullRecipes[0].Lifetime) || partial.NextInDestructionList is not null ||
                fullRecipes[0].Details.Type?.Equals(variable.Type) != true)
                throw Limit("The full-array recipe must name its exact linked metadata-only partial-aggregate companion.", "array-lifetime");
            ValidateExceptionMetadataRecipe(fullRecipes[0], false);
            ValidateExceptionMetadataRecipe(partial, false, arrayPartial: true);
            automaticArray = new(catchingFunction, variable, arrayType, element, throwingFunction, destructor, construct, destroy, action,
                fullRecipes[0], partial, InternalName("__ConstructAutomaticArray"), InternalName("__DestroyAutomaticArray"));
            automaticArrayCalls.Add(construct, true);
            automaticArrayCalls.Add(destroy, false);
            automaticArrayConstructors.Add(throwingFunction);
            validatedThrowingConstructors.Add(throwingFunction);
            ValidateGeneratedLifetimeRoutine(throwingFunction);
            ValidateGeneratedLifetimeRoutine(destructor);
            EnsureGeneratedNoThrow(destructor);
            semanticRuntimeFunctions.Add(new(constructHelper, SemanticRuntimeFunctionImplementation.GeneratedVectorConstructorContract));
            semanticRuntimeFunctions.Add(new(destroyHelper, SemanticRuntimeFunctionImplementation.GeneratedVectorDestructorContract));
        }

        private void ValidateAutomaticArrayTable(FunctionSymbol function, IReadOnlyList<SemanticNode> nodes)
        {
            var eh = function.Body!.ExceptionHandling;
            var table = eh.ArrayTable!;
            if (table.ExceptionTable is not { } projection || projection.Kind.Name != "element" || !projection.Owner.Equals(table) ||
                !projection.FunctionEh.Routine.Equals(function) || projection.FunctionEh.ArrayTable?.Equals(table) != true ||
                projection.FunctionEh.RegionTable?.Equals(eh.RegionTable) != true ||
                eh.RegionTable!.ExceptionTable!.FunctionEh.ArrayTable?.Equals(table) != true ||
                nodes.OfType<ReferenceOperation>().Any(reference => reference.Target?.Equals(table) == true) ||
                table.Initializer is not null || table.ConstantInitializer is not null ||
                table.LocalStaticInitializations.Any(initializer => !initializer.Variable.Equals(table) || initializer.Kind.Name != "initk_static" ||
                    initializer.Initialization is not null || initializer.Constant is not null || initializer.Lifetime is not null))
                throw Limit("The array table must be its exact metadata-only element projection without an application initializer or value reference.", "array-lifetime");
        }

        private static void ValidateGeneratedUInt64(CppType type)
        {
            ValidateAggregateQualifiers(type);
            if (MemoryRepresentationType(type) is not ArithmeticType arithmetic || arithmetic.Arithmetic.Bits != 64 || arithmetic.Arithmetic.Signed != false ||
                type.Size != 8 || type.Alignment != 8 || arithmetic.Size != 8 || arithmetic.Alignment != 8)
                throw Limit("The vector helper requires the retained natural unsigned 64-bit count and stride.", "array-lifetime");
        }

        private void ValidateArrayElementRoutine(FunctionSymbol function, RecordType element, bool destructor)
        {
            var signature = function.Signature;
            if (!functions.Contains(function) || function.Body is null || function.SpecialFunctionKind?.Name != (destructor ? "sfk_destructor" : "sfk_constructor") ||
                function.ConstructorOrDestructorKind?.Name != "cdk_complete" || function.PrimaryEntry is not null ||
                !signature.HasPrototype || signature.IsVariadic || !signature.HasThisParameter || signature.ReturnsValueAsParameter == true ||
                signature.ReturnsValueViaCopyConstructor || signature.ReturnValueParameterFollowsThis == true ||
                MemoryRepresentationType(signature.ReturnType).Kind.Name != "tk_void" || signature.Parameters.Count != 1 || function.Parameters.Count != 1 ||
                function.Parameters[0].Role.Name != "this_parameter" || function.Parameters[0].Symbol is null ||
                !GeneratedRecordPointer(function.Parameters[0].Type, false).Equals(element) ||
                destructor && signature.ExceptionSpecification?.NoexceptEffective != true)
                throw Limit("The array callbacks require the actual selected receiver-only complete constructor and closed noexcept destructor.", "array-lifetime");
            ValidateCallingConvention(signature);
        }

        private static bool IsRuntimeHelperOperand(CallOperation call, string name) =>
            call.Callee is ReferenceOperation { Target: FunctionSymbol target } && target.CurrentName == name;

        private FunctionSymbol ValidateAutomaticArrayHelperCall(CallOperation call, VariableSymbol variable, ArrayType array,
            FunctionSymbol constructor, FunctionSymbol destructor, bool construct)
        {
            if (call.Callee is not ReferenceOperation { Target: FunctionSymbol helper } || call.TargetMethod?.Equals(helper) != true ||
                helper.CurrentName != (construct ? "__cxa_vec_ctor" : "__cxa_vec_dtor") || helper.Body is not null || helper.BodyState.Name != "declaration_only" ||
                helper.PrimaryEntry is not null || helper.AlternateEntryPoints.Count != 0 || helper.Details.IsWeak == true || helper.Details.IsWeakref == true ||
                helper.Details.IsGnuAlias == true || helper.Details.IsIfunc == true || helper.Details.IsAliasEntry == true || helper.Details.IsUseComdat == true ||
                options.ExternalFunctions.Any(binding => binding.LinkageName == helper.CurrentName) ||
                helper.GnuSupplement?.AssemblyName is { } assemblyName && assemblyName != helper.CurrentName)
                throw Limit("The retained vector helper has an unimplemented definition, binding or entry policy.", "array-lifetime");
            var signature = helper.Signature;
            var arity = construct ? 5 : 4;
            if (!signature.HasPrototype || signature.IsVariadic || signature.HasThisParameter || signature.ReturnsValueAsParameter == true ||
                signature.ReturnsValueViaCopyConstructor || signature.ReturnValueParameterFollowsThis == true || signature.LanguageLinkage?.Name != "nlk_cplusplus_external" ||
                MemoryRepresentationType(signature.ReturnType).Kind.Name != "tk_void" || signature.Parameters.Count != arity || helper.Parameters.Count != arity ||
                call.Call.FunctionType?.Equals(signature) != true || !call.Type.Equals(signature.ReturnType) || call.ValueCategory.Name != "prvalue" ||
                call.Receiver is not null || call.Call.ReturnSlot is not null || !call.Operator.EvalLeftToRight || call.Operator.EvalRightToLeft ||
                call.Operands.Count != arity + 1 || !call.Operands[0].Equals(call.Callee) || call.Arguments.Count != arity)
                throw Limit("The vector helper requires its exact fixed ABI-parameter signature and retained left-to-right operand occurrence order.", "array-lifetime");
            ValidateCallingConvention(signature);
            for (var index = 0; index < arity; ++index)
            {
                var parameter = signature.Parameters[index];
                var argument = call.Arguments[index];
                if (parameter.Ordinal != index || parameter.Role.Name != "abi_parameter" || parameter.BaseSubobject is not null || parameter.Descriptor.Details.PassedViaCopyConstructor ||
                    helper.Parameters[index].Role.Name != "abi_parameter" || !helper.Parameters[index].Descriptor.Equals(parameter.Descriptor) ||
                    argument.Ordinal != index || argument.Role?.Name != "abi_parameter" || argument.Parameter?.Equals(parameter.Descriptor) != true ||
                    !argument.Value.Equals(call.Operands[index + 1]))
                    throw Limit("The vector helper's call and selected ABI descriptors differ.", "array-lifetime");
            }
            ValidateVectorVoidPointer(signature.Parameters[0].Type);
            ValidateGeneratedUInt64(signature.Parameters[1].Type);
            ValidateGeneratedUInt64(signature.Parameters[2].Type);
            if (!IsArrayBase(call.Arguments[0].Value, variable, array) ||
                !IsArrayInteger(call.Arguments[1].Value, array.Bound!.Value) || !IsArrayInteger(call.Arguments[2].Value, array.ElementType.Size!.Value))
                throw Limit("The actual vector helper operands must name the retained array base, count and element stride.", "array-lifetime");
            ValidateVectorCallback(call.Arguments[3].Value, signature.Parameters[3].Type, construct ? constructor : destructor);
            if (construct) ValidateVectorCallback(call.Arguments[4].Value, signature.Parameters[4].Type, destructor);
            return helper;
        }

        private static void ValidateVectorVoidPointer(CppType type)
        {
            ValidateAggregateQualifiers(type);
            if (MemoryRepresentationType(type) is not PointerType { IsReference: false, IsRValueReference: false } pointer ||
                type.Size != 8 || type.Alignment != 8 || pointer.Size != 8 || pointer.Alignment != 8 ||
                MemoryRepresentationType(pointer.PointeeType).Kind.Name != "tk_void")
                throw Limit("The vector ABI requires its ordinary natural void pointer.", "array-lifetime");
            ValidateAggregateQualifiers(pointer.PointeeType);
        }

        private static void ValidateVectorCallback(ExpressionOperation value, CppType expectedType, FunctionSymbol target)
        {
            if (!value.Type.Equals(expectedType)) throw Limit("A vector callback cast differs from its selected ABI type.", "array-lifetime");
            var node = value;
            var casts = 0;
            while (node is ConversionOperation { OperatorKind.Name: "eok_cast" } cast)
            {
                if (++casts > 2 || cast.Operands.Count != 1 || cast.ValueCategory.Name != "prvalue" || cast.Details.VolatileFetch ||
                    cast.Operator.EvalLeftToRight || cast.Operator.EvalRightToLeft ||
                    MemoryRepresentationType(cast.Type) is not PointerType { IsReference: false, IsRValueReference: false } pointer ||
                    cast.Type.Size != 8 || cast.Type.Alignment != 8 || pointer.PointeeType is not RoutineType routine ||
                    !routine.HasPrototype || routine.IsVariadic || routine.HasThisParameter || routine.Parameters.Count != 1 ||
                    routine.Parameters[0].Role.Name != "abi_parameter" || routine.Parameters[0].Descriptor.Details.PassedViaCopyConstructor ||
                    routine.ReturnsValueAsParameter == true || routine.ReturnsValueViaCopyConstructor ||
                    MemoryRepresentationType(routine.ReturnType).Kind.Name != "tk_void" || routine.LanguageLinkage?.Name != "nlk_cplusplus_external")
                    throw Limit("The callback adapter accepts only retained pure casts to the exact one-void-pointer ABI callback.", "array-lifetime");
                ValidateAggregateQualifiers(cast.Type);
                ValidateVectorVoidPointer(routine.Parameters[0].Type);
                ValidateCallingConvention(routine);
                node = cast.Operand;
            }
            if (casts == 0 || node is not ReferenceOperation { Target: FunctionSymbol function } reference || !function.Equals(target) ||
                reference.Operands.Count != 0 || reference.Details.VolatileFetch || reference.ValueCategory.Name != "prvalue" ||
                MemoryRepresentationType(reference.Type) is not PointerType { IsReference: false, IsRValueReference: false } address ||
                reference.Type.Size != 8 || reference.Type.Alignment != 8 || !address.PointeeType.Equals(target.Signature))
                throw Limit("A vector callback must retain the exact selected constructor/destructor reference, without address escape or effects.", "array-lifetime");
        }

        private static bool IsArrayInteger(ExpressionOperation expression, BigInteger value)
        {
            ValidateGeneratedUInt64(expression.Type);
            return expression is ConstantOperation { Constant.Value: IntegerValue integer } && integer.Value == value &&
                expression.Operands.Count == 0 && !expression.Details.VolatileFetch;
        }

        private static bool IsArrayBase(ExpressionOperation expression, VariableSymbol variable, ArrayType array)
        {
            var voidView = false;
            if (expression is ConversionOperation { OperatorKind.Name: "eok_cast" } cast)
            {
                ValidateVectorVoidPointer(cast.Type);
                if (cast.Operands.Count != 1 || cast.Details.VolatileFetch || cast.ValueCategory.Name != "prvalue") return false;
                voidView = true;
                expression = cast.Operand;
            }
            return expression is OperatorOperation { Operands.Count: 1 } address &&
                !address.Details.VolatileFetch && address.ValueCategory.Name == "prvalue" &&
                MemoryRepresentationType(address.Type) is PointerType { IsReference: false, IsRValueReference: false } pointer &&
                address.Type.Size == 8 && address.Type.Alignment == 8 && pointer.Size == 8 && pointer.Alignment == 8 &&
                (address.OperatorKind.Name == "eok_array_to_pointer" && !voidView && pointer.PointeeType.Equals(array.ElementType) ||
                    address.OperatorKind.Name == "eok_address_of" && voidView && pointer.PointeeType.Equals(array)) &&
                address.Operands[0] is ReferenceOperation { Target: VariableSymbol target } reference &&
                target.Equals(variable) && reference.Type.Equals(array) && reference.ValueCategory.Name == "lvalue" && reference.Operands.Count == 0 && !reference.Details.VolatileFetch;
        }

        private static bool HasArrayCompletionState(IReadOnlyList<SemanticNode> nodes, CallOperation call, CleanupRegion region, VariableSymbol variable, ArrayType array) =>
            nodes.OfType<BlockOperation>().Any(block => block.Statements.Zip(block.Statements.Skip(1)).Any(pair =>
                pair.First.Expression is OperatorOperation { OperatorKind.Name: "eok_comma", Operands.Count: 2 } comma &&
                comma.Operands[0].Equals(call) && IsArrayBase(comma.Operands[1], variable, array) &&
                pair.Second.Expression is CleanupStateOperation { IsUnreachable: false } state &&
                state.StateNumber == region.Ordinal && (state.Region is null || state.Region.Equals(region))));

        private static bool HasArrayDestructionState(IReadOnlyList<SemanticNode> nodes, CallOperation call, BigInteger? nullState) =>
            nodes.OfType<BlockOperation>().Any(block => block.Statements.Zip(block.Statements.Skip(1)).Any(pair =>
                pair.Second.Expression?.Equals(call) == true && pair.First.Expression is CleanupStateOperation { IsUnreachable: false } state &&
                state.StateNumber == nullState && state.Region is null));

        private bool TryPlanAutomaticArrayCall(CallOperation call, out ExpressionPlan plan)
        {
            plan = null!;
            if (!automaticArrayCalls.ContainsKey(call)) return false;
            if (automaticArray is null || !automaticArray.Function.Equals(currentFunction))
                throw Limit("The vector helper escaped its validated automatic-array occurrence.", "array-lifetime");
            // The callback operands were proven to be pure typed identities;
            // only the three actual value operands require evaluation here.
            var operands = call.Arguments.Take(3).Select(argument => PlanExpression(argument.Value)).ToArray();
            var ordered = OrderOperands(operands, false, false);
            plan = new(call.Type, call.ValueCategory, ordered.Prefix,
                new CsAutomaticArrayCall(CsType.Value(call.Type), call, ordered.Values), Effects(operands) | ExpressionEffects.Call | ExpressionEffects.MayThrow);
            return true;
        }

        private string RenderAutomaticArrayCall(CsAutomaticArrayCall call)
        {
            var plan = automaticArray ?? throw Limit("No selected automatic-array adapter exists.", "array-lifetime");
            if (!automaticArrayCalls.TryGetValue(call.Operation, out var construct) || call.Arguments.Count != 3)
                throw Limit("An automatic-array call lost its retained occurrence identity.", "array-lifetime");
            return $"{Owner}.{(construct ? plan.ConstructMethod : plan.DestroyMethod)}((void*)({RenderExpression(call.Arguments[0])}), {RenderExpression(call.Arguments[1])}, {RenderExpression(call.Arguments[2])})";
        }

        private void EmitAutomaticArraySupport()
        {
            if (automaticArray is not { } plan) return;
            var writer = new CodeWriter();
            var element = TypeName(plan.Element);
            var count = plan.Type.Bound!.Value.ToString(CultureInfo.InvariantCulture) + "UL";
            var stride = plan.Element.Size!.Value.ToString(CultureInfo.InvariantCulture) + "UL";
            writer.Line($"private static void {plan.ConstructMethod}(void* array, ulong count, ulong stride)"); writer.Open();
            writer.Line($"if (count != {count} || stride != {stride}) throw new InvalidOperationException(\"The retained automatic-array extent changed.\");");
            writer.Line("ulong completed = 0;");
            writer.Line($"{ExceptionCarrierType}? observed = null;");
            writer.Line("try"); writer.Open();
            writer.Line("while (completed < count)"); writer.Open();
            writer.Line($"{OwnedName(plan.Constructor)}(({element}*)((byte*)array + (nuint)(completed * stride)));");
            writer.Line("++completed;"); writer.Close(); writer.Close();
            writer.Line($"catch ({ExceptionCarrierType} exception) when ({ExceptionCarrierType}.ObserveUnwind(exception, ref observed))");
            writer.Open(); writer.Line("throw;"); writer.Close();
            writer.Line("finally"); writer.Open();
            writer.Line("if (observed is not null)"); writer.Open();
            writer.Line("observed.RequireSelectedHandler();");
            writer.Line("while (completed != 0)"); writer.Open(); writer.Line("--completed;");
            writer.Line($"{OwnedName(plan.Destructor)}(({element}*)((byte*)array + (nuint)(completed * stride)));");
            writer.Close(); writer.Close(); writer.Close(); writer.Close();
            writer.Line();
            writer.Line($"private static void {plan.DestroyMethod}(void* array, ulong count, ulong stride)"); writer.Open();
            writer.Line($"if (count != {count} || stride != {stride}) throw new InvalidOperationException(\"The retained automatic-array extent changed.\");");
            writer.Line("while (count != 0)"); writer.Open(); writer.Line("--count;");
            writer.Line($"{OwnedName(plan.Destructor)}(({element}*)((byte*)array + (nuint)(count * stride)));");
            writer.Close(); writer.Close();
            methods.Append(writer.TextWithIndent(1)).AppendLine();
        }
    }
}
