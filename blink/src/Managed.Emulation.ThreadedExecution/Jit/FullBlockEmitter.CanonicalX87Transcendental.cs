using System;
using TX = Managed.Emulation.Execution.FullCanonicalX87TranscendentalOperation;

namespace Managed.Emulation.Execution;

internal static partial class FullBlockEmitter
{
    internal static bool RecognizesCanonicalX87Transcendental(JitInstruction i) =>
        i.Operation==JitOperation.X87 && (i.Options&0xffff) is >=0xa001 and <=0xa008;
    internal static bool CanInlineCanonicalX87Transcendental(JitInstruction i) =>
        RecognizesCanonicalX87Transcendental(i); // isolated IR/contracts only; production enable and GuestActivationReady stay FALSE.
    internal static bool ValidateCanonicalX87Transcendental(JitInstruction i)
    {
        if(!RecognizesCanonicalX87Transcendental(i))return false;
        var op=(TX)(i.Options&0xffff);int n=FullCanonicalX87TranscendentalLowering.IsBinary(op)?1:0;
        uint fop=op switch {TX.F2xm1=>0x1f0,TX.Fyl2x=>0x1f1,TX.Fptan=>0x1f2,TX.Fpatan=>0x1f3,
            TX.Fyl2xp1=>0x1f9,TX.Fsincos=>0x1fb,TX.Fsin=>0x1fe,_=>0x1ff};
        if(i.Options!=(uint)op || i.Width!=80 || i.Length is <2 or >15 || i.Destination!=default ||
            i.Source!=new JitOperand(n,0) || i.Target!=0 || i.Condition!=0 || i.SourceWidth!=0 ||
            i.Address!=default || i.Extra!=new JitOperand(0,fop,IsImmediate:true))
            throw new ArgumentException("Invalid canonical x87 transcendental family IR");
        if(!CanonicalX87StackLayout.Supported)throw new PlatformNotSupportedException("Canonical x87 layout differs");
        return true;
    }
    internal static FullX87KernelKey CanonicalX87TranscendentalKernel(JitInstruction i)
    {
        if(!ValidateCanonicalX87Transcendental(i))throw new ArgumentException("Not transcendental IR");
        return (FullX87KernelKey)(69+(int)i.Options-0xa001);
    }
    internal static bool CanonicalX87TranscendentalRequiresArithmeticPrecision(JitInstruction i) =>
        ValidateCanonicalX87Transcendental(i) && false; // PC is ignored, including01.
    internal static bool ValidateCanonicalX87Transcendental(JitInstruction i,in FullX87KernelCall call)
    {
        if(!ValidateCanonicalX87Transcendental(i))return false;
        if(call.Key!=(byte)CanonicalX87TranscendentalKernel(i) || call.Entry==0 || call.OwnerCookie==0 ||
            call.CodeBytes<=0 || call.CodeBytes>FullX87KernelCatalog.StagingBytes)
            throw new ArgumentException("Invalid transcendental family kernel tuple");
        // Parent additionally authenticates under Authority.Gate through native
        // RET, owns domain admission, and MUST retain the async activation gate.
        return true;
    }

    private ref partial struct Writer
    {
        private bool TryCanonicalX87TranscendentalOperation(in JitInstruction i,in FullX87KernelCall call)
        {
            if(!CanInlineCanonicalX87Transcendental(i))return false;
            ValidateCanonicalX87Transcendental(i,in call);
            var op=(TX)i.Options;bool binary=FullCanonicalX87TranscendentalLowering.IsBinary(op);
            bool pair=FullCanonicalX87TranscendentalLowering.Pushes(op);
            FloatMmxWaitingException(i); // before Work/metadata/CC/stack effects
            int empty0=CanonicalX87SourceCheck(0),empty1=binary?CanonicalX87SourceCheck(1):-1;
            CanonicalX87LoadRaw(0,0);if(binary)CanonicalX87LoadRaw(1,1);
            int full=-1;
            if(pair)
            {
                // Finite C2 range completion needs no push destination. This
                // proof precedes PushCheck, while empty source still wins.
                CanonicalX87PrepareWork(0);
                CanonicalX87WorkLoad(24,0,16);IntegerConstant(4,0,0x7fff);
                IntegerConstant(7,0,0x403e);int needsPush0=NearCondition(0x82);
                IntegerConstant(7,0,0x7fff);int needsPush1=NearCondition(0x83);
                CanonicalX87WorkLoad(16,0);IntegerRR(0x85,0,0);int needsPush2=NearCondition(0x89);
                int noPush=IntegerJump();
                Patch(needsPush0,position);Patch(needsPush1,position);Patch(needsPush2,position);
                full=CanonicalX87PushCheck();Patch(noPush,position);
            }
            CanonicalX87PrepareWork(0,binary?1:-1);
            CanonicalX87Invoke(in call);
            // Nonzero admission is a protocol/unsupported-host response. This
            // payload changes no production admission; runtime/native-NE and
            // async-safe-boundary qualification remain parent activation gates.
            CanonicalX87RequireComplete();
            Span<int> bad=stackalloc int[512];int count=0;
            CanonicalX87ControlEqual(0,1,32,bad,ref count);CanonicalX87ControlEqual(4,384,32,bad,ref count);
            CanonicalX87ControlEqual(10,0,16,bad,ref count);
            CanonicalX87MachineLoad(672,1);IntegerConstant(4,1,0xffff);CanonicalX87ControlMatch(8,1,16,bad,ref count);
            CanonicalX87ControlEqual(236,0,32,bad,ref count);
            CanonicalX87ControlEqual(248,0,64,bad,ref count); // no private host RSP escape
            CanonicalX87ValidateTranscendentalSoftware(op,bad,ref count);
            // Entire response validation precedes every architectural write.
            CanonicalX87Metadata(i);CanonicalX87MergeTranscendentalStatus();
            CanonicalX87WorkLoad(84,0,32);IntegerConstant(4,0,0x400|0x800);int held=NearCondition(0x85);
            CanonicalX87WorkRaw(48,0);if(pair)CanonicalX87WorkRaw(64,1);
            CanonicalX87StoreRaw(binary?1:0,0);
            if(binary)CanonicalX87Pop();else if(pair)CanonicalX87PushRaw(1);
            int done=IntegerJump();
            int fullSuppressed=-1,fullMasked=-1;
            if(pair)
            {
                Patch(full,position);CanonicalX87Metadata(i);fullSuppressed=CanonicalX87StackFault(true);
                fullMasked=IntegerJump();
            }
            Patch(empty0,position);if(empty1>=0)Patch(empty1,position);
            CanonicalX87Metadata(i);int emptySuppressed=CanonicalX87StackFault(false);
            if(pair)Patch(fullMasked,position);
            CanonicalX87Indefinite(0);CanonicalX87StoreRaw(binary?1:0,0);
            if(binary)CanonicalX87Pop();else if(pair)CanonicalX87PushRaw(0);
            int stackDone=IntegerJump();
            for(int k=0;k<count;++k)Patch(bad[k],position);
            CanonicalX87WorkConstant(284,0x80000007);CanonicalX87ProtocolFatal();
            Patch(held,position);Patch(done,position);Patch(emptySuppressed,position);Patch(stackDone,position);
            if(pair)Patch(fullSuppressed,position);
            return true;
        }

        // Historical host validator retained for source/receipt comparison only.
        // No canonical guest binding or wrapper calls this unsafe host protocol.
        private void CanonicalX87ValidateTranscendentalLegacy(TX op,scoped Span<int> bad,ref int count)
        {
            bool pair=FullCanonicalX87TranscendentalLowering.Pushes(op);
            CanonicalX87WorkLoad(84,0,32);IntegerConstant(4,0,0x10000);int integer=NearCondition(0x85);
            bool trig=op is TX.Fptan or TX.Fsincos or TX.Fsin or TX.Fcos;
            uint kind=(uint)op-0xa000;
            CanonicalX87ControlReserved(80,0x023f023fu,bad,ref count);
            CanonicalX87ControlReserved(216,0xffff,bad,ref count);
            CanonicalX87WorkLoad(84,0,32);IntegerConstant(4,0,0x800);int range=NearCondition(0x85);
            CanonicalX87WorkLoad(84,0,32);IntegerConstant(4,0,0x8000);int undefined=NearCondition(0x85);
            CanonicalX87WorkLoad(80,2,16);CanonicalX87WorkLoad(8,1,16);IntegerUnary(2,1,32);
            IntegerRR(0x21,1,2,32);IntegerConstant(4,1,7);IntegerRR(0x85,1,1,32);int usable=NearCondition(0x84);
            // Expected priority event = lowbit(diagnostic & ~CW & 7).
            IntegerMov(0,1,32);IntegerConstant(5,0,1);IntegerRR(0x21,0,1,32);IntegerRR(0x31,1,0,32);
            CanonicalX87ControlMatch(82,1,16,bad,ref count);
            CanonicalX87ControlEqual(84,kind|0x400|0x2000,32,bad,ref count);
            CanonicalX87ControlEqual(244,0,32,bad,ref count);CanonicalX87ControlEqual(216,0,32,bad,ref count);
            CanonicalX87ControlEqual(232,0x02000000,32,bad,ref count);
            int zeroImages=IntegerJump();
            Patch(usable,position);
            IntegerImmediate(8,kind|0x100|0x2000|(pair?0x200u:0),32);
            CanonicalX87WorkLoad(8,0,16);IntegerConstant(4,0,0x38);IntegerConstant(7,0,0x38);
            int captured=NearCondition(0x85);
            CanonicalX87ControlMatch(82,2,16,bad,ref count);
            CanonicalX87ControlEqual(244,0,32,bad,ref count);CanonicalX87ControlEqual(216,0,32,bad,ref count);
            int eventsChecked=IntegerJump();Patch(captured,position);
            IntegerConstant(1,8,0x4000);CanonicalX87ControlEqual(244,1,32,bad,ref count);
            CanonicalX87WorkLoad(216,1,32);IntegerConstant(4,1,0x3800);IntegerConstant(7,1,pair?0x3000u:0x3800u);
            bad[count++]=NearCondition(0x85);
            // Captured raw status supplies actual post-operation rounding/range
            // events. IE preempts DE; pseudo-DE is explicitly retained.
            CanonicalX87WorkLoad(216,1,16);IntegerConstant(4,1,0x23f);
            CanonicalX87WorkLoad(80,2,16);IntegerConstant(4,2,3);IntegerRR(0x09,1,2,32);
            IntegerMov(0,1,32);IntegerConstant(4,0,1);int notInvalid=NearCondition(0x84);
            IntegerImmediate(1,1,32);Patch(notInvalid,position);
            CanonicalX87ControlMatch(82,1,16,bad,ref count);
            // Every byte of the private legacy capture must be scrubbed.
            for(int off=96;off<208;off+=8)CanonicalX87ControlEqual(off,0,64,bad,ref count);
            Patch(eventsChecked,position);CanonicalX87WorkLoad(82,2,16);
            IntegerMov(0,2,32);IntegerConstant(4,0,32);int noPE=NearCondition(0x84);
            CanonicalX87WorkLoad(8,0,16);IntegerConstant(4,0,32);int maskedPE=NearCondition(0x85);
            IntegerConstant(1,8,0x1000);Patch(noPE,position);Patch(maskedPE,position);
            CanonicalX87ControlMatch(84,8,32,bad,ref count);
            IntegerMov(1,2,32);IntegerConstant(4,1,0x200);CanonicalX87ControlMatch(232,1,16,bad,ref count);
            CanonicalX87ControlEqual(234,trig?0x600u:0x200u,16,bad,ref count);
            CanonicalX87ControlEqual(58,0,16,bad,ref count);CanonicalX87ControlEqual(60,0,32,bad,ref count);
            CanonicalX87ControlValue(48,bad,ref count);
            if(pair)
            {
                CanonicalX87ControlEqual(74,0,16,bad,ref count);CanonicalX87ControlEqual(76,0,32,bad,ref count);
                CanonicalX87ControlValue(64,bad,ref count);
            }
            else
            {CanonicalX87ControlEqual(64,0,64,bad,ref count);CanonicalX87ControlEqual(72,0,64,bad,ref count);}
            int done=IntegerJump();
            Patch(undefined,position);
            if(op is not (TX.F2xm1 or TX.Fyl2xp1))
            {IntegerImmediate(0,1,32);IntegerRR(0x85,0,0,32);bad[count++]=NearCondition(0x85);}
            CanonicalX87ControlEqual(84,kind|0x100|0x8000,32,bad,ref count);
            CanonicalX87ControlEqual(80,0,32,bad,ref count);CanonicalX87ControlEqual(232,0x02000000,32,bad,ref count);
            CanonicalX87ControlEqual(244,0,32,bad,ref count);CanonicalX87ControlEqual(216,0,32,bad,ref count);
            CanonicalX87TranscendentalUndefinedProof(op,bad,ref count);
            int input=op==TX.F2xm1?16:32;
            CanonicalX87WorkLoad(input,1);CanonicalX87ControlMatch(48,1,64,bad,ref count);
            CanonicalX87WorkLoad(input+8,1,16);CanonicalX87ControlMatch(56,1,16,bad,ref count);
            CanonicalX87ControlEqual(58,0,16,bad,ref count);CanonicalX87ControlEqual(60,0,32,bad,ref count);
            CanonicalX87ControlEqual(64,0,64,bad,ref count);CanonicalX87ControlEqual(72,0,64,bad,ref count);
            int undefinedDone=IntegerJump();
            Patch(range,position);
            if(!trig){IntegerImmediate(0,1,32);IntegerRR(0x85,0,0,32);bad[count++]=NearCondition(0x85);}
            CanonicalX87ControlEqual(84,kind|0x800,32,bad,ref count);
            CanonicalX87ControlEqual(244,0,32,bad,ref count);CanonicalX87ControlEqual(216,0,32,bad,ref count);
            CanonicalX87ControlEqual(80,0,32,bad,ref count);CanonicalX87ControlEqual(232,0x04000400,32,bad,ref count);
            CanonicalX87WorkLoad(24,0,16);IntegerConstant(4,0,0x7fff);IntegerConstant(7,0,0x403e);
            bad[count++]=NearCondition(0x82);IntegerConstant(7,0,0x7fff);bad[count++]=NearCondition(0x83);
            CanonicalX87WorkLoad(16,0);IntegerRR(0x85,0,0);bad[count++]=NearCondition(0x89);
            Patch(zeroImages,position);
            CanonicalX87ControlEqual(48,0,64,bad,ref count);CanonicalX87ControlEqual(56,0,64,bad,ref count);
            CanonicalX87ControlEqual(64,0,64,bad,ref count);CanonicalX87ControlEqual(72,0,64,bad,ref count);
            int legacyDone=IntegerJump();Patch(integer,position);
            CanonicalX87ValidateTranscendentalInteger(op,bad,ref count);
            Patch(legacyDone,position);Patch(done,position);Patch(undefinedDone,position);
        }
        private void CanonicalX87TranscendentalUndefinedProof(TX op,scoped Span<int> bad,ref int count)
        {
            // NaN/unsupported in EITHER participating operand outranks this
            // profile; finite/pseudo-denormal y is allowed for undefined copy.
            int inputs=op==TX.Fyl2xp1?2:1;
            for(int n=0;n<inputs;++n)
            {
                int off=16+16*n;
                CanonicalX87WorkLoad(off+8,0,16);IntegerConstant(4,0,0x7fff);
                IntegerRR(0x85,0,0,32);int exponentZero=NearCondition(0x84);
                CanonicalX87WorkLoad(off,2);IntegerRR(0x85,2,2);bad[count++]=NearCondition(0x89);
                IntegerConstant(7,0,0x7fff);int finite=NearCondition(0x85);
                IntegerImmediate(1,0x8000000000000000UL);IntegerRR(0x39,2,1);bad[count++]=NearCondition(0x85);
                Patch(finite,position);Patch(exponentZero,position);
            }
            CanonicalX87WorkLoad(24,0,16);IntegerMov(8,0,32);IntegerConstant(4,0,0x7fff);
            uint endpoint=op==TX.F2xm1?0x3fffu:0x3ffdu;
            IntegerConstant(7,0,endpoint);bad[count++]=NearCondition(0x82);int largerExponent=NearCondition(0x87);
            if(op==TX.F2xm1)IntegerImmediate(1,0x8000000000000000UL);
            else
            {
                IntegerConstant(4,8,0x8000);int negative=NearCondition(0x85);
                IntegerImmediate(1,0xd413cccfe7799211UL);int chosen=IntegerJump();
                Patch(negative,position);IntegerImmediate(1,0x95f619980c4336f7UL);Patch(chosen,position);
            }
            CanonicalX87WorkLoad(16,2);IntegerRR(0x39,2,1);bad[count++]=NearCondition(0x86);
            Patch(largerExponent,position);
        }
        private void CanonicalX87MergeTranscendentalStatus()
        {
            // R9=Work remains valid until StoreRaw. Only defined CC bits are
            // replaced; sticky six flags and undefined C0/C3 are retained.
            CanonicalX87WorkLoad(234,1,16);IntegerUnary(2,1,32);
            CanonicalX87MachineLoad(648,0);IntegerRR(0x21,0,1,32);
            CanonicalX87WorkLoad(232,1,16);IntegerRR(0x09,0,1,32);
            CanonicalX87WorkLoad(82,2,16);IntegerConstant(4,2,63);IntegerRR(0x09,0,2,32);
            CanonicalX87MachineLoad(672,1);IntegerUnary(2,1,32);IntegerRR(0x21,1,2,32);
            IntegerConstant(4,1,63);int masked=NearCondition(0x84);IntegerConstant(1,0,0x8080);
            Patch(masked,position);CanonicalX87MachineStore(648,0);
        }
    }
}
