using System;
using System.IO;
using System.Text.RegularExpressions;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class IncludeMacroOrderTests
{
    private static string Preprocess(string header, string source)
    {
        var dir = Path.Combine(Path.GetTempPath(), "dotcc-include-order-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            File.WriteAllText(Path.Combine(dir, "body.h"), header);
            var path = Path.Combine(dir, "main.c");
            File.WriteAllText(path, source);
            using var output = new StringWriter();
            Compiler.Preprocess(new[] { path }, output);
            return Regex.Replace(output.ToString(), @"\s+", "");
        }
        finally { Directory.Delete(dir, recursive: true); }
    }

    [Fact]
    public void Later_header_macro_does_not_rewrite_an_earlier_prototype()
    {
        var output = Preprocess("int alloc(int);\n#define alloc(x) ((x)+10)\n",
            "#include \"body.h\"\nint result=alloc(5);\n");
        output.ShouldContain("intalloc(int);");
        output.ShouldContain("intresult=((5)+10);");
    }

    [Fact]
    public void Header_expansions_keep_the_definition_in_effect_at_their_use()
    {
        var output = Preprocess("""
            #define PICK(x) x
            int first=PICK(value);
            #define value 42
            #define F(x) ((x)+1)
            int second=F(2);
            #undef F
            #define F(x) ((x)+10)
            """, "#include \"body.h\"\nint third=F(value);\n");
        output.ShouldContain("intfirst=value;");
        output.ShouldContain("intsecond=((2)+1);");
        output.ShouldContain("intthird=((42)+10);");
    }

    [Fact]
    public void Header_tail_function_macro_name_does_not_consume_parent_tokens()
    {
        var output = Preprocess("#define F(x) ((x)+1)\nF\n",
            "int result=\n#include \"body.h\"\n(41);\nint next=F(5);\n");
        output.ShouldContain("intresult=F(41);");
        output.ShouldContain("intnext=((5)+1);");
    }
}
