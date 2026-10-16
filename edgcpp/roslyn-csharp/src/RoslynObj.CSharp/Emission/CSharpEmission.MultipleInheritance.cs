using System.Text;
using RoslynObj;

namespace RoslynObj.CSharp;

public sealed partial class CSharpEmitter
{
    private sealed partial class Emission
    {
        private sealed record MultipleBase(RecordType Record, RecordType View, BaseSubobject Subobject,
            FieldSymbol Field, FieldSymbol Pointer, FieldSymbol ViewPointer, VirtualTable Table, FunctionSymbol DerivedTarget);
        private sealed record MultipleSite(CallOperation Call, MultipleBase Base, IReadOnlyList<FunctionSymbol> Targets, string Invoke);
        private sealed record MultipleVirtual(RecordType Derived, IReadOnlyList<MultipleBase> Bases, VariableSymbol Table,
            FunctionSymbol Wrapper, FunctionSymbol Overriding, ExpressionOperation Result,
            IReadOnlyList<FunctionSymbol> AddressTargets, IReadOnlyList<MultipleSite> Sites);
        private MultipleVirtual? multipleVirtual;

        private bool TryInitializeMultipleInheritance(IReadOnlyList<FunctionSymbol> selectedMethods)
        {
            if (selectedMethods.Count != 4) return false;
            RequireNormalLifetimeProfile();
            var derivedMethods = selectedMethods.Where(method => method.ContainingType?.Bases.Count == 2).ToArray();
            var roots = selectedMethods.Where(method => method.ContainingType?.Bases.Count == 0).ToArray();
            if (HostProtocolEnabled || roots.Length != 2 || derivedMethods.Length != 2 || derivedMethods[0].ContainingType is not RecordType derived ||
                derivedMethods.Any(method => !Equals(method.ContainingType, derived)) || derived.SubobjectPartner?.Equals(derived) != true ||
                derived.Bases.Count != 2 || derived.BaseSubobjects.Count != 2)
                throw Limit("The multiple-inheritance profile requires its selected three-record, two-direct-base generated graph.", "virtual-layout");
            RequireMultipleRecord(derived, 32, 32, 3);
            var derivedLayout = derived.Layout!;
            RequireVirtualIntField(derivedLayout.Fields[2], 28);
            var wrappers = functions.Where(function => function.OverridingFunctionForWrapper is not null || function.OverriddenFunctionForWrapper is not null).ToArray();
            if (wrappers.Length != 1 || wrappers[0].OverridingFunctionForWrapper is not { } overriding || !derivedMethods.Contains(overriding))
                throw Limit("This multiple-inheritance slice requires one selected actual fixed-adjustor entry.", "virtual-adjustor");
            var wrapper = wrappers[0];
            var derivedTable = derived.VirtualFunctionTableVariable ?? throw Limit("The multiple-inheritance virtual table is absent.", "virtual-table");
            var elements = RequireMultipleTable(derivedTable, 7);
            var basePlans = new List<MultipleBase>();
            for (var index = 0; index < 2; ++index)
            {
                var direct = derived.Bases[index];
                if (direct.Ordinal != index || direct.IsVirtual || direct.BaseType is not RecordType record || direct.Subobject is not { } subobject ||
                    subobject.MostDerivedType?.Equals(derived) != true || !subobject.BaseType.Equals(record) || !subobject.IsDirect || subobject.IsVirtual ||
                    subobject.IsAmbiguous || subobject.IsOptimizedEmptyBase || subobject.Offset != index * 16 || subobject.SharesVirtualFunctionInfo != (index == 0) ||
                    !derived.BaseSubobjects.Contains(subobject) || record.SubobjectPartner is not RecordType view || view.Equals(record) || view.SubobjectPartner?.Equals(record) != true)
                    throw Limit("The multiple-inheritance base must retain its distinct direct subobject, occupied partner and captured displacement.", "virtual-layout");
                RequireMultipleRecord(record, 16, 12, 2); RequireMultipleRecord(view, 12, 12, 2);
                RequireVirtualIntField(record.Layout!.Fields[1], 8); RequireVirtualIntField(view.Layout!.Fields[1], 8);
                var pointer = VirtualPointerField(record); var viewPointer = VirtualPointerField(view);
                var field = derivedLayout.Fields[index];
                if (field.Offset != subobject.Offset || field.BitWidth is not null || !field.Field.IsGeneratedBase || !field.Field.IsCompilerGenerated ||
                    !MemoryRepresentationType(field.Field.Type).Equals(view))
                    throw Limit("The multiple-inheritance occupied base field does not match its retained contextual base.", "virtual-layout");
                var baseMethods = roots.Where(method => Equals(method.ContainingType, record)).ToArray();
                if (baseMethods.Length != 1 || elements[index + 2].Value is not AddressValue { BaseEntity: FunctionSymbol derivedTarget } address ||
                    !IsDirectZeroAddress(address) || address.BaseKind.Name != "abk_routine" || !derivedMethods.Contains(derivedTarget))
                    throw Limit("The actual primary table slot must name one selected overriding definition.", "virtual-table");
                ValidateMultipleMethod(derivedTarget, derived, true, index);
                var relations = derivedTarget.Overrides;
                if (relations.Count != 1 || !relations[0].Overridden.Equals(baseMethods[0]) || relations[0].Subobject?.Equals(subobject) != true ||
                    relations[0].OverridingSubobject is not null || relations[0].ReturnAdjustmentSubobject is not null)
                    throw Limit("The table-selected method does not retain the matching base/overridden association.", "virtual-call");
                var baseTable = ValidateVirtualTable(record, baseMethods[0], pointer);
                basePlans.Add(new(record, view, subobject, field.Field, pointer, viewPointer, baseTable, derivedTarget));
            }
            if (basePlans[0].Record.Equals(basePlans[1].Record) || basePlans[0].View.Equals(basePlans[1].View) ||
                derived.PrimaryBaseSubobject?.Equals(basePlans[0].Subobject) != true ||
                derived.VirtualFunctionInfoBaseSubobject?.Equals(basePlans[0].Subobject) != true ||
                !overriding.Equals(basePlans[1].DerivedTarget) || wrapper.OverriddenFunctionForWrapper?.Equals(basePlans[1].Table.Target) != true)
                throw Limit("The selected primary/secondary bases and adjustor relations differ from the bounded graph.", "virtual-adjustor");
            ValidateMultipleMethod(wrapper, derived, false, null);
            var marker = ValidateMultipleAdjustorBody(wrapper, overriding, basePlans[1]);
            if (elements[0].Value is not IntegerValue { Value.IsZero: true } ||
                !IsMultipleTypeinfo(elements[1].Value, derived) || elements[4].Value is not IntegerValue secondaryOffset || secondaryOffset.Value != -16 ||
                !IsMultipleTypeinfo(elements[5].Value, derived) ||
                elements[6].Value is not AddressValue { BaseEntity: FunctionSymbol secondaryTarget } secondaryAddress ||
                secondaryAddress.BaseKind.Name != "abk_routine" || !IsDirectZeroAddress(secondaryAddress) || !secondaryTarget.Equals(wrapper))
                throw Limit("The captured primary/secondary virtual prefixes and exact adjustor slot must be preserved.", "virtual-table");
            var graph = functions.ToDictionary(function => function, function => ExceptionExecutionNodes(function.Body!.Root).ToArray());
            if (graph.Values.SelectMany(nodes => nodes).Any(node => node is ThrowOperation or TryOperation or AllocationOperation))
                throw Limit("This fixed-adjustor profile has no throwing, allocation or native-unwind execution.", "virtual-dispatch");
            var indirect = graph.Values.SelectMany(nodes => nodes.OfType<CallOperation>()).Where(call => call.Callee is not ReferenceOperation { Target: FunctionSymbol }).ToArray();
            if (indirect.Length != 2) throw Limit("The multiple-inheritance profile requires its two actual lowered indirect sites.", "virtual-call");
            var sites = basePlans.Select(basePlan =>
            {
                var calls = indirect.Where(call => Equals(call.Call.FunctionType, basePlan.Table.Target.Signature)).ToArray();
                if (calls.Length != 1) throw Limit("Each retained base signature must identify one actual lowered indirect site.", "virtual-call");
                ValidateLoweredVirtualCall(calls[0], basePlan.Table.Target, basePlan.Pointer);
                FunctionSymbol[] targets = [basePlan.Table.Target, basePlan.Subobject.Offset == 0 ? basePlan.DerivedTarget : wrapper];
                return new MultipleSite(calls[0], basePlan, targets, InternalName("__InvokeVirtual"));
            }).ToArray();
            FunctionSymbol[] addressTargets = [basePlans[0].Table.Target, basePlans[1].Table.Target, basePlans[0].DerivedTarget, basePlans[1].DerivedTarget, wrapper];
            multipleVirtual = new(derived, basePlans, derivedTable, wrapper, overriding, marker, addressTargets, sites);
            ValidateMultipleViews(graph);
            foreach (var primary in functions.Where(function => function.PrimaryEntry is null && IsMultipleCompleteRecord(function.ContainingType) &&
                function.SpecialFunctionKind?.Name is "sfk_constructor" or "sfk_destructor"))
            {
                if (primary.ConstructorOrDestructorKind?.Name != "cdk_complete" || primary.Signature.ExceptionSpecification?.NoexceptEffective != true ||
                    primary.AlternateEntryPoints.Count != 1 || !functions.Contains(primary.AlternateEntryPoints[0]))
                    throw Limit("The multiple-inheritance lifetime entry needs its distinct selected complete/subobject definitions.", "virtual-lifetime");
                var alternate = primary.AlternateEntryPoints[0];
                if (alternate.PrimaryEntry?.Equals(primary) != true || alternate.ConstructorOrDestructorKind?.Name != "cdk_subobject" ||
                    alternate.SpecialFunctionKind != primary.SpecialFunctionKind || alternate.ContainingType?.Equals(primary.ContainingType) != true)
                    throw Limit("The multiple-inheritance alternate entry lacks its reciprocal selected primary relation.", "virtual-lifetime");
                virtualAlternates.Add(alternate);
            }
            if (virtualAlternates.Count != 6) throw Limit("The multiple-inheritance graph requires its six actual lifetime forwarding entries.", "virtual-lifetime");
            foreach (var alternate in virtualAlternates) ValidateGeneratedAlternateBody(alternate);
            foreach (var target in addressTargets) EnsureGeneratedNoThrow(target);
            foreach (var function in functions.Where(function => Equals(function.ContainingType, derived) && function.PrimaryEntry is null &&
                function.SpecialFunctionKind?.Name is "sfk_constructor" or "sfk_destructor")) ValidateMultipleBaseCleanups(function, graph[function]);
            return true;
        }

        private static void RequireMultipleRecord(RecordType record, int size, int occupied, int fields)
        {
            ValidateAggregateQualifiers(record);
            if (record.Kind.Name is not ("tk_struct" or "tk_class") || record.Size != size || record.Alignment != 8 || record.IsIncomplete || record.IsPacked ||
                record.HasVirtualBases || !record.HasVirtualFunctions || record.HasPureVirtualFunctions || record.VirtualFunctionInfoOffset != 0 ||
                record.Layout is not { IsComplete: true } layout || layout.NonvirtualSize != occupied || layout.Fields.Count != fields ||
                size != 12 && layout.NonvirtualAlignment != 8)
                throw Limit("The multiple-inheritance record has no exercised ordinary complete/occupied layout representation.", "virtual-layout");
        }

        private static IReadOnlyList<AggregateElementDetails> RequireMultipleTable(VariableSymbol variable, int count)
        {
            if (variable.Type is not ArrayType array || array.Bound != count || array.Size != count * 8 || array.Alignment != 8 ||
                array.ElementType is not QualifiedType { IsConst: true, IsVolatile: false } word ||
                variable.InitializationKind.Name != "initk_static" || variable.Initializer is not null ||
                variable.ConstantInitializer?.Value is not AggregateValue aggregate || !aggregate.Details.Type.Equals(array))
                throw Limit("The multiple-inheritance table requires its complete captured constant-word array.", "virtual-table");
            ValidateVirtualWord(word.UnderlyingType);
            var elements = aggregate.Elements;
            if (elements.Count != count || elements.Where((element, index) => element.Ordinal != index || element.Type is null ||
                !MemoryRepresentationType(element.Type).Equals(word.UnderlyingType)).Any())
                throw Limit("The multiple-inheritance virtual table element order/types differ from the retained word array.", "virtual-table");
            return elements;
        }

        private static bool IsMultipleTypeinfo(ConstantValue value, RecordType record) =>
            value is AddressValue { BaseEntity: VariableSymbol typeinfo } address && address.BaseKind.Name == "abk_variable" &&
            IsDirectZeroAddress(address) && record.Details.TypeinfoVariable?.Equals(typeinfo) == true;

        private void ValidateMultipleMethod(FunctionSymbol function, RecordType record, bool isVirtual, int? number)
        {
            var signature = function.Signature;
            if (!functions.Contains(function) || function.Body is null || function.IsVirtual != isVirtual || function.IsPureVirtual ||
                function.PrimaryEntry is not null || function.AlternateEntryPoints.Count != 0 || function.SpecialFunctionKind?.Name != "sfk_none" ||
                function.Details.IsWeak == true || function.Details.IsWeakref == true || function.Details.IsGnuAlias == true || function.Details.IsIfunc == true || function.Details.IsAliasEntry == true ||
                function.ReturnAdjustment != 0 || function.VirtualCallIndex != 0 || function.VirtualBaseIndex != 0 ||
                (number is { } slot ? function.VirtualFunctionNumber != slot || function.ThisAdjustment != 0 : function.VirtualFunctionNumber is not null) ||
                !signature.HasPrototype || signature.IsVariadic || !signature.HasThisParameter || signature.ReturnsValueAsParameter == true || signature.ReturnsValueViaCopyConstructor ||
                signature.ExceptionSpecification?.NoexceptEffective != true || signature.Parameters.Count != 2 || function.Parameters.Count != 2 ||
                signature.Parameters[0].Role.Name != "this_parameter" || signature.Parameters[1].Role.Name != "ordinary_parameter" ||
                signature.ThisClass?.Equals(record) != true || function.ContainingType?.Equals(record) != true || !GeneratedRecordPointer(signature.Parameters[0].Type, false).Equals(record))
                throw Limit("Only the selected noexcept scalar receiver method/fixed-adjustor signature has this generated address contract.", "virtual-adjustor");
            ValidateCallingConvention(signature); ValidateGeneratedInt32(signature.ReturnType); ValidateGeneratedInt32(signature.Parameters[1].Type);
            for (var index = 0; index < 2; ++index)
            {
                var parameter = function.Parameters[index]; var selected = signature.Parameters[index];
                if (parameter.Ordinal != index || selected.Ordinal != index || parameter.Symbol is null || parameter.BaseSubobject is not null ||
                    selected.BaseSubobject is not null || parameter.Descriptor.Details.PassedViaCopyConstructor ||
                    !parameter.Descriptor.Equals(selected.Descriptor) || !parameter.Role.Equals(selected.Role))
                    throw Limit("The fixed-adjustor execution parameters do not match the actual selected signature.", "virtual-adjustor");
            }
        }

        private static ExpressionOperation ValidateMultipleAdjustorBody(FunctionSymbol wrapper, FunctionSymbol overriding, MultipleBase secondary)
        {
            var body = wrapper.Body!; var receiver = wrapper.Parameters[0].Symbol!;
            if (wrapper.ThisAdjustment != -secondary.Subobject.Offset || wrapper.ThisAdjustment != -16 ||
                !wrapper.ReturnType.Equals(overriding.ReturnType) || wrapper.Parameters.Where((parameter, index) => !parameter.Type.Equals(overriding.Parameters[index].Type)).Any() ||
                body.ExceptionHandling.RegionTable is not null || body.ExceptionHandling.ArrayTable is not null || body.ExceptionHandling.CleanupRegions.Count != 0 ||
                body.Lifetimes.Count != 0 || body.Locals.Count != 1 || body.Root is not BlockOperation { IsCompilerGenerated: true } block || block.Statements.Count != 3 ||
                block.Statements[0].Expression is not OperatorOperation { OperatorKind.Name: "eok_assign" } assignment || assignment.Operands.Count != 2 ||
                assignment.Operands[0] is not ReferenceOperation { Target: VariableSymbol storedReceiver } || !storedReceiver.Equals(receiver) ||
                assignment.Operands[1] is not ConversionOperation { OperatorKind.Name: "eok_cast" } cast || !cast.Type.Equals(wrapper.Parameters[0].Type) ||
                cast.Operand is not OperatorOperation { OperatorKind.Name: "eok_padd" } add || add.Operands.Count != 2 || add.Operator.PointerOperandIsSecond ||
                add.Operands[0] is not ConversionOperation { OperatorKind.Name: "eok_cast" } bytes ||
                MemoryRepresentationType(bytes.Type) is not PointerType { IsReference: false, IsRValueReference: false } pointer || pointer.PointeeType.Size != 1 || pointer.PointeeType.Alignment != 1 ||
                bytes.Operand is not ReferenceOperation { Target: VariableSymbol loadedReceiver } || !loadedReceiver.Equals(receiver) ||
                add.Operands[1] is not ConstantOperation { Constant.Value: IntegerValue delta } displacement || delta.Value != wrapper.ThisAdjustment ||
                block.Statements[1].Expression is not OperatorOperation { OperatorKind.Name: "eok_assign" } resultAssignment || resultAssignment.Operands.Count != 2 ||
                resultAssignment.Operands[0] is not ReferenceOperation { Target: VariableSymbol temporary } || !temporary.Equals(body.Locals[0]) ||
                resultAssignment.Operands[1] is not { Kind.Name: "enk_result_of_overriding_function" } result || result.Operands.Count != 0 ||
                result.ValueCategory.Name != "prvalue" || result.RepresentationRole.Name != "execution" || result.Details.VolatileFetch || !result.Type.Equals(overriding.ReturnType) ||
                block.Statements[2] is not JumpOperation { JumpKind.Name: "return", Value: ReferenceOperation { Target: VariableSymbol returned } } || !returned.Equals(temporary))
                throw Limit("The selected fixed adjustor must retain its actual one receiver byte-adjustment, one overriding-result occurrence and temporary return.", "virtual-adjustor");
            ValidateVirtualWord(displacement.Type);
            ValidateAggregateQualifiers(bytes.Type);
            if (bytes.Type.Size != 8 || bytes.Type.Alignment != 8 || !add.Type.Equals(bytes.Type) || cast.Operator.RequiresRuntimeCastCheck || bytes.Operator.RequiresRuntimeCastCheck ||
                !assignment.Type.Equals(wrapper.Parameters[0].Type) || !resultAssignment.Type.Equals(result.Type) || !temporary.Type.Equals(result.Type))
                throw Limit("The actual adjustor arithmetic/result occurrence changes its retained target representation.", "virtual-adjustor");
            return result;
        }

        private bool IsMultipleCompleteRecord(CppType? type) => multipleVirtual is { } plan && type is not null &&
            (type.Equals(plan.Derived) || plan.Bases.Any(basePlan => type.Equals(basePlan.Record)));

        private void ValidateMultipleViews(IReadOnlyDictionary<FunctionSymbol, SemanticNode[]> graph)
        {
            var plan = multipleVirtual!;
            bool IsView(CppType type) => plan.Bases.Any(basePlan => MemoryRepresentationType(type).Equals(basePlan.View));
            bool IsViewPointer(CppType type) => MemoryRepresentationType(type) is PointerType pointer && IsView(pointer.PointeeType);
            bool IsClass(CppType type) => IsView(type) || IsMultipleCompleteRecord(MemoryRepresentationType(type));
            if (inputs[0].TranslationUnit.Variables.Any(variable => IsView(variable.Type) || IsViewPointer(variable.Type)) ||
                functions.Any(function => IsClass(function.ReturnType) || IsViewPointer(function.ReturnType) ||
                    function.Parameters.Any(parameter => IsClass(parameter.Type) || IsViewPointer(parameter.Type))))
                throw Limit("The occupied base views have no standalone/pointer-storage contract, and these classes have no by-value call contract.", "virtual-layout");
            foreach (var node in graph.Values.SelectMany(nodes => nodes))
            {
                if (node is ExpressionOperation expression && IsView(expression.Type) &&
                    !(expression is ReferenceOperation { Target: FieldSymbol field } && plan.Bases.Any(basePlan => field.Equals(basePlan.Field))) &&
                    !(expression is OperatorOperation { OperatorKind.Name: "eok_points_to_field" or "eok_dot_field" } member && member.ValueCategory.Name == "lvalue" &&
                        member.Operands.Count == 2 && member.Operands[1] is ReferenceOperation { Target: FieldSymbol selected } && plan.Bases.Any(basePlan => selected.Equals(basePlan.Field))))
                    throw Limit("An occupied base view escaped its retained generated-field lvalue path.", "virtual-layout");
                if (node is ConversionOperation { OperatorKind.Name: "eok_lvalue_adjust" } conversion && !conversion.SourceType.Equals(conversion.TargetType))
                {
                    var primary = conversion.SourceType.Equals(plan.Derived) && conversion.TargetType.Equals(plan.Bases[0].Record);
                    var secondary = conversion.SourceType.Equals(plan.Bases[1].View) && conversion.TargetType.Equals(plan.Bases[1].Record) &&
                        conversion.Operand is OperatorOperation { OperatorKind.Name: "eok_points_to_field" or "eok_dot_field" } adjustedMember &&
                        adjustedMember.Operands.Count == 2 && adjustedMember.Operands[1] is ReferenceOperation { Target: FieldSymbol adjustedField } && adjustedField.Equals(plan.Bases[1].Field);
                    if ((!primary && !secondary) || conversion.ValueCategory.Name != "lvalue" || conversion.Operand.ValueCategory.Name != "lvalue" ||
                        conversion.Operator.RequiresRuntimeCastCheck || conversion.Operands.Count != 1)
                        throw Limit("Only the retained primary lvalue and already-addressed secondary-field views are admitted.", "virtual-layout");
                    virtualBaseViews.Add(conversion);
                }
                if (node is OperatorOperation operation && (operation.OperatorKind.Name is "eok_bassign" or "eok_class_rvalue_adjust" ||
                    operation.OperatorKind.Name == "eok_assign" && operation.Operands.Count != 0 && IsClass(operation.Operands[0].Type)))
                    throw Limit("This multiple-inheritance profile has no whole-object copy or class-rvalue adaptation contract.", "virtual-layout");
                if (node is ExpressionOperation { Kind.Name: "enk_result_of_overriding_function" } placeholder && !placeholder.Equals(plan.Result))
                    throw Limit("An overriding-result occurrence has no exact selected wrapper context.", "virtual-adjustor");
            }
        }

        private void ValidateMultipleBaseCleanups(FunctionSymbol function, IReadOnlyList<SemanticNode> nodes)
        {
            var plan = multipleVirtual!; var body = function.Body!; var regions = body.ExceptionHandling.CleanupRegions;
            var recipes = body.Lifetimes.SelectMany(lifetime => lifetime.Cleanups).Select(cleanup => cleanup.Initialization).ToArray();
            if (regions.Count != 2 || recipes.Length != 2 || function.Parameters.Count == 0 || function.Parameters[0].Symbol is not { } receiver)
                throw Limit("The multiple-inheritance lifetime body requires its two retained direct-base region/recipe associations.", "virtual-lifetime");
            for (var index = 0; index < 2; ++index)
            {
                var selectedBase = plan.Bases[index]; var region = regions[index];
                if (region.Ordinal != index || (index == 0 ? region.NextRegion is not null : region.NextRegion?.Equals(regions[0]) != true) || region.Actions.Count != 1 ||
                    region.Actions[0] is not { TargetMethod: { } destructor, Storage: { } storage } action ||
                    !action.Details.IsBaseSubobject || storage.Kind.Name != "receiver_relative" || !storage.UsesThisParameterOffset || storage.IsIndirect ||
                    storage.Offset != selectedBase.Subobject.Offset || storage.Variable is not null || storage.Details.BaseEntity is not null ||
                    !virtualAlternates.Contains(destructor) || destructor.SpecialFunctionKind?.Name != "sfk_destructor" ||
                    destructor.PrimaryEntry is not { } primary || !Equals(primary.ContainingType, selectedBase.Record))
                    throw Limit("Each base cleanup must select its actual this-relative displacement, entry and next-region link.", "virtual-lifetime");
                var matches = recipes.Where(recipe => recipe.Destructor?.Equals(primary) == true && recipe.Target is null && recipe.Details.Type is null &&
                    recipe.Lifetime?.BoundEntity?.Equals(body.Scope) == true).ToArray();
                if (matches.Length != 1) throw Limit("The selected subobject destructor needs its exact complete-destructor recipe association.", "virtual-lifetime");
                var recipe = matches[0]; ValidateExceptionMetadataRecipe(recipe, false, true);
                bool IsReceiver(ExpressionOperation value) => index == 0
                    ? value is ConversionOperation { OperatorKind.Name: "eok_cast" } cast && GeneratedRecordPointer(cast.Type, false).Equals(selectedBase.Record) &&
                        cast.Operand is ReferenceOperation { Target: VariableSymbol primaryReceiver } && primaryReceiver.Equals(receiver)
                    : value is OperatorOperation { OperatorKind.Name: "eok_address_of" } address && address.Operands.Count == 1 &&
                        address.Operands[0] is ConversionOperation conversion && virtualBaseViews.Contains(conversion) &&
                        conversion.TargetType.Equals(selectedBase.Record) && conversion.Operand is OperatorOperation member && member.Operands.Count == 2 &&
                        member.Operands[1] is ReferenceOperation { Target: FieldSymbol selectedField } && selectedField.Equals(selectedBase.Field) &&
                        member.Operands[0] is ReferenceOperation { Target: VariableSymbol secondaryReceiver } && secondaryReceiver.Equals(receiver);
                var calls = nodes.OfType<CallOperation>().Where(call => call.Receiver is { } value && IsReceiver(value)).ToArray();
                if (calls.Length != 1) throw Limit("Each base must have one actual lowered selected receiver call.", "virtual-lifetime");
                if (function.SpecialFunctionKind?.Name == "sfk_constructor")
                {
                    if (body.Scope is null || body.Scope.ConstructorInitializers.Count != 0 || calls[0].TargetMethod is not { } constructor ||
                        !virtualAlternates.Contains(constructor) || constructor.SpecialFunctionKind?.Name != "sfk_constructor" ||
                        !Equals(constructor.PrimaryEntry?.ContainingType, selectedBase.Record) || !HasImmediateCompletionState(nodes, calls[0], region))
                        throw Limit("A selected base constructor must complete before its actual retained cleanup state is armed.", "virtual-lifetime");
                }
                else if (function.SpecialFunctionKind?.Name != "sfk_destructor" || calls[0].TargetMethod?.Equals(destructor) != true ||
                    body.Scope?.ConstructorInitializers.Count(initializer => initializer.Kind.Name == "cik_direct_base_class" &&
                        initializer.BaseSubobject?.Equals(selectedBase.Subobject) == true && initializer.Initialization?.Equals(recipe) == true) != 1)
                    throw Limit("Normal base destruction must retain its actual call and exact direct-base recipe association.", "virtual-lifetime");
                var expectedNext = index == 0 ? null : recipes.Single(candidate => candidate.Destructor?.ContainingType?.Equals(plan.Bases[0].Record) == true);
                if (!Equals(recipe.NextInDestructionList, expectedNext))
                    throw Limit("The base recipe destruction link differs from the retained base cleanup chain.", "virtual-lifetime");
                virtualBaseCleanups.Add(action, new(function, recipe));
            }
        }

        private bool IsOverridingResult(ExpressionOperation expression) => multipleVirtual?.Result.Equals(expression) == true;
        private bool IsMultipleTableAddress(AddressValue address) => multipleVirtual is { } plan && address.BaseKind.Name == "abk_variable" &&
            !address.OnePastEnd && address.SubobjectPath is null && !address.Details.IsObjectReflection && address.Details.ParameterNumber is null &&
            (address.BaseEntity?.Equals(plan.Table) == true && (address.ByteOffset == 16 || address.ByteOffset == 48) ||
                address.ByteOffset == 16 && plan.Bases.Any(basePlan => address.BaseEntity?.Equals(basePlan.Table.Variable) == true));
        private bool TryPlanOverridingResult(ExpressionOperation expression, out ExpressionPlan plan)
        {
            plan = null!;
            if (!IsOverridingResult(expression)) return false;
            if (currentFunction?.Equals(multipleVirtual!.Wrapper) != true)
                throw Limit("The retained overriding result is outside its exact selected wrapper body.", "virtual-adjustor");
            plan = new(expression.Type, expression.ValueCategory, [], new CsOverridingResult(CsType.Value(expression.Type), expression),
                ExpressionEffects.Call | ExpressionEffects.MayThrow);
            return true;
        }

        private string RenderOverridingResult(CsOverridingResult expression)
        {
            if (multipleVirtual is not { } plan || !expression.Operation.Equals(plan.Result) || currentFunction?.Equals(plan.Wrapper) != true)
                throw Limit("The overriding-result renderer requires its exact selected wrapper occurrence.", "virtual-adjustor");
            // The stock result-of-overriding-function contract uses this entry's
            // current parameter variables in order, after its retained adjustment.
            return $"{OwnedName(plan.Overriding)}({string.Join(", ", plan.Wrapper.Parameters.Select(parameter => names[parameter.Symbol!]))})";
        }

        private bool TryEmitMultipleRecord(RecordType record, StringBuilder output)
        {
            if (multipleVirtual is not { } plan || !record.Equals(plan.Derived) && !plan.Bases.Any(basePlan => record.Equals(basePlan.Record) || record.Equals(basePlan.View))) return false;
            var view = plan.Bases.Any(basePlan => record.Equals(basePlan.View));
            output.Append("[StructLayout(LayoutKind.Explicit, Size = ").Append(StorageSize(record)).Append(", Pack = ").Append(view ? 1 : 8).AppendLine(")]");
            output.Append("public unsafe struct ").Append(TypeName(record)).AppendLine("\n{");
            if (view) output.AppendLine("    // Borrowed occupied-base view. Its owner supplies the captured 8-byte alignment.");
            if (record.Equals(plan.Derived)) output.AppendLine("    [FieldOffset(0)] public nint __layout_alignment;");
            foreach (var field in record.Layout!.Fields)
                output.Append("    [FieldOffset(").Append(field.Offset).Append(")] public ").Append(TypeName(field.Field.Type)).Append(' ').Append(FieldName(field.Field)).AppendLine(";");
            output.AppendLine("}").AppendLine(); return true;
        }

        private void EmitMultipleDispatchSupport()
        {
            if (multipleVirtual is not { } plan) return;
            foreach (var target in plan.AddressTargets) _ = FunctionAddress(target);
            foreach (var site in plan.Sites)
            {
                methods.Append("    private static int ").Append(site.Invoke).Append("(nint address, ").Append(TypeName(site.Base.Record)).AppendLine("* receiver, int argument)")
                    .AppendLine("    {");
                foreach (var target in site.Targets)
                    methods.Append("        if (address == ").Append(FunctionAddress(target)).Append(") return ").Append(OwnedName(target)).Append("((")
                        .Append(TypeName(target.Parameters[0].Type)).AppendLine(")receiver, argument);");
                methods.AppendLine("        throw new InvalidOperationException(\"Virtual address is outside the selected generated table closure.\");").AppendLine("    }").AppendLine();
            }
        }
    }
}
