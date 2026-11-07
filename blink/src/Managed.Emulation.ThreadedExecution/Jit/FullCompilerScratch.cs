using System.Runtime.InteropServices;

namespace Managed.Emulation.Execution;

// Translation is serialized by Authority.Gate. Extra slots allow bounded
// recursive translation through diagnostic hooks without overwriting a caller.
// Every byte is reserved/charged at owner setup; no per-translation arrays.
internal sealed unsafe class FullCompilerScratch : IDisposable
{
    internal const int MaximumInstructions = 512;
    internal const int SlotCount = 4;
    internal const int MaximumCodeBytes = 65536;
    private static readonly int PlanBytes = MaximumInstructions * sizeof(FullTranslationJit.Plan);
    private static readonly int IrBytes = MaximumInstructions * sizeof(FullBlockInstruction);
    private static readonly int Stride = (PlanBytes + IrBytes + MaximumCodeBytes + 63) & ~63;
    private byte* memory;
    private readonly bool[] used;
    internal static long ChargeFor(int workers) => (long)SlotCount * Stride + SlotCount + 256;
    internal FullCompilerScratch(int workers)
    {
        used = new bool[SlotCount];
        memory = (byte*)NativeMemory.Alloc(checked((nuint)(used.Length * (long)Stride)));
        if (memory == null) throw new OutOfMemoryException();
    }
    internal int Acquire()
    {
        for (int i = 0; i < used.Length; ++i)
            if (!used[i]) { used[i] = true; return i; }
        throw new InvalidOperationException("Full-JIT bounded recursive compiler scratch exhausted.");
    }
    internal Span<FullTranslationJit.Plan> Plans(int slot) => new(memory + slot * (long)Stride, MaximumInstructions);
    internal Span<FullBlockInstruction> Instructions(int slot) => new(memory + slot * (long)Stride + PlanBytes, MaximumInstructions);
    internal Span<byte> Code(int slot) => new(memory + slot * (long)Stride + PlanBytes + IrBytes, MaximumCodeBytes);
    internal void Release(int slot) { used[slot] = false; }
    public void Dispose()
    {
        foreach (bool busy in used) if (busy) throw new InvalidOperationException("Full-JIT compiler scratch is active.");
        NativeMemory.Free(memory); memory = null;
    }
}
