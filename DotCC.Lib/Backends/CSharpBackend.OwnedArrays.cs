using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    private string _initializerSourceName = "array";

    private void AddOwnerInitializer(string name, string member) =>
        _aggregateFactories.TryAdd(Compiler.OwnerInitializerPrefix + name, member);

    private string OwnedArrayText(PinnedArray array, int requestedAlignment)
    {
        var values = array.Elems!;
        string element = Cs(array.Element), pointer = element + "*";
        string aliasName = _initializerSourceName.TrimStart('@') + "_storage";
        while (!_initializerNames.Add(aliasName)) aliasName += "_";
        string alias = EmitHelpers.Id(aliasName);
        var stores = new StringBuilder();
        // This is a different initialization scope. In particular, a pin/base
        // alias in the enclosing _PyRuntime helper cannot escape into a method.
        // Global references remain symbolic until direct/object final linkage;
        // contextual references resolve against the entering owner and thread.
        var previousBases = _globalAddressBases;
        bool previousReuse = _addressReuseAllowed, previousHoist = _canHoist;
        _globalAddressBases = new(ReferenceEqualityComparer.Instance);
        _addressReuseAllowed = false;
        _canHoist = false;
        try { EmitArrayStores(stores, alias, array.Element, values); }
        finally
        {
            _globalAddressBases = previousBases;
            _addressReuseAllowed = previousReuse;
            _canHoist = previousHoist;
        }
        var zeroed = new PinnedArray(array.Element, null,
            new LitInt(values.Count.ToString(System.Globalization.CultureInfo.InvariantCulture), values.Count) { Type = CType.Int })
            { Type = array.Type };
        string allocation = PinnedArrayText(zeroed, requestedAlignment);
        string stem = "__dotcc_owned_array_" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
            pointer + "\n" + allocation + "\n" + stores)));
        string method = _instanceMethods ? "/*__dotcc_instance_method__*/ unsafe " : "static unsafe ";
        var helpers = new StringBuilder();
        string calls = InitializerHelpers(helpers, stores.ToString(), pointer, alias, stem + "_Fill", method + "void ",
            AddOwnerInitializer, GlobalStorageReferences.OwnerMember);
        string name = stem + "_Create";
        string body = "    " + method + pointer + " " + name + "()\n    {\n        " + pointer + " " + alias
            + " = " + allocation + ";\n" + calls + "        return " + alias + ";\n    }\n";
        _aggregateFactories.TryAdd(Compiler.OwnerInitializerPrefix + name, body);
        // The same recipe can serve equal payloads, but every evaluation still
        // allocates an independently rooted/writable C destination. There is no
        // cached destination, managed payload array, delegate or stack lifetime.
        return GlobalStorageReferences.OwnerMember(name) + "()";
    }
}
