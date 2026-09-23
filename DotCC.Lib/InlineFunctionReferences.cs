using System;
using System.Collections.Generic;
using System.Text;
using DotCC.Ir;

namespace DotCC;

/// <summary>Explicit function relocations emitted from bound symbols. The linker
/// rewrites only these annotated tokens, never arbitrary identifiers or strings.</summary>
internal static class InlineFunctionReferences
{
    private const string Marker = "/*__dotcc_inline_ref__*/";
    internal static string Emit(Symbol symbol, bool annotate) =>
        (annotate && symbol.IsInline && symbol.Storage == Storage.Static ? Marker : "") + symbol.TargetName;

    internal static string Rewrite(string source, IReadOnlyDictionary<string, string> names) =>
        BoundSymbolReferences.Rewrite(
            BoundSymbolReferences.Rewrite(
                BoundSymbolReferences.Rewrite(source, names, Marker),
                name => InstanceReferences.Target + names.GetValueOrDefault(name, name), InstanceReferences.Target),
            name => InstanceReferences.CallbackContext + names.GetValueOrDefault(name, name), InstanceReferences.CallbackContext);
}

/// <summary>Resolve explicit bound-symbol annotations, preserving strings and comments.</summary>
internal static class BoundSymbolReferences
{
    internal static string Rewrite(string source, IReadOnlyDictionary<string, string> names, string marker,
        string fallbackPrefix = "")
        => Rewrite(source, name => names.TryGetValue(name, out var target) ? target : fallbackPrefix + name, marker);

    internal static string Rewrite(string source, Func<string, string> resolve, string marker)
    {
        if (!source.Contains(marker, StringComparison.Ordinal)) return source;
        var result = new StringBuilder(source.Length);
        int i = 0;
        while (i < source.Length)
        {
            int start = i;
            if (source.AsSpan(i).StartsWith(marker, StringComparison.Ordinal))
            {
                i += marker.Length;
                start = i;
                while (i < source.Length && (char.IsAsciiLetterOrDigit(source[i]) || source[i] is '_' or '@')) i++;
                if (start == i) throw new CompileException("invalid bound symbol relocation; regenerate objects");
                var name = source[start..i];
                result.Append(resolve(name));
                continue;
            }
            if (source[i] is '\'' or '"')
            {
                char quote = source[i++];
                bool verbatim = quote == '"' && start > 0 && source[start - 1] == '@';
                while (i < source.Length)
                {
                    char c = source[i++];
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
                int end = source.IndexOf("*/", i + 2, StringComparison.Ordinal);
                i = end < 0 ? source.Length : end + 2;
            }
            else i++;
            result.Append(source, start, i - start);
        }
        return result.ToString();
    }
}
