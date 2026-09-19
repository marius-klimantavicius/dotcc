#nullable enable
using System;
using System.IO;
using DotCC;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class Libsmb2SyntaxTests
{
    private static string Emit(string source)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-libsmb2-syntax-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "main.c");
        File.WriteAllText(path, source);
        try { return Compiler.EmitCSharp(new[] { path }); }
        finally { Directory.Delete(directory, true); }
    }

    [Fact]
    public void Standalone_anonymous_enum_constants_bind_in_file_and_block_scope()
    {
        Emit("enum { FIRST = 5, SECOND }; int main(void) { enum { LOCAL = SECOND + 2 }; return LOCAL; }")
            .ShouldContain("return 8;");
    }

    [Fact]
    public void Diagnostic_unused_attributes_accept_parameters_and_local_declarators()
    {
        Emit("int main(int value __attribute__((unused))) { int local __attribute__((__unused__)) = 7; return local; }")
            .ShouldContain("int local = 7;");
    }

    [Fact]
    public void Unused_array_declarators_keep_dimensions_and_initializers()
    {
        Emit("int main(void) { unsigned char b[16] __attribute__((unused)), y[16] __attribute__((unused)); b[0] = 7; return b[0]; }")
            .ShouldContain("stackalloc byte[16]");
    }

    [Fact]
    public void Reserved_gnu_typeof_spelling_works_in_c17()
    {
        Emit("int main(void) { int value = 7; const __typeof__(value) copy = value; return copy; }")
            .ShouldContain("int copy = value");
    }

    [Theory]
    [InlineData("return 7;")]
    [InlineData("break;")]
    public void Unsupported_statement_expression_control_transfer_is_rejected(string body)
    {
        Should.Throw<CompileException>(() => Emit("int main(void) { return ({ " + body + " 2; }); }"))
            .Message.ShouldContain("statement expression");
    }

    [Fact]
    public void Unknown_parameter_attributes_are_rejected()
    {
        Should.Throw<CompileException>(() => Emit("int main(int value __attribute__((unknown_abi))) { return value; }"))
            .Message.ShouldContain("unsupported GNU attribute");
    }

    [Fact]
    public void Lazy_statement_expression_rejects_address_taken_capture()
    {
        Should.Throw<CompileException>(() => Emit("int main(void) { int value = 7; int *p = &value; return *p && ({ value; }); }"))
            .Message.ShouldContain("capturing an address-taken local");
    }

    [Fact]
    public void Statement_expression_scope_preserves_the_outer_binding()
    {
        Emit("int main(void) { int local = 9; int result = ({ int local = 7; local; }); return local; }")
            .ShouldContain("return local;");
    }

    [Fact]
    public void Statement_expression_returns_final_expression()
    {
        Emit("int main(void) { return ({ int value = 7; value + 2; }); }").ShouldContain("value + 2");
    }
}
