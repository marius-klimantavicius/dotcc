using RoslynObj;

namespace RoslynObj.CSharp;

public sealed partial class CSharpEmitter
{
    private sealed partial class Emission
    {
        private sealed record OrdinaryMethodBody(FunctionSymbol Function, RecordType Record,
            PointerType ReceiverType, VariableSymbol Receiver);
        private readonly Dictionary<FunctionSymbol, OrdinaryMethodBody> ordinaryMethodBodies = [];
        private readonly Dictionary<CallOperation, FunctionSymbol> ordinaryMethodBodyCalls = [];
        private readonly HashSet<RecordType> ordinaryFieldCarriers = [];
        private readonly HashSet<FieldSymbol> ordinaryCarrierFields = [];
        private readonly HashSet<PointerType> addressOnlyPointers = [];
        private readonly Dictionary<ExpressionOperation, HashSet<FunctionSymbol>> addressOnlyOccurrences = [];
        private bool ordinaryAddressCandidatesFrozen;
        private bool ordinaryAddressUsesValidated;

        private void InitializeOrdinaryMethodGraphs()
        {
            var candidates = functions.Where(function => function.Signature.HasThisParameter &&
                function.SpecialFunctionKind?.Name is "sfk_none" or "sfk_conversion" &&
                MemoryRepresentationType(function.ReturnType) is ArithmeticType { Arithmetic.IsBool: true } &&
                !IsVirtualMethod(function)).ToArray();
            if (candidates.Length == 0) return;
            RequireNormalLifetimeProfile();
            foreach (var function in candidates)
                ordinaryMethodBodies.Add(function, ValidateOrdinaryMethodSignature(function));
            foreach (var plan in ordinaryMethodBodies.Values) ValidateOrdinaryMethodBody(plan);
            foreach (var owner in functions)
                foreach (var call in ExceptionExecutionNodes(owner.Body!.Root).OfType<CallOperation>())
                {
                    var direct = call.Callee is ReferenceOperation { Target: FunctionSymbol function } ? CanonicalFunction(function) : null;
                    if (direct is not null && ordinaryMethodBodies.TryGetValue(direct, out var plan))
                    {
                        ValidateOrdinaryBodyCall(call, plan);
                        if (ordinaryMethodBodyCalls.TryGetValue(call, out var previous) && !previous.Equals(owner))
                            throw Limit("An ordinary method call occurrence belongs to different generated bodies.", "ordinary-method");
                        ordinaryMethodBodyCalls[call] = owner;
                    }
                    else if (call.TargetMethod is { } target && ordinaryMethodBodies.ContainsKey(CanonicalFunction(target)))
                        throw Limit("The ordinary method graph requires the actual direct selected callee.", "ordinary-method");
                }
            // This is a representation plan only after all required uses pass.
            // It neither grants a new reference context nor changes noexcept.
            CollectOrdinaryAddressTypes();
            ordinaryAddressCandidatesFrozen = true;
            ValidateOrdinaryAddressUses();
            ordinaryAddressUsesValidated = true;
        }

        private OrdinaryMethodBody ValidateOrdinaryMethodSignature(FunctionSymbol function)
        {
            var signature = function.Signature;
            if (function.Body is null || function.IsVirtual || function.IsPureVirtual ||
                function.ConstructorOrDestructorKind?.Name != "cdk_none" || function.PrimaryEntry is not null || function.AlternateEntryPoints.Count != 0 ||
                function.BuiltinKind is not null || function.OverridingFunctionForWrapper is not null || function.OverriddenFunctionForWrapper is not null ||
                function.ThisAdjustment != 0 || function.ReturnAdjustment != 0 || function.VirtualCallIndex != 0 || function.VirtualBaseIndex != 0 ||
                function.Details.IsWeak == true || function.Details.IsWeakref == true || function.Details.IsGnuAlias == true ||
                function.Details.IsIfunc == true || function.Details.IsAliasEntry == true ||
                function.GnuSupplement?.AliasedRoutine is not null || function.GnuSupplement?.TargetedVersions.Count > 0 ||
                function.GnuSupplement?.Representative is not null || function.GnuSupplement?.IsRepresentative == true ||
                function.GnuSupplement?.IsTargetSpecificVersion == true || function.GnuSupplement?.ResolverRequired == true ||
                !signature.HasPrototype || signature.IsVariadic || signature.ReturnsValueAsParameter == true || signature.ReturnsValueViaCopyConstructor ||
                signature.ReturnValueParameterFollowsThis == true || signature.LanguageLinkage?.Name != "nlk_cplusplus_external" ||
                signature.Parameters.Count != 1 || function.Parameters.Count != 1 || function.ContainingType is not RecordType record ||
                signature.ThisClass?.Equals(record) != true)
                throw Limit("The ordinary Boolean method graph supports fixed unadjusted nonvirtual const receivers only.", "ordinary-method");
            ValidateCallingConvention(signature);
            ValidateGraphBoolean(function.ReturnType);
            RegisterOrdinaryFieldCarrier(record, []);
            var parameter = function.Parameters[0]; var selected = signature.Parameters[0];
            if (parameter.Ordinal != 0 || selected.Ordinal != 0 || parameter.Role.Name != "this_parameter" ||
                !parameter.Role.Equals(selected.Role) || !parameter.Descriptor.Equals(selected.Descriptor) || !parameter.Type.Equals(selected.Type) ||
                parameter.BaseSubobject is not null || selected.BaseSubobject is not null || parameter.Descriptor.Details.PassedViaCopyConstructor ||
                parameter.Symbol is not { } receiver || parameter.Type is not PointerType { IsReference: false, IsRValueReference: false } pointer ||
                !GraphConstReceiver(pointer).Equals(record))
                throw Limit("The ordinary Boolean method requires its exact selected execution receiver and const record view.", "ordinary-method");
            ValidateAggregateQualifiers(receiver.Type);
            if (!receiver.Type.Equals(pointer) && (receiver.Type is not QualifiedType { IsConst: true, IsVolatile: false } constant ||
                !constant.UnderlyingType.Equals(pointer) || constant.Size != 8 || constant.Alignment != 8))
                throw Limit("The ordinary execution receiver supports its selected pointer and retained const pointer-object wrapper only.", "ordinary-method");
            return new(function, record, pointer, receiver);
        }

        private static void ValidateGraphBoolean(CppType type)
        {
            ValidateAggregateQualifiers(type);
            if (MemoryRepresentationType(type) is not ArithmeticType { Arithmetic.IsBool: true, Arithmetic.Bits: 8 } value ||
                type.Size != 1 || type.Alignment != 1 || value.Size != 1 || value.Alignment != 1)
                throw Limit("The ordinary Boolean method requires its captured natural one-byte Boolean representation.", "ordinary-method");
        }

        private static CppType GraphRepresentation(CppType type)
        {
            var original = type;
            for (var depth = 0; ; ++depth)
            {
                if (depth > 32) throw Limit("The ordinary method type wrapper chain exceeds the supported depth.", "ordinary-method");
                ValidateAggregateQualifiers(type);
                if (type.IsIncomplete || type.IsDependent || type.Size != original.Size || type.Alignment != original.Alignment)
                    throw Limit("An ordinary method type wrapper changes its effective captured representation.", "ordinary-method");
                if (type is not TypeReference reference) return type;
                type = reference.UnderlyingType;
            }
        }

        private static RecordType GraphConstReceiver(PointerType pointer)
        {
            ValidateProgramPointer(pointer); ValidateAggregateQualifiers(pointer);
            if (pointer.PointeeType is not QualifiedType { IsConst: true, IsVolatile: false } constant ||
                GraphRepresentation(constant) is not RecordType record)
                throw Limit("An ordinary receiver requires its retained const record or const alias view.", "ordinary-method");
            return record;
        }

        private void RegisterOrdinaryFieldCarrier(RecordType record, HashSet<RecordType> active)
        {
            if (ordinaryFieldCarriers.Contains(record)) return;
            if (ordinaryAddressCandidatesFrozen)
                throw Limit("A required lowered field carrier was not present in the frozen ordinary representation plan.", "address-only-pointer");
            if (!active.Add(record)) throw Limit("The lowered ordinary field carrier contains a cyclic value layout.", "ordinary-method");
            try
            {
                ValidateAggregateQualifiers(record);
                if (record.Kind.Name is not ("tk_struct" or "tk_class") || record.IsIncomplete || record.IsPacked ||
                    record.HasVirtualBases || record.HasVirtualFunctions || record.VirtualFunctionTableVariable is not null || record.VirtualTableTableVariable is not null ||
                    record.SubobjectPartner is { } partner && !partner.Equals(record) ||
                    record.Size is not { } size || size <= 0 || size > int.MaxValue || record.Alignment is not { } alignment ||
                    alignment != 1 && alignment != 2 && alignment != 4 && alignment != 8 || record.Layout is not { IsComplete: true } layout ||
                    layout.NonvirtualSize != size || layout.NonvirtualAlignment != alignment)
                    throw Limit("The ordinary method graph requires complete nonvirtual lowered field carriers with matching complete/nonvirtual storage.", "ordinary-method");
                foreach (var field in layout.Fields)
                {
                    ValidateAggregateQualifiers(field.Field.Type);
                    if (field.Field.ContainingType?.Equals(record) != true || !record.Fields.Contains(field.Field) || field.Field.IsBitField ||
                        field.Field.IsGeneratedVirtualPointer == true || field.Field.IsPacked == true || field.BitWidth is not null ||
                        field.Offset is not { } offset || offset < 0 || field.Field.Type.Size is not { } fieldSize || fieldSize <= 0 ||
                        offset + fieldSize > size || field.Field.Type.Alignment is not { } fieldAlignment || fieldAlignment <= 0 ||
                        fieldAlignment > alignment || offset % fieldAlignment != 0)
                        throw Limit("A lowered ordinary carrier field needs its exact natural type, offset and complete containing extent.", "ordinary-method");
                    var value = GraphRepresentation(field.Field.Type);
                    if (value is RecordType child) RegisterOrdinaryFieldCarrier(child, active);
                    else if (value is PointerType pointer)
                    {
                        ValidateProgramPointer(pointer); ValidateAggregateQualifiers(pointer);
                        if (MemoryRepresentationType(pointer.PointeeType) is RecordType pointee &&
                            (pointee.HasVirtualFunctions || pointee.HasVirtualBases) && !IsRegisteredVirtualCarrier(pointee))
                        {
                            ValidateAggregateQualifiers(pointer.PointeeType);
                            addressOnlyPointers.Add(pointer);
                        }
                        else if (MemoryRepresentationType(pointer.PointeeType) is PointerType)
                            throw Limit("Captured pointer-to-pointer field storage has no address-only method adapter.", "address-only-pointer");
                    }
                    else if (value is not ArithmeticType)
                        throw Limit("The ordinary field carrier currently supports scalar, pointer and captured record fields only.", "ordinary-method");
                    ordinaryCarrierFields.Add(field.Field);
                }
                foreach (var direct in record.Bases)
                {
                    if (direct.IsVirtual || direct.Subobject is not { } subobject || !subobject.IsDirect || subobject.IsVirtual || subobject.IsAmbiguous || subobject.IsPackExpansion ||
                        !record.BaseSubobjects.Contains(subobject) || subobject.MostDerivedType?.Equals(record) != true ||
                        GraphRepresentation(direct.BaseType) is not RecordType baseType || !subobject.BaseType.Equals(direct.BaseType) ||
                        subobject.Offset is not { } offset || offset < 0)
                        throw Limit("An ordinary lowered base association requires its actual nonvirtual record and subobject offset.", "ordinary-method");
                    RegisterOrdinaryFieldCarrier(baseType, active);
                    var fields = layout.Fields.Where(field => field.Field.IsGeneratedBase &&
                        GraphRepresentation(field.Field.Type).Equals(baseType) && field.Offset == offset).ToArray();
                    if (baseType.IsEmpty ? !subobject.IsOptimizedEmptyBase || fields.Length != 0 : fields.Length != 1)
                        throw Limit("The ordinary carrier supports an explicit captured base field or a recorded fieldless optimized empty base.", "ordinary-method");
                }
                if (record.BaseSubobjects.Any(subobject => subobject.IsVirtual || subobject.IsAmbiguous || subobject.IsPackExpansion) ||
                    layout.Fields.Any(field => field.Field.IsGeneratedBase && !record.Bases.Any(direct =>
                        GraphRepresentation(direct.BaseType).Equals(GraphRepresentation(field.Field.Type)) && direct.Subobject?.Offset == field.Offset)))
                    throw Limit("The ordinary carrier has an unsupported virtual/ambiguous or unmatched generated base field.", "ordinary-method");
                // Layout.Fields is the storage recipe. Metadata-only empty base
                // subobjects add no field, offset calculation or synthesized byte.
                ordinaryFieldCarriers.Add(record);
            }
            finally { active.Remove(record); }
        }

        private bool IsRegisteredVirtualCarrier(RecordType record) =>
            virtualDispatch is { } single && (record.Equals(single.Base) || record.Equals(single.Derived) || record.Equals(single.View)) ||
            multipleVirtual is { } multiple && (record.Equals(multiple.Derived) || multiple.Bases.Any(plan => record.Equals(plan.Record) || record.Equals(plan.View))) ||
            virtualInheritance is { } inherited && new[] { inherited.Root, inherited.Branch, inherited.Final }
                .Any(plan => record.Equals(plan.Type) || record.Equals(plan.View));

        private void ValidateOrdinaryMethodBody(OrdinaryMethodBody plan)
        {
            var body = plan.Function.Body!; var eh = body.ExceptionHandling;
            if (body.Lifetimes.Count != 0 || eh.RegionTable is not null || eh.ArrayTable is not null || eh.CleanupRegions.Count != 0 ||
                eh.Arrays.Count != 0 || eh.Details?.Prologue is not null || eh.Epilogues.Count != 0 ||
                eh.Details?.InitialState is not null || eh.Details?.NullState is not null)
                throw Limit("The ordinary Boolean body supports no lifetime or EH execution obligations.", "ordinary-method");
            foreach (var local in body.Locals)
            {
                ValidateGraphBoolean(local.Type);
                if (local.Details.StorageDuration?.Name != "automatic" || local.IsThreadLocal || local.Initializer is not null ||
                    local.ConstantInitializer is not null || local.LocalStaticInitializations.Count != 0)
                    throw Limit("The ordinary Boolean body supports ordinary scalar locals initialized by retained assignments.", "ordinary-method");
            }
            var nodes = ExceptionExecutionNodes(body.Root).ToArray();
            foreach (var node in nodes)
            {
                if (node.RepresentationRole.Name != "execution") throw Limit("Ordinary method bodies require retained execution occurrences.", "ordinary-method");
                switch (node)
                {
                    case BlockOperation { Lifetime: null }: break;
                    case StatementOperation { Kind.Name: "stmk_expr", Expression: not null }: break;
                    case JumpOperation { JumpKind.Name: "return", Target: null, CommonLifetime: null, Value: { } result }:
                        if (!result.Type.Equals(plan.Function.ReturnType)) throw Limit("An ordinary Boolean return changes its captured result type.", "ordinary-method");
                        break;
                    case ExpressionOperation expression: ValidateOrdinaryMethodExpression(expression, plan); break;
                    default: throw Limit($"Execution '{node.Kind}' has no supported ordinary Boolean body projection.", "ordinary-method");
                }
            }
        }

        private void ValidateOrdinaryMethodExpression(ExpressionOperation expression, OrdinaryMethodBody plan)
        {
            if (expression.Details.VolatileFetch) throw Limit("Volatile execution is outside the ordinary Boolean method graph.", "ordinary-method");
            ValidateAggregateQualifiers(expression.Type);
            switch (expression)
            {
                case ReferenceOperation { Operands.Count: 0, Target: VariableSymbol variable }:
                    if (variable.Equals(plan.Receiver) ? !expression.Type.Equals(plan.ReceiverType) :
                        !plan.Function.Body!.Locals.Contains(variable) || !expression.Type.Equals(variable.Type))
                        throw Limit("An ordinary Boolean body variable must be its selected receiver or scalar local.", "ordinary-method");
                    return;
                case ReferenceOperation { Operands.Count: 0, Target: FieldSymbol }: return;
                case ReferenceOperation { Operands.Count: 0, Target: FunctionSymbol function }:
                    if (!ordinaryMethodBodies.ContainsKey(CanonicalFunction(function)))
                        throw Limit("An ordinary Boolean body call must select another admitted generated Boolean body.", "ordinary-method");
                    return;
                case ConstantOperation constant when constant.Constant.Value is IntegerValue integer && integer.Value == 0: return;
                case CallOperation call:
                    if (call.Callee is not ReferenceOperation { Target: FunctionSymbol target } ||
                        !ordinaryMethodBodies.TryGetValue(CanonicalFunction(target), out var selected))
                        throw Limit("The ordinary Boolean method graph has an unadmitted direct or indirect call.", "ordinary-method");
                    ValidateOrdinaryBodyCall(call, selected);
                    return;
                case OperatorOperation operation:
                    if (operation.Operator.RequiresRuntimeCastCheck || operation.Operator.RetainedVirtualCall)
                        throw Limit("Runtime object adjustments and virtual calls are outside ordinary Boolean method bodies.", "ordinary-method");
                    switch (operation.OperatorKind.Name)
                    {
                        case "eok_assign":
                            if (operation.Operands.Count != 2 || !operation.Operator.EvalRightToLeft || operation.Operator.EvalLeftToRight ||
                                operation.Operands[0] is not ReferenceOperation { Target: VariableSymbol local } ||
                                !plan.Function.Body!.Locals.Contains(local) || !operation.Operands[1].Type.Equals(local.Type) || !operation.Type.Equals(local.Type))
                                throw Limit("An ordinary Boolean assignment requires its retained right-to-left scalar-local store.", "ordinary-method");
                            ValidateGraphBoolean(local.Type);
                            return;
                        case "eok_dot_field": case "eok_points_to_field": ValidateOrdinaryGraphField(operation); return;
                        case "eok_address_of":
                            if (operation.Operands.Count != 1 || operation.Operands[0].ValueCategory.Name != "lvalue" ||
                                GraphRepresentation(operation.Type) is not PointerType { IsReference: false } pointer ||
                                !GraphRepresentation(pointer.PointeeType).Equals(GraphRepresentation(operation.Operands[0].Type)) ||
                                ContainsAddressOnlyPointer(operation.Type))
                                throw Limit("The ordinary receiver address requires its selected captured field place and natural pointer view.", "ordinary-method");
                            ValidateProgramPointer(pointer);
                            return;
                        case "eok_cast":
                            if (operation.Operands.Count != 1 || !IsGraphBooleanScalar(operation.Type) || !IsGraphBooleanScalar(operation.Operands[0].Type))
                                throw Limit("An ordinary Boolean body cast supports its selected Boolean/int32 scalar views only.", "ordinary-method");
                            return;
                        case "eok_eq": case "eok_ne":
                            if (operation.Operands.Count != 2 || !IsGraphBooleanScalar(operation.Type) ||
                                !operation.Operands[0].Type.Equals(operation.Operands[1].Type))
                                throw Limit("An ordinary Boolean comparison requires exact matching operand types.", "ordinary-method");
                            if (!IsGraphBooleanScalar(operation.Operands[0].Type) && GraphRepresentation(operation.Operands[0].Type) is not PointerType)
                                throw Limit("The ordinary Boolean comparison supports scalar or captured pointer values only.", "ordinary-method");
                            return;
                    }
                    break;
            }
            throw Limit($"Expression '{expression.Kind}' has no supported ordinary Boolean method projection.", "ordinary-method");
        }

        private static bool IsGraphBooleanScalar(CppType type) => MemoryRepresentationType(type) is ArithmeticType arithmetic &&
            (arithmetic.Arithmetic.IsBool && type.Size == 1 && type.Alignment == 1 ||
             arithmetic.Arithmetic.Bits == 32 && arithmetic.Arithmetic.Signed == true && type.Size == 4 && type.Alignment == 4);

        private void ValidateOrdinaryGraphField(OperatorOperation operation)
        {
            if (operation.Operands.Count != 2 || operation.Operands[1] is not ReferenceOperation { Target: FieldSymbol field, Operands.Count: 0 } fieldReference ||
                !fieldReference.Type.Equals(field.Type) || field.ContainingType is not RecordType owner || !ordinaryCarrierFields.Contains(field) ||
                !GraphRepresentation(operation.Type).Equals(GraphRepresentation(field.Type)))
                throw Limit("An ordinary field access requires its actual selected field and effective type.", "ordinary-method");
            var objectType = GraphRepresentation(operation.Operands[0].Type);
            if (operation.OperatorKind.Name == "eok_points_to_field")
            {
                if (objectType is not PointerType { IsReference: false } pointer || addressOnlyPointers.Contains(pointer))
                    throw Limit("A lowered field receiver requires its typed carrier pointer, never an address-only pointee.", "ordinary-method");
                ValidateProgramPointer(pointer); objectType = GraphRepresentation(pointer.PointeeType);
            }
            if (!objectType.Equals(owner)) throw Limit("The lowered field receiver changes its captured containing record.", "ordinary-method");
        }

        private static void ValidateOrdinaryBodyCall(CallOperation call, OrdinaryMethodBody plan)
        {
            var signature = plan.Function.Signature;
            if (call.Callee is not ReferenceOperation { Target: FunctionSymbol target, Operands.Count: 0 } || !target.Equals(plan.Function) ||
                call.TargetMethod?.Equals(plan.Function) != true || call.CallKind.Name != "direct" || call.SourceTargetMethod is not null ||
                call.SourceDispatchKind is not null || call.Operator.RetainedVirtualCall || call.Operator.RequiresRuntimeCastCheck ||
                !call.Operator.EvalLeftToRight || call.Operator.EvalRightToLeft || call.RepresentationRole.Name != "execution" || call.ValueCategory.Name != "prvalue" ||
                call.Call.FunctionType?.Equals(signature) != true || !call.Type.Equals(plan.Function.ReturnType) || call.Call.ReturnSlot is not null ||
                call.Receiver is not { } receiver || GraphRepresentation(receiver.Type) is not PointerType { IsReference: false, IsRValueReference: false } actual ||
                !GraphConstReceiver(actual).Equals(plan.Record) || call.Arguments.Count != 1 || call.Operands.Count != 2 ||
                !call.Operands[0].Equals(call.Callee) || !call.Operands[1].Equals(receiver) || call.Arguments[0].Ordinal != 0 ||
                !call.Arguments[0].Value.Equals(receiver) || call.Arguments[0].Role?.Equals(signature.Parameters[0].Role) != true ||
                call.Arguments[0].Parameter?.Equals(signature.Parameters[0].Descriptor) != true)
                throw Limit("The ordinary Boolean call requires its actual direct signature and one retained const receiver alias in order.", "ordinary-method");
        }

        private void ValidateRegisteredOrdinaryBodyCall(CallOperation call, FunctionSymbol function)
        {
            if (ordinaryMethodBodies.ContainsKey(function) && (currentFunction is null ||
                !ordinaryMethodBodyCalls.TryGetValue(call, out var owner) || !owner.Equals(currentFunction)))
                throw Limit("The ordinary Boolean method is callable only at its prevalidated generated occurrences.", "ordinary-method");
        }

        private bool ContainsAddressOnlyPointer(CppType type)
        {
            var raw = MemoryRepresentationType(type);
            return raw is PointerType pointer && (addressOnlyPointers.Contains(pointer) || ContainsAddressOnlyPointer(pointer.PointeeType)) ||
                raw is ArrayType array && ContainsAddressOnlyPointer(array.ElementType);
        }

        private bool ContainsAddressOnlyStorage(CppType type, HashSet<CppType>? active = null)
        {
            if (addressOnlyPointers.Count == 0) return false;
            var raw = MemoryRepresentationType(type);
            if (ContainsAddressOnlyPointer(raw)) return true;
            active ??= [];
            if (!active.Add(raw)) return false;
            try
            {
                return raw is RecordType record && record.Layout?.Fields.Any(field => ContainsAddressOnlyStorage(field.Field.Type, active)) == true ||
                    raw is ArrayType array && ContainsAddressOnlyStorage(array.ElementType, active);
            }
            finally { active.Remove(raw); }
        }

        private bool AddressOnlyBoundaryType(CppType type) => ContainsAddressOnlyStorage(type) ||
            addressOnlyPointers.Count != 0 && MemoryRepresentationType(type) is PointerType pointer && ContainsAddressOnlyStorage(pointer.PointeeType);

        private void CollectOrdinaryAddressTypes()
        {
            // Discovery never installs a rendering permission. Finish this
            // finite required-value closure before checking even the first use.
            int previousPointers, previousCarriers;
            do
            {
                previousPointers = addressOnlyPointers.Count; previousCarriers = ordinaryFieldCarriers.Count;
                foreach (var function in functions)
                {
                    foreach (var local in function.Body!.Locals) Collect(local.Type);
                    foreach (var expression in ExceptionExecutionNodes(function.Body.Root).OfType<ExpressionOperation>())
                        Collect(expression.Type);
                }
                foreach (var item in storageOrder) Collect(item.Type);
            } while (previousPointers != addressOnlyPointers.Count || previousCarriers != ordinaryFieldCarriers.Count);

            void Collect(CppType type)
            {
                if (ContainsAddressOnlyStorage(type) && MemoryRepresentationType(type) is RecordType record)
                    RegisterOrdinaryFieldCarrier(record, []);
            }
        }

        private void ValidateOrdinaryAddressUses()
        {
            foreach (var function in functions)
            {
                var body = function.Body!;
                if (ContainsAddressOnlyStorage(function.ReturnType) || function.Parameters.Any(parameter => ContainsAddressOnlyStorage(parameter.Type)))
                    throw Limit("Address-only pointers have no selected captured function parameter or result adapter.", "address-only-pointer");
                if (function.SpecialFunctionKind?.Name is "sfk_constructor" or "sfk_destructor" &&
                    function.ContainingType is { } containing && ContainsAddressOnlyStorage(containing))
                    throw Limit("Address-only carrier construction and destruction require separate lifetime admission.", "address-only-pointer");
                foreach (var local in body.Locals) ValidateAddressOnlyStorageRole(local.Type, local.ConstantInitializer ?? local.Initializer?.Constant);
                var nodes = ExceptionExecutionNodes(body.Root).ToArray();
                var parents = new Dictionary<SemanticNode, List<SemanticNode>>();
                foreach (var node in nodes)
                {
                    IEnumerable<SemanticNode> children = node switch
                    {
                        ExpressionOperation expression => expression.Operands,
                        JumpOperation { Value: { } value } => [value],
                        InitializationStatementOperation initialization when initialization.Initialization.Expression is { } value => [value],
                        StatementOperation { Expression: { } value } => [value],
                        _ => []
                    };
                    foreach (var child in children)
                    {
                        if (!parents.TryGetValue(child, out var owners)) parents.Add(child, owners = []);
                        owners.Add(node);
                    }
                }
                foreach (var node in nodes)
                {
                    if (node is CallOperation call)
                    {
                        var selected = call.Callee is ReferenceOperation { Target: FunctionSymbol target } ? CanonicalFunction(target) : null;
                        var selectedType = selected?.Signature ?? call.Call.FunctionType;
                        if (selectedType is not RoutineType signature)
                            throw Limit("A required ordinary method-graph call has no selected current routine type.", "address-only-pointer");
                        if ((AddressOnlyBoundaryType(signature.ReturnType) || signature.Parameters.Any(parameter => AddressOnlyBoundaryType(parameter.Type))) &&
                            (selected is null || !functions.Contains(selected)))
                            throw Limit("An address-only pointer or containing carrier cannot cross a native or indirect boundary.", "address-only-pointer");
                    }
                    if (node is not ExpressionOperation expression) continue;
                    if (expression is ReferenceOperation { Target: FunctionSymbol method } && ordinaryMethodBodies.ContainsKey(CanonicalFunction(method)) &&
                        (!parents.TryGetValue(expression, out var callParents) || callParents.Any(parent => parent is not CallOperation directCall || !directCall.Callee.Equals(expression))))
                        throw Limit("Ordinary Boolean methods have no function-address or native callback adapter.", "ordinary-method");
                    if (MemoryRepresentationType(expression.Type) is RecordType record && ContainsAddressOnlyStorage(record) &&
                        expression.ValueCategory.Name != "lvalue" && expression is not ReferenceOperation { Target: FieldSymbol })
                        throw Limit("An address-only carrier has no by-value expression or copy adapter.", "address-only-pointer");
                    if (!ContainsAddressOnlyPointer(expression.Type)) continue;
                    if (GraphRepresentation(expression.Type) is not PointerType pointer || !addressOnlyPointers.Contains(pointer))
                        throw Limit("A captured pointer-to-pointer, array or unregistered address-only view is unsupported.", "address-only-pointer");
                    ValidateProgramPointer(pointer);
                    switch (expression)
                    {
                        case ConstantOperation constant when constant.Constant.Value is IntegerValue integer && integer.Value == 0 &&
                            constant.Constant.Type?.Equals(expression.Type) == true: break;
                        case ReferenceOperation { Target: FieldSymbol field } when ordinaryCarrierFields.Contains(field): break;
                        case OperatorOperation operation when operation.OperatorKind.Name is "eok_dot_field" or "eok_points_to_field":
                            ValidateOrdinaryGraphField(operation); break;
                        case ConversionOperation conversion when conversion.OperatorKind.Name == "eok_cast":
                            ValidateScalarSentinelAddress(conversion, function); break;
                        case OperatorOperation { OperatorKind.Name: "eok_assign", Operands.Count: 2 } assignment:
                            if (!assignment.Operands[0].Type.Equals(pointer) || !assignment.Operands[1].Type.Equals(pointer) ||
                                assignment.Operands[0] is not OperatorOperation { OperatorKind.Name: "eok_dot_field" or "eok_points_to_field" })
                                throw Limit("Address-only pointer stores require a selected registered field slot and exact pointer value.", "address-only-pointer");
                            break;
                        default: throw Limit("The address-only pointer occurrence has no selected load/store/null/address representation.", "address-only-pointer");
                    }
                    if (!parents.TryGetValue(expression, out var consumers) || consumers.Any(parent => !IsAddressOnlyConsumer(expression, parent)))
                        throw Limit("The address-only pointer has an unsupported dereference, escape or terminal consumer.", "address-only-pointer");
                    if (!addressOnlyOccurrences.TryGetValue(expression, out var owners)) addressOnlyOccurrences.Add(expression, owners = []);
                    owners.Add(function);
                }
            }
            foreach (var item in storageOrder) ValidateAddressOnlyStorageRole(item.Type, item.Initializer);
        }

        private void ValidateAddressOnlyStorageRole(CppType type, Constant? constant)
        {
            if (!ContainsAddressOnlyStorage(type)) return;
            if (GraphRepresentation(type) is not RecordType record)
                throw Limit("Standalone, array and captured pointer-slot locals need separate address-only storage admission.", "address-only-pointer");
            RegisterOrdinaryFieldCarrier(record, []);
            if (constant is not null)
                throw Limit("Address-only carrier constant initialization requires a separately selected retained recipe.", "address-only-pointer");
        }

        private bool IsAddressOnlyConsumer(ExpressionOperation value, SemanticNode parent)
        {
            if (parent is OperatorOperation operation)
            {
                if (operation.OperatorKind.Name is "eok_eq" or "eok_ne")
                    return operation.Operands.Count == 2 && operation.Operands.All(operand => operand.Type.Equals(value.Type));
                if (operation.OperatorKind.Name == "eok_assign")
                    return operation.Operands.Count == 2 && operation.Operands.All(operand => operand.Type.Equals(value.Type));
                if (operation.OperatorKind.Name is "eok_dot_field" or "eok_points_to_field")
                    return value is ReferenceOperation { Target: FieldSymbol field } && ordinaryCarrierFields.Contains(field) &&
                        operation.Operands.Count == 2 && operation.Operands[1].Equals(value);
            }
            return value is OperatorOperation { OperatorKind.Name: "eok_assign" } && parent is StatementOperation { Kind.Name: "stmk_expr" };
        }

        private void ValidateScalarSentinelAddress(ConversionOperation conversion, FunctionSymbol owner)
        {
            if (!conversion.Operator.IsReinterpretCast || conversion.Operator.RequiresRuntimeCastCheck ||
                conversion.Operand is not OperatorOperation { OperatorKind.Name: "eok_address_of", Operands.Count: 1 } address ||
                GraphRepresentation(address.Type) is not PointerType { IsReference: false } pointer ||
                GraphRepresentation(pointer.PointeeType) is not ArithmeticType scalar || scalar.Alignment != 8 || scalar.Size != 8 ||
                address.Operands[0].ValueCategory.Name != "lvalue" || !GraphRepresentation(address.Operands[0].Type).Equals(scalar) ||
                !IsSelectedScalarAddressPlace(address.Operands[0], owner))
                throw Limit("An address-only pointer cast requires the retained address of selected naturally aligned scalar storage.", "address-only-pointer");
            ValidateProgramPointer(pointer);
        }

        private bool IsSelectedScalarAddressPlace(ExpressionOperation expression, FunctionSymbol owner)
        {
            if (expression is ReferenceOperation { Target: VariableSymbol variable, Operands.Count: 0 })
                return owner.Body!.Locals.Contains(variable) && variable.Details.StorageDuration?.Name == "automatic";
            if (expression is not OperatorOperation { OperatorKind.Name: "eok_dot_field", Operands.Count: 2 } field ||
                field.Operands[1] is not ReferenceOperation { Target: FieldSymbol selected }) return false;
            if (selected.ContainingType is not RecordType record) return false;
            RegisterOrdinaryFieldCarrier(record, []); ValidateOrdinaryGraphField(field);
            return IsSelectedScalarAddressPlace(field.Operands[0], owner);
        }

        private bool TryOrdinaryAddressOnlyType(PointerType pointer, out string representation)
        {
            representation = "";
            if (!addressOnlyPointers.Contains(pointer)) return false;
            if (!ordinaryAddressUsesValidated) throw Limit("Address-only pointer rendering requires completed required-use admission.", "address-only-pointer");
            representation = "void*";
            return true;
        }

        private void ValidateOrdinaryAddressOccurrence(ExpressionOperation expression)
        {
            if (!ContainsAddressOnlyPointer(expression.Type)) return;
            if (!ordinaryAddressUsesValidated || currentFunction is null ||
                !addressOnlyOccurrences.TryGetValue(expression, out var owners) || !owners.Contains(currentFunction))
                throw Limit("The address-only expression is outside its prevalidated actual generated occurrence and owner.", "address-only-pointer");
        }

        private void ValidateOrdinaryCarrierEmission(RecordType record)
        {
            if (ContainsAddressOnlyStorage(record) && (!ordinaryAddressUsesValidated || !ordinaryFieldCarriers.Contains(record)))
                throw Limit("The address-only containing storage has no frozen required-carrier representation.", "address-only-pointer");
        }
    }
}
