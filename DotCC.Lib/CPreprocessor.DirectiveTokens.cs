using System.Collections.Generic;
using LALR.CC;
using LALR.CC.LexicalGrammar;

namespace DotCC;

internal sealed partial class CPreprocessor
{
    // Use the normal prescan/substitution/rescan machinery for directive
    // expressions too, preserving stringification and macro recursion hidesets.
    private IReadOnlyList<Item> ExpandDirectiveTokens(IReadOnlyList<Item> tokens)
    {
        using var input = new DirectiveTokens(tokens);
        using var rewriting = new DirectiveMacroTokens(input, this);
        using var expanding = new MacroExpander(rewriting, this);
        var result = new List<Item>();
        while (expanding.MoveNext()) result.Add(expanding.Current);
        return result;
    }

    private sealed class DirectiveTokens(IReadOnlyList<Item> tokens) : ISyncIterator<Item>
    {
        private int _index = -1;
        public Item Current => tokens[_index];
        public bool SupportsResetting => true;
        public bool MoveNext() => ++_index < tokens.Count;
        public void Reset() => _index = -1;
        public void Dispose() { }
    }

    private sealed class DirectiveMacroTokens(ISyncIterator<Item> input, CPreprocessor cpp) : RewritingTokenStream(input)
    {
        protected override void ProcessToken(Item token) => EmitRange(cpp.Rewrite(token));
    }
}
