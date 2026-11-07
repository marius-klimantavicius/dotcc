using System;
using CX = Managed.Emulation.Execution.FullCanonicalX87MemoryArithmeticOperation;

namespace Managed.Emulation.Execution;

internal static partial class FullBlockEmitter
{
    // This public integration seam intentionally covers the complete34-row
    // memory computation family. R150 conversions remain a separate domain.
    internal static bool CanInlineCanonicalX87MemoryArithmetic(JitInstruction i) =>
        i.Operation == JitOperation.X87 && (i.Options & 0xffff) >= (uint)CX.Add &&
        (i.Options & 0xffff) <= (uint)CX.Fbstp;

    internal static bool ValidateCanonicalX87MemoryArithmetic(JitInstruction i)
    {
        if (!CanInlineCanonicalX87MemoryArithmetic(i)) return false;
        uint fop = (uint)i.Extra.Immediate;
        byte modrm = (byte)fop;
        if (i.Extra != new JitOperand(0, fop, IsImmediate: true) || fop > 0x7ff ||
            !FullCanonicalX87MemoryArithmeticLowering.Decode((byte)(0xd8 | fop >> 8), modrm, out var op, out int width) ||
            i.Options != (uint)op || i.Length is < 2 or > 15 || i.Width != 80 || i.SourceWidth != width ||
            i.Source != (op == CX.Fbstp ? default : new JitOperand(0, 0, IsMemory: true)) ||
            i.Destination != (op == CX.Fbstp ? new JitOperand(0, 0, IsMemory: true) : default) ||
            i.Target != 0 || i.Condition != 0 || !CanonicalX87MemoryAddressShape(i.Address, modrm, i.Length))
            throw new ArgumentException("Invalid canonical x87 memory computation IR");
        if (!CanonicalX87StackLayout.Supported || !ScalarMiscLayout.Supported)
            throw new PlatformNotSupportedException("Canonical x87 memory computation layout differs");
        return true;
    }

    internal static bool CanonicalX87MemoryArithmeticRequiresPrecision(JitInstruction i) =>
        ValidateCanonicalX87MemoryArithmetic(i) && FullCanonicalX87MemoryArithmeticLowering.IsArithmetic((CX)i.Options);

    internal static FullX87KernelKey CanonicalX87MemoryArithmeticKernel(JitInstruction i)
    {
        if (!ValidateCanonicalX87MemoryArithmetic(i)) throw new ArgumentException("Not canonical x87 memory computation");
        var op = (CX)i.Options;
        if (op is CX.Fbld or CX.Fbstp) return (FullX87KernelKey)(op == CX.Fbld ? 64 : 65);
        int format = (int)FullCanonicalX87MemoryArithmeticLowering.Format((byte)(0xd8 | i.Extra.Immediate >> 8));
        return (FullX87KernelKey)(FullCanonicalX87MemoryArithmeticLowering.IsArithmetic(op) ?
            32 + format * 6 + (int)op - (int)CX.Add : 56 + format * 2 + (op == CX.Fcomp ? 1 : 0));
    }

    internal static bool ValidateCanonicalX87MemoryArithmetic(JitInstruction i, in FullX87KernelCall call)
    {
        if (!ValidateCanonicalX87MemoryArithmetic(i)) return false;
        if (call.Key != (byte)CanonicalX87MemoryArithmeticKernel(i) || call.Entry == 0 || call.OwnerCookie == 0 ||
            call.CodeBytes <= 0 || call.CodeBytes > FullX87KernelCatalog.StagingBytes)
            throw new ArgumentException("Invalid canonical x87 memory computation kernel call");
        // Same-Gate RX ownership and exact576/frame/header admission are parent
        // responsibilities. Original TB/native epoch/root spans continuation.
        return true;
    }

    private ref partial struct Writer
    {
        private bool TryCanonicalX87MemoryArithmeticOperation(in JitInstruction i, in FullX87KernelCall call)
        {
            if (!CanInlineCanonicalX87MemoryArithmetic(i)) return false;
            ValidateCanonicalX87MemoryArithmetic(i, in call);
            var op = (CX)i.Options;
            FloatMmxWaitingException(i); // before any Work/address/guest effects
            if (op is CX.Fbld or CX.Fbstp) { CanonicalX87MemoryBcd(i, in call, op == CX.Fbstp); return true; }
            // Fullspan faults precede every x87 architectural effect. Empty-ST0
            // concurrent memory-fault order is explicitly pending parent controls.
            ScalarMiscCheckCanonical(i, i.SourceWidth);
            NativeMemoryPrepare(i, FullMemoryAccess.Read, i.SourceWidth);
            CanonicalX87MemoryGather(i.SourceWidth); // XMM0 raw source, zero high
            int empty = CanonicalX87SourceCheck(0);
            Bytes(0x66, 0x0f, 0x6f, 0xc8); // XMM1=raw memory BEFORE ST0 load
            CanonicalX87LoadRaw(0, 0);
            CanonicalX87PrepareWork(0, 1);
            CanonicalX87Invoke(in call); CanonicalX87RequireComplete();
            Span<int> bad = stackalloc int[96]; int count = 0;
            CanonicalX87MemoryComputationHeader(bad, ref count);
            bool compare = FullCanonicalX87MemoryArithmeticLowering.IsCompare(op);
            int suppressed = -1;
            if (compare)
            {
                CanonicalX87ValidateCompare(false, op == CX.Fcomp ? 1 : 0, bad, ref count);
                CanonicalX87MemoryMetadata(i); CanonicalX87MemoryWorkAddress();
                CanonicalX87CommitCompareStatus();
                if (op == CX.Fcomp)
                {
                    CanonicalX87WorkLoad(84, 0, 32); IntegerConstant(4, 0, 2048);
                    suppressed = NearCondition(0x85); CanonicalX87Pop();
                }
            }
            else CanonicalX87MemoryArithmeticComplete(i, bad, ref count);
            int done = IntegerJump();
            Patch(empty, position);
            CanonicalX87MemoryMetadata(i);
            int emptySuppressed = CanonicalX87StackFault(false);
            if (compare)
            {
                CanonicalX87MachineLoad(648, 0); IntegerConstant(4, 0, ~0x4700u);
                IntegerConstant(1, 0, 0x4500); CanonicalX87MachineStore(648, 0);
                if (op == CX.Fcomp) CanonicalX87Pop();
            }
            else { CanonicalX87Indefinite(0); CanonicalX87StoreRaw(0, 0); }
            int emptyDone = IntegerJump();
            for (int k = 0; k < count; ++k) Patch(bad[k], position);
            CanonicalX87MemoryWorkAddress(); CanonicalX87WorkConstant(284, 0x80000000);
            CanonicalX87ProtocolFatal();
            Patch(done, position); Patch(emptySuppressed, position); Patch(emptyDone, position);
            if (suppressed >= 0) Patch(suppressed, position);
            NativeMemoryCount();
            return true;
        }

        private void CanonicalX87MemoryComputationHeader(scoped Span<int> bad, ref int count)
        {
            CanonicalX87ControlEqual(0, 1, 32, bad, ref count);
            CanonicalX87ControlEqual(4, 384, 32, bad, ref count);
            CanonicalX87ControlEqual(10, 0, 16, bad, ref count);
            CanonicalX87MachineLoad(672, 1); IntegerConstant(4, 1, 0xffff);
            CanonicalX87ControlMatch(8, 1, 16, bad, ref count);
        }

        private void CanonicalX87MemoryArithmeticComplete(in JitInstruction i, scoped Span<int> bad, ref int count)
        {
            // Complete finite metadata set, not only a permissive NormalValid.
            // Special-class bits and range-wrap images cannot be conflated.
            CanonicalX87WorkLoad(84, 8, 32);
            Span<int> valid = stackalloc int[16]; int n = 0;
            ReadOnlySpan<uint> masks = stackalloc uint[] {1,17,33,49,7,23,39,55,11,43,65,97,145,161,177};
            foreach (uint mask in masks)
            { IntegerConstant(7, 8, mask); valid[n++] = NearCondition(0x84); }
            bad[count++] = IntegerJump();
            for (int k = 0; k < n; ++k) Patch(valid[k], position);
            CanonicalX87ControlEqual(58, 0, 16, bad, ref count);
            CanonicalX87ControlEqual(60, 0, 32, bad, ref count);
            CanonicalX87ControlEqual(74, 0, 16, bad, ref count);
            CanonicalX87ControlEqual(76, 0, 32, bad, ref count);
            CanonicalX87WorkLoad(80, 2, 16); IntegerMov(0, 2, 32); IntegerConstant(4, 0, ~0x23fu);
            bad[count++] = NearCondition(0x85);
            CanonicalX87WorkLoad(82, 0, 16); IntegerConstant(4, 0, ~0x23fu);
            bad[count++] = NearCondition(0x85);
            CanonicalX87ControlValue(48, bad, ref count);
            // PreOperationException iff a pre flag actually exists.
            CanonicalX87WorkLoad(80, 2, 16); IntegerImmediate(1, 0, 32);
            IntegerMov(0, 2, 32); IntegerConstant(4, 0, 7); int noPre = NearCondition(0x84);
            IntegerImmediate(1, 32, 32); Patch(noPre, position);
            IntegerMov(0, 8, 32); IntegerConstant(4, 0, 32); IntegerRR(0x39, 0, 1, 32);
            bad[count++] = NearCondition(0x85);
            // An IE/ZE outcome is exclusive: no DE/range/PE/C1 leakage.
            IntegerMov(0, 2, 32); IntegerConstant(4, 0, 5); int noIeZe = NearCondition(0x84);
            IntegerRR(0x39, 0, 2, 32); bad[count++] = NearCondition(0x85);
            IntegerConstant(7, 0, 5); bad[count++] = NearCondition(0x84);
            Patch(noIeZe, position);
            // WrappedValid exactly matches Tiny/Overflow. No unused images.
            IntegerMov(0, 8, 32); IntegerConstant(4, 0, 2); int noWrapped = NearCondition(0x84);
            CanonicalX87ControlValue(64, bad, ref count);
            int wrappedChecked = IntegerJump();
            Patch(noWrapped, position);
            CanonicalX87ControlEqual(64, 0, 64, bad, ref count);
            CanonicalX87ControlEqual(72, 0, 64, bad, ref count);
            CanonicalX87ControlEqual(82, 0, 16, bad, ref count);
            Patch(wrappedChecked, position);
            // Unmasked pre conditions suppress result; discard lower post flags.
            CanonicalX87WorkLoad(80, 2, 16); CanonicalX87WorkLoad(8, 1, 16);
            IntegerMov(0, 1, 32); IntegerUnary(2, 0, 32); IntegerRR(0x21, 0, 2, 32);
            IntegerConstant(4, 0, 7); int pre = NearCondition(0x85);
            IntegerMov(0, 8, 32); IntegerConstant(4, 0, 8); int noOverflow = NearCondition(0x84);
            IntegerMov(0, 1, 32); IntegerConstant(4, 0, 8); int wrappedOverflow = NearCondition(0x84);
            Patch(noOverflow, position);
            IntegerMov(0, 8, 32); IntegerConstant(4, 0, 4); int noTiny = NearCondition(0x84);
            IntegerMov(0, 1, 32); IntegerConstant(4, 0, 16); int wrappedTiny = NearCondition(0x84);
            Patch(noTiny, position);
            CanonicalX87WorkRaw(48, 0); int normal = IntegerJump();
            Patch(wrappedOverflow, position); Patch(wrappedTiny, position);
            CanonicalX87WorkLoad(82, 2, 16); CanonicalX87WorkRaw(64, 0);
            Patch(normal, position);
            // EA metadata may destroy EDX/R9: preserve delta in caller storage.
            CanonicalX87WorkStore(288, 2, 32);
            CanonicalX87MemoryMetadata(i); CanonicalX87MemoryWorkAddress();
            CanonicalX87WorkLoad(288, 2, 32); CanonicalX87MergeArithmeticStatus();
            CanonicalX87StoreRaw(0, 0); int completed = IntegerJump();
            Patch(pre, position); IntegerConstant(4, 2, 7); CanonicalX87WorkStore(288, 2, 32);
            CanonicalX87MemoryMetadata(i); CanonicalX87MemoryWorkAddress();
            CanonicalX87WorkLoad(288, 2, 32); CanonicalX87MergeArithmeticStatus();
            Patch(completed, position);
        }
    }
}
