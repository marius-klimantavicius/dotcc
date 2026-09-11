#nullable enable
using System;
using System.IO;
using DotCC;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class StructExpressionArrayTests
{
    private static string Emit(string initializer)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-struct-expression-array-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, $$"""
            struct pair { int a; int b; };
            struct other { int a; int b; };
            struct pair make(int a) { struct pair result = {a}; return result; }
            int main(void) { const struct pair value = {7}; struct other wrong = {8}; struct pair values[3] = { {{initializer}} }; return values[2].a; }
            """);
        try { return Compiler.EmitCSharp(new[] { path }); }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData("make(1), make(2)")]
    [InlineData("value, {9}")]
    public void Compatible_aggregate_values_copy_and_omitted_elements_zero_fill(string initializer)
    {
        var emitted = Emit(initializer);
        emitted.ShouldContain("default(pair)");
    }

    [Fact]
    public void Different_struct_with_identical_members_is_not_assignment_compatible()
    {
        Should.Throw<CompileException>(() => Emit("wrong"));
    }
}
