using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

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
            var cleanup = new EmptyBlocks(result.Compilation.GetSemanticModel(tree), cancellationToken);
            var root = cleanup.Visit(tree.GetRoot(cancellationToken))!;
            if (cleanup.Removed == 0) continue;
            removed += cleanup.Removed;
            output = output.ReplaceSyntaxTree(tree, tree.WithRootAndOptions(root, tree.Options));
        }
        if (removed != 0) CondInliner.CheckErrors(output, "Cleaned", cancellationToken);
        return result with { Compilation = output, RemovedEmptyBlocks = removed };
    }

    private sealed class EmptyBlocks(SemanticModel model, CancellationToken token) : CSharpSyntaxRewriter
    {
        public int Removed { get; private set; }
        private static bool Empty(StatementSyntax statement)
            => statement is BlockSyntax { Statements.Count: 0, ContainsDirectives: false };

        public override SyntaxNode? VisitBlock(BlockSyntax node)
        {
            token.ThrowIfCancellationRequested();
            // A lambda's braces can be part of captured caller-argument text.
            if (SourceObservability.IsObservable(node, model, token)) return node;
            var visited = (BlockSyntax)base.VisitBlock(node)!;
            var statements = Remove(visited.Statements, Empty, out var trailing);
            // Remove statements from lists only: the containing block itself
            // remains the required method/if/loop/try/lambda/etc. body.
            return visited.WithStatements(statements).WithCloseBraceToken(
                visited.CloseBraceToken.WithLeadingTrivia(trailing.AddRange(visited.CloseBraceToken.LeadingTrivia)));
        }

        public override SyntaxNode? VisitSwitchSection(SwitchSectionSyntax node)
        {
            var visited = (SwitchSectionSyntax)base.VisitSwitchSection(node)!;
            var statements = Remove(visited.Statements, Empty, out var trailing);
            // Valid switch sections have a terminating statement. Keep trailing
            // trivia on it instead of losing comments from a removed last block.
            if (trailing.Count != 0 && statements.Count != 0)
                statements = statements.Replace(statements.Last(), statements.Last().WithTrailingTrivia(
                    statements.Last().GetTrailingTrivia().AddRange(trailing)));
            return visited.WithStatements(statements);
        }

        public override SyntaxNode? VisitCompilationUnit(CompilationUnitSyntax node)
        {
            var visited = (CompilationUnitSyntax)base.VisitCompilationUnit(node)!;
            var globals = visited.Members.OfType<GlobalStatementSyntax>().ToArray();
            // An otherwise empty top-level program still needs its implicit Main.
            int keep = globals.Length != 0 && globals.All(g => Empty(g.Statement)) ? globals[^1].SpanStart : -1;
            var members = Remove(visited.Members,
                m => m is GlobalStatementSyntax g && Empty(g.Statement) && g.SpanStart != keep, out var trailing);
            return visited.WithMembers(members).WithEndOfFileToken(
                visited.EndOfFileToken.WithLeadingTrivia(trailing.AddRange(visited.EndOfFileToken.LeadingTrivia)));
        }

        private SyntaxList<T> Remove<T>(SyntaxList<T> nodes, Func<T, bool> removable, out SyntaxTriviaList trailing)
            where T : SyntaxNode
        {
            var kept = new List<T>();
            trailing = default;
            foreach (var node in nodes)
            {
                token.ThrowIfCancellationRequested();
                if (removable(node))
                {
                    // Retain comments, whitespace and line breaks in lexical
                    // order. Directive-bearing blocks are never candidates.
                    trailing = trailing.AddRange(node.DescendantTrivia());
                    ++Removed;
                }
                else
                {
                    kept.Add(trailing.Count == 0 ? node : (T)node.WithLeadingTrivia(trailing.AddRange(node.GetLeadingTrivia())));
                    trailing = default;
                }
            }
            return List(kept);
        }
    }
}
