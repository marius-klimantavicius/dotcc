// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Authored managed session ownership; no native frontend or exporter dependency.
using System.Collections.ObjectModel;

namespace EdgSharp;

public sealed class LexicalResult
{
    public TokenBuffer Tokens { get; }
    public ReadOnlyCollection<Diagnostic> Diagnostics { get; }
    public bool IsSuccess => Diagnostics.All(d => d.Severity != DiagnosticSeverity.Error);
    internal LexicalResult(TokenBuffer tokens, ReadOnlyCollection<Diagnostic> diagnostics) => (Tokens, Diagnostics) = (tokens, diagnostics);
}

public sealed class AnalysisResult
{
    public SourceFile Source { get; }
    public TokenBuffer Tokens { get; }
    public Scope GlobalScope { get; }
    public ReadOnlyCollection<Declaration> Declarations => GlobalScope.Declarations;
    public ReadOnlyCollection<Diagnostic> Diagnostics { get; }
    public ReadOnlyCollection<StaticAssertion> StaticAssertions { get; }
    public PreprocessingResult Preprocessing { get; }
    public ReadOnlyCollection<SourceFile> PhysicalSources => Preprocessing.PhysicalSources;
    /// <summary>False on either invalid source or unsupported input. The graph then contains partial, diagnosed facts.</summary>
    public bool IsSuccess => Diagnostics.All(d => d.Severity != DiagnosticSeverity.Error);
    public bool ContainsUnsupportedFeatures => Diagnostics.Any(d => d.Class == DiagnosticClass.UnsupportedFeature);
    internal AnalysisResult(PreprocessingResult preprocessing, Scope global, ReadOnlyCollection<Diagnostic> diagnostics, StaticAssertion[] assertions)
        => (Source, Tokens, Preprocessing, GlobalScope, Diagnostics, StaticAssertions) =
            (preprocessing.Source, preprocessing.Tokens, preprocessing, global, diagnostics, Array.AsReadOnly(assertions));
}

/// <summary>
/// Owns canonical types and analysis state. Analyses are serialized within a session; separate sessions are independent.
/// Disposal ends analysis permission, while existing immutable source and completed graph queries remain valid.
/// Queries never instantiate templates or perform additional analysis.
/// </summary>
public sealed class CompilationSession : IDisposable
{
    private readonly object gate = new();
    private bool disposed;
    private readonly TypeTable types;
    public AnalysisOptions Options { get; }
    public CompilationSession(AnalysisOptions? options = null)
    {
        options ??= new AnalysisOptions();
        if (options.Language != LanguageProfile.Cpp20) throw new ArgumentOutOfRangeException(nameof(options), "Only C++20 is implemented as a profile.");
        ArgumentNullException.ThrowIfNull(options.Target);
        ArgumentNullException.ThrowIfNull(options.Preprocessing);
        if (options.ConstantEvaluationStepLimit <= 0 || options.ConstantEvaluationCallLimit <= 0 || options.TemplateInstantiationDepthLimit <= 0)
            throw new ArgumentOutOfRangeException(nameof(options), "Evaluation and instantiation limits must be positive.");
        Options = options with { Preprocessing = options.Preprocessing.CaptureTranslationTimestamp() };
        types = new TypeTable(options.Target);
    }
    public LexicalResult Lex(string text, string fileName = "input.cpp")
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(fileName);
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            var source = new SourceFile(fileName, text, encoding: SourceEncodingInfo.Managed(Options.Preprocessing.SourceInput));
            var diagnostics = new DiagnosticBag();
            if (!source.Encoding.Validate(source, diagnostics))
                return new LexicalResult(new TokenBuffer(source, [new Token(TokenKind.EndOfSource, source.Range(text.Length, 0))]), diagnostics.Snapshot());
            return new LexicalResult(new Lexer(LogicalInput.Normalize(source), diagnostics).Scan(), diagnostics.Snapshot());
        }
    }
    public PreprocessingResult Preprocess(string text, string fileName = "input.cpp")
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(fileName);
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            return new Preprocessor(Options.Preprocessing, new DiagnosticBag(), Options.Target).Process(
                new SourceFile(fileName, text, encoding: SourceEncodingInfo.Managed(Options.Preprocessing.SourceInput)));
        }
    }
    public AnalysisResult Analyze(string text, string fileName = "input.cpp")
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(fileName);
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            var source = new SourceFile(fileName, text, encoding: SourceEncodingInfo.Managed(Options.Preprocessing.SourceInput));
            var diagnostics = new DiagnosticBag();
            PreprocessingResult preprocessing = new Preprocessor(Options.Preprocessing, diagnostics, Options.Target).Process(source);
            TokenBuffer tokens = preprocessing.Tokens;
            var state = new ParseState(tokens.CreateCursor(), diagnostics);
            new Parser(state, types, Options).TranslationUnit();
            return new AnalysisResult(preprocessing, state.GlobalScope, diagnostics.Snapshot(), state.Assertions.ToArray());
        }
    }
    public void Dispose() { lock (gate) disposed = true; }
}
