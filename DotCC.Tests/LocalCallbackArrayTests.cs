using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class LocalCallbackArrayTests
{
    private static string Emit(string source)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-local-callback-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, source);
        try { return Compiler.EmitObject(path); }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData("int (*f[2])(int)={target}; return f[1]==0;")]
    [InlineData("int (*f[])(int)={target,target}; return f[1](2);")]
    [InlineData("int (*const f[2][2])(int)={{target},{target,target}}; return f[1][0](2);")]
    [InlineData("int n=2; int (*f[n])(int); f[0]=target; return f[0](2);")]
    public void Local_callback_arrays_keep_bounds_signature_and_qualifiers(string declaration)
        => Emit("int target(int x) { return x+1; } int main(void) { " + declaration + " }").ShouldNotBeEmpty();

    [Theory]
    [InlineData("int (*const f[2])(int)={target}; f[0]=target;")]
    [InlineData("int (*const f)(int)=target; f=target;")]
    public void Const_callback_storage_remains_read_only(string declaration)
        => Should.Throw<CompileException>(() => Emit("int target(int x) { return x; } int main(void) { "
            + declaration + "return 0; }")).Message.ShouldContain("read-only");

    [Fact]
    public void An_incomplete_automatic_callback_array_is_rejected()
        => Should.Throw<CompileException>(() => Emit("int main(void) { int (*f[])(int); return 0; }"))
            .Message.ShouldContain("complete bounds");
}
