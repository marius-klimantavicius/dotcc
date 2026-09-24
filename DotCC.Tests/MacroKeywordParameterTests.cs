using System;
using System.IO;
using Xunit;

namespace DotCC.Tests;

public sealed class MacroKeywordParameterTests
{
    [Theory]
    [InlineData("default")]
    [InlineData("enum")]
    [InlineData("int")]
    [InlineData("void")]
    [InlineData("struct")]
    [InlineData("const")]
    [InlineData("switch")]
    [InlineData("sizeof")]
    [InlineData("_Atomic")]
    public void Language_keywords_are_preprocessing_identifiers_in_parameters(string parameter)
    {
        var output = Preprocess($"#define VALUE 42\n#define USE({parameter}) ({parameter})\nint value=USE(VALUE);\n");
        Assert.Contains("int value = ( 42 ) ;", output);
    }

    [Fact]
    public void Keyword_parameters_are_substituted_before_token_pasting_and_rescanned()
    {
        var output = Preprocess("""
            #define config_default 42
            #define JOIN(enum, default) enum ## default
            int value=JOIN(config_, default);
            """);
        Assert.Contains("int value = 42 ;", output);
    }

    [Fact]
    public void Keyword_parameters_preserve_raw_stringification_and_prescan()
    {
        var output = Preprocess("""
            #define VALUE 42
            #define USE(default) #default, default, "default"
            const char *parts=USE(VALUE);
            """);
        Assert.Contains("\"VALUE\" , 42 , \"default\"", output);
    }

    [Fact]
    public void Keyword_parameters_expand_inside_directive_expressions()
    {
        var output = Preprocess("""
            #define ADD(default, enum) ((default)+(enum))
            #if ADD(40,2) == 42
            int value=42;
            #else
            int wrong=0;
            #endif
            """);
        Assert.Contains("int value = 42 ;", output);
        Assert.DoesNotContain("wrong", output);
    }

    private static string Preprocess(string source)
    {
        var path = Path.Combine(Path.GetTempPath(), $"dotcc-keyword-parameters-{Guid.NewGuid():N}.c");
        File.WriteAllText(path, source);
        try
        {
            using var output = new StringWriter();
            Compiler.Preprocess(new[] { path }, output);
            return output.ToString();
        }
        finally { File.Delete(path); }
    }
}
