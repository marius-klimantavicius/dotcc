using System.Globalization;
using System.Text;

namespace Managed.PGlite;

public sealed unsafe partial class PgliteDatabase
{
    /// <summary>Format actual parameter positions as PostgreSQL-quoted literals,
    /// using the server's described types and quote_nullable. Prefer Query with
    /// parameters for execution. Literals, identifiers and comments are preserved.</summary>
    public string FormatQuery(string sql, IReadOnlyList<object?>? parameters = null,
        PgliteQueryOptions? queryOptions = null)
    {
        RequireQueryEntry();
        ArgumentNullException.ThrowIfNull(sql);
        if (parameters == null || parameters.Count == 0) return sql;
        var description = DescribeQuery(sql, queryOptions?.ParameterTypes);
        if (parameters.Count != description.ParameterTypes.Count)
            throw new ArgumentException("Parameter count does not match the described statement.", nameof(parameters));
        var literals = new string[parameters.Count];
        for (int i = 0; i < literals.Length; i++)
        {
            var result = Query("SELECT quote_nullable($1)", new object?[] { parameters[i] },
                new PgliteQueryOptions { Codecs = queryOptions?.Codecs, ParameterTypes = new uint[] { description.ParameterTypes[i] } }).EnsureSuccess();
            literals[i] = result.ResultSets.Single().Rows.Single()[0]!;
        }
        bool standardStrings = UsesStandardStrings();
        return SubstituteParameters(sql, literals, standardStrings);
    }

    private static string SubstituteParameters(string sql, string[] literals, bool standardStrings)
    {
        var output = new StringBuilder();
        int i = 0;
        while (i < sql.Length)
        {
            int start = i;
            char c = sql[i++];
            if (c == '_' || char.IsLetter(c) || c >= 128)
            {
                while (i < sql.Length && (char.IsLetterOrDigit(sql[i]) || sql[i] is '_' or '$' || sql[i] >= 128)) i++;
            }
            else if (c is '\'' or '"')
            {
                bool escape = c == '\'' && (!standardStrings || start > 0 && (sql[start - 1] is 'E' or 'e') &&
                    (start < 2 || !(char.IsLetterOrDigit(sql[start - 2]) || sql[start - 2] == '_')));
                while (i < sql.Length)
                {
                    char next = sql[i++];
                    if (escape && next == '\\' && i < sql.Length) { i++; continue; }
                    if (next != c) continue;
                    if (i < sql.Length && sql[i] == c) { i++; continue; }
                    break;
                }
            }
            else if (c == '-' && i < sql.Length && sql[i] == '-')
            {
                i++;
                while (i < sql.Length && sql[i] is not ('\r' or '\n')) i++;
            }
            else if (c == '/' && i < sql.Length && sql[i] == '*')
            {
                i++; int depth = 1;
                while (i < sql.Length && depth > 0)
                {
                    if (sql[i] == '/' && i + 1 < sql.Length && sql[i + 1] == '*') { depth++; i += 2; }
                    else if (sql[i] == '*' && i + 1 < sql.Length && sql[i + 1] == '/') { depth--; i += 2; }
                    else i++;
                }
            }
            else if (c == '$')
            {
                if (i < sql.Length && char.IsAsciiDigit(sql[i]))
                {
                    int digits = i;
                    while (i < sql.Length && char.IsAsciiDigit(sql[i])) i++;
                    if (!int.TryParse(sql.AsSpan(digits, i - digits), NumberStyles.None, CultureInfo.InvariantCulture, out int index) ||
                        index < 1 || index > literals.Length) throw new ArgumentException("Invalid SQL parameter index.", nameof(sql));
                    output.Append(literals[index - 1]);
                    continue;
                }
                int tag = i;
                if (tag < sql.Length && (sql[tag] == '_' || char.IsLetter(sql[tag])))
                    while (tag < sql.Length && (sql[tag] == '_' || char.IsLetterOrDigit(sql[tag]))) tag++;
                if (tag < sql.Length && sql[tag] == '$')
                {
                    string delimiter = sql[start..(tag + 1)];
                    int end = sql.IndexOf(delimiter, tag + 1, StringComparison.Ordinal);
                    i = end < 0 ? sql.Length : end + delimiter.Length;
                }
            }
            output.Append(sql, start, i - start);
        }
        return output.ToString();
    }
}
