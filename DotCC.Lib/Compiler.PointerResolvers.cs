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
            types[key] = text.Replace(target, "DotCcFunctions." + resolver + "()", StringComparison.Ordinal);
            methods.Append("internal unsafe static ").Append(signature.Groups[1].Value).Append(' ').Append(resolver)
                .Append("() => &").Append(EmitHelpers.Id(name)).Append(";\n");
        }
        return (types, methods.ToString());
    }
}
