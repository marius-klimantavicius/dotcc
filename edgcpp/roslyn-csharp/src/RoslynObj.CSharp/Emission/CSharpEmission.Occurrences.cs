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

        // Keep the established flat subset readable. Forms needing a sequence,
        // selected branch effects or a captured place use only the typed plan.
        private bool NeedsExpressionPlan(ExpressionOperation expression)
        {
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

        private string ValueOccurrence(ExpressionOperation expression, CodeWriter writer)
        {
            if (!NeedsExpressionPlan(expression)) return Expression(expression);
            var plan = PlanExpression(expression);
            EmitPlanPrefix(plan, writer);
            return RenderPlanValue(plan);
        }

        private string ConditionOccurrence(ExpressionOperation expression, CodeWriter writer)
        {
            if (!NeedsExpressionPlan(expression)) return Truth(expression);
            var plan = PlanExpression(expression);
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
                        writer.Line("do"); Embedded(loop.Body, writer); writer.Line($"while ({Truth(Required(loop.Condition))});");
                    }
                    else
                    {
                        writer.Line(loop.Kind.Name == "stmk_for"
                            ? $"for (; {(loop.Condition is null ? "true" : Truth(loop.Condition))}; {(loop.Increment is null ? "" : Expression(loop.Increment))})"
                            : $"while ({Truth(Required(loop.Condition))})");
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
