using System;
using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    private string? LowerC11AtomicOrders(Call call, string operation)
    {
        var args = call.Args;
        string Order(int index) => $"(int)({Expr(args[index])})";
        // Fences always have an order argument; their names have no _explicit suffix.
        if (operation == "atomic_thread_fence") return $"GnuAtomic.ThreadFence({Order(0)})";
        if (operation == "atomic_signal_fence") return $"GnuAtomic.SignalFence({Order(0)})";
        if (!call.Callee.EndsWith("_explicit", StringComparison.Ordinal)
            || args.Count == 0 || args[0].Type.Unqualified is not CType.Pointer pointer)
            return null;

        var element = pointer.Pointee.Unqualified;
        // The existing pointer path owns native-integer storage and pointer stride.
        if (element.IsPointerLowered) return null;
        string type = Cs(element);
        bool eligible = _atomicEligible.Contains(type) || element is CType.Enum;
        string Location(int index) => $"ref *({Expr(args[index])})";
        string Value(int index) => $"({type})({Expr(args[index])})";

        // An order is a real argument, evaluated once before the helper touches
        // storage. The same data primitives and same-width access remain in use.
        return operation switch
        {
            "atomic_load" => $"GnuAtomic.{(eligible ? "Load" : "PlainLoad")}({Location(0)}, {Order(1)})",
            "atomic_store" => $"GnuAtomic.{(eligible ? "Store" : "PlainStore")}({Location(0)}, {Value(1)}, {Order(2)})",
            "atomic_exchange" => $"GnuAtomic.Exchange({Location(0)}, {Value(1)}, {Order(2)})",
            "atomic_compare_exchange_strong" or "atomic_compare_exchange_weak" =>
                $"((CBool)GnuAtomic.CompareExchange({Location(0)}, {Location(1)}, {Value(2)}, {(operation == "atomic_compare_exchange_weak" ? "true" : "false")}, {Order(3)}, {Order(4)}))",
            "atomic_fetch_add" => $"GnuAtomic.FetchAdd({Location(0)}, {Value(1)}, {Order(2)})",
            "atomic_fetch_sub" => $"GnuAtomic.FetchSub({Location(0)}, {Value(1)}, {Order(2)})",
            "atomic_fetch_and" => $"GnuAtomic.FetchAnd({Location(0)}, {Value(1)}, {Order(2)})",
            "atomic_fetch_or" => $"GnuAtomic.FetchOr({Location(0)}, {Value(1)}, {Order(2)})",
            "atomic_fetch_xor" => $"GnuAtomic.FetchXor({Location(0)}, {Value(1)}, {Order(2)})",
            "atomic_flag_test_and_set" => $"((CBool)(GnuAtomic.Exchange({Location(0)}, 1, {Order(1)}) != 0))",
            "atomic_flag_clear" => $"GnuAtomic.Store({Location(0)}, 0, {Order(1)})",
            _ => null,
        };
    }
}
