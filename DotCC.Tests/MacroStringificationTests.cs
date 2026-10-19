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
    public void Object_like_aliases_preserve_nested_argument_token_boundaries()
    {
        var output = Preprocess("""
            #define STR(x) #x
            #define XSTR(x) STR(x)
            #define MAJOR 1
            #define MINOR 5
            #define PATCH 7
            #define NUMBERS MAJOR.MINOR.PATCH
            #define VERSION XSTR(NUMBERS)
            #define SPACED XSTR(MAJOR . MINOR . PATCH)
            #define NAME identifier
            #define PUNCTUATION XSTR(NAME+NAME->member)
            #define EMPTY
            #define REMOVED XSTR(NAME EMPTY+NAME)
            const char *version = VERSION;
            const char *spaced = SPACED;
            const char *punctuation = PUNCTUATION;
            const char *removed = REMOVED;
            """);
        output.ShouldContain("version = \"1.5.7\"");
        output.ShouldContain("spaced = \"1 . 5 . 7\"");
        output.ShouldContain("punctuation = \"identifier+identifier->member\"");
        output.ShouldContain("removed = \"identifier +identifier\"");
    }

    [Fact]
    public void Object_expansion_before_stringification_retains_comments_and_literal_spelling()
    {
        var output = Preprocess("""
            #define STR(x) #x
            #define XSTR(x) STR(x)
            #define WORD word
            #define LITERAL "a\\b\"c"
            #define ALIAS XSTR(WORD/**/+   LITERAL)
            const char *text = ALIAS;
            """);
        output.ShouldContain(""""
            text = "word + \"a\\\\b\\\"c\""
            """");
    }

    [Fact]
    public void Removed_trailing_macro_tokens_preserve_whitespace_before_the_callers_suffix()
    {
        var output = Preprocess("""
            #define STR(x) #x
            #define XSTR(x) STR(x)
            #define EMPTY
            #define INNER x EMPTY
            #define OUTER XSTR(INNER+y)
            #define FORWARD(arg) XSTR(arg+y)
            #define PREFIX XSTR(
            const char *outer = OUTER;
            const char *forward = FORWARD(INNER);
            const char *prefix = PREFIX INNER+y);
            const char *direct = XSTR(INNER+y);
            """);
        foreach (var name in new[] { "outer", "forward", "prefix", "direct" })
            output.ShouldContain(name + " = \"x +y\"");
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
    public void Object_function_rescan_preserves_disabled_macro_names()
    {
        var output = Preprocess("""
            #define GLOBAL(type,value) value
            int storage;
            #define storage GLOBAL(int,storage)
            #define ALIAS storage
            #define PASS(x) x
            int value = storage;
            int alias = ALIAS;
            int nested = PASS(ALIAS);
            """);
        output.ShouldContain("value = storage ;");
        output.ShouldContain("alias = storage ;");
        output.ShouldContain("nested = storage ;");
        output.ShouldNotContain("GLOBAL");
    }

    [Fact]
    public void Function_alias_prescans_external_arguments_and_intersects_boundary_hides()
    {
        var output = Preprocess("""
            #define A F
            #define F(x) x
            first = A(A);
            #undef F
            #define F(x) A
            second = A(1);
            #undef A
            #define A F(1)
            third = A;
            """);
        output.ShouldContain("first = F ;");
        output.ShouldContain("second = F ;");
        output.ShouldContain("third = A ;");
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
