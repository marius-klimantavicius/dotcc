using System;
using System.IO;
using Xunit;

namespace DotCC.Tests;

public sealed class ScalarTypedefAlignmentTests
{
    [Theory]
    [InlineData("typedef __attribute__((aligned(1))) unsigned long T;")]
    [InlineData("typedef unsigned long U; typedef __attribute__((aligned(1))) U T;")]
    [InlineData("typedef unsigned long T __attribute__((__aligned__(1)));")]
    [InlineData("typedef unsigned long __attribute__((aligned(1))) T;")]
    public void Weakened_scalar_typedef_alignment_preserves_size_and_compatibility(string declaration)
    {
        Compile(declaration + """
            _Static_assert(sizeof(T) == sizeof(unsigned long), "size");
            _Static_assert(_Alignof(T) == 1, "alignment");
            typedef const T Qualified;
            _Static_assert(_Alignof(Qualified) == 1, "qualified alias");
            _Static_assert(_Generic((T)0, unsigned long: 1, default: 0), "compatible type");
            struct Record { char lead; T value; char tail; };
            _Static_assert(sizeof(struct Record) == 10, "record size");
            int main(void) { return 0; }
            """);
    }

    [Fact]
    public void Local_typedef_alignment_and_pointer_alignment_are_distinct()
    {
        Compile("""
            int main(void) {
                typedef __attribute__((aligned(1))) unsigned long T;
                typedef T *Pointer;
                _Static_assert(_Alignof(T) == 1, "local alias");
                _Static_assert(_Alignof(Pointer) == 8, "pointer retains natural alignment");
                return 0;
            }
            """);
    }

    [Fact]
    public void Invalid_scalar_alignment_is_rejected()
    {
        var error = Assert.ThrowsAny<CompileException>(() =>
            Compile("typedef __attribute__((aligned(3))) unsigned long T; int main(void) { return 0; }"));
        Assert.Contains("positive power of 2", error.Message);
    }

    private static void Compile(string text)
    {
        var path = Path.Combine(Path.GetTempPath(), $"dotcc-scalar-alignment-{Guid.NewGuid():N}.c");
        File.WriteAllText(path, text);
        try { Assert.NotEmpty(Compiler.EmitCSharp(new[] { path })); }
        finally { File.Delete(path); }
    }
}
