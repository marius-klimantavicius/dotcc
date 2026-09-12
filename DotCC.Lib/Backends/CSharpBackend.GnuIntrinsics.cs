#nullable enable
using System;
using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    private string? LowerGnuIntrinsicCall(Call call)
    {
        var name = call.Callee;
        var args = call.Args;
        if (name is "__builtin_bswap16" or "__builtin_bswap32" or "__builtin_bswap64")
            return $"System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(unchecked(({Cs(call.Type)})({Expr(args[0])})))";
        if (name == "__sync_synchronize") return "Atomic.ThreadFence()";
        if (!name.StartsWith("__sync_", StringComparison.Ordinal)
            && !name.StartsWith("__atomic_", StringComparison.Ordinal)) return null;
        var element = ((CType.Pointer)args[0].Type.Unqualified).Pointee.Unqualified;
        // Pointer types cannot be generic arguments. Preserve pointer bits in
        // native integer storage; GNU pointer RMW arithmetic is byte arithmetic.
        string storage = element.IsPointerLowered ? "nint" : Cs(element);
        string Location(int index) => $"ref *({storage}*)({Expr(args[index])})";
        string Value(int index) => $"unchecked(({storage})({Expr(args[index])}))";
        string Order(int index) => $"(int)({Expr(args[index])})";
        string result;
        if (name == "__atomic_compare_exchange_n")
            return $"((CBool)GnuAtomic.CompareExchange({Location(0)}, {Location(1)}, {Value(2)}, Cond.B({Expr(args[3])}), {Order(4)}, {Order(5)}))";
        if (name == "__sync_bool_compare_and_swap")
            return $"((CBool)Atomic.CompareExchangeMatches({Location(0)}, {Value(2)}, {Value(1)}))";
        if (name == "__sync_val_compare_and_swap")
            result = $"Atomic.ValueCompareExchange({Location(0)}, {Value(2)}, {Value(1)})";
        else if (name == "__sync_lock_release")
            return $"Atomic.Store({Location(0)}, ({storage})0)";
        else if (name == "__atomic_load_n")
            result = $"GnuAtomic.Load({Location(0)}, {Order(1)})";
        else if (name == "__atomic_store_n")
            return $"GnuAtomic.Store({Location(0)}, {Value(1)}, {Order(2)})";
        else if (name == "__atomic_exchange_n")
            result = $"GnuAtomic.Exchange({Location(0)}, {Value(1)}, {Order(2)})";
        else if (name == "__sync_lock_test_and_set")
            result = $"Atomic.Exchange({Location(0)}, {Value(1)})";
        else if (IrBuilder.GnuAtomicOperation(name) is { } operation)
            result = name.StartsWith("__atomic_", StringComparison.Ordinal)
                ? $"GnuAtomic.{operation}({Location(0)}, {Value(1)}, {Order(2)})"
                : $"Atomic.{operation}({Location(0)}, {Value(1)})";
        else throw new IrUnsupportedException("unsupported GNU atomic builtin: " + name);
        return element.IsPointerLowered ? $"(({Cs(element)})({result}))" : result;
    }
}
