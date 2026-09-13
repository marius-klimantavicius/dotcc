using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace DotCC.PostProcess;

// Fold a numeric encoding of a boolean back to that boolean, using the bound
// built-in operator. Never drop the condition's evaluation or invoke operator !
// on a user-defined truth type.
internal sealed class BooleanComparisonRewriter(SemanticModel model, CancellationToken token,
    Action<BinaryExpressionSyntax>? onSimplified = null,
    Func<BinaryExpressionSyntax, bool>? include = null) : CSharpSyntaxRewriter
{
    public int Simplified { get; private set; }

    public override SyntaxNode? VisitBinaryExpression(BinaryExpressionSyntax node)
    {
        token.ThrowIfCancellationRequested();
        if (!Comparison(node.Kind()) || Peel(node.Left) is not ConditionalExpressionSyntax
            && Peel(node.Right) is not ConditionalExpressionSyntax) return base.VisitBinaryExpression(node);
        if (node.ContainsDirectives || SourceObservability.IsObservable(node, model, token)) return node;
        var visited = (BinaryExpressionSyntax)base.VisitBinaryExpression(node)!;
        if (include != null && !include(node)) return visited;
        if (model.GetOperation(node, token) is not IBinaryOperation
            { OperatorMethod: null, IsLifted: false, Type.SpecialType: SpecialType.System_Boolean } operation) return visited;

        bool reversed = false;
        var conditional = Conditional(operation.LeftOperand);
        IOperation constant = operation.RightOperand;
        if (conditional == null)
        {
            conditional = Conditional(operation.RightOperand);
            constant = operation.LeftOperand;
            reversed = true;
        }
        if (conditional?.Syntax is not ConditionalExpressionSyntax original
            || !BooleanCondition(original.Condition, conditional.Condition)
            || !Bit(conditional.WhenTrue, out int whenTrue) || !Bit(conditional.WhenFalse, out int whenFalse)
            || !Bit(constant, out int compared)) return visited;

        bool trueResult = reversed ? Evaluate(node.Kind(), compared, whenTrue) : Evaluate(node.Kind(), whenTrue, compared);
        bool falseResult = reversed ? Evaluate(node.Kind(), compared, whenFalse) : Evaluate(node.Kind(), whenFalse, compared);
        // Constant results would still need to evaluate the condition. Leave
        // those expressions intact instead of erasing possible side effects.
        if (trueResult == falseResult) return visited;

        var retained = Peel(reversed ? visited.Right : visited.Left) as ConditionalExpressionSyntax;
        if (retained == null) return visited;
        ExpressionSyntax condition = retained.Condition;
        // A conditional supplies a bool target type to default/new(). Keep it
        // explicitly when the replacement is used with var or overload resolution.
        if (original.Condition.DescendantNodesAndSelf().Any(n => n is ImplicitObjectCreationExpressionSyntax
            or LiteralExpressionSyntax { RawKind: (int)SyntaxKind.DefaultLiteralExpression }))
            condition = CastExpression(PredefinedType(Token(SyntaxKind.BoolKeyword)), ParenthesizedExpression(condition));
        ExpressionSyntax replacement = ParenthesizedExpression(condition);
        if (!trueResult) replacement = PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, replacement);
        // Keep every comment/newline from the removed shell in lexical order,
        // including caller-line-number information. The condition keeps its own trivia.
        var removed = node.DescendantTrivia().Where(t => !original.Condition.FullSpan.Contains(t.Span)).ToArray();
        replacement = replacement.WithLeadingTrivia(removed.Where(t => t.Span.End <= original.Condition.FullSpan.Start))
            .WithTrailingTrivia(removed.Where(t => t.Span.Start >= original.Condition.FullSpan.End));
        ++Simplified;
        onSimplified?.Invoke(node);
        return replacement;
    }

    private static bool Comparison(SyntaxKind kind) => kind is SyntaxKind.EqualsExpression or SyntaxKind.NotEqualsExpression
        or SyntaxKind.LessThanExpression or SyntaxKind.LessThanOrEqualExpression
        or SyntaxKind.GreaterThanExpression or SyntaxKind.GreaterThanOrEqualExpression;

    private bool BooleanCondition(ExpressionSyntax expression, IOperation operation)
    {
        if (model.GetTypeInfo(expression, token).Type?.SpecialType == SpecialType.System_Boolean) return true;
        // Some Roslyn versions report no TypeInfo for a target-typed new() in
        // a condition. Its bound creation still proves the bool target.
        while (operation is IConversionOperation { IsImplicit: true, OperatorMethod: null } conversion)
            operation = conversion.Operand;
        return Peel(expression) is ImplicitObjectCreationExpressionSyntax
            && operation is IObjectCreationOperation { Type.SpecialType: SpecialType.System_Boolean };
    }

    private static bool Evaluate(SyntaxKind kind, int left, int right) => kind switch
    {
        SyntaxKind.EqualsExpression => left == right,
        SyntaxKind.NotEqualsExpression => left != right,
        SyntaxKind.LessThanExpression => left < right,
        SyntaxKind.LessThanOrEqualExpression => left <= right,
        SyntaxKind.GreaterThanExpression => left > right,
        SyntaxKind.GreaterThanOrEqualExpression => left >= right,
        _ => throw new InvalidOperationException("Not a comparison")
    };

    private static bool Numeric(ITypeSymbol? type) => type?.SpecialType is
        SpecialType.System_SByte or SpecialType.System_Byte or SpecialType.System_Int16 or SpecialType.System_UInt16
        or SpecialType.System_Int32 or SpecialType.System_UInt32 or SpecialType.System_Int64 or SpecialType.System_UInt64
        or SpecialType.System_IntPtr or SpecialType.System_UIntPtr or SpecialType.System_Single or SpecialType.System_Double
        or SpecialType.System_Decimal or SpecialType.System_Char;

    private static IConditionalOperation? Conditional(IOperation operation)
    {
        while (true)
        {
            switch (operation)
            {
                case IParenthesizedOperation parenthesized: operation = parenthesized.Operand; break;
                case IConversionOperation { OperatorMethod: null } conversion
                    when Numeric(conversion.Type) && Numeric(conversion.Operand.Type):
                    operation = conversion.Operand; break;
                default: return operation as IConditionalOperation;
            }
        }
    }

    private static ExpressionSyntax Peel(ExpressionSyntax expression) => expression switch
    {
        ParenthesizedExpressionSyntax p => Peel(p.Expression),
        CastExpressionSyntax c => Peel(c.Expression),
        CheckedExpressionSyntax c => Peel(c.Expression),
        _ => expression
    };

    private static bool Bit(IOperation? operation, out int bit)
    {
        bit = 0;
        if (!Numeric(operation?.Type) || operation?.ConstantValue is not { HasValue: true } constant) return false;
        // Do not round decimals or large integers through double: a value close
        // to 1 must not accidentally be accepted as exactly 1.
        switch (constant.Value)
        {
            case float single when single == 0 || single == 1: bit = (int)single; return true;
            case double number when number == 0 || number == 1: bit = (int)number; return true;
            case char character when character == 0 || character == 1: bit = character; return true;
            case sbyte or byte or short or ushort or int or uint or long or ulong or decimal:
                decimal value = Convert.ToDecimal(constant.Value, System.Globalization.CultureInfo.InvariantCulture);
                if (value != 0 && value != 1) return false;
                bit = (int)value; return true;
            default: return false;
        }
    }
}
