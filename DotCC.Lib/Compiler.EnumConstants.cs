using System;
using System.Collections.Generic;
using System.Linq;

namespace DotCC;

public static partial class Compiler
{
    internal const string EnumConstantPrefix = "enum-constant:";
    private const string EnumExportPrefix = "enum-export:";
    private const string EnumConstantsVersion = "//!!dotcc-obj enum-constants:1";

    private static Dictionary<string, string> SelectEnumExports(IReadOnlyDictionary<string, string> declarations,
        CSharpOutputOptions? options, string owner, IEnumerable<string> definitions)
    {
        var result = declarations.Where(d => !d.Key.StartsWith(EnumConstantPrefix, StringComparison.Ordinal)
                && !d.Key.StartsWith(EnumExportPrefix, StringComparison.Ordinal))
            .ToDictionary(d => d.Key, d => d.Value, StringComparer.Ordinal);
        var patterns = (options?.ExportEnum ?? Array.Empty<string>())
            .Concat(declarations.Keys.Where(key => key.StartsWith(EnumExportPrefix, StringComparison.Ordinal))
                .Select(key => key[EnumExportPrefix.Length..])).Distinct(StringComparer.Ordinal).ToArray();
        if (patterns.Length == 0) return result;

        var candidates = declarations.Where(d => d.Key.StartsWith(EnumConstantPrefix, StringComparison.Ordinal))
            .ToDictionary(d => d.Key[EnumConstantPrefix.Length..], d => d.Value, StringComparer.Ordinal);
        var selected = new HashSet<string>(StringComparer.Ordinal);
        foreach (var pattern in patterns)
        {
            var selector = new MacroExportSelector(new[] { pattern }, "--export-enum");
            var matches = candidates.Keys.Where(name => selector.Matches(name.TrimStart('@'))).ToArray();
            if (matches.Length == 0)
                throw new CompileException($"--export-enum pattern '{pattern}' matched no file-scope enumerators");
            selected.UnionWith(matches);
        }
        var reserved = new HashSet<string>(definitions.Concat(result.Keys).Concat(PredefinedTypeNames)
            .Append(owner).Select(name => name.TrimStart('@')), StringComparer.Ordinal);
        reserved.UnionWith(new[] { "Libc", "Cond", "CBool", "VaArg", "VaList", "System",
            "DotCcEntryPoint", "DotCcPointers", "DotCcLiterals", "DotCcExports", "DotCcImports", "DotCcStaticImports",
            "__DotCcTags", HelperClass(owner, "Globals"), HelperClass(owner, "ThreadGlobals"),
            HelperClass(owner, "GlobalsSpecial"), HelperClass(owner, "FunctionPointers"), HelperClass(owner, "Literals") });
        ValidateContextNames(options, selected);
        foreach (var name in selected.OrderBy(name => name, StringComparer.Ordinal))
        {
            if (candidates[name].Length == 0)
                throw new CompileException($"ambiguous --export-enum '{name.TrimStart('@')}': conflicting underlying types or values across translation units");
            if (reserved.Contains(name.TrimStart('@')) || result.ContainsKey(MacroConstantPrefix + name))
                throw new CompileException($"--export-enum '{name.TrimStart('@')}' conflicts with a generated type or member");
            // Reuse constant-field layout only after explicit selection and
            // collision checks; unselected metadata never becomes output code.
            result.Add(MacroConstantPrefix + name, candidates[name]);
        }
        return result;
    }
}
