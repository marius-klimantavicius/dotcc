using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class PreprocessorMultilineCommentTests
{
    [Theory]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public void Block_comment_inside_a_macro_does_not_end_the_directive(string newline)
    {
        WithSource(("#define FIELDS(p) int p##_left; /* first\nsecond */ int p##_right;\n"
            + "struct Pair { FIELDS(item) };\n").Replace("\n", newline), path =>
        {
            using var output = new StringWriter();
            Compiler.Preprocess(new[] { path }, output);
            output.ToString().ShouldContain("item_right");
            output.ToString().ShouldNotContain("##");
            Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib).ShouldContain("item_right");
        });
    }

    [Theory]
    [InlineData("#define VALUE (4 /* first\nsecond */ + 5)\nint value=VALUE;", "( 4 + 5 )")]
    [InlineData("#if 1 /* first\nsecond */ && 0\n#error wrong branch\n#endif\nint value=7;", "value = 7")]
    [InlineData("#line 80 /* first\nsecond */\nint value=__LINE__;", "value = 80")]
    [InlineData("// /* not a block comment\n#define VALUE 7\nint value=VALUE;", "value = 7")]
    [InlineData("char *text=\"/* literal */\";\nint value=__LINE__;", "value = 2")]
    [InlineData("#define TEXT(x) #x\nchar *text=TEXT(a/* first\nsecond */b);", "\"a b\"")]
    public void Directive_membership_and_physical_lines_survive_comments(string source, string expected)
        => WithSource(source, path =>
        {
            using var output = new StringWriter();
            Compiler.Preprocess(new[] { path }, output);
            output.ToString().ShouldContain(expected);
        });

    [Fact]
    public void Comments_preserve_whitespace_between_macro_name_and_parenthesis()
        => WithSource("#define VALUE /* first\nsecond */ (x) x\nVALUE(7)\n", path =>
        {
            using var output = new StringWriter();
            Compiler.Preprocess(new[] { path }, output);
            output.ToString().ShouldContain("( x ) x ( 7 )");
        });

    [Fact]
    public void Comment_lines_and_splices_keep_original_utf8_diagnostic_offsets()
    {
        const string source = "/* λ\ncomment */\nint value = \\\n @;\n";
        WithSource(source, path =>
        {
            var error = Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }));
            error.Message.ShouldContain("line 4, column 2");
            error.Message.ShouldContain("byte offset " + System.Text.Encoding.UTF8.GetByteCount(source[..source.IndexOf('@')]));
        });
    }

    private static void WithSource(string source, Action<string> test)
    {
        string path = Path.Combine(Path.GetTempPath(), "dotcc-comment-lines-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, source);
        try { test(path); }
        finally { File.Delete(path); }
    }
}
