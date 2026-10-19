using System;
using System.Collections.Generic;
using LALR.CC;
using LALR.CC.LexicalGrammar;

namespace DotCC;

internal sealed partial class TypeNameRewriter
{
    private readonly int _commaSymbol, _equalsSymbol, _forSymbol;
    private sealed class DeclarationContext(bool parameters = false)
    {
        public bool Parameters { get; } = parameters;
        public bool Active;
        public bool Initializer;
    }
    private readonly Stack<DeclarationContext> _declarations = new();
    private sealed class ForScope(long end, int blockDepth)
    {
        public long End { get; } = end;
        public int BlockDepth { get; } = blockDepth;
        public HashSet<string> Shadows { get; } = new(StringComparer.Ordinal);
    }
    private readonly Stack<ForScope> _forScopes = new();

    // Keep a declaration's comma list at its own delimiter depth. Commas in
    // calls, array bounds, initializers and parameter lists cannot start another
    // declarator using the enclosing declaration's type.
    private bool AdvanceDeclarationContext(Item token) =>
        AdvanceDeclarationContext(token, _declarations, _afterDeclarationType, _afterTagKeyword);

    private bool AdvanceDeclarationContext(Item token, Stack<DeclarationContext> contexts,
        bool afterType, bool afterTag)
    {
        var context = contexts.Peek();
        if (token.ID == _openParenSymbol || token.ID == _openBracketSymbol || token.ID == _openBraceSymbol)
        {
            var parameters = token.ID == _openParenSymbol && context.Active && !context.Initializer
                && !afterType && _previousSymbol != _forSymbol;
            contexts.Push(new DeclarationContext(parameters));
        }
        else if (token.ID == _closeParenSymbol || token.ID == _closeBracketSymbol || token.ID == _closeBraceSymbol)
        {
            if (contexts.Count > 1) contexts.Pop();
        }
        else if (token.ID == _semiSymbol)
        {
            context.Active = context.Initializer = false;
        }
        else if (token.ID == _equalsSymbol && context.Active)
        {
            context.Initializer = true;
        }
        else if (token.ID == _commaSymbol)
        {
            context.Initializer = false;
            if (context.Parameters) context.Active = false;
            return context.Active;
        }
        else if (_typeSpecifiers.Contains(token.ID) || afterTag)
        {
            context.Active = true;
        }
        return false;
    }

    private void RestoreCompletedForScopes()
    {
        while (_forScopes.TryPeek(out var scope) && _lookahead.Position > scope.End)
        {
            foreach (var name in _forScopes.Pop().Shadows) _typeNames.Add(name);
        }
    }

    // Only for-statements need lookahead: their declaration scope includes a
    // possibly unbraced statement (including nested loops and if/else). Buffer
    // tokens without rewriting them, so typedef handling and source positions
    // continue through the ordinary stream exactly once.
    private sealed class DeclarationLookahead(ISyncIterator<Item> inner) : ISyncIterator<Item>
    {
        private readonly List<Item> _buffer = new();
        private static readonly Dictionary<int, string> Syntax = SyntaxNames();
        private static Dictionary<int, string> SyntaxNames()
        {
            var names = new Dictionary<int, string>();
            foreach (var symbol in C.Definition.SymbolNames) names[symbol.ID] = symbol.Name;
            return names;
        }
        private int _head;
        public long Position { get; private set; }
        public Item Current { get; private set; } = Item.EOF;
        public bool SupportsResetting => inner.SupportsResetting;
        public bool MoveNext()
        {
            if (_head < _buffer.Count)
            {
                Current = _buffer[_head++];
                if (_head == _buffer.Count) { _buffer.Clear(); _head = 0; }
            }
            else if (inner.MoveNext()) Current = inner.Current;
            else return false;
            Position++;
            return true;
        }
        private string? At(int offset)
        {
            while (_buffer.Count <= _head + offset)
            {
                if (!inner.MoveNext()) return null;
                _buffer.Add(inner.Current);
            }
            var token = _buffer[_head + offset];
            return token == Item.EOF ? null : Syntax.GetValueOrDefault(token.ID, string.Empty);
        }
        private int Balanced(int start)
        {
            var close = At(start) switch { "(" => ")", "[" => "]", "{" => "}", _ => null };
            if (close is null) return start;
            var index = start + 1;
            while (At(index) is { } token && token != close)
                index = token is "(" or "[" or "{" ? Balanced(index) : index + 1;
            return At(index) is null ? index : index + 1;
        }
        private int Statement(int index)
        {
            var token = At(index);
            if (token is null) return index;
            if (token == "{") return Balanced(index);
            if (token is "for" or "while" or "switch" or "if")
            {
                var end = Statement(Balanced(index + 1));
                return token == "if" && At(end) == "else" ? Statement(end + 1) : end;
            }
            if (token == "do") return Balanced(Statement(index + 1) + 1) + 1;
            if (At(index + 1) == ":") return Statement(index + 2);
            while (At(index) is { } next && next != ";")
                index = next is "(" or "[" or "{" ? Balanced(index) : index + 1;
            return At(index) is null ? index : index + 1;
        }
        // The current 'for' has already been consumed; next is its header.
        public long StatementEnd() => Position + Statement(Balanced(0));
        public void Reset() { inner.Reset(); _buffer.Clear(); _head = 0; Position = 0; Current = Item.EOF; }
        public void Dispose() => inner.Dispose();
    }
}
