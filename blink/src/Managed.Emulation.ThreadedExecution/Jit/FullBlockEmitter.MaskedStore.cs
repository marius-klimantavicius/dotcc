using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

internal enum FullMaskedStoreState : uint { Empty = 0, Request = 1, Ready = 2, TicketReady = 3, Consumed = 4 }

// R120 compatible worker union at frame.StackMemory384. NOT a dense request.
// Parent MUST dispatch TagVersion+Kind before examining R85 Count/requests.
// Host pointers are page BASES, independently retained even on TLB collisions.
// Page0 is the first SELECTED page; second-only geometry compacts into slot0.
[StructLayout(LayoutKind.Explicit, Size = FullMaskedStoreDescriptor.ByteSize)]
internal unsafe struct FullMaskedStoreDescriptor
{
    internal const int ByteSize = 128;
    internal const uint CurrentTagVersion = 0x53500120;
    internal const uint SparseKind = 0x53500002;
    internal const uint NoDataAddressFaultsForZeroMask = 1u << 24;
    [FieldOffset(0)] public uint TagVersion;
    [FieldOffset(4)] public uint Kind;
    [FieldOffset(8)] public uint Site;
    [FieldOffset(12)] public FullMaskedStoreState State;
    [FieldOffset(16)] public ulong Epoch;
    [FieldOffset(24)] public ulong Ticket;
    [FieldOffset(32)] public ulong BaseEA;
    [FieldOffset(40)] public ulong DataLo;
    [FieldOffset(48)] public ulong DataHi;
    [FieldOffset(56)] public ulong MaskLo;
    [FieldOffset(64)] public ulong MaskHi;
    [FieldOffset(72)] public uint EnabledBits;
    [FieldOffset(76)] public uint Shape;
    [FieldOffset(80)] public byte* HostPage0;
    [FieldOffset(88)] public byte* HostPage1;
    [FieldOffset(96)] public ulong Page0;
    [FieldOffset(104)] public ulong Page1;
    [FieldOffset(112)] public uint Selected0;
    [FieldOffset(116)] public uint Selected1;
    [FieldOffset(120)] public ulong Ip;
}

internal static partial class FullBlockEmitter
{
    internal static bool CanInlineMaskedStore(JitInstruction i) => i.Operation == JitOperation.VectorInteger &&
        i.Options is FullMaskedStoreLowering.Mmx or FullMaskedStoreLowering.Xmm;

    internal static uint MaskedStoreShape(JitInstruction i) =>
        (uint)(i.Width / 8 | i.Source.Register << 8 | i.Extra.Register << 12 |
        (i.Address.AddressWidth == 32 ? 1 << 16 : 0) | (i.Address.Segment + 1) << 17) |
        FullMaskedStoreDescriptor.NoDataAddressFaultsForZeroMask;

    internal static unsafe bool HasExpectedMaskedStoreLayout()
    {
        Blink.Machine m = default;
        return HasExpectedMmxLayout() && sizeof(FullMaskedStoreDescriptor) == 128 &&
            sizeof(FullStackMemoryDescriptor) == 128 && sizeof(FullBlockFrame) == 416 &&
            FullBlockFrame.AbiVersion == 0x46420007 && sizeof(TranslationBlockHeader) == 160 &&
            Unsafe.ByteOffset(ref Unsafe.As<Blink.Machine, byte>(ref m), ref Unsafe.As<uint, byte>(ref m.fpu.sw)) == 648 &&
            Unsafe.ByteOffset(ref Unsafe.As<Blink.Machine, byte>(ref m), ref Unsafe.As<int, byte>(ref m.fpu.tw)) == 652;
    }

    private static class MaskedStoreLayout
    {
        internal static readonly bool Supported = HasExpectedMaskedStoreLayout();
    }

    internal static bool ValidateMaskedStore(JitInstruction i)
    {
        if (!CanInlineMaskedStore(i)) return false;
        int width = i.Options == FullMaskedStoreLowering.Mmx ? 64 : 128;
        int last = width == 64 ? 7 : 15;
        static bool Register(JitOperand p, int last) => p.Register >= 0 && p.Register <= last &&
            p == new JitOperand(p.Register, 0);
        if (i.Length is < 3 or > 15 || i.Width != width || i.SourceWidth != width ||
            i.Target != 0 || i.Condition != 0 || i.Destination != new JitOperand(0, 0, IsMemory: true) ||
            !Register(i.Source, last) || !Register(i.Extra, last) || i.Address.Segment is < -1 or > 5 ||
            i.Address.AddressWidth is not (32 or 64) ||
            i.Address != new JitAddress(7, -1, 0, 0, false, i.Address.AddressWidth, i.Address.Segment))
            throw new ArgumentException("Native masked store exact implicit-DI grammar");
        if (!MaskedStoreLayout.Supported) throw new PlatformNotSupportedException("R120 MMX SW648/TW652/ABI6 layout");
        return true;
    }

    private ref partial struct Writer
    {
        // R10 frame, R11 guest machine, R9 union. Only volatile GPRs/XMM0..2.
        // No host MASKMOV: guest EAs never become unchecked host pointers.
        private bool TryMaskedStoreOperation(in JitInstruction i)
        {
            if (!CanInlineMaskedStore(i)) return false;
            ValidateMaskedStore(i);
            bool mmx = i.Width == 64;
            if (mmx) FloatMmxWaitingException(i); // BEFORE snapshots, descriptor or zero-mask effects
            if (mmx) { MmxLoad(0, i.Source.Register); MmxLoad(1, i.Extra.Register); }
            else { VectorGuestLoad(0, i.Source.Register); VectorGuestLoad(1, i.Extra.Register); }
            Bytes(0x66, 0x0f, 0xd7, 0xc1); // PMOVMSKB eax,xmm1 (SSE2 baseline)
            if (mmx) IntegerConstant(4, 0, 255, 32);
            IntegerFrameStore(192, 0);
            IntegerRR(0x85, 0, 0, 32); int zero = NearCondition(0x84);
            NativeMemoryAddress(i); IntegerFrameStore(184, 0); // EA32 BEFORE FS/GS
            MaskedStoreSelectPages(i); // typed selected-byte canonical precheck; no mapping/effects
            IntegerFrameLoad(384, 9); IntegerRR(0x85, 9, 9);
            ExitCondition(0x84, FullBlockExit.Invalidated);
            MaskedStoreSummary();
            Span<int> restage = stackalloc int[32]; int misses = 0; // translation-time relocations only
            MaskedStoreCheckDword(0, FullMaskedStoreDescriptor.CurrentTagVersion, restage, ref misses);
            MaskedStoreCheckDword(4, FullMaskedStoreDescriptor.SparseKind, restage, ref misses);
            Bytes(0x41, 0x83, 0x79, 12, (byte)FullMaskedStoreState.Ready);
            int ready = NearCondition(0x84);
            Bytes(0x41, 0x83, 0x79, 12, (byte)FullMaskedStoreState.TicketReady);
            restage[misses++] = NearCondition(0x85); Patch(ready, position);
            // Compare EXACT identity, including all source/mask bytes, even
            // disabled bytes. Runtime additionally authenticates plan/header/context.
            Bytes(0x41, 0x8b, 0x82); U32(144); Bytes(0x41, 0x3b, 0x41, 8);
            restage[misses++] = NearCondition(0x85);
            MaskedStoreCheckDword(76, MaskedStoreShape(i), restage, ref misses);
            IntegerImmediate(0, i.Ip); Bytes(0x49, 0x3b, 0x41, 120); restage[misses++] = NearCondition(0x85);
            MaskedStoreCheckFrame(32, 184, restage, ref misses);
            MaskedStoreCheckFrame(72, 192, restage, ref misses, false);
            MaskedStoreCheckFrame(112, 200, restage, ref misses, false);
            MaskedStoreCheckFrame(116, 208, restage, ref misses, false);
            MaskedStorePage(0); Bytes(0x49, 0x3b, 0x41, 96); restage[misses++] = NearCondition(0x85);
            MaskedStorePage(1); Bytes(0x49, 0x3b, 0x41, 104); restage[misses++] = NearCondition(0x85);
            MaskedStoreCheckSnapshot(40, 0, restage, ref misses); MaskedStoreCheckSnapshot(56, 1, restage, ref misses);
            Bytes(0x49, 0x8b, 0x41, 16, 0x49, 0x3b, 0x82); U32(136);
            restage[misses++] = NearCondition(0x85);
            Bytes(0x49, 0x8b, 0x92); U32(128); IntegerRR(0x85, 2, 2);
            ExitCondition(0x84, FullBlockExit.Invalidated);
            Bytes(0x48, 0x3b, 0x02); restage[misses++] = NearCondition(0x85);
            // At least one selected byte, therefore slot0 is always required.
            Bytes(0x49, 0x83, 0x79, 80, 0); restage[misses++] = NearCondition(0x84);
            Bytes(0x41, 0x83, 0x79, 116, 0); int one = NearCondition(0x84);
            Bytes(0x49, 0x83, 0x79, 88, 0); restage[misses++] = NearCondition(0x84);
            int pointers = NativeNearJump(); Patch(one, position);
            Bytes(0x49, 0x83, 0x79, 88, 0); restage[misses++] = NearCondition(0x85); Patch(pointers, position);
            Bytes(0x41, 0x83, 0x79, 12, (byte)FullMaskedStoreState.TicketReady);
            int ordinary = NearCondition(0x85);
            Bytes(0x49, 0x83, 0x79, 24, 0); restage[misses++] = NearCondition(0x84);
            Bytes(0x41, 0xc7, 0x82); U32(324); U32((uint)FullMemoryFlags.BarrierConsumed);
            int authorized = NativeNearJump(); Patch(ordinary, position);
            Bytes(0x49, 0x83, 0x79, 24, 0); restage[misses++] = NearCondition(0x85); Patch(authorized, position);
            // All identity, epoch, pointers and authorization checked BEFORE consume.
            MaskedStoreDword(12, (uint)FullMaskedStoreState.Consumed); MaskedStoreQwordZero(24);
            Bytes(0x41, 0xc7, 0x82); U32(292); U32(0);
            MaskedStoreBytes(i.Width / 8);
            int success = NativeNearJump();
            foreach (int at in restage[..misses]) Patch(at, position);
            MaskedStoreStage(i); JumpExit(FullBlockExit.NeedMemory);
            Patch(zero, position);
            // Explicit fixed profile, not a universal hardware suppression claim.
            // No EA evaluation/canonical check, descriptor requirement or MMU access.
            IntegerFrameLoad(384, 9); IntegerRR(0x85, 9, 9); int noDescriptor = NearCondition(0x84);
            MaskedStoreDword(4, 0); MaskedStoreDword(0, FullMaskedStoreDescriptor.CurrentTagVersion);
            MaskedStoreDword(12, (uint)FullMaskedStoreState.Consumed);
            MaskedStoreQwordZero(16); MaskedStoreQwordZero(24);
            MaskedStoreQwordZero(80); MaskedStoreQwordZero(88);
            MaskedStoreQwordZero(96); MaskedStoreQwordZero(104);
            MaskedStoreDword(72, 0); MaskedStoreDword(112, 0); MaskedStoreDword(116, 0);
            Patch(noDescriptor, position);
            Bytes(0x41, 0xc7, 0x82); U32(292); U32(0);
            Bytes(0x41, 0xc7, 0x82); U32(324); U32(0);
            Bytes(0x49, 0xc7, 0x82); U32(344); U32(0);
            Patch(success, position);
            if (mmx) MmxTransition(legacyMaskedStore: true);
            NativeMemoryCount(); return true; // core performs the sole ordinary commit
        }

        private void MaskedStoreDword(int offset, uint value)
        { Bytes(0x41, 0xc7, 0x41, (byte)offset); U32(value); }
        private void MaskedStoreQwordZero(int offset)
        { Bytes(0x49, 0xc7, 0x41, (byte)offset); U32(0); }
        private void MaskedStoreCheckDword(int offset, uint value, scoped Span<int> restage, ref int misses)
        { Bytes(0x41, 0x81, 0x79, (byte)offset); U32(value); restage[misses++] = NearCondition(0x85); }
        private void MaskedStoreCheckFrame(int offset, int frame, scoped Span<int> restage, ref int misses, bool wide = true)
        {
            IntegerFrameLoad(frame, 0);
            Bytes(wide ? (byte)0x49 : (byte)0x41, 0x3b, 0x41, (byte)offset);
            restage[misses++] = NearCondition(0x85);
        }
        private void MaskedStoreCheckSnapshot(int offset, int xmm, scoped Span<int> restage, ref int misses)
        {
            VectorTransfer(0xf3, 0x6f, 2, 9, offset);
            Bytes(0x66, 0x0f, 0x74, (byte)(0xd0 | xmm), 0x66, 0x0f, 0xd7, 0xca);
            Bytes(0x81, 0xf9); U32(65535); restage[misses++] = NearCondition(0x85);
        }
        private int MaskedStoreSkipByte(int b)
        { Bytes(0x41, 0xf7, 0x82); U32(192); U32(1u << b); return NearCondition(0x84); }

        private void MaskedStoreSelectPages(JitInstruction i)
        {
            IntegerImmediate(0, 0); IntegerFrameStore(200, 0); IntegerFrameStore(208, 0);
            for (int b = 0; b < i.Width / 8; ++b)
            {
                int disabled = MaskedStoreSkipByte(b);
                IntegerFrameLoad(184, 0);
                int wrap = -1;
                if (b != 0) { IntegerConstant(0, 0, (uint)b, 64); wrap = NearCondition(0x82); }
                IntegerMov(1, 0); IntegerShift(4, 1, 64, 16); IntegerShift(7, 1, 64, 16);
                IntegerRR(0x39, 0, 1); int canonical = NearCondition(0x84);
                if (wrap >= 0) Patch(wrap, position);
                Bytes(0x41, 0xc7, 0x82); U32(168); U32(i.Address.Segment == 2 ? 12u : 13u);
                Bytes(0x41, 0xc7, 0x82); U32(172); U32(128);
                Bytes(0x49, 0xc7, 0x82); U32(176); U32(0);
                JumpExit(FullBlockExit.GuestFault); Patch(canonical, position);
                IntegerFrameLoad(184, 1); IntegerRR(0x31, 0, 1); IntegerShift(5, 0, 64, 12);
                IntegerRR(0x85, 0, 0); int second = NearCondition(0x85);
                Bytes(0x41, 0x81, 0x8a); U32(200); U32(1u << b);
                int done = NativeNearJump(); Patch(second, position);
                Bytes(0x41, 0x81, 0x8a); U32(208); U32(1u << b);
                Patch(done, position); Patch(disabled, position);
            }
            IntegerFrameLoad(200, 0); IntegerRR(0x85, 0, 0, 32); int first = NearCondition(0x85);
            // Nonzero mask with no geometrical first-page bytes: compact slot1.
            IntegerFrameLoad(208, 0); IntegerFrameStore(200, 0);
            IntegerImmediate(0, 0); IntegerFrameStore(208, 0); Patch(first, position);
        }

        // Recompute page identity from selected byte indices; never place guest
        // page keys into frame.MemoryHost pointers, even as temporary values.
        private void MaskedStorePage(int index)
        {
            IntegerFrameLoad(index == 0 ? 200 : 208, 0); IntegerRR(0x85, 0, 0, 32);
            int empty = NearCondition(0x84); // RAX already zero for unused slot
            Bytes(0x0f, 0xbc, 0xc0); Bytes(0x49, 0x03, 0x82); U32(184);
            IntegerConstant(4, 0, 0xfffff000, 64); Patch(empty, position);
        }

        private void MaskedStoreSummary()
        {
            IntegerFrameLoad(192, 0); Bytes(0x0f, 0xbc, 0xc0); // BSF nonzero mask
            Bytes(0x49, 0x03, 0x82); U32(184); IntegerFrameStore(216, 0);
            Bytes(0x41, 0x8b, 0x82); U32(144); Bytes(0x41, 0x89, 0x82); U32(248);
            Bytes(0x41, 0xc7, 0x82); U32(252); U32((uint)FullMemoryAccess.Write);
            Bytes(0x41, 0xc7, 0x82); U32(256); U32(8); // exact selected-byte materializer summary
            Bytes(0x41, 0xc7, 0x82); U32(292); U32(1);
            Bytes(0x41, 0xc7, 0x82); U32(324); U32(0);
            Bytes(0x49, 0xc7, 0x82); U32(344); U32(0);
            Bytes(0x49, 0xc7, 0x82); U32(224); U32(0);
            Bytes(0x49, 0xc7, 0x82); U32(232); U32(0);
        }
        private void MaskedStoreStage(JitInstruction i)
        {
            // Replacing any stale authorization requires parent ticket cancellation
            // on this NeedMemory return, before remapping/re-entering the same issue.
            MaskedStoreDword(4, 0); MaskedStoreDword(12, (uint)FullMaskedStoreState.Empty);
            MaskedStoreDword(0, FullMaskedStoreDescriptor.CurrentTagVersion);
            MaskedStoreQwordZero(16); MaskedStoreQwordZero(24);
            MaskedStoreQwordZero(80); MaskedStoreQwordZero(88);
            Bytes(0x41, 0x8b, 0x82); U32(144); Bytes(0x41, 0x89, 0x41, 8);
            IntegerFrameLoad(184, 0); Bytes(0x49, 0x89, 0x41, 32);
            VectorTransfer(0xf3, 0x7f, 0, 9, 40); VectorTransfer(0xf3, 0x7f, 1, 9, 56);
            MaskedStoreStageFrame(192, 72, false); MaskedStoreStageFrame(200, 112, false);
            MaskedStoreStageFrame(208, 116, false);
            MaskedStorePage(0); Bytes(0x49, 0x89, 0x41, 96);
            MaskedStorePage(1); Bytes(0x49, 0x89, 0x41, 104);
            MaskedStoreDword(76, MaskedStoreShape(i));
            IntegerImmediate(0, i.Ip); Bytes(0x49, 0x89, 0x41, 120);
            MaskedStoreDword(4, FullMaskedStoreDescriptor.SparseKind);
            MaskedStoreDword(12, (uint)FullMaskedStoreState.Request); // publish last
        }
        private void MaskedStoreStageFrame(int source, int destination, bool wide)
        { IntegerFrameLoad(source, 0); Bytes(wide ? (byte)0x49 : (byte)0x41, 0x89, 0x41, (byte)destination); }
        private void MaskedStoreBytes(int count)
        {
            // FULL data/mask snapshots already captured; no guest memory reads,
            // destination RMW, dense stores, per-byte calls or late page lookups.
            for (int b = 0; b < count; ++b)
            {
                int disabled = MaskedStoreSkipByte(b);
                Bytes(0x41, 0xf7, 0x41, 112); U32(1u << b); int second = NearCondition(0x84);
                Bytes(0x49, 0x8b, 0x51, 80); int chosen = NativeNearJump();
                Patch(second, position); Bytes(0x49, 0x8b, 0x51, 88); Patch(chosen, position);
                IntegerFrameLoad(184, 1); IntegerConstant(0, 1, (uint)b, 64); IntegerConstant(4, 1, 4095, 32);
                IntegerRR(0x01, 2, 1);
                Bytes(0x41, 0x8a, 0x41, (byte)(40 + b), 0x88, 0x02);
                Patch(disabled, position);
            }
        }
    }
}
