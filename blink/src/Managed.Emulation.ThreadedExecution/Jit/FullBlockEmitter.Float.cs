using System.Runtime.InteropServices;
using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

// Independently authored translation-time lowering. Every admitted operation
// computes in generated AMD64/SSE2 instructions. XMM0..5 and volatile GPRs only;
// no CALL, host stack, x87/MMX, optional SIMD ISA, or managed semantic helper.
internal static partial class FullBlockEmitter
{
    internal enum FullFloatOperation : ushort
    {
        Add = 1, Subtract, Multiply, Divide, Minimum, Maximum, SquareRoot,
        CompareMask, CompareOrderedFlags, CompareUnorderedFlags,
        IntegerToScalar, ScalarToInteger, ScalarToIntegerTruncate,
        SingleToDouble, DoubleToSingle, DwordToSingle, DwordToDouble,
        SingleToDword, SingleToDwordTruncate, DoubleToDword, DoubleToDwordTruncate
    }

    // Width128 denotes the XMM/GPR typed class, never a scalar memory width.
    // SourceWidth is the EXACT checked memory width. Extra is CMP imm8 only.
    internal const uint FloatDouble = 1u << 16;
    internal const uint FloatScalar = 1u << 17;
    internal const uint FloatWideInteger = 1u << 18;
    internal const uint FloatMmxConversions = 1u << 19;
    internal const int FloatRestoreHostEnvironmentBytes = 29;
    internal static bool CanInlineFloat(JitInstruction i) => i.Operation == JitOperation.VectorFloat;
    internal static FullFloatOperation FloatOperation(JitInstruction i) => (FullFloatOperation)(i.Options & 0xffff);

    internal static bool ValidateFloat(JitInstruction i)
    {
        if (!CanInlineFloat(i)) return false;
        var op = FloatOperation(i);
        bool scalar = (i.Options & FloatScalar) != 0, wide = (i.Options & FloatWideInteger) != 0;
        bool dbl = (i.Options & FloatDouble) != 0;
        bool mmx = (i.Options & FloatMmxConversions) != 0;
        if (i.Width != 128 || i.Length is < 1 or > 15 || i.Target != 0 || i.Condition != 0 ||
            (i.Options & ~(0xffffu | FloatDouble | FloatScalar | FloatWideInteger | FloatMmxConversions)) != 0 ||
            op is < FullFloatOperation.Add or > FullFloatOperation.DoubleToDwordTruncate ||
            !VectorRegister(i.Destination) || !(VectorRegister(i.Source) || VectorMemory(i.Source)))
            throw new ArgumentException("Native float operation/operands");
        bool scalarInteger = op is FullFloatOperation.IntegerToScalar or FullFloatOperation.ScalarToInteger or FullFloatOperation.ScalarToIntegerTruncate;
        bool compareFlags = op is FullFloatOperation.CompareOrderedFlags or FullFloatOperation.CompareUnorderedFlags;
        bool mixedFloat = op is FullFloatOperation.SingleToDouble or FullFloatOperation.DoubleToSingle;
        bool packedConvert = op >= FullFloatOperation.DwordToSingle;
        bool mmxInput = op is FullFloatOperation.DwordToSingle or FullFloatOperation.DwordToDouble;
        if (mmx && (!packedConvert || scalar || dbl || wide ||
            (mmxInput ? !i.Source.IsMemory && i.Source.Register >= 8 : i.Destination.Register >= 8)))
            throw new ArgumentException("Native MMX float conversion class");
        if (wide && !scalarInteger || (scalarInteger || compareFlags) && !scalar || packedConvert && scalar ||
            (mixedFloat || packedConvert) && dbl)
            throw new ArgumentException("Native float class metadata");
        if (op == FullFloatOperation.CompareMask ? !VectorImmediate(i.Extra) : i.Extra != default)
            throw new ArgumentException("Native float immediate");
        int expected = op switch
        {
            FullFloatOperation.IntegerToScalar => wide ? 64 : 32,
            FullFloatOperation.SingleToDouble => scalar ? 32 : 64,
            FullFloatOperation.DoubleToSingle => scalar ? 64 : 128,
            FullFloatOperation.DwordToDouble => 64,
            FullFloatOperation.DwordToSingle or FullFloatOperation.SingleToDword or
                FullFloatOperation.SingleToDwordTruncate or FullFloatOperation.DoubleToDword or
                FullFloatOperation.DoubleToDwordTruncate => 128,
            _ => scalar ? (dbl ? 64 : 32) : 128
        };
        if (mmx) expected = op is FullFloatOperation.DoubleToDword or FullFloatOperation.DoubleToDwordTruncate ? 128 : 64;
        if (i.SourceWidth != expected) throw new ArgumentException("Native float memory width");
        if (i.Source.IsMemory)
        {
            var a = i.Address;
            if (a.Base is < -1 or > 15 || a.Index is < -1 or > 15 || a.Scale is < 0 or > 3 ||
                a.AddressWidth is not (32 or 64) || a.Segment is < -1 or > 5 ||
                a.RipRelative && (a.Base != -1 || a.Index != -1))
                throw new ArgumentException("Native float address");
        }
        else if (i.Address != default) throw new ArgumentException("Native float unused address");
        if (!FloatLayout.Supported || mmx && !FloatMmxLayout.Supported) throw new PlatformNotSupportedException("Translated float/frame layout differs");
        return true;
    }

    private static class FloatLayout { internal static readonly bool Supported = HasExpectedFloatLayout(); }
    private static class FloatMmxLayout
    {
        internal static readonly bool Supported =
            Marshal.OffsetOf<Blink.Machine>(nameof(Blink.Machine.fpu)).ToInt32() == 584 &&
            Marshal.OffsetOf<Blink.MachineFpu>(nameof(Blink.MachineFpu.sw)).ToInt32() == 64 &&
            Marshal.OffsetOf<Blink.MachineFpu>(nameof(Blink.MachineFpu.cw)).ToInt32() == 88;
    }
    internal static bool HasExpectedFloatLayout() => HasExpectedVectorLayout() &&
        Marshal.OffsetOf<Blink.Machine>(nameof(Blink.Machine.mxcsr)).ToInt32() == 688 &&
        Marshal.OffsetOf<FullBlockFrame>(nameof(FullBlockFrame.HostMxcsrSaved)).ToInt32() == 352 &&
        Marshal.OffsetOf<FullBlockFrame>(nameof(FullBlockFrame.HostMxcsrWork)).ToInt32() == 356 &&
        Marshal.OffsetOf<FullBlockFrame>(nameof(FullBlockFrame.HostMxcsrDirty)).ToInt32() == 360 &&
        Marshal.OffsetOf<FullBlockFrame>(nameof(FullBlockFrame.HostMxcsrObserved)).ToInt32() == 364;

    private ref partial struct Writer
    {
        // Parent MUST put this on EVERY typed native RET after frame ABI/Size
        // authentication, including shared exits, helper/mapping/budget exits,
        // and stale linked bodies. Exactly29 bytes, no GPR/XMM clobber. A frame
        // with Dirty=0 is required on a fresh managed-to-native invocation.
        private void FloatRestoreHostEnvironment()
        {
            Bytes(0x41, 0x83, 0xba); U32(360); Bytes(0); // cmp [r10+360],0
            Bytes(0x74, 19);
            FloatMxcsr(2, 352); // ldmxcsr original host environment
            FloatFrameConstant(360, 0);
        }

        private void FloatMxcsr(int group, uint offset)
        { Bytes(0x41, 0x0f, 0xae, (byte)(0x82 | group << 3)); U32(offset); }
        private void FloatFrameConstant(uint offset, uint value)
        { Bytes(0x41, 0xc7, 0x82); U32(offset); U32(value); }
        private void FloatFrameBits(int group, uint offset, uint value)
        { Bytes(0x41, 0x81, (byte)(0x82 | group << 3)); U32(offset); U32(value); }
        private void FloatFrameLoad(int reg, uint offset)
        { IntegerPrefix(32, 10, reg); Bytes(0x8b); Memory(reg & 7, 2, (int)offset); }
        private void FloatFrameStore(int reg, uint offset)
        { IntegerPrefix(32, 10, reg); Bytes(0x89); Memory(reg & 7, 2, (int)offset); }
        private void FloatGuestControl(int reg = 0)
        { IntegerPrefix(32, 11, reg); Bytes(0x8b); Memory(reg & 7, 3, 688); }
        private void FloatBeginEnvironment()
        {
            Bytes(0x41, 0x83, 0xba); U32(360); Bytes(0);
            int saved = NearCondition(0x85);
            FloatMxcsr(3, 352); // first FP saves original host sticky/control
            FloatFrameConstant(360, 1);
            Patch(saved, position);
            FloatGuestControl();
            IntegerConstant(4, 0, 0x6000); // ONLY guest RC, never reserved/DAZ/FTZ bits
            IntegerConstant(1, 0, 0x1f80); // all six HOST masks, zero sticky
            FloatFrameStore(0, 356);
            FloatFrameConstant(200, 0); // instruction pre-computation status
            FloatFrameConstant(204, 0); // instruction post-computation status
        }

        private void FloatMove(int dst, int src) => Bytes(0x66, 0x0f, 0x6f, (byte)(0xc0 | dst << 3 | src));
        private void FloatExtract(int dst, int src, int byteOffset)
        {
            FloatMove(dst, src);
            if (byteOffset != 0) Bytes(0x66, 0x0f, 0x73, (byte)(0xd8 | dst), (byte)byteOffset);
        }
        private void FloatToInteger(int reg, int xmm, int width)
        {
            Bytes(0x66); IntegerPrefix(width, reg, xmm);
            Bytes(0x0f, 0x7e, (byte)(0xc0 | xmm << 3 | (reg & 7)));
        }
        private void FloatFromInteger(int xmm, int reg, int width)
        {
            Bytes(0x66); IntegerPrefix(width, reg, xmm);
            Bytes(0x0f, 0x6e, (byte)(0xc0 | xmm << 3 | (reg & 7)));
        }
        private void FloatScalarCode(byte opcode, int dst, int src, int width)
            => Bytes(width == 64 ? (byte)0xf2 : (byte)0xf3, 0x0f, opcode, (byte)(0xc0 | dst << 3 | src));

        // DAZ is implemented in generated integer instructions. Thus even a
        // baseline host whose MXCSR_MASK excludes DAZ receives no illegal bit.
        // Zero/sign tests never evaluate a NaN in host floating arithmetic.
        private void FloatDaz(int xmm, int width)
        {
            FloatGuestControl(); Bytes(0xa8, 0x40); int off = NearCondition(0x84);
            FloatToInteger(0, xmm, width); IntegerMov(2, 0);
            IntegerShift(5, 2, width, width == 64 ? 52 : 23);
            IntegerConstant(4, 2, width == 64 ? 2047u : 255u);
            IntegerRR(0x85, 2, 2, 32); int normal = NearCondition(0x85);
            IntegerShift(5, 0, width, width - 1); IntegerShift(4, 0, width, width - 1);
            FloatFromInteger(xmm, 0, width);
            Patch(normal, position); Patch(off, position);
        }

        private bool TryFloatOperation(in JitInstruction i)
        {
            if (!CanInlineFloat(i)) return false;
            var op = FloatOperation(i);
            bool scalar = (i.Options & FloatScalar) != 0, dbl = (i.Options & FloatDouble) != 0;
            bool wide = (i.Options & FloatWideInteger) != 0;
            bool mmx = (i.Options & FloatMmxConversions) != 0;
            bool toInteger = op is FullFloatOperation.ScalarToInteger or FullFloatOperation.ScalarToIntegerTruncate;
            bool flags = op is FullFloatOperation.CompareOrderedFlags or FullFloatOperation.CompareUnorderedFlags;
            bool integerInput = op is FullFloatOperation.IntegerToScalar or FullFloatOperation.DwordToSingle or FullFloatOperation.DwordToDouble;
            bool binary = op is >= FullFloatOperation.Add and <= FullFloatOperation.Maximum || op == FullFloatOperation.CompareMask || flags;
            int inputWidth = op switch
            {
                FullFloatOperation.SingleToDouble or FullFloatOperation.DwordToSingle or
                    FullFloatOperation.DwordToDouble or FullFloatOperation.SingleToDword or
                    FullFloatOperation.SingleToDwordTruncate => 32,
                FullFloatOperation.DoubleToSingle or FullFloatOperation.DoubleToDword or FullFloatOperation.DoubleToDwordTruncate => 64,
                FullFloatOperation.IntegerToScalar => wide ? 64 : 32,
                _ => dbl ? 64 : 32
            };
            int outputWidth = op switch
            {
                FullFloatOperation.SingleToDouble or FullFloatOperation.DwordToDouble => 64,
                FullFloatOperation.DoubleToSingle or FullFloatOperation.DwordToSingle or FullFloatOperation.SingleToDword or
                    FullFloatOperation.SingleToDwordTruncate or FullFloatOperation.DoubleToDword or FullFloatOperation.DoubleToDwordTruncate => 32,
                _ => toInteger ? (wide ? 64 : 32) : dbl ? 64 : 32
            };
            int lanes = scalar ? 1 : op is FullFloatOperation.SingleToDouble or FullFloatOperation.DwordToDouble or
                FullFloatOperation.DoubleToSingle or FullFloatOperation.DoubleToDword or FullFloatOperation.DoubleToDwordTruncate ? 2 : 128 / inputWidth;
            if (mmx) { lanes = 2; FloatMmxWaitingException(i); }

            // ALL memory mapping/prechecks and operand loads precede Dirty and
            // FP effects. Later memory misses restore any PREVIOUS FP state via
            // the common exit cleanup. No guest destination is touched here.
            if (i.Source.IsMemory)
            {
                if (mmx)
                {
                    ScalarMiscCheckCanonical(i,i.SourceWidth);
                    if (i.SourceWidth == 128) VectorMoveCheckAlignment(i);
                }
                NativeMemoryPrepare(i, FullMemoryAccess.Read, i.SourceWidth); VectorMemoryLoad(i.SourceWidth);
            }
            else if (op == FullFloatOperation.IntegerToScalar)
            { IntegerLoad(i.Source, inputWidth, 0); FloatFromInteger(1, 0, inputWidth); }
            else if (mmx && integerInput) MmxLoad(1, i.Source.Register);
            else VectorGuestLoad(1, i.Source.Register);
            // A PI destination is not an XMM input. Its old value is unused.
            if (mmx && !integerInput) Bytes(0x66, 0x0f, 0xef, 0xc0);
            else VectorGuestLoad(0, i.Destination.Register);
            if (mmx && op == FullFloatOperation.DwordToSingle)
            {
                FloatMove(2,0); Bytes(0x66,0x0f,0x73,0xda,8,0x66,0x0f,0x73,0xfa,8);
            }
            else if (scalar && !toInteger && !flags)
            {
                FloatMove(2, 0);
                Bytes(0x66, 0x0f, 0x73, 0xda, (byte)(outputWidth / 8));
                Bytes(0x66, 0x0f, 0x73, 0xfa, (byte)(outputWidth / 8));
            }
            else Bytes(0x66, 0x0f, 0xef, 0xd2);
            FloatBeginEnvironment();
            for (int lane = 0; lane < lanes; ++lane)
            {
                FloatExtract(3, 0, lane * inputWidth / 8);
                FloatExtract(4, 1, lane * inputWidth / 8);
                if (!integerInput)
                { if (binary) FloatDaz(3, inputWidth); FloatDaz(4, inputWidth); }
                FloatMxcsr(2, 356);
                switch (op)
                {
                    case FullFloatOperation.Add: FloatScalarCode(0x58, 3, 4, inputWidth); break;
                    case FullFloatOperation.Subtract: FloatScalarCode(0x5c, 3, 4, inputWidth); break;
                    case FullFloatOperation.Multiply: FloatScalarCode(0x59, 3, 4, inputWidth); break;
                    case FullFloatOperation.Divide: FloatScalarCode(0x5e, 3, 4, inputWidth); break;
                    case FullFloatOperation.Minimum: FloatScalarCode(0x5d, 3, 4, inputWidth); break;
                    case FullFloatOperation.Maximum: FloatScalarCode(0x5f, 3, 4, inputWidth); break;
                    case FullFloatOperation.SquareRoot: FloatScalarCode(0x51, 3, 4, inputWidth); break;
                    case FullFloatOperation.CompareMask:
                        FloatScalarCode(0xc2, 3, 4, inputWidth); Bytes((byte)(i.Extra.Immediate & 7)); break;
                    case FullFloatOperation.CompareOrderedFlags:
                    case FullFloatOperation.CompareUnorderedFlags:
                        if (inputWidth == 64) Bytes(0x66);
                        Bytes(0x0f, op == FullFloatOperation.CompareOrderedFlags ? (byte)0x2f : (byte)0x2e, 0xdc);
                        FloatCaptureCompareFlags(); break;
                    case FullFloatOperation.IntegerToScalar:
                    case FullFloatOperation.DwordToSingle:
                    case FullFloatOperation.DwordToDouble:
                        FloatToInteger(0, 4, inputWidth);
                        Bytes(outputWidth == 64 ? (byte)0xf2 : (byte)0xf3);
                        if (inputWidth == 64) Bytes(0x48);
                        Bytes(0x0f, 0x2a, 0xd8); break;
                    case FullFloatOperation.ScalarToInteger:
                    case FullFloatOperation.ScalarToIntegerTruncate:
                    case FullFloatOperation.SingleToDword:
                    case FullFloatOperation.SingleToDwordTruncate:
                    case FullFloatOperation.DoubleToDword:
                    case FullFloatOperation.DoubleToDwordTruncate:
                        Bytes(inputWidth == 64 ? (byte)0xf2 : (byte)0xf3);
                        if (outputWidth == 64) Bytes(0x48);
                        bool truncate = op is FullFloatOperation.ScalarToIntegerTruncate or FullFloatOperation.SingleToDwordTruncate or FullFloatOperation.DoubleToDwordTruncate;
                        Bytes(0x0f, truncate ? (byte)0x2c : (byte)0x2d, 0xc4);
                        FloatFromInteger(3, 0, outputWidth); break;
                    default: // CVTSS2SD/CVTSD2SS; packed forms scalarized.
                        FloatScalarCode(0x5a, 3, 4, inputWidth); break;
                }
                FloatMxcsr(3, 208); // this lane only, clear host sticky next lane
                if (op is FullFloatOperation.Multiply or FullFloatOperation.Divide or FullFloatOperation.DoubleToSingle)
                    FloatRangeReplay(i, lane, inputWidth, outputWidth);
                else if (op is FullFloatOperation.Add or FullFloatOperation.Subtract)
                {
                    FloatAdditiveTiny(outputWidth);
                    FloatAdditiveOverflow(i, lane, inputWidth);
                }
                FloatAccumulateStatus();
                if (!flags)
                {
                    FloatToInteger(0, 3, outputWidth); FloatFromInteger(5, 0, outputWidth);
                    if (lane != 0) Bytes(0x66, 0x0f, 0x73, 0xfd, (byte)(lane * outputWidth / 8));
                    Bytes(0x66, 0x0f, 0xeb, 0xd5);
                }
            }
            FloatFinishStatus(); // may exit19 with sticky set, BEFORE destination
            if (flags)
            { FloatFrameLoad(0, 184); IntegerMergeFlags(0xff0008d1); }
            else if (toInteger)
            { FloatToInteger(8, 2, outputWidth); Store(i.Destination, outputWidth); }
            else if (mmx && !integerInput) MmxStore(2, i.Destination.Register);
            else VectorGuestStore(2, i.Destination.Register);
            if (mmx) MmxTransition(); // only after FloatFinishStatus permits completion
            if (i.Source.IsMemory) NativeMemoryCount();
            return true;
        }

        private void FloatMmxWaitingException(JitInstruction i)
        {
            Bytes(0x41,0x8b,0x83); U32(672); IntegerUnary(2,0,32);
            Bytes(0x41,0x23,0x83); U32(648); IntegerConstant(4,0,63);
            int ready=NearCondition(0x84);
            Bytes(0x41,0xc7,0x82); U32(168); U32(16);
            Bytes(0x41,0xc7,0x82); U32(172); U32(0);
            Bytes(0x49,0xb8); U64(i.Ip); IntegerFrameStore(176,8);
            JumpExit(FullBlockExit.GuestFault); Patch(ready,position);
        }

        private void FloatCaptureCompareFlags()
        {
            Bytes(0x0f, 0x92, 0xc0, 0x0f, 0x94, 0xc1, 0x0f, 0x9a, 0xc2);
            Bytes(0x0f, 0xb6, 0xc0, 0x0f, 0xb6, 0xc9, 0x0f, 0xb6, 0xd2);
            IntegerShift(4, 1, 32, 6); IntegerRR(0x09, 0, 1, 32);
            IntegerConstant(6, 2, 1); IntegerShift(4, 2, 32, 24);
            IntegerRR(0x09, 0, 2, 32); FloatFrameStore(0, 184);
        }

        private void FloatAccumulateStatus()
        {
            FloatFrameLoad(0, 208); IntegerMov(1, 0, 32);
            IntegerConstant(4, 0, 7); IntegerConstant(4, 1, 0x38);
            Bytes(0x41, 0x09, 0x82); U32(200);
            Bytes(0x41, 0x09, 0x8a); U32(204);
        }

        private void FloatFinishStatus()
        {
            FloatFrameLoad(0, 200);
            FloatGuestControl(1); IntegerShift(5, 1, 32, 7); IntegerUnary(2, 1, 32);
            IntegerRR(0x85, 0, 1, 32); int preFault = NearCondition(0x85);
            FloatFrameLoad(2, 204); IntegerRR(0x09, 0, 2, 32);
            Patch(preFault, position); // no post flags if any pre exception unmasks
            FloatFrameStore(0, 364);
            Bytes(0x41, 0x09, 0x83); U32(688); // guest sticky before #XM
            IntegerRR(0x85, 0, 1, 32); int success = NearCondition(0x84);
            FloatFrameConstant(168, 19); // CPU #XM, NEVER Blink internal -9
            FloatFrameConstant(172, 0);
            Bytes(0x49, 0x8b, 0x03, 0x49, 0x89, 0x82); U32(176);
            JumpExit(FullBlockExit.GuestFault);
            Patch(success, position);
        }

        // Addition/subtraction tiny outputs are exact multiples of min-subnormal.
        // No second rounding is needed to detect exact unmasked underflow.
        private void FloatAdditiveTiny(int width)
        {
            FloatToInteger(0, 3, width); IntegerShift(4, 0, width, 1); IntegerShift(5, 0, width, 1);
            IntegerRR(0x85, 0, 0, width); int zero = NearCondition(0x84);
            IntegerShift(5, 0, width, width == 64 ? 52 : 23);
            IntegerRR(0x85, 0, 0, width); int normal = NearCondition(0x85);
            FloatTinyResponse(width);
            Patch(normal, position); Patch(zero, position);
        }

        // Called only for a mathematically tiny NONZERO result rounded with
        // unbounded exponent. FTZ is software; never install host bit15.
        private void FloatTinyResponse(int width)
        {
            FloatGuestControl(); IntegerConstant(4, 0, 0x800);
            IntegerRR(0x85, 0, 0, 32); int masked = NearCondition(0x85);
            FloatFrameBits(1, 208, 0x10); int done = NativeNearJump();
            Patch(masked, position);
            FloatGuestControl(); IntegerConstant(4, 0, 0x8000);
            IntegerRR(0x85, 0, 0, 32); int gradual = NearCondition(0x84);
            FloatToInteger(0, 3, width);
            IntegerShift(5, 0, width, width - 1); IntegerShift(4, 0, width, width - 1);
            FloatFromInteger(3, 0, width); FloatFrameBits(1, 208, 0x30);
            Patch(gradual, position); Patch(done, position);
        }

        private void FloatAdditiveOverflow(JitInstruction i, int lane, int width)
            => FloatAdditiveOverflow(FloatOperation(i) != FullFloatOperation.Add,0,lane,1,lane,width);

        private void FloatAdditiveOverflow(bool subtract,int leftVector,int leftLane,int rightVector,int rightLane,int width)
        {
            FloatFrameLoad(0, 208); Bytes(0xa8, 8); int noOverflow = NearCondition(0x84);
            FloatGuestControl(); IntegerConstant(4, 0, 0x400);
            IntegerRR(0x85, 0, 0, 32); int masked = NearCondition(0x85);
            FloatExtract(5,leftVector,leftLane*width/8); FloatExtract(4,rightVector,rightLane*width/8);
            FloatDaz(5, width); FloatDaz(4, width);
            // Scaling both operands down by an exact power of two avoids host
            // overflow. If the smaller operand itself loses bits during this
            // scaling, its PE is retained: adding it to an overflowing large
            // operand was necessarily inexact at the unbounded precision.
            IntegerImmediate(0, width == 64 ? 0x3c90000000000000UL : 0x2f800000UL, width);
            Bytes(0x49, 0x89, 0x82); U32(184);
            FloatMxcsr(2, 356);
            Bytes(width == 64 ? (byte)0xf2 : (byte)0xf3, 0x41, 0x0f, 0x59, 0xaa); U32(184);
            Bytes(width == 64 ? (byte)0xf2 : (byte)0xf3, 0x41, 0x0f, 0x59, 0xa2); U32(184);
            FloatScalarCode(subtract?(byte)0x5c:(byte)0x58,5,4,width);
            FloatMxcsr(3, 364);
            FloatFrameBits(4, 208, 0xffffffdf);
            FloatFrameLoad(0, 364); IntegerConstant(4, 0, 0x20);
            Bytes(0x41, 0x09, 0x82); U32(208);
            Patch(masked, position); Patch(noOverflow, position);
        }

        // Normalize a finite nonzero IEEE operand in xmm to signed [1,2),
        // returning its unbiased exponent in R8D/R9D. Integer bit operations
        // are exact even for the smallest subnormal. Clobbers RAX/RCX/RDX only.
        private void FloatNormalize(int xmm, int width, int exponent)
        {
            int fraction = width == 64 ? 52 : 23, bias = width == 64 ? 1023 : 127;
            FloatToInteger(0, xmm, width); IntegerMov(exponent, 0);
            IntegerShift(5, exponent, width, fraction); IntegerConstant(4, exponent, (uint)(2 * bias + 1));
            IntegerRR(0x85, exponent, exponent, 32); int subnormal = NearCondition(0x84);
            IntegerConstant(5, exponent, (uint)bias); int ready = NativeNearJump();
            Patch(subnormal, position);
            IntegerMov(2, 0); IntegerShift(4, 2, width, 1); IntegerShift(5, 2, width, 1);
            IntegerPrefix(width, 2, 2); Bytes(0x0f, 0xbd, 0xd2); // bsr rdx,rdx, known nonzero
            IntegerMov(exponent, 2, 32); IntegerConstant(5, exponent, (uint)(bias + fraction - 1));
            IntegerImmediate(1, (uint)fraction, 32); IntegerRR(0x29, 1, 2, 32);
            IntegerShift(4, 0, width);
            FloatToInteger(2, xmm, width); IntegerShift(5, 2, width, width - 1); IntegerShift(4, 2, width, width - 1);
            IntegerRR(0x09, 0, 2, width);
            Patch(ready, position);
            IntegerImmediate(2, (1UL << (width - 1)) | ((1UL << fraction) - 1), width);
            IntegerRR(0x21, 0, 2, width);
            IntegerImmediate(2, (ulong)bias << fraction, width); IntegerRR(0x09, 0, 2, width);
            FloatFromInteger(xmm, 0, width);
        }

        private int FloatNotFiniteNonzero(int xmm, int width)
        {
            FloatToInteger(0, xmm, width); IntegerShift(4, 0, width, 1); IntegerShift(5, 0, width, 1);
            IntegerRR(0x85, 0, 0, width); int zero = NearCondition(0x84);
            IntegerShift(5, 0, width, width == 64 ? 52 : 23);
            IntegerConstant(7, 0, width == 64 ? 2047u : 255u);
            // Both forward branches are patched by the caller. Chain the zero
            // case through a local skip so its one public fixup is sufficient.
            int finite = NearCondition(0x85); Patch(zero, position);
            int skip = NativeNearJump(); Patch(finite, position); return skip;
        }

        private void FloatRangeReplay(JitInstruction i, int lane, int inputWidth, int outputWidth)
        {
            var op = FloatOperation(i);
            FloatExtract(4, 1, lane * inputWidth / 8); FloatDaz(4, inputWidth);
            int sourceSpecial = FloatNotFiniteNonzero(4, inputWidth);
            int leftSpecial = -1;
            if (op is FullFloatOperation.Multiply or FullFloatOperation.Divide)
            {
                FloatExtract(5, 0, lane * inputWidth / 8); FloatDaz(5, inputWidth);
                leftSpecial = FloatNotFiniteNonzero(5, inputWidth);
                FloatNormalize(5, inputWidth, 8); FloatNormalize(4, inputWidth, 9);
                IntegerRR(op == FullFloatOperation.Multiply ? (byte)0x01 : (byte)0x29, 8, 9, 32);
                FloatMxcsr(2, 356);
                FloatScalarCode(op == FullFloatOperation.Multiply ? (byte)0x59 : (byte)0x5e, 5, 4, inputWidth);
            }
            else
            {
                FloatNormalize(4, inputWidth, 8); FloatMxcsr(2, 356);
                FloatScalarCode(0x5a, 5, 4, 64);
            }
            FloatMxcsr(3, 364); // intrinsic precision of unbounded-exponent result
            FloatToInteger(0, 5, outputWidth); IntegerShift(5, 0, outputWidth, outputWidth == 64 ? 52 : 23);
            IntegerConstant(4, 0, outputWidth == 64 ? 2047u : 255u);
            IntegerConstant(5, 0, outputWidth == 64 ? 1023u : 127u);
            IntegerRR(0x01, 8, 0, 32); // full result exponent with rounding carry
            IntegerConstant(7, 8, unchecked((uint)(outputWidth == 64 ? -1022 : -126)));
            int notTiny = NearCondition(0x8d); // signed >= min-normal exponent
            FloatTinyResponse(outputWidth);
            Patch(notTiny, position);
            // Masked O/U implies additional range rounding and forced PE.
            // Unmasked O/U instead reports precision at unbounded exponent.
            FloatGuestControl(); IntegerShift(5, 0, 32, 7); IntegerUnary(2, 0, 32);
            FloatFrameLoad(1, 208); IntegerRR(0x21, 0, 1, 32); IntegerConstant(4, 0, 0x18);
            IntegerRR(0x85, 0, 0, 32); int maskedRange = NearCondition(0x84);
            FloatFrameBits(4, 208, 0xffffffdf);
            FloatFrameLoad(0, 364); IntegerConstant(4, 0, 0x20);
            Bytes(0x41, 0x09, 0x82); U32(208);
            Patch(maskedRange, position);
            if (leftSpecial >= 0) Patch(leftSpecial, position);
            Patch(sourceSpecial, position);
        }
    }
}
