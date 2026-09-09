#nullable enable

using System;
using System.Collections.Generic;
using LALR.CC;
using LALR.CC.LexicalGrammar;

namespace DotCC;

/// <summary>
/// Folds <c>sizeof(T)</c> to its numeric value in the token stream so that
/// <c>sizeof(int) * CHAR_BIT</c> becomes <c>4 * 8</c> which the LALR parser
/// handles without conflict. Without this, the grammar's subscript production
/// (<c>E[E]</c>) creates a conflict that drops binary operators after
/// <c>sizeof</c>, causing e.g. <c>MAXABITS+1=32</c> to become just <c>4</c>.
///
/// Only single primitive keywords fold here. Complete type-specifier runs,
/// typedefs and aggregates stay as <c>sizeof(T)</c> for typed resolution,
/// preserving target layout and lexical scope. The conflict remains only against
/// the operators that double as unary-prefix operators: <c>sizeof(S) * x</c>,
/// <c>sizeof(S) - x</c>, <c>sizeof(S) &amp; x</c> get parsed as a cast inside
/// the operand (<c>sizeof((S)*x)</c> = the deref's type, dropping <c>* x</c>).
/// For those, the folder wraps the unfoldable sizeof in parens
/// (<c>(sizeof(S)) * x</c>) so it's a complete primary and the operator binds
/// as binary — the struct analogue of the NUM fold. (Other operators —
/// <c>| &lt;&lt;</c> … — have no unary form, so they never absorb and need no
/// wrap; and a sizeof NOT followed by one of these — e.g.
/// <c>malloc(sizeof(S))</c> — is left bare so the malloc-sizeof peephole still
/// sees a sole <c>SizeofType</c> marker.)
/// </summary>
internal sealed class SizeofFolder : RewritingTokenStream
{
    /// <summary>Type qualifiers that may appear in a type-specifier operand of
    /// <c>sizeof</c> (<c>sizeof(const char *)</c>) without making it an expression.</summary>
    private static readonly HashSet<string> TypeQualifiers = new(StringComparer.Ordinal)
    { "const", "volatile", "restrict", "_Atomic" };

    private readonly int _sizeofSym, _lparenSym, _rparenSym, _starSym, _numSym;
    private readonly int _minusSym, _ampSym, _plusSym;

    public SizeofFolder(ISyncIterator<Item> inner) : base(inner)
    {
        var map = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var sym in C.Definition.SymbolNames)
            map[sym.Name] = sym.ID;
        _sizeofSym = map["sizeof"];
        _lparenSym = map["("];
        _rparenSym = map[")"];
        _starSym = map["*"];
        _minusSym = map["-"];
        _ampSym = map["&"];
        _plusSym = map["+"];
        _numSym = map["NUM"];
    }

    protected override void ProcessToken(Item token)
    {
        if (token.ID != _sizeofSym) { Emit(token); return; }

        // Peek sizeof ( Type )
        if (!TryReadNext(out var lp) || lp!.ID != _lparenSym) { Emit(token); if (lp != null) Emit(lp); return; }

        // Collect type tokens with paren-depth tracking to find the matching )
        var typeTokens = new List<Item>();
        var depth = 1;
        while (depth > 0 && TryReadNext(out var t))
        {
            if (t!.ID == _lparenSym) depth++;
            else if (t.ID == _rparenSym) { depth--; if (depth == 0) break; }
            typeTokens.Add(t);
        }

        if (depth != 0) { Emit(token); Emit(lp); foreach (var x in typeTokens) Emit(x); return; }

        var size = EvalSizeof(typeTokens);
        if (size is int n)
        {
            // Replace entire sizeof(type) sequence with NUM
            Emit(new Item(_numSym, n.ToString(
                System.Globalization.CultureInfo.InvariantCulture), token.Position));
            return;
        }

        // Leave complete type resolution to the binder — re-emit the
        // `sizeof ( Type )` tokens. Peek the token that follows: if it's an
        // operator that ALSO has a unary-prefix form (`*` deref, `-` negate, `&`
        // address-of, `+` unary plus), the parser would absorb it into a cast
        // inside the operand (`sizeof(S) * x` → `sizeof((S)*x)`, silently
        // dropping `* x`). Wrap the sizeof in parens so it's a complete primary
        // and the operator binds as a binary operator. A bare sizeof (anything
        // else following — `)`, `;`, `|`, …) is left untouched so the
        // malloc-sizeof peephole still sees it.
        var hasNext = TryReadNext(out var nextTok);
        var wrap = hasNext && (nextTok!.ID == _starSym || nextTok.ID == _minusSym
            || nextTok.ID == _ampSym || nextTok.ID == _plusSym);
        if (wrap) { Emit(new Item(_lparenSym, "(", token.Position)); }
        Emit(token); Emit(lp);
        foreach (var x in typeTokens) Emit(x);
        Emit(new Item(_rparenSym, ")", token.Position));
        if (wrap) { Emit(new Item(_rparenSym, ")", token.Position)); }
        // Re-emit the peeked token (the operator, or whatever followed). It can't
        // be a `sizeof`/`typedef` needing ProcessToken — neither is valid right
        // after a `sizeof(Type)` without an intervening operator — so emitting it
        // directly is safe.
        if (hasNext) { Emit(nextTok!); }
    }

    private static int? EvalSizeof(List<Item> tokens)
    {
        // Fold only one unambiguous primitive keyword. Multi-keyword types,
        // typedefs, pointers and aggregates belong to the typed binder, which
        // knows their full declaration, target representation and block scope.
        // A token-level typedef-size cache loses shadowing and a backward scan
        // mistakes `long long int` for int (SQLite's Bitmask became 32 bits).
        var significant = tokens.Where(t => !TypeQualifiers.Contains(t.Content?.ToString() ?? "")).ToList();
        if (significant.Count != 1) return null;
        return significant[0].Content?.ToString() switch
        {
            "void" or "char" or "_Bool" => 1,
            "short" => 2,
            "int" or "signed" or "unsigned" or "float" => 4,
            "long" or "double" => 8,
            _ => null,
        };
    }
}
