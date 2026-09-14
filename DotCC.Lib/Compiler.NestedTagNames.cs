using System;
using System.Collections.Generic;
using System.Linq;

namespace DotCC;

public static partial class Compiler
{
    // C tags and ordinary identifiers have separate namespaces. Nested C#
    // types and methods do not. Keep colliding tags in their own nested scope;
    // aliases preserve all backend-emitted type references without changing C
    // function names, source bindings, or object bodies.
    private sealed record NestedTagLayout(string? Container, IReadOnlyDictionary<string, string> TypeNames)
    {
        internal string Aliases => string.Concat(TypeNames.Select(pair =>
            $"using {EmitHelpers.Id(pair.Key)} = global::{pair.Value};\n"));
    }

    private static NestedTagLayout PlanNestedTags(IEnumerable<string> types, IEnumerable<string> definitions,
        string owner, string? namespaceName, bool nested)
    {
        var typeNames = new HashSet<string>(types.Select(name => name.TrimStart('@')), StringComparer.Ordinal);
        var symbols = new HashSet<string>(definitions.Select(name => name.TrimStart('@')), StringComparer.Ordinal);
        var collisions = nested ? typeNames.Intersect(symbols).OrderBy(name => name, StringComparer.Ordinal).ToArray()
                                : Array.Empty<string>();
        if (collisions.Length == 0) return new(null, new Dictionary<string, string>());
        var reserved = new HashSet<string>(typeNames, StringComparer.Ordinal);
        reserved.UnionWith(symbols);
        reserved.Add(owner.TrimStart('@'));
        var container = "__DotCcTags";
        while (reserved.Contains(container) || typeNames.Contains(MacroConstantPrefix + container)) container += "_";
        var scope = NamespacePrefix(namespaceName) + owner + "." + container + ".";
        return new(container, collisions.ToDictionary(name => name, name => scope + EmitHelpers.Id(name), StringComparer.Ordinal));
    }
}
