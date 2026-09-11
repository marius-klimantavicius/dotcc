using System;
using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public unsafe class LibcMemorySizeTests
{
    [Fact]
    public void wide_memory_overloads_preserve_bytes_and_overlap()
    {
        byte* data = stackalloc byte[12];
        memset(data, 0x141, 12UL);
        for (int i = 0; i < 12; i++) data[i].ShouldBe((byte)'A');
        memcpy(data, L("abcdef\0"u8), 7UL);
        memmove(data + 1, data, 7UL);
        memcmp(data, L("aabcdef\0"u8), 8UL).ShouldBe(0);
        memmove(data, data + 1, 7UL);
        memcmp(data, L("abcdef\0"u8), 7UL).ShouldBe(0);
        ((nint)memchr(data, 'd', 7UL)).ShouldBe((nint)(data + 3));
        ((nint)memchr(data, 'z', 7UL)).ShouldBe(0);
    }
    [Fact]
    public void counts_that_previously_narrowed_to_zero_fail_before_any_access()
    {
        const ulong wide = 0x1_0000_0000;
        Should.Throw<ArgumentNullException>(() => memset(null, 1, wide));
        Should.Throw<ArgumentNullException>(() => memcpy(null, null, wide));
        Should.Throw<ArgumentNullException>(() => memmove(null, null, wide));
        Should.Throw<ArgumentNullException>(() => memcmp(null, null, wide));
        Should.Throw<ArgumentNullException>(() => memchr(null, 1, wide));
        Should.Throw<ArgumentNullException>(() => strncpy(null, null, wide));
    }
    [Fact]
    public void impossible_native_extents_and_wrapping_ranges_leave_destination_unchanged()
    {
        byte* data = stackalloc byte[4];
        data[0] = 0xa5;
        Should.Throw<ArgumentOutOfRangeException>(() => memset(data, 0, ulong.MaxValue));
        Should.Throw<ArgumentOutOfRangeException>(() => memcpy(data, data, ulong.MaxValue));
        Should.Throw<ArgumentOutOfRangeException>(() => memmove(data, data, ulong.MaxValue));
        Should.Throw<ArgumentOutOfRangeException>(() => memcmp(data, data, ulong.MaxValue));
        Should.Throw<ArgumentOutOfRangeException>(() => memchr(data, 0, ulong.MaxValue));
        Should.Throw<ArgumentOutOfRangeException>(() => strncpy(data, data, ulong.MaxValue));
        Should.Throw<ArgumentOutOfRangeException>(() => memset((void*)(nuint.MaxValue - 2), 0, 4UL));
        data[0].ShouldBe((byte)0xa5);
    }
    [Fact]
    public void zero_counts_permit_empty_ranges_and_int_overloads_reject_negative_lengths()
    {
        ((nint)memset(null, 1, 0UL)).ShouldBe(0);
        ((nint)memcpy(null, null, 0UL)).ShouldBe(0);
        ((nint)memmove(null, null, 0UL)).ShouldBe(0);
        memcmp(null, null, 0UL).ShouldBe(0);
        ((nint)memchr(null, 1, 0UL)).ShouldBe(0);
        ((nint)strncpy(null, null, 0UL)).ShouldBe(0);
        Should.Throw<OverflowException>(() => memset(null, 1, -1));
        Should.Throw<OverflowException>(() => memcpy(null, null, -1));
        Should.Throw<OverflowException>(() => memmove(null, null, -1));
        Should.Throw<OverflowException>(() => memcmp(null, null, -1));
        Should.Throw<OverflowException>(() => memchr(null, 1, -1));
        Should.Throw<OverflowException>(() => strncpy(null, null, -1));
        Should.Throw<OverflowException>(() => strncat(null, null, -1));
    }
    [Fact]
    public void huge_string_bounds_still_stop_at_the_real_terminator()
    {
        strncmp(L("same\0"u8), L("same\0"u8), ulong.MaxValue).ShouldBe(0);
        strncmp(L("abc\0"u8), L("abd\0"u8), ulong.MaxValue).ShouldBeLessThan(0);
        strncasecmp(L("AbC\0"u8), L("aBc\0"u8), ulong.MaxValue).ShouldBe(0);
        byte* data = stackalloc byte[8];
        strcpy(data, L("ab\0"u8));
        strncat(data, L("cd\0"u8), ulong.MaxValue);
        strcmp(data, L("abcd\0"u8)).ShouldBe(0);
        strncpy(data, L("hi\0"u8), 6UL);
        memcmp(data, L("hi\0\0\0\0"u8), 6UL).ShouldBe(0);
    }
}
