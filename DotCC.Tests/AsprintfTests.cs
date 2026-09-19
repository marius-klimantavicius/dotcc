using System;
using Shouldly;
using Xunit;
using RuntimeLibc = DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public unsafe class AsprintfTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Allocation_preserves_utf8_bytes_and_uses_matching_free(bool debug)
    {
        bool previous = RuntimeLibc._dbgHeap;
        RuntimeLibc._dbgHeap = debug;
        byte* result = null;
        try
        {
            fixed (byte* format = "ž:%s:%lld:%c\0"u8)
            fixed (byte* text = "猫\0"u8)
            {
                int length = RuntimeLibc.asprintf(&result, format, text, 4294967297L, 255);
                byte[] expected = [.."ž:猫:4294967297:"u8, 255, 0];
                length.ShouldBe(expected.Length - 1);
                new ReadOnlySpan<byte>(result, expected.Length).ToArray().ShouldBe(expected);
            }
        }
        finally { RuntimeLibc.free(result); RuntimeLibc._dbgHeap = previous; }
    }

    [Fact]
    public void Empty_output_still_allocates_a_freeable_terminator()
    {
        byte* result = null;
        fixed (byte* format = "\0"u8)
        {
            RuntimeLibc.asprintf(&result, format).ShouldBe(0);
            try { ((nint)result).ShouldNotBe(0); result[0].ShouldBe((byte)0); }
            finally { RuntimeLibc.free(result); }
        }
    }
}
