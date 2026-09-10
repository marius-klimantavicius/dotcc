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

    /// <summary>The generic conditional evaluator accepts integer tokens. C
    /// character constants therefore need decoding after macro substitution,
    /// only in conditional expressions. Ordinary tokens retain their spelling
    /// and type, including arguments used by macro stringification.</summary>
    internal PreprocessorTokenStream WrapStream(ISyncIterator<Item> inner)
    {
        var conditionals = C.BuildConditionals(this);
        return new PreprocessorTokenStream(
            new ConditionalExpressionTokens(inner, conditionals.IfSymbol, conditionals.ElifSymbol),
            C.BuildPreprocessor(this), RewriteConditionalToken, conditionals,
            (name, args) => NormalizeConditionalCharacters(ExpandFuncMacro(name, args)));
    }

    private IEnumerable<Item> RewriteConditionalToken(Item token)
    {
        var replacement = Rewrite(token);
        return token is ConditionalExpressionItem ? NormalizeConditionalCharacters(replacement) : replacement;
    }

    private IEnumerable<Item> NormalizeConditionalCharacters(IEnumerable<Item> tokens)
    {
        foreach (var token in tokens)
        {
            // An evaluated #if expression also requires C tokens. Nested
            // conditionals in an inactive group are discarded before this hook.
            CTokenValidator.Validate(token);
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

    private sealed class ConditionalExpressionItem(Item token) : SourceMappedItem(token);

    /// <summary>Keep expression membership on the token, since the directive
    /// reader looks ahead into the next line before invoking macro callbacks.
    /// Each included file has its own iterator and logical line coordinates.</summary>
    private sealed class ConditionalExpressionTokens(ISyncIterator<Item> inner, int ifSymbol, int elifSymbol)
        : RewritingTokenStream(inner)
    {
        private int _conditionalLine = -1;

        protected override void ProcessToken(Item token)
        {
            if (token.ID == ifSymbol || token.ID == elifSymbol)
            {
                _conditionalLine = token.Position.Line;
                Emit(token);
            }
            else Emit(token.Position.Line == _conditionalLine ? new ConditionalExpressionItem(token) : token);
        }

        public override void Reset()
        {
            _conditionalLine = -1;
            base.Reset();
        }
    }
}
