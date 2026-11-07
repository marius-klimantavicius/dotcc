namespace Managed.Emulation.Execution;

// Source authored, independent TB lowering; no QEMU code. Baseline AMD64 only.
internal static partial class FullBlockEmitter
{
    internal static bool ScalarAtomicFast(JitInstruction i) => CanInlineAtomic(i) &&
        i.Width is (8 or 16 or 32 or 64) && AtomicOperation(i) != FullAtomicOperation.CompareExchangePair;

    private ref partial struct Writer
    {
        private void AtomicFast(JitInstruction i)
        {
            // Keep the exact request staged throughout speculative attempts.
            // NeedMemory retains the issue; attention waits until its commit.
            Bytes(0x41, 0xc7, 0x82); U32(292); U32(1);
            int attempt = position;
            Bytes(0x49, 0x8b, 0x42, 32, 0x83, 0x38, 0);
            ExitCondition(0x85, FullBlockExit.NeedMemory);
            NativeMemoryEpochCheck();
            var op = AtomicOperation(i);
            if (op == FullAtomicOperation.CompareExchange)
            {
                IntegerLoad(i.Source, i.Width, 8);
                IntegerLoad(new(0, 0), i.Width, 0);
                AtomicHostCompareExchange(i.Width);
                // Both a successful and a failed guest comparison linearize
                // here. RAX is the actual old value, never a guessed preread.
                AtomicCompareExchange(i, storeMemory: false);
            }
            else
            {
                NativeMemoryLoad(i.Width);
                IntegerFrameStore(184, 0);
                if (op == FullAtomicOperation.Exchange) IntegerLoad(i.Source, i.Width, 8);
                else if (AtomicBit(op)) AtomicBitOperation(i, storeMemory: false, publish: false);
                else AtomicArithmetic(i, storeMemory: false, publish: false);
                IntegerFrameLoad(184, 0); // CMPXCHG's private expected old
                AtomicHostCompareExchange(i.Width);
                int failed = NearCondition(0x85);
                // The successful old is unchanged in RAX. Recompute selected
                // flags using the SAME arithmetic/width representation as the
                // ticket path, only after linearization. No guest rollback.
                if (op == FullAtomicOperation.Exchange)
                { IntegerMov(8, 0); Store(i.Source, i.Width); }
                else if (AtomicBit(op)) AtomicBitOperation(i, storeMemory: false);
                else AtomicArithmetic(i, storeMemory: false);
                int success = NativeNearJump();
                Patch(failed, position);
                // Every failed CAS returns to stop + epoch checks before the
                // next load/attempt. No Issue, cap, attention or guest effects.
                int retry = NativeNearJump(); Patch(retry, attempt);
                Patch(success, position);
            }
            // Nothing that can reject/revalidate is allowed between the locked
            // instruction and guest outputs above / caller's ordinary Commit.
            Bytes(0x41, 0xc7, 0x82); U32(292); U32(0);
        }

        partial void AtomicFastCompareExchangeProbe(int width); // erased production test seam

        private void AtomicHostCompareExchange(int width)
        {
            Bytes(0x49, 0x8b, 0x92); U32(224); // RDX exact aligned checked host
            AtomicFastCompareExchangeProbe(width);
            if (width == 16) Bytes(0x66);
            Bytes(0xf0, width == 64 ? (byte)0x4c : (byte)0x44,
                0x0f, width == 8 ? (byte)0xb0 : (byte)0xb1, 0x02); // LOCK CMPXCHG [rdx],r8[b/w/d/q]
        }
    }
}
