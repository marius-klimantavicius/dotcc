using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class SizeofArrayBoundTests
{
    [Theory]
    [InlineData("char[-1]")]
    [InlineData("char[2][-1]")]
    [InlineData("char[4294967296UL]")]
    public void Invalid_or_unrepresentable_array_bounds_are_not_folded_to_scalar_sizes(string type)
    {
        string path = Path.Combine(Path.GetTempPath(), "dotcc-sizeof-bound-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, "int main(void) { return sizeof(" + type + "); }\n");
        try
        {
            Should.Throw<CompileException>(() => Compiler.EmitObject(path)).Message.ShouldContain("bound");
        }
        finally { File.Delete(path); }
    }
}
