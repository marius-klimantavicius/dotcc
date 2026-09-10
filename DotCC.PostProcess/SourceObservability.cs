using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace DotCC.PostProcess;

internal static class SourceObservability
{
    internal static bool IsObservable(SyntaxNode node, SemanticModel model, CancellationToken token)
    {
        foreach (var ancestor in node.Ancestors())
        {
            // Query providers receive compiler-generated expression-tree
            // lambdas even though the source contains no lambda syntax.
            if (ancestor is QueryExpressionSyntax) return true;
            if (ancestor is AnonymousFunctionExpressionSyntax lambda
                && model.GetTypeInfo(lambda, token).ConvertedType is INamedTypeSymbol { Name: "Expression", Arity: 1 } type
                && type.ContainingNamespace.ToDisplayString() == "System.Linq.Expressions") return true;
            var parameters = ancestor switch
            {
                InvocationExpressionSyntax call when model.GetOperation(call, token) is IInvocationOperation invocation => invocation.TargetMethod.Parameters,
                BaseObjectCreationExpressionSyntax call when model.GetOperation(call, token) is IObjectCreationOperation creation => creation.Constructor?.Parameters ?? [],
                ConstructorInitializerSyntax call when model.GetOperation(call, token) is IInvocationOperation invocation => invocation.TargetMethod.Parameters,
                ElementAccessExpressionSyntax access when model.GetOperation(access, token) is IPropertyReferenceOperation property => property.Property.Parameters,
                ElementBindingExpressionSyntax access when model.GetOperation(access, token) is IPropertyReferenceOperation property => property.Property.Parameters,
                _ => []
            };
            if (parameters.Any(p => p.GetAttributes().Any(a =>
                    a.AttributeClass?.ToDisplayString() == "System.Runtime.CompilerServices.CallerArgumentExpressionAttribute"))) return true;
        }
        return false;
    }
}
