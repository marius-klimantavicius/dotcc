using RoslynObj;

namespace RoslynObj.CSharp;

public sealed partial class CSharpEmitter
{
    private sealed partial class Emission
    {
        private ExpressionPlan PlanVariadicArgument(OperatorOperation operation)
        {
            if (operation.Operands.Count != 1 || operation.ValueCategory.Name != "prvalue")
                throw Limit("A planned va_arg needs one retained cursor and a scalar prvalue; lvalue/address preservation requires a separate adapter.", "varargs");
            ValidateVaCursor(operation.Operands[0].Type);
            ValidateVariadicArgument(operation.Type, true);
            var operand = PlanExpression(operation.Operands[0]);
            var prefix = operand.Prefix.ToList();
            // Capture the actual cursor occurrence, including any prefix and
            // address-selection effects. The existing scalar adapter receives
            // this value; it never re-enters the raw operand expression path.
            var cursor = DeclareTemporary(CsType.Value(operand.Type), operand.Value, prefix);
            return new(operation.Type, operation.ValueCategory, prefix,
                new CsVariadicArgument(CsType.Value(operation.Type), operation, cursor),
                operand.Effects | ExpressionEffects.ReadStorage | ExpressionEffects.WriteStorage | ExpressionEffects.MayThrow);
        }
    }
}
