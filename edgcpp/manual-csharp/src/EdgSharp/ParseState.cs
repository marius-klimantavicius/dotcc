// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Handwritten adaptation of lexical/scope checkpoint contracts. Scope activation
// is represented by CurrentScope and its parent chain; inactive scopes remain
// retained in the graph. Namespace/class/other-symbol states are not implemented.
namespace EdgSharp;

internal sealed class ParseState
{
    private readonly List<Action> undo;
    private readonly List<ParseCheckpoint> checkpoints = [];
    private readonly bool replayState;
    internal TokenCursor Tokens { get; }
    internal DiagnosticBag Diagnostics { get; }
    internal Scope GlobalScope { get; }
    internal Scope CurrentScope { get; private set; }
    internal FunctionDeclaration? CurrentFunction { get; set; }
    // Explicit retention boundary for later deferred rescanning/instantiation.
    internal List<TokenBuffer> PendingReplays { get; } = [];
    private readonly List<StaticAssertion> assertions = [];
    internal IReadOnlyList<StaticAssertion> Assertions => assertions;
    internal ParseState(TokenCursor tokens, DiagnosticBag diagnostics, Scope? globalScope = null, ParseState? mutationOwner = null)
    {
        (Tokens, Diagnostics) = (tokens, diagnostics);
        undo = mutationOwner?.undo ?? [];
        replayState = mutationOwner != null;
        GlobalScope = globalScope ?? new Scope(ScopeKind.TranslationUnit, null);
        CurrentScope = GlobalScope;
    }
    internal void AddAssertion(StaticAssertion assertion)
    {
        assertions.Add(assertion);
        undo.Add(() => assertions.RemoveAt(assertions.Count - 1));
    }
    internal void AddInstanceDeclaration(FunctionDeclaration declaration)
    {
        declaration.Symbol.Add(declaration);
        undo.Add(declaration.Symbol.RemoveLastDeclaration);
    }
    internal Scope PushScope(ScopeKind kind)
    {
        var scope = new Scope(kind, CurrentScope);
        Scope parent = CurrentScope;
        parent.AddChild(scope);
        undo.Add(parent.RemoveLastChild);
        return CurrentScope = scope;
    }
    internal void PopScope() => CurrentScope = CurrentScope.Parent ?? throw new InvalidOperationException("Cannot pop global scope.");
    internal void ActivateRetainedScope(Scope scope)
    {
        if (!ReferenceEquals(scope.Parent, CurrentScope)) throw new InvalidOperationException("Retained scope has a different parent.");
        CurrentScope = scope;
    }
    internal void Enter(Symbol symbol)
    {
        symbol.Scope.Enter(symbol);
        undo.Add(() => symbol.Scope.Remove(symbol));
    }
    internal void AddDeclaration(Declaration declaration)
    {
        declaration.Symbol.Scope.Add(declaration);
        declaration.Symbol.Add(declaration);
        undo.Add(() => { declaration.Symbol.RemoveLastDeclaration(); declaration.Symbol.Scope.RemoveLastDeclaration(); });
    }
    internal void SetDefinition(Symbol symbol, FunctionDeclaration declaration)
    {
        var previous = symbol.Definition;
        symbol.Definition = declaration;
        undo.Add(() => symbol.Definition = previous);
    }
    internal void SetTemplateDefinition(Symbol symbol, FunctionTemplateDeclaration declaration)
    {
        var previous = symbol.TemplateDefinition;
        symbol.TemplateDefinition = declaration;
        undo.Add(() => symbol.TemplateDefinition = previous);
    }
    internal void RegisterInstance(FunctionTemplateDeclaration template, FunctionTemplateInstance instance)
    {
        template.AddInstance(instance);
        undo.Add(() => template.RemoveLastInstance(instance));
    }
    internal void UpdateInstance(FunctionTemplateInstance instance, TemplateInstanceState status,
        FunctionDeclaration? declaration = null, bool replay = false)
    {
        var previous = (instance.State, instance.Declaration, instance.BodyReplayCount);
        instance.State = status;
        if (declaration != null) instance.Declaration = declaration;
        if (replay) ++instance.BodyReplayCount;
        undo.Add(() => (instance.State, instance.Declaration, instance.BodyReplayCount) = previous);
    }
    internal void SetDependentValue(Symbol symbol, bool dependent)
    {
        bool previous = symbol.HasTemplateDependentValue;
        symbol.HasTemplateDependentValue = dependent;
        undo.Add(() => symbol.HasTemplateDependentValue = previous);
    }
    internal void SetVariableType(VariableDeclaration declaration, SemanticType type)
    {
        SemanticType previous = declaration.Type;
        declaration.Type = declaration.Symbol.Type = type;
        undo.Add(() => declaration.Type = declaration.Symbol.Type = previous);
    }
    internal ParseCheckpoint Checkpoint()
    {
        if (replayState) throw new InvalidOperationException("Replay states share mutations with their owner and cannot create independent checkpoints.");
        var checkpoint = new ParseCheckpoint(this, Tokens.Checkpoint(), CurrentScope, CurrentFunction, Diagnostics.Count, undo.Count, PendingReplays.ToArray());
        checkpoints.Add(checkpoint);
        return checkpoint;
    }
    internal void Restore(ParseCheckpoint checkpoint)
    {
        if (replayState) throw new InvalidOperationException("Only the replay mutation owner can restore checkpoints.");
        if (!ReferenceEquals(checkpoint.Owner, this)) throw new ArgumentException("Checkpoint belongs to another parse state.", nameof(checkpoint));
        int index = checkpoints.FindIndex(c => ReferenceEquals(c, checkpoint));
        // Validate the branch before changing any state. Counts alone cannot identify a parse branch.
        if (index < 0 || checkpoint.UndoCount > undo.Count || checkpoint.DiagnosticCount > Diagnostics.Count)
            throw new ArgumentException("Checkpoint was invalidated by an earlier rollback.", nameof(checkpoint));
        checkpoints.RemoveRange(index + 1, checkpoints.Count - index - 1);
        for (int i = undo.Count - 1; i >= checkpoint.UndoCount; --i) undo[i]();
        undo.RemoveRange(checkpoint.UndoCount, undo.Count - checkpoint.UndoCount);
        CurrentScope = checkpoint.Scope;
        CurrentFunction = checkpoint.Function;
        Diagnostics.Truncate(checkpoint.DiagnosticCount);
        PendingReplays.Clear();
        PendingReplays.AddRange(checkpoint.PendingReplays);
        Tokens.Restore(checkpoint.Tokens);
    }
}

internal sealed class ParseCheckpoint(ParseState owner, TokenCheckpoint tokens, Scope scope,
    FunctionDeclaration? function, int diagnosticCount, int undoCount, TokenBuffer[] pendingReplays)
{
    internal ParseState Owner { get; } = owner;
    internal TokenCheckpoint Tokens { get; } = tokens;
    internal Scope Scope { get; } = scope;
    internal FunctionDeclaration? Function { get; } = function;
    internal int DiagnosticCount { get; } = diagnosticCount;
    internal int UndoCount { get; } = undoCount;
    internal TokenBuffer[] PendingReplays { get; } = pendingReplays;
}
