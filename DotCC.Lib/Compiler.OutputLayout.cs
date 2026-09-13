using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotCC;

public static partial class Compiler
{
    private static void ValidateOutputOptions(CSharpOutputOptions? options, EmitMode emit)
    {
        if (options is null) return;
        if (!Enum.IsDefined(options.Runtime)) throw new CompileException("unknown runtime profile");
        if (options.ExportInline != null) _ = new MacroExportSelector(options.ExportInline, "--export-inline");
        if (emit == EmitMode.Object && (options.NestTypes || options.Runtime != RuntimeProfile.All || UsesInlineOptions(options)))
            throw new CompileException("--nest-types, --runtime, --deduplicate-inline and --export-inline must be set at link time for objects");
        if (options.NestTypes && emit is not (EmitMode.ManagedLib or EmitMode.SharedLib))
            throw new CompileException("--nest-types requires managed-library or shared-library output");
    }

    private static bool IncludeZigRuntime(CSharpOutputOptions? options, bool? usesZig)
    {
        if (options?.Runtime == RuntimeProfile.C && usesZig != false)
            throw new CompileException("--runtime=c requires C-only inputs with known language provenance; rebuild older objects or select all/auto");
        return options?.Runtime switch { RuntimeProfile.C => false, RuntimeProfile.Auto => usesZig != false, _ => true };
    }

    private static string HelperClass(string owner, string suffix) => owner.TrimStart('@') + suffix;
    private static string TypeScope(string? namespaceName, string owner, bool nested) =>
        NamespacePrefix(namespaceName) + (nested ? owner + "." : "");

    // The object contract stores one independently keyed cache field per function.
    // Coalesce those records only after linking/definition ownership is resolved.
    private static string RenderTypeDeclarations(IReadOnlyDictionary<string, string> declarations, string owner, bool isPublic)
    {
        var types = new StringBuilder();
        var fields = new StringBuilder();
        foreach (var (key, text) in declarations)
        {
            if (key.StartsWith(MacroConstantPrefix, StringComparison.Ordinal)) continue;
            if (!key.StartsWith(FunctionPointerNames.TypeKeyPrefix, StringComparison.Ordinal)) { types.Append(text); continue; }
            int start = text.IndexOf('{'), end = text.LastIndexOf('}');
            if (start < 0 || end <= start) throw new CompileException("invalid function-pointer object record; regenerate objects");
            fields.Append(text[(start + 1)..end].Trim('\r', '\n')).Append('\n');
        }
        if (fields.Length != 0)
            types.Append(isPublic ? "public " : "internal ").Append("static unsafe class ")
                .Append(HelperClass(owner, "FunctionPointers")).Append("\n{\n").Append(fields).Append("}\n");
        return types.ToString();
    }
}
