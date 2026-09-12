using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class FunctionTypedefTests
{
    private static string Emit(string source)
    {
        var path = Path.Combine(Path.GetTempPath(), $"dotcc-function-typedef-{Guid.NewGuid():N}.c");
        File.WriteAllText(path, source);
        try { return Compiler.EmitCSharp(new[] { path }); }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData("typedef int Callback(int);", "int value")]
    [InlineData("typedef int (Callback)(int);", "int value")]
    [InlineData("typedef int Callback(void);", "void")]
    [InlineData("typedef int (Callback)();", "void")]
    public void Bare_function_alias_declarations_do_not_allocate_callback_storage(string alias, string parameters)
    {
        var code = Emit($"{alias} Callback target; int target({parameters}) {{ return 42; }} int main(void) {{ Callback *p = target; return p == 0; }}");
        code.ShouldContain("static unsafe int target(");
        code.ShouldContain("delegate*<");
    }

    [Theory]
    [InlineData("typedef struct Base { int value; } Base; struct Container { Base; int tail; };")]
    [InlineData("struct Base { int value; }; struct Container { struct Base; int tail; };")]
    public void Microsoft_anonymous_aggregate_members_reuse_existing_storage(string declarations)
    {
        var code = Emit(declarations + "int main(void) { struct Container c = {0}; c.value = 42; return c.value; }");
        code.ShouldContain("__anon_Base.value");
    }

    [Fact]
    public void Function_alias_sizeof_is_rejected()
        => Should.Throw<CompileException>(() => Emit("typedef int Callback(int); int main(void) { return sizeof(Callback); }"));

    [Fact]
    public void Function_declaration_initializer_is_rejected()
        => Should.Throw<CompileException>(() => Emit("typedef int Callback(int); Callback bad = 0; int main(void) { return 0; }"));

    [Fact]
    public void Gnu_attribute_between_return_type_and_function_name_preserves_pointer_type()
    {
        var code = Emit("static inline char * __attribute__((no_instrument_function)) identity(char *value) { return value; } int main(void) { char text[2] = {42,0}; return identity(text)[0]; }");
        code.ShouldContain("static unsafe byte* identity(");
    }

    [Fact]
    public void Previous_function_specifiers_do_not_leak_into_typedef_spelled_prototype()
    {
        var code = Emit("_Noreturn void stop(void) { for (;;) {} } typedef int Callback(void); Callback target; int target(void) { return 42; } int main(void) { return target(); }");
        var baseline = Emit("int main(void) { return 0; }");
        code.Split("[System.Diagnostics.CodeAnalysis.DoesNotReturn]").Length
            .ShouldBe(baseline.Split("[System.Diagnostics.CodeAnalysis.DoesNotReturn]").Length + 1);
    }

    [Fact]
    public void Gnu_noreturn_list_does_not_leak_to_next_function()
    {
        var code = Emit("__attribute__((noinline, noreturn)) void stop(void) { for (;;) {} } __attribute__((always_inline, no_instrument_function)) int main(void) { return 0; }");
        var baseline = Emit("int main(void) { return 0; }");
        code.Split("[System.Diagnostics.CodeAnalysis.DoesNotReturn]").Length
            .ShouldBe(baseline.Split("[System.Diagnostics.CodeAnalysis.DoesNotReturn]").Length + 1);
    }
}
