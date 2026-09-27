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
    [InlineData("static int value[2] = {1,2}; static int value[2] = {1,2};")]
    [InlineData("struct Item { int x; }; const struct Item value = {1}; const struct Item value = {1};")]
    public void Multiple_initialized_definitions_are_rejected(string declarations)
    {
        Should.Throw<CompileException>(() => Emit(declarations + " int main(void) { return 0; }"))
            .Message.ShouldContain("redefinition of global 'value'");
    }

    [Theory]
    [InlineData("extern int value; long value;")]
    [InlineData("int *value; const int *value;")]
    [InlineData("typedef int (*A)(int); typedef int (*B)(long); A value; B value;")]
    [InlineData("static int value[2]; static int value[3];")]
    public void Incompatible_redeclarations_are_rejected(string declarations)
    {
        Should.Throw<CompileException>(() => Emit(declarations + " int main(void) { return 0; }"))
            .Message.ShouldContain("conflicting types for global 'value'");
    }

    [Fact]
    public void Repeated_tentative_declarations_emit_one_storage_field()
    {
        var output = Emit("int value; extern int value; int value = 17; int value; int main(void) { return value; }");
        (output.Split("public int value;").Length - 1).ShouldBe(1);
        output.ShouldContain("Globals.value = 17");
    }

    [Fact]
    public void Array_definition_replaces_tentative_storage_instead_of_allocating_twice()
    {
        var output = Emit("static unsigned char values[3]; static unsigned char values[3] = {1,2,3}; static unsigned char values[3]; int main(void) { return values[2]; }");
        (output.Split("public byte* values;").Length - 1).ShouldBe(1);
        output.ShouldNotContain("GlobalArrayZeroed<byte>(3)");
    }

    [Fact]
    public void Extern_initializer_is_a_definition()
    {
        var output = Emit("extern int value = 7; int value; int main(void) { return value; }");
        (output.Split("public int value;").Length - 1).ShouldBe(1);
        output.ShouldContain("Globals.value = 7");
    }
}
