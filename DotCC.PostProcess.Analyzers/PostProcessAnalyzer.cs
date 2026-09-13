using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DotCC.PostProcess.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PostProcessAnalyzer : DiagnosticAnalyzer
{
    public const string InlineConditionId = "DCCPP001";
    public const string EmptyBlockId = "DCCPP002";
    public const string BooleanComparisonId = "DCCPP003";

    private static readonly DiagnosticDescriptor InlineCondition = new(InlineConditionId,
        "Inline Cond.B", "Inline this proven Cond.B truth conversion", "Readability",
        DiagnosticSeverity.Info, isEnabledByDefault: true,
        description: "Replace a proven dotcc Cond.B call with its equivalent boolean expression.");
    private static readonly DiagnosticDescriptor EmptyBlock = new(EmptyBlockId,
        "Remove standalone empty block", "Remove this standalone empty block", "Readability",
        DiagnosticSeverity.Info, isEnabledByDefault: true,
        description: "Remove an empty block statement while retaining required bodies and source trivia.");

    private static readonly DiagnosticDescriptor BooleanComparison = new(BooleanComparisonId,
        "Simplify boolean comparison", "Simplify this comparison of a boolean encoded as 0/1", "Readability",
        DiagnosticSeverity.Info, isEnabledByDefault: true,
        description: "Replace a built-in comparison of a 0/1 conditional with its condition or logical negation.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [InlineCondition, EmptyBlock, BooleanComparison];

    public override void Initialize(AnalysisContext context)
    {
        // Emitted C# is the intended input, including .g.cs and auto-generated headers.
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterSemanticModelAction(Analyze);
    }

    private static void Analyze(SemanticModelAnalysisContext context)
    {
        var model = context.SemanticModel;
        var token = context.CancellationToken;
        // Editing may temporarily leave an invalid document. Never offer a
        // transformation against incomplete bindings; retry after it is valid.
        if (model.GetDiagnostics(cancellationToken: token).Any(d => d.Severity == DiagnosticSeverity.Error)) return;
        var root = model.SyntaxTree.GetRoot(token);
        ConditionRewriter.Create(model, token, onRewritten: node =>
            context.ReportDiagnostic(Diagnostic.Create(InlineCondition, node.GetLocation())))?.Visit(root);
        new BooleanComparisonRewriter(model, token, onSimplified: node =>
            context.ReportDiagnostic(Diagnostic.Create(BooleanComparison, node.GetLocation()))).Visit(root);
        new EmptyBlockRewriter(model, token, onRemoved: node =>
            context.ReportDiagnostic(Diagnostic.Create(EmptyBlock, node.GetLocation()))).Visit(root);
    }
}
