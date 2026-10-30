using System;
using System.Collections.Generic;
using System.Linq;
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
    internal static string Emit(string name) => Marker + name;
    internal static string ThreadSlot(string name) => ThreadMarker + name;
    internal static string Runtime(string name) => RuntimeMarker + name;
    internal static string OwnerMember(string name) => OwnerMemberMarker + name;
    // Object fragments do not yet know whether their final owner is a static
    // field or a context accessor. Retain explicit bound storage choices.
    private static string AddressRecord(string marker, params string[] fields) => marker
        + "C" + Convert.ToHexString(Encoding.UTF8.GetBytes(string.Join('\0', fields)));
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
    // the original root, evaluation branch and storage owner.
    internal static string RelocateAddressTypes(string source, IReadOnlyDictionary<string, string> names)
    {
        foreach (var (marker, count) in new[] { (AddressDeclarationMarker, 3), (AddressUseMarker, 4),
            (ContextAddressMarker, 2), (InitializerAddressMarker, 3) })
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
    private readonly HashSet<string> _ordinaryAddresses = new(StringComparer.Ordinal);
    private readonly HashSet<string> _reservedNames;
    private readonly Dictionary<string, (string Type, string Name)> _addressMembers = new(StringComparer.Ordinal);
    private static readonly IReadOnlyDictionary<string, string> NoRenames = new Dictionary<string, string>();

    internal GlobalStorageReferences(IReadOnlyList<CSharpGlobalSource> globals,
        IEnumerable<string> memberNames, string owner, string scope, string namespacePrefix, bool instanceMethods = false,
        bool stateContext = false)
    {
        var reserved = _reservedNames = new HashSet<string>(memberNames.Concat(globals.Select(g => g.Name)).Select(n =>
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
            {
                _stableAddresses.Add(global.Name);
                if (global.Field.Length != 0) _ordinaryAddresses.Add(global.Name);
            }
        }
    }

    private string AddressMember(string root, string type)
    {
        if (!_addressMembers.TryGetValue(root, out var member))
        {
            string name = "__dotcc_globalAddress_" + root.TrimStart('@');
            while (!_reservedNames.Add(name)) name += "_";
            _addressMembers.Add(root, member = (type, name));
        }
        else if (member.Type != type)
            throw new CompileException("inconsistent global address storage type: " + root);
        return _ownerPath + member.Name;
    }

    // Call after all source fragments have been rewritten. Ordinary globals are
    // fixed-address fields; special recipes own pinned/unmanaged storage. Bind
    // every pointer before evaluating C initializers, including self/forward
    // relocations and calls into compiler-owned aggregate initializer helpers.
    internal CSharpGlobalOutput Complete(CSharpGlobalOutput globals)
    {
        var members = new StringBuilder();
        var initializers = new StringBuilder();
        foreach (var (root, member) in _addressMembers.OrderBy(pair => !_ordinaryAddresses.Contains(pair.Key))
                     .ThenBy(pair => pair.Key, StringComparer.Ordinal))
        {
            members.Append("    internal static unsafe ").Append(member.Type).Append("* ").Append(member.Name).Append(";\n");
            initializers.Append("        ").Append(member.Name).Append(" = (").Append(member.Type)
                .Append("*)global::System.Runtime.CompilerServices.Unsafe.AsPointer(ref ")
                .Append(_references[root]).Append(");\n");
        }
        return globals with { AddressMembers = members.ToString(), Initializers = initializers.ToString() + globals.Initializers };
    }

    internal string Rewrite(string source, bool eagerAddresses = true)
    {
        source = BoundSymbolReferences.Rewrite(source, record =>
        {
            var fields = AddressFields(record, 3);
            return eagerAddresses && _stableAddresses.Contains(fields[0])
                ? AddressMember(fields[0], fields[1])
                : fields[2];
        }, InitializerAddressMarker);
        source = BoundSymbolReferences.Rewrite(source, record =>
        {
            AddressFields(record, 3);
            return "";
        }, AddressDeclarationMarker);
        source = BoundSymbolReferences.Rewrite(source, record =>
        {
            var fields = AddressFields(record, 4);
            return eagerAddresses && _stableAddresses.Contains(fields[0])
                ? AddressMember(fields[0], fields[1])
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
}
