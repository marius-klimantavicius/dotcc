using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace DotCC.PostProcess;

// Shared by the standalone tool and IDE diagnostics/fixes. Callbacks and
// selection always use original nodes, even when child removals shift spans.
internal sealed class EmptyBlockRewriter(SemanticModel model, CancellationToken token,
    Action<BlockSyntax>? onRemoved = null, Func<BlockSyntax, bool>? include = null) : CSharpSyntaxRewriter
{
    public int Removed { get; private set; }
    private static bool Empty(StatementSyntax statement)
        => statement is BlockSyntax { Statements.Count: 0, ContainsDirectives: false };

    public override SyntaxNode? VisitBlock(BlockSyntax node)
    {
        token.ThrowIfCancellationRequested();
        if (SourceObservability.IsObservable(node, model, token)) return node;
        var visited = (BlockSyntax)base.VisitBlock(node)!;
        var statements = Remove(visited.Statements, node.Statements, Empty, out var trailing);
        return visited.WithStatements(statements).WithCloseBraceToken(
            visited.CloseBraceToken.WithLeadingTrivia(trailing.AddRange(visited.CloseBraceToken.LeadingTrivia)));
    }

    public override SyntaxNode? VisitSwitchSection(SwitchSectionSyntax node)
    {
        var visited = (SwitchSectionSyntax)base.VisitSwitchSection(node)!;
        var statements = Remove(visited.Statements, node.Statements, Empty, out var trailing);
        if (trailing.Count != 0 && statements.Count != 0)
            statements = statements.Replace(statements.Last(), statements.Last().WithTrailingTrivia(
                statements.Last().GetTrailingTrivia().AddRange(trailing)));
        return visited.WithStatements(statements);
    }

    public override SyntaxNode? VisitCompilationUnit(CompilationUnitSyntax node)
    {
        var visited = (CompilationUnitSyntax)base.VisitCompilationUnit(node)!;
        var globals = visited.Members.OfType<GlobalStatementSyntax>().ToArray();
        // Preserve the implicit Main of an otherwise empty top-level program.
        int keep = globals.Length != 0 && globals.All(g => Empty(g.Statement)) ? globals[globals.Length - 1].SpanStart : -1;
        var members = Remove(visited.Members, node.Members,
            m => m is GlobalStatementSyntax g && Empty(g.Statement) && g.SpanStart != keep, out var trailing);
        return visited.WithMembers(members).WithEndOfFileToken(
            visited.EndOfFileToken.WithLeadingTrivia(trailing.AddRange(visited.EndOfFileToken.LeadingTrivia)));
    }

    private SyntaxList<T> Remove<T>(SyntaxList<T> nodes, SyntaxList<T> originals,
        Func<T, bool> removable, out SyntaxTriviaList trailing) where T : SyntaxNode
    {
        var kept = new List<T>();
        trailing = default;
        for (int index = 0; index < nodes.Count; index++)
        {
            token.ThrowIfCancellationRequested();
            var node = nodes[index];
            var original = originals[index] is GlobalStatementSyntax global ? global.Statement : (SyntaxNode)originals[index];
            if (removable(node) && original is BlockSyntax block && (include == null || include(block)))
            {
                // Retain lexical trivia, including line breaks used by caller
                // information. Required bodies are never entries in these lists.
                trailing = trailing.AddRange(node.DescendantTrivia());
                ++Removed;
                onRemoved?.Invoke(block);
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
