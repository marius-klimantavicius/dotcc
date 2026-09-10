using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace DotCC.PostProcess;

internal sealed class ConditionRewriter(SemanticModel model, INamedTypeSymbol helper, bool pureContainer,
    List<string> diagnostics, CancellationToken token,
    Action<InvocationExpressionSyntax>? onRewritten = null, Func<InvocationExpressionSyntax, bool>? include = null) : CSharpSyntaxRewriter
{
    internal static ConditionRewriter? Create(SemanticModel model, CancellationToken token,
        List<string>? diagnostics = null, Action<InvocationExpressionSyntax>? onRewritten = null,
        Func<InvocationExpressionSyntax, bool>? include = null)
    {
        var helper = model.Compilation.GetTypeByMetadataName("Cond");
        if (helper is null) return null;
        bool pureContainer = helper.IsStatic && helper.Arity == 0 && helper.GetAttributes().Length == 0
            && helper.GetMembers().All(m => m is IMethodSymbol { MethodKind: MethodKind.Ordinary, Name: "B" });
        return new(model, helper, pureContainer, diagnostics ?? [], token, onRewritten, include);
    }

    public int Rewritten { get; private set; }
    public int Skipped { get; private set; }
    private readonly Dictionary<IMethodSymbol, bool> methodProofs = new(SymbolEqualityComparer.Default);
    private readonly Dictionary<IMethodSymbol, bool> normalizerProofs = new(SymbolEqualityComparer.Default);
    private readonly Dictionary<SyntaxTree, SemanticModel> models = new() { [model.SyntaxTree] = model };
    private static bool Same(ISymbol? a, ISymbol? b) => SymbolEqualityComparer.Default.Equals(a, b);
    private SemanticModel Model(SyntaxNode node)
    {
        if (!models.TryGetValue(node.SyntaxTree, out var result))
            models.Add(node.SyntaxTree, result = model.Compilation.GetSemanticModel(node.SyntaxTree));
        return result;
    }
    private IOperation? Operation(SyntaxNode node) => Model(node).GetOperation(node, token);
    private static ExpressionSyntax Peel(ExpressionSyntax node)
    {
        while (node is ParenthesizedExpressionSyntax p) node = p.Expression;
        return node;
    }
    private static IOperation? Implicit(IOperation? operation)
    {
        while (operation is IConversionOperation { IsImplicit: true, OperatorMethod: null } c) operation = c.Operand;
        return operation;
    }
    private static bool Parameter(IOperation? operation, IParameterSymbol parameter)
        => Implicit(operation) is IParameterReferenceOperation p && Same(p.Parameter, parameter);
    private static bool Zero(IOperation? operation) => operation?.ConstantValue is { HasValue: true, Value: not null } value
        && Convert.ToDouble(value.Value, System.Globalization.CultureInfo.InvariantCulture) == 0;
    private static bool Null(IOperation? operation) => Implicit(operation)?.ConstantValue is { HasValue: true, Value: null };
    private static ExpressionSyntax? Body(IMethodSymbol method)
    {
        if (method.DeclaringSyntaxReferences.Length != 1) return null;
        var declaration = method.DeclaringSyntaxReferences[0].GetSyntax();
        return declaration switch
        {
            MethodDeclarationSyntax m => m.ExpressionBody?.Expression ?? ReturnValue(m.Body),
            ConversionOperatorDeclarationSyntax c => c.ExpressionBody?.Expression ?? ReturnValue(c.Body),
            _ => null
        };
    }
    private static ExpressionSyntax? ReturnValue(BlockSyntax? body)
        => body?.Statements.Count == 1 && body.Statements[0] is ReturnStatementSyntax statement ? statement.Expression : null;

    private static bool Numeric(ITypeSymbol type) => type.SpecialType is
        SpecialType.System_SByte or SpecialType.System_Byte or SpecialType.System_Int16 or SpecialType.System_UInt16
        or SpecialType.System_Int32 or SpecialType.System_UInt32 or SpecialType.System_Int64 or SpecialType.System_UInt64
        or SpecialType.System_IntPtr or SpecialType.System_UIntPtr or SpecialType.System_Single or SpecialType.System_Double;
    private static bool CBool(ITypeSymbol type) => type is INamedTypeSymbol { Name: "CBool", IsValueType: true }
        && (type.ContainingNamespace.IsGlobalNamespace || type.ContainingNamespace.ToDisplayString() == "DotCC.Libc");
    private bool IsHelper(IMethodSymbol method)
    {
        if (methodProofs.TryGetValue(method, out var result)) return result;
        result = ProveHelper(method); methodProofs.Add(method, result); return result;
    }
    private bool ProveHelper(IMethodSymbol method)
    {
        if (!pureContainer || !method.IsStatic || method.Arity != 0 || method.Parameters.Length != 1
            || method.Parameters[0].RefKind != RefKind.None || method.Parameters[0].IsOptional || method.GetAttributes().Length != 0
            || method.ReturnType.SpecialType != SpecialType.System_Boolean || Body(method) is not { } body) return false;
        var parameter = method.Parameters[0];
        var expression = Operation(body);
        if (parameter.Type.SpecialType == SpecialType.System_Boolean) return Parameter(expression, parameter);
        if (expression is not IBinaryOperation { OperatorKind: BinaryOperatorKind.NotEquals, OperatorMethod: null, IsLifted: false } binary) return false;
        if (Numeric(parameter.Type)) return Parameter(binary.LeftOperand, parameter) && Zero(binary.RightOperand);
        if (parameter.Type is IPointerTypeSymbol) return Parameter(binary.LeftOperand, parameter) && Null(binary.RightOperand);
        return CBool(parameter.Type) && binary.LeftOperand is IConversionOperation { Type.SpecialType: SpecialType.System_Int32 } conversion
            && Parameter(conversion.Operand, parameter) && Zero(binary.RightOperand);
    }
    public override SyntaxNode? VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        token.ThrowIfCancellationRequested();
        // Cheap candidate filter only. Acceptance below still requires the
        // resolved method symbol and a proven helper implementation.
        var name = Peel(node.Expression) switch
        {
            SimpleNameSyntax simple => simple.Identifier.ValueText,
            MemberAccessExpressionSyntax member => member.Name.Identifier.ValueText,
            _ => null
        };
        if (name != "B") return base.VisitInvocationExpression(node);
        if (Operation(node) is not IInvocationOperation operation || !Same(operation.TargetMethod.ContainingType, helper)
            || operation.TargetMethod.Name != "B") return base.VisitInvocationExpression(node);
        // Do not touch children either when their source form is observable.
        string? reason = SourceObservability.IsObservable(node, model, token) ? "expression tree or caller-argument text is observable" :
            node.ContainsDirectives ? "preprocessor directives cross the invocation" :
            !IsHelper(operation.TargetMethod) ? "helper implementation is not a proven dotcc truth conversion" : null;
        SyntaxNode context = node;
        while (context.Parent is ParenthesizedExpressionSyntax parent) context = parent;
        if (context.Parent is ExpressionStatementSyntax || context.Parent is ForStatementSyntax loop
            && (loop.Initializers.Contains((ExpressionSyntax)context) || loop.Incrementors.Contains((ExpressionSyntax)context)))
            reason = "discarded call requires a statement expression";
        if (reason != null)
        {
            ++Skipped;
            diagnostics.Add($"{node.GetLocation().GetLineSpan()}: kept Cond.B: {reason}");
            return node;
        }
        if (include != null && !include(node)) return base.VisitInvocationExpression(node);
        var visited = (InvocationExpressionSyntax)base.VisitInvocationExpression(node)!;
        var original = node.ArgumentList.Arguments[0].Expression;
        var operand = visited.ArgumentList.Arguments[0].Expression;
        var parameterType = operation.TargetMethod.Parameters[0].Type;
        ExpressionSyntax retained = original;
        if (CBool(parameterType) && Peel(original) is CastExpressionSyntax cast
            && Operation(cast) is IConversionOperation { OperatorMethod: { } normalizer }
            && ProveNormalizer(normalizer, parameterType))
        {
            retained = cast.Expression;
            operand = ((CastExpressionSyntax)Peel(operand)).Expression;
            parameterType = normalizer.Parameters[0].Type;
        }
        var sourceType = Model(retained).GetTypeInfo(retained, token).Type;
        // TypeInfo.Type can already report the contextual type of new().
        // Keep that target explicitly when removing the invocation context.
        bool targetTyped = retained.DescendantNodesAndSelf().Any(n => n is ImplicitObjectCreationExpressionSyntax
            or LiteralExpressionSyntax { RawKind: (int)SyntaxKind.DefaultLiteralExpression });
        var converted = Same(sourceType, parameterType) && !targetTyped ? operand : CastExpression(
            ParseTypeName(parameterType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)), ParenthesizedExpression(operand));
        // An explicit cast can prefer a different user conversion from the
        // implicit conversion selected for a method argument. Verify the
        // selected operator in the original call-site scope before using it.
        IOperation? incoming = operation.Arguments[0].Value;
        while (incoming is IConversionOperation { IsImplicit: true } conversion)
        {
            if (conversion.OperatorMethod is { } selected
                && converted is CastExpressionSyntax explicitConversion
                && !Same(model.GetSpeculativeSymbolInfo(node.SpanStart, explicitConversion,
                    SpeculativeBindingOption.BindAsExpression).Symbol, selected))
            {
                ++Skipped;
                diagnostics.Add($"{node.GetLocation().GetLineSpan()}: kept Cond.B: explicit rewrite would select a different user conversion");
                return visited;
            }
            incoming = conversion.Operand;
        }
        ExpressionSyntax replacement;
        if (parameterType.SpecialType == SpecialType.System_Boolean) replacement = ParenthesizedExpression(converted);
        else
        {
            if (CBool(parameterType))
            {
                var read = (IConversionOperation)((IBinaryOperation)Operation(Body(operation.TargetMethod)!)!).LeftOperand;
                converted = CheckedExpression(read.IsChecked ? SyntaxKind.CheckedExpression : SyntaxKind.UncheckedExpression,
                    CastExpression(PredefinedType(Token(SyntaxKind.IntKeyword)), ParenthesizedExpression(converted)));
            }
            var zero = parameterType is IPointerTypeSymbol
                ? LiteralExpression(SyntaxKind.NullLiteralExpression)
                : LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0));
            replacement = ParenthesizedExpression(BinaryExpression(SyntaxKind.NotEqualsExpression,
                ParenthesizedExpression(converted), Token(TriviaList(Space), SyntaxKind.ExclamationEqualsToken, TriviaList(Space)), zero));
        }
        // Preserve operand trivia in place and collect only trivia from the
        // removed call/cast shell. No text substitutions or whole-file format.
        var removed = node.DescendantTrivia().Where(t => !retained.FullSpan.Contains(t.Span)).ToArray();
        replacement = replacement.WithLeadingTrivia(removed.Where(t => t.Span.End <= retained.FullSpan.Start))
            .WithTrailingTrivia(removed.Where(t => t.Span.Start >= retained.FullSpan.End));
        ++Rewritten;
        onRewritten?.Invoke(node);
        return replacement;
    }

    private bool ProveNormalizer(IMethodSymbol normalizer, ITypeSymbol type)
    {
        if (!normalizerProofs.TryGetValue(normalizer, out var result))
            normalizerProofs.Add(normalizer, result = CheckNormalizer(normalizer, type));
        return result;
    }

    private bool CheckNormalizer(IMethodSymbol normalizer, ITypeSymbol type)
    {
        if (!Same(normalizer.ContainingType, type) || normalizer.MethodKind != MethodKind.Conversion
            || !Same(normalizer.ReturnType, type) || normalizer.Parameters.Length != 1
            || normalizer.GetAttributes().Length != 0 || type is not INamedTypeSymbol { IsReadOnly: true } structure
            || structure.StaticConstructors.Length != 0 || Body(normalizer) is not { } body) return false;
        var parameter = normalizer.Parameters[0];
        if (Operation(body) is not IObjectCreationOperation { Arguments.Length: 1, Constructor: { } constructor } creation
            || constructor.DeclaringSyntaxReferences.SingleOrDefault()?.GetSyntax() is not ConstructorDeclarationSyntax
            { Initializer: null, Body: { Statements.Count: 1 } constructorBody }
            || constructorBody.Statements[0] is not ExpressionStatementSyntax { Expression: AssignmentExpressionSyntax assignment }
            || Operation(assignment) is not ISimpleAssignmentOperation { Target: IFieldReferenceOperation field, Value: IParameterReferenceOperation value }
            || !Same(value.Parameter, constructor.Parameters[0]) || field.Field.Type.SpecialType != SpecialType.System_Byte
            || !field.Field.IsReadOnly || field.Field.IsStatic || !Same(field.Field.ContainingType, type)
            || structure.GetMembers().OfType<IFieldSymbol>().Any(f => !Same(f, field.Field))
            || field.Field.DeclaringSyntaxReferences.Any(r => r.GetSyntax() is VariableDeclaratorSyntax { Initializer: not null })) return false;
        // Verify that CBool's int read is precisely the stored byte, so the
        // eliminated normalize/read pair has no hidden user-code effects.
        var reader = structure.GetMembers("op_Implicit").OfType<IMethodSymbol>().SingleOrDefault(m =>
            m.Parameters.Length == 1 && Same(m.Parameters[0].Type, type) && m.ReturnType.SpecialType == SpecialType.System_Int32);
        if (reader is null || reader.GetAttributes().Length != 0 || Body(reader) is not { } readBody
            || Implicit(Operation(readBody)) is not IFieldReferenceOperation readField || !Same(readField.Field, field.Field)
            || !Parameter(readField.Instance, reader.Parameters[0])) return false;
        var argument = creation.Arguments[0].Value;
        if (argument is not IConversionOperation { Type.SpecialType: SpecialType.System_Byte, OperatorMethod: null, Operand: IConditionalOperation conditional }
            || !Zero(conditional.WhenFalse) || conditional.WhenTrue.ConstantValue is not { HasValue: true, Value: 1 }) return false;
        if (parameter.Type.SpecialType == SpecialType.System_Boolean) return Parameter(conditional.Condition, parameter);
        return conditional.Condition is IBinaryOperation { OperatorKind: BinaryOperatorKind.NotEquals, OperatorMethod: null } binary
            && Parameter(binary.LeftOperand, parameter)
            && (Numeric(parameter.Type) && Zero(binary.RightOperand) || parameter.Type is IPointerTypeSymbol && Null(binary.RightOperand));
    }
}
