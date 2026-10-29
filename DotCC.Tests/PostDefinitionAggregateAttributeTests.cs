using System;
using System.IO;
using Xunit;

namespace DotCC.Tests;

public sealed class PostDefinitionAggregateAttributeTests
{
    [Theory]
    [InlineData("struct", "short a; short b; short c; short d; short e;", "packed", 10, 1)]
    [InlineData("struct", "char a; int b; short c;", "packed", 7, 1)]
    [InlineData("struct", "short a; short b; short c; short d; short e;", "packed, aligned(8)", 16, 8)]
    [InlineData("struct", "short a; short b; short c; short d; short e;", "aligned(16)", 16, 16)]
    [InlineData("union", "char a; long b;", "packed", 8, 1)]
    [InlineData("union", "char a; long b;", "packed, aligned(16)", 16, 16)]
    public void Post_definition_attributes_preserve_layout_at_file_and_block_scope(
        string kind, string fields, string attribute, int size, int alignment)
    {
        var declaration = $"{kind} Header {{ {fields} }} __attribute__(({attribute}));";
        var assertion = $"_Static_assert(sizeof({kind} Header)=={size} && _Alignof({kind} Header)=={alignment}, \"layout\");";
        foreach (bool block in new[] { false, true })
        {
            var source = block ? $"int main(void) {{ {declaration} {assertion} return 0; }}"
                : declaration + assertion + "int main(void) { return 0; }";
            var path = Path.Combine(Path.GetTempPath(), "dotcc-post-aggregate-" + Guid.NewGuid().ToString("N") + ".c");
            File.WriteAllText(path, source);
            try { Assert.NotEmpty(Compiler.EmitObject(path)); }
            finally { File.Delete(path); }
        }
    }
}
