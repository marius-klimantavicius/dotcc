using LALR.CC;
using LALR.CC.LexicalGrammar;
using Xunit;

namespace DotCC.Tests;

public sealed class ZigTranslationGrammarTests
{
    [Theory]
    [InlineData("pub fn main() u8 { return @intCast(add(20, 22,),); }")]
    [InlineData("pub fn main() bool { return false or switch (x) { 1 => true, else => false, }; }")]
    [InlineData("const optional_export = if (enabled) implementation else {};")]
    public void Generated_and_rebuilt_tables_accept_upstream_expression_shapes(string source)
    {
        AssertParses(Zig.BuildParser(Zig.IdentityVisitor.Instance), source);
        AssertParses(new Parser(Zig.Definition), source);
    }

    private static void AssertParses(Parser parser, string source)
    {
        using var lexer = BytesLexer.FromString(source, Zig.BuildLexer());
        using var tokens = new SyncLATokenIterator(lexer);
        var root = parser.ParseInput(tokens, debugger: null,
            errorMode: ParserErrorMode.Return, trimReductions: true);
        Assert.False(root.IsError, $"Failed to parse: {root}");
    }
}
