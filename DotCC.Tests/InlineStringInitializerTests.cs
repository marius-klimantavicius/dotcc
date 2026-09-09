using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class InlineStringInitializerTests
{
    [Theory]
    [InlineData("\"abcd\"")]
    [InlineData("{\"abcd\"}")]
    [InlineData("\"λab\"")]
    public void Literal_characters_cannot_overflow_inline_storage(string initializer)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-inline-string-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, "struct Text { char value[3]; }; static struct Text text = {" + initializer + "}; int main(void) { return 0; }");
        try
        {
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }))
                .Message.ShouldContain("string literal is too long for inline array member [3]");
        }
        finally { File.Delete(path); }
    }
}
