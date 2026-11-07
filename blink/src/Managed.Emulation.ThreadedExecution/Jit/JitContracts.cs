using System.Runtime.InteropServices;

namespace Managed.Emulation.Execution;

public enum GuestJitMode { Interpreter, Hybrid, Full }
public enum GuestJitHostFeatures { Detected, BaselineOnly }

/// <summary>Select interpreter, opportunistic native acceleration, or strict compiled execution.</summary>
public sealed record GuestJitOptions
{
    /// <summary>Explicit execution selection. Full never enters instruction interpretation.</summary>
    public GuestJitMode Mode { get; init; }
    /// <summary>BaselineOnly excludes optional host instructions from emission;
    /// guest CPUID and software semantics remain unchanged.</summary>
    public GuestJitHostFeatures HostFeatures { get; init; }
    /// <summary>Legacy hybrid opt-in. Mode takes precedence when explicitly nondefault.</summary>
    public bool Enabled { get; init; }
    internal GuestJitMode EffectiveMode => Mode == GuestJitMode.Interpreter && Enabled ? GuestJitMode.Hybrid : Mode;
    private long? cacheLimitBytes;
    /// <summary>Charged owner retention quota. Full defaults to a 1 GiB retention limit (allocated on demand);
    /// Hybrid retains its 8 MiB default. Explicit limits are never raised.</summary>
    public long CacheLimitBytes
    {
        get => cacheLimitBytes ?? (EffectiveMode == GuestJitMode.Full ? 1L << 30 : 8L << 20);
        init => cacheLimitBytes = value;
    }
    /// <summary>Explicitly allow the implemented but unqualified Windows x64 backend.</summary>
    public bool AllowUnverifiedWindows { get; init; }
    /// <summary>Retain final architectural bytes for bounded diagnostic runs.</summary>
    public bool CaptureFinalState { get; init; }
    public int HotBlockThreshold { get; init; } = 16;
    public GuestJitOptions Validate()
    {
        if (!Enum.IsDefined(Mode)) throw new ArgumentOutOfRangeException(nameof(Mode));
        if (!Enum.IsDefined(HostFeatures)) throw new ArgumentOutOfRangeException(nameof(HostFeatures));
        if (CacheLimitBytes is < 65536 or > 2147483648L) throw new ArgumentOutOfRangeException(nameof(CacheLimitBytes));
        if (HotBlockThreshold is < 1 or > 65535) throw new ArgumentOutOfRangeException(nameof(HotBlockThreshold));
        return this with { CacheLimitBytes = CacheLimitBytes };
    }
}

public sealed record GuestCpuState(ulong InstructionPointer, byte OperationLength,
    uint Flags, ulong ExportedFlags, byte[] GeneralRegisters, byte[] VectorRegisters,
    byte[] FloatingPoint, uint Mxcsr, byte[] Segments);

public sealed record GuestJitStatistics
{
    public string Mode { get; init; } = "Interpreter";
    public string HostFeatures { get; init; } = "Detected";
    public long InlineNativeInstructions { get; init; }
    public long SemanticHelperInstructions { get; init; }
    public long SemanticHelperCalls { get; init; }
    public long InterpreterAttempts { get; init; }
    public int PhysicalAuthorityPages { get; init; }
    public int ActivePhysicalCodeBorrows { get; init; }
    public int PeakPhysicalCodeBorrows { get; init; }
    public bool AuthorityRegistrySaturated { get; init; }
    public long HostBackingMutationScopes { get; init; }
    public long NativeAuthorityEnrollmentMisses { get; init; }
    public long AuthorityEpochAdvances { get; init; }
    public long VirtualWriteNotifications { get; init; }
    public long ConservativeVirtualWriteBarriers { get; init; }
    /// <summary>Unique keys currently resident in the exact owner directory.</summary>
    public long UniqueBlockKeys { get; init; }
    public long RetainedBlockExecutions { get; init; }
    /// <summary>Worker jump-hint misses; token resumes and explicit index lookups are separate.</summary>
    public long LookupTierMisses { get; init; }
    public long WorkerKeyLookups { get; init; }
    public long SharedIndexLookups { get; init; }
    public long ResumeTokenLookups { get; init; }
    public long LinkTargetLookups { get; init; }
    public long LinkReuseHits { get; init; }
    public long LastExitLinkAttempts { get; init; }
    public long LastExitLinksCompleted { get; init; }
    public long DeferredLinkPreparations { get; init; }
    public long ActualBlockCompilations { get; init; }
    public int RepKernelCount { get; init; }
    public long RepKernelEmittedBytes { get; init; }
    public long RepKernelReservedBytes { get; init; }
    public long NativeRepIterations { get; init; }
    public long NativeRepCompletions { get; init; }
    public bool RepIterationCounterSaturated { get; init; }
    public long CompiledInstructionSites { get; init; }
    public long CompiledInlineSites { get; init; }
    public long CompiledHelperSites { get; init; }
    public long EmissionSizeRetries { get; init; }
    public long PublicationQuiescenceRequests { get; init; }
    public long NativePublicationAdmissionDenials { get; init; }
    public IReadOnlyList<long> BlockLengthCounts { get; init; } = Array.Empty<long>();
    public IReadOnlyDictionary<string, long> BlockStopCounts { get; init; } = new Dictionary<string, long>();
    public IReadOnlyList<long> NativeExitCounts { get; init; } = Array.Empty<long>();
    public long ContentInvalidations { get; init; }
    public long AuthorityRevalidations { get; init; }
    public long SetupAllocatedBytes { get; init; }
    /// <summary>CLR host allocation inside typed helpers and their host services,
    /// excluding nested compilation/publication. Guest malloc is a separate resource.</summary>
    public long HelperManagedAllocatedBytes { get; init; }
    public long PublicationAllocatedBytes { get; init; }
    public long DiagnosticsAllocatedBytes { get; init; }
    public long RetainedNativeBytes { get; init; }
    /// <summary>Owner common code is emitted once, separately from TB publication bytes.</summary>
    public long SharedScaffoldEmittedBytes { get; init; }
    public long SharedScaffoldReservedBytes { get; init; }
    public long PeakSharedScaffoldReservedBytes { get; init; }
    // Arena occupancy is current; peak fields and flush totals survive cleanup.
    public long UsefulPublishedCodeBytes { get; init; }
    public long PeakUsefulPublishedCodeBytes { get; init; }
    public long AlignedOccupiedCodeBytes { get; init; }
    public long PeakAlignedOccupiedCodeBytes { get; init; }
    public long PayloadUsedBytes { get; init; }
    public long PeakPayloadUsedBytes { get; init; }
    public long DescriptorUsedBytes { get; init; }
    public long PeakDescriptorUsedBytes { get; init; }
    public long IndexUsedBytes { get; init; }
    public long PeakIndexUsedBytes { get; init; }
    public long FixedChargedBytes { get; init; }
    public long PeakFixedChargedBytes { get; init; }
    public long ArenaReservedBytes { get; init; }
    public long PeakArenaReservedBytes { get; init; }
    public long ArenaUsedBytes { get; init; }
    public long DataArenaUsedBytes { get; init; }
    public long DataArenaReservedBytes { get; init; }
    public long DataArenaSlackBytes { get; init; }
    public long DataArenaOverheadBytes { get; init; }
    public long CodeReservedBytes { get; init; }
    public long RegionSlackBytes { get; init; }
    public long PageSlackBytes { get; init; }
    public long ProtectionCalls { get; init; }
    public long TotalFlushes { get; init; }
    public long FlushRetirements { get; init; }
    public long FlushRequests { get; init; }
    public long FlushDenials { get; init; }
    public int ResidentTranslationBlocks { get; init; }
    public int PeakResidentTranslationBlocks { get; init; }
    public long TranslationMisses { get; init; }
    public long CodeValidationMisses { get; init; }
    public long SemanticPlanBindings { get; init; }
    public long SemanticCacheHits { get; init; }
    public long SemanticCapacityEvictions { get; init; }
    public int SemanticCacheCapacity { get; init; }
    public int SemanticCacheEntries { get; init; }
    public int SemanticPeakCacheEntries { get; init; }
    public long SemanticDirectoryProbes { get; init; }
    public long SemanticDirectoryCollisions { get; init; }
    public long SemanticDataBorrows { get; init; }
    public long PageTableReadBorrows { get; init; }
    public long DeferredAnonymousRecycles { get; init; }
    public long PeakDeferredAnonymousPages { get; init; }
    public int ActiveDataBorrows { get; init; }
    public int DeferredAnonymousPages { get; init; }
    public IReadOnlyDictionary<string, long> SemanticHelpers { get; init; } = new Dictionary<string, long>();
    public string Capability { get; init; } = "Disabled";
    public long CompiledBlocks { get; init; }
    public long NativeEntries { get; init; }
    public long NativeDispatches { get; init; }
    public long Continuations { get; init; }
    public long NativeTailJumps { get; init; }
    public long NativeChainedInstructions { get; init; }
    public long GuestInstructions { get; init; }
    public long NativeMemoryInstructions { get; init; }
    public long NativeMemoryAssists { get; init; }
    public IReadOnlyDictionary<string, long> NativeMemoryAssistOutcomes { get; init; } = new Dictionary<string, long>();
    public IReadOnlyDictionary<string, long>? FullAuthorityCounts { get; init; }
    public ulong NativeMemoryLastAssistPc { get; init; }
    public ulong NativeMemoryLastAssistAddress { get; init; }
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

internal enum JitOperation
{
    Mov, Add, Sub, Cmp, And, Or, Xor, Test, Inc, Dec, Jump, ConditionalJump, Nop,
    Lea, Movzx, Movsx, LoadMov, LoadMovzx, StoreMov, Push, Pop, MemoryCmp,
    MemoryCmpReverse, MemoryTest, Call, Ret,
    Adc, Sbb, Neg, Not, ShiftLeft, ShiftRight, ShiftArithmeticRight,
    RotateLeft, RotateRight, RotateCarryLeft, RotateCarryRight,
    ShiftDoubleLeft, ShiftDoubleRight, MultiplyUnsigned, MultiplySigned,
    MultiplySignedLow, DivideUnsigned, DivideSigned, ByteSwap,
    BitScanForward, BitScanReverse, BitTest, BitSet, BitReset, BitComplement,
    SetCondition, ConditionalMove, Exchange, ExchangeAdd, CompareExchange,
    CompareExchangePair, PushFlags, PopFlags, Leave, Enter, IndirectJump,
    IndirectCall, ReturnAdjust, SignExtendAccumulator, SignExtendDividend,
    ClearCarry, SetCarry, ComplementCarry, DirectionClear, DirectionSet,
    VectorInteger, AtomicMemory, VectorFloat, Pause,
    Loop, LoopEqual, LoopNotEqual, CounterZeroJump, LoadFlagsAh, StoreFlagsAh,
    StringMove, StringStore, StringLoad, StringCompare, StringScan,
    ReadSegmentSelector, TranslateByte, NonTemporalStore,
    PopulationCount, LeadingZeroCount, TrailingZeroCount, X87, SystemState, ChecksumCrc32
}
internal enum JitMemoryAccess : byte { Read = 1, Write = 2 }
// Register index follows x86 encoding; high-byte registers use ByteOffset=1.
internal readonly record struct JitOperand(int Register, ulong Immediate, bool IsImmediate = false, int ByteOffset = 0, bool IsMemory = false);
// Address-size-64 offset only. No segment base; memory forms guard before use.
// A missing base/index uses -1; Scale is log2 of the x86 index multiplier.
internal readonly record struct JitAddress(int Base, int Index, int Scale, long Displacement, bool RipRelative = false,
    int AddressWidth = 64, int Segment = -1);
// SourceWidth is the actual memory width for every checked memory operation.
internal readonly record struct JitInstruction(ulong Ip, int Length, JitOperation Operation,
    int Width, JitOperand Destination, JitOperand Source, ulong Target = 0, int Condition = 0,
    int SourceWidth = 0, JitAddress Address = default, JitOperand Extra = default, uint Options = 0);
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
