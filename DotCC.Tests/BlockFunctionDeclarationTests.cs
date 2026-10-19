using System;
using System.IO;
using Xunit;

namespace DotCC.Tests;

public sealed class BlockFunctionDeclarationTests
{
    [Theory]
    [InlineData("long target(void);")]
    [InlineData("long target();")]
    [InlineData("long target(long value);")]
    [InlineData("extern long target(long);")]
    [InlineData("long (target)(long);")]
    public void Block_prototypes_preserve_function_declarators(string declaration)
    {
        WithSource("void caller(void) { " + declaration + " }", path => Assert.NotEmpty(Compiler.EmitObject(path)));
    }

    [Fact]
    public void Block_function_declarations_cannot_request_static_storage()
    {
        WithSource("void caller(void) { static int target(void); }",
            path => Assert.ThrowsAny<CompileException>(() => Compiler.EmitObject(path)));
    }

    private static void WithSource(string source, Action<string> check)
    {
        var path = Path.Combine(Path.GetTempPath(), $"dotcc-block-functions-{Guid.NewGuid():N}.c");
        File.WriteAllText(path, source);
        try { check(path); }
        finally { File.Delete(path); }
    }
}
