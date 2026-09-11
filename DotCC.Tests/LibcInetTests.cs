using System;
using System.Text;
using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public unsafe class LibcInetTests
{
    [Theory]
    [InlineData(2, "0.0.0.0", "00000000", "0.0.0.0")]
    [InlineData(2, "192.0.2.128", "C0000280", "192.0.2.128")]
    [InlineData(2, "255.255.255.255", "FFFFFFFF", "255.255.255.255")]
    [InlineData(10, "::", "00000000000000000000000000000000", "::")]
    [InlineData(10, "::1", "00000000000000000000000000000001", "::1")]
    [InlineData(10, "2001:0DB8:0000:0000:0000:0000:0000:0001", "20010DB8000000000000000000000001", "2001:db8::1")]
    [InlineData(10, "::ffff:192.0.2.128", "00000000000000000000FFFFC0000280", "::ffff:192.0.2.128")]
    [InlineData(10, "::192.0.2.128", "000000000000000000000000C0000280", "::192.0.2.128")]
    [InlineData(10, "1:0:0:2:0:0:3:4", "00010000000000020000000000030004", "1::2:0:0:3:4")]
    [InlineData(10, "1:2:3:4:5:6:7:8", "00010002000300040005000600070008", "1:2:3:4:5:6:7:8")]
    [InlineData(10, "2001:db8::", "20010DB8000000000000000000000000", "2001:db8::")]
    [InlineData(10, "::ffff:0:192.0.2.128", "0000000000000000FFFF0000C0000280", "::ffff:0:c000:280")]
    [InlineData(10, "::0.0.0.2", "00000000000000000000000000000002", "::2")]
    public void network_order_and_native_presentation_round_trip(int family, string input, string expectedHex, string expectedText)
    {
        byte* packed = stackalloc byte[18]; new Span<byte>(packed, 18).Fill(0xa5);
        byte[] source = Encoding.ASCII.GetBytes(input + '\0');
        errno = EBUSY;
        fixed (byte* text = source) inet_pton(family, text, packed + 1).ShouldBe(1);
        errno.ShouldBe(EBUSY);
        int count = family == 2 ? 4 : 16;
        Convert.ToHexString(new ReadOnlySpan<byte>(packed + 1, count)).ShouldBe(expectedHex);
        packed[0].ShouldBe((byte)0xa5); packed[count + 1].ShouldBe((byte)0xa5);
        byte* output = stackalloc byte[48]; new Span<byte>(output, 48).Fill(0xa5);
        ((nint)inet_ntop(family, packed + 1, output + 1, (uint)expectedText.Length + 1)).ShouldBe((nint)(output + 1));
        errno.ShouldBe(EBUSY);
        Encoding.ASCII.GetString(new ReadOnlySpan<byte>(output + 1, expectedText.Length)).ShouldBe(expectedText);
        output[0].ShouldBe((byte)0xa5); output[expectedText.Length + 1].ShouldBe((byte)0);
        output[expectedText.Length + 2].ShouldBe((byte)0xa5);
        new Span<byte>(output, 48).Fill(0xa5);
        ((nint)inet_ntop(family, packed + 1, output, (uint)expectedText.Length)).ShouldBe(0);
        errno.ShouldBe(ENOSPC);
        new ReadOnlySpan<byte>(output, 48).IndexOfAnyExcept((byte)0xa5).ShouldBe(-1);
    }

    [Theory]
    [InlineData(2, "")]
    [InlineData(2, "123")]
    [InlineData(2, "127.1")]
    [InlineData(2, "1.2.3")]
    [InlineData(2, "1.2.3.4.5")]
    [InlineData(2, "01.2.3.4")]
    [InlineData(2, "1.2.3.004")]
    [InlineData(2, "0x7f.0.0.1")]
    [InlineData(2, "256.0.0.1")]
    [InlineData(2, "1.2.3.4 ")]
    [InlineData(2, " 1.2.3.4")]
    [InlineData(2, "+1.2.3.4")]
    [InlineData(2, "::1")]
    [InlineData(10, "")]
    [InlineData(10, "1.2.3.4")]
    [InlineData(10, "[::1]")]
    [InlineData(10, "[::1]:443")]
    [InlineData(10, "fe80::1%2")]
    [InlineData(10, "fe80::1%eth0")]
    [InlineData(10, " ::1")]
    [InlineData(10, "::1\n")]
    [InlineData(10, ":::1")]
    [InlineData(10, "1::2::3")]
    [InlineData(10, "1:2:3:4:5:6:7")]
    [InlineData(10, "1:2:3:4:5:6:7:8:9")]
    [InlineData(10, "1:2:3:4:5:6:7:00008")]
    [InlineData(10, "::ffff:192.000.2.1")]
    [InlineData(10, "::ffff:192.0.2.256")]
    [InlineData(10, "::ffff:127.1")]
    [InlineData(10, "::ffff:0x7f.0.0.1")]
    public void invalid_presentation_does_not_change_destination_or_errno(int family, string input)
    {
        byte* output = stackalloc byte[18]; new Span<byte>(output, 18).Fill(0xa5);
        errno = EBUSY;
        byte[] source = Encoding.ASCII.GetBytes(input + '\0');
        fixed (byte* text = source) inet_pton(family, text, output + 1).ShouldBe(0);
        errno.ShouldBe(EBUSY);
        new ReadOnlySpan<byte>(output, 18).IndexOfAnyExcept((byte)0xa5).ShouldBe(-1);
    }

    [Fact]
    public void unsupported_family_and_zero_capacity_fail_without_access_or_mutation()
    {
        inet_pton(0, null, null).ShouldBe(-1); errno.ShouldBe(EAFNOSUPPORT);
        ((nint)inet_ntop(0, null, null, 0)).ShouldBe(0); errno.ShouldBe(EAFNOSUPPORT);
        byte* address = stackalloc byte[16]; new Span<byte>(address, 16).Clear();
        byte output = 0xa5;
        ((nint)inet_ntop(10, address, &output, 0)).ShouldBe(0); errno.ShouldBe(ENOSPC); output.ShouldBe((byte)0xa5);
        errno = EBUSY; inet_pton(10, null, address).ShouldBe(0); errno.ShouldBe(EBUSY);
    }
}
