using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class StdintConstantMacroTests
{
    [Theory]
    [InlineData("INT8_C", "123")]
    [InlineData("UINT8_C", "123")]
    [InlineData("INT16_C", "123")]
    [InlineData("UINT16_C", "123")]
    [InlineData("INT32_C", "123")]
    [InlineData("UINT32_C", "123U")]
    [InlineData("INT64_C", "123L")]
    [InlineData("UINT64_C", "123UL")]
    [InlineData("INTMAX_C", "123L")]
    [InlineData("UINTMAX_C", "123UL")]
    public void Integer_constant_macros_expand_to_target_promoted_types(string macro, string expected)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-stdint-constant-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "main.c");
        File.WriteAllText(path, "#include <stdint.h>\nint value = " + macro + "(123);\n");
        try
        {
            using var output = new StringWriter();
            Compiler.Preprocess(new[] { path }, output);
            output.ToString().ShouldNotContain(macro);
            output.ToString().ShouldContain("value = " + expected + " ;");
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
