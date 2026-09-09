using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class PointerConditionalTests
{
    [Theory]
    [InlineData("int *a; double *b; int main(void) { return (1 ? a : b) == 0; }")]
    [InlineData("typedef int (*Callback)(int); Callback a; void *b; int main(void) { return (1 ? a : b) == 0; }")]
    [InlineData("int *a; int b; int main(void) { return (1 ? a : b) == 0; }")]
    public void Incompatible_pointer_conditional_operands_are_diagnosed(string source)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-pointer-conditional-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, source);
        try
        {
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }))
                .Message.ShouldContain("conditional pointer operands");
        }
        finally { File.Delete(path); }
    }
}
