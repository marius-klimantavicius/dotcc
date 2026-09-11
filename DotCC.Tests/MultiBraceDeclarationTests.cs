#nullable enable
using System;
using System.IO;
using DotCC;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class MultiBraceDeclarationTests
{
    private static string Emit(string source, string standard = "c17")
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-multi-brace-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, source);
        try { return Compiler.EmitCSharp(new[] { path }, dialect: CDialect.Parse(standard)); }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Picotls_iovec_declarations_bind_both_zero_initializers()
    {
        var emitted = Emit("""
            typedef struct { unsigned char *base; unsigned long len; } ptls_iovec_t;
            int main(void) { ptls_iovec_t pubkey = {0}, ecdh_secret = {0}; return pubkey.len + ecdh_secret.len; }
            """);
        emitted.ShouldContain("pubkey");
        emitted.ShouldContain("ecdh_secret");
    }

    [Theory]
    [InlineData("struct pair first = {1}, second = {.b=2};", "")]
    [InlineData("static struct pair first = {1}, second = {.b=2};", "")]
    [InlineData("", "static struct pair first = {1}, second = {.b=2};")]
    public void Positional_and_designated_lists_bind_at_each_storage_scope(string globals, string locals)
    {
        Emit($$"""
            struct pair { int a; int b; };
            {{globals}}
            int main(void) { {{locals}} return first.a + second.b - 3; }
            """).ShouldContain("second");
    }

    [Fact]
    public void First_const_pointer_does_not_change_following_object_type()
    {
        var emitted = Emit("""
            struct pair { int value; };
            int main(void) {
                struct pair value = {1};
                struct pair *const first = {&value}, second = {2}, *third = {&second};
                return first->value + second.value + third->value - 5;
            }
            """);
        emitted.ShouldContain("pair second");
        emitted.ShouldContain("pair* third");
    }

    [Theory]
    [InlineData("first = &value;")]
    [InlineData("*second = 2;")]
    public void Braces_preserve_pointer_and_pointee_const_constraints(string invalidWrite)
    {
        Should.Throw<CompileException>(() => Emit($$"""
            int main(void) { int value = 1; const int *const first = {&value}, *second = {&value}; {{invalidWrite}} return 0; }
            """));
    }

    [Fact]
    public void Empty_lists_bind_each_declarator_type_in_c23()
    {
        Emit("""
            struct pair { int value; };
            int main(void) { struct pair first = {}, second = {}; int scalar = {}, *pointer = {}; return first.value + second.value + scalar + (pointer != 0); }
            """, "c23").ShouldContain("pointer");
    }
}
