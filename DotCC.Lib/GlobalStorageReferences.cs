using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using DotCC.Backends;

namespace DotCC;

/// <summary>Global relocations resolve against definitions, after object linking.
/// An extern declaration need not know the definition's backing-storage strategy.</summary>
internal sealed class GlobalStorageReferences
{
    private const string Marker = "/*__dotcc_global_ref__*/";
    private const string ThreadMarker = "/*__dotcc_thread_slot__*/";
    private const string RuntimeMarker = "/*__dotcc_runtime_global__*/";
    private const string OwnerMemberMarker = "/*__dotcc_owner_member__*/";
    private const string AddressDeclarationMarker = "/*__dotcc_address_declaration__*/";
    private const string AddressUseMarker = "/*__dotcc_address_use__*/";
    private const string ContextAddressMarker = "/*__dotcc_context_address__*/";
    private const string InitializerAddressMarker = "/*__dotcc_initializer_address__*/";
    private const string BoundInitializerAddressMarker = "/*__dotcc_bound_initializer_address__*/";
    internal static string Emit(string name) => Marker + name;
    internal static string ThreadSlot(string name) => ThreadMarker + name;
    internal static string Runtime(string name) => RuntimeMarker + name;
    internal static string OwnerMember(string name) => OwnerMemberMarker + name;
    // Object fragments do not yet know whether their final owner is a static
    // field or a context accessor. Retain explicit bound storage choices.
    private static string AddressRecord(string marker, params string[] fields) => marker
        + "C" + Convert.ToHexString(Encoding.UTF8.GetBytes(string.Join('\0', fields)));
    internal static string AddressDeclaration(string root, string type, string local) =>
        AddressRecord(AddressDeclarationMarker, root, type, local);
    internal static string AddressUse(string root, string type, string local, string contextual) =>
        AddressRecord(AddressUseMarker, root, type, local, contextual);
    internal static string ContextAddress(string root, string declaration) =>
        AddressRecord(ContextAddressMarker, root, declaration);
    internal static string InitializerAddress(string root, string type, string contextual) =>
        AddressRecord(InitializerAddressMarker, root, type, contextual);
    private static string[] AddressFields(string record, int count)
    {
        try
        {
            var fields = Encoding.UTF8.GetString(Convert.FromHexString(record[1..])).Split('\0');
            if (record[0] == 'C' && fields.Length == count) return fields;
        }
        catch (Exception error) when (error is FormatException or ArgumentException) { }
        throw new CompileException("invalid global address relocation; regenerate objects");
    }

    // Type spellings also occur inside the encoded address records. Resolve
    // those fields before storage selection/materialization, while preserving
    // the original root, evaluation branch, owner and tracked-ref mechanism.
    internal static string RelocateAddressTypes(string source, IReadOnlyDictionary<string, string> names)
    {
        foreach (var (marker, count) in new[] { (AddressDeclarationMarker, 3), (AddressUseMarker, 4),
            (ContextAddressMarker, 2), (InitializerAddressMarker, 3), (BoundInitializerAddressMarker, 3) })
            source = BoundSymbolReferences.Rewrite(source, record =>
            {
                var fields = AddressFields(record, count);
                for (var index = 1; index < fields.Length; index++)
                    fields[index] = AnonymousTypedefNames.Rewrite(fields[index], names);
                return AddressRecord(marker, fields);
            }, marker);
        return source;
    }

    internal string GlobalName { get; }
    internal string ThreadName { get; }
    internal string ThreadBackingName { get; }
    private readonly Dictionary<string, string> _references = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _threadSlots = new(StringComparer.Ordinal);
    private readonly string _runtimePath;
    private readonly string _ownerPath;
    private readonly HashSet<string> _stableAddresses = new(StringComparer.Ordinal);
    private static readonly IReadOnlyDictionary<string, string> NoRenames = new Dictionary<string, string>();

    internal GlobalStorageReferences(IReadOnlyList<CSharpGlobalSource> globals,
        IEnumerable<string> memberNames, string owner, string scope, string namespacePrefix, bool instanceMethods = false,
        bool stateContext = false)
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
        var ownerPath = instanceMethods ? "this." : "global::" + namespacePrefix + owner + ".";
        _ownerPath = ownerPath;
        var specialPath = instanceMethods ? "this." : "global::" + scope + owner.TrimStart('@') + "GlobalsSpecial.";
        _runtimePath = "global::" + scope + "Libc.";
        foreach (var global in globals)
        {
            _references.Add(global.Name, (global.Field.Length != 0 ? ownerPath + GlobalName + "."
                : global.StaticMembers.Length != 0 ? specialPath
                : ownerPath + ThreadName + ".") + global.Name);
            if (global.ThreadField.Length != 0)
                _threadSlots.Add(global.Name, ownerPath + ThreadName + "." + global.Name);
            // Value fields have unmanaged CLR static storage. Special aligned
            // recipes own pinned/unmanaged storage for the same lifetime.
            // Context/instance/TLS and unresolved external owners are excluded.
            if (!instanceMethods && !stateContext && global.ThreadField.Length == 0
                && (global.Field.Length != 0 || global.StaticMembers.Length != 0))
                _stableAddresses.Add(global.Name);
        }
    }

    internal string Rewrite(string source)
    {
        source = BoundSymbolReferences.Rewrite(source, record =>
        {
            var fields = AddressFields(record, 3);
            return _stableAddresses.Contains(fields[0])
                ? AddressRecord(BoundInitializerAddressMarker, fields[0], fields[1], _references[fields[0]])
                : fields[2];
        }, InitializerAddressMarker);
        source = BoundSymbolReferences.Rewrite(source, record =>
        {
            var fields = AddressFields(record, 3);
            if (!_stableAddresses.Contains(fields[0])) return "";
            string type = fields[1], local = fields[2], value = _references[fields[0]];
            // Null initialization dominates every C label. Acquisition remains
            // lazy at an original use, including inside a conditional branch.
            return GlobalAddressCache.Local(type, local, value);
        }, AddressDeclarationMarker);
        source = BoundSymbolReferences.Rewrite(source, record =>
        {
            var fields = AddressFields(record, 4);
            string type = fields[1], local = fields[2];
            return _stableAddresses.Contains(fields[0])
                ? GlobalAddressCache.Use(type, local)
                : fields[3];
        }, AddressUseMarker);
        source = BoundSymbolReferences.Rewrite(source, record =>
        {
            var fields = AddressFields(record, 2);
            return _stableAddresses.Contains(fields[0]) ? "" : fields[1];
        }, ContextAddressMarker);
        source = BoundSymbolReferences.Rewrite(source, NoRenames, OwnerMemberMarker, _ownerPath);
        return BoundSymbolReferences.Rewrite(
            BoundSymbolReferences.Rewrite(BoundSymbolReferences.Rewrite(source, _references, Marker),
                _threadSlots, ThreadMarker), NoRenames, RuntimeMarker, _runtimePath);
    }

    // Called only after C025 has selected the actual initializer helper body.
    // Coalesce across global declaration fragments, never across helper bounds.
    internal static string MaterializeInitializerAddresses(string source)
    {
        var counts = new Dictionary<string, int>(StringComparer.Ordinal);
        BoundSymbolReferences.Rewrite(source, record =>
        {
            counts[record] = counts.GetValueOrDefault(record) + 1;
            return record;
        }, BoundInitializerAddressMarker);
        if (counts.Count == 0) return source;
        var locals = new Dictionary<string, string>(StringComparer.Ordinal);
        var declarations = new StringBuilder();
        foreach (var pair in counts.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            if (pair.Value < 2) continue;
            var fields = AddressFields(pair.Key, 3);
            string name = "__dotcc_initializerAddress_" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(pair.Key)));
            locals.Add(pair.Key, name);
            declarations.Append(GlobalAddressCache.Local(fields[1], name, fields[2]));
        }
        var body = BoundSymbolReferences.Rewrite(source, record =>
        {
            var fields = AddressFields(record, 3);
            return locals.TryGetValue(record, out var local) ? GlobalAddressCache.Use(fields[1], local)
                : "((" + fields[1] + "*)global::System.Runtime.CompilerServices.Unsafe.AsPointer(ref " + fields[2] + "))";
        }, BoundInitializerAddressMarker);
        return declarations.Append(body).ToString();
    }
}
