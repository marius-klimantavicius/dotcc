using System.Runtime.CompilerServices;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    /// <summary>GNU branch prediction hint. Both arguments are evaluated normally;
    /// the result is the first argument converted to C long (LP64).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long __builtin_expect(long value, long expected) => value;
}
