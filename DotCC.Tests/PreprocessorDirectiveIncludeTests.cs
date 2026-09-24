using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class PreprocessorDirectiveIncludeTests
{
    [Theory]
    [InlineData("#define HEADER \"chosen.h\"\n#include HEADER")]
    [InlineData("#define HEADER <chosen.h>\n#define ALIAS HEADER\n#include ALIAS")]
    [InlineData("#define STR_INNER(x) #x\n#define STR(x) STR_INNER(x)\n#include STR(chosen.h)")]
    public void Include_operand_expands_object_and_function_macros(string directive)
        => WithHeaders(directive, output => output.ShouldContain("chosen_header"));

    [Fact]
    public void Literal_header_names_are_not_macro_expanded()
        => WithHeaders("#define literal chosen\n#include <literal.h>", output =>
        {
            output.ShouldContain("literal_header");
            output.ShouldNotContain("chosen_header");
        });

    [Fact]
    public void Has_include_uses_macro_expansion_and_keeps_lazy_branch_evaluation()
        => WithHeaders("""
            #define HEADER "chosen.h"
            #if __has_include(HEADER)
            int found_header;
            #else
            int wrong;
            #endif
            """, output =>
        {
            output.ShouldContain("found_header");
            output.ShouldNotContain("wrong");
        });

    [Fact]
    public void Null_directives_disappear_without_changing_macro_stringification()
        => WithHeaders("#\n  # /* ignored */\n#define STR(x) #x\nchar *value = STR(token);\n", output =>
        {
            output.ShouldContain("\"token\"");
            output[(output.IndexOf('\n') + 1)..].ShouldNotContain("#");
        });

    private static void WithHeaders(string source, Action<string> check)
    {
        string directory = Path.Combine(Path.GetTempPath(), "dotcc-pp-include-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "chosen.h"), "int chosen_header;\n");
        File.WriteAllText(Path.Combine(directory, "literal.h"), "int literal_header;\n");
        string path = Path.Combine(directory, "main.c");
        File.WriteAllText(path, source + "\n");
        try
        {
            using var output = new StringWriter();
            Compiler.Preprocess(new[] { path }, output, includeDirs: new[] { directory });
            check(output.ToString());
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
