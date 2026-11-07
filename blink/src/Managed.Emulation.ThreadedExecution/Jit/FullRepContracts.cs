using System.Runtime.InteropServices;

namespace Managed.Emulation.Execution;

internal enum FullRepPhase : uint { Idle, Active, Done, FaultPending, FaultFinal, Abandoned }
internal enum FullRepReason : uint { None, NeedMemory, Authority, TicketBoundary, Attention, Stop, Fault, Quantum, Complete, Latched }

// Scope-owned unmanaged stack local. Parent never exposes this to guest code,
// carries it past its synchronous invocation, or stores it in a worker slot.
// No host mapping/code/payload pointer is retained here. Parent separately
// retains the issued Plan/value identity and the owner kernel capability.
[StructLayout(LayoutKind.Explicit, Size = FullRepState.ByteSize)]
internal unsafe struct FullRepState
{
    internal const int ByteSize = 256;
    internal const uint CurrentVersion = 0x52500001;
    internal const uint FrameAbi = 0x46420007;
    internal const int FrameOffset = 392;
    internal const FullBlockExit YieldExit = (FullBlockExit)8;
    [FieldOffset(0)] internal uint Version;
    [FieldOffset(4)] internal uint Size;
    [FieldOffset(8)] internal FullRepPhase Phase;
    [FieldOffset(12)] internal FullRepReason Reason;
    [FieldOffset(16)] internal ulong OwnerCookie;
    [FieldOffset(24)] internal ulong WorkerCookie;
    [FieldOffset(32)] internal ulong AttemptGeneration;
    [FieldOffset(40)] internal ulong OriginBodyGeneration;
    [FieldOffset(48)] internal ulong OriginPc;
    [FieldOffset(56)] internal ulong FallthroughPc;
    [FieldOffset(64)] internal ulong Context;
    [FieldOffset(72)] internal ulong KernelCookie;
    [FieldOffset(80)] internal uint OriginalFlags;
    [FieldOffset(84)] internal uint LastFlags;
    [FieldOffset(88)] internal FullRepOperation Operation;
    [FieldOffset(92)] internal uint Width;
    [FieldOffset(96)] internal uint AddressWidth;
    [FieldOffset(100)] internal FullRepRepeat RepeatMode;
    [FieldOffset(104)] internal int SourceSegment;
    [FieldOffset(108)] internal uint InstructionLength;
    [FieldOffset(112)] internal ulong SourceBase;
    [FieldOffset(120)] internal ulong CompletedIterations;
    [FieldOffset(128)] internal ulong RemainingCount; // full architectural RCX snapshot
    [FieldOffset(136)] internal ulong SourceIndex;
    [FieldOffset(144)] internal ulong DestinationIndex;
    [FieldOffset(152)] internal ulong Accumulator;
    [FieldOffset(160)] internal uint OriginSite;
    [FieldOffset(164)] internal uint IterationsThisCall;
    [FieldOffset(168)] internal ulong AdmissionEpoch; // zero unless successfully enrolled
    [FieldOffset(176)] internal ulong RequestGeneration;
    [FieldOffset(184)] internal fixed byte DecodedBytes[16];
    [FieldOffset(200)] internal uint QuantumLimit; // 0 NORMAL; nonzero diagnostic only
    [FieldOffset(204)] internal fixed byte Reserved[52];
}

// Value metadata only. Parent publishes into its dynamically charged PACKED
// owner code region/manifest, independently of TB flush. No allocator here.
internal readonly record struct FullRepKernelLayout(int CodeLength, bool WindowsAbi,
    int ResumeOffset, int FaultFinalizeOffset, ulong OwnerCookie, ulong KernelCookie);
