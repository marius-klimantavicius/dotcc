using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LALR.CC;
using LALR.CC.LexicalGrammar;

namespace DotCC;

internal sealed partial class CPreprocessor
{
    private static readonly HashSet<int> CharacterSymbols = C.Definition.SymbolNames
        .Where(symbol => symbol.Name is "CHAR" or "WCHAR" or "U16CHAR" or "U32CHAR" or "U8CHAR")
        .Select(symbol => symbol.ID).ToHashSet();

    /// <summary>The generic directive stack owns branch suppression; C owns
    /// intmax_t/uintmax_t expression semantics. A lazy expression token reaches
    /// the rewrite hook only when its #if/#elif branch needs evaluation.</summary>
    internal PreprocessorTokenStream WrapStream(ISyncIterator<Item> inner)
    {
        var conditionals = C.BuildConditionals(this);
        return new PreprocessorTokenStream(
            new ConditionalExpressionTokens(inner, conditionals.IfSymbol, conditionals.ElifSymbol, _numSymbolId),
            C.BuildPreprocessor(this), RewriteConditionalToken, conditionals,
            (name, args) => NormalizeConditionalCharacters(ExpandFuncMacro(name, args)));
    }

    private IEnumerable<Item> RewriteConditionalToken(Item token)
    {
        if (token is ConditionalExpressionItem expression)
        {
            try
            {
                return new[] { SourceMappedItem.Create(_numSymbolId,
                    EvaluateConditionalExpression(expression.Tokens) ? "1" : "0", token) };
            }
            catch (CompileException error)
            {
                var origin = expression.Tokens.Count > 0 ? expression.Tokens[0] : token;
                var position = SourceMappedItem.Physical(origin);
                var file = SourceMappedItem.FileOf(origin)?.Name ?? _currentlyIncluding;
                throw new CompileException($"{error.Message} ({file}:{position.Line}:{position.Column})", error);
            }
        }
        return Rewrite(token);
    }

    private IEnumerable<Item> NormalizeConditionalCharacters(IEnumerable<Item> tokens)
    {
        foreach (var token in tokens)
        {
            // An evaluated #if expression also requires C tokens. Nested
            // conditionals in an inactive group are discarded before this hook.
            CTokenValidator.Validate(token);
            if (token.Content?.ToString() == RuntimeIntrinsicNames.IsLittleEndian)
                throw new CompileException("runtime intrinsic __dotcc_is_little_endian is not allowed in #if/#elif expressions");
            if (CharacterSymbols.Contains(token.ID) && token.Content is string text)
            {
                var quote = text.IndexOf('\'');
                var value = CCharacterLiteral.Decode(text[(quote + 1)..^1]);
                yield return SourceMappedItem.Create(_numSymbolId,
                    value.ToString(CultureInfo.InvariantCulture), token);
            }
            else yield return token;
        }
    }

    private sealed class ConditionalExpressionItem(Item token, IReadOnlyList<Item> tokens) : SourceMappedItem(token)
    {
        internal IReadOnlyList<Item> Tokens { get; } = tokens;
    }

    /// <summary>Keep expression membership on the token, since the directive
    /// reader looks ahead into the next line before invoking macro callbacks.
    /// Each included file has its own iterator and logical line coordinates.</summary>
    private sealed class ConditionalExpressionTokens(ISyncIterator<Item> inner, int ifSymbol, int elifSymbol, int numberSymbol)
        : RewritingTokenStream(inner)
    {
        private int _previousLine = -1;

        protected override void ProcessToken(Item token)
        {
            bool firstOnLine = token.Position.Line != _previousLine;
            _previousLine = token.Position.Line;
            if (firstOnLine && token.Content?.ToString() == "#")
            {
                // A lone # (including trailing whitespace/comments) is C's
                // null directive. Preserve nonempty unknown directives so they
                // still diagnose; # and ## inside #define stay untouched.
                var rest = CollectUntil(next => next.Position.Line != token.Position.Line && next.Position.Line != 0);
                if (rest.Count > 0) { Emit(token); EmitRange(rest); }
                return;
            }
            if (token.ID == ifSymbol || token.ID == elifSymbol)
            {
                Emit(token);
                var expression = CollectUntil(next => next.Position.Line != token.Position.Line && next.Position.Line != 0);
                Emit(new ConditionalExpressionItem(SourceMappedItem.Create(numberSymbol, "0", token), expression));
            }
            else Emit(token);
        }

        public override void Reset() { _previousLine = -1; base.Reset(); }
    }
}
