using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DotCC.PostProcess;

public sealed record RewriteResult(CSharpCompilation Compilation, int Rewritten, int Skipped,
    IReadOnlyList<string> Diagnostics)
{
    public int RemovedEmptyBlocks { get; init; }
    public int SimplifiedBooleanComparisons { get; init; }
}

/// <summary>Optional, post-emission tree rewrite. All bindings come from the
/// original compilation; rewritten subexpressions retain their boolean type.</summary>
public static class CondInliner
{
    public static RewriteResult Rewrite(CSharpCompilation compilation, CancellationToken cancellationToken = default)
        => Rewrite(compilation, cancellationToken, shouldRewrite: null);

    internal static RewriteResult Rewrite(CSharpCompilation compilation, CancellationToken cancellationToken,
        Func<SyntaxTree, bool>? shouldRewrite)
    {
        CheckErrors(compilation, "Input", cancellationToken);
        var diagnostics = new List<string>();
        var replacements = new List<(SyntaxTree Old, SyntaxTree New)>();
        int rewritten = 0, skipped = 0;
        foreach (var tree in compilation.SyntaxTrees)
        {
            if (shouldRewrite != null && !shouldRewrite(tree)) continue;
            cancellationToken.ThrowIfCancellationRequested();
            var visitor = ConditionRewriter.Create(compilation.GetSemanticModel(tree), cancellationToken, diagnostics);
            if (visitor is null) continue;
            var root = visitor.Visit(tree.GetRoot(cancellationToken))!;
            rewritten += visitor.Rewritten; skipped += visitor.Skipped;
            if (visitor.Rewritten != 0) replacements.Add((tree, tree.WithRootAndOptions(root, tree.Options)));
        }
        var output = compilation;
        foreach (var (oldTree, newTree) in replacements) output = output.ReplaceSyntaxTree(oldTree, newTree);
        CheckErrors(output, "Rewritten", cancellationToken);
        return new(output, rewritten, skipped, diagnostics);
    }

    internal static void CheckErrors(CSharpCompilation compilation, string phase, CancellationToken token)
    {
        var errors = compilation.GetDiagnostics(token).Where(d => d.Severity == DiagnosticSeverity.Error).Take(20).ToArray();
        if (errors.Length != 0) throw new InvalidOperationException(phase + " compilation failed:\n" + string.Join("\n", errors.Select(d => d.ToString())));
    }
}
