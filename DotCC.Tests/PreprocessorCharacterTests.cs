using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class PreprocessorCharacterTests
{
    [Theory]
    [InlineData("'A' != '\\301'")]
    [InlineData("'A' == 65 && '\\101' == 65 && '\\x41' == 65")]
    [InlineData("'\\n' == 10 && '\\t' == 9 && '\\r' == 13")]
    [InlineData("'\\\\' == 92 && '\\'' == 39 && '\\?' == 63")]
    [InlineData("'\\0' == 0 && '\\377' == 255")]
    [InlineData("LETTER == 65 && ID(LETTER) == 65 && ID('B') == 66")]
    public void Character_constants_choose_the_same_branch_as_C(string condition)
    {
        var output = Preprocess("#define LETTER 'A'\n#define ID(x) x\n#if " + condition
            + "\nint selected;\n#else\nint wrong;\n#endif\n");
        output.ShouldContain("selected");
        output.ShouldNotContain("wrong");
    }

    [Fact]
    public void Spaced_directives_identify_character_expression_context()
    {
        var output = Preprocess("""
            # if 'A' == 65
            int first;
            # else
            int wrong;
            # endif
            # if 0
            int wrong;
            #  elif '\101' == 65
            int second;
            # else
            int wrong;
            # endif
            """);
        output.ShouldContain("first");
        output.ShouldContain("second");
        output.ShouldNotContain("wrong");
    }

    [Fact]
    public void Elif_and_macro_stringification_preserve_character_spelling()
    {
        var output = Preprocess("""
            #define LETTER 'A'
            #define STR(x) #x
            #if 'A' == '\301'
            int wrong;
            #elif LETTER == '\x41'
            int selected;
            #else
            int wrong;
            #endif
            int ordinary = LETTER;
            const char *spelling = STR('\101');
            """);
        output.ShouldContain("selected");
        output.ShouldNotContain("wrong");
        output.ShouldContain("'A'");
        output.ShouldContain("\"'\\\\101'\"");
    }

    private static string Preprocess(string source)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-preprocessor-char-" + Guid.NewGuid().ToString("N") + ".c");
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
