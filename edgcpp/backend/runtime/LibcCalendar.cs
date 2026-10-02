#nullable enable
using System.Runtime.CompilerServices;

namespace Edg.Runtime;

/// <summary>Stable timezone names for translated calendar structures. The UTC
/// name is immutable program metadata, shared across generated engine owners.
/// Local timezone registration/conversion is a separate provider component.</summary>
public static unsafe class LibcCalendar
{
    private static readonly byte[] names = MakeNames();
    private static byte[] MakeNames()
    {
        byte[] result = System.GC.AllocateArray<byte>(5, pinned: true);
        result[1] = (byte)'U'; result[2] = (byte)'T'; result[3] = (byte)'C';
        return result;
    }
    private static sbyte* Empty => (sbyte*)Unsafe.AsPointer(ref names[0]);
    public static sbyte* UtcZone() => Empty + 1;
    public static sbyte* ZoneName(sbyte* name)
    {
        // Match musl's pointer validation: an arbitrary tm_zone pointer is
        // never dereferenced. Additional timezone producers must register their
        // stable names here before local-time coverage can be enabled.
        return name == UtcZone() ? name : Empty;
    }
}
