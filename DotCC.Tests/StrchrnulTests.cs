using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class StrchrnulTests
{
    [Fact]
    public void Returns_first_match_or_the_original_terminator_without_changing_errno()
    {
        using var owner = new RuntimeContext();
        using var binding = owner.Enter();
        byte* value = stackalloc byte[] { (byte)'a', (byte)'b', (byte)'a', 0, (byte)'z' };
        errno = EIO;
        ((nint)strchrnul(value, 'a')).ShouldBe((nint)value);
        ((nint)strchrnul(value, 'b')).ShouldBe((nint)(value + 1));
        ((nint)strchrnul(value, 'z')).ShouldBe((nint)(value + 3));
        ((nint)strchrnul(value, 0)).ShouldBe((nint)(value + 3));
        ((nint)strchrnul(value + 3, 'a')).ShouldBe((nint)(value + 3));
        errno.ShouldBe(EIO);
        value[3].ShouldBe((byte)0);
        value[4].ShouldBe((byte)'z');
    }

    [Theory]
    [InlineData(128)]
    [InlineData(-128)]
    [InlineData(384)]
    public void Converts_the_search_character_to_an_unsigned_byte(int character)
    {
        byte* value = stackalloc byte[] { 0x7f, 0x80, 0xff, 0 };
        ((nint)strchrnul(value, character)).ShouldBe((nint)(value + 1));
        ((nint)strchrnul(value, -1)).ShouldBe((nint)(value + 2));
        ((nint)strchrnul(value, 256)).ShouldBe((nint)(value + 3));
    }
}
