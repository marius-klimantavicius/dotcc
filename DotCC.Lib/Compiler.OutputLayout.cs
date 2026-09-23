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
        if (emit == EmitMode.Object && (options.NestTypes || options.Runtime != RuntimeProfile.All || UsesInlineOptions(options) || options.LiteralPool || options.StateContext))
            throw new CompileException("--nest-types, --runtime, --deduplicate-inline, --export-inline --literal-pool and --state-context must be set at link time for objects");
        if (options.StateContext && emit != EmitMode.ManagedLib)
            throw new CompileException("--state-context requires managed-library output");
        if (options.StateContext && options.Runtime != RuntimeProfile.C)
            throw new CompileException("--state-context currently requires --runtime=c");
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

    private static Backends.CSharpGlobalOutput RenderGlobals(
        IReadOnlyList<Backends.CSharpGlobalSource> globals, LiteralPool.Output literals, GlobalStorageReferences storage) => new(
        string.Concat(globals.Select(global => storage.Rewrite(literals.Rewrite(global.Field)))),
        string.Concat(globals.Select(global => storage.Rewrite(literals.Rewrite(global.Initializer)))),
        string.Concat(globals.Select(global => storage.Rewrite(literals.Rewrite(global.ThreadField)))),
        string.Concat(globals.Select(global => storage.Rewrite(literals.Rewrite(global.StaticMembers)))),
        storage.GlobalName, storage.ThreadName, storage.ThreadBackingName);

    // The object contract stores one independently keyed cache property per function.
    // Coalesce those records only after linking/definition ownership is resolved.
    private static string RenderTypeDeclarations(IReadOnlyDictionary<string, string> declarations, string owner, bool isPublic, LiteralPool.Output literals,
        NestedTagLayout? tagLayout = null)
    {
        var types = new StringBuilder();
        var pointers = new Dictionary<string, string>(StringComparer.Ordinal);
        var tags = new StringBuilder();
        foreach (var (key, text) in declarations)
        {
            if (key.StartsWith(MacroConstantPrefix, StringComparison.Ordinal)
                || key.StartsWith(LiteralPool.TypeKeyPrefix, StringComparison.Ordinal)) continue;
            if (tagLayout?.TypeNames.ContainsKey(key.TrimStart('@')) == true)
            {
                tags.Append(literals.Rewrite(text));
                continue;
            }
            if (!key.StartsWith(FunctionPointerNames.TypeKeyPrefix, StringComparison.Ordinal)) { types.Append(literals.Rewrite(text)); continue; }
            int start = text.IndexOf('{'), end = text.LastIndexOf('}');
            if (start < 0 || end <= start) throw new CompileException("invalid function-pointer object record; regenerate objects");
            pointers.Add(key[FunctionPointerNames.TypeKeyPrefix.Length..].TrimStart('@'),
                text[(start + 1)..end].Trim('\r', '\n') + "\n");
        }
        if (pointers.Count != 0)
        {
            var layers = new Dictionary<string, int>(StringComparer.Ordinal);
            int Layer(string name)
            {
                if (layers.TryGetValue(name, out int found)) return found;
                return layers[name] = name.StartsWith("get_", StringComparison.Ordinal) && pointers.ContainsKey(name[4..])
                    ? Layer(name[4..]) + 1 : 0;
            }
            int last = pointers.Keys.Max(Layer);
            string? parent = null;
            for (int layer = 0; layer <= last; layer++)
            {
                var name = HelperClass(owner, "FunctionPointers");
                if (layer != last)
                {
                    name = "__DotCc" + name + "Base" + layer;
                    while (declarations.ContainsKey(name) || pointers.ContainsKey(name)) name += "_";
                }
                // C# reserves get_Foo for Foo's accessor. Keep both Foo and
                // get_Foo accessible under their original names by inheriting
                // conflicting static properties from separate abstract layers.
                // The usual, collision-free container remains a static class.
                types.Append(isPublic ? "public " : "internal ")
                    .Append(last == 0 ? "static unsafe class " : "abstract unsafe class ").Append(name);
                if (parent != null) types.Append(" : ").Append(parent);
                types.Append("\n{\n");
                foreach (var (pointer, text) in pointers)
                    if (layers[pointer] == layer) types.Append(text);
                types.Append("}\n");
                parent = name;
            }
        }
        if (tags.Length != 0)
            types.Append(isPublic ? "public " : "internal ").Append("static unsafe class ")
                .Append(tagLayout!.Container).Append("\n{\n").Append(tags).Append("}\n");
        types.Append(literals.Source);
        return types.ToString();
    }
}
