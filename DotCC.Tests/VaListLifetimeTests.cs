using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class VaListLifetimeTests
{
    [Theory]
    [InlineData("va_list saved;", "global")]
    [InlineData("void f(int n,...) { static va_list saved; va_start(saved,n); }", "global")]
    [InlineData("typedef va_list cursor; struct S { cursor saved; };", "field")]
    [InlineData("void f(void) { va_list cursors[2]; }", "array")]
    [InlineData("void f(va_list *cursor) { }", "pointer")]
    [InlineData("void sink(void *p); void f(int n,...) { va_list a; va_start(a,n); sink(&a); }", "pointer")]
    [InlineData("int f(va_list a) { return sizeof a; }", "sizeof")]
    [InlineData("int f(void) { return _Alignof(va_list); }", "_Alignof")]
    [InlineData("void sink(int n,...); void f(va_list a) { sink(1,a); }", "variadic argument")]
    public void Ref_struct_cursor_storage_and_layout_are_diagnosed(string source, string reason)
        => WithSource(source, path => Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }))
            .Message.ShouldContain(reason));

    [Theory]
    [InlineData("va_list f(int n,...) { va_list a; va_start(a,n); return a; }")]
    [InlineData("va_list f(int n,...) { va_list a,b,c; va_start(a,n); va_copy(b,a); c=b; return c; }")]
    [InlineData("va_list relay(va_list a) { return a; } va_list f(int n,...) { va_list a; va_start(a,n); return relay(a); }")]
    public void Returning_a_locally_started_cursor_is_diagnosed(string source)
        => WithSource(source, path => Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }))
            .Message.ShouldContain("return"));

    [Fact]
    public void Comma_delegate_cannot_capture_a_cursor()
        => WithSource("void touch(void); int f(va_list a) { return 1 && (touch(),va_arg(a,int)); }", path =>
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }))
                .Message.ShouldContain("capture"));

    [Fact]
    public void Comma_tuple_cannot_store_a_cursor_value()
        => WithSource("int touch(void); va_list f(va_list a) { return 1 ? (touch(),a) : a; }", path =>
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }))
                .Message.ShouldContain("tuple"));

    [Fact]
    public void Comma_delegate_cannot_return_a_cursor_without_capturing_one()
        => WithSource("void touch(void); va_list empty(void); va_list f(int n) { return n ? (touch(),empty()) : empty(); }", path =>
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }))
                .Message.ShouldContain("delegate result"));

    [Fact]
    public void Incoming_cursor_returns_and_callback_storage_are_preserved()
        => WithSource("""
            typedef va_list cursor;
            typedef int (*reader)(cursor);
            struct Callbacks { reader read; };
            int consume(cursor a) { return va_arg(a,int); }
            reader callback = consume;
            cursor relay(cursor a) { cursor copy; va_copy(copy,a); return copy; }
            int forward(cursor a) { return callback(relay(a)); }
            """, path => Compiler.EmitCSharp(new[] { path }).ShouldContain("VaList relay(VaList a)"));

    [Fact]
    public void Scalar_comma_reads_and_statement_commas_do_not_capture()
        => WithSource("""
            void touch(void);
            int f(va_list a) {
                touch(), va_arg(a,int);
                return (va_arg(a,int),va_arg(a,int));
            }
            """, path => Compiler.EmitCSharp(new[] { path }).ShouldContain(".Next()"));

    [Fact]
    public void Locally_started_cursor_aliases_and_overwritten_parameters_are_scoped()
        => WithSource("""
            int f(va_list incoming, int n,...) {
                va_list a,b,c;
                va_start(a,n); va_copy(b,a); c=b;
                va_start(incoming,n);
                return va_arg(c,int)+va_arg(incoming,int);
            }
            """, path => {
                var emitted = Compiler.EmitCSharp(new[] { path });
                emitted.ShouldContain("scoped VaList incoming");
                emitted.ShouldContain("scoped VaList a = default, b = default, c = default");
            });

    [Fact]
    public void Hoisted_comma_cursor_reads_are_preserved()
        => WithSource("void touch(void); int f(va_list a) { return (touch(),va_arg(a,int)); }",
            path => Compiler.EmitCSharp(new[] { path }).ShouldContain(".Next()"));

    private static void WithSource(string source, Action<string> test)
    {
        string directory = Path.Combine(Path.GetTempPath(), "dotcc-va-lifetime-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, "main.c");
        File.WriteAllText(path, "#include <stdarg.h>\n" + source + "\nint main(void) { return 0; }\n");
        try { test(path); }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
