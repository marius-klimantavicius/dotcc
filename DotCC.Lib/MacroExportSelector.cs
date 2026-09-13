using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DotCC;

/// <summary>Additional object-like macros to export. Patterns match a whole,
/// case-sensitive C identifier; '*' matches any sequence and '?' one character.</summary>
internal sealed class MacroExportSelector
{
    private readonly Regex[] _patterns;

    internal MacroExportSelector(IReadOnlyList<string> patterns)
    {
        _patterns = patterns.Select(pattern =>
        {
            if (string.IsNullOrEmpty(pattern) || pattern.Length > 1024
                || pattern.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not ('_' or '*' or '?')))
                throw new CompileException("invalid --emit-define pattern: " + pattern);
            return new Regex("\\A" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "\\z",
                RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);
        }).ToArray();
    }

    internal bool Matches(string name) => _patterns.Any(pattern => pattern.IsMatch(name));
}
