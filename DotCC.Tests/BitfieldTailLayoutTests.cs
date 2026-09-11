using System;
using System.IO;
using DotCC;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class BitfieldTailLayoutTests
{
    private static string Emit(string source)
    {
        string path = Path.Combine(Path.GetTempPath(), $"dotcc-bitfield-tail-{Guid.NewGuid():N}.c");
        File.WriteAllText(path, source);
        try { return Compiler.EmitCSharp(new[] { path }); }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(7, 1)]
    [InlineData(9, 2)]
    [InlineData(17, 3)]
    public void Ordinary_byte_uses_bitfield_tail_and_accessors_do_not_touch_it(int width, int offset)
    {
        string emitted = Emit($$"""
            struct Tail { unsigned bits : {{width}}; unsigned char next; };
            int main(void) { return (int)offsetof(struct Tail, next); }
            """);
        emitted.ShouldContain($"public const ulong Value = {offset}UL;");
        emitted.ShouldContain("LayoutKind.Explicit, Size = 4, Pack = 4");
        emitted.ShouldContain($"[System.Runtime.InteropServices.FieldOffset({offset})]\n    public byte next;");
        emitted.ShouldContain("private uint __bf0;");
        emitted.ShouldContain($"__bytes[{offset - 1}]");
        emitted.ShouldNotContain($"__bytes[{offset}]");
        emitted.ShouldNotContain("__bf0 =");
    }

    [Fact]
    public void Pointer_sized_prefix_keeps_native_aggregate_alignment()
    {
        string emitted = Emit("""
            struct Tail { unsigned long prefix; unsigned bits : 1; unsigned char next; unsigned long suffix; };
            int main(void) { return (int)offsetof(struct Tail, next); }
            """);
        emitted.ShouldContain("public const ulong Value = 9UL;");
        emitted.ShouldContain("LayoutKind.Explicit, Size = 24, Pack = 8");
        emitted.ShouldContain("[System.Runtime.InteropServices.FieldOffset(8)]\n    private uint __bf0;");
        emitted.ShouldContain("[System.Runtime.InteropServices.FieldOffset(16)]\n    public ulong suffix;");
    }

    [Theory]
    [InlineData("unsigned bits : 1; unsigned : 0; unsigned char next;", 4)]
    [InlineData("unsigned bits : 25; unsigned char next;", 4)]
    [InlineData("unsigned bits : 1; unsigned int next;", 4)]
    public void Barrier_full_occupied_bytes_and_member_alignment_prevent_tail_reuse(string fields, int offset)
    {
        string emitted = Emit($$"""
            struct Tail { {{fields}} };
            int main(void) { return (int)offsetof(struct Tail, next); }
            """);
        emitted.ShouldContain($"public const ulong Value = {offset}UL;");
        // Explicit storage now uses the same shared layout map for all bitfields.
        emitted.ShouldContain("LayoutKind.Explicit, Size =");
        emitted.ShouldContain("__bf0 =");
    }

    [Fact]
    public void Flexible_tail_reuses_padding_without_losing_header_alignment()
    {
        string emitted = Emit("""
            struct Tail { unsigned bits : 1; unsigned char data[]; };
            int main(void) { return (int)offsetof(struct Tail, data); }
            """);
        emitted.ShouldContain("public const ulong Value = 1UL;");
        emitted.ShouldContain("public const int Size = 4;");
        emitted.ShouldContain("__dotcc_flex_alignment");
        emitted.ShouldNotContain("__bytes[1]");
    }
}
