using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class TentativeGlobalTests
{
    private static string Emit(string source)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-tentative-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, source);
        try { return Compiler.EmitCSharp(new[] { path }); }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData("int value = 1; int value = 1;")]
    [InlineData("int value; int value = 1; int value = 2;")]
    [InlineData("extern int value = 1; int value = 2;")]
    public void Multiple_initialized_definitions_are_rejected(string declarations)
    {
        Should.Throw<CompileException>(() => Emit(declarations + " int main(void) { return 0; }"))
            .Message.ShouldContain("redefinition of global 'value'");
    }

    [Theory]
    [InlineData("extern int value; long value;")]
    [InlineData("int *value; const int *value;")]
    [InlineData("typedef int (*A)(int); typedef int (*B)(long); A value; B value;")]
    public void Incompatible_redeclarations_are_rejected(string declarations)
    {
        Should.Throw<CompileException>(() => Emit(declarations + " int main(void) { return 0; }"))
            .Message.ShouldContain("conflicting types for global 'value'");
    }

    [Fact]
    public void Repeated_tentative_declarations_emit_one_storage_field()
    {
        var output = Emit("int value; extern int value; int value = 17; int value; int main(void) { return value; }");
        (output.Split("unsafe int value").Length - 1).ShouldBe(1);
        output.ShouldContain("int value = 17");
    }

    [Fact]
    public void Extern_initializer_is_a_definition()
    {
        var output = Emit("extern int value = 7; int value; int main(void) { return value; }");
        (output.Split("unsafe int value").Length - 1).ShouldBe(1);
        output.ShouldContain("int value = 7");
    }
}
