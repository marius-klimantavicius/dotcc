using System.Globalization;
using System.Text;

namespace DotObj;

internal sealed class Names
{
    private static readonly HashSet<string> ReservedKeywords = new(StringComparer.Ordinal)
    {
        "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const",
        "continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit", "extern",
        "false", "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface",
        "internal", "is", "lock", "long", "namespace", "new", "null", "object", "operator", "out", "override",
        "params", "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short",
        "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof",
        "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while",
        "__arglist", "__makeref", "__reftype", "__refvalue"
    };
    // These words are ordinary binding/member names in this non-async scalar
    // profile, but options can occur in namespace/type declaration contexts.
    private static readonly HashSet<string> ContextualKeywords = new(StringComparer.Ordinal)
    {
        "add", "alias", "allows", "and", "ascending", "args", "async", "await", "by", "descending", "dynamic",
        "equals", "extension", "field", "file", "from", "get", "global", "group", "init", "into", "join", "let",
        "managed", "nameof", "nint", "not", "notnull", "nuint", "on", "or", "orderby", "partial", "record",
        "remove", "required", "scoped", "select", "set", "unmanaged", "value", "var", "when", "where", "with", "yield"
    };
    private readonly HashSet<string> allocated = new(StringComparer.Ordinal);

    public string Allocate(string source, string fallback)
    {
        string name = Identifier(string.IsNullOrEmpty(source) ? fallback : source);
        string candidate = name;
        int suffix = 2;
        while (!allocated.Add(candidate)) candidate = name + "__" + suffix++.ToString(CultureInfo.InvariantCulture);
        return ReservedKeywords.Contains(candidate) ? "@" + candidate : candidate;
    }

    public void Reserve(string name) => allocated.Add(name.TrimStart('@'));

    public static string Identifier(string text)
    {
        var result = new StringBuilder();
        foreach (char character in text)
        {
            if (char.IsAsciiLetterOrDigit(character) || character == '_') result.Append(character);
            else result.Append("_u").Append(((int)character).ToString("x4", CultureInfo.InvariantCulture)).Append('_');
        }
        if (result.Length == 0 || char.IsAsciiDigit(result[0])) result.Insert(0, '_');
        return result.ToString();
    }

    public static string OptionIdentifier(string text)
    {
        if (text.Length == 0 || !(char.IsAsciiLetter(text[0]) || text[0] == '_') ||
            text.Any(c => !char.IsAsciiLetterOrDigit(c) && c != '_'))
            throw new ArgumentException($"Invalid C# identifier option: {text}");
        return ReservedKeywords.Contains(text) || ContextualKeywords.Contains(text) ? "@" + text : text;
    }

    public static string Namespace(string text) => string.Join('.', text.Split('.').Select(OptionIdentifier));
}
