using static Managed.Transport.MsQuic;
using System.Runtime.InteropServices;

namespace Managed.Transport.Hosting;

// Compile-linked service test: unexercised operations terminate the process.
// This table and allocator are never compiled into the product host.
public sealed unsafe partial class MsQuicHost : IDisposable
{
    private readonly HashSet<nint> allocations = [];
    internal MSQUIC_HOST_TABLE Table;
    internal MsQuicHost()
    {
        InitializeResources();
        Table.Size = (uint)sizeof(MSQUIC_HOST_TABLE);
        Table.Version = 1;
        Table.Context = ContextPointer;
        Table.ProcessorCount = 1;
        Table.TotalMemory = 1UL << 30;
        RegisterUnexercised(ref Table);
        RegisterCrypto(ref Table);
        Table.CxPlatAlloc = &Allocate;
        Table.CxPlatAllocUninitialized = &AllocateUninitialized;
        Table.CxPlatFree = &Free;
        fixed (MSQUIC_HOST_TABLE* table = &Table)
            if (MsQuic.MsQuicHostInstall(table) != 0) throw new InvalidOperationException("Test host installation failed");
    }
    private static void* Allocate(void* context, ulong length, uint tag)
        => AllocateTest(context, length, true);
    private static void* AllocateUninitialized(void* context, ulong length, uint tag)
        => AllocateTest(context, length, false);
    private static void* AllocateTest(void* context, ulong length, bool zero)
    {
        if (length > int.MaxValue) return null;
        var host = FromContext(context);
        void* pointer = NativeMemory.AlignedAlloc((nuint)Math.Max(16UL, (length + 15) & ~15UL), 16);
        if (pointer == null) return null;
        if (zero) new Span<byte>(pointer, (int)length).Clear();
        try { host.allocations.Add((nint)pointer); return pointer; }
        catch { NativeMemory.AlignedFree(pointer); return null; }
    }
    private static void Free(void* context, void* pointer, uint tag)
    {
        if (pointer == null) return;
        if (!FromContext(context).allocations.Remove((nint)pointer)) FatalInvariant("Unknown test allocation");
        NativeMemory.AlignedFree(pointer);
    }
    public void Dispose()
    {
        if (OutstandingResources != 0 || allocations.Count != 0)
            throw new InvalidOperationException($"Leaked crypto resources: {OutstandingResources}; allocations: {allocations.Count}");
        if (MsQuic.MsQuicHostUninstall() != 0) throw new InvalidOperationException("Test host uninstall failed");
        ReleaseContext();
    }
}
