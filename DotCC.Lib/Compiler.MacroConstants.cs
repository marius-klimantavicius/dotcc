using System;
using System.Collections.Generic;
using System.Linq;

namespace DotCC;

public static partial class Compiler
{
    internal const string MacroConstantPrefix = "macro-constant:";

    private static string RenderMacroFields(IReadOnlyDictionary<string, string>? declarations, string owner, IEnumerable<string> definitions)
    {
        if (declarations == null) return "";
        var reserved = new HashSet<string>(definitions, StringComparer.Ordinal) { owner };
        reserved.UnionWith(declarations.Keys.Where(k => !k.StartsWith(MacroConstantPrefix, StringComparison.Ordinal)));
        reserved.UnionWith(PredefinedTypeNames);
        return string.Concat(declarations.Where(d => d.Key.StartsWith(MacroConstantPrefix, StringComparison.Ordinal)
                && !reserved.Contains(d.Key[MacroConstantPrefix.Length..]))
            .OrderBy(d => d.Key, StringComparer.Ordinal).Select(d => d.Value));
    }
}
