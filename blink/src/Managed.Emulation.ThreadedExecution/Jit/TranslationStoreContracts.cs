using System.Runtime.InteropServices;

namespace Managed.Emulation.Execution;

internal readonly record struct TranslationKey(ulong Pc, ulong Context);
internal readonly record struct TranslationToken(int Slot, ulong Generation);

// Shared ABI. Mutable links are RW data, never patches to RX instructions.
// A native frame must hold the owner's NativeEpochLease before reading ANY
// header, including a link target. Every instruction checks Live, generation
// and CodeEpoch. Link traversal additionally authenticates owner/context/PC.
[StructLayout(LayoutKind.Explicit, Size = 160)]
internal struct TranslationBlockHeader
{
    [FieldOffset(0)] public ulong OwnerCookie;
    [FieldOffset(8)] public ulong BodyGeneration;
    [FieldOffset(16)] public ulong GuestPc;
    [FieldOffset(24)] public nint Entry;
    [FieldOffset(32)] public ulong Context;
    [FieldOffset(40)] public int Live;
    [FieldOffset(44)] public uint Slot;
    [FieldOffset(48)] public nint Payload;
    [FieldOffset(56)] public uint PayloadBytes;
    [FieldOffset(60)] public uint GuestLength;
    [FieldOffset(64)] public nint Link0TargetHeader;
    [FieldOffset(72)] public nint Link1TargetHeader;
    [FieldOffset(80)] public ulong Link0TargetGeneration;
    [FieldOffset(88)] public ulong Link1TargetGeneration;
    [FieldOffset(96)] public ulong Link0Pc;
    [FieldOffset(104)] public ulong Link1Pc;
    [FieldOffset(112)] public ulong CodeEpoch;
    [FieldOffset(120)] public ulong ValidatedCodeEpoch;
    // Full ABI4: code/mapping freshness, NOT runtime/data admission. Ordinary
    // non-code writers retain this stamp; Live/CodeEpoch revoke physical code.
    [FieldOffset(128)] public ulong ValidatedGlobalEpoch;
    [FieldOffset(136)] public ulong Reserved136;
    // Full owner FEL1 immutable static exit: taken PC and tagged count/helper/edge metadata.
    [FieldOffset(144)] public ulong Reserved144;
    [FieldOffset(152)] public ulong Reserved152;
}

internal sealed record TranslationStoreOptions
{
    public long BudgetBytes { get; init; } = 128L << 20;
    public int MaximumWorkers { get; init; } = 32;
    public int HintsPerWorker { get; init; } = 256;
    public int RegionBytes { get; init; } = 1 << 20;
    // Compatibility hints only: arenas have no body-slot limit or per-body
    // replacement policy. Descriptor/index capacity grows within BudgetBytes.
    public int BodyCapacity { get; init; }
    // Optional aligned payload occupancy limit within the SAME shared budget;
    // zero lets descriptor/payload/code arenas share the whole byte quota.
    public long PayloadBytes { get; init; }
    public int ProtectedPercent { get; init; } = 75;
    public int PhaseAdmissionInterval { get; init; } = 32;
    public bool AllowUnverifiedWindows { get; init; }
    public bool RequireNativeFlags { get; init; } = true;
}

internal enum TranslationStoreFailure
{
    None, AlreadyPresent, InvalidSize, Capacity, NativeEpochActive,
    LeasedCapacity, PlatformDenied, NativeAllocation, StaleDraft, AuthorityChanged,
    NotFound, NotPublished, LeaseCapacity
}

// A draft is a unique publication capability. Copies cannot publish/abort a
// replacement generation. Only its reserving compiler may write its spans;
// discard all writable spans before Publish/Abort. No retained guest pointers.
// One outstanding draft closes store-wide native admission, including while
// compiler code uses these spans outside the translation gate. Copies share
// that admission and cannot consume it twice. Abort seals shared pages RX.
internal unsafe struct TranslationDraft
{
    internal RetainedTranslationStore? Owner;
    public TranslationToken Token { get; internal set; }
    public nint Header { get; internal set; }
    internal nint CodePointer, PayloadPointer;
    public int CodeCapacity { get; internal set; }
    public int PayloadCapacity { get; internal set; }
    public Span<byte> Code => Owner!.GetDraftCode(in this);
    public Span<byte> Payload => Owner!.GetDraftPayload(in this);
    internal void Clear() { this = default; }
}

// Value capabilities have bounded owner-side tickets. Disposing a copied
// capability twice is harmless; using either copy afterwards is rejected.
internal struct TranslationLease : IDisposable
{
    internal RetainedTranslationStore? Owner;
    internal int Ticket;
    internal ulong Serial;
    public TranslationToken Token { get; internal set; }
    public void Dispose() { Owner?.ReleaseManagedLease(Ticket, Serial); this = default; }
}

internal struct NativeEpochLease : IDisposable
{
    internal RetainedTranslationStore? Owner;
    internal int Ticket;
    internal ulong Serial;
    public void Dispose() { Owner?.EndNativeEpoch(Ticket, Serial); this = default; }
}

internal struct TranslationWorkerCache : IDisposable
{
    internal RetainedTranslationStore? Owner;
    internal int Worker;
    internal ulong Serial;
    public void Dispose() { Owner?.ReleaseWorker(Worker, Serial); this = default; }
}

internal readonly record struct TranslationStoreStatistics(
    long ChargedBytes, long NativeReservedBytes, int BodyCapacity, int ResidentBodies,
    int RetiredBodies, int ActiveManagedLeases, int ActiveNativeEpochs,
    long Publications, long Lookups, long HintHits, long DirectoryProbes,
    long CapacityRetirements, long ContentRetirements, long AuthorityRevocations,
    long Revalidations, long ReclaimedBodies, long CapacityDenials,
    long SetupAllocatedBytes, long GrowAllocatedBytes, long RetireAllocatedBytes,
    long CompilationAllocatedBytes, long PublicationAllocatedBytes,
    long NativeRegionAllocations, long CodePageReuses, long ProtectionFailures,
    long PhaseDemotions)
{
    // CapacityRetirements is the compatibility view of FlushRetirements:
    // bodies removed by a WHOLE-buffer flush, never per-body evictions.
    // ReclaimedBodies counts drained retired lifetimes, not reusable bytes.
    // PhaseDemotions is always zero; no hot/cold replacement policy remains.
    public long UsefulPublishedCodeBytes { get; init; }
    public long PeakUsefulPublishedCodeBytes { get; init; }
    // All aligned appends, including revoked/retired/aborted arena occupants.
    public long AlignedOccupiedCodeBytes { get; init; }
    public long PeakAlignedOccupiedCodeBytes { get; init; }
    public long PayloadUsedBytes { get; init; }
    public long PeakPayloadUsedBytes { get; init; }
    public long PublishedPayloadBytes { get; init; }
    // Descriptors include Header160, reverse links and provenance nodes.
    public long DescriptorUsedBytes { get; init; }
    public long PeakDescriptorUsedBytes { get; init; }
    // Includes obsolete grow-in-arena pointer/bucket arrays until bulk reset.
    public long IndexUsedBytes { get; init; }
    public long PeakIndexUsedBytes { get; init; }
    public long FixedChargedBytes { get; init; }
    public long PeakFixedChargedBytes { get; init; }
    public long ArenaReservedBytes { get; init; }
    // Independent owner-generated routines share the budget and survive flush.
    public long ExternalReservedBytes { get; init; }
    public long PeakArenaReservedBytes { get; init; }
    public long ArenaUsedBytes { get; init; }
    public long DataArenaUsedBytes { get; init; }
    public long DataArenaReservedBytes { get; init; }
    public long DataArenaSlackBytes { get; init; }
    public long DataArenaOverheadBytes { get; init; }
    public long CodeReservedBytes { get; init; }
    // Untouched code reservation versus slack in pages actually touched.
    public long RegionSlackBytes { get; init; }
    public long PageSlackBytes { get; init; }
    public long CumulativeEmittedCodeBytes { get; init; }
    public long PeakChargedBytes { get; init; }
    public long ProtectionCalls { get; init; }
    public long TotalFlushes { get; init; }
    public long FlushRetirements { get; init; }
    public long FlushRequests { get; init; }
    public long FlushDenials { get; init; }
    public bool FlushPending { get; init; }
    public int ActiveDrafts { get; init; }
    public bool Quarantined { get; init; }
    // Only worker-key lookups consult jump hints. Resume-token acquisition
    // and explicit shared-index queries are not jump-cache misses.
    public long WorkerLookups { get; init; }
    public long DirectKeyLookups { get; init; }
    public long TokenLookups { get; init; }
    public long WorkerHintMisses { get; init; }
    public long PhysicalIndexProbes { get; init; }
    public long VirtualIndexProbes { get; init; }
}
