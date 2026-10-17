namespace RoslynObj.CSharp;

public sealed partial class CSharpEmitter
{
    private sealed partial class Emission
    {
        private int expressionTemporaryOrdinal;

        private ExpressionPlan PlanExpression(ExpressionOperation expression)
        {
            ValidateOrdinaryAddressOccurrence(expression);
            ++operationOrdinal;
            return ApplyRecordedVolatileFetch(expression, PlanExpressionCore(expression));
        }

        private ExpressionPlan PlanExpressionCore(ExpressionOperation expression)
        {
            if (TryPlanNormalLifetime(expression, out var normalLifetime)) return normalLifetime;
            if (TryPlanExceptionExpression(expression, out var exceptionExpression)) return exceptionExpression;
            if (TryPlanOverridingResult(expression, out var overridingResult)) return overridingResult;
            var type = CsType.Value(expression.Type);
            switch (expression)
            {
                case ConstantOperation constant:
                    if (TryPlanStoredConstant(constant, out var storedConstant)) return storedConstant;
                    return new(expression.Type, expression.ValueCategory, [], new CsLeaf(type, Constant(constant.Constant), true), ExpressionEffects.None);
                case ReferenceOperation reference:
                {
                    var value = new CsLeaf(type, Reference(reference), reference.Target is FunctionSymbol);
                    var place = reference.Target is VariableSymbol variable ? MemoryPlace(expression.Type, LValueKind.Slot, value, variable: variable) : null;
                    return new(expression.Type, expression.ValueCategory, [], place is null ? value : new CsLoad(place),
                        place is null ? ExpressionEffects.None : ExpressionEffects.ReadStorage, place);
                }
                case CallOperation call: return PlanCall(call);
                case ConversionOperation conversion: return PlanConversion(conversion);
                case OperatorOperation operation: return PlanOperator(operation);
                case LifetimeOperation lifetime when lifetime.Lifetime is null || lifetime.Lifetime.Cleanups.Count == 0:
                    return PlanExpression(lifetime.Value) with { Type = expression.Type, ValueCategory = expression.ValueCategory };
                case ConditionOperation condition when condition.Initialization is null && condition.DynamicInitialization is null:
                    return PlanExpression(Required(condition.Value)) with { Type = expression.Type, ValueCategory = expression.ValueCategory };
                default: throw Limit($"Expression '{expression.Kind}' has no typed execution plan.", expression.Kind.Name);
            }
        }

        private ExpressionPlan PlanConversion(ConversionOperation conversion)
        {
            if (TryPlanOrdinaryBaseView(conversion, out var ordinaryBaseView)) return ordinaryBaseView;
            if (TryPlanVirtualBaseView(conversion, out var virtualBaseView)) return virtualBaseView;
            var kind = conversion.OperatorKind.Name;
            if (conversion.Operator.RequiresRuntimeCastCheck || kind is "eok_dynamic_cast" or "eok_ref_dynamic_cast" or "eok_base_class_cast" or "eok_derived_class_cast")
                throw Limit("Conversion requires a selected C++ object-model adapter.", "conversion");
            var operand = PlanExpression(conversion.Operand);
            if (kind == "eok_cast" && IsVoid(CsType.Value(conversion.TargetType)))
            {
                var prefix = operand.Prefix.ToList();
                prefix.Add(new CsEvaluate(operand.Value));
                return new(conversion.Type, conversion.ValueCategory, prefix,
                    new CsLeaf(CsType.Void, "", true), operand.Effects);
            }
            CsExpression value;
            LValuePlan? place = null;
            if (kind == "eok_bool_cast") value = SourceBoolean(conversion.Type, PlanTruth(operand.Value, operand.Type));
            else if (kind is "eok_lvalue_adjust" or "eok_class_rvalue_adjust")
            {
                if (!conversion.SourceType.Equals(conversion.TargetType) && TypeName(conversion.SourceType) != TypeName(conversion.TargetType))
                    throw Limit("Lvalue adjustment changes its selected storage representation.", "conversion");
                value = operand.Value; place = operand.Place;
            }
            else if (kind == "eok_array_to_pointer")
            {
                var sourcePlace = operand.Place ?? throw Limit("Array decay requires an addressable selected array.", "array");
                value = new CsCast(CsType.Value(conversion.Type), new CsAddress(CsType.Address(operand.Type), sourcePlace));
            }
            else if (kind is "eok_cast" or "eok_lvalue_cast" or "eok_ref_cast") value = new CsCast(CsType.Value(conversion.Type), operand.Value);
            else throw Limit($"Conversion '{kind}' has no typed plan.", "conversion");
            return new(conversion.Type, conversion.ValueCategory, operand.Prefix, value, operand.Effects, place);
        }

        private ExpressionPlan PlanOperator(OperatorOperation operation)
        {
            var kind = operation.OperatorKind.Name;
            var operands = operation.Operands;
            if (operation.Operator.EvalLeftToRight && operation.Operator.EvalRightToLeft)
                throw Limit("This combination of retained operand constraints has no selected sequencing plan.", "sequencing");
            if (kind == "eok_parens") return PlanExpression(operands[0]) with { Type = operation.Type, ValueCategory = operation.ValueCategory };
            if (kind == "eok_comma") return PlanComma(operation);
            if (kind == "eok_question") return PlanConditional(operation);
            if (kind is "eok_land" or "eok_lor") return PlanShortCircuit(operation);
            if (kind == "eok_va_arg") return PlanVariadicArgument(operation);
            if (kind == "eok_bassign") return PlanAggregateAssignment(operation);
            if (AssignmentOperators.ContainsKey(kind) || NeedsPlannedCompoundOperation(operation)) return PlanAssignment(operation);
            if (kind is "eok_pre_incr" or "eok_pre_decr" or "eok_post_incr" or "eok_post_decr") return PlanIncrement(operation);
            if (kind is "eok_dot_field" or "eok_points_to_field") return PlanField(operation);
            if (kind is "eok_address_of" or "eok_reference_to")
            {
                if (operands[0] is ReferenceOperation { Target: FunctionSymbol function })
                    return new(operation.Type, operation.ValueCategory, [], new CsLeaf(CsType.Value(operation.Type), FunctionAddress(function), true), ExpressionEffects.None);
                var operand = PlanExpression(operands[0]);
                var place = operand.Place ?? throw Limit("Address-taking requires a selected place.", "lvalue");
                if (place.IsBitField) throw Limit("A bitfield place has no ordinary native address.", "bitfield");
                return new(operation.Type, operation.ValueCategory, operand.Prefix, new CsAddress(CsType.Value(operation.Type), place), operand.Effects);
            }
            if (kind is "eok_indirect" or "eok_ref_indirect")
            {
                var operand = PlanExpression(operands[0]);
                var place = MemoryPlace(operation.Type, LValueKind.Address, operand.Value);
                return new(operation.Type, operation.ValueCategory, operand.Prefix, new CsLoad(place), operand.Effects | ExpressionEffects.ReadStorage, place);
            }
            if (kind is "eok_subscript" or "eok_padd" or "eok_psubtract" or "eok_pdiff")
            {
                var plans = operands.Select(PlanExpression).ToArray();
                var (prefix, values) = OrderOperands(plans, operation.Operator.EvalRightToLeft, operation.Operator.PointerOperandIsSecond);
                var pointer = operation.Operator.PointerOperandIsSecond ? values[1] : values[0];
                var index = operation.Operator.PointerOperandIsSecond ? values[0] : values[1];
                if (kind == "eok_subscript")
                {
                    var address = new CsBinary(CsType.Address(operation.Type), "+", pointer, index);
                    var place = MemoryPlace(operation.Type, LValueKind.Address, address);
                    return new(operation.Type, operation.ValueCategory, prefix, new CsLoad(place), Effects(plans) | ExpressionEffects.ReadStorage, place);
                }
                return new(operation.Type, operation.ValueCategory, prefix,
                    new CsBinary(CsType.Value(operation.Type), kind == "eok_padd" ? "+" : "-", pointer, index), Effects(plans));
            }
            var children = operands.Select(PlanExpression).ToArray();
            var ordered = OrderOperands(children, operation.Operator.EvalRightToLeft, false);
            var resultType = CsType.Value(operation.Type);
            CsExpression result;
            if (kind == "eok_not") result = SourceBoolean(operation.Type, new CsUnary(CsType.Boolean, "!", PlanTruth(ordered.Values[0], children[0].Type)));
            else if (kind is "eok_negate" or "eok_unary_plus" or "eok_complement")
                result = new CsCast(resultType, new CsUnary(resultType, kind == "eok_negate" ? "-" : kind == "eok_unary_plus" ? "+" : "~", ordered.Values[0]));
            else if (ComparisonOperators.TryGetValue(kind, out var comparison))
                result = SourceBoolean(operation.Type, new CsBinary(CsType.Boolean, comparison, ordered.Values[0], ordered.Values[1]));
            else if (BinaryOperators.TryGetValue(kind, out var binary))
                result = new CsCast(resultType, new CsBinary(resultType, binary, ordered.Values[0], ordered.Values[1]));
            else if (kind is "eok_shiftl" or "eok_shiftr") result = new CsTargetShift(resultType, kind == "eok_shiftr", ordered.Values[0], ordered.Values[1]);
            else throw Limit($"Operator '{kind}' has no typed execution plan.", kind);
            return new(operation.Type, operation.ValueCategory, ordered.Prefix, result, Effects(children));
        }

        private ExpressionPlan PlanField(OperatorOperation operation)
        {
            if (operation.Operands.Count != 2 || operation.Operands[1] is not ReferenceOperation { Target: FieldSymbol field })
                throw Limit("Member access needs the selected object and field operands.", "field");
            var owner = PlanExpression(operation.Operands[0]);
            var prefix = owner.Prefix.ToList();
            CsExpression receiver;
            if (operation.OperatorKind.Name == "eok_points_to_field") receiver = owner.Value;
            else
            {
                var place = owner.Place;
                if (place is null)
                {
                    var temporary = DeclareTemporary(CsType.Value(owner.Type), owner.Value, prefix);
                    place = new(owner.Type, LValueKind.Slot, temporary);
                }
                receiver = new CsAddress(CsType.Address(owner.Type), place);
            }
            var member = MemoryPlace(operation.Type, LValueKind.Field, receiver, field);
            return new(operation.Type, operation.ValueCategory, prefix, new CsLoad(member), owner.Effects | ExpressionEffects.ReadStorage, member);
        }

        private ExpressionPlan PlanComma(OperatorOperation operation)
        {
            if (operation.Operands.Count != 2) throw Limit("Comma requires two retained operand occurrences.", "sequencing");
            var left = PlanExpression(operation.Operands[0]); var right = PlanExpression(operation.Operands[1]);
            var prefix = left.Prefix.ToList();
            prefix.Add(new CsEvaluate(left.Value)); prefix.AddRange(right.Prefix);
            return new(operation.Type, operation.ValueCategory, prefix, right.Value, left.Effects | right.Effects, right.Place);
        }

        private ExpressionPlan PlanConditional(OperatorOperation operation)
        {
            if (operation.Operator.IsGnuTwoOperandConditional || operation.Operands.Count != 3)
                throw Limit("GNU reused-condition selection requires a distinct retained reuse plan.", "sequencing");
            var condition = PlanExpression(operation.Operands[0]);
            var yes = PlanExpression(operation.Operands[1]); var no = PlanExpression(operation.Operands[2]);
            var prefix = condition.Prefix.ToList();
            var whenTrue = yes.Prefix.ToList(); var whenFalse = no.Prefix.ToList();
            LValuePlan? resultPlace = null;
            CsExpression result;
            if (operation.ValueCategory.Name is "lvalue" or "xvalue")
            {
                if (yes.Place is null || no.Place is null || yes.Place.IsBitField || no.Place.IsBitField)
                    throw Limit("Conditional lvalue requires addressable selected branch places.", "lvalue");
                var address = DeclareTemporary(CsType.Address(operation.Type), null, prefix);
                whenTrue.Add(new CsSetVariable(address, new CsAddress(address.Type, yes.Place)));
                whenFalse.Add(new CsSetVariable(address, new CsAddress(address.Type, no.Place)));
                resultPlace = MemoryPlace(operation.Type, LValueKind.Address, address); result = new CsLoad(resultPlace);
            }
            else if (IsVoid(CsType.Value(operation.Type)))
            {
                whenTrue.Add(new CsEvaluate(yes.Value)); whenFalse.Add(new CsEvaluate(no.Value));
                result = new CsLeaf(CsType.Void, "", true);
            }
            else
            {
                var value = DeclareTemporary(CsType.Value(operation.Type), null, prefix);
                whenTrue.Add(new CsSetVariable(value, yes.Value)); whenFalse.Add(new CsSetVariable(value, no.Value)); result = value;
                if (MemoryRepresentationType(operation.Type) is RecordType record)
                {
                    if (IsVolatileObjectType(operation.Type) || yes.Place?.VolatileTarget == true || no.Place?.VolatileTarget == true ||
                        !MemoryRepresentationType(yes.Type).Equals(record) || !MemoryRepresentationType(no.Type).Equals(record) ||
                        yes.Type.Size != operation.Type.Size || no.Type.Size != operation.Type.Size ||
                        yes.Type.Alignment != operation.Type.Alignment || no.Type.Alignment != operation.Type.Alignment)
                        throw Limit("A conditional record value needs identical selected ordinary storage representations.", "aggregate");
                    resultPlace = MemoryPlace(operation.Type, LValueKind.Slot, value);
                    result = new CsLoad(resultPlace);
                }
            }
            prefix.Add(new CsIf(PlanTruth(condition.Value, condition.Type), whenTrue, whenFalse));
            return new(operation.Type, operation.ValueCategory, prefix, result, condition.Effects | yes.Effects | no.Effects, resultPlace);
        }

        private ExpressionPlan PlanShortCircuit(OperatorOperation operation)
        {
            var left = PlanExpression(operation.Operands[0]); var right = PlanExpression(operation.Operands[1]);
            var isAnd = operation.OperatorKind.Name == "eok_land";
            var prefix = left.Prefix.ToList();
            var result = DeclareTemporary(CsType.Value(operation.Type), SourceBoolean(operation.Type, new CsBooleanLiteral(!isAnd)), prefix);
            var branch = right.Prefix.ToList();
            branch.Add(new CsSetVariable(result, SourceBoolean(operation.Type, PlanTruth(right.Value, right.Type))));
            var test = PlanTruth(left.Value, left.Type);
            if (!isAnd) test = new CsUnary(CsType.Boolean, "!", test);
            prefix.Add(new CsIf(test, branch, []));
            return new(operation.Type, operation.ValueCategory, prefix, result, left.Effects | right.Effects);
        }

        private ExpressionPlan PlanAssignment(OperatorOperation operation)
        {
            ValidatePlannedCompound(operation);
            if (MemoryRepresentationType(operation.Operands[0].Type) is RecordType or ArrayType) return PlanAggregateAssignment(operation);
            var left = PlanExpression(operation.Operands[0]); var right = PlanExpression(operation.Operands[1]);
            var originalPlace = left.Place ?? throw Limit("Assignment needs an actual selected lvalue.", "lvalue");
            var prefix = new List<CsPlanStatement>();
            LValuePlan place;
            CsExpression rightValue;
            CsVariable? oldValue = null;
            var compound = operation.OperatorKind.Name != "eok_assign";
            if (compound && MemoryRepresentationType(left.Type) is ArithmeticType { Arithmetic.IsBool: true })
                throw Limit("Boolean compound assignment needs a selected normalization plan.", "compound-assignment");
            if (operation.Operator.EvalRightToLeft)
            {
                prefix.AddRange(right.Prefix); rightValue = CaptureValue(right.Value, prefix);
                prefix.AddRange(left.Prefix); place = CapturePlace(originalPlace, prefix);
                if (compound) oldValue = DeclareTemporary(CsType.Value(left.Type), ReadSelectedPlace(place), prefix);
            }
            else
            {
                prefix.AddRange(left.Prefix); place = CapturePlace(originalPlace, prefix);
                if (compound) oldValue = DeclareTemporary(CsType.Value(left.Type), ReadSelectedPlace(place), prefix);
                prefix.AddRange(right.Prefix); rightValue = CaptureValue(right.Value, prefix);
            }
            CsExpression stored;
            if (compound)
            {
                if (operation.OperatorKind.Name is "eok_padd_assign" or "eok_psubtract_assign")
                    prefix.Add(new CsSetVariable(oldValue!, new CsBinary(CsType.Value(left.Type),
                        operation.OperatorKind.Name == "eok_padd_assign" ? "+" : "-", oldValue!, rightValue)));
                else if (operation.OperatorKind.Name is "eok_shiftl_assign" or "eok_shiftr_assign")
                    prefix.Add(new CsSetVariable(oldValue!, new CsCast(CsType.Value(left.Type),
                        new CsTargetShift(CsType.Value(left.Type), operation.OperatorKind.Name == "eok_shiftr_assign", oldValue!, rightValue))));
                else
                {
                    if (TryPlanCompoundConversion(operation, oldValue!, rightValue, out var converted))
                        prefix.Add(new CsSetVariable(oldValue!, converted));
                    else
                    {
                        var leftName = TypeName(left.Type); var rightName = TypeName(right.Type);
                        if (leftName != rightName && !(leftName is "sbyte" or "byte" or "short" or "ushort" && rightName == "int"))
                            throw Limit("Mixed compound arithmetic requires an explicit selected computation-type adapter.", "compound-assignment");
                        prefix.Add(new CsModifyVariable(oldValue!, AssignmentOperators[operation.OperatorKind.Name], rightValue));
                    }
                }
                stored = oldValue!;
            }
            else stored = new CsCast(CsType.Value(left.Type), rightValue);
            prefix.Add(new CsStore(place, stored));
            // Reading through the selected getter observes signedness/truncation after a bitfield store.
            var observed = place.VolatileTarget ? stored : new CsLoad(place);
            var result = DeclareTemporary(CsType.Value(operation.Type), new CsCast(CsType.Value(operation.Type), observed), prefix);
            return new(operation.Type, operation.ValueCategory, prefix, result,
                left.Effects | right.Effects | ExpressionEffects.ReadStorage | StoreEffects(place),
                operation.ValueCategory.Name is "lvalue" or "xvalue" ? place : null);
        }

        private ExpressionPlan PlanAggregateAssignment(OperatorOperation operation)
        {
            var left = PlanExpression(operation.Operands[0]); var right = PlanExpression(operation.Operands[1]);
            if (left.Place is null || right.Place is null) throw Limit("Aggregate copy needs two selected storage places; return-value materialization is not implemented.", "aggregate");
            if (left.Place.VolatileTarget || right.Place.VolatileTarget || operation.Details.VolatileFetch)
                throw Limit("Volatile aggregate copies require an exercised explicit access plan.", "volatile");
            var blockCopy = operation.OperatorKind.Name == "eok_bassign";
            // The current Copy<T> runtime represents complete objects of one selected storage type.
            // EDG block copies can instead use a smaller source to partially fill a destination.
            if (StorageSize(left.Type) != StorageSize(right.Type) || TypeName(left.Type) != TypeName(right.Type))
                throw Limit("Source-sized partial or differing-representation block copies need a separate native storage adapter.", "aggregate");
            if (blockCopy && !IsVoid(CsType.Value(operation.Type)))
                throw Limit("This selected block-copy result has no void execution plan.", "aggregate");
            var prefix = new List<CsPlanStatement>();
            LValuePlan destination; LValuePlan source;
            if (operation.Operator.EvalRightToLeft)
            {
                prefix.AddRange(right.Prefix); source = CapturePlace(right.Place, prefix);
                if (!blockCopy)
                {
                    // Value assignment captures RHS bytes; eok_bassign instead copies from the
                    // selected source address after evaluating both memcpy address operands.
                    var snapshot = DeclareTemporary(CsType.Value(right.Type), new CsLeaf(CsType.Value(right.Type), "default", true), prefix);
                    var snapshotPlace = new LValuePlan(right.Type, LValueKind.Slot, snapshot);
                    prefix.Add(new CsCopy(snapshotPlace, source, right.Type)); source = snapshotPlace;
                }
                prefix.AddRange(left.Prefix); destination = CapturePlace(left.Place, prefix);
            }
            else
            {
                prefix.AddRange(left.Prefix); destination = CapturePlace(left.Place, prefix);
                prefix.AddRange(right.Prefix); source = CapturePlace(right.Place, prefix);
            }
            prefix.Add(new CsCopy(destination, source, right.Type));
            CsExpression result = blockCopy ? new CsLeaf(CsType.Void, "", true) : new CsLoad(destination);
            return new(operation.Type, operation.ValueCategory, prefix, result,
                left.Effects | right.Effects | ExpressionEffects.ReadStorage | ExpressionEffects.WriteStorage,
                operation.ValueCategory.Name is "lvalue" or "xvalue" ? destination : null);
        }

        private ExpressionPlan PlanIncrement(OperatorOperation operation)
        {
            var operand = PlanExpression(operation.Operands[0]);
            if (MemoryRepresentationType(operand.Type) is ArithmeticType { Arithmetic.IsBool: true })
                throw Limit("Boolean increment/decrement needs a selected normalization plan.", "increment");
            var prefix = operand.Prefix.ToList();
            var place = CapturePlace(operand.Place ?? throw Limit("Increment needs an actual selected lvalue.", "lvalue"), prefix);
            var oldValue = DeclareTemporary(CsType.Value(operand.Type), ReadSelectedPlace(place), prefix);
            var computed = DeclareTemporary(CsType.Value(operand.Type), oldValue, prefix);
            var increment = operation.OperatorKind.Name is "eok_pre_incr" or "eok_post_incr";
            prefix.Add(new CsModifyVariable(computed, increment ? "++" : "--", null));
            prefix.Add(new CsStore(place, computed));
            var post = operation.OperatorKind.Name is "eok_post_incr" or "eok_post_decr";
            CsExpression result = post ? oldValue : DeclareTemporary(CsType.Value(operation.Type), new CsCast(CsType.Value(operation.Type), place.VolatileTarget ? computed : new CsLoad(place)), prefix);
            return new(operation.Type, operation.ValueCategory, prefix, result, operand.Effects | StoreEffects(place) | ExpressionEffects.ReadStorage,
                operation.ValueCategory.Name is "lvalue" or "xvalue" ? place : null);
        }

        private ExpressionPlan PlanCall(CallOperation call)
        {
            if (TryPlanStartupRegistration(call, out var startupRegistration)) return startupRegistration;
            if (TryPlanAutomaticArrayCall(call, out var automaticArrayCall)) return automaticArrayCall;
            ValidateGeneratedCallRoles(call);
            if (call.Operator.EvalLeftToRight && call.Operator.EvalRightToLeft)
                throw Limit("This retained combination of call constraints has no selected plan.", "sequencing");
            var rawOperands = call.Operands;
            if (rawOperands.Count != call.Arguments.Count + 1 || !rawOperands[0].Equals(call.Callee)
                || call.Arguments.Where((argument, index) => !rawOperands[index + 1].Equals(argument.Value)).Any())
                throw Limit("Typed call associations do not expose the same selected operand occurrence order.", "sequencing");
            var arguments = call.Arguments.Select(a => PlanExpression(a.Value)).ToArray();
            ExpressionPlan? callee = null;
            if (call.Callee is not ReferenceOperation { Target: FunctionSymbol })
            {
                var address = !IsVirtualCall(call) && call.Callee is OperatorOperation { OperatorKind.Name: "eok_indirect" } indirect ? indirect.Operands[0] : call.Callee;
                callee = PlanExpression(address);
            }
            var all = callee is null ? arguments : new[] { callee! }.Concat(arguments).ToArray();
            var ordered = OrderOperands(all, call.Operator.EvalRightToLeft, false);
            var argumentValues = callee is null ? ordered.Values : ordered.Values.Skip(1).ToArray();
            return new(call.Type, call.ValueCategory, ordered.Prefix, new CsPlannedCall(CsType.Value(call.Type), call,
                callee is null ? null : ordered.Values[0], argumentValues), Effects(all) | ExpressionEffects.Call | ExpressionEffects.MayThrow);
        }

        private (IReadOnlyList<CsPlanStatement> Prefix, IReadOnlyList<CsExpression> Values) OrderOperands(IReadOnlyList<ExpressionPlan> plans, bool rightToLeft, bool forceMaterialize)
        {
            var prefix = new List<CsPlanStatement>(); var values = new CsExpression[plans.Count];
            var materialize = forceMaterialize || rightToLeft || plans.Any(p => p.Prefix.Count != 0);
            // EDG flags constrain occurrences. In their absence this consumer chooses left-to-right.
            var order = rightToLeft ? Enumerable.Range(0, plans.Count).Reverse() : Enumerable.Range(0, plans.Count);
            foreach (var index in order)
            {
                prefix.AddRange(plans[index].Prefix);
                values[index] = materialize ? CaptureValue(plans[index].Value, prefix) : plans[index].Value;
            }
            return (prefix, values);
        }

        private CsExpression CaptureValue(CsExpression value, List<CsPlanStatement> prefix)
            => value is CsVariable or CsLeaf { Stable: true } || IsVoid(value.Type) ? value : DeclareTemporary(value.Type, value, prefix);
        private LValuePlan CapturePlace(LValuePlan place, List<CsPlanStatement> prefix)
        {
            if (IsRawUInt128(place.Type) && IsStableUInt128LocalPlace(place)) return place;
            if (place.IsBitField)
            {
                var receiver = DeclareTemporary(CsType.Address(place.Field!.ContainingType), place.Operand, prefix);
                return place with { Operand = receiver };
            }
            var address = DeclareTemporary(CsType.Address(place.Type), new CsAddress(CsType.Address(place.Type), place), prefix);
            return new(place.Type, LValueKind.Address, address, VolatileTarget: place.VolatileTarget);
        }
        private CsVariable DeclareTemporary(CsType type, CsExpression? initializer, List<CsPlanStatement> prefix)
        {
            string proposed;
            do { proposed = "__sequence_" + ++expressionTemporaryOrdinal; }
            while (names.Values.Contains(proposed, StringComparer.Ordinal) || names.Values.Contains("@" + proposed, StringComparer.Ordinal));
            var variable = new CsVariable(type, InternalName(proposed)); prefix.Add(new CsDeclare(variable, initializer)); return variable;
        }
        private static ExpressionEffects Effects(IEnumerable<ExpressionPlan> plans) => plans.Aggregate(ExpressionEffects.None, (effects, plan) => effects | plan.Effects);
        private static CsExpression PlanTruth(CsExpression value, CppType type) => new CsTruth(value, type);
        private static CsExpression SourceBoolean(CppType type, CsExpression condition) => new CsSourceBoolean(CsType.Value(type), condition);
        private static bool IsVoid(CsType type) => type.Intrinsic == "void" || type.AddedPointerDepth == 0 && type.Captured is { } captured && MemoryRepresentationType(captured).Kind.Name == "tk_void";

        private string RenderType(CsType type) => type.Intrinsic ?? TypeName(type.Captured!) + new string('*', type.AddedPointerDepth);
        private string RenderPlanValue(ExpressionPlan plan) => RenderExpression(plan.Value);
        private string RenderExpression(CsExpression expression) => expression switch
        {
            CsBooleanLiteral literal => literal.Value ? "true" : "false",
            CsLeaf leaf => leaf.Text,
            CsVariable variable => variable.Name,
            CsUnary unary => $"({unary.Operator}({RenderExpression(unary.Operand)}))",
            CsBinary binary => $"unchecked(({RenderExpression(binary.Left)}) {binary.Operator} ({RenderExpression(binary.Right)}))",
            CsCast cast => $"unchecked(({RenderType(cast.Type)})({RenderExpression(cast.Operand)}))",
            CsChoice choice => $"({RenderExpression(choice.Condition)} ? {RenderExpression(choice.Then)} : {RenderExpression(choice.Else)})",
            CsLoad load => RenderMemoryLoad(load),
            CsAddress address => RenderAddress(address.Place),
            CsPlannedCall call => RenderCall(call),
            CsAutomaticArrayCall call => RenderAutomaticArrayCall(call),
            CsOverridingResult result => RenderOverridingResult(result),
            CsVariadicArgument argument => VariadicOperator(argument.Operation, [RenderExpression(argument.Cursor)]),
            CsTargetShift shift => $"TargetArithmetic.{(shift.Right ? "ShiftRight" : "ShiftLeft")}({RenderExpression(shift.Left)}, checked((int)({RenderExpression(shift.Count)})))",
            CsTruth { Value: CsSourceBoolean boolean } => RenderExpression(boolean.Condition),
            CsTruth truth => TruthValue(RenderExpression(truth.Value), truth.SourceType),
            CsSourceBoolean { Condition: CsBooleanLiteral literal } boolean => $"(({RenderType(boolean.Type)}){(literal.Value ? "1" : "0")})",
            CsSourceBoolean boolean => $"({RenderExpression(boolean.Condition)} ? ({RenderType(boolean.Type)})1 : ({RenderType(boolean.Type)})0)",
            _ => throw Limit("Unknown typed C# expression node.", "expression-plan")
        };
        private string RenderPlace(LValuePlan place) => place.Kind switch
        {
            LValueKind.Slot => RenderExpression(place.Operand),
            LValueKind.Address => $"(*({RenderExpression(place.Operand)}))",
            LValueKind.Field when IsFlexibleTailField(place.Field!) => throw Limit("A flexible tail has an address and no ordinary array value.", "array"),
            LValueKind.Field => $"({RenderExpression(place.Operand)})->{FieldName(place.Field!)}",
            _ => throw Limit("Unknown selected storage place.", "lvalue")
        };
        private string RenderAddress(LValuePlan place)
        {
            ValidateValueAddress(place.Type);
            if (place.IsBitField) throw Limit("Bitfield properties have no native field address.", "bitfield");
            if (place.Kind == LValueKind.Field && IsFlexibleTailField(place.Field!))
                return FlexibleTailAddress(place.Field!, RenderExpression(place.Operand));
            return place.Kind == LValueKind.Address ? RenderExpression(place.Operand) : $"&({RenderPlace(place)})";
        }
        private string RenderCall(CsPlannedCall plannedCall)
        {
            var call = plannedCall.Operation;
            if (IsVirtualCall(call))
                return VirtualCall(call, RenderExpression(plannedCall.Callee ?? throw Limit("The lowered virtual callee value is absent.", "virtual-call")),
                    plannedCall.Arguments.Select(RenderExpression).ToArray());
            if (call.Callee is ReferenceOperation { Target: FunctionSymbol function })
            {
                function = CanonicalFunction(function);
                if (call.TargetMethod is { } selected && !CanonicalFunction(selected).Equals(function)) throw Limit("Selected callee needs an explicit ABI adapter.", "call");
                if (TryIntrinsicCall(call, plannedCall.Arguments.Select(RenderExpression).ToArray(), out var intrinsic)) return intrinsic;
                RequireCallable(function);
                if (function.Signature.IsVariadic)
                    return ProgramResultView(function.ReturnType, call.Type, VariadicCall(function.Signature, function,
                        call.Arguments.Select(argument => argument.Value.Type).ToArray(), plannedCall.Arguments.Select(RenderExpression).ToArray()));
                if (function.Signature.Parameters.Count != plannedCall.Arguments.Count)
                    throw Limit("A selected fixed call needs explicit parameter adaptation.", "call");
                if (HasAggregateSignature(function.Signature) && !functions.Contains(function))
                    return AggregateCall(function.Signature, function, call.Arguments.Select(argument => argument.Value.Type).ToArray(),
                        plannedCall.Arguments.Select(RenderExpression).ToArray());
                var arguments = string.Join(", ", plannedCall.Arguments.Select((value, index) =>
                    ProgramArgumentView(call, index, function, RenderExpression(value))));
                return ProgramResultView(call, function, $"{OwnedName(function)}({arguments})");
            }
            var type = Unwrap(call.Call.FunctionType ?? call.Callee.Type);
            if (type is PointerType pointer) type = Unwrap(pointer.PointeeType);
            if (type is not RoutineType signature || plannedCall.Callee is null)
                throw Limit("Indirect call needs a selected signature and function-address representation.", "call");
            if (signature.IsVariadic)
                return ProgramResultView(signature.ReturnType, call.Type, VariadicCall(signature, null,
                    call.Arguments.Select(argument => argument.Value.Type).ToArray(), plannedCall.Arguments.Select(RenderExpression).ToArray(),
                    RenderExpression(plannedCall.Callee)));
            if (HasAggregateSignature(signature))
                return AggregateCall(signature, null, call.Arguments.Select(argument => argument.Value.Type).ToArray(),
                    plannedCall.Arguments.Select(RenderExpression).ToArray(), RenderExpression(plannedCall.Callee));
            ValidateCallingConvention(signature);
            ValidateNativeScalarSignature(signature);
            if (signature.Parameters.Count != plannedCall.Arguments.Count) throw Limit("Indirect call parameter adaptation is unsupported.", "call");
            if (!indirectCalls.TryGetValue(signature, out var helper)) indirectCalls.Add(signature, helper = InternalName($"__Invoke_{indirectCalls.Count + 1}"));
            var indirectArguments = string.Join(", ", plannedCall.Arguments.Select((value, index) =>
                ProgramArgumentView(call.Arguments[index].Value.Type, signature.Parameters[index].Type, RenderExpression(value))));
            return ProgramResultView(signature.ReturnType, call.Type,
                $"{Owner}.{helper}({RenderExpression(plannedCall.Callee)}{(indirectArguments.Length == 0 ? "" : ", " + indirectArguments)})");
        }
        private void EmitPlanPrefix(ExpressionPlan plan, CodeWriter writer) => EmitPlanStatements(plan.Prefix, writer);
        private void EmitPlanStatements(IEnumerable<CsPlanStatement> statements, CodeWriter writer)
        {
            foreach (var statement in statements)
                switch (statement)
                {
                    case CsDeclare declaration:
                        writer.Line(RenderType(declaration.Variable.Type) + " " + declaration.Variable.Name
                            + (declaration.Initializer is null ? ";" : " = " + RenderExpression(declaration.Initializer) + ";")); break;
                    case CsSetVariable assignment: writer.Line(assignment.Variable.Name + " = " + RenderExpression(assignment.Value) + ";"); break;
                    case CsStore store: EmitMemoryStore(store, writer); break;
                    case CsCopy copy: writer.Line($"TargetStorage.Copy(ref {RenderPlace(copy.Destination)}, ref {RenderPlace(copy.Source)}, {StorageSize(copy.StorageType)});"); break;
                    case CsModifyVariable modify:
                        if (modify.Right is not null && RenderType(modify.Variable.Type) is "sbyte" or "byte" or "short" or "ushort")
                        {
                            var helper = modify.Operator switch
                            {
                                "+=" => "Add", "-=" => "Subtract", "*=" => "Multiply", "/=" => "Divide", "%=" => "Remainder",
                                "&=" => "And", "|=" => "Or", "^=" => "Xor",
                                _ => throw Limit("This narrow compound operation has no target helper.", "compound-assignment")
                            };
                            writer.Line($"TargetArithmetic.{helper}Assign(ref {modify.Variable.Name}, {RenderExpression(modify.Right)});");
                        }
                        else writer.Line("unchecked { " + modify.Variable.Name + modify.Operator + (modify.Right is null ? "" : " " + RenderExpression(modify.Right)) + "; }");
                        break;
                    case CsEvaluate evaluation:
                        if (!IsVoid(evaluation.Value.Type)) writer.Line("_ = " + RenderExpression(evaluation.Value) + ";");
                        else if (RenderExpression(evaluation.Value) is { Length: > 0 } voidValue) writer.Line(voidValue + ";");
                        break;
                    case CsIf conditional:
                        writer.Line("if (" + RenderExpression(conditional.Condition) + ")"); writer.Open(); EmitPlanStatements(conditional.Then, writer); writer.Close();
                        if (conditional.Else.Count != 0) { writer.Line("else"); writer.Open(); EmitPlanStatements(conditional.Else, writer); writer.Close(); }
                        break;
                    default: throw Limit("Unknown typed C# prefix statement.", "expression-plan");
                }
        }
        private void EmitPlannedExpressionStatement(ExpressionOperation expression, CodeWriter writer)
        {
            EmitPlanStatements(PlanDiscardedExpression(expression), writer);
        }

        private IReadOnlyList<CsPlanStatement> PlanDiscardedExpression(ExpressionOperation expression)
        {
            if (!expression.Details.VolatileFetch && expression is OperatorOperation operation
                && !(operation.Operator.EvalLeftToRight && operation.Operator.EvalRightToLeft))
            {
                if (operation.OperatorKind.Name == "eok_comma" && operation.Operands.Count == 2 && !operation.Operator.EvalRightToLeft)
                {
                    ++operationOrdinal;
                    return PlanDiscardedExpression(operation.Operands[0]).Concat(PlanDiscardedExpression(operation.Operands[1])).ToArray();
                }
                if (operation.OperatorKind.Name is "eok_pre_incr" or "eok_pre_decr" or "eok_post_incr" or "eok_post_decr"
                    && TryDiscardedLocalUpdate(operation, out var local))
                {
                    operationOrdinal += 2; // The selected unary occurrence and its exact reference operand.
                    var increment = operation.OperatorKind.Name is "eok_pre_incr" or "eok_post_incr";
                    return [new CsModifyVariable(local, increment ? "++" : "--", null)];
                }
            }
            var plan = PlanExpression(expression);
            return plan.Prefix.Append(new CsEvaluate(plan.Value)).ToArray();
        }

        private bool TryDiscardedLocalUpdate(OperatorOperation operation, out CsVariable local)
        {
            local = null!;
            if (operation.Operands.Count != 1 || operation.Operands[0] is not ReferenceOperation { Target: VariableSymbol variable } reference
                || reference.Details.VolatileFetch || currentFunction is null || !names.TryGetValue(variable, out var name)) return false;
            var ownLocal = currentFunction.Body?.Locals.Contains(variable) == true
                || currentFunction.Parameters.Any(parameter => parameter.Symbol?.Equals(variable) == true);
            if (!ownLocal || variable.IsThreadLocal || variable.LocalStaticInitializations.Count != 0
                || variable.Details.StorageDuration?.Name is "static" or "thread") return false;
            if (!OrdinaryLocalUpdateType(variable.Type) || !OrdinaryLocalUpdateType(reference.Type)) return false;
            var type = Unwrap(reference.Type);
            if (!Unwrap(variable.Type).Equals(type) || type is not ArithmeticType arithmetic
                || arithmetic.Arithmetic.ArithmeticKind.Name is not ("signed_integer" or "unsigned_integer")
                || arithmetic.Arithmetic.IsBool || arithmetic.Arithmetic.IsSaturating
                || arithmetic.Arithmetic.Bits is not (32 or 64)) return false;
            var naturalSize = arithmetic.Arithmetic.Bits == 32 ? 4 : 8;
            if (type.Size != naturalSize || type.Alignment != naturalSize
                || reference.Type.Size != type.Size || reference.Type.Alignment != type.Alignment
                || variable.Type.Size != type.Size || variable.Type.Alignment != type.Alignment) return false;
            local = new(CsType.Value(reference.Type), name);
            return true;
        }

        private static bool OrdinaryLocalUpdateType(CppType type)
        {
            var seen = new HashSet<CppType>();
            while (true)
            {
                if (!seen.Add(type) || type.Details.IsAtomic) return false;
                if (type is QualifiedType qualified && (qualified.IsVolatile || qualified.IsConst || qualified.Qualification.IsAtomic
                    || qualified.Qualification.AddressSpace is not (null or 0) || qualified.Qualification.IsShared
                    || qualified.Qualification.IsNear || qualified.Qualification.IsFar
                    || qualified.Qualification.IsUpcStrict || qualified.Qualification.IsUpcRelaxed)) return false;
                if (type is not TypeReference reference) return true;
                type = reference.UnderlyingType;
            }
        }
    }
}
