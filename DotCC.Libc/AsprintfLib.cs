#nullable enable
using System;
using System.IO;
using System.Text;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    /// <summary>Format into a malloc-owned, NUL-terminated byte buffer. The
    /// caller frees it with free(). Failure returns -1 and clears the output.</summary>
    public static int asprintf(byte** destination, byte* format, params ReadOnlySpan<VaArg> arguments)
        => vasprintf(destination, format, new VaList(arguments));

    public static int vasprintf(byte** destination, byte* format, scoped VaList arguments)
    {
        if (destination == null) { errno = EINVAL; return -1; }
        *destination = null;
        if (format == null) { errno = EINVAL; return -1; }
        byte* allocation = null;
        try
        {
            using var writer = new StringWriter(global::System.Globalization.CultureInfo.InvariantCulture);
            new PrintfBuilder(writer, format, byteOutput: true).Arguments(arguments).Done();
            string text = writer.ToString();
            int size = checked(text.Length + 1);
            allocation = (byte*)malloc(size);
            if (allocation == null) { errno = ENOMEM; return -1; }
            Encoding.Latin1.GetBytes(text.AsSpan(), new Span<byte>(allocation, text.Length));
            allocation[text.Length] = 0;
            *destination = allocation;
            return text.Length;
        }
        catch (OutOfMemoryException) { free(allocation); errno = ENOMEM; return -1; }
        catch (OverflowException) { free(allocation); errno = EOVERFLOW; return -1; }
    }
}
