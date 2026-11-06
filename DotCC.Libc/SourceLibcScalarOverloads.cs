namespace DotCC.Libc;

public static unsafe partial class Libc
{
    // Source std.c declares these scalar slots as unsigned32. Retain every
    // bit at the existing runtime services instead of changing arbitrary calls.
    public static long readv(int descriptor, void* vectors, uint count) =>
        readv(descriptor, vectors, count > int.MaxValue ? -1 : (int)count);

    public static int madvise(void* address, ulong length, uint advice) =>
        madvise(address, length, unchecked((int)advice));
}
