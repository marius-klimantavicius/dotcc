using System.Text;
using RoslynObj;

namespace RoslynObj.CSharp;

public sealed partial class CSharpEmitter
{
    private sealed partial class Emission
    {
        private sealed record BorrowedByteView(RecordType Record, HashSet<FieldSymbol> Fields);
        private readonly Dictionary<RecordType, BorrowedByteView> borrowedByteViews = [];
        private readonly HashSet<FunctionSymbol> pointerMethodFunctions = [];
        private readonly Dictionary<ConversionOperation, FunctionSymbol> ordinaryBaseViews = [];
        private readonly HashSet<RecordType> unusedOrdinarySubobjectViews = [];
        private readonly HashSet<FunctionSymbol> pointerValueSignatures = [];
        private readonly Dictionary<VariableSymbol, FunctionSymbol> pointerValueLocals = [];
        private bool pointerMethodsActive;

        private void InitializePointerMethods()
        {
            foreach (var function in functions.Where(function => function.Signature.HasThisParameter &&
                function.SpecialFunctionKind?.Name is "sfk_none" or "sfk_operator" && !IsVirtualMethod(function) &&
                MemoryRepresentationType(function.ReturnType) is PointerType { IsReference: false, IsRValueReference: false }))
                pointerMethodFunctions.Add(function);
            // A borrowed byte view describes selected reads, not the class's
            // representation, lifetime, virtual methods or native ABI.
            foreach (var function in functions.Where(function => function.Signature.HasThisParameter &&
                function.SpecialFunctionKind?.Name == "sfk_none" && !IsVirtualMethod(function) &&
                MemoryRepresentationType(function.ReturnType) is ArithmeticType { Arithmetic.Bits: 8, Arithmetic.Signed: false, Arithmetic.IsBool: false } &&
                function.ContainingType is RecordType { HasVirtualFunctions: true }))
            {
                var record = (RecordType)function.ContainingType!;
                if (IsRegisteredVirtualCarrier(record)) continue;
                RequireNormalLifetimeProfile();
                ValidateBorrowedByteViewRecord(record);
                if (!borrowedByteViews.TryGetValue(record, out var view))
                    borrowedByteViews.Add(record, view = new(record, []));
                var plan = ValidateOrdinaryConstMethod(function);
                view.Fields.Add(plan.Field);
                ordinaryConstMethods.Add(function, plan);
                addressOnlyPointers.Add(plan.ReceiverType);
            }
            pointerMethodsActive = pointerMethodFunctions.Count != 0 || borrowedByteViews.Count != 0;
            if (pointerMethodsActive) RequireNormalLifetimeProfile();
        }

        private static void ValidateBorrowedByteViewRecord(RecordType record)
        {
            ValidateAggregateQualifiers(record);
            if (record.Kind.Name is not ("tk_struct" or "tk_class") || record.IsIncomplete || record.IsPacked ||
                !record.HasVirtualFunctions || record.HasVirtualBases ||
                record.Size is not { } size || size <= 0 || size > int.MaxValue || record.Alignment is not { } alignment ||
                alignment != 1 && alignment != 2 && alignment != 4 && alignment != 8 ||
                record.SubobjectPartner is { } partner && !partner.Equals(record) ||
                record.Layout is not { IsComplete: true } layout || layout.NonvirtualSize != size || layout.NonvirtualAlignment != alignment)
                throw Limit("A borrowed byte-field view requires its complete ordinary-alignment layout without virtual-base or occupied-view adaptation.", "borrowed-field-view");
        }

        private bool IsBorrowedByteView(RecordType record) => borrowedByteViews.ContainsKey(record);

        private bool ContainsBorrowedViewStorage(CppType type, HashSet<CppType>? active = null)
        {
            if (borrowedByteViews.Count == 0 && borrowedMemberViews.Count == 0 && addressOnlyPointers.Count == 0) return false;
            var raw = MemoryRepresentationType(type);
            active ??= [];
            if (!active.Add(raw)) return false;
            try
            {
                return raw switch
                {
                    RecordType record => IsBorrowedByteView(record) || IsBorrowedMemberView(record) || record.Layout?.Fields.Any(field => ContainsBorrowedViewStorage(field.Field.Type, active)) == true,
                    PointerType pointer => addressOnlyPointers.Contains(pointer) || ContainsBorrowedViewStorage(pointer.PointeeType, active),
                    ArrayType array => ContainsBorrowedViewStorage(array.ElementType, active),
                    RoutineType routine => ContainsBorrowedViewStorage(routine.ReturnType, active) ||
                        routine.Parameters.Any(parameter => ContainsBorrowedViewStorage(parameter.Type, active)),
                    _ => false
                };
            }
            finally { active.Remove(raw); }
        }

        private string BorrowedByteViewName(RecordType record)
        {
            if (!ordinaryAddressUsesValidated)
                throw Limit("A borrowed field view requires completed required-use admission.", "borrowed-field-view");
            if (planned.Add(record))
            {
                records.Add(record);
                Name(record, (record.Name ?? "Record") + "_BorrowedReadView", globalNames);
            }
            return names[record];
        }

        private bool TryEmitBorrowedByteView(RecordType record, StringBuilder output)
        {
            if (!borrowedByteViews.TryGetValue(record, out var view)) return false;
            output.Append("// Borrowed read-only field view; no C++ class or virtual implementation.\n")
                .Append("[StructLayout(LayoutKind.Explicit, Size = ").Append(record.Size).Append(", Pack = ").Append(record.Alignment).AppendLine(")]")
                .Append("public unsafe struct ").Append(names[record]).AppendLine("\n{")
                .Append("    [FieldOffset(0)] public ").Append(AlignmentCarrier(record.Alignment!.Value)).AppendLine(" __borrowed_alignment;");
            foreach (var field in record.Layout!.Fields.Where(entry => view.Fields.Contains(entry.Field)))
                output.Append("    [FieldOffset(").Append(field.Offset).Append(")] public ")
                    .Append(TypeName(field.Field.Type)).Append(' ').Append(FieldName(field.Field)).AppendLine(";");
            output.AppendLine("}").AppendLine();
            return true;
        }

        private bool IsPointerMethod(FunctionSymbol function) => pointerMethodFunctions.Contains(function);

        private void CollectOrdinaryBaseViews()
        {
            if (!pointerMethodsActive) return;
            foreach (var owner in functions)
                foreach (var conversion in ExceptionExecutionNodes(owner.Body!.Root).OfType<ConversionOperation>())
                {
                    var source = GraphRepresentation(conversion.SourceType); var target = GraphRepresentation(conversion.TargetType);
                    if (conversion.OperatorKind.Name == "eok_lvalue_adjust" && source is RecordType derived && target is RecordType empty &&
                        !derived.Equals(empty) && ordinaryFieldCarriers.Contains(derived) && ordinaryFieldCarriers.Contains(empty) ||
                        conversion.OperatorKind.Name == "eok_cast" && source is PointerType from && target is PointerType to &&
                        MemoryRepresentationType(from.PointeeType) is RecordType && MemoryRepresentationType(to.PointeeType) is RecordType &&
                        GraphRepresentation(from.PointeeType) is RecordType baseType && GraphRepresentation(to.PointeeType) is RecordType complete &&
                        !baseType.Equals(complete) && ordinaryFieldCarriers.Contains(baseType) && ordinaryFieldCarriers.Contains(complete))
                        ValidateOrdinaryBaseView(conversion, owner);
                }
        }

        private void ValidateOrdinaryBaseOccurrence(ExpressionOperation expression)
        {
            if (expression is ConversionOperation conversion && ordinaryBaseViews.TryGetValue(conversion, out var owner) && !owner.Equals(currentFunction))
                throw Limit("The captured base-view occurrence belongs to a different generated body.", "ordinary-base-view");
            if (currentFunction is null || !pointerMethodsActive) return;
            if (MemoryRepresentationType(expression.Type) is RecordType record && unusedOrdinarySubobjectViews.Contains(record))
                throw Limit("An occupied-subobject partner is outside the selected complete-carrier projection.", "ordinary-method");
        }

        private void CollectPointerValueType(CppType type, HashSet<CppType>? active = null)
        {
            var raw = MemoryRepresentationType(type);
            active ??= [];
            if (!active.Add(raw)) return;
            try
            {
                switch (raw)
                {
                    case ArrayType array: CollectPointerValueType(array.ElementType, active); return;
                    case RecordType valueRecord:
                        foreach (var field in valueRecord.Layout?.Fields ?? []) CollectPointerValueType(field.Field.Type, active);
                        return;
                    case RoutineType routine:
                        CollectPointerValueType(routine.ReturnType, active);
                        foreach (var parameter in routine.Parameters) CollectPointerValueType(parameter.Type, active);
                        return;
                    case PointerType pointer:
                        var pointee = MemoryRepresentationType(pointer.PointeeType);
                        if (pointee is PointerType or RoutineType) { CollectPointerValueType(pointee, active); return; }
                        if (!pointer.IsReference && pointee is RecordType record &&
                            (record.HasVirtualFunctions || record.HasVirtualBases || IsBorrowedMemberView(record)) && !IsRegisteredVirtualCarrier(record))
                        {
                            ValidateProgramPointer(pointer); ValidateAggregateQualifiers(pointer.PointeeType);
                            addressOnlyPointers.Add(pointer);
                        }
                        return;
                }
            }
            finally { active.Remove(raw); }
        }

        private void ValidatePointerAddressUses()
        {
            // Populate signature contexts before any call-use validation. This
            // registry does not add representation candidates after the freeze.
            foreach (var function in functions)
                if (function.Parameters.Select(parameter => parameter.Type).Prepend(function.ReturnType).Any(type => ContainsAddressOnlyStorage(type)))
                    pointerValueSignatures.Add(function);
            foreach (var owner in functions)
            {
                var body = owner.Body!;
                if (owner.SpecialFunctionKind?.Name is "sfk_constructor" or "sfk_destructor" &&
                    owner.ContainingType is { } containing && (ContainsAddressOnlyStorage(containing) || ContainsBorrowedViewStorage(containing)))
                    throw Limit("Borrowed pointer carrier construction and destruction require separate lifetime admission.", "address-only-pointer");
                var signatureTypes = owner.Parameters.Select(parameter => parameter.Type).Prepend(owner.ReturnType).ToArray();
                foreach (var type in signatureTypes) RejectBorrowedValueStorage(type);
                if (signatureTypes.Any(type => ContainsAddressOnlyStorage(type)))
                {
                    RequirePointerBodyWithoutCleanup(owner);
                    if (!owner.Signature.HasPrototype || owner.Signature.IsVariadic || owner.Signature.ReturnsValueAsParameter == true ||
                        owner.Signature.ReturnsValueViaCopyConstructor || owner.Signature.ReturnValueParameterFollowsThis == true ||
                        owner.SpecialFunctionKind?.Name is "sfk_constructor" or "sfk_destructor" ||
                        owner.Parameters.Any(parameter => parameter.Role.Name is not ("ordinary_parameter" or "this_parameter") ||
                            parameter.BaseSubobject is not null || parameter.Descriptor.Details.PassedViaCopyConstructor))
                        throw Limit("Borrowed pointer values require fixed selected managed roles without lifetime or ABI adaptation.", "address-only-pointer");
                    foreach (var type in signatureTypes.Where(type => ContainsAddressOnlyStorage(type)))
                        if (GraphRepresentation(type) is not PointerType { IsReference: false, IsRValueReference: false } pointer || !addressOnlyPointers.Contains(pointer))
                            throw Limit("A borrowed pointer signature has an unsupported by-value, reference or nested-pointer role.", "address-only-pointer");
                    if (ContainsAddressOnlyStorage(owner.ReturnType) && !IsPointerMethod(owner))
                        throw Limit("A borrowed pointer result requires its selected admitted ordinary pointer body.", "address-only-pointer");
                    pointerValueSignatures.Add(owner);
                }
                foreach (var local in body.Locals)
                {
                    RejectBorrowedValueStorage(local.Type);
                    if (GraphRepresentation(local.Type) is PointerType { IsReference: false } pointer && addressOnlyPointers.Contains(pointer))
                    {
                        if (local.Details.StorageDuration?.Name != "automatic" || local.IsThreadLocal || local.LocalStaticInitializations.Count != 0)
                            throw Limit("A borrowed pointer local requires automatic captured storage.", "address-only-pointer");
                        ValidatePointerMethodLocal(owner, local);
                    }
                    else ValidateAddressOnlyStorageRole(local.Type, local.ConstantInitializer ?? local.Initializer?.Constant);
                }
                var nodes = ExceptionExecutionNodes(body.Root).ToArray();
                var parents = new Dictionary<SemanticNode, List<SemanticNode>>();
                foreach (var node in nodes)
                {
                    IEnumerable<SemanticNode> children = node switch
                    {
                        ExpressionOperation expression => expression.Operands,
                        JumpOperation { Value: { } value } => [value],
                        InitializationStatementOperation initialization when initialization.Initialization.Expression is { } value => [value],
                        ConditionalOperation conditional => new SemanticNode?[] { conditional.Condition, conditional.Then, conditional.Else }.OfType<SemanticNode>(),
                        StatementOperation { Expression: { } value } => [value],
                        _ => []
                    };
                    foreach (var child in children)
                    {
                        if (!parents.TryGetValue(child, out var consumers)) parents.Add(child, consumers = []);
                        consumers.Add(node);
                    }
                }
                foreach (var node in nodes)
                {
                    if (node is CallOperation call) ValidateBorrowedPointerCall(call);
                    if (node is not ExpressionOperation expression) continue;
                    if (MemoryRepresentationType(expression.Type) is RecordType valueRecord)
                    {
                        if (unusedOrdinarySubobjectViews.Contains(valueRecord))
                            throw Limit("The occupied-subobject view has no required expression projection.", "ordinary-method");
                        if (IsBorrowedByteView(valueRecord) || IsBorrowedMemberView(valueRecord))
                            throw Limit("The borrowed read view grants no class-valued execution or addressable class place.", "borrowed-field-view");
                        if (ContainsAddressOnlyStorage(valueRecord) && expression.ValueCategory.Name != "lvalue" && expression is not ReferenceOperation { Target: FieldSymbol })
                            throw Limit("A borrowed-pointer carrier has no by-value expression or copy adapter.", "address-only-pointer");
                    }
                    if (expression is ReferenceOperation { Target: FunctionSymbol routine } &&
                        (ordinaryMethodBodies.ContainsKey(routine) || ordinaryConstMethods.ContainsKey(routine) ||
                            AddressOnlyBoundaryType(routine.ReturnType) || routine.Signature.Parameters.Any(parameter => AddressOnlyBoundaryType(parameter.Type))) &&
                        (!parents.TryGetValue(expression, out var referenceParents) || referenceParents.Any(parent => parent is not CallOperation direct || !direct.Callee.Equals(expression))))
                        throw Limit("The selected managed pointer graph has no address-exposed or native callback adapter.", "address-only-pointer");
                    if (expression is ConversionOperation conversion && ordinaryBaseViews.ContainsKey(conversion) &&
                        conversion.OperatorKind.Name == "eok_lvalue_adjust" &&
                        (!parents.TryGetValue(expression, out var baseParents) || baseParents.Any(parent => parent is not OperatorOperation { OperatorKind.Name: "eok_address_of" })))
                        throw Limit("The fieldless base adjustment supports only its captured address transport, never a value copy.", "ordinary-base-view");
                    if (!ContainsAddressOnlyPointer(expression.Type)) continue;
                    if (GraphRepresentation(expression.Type) is not PointerType { IsReference: false, IsRValueReference: false } pointer || !addressOnlyPointers.Contains(pointer))
                        throw Limit("A captured pointer-to-pointer, array or unregistered borrowed pointer use is unsupported.", "address-only-pointer");
                    ValidateProgramPointer(pointer);
                    ValidateBorrowedPointerExpression(expression, owner, pointer);
                    if (!parents.TryGetValue(expression, out var uses) || uses.Any(parent => !IsBorrowedPointerConsumer(expression, parent, owner)))
                        throw Limit("The borrowed pointer has an unsupported dereference, store, escape or terminal consumer.", "address-only-pointer");
                    if (!addressOnlyOccurrences.TryGetValue(expression, out var owners)) addressOnlyOccurrences.Add(expression, owners = []);
                    owners.Add(owner);
                }
            }
            foreach (var item in storageOrder)
            {
                RejectBorrowedValueStorage(item.Type);
                ValidateAddressOnlyStorageRole(item.Type, item.Initializer);
            }
        }

        private void RejectBorrowedValueStorage(CppType type)
        {
            var value = MemoryRepresentationType(type);
            if (value is PointerType pointer)
            {
                if (MemoryRepresentationType(pointer.PointeeType) is RoutineType && ContainsBorrowedViewStorage(pointer))
                    throw Limit("Borrowed pointer/view roles nested in callback signatures have no storage or native adapter.", "address-only-pointer");
                return;
            }
            if (value is ArrayType array) { RejectBorrowedValueStorage(array.ElementType); return; }
            if (value is not RecordType record) return;
            if (IsBorrowedByteView(record) || IsBorrowedMemberView(record) || unusedOrdinarySubobjectViews.Contains(record))
                throw Limit("The selected borrowed field view or occupied partner has no by-value storage/ABI representation.", "borrowed-field-view");
            foreach (var field in record.Layout?.Fields ?? []) RejectBorrowedValueStorage(field.Field.Type);
        }

        private void ValidateBorrowedPointerCall(CallOperation call)
        {
            if (call.Call.FunctionType is not RoutineType signature)
                throw Limit("A required pointer-graph call has no retained routine type.", "address-only-pointer");
            if (!AddressOnlyBoundaryType(signature.ReturnType) && !signature.Parameters.Any(parameter => AddressOnlyBoundaryType(parameter.Type))) return;
            if (call.Callee is not ReferenceOperation { Target: FunctionSymbol selected } || !functions.Contains(selected) ||
                call.TargetMethod?.Equals(selected) != true || !signature.Equals(selected.Signature) || call.CallKind.Name != "direct" ||
                call.Operator.RetainedVirtualCall || call.Operator.RequiresRuntimeCastCheck || call.Call.ReturnSlot is not null)
                throw Limit("A borrowed pointer or containing carrier cannot cross an indirect or native boundary.", "address-only-pointer");
            if (ordinaryMethodBodies.TryGetValue(selected, out var method)) ValidateOrdinaryBodyCall(call, method);
            else if (ordinaryConstMethods.TryGetValue(selected, out var read)) ValidateOrdinaryConstMethodCall(call, read);
            else if (!selected.Signature.HasThisParameter)
            {
                if (!pointerValueSignatures.Contains(selected))
                    throw Limit("A selected borrowed pointer call has no registered managed signature.", "address-only-pointer");
                ValidateOrdinaryRoutineRoles(selected.Signature);
            }
            // Borrowed member-reference calls are reconciled, including all
            // binding locals, by their separate existing preflight immediately
            // after this frozen representation pass.
            else if (MemoryRepresentationType(selected.ReturnType) is not PointerType { IsReference: true })
                throw Limit("The selected borrowed pointer receiver has no admitted body adapter.", "address-only-pointer");
        }

        private void ValidateBorrowedPointerExpression(ExpressionOperation expression, FunctionSymbol owner, PointerType pointer)
        {
            switch (expression)
            {
                case ConstantOperation constant when constant.Constant.Value is IntegerValue integer && integer.Value == 0 &&
                    constant.Constant.Type?.Equals(expression.Type) == true: return;
                case ReferenceOperation { Target: FieldSymbol field } when ordinaryCarrierFields.Contains(field): return;
                case ReferenceOperation { Target: VariableSymbol variable }:
                    if (owner.Parameters.Any(parameter => parameter.Symbol?.Equals(variable) == true &&
                        GraphRepresentation(parameter.Type).Equals(pointer)) ||
                        pointerValueLocals.TryGetValue(variable, out var localOwner) && localOwner.Equals(owner) &&
                        GraphRepresentation(variable.Type).Equals(pointer)) return;
                    break;
                case CallOperation call when call.Callee is ReferenceOperation { Target: FunctionSymbol target } && IsPointerMethod(target):
                    ValidateOrdinaryBodyCall(call, ordinaryMethodBodies[target]); return;
                case OperatorOperation operation when operation.OperatorKind.Name is "eok_dot_field" or "eok_points_to_field":
                    ValidateOrdinaryGraphField(operation); return;
                case ConversionOperation conversion when conversion.OperatorKind.Name == "eok_cast":
                    if (GraphRepresentation(conversion.Operand.Type) is PointerType { IsReference: false } source &&
                        addressOnlyPointers.Contains(source) && GraphRepresentation(source.PointeeType).Equals(GraphRepresentation(pointer.PointeeType)) &&
                        (source.PointeeType is not QualifiedType { IsConst: true } || pointer.PointeeType is QualifiedType { IsConst: true }) &&
                        !conversion.Operator.RequiresRuntimeCastCheck && !conversion.Operator.IsReinterpretCast && !conversion.Operator.IsConstCast) return;
                    ValidateScalarSentinelAddress(conversion, owner); return;
                case OperatorOperation { OperatorKind.Name: "eok_assign", Operands.Count: 2 } assignment:
                    if (GraphRepresentation(assignment.Operands[0].Type).Equals(pointer) && GraphRepresentation(assignment.Operands[1].Type).Equals(pointer) &&
                        (assignment.Operands[0] is OperatorOperation { OperatorKind.Name: "eok_dot_field" or "eok_points_to_field" } ||
                         assignment.Operands[0] is ReferenceOperation { Target: VariableSymbol local } && pointerValueLocals.TryGetValue(local, out var assignedOwner) && assignedOwner.Equals(owner))) return;
                    break;
            }
            throw Limit("The pointer occurrence has no selected captured field/local/call/qualification projection.", "address-only-pointer");
        }

        private bool IsBorrowedPointerConsumer(ExpressionOperation value, SemanticNode parent, FunctionSymbol owner)
        {
            if (parent is OperatorOperation operation)
            {
                if (operation.OperatorKind.Name is "eok_eq" or "eok_ne")
                    return operation.Operands.Count == 2 && operation.Operands.All(operand => GraphRepresentation(operand.Type).Equals(GraphRepresentation(value.Type)));
                if (operation.OperatorKind.Name == "eok_not") return operation.Operands.Count == 1;
                if (operation.OperatorKind.Name == "eok_assign")
                    return operation.Operands.Count == 2 && operation.Operands.All(operand => GraphRepresentation(operand.Type).Equals(GraphRepresentation(value.Type)));
                if (operation is ConversionOperation { OperatorKind.Name: "eok_cast" } cast)
                    return GraphRepresentation(cast.Type) is PointerType castTarget && addressOnlyPointers.Contains(castTarget) &&
                        GraphRepresentation(castTarget.PointeeType).Equals(GraphRepresentation(((PointerType)GraphRepresentation(value.Type)).PointeeType));
                if (operation is CallOperation call)
                    return call.Arguments.Any(argument => argument.Value.Equals(value)) && call.Callee is ReferenceOperation { Target: FunctionSymbol callTarget } &&
                        (IsPointerMethod(callTarget) || ordinaryConstMethods.ContainsKey(callTarget) || pointerValueSignatures.Contains(callTarget));
                if (operation.OperatorKind.Name is "eok_dot_field" or "eok_points_to_field")
                {
                    if (value is ReferenceOperation { Target: FieldSymbol field }) return ordinaryCarrierFields.Contains(field) && operation.Operands[1].Equals(value);
                    return operation.OperatorKind.Name == "eok_points_to_field" && operation.Operands[0].Equals(value) &&
                        (IsBorrowedMemberAddressPlace(operation, owner) ||
                        ordinaryConstMethods.TryGetValue(owner, out var read) && IsBorrowedByteView(read.Record) &&
                        operation.Operands[1] is ReferenceOperation { Target: FieldSymbol selected } && selected.Equals(read.Field) &&
                        operation.ValueCategory.Name == "prvalue" && operation.Type.Equals(read.Function.ReturnType));
                }
            }
            if (parent is InitializationStatementOperation statement)
                return statement.Initialization.Target is VariableSymbol local && pointerValueLocals.TryGetValue(local, out var registered) &&
                    registered.Equals(owner) && statement.Initialization.Expression?.Equals(value) == true;
            if (parent is JumpOperation { JumpKind.Name: "return" })
                return IsPointerMethod(owner) && (owner.ReturnType.Equals(value.Type) || IsRegisteredOrdinaryPointerReturn(value, owner));
            return value is OperatorOperation { OperatorKind.Name: "eok_assign" } && parent is StatementOperation { Kind.Name: "stmk_expr" };
        }

        private void ValidateOrdinaryCompleteExtent(RecordType record)
        {
            if (!pointerMethodsActive)
            {
                if (record.SubobjectPartner is { } oldPartner && !oldPartner.Equals(record) ||
                    record.Layout!.NonvirtualSize != record.Size || record.Layout.NonvirtualAlignment != record.Alignment)
                    throw Limit("The ordinary carrier requires matching complete and nonvirtual storage.", "ordinary-method");
                return;
            }
            if (unusedOrdinarySubobjectViews.Contains(record))
                throw Limit("An occupied-subobject view has no required storage or execution adapter in this method graph.", "ordinary-method");
            var layout = record.Layout!;
            if (layout.NonvirtualSize is not { } occupied || occupied <= 0 || occupied > record.Size || layout.NonvirtualAlignment != record.Alignment)
                throw Limit("The selected complete carrier requires captured complete/nonvirtual extents and alignment.", "ordinary-method");
            if (record.SubobjectPartner is { } retainedPartner && !retainedPartner.Equals(record))
            {
                if (retainedPartner is not RecordType partner || partner.SubobjectPartner?.Equals(record) != true || partner.Size != occupied || partner.Alignment != record.Alignment ||
                    occupied >= record.Size || partner.IsIncomplete || partner.HasVirtualFunctions || partner.HasVirtualBases)
                    throw Limit("The selected complete carrier has an unsupported occupied-view relationship.", "ordinary-method");
                unusedOrdinarySubobjectViews.Add(partner);
            }
        }

        private void ValidatePointerMethodLocal(FunctionSymbol function, VariableSymbol local)
        {
            ValidateAggregateQualifiers(local.Type);
            if (GraphRepresentation(local.Type) is not PointerType { IsReference: false, IsRValueReference: false } pointer)
            {
                ValidateGraphBoolean(local.Type);
                if (local.Initializer is not null || local.ConstantInitializer is not null)
                    throw Limit("An ordinary Boolean local requires its retained assignment occurrence.", "ordinary-method");
                return;
            }
            ValidateProgramPointer(pointer);
            if (local.ConstantInitializer is not null)
                throw Limit("Pointer-method locals require explicit executable initialization or retained assignments.", "ordinary-method");
            if (local.Initializer is { } initialization)
            {
                var statements = ExceptionExecutionNodes(function.Body!.Root).OfType<InitializationStatementOperation>()
                    .Where(statement => statement.Initialization.Equals(initialization)).Distinct().ToArray();
                if (statements.Length != 1) throw Limit("A pointer local requires its reciprocal executable initialization statement.", "ordinary-method");
                _ = ValidatePointerInitialization(statements[0], function);
                if (initialization.Target?.Equals(local) != true || initialization.Details.Type?.Equals(local.Type) != true ||
                    initialization.Expression is not ExpressionOperation value || !GraphRepresentation(value.Type).Equals(pointer) &&
                    !IsRegisteredOrdinaryPointerInitialization(initialization, function, local, value))
                    throw Limit("The pointer local and its actual initializer have different captured value views.", "ordinary-method");
            }
            pointerValueLocals[local] = function;
        }

        private bool ValidatePointerMethodNode(SemanticNode node, OrdinaryMethodBody plan)
        {
            if (!IsPointerMethod(plan.Function)) return false;
            switch (node)
            {
                case DeclarationOperation: return true;
                case InitializationStatementOperation initialization:
                    if (initialization.Initialization.Target is not VariableSymbol local ||
                        !pointerValueLocals.TryGetValue(local, out var owner) || !owner.Equals(plan.Function))
                        throw Limit("An ordinary pointer initializer must name its registered automatic local.", "ordinary-method");
                    _ = ValidatePointerInitialization(initialization, plan.Function);
                    return true;
                case ConditionalOperation { Condition: { } condition }:
                    if (!IsGraphBooleanScalar(condition.Type))
                        throw Limit("An ordinary pointer branch requires its retained scalar condition.", "ordinary-method");
                    return true;
            }
            return false;
        }

        private ExpressionOperation ValidatePointerInitialization(InitializationStatementOperation statement, FunctionSymbol owner)
        {
            var initialization = statement.Initialization; var flags = initialization.Details;
            if (statement.RepresentationRole.Name != "execution" || initialization.Kind.Name != "dik_expression" ||
                initialization.Operation is not { RepresentationRole.Name: "execution" } operation || !operation.Initialization.Equals(initialization) ||
                initialization.Target is not VariableSymbol local || owner.Body!.Locals.Contains(local) != true ||
                local.Details.StorageDuration?.Name != "automatic" || local.Initializer?.Equals(initialization) != true ||
                flags.Type?.Equals(local.Type) != true || initialization.Expression is not ExpressionOperation { RepresentationRole.Name: "execution" } value ||
                GraphRepresentation(local.Type) is not PointerType { IsReference: false, IsRValueReference: false } pointer ||
                initialization.Constructor is not null || initialization.Destructor is not null || initialization.Constant is not null ||
                initialization.Lambda is not null || initialization.BitwiseCopySource is not null || flags.Source is not null || initialization.Operands.Count != 0 ||
                initialization.Lifetime is not null || initialization.InitExpressionLifetime is not null || initialization.OverlappingLifetime is not null ||
                initialization.Master is not null || initialization.NextInDestructionList is not null ||
                flags.IsUnordered || flags.IsConditional || flags.IsReusedValue || flags.IsOverlapping || flags.IsClassResultMaster || flags.IsStaticTemporary ||
                flags.IsPartialAggregate || flags.IsPartialInitialization || flags.HasTemporaryLifetime || flags.IsConstructorInitialization ||
                flags.ConstructorCopyImpliedSource || flags.ConstructorImplicitCopy || flags.ConstructorValueInitialization || flags.ConstructorSequencedArguments ||
                flags.ConstructorArrayCopy || flags.IsQuestionResult || flags.IsCommaResult || flags.FreesStorage || flags.FreesArray || flags.IsConditionalGuard ||
                flags.IsVlaDeallocation || flags.IsFreeingOfExceptionObject || flags.CreatesInitializerList || flags.IsInitializerListArray)
                throw Limit("An ordinary pointer local requires its reciprocal executable value, allowing only its captured const pointer-object declaration view.", "ordinary-method");
            ValidateProgramPointer(pointer);
            var exactValue = (value.Type.Equals(local.Type) || local.Type is QualifiedType { IsConst: true, IsVolatile: false } pointerObject &&
                pointerObject.UnderlyingType.Equals(value.Type)) && GraphRepresentation(value.Type).Equals(pointer);
            if (!exactValue && !RegisterOrdinaryPointerInitialization(initialization, owner, local, value))
                throw Limit("The pointer local requires its exact value type or its selected direct-call initializer alias view.", "ordinary-method");
            return value;
        }

        private bool ValidatePointerMethodExpression(ExpressionOperation expression, OrdinaryMethodBody plan)
        {
            if (!IsPointerMethod(plan.Function)) return false;
            switch (expression)
            {
                case ReferenceOperation { Operands.Count: 0, Target: VariableSymbol variable }:
                    if (variable.Equals(plan.Receiver) ? !expression.Type.Equals(plan.ReceiverType) :
                        !plan.Function.Body!.Locals.Contains(variable) || !GraphRepresentation(expression.Type).Equals(GraphRepresentation(variable.Type)))
                        throw Limit("A pointer-method variable must be its selected receiver or registered scalar/pointer local.", "ordinary-method");
                    return true;
                case ReferenceOperation { Operands.Count: 0, Target: FieldSymbol }: return true;
                case ReferenceOperation { Operands.Count: 0, Target: FunctionSymbol function }:
                    if (!functions.Contains(CanonicalFunction(function)) && function.BuiltinKind?.Name != "bfk_expect")
                        throw Limit("An ordinary pointer body has an unimplemented selected routine reference.", "ordinary-method");
                    return true;
                case ConstantOperation constant when constant.Constant.Value is IntegerValue &&
                    (IsPointerGraphScalar(expression.Type) || GraphRepresentation(expression.Type) is PointerType): return true;
                case CallOperation call:
                    ValidatePointerGraphCall(call);
                    return true;
                case ConversionOperation conversion when conversion.OperatorKind.Name == "eok_lvalue_adjust":
                    ValidateOrdinaryBaseView(conversion, plan.Function);
                    return true;
                case ConversionOperation conversion when conversion.OperatorKind.Name == "eok_cast":
                    if (IsPointerGraphScalar(conversion.Type) && IsPointerGraphScalar(conversion.Operand.Type)) return true;
                    if (GraphRepresentation(conversion.Type) is PointerType target && GraphRepresentation(conversion.Operand.Type) is PointerType source)
                    {
                        ValidateProgramPointer(target); ValidateProgramPointer(source);
                        if (GraphRepresentation(target.PointeeType).Equals(GraphRepresentation(source.PointeeType))) return true;
                        ValidateOrdinaryBaseView(conversion, plan.Function);
                        return true;
                    }
                    break;
                case OperatorOperation { OperatorKind.Name: "eok_assign", Operands.Count: 2 } assignment:
                    if (!assignment.Operator.EvalRightToLeft || assignment.Operator.EvalLeftToRight ||
                        assignment.Operands[0] is not ReferenceOperation { Target: VariableSymbol local } ||
                        !plan.Function.Body!.Locals.Contains(local) ||
                        !GraphRepresentation(assignment.Type).Equals(GraphRepresentation(local.Type)) ||
                        !GraphRepresentation(assignment.Operands[1].Type).Equals(GraphRepresentation(local.Type)))
                        throw Limit("An ordinary pointer result assignment requires its captured right-to-left local store.", "ordinary-method");
                    return true;
                case OperatorOperation { OperatorKind.Name: "eok_not", Operands.Count: 1 } operation:
                    if (!IsGraphBooleanScalar(operation.Type) || !IsPointerGraphScalar(operation.Operands[0].Type) &&
                        GraphRepresentation(operation.Operands[0].Type) is not PointerType)
                        throw Limit("An ordinary pointer truth operation requires its captured scalar or pointer value.", "ordinary-method");
                    return true;
                case OperatorOperation comparison when comparison.OperatorKind.Name is "eok_eq" or "eok_ne":
                    if (comparison.Operands.Count != 2 || !IsGraphBooleanScalar(comparison.Type) ||
                        !comparison.Operands[0].Type.Equals(comparison.Operands[1].Type) ||
                        !IsPointerGraphScalar(comparison.Operands[0].Type) && GraphRepresentation(comparison.Operands[0].Type) is not PointerType)
                        throw Limit("An ordinary pointer-body comparison requires exact captured scalar/pointer operand types.", "ordinary-method");
                    return true;
            }
            return false;
        }

        private static bool IsPointerGraphScalar(CppType type) => MemoryRepresentationType(type) is ArithmeticType arithmetic &&
            (IsGraphBooleanScalar(type) || arithmetic.Arithmetic.Bits == 64 && arithmetic.Arithmetic.Signed == true && type.Size == 8 && type.Alignment == 8);

        private void ValidatePointerGraphCall(CallOperation call)
        {
            if (IsSupportedConstantHintExpectation(call)) return;
            if (call.Callee is not ReferenceOperation { Operands.Count: 0, Target: FunctionSymbol target } ||
                !functions.Contains(target) || call.TargetMethod?.Equals(target) != true || call.CallKind.Name != "direct" ||
                call.Call.FunctionType?.Equals(target.Signature) != true || !call.Type.Equals(target.ReturnType) ||
                call.Call.ReturnSlot is not null || !call.Operator.EvalLeftToRight || call.Operator.EvalRightToLeft ||
                call.SourceDispatchKind is not null || call.SourceTargetMethod is not null || call.Operator.RetainedVirtualCall ||
                call.Operator.RequiresRuntimeCastCheck || call.Operands.Count != call.Arguments.Count + 1 ||
                !call.Operands[0].Equals(call.Callee) || call.Arguments.Count != target.Signature.Parameters.Count)
                throw Limit("An ordinary pointer graph requires actual direct defined calls with exact retained signature and argument order.", "ordinary-method");
            if (ordinaryMethodBodies.TryGetValue(target, out var method)) ValidateOrdinaryBodyCall(call, method);
            else
            {
                if (target.Signature.HasThisParameter || NeedsGeneratedLifetimeRoles(target.Signature) || !target.Signature.HasPrototype || target.Signature.IsVariadic)
                    throw Limit("An ordinary pointer graph call has an unadmitted receiver/reference/result role.", "ordinary-method");
                ValidateOrdinaryRoutineRoles(target.Signature); ValidateCallingConvention(target.Signature);
                RequirePointerBodyWithoutCleanup(target);
                if (!IsPointerGraphScalar(target.ReturnType) && MemoryRepresentationType(target.ReturnType).Kind.Name != "tk_void" ||
                    target.Parameters.Any(parameter => !IsPointerGraphScalar(parameter.Type)))
                    throw Limit("An ordinary pointer graph helper requires its selected ordinary scalar signature.", "ordinary-method");
            }
            for (var index = 0; index < call.Arguments.Count; ++index)
            {
                var argument = call.Arguments[index]; var parameter = target.Signature.Parameters[index];
                if (argument.Ordinal != index || !argument.Value.Equals(call.Operands[index + 1]) ||
                    argument.Parameter?.Equals(parameter.Descriptor) != true || argument.Role?.Equals(parameter.Role) != true)
                    throw Limit("An ordinary pointer graph call changes its actual raw/typed argument aliases.", "ordinary-method");
            }
        }

        private static void RequirePointerBodyWithoutCleanup(FunctionSymbol function)
        {
            var body = function.Body!; var eh = body.ExceptionHandling;
            if (body.Lifetimes.Count != 0 || eh.RegionTable is not null || eh.ArrayTable is not null || eh.CleanupRegions.Count != 0 ||
                eh.Arrays.Count != 0 || eh.Details?.Prologue is not null || eh.Epilogues.Count != 0 ||
                eh.Details?.InitialState is not null || eh.Details?.NullState is not null ||
                ExceptionExecutionNodes(body.Root).Any(node => node is ThrowOperation or TryOperation or LoweredEhOperation or LifetimeOperation))
                throw Limit("An ordinary pointer graph body has unsupported lifetime or exception execution obligations.", "ordinary-method");
        }

        private bool IsZeroEmptyBase(RecordType derived, RecordType empty)
        {
            if (!ordinaryFieldCarriers.Contains(derived) || !ordinaryFieldCarriers.Contains(empty) || !empty.IsEmpty ||
                empty.Layout!.Fields.Count != 0) return false;
            return derived.Bases.Any(direct => !direct.IsVirtual && GraphRepresentation(direct.BaseType).Equals(empty) &&
                direct.Subobject is { IsDirect: true, IsVirtual: false, IsAmbiguous: false, IsOptimizedEmptyBase: true } subobject &&
                subobject.Offset == 0 && subobject.MostDerivedType?.Equals(derived) == true && derived.BaseSubobjects.Contains(subobject));
        }

        private void ValidateOrdinaryBaseView(ConversionOperation conversion, FunctionSymbol owner)
        {
            if (conversion.Operands.Count != 1 || conversion.RepresentationRole.Name != "execution" || conversion.Details.VolatileFetch ||
                conversion.Operator.RequiresRuntimeCastCheck || conversion.Operator.RetainedVirtualCall ||
                conversion.Operator.EvalLeftToRight || conversion.Operator.EvalRightToLeft)
                throw Limit("A captured empty-base view has unsupported execution or adjustment flags.", "ordinary-base-view");
            var source = GraphRepresentation(conversion.SourceType); var target = GraphRepresentation(conversion.TargetType);
            if (conversion.OperatorKind.Name == "eok_lvalue_adjust" && source is RecordType derived && target is RecordType empty &&
                conversion.ValueCategory.Name == "lvalue" && conversion.Operand.ValueCategory.Name == "lvalue" && IsZeroEmptyBase(derived, empty))
            { ordinaryBaseViews[conversion] = owner; return; }
            if (conversion.OperatorKind.Name == "eok_cast" && source is PointerType from && target is PointerType to &&
                from.PointeeType is QualifiedType { IsConst: true, IsVolatile: false } && to.PointeeType is QualifiedType { IsConst: true, IsVolatile: false } &&
                GraphRepresentation(from.PointeeType) is RecordType baseType && GraphRepresentation(to.PointeeType) is RecordType complete &&
                IsZeroEmptyBase(complete, baseType) && conversion.Operand is ReferenceOperation { Target: VariableSymbol receiver } &&
                owner.Parameters.FirstOrDefault()?.Symbol?.Equals(receiver) == true && IsPointerMethod(owner))
            { ordinaryBaseViews[conversion] = owner; return; }
            throw Limit("Only the captured zero-offset empty-base place and selected const-receiver reverse cast are supported.", "ordinary-base-view");
        }

        private bool TryPlanOrdinaryBaseView(ConversionOperation conversion, out ExpressionPlan result)
        {
            result = null!;
            if (conversion.OperatorKind.Name != "eok_lvalue_adjust" || !ordinaryBaseViews.TryGetValue(conversion, out var owner)) return false;
            if (!owner.Equals(currentFunction)) throw Limit("The empty-base place is outside its selected generated owner.", "ordinary-base-view");
            var source = PlanExpression(conversion.Operand);
            var place = source.Place ?? throw Limit("The captured empty-base adjustment requires an actual source place.", "ordinary-base-view");
            var address = new CsCast(CsType.Address(conversion.Type), new CsAddress(CsType.Address(source.Type), place));
            var target = MemoryPlace(conversion.Type, LValueKind.Address, address);
            result = new(conversion.Type, conversion.ValueCategory, source.Prefix, new CsLoad(target), source.Effects, target);
            return true;
        }

        private bool TryLegacyOrdinaryBaseView(ConversionOperation conversion, out string result)
        {
            result = "";
            if (conversion.OperatorKind.Name != "eok_lvalue_adjust" || !ordinaryBaseViews.TryGetValue(conversion, out var owner)) return false;
            if (!owner.Equals(currentFunction)) throw Limit("The empty-base place is outside its selected generated owner.", "ordinary-base-view");
            result = $"(*(({TypeName(conversion.Type)}*)(&({Expression(conversion.Operand)}))))";
            return true;
        }
    }
}
