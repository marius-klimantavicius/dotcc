using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LALR.CC;
using LALR.CC.LexicalGrammar;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class LexerGrammarTests
{
    private static (string[] Tokens, string? Error) Read(ISyncIterator<Item> lexer)
    {
        using (lexer)
        {
            var tokens = new List<string>();
            string? error = null;
            try
            {
                while (lexer.MoveNext())
                {
                    var token = lexer.Current;
                    tokens.Add($"{token.ID}:{token.Position}:{token.Position.ByteOffset}:{token.Content}");
                }
            }
            catch (LexerException ex) { error = ex.Message; }
            lexer.MoveNext().ShouldBeFalse();
            return (tokens.ToArray(), error);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(37)]
    public void C_tokens_and_errors_match_reference_lexer(int line)
    {
        string[] inputs = [
            "", " \t\r\n", "int main(void) { return 0xFFu + .5e-2; }",
            "#define JOIN(a,b) a ## b\n#if defined(JOIN) && 2 < 3\n#endif\n",
            "foo >>= 2; bar <<= 1; a+++b; x->y != sizeof(x); ...",
            "/* λ 😀 **/\r\nconst char *s = \"a\\n世界\"; char c = '\\x41';",
            "// comment\n/* multiline\ncomment */\nint x;",
            "int x; @", "/* λ */\nint x; \\", "\"unterminated", "\"abc\"\u0000",
            "# 42 \"test.h\"\n__attribute__((packed)) struct S { int a : 3; };"
        ];
        foreach (var input in inputs)
        {
            var expected = Read(BytesLexer.FromString(input, C.BuildLexer(), initialLine: line));
            var actual = Read(LexerGrammar.C.FromString(input, initialLine: line));
            actual.Tokens.ShouldBe(expected.Tokens, input);
            actual.Error.ShouldBe(expected.Error, input);
        }
    }

    [Theory]
    [InlineData("const x: u32 = 42; pub fn main() void { _ = x; }")]
    [InlineData("// λ 😀\nconst s = \"世界\"; const t = @\"type\"; x.* += 0xFF;")]
    [InlineData("const x = 1e3; const s = \\\\line\n;")]
    [InlineData("const x = $;")]
    public void Zig_tokens_and_errors_match_reference_lexer(string input)
    {
        var expected = Read(BytesLexer.FromString(input, Zig.BuildLexer()));
        var actual = Read(LexerGrammar.Zig.FromString(input));
        actual.Tokens.ShouldBe(expected.Tokens);
        actual.Error.ShouldBe(expected.Error);
    }

    [Fact]
    public void Shared_tables_keep_concurrent_stream_positions_independent()
    {
        const string input = "/* λ */\nint a = 42;\n";
        var expected = Read(BytesLexer.FromString(input, C.BuildLexer()));
        Parallel.For(0, 32, _ =>
        {
            using var outer = LexerGrammar.C.FromString(input);
            outer.MoveNext().ShouldBeTrue();
            Read(LexerGrammar.C.FromString(input)).Tokens.ShouldBe(expected.Tokens);
            var position = outer.Current.Position;
            Read(LexerGrammar.C.FromString("\n\n/* nested include */ int b;", initialLine: 100));
            outer.Current.Position.ShouldBe(position);
            outer.MoveNext().ShouldBeTrue();
            outer.Current.Content.ShouldBe("a");
        });
    }

    [Fact]
    public void State_push_pop_and_rule_priority_match_reference_lexer()
    {
        var rules = new Dictionary<string, LexRule[]>
        {
            ["root"] = [new(1, new CharRx('x'), "body"), new(2, new CharRx('x'), null),
                        new(3, new CharRx(' '), "#ignore")],
            ["body"] = [new(4, new CharRx('y'), null), new(5, new CharRx('z'), "#pop")]
        };
        var grammar = new LexerGrammar(rules);
        foreach (var input in new[] { "xyz xyz", "xyyzx", "xy?" })
        {
            var expected = Read(BytesLexer.FromString(input, rules));
            var actual = Read(grammar.FromString(input));
            actual.Tokens.ShouldBe(expected.Tokens);
            actual.Error.ShouldBe(expected.Error);
        }
    }
}
