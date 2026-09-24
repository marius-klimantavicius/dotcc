#nullable enable
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace DotCC.Libc;

/// <summary>Value-producing C volatile operations. A read/modify/write is not
/// an atomic RMW; each individual access has acquire/release ordering.</summary>
public static unsafe partial class Libc
{
public static class VolatileValue
{
    public static T Load<T>(ref T location) where T : unmanaged
    {
        if (Unsafe.SizeOf<T>() == 1) { byte value = Volatile.Read(ref Unsafe.As<T, byte>(ref location)); return Unsafe.As<byte, T>(ref value); }
        if (Unsafe.SizeOf<T>() == 2) { short value = Volatile.Read(ref Unsafe.As<T, short>(ref location)); return Unsafe.As<short, T>(ref value); }
        if (Unsafe.SizeOf<T>() == 4) { int value = Volatile.Read(ref Unsafe.As<T, int>(ref location)); return Unsafe.As<int, T>(ref value); }
        if (Unsafe.SizeOf<T>() == 8) { long value = Volatile.Read(ref Unsafe.As<T, long>(ref location)); return Unsafe.As<long, T>(ref value); }
        throw new NotSupportedException("Volatile scalar storage must be 1, 2, 4, or 8 bytes.");
    }

    public static T Store<T>(ref T location, T value) where T : unmanaged
    {
        if (Unsafe.SizeOf<T>() == 1) Volatile.Write(ref Unsafe.As<T, byte>(ref location), Unsafe.As<T, byte>(ref value));
        else if (Unsafe.SizeOf<T>() == 2) Volatile.Write(ref Unsafe.As<T, short>(ref location), Unsafe.As<T, short>(ref value));
        else if (Unsafe.SizeOf<T>() == 4) Volatile.Write(ref Unsafe.As<T, int>(ref location), Unsafe.As<T, int>(ref value));
        else if (Unsafe.SizeOf<T>() == 8) Volatile.Write(ref Unsafe.As<T, long>(ref location), Unsafe.As<T, long>(ref value));
        else throw new NotSupportedException("Volatile scalar storage must be 1, 2, 4, or 8 bytes.");
        return value;
    }

    public static T Update<T, TArgument>(ref T location, TArgument argument,
        Func<T, TArgument, T> operation, bool returnOld) where T : unmanaged
    {
        T before = Load(ref location);
        T after = operation(before, argument);
        Store(ref location, after);
        return returnOld ? before : after;
    }
}
}
