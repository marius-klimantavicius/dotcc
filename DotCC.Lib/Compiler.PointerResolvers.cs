using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DotCC;

public static partial class Compiler
{
    // Unresolved C methods may be supplied by an authored partial class, a
    // selected native import, or the embedded runtime. Let C# perform exactly
    // the lexical lookup used by direct calls, inside the translated owner.
    // A C prototype need not be the CLR service's exact signature (notably a
    // header-owned T* versus void*). Take the address of one typed forwarding
    // method per unresolved C symbol, rather than the runtime method group.
    private static (IReadOnlyDictionary<string, string> Types, string Methods) ResolveExternalPointerOwners(
        IReadOnlyDictionary<string, string> declarations, IEnumerable<string> definitions,
        IEnumerable<string> otherNames, bool instanceMethods = false)
    {
        var defined = new HashSet<string>(definitions, StringComparer.Ordinal);
        if (instanceMethods)
        {
            var instanceTypes = new Dictionary<string, string>(StringComparer.Ordinal);
            var adapters = new StringBuilder();
            foreach (var (key, source) in declarations)
            {
                const string marker = "    //!!dotcc-instance-adapter\n";
                int adapter = key.StartsWith(FunctionPointerNames.TypeKeyPrefix, StringComparison.Ordinal)
                    ? source.IndexOf(marker, StringComparison.Ordinal) : -1;
                if (adapter < 0) { instanceTypes.Add(key, source); continue; }
                int end = source.LastIndexOf('}');
                adapters.Append(InstanceReferences.Resolve(source[(adapter + marker.Length)..end], defined, "instance"));
                instanceTypes.Add(key, source[..adapter] + "}\n");
            }
            return (instanceTypes, adapters.ToString());
        }
        var occupied = new HashSet<string>(defined.Concat(otherNames).Concat(declarations.Keys), StringComparer.Ordinal);
        var types = new Dictionary<string, string>(declarations, StringComparer.Ordinal);
        var methods = new StringBuilder();
        foreach (var key in declarations.Keys.Order(StringComparer.Ordinal))
        {
            if (!key.StartsWith(FunctionPointerNames.TypeKeyPrefix, StringComparison.Ordinal)) continue;
            var name = key[FunctionPointerNames.TypeKeyPrefix.Length..];
            if (defined.Contains(name)) continue;
            var text = declarations[key];
            var signature = Regex.Match(text, @"^    public static (delegate\*<[^\r\n]+>) " + Regex.Escape(name) + @"\s*$", RegexOptions.Multiline);
            if (!signature.Success) throw new CompileException("invalid external function-pointer object record; regenerate objects");
            var resolver = FunctionPointerNames.OwnerAlias(name).Replace("Owner", "Resolver", StringComparison.Ordinal);
            while (!occupied.Add(resolver)) resolver += "_";
            var target = "&" + FunctionPointerNames.OwnerAlias(name) + "." + EmitHelpers.Id(name);
            if (!text.Contains(target, StringComparison.Ordinal)) throw new CompileException("invalid function-pointer target record; regenerate objects");
            types[key] = text.Replace(target, "&DotCcFunctions." + resolver, StringComparison.Ordinal);
            methods.Append(ExternalPointerAdapter(signature.Groups[1].Value, resolver, EmitHelpers.Id(name)));
        }
        return (types, methods.ToString());
    }

    private static string ExternalPointerAdapter(string signature, string name, string target)
    {
        // The existing object record carries the fully rendered C ABI. Split
        // only its outer parameter list: callbacks and generic runtime types
        // can themselves contain commas. No source-language type inference is
        // needed here, and aggregate relocations have already been applied.
        var types = new List<string>();
        int start = "delegate*<".Length, depth = 0;
        for (int i = start; i < signature.Length - 1; i++)
        {
            if (signature[i] == '<') depth++;
            else if (signature[i] == '>') depth--;
            else if (signature[i] == ',' && depth == 0)
            {
                types.Add(signature[start..i].Trim());
                start = i + 1;
            }
        }
        types.Add(signature[start..^1].Trim());
        var result = types[^1];
        var parameters = types.Take(types.Count - 1).Select((type, i) =>
            (type.StartsWith("ref ", StringComparison.Ordinal) ? "scoped " : "") + type + " a" + i);
        var arguments = types.Take(types.Count - 1).Select((type, i) =>
            (type.StartsWith("ref ", StringComparison.Ordinal) ? "ref " : "") + "a" + i);
        var call = target + "(" + string.Join(", ", arguments) + ")";
        var expression = result == "void" ? call
            : result.StartsWith("ref ", StringComparison.Ordinal) ? "ref " + call
            : "unchecked((" + result + ")(" + call + "))";
        return "internal unsafe static " + result + " " + name + "(" + string.Join(", ", parameters)
            + ") => " + expression + ";\n";
    }
}
