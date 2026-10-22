// Manual .NET adaptation of @electric-sql/pglite src/templating.ts (Apache-2.0).
using System.Globalization;
using System.Text;

namespace Managed.PGlite;

/// <summary>A SQL template and its separate bound values. Compose with
/// PgliteTemplate so parameter numbers are assigned after nesting and joining.</summary>
public sealed class PgliteSql
{
    internal readonly record struct Part(string? Literal, object? Value, bool IsParameter);
    internal IReadOnlyList<Part> Parts { get; }

    public string Text { get; }
    public IReadOnlyList<object?> Parameters { get; }
    public static PgliteSql Empty { get; } = new([]);

    internal PgliteSql(IEnumerable<Part> parts)
    {
        var owned = parts.ToArray();
        Parts = Array.AsReadOnly(owned);
        var text = new StringBuilder();
        var parameters = new List<object?>();
        foreach (var part in owned)
        {
            if (part.IsParameter)
            {
                parameters.Add(part.Value);
                text.Append('$').Append(parameters.Count.ToString(CultureInfo.InvariantCulture));
            }
            else text.Append(part.Literal);
        }
        Text = text.ToString();
        Parameters = parameters.AsReadOnly();
    }

    public override string ToString() => Text;
}
