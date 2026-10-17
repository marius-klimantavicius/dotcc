using RoslynObj;

namespace RoslynObj.CSharp;

public sealed partial class CSharpEmitter
{
    private sealed partial class Emission
    {
        private readonly Stack<string?> loopContinueLabels = [];
        private readonly HashSet<LabelSymbol> continuedLabels = [];
        private readonly HashSet<LabelSymbol> retainedLabels = [];
        private readonly HashSet<string> referencedContinuations = [];

        // This predicate selects execution schedules, including loop lowering.
        // Preferring typed syntax for an ordinary scalar tree is separate below.
        private bool NeedsExpressionPlan(ExpressionOperation expression)
        {
            if (NeedsNormalLifetimePlan(expression)) return true;
            if (NeedsExceptionExpressionPlan(expression)) return true;
            if (NeedsVirtualDispatchPlan(expression)) return true;
            if (NeedsPlannedCompoundConversion(expression) || NeedsPlannedAggregateConditional(expression)) return true;
            if (NeedsPlannedMemoryAccess(expression)) return true;
            if (NeedsPlannedCompoundOperation(expression)) return true;
            if (NeedsFlexibleTailPlan(expression)) return true;
            if (expression is ConstantOperation { Constant.Value: StringValue or AggregateValue }) return true;
            if (expression is OperatorOperation operation)
            {
                var kind = operation.OperatorKind.Name;
                if (kind == "eok_comma" || operation.Operator.EvalRightToLeft && operation.Operands.Any(HasEffects)) return true;
                if (kind is "eok_question" or "eok_land" or "eok_lor" && operation.Operands.Any(HasEffects)) return true;
                if (operation.Operands.Count > 0 && IsBitFieldPlace(operation.Operands[0]) &&
                    (AssignmentOperators.ContainsKey(kind) || kind is "eok_pre_incr" or "eok_pre_decr" or "eok_post_incr" or "eok_post_decr")) return true;
                if (AssignmentOperators.ContainsKey(kind) && kind != "eok_assign" && operation.Operands.Count == 2 &&
                    TypeName(operation.Operands[0].Type) is "byte" or "sbyte" or "short" or "ushort" && operation.Operands.Any(HasEffects)) return true;
                if (operation.Operator.PointerOperandIsSecond && operation.Operands.Any(HasEffects)) return true;
            }
            if (expression is CallOperation call && call.Callee is not ReferenceOperation { Target: FunctionSymbol } && HasEffects(call.Callee)) return true;
            return expression.Operands.Any(NeedsExpressionPlan);
        }

        // Prefer the typed renderer for the existing flat scalar subset. Every
        // admitted node has a zero-prefix plan: no calls, updates, branch plans,
        // lifetime wrappers or member receivers that could need a temporary.
        // This is a renderer choice, not an additional source-language admission.
        private bool CanRenderScalarExpressionPlan(ExpressionOperation expression)
        {
            if (expression.Details.VolatileFetch || expression.Type.Details.IsAtomic ||
                Unwrap(expression.Type) is not (ArithmeticType or PointerType)) return false;
            return expression switch
            {
                ConstantOperation { Constant.Value: IntegerValue or FloatingValue or AddressValue } => true,
                ReferenceOperation { Target: VariableSymbol or FunctionSymbol } => true,
                ConversionOperation conversion => !conversion.Operator.RequiresRuntimeCastCheck &&
                    conversion.OperatorKind.Name is "eok_cast" or "eok_lvalue_cast" or "eok_ref_cast" or "eok_bool_cast" or "eok_lvalue_adjust" or "eok_class_rvalue_adjust" &&
                    CanRenderScalarExpressionPlan(conversion.Operand),
                OperatorOperation operation => !operation.Operator.EvalRightToLeft && !operation.Operator.PointerOperandIsSecond &&
                    !operation.Operator.RequiresRuntimeCastCheck && !operation.Operator.RetainedVirtualCall &&
                    (BinaryOperators.ContainsKey(operation.OperatorKind.Name) || ComparisonOperators.ContainsKey(operation.OperatorKind.Name) ||
                     operation.OperatorKind.Name is "eok_parens" or "eok_not" or "eok_negate" or "eok_unary_plus" or "eok_complement" or
                         "eok_shiftl" or "eok_shiftr" or "eok_padd" or "eok_psubtract" or "eok_pdiff" or "eok_subscript" or
                         "eok_indirect" or "eok_ref_indirect" or "eok_address_of" or "eok_reference_to") &&
                    operation.Operands.All(CanRenderScalarExpressionPlan),
                _ => false
            };
        }

        private ExpressionPlan PlanOccurrenceExpression(ExpressionOperation expression, bool needsSchedule)
        {
            var plan = PlanExpression(expression);
            if (!needsSchedule && plan.Prefix.Count != 0)
                throw Limit("A preferred flat scalar rendering unexpectedly needs an execution prefix.", "expression-plan");
            return plan;
        }

        private string ValueOccurrence(ExpressionOperation expression, CodeWriter writer)
        {
            var needsSchedule = NeedsExpressionPlan(expression);
            if (!needsSchedule && !CanRenderScalarExpressionPlan(expression)) return Expression(expression);
            var plan = PlanOccurrenceExpression(expression, needsSchedule);
            EmitPlanPrefix(plan, writer);
            return RenderPlanValue(plan);
        }

        private string ConditionOccurrence(ExpressionOperation expression, CodeWriter writer)
        {
            var needsSchedule = NeedsExpressionPlan(expression);
            if (!needsSchedule && !CanRenderScalarExpressionPlan(expression)) return Truth(expression);
            var plan = PlanOccurrenceExpression(expression, needsSchedule);
            EmitPlanPrefix(plan, writer);
            return RenderExpression(new CsTruth(plan.Value, plan.Type));
        }

        private void ExpressionStatementOccurrence(ExpressionOperation expression, CodeWriter writer)
        {
            if (expression is ConversionOperation conversion && Unwrap(conversion.TargetType).Kind.Name == "tk_void")
            {
                ExpressionStatementOccurrence(conversion.Operand, writer);
                return;
            }
            if (TryEmitExceptionExpressionStatement(expression, writer)) return;
            if (expression is ConstantOperation { Constant.Value: IntegerValue or FloatingValue }
                && !expression.Details.VolatileFetch && expression.Operands.Count == 0)
            {
                // Validate the normal exact literal projection, then discard
                // only this operand-free scalar constant. Reads, conversions,
                // calls and all effectful plans keep their original execution.
                _ = Expression(expression);
                return;
            }
            if (NeedsExpressionPlan(expression)) { EmitPlannedExpressionStatement(expression, writer); return; }
            var value = Expression(expression);
            if (expression is OperatorOperation op && IsStatementExpression(op.OperatorKind.Name) || expression is CallOperation)
                writer.Line(value + ";");
            else writer.Line("_ = " + value + ";");
        }

        private void CollectContinueTargets(StatementOperation statement, HashSet<string> localNames)
        {
            switch (statement)
            {
                case LabelOperation label:
                    retainedLabels.Add(label.Label);
                    break;
                case JumpOperation { JumpKind.Name: "continue", Target: { } target }:
                    if (continuedLabels.Add(target)) Name(target, target.Name ?? "__continue_point", localNames);
                    break;
                case BlockOperation block:
                    foreach (var child in block.Statements) CollectContinueTargets(child, localNames);
                    break;
                case ConditionalOperation conditional:
                    if (conditional.Then is not null) CollectContinueTargets(conditional.Then, localNames);
                    if (conditional.Else is not null) CollectContinueTargets(conditional.Else, localNames);
                    break;
                case LoopOperation loop:
                    if (loop.Initialization is not null) CollectContinueTargets(loop.Initialization, localNames);
                    if (loop.Body is not null) CollectContinueTargets(loop.Body, localNames);
                    break;
            }
        }

        private void EmitLoop(LoopOperation loop, CodeWriter writer)
        {
            if (loop.Kind.Name is not ("stmk_end_test_while" or "stmk_for" or "stmk_while"))
                throw Limit($"Loop '{loop.Kind}' has no selected occurrence schedule.", "loop");
            if (loop.Initialization is not null) Statement(loop.Initialization, writer);
            var lower = loop.Condition is not null && NeedsExpressionPlan(loop.Condition) || loop.Increment is not null && NeedsExpressionPlan(loop.Increment);
            if (!lower)
            {
                loopContinueLabels.Push(null);
                try
                {
                    if (loop.Kind.Name == "stmk_end_test_while")
                    {
                        writer.Line("do"); Embedded(loop.Body, writer); writer.Line($"while ({ConditionOccurrence(Required(loop.Condition), writer)});");
                    }
                    else
                    {
                        writer.Line(loop.Kind.Name == "stmk_for"
                            ? $"for (; {(loop.Condition is null ? "true" : ConditionOccurrence(loop.Condition, writer))}; {(loop.Increment is null ? "" : Expression(loop.Increment))})"
                            : $"while ({ConditionOccurrence(Required(loop.Condition), writer)})");
                        Embedded(loop.Body, writer);
                    }
                }
                finally { loopContinueLabels.Pop(); }
                return;
            }
            var continuation = InternalName("__loop_continue_" + ++expressionTemporaryOrdinal);
            loopContinueLabels.Push(continuation);
            try
            {
                writer.Line("while (true)"); writer.Open();
                if (loop.Kind.Name != "stmk_end_test_while" && loop.Condition is not null)
                    writer.Line($"if (!({ConditionOccurrence(loop.Condition, writer)})) break;");
                Embedded(loop.Body, writer);
                if (referencedContinuations.Contains(continuation)) writer.Line(continuation + ":;");
                if (loop.Kind.Name == "stmk_end_test_while")
                    writer.Line($"if (!({ConditionOccurrence(Required(loop.Condition), writer)})) break;");
                else if (loop.Increment is not null) ExpressionStatementOccurrence(loop.Increment, writer);
                writer.Close();
            }
            finally { loopContinueLabels.Pop(); }
        }
    }
}
