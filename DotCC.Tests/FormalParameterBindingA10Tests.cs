using DotCC;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class FormalParameterBindingA10Tests
{
    [Theory]
    [InlineData("strftime(output, 64, format, calendar)", "strftime(output, (ulong)(64), format, calendar)")]
    [InlineData("strftime(output, 4, format, calendar)", "strftime(output, (ulong)(4), format, calendar)")]
    [InlineData("strftime(output, 1, format, calendar)", "strftime(output, (ulong)(1), format, calendar)")]
    [InlineData("sin(fraction)", "sin((double)(fraction))")]
    [InlineData("sin(truth)", "sin((double)(int)(truth))")]
    public void Direct_runtime_calls_bind_the_fixed_C_formal_type(string call, string expected)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-formal-a10-" + Guid.NewGuid().ToString("N") + ".c");
        try
        {
            File.WriteAllText(path, $$"""
                #include <time.h>
                #include <math.h>
                double invoke(char *output, const char *format, const struct tm *calendar) {
                    float fraction=1.25f;
                    _Bool truth=1;
                    return {{call}};
                }
                """);
            Compiler.EmitCSharp([path], emit: EmitMode.ManagedLib).ShouldContain(expected);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Generated_C_calls_and_fixed_function_pointers_keep_their_existing_typed_boundary()
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-formal-controls-a10-" + Guid.NewGuid().ToString("N") + ".c");
        try
        {
            File.WriteAllText(path, """
                unsigned long identity(unsigned long value) { return value; }
                unsigned long invoke(void) {
                    unsigned long (*callback)(unsigned long)=identity;
                    return identity(64)+callback(4);
                }
                """);
            var emitted = Compiler.EmitCSharp([path], emit: EmitMode.ManagedLib);
            emitted.ShouldContain("identity(64)");
            emitted.ShouldContain("callback(4)");
            emitted.ShouldNotContain("identity((ulong)(64))");
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Runtime_variadic_tail_keeps_C_default_promotions()
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-formal-varargs-a10-" + Guid.NewGuid().ToString("N") + ".c");
        try
        {
            File.WriteAllText(path, """
                #include <stdio.h>
                int invoke(char *output) {
                    float fraction=1.25f;
                    unsigned short narrow=65535;
                    return snprintf(output, 64, "%g %d", fraction, narrow);
                }
                """);
            var emitted = Compiler.EmitCSharp([path], emit: EmitMode.ManagedLib);
            emitted.ShouldContain(".Arg((double)(fraction)).Arg((int)(narrow)).Done()");
        }
        finally { File.Delete(path); }
    }
}
