#nullable enable
using System;
using System.Text;
using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    private void EmitThreadLocalArray(StringBuilder output, GlobalVar global, PinnedArray array)
    {
        string alias = InitializerAlias(global);
        var stores = new StringBuilder();
        if (array.Elems is { } values)
        {
            if (!System.Linq.Enumerable.All(values, ConstantThreadInitializer))
                throw new IrUnsupportedException("_Thread_local array requires a supported constant initializer");
            EmitArrayStores(stores, alias, array.Element, values);
        }
        string count = array.Elems is { } elements ? elements.Count.ToString(System.Globalization.CultureInfo.InvariantCulture)
            : array.Count is { } length ? Expr(length) : "0";
        EmitThreadStorage(output, global, Cs(global.Sym.Type), Cs(array.Element), count, alias, stores.ToString(), record: false);
    }

    private bool ConstantThreadInitializer(CExpr value) => value switch
    {
        StructInit aggregate => System.Linq.Enumerable.All(aggregate.Members, member => ConstantThreadInitializer(member.Value)),
        InlineArrayInit array => System.Linq.Enumerable.All(array.Elems, ConstantThreadInitializer),
        _ => ReadableRecordScalar(value)
    };

    private void EmitThreadLocalAggregate(StringBuilder output, GlobalVar global, StructInit aggregate)
    {
        string alias = InitializerAlias(global);
        var stores = new StringBuilder();
        var previous = _globalAddressBases;
        _globalAddressBases = new(previous, System.Collections.Generic.ReferenceEqualityComparer.Instance) { [global.Sym] = alias };
        try { EmitStaticAggregateStores(stores, "(*" + alias + ")", aggregate.Type, aggregate); }
        finally { _globalAddressBases = previous; }
        string type = Cs(aggregate.Type);
        EmitThreadStorage(output, global, type, type, "1", alias, stores.ToString(), record: true);
    }

    private void EmitThreadStorage(StringBuilder output, GlobalVar global, string type, string element,
        string count, string alias, string stores, bool record)
    {
        var symbol = global.Sym;
        var alignment = ObjectAlignment(symbol);
        var backing = "__dotcc_tls_array_" + symbol.TargetName.TrimStart('@');
        string pointer = record ? type + "*" : type;
        string slot = GlobalStorageReferences.ThreadSlot(symbol.TargetName);
        var helpers = new StringBuilder();
        string method = _instanceMethods ? "/*__dotcc_instance_method__*/ unsafe void " : "static unsafe void ";
        string calls = InitializerHelpers(helpers, stores, pointer, alias,
            "__dotcc_tls_initialize_" + symbol.TargetName.TrimStart('@'), method,
            AddOwnerInitializer, GlobalStorageReferences.OwnerMember);
        // ThreadStatic owns the pinned managed object, not just its address.
        // It stays rooted while the thread can use the C pointer, and can be
        // collected with the thread's storage. Byte backing also supports C
        // pointer elements and explicit over-alignment without generic T* args.
        output.Append("    [ThreadStatic]\n    private static byte[] ").Append(backing).Append(";\n");
        output.Append("    public static unsafe ").Append(record ? "ref " : "").Append(type).Append(' ').Append(symbol.TargetName)
            .Append("\n    {\n        get\n        {\n            var storage = ").Append(backing)
            .Append(";\n            if (storage is null)\n            {\n                storage = global::System.GC.AllocateArray<byte>(checked((")
            .Append(count).Append(") * sizeof(").Append(element).Append(") + ").Append(alignment - 1)
            .Append("), pinned: true);\n                ").Append(backing).Append(" = storage;\n                ")
            .Append(slot).Append(" = (").Append(pointer)
            .Append(")(((nuint)global::System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::System.Runtime.InteropServices.MemoryMarshal.GetArrayDataReference(storage)) + ")
            .Append(alignment - 1).Append(") & ~(nuint)").Append(alignment - 1)
            .Append(");\n                ").Append(pointer).Append(' ').Append(alias).Append(" = ").Append(slot)
            .Append(";\n").Append(calls).Append("            }\n            return ")
            .Append(record ? "ref *" : "").Append(slot).Append(";\n        }\n    }\n").Append(helpers);
    }
}
