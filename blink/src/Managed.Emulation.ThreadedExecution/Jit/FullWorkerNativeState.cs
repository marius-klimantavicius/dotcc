using System.Runtime.InteropServices;

namespace Managed.Emulation.Execution;

// Nonowning translation hints and checked data mappings live with the worker,
// outside resettable code arenas. No allocation occurs at a memory miss.
internal sealed unsafe partial class FullWorkerNativeState : IDisposable
{
    private byte* storage;
    internal FullMemoryTlbEntry* Tlb => (FullMemoryTlbEntry*)storage;
    internal FullNativeJumpEntry* Jumps => (FullNativeJumpEntry*)(storage + FullNativeMemory.TlbCount * FullMemoryTlbEntry.ByteSize);
    internal FullStackMemoryDescriptor* StackMemory => (FullStackMemoryDescriptor*)(storage +
        FullNativeMemory.TlbCount * FullMemoryTlbEntry.ByteSize + FullNativeMemory.JumpCount * FullNativeJumpEntry.ByteSize);
    internal FullX87RegisterFile* X87Pointer => storage == null ? null : (FullX87RegisterFile*)(storage +
        FullNativeMemory.WorkerNativeStorageBytes - FullX87NativeStorage.ByteSize);
    internal FullExtended80WorkFrame* X87WorkPointer => storage == null ? null :
        (FullExtended80WorkFrame*)((byte*)X87Pointer + FullX87NativeStorage.WorkOffset);
    // Only terminal, returned/detached cleanup may erase caller continuation bytes.
    internal void ClearX87Work() { if (storage != null) NativeMemory.Clear(X87WorkPointer, FullExtended80WorkFrame.ByteSize); }
    internal long NativeStorageBytes => storage == null ? 0 : FullNativeMemory.WorkerNativeStorageBytes;
    internal FullWorkerNativeState()
    {
        // Owner has reserved WorkerCharge (including the binding allowance)
        // upfront. Allocate only the aligned physical bytes, once per worker.
        storage = (byte*)NativeMemory.AlignedAlloc((nuint)FullNativeMemory.WorkerNativeStorageBytes, 16);
        if (storage == null) throw new OutOfMemoryException();
        NativeMemory.Clear(storage, (nuint)FullNativeMemory.WorkerNativeStorageBytes);
    }
    public void Dispose() { NativeMemory.AlignedFree(storage); storage = null; }
}
