using System.Runtime.InteropServices;

namespace Managed.Emulation.Execution;

/// <summary>Experimental native guest acceleration. Unsupported instructions always interpret.</summary>
public sealed record GuestJitOptions
{
    public bool Enabled { get; init; }
    public long CacheLimitBytes { get; init; } = 8L << 20;
    /// <summary>Explicitly allow the implemented but unqualified Windows x64 backend.</summary>
    public bool AllowUnverifiedWindows { get; init; }
    /// <summary>Retain final architectural bytes for bounded diagnostic runs.</summary>
    public bool CaptureFinalState { get; init; }
    public int HotBlockThreshold { get; init; } = 16;
    public GuestJitOptions Validate()
    {
        if (CacheLimitBytes is < 65536 or > 67108864) throw new ArgumentOutOfRangeException(nameof(CacheLimitBytes));
        if (HotBlockThreshold is < 1 or > 65535) throw new ArgumentOutOfRangeException(nameof(HotBlockThreshold));
        return this;
    }
}

public sealed record GuestCpuState(ulong InstructionPointer, byte OperationLength,
    uint Flags, ulong ExportedFlags, byte[] GeneralRegisters, byte[] VectorRegisters,
    byte[] FloatingPoint, uint Mxcsr, byte[] Segments);

public sealed record GuestJitStatistics
{
    public string Capability { get; init; } = "Disabled";
    public long CompiledBlocks { get; init; }
    public long NativeEntries { get; init; }
    public long NativeDispatches { get; init; }
    public long Continuations { get; init; }
    public long GuestInstructions { get; init; }
    public long NativeMemoryInstructions { get; init; }
    public long NativeMemoryInstructionsAtFirstChild { get; init; }
    public long EntryHintHits { get; init; }
    public long StaleEntryHints { get; init; }
    public long StructuralNegativeHits { get; init; }
    public long HotPositiveEvictions { get; init; }
    public long CompileAllocatedBytes { get; init; }
    public long EmittedCodeBytes { get; init; }
    public long BudgetEvictions { get; init; }
    public long NegativeBudgetEvictions { get; init; }
    public long NegativePromotions { get; init; }
    public long CacheHits { get; init; }
    public long BlockEntries { get; init; }
    public long CompileAttempts { get; init; }
    public long HotnessCollisions { get; init; }
    public long EntryCollisions { get; init; }
    public long Evictions { get; init; }
    public long NegativeCacheHits { get; init; }
    public long CompileBudgetDenials { get; init; }
    public long ShortNativeExits { get; init; }
    public long FirstChildInstructions { get; init; } = -1;
    public long NativeInstructionsAtFirstChild { get; init; }
    public long StaticCodePages { get; init; }
    public long RevokedStaticCodePages { get; init; }
    public IReadOnlyList<long> FragmentLengths { get; init; } = Array.Empty<long>();
    public long CacheBytes { get; init; }
    public long PeakCacheBytes { get; init; }
    public long ValidationFailures { get; init; }
    public long InterpreterInstructions { get; init; }
    public long Callbacks { get; init; }
    public double CompileMilliseconds { get; init; }
    public IReadOnlyDictionary<string, long> Fallbacks { get; init; } = new Dictionary<string, long>();
}

internal enum JitOperation { Mov, Add, Sub, Cmp, And, Or, Xor, Test, Inc, Dec, Jump, ConditionalJump, Nop, Lea, Movzx, Movsx, LoadMov, LoadMovzx, StoreMov, Push, Pop, MemoryCmp, MemoryCmpReverse, MemoryTest, Call, Ret }
internal enum JitMemoryAccess : byte { Read = 1, Write = 2 }
// Register index follows x86 encoding; high-byte registers use ByteOffset=1.
internal readonly record struct JitOperand(int Register, ulong Immediate, bool IsImmediate = false, int ByteOffset = 0);
// Address-size-64 offset only. No segment base; memory forms guard before use.
// A missing base/index uses -1; Scale is log2 of the x86 index multiplier.
internal readonly record struct JitAddress(int Base, int Index, int Scale, long Displacement, bool RipRelative = false);
// SourceWidth is the actual memory width for every checked memory operation.
internal readonly record struct JitInstruction(ulong Ip, int Length, JitOperation Operation,
    int Width, JitOperand Destination, JitOperand Source, ulong Target = 0, int Condition = 0,
    int SourceWidth = 0, JitAddress Address = default);
internal enum JitExit : uint { Continue = 0, Stop = 1, Attention = 2, Budget = 3, MemoryMiss = 4 }

// Invocation-owned, authenticated private data backing. Mapping identity,
// permissions and lifetime are protected by the mapping mutex and native gate; contents
// may change concurrently. No pointer in this table comes from an unchecked TLB.
[StructLayout(LayoutKind.Explicit, Size = 48)]
internal struct JitDataDescriptor
{
    public const int MaximumCount = 4;
    [FieldOffset(0)] public ulong Page;
    [FieldOffset(8)] public nint Host;
    [FieldOffset(16)] public ulong Identity;
    [FieldOffset(24)] public ulong Generation;
    [FieldOffset(32)] public uint ValidStart;
    [FieldOffset(36)] public uint ValidEnd;
    [FieldOffset(40)] public uint Permissions;
}

// Leaf-only v4: no native CALL, stack adjustment, or retained managed reference.
// OwnerHandle is opaque and rooted for the entire cache lifetime. Native pointers
// to managed counters are pinned only for the synchronous unmanaged invocation.
[StructLayout(LayoutKind.Explicit, Size = 96)]
internal unsafe struct JitFrame
{
    public const uint AbiVersion = 4;
    [FieldOffset(0)] public uint Version;
    [FieldOffset(4)] public uint Size;
    [FieldOffset(8)] public void* Machine;
    [FieldOffset(16)] public long* Issued;
    [FieldOffset(24)] public long* Completed;
    [FieldOffset(32)] public int* Stop;
    [FieldOffset(40)] public ulong Budget;
    [FieldOffset(48)] public ulong Executed;
    [FieldOffset(56)] public JitExit Exit;
    // Zero requests the compiled cap; otherwise min(request, compiled cap).
    // Native entry writes the effective invocation cap here for exact exits.
    [FieldOffset(60)] public uint NativeLimit;
    [FieldOffset(64)] public nint OwnerHandle;
    [FieldOffset(72)] public ulong NextIp;
    [FieldOffset(80)] public JitDataDescriptor* DataDescriptors;
    [FieldOffset(88)] public uint DataDescriptorCount;
    [FieldOffset(92)] public uint DataGeneration;
}

internal static class JitMemoryCounts
{
    // Early conditional branches are external side exits. Every committed
    // prefix follows the linear fall-through, optionally repeating only the
    // final backedge. Any taken early branch ends that same prefix, so the exact
    // completed count indexes memory work without extra native counter stores.
    internal static byte[] Build(ReadOnlySpan<JitInstruction> instructions)
    {
        byte[] counts = new byte[32 + 1];
        int backedge = -1;
        if (instructions[^1].Operation is JitOperation.Jump or JitOperation.ConditionalJump)
            for (int i = 0; i < instructions.Length; ++i)
                if (instructions[i].Ip == instructions[^1].Target) { backedge = i; break; }
        int index = 0;
        for (int executed = 0; executed < 32; ++executed)
        {
            var op = instructions[index].Operation;
            counts[executed + 1] = (byte)(counts[executed] +
                (op is JitOperation.LoadMov or JitOperation.LoadMovzx or JitOperation.StoreMov or
                    JitOperation.Push or JitOperation.Pop or JitOperation.Call or JitOperation.Ret or JitOperation.MemoryCmp or
                    JitOperation.MemoryCmpReverse or JitOperation.MemoryTest ? 1 : 0));
            if (++index == instructions.Length)
            {
                if (backedge < 0) break;
                index = backedge;
            }
        }
        return counts;
    }

}
