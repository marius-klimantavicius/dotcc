using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class NestedStringArrayTests
{
    [Theory]
    [InlineData("static char rows[1][2] = {\"abc\"};", "too long")]
    [InlineData("static char rows[1][2] = {\"a\", \"b\"};", "too many string initializers")]
    [InlineData("static char rows[][2] = {\"abc\"};", "too long")]
    [InlineData("static int rows[][2] = {{1, 2, 3}};", "too many initializers")]
    [InlineData("static int rows[][0] = {{1}};", "positive constant inner dimensions")]
    public void Nested_character_array_initializers_reject_overflow(string declaration, string diagnostic)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-string-array-" + Guid.NewGuid().ToString("N") + ".c");
        try {
            File.WriteAllText(path, declaration + "\nint main(void) { return 0; }\n");
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path })).Message.ShouldContain(diagnostic);
        }
        finally { File.Delete(path); }
    }
}
