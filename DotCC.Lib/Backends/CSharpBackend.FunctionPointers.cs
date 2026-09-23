#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    private readonly Dictionary<string, string> _functionPointers = new(StringComparer.Ordinal);

    private readonly HashSet<string> _usedFunctionAddresses = new(StringComparer.Ordinal);

    private string FunctionPointer(Symbol function, bool synthetic = false)
    {
        var name = function.TargetName;
        if (!synthetic) _usedFunctionAddresses.Add(name);
        var signature = Cs(function.Type.Unqualified);
        // The final shell/link binds this alias to the translated class when a
        // definition exists, otherwise Libc. Header provenance does not decide
        // linkage: runtime functions may be declared without including a header.
        var container = FunctionPointerNames.OwnerAlias(name);
        var entry = "__DotCcEntry_" + Convert.ToHexString(System.Text.Encoding.UTF8.GetBytes(name));
        var adapter = "";
        if (_instanceMethods)
        {
            var type = (CType.Func)function.Type.Unqualified;
            if (type.IsNativeCallConv) throw new CompileException("native function addresses are unsupported with --instance-methods");
            var parameters = type.Params.Select((p, i) => Cs(p) + " a" + i).ToList();
            var arguments = type.Params.Select((_, i) => "a" + i).ToList();
            if (type.Variadic) { parameters.Add("global::System.ReadOnlySpan<VaArg> tail"); arguments.Add("tail"); }
            parameters.Insert(0, "DotCcFunctions instance");
            var target = InstanceReferences.Target + name;
            var context = (type.Params.Any(ContainsInstanceCallback) || ContainsInstanceCallback(type.Return)) ? InstanceReferences.CallbackContext + name + " " : "";
            adapter = "    //!!dotcc-instance-adapter\n" + $"    internal static {Cs(type.Return)} {entry}({string.Join(", ", parameters)})\n"
                + "    {\n        using var binding = global::System.Object.ReferenceEquals(Libc.RuntimeContext.Current, instance.__DotCcRuntime) ? null : instance.__DotCcEnter();\n        "
                + (type.Return is CType.VoidType ? "" : "return ") + target + "(" + context
                + string.Join(", ", arguments) + ");\n    }\n";
        }
        // Capture each address only on demand. The per-property cache deliberately
        // uses unsynchronized initialization, with no shared class initializer.
        var declaration = $"{(_publicTypes ? "public" : "internal")} static unsafe partial class DotCcFunctionPointers\n"
            + "{\n"
            + $"    public static {signature} {name}\n"
            + "    {\n"
            + "        get\n"
            + "        {\n"
            + $"            if (field == null) field = &{(_instanceMethods ? "DotCcFunctions." + entry : container + "." + FunctionName(function))};\n"
            + "            return field;\n"
            + "        }\n"
            + "    }\n"
            + adapter + "}\n";
        if (_functionPointers.TryGetValue(name, out var previous) && previous != declaration)
            throw new IrUnsupportedException("conflicting canonical function pointer signatures for '" + name + "'");
        _functionPointers[name] = declaration;
        return (_relocatable ? "DotCcPointers." : "global::" + _pointerClass + ".") + name;
    }

    private bool ContainsInstanceCallback(CType type)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        bool Visit(CType current) => current.Unqualified switch
        {
            CType.Func => true,
            CType.Pointer pointer => Visit(pointer.Pointee),
            CType.Array array => Visit(array.Element),
            CType.Named named => seen.Add(named.Name) && _aggregateDefinitions.TryGetValue(named.Name, out var aggregate)
                && aggregate.Fields.Any(field => Visit(field.Type)),
            _ => false,
        };
        return Visit(type);
    }

    private bool HasInstanceCallbackBoundary(Call call)
        => ContainsInstanceCallback(call.Type) || call.Args.Select((argument, index) =>
            call.ParamTypes != null && index < call.ParamTypes.Count ? call.ParamTypes[index] : argument.Type)
            .Any(ContainsInstanceCallback);

    private void RegisterPublicFunctionPointers(IrBuilder unit)
    {
        // Managed consumers need a stable API even when the C source itself never
        // takes an exported method's address. Variadic pointers include the explicit
        // span tail used by the emitted method's managed calling convention.
        // Macro-generated helpers are callable methods, but do not need an API
        // address unless an actual C designator use requests one via FunctionPointer.
        if (_publicTypes)
            foreach (var function in unit.Functions)
                if (!function.Sym.IsMacroGenerated)
                    FunctionPointer(function.Sym, synthetic: true);
    }
}
