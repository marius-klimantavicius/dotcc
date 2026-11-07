using System.Runtime.InteropServices;

namespace Managed.Emulation.Execution;

// This is translation-time value IR, never a runtime opcode dispatcher. A helper
// site uses only Instruction.Ip/Length; the operation-specific immutable helper
// plan belongs to the corresponding instruction index in the retained payload.
internal readonly record struct FullBlockInstruction(JitInstruction Instruction, bool RequiresHelper = false,
    FullX87KernelCall X87Kernel = default)
{
    internal static FullBlockInstruction Helper(ulong ip, int length)
        => new(new(ip, length, JitOperation.Nop, 64, default, default), true);
}

// Deliberately distinct from JitExit: value 4 means Invalidated, NOT MemoryMiss.
internal enum FullBlockExit : uint
{
    Continue = 0, Stop = 1, Attention = 2, Budget = 3, Invalidated = 4, NeedHelper = 5,
    NeedMemory = 6, GuestFault = 7, RepYield = 8, NeedEnvironment = 9
}

// Leaf ABI, independent of Hybrid. Offsets 0..95 match JitFrame exactly, but
// Version/Size and Exit are Full-specific. Every pointer is invocation-owned:
// pin counters synchronously and hold the store's GLOBAL native epoch over the
// whole native frame, including all linked bodies. Header/payload/code must not
// be recycled during that epoch. No managed reference is embedded in code.
//
// Initialize Executed/TailJumps=0 for a fresh native call. NativeLimit=0 means
// unbounded normal native execution; a nonzero value is an explicit TOTAL cap,
// not remaining fuel. NativeLimit/Executed/TailJumps survive every native link.
// Translation's maximum of 512 sites is independent of this invocation cap.
// Across a managed helper, the parent subtracts completed inline/helper work
// from an explicit logical invocation cap before creating the next frame (one
// for signal actors); zero stays unbounded through every helper continuation.
//
// NeedHelper has reserved one issue and sets machine.oplen=Length, machine.ip=
// Ip+Length, HelperIndex=index, ResumeIndex=index+1. It has NOT completed the
// instruction. Copy the typed plan while leased, return from native completely,
// release native epochs/authority before managed helper invocation, then apply
// the ordinary helper commit/stash/oplen/completion protocol. Resume only after
// exact-token reacquisition and revalidation, if IP matches the next instruction.
// ResumeIndex==instruction count is informational and must dispatch in managed
// code; it is not a valid native entry index. A helper may change mode/context,
// so eligibility must be rechecked after every helper before resuming.
//
// FirstPreissued=1 means the parent already approved/reserved the cold demand
// fetch's issue. The first reached Issue consumes this flag, once, without a
// second increment or budget check. Poll/authority failures preserve the flag.
// Normal issue uses lock-xadd: an exhausted attempt still increments Issued and
// exits Budget with no architectural operation effects, matching the interpreter.
[StructLayout(LayoutKind.Explicit, Size = FullBlockFrame.ByteSize)]
internal unsafe struct FullBlockFrame
{
    public const int ByteSize = 416;
    public const uint AbiVersion = 0x46420007;
    public const uint NoHelperIndex = uint.MaxValue;
    [FieldOffset(0)] public uint Version;
    [FieldOffset(4)] public uint Size;
    [FieldOffset(8)] public void* Machine;
    [FieldOffset(16)] public long* Issued;
    [FieldOffset(24)] public long* Completed;
    [FieldOffset(32)] public int* Stop;
    [FieldOffset(40)] public ulong Budget;
    [FieldOffset(48)] public ulong Executed;
    [FieldOffset(56)] public FullBlockExit Exit;
    [FieldOffset(60)] public uint NativeLimit;
    [FieldOffset(64)] public nint OwnerHandle;
    [FieldOffset(72)] public ulong NextIp;
    // Hybrid-prefix fields remain fixed; Full uses its separate software TLB.
    [FieldOffset(80)] public JitDataDescriptor* DataDescriptors;
    [FieldOffset(88)] public uint DataDescriptorCount;
    [FieldOffset(92)] public uint DataGeneration;
    [FieldOffset(96)] public TranslationBlockHeader* Header;
    [FieldOffset(104)] public ulong OwnerCookie;
    [FieldOffset(112)] public ulong BodyGeneration;
    [FieldOffset(120)] public ulong Context;
    // Runtime admission/data-TLB tag: EVERY new physical writer still pulses
    // this word and drains native execution, even on known non-code pages.
    [FieldOffset(128)] public ulong* GlobalEpochPointer;
    [FieldOffset(136)] public ulong ObservedGlobalEpoch;
    [FieldOffset(144)] public uint ResumeIndex;
    [FieldOffset(148)] public uint HelperIndex;
    [FieldOffset(152)] public uint FirstPreissued;
    [FieldOffset(156)] public uint Reserved;
    [FieldOffset(160)] public ulong TailJumps;
    // Architectural processor vector, never a Linux signal or Blink halt code.
    // The controller maps #DE0/#PF14 to kMachineDivideError/SegmentationFault.
    // FaultCode is segvcode (1/2 or uint(-2) BUS_ADRERR).
    [FieldOffset(168)] public uint FaultTrap;
    [FieldOffset(172)] public uint FaultCode;
    [FieldOffset(176)] public ulong FaultAddress;
    [FieldOffset(184)] public ulong Temporary0;
    [FieldOffset(192)] public ulong Temporary1;
    [FieldOffset(200)] public ulong Temporary2;
    [FieldOffset(208)] public ulong Temporary3;
    [FieldOffset(216)] public ulong MemoryAddress;
    [FieldOffset(224)] public byte* MemoryHost0;
    [FieldOffset(232)] public byte* MemoryHost1;
    [FieldOffset(240)] public ulong MemoryEpoch;
    [FieldOffset(248)] public uint MemorySite;
    [FieldOffset(252)] public FullMemoryAccess MemoryAccess;
    [FieldOffset(256)] public uint MemoryWidth;
    [FieldOffset(260)] public uint MemoryLength0;
    [FieldOffset(264)] public ulong NativeMemoryComputations;
    [FieldOffset(272)] public ulong MemoryAssists;
    [FieldOffset(280)] public FullMemoryTlbEntry* MemoryTlb;
    [FieldOffset(288)] public uint MemoryTlbMask;
    // 0 normally; 1 reserves the SAME approved issue across NeedMemory. The
    // parent must set FirstPreissued=1 for the exact token/site on reentry.
    [FieldOffset(292)] public uint MemoryResume;
    [FieldOffset(296)] public ulong* StoreGenerationPointer;
    [FieldOffset(304)] public ulong ObservedStoreGeneration;
    [FieldOffset(312)] public FullNativeJumpEntry* JumpCache;
    [FieldOffset(320)] public uint JumpCacheMask;
    [FieldOffset(324)] public FullMemoryFlags MemoryFlags;
    [FieldOffset(328)] public ulong FsBase;
    [FieldOffset(336)] public ulong GsBase;
    [FieldOffset(344)] public ulong MemoryTicket;
    [FieldOffset(352)] public uint HostMxcsrSaved;
    [FieldOffset(356)] public uint HostMxcsrWork;
    [FieldOffset(360)] public uint HostMxcsrDirty;
    [FieldOffset(364)] public uint HostMxcsrObserved;
    // Header.ValidatedGlobalEpoch is a CODE/MAPPING stamp, independent of the
    // runtime/data word above. All three transfer paths authenticate both.
    [FieldOffset(368)] public ulong* CodeAuthorityEpochPointer;
    [FieldOffset(376)] public ulong ObservedCodeAuthorityEpoch;
    // Frame storage is invocation-owned through RET;
    // the controller may retain value request fields to resume the same issue.
    [FieldOffset(384)] public FullStackMemoryDescriptor* StackMemory;
    // Synchronous invocation-owned REP progress; never a worker-global slot.
    [FieldOffset(392)] public FullRepState* Rep;
    // Active canonical physical80 storage; null for inactive compatibility.
    [FieldOffset(400)] public FullX87RegisterFile* X87;
    [FieldOffset(408)] public ulong X87Reserved;
}
