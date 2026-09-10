using System.Linq;
using LALR.CC;
using LALR.CC.LexicalGrammar;

namespace DotCC;

/// <summary>Phase 4 may discard or stringify characters that are preprocessing
/// tokens but not C tokens. Diagnose only those left for the language parser,
/// retaining macro-invocation and included-file physical source locations.</summary>
internal sealed class CTokenValidator(ISyncIterator<Item> inner) : RewritingTokenStream(inner)
{
    private static readonly int OtherSymbol = C.Definition.SymbolNames.Single(s => s.Name == "PP_OTHER").ID;

    internal static void Validate(Item token)
    {
        if (token.ID != OtherSymbol) return;
        var position = SourceMappedItem.Physical(token);
        var file = SourceMappedItem.FileOf(token)?.Name;
        throw new CompileException($"lex failed{(file is null ? "" : " in " + file)}: "
            + $"invalid C token '{token.Content}' at line {position.Line}, column {position.Column} "
            + $"(byte offset {position.ByteOffset})");
    }

    protected override void ProcessToken(Item token)
    {
        Validate(token);
        Emit(token);
    }
}
