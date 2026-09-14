#nullable enable
using DotCC.Libc;
using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

public unsafe class VsnprintfTests
{
    [Fact]
    public void Zero_capacity_never_touches_destination_and_returns_required_bytes()
    {
        byte sentinel = 99;
        vsnprintf(&sentinel, 0, L("%s-%d\0"u8), new VaList(new VaArg[] { L("abc\0"u8), 42 })).ShouldBe(6);
        sentinel.ShouldBe((byte)99);
        vsnprintf(null, 0, L("%%\0"u8), new VaList([])).ShouldBe(1);
        snprintf(null, 0, L("%d\0"u8)).Arg(123).Done().ShouldBe(3);
    }

    [Fact]
    public void Truncation_stays_inside_destination_and_counts_discarded_bytes()
    {
        byte* buffer = stackalloc byte[8];
        for (int i = 0; i < 8; i++) buffer[i] = 99;
        vsnprintf(buffer + 2, 3, L("%s\0"u8), new VaList(new VaArg[] { L("abcdef\0"u8) })).ShouldBe(6);
        buffer[1].ShouldBe((byte)99);
        buffer[2].ShouldBe((byte)'a');
        buffer[3].ShouldBe((byte)'b');
        buffer[4].ShouldBe((byte)0);
        buffer[5].ShouldBe((byte)99);
    }

    [Fact]
    public void Cursor_copy_and_advanced_cursor_format_without_escaping_borrow()
    {
        byte* buffer = stackalloc byte[64];
        VaArg[] values = [123, 7, L("abcdef\0"u8)];
        var cursor = new VaList(values);
        ((int)cursor.Next()).ShouldBe(123);
        vsnprintf(buffer, 64, L("%d:%.3s\0"u8), cursor).ShouldBe(5);
        global::System.Text.Encoding.Latin1.GetString(buffer, 5).ShouldBe("7:abc");
        ((int)cursor.Next()).ShouldBe(7);
    }
}
