// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Handwritten adaptation of lexical/scope checkpoint contracts. Scope activation
// is represented by CurrentScope and its parent chain; inactive scopes remain
// retained in the graph. Namespace/class/other-symbol states are not implemented.
namespace EdgSharp;

internal sealed class ParseState
{
    private readonly List<Action> undo = [];
    private readonly List<ParseCheckpoint> checkpoints = [];
    internal TokenCursor Tokens { get; }
    internal DiagnosticBag Diagnostics { get; }
    internal Scope GlobalScope { get; } = new(ScopeKind.TranslationUnit, null);
    internal Scope CurrentScope { get; private set; }
    internal FunctionDeclaration? CurrentFunction { get; set; }
    // Explicit retention boundary for later deferred rescanning/instantiation.
    internal List<TokenBuffer> PendingReplays { get; } = [];
    internal ParseState(TokenCursor tokens, DiagnosticBag diagnostics)
    {
        (Tokens, Diagnostics) = (tokens, diagnostics);
        CurrentScope = GlobalScope;
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
        undo.Add(() => symbol.Scope.Remove(symbol.Name));
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
    internal ParseCheckpoint Checkpoint()
    {
        var checkpoint = new ParseCheckpoint(this, Tokens.Checkpoint(), CurrentScope, CurrentFunction, Diagnostics.Count, undo.Count, PendingReplays.ToArray());
        checkpoints.Add(checkpoint);
        return checkpoint;
    }
    internal void Restore(ParseCheckpoint checkpoint)
    {
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
