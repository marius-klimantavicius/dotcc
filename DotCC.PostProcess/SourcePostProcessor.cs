using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DotCC.PostProcess;

/// <summary>Standalone semantic inlining followed by conservative tree cleanup.</summary>
public static class SourcePostProcessor
{
    public static RewriteResult Rewrite(CSharpCompilation compilation, CancellationToken cancellationToken = default)
    {
        var result = CondInliner.Rewrite(compilation, cancellationToken);
        var output = result.Compilation;
        int removed = 0;
        foreach (var tree in result.Compilation.SyntaxTrees)
        {
            var cleanup = new EmptyBlockRewriter(result.Compilation.GetSemanticModel(tree), cancellationToken);
            var root = cleanup.Visit(tree.GetRoot(cancellationToken))!;
            if (cleanup.Removed == 0) continue;
            removed += cleanup.Removed;
            output = output.ReplaceSyntaxTree(tree, tree.WithRootAndOptions(root, tree.Options));
        }
        if (removed != 0) CondInliner.CheckErrors(output, "Cleaned", cancellationToken);
        return result with { Compilation = output, RemovedEmptyBlocks = removed };
    }
}
