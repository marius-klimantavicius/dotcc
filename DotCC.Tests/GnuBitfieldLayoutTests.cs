using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class GnuBitfieldLayoutTests
{
    private static string Emit(string fields)
    {
        string path = Path.Combine(Path.GetTempPath(), $"dotcc-bitfield-gnu-{Guid.NewGuid():N}.c");
        File.WriteAllText(path, $"struct S {{ {fields} }}; int main(void) {{ return (int)offsetof(struct S, tail); }}");
        try { return Compiler.EmitCSharp(new[] { path }); }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData("unsigned char prefix; unsigned a:2; unsigned b:7; unsigned short tail;", 8, 4, 4)]
    [InlineData("unsigned char prefix[3]; unsigned a:12; unsigned char tail;", 8, 4, 6)]
    [InlineData("unsigned char prefix; unsigned :0; unsigned char tail;", 5, 1, 4)]
    [InlineData("unsigned char prefix; unsigned :2; unsigned char tail;", 3, 1, 2)]
    [InlineData("unsigned char prefix[300000000]; unsigned a:1; unsigned char tail;", 300000004, 4, 300000001)]
    [InlineData("unsigned char prefix; unsigned char a:3; unsigned b:7; unsigned short c:5; signed int d:6; unsigned char tail;", 8, 4, 4)]
    public void Native_gnu_size_alignment_and_member_offsets(string fields, int size, int alignment, int offset)
    {
        var emitted = Emit(fields);
        emitted.ShouldContain($"LayoutKind.Explicit, Size = {size}, Pack = {alignment}");
        emitted.ShouldContain($"public const ulong Value = {offset}UL;");
        emitted.ShouldContain($"FieldOffset({offset})");
    }

    [Fact]
    public void Leading_byte_and_mixed_width_fields_share_measured_bit_positions()
    {
        var emitted = Emit("unsigned char prefix; unsigned char a:3; unsigned b:7; unsigned short c:5; signed int d:6; unsigned char tail;");
        emitted.ShouldContain("FieldOffset(1)]\n    private byte __bf0;");
        emitted.ShouldContain("FieldOffset(2)]\n    private ushort __bf1;");
        emitted.ShouldNotContain("__bf2");
        emitted.ShouldContain("(__bf0 >> 3)");
        emitted.ShouldContain("(__bf1 >> 2)");
        emitted.ShouldContain("(__bf1 >> 7)");
        emitted.ShouldNotContain("__bytes");
    }
}
