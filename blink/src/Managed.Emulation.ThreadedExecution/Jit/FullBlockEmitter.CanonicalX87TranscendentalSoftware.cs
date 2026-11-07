using System;
using TX = Managed.Emulation.Execution.FullCanonicalX87TranscendentalOperation;

namespace Managed.Emulation.Execution;

internal static partial class FullBlockEmitter
{
    // Emission-time fixups only. No runtime selector, allocation, callback or CALL.
    private ref struct TranscendentalSoftwareLabels
    {
        internal Span<int> Positions, Fixups;
        private Span<int> count;
        internal int Count { readonly get => count[0]; set => count[0]=value; }
        internal TranscendentalSoftwareLabels(Span<int> positions, Span<int> fixups, Span<int> count)
        { Positions=positions; Fixups=fixups; this.count=count; Count=0; positions.Fill(-1); }
    }
    private ref partial struct Writer
    {
        private void TransSoftwareJump(scoped TranscendentalSoftwareLabels g,int label,int cc=-1)
        {
            if(g.Count+2>g.Fixups.Length)throw new InvalidOperationException("Software validation fixups");
            g.Fixups[g.Count++]=cc<0?IntegerJump():NearCondition(checked((byte)cc));
            g.Fixups[g.Count++]=label;
        }
        private void TransSoftwareBind(scoped TranscendentalSoftwareLabels g,int label)
        {
            if(g.Positions[label]>=0)throw new InvalidOperationException("Software validation label");
            g.Positions[label]=position;
        }
        private void TransSoftwareResolve(scoped TranscendentalSoftwareLabels g)
        {
            for(int k=0;k<g.Count;k+=2)
            {
                int p=g.Positions[g.Fixups[k+1]];
                if(p<0)throw new InvalidOperationException("Unbound software validation label");
                Patch(g.Fixups[k],p);
            }
        }
        // Independent RAW-only classifier: zero0 finite1 infinity2 qNaN3 sNaN4
        // unsupported5 denormal/pseudo6. Returns EDX; never reads producer scratch.
        private void TransSoftwareClass(int raw)
        {
            CanonicalX87WorkLoad(raw+8,0,16);IntegerConstant(4,0,0x7fff);
            IntegerRR(0x85,0,0,32);int exponentZero=NearCondition(0x84);
            CanonicalX87WorkLoad(raw,1);IntegerRR(0x85,1,1);int unsupported=NearCondition(0x89);
            IntegerConstant(7,0,0x7fff);int special=NearCondition(0x84);
            IntegerImmediate(2,1,32);int normalDone=IntegerJump();
            Patch(special,position);IntegerImmediate(2,0x8000000000000000UL);
            IntegerRR(0x39,1,2);int infinity=NearCondition(0x84);
            IntegerImmediate(2,0x4000000000000000UL);IntegerRR(0x85,1,2);
            int quiet=NearCondition(0x85);IntegerImmediate(2,4,32);int snanDone=IntegerJump();
            Patch(quiet,position);IntegerImmediate(2,3,32);int quietDone=IntegerJump();
            Patch(infinity,position);IntegerImmediate(2,2,32);int infinityDone=IntegerJump();
            Patch(unsupported,position);IntegerImmediate(2,5,32);int unsupportedDone=IntegerJump();
            Patch(exponentZero,position);CanonicalX87WorkLoad(raw,1);IntegerRR(0x85,1,1);
            int zero=NearCondition(0x84);IntegerImmediate(2,6,32);int denormalDone=IntegerJump();
            Patch(zero,position);IntegerImmediate(2,0,32);
            Patch(normalDone,position);Patch(snanDone,position);Patch(quietDone,position);
            Patch(infinityDone,position);Patch(unsupportedDone,position);Patch(denormalDone,position);
        }
        private void TransSoftwareClassBranch(scoped TranscendentalSoftwareLabels g,int slot,int value,int label)
        {CanonicalX87WorkLoad(slot,0,32);IntegerConstant(7,0,(uint)value);TransSoftwareJump(g,label,0x84);}
        private void TransSoftwareZeroBranch(scoped TranscendentalSoftwareLabels g,int raw,int label)
        {CanonicalX87WorkLoad(raw,0);IntegerRR(0x85,0,0);TransSoftwareJump(g,label,0x84);}
        private void TransSoftwareInfinityBranch(scoped TranscendentalSoftwareLabels g,int raw,int label)
        {CanonicalX87WorkLoad(raw+8,0,16);IntegerConstant(4,0,0x7fff);IntegerConstant(7,0,0x7fff);TransSoftwareJump(g,label,0x84);}
        // Expectation uses only the pre-existing verifier charge288..319:
        // classX288 classY292 pre296 mode300 sig304 se312 spare316.
        // Mode0 literal exact,1 approximation,2 exact dyadic,4 NaN,8 pi constant.
        private void TransSoftwareExpected(ulong sig,uint se)
        {IntegerImmediate(2,sig);CanonicalX87WorkStore(304,2);CanonicalX87WorkConstant(312,se);}
        private void TransSoftwareSign(int raw)
        {CanonicalX87WorkLoad(raw+8,1,16);IntegerConstant(4,1,0x8000);CanonicalX87WorkStore(312,1,32);}
        private void TransSoftwareXorSign()
        {
            CanonicalX87WorkLoad(24,1,16);CanonicalX87WorkLoad(40,2,16);
            IntegerRR(0x31,1,2,32);IntegerConstant(4,1,0x8000);CanonicalX87WorkStore(312,1,32);
        }
        private void TransSoftwareLogSign()
        {
            TransSoftwareSign(32);CanonicalX87WorkLoad(24,0,16);IntegerConstant(7,0,0x3fff);
            int positive=NearCondition(0x83);IntegerConstant(6,1,0x8000);CanonicalX87WorkStore(312,1,32);Patch(positive,position);
        }
        private void TransSoftwareMakeInfinity()
        {
            IntegerImmediate(2,0x8000000000000000UL);CanonicalX87WorkStore(304,2);
            CanonicalX87WorkLoad(312,1,32);IntegerConstant(1,1,0x7fff);CanonicalX87WorkStore(312,1,32);
        }
        // Applies independent literal pi rounding, certified in math/status_profile.py.
        // Uses final ST1 sign for atan2, including signed axes and infinite quadrants.
        private void TransSoftwarePi(int exponent,bool threeQuarters=false)
        {
            ulong floor=threeQuarters?0x96cbe3f9990e91a7UL:0xc90fdaa22168c234UL;
            TransSoftwareExpected(floor,0);TransSoftwareSign(32);CanonicalX87WorkConstant(300,8);
            CanonicalX87WorkConstant(292,0x20);
            CanonicalX87WorkLoad(8,1,16);IntegerShift(5,1,32,10);IntegerConstant(4,1,3);
            IntegerRR(0x85,1,1,32);int nearest=NearCondition(0x84);
            IntegerConstant(7,1,3);int truncate=NearCondition(0x84);
            CanonicalX87WorkLoad(312,2,32);IntegerShift(5,2,32,15);IntegerRR(0x01,1,2,32);
            IntegerConstant(7,1,2);int noIncrement=NearCondition(0x85);
            Patch(nearest,position);CanonicalX87WorkLoad(304,1);IntegerConstant(0,1,1,64);CanonicalX87WorkStore(304,1);
            CanonicalX87WorkConstant(292,0x220);
            Patch(truncate,position);Patch(noIncrement,position);
            CanonicalX87WorkLoad(312,1,32);IntegerConstant(1,1,(uint)exponent);CanonicalX87WorkStore(312,1,32);
        }
        // R165's independent 32x32 dyadic/rounding proof remains EXACTLY intact
        // on its covered cells. Ordinary cells outside it enter the general path.
        private int TransSoftwareRetainedAnchor(TX op,scoped Span<int> bad,ref int count)
        {
            Span<int> skip=stackalloc int[16];int n=0;
            if(op==TX.Fyl2x)
            {
                CanonicalX87WorkLoad(24,0,16);IntegerConstant(7,0,1);skip[n++]=NearCondition(0x82);
                IntegerConstant(7,0,0x7fff);skip[n++]=NearCondition(0x83);
                CanonicalX87WorkLoad(16,0);IntegerImmediate(1,0x8000000000000000UL);IntegerRR(0x39,0,1);skip[n++]=NearCondition(0x85);
                CanonicalX87WorkLoad(40,0,16);IntegerConstant(4,0,0x7fff);
                IntegerConstant(7,0,383);skip[n++]=NearCondition(0x82);
                IntegerConstant(7,0,32383);skip[n++]=NearCondition(0x87);
                CanonicalX87WorkLoad(32,0);IntegerRR(0x85,0,0);skip[n++]=NearCondition(0x89);
            }
            else if(op is TX.Fpatan or TX.Fyl2xp1)return -1;
            else
            {
                CanonicalX87WorkLoad(24,0,16);IntegerConstant(4,0,0x7fff);
                if(op==TX.F2xm1)
                {
                    IntegerRR(0x85,0,0,32);int zero=NearCondition(0x84);
                    IntegerConstant(7,0,0x3fff);skip[n++]=NearCondition(0x85);
                    CanonicalX87WorkLoad(16,0);IntegerImmediate(1,0x8000000000000000UL);
                    IntegerRR(0x39,0,1);skip[n++]=NearCondition(0x85);int selected=IntegerJump();
                    Patch(zero,position);CanonicalX87WorkLoad(16,0);IntegerRR(0x85,0,0);skip[n++]=NearCondition(0x85);Patch(selected,position);
                }
                else
                {IntegerRR(0x85,0,0,32);skip[n++]=NearCondition(0x85);CanonicalX87WorkLoad(16,0);IntegerRR(0x85,0,0);skip[n++]=NearCondition(0x85);}
            }
            CanonicalX87ValidateTranscendentalInteger(op,bad,ref count);int done=IntegerJump();
            for(int k=0;k<n;++k)Patch(skip[k],position);return done;
        }
        private void CanonicalX87ValidateTranscendentalSoftware(TX op,scoped Span<int> bad,ref int count)
        {
            bool binary=FullCanonicalX87TranscendentalLowering.IsBinary(op),pair=FullCanonicalX87TranscendentalLowering.Pushes(op);
            bool trig=op is TX.Fptan or TX.Fsincos or TX.Fsin or TX.Fcos;
            uint kind=(uint)op-0xa000;
            CanonicalX87IntegerInputMatch(0,16,bad,ref count);
            if(binary)CanonicalX87IntegerInputMatch(1,32,bad,ref count);
            // Inputs and outputs are raw80 in16B slots, never unchecked media.
            CanonicalX87ControlEqual(26,0,16,bad,ref count);CanonicalX87ControlEqual(28,0,32,bad,ref count);
            CanonicalX87ControlEqual(42,0,16,bad,ref count);CanonicalX87ControlEqual(44,0,32,bad,ref count);
            if(!binary){CanonicalX87ControlEqual(32,0,64,bad,ref count);CanonicalX87ControlEqual(40,0,64,bad,ref count);}
            for(int p=96;p<232;p+=8)CanonicalX87ControlEqual(p,0,64,bad,ref count);
            for(int p=236;p<256;p+=4)CanonicalX87ControlEqual(p,0,32,bad,ref count);
            // The bridge clears88 after returning; no producer pointer survives.
            CanonicalX87ControlEqual(88,0,64,bad,ref count);
            int anchor=TransSoftwareRetainedAnchor(op,bad,ref count);
            Span<int> positions=stackalloc int[64],fixups=stackalloc int[1024],labelCount=stackalloc int[1];
            var g=new TranscendentalSoftwareLabels(positions,fixups,labelCount);
            const int Invalid=0,SNaN=1,QNaN=2,NanSelect=3,UseX=4,UseY=5,Chosen=6,Check=7,
                General=8,Undefined=9,Range=10,Suppressed=11,Complete=12,End=13,
                ZeroX=14,ZeroY=15,InfX=16,InfY=17,ZeroResult=18,InfResult=19,
                Pi=20,HalfPi=21,QuarterPi=22,ThreeQuarterPi=23,ExactEndpoint=24,
                NanXQuiet=25,NanSame=26,NanResult=27;
            for(int p=288;p<320;p+=4)CanonicalX87WorkConstant(p,0);
            TransSoftwareClass(16);CanonicalX87WorkStore(288,2,32);
            if(binary){TransSoftwareClass(32);CanonicalX87WorkStore(292,2,32);}
            TransSoftwareClassBranch(g,288,5,Invalid);if(binary)TransSoftwareClassBranch(g,292,5,Invalid);
            TransSoftwareClassBranch(g,288,4,SNaN);if(binary)TransSoftwareClassBranch(g,292,4,SNaN);
            TransSoftwareClassBranch(g,288,3,QNaN);if(binary)TransSoftwareClassBranch(g,292,3,QNaN);
            // DE is independently classified from original raw, including pseudo.
            CanonicalX87WorkLoad(288,0,32);IntegerConstant(7,0,6);int noDE0=NearCondition(0x85);
            CanonicalX87WorkConstant(296,2);Patch(noDE0,position);
            if(binary){CanonicalX87WorkLoad(292,0,32);IntegerConstant(7,0,6);int noDE1=NearCondition(0x85);CanonicalX87WorkConstant(296,2);Patch(noDE1,position);}
            if(trig)
            {
                TransSoftwareInfinityBranch(g,16,Invalid);
                CanonicalX87WorkLoad(24,0,16);IntegerConstant(4,0,0x7fff);IntegerConstant(7,0,0x403e);TransSoftwareJump(g,Range,0x83);
                TransSoftwareZeroBranch(g,16,ZeroX);TransSoftwareJump(g,General);
                TransSoftwareBind(g,ZeroX);
                if(op==TX.Fcos)TransSoftwareExpected(0x8000000000000000UL,0x3fff);else TransSoftwareSign(16);
                TransSoftwareJump(g,Check);
            }
            else if(op==TX.F2xm1)
            {
                TransSoftwareInfinityBranch(g,16,Undefined);
                CanonicalX87WorkLoad(24,0,16);IntegerConstant(4,0,0x7fff);IntegerConstant(7,0,0x3fff);
                TransSoftwareJump(g,Undefined,0x87);int smaller=NearCondition(0x82);
                CanonicalX87WorkLoad(16,0);IntegerImmediate(1,0x8000000000000000UL);IntegerRR(0x39,0,1);
                TransSoftwareJump(g,Undefined,0x87);TransSoftwareJump(g,ExactEndpoint);
                Patch(smaller,position);TransSoftwareZeroBranch(g,16,ZeroX);TransSoftwareJump(g,General);
                TransSoftwareBind(g,ExactEndpoint);TransSoftwareExpected(0x8000000000000000UL,0x3fff);
                CanonicalX87WorkLoad(24,0,16);IntegerConstant(4,0,0x8000);int positive=NearCondition(0x84);
                CanonicalX87WorkConstant(312,0xbffe);Patch(positive,position);TransSoftwareJump(g,Check);
                TransSoftwareBind(g,ZeroX);TransSoftwareSign(16);TransSoftwareJump(g,Check);
            }
            else if(op==TX.Fyl2xp1)
            {
                TransSoftwareInfinityBranch(g,16,Undefined);
                CanonicalX87WorkLoad(24,0,16);IntegerConstant(4,0,0x7fff);IntegerConstant(7,0,0x3ffd);
                TransSoftwareJump(g,Undefined,0x87);int small=NearCondition(0x82);
                CanonicalX87WorkLoad(24,0,16);IntegerConstant(4,0,0x8000);int negative=NearCondition(0x85);
                IntegerImmediate(1,0xd413cccfe7799211UL);int boundary=IntegerJump();
                Patch(negative,position);IntegerImmediate(1,0x95f619980c4336f7UL);Patch(boundary,position);
                CanonicalX87WorkLoad(16,0);IntegerRR(0x39,0,1);TransSoftwareJump(g,Undefined,0x87);Patch(small,position);
                TransSoftwareXorSign();TransSoftwareZeroBranch(g,16,ZeroX);
                TransSoftwareInfinityBranch(g,32,InfResult);TransSoftwareZeroBranch(g,32,Check);TransSoftwareJump(g,General);
                TransSoftwareBind(g,ZeroX);TransSoftwareInfinityBranch(g,32,Invalid);TransSoftwareJump(g,Check);
                TransSoftwareBind(g,InfResult);TransSoftwareMakeInfinity();TransSoftwareJump(g,Check);
            }
            else if(op==TX.Fyl2x)
            {
                TransSoftwareZeroBranch(g,16,ZeroX);
                CanonicalX87WorkLoad(24,0,16);IntegerConstant(4,0,0x8000);TransSoftwareJump(g,Invalid,0x85);
                TransSoftwareInfinityBranch(g,16,InfX);TransSoftwareLogSign();
                TransSoftwareInfinityBranch(g,32,InfY);TransSoftwareZeroBranch(g,32,Check);
                CanonicalX87WorkLoad(24,0,16);IntegerConstant(7,0,0x3fff);int notOne=NearCondition(0x85);
                CanonicalX87WorkLoad(16,0);IntegerImmediate(1,0x8000000000000000UL);IntegerRR(0x39,0,1);
                TransSoftwareJump(g,Check,0x84);Patch(notOne,position);
                // Any positive finite power of two, including raw subnormals,
                // is exact k*y arithmetic: PE is not forced on that mode.
                CanonicalX87WorkLoad(16,1);IntegerMov(2,1);IntegerConstant(5,2,1,64);IntegerRR(0x21,1,2);
                int notDyadic=NearCondition(0x85);CanonicalX87WorkConstant(300,2);TransSoftwareJump(g,Check);
                Patch(notDyadic,position);TransSoftwareJump(g,General);
                TransSoftwareBind(g,ZeroX);TransSoftwareZeroBranch(g,32,Invalid);
                TransSoftwareSign(32);IntegerConstant(6,1,0x8000);CanonicalX87WorkStore(312,1,32);
                TransSoftwareMakeInfinity();CanonicalX87WorkLoad(296,1,32);IntegerConstant(1,1,4);CanonicalX87WorkStore(296,1,32);TransSoftwareJump(g,Check);
                TransSoftwareBind(g,InfX);TransSoftwareZeroBranch(g,32,Invalid);TransSoftwareSign(32);TransSoftwareMakeInfinity();TransSoftwareJump(g,Check);
                TransSoftwareBind(g,InfY);
                CanonicalX87WorkLoad(24,0,16);IntegerConstant(7,0,0x3fff);int nonUnitE=NearCondition(0x85);
                CanonicalX87WorkLoad(16,0);IntegerImmediate(1,0x8000000000000000UL);IntegerRR(0x39,0,1);TransSoftwareJump(g,Invalid,0x84);
                Patch(nonUnitE,position);TransSoftwareMakeInfinity();TransSoftwareJump(g,Check);
            }
            else // FPATAN: independently resolve signed axes/infinite quadrants.
            {
                TransSoftwareSign(32);TransSoftwareZeroBranch(g,32,ZeroY);TransSoftwareZeroBranch(g,16,HalfPi);
                TransSoftwareInfinityBranch(g,16,InfX);TransSoftwareInfinityBranch(g,32,HalfPi);TransSoftwareJump(g,General);
                TransSoftwareBind(g,ZeroY);CanonicalX87WorkLoad(24,0,16);IntegerConstant(4,0,0x8000);TransSoftwareJump(g,Pi,0x85);TransSoftwareJump(g,Check);
                TransSoftwareBind(g,InfX);TransSoftwareInfinityBranch(g,32,InfY);
                CanonicalX87WorkLoad(24,0,16);IntegerConstant(4,0,0x8000);TransSoftwareJump(g,Pi,0x85);TransSoftwareJump(g,Check);
                TransSoftwareBind(g,InfY);CanonicalX87WorkLoad(24,0,16);IntegerConstant(4,0,0x8000);
                TransSoftwareJump(g,ThreeQuarterPi,0x85);TransSoftwareJump(g,QuarterPi);
                TransSoftwareBind(g,Pi);TransSoftwarePi(0x4000);TransSoftwareJump(g,Check);
                TransSoftwareBind(g,HalfPi);TransSoftwarePi(0x3fff);TransSoftwareJump(g,Check);
                TransSoftwareBind(g,QuarterPi);TransSoftwarePi(0x3ffe);TransSoftwareJump(g,Check);
                TransSoftwareBind(g,ThreeQuarterPi);TransSoftwarePi(0x4000,true);TransSoftwareJump(g,Check);
            }
            TransSoftwareBind(g,General);if(op==TX.F2xm1)TransSoftwareSign(16);CanonicalX87WorkConstant(300,1);TransSoftwareJump(g,Check);
            TransSoftwareBind(g,Invalid);CanonicalX87WorkConstant(296,1);CanonicalX87WorkConstant(300,4);
            TransSoftwareExpected(0xc000000000000000UL,0xffff);TransSoftwareJump(g,Check);
            TransSoftwareBind(g,SNaN);CanonicalX87WorkConstant(296,1);TransSoftwareJump(g,NanSelect);
            TransSoftwareBind(g,QNaN);CanonicalX87WorkConstant(296,0);
            TransSoftwareBind(g,NanSelect);CanonicalX87WorkConstant(300,4);
            if(binary)
            {
                TransSoftwareClassBranch(g,288,3,NanXQuiet);TransSoftwareClassBranch(g,292,3,UseY);
                TransSoftwareClassBranch(g,288,4,NanSame);TransSoftwareJump(g,UseY);
                TransSoftwareBind(g,NanSame);TransSoftwareClassBranch(g,292,4,NanResult);TransSoftwareJump(g,UseX);
                TransSoftwareBind(g,NanXQuiet);TransSoftwareClassBranch(g,292,3,NanResult);TransSoftwareJump(g,UseX);
                TransSoftwareBind(g,NanResult);
                CanonicalX87WorkLoad(16,1);CanonicalX87WorkLoad(32,2);IntegerRR(0x39,1,2);TransSoftwareJump(g,UseY,0x82);
            }
            TransSoftwareBind(g,UseX);CanonicalX87WorkLoad(16,1);CanonicalX87WorkLoad(24,2,16);TransSoftwareJump(g,Chosen);
            TransSoftwareBind(g,UseY);CanonicalX87WorkLoad(32,1);CanonicalX87WorkLoad(40,2,16);
            TransSoftwareBind(g,Chosen);IntegerImmediate(8,0xc000000000000000UL);IntegerRR(0x09,1,8);
            CanonicalX87WorkStore(304,1);CanonicalX87WorkStore(312,2,32);TransSoftwareJump(g,Check);
            TransSoftwareBind(g,Undefined);
            if(op is TX.F2xm1 or TX.Fyl2xp1)
            {
                int input=op==TX.F2xm1?16:32;
                CanonicalX87ControlEqual(84,kind|0x8100,32,bad,ref count);CanonicalX87ControlEqual(80,0,32,bad,ref count);
                CanonicalX87ControlEqual(232,0x02000000,32,bad,ref count);
                CanonicalX87WorkLoad(input,1);CanonicalX87ControlMatch(48,1,64,bad,ref count);
                CanonicalX87WorkLoad(input+8,1,16);CanonicalX87ControlMatch(56,1,16,bad,ref count);
                TransSoftwareOutputPadding(false,bad,ref count);TransSoftwareJump(g,End);
            }
            TransSoftwareBind(g,Range);
            if(trig)
            {
                CanonicalX87ControlEqual(84,kind|0x800,32,bad,ref count);CanonicalX87ControlEqual(80,0,32,bad,ref count);
                CanonicalX87ControlEqual(232,0x04000400,32,bad,ref count);TransSoftwareZeroImages(bad,ref count);TransSoftwareJump(g,End);
            }
            TransSoftwareBind(g,Check);
            TransSoftwareDiagnostic(op,bad,ref count);
            CanonicalX87WorkLoad(296,2,32);CanonicalX87WorkLoad(8,1,16);IntegerUnary(2,1,32);
            IntegerRR(0x21,1,2,32);IntegerConstant(4,1,7);TransSoftwareJump(g,Complete,0x84);
            // Select lowest unmasked pre-event; no architectural state has changed.
            IntegerMov(2,1,32);IntegerUnary(3,2,32);IntegerRR(0x21,2,1,32);CanonicalX87ControlMatch(82,2,16,bad,ref count);
            CanonicalX87ControlEqual(84,kind|0x10400,32,bad,ref count);CanonicalX87ControlEqual(232,0x02000000,32,bad,ref count);
            TransSoftwareZeroImages(bad,ref count);TransSoftwareJump(g,End);
            TransSoftwareBind(g,Complete);
            TransSoftwareCompleted(op,bad,ref count);
            TransSoftwareBind(g,End);
            for(int p=288;p<320;p+=4)CanonicalX87WorkConstant(p,0);
            TransSoftwareResolve(g);if(anchor>=0)Patch(anchor,position);
        }
        private void TransSoftwareZeroImages(scoped Span<int> bad,ref int count)
        {for(int p=48;p<80;p+=8)CanonicalX87ControlEqual(p,0,64,bad,ref count);}
        private void TransSoftwareOutputPadding(bool pair,scoped Span<int> bad,ref int count)
        {
            CanonicalX87ControlEqual(58,0,16,bad,ref count);CanonicalX87ControlEqual(60,0,32,bad,ref count);
            if(pair){CanonicalX87ControlEqual(74,0,16,bad,ref count);CanonicalX87ControlEqual(76,0,32,bad,ref count);}
            else{CanonicalX87ControlEqual(64,0,64,bad,ref count);CanonicalX87ControlEqual(72,0,64,bad,ref count);}
        }
        private void TransSoftwareDiagnostic(TX op,scoped Span<int> bad,ref int count)
        {
            CanonicalX87ControlReserved(80,0x023f023fu,bad,ref count);
            // Low pre-event bits must be EXACT independently classified values.
            CanonicalX87WorkLoad(80,1,16);IntegerConstant(4,1,7);
            CanonicalX87WorkLoad(296,2,32);IntegerRR(0x39,1,2,32);bad[count++]=NearCondition(0x85);
            CanonicalX87WorkLoad(300,0,32);IntegerConstant(4,0,3);int numeric=NearCondition(0x85);
            // Literal and selected NaN outputs have no post-event bits. Pi axes
            // have an independent literal PE/C1 decision in292.
            CanonicalX87WorkLoad(300,0,32);IntegerConstant(7,0,8);int literal=NearCondition(0x85);
            CanonicalX87WorkLoad(292,1,32);IntegerRR(0x09,2,1,32);Patch(literal,position);
            CanonicalX87ControlMatch(80,2,16,bad,ref count);int done=IntegerJump();
            Patch(numeric,position);
            CanonicalX87WorkLoad(80,1,16);IntegerConstant(4,1,0x18);IntegerConstant(7,1,0x18);bad[count++]=NearCondition(0x84);
            // Diagnostic masked range necessarily implies PE. C1 requires PE.
            TransSoftwareStatusImplication(80,0x218,32,bad,ref count);
            CanonicalX87WorkLoad(300,0,32);IntegerConstant(7,0,1);int dyadic=NearCondition(0x85);
            CanonicalX87WorkLoad(80,0,16);IntegerConstant(4,0,32);bad[count++]=NearCondition(0x84);Patch(dyadic,position);
            Patch(done,position);
        }
        private void TransSoftwareStatusImplication(int offset,uint trigger,uint required,scoped Span<int> bad,ref int count)
        {
            CanonicalX87WorkLoad(offset,0,16);IntegerConstant(4,0,trigger);int absent=NearCondition(0x84);
            CanonicalX87WorkLoad(offset,0,16);IntegerConstant(4,0,required);IntegerConstant(7,0,required);bad[count++]=NearCondition(0x85);Patch(absent,position);
        }
        private void TransSoftwareCompleted(TX op,scoped Span<int> bad,ref int count)
        {
            bool pair=FullCanonicalX87TranscendentalLowering.Pushes(op),trig=op is TX.Fptan or TX.Fsincos or TX.Fsin or TX.Fcos;
            TransSoftwareOutputPadding(pair,bad,ref count);
            // Completed status has classified pre-events plus independently
            // constrained post-events; arbitrary IE/DE/ZE cannot be laundered.
            CanonicalX87WorkLoad(82,1,16);IntegerConstant(4,1,7);CanonicalX87WorkLoad(296,2,32);
            IntegerRR(0x39,1,2,32);bad[count++]=NearCondition(0x85);
            CanonicalX87WorkLoad(300,0,32);IntegerConstant(4,0,3);int numeric=NearCondition(0x85);
            CanonicalX87WorkLoad(80,1,16);CanonicalX87ControlMatch(82,1,16,bad,ref count);
            CanonicalX87WorkLoad(304,1);CanonicalX87ControlMatch(48,1,64,bad,ref count);
            CanonicalX87WorkLoad(312,1,16);CanonicalX87ControlMatch(56,1,16,bad,ref count);int valueDone=IntegerJump();
            Patch(numeric,position);TransSoftwareNumericStatus(op,bad,ref count);TransSoftwareNumericValue(op,bad,ref count);
            Patch(valueDone,position);
            if(pair)
            {
                if(op==TX.Fsincos)
                {
                    CanonicalX87WorkLoad(300,0,32);IntegerConstant(7,0,4);int ordinary=NearCondition(0x85);
                    CanonicalX87WorkLoad(304,1);CanonicalX87ControlMatch(64,1,64,bad,ref count);
                    CanonicalX87WorkLoad(312,1,16);CanonicalX87ControlMatch(72,1,16,bad,ref count);int pairDone=IntegerJump();
                    Patch(ordinary,position);CanonicalX87WorkLoad(300,0,32);IntegerRR(0x85,0,0,32);int computed=NearCondition(0x85);
                    TransSoftwareOne(64,bad,ref count);int exactDone=IntegerJump();
                    Patch(computed,position);TransSoftwareBounded(64,0x3fff,0x8000000000000000UL,bad,ref count);
                    Patch(exactDone,position);Patch(pairDone,position);
                }
                else TransSoftwareOne(64,bad,ref count);
            }
            CanonicalX87WorkLoad(82,2,16);IntegerMov(1,2,32);IntegerConstant(4,1,0x200);
            CanonicalX87ControlMatch(232,1,16,bad,ref count);CanonicalX87ControlEqual(234,trig?0x600u:0x200u,16,bad,ref count);
            IntegerImmediate(8,(uint)op-0xa000|0x10100|(pair?0x200u:0),32);
            IntegerConstant(4,2,32);int noPE=NearCondition(0x84);CanonicalX87WorkLoad(8,0,16);IntegerConstant(4,0,32);int masked=NearCondition(0x85);
            IntegerConstant(1,8,0x1000);Patch(noPE,position);Patch(masked,position);CanonicalX87ControlMatch(84,8,32,bad,ref count);
        }
        private void TransSoftwareOne(int output,scoped Span<int> bad,ref int count)
        {IntegerImmediate(1,0x8000000000000000UL);CanonicalX87ControlMatch(output,1,64,bad,ref count);CanonicalX87ControlEqual(output+8,0x3fff,16,bad,ref count);}
        private void TransSoftwareNumericStatus(TX op,scoped Span<int> bad,ref int count)
        {
            TransSoftwareStatusImplication(82,0x200,32,bad,ref count);
            CanonicalX87WorkLoad(82,1,16);IntegerConstant(4,1,0x18);IntegerConstant(7,1,0x18);bad[count++]=NearCondition(0x84);
            CanonicalX87WorkLoad(300,0,32);IntegerConstant(7,0,1);int dyadic=NearCondition(0x85);
            CanonicalX87WorkLoad(82,0,16);IntegerConstant(4,0,32);bad[count++]=NearCondition(0x84);Patch(dyadic,position);
            // Fully masked numeric post-events coincide EXACTLY with diagnostic.
            CanonicalX87WorkLoad(8,0,16);IntegerConstant(4,0,24);IntegerConstant(7,0,24);int ranged=NearCondition(0x85);
            CanonicalX87WorkLoad(80,1,16);CanonicalX87ControlMatch(82,1,16,bad,ref count);int checkedMasks=IntegerJump();
            Patch(ranged,position);
            // Overflow detection before range adjustment is mask-independent.
            CanonicalX87WorkLoad(80,1,16);CanonicalX87WorkLoad(82,2,16);IntegerRR(0x31,1,2,32);IntegerConstant(4,1,8);bad[count++]=NearCondition(0x85);
            // A masked underflow in actual cannot disappear from diagnostic.
            CanonicalX87WorkLoad(8,0,16);IntegerConstant(4,0,16);int unmaskedUE=NearCondition(0x84);
            CanonicalX87WorkLoad(80,1,16);CanonicalX87WorkLoad(82,2,16);IntegerRR(0x31,1,2,32);IntegerConstant(4,1,16);bad[count++]=NearCondition(0x85);
            Patch(unmaskedUE,position);Patch(checkedMasks,position);
            if(op is not (TX.Fyl2x or TX.Fyl2xp1))
            {CanonicalX87WorkLoad(82,0,16);IntegerConstant(4,0,8);bad[count++]=NearCondition(0x85);}
            // A diagnostic tiny/inexact event also implies actual tininess.
            CanonicalX87WorkLoad(80,0,16);IntegerConstant(4,0,16);int noDiagnosticUE=NearCondition(0x84);
            CanonicalX87WorkLoad(82,0,16);IntegerConstant(4,0,16);bad[count++]=NearCondition(0x84);Patch(noDiagnosticUE,position);
            // If actual has no range event, masks cannot have changed rounding.
            CanonicalX87WorkLoad(82,0,16);IntegerConstant(4,0,24);int actualRange=NearCondition(0x85);
            CanonicalX87WorkLoad(80,1,16);CanonicalX87ControlMatch(82,1,16,bad,ref count);Patch(actualRange,position);
            // All other numerical event/C1 boundary decisions are certified
            // against the immutable numerical entry, not guessed from raw80.
        }
        private void TransSoftwareBounded(int output,uint exponent,ulong sig,scoped Span<int> bad,ref int count)
        {
            TransSoftwareCanonicalValue(output,bad,ref count);
            CanonicalX87WorkLoad(output+8,0,16);IntegerConstant(4,0,0x7fff);IntegerConstant(7,0,exponent);
            bad[count++]=NearCondition(0x87);int lower=NearCondition(0x82);
            CanonicalX87WorkLoad(output,0);IntegerImmediate(1,sig);IntegerRR(0x39,0,1);bad[count++]=NearCondition(0x87);Patch(lower,position);
        }
        private void TransSoftwareCanonicalValue(int output,scoped Span<int> bad,ref int count)
        {
            CanonicalX87ControlValue(output,bad,ref count);
            CanonicalX87WorkLoad(output+8,0,16);IntegerConstant(4,0,0x7fff);int nonzeroExponent=NearCondition(0x85);
            CanonicalX87WorkLoad(output,0);IntegerRR(0x85,0,0);bad[count++]=NearCondition(0x88); // output pseudo-denormal forbidden
            Patch(nonzeroExponent,position);
        }
        private void TransSoftwareNumericValue(TX op,scoped Span<int> bad,ref int count)
        {
            TransSoftwareCanonicalValue(48,bad,ref count);
            if(op==TX.F2xm1)
            {
                CanonicalX87WorkLoad(82,0,16);IntegerConstant(4,0,16);int biased=NearCondition(0x85);
                CanonicalX87WorkLoad(24,0,16);IntegerConstant(4,0,0x8000);int positive=NearCondition(0x84);
                TransSoftwareBounded(48,0x3ffe,0x8000000000000000UL,bad,ref count);int bounded=IntegerJump();
                Patch(positive,position);TransSoftwareBounded(48,0x3fff,0x8000000000000000UL,bad,ref count);
                Patch(bounded,position);Patch(biased,position);
            }
            if(op is TX.Fsin or TX.Fcos or TX.Fsincos)
            {
                // Unmasked UE carries a biased exponent; its value is checked
                // by the range certificate, not by a bound in the biased scale.
                CanonicalX87WorkLoad(82,0,16);IntegerConstant(4,0,16);int tiny=NearCondition(0x85);
                TransSoftwareBounded(48,0x3fff,0x8000000000000000UL,bad,ref count);Patch(tiny,position);
            }
            if(op==TX.Fpatan)
            {
                CanonicalX87WorkLoad(82,0,16);IntegerConstant(4,0,16);int tiny=NearCondition(0x85);
                TransSoftwareBounded(48,0x4000,0xc90fdaa22168c235UL,bad,ref count);Patch(tiny,position);
            }
            if(op is TX.F2xm1 or TX.Fyl2x or TX.Fyl2xp1 or TX.Fpatan)
            {
                CanonicalX87WorkLoad(56,1,16);IntegerConstant(4,1,0x8000);CanonicalX87WorkLoad(312,2,16);IntegerConstant(4,2,0x8000);
                IntegerRR(0x39,1,2,32);bad[count++]=NearCondition(0x85);
            }
            CanonicalX87WorkLoad(82,0,16);IntegerConstant(4,0,16);int noUE=NearCondition(0x84);
            CanonicalX87WorkLoad(8,0,16);IntegerConstant(4,0,16);int biasedTiny=NearCondition(0x84);
            TransSoftwareStatusImplication(82,16,32,bad,ref count);
            CanonicalX87WorkLoad(56,0,16);IntegerConstant(4,0,0x7fff);IntegerConstant(7,0,1);bad[count++]=NearCondition(0x87);
            int subnormal=NearCondition(0x82);IntegerImmediate(1,0x8000000000000000UL);CanonicalX87ControlMatch(48,1,64,bad,ref count);
            Patch(subnormal,position);int tinyDone=IntegerJump();
            Patch(biasedTiny,position);CanonicalX87WorkLoad(56,0,16);IntegerConstant(4,0,0x7fff);
            // Entire finite family minimum target is above2^-32912; biased
            // underflow exponents therefore lie in this conservative range.
            IntegerConstant(7,0,0x1f60);bad[count++]=NearCondition(0x82);IntegerConstant(7,0,0x6000);bad[count++]=NearCondition(0x87);
            CanonicalX87WorkLoad(48,0);IntegerRR(0x85,0,0);bad[count++]=NearCondition(0x89);
            Patch(tinyDone,position);Patch(noUE,position);
            // NaN and infinity only come from independently handled specials,
            // except masked overflow, whose saturation is checked below.
            CanonicalX87WorkLoad(56,0,16);IntegerConstant(4,0,0x7fff);IntegerConstant(7,0,0x7fff);int finite=NearCondition(0x85);
            CanonicalX87WorkLoad(82,0,16);IntegerConstant(4,0,8);bad[count++]=NearCondition(0x84);
            IntegerImmediate(1,0x8000000000000000UL);CanonicalX87ControlMatch(48,1,64,bad,ref count);Patch(finite,position);
            CanonicalX87WorkLoad(82,0,16);IntegerConstant(4,0,8);int noOE=NearCondition(0x84);
            CanonicalX87WorkLoad(8,0,16);IntegerConstant(4,0,8);int wrapped=NearCondition(0x84);
            // Masked OE: literal saturation chosen from RC and sign, including C1.
            CanonicalX87WorkLoad(8,1,16);IntegerShift(5,1,32,10);IntegerConstant(4,1,3);IntegerRR(0x85,1,1,32);int infinity=NearCondition(0x84);
            IntegerConstant(7,1,3);int maximum=NearCondition(0x84);CanonicalX87WorkLoad(56,2,16);IntegerShift(5,2,32,15);
            IntegerRR(0x01,1,2,32);IntegerConstant(7,1,2);int directedInf=NearCondition(0x84);
            Patch(maximum,position);IntegerImmediate(1,ulong.MaxValue);CanonicalX87ControlMatch(48,1,64,bad,ref count);
            CanonicalX87WorkLoad(56,0,16);IntegerConstant(4,0,0x7fff);IntegerConstant(7,0,0x7ffe);bad[count++]=NearCondition(0x85);
            CanonicalX87WorkLoad(82,0,16);IntegerConstant(4,0,0x228);IntegerConstant(7,0,0x28);bad[count++]=NearCondition(0x85);int saturated=IntegerJump();
            Patch(infinity,position);Patch(directedInf,position);IntegerImmediate(1,0x8000000000000000UL);CanonicalX87ControlMatch(48,1,64,bad,ref count);
            CanonicalX87WorkLoad(56,0,16);IntegerConstant(4,0,0x7fff);IntegerConstant(7,0,0x7fff);bad[count++]=NearCondition(0x85);
            CanonicalX87WorkLoad(82,0,16);IntegerConstant(4,0,0x228);IntegerConstant(7,0,0x228);bad[count++]=NearCondition(0x85);int saturationDone=IntegerJump();
            Patch(wrapped,position);
            CanonicalX87WorkLoad(56,0,16);IntegerConstant(4,0,0x7fff);IntegerConstant(7,0,0x1fff);bad[count++]=NearCondition(0x82);
            IntegerConstant(7,0,0x200e);bad[count++]=NearCondition(0x87);
            Patch(saturated,position);Patch(saturationDone,position);Patch(noOE,position);
        }
    }
}
