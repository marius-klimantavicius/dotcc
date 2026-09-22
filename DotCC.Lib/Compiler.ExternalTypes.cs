using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace DotCC;

public static partial class Compiler
{
    private const string ExternalTypeMarker = "//!!dotcc-obj external-type:";

    private static string SerializeExternalTypes(IReadOnlyList<ExternalTypeOverride>? types)
    {
        var text = new StringBuilder();
        foreach (var type in types ?? Array.Empty<ExternalTypeOverride>())
            text.Append(ExternalTypeMarker).Append(type.Name).Append(' ')
                .Append(type.Layout?.Size.ToString(CultureInfo.InvariantCulture) ?? "-").Append(' ')
                .Append(type.Layout?.Alignment.ToString(CultureInfo.InvariantCulture) ?? "-").Append('\n');
        return text.ToString();
    }

    private static void MergeExternalTypes(string text, string path, Dictionary<string, ExternalTypeLayout?> contracts)
    {
        var local = new HashSet<string>(StringComparer.Ordinal);
        foreach (var line in text.Split('\n').Where(l => l.StartsWith(ExternalTypeMarker, StringComparison.Ordinal)))
        {
            var parts = line[ExternalTypeMarker.Length..].Split(' ');
            if (parts.Length != 3 || !CPreprocessingOptions.Identifier(parts[0]) || !local.Add(parts[0]))
                throw new CompileException("invalid or duplicate external type metadata in object: " + path);
            ExternalTypeLayout? layout = null;
            if (parts[1] != "-" || parts[2] != "-")
            {
                if (!int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var size)
                    || !int.TryParse(parts[2], NumberStyles.None, CultureInfo.InvariantCulture, out var alignment)
                    || size <= 0 || alignment <= 0 || alignment > 128 || (alignment & (alignment - 1)) != 0 || size % alignment != 0)
                    throw new CompileException("invalid external type layout metadata in object: " + path);
                layout = new(size, alignment);
            }
            if (contracts.TryGetValue(parts[0], out var existing) && existing != layout)
                throw new CompileException("conflicting external type layout for '" + parts[0] + "' in object: " + path);
            contracts[parts[0]] = layout;
        }
    }

    private static void ValidateExternalTypeDefinitions(IReadOnlyDictionary<string, ExternalTypeLayout?> contracts, IEnumerable<string> emittedTypes)
    {
        foreach (var name in emittedTypes)
            if (contracts.ContainsKey(name)) throw new CompileException("generated C definition conflicts with external type: " + name);
    }
}
