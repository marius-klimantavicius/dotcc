using System;
using System.Collections.Generic;
using System.Linq;

namespace DotCC;

public static partial class Compiler
{
    internal const string OwnerInitializerPrefix = "owner-initializer:";

    private static string RenderOwnerInitializers(IReadOnlyDictionary<string, string> declarations,
        LiteralPool.Output literals, GlobalStorageReferences storage, IEnumerable<string> definitions, bool instance)
    {
        var defined = definitions.ToHashSet(StringComparer.Ordinal);
        return string.Concat(declarations.Where(pair => pair.Key.StartsWith(OwnerInitializerPrefix, StringComparison.Ordinal))
            .OrderBy(pair => pair.Key, StringComparer.Ordinal).Select(pair =>
            {
                var text = storage.Rewrite(literals.Rewrite(instance
                    ? InstanceReferences.Resolve(pair.Value, defined, "this") : pair.Value));
                // Each compiler-owned record is exactly one method. Materialize
                // lazy address reuse inside that body, after final storage has
                // been selected; enclosing pins/locals cannot cross this scope.
                int start = text.IndexOf('{'), end = text.LastIndexOf('}');
                if (start < 0 || end <= start)
                    throw new CompileException("invalid owned initializer record; regenerate objects");
                return text[..(start + 1)]
                    + GlobalStorageReferences.MaterializeInitializerAddresses(text[(start + 1)..end]) + text[end..];
            }));
    }
}
