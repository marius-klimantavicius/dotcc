using System;
using System.Collections.Generic;
using System.Linq;
using DotCC.Backends;

namespace DotCC;

/// <summary>Global relocations resolve against definitions, after object linking.
/// An extern declaration need not know the definition's backing-storage strategy.</summary>
internal sealed class GlobalStorageReferences
{
    private const string Marker = "/*__dotcc_global_ref__*/";
    private const string ThreadMarker = "/*__dotcc_thread_slot__*/";
    private const string RuntimeMarker = "/*__dotcc_runtime_global__*/";
    internal static string Emit(string name) => Marker + name;
    internal static string ThreadSlot(string name) => ThreadMarker + name;
    internal static string Runtime(string name) => RuntimeMarker + name;

    internal string GlobalName { get; }
    internal string ThreadName { get; }
    internal string ThreadBackingName { get; }
    private readonly Dictionary<string, string> _references = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _threadSlots = new(StringComparer.Ordinal);
    private readonly string _runtimePath;
    private static readonly IReadOnlyDictionary<string, string> NoRenames = new Dictionary<string, string>();

    internal GlobalStorageReferences(IReadOnlyList<CSharpGlobalSource> globals,
        IEnumerable<string> memberNames, string owner, string scope, string namespacePrefix)
    {
        var reserved = new HashSet<string>(memberNames.Select(n =>
            (n.StartsWith(Compiler.MacroConstantPrefix, StringComparison.Ordinal)
                ? n[Compiler.MacroConstantPrefix.Length..] : n).TrimStart('@')), StringComparer.Ordinal);
        reserved.Add(owner.TrimStart('@'));
        string Allocate(string name)
        {
            while (!reserved.Add(name)) name += "_";
            return name;
        }
        GlobalName = Allocate("Globals");
        ThreadName = Allocate("ThreadGlobals");
        ThreadBackingName = Allocate("__threadGlobals");
        var ownerPath = "global::" + namespacePrefix + owner + ".";
        var specialPath = "global::" + scope + owner.TrimStart('@') + "GlobalsSpecial.";
        _runtimePath = "global::" + scope + "Libc.";
        foreach (var global in globals)
        {
            _references.Add(global.Name, (global.Field.Length != 0 ? ownerPath + GlobalName + "."
                : global.StaticMembers.Length != 0 ? specialPath
                : ownerPath + ThreadName + ".") + global.Name);
            if (global.ThreadField.Length != 0)
                _threadSlots.Add(global.Name, ownerPath + ThreadName + "." + global.Name);
        }
    }

    internal string Rewrite(string source) => BoundSymbolReferences.Rewrite(
        BoundSymbolReferences.Rewrite(BoundSymbolReferences.Rewrite(source, _references, Marker),
            _threadSlots, ThreadMarker), NoRenames, RuntimeMarker, _runtimePath);
}
