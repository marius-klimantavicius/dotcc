using System;
using QX = Managed.Emulation.Execution.FullCanonicalX87ControlOperation;

namespace Managed.Emulation.Execution;

internal static partial class FullBlockEmitter
{
    internal static bool CanInlineCanonicalX87Control(JitInstruction i) =>
        CanInlineCanonicalX87Stack(i) && (i.Options & 0xffff) != (uint)FullCanonicalX87StackOperation.Fwait ||
        i.Operation == JitOperation.X87 && ((i.Options & 0xffff) is >= 0x9001 and <= 0x9023);

    internal static bool ValidateCanonicalX87Control(JitInstruction i)
    {
        if (CanInlineCanonicalX87Stack(i) && (i.Options & 0xffff) != (uint)FullCanonicalX87StackOperation.Fwait)
            return ValidateCanonicalX87Stack(i);
        if (i.Operation != JitOperation.X87 || (i.Options & 0xffff) is < 0x9001 or > 0x9023) return false;
        var op = (QX)(i.Options & 0xffff); int n = i.Source.Register;
        if (op >= QX.Fldcw) return ValidateCanonicalX87ControlState(i,op);
        bool reduction = op is QX.Fscale or QX.Fprem or QX.Fprem1;
        bool unary = op >= QX.Ftst && !reduction, pair = op is QX.Fcompp or QX.Fucompp || reduction;
        uint fop = op switch
        {
            QX.Fcom => (uint)(0x0d0+n), QX.Fcomp => (uint)(0x0d8+n), QX.Fcompp => 0x6d9,
            QX.Fucom => (uint)(0x5e0+n), QX.Fucomp => (uint)(0x5e8+n), QX.Fucompp => 0x2e9,
            QX.Fcomi => (uint)(0x3f0+n), QX.Fcomip => (uint)(0x7f0+n),
            QX.Fucomi => (uint)(0x3e8+n), QX.Fucomip => (uint)(0x7e8+n),
            QX.Fcmovb => (uint)(0x2c0+n), QX.Fcmove => (uint)(0x2c8+n),
            QX.Fcmovbe => (uint)(0x2d0+n), QX.Fcmovu => (uint)(0x2d8+n),
            QX.Fcmovnb => (uint)(0x3c0+n), QX.Fcmovne => (uint)(0x3c8+n),
            QX.Fcmovnbe => (uint)(0x3d0+n), QX.Fcmovnu => (uint)(0x3d8+n),
            QX.Ftst => 0x1e4, QX.Fxam => 0x1e5, QX.Frndint => 0x1fc,
            QX.Fsqrt => 0x1fa, QX.Fscale => 0x1fd, QX.Fprem => 0x1f8, QX.Fprem1 => 0x1f5, _ => 0x1f4
        };
        if (i.Options != (uint)op || i.Length is < 2 or > 15 || i.Width != 80 ||
            i.Destination != default || i.Source != new JitOperand(n,0) || (uint)n > 7 ||
            unary && n != 0 || pair && n != 1 || i.Target != 0 || i.Condition != 0 ||
            i.SourceWidth != 0 || i.Address != default || i.Extra != new JitOperand(0,fop,IsImmediate:true))
            throw new ArgumentException("Invalid canonical raw80 comparison/control/transform IR");
        if (!CanonicalX87StackLayout.Supported)
            throw new PlatformNotSupportedException("Canonical x87 Machine/file/frame layout differs");
        return true;
    }

    internal static bool CanonicalX87ControlNeedsKernel(JitInstruction i)
    {
        if (!ValidateCanonicalX87Control(i)) return false;
        if (CanInlineCanonicalX87Stack(i)) return false;
        var op = (QX)(i.Options & 0xffff);
        return op <= QX.Fucomip || op is QX.Ftst or QX.Frndint or QX.Fsqrt or QX.Fxtract or QX.Fscale or QX.Fprem or QX.Fprem1;
    }
    internal static bool CanonicalX87ControlRequiresArithmeticPrecision(JitInstruction i) =>
        ValidateCanonicalX87Control(i) && (QX)i.Options == QX.Fsqrt;

    internal static FullX87KernelKey CanonicalX87ControlKernel(JitInstruction i)
    {
        if (!CanonicalX87ControlNeedsKernel(i)) throw new ArgumentException("Raw control has no kernel");
        return (QX)i.Options switch
        {
            QX.Fcom or QX.Ftst => FullX87KernelKey.Fcom, QX.Fcomp => FullX87KernelKey.Fcomp,
            QX.Fcompp => FullX87KernelKey.Fcompp, QX.Fucom => FullX87KernelKey.Fucom,
            QX.Fucomp => FullX87KernelKey.Fucomp, QX.Fucompp => FullX87KernelKey.Fucompp,
            QX.Fcomi => FullX87KernelKey.Fcomi, QX.Fcomip => FullX87KernelKey.Fcomip,
            QX.Fucomi => FullX87KernelKey.Fucomi, QX.Fucomip => FullX87KernelKey.Fucomip,
            QX.Frndint => FullX87KernelKey.RoundToIntegral,
            QX.Fsqrt => FullX87KernelKey.Sqrt,
            QX.Fxtract => FullX87KernelKey.Extract,
            QX.Fscale => (FullX87KernelKey)FullExtended80ReductionProtocol.ScaleKey,
            QX.Fprem => (FullX87KernelKey)FullExtended80ReductionProtocol.RemainderKey,
            QX.Fprem1 => (FullX87KernelKey)FullExtended80ReductionProtocol.RemainderNearestKey,
            _ => throw new ArgumentException("No control kernel")
        };
    }

    internal static bool ValidateCanonicalX87Control(JitInstruction i, in FullX87KernelCall call)
    {
        if (!ValidateCanonicalX87Control(i)) return false;
        if (!CanonicalX87ControlNeedsKernel(i))
        {
            if (call != default) throw new ArgumentException("Raw control must not carry a fake kernel");
        }
        else if (call.Key != (byte)CanonicalX87ControlKernel(i) || call.Entry == 0 || call.OwnerCookie == 0 ||
            call.CodeBytes <= 0 || call.CodeBytes > FullX87KernelCatalog.StagingBytes)
            throw new ArgumentException("Invalid canonical x87 control kernel tuple");
        // Parent authenticates literal Entry/size/owner under the SAME authority
        // gate, with catalog/root/epoch enrollment held through native RET.
        return true;
    }

    private static bool ValidateCanonicalX87ControlState(JitInstruction i,QX op)
    {
        bool fx = op is QX.Fxsave or QX.Fxrstor;
        bool small = (i.Options & FullCanonicalX87ControlLowering.StateEnvironment16) != 0;
        bool wide = (i.Options & FullCanonicalX87ControlLowering.StatePointers64) != 0;
        bool environment = op is QX.Fldenv or QX.Fnstenv or QX.Frstor or QX.Fnsave;
        uint encoding = (uint)i.Extra.Immediate; byte modrm = (byte)encoding;
        byte opcode = fx?(byte)0x0f:(byte)(0xd8|encoding>>8);
        var memory = new JitOperand(0,0,IsMemory:true); bool load = FullCanonicalX87ControlLowering.StateLoad(op);
        uint flags = FullCanonicalX87ControlLowering.StateEnvironment16 | FullCanonicalX87ControlLowering.StatePointers64;
        if (!CanInlineCanonicalX87Control(i) || i.Options != ((uint)op|(i.Options&flags)) ||
            small && !environment || wide && !fx || encoding > (fx?255u:0x7ffu) ||
            i.Extra != new JitOperand(0,encoding,IsImmediate:true) ||
            !FullCanonicalX87ControlLowering.StateOperation(opcode,modrm,out var decoded) || decoded != op ||
            i.Width != 80 || i.Length is < 2 or > 15 || (fx || small) && i.Length < 3 ||
            i.SourceWidth != FullCanonicalX87ControlLowering.StateBytes(op,small)*8 ||
            i.Source != (load?memory:default) || i.Destination != (load?default:memory) ||
            i.Target != 0 || i.Condition != 0 || !CanonicalX87MemoryAddressShape(i.Address,modrm,i.Length-(fx?1:0)))
            throw new ArgumentException("Invalid canonical x87 state/control memory IR");
        if (!CanonicalX87StackLayout.Supported || !ScalarMiscLayout.Supported)
            throw new PlatformNotSupportedException("Canonical x87 state layout differs");
        return true;
    }

    private ref partial struct Writer
    {
        private bool TryCanonicalX87ControlOperation(in JitInstruction i, in FullX87KernelCall call)
        {
            if (!CanInlineCanonicalX87Control(i)) return false;
            ValidateCanonicalX87Control(i,in call);
            if (CanInlineCanonicalX87Stack(i)) return TryCanonicalX87StackOperation(i);
            var op = (QX)(i.Options & 0xffff);
            if (op >= QX.Fldcw) { CanonicalX87ControlState(i,op); return true; }
            if (op == QX.Fxtract) { CanonicalX87ExtractOperation(i,in call); return true; }
            if (op is QX.Fscale or QX.Fprem or QX.Fprem1) { CanonicalX87ReductionOperation(i,op,in call); return true; }
            // All polls/context/owned576/canonicalActive/owner/PC admission and
            // Issue are parent owned. FSQRT uses software PC01=64. Prior six-mask
            // pending #MF precedes metadata, CC writes, stack checks or Work.
            FloatMmxWaitingException(i);
            if (op == QX.Fxam) { CanonicalX87Examine(i); return true; }
            if (op is >= QX.Fcmovb and <= QX.Fcmovnu) { CanonicalX87ConditionalMove(i,op); return true; }
            bool compare = op <= QX.Fucomip || op == QX.Ftst;
            bool flags = op is QX.Fcomi or QX.Fcomip or QX.Fucomi or QX.Fucomip;
            int pops = op is QX.Fcompp or QX.Fucompp ? 2 :
                op is QX.Fcomp or QX.Fucomp or QX.Fcomip or QX.Fucomip ? 1 : 0;
            int empty0 = CanonicalX87SourceCheck(0), emptyN = -1;
            if (compare && op != QX.Ftst) emptyN = CanonicalX87SourceCheck(i.Source.Register);
            CanonicalX87LoadRaw(0,0);
            if (compare)
            {
                if (op == QX.Ftst) CanonicalX87Literal(1,0,0);
                else CanonicalX87LoadRaw(i.Source.Register,1);
                CanonicalX87PrepareWork(0,1);
            }
            else CanonicalX87PrepareWork(0);
            CanonicalX87Invoke(in call); CanonicalX87RequireComplete();
            // No architectural write has occurred. Validate the operation's
            // protocol in full before interpreting any result or side effect.
            Span<int> bad = stackalloc int[96]; int count = 0;
            CanonicalX87ControlEqual(0,1,32,bad,ref count);
            CanonicalX87ControlEqual(4,384,32,bad,ref count);
            CanonicalX87ControlEqual(10,0,16,bad,ref count);
            CanonicalX87MachineLoad(672,1); IntegerConstant(4,1,0xffff);
            CanonicalX87ControlMatch(8,1,16,bad,ref count);
            if (compare) CanonicalX87ValidateCompare(flags,pops,bad,ref count);
            else CanonicalX87ValidateUnary(op,bad,ref count);
            CanonicalX87Metadata(i);
            int suppressed = -1;
            if (compare)
            {
                CanonicalX87CommitCompareStatus();
                if (flags) CanonicalX87CommitCompareFlags();
                if (pops != 0)
                {
                    CanonicalX87WorkLoad(84,0,32); IntegerConstant(4,0,2048);
                    suppressed = NearCondition(0x85);
                    for (int p = 0; p < pops; ++p) CanonicalX87Pop();
                }
            }
            else
            {
                CanonicalX87WorkLoad(82,2,16);
                CanonicalX87MergeControlDelta();
                CanonicalX87WorkLoad(84,0,32); IntegerConstant(4,0,op == QX.Frndint ? 256u : 1024u);
                suppressed = NearCondition(0x85);
                CanonicalX87WorkRaw(48,0);
                CanonicalX87StoreRaw(0,0);
            }
            int done = IntegerJump();
            Patch(empty0,position); if (emptyN >= 0) Patch(emptyN,position);
            CanonicalX87Metadata(i);
            if (flags) { IntegerImmediate(0,0,32); IntegerMergeFlags(0x890); }
            int emptySuppressed = CanonicalX87StackFault(false);
            if (compare)
            {
                if (flags) { IntegerImmediate(0,0x41,32); IntegerMergeFlags(0xff0008d1); }
                else
                {
                    CanonicalX87MachineLoad(648,0); IntegerConstant(4,0,~0x4700u);
                    IntegerConstant(1,0,0x4500); CanonicalX87MachineStore(648,0);
                }
                for (int p = 0; p < pops; ++p) CanonicalX87Pop();
            }
            else
            {
                CanonicalX87Indefinite(0); CanonicalX87StoreRaw(0,0);
            }
            int emptyDone = IntegerJump();
            for (int k = 0; k < count; ++k) Patch(bad[k],position);
            CanonicalX87WorkConstant(284,0x80000000); CanonicalX87ProtocolFatal();
            Patch(done,position); Patch(emptySuppressed,position); Patch(emptyDone,position);
            if (suppressed >= 0) Patch(suppressed,position);
            return true;
        }

        // Extract's two destinations complete together. Empty ST0 wins over
        // an occupied push slot; masked #IS writes BOTH indefinite images.
        // Numeric C1 and C0/C2/C3 are undefined and retained by this profile.
        private void CanonicalX87ExtractOperation(in JitInstruction i,in FullX87KernelCall call)
        {
            FloatMmxWaitingException(i);
            int empty = CanonicalX87SourceCheck(0), full = CanonicalX87PushCheck();
            CanonicalX87LoadRaw(0,0); CanonicalX87PrepareWork(0);
            CanonicalX87Invoke(in call); CanonicalX87RequireComplete();
            Span<int> bad = stackalloc int[96]; int count = 0;
            CanonicalX87ControlEqual(0,1,32,bad,ref count);
            CanonicalX87ControlEqual(4,384,32,bad,ref count);
            CanonicalX87ControlEqual(10,0,16,bad,ref count);
            CanonicalX87MachineLoad(672,1); IntegerConstant(4,1,0xffff);
            CanonicalX87ControlMatch(8,1,16,bad,ref count);
            CanonicalX87ValidateExtract(bad,ref count);
            // All protocol rejection edges precede every architectural write.
            CanonicalX87Metadata(i); CanonicalX87WorkLoad(82,2,16);
            CanonicalX87MachineLoad(648,0); IntegerRR(0x09,0,2,32);
            CanonicalX87MachineLoad(672,1); IntegerUnary(2,1,32);
            IntegerRR(0x21,1,2,32); IntegerConstant(4,1,63); int masked = NearCondition(0x84);
            IntegerConstant(1,0,0x8080); Patch(masked,position); CanonicalX87MachineStore(648,0);
            CanonicalX87WorkLoad(84,0,32); IntegerConstant(4,0,1024); int suppressed = NearCondition(0x85);
            // Wrapped is the exponent in old ST0; Normal is the pushed
            // significand. Load both before StoreRaw clobbers Work R9.
            CanonicalX87WorkRaw(64,1); CanonicalX87WorkRaw(48,0);
            CanonicalX87StoreRaw(0,1); CanonicalX87PushRaw(0); int done = IntegerJump();
            Patch(full,position); CanonicalX87Metadata(i);
            int fullSuppressed = CanonicalX87StackFault(true); int fullMasked = IntegerJump();
            Patch(empty,position); CanonicalX87Metadata(i);
            int emptySuppressed = CanonicalX87StackFault(false);
            Patch(fullMasked,position); CanonicalX87Indefinite(0);
            CanonicalX87StoreRaw(0,0); CanonicalX87PushRaw(0); int faultDone = IntegerJump();
            for (int k = 0; k < count; ++k) Patch(bad[k],position);
            CanonicalX87WorkConstant(284,0x80000003); CanonicalX87ProtocolFatal();
            Patch(suppressed,position); Patch(done,position); Patch(fullSuppressed,position);
            Patch(emptySuppressed,position); Patch(faultDone,position);
        }

        private void CanonicalX87ValidateExtract(scoped Span<int> bad,ref int count)
        {
            // Extract has only mutually exclusive IE/DE/ZE. No PE/C1 output,
            // arithmetic wrapped exponent or precision-control dependency.
            CanonicalX87WorkLoad(80,2,16); IntegerMov(0,2,32); IntegerConstant(4,0,~7u);
            bad[count++] = NearCondition(0x85);
            IntegerMov(0,2,32); IntegerConstant(5,0,1); IntegerRR(0x21,0,2,32);
            bad[count++] = NearCondition(0x85); // zero or one event only
            CanonicalX87WorkLoad(8,1,16); IntegerUnary(2,1,32); IntegerRR(0x21,1,2,32);
            IntegerConstant(4,1,7); IntegerRR(0x85,1,1,32); int usable = NearCondition(0x84);
            CanonicalX87ControlMatch(82,1,16,bad,ref count);
            IntegerImmediate(8,2u|1024u|2048u,32); int metadata = IntegerJump();
            Patch(usable,position); CanonicalX87ControlMatch(82,2,16,bad,ref count);
            IntegerImmediate(8,2u|4u|8u,32);
            IntegerMov(0,2,32); IntegerConstant(4,0,1); int inexact = NearCondition(0x85);
            CanonicalX87WorkLoad(24,0,16); IntegerConstant(4,0,0x7fff); IntegerConstant(7,0,0x7fff);
            int special = NearCondition(0x84); IntegerConstant(1,8,16);
            Patch(special,position); Patch(inexact,position); Patch(metadata,position);
            // EDX still contains the diagnostic event; suppressed and usable
            // events coincide for this operation's one-event protocol.
            ReadOnlySpan<int> eventBits = [1,2,4];
            foreach (int bit in eventBits)
            {
                IntegerMov(0,2,32); IntegerConstant(4,0,(uint)bit); int absent = NearCondition(0x84);
                IntegerConstant(1,8,bit == 1?32u:bit == 2?64u:128u); Patch(absent,position);
            }
            CanonicalX87ControlMatch(84,8,32,bad,ref count);
            IntegerRR(0x85,1,1,32); int images = NearCondition(0x84);
            CanonicalX87ControlEqual(48,0,64,bad,ref count); CanonicalX87ControlEqual(56,0,64,bad,ref count);
            CanonicalX87ControlEqual(64,0,64,bad,ref count); CanonicalX87ControlEqual(72,0,64,bad,ref count);
            int checkedImages = IntegerJump();
            Patch(images,position);
            CanonicalX87ControlEqual(58,0,16,bad,ref count); CanonicalX87ControlEqual(60,0,32,bad,ref count);
            CanonicalX87ControlEqual(74,0,16,bad,ref count); CanonicalX87ControlEqual(76,0,32,bad,ref count);
            CanonicalX87ControlValue(48,bad,ref count); CanonicalX87ControlValue(64,bad,ref count);
            Patch(checkedImages,position);
        }

        private void CanonicalX87ReductionOperation(in JitInstruction i,QX op,in FullX87KernelCall call)
        {
            FloatMmxWaitingException(i);
            int empty0 = CanonicalX87SourceCheck(0),empty1 = CanonicalX87SourceCheck(1);
            CanonicalX87LoadRaw(0,0); CanonicalX87LoadRaw(1,1); CanonicalX87PrepareWork(0,1);
            CanonicalX87Invoke(in call); CanonicalX87RequireComplete();
            Span<int> bad = stackalloc int[96]; int count = 0;
            CanonicalX87ControlEqual(0,1,32,bad,ref count); CanonicalX87ControlEqual(4,384,32,bad,ref count);
            CanonicalX87ControlEqual(10,0,16,bad,ref count); CanonicalX87MachineLoad(672,1);
            IntegerConstant(4,1,0xffff); CanonicalX87ControlMatch(8,1,16,bad,ref count);
            CanonicalX87ValidateReduction(op,bad,ref count);
            // Result selection follows full80 exponent range and actual masks.
            // These operations IGNORE PC, including01. Remainder ignores RC too.
            CanonicalX87WorkLoad(80,2,16); CanonicalX87WorkLoad(8,1,16);
            IntegerMov(0,1,32); IntegerUnary(2,0,32); IntegerRR(0x21,0,2,32); IntegerConstant(4,0,3);
            int pre = NearCondition(0x85);
            CanonicalX87WorkLoad(84,0,32); IntegerConstant(4,0,8); int noOverflow = NearCondition(0x84);
            IntegerMov(0,1,32); IntegerConstant(4,0,8); int wrappedOverflow = NearCondition(0x84);
            Patch(noOverflow,position); CanonicalX87WorkLoad(84,0,32); IntegerConstant(4,0,4); int noTiny = NearCondition(0x84);
            IntegerMov(0,1,32); IntegerConstant(4,0,16); int wrappedTiny = NearCondition(0x84);
            Patch(noTiny,position); CanonicalX87WorkRaw(48,0); int selected = IntegerJump();
            Patch(wrappedOverflow,position); Patch(wrappedTiny,position);
            CanonicalX87WorkRaw(64,0); CanonicalX87WorkLoad(82,2,16);
            Patch(selected,position); CanonicalX87Metadata(i); CanonicalX87CommitReductionStatus();
            CanonicalX87StoreRaw(0,0); int done = IntegerJump();
            Patch(pre,position); IntegerConstant(4,2,3);
            CanonicalX87Metadata(i); CanonicalX87CommitReductionStatus(); int stopped = IntegerJump();
            Patch(empty0,position); Patch(empty1,position); CanonicalX87Metadata(i);
            int emptySuppressed = CanonicalX87StackFault(false);
            CanonicalX87Indefinite(0); CanonicalX87StoreRaw(0,0); int emptyDone = IntegerJump();
            for (int p = 0; p < count; ++p) Patch(bad[p],position);
            CanonicalX87WorkConstant(284,0x80000002); CanonicalX87ProtocolFatal();
            Patch(done,position); Patch(stopped,position); Patch(emptySuppressed,position); Patch(emptyDone,position);
        }
        private void CanonicalX87ValidateReduction(QX op,scoped Span<int> bad,ref int count)
        {
            bool scale = op == QX.Fscale;
            CanonicalX87ControlEqual(244,scale?1u:op==QX.Fprem?2u:4u,32,bad,ref count);
            CanonicalX87ControlReserved(84,255,bad,ref count);
            CanonicalX87WorkLoad(84,0,32); IntegerConstant(4,0,1); bad[count++] = NearCondition(0x84);
            CanonicalX87WorkLoad(80,2,16); IntegerMov(0,2,32); IntegerConstant(4,0,scale?~0x23bu:~3u);
            bad[count++] = NearCondition(0x85);
            IntegerMov(0,2,32); IntegerConstant(4,0,1); int noInvalid = NearCondition(0x84);
            IntegerConstant(7,2,1); bad[count++] = NearCondition(0x85); Patch(noInvalid,position);
            IntegerMov(0,2,32); IntegerConstant(4,0,512); int noRoundup = NearCondition(0x84);
            IntegerMov(0,2,32); IntegerConstant(4,0,32); bad[count++] = NearCondition(0x84); Patch(noRoundup,position);
            CanonicalX87WorkLoad(82,0,16); IntegerConstant(4,0,scale?~0x1au:~0x12u); bad[count++] = NearCondition(0x85);
            // WrappedValid iff a tiny/overflow candidate exists. Both together
            // are impossible; the selected wrapped image must be valid FIRST.
            CanonicalX87WorkLoad(84,0,32); IntegerConstant(4,0,12); IntegerConstant(7,0,12);
            bad[count++] = NearCondition(0x84); IntegerRR(0x85,0,0,32); int noRange = NearCondition(0x84);
            CanonicalX87WorkLoad(84,0,32); IntegerConstant(4,0,2); bad[count++] = NearCondition(0x84);
            CanonicalX87WorkLoad(80,1,16); IntegerConstant(4,1,2);
            CanonicalX87WorkLoad(84,0,32); IntegerConstant(4,0,4); int overflow = NearCondition(0x84);
            IntegerConstant(1,1,16); int rangeEvent = IntegerJump();
            Patch(overflow,position); IntegerConstant(1,1,8); Patch(rangeEvent,position);
            CanonicalX87ControlMatch(82,1,16,bad,ref count);
            CanonicalX87ControlValue(64,bad,ref count); int wrappedDone = IntegerJump();
            Patch(noRange,position); CanonicalX87WorkLoad(84,0,32); IntegerConstant(4,0,2); bad[count++] = NearCondition(0x85);
            CanonicalX87ControlEqual(82,0,16,bad,ref count);
            CanonicalX87ControlEqual(64,0,64,bad,ref count); CanonicalX87ControlEqual(72,0,64,bad,ref count);
            Patch(wrappedDone,position);
            if (!scale)
            {
                CanonicalX87WorkLoad(84,0,32); IntegerConstant(4,0,8); bad[count++] = NearCondition(0x85);
            }
            CanonicalX87ControlEqual(58,0,16,bad,ref count); CanonicalX87ControlEqual(60,0,32,bad,ref count);
            CanonicalX87ControlEqual(74,0,16,bad,ref count); CanonicalX87ControlEqual(76,0,32,bad,ref count);
            CanonicalX87ControlValue(48,bad,ref count);
            if (scale)
            {
                CanonicalX87ControlEqual(240,0,16,bad,ref count); CanonicalX87ControlEqual(242,0x200,16,bad,ref count);
            }
            else
            {
                CanonicalX87WorkLoad(80,0,16); CanonicalX87WorkLoad(8,1,16); IntegerUnary(2,1,32);
                IntegerRR(0x21,0,1,32); IntegerConstant(4,0,3); int pre = NearCondition(0x85);
                CanonicalX87WorkLoad(84,0,32); IntegerConstant(4,0,64); int special = NearCondition(0x85);
                CanonicalX87ControlEqual(242,0x4700,16,bad,ref count);
                CanonicalX87WorkLoad(240,0,16); IntegerMov(1,0,32); IntegerConstant(4,0,~0x4700u); bad[count++] = NearCondition(0x85);
                IntegerMov(0,1,32); IntegerConstant(4,0,0x400); int complete = NearCondition(0x84);
                IntegerConstant(7,1,0x400); bad[count++] = NearCondition(0x85);
                Patch(complete,position); int ordinary = IntegerJump();
                Patch(pre,position); Patch(special,position);
                CanonicalX87ControlEqual(240,0,16,bad,ref count); CanonicalX87ControlEqual(242,0x200,16,bad,ref count);
                Patch(ordinary,position);
            }
        }
        private void CanonicalX87CommitReductionStatus()
        {
            CanonicalX87WorkLoad(240,8,16); CanonicalX87WorkLoad(242,1,16); IntegerUnary(2,1,32);
            CanonicalX87MachineLoad(648,0); IntegerRR(0x21,0,1,32); IntegerRR(0x09,0,8,32); IntegerRR(0x09,0,2,32);
            CanonicalX87MachineLoad(672,1); IntegerUnary(2,1,32); IntegerRR(0x21,1,2,32); IntegerConstant(4,1,63);
            int masked = NearCondition(0x84); IntegerConstant(1,0,0x8080); Patch(masked,position); CanonicalX87MachineStore(648,0);
        }

        private void CanonicalX87ConditionalMove(in JitInstruction i,QX op)
        {
            // Intel AP-526 242816-001 p39 explicitly reads BOTH operands
            // unconditionally, then tests cc. False cc cannot hide #IS.
            int a = CanonicalX87SourceCheck(0), b = CanonicalX87SourceCheck(i.Source.Register);
            CanonicalX87LoadRaw(i.Source.Register,0);
            CanonicalX87Metadata(i); // C1 ordinary/undefined CC retained by profile
            int cc = op switch { QX.Fcmovb => 2, QX.Fcmove => 4, QX.Fcmovbe => 6,
                QX.Fcmovu => 10, QX.Fcmovnb => 3, QX.Fcmovne => 5, QX.Fcmovnbe => 7, _ => 11 };
            Condition(cc); IntegerRR(0x85,0,0,32); int noMove = NearCondition(0x84);
            CanonicalX87StoreRaw(0,0); int done = IntegerJump();
            Patch(a,position); Patch(b,position); CanonicalX87Metadata(i);
            int suppressed = CanonicalX87StackFault(false);
            CanonicalX87Indefinite(0); CanonicalX87StoreRaw(0,0);
            Patch(done,position); Patch(noMove,position); Patch(suppressed,position);
        }

        private void CanonicalX87Examine(in JitInstruction i)
        {
            // Even empty slots retain a raw sign. FXAM never generates IE/DE
            // or #IS, never quiets SNaN, and never rewrites TOP/tags/raw data.
            CanonicalX87LoadRaw(0,0);
            Bytes(0x66,0x48,0x0f,0x7e,0xc2); // movq rdx,xmm0
            Bytes(0x66,0x44,0x0f,0xc5,0xc0,4); // pextrw r8d,xmm0,4
            IntegerMov(8,8,32); IntegerShift(5,8,32,6); IntegerConstant(4,8,0x200);
            int empty = CanonicalX87SourceCheck(0);
            Bytes(0x66,0x0f,0xc5,0xc8,4); IntegerConstant(4,1,0x7fff);
            IntegerRR(0x85,1,1,32); int exp0 = NearCondition(0x84);
            IntegerRR(0x85,2,2); int unsupported = NearCondition(0x89); // J=0
            IntegerConstant(7,1,0x7fff); int special = NearCondition(0x84);
            IntegerConstant(1,8,0x400); int normal = IntegerJump();
            Patch(special,position); IntegerImmediate(0,0x8000000000000000UL);
            IntegerRR(0x39,2,0); int nan = NearCondition(0x85);
            IntegerConstant(1,8,0x500); int infinity = IntegerJump();
            Patch(nan,position); IntegerConstant(1,8,0x100); int nanDone = IntegerJump();
            Patch(exp0,position); IntegerRR(0x85,2,2); int zero = NearCondition(0x84);
            IntegerConstant(1,8,0x4400); int denormal = IntegerJump();
            Patch(zero,position); IntegerConstant(1,8,0x4000); int zeroDone = IntegerJump();
            Patch(empty,position); IntegerConstant(1,8,0x4100);
            Patch(unsupported,position); Patch(normal,position); Patch(infinity,position);
            Patch(nanDone,position); Patch(denormal,position); Patch(zeroDone,position);
            CanonicalX87Metadata(i); CanonicalX87MachineLoad(648,0);
            IntegerConstant(4,0,~0x4700u); IntegerRR(0x09,0,8,32); CanonicalX87MachineStore(648,0);
        }

        // Protocol checks only: all failure edges reach fatal BEFORE metadata,
        // SW/EFLAGS/TOP/tag/raw/IP writes. Work private scratch may change.
        private void CanonicalX87ControlEqual(int offset,uint value,int width,scoped Span<int> bad,ref int count)
        {
            CanonicalX87WorkLoad(offset,0,width); IntegerConstant(7,0,value,width == 64 ? 64 : 32);
            bad[count++] = NearCondition(0x85);
        }
        private void CanonicalX87ControlMatch(int offset,int expected,int width,scoped Span<int> bad,ref int count)
        {
            CanonicalX87WorkLoad(offset,0,width); IntegerRR(0x39,0,expected,width == 64 ? 64 : 32);
            bad[count++] = NearCondition(0x85);
        }
        private void CanonicalX87ControlReserved(int offset,uint allowed,scoped Span<int> bad,ref int count)
        {
            CanonicalX87WorkLoad(offset,0,32); IntegerConstant(4,0,~allowed);
            bad[count++] = NearCondition(0x85);
        }
        private void CanonicalX87ValidateCompare(bool flags,int pops,scoped Span<int> bad,ref int count)
        {
            CanonicalX87WorkLoad(48,8,32); IntegerConstant(7,8,3); bad[count++] = NearCondition(0x87);
            CanonicalX87WorkLoad(80,2,16); IntegerConstant(7,2,2); bad[count++] = NearCondition(0x87);
            CanonicalX87ControlMatch(82,2,16,bad,ref count);
            CanonicalX87ControlEqual(64,0,64,bad,ref count); CanonicalX87ControlEqual(72,0,64,bad,ref count);
            // Expected metadata from relation/class events and CW. No output
            // write mask is trusted just because its reserved bits are clear.
            IntegerImmediate(1,1,32);
            IntegerConstant(7,8,3); int unordered = NearCondition(0x84);
            IntegerConstant(1,1,4); int relationDone = IntegerJump();
            Patch(unordered,position); IntegerConstant(7,2,2); bad[count++] = NearCondition(0x84);
            Patch(relationDone,position);
            IntegerConstant(7,2,1); int notInvalid = NearCondition(0x85);
            IntegerConstant(7,8,3); bad[count++] = NearCondition(0x85);
            IntegerConstant(1,1,8);
            Patch(notInvalid,position);
            IntegerConstant(7,2,2); int noDE = NearCondition(0x85);
            IntegerConstant(1,1,16|1024); Patch(noDE,position);
            CanonicalX87WorkLoad(8,0,16); IntegerUnary(2,0,32); IntegerRR(0x21,0,2,32);
            IntegerConstant(4,0,3); // any unmasked IE/DE suppresses requested pops
            int noSuppressPop = NearCondition(0x84);
            if (pops != 0) IntegerConstant(1,1,2048);
            Patch(noSuppressPop,position);
            IntegerConstant(4,0,1); int noSuppressCondition = NearCondition(0x84);
            IntegerConstant(1,1,128); Patch(noSuppressCondition,position);
            CanonicalX87ControlMatch(84,1,32,bad,ref count);
            // Branch on SUPPRESS CONDITION (IE only), independently of DE/pop.
            IntegerConstant(4,1,128); int ordinary = NearCondition(0x84);
            CanonicalX87ControlEqual(54,0x200,16,bad,ref count);
            CanonicalX87ControlEqual(60,flags ? 0x890u : 0,32,bad,ref count);
            int masksDone = IntegerJump();
            Patch(ordinary,position);
            CanonicalX87ControlEqual(54,flags ? 0x200u : 0x4700u,16,bad,ref count);
            CanonicalX87ControlEqual(60,flags ? 0x8d5u : 0,32,bad,ref count);
            Patch(masksDone,position);
            // Expected relation bits from an independent finite lookup in
            // emitted code. Even ignored masked-out contributions are checked.
            IntegerImmediate(1,0,32); IntegerRR(0x85,8,8,32); int greater = NearCondition(0x84);
            IntegerImmediate(1,flags ? 1u : 0x100u,32); IntegerConstant(7,8,1); int less = NearCondition(0x84);
            IntegerImmediate(1,flags ? 0x40u : 0x4000u,32); IntegerConstant(7,8,2); int equal = NearCondition(0x84);
            IntegerImmediate(1,flags ? 0x45u : 0x4500u,32);
            Patch(greater,position); Patch(less,position); Patch(equal,position);
            CanonicalX87ControlMatch(flags ? 56 : 52,1,flags ? 32 : 16,bad,ref count);
            CanonicalX87ControlEqual(flags ? 52 : 56,0,flags ? 16 : 32,bad,ref count);
        }

        private void CanonicalX87ValidateUnary(QX op,scoped Span<int> bad,ref int count)
        {
            bool round = op == QX.Frndint;
            const uint allowedStatus = 0x223;
            CanonicalX87WorkLoad(80,2,16); IntegerMov(0,2,32); IntegerConstant(4,0,~allowedStatus);
            bad[count++] = NearCondition(0x85);
            // C1=1 requires PE, IE is mutually exclusive with every other flag.
            IntegerMov(0,2,32); IntegerConstant(4,0,1); int noIE = NearCondition(0x84);
            IntegerConstant(7,2,1); bad[count++] = NearCondition(0x85); Patch(noIE,position);
            IntegerMov(0,2,32); IntegerConstant(4,0,0x200); int noUp = NearCondition(0x84);
            IntegerMov(0,2,32); IntegerConstant(4,0,32); bad[count++] = NearCondition(0x84); Patch(noUp,position);
            CanonicalX87WorkLoad(8,1,16); IntegerUnary(2,1,32); IntegerRR(0x21,1,2,32);
            IntegerConstant(4,1,3u);
            IntegerRR(0x85,1,1,32); int running = NearCondition(0x84);
            // Invalid/DE/ZE priority. They cannot overlap for these unary forms.
            CanonicalX87ControlMatch(82,1,16,bad,ref count);
            int suppressedStatus = IntegerJump();
            Patch(running,position); CanonicalX87ControlMatch(82,2,16,bad,ref count);
            Patch(suppressedStatus,position);
            if (round)
            {
                // Round metadata describes the masked numeric candidate even
                // when actual unmasked DE suppresses its PE/C1 contribution.
                IntegerImmediate(8,2,32);
                IntegerMov(0,2,32); IntegerConstant(4,0,1); int noInvalid = NearCondition(0x84);
                IntegerConstant(1,8,8); Patch(noInvalid,position);
                IntegerMov(0,2,32); IntegerConstant(4,0,2); int noDenormal = NearCondition(0x84);
                IntegerConstant(1,8,16); Patch(noDenormal,position);
                IntegerMov(0,2,32); IntegerConstant(4,0,32); int exact = NearCondition(0x84);
                IntegerConstant(1,8,32);
                IntegerMov(0,2,32); IntegerConstant(4,0,512); int notUp = NearCondition(0x84);
                IntegerConstant(1,8,64); Patch(notUp,position); int precisionDone = IntegerJump();
                Patch(exact,position); IntegerMov(0,2,32); IntegerConstant(4,0,1); int inexact = NearCondition(0x85);
                IntegerConstant(1,8,4); Patch(inexact,position); Patch(precisionDone,position);
                IntegerRR(0x85,1,1,32); int notSuppressed = NearCondition(0x84);
                IntegerConstant(1,8,256); int completion = IntegerJump();
                Patch(notSuppressed,position);
                CanonicalX87WorkLoad(8,0,16); IntegerUnary(2,0,32); IntegerRR(0x21,0,2,32);
                IntegerConstant(4,0,32); int noTrap = NearCondition(0x84);
                IntegerConstant(1,8,512); Patch(noTrap,position); Patch(completion,position);
                CanonicalX87ControlMatch(84,8,32,bad,ref count);
            }
            else
            {
                // Transform metadata uses ACTUAL events and zeros suppressed
                // images. Exact is restricted below from raw Left class.
                IntegerImmediate(8,1u,32);
                IntegerRR(0x85,1,1,32); int usable = NearCondition(0x84);
                IntegerConstant(1,8,1024u); int validityDone = IntegerJump();
                Patch(usable,position); IntegerConstant(1,8,4u|8192u);
                CanonicalX87WorkLoad(82,0,16); IntegerConstant(4,0,33); int notExact = NearCondition(0x85);
                CanonicalX87WorkLoad(24,0,16); IntegerConstant(4,0,0x7fff); IntegerConstant(7,0,0x7fff);
                int special = NearCondition(0x84); IntegerConstant(1,8,16); Patch(special,position); Patch(notExact,position);
                {
                    CanonicalX87WorkLoad(8,0,16); IntegerUnary(2,0,32); IntegerRR(0x21,0,2,32);
                    IntegerConstant(4,0,32); int noTrap = NearCondition(0x84);
                    IntegerConstant(1,8,4096); Patch(noTrap,position);
                }
                Patch(validityDone,position);
                CanonicalX87WorkLoad(82,2,16);
                ReadOnlySpan<int> eventBits = [1,2,4,32,512];
                foreach (int bit in eventBits)
                {
                    IntegerMov(0,2,32); IntegerConstant(4,0,(uint)bit); int absent = NearCondition(0x84);
                    IntegerConstant(1,8,bit switch { 1 => 32u,2 => 64u,4 => 128u,32 => 256u,_ => 512u });
                    Patch(absent,position);
                }
                CanonicalX87ControlMatch(84,8,32,bad,ref count);
                IntegerRR(0x85,1,1,32); int hasImages = NearCondition(0x84);
                CanonicalX87ControlEqual(48,0,64,bad,ref count); CanonicalX87ControlEqual(56,0,64,bad,ref count);
                CanonicalX87ControlEqual(64,0,64,bad,ref count); CanonicalX87ControlEqual(72,0,64,bad,ref count);
                Patch(hasImages,position);
            }
            // Canonical output padding; round and sqrt Wrapped always zero.
            CanonicalX87ControlEqual(58,0,16,bad,ref count); CanonicalX87ControlEqual(60,0,32,bad,ref count);
            CanonicalX87ControlEqual(64,0,64,bad,ref count); CanonicalX87ControlEqual(72,0,64,bad,ref count);
            CanonicalX87ControlValue(48,bad,ref count);
        }
        private void CanonicalX87ControlValue(int offset,scoped Span<int> bad,ref int count)
        {
            CanonicalX87WorkLoad(offset+8,0,16); IntegerConstant(4,0,0x7fff);
            IntegerRR(0x85,0,0,32); int exponentZero = NearCondition(0x84);
            CanonicalX87WorkLoad(offset,2); IntegerRR(0x85,2,2);
            bad[count++] = NearCondition(0x89); // J=0 is unsupported when e!=0
            IntegerConstant(7,0,0x7fff); int finite = NearCondition(0x85);
            IntegerImmediate(1,0x8000000000000000UL); IntegerRR(0x39,2,1);
            int infinity = NearCondition(0x84);
            IntegerImmediate(1,0x4000000000000000UL); IntegerRR(0x85,2,1);
            bad[count++] = NearCondition(0x84); // only QUIET NaN may be an output
            Patch(infinity,position); Patch(finite,position); Patch(exponentZero,position);
        }

        private void CanonicalX87MergeControlDelta()
        {
            // EDX=priority-restricted delta; replace defined C1 and retain
            // physical undefined C0/C2/C3 by explicit profile policy.
            CanonicalX87MachineLoad(648,0);
            IntegerConstant(4,0,~0x200u);
            IntegerRR(0x09,0,2,32); CanonicalX87MachineLoad(672,1); IntegerUnary(2,1,32);
            IntegerRR(0x21,1,2,32); IntegerConstant(4,1,63); int masked = NearCondition(0x84);
            IntegerConstant(1,0,0x8080); Patch(masked,position); CanonicalX87MachineStore(648,0);
        }
        private void CanonicalX87CommitCompareStatus()
        {
            CanonicalX87WorkLoad(54,1,16); IntegerUnary(2,1,32);
            CanonicalX87MachineLoad(648,0); IntegerRR(0x21,0,1,32);
            CanonicalX87WorkLoad(52,2,16); CanonicalX87WorkLoad(54,1,16);
            IntegerRR(0x21,2,1,32); IntegerRR(0x09,0,2,32);
            CanonicalX87WorkLoad(82,2,16); IntegerRR(0x09,0,2,32);
            CanonicalX87MachineLoad(672,1); IntegerUnary(2,1,32); IntegerRR(0x21,1,2,32);
            IntegerConstant(4,1,63); int masked = NearCondition(0x84);
            IntegerConstant(1,0,0x8080); Patch(masked,position); CanonicalX87MachineStore(648,0);
        }
        private void CanonicalX87CommitCompareFlags()
        {
            CanonicalX87WorkLoad(60,0,32); IntegerConstant(7,0,0x890); int hold = NearCondition(0x84);
            CanonicalX87WorkLoad(56,0,32); IntegerMov(1,0,32); IntegerShift(5,1,32,2);
            IntegerConstant(6,1,1); IntegerConstant(4,1,1); IntegerShift(4,1,32,24);
            IntegerConstant(4,0,0x41); IntegerRR(0x09,0,1,32); IntegerMergeFlags(0xff0008d1);
            int done = IntegerJump(); Patch(hold,position);
            IntegerImmediate(0,0,32); IntegerMergeFlags(0x890); Patch(done,position);
        }

        // Native integer state codecs. Mapping/lifetime/write authority for the
        // ENTIRE operand is established once before reading/restoring any field.
        // FXRSTOR validates and stages MXCSR before the first architectural
        // mutation, then performs only reads from already checked live pages.
        // No managed codec, host FXSAVE/FXRSTOR, MXCSR instruction or CALL.
        private void CanonicalX87ControlState(in JitInstruction i,QX op)
        {
            bool load = FullCanonicalX87ControlLowering.StateLoad(op);
            bool fx = op is QX.Fxsave or QX.Fxrstor;
            bool small = (i.Options & FullCanonicalX87ControlLowering.StateEnvironment16) != 0;
            bool wide = (i.Options & FullCanonicalX87ControlLowering.StatePointers64) != 0;
            if (op is QX.Fldcw or QX.Fldenv or QX.Frstor) FloatMmxWaitingException(i);
            // All FN forms and FX state transfers bypass prior x87 #MF.
            if (fx)
            {
                NativeMemoryAddress(i); IntegerConstant(4,0,15,64); int aligned = NearCondition(0x84);
                CanonicalX87StateGeneralProtection(); Patch(aligned,position);
            }
            ScalarMiscCheckCanonical(i,fx?4096:i.SourceWidth);
            if (fx) CanonicalX87ControlPermissionProbe(i,load?FullMemoryAccess.Read:FullMemoryAccess.Write);
            NativeMemoryPrepare(i,load?FullMemoryAccess.Read:FullMemoryAccess.Write,i.SourceWidth);
            if (op == QX.Fldcw)
            { CanonicalX87StateRead(0,16); CanonicalX87MachineStore(672,8); }
            else if (op is QX.Fnstcw or QX.Fnstsw)
            { CanonicalX87StateMachineRead(op == QX.Fnstcw?672:648,16); CanonicalX87StateWrite(0,16); }
            else if (load)
            {
                CanonicalX87StateWork();
                if (fx)
                {
                    CanonicalX87StateRead(24,32); IntegerMov(0,8,32); IntegerConstant(4,0,~0xffffu);
                    int valid = NearCondition(0x84); CanonicalX87StateGeneralProtection(); Patch(valid,position);
                    CanonicalX87WorkStore(296,8,32); // validated MXCSR cannot be re-read/forged later
                }
                CanonicalX87StateRead(fx?2:small?2:4,16);
                IntegerMov(0,8,32); IntegerConstant(4,8,0x7fff); IntegerConstant(4,0,0x80);
                IntegerShift(4,0,32,8); IntegerRR(0x09,8,0,32); CanonicalX87WorkStore(288,8,32);
                CanonicalX87StateStageTags(fx,small);
                if (fx || op == QX.Frstor)
                    for (int logical = 0; logical < 8; ++logical)
                        CanonicalX87StateRestoreRaw(logical,(fx?32:small?14:28)+logical*(fx?16:10));
                CanonicalX87StateRead(0,16); CanonicalX87MachineStore(672,8);
                CanonicalX87WorkLoad(288,8,32); CanonicalX87MachineStore(648,8);
                // Explicit virtual-CPU metadata profile, portable integer
                // behavior matching the finite AMD legacy-image witness.
                // Former omitted-FOP gap is not resolved by an ISA zero rule.
                if (small) IntegerImmediate(8,0,32);
                else { CanonicalX87StateRead(fx?6:18,16); IntegerConstant(4,8,0x7ff); }
                CanonicalX87MachineStore(656,8);
                CanonicalX87StateRead(fx?8:small?6:12,fx&&wide?64:small?16:32); CanonicalX87MachineStore(664,8,64);
                CanonicalX87StateRead(fx?16:small?10:20,fx&&wide?64:small?16:32); CanonicalX87MachineStore(680,8,64);
                if (fx && wide) IntegerImmediate(8,0,32); else CanonicalX87StateRead(fx?12:small?8:16,16);
                CanonicalX87StateSelectorStore(40);
                if (fx && wide) IntegerImmediate(8,0,32); else CanonicalX87StateRead(fx?20:small?12:24,16);
                CanonicalX87StateSelectorStore(42);
                CanonicalX87StateExpandTags();
                if (fx)
                {
                    CanonicalX87WorkLoad(296,8,32); CanonicalX87MachineStore(688,8);
                    for (int offset = 160; offset < 416; offset += 8)
                    { CanonicalX87StateRead(offset,64); CanonicalX87MachineStore(offset,8,64); }
                }
            }
            else if (fx) CanonicalX87StateSaveFx(wide);
            else
            {
                CanonicalX87StateSaveEnvironment(small);
                if (op == QX.Fnsave)
                {
                    for (int logical = 0; logical < 8; ++logical)
                        CanonicalX87StateSaveRaw(logical,(small?14:28)+logical*10,false);
                    CanonicalX87StateReset(); // only after ALL successful stores
                }
                else
                {
                    CanonicalX87StateMachineRead(672,16); IntegerConstant(1,8,63); CanonicalX87MachineStore(672,8);
                }
            }
            NativeMemoryCount();
        }
        // SAME R150 mapping-only tag. Architectural FX access validates512
        // bytes but authorizes data effects ONLY for the defined416 prefix.
        // The parent republishes this proof after actual416 assists and AFTER
        // final writer promotion/revocation at the final admitted epoch.
        private void CanonicalX87ControlPermissionProbe(in JitInstruction i,FullMemoryAccess access)
        {
            NativeStackMemoryDescriptor(); // R9=owned128 descriptor
            Bytes(0x41,0x81,0x39); U32(FullCanonicalX87MemoryLowering.MemoryProbeTag);
            int wrongTag = NearCondition(0x85);
            Bytes(0x41,0x83,0x79,4,1); int wrongCount = NearCondition(0x85);
            Bytes(0x41,0x83,0x79,12,(byte)FullStackMemoryState.Ready); int ready = NearCondition(0x84);
            Patch(wrongTag,position); Patch(wrongCount,position);
            // A malformed/missing proof MUST NOT silently discard an actual
            // 416-byte reservation. Parent releases its ticket scope on RET.
            Bytes(0x41,0x83,0xba); U32(324); Bytes(0); ExitCondition(0x85,FullBlockExit.Invalidated);
            Bytes(0x49,0x83,0xba); U32(344); Bytes(0); ExitCondition(0x85,FullBlockExit.Invalidated);
            NativeMemoryAddress(i); IntegerFrameStore(216,0);
            Bytes(0x49,0x89,0x41,32);
            NativeStackMemoryDword(0,FullCanonicalX87MemoryLowering.MemoryProbeTag);
            NativeStackMemoryDword(4,1);
            Bytes(0x41,0x8b,0x82); U32(144); Bytes(0x41,0x89,0x41,8);
            NativeStackMemoryDword(12,(uint)FullStackMemoryState.Request);
            NativeStackMemoryZeroPointer(16); NativeStackMemoryZeroPointer(24);
            for (int offset = 40; offset < 128; offset += 8) NativeStackMemoryZeroPointer(offset);
            NativeStackMemoryDword(56,(uint)access); NativeStackMemoryDword(60,4096);
            Bytes(0x41,0x8b,0x82); U32(144); Bytes(0x41,0x89,0x82); U32(248);
            Bytes(0x41,0xc7,0x82); U32(252); U32((uint)access);
            Bytes(0x41,0xc7,0x82); U32(256); U32(4096);
            Bytes(0x41,0xc7,0x82); U32(292); U32(1);
            Bytes(0x49,0xc7,0x82); U32(224); U32(0);
            Bytes(0x49,0xc7,0x82); U32(232); U32(0);
            Bytes(0x49,0xc7,0x82); U32(240); U32(0);
            Bytes(0x41,0xc7,0x82); U32(260); U32(0);
            // No data pointer, TLB entry, ticket or write authority is created.
            JumpExit(FullBlockExit.NeedMemory);

            Patch(ready,position);
            Bytes(0x41,0x8b,0x82); U32(144); Bytes(0x41,0x3b,0x41,8);
            ExitCondition(0x85,FullBlockExit.Invalidated);
            Bytes(0x41,0x3b,0x82); U32(248); ExitCondition(0x85,FullBlockExit.Invalidated);
            Bytes(0x41,0x83,0xba); U32(292); Bytes(1); ExitCondition(0x85,FullBlockExit.Invalidated);
            NativeMemoryAddress(i); Bytes(0x49,0x3b,0x41,32); ExitCondition(0x85,FullBlockExit.Invalidated);
            Bytes(0x49,0x3b,0x82); U32(216); ExitCondition(0x85,FullBlockExit.Invalidated);
            Bytes(0x41,0x81,0xba); U32(252); U32((uint)access); ExitCondition(0x85,FullBlockExit.Invalidated);
            Bytes(0x41,0x81,0x79,56); U32((uint)access); ExitCondition(0x85,FullBlockExit.Invalidated);
            Bytes(0x41,0x81,0x79,60); U32(4096); ExitCondition(0x85,FullBlockExit.Invalidated);
            for (int offset = 24; offset < 128; offset += 8)
            {
                if (offset is 32 or 56) continue;
                Bytes(0x49,0x83,0x79,(byte)offset,0); ExitCondition(0x85,FullBlockExit.Invalidated);
            }
            // Initial proof: summary512, no reservation. Actual-resume proof:
            // summary416, either a filled TLB or exact actual416 ticket. The
            // descriptor itself NEVER owns any host pointer or ticket.
            Bytes(0x41,0x81,0xba); U32(256); U32(4096); int actual = NearCondition(0x85);
            Bytes(0x41,0x83,0xba); U32(324); Bytes(0); ExitCondition(0x85,FullBlockExit.Invalidated);
            Bytes(0x49,0x83,0xba); U32(344); Bytes(0); ExitCondition(0x85,FullBlockExit.Invalidated);
            int shape = IntegerJump();
            Patch(actual,position);
            Bytes(0x41,0x81,0xba); U32(256); U32(3328); ExitCondition(0x85,FullBlockExit.Invalidated);
            Bytes(0x41,0x83,0xba); U32(324); Bytes(0); int hasTicket = NearCondition(0x85);
            Bytes(0x49,0x83,0xba); U32(344); Bytes(0); ExitCondition(0x85,FullBlockExit.Invalidated);
            int ordinary = IntegerJump();
            Patch(hasTicket,position);
            Bytes(0x41,0x83,0xba); U32(324); Bytes((byte)FullMemoryFlags.BarrierTicket);
            ExitCondition(0x85,FullBlockExit.Invalidated);
            Bytes(0x49,0x83,0xba); U32(344); Bytes(0); ExitCondition(0x84,FullBlockExit.Invalidated);
            Bytes(0x49,0x8b,0x82); U32(240); Bytes(0x49,0x3b,0x82); U32(136);
            ExitCondition(0x85,FullBlockExit.Invalidated);
            Patch(ordinary,position); Patch(shape,position);
            Bytes(0x49,0x8b,0x41,16); Bytes(0x49,0x3b,0x82); U32(136);
            ExitCondition(0x85,FullBlockExit.Invalidated);
            Bytes(0x49,0x8b,0x92); U32(128); IntegerRR(0x85,2,2); ExitCondition(0x84,FullBlockExit.Invalidated);
            Bytes(0x48,0x3b,0x02); ExitCondition(0x85,FullBlockExit.Invalidated);
            NativeStackMemoryDword(12,(uint)FullStackMemoryState.Consumed);
            // This exact512 proof has now authenticated and consumed. Restore
            // the inert dense tag/count BEFORE ordinary416 prepare: its separate
            // actual ticket remains in the frame and must not be discarded or
            // mistaken for ownership by the mapping-only proof descriptor.
            NativeStackMemoryDword(0,FullStackMemoryDescriptor.CurrentVersion);
            NativeStackMemoryDword(4,0);
            Bytes(0x41,0x81,0xba); U32(256); U32(4096); int keepActual = NearCondition(0x85);
            Bytes(0x41,0xc7,0x82); U32(292); U32(0);
            Patch(keepActual,position);
            CanonicalX87StateWork(); // no architecture touched; restore R9 Work
        }

        private void CanonicalX87StateGeneralProtection()
        {
            Bytes(0x41,0xc7,0x82); U32(168); U32(13);
            // Architectural #GP(0); this frame field is Linux siginfo.si_code,
            // not the CPU error code. Parent requires SI_KERNEL=128/address0.
            Bytes(0x41,0xc7,0x82); U32(172); U32(128);
            Bytes(0x49,0xc7,0x82); U32(176); U32(0);
            JumpExit(FullBlockExit.GuestFault);
        }
        private void CanonicalX87StateWork()
        { IntegerFrameLoad(400,9); IntegerConstant(0,9,192,64); }
        // NativeBytePointer preserves R8/R9. All paths are exact-width byte
        // accesses, including unaligned/cross-page headers and raw80 slots.
        private void CanonicalX87StateRead(int offset,int width)
        {
            IntegerImmediate(8,0,64);
            for (int b = 0; b < width/8; ++b)
            {
                NativeBytePointer(offset+b); Bytes(0x0f,0xb6,0x02);
                if (b != 0) IntegerShift(4,0,64,8*b);
                IntegerRR(0x09,8,0,64);
            }
        }
        private void CanonicalX87StateWrite(int offset,int width)
        {
            for (int b = 0; b < width/8; ++b)
            {
                NativeBytePointer(offset+b); IntegerMov(0,8,64);
                if (b != 0) IntegerShift(5,0,64,8*b);
                Bytes(0x88,0x02);
            }
        }
        private void CanonicalX87StateMachineRead(int offset,int width)
        {
            IntegerPrefix(width==64?64:32,11,8); Bytes(0x8b); Memory(0,3,offset);
            if (width == 16) IntegerConstant(4,8,0xffff);
        }
        private void CanonicalX87StateSelectorRead(int offset)
        {
            IntegerFrameLoad(400,0); IntegerPrefix(32,0,8); Bytes(0x0f,0xb7); Memory(0,0,offset);
        }
        private void CanonicalX87StateSelectorStore(int offset)
        {
            IntegerFrameLoad(400,0); IntegerPrefix(16,0,8); Bytes(0x89); Memory(0,0,offset);
        }
        private void CanonicalX87StateConstant(int offset,int width,uint value)
        { IntegerImmediate(8,value,32); CanonicalX87StateWrite(offset,width); }
        private void CanonicalX87StateSaveRaw(int logical,int offset,bool padding)
        {
            CanonicalX87LoadRaw(logical,0);
            Bytes(0x66,0x49,0x0f,0x7e,0xc0); CanonicalX87StateWrite(offset,64);
            Bytes(0x66,0x44,0x0f,0xc5,0xc0,4); CanonicalX87StateWrite(offset+8,16);
            if (padding)
            { CanonicalX87StateConstant(offset+10,16,0); CanonicalX87StateConstant(offset+12,32,0); }
        }
        private void CanonicalX87StateRestoreRaw(int logical,int offset)
        {
            CanonicalX87StateRead(offset,64); Bytes(0x66,0x49,0x0f,0x6e,0xc0);
            CanonicalX87StateRead(offset+8,16); Bytes(0x66,0x41,0x0f,0xc4,0xc0,4);
            CanonicalX87WorkLoad(288,1,32); IntegerShift(5,1,32,11);
            IntegerConstant(0,1,(uint)logical); IntegerConstant(4,1,7); IntegerShift(4,1,32,4);
            IntegerConstant(0,1,64); IntegerFrameLoad(400,0); IntegerRR(0x01,0,1,64);
            Bytes(0x66,0x0f,0xd6,0x00,0x66,0x0f,0xc5,0xc8,4); Bytes(0x66,0x89,0x48,8);
            Bytes(0x66,0xc7,0x40,10,0,0,0xc7,0x40,12); U32(0);
        }
        private void CanonicalX87StateStageTags(bool fx,bool small)
        {
            CanonicalX87StateRead(fx?4:small?4:8,fx?8:16);
            if (fx) CanonicalX87WorkStore(292,8,32);
            else
            {
                IntegerImmediate(2,0,32);
                for (int p = 0; p < 8; ++p)
                {
                    IntegerMov(0,8,32); IntegerShift(5,0,32,p*2); IntegerConstant(4,0,3);
                    IntegerConstant(7,0,3); int empty = NearCondition(0x84);
                    IntegerConstant(1,2,1u<<p); Patch(empty,position);
                }
                CanonicalX87WorkStore(292,2,32);
            }
        }
        private void CanonicalX87StateExpandTags()
        {
            CanonicalX87WorkConstant(300,0);
            for (int p = 0; p < 8; ++p)
            {
                CanonicalX87WorkLoad(292,0,32); IntegerConstant(4,0,1u<<p); int empty = NearCondition(0x84);
                IntegerFrameLoad(400,0); Bytes(0xf3,0x0f,0x6f); Memory(0,0,64+16*p);
                Bytes(0x66,0x48,0x0f,0x7e,0xc2,0x66,0x44,0x0f,0xc5,0xc0,4);
                IntegerConstant(4,8,0x7fff); IntegerRR(0x85,8,8,32); int exp0 = NearCondition(0x84);
                IntegerConstant(7,8,0x7fff); int special = NearCondition(0x84);
                IntegerRR(0x85,2,2,64); int unsupported = NearCondition(0x89);
                IntegerImmediate(2,0,32); int normal = IntegerJump();
                Patch(exp0,position); IntegerRR(0x85,2,2,64); int denormal = NearCondition(0x85);
                IntegerImmediate(2,1,32); int zero = IntegerJump();
                Patch(special,position); Patch(unsupported,position); Patch(denormal,position);
                IntegerImmediate(2,2,32); int specialDone = IntegerJump();
                Patch(empty,position); IntegerImmediate(2,3,32);
                Patch(normal,position); Patch(zero,position); Patch(specialDone,position);
                IntegerShift(4,2,32,p*2); CanonicalX87WorkLoad(300,0,32);
                IntegerRR(0x09,0,2,32); CanonicalX87WorkStore(300,0,32);
            }
            CanonicalX87WorkLoad(300,8,32); CanonicalX87MachineStore(652,8);
        }
        private void CanonicalX87StateSaveEnvironment(bool small)
        {
            CanonicalX87StateMachineRead(672,16); CanonicalX87StateWrite(0,16);
            CanonicalX87StateMachineRead(648,16); CanonicalX87StateWrite(small?2:4,16);
            CanonicalX87StateMachineRead(652,16); CanonicalX87StateWrite(small?4:8,16);
            CanonicalX87StateMachineRead(664,small?16:32); CanonicalX87StateWrite(small?6:12,small?16:32);
            CanonicalX87StateSelectorRead(40); CanonicalX87StateWrite(small?8:16,16);
            CanonicalX87StateMachineRead(680,small?16:32); CanonicalX87StateWrite(small?10:20,small?16:32);
            CanonicalX87StateSelectorRead(42); CanonicalX87StateWrite(small?12:24,16);
            if (!small)
            {
                CanonicalX87StateMachineRead(656,16); IntegerConstant(4,8,0x7ff); CanonicalX87StateWrite(18,16);
                CanonicalX87StateConstant(2,16,0xffff); CanonicalX87StateConstant(6,16,0xffff);
                CanonicalX87StateConstant(10,16,0xffff); CanonicalX87StateConstant(26,16,0xffff);
            }
        }
        private void CanonicalX87StateSaveFx(bool wide)
        {
            CanonicalX87StateMachineRead(672,16); CanonicalX87StateWrite(0,16);
            CanonicalX87StateMachineRead(648,16); CanonicalX87StateWrite(2,16);
            CanonicalX87MachineLoad(652,2); IntegerImmediate(8,0,32);
            for (int p = 0; p < 8; ++p)
            {
                IntegerMov(0,2,32); IntegerShift(5,0,32,p*2); IntegerConstant(4,0,3);
                IntegerConstant(7,0,3); int empty = NearCondition(0x84);
                IntegerConstant(1,8,1u<<p); Patch(empty,position);
            }
            CanonicalX87StateWrite(4,8); CanonicalX87StateConstant(5,8,0);
            CanonicalX87StateMachineRead(656,16); IntegerConstant(4,8,0x7ff); CanonicalX87StateWrite(6,16);
            CanonicalX87StateMachineRead(664,wide?64:32); CanonicalX87StateWrite(8,wide?64:32);
            CanonicalX87StateMachineRead(680,wide?64:32); CanonicalX87StateWrite(16,wide?64:32);
            if (!wide)
            {
                CanonicalX87StateSelectorRead(40); CanonicalX87StateWrite(12,16); CanonicalX87StateConstant(14,16,0);
                CanonicalX87StateSelectorRead(42); CanonicalX87StateWrite(20,16); CanonicalX87StateConstant(22,16,0);
            }
            CanonicalX87StateMachineRead(688,32); CanonicalX87StateWrite(24,32); CanonicalX87StateConstant(28,32,0xffff);
            for (int logical = 0; logical < 8; ++logical) CanonicalX87StateSaveRaw(logical,32+logical*16,true);
            for (int offset = 160; offset < 416; offset += 8)
            { CanonicalX87StateMachineRead(offset,64); CanonicalX87StateWrite(offset,64); }
            // Bytes416..511 have already passed full512 canonical/permission
            // proof, but are never data-read/written or enrolled as writers.
        }
        private void CanonicalX87StateReset()
        {
            CanonicalX87MachineConstant(648,0); CanonicalX87MachineConstant(652,0xffff);
            CanonicalX87MachineConstant(656,0); CanonicalX87MachineConstant(672,0x37f);
            IntegerImmediate(0,0,64); CanonicalX87MachineStore(664,0,64); CanonicalX87MachineStore(680,0,64);
            IntegerFrameLoad(400,0); Bytes(0xc7,0x40,40); U32(0);
        }
    }
}
