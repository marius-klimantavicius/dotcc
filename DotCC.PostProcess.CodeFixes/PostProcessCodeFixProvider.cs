using System.Collections.Immutable;
using System.Composition;
using DotCC.PostProcess.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

namespace DotCC.PostProcess.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PostProcessCodeFixProvider)), Shared]
public sealed class PostProcessCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        [PostProcessAnalyzer.InlineConditionId, PostProcessAnalyzer.EmptyBlockId, PostProcessAnalyzer.BooleanComparisonId];
    public override FixAllProvider GetFixAllProvider() => new DocumentFixAllProvider();

    public override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        foreach (var diagnostic in context.Diagnostics)
        {
            var id = diagnostic.Id;
            if (!FixableDiagnosticIds.Contains(id)) continue;
            context.RegisterCodeFix(CodeAction.Create(Title(id),
                token => RewriteAsync(context.Document, id, [diagnostic.Location.SourceSpan], token), id), diagnostic);
        }
        return Task.CompletedTask;
    }

    private static string Title(string id) => id switch
    {
        PostProcessAnalyzer.InlineConditionId => "Inline Cond.B",
        PostProcessAnalyzer.BooleanComparisonId => "Simplify boolean comparison",
        _ => "Remove standalone empty block"
    };

    private static async Task<Document> RewriteAsync(Document document, string id, IEnumerable<TextSpan> spans, CancellationToken token)
    {
        var model = await document.GetSemanticModelAsync(token).ConfigureAwait(false);
        var root = await document.GetSyntaxRootAsync(token).ConfigureAwait(false);
        if (model == null || root == null
            || model.GetDiagnostics(cancellationToken: token).Any(d => d.Severity == DiagnosticSeverity.Error)) return document;
        var selected = new SpanSelection(spans);
        // Selection uses original spans. Fixing an outer expression/block also
        // fixes its eligible descendants, but never unrelated siblings.
        var rewritten = id switch
        {
            PostProcessAnalyzer.InlineConditionId =>
                ConditionRewriter.Create(model, token, include: node => selected.Contains(node.Span))?.Visit(root) ?? root,
            PostProcessAnalyzer.BooleanComparisonId =>
                new BooleanComparisonRewriter(model, token, include: node => selected.Contains(node.Span)).Visit(root)!,
            _ => new EmptyBlockRewriter(model, token, include: node => selected.Contains(node.Span)).Visit(root)!
        };
        // Text round-trip avoids formatter/simplifier passes by the code-action
        // host and rebinds exactly what Rider will save to disk.
        var changed = document.WithText(SourceText.From(rewritten.ToFullString(), (await document.GetTextAsync(token).ConfigureAwait(false)).Encoding));
        var changedModel = await changed.GetSemanticModelAsync(token).ConfigureAwait(false);
        return changedModel != null && !changedModel.GetDiagnostics(cancellationToken: token).Any(d => d.Severity == DiagnosticSeverity.Error)
            ? changed : document;
    }

    // Merge nested intervals and binary-search them: SQLite has tens of
    // thousands of diagnostics, so a scan of all spans per node is quadratic.
    private sealed class SpanSelection
    {
        private readonly List<TextSpan> intervals = [];
        internal SpanSelection(IEnumerable<TextSpan> spans)
        {
            foreach (var span in spans.OrderBy(s => s.Start).ThenByDescending(s => s.Length))
                if (intervals.Count == 0 || !intervals[intervals.Count - 1].Contains(span)) intervals.Add(span);
        }
        internal bool Contains(TextSpan span)
        {
            int low = 0, high = intervals.Count - 1;
            while (low <= high)
            {
                int middle = low + (high - low) / 2;
                if (intervals[middle].Start <= span.Start) low = middle + 1;
                else high = middle - 1;
            }
            return high >= 0 && intervals[high].Contains(span);
        }
    }

    private sealed class DocumentFixAllProvider : FixAllProvider
    {
        public override IEnumerable<FixAllScope> GetSupportedFixAllScopes() => [FixAllScope.Document, FixAllScope.Project, FixAllScope.Solution];

        public override async Task<CodeAction?> GetFixAsync(FixAllContext context)
        {
            var id = context.CodeActionEquivalenceKey;
            if (id is not (PostProcessAnalyzer.InlineConditionId or PostProcessAnalyzer.EmptyBlockId or PostProcessAnalyzer.BooleanComparisonId)) return null;
            var documents = context.Scope switch
            {
                FixAllScope.Document => context.Document == null ? [] : new[] { context.Document },
                FixAllScope.Project => context.Project.Documents,
                FixAllScope.Solution => context.Solution.Projects.Where(p => p.Language == LanguageNames.CSharp).SelectMany(p => p.Documents),
                _ => Enumerable.Empty<Document>()
            };
            var selected = new List<(Document Document, ImmutableArray<TextSpan> Spans)>();
            foreach (var document in documents)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                var diagnostics = await context.GetDocumentDiagnosticsAsync(document).ConfigureAwait(false);
                var spans = diagnostics.Where(d => d.Id == id && !d.IsSuppressed && d.Location.IsInSource)
                    .Select(d => d.Location.SourceSpan).ToImmutableArray();
                if (spans.Length != 0) selected.Add((document, spans));
            }
            if (selected.Count == 0) return null;
            return CodeAction.Create(Title(id) + " (Fix all)", async token =>
            {
                var solution = context.Solution;
                foreach (var item in selected)
                {
                    token.ThrowIfCancellationRequested();
                    var changed = await RewriteAsync(item.Document, id!, item.Spans, token).ConfigureAwait(false);
                    solution = solution.WithDocumentText(item.Document.Id, await changed.GetTextAsync(token).ConfigureAwait(false));
                }
                return solution;
            }, id);
        }
    }
}
