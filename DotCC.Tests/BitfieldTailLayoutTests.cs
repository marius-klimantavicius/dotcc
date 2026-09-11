using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
        try
        {
            var emitted = Compiler.EmitCSharp(new[] { path });
            emitted.ShouldNotContain("__bytes");
            emitted.ShouldNotContain("__storage");
            foreach (var (offset, bytes) in Storage(emitted)) (offset % bytes).ShouldBe(0);
            return emitted;
        }
        finally { File.Delete(path); }
    }

    private static (int Offset, int Bytes)[] Storage(string emitted) =>
        Regex.Matches(emitted, @"FieldOffset\((\d+)\)\]\s+private (byte|ushort|uint|ulong) __bf\d+;")
            .Select(match => (int.Parse(match.Groups[1].Value), match.Groups[2].Value switch
            { "byte" => 1, "ushort" => 2, "uint" => 4, _ => 8 })).ToArray();

    [Theory]
    [InlineData(1, 1, "byte")]
    [InlineData(7, 1, "byte")]
    [InlineData(8, 1, "byte")]
    [InlineData(9, 2, "ushort")]
    [InlineData(16, 2, "ushort")]
    [InlineData(17, 3, "ushort")]
    [InlineData(24, 3, "ushort")]
    public void Ordinary_byte_uses_bitfield_tail_and_accessors_do_not_touch_it(int width, int offset, string storage)
    {
        string emitted = Emit($$"""
            struct Tail { unsigned bits : {{width}}; unsigned char next; };
            int main(void) { return (int)offsetof(struct Tail, next); }
            """);
        emitted.ShouldContain($"public const ulong Value = {offset}UL;");
        emitted.ShouldContain("LayoutKind.Explicit, Size = 4, Pack = 4");
        emitted.ShouldContain($"[System.Runtime.InteropServices.FieldOffset({offset})]\n    public byte next;");
        emitted.ShouldContain($"private {storage} __bf0;");
        if (offset == 3)
        {
            Storage(emitted).ShouldBe(new[] { (0, 2), (2, 1) });
            emitted.ShouldContain("__bf0 = (ushort)");
            emitted.ShouldContain("__bf1 = (byte)");
        }
        else
        {
            emitted.ShouldNotContain("__bytes");
            emitted.ShouldContain($"__bf0 = ({storage})");
        }
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
        emitted.ShouldContain("[System.Runtime.InteropServices.FieldOffset(8)]\n    private byte __bf0;");
        emitted.ShouldContain("[System.Runtime.InteropServices.FieldOffset(16)]\n    public ulong suffix;");
    }

    [Theory]
    [InlineData(8, "byte")]
    [InlineData(16, "ushort")]
    [InlineData(32, "uint")]
    public void Wide_declared_type_can_use_narrow_scalar_storage(int width, string storage)
    {
        var emitted = Emit($$"""
            struct Tail { signed long bits : {{width}}; unsigned char next; };
            int main(void) { return (int)offsetof(struct Tail, next); }
            """);
        emitted.ShouldContain("LayoutKind.Explicit, Size = 8, Pack = 8");
        emitted.ShouldContain($"private {storage} __bf0;");
        emitted.ShouldContain("public long bits");
        emitted.ShouldNotContain("__bytes");
    }

    [Fact]
    public void Shared_fields_and_unnamed_bits_determine_the_backing_width()
    {
        var emitted = Emit("""
            struct Tail { signed a : 3; unsigned : 6; unsigned b : 2; unsigned char next; };
            int main(void) { return (int)offsetof(struct Tail, next); }
            """);
        emitted.ShouldContain("private ushort __bf0;");
        emitted.ShouldNotContain("__bf1");
        emitted.ShouldContain("(__bf0 >> 9)");
        emitted.ShouldNotContain("__bytes");
    }

    [Theory]
    [InlineData("unsigned char prefix; unsigned bits : 7; unsigned char next;", 1, "byte", 0)]
    [InlineData("unsigned short prefix; unsigned a : 1; unsigned bits : 2; unsigned char next;", 2, "byte", 1)]
    [InlineData("unsigned char prefix[3]; unsigned bits : 8; unsigned char next;", 3, "byte", 0)]
    [InlineData("unsigned short prefix; unsigned a : 3; signed bits : 13; unsigned char next;", 2, "ushort", 3)]
    [InlineData("unsigned int prefix; signed long bits : 32; unsigned char next;", 4, "uint", 0)]
    public void Prefix_bytes_are_excluded_by_rebasing_the_narrow_backing_field(string fields, int offset, string storage, int shift)
    {
        var emitted = Emit($$"""
            struct Tail { {{fields}} };
            int main(void) { return (int)offsetof(struct Tail, next); }
            """);
        emitted.ShouldContain($"FieldOffset({offset})]\n    private {storage} __bf0;");
        emitted.ShouldContain($"(__bf0 >> {shift})");
        emitted.ShouldContain($"__bf0 = ({storage})");
        emitted.ShouldNotContain("__bytes");
    }

    [Theory]
    [InlineData("unsigned char prefix; signed bits : 9; unsigned char next;", 1, 1, 2, 1)]
    [InlineData("unsigned char prefix[3]; signed long bits : 16; unsigned char next;", 3, 1, 4, 1)]
    [InlineData("unsigned short prefix; signed long bits : 32; unsigned char next;", 2, 2, 4, 2)]
    [InlineData("unsigned char prefix; signed bits : 19; unsigned char next;", 1, 1, 2, 2)]
    public void Groups_that_need_multiple_integers_use_aligned_storage(
        string fields, int firstOffset, int firstBytes, int secondOffset, int secondBytes)
    {
        var emitted = Emit($$"""
            struct Tail { {{fields}} };
            int main(void) { return (int)offsetof(struct Tail, next); }
            """);
        Storage(emitted).ShouldBe(new[] { (firstOffset, firstBytes), (secondOffset, secondBytes) });
        emitted.ShouldContain("__bf0 =");
        emitted.ShouldContain("__bf1 =");
    }

    [Fact]
    public void Other_units_keep_their_existing_scalar_storage()
    {
        var emitted = Emit("""
            struct Tail { unsigned a : 17; unsigned char next; unsigned b : 1; unsigned int last; };
            int main(void) { return (int)offsetof(struct Tail, next); }
            """);
        Storage(emitted).ShouldBe(new[] { (0, 2), (2, 1), (4, 4) });
        emitted.ShouldContain("__bf2 = (uint)");
    }

    [Fact]
    public void Unnamed_bits_are_preserved_in_shared_storage()
    {
        var emitted = Emit("""
            struct Tail { unsigned bits : 1; unsigned : 16; unsigned char next; };
            int main(void) { return (int)offsetof(struct Tail, next); }
            """);
        // No accessor needs the unnamed third byte. The scalar setter preserves
        // the unnamed bits that share the first two bytes.
        Storage(emitted).ShouldBe(new[] { (0, 2) });
        emitted.ShouldContain("__bf0 & 65534");
    }

    [Theory]
    [InlineData("")]
    [InlineData("unsigned char next;")]
    public void Zero_width_boundaries_keep_independent_groups_in_separate_storage(string tail)
    {
        var emitted = Emit($$"""
            struct Tail { unsigned a : 1; unsigned char : 0; unsigned b : 1; {{tail}} };
            int main(void) { return 0; }
            """);
        Storage(emitted).ShouldBe(new[] { (0, 1), (1, 1) });
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
