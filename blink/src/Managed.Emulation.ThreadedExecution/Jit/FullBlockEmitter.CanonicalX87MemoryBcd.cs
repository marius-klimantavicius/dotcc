using System;

namespace Managed.Emulation.Execution;

internal static partial class FullBlockEmitter
{
    private ref partial struct Writer
    {
        // R152 numeric foundation is immutable. Guest A..F source policy below
        // is explicit software behavior for an architecturally undefined result.
        private void CanonicalX87MemoryBcd(in JitInstruction i, in FullX87KernelCall call, bool store)
        {
            if (store) { CanonicalX87MemoryBcdStore(i, in call); return; }
            ScalarMiscCheckCanonical(i, 80); NativeMemoryPrepare(i, FullMemoryAccess.Read, 80);
            CanonicalX87MemoryGather(80);
            int full = CanonicalX87PushCheck();
            CanonicalX87PrepareWork(0); CanonicalX87Invoke(in call);
            Span<int> bad = stackalloc int[96]; int count = 0;
            // Admission3 has a distinct, guest-controlled meaning for FBLD.
            // Recognize it BEFORE generic RequireComplete; never crash/fallback
            // for invalid digits. Return/frame agreement and exact undefined
            // input domain must still authenticate this protocol disposition.
            IntegerConstant(7, 2, 3); int ordinary = NearCondition(0x85);
            CanonicalX87ControlEqual(12, 3, 32, bad, ref count);
            CanonicalX87MemoryComputationHeader(bad, ref count);
            for (int p = 48; p < 88; p += 8) CanonicalX87ControlEqual(p, 0, 64, bad, ref count);
            Span<int> undefined = stackalloc int[18];
            for (int digit = 0; digit < 18; ++digit)
            {
                CanonicalX87WorkLoad(16 + digit / 2, 0, 16);
                if ((digit & 1) != 0) IntegerShift(5, 0, 32, 4);
                IntegerConstant(4, 0, 15); IntegerConstant(7, 0, 9);
                undefined[digit] = NearCondition(0x87);
            }
            bad[count++] = IntegerJump(); // admission3 on defined digits is malformed
            for (int digit = 0; digit < 18; ++digit) Patch(undefined[digit], position);
            CanonicalX87Indefinite(0);
            CanonicalX87MemoryMetadata(i); CanonicalX87MemoryWorkAddress();
            IntegerImmediate(2, 0, 32); CanonicalX87MergeArithmeticStatus();
            CanonicalX87PushRaw(0); int undefinedDone = IntegerJump();

            Patch(ordinary, position); CanonicalX87RequireComplete();
            CanonicalX87MemoryComputationHeader(bad, ref count);
            CanonicalX87MemoryBcdResult(false, bad, ref count);
            CanonicalX87MemoryMetadata(i); CanonicalX87MemoryWorkAddress();
            CanonicalX87WorkLoad(82, 2, 16); CanonicalX87MergeArithmeticStatus();
            CanonicalX87WorkRaw(48, 0); CanonicalX87PushRaw(0);
            int done = IntegerJump();
            Patch(full, position); CanonicalX87MemoryMetadata(i);
            int suppressedFull = CanonicalX87StackFault(true);
            CanonicalX87Indefinite(0); CanonicalX87PushRaw(0); int fullDone = IntegerJump();
            for (int k = 0; k < count; ++k) Patch(bad[k], position);
            CanonicalX87MemoryWorkAddress(); CanonicalX87WorkConstant(284, 0x80000000); CanonicalX87ProtocolFatal();
            Patch(undefinedDone, position); Patch(done, position); Patch(suppressedFull, position); Patch(fullDone, position);
            NativeMemoryCount();
        }

        private void CanonicalX87MemoryBcdStore(in JitInstruction i, in FullX87KernelCall call)
        {
            int empty = CanonicalX87SourceCheck(0);
            CanonicalX87LoadRaw(0, 0); CanonicalX87PrepareWork(0); CanonicalX87WorkConstant(288, 0);
            int staged = IntegerJump();
            Patch(empty, position);
            CanonicalX87Indefinite(0); CanonicalX87PrepareWork(0); CanonicalX87WorkConstant(288, 0x41);
            CanonicalX87MachineLoad(672, 0); IntegerConstant(4, 0, 1);
            int maskedEmpty = NearCondition(0x85);
            ScalarMiscCheckCanonical(i, 80); CanonicalX87MemoryProbe(i);
            CanonicalX87MemoryMetadata(i); int emptySuppressed = CanonicalX87StackFault(false);
            int emptyDone = IntegerJump();
            Patch(maskedEmpty, position); Patch(staged, position);
            CanonicalX87Invoke(in call); CanonicalX87RequireComplete();
            Span<int> bad = stackalloc int[96]; int count = 0;
            CanonicalX87MemoryComputationHeader(bad, ref count);
            CanonicalX87MemoryBcdResult(true, bad, ref count);
            CanonicalX87WorkLoad(84, 0, 32); IntegerConstant(4, 0, 512);
            int suppressed = NearCondition(0x85);
            ScalarMiscCheckCanonical(i, 80); NativeMemoryPrepare(i, FullMemoryAccess.Write, 80);
            CanonicalX87MemoryWorkAddress(); CanonicalX87WorkRaw(48, 0);
            CanonicalX87MemoryScatter(80);
            CanonicalX87MemoryMetadata(i); CanonicalX87MemoryMergeStatus();
            CanonicalX87Pop(); NativeMemoryCount(); int done = IntegerJump();
            Patch(suppressed, position);
            ScalarMiscCheckCanonical(i, 80); CanonicalX87MemoryProbe(i);
            CanonicalX87MemoryMetadata(i); CanonicalX87MemoryMergeStatus(); int suppressedDone = IntegerJump();
            for (int k = 0; k < count; ++k) Patch(bad[k], position);
            CanonicalX87MemoryWorkAddress(); CanonicalX87WorkConstant(284, 0x80000000); CanonicalX87ProtocolFatal();
            Patch(emptySuppressed, position); Patch(emptyDone, position); Patch(done, position); Patch(suppressedDone, position);
        }

        private void CanonicalX87MemoryBcdResult(bool store, scoped Span<int> bad, ref int count)
        {
            CanonicalX87ControlEqual(58, 0, 16, bad, ref count);
            CanonicalX87ControlEqual(60, 0, 32, bad, ref count);
            CanonicalX87ControlEqual(64, 0, 64, bad, ref count);
            CanonicalX87ControlEqual(72, 0, 64, bad, ref count);
            if (!store)
            {
                CanonicalX87ControlEqual(80, 0, 32, bad, ref count);
                CanonicalX87ControlEqual(84, 3, 32, bad, ref count);
                CanonicalX87ControlValue(48, bad, ref count);
                CanonicalX87WorkLoad(56, 0, 16); IntegerConstant(4, 0, 0x7fff);
                IntegerConstant(7, 0, 0x403a); bad[count++] = NearCondition(0x87);
                return;
            }
            CanonicalX87WorkLoad(80, 2, 16);
            IntegerMov(0, 2, 32); IntegerConstant(4, 0, ~0x221u); bad[count++] = NearCondition(0x85);
            CanonicalX87ControlMatch(82, 2, 16, bad, ref count);
            // Reconstruct exact BCD metadata from masked status and CW.
            IntegerImmediate(1, 1, 32); IntegerConstant(7, 2, 1); int invalid = NearCondition(0x84);
            IntegerMov(0, 2, 32); IntegerConstant(4, 0, 32); int exact = NearCondition(0x84);
            IntegerConstant(1, 1, 128);
            IntegerMov(0, 2, 32); IntegerConstant(4, 0, 512); int noUp = NearCondition(0x84);
            IntegerConstant(1, 1, 256); Patch(noUp, position);
            CanonicalX87WorkLoad(8, 0, 16); IntegerConstant(4, 0, 32); int precisionMasked = NearCondition(0x85);
            IntegerConstant(1, 1, 1024); Patch(precisionMasked, position);
            int precision = IntegerJump();
            Patch(exact, position); IntegerConstant(7, 2, 0); bad[count++] = NearCondition(0x85);
            IntegerConstant(1, 1, 2); Patch(precision, position);
            // A normal BCD image has only decimal nibbles, sign-only byte9.
            for (int digit = 0; digit < 18; ++digit)
            {
                CanonicalX87WorkLoad(48 + digit / 2, 0, 16);
                if ((digit & 1) != 0) IntegerShift(5, 0, 32, 4);
                IntegerConstant(4, 0, 15); IntegerConstant(7, 0, 9); bad[count++] = NearCondition(0x87);
            }
            CanonicalX87WorkLoad(56, 0, 16); IntegerConstant(4, 0, 0x7f00); bad[count++] = NearCondition(0x85);
            int imageChecked = IntegerJump();
            Patch(invalid, position); IntegerImmediate(1, 9, 32);
            CanonicalX87WorkLoad(8, 0, 16); IntegerConstant(4, 0, 1); int invalidMasked = NearCondition(0x85);
            IntegerConstant(1, 1, 512); Patch(invalidMasked, position);
            CanonicalX87WorkLoad(48, 0); IntegerImmediate(8, 0xc000000000000000UL);
            IntegerRR(0x39, 0, 8); bad[count++] = NearCondition(0x85);
            CanonicalX87ControlEqual(56, 0xffff, 16, bad, ref count);
            Patch(imageChecked, position); CanonicalX87ControlMatch(84, 1, 32, bad, ref count);
        }
    }
}
