using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class IncompleteRowParameterTests
{
    [Theory]
    [InlineData("return sizeof(*row);")]
    [InlineData("return row[1][0];")]
    [InlineData("return (row + 1) != row;")]
    public void A_pointer_to_an_unknown_row_extent_cannot_supply_a_stride_or_size(string body)
    {
        string path = Path.Combine(Path.GetTempPath(), "dotcc-incomplete-row-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, "int f(char (*row)[]) { " + body + " }\n");
        try { Should.Throw<CompileException>(() => Compiler.EmitObject(path)).Message.ShouldContain("complete"); }
        finally { File.Delete(path); }
    }
}
