#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    private readonly HashSet<Symbol> _alignedArraySymbols = new();
    private bool HasRequestedTypeAlignment(string name)
    {
        if (!_offsetDocument.Aggregates.TryGetValue(name, out var aggregate)) return false;
        if (aggregate.Alignment != 0 || aggregate.Pack != 0) return true;
        foreach (var field in aggregate.Fields)
        {
            if (field.Alignment != 0) return true;
            var type = field.Type;
            while (type.StartsWith("a:", StringComparison.Ordinal)) type = type[(type.IndexOf(':', 2) + 1)..];
            if (type.StartsWith("p:", StringComparison.Ordinal) &&
                int.TryParse(type[(type.LastIndexOf(':') + 1)..], out var scalarAlignment) && scalarAlignment > 8) return true;
            if (type.StartsWith("n:", StringComparison.Ordinal) && HasRequestedTypeAlignment(type[2..])) return true;
        }
        return false;
    }
    private int TypeAlignment(CType type)
    {
        if (type.Unqualified is CType.Named named && !HasRequestedTypeAlignment(named.Name)) return 8;
        if (type.Unqualified is CType.Array array) return TypeAlignment(array.Element);
        if (type.Unqualified is not (CType.Prim or CType.Pointer or CType.Func or CType.Enum or CType.Array or CType.Named)) return 0;
        return _offsetModel.Type(IrBuilder.LayoutType(type)).Alignment;
    }

    private int ObjectAlignment(Symbol symbol) => Math.Max(symbol.Alignment, TypeAlignment(symbol.Type));
    private bool RequiresAlignedObject(Symbol symbol) => symbol.Alignment > 0 || TypeAlignment(symbol.Type) > 8;
    private static string AlignedName(Symbol symbol) => "__dotcc_aligned_" + symbol.TargetName.TrimStart('@');

    private void EmitAlignedLocal(StringBuilder sb, string pad, Symbol symbol, string init)
    {
        var type = Cs(symbol.Type);
        var name = AlignedName(symbol);
        var alignment = ObjectAlignment(symbol);
        sb.Append(pad).Append("byte* ").Append(name).Append("_bytes = stackalloc byte[sizeof(")
            .Append(type).Append(") + ").Append(alignment - 1).Append("];\n");
        sb.Append(pad).Append(type).Append("* ").Append(name).Append(" = (").Append(type)
            .Append("*)(((nuint)").Append(name).Append("_bytes + ").Append(alignment - 1)
            .Append(") & ~(nuint)").Append(alignment - 1).Append(");\n");
        sb.Append(pad).Append('*').Append(name).Append(" = ").Append(init).Append(";\n");
    }

    private bool EmitAlignedArray(StringBuilder sb, string pad, ArrayDecl array)
    {
        var alignment = Math.Max(array.Sym.Alignment, TypeAlignment(array.Element));
        if (array.Sym.Alignment == 0 && alignment <= 8) return false;
        _alignedArraySymbols.Add(array.Sym);
        var type = Cs(array.Element);
        var name = array.Sym.TargetName;
        var storage = AlignedName(array.Sym);
        var count = array.Inits is { } inits ? inits.Count.ToString(System.Globalization.CultureInfo.InvariantCulture)
            : array.CountExpr is { } expression ? Expr(expression) : "0";
        sb.Append(pad).Append("byte* ").Append(storage).Append("_bytes = stackalloc byte[sizeof(")
            .Append(type).Append(") * (").Append(count).Append(") + ").Append(alignment - 1).Append("];\n");
        sb.Append(pad).Append(type).Append("* ").Append(name).Append(" = (").Append(type)
            .Append("*)(((nuint)").Append(storage).Append("_bytes + ").Append(alignment - 1)
            .Append(") & ~(nuint)").Append(alignment - 1).Append(");\n");
        if (array.Inits is { } values)
            for (var index = 0; index < values.Count; index++)
                sb.Append(pad).Append(name).Append('[').Append(index).Append("] = ")
                    .Append(Coerced(values[index], array.Element)).Append(";\n");
        return true;
    }

    private bool IsSpecialAlignedGlobal(GlobalVar global) =>
        RequiresAlignedObject(global.Sym) && global.Init is not PinnedArray;

    private bool EmitAlignedGlobal(StringBuilder sb, GlobalVar global)
    {
        var symbol = global.Sym;
        var alignment = ObjectAlignment(symbol);
        if (!IsSpecialAlignedGlobal(global)) return false;
        if (symbol.IsThreadLocal)
            throw new IrUnsupportedException("over-aligned thread-local storage requires a per-thread aligned allocation");
        var type = NintStorage(symbol) ? "nint" : Cs(symbol.Type);
        var storageType = symbol.Type.IsPointerLowered ? "nint" : type;
        var init = global.Init is { } value ? Coerced(value, symbol.Type) : "default";
        if (symbol.Type.IsPointerLowered && global.Init is not null) init = "(nint)(" + init + ")";
        sb.Append("    private static unsafe ").Append(type).Append("* ").Append(AlignedName(symbol))
            .Append(" = (").Append(type).Append("*)Libc.GlobalAlignedFrom<").Append(storageType).Append(">(new ").Append(storageType)
            .Append("[]{ ").Append(init).Append(" }, ").Append(alignment).Append(");\n");
        sb.Append("    public static unsafe ref ").Append(type).Append(' ').Append(symbol.TargetName)
            .Append(" => ref *").Append(AlignedName(symbol)).Append(";\n");
        return true;
    }
}
