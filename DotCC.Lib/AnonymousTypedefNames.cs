using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotCC;

// Additive metadata in the existing C# object format. Semantic identity stays
// in aggregate signatures and inline proofs; aliases are output names only.
internal static class AnonymousTypedefNames
{
    internal const string TypePrefix = "__DotCcTypedef_";
    internal const string MetadataPrefix = "//!!dotcc-obj anonymous-typedef:";

    internal static string Serialize(IReadOnlyDictionary<string, string> aliases) => string.Concat(
        aliases.OrderBy(p => p.Key, StringComparer.Ordinal).Select(p => MetadataPrefix + p.Key + " "
            + Convert.ToBase64String(Encoding.UTF8.GetBytes(p.Value)) + "\n"));

    internal static void Read(string source, Dictionary<string, string> aliases)
    {
        foreach (var line in source.Split('\n').Where(l => l.StartsWith(MetadataPrefix, StringComparison.Ordinal)))
        {
            var parts = line[MetadataPrefix.Length..].Split(' ');
            if (parts.Length != 2 || !parts[0].StartsWith(TypePrefix, StringComparison.Ordinal)
                || parts[0].Length != TypePrefix.Length + 64 || !parts[0][TypePrefix.Length..].All(Uri.IsHexDigit))
                throw new CompileException("invalid anonymous typedef metadata; regenerate objects");
            string alias;
            try { alias = Encoding.UTF8.GetString(Convert.FromBase64String(parts[1])); }
            catch (FormatException) { throw new CompileException("invalid anonymous typedef alias; regenerate objects"); }
            if (alias.Length == 0 || !alias.All(c => char.IsAsciiLetterOrDigit(c) || c == '_'))
                throw new CompileException("invalid anonymous typedef alias; regenerate objects");
            if (aliases.TryGetValue(parts[0], out var previous) && previous != alias)
                throw new CompileException("conflicting anonymous typedef aliases for '" + parts[0] + "'");
            aliases[parts[0]] = alias;
        }
    }

    internal static IReadOnlyDictionary<string, string> Plan(IReadOnlyDictionary<string, string> aliases,
        IEnumerable<string> typeNames, IEnumerable<string> definitions)
    {
        var reserved = typeNames.Concat(definitions).Select(n => n.TrimStart('@')).ToHashSet(StringComparer.Ordinal);
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var group in aliases.Where(p => reserved.Contains(p.Key)).GroupBy(p => p.Value, StringComparer.Ordinal))
        {
            // Neither declaration order nor link order selects a public winner
            // when different C types have the same ordinary alias.
            if (group.Count() == 1 && !reserved.Contains(group.Key))
                result.Add(group.Single().Key, EmitHelpers.Id(group.Key));
        }
        return result;
    }

    internal static IReadOnlyDictionary<string, string> Resolve(IReadOnlyDictionary<string, string> files,
        IReadOnlyDictionary<string, string> aliases, IEnumerable<string> types, IEnumerable<string> definitions)
    {
        var names = Plan(aliases, types, definitions);
        return names.Count == 0 ? files : files.ToDictionary(p => p.Key, p => Rewrite(p.Value, names), StringComparer.Ordinal);
    }

    // Resolve only compiler-generated identity tokens. Never replace substrings,
    // member names, literal bytes, quoted text or comments. This runs after all
    // semantic merging/layout/inline checks, on raw emission as well as linking.
    internal static string Rewrite(string source, IReadOnlyDictionary<string, string> names)
    {
        var result = new StringBuilder(source.Length);
        var i = 0;
        while (i < source.Length)
        {
            var start = i;
            if (source[i] is '\'' or '"')
            {
                var quote = source[i++];
                var verbatim = quote == '"' && start > 0 && source[start - 1] == '@';
                while (i < source.Length)
                {
                    var c = source[i++];
                    if (!verbatim && c == '\\' && i < source.Length) i++;
                    else if (c == quote)
                    {
                        if (verbatim && i < source.Length && source[i] == quote) i++;
                        else break;
                    }
                }
            }
            else if (source.AsSpan(i).StartsWith("//", StringComparison.Ordinal))
            { while (i < source.Length && source[i] != '\n') i++; }
            else if (source.AsSpan(i).StartsWith("/*", StringComparison.Ordinal))
            {
                var end = source.IndexOf("*/", i + 2, StringComparison.Ordinal);
                i = end < 0 ? source.Length : end + 2;
            }
            else if (char.IsAsciiLetter(source[i]) || source[i] is '_' or '@')
            {
                while (i < source.Length && (char.IsAsciiLetterOrDigit(source[i]) || source[i] is '_' or '@')) i++;
                var token = source[start..i];
                result.Append(names.GetValueOrDefault(token, token));
                continue;
            }
            else i++;
            result.Append(source, start, i - start);
        }
        return result.ToString();
    }
}
