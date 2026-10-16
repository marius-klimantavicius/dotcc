using System.Numerics;
using System.Text;
using RoslynObj;

namespace RoslynObj.CSharp;

public sealed partial class CSharpEmitter
{
    private sealed partial class Emission
    {
        private sealed record VirtualTable(RecordType Record, VariableSymbol Variable, VariableSymbol Typeinfo,
            FunctionSymbol Target, FieldSymbol PointerField);
        private sealed record VirtualDispatch(RecordType Base, RecordType Derived, RecordType View,
            BaseSubobject BaseSubobject, FieldSymbol BaseField, FieldSymbol ViewPointer,
            IReadOnlyList<VirtualTable> Tables, CallOperation Call, string Invoke);
        private sealed record VirtualBaseCleanup(FunctionSymbol Function, Initialization Recipe);
        private VirtualDispatch? virtualDispatch;
        private readonly HashSet<FunctionSymbol> virtualAlternates = [];
        private readonly HashSet<ConversionOperation> virtualBaseViews = [];
        private readonly Dictionary<CleanupAction, VirtualBaseCleanup> virtualBaseCleanups = [];

        private void InitializeVirtualDispatch()
        {
            var methods = functions.Where(function => function.IsVirtual).ToArray();
            if (methods.Length == 0) return;
            if (TryInitializeMultipleInheritance(methods)) return;
            RequireNormalLifetimeProfile();
            if (methods.Length != 2 || HostProtocolEnabled || methods.Any(function => function.ContainingType is not RecordType))
                throw Limit("The bounded virtual projection requires two selected scalar methods in one generated-only record pair.", "virtual-dispatch");
            var roots = methods.Where(function => function.ContainingType!.Bases.Count == 0).ToArray();
            var extensions = methods.Where(function => function.ContainingType!.Bases.Count == 1).ToArray();
            if (roots.Length != 1 || extensions.Length != 1 || roots[0].ContainingType is not RecordType baseRecord ||
                extensions[0].ContainingType is not RecordType derived || baseRecord.SubobjectPartner is not RecordType view ||
                view.Equals(baseRecord) || view.SubobjectPartner?.Equals(baseRecord) != true)
                throw Limit("The selected virtual records need one explicit complete/subobject partner and one direct base.", "virtual-layout");
            var direct = derived.Bases[0];
            if (!direct.BaseType.Equals(baseRecord) || direct.IsVirtual || direct.Subobject is not { } subobject ||
                !subobject.BaseType.Equals(baseRecord) || subobject.MostDerivedType?.Equals(derived) != true || subobject.Offset != 0 ||
                !subobject.IsDirect || subobject.IsVirtual || subobject.IsAmbiguous || subobject.IsOptimizedEmptyBase ||
                !subobject.SharesVirtualFunctionInfo || derived.BaseSubobjects.Count != 1 || !derived.BaseSubobjects[0].Equals(subobject) ||
                derived.PrimaryBaseSubobject?.Equals(subobject) != true || derived.VirtualFunctionInfoBaseSubobject?.Equals(subobject) != true)
                throw Limit("This virtual projection requires the retained single ordinary zero-offset primary base.", "virtual-layout");
            foreach (var record in new[] { baseRecord, derived, view })
            {
                ValidateAggregateQualifiers(record);
                if (record.Kind.Name is not ("tk_struct" or "tk_class") || record.IsIncomplete || record.IsPacked || record.HasVirtualBases ||
                    !record.HasVirtualFunctions || record.HasPureVirtualFunctions || record.Layout is not { IsComplete: true } ||
                    record.Alignment != 8 || record.VirtualFunctionInfoOffset != 0)
                    throw Limit("The virtual record requires its exact complete ordinary captured layout and zero vptr displacement.", "virtual-layout");
            }
            var baseLayout = baseRecord.Layout ?? throw Limit("The complete base layout is absent.", "virtual-layout");
            var viewLayout = view.Layout ?? throw Limit("The occupied base layout is absent.", "virtual-layout");
            var derivedLayout = derived.Layout ?? throw Limit("The complete derived layout is absent.", "virtual-layout");
            if (baseRecord.Size != 16 || baseLayout.NonvirtualSize != 12 || baseLayout.NonvirtualAlignment != 8 ||
                view.Size != 12 || viewLayout.NonvirtualSize != 12 || derived.Size != 16 || derivedLayout.NonvirtualSize != 16 ||
                derivedLayout.NonvirtualAlignment != 8 || baseLayout.Fields.Count != 2 || viewLayout.Fields.Count != 2 || derivedLayout.Fields.Count != 2)
                throw Limit("Only the captured 16-byte complete records with a 12-byte borrowed base view are represented.", "virtual-layout");
            var basePointer = VirtualPointerField(baseRecord);
            var viewPointer = VirtualPointerField(view);
            RequireVirtualIntField(baseLayout.Fields[1], 8);
            RequireVirtualIntField(viewLayout.Fields[1], 8);
            var baseField = derivedLayout.Fields[0];
            if (baseField.Offset != 0 || baseField.BitWidth is not null || !baseField.Field.IsGeneratedBase ||
                !baseField.Field.IsCompilerGenerated || !MemoryRepresentationType(baseField.Field.Type).Equals(view))
                throw Limit("The derived base field must retain the exact occupied subobject type at offset zero.", "virtual-layout");
            RequireVirtualIntField(derivedLayout.Fields[1], 12);
            var tables = new[] { ValidateVirtualTable(baseRecord, roots[0], basePointer), ValidateVirtualTable(derived, extensions[0], viewPointer) };
            var allNodes = functions.ToDictionary(function => function, function => ExceptionExecutionNodes(function.Body!.Root).ToArray());
            if (allNodes.Values.SelectMany(nodes => nodes).Any(node => node is ThrowOperation or TryOperation or AllocationOperation))
                throw Limit("This ordinary virtual profile has no throwing, allocation or native-unwind execution.", "virtual-dispatch");
            var indirect = allNodes.Values.SelectMany(nodes => nodes.OfType<CallOperation>()).Where(call => call.Callee is not ReferenceOperation { Target: FunctionSymbol }).ToArray();
            if (indirect.Length != 1)
                throw Limit("The bounded virtual profile requires one actual lowered indirect call site.", "virtual-dispatch");
            var call = indirect[0];
            ValidateLoweredVirtualCall(call, roots[0], basePointer);
            virtualDispatch = new(baseRecord, derived, view, subobject, baseField.Field, viewPointer, tables, call, InternalName("__InvokeVirtual"));
            ValidateVirtualViewUses(allNodes);
            foreach (var primary in functions.Where(function => function.ContainingType?.Equals(baseRecord) == true || function.ContainingType?.Equals(derived) == true)
                .Where(function => function.PrimaryEntry is null && function.SpecialFunctionKind?.Name is "sfk_constructor" or "sfk_destructor"))
            {
                if (primary.ConstructorOrDestructorKind?.Name != "cdk_complete" || primary.Signature.ExceptionSpecification?.NoexceptEffective != true ||
                    primary.AlternateEntryPoints.Count != 1 || !functions.Contains(primary.AlternateEntryPoints[0]))
                    throw Limit("Virtual lifetime entries require their selected complete noexcept body and distinct defined forwarding entry.", "virtual-lifetime");
                var alternate = primary.AlternateEntryPoints[0];
                if (alternate.PrimaryEntry?.Equals(primary) != true || alternate.ConstructorOrDestructorKind?.Name != "cdk_subobject" ||
                    alternate.SpecialFunctionKind != primary.SpecialFunctionKind || alternate.ContainingType?.Equals(primary.ContainingType) != true)
                    throw Limit("The retained virtual lifetime alternate does not name its exact selected primary.", "virtual-lifetime");
                virtualAlternates.Add(alternate);
            }
            if (virtualAlternates.Count != 4)
                throw Limit("This virtual lifetime projection requires the four actual complete/subobject entry pairs.", "virtual-lifetime");
            foreach (var alternate in virtualAlternates) ValidateGeneratedAlternateBody(alternate);
            foreach (var table in tables) EnsureGeneratedNoThrow(table.Target);
            foreach (var function in functions.Where(function => function.ContainingType?.Equals(derived) == true && function.PrimaryEntry is null &&
                function.SpecialFunctionKind?.Name is "sfk_constructor" or "sfk_destructor")) ValidateVirtualBaseCleanup(function, allNodes[function]);
        }

        private static FieldSymbol VirtualPointerField(RecordType record)
        {
            var field = record.Layout!.Fields[0];
            if (field.Offset != 0 || field.BitWidth is not null || field.Field.IsGeneratedBase || !field.Field.IsCompilerGenerated ||
                MemoryRepresentationType(field.Field.Type) is not PointerType { IsReference: false, IsRValueReference: false } pointer ||
                pointer.Size != 8 || pointer.Alignment != 8 || field.Field.Type.Size != 8 || field.Field.Type.Alignment != 8 ||
                pointer.PointeeType is not QualifiedType { IsConst: true, IsVolatile: false } qualified)
                throw Limit("The retained vptr path requires a generated natural pointer field at offset zero.", "virtual-layout");
            ValidateAggregateQualifiers(field.Field.Type); ValidateAggregateQualifiers(pointer.PointeeType);
            ValidateVirtualWord(qualified.UnderlyingType);
            return field.Field;
        }

        private static void ValidateVirtualWord(CppType type)
        {
            ValidateAggregateQualifiers(type);
            if (MemoryRepresentationType(type) is not ArithmeticType { Arithmetic.Bits: 64, Arithmetic.Signed: true } word ||
                type.Size != 8 || type.Alignment != 8 || word.Size != 8 || word.Alignment != 8)
                throw Limit("The captured virtual table entry requires the natural signed 64-bit target word.", "virtual-layout");
        }

        private static void RequireVirtualIntField(FieldLayout field, BigInteger offset)
        {
            if (field.Offset != offset || field.BitWidth is not null || field.Field.IsGeneratedBase || field.Field.IsBitField)
                throw Limit("The virtual record scalar field differs from its retained ordinary layout.", "virtual-layout");
            ValidateGeneratedInt32(field.Field.Type);
        }

        private VirtualTable ValidateVirtualTable(RecordType record, FunctionSymbol method, FieldSymbol pointerField)
        {
            var signature = method.Signature;
            if (!functions.Contains(method) || method.Body is null || !method.IsVirtual || method.IsPureVirtual || method.PrimaryEntry is not null ||
                method.AlternateEntryPoints.Count != 0 || method.SpecialFunctionKind?.Name != "sfk_none" ||
                method.ThisAdjustment != 0 || method.ReturnAdjustment != 0 || method.VirtualCallIndex != 0 || method.VirtualBaseIndex != 0 || method.VirtualFunctionNumber != 0 ||
                method.Details.IsWeak == true || method.Details.IsWeakref == true || method.Details.IsGnuAlias == true || method.Details.IsIfunc == true || method.Details.IsAliasEntry == true ||
                !signature.HasPrototype || signature.IsVariadic || !signature.HasThisParameter || signature.ReturnsValueAsParameter == true || signature.ReturnsValueViaCopyConstructor ||
                signature.ExceptionSpecification?.NoexceptEffective != true || signature.Parameters.Count != 2 || method.Parameters.Count != 2 ||
                signature.Parameters[0].Role.Name != "this_parameter" || signature.Parameters[1].Role.Name != "ordinary_parameter" ||
                signature.ThisClass?.Equals(record) != true || !GeneratedRecordPointer(signature.Parameters[0].Type, false).Equals(record))
                throw Limit("Only the actual zero-adjustment noexcept int method with its selected receiver and one int argument has a virtual address adapter.", "virtual-call");
            ValidateCallingConvention(signature); ValidateGeneratedInt32(signature.ReturnType); ValidateGeneratedInt32(signature.Parameters[1].Type);
            foreach (var parameter in method.Parameters)
                if (parameter.Ordinal < 0 || parameter.Ordinal >= 2 || parameter.Symbol is null || parameter.BaseSubobject is not null ||
                    parameter.Descriptor.Details.PassedViaCopyConstructor || !parameter.Descriptor.Equals(signature.Parameters[(int)parameter.Ordinal].Descriptor) ||
                    !parameter.Role.Equals(signature.Parameters[(int)parameter.Ordinal].Role))
                    throw Limit("The virtual method execution parameters differ from the actual selected routine signature.", "virtual-call");
            if (record.VirtualFunctionTableVariable is not { } variable || variable.Type is not ArrayType { Bound: { } bound } array || bound != 3 ||
                array.Size != 24 || array.Alignment != 8 || array.ElementType is not QualifiedType { IsConst: true, IsVolatile: false } word ||
                variable.InitializationKind.Name != "initk_static" || variable.Initializer is not null ||
                variable.ConstantInitializer?.Value is not AggregateValue aggregate || !aggregate.Details.Type.Equals(array))
                throw Limit("The virtual table must retain its complete three-word constant array and ordinary static initializer.", "virtual-table");
            ValidateVirtualWord(word.UnderlyingType);
            var elements = aggregate.Elements;
            if (elements.Count != 3 || elements.Where((element, index) => element.Ordinal != index || element.Type is null || !MemoryRepresentationType(element.Type).Equals(word.UnderlyingType)).Any() ||
                elements[0].Value is not IntegerValue { Value.IsZero: true } ||
                elements[1].Value is not AddressValue { BaseEntity: VariableSymbol typeinfo } typeinfoAddress ||
                typeinfoAddress.BaseKind.Name != "abk_variable" || !IsDirectZeroAddress(typeinfoAddress) || record.Details.TypeinfoVariable?.Equals(typeinfo) != true ||
                elements[2].Value is not AddressValue { BaseEntity: FunctionSymbol target } methodAddress ||
                methodAddress.BaseKind.Name != "abk_routine" || !IsDirectZeroAddress(methodAddress) || !target.Equals(method))
                throw Limit("The retained virtual table prefix and function slot do not match its exact captured identities.", "virtual-table");
            return new(record, variable, typeinfo, method, pointerField);
        }

        private static bool IsDirectZeroAddress(AddressValue address) => address.ByteOffset == 0 && !address.OnePastEnd &&
            address.SubobjectPath is null && !address.Details.IsObjectReflection && address.Details.ParameterNumber is null;

        private static void ValidateLoweredVirtualCall(CallOperation call, FunctionSymbol method, FieldSymbol pointerField)
        {
            var signature = method.Signature;
            if (call.CallKind.Name != "indirect" || call.TargetMethod is not null || call.SourceTargetMethod is not null || call.SourceDispatchKind is not null ||
                call.Operator.RetainedVirtualCall || !call.Operator.EvalLeftToRight || call.Operator.EvalRightToLeft ||
                call.Call.ReturnSlot is not null || !Equals(call.Call.FunctionType, signature) || !call.Type.Equals(method.ReturnType) || call.ValueCategory.Name != "prvalue" ||
                call.Arguments.Count != 2 || call.Operands.Count != 3 || !call.Operands[0].Equals(call.Callee) ||
                call.Receiver is not ReferenceOperation { Target: VariableSymbol receiver } receiverExpression ||
                call.Callee is not OperatorOperation { OperatorKind.Name: "eok_indirect" } load || load.Operands.Count != 1 ||
                MemoryRepresentationType(load.Type) is not PointerType functionPointer || !functionPointer.PointeeType.Equals(signature) ||
                load.Operands[0] is not ConversionOperation { OperatorKind.Name: "eok_cast" } cast ||
                MemoryRepresentationType(cast.Type) is not PointerType slotPointer || !slotPointer.PointeeType.Equals(load.Type) ||
                cast.Operand is not OperatorOperation { OperatorKind.Name: "eok_padd" } add || add.Operands.Count != 2 ||
                add.Operator.PointerOperandIsSecond || add.Operands[1] is not ConstantOperation { Constant.Value: IntegerValue { Value.IsZero: true } } ||
                add.Operands[0] is not OperatorOperation { OperatorKind.Name: "eok_points_to_field" } field || field.Operands.Count != 2 ||
                field.Operands[0] is not ReferenceOperation { Target: VariableSymbol actualReceiver } || !actualReceiver.Equals(receiver) ||
                field.Operands[1] is not ReferenceOperation { Target: FieldSymbol actualField } || !actualField.Equals(pointerField) ||
                !receiverExpression.Type.Equals(signature.Parameters[0].Type))
                throw Limit("The virtual site must retain the actual pointer-field load, zero slot displacement and explicit receiver operand.", "virtual-call");
            foreach (var type in new[] { load.Type, cast.Type, add.Type, field.Type })
            {
                ValidateAggregateQualifiers(type);
                if (type.Size != 8 || type.Alignment != 8)
                    throw Limit("The actual virtual callee load needs its natural eight-byte pointer views.", "virtual-call");
            }
            if (cast.Operator.RequiresRuntimeCastCheck || !field.Type.Equals(pointerField.Type) ||
                !field.Operands[0].Type.Equals(receiverExpression.Type) || !add.Type.Equals(field.Type))
                throw Limit("The lowered virtual pointer path changes its retained representation.", "virtual-call");
            for (var index = 0; index < 2; ++index)
            {
                var argument = call.Arguments[index]; var parameter = signature.Parameters[index];
                if (argument.Ordinal != index || !argument.Value.Equals(call.Operands[index + 1]) || argument.Parameter?.Equals(parameter.Descriptor) != true ||
                    argument.Role?.Equals(parameter.Role) != true || !argument.Value.Type.Equals(parameter.Type) ||
                    index == 0 && !argument.Value.Equals(call.Receiver))
                    throw Limit("The virtual receiver is already present once in the captured argument list.", "virtual-call");
            }
        }

        private void ValidateVirtualViewUses(IReadOnlyDictionary<FunctionSymbol, SemanticNode[]> graph)
        {
            var plan = virtualDispatch!;
            bool IsViewOrViewPointer(CppType type) => MemoryRepresentationType(type).Equals(plan.View) ||
                MemoryRepresentationType(type) is PointerType pointer && MemoryRepresentationType(pointer.PointeeType).Equals(plan.View);
            bool IsClassValue(CppType type) => MemoryRepresentationType(type).Equals(plan.View) ||
                MemoryRepresentationType(type).Equals(plan.Base) || MemoryRepresentationType(type).Equals(plan.Derived);
            if (inputs[0].TranslationUnit.Variables.Any(variable => IsViewOrViewPointer(variable.Type)) ||
                functions.Any(function => IsClassValue(function.ReturnType) || IsViewOrViewPointer(function.ReturnType) ||
                    function.Parameters.Any(parameter => IsClassValue(parameter.Type) || IsViewOrViewPointer(parameter.Type))))
                throw Limit("The packed base view has no standalone/pointer-storage contract, and these virtual classes have no by-value call contract.", "virtual-layout");
            foreach (var node in graph.Values.SelectMany(nodes => nodes))
            {
                if (node is ExpressionOperation expression && MemoryRepresentationType(expression.Type).Equals(plan.View) &&
                    !(expression is ReferenceOperation { Target: FieldSymbol field } && field.Equals(plan.BaseField)) &&
                    !(expression is OperatorOperation { OperatorKind.Name: "eok_points_to_field" } member && expression.ValueCategory.Name == "lvalue" &&
                        member.Operands.Count == 2 && member.Operands[1] is ReferenceOperation { Target: FieldSymbol selected } && selected.Equals(plan.BaseField)))
                    throw Limit("The occupied base view may only serve its captured generated-field lvalue path.", "virtual-layout");
                if (node is ConversionOperation { OperatorKind.Name: "eok_lvalue_adjust" } conversion &&
                    !conversion.SourceType.Equals(conversion.TargetType))
                {
                    if (!conversion.SourceType.Equals(plan.Derived) || !conversion.TargetType.Equals(plan.Base) ||
                        conversion.ValueCategory.Name != "lvalue" || conversion.Operand.ValueCategory.Name != "lvalue" ||
                        conversion.Operator.RequiresRuntimeCastCheck || conversion.Operands.Count != 1)
                        throw Limit("Only the captured zero-offset derived-to-base lvalue view is admitted.", "virtual-layout");
                    virtualBaseViews.Add(conversion);
                }
                if (node is OperatorOperation operation && (operation.OperatorKind.Name is "eok_bassign" or "eok_class_rvalue_adjust" ||
                    operation.OperatorKind.Name == "eok_assign" && operation.Operands.Count != 0 &&
                        (MemoryRepresentationType(operation.Operands[0].Type).Equals(plan.Base) || MemoryRepresentationType(operation.Operands[0].Type).Equals(plan.Derived))))
                    throw Limit("This virtual record profile has no whole-object copy or class-rvalue adaptation contract.", "virtual-layout");
            }
        }

        private void ValidateVirtualBaseCleanup(FunctionSymbol function, IReadOnlyList<SemanticNode> nodes)
        {
            var plan = virtualDispatch!; var body = function.Body!; var regions = body.ExceptionHandling.CleanupRegions;
            if (regions.Count != 1 || regions[0].Actions.Count != 1 || regions[0].Actions[0] is not { TargetMethod: { } destructor, Storage: { } storage } action ||
                !action.Details.IsBaseSubobject || storage.Kind.Name != "receiver_relative" || !storage.UsesThisParameterOffset || storage.IsIndirect ||
                storage.Offset != plan.BaseSubobject.Offset || storage.Variable is not null || storage.Details.BaseEntity is not null ||
                !virtualAlternates.Contains(destructor) || destructor.SpecialFunctionKind?.Name != "sfk_destructor" ||
                destructor.PrimaryEntry is not { } primary || primary.ContainingType?.Equals(plan.Base) != true ||
                function.Parameters.Count == 0 || function.Parameters[0].Symbol is not { } receiver)
                throw Limit("The virtual base cleanup must retain its exact receiver-relative subobject destructor and selected entry.", "virtual-lifetime");
            var recipes = body.Lifetimes.SelectMany(lifetime => lifetime.Cleanups).Select(cleanup => cleanup.Initialization).ToArray();
            if (recipes.Length != 1 || recipes[0].Destructor?.Equals(primary) != true || recipes[0].Target is not null || recipes[0].Details.Type is not null ||
                recipes[0].Lifetime?.BoundEntity?.Equals(body.Scope) != true)
                throw Limit("The virtual base recipe must match the actual complete destructor associated with the selected subobject entry.", "virtual-lifetime");
            var recipe = recipes[0]; ValidateExceptionMetadataRecipe(recipe, false, true);
            bool IsReceiver(ExpressionOperation value) => value is ConversionOperation { OperatorKind.Name: "eok_cast" } cast &&
                GeneratedRecordPointer(cast.Type, false).Equals(plan.Base) && cast.Operand is ReferenceOperation { Target: VariableSymbol actual } && actual.Equals(receiver);
            var calls = nodes.OfType<CallOperation>().Where(call => call.Receiver is { } value && IsReceiver(value)).ToArray();
            if (function.SpecialFunctionKind?.Name == "sfk_constructor")
            {
                if (body.Scope is null || body.Scope.ConstructorInitializers.Count != 0 || calls.Length != 1 ||
                    calls[0].TargetMethod is not { } constructor || !virtualAlternates.Contains(constructor) || constructor.SpecialFunctionKind?.Name != "sfk_constructor" ||
                    constructor.PrimaryEntry?.ContainingType?.Equals(plan.Base) != true || !HasImmediateCompletionState(nodes, calls[0], regions[0]))
                    throw Limit("The retained base constructor call must precede its actual completion state; metadata initialization is not replayed.", "virtual-lifetime");
            }
            else if (function.SpecialFunctionKind?.Name != "sfk_destructor" || calls.Length != 1 || calls[0].TargetMethod?.Equals(destructor) != true ||
                body.Scope?.ConstructorInitializers.Count(initializer => initializer.Kind.Name == "cik_direct_base_class" &&
                    initializer.BaseSubobject?.Equals(plan.BaseSubobject) == true && initializer.Initialization?.Equals(recipe) == true) != 1)
                throw Limit("Normal base destruction needs its selected explicit call and matching direct-base recipe association.", "virtual-lifetime");
            virtualBaseCleanups.Add(action, new(function, recipe));
        }

        private bool IsVirtualMethod(FunctionSymbol function) => virtualDispatch?.Tables.Any(table => table.Target.Equals(function)) == true ||
            multipleVirtual?.AddressTargets.Contains(function) == true;
        private bool IsVirtualCall(CallOperation call) => virtualDispatch?.Call.Equals(call) == true || multipleVirtual?.Sites.Any(site => site.Call.Equals(call)) == true;
        private IReadOnlyList<FunctionSymbol> VirtualCallTargets(CallOperation call)
        {
            if (multipleVirtual?.Sites.FirstOrDefault(site => site.Call.Equals(call)) is { } site) return site.Targets;
            return virtualDispatch is { } single && single.Call.Equals(call) ? single.Tables.Select(table => table.Target).ToArray() : [];
        }
        private bool IsVirtualAlternate(FunctionSymbol function) => virtualAlternates.Contains(function);
        private bool IsVirtualBaseRecipe(Initialization recipe) => virtualBaseCleanups.Values.Any(cleanup => cleanup.Recipe.Equals(recipe));
        private bool IsVirtualTableAddress(ConstantOperation constant) => constant.Constant.Value is AddressValue address &&
            (virtualDispatch?.Tables.Any(table => address.BaseEntity?.Equals(table.Variable) == true && address.BaseKind.Name == "abk_variable" &&
                address.ByteOffset == 16 && !address.OnePastEnd && address.SubobjectPath is null && !address.Details.IsObjectReflection && address.Details.ParameterNumber is null) == true ||
                IsMultipleTableAddress(address));
        private int VirtualConstructorArgumentCount(FunctionSymbol function) => multipleVirtual is { } multiple && Equals(function.ContainingType, multiple.Derived) ? 3 :
            virtualDispatch is { } plan && function.ContainingType?.Equals(plan.Derived) == true ? 2 : 1;
        private bool NeedsVirtualDispatchPlan(ExpressionOperation expression) => expression is CallOperation call && IsVirtualCall(call) ||
            expression is ConversionOperation conversion && virtualBaseViews.Contains(conversion) || IsOverridingResult(expression);

        private bool TryPlanVirtualBaseView(ConversionOperation conversion, out ExpressionPlan result)
        {
            result = null!;
            if (!virtualBaseViews.Contains(conversion)) return false;
            var operand = PlanExpression(conversion.Operand);
            var place = operand.Place ?? throw Limit("The retained base adjustment needs its actual addressable operand.", "virtual-layout");
            var address = new CsCast(CsType.Address(conversion.Type), new CsAddress(CsType.Address(operand.Type), place));
            var view = new LValuePlan(conversion.Type, LValueKind.Address, address);
            result = new(conversion.Type, conversion.ValueCategory, operand.Prefix, new CsLoad(view), operand.Effects, view);
            return true;
        }

        private bool TryEmitVirtualRecord(RecordType record, StringBuilder output)
        {
            if (TryEmitMultipleRecord(record, output)) return true;
            if (virtualDispatch is not { } plan || !record.Equals(plan.Base) && !record.Equals(plan.Derived) && !record.Equals(plan.View)) return false;
            var borrowedView = record.Equals(plan.View);
            output.Append("[StructLayout(LayoutKind.Explicit, Size = ").Append(StorageSize(record)).Append(", Pack = ").Append(borrowedView ? 1 : 8).AppendLine(")]");
            output.Append("public unsafe struct ").Append(TypeName(record)).AppendLine("\n{");
            if (borrowedView) output.AppendLine("    // Borrowed occupied-base view. Its owner supplies the captured 8-byte alignment.");
            if (record.Equals(plan.Derived)) output.AppendLine("    [FieldOffset(0)] public nint __layout_alignment;");
            foreach (var field in record.Layout!.Fields)
                output.Append("    [FieldOffset(").Append(field.Offset).Append(")] public ").Append(TypeName(field.Field.Type)).Append(' ').Append(FieldName(field.Field)).AppendLine(";");
            output.AppendLine("}").AppendLine(); return true;
        }

        private string VirtualCall(CallOperation call, string address, IReadOnlyList<string> arguments)
        {
            if (!IsVirtualCall(call) || arguments.Count != 2) throw Limit("An indirect member call has no validated generated virtual gateway.", "virtual-call");
            if (multipleVirtual?.Sites.FirstOrDefault(site => site.Call.Equals(call)) is { } site)
                return $"{Owner}.{site.Invoke}({address}, {arguments[0]}, {arguments[1]})";
            return $"{Owner}.{virtualDispatch!.Invoke}({address}, {arguments[0]}, {arguments[1]})";
        }

        private void EmitVirtualDispatchSupport()
        {
            EmitMultipleDispatchSupport();
            if (virtualDispatch is not { } plan) return;
            methods.Append("    private static int ").Append(plan.Invoke).Append("(nint address, ").Append(TypeName(plan.Base)).AppendLine("* receiver, int argument)")
                .AppendLine("    {");
            foreach (var table in plan.Tables)
                methods.Append("        if (address == ").Append(FunctionAddress(table.Target)).Append(") return ").Append(OwnedName(table.Target)).Append("((")
                    .Append(TypeName(table.Record)).AppendLine("*)receiver, argument);");
            methods.AppendLine("        throw new InvalidOperationException(\"Virtual address is outside the selected generated table closure.\");").AppendLine("    }").AppendLine();
        }
    }
}
