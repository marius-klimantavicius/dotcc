using System;
using System.IO;
using Xunit;

namespace DotCC.Tests;

public sealed class PrimitiveSizeofTypeTests
{
    [Theory]
    [InlineData("1U < -1")]
    [InlineData("1UL < -1")]
    [InlineData("4294967295U == -1")]
    [InlineData("18446744073709551615UL == -1")]
    [InlineData("-1L < 1U")]
    [InlineData("!(1U < -1L)")]
    [InlineData("65535U != (unsigned short)65535 + 1")]
    public void Integer_constant_comparisons_use_C_common_type(string condition)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-constant-compare-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, "_Static_assert(" + condition + ", \"common integer type\");\nint main(void) { return 0; }\n");
        try { Compiler.EmitCSharp(new[] { path }); }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData("char")]
    [InlineData("int")]
    [InlineData("long")]
    [InlineData("Word")]
    public void Sizeof_retains_size_t_type_and_unsigned_promotions(string type)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-sizeof-type-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, "typedef long Word;\n"
            + "_Static_assert(_Generic(sizeof(" + type + "), unsigned long: 1, default: 0), \"sizeof type\");\n"
            + "_Static_assert(sizeof(" + type + ") < -1, \"unsigned comparison\");\n"
            + "int main(void) { return 0; }\n");
        try { Compiler.EmitCSharp(new[] { path }); }
        finally { File.Delete(path); }
    }
}
