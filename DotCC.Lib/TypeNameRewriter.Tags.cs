using System.Collections.Generic;
using LALR.CC.LexicalGrammar;

namespace DotCC;

internal sealed partial class TypeNameRewriter
{
    private sealed class TagHeadContext
    {
        public bool AfterKeyword;
        public bool AggregateHead;
        public int ParenthesisDepth;
        public bool AttributeOpening;
        public Stack<int> AttributeDepths { get; } = new();

        public void Reset()
        {
            AfterKeyword = AggregateHead = AttributeOpening = false;
            ParenthesisDepth = 0;
            AttributeDepths.Clear();
        }
    }

    // A tag remains in the tag namespace across GNU attributes between its
    // keyword and name. Suspend that expectation for each balanced attribute,
    // so types (including other tags) inside its arguments still classify
    // normally. Share this state machine with the buffered typedef path.
    private bool AdvanceTagHead(Item token, TagHeadContext head)
    {
        var afterTag = head.AfterKeyword;
        if (head.AttributeOpening)
        {
            head.AttributeOpening = false;
            if (token.ID == _openParenSymbol)
                head.AttributeDepths.Push(head.ParenthesisDepth);
        }
        if (token.ID == _openParenSymbol) head.ParenthesisDepth++;
        else if (token.ID == _closeParenSymbol)
        {
            head.ParenthesisDepth--;
            if (head.AttributeDepths.TryPeek(out var depth) && depth == head.ParenthesisDepth)
            {
                head.AttributeDepths.Pop();
                head.AfterKeyword = head.AggregateHead = true;
                return false;
            }
        }

        head.AfterKeyword = token.ID == _structSymbol || token.ID == _unionSymbol || token.ID == _enumSymbol;
        head.AggregateHead = head.AfterKeyword || (afterTag && token.ID == _idSymbol);
        head.AttributeOpening = afterTag && token.ID == _attributeSymbol;
        return afterTag;
    }
}
