using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DotCC.PostProcess;

/// <summary>Standalone semantic inlining followed by conservative tree cleanup.</summary>
public static class SourcePostProcessor
{
    public static RewriteResult Rewrite(CSharpCompilation compilation, CancellationToken cancellationToken = default)
        => Rewrite(compilation, cancellationToken, shouldRewrite: null);

    internal static RewriteResult Rewrite(CSharpCompilation compilation, CancellationToken cancellationToken,
        Func<SyntaxTree, bool>? shouldRewrite)
    {
        var result = CondInliner.Rewrite(compilation, cancellationToken, shouldRewrite);
        var output = result.Compilation;
        int simplified = 0, removed = 0;
        // Bind again after Cond.B/CBool inlining: those rewrites introduce the
        // numeric comparisons that this pass can now recognize.
        foreach (var tree in result.Compilation.SyntaxTrees)
        {
            if (shouldRewrite != null && !shouldRewrite(tree)) continue;
            var comparisons = new BooleanComparisonRewriter(result.Compilation.GetSemanticModel(tree), cancellationToken);
            var root = comparisons.Visit(tree.GetRoot(cancellationToken))!;
            if (comparisons.Simplified == 0) continue;
            simplified += comparisons.Simplified;
            output = output.ReplaceSyntaxTree(tree, tree.WithRootAndOptions(root, tree.Options));
        }
        if (simplified != 0) CondInliner.CheckErrors(output, "Simplified", cancellationToken);
        var conditions = output;
        foreach (var tree in conditions.SyntaxTrees)
        {
            if (shouldRewrite != null && !shouldRewrite(tree)) continue;
            var cleanup = new EmptyBlockRewriter(conditions.GetSemanticModel(tree), cancellationToken);
            var root = cleanup.Visit(tree.GetRoot(cancellationToken))!;
            if (cleanup.Removed == 0) continue;
            removed += cleanup.Removed;
            output = output.ReplaceSyntaxTree(tree, tree.WithRootAndOptions(root, tree.Options));
        }
        if (removed != 0) CondInliner.CheckErrors(output, "Cleaned", cancellationToken);
        return result with { Compilation = output, RemovedEmptyBlocks = removed, SimplifiedBooleanComparisons = simplified };
    }
}
