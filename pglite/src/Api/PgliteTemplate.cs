// Manual .NET adaptation of @electric-sql/pglite src/templating.ts (Apache-2.0).
using System.Globalization;
using System.Text;

namespace Managed.PGlite;

/// <summary>Safe SQL interpolation. Ordinary values, including arrays and null,
/// become parameters. Only explicit SQL fragments contribute SQL text.</summary>
public static class PgliteTemplate
{
    public static PgliteSql Sql(FormattableString template)
    {
        ArgumentNullException.ThrowIfNull(template);
        string format = template.Format;
        RejectNul(format);
        object?[] arguments = template.GetArguments();
        var parts = new List<PgliteSql.Part>();
        var literal = new StringBuilder();
        for (int i = 0; i < format.Length;)
        {
            char c = format[i++];
            if (c == '{')
            {
                if (i < format.Length && format[i] == '{') { literal.Append('{'); i++; continue; }
                int start = i;
                while (i < format.Length && char.IsAsciiDigit(format[i])) i++;
                if (start == i || !int.TryParse(format.AsSpan(start, i - start), NumberStyles.None,
                    CultureInfo.InvariantCulture, out int index)) throw InvalidFormat();
                while (i < format.Length && format[i] == ' ') i++;
                if (i < format.Length && format[i] is ',' or ':')
                    throw new FormatException("SQL interpolation does not support alignment or format specifiers; pass the value as a parameter.");
                if (i >= format.Length || format[i++] != '}' || index >= arguments.Length) throw InvalidFormat();
                if (literal.Length != 0)
                {
                    parts.Add(new(literal.ToString(), null, false));
                    literal.Clear();
                }
                AddValue(parts, arguments[index]);
            }
            else if (c == '}')
            {
                if (i >= format.Length || format[i++] != '}') throw InvalidFormat();
                literal.Append('}');
            }
            else literal.Append(c);
        }
        if (literal.Length != 0) parts.Add(new(literal.ToString(), null, false));
        return new(parts);
    }

    public static PgliteSql Query(FormattableString template) => Sql(template);

    /// <summary>Insert trusted SQL syntax verbatim. Callers are responsible for
    /// the contents; use Sql or Value for user-supplied values.</summary>
    public static PgliteSql Raw(string trustedSql)
    {
        ArgumentNullException.ThrowIfNull(trustedSql);
        RejectNul(trustedSql);
        return new([new(trustedSql, null, false)]);
    }

    /// <summary>Quote each identifier component, doubling embedded quotes.
    /// Identifier("schema", "table") produces "schema"."table".</summary>
    public static PgliteSql Identifier(params string[] components)
    {
        ArgumentNullException.ThrowIfNull(components);
        if (components.Length == 0) throw new ArgumentException("At least one identifier component is required.", nameof(components));
        var text = new StringBuilder();
        foreach (string component in components)
        {
            ArgumentException.ThrowIfNullOrEmpty(component);
            RejectNul(component);
            if (text.Length != 0) text.Append('.');
            text.Append('"').Append(component.Replace("\"", "\"\"", StringComparison.Ordinal)).Append('"');
        }
        return Raw(text.ToString());
    }

    /// <summary>Force a value to be a parameter, even if it is a SQL fragment.</summary>
    public static PgliteSql Value(object? value) => new([new(null, value, true)]);

    public static PgliteSql Join(IEnumerable<object?> values, string separator = ", ") => Join<object?>(values, separator);

    public static PgliteSql Join<T>(IEnumerable<T> values, string separator = ", ") => Join(values, Raw(separator));

    public static PgliteSql Join(IEnumerable<object?> values, PgliteSql separator) => Join<object?>(values, separator);

    public static PgliteSql Join<T>(IEnumerable<T> values, PgliteSql separator)
    {
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(separator);
        var parts = new List<PgliteSql.Part>();
        bool first = true;
        foreach (T value in values)
        {
            if (!first) parts.AddRange(separator.Parts);
            AddValue(parts, value);
            first = false;
        }
        return new(parts);
    }

    private static void AddValue(List<PgliteSql.Part> parts, object? value)
    {
        if (value is PgliteSql sql) parts.AddRange(sql.Parts);
        else parts.Add(new(null, value, true));
    }

    private static FormatException InvalidFormat() => new("Invalid SQL interpolation format.");
    private static void RejectNul(string text)
    {
        if (text.Contains('\0')) throw new ArgumentException("SQL text and identifiers cannot contain NUL.");
    }
}
