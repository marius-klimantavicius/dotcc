using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class PreprocessingOtherTokenTests
{
    [Theory]
    [InlineData("$variable")]
    [InlineData("@annotation `command` \\path")]
    public void Inactive_regions_discard_non_C_preprocessing_tokens(string text)
    {
        WithSource("#if 0\n" + text + "\n#if 1\n$nested\n#endif\n#else\nint selected;\n#endif\n", path =>
        {
            Compiler.EmitCSharp(new[] { path }).ShouldContain("selected");
            using var output = new StringWriter();
            Compiler.Preprocess(new[] { path }, output);
            output.ToString().ShouldContain("selected");
            output.ToString().ShouldNotContain("$nested");
        });
    }

    [Fact]
    public void Macro_stringification_can_consume_other_preprocessing_tokens()
    {
        WithSource("#define STR(x) #x\nconst char *text = STR($ @ `);\n", path =>
            Compiler.EmitCSharp(new[] { path }).ShouldContain("$ @ `"));
    }

    [Fact]
    public void Preprocess_only_preserves_active_other_tokens()
    {
        WithSource("$ @ `\n", path =>
        {
            using var output = new StringWriter();
            Compiler.Preprocess(new[] { path }, output);
            output.ToString().ShouldContain("$ @ `");
        });
    }

    [Fact]
    public void Inactive_nested_conditions_are_not_evaluated()
    {
        WithSource("#if 0\n#if @\n$unused\n#endif\n#endif\nint selected;\n", path =>
            Compiler.EmitCSharp(new[] { path }).ShouldContain("selected"));
    }

    [Fact]
    public void Evaluated_condition_rejects_other_tokens()
    {
        WithSource("#if @\nint wrong;\n#endif\n", path =>
        {
            using var output = new StringWriter();
            Should.Throw<CompileException>(() => Compiler.Preprocess(new[] { path }, output))
                .Message.ShouldContain("lex failed");
        });
    }

    [Theory]
    [InlineData("$")]
    [InlineData("@")]
    [InlineData("`")]
    [InlineData("\\")]
    public void Surviving_other_tokens_are_rejected_as_C_tokens(string token)
    {
        WithSource("int value = " + token + ";\n", path =>
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }))
                .Message.ShouldContain("lex failed"));
    }

    private static void WithSource(string source, Action<string> test)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-pp-other-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "main.c");
        File.WriteAllText(path, source + "\nint main(void) { return 0; }\n");
        try { test(path); }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
