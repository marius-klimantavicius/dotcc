using System;
using System.IO;
using DotCC;
using Xunit;

namespace DotCC.Tests;

public sealed class InferredAggregateArrayDimensionsTests
{
    [Theory]
    [InlineData("struct Pair", "{{{1,2}}, {{3,4},{5,6}}}")]
    [InlineData("union Value", "{{{1}}, {{2},{3}}}")]
    [InlineData("struct Pair", "{[2][1] = {7,8}, [0] = {{9,10}}}")]
    public void Omitted_outer_bound_uses_array_rows_for_aggregate_elements(string element, string initializer)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-inferred-aggregate-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, $$"""
            struct Pair { int a; int b; };
            union Value { int a; unsigned b; };
            static const {{element}} values[][2] = {{initializer}};
            int main(void) { return sizeof(values) / sizeof(values[0]); }
            """);
        try { Compiler.EmitCSharp(new[] { path }); }
        finally { File.Delete(path); }
    }
}
