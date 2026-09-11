#nullable enable
using System;
using System.IO;
using DotCC;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class ConstPointerTailTests
{
    [Theory]
    [InlineData("end = p;")]
    [InlineData("*end = 1;")]
    public void Later_const_pointer_retains_both_pointer_and_pointee_qualification(string invalidWrite)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-const-tail-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, $$"""
            int main(void) { const char *p = "hello", *const end = p + 5; {{invalidWrite}} return 0; }
            """);
        try { Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path })); }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Const_pointer_at_inner_level_does_not_make_outer_pointer_const()
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-const-tail-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, """
            int main(void) { int value = 7; int *p = &value, *const fixed_p = p, *const *cursor = &fixed_p; cursor = &fixed_p; **cursor = 9; return value - 9; }
            """);
        try { Compiler.EmitCSharp(new[] { path }).ShouldContain("int** cursor"); }
        finally { File.Delete(path); }
    }
}
