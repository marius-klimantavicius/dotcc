using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class RuntimeIntrinsicTests
{
    private static void Source(string source, Action<string> action)
    {
        var dir = Path.Combine(Path.GetTempPath(), "dotcc-intrinsic-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try { var path = Path.Combine(dir, "test.c"); File.WriteAllText(path, source); action(path); }
        finally { Directory.Delete(dir, true); }
    }
    [Fact]
    public void Endian_is_a_typed_runtime_fact_not_a_host_constant_or_import()
    {
        Source("""
            #define ENDIAN __dotcc_is_little_endian()
            #define ALIAS ENDIAN
            #define COMBINED (ALIAS ? 2 : 3)
            int main(void) { _Bool b = ENDIAN; return b + (unsigned char)ALIAS + COMBINED; }
            """, path =>
        {
            using var report = new StringWriter();
            var output = Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib,
                preprocessing: new CPreprocessingOptions(Array.Empty<MacroOverride>(), report: report));
            output.ShouldContain("((CBool)global::System.BitConverter.IsLittleEndian)");
            output.ShouldNotContain("const int ENDIAN");
            output.ShouldNotContain("const int ALIAS");
            output.ShouldNotContain("const int COMBINED");
            output.ShouldNotContain("extern int __dotcc_is_little_endian");
            report.ToString().ShouldContain("macro-export-skipped");
            report.ToString().ShouldContain("\"event\":\"intrinsic\"");
            Should.Throw<CompileException>(() => Compiler.EmitWat(new[] { path })).Message.ShouldContain("WAT backend");
        });
    }
    [Theory]
    [InlineData("#if __dotcc_is_little_endian()\n#endif", "#if/#elif")]
    [InlineData("#define A __dotcc_is_little_endian()\n#if A\n#endif", "#if/#elif")]
    [InlineData("#define A() __dotcc_is_little_endian()\n#if A()\n#endif", "#if/#elif")]
    [InlineData("#if 0 && __dotcc_is_little_endian()\n#endif", "#if/#elif")]
    [InlineData("#if 1 || __dotcc_is_little_endian()\n#endif", "#if/#elif")]
    [InlineData("#if 0\n#elif __dotcc_is_little_endian()\n#endif", "#if/#elif")]
    [InlineData("int f(void) { return __dotcc_is_little_endian(1); }", "zero arguments")]
    [InlineData("int f(void) { return __dotcc_is_little_endian; }", "must be invoked")]
    [InlineData("int f(void) { return (int)&__dotcc_is_little_endian; }", "address")]
    [InlineData("int f(void) { return (int)&__dotcc_is_little_endian(); }", "lvalue")]
    [InlineData("int f(void) { __dotcc_is_little_endian() = 1; return 0; }", "lvalue")]
    [InlineData("int f(void) { return __dotcc_is_little_endian()++; }", "lvalue")]
    [InlineData("int __dotcc_is_little_endian(void);", "reserved")]
    [InlineData("int __dotcc_is_little_endian;", "reserved")]
    [InlineData("typedef int __dotcc_is_little_endian;", "reserved")]
    [InlineData("typedef int (*__dotcc_is_little_endian)(void);", "reserved")]
    [InlineData("typedef struct { int n; } __dotcc_is_little_endian;", "reserved")]
    [InlineData("#define __dotcc_is_little_endian() 1", "redefine")]
    [InlineData("int f(void) { return __dotcc_unknown(); }", "unknown dotcc intrinsic")]
    [InlineData("enum E { X = __dotcc_is_little_endian() };", "non-constant")]
    [InlineData("enum E { X = 0 && __dotcc_is_little_endian() };", "non-constant")]
    [InlineData("_Static_assert(__dotcc_is_little_endian(), \"x\");", "not constant")]
    [InlineData("int f(int n) { switch(n) { case __dotcc_is_little_endian(): return 1; } return 0; }", "case label")]
    [InlineData("int x = __dotcc_is_little_endian();", "static initializer")]
    [InlineData("int x[] = { __dotcc_is_little_endian() };", "static initializer")]
    [InlineData("struct S {int x;}; struct S s = { __dotcc_is_little_endian() };", "static initializer")]
    [InlineData("int f(void) { static int x = __dotcc_is_little_endian(); return x; }", "static initializer")]
    [InlineData("int x[__dotcc_is_little_endian() + 1];", "constant")]
    [InlineData("int x[__dotcc_is_little_endian() + 1] = {1};", "constant")]
    public void Invalid_intrinsic_context_has_a_compiler_diagnostic(string source, string diagnostic) =>
        Source(source, path => Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path })).Message.ShouldContain(diagnostic));

    [Fact]
    public void Defined_and_inactive_groups_do_not_evaluate_runtime_intrinsics()
    {
        Source("""
            #if defined(__dotcc_is_little_endian)
            #error not a predefined macro
            #endif
            #if 0
            #if __dotcc_is_little_endian()
            #error inactive
            #endif
            #endif
            int f(void) { int a[1+__dotcc_is_little_endian()]; a[0]=42; return a[0]; }
            """, path => Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib).ShouldContain("BitConverter.IsLittleEndian"));
    }
}
