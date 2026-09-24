#nullable enable

using System;
using System.Collections.Generic;
using LALR.CC;
using LALR.CC.LexicalGrammar;

namespace DotCC;

/// <summary>
/// The C "lexer hack" as a <see cref="RewritingTokenStream"/>. Sits between
/// the preprocessor and the parser's <c>SyncLATokenIterator</c>:
/// <list type="bullet">
///   <item>When the upstream emits a <c>typedef</c> directive, the rewriter
///     consumes the body up to the matching top-level <c>;</c>, identifies the
///     new alias name (the last <c>ID</c> token at brace-depth zero), and
///     adds it to the set of known type names. The body tokens themselves
///     are still forwarded so the parser reduces the <c>typedefAlias</c> or
///     <c>typedefStruct</c> action and the visitor gets to emit the C# alias
///     (or struct decl).</item>
///   <item>For any subsequent <c>ID</c> whose content matches a known type
///     name, the rewriter swaps the token's symbol id for <c>TYPE_NAME</c>
///     (content unchanged). The parser then unambiguously reduces
///     <c>Type -> TYPE_NAME</c> and never confuses <c>Color * x;</c>
///     (declaration when <c>Color</c> is a typedef) with a multiplication
///     expression.</item>
/// </list>
/// </summary>
/// <remarks>
/// Typedef names are visible through their containing braced scope. Names
/// introduced locally are removed when that scope closes, while shadowing an
/// existing typedef keeps its lexical classification (the binder restores its
/// underlying type). Ordinary-variable shadowing of a visible typedef remains
/// a separate declaration-context problem.
/// <para>
/// All the iterator plumbing (ready queue, look-ahead buffer, exhaustion
/// flag) lives in <see cref="RewritingTokenStream"/>; this class is pure
/// policy. The pattern shape — track state, optionally consume neighbouring
/// tokens, then emit (possibly transformed) tokens — is the same one
/// <see cref="PreprocessorTokenStream"/> uses for macro expansion.
/// </para>
/// </remarks>
internal sealed class TypeNameRewriter : RewritingTokenStream
{
    private readonly int _idSymbol;
    private readonly int _typeNameSymbol;
    private readonly int _typedefSymbol;
    private readonly int _semiSymbol;
    private readonly int _openBraceSymbol;
    private readonly int _closeBraceSymbol;
    private readonly int _openParenSymbol;
    private readonly int _closeParenSymbol;
    private readonly int _starSymbol;
    private readonly int _openBracketSymbol;
    private readonly int _closeBracketSymbol;
    private readonly int _structSymbol;
    private readonly int _unionSymbol;
    private readonly int _enumSymbol;
    private readonly int _dotSymbol;
    private readonly int _arrowSymbol;
    private readonly HashSet<int> _pointerQualifiers;
    private readonly HashSet<int> _typeSpecifiers;
    private readonly HashSet<int> _parenthesizedTypeOperators;
    private readonly HashSet<string> _typeNames;
    private readonly HashSet<string> _seedTypeNames;
    private readonly Stack<HashSet<string>> _scopeTypeNames = new();
    private readonly Stack<HashSet<string>> _scopeShadowedTypeNames = new();
    private readonly Stack<bool> _aggregateScopes = new();
    // Parameter names may hide typedefs in the parameter list and function
    // body, but must not leak out of a prototype or function definition.
    private sealed class ParenthesisScope(bool declaratorGroup, int blockDepth, bool completesType)
    {
        public bool DeclaratorGroup { get; } = declaratorGroup;
        public int BlockDepth { get; } = blockDepth;
        public bool CompletesType { get; } = completesType;
        public HashSet<string> Shadows { get; } = new(StringComparer.Ordinal);
    }
    private readonly Stack<ParenthesisScope> _parenthesisScopes = new();
    private HashSet<string>? _pendingParameterShadows;

    // True when the previous token forwarded through ProcessToken was a
    // `struct` / `union` / `enum` keyword — so the NEXT ID is a tag, not a
    // typedef-name, and must not be promoted to TYPE_NAME. C keeps tags in a
    // separate namespace, so a tag may legally collide with a typedef-name (the
    // `typedef struct lua_State lua_State;` idiom). Reset across the typedef
    // path (its body is consumed wholesale, ending at `;`).
    private bool _afterTagKeyword;
    private int _previousSymbol = -1;
    private bool _afterDeclarationType;
    private bool _aggregateHead;

    /// <summary>
    /// Construct a rewriter over <paramref name="inner"/>, optionally
    /// pre-populated with <paramref name="seedTypeNames"/> — identifiers
    /// the user code can reference as types without first seeing a
    /// <c>typedef</c> definition. Used to expose C#-side libc types
    /// (e.g. <c>LongJmpToken</c> for <c>&lt;setjmp.h&gt;</c>) so a
    /// synthetic header can write
    /// <c>typedef LongJmpToken jmp_buf;</c> and have it parse —
    /// without introducing non-C keywords into the grammar.
    /// </summary>
    public TypeNameRewriter(
        ISyncIterator<Item> inner,
        IEnumerable<string>? seedTypeNames = null) : base(inner)
    {
        // Resolve symbol ids by name from the generated grammar's definition.
        // Done once at construction so per-token dispatch stays O(1).
        var map = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var sym in C.Definition.SymbolNames)
        {
            map[sym.Name] = sym.ID;
        }
        _idSymbol = map["ID"];
        _typeNameSymbol = map["TYPE_NAME"];
        _typedefSymbol = map["typedef"];
        _semiSymbol = map[";"];
        _openBraceSymbol = map["{"];
        _closeBraceSymbol = map["}"];
        _openParenSymbol = map["("];
        _closeParenSymbol = map[")"];
        _starSymbol = map["*"];
        _openBracketSymbol = map["["];
        _closeBracketSymbol = map["]"];
        _structSymbol = map["struct"];
        _unionSymbol = map["union"];
        _enumSymbol = map["enum"];
        _dotSymbol = map["."];
        _arrowSymbol = map["->"];
        _pointerQualifiers = new HashSet<int> { _starSymbol, map["const"], map["volatile"], map["restrict"] };
        _parenthesizedTypeOperators = new HashSet<int> { map["_Atomic"], map["typeof"] };
        _typeSpecifiers = new HashSet<int>();
        foreach (var keyword in new[] { "void", "char", "short", "int", "long", "signed", "unsigned",
                     "float", "double", "_Bool", "_Complex", "_Float128", "__int128" })
            _typeSpecifiers.Add(map[keyword]);

        // Seed set lives separately from the dynamic set populated by
        // user `typedef`s, so Reset() can clear the dynamic side without
        // forgetting the predefined libc-class names.
        _seedTypeNames = new HashSet<string>(StringComparer.Ordinal);
        if (seedTypeNames is not null)
        {
            foreach (var name in seedTypeNames) { _seedTypeNames.Add(name); }
        }
        _typeNames = new HashSet<string>(_seedTypeNames, StringComparer.Ordinal);
    }

    protected override void ProcessToken(Item token)
    {
        var closesAggregate = token.ID == _closeBraceSymbol && _aggregateScopes.Count > 0 && _aggregateScopes.Peek();
        if (token.ID == _openBraceSymbol)
        {
            _scopeTypeNames.Push(new HashSet<string>(StringComparer.Ordinal));
            _scopeShadowedTypeNames.Push(new HashSet<string>(StringComparer.Ordinal));
            _aggregateScopes.Push(_aggregateHead);
            if (_pendingParameterShadows is { } parameters)
            {
                foreach (var parameter in parameters)
                {
                    _typeNames.Remove(parameter);
                    _scopeShadowedTypeNames.Peek().Add(parameter);
                }
            }
        }
        else if (token.ID == _closeBraceSymbol && _scopeTypeNames.Count > 0)
        {
            foreach (var local in _scopeTypeNames.Pop()) _typeNames.Remove(local);
            foreach (var shadowed in _scopeShadowedTypeNames.Pop()) _typeNames.Add(shadowed);
            _aggregateScopes.Pop();
        }

        _pendingParameterShadows = null;
        var closesParenthesizedType = false;
        if (token.ID == _openParenSymbol)
        {
            var completesType = _parenthesizedTypeOperators.Contains(_previousSymbol);
            _parenthesisScopes.Push(new ParenthesisScope(_afterDeclarationType || completesType, _aggregateScopes.Count, completesType));
        }
        else if (token.ID == _closeParenSymbol && _parenthesisScopes.Count > 0)
        {
            var scope = _parenthesisScopes.Pop();
            closesParenthesizedType = scope.CompletesType;
            _pendingParameterShadows = scope.Shadows;
            foreach (var parameter in _pendingParameterShadows) _typeNames.Add(parameter);
        }

        // Did a struct/union/enum keyword just go by? Snapshot it, then update
        // the flag for the next token from THIS token's identity.
        var afterTag = _afterTagKeyword;
        _afterTagKeyword = token.ID == _structSymbol
            || token.ID == _unionSymbol
            || token.ID == _enumSymbol;
        _aggregateHead = _afterTagKeyword || (afterTag && token.ID == _idSymbol);

        // Promote a known typedef-name ID to TYPE_NAME so the parser routes it
        // through `Type -> TYPE_NAME`. The original ID's content (the
        // identifier string) and source position carry over. EXCEPT right after
        // a `struct`/`union`/`enum` keyword: there the ID is a TAG (a distinct C
        // namespace that may collide with a typedef-name), so leave it as ID —
        // otherwise `struct lua_State { … }` / `struct lua_State *p` would feed a
        // TYPE_NAME where the grammar wants a plain ID. A member after . or ->
        // likewise belongs to a distinct namespace. Once a complete type has
        // appeared, a matching name is a declarator: `void *list`, `struct list
        // *list`, and `list *list` all name a value that may hide the typedef.
        if (token.ID == _idSymbol
            && token.Content is string name
            && !afterTag
            && !_afterDeclarationType
            && _previousSymbol != _dotSymbol
            && _previousSymbol != _arrowSymbol
            && _typeNames.Contains(name))
        {
            Emit(SourceFileOrigin.Rewrite(token, _typeNameSymbol, name));
            _previousSymbol = _typeNameSymbol;
            _afterDeclarationType = true;
            return;
        }

        // A block-local declaration can hide a typedef until that block exits.
        // Aggregate fields have their own namespace and must not hide the type.
        if (_afterDeclarationType && !afterTag && token.ID == _idSymbol && token.Content is string localName
            && _typeNames.Contains(localName) && DeclarationShadowScope() is { } shadows)
        {
            _typeNames.Remove(localName);
            shadows.Add(localName);
        }
        _afterDeclarationType = closesAggregate || closesParenthesizedType
            || IsAfterDeclarationType(token, afterTag, _afterDeclarationType);

        if (token.ID == _typedefSymbol)
        {
            HandleTypedef(token);
            return;
        }

        Emit(token);
        _previousSymbol = token.ID;
    }

    // Parentheses in `void (*name)(...)` group a declarator; they do not
    // introduce parameter scope. Bind a shadowing name to the enclosing actual
    // parameter list or local block. Aggregate members keep their own namespace.
    private HashSet<string>? DeclarationShadowScope()
    {
        foreach (var scope in _parenthesisScopes)
            if (!scope.DeclaratorGroup && scope.BlockDepth == _aggregateScopes.Count)
                return scope.Shadows;
        return _aggregateScopes.Count > 0 && !_aggregateScopes.Peek()
            ? _scopeShadowedTypeNames.Peek() : null;
    }

    private bool IsAfterDeclarationType(Item token, bool afterTag, bool previouslyAfterType) =>
        _typeSpecifiers.Contains(token.ID) || (afterTag && token.ID == _idSymbol)
        || (previouslyAfterType && (_pointerQualifiers.Contains(token.ID) || token.ID == _openParenSymbol));

    /// <summary>
    /// On <c>typedef</c>: drain tokens up to (and including) the matching
    /// top-level <c>;</c>, capture the alias name, register it, and emit the
    /// whole sequence so the parser still reduces the typedef production.
    /// Brace-tracking ensures <c>typedef struct Foo { … } Foo;</c> stops at
    /// the trailing semicolon, not the one between the members.
    /// </summary>
    private void HandleTypedef(Item typedefToken)
    {
        var body = new List<Item>();
        var depth = 0;
        while (TryReadNext(out var next))
        {
            body.Add(next);
            if (next.ID == _openBraceSymbol)
            {
                depth++;
            }
            else if (next.ID == _closeBraceSymbol)
            {
                depth--;
            }
            else if (next.ID == _semiSymbol && depth == 0)
            {
                break;
            }
        }

        // Identify the alias position: the `( * ID )` group of a fn-ptr
        // typedef, else the last ID token at brace/paren-depth zero before
        // the terminating semicolon. ONE discovery for both registration and
        // emission — they used to disagree: registration knew the fn-ptr
        // pattern but emission took "last ID in the body", which for
        // `typedef sexp (*sexp_proc1)(sexp, sexp, sexp_sint_t);` is the
        // PARAM `sexp_sint_t`, wrongly exempting it from TYPE_NAME promotion
        // (chibi-scheme's procedure typedefs; Lua's fn-ptr typedef params are
        // keyword types, which is why it never surfaced there).
        var aliasIndex = FindAliasIndex(body);
        var aliasName = aliasIndex >= 0 ? body[aliasIndex].Content as string : null;
        if (aliasName is not null)
        {
            if (_typeNames.Add(aliasName) && _scopeTypeNames.Count > 0)
                _scopeTypeNames.Peek().Add(aliasName);
        }

        // Emit the typedef token + body so the parser sees the full sequence.
        // Body IDs that already match a known typedef-name get promoted to
        // TYPE_NAME (covers `typedef Color Color2;` chaining); the alias
        // position itself stays as ID because we've identified it from the
        // raw token list, not from the promoted stream.
        Emit(typedefToken);
        _previousSymbol = typedefToken.ID;
        var afterDeclarationType = false;
        var parenthesizedTypes = new Stack<bool>();
        var aggregateScopes = new Stack<bool>();
        var aggregateHead = false;
        for (var i = 0; i < body.Count; i++)
        {
            var t = body[i];
            var closesAggregate = false;
            if (t.ID == _openBraceSymbol) aggregateScopes.Push(aggregateHead);
            else if (t.ID == _closeBraceSymbol && aggregateScopes.Count > 0)
                closesAggregate = aggregateScopes.Pop();
            var closesParenthesizedType = false;
            if (t.ID == _openParenSymbol)
                parenthesizedTypes.Push(_parenthesizedTypeOperators.Contains(_previousSymbol));
            else if (t.ID == _closeParenSymbol && parenthesizedTypes.Count > 0)
                closesParenthesizedType = parenthesizedTypes.Pop();
            // Same tag rule as ProcessToken: an ID right after struct/union/enum
            // is a tag, never a typedef-name — don't promote it (covers e.g.
            // `typedef struct PriorAlias NewName;`).
            var afterTag = i > 0 && (body[i - 1].ID == _structSymbol
                || body[i - 1].ID == _unionSymbol
                || body[i - 1].ID == _enumSymbol);
            aggregateHead = t.ID == _structSymbol || t.ID == _unionSymbol || t.ID == _enumSymbol
                || (afterTag && t.ID == _idSymbol);
            if (i != aliasIndex
                && t.ID == _idSymbol
                && !afterTag
                && !afterDeclarationType
                && _previousSymbol != _dotSymbol
                && _previousSymbol != _arrowSymbol
                && t.Content is string s
                && _typeNames.Contains(s)
                && s != aliasName)
            {
                Emit(SourceFileOrigin.Rewrite(t, _typeNameSymbol, s));
                _previousSymbol = _typeNameSymbol;
                afterDeclarationType = true;
            }
            else
            {
                Emit(t);
                _previousSymbol = t.ID;
                afterDeclarationType = closesAggregate || closesParenthesizedType
                    || IsAfterDeclarationType(t, afterTag, afterDeclarationType);
            }
        }

        // The typedef body was consumed wholesale (it ended at `;`), so the next
        // ProcessToken token is not after a tag keyword.
        _afterTagKeyword = false;
        _afterDeclarationType = false;
        _aggregateHead = false;
    }

    private int FindAliasIndex(List<Item> body)
    {
        // Function-pointer typedef has the alias INSIDE the first parenthesized
        // group: `typedef Ret (*Name)(args);`. The body has `( * ID )` at
        // brace/paren depth 0 — scan for that pattern first.
        var aggregateDepth = 0;
        var parenDepth = 0;
        for (var i = 0; i + 3 < body.Count; i++)
        {
            if (body[i].ID == _openBraceSymbol) { aggregateDepth++; }
            else if (body[i].ID == _closeBraceSymbol) { aggregateDepth--; }
            // Parentheses grouping a function typedef name: Ret (Name)(args).
            if (aggregateDepth == 0 && parenDepth == 0 && i + 3 < body.Count
                && body[i].ID == _openParenSymbol && body[i + 1].ID == _idSymbol
                && body[i + 2].ID == _closeParenSymbol && body[i + 3].ID == _openParenSymbol)
                return i + 1;
            // A callback field inside a typedef aggregate is a member, not the
            // enclosing typedef's alias. Only inspect file-level declarators.
            if (aggregateDepth == 0 && parenDepth == 0 && body[i].ID == _openParenSymbol
                && body[i + 1].ID == _starSymbol
                && body[i + 2].ID == _idSymbol
                && body[i + 3].ID == _closeParenSymbol)
            {
                return i + 2;
            }
            if (body[i].ID == _openParenSymbol) parenDepth++;
            else if (body[i].ID == _closeParenSymbol) parenDepth--;
        }

        // Otherwise (simple alias / struct-with-tag / struct-with-body):
        // reverse-walk for the last top-level ID. Skip anything inside braces,
        // parens, OR brackets — parens guard the param list of a fnptr typedef
        // (handled above); brackets guard an array-typedef bound (`typedef
        // char buf[MAX];` must bind `buf`, not the bound identifier `MAX`).
        var depth = 0;
        for (var i = body.Count - 1; i >= 0; i--)
        {
            var t = body[i];
            if (t.ID == _closeBraceSymbol || t.ID == _closeParenSymbol || t.ID == _closeBracketSymbol) { depth++; continue; }
            if (t.ID == _openBraceSymbol  || t.ID == _openParenSymbol  || t.ID == _openBracketSymbol)  { depth--; continue; }
            if (depth != 0) { continue; }
            if (t.ID == _idSymbol && t.Content is string)
            {
                return i;
            }
        }
        return -1;
    }

    public override void Reset()
    {
        // Restore to the seed-only state so predefined libc-class type
        // names survive across reuse but per-TU typedef'd aliases don't.
        _typeNames.Clear();
        _scopeTypeNames.Clear();
        _scopeShadowedTypeNames.Clear();
        _aggregateScopes.Clear();
        _parenthesisScopes.Clear();
        _pendingParameterShadows = null;
        foreach (var name in _seedTypeNames) { _typeNames.Add(name); }
        _afterTagKeyword = false;
        _previousSymbol = -1;
        _afterDeclarationType = false;
        _aggregateHead = false;
        base.Reset();
    }
}
