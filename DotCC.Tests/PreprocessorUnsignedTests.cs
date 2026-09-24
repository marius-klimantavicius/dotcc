using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class PreprocessorUnsignedTests
{
    [Theory]
    [InlineData("18446744073709551615UL == 0xffffffffffffffffULL", true)]
    [InlineData("18446744073709551615UL > 0", true)]
    [InlineData("0xffffffffffffffffUL > 0x7fffffffffffffffL", true)]
    [InlineData("-1 < 1U", false)]
    [InlineData("-1 == 18446744073709551615ULL", true)]
    [InlineData("18446744073709551615UL / 3 == 6148914691236517205ULL", true)]
    [InlineData("18446744073709551615UL % 10 == 5", true)]
    [InlineData("18446744073709551615UL + 1 == 0", true)]
    [InlineData("(0x8000000000000000ULL >> 63) == 1", true)]
    [InlineData("(-8 >> 2) == -2", true)]
    [InlineData("~0U == 18446744073709551615UL", true)]
    [InlineData("(1 ? -1 : 0U) > 0", true)]
    [InlineData("(1 ? -1 : (0U + 0)) > 0", true)]
    [InlineData("(1 ? -1 : (0U << 1)) > 0", true)]
    [InlineData("012 == 10 && 0xFUL == 15 && 0b1010U == 10", true)]
    [InlineData("-5 / 2 == -2 && -5 % 2 == -1", true)]
    [InlineData("0 && 1 / 0", false)]
    [InlineData("1 || 1 / 0", true)]
    [InlineData("1 ? 1 : 1 / 0", true)]
    [InlineData("0 ? 1 / 0 : 1", true)]
    [InlineData("1 ? 1 : 1 << 64", true)]
    public void Intmax_and_uintmax_branches_match_C(string expression, bool selected)
    {
        string output = Preprocess("#if " + expression + "\nint chosen;\n#else\nint other;\n#endif\n");
        output.ShouldContain(selected ? "chosen" : "other");
        output.ShouldNotContain(selected ? "other" : "chosen");
    }

    [Fact]
    public void Header_limits_function_macros_defined_and_elif_expand_in_expression_context()
    {
        string output = Preprocess("""
            #include <stdint.h>
            #define ID(x) x
            #define ALIAS ID
            #define MARKER not_a_number
            #if UINTPTR_MAX != 0xffffffffffffffffULL
            int wrong;
            #elif defined MARKER && defined(ID) && ALIAS(UINT64_C(0xffffffffffffffff)) > 0
            int chosen;
            #else
            int wrong;
            #endif
            #if 0
            #if 1 / 0
            int wrong;
            #endif
            #elif 1
            int second;
            #elif 1 / 0
            int wrong;
            #endif
            """);
        output.ShouldContain("chosen");
        output.ShouldContain("second");
        output.ShouldNotContain("wrong");
    }

    [Theory]
    [InlineData("1 / 0", "division by zero")]
    [InlineData("1 << 64", "shift count")]
    [InlineData("18446744073709551616ULL", "out-of-range integer")]
    public void Evaluated_invalid_arithmetic_is_diagnosed(string expression, string message)
        => Should.Throw<CompileException>(() => Preprocess("#if " + expression + "\n#endif\n"))
            .Message.ShouldContain(message);

    [Theory]
    [InlineData("packed", true)]
    [InlineData("__aligned__", true)]
    [InlineData("format", true)]
    [InlineData("alloc_size", true)]
    [InlineData("unused", true)]
    [InlineData("target", false)]
    [InlineData("weak", false)]
    [InlineData("no_sanitize", false)]
    [InlineData("invented_attribute", false)]
    public void Attribute_capability_queries_match_supported_frontend_attributes(string attribute, bool supported)
    {
        string output = Preprocess("#if defined(__has_attribute) && __has_attribute(" + attribute +
            ")\nint chosen;\n#else\nint other;\n#endif\n");
        output.ShouldContain(supported ? "chosen" : "other");
        output.ShouldNotContain(supported ? "other" : "chosen");
    }

    [Fact]
    public void Guarded_unsupported_attribute_query_still_parses_as_a_real_builtin()
    {
        string output = Preprocess("#if defined(__x86_64__) && defined(__has_attribute) && __has_attribute(target)\nint wrong;\n#else\nint chosen;\n#endif\n");
        output.ShouldContain("chosen");
        output.ShouldNotContain("wrong");
    }

    [Fact]
    public void Conditional_diagnostics_retain_the_source_filename_and_line()
    {
        var error = Should.Throw<CompileException>(() => Preprocess("\n#if 1 / 0\n#endif\n"));
        error.Message.ShouldContain(".c:2:");
    }

    private static string Preprocess(string source)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-pp-unsigned-" + Guid.NewGuid().ToString("N") + ".c");
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
