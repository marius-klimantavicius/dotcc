using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

// Relative metadata contains no native ownership or managed references. Emit once
// into RW staging, publish exactly CodeLength with NativeCodeAllocation.TryCreate,
// then Resolve against that allocation's RX Entry. Charge its actual page-rounded
// mapping ONCE per owner, separately from body/store arenas and staging scratch.
internal readonly record struct FullSharedScaffoldLayout(
    int CodeLength, bool WindowsAbi, int EntryOffset, int InlineGuardOffset,
    int HelperGuardOffset, int LinkOffset, int ExitsOffset, int BoundaryOffset,
    int MemoryReadOffset, int MemoryWriteOffset, int MemoryReadWriteOffset, int MemoryAtomicFastOffset)
{
    internal const int ExitStride = 21 + FullBlockEmitter.FloatRestoreHostEnvironmentBytes;

    internal FullSharedScaffold Resolve(nint rxEntry)
    {
        if (rxEntry == 0 || CodeLength <= 0 || EntryOffset < 0 || InlineGuardOffset < 0 ||
            HelperGuardOffset < 0 || LinkOffset < 0 || ExitsOffset < 0 || BoundaryOffset < 0 ||
            EntryOffset > CodeLength - 4 || InlineGuardOffset > CodeLength - 4 ||
            HelperGuardOffset > CodeLength - 4 || LinkOffset > CodeLength - 4 || BoundaryOffset > CodeLength - 4 ||
            ExitsOffset > CodeLength - ExitStride * ((int)FullBlockExit.NeedEnvironment + 1) ||
            MemoryReadOffset <= BoundaryOffset || MemoryWriteOffset <= MemoryReadOffset ||
            MemoryReadWriteOffset <= MemoryWriteOffset || MemoryAtomicFastOffset <= MemoryReadWriteOffset ||
            MemoryAtomicFastOffset > ExitsOffset - 4)
            throw new ArgumentException("Uninitialized or invalid Full scaffold layout");
        _ = checked(rxEntry + CodeLength);
        return new(rxEntry, this);
    }
}

// Nonowning immutable addresses. The single RX slab MUST outlive all body code,
// native entries, leases and global store epochs, including across bulk flushes.
// Dispose only at quiescent owner disposal, never on an individual TB retirement.
// These are internal JMP targets (R10=frame, R11=machine; Entry also takes R9=
// body resume-dispatch continuation), NOT host-ABI TB entries. Header.Entry must
// always point at the emitted BODY entry. The owner roots the allocation object.
// EmitOwnerScaffold authenticates immutable identity at Entry: its guard-only
// addresses require that SAME authenticated body invocation, not a host entry.
// A shared native frame is still a volatile-register leaf: no CALL, stack change,
// managed helper, GC safepoint or asynchronous generated-frame unwind contract.
// Frame.NextIp is a PRIVATE expected-root operand while native runs. A shared
// memory lookup temporarily uses it as a native continuation: no header guard,
// poll or CALL occurs until its ENDBR64 landing restores the compile-time root.
// Lookup failures go straight to typed exits, which overwrite it. Every entry
// seeds it from its own compile-time root (also on resume/tail entry), and EVERY
// typed exit replaces it with machine.ip before RET. Consumers may read it only
// before entry or after the leaf returns. Guest state/oplen are never used as
// scratch. Header144/152 hold owner-only FEL1 exit metadata; Frame.Reserved156 remains unused.
internal readonly record struct FullSharedScaffold(nint BaseAddress, FullSharedScaffoldLayout Layout)
{
    internal bool IsValid => BaseAddress != 0 && Layout.CodeLength > 0;
    internal bool WindowsAbi => Layout.WindowsAbi;
    internal nint Entry => BaseAddress + Layout.EntryOffset;
    internal nint InlineGuard => BaseAddress + Layout.InlineGuardOffset;
    internal nint HelperGuard => BaseAddress + Layout.HelperGuardOffset;
    // Guard-only continuation ABI: R8=PC, R9=operation, RDX=pre-Issue continuation.
    internal nint Boundary => BaseAddress + Layout.BoundaryOffset;
    // Fixed memory guard entries: RAX=exact input EA, R10/R11=frame/machine;
    // R9 survives, RAX/RCX/R8 are scratch, RDX returns the checked host address.
    // Success JMPs through private Frame.NextIp, never through a host stack.
    internal nint MemoryRead => BaseAddress + Layout.MemoryReadOffset;
    internal nint MemoryWrite => BaseAddress + Layout.MemoryWriteOffset;
    internal nint MemoryReadWrite => BaseAddress + Layout.MemoryReadWriteOffset;
    internal nint MemoryAtomicFast => BaseAddress + Layout.MemoryAtomicFastOffset;
    internal nint Link => BaseAddress + Layout.LinkOffset;
    internal nint Exit(FullBlockExit exit) => BaseAddress + Layout.ExitsOffset +
        (int)exit * FullSharedScaffoldLayout.ExitStride;
}

// A retained, mixed-instruction TB. Runtime control flow consists only of
// instruction labels, typed helper returns, and two authenticated RW data links.
// There is no runtime opcode selection, managed CALL, stack adjustment, or use of
// nonvolatile registers on either ABI. R10=frame and R11=machine throughout.
internal static partial class FullBlockEmitter
{
    internal const int MaximumInstructions = 512;
    private const int ExitCount = (int)FullBlockExit.NeedEnvironment + 1;
    private const int Link0Label = MaximumInstructions + ExitCount;
    private const int Link1Label = Link0Label + 1;
    private const int InlineGuardLabel = Link1Label + 1;
    private const int HelperGuardLabel = InlineGuardLabel + 1;
    private const int BoundaryLabel = HelperGuardLabel + 1;
    private const int InlineIssueLabel = BoundaryLabel + 1;
    private const int IssueLabel = InlineIssueLabel + 1;
    private const int LinkHeaderLabel = IssueLabel + 1;
    private const int EntryLabel = LinkHeaderLabel + 1;
    private const int AbiFailureLabel = EntryLabel + 1;
    private const int LabelCount = AbiFailureLabel + 1;
    // Practical staging/publication envelope matching the smallest code region,
    // not a host unwind-info limit. Large scalar bodies may exceed it: TryEmit
    // then returns false and the compiler retries with half as many sites.
    // All RIP/rel32 fixups are local to this envelope; external JMPs use imm64.
    // Forward-fixup chains live in staging, so only labels/heads (~4 KiB) and
    // an 11-byte discarded-output sink live on the emitting thread's stack.
    internal const int CodeCapacity = 64 * 1024;
    // Still one 4096-byte RX page. FullTranslationJit also charges staging by
    // this constant: its fixed owner allowance therefore rises by 2048 bytes.
    internal const int ScaffoldCapacity = 4096;
    internal const int ScratchBytes = LabelCount * sizeof(int) * 2 + 11;

    // Emits directly into caller-owned RW staging memory; returns the exact RX
    // publication length. The caller owns the retained IR and helper payload.
    // windowsAbi exists for source/encoding contract checks on either platform;
    // only invoke emitted code for the current platform's actual ABI.
    internal static int Emit(ReadOnlySpan<FullBlockInstruction> instructions,
        Span<byte> destination, bool? windowsAbi = null)
    {
        if (!TryEmit(instructions, destination, out int length, windowsAbi))
            throw new ArgumentException("Full block exceeds the destination or code envelope", nameof(destination));
        return length;
    }

    // False means oversized, length=0; discard all staging bytes and retry a
    // shorter prefix. Invalid IR/layout still throws. Neither success nor
    // oversize allocates. Never publish partial output, even with a large span.
    internal static bool TryEmit(ReadOnlySpan<FullBlockInstruction> instructions,
        Span<byte> destination, out int codeLength, bool? windowsAbi = null)
    {
        Validate(instructions);
        if (!Layout.Supported)
            throw new PlatformNotSupportedException("Translated Machine/Full block layout differs from the native ABI");
        Span<int> labels = stackalloc int[LabelCount];
        Span<int> fixupHeads = stackalloc int[LabelCount];
        Span<byte> overflowBytes = stackalloc byte[11];
        var writer = new Writer(instructions, destination[..Math.Min(destination.Length, CodeCapacity)],
            labels, fixupHeads, overflowBytes, windowsAbi ?? OperatingSystem.IsWindows());
        codeLength = writer.Emit();
        return codeLength != 0;
    }

    // Same translation-time validation and scalar/helper code as standalone.
    // Cross-allocation transfers are movabs/JMP, never a rel32 range assumption.
    // No native allocation or managed object is created by either emitter API.
    internal static int Emit(ReadOnlySpan<FullBlockInstruction> instructions,
        Span<byte> destination, in FullSharedScaffold scaffold, bool? windowsAbi = null)
    {
        if (!TryEmit(instructions, destination, in scaffold, out int length, windowsAbi))
            throw new ArgumentException("Full block exceeds the destination or code envelope", nameof(destination));
        return length;
    }

    internal static bool TryEmit(ReadOnlySpan<FullBlockInstruction> instructions,
        Span<byte> destination, in FullSharedScaffold scaffold, out int codeLength, bool? windowsAbi = null)
        => TryEmitSharedCore(instructions, destination, in scaffold, out codeLength, windowsAbi,
            ownerPrivateCompletion: false);

    // Explicit owner-only completion capability, independent of scaffold validity
    // and retained-identity guards. EVERY caller/linked target must supply a fresh
    // invocation-local Completed long, live until native RET and the host x87 return
    // guard, then publish its delta once with Interlocked.Add. No concurrent reader,
    // writer, retained request or managed helper may use that long while native runs.
    // Full/ENV use the same owner body; Issue/Budget remain shared and locked.
    // This is a compile-time emission choice, never a frame/descriptor ABI flag.
    internal static bool TryEmitOwnerPrivateCompletion(ReadOnlySpan<FullBlockInstruction> instructions,
        Span<byte> destination, in FullSharedScaffold scaffold, out int codeLength, bool? windowsAbi = null)
        => TryEmitSharedCore(instructions, destination, in scaffold, out codeLength, windowsAbi,
            ownerPrivateCompletion: true);

    private static bool TryEmitSharedCore(ReadOnlySpan<FullBlockInstruction> instructions,
        Span<byte> destination, in FullSharedScaffold scaffold, out int codeLength, bool? windowsAbi,
        bool ownerPrivateCompletion)
    {
        Validate(instructions);
        if (!Layout.Supported)
            throw new PlatformNotSupportedException("Translated Machine/Full block layout differs from the native ABI");
        if (!scaffold.IsValid || (windowsAbi.HasValue && windowsAbi.Value != scaffold.WindowsAbi))
            throw new ArgumentException("Full scaffold is uninitialized or uses a different host ABI", nameof(scaffold));
        Span<int> labels = stackalloc int[LabelCount];
        Span<int> fixupHeads = stackalloc int[LabelCount];
        Span<byte> overflowBytes = stackalloc byte[11];
        var writer = new Writer(instructions, destination[..Math.Min(destination.Length, CodeCapacity)], labels, fixupHeads,
            overflowBytes, scaffold.WindowsAbi, scaffold, ownerPrivateCompletion: ownerPrivateCompletion);
        codeLength = writer.Emit();
        return codeLength != 0;
    }

    internal static int EmitScaffold(Span<byte> destination, out FullSharedScaffoldLayout layout,
        bool? windowsAbi = null)
        => EmitScaffoldCore(destination, out layout, windowsAbi, retainedIdentity: false);

    // Owner-only contract: the root TranslationLease and the ACTUAL store's
    // NativeEpochLease span every entry/link/continuation through RET. Frame
    // identity/counters cannot alias header metadata. Runtime enrollment remains
    // the controller's independent obligation; this is no admission capability.
    // Standalone and general-purpose scaffolds retain their original guards.
    internal static int EmitOwnerScaffold(Span<byte> destination, out FullSharedScaffoldLayout layout,
        bool? windowsAbi = null)
        => EmitScaffoldCore(destination, out layout, windowsAbi, retainedIdentity: true);

    private static int EmitScaffoldCore(Span<byte> destination, out FullSharedScaffoldLayout layout,
        bool? windowsAbi, bool retainedIdentity)
    {
        if (!Layout.Supported)
            throw new PlatformNotSupportedException("Translated Machine/Full block layout differs from the native ABI");
        Span<int> labels = stackalloc int[LabelCount];
        Span<int> fixupHeads = stackalloc int[LabelCount];
        Span<byte> overflowBytes = stackalloc byte[11];
        var writer = new Writer(default, destination[..Math.Min(destination.Length, ScaffoldCapacity)],
            labels, fixupHeads, overflowBytes, windowsAbi ?? OperatingSystem.IsWindows(), commonScaffold: true,
            retainedIdentity: retainedIdentity);
        int length = writer.EmitScaffold(out layout);
        if (length == 0)
            throw new ArgumentException("Full scaffold destination is too small", nameof(destination));
        return length;
    }

    internal static bool CanInline(JitInstruction instruction) => CanInlineCanonicalX87Invalid(instruction) || CanInlineProcessorEnvironment(instruction) || CanInlineResidualScalar(instruction) || CanInlineNearControl(instruction) || CanInlineEnter(instruction) || CanInlineMaskedStore(instruction) || CanInlineCanonicalX87Stack(instruction) || CanInlineCanonicalX87Arithmetic(instruction) || CanInlineCanonicalX87Memory(instruction) || CanInlineCanonicalX87Control(instruction) || CanInlineCanonicalX87Transcendental(instruction) || CanInlineCanonicalX87MemoryArithmetic(instruction) || CanInlineChecksum(instruction) || CanInlineFlagStack(instruction) || CanInlineProcessorQuery(instruction) || CanInlineSystemState(instruction) || CanInlineBitCount(instruction) || CanInlineScalarMisc(instruction) || CanInlineString(instruction) || CanInlineControlFlag(instruction) || CanInlineHint(instruction) || CanInlineStackMemory(instruction) || CanInlineAtomic(instruction) || CanInlineMemoryInteger(instruction) || CanInlineFloat(instruction) || CanInlineVectorMove(instruction) || CanInlineVector(instruction) || CanInlineMemory(instruction) ||
        CanInlineInteger(instruction) || (!instruction.Destination.IsMemory && !instruction.Source.IsMemory &&
        !instruction.Extra.IsMemory && instruction.Operation is (
        JitOperation.Mov or JitOperation.Jump or
        JitOperation.ConditionalJump or JitOperation.Nop or JitOperation.Lea or
        JitOperation.Movzx or JitOperation.Movsx));

    private static void Validate(ReadOnlySpan<FullBlockInstruction> instructions)
    {
        if (instructions.Length is < 1 or > MaximumInstructions)
            throw new ArgumentOutOfRangeException(nameof(instructions));
        for (int i = 0; i < instructions.Length; ++i)
        {
            var site = instructions[i];
            var instruction = site.Instruction;
            if (instruction.Length is < 1 or > 15)
                throw new ArgumentException("Invalid Full instruction length");
            if (i != 0 && instruction.Ip != unchecked(instructions[i - 1].Instruction.Ip +
                (uint)instructions[i - 1].Instruction.Length))
                throw new ArgumentException("Full TB instructions must be contiguous");
            // Helpers may represent ANY predecoded operation. Their opcode,
            // operands, mode and semantic function pointer live in the payload.
            if (site.RequiresHelper) continue;
            // Strict family validation must precede the broad legacy gate.
            if (ValidateAddressExtension(instruction)) continue;
            if (!CanInline(instruction))
                throw new ArgumentException("Instruction requires a typed Full helper");
            if (ValidateNearControl(instruction))
            {
                if (i != instructions.Length - 1)
                    throw new ArgumentException("A Full near control transfer must terminate its TB");
                continue;
            }
            if (ValidateCanonicalX87Invalid(instruction)) continue;
            if (ValidateCanonicalX87Stack(instruction)) continue;
            var kernel = site.X87Kernel;
            if (ValidateCanonicalX87Arithmetic(instruction, in kernel)) continue;
            if (ValidateCanonicalX87Memory(instruction, in kernel)) continue;
            if (ValidateCanonicalX87Control(instruction, in kernel)) continue;
            if (ValidateCanonicalX87Transcendental(instruction, in kernel)) continue;
            if (ValidateCanonicalX87MemoryArithmetic(instruction, in kernel)) continue;
            if (ValidateChecksum(instruction)) continue;
            if (ValidateFlagStack(instruction)) continue;
            if (ValidateProcessorQuery(instruction)) continue;
            if (ValidateProcessorEnvironment(instruction)) continue;
            if (ValidateResidualScalar(instruction)) continue;
            if (ValidateSystemState(instruction)) continue;
            if (ValidateBitCount(instruction)) continue;
            if (ValidateScalarMisc(instruction)) continue;
            if (ValidateString(instruction)) continue;
            if (ValidateControlFlag(instruction))
            {
                if (IsNativeCounterBranch(instruction) && i != instructions.Length - 1)
                    throw new ArgumentException("A Full counter branch must terminate its TB");
                continue;
            }
            if (ValidateHint(instruction)) continue;
            if (ValidateAtomic(instruction)) continue;
            if (ValidateFloatExtended(instruction)) continue;
            if (ValidateFloat(instruction)) continue;
            if (ValidateEnter(instruction)) continue;
            if (ValidateMaskedStore(instruction)) continue;
            if (ValidateMmx(instruction)) continue;
            if (ValidateVectorExtended(instruction)) continue;
            if (ValidateVectorMove(instruction)) continue;
            if (ValidateVector(instruction)) continue;
            if (instruction.Width is not (8 or 16 or 32 or 64))
                throw new ArgumentException("Invalid scalar native instruction width");
            if (ValidateMemoryInteger(instruction)) continue;
            if (ValidateMemory(instruction))
            {
                if (IsNativeMemoryControl(instruction) && i != instructions.Length - 1)
                    throw new ArgumentException("A Full memory control transfer must terminate its TB");
                continue;
            }
            if (CanInlineInteger(instruction)) continue;
            if (instruction.Operation is JitOperation.Jump or JitOperation.ConditionalJump)
            {
                if (i != instructions.Length - 1)
                    throw new ArgumentException("A Full control transfer must terminate its TB");
                if (instruction.Operation == JitOperation.ConditionalJump && instruction.Condition is < 0 or > 15)
                    throw new ArgumentException("Invalid Full branch condition");
                continue;
            }
            if (instruction.Operation == JitOperation.Nop) continue;
            ValidateOperand(instruction.Destination, instruction.Width, destination: true);
            if (instruction.Operation == JitOperation.Lea)
            {
                var address = instruction.Address;
                if (instruction.Width is not (32 or 64) || address.Base is < -1 or > 15 ||
                    address.Index is < -1 or > 15 || address.Scale is < 0 or > 3 ||
                    (address.RipRelative && (address.Base != -1 || address.Index != -1)))
                    throw new ArgumentException("Invalid Full LEA operand");
            }
            else if (instruction.Operation is JitOperation.Movzx or JitOperation.Movsx)
            {
                if (instruction.Width is not (16 or 32 or 64) || instruction.SourceWidth is not (8 or 16 or 32) ||
                    instruction.SourceWidth > instruction.Width ||
                    (instruction.SourceWidth == 32 && (instruction.Operation != JitOperation.Movsx || instruction.Width != 64)) ||
                    instruction.Source.IsImmediate)
                    throw new ArgumentException("Invalid Full extension operand");
                ValidateOperand(instruction.Source, instruction.SourceWidth, destination: false);
            }
            else if (instruction.Operation is not (JitOperation.Inc or JitOperation.Dec))
                ValidateOperand(instruction.Source, instruction.Width, destination: false);
        }
    }

    private static void ValidateOperand(JitOperand operand, int width, bool destination)
    {
        if (operand.IsMemory) throw new ArgumentException("Unprepared Full memory operand");
        if (operand.IsImmediate)
        {
            if (destination) throw new ArgumentException("Immediate destination");
            return;
        }
        if (operand.Register is < 0 or > 15 || operand.ByteOffset is < 0 or > 1 ||
            (operand.ByteOffset != 0 && (width != 8 || operand.Register > 3)))
            throw new ArgumentException("Invalid Full register operand");
    }

    private static class Layout
    {
        internal static readonly bool Supported = HasExpectedLayout();
    }

    internal static unsafe bool HasExpectedLayout()
    {
        if (!X64BlockEmitter.IsLayoutSupported || !FullCanonicalX87ExecutionProfile.LayoutSupported || sizeof(FullBlockFrame) != FullBlockFrame.ByteSize ||
            sizeof(TranslationBlockHeader) != 160) return false;
        Blink.Machine m = default;
        byte* p = (byte*)&m;
        // XedMachineMode is one packed byte, with bitfield properties rather
        // than addressable fields. Byte 10 is Machine.threaded, not genmode.
        m.mode.omode = Blink.XED_MODE_LONG;
        m.mode.genmode = Blink.XED_GEN_MODE_PROTECTED;
        if (sizeof(Blink.XedMachineMode) != 1 || p[9] !=
            (Blink.XED_MODE_LONG | Blink.XED_GEN_MODE_PROTECTED << 2)) return false;
        // Verify every frame/header offset consumed by emitted code, rather
        // than relying on the explicitly requested layout alone.
        return
            Offset<FullBlockFrame>(nameof(FullBlockFrame.Version)) == 0 &&
            Offset<FullBlockFrame>(nameof(FullBlockFrame.Size)) == 4 &&
            Offset<FullBlockFrame>(nameof(FullBlockFrame.Machine)) == 8 &&
            Offset<FullBlockFrame>(nameof(FullBlockFrame.Issued)) == 16 &&
            Offset<FullBlockFrame>(nameof(FullBlockFrame.Completed)) == 24 &&
            Offset<FullBlockFrame>(nameof(FullBlockFrame.Stop)) == 32 &&
            Offset<FullBlockFrame>(nameof(FullBlockFrame.Budget)) == 40 &&
            Offset<FullBlockFrame>(nameof(FullBlockFrame.Executed)) == 48 &&
            Offset<FullBlockFrame>(nameof(FullBlockFrame.Exit)) == 56 &&
            Offset<FullBlockFrame>(nameof(FullBlockFrame.NativeLimit)) == 60 &&
            Offset<FullBlockFrame>(nameof(FullBlockFrame.NextIp)) == 72 &&
            Offset<FullBlockFrame>(nameof(FullBlockFrame.Header)) == 96 &&
            Offset<FullBlockFrame>(nameof(FullBlockFrame.OwnerCookie)) == 104 &&
            Offset<FullBlockFrame>(nameof(FullBlockFrame.BodyGeneration)) == 112 &&
            Offset<FullBlockFrame>(nameof(FullBlockFrame.Context)) == 120 &&
            Offset<FullBlockFrame>(nameof(FullBlockFrame.GlobalEpochPointer)) == 128 &&
            Offset<FullBlockFrame>(nameof(FullBlockFrame.ObservedGlobalEpoch)) == 136 &&
            Offset<FullBlockFrame>(nameof(FullBlockFrame.ResumeIndex)) == 144 &&
            Offset<FullBlockFrame>(nameof(FullBlockFrame.HelperIndex)) == 148 &&
            Offset<FullBlockFrame>(nameof(FullBlockFrame.FirstPreissued)) == 152 &&
            Offset<FullBlockFrame>(nameof(FullBlockFrame.TailJumps)) == 160 &&
            Offset<FullBlockFrame>(nameof(FullBlockFrame.FaultTrap)) == 168 &&
            Offset<FullBlockFrame>(nameof(FullBlockFrame.FaultCode)) == 172 &&
            Offset<FullBlockFrame>(nameof(FullBlockFrame.FaultAddress)) == 176 &&
            Offset<FullBlockFrame>(nameof(FullBlockFrame.Temporary0)) == 184 &&
            Offset<FullBlockFrame>(nameof(FullBlockFrame.Temporary1)) == 192 &&
            Offset<FullBlockFrame>(nameof(FullBlockFrame.Temporary2)) == 200 &&
            Offset<FullBlockFrame>(nameof(FullBlockFrame.Temporary3)) == 208 &&
            Offset<FullBlockFrame>(nameof(FullBlockFrame.CodeAuthorityEpochPointer)) == 368 &&
            Offset<FullBlockFrame>(nameof(FullBlockFrame.ObservedCodeAuthorityEpoch)) == 376 &&
            Offset<FullBlockFrame>(nameof(FullBlockFrame.StackMemory)) == 384 &&
            Offset<FullBlockFrame>(nameof(FullBlockFrame.Rep)) == 392 &&
            Offset<FullBlockFrame>(nameof(FullBlockFrame.X87)) == 400 &&
            Offset<FullBlockFrame>(nameof(FullBlockFrame.X87Reserved)) == 408 &&
            sizeof(FullStackMemoryDescriptor) == 128 && sizeof(FullStackMemoryRequest) == 48 &&
            Offset<TranslationBlockHeader>(nameof(TranslationBlockHeader.OwnerCookie)) == 0 &&
            Offset<TranslationBlockHeader>(nameof(TranslationBlockHeader.BodyGeneration)) == 8 &&
            Offset<TranslationBlockHeader>(nameof(TranslationBlockHeader.GuestPc)) == 16 &&
            Offset<TranslationBlockHeader>(nameof(TranslationBlockHeader.Entry)) == 24 &&
            Offset<TranslationBlockHeader>(nameof(TranslationBlockHeader.Context)) == 32 &&
            Offset<TranslationBlockHeader>(nameof(TranslationBlockHeader.Live)) == 40 &&
            Offset<TranslationBlockHeader>(nameof(TranslationBlockHeader.Link0TargetHeader)) == 64 &&
            Offset<TranslationBlockHeader>(nameof(TranslationBlockHeader.Link1TargetHeader)) == 72 &&
            Offset<TranslationBlockHeader>(nameof(TranslationBlockHeader.Link0TargetGeneration)) == 80 &&
            Offset<TranslationBlockHeader>(nameof(TranslationBlockHeader.Link1TargetGeneration)) == 88 &&
            Offset<TranslationBlockHeader>(nameof(TranslationBlockHeader.Link0Pc)) == 96 &&
            Offset<TranslationBlockHeader>(nameof(TranslationBlockHeader.Link1Pc)) == 104 &&
            Offset<TranslationBlockHeader>(nameof(TranslationBlockHeader.CodeEpoch)) == 112 &&
            Offset<TranslationBlockHeader>(nameof(TranslationBlockHeader.ValidatedCodeEpoch)) == 120 &&
            Offset<TranslationBlockHeader>(nameof(TranslationBlockHeader.ValidatedGlobalEpoch)) == 128;
    }

    private static int Offset<T>(string field) where T : struct => Marshal.OffsetOf<T>(field).ToInt32();

    private ref partial struct Writer
    {
        private readonly ReadOnlySpan<FullBlockInstruction> instructions;
        private readonly Span<byte> code;
        private readonly Span<int> labels;
        private readonly Span<int> fixupHeads;
        private readonly Span<byte> overflowBytes;
        private readonly bool windowsAbi;
        private readonly FullSharedScaffold shared;
        private readonly bool commonScaffold;
        private readonly bool retainedIdentity;
        private readonly bool ownerPrivateCompletion; // emitting stack only; no persistent charge
        private int position;
        private bool oversized;

        internal Writer(ReadOnlySpan<FullBlockInstruction> instructions, Span<byte> code,
            Span<int> labels, Span<int> fixupHeads, Span<byte> overflowBytes, bool windowsAbi,
            FullSharedScaffold shared = default, bool commonScaffold = false, bool retainedIdentity = false,
            bool ownerPrivateCompletion = false)
        {
            this.instructions = instructions; this.code = code;
            this.labels = labels; this.fixupHeads = fixupHeads; this.windowsAbi = windowsAbi;
            this.overflowBytes = overflowBytes;
            this.shared = shared; this.commonScaffold = commonScaffold;
            this.retainedIdentity = retainedIdentity;
            this.ownerPrivateCompletion = ownerPrivateCompletion;
            position = 0; oversized = false; labels.Fill(-1); fixupHeads.Fill(-1);
        }

        internal int Emit()
        {
            Bytes(0xf3, 0x0f, 0x1e, 0xfa); // ENDBR64, including linked entries
            Bytes(0x49, 0x89, windowsAbi ? (byte)0xca : (byte)0xfa); // r10 = RCX/RDI
            Bytes(0x4d, 0x8b, 0x5a, 0x08); // r11 = machine
            ProcessorEnvironmentReadyEntry(); // BEFORE shared.Entry and Prologue
            if (shared.IsValid)
            {
                Bytes(0x48, 0xb8); U64(instructions[0].Instruction.Ip);
                Bytes(0x49, 0x89, 0x42, 72); // private expected-root operand
                int dispatcher = Continuation();
                AbsoluteJump(shared.Entry);
                Patch(dispatcher, position);
                EndBranch(); // common prologue JMP r9 returns here
            }
            else Prologue();
            Bytes(0x41, 0x8b, 0x82); U32(144); // actual known instruction labels
            for (int i = 0; i < instructions.Length; ++i)
            {
                Bytes(0x3d); U32((uint)i); // CMP eax,imm32: no imm8 sign extension/truncation
                Bytes(0x0f, 0x84); Reference(i);
            }
            JumpExit(FullBlockExit.Invalidated); // unknown resume can never execute

            for (int i = 0; i < instructions.Length; ++i)
            {
                var site = instructions[i];
                var instruction = site.Instruction;
                Bind(i);
                Bytes(0x41, 0xc7, 0x82); U32(144); U32((uint)i);
                Bytes(0x49, 0xb8); U64(instruction.Ip); // r8 = expected machine PC
                // R9 is the operation continuation. The shared guard uses RDX
                // for its inline/helper continuation, then Issue may clobber
                // RDX with lock-xadd. No native return address or stack frame.
                int continuation = Continuation();
                if (!site.RequiresHelper && (CanInlineCanonicalX87Invalid(instruction) || CanInlineCanonicalX87Stack(instruction) || CanInlineCanonicalX87Arithmetic(instruction) || CanInlineCanonicalX87Memory(instruction) || CanInlineCanonicalX87Control(instruction) || CanInlineCanonicalX87Transcendental(instruction) || CanInlineCanonicalX87MemoryArithmetic(instruction)))
                {
                    // The common boundary retains the existing Poll/Header/IP/
                    // Cap priority. Its RDX return lands BEFORE this site's Issue.
                    // No frame word is repurposed and no semantic opcode is read.
                    Bytes(0x48, 0x8d, 0x15); int prepare = position; U32(0);
                    if (shared.IsValid) AbsoluteJump(shared.Boundary);
                    else { Bytes(0xe9); Reference(BoundaryLabel); }
                    Patch(prepare, position); EndBranch();
                    GuardInlineContext(noDataFault: CanInlineCanonicalX87Invalid(instruction));
                    if (CanInlineCanonicalX87Arithmetic(instruction))
                    {
                        var call = site.X87Kernel;
                        RequireCanonicalX87Kernel(in call, arithmetic: true);
                    }
                    else if (CanInlineCanonicalX87Memory(instruction))
                    {
                        var call = site.X87Kernel;
                        RequireCanonicalX87Kernel(in call, arithmetic: false);
                    }
                    else if (CanInlineCanonicalX87MemoryArithmetic(instruction))
                    {
                        var call = site.X87Kernel;
                        RequireCanonicalX87Kernel(in call, CanonicalX87MemoryArithmeticRequiresPrecision(instruction));
                    }
                    else if (CanInlineCanonicalX87Control(instruction) && CanonicalX87ControlNeedsKernel(instruction))
                    {
                        var call = site.X87Kernel;
                        RequireCanonicalX87Kernel(in call, CanonicalX87ControlRequiresArithmeticPrecision(instruction));
                    }
                    else if (CanInlineCanonicalX87Transcendental(instruction))
                    {
                        var call = site.X87Kernel;
                        RequireCanonicalX87Kernel(in call, arithmetic: false);
                    }
                    else RequireCanonicalX87();
                    Issue();
                    Bytes(0x41, 0xff, 0xe1); // operation continuation R9
                }
                else if (shared.IsValid)
                    AbsoluteJump(site.RequiresHelper ? shared.HelperGuard : shared.InlineGuard);
                else
                { Bytes(0xe9); Reference(site.RequiresHelper ? HelperGuardLabel : InlineGuardLabel); }
                Patch(continuation, position);
                EndBranch(); // every indirect continuation is also CET-safe
                if (site.RequiresHelper)
                {
                    Bytes(0x41, 0xc6, 0x43, 8, (byte)instruction.Length); // pre-helper oplen
                    SetIp(unchecked(instruction.Ip + (uint)instruction.Length));
                    Bytes(0x41, 0xc7, 0x82); U32(148); U32((uint)i);
                    Bytes(0x41, 0xc7, 0x82); U32(144); U32((uint)(i + 1));
                    JumpExit(FullBlockExit.NeedHelper); // no commit, CALL, or epoch escape
                }
                else if (IsNativeNearControl(instruction))
                {
                    NativeNearControl(instruction); // owns target proof, Commit and Dispatch
                }
                else if (IsNativeCounterBranch(instruction))
                {
                    NativeCounterBranch(instruction); // owns Commit and selected static edge
                }
                else if (IsNativeMemoryControl(instruction))
                {
                    NativeMemoryControl(instruction); // owns Commit and dynamic dispatch
                }
                else if (instruction.Operation == JitOperation.ConditionalJump)
                {
                    Condition(instruction.Condition);
                    Bytes(0x85, 0xc0);
                    int taken = NearCondition(0x85);
                    Commit(unchecked(instruction.Ip + (uint)instruction.Length));
                    Dispatch(unchecked(instruction.Ip + (uint)instruction.Length), 0);
                    Patch(taken, position);
                    Commit(instruction.Target);
                    Dispatch(instruction.Target, 1);
                }
                else
                {
                    Operation(instruction, site.X87Kernel);
                    ulong next = instruction.Operation == JitOperation.Jump ? instruction.Target :
                        unchecked(instruction.Ip + (uint)instruction.Length);
                    // POPF changes execution context (TF/AC) and is always a
                    // controller boundary. Never enter a stored helper-first
                    // link under the pre-POPF frame context.
                    if (instruction.Operation == JitOperation.PopFlags)
                    { Bytes(0x41, 0xc7, 0x82); U32(144); U32((uint)(i + 1)); }
                    Commit(next);
                    if (instruction.Operation == JitOperation.PopFlags)
                        JumpExit(FullBlockExit.Continue);
                    else if (i + 1 == instructions.Length)
                        Dispatch(next, instruction.Operation == JitOperation.Jump ? 1 : 0);
                    // Ordinary sites fall through to the next full guard/poll.
                }
            }
            // Compact bodies retain only edge selectors and local conditional
            // exit trampolines. All authentication/RET bytes live in the owner.
            Bind(Link0Label);
            Bytes(0x45, 0x31, 0xc9); // r9d = link0 byte offset
            if (shared.IsValid) AbsoluteJump(shared.Link);
            else { Bytes(0xe9); Reference(LinkHeaderLabel); }
            Bind(Link1Label);
            Bytes(0x41, 0xb9); U32(8); // r9d = link1 byte offset
            if (shared.IsValid)
            {
                AbsoluteJump(shared.Link);
                for (int exit = 0; exit < ExitCount; ++exit)
                    if (fixupHeads[MaximumInstructions + exit] >= 0)
                    {
                        Bind(MaximumInstructions + exit);
                        AbsoluteJump(shared.Exit((FullBlockExit)exit));
                    }
            }
            else
            {
                Bind(LinkHeaderLabel); Link();
                Scaffolding();
                Exits();
            }
            return oversized ? 0 : position;
        }

        internal int EmitScaffold(out FullSharedScaffoldLayout layout)
        {
            Bind(EntryLabel); EndBranch();
            Prologue();
            Bytes(0x41, 0xff, 0xe1); // JMP r9 to body's known-label resume dispatcher
            Bind(LinkHeaderLabel); EndBranch(); Link();
            Scaffolding();
            int read = SharedMemoryGuard(FullMemoryAccess.Read);
            int write = SharedMemoryGuard(FullMemoryAccess.Write);
            int readWrite = SharedMemoryGuard(FullMemoryAccess.Read | FullMemoryAccess.Write);
            int atomicFast = SharedMemoryGuard(FullMemoryAccess.Read | FullMemoryAccess.Write, atomicFast: true);
            int exits = position;
            Exits();
            if (oversized) { layout = default; return 0; }
            layout = new(position, windowsAbi, labels[EntryLabel], labels[InlineGuardLabel],
                labels[HelperGuardLabel], labels[LinkHeaderLabel], exits, labels[BoundaryLabel],
                read, write, readWrite, atomicFast);
            return position;
        }

        private void Prologue()
        {
            Bytes(0x41, 0x81, 0x3a); U32(FullBlockFrame.AbiVersion);
            Bytes(0x0f, 0x85); Reference(AbiFailureLabel);
            Bytes(0x41, 0x81, 0x7a, 4); U32(FullBlockFrame.ByteSize);
            Bytes(0x0f, 0x85); Reference(AbiFailureLabel);
            Bytes(0x41, 0x83, 0xba); U32(152); Bytes(1); // FirstPreissued is 0/1
            ExitCondition(0x87, FullBlockExit.Invalidated);
            GuardX87Frame(independentRep: false);
            if (retainedIdentity)
            {
                // EACH body seeds NextIp with its literal root before this
                // entry, including helper resume, REP latch and both link kinds.
                // ENV Ready has its separate pre-Prologue authentication/commit.
                Bytes(0x49, 0x8b, 0x42, 96, 0x48, 0x85, 0xc0);
                ExitCondition(0x84, FullBlockExit.Invalidated);
                GuardHeaderIdentity(FullBlockExit.Invalidated);
            }
            // NativeLimit=0 is genuinely unbounded; nonzero caps are exact.
            // Entry (including linked entry) never resets cap or aggregates.
            Bytes(0x41, 0xc7, 0x82); U32(148); U32(FullBlockFrame.NoHelperIndex);
        }

        private void Exits()
        {
            for (int exit = 0; exit < ExitCount; ++exit)
            {
                Bind(MaximumInstructions + exit);
                if (commonScaffold) EndBranch();
                FloatRestoreHostEnvironment();
                Bytes(0x49, 0x8b, 0x03, 0x49, 0x89, 0x42, 72); // NextIp = materialized machine.ip
                Bytes(0xb8); U32((uint)exit);
                Bytes(0x41, 0x89, 0x42, 56, 0xc3); // full typed exit + leaf RET
            }
            // An unauthenticated caller may have provided only the 96-byte
            // prefix. Never inspect extended FP slots before version/size pass.
            Bind(AbiFailureLabel);
            Bytes(0x49, 0x8b, 0x03, 0x49, 0x89, 0x42, 72);
            Bytes(0xb8); U32((uint)FullBlockExit.Invalidated);
            Bytes(0x41, 0x89, 0x42, 56, 0xc3);
        }

        private void AbsoluteJump(nint target)
        {
            Bytes(0x48, 0xb8); U64(unchecked((ulong)target));
            Bytes(0xff, 0xe0); // JMP rax; no native CALL/rel32 allocation constraint
        }

        private void Scaffolding()
        {
            Bind(InlineGuardLabel);
            if (commonScaffold) EndBranch();
            Bytes(0x48, 0x8d, 0x15); Reference(InlineIssueLabel); // rdx = inline continuation
            Bytes(0xe9); Reference(BoundaryLabel);
            Bind(HelperGuardLabel);
            if (commonScaffold) EndBranch();
            Bytes(0x48, 0x8d, 0x15); Reference(IssueLabel); // helpers skip mode/TF/RF
            Bind(BoundaryLabel);
            if (commonScaffold) EndBranch(); // exported native guard-only entry
            Poll();
            GuardHeader(); // mutable guards always; identity at retained owner Entry
            // Preserves r8=expected PC, r9=operation, rdx=continuation.
            Bytes(0x4d, 0x39, 0x03); // machine.ip == expected site PC
            ExitCondition(0x85, FullBlockExit.Invalidated);
            GuardCap();
            Bytes(0xff, 0xe2); // JMP rdx, never CALL
            Bind(InlineIssueLabel);
            EndBranch();
            GuardInlineContext();
            Bind(IssueLabel);
            EndBranch();
            Issue();
            Bytes(0x41, 0xff, 0xe1); // JMP r9 to exact scalar operation/helper staging
        }

        private void EndBranch() => Bytes(0xf3, 0x0f, 0x1e, 0xfa);

        // Return the RIP-relative displacement to patch after the following
        // direct JMP. These local fixups reuse the same staging-byte scratch.
        private int Continuation()
        {
            Bytes(0x4c, 0x8d, 0x0d); // lea r9,[rip+continuation]
            int at = position; U32(0); return at;
        }

        private void Poll()
        {
            Bytes(0x49, 0x8b, 0x42, 32, 0x83, 0x38, 0);
            ExitCondition(0x85, FullBlockExit.Stop);
            // The parent already approved the first instruction before demand
            // fetch. Execute it before handling attention, as ExecuteOne and
            // recursive SignalActor do; never discard that issued allowance.
            Bytes(0x41, 0x83, 0xba); U32(152); Bytes(0);
            int approved = NearCondition(0x85);
            Bytes(0x41, 0x80, 0x7b, 11, 0);
            ExitCondition(0x85, FullBlockExit.Attention);
            Patch(approved, position);
        }

        // A guard is an instruction boundary, not a lock against a concurrent
        // mutation that starts after it. The parent must retain its authority
        // barrier or equivalent write/version protocol over the native frame.
        // The store's global native epoch protects lifetime, not guest content.
        private void GuardHeader(FullBlockExit failure = FullBlockExit.Invalidated)
        {
            // Only RAX/RCX are scratch: R8/R9/RDX carry the site's PC and
            // continuations across this shared instruction-boundary guard.
            Bytes(0x49, 0x8b, 0x42, 96, 0x48, 0x85, 0xc0); // rax = current header
            ExitCondition(0x84, failure);
            Bytes(0x83, 0x78, 40, 1); // published Live == 1
            ExitCondition(0x85, failure);
            if (!retainedIdentity) GuardHeaderIdentity(failure);
            Bytes(0x48, 0x8b, 0x48, 112, 0x48, 0x85, 0xc9); // nonzero CodeEpoch
            ExitCondition(0x84, failure);
            Bytes(0x48, 0x3b, 0x48, 120); // validated SAME code version
            ExitCondition(0x85, failure);
            Bytes(0x49, 0x8b, 0x8a); U32(376); // observed code/mapping epoch
            Bytes(0x48, 0x3b, 0x88); U32(128); // header.ValidatedGlobalEpoch
            ExitCondition(0x85, failure);
            Bytes(0x49, 0x8b, 0x82); U32(368); // frame.CodeAuthorityEpochPointer
            Bytes(0x48, 0x85, 0xc0);
            ExitCondition(0x84, failure);
            Bytes(0x48, 0x3b, 0x08);
            ExitCondition(0x85, failure);
            // Independent runtime poll: data writers still stop this frame
            // and make all cached native data pointers recheck admission.
            Bytes(0x49, 0x8b, 0x8a); U32(136);
            Bytes(0x49, 0x8b, 0x82); U32(128); // frame.GlobalEpochPointer (header no longer needed)
            Bytes(0x48, 0x85, 0xc0);
            ExitCondition(0x84, failure);
            Bytes(0x48, 0x3b, 0x08); // *global == observed
            ExitCondition(0x85, failure);
        }

        // RAX is the nonnull current header; only RCX is scratch. Under the
        // retained owner epoch these four fields cannot change or be recycled.
        // R10/R11 and the root/frame identity survive all internal leaf paths;
        // links adopt a new header/generation only before re-entering BODY Entry.
        private void GuardHeaderIdentity(FullBlockExit failure)
        {
            Bytes(0x48, 0x8b, 0x08, 0x49, 0x3b, 0x4a, 104); // owner
            ExitCondition(0x85, failure);
            Bytes(0x48, 0x8b, 0x48, 8, 0x49, 0x3b, 0x4a, 112); // exact body generation
            ExitCondition(0x85, failure);
            Bytes(0x48, 0x8b, 0x48, 32, 0x49, 0x3b, 0x4a, 120); // immutable context
            ExitCondition(0x85, failure);
            if (commonScaffold)
                Bytes(0x49, 0x8b, 0x4a, 72); // compile-time root seeded by EACH body entry
            else
            { Bytes(0x48, 0xb9); U64(instructions[0].Instruction.Ip); }
            Bytes(0x48, 0x39, 0x48, 16); // this emitted body's root PC
            ExitCondition(0x85, failure);
        }

        private void GuardInlineContext(bool noDataFault = false)
        {
            Bytes(0x41, 0x80, 0x7b, 9, (byte)(Blink.XED_MODE_LONG | Blink.XED_GEN_MODE_PROTECTED << 2));
            ExitCondition(0x85, FullBlockExit.Invalidated);
            // A live canonical frame selects the explicitly scoped native12
            // software flag policy. Ordinary inactive frames keep their prior
            // conservative context contract. Only RAX/RCX are scratch here.
            IntegerFrameLoad(400, 0); IntegerRR(0x85, 0, 0);
            int legacy = NearCondition(0x84);
            Bytes(0x41, 0x80, 0xbb); U32((uint)FullCanonicalX87ExecutionProfile.MachineMetalOffset); Bytes(0);
            ExitCondition(0x85, FullBlockExit.Invalidated);
            Bytes(0x49, 0x83, 0xbb); U32((uint)FullCanonicalX87ExecutionProfile.MachineStashOffset); Bytes(0);
            ExitCondition(0x85, FullBlockExit.Invalidated);
            int noData = noDataFault ? IntegerJump() : -1;
            Bytes(0x41, 0xf7, 0x43, 12); U32(FullCanonicalX87ExecutionProfile.AlignmentMask);
            int acClear = NearCondition(0x84);
            Bytes(0x49, 0x8b, 0x83); U32((uint)FullCanonicalX87ExecutionProfile.MachineSystemOffset);
            IntegerRR(0x85, 0, 0); ExitCondition(0x84, FullBlockExit.Invalidated);
            Bytes(0xf7, 0x80); U32((uint)FullCanonicalX87ExecutionProfile.SystemCr0Offset);
            U32(FullCanonicalX87ExecutionProfile.AlignmentMask);
            ExitCondition(0x85, FullBlockExit.Invalidated);
            int canonicalDone = IntegerJump();
            Patch(legacy, position);
            Bytes(0x41, 0xf7, 0x43, 12); U32((1u << Blink.FLAGS_TF) | (1u << Blink.FLAGS_RF) | (1u << Blink.FLAGS_AC));
            ExitCondition(0x85, FullBlockExit.Invalidated);
            Patch(acClear, position); Patch(canonicalDone, position);
            if (noData >= 0) Patch(noData, position);
        }

        private void GuardCap()
        {
            Bytes(0x41, 0xf7, 0x82); U32(324); U32((uint)FullMemoryFlags.BarrierConsumed);
            ExitCondition(0x85, FullBlockExit.Continue);
            Bytes(0x41, 0x8b, 0x42, 60, 0x85, 0xc0);
            int unbounded = NearCondition(0x84);
            Bytes(0x49, 0x39, 0x42, 48); // Executed >= explicit TOTAL cap
            ExitCondition(0x83, FullBlockExit.Continue);
            Patch(unbounded, position);
        }

        private void Issue()
        {
            Bytes(0x41, 0x83, 0xba); U32(152); Bytes(0);
            int ordinary = NearCondition(0x84);
            Bytes(0x41, 0xc7, 0x82); U32(152); U32(0); // consume approved cold issue once
            Bytes(0xe9); int done = position; U32(0);
            Patch(ordinary, position);
            Bytes(0x49, 0x8b, 0x42, 16, 0xba, 1, 0, 0, 0);
            Bytes(0xf0, 0x48, 0x0f, 0xc1, 0x10); // lock xadd *Issued,rdx (old issued)
            Bytes(0x49, 0x3b, 0x52, 40);
            ExitCondition(0x83, FullBlockExit.Budget); // rejected attempt is still issued
            Patch(done, position);
        }

        private void SetIp(ulong ip)
        {
            Bytes(0x48, 0xb8); U64(ip);
            Bytes(0x49, 0x89, 0x03);
        }

        private void Commit(ulong ip)
        {
            SetIp(ip);
            Bytes(0x41, 0xc6, 0x43, 8, 0); // ordinary architectural commit
            EmitCompletedIncrement();
            Bytes(0x49, 0xff, 0x42, 48); // ++frame.Executed (inline only)
            GuardCap(); // no extra poll/issue/link after the last permitted instruction
        }

        // Translation-time helper only: emits inline bytes, never a native CALL.
        // All three completion sites use the SAME explicit capability. A general
        // shared scaffold may carry a global Completed pointer and MUST stay locked.
        private void EmitCompletedIncrement()
        {
            Bytes(0x49, 0x8b, 0x42, 24); // rax = frame.Completed
            if (!ownerPrivateCompletion) Bytes(0xf0);
            Bytes(0x48, 0xff, 0x00); // [lock] inc qword ptr [rax]
        }

        private void Dispatch(ulong ip, int edge)
        {
            // Internal branches are only the final transfer, and re-enter the
            // complete per-instruction poll/authority/context/issue sequence.
            for (int i = 0; i < instructions.Length; ++i)
                if (instructions[i].Instruction.Ip == ip)
                { Bytes(0xe9); Reference(i); return; }
            Bytes(0xe9); Reference(edge == 0 ? Link0Label : Link1Label);
        }

        private void Link()
        {
            // Do not hold a store lock: the external GLOBAL native epoch keeps
            // every loaded target pointer, body, code and payload alive. Failed
            // source/target authentication simply returns Continue to managed.
            GuardCap();
            // Link failures are Continue, whereas per-instruction failures are
            // Invalidated. Keep all mutable source checks after the final commit;
            // the retained owner already authenticated immutable source identity.
            GuardHeader(FullBlockExit.Continue);
            Bytes(0x4d, 0x8b, 0x42, 96, 0x4d, 0x01, 0xc8); // r8 = source header + edge offset
            Bytes(0x49, 0x8b, 0x48, 64); // rcx=target header, selected RW link cell
            Bytes(0x48, 0x85, 0xc9);
            ExitCondition(0x84, FullBlockExit.Continue);
            Bytes(0x49, 0x8b, 0x50, 96); // selected source edge PC
            Bytes(0x49, 0x3b, 0x13); // == actual next machine.ip
            ExitCondition(0x85, FullBlockExit.Continue);
            Bytes(0x48, 0x3b, 0x51, 16); // == target.GuestPc
            ExitCondition(0x85, FullBlockExit.Continue);
            Bytes(0x83, 0x79, 40, 1); // target.Live == 1
            ExitCondition(0x85, FullBlockExit.Continue);
            Bytes(0x48, 0x8b, 0x11, 0x49, 0x3b, 0x52, 104); // target owner == frame owner
            ExitCondition(0x85, FullBlockExit.Continue);
            Bytes(0x48, 0x8b, 0x51, 32, 0x49, 0x3b, 0x52, 120); // context
            ExitCondition(0x85, FullBlockExit.Continue);
            Bytes(0x49, 0x8b, 0x50, 80); // selected source target-generation snapshot
            Bytes(0x48, 0x3b, 0x51, 8);
            ExitCondition(0x85, FullBlockExit.Continue);
            Bytes(0x48, 0x8b, 0x51, 112, 0x48, 0x85, 0xd2);
            ExitCondition(0x84, FullBlockExit.Continue);
            Bytes(0x48, 0x3b, 0x51, 120); // target.CodeEpoch == ValidatedCodeEpoch
            ExitCondition(0x85, FullBlockExit.Continue);
            Bytes(0x49, 0x8b, 0x92); U32(376); // observed code/mapping epoch
            Bytes(0x48, 0x3b, 0x91); U32(128); // target.ValidatedGlobalEpoch
            ExitCondition(0x85, FullBlockExit.Continue);
            Bytes(0x49, 0x8b, 0x82); U32(368); // code epoch pointer checked by source guard
            Bytes(0x48, 0x3b, 0x10);
            ExitCondition(0x85, FullBlockExit.Continue);
            Bytes(0x48, 0x8b, 0x41, 24, 0x48, 0x85, 0xc0); // authenticated immutable Entry
            ExitCondition(0x84, FullBlockExit.Continue);
            Bytes(0x49, 0x89, 0x4a, 96); // adopt target header
            Bytes(0x48, 0x8b, 0x51, 8, 0x49, 0x89, 0x52, 112); // adopt target body generation
            Bytes(0x41, 0xc7, 0x82); U32(144); U32(0); // target starts at instruction 0
            Bytes(0x49, 0xff, 0x82); U32(160); // ++TailJumps, no managed allocation
            // RCX was used for target authentication. Restore exactly the ABI
            // argument register; Windows RDI is nonvolatile and never touched.
            Bytes(0x4c, 0x89, windowsAbi ? (byte)0xd1 : (byte)0xd7);
            Bytes(0xff, 0xe0); // JMP rax, target prologue must preserve total fuel
        }

        private void Operation(JitInstruction instruction, FullX87KernelCall kernel)
        {
            if (TryAddressExtensionOperation(in instruction)) return;
            if (TryCanonicalX87InvalidOperation(in instruction)) return;
            if (TryCanonicalX87MemoryArithmeticOperation(in instruction, in kernel)) return;
            if (TryCanonicalX87TranscendentalOperation(in instruction, in kernel)) return;
            if (TryCanonicalX87ControlOperation(in instruction, in kernel)) return;
            if (TryCanonicalX87MemoryOperation(in instruction, in kernel)) return;
            if (TryCanonicalX87ArithmeticOperation(in instruction, in kernel)) return;
            if (TryCanonicalX87StackOperation(in instruction)) return;
            if (TryChecksumOperation(in instruction)) return;
            if (TryFlagStackOperation(in instruction)) return;
            if (TryProcessorQueryOperation(in instruction)) return;
            if (TryProcessorEnvironmentOperation(in instruction)) return;
            if (TryResidualScalarOperation(in instruction)) return;
            if (TrySystemStateOperation(in instruction)) return;
            if (TryBitCountOperation(in instruction)) return;
            if (TryScalarMiscOperation(in instruction)) return;
            if (TryStringOperation(in instruction)) return;
            if (TryControlFlagOperation(in instruction)) return;
            if (TryHintOperation(in instruction)) return;
            if (TryAtomicOperation(in instruction)) return;
            if (TryFloatExtendedOperation(in instruction)) return;
            if (TryFloatOperation(in instruction)) return;
            if (TryEnterOperation(in instruction)) return;
            if (TryMaskedStoreOperation(in instruction)) return;
            if (TryMmxOperation(in instruction)) return;
            if (TryVectorExtendedOperation(in instruction)) return;
            if (TryVectorMoveOperation(in instruction)) return;
            if (TryVectorOperation(in instruction)) return;
            if (TryStackMemoryOperation(in instruction)) return;
            if (TryMemoryIntegerOperation(in instruction)) return;
            if (TryMemoryOperation(in instruction) || IntegerOperation(in instruction)) return;
            JitOperation op = instruction.Operation;
            if (op is JitOperation.Jump or JitOperation.Nop) return;
            int width = instruction.Width;
            if (op == JitOperation.Lea)
            {
                Address(instruction);
                Bytes(0x49, 0x89, 0xc0); // r8=rax
                Store(instruction.Destination, width);
                return;
            }
            if (op is JitOperation.Movzx or JitOperation.Movsx)
            {
                Load(instruction.Source, instruction.SourceWidth, 0);
                if (op == JitOperation.Movsx)
                {
                    if (instruction.SourceWidth == 32) Bytes(0x48, 0x63, 0xc0); // movsxd rax,eax
                    else Bytes(0x48, 0x0f, instruction.SourceWidth == 8 ? (byte)0xbe : (byte)0xbf, 0xc0);
                }
                // A 32-bit destination zeroes its high half even for MOVSX.
                if (width == 32) Bytes(0x89, 0xc0);
                Bytes(0x49, 0x89, 0xc0);
                Store(instruction.Destination, width);
                return;
            }
            if (op == JitOperation.Mov)
            {
                Load(instruction.Source, width, 0);
                Bytes(0x49, 0x89, 0xc0); // r8=rax
                Store(instruction.Destination, width);
                return;
            }
            throw new ArgumentException("Unsupported native operation");
        }

        private void Address(JitInstruction instruction, bool truncate = true)
        {
            var address = instruction.Address;
            ulong constant = unchecked((ulong)address.Displacement +
                (address.RipRelative ? instruction.Ip + (uint)instruction.Length : 0));
            Bytes(0x48, 0xb8); U64(constant); // rax = signed displacement / next RIP
            if (address.Base >= 0)
            {
                Load(new(address.Base, 0), 64, 1);
                Bytes(0x48, 0x01, 0xc8); // add rax,rcx
            }
            if (address.Index >= 0)
            {
                Load(new(address.Index, 0), 64, 1);
                if (address.Scale != 0) Bytes(0x48, 0xc1, 0xe1, (byte)address.Scale);
                Bytes(0x48, 0x01, 0xc8);
            }
            if (truncate && instruction.Width == 32) Bytes(0x89, 0xc0); // LEA modulo 2^32, zero extended
        }

        private void Load(JitOperand operand, int width, int register)
        {
            if (operand.IsImmediate)
            {
                if (width == 32) { Bytes((byte)(0xb8 + register)); U32((uint)operand.Immediate); }
                else { Bytes(0x48, (byte)(0xb8 + register)); U64(operand.Immediate); }
                return;
            }
            int offset = 24 + operand.Register * 8 + operand.ByteOffset;
            byte rex = width == 64 ? (byte)0x49 : (byte)0x41;
            Bytes(rex);
            if (width is 8 or 16) Bytes(0x0f, width == 8 ? (byte)0xb6 : (byte)0xb7);
            else Bytes(0x8b);
            Memory(register, 3, offset); // reg <- [r11+offset]
        }

        private void Store(JitOperand operand, int width)
        {
            if (TryStoreMemoryInteger(operand, width)) return;
            int offset = 24 + operand.Register * 8 + operand.ByteOffset;
            if (width == 16) Bytes(0x66);
            Bytes(width is 32 or 64 ? (byte)0x4d : (byte)0x45, width == 8 ? (byte)0x88 : (byte)0x89);
            Memory(0, 3, offset); // [r11+offset] <- r8 (32-bit guest writes zero extend all 64 bits)
        }

        private void Memory(int reg, int baseLow, int offset)
        {
            if (offset < 128) Bytes((byte)(0x40 | reg << 3 | baseLow), (byte)offset);
            else { Bytes((byte)(0x80 | reg << 3 | baseLow)); U32((uint)offset); }
        }

        private void Condition(int condition)
        {
            Bytes(0x41, 0x8b, 0x43, 0x0c); // eax=flags, never install guest bits in RFLAGS
            switch (condition >> 1)
            {
                case 0: Bytes(0xc1, 0xe8, 11); break; // OF
                case 1: break; // CF
                case 2: Bytes(0xc1, 0xe8, 6); break; // ZF
                case 3: Bytes(0x89, 0xc1, 0xc1, 0xe9, 6, 0x09, 0xc8); break; // CF|ZF
                case 4: Bytes(0xc1, 0xe8, 7); break; // SF
                case 5:
                    Bytes(0xc1, 0xe8, 24, 0x84, 0xc0, 0x0f, 0x9a, 0xc0, 0x0f, 0xb6, 0xc0); // lazy parity
                    break;
                case 6: // SF != OF
                case 7: // (SF != OF) | ZF
                    if ((condition >> 1) == 7) Bytes(0x89, 0xc2, 0xc1, 0xea, 6, 0x83, 0xe2, 1);
                    Bytes(0x89, 0xc1, 0xc1, 0xe9, 4, 0x31, 0xc8, 0xc1, 0xe8, 7);
                    if ((condition >> 1) == 7) Bytes(0x09, 0xd0);
                    break;
            }
            Bytes(0x83, 0xe0, 1);
            if ((condition & 1) != 0) Bytes(0x83, 0xf0, 1);
        }

        private int NearCondition(byte condition)
        { Bytes(0x0f, condition); int at = position; U32(0); return at; }
        private void ExitCondition(byte condition, FullBlockExit exit)
        { Bytes(0x0f, condition); Reference(MaximumInstructions + (int)exit); }
        private void JumpExit(FullBlockExit exit)
        { Bytes(0xe9); Reference(MaximumInstructions + (int)exit); }

        private void Reference(int label)
        {
            int at = position;
            // Negative one terminates the in-buffer forward-reference chain.
            // Bound labels can be patched immediately, including backedges.
            U32(unchecked((uint)fixupHeads[label]));
            if (oversized) return;
            if (labels[label] >= 0) Patch(at, labels[label]);
            else fixupHeads[label] = at;
        }

        private void Bind(int label)
        {
            if (oversized) return;
            int at = fixupHeads[label];
            while (at >= 0)
            {
                int next = BinaryPrimitives.ReadInt32LittleEndian(code.Slice(at, 4));
                Patch(at, position);
                at = next;
            }
            fixupHeads[label] = -1;
            labels[label] = position;
        }

        private void Patch(int at, int target)
        {
            if (oversized) return;
            int displacement = checked(target - (at + 4));
            BinaryPrimitives.WriteInt32LittleEndian(code.Slice(at, 4), displacement);
        }

        private Span<byte> Reserve(int count)
        {
            if (oversized || count > code.Length - position)
            {
                oversized = true;
                return overflowBytes[..count]; // fixed-arity writes, never read as fixups
            }
            Span<byte> bytes = code.Slice(position, count);
            position += count;
            return bytes;
        }

        // Fixed arities keep literal byte sequences allocation-free on both runtimes.
        private void Bytes(byte b0)
        {
            Span<byte> bytes = Reserve(1);
            bytes[0] = b0;
        }
        private void Bytes(byte b0, byte b1)
        {
            Span<byte> bytes = Reserve(2);
            bytes[0] = b0; bytes[1] = b1;
        }
        private void Bytes(byte b0, byte b1, byte b2)
        {
            Span<byte> bytes = Reserve(3);
            bytes[0] = b0; bytes[1] = b1; bytes[2] = b2;
        }
        private void Bytes(byte b0, byte b1, byte b2, byte b3)
        {
            Span<byte> bytes = Reserve(4);
            bytes[0] = b0; bytes[1] = b1; bytes[2] = b2; bytes[3] = b3;
        }
        private void Bytes(byte b0, byte b1, byte b2, byte b3, byte b4)
        {
            Span<byte> bytes = Reserve(5);
            bytes[0] = b0; bytes[1] = b1; bytes[2] = b2; bytes[3] = b3;
            bytes[4] = b4;
        }
        private void Bytes(byte b0, byte b1, byte b2, byte b3, byte b4, byte b5)
        {
            Span<byte> bytes = Reserve(6);
            bytes[0] = b0; bytes[1] = b1; bytes[2] = b2; bytes[3] = b3;
            bytes[4] = b4; bytes[5] = b5;
        }
        private void Bytes(byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6)
        {
            Span<byte> bytes = Reserve(7);
            bytes[0] = b0; bytes[1] = b1; bytes[2] = b2; bytes[3] = b3;
            bytes[4] = b4; bytes[5] = b5; bytes[6] = b6;
        }
        private void Bytes(byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7)
        {
            Span<byte> bytes = Reserve(8);
            bytes[0] = b0; bytes[1] = b1; bytes[2] = b2; bytes[3] = b3;
            bytes[4] = b4; bytes[5] = b5; bytes[6] = b6; bytes[7] = b7;
        }
        private void Bytes(byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7, byte b8)
        {
            Span<byte> bytes = Reserve(9);
            bytes[0] = b0; bytes[1] = b1; bytes[2] = b2; bytes[3] = b3;
            bytes[4] = b4; bytes[5] = b5; bytes[6] = b6; bytes[7] = b7;
            bytes[8] = b8;
        }
        private void Bytes(byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7, byte b8, byte b9)
        {
            Span<byte> bytes = Reserve(10);
            bytes[0] = b0; bytes[1] = b1; bytes[2] = b2; bytes[3] = b3;
            bytes[4] = b4; bytes[5] = b5; bytes[6] = b6; bytes[7] = b7;
            bytes[8] = b8; bytes[9] = b9;
        }
        private void Bytes(byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7, byte b8, byte b9, byte b10)
        {
            Span<byte> bytes = Reserve(11);
            bytes[0] = b0; bytes[1] = b1; bytes[2] = b2; bytes[3] = b3;
            bytes[4] = b4; bytes[5] = b5; bytes[6] = b6; bytes[7] = b7;
            bytes[8] = b8; bytes[9] = b9; bytes[10] = b10;
        }

        private void U32(uint value)
            => BinaryPrimitives.WriteUInt32LittleEndian(Reserve(4), value);
        private void U64(ulong value)
            => BinaryPrimitives.WriteUInt64LittleEndian(Reserve(8), value);
    }
}
