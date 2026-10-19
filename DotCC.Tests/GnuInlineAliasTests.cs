using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class GnuInlineAliasTests
{
    [Theory]
    [InlineData("__inline", "c90")]
    [InlineData("__inline__", "c90")]
    [InlineData("__inline", "c11")]
    [InlineData("__inline__", "c11")]
    public void Reserved_inline_aliases_keep_function_specifier_semantics(string spelling, string standard)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-gnu-inline-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, "#define INLINE " + spelling + "\n"
            + "static INLINE int increment(int n) { return n+1; }\n"
            + "int main(void) { return increment(6)-7; }\n");
        try
        {
            using var preprocessed = new StringWriter();
            Compiler.Preprocess([path], preprocessed);
            preprocessed.ToString().ShouldContain(spelling);
            Compiler.EmitObject(path, dialect: CDialect.Parse(standard))
                .ShouldContain("MethodImplOptions.AggressiveInlining");
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void An_explicit_macro_still_controls_a_reserved_alias()
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-gnu-inline-macro-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, "#define __inline\nstatic __inline int value(void) { return 4; }\n");
        try
        {
            Compiler.EmitObject(path).ShouldNotContain("MethodImplOptions.AggressiveInlining");
        }
        finally { File.Delete(path); }
    }
}
