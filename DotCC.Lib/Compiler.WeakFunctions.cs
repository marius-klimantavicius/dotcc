using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DotCC;

public static partial class Compiler
{
    private const string FragWeakFunction = "//!!dotcc-obj weak-function:";
    private const string FragWeakReference = "//!!dotcc-obj weak-reference:";

    // Resolve before processing inline metadata or C# function sections. All
    // call sites and canonical function-pointer fields continue using the same
    // public symbol; only the losing definitions are omitted from the product.
    private static HashSet<(int Object, string Name)> ResolveWeakObjectFunctions(IReadOnlyList<string> paths)
    {
        var definitions = new Dictionary<string, List<(int Object, bool Weak)>>(StringComparer.Ordinal);
        var weakReferences = new HashSet<string>(StringComparer.Ordinal);
        for (int index = 0; index < paths.Count; index++)
        {
            var lines = File.ReadAllLines(paths[index]);
            foreach (var line in lines.Where(line => line.StartsWith(FragWeakReference, StringComparison.Ordinal)))
            {
                var name = line[FragWeakReference.Length..];
                if (name.Length == 0 || name.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not ('_' or '@')))
                    throw new CompileException("invalid weak reference metadata in '" + paths[index] + "'");
                weakReferences.Add(name);
            }
            var weak = new HashSet<string>(StringComparer.Ordinal);
            foreach (var line in lines.Where(line => line.StartsWith(FragWeakFunction, StringComparison.Ordinal)))
            {
                var name = line[FragWeakFunction.Length..];
                if (name.Length == 0 || name.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not ('_' or '@')) || !weak.Add(name))
                    throw new CompileException("invalid weak function metadata in '" + paths[index] + "'");
            }
            var functions = lines.Where(line => line.StartsWith(FragFunction, StringComparison.Ordinal))
                .Select(line => line[FragFunction.Length..]).ToHashSet(StringComparer.Ordinal);
            if (!weak.IsSubsetOf(functions))
                throw new CompileException("weak function metadata has no definition in '" + paths[index] + "'; regenerate objects");
            foreach (var name in functions)
            {
                if (!definitions.TryGetValue(name, out var entries)) definitions.Add(name, entries = new());
                entries.Add((index, weak.Contains(name)));
            }
        }
        var discarded = new HashSet<(int Object, string Name)>();
        foreach (var name in weakReferences)
            if (!definitions.ContainsKey(name))
                throw new CompileException("undefined weak function reference '" + name + "' is not supported");
        foreach (var (name, entries) in definitions)
        {
            if (!entries.Any(entry => entry.Weak)) continue;
            var strong = entries.Where(entry => !entry.Weak).ToArray();
            if (strong.Length > 1)
                throw new CompileException("duplicate strong definition of function '" + name + "' in linked objects");
            int winner = strong.Length == 1 ? strong[0].Object : entries[0].Object;
            foreach (var entry in entries)
                if (entry.Object != winner) discarded.Add((entry.Object, name));
        }
        return discarded;
    }
}
