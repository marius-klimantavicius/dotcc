using System;
using MX = Managed.Emulation.Execution.FullCanonicalX87MemoryOperation;

namespace Managed.Emulation.Execution;

internal static partial class FullBlockEmitter
{
    internal static bool CanInlineCanonicalX87Memory(JitInstruction i) =>
        i.Operation == JitOperation.X87 && (i.Options & 0xffff) >= (uint)MX.LoadBinary32 &&
        (i.Options & 0xffff) <= (uint)MX.StoreRaw80;

    internal static bool ValidateCanonicalX87Memory(JitInstruction i)
    {
        if (!CanInlineCanonicalX87Memory(i)) return false;
        var op = (MX)(i.Options & 0xffff);
        uint fop = (uint)i.Extra.Immediate;
        byte modrm = (byte)fop;
        bool load = FullCanonicalX87MemoryLowering.IsLoad(op);
        var memory = new JitOperand(0, 0, IsMemory: true);
        if (i.Extra != new JitOperand(0, fop, IsImmediate: true) || fop > 0x7ff ||
            !FullCanonicalX87MemoryLowering.Decode((byte)(0xd8 | fop >> 8), modrm, out var decoded, out int width, out bool pop) ||
            decoded != op || i.Options != ((uint)op | (pop ? FullCanonicalX87MemoryLowering.Pop : 0)) ||
            i.Length is < 2 or > 15 || i.Width != 80 || i.SourceWidth != width ||
            i.Source != (load ? memory : default) || i.Destination != (load ? default : memory) ||
            i.Target != 0 || i.Condition != 0 || !CanonicalX87MemoryAddressShape(i.Address, modrm, i.Length))
            throw new ArgumentException("Invalid canonical x87 memory IR");
        if (!CanonicalX87StackLayout.Supported || !ScalarMiscLayout.Supported)
            throw new PlatformNotSupportedException("Canonical x87 memory layout differs");
        return true;
    }

    private static bool CanonicalX87MemoryAddressShape(JitAddress a, byte modrm, int length)
    {
        if (a.Base is < -1 or > 15 || a.Index is < -1 or > 15 || a.Scale is < 0 or > 3 ||
            a.AddressWidth is not (32 or 64) || a.Segment is < -1 or > 5 ||
            a.Displacement < int.MinValue || a.Displacement > int.MaxValue) return false;
        int mod = modrm >> 6, rm = modrm & 7, minimum = 2;
        if (mod == 3) return false;
        if (mod == 1) { ++minimum; if (a.Displacement is < -128 or > 127) return false; }
        if (mod == 2) minimum += 4;
        if (rm == 4)
        {
            ++minimum;
            if (a.RipRelative || a.Index == 4) return false; // R12 is valid with REX.X
            if (a.Base == -1) { if (mod != 0) return false; minimum += 4; }
            else if (mod == 0 && (a.Base & 7) == 5) return false;
        }
        else if (mod == 0 && rm == 5)
        {
            minimum += 4;
            if (!a.RipRelative || a.Base != -1 || a.Index != -1 || a.Scale != 0) return false;
        }
        else if (a.RipRelative || a.Base < 0 || (a.Base & 7) != rm || a.Index != -1 || a.Scale != 0) return false;
        if (mod == 0 && !(rm == 5 || rm == 4 && a.Base == -1) && a.Displacement != 0) return false;
        return length >= minimum;
    }

    internal static FullX87KernelKey CanonicalX87MemoryKernel(JitInstruction i)
    {
        if (!ValidateCanonicalX87Memory(i)) throw new ArgumentException("Not canonical x87 memory");
        return ((MX)(i.Options & 0xffff)) switch
        {
            MX.LoadBinary32 => FullX87KernelKey.LoadBinary32, MX.LoadBinary64 => FullX87KernelKey.LoadBinary64,
            MX.LoadInt16 => FullX87KernelKey.LoadInt16, MX.LoadInt32 => FullX87KernelKey.LoadInt32,
            MX.LoadInt64 => FullX87KernelKey.LoadInt64, MX.LoadRaw80 => FullX87KernelKey.LoadRaw80,
            MX.StoreBinary32 => FullX87KernelKey.StoreBinary32, MX.StoreBinary64 => FullX87KernelKey.StoreBinary64,
            MX.StoreInt16 => FullX87KernelKey.StoreInt16, MX.StoreInt32 => FullX87KernelKey.StoreInt32,
            MX.StoreInt64 => FullX87KernelKey.StoreInt64, MX.StoreRaw80 => FullX87KernelKey.StoreRaw80,
            MX.TruncateInt16 => FullX87KernelKey.TruncateInt16, MX.TruncateInt32 => FullX87KernelKey.TruncateInt32,
            _ => FullX87KernelKey.TruncateInt64
        };
    }

    internal static bool ValidateCanonicalX87Memory(JitInstruction i, in FullX87KernelCall call)
    {
        if (!ValidateCanonicalX87Memory(i)) return false;
        if (call.Key != (byte)CanonicalX87MemoryKernel(i) || call.Entry == 0 || call.OwnerCookie == 0 ||
            call.CodeBytes <= 0 || call.CodeBytes > FullX87KernelCatalog.StagingBytes)
            throw new ArgumentException("Invalid canonical x87 memory kernel call");
        // Parent MUST additionally authenticate RX owner/binding under Gate and
        // exact frame-owned576 identity before Issue. A tuple proves no lifetime.
        return true;
    }
    private ref partial struct Writer
    {
        // Parent pre-Issue seam authenticates Work/owner/header, inline context,
        // and reserves Issue. No native/managed semantic helper occurs here.
        private bool TryCanonicalX87MemoryOperation(in JitInstruction i, in FullX87KernelCall call)
        {
            if (!CanInlineCanonicalX87Memory(i)) return false;
            ValidateCanonicalX87Memory(i, in call);
            var op = (MX)(i.Options & 0xffff);
            bool load = FullCanonicalX87MemoryLowering.IsLoad(op);
            bool pop = (i.Options & FullCanonicalX87MemoryLowering.Pop) != 0;
            FloatMmxWaitingException(i); // six masks, prior #MF before address checks

            int stack = -1, emptyJoin = -1;
            if (load)
            {
                ScalarMiscCheckCanonical(i, i.SourceWidth);
                NativeMemoryPrepare(i, FullMemoryAccess.Read, i.SourceWidth);
                CanonicalX87MemoryGather(i.SourceWidth); // XMM0, exact span only
                stack = CanonicalX87PushCheck();
                CanonicalX87PrepareWork(0);
                CanonicalX87WorkConstant(288, 0); // no stack-underflow delta
            }
            else
            {
                stack = CanonicalX87SourceCheck(0);
                CanonicalX87LoadRaw(0, 0);
                CanonicalX87PrepareWork(0);
                CanonicalX87WorkConstant(288, 0);
                emptyJoin = IntegerJump();
                Patch(stack, position);
                // Stage empty-source indefinite without changing guest SW/TOP.
                // Memory must be checked before a MASKED empty-source write.
                CanonicalX87Indefinite(0); CanonicalX87PrepareWork(0);
                CanonicalX87WorkConstant(288, 0x41); // IE/SF even for raw-copy kernel
                CanonicalX87MachineLoad(672, 0); IntegerConstant(4, 0, 1);
                int maskedEmpty = NearCondition(0x85);
                ScalarMiscCheckCanonical(i, i.SourceWidth);
                CanonicalX87MemoryProbe(i);
                CanonicalX87MemoryMetadata(i);
                int suppressedEmpty = CanonicalX87StackFault(false);
                // IM was clear, so the branch always suppresses the write/pop.
                // Resolve it at the common end after emitting the numeric body.
                int emptyDone = IntegerJump();
                Patch(maskedEmpty, position); Patch(emptyJoin, position);
                CanonicalX87Invoke(in call); CanonicalX87RequireComplete();
                CanonicalX87MemoryRequireResult(op);
                CanonicalX87WorkLoad(84, 0, 32); IntegerConstant(4, 0, 512);
                int suppressed = NearCondition(0x85);

                // Conversion result/status remain in exclusive Work through any
                // mapping-only miss. A resumed SAME issue recomputes from the
                // unchanged source. No guest state is committed on that miss.
                ScalarMiscCheckCanonical(i, i.SourceWidth);
                NativeMemoryPrepare(i, FullMemoryAccess.Write, i.SourceWidth);
                CanonicalX87MemoryWorkAddress();
                CanonicalX87WorkRaw(48, 0);
                CanonicalX87MemoryScatter(i.SourceWidth);
                CanonicalX87MemoryMetadata(i);
                CanonicalX87MemoryMergeStatus();
                if (pop) CanonicalX87Pop();
                NativeMemoryCount();
                int completedStore = IntegerJump();
                Patch(suppressed, position);
                ScalarMiscCheckCanonical(i, i.SourceWidth);
                CanonicalX87MemoryProbe(i);
                CanonicalX87MemoryMetadata(i); CanonicalX87MemoryMergeStatus();
                Patch(suppressedEmpty, position); Patch(emptyDone, position);
                Patch(completedStore, position);
                return true;
            }

            CanonicalX87Invoke(in call); CanonicalX87RequireComplete();
            CanonicalX87MemoryRequireResult(op);
            CanonicalX87MemoryMetadata(i); CanonicalX87MemoryMergeStatus();
            CanonicalX87WorkLoad(84, 0, 32); IntegerConstant(4, 0, 512);
            int suppressedLoad = NearCondition(0x85);
            CanonicalX87WorkRaw(48, 0); CanonicalX87PushRaw(0);
            int completedLoad = IntegerJump();
            Patch(stack, position);
            CanonicalX87MemoryMetadata(i);
            int suppressedFull = CanonicalX87StackFault(true);
            CanonicalX87Indefinite(0); CanonicalX87PushRaw(0);
            Patch(suppressedLoad, position); Patch(completedLoad, position); Patch(suppressedFull, position);
            NativeMemoryCount();
            return true;
        }

        private void CanonicalX87MemoryWorkAddress()
        { IntegerFrameLoad(400, 9); IntegerConstant(0, 9, 192, 64); }

        // A suppressed x87 store STILL incurs canonical/RW/PF/EOF checks, but
        // must not borrow a physical writer or revoke code aliases. The parent
        // resolves this exact tagged proof using PTE/backing inspection only.
        // No persistent Work input/result is needed for continuation: resuming
        // the SAME issued instruction recomputes the pure staged result first.
        private void CanonicalX87MemoryProbe(in JitInstruction i)
        {
            NativeStackMemoryDescriptor(); // R9=existing owned128 descriptor
            Bytes(0x41, 0x81, 0x39); U32(FullCanonicalX87MemoryLowering.MemoryProbeTag);
            int wrongTag = NearCondition(0x85);
            Bytes(0x41, 0x83, 0x79, 4, 1); int wrongCount = NearCondition(0x85);
            Bytes(0x41, 0x83, 0x79, 12, (byte)FullStackMemoryState.Ready);
            int ready = NearCondition(0x84);
            Patch(wrongTag, position); Patch(wrongCount, position);
            NativeMemoryAddress(i);
            IntegerFrameStore(216, 0); // exact request summary (not faultaddr)
            Bytes(0x49, 0x89, 0x41, 32); // Request0.Address
            NativeStackMemoryDword(0, FullCanonicalX87MemoryLowering.MemoryProbeTag);
            NativeStackMemoryDword(4, 1);
            Bytes(0x41, 0x8b, 0x82); U32(144); Bytes(0x41, 0x89, 0x41, 8);
            NativeStackMemoryDword(12, (uint)FullStackMemoryState.Request);
            NativeStackMemoryZeroPointer(16); NativeStackMemoryZeroPointer(24);
            // Everything besides the exact EA/Access/Width is explicitly zero.
            for (int offset = 40; offset < 128; offset += 8) NativeStackMemoryZeroPointer(offset);
            NativeStackMemoryDword(56, (uint)FullMemoryAccess.Write);
            NativeStackMemoryDword(60, (uint)i.SourceWidth);
            Bytes(0x41, 0x8b, 0x82); U32(144); Bytes(0x41, 0x89, 0x82); U32(248);
            Bytes(0x41, 0xc7, 0x82); U32(252); U32((uint)FullMemoryAccess.Write);
            Bytes(0x41, 0xc7, 0x82); U32(256); U32((uint)i.SourceWidth);
            Bytes(0x41, 0xc7, 0x82); U32(292); U32(1);
            Bytes(0x41, 0xc7, 0x82); U32(324); U32(0);
            Bytes(0x49, 0xc7, 0x82); U32(344); U32(0);
            JumpExit(FullBlockExit.NeedMemory);

            Patch(ready, position);
            Bytes(0x41, 0x8b, 0x82); U32(144); Bytes(0x41, 0x3b, 0x41, 8);
            ExitCondition(0x85, FullBlockExit.Invalidated);
            Bytes(0x41, 0x3b, 0x82); U32(248); ExitCondition(0x85, FullBlockExit.Invalidated);
            Bytes(0x41, 0x83, 0xba); U32(292); Bytes(1); ExitCondition(0x85, FullBlockExit.Invalidated);
            NativeMemoryAddress(i); Bytes(0x49, 0x3b, 0x41, 32); ExitCondition(0x85, FullBlockExit.Invalidated);
            Bytes(0x49, 0x3b, 0x82); U32(216); ExitCondition(0x85, FullBlockExit.Invalidated);
            Bytes(0x41, 0x81, 0xba); U32(252); U32((uint)FullMemoryAccess.Write); ExitCondition(0x85, FullBlockExit.Invalidated);
            Bytes(0x41, 0x81, 0xba); U32(256); U32((uint)i.SourceWidth); ExitCondition(0x85, FullBlockExit.Invalidated);
            Bytes(0x41, 0x83, 0xba); U32(324); Bytes(0); ExitCondition(0x85, FullBlockExit.Invalidated);
            Bytes(0x49, 0x83, 0xba); U32(344); Bytes(0); ExitCondition(0x85, FullBlockExit.Invalidated);
            Bytes(0x41, 0x83, 0x79, 56, (byte)FullMemoryAccess.Write); ExitCondition(0x85, FullBlockExit.Invalidated);
            Bytes(0x41, 0x83, 0x79, 60, (byte)i.SourceWidth); ExitCondition(0x85, FullBlockExit.Invalidated);
            for (int offset = 24; offset < 128; offset += 8)
            {
                if (offset is 32 or 56) continue;
                Bytes(0x49, 0x83, 0x79, (byte)offset, 0); ExitCondition(0x85, FullBlockExit.Invalidated);
            }
            Bytes(0x49, 0x8b, 0x41, 16); Bytes(0x49, 0x3b, 0x82); U32(136);
            int stale = NearCondition(0x85);
            Bytes(0x49, 0x8b, 0x92); U32(128); IntegerRR(0x85, 2, 2); ExitCondition(0x84, FullBlockExit.Invalidated);
            Bytes(0x48, 0x3b, 0x02); int current = NearCondition(0x84);
            Patch(stale, position);
            NativeStackMemoryDword(12, (uint)FullStackMemoryState.Request);
            Bytes(0x41, 0xc7, 0x82); U32(292); U32(1);
            JumpExit(FullBlockExit.NeedMemory);
            Patch(current, position);
            NativeStackMemoryDword(12, (uint)FullStackMemoryState.Consumed);
            Bytes(0x41, 0xc7, 0x82); U32(292); U32(0);
            CanonicalX87MemoryWorkAddress(); // restore conversion Work base
        }

        // All pages are checked BEFORE the first guest byte read or write.
        // NativeBytePointer preserves XMM and R9. The 80-bit path never asks
        // VectorMemoryLoad/Store to interpret an unsupported SIMD width.
        private void CanonicalX87MemoryGather(int width)
        {
            if (width != 80)
            { VectorMemoryLoad(width); Bytes(0x66, 0x0f, 0x6f, 0xc1); return; }
            Bytes(0x49, 0xc7, 0x82); U32(184); U32(0);
            Bytes(0x49, 0xc7, 0x82); U32(192); U32(0);
            for (int b = 0; b < 10; ++b)
            {
                NativeBytePointer(b); Bytes(0x8a, 0x02, 0x41, 0x88, 0x82); U32((uint)(184 + b));
            }
            VectorTransfer(0xf3, 0x6f, 0, 10, 184);
        }

        private void CanonicalX87MemoryScatter(int width)
        {
            if (width != 80) { VectorMemoryStore(width); return; }
            VectorTransfer(0xf3, 0x7f, 0, 10, 184);
            for (int b = 0; b < 10; ++b)
            {
                NativeBytePointer(b); Bytes(0x41, 0x8a, 0x82); U32((uint)(184 + b)); Bytes(0x88, 0x02);
            }
        }

        // Selected 64-bit non-deprecated pointer profile: full linear FDP,
        // FIP includes prefixes, complete FOP, visible effective FCS/FDS.
        // DP-only-on-exception/deprecated-selectors CPU profiles remain MISSING.
        private void CanonicalX87MemoryMetadata(in JitInstruction i)
        {
            NativeMemoryAddress(i); CanonicalX87MachineStore(680, 0, 64);
            CanonicalX87Metadata(i);
            int segment = i.Address.Segment >= 0 ? i.Address.Segment :
                FullScalarMiscLowering.UsesStackSegment(i.Address) ? 2 : 3;
            Bytes(0x41, 0x0f, 0xb7, 0x83); U32((uint)(456 + segment * 16));
            IntegerFrameLoad(400, 1); Bytes(0x66, 0x89, 0x41, 42);
        }

        private void CanonicalX87MemoryMergeStatus()
        {
            CanonicalX87MemoryWorkAddress();
            CanonicalX87WorkLoad(82, 2, 16); CanonicalX87WorkLoad(288, 0, 32);
            IntegerRR(0x09, 2, 0, 32);
            CanonicalX87MachineLoad(648, 0); IntegerConstant(4, 0, ~0x200u);
            IntegerRR(0x09, 0, 2, 32);
            CanonicalX87MachineLoad(672, 1); IntegerUnary(2, 1, 32);
            IntegerRR(0x21, 1, 2, 32); IntegerConstant(4, 1, 63);
            int masked = NearCondition(0x84); IntegerConstant(1, 0, 0x8080);
            Patch(masked, position); CanonicalX87MachineStore(648, 0);
        }

        private void CanonicalX87MemoryRequireResult(MX op)
        {
            bool raw = op is MX.LoadRaw80 or MX.StoreRaw80;
            bool integerLoad = op is MX.LoadInt16 or MX.LoadInt32 or MX.LoadInt64;
            bool floatingLoad = op is MX.LoadBinary32 or MX.LoadBinary64;
            bool floatingStore = op is MX.StoreBinary32 or MX.StoreBinary64;
            bool truncate = op is MX.TruncateInt16 or MX.TruncateInt32 or MX.TruncateInt64;
            uint allowedStatus = raw || integerLoad ? 0u : floatingLoad ? 3u : floatingStore ? 0x239u : truncate ? 0x21u : 0x221u;
            uint allowedMetadata = raw ? 7u : integerLoad ? 3u : floatingLoad ? 0xa1bu : floatingStore ? 0x7ebu : 0x78bu;
            CanonicalX87WorkLoad(84, 0, 32); IntegerConstant(4, 0, 1);
            int noValue = NearCondition(0x84);
            CanonicalX87WorkLoad(84, 0, 32); IntegerConstant(4, 0, ~allowedMetadata);
            int badMetadata = NearCondition(0x85);
            CanonicalX87WorkLoad(80, 0, 16); IntegerConstant(4, 0, ~allowedStatus);
            int badNormal = NearCondition(0x85);
            CanonicalX87WorkLoad(82, 0, 16); IntegerConstant(4, 0, ~allowedStatus);
            int badEvent = NearCondition(0x85);
            int valid = IntegerJump();
            Patch(noValue, position); Patch(badMetadata, position); Patch(badNormal, position); Patch(badEvent, position);
            CanonicalX87WorkConstant(284, 0x80000001); CanonicalX87ProtocolFatal();
            Patch(valid, position);
        }
    }
}
