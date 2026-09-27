using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class QuickJsDeclarationTests
{
    [Theory]
    [InlineData("noinline")]
    [InlineData("unused")]
    [InlineData("always_inline")]
    public void Static_function_attribute_before_return_type_preserves_definition(string attribute)
    {
        Emit($"static __attribute__(({attribute})) int helper(int x) {{ return x + 1; }} int main(void) {{ return helper(41); }}")
            .ShouldContain("helper(");
    }

    [Fact]
    public void Chained_static_function_attributes_preserve_definition()
        => Emit("static __attribute__((noinline)) __attribute__((warn_unused_result)) int helper(void) { return 42; } int main(void) { return helper(); }")
            .ShouldContain("helper(");

    [Fact]
    public void Inline_attribute_before_keyword_return_type_preserves_definition()
        => Emit("static inline __attribute__((warn_unused_result)) int helper(void) { return 42; } int main(void) { return helper(); }")
            .ShouldContain("helper(");

    [Fact]
    public void Inline_attribute_before_typedef_return_type_preserves_definition()
        => Emit("typedef struct Shape { int value; } Shape; static inline __attribute__((always_inline)) Shape *identity(Shape *p) { return p; } int main(void) { return 0; }")
            .ShouldContain("identity(");

    [Fact]
    public void Aligned_flexible_member_controls_tail_offset_size_and_aggregate_alignment()
    {
        Emit("""
            #include <stddef.h>
            struct Header { unsigned short index; unsigned char size; unsigned char mark; int refs;
                __attribute__((aligned(16))) unsigned char data[]; };
            _Static_assert(offsetof(struct Header, data) == 16, "tail offset");
            _Static_assert(sizeof(struct Header) == 16, "header size");
            _Static_assert(_Alignof(struct Header) == 16, "header alignment");
            int main(void) { return 0; }
            """).ShouldContain("Header");
    }

    [Fact]
    public void Flexible_member_after_comma_keeps_zero_storage_and_element_alignment()
        => Emit("""
            #include <stddef.h>
            struct Value { long payload, tag; };
            struct Data { int index; struct Value iter, next, values[]; };
            _Static_assert(offsetof(struct Data, values) == 40, "tail offset");
            _Static_assert(sizeof(struct Data) == 40, "header size");
            int main(void) { return 0; }
            """).ShouldContain("Data");

    [Fact]
    public void Static_minimum_array_parameter_adjusts_to_pointer()
        => Emit("static int helper(double fields[static 9]) { return sizeof(fields) == sizeof(double *); } int main(void) { double values[9]; return !helper(values); }")
            .ShouldContain("double* fields");

    [Theory]
    [InlineData("0")]
    [InlineData("bound()")]
    public void Static_array_bounds_are_not_silently_erased_when_runtime_evaluation_is_required(string bound)
        => Should.Throw<CompileException>(() => Emit($"int bound(void); int helper(double fields[static {bound}]) {{ return 0; }}"));

    [Fact]
    public void Alignas_before_multiple_keyword_specifiers_preserves_layout()
        => Emit("""
            struct Header { _Alignas(16) unsigned char value; };
            _Static_assert(_Alignof(struct Header) == 16, "alignment");
            int main(void) { return 0; }
            """).ShouldContain("Header");

    [Fact]
    public void Unknown_member_attribute_still_fails()
        => Should.Throw<CompileException>(() => Emit("struct Header { __attribute__((unknown_abi)) int value; };"));

    private static string Emit(string source)
    {
        var path = Path.Combine(Path.GetTempPath(), $"dotcc-quickjs-declarations-{Guid.NewGuid():N}.c");
        File.WriteAllText(path, source);
        try { return Compiler.EmitCSharp(new[] { path }, dialect: CDialect.Parse("c11")); }
        finally { File.Delete(path); }
    }
}
