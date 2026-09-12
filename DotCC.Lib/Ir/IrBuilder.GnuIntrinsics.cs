#nullable enable
using System;
using System.Collections.Generic;

namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
    private CExpr? TryBuildGnuIntrinsic(string name, List<CExpr> args)
    {
        if (name is "__builtin_bswap16" or "__builtin_bswap32" or "__builtin_bswap64")
        {
            RequireCount(1);
            if (!args[0].Type.IsArithmetic) throw Bad("requires an arithmetic argument");
            return new Call(name, args) { Type = name == "__builtin_bswap16" ? CType.UShort
                : name == "__builtin_bswap32" ? CType.UInt : CType.ULong };
        }
        if (!name.StartsWith("__sync_", StringComparison.Ordinal)
            && !name.StartsWith("__atomic_", StringComparison.Ordinal)) return null;
        if (name == "__sync_synchronize")
        {
            RequireCount(0);
            return new Call(name, args) { Type = CType.Void };
        }
        bool compare = name is "__sync_val_compare_and_swap" or "__sync_bool_compare_and_swap"
            or "__atomic_compare_exchange_n";
        bool load = name == "__atomic_load_n";
        bool store = name is "__atomic_store_n" or "__sync_lock_release";
        bool exchange = name is "__sync_lock_test_and_set" or "__atomic_exchange_n";
        string? operation = GnuAtomicOperation(name);
        if (!compare && !load && !store && !exchange && operation is null)
            throw Bad("unsupported GNU atomic builtin");
        int count = name == "__atomic_compare_exchange_n" ? 6 : compare ? 3
            : name == "__sync_lock_release" ? 1 : load ? 2
            : name.StartsWith("__atomic_", StringComparison.Ordinal) ? 3 : 2;
        RequireCount(count);
        if (args[0].Type.Unqualified is not CType.Pointer pointer)
            throw Bad("first argument must point to an integer or pointer object");
        var element = pointer.Pointee.Unqualified;
        bool boolean = element == CType.Bool;
        if (!(element is CType.Prim { Integer: true, Bytes: 1 or 2 or 4 or 8 }
            || element.IsPointerLowered))
            throw Bad("requires a 1-, 2-, 4-, or 8-byte integer or pointer object");
        if (boolean && (name.StartsWith("__sync_", StringComparison.Ordinal) || operation is not null))
            throw Bad("does not accept _Bool arithmetic operands");
        if (!load && pointer.Pointee.IsConst) throw Bad("cannot modify a const object");
        if (name == "__atomic_compare_exchange_n")
        {
            if (args[1].Type.Unqualified is not CType.Pointer expected
                || expected.Pointee.Unqualified != element || expected.Pointee.IsConst)
                throw Bad("expected argument must point to a writable object of the same type");
            CheckOrder(4, false, false);
            CheckOrder(5, true, false);
            if (ConstEval(args[4]) is { } success && ConstEval(args[5]) is { } failure
                && !GnuFailureOrderAllowed(success, failure))
                throw Bad("failure memory order is stronger than success memory order");
        }
        else if (name.StartsWith("__atomic_", StringComparison.Ordinal))
            CheckOrder(args.Count - 1, load, store);
        var result = name is "__sync_bool_compare_and_swap" or "__atomic_compare_exchange_n"
            ? CType.Bool : store ? CType.Void : element;
        return new Call(name, args) { Type = result };

        IrUnsupportedException Bad(string message) => new(name + ": " + message);
        void RequireCount(int expected)
        {
            if (args.Count != expected) throw Bad("requires " + expected + " arguments");
        }
        void CheckOrder(int index, bool reading, bool writing)
        {
            if (!args[index].Type.IsInteger) throw Bad("memory order must be an integer");
            if (ConstEval(args[index]) is not { } order) return;
            if (order is < 0 or > 5 || reading && order is 3 or 4 || writing && order is 1 or 2 or 4)
                throw Bad("invalid memory order " + order);
        }
    }

    private static bool GnuFailureOrderAllowed(long success, long failure) => failure switch
    {
        0 => true,
        1 => success is 1 or 2 or 4 or 5,
        2 => success is 2 or 4 or 5,
        5 => success == 5,
        _ => false,
    };

    internal static string? GnuAtomicOperation(string name) => name switch
    {
        "__sync_fetch_and_add" or "__atomic_fetch_add" => "FetchAdd",
        "__sync_add_and_fetch" or "__atomic_add_fetch" => "AddFetch",
        "__sync_fetch_and_sub" or "__atomic_fetch_sub" => "FetchSub",
        "__sync_sub_and_fetch" or "__atomic_sub_fetch" => "SubFetch",
        "__sync_fetch_and_and" or "__atomic_fetch_and" => "FetchAnd",
        "__sync_and_and_fetch" or "__atomic_and_fetch" => "AndFetch",
        "__sync_fetch_and_or" or "__atomic_fetch_or" => "FetchOr",
        "__sync_or_and_fetch" or "__atomic_or_fetch" => "OrFetch",
        "__sync_fetch_and_xor" or "__atomic_fetch_xor" => "FetchXor",
        "__sync_xor_and_fetch" or "__atomic_xor_fetch" => "XorFetch",
        _ => null,
    };
}
