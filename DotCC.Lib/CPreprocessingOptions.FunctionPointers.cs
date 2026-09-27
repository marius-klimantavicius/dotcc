using System;
using System.Collections.Generic;
using System.Linq;

namespace DotCC;

public sealed partial class CPreprocessingOptions
{
    /// <summary>Original function names whose stable addresses must be available to managed consumers.</summary>
    public IReadOnlyList<string> StableFunctionPointers { get; }

    private static IReadOnlyList<string> ValidateStableFunctionPointers(IReadOnlyList<string>? names)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);
        foreach (var name in names ?? Array.Empty<string>())
        {
            if (name is null || !Identifier(name))
                throw new CompileException("stableFunctionPointers name must be a function identifier: " + name);
            result.Add(name);
        }
        return Array.AsReadOnly(result.OrderBy(name => name, StringComparer.Ordinal).ToArray());
    }
}
