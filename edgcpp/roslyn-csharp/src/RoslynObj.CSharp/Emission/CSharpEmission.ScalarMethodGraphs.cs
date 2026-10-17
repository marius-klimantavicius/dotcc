using System.Text;
using RoslynObj;

namespace RoslynObj.CSharp;

public sealed partial class CSharpEmitter
{
    private sealed partial class Emission
    {
        private readonly HashSet<FunctionSymbol> ordinaryScalarMethods = [];
        private readonly Dictionary<SemanticNode, HashSet<FunctionSymbol>> ordinaryViewOwners = [];
        private readonly Dictionary<ExpressionOperation, HashSet<FunctionSymbol>> ordinaryViewOccurrences = [];
        private readonly Dictionary<CallOperation, FunctionSymbol> ordinaryPointerCallViews = [];
        private readonly Dictionary<ExpressionOperation, HashSet<FunctionSymbol>> ordinaryPointerReturnViews = [];
        private sealed record OrdinaryPointerInitialization(FunctionSymbol Owner, VariableSymbol Variable, ExpressionOperation Value);
        private readonly Dictionary<Initialization, OrdinaryPointerInitialization> ordinaryPointerInitializations = [];
        private readonly Dictionary<RecordType, HashSet<FieldSymbol>> borrowedMemberViews = [];
        private readonly Dictionary<FunctionSymbol, FieldSymbol> borrowedMemberAddressMethods = [];
        private bool borrowedMemberViewsValidated;

        private bool IsOrdinaryScalarMethod(FunctionSymbol function) => ordinaryScalarMethods.Contains(function);

        private void CollectOrdinaryScalarMethods()
        {
            foreach (var owner in functions.Where(owner => owner.Signature.HasThisParameter &&
                owner.SpecialFunctionKind?.Name == "sfk_none" && !IsVirtualMethod(owner) &&
                MemoryRepresentationType(owner.ReturnType) is ArithmeticType { Arithmetic.IsBool: false, Arithmetic.Bits: 8, Arithmetic.Signed: false } &&
                owner.ContainingType is RecordType { HasVirtualFunctions: false, HasVirtualBases: false }))
            {
                var body = owner.Body!;
                var nodes = ExceptionExecutionNodes(body.Root).ToArray();
                if (body.Locals.Count != 0 || nodes.Any(node => node is CallOperation or ConditionalOperation))
                    ordinaryScalarMethods.Add(owner);
            }
            CollectBorrowedMemberViews();
            if (ordinaryScalarMethods.Count == 0 && !pointerMethodsActive && !functions.Any(function =>
                function.Signature.HasThisParameter && !IsVirtualMethod(function) &&
                function.SpecialFunctionKind?.Name is "sfk_none" or "sfk_conversion" &&
                MemoryRepresentationType(function.ReturnType) is ArithmeticType { Arithmetic.IsBool: true })) return;
            foreach (var owner in functions)
            {
                foreach (var node in ExceptionExecutionNodes(owner.Body!.Root))
                {
                    if (!ordinaryViewOwners.TryGetValue(node, out var owners)) ordinaryViewOwners.Add(node, owners = []);
                    owners.Add(owner);
                }
            }
        }

        private void ValidateOrdinaryScalarMethodBodies()
        {
            foreach (var function in ordinaryScalarMethods)
            {
                var plan = ordinaryMethodBodies[function];
                var body = function.Body!;
                RequirePointerBodyWithoutCleanup(function);
                foreach (var local in body.Locals)
                {
                    if (IsRegisteredBorrowedLocal(function, local)) continue;
                    if (local.Details.StorageDuration?.Name != "automatic" || local.IsThreadLocal || local.LocalStaticInitializations.Count != 0 ||
                        !IsGraphBooleanScalar(local.Type) || local.Initializer is not null || local.ConstantInitializer is not null)
                        throw Limit("An ordinary scalar body local requires its exact borrowed binding or an ordinary retained scalar assignment.", "ordinary-method");
                }
                foreach (var node in ExceptionExecutionNodes(body.Root))
                {
                    if (node.RepresentationRole.Name != "execution")
                        throw Limit("An ordinary scalar body requires retained execution occurrences.", "ordinary-method");
                    switch (node)
                    {
                        case BlockOperation { Lifetime: null }: break;
                        case DeclarationOperation: break;
                        case InitializationStatementOperation statement when IsRegisteredBorrowedInitialization(function, statement.Initialization): break;
                        case InitializationOperation initialization when IsRegisteredBorrowedInitialization(function, initialization.Initialization): break;
                        case ConditionalOperation { Condition: { } condition } when IsGraphBooleanScalar(condition.Type): break;
                        case StatementOperation { Kind.Name: "stmk_expr", Expression: not null }: break;
                        case JumpOperation { JumpKind.Name: "return", Target: null, CommonLifetime: null, Value: { } result } when result.Type.Equals(function.ReturnType): break;
                        case ExpressionOperation expression: ValidateOrdinaryScalarExpression(expression, plan); break;
                        default: throw Limit($"Execution '{node.Kind.Name}' has no ordinary scalar composition projection.", "ordinary-method");
                    }
                }
            }
            borrowedMemberViewsValidated = true;
        }

        private void CollectBorrowedMemberViews()
        {
            foreach (var scalar in ordinaryScalarMethods)
            {
                var record = (RecordType)scalar.ContainingType!;
                if (IsRegisteredVirtualCarrier(record) || ordinaryFieldCarriers.Contains(record)) continue;
                foreach (var getter in functions.Where(function => function.ContainingType?.Equals(record) == true &&
                    function.Signature.HasThisParameter && function.SpecialFunctionKind?.Name == "sfk_none" && !IsVirtualMethod(function) &&
                    function.ReturnType is PointerType { IsReference: true, IsRValueReference: false }))
                {
                    if (getter.Body!.Root is not BlockOperation { Statements.Count: 1 } block ||
                        block.Statements[0] is not JumpOperation { JumpKind.Name: "return", Value: OperatorOperation { OperatorKind.Name: "eok_address_of", Operands.Count: 1 } address }) continue;
                    var place = address.Operands[0];
                    if (place is ConversionOperation { OperatorKind.Name: "eok_lvalue_adjust", Operands.Count: 1 } qualified) place = qualified.Operand;
                    if (place is not OperatorOperation { OperatorKind.Name: "eok_points_to_field", Operands.Count: 2 } fieldPlace ||
                        fieldPlace.Operands[1] is not ReferenceOperation { Target: FieldSymbol field } || !field.ContainingType.Equals(record)) continue;
                    ValidateAggregateQualifiers(record);
                    if (record.Kind.Name is not ("tk_struct" or "tk_class") || record.IsIncomplete || record.IsPacked || record.Bases.Count != 0 || record.BaseSubobjects.Count != 0 ||
                        record.HasVirtualFunctions || record.HasVirtualBases || record.VirtualFunctionTableVariable is not null || record.VirtualTableTableVariable is not null ||
                        record.SubobjectPartner is { } partner && !partner.Equals(record) ||
                        record.Size is not { } size || size <= 0 || size > int.MaxValue || record.Alignment is not { } alignment ||
                        alignment != 1 && alignment != 2 && alignment != 4 && alignment != 8 ||
                        record.Layout is not { IsComplete: true } layout || layout.NonvirtualSize != size || layout.NonvirtualAlignment != alignment ||
                        field.IsGeneratedBase || field.IsGeneratedVirtualPointer == true || field.IsBitField || field.IsPacked == true ||
                        MemoryRepresentationType(field.Type) is not RecordType member || field.Type.Size != member.Size || field.Type.Alignment != member.Alignment ||
                        layout.Fields.Count(entry => entry.Field.Equals(field) && entry.BitWidth is null && entry.Offset is { } offset && offset >= 0 &&
                            offset % member.Alignment == 0 && offset + member.Size <= size) != 1)
                        throw Limit("A borrowed member-address view requires its captured complete owner extent and exact ordinary in-bounds field layout.", "borrowed-member-view");
                    RegisterOrdinaryFieldCarrier(member, []);
                    if (!borrowedMemberViews.TryGetValue(record, out var fields)) borrowedMemberViews.Add(record, fields = []);
                    fields.Add(field);
                    if (borrowedMemberAddressMethods.TryGetValue(getter, out var prior) && !prior.Equals(field))
                        throw Limit("A borrowed member-address method has different selected field projections.", "borrowed-member-view");
                    borrowedMemberAddressMethods[getter] = field;
                }
            }
            if (borrowedMemberViews.Count != 0) pointerMethodsActive = true;
        }

        private bool IsBorrowedMemberView(RecordType record) => borrowedMemberViews.ContainsKey(record);

        private bool IsBorrowedMemberAddressPlace(OperatorOperation operation, FunctionSymbol owner) =>
            borrowedMemberAddressMethods.TryGetValue(owner, out var field) && operation.OperatorKind.Name == "eok_points_to_field" &&
            operation.Operands.Count == 2 && operation.Operands[1] is ReferenceOperation { Target: FieldSymbol selected } && selected.Equals(field) &&
            operation.ValueCategory.Name == "lvalue" && GraphRepresentation(operation.Type).Equals(GraphRepresentation(field.Type));

        private string BorrowedMemberViewName(RecordType record)
        {
            if (!ordinaryAddressUsesValidated || !borrowedMemberViewsValidated)
                throw Limit("The borrowed member-address view requires completed selected field and required-use admission.", "borrowed-member-view");
            if (planned.Add(record)) { records.Add(record); Name(record, (record.Name ?? "Record") + "_BorrowedMemberView", globalNames); }
            return names[record];
        }

        private bool TryEmitBorrowedMemberView(RecordType record, StringBuilder output)
        {
            if (!borrowedMemberViews.TryGetValue(record, out var fields)) return false;
            output.Append("// Borrowed member-address view; no complete C++ value or lifetime implementation.\n")
                .Append("[StructLayout(LayoutKind.Explicit, Size = ").Append(record.Size).Append(", Pack = ").Append(record.Alignment).AppendLine(")]")
                .Append("public unsafe struct ").Append(names[record]).AppendLine("\n{")
                .Append("    [FieldOffset(0)] public ").Append(AlignmentCarrier(record.Alignment!.Value)).AppendLine(" __borrowed_alignment;");
            foreach (var entry in record.Layout!.Fields.Where(entry => fields.Contains(entry.Field)))
                output.Append("    [FieldOffset(").Append(entry.Offset).Append(")] public ").Append(TypeName(entry.Field.Type)).Append(' ').Append(FieldName(entry.Field)).AppendLine(";");
            output.AppendLine("}").AppendLine();
            return true;
        }

        private void ValidateOrdinaryScalarExpression(ExpressionOperation expression, OrdinaryMethodBody plan)
        {
            if (expression.Details.VolatileFetch || expression.RepresentationRole.Name != "execution")
                throw Limit("The ordinary scalar graph has an unsupported volatile or metadata expression.", "ordinary-method");
            ValidateAggregateQualifiers(expression.Type);
            switch (expression)
            {
                case ReferenceOperation { Operands.Count: 0, Target: VariableSymbol variable }:
                    if (variable.Equals(plan.Receiver) && expression.Type.Equals(plan.ReceiverType) ||
                        IsRegisteredBorrowedLocalRead(plan.Function, variable, expression) ||
                        plan.Function.Body!.Locals.Contains(variable) && variable.Type.Equals(expression.Type) && IsGraphBooleanScalar(expression.Type)) return;
                    break;
                case ReferenceOperation { Operands.Count: 0, Target: FunctionSymbol function }:
                    if (ordinaryMethodBodies.ContainsKey(function) || ordinaryConstMethods.ContainsKey(function) || IsBorrowedFieldMethod(function)) return;
                    break;
                case ConstantOperation constant when constant.Constant.Value is IntegerValue &&
                    (constant.Type.Equals(plan.Function.ReturnType) || IsGraphBooleanScalar(constant.Type)): return;
                case CallOperation call:
                    if (call.Callee is not ReferenceOperation { Target: FunctionSymbol target }) break;
                    if (ordinaryMethodBodies.TryGetValue(target, out var selected)) { ValidateOrdinaryBodyCall(call, selected); return; }
                    if (ordinaryConstMethods.TryGetValue(target, out var read)) { ValidateOrdinaryConstMethodCall(call, read); return; }
                    if (IsRegisteredBorrowedBindingCall(plan.Function, call, target)) return;
                    break;
                case ConversionOperation conversion when conversion.OperatorKind.Name == "eok_cast":
                    if (conversion.Operands.Count != 1 || conversion.Operator.RequiresRuntimeCastCheck || conversion.Operator.RetainedVirtualCall ||
                        conversion.Operator.IsReinterpretCast || conversion.Operator.IsConstCast) break;
                    if (IsGraphBooleanScalar(conversion.Type) && IsGraphBooleanScalar(conversion.Operand.Type)) return;
                    if (SameOrdinaryPointerView(conversion.Operand.Type, conversion.Type, allowAddedConst: true))
                    { RegisterOrdinaryView(conversion, plan.Function); return; }
                    break;
                case OperatorOperation { OperatorKind.Name: "eok_not", Operands.Count: 1 } operation when IsGraphBooleanScalar(operation.Type):
                    if (IsGraphBooleanScalar(operation.Operands[0].Type) || MemoryRepresentationType(operation.Operands[0].Type) is PointerType { IsReference: false, IsRValueReference: false }) return;
                    break;
            }
            // Keep the existing Boolean assignment/comparison grammar rather
            // than synthesizing a getter, branch or pointer truth test.
            ValidateOrdinaryMethodExpression(expression, plan);
        }

        private static bool SameOrdinaryPointerView(CppType actualType, CppType selectedType, bool allowAddedConst = false)
        {
            if (GraphRepresentation(actualType) is not PointerType { IsReference: false, IsRValueReference: false } actual ||
                GraphRepresentation(selectedType) is not PointerType { IsReference: false, IsRValueReference: false } selected) return false;
            ValidateProgramPointer(actual); ValidateProgramPointer(selected);
            var actualRecord = OrdinaryPointerPointee(actual.PointeeType, out var actualConst);
            var selectedRecord = OrdinaryPointerPointee(selected.PointeeType, out var selectedConst);
            return actualRecord is not null && actualRecord.Equals(selectedRecord) &&
                (actualConst == selectedConst || allowAddedConst && !actualConst && selectedConst);
        }

        private static RecordType? OrdinaryPointerPointee(CppType type, out bool isConst)
        {
            isConst = false;
            var size = type.Size; var alignment = type.Alignment;
            for (var depth = 0; depth <= 32; ++depth)
            {
                ValidateAggregateQualifiers(type);
                if (type.Size != size || type.Alignment != alignment || type.IsIncomplete || type.IsDependent) return null;
                if (type is QualifiedType qualified) isConst |= qualified.IsConst;
                if (type is not TypeReference reference) return type as RecordType;
                type = reference.UnderlyingType;
            }
            throw Limit("The ordinary pointer alias view exceeds the supported captured wrapper depth.", "ordinary-method");
        }

        private void RegisterOrdinaryView(ExpressionOperation expression, FunctionSymbol owner)
        {
            if (!ordinaryViewOwners.TryGetValue(expression, out var actualOwners) || !actualOwners.Contains(owner))
                throw Limit("An ordinary pointer view has no actual selected expression and owning body.", "ordinary-method");
            if (!ordinaryViewOccurrences.TryGetValue(expression, out var owners)) ordinaryViewOccurrences.Add(expression, owners = []);
            owners.Add(owner);
        }

        private bool RegisterOrdinaryPointerReturn(FunctionSymbol owner, SemanticNode statement, ExpressionOperation result)
        {
            if (!IsPointerMethod(owner) || statement is not JumpOperation { JumpKind.Name: "return", Value: { } value } ||
                !value.Equals(result) || !SameOrdinaryPointerView(result.Type, owner.ReturnType)) return false;
            RegisterOrdinaryView(result, owner);
            if (!ordinaryPointerReturnViews.TryGetValue(result, out var owners)) ordinaryPointerReturnViews.Add(result, owners = []);
            owners.Add(owner);
            return true;
        }

        private bool IsRegisteredOrdinaryPointerReturn(ExpressionOperation result, FunctionSymbol owner) =>
            ordinaryPointerReturnViews.TryGetValue(result, out var owners) && owners.Contains(owner);

        private bool RegisterOrdinaryPointerCall(CallOperation call, FunctionSymbol target)
        {
            if (!IsPointerMethod(target) || !SameOrdinaryPointerView(target.ReturnType, call.Type) ||
                call.TargetMethod?.Equals(target) != true || call.Call.FunctionType?.Equals(target.Signature) != true ||
                !ordinaryViewOwners.TryGetValue(call, out var owners)) return false;
            foreach (var owner in owners) RegisterOrdinaryView(call, owner);
            ordinaryPointerCallViews[call] = target;
            return true;
        }

        private bool IsRegisteredOrdinaryPointerCall(CallOperation call, FunctionSymbol target) =>
            ordinaryPointerCallViews.TryGetValue(call, out var selected) && selected.Equals(target) &&
            currentFunction is { } owner && ordinaryViewOccurrences.TryGetValue(call, out var owners) && owners.Contains(owner);

        private bool RegisterOrdinaryPointerInitialization(Initialization initialization, FunctionSymbol owner,
            VariableSymbol variable, ExpressionOperation value)
        {
            if (value is not CallOperation { Callee: ReferenceOperation { Target: FunctionSymbol target } } call ||
                !IsPointerMethod(target) || !ordinaryMethodBodies.TryGetValue(target, out var method) ||
                !SameOrdinaryPointerView(value.Type, variable.Type) ||
                !ordinaryViewOwners.TryGetValue(value, out var owners) || !owners.Contains(owner)) return false;
            ValidateOrdinaryBodyCall(call, method);
            var selected = new OrdinaryPointerInitialization(owner, variable, value);
            if (ordinaryPointerInitializations.TryGetValue(initialization, out var previous) && !previous.Equals(selected))
                throw Limit("A pointer initializer alias has different retained local, call or function owners.", "ordinary-method");
            ordinaryPointerInitializations[initialization] = selected;
            RegisterOrdinaryView(value, owner);
            return true;
        }

        private bool IsRegisteredOrdinaryPointerInitialization(Initialization initialization, FunctionSymbol owner,
            VariableSymbol variable, ExpressionOperation value) =>
            ordinaryPointerInitializations.TryGetValue(initialization, out var selected) && selected.Owner.Equals(owner) &&
            selected.Variable.Equals(variable) && selected.Value.Equals(value);

        private void RegisterOrdinaryReceiverView(CallOperation call, FunctionSymbol target, ExpressionOperation receiver, PointerType selectedType)
        {
            if (receiver.Type.Equals(selectedType)) return;
            if (!SameOrdinaryPointerView(receiver.Type, selectedType) || call.Receiver?.Equals(receiver) != true ||
                call.TargetMethod?.Equals(target) != true || !ordinaryViewOwners.TryGetValue(call, out var owners))
                throw Limit("The ordinary receiver requires its exact retained alias-qualified pointer view and selected call context.", "ordinary-method");
            foreach (var owner in owners) RegisterOrdinaryView(receiver, owner);
        }

        private void ValidateOrdinaryViewOccurrence(ExpressionOperation expression)
        {
            if (ordinaryViewOccurrences.TryGetValue(expression, out var owners) && (currentFunction is null || !owners.Contains(currentFunction)))
                throw Limit("An ordinary pointer view is outside its validated selected expression and owner.", "ordinary-method");
        }
    }
}
