using System;
using System.Linq;
using System.Text;

namespace DotCC;

public static partial class Compiler
{
    internal const string EnumAliasMarker = "//!!dotcc-enum-alias:";
    internal static string EnumAliasName(string name) => "__DotCcEnum_" + Convert.ToHexString(Encoding.UTF8.GetBytes(name));
    private static string NamespacePrefix(string? name) => name == null ? "" : name + ".";
    private static string NamespaceDeclaration(string? name) => name == null ? "" : "namespace " + name + ";\n";

    private static string? ResolveNamespace(string? name, EmitMode emit)
    {
        if (name == null) return null;
        if (emit == EmitMode.Object) throw new CompileException("--namespace must be set at link time for objects");
        return string.Join(".", name.Split('.').Select(part =>
        {
            var identifier = part.TrimStart('@');
            if (identifier.Length == 0 || part.StartsWith("@@", StringComparison.Ordinal)
                || !(char.IsAsciiLetter(identifier[0]) || identifier[0] == '_')
                || identifier.Any(c => !char.IsAsciiLetterOrDigit(c) && c != '_'))
                throw new CompileException("--namespace must be a dotted sequence of ASCII C# identifiers");
            return part.StartsWith('@') || identifier is "null" or "default" or "var" or "dynamic"
                or "nint" or "nuint" or "record" or "required" or "file" or "scoped"
                ? "@" + identifier : EmitHelpers.Id(identifier);
        }));
    }

    private static string ResolveGeneratedAliases(string aliases, string? namespaceName) =>
        string.Join("\n", aliases.Split('\n').Select(line =>
        {
            if (!line.StartsWith(EnumAliasMarker, StringComparison.Ordinal)) return line;
            var name = line[EnumAliasMarker.Length..];
            return $"using {EnumAliasName(name)} = global::{NamespacePrefix(namespaceName)}{name};";
        }));
}
