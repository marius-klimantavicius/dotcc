using System;

namespace DotCC;

/// <summary>An ordinary original-name pattern, optionally constrained to an
/// actual bound symbol from object metadata. Constraining an export selects its
/// provider; it does not assert equivalence or merge other implementations.</summary>
internal sealed class InlineExportSelector
{
    private readonly MacroExportSelector _original;
    private readonly MacroExportSelector? _bound;

    internal InlineExportSelector(string pattern)
    {
        if (string.IsNullOrEmpty(pattern))
            throw new CompileException("invalid --export-inline pattern: " + pattern);
        var fields = pattern.Split('=');
        if (fields.Length is < 1 or > 2)
            throw new CompileException("invalid --export-inline pattern: " + pattern);
        _original = new MacroExportSelector(new[] {fields[0]}, "--export-inline");
        if (fields.Length == 2)
            _bound = new MacroExportSelector(new[] {fields[1]}, "--export-inline bound symbol");
    }

    internal bool Matches(string original, string bound) =>
        _original.Matches(original) && (_bound is null || _bound.Matches(bound));
}
