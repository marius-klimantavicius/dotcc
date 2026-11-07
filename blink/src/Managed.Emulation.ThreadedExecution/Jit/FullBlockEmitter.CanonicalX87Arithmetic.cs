using System;
using AX = Managed.Emulation.Execution.FullCanonicalX87ArithmeticOperation;

namespace Managed.Emulation.Execution;

internal static partial class FullBlockEmitter
{
    internal static bool CanInlineCanonicalX87Arithmetic(JitInstruction i) =>
        i.Operation == JitOperation.X87 && (i.Options & 0xffff) >= (uint)AX.FaddSt0 &&
        (i.Options & 0xffff) <= (uint)AX.FdivrSti;

    internal static bool ValidateCanonicalX87Arithmetic(JitInstruction i)
    {
        if (!CanInlineCanonicalX87Arithmetic(i)) return false;
        var op = (AX)(i.Options & 0xffff);
        bool sti = op >= AX.FaddSti, pop = (i.Options & FullCanonicalX87ArithmeticLowering.Pop) != 0;
        int k = (int)op - (int)(sti ? AX.FaddSti : AX.FaddSt0), n = i.Source.Register;
        int group = k switch { 0 => 0, 1 => 1, 2 => sti ? 5 : 4,
            3 => sti ? 4 : 5, 4 => sti ? 7 : 6, _ => sti ? 6 : 7 };
        uint fop = (uint)((pop ? 6 : sti ? 4 : 0) << 8 | 0xc0 | group << 3 | n);
        if ((i.Options & ~FullCanonicalX87ArithmeticLowering.Pop) != (uint)op || pop && !sti ||
            i.Length is < 2 or > 15 || i.Width != 80 || i.Destination != default ||
            (uint)n > 7 || i.Source != new JitOperand(n, 0) || i.Target != 0 || i.Condition != 0 ||
            i.SourceWidth != 0 || i.Address != default || i.Extra != new JitOperand(0, fop, IsImmediate: true))
            throw new ArgumentException("Invalid canonical raw80 arithmetic IR");
        if (!CanonicalX87StackLayout.Supported)
            throw new PlatformNotSupportedException("Canonical x87 Machine/file/frame layout differs");
        return true;
    }

    internal static FullX87KernelKey CanonicalX87ArithmeticKernel(JitInstruction i)
    {
        if (!ValidateCanonicalX87Arithmetic(i)) throw new ArgumentException("Not canonical x87 arithmetic");
        var op = (AX)(i.Options & 0xffff);
        return op switch
        {
            AX.FaddSt0 or AX.FaddSti => FullX87KernelKey.Add,
            AX.FmulSt0 or AX.FmulSti => FullX87KernelKey.Multiply,
            AX.FsubSt0 or AX.FsubrSt0 or AX.FsubSti or AX.FsubrSti => FullX87KernelKey.Subtract,
            _ => FullX87KernelKey.Divide
        };
    }

    internal static bool ValidateCanonicalX87Arithmetic(JitInstruction i, in FullX87KernelCall call)
    {
        if (!ValidateCanonicalX87Arithmetic(i)) return false;
        if (call.Key != (byte)CanonicalX87ArithmeticKernel(i) || call.Entry == 0 || call.OwnerCookie == 0 ||
            call.CodeBytes <= 0 || call.CodeBytes > FullX87KernelCatalog.StagingBytes)
            throw new ArgumentException("Invalid canonical x87 arithmetic kernel call");
        // Address/size/owner/code identity must ALSO be authenticated under the
        // authority gate by the parent. A raw tuple cannot prove RX ownership.
        return true;
    }

    private ref partial struct Writer
    {
        private bool TryCanonicalX87ArithmeticOperation(in JitInstruction i, in FullX87KernelCall call)
        {
            if (!CanInlineCanonicalX87Arithmetic(i)) return false;
            ValidateCanonicalX87Arithmetic(i, in call);
            var op = (AX)(i.Options & 0xffff);
            bool sti = op >= AX.FaddSti, pop = (i.Options & FullCanonicalX87ArithmeticLowering.Pop) != 0;
            bool reverse = op is AX.FsubrSt0 or AX.FdivrSt0 or AX.FsubrSti or AX.FdivrSti;
            int n = i.Source.Register, destination = sti ? n : 0;
            int left = reverse ? (sti ? 0 : n) : destination;
            int right = reverse ? destination : (sti ? 0 : n);

            // Parent has already polled/admitted context, Work, all-PC software precision and owner,
            // then reserved Issue. Prior six-flag #MF precedes ALL effects here.
            FloatMmxWaitingException(i);
            int empty0 = CanonicalX87SourceCheck(0), emptyN = CanonicalX87SourceCheck(n);
            // Stage both raw inputs BEFORE any alias overwrite or TOP motion.
            CanonicalX87LoadRaw(left, 0); CanonicalX87LoadRaw(right, 1);
            CanonicalX87PrepareWork(0, 1);
            CanonicalX87Invoke(in call);
            CanonicalX87RequireComplete();
            // Reject malformed response rather than consuming stale/invalid data.
            // Both normal validity and status reserved bits are protocol checks.
            CanonicalX87WorkLoad(84, 0, 32); IntegerConstant(4, 0, 1);
            int badNormal = NearCondition(0x84);
            CanonicalX87WorkLoad(80, 0, 16); IntegerConstant(4, 0, ~0x23fu);
            int badStatus = NearCondition(0x85);

            // Pre-operation IE/DE/ZE: suppress destination/pop when ANY unmasks.
            // The kernel already resolves IE/NaN/ZE class priority. When DE
            // stops computation, lower OE/UE/PE and their C1 must not leak out.
            CanonicalX87WorkLoad(80, 2, 16); CanonicalX87WorkLoad(8, 1, 16);
            IntegerMov(0, 1, 32); IntegerUnary(2, 0, 32);
            IntegerRR(0x21, 0, 2, 32); IntegerConstant(4, 0, 7);
            int preOperation = NearCondition(0x85);

            // Choose wrapped result by unbounded rounded classification, NOT
            // NormalStatus. Exact tiny masked results deliberately lack UE.
            CanonicalX87WorkLoad(84, 0, 32); IntegerConstant(4, 0, 8);
            int noOverflow = NearCondition(0x84);
            IntegerMov(0, 1, 32); IntegerConstant(4, 0, 8);
            int wrappedOverflow = NearCondition(0x84);
            Patch(noOverflow, position);
            CanonicalX87WorkLoad(84, 0, 32); IntegerConstant(4, 0, 4);
            int normalNotTiny = NearCondition(0x84);
            IntegerMov(0, 1, 32); IntegerConstant(4, 0, 16);
            int wrappedTiny = NearCondition(0x84);
            Patch(normalNotTiny, position);
            CanonicalX87WorkRaw(48, 0); // normal result and status in EDX
            int chosenNormal = IntegerJump();
            Patch(wrappedOverflow, position); Patch(wrappedTiny, position);
            CanonicalX87WorkLoad(84, 0, 32); IntegerConstant(4, 0, 2);
            int badWrapped = NearCondition(0x84);
            CanonicalX87WorkLoad(82, 2, 16);
            IntegerMov(0, 2, 32); IntegerConstant(4, 0, ~0x23fu);
            int badWrappedStatus = NearCondition(0x85);
            CanonicalX87WorkRaw(64, 0);
            Patch(chosenNormal, position);
            // Commit valid arithmetic. Metadata helper clobbers EAX/ECX only;
            // SSE raw result + EDX status remain live. Undefined C0/C2/C3 retain.
            CanonicalX87Metadata(i);
            CanonicalX87MergeArithmeticStatus();
            CanonicalX87StoreRaw(destination, 0);
            if (pop) CanonicalX87Pop();
            int completed = IntegerJump();

            Patch(preOperation, position);
            IntegerConstant(4, 2, 7); // only pre flags, C1 cleared
            CanonicalX87Metadata(i); CanonicalX87MergeArithmeticStatus();
            int suppressed = IntegerJump();

            Patch(empty0, position); Patch(emptyN, position);
            // Empty stack outranks even unsupported/SNaN in the other operand.
            CanonicalX87Metadata(i);
            int emptySuppressed = CanonicalX87StackFault(false);
            CanonicalX87Indefinite(0); CanonicalX87StoreRaw(destination, 0);
            if (pop) CanonicalX87Pop();
            int emptyComplete = IntegerJump();

            Patch(badNormal, position); Patch(badStatus, position);
            Patch(badWrapped, position); Patch(badWrappedStatus, position);
            CanonicalX87WorkConstant(284, 0x80000000); // invalid validity/status protocol
            CanonicalX87ProtocolFatal();
            // Parent MUST inspect Work280 after return and terminate internally;
            // no fallback/retry is legal. No valid body can reach this seam.
            Patch(completed, position); Patch(suppressed, position);
            Patch(emptySuppressed, position); Patch(emptyComplete, position);
            return true;
        }

        // SHARED R148/R150 native bridge. Exact clobber/ownership contract in
        // D/API.txt. No result/status/guest-metadata policy is hidden here.
        // Caller stages raw inputs BEFORE Prepare; caller owns all288..383.
        private void CanonicalX87PrepareWork()
        {
            IntegerFrameLoad(400, 9); IntegerConstant(0, 9, 192, 64);
            CanonicalX87WorkConstant(280, 0); CanonicalX87WorkConstant(284, 0);
            CanonicalX87WorkConstant(0, 1); CanonicalX87WorkConstant(4, 384);
            CanonicalX87MachineLoad(672, 0); CanonicalX87WorkStore(8, 0, 16);
            IntegerImmediate(0, 0, 32); CanonicalX87WorkStore(10, 0, 16);
            CanonicalX87WorkConstant(12, uint.MaxValue);
            IntegerMov(0, 9);
        }
        private void CanonicalX87PrepareWork(int leftXmm, int rightXmm = -1)
        {
            if ((uint)leftXmm > 5 || rightXmm is < -1 or > 5)
                throw new ArgumentOutOfRangeException(nameof(leftXmm));
            CanonicalX87PrepareWork();
            Bytes(0xf3, 0x41, 0x0f, 0x7f); Memory(leftXmm, 1, 16);
            if (rightXmm >= 0)
            { Bytes(0xf3, 0x41, 0x0f, 0x7f); Memory(rightXmm, 1, 32); }
            else
            {
                IntegerImmediate(0, 0, 32); CanonicalX87WorkStore(32, 0); CanonicalX87WorkStore(40, 0);
                IntegerMov(0, 9);
            }
        }
        private void CanonicalX87Invoke(in FullX87KernelCall call)
        {
            if (call.Entry == 0 || call.OwnerCookie == 0 || call.CodeBytes <= 0 || call.CodeBytes > FullX87KernelCatalog.StagingBytes)
                throw new ArgumentException("Invalid native x87 continuation call");
            CanonicalX87WorkStore(256, 10); CanonicalX87WorkStore(264, 11);
            IntegerFrameLoad(400, 0); CanonicalX87WorkStore(272, 0);
            Bytes(0x48, 0x8d, 0x05); int continuation = position; U32(0);
            CanonicalX87WorkStore(88, 0);
            IntegerMov(11, 9); AbsoluteJump(call.Entry);
            Patch(continuation, position); EndBranch();
            IntegerMov(9, 11);
            CanonicalX87WorkLoad(256, 10); CanonicalX87WorkLoad(264, 11);
            IntegerMov(2, 0, 32); // preserve admission before returning RAX=Work
            CanonicalX87WorkConstant(88, 0); CanonicalX87WorkConstant(92, 0);
            IntegerMov(0, 9);
        }
        private void CanonicalX87RequireComplete()
        {
            IntegerMov(0, 2, 32); CanonicalX87WorkLoad(12, 1, 32);
            IntegerRR(0x09, 0, 1, 32); IntegerRR(0x85, 0, 0, 32);
            int complete = NearCondition(0x84);
            CanonicalX87WorkStore(284, 0, 32); CanonicalX87ProtocolFatal();
            Patch(complete, position);
        }
        private void CanonicalX87ProtocolFatal()
        {
            CanonicalX87WorkConstant(280, 0x58383746);
            Bytes(0x41, 0xc7, 0x82); U32(152); U32(1); // preserve SAME reserved issue
            JumpExit(FullBlockExit.Invalidated);
        }

        // All Work accesses use R9. Helpers emit no native CALL or stack use.
        private void CanonicalX87WorkLoad(int offset, int register, int width = 64)
        {
            IntegerPrefix(width == 16 ? 32 : width, 9, register);
            if (width == 16) Bytes(0x0f, 0xb7); else Bytes(0x8b);
            Memory(register & 7, 1, offset);
        }
        private void CanonicalX87WorkStore(int offset, int register, int width = 64)
        { IntegerPrefix(width, 9, register); Bytes(0x89); Memory(register & 7, 1, offset); }
        private void CanonicalX87WorkConstant(int offset, uint value)
        { Bytes(0x41, 0xc7, 0x81); U32((uint)offset); U32(value); }
        private void CanonicalX87WorkRaw(int offset, int xmm)
        {
            // movq zeroes upper64; pinsrw supplies ONLY the sign/exponent word.
            Bytes(0xf3, 0x41, 0x0f, 0x7e); Memory(xmm, 1, offset);
            Bytes(0x66, 0x41, 0x0f, 0xc4); Memory(xmm, 1, offset + 8); Bytes(4);
        }
        // EDX = selected architectural delta, already priority-restricted.
        // Replaces C1; retains sticky six flags, SF, undefined CCs and upper SW.
        // ES/B become sticky when any selected exception is unmasked.
        private void CanonicalX87MergeArithmeticStatus()
        {
            CanonicalX87MachineLoad(648, 0); IntegerConstant(4, 0, ~0x200u);
            IntegerRR(0x09, 0, 2, 32);
            CanonicalX87MachineLoad(672, 1); IntegerUnary(2, 1, 32);
            IntegerRR(0x21, 1, 2, 32); IntegerConstant(4, 1, 63);
            int masked = NearCondition(0x84);
            IntegerConstant(1, 0, 0x8080);
            Patch(masked, position); CanonicalX87MachineStore(648, 0);
        }
    }
}
