using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class PointerArrayOperatorTests
{
    [Theory]
    [InlineData("rows + rows", "addition of two pointers")]
    [InlineData("1 - rows", "subtraction of a pointer from an integer")]
    public void Invalid_pointer_array_arithmetic_is_diagnosed(string expression, string diagnostic)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-array-operator-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, $"int main(void) {{ int rows[2][3]; return (int)({expression}); }}");
        try
        {
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }))
                .Message.ShouldContain(diagnostic);
        }
        finally { File.Delete(path); }
    }
}
