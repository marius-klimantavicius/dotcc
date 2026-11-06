#nullable enable
using System.Globalization;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    // The physical sequence carrier alone cannot distinguish []u21 from
    // []u32. Key only witnessed element evidence; unknown remains unknown.
    private static string GenericElementLocationMangle(ZigResultLocation? element)
    {
        if (element is null) return "";
        var child = GenericElementLocationMangle(element.Element);
        if (element.IntegerBits is null && element.Sentinel is null && child.Length == 0) return "";
        return "_e" + (element.IntegerBits?.ToString(CultureInfo.InvariantCulture) ?? "x")
            + SentinelMangle(element.Sentinel) + child;
    }
}
