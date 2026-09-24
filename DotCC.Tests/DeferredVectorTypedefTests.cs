using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class DeferredVectorTypedefTests
{
    private const string Declaration = "typedef unsigned long long lanes __attribute__((vector_size(16)));\n";

    [Fact]
    public void Unused_vector_typedef_is_retained_without_scalar_storage()
        => WithSource(Declaration + "int value(void) { return 42; }", path =>
            Compiler.EmitObject(path).ShouldContain("value"));

    [Theory]
    [InlineData("lanes global;")]
    [InlineData("lanes *pointer;")]
    [InlineData("struct S { lanes member; };")]
    [InlineData("int size(void) { return sizeof(lanes); }")]
    [InlineData("int alignment(void) { return _Alignof(lanes); }")]
    [InlineData("void consume(lanes argument);")]
    [InlineData("lanes produce(void);")]
    [InlineData("void local(void) { lanes value; }")]
    public void Materializing_or_querying_vector_types_has_an_explicit_diagnostic(string use)
        => WithSource(Declaration + use, path =>
        {
            var error = Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }));
            error.Message.ShouldContain("vector_size(16)");
            error.Message.ShouldContain("lanes");
            error.Message.ShouldContain("lowering is not implemented");
        });

    [Theory]
    [InlineData("0")]
    [InlineData("12")]
    [InlineData("4")]
    [InlineData("-16")]
    public void Invalid_vector_widths_are_rejected_even_when_unused(string width)
        => WithSource("typedef unsigned long long lanes __attribute__((vector_size(" + width + ")));", path =>
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }))
                .Message.ShouldContain("positive power-of-two byte size"));

    [Fact]
    public void Local_scalar_typedef_shadows_the_deferred_vector_typedef()
        => WithSource(Declaration + "int value(void) { typedef unsigned int lanes; lanes local = 42; return local; }", path =>
            Compiler.EmitObject(path).ShouldContain("value"));

    [Fact]
    public void Leaving_local_typedef_scope_restores_the_vector_diagnostic()
        => WithSource(Declaration + "void local(void) { typedef int lanes; lanes value; }\nlanes global;", path =>
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }))
                .Message.ShouldContain("vector_size(16)"));

    [Fact]
    public void Typedef_registries_do_not_leak_between_translation_units()
    {
        WithSource(Declaration + "int first(void) { return 1; }", first => Compiler.EmitObject(first));
        WithSource("typedef int lanes; lanes value = 42;", second => Compiler.EmitObject(second));
    }

    private static void WithSource(string source, Action<string> action)
    {
        string path = Path.Combine(Path.GetTempPath(), "dotcc-deferred-vector-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, source);
        try { action(path); }
        finally { File.Delete(path); }
    }
}
