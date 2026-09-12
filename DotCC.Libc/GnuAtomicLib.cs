#nullable enable
using System;
using System.Numerics;

namespace DotCC.Libc;

/// <summary>GNU atomic memory-order entry points. Order expressions are evaluated
/// once by the caller; all supported orders use the stronger seq-cst Atomic
/// primitives. Dynamic invalid orders fail explicitly before touching storage.</summary>
public static class GnuAtomic
{
    private static void Order(int order, bool load = false, bool store = false)
    {
        if (order is < 0 or > 5 || load && order is 3 or 4 || store && order is 1 or 2 or 4)
            throw new ArgumentOutOfRangeException(nameof(order), "Invalid atomic memory order");
    }

    public static T Load<T>(ref T loc, int order) where T : unmanaged
    { Order(order, load: true); return Atomic.Load(ref loc); }

    public static void Store<T>(ref T loc, T value, int order) where T : unmanaged
    { Order(order, store: true); Atomic.Store(ref loc, value); }

    public static T Exchange<T>(ref T loc, T value, int order) where T : unmanaged
    { Order(order); return Atomic.Exchange(ref loc, value); }

    public static bool CompareExchange<T>(ref T loc, ref T expected, T desired, bool weak,
        int success, int failure) where T : unmanaged
    {
        Order(success);
        Order(failure, load: true);
        bool allowed = failure switch
        {
            0 => true,
            1 => success is 1 or 2 or 4 or 5,
            2 => success is 2 or 4 or 5,
            5 => success == 5,
            _ => false,
        };
        if (!allowed) throw new ArgumentOutOfRangeException(nameof(failure), "Failure order exceeds success order");
        return Atomic.CompareExchange(ref loc, ref expected, desired);
    }

    public static T FetchAdd<T>(ref T loc, T value, int order) where T : unmanaged, INumber<T>
    { Order(order); return Atomic.FetchAdd(ref loc, value); }
    public static T AddFetch<T>(ref T loc, T value, int order) where T : unmanaged, INumber<T>
    { Order(order); return Atomic.AddFetch(ref loc, value); }
    public static T FetchSub<T>(ref T loc, T value, int order) where T : unmanaged, INumber<T>
    { Order(order); return Atomic.FetchSub(ref loc, value); }
    public static T SubFetch<T>(ref T loc, T value, int order) where T : unmanaged, INumber<T>
    { Order(order); return Atomic.SubFetch(ref loc, value); }
    public static T FetchAnd<T>(ref T loc, T value, int order) where T : unmanaged, IBinaryInteger<T>
    { Order(order); return Atomic.FetchAnd(ref loc, value); }
    public static T AndFetch<T>(ref T loc, T value, int order) where T : unmanaged, IBinaryInteger<T>
    { Order(order); return Atomic.AndFetch(ref loc, value); }
    public static T FetchOr<T>(ref T loc, T value, int order) where T : unmanaged, IBinaryInteger<T>
    { Order(order); return Atomic.FetchOr(ref loc, value); }
    public static T OrFetch<T>(ref T loc, T value, int order) where T : unmanaged, IBinaryInteger<T>
    { Order(order); return Atomic.OrFetch(ref loc, value); }
    public static T FetchXor<T>(ref T loc, T value, int order) where T : unmanaged, IBinaryInteger<T>
    { Order(order); return Atomic.FetchXor(ref loc, value); }
    public static T XorFetch<T>(ref T loc, T value, int order) where T : unmanaged, IBinaryInteger<T>
    { Order(order); return Atomic.XorFetch(ref loc, value); }
}
