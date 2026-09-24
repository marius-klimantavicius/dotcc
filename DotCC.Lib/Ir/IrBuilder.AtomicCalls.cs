#nullable enable
using System;
using System.Collections.Generic;

namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
    private static CExpr? TryBuildAtomicCall(string name, IReadOnlyList<CExpr> args)
    {
        if (!name.StartsWith("atomic_", StringComparison.Ordinal)) return null;
        var operation = name.EndsWith("_explicit", StringComparison.Ordinal) ? name[..^9] : name;
        CType? result = operation switch
        {
            "atomic_load" or "atomic_exchange" or "atomic_fetch_add" or "atomic_fetch_sub" or
            "atomic_fetch_and" or "atomic_fetch_or" or "atomic_fetch_xor" =>
                args.Count > 0 && args[0].Type.Unqualified is CType.Pointer pointer ? pointer.Pointee.Unqualified : null,
            "atomic_compare_exchange_strong" or "atomic_compare_exchange_weak" or "atomic_flag_test_and_set" => CType.Bool,
            "atomic_store" or "atomic_init" or "atomic_flag_clear" or "atomic_thread_fence" or "atomic_signal_fence" => CType.Void,
            "atomic_is_lock_free" => CType.Int,
            _ => null,
        };
        return result is null ? null : new Call(name, args) { Type = result };
    }
}
