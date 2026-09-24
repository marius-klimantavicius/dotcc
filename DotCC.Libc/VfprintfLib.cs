#nullable enable
using System;
using System.IO;
using System.Text;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    /// <summary>Write a fixed va_list cursor without repacking it into varargs.
    /// File/socket output preserves formatted bytes, including embedded NULs.</summary>
    public static int vfprintf(FILE* stream, byte* format, scoped VaList arguments)
    {
        var slot = Slot(stream);
        if (slot == null || slot.Kind == FileSlot.K.In || slot.Stream is { CanWrite: false })
        {
            errno = EBADF;
            if (slot != null) slot.Err = true;
            return -1;
        }
        byte* buffer = null;
        try
        {
            // Format once: evaluating %n twice during sizing would repeat its
            // observable write. vasprintf owns the byte-oriented cursor formatter.
            int count = vasprintf(&buffer, format, arguments);
            if (count < 0) return -1;
            if (slot.Kind is FileSlot.K.Out or FileSlot.K.Err)
            {
                // Console redirection uses TextWriter, while the return value is
                // still the number of C bytes, not the decoded character count.
                var writer = slot.Kind == FileSlot.K.Err ? Console.Error : Console.Out;
                writer.Write(Encoding.UTF8.GetString(buffer, count));
            }
            else
            {
                for (int i = 0; i < count; i++)
                    if (!WriteByteSlot(slot, buffer[i])) { slot.Err = true; return -1; }
            }
            return count;
        }
        catch (IOException) { slot.Err = true; errno = EIO; return -1; }
        catch (ObjectDisposedException) { slot.Err = true; errno = EBADF; return -1; }
        catch (UnauthorizedAccessException) { slot.Err = true; errno = EACCES; return -1; }
        finally { free(buffer); }
    }

    public static int vprintf(byte* format, scoped VaList arguments)
        => vfprintf(stdout, format, arguments);

    public static int vsprintf(byte* destination, byte* format, scoped VaList arguments)
        => new SprintfBuilder(destination, format, capacity: -1).Arguments(arguments).Done();
}
