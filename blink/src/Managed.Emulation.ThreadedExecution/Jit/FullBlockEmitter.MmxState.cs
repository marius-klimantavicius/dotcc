namespace Managed.Emulation.Execution;

internal static partial class FullBlockEmitter
{
    private ref partial struct Writer
    {
        // ABI7 ingress has validated any nonnull frame.X87. The owner pins that
        // exact file through native return; these are inline native instructions,
        // with no managed selector, helper call, f64 import or host MMX use.
        // RAX and host ALU flags are dead at every call site. Keep RCX/RDX/R8/R9
        // live (memory pointers, scalar results, sparse snapshots/continuations).
        // XMM registers and MXCSR are untouched except the explicit load target.
        // No stack adjustment or additional frame scratch is required.
        private void MmxLoad(int xmm, int guest)
        {
            IntegerFrameLoad(400, 0); IntegerRR(0x85, 0, 0);
            int legacy = NearCondition(0x84);
            VectorTransfer(0xf3, 0x7e, xmm, 0,
                FullX87RegisterFile.SlotOffset + guest * FullX87RegisterFile.SlotStride);
            int done = NativeNearJump();
            Patch(legacy, position);
            VectorTransfer(0xf3, 0x7e, xmm, 11, 160 + guest * 16);
            Patch(done, position);
        }

        // Only after all fault/exception exits. MOVQ clears host upper64, while
        // the canonical slot tail is explicitly signexp=ffff and six zero bytes.
        // Null preserves the historical Machine.XMM upper64 verbatim.
        private void MmxStore(int xmm, int guest)
        {
            IntegerFrameLoad(400, 0); IntegerRR(0x85, 0, 0);
            int legacy = NearCondition(0x84);
            int slot = FullX87RegisterFile.SlotOffset + guest * FullX87RegisterFile.SlotStride;
            VectorTransfer(0x66, 0xd6, xmm, 0, slot);
            Bytes(0x48, 0xc7); Memory(0, 0, slot + 8); U32(0xffff);
            int done = NativeNearJump();
            Patch(legacy, position);
            VectorTransfer(0x66, 0xd6, xmm, 11, 160 + guest * 16);
            Patch(done, position);
        }

        // Also required for source-only operations and successful zero MASKMOVQ.
        // The inherited masked-store path already transitioned with a null file;
        // every other null-file operation retains its exact old metadata behavior.
        // Called after result stores, when RAX/ALU flags are no longer inputs.
        private void MmxTransition(bool legacyMaskedStore = false)
        {
            int inactive = -1;
            if (!legacyMaskedStore)
            {
                IntegerFrameLoad(400, 0); IntegerRR(0x85, 0, 0);
                inactive = NearCondition(0x84);
            }
            Bytes(0x41, 0x81, 0xa3); U32(648); U32(~0x3800u);
            Bytes(0x41, 0xc7, 0x83); U32((uint)MmxLayout.TagOffset); U32(0);
            if (inactive >= 0) Patch(inactive, position);
        }

        private void MmxEmms()
        {
            IntegerFrameLoad(400, 0); IntegerRR(0x85, 0, 0);
            int legacy = NearCondition(0x84);
            Bytes(0x41, 0xc7, 0x83); U32((uint)MmxLayout.TagOffset); U32(0xffff);
            int done = NativeNearJump();
            Patch(legacy, position);
            Bytes(0x41, 0xc7, 0x83); U32((uint)MmxLayout.TagOffset); U32(uint.MaxValue);
            Patch(done, position); // both paths preserve TOP and all raw payloads
        }
    }
}
