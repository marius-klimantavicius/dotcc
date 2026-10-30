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
                // Each compiler-owned record is exactly one method. Address
                // relocations now refer to the owner's eager pointer members.
                int start = text.IndexOf('{'), end = text.LastIndexOf('}');
                if (start < 0 || end <= start)
                    throw new CompileException("invalid owned initializer record; regenerate objects");
                return text;
            }));
    }
}
