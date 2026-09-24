namespace DotCC.Libc;

public static unsafe partial class Libc
{
    /// <summary>POSIX basename: remove trailing slashes and return the final
    /// component within the caller's writable, NUL-terminated byte string.
    /// Null/empty inputs return libc-owned "." storage, which must not be
    /// modified or freed. Backslashes and non-ASCII bytes are ordinary bytes.</summary>
    public static byte* basename(byte* path)
    {
        if (path == null || *path == 0) return LiteralPool.Pointer + LiteralPool.DecimalPoint;

        byte* end = path;
        while (*end != 0) end++;
        while (end > path && end[-1] == (byte)'/') end--;
        if (end == path)
        {
            path[1] = 0;
            return path;
        }

        *end = 0;
        byte* component = end;
        while (component > path && component[-1] != (byte)'/') component--;
        return component;
    }

    /// <summary>POSIX dirname: return the directory prefix in the caller's
    /// writable byte string. A path without a directory, including null/empty,
    /// returns libc-owned "." storage. Exactly two leading slashes are kept
    /// as the root, matching Linux; three or more root slashes become one.
    /// The result aliases the input or static storage; never free it separately.</summary>
    public static byte* dirname(byte* path)
    {
        if (path == null || *path == 0) return LiteralPool.Pointer + LiteralPool.DecimalPoint;

        byte* end = path;
        while (*end != 0) end++;
        while (end > path && end[-1] == (byte)'/') end--;

        byte* separator = end;
        while (separator > path && separator[-1] != (byte)'/') separator--;
        if (separator == path && end != path)
            return LiteralPool.Pointer + LiteralPool.DecimalPoint;

        while (separator > path && separator[-1] == (byte)'/') separator--;
        if (separator == path)
        {
            // path[2] is read only when the first two bytes are slashes.
            int rootLength = path[1] == (byte)'/' && path[2] != (byte)'/' ? 2 : 1;
            path[rootLength] = 0;
        }
        else *separator = 0;
        return path;
    }
}
