#nullable enable
using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class MacroStringificationTests
{
    private static string Preprocess(string source)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-stringify-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, source);
        try
        {
            using var output = new StringWriter();
            Compiler.Preprocess(new[] { path }, output);
            return output.ToString();
        }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData("->", "->")]
    [InlineData("->>", "->>")]
    [InlineData("-> >", "-> >")]
    [InlineData("a+b", "a+b")]
    [InlineData("  a  +\t b  ", "a + b")]
    [InlineData("a/**/b", "a b")]
    [InlineData("a\n+b", "a +b")]
    [InlineData("a\\\n+b", "a+b")]
    [InlineData("", "")]
    public void Stringification_preserves_adjacency_and_normalizes_only_existing_whitespace(string input, string expected)
    {
        Preprocess("#define STR(x) #x\nconst char *s=STR(" + input + ");\n")
            .ShouldContain("\"" + expected + "\"");
    }

    [Fact]
    public void Raw_argument_is_not_expanded_but_forwarded_argument_is()
    {
        var output = Preprocess("""
            #define VALUE 42
            #define STR(x) #x
            #define XSTR(x) STR(x)
            const char *raw = STR(VALUE);
            const char *expanded = XSTR(VALUE);
            """);
        output.ShouldContain("raw = \"VALUE\"");
        output.ShouldContain("expanded = \"42\"");
    }

    [Fact]
    public void Replacement_boundaries_keep_formal_parameter_spacing()
    {
        var output = Preprocess("""
            #define STR(x) #x
            #define XSTR(x) STR(x)
            #define JOIN(x) a+x
            #define SPACED(x) a + x
            #define OP ->>
            const char *tight = XSTR(JOIN(b));
            const char *spaced = XSTR(SPACED(b));
            const char *arrow = XSTR(OP);
            """);
        output.ShouldContain("tight = \"a+b\"");
        output.ShouldContain("spaced = \"a + b\"");
        output.ShouldContain("arrow = \"->>\"");
    }

    [Fact]
    public void Variadic_stringification_retains_comma_spacing()
    {
        Preprocess("#define STR(...) #__VA_ARGS__\nconst char *s=STR(a,b , c);\n")
            .ShouldContain("\"a,b , c\"");
    }

    [Fact]
    public void Empty_replacements_retain_their_boundary_whitespace()
    {
        var output = Preprocess("""
            #define STR(x) #x
            #define XSTR(x) STR(x)
            #define PREFIX(x) a x+b
            #define EMPTY
            const char *parameter = XSTR(PREFIX());
            const char *object = XSTR(a EMPTY+b);
            """);
        output.ShouldContain("parameter = \"a +b\"");
        output.ShouldContain("object = \"a +b\"");
    }

    [Fact]
    public void Stringification_escapes_literal_quotes_and_backslashes()
    {
        var output = Preprocess("""
            #define STR(x) #x
            const char *s = STR("a\\b\"c");
            """);
        output.ShouldContain(
            """"
            "\"a\\\\b\\\"c\""
            """");
    }
}
