using System.Runtime.InteropServices;

namespace RoslynObj.CSharp.Runtime;

/// <summary>Exact primitive volatile accesses for the exercised Linux x64 target.</summary>
public static unsafe class TargetVolatileMemory
{
    public static ushort LoadUInt16(void* address)
    { RequireAddress(address, 2); return Volatile.Read(ref *(ushort*)address); }
    public static uint LoadUInt32(void* address)
    { RequireAddress(address, 4); return Volatile.Read(ref *(uint*)address); }
    public static int LoadInt32(void* address)
    { RequireAddress(address, 4); return Volatile.Read(ref *(int*)address); }
    public static double LoadDouble(void* address)
    { RequireAddress(address, 8); return Volatile.Read(ref *(double*)address); }
    public static void StoreUInt16(void* address, ushort value)
    { RequireAddress(address, 2); Volatile.Write(ref *(ushort*)address, value); }
    public static void StoreUInt32(void* address, uint value)
    { RequireAddress(address, 4); Volatile.Write(ref *(uint*)address, value); }
    public static void StoreInt32(void* address, int value)
    { RequireAddress(address, 4); Volatile.Write(ref *(int*)address, value); }
    public static void StoreDouble(void* address, double value)
    { RequireAddress(address, 8); Volatile.Write(ref *(double*)address, value); }

    private static void RequireAddress(void* address, uint width)
    {
        if (!OperatingSystem.IsLinux() || RuntimeInformation.ProcessArchitecture != Architecture.X64)
            throw new PlatformNotSupportedException("The selected volatile memory contract requires Linux x64.");
        if (address == null || (nuint)address % width != 0)
            throw new InvalidOperationException("Volatile primitive storage requires a nonzero naturally aligned address.");
    }
}
