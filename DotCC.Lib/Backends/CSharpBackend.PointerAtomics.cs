using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    private string? LowerPointerAtomicCall(Call call, string operation, CType element)
    {
        var args = call.Args;
        var explicitOrder = call.Callee.EndsWith("_explicit", global::System.StringComparison.Ordinal);
        // Reinterpret the actual pointer slot, including expected's slot, at
        // pointer width. Never instantiate a generic runtime helper with T*.
        string Location(int index) => $"ref *(nint*)({Expr(args[index])})";
        string Value(int index) => $"unchecked((nint)({Expr(args[index])}))";
        string Order(int index) => $"(int)({Expr(args[index])})";
        var helper = explicitOrder ? "GnuAtomic" : "Atomic";
        string Tail(int index) => explicitOrder ? $", {Order(index)}" : "";
        string Result(string text) => $"(({Cs(element)})({text}))";
        switch (operation)
        {
            case "atomic_init": return $"*({Expr(args[0])}) = ({Cs(element)})({Expr(args[1])})";
            case "atomic_load": return Result($"{helper}.Load({Location(0)}{Tail(1)})");
            case "atomic_store": return $"{helper}.Store({Location(0)}, {Value(1)}{Tail(2)})";
            case "atomic_exchange": return Result($"{helper}.Exchange({Location(0)}, {Value(1)}{Tail(2)})");
            case "atomic_compare_exchange_strong":
            case "atomic_compare_exchange_weak":
                var order = explicitOrder
                    ? $", {(operation == "atomic_compare_exchange_weak" ? "true" : "false")}, {Order(3)}, {Order(4)}" : "";
                return $"((CBool){helper}.CompareExchange({Location(0)}, {Location(1)}, {Value(2)}{order}))";
            case "atomic_fetch_add":
            case "atomic_fetch_sub":
                var pointee = element is CType.Pointer p ? Cs(p.Pointee.Unqualified) : "byte";
                var stride = pointee == "void" ? "1" : $"sizeof({pointee})";
                var method = operation == "atomic_fetch_add" ? "FetchAdd" : "FetchSub";
                return Result($"{helper}.{method}({Location(0)}, unchecked({Value(1)} * {stride}){Tail(2)})");
            case "atomic_is_lock_free": return "1";
            default: return null;
        }
    }
}
