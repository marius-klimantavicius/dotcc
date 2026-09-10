#nullable enable

using System.IO;
using DotCC;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

/// <summary>Shared layout constants emitted directly into standalone C# source.</summary>
[Collection("Offsetof")]
public sealed class OffsetofTests
{
    [Fact]
    public void offsets_emit_constants_directly_without_delegates()
    {
        var src = WriteTemp("struct S { char a; double b; }; int main(void) { return (int)offsetof(struct S, b); }");
        try
        {
            var emitted = Compiler.EmitCSharp(new[] { src });
            emitted.ShouldContain("const ulong");
            emitted.ShouldContain("dotcc-layout-v1");
            emitted.ShouldNotContain("System.Func<ulong>");
        }
        finally { File.Delete(src); }
    }

    private static string WriteTemp(string body)
    {
        var path = Path.Combine(Path.GetTempPath(), $"dotcc-of-{System.Guid.NewGuid():N}.c");
        File.WriteAllText(path, body);
        return path;
    }

    [Fact]
    public void indexed_nested_member_designator_is_a_constant()
    {
        var src = WriteTemp("struct I { char a; int b; }; struct O { char tag; struct I items[3]; }; int main(void) { return (int)offsetof(struct O, items[2].b); }");
        try { Compiler.EmitCSharp(new[] { src }).ShouldContain("public const ulong Value = 24UL;"); }
        finally { File.Delete(src); }
    }

    [Fact]
    public void bitfield_offset_is_diagnosed()
    {
        var src = WriteTemp("struct S { int a:3; }; int main(void) { return (int)offsetof(struct S, a); }");
        try { Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { src })).Message.ShouldContain("bit-field"); }
        finally { File.Delete(src); }
    }

    [Fact]
    public void modellable_struct_folds_to_constant()
    {
        var src = WriteTemp("""
            struct S { double a; char b; int c; };
            int main(void) { return (int)offsetof(struct S, c); }
            """);
        try
        {
            var emitted = Compiler.EmitCSharp(new[] { src });
            emitted.ShouldContain("public const ulong Value = 12UL;");
            emitted.ShouldNotContain("System.Func<ulong>");
        }
        finally { File.Delete(src); }
    }

    [Fact]
    public void alignment_padding_folds()
    {
        var src = WriteTemp("""
            struct S { char c; double d; };
            int main(void) { return (int)offsetof(struct S, d); }
            """);
        try
        {
            Compiler.EmitCSharp(new[] { src }).ShouldContain("public const ulong Value = 8UL;");
        }
        finally { File.Delete(src); }
    }

    [Fact]
    public void offsetof_as_array_bound_folds_to_literal_dimension()
    {
        var src = WriteTemp("""
            struct S { int a; double b; };
            int main(void) { char pad[offsetof(struct S, b)]; return (int)sizeof(pad); }
            """);
        try
        {
            Compiler.EmitCSharp(new[] { src }).ShouldContain("stackalloc byte[8]");
        }
        finally { File.Delete(src); }
    }

    [Fact]
    public void union_member_offset_is_zero()
    {
        var src = WriteTemp("""
            union U { int a; double b; };
            int main(void) { return (int)offsetof(union U, b); }
            """);
        try
        {
            var emitted = Compiler.EmitCSharp(new[] { src });
            emitted.ShouldContain("public const ulong Value = 0UL;");
            emitted.ShouldNotContain("System.Func<ulong>");
        }
        finally { File.Delete(src); }
    }

    [Fact]
    public void field_after_bitfield_uses_lowered_storage_model()
    {
        var src = WriteTemp("""
            struct S { int a; int b : 3; int c; };
            int main(void) { return (int)offsetof(struct S, c); }
            """);
        try
        {
            var emitted = Compiler.EmitCSharp(new[] { src });
            emitted.ShouldNotContain("System.Func<ulong>");
            emitted.ShouldContain("public const ulong Value = 8UL;");
        }
        finally { File.Delete(src); }
    }

    [Fact]
    public void fixed_buffer_member_has_constant_offset()
    {
        var src = WriteTemp("""
            struct S { int a : 3; int grid[2]; };
            int main(void) { return (int)offsetof(struct S, grid); }
            """);
        try
        {
            var emitted = Compiler.EmitCSharp(new[] { src });
            emitted.ShouldContain("public const ulong Value = 4UL;");
            emitted.ShouldNotContain("System.Func<ulong>");
        }
        finally { File.Delete(src); }
    }

    [Fact]
    public void duplicate_offsetof_emits_one_helper()
    {
        var src = WriteTemp("""
            struct S { int x : 3; int b; };
            int main(void) { return (int)offsetof(struct S, b) + (int)offsetof(struct S, b); }
            """);
        try
        {
            var emitted = Compiler.EmitCSharp(new[] { src });
            emitted.Split("internal static class __DotccOffset_").Length.ShouldBe(2);
            emitted.ShouldNotContain("System.Func<ulong>");
        }
        finally { File.Delete(src); }
    }
}
