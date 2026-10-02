#nullable enable
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Edg.Runtime;

/// <summary>BCL wait/wake boundary for libc++'s aligned 64-bit contention words.
/// Addresses remain owned by the translated caller. A fixed table keeps no
/// address or owner alive and allocates nothing per wait or notification.</summary>
public static unsafe class LibcxxAtomicWait
{
    // libc++'s private synchronization table is shared across engine owners.
    // Its entries are two 64-bit words padded/aligned to 64 bytes. Program
    // storage is pinned; ordinary application atomic objects retain their
    // configured owner/global/TLS/native-heap allocation behavior.
    private static class ContentionStorage
    {
        internal static readonly byte[] Bytes = GC.AllocateArray<byte>(256 * 64 + 63, pinned: true);
        internal static readonly nint Address =
            (nint)(((nuint)Unsafe.AsPointer(ref Bytes[0]) + 63) & ~(nuint)63);
    }
    public static void* Table() => (void*)ContentionStorage.Address;
    private static readonly object[] gates = MakeGates();
    private static object[] MakeGates()
    {
        var result = new object[256];
        for (int i = 0; i < result.Length; ++i) result[i] = new object();
        return result;
    }
    private static object Gate(void* address)
    {
        if (address == null || ((nuint)address & 7) != 0)
            Environment.FailFast("Invalid libc++ atomic contention address");
        nuint bits = (nuint)address;
        return gates[(int)(((bits >> 3) ^ (bits >> 11)) & 255)];
    }

    public static void Wait64(void* address, long expected)
    {
        object gate = Gate(address);
        lock (gate)
        {
            // Read and parking share the notification lock. A notify between
            // the caller's comparison and this read cannot become a lost wake.
            while (Interlocked.Read(ref *(long*)address) == expected)
                Monitor.Wait(gate);
        }
    }

    public static void Wake64(void* address)
    {
        object gate = Gate(address);
        // Different contention addresses can share a bucket. PulseAll is
        // necessary even for notify_one; each waiter rechecks its own word.
        lock (gate) Monitor.PulseAll(gate);
    }
}
