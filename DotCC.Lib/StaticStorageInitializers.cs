using System;
using System.Collections.Generic;
using System.Text;

namespace DotCC;

// Kept in the existing per-global initializer fragments so object linking can
// establish every array's final address before applying any C relocations.
internal static class StaticStorageInitializers
{
    internal const string AllocationMarker = "/*__dotcc_static_allocation__*/";

    internal static string Order(IEnumerable<string> initializers)
    {
        var allocations = new StringBuilder();
        var values = new StringBuilder();
        foreach (var initializer in initializers)
            foreach (var line in initializer.Split('\n'))
            {
                if (line.Length == 0) continue;
                if (line.StartsWith(AllocationMarker, StringComparison.Ordinal))
                    allocations.Append(line.AsSpan(AllocationMarker.Length)).Append('\n');
                else values.Append(line).Append('\n');
            }
        return allocations.Append(values).ToString();
    }
}
