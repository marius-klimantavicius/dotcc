using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

// R116 ordinary x87 computation in the inherited binary64 storage profile.
// This is NOT an 80-bit architectural state implementation. No host x87, MMX,
// CALL, stack adjustment, optional SIMD, or nonvolatile scratch is emitted.
internal static partial class FullBlockEmitter
{
    internal enum FullX87Operation : ushort
    {
        Load = 0x4001, Store, Exchange, Free, IncrementTop, DecrementTop,
        One, Log2Ten, Log2E, Pi, Log10Two, LnTwo, Zero, ChangeSign, Absolute,
        Add, Multiply, Subtract, SubtractReverse, Divide, DivideReverse,
        Compare, CompareUnordered, CompareFlags, CompareUnorderedFlags, Test,
        MoveBelow, MoveEqual, MoveBelowEqual, MoveUnordered,
        MoveNotBelow, MoveNotEqual, MoveNotBelowEqual, MoveOrdered,
        LoadInteger, StoreInteger, StoreIntegerTruncate,
        LoadControl, StoreControl, StoreStatus, StoreStatusAx,
        LoadEnvironment, StoreEnvironment, Save, Restore, Wait, Initialize,
        ClearExceptions, Nop, Examine, Extract, Round, Scale, Remainder,
        RemainderNearest, SquareRoot
    }
    internal const uint X87IntegerMemory = 1u << 18, X87Environment16 = 1u << 19;
    internal static bool CanInlineX87(JitInstruction i) => i.Operation == JitOperation.X87;
    internal static FullX87Operation X87Operation(JitInstruction i) => (FullX87Operation)(i.Options & 0xffff);
    internal static int X87Pops(JitInstruction i) => (int)(i.Options >> 16 & 3);
    private static bool X87Stack(JitOperand p) => VectorRegister(p) && p.Register < 8;
    private static bool X87Control(FullX87Operation op) => op is FullX87Operation.LoadControl or FullX87Operation.StoreControl or
        FullX87Operation.StoreStatus or FullX87Operation.StoreStatusAx or FullX87Operation.LoadEnvironment or
        FullX87Operation.StoreEnvironment or FullX87Operation.Save or FullX87Operation.Restore or
        FullX87Operation.Wait or FullX87Operation.Initialize or FullX87Operation.ClearExceptions;
    private static bool X87NonWaiting(FullX87Operation op) => op is FullX87Operation.StoreControl or FullX87Operation.StoreStatus or
        FullX87Operation.StoreStatusAx or FullX87Operation.StoreEnvironment or FullX87Operation.Save or
        FullX87Operation.Initialize or FullX87Operation.ClearExceptions;

    internal static bool ValidateX87(JitInstruction i)
    {
        if (!CanInlineX87(i)) return false;
        var op = X87Operation(i); int pops = X87Pops(i);
        bool memory = i.Source.IsMemory || i.Destination.IsMemory;
        bool env = op is FullX87Operation.LoadEnvironment or FullX87Operation.StoreEnvironment or FullX87Operation.Save or FullX87Operation.Restore;
        if (i.Width != 64 || i.Length is < 1 or > 15 || i.Target != 0 || i.Condition != 0 ||
            (i.Options & ~0xfffffu) != 0 || op is < FullX87Operation.Load or > FullX87Operation.SquareRoot ||
            !(X87Stack(i.Source) || VectorMemory(i.Source)) || !(X87Stack(i.Destination) || VectorMemory(i.Destination)) ||
            i.Source.IsMemory && i.Destination.IsMemory || pops > 2 ||
            pops == 2 && op is not (FullX87Operation.Compare or FullX87Operation.CompareUnordered) ||
            pops != 0 && op is not (FullX87Operation.Store or FullX87Operation.Free or FullX87Operation.Add or FullX87Operation.Multiply or
                FullX87Operation.Subtract or FullX87Operation.SubtractReverse or FullX87Operation.Divide or FullX87Operation.DivideReverse or
                FullX87Operation.Compare or FullX87Operation.CompareUnordered or FullX87Operation.CompareFlags or FullX87Operation.CompareUnorderedFlags or
                FullX87Operation.StoreInteger or FullX87Operation.StoreIntegerTruncate))
            throw new ArgumentException("Native x87 operation/operands/pops");
        if (op == FullX87Operation.Wait ? i.Extra != default :
            !i.Extra.IsImmediate || i.Extra.IsMemory || i.Extra.Register != 0 || i.Extra.ByteOffset != 0 || i.Extra.Immediate > 0x7ff)
            throw new ArgumentException("Native x87 compile-time FOP metadata");
        bool intMemory = (i.Options & X87IntegerMemory) != 0;
        if (intMemory && (!i.Source.IsMemory || op is < FullX87Operation.Add or > FullX87Operation.Compare) ||
            (i.Options & X87Environment16) != 0 && !env)
            throw new ArgumentException("Native x87 memory kind");
        if (op == FullX87Operation.Store && i.Source != default ||
            op is FullX87Operation.Compare or FullX87Operation.CompareUnordered or FullX87Operation.CompareFlags or
                FullX87Operation.CompareUnorderedFlags or FullX87Operation.Load or FullX87Operation.LoadInteger or FullX87Operation.Exchange or
                FullX87Operation.Free or >= FullX87Operation.MoveBelow and <= FullX87Operation.MoveOrdered && i.Destination != default ||
            !memory && op is not (FullX87Operation.Store or FullX87Operation.Add or FullX87Operation.Multiply or FullX87Operation.Subtract or
                FullX87Operation.SubtractReverse or FullX87Operation.Divide or FullX87Operation.DivideReverse or FullX87Operation.Compare or
                FullX87Operation.CompareUnordered or FullX87Operation.CompareFlags or FullX87Operation.CompareUnorderedFlags or
                FullX87Operation.Load or FullX87Operation.Exchange or FullX87Operation.Free or
                >= FullX87Operation.MoveBelow and <= FullX87Operation.MoveOrdered) && (i.Source != default || i.Destination != default))
            throw new ArgumentException("Native x87 unused/fixed operands");
        if (memory)
        {
            if (i.Destination.IsMemory != FullX87Lowering.IsMemoryWrite(op))
                throw new ArgumentException("Native x87 memory direction");
            int w = i.SourceWidth;
            bool validWidth = op switch
            {
                FullX87Operation.Load or FullX87Operation.Store => w is 32 or 64 or 80,
                FullX87Operation.LoadInteger or FullX87Operation.StoreInteger or FullX87Operation.StoreIntegerTruncate => w is 16 or 32 or 64,
                FullX87Operation.LoadControl or FullX87Operation.StoreControl or FullX87Operation.StoreStatus => w == 16,
                FullX87Operation.LoadEnvironment or FullX87Operation.StoreEnvironment => w == ((i.Options & X87Environment16) != 0 ? 112 : 224),
                FullX87Operation.Save or FullX87Operation.Restore => w == ((i.Options & X87Environment16) != 0 ? 752 : 864),
                >= FullX87Operation.Add and <= FullX87Operation.Compare => intMemory ? w is 16 or 32 : w is 32 or 64,
                _ => false
            };
            var a = i.Address;
            if (!validWidth || a.Base is < -1 or > 15 || a.Index is < -1 or > 15 || a.Scale is < 0 or > 3 ||
                a.AddressWidth is not (32 or 64) || a.Segment is < -1 or > 5 || a.RipRelative && (a.Base != -1 || a.Index != -1))
                throw new ArgumentException("Native x87 exact memory width/address");
        }
        else if (i.SourceWidth != 0 || i.Address != default || env || intMemory ||
            op is FullX87Operation.LoadInteger or FullX87Operation.StoreInteger or FullX87Operation.StoreIntegerTruncate or
                FullX87Operation.LoadControl or FullX87Operation.StoreControl or FullX87Operation.StoreStatus)
            throw new ArgumentException("Native x87 unused/missing memory");
        if (!X87Layout.Supported) throw new PlatformNotSupportedException("Translated x87/frame layout differs");
        return true;
    }
    private static class X87Layout { internal static readonly bool Supported = HasExpectedX87Layout(); }
    internal static unsafe bool HasExpectedX87Layout()
    {
        Blink.Machine m = default;
        ref byte b = ref Unsafe.As<Blink.Machine, byte>(ref m);
        return HasExpectedFloatLayout() && sizeof(Blink.MachineFpu) == 104 &&
            Unsafe.ByteOffset(ref b, ref Unsafe.As<double, byte>(ref m.fpu.st[0])) == 584 &&
            Unsafe.ByteOffset(ref b, ref Unsafe.As<uint, byte>(ref m.fpu.sw)) == 648 &&
            Unsafe.ByteOffset(ref b, ref Unsafe.As<int, byte>(ref m.fpu.tw)) == 652 &&
            Marshal.OffsetOf<Blink.Machine>(nameof(Blink.Machine.fpu)).ToInt32() == 584 &&
            Marshal.OffsetOf<Blink.MachineFpu>(nameof(Blink.MachineFpu.op)).ToInt32() == 72 &&
            Marshal.OffsetOf<Blink.MachineFpu>(nameof(Blink.MachineFpu.ip)).ToInt32() == 80 &&
            Marshal.OffsetOf<Blink.MachineFpu>(nameof(Blink.MachineFpu.cw)).ToInt32() == 88 &&
            Marshal.OffsetOf<Blink.MachineFpu>(nameof(Blink.MachineFpu.dp)).ToInt32() == 96;
    }

    private ref partial struct Writer
    {
        private void X87LoadField(int reg, int offset, int width = 32)
        { IntegerPrefix(width, 11, reg); Bytes(0x8b); Memory(reg & 7, 3, offset); }
        private void X87StoreField(int reg, int offset, int width = 32)
        { IntegerPrefix(width, 11, reg); Bytes(0x89); Memory(reg & 7, 3, offset); }
        private void X87Bits(int offset, int group, uint bits)
        { Bytes(0x41, 0x81, (byte)(0x83 | group << 3)); U32((uint)offset); U32(bits); }
        private void X87Status(uint bits) => X87Bits(648, 1, bits);
        private void X87Constant(int xmm, ulong bits)
        { IntegerImmediate(0, bits); FloatFromInteger(xmm, 0, 64); }
        private void X87Index(int logical)
        {
            X87LoadField(0, 648); IntegerShift(5, 0, 32, 11);
            if (logical != 0) IntegerConstant(0, 0, unchecked((uint)logical));
            IntegerConstant(4, 0, 7);
        }
        // RAX=physical index, ECX=tag shift, EDX=tag. TOP is read afresh after
        // every push/pop; register aliases and ST(0) destinations are legal.
        private void X87Tag(int logical)
        {
            X87Index(logical); IntegerMov(1, 0, 32); IntegerShift(4, 1, 32, 1);
            X87LoadField(2, 652); IntegerShift(5, 2, 32); IntegerConstant(4, 2, 3);
        }
        private void X87SetTag(int logical, int tag)
        {
            X87Index(logical); IntegerMov(1, 0, 32); IntegerShift(4, 1, 32, 1);
            IntegerImmediate(2, 3, 32); IntegerShift(4, 2, 32); IntegerUnary(2, 2, 32);
            X87LoadField(0, 652); IntegerRR(0x21, 0, 2, 32);
            IntegerImmediate(2, (uint)tag, 32); IntegerShift(4, 2, 32);
            IntegerRR(0x09, 0, 2, 32); X87StoreField(0, 652);
        }
        private void X87Transfer(int xmm, bool store)
        { Bytes(0xf2, 0x41, 0x0f, store ? (byte)0x11 : (byte)0x10, (byte)(0x84 | xmm << 3), 0xc3); U32(584); }
        private void X87StLoad(int xmm, int logical, bool check = true)
        {
            if (!check) { X87Index(logical); X87Transfer(xmm, false); return; }
            X87Tag(logical); IntegerConstant(7, 2, 3);
            int occupied = NearCondition(0x85);
            X87Status(0x41); X87Bits(648, 4, ~0x200u);
            X87Constant(xmm, 0xfff8000000000000); int done = NativeNearJump();
            Patch(occupied, position); X87Transfer(xmm, false); Patch(done, position);
        }
        private void X87StStore(int xmm, int logical, bool retag = true)
        {
            X87Index(logical); X87Transfer(xmm, true);
            if (!retag) return;
            FloatToInteger(8, xmm, 64); IntegerMov(9, 8);
            IntegerShift(4, 9, 64, 1); IntegerRR(0x85, 9, 9);
            int zero = NearCondition(0x84);
            IntegerShift(5, 9, 64, 53); IntegerConstant(7, 9, 2047);
            int special = NearCondition(0x84);
            IntegerRR(0x85, 9, 9, 32); int denormal = NearCondition(0x84);
            X87SetTag(logical, 0); int done = NativeNearJump();
            Patch(zero, position); X87SetTag(logical, 1); int doneZero = NativeNearJump();
            Patch(special, position); Patch(denormal, position); X87SetTag(logical, 2);
            Patch(done, position); Patch(doneZero, position);
        }
        private void X87AdjustTop(int delta)
        {
            X87LoadField(0, 648); IntegerMov(2, 0, 32);
            IntegerConstant(0, 2, unchecked((uint)(delta << 11))); IntegerConstant(4, 2, 0x3800);
            IntegerConstant(4, 0, ~0x3800u); IntegerRR(0x09, 0, 2, 32); X87StoreField(0, 648);
        }
        private void X87Pop()
        { X87SetTag(0, 3); X87AdjustTop(1); }
        private void X87Push(int xmm)
        {
            X87Tag(-1); IntegerConstant(7, 2, 3); int space = NearCondition(0x84);
            X87Status(0x241); X87Constant(xmm, 0xfff8000000000000);
            Patch(space, position); int abort = X87FinishExceptions();
            X87AdjustTop(-1); X87StStore(xmm, 0); Patch(abort, position);
        }
        // Borrow the parent's authenticated MXCSR lifetime/RET cleanup. Guest
        // MXCSR is never installed or changed. All six host exceptions masked,
        // DAZ/FTZ off; x87 RC is mapped to SSE RC. PC cannot be represented by
        // the inherited double[8] state (an explicit acceptance blocker).
        private void X87BeginEnvironment()
        {
            Bytes(0x41, 0x83, 0xba); U32(360); Bytes(0); int saved = NearCondition(0x85);
            FloatMxcsr(3, 352); FloatFrameConstant(360, 1); Patch(saved, position);
            X87LoadField(0, 672); IntegerConstant(4, 0, 0xc00); IntegerShift(4, 0, 32, 3);
            IntegerConstant(1, 0, 0x1f80); FloatFrameStore(0, 356); FloatMxcsr(2, 356);
            X87Bits(648, 4, ~0x200u);
        }
        private int X87FinishExceptions()
        {
            // A subnormal in the binary64 backing array is a normal x87
            // extended value. SSE's DE for that register representation must
            // not be copied to the guest. Memory source DE is recorded from
            // its original encoding by X87MemoryLoad instead.
            FloatMxcsr(3, 364); FloatFrameLoad(0, 364); IntegerConstant(4, 0, 61);
            X87LoadField(2, 648); IntegerRR(0x09, 0, 2, 32); X87StoreField(0, 648);
            X87LoadField(1, 672); IntegerUnary(2, 1, 32); IntegerRR(0x21, 0, 1, 32); IntegerConstant(4, 0, 63);
            int masked = NearCondition(0x84); X87Status(0x8080);
            int abort = NativeNearJump(); Patch(masked, position); return abort;
        }
        private void X87Context(JitInstruction i)
        {
            IntegerImmediate(0, i.Ip); X87StoreField(0, 664, 64);
            IntegerImmediate(0, i.Extra.Immediate, 32); X87StoreField(0, 656);
            if (i.Source.IsMemory || i.Destination.IsMemory)
            { IntegerFrameLoad(216, 0); X87StoreField(0, 680, 64); }
        }
        private void X87ReadBytes(int offset, int count)
        {
            IntegerImmediate(8, 0);
            for (int b = 0; b < count; ++b)
            {
                NativeBytePointer(offset + b); Bytes(0x0f, 0xb6, 0x02);
                if (b != 0) IntegerShift(4, 0, 64, b * 8); IntegerRR(0x09, 8, 0);
            }
        }
        private void X87WriteBytes(int offset, int count)
        {
            for (int b = 0; b < count; ++b)
            {
                NativeBytePointer(offset + b); IntegerMov(0, 8);
                if (b != 0) IntegerShift(5, 0, 64, b * 8); Bytes(0x88, 0x02);
            }
        }
        private void X87MemoryLoad(JitInstruction i)
        {
            if (i.SourceWidth == 80)
            {
                // DE belongs to the memory-source operation, not the shared
                // decoder contract used by restore/FXRSTOR. Check the original
                // binary80 encoding before narrowing into the backing model.
                X87ReadBytes(8, 2); IntegerConstant(4, 8, 0x7fff);
                int normal80 = NearCondition(0x85); X87ReadBytes(0, 8); IntegerRR(0x85, 8, 8);
                int zero80 = NearCondition(0x84); int pseudo80 = NearCondition(0x88);
                X87Status(2); Patch(normal80, position); Patch(zero80, position); Patch(pseudo80, position);
                X87Load80(0); return;
            }
            NativeMemoryLoad(i.SourceWidth);
            if (X87Operation(i) != FullX87Operation.LoadInteger && (i.Options & X87IntegerMemory) == 0)
            {
                // Test the original m32/m64 encoding before widening. Keep
                // RAX intact for the conversion and avoid evaluating NaNs.
                IntegerMov(8, 0); IntegerMov(9, 8);
                IntegerShift(4, 9, i.SourceWidth, 1); IntegerRR(0x85, 9, 9, i.SourceWidth);
                int zero = NearCondition(0x84);
                IntegerShift(5, 9, i.SourceWidth, i.SourceWidth == 32 ? 24 : 53);
                IntegerRR(0x85, 9, 9, 32); int normal = NearCondition(0x85);
                X87Status(2); Patch(zero, position); Patch(normal, position);
            }
            FloatFromInteger(1, 0, 64);
            if (X87Operation(i) == FullX87Operation.LoadInteger || (i.Options & X87IntegerMemory) != 0)
            {
                IntegerSignExtend(0, i.SourceWidth); Bytes(0xf2, 0x48, 0x0f, 0x2a, 0xc8);
            }
            else if (i.SourceWidth == 32) Bytes(0xf3, 0x0f, 0x5a, 0xc9);
            else X87QuietNaN(1);
        }

        private bool TryX87Operation(in JitInstruction i)
        {
            if (!CanInlineX87(i)) return false;
            var op = X87Operation(i); int pops = X87Pops(i);
            bool memory = i.Destination.IsMemory || i.Source.IsMemory;
            if (!X87NonWaiting(op)) FloatMmxWaitingException(i);
            if (memory)
            {
                ScalarMiscCheckCanonical(i, i.SourceWidth);
                NativeMemoryPrepare(i, i.Destination.IsMemory ? FullMemoryAccess.Write : FullMemoryAccess.Read, i.SourceWidth);
            }
            if (X87Control(op)) X87ControlOperation(i);
            else if (op == FullX87Operation.Nop) { /* FNOP is waiting; no context */ }
            else
            {
                X87Context(i); X87BeginEnvironment();
                if (op == FullX87Operation.IncrementTop || op == FullX87Operation.DecrementTop)
                    X87AdjustTop(op == FullX87Operation.IncrementTop ? 1 : -1);
                else if (op == FullX87Operation.Free)
                { X87SetTag(i.Source.Register, 3); if (pops != 0) X87Pop(); }
                else if (op is >= FullX87Operation.One and <= FullX87Operation.Zero)
                {
                    X87Constant(0, op switch
                    {
                        FullX87Operation.One => 0x3ff0000000000000, FullX87Operation.Log2Ten => 0x400a934f0979a371,
                        FullX87Operation.Log2E => 0x3ff71547652b82fe, FullX87Operation.Pi => 0x400921fb54442d18,
                        FullX87Operation.Log10Two => 0x3fd34413509f79ff, FullX87Operation.LnTwo => 0x3fe62e42fefa39ef, _ => 0
                    }); X87Push(0);
                }
                else if (op is >= FullX87Operation.MoveBelow and <= FullX87Operation.MoveOrdered)
                {
                    int condition = op switch
                    { FullX87Operation.MoveBelow => 2, FullX87Operation.MoveEqual => 4, FullX87Operation.MoveBelowEqual => 6,
                      FullX87Operation.MoveUnordered => 10, FullX87Operation.MoveNotBelow => 3, FullX87Operation.MoveNotEqual => 5,
                      FullX87Operation.MoveNotBelowEqual => 7, _ => 11 };
                    // Check BOTH stack operands even when the condition is false.
                    X87StLoad(0, 0); X87StLoad(1, i.Source.Register); Condition(condition);
                    IntegerRR(0x85, 0, 0, 32); int unchanged = NearCondition(0x84); FloatMove(0, 1); Patch(unchanged, position);
                    int abort = X87FinishExceptions(); X87StStore(0, 0); Patch(abort, position);
                }
                else if (op == FullX87Operation.Load || op == FullX87Operation.LoadInteger)
                {
                    if (memory) X87MemoryLoad(i); else X87StLoad(1, i.Source.Register);
                    X87Push(1);
                }
                else if (op == FullX87Operation.Examine) X87Examine();
                else
                {
                    X87StLoad(0, i.Destination.IsMemory || op == FullX87Operation.Store ? 0 : i.Destination.Register);
                    if (i.Source.IsMemory) X87MemoryLoad(i);
                    else if (op is >= FullX87Operation.Add and <= FullX87Operation.CompareUnorderedFlags || op == FullX87Operation.Exchange)
                        X87StLoad(1, i.Source.Register);
                    if (op == FullX87Operation.Test) X87Constant(1, 0);
                    if (op is >= FullX87Operation.Add and <= FullX87Operation.DivideReverse)
                    {
                        bool reverse = op is FullX87Operation.SubtractReverse or FullX87Operation.DivideReverse;
                        if (reverse) { FloatMove(2, 0); FloatMove(0, 1); FloatMove(1, 2); }
                        FloatScalarCode(op switch { FullX87Operation.Add => 0x58, FullX87Operation.Multiply => 0x59,
                            FullX87Operation.Subtract or FullX87Operation.SubtractReverse => 0x5c, _ => 0x5e }, 0, 1, 64);
                    }
                    else if (op is >= FullX87Operation.Compare and <= FullX87Operation.Test)
                        X87Compare(op);
                    else if (op == FullX87Operation.ChangeSign || op == FullX87Operation.Absolute)
                    {
                        FloatToInteger(8, 0, 64); IntegerImmediate(9, 0x8000000000000000);
                        if (op == FullX87Operation.Absolute) { IntegerUnary(2, 9, 64); IntegerRR(0x21, 8, 9); }
                        else IntegerRR(0x31, 8, 9); FloatFromInteger(0, 8, 64);
                    }
                    else if (op == FullX87Operation.SquareRoot) FloatScalarCode(0x51, 0, 0, 64);
                    else if (op == FullX87Operation.Round || op == FullX87Operation.StoreInteger) X87Round();
                    else if (op == FullX87Operation.Scale) { X87StLoad(1, 1); X87Scale(); }
                    else if (op is FullX87Operation.Remainder or FullX87Operation.RemainderNearest)
                    { X87StLoad(1, 1); X87Remainder(op == FullX87Operation.RemainderNearest); }
                    else if (op == FullX87Operation.Extract) X87Extract();
                    if (op is FullX87Operation.StoreInteger or FullX87Operation.StoreIntegerTruncate)
                        X87IntegerResult(i.SourceWidth);
                    else if (op == FullX87Operation.Store && memory && i.SourceWidth == 64) X87QuietNaN(0);
                    else if (op == FullX87Operation.Store && memory && i.SourceWidth == 32)
                        Bytes(0xf2, 0x0f, 0x5a, 0xc8); // CVTSD2SS before exception commit
                    int abort = X87FinishExceptions();
                    if (op is FullX87Operation.CompareFlags or FullX87Operation.CompareUnorderedFlags)
                    { FloatFrameLoad(0, 184); IntegerMergeFlags(0xff0008d1); }
                    else if (op is FullX87Operation.Compare or FullX87Operation.CompareUnordered or FullX87Operation.Test)
                    {
                        FloatFrameLoad(0, 192); X87LoadField(2, 648); IntegerConstant(4, 2, ~0x4700u);
                        IntegerRR(0x09, 0, 2, 32); X87StoreField(0, 648);
                    }
                    else if (op == FullX87Operation.Store)
                    {
                        // The source is ALWAYS ST0, including FST ST(i).
                        if (memory)
                        {
                            if (i.SourceWidth == 80) X87Store80(0, 0);
                            else { FloatToInteger(8, i.SourceWidth == 32 ? 1 : 0, 64); NativeMemoryStore(i.SourceWidth); }
                        }
                        else X87StStore(0, i.Destination.Register);
                    }
                    else if (op is FullX87Operation.StoreInteger or FullX87Operation.StoreIntegerTruncate)
                    { IntegerFrameLoad(192, 8); NativeMemoryStore(i.SourceWidth); }
                    else if (op == FullX87Operation.Exchange)
                    { X87StStore(0, i.Source.Register); X87StStore(1, 0); }
                    else if (op == FullX87Operation.Extract)
                    { X87StStore(0, 0); X87Push(1); }
                    else X87StStore(0, i.Destination.Register);
                    for (int n = 0; n < pops; ++n) X87Pop();
                    Patch(abort, position);
                }
            }
            if (memory) NativeMemoryCount(); return true;
        }

        private void X87Compare(FullX87Operation op)
        {
            bool unordered = op is FullX87Operation.CompareUnordered or FullX87Operation.CompareUnorderedFlags;
            Bytes(0x66, 0x0f, unordered ? (byte)0x2e : (byte)0x2f, 0xc1);
            FloatCaptureCompareFlags(); // 184 = CF/ZF/lazy PF, OF/SF/AF clear
            if (op is FullX87Operation.CompareFlags or FullX87Operation.CompareUnorderedFlags) return;
            FloatFrameLoad(0, 184); IntegerMov(1, 0, 32); IntegerConstant(4, 0, 1); IntegerShift(4, 0, 32, 8);
            IntegerConstant(4, 1, 64); IntegerShift(4, 1, 32, 8); IntegerRR(0x09, 0, 1, 32);
            FloatFrameLoad(1, 184); IntegerShift(5, 1, 32, 24); IntegerConstant(6, 1, 1);
            IntegerShift(4, 1, 32, 10); IntegerRR(0x09, 0, 1, 32);
            FloatFrameStore(0, 192);
        }

        private void X87QuietNaN(int xmm)
        {
            FloatToInteger(8, xmm, 64); IntegerMov(9, 8); IntegerShift(4, 9, 64, 1); IntegerShift(5, 9, 64, 1);
            IntegerImmediate(0, 0x7ff0000000000000); IntegerRR(0x39, 9, 0); int finite = NearCondition(0x86);
            IntegerMov(9, 8); IntegerShift(5, 9, 64, 51); IntegerConstant(4, 9, 1); int quiet = NearCondition(0x85);
            X87Status(1); IntegerImmediate(9, 0x8000000000000); IntegerRR(0x09, 8, 9); FloatFromInteger(xmm, 8, 64);
            Patch(finite, position); Patch(quiet, position);
        }

        // Baseline SSE2 rint: adding/subtracting signed 2^52 under guest RC.
        // Values already integral (including large values) bypass arithmetic.
        private void X87Round()
        {
            X87QuietNaN(0);
            FloatToInteger(8, 0, 64); IntegerMov(9, 8); IntegerShift(4, 9, 64, 1); IntegerShift(5, 9, 64, 53);
            IntegerConstant(7, 9, 1075); int integral = NearCondition(0x83);
            IntegerShift(5, 8, 64, 63); IntegerShift(4, 8, 64, 63); IntegerFrameStore(192, 8);
            IntegerImmediate(9, 0x4330000000000000); IntegerRR(0x09, 8, 9); FloatFromInteger(1, 8, 64);
            FloatScalarCode(0x58, 0, 1, 64); FloatScalarCode(0x5c, 0, 1, 64);
            FloatToInteger(8, 0, 64); IntegerShift(4, 8, 64, 1); IntegerShift(5, 8, 64, 1);
            IntegerFrameLoad(192, 9); IntegerRR(0x09, 8, 9); FloatFromInteger(0, 8, 64);
            Patch(integral, position);
        }
        private void X87IntegerResult(int width)
        {
            // CVTT has an architectural indefinite sentinel for invalid32/64.
            // 16-bit range is checked before narrowing; never wrap into int16.
            Bytes(0xf2); if (width == 64) Bytes(0x48); Bytes(0x0f, 0x2c, 0xc0);
            if (width == 16)
            {
                IntegerConstant(7, 0, unchecked((uint)-32768)); int low = NearCondition(0x8c);
                IntegerConstant(7, 0, 32767); int high = NearCondition(0x8f);
                int good = NativeNearJump(); Patch(low, position); Patch(high, position);
                X87Status(1); IntegerImmediate(0, 0x8000, 32); Patch(good, position);
            }
            IntegerFrameStore(192, 0);
        }

        private void X87Initialize()
        {
            IntegerImmediate(0, 0x37f, 32); X87StoreField(0, 672);
            IntegerImmediate(0, 0, 32); X87StoreField(0, 648); X87StoreField(0, 656);
            X87StoreField(0, 664, 64); X87StoreField(0, 680, 64);
            IntegerImmediate(0, uint.MaxValue, 32); X87StoreField(0, 652);
        }
        private void X87ControlOperation(JitInstruction i)
        {
            var op = X87Operation(i);
            switch (op)
            {
                case FullX87Operation.Wait: return;
                case FullX87Operation.Initialize: X87Initialize(); return;
                case FullX87Operation.ClearExceptions: X87Bits(648, 4, ~0x80ffu); return;
                case FullX87Operation.LoadControl:
                    NativeMemoryLoad(16); X87StoreField(0, 672); return;
                case FullX87Operation.StoreControl:
                case FullX87Operation.StoreStatus:
                    X87LoadField(8, op == FullX87Operation.StoreControl ? 672 : 648); NativeMemoryStore(16); return;
                case FullX87Operation.StoreStatusAx:
                    X87LoadField(8, 648); Store(default, 16); return;
            }
            bool narrow = (i.Options & X87Environment16) != 0;
            bool restore = op is FullX87Operation.LoadEnvironment or FullX87Operation.Restore;
            if (restore) X87RestoreEnvironment(narrow);
            else X87SaveEnvironment(narrow);
            if (op is FullX87Operation.Save or FullX87Operation.Restore)
            {
                // One full-span MMU prepare already established permissions,
                // lifetime and BOTH pages before any field/data was touched.
                int start = narrow ? 14 : 28;
                X87BeginEnvironment();
                for (int r = 0; r < 8; ++r)
                {
                    if (restore)
                    { X87Load80(start + r * 10); X87StStore(1, r, retag: false); }
                    else
                    { X87StLoad(0, r, check: false); X87Store80(start + r * 10, 0); }
                }
                if (!restore) X87Initialize();
                // FRSTOR's storage narrowing is an inherited model gap; the
                // saved SW/TW are restored literally, not recomputed from SSE.
                // Conversion helper sticky flags are deliberately not folded.
                if (restore) X87RestoreEnvironment(narrow);
            }
            if (op == FullX87Operation.StoreEnvironment) X87Bits(672, 1, 63);
        }
        private void X87SaveEnvironment(bool narrow)
        {
            X87LoadField(8, 672); X87WriteBytes(0, 2);
            X87LoadField(8, 648); X87WriteBytes(narrow ? 2 : 4, 2);
            X87LoadField(8, 652); X87WriteBytes(narrow ? 4 : 8, 2);
            X87LoadField(8, 664, 64); X87WriteBytes(narrow ? 6 : 12, narrow ? 2 : 4);
            X87LoadField(8, 680, 64); X87WriteBytes(narrow ? 10 : 20, narrow ? 2 : 4);
            IntegerImmediate(8, 0, 32); X87WriteBytes(narrow ? 8 : 16, 2); X87WriteBytes(narrow ? 12 : 24, 2);
            if (!narrow)
            {
                X87LoadField(8, 656); X87WriteBytes(18, 2);
                IntegerImmediate(8, 65535, 32);
                X87WriteBytes(2, 2); X87WriteBytes(6, 2); X87WriteBytes(10, 2); X87WriteBytes(26, 2);
            }
        }
        private void X87RestoreEnvironment(bool narrow)
        {
            X87ReadBytes(0, 2); X87StoreField(8, 672);
            X87ReadBytes(narrow ? 2 : 4, 2); X87StoreField(8, 648);
            X87ReadBytes(narrow ? 4 : 8, 2); X87StoreField(8, 652);
            X87ReadBytes(narrow ? 6 : 12, narrow ? 2 : 4); X87StoreField(8, 664, 64);
            X87ReadBytes(narrow ? 10 : 20, narrow ? 2 : 4); X87StoreField(8, 680, 64);
            if (!narrow) { X87ReadBytes(18, 2); IntegerConstant(4, 8, 0x7ff); X87StoreField(8, 656); }
        }

        private void X87Examine()
        {
            X87StLoad(0, 0, check: false); FloatToInteger(8, 0, 64);
            IntegerMov(9, 8); IntegerShift(5, 9, 64, 63); IntegerShift(4, 9, 32, 9);
            X87LoadField(2, 648); IntegerConstant(4, 2, ~0x4700u); IntegerRR(0x09, 9, 2, 32);
            // Keep sign/class result in a private scratch slot while reading TW.
            IntegerFrameStore(184, 9); X87Tag(0); IntegerConstant(7, 2, 3); int empty = NearCondition(0x84);
            FloatToInteger(8, 0, 64); IntegerShift(4, 8, 64, 1); IntegerRR(0x85, 8, 8);
            int zero = NearCondition(0x84);
            IntegerMov(9, 8); IntegerShift(5, 9, 64, 53); IntegerRR(0x85, 9, 9, 32);
            int subnormal = NearCondition(0x84); IntegerConstant(7, 9, 2047); int special = NearCondition(0x84);
            IntegerImmediate(8, 0x400, 32); int normalDone = NativeNearJump();
            Patch(special, position); IntegerShift(4, 8, 64, 11); IntegerRR(0x85, 8, 8);
            int infinity = NearCondition(0x84); IntegerImmediate(8, 0x100, 32); int nanDone = NativeNearJump();
            Patch(infinity, position); IntegerImmediate(8, 0x500, 32); int infDone = NativeNearJump();
            Patch(subnormal, position); IntegerImmediate(8, 0x4400, 32); int subDone = NativeNearJump();
            Patch(zero, position); IntegerImmediate(8, 0x4000, 32); int zeroDone = NativeNearJump();
            Patch(empty, position); IntegerImmediate(8, 0x4100, 32);
            Patch(normalDone, position); Patch(nanDone, position); Patch(infDone, position); Patch(subDone, position); Patch(zeroDone, position);
            IntegerFrameLoad(184, 9); IntegerRR(0x09, 8, 9, 32); X87StoreField(8, 648);
        }

        // Exact serialization of the inherited binary64 value to binary80.
        // Unlike selected ldbl.c, binary64 subnormals are NORMALIZED, and the
        // explicit integer bit is present for infinities/NaNs. No FP rounding.
        private void X87Store80(int offset, int xmm)
        {
            X87Encode80(xmm); FloatFromInteger(4, 9, 64); X87WriteBytes(offset, 8);
            FloatToInteger(8, 4, 64); X87WriteBytes(offset + 8, 2);
        }
        // Reusable by SystemState. Input low64 XMM[xmm]; output R8=significand,
        // R9=sign/exponent16. Clobbers volatile GPRs, XMM4 and frame184.
        // Performs no guest state or memory write.
        private void X87Encode80(int xmm)
        {
            FloatToInteger(8, xmm, 64); IntegerMov(9, 8); IntegerShift(5, 9, 64, 63); IntegerShift(4, 9, 32, 15);
            IntegerFrameStore(184, 9);
            IntegerMov(2, 8); IntegerShift(5, 2, 64, 52); IntegerConstant(4, 2, 2047);
            IntegerImmediate(0, 0xfffffffffffff); IntegerRR(0x21, 8, 0);
            IntegerRR(0x85, 2, 2, 32); int tiny = NearCondition(0x84);
            IntegerConstant(7, 2, 2047); int special = NearCondition(0x84);
            IntegerConstant(0, 2, 16383 - 1023); IntegerImmediate(0, 0x10000000000000); IntegerRR(0x09, 8, 0);
            IntegerShift(4, 8, 64, 11); int normalDone = NativeNearJump();
            Patch(special, position); IntegerImmediate(2, 32767, 32); IntegerShift(4, 8, 64, 11);
            IntegerImmediate(0, 0x8000000000000000); IntegerRR(0x09, 8, 0); int specialDone = NativeNearJump();
            Patch(tiny, position); IntegerRR(0x85, 8, 8); int zero = NearCondition(0x84);
            IntegerPrefix(64, 8, 1); Bytes(0x0f, 0xbd, 0xc8); // bsr rcx,r8
            IntegerMov(2, 1, 32); IntegerConstant(0, 2, 16383 - 1074);
            IntegerImmediate(0, 63, 32); IntegerRR(0x29, 0, 1, 32); IntegerMov(1, 0, 32); IntegerShift(4, 8, 64);
            Patch(zero, position); Patch(normalDone, position); Patch(specialDone, position);
            IntegerFrameLoad(184, 9); IntegerRR(0x09, 9, 2, 32);
        }

        // Binary80 -> binary64 is necessarily narrowing in this Machine model.
        // Integer guard/round/sticky logic honors RC without first converting
        // through a host x87 value or double-rounding an underflowed product.
        // Output XMM1. XMM3 holds output exponent and XMM4 holds sign.
        private void X87Load80(int offset)
        {
            X87ReadBytes(offset + 8, 2); IntegerMov(9, 8, 32);
            FloatFromInteger(3, 9, 64); X87ReadBytes(offset, 8); FloatToInteger(9, 3, 64); X87Decode80();
        }
        // Input R8=significand64,R9=sign/exponent16; output low64 XMM1.
        // Requires X87BeginEnvironment; clobbers volatile GPRs/XMM3/4/frame200.
        // May OR narrowing exceptions into SW. State restoration must reinstall
        // the literal saved SW after all conversions.
        private void X87Decode80()
        {
            IntegerMov(0, 9, 32); IntegerShift(5, 0, 32, 15); IntegerShift(4, 0, 64, 63); FloatFromInteger(4, 0, 64);
            IntegerConstant(4, 9, 32767); IntegerConstant(7, 9, 32767); int special = NearCondition(0x84);
            IntegerRR(0x85, 9, 9, 32); int exponentZero = NearCondition(0x84);
            IntegerRR(0x85, 8, 8); int unsupported = NearCondition(0x89); // positive => missing explicit integer bit
            int valid = NativeNearJump(); Patch(exponentZero, position); IntegerImmediate(9, 1, 32); Patch(valid, position);
            IntegerConstant(5, 9, 16383); IntegerConstant(7, 9, 1023); int overflow = NearCondition(0x8f);
            IntegerImmediate(1, 11, 32); IntegerConstant(7, 9, unchecked((uint)-1022)); int normal = NearCondition(0x8d);
            IntegerImmediate(0, unchecked((uint)-1022), 32); IntegerRR(0x29, 0, 9, 32);
            IntegerRR(0x01, 1, 0, 32); IntegerImmediate(9, unchecked((uint)-1023), 32);
            Patch(normal, position); IntegerConstant(0, 9, 1023); FloatFromInteger(3, 9, 64);
            X87RoundShift();
            FloatToInteger(9, 3, 64); IntegerRR(0x85, 9, 9, 32); int sub = NearCondition(0x84);
            IntegerImmediate(0, 0x20000000000000); IntegerRR(0x39, 8, 0); int noCarry = NearCondition(0x82);
            IntegerShift(5, 8, 64, 1); IntegerConstant(0, 9, 1); Patch(noCarry, position);
            IntegerConstant(7, 9, 2047); int roundedOverflow = NearCondition(0x83);
            IntegerImmediate(0, 0xfffffffffffff); IntegerRR(0x21, 8, 0); IntegerShift(4, 9, 64, 52); IntegerRR(0x09, 8, 9);
            int packed = NativeNearJump(); Patch(sub, position);
            // Rounded subnormal may carry into the minimum normal encoding.
            FloatFrameLoad(0, 200); IntegerConstant(4, 0, 32); int exactTiny = NearCondition(0x84);
            X87Status(16); Patch(exactTiny, position); int subDone = NativeNearJump();
            Patch(overflow, position); Patch(roundedOverflow, position); X87OverflowResult(); int overDone = NativeNearJump();
            Patch(special, position); IntegerRR(0x85, 8, 8); int unsupportedSpecial = NearCondition(0x89);
            IntegerMov(9, 8); IntegerShift(4, 9, 64, 1); IntegerRR(0x85, 9, 9); int infinity = NearCondition(0x84);
            IntegerMov(9, 8); IntegerShift(5, 9, 64, 62); IntegerConstant(4, 9, 1); int quiet = NearCondition(0x85);
            X87Status(1); Patch(quiet, position); IntegerShift(5, 8, 64, 11);
            IntegerImmediate(9, 0x7ff8000000000000); IntegerRR(0x09, 8, 9); int nan = NativeNearJump();
            Patch(infinity, position); IntegerImmediate(8, 0x7ff0000000000000); int infDone = NativeNearJump();
            Patch(unsupported, position); Patch(unsupportedSpecial, position); X87Status(1);
            IntegerImmediate(8, 0xfff8000000000000);
            Patch(packed, position); Patch(subDone, position); Patch(overDone, position); Patch(nan, position); Patch(infDone, position);
            FloatToInteger(9, 4, 64); IntegerRR(0x09, 8, 9); FloatFromInteger(1, 8, 64);
        }
        private void X87OverflowResult()
        {
            X87Status(0x28); X87LoadField(0, 672); IntegerShift(5, 0, 32, 10); IntegerConstant(4, 0, 3);
            IntegerConstant(7, 0, 0); int nearest = NearCondition(0x84);
            IntegerConstant(7, 0, 3); int truncate = NearCondition(0x84);
            FloatToInteger(9, 4, 64); IntegerShift(5, 9, 64, 63);
            IntegerRR(0x01, 0, 9, 32); IntegerConstant(7, 0, 2); int infinity = NearCondition(0x84);
            Patch(truncate, position); IntegerImmediate(8, 0x7fefffffffffffff); int done = NativeNearJump();
            Patch(nearest, position); Patch(infinity, position); IntegerImmediate(8, 0x7ff0000000000000); Patch(done, position);
        }
        private void X87RoundShift()
        {
            // R8 unsigned significand; ECX discard count >=11. Exponent/sign
            // remain in XMM3/4. No integer shift ever uses count64 modulo64.
            FloatFrameConstant(200, 0); IntegerConstant(7, 1, 64); int huge = NearCondition(0x83);
            IntegerImmediate(9, ulong.MaxValue); IntegerShift(4, 9, 64); IntegerUnary(2, 9, 64); IntegerRR(0x21, 9, 8);
            IntegerShift(5, 8, 64); IntegerRR(0x85, 9, 9); int exact = NearCondition(0x84);
            X87Status(32); FloatFrameConstant(200, 32);
            X87LoadField(0, 672); IntegerShift(5, 0, 32, 10); IntegerConstant(4, 0, 3); IntegerRR(0x85, 0, 0, 32);
            int directed = NearCondition(0x85);
            IntegerConstant(5, 1, 1); IntegerImmediate(0, 1); IntegerShift(4, 0, 64);
            IntegerRR(0x39, 9, 0); int increment = NearCondition(0x87); int less = NearCondition(0x82);
            IntegerMov(0, 8, 32); IntegerConstant(4, 0, 1); int even = NearCondition(0x84);
            int tie = NativeNearJump();
            Patch(huge, position); IntegerMov(9, 8); IntegerImmediate(8, 0); IntegerRR(0x85, 9, 9); int hugeZero = NearCondition(0x84);
            X87Status(32); FloatFrameConstant(200, 32); X87LoadField(0, 672); IntegerShift(5, 0, 32, 10); IntegerConstant(4, 0, 3);
            IntegerRR(0x85, 0, 0, 32); int hugeDirected = NearCondition(0x85);
            IntegerConstant(7, 1, 64); int far = NearCondition(0x87);
            IntegerImmediate(0, 0x8000000000000000); IntegerRR(0x39, 9, 0); int moreThanHalf = NearCondition(0x87); int halfOrLess = NativeNearJump();
            Patch(directed, position); Patch(hugeDirected, position);
            IntegerConstant(7, 0, 3); int trunc = NearCondition(0x84);
            FloatToInteger(2, 4, 64); IntegerShift(5, 2, 64, 63); IntegerRR(0x01, 0, 2, 32);
            IntegerConstant(7, 0, 2); int away = NearCondition(0x85);
            Patch(increment, position); Patch(tie, position); Patch(moreThanHalf, position); IntegerConstant(0, 8, 1, 64);
            Patch(exact, position); Patch(less, position); Patch(even, position); Patch(hugeZero, position); Patch(far, position);
            Patch(halfOrLess, position); Patch(trunc, position); Patch(away, position);
        }

        // Normalize a nonzero finite binary64 input without evaluating it.
        // Output R8=53-bit significand, EDX=unbiased top exponent.
        private void X87Decompose(int xmm)
        {
            FloatToInteger(8, xmm, 64); IntegerShift(4, 8, 64, 1); IntegerShift(5, 8, 64, 1);
            IntegerMov(2, 8); IntegerShift(5, 2, 64, 52); IntegerRR(0x85, 2, 2, 32);
            int tiny = NearCondition(0x84);
            IntegerImmediate(0, 0xfffffffffffff); IntegerRR(0x21, 8, 0);
            IntegerImmediate(0, 0x10000000000000); IntegerRR(0x09, 8, 0); IntegerConstant(5, 2, 1023);
            int done = NativeNearJump(); Patch(tiny, position);
            IntegerPrefix(64, 8, 1); Bytes(0x0f, 0xbd, 0xc8); // bsr rcx,r8 (nonzero is caller precondition)
            IntegerMov(2, 1, 32); IntegerConstant(5, 2, 1074);
            IntegerImmediate(0, 52, 32); IntegerRR(0x29, 0, 1, 32); IntegerMov(1, 0, 32); IntegerShift(4, 8, 64);
            Patch(done, position);
        }

        private void X87Extract()
        {
            FloatToInteger(8, 0, 64); IntegerFrameStore(208, 8);
            IntegerShift(4, 8, 64, 1); IntegerShift(5, 8, 64, 1);
            IntegerRR(0x85, 8, 8); int zero = NearCondition(0x84);
            IntegerImmediate(9, 0x7ff0000000000000); IntegerRR(0x39, 8, 9); int special = NearCondition(0x83);
            X87Decompose(0); IntegerImmediate(9, 0xfffffffffffff); IntegerRR(0x21, 8, 9);
            IntegerImmediate(9, 0x3ff0000000000000); IntegerRR(0x09, 8, 9);
            IntegerFrameLoad(208, 9); IntegerShift(5, 9, 64, 63); IntegerShift(4, 9, 64, 63); IntegerRR(0x09, 8, 9);
            FloatFromInteger(1, 8, 64); IntegerSignExtend(2, 32); Bytes(0xf2, 0x48, 0x0f, 0x2a, 0xc2);
            int normalDone = NativeNearJump();
            Patch(zero, position); X87Status(4); FloatMove(1, 0); X87Constant(0, 0xfff0000000000000); int zeroDone = NativeNearJump();
            Patch(special, position); IntegerRR(0x39, 8, 9); int infinity = NearCondition(0x84);
            FloatScalarCode(0x58, 0, 0, 64); FloatMove(1, 0); int nanDone = NativeNearJump();
            Patch(infinity, position); FloatMove(1, 0); X87Constant(0, 0x7ff0000000000000);
            Patch(normalDone, position); Patch(zeroDone, position); Patch(nanDone, position);
            // Detect an unmasked push overflow before overwriting old ST0.
            X87Tag(-1); IntegerConstant(7, 2, 3); int space = NearCondition(0x84);
            X87Status(0x241); Patch(space, position);
        }

        // Scale by adjusting the exact binary80 encoding of a binary64 value,
        // then narrow ONCE with guard/round/sticky. The bounded +/-4096 exponent
        // clamp exceeds every binary64 overflow/underflow threshold.
        private void X87Scale()
        {
            Bytes(0x66, 0x0f, 0x2e, 0xc9); int nan = NearCondition(0x8a);
            X87Constant(2, 0x40b0000000000000); // 4096
            Bytes(0x66, 0x0f, 0x2e, 0xca); int high = NearCondition(0x87);
            X87Constant(2, 0xc0b0000000000000);
            Bytes(0x66, 0x0f, 0x2e, 0xca); int low = NearCondition(0x82);
            Bytes(0xf2, 0x48, 0x0f, 0x2c, 0xc1); int converted = NativeNearJump();
            Patch(high, position); IntegerImmediate(0, 4096); int highDone = NativeNearJump();
            Patch(low, position); IntegerImmediate(0, unchecked((ulong)-4096L));
            Patch(converted, position); Patch(highDone, position); IntegerFrameStore(192, 0);
            FloatMxcsr(2, 356); // Truncating scale's exponent does not raise PE.
            FloatToInteger(8, 0, 64); IntegerShift(4, 8, 64, 1); IntegerShift(5, 8, 64, 1);
            IntegerRR(0x85, 8, 8); int zero = NearCondition(0x84);
            IntegerImmediate(9, 0x7ff0000000000000); IntegerRR(0x39, 8, 9); int special = NearCondition(0x83);
            X87Encode80(0); IntegerFrameLoad(192, 0); IntegerRR(0x01, 9, 0, 32); X87Decode80(); FloatMove(0, 1);
            int done = NativeNearJump();
            Patch(nan, position); Patch(special, position); FloatScalarCode(0x58, 0, 1, 64);
            Patch(zero, position); Patch(done, position);
        }

        // Exact f64 remainder by unsigned significand long division. A partial
        // step reduces exponent distance by32 when D>=64 and sets C2; otherwise
        // at most64 quotient bits are reduced. No libm/helper/host x87 required.
        private void X87Remainder(bool nearest)
        {
            FloatToInteger(8, 0, 64); IntegerFrameStore(184, 8);
            IntegerMov(9, 8); IntegerShift(5, 9, 64, 63); IntegerShift(4, 9, 64, 63); IntegerFrameStore(208, 9);
            FloatToInteger(9, 1, 64); IntegerFrameStore(192, 9);
            IntegerShift(4, 8, 64, 1); IntegerShift(5, 8, 64, 1); IntegerShift(4, 9, 64, 1); IntegerShift(5, 9, 64, 1);
            IntegerImmediate(0, 0x7ff0000000000000); IntegerRR(0x39, 8, 0); int xSpecial = NearCondition(0x83);
            IntegerRR(0x39, 9, 0); int yNan = NearCondition(0x87); int yInfinity = NearCondition(0x84);
            IntegerRR(0x85, 9, 9); int divisorZero = NearCondition(0x84);
            IntegerRR(0x85, 8, 8); int dividendZero = NearCondition(0x84);
            X87Decompose(0); FloatFromInteger(2, 8, 64); FloatFromInteger(3, 2, 64);
            X87Decompose(1); IntegerMov(9, 8); FloatFromInteger(4, 2, 64);
            FloatToInteger(8, 2, 64); FloatToInteger(1, 3, 64); IntegerRR(0x29, 1, 2, 32);
            IntegerRR(0x85, 1, 1, 32); int smaller = NearCondition(0x88);
            IntegerImmediate(0, 0, 32); IntegerImmediate(2, 0, 32); FloatFrameConstant(200, 0);
            IntegerConstant(7, 1, 64); int complete = NearCondition(0x82);
            IntegerMov(2, 1, 32); IntegerConstant(5, 2, 32); FloatFrameConstant(200, 1);
            Patch(complete, position);
            int loop = position;
            IntegerRR(0x39, 8, 9); int less = NearCondition(0x82);
            IntegerRR(0x29, 8, 9); IntegerConstant(1, 0, 1);
            Patch(less, position); IntegerRR(0x39, 1, 2, 32); int reduced = NearCondition(0x84);
            IntegerShift(4, 8, 64, 1); IntegerShift(4, 0, 32, 1); IntegerConstant(5, 1, 1);
            int again = NativeNearJump(); Patch(again, loop); Patch(reduced, position);
            FloatToInteger(2, 4, 64); FloatFrameLoad(9, 200); IntegerRR(0x85, 9, 9, 32); int full = NearCondition(0x84);
            IntegerRR(0x01, 2, 1, 32); X87Bits(648, 4, ~0x400u); X87Status(0x400); int partial = NativeNearJump();
            Patch(full, position);
            if (nearest)
            {
                // Reconstruct divisor from untouched XMM1 (decomposition only
                // clobbers GPRs; XMM2/3 now available as private integer slots).
                FloatFromInteger(2, 8, 64); FloatFromInteger(3, 0, 64); X87Decompose(1);
                IntegerMov(9, 8); FloatToInteger(8, 2, 64); FloatToInteger(0, 3, 64);
                IntegerMov(1, 8); IntegerShift(4, 1, 64, 1); IntegerRR(0x39, 1, 9);
                int above = NearCondition(0x87); int below = NearCondition(0x82);
                IntegerMov(1, 0, 32); IntegerConstant(4, 1, 1); int even = NearCondition(0x84);
                Patch(above, position); IntegerRR(0x29, 9, 8); IntegerMov(8, 9); IntegerConstant(0, 0, 1);
                X87FlipRemainderSign(); Patch(below, position); Patch(even, position);
            }
            int fullQuotient = NativeNearJump();
            Patch(smaller, position);
            if (nearest)
            {
                IntegerConstant(7, 1, uint.MaxValue); int muchSmaller = NearCondition(0x85);
                IntegerRR(0x39, 8, 9); int belowHalf = NearCondition(0x86);
                IntegerShift(4, 9, 64, 1); IntegerRR(0x29, 9, 8); IntegerMov(8, 9); IntegerConstant(5, 2, 1);
                IntegerImmediate(0, 1, 32); X87FlipRemainderSign(); int smallRounded = NativeNearJump();
                Patch(muchSmaller, position); Patch(belowHalf, position);
                // This jump is patched alongside other exact dividend returns.
                int original = NativeNearJump();
                Patch(smallRounded, position); Patch(fullQuotient, position); X87QuotientFlags();
                Patch(partial, position); X87PackRemainder(); int packed = NativeNearJump();
                Patch(original, position); Patch(yInfinity, position); Patch(dividendZero, position); X87Bits(648, 4, ~0x4700u);
                int originalDone = NativeNearJump();
                X87RemainderExceptional(xSpecial, yNan, divisorZero);
                Patch(packed, position); Patch(originalDone, position);
            }
            else
            {
                int original = NativeNearJump();
                Patch(fullQuotient, position); X87QuotientFlags(); Patch(partial, position); X87PackRemainder(); int packed = NativeNearJump();
                Patch(original, position); Patch(yInfinity, position); Patch(dividendZero, position); X87Bits(648, 4, ~0x4700u);
                int originalDone = NativeNearJump(); X87RemainderExceptional(xSpecial, yNan, divisorZero);
                Patch(packed, position); Patch(originalDone, position);
            }
        }
        private void X87FlipRemainderSign()
        {
            // Preserve RAX quotient, R8 remainder and EDX exponent.
            IntegerFrameLoad(208, 1); IntegerImmediate(9, 0x8000000000000000);
            IntegerRR(0x31, 1, 9); IntegerFrameStore(208, 1);
        }
        private void X87QuotientFlags()
        {
            // Quotient low3, including sign, map Q0->C1,Q1->C3,Q2->C0.
            IntegerFrameLoad(184, 9); IntegerFrameLoad(192, 1); IntegerRR(0x31, 9, 1);
            IntegerRR(0x85, 9, 9); int positive = NearCondition(0x89); IntegerUnary(3, 0, 32); Patch(positive, position);
            IntegerMov(9, 0, 32); IntegerMov(1, 0, 32); IntegerConstant(4, 0, 1); IntegerShift(4, 0, 32, 9);
            IntegerConstant(4, 9, 2); IntegerShift(4, 9, 32, 13); IntegerRR(0x09, 0, 9, 32);
            IntegerConstant(4, 1, 4); IntegerShift(4, 1, 32, 6); IntegerRR(0x09, 0, 1, 32);
            FloatFromInteger(2, 2, 64); X87LoadField(2, 648); IntegerConstant(4, 2, ~0x4700u);
            IntegerRR(0x09, 0, 2, 32); X87StoreField(0, 648); FloatToInteger(2, 2, 64);
        }
        private void X87PackRemainder()
        {
            IntegerRR(0x85, 8, 8); int zero = NearCondition(0x84);
            IntegerPrefix(64, 8, 1); Bytes(0x0f, 0xbd, 0xc8);
            IntegerImmediate(0, 52, 32); IntegerRR(0x29, 0, 1, 32); IntegerMov(1, 0, 32);
            IntegerShift(4, 8, 64); IntegerRR(0x29, 2, 1, 32);
            IntegerConstant(7, 2, unchecked((uint)-1022)); int subnormal = NearCondition(0x8c);
            IntegerImmediate(0, 0xfffffffffffff); IntegerRR(0x21, 8, 0);
            IntegerConstant(0, 2, 1023); IntegerShift(4, 2, 64, 52); IntegerRR(0x09, 8, 2); int normalDone = NativeNearJump();
            Patch(subnormal, position); IntegerImmediate(1, unchecked((uint)-1022), 32);
            IntegerRR(0x29, 1, 2, 32); IntegerShift(5, 8, 64);
            Patch(zero, position); Patch(normalDone, position); IntegerFrameLoad(208, 9); IntegerRR(0x09, 8, 9); FloatFromInteger(0, 8, 64);
        }
        private void X87RemainderExceptional(int xSpecial, int yNan, int divisorZero)
        {
            Patch(xSpecial, position); FloatToInteger(8, 0, 64); IntegerShift(4, 8, 64, 1); IntegerShift(5, 8, 64, 1);
            IntegerImmediate(9, 0x7ff0000000000000); IntegerRR(0x39, 8, 9); int xNan = NearCondition(0x87);
            // If either operand is a NaN, propagate/quiet it before considering
            // infinity dividend or zero divisor invalid conditions.
            FloatToInteger(9, 1, 64); IntegerShift(4, 9, 64, 1); IntegerShift(5, 9, 64, 1);
            IntegerImmediate(0, 0x7ff0000000000000); IntegerRR(0x39, 9, 0); int alsoYnan = NearCondition(0x87);
            int invalidX = NativeNearJump();
            Patch(yNan, position); Patch(xNan, position); Patch(alsoYnan, position); FloatScalarCode(0x58, 0, 1, 64); int nanDone = NativeNearJump();
            Patch(invalidX, position); Patch(divisorZero, position); X87Status(1); X87Constant(0, 0xfff8000000000000);
            Patch(nanDone, position);
        }
    }
}
