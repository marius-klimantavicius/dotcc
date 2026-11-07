using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Blink = Managed.Emulation.BlinkCore;
using CX = Managed.Emulation.Execution.FullCanonicalX87StackOperation;

namespace Managed.Emulation.Execution;

internal static partial class FullBlockEmitter
{
    internal static bool CanInlineCanonicalX87Stack(JitInstruction i) =>
        i.Operation == JitOperation.X87 && (i.Options & 0xffff) >= (uint)CX.Fld &&
        (i.Options & 0xffff) <= (uint)CX.Ffreep;

    internal static bool ValidateCanonicalX87Stack(JitInstruction i)
    {
        if (!CanInlineCanonicalX87Stack(i)) return false;
        var op = (CX)(i.Options & 0xffff);
        int n = i.Source.Register;
        ulong fop = i.Extra.Immediate;
        bool form;
        if (op == CX.Fwait) form = fop == 0 && n == 0 && i.Options == (uint)op;
        else
            form = fop <= 0x7ff && FullCanonicalX87StackLowering.DecodeRegister(
                (byte)(0xd8 | fop >> 8), (byte)fop, out var decoded, out int index, out bool alias) &&
                decoded == op && index == n && i.Options == ((uint)op |
                    (alias ? FullCanonicalX87StackLowering.SelectedAlias : 0));
        if (!form || i.Length < (op == CX.Fwait ? 1 : 2) || i.Length > 15 ||
            i.Width != 80 || i.Destination != default || i.Source != new JitOperand(n, 0) ||
            i.Target != 0 || i.Condition != 0 ||
            i.SourceWidth != 0 || i.Address != default || i.Extra != new JitOperand(0, fop, IsImmediate: true))
            throw new ArgumentException("Invalid canonical raw80 stack IR");
        if (!CanonicalX87StackLayout.Supported)
            throw new PlatformNotSupportedException("Canonical x87 Machine/file/frame layout differs");
        return true;
    }

    private static unsafe bool CanonicalX87CodeSelectorLayout()
    {
        // Machine.cs is a generated ref property, not a Marshal.OffsetOf field.
        Blink.Machine machine = default;
        ref byte first = ref Unsafe.As<Blink.Machine, byte>(ref machine);
        ref byte selector = ref Unsafe.As<ushort, byte>(ref machine.cs.sel);
        return Unsafe.ByteOffset(ref first, ref selector) == 472;
    }

    private static class CanonicalX87StackLayout
    {
        internal static readonly bool Supported =
            Marshal.OffsetOf<Blink.Machine>(nameof(Blink.Machine.fpu)).ToInt32() == 584 &&
            Marshal.OffsetOf<Blink.MachineFpu>(nameof(Blink.MachineFpu.sw)).ToInt32() == 64 &&
            Marshal.OffsetOf<Blink.MachineFpu>(nameof(Blink.MachineFpu.tw)).ToInt32() == 68 &&
            Marshal.OffsetOf<Blink.MachineFpu>(nameof(Blink.MachineFpu.op)).ToInt32() == 72 &&
            Marshal.OffsetOf<Blink.MachineFpu>(nameof(Blink.MachineFpu.ip)).ToInt32() == 80 &&
            Marshal.OffsetOf<Blink.MachineFpu>(nameof(Blink.MachineFpu.cw)).ToInt32() == 88 &&
            Marshal.OffsetOf<Blink.MachineFpu>(nameof(Blink.MachineFpu.dp)).ToInt32() == 96 &&
            Marshal.OffsetOf<Blink.Machine>(nameof(Blink.Machine.xmm)).ToInt32() == 160 &&
            Marshal.SizeOf<FullBlockFrame>() == 416 &&
            Marshal.OffsetOf<FullBlockFrame>(nameof(FullBlockFrame.X87)).ToInt32() == 400 &&
            Marshal.OffsetOf<FullBlockFrame>(nameof(FullBlockFrame.X87Reserved)).ToInt32() == 408 &&
            Marshal.SizeOf<FullX87RegisterFile>() == 192 && Marshal.SizeOf<FullExtended80>() == 16 &&
            Marshal.OffsetOf<FullX87RegisterFile>(nameof(FullX87RegisterFile.R0)).ToInt32() == 64 &&
            Marshal.OffsetOf<FullX87RegisterFile>(nameof(FullX87RegisterFile.R7)).ToInt32() == 176 &&
            Marshal.OffsetOf<FullExtended80>(nameof(FullExtended80.Significand)).ToInt32() == 0 &&
            Marshal.OffsetOf<FullExtended80>(nameof(FullExtended80.SignExponent)).ToInt32() == 8 &&
            Marshal.OffsetOf<FullX87RegisterFile>(nameof(FullX87RegisterFile.CodeSelector)).ToInt32() == 40 &&
            Marshal.OffsetOf<FullX87RegisterFile>(nameof(FullX87RegisterFile.DataSelector)).ToInt32() == 42 &&
            CanonicalX87CodeSelectorLayout();
    }

    private ref partial struct Writer
    {
        // Private reuse/clobber contract: D/API.txt. All operations below emit
        // native integer or SSE2 BIT transfers; there is no host FP environment.
        // Parent must RequireCanonicalX87 BEFORE Issue and authenticate ABI7.
        private bool TryCanonicalX87StackOperation(in JitInstruction i)
        {
            if (!CanInlineCanonicalX87Stack(i)) return false;
            var op = (CX)(i.Options & 0xffff); int n = i.Source.Register;
            if (op is not (CX.Fninit or CX.Fnclex or CX.FnstswAx)) FloatMmxWaitingException(i);
            switch (op)
            {
                case CX.Fwait: return true; // WAIT does not update FIP/FOP/FDP.
                case CX.Fninit:
                    CanonicalX87MachineConstant(648, 0); CanonicalX87MachineConstant(652, 0xffff);
                    CanonicalX87MachineConstant(656, 0); CanonicalX87MachineConstant(672, 0x37f);
                    IntegerImmediate(0, 0); CanonicalX87MachineStore(664, 0, 64); CanonicalX87MachineStore(680, 0, 64);
                    IntegerFrameLoad(400, 0); Bytes(0xc7, 0x40, 40); U32(0); // FCS/FDS only
                    return true;
                case CX.Fnclex:
                    CanonicalX87MachineLoad(648, 0); IntegerConstant(4, 0, ~0x80ffu);
                    CanonicalX87MachineStore(648, 0); return true;
                case CX.FnstswAx:
                    CanonicalX87MachineLoad(648, 8); Store(new(0, 0), 16); return true;
            }
            CanonicalX87Metadata(i);
            // Undefined CC are retained by this deterministic software profile.
            if (op is not (CX.Ffree or CX.Ffreep or CX.Fnop)) CanonicalX87ClearC1();
            if (op == CX.Fnop) return true;
            if (op == CX.Ffree) { CanonicalX87SetTag(n, 3); return true; }
            if (op == CX.Ffreep)
            {
                // AMD 22007I p146: FFREE ST(i), then increment TOP. This is
                // NOT CanonicalX87Pop(): the old ST0 tag survives for i != 0.
                // No value read/empty check/IE/SF; raw80 and padding survive.
                // Selected legacy FpuPop disagrees (see R174 domain record).
                CanonicalX87SetTag(n, 3); CanonicalX87MoveTop(1); return true;
            }
            if (op is CX.Fincstp or CX.Fdecstp) { CanonicalX87MoveTop(op == CX.Fincstp ? 1 : -1); return true; }

            int underflow = -1, secondUnderflow = -1, overflow = -1;
            if (op is CX.Fld or CX.Fst or CX.Fstp or CX.Fxch or CX.Fchs or CX.Fabs)
                underflow = CanonicalX87SourceCheck(op == CX.Fld ? n : 0);
            if (op == CX.Fxch) secondUnderflow = CanonicalX87SourceCheck(n);
            bool push = op == CX.Fld || FullCanonicalX87StackLowering.IsConstant(op);
            if (push) overflow = CanonicalX87PushCheck();

            if (op == CX.Fld) { CanonicalX87LoadRaw(n, 0); CanonicalX87PushRaw(0); }
            else if (FullCanonicalX87StackLowering.IsConstant(op)) { CanonicalX87Constant(op, 0); CanonicalX87PushRaw(0); }
            else if (op is CX.Fst or CX.Fstp)
            {
                CanonicalX87LoadRaw(0, 0); CanonicalX87StoreRaw(n, 0);
                if (op == CX.Fstp) CanonicalX87Pop();
            }
            else if (op == CX.Fxch)
            {
                CanonicalX87LoadRaw(0, 0); CanonicalX87LoadRaw(n, 1);
                CanonicalX87StoreRaw(0, 1); CanonicalX87StoreRaw(n, 0);
            }
            else
            {
                CanonicalX87Address(0); Bytes(0x66, 0x81, op == CX.Fchs ? (byte)0x70 : (byte)0x60, 8);
                Bytes(op == CX.Fchs ? (byte)0x00 : (byte)0xff, op == CX.Fchs ? (byte)0x80 : (byte)0x7f);
            }
            int success = IntegerJump();
            int suppressedUnderflow = -1, handledUnderflow = -1;
            if (underflow >= 0)
            {
                Patch(underflow, position); if (secondUnderflow >= 0) Patch(secondUnderflow, position);
                suppressedUnderflow = CanonicalX87StackFault(false);
                if (op == CX.Fxch)
                {
                    // Intel Vol1 Table8-10: replace EACH empty input, then exchange.
                    CanonicalX87LoadOrIndefinite(0, 0); CanonicalX87LoadOrIndefinite(n, 1);
                    CanonicalX87StoreRaw(0, 1); CanonicalX87StoreRaw(n, 0);
                }
                else
                {
                    CanonicalX87Indefinite(0);
                    if (push) CanonicalX87PushRaw(0);
                    else
                    {
                        CanonicalX87StoreRaw(op is CX.Fst or CX.Fstp ? n : 0, 0);
                        if (op == CX.Fstp) CanonicalX87Pop();
                    }
                }
                handledUnderflow = IntegerJump();
            }
            int suppressedOverflow = -1;
            if (overflow >= 0)
            {
                Patch(overflow, position); suppressedOverflow = CanonicalX87StackFault(true);
                CanonicalX87Indefinite(0); CanonicalX87PushRaw(0);
            }
            Patch(success, position);
            if (suppressedUnderflow >= 0) Patch(suppressedUnderflow, position);
            if (handledUnderflow >= 0) Patch(handledUnderflow, position);
            if (suppressedOverflow >= 0) Patch(suppressedOverflow, position);
            return true;
        }

        private void CanonicalX87MachineLoad(int offset, int register)
        { IntegerPrefix(32, 11, register); Bytes(0x8b); Memory(register & 7, 3, offset); }
        private void CanonicalX87MachineStore(int offset, int register, int width = 32)
        { IntegerPrefix(width, 11, register); Bytes(0x89); Memory(register & 7, 3, offset); }
        private void CanonicalX87MachineConstant(int offset, uint value)
        { Bytes(0x41, 0xc7, 0x83); U32((uint)offset); U32(value); }

        // RAX=canonical address, ECX scratch; all logical indices modulo8.
        // No indexed loads ever address the independent Machine XMM region.
        private void CanonicalX87Address(int logical)
        {
            CanonicalX87MachineLoad(648, 1); IntegerShift(5, 1, 32, 11);
            IntegerConstant(0, 1, (uint)logical); IntegerConstant(4, 1, 7);
            IntegerShift(4, 1, 32, 4); IntegerConstant(0, 1, 64);
            IntegerFrameLoad(400, 0); IntegerRR(0x01, 0, 1);
        }
        private void CanonicalX87GetTag(int logical)
        {
            CanonicalX87MachineLoad(648, 1); IntegerShift(5, 1, 32, 11);
            IntegerConstant(0, 1, (uint)logical); IntegerConstant(4, 1, 7); IntegerShift(4, 1, 32, 1);
            CanonicalX87MachineLoad(652, 0); IntegerShift(5, 0, 32); IntegerConstant(4, 0, 3);
        }
        private void CanonicalX87SetTag(int logical, int tag)
        {
            CanonicalX87MachineLoad(648, 1); IntegerShift(5, 1, 32, 11);
            IntegerConstant(0, 1, (uint)logical); IntegerConstant(4, 1, 7); IntegerShift(4, 1, 32, 1);
            IntegerImmediate(2, 3, 32); IntegerShift(4, 2, 32); IntegerUnary(2, 2, 32);
            CanonicalX87MachineLoad(652, 0); IntegerRR(0x21, 0, 2, 32);
            IntegerImmediate(2, (uint)tag, 32); IntegerShift(4, 2, 32); IntegerRR(0x09, 0, 2, 32);
            CanonicalX87MachineStore(652, 0);
        }
        private void CanonicalX87LoadRaw(int logical, int xmm)
        {
            CanonicalX87Address(logical);
            Bytes(0xf3, 0x0f, 0x7e, (byte)(xmm << 3)); // movq xmm,[rax], zero upper64
            Bytes(0x66, 0x0f, 0xc4, (byte)(0x40 | xmm << 3), 8, 4); // pinsrw xmm,[rax+8],4
        }
        private void CanonicalX87StoreRaw(int logical, int xmm)
        {
            CanonicalX87Address(logical);
            Bytes(0x66, 0x0f, 0xd6, (byte)(xmm << 3)); // movq [rax],xmm
            Bytes(0x66, 0x0f, 0xc5, (byte)(0xc8 | xmm), 4); // pextrw ecx,xmm,4
            Bytes(0x66, 0x89, 0x48, 8); // preserve slot padding bytes10..15
            CanonicalX87ClassifyTag(logical, xmm);
        }
        private void CanonicalX87ClassifyTag(int logical, int xmm)
        {
            Bytes(0x66, 0x49, 0x0f, 0x7e, (byte)(0xc0 | xmm << 3)); // movq r8,xmm
            Bytes(0x66, 0x44, 0x0f, 0xc5, (byte)(0xc8 | xmm), 4); // pextrw r9d,xmm,4
            IntegerConstant(4, 9, 0x7fff); IntegerRR(0x85, 9, 9, 32); int expZero = NearCondition(0x84);
            IntegerConstant(7, 9, 0x7fff); int expSpecial = NearCondition(0x84);
            IntegerRR(0x85, 8, 8); int unsupported = NearCondition(0x89); // J=0 (nonnegative sig)
            CanonicalX87SetTag(logical, 0); int normal = IntegerJump();
            Patch(expZero, position); IntegerRR(0x85, 8, 8); int nonzero = NearCondition(0x85);
            CanonicalX87SetTag(logical, 1); int zero = IntegerJump();
            Patch(expSpecial, position); Patch(unsupported, position); Patch(nonzero, position);
            CanonicalX87SetTag(logical, 2);
            Patch(normal, position); Patch(zero, position);
        }
        private void CanonicalX87Indefinite(int xmm)
            => CanonicalX87Literal(xmm, 0xc000000000000000UL, 0xffff);
        private void CanonicalX87Literal(int xmm, ulong significand, ushort signExponent)
        {
            IntegerImmediate(0, significand); Bytes(0x66, 0x48, 0x0f, 0x6e, (byte)(0xc0 | xmm << 3));
            IntegerImmediate(1, signExponent, 32); Bytes(0x66, 0x0f, 0xc4, (byte)(0xc1 | xmm << 3), 4);
        }
        private void CanonicalX87LoadOrIndefinite(int logical, int xmm)
        {
            int empty = CanonicalX87SourceCheck(logical); CanonicalX87LoadRaw(logical, xmm); int done = IntegerJump();
            Patch(empty, position); CanonicalX87Indefinite(xmm); Patch(done, position);
        }
        private int CanonicalX87SourceCheck(int logical)
        { CanonicalX87GetTag(logical); IntegerConstant(7, 0, 3); return NearCondition(0x84); }
        private int CanonicalX87PushCheck()
        { CanonicalX87GetTag(7); IntegerConstant(7, 0, 3); return NearCondition(0x85); }
        private int CanonicalX87StackFault(bool overflow)
        {
            CanonicalX87MachineLoad(648, 0); IntegerConstant(4, 0, ~0x200u);
            IntegerConstant(1, 0, overflow ? 0x241u : 0x41u); CanonicalX87MachineStore(648, 0);
            // ES/B only for unmasked IE. Original TOP/destinations must survive.
            CanonicalX87MachineLoad(672, 0); IntegerConstant(4, 0, 1); int masked = NearCondition(0x85);
            CanonicalX87MachineLoad(648, 0); IntegerConstant(1, 0, 0x8080); CanonicalX87MachineStore(648, 0);
            Patch(masked, position);
            CanonicalX87MachineLoad(672, 0); IntegerConstant(4, 0, 1); return NearCondition(0x84);
        }
        private void CanonicalX87MoveTop(int delta)
        {
            CanonicalX87MachineLoad(648, 0); IntegerMov(1, 0, 32);
            IntegerConstant(0, 1, unchecked((uint)(delta * 0x800))); IntegerConstant(4, 1, 0x3800);
            IntegerConstant(4, 0, ~0x3800u); IntegerRR(0x09, 0, 1, 32); CanonicalX87MachineStore(648, 0);
        }
        private void CanonicalX87Pop() { CanonicalX87SetTag(0, 3); CanonicalX87MoveTop(1); }
        private void CanonicalX87PushRaw(int xmm) { CanonicalX87MoveTop(-1); CanonicalX87StoreRaw(0, xmm); }
        private void CanonicalX87ClearC1()
        { CanonicalX87MachineLoad(648, 0); IntegerConstant(4, 0, ~0x200u); CanonicalX87MachineStore(648, 0); }
        private void CanonicalX87Metadata(in JitInstruction i)
        {
            IntegerImmediate(0, i.Ip); CanonicalX87MachineStore(664, 0, 64);
            CanonicalX87MachineConstant(656, (uint)i.Extra.Immediate);
            // Selector-preserving, non-deprecated FCS profile; CS belongs to
            // instruction metadata, independently of undefined register-only FDP/FDS.
            Bytes(0x41, 0x0f, 0xb7, 0x83); U32(472); // movzx eax,word [r11+cs.sel]
            IntegerFrameLoad(400, 1); Bytes(0x66, 0x89, 0x41, 40); // file.FCS=ax
        }
        private void CanonicalX87Constant(CX op, int xmm)
        {
            // Positive internal constants rounded to64 by RC; PC is immaterial.
            // No guest PE or rounded-up C1 contribution from constant loads.
            (ulong lower, ushort exponent, bool nearestUp) = op switch
            {
                CX.Fld1 => (0x8000000000000000UL, (ushort)0x3fff, false),
                CX.Fldl2t => (0xd49a784bcd1b8afeUL, (ushort)0x4000, false),
                CX.Fldl2e => (0xb8aa3b295c17f0bbUL, (ushort)0x3fff, true),
                CX.Fldpi => (0xc90fdaa22168c234UL, (ushort)0x4000, true),
                CX.Fldlg2 => (0x9a209a84fbcff798UL, (ushort)0x3ffd, true),
                CX.Fldln2 => (0xb17217f7d1cf79abUL, (ushort)0x3ffe, true),
                CX.Fldz => (0UL, (ushort)0, false),
                _ => throw new ArgumentException("Non-constant canonical x87 operation")
            };
            if (op is CX.Fld1 or CX.Fldz) { CanonicalX87Literal(xmm, lower, exponent); return; }
            CanonicalX87MachineLoad(672, 0); IntegerShift(5, 0, 32, 10); IntegerConstant(4, 0, 3);
            IntegerConstant(7, 0, 2); int up = NearCondition(0x84);
            int nearest = -1;
            if (nearestUp) { IntegerRR(0x85, 0, 0, 32); nearest = NearCondition(0x84); }
            CanonicalX87Literal(xmm, lower, exponent); int down = IntegerJump();
            Patch(up, position); if (nearest >= 0) Patch(nearest, position);
            CanonicalX87Literal(xmm, lower + 1, exponent); Patch(down, position);
        }
    }
}
