#nullable enable
namespace DotCC.Libc;

public static unsafe partial class Libc
{
    /// <summary>GNU strchrnul: find the first unsigned-byte match, returning
    /// the terminating NUL when the character is absent. No allocation, locale
    /// dependency, errno mutation, or read beyond the first NUL.</summary>
    public static byte* strchrnul(byte* text, int character)
    {
        byte value = unchecked((byte)character);
        while (*text != 0 && *text != value) text++;
        return text;
    }
}
