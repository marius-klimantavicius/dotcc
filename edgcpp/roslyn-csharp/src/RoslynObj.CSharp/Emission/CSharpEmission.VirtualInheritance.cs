using System.Numerics;
using System.Text;
using RoslynObj;

namespace RoslynObj.CSharp;

public sealed partial class CSharpEmitter
{
    private sealed partial class Emission
    {
        private sealed record VbaseRecord(RecordType Type, RecordType View, FunctionSymbol Method,
            FunctionSymbol? Thunk, FieldSymbol Pointer, VariableSymbol Table, VariableSymbol? Vtt);
        private sealed record VbaseResult(FunctionSymbol Wrapper, FunctionSymbol Target, ExpressionOperation Operation);
        private sealed record VbaseDispatch(VbaseRecord Root, VbaseRecord Branch, VbaseRecord Final,
            BaseSubobject BranchRoot, BaseSubobject FinalBranch, BaseSubobject FinalRoot,
            IReadOnlyList<VariableSymbol> Tables, IReadOnlyList<FunctionSymbol> Targets,
            IReadOnlyList<VbaseResult> Results, IReadOnlyList<FunctionSymbol> Lifetimes,
            CallOperation Call, string Invoke);
        private VbaseDispatch? virtualInheritance;
        private readonly HashSet<FunctionSymbol> vbaseNormalBodies = [];

        private bool TryInitializeVirtualInheritance(IReadOnlyList<FunctionSymbol> selectedMethods)
        {
            if (selectedMethods.Count != 3 || !selectedMethods.Any(method => method.ContainingType is RecordType { HasVirtualBases: true })) return false;
            RequireNormalLifetimeProfile();
            var rootMethods = selectedMethods.Where(method => method.ContainingType?.Bases.Count == 0).ToArray();
            var branchMethods = selectedMethods.Where(method => method.ContainingType?.Bases is { Count: 1 } bases && bases[0].IsVirtual).ToArray();
            var finalMethods = selectedMethods.Where(method => method.ContainingType?.Bases is { Count: 1 } bases && !bases[0].IsVirtual).ToArray();
            if (HostProtocolEnabled || rootMethods.Length != 1 || branchMethods.Length != 1 || finalMethods.Length != 1 ||
                rootMethods[0].ContainingType is not RecordType root || branchMethods[0].ContainingType is not RecordType branch || finalMethods[0].ContainingType is not RecordType final ||
                !branch.Bases[0].BaseType.Equals(root) || !final.Bases[0].BaseType.Equals(branch) ||
                branch.Bases[0].Subobject is not { } branchRoot || final.Bases[0].Subobject is not { } finalBranch ||
                branch.BaseSubobjects.Count != 1 || final.BaseSubobjects.Count != 2)
                throw Limit("The virtual-base profile requires its exact retained root, virtual-base branch and final direct extension.", "virtual-base");
            var finalRoots = final.BaseSubobjects.Where(subobject => subobject.BaseType.Equals(root)).ToArray();
            if (finalRoots.Length != 1) throw Limit("The final record must retain one unique virtual root subobject.", "virtual-base");
            var finalRoot = finalRoots[0];
            RequireVbaseSubobject(branchRoot, branch, root, 16, true, true, false);
            RequireVbaseSubobject(finalBranch, final, branch, 0, false, true, true);
            RequireVbaseSubobject(finalRoot, final, root, 24, true, false, false);
            if (!branch.BaseSubobjects.Contains(branchRoot) || !final.BaseSubobjects.Contains(finalBranch) ||
                final.PrimaryBaseSubobject?.Equals(finalBranch) != true || final.VirtualFunctionInfoBaseSubobject?.Equals(finalBranch) != true ||
                branchRoot.VirtualBaseOffsetIndex != -3 || finalRoot.VirtualBaseOffsetIndex != -3 ||
                branchRoot.ConstructionTableIndex != 2 || finalRoot.ConstructionTableIndex != 4 || finalBranch.ConstructionSubarrayIndex != 2)
                throw Limit("Virtual-base, primary-base and VTT subarray associations differ from the exercised captured graph.", "virtual-base");
            var wrappers = functions.Where(function => function.OverridingFunctionForWrapper is not null || function.OverriddenFunctionForWrapper is not null).ToArray();
            if (wrappers.Length != 2) throw Limit("The virtual-base profile requires two actual dynamic adjustor definitions.", "virtual-adjustor");
            var branchWrappers = wrappers.Where(function => function.OverridingFunctionForWrapper?.Equals(branchMethods[0]) == true).ToArray();
            var finalWrappers = wrappers.Where(function => function.OverridingFunctionForWrapper?.Equals(finalMethods[0]) == true).ToArray();
            if (branchWrappers.Length != 1 || finalWrappers.Length != 1 || branchWrappers[0].Equals(finalWrappers[0]) ||
                wrappers.Any(wrapper => wrapper.OverriddenFunctionForWrapper?.Equals(rootMethods[0]) != true))
                throw Limit("Each dynamic adjustor must retain its exact overriding and root-method identities.", "virtual-adjustor");
            var branchThunk = branchWrappers[0]; var finalThunk = finalWrappers[0];
            var rootPlan = RequireVbaseRecord(root, rootMethods[0], null, 16, 12, 2, false);
            var branchPlan = RequireVbaseRecord(branch, branchMethods[0], branchThunk, 32, 12, 3, true);
            var finalPlan = RequireVbaseRecord(final, finalMethods[0], finalThunk, 40, 24, 3, true);
            RequireVbaseField(branch.Layout!.Fields[2], rootPlan.View, 16);
            RequireVbaseField(final.Layout!.Fields[0], branchPlan.View, 0);
            RequireVbaseField(final.Layout!.Fields[2], rootPlan.View, 24);
            RequireVbaseField(finalPlan.View.Layout!.Fields[0], branchPlan.View, 0);
            RequireVirtualIntField(root.Layout!.Fields[1], 8); RequireVirtualIntField(rootPlan.View.Layout!.Fields[1], 8);
            RequireVirtualIntField(branch.Layout!.Fields[1], 8); RequireVirtualIntField(branchPlan.View.Layout!.Fields[1], 8);
            RequireVbaseLongField(final.Layout!.Fields[1], 16); RequireVbaseLongField(finalPlan.View.Layout!.Fields[1], 16);
            if (branchMethods[0].Overrides.Count != 1 || !branchMethods[0].Overrides[0].Overridden.Equals(rootMethods[0]) ||
                branchMethods[0].Overrides[0].Subobject?.Equals(branchRoot) != true || finalMethods[0].Overrides.Count != 2 ||
                !finalMethods[0].Overrides.Any(relation => relation.Overridden.Equals(branchMethods[0]) && relation.Subobject?.Equals(finalBranch) == true) ||
                !finalMethods[0].Overrides.Any(relation => relation.Overridden.Equals(rootMethods[0]) && relation.Subobject?.Equals(finalRoot) == true) ||
                selectedMethods.SelectMany(method => method.Overrides).Any(relation => relation.OverridingSubobject is not null || relation.ReturnAdjustmentSubobject is not null))
                throw Limit("The virtual-base methods lack their exact captured override/subobject relations.", "virtual-base");
            foreach (var plan in new[] { rootPlan, branchPlan, finalPlan }) ValidateVbaseMethod(plan.Method, plan.Type, false);
            ValidateVbaseMethod(branchThunk, branch, true); ValidateVbaseMethod(finalThunk, final, true);
            var results = new[] { new VbaseResult(branchThunk, branchMethods[0], ValidateVbaseAdjustor(branchThunk, branchMethods[0])),
                new VbaseResult(finalThunk, finalMethods[0], ValidateVbaseAdjustor(finalThunk, finalMethods[0])) };
            var rootTable = RequireMultipleTable(rootPlan.Table, 3);
            RequireVbaseInteger(rootTable[0].Value, 0); RequireVbaseTypeinfo(rootTable[1].Value, root); RequireVbaseRoutine(rootTable[2].Value, rootPlan.Method);
            ValidateVbaseCompleteTable(branchPlan, 16); ValidateVbaseCompleteTable(finalPlan, 24);
            var branchVtt = RequireVbaseVtt(branchPlan.Vtt!, 2); var finalVtt = RequireVbaseVtt(finalPlan.Vtt!, 4);
            RequireVbaseAddress(branchVtt[0].Value, branchPlan.Table, 24); RequireVbaseAddress(branchVtt[1].Value, branchPlan.Table, 56);
            RequireVbaseAddress(finalVtt[0].Value, finalPlan.Table, 24); RequireVbaseAddress(finalVtt[3].Value, finalPlan.Table, 56);
            if (finalVtt[1].Value is not AddressValue { BaseEntity: VariableSymbol constructionBranch } ||
                finalVtt[2].Value is not AddressValue { BaseEntity: VariableSymbol constructionRoot } || constructionBranch.Equals(constructionRoot))
                throw Limit("The final VTT must preserve its two distinct captured construction tables.", "virtual-table");
            RequireVbaseAddress(finalVtt[1].Value, constructionBranch, 24); RequireVbaseAddress(finalVtt[2].Value, constructionRoot, 24);
            var branchConstruction = RequireMultipleTable(constructionBranch, 4); var rootConstruction = RequireMultipleTable(constructionRoot, 4);
            RequireVbaseInteger(branchConstruction[0].Value, 24); RequireVbaseInteger(branchConstruction[1].Value, 0);
            RequireVbaseTypeinfo(branchConstruction[2].Value, branch); RequireVbaseRoutine(branchConstruction[3].Value, branchPlan.Method);
            RequireVbaseInteger(rootConstruction[0].Value, -24); RequireVbaseInteger(rootConstruction[1].Value, -24);
            RequireVbaseTypeinfo(rootConstruction[2].Value, branch); RequireVbaseRoutine(rootConstruction[3].Value, branchThunk);
            var graph = functions.ToDictionary(function => function, function => ExceptionExecutionNodes(function.Body!.Root).ToArray());
            if (graph.Values.SelectMany(nodes => nodes).Any(node => node is ThrowOperation or TryOperation or AllocationOperation or TemporaryOperation or ReuseOperation))
                throw Limit("This virtual-base profile has no throwing, allocation, temporary ownership or reuse execution.", "virtual-base");
            var calls = graph.Values.SelectMany(nodes => nodes.OfType<CallOperation>()).Where(call => call.Callee is not ReferenceOperation { Target: FunctionSymbol }).ToArray();
            if (calls.Length != 1) throw Limit("The virtual-base profile requires one actual lowered root-slot call.", "virtual-call");
            ValidateLoweredVirtualCall(calls[0], rootPlan.Method, rootPlan.Pointer);
            var lifetimes = functions.Where(function => new[] { root, branch, final }.Contains(function.ContainingType) &&
                function.SpecialFunctionKind?.Name is "sfk_constructor" or "sfk_destructor").ToArray();
            if (lifetimes.Length != 14) throw Limit("The virtual-base graph requires its fourteen distinct selected lifetime definitions.", "virtual-lifetime");
            virtualInheritance = new(rootPlan, branchPlan, finalPlan, branchRoot, finalBranch, finalRoot,
                [rootPlan.Table, branchPlan.Table, finalPlan.Table, constructionBranch, constructionRoot],
                [rootPlan.Method, branchPlan.Method, branchThunk, finalPlan.Method, finalThunk], results, lifetimes, calls[0], InternalName("__InvokeVirtual"));
            ValidateVbaseViews(graph);
            foreach (var function in lifetimes) ValidateVbaseLifetimeSignature(function);
            foreach (var function in lifetimes.Where(function => function.PrimaryEntry is not null)) virtualAlternates.Add(function);
            ValidateVbaseEntryBodies(graph);
            foreach (var function in lifetimes) EnsureGeneratedNoThrow(function);
            foreach (var target in virtualInheritance.Targets) EnsureGeneratedNoThrow(target);
            foreach (var function in functions.Where(function => function.Body!.ExceptionHandling.CleanupRegions.Count != 0)) ValidateVbaseNormalBody(function, graph[function]);
            return true;
        }

        private static void RequireVbaseSubobject(BaseSubobject subobject, RecordType owner, RecordType type, int offset, bool isVirtual, bool direct, bool shared)
        {
            if (!subobject.BaseType.Equals(type) || subobject.MostDerivedType?.Equals(owner) != true || subobject.Offset != offset ||
                subobject.IsVirtual != isVirtual || subobject.IsDirect != direct || subobject.SharesVirtualFunctionInfo != shared ||
                subobject.IsAmbiguous || subobject.IsOptimizedEmptyBase || subobject.IsPackExpansion)
                throw Limit("The virtual-base subobject lacks its exact ordinary captured identity and layout.", "virtual-layout");
        }

        private VbaseRecord RequireVbaseRecord(RecordType record, FunctionSymbol method, FunctionSymbol? thunk, int size, int occupied, int fields, bool virtualBases)
        {
            if (record.SubobjectPartner is not RecordType view || view.Equals(record) || view.SubobjectPartner?.Equals(record) != true)
                throw Limit("Each virtual-base record requires its distinct retained occupied layout view.", "virtual-layout");
            foreach (var type in new[] { record, view })
            {
                ValidateAggregateQualifiers(type);
                if (type.Kind.Name is not ("tk_struct" or "tk_class") || type.IsIncomplete || type.IsPacked || type.Alignment != 8 ||
                    !type.HasVirtualFunctions || type.HasPureVirtualFunctions || type.VirtualFunctionInfoOffset != 0 || type.Layout is not { IsComplete: true })
                    throw Limit("The virtual-base representation requires ordinary complete captured layouts and natural vptr alignment.", "virtual-layout");
            }
            if (record.Size != size || record.Layout!.NonvirtualSize != occupied || record.Layout!.NonvirtualAlignment != 8 ||
                record.Layout!.Fields.Count != fields || record.HasVirtualBases != virtualBases || view.HasVirtualBases || view.Size != occupied ||
                view.Layout!.NonvirtualSize != occupied || view.Layout!.Fields.Count != 2 ||
                record.VirtualFunctionTableVariable is not { } table || virtualBases != (record.VirtualTableTableVariable is not null))
                throw Limit("The virtual-base complete/occupied layout or actual table associations differ from the bounded representation.", "virtual-layout");
            var pointer = record.Bases.Count == 1 && !record.Bases[0].IsVirtual && record.Bases[0].BaseType is RecordType { SubobjectPartner: RecordType occupiedBase }
                ? VirtualPointerField(occupiedBase) : VirtualPointerField(record);
            return new(record, view, method, thunk, pointer, table, record.VirtualTableTableVariable);
        }

        private static void RequireVbaseField(FieldLayout field, RecordType type, int offset)
        {
            if (field.Offset != offset || field.BitWidth is not null || !field.Field.IsGeneratedBase || !field.Field.IsCompilerGenerated ||
                !MemoryRepresentationType(field.Field.Type).Equals(type))
                throw Limit("The virtual-base field must retain its exact occupied record identity and displacement.", "virtual-layout");
        }
        private static void RequireVbaseLongField(FieldLayout field, int offset)
        {
            if (field.Offset != offset || field.BitWidth is not null || field.Field.IsGeneratedBase || field.Field.IsBitField)
                throw Limit("The virtual-base long field differs from its captured scalar layout.", "virtual-layout");
            ValidateVirtualWord(field.Field.Type);
        }
        private static void RequireVbaseInteger(ConstantValue value, int expected)
        {
            if (value is not IntegerValue integer || integer.Value != expected) throw Limit("A captured virtual-table signed prefix differs from the bounded profile.", "virtual-table");
        }
        private static void RequireVbaseTypeinfo(ConstantValue value, RecordType record)
        {
            if (!IsMultipleTypeinfo(value, record)) throw Limit("A captured table typeinfo entry does not name its exact retained record.", "virtual-table");
        }
        private static void RequireVbaseRoutine(ConstantValue value, FunctionSymbol function)
        {
            if (value is not AddressValue address || address.BaseKind.Name != "abk_routine" || !IsDirectZeroAddress(address) || address.BaseEntity?.Equals(function) != true)
                throw Limit("A captured virtual slot does not name its exact selected generated entry.", "virtual-table");
        }
        private static void RequireVbaseAddress(ConstantValue value, VariableSymbol variable, int offset)
        {
            if (value is not AddressValue address || address.BaseKind.Name != "abk_variable" || address.BaseEntity?.Equals(variable) != true ||
                address.ByteOffset != offset || address.OnePastEnd || address.SubobjectPath is not null || address.Details.IsObjectReflection || address.Details.ParameterNumber is not null)
                throw Limit("A captured VTT element must preserve its exact table variable and byte address point.", "virtual-table");
        }
        private static IReadOnlyList<AggregateElementDetails> RequireVbaseVtt(VariableSymbol variable, int count)
        {
            if (variable.Type is not ArrayType array || array.Bound != count || array.Size != count * 8 || array.Alignment != 8 ||
                array.ElementType is not QualifiedType { IsConst: true, IsVolatile: false } qualified ||
                MemoryRepresentationType(qualified) is not PointerType pointer || pointer.Size != 8 || pointer.Alignment != 8 ||
                variable.InitializationKind.Name != "initk_static" || variable.Initializer is not null ||
                variable.ConstantInitializer?.Value is not AggregateValue aggregate || !aggregate.Details.Type.Equals(array) || aggregate.Elements.Count != count)
                throw Limit("The VTT requires its actual immutable array of natural table-pointer elements.", "virtual-table");
            ValidateVirtualWord(pointer.PointeeType);
            if (aggregate.Elements.Where((element, index) => element.Ordinal != index || element.Type is null || !MemoryRepresentationType(element.Type).Equals(pointer)).Any())
                throw Limit("The VTT element order or retained pointer view differs from the captured array.", "virtual-table");
            return aggregate.Elements;
        }
        private static void ValidateVbaseCompleteTable(VbaseRecord plan, int rootOffset)
        {
            var elements = RequireMultipleTable(plan.Table, 8);
            RequireVbaseInteger(elements[0].Value, rootOffset); RequireVbaseInteger(elements[1].Value, 0);
            RequireVbaseTypeinfo(elements[2].Value, plan.Type); RequireVbaseRoutine(elements[3].Value, plan.Method);
            RequireVbaseInteger(elements[4].Value, -rootOffset); RequireVbaseInteger(elements[5].Value, -rootOffset);
            RequireVbaseTypeinfo(elements[6].Value, plan.Type); RequireVbaseRoutine(elements[7].Value, plan.Thunk!);
        }

        private void ValidateVbaseMethod(FunctionSymbol function, RecordType record, bool thunk)
        {
            var signature = function.Signature;
            if (!functions.Contains(function) || function.Body is null || function.IsVirtual == thunk || function.IsPureVirtual ||
                function.PrimaryEntry is not null || function.AlternateEntryPoints.Count != 0 || function.SpecialFunctionKind?.Name != "sfk_none" ||
                function.Details.IsWeak == true || function.Details.IsWeakref == true || function.Details.IsGnuAlias == true || function.Details.IsIfunc == true || function.Details.IsAliasEntry == true ||
                function.ThisAdjustment != 0 || function.ReturnAdjustment != 0 || function.VirtualCallIndex != (thunk ? -3 : 0) || function.VirtualBaseIndex != 0 ||
                (thunk ? function.VirtualFunctionNumber is not null : function.VirtualFunctionNumber != 0) ||
                !signature.HasPrototype || signature.IsVariadic || !signature.HasThisParameter || signature.ReturnsValueAsParameter == true || signature.ReturnsValueViaCopyConstructor ||
                signature.ExceptionSpecification?.NoexceptEffective != true || signature.Parameters.Count != 2 || function.Parameters.Count != 2 ||
                signature.ThisClass?.Equals(record) != true || function.ContainingType?.Equals(record) != true ||
                signature.Parameters[0].Role.Name != "this_parameter" || signature.Parameters[1].Role.Name != "ordinary_parameter" ||
                !GeneratedRecordPointer(signature.Parameters[0].Type, false).Equals(record))
                throw Limit("The dynamic virtual entry requires its exact noexcept scalar signature and retained adjustment metadata.", "virtual-adjustor");
            ValidateCallingConvention(signature); ValidateGeneratedInt32(function.ReturnType); ValidateGeneratedInt32(function.Parameters[1].Type);
            ValidateVbaseParameterAssociations(function);
        }

        private static void ValidateVbaseParameterAssociations(FunctionSymbol function)
        {
            if (function.Parameters.Count != function.Signature.Parameters.Count) throw Limit("The selected execution parameter count differs from its signature.", "virtual-lifetime");
            for (var index = 0; index < function.Parameters.Count; ++index)
            {
                var parameter = function.Parameters[index]; var selected = function.Signature.Parameters[index];
                if (parameter.Ordinal != index || selected.Ordinal != index || parameter.Symbol is null || parameter.BaseSubobject is not null || selected.BaseSubobject is not null ||
                    parameter.Descriptor.Details.PassedViaCopyConstructor || !parameter.Descriptor.Equals(selected.Descriptor) || !parameter.Role.Equals(selected.Role) || !parameter.Type.Equals(selected.Type))
                    throw Limit("A virtual-base entry changes the actual selected parameter identity, role or type.", "virtual-lifetime");
            }
        }

        private static ExpressionOperation ValidateVbaseAdjustor(FunctionSymbol wrapper, FunctionSymbol overriding)
        {
            var body = wrapper.Body!; var receiver = wrapper.Parameters[0].Symbol!;
            if (!wrapper.ReturnType.Equals(overriding.ReturnType) || wrapper.Parameters.Where((parameter, index) => !parameter.Type.Equals(overriding.Parameters[index].Type)).Any() ||
                body.ExceptionHandling.RegionTable is not null || body.ExceptionHandling.ArrayTable is not null || body.ExceptionHandling.CleanupRegions.Count != 0 ||
                body.Lifetimes.Count != 0 || body.Locals.Count != 1 || body.Root is not BlockOperation { IsCompilerGenerated: true } block || block.Statements.Count != 3 ||
                block.Statements[0].Expression is not OperatorOperation { OperatorKind.Name: "eok_assign" } assignment || assignment.Operands.Count != 2 ||
                !IsVbaseReference(assignment.Operands[0], receiver) || assignment.Operands[1] is not ConversionOperation { OperatorKind.Name: "eok_cast" } cast || !cast.Type.Equals(receiver.Type) ||
                cast.Operand is not OperatorOperation { OperatorKind.Name: "eok_padd" } bytesAdd || bytesAdd.Operands.Count != 2 || bytesAdd.Operator.PointerOperandIsSecond ||
                bytesAdd.Operands[0] is not ConversionOperation { OperatorKind.Name: "eok_cast" } bytes || !IsVbaseBytePointer(bytes.Type) || !IsVbaseReference(bytes.Operand, receiver) ||
                bytesAdd.Operands[1] is not OperatorOperation { OperatorKind.Name: "eok_indirect" } offset || offset.Operands.Count != 1 ||
                offset.Operands[0] is not OperatorOperation { OperatorKind.Name: "eok_padd" } slot || slot.Operands.Count != 2 || slot.Operator.PointerOperandIsSecond ||
                slot.Operands[1] is not ConstantOperation { Constant.Value: IntegerValue index } indexExpression || index.Value != wrapper.VirtualCallIndex ||
                slot.Operands[0] is not OperatorOperation { OperatorKind.Name: "eok_indirect" } vptr || vptr.Operands.Count != 1 ||
                vptr.Operands[0] is not ConversionOperation { OperatorKind.Name: "eok_cast" } pointer || !IsVbaseReference(pointer.Operand, receiver) ||
                MemoryRepresentationType(pointer.Type) is not PointerType pointerToPointer || !pointerToPointer.PointeeType.Equals(vptr.Type) ||
                MemoryRepresentationType(vptr.Type) is not PointerType tablePointer || !tablePointer.PointeeType.Equals(offset.Type) && !MemoryRepresentationType(tablePointer.PointeeType).Equals(offset.Type) ||
                !slot.Type.Equals(vptr.Type) || !bytesAdd.Type.Equals(bytes.Type) || cast.Operator.RequiresRuntimeCastCheck || bytes.Operator.RequiresRuntimeCastCheck || pointer.Operator.RequiresRuntimeCastCheck ||
                block.Statements[1].Expression is not OperatorOperation { OperatorKind.Name: "eok_assign" } resultAssignment || resultAssignment.Operands.Count != 2 ||
                !IsVbaseReference(resultAssignment.Operands[0], body.Locals[0]) ||
                resultAssignment.Operands[1] is not { Kind.Name: "enk_result_of_overriding_function", RepresentationRole.Name: "execution", Operands.Count: 0, ValueCategory.Name: "prvalue" } result ||
                result.Details.VolatileFetch || !result.Type.Equals(overriding.ReturnType) || !body.Locals[0].Type.Equals(result.Type) ||
                !assignment.Type.Equals(receiver.Type) || !resultAssignment.Type.Equals(result.Type) ||
                block.Statements[2] is not JumpOperation { JumpKind.Name: "return", Value: ReferenceOperation { Target: VariableSymbol returned } } || !returned.Equals(body.Locals[0]))
                throw Limit("The dynamic adjustor must retain its one vptr-indexed byte adjustment, actual overriding-result occurrence and temporary return.", "virtual-adjustor");
            ValidateVirtualWord(offset.Type); ValidateVirtualWord(indexExpression.Type);
            foreach (var type in new[] { cast.Type, bytes.Type, pointer.Type, vptr.Type, slot.Type })
            {
                ValidateAggregateQualifiers(type);
                if (type.Size != 8 || type.Alignment != 8) throw Limit("The dynamic adjustor requires its natural captured pointer representations.", "virtual-adjustor");
            }
            return result;
        }

        private static bool IsVbaseReference(ExpressionOperation expression, VariableSymbol variable) => expression is ReferenceOperation { Target: VariableSymbol target } && target.Equals(variable);
        private static bool IsVbaseBytePointer(CppType type) => MemoryRepresentationType(type) is PointerType { IsReference: false, IsRValueReference: false } pointer &&
            type.Size == 8 && type.Alignment == 8 && pointer.PointeeType.Size == 1 && pointer.PointeeType.Alignment == 1;

        private FunctionSymbol VbaseEntry(RecordType record, string special, string kind)
        {
            var entries = virtualInheritance!.Lifetimes.Where(function => Equals(function.ContainingType, record) &&
                function.SpecialFunctionKind?.Name == special && function.ConstructorOrDestructorKind?.Name == kind).ToArray();
            return entries.Length == 1 ? entries[0] : throw Limit("A virtual-base lifetime role has no unique selected entry.", "virtual-lifetime");
        }

        private void ValidateVbaseLifetimeSignature(FunctionSymbol function)
        {
            var plan = virtualInheritance!; var signature = function.Signature;
            var record = (RecordType)function.ContainingType!; var root = record.Equals(plan.Root.Type);
            var constructor = function.SpecialFunctionKind?.Name == "sfk_constructor";
            var kind = function.ConstructorOrDestructorKind?.Name;
            var primaryKind = root ? "cdk_complete" : "cdk_subobject";
            var primary = VbaseEntry(record, function.SpecialFunctionKind!.Name, primaryKind);
            var hasVtt = !root && kind is "cdk_subobject" or "cdk_delegation";
            var scalarCount = constructor ? root ? 1 : record.Equals(plan.Branch.Type) ? 2 : 3 : 0;
            if (!functions.Contains(function) || function.Body is null || kind is not ("cdk_complete" or "cdk_subobject" or "cdk_delegation") ||
                constructor && kind == "cdk_delegation" || function.IsVirtual || function.IsPureVirtual ||
                function.Details.IsWeak == true || function.Details.IsWeakref == true || function.Details.IsGnuAlias == true || function.Details.IsIfunc == true ||
                !signature.HasPrototype || signature.IsVariadic || !signature.HasThisParameter || signature.ReturnsValueAsParameter == true || signature.ReturnsValueViaCopyConstructor ||
                MemoryRepresentationType(signature.ReturnType).Kind.Name != "tk_void" || !Equals(signature.ThisClass, record) ||
                function.Parameters.Count != 1 + (hasVtt ? 1 : 0) + scalarCount || function.Parameters[0].Role.Name != "this_parameter" ||
                !GeneratedRecordPointer(function.Parameters[0].Type, false).Equals(record) ||
                (function.Equals(primary) ? function.PrimaryEntry is not null : function.PrimaryEntry?.Equals(primary) != true || !primary.AlternateEntryPoints.Contains(function)) ||
                function.Signature.ExceptionSpecification?.NoexceptEffective != true && !(root && kind == "cdk_subobject" || !root && kind == "cdk_delegation") ||
                function.Signature.ExceptionSpecification?.NoexceptEffective == false)
                throw Limit("The virtual-base lifetime entry lacks its exact selected complete/subobject/delegation signature and primary relation.", "virtual-lifetime");
            ValidateCallingConvention(signature); ValidateVbaseParameterAssociations(function);
            if (primary.AlternateEntryPoints.Count != (root ? 1 : constructor ? 1 : 2) || primary.AlternateEntryPoints.Any(alternate => !plan.Lifetimes.Contains(alternate)))
                throw Limit("The virtual-base primary has an unexpected or unavailable alternate entry.", "virtual-lifetime");
            var scalarStart = hasVtt ? 2 : 1;
            if (hasVtt)
            {
                var parameter = function.Parameters[1];
                if (parameter.Role.Name != "vtt_parameter" || parameter.IsSourceParameter ||
                    MemoryRepresentationType(parameter.Type) is not PointerType { IsReference: false, IsRValueReference: false } pointer ||
                    parameter.Type.Size != 8 || parameter.Type.Alignment != 8 || pointer.Size != 8 || pointer.Alignment != 8 ||
                    plan.Branch.Vtt!.Type is not ArrayType vtt || !pointer.PointeeType.Equals(vtt.ElementType))
                    throw Limit("The hidden VTT must be the actual selected natural pointer to the captured const table-pointer element.", "virtual-lifetime");
                ValidateAggregateQualifiers(parameter.Type); ValidateAggregateQualifiers(pointer.PointeeType);
            }
            for (var index = 0; index < scalarCount; ++index)
            {
                var parameter = function.Parameters[scalarStart + index];
                var role = function.Equals(primary) ? "ordinary_parameter" : "abi_parameter";
                if (parameter.Role.Name != role) throw Limit("A virtual-base constructor scalar changed its selected source/ABI role.", "virtual-lifetime");
                if (record.Equals(plan.Final.Type) && index == 2) ValidateVirtualWord(parameter.Type);
                else ValidateGeneratedInt32(parameter.Type);
            }
        }

        private void ValidateVbaseEntryBodies(IReadOnlyDictionary<FunctionSymbol, SemanticNode[]> graph)
        {
            var plan = virtualInheritance!;
            foreach (var function in plan.Lifetimes)
            {
                var record = (RecordType)function.ContainingType!;
                if (record.Equals(plan.Root.Type))
                {
                    if (function.PrimaryEntry is not null) ValidateGeneratedAlternateBody(function);
                    continue;
                }
                var kind = function.ConstructorOrDestructorKind?.Name;
                if (kind == "cdk_delegation")
                {
                    var receiver = function.Parameters[0].Symbol!; var vtt = function.Parameters[1].Symbol!;
                    if (function.Body!.ExceptionHandling.CleanupRegions.Count != 0 || function.Body.Lifetimes.Count != 0 || function.Body.Locals.Count != 0 ||
                        function.Body.Root is not BlockOperation block || block.Statements.Count != 2 ||
                        block.Statements[0].Expression is not OperatorOperation { OperatorKind.Name: "eok_question", Operands.Count: 3 } question ||
                        !IsVbaseReference(question.Operands[0], vtt) ||
                        !IsVbaseDelegationArm(question.Operands[1], VbaseEntry(record, "sfk_destructor", "cdk_subobject"), receiver, vtt) ||
                        !IsVbaseDelegationArm(question.Operands[2], VbaseEntry(record, "sfk_destructor", "cdk_complete"), receiver, null) ||
                        block.Statements[1] is not JumpOperation { JumpKind.Name: "return", Value: null })
                        throw Limit("The selected delegation destructor must retain its exact VTT-tested pair of explicit calls.", "virtual-lifetime");
                }
                else if (kind == "cdk_complete")
                {
                    var calls = graph[function].OfType<CallOperation>().ToArray();
                    var constructor = function.SpecialFunctionKind?.Name == "sfk_constructor";
                    var vtt = record.Equals(plan.Branch.Type) ? plan.Branch.Vtt! : plan.Final.Vtt!;
                    var assignments = graph[function].OfType<OperatorOperation>().Where(operation => operation.OperatorKind.Name == "eok_assign" &&
                        operation.Operands.Count == 2 && operation.Operands[0] is ReferenceOperation { Target: VariableSymbol local } && function.Body!.Locals.Contains(local) &&
                        IsVbaseArrayDecay(operation.Operands[1], vtt)).ToArray();
                    if (assignments.Length != 1 || assignments[0].Operands[0] is not ReferenceOperation { Target: VariableSymbol vttLocal } || calls.Length != 2)
                        throw Limit("The complete lifetime entry requires its actual local VTT assignment and two lowered lifetime calls.", "virtual-lifetime");
                    var primary = VbaseEntry(record, function.SpecialFunctionKind!.Name, "cdk_subobject");
                    var forwarded = calls[constructor ? 1 : 0]; var rootCall = calls[constructor ? 0 : 1];
                    var expectedRoot = VbaseEntry(plan.Root.Type, function.SpecialFunctionKind!.Name, "cdk_subobject");
                    if (forwarded.TargetMethod?.Equals(primary) != true || rootCall.TargetMethod?.Equals(expectedRoot) != true ||
                        forwarded.Arguments.Count != function.Parameters.Count + 1 || !IsVbaseReference(forwarded.Arguments[0].Value, function.Parameters[0].Symbol!) ||
                        !IsVbaseReference(forwarded.Arguments[1].Value, vttLocal) || rootCall.Receiver is null || !IsVbaseRootFieldAddress(rootCall.Receiver, function.Parameters[0].Symbol!, record))
                        throw Limit("The complete lifetime body must preserve its actual selected subobject call, VTT local and separate virtual-root call.", "virtual-lifetime");
                    for (var index = 1; index < function.Parameters.Count; ++index)
                        if (!IsVbaseReference(forwarded.Arguments[index + 1].Value, function.Parameters[index].Symbol!))
                            throw Limit("A complete constructor does not forward its current selected scalar parameter once.", "virtual-lifetime");
                }
                else
                {
                    var vtt = function.Parameters[1].Symbol!;
                    var vptrStores = graph[function].OfType<OperatorOperation>().Where(operation => operation.OperatorKind.Name == "eok_assign" && operation.Operands.Count == 2 &&
                        IsVbasePrimaryPointer(operation.Operands[0], function.Parameters[0].Symbol!, record) && IsVbaseVttLoad(operation.Operands[1], vtt, 0)).ToArray();
                    var expected = function.SpecialFunctionKind?.Name == "sfk_constructor" ? 2 : 1;
                    var rootStores = graph[function].OfType<OperatorOperation>().Where(operation => operation.OperatorKind.Name == "eok_assign" && operation.Operands.Count == 2 &&
                        operation.Operands[0] is OperatorOperation { OperatorKind.Name: "eok_points_to_field", Operands.Count: 2 } field &&
                        field.Operands[1] is ReferenceOperation { Target: FieldSymbol pointerField } && pointerField.Equals(plan.Root.Pointer) &&
                        IsVbaseDynamicRootPointer(field.Operands[0], function.Parameters[0].Symbol!, record) &&
                        IsVbaseVttLoad(operation.Operands[1], vtt, record.Equals(plan.Branch.Type) ? 1 : 3)).ToArray();
                    if (vptrStores.Length != expected || rootStores.Length != 1)
                        throw Limit("The subobject lifetime body must retain its actual VTT-driven primary and dynamic virtual-root vptr stores.", "virtual-lifetime");
                }
            }
        }

        private static bool IsVbaseDelegationArm(ExpressionOperation expression, FunctionSymbol target, VariableSymbol receiver, VariableSymbol? vtt) =>
            expression is OperatorOperation { OperatorKind.Name: "eok_comma", Operands.Count: 2 } comma &&
            comma.Operands[0] is CallOperation call && call.TargetMethod?.Equals(target) == true && call.Arguments.Count == (vtt is null ? 1 : 2) &&
            IsVbaseReference(call.Arguments[0].Value, receiver) && (vtt is null || IsVbaseReference(call.Arguments[1].Value, vtt)) &&
            comma.Operands[1] is ConversionOperation cast && MemoryRepresentationType(cast.Type).Kind.Name == "tk_void" &&
            cast.Operand is ConstantOperation { Constant.Value: IntegerValue { Value.IsZero: true } };

        private static bool IsVbaseArrayDecay(ExpressionOperation expression, VariableSymbol array) =>
            expression is OperatorOperation { OperatorKind.Name: "eok_array_to_pointer", Operands.Count: 1 } decay && IsVbaseReference(decay.Operands[0], array);
        private static bool IsVbaseVttAdd(ExpressionOperation expression, VariableSymbol vtt, int index) =>
            expression is OperatorOperation { OperatorKind.Name: "eok_padd", Operands.Count: 2 } add && !add.Operator.PointerOperandIsSecond &&
            IsVbaseReference(add.Operands[0], vtt) && add.Operands[1] is ConstantOperation { Constant.Value: IntegerValue integer } && integer.Value == index && add.Type.Equals(vtt.Type);
        private static bool IsVbaseVttLoad(ExpressionOperation expression, VariableSymbol vtt, int index) =>
            expression is OperatorOperation { OperatorKind.Name: "eok_indirect", Operands.Count: 1 } load &&
            (index == 0 ? IsVbaseReference(load.Operands[0], vtt) : IsVbaseVttAdd(load.Operands[0], vtt, index));

        private bool IsVbasePrimaryPointer(ExpressionOperation expression, VariableSymbol receiver, RecordType record)
        {
            var plan = virtualInheritance!;
            if (record.Equals(plan.Branch.Type))
                return expression is OperatorOperation { OperatorKind.Name: "eok_points_to_field", Operands.Count: 2 } field &&
                    IsVbaseReference(field.Operands[0], receiver) && field.Operands[1] is ReferenceOperation { Target: FieldSymbol pointer } && pointer.Equals(plan.Branch.Pointer);
            return record.Equals(plan.Final.Type) && expression is OperatorOperation { OperatorKind.Name: "eok_dot_field", Operands.Count: 2 } nested &&
                nested.Operands[1] is ReferenceOperation { Target: FieldSymbol viewPointer } && viewPointer.Equals(VirtualPointerField(plan.Branch.View)) &&
                nested.Operands[0] is OperatorOperation { OperatorKind.Name: "eok_points_to_field", Operands.Count: 2 } member &&
                IsVbaseReference(member.Operands[0], receiver) && member.Operands[1] is ReferenceOperation { Target: FieldSymbol baseField } && baseField.Equals(plan.Final.Type.Layout!.Fields[0].Field);
        }

        private bool IsVbaseDynamicRootPointer(ExpressionOperation expression, VariableSymbol receiver, RecordType record) =>
            expression is ConversionOperation cast && MemoryRepresentationType(cast.Type) is PointerType pointer && MemoryRepresentationType(pointer.PointeeType).Equals(virtualInheritance!.Root.Type) &&
            cast.Operand is OperatorOperation { OperatorKind.Name: "eok_padd", Operands.Count: 2 } add && !add.Operator.PointerOperandIsSecond &&
            add.Operands[0] is ConversionOperation bytes && IsVbaseBytePointer(bytes.Type) && IsVbaseReference(bytes.Operand, receiver) &&
            add.Operands[1] is OperatorOperation { OperatorKind.Name: "eok_subscript", Operands.Count: 2 } offset && !offset.Operator.PointerOperandIsSecond &&
            IsVbasePrimaryPointer(offset.Operands[0], receiver, record) && offset.Operands[1] is ConstantOperation { Constant.Value: IntegerValue integer } && integer.Value == -3;

        private bool IsVbaseRootFieldAddress(ExpressionOperation expression, VariableSymbol receiver, RecordType owner)
        {
            var plan = virtualInheritance!;
            var field = owner.Equals(plan.Branch.Type) ? plan.Branch.Type.Layout!.Fields[2].Field : owner.Equals(plan.Final.Type) ? plan.Final.Type.Layout!.Fields[2].Field : null;
            return field is not null && expression is OperatorOperation { OperatorKind.Name: "eok_address_of", Operands.Count: 1 } address &&
                address.Operands[0] is ConversionOperation { OperatorKind.Name: "eok_lvalue_adjust" } view && virtualBaseViews.Contains(view) &&
                view.Operand is OperatorOperation { OperatorKind.Name: "eok_points_to_field", Operands.Count: 2 } member &&
                IsVbaseReference(member.Operands[0], receiver) && member.Operands[1] is ReferenceOperation { Target: FieldSymbol actual } && actual.Equals(field);
        }

        private void ValidateVbaseViews(IReadOnlyDictionary<FunctionSymbol, SemanticNode[]> graph)
        {
            var plan = virtualInheritance!; var records = new[] { plan.Root, plan.Branch, plan.Final };
            bool IsView(CppType type) => records.Any(record => MemoryRepresentationType(type).Equals(record.View));
            bool IsClass(CppType type) => IsView(type) || records.Any(record => MemoryRepresentationType(type).Equals(record.Type));
            bool ViewPointer(CppType type) => MemoryRepresentationType(type) is PointerType pointer && IsView(pointer.PointeeType);
            var viewFields = records.SelectMany(record => record.Type.Layout!.Fields.Concat(record.View.Layout!.Fields)).Where(field => IsView(field.Field.Type)).Select(field => field.Field).ToArray();
            if (inputs[0].TranslationUnit.Variables.Any(variable => IsView(variable.Type) || ViewPointer(variable.Type)) ||
                functions.Any(function => IsClass(function.ReturnType) || ViewPointer(function.ReturnType) || function.Parameters.Any(parameter => IsClass(parameter.Type) || ViewPointer(parameter.Type))))
                throw Limit("Occupied virtual-base records are borrowed field views, without standalone/pointer-to-view or by-value call representation.", "virtual-layout");
            foreach (var node in graph.Values.SelectMany(nodes => nodes))
            {
                if (node is ExpressionOperation expression && IsView(expression.Type) &&
                    !(expression is ReferenceOperation { Target: FieldSymbol field } && viewFields.Contains(field)) &&
                    !(expression is OperatorOperation { OperatorKind.Name: "eok_points_to_field" or "eok_dot_field", Operands.Count: 2, ValueCategory.Name: "lvalue" } member &&
                        member.Operands[1] is ReferenceOperation { Target: FieldSymbol selected } && viewFields.Contains(selected)))
                    throw Limit("An occupied virtual-base layout escaped its exact generated-field lvalue use.", "virtual-layout");
                if (node is ConversionOperation { OperatorKind.Name: "eok_lvalue_adjust" } conversion && !conversion.SourceType.Equals(conversion.TargetType))
                {
                    var branchView = conversion.SourceType.Equals(plan.Final.Type) && conversion.TargetType.Equals(plan.Branch.Type);
                    var rootView = conversion.SourceType.Equals(plan.Root.View) && conversion.TargetType.Equals(plan.Root.Type) &&
                        conversion.Operand is OperatorOperation { OperatorKind.Name: "eok_points_to_field" or "eok_dot_field", Operands.Count: 2 } rootMember &&
                        rootMember.Operands[1] is ReferenceOperation { Target: FieldSymbol rootField } && viewFields.Contains(rootField) && rootField.Type.Equals(plan.Root.View);
                    if ((!branchView && !rootView) || conversion.ValueCategory.Name != "lvalue" || conversion.Operand.ValueCategory.Name != "lvalue" ||
                        conversion.Operator.RequiresRuntimeCastCheck || conversion.Operands.Count != 1)
                        throw Limit("Only the retained zero-offset branch and already-addressed occupied-root lvalue views are admitted.", "virtual-layout");
                    virtualBaseViews.Add(conversion);
                }
                if (node is OperatorOperation operation && (operation.OperatorKind.Name is "eok_bassign" or "eok_class_rvalue_adjust" ||
                    operation.OperatorKind.Name == "eok_assign" && operation.Operands.Count != 0 && IsClass(operation.Operands[0].Type)))
                    throw Limit("The virtual-base profile has no whole-record-copy or class-rvalue conversion contract.", "virtual-layout");
                if (node is ExpressionOperation { Kind.Name: "enk_result_of_overriding_function" } marker && !plan.Results.Any(result => result.Operation.Equals(marker)))
                    throw Limit("An overriding-result occurrence lacks its exact captured dynamic wrapper context.", "virtual-adjustor");
            }
        }

        private void ValidateVbaseNormalBody(FunctionSymbol function, IReadOnlyList<SemanticNode> capturedNodes)
        {
            var plan = virtualInheritance!; var body = function.Body!; var eh = body.ExceptionHandling;
            if (function.Signature.ExceptionSpecification?.NoexceptEffective != true || eh.Details is not { } details ||
                eh.RegionTable is not { } table || !body.Locals.Contains(table) || eh.ArrayTable is not null || eh.Arrays.Count != 0 ||
                details.InitialState != 65535 || details.NullState != 65535 || details.Prologue is null || eh.Epilogues.Count != 1 ||
                table.ExceptionTable is not { Kind.Name: "cleanup" } projection || !projection.Owner.Equals(table) ||
                projection.FunctionEh.Routine.Equals(function) != true || projection.FunctionEh.RegionTable?.Equals(table) != true ||
                table.Initializer is not null || table.ConstantInitializer is not null || table.LocalStaticInitializations.Any(initializer =>
                    !initializer.Variable.Equals(table) || initializer.Kind.Name != "initk_static" || initializer.Initialization is not null || initializer.Constant is not null || initializer.Lifetime is not null))
                throw Limit("The virtual-base normal cleanup requires its exact metadata-only region table and null-state prologue/epilogue.", "virtual-lifetime");
            var nodes = NormalLifetimeNodes(body.Root).ToArray();
            if (nodes.Any(node => !capturedNodes.Contains(node)) || nodes.OfType<ReferenceOperation>().Any(reference => reference.Target?.Equals(table) == true) ||
                nodes.OfType<ConstantOperation>().Any(constant => constant.Constant.Value is not IntegerValue && !IsVirtualTableAddress(constant)))
                throw Limit("The virtual-base normal body has an unvalidated executable table/constant association.", "virtual-lifetime");
            EnsureGeneratedNoThrow(function);
            foreach (var call in nodes.OfType<CallOperation>()) ValidateGeneratedCallRoles(call);
            var recipes = body.Lifetimes.SelectMany(lifetime => lifetime.Cleanups.Select(cleanup => (Lifetime: lifetime, Recipe: cleanup.Initialization))).ToArray();
            if (body.Lifetimes.Count != 1 || body.Scope is null || recipes.Length == 0 || recipes.Any(pair => pair.Lifetime.BoundEntity?.Equals(body.Scope) != true ||
                pair.Recipe.Lifetime?.Equals(pair.Lifetime) != true))
                throw Limit("The virtual-base recipes must retain their one actual scope-bound normal lifetime.", "virtual-lifetime");
            var primary = plan.Lifetimes.Contains(function) && function.PrimaryEntry is null && function.ConstructorOrDestructorKind?.Name == "cdk_subobject";
            var final = Equals(function.ContainingType, plan.Final.Type);
            var expectedActions = primary && final ? 2 : 1;
            var regions = eh.CleanupRegions;
            if (regions.Count != (primary ? expectedActions * 2 : expectedActions) || recipes.Length != expectedActions)
                throw Limit("The virtual-base region/action/recipe cardinality differs from the captured normal profile.", "virtual-lifetime");
            var matched = new List<Initialization>();
            for (var index = 0; index < regions.Count; ++index)
            {
                var region = regions[index]; var supplement = primary && index % 2 == 1;
                if (region.Ordinal != index || !region.Details.FunctionEh.Routine.Equals(function) || region.Details.IsSupplement != supplement ||
                    (supplement ? region.Details.SupplementOwner?.Equals(regions[index - 1]) != true || region.NextRegion is not null || region.Condition is not null || region.Actions.Count != 0
                        : region.Details.SupplementOwner is not null || region.Actions.Count != 1 || (index == 0 ? region.NextRegion is not null : region.NextRegion?.Equals(regions[0]) != true)))
                    throw Limit("Each virtual-base cleanup region must retain its exact next link or empty owner-associated supplement.", "virtual-lifetime");
                if (supplement) continue;
                var action = region.Actions[0]; var flags = action.Details;
                if (action.Kind.Name != "destructor" || flags.Ordinal != 0 || !flags.Region.Equals(region) || action.Array is not null ||
                    flags.IsArray || flags.IsIndirect || flags.IsVla || flags.IsDeallocation || flags.IsLocalStaticGuard || action.Storage is not { } storage ||
                    action.TargetMethod is not { } destructor || !plan.Lifetimes.Contains(destructor) || destructor.SpecialFunctionKind?.Name != "sfk_destructor")
                    throw Limit("The virtual-base action has unsupported storage, lifetime, array or ABI obligations.", "virtual-lifetime");
                var recipeDestructor = destructor.PrimaryEntry ?? destructor;
                var matches = recipes.Where(pair => pair.Recipe.Destructor?.Equals(recipeDestructor) == true).ToArray();
                if (matches.Length != 1) throw Limit("The action requires its exact selected destructor/primary recipe association.", "virtual-lifetime");
                var recipe = matches[0].Recipe; matched.Add(recipe);
                var local = !plan.Lifetimes.Contains(function);
                ValidateExceptionMetadataRecipe(recipe, false, !local);
                if (recipe.NextInDestructionList is { } next && (index != 2 || matched.Count != 2 || !next.Equals(matched[0])) ||
                    index == 2 && recipe.NextInDestructionList?.Equals(matched[0]) != true)
                    throw Limit("The virtual-base metadata destruction link must match the captured branch-to-root recipe association.", "virtual-lifetime");
                if (local)
                {
                    if (flags.IsBaseSubobject || flags.IsConditional || region.Condition is not null || action.AbiDetail is not null ||
                        storage.Kind.Name != "direct_variable" || storage.Variable is not { } variable || !body.Locals.Contains(variable) ||
                        !variable.Type.Equals(plan.Final.Type) || storage.Offset != 0 || storage.IsIndirect || storage.UsesThisParameterOffset || storage.Details.BaseEntity is not null ||
                        recipe.Target?.Equals(variable) != true || recipe.Details.Type?.Equals(variable.Type) != true ||
                        !destructor.Equals(VbaseEntry(plan.Final.Type, "sfk_destructor", "cdk_complete")))
                        throw Limit("The local final-object recipe/action must retain its actual complete destruction entry and storage.", "virtual-lifetime");
                    var constructors = nodes.OfType<CallOperation>().Where(call => call.TargetMethod?.Equals(VbaseEntry(plan.Final.Type, "sfk_constructor", "cdk_complete")) == true &&
                        call.Receiver is { } receiver && IsAddressOfLocal(receiver, variable)).ToArray();
                    if (constructors.Length != 1 || !HasImmediateCompletionState(nodes, constructors[0], region) ||
                        nodes.OfType<CallOperation>().Count(call => call.TargetMethod?.Equals(destructor) == true && call.Receiver is { } receiver && IsAddressOfLocal(receiver, variable)) != 1)
                        throw Limit("The local final-object cleanup must match its actual completed constructor and explicit normal destructor call.", "virtual-lifetime");
                }
                else
                {
                    var owner = (RecordType)function.ContainingType!; var rootAction = Equals(destructor.ContainingType, plan.Root.Type);
                    var expectedOffset = rootAction ? owner.Equals(plan.Branch.Type) ? 16 : 24 : 0;
                    if (!flags.IsBaseSubobject || recipe.Target is not null || recipe.Details.Type is not null ||
                        storage.Kind.Name != "receiver_relative" || !storage.UsesThisParameterOffset || storage.IsIndirect || storage.Variable is not null ||
                        storage.Details.BaseEntity is not null || storage.Offset != expectedOffset ||
                        (rootAction ? !destructor.Equals(VbaseEntry(plan.Root.Type, "sfk_destructor", "cdk_subobject")) :
                            !final || !destructor.Equals(VbaseEntry(plan.Branch.Type, "sfk_destructor", "cdk_subobject"))))
                        throw Limit("The normal virtual-base cleanup requires its exact receiver-relative root or branch entry.", "virtual-lifetime");
                    var conditional = primary && rootAction;
                    if (flags.IsConditional != conditional || conditional != (region.Condition is not null))
                        throw Limit("The root action condition must retain its actual constructed-flag association.", "virtual-lifetime");
                    var selectedCall = nodes.OfType<CallOperation>().Where(call => call.TargetMethod is { } target && target.SpecialFunctionKind == function.SpecialFunctionKind &&
                        Equals(target.ContainingType, destructor.ContainingType) && call.Receiver is { } receiver &&
                        (rootAction ? IsVbaseRootFieldAddress(receiver, function.Parameters[0].Symbol!, owner) : IsVbaseBranchReceiver(receiver, function.Parameters[0].Symbol!))).ToArray();
                    if (selectedCall.Length != 1 || function.SpecialFunctionKind?.Name == "sfk_destructor" && selectedCall[0].TargetMethod?.Equals(destructor) != true)
                        throw Limit("The cleanup metadata must identify one actual explicit constructor/destructor receiver call.", "virtual-lifetime");
                    VariableSymbol? holder = null;
                    if (!rootAction)
                    {
                        if (action.AbiDetail is not { } abi || abi.IsDelegating != false || abi.VlaCountLocation is not null || abi.DelegationArgumentLocation is not null ||
                            abi.VttLocation is not { Kind.Name: "indirect_variable", IsIndirect: true, UsesThisParameterOffset: false } location ||
                            location.Variable is not { } saved || !body.Locals.Contains(saved) || location.Offset != 0 || location.Details.BaseEntity is not null ||
                            !saved.Type.Equals(function.Parameters[1].Type) || selectedCall[0].Arguments.Count < 2 ||
                            !IsVbaseVttAdd(selectedCall[0].Arguments[1].Value, function.Parameters[1].Symbol!, 1))
                            throw Limit("The branch cleanup ABI must retain its exact indirect saved VTT local and selected subarray call argument.", "virtual-lifetime");
                        holder = saved;
                        var assignments = nodes.OfType<OperatorOperation>().Where(operation => operation.OperatorKind.Name == "eok_assign" && operation.Operands.Count == 2 &&
                            IsVbaseReference(operation.Operands[0], saved) && IsVbaseVttAdd(operation.Operands[1], function.Parameters[1].Symbol!, 1)).ToArray();
                        if (assignments.Length != 1 || !HasVbaseArmingSequence(nodes, region, assignments[0], function.SpecialFunctionKind?.Name == "sfk_constructor" ? selectedCall[0] : null))
                            throw Limit("The saved VTT assignment and actual region-arming state must follow the captured call/assignment sequence.", "virtual-lifetime");
                    }
                    else if (action.AbiDetail is not null) throw Limit("The root cleanup has no additional retained ABI payload.", "virtual-lifetime");
                    if (conditional) ValidateVbaseCondition(function, region.Condition!, selectedCall[0], nodes);
                    if (function.SpecialFunctionKind?.Name == "sfk_constructor" && holder is null && !HasImmediateCompletionState(nodes, selectedCall[0], region))
                        throw Limit("The virtual-root constructor call must precede its actual completion state.", "virtual-lifetime");
                }
            }
            if (matched.Distinct().Count() != recipes.Length || recipes.Any(pair => !matched.Contains(pair.Recipe)))
                throw Limit("The virtual-base cleanup recipes do not have unique captured action associations.", "virtual-lifetime");
            foreach (var initializer in body.Scope.ConstructorInitializers)
            {
                if (initializer.Initialization is not { } recipe || !matched.Contains(recipe) || initializer.Field is not null || initializer.BaseSubobject is not { } subobject ||
                    initializer.SourceExpression is not null || initializer.UsesFieldInitializer ||
                    (initializer.Kind.Name == "cik_virtual_base_class" ? !subobject.Equals(final ? plan.FinalRoot : plan.BranchRoot) || recipe.Destructor?.ContainingType?.Equals(plan.Root.Type) != true :
                        initializer.Kind.Name != "cik_direct_base_class" || !final || !subobject.Equals(plan.FinalBranch) || recipe.Destructor?.ContainingType?.Equals(plan.Branch.Type) != true))
                    throw Limit("A retained virtual-base initializer descriptor adds an unmatched subobject or executable recipe.", "virtual-lifetime");
            }
            var expectedInitializers = primary ? function.SpecialFunctionKind?.Name == "sfk_destructor" && final ? 2 : 1 : 0;
            if (body.Scope.ConstructorInitializers.Count != expectedInitializers)
                throw Limit("The retained normal virtual-base scope has an unexpected initializer association count.", "virtual-lifetime");
            foreach (var marker in nodes.OfType<LoweredEhOperation>()) ValidateNormalCleanupMarker(marker, function, regions);
            if (!nodes.Contains(details.Prologue) || eh.Epilogues.Any(epilogue => !nodes.Contains(epilogue)))
                throw Limit("The normal virtual-base prologue/epilogue is absent from the actual typed body.", "virtual-lifetime");
            normalCleanupFunctions.Add(function); omittedNormalRegionTables.Add(function, table); vbaseNormalBodies.Add(function);
            foreach (var marker in nodes.OfType<LoweredEhOperation>()) normalCleanupMarkers.Add(marker);
        }

        private bool IsVbaseBranchReceiver(ExpressionOperation value, VariableSymbol receiver) => value is ConversionOperation cast &&
            GeneratedRecordPointer(cast.Type, false).Equals(virtualInheritance!.Branch.Type) && IsVbaseReference(cast.Operand, receiver);

        private static bool HasVbaseArmingSequence(IReadOnlyList<SemanticNode> nodes, CleanupRegion region, OperatorOperation saved, CallOperation? constructor)
        {
            foreach (var block in nodes.OfType<BlockOperation>())
                for (var index = 0; index + 1 < block.Statements.Count; ++index)
                    if (block.Statements[index].Expression?.Equals(saved) == true &&
                        block.Statements[index + 1].Expression is CleanupStateOperation { IsUnreachable: false } state && state.Region?.Equals(region) == true && state.StateNumber == region.Ordinal &&
                        (constructor is null || index > 0 && block.Statements[index - 1].Expression?.Equals(constructor) == true)) return true;
            return false;
        }

        private static void ValidateVbaseCondition(FunctionSymbol function, CleanupCondition condition, CallOperation selectedCall, IReadOnlyList<SemanticNode> nodes)
        {
            var body = function.Body!; var vtt = function.Parameters[1].Symbol!; var storage = condition.Storage;
            if (condition.Kind.Name != "constructed" || condition.Details.ComparisonValue != 0 || storage.Kind.Name != "direct_variable" ||
                storage.Variable is not { } flag || !body.Locals.Contains(flag) || storage.Offset != 0 || storage.IsIndirect || storage.UsesThisParameterOffset || storage.Details.BaseEntity is not null)
                throw Limit("The root cleanup condition requires its actual direct constructed-flag local and comparison zero.", "virtual-lifetime");
            ValidateGeneratedInt32(flag.Type);
            var writes = nodes.OfType<OperatorOperation>().Where(operation => operation.OperatorKind.Name == "eok_assign" && operation.Operands.Count == 2 && IsVbaseReference(operation.Operands[0], flag)).ToArray();
            if (writes.Length != 1 || writes[0].Operands[1] is not OperatorOperation { OperatorKind.Name: "eok_eq", Operands.Count: 2 } comparison ||
                !IsVbaseReference(comparison.Operands[0], vtt) || comparison.Operands[1] is not ConstantOperation { Constant.Value: IntegerValue { Value.IsZero: true } } ||
                nodes.OfType<ConditionalOperation>().Count(conditional => conditional.Condition is { } test && IsVbaseReference(test, flag) &&
                    conditional.Then is { } then && ExceptionExecutionNodes(then).Contains(selectedCall)) != 1)
                throw Limit("The constructed flag must retain its one VTT-null assignment and actual conditional root call.", "virtual-lifetime");
        }

        private bool TryEmitVbaseRecord(RecordType record, StringBuilder output)
        {
            if (virtualInheritance is not { } plan) return false;
            var records = new[] { plan.Root, plan.Branch, plan.Final };
            if (!records.Any(selected => selected.Type.Equals(record) || selected.View.Equals(record))) return false;
            var view = records.Any(selected => selected.View.Equals(record));
            output.Append("[StructLayout(LayoutKind.Explicit, Size = ").Append(StorageSize(record)).Append(", Pack = ").Append(view ? 1 : 8).AppendLine(")]");
            output.Append("public unsafe struct ").Append(TypeName(record)).AppendLine("\n{");
            if (view) output.AppendLine("    // Borrowed occupied-base view. Its owner supplies the captured 8-byte alignment.");
            if (record.Equals(plan.Final.Type)) output.AppendLine("    [FieldOffset(0)] public nint __layout_alignment;");
            foreach (var field in record.Layout!.Fields)
                output.Append("    [FieldOffset(").Append(field.Offset).Append(")] public ").Append(TypeName(field.Field.Type)).Append(' ').Append(FieldName(field.Field)).AppendLine(";");
            output.AppendLine("}").AppendLine(); return true;
        }

        private bool IsVbaseTableAddress(AddressValue address) => virtualInheritance is { } plan && address.BaseEntity is VariableSymbol variable &&
            plan.Tables.Contains(variable) && address.BaseKind.Name == "abk_variable" && !address.OnePastEnd && address.SubobjectPath is null &&
            !address.Details.IsObjectReflection && address.Details.ParameterNumber is null &&
            (variable.Equals(plan.Root.Table) ? address.ByteOffset == 16 : variable.Equals(plan.Branch.Table) || variable.Equals(plan.Final.Table) ?
                address.ByteOffset == 24 || address.ByteOffset == 56 : address.ByteOffset == 24);

        private VbaseResult? VbaseOverridingResult(ExpressionOperation expression) => virtualInheritance?.Results.FirstOrDefault(result => result.Operation.Equals(expression));

        private void EmitVbaseDispatchSupport()
        {
            if (virtualInheritance is not { } plan) return;
            foreach (var target in plan.Targets) _ = FunctionAddress(target);
            methods.Append("    private static int ").Append(plan.Invoke).Append("(nint address, ").Append(TypeName(plan.Root.Type)).AppendLine("* receiver, int argument)")
                .AppendLine("    {");
            foreach (var target in new[] { plan.Root.Method, plan.Branch.Thunk!, plan.Final.Thunk! })
                methods.Append("        if (address == ").Append(FunctionAddress(target)).Append(") return ").Append(OwnedName(target)).Append("((")
                    .Append(TypeName(target.Parameters[0].Type)).AppendLine(")receiver, argument);");
            methods.AppendLine("        throw new InvalidOperationException(\"Virtual address is outside the selected generated table closure.\");").AppendLine("    }").AppendLine();
        }
    }
}
