using System;
using System.IO;
using Xunit;

namespace DotCC.Tests;

public sealed class QualifiedCallbackParameterTests
{
    [Theory]
    [InlineData("int invoke(int (*const callback)(int)) { callback=target; return 0; }")]
    [InlineData("int invoke(int (*const *slot)(int)) { *slot=target; return 0; }")]
    public void Parameter_declarators_retain_callback_constness(string function)
    {
        var error = Assert.ThrowsAny<CompileException>(() =>
            Emit("int target(int value) { return value; } " + function));
        Assert.Contains("read-only", error.Message);
    }

    [Theory]
    [InlineData("int invoke(int (*const callback)(int)) { return callback(42); }")]
    [InlineData("int invoke(int (*const *slot)(int)) { return (*slot)(42); }")]
    [InlineData("int invoke(int (*const callbacks[2])(int)) { return callbacks[1](42); }")]
    [InlineData("int invoke(int (*const callbacks[])(int)) { return callbacks[0](42); }")]
    public void Qualified_callback_parameter_shapes_lower(string source) => Emit(source);

    private static void Emit(string text)
    {
        var path = Path.Combine(Path.GetTempPath(), $"dotcc-callback-parameter-{Guid.NewGuid():N}.c");
        File.WriteAllText(path, text);
        try { Assert.NotEmpty(Compiler.EmitObject(path)); }
        finally { File.Delete(path); }
    }
}
